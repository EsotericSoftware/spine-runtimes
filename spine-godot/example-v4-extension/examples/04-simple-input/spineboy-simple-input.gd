extends SpineSprite

func _ready():
	get_animation_state().set_animation("idle", true, 0)

func _process(_delta):
	if Input.is_action_just_pressed("ui_left"):
		get_animation_state().set_animation("run", true, 0)
		get_skeleton().set_scale_x(-1)
		
	if Input.is_action_just_released("ui_left"):
		get_animation_state().set_animation("idle", true, 0)
		
	if (Input.is_action_just_pressed("ui_right")):
		get_animation_state().set_animation("run", true, 0)
		get_skeleton().set_scale_x(1)
		
	if Input.is_action_just_released("ui_right"):
		get_animation_state().set_animation("idle", true, 0)
