package spine.attachments {
import spine.Bone;

public class RegionAttachment extends Attachment {
	public const X1:int = 0;
	public const Y1:int = 1;
	public const X2:int = 2;
	public const Y2:int = 3;
	public const X3:int = 4;
	public const Y3:int = 5;
	public const X4:int = 6;
	public const Y4:int = 7;

	public var x:Number;
	public var y:Number;
	public var scaleX:Number = 1;
	public var scaleY:Number = 1;
	public var rotation:Number;
	public var width:Number;
	public var height:Number;

	public var texture:Object;
	public var regionOffsetX:Number; // Pixels stripped from the bottom left, unrotated.
	public var regionOffsetY:Number;
	public var regionWidth:Number; // Unrotated, stripped size.
	public var regionHeight:Number;
	public var regionOriginalWidth:Number; // Unrotated, unstripped size.
	public var regionOriginalHeight:Number;

	public var vertices:Vector.<Number> = new Vector.<Number>();
	public var offset:Vector.<Number> = new Vector.<Number>();
	public var uvs:Vector.<Number> = new Vector.<Number>();

	public function RegionAttachment (name:String) {
		super(name);
	}

	public function setUVs (u:Number, v:Number, u2:Number, v2:Number, rotate:Boolean) : void {
		if (rotate) {
			uvs[X2] = u;
			uvs[Y2] = v2;
			uvs[X3] = u;
			uvs[Y3] = v;
			uvs[X4] = u2;
			uvs[Y4] = v;
			uvs[X1] = u2;
			uvs[Y1] = v2;
		} else {
			uvs[X1] = u;
			uvs[Y1] = v2;
			uvs[X2] = u;
			uvs[Y2] = v;
			uvs[X3] = u2;
			uvs[Y3] = v;
			uvs[X4] = u2;
			uvs[Y4] = v2;
		}
	}

	public function updateOffset () : void {
		var regionScaleX:Number = width / regionOriginalWidth * scaleX;
		var regionScaleY:Number = height / regionOriginalHeight * scaleY;
		var localX:Number = -width / 2 * scaleX + regionOffsetX * regionScaleX;
		var localY:Number = -height / 2 * scaleY + regionOffsetY * regionScaleY;
		var localX2:Number = localX + regionWidth * regionScaleX;
		var localY2:Number = localY + regionHeight * regionScaleY;
		var radians:Number = rotation * Math.PI / 180;
		var cos:Number = Math.cos(radians);
		var sin:Number = Math.sin(radians);
		var localXCos:Number = localX * cos + x;
		var localXSin:Number = localX * sin;
		var localYCos:Number = localY * cos + y;
		var localYSin:Number = localY * sin;
		var localX2Cos:Number = localX2 * cos + x;
		var localX2Sin:Number = localX2 * sin;
		var localY2Cos:Number = localY2 * cos + y;
		var localY2Sin:Number = localY2 * sin;
		offset[X1] = localXCos - localYSin;
		offset[Y1] = localYCos + localXSin;
		offset[X2] = localXCos - localY2Sin;
		offset[Y2] = localY2Cos + localXSin;
		offset[X3] = localX2Cos - localY2Sin;
		offset[Y3] = localY2Cos + localX2Sin;
		offset[X4] = localX2Cos - localYSin;
		offset[Y4] = localYCos + localX2Sin;
	}

	public function updateVertices (bone:Bone) : void {
		var x:Number = bone.worldX;
		var y:Number = bone.worldY;
		var m00:Number = bone.m00;
		var m01:Number = bone.m01;
		var m10:Number = bone.m10;
		var m11:Number = bone.m11;
		vertices[X1] = offset[X1] * m00 + offset[Y1] * m01 + x;
		vertices[Y1] = offset[X1] * m10 + offset[Y1] * m11 + y;
		vertices[X2] = offset[X2] * m00 + offset[Y2] * m01 + x;
		vertices[Y2] = offset[X2] * m10 + offset[Y2] * m11 + y;
		vertices[X3] = offset[X3] * m00 + offset[Y3] * m01 + x;
		vertices[Y3] = offset[X3] * m10 + offset[Y3] * m11 + y;
		vertices[X4] = offset[X4] * m00 + offset[Y4] * m01 + x;
		vertices[Y4] = offset[X4] * m10 + offset[Y4] * m11 + y;
	}
}

}
