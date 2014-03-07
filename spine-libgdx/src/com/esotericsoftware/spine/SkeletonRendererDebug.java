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

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

import static com.badlogic.gdx.graphics.g2d.Batch.*;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
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
	private float scale = 1;

	public SkeletonRendererDebug () {
		renderer = new ShapeRenderer();
	}

	public void draw (Skeleton skeleton) {
		float skeletonX = skeleton.getX();
		float skeletonY = skeleton.getY();

		Gdx.gl.glEnable(GL20.GL_BLEND);
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
				renderer.circle(skeletonX + bone.worldX, skeletonY + bone.worldY, 3 * scale);
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

	public void setScale (float scale) {
		this.scale = scale;
	}

	public void setRegionAttachments (boolean regionAttachments) {
		this.drawRegionAttachments = regionAttachments;
	}

	public void setBoundingBoxes (boolean boundingBoxes) {
		this.drawBoundingBoxes = boundingBoxes;
	}
}
