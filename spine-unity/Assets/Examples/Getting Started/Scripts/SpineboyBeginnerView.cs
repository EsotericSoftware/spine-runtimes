/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

﻿using UnityEngine;
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
