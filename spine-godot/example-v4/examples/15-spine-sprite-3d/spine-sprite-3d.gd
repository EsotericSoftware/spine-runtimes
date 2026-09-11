extends Node3D

func _ready():
	$LeftSpineboy.get_animation_state().set_animation("walk", true, 0)
	$RightSpineboy.get_animation_state().set_animation("hoverboard", true, 0)
	$Coin.get_animation_state().set_animation("animation", true, 0)
