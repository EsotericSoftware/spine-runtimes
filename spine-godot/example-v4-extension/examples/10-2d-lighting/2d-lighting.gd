extends Node2D

func _ready():
	$SpineSprite.get_animation_state().set_animation("walk");
