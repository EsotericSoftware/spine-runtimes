package spine {
import flash.geom.Matrix;
import flash.geom.Point;
import flash.geom.Rectangle;

import spine.Bone;
import spine.Skeleton;
import spine.SkeletonData;
import spine.Slot;
import spine.attachments.RegionAttachment;

import starling.core.RenderSupport;
import starling.display.DisplayObject;
import starling.events.EnterFrameEvent;
import starling.events.Event;
import starling.utils.MatrixUtil;

public class SkeletonSprite extends DisplayObject {
	static private var tempPoint:Point = new Point();
	static private var tempMatrix:Matrix = new Matrix();

	private var _skeleton:Skeleton;
	public var timeScale:Number = 1;

	public function SkeletonSprite (skeletonData:SkeletonData) {
		_skeleton = new Skeleton(skeletonData);
		_skeleton.updateWorldTransform();

		addEventListener(Event.ENTER_FRAME, onEnterFrame);

		Bone.yDown = true;
	}

	protected function onEnterFrame (event:EnterFrameEvent) : void {
		_skeleton.update(event.passedTime * timeScale);
	}

	override public function render (support:RenderSupport, alpha:Number) : void {
		var drawOrder:Vector.<Slot> = skeleton.drawOrder;
		for (var i:int = 0, n:int = drawOrder.length; i < n; i++) {
			var slot:Slot = drawOrder[i];
			var regionAttachment:RegionAttachment = slot.attachment as RegionAttachment;
			if (regionAttachment != null) {
				regionAttachment.updateVertices(slot.bone);
				var vertices:Vector.<Number> = regionAttachment.vertices;
				var r:Number = skeleton.r * slot.r;
				var g:Number = skeleton.g * slot.g;
				var b:Number = skeleton.b * slot.b;
				var a:Number = skeleton.a * slot.a;

				var image:SkeletonImage = regionAttachment.texture as SkeletonImage;
				var vertexData:Vector.<Number> = image.vertexData.rawData;

				vertexData[0] = vertices[2];
				vertexData[1] = vertices[3];
				vertexData[2] = r;
				vertexData[3] = g;
				vertexData[4] = b;
				vertexData[5] = a;

				vertexData[8] = vertices[4];
				vertexData[9] = vertices[5];
				vertexData[10] = r;
				vertexData[11] = g;
				vertexData[12] = b;
				vertexData[13] = a;

				vertexData[16] = vertices[0];
				vertexData[17] = vertices[1];
				vertexData[18] = r;
				vertexData[19] = g;
				vertexData[20] = b;
				vertexData[21] = a;

				vertexData[24] = vertices[6];
				vertexData[25] = vertices[7];
				vertexData[26] = r;
				vertexData[27] = g;
				vertexData[28] = b;
				vertexData[29] = a;

				support.batchQuad(image, alpha, image.texture);
			}
		}
	}

	override public function getBounds (targetSpace:DisplayObject, resultRect:Rectangle = null) : Rectangle {
		var minX:Number = Number.MAX_VALUE, minY:Number = Number.MAX_VALUE;
		var maxX:Number = Number.MIN_VALUE, maxY:Number = Number.MIN_VALUE;
		var scaleX:Number = this.scaleX;
		var scaleY:Number = this.scaleY;
		var slots:Vector.<Slot> = skeleton.slots;
		var value:Number;
		for (var i:int = 0, n:int = slots.length; i < n; i++) {
			var slot:Slot = slots[i];
			var regionAttachment:RegionAttachment = slot.attachment as RegionAttachment;
			if (!regionAttachment)
				continue;

			regionAttachment.updateVertices(slot.bone);
			var vertices:Vector.<Number> = regionAttachment.vertices;

			value = vertices[0] * scaleX;
			if (value < minX)
				minX = value;
			if (value > maxX)
				maxX = value;

			value = vertices[1] * scaleY;
			if (value < minY)
				minY = value;
			if (value > maxY)
				maxY = value;

			value = vertices[2] * scaleX;
			if (value < minX)
				minX = value;
			if (value > maxX)
				maxX = value;

			value = vertices[3] * scaleY;
			if (value < minY)
				minY = value;
			if (value > maxY)
				maxY = value;

			value = vertices[4] * scaleX;
			if (value < minX)
				minX = value;
			if (value > maxX)
				maxX = value;

			value = vertices[5] * scaleY;
			if (value < minY)
				minY = value;
			if (value > maxY)
				maxY = value;

			value = vertices[6] * scaleX;
			if (value < minX)
				minX = value;
			if (value > maxX)
				maxX = value;

			value = vertices[7] * scaleY;
			if (value < minY)
				minY = value;
			if (value > maxY)
				maxY = value;
		}

		if (!resultRect)
			resultRect = new Rectangle();

		// FIXME
		resultRect.setTo(0, 0, 0, 0);
		return resultRect;
		// No idea why the below makes rendering very small. :( Returning 0,0 0x0 renders fine??
		if (targetSpace == this) {
			resultRect.x = minX;
			resultRect.y = minY;
			resultRect.width = maxX - minX;
			resultRect.height = maxY - minY;
		} else if (targetSpace == parent && rotation == 0.0) {
			resultRect.x = x + minX - pivotX * scaleX;
			resultRect.y = y + minY - pivotY * scaleY;
			resultRect.width = (maxX - minX) * scaleX;
			resultRect.height = (maxY - minY) * scaleY;
			if (scaleX < 0) {
				resultRect.width *= -1;
				resultRect.x -= resultRect.width;
			}
			if (scaleY < 0) {
				resultRect.height *= -1;
				resultRect.y -= resultRect.height;
			}
		} else {
			getTransformationMatrix(targetSpace, tempMatrix);
			MatrixUtil.transformCoords(tempMatrix, minX, minY, tempPoint);
			minX = tempPoint.x;
			minY = tempPoint.y;
			MatrixUtil.transformCoords(tempMatrix, maxX, maxY, tempPoint);
			if (minX > tempPoint.x) {
				maxX = minX;
				minX = tempPoint.x;
			} else
				maxX = tempPoint.x;
			if (minY > tempPoint.y) {
				maxY = minY;
				minY = tempPoint.y;
			} else
				maxY = tempPoint.y;
			resultRect.x = minX;
			resultRect.y = minY;
			resultRect.width = maxX - minX;
			resultRect.height = maxY - minY;
		}
		return resultRect;
	}

	public function get skeleton () : Skeleton {
		return _skeleton;
	}
}

}

