extends Node2D

func _ready():
	$SpineSprite2D.get_animation_state().set_animation("walk");
