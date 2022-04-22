extends Node2D

onready var spineboy: SpineSprite = $Spineboy

func _before_world_transforms_change(_sprite):
	spineboy.set_global_bone_transform("crosshair", Transform2D(0, get_viewport().get_mouse_position()))

func _ready():
	spineboy.get_animation_state().set_animation("walk", true, 0)
	spineboy.get_animation_state().set_animation("aim", true, 1)
	spineboy.connect("before_world_transforms_change", self, "_before_world_transforms_change")
