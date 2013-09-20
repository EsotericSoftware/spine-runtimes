/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
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
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;

public class SkeletonBounds {
	private boolean aabb;
	private float minX, minY, maxX, maxY;
	private Array<BoundingBoxAttachment> boundingBoxes = new Array();
	private Array<FloatArray> polygons = new Array();

	public void update (Skeleton skeleton) {
		aabb = false;

		Array<BoundingBoxAttachment> boundingBoxes = this.boundingBoxes;
		Array<FloatArray> polygons = this.polygons;
		Array<Slot> slots = skeleton.slots;
		int slotCount = slots.size;
		float x = skeleton.getX(), y = skeleton.getY();

		boundingBoxes.clear();
		polygons.clear();
		polygons.ensureCapacity(slotCount);

		for (int i = 0; i < slotCount; i++) {
			Slot slot = slots.get(i);
			Attachment attachment = slot.attachment;
			if (attachment instanceof BoundingBoxAttachment) {
				BoundingBoxAttachment boundingBox = (BoundingBoxAttachment)attachment;
				boundingBoxes.add(boundingBox);

				polygons.size = boundingBoxes.size;
				FloatArray polygon = polygons.peek();
				if (polygon == null) polygons.set(polygons.size - 1, polygon = new FloatArray());

				int vertexCount = boundingBox.getVertices().length;
				polygon.ensureCapacity(vertexCount);
				polygon.size = vertexCount;
				boundingBox.computeWorldVertices(x, y, slot.bone, polygon.items);
			}
		}
	}

	private void aabbCompute () {
		float minX = Integer.MAX_VALUE, minY = Integer.MAX_VALUE, maxX = Integer.MIN_VALUE, maxY = Integer.MIN_VALUE;
		Array<FloatArray> polygons = this.polygons;
		for (int i = 0, n = polygons.size; i < n; i++) {
			FloatArray polygon = polygons.get(i);
			float[] vertices = polygon.items;
			for (int ii = 0, nn = polygon.size; ii < nn; ii += 2) {
				float x = vertices[ii];
				float y = vertices[ii + 1];
				minX = Math.min(minX, x);
				minY = Math.min(minY, y);
				maxX = Math.max(maxX, x);
				maxY = Math.max(maxY, y);
			}
		}
		this.minX = minX;
		this.minY = minY;
		this.maxX = maxX;
		this.maxY = maxY;
		aabb = true;
	}

	/** Returns true if the axis aligned bounding box contains the point. */
	public boolean aabbContainsPoint (float x, float y) {
		if (!aabb) aabbCompute();
		return x >= minX && x <= maxX && y >= minY && y <= maxY;
	}

	/** Returns true if the axis aligned bounding box intersects the line segment. */
	public boolean aabbIntersectsSegment (float x1, float y1, float x2, float y2) {
		if (!aabb) aabbCompute();
		float minX = this.minX;
		float minY = this.minY;
		float maxX = this.maxX;
		float maxY = this.maxY;
		if ((x1 <= minX && x2 <= minX) || (y1 <= minY && y2 <= minY) || (x1 >= maxX && x2 >= maxX) || (y1 >= maxY && y2 >= maxY))
			return false;
		float m = (y2 - y1) / (x2 - x1);
		float y = m * (minX - x1) + y1;
		if (y > minY && y < maxY) return true;
		y = m * (maxX - x1) + y1;
		if (y > minY && y < maxY) return true;
		float x = (minY - y1) / m + x1;
		if (x > minX && x < maxX) return true;
		x = (maxY - y1) / m + x1;
		if (x > minX && x < maxX) return true;
		return false;
	}

	/** Returns true if the axis aligned bounding box intersects the axis aligned bounding box of the specified bounds. */
	public boolean aabbIntersectsSkeleton (SkeletonBounds bounds) {
		if (!aabb) aabbCompute();
		if (!bounds.aabb) bounds.aabbCompute();
		return minX < bounds.maxX && maxX > bounds.minX && minY < bounds.maxY && maxY > bounds.minY;
	}

	/** Returns true if the bounding box attachment contains the point. */
	public boolean containsPoint (int index, float x, float y) {
		FloatArray polygon = polygons.get(index);
		float[] vertices = polygon.items;
		int nn = polygon.size;

		int prevIndex = nn - 2;
		boolean inside = false;
		for (int ii = 0; ii < nn; ii += 2) {
			float vertexY = vertices[ii + 1];
			float prevY = vertices[prevIndex + 1];
			if (vertexY < y && prevY >= y || prevY < y && vertexY >= y) {
				float vertexX = vertices[ii];
				if (vertexX + (y - vertexY) / (prevY - vertexY) * (vertices[prevIndex] - vertexX) < x) inside = !inside;
			}
			prevIndex = ii;
		}
		return inside;
	}

	/** Returns the first bounding box attachment that contains the point, or null. When doing many checks, it is usually more
	 * efficient to only call this method if {@link #aabbContainsPoint(float, float)} return true. */
	public BoundingBoxAttachment containsPoint (float x, float y) {
		Array<BoundingBoxAttachment> boundingBoxes = this.boundingBoxes;
		for (int i = 0, n = boundingBoxes.size; i < n; i++)
			if (containsPoint(i, x, y)) return boundingBoxes.get(i);
		return null;
	}

	/** Returns true if the bounding box attachment contains the point. The bounding box must be in the SkeletonBounds. */
	public boolean containsPoint (BoundingBoxAttachment attachment, float x, float y) {
		int index = boundingBoxes.indexOf(attachment, true);
		return index == -1 ? false : containsPoint(index, x, y);
	}

	/** Returns the first bounding box attachment that contains the line segment, or null. When doing many checks, it is usually
	 * more efficient to only call this method if {@link #aabbIntersectsSegment(float, float, float, float)} return true. */
	public BoundingBoxAttachment intersectsSegment (float x1, float y1, float x2, float y2) {
		Array<BoundingBoxAttachment> boundingBoxes = this.boundingBoxes;
		for (int i = 0, n = boundingBoxes.size; i < n; i++) {
			BoundingBoxAttachment attachment = boundingBoxes.get(i);
			if (intersectsSegment(attachment, x1, y1, x2, y2)) return attachment;
		}
		return null;
	}

	/** Returns true if the bounding box attachment contains the line segment. */
	public boolean intersectsSegment (BoundingBoxAttachment attachment, float x1, float y1, float x2, float y2) {
		int index = boundingBoxes.indexOf(attachment, true);
		return index == -1 ? false : intersectsSegment(index, x1, y1, x2, y2);
	}

	/** Returns true if the bounding box attachment contains the line segment. */
	public boolean intersectsSegment (int index, float x1, float y1, float x2, float y2) {
		FloatArray polygon = polygons.get(index);
		float[] vertices = polygon.items;
		int nn = polygon.size;

		float width12 = x1 - x2, height12 = y1 - y2;
		float det1 = x1 * y2 - y1 * x2;
		float x3 = vertices[nn - 2], y3 = vertices[nn - 1];
		for (int ii = 0; ii < nn; ii += 2) {
			float x4 = vertices[ii], y4 = vertices[ii + 1];
			float det2 = x3 * y4 - y3 * x4;
			float width34 = x3 - x4, height34 = y3 - y4;
			float det3 = width12 * height34 - height12 * width34;
			float x = (det1 * width34 - width12 * det2) / det3;
			if (((x >= x3 && x <= x4) || (x >= x4 && x <= x3)) && ((x >= x1 && x <= x2) || (x >= x2 && x <= x1))) {
				float y = (det1 * height34 - height12 * det2) / det3;
				if (((y >= y3 && y <= y4) || (y >= y4 && y <= y3)) && ((y >= y1 && y <= y2) || (y >= y2 && y <= y1))) return true;
			}
			x3 = x4;
			y3 = y4;
		}
		return false;
	}

	public float getMinX () {
		if (!aabb) aabbCompute();
		return minX;
	}

	public float getMinY () {
		if (!aabb) aabbCompute();
		return minY;
	}

	public float getMaxX () {
		if (!aabb) aabbCompute();
		return maxX;
	}

	public float getMaxY () {
		if (!aabb) aabbCompute();
		return maxY;
	}

	public float getWidth () {
		if (!aabb) aabbCompute();
		return maxX - minX;
	}

	public float getHeight () {
		if (!aabb) aabbCompute();
		return maxY - minY;
	}

	public Array<BoundingBoxAttachment> getBoundingBoxes () {
		return boundingBoxes;
	}

	public Array<FloatArray> getPolygons () {
		return polygons;
	}

	/** Returns the polygon for the specified bounding box, or null. */
	public FloatArray getPolygon (BoundingBoxAttachment boundingBox) {
		int index = boundingBoxes.indexOf(boundingBox, true);
		return index == -1 ? null : polygons.get(index);
	}
}
