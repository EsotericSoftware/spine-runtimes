extends Node2D

onready var player = $AnimationPlayer

func _ready():
	player.play("walk-run-die")
