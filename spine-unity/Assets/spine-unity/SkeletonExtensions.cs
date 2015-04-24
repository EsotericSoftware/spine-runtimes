

/*****************************************************************************
 * Spine Extensions created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using System.Collections;
using Spine;

public static class SkeletonExtensions {

	public static void SetColor (this Skeleton skeleton, Color color) {
		skeleton.A = color.a;
		skeleton.R = color.r;
		skeleton.G = color.g;
		skeleton.B = color.b;
	}
	
	public static void SetColor (this Skeleton skeleton, Color32 color) {
		skeleton.A = color.a / 255f;
		skeleton.R = color.r / 255f;
		skeleton.G = color.g / 255f;
		skeleton.B = color.b / 255f;
	}

	public static void SetColor (this Slot slot, Color color) {
		slot.A = color.a;
		slot.R = color.r;
		slot.G = color.g;
		slot.B = color.b;
	}

	public static void SetColor (this Slot slot, Color32 color) {
		slot.A = color.a / 255f;
		slot.R = color.r / 255f;
		slot.G = color.g / 255f;
		slot.B = color.b / 255f;
	}

	public static void SetColor (this RegionAttachment attachment, Color color) {
		attachment.A = color.a;
		attachment.R = color.r;
		attachment.G = color.g;
		attachment.B = color.b;
	}

	public static void SetColor (this RegionAttachment attachment, Color32 color) {
		attachment.A = color.a / 255f;
		attachment.R = color.r / 255f;
		attachment.G = color.g / 255f;
		attachment.B = color.b / 255f;
	}

	public static void SetColor (this MeshAttachment attachment, Color color) {
		attachment.A = color.a;
		attachment.R = color.r;
		attachment.G = color.g;
		attachment.B = color.b;
	}

	public static void SetColor (this MeshAttachment attachment, Color32 color) {
		attachment.A = color.a / 255f;
		attachment.R = color.r / 255f;
		attachment.G = color.g / 255f;
		attachment.B = color.b / 255f;
	}

	public static void SetColor (this SkinnedMeshAttachment attachment, Color color) {
		attachment.A = color.a;
		attachment.R = color.r;
		attachment.G = color.g;
		attachment.B = color.b;
	}

	public static void SetColor (this SkinnedMeshAttachment attachment, Color32 color) {
		attachment.A = color.a / 255f;
		attachment.R = color.r / 255f;
		attachment.G = color.g / 255f;
		attachment.B = color.b / 255f;
	}

	public static void SetPosition (this Bone bone, Vector2 position) {
		bone.X = position.x;
		bone.Y = position.y;
	}

	public static void SetPosition (this Bone bone, Vector3 position) {
		bone.X = position.x;
		bone.Y = position.y;
	}

	public static Attachment AttachUnitySprite (this Skeleton skeleton, string slotName, Sprite sprite, string shaderName = "Spine/Skeleton") {
		var att = sprite.ToRegionAttachment(shaderName);
		skeleton.FindSlot(slotName).Attachment = att;

		return att;
	}

	public static Attachment AddUnitySprite (this SkeletonData skeletonData, string slotName, Sprite sprite, string skinName = "", string shaderName = "Spine/Skeleton") {
		var att = sprite.ToRegionAttachment(shaderName);

		var slotIndex = skeletonData.FindSlotIndex(slotName);
		Skin skin = skeletonData.defaultSkin;
		if (skinName != "")
			skin = skeletonData.FindSkin(skinName);

		skin.AddAttachment(slotIndex, att.Name, att);

		return att;
	}

	public static RegionAttachment ToRegionAttachment (this Sprite sprite, string shaderName = "Spine/Skeleton") {
		var loader = new SpriteAttachmentLoader(sprite, Shader.Find(shaderName));
		var att = loader.NewRegionAttachment(null, sprite.name, "");
		loader = null;
		return att;
	}
}