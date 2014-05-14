/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections.Generic;

namespace Spine {
	public class SkeletonBounds {
		private List<Polygon> polygonPool = new List<Polygon>();
		private float minX, minY, maxX, maxY;

		public List<BoundingBoxAttachment> BoundingBoxes { get; private set; }
		public List<Polygon> Polygons { get; private set; }
		public float MinX { get { return minX; } set { minX = value; } }
		public float MinY { get { return minY; } set { minY = value; } }
		public float MaxX { get { return maxX; } set { maxX = value; } }
		public float MaxY { get { return maxY; } set { maxY = value; } }
		public float Width { get { return maxX - minX; } }
		public float Height { get { return maxY - minY; } }

		public SkeletonBounds () {
			BoundingBoxes = new List<BoundingBoxAttachment>();
			Polygons = new List<Polygon>();
		}

		public void Update (Skeleton skeleton, bool updateAabb) {
			List<BoundingBoxAttachment> boundingBoxes = BoundingBoxes;
			List<Polygon> polygons = Polygons;
			List<Slot> slots = skeleton.slots;
			int slotCount = slots.Count;
			float x = skeleton.x, y = skeleton.y;

			boundingBoxes.Clear();
			foreach (Polygon polygon in polygons)
				polygonPool.Add(polygon);
			polygons.Clear();

			for (int i = 0; i < slotCount; i++) {
				Slot slot = slots[i];
				BoundingBoxAttachment boundingBox = slot.attachment as BoundingBoxAttachment;
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
				boundingBox.ComputeWorldVertices(x, y, slot.bone, polygon.Vertices);
			}

			if (updateAabb) aabbCompute();
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
		}


		/// <summary>Returns true if the axis aligned bounding box contains the point.</summary>
		public bool AabbContainsPoint (float x, float y) {
			return x >= minX && x <= maxX && y >= minY && y <= maxY;
		}

		/// <summary>Returns true if the axis aligned bounding box intersects the line segment.</summary>
		public bool AabbIntersectsSegment (float x1, float y1, float x2, float y2) {
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

		/// <summary>Returns true if the axis aligned bounding box intersects the axis aligned bounding box of the specified bounds.</summary>
		public bool AabbIntersectsSkeleton (SkeletonBounds bounds) {
			return minX < bounds.maxX && maxX > bounds.minX && minY < bounds.maxY && maxY > bounds.minY;
		}

		/// <summary>Returns true if the polygon contains the point.</summary>
		public bool ContainsPoint (Polygon polygon, float x, float y) {
			float[] vertices = polygon.Vertices;
			int nn = polygon.Count;

			int prevIndex = nn - 2;
			bool inside = false;
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

		/// <summary>Returns the first bounding box attachment that contains the point, or null. When doing many checks, it is usually more
		/// efficient to only call this method if {@link #aabbContainsPoint(float, float)} returns true.</summary>
		public BoundingBoxAttachment ContainsPoint (float x, float y) {
			List<Polygon> polygons = Polygons;
			for (int i = 0, n = polygons.Count; i < n; i++)
				if (ContainsPoint(polygons[i], x, y)) return BoundingBoxes[i];
			return null;
		}

		/// <summary>Returns the first bounding box attachment that contains the line segment, or null. When doing many checks, it is usually
		/// more efficient to only call this method if {@link #aabbIntersectsSegment(float, float, float, float)} returns true.</summary>
		public BoundingBoxAttachment IntersectsSegment (float x1, float y1, float x2, float y2) {
			List<Polygon> polygons = Polygons;
			for (int i = 0, n = polygons.Count; i < n; i++)
				if (IntersectsSegment(polygons[i], x1, y1, x2, y2)) return BoundingBoxes[i];
			return null;
		}

		/// <summary>Returns true if the polygon contains the line segment.</summary>
		public bool IntersectsSegment (Polygon polygon, float x1, float y1, float x2, float y2) {
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

		public Polygon getPolygon (BoundingBoxAttachment attachment) {
			int index = BoundingBoxes.IndexOf(attachment);
			return index == -1 ? null : Polygons[index];
		}
	}

	public class Polygon {
		public float[] Vertices { get; set; }
		public int Count { get; set; }

		public Polygon () {
			Vertices = new float[16];
		}
	}
}
