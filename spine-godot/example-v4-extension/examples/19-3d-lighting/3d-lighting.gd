extends Node3D

@onready var raptor: SpineSprite3D = $Raptor
@onready var light: OmniLight3D = $OmniLight3D
var auto_light: CheckButton
var lighting: CheckButton
var normal_map: CheckButton
var flip_normal_y: CheckButton
var shadows: CheckButton
var normal_scale: HSlider
var shadow_cutoff: HSlider
var status: Label
var light_angle := 0.0

func _ready():
	$Camera3D.look_at(Vector3(0, 0.4, 0))
	var animation_state := raptor.get_animation_state()
	if animation_state != null:
		animation_state.set_animation("walk", true, 0)
	create_controls()
	update_status()

func _process(delta: float):
	if auto_light != null and auto_light.button_pressed:
		light_angle = fposmod(light_angle + delta * 35.0, 360.0)
		var radians := deg_to_rad(light_angle)
		light.position = Vector3(cos(radians) * 3.5, 2.5 + sin(radians * 0.5), 4.0)
		update_status()

func create_controls():
	var panel := PanelContainer.new()
	panel.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	$UI.add_child(panel)
	var margin := MarginContainer.new()
	for side in ["left", "top", "right", "bottom"]:
		margin.add_theme_constant_override("margin_" + side, 12)
	panel.add_child(margin)
	var rows := VBoxContainer.new()
	margin.add_child(rows)
	var title := Label.new()
	title.text = "SpineSprite3D — generated lighting, atlas normal maps, and shadows"
	title.add_theme_font_size_override("font_size", 22)
	rows.add_child(title)
	var controls := HFlowContainer.new()
	rows.add_child(controls)
	auto_light = add_toggle(controls, "Move light", true)
	lighting = add_toggle(controls, "Lighting", raptor.lighting_enabled)
	normal_map = add_toggle(controls, "Normal map", raptor.normal_map_enabled)
	flip_normal_y = add_toggle(controls, "Flip normal Y", raptor.normal_map_flip_y)
	shadows = add_toggle(controls, "Shadows", raptor.shadow_casting != SpineSprite3D.SHADOW_CASTING_OFF)
	var normal_scale_label := Label.new()
	normal_scale_label.text = "Normal strength"
	controls.add_child(normal_scale_label)
	normal_scale = HSlider.new()
	normal_scale.custom_minimum_size.x = 90
	normal_scale.min_value = 0
	normal_scale.max_value = 3
	normal_scale.step = 0.05
	normal_scale.value = raptor.normal_scale
	controls.add_child(normal_scale)
	var shadow_cutoff_label := Label.new()
	shadow_cutoff_label.text = "Shadow cutoff"
	controls.add_child(shadow_cutoff_label)
	shadow_cutoff = HSlider.new()
	shadow_cutoff.custom_minimum_size.x = 90
	shadow_cutoff.min_value = 0
	shadow_cutoff.max_value = 1
	shadow_cutoff.step = 0.01
	shadow_cutoff.value = raptor.shadow_alpha_cutoff
	controls.add_child(shadow_cutoff)
	status = Label.new()
	rows.add_child(status)
	var note := Label.new()
	note.text = "Normal map: assets/raptor/n_raptor.png (loaded by the atlas importer). The opaque ground receives the texture-alpha shadow."
	rows.add_child(note)
	lighting.toggled.connect(func(enabled): raptor.lighting_enabled = enabled; update_status())
	normal_map.toggled.connect(func(enabled): raptor.normal_map_enabled = enabled; update_status())
	flip_normal_y.toggled.connect(func(enabled): raptor.normal_map_flip_y = enabled; update_status())
	shadows.toggled.connect(func(enabled):
		raptor.shadow_casting = SpineSprite3D.SHADOW_CASTING_DOUBLE_SIDED if enabled else SpineSprite3D.SHADOW_CASTING_OFF
		update_status())
	normal_scale.value_changed.connect(func(value): raptor.normal_scale = value; update_status())
	shadow_cutoff.value_changed.connect(func(value): raptor.shadow_alpha_cutoff = value; update_status())

func add_toggle(parent: Control, text: String, enabled: bool) -> CheckButton:
	var button := CheckButton.new()
	button.text = text
	button.button_pressed = enabled
	parent.add_child(button)
	return button

func update_status():
	if status == null:
		return
	status.text = "Normal strength %.2f | shadow cutoff %.2f | light (%.1f, %.1f, %.1f)" % [
		raptor.normal_scale, raptor.shadow_alpha_cutoff, light.position.x, light.position.y, light.position.z]
