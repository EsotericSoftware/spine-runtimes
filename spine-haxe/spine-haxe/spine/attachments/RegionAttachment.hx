package spine.attachments;

import openfl.Vector;
import spine.Bone;
import spine.Color;

class RegionAttachment extends Attachment {
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
	public var regionOffsetX:Float = 0; // Pixels stripped from the bottom left, unrotated.
	public var regionOffsetY:Float = 0;
	public var regionWidth:Float = 0; // Unrotated, stripped size.
	public var regionHeight:Float = 0;
	public var regionOriginalWidth:Float = 0; // Unrotated, unstripped size.
	public var regionOriginalHeight:Float = 0;

	private var offsets:Vector<Float> = new Vector<Float>(8, true);

	public var uvs:Vector<Float> = new Vector<Float>(8, true);

	public function new(name:String) {
		super(name);
	}

	public function updateOffset():Void {
		var regionScaleX:Float = width / regionOriginalWidth * scaleX;
		var regionScaleY:Float = height / regionOriginalHeight * scaleY;
		var localX:Float = -width * 0.5 * scaleX + regionOffsetX * regionScaleX;
		var localY:Float = -height * 0.5 * scaleY + regionOffsetY * regionScaleY;
		var localX2:Float = localX + regionWidth * regionScaleX;
		var localY2:Float = localY + regionHeight * regionScaleY;

		var radians:Float = rotation * MathUtils.degRad;
		var cos:Float = Math.cos(radians);
		var sin:Float = Math.sin(radians);
		var localXCos:Float = localX * cos + x;
		var localXSin:Float = localX * sin;
		var localYCos:Float = localY * cos + y;
		var localYSin:Float = localY * sin;
		var localX2Cos:Float = localX2 * cos + x;
		var localX2Sin:Float = localX2 * sin;
		var localY2Cos:Float = localY2 * cos + y;
		var localY2Sin:Float = localY2 * sin;

		offsets[BLX] = localXCos - localYSin;
		offsets[BLY] = localYCos + localXSin;
		offsets[ULX] = localXCos - localY2Sin;
		offsets[ULY] = localY2Cos + localXSin;
		offsets[URX] = localX2Cos - localY2Sin;
		offsets[URY] = localY2Cos + localX2Sin;
		offsets[BRX] = localX2Cos - localYSin;
		offsets[BRY] = localYCos + localX2Sin;
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

	public function computeWorldVertices(bone:Bone, worldVertices:Vector<Float>, offset:Int, stride:Int):Void {
		var x:Float = bone.worldX, y:Float = bone.worldY;
		var a:Float = bone.a,
			b:Float = bone.b,
			c:Float = bone.c,
			d:Float = bone.d;
		var offsetX:Float = 0, offsetY:Float = 0;

		offsetX = offsets[BRX];
		offsetY = offsets[BRY];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // br
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = offsets[BLX];
		offsetY = offsets[BLY];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // bl
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = offsets[ULX];
		offsetY = offsets[ULY];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ul
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = offsets[URX];
		offsetY = offsets[URY];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ur
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
	}

	override public function copy():Attachment {
		var copy:RegionAttachment = new RegionAttachment(name);
		copy.regionWidth = regionWidth;
		copy.regionHeight = regionHeight;
		copy.regionOffsetX = regionOffsetX;
		copy.regionOffsetY = regionOffsetY;
		copy.regionOriginalWidth = regionOriginalWidth;
		copy.regionOriginalHeight = regionOriginalHeight;
		copy.rendererObject = rendererObject;
		copy.path = path;
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
		return copy;
	}
}
