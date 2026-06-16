using Godot;
using System;

public partial class Spineboy : SpineSprite2D {
	public override void _Ready() {
		GetAnimationState().SetAnimation("run", true, 0);
	}
}
