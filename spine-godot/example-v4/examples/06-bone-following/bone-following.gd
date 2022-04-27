extends Node2D

@onready var spineboy: SpineSprite = $Spineboy
@onready var sprite: Sprite2D = $Spineboy/Sprite

func _world_transforms_changed(_sprite):
	sprite.global_transform = spineboy.get_global_bone_transform("gun-tip")

func _ready():
	spineboy.get_animation_state().set_animation("walk", true, 0)
	spineboy.connect("world_transforms_changed", Callable(self, "_world_transforms_changed"))
