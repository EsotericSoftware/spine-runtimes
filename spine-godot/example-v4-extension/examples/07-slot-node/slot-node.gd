extends Node2D

@onready var spineboy: SpineSprite = $Spineboy
@onready var raptor: SpineSprite = $Spineboy/GunSlot/Raptor
@onready var tiny_spineboy: SpineSprite = $Spineboy/FrontFistSlot/TinySpineboy

func _ready():	
	var entry = spineboy.get_animation_state().set_animation("run", true, 0)
	entry.set_time_scale(0.1)
	raptor.get_animation_state().set_animation("walk", true, 0)
	tiny_spineboy.get_animation_state().set_animation("walk", true, 0)
