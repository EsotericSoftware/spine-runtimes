extends Node2D

@onready var spineboy = $SpineSprite
@onready var center_bone = $SpineSprite/HoverboardCenterBone
@onready var center_ray = $SpineSprite/HoverboardCenterBone/RayCast2D
@onready var target_bone = $SpineSprite/HoverboardTargetBone
@onready var target_ray = $SpineSprite/HoverboardTargetBone/RayCast2D

func _ready():
	spineboy.get_animation_state().set_animation("hoverboard", true, 0)

func _physics_process(delta):
	if target_ray.is_colliding():
		target_bone.global_position.y = target_ray.get_collision_point().y - 50
	if center_ray.is_colliding():
		center_bone.global_position.y = center_ray.get_collision_point().y - 50

	spineboy.global_position.x += delta * 150;
	spineboy.global_position.y = center_bone.global_position.y
