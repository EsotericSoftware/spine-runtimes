/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.starling;

import starling.animation.IAnimatable;
import openfl.geom.Matrix;
import openfl.geom.Point;
import openfl.geom.Rectangle;
import spine.Bone;
import spine.Skeleton;
import spine.SkeletonClipping;
import spine.SkeletonData;
import spine.Slot;
import spine.animation.AnimationState;
import spine.animation.AnimationStateData;
import spine.attachments.Attachment;
import spine.attachments.ClippingAttachment;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;
import starling.display.BlendMode;
import starling.display.DisplayObject;
import starling.rendering.IndexData;
import starling.rendering.Painter;
import starling.rendering.VertexData;
import starling.textures.Texture;
import starling.utils.Color;
import starling.utils.MatrixUtil;
import starling.utils.Max;

class SkeletonSprite extends DisplayObject implements IAnimatable {
	static private var _tempPoint:Point = new Point();
	static private var _tempMatrix:Matrix = new Matrix();
	static private var _tempVertices:Array<Float> = new Array<Float>();
	static private var blendModes:Array<String> = [BlendMode.NORMAL, BlendMode.ADD, BlendMode.MULTIPLY, BlendMode.SCREEN];

	private var _skeleton:Skeleton;

	public var _state:AnimationState;

	private var _smoothing:String = "bilinear";

	public static var clipper(default, never):SkeletonClipping = new SkeletonClipping();
	private static var QUAD_INDICES:Array<Int> = [0, 1, 2, 2, 3, 0];

	private var tempLight:spine.Color = new spine.Color(0, 0, 0);
	private var tempDark:spine.Color = new spine.Color(0, 0, 0);

	public function new(skeletonData:SkeletonData, animationStateData:AnimationStateData = null) {
		super();
		Bone.yDown = true;
		_skeleton = new Skeleton(skeletonData);
		_skeleton.updateWorldTransform(Physics.update);
		_state = new AnimationState(animationStateData != null ? animationStateData : new AnimationStateData(skeletonData));
	}

	override public function render(painter:Painter):Void {
		var clipper:SkeletonClipping = SkeletonSprite.clipper;
		painter.state.alpha *= skeleton.color.a;
		var originalBlendMode:String = painter.state.blendMode;
		var r:Float = skeleton.color.r * 255;
		var g:Float = skeleton.color.g * 255;
		var b:Float = skeleton.color.b * 255;
		var drawOrder:Array<Slot> = skeleton.drawOrder;
		var attachmentColor:spine.Color;
		var rgb:Int;
		var a:Float;
		var dark:Int;
		var mesh:SkeletonMesh = null;
		var verticesLength:Int;
		var verticesCount:Int;
		var indicesLength:Int;
		var indexData:IndexData;
		var indices:Array<Int> = null;
		var vertexData:VertexData;
		var uvs:Array<Float>;

		for (slot in drawOrder) {
			if (!slot.bone.active) {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			var worldVertices:Array<Float> = _tempVertices;
			if (Std.isOfType(slot.attachment, RegionAttachment)) {
				var region:RegionAttachment = cast(slot.attachment, RegionAttachment);
				verticesLength = 8;
				verticesCount = verticesLength >> 1;
				if (worldVertices.length < verticesLength)
					worldVertices.resize(verticesLength);
				region.computeWorldVertices(slot, worldVertices, 0, 2);

				mesh = null;
				if (Std.isOfType(region.rendererObject, SkeletonMesh)) {
					mesh = cast(region.rendererObject, SkeletonMesh);
					mesh.texture = region.region.texture;
					indices = QUAD_INDICES;
				} else {
					mesh = region.rendererObject = new SkeletonMesh(cast(region.region.texture, Texture));

					indexData = mesh.getIndexData();
					indices = QUAD_INDICES;
					for (i in 0...indices.length) {
						indexData.setIndex(i, indices[i]);
					}
					indexData.numIndices = indices.length;
					indexData.trim();
				}

				indexData = mesh.getIndexData();
				attachmentColor = region.color;
				uvs = region.uvs;
			} else if (Std.isOfType(slot.attachment, MeshAttachment)) {
				var meshAttachment:MeshAttachment = cast(slot.attachment, MeshAttachment);
				verticesLength = meshAttachment.worldVerticesLength;
				verticesCount = verticesLength >> 1;
				if (worldVertices.length < verticesLength)
					worldVertices.resize(verticesLength);
				meshAttachment.computeWorldVertices(slot, 0, meshAttachment.worldVerticesLength, worldVertices, 0, 2);

				mesh = null;
				if (Std.isOfType(meshAttachment.rendererObject, SkeletonMesh)) {
					mesh = cast(meshAttachment.rendererObject, SkeletonMesh);
					mesh.texture = meshAttachment.region.texture;
					indices = meshAttachment.triangles;
				} else {
					mesh = meshAttachment.rendererObject = new SkeletonMesh(cast(meshAttachment.region.texture, Texture));

					indexData = mesh.getIndexData();
					indices = meshAttachment.triangles;
					indicesLength = indices.length;
					for (i in 0...indicesLength) {
						indexData.setIndex(i, indices[i]);
					}
					indexData.numIndices = indicesLength;
					indexData.trim();
				}

				indexData = mesh.getIndexData();
				attachmentColor = meshAttachment.color;
				uvs = meshAttachment.uvs;
			} else if (Std.isOfType(slot.attachment, ClippingAttachment)) {
				var clip:ClippingAttachment = cast(slot.attachment, ClippingAttachment);
				clipper.clipStart(slot, clip);
				continue;
			} else {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			a = slot.color.a * attachmentColor.a;
			if (a == 0) {
				clipper.clipEndWithSlot(slot);
				continue;
			}
			rgb = Color.rgb(Std.int(r * slot.color.r * attachmentColor.r), Std.int(g * slot.color.g * attachmentColor.g),
				Std.int(b * slot.color.b * attachmentColor.b));
			if (slot.darkColor == null) {
				dark = Color.rgb(0, 0, 0);
			} else {
				dark = Color.rgb(Std.int(slot.darkColor.r * 255), Std.int(slot.darkColor.g * 255), Std.int(slot.darkColor.b * 255));
			}

			if (clipper.isClipping()) {
				clipper.clipTriangles(worldVertices, indices, indices.length, uvs);

				// Need to create a new mesh here, see https://github.com/EsotericSoftware/spine-runtimes/issues/1125
				mesh = new SkeletonMesh(mesh.texture);
				indexData = mesh.getIndexData();

				verticesCount = clipper.clippedVertices.length >> 1;
				worldVertices = clipper.clippedVertices;
				uvs = clipper.clippedUvs;

				indices = clipper.clippedTriangles;
				indicesLength = indices.length;
				indexData.numIndices = indicesLength;
				indexData.trim();
				for (i in 0...indicesLength) {
					indexData.setIndex(i, indices[i]);
				}
			}

			vertexData = mesh.getVertexData();
			vertexData.numVertices = verticesCount;
			vertexData.colorize("color", rgb, a);
			var ii:Int = 0;
			for (i in 0...verticesCount) {
				mesh.setVertexPosition(i, worldVertices[ii], worldVertices[ii + 1]);
				mesh.setTexCoords(i, uvs[ii], uvs[ii + 1]);
				ii += 2;
			}

			if (indexData.numIndices > 0 && vertexData.numVertices > 0) {
				painter.state.blendMode = blendModes[slot.data.blendMode.ordinal];
				painter.batchMesh(mesh);
			}

			clipper.clipEndWithSlot(slot);
		}
		painter.state.blendMode = originalBlendMode;
		clipper.clipEnd();
	}

	override public function hitTest(localPoint:Point):DisplayObject {
		if (!this.visible || !this.touchable)
			return null;

		var minX:Float = Max.MAX_VALUE;
		var minY:Float = Max.MAX_VALUE;
		var maxX:Float = -Max.MAX_VALUE;
		var maxY:Float = -Max.MAX_VALUE;
		var slots:Array<Slot> = skeleton.slots;
		var worldVertices:Array<Float> = _tempVertices;
		var empty:Bool = true;
		for (i in 0...slots.length) {
			var slot:Slot = slots[i];
			var attachment:Attachment = slot.attachment;
			if (attachment == null)
				continue;
			var verticesLength:Int;
			if (Std.isOfType(attachment, RegionAttachment)) {
				var region:RegionAttachment = cast(slot.attachment, RegionAttachment);
				verticesLength = 8;
				region.computeWorldVertices(slot, worldVertices, 0, 2);
			} else if (Std.isOfType(attachment, MeshAttachment)) {
				var mesh:MeshAttachment = cast(attachment, MeshAttachment);
				verticesLength = mesh.worldVerticesLength;
				if (worldVertices.length < verticesLength)
					worldVertices.resize(verticesLength);
				mesh.computeWorldVertices(slot, 0, verticesLength, worldVertices, 0, 2);
			} else {
				continue;
			}

			if (verticesLength != 0) {
				empty = false;
			}

			var ii:Int = 0;
			while (ii < verticesLength) {
				var x:Float = worldVertices[ii],
					y:Float = worldVertices[ii + 1];
				minX = minX < x ? minX : x;
				minY = minY < y ? minY : y;
				maxX = maxX > x ? maxX : x;
				maxY = maxY > y ? maxY : y;
				ii += 2;
			}
		}

		if (empty) {
			return null;
		}

		var temp:Float;
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

		if (localPoint.x >= minX && localPoint.x < maxX && localPoint.y >= minY && localPoint.y < maxY) {
			return this;
		}

		return null;
	}

	override public function getBounds(targetSpace:DisplayObject, resultRect:Rectangle = null):Rectangle {
		if (resultRect == null) {
			resultRect = new Rectangle();
		}
		if (targetSpace == this) {
			resultRect.setTo(0, 0, 0, 0);
		} else if (targetSpace == parent) {
			resultRect.setTo(x, y, 0, 0);
		} else {
			getTransformationMatrix(targetSpace, _tempMatrix);
			MatrixUtil.transformCoords(_tempMatrix, 0, 0, _tempPoint);
			resultRect.setTo(_tempPoint.x, _tempPoint.y, 0, 0);
		}
		return resultRect;
	}

	public var skeleton(get, never):Skeleton;

	private function get_skeleton():Skeleton {
		return _skeleton;
	}

	public var state(get, never):AnimationState;

	private function get_state():AnimationState {
		return _state;
	}

	public var smoothing(get, set):String;

	private function get_smoothing():String {
		return _smoothing;
	}

	private function set_smoothing(smoothing:String):String {
		_smoothing = smoothing;
		return _smoothing;
	}

	public function advanceTime(time:Float):Void {
		_state.update(time);
		_state.apply(skeleton);
		skeleton.update(time);
		skeleton.updateWorldTransform(Physics.update);
		this.setRequiresRedraw();
	}
}
