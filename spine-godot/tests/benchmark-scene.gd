extends SceneTree

var failed := false

func _init():
	call_deferred("_run")

func _check(condition: bool, message: String):
	if not condition:
		failed = true
		push_error(message)

func _settle():
	for i in 8:
		await process_frame
	RenderingServer.force_draw(false, 0.0)
	RenderingServer.force_sync()

func _run():
	var scene: PackedScene = load("res://examples/17-interactive-benchmark/benchmark.tscn")
	var benchmark = scene.instantiate()
	benchmark.initial_count = 50
	root.add_child(benchmark)
	await _settle()
	_check(benchmark.viewport.size.x > 0 and benchmark.viewport.size.y > 0, "benchmark viewport has drawable size")
	for dimension in [0, 1]:
		benchmark.mode.select(dimension)
		for amount in [50, 96, 1000]:
			benchmark.count.value = amount
			var start := Time.get_ticks_usec()
			benchmark.rebuild()
			await _settle()
			print("BENCH_SCENE_LOAD ", JSON.stringify({"mode": 2 + dimension, "characters": amount,
				"build_and_eight_frames_ms": (Time.get_ticks_usec() - start) / 1000.0}))
			_check(benchmark.sprites.size() == amount, "%d characters in %dD mode" % [amount, 2 + dimension])
			var values: Dictionary = benchmark.collect_statistics()
			_check(values.draws > 0 and values.primitives > 0, "benchmark viewport submits visible geometry")
			if dimension == 1:
				_check(values.batches >= amount, "native 3D batches are reported")
				_check(values.shared_shaders == 1, "one default shader variant is shared regardless of character count")
			benchmark.refresh_statistics()
			_check("CPU update loop" in benchmark.statistics.text, "HUD reports measured update timing")
			var entry: SpineTrackEntry = benchmark.sprites[0].get_animation_state().get_track(0)
			benchmark.pause_updates.button_pressed = true
			var paused_time := entry.get_track_time()
			await _settle()
			_check(is_equal_approx(entry.get_track_time(), paused_time), "pause updates freezes native animation time")
			benchmark.pause_updates.button_pressed = false
			await _settle()
			_check(entry.get_track_time() > paused_time, "unpausing resumes native updates")
		benchmark.animation.select(3)
		benchmark.restart_animation()
		await _settle()
		_check(benchmark.sprites[0].get_animation_state().get_track(0).get_animation().get_name() == "hoverboard", "animation selector changes workload")
		benchmark.animation.select(0)
		benchmark.restart_animation()
	benchmark.reset_samples()
	_check(benchmark.samples.is_empty(), "sampling window can be reset")
	benchmark.free()
	await process_frame
	print("Spine interactive benchmark regression ", "FAILED." if failed else "passed.")
	quit(1 if failed else 0)
