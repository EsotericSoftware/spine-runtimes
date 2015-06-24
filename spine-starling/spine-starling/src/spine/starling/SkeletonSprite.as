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
import flash.display3D.Context3D;
import flash.display3D.textures.Texture;
import flash.geom.Matrix;
import flash.geom.Point;
import flash.geom.Rectangle;

import spine.Bone;
import spine.Skeleton;
import spine.SkeletonData;
import spine.Slot;
import spine.atlas.AtlasRegion;
import spine.attachments.Attachment;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;
import spine.attachments.SkinnedMeshAttachment;

import starling.core.RenderSupport;
import starling.core.Starling;
import starling.display.BlendMode;
import starling.display.DisplayObject;
import starling.utils.Color;
import starling.utils.MatrixUtil;
import starling.utils.VertexData;

public class SkeletonSprite extends DisplayObject {
	static private var _tempPoint:Point = new Point();
	static private var _tempMatrix:Matrix = new Matrix();
	static private var _tempVertices:Vector.<Number> = new Vector.<Number>(8);
	static private var _quadTriangles:Vector.<uint> = new <uint>[0, 1, 2, 2, 3, 0];
	static internal var blendModes:Vector.<String> = new <String>[
		BlendMode.NORMAL, BlendMode.ADD, BlendMode.MULTIPLY, BlendMode.SCREEN];

	private var _skeleton:Skeleton;
	private var _polygonBatch:PolygonBatch;
	public var batchable:Boolean = true;
	private var _batched:Boolean;
	private var _smoothing:String = "bilinear";

	/** @param renderMeshes If false, meshes won't be rendered. This may improve batching with non-Spine display objects. */
	public function SkeletonSprite (skeletonData:SkeletonData, renderMeshes:Boolean = true) {
		Bone.yDown = true;

		if (renderMeshes) _polygonBatch = new PolygonBatch();

		_skeleton = new Skeleton(skeletonData);
		_skeleton.updateWorldTransform();
	}

	override public function render (support:RenderSupport, alpha:Number) : void {
		alpha *= this.alpha * skeleton.a;
		var originalBlendMode:String = support.blendMode;
		if (_polygonBatch)
			renderMeshes(support, alpha);
		else
			renderRegions(support, alpha, originalBlendMode);
		support.blendMode = originalBlendMode;
	}

	private function renderMeshes (support:RenderSupport, alpha:Number) : void {
		if (!batchable) {
			_polygonBatch.begin(support, alpha, blendMode);
			addToBatch(_polygonBatch, support, alpha, null);
			_polygonBatch.end();
		} else if (!_batched) {
			support.popMatrix();
			_polygonBatch.begin(support, alpha, blendMode);
			addToBatch(_polygonBatch, support, alpha, transformationMatrix);
			for(var i:int = parent.getChildIndex(this) + 1, n:int = parent.numChildren; i < n; ++i) {
				var skeletonSprite:SkeletonSprite = parent.getChildAt(i) as SkeletonSprite;
				if (!skeletonSprite || !skeletonSprite.batchable || skeletonSprite.blendMode != blendMode) break;
				skeletonSprite._batched = true;
				skeletonSprite.addToBatch(_polygonBatch, support, alpha, skeletonSprite.transformationMatrix);
			}
			_polygonBatch.end();
			support.pushMatrix();
			support.transformMatrix(this);
		} else
			_batched = false;
	}

	private function addToBatch (polygonBatch:PolygonBatch, support:RenderSupport, skeletonA:Number, matrix:Matrix) : void {
		var skeletonR:Number = skeleton.r;
		var skeletonG:Number = skeleton.g;
		var skeletonB:Number = skeleton.b;
		var x:Number = skeleton.x;
		var y:Number = skeleton.y;
		var worldVertices:Vector.<Number> = _tempVertices;
		var drawOrder:Vector.<Slot> = skeleton.drawOrder;
		for (var i:int = 0, n:int = drawOrder.length; i < n; ++i) {
			var slot:Slot = drawOrder[i];
			var attachment:Attachment = slot.attachment;
			if (!attachment) continue;
			var image:SkeletonImage, verticesLength:int, uvs:Vector.<Number>, triangles:Vector.<uint>;
			var r:Number, g:Number, b:Number, a:Number;
			if (attachment is RegionAttachment) {
				var region:RegionAttachment = RegionAttachment(slot.attachment);
				verticesLength = 8;
				region.computeWorldVertices(x, y, slot.bone, worldVertices);
				uvs = region.uvs;
				triangles = _quadTriangles;
				r = region.r;
				g = region.g;
				b = region.b;
				a = region.a;
				image = region.rendererObject as SkeletonImage;
				if (image == null) region.rendererObject = image = SkeletonImage(AtlasRegion(region.rendererObject).rendererObject);
			} else if (attachment is MeshAttachment) {
				var mesh:MeshAttachment = MeshAttachment(attachment);
				verticesLength = mesh.vertices.length;
				if (worldVertices.length < verticesLength) worldVertices.length = verticesLength;
				mesh.computeWorldVertices(x, y, slot, worldVertices);
				uvs = mesh.uvs;
				triangles = mesh.triangles;
				r = mesh.r;
				g = mesh.g;
				b = mesh.b;
				a = mesh.a;
				image = mesh.rendererObject as SkeletonImage;
				if (image == null) mesh.rendererObject = image = SkeletonImage(AtlasRegion(mesh.rendererObject).rendererObject);
			} else if (attachment is SkinnedMeshAttachment) {
				var skinnedMesh:SkinnedMeshAttachment = SkinnedMeshAttachment(attachment);
				verticesLength = skinnedMesh.uvs.length;
				if (worldVertices.length < verticesLength) worldVertices.length = verticesLength;
				skinnedMesh.computeWorldVertices(x, y, slot, worldVertices);
				uvs = skinnedMesh.uvs;
				triangles = skinnedMesh.triangles;
				r = skinnedMesh.r;
				g = skinnedMesh.g;
				b = skinnedMesh.b;
				a = skinnedMesh.a;
				image = skinnedMesh.rendererObject as SkeletonImage;
				if (image == null) skinnedMesh.rendererObject = image = SkeletonImage(AtlasRegion(skinnedMesh.rendererObject).rendererObject);
			}
			if (image) {
				a *= skeletonA * slot.a;
				r *= skeletonR * slot.r * a;
				g *= skeletonG * slot.g * a;
				b *= skeletonB * slot.b * a;
				polygonBatch.add(image.texture, worldVertices, verticesLength, uvs, triangles, r, g, b, a, slot.data.blendMode, matrix);
			}
		}
	}

	private function renderRegions (support:RenderSupport, alpha:Number, blendMode:String) : void {
		var r:Number = skeleton.r * 255;
		var g:Number = skeleton.g * 255;
		var b:Number = skeleton.b * 255;
		var x:Number = skeleton.x;
		var y:Number = skeleton.y;
		var drawOrder:Vector.<Slot> = skeleton.drawOrder;
		var worldVertices:Vector.<Number> = _tempVertices;
		for (var i:int = 0, n:int = drawOrder.length; i < n; ++i) {
			var slot:Slot = drawOrder[i];
			var region:RegionAttachment = slot.attachment as RegionAttachment;
			if (region != null) {
				region.computeWorldVertices(x, y, slot.bone, worldVertices);
				var a:Number = slot.a * region.a;
				var rgb:uint = Color.rgb(
					r * slot.r * region.r,
					g * slot.g * region.g,
					b * slot.b * region.b);

				var image:SkeletonImage = region.rendererObject as SkeletonImage;
				if (image == null) region.rendererObject = image = SkeletonImage(AtlasRegion(region.rendererObject).rendererObject);

				var vertexData:VertexData = image.vertexData;
				vertexData.setPosition(0, worldVertices[2], worldVertices[3]);
				vertexData.setColorAndAlpha(0, rgb, a);
				
				vertexData.setPosition(1, worldVertices[4], worldVertices[5]);
				vertexData.setColorAndAlpha(1, rgb, a);
				
				vertexData.setPosition(2, worldVertices[0], worldVertices[1]);
				vertexData.setColorAndAlpha(2, rgb, a);
				
				vertexData.setPosition(3, worldVertices[6], worldVertices[7]);
				vertexData.setColorAndAlpha(3, rgb, a);
				
				image.updateVertices();
				support.blendMode = blendModes[slot.data.blendMode.ordinal];
				support.batchQuad(image, alpha, image.texture, _smoothing);
			}
		}
	}

	override public function hitTest (localPoint:Point, forTouch:Boolean = false) : DisplayObject {
		if (forTouch && (!visible || !touchable))
			return null;

		var minX:Number = Number.MAX_VALUE, minY:Number = Number.MAX_VALUE;
		var maxX:Number = Number.MIN_VALUE, maxY:Number = Number.MIN_VALUE;
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
				verticesLength = mesh.vertices.length;
				if (worldVertices.length < verticesLength) worldVertices.length = verticesLength;
				mesh.computeWorldVertices(0, 0, slot, worldVertices);
			} else if (attachment is SkinnedMeshAttachment) {
				var skinnedMesh:SkinnedMeshAttachment = SkinnedMeshAttachment(attachment);
				verticesLength = skinnedMesh.uvs.length;
				if (worldVertices.length < verticesLength) worldVertices.length = verticesLength;
				skinnedMesh.computeWorldVertices(0, 0, slot, worldVertices);
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
		if (_polygonBatch) _polygonBatch.smoothing = _smoothing;
	}
}

}
