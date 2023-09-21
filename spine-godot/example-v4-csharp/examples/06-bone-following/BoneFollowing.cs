using Godot;
using System;

public partial class BoneFollowing : Node2D
{
	public override void _Ready()
	{
		GetNode<SpineSprite>("Spineboy").GetAnimationState().SetAnimation("walk", true, 0);
	}
}
