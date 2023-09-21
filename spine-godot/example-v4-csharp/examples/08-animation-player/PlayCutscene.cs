using Godot;
using System;

public partial class PlayCutscene : Node2D
{
	AnimationPlayer player;
	SpineSprite spineboy;
	float speed = 400;
	float velocityX = 0;
		
	public override void _Ready()
	{
		player = GetNode<AnimationPlayer>("AnimationPlayer");
		player.Play("cutscene");
		spineboy = GetNode<SpineSprite>("Spineboy");
	}
	
	public override void _Process(double delta)
	{
		if (player.IsPlaying()) return;
		
		if (Input.IsActionJustPressed("ui_left"))
		{
			spineboy.GetAnimationState().SetAnimation("run", true, 0);
			spineboy.GetSkeleton().SetScaleX(-1);
			velocityX = -1;
		}

		if (Input.IsActionJustReleased("ui_left"))
		{
			spineboy.GetAnimationState().SetAnimation("idle", true, 0);
			velocityX = 0;
		}

		if (Input.IsActionJustPressed("ui_right"))
		{
			spineboy.GetAnimationState().SetAnimation("run", true, 0);
			spineboy.GetSkeleton().SetScaleX(1);
			velocityX = 1;
		}

		if (Input.IsActionJustReleased("ui_right"))
		{
			spineboy.GetAnimationState().SetAnimation("idle", true, 0);
			velocityX = 0;
		}

		var newPosition = spineboy.Position;
		newPosition.X += velocityX * speed * (float)delta;
		spineboy.Position = newPosition;
	}
}
