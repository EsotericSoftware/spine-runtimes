/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {

public class Bone {
	static public var yDown:Boolean;

	internal var _data:BoneData;
	internal var _skeleton:Skeleton;
	internal var _parent:Bone;
	public var x:Number;
	public var y:Number;
	public var rotation:Number;
	public var rotationIK:Number;
	public var scaleX:Number
	public var scaleY:Number;
	public var flipX:Boolean;
	public var flipY:Boolean;

	internal var _m00:Number;
	internal var _m01:Number;
	internal var _m10:Number;
	internal var _m11:Number;
	internal var _worldX:Number;
	internal var _worldY:Number;
	internal var _worldRotation:Number;
	internal var _worldScaleX:Number;
	internal var _worldScaleY:Number;
	internal var _worldFlipX:Boolean;
	internal var _worldFlipY:Boolean;

	/** @param parent May be null. */
	public function Bone (data:BoneData, skeleton:Skeleton, parent:Bone) {
		if (data == null) throw new ArgumentError("data cannot be null.");
		if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");
		_data = data;
		_skeleton = skeleton;
		_parent = parent;
		setToSetupPose();
	}

	/** Computes the world SRT using the parent bone and the local SRT. */
	public function updateWorldTransform () : void {
		var parent:Bone = _parent;
		if (parent) {
			_worldX = x * parent._m00 + y * parent._m01 + parent._worldX;
			_worldY = x * parent._m10 + y * parent._m11 + parent._worldY;
			if (_data.inheritScale) {
				_worldScaleX = parent._worldScaleX * scaleX;
				_worldScaleY = parent._worldScaleY * scaleY;
			} else {
				_worldScaleX = scaleX;
				_worldScaleY = scaleY;
			}
			_worldRotation = _data.inheritRotation ? parent._worldRotation + rotationIK : rotationIK;
			_worldFlipX = parent._worldFlipX != flipX;
			_worldFlipY = parent._worldFlipY != flipY;
		} else {
			var skeletonFlipX:Boolean = _skeleton.flipX, skeletonFlipY:Boolean = _skeleton.flipY;
			_worldX = skeletonFlipX ? -x : x;
			_worldY = skeletonFlipY != yDown ? -y : y;
			_worldScaleX = scaleX;
			_worldScaleY = scaleY;
			_worldRotation = rotationIK;
			_worldFlipX = skeletonFlipX != flipX;
			_worldFlipY = skeletonFlipY != flipY;
		}
		var radians:Number = _worldRotation * (Math.PI / 180);
		var cos:Number = Math.cos(radians);
		var sin:Number = Math.sin(radians);
		if (_worldFlipX) {
			_m00 = -cos * _worldScaleX;
			_m01 = sin * _worldScaleY;
		} else {
			_m00 = cos * _worldScaleX;
			_m01 = -sin * _worldScaleY;
		}
		if (_worldFlipY != yDown) {
			_m10 = -sin * _worldScaleX;
			_m11 = -cos * _worldScaleY;
		} else {
			_m10 = sin * _worldScaleX;
			_m11 = cos * _worldScaleY;
		}
	}

	public function setToSetupPose () : void {
		x = _data.x;
		y = _data.y;
		rotation = _data.rotation;
		rotationIK = rotation;
		scaleX = _data.scaleX;
		scaleY = _data.scaleY;
		flipX = _data.flipX;
		flipY = _data.flipY;
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
	
	public function get worldFlipX () : Boolean {
		return _worldFlipX;
	}
	
	public function get worldFlipY () : Boolean {
		return _worldFlipY;
	}

	public function worldToLocal (world:Vector.<Number>) : void {
		var dx:Number = world[0] - _worldX, dy:Number = world[1] - _worldY;
		var m00:Number = _m00, m10:Number = _m10, m01:Number = _m01, m11:Number = _m11;
		if (_worldFlipX != (_worldFlipY != yDown)) {
			m00 = -m00;
			m11 = -m11;
		}
		var invDet:Number = 1 / (m00 * m11 - m01 * m10);
		world[0] = (dx * m00 * invDet - dy * m01 * invDet);
		world[1] = (dy * m11 * invDet - dx * m10 * invDet);
	}

	public function localToWorld (local:Vector.<Number>) : void {
		var localX:Number = local[0], localY:Number = local[1];
		local[0] = localX * _m00 + localY * _m01 + _worldX;
		local[1] = localX * _m10 + localY * _m11 + _worldY;
	}

	public function toString () : String {
		return _data._name;
	}
}

}
