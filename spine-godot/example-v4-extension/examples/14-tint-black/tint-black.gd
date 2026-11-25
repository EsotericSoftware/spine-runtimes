extends Node2D

func _ready():
	# The coin's tint-black slot is additive. The middle coin overrides its additive
	# material with a CanvasItemMaterial, which only supports light tint.
	# The other coins use the default tint-black shader, with modulation on the right.
	for coin in [$Original, $SingleColor, $Modulated]:
		coin.get_animation_state().set_animation("animation", true, 0)
