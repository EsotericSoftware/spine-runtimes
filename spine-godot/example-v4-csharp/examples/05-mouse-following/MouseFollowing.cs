using Godot;
using System;

public partial class MouseFollowing : Node2D
{
	private SpineSprite2D spineboy;

	private SpineBoneNode2D crosshairBonne;
	
	public override void _Ready()
	{
		spineboy = GetNode<SpineSprite2D>("Spineboy");
		crosshairBonne = spineboy.GetNode<SpineBoneNode2D>("CrosshairBone");
		spineboy.GetAnimationState().SetAnimation("walk", true, 0);
		spineboy.GetAnimationState().SetAnimation("aim", true, 1);
	}
	
	public override void _Process(double delta)
	{
		crosshairBonne.GlobalPosition = GetViewport().GetMousePosition();
	}
}
