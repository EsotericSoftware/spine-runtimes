extends Control

const TimingCapture = preload("timing_capture.gd")

@export var skeleton_data_res: SpineSkeletonDataResource
@export_range(1, 1000) var initial_count := 96
@export_enum("2D", "3D") var initial_mode := 1

var sprites: Array[Node] = []
var mode: OptionButton
var presets: OptionButton
var count: SpinBox
var animation: OptionButton
var pause_updates: CheckButton
var speed: SpinBox
var stagger: CheckButton
var statistics: Label
var viewport: SubViewport
var camera: Camera3D
var population: Node
var timing = TimingCapture.new()
var samples: Array[float]:
	get:
		return timing.cpu_samples
var warmup_time: SpinBox
var sample_time: SpinBox
var vsync: CheckButton
var original_vsync := DisplayServer.VSYNC_ENABLED
var original_low_processor_mode := false
var original_max_fps := 0
var previous_cpu_ms := 0.0
var display_usec := 0
var render_snapshot: Dictionary = {}
var previous_size := Vector2i.ZERO
var rebuild_serial := 0

func _ready():
	# Example projects conserve CPU by default. A benchmark must not inherit
	# their artificial 6.9 ms pacing delay or an application FPS cap.
	original_low_processor_mode = OS.is_in_low_processor_usage_mode()
	original_max_fps = Engine.max_fps
	OS.low_processor_usage_mode = false
	Engine.max_fps = 0
	var margin := MarginContainer.new()
	margin.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	for side in ["left", "top", "right", "bottom"]:
		margin.add_theme_constant_override("margin_" + side, 12)
	add_child(margin)
	var layout := VBoxContainer.new()
	layout.add_theme_constant_override("separation", 8)
	margin.add_child(layout)
	var title := Label.new()
	title.text = "Spine interactive benchmark — native 2D / 3D"
	title.add_theme_font_size_override("font_size", 22)
	layout.add_child(title)
	var controls := HFlowContainer.new()
	layout.add_child(controls)
	mode = OptionButton.new()
	mode.add_item("2D — SpineSprite")
	mode.add_item("3D — SpineSprite3D")
	mode.select(initial_mode)
	controls.add_child(mode)
	presets = OptionButton.new()
	presets.add_item("Presets…", 0)
	for amount in [50, 96, 250, 500, 1000]:
		presets.add_item("%d Spineboys" % amount, amount)
	presets.select(maxi(0, presets.get_item_index(initial_count)))
	controls.add_child(presets)
	count = SpinBox.new()
	count.custom_minimum_size.x = 100
	count.min_value = 1
	count.max_value = 1000
	count.step = 1
	count.value = initial_count
	controls.add_child(count)
	var apply := Button.new()
	apply.text = "Apply count"
	controls.add_child(apply)
	animation = OptionButton.new()
	for name in ["walk", "run", "idle", "hoverboard"]:
		animation.add_item(name)
	controls.add_child(animation)
	speed = SpinBox.new()
	speed.custom_minimum_size.x = 125
	speed.prefix = "Speed"
	speed.min_value = 0
	speed.max_value = 4
	speed.step = 0.25
	speed.value = 1
	controls.add_child(speed)
	stagger = CheckButton.new()
	stagger.text = "Stagger phases"
	stagger.button_pressed = true
	controls.add_child(stagger)
	pause_updates = CheckButton.new()
	pause_updates.text = "Pause updates"
	controls.add_child(pause_updates)
	warmup_time = SpinBox.new()
	warmup_time.custom_minimum_size.x = 155
	warmup_time.prefix = "Settle s"
	warmup_time.min_value = 0
	warmup_time.max_value = 15
	warmup_time.step = 0.5
	warmup_time.value = 3
	controls.add_child(warmup_time)
	sample_time = SpinBox.new()
	sample_time.custom_minimum_size.x = 160
	sample_time.prefix = "Sample s"
	sample_time.min_value = 1
	sample_time.max_value = 30
	sample_time.step = 1
	sample_time.value = 5
	controls.add_child(sample_time)
	original_vsync = DisplayServer.window_get_vsync_mode()
	vsync = CheckButton.new()
	vsync.text = "VSync"
	vsync.button_pressed = original_vsync != DisplayServer.VSYNC_DISABLED
	controls.add_child(vsync)
	var reset := Button.new()
	reset.text = "Restart capture"
	controls.add_child(reset)
	statistics = Label.new()
	statistics.clip_text = true
	statistics.custom_minimum_size.y = 144
	layout.add_child(statistics)
	var note := Label.new()
	note.text = "Frame intervals use wall time (include renderer, UI, OS and VSync); they are not GPU timings. CPU = skeleton update loop.\nCapture excludes settling/build time. Mobile's 3D draw counter counts instances. No cross-character 3D batching."
	layout.add_child(note)
	var container := SubViewportContainer.new()
	container.size_flags_vertical = Control.SIZE_EXPAND_FILL
	container.stretch = true
	layout.add_child(container)
	viewport = SubViewport.new()
	viewport.own_world_3d = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	container.add_child(viewport)
	population = Node.new()
	population.name = "Population"
	viewport.add_child(population)
	camera = Camera3D.new()
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.position.z = 10
	viewport.add_child(camera)
	camera.current = true
	mode.item_selected.connect(func(_index): rebuild())
	presets.item_selected.connect(func(index):
		if index == 0:
			return
		count.value = presets.get_item_id(index)
		rebuild())
	count.value_changed.connect(func(value): presets.select(maxi(0, presets.get_item_index(int(value)))))
	apply.pressed.connect(rebuild)
	animation.item_selected.connect(func(_index): restart_animation())
	stagger.toggled.connect(func(_enabled): restart_animation())
	pause_updates.toggled.connect(func(_enabled): reset_samples())
	speed.value_changed.connect(func(_value): reset_samples())
	reset.pressed.connect(reset_samples)
	warmup_time.value_changed.connect(func(_value): reset_samples())
	sample_time.value_changed.connect(func(_value): reset_samples())
	vsync.toggled.connect(func(enabled):
		DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_ENABLED if enabled else DisplayServer.VSYNC_DISABLED)
		reset_samples())
	rebuild()

func _exit_tree():
	DisplayServer.window_set_vsync_mode(original_vsync)
	OS.low_processor_usage_mode = original_low_processor_mode
	Engine.max_fps = original_max_fps

func reset_samples():
	timing.reset(Time.get_ticks_usec(), warmup_time.value, sample_time.value)
	previous_cpu_ms = 0
	display_usec = 0
	render_snapshot.clear()

func rebuild():
	for sprite in sprites:
		sprite.free()
	sprites.clear()
	rebuild_serial += 1
	if skeleton_data_res == null or not skeleton_data_res.is_skeleton_data_loaded():
		statistics.text = "Assign a loaded Spineboy skeleton_data_res."
		return
	for i in int(count.value):
		var sprite: Node = SpineSprite3D.new() if mode.selected == 1 else SpineSprite.new()
		sprite.update_mode = SpineConstant.UpdateMode_Manual
		sprite.skeleton_data_res = skeleton_data_res
		population.add_child(sprite)
		sprites.append(sprite)
	restart_animation()
	layout_population()

func restart_animation():
	var name := animation.get_item_text(animation.selected)
	for i in sprites.size():
		var sprite: Node = sprites[i]
		var entry: SpineTrackEntry = sprite.get_animation_state().set_animation(name, true, 0)
		entry.set_mix_duration(0)
		if stagger.button_pressed:
			entry.set_track_time(fposmod(i * 0.61803398875, 1.0) * entry.get_animation().get_duration())
		sprite.update_skeleton(0)
	reset_samples()

func layout_population():
	previous_size = viewport.size
	if sprites.is_empty() or viewport.size.x < 1 or viewport.size.y < 1:
		return
	var area := Vector2(viewport.size)
	var columns := maxi(1, ceili(sqrt(sprites.size() * area.x / area.y)))
	var rows := ceili(float(sprites.size()) / columns)
	var cell := area / Vector2(columns, rows)
	var character_scale := minf(cell.x / 450.0, cell.y / 700.0) * 0.9
	camera.size = area.y * 0.01
	for i in sprites.size():
		var pixel := Vector2((i % columns + 0.5) * cell.x, (i / columns + 0.93) * cell.y)
		if mode.selected == 1:
			sprites[i].pixel_size = character_scale * 0.01
			sprites[i].position = Vector3((pixel.x - area.x * 0.5) * 0.01, (area.y * 0.5 - pixel.y) * 0.01, 0)
		else:
			sprites[i].scale = Vector2.ONE * character_scale
			sprites[i].position = pixel
	reset_samples()

func _process(delta: float):
	if viewport == null:
		return
	var now := Time.get_ticks_usec()
	if viewport.size != previous_size:
		layout_population()
	if timing.advance(now, previous_cpu_ms):
		render_snapshot = collect_statistics()
		var report: Dictionary = timing.result.duplicate(true)
		report.merge({"mode": "3D" if mode.selected == 1 else "2D", "characters": sprites.size(),
			"animation": animation.get_item_text(animation.selected), "speed": speed.value,
			"paused": pause_updates.button_pressed, "staggered": stagger.button_pressed,
			"renderer": RenderingServer.get_current_rendering_method(), "settle_seconds": warmup_time.value,
			"viewport_size": [viewport.size.x, viewport.size.y], "vsync": DisplayServer.window_get_vsync_mode(),
			"editor_binary": OS.has_feature("editor"), "debug_features": OS.is_debug_build(),
			"low_processor_mode": OS.is_in_low_processor_usage_mode(), "max_fps": Engine.max_fps,
			"engine": Engine.get_version_info().string, "render": render_snapshot})
		print("SPINE_FRAME_BENCH ", JSON.stringify(report))
		display_usec = 0
	previous_cpu_ms = 0
	if not pause_updates.button_pressed:
		var step: float = delta * speed.value
		var start := Time.get_ticks_usec()
		for sprite in sprites:
			sprite.update_skeleton(step)
		previous_cpu_ms = (Time.get_ticks_usec() - start) / 1000.0
	if now - display_usec >= 500000:
		display_usec = now
		refresh_statistics()

func collect_statistics() -> Dictionary:
	var result := {"characters": sprites.size(), "batches": 0, "materials": 0, "mesh_builds": 0,
		"material_builds": 0, "vertex_uploads": 0, "index_uploads": 0, "shared_shaders": 0}
	if mode.selected == 1:
		for sprite in sprites:
			var values: Dictionary = sprite.get_render_statistics()
			result.shared_shaders = values.shared_shaders
			for key in ["batches", "materials", "mesh_builds", "material_builds", "vertex_uploads", "index_uploads"]:
				result[key] += values[key]
	var category := Viewport.RENDER_INFO_TYPE_VISIBLE if mode.selected == 1 else Viewport.RENDER_INFO_TYPE_CANVAS
	result["draws"] = viewport.get_render_info(category, Viewport.RENDER_INFO_DRAW_CALLS_IN_FRAME)
	result["primitives"] = viewport.get_render_info(category, Viewport.RENDER_INFO_PRIMITIVES_IN_FRAME)
	return result

func refresh_statistics():
	# Avoid polling every sprite or sorting sample arrays during the capture.
	if timing.started_usec < 0 or render_snapshot.is_empty():
		render_snapshot = collect_statistics()
	var phase: String
	if timing.completed:
		phase = "Complete: %.2f s / %d frames / %.1f FPS — Restart capture to measure again" % [
			timing.result.seconds, timing.result.frames, timing.result.fps]
	elif timing.started_usec < 0:
		phase = "Settling: %.1f s remaining, then %.1f s capture" % [
			maxf(0, (timing.settle_until_usec - Time.get_ticks_usec()) / 1000000.0), sample_time.value]
	else:
		phase = "Sampling: %.1f / %.1f s (%d frames)" % [timing.sampled_seconds, sample_time.value, samples.size()]
	statistics.text = "%s | %d characters | %s | %s | VSync %s\n%s" % [
		"3D" if mode.selected == 1 else "2D", sprites.size(), RenderingServer.get_current_rendering_method(),
		"updates paused" if pause_updates.button_pressed else "animating", "on" if vsync.button_pressed else "off", phase]
	if timing.completed:
		for key in ["frame_ms", "cpu_update_ms"]:
			var values: Dictionary = timing.result[key]
			statistics.text += "\n%s: mean %.3f | median %.3f | p95 %.3f | worst %.3f ms" % [
				"Frame interval" if key == "frame_ms" else "CPU update loop", values.mean, values.median, values.p95, values.worst]
	else:
		statistics.text += "\nFrame interval: awaiting completed capture\nCPU update loop: awaiting completed capture"
	statistics.text += "\nViewport draw counter: %d   Primitives: %d" % [render_snapshot.draws, render_snapshot.primitives]
	if mode.selected == 1:
		statistics.text += "\nBatches/materials: %d/%d   Shared shaders: %d   Mesh/material builds: %d/%d   Vertex/index uploads: %d/%d" % [
			render_snapshot.batches, render_snapshot.materials, render_snapshot.shared_shaders,
			render_snapshot.mesh_builds, render_snapshot.material_builds, render_snapshot.vertex_uploads, render_snapshot.index_uploads]
	else:
		statistics.text += "\n2D batching is handled by Godot's canvas renderer; counters above describe this viewport."
