extends Node2D

onready var player = $AnimationPlayer

func _ready():
	pass
	
func _process(_delta):
	if Input.is_action_just_pressed("ui_accept"):
		player.play("walk-run-die")
