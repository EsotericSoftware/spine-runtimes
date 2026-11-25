extends SceneTree

const Fixture = preload("animation-player-fixture.gd")
var failed := false

func _init():
	call_deferred("_run")

func _check(condition: bool, message: String):
	if not condition:
		failed = true
		push_error(message)

func _run():
	var directory := "user://spine-animation-player-%d" % OS.get_process_id()
	var data := Fixture.create_data(directory)
	var scene := Node.new()
	root.add_child(scene)
	var sprites: Array = [Fixture.create_sprite(scene, data, false), Fixture.create_sprite(scene, data, true)]
	await process_frame
	for sprite in sprites:
		var track: SpineAnimationTrack = sprite.get_node("Track")
		var player: AnimationPlayer = track.get_child(0)
		player.callback_mode_process = AnimationMixer.ANIMATION_CALLBACK_MODE_PROCESS_MANUAL
		_check(track.track_index == 0 and player.has_animation("walk") and player.has_animation("walk_looped") and player.has_animation("RESET"),
			"generated animations on " + sprite.name)
		player.play("walk_looped")
		player.advance(0)
		sprite.update_skeleton(0.25)
		var entry: SpineTrackEntry = sprite.get_animation_state().get_track(0)
		_check(entry != null and entry.get_animation().get_name() == "walk" and entry.get_loop(), "looped playback on " + sprite.name)
		_check(is_equal_approx(entry.get_track_time(), 0.25), "playback advances native time on " + sprite.name)
		_check(is_equal_approx(sprite.get_skeleton().find_bone("root").get_applied_pose().get_rotation(), 11.25), "native pose is animated on " + sprite.name)
		sprite.update_skeleton(2.0)
		_check(is_equal_approx(entry.get_track_time(), 2.25) and is_equal_approx(entry.get_animation_time(), 0.25), "native looping on " + sprite.name)
		track.mix_duration = 0.2
		track.alpha = 0.8
		track.time_scale = 0.5
		track.reverse = true
		track.additive = true
		player.play("run")
		player.advance(0)
		sprite.update_skeleton(0.1)
		entry = sprite.get_animation_state().get_track(0)
		_check(entry.get_animation().get_name() == "run" and not entry.get_loop(), "animation switch on " + sprite.name)
		_check(is_equal_approx(entry.get_mix_duration(), 0.2) and is_equal_approx(entry.get_alpha(), 0.8) and
			is_equal_approx(entry.get_time_scale(), 0.5) and entry.get_reverse() and entry.get_additive(), "track options on " + sprite.name)
		track.mix_duration = 0
		track.time_scale = 1
		track.alpha = 1
		track.reverse = false
		track.additive = false
		player.play("-- Empty --")
		player.advance(0)
		sprite.update_skeleton(0)
		_check(sprite.get_animation_state().get_track(0).get_animation().get_name() == "<empty>", "empty animation on " + sprite.name)

	for three_d in [false, true]:
		var late: Node = Fixture.create_sprite(scene, null, three_d)
		_check(late.get_node("Track").get_child_count() == 0, "unloaded resource does not create clips")
		late.skeleton_data_res = data
		late.update_skeleton(0)
		_check(late.get_node("Track").get_child_count() == 1 and late.get_node("Track").track_index == 0, "late resource assignment initializes the track")
		late.free()

	var master := Fixture.create_master(scene, sprites)
	master.play("cutscene")
	master.advance(0)
	for sprite in sprites:
		sprite.get_node("Track").get_child(0).advance(0)
		sprite.update_skeleton(0.2)
		_check(sprite.get_animation_state().get_track(0).get_animation().get_name() == "walk", "nested AnimationPlayer starts " + sprite.name)
	master.advance(1.2)
	for sprite in sprites:
		sprite.get_node("Track").get_child(0).advance(0)
		sprite.update_skeleton(0.1)
		_check(sprite.get_animation_state().get_track(0).get_animation().get_name() == "run", "nested AnimationPlayer switches " + sprite.name)
	master.stop()

	# Bound callback cannot silently retarget/disconnect another sprite.
	var first_track: SpineAnimationTrack = sprites[0].get_node("Track")
	first_track.update_animation_state(sprites[1])
	sprites[0].remove_child(first_track)
	sprites[1].add_child(first_track)
	sprites[1].update_skeleton(0)
	first_track.get_child(0).play("walk")
	first_track.get_child(0).advance(0)
	first_track.update_animation_state(sprites[1])
	_check(sprites[1].get_animation_state().get_track(first_track.track_index).get_animation().get_name() == "walk", "track reparents from 2D to 3D")
	first_track.free()

	for sprite in sprites:
		# Late resource assignment and native resource replacement must rebuild clips.
		var track := SpineAnimationTrack.new()
		track.track_index = 2
		sprite.add_child(track)
		sprite.skeleton_data_res = null
		sprite.skeleton_data_res = data
		sprite.update_skeleton(0)
		var player: AnimationPlayer = track.get_child(0)
		_check(player.has_animation("run"), "resource replacement restores generated clips on " + sprite.name)
		player.play("run")
		player.advance(0)
		sprite.animation_started.connect(func(_sprite, _state, _entry): sprite.skeleton_data_res = null, CONNECT_ONE_SHOT)
		sprite.update_skeleton(0)
		_check(sprite.skeleton_data_res == null and sprite.get_skeleton() == null, "resource clear from track start callback on " + sprite.name)

	scene.free()
	Fixture.cleanup(directory)
	print("Spine AnimationPlayer regression ", "FAILED." if failed else "passed.")
	quit(1 if failed else 0)
