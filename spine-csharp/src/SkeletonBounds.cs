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

using System;
using System.Collections.Generic;

namespace Spine {
	public class SkeletonBounds {
		private bool aabb;
		private List<Polygon> polygonPool = new List<Polygon>();

		public List<BoundingBoxAttachment> BoundingBoxes { get; private set; }
		public List<Polygon> Polygons { get; private set; }

		private float minX;
		public float MinX {
			get {
				if (!aabb) aabbCompute();
				return minX;
			}
			private set {
				minX = value;
			}
		}

		private float maxX;
		public float MaxX {
			get {
				if (!aabb) aabbCompute();
				return maxX;
			}
			private set {
				maxX = value;
			}
		}

		private float minY;
		public float MinY {
			get {
				if (!aabb) aabbCompute();
				return minY;
			}
			private set {
				minY = value;
			}
		}

		private float maxY;
		public float MaxY {
			get {
				if (!aabb) aabbCompute();
				return maxY;
			}
			private set {
				maxY = value;
			}
		}

		public float Width {
			get {
				if (!aabb) aabbCompute();
				return maxX - minX;
			}
		}

		public float Height {
			get {
				if (!aabb) aabbCompute();
				return maxY - minY;
			}
		}

		public SkeletonBounds () {
			BoundingBoxes = new List<BoundingBoxAttachment>();
			Polygons = new List<Polygon>();
		}

		public void Update (Skeleton skeleton) {
			aabb = false;

			List<BoundingBoxAttachment> boundingBoxes = BoundingBoxes;
			List<Polygon> polygons = Polygons;
			List<Slot> slots = skeleton.Slots;
			int slotCount = slots.Count;
			float x = skeleton.X, y = skeleton.Y;

			boundingBoxes.Clear();
			foreach (Polygon polygon in polygons)
				polygonPool.Add(polygon);
			polygons.Clear();

			for (int i = 0; i < slotCount; i++) {
				Slot slot = slots[i];
				BoundingBoxAttachment boundingBox = slot.Attachment as BoundingBoxAttachment;
				if (boundingBox == null) continue;
				boundingBoxes.Add(boundingBox);

				Polygon polygon = null;
				int poolCount = polygonPool.Count;
				if (poolCount > 0) {
					polygon = polygonPool[poolCount - 1];
					polygonPool.RemoveAt(poolCount - 1);
				} else
					polygon = new Polygon();
				polygons.Add(polygon);

				int count = boundingBox.Vertices.Length;
				polygon.Count = count;
				if (polygon.Vertices.Length < count) polygon.Vertices = new float[count];
				boundingBox.ComputeWorldVertices(x, y, slot.Bone, polygon.Vertices);
			}
		}

		private void aabbCompute () {
			float minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
			List<Polygon> polygons = Polygons;
			for (int i = 0, n = polygons.Count; i < n; i++) {
				Polygon polygon = polygons[i];
				float[] vertices = polygon.Vertices;
				for (int ii = 0, nn = polygon.Count; ii < nn; ii += 2) {
					float x = vertices[ii];
					float y = vertices[ii + 1];
					minX = Math.Min(minX, x);
					minY = Math.Min(minY, y);
					maxX = Math.Max(maxX, x);
					maxY = Math.Max(maxY, y);
				}
			}
			this.minX = minX;
			this.minY = minY;
			this.maxX = maxX;
			this.maxY = maxY;
			aabb = true;
		}


		/** Returns true if the axis aligned bounding box contains the point. */
		public bool AabbContainsPoint (float x, float y) {
			if (!aabb) aabbCompute();
			return x >= minX && x <= maxX && y >= minY && y <= maxY;
		}

		/** Returns true if the axis aligned bounding box intersects the line segment. */
		public bool AabbIntersectsSegment (float x1, float y1, float x2, float y2) {
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
		public bool AabbIntersectsSkeleton (SkeletonBounds bounds) {
			if (!aabb) aabbCompute();
			if (!bounds.aabb) bounds.aabbCompute();
			return minX < bounds.maxX && maxX > bounds.minX && minY < bounds.maxY && maxY > bounds.minY;
		}

		/** Returns true if the bounding box attachment contains the point. */
		public bool ContainsPoint (int index, float x, float y) {
			Polygon polygon = Polygons[index];
			float[] vertices = polygon.Vertices;
			int nn = polygon.Count;

			int prevIndex = nn - 2;
			bool inside = false;
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
		public BoundingBoxAttachment ContainsPoint (float x, float y) {
			List<BoundingBoxAttachment> boundingBoxes = BoundingBoxes;
			for (int i = 0, n = boundingBoxes.Count; i < n; i++)
				if (ContainsPoint(i, x, y)) return boundingBoxes[i];
			return null;
		}

		/** Returns true if the bounding box attachment contains the point. The bounding box must be in the SkeletonBounds. */
		public bool containsPoint (BoundingBoxAttachment attachment, float x, float y) {
			int index = BoundingBoxes.IndexOf(attachment);
			return index == -1 ? false : ContainsPoint(index, x, y);
		}

		/** Returns the first bounding box attachment that contains the line segment, or null. When doing many checks, it is usually
		 * more efficient to only call this method if {@link #aabbIntersectsSegment(float, float, float, float)} return true. */
		public BoundingBoxAttachment IntersectsSegment (float x1, float y1, float x2, float y2) {
			List<BoundingBoxAttachment> boundingBoxes = BoundingBoxes;
			for (int i = 0, n = boundingBoxes.Count; i < n; i++) {
				BoundingBoxAttachment attachment = boundingBoxes[i];
				if (IntersectsSegment(attachment, x1, y1, x2, y2)) return attachment;
			}
			return null;
		}

		/** Returns true if the bounding box attachment contains the line segment. */
		public bool IntersectsSegment (BoundingBoxAttachment attachment, float x1, float y1, float x2, float y2) {
			int index = BoundingBoxes.IndexOf(attachment);
			return index == -1 ? false : IntersectsSegment(index, x1, y1, x2, y2);
		}

		/** Returns true if the bounding box attachment contains the line segment. */
		public bool IntersectsSegment (int index, float x1, float y1, float x2, float y2) {
			Polygon polygon = Polygons[index];
			float[] vertices = polygon.Vertices;
			int nn = polygon.Count;

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
	}
}

public class Polygon {
	public float[] Vertices { get; set; }
	public int Count { get; set; }

	public Polygon () {
		Vertices = new float[16];
	}
}
