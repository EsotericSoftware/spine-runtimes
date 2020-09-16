/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace Spine.Unity.Examples {
	public class SpineboyBeginnerView : MonoBehaviour {

		#region Inspector
		[Header("Components")]
		public SpineboyBeginnerModel model;
		public SkeletonAnimation skeletonAnimation;

		public AnimationReferenceAsset run, idle, aim, shoot, jump;
		public EventDataReferenceAsset footstepEvent;

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
			model.StartAimEvent += StartPlayingAim;
			model.StopAimEvent += StopPlayingAim;
			skeletonAnimation.AnimationState.Event += HandleEvent;
		}

		void HandleEvent (Spine.TrackEntry trackEntry, Spine.Event e) {
			if (e.Data == footstepEvent.EventData)
				PlayFootstepSound();
		}

		void Update () {
			if (skeletonAnimation == null) return;
			if (model == null) return;

			if ((skeletonAnimation.skeleton.ScaleX < 0) != model.facingLeft) {	// Detect changes in model.facingLeft
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
			Animation nextAnimation;

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

			skeletonAnimation.AnimationState.SetAnimation(0, nextAnimation, true);
		}

		void PlayFootstepSound () {
			footstepSource.Play();
			footstepSource.pitch = GetRandomPitch(footstepPitchOffset);
		}

		[ContextMenu("Check Tracks")]
		void CheckTracks () {
			var state = skeletonAnimation.AnimationState;
			Debug.Log(state.GetCurrent(0));
			Debug.Log(state.GetCurrent(1));
		}

		#region Transient Actions
		public void PlayShoot () {
			// Play the shoot animation on track 1.
			var shootTrack = skeletonAnimation.AnimationState.SetAnimation(1, shoot, false);
			shootTrack.AttachmentThreshold = 1f;
			shootTrack.MixDuration = 0f;
			var empty1 = skeletonAnimation.state.AddEmptyAnimation(1, 0.5f, 0.1f);
			empty1.AttachmentThreshold = 1f;

			// Play the aim animation on track 2 to aim at the mouse target.
			var aimTrack = skeletonAnimation.AnimationState.SetAnimation(2, aim, false);
			aimTrack.AttachmentThreshold = 1f;
			aimTrack.MixDuration = 0f;
			var empty2 = skeletonAnimation.state.AddEmptyAnimation(2, 0.5f, 0.1f);
			empty2.AttachmentThreshold = 1f;

			gunSource.pitch = GetRandomPitch(gunsoundPitchOffset);
			gunSource.Play();
			//gunParticles.randomSeed = (uint)Random.Range(0, 100);
			gunParticles.Play();
		}

		public void StartPlayingAim () {
			// Play the aim animation on track 2 to aim at the mouse target.
			var aimTrack = skeletonAnimation.AnimationState.SetAnimation(2, aim, true);
			aimTrack.AttachmentThreshold = 1f;
			aimTrack.MixDuration = 0f;
		}

		public void StopPlayingAim () {
			var empty2 = skeletonAnimation.state.AddEmptyAnimation(2, 0.5f, 0.1f);
			empty2.AttachmentThreshold = 1f;
		}

		public void Turn (bool facingLeft) {
			skeletonAnimation.Skeleton.ScaleX = facingLeft ? -1f : 1f;
			// Maybe play a transient turning animation too, then call ChangeStableAnimation.
		}
		#endregion

		#region Utility
		public float GetRandomPitch (float maxPitchOffset) {
			return 1f + Random.Range(-maxPitchOffset, maxPitchOffset);
		}
		#endregion
	}

}
