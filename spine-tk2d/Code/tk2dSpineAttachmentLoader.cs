using System;
using UnityEngine;
using Spine;

// TODO: handle TPackerCW flip mode (probably not swap uv horizontaly)

/*
 */
public class tk2dSpineAttachmentLoader : AttachmentLoader {
	
	/*
	 */
	private tk2dSpriteCollectionData sprites;
	
	/*
	 */
	public tk2dSpineAttachmentLoader(tk2dSpriteCollectionData s) {
		if(s == null) throw new ArgumentNullException("sprites cannot be null.");
		sprites = s;
	}

	public Attachment NewAttachment(Skin skin,AttachmentType type,String name) {
		if(type != AttachmentType.region) throw new Exception("Unknown attachment type: " + type);
		
		tk2dSpriteDefinition attachmentParameters = null;
		for(int i = 0; i < sprites.spriteDefinitions.Length; ++i) {
			tk2dSpriteDefinition def = sprites.spriteDefinitions[i];
			if(def.name == name) {
				attachmentParameters = def;
				break;
			}
		}
		
		if(attachmentParameters == null ) throw new Exception("Sprite not found in atlas: " + name + " (" + type + ")");
		if(attachmentParameters.complexGeometry) throw new NotImplementedException("Complex geometry is not supported: " + name + " (" + type + ")");
		if(attachmentParameters.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW) throw new NotImplementedException("Only 2d toolkit atlases are supported: " + name + " (" + type + ")");
		Texture tex = attachmentParameters.material.mainTexture;
		
		Vector2 minTexCoords = Vector2.one;
		Vector2 maxTexCoords = Vector2.zero;
		for(int i = 0; i < attachmentParameters.uvs.Length; ++i) {
			Vector2 uv = attachmentParameters.uvs[i];
			minTexCoords = Vector2.Min(minTexCoords,uv);
			maxTexCoords = Vector2.Max(maxTexCoords,uv);
		}
		
		int width = (int)(Mathf.Abs(maxTexCoords.x - minTexCoords.x) * tex.width);
		int height = (int)(Mathf.Abs(maxTexCoords.y - minTexCoords.y) * tex.height);
		
		bool rotated = (attachmentParameters.flipped == tk2dSpriteDefinition.FlipMode.Tk2d);
		
		if(rotated) {
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
		
		attachment.RegionWidth = width;
		attachment.RegionHeight = height;
		attachment.RegionOriginalWidth = width;
		attachment.RegionOriginalHeight = height;
		
		return attachment;
	}
}
