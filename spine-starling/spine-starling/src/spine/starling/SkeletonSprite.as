/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.starling {
import spine.Bone;
import spine.Skeleton;
import spine.SkeletonData;
import spine.Slot;
import spine.atlas.AtlasRegion;
import spine.attachments.Attachment;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;

import starling.display.BlendMode;
import starling.display.DisplayObject;
import starling.display.Image;
import starling.rendering.IndexData;
import starling.rendering.Painter;
import starling.rendering.VertexData;
import starling.utils.Color;
import starling.utils.MatrixUtil;

import flash.geom.Matrix;
import flash.geom.Point;
import flash.geom.Rectangle;

public class SkeletonSprite extends DisplayObject {
	static private var _tempPoint:Point = new Point();
	static private var _tempMatrix:Matrix = new Matrix();
	static private var _tempVertices:Vector.<Number> = new Vector.<Number>(8);	
	static internal var blendModes:Vector.<String> = new <String>[
		BlendMode.NORMAL, BlendMode.ADD, BlendMode.MULTIPLY, BlendMode.SCREEN];

	private var _skeleton:Skeleton;		
	public var batchable:Boolean = true;	
	private var _smoothing:String = "bilinear";
	
	public function SkeletonSprite (skeletonData:SkeletonData) {
		Bone.yDown = true;
		_skeleton = new Skeleton(skeletonData);
		_skeleton.updateWorldTransform();
	}

	override public function render (painter:Painter) : void {
		alpha *= this.alpha * skeleton.a;		
		var originalBlendMode:String = painter.state.blendMode;
		var r:Number = skeleton.r * 255;
		var g:Number = skeleton.g * 255;
		var b:Number = skeleton.b * 255;
		var x:Number = skeleton.x;
		var y:Number = skeleton.y;
		var drawOrder:Vector.<Slot> = skeleton.drawOrder;
		var worldVertices:Vector.<Number> = _tempVertices;
		var ii:int, iii:int;
		var rgb:uint, a:Number;
		var mesh:SkeletonMesh;
		var verticesLength:int, verticesCount:int, indicesLength:int;		
		var indexData:IndexData, indices:Vector.<uint>, vertexData:VertexData;
		var uvs: Vector.<Number>;	
		
		for (var i:int = 0, n:int = drawOrder.length; i < n; ++i) {
			var slot:Slot = drawOrder[i];			
			if (slot.attachment is RegionAttachment) {
				var region:RegionAttachment = slot.attachment as RegionAttachment;
				region.computeWorldVertices(x, y, slot.bone, worldVertices);
				// FIXME pre-multiplied alpha?
				a = slot.a * region.a;
				rgb = Color.rgb(
					r * slot.r * region.r,
					g * slot.g * region.g,
					b * slot.b * region.b);

				var image:Image = region.rendererObject as Image;
				if (image == null) {
					var origImage:Image = Image(AtlasRegion(region.rendererObject).rendererObject);
					region.rendererObject = image = new Image(origImage.texture);
					for (var j:int = 0; j < 4; j++) {
						var p: Point = origImage.getTexCoords(j);
						image.setTexCoords(j, p.x, p.y);
					}
				}
				
				image.setVertexPosition(0, worldVertices[2], worldVertices[3]);
				image.setVertexColor(0, rgb);
				image.setVertexAlpha(0, a);
				
				image.setVertexPosition(1, worldVertices[4], worldVertices[5]);
				image.setVertexColor(1, rgb);
				image.setVertexAlpha(1, a);
				
				image.setVertexPosition(2, worldVertices[0], worldVertices[1]);
				image.setVertexColor(2, rgb);
				image.setVertexAlpha(2, a);
				
				image.setVertexPosition(3, worldVertices[6], worldVertices[7]);
				image.setVertexColor(3, rgb);
				image.setVertexAlpha(3, a);
				
				image.setRequiresRedraw();				
				painter.state.blendMode = blendModes[slot.data.blendMode.ordinal];
				// FIXME set smoothing/filter			
				painter.batchMesh(image);				
			} else if (slot.attachment is MeshAttachment) {
				var meshAttachment:MeshAttachment = MeshAttachment(slot.attachment);
				verticesLength  = meshAttachment.worldVerticesLength;
				verticesCount = verticesLength >> 1;				
				if (worldVertices.length < verticesLength) worldVertices.length = verticesLength;
				meshAttachment.computeWorldVertices(slot, worldVertices);
				mesh = meshAttachment.rendererObject as SkeletonMesh;
				if (mesh == null) {
					if (meshAttachment.rendererObject is Image) 
						meshAttachment.rendererObject = mesh = new SkeletonMesh(Image(meshAttachment.rendererObject).texture);
					if (meshAttachment.rendererObject is AtlasRegion)				
						meshAttachment.rendererObject = mesh = new SkeletonMesh(Image(AtlasRegion(meshAttachment.rendererObject).rendererObject).texture);
				}
								
				if (mesh.numIndices != meshAttachment.triangles.length) {
					indexData = mesh.getIndexData();
					indices = meshAttachment.triangles;
					indicesLength = meshAttachment.triangles.length;
					for (ii = 0; ii < indicesLength; ii++) {
						indexData.setIndex(ii, indices[ii]);
					}
					indexData.numIndices = indicesLength;
					indexData.trim();
				}
				
				// FIXME pre-multiplied alpha?
				a = slot.a * meshAttachment.a;
				rgb = Color.rgb(
					r * slot.r * meshAttachment.r,
					g * slot.g * meshAttachment.g,
					b * slot.b * meshAttachment.b);	
					
				vertexData = mesh.getVertexData();
				uvs = meshAttachment.uvs;
				vertexData.colorize("color", rgb, a);
				for (ii = 0, iii = 0; ii < verticesCount; ii++, iii+=2) {
					mesh.setVertexPosition(ii, worldVertices[iii], worldVertices[iii+1]);
					mesh.setTexCoords(ii, uvs[iii], uvs[iii+1]);			
				}
				vertexData.numVertices = verticesCount;
				// FIXME set smoothing/filter
				painter.batchMesh(mesh);																		
			}
		}
		painter.state.blendMode = originalBlendMode;
	}

	override public function hitTest (localPoint:Point) : DisplayObject {
		// FIXME what to do here?
//		if (forTouch && (!visible || !touchable))
//			return null;

		var minX:Number = Number.MAX_VALUE, minY:Number = Number.MAX_VALUE;
		var maxX:Number = -Number.MAX_VALUE, maxY:Number = -Number.MAX_VALUE;
		var slots:Vector.<Slot> = skeleton.slots;
		var worldVertices:Vector.<Number> = _tempVertices;
		for (var i:int = 0, n:int = slots.length; i < n; ++i) {
			var slot:Slot = slots[i];
			var attachment:Attachment = slot.attachment;
			if (!attachment) continue;
			var verticesLength:int;
			if (attachment is RegionAttachment) {
				var region:RegionAttachment = RegionAttachment(slot.attachment);
				verticesLength = 8;
				region.computeWorldVertices(0, 0, slot.bone, worldVertices);
			} else if (attachment is MeshAttachment) {
				var mesh:MeshAttachment = MeshAttachment(attachment);
				verticesLength = mesh.worldVerticesLength;
				if (worldVertices.length < verticesLength) worldVertices.length = verticesLength;
				mesh.computeWorldVertices(slot, worldVertices);			
			} else
				continue;
			for (var ii:int = 0; ii < verticesLength; ii += 2) {
				var x:Number = worldVertices[ii], y:Number = worldVertices[ii + 1];
				minX = minX < x ? minX : x;
				minY = minY < y ? minY : y;
				maxX = maxX > x ? maxX : x;
				maxY = maxY > y ? maxY : y;
			}
		}

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
			getTransformationMatrix(targetSpace, _tempMatrix);
			MatrixUtil.transformCoords(_tempMatrix, 0, 0, _tempPoint);
			resultRect.setTo(_tempPoint.x, _tempPoint.y, 0, 0);
		}
		return resultRect;
	}
	
	public function get skeleton () : Skeleton {
		return _skeleton;
	}

	public function get smoothing () : String {
		return _smoothing;
	}

	public function set smoothing (smoothing:String) : void {
		_smoothing = smoothing;		
	}
}

}
