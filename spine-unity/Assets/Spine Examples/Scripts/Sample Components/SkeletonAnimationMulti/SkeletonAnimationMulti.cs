/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine;
using Spine.Unity;

namespace Spine.Unity {

	using Animation = Spine.Animation;
	using AnimationState = Spine.AnimationState;

	public class SkeletonAnimationMulti : MonoBehaviour {
		const int MainTrackIndex = 0;

		public bool initialFlipX, initialFlipY;
		public string initialAnimation;
		public bool initialLoop;
		[Space]
		public List<SkeletonDataAsset> skeletonDataAssets = new List<SkeletonDataAsset>();
		[Header("Settings")]
		public MeshGenerator.Settings meshGeneratorSettings = MeshGenerator.Settings.Default;

		readonly List<SkeletonAnimation> skeletonAnimations = new List<SkeletonAnimation>();
		readonly Dictionary<string, Animation> animationNameTable = new Dictionary<string, Animation>();
		readonly Dictionary<Animation, SkeletonAnimation> animationSkeletonTable = new Dictionary<Animation, SkeletonAnimation>();
		//Stateful
		SkeletonAnimation currentSkeletonAnimation;

		void Clear () {
			foreach (var s in skeletonAnimations)
				Destroy(s.gameObject);

			skeletonAnimations.Clear();
			animationNameTable.Clear();
			animationSkeletonTable.Clear();
		}

		void SetActiveSkeleton (SkeletonAnimation skeletonAnimation) {
			foreach (var sa in skeletonAnimations)
				sa.gameObject.SetActive(sa == skeletonAnimation);

			currentSkeletonAnimation = skeletonAnimation;
		}

		#region Lifecycle
		void Awake () {
			Initialize(false);
		}
		#endregion

		#region API
		public Dictionary<Animation, SkeletonAnimation> AnimationSkeletonTable { get { return this.animationSkeletonTable; } }
		public Dictionary<string, Animation> AnimationNameTable { get { return this.animationNameTable; } }
		public SkeletonAnimation CurrentSkeletonAnimation { get { return this.currentSkeletonAnimation; } }

		public void Initialize (bool overwrite) {
			if (skeletonAnimations.Count != 0 && !overwrite) return;

			Clear();

			var settings = this.meshGeneratorSettings;
			Transform thisTransform = this.transform;
			foreach (var sda in skeletonDataAssets) {
				var sa = SkeletonAnimation.NewSkeletonAnimationGameObject(sda);
				sa.transform.SetParent(thisTransform, false);

				sa.SetMeshSettings(settings);
				sa.initialFlipX = this.initialFlipX;
				sa.initialFlipY = this.initialFlipY;
				var skeleton = sa.skeleton;
				skeleton.ScaleX = this.initialFlipX ? -1 : 1;
				skeleton.ScaleY = this.initialFlipY ? -1 : 1;

				sa.Initialize(false);
				skeletonAnimations.Add(sa);
			}

			// Build cache
			var animationNameTable = this.animationNameTable;
			var animationSkeletonTable = this.animationSkeletonTable;
			foreach (var skeletonAnimation in skeletonAnimations) {
				foreach (var animationObject in skeletonAnimation.Skeleton.Data.Animations) {
					animationNameTable[animationObject.Name] = animationObject;
					animationSkeletonTable[animationObject] = skeletonAnimation;
				}
			}

			SetActiveSkeleton(skeletonAnimations[0]);
			SetAnimation(initialAnimation, initialLoop);
		}

		public Animation FindAnimation (string animationName) {
			// Analysis disable once LocalVariableHidesMember
			Animation animation;
			animationNameTable.TryGetValue(animationName, out animation);
			return animation;
		}

		public TrackEntry SetAnimation (string animationName, bool loop) {
			return SetAnimation(FindAnimation(animationName), loop);
		}

		public TrackEntry SetAnimation (Animation animation, bool loop) {
			if (animation == null) return null;

			SkeletonAnimation skeletonAnimation;
			animationSkeletonTable.TryGetValue(animation, out skeletonAnimation);

			if (skeletonAnimation != null) {
				SetActiveSkeleton(skeletonAnimation);
				skeletonAnimation.skeleton.SetToSetupPose();
				return skeletonAnimation.state.SetAnimation(MainTrackIndex, animation, loop);
			}

			return null;
		}

		public void SetEmptyAnimation (float mixDuration) {
			currentSkeletonAnimation.state.SetEmptyAnimation(MainTrackIndex, mixDuration);
		}

		public void ClearAnimation () {
			currentSkeletonAnimation.state.ClearTrack(MainTrackIndex);
		}

		public TrackEntry GetCurrent () {
			return currentSkeletonAnimation.state.GetCurrent(MainTrackIndex);
		}
		#endregion
	}
}
