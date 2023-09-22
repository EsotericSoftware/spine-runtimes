using Godot;
using System;

public partial class BoneNode : Node2D
{
	private SpineSprite spineboy;
	private SpineBoneNode centerBone;
	private RayCast2D centerRay;
	private SpineBoneNode targetBone;
	private RayCast2D targetRay;
	private SpineBoneNode hipBone;
	private float centerHipDistance = 0;
	
	public override void _Ready()
	{
		spineboy = GetNode<SpineSprite>("SpineSprite");
		centerBone = GetNode<SpineBoneNode>("SpineSprite/HoverboardCenterBone");
		centerRay = GetNode<RayCast2D>("SpineSprite/HoverboardCenterBone/CenterRay");
		targetBone = GetNode<SpineBoneNode>("SpineSprite/HoverboardTargetBone");
		targetRay = GetNode<RayCast2D>("SpineSprite/HoverboardTargetBone/TargetRay");
		hipBone = GetNode<SpineBoneNode>("SpineSprite/HipBone");
		spineboy.GetAnimationState().SetAnimation("hoverboard", true, 0);
		spineboy.UpdateSkeleton(0);
		centerHipDistance = hipBone.GlobalPosition.Y - centerBone.GlobalPosition.Y;
	}
	
	public override void _Process(double delta) 
	{
		if (targetRay.IsColliding())
		{
			var newPosition = targetBone.GlobalPosition;
			newPosition.Y = targetRay.GetCollisionPoint().Y - 30;
			targetBone.Position = newPosition;
		}
		
		if (centerRay.IsColliding())
		{
			var newPosition = centerBone.GlobalPosition;
			newPosition.Y = centerRay.GetCollisionPoint().Y - 30;
			centerBone.Position = newPosition;
		}

		if (Math.Abs(hipBone.GlobalPosition.Y - centerBone.GlobalPosition.Y) - Math.Abs(centerHipDistance) < 20)
		{
			var newPosition = hipBone.GlobalPosition;
			newPosition.Y = centerBone.GlobalPosition.Y + centerHipDistance;
			hipBone.Position = newPosition;
		}

		var position = spineboy.GlobalPosition;
		position.X += (float)delta * 150;
		spineboy.GlobalPosition = position;
	}
}
