extends SceneTree

var failed := false

func _init():
	call_deferred("_run")

func _check(condition: bool, message: String):
	if not condition:
		failed = true
		push_error(message)

func _settle():
	for i in 5:
		await process_frame
	RenderingServer.force_draw(false, 0)
	RenderingServer.force_sync()

func _run():
	var scene: PackedScene = load("res://examples/18-depth-offset-orbit/depth-offset-orbit.tscn")
	var orbit = scene.instantiate()
	root.add_child(orbit)
	_check(orbit.spineboy.camera_relative_depth and orbit.spineboy.depth_write_enabled and orbit.spineboy.normal_material == null,
		"orbit scene uses native depth defaults, not a prototype material")
	_check(FileAccess.get_file_as_string("res://examples/18-depth-offset-orbit/spine-depth.gdshaderinc").strip_edges() == SpineSprite3D.get_depth_shader_code().strip_edges(),
		"distributed custom-shader include matches the runtime helper")
	orbit.flip_depth.button_pressed = false
	orbit.auto_orbit.button_pressed = false
	orbit.pause_animation.button_pressed = true
	orbit.spineboy.get_animation_state().get_track(0).set_track_time(0.3)
	await _settle()
	var order: Array = orbit.spineboy.get_skeleton().get_draw_order()
	var snapshots := ""
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--snapshots="):
			snapshots = argument.trim_prefix("--snapshots=")
			DirAccess.make_dir_recursive_absolute(snapshots)
	for degrees in [0, 90, 180, 270, 360]:
		orbit.set_angle(degrees)
		await _settle()
		_check(is_equal_approx(orbit.spineboy.slot_depth_offset, 0.001), "camera orbit does not change depth spacing")
		var current: Array = orbit.spineboy.get_skeleton().get_draw_order()
		for i in order.size():
			_check(current[i].get_data().get_index() == order[i].get_data().get_index(), "camera orbit does not reverse Spine draw order")
		_check(orbit.spineboy.get_render_statistics().batches > 0, "native geometry remains active throughout the orbit")
		if not snapshots.is_empty() and degrees in [0, 180]:
			root.get_texture().get_image().save_png(snapshots.path_join("orbit-%d.png" % degrees))
	orbit.set_angle(0)
	await _settle()
	var front_fixed := root.get_texture().get_image()
	orbit.flip_depth.button_pressed = true
	await _settle()
	_check(_difference(front_fixed, root.get_texture().get_image()) < 10, "front-view pixels stay unchanged when enabling the flip")
	orbit.set_angle(180)
	orbit.flip_depth.button_pressed = false
	await _settle()
	var back_fixed := root.get_texture().get_image()
	orbit.flip_depth.button_pressed = true
	await _settle()
	var back_flipped := root.get_texture().get_image()
	_check(_difference(back_fixed, back_flipped) > 200, "camera-relative depth changes the back-view composite")
	if not snapshots.is_empty():
		back_flipped.save_png(snapshots.path_join("orbit-back-flipped.png"))
	orbit.face_ignores_flip.button_pressed = true
	await _settle()
	_check(_difference(back_flipped, root.get_texture().get_image()) > 50, "nonparticipating face material visibly breaks the corrected composite")
	if not snapshots.is_empty():
		root.get_texture().get_image().save_png(snapshots.path_join("orbit-face-opt-out.png"))
	orbit.face_ignores_flip.button_pressed = false
	orbit.set_angle(145)
	orbit.depth_gap.value = 0.03
	orbit.show_marker.button_pressed = true
	await _settle()
	var marker_fixed := root.get_texture().get_image()
	var physical_depth: float = orbit.marker_slot.position.z
	_check(is_zero_approx(orbit.marker.position.z), "ordinary slot child stays at its unflipped anchor")
	if not snapshots.is_empty():
		marker_fixed.save_png(snapshots.path_join("orbit-marker-unmatched.png"))
	orbit.marker_follows.button_pressed = true
	await _settle()
	var marker_position: Vector3 = orbit.spineboy.to_local(orbit.marker.global_position)
	_check(is_equal_approx(marker_position.z, -physical_depth) and orbit.marker.position == Vector3.ZERO,
		"native single-camera helper moves only the slot child's anchor")
	_check(_difference(marker_fixed, root.get_texture().get_image()) > 20, "slot child correction is visible in the exaggerated view")
	if not snapshots.is_empty():
		root.get_texture().get_image().save_png(snapshots.path_join("orbit-marker-matched.png"))
	orbit.depth_gap.value = 0.02
	_check(is_equal_approx(orbit.spineboy.slot_depth_offset, 0.02), "depth control changes the fixed physical stack")
	orbit.alpha_cutoff.value = 0.25
	_check(is_equal_approx(orbit.spineboy.alpha_cutoff, 0.25), "cutoff control uses the native property")
	var before: float = orbit.orbit_angle
	orbit.auto_orbit.button_pressed = true
	await _settle()
	_check(orbit.orbit_angle != before, "automatic orbit advances")
	orbit.free()
	await process_frame
	print("Spine depth-offset orbit scene regression ", "FAILED." if failed else "passed.")
	quit(1 if failed else 0)

func _difference(a: Image, b: Image) -> int:
	# Exclude both HUD panels; compare only the character/ground region.
	var changed := 0
	for y in range(int(a.get_height() * 0.34), int(a.get_height() * 0.76)):
		for x in a.get_width():
			var ca := a.get_pixel(x, y)
			var cb := b.get_pixel(x, y)
			if maxf(absf(ca.r - cb.r), maxf(absf(ca.g - cb.g), absf(ca.b - cb.b))) > 0.05:
				changed += 1
	return changed
