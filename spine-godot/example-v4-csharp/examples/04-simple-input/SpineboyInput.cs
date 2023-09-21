using Godot;
using System;

public partial class SpineboyInput : SpineSprite
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetAnimationState().SetAnimation("idle", true, 0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_left"))
		{
			GetAnimationState().SetAnimation("run", true, 0);
			GetSkeleton().SetScaleX(-1);
		}

		if (Input.IsActionJustReleased("ui_left"))
			GetAnimationState().SetAnimation("idle", true, 0);

		if (Input.IsActionJustPressed("ui_right"))
		{
			GetAnimationState().SetAnimation("run", true, 0);
			GetSkeleton().SetScaleX(1);
		}

		if (Input.IsActionJustReleased("ui_right"))
			GetAnimationState().SetAnimation("idle", true, 0);
	}
}
