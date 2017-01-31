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

module spine {
	export class SkeletonBounds {
		minX = 0; minY = 0; maxX = 0; maxY = 0;
		boundingBoxes = new Array<BoundingBoxAttachment>();
		polygons = new Array<ArrayLike<number>>();
		private polygonPool = new Pool<ArrayLike<number>>(() => {
			return Utils.newFloatArray(16);
		});

		update (skeleton: Skeleton, updateAabb: boolean) {
			if (skeleton == null) throw new Error("skeleton cannot be null.");
			let boundingBoxes = this.boundingBoxes;
			let polygons = this.polygons;
			let polygonPool = this.polygonPool;
			let slots = skeleton.slots;
			let slotCount = slots.length;

			boundingBoxes.length = 0;
			polygonPool.freeAll(polygons);
			polygons.length = 0;

			for (let i = 0; i < slotCount; i++) {
				let slot = slots[i];
				let attachment = slot.getAttachment();
				if (attachment instanceof BoundingBoxAttachment) {
					let boundingBox = attachment as BoundingBoxAttachment;
					boundingBoxes.push(boundingBox);

					let polygon = polygonPool.obtain();
					if (polygon.length != boundingBox.worldVerticesLength) {
						polygon = Utils.newFloatArray(boundingBox.worldVerticesLength);
					}
					polygons.push(polygon);
					boundingBox.computeWorldVertices(slot, 0, boundingBox.worldVerticesLength, polygon, 0, 2);
				}
			}

			if (updateAabb) {
				this.aabbCompute();
			} else {
				this.minX = Number.POSITIVE_INFINITY;
				this.minY = Number.POSITIVE_INFINITY;
				this.maxX = Number.NEGATIVE_INFINITY;
				this.maxY = Number.NEGATIVE_INFINITY;
			}
		}

		aabbCompute () {
			let minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY, maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
			let polygons = this.polygons;
			for (let i = 0, n = polygons.length; i < n; i++) {
				let polygon = polygons[i];
				let vertices = polygon;
				for (let ii = 0, nn = polygon.length; ii < nn; ii += 2) {
					let x = vertices[ii];
					let y = vertices[ii + 1];
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
		aabbContainsPoint (x: number, y: number) {
			return x >= this.minX && x <= this.maxX && y >= this.minY && y <= this.maxY;
		}

		/** Returns true if the axis aligned bounding box intersects the line segment. */
		aabbIntersectsSegment (x1: number, y1: number, x2: number, y2: number) {
			let minX = this.minX;
			let minY = this.minY;
			let maxX = this.maxX;
			let maxY = this.maxY;
			if ((x1 <= minX && x2 <= minX) || (y1 <= minY && y2 <= minY) || (x1 >= maxX && x2 >= maxX) || (y1 >= maxY && y2 >= maxY))
				return false;
			let m = (y2 - y1) / (x2 - x1);
			let y = m * (minX - x1) + y1;
			if (y > minY && y < maxY) return true;
			y = m * (maxX - x1) + y1;
			if (y > minY && y < maxY) return true;
			let x = (minY - y1) / m + x1;
			if (x > minX && x < maxX) return true;
			x = (maxY - y1) / m + x1;
			if (x > minX && x < maxX) return true;
			return false;
		}

		/** Returns true if the axis aligned bounding box intersects the axis aligned bounding box of the specified bounds. */
		aabbIntersectsSkeleton (bounds: SkeletonBounds) {
			return this.minX < bounds.maxX && this.maxX > bounds.minX && this.minY < bounds.maxY && this.maxY > bounds.minY;
		}

		/** Returns the first bounding box attachment that contains the point, or null. When doing many checks, it is usually more
		 * efficient to only call this method if {@link #aabbContainsPoint(float, float)} returns true. */
		containsPoint (x: number, y: number): BoundingBoxAttachment {
			let polygons = this.polygons;
			for (let i = 0, n = polygons.length; i < n; i++)
				if (this.containsPointPolygon(polygons[i], x, y)) return this.boundingBoxes[i];
			return null;
		}

		/** Returns true if the polygon contains the point. */
		containsPointPolygon (polygon: ArrayLike<number>, x: number, y: number) {
			let vertices = polygon;
			let nn = polygon.length;

			let prevIndex = nn - 2;
			let inside = false;
			for (let ii = 0; ii < nn; ii += 2) {
				let vertexY = vertices[ii + 1];
				let prevY = vertices[prevIndex + 1];
				if ((vertexY < y && prevY >= y) || (prevY < y && vertexY >= y)) {
					let vertexX = vertices[ii];
					if (vertexX + (y - vertexY) / (prevY - vertexY) * (vertices[prevIndex] - vertexX) < x) inside = !inside;
				}
				prevIndex = ii;
			}
			return inside;
		}

		/** Returns the first bounding box attachment that contains any part of the line segment, or null. When doing many checks, it
		 * is usually more efficient to only call this method if {@link #aabbIntersectsSegment(float, float, float, float)} returns
		 * true. */
		intersectsSegment (x1: number, y1: number, x2: number, y2: number) {
			let polygons = this.polygons;
			for (let i = 0, n = polygons.length; i < n; i++)
				if (this.intersectsSegmentPolygon(polygons[i], x1, y1, x2, y2)) return this.boundingBoxes[i];
			return null;
		}

		/** Returns true if the polygon contains any part of the line segment. */
		intersectsSegmentPolygon (polygon: ArrayLike<number>, x1: number, y1: number, x2: number, y2: number) {
			let vertices = polygon;
			let nn = polygon.length;

			let width12 = x1 - x2, height12 = y1 - y2;
			let det1 = x1 * y2 - y1 * x2;
			let x3 = vertices[nn - 2], y3 = vertices[nn - 1];
			for (let ii = 0; ii < nn; ii += 2) {
				let x4 = vertices[ii], y4 = vertices[ii + 1];
				let det2 = x3 * y4 - y3 * x4;
				let width34 = x3 - x4, height34 = y3 - y4;
				let det3 = width12 * height34 - height12 * width34;
				let x = (det1 * width34 - width12 * det2) / det3;
				if (((x >= x3 && x <= x4) || (x >= x4 && x <= x3)) && ((x >= x1 && x <= x2) || (x >= x2 && x <= x1))) {
					let y = (det1 * height34 - height12 * det2) / det3;
					if (((y >= y3 && y <= y4) || (y >= y4 && y <= y3)) && ((y >= y1 && y <= y2) || (y >= y2 && y <= y1))) return true;
				}
				x3 = x4;
				y3 = y4;
			}
			return false;
		}

		/** Returns the polygon for the specified bounding box, or null. */
		getPolygon (boundingBox: BoundingBoxAttachment) {
			if (boundingBox == null) throw new Error("boundingBox cannot be null.");
			let index = this.boundingBoxes.indexOf(boundingBox);
			return index == -1 ? null : this.polygons[index];
		}

		getWidth () {
			return this.maxX - this.minX;
		}

		getHeight () {
			return this.maxY - this.minY;
		}
	}

}
