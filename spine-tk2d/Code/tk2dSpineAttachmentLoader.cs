using System;
using UnityEngine;
using Spine;

// TODO: handle TPackerCW flip mode (probably not swap uv horizontaly)

public class tk2dSpineAttachmentLoader : AttachmentLoader {
	private tk2dSpriteCollectionData sprites;

	public tk2dSpineAttachmentLoader(tk2dSpriteCollectionData sprites) {
		if (sprites == null) throw new ArgumentNullException("sprites cannot be null.");
		this.sprites = sprites;
	}

	public Attachment NewAttachment(Skin skin, AttachmentType type, String name) {
		if (type != AttachmentType.region) throw new Exception("Unknown attachment type: " + type);

		// Strip folder names.
		int index = name.LastIndexOfAny(new char[] {'/', '\\'});
		if (index != -1) name = name.Substring(index + 1);

		tk2dSpriteDefinition attachmentParameters = null;
		for (int i = 0; i < sprites.inst.spriteDefinitions.Length; ++i) {
			tk2dSpriteDefinition def = sprites.inst.spriteDefinitions[i];
			if (def.name == name) {
				attachmentParameters = def;
				break;
			}
		}
		
		if (attachmentParameters == null) throw new Exception("Sprite not found in atlas: " + name + " (" + type + ")");
		if (attachmentParameters.complexGeometry) throw new NotImplementedException("Complex geometry is not supported: " + name + " (" + type + ")");
		if (attachmentParameters.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW) throw new NotImplementedException("Only 2D Toolkit atlases are supported: " + name + " (" + type + ")");

		Vector2 minTexCoords = Vector2.one;
		Vector2 maxTexCoords = Vector2.zero;
		for (int i = 0; i < attachmentParameters.uvs.Length; ++i) {
			Vector2 uv = attachmentParameters.uvs[i];
			minTexCoords = Vector2.Min(minTexCoords, uv);
			maxTexCoords = Vector2.Max(maxTexCoords, uv);
		}
		
		Texture texture = attachmentParameters.material.mainTexture;
		int width = (int)(Mathf.Abs(maxTexCoords.x - minTexCoords.x) * texture.width);
		int height = (int)(Mathf.Abs(maxTexCoords.y - minTexCoords.y) * texture.height);
		
		bool rotated = (attachmentParameters.flipped == tk2dSpriteDefinition.FlipMode.Tk2d);
		
		if (rotated) {
			float temp = minTexCoords.x;
			minTexCoords.x = maxTexCoords.x;
			maxTexCoords.x = temp;
		}
		
		RegionAttachment attachment = new RegionAttachment(name);
		
		attachment.SetUVs(
			minTexCoords.x,
			maxTexCoords.y,
			maxTexCoords.x,
			minTexCoords.y,
			rotated
		);

		// TODO - Set attachment.RegionOffsetX/Y. What units does attachmentParameters.untrimmedBoundsData use?!
		attachment.RegionWidth = width;
		attachment.RegionHeight = height;
		attachment.RegionOriginalWidth = width;
		attachment.RegionOriginalHeight = height;
		
		return attachment;
	}
}
