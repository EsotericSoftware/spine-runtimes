extends Node3D

@onready var spineboy: SpineSprite3D = $Spineboy
@onready var camera: Camera3D = $Camera3D
var auto_orbit: CheckButton
var pause_animation: CheckButton
var angle: HSlider
var depth_gap: SpinBox
var alpha_cutoff: SpinBox
var animation: OptionButton
var status: Label
var flip_depth: CheckButton
var show_marker: CheckButton
var marker_follows: CheckButton
var face_ignores_flip: CheckButton
var marker_slot: SpineSlotNode3D
var marker: MeshInstance3D
var face_slots: Array[SpineSlotNode3D] = []
var face_material: ShaderMaterial
var orbit_angle := 0.0
const ORBIT_CENTER := Vector3(0, 4.1, 0)
const ORBIT_RADIUS := 10.0

func _ready():
	var panel := PanelContainer.new()
	panel.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	$UI.add_child(panel)
	var margin := MarginContainer.new()
	for side in ["left", "top", "right", "bottom"]:
		margin.add_theme_constant_override("margin_" + side, 12)
	panel.add_child(margin)
	var layout := VBoxContainer.new()
	margin.add_child(layout)
	var title := Label.new()
	title.text = "Spineboy — depth policy orbit"
	title.add_theme_font_size_override("font_size", 22)
	layout.add_child(title)
	var controls := HFlowContainer.new()
	layout.add_child(controls)
	auto_orbit = CheckButton.new()
	auto_orbit.text = "Auto orbit"
	auto_orbit.button_pressed = true
	controls.add_child(auto_orbit)
	pause_animation = CheckButton.new()
	pause_animation.text = "Pause animation"
	controls.add_child(pause_animation)
	animation = OptionButton.new()
	for name in ["walk", "run", "idle", "hoverboard"]:
		animation.add_item(name)
	controls.add_child(animation)
	for label in ["Front", "Side", "Back"]:
		var button := Button.new()
		button.text = label
		button.pressed.connect(func():
			auto_orbit.button_pressed = false
			set_angle(0 if label == "Front" else 90 if label == "Side" else 180))
		controls.add_child(button)
	depth_gap = SpinBox.new()
	depth_gap.custom_minimum_size.x = 195
	depth_gap.prefix = "Depth gap"
	depth_gap.min_value = 0
	depth_gap.max_value = 0.1
	depth_gap.step = 0.001
	depth_gap.value = spineboy.slot_depth_offset
	controls.add_child(depth_gap)
	alpha_cutoff = SpinBox.new()
	alpha_cutoff.custom_minimum_size.x = 180
	alpha_cutoff.prefix = "Alpha cutoff"
	alpha_cutoff.min_value = 0
	alpha_cutoff.max_value = 1
	alpha_cutoff.step = 0.001
	alpha_cutoff.value = spineboy.alpha_cutoff
	controls.add_child(alpha_cutoff)
	angle = HSlider.new()
	angle.min_value = 0
	angle.max_value = 360
	angle.step = 0.1
	layout.add_child(angle)
	status = Label.new()
	layout.add_child(status)
	var note := Label.new()
	note.text = "Depth writes + alpha discard, both faces visible. Compare the camera-relative toggle below.\nSpine draw order stays unchanged. Bounds cover both depth directions; editor picking is conservative."
	layout.add_child(note)
	angle.value_changed.connect(func(value):
		auto_orbit.button_pressed = false
		set_angle(value))
	depth_gap.value_changed.connect(func(value):
		spineboy.slot_depth_offset = value
		update_status())
	alpha_cutoff.value_changed.connect(func(_value): update_materials())
	pause_animation.toggled.connect(func(paused): spineboy.time_scale = 0.0 if paused else 1.0)
	animation.item_selected.connect(func(index): spineboy.get_animation_state().set_animation(animation.get_item_text(index), true, 0))
	create_diagnostics()
	spineboy.get_animation_state().set_animation("walk", true, 0)
	set_angle(0)

func _process(delta: float):
	if auto_orbit != null and auto_orbit.button_pressed:
		set_angle(fposmod(orbit_angle + delta * 18.0, 360.0))

func set_angle(degrees: float):
	orbit_angle = degrees
	var radians := deg_to_rad(degrees)
	camera.position = ORBIT_CENTER + Vector3(sin(radians) * ORBIT_RADIUS, 0.8, cos(radians) * ORBIT_RADIUS)
	camera.look_at(ORBIT_CENTER)
	if angle != null:
		angle.set_value_no_signal(degrees)
	update_status()

func update_status():
	if status == null:
		return
	var facing := cos(deg_to_rad(orbit_angle))
	var side := "front" if facing > 0.05 else "back" if facing < -0.05 else "edge-on"
	status.text = "Camera %.1f° (%s) | depth gap %.3f world units | alpha cutoff %.3f" % [
		orbit_angle, side, spineboy.slot_depth_offset, alpha_cutoff.value]

func create_diagnostics():
	var panel := PanelContainer.new()
	$UI.add_child(panel)
	panel.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	panel.grow_vertical = Control.GROW_DIRECTION_BEGIN
	var margin := MarginContainer.new()
	for side in ["left", "top", "right", "bottom"]:
		margin.add_theme_constant_override("margin_" + side, 10)
	panel.add_child(margin)
	var layout := VBoxContainer.new()
	margin.add_child(layout)
	var controls := HFlowContainer.new()
	layout.add_child(controls)
	flip_depth = CheckButton.new()
	flip_depth.text = "Camera-relative depth"
	flip_depth.button_pressed = spineboy.camera_relative_depth
	controls.add_child(flip_depth)
	show_marker = CheckButton.new()
	show_marker.text = "Slot marker"
	controls.add_child(show_marker)
	marker_follows = CheckButton.new()
	marker_follows.text = "Marker follows flip"
	controls.add_child(marker_follows)
	face_ignores_flip = CheckButton.new()
	face_ignores_flip.text = "Face shader ignores flip"
	controls.add_child(face_ignores_flip)
	var exaggerate := Button.new()
	exaggerate.text = "Exaggerate gap"
	exaggerate.pressed.connect(func():
		auto_orbit.button_pressed = false
		pause_animation.button_pressed = true
		flip_depth.button_pressed = true
		show_marker.button_pressed = true
		marker_follows.button_pressed = false
		depth_gap.value = 0.03
		set_angle(145))
	controls.add_child(exaggerate)
	var normal_gap := Button.new()
	normal_gap.text = "Gap 0.001"
	normal_gap.pressed.connect(func(): depth_gap.value = 0.001)
	controls.add_child(normal_gap)
	var note := Label.new()
	note.text = "Green sphere: ordinary slot child. Follow moves its anchor for THIS camera only, not its mesh shape.\nFace override opts out of the shader flip: eyes/goggles can disappear or detach. No user shader is rewritten."
	layout.add_child(note)

	marker_slot = SpineSlotNode3D.new()
	marker_slot.slot_name = "front-fist"
	spineboy.add_child(marker_slot)
	marker = MeshInstance3D.new()
	var sphere := SphereMesh.new()
	sphere.radius = 0.12
	sphere.height = 0.24
	var green := StandardMaterial3D.new()
	green.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	green.albedo_color = Color.GREEN
	sphere.material = green
	marker.mesh = sphere
	marker.visible = false
	marker_slot.add_child(marker)
	face_material = ShaderMaterial.new()
	face_material.shader = preload("res://examples/18-depth-offset-orbit/depth-normal.gdshader")
	face_material.set_shader_parameter("apply_camera_depth", false)
	for name in ["eye", "goggles"]:
		var helper := SpineSlotNode3D.new()
		helper.slot_name = name
		spineboy.add_child(helper)
		face_slots.append(helper)
	flip_depth.toggled.connect(func(_enabled): update_materials())
	face_ignores_flip.toggled.connect(func(_enabled): update_materials())
	show_marker.toggled.connect(func(enabled): marker.visible = enabled)
	marker_follows.toggled.connect(func(_enabled): update_marker())
	update_materials()

func update_materials():
	spineboy.camera_relative_depth = flip_depth.button_pressed
	spineboy.alpha_cutoff = alpha_cutoff.value
	for helper in face_slots:
		helper.normal_material = face_material if face_ignores_flip.button_pressed else null
	update_status()

func update_marker():
	# The native helper explicitly follows this one camera. Empty keeps physical depth.
	marker_slot.depth_camera = marker_slot.get_path_to(camera) if marker_follows.button_pressed else NodePath()
