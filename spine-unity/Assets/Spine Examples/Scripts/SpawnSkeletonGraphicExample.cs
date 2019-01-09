using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SpawnSkeletonGraphicExample : MonoBehaviour {

		public SkeletonDataAsset skeletonDataAsset;

		[SpineAnimation(dataField: "skeletonDataAsset")]
		public string startingAnimation;

		[SpineSkin(dataField: "skeletonDataAsset")]
		public string startingSkin = "base";
		public Material skeletonGraphicMaterial;
		
		IEnumerator Start () {
			if (skeletonDataAsset == null) yield break;
			skeletonDataAsset.GetSkeletonData(false); // Preload SkeletonDataAsset.
			yield return new WaitForSeconds(1f); // Pretend stuff is happening.

			var sg = SkeletonGraphic.NewSkeletonGraphicGameObject(skeletonDataAsset, this.transform, skeletonGraphicMaterial); // Spawn a new SkeletonGraphic GameObject.
			sg.gameObject.name = "SkeletonGraphic Instance";

			// Extra Stuff
			sg.Initialize(false);
			sg.Skeleton.SetSkin(startingSkin);
			sg.Skeleton.SetSlotsToSetupPose();
			sg.AnimationState.SetAnimation(0, startingAnimation, true);
		}
	}

}
