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

// Contributed by: Mitch Thompson

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
		public float jumpDuration = 0.5f;
		public float jumpInterruptFactor = 100;
		public float forceCrouchVelocity = 25;
		public float forceCrouchDuration = 0.5f;

		[Header("Graphics")]
		public SkeletonAnimation skeletonAnimation;

		[Header("Animation")]
		[SpineAnimation(dataField: "skeletonAnimation")]
		public string walkName = "Walk";
		[SpineAnimation(dataField: "skeletonAnimation")]
		public string runName = "Run";
		[SpineAnimation(dataField: "skeletonAnimation")]
		public string idleName = "Idle";
		[SpineAnimation(dataField: "skeletonAnimation")]
		public string jumpName = "Jump";
		[SpineAnimation(dataField: "skeletonAnimation")]
		public string fallName = "Fall";
		[SpineAnimation(dataField: "skeletonAnimation")]
		public string crouchName = "Crouch";

		[Header("Effects")]
		public AudioSource jumpAudioSource;
		public AudioSource hardfallAudioSource;
		public AudioSource footstepAudioSource;
		public ParticleSystem landParticles;
		[SpineEvent]
		public string footstepEventName = "Footstep";
		CharacterController controller;
		Vector3 velocity = default(Vector3);
		float jumpEndTime = 0;
		bool jumpInterrupt = false;
		float forceCrouchEndTime;
		Vector2 input;
		bool wasGrounded = false;

		void Awake () {
			controller = GetComponent<CharacterController>();
		}

		void Start () {
			skeletonAnimation.AnimationState.Event += HandleEvent;
		}

		void HandleEvent (Spine.TrackEntry trackEntry, Spine.Event e) {
			if (e.Data.Name == footstepEventName) {
				footstepAudioSource.Stop();
				footstepAudioSource.pitch = GetRandomPitch(0.2f);
				footstepAudioSource.Play();
			}
		}

		static float GetRandomPitch (float maxOffset) {
			return 1f + Random.Range(-maxOffset, maxOffset);
		}

		void Update () {
			input.x = Input.GetAxis(XAxis);
			input.y = Input.GetAxis(YAxis);
			bool crouching = (controller.isGrounded && input.y < -0.5f) || (forceCrouchEndTime > Time.time);
			velocity.x = 0;
			float dt = Time.deltaTime;

			if (!crouching) { 
				if (Input.GetButtonDown(JumpButton) && controller.isGrounded) {					
					jumpAudioSource.Stop();
					jumpAudioSource.Play();
					velocity.y = jumpSpeed;
					jumpEndTime = Time.time + jumpDuration;
				} else {
					jumpInterrupt |= Time.time < jumpEndTime && Input.GetButtonUp(JumpButton);
				}

				if (input.x != 0) {
					velocity.x = Mathf.Abs(input.x) > 0.6f ? runSpeed : walkSpeed;
					velocity.x *= Mathf.Sign(input.x);
				}

				if (jumpInterrupt) {
					if (velocity.y > 0) {
						velocity.y = Mathf.MoveTowards(velocity.y, 0, dt * jumpInterruptFactor);
					} else { 
						jumpInterrupt = false;
					}
				}
			}

			var gravityDeltaVelocity = Physics.gravity * gravityScale * dt;

			if (controller.isGrounded) {
				jumpInterrupt = false;
			} else {
				if (wasGrounded) {
					if (velocity.y < 0)
						velocity.y = 0;
				} else {
					velocity += gravityDeltaVelocity;
				}
			}

			wasGrounded = controller.isGrounded;

			controller.Move(velocity * dt);

			if (!wasGrounded && controller.isGrounded) {
				if (-velocity.y > forceCrouchVelocity) {
					forceCrouchEndTime = Time.time + forceCrouchDuration;
					hardfallAudioSource.Play();
				} else {
					footstepAudioSource.Play();
				}
					
				landParticles.Emit((int)(velocity.y / -9f) + 2);
			}

			if (controller.isGrounded) {
				if (crouching) {
					skeletonAnimation.AnimationName = crouchName;
				} else {
					if (input.x == 0)
						skeletonAnimation.AnimationName = idleName;
					else
						skeletonAnimation.AnimationName = Mathf.Abs(input.x) > 0.6f ? runName : walkName;
				}
			} else {
				skeletonAnimation.AnimationName = velocity.y > 0 ? jumpName : fallName;
			}

			if (input.x != 0)
				skeletonAnimation.Skeleton.FlipX = input.x < 0;
			
		}
	}
}