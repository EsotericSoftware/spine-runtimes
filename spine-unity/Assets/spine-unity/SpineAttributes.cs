using UnityEngine;
using System.Collections;

public class SpineSlot : PropertyAttribute {
	public string startsWith = "";
	public string dataSource = "";

	/// <summary>
	/// 
	/// </summary>
	/// <param name="startsWith"></param>
	/// <param name="dataSource">SerializedProperty name containing a reference to either a SkeletonRenderer or a SkeletonDataAsset</param>
	public SpineSlot(string startsWith = "", string dataSource = "") {
		this.startsWith = startsWith;
		this.dataSource = dataSource;
	}
}

public class SpineSkin : PropertyAttribute {
	public string startsWith = "";
	public string dataSource = "";

	/// <summary>
	/// 
	/// </summary>
	/// <param name="startsWith"></param>
	/// <param name="dataSource">SerializedProperty name containing a reference to either a SkeletonRenderer or a SkeletonDataAsset</param>
	public SpineSkin(string startsWith = "", string dataSource = "") {
		this.startsWith = startsWith;
		this.dataSource = dataSource;
	}
}

public class SpineAtlasRegion : PropertyAttribute {

}

public class SpineAnimation : PropertyAttribute {
	public string startsWith = "";
	public string dataSource = "";

	/// <summary>
	/// 
	/// </summary>
	/// <param name="startsWith"></param>
	/// <param name="dataSource">SerializedProperty name containing a reference to either a SkeletonRenderer or a SkeletonDataAsset</param>
	public SpineAnimation(string startsWith = "", string dataSource = "") {
		this.startsWith = startsWith;
		this.dataSource = dataSource;
	}
}

public class SpineAttachment : PropertyAttribute {
	public bool returnFullPath;
	public bool currentSkinOnly;
	public string dataSource = "";
	public string slotSource = "";


	public SpineAttachment() {

	}

	public SpineAttachment(bool currentSkinOnly = true, bool returnFullPath = false, string slot = "", string dataSource = "") {
		this.currentSkinOnly = currentSkinOnly;
		this.returnFullPath = returnFullPath;
		this.slotSource = slot;
		this.dataSource = dataSource;		
	}

	public static Hierarchy GetHierarchy(string fullPath) {
		return new Hierarchy(fullPath);
	}

	public static Spine.Attachment GetAttachment(string fullPath, Spine.SkeletonData skeletonData) {
		var hierarchy = SpineAttachment.GetHierarchy(fullPath);
		if (hierarchy.name == "")
			return null;

		return skeletonData.FindSkin(hierarchy.skin).GetAttachment(skeletonData.FindSlotIndex(hierarchy.slot), hierarchy.name);
	}

	public static Spine.Attachment GetAttachment(string fullPath, SkeletonDataAsset skeletonDataAsset) {
		return GetAttachment(fullPath, skeletonDataAsset.GetSkeletonData(true));
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