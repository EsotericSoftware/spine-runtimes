/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.starling {
	import starling.styles.MeshStyle;
	import spine.attachments.ClippingAttachment;
	import spine.SkeletonClipping;
	import spine.Bone;
	import spine.Skeleton;
	import spine.SkeletonData;
	import spine.Slot;
	import spine.atlas.AtlasRegion;
	import spine.attachments.Attachment;
	import spine.attachments.MeshAttachment;
	import spine.attachments.RegionAttachment;
	import spine.VertexEffect;

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
		static private var _tempPoint : Point = new Point();
		static private var _tempMatrix : Matrix = new Matrix();
		static private var _tempVertices : Vector.<Number> = new Vector.<Number>(8);
		static internal var blendModes : Vector.<String> = new <String>[BlendMode.NORMAL, BlendMode.ADD, BlendMode.MULTIPLY, BlendMode.SCREEN];
		private var _skeleton : Skeleton;
		private var _smoothing : String = "bilinear";
		private var _twoColorTint : Boolean = false;
		private static var clipper: SkeletonClipping = new SkeletonClipping();
		private static var QUAD_INDICES : Vector.<uint> = new <uint>[0, 1, 2, 2, 3, 0];

		public var vertexEffect : VertexEffect;
		private var tempLight : spine.Color = new spine.Color(0, 0, 0);
		private var tempDark : spine.Color = new spine.Color(0, 0, 0);
		private var tempVertex : spine.Vertex = new spine.Vertex();

		public function SkeletonSprite(skeletonData : SkeletonData) {
			Bone.yDown = true;
			_skeleton = new Skeleton(skeletonData);
			_skeleton.updateWorldTransform();
		}

		override public function render(painter : Painter) : void {
			var clipper: SkeletonClipping = SkeletonSprite.clipper;
			painter.state.alpha *= skeleton.color.a;
			var originalBlendMode : String = painter.state.blendMode;
			var r : Number = skeleton.color.r * 255;
			var g : Number = skeleton.color.g * 255;
			var b : Number = skeleton.color.b * 255;
			var drawOrder : Vector.<Slot> = skeleton.drawOrder;
			var ii : int, iii : int;
			var attachmentColor: spine.Color;
			var rgb : uint, a : Number;
			var dark : uint;
			var mesh : SkeletonMesh;
			var verticesLength : int, verticesCount : int, indicesLength : int;
			var indexData : IndexData, indices : Vector.<uint>, vertexData : VertexData;
			var uvs : Vector.<Number>;

			if (vertexEffect != null) vertexEffect.begin(skeleton);

			for (var i : int = 0, n : int = drawOrder.length; i < n; ++i) {
				var worldVertices : Vector.<Number> = _tempVertices;
				var slot : Slot = drawOrder[i];
				if (!slot.bone.active) continue;

				if (slot.attachment is RegionAttachment) {
					var region : RegionAttachment = slot.attachment as RegionAttachment;
					verticesLength = 4 * 2;
					verticesCount = verticesLength >> 1;
					if (worldVertices.length < verticesLength) worldVertices.length = verticesLength;
					region.computeWorldVertices(slot.bone, worldVertices, 0, 2);

					mesh = region.rendererObject as SkeletonMesh;
					indices = QUAD_INDICES;
					if (mesh == null) {
						if (region.rendererObject is Image)
							region.rendererObject = mesh = new SkeletonMesh(Image(region.rendererObject).texture);
						if (region.rendererObject is AtlasRegion)
							region.rendererObject = mesh = new SkeletonMesh(Image(AtlasRegion(region.rendererObject).rendererObject).texture);
						if (_twoColorTint) mesh.setStyle(new TwoColorMeshStyle());
						indexData = mesh.getIndexData();
						for (ii = 0; ii < indices.length; ii++)
							indexData.setIndex(ii, indices[ii]);
						indexData.numIndices = indices.length;
						indexData.trim();
					}
					indexData = mesh.getIndexData();
					attachmentColor = region.color;
					uvs = region.uvs;
				} else if (slot.attachment is MeshAttachment) {
					var meshAttachment : MeshAttachment = MeshAttachment(slot.attachment);
					verticesLength = meshAttachment.worldVerticesLength;
					verticesCount = verticesLength >> 1;
					if (worldVertices.length < verticesLength) worldVertices.length = verticesLength;
					meshAttachment.computeWorldVertices(slot, 0, meshAttachment.worldVerticesLength, worldVertices, 0, 2);

					mesh = meshAttachment.rendererObject as SkeletonMesh;
					indices = meshAttachment.triangles;
					if (mesh == null) {
						if (meshAttachment.rendererObject is Image)
							meshAttachment.rendererObject = mesh = new SkeletonMesh(Image(meshAttachment.rendererObject).texture);
						if (meshAttachment.rendererObject is AtlasRegion)
							meshAttachment.rendererObject = mesh = new SkeletonMesh(Image(AtlasRegion(meshAttachment.rendererObject).rendererObject).texture);
						if (_twoColorTint) mesh.setStyle(new TwoColorMeshStyle());

						indexData = mesh.getIndexData();
						indicesLength = meshAttachment.triangles.length;
						for (ii = 0; ii < indicesLength; ii++) {
							indexData.setIndex(ii, indices[ii]);
						}
						indexData.numIndices = indicesLength;
						indexData.trim();
					}
					indexData = mesh.getIndexData();
					attachmentColor = meshAttachment.color;
					uvs = meshAttachment.uvs;
				} else if (slot.attachment is ClippingAttachment) {
					var clip : ClippingAttachment = ClippingAttachment(slot.attachment);
					clipper.clipStart(slot, clip);
					continue;
				} else {
					continue;
				}

				a = slot.color.a * attachmentColor.a;
				if (a == 0) {
					clipper.clipEndWithSlot(slot);
					continue;
				}
				rgb = Color.rgb(r * slot.color.r * attachmentColor.r, g * slot.color.g * attachmentColor.g, b * slot.color.b * attachmentColor.b);
				if (slot.darkColor == null) dark = Color.rgb(0, 0, 0);
				else dark = Color.rgb(slot.darkColor.r * 255, slot.darkColor.g * 255, slot.darkColor.b * 255);

				if (clipper.isClipping()) {
					clipper.clipTriangles(worldVertices, indices, indices.length, uvs);

					// Need to create a new mesh here, see https://github.com/EsotericSoftware/spine-runtimes/issues/1125
					mesh = new SkeletonMesh(mesh.texture);
					if (_twoColorTint) mesh.setStyle(new TwoColorMeshStyle());
					indexData = mesh.getIndexData();

					verticesCount = clipper.clippedVertices.length >> 1;
					worldVertices = clipper.clippedVertices;
					uvs = clipper.clippedUvs;

					indices = clipper.clippedTriangles;
					indicesLength = indices.length;
					indexData.numIndices = indicesLength;
					indexData.trim();
					for (ii = 0; ii < indicesLength; ii++) {
						indexData.setIndex(ii, indices[ii]);
					}
				}

				vertexData = mesh.getVertexData();
				vertexData.numVertices = verticesCount;
				if (vertexEffect != null) {
					tempLight.r = ((rgb >> 16) & 0xff) / 255.0;
					tempLight.g = ((rgb >> 8) & 0xff) / 255.0;
					tempLight.b = (rgb & 0xff) / 255.0;
					tempLight.a = a;
					tempDark.r = ((dark >> 16) & 0xff) / 255.0;
					tempDark.g = ((dark >> 8) & 0xff) / 255.0;
					tempDark.b = (dark & 0xff) / 255.0;
					tempDark.a = 0;
					for (ii = 0, iii = 0; ii < verticesCount; ii++, iii += 2) {
						tempVertex.x = worldVertices[iii];
						tempVertex.y = worldVertices[iii + 1];
						tempVertex.u = uvs[iii];
						tempVertex.v = uvs[iii + 1];
						tempVertex.light.setFromColor(tempLight);
						tempVertex.dark.setFromColor(tempDark);
						vertexEffect.transform(tempVertex);
						vertexData.colorize("color", Color.rgb(tempVertex.light.r * 255, tempVertex.light.g * 255, tempVertex.light.b * 255), tempVertex.light.a, ii, 1);
						if (_twoColorTint) vertexData.colorize("color2", Color.rgb(tempVertex.dark.r * 255, tempVertex.dark.g * 255, tempVertex.dark.b * 255), a, ii, 1);
						mesh.setVertexPosition(ii, tempVertex.x, tempVertex.y);
						mesh.setTexCoords(ii, tempVertex.u, tempVertex.v);
					}
				} else {
					vertexData.colorize("color", rgb, a);
					if (_twoColorTint) vertexData.colorize("color2", dark);
					for (ii = 0, iii = 0; ii < verticesCount; ii++, iii += 2) {
						mesh.setVertexPosition(ii, worldVertices[iii], worldVertices[iii + 1]);
						mesh.setTexCoords(ii, uvs[iii], uvs[iii + 1]);
					}
				}
				if (indexData.numIndices > 0 && vertexData.numVertices > 0) {
					painter.state.blendMode = blendModes[slot.data.blendMode.ordinal];
					painter.batchMesh(mesh);
				}

				clipper.clipEndWithSlot(slot);
			}
			painter.state.blendMode = originalBlendMode;
			clipper.clipEnd();

			if (vertexEffect != null) vertexEffect.end();
		}

		override public function hitTest(localPoint : Point) : DisplayObject {
			if (!this.visible || !this.touchable) return null;

			var minX : Number = Number.MAX_VALUE, minY : Number = Number.MAX_VALUE;
			var maxX : Number = -Number.MAX_VALUE, maxY : Number = -Number.MAX_VALUE;
			var slots : Vector.<Slot> = skeleton.slots;
			var worldVertices : Vector.<Number> = _tempVertices;
			var empty : Boolean = true;
			for (var i : int = 0, n : int = slots.length; i < n; ++i) {
				var slot : Slot = slots[i];
				var attachment : Attachment = slot.attachment;
				if (!attachment) continue;
				var verticesLength : int;
				if (attachment is RegionAttachment) {
					var region : RegionAttachment = RegionAttachment(slot.attachment);
					verticesLength = 8;
					region.computeWorldVertices(slot.bone, worldVertices, 0, 2);
				} else if (attachment is MeshAttachment) {
					var mesh : MeshAttachment = MeshAttachment(attachment);
					verticesLength = mesh.worldVerticesLength;
					if (worldVertices.length < verticesLength) worldVertices.length = verticesLength;
					mesh.computeWorldVertices(slot, 0, verticesLength, worldVertices, 0, 2);
				} else
					continue;

				if (verticesLength != 0)
					empty = false;

				for (var ii : int = 0; ii < verticesLength; ii += 2) {
					var x : Number = worldVertices[ii], y : Number = worldVertices[ii + 1];
					minX = minX < x ? minX : x;
					minY = minY < y ? minY : y;
					maxX = maxX > x ? maxX : x;
					maxY = maxY > y ? maxY : y;
				}
			}

			if (empty)
				return null;

			var temp : Number;
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

		override public function getBounds(targetSpace : DisplayObject, resultRect : Rectangle = null) : Rectangle {
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

		public function get skeleton() : Skeleton {
			return _skeleton;
		}

		public function get smoothing() : String {
			return _smoothing;
		}

		public function set smoothing(smoothing : String) : void {
			_smoothing = smoothing;
		}

		public function get twoColorTint() : Boolean {
			return _twoColorTint;
		}

		public function set twoColorTint(tint : Boolean) : void {
			_twoColorTint = tint;
		}
	}
}
