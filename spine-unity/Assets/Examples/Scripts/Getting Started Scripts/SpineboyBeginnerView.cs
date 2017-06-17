/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
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
			skeletonAnimation.AnimationState.Event += HandleEvent;
		}

		void HandleEvent (Spine.TrackEntry trackEntry, Spine.Event e) {
			if (e.Data.Name == footstepEventName)
				PlayFootstepSound();
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
			var track = skeletonAnimation.AnimationState.SetAnimation(1, shoot, false);
			track.AttachmentThreshold = 1f;
			track.MixDuration = 0f;
			var empty = skeletonAnimation.state.AddEmptyAnimation(1, 0.5f, 0.1f);
			empty.AttachmentThreshold = 1f;
			gunSource.pitch = GetRandomPitch(gunsoundPitchOffset);
			gunSource.Play();
			//gunParticles.randomSeed = (uint)Random.Range(0, 100);
			gunParticles.Play();
		}

		public void Turn (bool facingLeft) {
			skeletonAnimation.Skeleton.FlipX = facingLeft;
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
