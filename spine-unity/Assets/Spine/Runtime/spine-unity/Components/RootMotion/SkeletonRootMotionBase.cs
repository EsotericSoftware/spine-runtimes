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
using System.Collections.Generic;
using Spine.Unity.AnimationTools;

namespace Spine.Unity {

	/// <summary>
	/// Base class for skeleton root motion components.
	/// </summary>
	abstract public class SkeletonRootMotionBase : MonoBehaviour {
		#region Inspector

		[SpineBone]
		[SerializeField]
		protected string rootMotionBoneName = "root";
		public bool transformPositionX = true;
		public bool transformPositionY = true;

		[Header("Optional")]
		public Rigidbody2D rigidBody2D;
		public Rigidbody rigidBody;

		public bool UsesRigidbody {
			get { return rigidBody != null || rigidBody2D != null; }
		}
		#endregion

		protected ISkeletonComponent skeletonComponent;
		protected Bone rootMotionBone;
		protected int rootMotionBoneIndex;
		protected List<Bone> topLevelBones = new List<Bone>();
		protected Vector2 rigidbodyDisplacement;

		protected virtual void Reset () {
			FindRigidbodyComponent();
		}

		protected virtual void Start () {
			skeletonComponent = GetComponent<ISkeletonComponent>();
			GatherTopLevelBones();
			SetRootMotionBone(rootMotionBoneName);

			var skeletonAnimation = skeletonComponent as ISkeletonAnimation;
			if (skeletonAnimation != null)
				skeletonAnimation.UpdateLocal += HandleUpdateLocal;
		}

		abstract protected Vector2 CalculateAnimationsMovementDelta ();

		protected virtual float AdditionalScale { get { return 1.0f; } }

		protected Vector2 GetTimelineMovementDelta (float startTime, float endTime,
			TranslateTimeline timeline, Animation animation) {

			Vector2 currentDelta;
			if (startTime > endTime) // Looped
				currentDelta = (timeline.Evaluate(animation.duration) - timeline.Evaluate(startTime))
					+ (timeline.Evaluate(endTime) - timeline.Evaluate(0));
			else if (startTime != endTime) // Non-looped
				currentDelta = timeline.Evaluate(endTime) - timeline.Evaluate(startTime);
			else
				currentDelta = Vector2.zero;
			return currentDelta;
		}

		void GatherTopLevelBones () {
			topLevelBones.Clear();
			var skeleton = skeletonComponent.Skeleton;
			foreach (var bone in skeleton.Bones) {
				if (bone.Parent == null)
					topLevelBones.Add(bone);
			}
		}

		public void SetRootMotionBone (string name) {
			var skeleton = skeletonComponent.Skeleton;
			int index = skeleton.FindBoneIndex(name);
			if (index >= 0) {
				this.rootMotionBoneIndex = index;
				this.rootMotionBone = skeleton.bones.Items[index];
			}
			else {
				Debug.Log("Bone named \"" + name + "\" could not be found.");
				this.rootMotionBoneIndex = 0;
				this.rootMotionBone = skeleton.RootBone;
			}
		}

		void HandleUpdateLocal (ISkeletonAnimation animatedSkeletonComponent) {
			if (!this.isActiveAndEnabled)
				return; // Root motion is only applied when component is enabled.

			var movementDelta = CalculateAnimationsMovementDelta();
			AdjustMovementDeltaToConfiguration(ref movementDelta, animatedSkeletonComponent.Skeleton);
			ApplyRootMotion(movementDelta);
		}

		void AdjustMovementDeltaToConfiguration (ref Vector2 localDelta, Skeleton skeleton) {
			if (skeleton.ScaleX < 0) localDelta.x = -localDelta.x;
			if (skeleton.ScaleY < 0) localDelta.y = -localDelta.y;
			if (!transformPositionX) localDelta.x = 0f;
			if (!transformPositionY) localDelta.y = 0f;
		}

		void ApplyRootMotion (Vector2 localDelta) {
			localDelta *= AdditionalScale;
			// Apply root motion to Transform or RigidBody;
			if (UsesRigidbody) {
				rigidbodyDisplacement += (Vector2)transform.TransformVector(localDelta);
				// Accumulated displacement is applied on the next Physics update (FixedUpdate)
			}
			else {

				transform.position += transform.TransformVector(localDelta);
			}

			// Move top level bones in opposite direction of the root motion bone
			foreach (var topLevelBone in topLevelBones) {
				if (transformPositionX) topLevelBone.x -= rootMotionBone.x;
				if (transformPositionY) topLevelBone.y -= rootMotionBone.y;
			}
		}

		protected virtual void FixedUpdate () {
			if (!this.isActiveAndEnabled)
				return; // Root motion is only applied when component is enabled.

			if(rigidBody2D != null) {
				rigidBody2D.MovePosition(new Vector2(transform.position.x, transform.position.y)
					+ rigidbodyDisplacement);
			}
			if (rigidBody != null) {
				rigidBody.MovePosition(transform.position
					+ new Vector3(rigidbodyDisplacement.x, rigidbodyDisplacement.y, 0));
			}
			rigidbodyDisplacement = Vector2.zero;
		}

		protected virtual void OnDisable () {
			rigidbodyDisplacement = Vector2.zero;
		}

		protected void FindRigidbodyComponent () {
			rigidBody2D = this.GetComponent<Rigidbody2D>();
			if (!rigidBody2D)
				rigidBody = this.GetComponent<Rigidbody>();

			if (!rigidBody2D && !rigidBody) {
				rigidBody2D = this.GetComponentInParent<Rigidbody2D>();
				if (!rigidBody2D)
					rigidBody = this.GetComponentInParent<Rigidbody>();
			}
		}
	}
}
