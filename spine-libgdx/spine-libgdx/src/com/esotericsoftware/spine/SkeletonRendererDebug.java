/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.PointAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

public class SkeletonRendererDebug {
	static private final Color boneLineColor = Color.RED;
	static private final Color boneOriginColor = Color.GREEN;
	static private final Color attachmentLineColor = new Color(0, 0, 1, 0.5f);
	static private final Color triangleLineColor = new Color(1, 0.64f, 0, 0.5f); // ffa3007f
	static private final Color aabbColor = new Color(0, 1, 0, 0.5f);

	private final ShapeRenderer shapes;
	private boolean drawBones = true, drawRegionAttachments = true, drawBoundingBoxes = true, drawPoints = true;
	private boolean drawMeshHull = true, drawMeshTriangles = true, drawPaths = true, drawClipping = true;
	private final SkeletonBounds bounds = new SkeletonBounds();
	private final FloatArray vertices = new FloatArray(32);
	private float scale = 1;
	private float boneWidth = 2;
	private boolean premultipliedAlpha;
	private final Vector2 temp1 = new Vector2(), temp2 = new Vector2();

	public SkeletonRendererDebug () {
		shapes = new ShapeRenderer();
	}

	public SkeletonRendererDebug (ShapeRenderer shapes) {
		this.shapes = shapes;
	}

	public void draw (Skeleton skeleton) {
		Gdx.gl.glEnable(GL20.GL_BLEND);
		int srcFunc = premultipliedAlpha ? GL20.GL_ONE : GL20.GL_SRC_ALPHA;
		Gdx.gl.glBlendFunc(srcFunc, GL20.GL_ONE_MINUS_SRC_ALPHA);

		ShapeRenderer shapes = this.shapes;
		Array<Bone> bones = skeleton.getBones();
		Array<Slot> slots = skeleton.getSlots();

		shapes.begin(ShapeType.Filled);

		if (drawBones) {
			for (int i = 0, n = bones.size; i < n; i++) {
				Bone bone = bones.get(i);
				if (bone.parent == null) continue;
				float length = bone.data.length, width = boneWidth;
				if (length == 0) {
					length = 8;
					width /= 2;
					shapes.setColor(boneOriginColor);
				} else
					shapes.setColor(boneLineColor);
				float x = length * bone.a + bone.worldX;
				float y = length * bone.c + bone.worldY;
				shapes.rectLine(bone.worldX, bone.worldY, x, y, width * scale);
			}
			shapes.x(skeleton.getX(), skeleton.getY(), 4 * scale);
		}

		if (drawPoints) {
			shapes.setColor(boneOriginColor);
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (!(attachment instanceof PointAttachment)) continue;
				PointAttachment point = (PointAttachment)attachment;
				point.computeWorldPosition(slot.getBone(), temp1);
				temp2.set(8, 0).rotate(point.computeWorldRotation(slot.getBone()));
				shapes.rectLine(temp1, temp2, boneWidth / 2 * scale);
			}
		}

		shapes.end();
		shapes.begin(ShapeType.Line);

		if (drawRegionAttachments) {
			shapes.setColor(attachmentLineColor);
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (attachment instanceof RegionAttachment) {
					RegionAttachment region = (RegionAttachment)attachment;
					float[] vertices = this.vertices.items;
					region.computeWorldVertices(slot.getBone(), vertices, 0, 2);
					shapes.line(vertices[0], vertices[1], vertices[2], vertices[3]);
					shapes.line(vertices[2], vertices[3], vertices[4], vertices[5]);
					shapes.line(vertices[4], vertices[5], vertices[6], vertices[7]);
					shapes.line(vertices[6], vertices[7], vertices[0], vertices[1]);
				}
			}
		}

		if (drawMeshHull || drawMeshTriangles) {
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (!(attachment instanceof MeshAttachment)) continue;
				MeshAttachment mesh = (MeshAttachment)attachment;
				float[] vertices = this.vertices.setSize(mesh.getWorldVerticesLength());
				mesh.computeWorldVertices(slot, 0, mesh.getWorldVerticesLength(), vertices, 0, 2);
				short[] triangles = mesh.getTriangles();
				int hullLength = mesh.getHullLength();
				if (drawMeshTriangles) {
					shapes.setColor(triangleLineColor);
					for (int ii = 0, nn = triangles.length; ii < nn; ii += 3) {
						int v1 = triangles[ii] * 2, v2 = triangles[ii + 1] * 2, v3 = triangles[ii + 2] * 2;
						shapes.triangle(vertices[v1], vertices[v1 + 1], //
							vertices[v2], vertices[v2 + 1], //
							vertices[v3], vertices[v3 + 1] //
						);
					}
				}
				if (drawMeshHull && hullLength > 0) {
					shapes.setColor(attachmentLineColor);
					float lastX = vertices[hullLength - 2], lastY = vertices[hullLength - 1];
					for (int ii = 0, nn = hullLength; ii < nn; ii += 2) {
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

		if (drawClipping) {
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (!(attachment instanceof ClippingAttachment)) continue;
				ClippingAttachment clip = (ClippingAttachment)attachment;
				int nn = clip.getWorldVerticesLength();
				float[] vertices = this.vertices.setSize(nn);
				clip.computeWorldVertices(slot, 0, nn, vertices, 0, 2);
				shapes.setColor(clip.getColor());
				for (int ii = 2; ii < nn; ii += 2)
					shapes.line(vertices[ii - 2], vertices[ii - 1], vertices[ii], vertices[ii + 1]);
				shapes.line(vertices[0], vertices[1], vertices[nn - 2], vertices[nn - 1]);
			}
		}

		if (drawPaths) {
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (!(attachment instanceof PathAttachment)) continue;
				PathAttachment path = (PathAttachment)attachment;
				int nn = path.getWorldVerticesLength();
				float[] vertices = this.vertices.setSize(nn);
				path.computeWorldVertices(slot, 0, nn, vertices, 0, 2);
				Color color = path.getColor();
				float x1 = vertices[2], y1 = vertices[3], x2 = 0, y2 = 0;
				if (path.getClosed()) {
					shapes.setColor(color);
					float cx1 = vertices[0], cy1 = vertices[1], cx2 = vertices[nn - 2], cy2 = vertices[nn - 1];
					x2 = vertices[nn - 4];
					y2 = vertices[nn - 3];
					shapes.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, 32);
					shapes.setColor(Color.LIGHT_GRAY);
					shapes.line(x1, y1, cx1, cy1);
					shapes.line(x2, y2, cx2, cy2);
				}
				nn -= 4;
				for (int ii = 4; ii < nn; ii += 6) {
					float cx1 = vertices[ii], cy1 = vertices[ii + 1], cx2 = vertices[ii + 2], cy2 = vertices[ii + 3];
					x2 = vertices[ii + 4];
					y2 = vertices[ii + 5];
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
				shapes.circle(bone.worldX, bone.worldY, 3 * scale, 8);
			}
		}

		if (drawPoints) {
			shapes.setColor(boneOriginColor);
			for (int i = 0, n = slots.size; i < n; i++) {
				Slot slot = slots.get(i);
				Attachment attachment = slot.attachment;
				if (!(attachment instanceof PointAttachment)) continue;
				PointAttachment point = (PointAttachment)attachment;
				point.computeWorldPosition(slot.getBone(), temp1);
				shapes.circle(temp1.x, temp1.y, 3 * scale, 8);
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

	public void setPoints (boolean points) {
		this.drawPoints = points;
	}
	
	public void setClipping (boolean clipping) {
		this.drawClipping = clipping;
	}

	public void setPremultipliedAlpha (boolean premultipliedAlpha) {
		this.premultipliedAlpha = premultipliedAlpha;
	}
}
