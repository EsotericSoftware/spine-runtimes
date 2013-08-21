using System;
using UnityEngine;
using Spine;

// TODO: handle TPackerCW flip mode (probably not swap uv horizontaly)

public class SpriteCollectionAttachmentLoader : AttachmentLoader {
	private tk2dSpriteCollectionData sprites;

	public SpriteCollectionAttachmentLoader (tk2dSpriteCollectionData sprites) {
		if (sprites == null)
			throw new ArgumentNullException("sprites cannot be null.");
		this.sprites = sprites;
	}

	public Attachment NewAttachment (Skin skin, AttachmentType type, String name) {
		if (type != AttachmentType.region)
			throw new Exception("Unknown attachment type: " + type);
		
		// Strip folder names.
		int index = name.LastIndexOfAny(new char[] {'/', '\\'});
		if (index != -1)
			name = name.Substring(index + 1);
		
		tk2dSpriteDefinition def = sprites.inst.GetSpriteDefinition(name);
		
		if (def == null)
			throw new Exception("Sprite not found in atlas: " + name + " (" + type + ")");
		if (def.complexGeometry)
			throw new NotImplementedException("Complex geometry is not supported: " + name + " (" + type + ")");
		if (def.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
			throw new NotImplementedException("Only 2D Toolkit atlases are supported: " + name + " (" + type + ")");

		RegionAttachment attachment = new RegionAttachment(name);
		
		Vector2 minTexCoords = Vector2.one;
		Vector2 maxTexCoords = Vector2.zero;
		for (int i = 0; i < def.uvs.Length; ++i) {
			Vector2 uv = def.uvs[i];
			minTexCoords = Vector2.Min(minTexCoords, uv);
			maxTexCoords = Vector2.Max(maxTexCoords, uv);
		}
		bool rotated = def.flipped == tk2dSpriteDefinition.FlipMode.Tk2d;
		if (rotated) {
			float temp = minTexCoords.x;
			minTexCoords.x = maxTexCoords.x;
			maxTexCoords.x = temp;
		}
		attachment.SetUVs(
			minTexCoords.x,
			maxTexCoords.y,
			maxTexCoords.x,
			minTexCoords.y,
			rotated
		);
		
		attachment.RegionOriginalWidth = (int)(def.untrimmedBoundsData[1].x / def.texelSize.x);
		attachment.RegionOriginalHeight = (int)(def.untrimmedBoundsData[1].y / def.texelSize.y);

		attachment.RegionWidth = (int)(def.boundsData[1].x / def.texelSize.x);
		attachment.RegionHeight = (int)(def.boundsData[1].y / def.texelSize.y);

		float x0 = def.untrimmedBoundsData[0].x - def.untrimmedBoundsData[1].x / 2;
		float x1 = def.boundsData[0].x - def.boundsData[1].x / 2;
		attachment.RegionOffsetX = (int)((x1 - x0) / def.texelSize.x);

		float y0 = def.untrimmedBoundsData[0].y - def.untrimmedBoundsData[1].y / 2;
		float y1 = def.boundsData[0].y - def.boundsData[1].y / 2;
		attachment.RegionOffsetY = (int)((y1 - y0) / def.texelSize.y);

		attachment.RendererObject = def.material;

		return attachment;
	}
}
