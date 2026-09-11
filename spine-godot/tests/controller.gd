extends SceneTree

var fixture_dir: String
var failures: Array[String] = []
var sprites: Array[SpineSprite] = []

func _init():
	call_deferred("_run")

func _run():
	fixture_dir = "user://spine-godot-controller-regression-%d" % OS.get_process_id()
	var skeleton_data := _create_fixture()
	if skeleton_data == null:
		_finish()
		return

	await _test_public_api_and_shared_resource(skeleton_data)
	await _test_signal_arguments_and_order(skeleton_data)
	await _test_visibility_timing(skeleton_data)
	await _test_phase_callback_resource_changes(skeleton_data)
	await _test_animation_callback_resource_clear(skeleton_data)
	await _test_modified_bone_second_update(skeleton_data)
	await _test_retained_wrappers_after_reload(skeleton_data)
	await _test_sprite_destruction(skeleton_data)
	_finish()

func _test_public_api_and_shared_resource(skeleton_data: SpineSkeletonDataResource):
	var first := _new_sprite(skeleton_data)
	var second := _new_sprite(skeleton_data)
	await process_frame
	_check(first.get_skeleton() != null and second.get_skeleton() != null, "shared resource creates both skeleton instances")
	_check(first.get_skeleton() != second.get_skeleton(), "shared resource does not share instance skeleton wrappers")
	var first_bone := first.get_skeleton().find_bone("child")
	var second_bone := second.get_skeleton().find_bone("child")
	_check(first_bone != null and second_bone != null, "shared resource bones are available")
	if first_bone != null:
		_check(not first_bone.has_method("get_global_transform"), "SpineBone.get_global_transform is removed")
		_check(not first_bone.has_method("set_global_transform"), "SpineBone.set_global_transform is removed")
	first.position = Vector2(23, 17)
	first.update_skeleton(0.0)
	var child_transform := first.get_global_bone_transform("child")
	_check(child_transform.origin.distance_to(Vector2(28, 17)) < 0.001, "sprite global bone transform includes the Node2D transform")

func _test_signal_arguments_and_order(skeleton_data: SpineSkeletonDataResource):
	var sprite := _new_sprite(skeleton_data)
	await process_frame
	var phases: Array[String] = []
	sprite.before_animation_state_update.connect(func(arg):
		_check(arg == sprite, "before_animation_state_update keeps SpineSprite argument")
		phases.append("update")
	)
	sprite.before_animation_state_apply.connect(func(arg):
		_check(arg == sprite, "before_animation_state_apply keeps SpineSprite argument")
		phases.append("apply")
	)
	sprite.before_world_transforms_change.connect(func(arg):
		_check(arg == sprite, "before_world_transforms_change keeps SpineSprite argument")
		phases.append("before_world")
	)
	sprite.world_transforms_changed.connect(func(arg):
		_check(arg == sprite, "world_transforms_changed keeps SpineSprite argument")
		phases.append("world")
	)
	var started := {"args": []}
	sprite.animation_started.connect(func(sprite_arg, state_arg, entry_arg):
		started.args = [sprite_arg, state_arg, entry_arg]
	)
	var state := sprite.get_animation_state()
	var entry := state.set_animation("idle", true, 0)
	_check(started.args.size() == 3, "animation_started is forwarded synchronously")
	if started.args.size() == 3:
		_check(started.args[0] == sprite, "animation_started keeps SpineSprite argument")
		_check(started.args[1] == state, "animation_started keeps SpineAnimationState argument")
		_check(started.args[2] is SpineTrackEntry and started.args[2].get_track_index() == entry.get_track_index(),
			"animation_started keeps SpineTrackEntry argument type and native entry")
	sprite.update_skeleton(0.1)
	_check(phases == ["update", "apply", "before_world", "world"], "controller preserves update/apply/world signal ordering")

func _test_visibility_timing(skeleton_data: SpineSkeletonDataResource):
	var hide_sprite := _new_sprite(skeleton_data)
	await process_frame
	var hidden_phases: Array[String] = []
	hide_sprite.before_animation_state_update.connect(func(sprite_arg):
		hidden_phases.append("update")
		sprite_arg.visible = false
	, Object.CONNECT_ONE_SHOT)
	hide_sprite.before_animation_state_apply.connect(func(_sprite_arg): hidden_phases.append("apply"))
	hide_sprite.update_skeleton(0.1)
	_check(hidden_phases == ["update"], "visibility is checked after animation-state update callbacks")

	var show_sprite := _new_sprite(skeleton_data)
	await process_frame
	show_sprite.visible = false
	var shown_phases: Array[String] = []
	show_sprite.before_animation_state_update.connect(func(sprite_arg):
		shown_phases.append("update")
		sprite_arg.visible = true
	, Object.CONNECT_ONE_SHOT)
	show_sprite.before_animation_state_apply.connect(func(_sprite_arg): shown_phases.append("apply"))
	show_sprite.before_world_transforms_change.connect(func(_sprite_arg): shown_phases.append("before_world"))
	show_sprite.world_transforms_changed.connect(func(_sprite_arg): shown_phases.append("world"))
	show_sprite.update_skeleton(0.1)
	_check(shown_phases == ["update", "apply", "before_world", "world"], "showing in the update callback applies the pose in the same frame")

	var time_sprite := _new_sprite(skeleton_data)
	await process_frame
	var entry := time_sprite.get_animation_state().set_animation("idle", true, 0)
	time_sprite.set_time_scale(2.0)
	time_sprite.visible = false
	var old_track_time := entry.get_track_time()
	time_sprite.update_skeleton(0.25)
	_check(absf(entry.get_track_time() - old_track_time - 0.5) < 0.001, "animation time advances with time scale while invisible")

func _test_phase_callback_resource_changes(skeleton_data: SpineSkeletonDataResource):
	for phase in ["before_animation_state_update", "before_animation_state_apply", "before_world_transforms_change", "world_transforms_changed"]:
		var clear_sprite := _new_sprite(skeleton_data)
		await process_frame
		var old_bone := clear_sprite.get_skeleton().find_bone("child")
		clear_sprite.connect(phase, func(sprite_arg): sprite_arg.skeleton_data_res = null, Object.CONNECT_ONE_SHOT)
		clear_sprite.update_skeleton(0.1)
		_check(clear_sprite.get_skeleton() == null, "%s can clear the resource safely" % phase)
		_check(old_bone != null, "%s retained wrapper survives as a Godot object" % phase)

		var replacement := _create_skeleton_data()
		var replace_sprite := _new_sprite(skeleton_data)
		await process_frame
		var old_skeleton := replace_sprite.get_skeleton()
		var observed := {"resource_during_callback": null}
		replace_sprite.connect(phase, func(sprite_arg):
			sprite_arg.skeleton_data_res = replacement
			observed.resource_during_callback = sprite_arg.skeleton_data_res
		, Object.CONNECT_ONE_SHOT)
		replace_sprite.update_skeleton(0.1)
		_check(observed.resource_during_callback == skeleton_data, "%s keeps the public resource getter paired with active native state during deferral" % phase)
		_check(replace_sprite.skeleton_data_res == replacement, "%s installs the replacement resource" % phase)
		_check(replace_sprite.get_skeleton() != null and replace_sprite.get_skeleton() != old_skeleton, "%s replaces the instance skeleton safely" % phase)

func _test_animation_callback_resource_clear(skeleton_data: SpineSkeletonDataResource):
	var sprite := _new_sprite(skeleton_data)
	await process_frame
	var state := sprite.get_animation_state()
	sprite.animation_started.connect(func(sprite_arg, _state_arg, entry_arg):
		entry_arg.set_track_time(12.0)
		sprite_arg.skeleton_data_res = null
	, Object.CONNECT_ONE_SHOT)
	var returned_track := state.set_animation("idle", true, 0)
	_check(sprite.get_skeleton() == null and sprite.get_animation_state() == null, "animation callbacks can clear resources without deleting an active native call")
	_check(returned_track.get_track_time() == 0.0, "track returned across an animation-callback reset is inert, not dangling")

func _test_modified_bone_second_update(skeleton_data: SpineSkeletonDataResource):
	var sprite := _new_sprite(skeleton_data)
	await process_frame
	sprite.world_transforms_changed.connect(func(sprite_arg):
		var root_transform: Transform2D = sprite_arg.get_global_bone_transform("root")
		root_transform.origin += Vector2(10, 7)
		sprite_arg.set_global_bone_transform("root", root_transform)
	, Object.CONNECT_ONE_SHOT)
	sprite.update_skeleton(0.0)
	var root_transform: Transform2D = sprite.get_global_bone_transform("root")
	var child_transform: Transform2D = sprite.get_global_bone_transform("child")
	_check((child_transform.origin - root_transform.origin).distance_to(Vector2(5, 0)) < 0.001, "modified bones trigger the second world-transform update")

func _test_retained_wrappers_after_reload(skeleton_data: SpineSkeletonDataResource):
	var sprite := _new_sprite(skeleton_data)
	await process_frame
	var retained_skeleton := sprite.get_skeleton()
	var retained_state := sprite.get_animation_state()
	var retained_bone := retained_skeleton.find_bone("child")
	var retained_track := retained_state.set_animation("idle", true, 0)
	retained_track.set_track_time(12.0)
	sprite.skeleton_data_res = _create_skeleton_data()
	_check(retained_skeleton.get_bones().is_empty(), "retained skeleton wrapper is inert after resource replacement")
	_check(retained_state.get_num_tracks() == 0, "retained animation-state wrapper is inert after resource replacement")
	_check(retained_bone.get_transform() == Transform2D(), "retained bone wrapper is inert after resource replacement")
	_check(retained_track.get_track_time() == 0.0, "retained track wrapper is inert after resource replacement")

func _test_sprite_destruction(skeleton_data: SpineSkeletonDataResource):
	var sprite := _new_sprite(skeleton_data)
	await process_frame
	var retained_skeleton := sprite.get_skeleton()
	var retained_state := sprite.get_animation_state()
	var retained_bone := retained_skeleton.find_bone("child")
	var retained_track := retained_state.set_animation("idle", true, 0)
	retained_track.set_track_time(12.0)
	sprites.erase(sprite)
	sprite.free()
	_check(is_instance_valid(retained_bone), "retained wrappers remain valid Godot objects after sprite destruction")
	_check(retained_skeleton.get_bones().is_empty(), "retained skeleton wrapper is inert after sprite destruction")
	_check(retained_state.get_num_tracks() == 0, "retained animation-state wrapper is inert after sprite destruction")
	_check(retained_bone.get_transform() == Transform2D(), "retained bone wrapper is invalidated before native skeleton deletion")
	_check(retained_track.get_track_time() == 0.0, "retained track wrapper is inert after sprite destruction")

func _new_sprite(skeleton_data: SpineSkeletonDataResource) -> SpineSprite:
	var sprite := SpineSprite.new()
	sprite.update_mode = SpineConstant.UpdateMode_Manual
	sprite.skeleton_data_res = skeleton_data
	root.add_child(sprite)
	sprites.append(sprite)
	return sprite

func _create_fixture() -> SpineSkeletonDataResource:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(fixture_dir))
	var image := Image.create(1, 1, false, Image.FORMAT_RGBA8)
	image.set_pixel(0, 0, Color.WHITE)
	if image.save_png(fixture_dir.path_join("pixel.png")) != OK:
		_fail("could not save fixture texture")
		return null
	_write_text("fixture.atlas", """pixel.png
size: 1, 1
filter: Nearest, Nearest
pixel
bounds: 0, 0, 1, 1
""")
	_write_text("fixture.spine-json", """{
"skeleton": { "spine": "4.3.75" },
"bones": [ { "name": "root" }, { "name": "child", "parent": "root", "x": 5 } ],
"slots": [ { "name": "slot", "bone": "child", "attachment": "pixel" } ],
"skins": [ { "name": "default", "attachments": { "slot": { "pixel": { "type": "region", "path": "pixel", "width": 1, "height": 1 } } } } ],
"animations": { "idle": {} }
}""")
	return _create_skeleton_data()

func _create_skeleton_data() -> SpineSkeletonDataResource:
	var atlas := SpineAtlasResource.new()
	if atlas.load_from_atlas_file(fixture_dir.path_join("fixture.atlas")) != OK:
		_fail("could not load fixture atlas")
		return null
	var skeleton_file := SpineSkeletonFileResource.new()
	if skeleton_file.load_from_file(fixture_dir.path_join("fixture.spine-json")) != OK:
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
	for sprite in sprites:
		if is_instance_valid(sprite):
			sprite.free()
	sprites.clear()
	for file_name in ["pixel.png", "fixture.atlas", "fixture.spine-json"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir.path_join(file_name)))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir))
	if failures.is_empty():
		print("Spine controller regression passed.")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		print("Spine controller regression failed with %d assertion(s)." % failures.size())
		quit(1)
