using Godot;
using System;

public partial class Spineboy : SpineSprite {
	public override void _Ready() {
		GetAnimationState().SetAnimation("run", true, 0);
	}
}
