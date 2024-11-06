extends Node2D

@onready var celestial_circus: SpineSprite = $"celestial-circus"

var last_x = -1
var last_y = -1
var isMouseOver = false

func _ready():
	celestial_circus.get_animation_state().set_animation("wind-idle", true, 0)
	celestial_circus.get_animation_state().set_animation("eyeblink-long", true, 1)
	celestial_circus.get_animation_state().set_animation("stars", true, 2)
	
func _process(_delta):
	if (Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT) and isMouseOver):
		var pos = get_viewport().get_mouse_position()
		if (last_x != -1):
			var dx = pos.x - last_x
			var dy = pos.y - last_y
			celestial_circus.global_position += Vector2(dx, dy)
			celestial_circus.get_skeleton().physics_translate(dx * 1 / celestial_circus.scale.x, dy * 1 / celestial_circus.scale.y)
		last_x = pos.x
		last_y = pos.y
	else:
		last_x = -1
		last_y = -1

func _on_area_2d_mouse_entered():
	isMouseOver = true

func _on_area_2d_mouse_exited():
	isMouseOver = false
