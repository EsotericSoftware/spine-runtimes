/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import static com.badlogic.gdx.graphics.g2d.Batch.*;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

public class SkeletonRendererDebug {
	static private final Color boneLineColor = Color.RED;
	static private final Color boneOriginColor = Color.GREEN;
	static private final Color attachmentLineColor = new Color(0, 0, 1, 0.5f);
	static private final Color triangleLineColor = new Color(1, 0.64f, 0, 0.5f);
	static private final Color aabbColor = new Color(0, 1, 0, 0.5f);

	private final ShapeRenderer shapes;
	private boolean drawBones = true, drawRegionAttachments = true, drawBoundingBoxes = true;
	private boolean drawMeshHull = true, drawMeshTriangles = true, drawPaths = true;
	private final SkeletonBounds bounds = new SkeletonBounds();
	private final FloatArray temp = new FloatArray();
	private float scale = 1;
	private float boneWidth = 2;
	private boolean premultipliedAlpha;

	public SkeletonRendererDebug () {
		shapes = new ShapeRenderer();
	}

	public SkeletonRendererDebug (ShapeRenderer shapes) {
		this.shapes = shapes;
	}

	public void draw (Skeleton skeleton) {
		float skeletonX = skeleton.getX();
		float skeletonY = skeleton.getY();

		Gdx.gl.glEnable(GL20.GL_BLEND);
		int srcFunc = premultipliedAlpha ? GL20.GL_ONE : GL20.GL_SRC_ALPHA;
		Gdx.gl.glBlendFunc(srcFunc, GL20.GL_ONE_MINUS_SRC_ALPHA);

		ShapeRenderer shapes = this.shapes;

		Array<Bone> bones = skeleton.getBones();
		if (drawBones) {
			shapes.setColor(boneLineColor);
			shapes.begin(ShapeType.Filled);
			for (int i = 0, n = bones.size; i < n; i++) {
				Bone bone = bones.get(i);
				if (bone.parent == null) continue;
				float x = skeletonX + bone.data.length * bone.a + bone.worldX;
				float y = skeletonY + bone.data.length * bone.c + bone.worldY;
				shapes.rectLine(skeletonX + bone.worldX, skeletonY + bone.worldY, x, y, boneWidth * scale);
			}
			shapes.end();
			shapes.begin(ShapeType.Line);
			shapes.x(skeletonX, skeletonY, 4 * scale);
		} else
			shapes.begin(ShapeType.Line);

		if (drawRegionAttachments) {
			shapes.setColor(attachmentLineColor);
			Array<Slot> slots = skeleton.getSlots();
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (attachment instanceof RegionAttachment) {
					RegionAttachment regionAttachment = (RegionAttachment)attachment;
					float[] vertices = regionAttachment.updateWorldVertices(slot, false);
					shapes.line(vertices[X1], vertices[Y1], vertices[X2], vertices[Y2]);
					shapes.line(vertices[X2], vertices[Y2], vertices[X3], vertices[Y3]);
					shapes.line(vertices[X3], vertices[Y3], vertices[X4], vertices[Y4]);
					shapes.line(vertices[X4], vertices[Y4], vertices[X1], vertices[Y1]);
				}
			}
		}

		if (drawMeshHull || drawMeshTriangles) {
			Array<Slot> slots = skeleton.getSlots();
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (!(attachment instanceof MeshAttachment)) continue;
				MeshAttachment mesh = (MeshAttachment)attachment;
				mesh.updateWorldVertices(slot, false);
				float[] vertices = mesh.getWorldVertices();
				short[] triangles = mesh.getTriangles();
				int hullLength = mesh.getHullLength();
				if (drawMeshTriangles) {
					shapes.setColor(triangleLineColor);
					for (int ii = 0, nn = triangles.length; ii < nn; ii += 3) {
						int v1 = triangles[ii] * 5, v2 = triangles[ii + 1] * 5, v3 = triangles[ii + 2] * 5;
						shapes.triangle(vertices[v1], vertices[v1 + 1], //
							vertices[v2], vertices[v2 + 1], //
							vertices[v3], vertices[v3 + 1] //
						);
					}
				}
				if (drawMeshHull && hullLength > 0) {
					shapes.setColor(attachmentLineColor);
					hullLength = (hullLength >> 1) * 5;
					float lastX = vertices[hullLength - 5], lastY = vertices[hullLength - 4];
					for (int ii = 0, nn = hullLength; ii < nn; ii += 5) {
						float x = vertices[ii], y = vertices[ii + 1];
						shapes.line(x, y, lastX, lastY);
						lastX = x;
						lastY = y;
					}
				}
			}
		}

		if (drawBoundingBoxes) {
			SkeletonBounds bounds = this.bounds;
			bounds.update(skeleton, true);
			shapes.setColor(aabbColor);
			shapes.rect(bounds.getMinX(), bounds.getMinY(), bounds.getWidth(), bounds.getHeight());
			Array<FloatArray> polygons = bounds.getPolygons();
			Array<BoundingBoxAttachment> boxes = bounds.getBoundingBoxes();
			for (int i = 0, n = polygons.size; i < n; i++) {
				FloatArray polygon = polygons.get(i);
				shapes.setColor(boxes.get(i).getColor());
				shapes.polygon(polygon.items, 0, polygon.size);
			}
		}

		if (drawPaths) {
			Array<Slot> slots = skeleton.getSlots();
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (!(attachment instanceof PathAttachment)) continue;
				PathAttachment path = (PathAttachment)attachment;
				int nn = path.getWorldVerticesLength();
				float[] world = temp.setSize(nn);
				path.computeWorldVertices(slot, world);
				Color color = path.getColor();
				float x1 = world[2], y1 = world[3], x2 = 0, y2 = 0;
				if (path.getClosed()) {
					shapes.setColor(color);
					float cx1 = world[0], cy1 = world[1], cx2 = world[nn - 2], cy2 = world[nn - 1];
					x2 = world[nn - 4];
					y2 = world[nn - 3];
					shapes.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, 32);
					shapes.setColor(Color.LIGHT_GRAY);
					shapes.line(x1, y1, cx1, cy1);
					shapes.line(x2, y2, cx2, cy2);
				}
				nn -= 4;
				for (int ii = 4; ii < nn; ii += 6) {
					float cx1 = world[ii], cy1 = world[ii + 1], cx2 = world[ii + 2], cy2 = world[ii + 3];
					x2 = world[ii + 4];
					y2 = world[ii + 5];
					shapes.setColor(color);
					shapes.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, 32);
					shapes.setColor(Color.LIGHT_GRAY);
					shapes.line(x1, y1, cx1, cy1);
					shapes.line(x2, y2, cx2, cy2);
					x1 = x2;
					y1 = y2;
				}
			}
		}

		shapes.end();
		shapes.begin(ShapeType.Filled);

		if (drawBones) {
			shapes.setColor(boneOriginColor);
			for (int i = 0, n = bones.size; i < n; i++) {
				Bone bone = bones.get(i);
				shapes.setColor(Color.GREEN);
				shapes.circle(skeletonX + bone.worldX, skeletonY + bone.worldY, 3 * scale, 8);
			}
		}

		shapes.end();

	}

	public ShapeRenderer getShapeRenderer () {
		return shapes;
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

	public void setMeshHull (boolean meshHull) {
		this.drawMeshHull = meshHull;
	}

	public void setMeshTriangles (boolean meshTriangles) {
		this.drawMeshTriangles = meshTriangles;
	}

	public void setPaths (boolean paths) {
		this.drawPaths = paths;
	}

	public void setPremultipliedAlpha (boolean premultipliedAlpha) {
		this.premultipliedAlpha = premultipliedAlpha;
	}
}
