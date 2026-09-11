@tool
extends EditorPlugin

const Fixture = preload("animation-player-fixture.gd")
var failed := false

func _enter_tree():
	call_deferred("_run")

func _check(condition: bool, message: String):
	if not condition:
		failed = true
		push_error(message)

func _settle():
	for i in 10:
		await get_tree().process_frame

func _run():
	for i in 60:
		await get_tree().process_frame
	var scene := EditorInterface.get_edited_scene_root()
	if scene == null:
		get_tree().quit(1)
		return
	var directory := "user://spine-editor-animation-player-%d" % OS.get_process_id()
	var data := Fixture.create_data(directory)
	var sprites: Array = [Fixture.create_sprite(scene, data, false), Fixture.create_sprite(scene, data, true)]
	await _settle()
	var master := Fixture.create_master(scene, sprites)
	EditorInterface.edit_node(master)
	await _settle()
	master.assigned_animation = "cutscene"
	for time in [0.75, 1.4, 0.25, 1.9]:
		master.seek(time, true)
		await _settle()
		for sprite in sprites:
			sprite.update_skeleton(0)
			var entry: SpineTrackEntry = sprite.get_animation_state().get_track(0)
			var expected_name := "walk" if time < 1 else "run"
			var expected_time: float = time if time < 1 else time - 1
			_check(entry != null, "editor creates native entry on " + sprite.name)
			if entry != null:
				_check(entry.get_animation().get_name() == expected_name and is_equal_approx(entry.get_track_time(), expected_time),
					"editor scrubs forward/back through nested root_node on " + sprite.name)
				var angle: float = 45 * expected_time + (0 if time < 1 else -90)
				_check(abs(sprite.get_skeleton().find_bone("root").get_applied_pose().get_rotation() - angle) < 0.001,
					"scrub applies the native pose on " + sprite.name)
		await _settle()

	# Selecting ordinary scene nodes and closing the dock must not start a scrubbed clip.
	EditorInterface.edit_node(scene)
	hide_bottom_panel()
	await _settle()
	for sprite in sprites:
		sprite.update_skeleton(0.5)
		_check(is_equal_approx(sprite.get_animation_state().get_track(0).get_track_time(), 0.9), "scene selection preserves the scrubbed time on " + sprite.name)
		_check(abs(sprite.get_skeleton().find_bone("root").get_applied_pose().get_rotation() + 49.5) < 0.001,
			"scene selection preserves the scrubbed pose on " + sprite.name)

	# An animation dock editing a different player must not steal preview time.
	var unrelated := AnimationPlayer.new()
	scene.add_child(unrelated)
	unrelated.owner = scene
	var library := AnimationLibrary.new()
	library.add_animation("unrelated", Animation.new())
	unrelated.add_animation_library("", library)
	EditorInterface.edit_node(unrelated)
	await _settle()
	for sprite in sprites:
		sprite.update_skeleton(0.5)
		_check(is_equal_approx(sprite.get_animation_state().get_track(0).get_track_time(), 0.9), "switching players keeps the old scrubbed entry frozen on " + sprite.name)
		sprite.get_animation_state().set_animation("walk", true, 0)
		sprite.update_skeleton(0.3)
		_check(is_equal_approx(sprite.get_animation_state().get_track(0).get_track_time(), 0.3), "inactive track leaves sprite preview alone on " + sprite.name)

	EditorInterface.edit_node(master)
	await _settle()
	master.assigned_animation = "cutscene"
	master.seek(0.5, true)
	await _settle()
	for sprite in sprites:
		sprite.update_skeleton(0)
		_check(is_equal_approx(sprite.get_animation_state().get_track(0).get_track_time(), 0.5), "switching back reacquires the editing player")
	# A different active editor driver must be able to resume these frozen entries.
	var tree := AnimationTree.new()
	tree.active = false
	tree.tree_root = AnimationNodeBlendTree.new()
	scene.add_child(tree)
	tree.owner = scene
	await _settle()
	EditorInterface.edit_node(tree)
	await _settle()
	for sprite in sprites:
		var track: SpineAnimationTrack = sprite.get_node("Track")
		track.blend_tree_mode = true
		sprite.update_skeleton(0.2)
		_check(is_equal_approx(sprite.get_animation_state().get_track(0).get_track_time(), 0.7), "AnimationTree editor handoff resumes the entry on " + sprite.name)
		track.blend_tree_mode = false
	EditorInterface.get_selection().clear()
	EditorInterface.edit_node(scene)
	tree.free()
	for sprite in sprites:
		sprite.free()
	master.get_parent().free()
	unrelated.free()
	Fixture.cleanup(directory)
	print("Spine AnimationPlayer editor regression ", "FAILED." if failed else "passed.")
	get_tree().quit(1 if failed else 0)
