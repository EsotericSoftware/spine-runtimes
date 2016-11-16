/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {

public class Bone implements Updatable {
	static public var yDown:Boolean;

	internal var _data:BoneData;
	internal var _skeleton:Skeleton;
	internal var _parent:Bone;
	internal var _children:Vector.<Bone> = new Vector.<Bone>();
	public var x:Number;
	public var y:Number;
	public var rotation:Number;
	public var scaleX:Number;
	public var scaleY:Number;
	public var shearX:Number;
	public var shearY:Number;
	public var ax:Number;
	public var ay:Number;
	public var arotation:Number;
	public var ascaleX:Number;
	public var ascaleY:Number;
	public var ashearX:Number;
	public var ashearY:Number;
	public var appliedValid:Boolean;	

	internal var _a:Number;
	internal var _b:Number;
	internal var _c:Number;
	internal var _d:Number;
	internal var _worldX:Number;
	internal var _worldY:Number;
	
	internal var _sorted:Boolean;

	/** @param parent May be null. */
	public function Bone (data:BoneData, skeleton:Skeleton, parent:Bone) {
		if (data == null) throw new ArgumentError("data cannot be null.");
		if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");
		_data = data;
		_skeleton = skeleton;
		_parent = parent;
		setToSetupPose();
	}
	
	/** Same as updateWorldTransform(). This method exists for Bone to implement Updatable. */
	public function update () : void {
		updateWorldTransformWith(x, y, rotation, scaleX, scaleY, shearX, shearY);
	}

	/** Computes the world SRT using the parent bone and this bone's local SRT. */
	public function updateWorldTransform () : void {
		updateWorldTransformWith(x, y, rotation, scaleX, scaleY, shearX, shearY);
	}

	/** Computes the world SRT using the parent bone and the specified local SRT. */
	public function updateWorldTransformWith (x:Number, y:Number, rotation:Number, scaleX:Number, scaleY:Number, shearX:Number, shearY:Number) : void {
		ax = x;
		ay = y;
		arotation = rotation;
		ascaleX = scaleX;
		ascaleY = scaleY;
		ashearX = shearX;
		ashearY = shearY;
		appliedValid = true;
		
		var rotationY:Number = 0, la:Number = 0, lb:Number = 0, lc:Number = 0, ld:Number = 0;
		var sin:Number = 0, cos:Number = 0;
		var s:Number = 0;
		
		var parent:Bone = _parent;
		if (!parent) { // Root bone.
			rotationY = rotation + 90 + shearY;
			la = MathUtils.cosDeg(rotation + shearX) * scaleX;
			lb = MathUtils.cosDeg(rotationY) * scaleY;
			lc = MathUtils.sinDeg(rotation + shearX) * scaleX;
			ld = MathUtils.sinDeg(rotationY) * scaleY;
			var skeleton:Skeleton = _skeleton;
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
			_worldX = x + skeleton.x;
			_worldY = y + skeleton.y;	
			return;
		}

		var pa:Number = parent._a, pb:Number = parent._b, pc:Number = parent._c, pd:Number = parent._d;
		_worldX = pa * x + pb * y + parent._worldX;
		_worldY = pc * x + pd * y + parent._worldY;		

		switch (_data.transformMode) {
		case TransformMode.normal: {
			rotationY = rotation + 90 + shearY;
			la = MathUtils.cosDeg(rotation + shearX) * scaleX;
			lb = MathUtils.cosDeg(rotationY) * scaleY;
			lc = MathUtils.sinDeg(rotation + shearX) * scaleX;
			ld = MathUtils.sinDeg(rotationY) * scaleY;
			_a = pa * la + pb * lc;
			_b = pa * lb + pb * ld;
			_c = pc * la + pd * lc;
			_d = pc * lb + pd * ld;
			return;
		}
		case TransformMode.onlyTranslation: {
			rotationY = rotation + 90 + shearY;
			_a = MathUtils.cosDeg(rotation + shearX) * scaleX;
			_b = MathUtils.cosDeg(rotationY) * scaleY;
			_c = MathUtils.sinDeg(rotation + shearX) * scaleX;
			_d = MathUtils.sinDeg(rotationY) * scaleY;
			break;
		}
		case TransformMode.noRotationOrReflection: {
			s = pa * pa + pc * pc;
			var prx:Number = 0;
			if (s > 0.0001) {
				s = Math.abs(pa * pd - pb * pc) / s;
				pb = pc * s;
				pd = pa * s;
				prx = Math.atan2(pc, pa) * MathUtils.radDeg;
			} else {
				pa = 0;
				pc = 0;
				prx = 90 - Math.atan2(pd, pb) * MathUtils.radDeg;
			}
			var rx:Number = rotation + shearX - prx;
			var ry:Number = rotation + shearY - prx + 90;
			la = MathUtils.cosDeg(rx) * scaleX;
			lb = MathUtils.cosDeg(ry) * scaleY;
			lc = MathUtils.sinDeg(rx) * scaleX;
			ld = MathUtils.sinDeg(ry) * scaleY;
			_a = pa * la - pb * lc;
			_b = pa * lb - pb * ld;
			_c = pc * la + pd * lc;
			_d = pc * lb + pd * ld;
			break;
		}
		case TransformMode.noScale:
		case TransformMode.noScaleOrReflection: {
			cos = MathUtils.cosDeg(rotation);
			sin = MathUtils.sinDeg(rotation);
			var za:Number = pa * cos + pb * sin;
			var zc:Number = pc * cos + pd * sin;
			s = Math.sqrt(za * za + zc * zc);
			if (s > 0.00001) s = 1 / s;
			za *= s;
			zc *= s;
			s = Math.sqrt(za * za + zc * zc);
			var r:Number = Math.PI / 2 + Math.atan2(zc, za);
			var zb:Number = Math.cos(r) * s;
			var zd:Number = Math.sin(r) * s;
			la = MathUtils.cosDeg(shearX) * scaleX;
			lb = MathUtils.cosDeg(90 + shearY) * scaleY;
			lc = MathUtils.sinDeg(shearX) * scaleX;
			ld = MathUtils.sinDeg(90 + shearY) * scaleY;
			_a = za * la + zb * lc;
			_b = za * lb + zb * ld;
			_c = zc * la + zd * lc;
			_d = zc * lb + zd * ld;
			if (_data.transformMode != TransformMode.noScaleOrReflection ? pa * pd - pb * pc < 0 : skeleton.flipX != skeleton.flipY) {
				_b = -_b;
				_d = -_d;
			}
			return;
		}
		}
		if (_skeleton.flipX) {
			_a = -_a;
			_b = -_b;
		}
		if (_skeleton.flipY != yDown) {
			_c = -_c;
			_d = -_d;
		}
	}

	public function setToSetupPose () : void {
		x = _data.x;
		y = _data.y;
		rotation = _data.rotation;
		scaleX = _data.scaleX;
		scaleY = _data.scaleY;
		shearX = _data.shearX;
		shearY = _data.shearY;
	}

	public function get data () : BoneData {
		return _data;
	}
	
	public function get skeleton () : Skeleton {
		return _skeleton;
	}
	
	public function get parent () : Bone {
		return _parent;
	}
	
	public function get children () : Vector.<Bone> {;
		return _children;
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

	public function get worldRotationX () : Number {
		return Math.atan2(_c, _a) * MathUtils.radDeg;
	}

	public function get worldRotationY () : Number {
		return Math.atan2(_d, _b) * MathUtils.radDeg;
	}

	public function get worldScaleX () : Number {
		return Math.sqrt(_a * _a + _c * _c);
	}

	public function get worldScaleY () : Number {
		return Math.sqrt(_b * _b + _d * _d);
	}
	
	public function worldToLocalRotationX () : Number {
		var parent:Bone = _parent;
		if (parent == null) return arotation;
		var pa:Number = parent.a, pb:Number = parent.b, pc:Number = parent.c, pd:Number = parent.d, a:Number = this.a, c:Number = this.c;
		return Math.atan2(pa * c - pc * a, pd * a - pb * c) * MathUtils.radDeg;
	}

	public function worldToLocalRotationY () : Number {
		var parent:Bone = _parent;
		if (parent == null) return arotation;
		var pa:Number = parent.a, pb:Number = parent.b, pc:Number = parent.c, pd:Number = parent.d, b:Number = this.b, d:Number = this.d;
		return Math.atan2(pa * d - pc * b, pd * b - pb * d) * MathUtils.radDeg;
	}

	public function rotateWorld (degrees:Number) : void {
		var a:Number = this.a, b:Number = this.b, c:Number = this.c, d:Number = this.d;
		var cos:Number = MathUtils.cosDeg(degrees), sin:Number = MathUtils.sinDeg(degrees);
		this._a = cos * a - sin * c;
		this._b = cos * b - sin * d;
		this._c = sin * a + cos * c;
		this._d = sin * b + cos * d;
		this.appliedValid = false;
	}

	/** Computes the individual applied transform values from the world transform. This can be useful to perform processing using
	 * the applied transform after the world transform has been modified directly (eg, by a constraint).
	 * <p>
	 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. */
	internal function updateAppliedTransform () : void {
		appliedValid = true;
		var parent:Bone = this.parent;
		if (parent == null) {
			ax = worldX;
			ay = worldY;
			arotation = Math.atan2(c, a) * MathUtils.radDeg;
			ascaleX = Math.sqrt(a * a + c * c);
			ascaleY = Math.sqrt(b * b + d * d);			
			ashearX = 0;
			ashearY = Math.atan2(a * b + c * d, a * d - b * c) * MathUtils.radDeg;
			return;
		}
		var pa:Number = parent.a, pb:Number = parent.b, pc:Number = parent.c, pd:Number = parent.d;
		var pid:Number = 1 / (pa * pd - pb * pc);
		var dx:Number = worldX - parent.worldX, dy:Number = worldY - parent.worldY;
		ax = (dx * pd * pid - dy * pb * pid);
		ay = (dy * pa * pid - dx * pc * pid);
		var ia:Number = pid * pd;
		var id:Number = pid * pa;
		var ib:Number = pid * pb;
		var ic:Number = pid * pc;
		var ra:Number = ia * a - ib * c;
		var rb:Number = ia * b - ib * d;
		var rc:Number = id * c - ic * a;
		var rd:Number = id * d - ic * b;
		ashearX = 0;
		ascaleX = Math.sqrt(ra * ra + rc * rc);
		if (scaleX > 0.0001) {
			var det:Number = ra * rd - rb * rc;
			ascaleY = det /ascaleX;
			ashearY = Math.atan2(ra * rb + rc * rd, det) * MathUtils.radDeg;
			arotation = Math.atan2(rc, ra) * MathUtils.radDeg;
		} else {
			ascaleX = 0;
			ascaleY = Math.sqrt(rb * rb + rd * rd);
			ashearY = 0;
			arotation = 90 - Math.atan2(rd, rb) * MathUtils.radDeg;
		}		
	}

	public function worldToLocal (world:Vector.<Number>) : void {
		var a:Number = _a, b:Number = _b, c:Number = _c, d:Number = _d;
		var invDet:Number = 1 / (a * d - b * c);
		var x:Number = world[0] - _worldX, y:Number = world[1] - _worldY;				
		world[0] = (x * d * invDet - y * b * invDet);
		world[1] = (y * a * invDet - x * c * invDet);
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
