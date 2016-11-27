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

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.Pool;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;

/** Collects each {@link BoundingBoxAttachment} that is visible and computes the world vertices for its polygon. The polygon
 * vertices are provided along with convenience methods for doing hit detection. */
public class SkeletonBounds {
	private float minX, minY, maxX, maxY;
	private Array<BoundingBoxAttachment> boundingBoxes = new Array();
	private Array<FloatArray> polygons = new Array();
	private Pool<FloatArray> polygonPool = new Pool() {
		protected Object newObject () {
			return new FloatArray();
		}
	};

	/** Clears any previous polygons, finds all visible bounding box attachments, and computes the world vertices for each bounding
	 * box's polygon.
	 * @param updateAabb If true, the axis aligned bounding box containing all the polygons is computed. If false, the
	 *           SkeletonBounds AABB methods will always return true. */
	public void update (Skeleton skeleton, boolean updateAabb) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		Array<BoundingBoxAttachment> boundingBoxes = this.boundingBoxes;
		Array<FloatArray> polygons = this.polygons;
		Array<Slot> slots = skeleton.slots;
		int slotCount = slots.size;

		boundingBoxes.clear();
		polygonPool.freeAll(polygons);
		polygons.clear();

		for (int i = 0; i < slotCount; i++) {
			Slot slot = slots.get(i);
			Attachment attachment = slot.attachment;
			if (attachment instanceof BoundingBoxAttachment) {
				BoundingBoxAttachment boundingBox = (BoundingBoxAttachment)attachment;
				boundingBoxes.add(boundingBox);

				FloatArray polygon = polygonPool.obtain();
				polygons.add(polygon);
				boundingBox.computeWorldVertices(slot, 0, boundingBox.getWorldVerticesLength(),
					polygon.setSize(boundingBox.getWorldVerticesLength()), 0, 2);
			}
		}

		if (updateAabb)
			aabbCompute();
		else {
			minX = Integer.MIN_VALUE;
			minY = Integer.MIN_VALUE;
			maxX = Integer.MAX_VALUE;
			maxY = Integer.MAX_VALUE;
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
	}

	/** Returns true if the axis aligned bounding box contains the point. */
	public boolean aabbContainsPoint (float x, float y) {
		return x >= minX && x <= maxX && y >= minY && y <= maxY;
	}

	/** Returns true if the axis aligned bounding box intersects the line segment. */
	public boolean aabbIntersectsSegment (float x1, float y1, float x2, float y2) {
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
		return minX < bounds.maxX && maxX > bounds.minX && minY < bounds.maxY && maxY > bounds.minY;
	}

	/** Returns the first bounding box attachment that contains the point, or null. When doing many checks, it is usually more
	 * efficient to only call this method if {@link #aabbContainsPoint(float, float)} returns true. */
	public BoundingBoxAttachment containsPoint (float x, float y) {
		Array<FloatArray> polygons = this.polygons;
		for (int i = 0, n = polygons.size; i < n; i++)
			if (containsPoint(polygons.get(i), x, y)) return boundingBoxes.get(i);
		return null;
	}

	/** Returns true if the polygon contains the point. */
	public boolean containsPoint (FloatArray polygon, float x, float y) {
		float[] vertices = polygon.items;
		int nn = polygon.size;

		int prevIndex = nn - 2;
		boolean inside = false;
		for (int ii = 0; ii < nn; ii += 2) {
			float vertexY = vertices[ii + 1];
			float prevY = vertices[prevIndex + 1];
			if ((vertexY < y && prevY >= y) || (prevY < y && vertexY >= y)) {
				float vertexX = vertices[ii];
				if (vertexX + (y - vertexY) / (prevY - vertexY) * (vertices[prevIndex] - vertexX) < x) inside = !inside;
			}
			prevIndex = ii;
		}
		return inside;
	}

	/** Returns the first bounding box attachment that contains any part of the line segment, or null. When doing many checks, it
	 * is usually more efficient to only call this method if {@link #aabbIntersectsSegment(float, float, float, float)} returns
	 * true. */
	public BoundingBoxAttachment intersectsSegment (float x1, float y1, float x2, float y2) {
		Array<FloatArray> polygons = this.polygons;
		for (int i = 0, n = polygons.size; i < n; i++)
			if (intersectsSegment(polygons.get(i), x1, y1, x2, y2)) return boundingBoxes.get(i);
		return null;
	}

	/** Returns true if the polygon contains any part of the line segment. */
	public boolean intersectsSegment (FloatArray polygon, float x1, float y1, float x2, float y2) {
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

	/** The left edge of the axis aligned bounding box. */
	public float getMinX () {
		return minX;
	}

	/** The bottom edge of the axis aligned bounding box. */
	public float getMinY () {
		return minY;
	}

	/** The right edge of the axis aligned bounding box. */
	public float getMaxX () {
		return maxX;
	}

	/** The top edge of the axis aligned bounding box. */
	public float getMaxY () {
		return maxY;
	}

	/** The width of the axis aligned bounding box. */
	public float getWidth () {
		return maxX - minX;
	}

	/** The height of the axis aligned bounding box. */
	public float getHeight () {
		return maxY - minY;
	}

	/** The visible bounding boxes. */
	public Array<BoundingBoxAttachment> getBoundingBoxes () {
		return boundingBoxes;
	}

	/** The world vertices for the bounding box polygons. */
	public Array<FloatArray> getPolygons () {
		return polygons;
	}

	/** Returns the polygon for the specified bounding box, or null. */
	public FloatArray getPolygon (BoundingBoxAttachment boundingBox) {
		if (boundingBox == null) throw new IllegalArgumentException("boundingBox cannot be null.");
		int index = boundingBoxes.indexOf(boundingBox, true);
		return index == -1 ? null : polygons.get(index);
	}
}
