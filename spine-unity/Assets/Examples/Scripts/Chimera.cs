

/*****************************************************************************
 * Basic Platformer Controller created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using UnityEngine;
using System.Collections;

public class Chimera : MonoBehaviour {

	public SkeletonDataAsset skeletonDataSource;

	[SpineAttachment(currentSkinOnly: false, returnAttachmentPath: true, dataField: "skeletonDataSource")]
	public string attachmentPath;

	[SpineSlot]
	public string targetSlot;

	void Start() {
		GetComponent<SkeletonRenderer>().skeleton.FindSlot(targetSlot).Attachment = SpineAttachment.GetAttachment(attachmentPath, skeletonDataSource);
	}
}