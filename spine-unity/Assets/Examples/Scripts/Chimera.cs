using UnityEngine;
using System.Collections;

public class Chimera : MonoBehaviour {

	public SkeletonDataAsset skeletonDataSource;

	[SpineAttachment(currentSkinOnly: false, returnFullPath: true, dataSource: "skeletonDataSource")]
	public string attachmentPath;

	[SpineSlot]
	public string targetSlot;

	void Start() {
		GetComponent<SkeletonRenderer>().skeleton.FindSlot(targetSlot).Attachment = SpineAttachment.GetAttachment(attachmentPath, skeletonDataSource);
	}
}
