extends SceneTree

var fixture_dir: String
var failures: Array[String] = []
var nodes: Array[Node] = []

func _init():
	call_deferred("_run")

func _run():
	_check(ClassDB.class_exists("SpineSprite3D"), "SpineSprite3D is registered")
	fixture_dir = "user://spine-godot-controller-3d-regression-%d" % OS.get_process_id()
	var skeleton_data := _create_fixture()
	if skeleton_data == null:
		_finish()
		return
	await _test_mixed_shared_resource(skeleton_data)
	await _test_signals_and_visibility(skeleton_data)
	await _test_callback_resource_replacement(skeleton_data)
	await _test_retained_wrappers(skeleton_data)
	await _test_shared_resource_reload(skeleton_data)
	await _test_slot_helper_lifecycle(skeleton_data)
	await _test_priority_limits()
	_finish()

func _test_mixed_shared_resource(skeleton_data: SpineSkeletonDataResource):
	var sprite_2d := SpineSprite.new()
	sprite_2d.update_mode = SpineConstant.UpdateMode_Manual
	sprite_2d.skeleton_data_res = skeleton_data
	root.add_child(sprite_2d)
	nodes.append(sprite_2d)
	var sprite_3d := _new_sprite_3d(skeleton_data)
	await process_frame
	_check(sprite_2d.get_skeleton() != null and sprite_3d.get_skeleton() != null, "mixed 2D and 3D sprites create skeletons")
	_check(sprite_2d.get_skeleton() != sprite_3d.get_skeleton(), "mixed sprites have independent controller state")
	var state_2d := sprite_2d.get_animation_state()
	var state_3d := sprite_3d.get_animation_state()
	var entry_2d := state_2d.set_animation("idle", true, 0)
	var entry_3d := state_3d.set_animation("idle", true, 0)
	sprite_2d.update_skeleton(0.25)
	sprite_3d.update_skeleton(0.5)
	_check(absf(entry_2d.get_track_time() - 0.25) < 0.001, "2D animation remains independent")
	_check(absf(entry_3d.get_track_time() - 0.5) < 0.001, "3D animation remains independent")
	_check(absf(sprite_3d.pixel_size - 0.01) < 0.000001, "3D pixel size has the documented default")
	sprite_3d.pixel_size = 0.02
	sprite_3d.slot_depth_offset = 0.001
	sprite_3d.cull_mode = SpineSprite3D.CULL_BACK
	_check(absf(sprite_3d.pixel_size - 0.02) < 0.000001 and absf(sprite_3d.slot_depth_offset - 0.001) < 0.000001,
		"3D rendering properties round trip")

func _test_signals_and_visibility(skeleton_data: SpineSkeletonDataResource):
	var sprite := _new_sprite_3d(skeleton_data)
	await process_frame
	var phases: Array[String] = []
	sprite.before_animation_state_update.connect(func(arg):
		_check(arg == sprite and arg is SpineSprite3D, "3D update signal keeps SpineSprite3D argument")
		phases.append("update")
	)
	sprite.before_animation_state_apply.connect(func(_arg): phases.append("apply"))
	sprite.before_world_transforms_change.connect(func(_arg): phases.append("before_world"))
	sprite.world_transforms_changed.connect(func(_arg): phases.append("world"))
	var started := {"called": false}
	sprite.animation_started.connect(func(sprite_arg, state_arg, entry_arg):
		started.called = sprite_arg == sprite and state_arg == sprite.get_animation_state() and entry_arg is SpineTrackEntry
	)
	var hidden_entry := sprite.get_animation_state().set_animation("idle", true, 0)
	_check(started.called, "3D animation_started is synchronous and typed")
	sprite.update_skeleton(0.1)
	_check(phases == ["update", "apply", "before_world", "world"], "3D controller signal phases preserve ordering")

	sprite.time_scale = 2.0
	sprite.visible = false
	var old_time: float = hidden_entry.get_track_time()
	sprite.update_skeleton(0.25)
	_check(absf(hidden_entry.get_track_time() - old_time - 0.5) < 0.001, "3D animation time advances while invisible")

func _test_callback_resource_replacement(skeleton_data: SpineSkeletonDataResource):
	var sprite := _new_sprite_3d(skeleton_data)
	await process_frame
	var replacement := _create_skeleton_data()
	var old_skeleton := sprite.get_skeleton()
	var observed := {"resource": null}
	sprite.before_animation_state_apply.connect(func(sprite_arg):
		sprite_arg.skeleton_data_res = replacement
		observed.resource = sprite_arg.skeleton_data_res
	, Object.CONNECT_ONE_SHOT)
	sprite.update_skeleton(0.1)
	_check(observed.resource == skeleton_data, "3D deferred replacement reports the controller-active resource")
	_check(sprite.skeleton_data_res == replacement and sprite.get_skeleton() != old_skeleton, "3D callback safely installs replacement state")

	var state := sprite.get_animation_state()
	sprite.animation_started.connect(func(sprite_arg, _state_arg, entry_arg):
		entry_arg.set_track_time(12.0)
		sprite_arg.skeleton_data_res = null
	, Object.CONNECT_ONE_SHOT)
	var returned_track := state.set_animation("idle", true, 0)
	_check(sprite.get_skeleton() == null and sprite.get_animation_state() == null, "3D animation callback can clear native state")
	_check(returned_track.get_track_time() == 0.0, "3D callback-returned track is inert after reset")

func _test_retained_wrappers(skeleton_data: SpineSkeletonDataResource):
	var sprite := _new_sprite_3d(skeleton_data)
	await process_frame
	var retained_skeleton := sprite.get_skeleton()
	var retained_state := sprite.get_animation_state()
	var retained_bone := retained_skeleton.find_bone("child")
	var retained_track := retained_state.set_animation("idle", true, 0)
	retained_track.set_track_time(12.0)
	nodes.erase(sprite)
	sprite.free()
	_check(retained_skeleton.get_bones().is_empty(), "3D retained skeleton is inert after node destruction")
	_check(retained_state.get_num_tracks() == 0, "3D retained animation state is inert after node destruction")
	_check(retained_bone.get_transform() == Transform2D(), "3D retained bone is inert after node destruction")
	_check(retained_track.get_track_time() == 0.0, "3D retained track is inert after node destruction")

func _test_shared_resource_reload(skeleton_data: SpineSkeletonDataResource):
	var first := _new_sprite_3d(skeleton_data)
	var second := _new_sprite_3d(skeleton_data)
	var old_first := first.get_skeleton()
	var old_second := second.get_skeleton()
	skeleton_data.emit_signal("skeleton_data_changed")
	_check(first.get_skeleton() != old_first and second.get_skeleton() != old_second, "resource reload rebuilds both independent controllers")
	_check(old_first.get_bones().is_empty() and old_second.get_bones().is_empty(), "shared-resource reload invalidates both retained skeletons")
	await process_frame
	var detached := second.get_skeleton()
	root.remove_child(second)
	root.add_child(second)
	second.update_skeleton(0)
	_check(second.get_skeleton() == detached, "3D world exit/re-entry preserves controller state")

func _test_slot_helper_lifecycle(skeleton_data: SpineSkeletonDataResource):
	var sprite := _new_sprite_3d(skeleton_data)
	var helper := SpineSlotNode3D.new()
	helper.slot_name = "slot"
	sprite.add_child(helper)
	var geometry := MeshInstance3D.new()
	geometry.mesh = QuadMesh.new()
	geometry.sorting_offset = 4.25
	geometry.sorting_use_aabb_center = true
	helper.add_child(geometry)
	await process_frame
	sprite.update_skeleton(0)
	_check(helper.get_slot_index() == 0 and helper.position.distance_to(Vector3(0.05, 0, 0)) < 0.00001,
		"slot helper resolves and follows native bone coordinates with pixel scaling")
	sprite.get_skeleton().find_bone("child").get_pose().set_x(17)
	sprite.update_skeleton(0)
	_check(helper.position.distance_to(Vector3(0.17, 0, 0)) < 0.00001, "slot helper follows subsequent bone pose changes")
	_check(not geometry.sorting_use_aabb_center, "slot helper owns explicit child sorting")
	sprite.skeleton_data_res = null
	_check(helper.get_slot_index() == -1 and geometry.sorting_offset == 4.25 and geometry.sorting_use_aabb_center,
		"resource clear releases helper sorting and native slot association")
	sprite.skeleton_data_res = skeleton_data
	sprite.update_skeleton(0)
	_check(helper.get_slot_index() == 0, "resource replacement reacquires the slot without stale native pointers")
	helper.slot_name = "missing"
	sprite.update_skeleton(0)
	_check(helper.get_slot_index() == -1 and not helper.get_sorting_warnings().is_empty(), "missing slots are diagnosed")
	_check(geometry.sorting_offset == 4.25 and geometry.sorting_use_aabb_center, "unresolved slot leaves ordinary child sorting intact")

func _test_priority_limits():
	for count in [0, 1, 255, 256, 257, 1024]:
		var slots: Array = []
		for i in count:
			slots.append({"name": "slot%d" % i, "bone": "root"})
		_write_text("limit.spine-json", JSON.stringify({
			"skeleton": {"spine": "4.3.75"}, "bones": [{"name": "root"}], "slots": slots
		}))
		var sprite := _new_sprite_3d(_create_skeleton_data("limit.spine-json"))
		_check(sprite.get_skeleton().get_slots().size() == count, "controller retains all %d slots without an artificial renderer cap" % count)
		sprite.render_priority = -128
		_check(sprite.render_priority == -128, "character priority lower bound is independent of %d slots" % count)
		sprite.render_priority = 127
		_check(sprite.render_priority == 127, "character priority upper bound is independent of %d slots" % count)
		sprite.update_skeleton(0)
		await process_frame

func _new_sprite_3d(skeleton_data: SpineSkeletonDataResource) -> SpineSprite3D:
	var sprite := SpineSprite3D.new()
	sprite.update_mode = SpineConstant.UpdateMode_Manual
	sprite.skeleton_data_res = skeleton_data
	root.add_child(sprite)
	nodes.append(sprite)
	return sprite

func _create_fixture() -> SpineSkeletonDataResource:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(fixture_dir))
	var image := Image.create(2, 2, false, Image.FORMAT_RGBA8)
	image.fill(Color.WHITE)
	if image.save_png(fixture_dir.path_join("pixel.png")) != OK:
		_fail("could not save fixture texture")
		return null
	_write_text("fixture.atlas", """pixel.png
size: 2, 2
filter: Nearest, Nearest
pixel
bounds: 0, 0, 2, 2
""")
	_write_text("fixture.spine-json", """{
"skeleton": { "spine": "4.3.75" },
"bones": [ { "name": "root" }, { "name": "child", "parent": "root", "x": 5 } ],
"slots": [ { "name": "slot", "bone": "child", "attachment": "pixel" } ],
"skins": [ { "name": "default", "attachments": { "slot": { "pixel": { "type": "region", "path": "pixel", "width": 2, "height": 2 } } } } ],
"animations": { "idle": {} }
}""")
	return _create_skeleton_data()

func _create_skeleton_data(file_name := "fixture.spine-json") -> SpineSkeletonDataResource:
	var atlas := SpineAtlasResource.new()
	if atlas.load_from_atlas_file(fixture_dir.path_join("fixture.atlas")) != OK:
		_fail("could not load fixture atlas")
		return null
	var skeleton_file := SpineSkeletonFileResource.new()
	if skeleton_file.load_from_file(fixture_dir.path_join(file_name)) != OK:
		_fail("could not load fixture skeleton")
		return null
	var skeleton_data := SpineSkeletonDataResource.new()
	skeleton_data.atlas_res = atlas
	skeleton_data.skeleton_file_res = skeleton_file
	if not skeleton_data.is_skeleton_data_loaded():
		_fail("fixture skeleton data was not loaded")
		return null
	return skeleton_data

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
	for node in nodes:
		if is_instance_valid(node):
			node.free()
	nodes.clear()
	for file_name in ["pixel.png", "fixture.atlas", "fixture.spine-json", "limit.spine-json"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir.path_join(file_name)))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir))
	if failures.is_empty():
		print("Spine 3D controller regression passed.")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		print("Spine 3D controller regression failed with %d assertion(s)." % failures.size())
		quit(1)
