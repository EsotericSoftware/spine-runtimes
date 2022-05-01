extends Node2D

func _ready():
	var spineboy: SpineSprite = $Spineboy
	spineboy.get_animation_state().set_animation("walk", true, 0)
	spineboy.connect("world_transforms_changed", self, "_world_transforms_changed")
