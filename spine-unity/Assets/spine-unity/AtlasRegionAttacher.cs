using UnityEngine;
using System.Collections;
using Spine;


public class AtlasRegionAttacher : MonoBehaviour {

	[System.Serializable]
	public class SlotRegionPair {
		[SpineSlot]
		public string slot;

		[SpineAtlasRegion]
		public string region;
	}

	public AtlasAsset atlasAsset;
	public SlotRegionPair[] attachments;

	[HideInInspector]
	public SkeletonRenderer skeletonRenderer;


	Atlas atlas;

	void Start() {
		atlas = atlasAsset.GetAtlas();
		this.skeletonRenderer = GetComponent<SkeletonRenderer>();

		AtlasAttachmentLoader loader = new AtlasAttachmentLoader(atlas);

		float scaleMultiplier = skeletonRenderer.skeletonDataAsset.scale;

		var enumerator = attachments.GetEnumerator();
		while (enumerator.MoveNext()) {
			var entry = (SlotRegionPair)enumerator.Current;
			var regionAttachment = loader.NewRegionAttachment(null, entry.region, entry.region);
			regionAttachment.Width = regionAttachment.RegionOriginalWidth * scaleMultiplier;
			regionAttachment.Height = regionAttachment.RegionOriginalHeight * scaleMultiplier;

			regionAttachment.SetColor(new Color(1, 1, 1, 1));
			regionAttachment.UpdateOffset();

			var slot = this.skeletonRenderer.skeleton.FindSlot(entry.slot);
			slot.Attachment = regionAttachment;
		}
	}

}
