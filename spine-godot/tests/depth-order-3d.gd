extends "batching-3d.gd"

# Experiment using the existing public ShaderMaterial hooks: no renderer changes.
# Compare explicit discard + depth writes in the alpha queue with Godot's
# built-in alpha-scissor material, which can enter the opaque queue.
var depth_material: ShaderMaterial
var split_helper: SpineSlotNode3D
const RED := Color(1, 0, 0, 0.75)
const BLUE := Color(0, 0, 1, 0.75)

func _run():
	if DisplayServer.get_name() == "headless":
		push_error("Depth/order tests require a GPU renderer.")
		quit(1)
		return
	fixture_dir = "user://spine-depth-order-%d" % OS.get_process_id()
	_create_fixture()
	var file := FileAccess.open(fixture_dir.path_join("fixture.spine-json"), FileAccess.READ)
	var data: Dictionary = JSON.parse_string(file.get_as_text())
	file.close()
	for name in ["region", "mesh", "weighted"]:
		data.skins[0].attachments.slot[name].color = "ffffffff"
	_write_text("fixture.spine-json", JSON.stringify(data))
	var image := Image.create(48, 16, false, Image.FORMAT_RGBA8)
	for y in 16:
		for x in 48:
			image.set_pixel(x, y, Color(1, 1, 1, 0.0 if x < 16 else 0.25 if x < 32 else 1.0))
	_expect_ok("save alpha-cutoff bands", image.save_png(fixture_dir.path_join("bands.png")))
	_create_viewport()
	_set_clipping(false)
	sprite.get_skeleton().set_color(Color.WHITE)
	sprite.get_skeleton().set_attachment("spare", "region")
	_set_colors(sprite, slot, RED, Color.BLACK, false)
	_set_colors(sprite, sprite.get_skeleton().find_slot("spare"), BLUE, Color.BLACK, false)
	depth_material = _depth_material(false, true)
	sprite.normal_material = depth_material

	if OS.get_cmdline_user_args().has("--offsets"):
		await _test_depth_offsets()
		if split_helper != null:
			split_helper.free()
		depth_material = null
		_finish()
		return

	for split in [false, true]:
		_set_split(split, depth_material)
		for projection in [Camera3D.PROJECTION_ORTHOGONAL, Camera3D.PROJECTION_PERSPECTIVE]:
			camera.projection = projection
			for back in [false, true]:
				var distance := 40 if projection == Camera3D.PROJECTION_PERSPECTIVE else 10
				camera.position.z = -distance if back else distance
				camera.rotation.y = PI if back else 0
				for swapped in [false, true]:
					_set_order(swapped)
					var colors: Array[Color] = [RED, BLUE]
					if swapped:
						colors.reverse()
					var expected := await _reference(colors)
					var rendered := await _render_image()
					var solid_x := BLACK_X if back else WHITE_X
					_assert_close("depth-writing batch order split=%s projection=%d back=%s swapped=%s" % [split, projection, back, swapped],
						expected, rendered.get_pixel(solid_x, SAMPLE_Y))
					_assert_transparent("below-threshold fragments are discarded", rendered.get_pixel(GRAY_X, SAMPLE_Y))
					_check(sprite.get_render_statistics().batches == (2 if split else 1), "test actually exercises one/two native batches")

	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.position.z = 10
	camera.rotation.y = 0
	_set_order(false)
	await _test_scene_depth()
	await _test_transformed_batches()
	await _test_coplanar_insertion()
	await _test_opaque_queue_control()
	if split_helper != null:
		split_helper.free()
	depth_material = null
	print("Depth-write/discard experiment: ", "PASS" if failures.is_empty() else "FAIL")
	_finish()

func _test_depth_offsets():
	_set_split(true, depth_material)
	sprite.rotation = Vector3(0.12, 0.35, 0.18)
	sprite.scale = Vector3(-1.3, 0.8, 1)
	var successful_front_offsets: Array[float] = []
	for offset in [0.0, 0.00001, 0.0001, 0.001, 0.01]:
		sprite.slot_depth_offset = offset
		var front_passes := 0
		var front_total := 0
		for projection in [Camera3D.PROJECTION_ORTHOGONAL, Camera3D.PROJECTION_PERSPECTIVE]:
			camera.projection = projection
			for back in [false, true]:
				camera.position.z = -60 if back else 60
				camera.rotation.y = PI if back else 0
				for swapped in [false, true]:
					_set_order(swapped)
					sprite.get_skeleton().set_attachment("slot", "mesh")
					var colors: Array[Color] = [RED, BLUE]
					if swapped:
						colors.reverse()
					var expected := await _reference(colors)
					var near_only := await _reference([colors[0]])
					var point := Vector2i(camera.unproject_position(sprite.to_global(Vector3(16, 0, 0))))
					var actual := (await _render_image()).get_pixelv(point)
					var matches_order := maxf(absf(actual.r - expected.r), absf(actual.b - expected.b)) <= TOLERANCE
					var matches_near := maxf(absf(actual.r - near_only.r), absf(actual.b - near_only.b)) <= TOLERANCE
					if not back:
						front_total += 1
						if matches_order:
							front_passes += 1
					print("DEPTH_OFFSET_PROBE ", JSON.stringify({"offset": offset, "projection": projection, "back": back,
						"swapped": swapped, "matches_spine_order": matches_order, "matches_first_near_surface": matches_near,
						"actual": actual, "expected": expected}))
		if offset > 0 and front_passes == front_total:
			successful_front_offsets.append(offset)
	_check(successful_front_offsets.has(0.001), "a 0.001-unit slot depth offset fixes the front-view precision repro")
	print("DEPTH_OFFSET_SUMMARY ", JSON.stringify({"renderer": RenderingServer.get_current_rendering_method(),
		"successful_front_offsets": successful_front_offsets}))

	# Recheck ordinary world composition and slot insertion with a passing offset.
	sprite.rotation = Vector3.ZERO
	sprite.scale = Vector3.ONE
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.position.z = 10
	camera.rotation.y = 0
	if not successful_front_offsets.is_empty():
		sprite.slot_depth_offset = 0.001
		_set_order(false)
		await _test_scene_depth()
		await _test_coplanar_insertion()
		await _test_opaque_queue_control()

func _depth_material(scissor: bool, writes: bool, diagnostic_constant_depth := false) -> ShaderMaterial:
	var shader := Shader.new()
	shader.code = """shader_type spatial;
render_mode unshaded, cull_disabled, blend_mix, %s;
uniform sampler2D spine_texture : repeat_disable;
uniform bool spine_premultiplied_alpha = false;
uniform float alpha_threshold = 0.5;
varying vec4 spine_color;
void vertex() { spine_color = CUSTOM0; }
void fragment() {
	float opacity = texture(spine_texture, UV).a * spine_color.a;
	%s
	ALBEDO = spine_color.rgb;
	ALPHA = opacity;
	%s
}
""" % ["depth_draw_always" if writes else "depth_draw_never",
		"ALPHA_SCISSOR_THRESHOLD = alpha_threshold;" if scissor else "if (opacity < alpha_threshold) discard;",
		"DEPTH = 0.5;" if diagnostic_constant_depth else ""]
	var material := ShaderMaterial.new()
	material.shader = shader
	return material

func _set_order(swapped: bool):
	sprite.get_animation_state().clear_tracks()
	sprite.get_skeleton().set_to_setup_pose()
	_set_clipping(false)
	sprite.get_skeleton().set_attachment("spare", "region")
	sprite.get_skeleton().set_color(Color.WHITE)
	_set_colors(sprite, slot, RED, Color.BLACK, false)
	_set_colors(sprite, sprite.get_skeleton().find_slot("spare"), BLUE, Color.BLACK, false)
	if swapped:
		sprite.get_animation_state().set_animation("swap", false, 0)
	sprite.update_skeleton(0)

func _set_split(split: bool, material: ShaderMaterial):
	if split_helper != null:
		split_helper.free()
		split_helper = null
	if split:
		split_helper = SpineSlotNode3D.new()
		split_helper.slot_name = "spare"
		split_helper.normal_material = material.duplicate()
		sprite.add_child(split_helper)
	sprite.update_skeleton(0)

func _test_scene_depth():
	_set_split(true, depth_material)
	var expected := await _reference([RED, BLUE])
	var overlay := _quad(_solid_material(Color.GREEN))
	overlay.mesh.material.render_priority = 10 # Force submission after Spine.
	overlay.position.z = -1
	viewport.add_child(overlay)
	var rendered := await _render_image()
	_assert_close("Spine depth blocks a later transparent object behind it", expected, rendered.get_pixel(WHITE_X, SAMPLE_Y))
	_assert_close("alpha-zero holes do not write depth", Color.GREEN, rendered.get_pixel(BLACK_X, SAMPLE_Y))
	_assert_close("below-cutoff pixels do not write depth", Color.GREEN, rendered.get_pixel(GRAY_X, SAMPLE_Y))
	overlay.position.z = 1
	_assert_close("later transparent object in front passes depth test", Color.GREEN, (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	overlay.position.z = -1
	sprite.normal_material = _depth_material(false, false)
	_set_split(true, sprite.normal_material)
	_assert_close("no-depth-write control lets the forced-late object show through", Color.GREEN, (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	sprite.normal_material = depth_material
	_set_split(true, depth_material)
	overlay.free()

	var opaque_expected := await _reference([Color.GREEN, RED, BLUE])
	var opaque_shader := Shader.new()
	opaque_shader.code = "shader_type spatial; render_mode unshaded, cull_disabled; void fragment() { ALBEDO = vec3(0, 1, 0); }"
	var opaque_material := ShaderMaterial.new()
	opaque_material.shader = opaque_shader
	var opaque := _quad(opaque_material)
	opaque.position.z = -1
	viewport.add_child(opaque)
	_assert_close("depth-writing Spine blends over opaque geometry behind", opaque_expected, (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	opaque.position.z = 1
	_assert_close("opaque geometry in front occludes Spine", Color.GREEN, (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	opaque.free()

func _test_transformed_batches():
	_set_split(true, depth_material)
	sprite.rotation = Vector3(0.12, 0.35, 0.18)
	sprite.scale = Vector3(-1.3, 0.8, 1)
	for projection in [Camera3D.PROJECTION_ORTHOGONAL, Camera3D.PROJECTION_PERSPECTIVE]:
		camera.projection = projection
		for back in [false, true]:
			camera.position.z = -60 if back else 60
			camera.rotation.y = PI if back else 0
			for swapped in [false, true]:
				_set_order(swapped)
				sprite.get_skeleton().set_attachment("slot", "mesh")
				var colors: Array[Color] = [RED, BLUE]
				if swapped:
					colors.reverse()
				var expected := await _reference(colors)
				var point := Vector2i(camera.unproject_position(sprite.to_global(Vector3(16, 0, 0))))
				var rendered := await _render_image()
				var actual := rendered.get_pixelv(point)
				if maxf(absf(actual.r - expected.r), absf(actual.b - expected.b)) > TOLERANCE:
					var no_depth := _depth_material(false, false)
					sprite.normal_material = no_depth
					_set_split(true, no_depth)
					var control := (await _render_image()).get_pixelv(point)
					_assert_close("same transformed geometry/order without depth writes", expected, control)
					# Diagnostic only, NOT a proposed scene-composition depth value.
					var equal_depth := _depth_material(false, true, true)
					sprite.normal_material = equal_depth
					_set_split(true, equal_depth)
					var equal_depth_pixel := (await _render_image()).get_pixelv(point)
					_assert_close("identical fragment depth removes the transformed overlap failure", expected, equal_depth_pixel)
					print("DEPTH_PRECISION_PROBE ", JSON.stringify({"projection": projection, "back": back, "swapped": swapped,
						"expected": expected, "with_writes": actual, "without_writes": control, "constant_depth": equal_depth_pixel}))
					sprite.normal_material = depth_material
					_set_split(true, depth_material)
				_assert_close("rotated/mirrored mesh+region batches projection=%d back=%s swapped=%s" % [projection, back, swapped],
					expected, rendered.get_pixelv(point))
	sprite.rotation = Vector3.ZERO
	sprite.scale = Vector3.ONE
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.position.z = 10
	camera.rotation.y = 0
	_set_order(false)

func _test_coplanar_insertion():
	var green := Color(0, 1, 0, 0.75)
	var expected := await _reference([RED, green, BLUE])
	var helper := SpineSlotNode3D.new()
	helper.slot_name = "slot"
	sprite.add_child(helper)
	helper.add_child(_quad(_solid_material(green)))
	_assert_close("coplanar slot insertion survives depth-writing batches", expected, (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	helper.free()

func _test_opaque_queue_control():
	var scissor := _depth_material(true, true)
	sprite.normal_material = scissor
	_set_split(true, scissor)
	_set_order(false)
	var before := (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y)
	_set_order(true)
	var after := (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y)
	var changed := maxf(absf(before.r - after.r), absf(before.b - after.b)) > TOLERANCE
	# This is a diagnostic control, not a requirement that every Godot/backend
	# must choose the same opaque sort key or exhibit the same wrong order.
	print("OPAQUE_QUEUE_CONTROL ", JSON.stringify({"renderer": RenderingServer.get_current_rendering_method(),
		"spine_order_changed_pixel": changed, "before": before, "after": after}))
