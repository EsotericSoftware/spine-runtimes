using Godot;
using System;

public partial class Lighting : Node2D
{	
	public override void _Ready()
	{
		GetNode<SpineSprite2D>("SpineSprite2D").GetAnimationState().SetAnimation("walk", true, 0);
	}
}
