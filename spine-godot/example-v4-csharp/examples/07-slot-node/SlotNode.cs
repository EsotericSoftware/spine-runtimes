using Godot;
using System;

public partial class SlotNode : Node2D
{
	public override void _Ready()
	{
		SpineSprite2D spineboy = GetNode<SpineSprite2D>("Spineboy");
		SpineSprite2D raptor = GetNode<SpineSprite2D>("Spineboy/GunSlot/Raptor");
		SpineSprite2D tinySpineboy = GetNode<SpineSprite2D>("Spineboy/FrontFirstSlot/TinySpineboy");

		var entry = spineboy.GetAnimationState().SetAnimation("run", true, 0);
		entry.SetTimeScale(0.1f);
		raptor.GetAnimationState().SetAnimation("walk", true, 0);
		tinySpineboy.GetAnimationState().SetAnimation("walk", true, 0);
	}
}
