/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

import com.badlogic.gdx.graphics.GL11;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.Batch;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.utils.Array;

public class SkeletonRenderer {
	static private final short[] quadTriangle = {0, 1, 2, 2, 3, 0};

	private boolean premultipliedAlpha;

	public void draw (PolygonSpriteBatch batch, Skeleton skeleton) {
		boolean premultipliedAlpha = this.premultipliedAlpha;
		int srcFunc = premultipliedAlpha ? GL11.GL_ONE : GL11.GL_SRC_ALPHA;
		batch.setBlendFunction(srcFunc, GL11.GL_ONE_MINUS_SRC_ALPHA);

		boolean additive = false;

		float[] vertices;
		short[] triangles;
		Texture texture;
		Array<Slot> drawOrder = skeleton.drawOrder;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			Slot slot = drawOrder.get(i);
			Attachment attachment = slot.attachment;
			if (attachment instanceof RegionAttachment) {
				RegionAttachment region = (RegionAttachment)attachment;
				region.updateWorldVertices(slot, premultipliedAlpha);
				vertices = region.getWorldVertices();
				triangles = quadTriangle;
				texture = region.getRegion().getTexture();
			} else
				continue;

			if (slot.data.getAdditiveBlending() != additive) {
				additive = !additive;
				if (additive)
					batch.setBlendFunction(srcFunc, GL11.GL_ONE);
				else
					batch.setBlendFunction(srcFunc, GL11.GL_ONE_MINUS_SRC_ALPHA);
			}

			batch.draw(texture, vertices, 0, vertices.length, triangles, 0, triangles.length);
		}
	}

	public void draw (Batch batch, Skeleton skeleton) {
		boolean premultipliedAlpha = this.premultipliedAlpha;
		int srcFunc = premultipliedAlpha ? GL11.GL_ONE : GL11.GL_SRC_ALPHA;
		batch.setBlendFunction(srcFunc, GL11.GL_ONE_MINUS_SRC_ALPHA);

		boolean additive = false;

		Array<Slot> drawOrder = skeleton.drawOrder;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			Slot slot = drawOrder.get(i);
			Attachment attachment = slot.attachment;
			if (attachment instanceof RegionAttachment) {
				RegionAttachment regionAttachment = (RegionAttachment)attachment;
				regionAttachment.updateWorldVertices(slot, premultipliedAlpha);
				float[] vertices = regionAttachment.getWorldVertices();
				if (slot.data.getAdditiveBlending() != additive) {
					additive = !additive;
					if (additive)
						batch.setBlendFunction(srcFunc, GL11.GL_ONE);
					else
						batch.setBlendFunction(srcFunc, GL11.GL_ONE_MINUS_SRC_ALPHA);
				}
				batch.draw(regionAttachment.getRegion().getTexture(), vertices, 0, 20);
			}
		}
	}

	public void setPremultipliedAlpha (boolean premultipliedAlpha) {
		this.premultipliedAlpha = premultipliedAlpha;
	}
}
