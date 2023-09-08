package spine.attachments;

import openfl.Vector;
import spine.Bone;
import spine.Color;
import spine.MathUtils;

class PointAttachment extends VertexAttachment {
	public var x:Float = 0;
	public var y:Float = 0;
	public var rotation:Float = 0;
	public var color:Color = new Color(0.38, 0.94, 0, 1);

	public function new(name:String) {
		super(name);
	}

	public function computeWorldPosition(bone:Bone, point:Vector<Float>):Vector<Float> {
		point[0] = x * bone.a + y * bone.b + bone.worldX;
		point[1] = x * bone.c + y * bone.d + bone.worldY;
		return point;
	}

	public function computeWorldRotation(bone:Bone):Float {
		var cos:Float = MathUtils.cosDeg(this.rotation),
			sin:Float = MathUtils.sinDeg(this.rotation);
		var x:Float = cos * bone.a + sin * bone.b;
		var y:Float = cos * bone.c + sin * bone.d;
		return Math.atan2(y, x) * MathUtils.radDeg;
	}

	override public function copy():Attachment {
		var copy:PointAttachment = new PointAttachment(name);
		copy.x = x;
		copy.y = y;
		copy.rotation = rotation;
		copy.color.setFromColor(color);
		return copy;
	}
}
