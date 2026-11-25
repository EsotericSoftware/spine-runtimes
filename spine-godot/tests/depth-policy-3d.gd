extends "depth-order-3d.gd"

func _use_no_write_rendering(_target: SpineSprite3D):
	pass # This test deliberately exercises the new native defaults.

func _run():
	if DisplayServer.get_name() == "headless":
		push_error("Depth-policy tests require a GPU renderer.")
		quit(1)
		return
	fixture_dir = "user://spine-depth-policy-%d" % OS.get_process_id()
	_create_fixture()
	var data: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(fixture_dir.path_join("fixture.spine-json")))
	for name in ["region", "mesh", "weighted"]:
		data.skins[0].attachments.slot[name].color = "ffffffff"
	_write_text("fixture.spine-json", JSON.stringify(data))
	var bands := Image.create(48, 16, false, Image.FORMAT_RGBA8)
	for y in 16:
		for x in 48:
			bands.set_pixel(x, y, Color(1, 1, 1, 0 if x < 16 else 0.25 if x < 32 else 1))
	_expect_ok("save depth bands", bands.save_png(fixture_dir.path_join("bands.png")))
	_create_viewport()
	_check(sprite.camera_relative_depth and sprite.depth_write_enabled, "camera-relative depth writes default on")
	_check(is_equal_approx(sprite.slot_depth_offset, 0.001) and is_equal_approx(sprite.alpha_cutoff, 0.001), "native gap and cutoff defaults")
	_set_order(false)
	await _test_native_order(0.01, 0.001) # Default pixel size and depth spacing.
	await _test_native_order(1.0, 0.01) # Large 48-unit geometry, camera 60 units away.
	await _test_native_depth_and_cache()
	await _test_custom_depth_contract()
	await _test_two_views_and_bounds()
	await _test_clipping_and_deform_depth()
	await _test_child_camera()
	print("Spine native depth policy: ", "PASS" if failures.is_empty() else "FAIL")
	_finish()

func _test_native_order(units: float, gap: float):
	sprite.pixel_size = units
	sprite.slot_depth_offset = gap
	camera.size = 48 * units
	var boundary := SpineSlotNode3D.new()
	boundary.slot_name = "slot"
	# An empty GeometryInstance3D supplies an insertion boundary without pixels.
	boundary.add_child(MeshInstance3D.new())
	for split in [false, true]:
		if split:
			sprite.add_child(boundary)
		for scale in [Vector3.ONE, Vector3(-1.3, 0.8, 1), Vector3(-1.3, 0.8, -1)]:
			sprite.scale = scale
			sprite.rotation = Vector3(0.12, 0.35, 0.18)
			for projection in [Camera3D.PROJECTION_ORTHOGONAL, Camera3D.PROJECTION_PERSPECTIVE]:
				camera.projection = projection
				for back in [false, true]:
					camera.position.z = (-60 if back else 60) * units
					camera.rotation.y = PI if back else 0
					for swapped in [false, true]:
						boundary.slot_name = "spare" if swapped else "slot"
						_set_order(swapped)
						sprite.get_skeleton().set_attachment("slot", "mesh")
						var colors: Array[Color] = [RED, BLUE]
						if swapped:
							colors.reverse()
						var expected := await _reference(colors)
						var point := Vector2i(camera.unproject_position(sprite.to_global(Vector3(16 * units, 0, 0))))
						_assert_close("native depth order units=%s gap=%s split=%s scale=%s projection=%d back=%s swap=%s" % [units, gap, split, scale, projection, back, swapped],
							expected, (await _render_image()).get_pixelv(point))
						_check(sprite.get_render_statistics().batches == (2 if split else 1), "native generated shader batching")
	boundary.free()
	sprite.pixel_size = 1
	sprite.slot_depth_offset = 0.001
	camera.size = 48
	sprite.rotation = Vector3.ZERO
	sprite.scale = Vector3.ONE
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.position.z = 10
	camera.rotation.y = 0
	_set_order(false)

func _test_native_depth_and_cache():
	var expected := await _reference([RED, BLUE])
	var overlay := _quad(_solid_material(Color.GREEN))
	overlay.mesh.material.render_priority = 10
	overlay.position.z = -1
	viewport.add_child(overlay)
	_assert_close("native writes block forced-late transparent geometry behind", expected, (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	_assert_close("alpha zero never writes depth", Color.GREEN, (await _render_image()).get_pixel(BLACK_X, SAMPLE_Y))
	sprite.alpha_cutoff = 0.5
	_assert_close("native cutoff discards tint-times-texture alpha before depth", Color.GREEN, (await _render_image()).get_pixel(GRAY_X, SAMPLE_Y))
	sprite.depth_write_enabled = false
	_assert_close("explicit no-write mode restores late transparent overlay", Color.GREEN, (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	sprite.depth_write_enabled = true
	overlay.position.z = 1
	_assert_close("transparent geometry in front survives", Color.GREEN, (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	overlay.free()
	var before := sprite.get_render_statistics()
	for i in 8:
		sprite.camera_relative_depth = i % 2 == 0
		sprite.alpha_cutoff = 0.25 if i % 2 == 0 else 0.5
		sprite.depth_write_enabled = i % 2 == 0
	sprite.camera_relative_depth = true
	sprite.depth_write_enabled = true
	var after := sprite.get_render_statistics()
	_check(after.material_builds == before.material_builds and after.mesh_builds == before.mesh_builds, "warmed depth states reuse materials and meshes")

func _custom_depth_material(invisible := false) -> ShaderMaterial:
	var shader := Shader.new()
	shader.code = "shader_type spatial;\nrender_mode unshaded, cull_disabled, blend_mix, depth_draw_always;\n" + SpineSprite3D.get_depth_shader_code() + """
uniform sampler2D spine_texture : repeat_disable;
varying vec4 tint;
void vertex() {
	VERTEX = spine_apply_camera_depth(VERTEX, MODEL_MATRIX[3].xyz, MODEL_NORMAL_MATRIX[2], INV_VIEW_MATRIX[3].xyz);
	tint = CUSTOM0;
}
void fragment() {
	ALBEDO = tint.rgb;
	ALPHA = %s;
	spine_apply_alpha_cutoff(ALPHA);
}
""" % ("0.0" if invisible else "texture(spine_texture, UV).a * tint.a")
	var material := ShaderMaterial.new()
	material.shader = shader
	# Reserved uniforms in the source template must not override the node.
	material.set_shader_parameter("spine_camera_relative_depth", false)
	material.set_shader_parameter("spine_alpha_cutoff", 1.0)
	return material

func _test_custom_depth_contract():
	camera.position.z = -10
	camera.rotation.y = PI
	var expected := await _reference([RED, BLUE])
	var material := _custom_depth_material()
	var source := material.shader.code
	sprite.normal_material = material
	_assert_close("custom helper receives node-owned uniforms", expected, (await _render_image()).get_pixel(BLACK_X, SAMPLE_Y))
	var builds: int = sprite.get_render_statistics().material_builds
	sprite.normal_material = material
	_assert_close("template refresh retains node-owned uniforms", expected, (await _render_image()).get_pixel(BLACK_X, SAMPLE_Y))
	_check(sprite.get_render_statistics().material_builds == builds, "single-pass uniform refresh retains material objects")
	_check(material.shader.code == source and material.get_shader_parameter("spine_camera_relative_depth") == false, "user shader and source uniforms remain untouched")
	var first := _custom_depth_material(true)
	first.next_pass = material
	sprite.normal_material = first
	_assert_close("custom next pass receives camera and cutoff uniforms", expected, (await _render_image()).get_pixel(BLACK_X, SAMPLE_Y))
	sprite.alpha_cutoff = 0.9
	_assert_transparent("cutoff updates cached next passes", (await _render_image()).get_pixel(BLACK_X, SAMPLE_Y))
	sprite.alpha_cutoff = 0.5
	sprite.normal_material = null
	camera.position.z = 10
	camera.rotation.y = 0

func _test_two_views_and_bounds():
	var expected := await _reference([RED, BLUE])
	var other := SubViewport.new()
	other.size = VIEWPORT_SIZE
	other.transparent_bg = true
	other.world_3d = viewport.find_world_3d()
	other.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(other)
	var back := Camera3D.new()
	back.projection = Camera3D.PROJECTION_ORTHOGONAL
	back.size = 48
	back.position.z = -10
	back.rotation.y = PI
	other.add_child(back)
	back.current = true
	var bounds := sprite.get_aabb()
	_check(bounds.position.z < 0 and is_equal_approx(bounds.position.z, -bounds.end.z), "culling bounds cover both depth directions")
	var before := sprite.get_render_statistics()
	_assert_close("first viewport front composite", expected, (await _render_image(false)).get_pixel(WHITE_X, SAMPLE_Y))
	_assert_close("simultaneous opposite viewport composite", expected, other.get_texture().get_image().get_pixel(BLACK_X, SAMPLE_Y))
	var after := sprite.get_render_statistics()
	_check(before.vertex_uploads == after.vertex_uploads and before.index_uploads == after.index_uploads, "opposite views require no geometry uploads")
	other.free()

	# Only the flipped, later blue attachment lies inside this far plane.
	var blue := await _reference([BLUE])
	sprite.slot_depth_offset = 2
	camera.position.z = -10
	camera.rotation.y = PI
	camera.far = 7
	_assert_close("expanded bounds retain shader-flipped geometry outside raw positive-Z bounds", blue, (await _render_image()).get_pixel(BLACK_X, SAMPLE_Y))
	sprite.camera_relative_depth = false
	_check(sprite.get_aabb().position.z > 0, "fixed mode restores physical-only bounds")
	_assert_transparent("fixed geometry really is beyond the far plane", (await _render_image()).get_pixel(BLACK_X, SAMPLE_Y))
	sprite.camera_relative_depth = true
	sprite.slot_depth_offset = 0.001
	camera.far = 4000
	camera.position.z = 10
	camera.rotation.y = 0

func _test_clipping_and_deform_depth():
	_set_order(false)
	sprite.alpha_cutoff = 0.001
	var spare := sprite.get_skeleton().find_slot("spare")
	spare.get_pose().set_attachment(null)
	spare.get_applied_pose().set_attachment(null)
	_set_clipping(true)
	var image := await _render_image()
	_assert_visible("native depth shader retains clipped partial-alpha geometry", image.get_pixel(44, SAMPLE_Y))
	_assert_transparent("native clipping excludes the solid right band", image.get_pixel(WHITE_X, SAMPLE_Y))
	_set_clipping(false)
	sprite.get_skeleton().set_attachment("slot", "weighted")
	sprite.get_animation_state().set_animation("deform", false, 0)
	image = await _render_image()
	_check(sprite.get_aabb().position.x > 0, "deformed native bounds update while retaining both depth signs")
	_assert_visible("weighted/deformed geometry renders with native depth defaults", image.get_pixel(80, SAMPLE_Y))
	_set_order(false)

func _test_child_camera():
	var helper := SpineSlotNode3D.new()
	helper.slot_name = "slot"
	sprite.add_child(helper)
	var child := MeshInstance3D.new()
	child.mesh = BoxMesh.new()
	child.position = Vector3(1, 2, 0.3)
	child.scale = Vector3(1, 2, 3)
	helper.add_child(child)
	await _render_image()
	var physical := helper.position.z
	var child_transform := child.transform
	var target := Camera3D.new()
	target.position.z = -10
	viewport.add_child(target)
	helper.depth_camera = helper.get_path_to(target)
	await _render_image(false)
	_check(is_equal_approx(helper.position.z, -physical), "explicit camera moves the slot anchor on a manual sprite")
	_check(child.transform == child_transform, "slot child local geometry/offsets are not reflected")
	target.position.z = 10
	await _render_image(false)
	_check(is_equal_approx(helper.position.z, physical), "camera motion updates the anchor without skeleton advancement")
	target.position.z = -10
	sprite.camera_relative_depth = false
	await _render_image(false)
	_check(is_equal_approx(helper.position.z, physical), "parent opt-out restores physical anchor")
	sprite.camera_relative_depth = true
	await _render_image(false)
	_check(is_equal_approx(helper.position.z, -physical), "parent opt-in restores explicit camera following")
	helper.depth_camera = NodePath()
	_check(is_equal_approx(helper.position.z, physical), "clearing the camera restores physical anchor immediately")
	var foreign := SubViewport.new()
	foreign.own_world_3d = true
	root.add_child(foreign)
	var foreign_camera := Camera3D.new()
	foreign.add_child(foreign_camera)
	helper.depth_camera = helper.get_path_to(foreign_camera)
	await _render_image(false)
	_check(is_equal_approx(helper.position.z, physical), "different-world camera retains physical depth")
	_check(helper.get_sorting_warnings().size() >= 2, "different-world camera is diagnosed")
	foreign.free()
	helper.depth_camera = helper.get_path_to(target)
	target.free()
	await _render_image(false)
	_check(is_equal_approx(helper.position.z, physical), "deleted camera falls back safely")
	_check(helper.get_sorting_warnings().size() >= 2, "invalid camera and single-view limitations are diagnosed")
	helper.depth_camera = NodePath()
	helper.free()
