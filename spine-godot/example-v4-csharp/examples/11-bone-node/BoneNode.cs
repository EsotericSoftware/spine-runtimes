using Godot;
using System;

public partial class BoneNode : Node2D
{
	private SpineSprite2D spineboy;
	private SpineBoneNode2D centerBone;
	private RayCast2D centerRay;
	private SpineBoneNode2D targetBone;
	private RayCast2D targetRay;
	private SpineBoneNode2D hipBone;
	private float centerHipDistance = 0;
	
	public override void _Ready()
	{
		spineboy = GetNode<SpineSprite2D>("SpineSprite2D");
		centerBone = GetNode<SpineBoneNode2D>("SpineSprite2D/HoverboardCenterBone");
		centerRay = GetNode<RayCast2D>("SpineSprite2D/HoverboardCenterBone/CenterRay");
		targetBone = GetNode<SpineBoneNode2D>("SpineSprite2D/HoverboardTargetBone");
		targetRay = GetNode<RayCast2D>("SpineSprite2D/HoverboardTargetBone/TargetRay");
		hipBone = GetNode<SpineBoneNode2D>("SpineSprite2D/HipBone");
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
