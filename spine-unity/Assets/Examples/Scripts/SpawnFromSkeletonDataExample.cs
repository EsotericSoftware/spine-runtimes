using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SpawnFromSkeletonDataExample : MonoBehaviour {

		public SkeletonDataAsset skeletonDataAsset;
		[Range(0, 100)]
		public int count = 20;

		[SpineAnimation(dataField:"skeletonDataAsset")]
		public string startingAnimation;

		IEnumerator Start () {
			if (skeletonDataAsset == null) yield break;
			skeletonDataAsset.GetSkeletonData(false); // Preload SkeletonDataAsset.
			yield return new WaitForSeconds(1f); // Pretend stuff is happening.

			var spineAnimation = skeletonDataAsset.GetSkeletonData(false).FindAnimation(startingAnimation);
			for (int i = 0; i < count; i++) {
				var sa = SkeletonAnimation.NewSkeletonAnimationGameObject(skeletonDataAsset); // Spawn a new SkeletonAnimation GameObject.
				DoExtraStuff(sa, spineAnimation); // optional stuff for fun.
				sa.gameObject.name = i.ToString();
				yield return new WaitForSeconds(1f/8f);
			}

		}

		void DoExtraStuff (SkeletonAnimation sa, Spine.Animation spineAnimation) {
			sa.transform.localPosition = Random.insideUnitCircle * 6f;
			sa.transform.SetParent(this.transform, false);

			if (spineAnimation != null) {
				sa.Initialize(false);
				sa.AnimationState.SetAnimation(0, spineAnimation, true);
			}
		}

	}

}
