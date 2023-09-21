using Godot;
using System;

public partial class MouseFollowing : Node2D
{
	private SpineSprite spineboy;

	private SpineBoneNode crosshairBonne;
	
	public override void _Ready()
	{
		spineboy = GetNode<SpineSprite>("Spineboy");
		crosshairBonne = spineboy.GetNode<SpineBoneNode>("CrosshairBone");
		spineboy.GetAnimationState().SetAnimation("walk", true, 0);
		spineboy.GetAnimationState().SetAnimation("aim", true, 1);
	}
	
	public override void _Process(double delta)
	{
		crosshairBonne.GlobalPosition = GetViewport().GetMousePosition();
	}
}
