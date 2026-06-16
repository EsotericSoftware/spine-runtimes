extends Node2D

func _ready():
	var spineboy: SpineSprite2D = $Spineboy
	spineboy.get_animation_state().set_animation("walk", true, 0)	
