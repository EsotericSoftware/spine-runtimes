using Godot;
using System;

public partial class Lighting : Node2D
{	
	public override void _Ready()
	{
		GetNode<SpineSprite>("SpineSprite").GetAnimationState().SetAnimation("walk", true, 0);
	}
}
