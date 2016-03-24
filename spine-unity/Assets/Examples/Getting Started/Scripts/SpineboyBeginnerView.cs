using UnityEngine;
using System.Collections;
using Spine.Unity;

public class SpineboyBeginnerView : MonoBehaviour {
	
	#region Inspector
	[Header("Components")]
	public SpineboyBeginnerModel model;
	public SkeletonAnimation skeletonAnimation;
	//public ParticleSystem gunParticles;

	[SpineAnimation] public string run, idle, shoot, jump;
	[SpineEvent] public string footstepEventName;

	[Header("Audio")]
	public float footstepPitchOffset = 0.2f;
	public float gunsoundPitchOffset = 0.13f;
	public AudioSource footstepSource, gunSource, jumpSource;

	[Header("Effects")]
	public ParticleSystem gunParticles;
	#endregion

	SpineBeginnerBodyState previousViewState;

	void Start () {
		if (skeletonAnimation == null) return;
		model.ShootEvent += PlayShoot;
		skeletonAnimation.state.Event += HandleEvent;
	}

	void HandleEvent (Spine.AnimationState state, int trackIndex, Spine.Event e) {
		if (e.Data.Name == footstepEventName) {
			PlayFootstepSound();
		}
	}

	void Update () {
		if (skeletonAnimation == null) return;
		if (model == null) return;

		if (skeletonAnimation.skeleton.FlipX != model.facingLeft) {	// Detect changes in model.facingLeft
			Turn(model.facingLeft);
		}

		// Detect changes in model.state
		var currentModelState = model.state;

		if (previousViewState != currentModelState) {
			PlayNewStableAnimation();
		}
		
		previousViewState = currentModelState;
	}

	void PlayNewStableAnimation () {
		var newModelState = model.state;
		string nextAnimation;

		// Add conditionals to not interrupt transient animations.

		if (previousViewState == SpineBeginnerBodyState.Jumping && newModelState != SpineBeginnerBodyState.Jumping) {
			PlayFootstepSound();
		}

		if (newModelState == SpineBeginnerBodyState.Jumping) {
			jumpSource.Play();
			nextAnimation = jump;
		} else {
			if (newModelState == SpineBeginnerBodyState.Running) {
				nextAnimation = run;
			} else {
				nextAnimation = idle;
			}
		}

		skeletonAnimation.state.SetAnimation(0, nextAnimation, true);
	}

	void PlayFootstepSound () {
		footstepSource.Play();
		footstepSource.pitch = GetRandomPitch(footstepPitchOffset);
	}

	#region Transient Actions
	public void PlayShoot () {
		// Play the shoot animation on track 1.
		skeletonAnimation.state.SetAnimation(1, shoot, false);
		gunSource.pitch = GetRandomPitch(gunsoundPitchOffset);
		gunSource.Play();
		gunParticles.Play();
	}

	public void Turn (bool facingLeft) {
		skeletonAnimation.skeleton.FlipX = facingLeft;
		// Maybe play a transient turning animation too, then call ChangeStableAnimation.
	}
	#endregion

	#region Utility
	public float GetRandomPitch (float maxPitchOffset) {
		return 1f + Random.Range(-maxPitchOffset, maxPitchOffset);
	}
	#endregion
}
