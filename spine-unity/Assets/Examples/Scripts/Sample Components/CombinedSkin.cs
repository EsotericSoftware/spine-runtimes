using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity.Modules.AttachmentTools; 

namespace Spine.Unity.Examples {
	public class CombinedSkin : MonoBehaviour {
		[SpineSkin]
		public List<string> skinsToCombine;

		Skin combinedSkin;

		void Start () {
			var skeletonComponent = GetComponent<ISkeletonComponent>();
			if (skeletonComponent == null) return;
			var skeleton = skeletonComponent.Skeleton;
			if (skeleton == null) return;

			combinedSkin = combinedSkin ?? new Skin("combined");
			combinedSkin.Clear();
			foreach (var skinName in skinsToCombine) {
				var skin = skeleton.Data.FindSkin(skinName);
				if (skin != null) combinedSkin.Append(skin);
			}

			skeleton.SetSkin(combinedSkin);
			skeleton.SetToSetupPose();
			var animationStateComponent = skeletonComponent as IAnimationStateComponent;
			if (animationStateComponent != null) animationStateComponent.AnimationState.Apply(skeleton);
		}
	}

}
