extends Node2D

@onready var player = $AnimationPlayer
@onready var spineboy = $Spineboy

var speed = 400;
var velocity_x = 0;

func _ready():
	player.play("cutscene")
	pass

func _process(delta):
	if (!player.is_playing()):
		if Input.is_action_just_released("ui_left"):
			spineboy.get_animation_state().set_animation("idle", true, 0)
			velocity_x = 0

		if Input.is_action_just_released("ui_right"):
			spineboy.get_animation_state().set_animation("idle", true, 0)
			velocity_x = 0

		if (Input.is_action_just_pressed("ui_right")):
			spineboy.get_animation_state().set_animation("run", true, 0)
			spineboy.get_skeleton().set_scale_x(1)
			velocity_x = 1

		if Input.is_action_just_pressed("ui_left"):
			spineboy.get_animation_state().set_animation("run", true, 0)
			spineboy.get_skeleton().set_scale_x(-1)
			velocity_x = -1

		spineboy.position.x += velocity_x * speed * delta
