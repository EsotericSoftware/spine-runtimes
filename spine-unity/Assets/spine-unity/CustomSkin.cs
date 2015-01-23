using UnityEngine;
using System.Collections;
using Spine;

public class CustomSkin : MonoBehaviour {


	[System.Serializable]
	public class SkinPair {
		[SpineAttachment(currentSkinOnly: false, returnAttachmentPath: true, dataField: "skinSource")]
		public string sourceAttachment;
		[SpineSlot]
		public string targetSlot;
		[SpineAttachment(currentSkinOnly: true, placeholdersOnly: true)]
		public string targetAttachment;
	}

	public SkeletonDataAsset skinSource;
	public SkinPair[] skinning;
	public Skin customSkin;

	SkeletonRenderer skeletonRenderer;
	void Start() {
		skeletonRenderer = GetComponent<SkeletonRenderer>();
		Skeleton skeleton = skeletonRenderer.skeleton;

		customSkin = new Skin("CustomSkin");

		foreach (var pair in skinning) {
			var attachment = SpineAttachment.GetAttachment(pair.sourceAttachment, skinSource);
			customSkin.AddAttachment(skeleton.FindSlotIndex(pair.targetSlot), pair.targetAttachment, attachment);
		}

		skeleton.SetSkin(customSkin);
	}
}
