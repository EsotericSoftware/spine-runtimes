using Godot;
using System;

public partial class SlotNode : Node2D
{
	public override void _Ready()
	{
		SpineSprite spineboy = GetNode<SpineSprite>("Spineboy");
		SpineSprite raptor = GetNode<SpineSprite>("Spineboy/GunSlot/Raptor");
		SpineSprite tinySpineboy = GetNode<SpineSprite>("Spineboy/FrontFirstSlot/TinySpineboy");

		var entry = spineboy.GetAnimationState().SetAnimation("run", true, 0);
		entry.SetTimeScale(0.1f);
		raptor.GetAnimationState().SetAnimation("walk", true, 0);
		tinySpineboy.GetAnimationState().SetAnimation("walk", true, 0);
	}
}
