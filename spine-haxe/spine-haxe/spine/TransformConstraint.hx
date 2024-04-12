/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine;

class TransformConstraint implements Updatable {
	private var _data:TransformConstraintData;
	private var _bones:Array<Bone>;

	public var target:Bone;
	public var mixRotate:Float = 0;
	public var mixX:Float = 0;
	public var mixY:Float = 0;
	public var mixScaleX:Float = 0;
	public var mixScaleY:Float = 0;
	public var mixShearY:Float = 0;

	private var _temp:Array<Float> = new Array<Float>();

	public var active:Bool = false;

	public function new(data:TransformConstraintData, skeleton:Skeleton) {
		if (data == null)
			throw new SpineException("data cannot be null.");
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");
		_data = data;
		mixRotate = data.mixRotate;
		mixX = data.mixX;
		mixY = data.mixY;
		mixScaleX = data.mixScaleX;
		mixScaleY = data.mixScaleY;
		mixShearY = data.mixShearY;
		_bones = new Array<Bone>();
		for (boneData in data.bones) {
			_bones.push(skeleton.findBone(boneData.name));
		}
		target = skeleton.findBone(data.target.name);
	}

	public function isActive():Bool {
		return active;
	}

	public function setToSetupPose () {
		var data:TransformConstraintData = _data;
		mixRotate = data.mixRotate;
		mixX = data.mixX;
		mixY = data.mixY;
		mixScaleX = data.mixScaleX;
		mixScaleY = data.mixScaleY;
		mixShearY = data.mixShearY;
	}

	public function update(physics:Physics):Void {
		if (mixRotate == 0 && mixX == 0 && mixY == 0 && mixScaleX == 0 && mixScaleY == 0 && mixShearY == 0)
			return;

		if (data.local) {
			if (data.relative) {
				applyRelativeLocal();
			} else {
				applyAbsoluteLocal();
			}
		} else {
			if (data.relative) {
				applyRelativeWorld();
			} else {
				applyAbsoluteWorld();
			}
		}
	}

	private function applyAbsoluteWorld():Void {
		var translate:Bool = mixX != 0 || mixY != 0;
		var ta:Float = target.a,
			tb:Float = target.b,
			tc:Float = target.c,
			td:Float = target.d;
		var degRadReflect:Float = ta * td - tb * tc > 0 ? MathUtils.degRad : -MathUtils.degRad;
		var offsetRotation:Float = data.offsetRotation * degRadReflect;
		var offsetShearY:Float = data.offsetShearY * degRadReflect;
		for (bone in bones) {
			if (mixRotate != 0) {
				var a:Float = bone.a,
					b:Float = bone.b,
					c:Float = bone.c,
					d:Float = bone.d;
				var r:Float = Math.atan2(tc, ta) - Math.atan2(c, a) + offsetRotation;
				if (r > Math.PI)
					r -= Math.PI * 2;
				else if (r < -Math.PI)
					r += Math.PI * 2;
				r *= mixRotate;
				var cos:Float = Math.cos(r), sin:Float = Math.sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}

			if (translate) {
				_temp[0] = data.offsetX;
				_temp[1] = data.offsetY;
				target.localToWorld(_temp);
				bone.worldX += (_temp[0] - bone.worldX) * mixX;
				bone.worldY += (_temp[1] - bone.worldY) * mixY;
			}

			if (mixScaleX != 0) {
				var s:Float = Math.sqrt(bone.a * bone.a + bone.c * bone.c);
				if (s != 0)
					s = (s + (Math.sqrt(ta * ta + tc * tc) - s + _data.offsetScaleX) * mixScaleX) / s;
				bone.a *= s;
				bone.c *= s;
			}

			if (mixScaleY != 0) {
				var s:Float = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
				if (s != 0)
					s = (s + (Math.sqrt(tb * tb + td * td) - s + _data.offsetScaleY) * mixScaleY) / s;
				bone.b *= s;
				bone.d *= s;
			}

			if (mixShearY > 0) {
				var by:Float = Math.atan2(bone.d, bone.b);
				var r:Float = Math.atan2(td, tb) - Math.atan2(tc, ta) - (by - Math.atan2(bone.c, bone.a));
				if (r > Math.PI)
					r -= Math.PI * 2;
				else if (r < -Math.PI)
					r += Math.PI * 2;
				r = by + (r + offsetShearY) * mixShearY;
				var s:Float = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
				bone.b = Math.cos(r) * s;
				bone.d = Math.sin(r) * s;
			}

			bone.updateAppliedTransform();
		}
	}

	public function applyRelativeWorld():Void {
		var translate:Bool = mixX != 0 || mixY != 0;
		var ta:Float = target.a,
			tb:Float = target.b,
			tc:Float = target.c,
			td:Float = target.d;
		var degRadReflect:Float = ta * td - tb * tc > 0 ? MathUtils.degRad : -MathUtils.degRad;
		var offsetRotation:Float = _data.offsetRotation * degRadReflect,
			offsetShearY:Float = _data.offsetShearY * degRadReflect;
		for (bone in bones) {
			if (mixRotate != 0) {
				var a:Float = bone.a,
					b:Float = bone.b,
					c:Float = bone.c,
					d:Float = bone.d;
				var r:Float = Math.atan2(tc, ta) + offsetRotation;
				if (r > MathUtils.PI)
					r -= MathUtils.PI2;
				else if (r < -MathUtils.PI)
					r += MathUtils.PI2;
				r *= mixRotate;
				var cos:Float = Math.cos(r), sin:Float = Math.sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}

			if (translate) {
				var temp:Array<Float> = _temp;
				temp[0] = _data.offsetX;
				temp[1] = _data.offsetY;
				target.localToWorld(temp);
				bone.worldX += temp[0] * mixX;
				bone.worldY += temp[1] * mixY;
			}

			if (mixScaleX != 0) {
				var s:Float = (Math.sqrt(ta * ta + tc * tc) - 1 + _data.offsetScaleX) * mixScaleX + 1;
				bone.a *= s;
				bone.c *= s;
			}

			if (mixScaleY != 0) {
				var s:Float = (Math.sqrt(tb * tb + td * td) - 1 + _data.offsetScaleY) * mixScaleY + 1;
				bone.b *= s;
				bone.d *= s;
			}

			if (mixShearY > 0) {
				var r = Math.atan2(td, tb) - Math.atan2(tc, ta);
				if (r > MathUtils.PI)
					r -= MathUtils.PI2;
				else if (r < -MathUtils.PI)
					r += MathUtils.PI2;
				var b = bone.b;
				var d = bone.d;
				r = Math.atan2(d, b) + (r - MathUtils.PI / 2 + offsetShearY) * mixShearY;
				var s = Math.sqrt(b * b + d * d);
				bone.b = Math.cos(r) * s;
				bone.d = Math.sin(r) * s;
			}

			bone.updateAppliedTransform();
		}
	}

	public function applyAbsoluteLocal():Void {
		for (bone in bones) {
			var rotation:Float = bone.arotation;
			if (mixRotate != 0) rotation += (target.arotation - rotation + _data.offsetRotation) * mixRotate;

			var x:Float = bone.ax, y:Float = bone.ay;
			x += (target.ax - x + _data.offsetX) * mixX;
			y += (target.ay - y + _data.offsetY) * mixY;

			var scaleX:Float = bone.ascaleX, scaleY:Float = bone.ascaleY;
			if (mixScaleX != 0 && scaleX != 0) {
				scaleX = (scaleX + (target.ascaleX - scaleX + _data.offsetScaleX) * mixScaleX) / scaleX;
			}
			if (mixScaleY != 0 && scaleY != 0) {
				scaleY = (scaleY + (target.ascaleY - scaleY + _data.offsetScaleY) * mixScaleX) / scaleY;
			}

			var shearY:Float = bone.ashearY;
			if (mixShearY != 0) shearY += (target.ashearY - shearY + _data.offsetShearY) * mixShearY;

			bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
		}
	}

	public function applyRelativeLocal():Void {
		for (bone in bones) {
			var rotation:Float = bone.arotation + (target.arotation + _data.offsetRotation) * mixRotate;
			var x:Float = bone.ax + (target.ax + _data.offsetX) * mixX;
			var y:Float = bone.ay + (target.ay + _data.offsetY) * mixY;
			var scaleX:Float = bone.ascaleX * (((target.ascaleX - 1 + _data.offsetScaleX) * mixScaleX) + 1);
			var scaleY:Float = bone.ascaleY * (((target.ascaleY - 1 + _data.offsetScaleY) * mixScaleY) + 1);
			var shearY:Float = bone.ashearY + (target.ashearY + _data.offsetShearY) * mixShearY;
			bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
		}
	}

	public var data(get, never):TransformConstraintData;

	private function get_data():TransformConstraintData {
		return _data;
	}

	public var bones(get, never):Array<Bone>;

	private function get_bones():Array<Bone> {
		return _bones;
	}

	public function toString():String {
		return _data.name != null ? _data.name : "TransformConstraint?";
	}
}
