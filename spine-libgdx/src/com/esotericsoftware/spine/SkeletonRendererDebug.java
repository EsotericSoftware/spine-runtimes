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

import static com.badlogic.gdx.graphics.g2d.SpriteBatch.*;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;

public class SkeletonRendererDebug {
	static private final Color boneLineColor = Color.RED;
	static private final Color boneOriginColor = Color.GREEN;
	static private final Color regionAttachmentLineColor = new Color(0, 0, 1, 0.5f);
	static private final Color boundingBoxColor = new Color(0, 1, 0, 0.8f);
	static private final Color aabbColor = new Color(0, 1, 0, 0.5f);

	private final ShapeRenderer renderer;
	private boolean drawBones = true, drawRegionAttachments = true, drawBoundingBoxes = true;
	private final SkeletonBounds bounds = new SkeletonBounds();

	public SkeletonRendererDebug () {
		renderer = new ShapeRenderer();
	}

	public void draw (Skeleton skeleton) {
		float skeletonX = skeleton.getX();
		float skeletonY = skeleton.getY();

		Gdx.gl.glEnable(GL10.GL_BLEND);
		ShapeRenderer renderer = this.renderer;
		renderer.begin(ShapeType.Line);

		Array<Bone> bones = skeleton.getBones();
		if (drawBones) {
			renderer.setColor(boneLineColor);
			for (int i = 0, n = bones.size; i < n; i++) {
				Bone bone = bones.get(i);
				if (bone.parent == null) continue;
				float x = skeletonX + bone.data.length * bone.m00 + bone.worldX;
				float y = skeletonY + bone.data.length * bone.m10 + bone.worldY;
				renderer.line(skeletonX + bone.worldX, skeletonY + bone.worldY, x, y);
			}
		}

		if (drawRegionAttachments) {
			renderer.setColor(regionAttachmentLineColor);
			Array<Slot> slots = skeleton.getSlots();
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (attachment instanceof RegionAttachment) {
					RegionAttachment regionAttachment = (RegionAttachment)attachment;
					regionAttachment.updateWorldVertices(slot, false);
					float[] vertices = regionAttachment.getWorldVertices();
					renderer.line(vertices[X1], vertices[Y1], vertices[X2], vertices[Y2]);
					renderer.line(vertices[X2], vertices[Y2], vertices[X3], vertices[Y3]);
					renderer.line(vertices[X3], vertices[Y3], vertices[X4], vertices[Y4]);
					renderer.line(vertices[X4], vertices[Y4], vertices[X1], vertices[Y1]);
				}
			}
		}

		if (drawBoundingBoxes) {
			SkeletonBounds bounds = this.bounds;
			bounds.update(skeleton, true);
			renderer.setColor(aabbColor);
			renderer.rect(bounds.getMinX(), bounds.getMinY(), bounds.getWidth(), bounds.getHeight());
			renderer.setColor(boundingBoxColor);
			Array<FloatArray> polygons = bounds.getPolygons();
			for (int i = 0, n = polygons.size; i < n; i++) {
				FloatArray polygon = polygons.get(i);
				renderer.polygon(polygon.items, 0, polygon.size);
			}
		}

		renderer.end();
		renderer.begin(ShapeType.Filled);

		if (drawBones) {
			renderer.setColor(boneOriginColor);
			for (int i = 0, n = bones.size; i < n; i++) {
				Bone bone = bones.get(i);
				renderer.setColor(Color.GREEN);
				renderer.circle(skeletonX + bone.worldX, skeletonY + bone.worldY, 3);
			}
		}

		renderer.end();
	}

	public ShapeRenderer getShapeRenderer () {
		return renderer;
	}

	public void setBones (boolean bones) {
		this.drawBones = bones;
	}

	public void setRegionAttachments (boolean regionAttachments) {
		this.drawRegionAttachments = regionAttachments;
	}

	public void setBoundingBoxes (boolean boundingBoxes) {
		this.drawBoundingBoxes = boundingBoxes;
	}
}
