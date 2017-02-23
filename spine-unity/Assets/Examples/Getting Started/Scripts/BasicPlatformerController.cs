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
		public float walkSpeed = 4;
		public float runSpeed = 10;
		public float gravity = 65;

		[Header("Jumping")]
		public float jumpSpeed = 25;
		public float jumpDuration = 0.5f;
		public float jumpInterruptFactor = 100;
		public float forceCrouchVelocity = 25;
		public float forceCrouchDuration = 0.5f;

		[Header("Graphics")]
		public Transform graphicsRoot;
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

		[Header("Audio")]
		public AudioSource jumpAudioSource;
		public AudioSource hardfallAudioSource;
		public AudioSource footstepAudioSource;
		[SpineEvent]
		public string footstepEventName = "Footstep";
		CharacterController controller;
		Vector2 velocity = Vector2.zero;
		Vector2 lastVelocity = Vector2.zero;
		bool lastGrounded = false;
		float jumpEndTime = 0;
		bool jumpInterrupt = false;
		float forceCrouchEndTime;
		Quaternion flippedRotation = Quaternion.Euler(0, 180, 0);

		void Awake () {
			controller = GetComponent<CharacterController>();
		}

		void Start () {
			// Register a callback for Spine Events (in this case, Footstep)
			skeletonAnimation.AnimationState.Event += HandleEvent;
		}

		void HandleEvent (Spine.TrackEntry trackEntry, Spine.Event e) {
			// Play some sound if footstep event fired
			if (e.Data.Name == footstepEventName) {
				footstepAudioSource.Stop();
				footstepAudioSource.pitch = GetRandomPitch(0.2f);
				footstepAudioSource.Play();
			}
		}

		void Update () {
			//control inputs
			float x = Input.GetAxis(XAxis);
			float y = Input.GetAxis(YAxis);
			//check for force crouch
			bool crouching = (controller.isGrounded && y < -0.5f) || (forceCrouchEndTime > Time.time);
			velocity.x = 0;

			//Calculate control velocity
			if (!crouching) { 
				if (Input.GetButtonDown(JumpButton) && controller.isGrounded) {
					//jump
					jumpAudioSource.Stop();
					jumpAudioSource.Play();
					velocity.y = jumpSpeed;
					jumpEndTime = Time.time + jumpDuration;
				} else if (Time.time < jumpEndTime && Input.GetButtonUp(JumpButton)) {
					jumpInterrupt = true;
				}

	            
				if (x != 0) {
					//walk or run
					velocity.x = Mathf.Abs(x) > 0.6f ? runSpeed : walkSpeed;
					velocity.x *= Mathf.Sign(x);
				}

				if (jumpInterrupt) {
					//interrupt jump and smoothly cut Y velocity
					if (velocity.y > 0) {
						velocity.y = Mathf.MoveTowards(velocity.y, 0, Time.deltaTime * 100);
					} else { 
						jumpInterrupt = false;
					}
				}
			}

			//apply gravity F = mA (Learn it, love it, live it)
			velocity.y -= gravity * Time.deltaTime;

			//move
			controller.Move(new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime);
	        
			if (controller.isGrounded) {
				//cancel out Y velocity if on ground
				velocity.y = -gravity * Time.deltaTime;
				jumpInterrupt = false;
			}

	        
			Vector2 deltaVelocity = lastVelocity - velocity;

			if (!lastGrounded && controller.isGrounded) {
				//detect hard fall
				if ((gravity * Time.deltaTime) - deltaVelocity.y > forceCrouchVelocity) {
					forceCrouchEndTime = Time.time + forceCrouchDuration;
					hardfallAudioSource.Play();
				} else {
					//play footstep audio if light fall because why not
					footstepAudioSource.Play();
				}
	            
			}

			//graphics updates
			if (controller.isGrounded) {
				if (crouching) { //crouch
					skeletonAnimation.AnimationName = crouchName;
				} else {
					if (x == 0) //idle
						skeletonAnimation.AnimationName = idleName;
					else //move
						skeletonAnimation.AnimationName = Mathf.Abs(x) > 0.6f ? runName : walkName;
				}
			} else {
				if (velocity.y > 0) //jump
					skeletonAnimation.AnimationName = jumpName;
				else //fall
					skeletonAnimation.AnimationName = fallName;
			}

			//flip left or right
			if (x > 0)
				graphicsRoot.localRotation = Quaternion.identity;
			else if (x < 0)
				graphicsRoot.localRotation = flippedRotation;

			//store previous state
			lastVelocity = velocity;
			lastGrounded = controller.isGrounded;
		}
			
		static float GetRandomPitch (float maxOffset) {
			return 1f + Random.Range(-maxOffset, maxOffset);
		}
	}
}