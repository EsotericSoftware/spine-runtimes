﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine;
using Spine.Unity;

namespace Spine.Unity.Examples {
	public class SpineboyFootplanter : MonoBehaviour {

		public float timeScale = 0.5f;
		[SpineBone]	public string nearBoneName, farBoneName;

		[Header("Settings")]
		public Vector2 footSize;
		public float footRayRaise = 2f;
		public float comfyDistance = 1f;
		public float centerOfGravityXOffset = -0.25f;
		public float feetTooFarApartThreshold = 3f;
		public float offBalanceThreshold = 1.4f;
		public float minimumSpaceBetweenFeet = 0.5f;
		public float maxNewStepDisplacement = 2f;
		public float shuffleDistance = 1f;
		public float baseLerpSpeed = 3.5f;
		public FootMovement forward, backward;

		[Header("Debug")]
		[SerializeField] float balance;
		[SerializeField] float distanceBetweenFeet;
		[SerializeField] Foot nearFoot, farFoot;

		Skeleton skeleton;
		Bone nearFootBone, farFootBone;

		[System.Serializable]
		public class FootMovement {
			public AnimationCurve xMoveCurve;
			public AnimationCurve raiseCurve;
			public float maxRaise;
			public float minDistanceCompensate;
			public float maxDistanceCompensate;
		}

		[System.Serializable]
		public class Foot {
			public Vector2 worldPos;
			public float displacementFromCenter;
			public float distanceFromCenter;

			[Space]
			public float lerp;
			public Vector2 worldPosPrev;
			public Vector2 worldPosNext;

			public bool IsStepInProgress { get { return lerp < 1f; } }
			public bool IsPrettyMuchDoneStepping { get { return lerp > 0.7f; } }

			public void UpdateDistance (float centerOfGravityX) {
				displacementFromCenter = worldPos.x - centerOfGravityX;
				distanceFromCenter = Mathf.Abs(displacementFromCenter);
			}

			public void StartNewStep (float newDistance, float centerOfGravityX, float tentativeY, float footRayRaise, RaycastHit2D[] hits, Vector2 footSize) {
				lerp = 0f;
				worldPosPrev = worldPos;
				float newX = centerOfGravityX - newDistance;
				Vector2 origin = new Vector2(newX, tentativeY + footRayRaise);
				//int hitCount = Physics2D.BoxCastNonAlloc(origin, footSize, 0f, Vector2.down, hits);
				int hitCount = Physics2D.BoxCast(origin, footSize, 0f, Vector2.down, new ContactFilter2D { useTriggers = false }, hits);
				worldPosNext = hitCount > 0 ? hits[0].point : new Vector2(newX, tentativeY);
			}

			public void UpdateStepProgress (float deltaTime, float stepSpeed, float shuffleDistance, FootMovement forwardMovement, FootMovement backwardMovement) {
				if (!this.IsStepInProgress)
					return;

				lerp += deltaTime * stepSpeed;

				float strideSignedSize = worldPosNext.x - worldPosPrev.x;
				float strideSign = Mathf.Sign(strideSignedSize);
				float strideSize = (Mathf.Abs(strideSignedSize));

				var movement = strideSign > 0 ? forwardMovement : backwardMovement;

				worldPos.x = Mathf.Lerp(worldPosPrev.x, worldPosNext.x, movement.xMoveCurve.Evaluate(lerp));
				float groundLevel = Mathf.Lerp(worldPosPrev.y, worldPosNext.y, lerp);

				if (strideSize > shuffleDistance) {
					float strideSizeFootRaise = Mathf.Clamp((strideSize * 0.5f), 1f, 2f);
					worldPos.y = groundLevel + (movement.raiseCurve.Evaluate(lerp) * movement.maxRaise * strideSizeFootRaise);
				} else {
					lerp += Time.deltaTime;
					worldPos.y = groundLevel;
				}

				if (lerp > 1f)
					lerp = 1f;
			}

			public static float GetNewDisplacement (float otherLegDisplacementFromCenter, float comfyDistance, float minimumFootDistanceX, float maxNewStepDisplacement, FootMovement forwardMovement, FootMovement backwardMovement) {
				var movement = Mathf.Sign(otherLegDisplacementFromCenter) < 0 ? forwardMovement : backwardMovement;
				float randomCompensate = Random.Range(movement.minDistanceCompensate, movement.maxDistanceCompensate);

				float newDisplacement = (otherLegDisplacementFromCenter * randomCompensate);
				if (Mathf.Abs(newDisplacement) > maxNewStepDisplacement || Mathf.Abs(otherLegDisplacementFromCenter) < minimumFootDistanceX)
					newDisplacement = comfyDistance * Mathf.Sign(newDisplacement) * randomCompensate;

				return newDisplacement;
			}

		}

		public float Balance { get { return balance; } }

		void Start () {
			Time.timeScale = timeScale;
			var tpos = transform.position;

			// Default starting positions.
			nearFoot.worldPos = tpos;
			nearFoot.worldPos .x -= comfyDistance;
			nearFoot.worldPosPrev = nearFoot.worldPosNext = nearFoot.worldPos;

			farFoot.worldPos = tpos;
			farFoot.worldPos.x += comfyDistance;
			farFoot.worldPosPrev = farFoot.worldPosNext = farFoot.worldPos;

			var skeletonAnimation = GetComponent<SkeletonAnimation>();
			skeleton = skeletonAnimation.Skeleton;

			skeletonAnimation.UpdateLocal += UpdateLocal;

			nearFootBone = skeleton.FindBone(nearBoneName);
			farFootBone = skeleton.FindBone(farBoneName);

			nearFoot.lerp = 1f;
			farFoot.lerp = 1f;
		}

		RaycastHit2D[] hits = new RaycastHit2D[1];

		private void UpdateLocal (ISkeletonAnimation animated) {
			Transform thisTransform = transform;

			Vector2 thisTransformPosition = thisTransform.position;
			float centerOfGravityX = thisTransformPosition.x + centerOfGravityXOffset;

			nearFoot.UpdateDistance(centerOfGravityX);
			farFoot.UpdateDistance(centerOfGravityX);
			balance = nearFoot.displacementFromCenter + farFoot.displacementFromCenter;
			distanceBetweenFeet = Mathf.Abs(nearFoot.worldPos.x - farFoot.worldPos.x);

			// Detect time to make a new step
			bool isTooOffBalance = Mathf.Abs(balance) > offBalanceThreshold;
			bool isFeetTooFarApart = distanceBetweenFeet > feetTooFarApartThreshold;
			bool timeForNewStep = isFeetTooFarApart || isTooOffBalance;
			if (timeForNewStep) {

				// Choose which foot to use for next step.
				Foot stepFoot, otherFoot;
				bool stepLegIsNearLeg = nearFoot.distanceFromCenter > farFoot.distanceFromCenter;
				if (stepLegIsNearLeg) {
					stepFoot = nearFoot;
					otherFoot = farFoot;
				} else {
					stepFoot = farFoot;
					otherFoot = nearFoot;
				}

				// Start a new step.
				if (!stepFoot.IsStepInProgress && otherFoot.IsPrettyMuchDoneStepping) {
					float newDisplacement = Foot.GetNewDisplacement(otherFoot.displacementFromCenter, comfyDistance, minimumSpaceBetweenFeet, maxNewStepDisplacement, forward, backward);
					stepFoot.StartNewStep(newDisplacement, centerOfGravityX, thisTransformPosition.y, footRayRaise, hits, footSize);
				}

			}


			float deltaTime = Time.deltaTime;
			float stepSpeed = baseLerpSpeed;
			stepSpeed += (Mathf.Abs(balance) - 0.6f) * 2.5f;

			// Animate steps that are in progress.
			nearFoot.UpdateStepProgress(deltaTime, stepSpeed, shuffleDistance, forward, backward);
			farFoot.UpdateStepProgress(deltaTime, stepSpeed, shuffleDistance, forward, backward);

			nearFootBone.SetPosition(thisTransform.InverseTransformPoint(nearFoot.worldPos));
			farFootBone.SetPosition(thisTransform.InverseTransformPoint(farFoot.worldPos));
		}



		void OnDrawGizmos () {
			if (Application.isPlaying) {
				const float Radius = 0.15f;

				Gizmos.color = Color.green;
				Gizmos.DrawSphere(nearFoot.worldPos, Radius);
				Gizmos.DrawWireSphere(nearFoot.worldPosNext, Radius);

				Gizmos.color = Color.magenta;
				Gizmos.DrawSphere(farFoot.worldPos, Radius);
				Gizmos.DrawWireSphere(farFoot.worldPosNext, Radius);
			}
		}

	}
}
