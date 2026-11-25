@tool
extends EditorPlugin

# Run only in an isolated editor project; this drives the real 3D editor's
# gui_input selection path rather than testing a separate ray implementation.
var failed := false
var surface: Control
var camera: Camera3D
var sprite: SpineSprite3D
var directory := "user://spine-editor-picking-%d" % OS.get_process_id()

func _enter_tree():
	call_deferred("_run")

func _check(condition: bool, message: String):
	if not condition:
		failed = true
		push_error(message)

func _settle():
	for i in 5:
		await get_tree().process_frame

func _button(position: Vector2, pressed: bool):
	var event := InputEventMouseButton.new()
	event.button_index = MOUSE_BUTTON_LEFT
	event.pressed = pressed
	event.position = position
	event.global_position = surface.get_global_rect().position + position
	surface.gui_input.emit(event)

func _click(point: Vector3, expected: bool, label: String):
	EditorInterface.get_selection().clear()
	await _settle()
	var position := camera.unproject_position(sprite.to_global(point))
	_button(position, true)
	_button(position, false)
	await _settle()
	_check(EditorInterface.get_selection().get_selected_nodes().has(sprite) == expected, label)

func _box(minimum: Vector3, maximum: Vector3, expected: bool, label: String):
	EditorInterface.get_selection().clear()
	await _settle()
	var rectangle := Rect2(camera.unproject_position(sprite.to_global(minimum)), Vector2.ZERO)
	for point in [maximum, Vector3(minimum.x, maximum.y, 0), Vector3(maximum.x, minimum.y, 0)]:
		rectangle = rectangle.expand(camera.unproject_position(sprite.to_global(point)))
	rectangle = rectangle.grow(15)
	_button(rectangle.position, true)
	var motion := InputEventMouseMotion.new()
	motion.button_mask = MOUSE_BUTTON_MASK_LEFT
	motion.position = rectangle.end
	motion.relative = rectangle.size
	surface.gui_input.emit(motion)
	_button(rectangle.end, false)
	await _settle()
	_check(EditorInterface.get_selection().get_selected_nodes().has(sprite) == expected, label)

func _run():
	for i in 60:
		await get_tree().process_frame
	var scene := EditorInterface.get_edited_scene_root()
	if scene == null:
		push_error("Open an empty Node3D scene in the isolated picking test project.")
		get_tree().quit(1)
		return
	EditorInterface.set_main_screen_editor("3D")
	await _settle()
	var viewport := EditorInterface.get_editor_viewport_3d(0)
	camera = viewport.get_camera_3d()
	for child in viewport.get_parent().get_parent().get_children():
		if not child is Control:
			continue
		for connection in child.get_signal_connection_list("gui_input"):
			if connection.callable.get_object() == viewport.get_parent().get_parent():
				surface = child
	if surface == null:
		push_error("Cannot locate the 3D editor selection surface.")
		get_tree().quit(1)
		return

	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(directory))
	var image := Image.create(1, 1, false, Image.FORMAT_RGBA8)
	image.fill(Color.WHITE)
	_check(image.save_png(directory.path_join("pixel.png")) == OK, "save pixel")
	var atlas_file := FileAccess.open(directory.path_join("pixel.atlas"), FileAccess.WRITE)
	atlas_file.store_string("pixel.png\nsize: 1, 1\nfilter: Nearest, Nearest\npixel\nbounds: 0, 0, 1, 1\n")
	atlas_file.close()
	var json_file := FileAccess.open(directory.path_join("skeleton.spine-json"), FileAccess.WRITE)
	json_file.store_string(JSON.stringify({"skeleton": {"spine": "4.3.75"}, "bones": [{"name": "root"}],
		"slots": [{"name": "slot", "bone": "root", "attachment": "region"}, {"name": "spare", "bone": "root"}],
		"skins": [{"name": "default", "attachments": {
			"slot": {"region": {"type": "region", "path": "pixel", "width": 2, "height": 2}},
			"spare": {"region": {"type": "region", "path": "pixel", "width": 2, "height": 2, "x": 3}}
		}}]}))
	json_file.close()
	var atlas := SpineAtlasResource.new()
	_check(atlas.load_from_atlas_file(directory.path_join("pixel.atlas")) == OK, "load atlas")
	var file := SpineSkeletonFileResource.new()
	_check(file.load_from_file(directory.path_join("skeleton.spine-json")) == OK, "load skeleton")
	var data := SpineSkeletonDataResource.new()
	data.atlas_res = atlas
	data.skeleton_file_res = file
	sprite = SpineSprite3D.new()
	sprite.name = "PickableSpine"
	sprite.update_mode = SpineConstant.UpdateMode_Manual
	sprite.pixel_size = 1
	scene.add_child(sprite)
	sprite.owner = scene
	sprite.skeleton_data_res = data
	sprite.update_skeleton(0)
	EditorInterface.set_main_screen_editor("3D")
	await _settle()
	_check(not sprite.get_gizmos().is_empty(), "SpineSprite3D has an editor picking gizmo")
	await _click(Vector3.ZERO, true, "click selects Spine geometry")
	await _box(Vector3(-1, -1, 0), Vector3(1, 1, 0), true, "box selects Spine geometry")
	await _click(Vector3(3, 0, 0), false, "empty area is not selectable")
	sprite.get_skeleton().set_attachment("spare", "region")
	sprite.update_skeleton(0)
	await _click(Vector3(3, 0, 0), true, "added attachment updates picking triangles")
	sprite.get_skeleton().set_attachment("spare", "")
	sprite.update_skeleton(0)
	await _click(Vector3(3, 0, 0), false, "removed attachment and padded indices are not pickable")
	sprite.get_skeleton().find_bone("root").get_pose().set_x(3)
	sprite.update_skeleton(0)
	await _click(Vector3.ZERO, false, "old pose is no longer pickable")
	await _click(Vector3(3, 0, 0), true, "updated bone pose is pickable")
	await _box(Vector3(2, -1, 0), Vector3(4, 1, 0), true, "box follows the updated pose")
	sprite.pixel_size = 0.5
	await _click(Vector3(1.5, 0, 0), true, "pixel size refreshes picking geometry")
	sprite.hide()
	await _click(Vector3(1.5, 0, 0), false, "hidden geometry is not pickable")
	sprite.show()
	await _click(Vector3(1.5, 0, 0), true, "shown geometry is pickable again")
	sprite.skeleton_data_res = null
	await _click(Vector3(1.5, 0, 0), false, "clearing the resource clears picking geometry")
	sprite.skeleton_data_res = data
	sprite.update_skeleton(0)
	await _click(Vector3.ZERO, true, "resource replacement rebuilds picking geometry")
	EditorInterface.get_selection().clear()
	sprite.free()
	for name in ["pixel.png", "pixel.atlas", "skeleton.spine-json"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(directory.path_join(name)))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(directory))
	print("Spine 3D editor picking regression ", "FAILED." if failed else "passed.")
	get_tree().quit(1 if failed else 0)
