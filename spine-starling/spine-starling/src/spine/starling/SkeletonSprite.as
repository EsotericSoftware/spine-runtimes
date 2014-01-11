/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.starling {
import flash.geom.Matrix;
import flash.geom.Point;
import flash.geom.Rectangle;

import spine.Bone;
import spine.Skeleton;
import spine.SkeletonData;
import spine.Slot;
import spine.atlas.AtlasPage;
import spine.atlas.AtlasRegion;
import spine.attachments.RegionAttachment;

import starling.animation.IAnimatable;
import starling.core.RenderSupport;
import starling.display.BlendMode;
import starling.display.DisplayObject;
import starling.utils.Color;
import starling.utils.MatrixUtil;
import starling.utils.VertexData;

public class SkeletonSprite extends DisplayObject implements IAnimatable {
	static private var tempPoint:Point = new Point();
	static private var tempMatrix:Matrix = new Matrix();
	static private var tempVertices:Vector.<Number> = new Vector.<Number>(8);

	private var _skeleton:Skeleton;

	public function SkeletonSprite (skeletonData:SkeletonData) {
		Bone.yDown = true;

		_skeleton = new Skeleton(skeletonData);
		_skeleton.updateWorldTransform();
	}

	public function advanceTime (delta:Number) : void {
		_skeleton.update(delta);
	}

	override public function render (support:RenderSupport, alpha:Number) : void {
		alpha *= this.alpha * skeleton.a;
		var r:Number = skeleton.r * 255;
		var g:Number = skeleton.g * 255;
		var b:Number = skeleton.b * 255;
		var x:Number = skeleton.x;
		var y:Number = skeleton.y;
		var drawOrder:Vector.<Slot> = skeleton.drawOrder;
		for (var i:int = 0, n:int = drawOrder.length; i < n; i++) {
			var slot:Slot = drawOrder[i];
			var regionAttachment:RegionAttachment = slot.attachment as RegionAttachment;
			if (regionAttachment != null) {
				var vertices:Vector.<Number> = tempVertices;
				regionAttachment.computeWorldVertices(x, y, slot.bone, vertices);
				var a:Number = slot.a;
				var rgb:uint = Color.rgb(r * slot.r, g * slot.g, b * slot.b);

				var image:SkeletonImage;
				image = regionAttachment.rendererObject as SkeletonImage;
				if (image == null) {
					image = SkeletonImage(AtlasRegion(regionAttachment.rendererObject).rendererObject);
					regionAttachment.rendererObject = image;
				}

				var vertexData:VertexData = image.vertexData;

				vertexData.setPosition(0, vertices[2], vertices[3]);
				vertexData.setColorAndAlpha(0, rgb, a);

				vertexData.setPosition(1, vertices[4], vertices[5]);
				vertexData.setColorAndAlpha(1, rgb, a);
				
				vertexData.setPosition(2, vertices[0], vertices[1]);
				vertexData.setColorAndAlpha(2, rgb, a);

				vertexData.setPosition(3, vertices[6], vertices[7]);
				vertexData.setColorAndAlpha(3, rgb, a);

				image.updateVertices();
				support.blendMode = slot.data.additiveBlending ? BlendMode.ADD : BlendMode.NORMAL;
				support.batchQuad(image, alpha, image.texture);
			}
		}
	}

	override public function hitTest (localPoint:Point, forTouch:Boolean = false) : DisplayObject {
		if (forTouch && (!visible || !touchable))
			return null;

		var minX:Number = Number.MAX_VALUE, minY:Number = Number.MAX_VALUE;
		var maxX:Number = Number.MIN_VALUE, maxY:Number = Number.MIN_VALUE;
		var slots:Vector.<Slot> = skeleton.slots;
		var value:Number;
		for (var i:int = 0, n:int = slots.length; i < n; i++) {
			var slot:Slot = slots[i];
			var regionAttachment:RegionAttachment = slot.attachment as RegionAttachment;
			if (!regionAttachment)
				continue;

			var vertices:Vector.<Number> = tempVertices;
			regionAttachment.computeWorldVertices(skeleton.x, skeleton.y, slot.bone, vertices);

			value = vertices[0];
			if (value < minX)
				minX = value;
			if (value > maxX)
				maxX = value;

			value = vertices[1];
			if (value < minY)
				minY = value;
			if (value > maxY)
				maxY = value;

			value = vertices[2];
			if (value < minX)
				minX = value;
			if (value > maxX)
				maxX = value;

			value = vertices[3];
			if (value < minY)
				minY = value;
			if (value > maxY)
				maxY = value;

			value = vertices[4];
			if (value < minX)
				minX = value;
			if (value > maxX)
				maxX = value;

			value = vertices[5];
			if (value < minY)
				minY = value;
			if (value > maxY)
				maxY = value;

			value = vertices[6];
			if (value < minX)
				minX = value;
			if (value > maxX)
				maxX = value;

			value = vertices[7];
			if (value < minY)
				minY = value;
			if (value > maxY)
				maxY = value;
		}

		minX *= scaleX;
		maxX *= scaleX;
		minY *= scaleY;
		maxY *= scaleY;
		var temp:Number;
		if (maxX < minX) {
			temp = maxX;
			maxX = minX;
			minX = temp;
		}
		if (maxY < minY) {
			temp = maxY;
			maxY = minY;
			minY = temp;
		}

		if (localPoint.x >= minX && localPoint.x < maxX && localPoint.y >= minY && localPoint.y < maxY)
			return this;

		return null;
	}

	override public function getBounds (targetSpace:DisplayObject, resultRect:Rectangle = null) : Rectangle {
		if (!resultRect)
			resultRect = new Rectangle();
		if (targetSpace == this)
			resultRect.setTo(0, 0, 0, 0);
		else if (targetSpace == parent)
			resultRect.setTo(x, y, 0, 0);
		else {
			getTransformationMatrix(targetSpace, tempMatrix);
			MatrixUtil.transformCoords(tempMatrix, 0, 0, tempPoint);
			resultRect.setTo(tempPoint.x, tempPoint.y, 0, 0);
		}
		return resultRect;
	}

	public function get skeleton () : Skeleton {
		return _skeleton;
	}
}

}
