extends Node2D

@onready var spineboy: SpineSprite = $Spineboy
@onready var crosshair_bone: SpineBoneNode = $Spineboy/CrosshairBone

func _ready():
	spineboy.get_animation_state().set_animation("walk", true, 0)
	spineboy.get_animation_state().set_animation("aim", true, 1)
	
func _process(_delta):
	crosshair_bone.global_position = get_viewport().get_mouse_position()
