extends SceneTree

# Standalone CPU-submission microbenchmark, also runnable on the pre-batching
# Stage2 binary. This is not a GPU frame-time or end-to-end FPS benchmark.
func _init():
	call_deferred("_run")

func _run():
	if DisplayServer.get_name() == "headless":
		push_error("Use a GPU renderer for the performance comparison.")
		quit(1)
		return
	var directory := "user://spine-performance-3d-%d" % OS.get_process_id()
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(directory))
	var image := Image.create(1, 1, false, Image.FORMAT_RGBA8)
	image.fill(Color.WHITE)
	assert(image.save_png(directory.path_join("pixel.png")) == OK)
	var atlas_file := FileAccess.open(directory.path_join("pixel.atlas"), FileAccess.WRITE)
	atlas_file.store_string("pixel.png\nsize: 1, 1\nfilter: Nearest, Nearest\npixel\nbounds: 0, 0, 1, 1\n")
	atlas_file.close()
	var slots: Array = []
	var attachments := {}
	for i in 128:
		var name := "slot%d" % i
		slots.append({"name": name, "bone": "root", "attachment": "pixel"})
		attachments[name] = {"pixel": {"type": "region", "path": "pixel", "width": 0.4, "height": 0.4,
			"x": (i % 16) - 8, "y": (i / 16) - 4}}
	var json_file := FileAccess.open(directory.path_join("skeleton.spine-json"), FileAccess.WRITE)
	json_file.store_string(JSON.stringify({"skeleton": {"spine": "4.3.75"}, "bones": [{"name": "root"}], "slots": slots,
		"skins": [{"name": "default", "attachments": attachments}]}))
	json_file.close()
	var atlas := SpineAtlasResource.new()
	assert(atlas.load_from_atlas_file(directory.path_join("pixel.atlas")) == OK)
	var file := SpineSkeletonFileResource.new()
	assert(file.load_from_file(directory.path_join("skeleton.spine-json")) == OK)
	var data := SpineSkeletonDataResource.new()
	data.atlas_res = atlas
	data.skeleton_file_res = file
	var viewport := SubViewport.new()
	viewport.size = Vector2i(96, 48)
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var camera := Camera3D.new()
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.size = 48
	camera.position.z = 10
	viewport.add_child(camera)
	camera.current = true
	var sprite := SpineSprite3D.new()
	sprite.update_mode = SpineConstant.UpdateMode_Manual
	sprite.pixel_size = 1.0
	sprite.slot_depth_offset = 0.0
	# Keep the pre-batching comparison's original render state on newer builds.
	if sprite.has_method("set_depth_write_enabled"):
		sprite.call("set_depth_write_enabled", false)
		sprite.call("set_camera_relative_depth", false)
		sprite.call("set_alpha_cutoff", 0)
	viewport.add_child(sprite)
	sprite.skeleton_data_res = data
	for i in 30:
		sprite.update_skeleton(1.0 / 60.0)
	await process_frame
	RenderingServer.force_draw(false, 0.0)
	RenderingServer.force_sync()
	var samples: Array[int] = []
	for i in 1000:
		var start := Time.get_ticks_usec()
		sprite.update_skeleton(1.0 / 60.0)
		samples.append(Time.get_ticks_usec() - start)
	samples.sort()
	RenderingServer.force_draw(false, 0.0)
	RenderingServer.force_sync()
	print("PERFORMANCE_COMPARISON ", JSON.stringify({"case": "128-compatible-quads", "samples": 1000,
		"median_cpu_submission_us": samples[500], "p95_cpu_submission_us": samples[950],
		"viewport_draw_counter": viewport.get_render_info(Viewport.RENDER_INFO_TYPE_VISIBLE, Viewport.RENDER_INFO_DRAW_CALLS_IN_FRAME),
		"renderer": RenderingServer.get_current_rendering_method(), "engine": Engine.get_version_info().string}))
	viewport.free()
	for name in ["pixel.png", "pixel.atlas", "skeleton.spine-json"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(directory.path_join(name)))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(directory))
	quit(0)
