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

package spine {

public class Bone implements Updatable {
	static public var yDown:Boolean;

	internal var _data:BoneData;
	internal var _skeleton:Skeleton;
	internal var _parent:Bone;
	public var x:Number;
	public var y:Number;
	public var rotation:Number;
	public var scaleX:Number;
	public var scaleY:Number;
	public var appliedRotation:Number;
	public var appliedScaleX:Number;
	public var appliedScaleY:Number;

	internal var _a:Number;
	internal var _b:Number;
	internal var _c:Number;
	internal var _d:Number;
	internal var _worldX:Number;
	internal var _worldY:Number;
	internal var _worldSignX:Number;
	internal var _worldSignY:Number;

	/** @param parent May be null. */
	public function Bone (data:BoneData, skeleton:Skeleton, parent:Bone) {
		if (data == null) throw new ArgumentError("data cannot be null.");
		if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");
		_data = data;
		_skeleton = skeleton;
		_parent = parent;
		setToSetupPose();
	}

	/** Computes the world SRT using the parent bone and this bone's local SRT. */
	public function updateWorldTransform () : void {
		updateWorldTransformWith(x, y, rotation, scaleX, scaleY);
	}

	/** Same as updateWorldTransform(). This method exists for Bone to implement Updatable. */
	public function update () : void {
		updateWorldTransformWith(x, y, rotation, scaleX, scaleY);
	}

	/** Computes the world SRT using the parent bone and the specified local SRT. */
	public function updateWorldTransformWith (x:Number, y:Number, rotation:Number, scaleX:Number, scaleY:Number) : void {
		appliedRotation = rotation;
		appliedScaleX = scaleX;
		appliedScaleY = scaleY;

		var radians:Number = rotation * MathUtils.degRad;
		var cos:Number = Math.cos(radians), sin:Number = Math.sin(radians);
		var la:Number = cos * scaleX, lb:Number = -sin * scaleY, lc:Number = sin * scaleX, ld:Number = cos * scaleY;
		var parent:Bone = _parent;
		if (!parent) { // Root bone.
			var skeleton:Skeleton = this.skeleton;
			if (skeleton.flipX) {
				x = -x;
				la = -la;
				lb = -lb;
			}
			if (skeleton.flipY != yDown) {
				y = -y;
				lc = -lc;
				ld = -ld;
			}
			_a = la;
			_b = lb;
			_c = lc;
			_d = ld;
			_worldX = x;
			_worldY = y;
			_worldSignX = scaleX < 0 ? -1 : 1;
			_worldSignY = scaleY < 0 ? -1 : 1;
			return;
		}

		var pa:Number = parent._a, pb:Number = parent._b, pc:Number = parent._c, pd:Number = parent._d;
		_worldX = pa * x + pb * y + parent._worldX;
		_worldY = pc * x + pd * y + parent._worldY;
		_worldSignX = parent._worldSignX * (scaleX < 0 ? -1 : 1);
		_worldSignY = parent._worldSignY * (scaleY < 0 ? -1 : 1);

		if (data.inheritRotation && data.inheritScale) {
			_a = pa * la + pb * lc;
			_b = pa * lb + pb * ld;
			_c = pc * la + pd * lc;
			_d = pc * lb + pd * ld;
		} else if (data.inheritRotation) { // No scale inheritance.
			pa = 1;
			pb = 0;
			pc = 0;
			pd = 1;
			while (parent != null) {
				radians = parent.appliedRotation * MathUtils.degRad;
				cos = Math.cos(radians);
				sin = Math.sin(radians);
				var temp1:Number = pa * cos + pb * sin;
				pb = pa * -sin + pb * cos;
				pa = temp1;
				temp1 = pc * cos + pd * sin;
				pd = pc * -sin + pd * cos;
				pc = temp1;
				parent = parent.parent;
			}
			_a = pa * la + pb * lc;
			_b = pa * lb + pb * ld;
			_c = pc * la + pd * lc;
			_d = pc * lb + pd * ld;
			if (skeleton.flipX) {
				_a = -_a;
				_b = -_b;
			}
			if (skeleton.flipY != yDown) {
				_c = -_c;
				_d = -_d;
			}
		} else if (data.inheritScale) { // No rotation inheritance.
			pa = 1;
			pb = 0;
			pc = 0;
			pd = 1;
			while (parent) {
				radians = parent.rotation * MathUtils.degRad;
				cos = Math.cos(radians);
				sin = Math.sin(radians);
				var psx:Number = parent.appliedScaleX, psy:Number = parent.appliedScaleY;
				var za:Number = cos * psx, zb:Number = -sin * psy, zc:Number = sin * psx, zd:Number = cos * psy;
				var temp2:Number = pa * za + pb * zc;
				pb = pa * zb + pb * zd;
				pa = temp2;
				temp2 = pc * za + pd * zc;
				pd = pc * zb + pd * zd;
				pc = temp2;

				if (psx < 0) radians = -radians;
				cos = Math.cos(-radians);
				sin = Math.sin(-radians);
				temp2 = pa * cos + pb * sin;
				pb = pa * -sin + pb * cos;
				pa = temp2;
				temp2 = pc * cos + pd * sin;
				pd = pc * -sin + pd * cos;
				pc = temp2;

				parent = parent.parent;
			}
			_a = pa * la + pb * lc;
			_b = pa * lb + pb * ld;
			_c = pc * la + pd * lc;
			_d = pc * lb + pd * ld;
			if (skeleton.flipX) {
				_a = -_a;
				_b = -_b;
			}
			if (skeleton.flipY != yDown) {
				_c = -_c;
				_d = -_d;
			}
		} else {
			_a = la;
			_b = lb;
			_c = lc;
			_d = ld;
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
	
	public function get skeleton () : Skeleton {
		return _skeleton;
	}

	public function get a () : Number {
		return _a;
	}

	public function get b () : Number {
		return _b;
	}

	public function get c () : Number {
		return _c;
	}

	public function get d () : Number {
		return _d;
	}

	public function get worldX () : Number {
		return _worldX;
	}

	public function get worldY () : Number {
		return _worldY;
	}

	public function get worldSignX () : Number {
		return _worldSignX;
	}

	public function get worldSignY () : Number {
		return _worldSignY;
	}

	public function get worldRotationX () : Number {
		return Math.atan2(_c, _a) * MathUtils.radDeg;
	}

	public function get worldRotationY () : Number {
		return Math.atan2(_d, _b) * MathUtils.radDeg;
	}

	public function get worldScaleX () : Number {
		return Math.sqrt(_a * _a + _b * _b) * _worldSignX;
	}

	public function get worldScaleY () : Number {
		return Math.sqrt(_c * _c + _d * _d) * _worldSignY;
	}

	public function worldToLocal (world:Vector.<Number>) : void {
		var x:Number = world[0] - _worldX, y:Number = world[1] - _worldY;
		var a:Number = _a, b:Number = _b, c:Number = _c, d:Number = _d;
		var invDet:Number = 1 / (a * d - b * c);
		world[0] = (x * a * invDet - y * b * invDet);
		world[1] = (y * d * invDet - x * c * invDet);
	}

	public function localToWorld (local:Vector.<Number>) : void {
		var localX:Number = local[0], localY:Number = local[1];
		local[0] = localX * _a + localY * _b + _worldX;
		local[1] = localX * _c + localY * _d + _worldY;
	}

	public function toString () : String {
		return _data._name;
	}
}

}
