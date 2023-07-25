/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
			foreach (SkeletonAnimation skeletonAnimation in skeletonAnimations)
				Destroy(skeletonAnimation.gameObject);

			skeletonAnimations.Clear();
			animationNameTable.Clear();
			animationSkeletonTable.Clear();
		}

		void SetActiveSkeleton (int index) {
			if (index < 0 || index >= skeletonAnimations.Count)
				SetActiveSkeleton(null);
			else
				SetActiveSkeleton(skeletonAnimations[index]);
		}

		void SetActiveSkeleton (SkeletonAnimation skeletonAnimation) {
			foreach (SkeletonAnimation iter in skeletonAnimations)
				iter.gameObject.SetActive(iter == skeletonAnimation);

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
		public List<SkeletonAnimation> SkeletonAnimations { get { return skeletonAnimations; } }

		public void Initialize (bool overwrite) {
			if (skeletonAnimations.Count != 0 && !overwrite) return;

			Clear();

			MeshGenerator.Settings settings = this.meshGeneratorSettings;
			Transform thisTransform = this.transform;
			foreach (SkeletonDataAsset dataAsset in skeletonDataAssets) {
				SkeletonAnimation newSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(dataAsset);
				newSkeletonAnimation.transform.SetParent(thisTransform, false);

				newSkeletonAnimation.SetMeshSettings(settings);
				newSkeletonAnimation.initialFlipX = this.initialFlipX;
				newSkeletonAnimation.initialFlipY = this.initialFlipY;
				Skeleton skeleton = newSkeletonAnimation.skeleton;
				skeleton.ScaleX = this.initialFlipX ? -1 : 1;
				skeleton.ScaleY = this.initialFlipY ? -1 : 1;

				newSkeletonAnimation.Initialize(false);
				skeletonAnimations.Add(newSkeletonAnimation);
			}

			// Build cache
			Dictionary<string, Animation> animationNameTable = this.animationNameTable;
			Dictionary<Animation, SkeletonAnimation> animationSkeletonTable = this.animationSkeletonTable;
			foreach (SkeletonAnimation skeletonAnimation in skeletonAnimations) {
				foreach (Animation animationObject in skeletonAnimation.Skeleton.Data.Animations) {
					animationNameTable[animationObject.Name] = animationObject;
					animationSkeletonTable[animationObject] = skeletonAnimation;
				}
			}

			SetActiveSkeleton(skeletonAnimations[0]);
			SetAnimation(initialAnimation, initialLoop);
		}

		public Animation FindAnimation (string animationName) {
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
				TrackEntry trackEntry = skeletonAnimation.state.SetAnimation(MainTrackIndex, animation, loop);
				skeletonAnimation.Update(0);
				return trackEntry;
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
