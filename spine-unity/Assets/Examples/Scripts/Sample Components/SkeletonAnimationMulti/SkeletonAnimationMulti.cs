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
				skeleton.FlipX = this.initialFlipX;
				skeleton.FlipY = this.initialFlipY;

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
