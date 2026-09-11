extends SceneTree

const VIEWPORT_SIZE := Vector2i(96, 32)
const SAMPLE_Y := 16
const BLACK_X := 32
const GRAY_X := 48
const WHITE_X := 64
const TOLERANCE := 0.025

const SKELETON_COLOR := Color(0.8, 0.7, 0.6, 0.8)
const SLOT_LIGHT := Color(0.9, 0.6, 0.8, 0.75)
const SLOT_DARK := Color(0.1, 0.7, 0.2, 1.0)
const DYNAMIC_DARK := Color(0.8, 0.1, 0.9, 1.0)
const ATTACHMENT_COLOR := Color(0.7, 0.5, 0.9, 0.5)
const PARENT_MODULATE := Color(0.5, 0.75, 0.25, 0.5)

var fixture_dir: String
var viewport: SubViewport
var sprite: SpineSprite
var slot: SpineSlot
var clipper_slot: SpineSlot
var slot_node: SpineSlotNode
var background: Polygon2D
var rendering_device: RenderingDevice
var use_linear_colors := false
var failures: Array[String] = []

func _init():
	call_deferred("_run")

func _run():
	if DisplayServer.get_name() == "headless":
		push_error("Tint-black rendering tests require a GPU renderer; do not use --headless.")
		quit(1)
		return
	fixture_dir = "user://spine-godot-tint-black-regression-%d" % OS.get_process_id()
	rendering_device = RenderingServer.get_rendering_device()
	_create_fixture()
	if failures.is_empty():
		_create_viewport()
	if not failures.is_empty():
		_finish()
		return

	# Compatibility must keep sRGB attributes even when this viewport option is enabled.
	for use_hdr in [false, true]:
		viewport.use_hdr_2d = use_hdr
		await _render_image()
		use_linear_colors = viewport.is_using_hdr_2d() and rendering_device != null
		var mode := "HDR 2D" if use_hdr else "SDR"
		print("Tint-black regression: %s, RenderingDevice=%s, linear expectations=%s" % [mode, rendering_device != null, use_linear_colors])
		await _run_mode(mode)
	_finish()

func _finish():
	_cleanup()
	if failures.is_empty():
		print("Tint-black rendering regression passed.")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		print("Tint-black rendering regression failed with %d assertion(s)." % failures.size())
		quit(1)

func _run_mode(mode: String):
	_set_slot_colors(SLOT_LIGHT, SLOT_DARK, true)
	_set_clipping(true)
	var image := await _render_image()
	_assert_color(mode + " initial clipping visible band", _sample(image, BLACK_X), _two_color(0.0, _light_color(), _dark_color()))
	_assert_color(mode + " initial clipping discarded band", _sample(image, WHITE_X), Color(0, 0, 0, 0))
	_set_clipping(false)
	image = await _render_image()
	_assert_two_color_bands(mode + " clipping topology update", image, _light_color(), _dark_color())

	_reset_fixture_state()
	_set_slot_colors(SLOT_LIGHT, SLOT_DARK, true)
	image = await _render_image()
	_assert_two_color_bands(mode + " region", image, _light_color(), _dark_color())

	_set_slot_colors(SLOT_LIGHT, DYNAMIC_DARK, true)
	image = await _render_image()
	_assert_two_color_bands(mode + " dynamic dark color", image, _light_color(), _dark_color(DYNAMIC_DARK))

	_set_slot_colors(SLOT_LIGHT, DYNAMIC_DARK, false)
	image = await _render_image()
	_assert_default_bands(mode + " dark color disabled", image, _light_color())

	_set_slot_colors(SLOT_LIGHT, SLOT_DARK, true)
	image = await _render_image()
	_assert_two_color_bands(mode + " dark color re-enabled", image, _light_color(), _dark_color())

	sprite.get_skeleton().set_attachment("slot", "mesh")
	image = await _render_image()
	_assert_two_color_bands(mode + " mesh attachment", image, _light_color(), _dark_color())

	sprite.get_animation_state().set_animation("swap-draw-order", false, 0)
	image = await _render_image()
	var draw_order := sprite.get_skeleton().get_draw_order()
	if draw_order.size() != 3 or draw_order[1].get_data().get_name() != "spare":
		_fail(mode + " draw-order animation did not move the rendered slot")
	_assert_two_color_bands(mode + " draw-order change", image, _light_color(), _dark_color())

	await _assert_custom_material_override(mode)
	await _assert_parent_modulation(mode)
	await _assert_zero_light_color(mode)
	await _assert_slot_node_material_override(mode)
	await _assert_blend_modes(mode)

func _create_fixture():
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(fixture_dir))
	var image := Image.create(48, 16, false, Image.FORMAT_RGBA8)
	for y in image.get_height():
		for x in image.get_width():
			var band := Color.BLACK if x < 16 else Color(0.5, 0.5, 0.5, 1.0) if x < 32 else Color.WHITE
			image.set_pixel(x, y, band)
	_expect_error("save fixture texture", image.save_png(fixture_dir.path_join("bands.png")))

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
"skins": [ {
	"name": "default",
	"attachments": {
		"clipper": {
			"clip": { "type": "clipping", "end": "slot", "vertexCount": 4, "vertices": [ -24, -8, 0, -8, 0, 8, -24, 8 ] }
		},
		"slot": {
			"region": { "type": "region", "path": "bands", "width": 48, "height": 16, "color": "b380e680" },
			"mesh": {
				"type": "mesh", "path": "bands", "uvs": [ 0, 0, 1, 0, 1, 1, 0, 1 ],
				"triangles": [ 0, 1, 2, 2, 3, 0 ], "vertices": [ -24, -8, 24, -8, 24, 8, -24, 8 ],
				"hull": 4, "width": 48, "height": 16, "color": "b380e680"
			}
		}
	}
} ],
"animations": {
	"swap-draw-order": { "drawOrder": [ { "offsets": [ { "slot": "slot", "offset": 1 } ] } ] }
}
}""")

func _create_viewport():
	viewport = SubViewport.new()
	viewport.size = VIEWPORT_SIZE
	viewport.transparent_bg = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	viewport.render_target_clear_mode = SubViewport.CLEAR_MODE_ALWAYS
	root.add_child(viewport)

	background = Polygon2D.new()
	background.polygon = PackedVector2Array([Vector2(0, 0), Vector2(VIEWPORT_SIZE.x, 0), Vector2(VIEWPORT_SIZE), Vector2(0, VIEWPORT_SIZE.y)])
	background.color = Color(0.35, 0.45, 0.55, 1.0)
	background.visible = false
	viewport.add_child(background)

	var atlas := SpineAtlasResource.new()
	_expect_error("load fixture atlas", atlas.load_from_atlas_file(fixture_dir.path_join("fixture.atlas")))
	var skeleton_file := SpineSkeletonFileResource.new()
	_expect_error("load fixture skeleton", skeleton_file.load_from_file(fixture_dir.path_join("fixture.spine-json")))
	if not failures.is_empty():
		return
	var skeleton_data := SpineSkeletonDataResource.new()
	skeleton_data.atlas_res = atlas
	skeleton_data.skeleton_file_res = skeleton_file

	sprite = SpineSprite.new()
	sprite.position = Vector2(48, 16)
	sprite.skeleton_data_res = skeleton_data
	viewport.add_child(sprite)
	if sprite.get_skeleton() == null:
		_fail("fixture skeleton was not created")
		return
	slot = sprite.get_skeleton().find_slot("slot")
	clipper_slot = sprite.get_skeleton().find_slot("clipper")
	slot_node = SpineSlotNode.new()
	sprite.add_child(slot_node)
	slot_node.set("slot_name", "slot")
	if slot == null or clipper_slot == null:
		_fail("fixture slots were not created")

func _reset_fixture_state():
	sprite.normal_material = null
	sprite.modulate = Color.WHITE
	background.visible = false
	sprite.get_animation_state().clear_tracks()
	sprite.get_skeleton().set_to_setup_pose()
	_set_clipping(false)
	sprite.get_skeleton().set_attachment("slot", "region")
	slot.get_data().set_blend_mode(SpineConstant.BlendMode_Normal)
	_set_slot_colors(SLOT_LIGHT, SLOT_DARK, true)

func _set_slot_colors(light: Color, dark: Color, has_dark_color: bool):
	var pose := slot.get_pose()
	pose.set_color(light)
	pose.set_dark_color(dark)
	pose.set_has_dark_color(has_dark_color)
	var applied_pose := slot.get_applied_pose()
	applied_pose.set_color(light)
	applied_pose.set_dark_color(dark)
	applied_pose.set_has_dark_color(has_dark_color)
	sprite.get_skeleton().set_color(SKELETON_COLOR)

func _assert_custom_material_override(mode: String):
	_reset_fixture_state()
	var shader := Shader.new()
	shader.code = "shader_type canvas_item;\nvoid fragment() { COLOR = COLOR; }"
	var material := ShaderMaterial.new()
	material.shader = shader
	sprite.normal_material = material
	var image := await _render_image()
	_assert_color(mode + " custom material black band retains light COLOR", _sample(image, BLACK_X), Color(0, 0, 0, _light_color().a))
	_assert_color(mode + " custom material white band retains light COLOR", _sample(image, WHITE_X), _light_color())
	sprite.normal_material = null
	material.shader = null

func _assert_parent_modulation(mode: String):
	_reset_fixture_state()
	sprite.modulate = PARENT_MODULATE
	var image := await _render_image()
	var light := _multiply(_light_color(), _to_render_color(PARENT_MODULATE))
	var dark := _multiply_rgb(_dark_color(), _to_render_color(PARENT_MODULATE))
	_assert_two_color_bands(mode + " parent CanvasItem modulation", image, light, dark)
	sprite.modulate = Color.WHITE

func _assert_zero_light_color(mode: String):
	_reset_fixture_state()
	var zero_light := Color(0, 0, 0, SLOT_LIGHT.a)
	_set_slot_colors(zero_light, SLOT_DARK, true)
	sprite.modulate = PARENT_MODULATE
	var image := await _render_image()
	var light := _multiply(_light_color(zero_light), _to_render_color(PARENT_MODULATE))
	var dark := _multiply_rgb(_dark_color(), _to_render_color(PARENT_MODULATE))
	_assert_two_color_bands(mode + " zero light with parent modulation", image, light, dark)
	sprite.modulate = Color.WHITE

func _assert_slot_node_material_override(mode: String):
	_reset_fixture_state()
	var sprite_shader := Shader.new()
	sprite_shader.code = "shader_type canvas_item;\nvoid fragment() { COLOR = vec4(1.0, 0.0, 0.0, COLOR.a); }"
	var sprite_material := ShaderMaterial.new()
	sprite_material.shader = sprite_shader
	var slot_shader := Shader.new()
	slot_shader.code = "shader_type canvas_item;\nvoid fragment() { COLOR = vec4(0.0, 1.0, 0.0, COLOR.a); }"
	var slot_material := ShaderMaterial.new()
	slot_material.shader = slot_shader
	sprite.normal_material = sprite_material
	slot_node.normal_material = slot_material
	var image := await _render_image()
	_assert_color(mode + " SpineSlotNode material overrides sprite material", _sample(image, WHITE_X), Color(0, 1, 0, _light_color().a))
	slot_node.normal_material = null
	sprite.normal_material = null
	sprite_material.shader = null
	slot_material.shader = null

func _set_clipping(enabled: bool):
	var clipping = sprite.get_skeleton().get_attachment_by_slot_name("clipper", "clip") if enabled else null
	clipper_slot.get_pose().set_attachment(clipping)
	clipper_slot.get_applied_pose().set_attachment(clipping)

func _assert_blend_modes(mode: String):
	for blend in [["additive", SpineConstant.BlendMode_Additive], ["multiply", SpineConstant.BlendMode_Multiply]]:
		_reset_fixture_state()
		background.visible = true
		slot.get_data().set_blend_mode(blend[1])
		_set_slot_colors(SLOT_LIGHT, Color.WHITE, false)
		var without_dark := await _render_image()
		_set_slot_colors(SLOT_LIGHT, Color.WHITE, true)
		var with_dark := await _render_image()
		_assert_not_equal(mode + " " + blend[0] + " black band uses dark tint", _sample(without_dark, BLACK_X), _sample(with_dark, BLACK_X))
		_assert_not_equal(mode + " " + blend[0] + " gray band uses dark tint", _sample(without_dark, GRAY_X), _sample(with_dark, GRAY_X))
		background.visible = false
		slot.get_data().set_blend_mode(SpineConstant.BlendMode_Normal)

func _assert_two_color_bands(label: String, image: Image, light: Color, dark: Color):
	_assert_color(label + " black band", _sample(image, BLACK_X), _two_color(0.0, light, dark))
	_assert_color(label + " gray band", _sample(image, GRAY_X), _two_color(_texture_gray(), light, dark))
	_assert_color(label + " white band", _sample(image, WHITE_X), _two_color(1.0, light, dark))

func _assert_default_bands(label: String, image: Image, light: Color):
	_assert_color(label + " black band", _sample(image, BLACK_X), Color(0, 0, 0, light.a))
	_assert_color(label + " gray band", _sample(image, GRAY_X), Color(light.r * _texture_gray(), light.g * _texture_gray(), light.b * _texture_gray(), light.a))
	_assert_color(label + " white band", _sample(image, WHITE_X), light)

func _sample(image: Image, x: int) -> Color:
	var color := image.get_pixel(x, SAMPLE_Y)
	# Blending over a transparent SubViewport background stores premultiplied RGB.
	if color.a > 0.0:
		color.r /= color.a
		color.g /= color.a
		color.b /= color.a
	return color

func _light_color(light := SLOT_LIGHT) -> Color:
	return _to_render_color(_multiply(_multiply(SKELETON_COLOR, light), ATTACHMENT_COLOR))

func _dark_color(dark := SLOT_DARK) -> Color:
	return _to_render_color(_multiply_rgb(_multiply_rgb(SKELETON_COLOR, dark), ATTACHMENT_COLOR))

func _texture_gray() -> float:
	return Color(0.5, 0.5, 0.5, 1.0).srgb_to_linear().r if use_linear_colors else 0.5

func _to_render_color(color: Color) -> Color:
	return color.srgb_to_linear() if use_linear_colors else color

func _two_color(texture_value: float, light: Color, dark: Color) -> Color:
	return Color(
		texture_value * light.r + (1.0 - texture_value) * dark.r,
		texture_value * light.g + (1.0 - texture_value) * dark.g,
		texture_value * light.b + (1.0 - texture_value) * dark.b,
		light.a
	)

func _multiply(a: Color, b: Color) -> Color:
	return Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a)

func _multiply_rgb(a: Color, b: Color) -> Color:
	return Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a)

func _assert_color(label: String, actual: Color, expected: Color):
	var difference := maxf(maxf(absf(actual.r - expected.r), absf(actual.g - expected.g)), maxf(absf(actual.b - expected.b), absf(actual.a - expected.a)))
	if difference > TOLERANCE:
		_fail("%s: expected %s, got %s" % [label, expected, actual])

func _assert_not_equal(label: String, a: Color, b: Color):
	var difference := maxf(maxf(absf(a.r - b.r), absf(a.g - b.g)), maxf(absf(a.b - b.b), absf(a.a - b.a)))
	if difference <= TOLERANCE:
		_fail("%s: expected different pixels, both were %s" % [label, a])

func _expect_error(label: String, error: Error):
	if error != OK:
		_fail("%s failed: %s" % [label, error_string(error)])

func _fail(message: String):
	failures.append(message)

func _write_text(file_name: String, contents: String):
	var file := FileAccess.open(fixture_dir.path_join(file_name), FileAccess.WRITE)
	if file == null:
		_fail("could not write fixture file %s" % file_name)
		return
	file.store_string(contents)
	file.close()

func _render_image() -> Image:
	await process_frame
	await RenderingServer.frame_post_draw
	await process_frame
	await RenderingServer.frame_post_draw
	return viewport.get_texture().get_image()

func _cleanup():
	# Clear wrappers before their SpineSprite owner is destroyed.
	slot = null
	clipper_slot = null
	slot_node = null
	sprite = null
	background = null
	if viewport != null:
		viewport.free()
	viewport = null
	rendering_device = null
	for file_name in ["bands.png", "fixture.atlas", "fixture.spine-json"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir.path_join(file_name)))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir))
