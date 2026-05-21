extends Node2D

# Render separation demo — the Godot equivalent of spine-unity's
# SkeletonRenderSeparator (see issue #2689).
#
# Each pair below draws a raptor *between* a spineboy's back and front halves,
# without duplicating the spineboy:
#
#   * A SpineSlotRangeProxy claims the spineboy's [front-upper-arm .. last slot]
#     range and renders those slots itself; the spineboy renders the rest.
#   * The spineboy (the proxy's source) gets z_index = -1, so the slots it still
#     renders — its back half — draw behind the raptor. The proxy keeps the
#     default z_index, so the claimed front half draws in front of the raptor.
#
# Driving the split with z_index keeps it correct regardless of where the
# spineboy sits in the scene tree:
#
#   * Pair 1 (left): Spineboy1 is bone-attached to Raptor1 — parented under a
#     SpineBoneNode on the raptor's "head" bone — so it rides the raptor, and
#     the separation still works while nested inside it.
#   * Pair 2 (right): Spineboy2 and Raptor2 are plain siblings.
#
# For a whole skeleton split at once, a SpineSpriteRenderSeparator can build the
# proxies automatically from a list of separator slot names.

@onready var raptor_1: SpineSprite = $Raptor1
@onready var spineboy_1: SpineSprite = $Raptor1/SpineBoneNode/Spineboy1
@onready var raptor_2: SpineSprite = $Raptor2
@onready var spineboy_2: SpineSprite = $Spineboy2

func _ready() -> void:
	raptor_1.get_animation_state().set_animation("walk", true, 0)
	spineboy_1.get_animation_state().set_animation("walk", true, 0)
	raptor_2.get_animation_state().set_animation("walk", true, 0)
	spineboy_2.get_animation_state().set_animation("walk", true, 0)
