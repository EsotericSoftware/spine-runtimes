extends Node2D

@onready var spineboy = $SpineSprite
@onready var center_bone = $SpineSprite/HoverboardCenterBone
@onready var center_ray = $SpineSprite/HoverboardCenterBone/CenterRay
@onready var target_bone = $SpineSprite/HoverboardTargetBone
@onready var target_ray = $SpineSprite/HoverboardTargetBone/TargetRay
@onready var hip_bone = $SpineSprite/HipBone
var center_hip_distance = 0

func _ready():
	spineboy.get_animation_state().set_animation("hoverboard", true, 0)
	spineboy.update_skeleton(0);
	center_hip_distance = hip_bone.global_position.y - center_bone.global_position.y

func _physics_process(delta):
	if target_ray.is_colliding():
		target_bone.global_position.y = target_ray.get_collision_point().y - 30

	if center_ray.is_colliding():
		center_bone.global_position.y = center_ray.get_collision_point().y - 30
		
	if abs(hip_bone.global_position.y - center_bone.global_position.y) - abs(center_hip_distance) < 20:
		hip_bone.global_position.y = center_bone.global_position.y + center_hip_distance
		
	spineboy.global_position.x += delta * 150;
