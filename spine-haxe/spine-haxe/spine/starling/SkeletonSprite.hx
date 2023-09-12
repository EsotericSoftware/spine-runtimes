package spine.starling;

import starling.utils.Max;
import openfl.geom.Matrix;
import openfl.geom.Point;
import openfl.geom.Rectangle;
import openfl.Vector;
import spine.atlas.TextureAtlasRegion;
import spine.attachments.Attachment;
import spine.attachments.ClippingAttachment;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;
import spine.Bone;
import spine.Skeleton;
import spine.SkeletonClipping;
import spine.SkeletonData;
import spine.Slot;
import starling.display.BlendMode;
import starling.display.DisplayObject;
import starling.display.Image;
import starling.rendering.IndexData;
import starling.rendering.Painter;
import starling.rendering.VertexData;
import starling.utils.Color;
import starling.utils.MatrixUtil;

class SkeletonSprite extends DisplayObject {
	static private var _tempPoint:Point = new Point();
	static private var _tempMatrix:Matrix = new Matrix();
	static private var _tempVertices:Vector<Float> = new Vector<Float>();
	static private var blendModes:Vector<String> = Vector.ofArray([BlendMode.NORMAL, BlendMode.ADD, BlendMode.MULTIPLY, BlendMode.SCREEN]);

	private var _skeleton:Skeleton;
	private var _smoothing:String = "bilinear";

	private static var clipper:SkeletonClipping = new SkeletonClipping();
	private static var QUAD_INDICES:Vector<Int> = Vector.ofArray([0, 1, 2, 2, 3, 0]);

	private var tempLight:spine.Color = new spine.Color(0, 0, 0);
	private var tempDark:spine.Color = new spine.Color(0, 0, 0);

	public function new(skeletonData:SkeletonData) {
		super();
		Bone.yDown = true;
		_skeleton = new Skeleton(skeletonData);
		_skeleton.updateWorldTransform();
	}

	override public function render(painter:Painter):Void {
		var clipper:SkeletonClipping = SkeletonSprite.clipper;
		painter.state.alpha *= skeleton.color.a;
		var originalBlendMode:String = painter.state.blendMode;
		var r:Float = skeleton.color.r * 255;
		var g:Float = skeleton.color.g * 255;
		var b:Float = skeleton.color.b * 255;
		var drawOrder:Vector<Slot> = skeleton.drawOrder;
		var attachmentColor:spine.Color;
		var rgb:Int;
		var a:Float;
		var dark:Int;
		var mesh:SkeletonMesh = null;
		var verticesLength:Int;
		var verticesCount:Int;
		var indicesLength:Int;
		var indexData:IndexData;
		var indices:Vector<Int> = null;
		var vertexData:VertexData;
		var uvs:Vector<Float>;

		for (slot in drawOrder) {
			if (!slot.bone.active) {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			var worldVertices:Vector<Float> = _tempVertices;
			if (Std.isOfType(slot.attachment, RegionAttachment)) {
				var region:RegionAttachment = cast(slot.attachment, RegionAttachment);
				verticesLength = 8;
				verticesCount = verticesLength >> 1;
				if (worldVertices.length < verticesLength)
					worldVertices.length = verticesLength;
				region.computeWorldVertices(slot, worldVertices, 0, 2);

				mesh = null;
				if (Std.isOfType(region.rendererObject, SkeletonMesh)) {
					mesh = cast(region.rendererObject, SkeletonMesh);
					indices = QUAD_INDICES;
				} else {
					if (Std.isOfType(region.rendererObject, Image)) {
						region.rendererObject = mesh = new SkeletonMesh(cast(region.rendererObject, Image).texture);
					} else if (Std.isOfType(region.rendererObject, TextureAtlasRegion)) {
						region.rendererObject = mesh = new SkeletonMesh(cast(region.rendererObject, TextureAtlasRegion).texture);
					}

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
					worldVertices.length = verticesLength;
				meshAttachment.computeWorldVertices(slot, 0, meshAttachment.worldVerticesLength, worldVertices, 0, 2);

				mesh = null;
				if (Std.isOfType(meshAttachment.rendererObject, SkeletonMesh)) {
					mesh = cast(meshAttachment.rendererObject, SkeletonMesh);
					indices = meshAttachment.triangles;
				} else {
					if (Std.isOfType(meshAttachment.rendererObject, Image)) {
						meshAttachment.rendererObject = mesh = new SkeletonMesh(cast(meshAttachment.rendererObject, Image).texture);
					} else if (Std.isOfType(meshAttachment.rendererObject, TextureAtlasRegion)) {
						meshAttachment.rendererObject = mesh = new SkeletonMesh(cast(meshAttachment.rendererObject, TextureAtlasRegion).texture);
					}

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
		var slots:Vector<Slot> = skeleton.slots;
		var worldVertices:Vector<Float> = _tempVertices;
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
					worldVertices.length = verticesLength;
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

	public var smoothing(get, set):String;

	private function get_smoothing():String {
		return _smoothing;
	}

	private function set_smoothing(smoothing:String):String {
		_smoothing = smoothing;
		return _smoothing;
	}
}
