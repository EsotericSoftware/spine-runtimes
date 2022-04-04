extends SpineSprite

func _ready():
	var data = get_skeleton().get_data()
	var customSkin = SpineSkin.new()
	var skinBase = data.find_skin("skin-base")
	#customSkin.add_skin()
	#customSkin.add_skin(data.find_skin("nose/short"))
	#customSkin.add_skin(data.find_skin("eyelids/girly"))
	#customSkin.add_skin(data.find_skin("eyes/violet"))
	#customSkin.add_skin(data.find_skin("hair/brown"))
	#customSkin.add_skin(data.find_skin("clothes/hoodie-orange"))
	#customSkin.add_skin(data.find_skin("legs/pants-jeans"))
	#customSkin.add_skin(data.find_skin("accessories/bag"))
	#customSkin.add_skin(data.find_skin("accessories/hat-red-yellow"))
	get_skeleton().set_skin(customSkin);
