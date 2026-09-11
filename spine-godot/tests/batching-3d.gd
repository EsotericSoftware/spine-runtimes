extends "rendering-3d.gd"

# GPU ordering proofs and repeatable warm-update measurements. The inherited
# fixture/readback helpers are shared with the protected 3D feature regression.
func _run():
	if DisplayServer.get_name() == "headless":
		push_error("Batching tests require a GPU renderer.")
		quit(1)
		return
	fixture_dir = "user://spine-godot-batching-3d-%d" % OS.get_process_id()
	_create_fixture()
	_create_viewport()
	await _test_slot_insertion()
	await _test_batch_counts_and_capacity()
	for name in ["many.spine-json", "other.png", "dual.atlas"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir.path_join(name)))
	_finish()

func _solid_material(color: Color) -> ShaderMaterial:
	var shader := Shader.new()
	shader.code = """shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_never, blend_mix;
uniform vec4 tint;
void fragment() { ALBEDO = tint.rgb; ALPHA = tint.a; }
"""
	var material := ShaderMaterial.new()
	material.shader = shader
	material.set_shader_parameter("tint", Vector4(color.r, color.g, color.b, color.a))
	return material

func _quad(material: Material) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	var mesh := QuadMesh.new()
	mesh.size = Vector2(48, 16)
	mesh.material = material
	node.mesh = mesh
	return node

func _reference(colors: Array[Color]) -> Color:
	sprite.visible = false
	var reference_nodes: Array[MeshInstance3D] = []
	for i in colors.size():
		var node := _quad(_solid_material(colors[i]))
		node.sorting_use_aabb_center = false
		node.sorting_offset = float(i + 1)
		viewport.add_child(node)
		reference_nodes.append(node)
	var result := (await _render_image(false)).get_pixel(GRAY_X, SAMPLE_Y)
	for node in reference_nodes:
		node.free()
	sprite.visible = true
	return result

func _test_slot_insertion():
	_reset_fixture_state()
	var red := Color(1, 0, 0, 0.5)
	var green := Color(0, 1, 0, 0.5)
	var blue := Color(0, 0, 1, 0.5)
	var yellow := Color(1, 1, 0, 0.5)
	var red_material := _solid_material(red)
	sprite.normal_material = red_material
	sprite.get_skeleton().set_attachment("spare", "region")
	var insertion := SpineSlotNode3D.new()
	insertion.slot_name = "slot"
	sprite.add_child(insertion)
	var last_slot := SpineSlotNode3D.new()
	last_slot.slot_name = "spare"
	last_slot.normal_material = _solid_material(blue)
	sprite.add_child(last_slot)
	var inserted := _quad(_solid_material(green))
	inserted.position = Vector3(0, 0, -5)
	inserted.sorting_offset = 4.25
	inserted.sorting_use_aabb_center = true
	insertion.add_child(inserted)

	for projection in [Camera3D.PROJECTION_ORTHOGONAL, Camera3D.PROJECTION_PERSPECTIVE]:
		camera.projection = projection
		for back in [false, true]:
			camera.position.z = -10 if back else 10
			camera.rotation.y = PI if back else 0
			var expected := await _reference([red, green, blue])
			var actual := (await _render_image()).get_pixel(GRAY_X, SAMPLE_Y)
			_assert_close("insertion between batches, projection=%d back=%s" % [projection, back], expected, actual)
			_check(sprite.get_render_statistics().batches == 2, "material and insertion boundaries form two Spine batches")
			_check(not inserted.sorting_use_aabb_center, "inserted geometry uses explicit origin sorting")
			_check(insertion.get_sorting_warnings().is_empty(), "matching inserted material priorities satisfy the contract")

	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.position.z = 10
	camera.rotation.y = 0
	var second := _quad(_solid_material(yellow))
	second.position = Vector3(8, 0, 3)
	insertion.add_child(second)
	var expected := await _reference([red, green, yellow, blue])
	_assert_close("separate children receive explicit preorder keys despite different origins", expected, (await _render_image()).get_pixel(GRAY_X, SAMPLE_Y))

	var material_builds: int = sprite.get_render_statistics().material_builds
	red_material.set_shader_parameter("tint", Vector4(1, 1, 0, 0.5))
	sprite.normal_material = red_material
	expected = await _reference([yellow, green, yellow, blue])
	_assert_close("reassigned uniforms update cached GPU material", expected, (await _render_image()).get_pixel(GRAY_X, SAMPLE_Y))
	_check(sprite.get_render_statistics().material_builds == material_builds, "uniform refresh does not duplicate materials")
	red_material.set_shader_parameter("tint", Vector4(1, 0, 0, 0.5))
	sprite.normal_material = red_material

	# Multi-surface GeometryInstance3D: internal equal-key surface order belongs
	# to Godot. Identical colors prove *both* draws stay inside the insertion gap.
	var multi := ArrayMesh.new()
	var quad := QuadMesh.new()
	quad.size = Vector2(48, 16)
	for i in 2:
		multi.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, quad.surface_get_arrays(0))
		multi.surface_set_material(i, _solid_material(green))
	inserted.mesh = multi
	expected = await _reference([red, green, green, yellow, blue])
	_assert_close("all transparent surfaces of a multi-draw child stay in the gap", expected, (await _render_image()).get_pixel(GRAY_X, SAMPLE_Y))
	var insertion_draws := viewport.get_render_info(Viewport.RENDER_INFO_TYPE_VISIBLE, Viewport.RENDER_INFO_DRAW_CALLS_IN_FRAME)
	# Godot Mobile's "draw calls" statistic is assigned instances.size(), not
	# surface draw count (4.6/4.7 render_forward_mobile.cpp). Pixels above prove
	# both surfaces rendered; don't mislabel Mobile's four-instance telemetry.
	var reported_expected := 4 if RenderingServer.get_current_rendering_method() == "mobile" else 5
	_check(insertion_draws == reported_expected,
		"insertion render telemetry: expected %d, got %d" % [reported_expected, insertion_draws])

	multi.surface_get_material(0).render_priority = 5
	_check(not insertion.get_sorting_warnings().is_empty(), "mismatched surface priorities are diagnosed")
	multi.surface_get_material(0).render_priority = 0

	# Opaque inserted geometry keeps physical depth, not transparent painter order.
	var opaque_shader := Shader.new()
	opaque_shader.code = "shader_type spatial; render_mode unshaded, cull_disabled; void fragment() { ALBEDO = vec3(0.0, 1.0, 0.0); }"
	var opaque := ShaderMaterial.new()
	opaque.shader = opaque_shader
	inserted.mesh = quad
	inserted.material_override = opaque
	second.visible = false
	inserted.position.z = 1
	_assert_close("opaque inserted child in front occludes both Spine batches", Color(0, 1, 0, 1), (await _render_image()).get_pixel(GRAY_X, SAMPLE_Y))
	inserted.position.z = -1
	expected = await _reference([Color(0, 1, 0, 1), red, blue])
	_assert_close("opaque inserted child behind is covered by both Spine batches", expected, (await _render_image()).get_pixel(GRAY_X, SAMPLE_Y))

	insertion.remove_child(inserted)
	_check(inserted.sorting_offset == 4.25 and inserted.sorting_use_aabb_center,
		"detaching inserted geometry restores its original sorting fields")
	inserted.free()
	second.free()
	last_slot.normal_material = null
	await _render_image()
	_check(sprite.get_render_statistics().batches == 1, "removing insertion/state boundaries merges compatible geometry")
	var builds: int = sprite.get_render_statistics().mesh_builds
	var pool: int = sprite.get_render_statistics().batch_pool
	for i in 10:
		sprite.update_skeleton(0)
	_check(sprite.get_render_statistics().mesh_builds == builds and sprite.get_render_statistics().batch_pool == pool,
		"warm frames reuse batch meshes and pool entries")
	insertion.free()
	last_slot.free()
	sprite.normal_material = null

func _many_data(count: int, alternate := false, alternate_texture := false, atlas_override: SpineAtlasResource = null) -> SpineSkeletonDataResource:
	var slots: Array = []
	var attachments := {}
	for i in count:
		var name := "slot%d" % i
		slots.append({"name": name, "bone": "root", "attachment": "region", "blend": "additive" if alternate and i % 2 == 1 else "normal"})
		attachments[name] = {"region": {"type": "region", "path": "other" if alternate_texture and i % 2 == 1 else "bands", "width": 0.4, "height": 0.15,
			"x": (i % 128) * 0.5 - 32, "y": (i / 128) * 0.2 - 12}}
		if count > 16384 and i == count - 1:
			attachments[name].region.merge({"width": 48, "height": 16, "x": 0, "y": 0}, true)
	_write_text("many.spine-json", JSON.stringify({"skeleton": {"spine": "4.3.75"}, "bones": [{"name": "root"}],
		"slots": slots, "skins": [{"name": "default", "attachments": attachments}]}))
	var file := SpineSkeletonFileResource.new()
	_expect_ok("load many-slot skeleton", file.load_from_file(fixture_dir.path_join("many.spine-json")))
	var data := SpineSkeletonDataResource.new()
	data.atlas_res = atlas_override if atlas_override != null else skeleton_data.atlas_res
	data.skeleton_file_res = file
	return data

func _measure(label: String, frames := 120):
	for i in 20:
		sprite.update_skeleton(1.0 / 60.0)
	var before: Dictionary = sprite.get_render_statistics()
	var times: Array[int] = []
	for i in frames:
		var start := Time.get_ticks_usec()
		sprite.update_skeleton(1.0 / 60.0)
		times.append(Time.get_ticks_usec() - start)
	times.sort()
	var after: Dictionary = sprite.get_render_statistics()
	await _render_image()
	var draws := viewport.get_render_info(Viewport.RENDER_INFO_TYPE_VISIBLE, Viewport.RENDER_INFO_DRAW_CALLS_IN_FRAME)
	_check(after.mesh_builds == before.mesh_builds, label + ": no warm mesh creation")
	_check(after.material_builds == before.material_builds, label + ": no warm material duplication")
	_check(after.index_uploads == before.index_uploads, label + ": unchanged topology skips index upload")
	_check(after.vertex_uploads == before.vertex_uploads and after.attribute_uploads == before.attribute_uploads,
		label + ": unchanged data skips vertex/attribute uploads")
	print("BATCH_BENCH ", JSON.stringify({"case": label, "frames": frames, "cpu_median_us": times[frames / 2],
		"cpu_p95_us": times[int(frames * 0.95)], "draw_calls": draws, "stats": after}))
	return draws

func _test_batch_counts_and_capacity():
	# This also crosses Godot's 16-bit index boundary in a single batch.
	for count in [512, 16385]:
		sprite.skeleton_data_res = _many_data(count)
		var draws: int = await _measure("compatible-%d" % count)
		_check(sprite.get_render_statistics().batches == 1 and draws == 1, "%d compatible attachments explicitly merge into one actual draw" % count)
		_check(sprite.get_render_statistics().vertices == count * 4, "all vertices are retained above the old slot/index limits")
		if count == 512:
			var first := _solid_material(Color(1, 0, 0, 0.5))
			var second := _solid_material(Color(0, 0, 1, 0.5))
			sprite.normal_material = first
			sprite.normal_material = second
			var builds: int = sprite.get_render_statistics().material_builds
			for i in 100:
				first.set_shader_parameter("tint", Vector4(1.0, float(i) / 100, 0.0, 0.5))
				sprite.normal_material = first if i % 2 == 0 else second
				sprite.update_skeleton(0)
			_check(sprite.get_render_statistics().material_builds == builds, "animated uniforms and repeated material state changes reuse cached materials")
			sprite.normal_material = null
			for cull in 3:
				for priority in [-1, 0, 1]:
					sprite.cull_mode = cull
					sprite.render_priority = priority
			builds = sprite.get_render_statistics().material_builds
			for i in 30:
				sprite.cull_mode = i % 3
				sprite.render_priority = i % 3 - 1
			_check(sprite.get_render_statistics().material_builds == builds, "repeated culling/priority states reuse their cached materials")
			sprite.cull_mode = SpineSprite3D.CULL_DISABLED
			sprite.render_priority = 0
		if count > 16384:
			_assert_close("indices above 65535 address the final visible attachment", Color.WHITE, (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
			var big_slot := sprite.get_skeleton().find_slot("slot%d" % (count - 1))
			big_slot.get_pose().set_attachment(null)
			big_slot.get_applied_pose().set_attachment(null)
			var builds: int = sprite.get_render_statistics().mesh_builds
			_assert_transparent("shrinking a batch clears stale padded indices", (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
			_check(sprite.get_render_statistics().mesh_builds == builds, "shrinking active geometry keeps the allocated mesh capacity")

	sprite.skeleton_data_res = _many_data(512, true)
	var draws: int = await _measure("alternating-blends-512", 60)
	_check(sprite.get_render_statistics().batches == 512 and draws == 512, "alternating states split without reordering")
	_check(sprite.get_render_statistics().materials == 2, "nonconsecutive batches share two compatible cached materials")

	var image := Image.load_from_file(fixture_dir.path_join("bands.png"))
	_expect_ok("save second atlas page", image.save_png(fixture_dir.path_join("other.png")))
	_write_text("dual.atlas", "bands.png\nsize: 48, 16\nfilter: Nearest, Nearest\nbands\nbounds: 0, 0, 48, 16\n\nother.png\nsize: 48, 16\nfilter: Nearest, Nearest\nother\nbounds: 0, 0, 48, 16\n")
	var atlas := SpineAtlasResource.new()
	_expect_ok("load two-page atlas", atlas.load_from_atlas_file(fixture_dir.path_join("dual.atlas")))
	sprite.skeleton_data_res = _many_data(512, false, true, atlas)
	draws = await _measure("alternating-textures-512", 60)
	_check(sprite.get_render_statistics().batches == 512 and draws == 512 and sprite.get_render_statistics().materials == 2,
		"texture changes split ordered batches and reuse page materials")

	sprite.skeleton_data_res = _many_data(512)
	var helpers: Array[SpineSlotNode3D] = []
	for i in 8:
		var helper := SpineSlotNode3D.new()
		helper.slot_name = "slot%d" % (31 + i * 64)
		sprite.add_child(helper)
		var child := _quad(_solid_material(Color(0, 1, 0, 0.1)))
		child.scale = Vector3(0.02, 0.02, 0.02)
		helper.add_child(child)
		helpers.append(helper)
	draws = await _measure("slot-boundaries-512", 60)
	_check(sprite.get_render_statistics().batches == 9 and draws == 17, "eight insertion boundaries produce nine batches and eight child draws")
	for helper in helpers:
		helper.free()

	# Vary clipping/deform after capacity warm-up. Geometry counts/topology may
	# change; surfaces and materials must not be recreated unless capacity grows.
	sprite.skeleton_data_res = skeleton_data
	slot = sprite.get_skeleton().find_slot("slot")
	clipper_slot = sprite.get_skeleton().find_slot("clipper")
	sprite.get_skeleton().set_attachment("slot", "weighted")
	_set_slot_colors(SLOT_LIGHT, SLOT_DARK, true)
	var deform: Array = [0, 0, 0, 0, 0, 0, 0, 0]
	for i in 40:
		for j in [0, 2, 4, 6]:
			deform[j] = float(i % 20 - 10) * 3
		slot.get_pose().set_deform(deform)
		slot.get_applied_pose().set_deform(deform)
		sprite.update_skeleton(0)
	var before: Dictionary = sprite.get_render_statistics()
	var start := Time.get_ticks_usec()
	for i in 120:
		for j in [0, 2, 4, 6]:
			deform[j] = float(i % 20 - 10) * 3
		slot.get_pose().set_deform(deform)
		slot.get_applied_pose().set_deform(deform)
		sprite.update_skeleton(0)
	var elapsed := Time.get_ticks_usec() - start
	var after: Dictionary = sprite.get_render_statistics()
	_check(before.mesh_builds == after.mesh_builds and before.material_builds == after.material_builds,
		"animated clipping/deform reuses warmed surface capacities and materials")
	_check(after.index_uploads > before.index_uploads and after.vertex_uploads > before.vertex_uploads,
		"animated clipping actually changes topology and vertex buffers")
	await _render_image()
	print("BATCH_BENCH ", JSON.stringify({"case": "animated-clipping-deform", "frames": 120,
		"cpu_mean_including_gdscript_us": elapsed / 120.0, "stats": after}))
