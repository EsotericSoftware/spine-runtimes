using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class DataAssetsFromExportsExample : MonoBehaviour {

		public TextAsset skeletonJson;
		public TextAsset atlasText;
		public Texture2D[] textures;
		public Material materialPropertySource;

		AtlasAsset runtimeAtlasAsset;
		SkeletonDataAsset runtimeSkeletonDataAsset;
		SkeletonAnimation runtimeSkeletonAnimation;

		void CreateRuntimeAssetsAndGameObject () {
			// 1. Create the AtlasAsset (needs atlas text asset and textures, and materials/shader);
			// 2. Create SkeletonDataAsset (needs json or binary asset file, and an AtlasAsset)
			// 3. Create SkeletonAnimation (needs a valid SkeletonDataAsset)

			runtimeAtlasAsset = AtlasAsset.CreateRuntimeInstance(atlasText, textures, materialPropertySource, true);
			runtimeSkeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(skeletonJson, runtimeAtlasAsset, true);
		}

		IEnumerator Start () {
			CreateRuntimeAssetsAndGameObject();
			runtimeSkeletonDataAsset.GetSkeletonData(false); // preload.
			yield return new WaitForSeconds(0.5f);

			runtimeSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(runtimeSkeletonDataAsset);

			// Extra Stuff
			runtimeSkeletonAnimation.Initialize(false);
			runtimeSkeletonAnimation.Skeleton.SetSkin("base");
			runtimeSkeletonAnimation.Skeleton.SetSlotsToSetupPose();
			runtimeSkeletonAnimation.AnimationState.SetAnimation(0, "run", true);
			runtimeSkeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = 10;
			runtimeSkeletonAnimation.transform.Translate(Vector3.down * 2);

		}
	}

}
