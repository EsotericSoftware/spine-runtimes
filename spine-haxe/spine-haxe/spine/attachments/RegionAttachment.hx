package spine.attachments;

import openfl.Vector;
import spine.Bone;
import spine.Color;

class RegionAttachment extends Attachment implements HasTextureRegion {
	public static inline var BLX:Int = 0;
	public static inline var BLY:Int = 1;
	public static inline var ULX:Int = 2;
	public static inline var ULY:Int = 3;
	public static inline var URX:Int = 4;
	public static inline var URY:Int = 5;
	public static inline var BRX:Int = 6;
	public static inline var BRY:Int = 7;

	public var x:Float = 0;
	public var y:Float = 0;
	public var scaleX:Float = 1;
	public var scaleY:Float = 1;
	public var rotation:Float = 0;
	public var width:Float = 0;
	public var height:Float = 0;
	public var color:Color = new Color(1, 1, 1, 1);
	public var path:String;
	public var rendererObject:Dynamic;
	public var region:TextureRegion;
	public var sequence:Sequence;

	private var offsets:Vector<Float> = new Vector<Float>(8, true);

	public var uvs:Vector<Float> = new Vector<Float>(8, true);

	public function new(name:String, path:String) {
		super(name);
		this.path = path;
	}

	public function updateRegion():Void {
		if (region == null) {
			trace("Region not set.");
			uvs[0] = 0;
			uvs[1] = 0;
			uvs[2] = 0;
			uvs[3] = 1;
			uvs[4] = 1;
			uvs[5] = 1;
			uvs[6] = 1;
			uvs[7] = 0;
			return;
		}

		var regionScaleX = width / region.originalWidth * scaleX;
		var regionScaleY = height / region.originalHeight * scaleY;
		var localX = -width / 2 * scaleX + region.offsetX * regionScaleX;
		var localY = -height / 2 * scaleY + region.offsetY * regionScaleY;
		var localX2 = localX + region.width * regionScaleX;
		var localY2 = localY + region.height * regionScaleY;
		var radians = rotation * Math.PI / 180;
		var cos = Math.cos(radians);
		var sin = Math.sin(radians);
		var x = this.x, y = this.y;
		var localXCos = localX * cos + x;
		var localXSin = localX * sin;
		var localYCos = localY * cos + y;
		var localYSin = localY * sin;
		var localX2Cos = localX2 * cos + x;
		var localX2Sin = localX2 * sin;
		var localY2Cos = localY2 * cos + y;
		var localY2Sin = localY2 * sin;

		offsets[0] = localXCos - localYSin;
		offsets[1] = localYCos + localXSin;
		offsets[2] = localXCos - localY2Sin;
		offsets[3] = localY2Cos + localXSin;
		offsets[4] = localX2Cos - localY2Sin;
		offsets[5] = localY2Cos + localX2Sin;
		offsets[6] = localX2Cos - localYSin;
		offsets[7] = localYCos + localX2Sin;

		if (region.degrees == 90) {
			uvs[0] = region.u2;
			uvs[1] = region.v2;
			uvs[2] = region.u;
			uvs[3] = region.v2;
			uvs[4] = region.u;
			uvs[5] = region.v;
			uvs[6] = region.u2;
			uvs[7] = region.v;
		} else {
			uvs[0] = region.u;
			uvs[1] = region.v2;
			uvs[2] = region.u;
			uvs[3] = region.v;
			uvs[4] = region.u2;
			uvs[5] = region.v;
			uvs[6] = region.u2;
			uvs[7] = region.v2;
		}
	}

	public function setUVs(u:Float, v:Float, u2:Float, v2:Float, degrees:Int):Void {
		if (degrees == 90) {
			uvs[4] = u;
			uvs[5] = v2;
			uvs[6] = u;
			uvs[7] = v;
			uvs[0] = u2;
			uvs[1] = v;
			uvs[2] = u2;
			uvs[3] = v2;
		} else {
			uvs[2] = u;
			uvs[3] = v2;
			uvs[4] = u;
			uvs[5] = v;
			uvs[6] = u2;
			uvs[7] = v;
			uvs[0] = u2;
			uvs[1] = v2;
		}
	}

	public function computeWorldVertices(slot:Slot, worldVertices:Vector<Float>, offset:Int, stride:Int):Void {
		if (sequence != null)
			sequence.apply(slot, this);

		var bone = slot.bone;
		var vertexOffset = this.offsets;
		var x = bone.worldX, y = bone.worldY;
		var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
		var offsetX:Float = 0, offsetY:Float = 0;

		offsetX = vertexOffset[0];
		offsetY = vertexOffset[1];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // br
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffset[2];
		offsetY = vertexOffset[3];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // bl
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffset[4];
		offsetY = vertexOffset[5];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ul
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffset[6];
		offsetY = vertexOffset[7];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ur
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
	}

	override public function copy():Attachment {
		var copy:RegionAttachment = new RegionAttachment(name, path);
		copy.region = region;
		copy.rendererObject = rendererObject;
		copy.x = x;
		copy.y = y;
		copy.scaleX = scaleX;
		copy.scaleY = scaleY;
		copy.rotation = rotation;
		copy.width = width;
		copy.height = height;
		copy.uvs = uvs.concat();
		copy.offsets = offsets.concat();
		copy.color.setFromColor(color);
		copy.sequence = sequence != null ? sequence.copy() : null;
		return copy;
	}
}
