/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
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

package spine {
import spine.attachments.Attachment;
import spine.attachments.BoundingBoxAttachment;

public class SkeletonBounds {
	private var polygonPool:Vector.<Polygon> = new Vector.<Polygon>();

	public var boundingBoxes:Vector.<BoundingBoxAttachment> = new Vector.<BoundingBoxAttachment>();
	public var polygons:Vector.<Polygon> = new Vector.<Polygon>();
	public var minX:Number, minY:Number, maxX:Number, maxY:Number;

	public function update (skeleton:Skeleton, updateAabb:Boolean) : void {
		var slots:Vector.<Slot> = skeleton.slots;
		var slotCount:int = slots.length;
		var x:Number = skeleton.x, y:Number = skeleton.y;
		
		boundingBoxes.length = 0;
		for each (var polygon:Polygon in polygons)
			polygonPool[polygonPool.length] = polygon;
		polygons.length = 0;

		for (var i:int = 0; i < slotCount; i++) {
			var slot:Slot = slots[i];
			var boundingBox:BoundingBoxAttachment = slot.attachment as BoundingBoxAttachment;
			if (boundingBox == null) continue;
			boundingBoxes[boundingBoxes.length] = boundingBox;

			var poolCount:int = polygonPool.length;
			if (poolCount > 0) {
				polygon = polygonPool[poolCount - 1];
				polygonPool.splice(poolCount - 1, 1);
			} else
				polygon = new Polygon();
			polygons[polygons.length] = polygon;

			polygon.vertices.length = boundingBox.vertices.length;
			boundingBox.computeWorldVertices(x, y, slot.bone, polygon.vertices);
		}

		if (updateAabb) aabbCompute();
	}

	private function aabbCompute () : void {
		var minX:Number = int.MAX_VALUE, minY:Number = int.MAX_VALUE, maxX:Number = int.MIN_VALUE, maxY:Number = int.MIN_VALUE;
		for (var i:int = 0, n:int = polygons.length; i < n; i++) {
			var polygon:Polygon = polygons[i];
			var vertices:Vector.<Number> = polygon.vertices;
			for (var ii:int = 0, nn:int = vertices.length; ii < nn; ii += 2) {
				var x:Number = vertices[ii];
				var y:Number = vertices[ii + 1];
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
	public function aabbContainsPoint (x:Number, y:Number) : Boolean {
		return x >= minX && x <= maxX && y >= minY && y <= maxY;
	}
	
	/** Returns true if the axis aligned bounding box intersects the line segment. */
	public function aabbIntersectsSegment (x1:Number, y1:Number, x2:Number, y2:Number) : Boolean {
		if ((x1 <= minX && x2 <= minX) || (y1 <= minY && y2 <= minY) || (x1 >= maxX && x2 >= maxX) || (y1 >= maxY && y2 >= maxY))
			return false;
		var m:Number = (y2 - y1) / (x2 - x1);
		var y:Number = m * (minX - x1) + y1;
		if (y > minY && y < maxY) return true;
		y = m * (maxX - x1) + y1;
		if (y > minY && y < maxY) return true;
		var x:Number = (minY - y1) / m + x1;
		if (x > minX && x < maxX) return true;
		x = (maxY - y1) / m + x1;
		if (x > minX && x < maxX) return true;
		return false;
	}
	
	/** Returns true if the axis aligned bounding box intersects the axis aligned bounding box of the specified bounds. */
	public function aabbIntersectsSkeleton (bounds:SkeletonBounds) : Boolean {
		return minX < bounds.maxX && maxX > bounds.minX && minY < bounds.maxY && maxY > bounds.minY;
	}
	
	/** Returns the first bounding box attachment that contains the point, or null. When doing many checks, it is usually more
	 * efficient to only call this method if {@link #aabbContainsPoint(float, float)} returns true. */
	public function containsPoint (x:Number, y:Number) : BoundingBoxAttachment {
		for (var i:int = 0, n:int = polygons.length; i < n; i++)
			if (polygons[i].containsPoint(x, y)) return boundingBoxes[i];
		return null;
	}
	
	/** Returns the first bounding box attachment that contains the line segment, or null. When doing many checks, it is usually
	 * more efficient to only call this method if {@link #aabbIntersectsSegment(float, float, float, float)} returns true. */
	public function intersectsSegment (x1:Number, y1:Number, x2:Number, y2:Number) : BoundingBoxAttachment {
		for (var i:int = 0, n:int = polygons.length; i < n; i++)
			if (polygons[i].intersectsSegment(x1, y1, x2, y2)) return boundingBoxes[i];
		return null;
	}

	public function getPolygon (attachment:BoundingBoxAttachment) : Polygon {
		var index:int = boundingBoxes.indexOf(attachment);
		return index == -1 ? null : polygons[index];
	}

	public function get width () : Number {
		return maxX - minX;
	}
	
	public function get height () : Number {
		return maxY - minY;
	}
}

}
