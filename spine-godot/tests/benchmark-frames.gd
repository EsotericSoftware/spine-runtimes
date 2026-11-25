extends SceneTree

var drawn_frames := 0

func _init():
	call_deferred("_run")

func _run():
	var options := {"count": 96, "dimension": 3, "settle": 3.0, "seconds": 5.0, "vsync": false}
	for argument in OS.get_cmdline_user_args():
		var parts := argument.trim_prefix("--").split("=", true, 1)
		if parts.size() != 2 or not options.has(parts[0]):
			continue
		options[parts[0]] = parts[1] == "true" if parts[0] == "vsync" else float(parts[1])
	var original_low_processor_mode := OS.is_in_low_processor_usage_mode()
	var original_max_fps := Engine.max_fps
	var scene: PackedScene = load("res://examples/17-interactive-benchmark/benchmark.tscn")
	var benchmark = scene.instantiate()
	benchmark.initial_count = int(options.count)
	benchmark.initial_mode = 1 if int(options.dimension) == 3 else 0
	root.add_child(benchmark)
	benchmark.warmup_time.value = options.settle
	benchmark.sample_time.value = options.seconds
	benchmark.vsync.button_pressed = options.vsync
	benchmark.reset_samples()
	assert(not OS.is_in_low_processor_usage_mode() and Engine.max_fps == 0, "benchmark disables application throttling")
	RenderingServer.frame_post_draw.connect(func(): drawn_frames += 1)
	var start := Time.get_ticks_usec()
	while not benchmark.timing.completed:
		await process_frame
		if Time.get_ticks_usec() - start > 60000000:
			push_error("Frame capture did not complete within 60 seconds.")
			quit(1)
			return
	var result: Dictionary = benchmark.timing.result
	assert(result.frames > 0 and result.seconds >= options.seconds)
	assert(drawn_frames >= result.frames - 2, "frame capture must be backed by actual renderer draws")
	assert(result.frame_ms.mean > 0 and result.frame_ms.p95 > 0)
	assert(result.cpu_update_ms.mean >= 0)
	benchmark.free()
	assert(OS.is_in_low_processor_usage_mode() == original_low_processor_mode and Engine.max_fps == original_max_fps,
		"benchmark restores application pacing settings")
	await process_frame
	print("Spine settled frame timing regression passed.")
	quit()
