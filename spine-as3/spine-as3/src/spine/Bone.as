package spine {

public class Bone {
	static public var yDown:Boolean;

	internal var _data:BoneData;
	internal var _parent:Bone;
	public var x:Number;
	public var y:Number;
	public var rotation:Number;
	public var scaleX:Number
	public var scaleY:Number;

	internal var _m00:Number;
	internal var _m01:Number;
	internal var _m10:Number;
	internal var _m11:Number;
	internal var _worldX:Number;
	internal var _worldY:Number;
	internal var _worldRotation:Number;
	internal var _worldScaleX:Number;
	internal var _worldScaleY:Number;

	/** @param parent May be null. */
	public function Bone (data:BoneData, parent:Bone) {
		if (data == null)
			throw new ArgumentError("data cannot be null.");
		_data = data;
		_parent = parent;
		setToSetupPose();
	}

	/** Computes the world SRT using the parent bone and the local SRT. */
	public function updateWorldTransform (flipX:Boolean, flipY:Boolean) : void {
		if (_parent != null) {
			_worldX = x * _parent._m00 + y * _parent._m01 + _parent._worldX;
			_worldY = x * _parent._m10 + y * _parent._m11 + _parent._worldY;
			_worldScaleX = _parent._worldScaleX * scaleX;
			_worldScaleY = _parent._worldScaleY * scaleY;
			_worldRotation = _parent._worldRotation + rotation;
		} else {
			_worldX = flipX ? -x : x;
			_worldY = flipY ? -y : y;
			_worldScaleX = scaleX;
			_worldScaleY = scaleY;
			_worldRotation = rotation;
		}
		var radians:Number = _worldRotation * Math.PI / 180;
		var cos:Number = Math.cos(radians);
		var sin:Number = Math.sin(radians);
		_m00 = cos * _worldScaleX;
		_m10 = sin * _worldScaleX;
		_m01 = -sin * _worldScaleY;
		_m11 = cos * _worldScaleY;
		if (flipX) {
			_m00 = -_m00;
			_m01 = -_m01;
		}
		if (flipY != yDown) {
			_m10 = -_m10;
			_m11 = -_m11;
		}
	}

	public function setToSetupPose () : void {
		x = _data.x;
		y = _data.y;
		rotation = _data.rotation;
		scaleX = _data.scaleX;
		scaleY = _data.scaleY;
	}

	public function get data () : BoneData {
		return _data;
	}

	public function get parent () : Bone {
		return _parent;
	}

	public function get m00 () : Number {
		return _m00;
	}

	public function get m01 () : Number {
		return _m01;
	}

	public function get m10 () : Number {
		return _m10;
	}

	public function get m11 () : Number {
		return _m11;
	}

	public function get worldX () : Number {
		return _worldX;
	}

	public function get worldY () : Number {
		return _worldY;
	}

	public function get worldRotation () : Number {
		return _worldRotation;
	}

	public function get worldScaleX () : Number {
		return _worldScaleX;
	}

	public function get worldScaleY () : Number {
		return _worldScaleY;
	}

	public function toString () : String {
		return _data._name;
	}
}

}
