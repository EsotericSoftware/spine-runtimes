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

package spine {
	import spine.attachments.BoundingBoxAttachment;

	public class SkeletonBounds {
		private var polygonPool : Vector.<Polygon> = new Vector.<Polygon>();
		public var boundingBoxes : Vector.<BoundingBoxAttachment> = new Vector.<BoundingBoxAttachment>();
		public var polygons : Vector.<Polygon> = new Vector.<Polygon>();
		public var minX : Number, minY : Number, maxX : Number, maxY : Number;

		public function SkeletonBounds() {
		}

		public function update(skeleton : Skeleton, updateAabb : Boolean) : void {
			var slots : Vector.<Slot> = skeleton.slots;
			var slotCount : int = slots.length;

			boundingBoxes.length = 0;
			for each (var polygon : Polygon in polygons)
				polygonPool[polygonPool.length] = polygon;
			polygons.length = 0;

			for (var i : int = 0; i < slotCount; i++) {
				var slot : Slot = slots[i];
				var boundingBox : BoundingBoxAttachment = slot.attachment as BoundingBoxAttachment;
				if (boundingBox == null) continue;
				boundingBoxes[boundingBoxes.length] = boundingBox;

				var poolCount : int = polygonPool.length;
				if (poolCount > 0) {
					polygon = polygonPool[poolCount - 1];
					polygonPool.splice(poolCount - 1, 1);
				} else
					polygon = new Polygon();
				polygons[polygons.length] = polygon;

				polygon.vertices.length = boundingBox.worldVerticesLength;
				boundingBox.computeWorldVertices(slot, 0, boundingBox.worldVerticesLength, polygon.vertices, 0, 2);
			}

			if (updateAabb)
				aabbCompute();
			else {
				minX = Number.MIN_VALUE;
				minY = Number.MIN_VALUE;
				maxX = Number.MAX_VALUE;
				maxY = Number.MAX_VALUE;
			}
		}

		private function aabbCompute() : void {
			var minX : Number = Number.MAX_VALUE, minY : Number = Number.MAX_VALUE;
			var maxX : Number = -Number.MAX_VALUE, maxY : Number = -Number.MAX_VALUE;
			for (var i : int = 0, n : int = polygons.length; i < n; i++) {
				var polygon : Polygon = polygons[i];
				var vertices : Vector.<Number> = polygon.vertices;
				for (var ii : int = 0, nn : int = vertices.length; ii < nn; ii += 2) {
					var x : Number = vertices[ii];
					var y : Number = vertices[ii + 1];
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
		public function aabbContainsPoint(x : Number, y : Number) : Boolean {
			return x >= minX && x <= maxX && y >= minY && y <= maxY;
		}

		/** Returns true if the axis aligned bounding box intersects the line segment. */
		public function aabbIntersectsSegment(x1 : Number, y1 : Number, x2 : Number, y2 : Number) : Boolean {
			if ((x1 <= minX && x2 <= minX) || (y1 <= minY && y2 <= minY) || (x1 >= maxX && x2 >= maxX) || (y1 >= maxY && y2 >= maxY))
				return false;
			var m : Number = (y2 - y1) / (x2 - x1);
			var y : Number = m * (minX - x1) + y1;
			if (y > minY && y < maxY) return true;
			y = m * (maxX - x1) + y1;
			if (y > minY && y < maxY) return true;
			var x : Number = (minY - y1) / m + x1;
			if (x > minX && x < maxX) return true;
			x = (maxY - y1) / m + x1;
			if (x > minX && x < maxX) return true;
			return false;
		}

		/** Returns true if the axis aligned bounding box intersects the axis aligned bounding box of the specified bounds. */
		public function aabbIntersectsSkeleton(bounds : SkeletonBounds) : Boolean {
			return minX < bounds.maxX && maxX > bounds.minX && minY < bounds.maxY && maxY > bounds.minY;
		}

		/** Returns the first bounding box attachment that contains the point, or null. When doing many checks, it is usually more
		 * efficient to only call this method if {@link #aabbContainsPoint(float, float)} returns true. */
		public function containsPoint(x : Number, y : Number) : BoundingBoxAttachment {
			for (var i : int = 0, n : int = polygons.length; i < n; i++)
				if (polygons[i].containsPoint(x, y)) return boundingBoxes[i];
			return null;
		}

		/** Returns the first bounding box attachment that contains the line segment, or null. When doing many checks, it is usually
		 * more efficient to only call this method if {@link #aabbIntersectsSegment(float, float, float, float)} returns true. */
		public function intersectsSegment(x1 : Number, y1 : Number, x2 : Number, y2 : Number) : BoundingBoxAttachment {
			for (var i : int = 0, n : int = polygons.length; i < n; i++)
				if (polygons[i].intersectsSegment(x1, y1, x2, y2)) return boundingBoxes[i];
			return null;
		}

		public function getPolygon(attachment : BoundingBoxAttachment) : Polygon {
			var index : int = boundingBoxes.indexOf(attachment);
			return index == -1 ? null : polygons[index];
		}

		public function get width() : Number {
			return maxX - minX;
		}

		public function get height() : Number {
			return maxY - minY;
		}
	}
}