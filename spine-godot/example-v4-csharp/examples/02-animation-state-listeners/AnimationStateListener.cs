using Godot;
using System;

public partial class AnimationStateListener : Node2D
{
	public override void _Ready()
	{
		var footStepAudio = GetNode<AudioStreamPlayer>("FootstepAudio");
		var spineboy = GetNode<SpineSprite>("Spineboy");
		spineboy.AnimationStarted += (sprite, animationState, trackEntry) =>
		{
			var spineTrackEntry = trackEntry as SpineTrackEntry;
			GD.Print("Animation started: " + spineTrackEntry.GetAnimation().GetName());
		};
		spineboy.AnimationInterrupted += (sprite, animationState, trackEntry) =>
		{
			var spineTrackEntry = trackEntry as SpineTrackEntry;
			GD.Print("Animation interrupted: " + spineTrackEntry.GetAnimation().GetName());
		};
		spineboy.AnimationCompleted += (sprite, animationState, trackEntry) =>
		{
			var spineTrackEntry = trackEntry as SpineTrackEntry;
			GD.Print("Animation completed: " + spineTrackEntry.GetAnimation().GetName());
		};
		spineboy.AnimationDisposed += (sprite, animationState, trackEntry) =>
		{
			var spineTrackEntry = trackEntry as SpineTrackEntry;
			GD.Print("Animation disposed: " + spineTrackEntry.GetAnimation().GetName());
		};
		spineboy.AnimationEvent += (sprite, animationState, trackEntry, eventObject) =>
		{
			var spineTrackEntry = trackEntry as SpineTrackEntry;
			var spineEvent = eventObject as SpineEvent;
			GD.Print("Animation event: " + spineTrackEntry.GetAnimation().GetName() + ", " + spineEvent.GetData().GetEventName());
			if (spineEvent.GetData().GetEventName() == "footstep")
				footStepAudio.Play();
		};
		var animationState = spineboy.GetAnimationState();
		animationState.SetAnimation("jump", false, 0);
		animationState.AddAnimation("walk", 0, true, 0);
		animationState.AddAnimation("run", 2, true, 0);
	}
}
