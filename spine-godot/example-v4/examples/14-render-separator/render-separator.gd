extends Node2D

# Render separation demo — the Godot equivalent of spine-unity's
# SkeletonRenderSeparator (see issue #2689).
#
# CharacterB (the raptor) is drawn *between* the back and front halves of
# CharacterA (spineboy) without duplicating CharacterA:
#
#   * FrontProxy is a SpineSlotRangeProxy. Its source is CharacterA and it
#     claims the slot range [front-upper-arm .. last slot], rendering only
#     those slots itself.
#   * CharacterA renders every slot the proxy does NOT claim.
#   * Scene/tree order is CharacterA, CharacterB, FrontProxy, so the draw
#     order becomes:
#         CharacterA back half  ->  CharacterB  ->  CharacterA front half
#     which places the raptor between spineboy's torso/back-arm and his
#     head/front-arm.
#
# For a whole skeleton split at once, a SpineSpriteRenderSeparator can build
# the proxies automatically from a list of separator slot names.

@onready var character_a: SpineSprite = $CharacterA
@onready var character_b: SpineSprite = $CharacterB

func _ready() -> void:
	character_a.get_animation_state().set_animation("walk", true, 0)
	character_b.get_animation_state().set_animation("walk", true, 0)
