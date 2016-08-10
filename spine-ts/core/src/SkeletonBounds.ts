module spine {
	export class SkeletonBounds {
		minX = 0; minY = 0; maxX = 0; maxY = 0;
		boundingBoxes = new Array<BoundingBoxAttachment>();
		polygons = new Array<Array<number>>();

		update (skeleton: Skeleton, updateAabb: boolean) {
			if (skeleton == null) throw new Error("skeleton cannot be null.");
			let boundingBoxes = this.boundingBoxes;
			let polygons = this.polygons;
			let slots = skeleton.slots;
			let slotCount = slots.length;

			boundingBoxes.length = 0;            
			polygons.length = 0;

			for (var i = 0; i < slotCount; i++) {
				let slot = slots[i];
				let attachment = slot.getAttachment();                
				if (attachment instanceof BoundingBoxAttachment) {
					let boundingBox = attachment as BoundingBoxAttachment;
					boundingBoxes.push(boundingBox);

					let polygon = new Array<number>();
					polygons.push(polygon);
					boundingBox.computeWorldVertices(slot, Utils.setArraySize(polygon, boundingBox.worldVerticesLength));
				}
			}

			if (updateAabb) this.aabbCompute();
		}

		aabbCompute () {
			var minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY, maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
			let polygons = this.polygons;
			for (var i = 0, n = polygons.length; i < n; i++) {
				let polygon = polygons[i];
				let vertices = polygon;
				for (var ii = 0, nn = polygon.length; ii < nn; ii += 2) {
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
			for (var i = 0, n = polygons.length; i < n; i++)
				if (this.containsPointPolygon(polygons[i], x, y)) return this.boundingBoxes[i];
			return null;
		}

		/** Returns true if the polygon contains the point. */
		containsPointPolygon (polygon: Array<number>, x: number, y: number) {
			let vertices = polygon;
			let nn = polygon.length;

			let prevIndex = nn - 2;
			var inside = false;
			for (var ii = 0; ii < nn; ii += 2) {
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
			for (var i = 0, n = polygons.length; i < n; i++)
				if (this.intersectsSegmentPolygon(polygons[i], x1, y1, x2, y2)) return this.boundingBoxes[i];
			return null;
		}

		/** Returns true if the polygon contains any part of the line segment. */
		intersectsSegmentPolygon (polygon: Array<number>, x1: number, y1: number, x2: number, y2: number) {
			let vertices = polygon;
			let nn = polygon.length;

			let width12 = x1 - x2, height12 = y1 - y2;
			let det1 = x1 * y2 - y1 * x2;
			let x3 = vertices[nn - 2], y3 = vertices[nn - 1];
			for (var ii = 0; ii < nn; ii += 2) {
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
			// FIXME identity equals used in indexOf?
			let index = this.boundingBoxes.indexOf(boundingBox);
			return index == -1 ? null : this.polygons[index];
		}
	}

}
