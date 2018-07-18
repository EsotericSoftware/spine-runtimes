using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine;
using Spine.Unity;

namespace Spine.Unity.Examples {
	public class AnimationStateWithMecanimExample : MonoBehaviour {

		SkeletonAnimation skeletonAnimation;
		Animator logicAnimator;
		
		[Header("Controls")]
		public KeyCode walkButton = KeyCode.LeftShift;
		public KeyCode jumpButton = KeyCode.Space;

		[Header("Animator Properties")]
		public string horizontalSpeedProperty = "Speed";
		public string verticalSpeedProperty = "VerticalSpeed";
		public string groundedProperty = "Grounded";

		[Header("Fake Physics")]
		public float jumpDuration = 1.5f;
		public Vector2 speed;
		public bool isGrounded;

		void Awake () {
			skeletonAnimation = GetComponent<SkeletonAnimation>();
			logicAnimator = GetComponent<Animator>();

			isGrounded = true;
		}

		void Update () {
			float x = Input.GetAxisRaw("Horizontal");			
			if (Input.GetKey(walkButton)) {
				x *= 0.4f;
			}

			speed.x = x;

			// Flip skeleton.
			if (x != 0) {
				skeletonAnimation.Skeleton.ScaleX = x > 0 ? 1f : -1f;
			}

			if (Input.GetKeyDown(jumpButton)) {
				if (isGrounded)
					StartCoroutine(FakeJump());
			}
				
			logicAnimator.SetFloat(horizontalSpeedProperty, Mathf.Abs(speed.x));
			logicAnimator.SetFloat(verticalSpeedProperty, speed.y);
			logicAnimator.SetBool(groundedProperty, isGrounded);
			
		}

		IEnumerator FakeJump () {
			// Rise
			isGrounded = false;
			speed.y = 10f;
			float durationLeft = jumpDuration * 0.5f;
			while (durationLeft > 0) {
				durationLeft -= Time.deltaTime;
				if (!Input.GetKey(jumpButton)) break;
				yield return null;
			}

			// Fall
			speed.y = -10f;
			float fallDuration = (jumpDuration * 0.5f) - durationLeft;
			yield return new WaitForSeconds(fallDuration);

			// Land
			speed.y = 0f;
			isGrounded = true;
			yield return null;
		}
	}

}
