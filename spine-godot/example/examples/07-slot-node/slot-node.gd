extends Node2D

onready var spineboy: SpineSprite = $Spineboy

func _ready():
	spineboy.get_animation_state().set_animation("walk", true, 0)
