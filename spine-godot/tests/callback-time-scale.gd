extends SceneTree

const Fixture = preload("animation-player-fixture.gd")
var failures: Array[String] = []

func _init():
	call_deferred("_run")

func _check(condition: bool, message: String):
	if not condition:
		failures.append(message)

func _sprite(data: SpineSkeletonDataResource, three_d: bool) -> Node:
	var sprite: Node = SpineSprite3D.new() if three_d else SpineSprite.new()
	sprite.update_mode = SpineConstant.UpdateMode_Manual
	sprite.skeleton_data_res = data
	root.add_child(sprite)
	return sprite

func _run():
	var directory := "user://spine-callback-time-scale-%d" % OS.get_process_id()
	var data := Fixture.create_data(directory)
	for three_d in [false, true]:
		for phase in ["before_animation_state_update", "before_animation_state_apply", "before_world_transforms_change", "world_transforms_changed"]:
			for value in [0.0, 2.0]:
				var sprite := _sprite(data, three_d)
				var entry = sprite.get_animation_state().set_animation("walk", true, 0)
				var start: float = sprite.get_skeleton().get_time()
				sprite.connect(phase, func(owner): owner.set_time_scale(value), CONNECT_ONE_SHOT)
				sprite.update_skeleton(0.25)
				var track_delta: float = 0.25 * value if phase == "before_animation_state_update" else 0.25
				var skeleton_delta: float = 0.25 if phase == "world_transforms_changed" else 0.25 * value
				_check(is_equal_approx(entry.get_track_time(), track_delta), "3D=%s %s scale=%s: current animation advance" % [three_d, phase, value])
				_check(is_equal_approx(sprite.get_skeleton().get_time() - start, skeleton_delta), "3D=%s %s scale=%s: current skeleton/physics advance" % [three_d, phase, value])
				sprite.free()
		var sprite := _sprite(data, three_d)
		var entry = sprite.get_animation_state().set_animation("walk", true, 0)
		var start: float = sprite.get_skeleton().get_time()
		sprite.before_animation_state_update.connect(func(owner): owner.set_time_scale(2), CONNECT_ONE_SHOT)
		sprite.before_world_transforms_change.connect(func(owner): owner.set_time_scale(3), CONNECT_ONE_SHOT)
		sprite.update_skeleton(0.25)
		_check(is_equal_approx(entry.get_track_time(), 0.5), "3D=%s: animation phase reads the first callback's scale" % three_d)
		_check(is_equal_approx(sprite.get_skeleton().get_time() - start, 0.75), "3D=%s: skeleton phase rereads the second callback's scale" % three_d)
		sprite.visible = false
		start = sprite.get_skeleton().get_time()
		var previous: float = entry.get_track_time()
		sprite.before_animation_state_update.connect(func(owner): owner.set_time_scale(2), CONNECT_ONE_SHOT)
		sprite.update_skeleton(0.25)
		_check(is_equal_approx(entry.get_track_time() - previous, 0.5), "3D=%s: hidden animation clock uses callback scale" % three_d)
		_check(is_equal_approx(sprite.get_skeleton().get_time(), start), "3D=%s: hidden skeleton clock remains paused" % three_d)
		sprite.free()
	data = null
	Fixture.cleanup(directory)
	for failure in failures:
		push_error(failure)
	print("Spine callback time-scale regression ", "passed." if failures.is_empty() else "FAILED.")
	quit(0 if failures.is_empty() else 1)
