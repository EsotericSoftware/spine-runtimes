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
using Spine.Unity;

namespace Spine.Unity.Examples {

	[RequireComponent(typeof(CharacterController))]
	public class BasicPlatformerController : MonoBehaviour {

		[Header("Controls")]
		public string XAxis = "Horizontal";
		public string YAxis = "Vertical";
		public string JumpButton = "Jump";

		[Header("Moving")]
		public float walkSpeed = 1.5f;
		public float runSpeed = 7f;
		public float gravityScale = 6.6f;

		[Header("Jumping")]
		public float jumpSpeed = 25;
		public float minimumJumpDuration = 0.5f;
		public float jumpInterruptFactor = 0.5f;
		public float forceCrouchVelocity = 25;
		public float forceCrouchDuration = 0.5f;

		[Header("Visuals")]
		public SkeletonAnimation skeletonAnimation;

		[Header("Animation")]
		public TransitionDictionaryExample transitions;
		public AnimationReferenceAsset walk;
		public AnimationReferenceAsset run;
		public AnimationReferenceAsset idle;
		public AnimationReferenceAsset jump;
		public AnimationReferenceAsset fall;
		public AnimationReferenceAsset crouch;
		public AnimationReferenceAsset runFromFall;

		[Header("Effects")]
		public AudioSource jumpAudioSource;
		public AudioSource hardfallAudioSource;
		public ParticleSystem landParticles;
		public HandleEventWithAudioExample footstepHandler;

		CharacterController controller;
		Vector2 input = default(Vector2);
		Vector3 velocity = default(Vector3);
		float minimumJumpEndTime = 0;
		float forceCrouchEndTime;
		bool wasGrounded = false;

		AnimationReferenceAsset targetAnimation;
		AnimationReferenceAsset previousTargetAnimation;

		void Awake () {
			controller = GetComponent<CharacterController>();
		}

		void Update () {
			float dt = Time.deltaTime;
			bool isGrounded = controller.isGrounded;
			bool landed = !wasGrounded && isGrounded;

			// Dummy input.
			input.x = Input.GetAxis(XAxis);
			input.y = Input.GetAxis(YAxis);
			bool inputJumpStop = Input.GetButtonUp(JumpButton);
			bool inputJumpStart = Input.GetButtonDown(JumpButton);			
			bool doCrouch = (isGrounded && input.y < -0.5f) || (forceCrouchEndTime > Time.time);
			bool doJumpInterrupt = false;
			bool doJump = false;
			bool hardLand = false;

			if (landed) {
				if (-velocity.y > forceCrouchVelocity) {
					hardLand = true;
					doCrouch = true;
					forceCrouchEndTime = Time.time + forceCrouchDuration;
				}
			}

			if (!doCrouch) {
				if (isGrounded) {
					if (inputJumpStart) {
						doJump = true;
					}
				} else {
					doJumpInterrupt = inputJumpStop && Time.time < minimumJumpEndTime;
				}
			}

			// Dummy physics and controller using UnityEngine.CharacterController.
			Vector3 gravityDeltaVelocity = Physics.gravity * gravityScale * dt;
			

			if (doJump) {
				velocity.y = jumpSpeed;
				minimumJumpEndTime = Time.time + minimumJumpDuration;
			} else if (doJumpInterrupt) {
				if (velocity.y > 0)
					velocity.y *= jumpInterruptFactor;
			}

			velocity.x = 0;
			if (!doCrouch) {
				if (input.x != 0) {
					velocity.x = Mathf.Abs(input.x) > 0.6f ? runSpeed : walkSpeed;
					velocity.x *= Mathf.Sign(input.x);
				}
			}
			
			
			if (!isGrounded) {
				if (wasGrounded) {
					if (velocity.y < 0)
						velocity.y = 0;
				} else {
					velocity += gravityDeltaVelocity;
				}
			}
			controller.Move(velocity * dt);

			// Animation
			// Determine target animation.
			if (isGrounded) {
				if (doCrouch) {
					targetAnimation = crouch;
				} else {
					if (input.x == 0)
						targetAnimation = idle;
					else
						targetAnimation = Mathf.Abs(input.x) > 0.6f ? run : walk;
				}
			} else {
				targetAnimation = velocity.y > 0 ? jump : fall;
			}

			// Handle change in target animation.
			if (previousTargetAnimation != targetAnimation) {
				Animation transition = null;
				if (transitions != null && previousTargetAnimation != null) {
					transition = transitions.GetTransition(previousTargetAnimation, targetAnimation);
				}

				if (transition != null) {
					skeletonAnimation.AnimationState.SetAnimation(0, transition, false).MixDuration = 0.05f;
					skeletonAnimation.AnimationState.AddAnimation(0, targetAnimation, true, 0f);
				} else {
					skeletonAnimation.AnimationState.SetAnimation(0, targetAnimation, true);
				}
			}
			previousTargetAnimation = targetAnimation;

			// Face intended direction.
			if (input.x != 0)
				skeletonAnimation.Skeleton.FlipX = input.x < 0;


			// Effects
			if (doJump) {
				jumpAudioSource.Stop();
				jumpAudioSource.Play();
			}

			if (landed) {
				if (hardLand) {
					hardfallAudioSource.Play();
				} else {
					footstepHandler.Play();
				}

				landParticles.Emit((int)(velocity.y / -9f) + 2);
			}

			wasGrounded = isGrounded;
		}

	}
}