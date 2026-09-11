extends SceneTree

const VIEWPORT_SIZE := Vector2i(96, 48)
const SAMPLE_Y := 24
const BLACK_X := 32
const GRAY_X := 48
const WHITE_X := 64
const TOLERANCE := 0.04

const SKELETON_COLOR := Color(0.8, 0.7, 0.6, 0.8)
const SLOT_LIGHT := Color(0.9, 0.6, 0.8, 0.75)
const SLOT_DARK := Color(0.1, 0.7, 0.2, 1.0)
const DYNAMIC_DARK := Color(0.8, 0.1, 0.9, 1.0)
const ATTACHMENT_COLOR := Color(0.7, 0.5, 0.9, 0.5)

var fixture_dir: String
var viewport: SubViewport
var camera: Camera3D
var sprite: SpineSprite3D
var slot: SpineSlot
var clipper_slot: SpineSlot
var skeleton_data: SpineSkeletonDataResource
var failures: Array[String] = []

func _init():
	call_deferred("_run")

func _run():
	if DisplayServer.get_name() == "headless":
		push_error("Spine 3D rendering tests require a GPU renderer; do not use --headless.")
		quit(1)
		return
	fixture_dir = "user://spine-godot-3d-rendering-regression-%d" % OS.get_process_id()
	_create_fixture()
	if failures.is_empty():
		_create_viewport()
	if failures.is_empty():
		print("Spine 3D rendering regression: region, mesh, tint and clipping")
		await _test_region_mesh_tint_and_clipping()
		await _test_deform_scale_visibility_and_draw_order()
		print("Spine 3D rendering regression: blending and custom material")
		await _test_blending_and_custom_material()
		await _test_premultiplied_atlas()
		print("Spine 3D rendering regression: front/back culling")
		await _test_front_back_culling()
		print("Spine 3D rendering regression: multiple-character priorities")
		await _test_multiple_character_priorities()
	_finish()

func _test_region_mesh_tint_and_clipping():
	_set_slot_colors(SLOT_LIGHT, SLOT_DARK, true)
	_set_clipping(true)
	var image := await _render_image()
	_assert_visible("clipped visible band", image.get_pixel(BLACK_X, SAMPLE_Y))
	_assert_transparent("clipped discarded band", image.get_pixel(WHITE_X, SAMPLE_Y))

	_set_clipping(false)
	image = await _render_image()
	var initial_black := _sample(image, BLACK_X)
	var initial_gray := _sample(image, GRAY_X)
	var initial_white := _sample(image, WHITE_X)
	_assert_visible("region black band", initial_black)
	_assert_visible("region gray band", initial_gray)
	_assert_visible("region white band", initial_white)
	_check(initial_black.r < initial_white.r and initial_black.g > initial_white.g,
		"tint black contributes dark color independently of light tint")

	_set_slot_colors(SLOT_LIGHT, DYNAMIC_DARK, true)
	image = await _render_image()
	_assert_not_equal("dynamic dark changes black", initial_black, _sample(image, BLACK_X))
	_assert_close("dynamic dark leaves white controlled by light tint", initial_white, _sample(image, WHITE_X))

	_set_slot_colors(SLOT_LIGHT, SLOT_DARK, false)
	image = await _render_image()
	var no_dark_black := _sample(image, BLACK_X)
	_check(maxf(no_dark_black.r, maxf(no_dark_black.g, no_dark_black.b)) < TOLERANCE, "disabling tint black removes dark contribution")

	_set_slot_colors(SLOT_LIGHT, SLOT_DARK, true)
	sprite.get_skeleton().set_attachment("slot", "mesh")
	image = await _render_image()
	_assert_close("mesh black matches region tint", initial_black, _sample(image, BLACK_X))
	_assert_close("mesh gray matches region tint", initial_gray, _sample(image, GRAY_X))
	_assert_close("mesh white matches region tint", initial_white, _sample(image, WHITE_X))

func _test_deform_scale_visibility_and_draw_order():
	_reset_fixture_state()
	var skin := sprite.new_skin("runtime-skin")
	skin.add_skin(skeleton_data.find_skin("default"))
	sprite.get_skeleton().set_skin(skin)
	_set_clipping(false)
	_check(sprite.get_skeleton().get_skin() != null, "controller-created skin can be installed in 3D")
	sprite.get_skeleton().set_attachment("slot", "weighted")
	var original := await _render_image()
	_assert_visible("weighted mesh", original.get_pixel(WHITE_X, SAMPLE_Y))
	sprite.get_animation_state().set_animation("deform", false, 0)
	var deformed := await _render_image()
	_assert_transparent("deform moves weighted vertices outside old bounds", deformed.get_pixel(BLACK_X, SAMPLE_Y))
	_assert_visible("deform updates mesh bounds", deformed.get_pixel(80, SAMPLE_Y))
	_reset_fixture_state()
	sprite.pixel_size = 0.5
	_assert_transparent("pixel size shrinks geometry", (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	sprite.pixel_size = 1.0
	var before_hide := await _render_image()
	sprite.visible = false
	sprite.pixel_size = 1.0 # Refresh geometry while hidden, without advancing the pose.
	_assert_transparent("hidden renderer stays hidden during property refresh", (await _render_image(false)).get_pixel(WHITE_X, SAMPLE_Y))
	sprite.visible = true
	_assert_close("manual sprite reappears without a new pose update", _sample(before_hide, WHITE_X), _sample(await _render_image(false), WHITE_X))

	var spare := sprite.get_skeleton().find_slot("spare")
	sprite.get_skeleton().set_attachment("spare", "region")
	_set_colors(sprite, spare, Color(0.1, 0.1, 1, 0.7), Color.BLACK, false)
	_set_slot_colors(Color(1, 0.1, 0.1, 0.7), Color.BLACK, false)
	var setup_order := _sample(await _render_image(), WHITE_X)
	sprite.get_animation_state().set_animation("swap", false, 0)
	var reversed_order := _sample(await _render_image(), WHITE_X)
	_assert_not_equal("draw-order animation changes transparent composition", setup_order, reversed_order)
	camera.position.z = -10
	camera.rotation.y = PI
	_assert_close("back camera preserves Spine order", reversed_order, _sample(await _render_image(), BLACK_X))
	camera.position.z = 10
	camera.rotation.y = 0
	_reset_fixture_state()

func _test_blending_and_custom_material():
	_reset_fixture_state()
	var background := MeshInstance3D.new()
	var background_mesh := QuadMesh.new()
	background_mesh.size = Vector2(VIEWPORT_SIZE)
	var background_material := StandardMaterial3D.new()
	background_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	background_material.albedo_color = Color(0.2, 0.3, 0.4, 1.0)
	background_mesh.material = background_material
	background.mesh = background_mesh
	background.position.z = -1.0
	viewport.add_child(background)
	slot.get_data().set_blend_mode(SpineConstant.BlendMode_Normal)
	var normal := _sample(await _render_image(), GRAY_X)
	for unsupported in [SpineConstant.BlendMode_Multiply, SpineConstant.BlendMode_Screen]:
		slot.get_data().set_blend_mode(unsupported)
		_assert_close("unsupported mode explicitly falls back to normal", normal, _sample(await _render_image(), GRAY_X))
	slot.get_data().set_blend_mode(SpineConstant.BlendMode_Normal)
	var light := DirectionalLight3D.new()
	light.light_energy = 16.0
	viewport.add_child(light)
	_assert_close("generated material remains unlit", normal, _sample(await _render_image(), GRAY_X))
	light.free()
	background.position.z = 1.0
	var occluded := _sample(await _render_image(), GRAY_X)
	sprite.visible = false
	_assert_close("opaque 3D depth occludes transparent Spine", occluded, _sample(await _render_image(), GRAY_X))
	sprite.visible = true
	background.position.z = -1.0
	slot.get_data().set_blend_mode(SpineConstant.BlendMode_Additive)
	var additive := _sample(await _render_image(), GRAY_X)
	_assert_not_equal("additive differs honestly from normal", normal, additive)

	var shader := Shader.new()
	shader.code = """shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_never, blend_mix;
uniform sampler2D spine_texture : source_color;
uniform bool spine_premultiplied_alpha = false;
void fragment() {
	vec4 tex = texture(spine_texture, UV);
	ALBEDO = vec3(0.0, 1.0, 0.0);
	ALPHA = tex.a;
}
"""
	var material := ShaderMaterial.new()
	material.shader = shader
	material.render_priority = 20
	var shader_source := shader.code
	sprite.additive_material = material
	sprite.cull_mode = SpineSprite3D.CULL_FRONT # Must not override the authored cull_disabled mode.
	var custom := _sample(await _render_image(), WHITE_X)
	_check(shader.code == shader_source and material.render_priority == 20, "custom shader source and template priority stay unchanged")
	_check(custom.g > custom.r + 0.3 and custom.g > custom.b + 0.3, "custom spatial shader receives the atlas texture without source rewriting")
	sprite.additive_material = null
	material.shader = null
	background.free()

func _test_premultiplied_atlas():
	sprite.visible = false
	var target := SpineSprite3D.new()
	_use_no_write_rendering(target)
	target.update_mode = SpineConstant.UpdateMode_Manual
	target.pixel_size = 1.0
	viewport.add_child(target)
	for blend in [SpineConstant.BlendMode_Normal, SpineConstant.BlendMode_Additive]:
		var reference: Image
		for pma in [false, true]:
			var image := Image.create(48, 16, false, Image.FORMAT_RGBA8)
			for y in 16:
				for x in 48:
					var value := 0.0 if x < 16 else 0.5 if x < 32 else 1.0
					if pma:
						value *= 0.5
					image.set_pixel(x, y, Color(value, value, value, 0.5))
			var name := "pma" if pma else "straight"
			_expect_ok("save alpha texture", image.save_png(fixture_dir.path_join(name + ".png")))
			_write_text(name + ".atlas", "%s.png\nsize: 48, 16\npma: %s\nfilter: Nearest, Nearest\nbands\nbounds: 0, 0, 48, 16\n" % [name, "true" if pma else "false"])
			var atlas := SpineAtlasResource.new()
			_expect_ok("load alpha atlas", atlas.load_from_atlas_file(fixture_dir.path_join(name + ".atlas")))
			var data := SpineSkeletonDataResource.new()
			data.atlas_res = atlas
			data.skeleton_file_res = skeleton_data.skeleton_file_res
			target.skeleton_data_res = data
			var clipping := target.get_skeleton().find_slot("clipper")
			clipping.get_pose().set_attachment(null)
			clipping.get_applied_pose().set_attachment(null)
			var target_slot := target.get_skeleton().find_slot("slot")
			target_slot.get_data().set_blend_mode(blend)
			_set_colors(target, target_slot, SLOT_LIGHT, SLOT_DARK, true)
			var rendered := await _render_image()
			if not pma:
				reference = rendered
			else:
				for x in [BLACK_X, GRAY_X, WHITE_X]:
					_assert_close("PMA matches straight alpha including tint black (blend %d, x %d)" % [blend, x], reference.get_pixel(x, SAMPLE_Y), rendered.get_pixel(x, SAMPLE_Y))
	target.free()
	sprite.visible = true

func _test_front_back_culling():
	_reset_fixture_state()
	sprite.cull_mode = SpineSprite3D.CULL_BACK
	camera.position.z = 10
	camera.rotation.y = 0
	_assert_visible("front camera with back-face culling", (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	sprite.cull_mode = SpineSprite3D.CULL_FRONT
	_assert_transparent("front camera with front-face culling", (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	camera.position.z = -10
	camera.rotation.y = PI
	_assert_visible("back camera with front-face culling", (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	sprite.cull_mode = SpineSprite3D.CULL_BACK
	_assert_transparent("back camera with back-face culling", (await _render_image()).get_pixel(WHITE_X, SAMPLE_Y))
	camera.position.z = 10
	camera.rotation.y = 0
	sprite.cull_mode = SpineSprite3D.CULL_DISABLED

func _test_multiple_character_priorities():
	_reset_fixture_state()
	var second := SpineSprite3D.new()
	_use_no_write_rendering(second)
	second.update_mode = SpineConstant.UpdateMode_Manual
	second.pixel_size = 1.0
	second.render_priority = 40
	viewport.add_child(second)
	second.skeleton_data_res = skeleton_data
	var second_slot := second.get_skeleton().find_slot("slot")
	var second_clipper := second.get_skeleton().find_slot("clipper")
	second_clipper.get_pose().set_attachment(null)
	second_clipper.get_applied_pose().set_attachment(null)
	_set_colors(second, second_slot, Color(0.1, 0.1, 1.0, 0.65), Color.BLACK, false)
	second.position = Vector3(0, 0, 0.1)
	sprite.render_priority = -40
	_set_slot_colors(Color(1.0, 0.1, 0.1, 0.65), Color.BLACK, false)
	var blue_front := _sample(await _render_image(), WHITE_X)
	second.render_priority = -40
	sprite.render_priority = 40
	var red_front := _sample(await _render_image(), WHITE_X)
	_assert_not_equal("disjoint character ranges control whole-character front/back order", blue_front, red_front)
	_check(blue_front.b > red_front.b and red_front.r > blue_front.r, "higher character priority is composited in front")
	sprite.render_priority = 0
	second.render_priority = 0
	_assert_close("overlapping ranges use camera depth for equal-priority slots", blue_front, _sample(await _render_image(), WHITE_X))
	camera.position.z = -10
	camera.rotation.y = PI
	_assert_close("equal-priority character depth reverses from the back", red_front, _sample(await _render_image(), BLACK_X))
	camera.position.z = 10
	camera.rotation.y = 0
	second.free()

func _create_fixture():
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(fixture_dir))
	var image := Image.create(48, 16, false, Image.FORMAT_RGBA8)
	for y in image.get_height():
		for x in image.get_width():
			image.set_pixel(x, y, Color.BLACK if x < 16 else Color(0.5, 0.5, 0.5, 1.0) if x < 32 else Color.WHITE)
	_expect_ok("save fixture texture", image.save_png(fixture_dir.path_join("bands.png")))
	_write_text("fixture.atlas", """bands.png
size: 48, 16
filter: Nearest, Nearest
bands
bounds: 0, 0, 48, 16
""")
	_write_text("fixture.spine-json", """{
"skeleton": { "spine": "4.3.75" },
"bones": [ { "name": "root" } ],
"slots": [
	{ "name": "clipper", "bone": "root", "attachment": "clip" },
	{ "name": "slot", "bone": "root", "attachment": "region" },
	{ "name": "spare", "bone": "root" }
],
"skins": [ { "name": "default", "attachments": {
	"clipper": { "clip": { "type": "clipping", "end": "slot", "vertexCount": 4, "vertices": [ -24, -8, 0, -8, 0, 8, -24, 8 ] } },
	"slot": {
		"region": { "type": "region", "path": "bands", "width": 48, "height": 16, "color": "b380e680" },
		"mesh": { "type": "mesh", "path": "bands", "uvs": [ 0, 0, 1, 0, 1, 1, 0, 1 ], "triangles": [ 0, 1, 2, 2, 3, 0 ], "vertices": [ -24, -8, 24, -8, 24, 8, -24, 8 ], "hull": 4, "width": 48, "height": 16, "color": "b380e680" },
		"weighted": { "type": "mesh", "path": "bands", "uvs": [ 0, 0, 1, 0, 1, 1, 0, 1 ], "triangles": [ 0, 1, 2, 2, 3, 0 ], "vertices": [ 1, 0, -24, -8, 1, 1, 0, 24, -8, 1, 1, 0, 24, 8, 1, 1, 0, -24, 8, 1 ], "hull": 4, "width": 48, "height": 16, "color": "b380e680" }
	},
	"spare": { "region": { "type": "region", "path": "bands", "width": 48, "height": 16 } }
} } ],
"animations": {
	"swap": { "drawOrder": [ { "offsets": [ { "slot": "slot", "offset": 1 } ] } ] },
	"deform": { "attachments": { "default": { "slot": { "weighted": { "deform": [ { "vertices": [ 32, 0, 32, 0, 32, 0, 32, 0 ] } ] } } } } }
}
}""")

func _create_viewport():
	viewport = SubViewport.new()
	viewport.size = VIEWPORT_SIZE
	viewport.transparent_bg = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	viewport.render_target_clear_mode = SubViewport.CLEAR_MODE_ALWAYS
	root.add_child(viewport)
	camera = Camera3D.new()
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.size = 48
	camera.position = Vector3(0, 0, 10)
	viewport.add_child(camera)
	camera.current = true
	var atlas := SpineAtlasResource.new()
	_expect_ok("load fixture atlas", atlas.load_from_atlas_file(fixture_dir.path_join("fixture.atlas")))
	var skeleton_file := SpineSkeletonFileResource.new()
	_expect_ok("load fixture skeleton", skeleton_file.load_from_file(fixture_dir.path_join("fixture.spine-json")))
	skeleton_data = SpineSkeletonDataResource.new()
	skeleton_data.atlas_res = atlas
	skeleton_data.skeleton_file_res = skeleton_file
	sprite = SpineSprite3D.new()
	_use_no_write_rendering(sprite)
	sprite.update_mode = SpineConstant.UpdateMode_Manual
	sprite.pixel_size = 1.0
	sprite.skeleton_data_res = skeleton_data
	viewport.add_child(sprite)
	slot = sprite.get_skeleton().find_slot("slot")
	clipper_slot = sprite.get_skeleton().find_slot("clipper")
	if slot == null or clipper_slot == null:
		_fail("fixture slots were not created")

func _use_no_write_rendering(target: SpineSprite3D):
	# Preserve the original transparent-composition regression. Native depth
	# defaults and camera-relative rendering have their own GPU regression.
	target.depth_write_enabled = false
	target.slot_depth_offset = 0
	target.camera_relative_depth = false
	target.alpha_cutoff = 0

func _reset_fixture_state():
	sprite.normal_material = null
	sprite.additive_material = null
	sprite.cull_mode = SpineSprite3D.CULL_DISABLED
	sprite.render_priority = 0
	sprite.get_animation_state().clear_tracks()
	sprite.get_skeleton().set_to_setup_pose()
	sprite.get_skeleton().set_attachment("slot", "region")
	slot.get_data().set_blend_mode(SpineConstant.BlendMode_Normal)
	_set_clipping(false)
	_set_slot_colors(SLOT_LIGHT, SLOT_DARK, true)

func _set_slot_colors(light: Color, dark: Color, has_dark: bool):
	_set_colors(sprite, slot, light, dark, has_dark)
	sprite.get_skeleton().set_color(SKELETON_COLOR)

func _set_colors(target: SpineSprite3D, target_slot: SpineSlot, light: Color, dark: Color, has_dark: bool):
	for pose in [target_slot.get_pose(), target_slot.get_applied_pose()]:
		pose.set_color(light)
		pose.set_dark_color(dark)
		pose.set_has_dark_color(has_dark)
	target.update_skeleton(0.0)

func _set_clipping(enabled: bool):
	var clipping = sprite.get_skeleton().get_attachment_by_slot_name("clipper", "clip") if enabled else null
	clipper_slot.get_pose().set_attachment(clipping)
	clipper_slot.get_applied_pose().set_attachment(clipping)
	sprite.update_skeleton(0.0)

func _render_image(update := true) -> Image:
	if update:
		sprite.update_skeleton(0.0)
	await process_frame
	await process_frame
	await process_frame
	# A manual-mode sprite in an otherwise empty main window need not request a
	# window redraw. Explicitly draw this offscreen test target before readback.
	RenderingServer.force_draw(false, 0.0)
	RenderingServer.force_sync()
	return viewport.get_texture().get_image()

func _sample(image: Image, x: int) -> Color:
	var color := image.get_pixel(x, SAMPLE_Y)
	if color.a > 0.0:
		color.r /= color.a
		color.g /= color.a
		color.b /= color.a
	return color

func _assert_visible(label: String, color: Color):
	_check(color.a > 0.1, label + ": expected a visible pixel, got %s" % color)

func _assert_transparent(label: String, color: Color):
	_check(color.a < 0.02, label + ": expected a transparent pixel, got %s" % color)

func _assert_close(label: String, expected: Color, actual: Color):
	var difference := maxf(maxf(absf(actual.r - expected.r), absf(actual.g - expected.g)), maxf(absf(actual.b - expected.b), absf(actual.a - expected.a)))
	_check(difference <= TOLERANCE, "%s: expected %s, got %s" % [label, expected, actual])

func _assert_not_equal(label: String, a: Color, b: Color):
	var difference := maxf(maxf(absf(a.r - b.r), absf(a.g - b.g)), maxf(absf(a.b - b.b), absf(a.a - b.a)))
	_check(difference > TOLERANCE, "%s: expected different pixels, both were %s" % [label, a])

func _expect_ok(label: String, error: Error):
	if error != OK:
		_fail("%s failed: %s" % [label, error_string(error)])

func _write_text(file_name: String, contents: String):
	var file := FileAccess.open(fixture_dir.path_join(file_name), FileAccess.WRITE)
	if file == null:
		_fail("could not write fixture file %s" % file_name)
		return
	file.store_string(contents)
	file.close()

func _check(condition: bool, message: String):
	if not condition:
		_fail(message)

func _fail(message: String):
	failures.append(message)

func _finish():
	slot = null
	clipper_slot = null
	sprite = null
	camera = null
	if viewport != null:
		viewport.free()
	viewport = null
	skeleton_data = null
	for file_name in ["bands.png", "fixture.atlas", "fixture.spine-json", "pma.png", "pma.atlas", "straight.png", "straight.atlas"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir.path_join(file_name)))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir))
	if failures.is_empty():
		print("Spine 3D rendering regression passed.")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		print("Spine 3D rendering regression failed with %d assertion(s)." % failures.size())
		quit(1)
