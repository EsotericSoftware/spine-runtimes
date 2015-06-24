

/*****************************************************************************
 * Spine Attributes created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using UnityEngine;
using System.Collections;

public class SpineSlot : PropertyAttribute {
	public string startsWith = "";
	public string dataField = "";
	public bool containsBoundingBoxes = false;

	/// <summary>
	/// Smart popup menu for Spine Slots
	/// </summary>
	/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
	/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
	/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives).
	/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
	/// </param>
	/// <param name="containsBoundingBoxes">Disables popup results that don't contain bounding box attachments when true.</param>
	public SpineSlot(string startsWith = "", string dataField = "", bool containsBoundingBoxes = false) {
		this.startsWith = startsWith;
		this.dataField = dataField;
		this.containsBoundingBoxes = containsBoundingBoxes;
	}
}

public class SpineSkin : PropertyAttribute {
	public string startsWith = "";
	public string dataField = "";

	/// <summary>
	/// Smart popup menu for Spine Skins
	/// </summary>
	/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
	/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
	/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives)
	/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
	/// </param>
	public SpineSkin(string startsWith = "", string dataField = "") {
		this.startsWith = startsWith;
		this.dataField = dataField;
	}
}
public class SpineAnimation : PropertyAttribute {
	public string startsWith = "";
	public string dataField = "";

	/// <summary>
	/// Smart popup menu for Spine Animations
	/// </summary>
	/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
	/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
	/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives)
	/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
	/// </param>
	public SpineAnimation(string startsWith = "", string dataField = "") {
		this.startsWith = startsWith;
		this.dataField = dataField;
	}
}

public class SpineAttachment : PropertyAttribute {
	public bool returnAttachmentPath = false;
	public bool currentSkinOnly = false;
	public bool placeholdersOnly = false;
	public string dataField = "";
	public string slotField = "";


	public SpineAttachment() {

	}

	/// <summary>
	/// Smart popup menu for Spine Attachments
	/// </summary>
	/// <param name="currentSkinOnly">Filters popup results to only include the current Skin.  Only valid when a SkeletonRenderer is the data source.</param>
	/// <param name="returnAttachmentPath">Returns a fully qualified path for an Attachment in the format "Skin/Slot/AttachmentName"</param>
	/// <param name="placeholdersOnly">Filters popup results to exclude attachments that are not children of Skin Placeholders</param>
	/// <param name="slotField">If specified, a locally scoped field with the name supplied by in slotField will be used to limit the popup results to children of a named slot</param>
	/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
	/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives)
	/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
	/// </param>
	public SpineAttachment(bool currentSkinOnly = true, bool returnAttachmentPath = false, bool placeholdersOnly = false, string slotField = "", string dataField = "") {
		this.currentSkinOnly = currentSkinOnly;
		this.returnAttachmentPath = returnAttachmentPath;
		this.placeholdersOnly = placeholdersOnly;
		this.slotField = slotField;
		this.dataField = dataField;		
	}

	public static Hierarchy GetHierarchy(string fullPath) {
		return new Hierarchy(fullPath);
	}

	public static Spine.Attachment GetAttachment(string attachmentPath, Spine.SkeletonData skeletonData) {
		var hierarchy = SpineAttachment.GetHierarchy(attachmentPath);
		if (hierarchy.name == "")
			return null;

		return skeletonData.FindSkin(hierarchy.skin).GetAttachment(skeletonData.FindSlotIndex(hierarchy.slot), hierarchy.name);
	}

	public static Spine.Attachment GetAttachment(string attachmentPath, SkeletonDataAsset skeletonDataAsset) {
		return GetAttachment(attachmentPath, skeletonDataAsset.GetSkeletonData(true));
	}

	public struct Hierarchy {
		public string skin;
		public string slot;
		public string name;

		public Hierarchy(string fullPath) {
			string[] chunks = fullPath.Split(new char[]{'/'}, System.StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length == 0) {
				skin = "";
				slot = "";
				name = "";
				return;
			}
			else if (chunks.Length < 2) {
				throw new System.Exception("Cannot generate Attachment Hierarchy from string! Not enough components! [" + fullPath + "]");
			}
			skin = chunks[0];
			slot = chunks[1];
			name = "";
			for (int i = 2; i < chunks.Length; i++) {
				name += chunks[i];
			}
		}
	}
}

public class SpineBone : PropertyAttribute {
	public string startsWith = "";
	public string dataField = "";

	/// <summary>
	/// Smart popup menu for Spine Bones
	/// </summary>
	/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
	/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
	/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives)
	/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
	/// </param>
	public SpineBone(string startsWith = "", string dataField = "") {
		this.startsWith = startsWith;
		this.dataField = dataField;
	}

	public static Spine.Bone GetBone(string boneName, SkeletonRenderer renderer) {
		if (renderer.skeleton == null)
			return null;

		return renderer.skeleton.FindBone(boneName);
	}

	public static Spine.BoneData GetBoneData(string boneName, SkeletonDataAsset skeletonDataAsset) {
		var data = skeletonDataAsset.GetSkeletonData(true);

		return data.FindBone(boneName);
	}
}

public class SpineAtlasRegion : PropertyAttribute {
	//TODO:  Standardize with Skeleton attributes
	//NOTE:  For now, relies on locally scoped field named "atlasAsset" for source.
}