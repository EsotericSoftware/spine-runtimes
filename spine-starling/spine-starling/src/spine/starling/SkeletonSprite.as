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

package spine.starling {
import flash.geom.Matrix;
import flash.geom.Point;
import flash.geom.Rectangle;

import spine.Bone;
import spine.Skeleton;
import spine.SkeletonData;
import spine.Slot;
import spine.attachments.RegionAttachment;

import starling.animation.IAnimatable;
import starling.core.RenderSupport;
import starling.display.BlendMode;
import starling.display.DisplayObject;
import starling.filters.FragmentFilter
import starling.utils.Color;
import starling.utils.MatrixUtil;

public class SkeletonSprite extends DisplayObject implements IAnimatable {
	static private var tempPoint:Point = new Point();
	static private var tempMatrix:Matrix = new Matrix();

	private var _skeleton:Skeleton;
	private var vertices:Vector.<Number> = new Vector.<Number>();

	public function SkeletonSprite (skeletonData:SkeletonData) {
		Bone.yDown = true;

		_skeleton = new Skeleton(skeletonData);
		_skeleton.updateWorldTransform();
		
		vertices.length = 8;
	}

	public function advanceTime (delta:Number) : void {
		_skeleton.update(delta);
	}
    
    public function findSlot(name:String) : Slot {
        return _skeleton.findSlot(name);
    }

	override public function render (support:RenderSupport, alpha:Number) : void {
		alpha *= this.alpha * skeleton.a;
		var drawOrder:Vector.<Slot> = skeleton.drawOrder;
		for (var i:int = 0, n:int = drawOrder.length; i < n; i++) {
			var slot:Slot = drawOrder[i];
			var regionAttachment:RegionAttachment = slot.attachment as RegionAttachment;
			if (regionAttachment != null) {
				var vertices:Vector.<Number> = this.vertices;
				regionAttachment.computeWorldVertices(skeleton.x, skeleton.y, slot.bone, vertices);
				var r:Number = skeleton.r * slot.r * 255;
				var g:Number = skeleton.g * slot.g * 255;
				var b:Number = skeleton.b * slot.b * 255;
				var a:Number = slot.a;
				var rgb:uint = Color.rgb(r,g,b);

				var image:SkeletonImage = regionAttachment.rendererObject as SkeletonImage;
				var vertexData:Vector.<Number> = image.vertexData.rawData;
                
                image.vertexData.setPosition(0, vertices[2], vertices[3]);				
                image.vertexData.setColor(0, rgb);
                image.vertexData.setAlpha(0, a);
                
                image.vertexData.setPosition(1, vertices[4], vertices[5]);
                image.vertexData.setColor(1, rgb);
                image.vertexData.setAlpha(1, a);
                
                image.vertexData.setPosition(2, vertices[0], vertices[1]);
                image.vertexData.setColor(2, rgb);
                image.vertexData.setAlpha(2, a);
                
                image.vertexData.setPosition(3, vertices[6], vertices[7]);
                image.vertexData.setColor(3, rgb);
                image.vertexData.setAlpha(3, a);

				image.updateVertices();
				support.blendMode = slot.data.additiveBlending ? BlendMode.ADD : BlendMode.NORMAL;
				support.batchQuad(image, alpha, image.texture);
			}
            else
            {
                var skeletonAttachment:SkeletonAttachment = slot.attachment as SkeletonAttachment;
                if (skeletonAttachment != null) {
                    var skt:SkeletonAnimation = skeletonAttachment.skeletion;
                    
                    if (skt.hasVisibleArea) {
                        skt.x = slot.bone.worldX;
                        skt.y = slot.bone.worldY;
                        skt.rotation = slot.bone.worldRotation * (3.1415926) / 180;
                        skt.scaleX = slot.bone.worldScaleX;
                        skt.scaleY = slot.bone.worldScaleY;
                        
                        skt.alpha = slot.a;
                        
                        var filter:FragmentFilter = skt.filter;
                        
                        support.pushMatrix();
                        support.transformMatrix(skt);
                        support.blendMode = skt.blendMode;
                        
                        if (filter) filter.render(skt, support, alpha);
                        else        skt.render(support, alpha);
                        
                        support.blendMode = blendMode;
                        support.popMatrix();
                    }
                }
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

			var vertices:Vector.<Number> = this.vertices;
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
