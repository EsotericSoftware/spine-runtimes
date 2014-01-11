/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
		switch (type) {
		case AttachmentType.region:
			break;
		case AttachmentType.boundingbox:
			return new BoundingBoxAttachment(name);
		default:
			throw new Exception("Unknown attachment type: " + type);
		}
		
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
