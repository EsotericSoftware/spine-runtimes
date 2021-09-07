/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {
	public class TransformConstraint implements Updatable {
		internal var _data : TransformConstraintData;
		internal var _bones : Vector.<Bone>;
		public var target : Bone;
		public var mixRotate : Number;
		public var mixX : Number, mixY : Number;
		public var mixScaleX : Number, mixScaleY : Number;
		public var mixShearY : Number;
		internal var _temp : Vector.<Number> = new Vector.<Number>(2, true);
		public var active : Boolean;

		public function TransformConstraint(data : TransformConstraintData, skeleton : Skeleton) {
			if (data == null) throw new ArgumentError("data cannot be null.");
			if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");
			_data = data;
			mixRotate = data.mixRotate;
			mixX = data.mixX;
			mixY = data.mixY;
			mixScaleX = data.mixScaleX;
			mixScaleY = data.mixScaleY;
			mixShearY = data.mixShearY;
			_bones = new Vector.<Bone>();
			for each (var boneData : BoneData in data.bones)
				_bones.push(skeleton.findBone(boneData.name));
			target = skeleton.findBone(data.target._name);
		}
		
		public function isActive() : Boolean {
			return active;
		}

		public function update() : void {
			if (mixRotate == 0 && mixX == 0 && mixY == 0 && mixScaleX == 0 && mixScaleX == 0 && mixShearY == 0) return;

			if (_data.local) {
				if (_data.relative)
					applyRelativeLocal();
				else
					applyAbsoluteLocal();
			} else {
				if (_data.relative)
					applyRelativeWorld();
				else
					applyAbsoluteWorld();
			}
		}

		internal function applyAbsoluteWorld() : void {
			var mixRotate : Number = this.mixRotate, mixX : Number = this.mixX, mixY : Number = this.mixY;
			var mixScaleX : Number = this.mixScaleX, mixScaleY : Number = this.mixScaleY, mixShearY : Number = this.mixShearY;
			var translate : Boolean = mixX != 0 || mixY != 0;

			var target : Bone = this.target;
			var ta : Number = target.a, tb : Number = target.b, tc : Number = target.c, td : Number = target.d;
			var degRadReflect : Number = ta * td - tb * tc > 0 ? MathUtils.degRad : -MathUtils.degRad;
			var offsetRotation : Number = _data.offsetRotation * degRadReflect;
			var offsetShearY : Number = _data.offsetShearY * degRadReflect;

			var bones : Vector.<Bone> = _bones;
			for (var i : int = 0, n : int = bones.length; i < n; i++) {
				var bone : Bone = bones[i];

				if (mixRotate != 0) {
					var a : Number = bone.a, b : Number = bone.b, c : Number = bone.c, d : Number = bone.d;
					var r : Number = Math.atan2(tc, ta) - Math.atan2(c, a) + offsetRotation;
					if (r > Math.PI)
						r -= Math.PI * 2;
					else if (r < -Math.PI) //
						r += Math.PI * 2;
					r *= mixRotate;
					var cos : Number = Math.cos(r), sin : Number = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}

				if (translate) {
					_temp[0] = _data.offsetX;
					_temp[1] = _data.offsetY;
					target.localToWorld(_temp);
					bone.worldX += (_temp[0] - bone.worldX) * mixX;
					bone.worldY += (_temp[1] - bone.worldY) * mixY;
				}

				var s : Number;
				if (mixScaleX != 0) {
					s = Math.sqrt(bone.a * bone.a + bone.c * bone.c);
					if (s != 0) s = (s + (Math.sqrt(ta * ta + tc * tc) - s + _data.offsetScaleX) * mixScaleX) / s;
					bone.a *= s;
					bone.c *= s;
				}
				if (mixScaleY != 0) {
					s = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
					if (s != 0) s = (s + (Math.sqrt(tb * tb + td * td) - s + _data.offsetScaleY) * mixScaleY) / s;
					bone.b *= s;
					bone.d *= s;
				}

				if (mixShearY > 0) {
					var by : Number = Math.atan2(bone.d, bone.b);
					r = Math.atan2(td, tb) - Math.atan2(tc, ta) - (by - Math.atan2(bone.c, bone.a));
					if (r > Math.PI)
						r -= Math.PI * 2;
					else if (r < -Math.PI) //
						r += Math.PI * 2;
					r = by + (r + offsetShearY) * mixShearY;
					s = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
					bone.b = Math.cos(r) * s;
					bone.d = Math.sin(r) * s;
				}

				bone.updateAppliedTransform();
			}
		}

		public function applyRelativeWorld() : void {
			var mixRotate : Number = this.mixRotate, mixX : Number = this.mixX, mixY : Number = this.mixY;
			var mixScaleX : Number = this.mixScaleX, mixScaleY : Number = this.mixScaleY, mixShearY : Number = this.mixShearY;
			var translate : Boolean = mixX != 0 || mixY != 0;

			var target : Bone = this.target;
			var ta : Number = target.a, tb : Number = target.b, tc : Number = target.c, td : Number = target.d;
			var degRadReflect : Number = ta * td - tb * tc > 0 ? MathUtils.degRad : -MathUtils.degRad;
			var offsetRotation : Number = _data.offsetRotation * degRadReflect, offsetShearY : Number = _data.offsetShearY * degRadReflect;

			var bones : Vector.<Bone> = _bones;
			for (var i : int = 0, n : int = bones.length; i < n; i++) {
				var bone : Bone = bones[i];

				if (mixRotate != 0) {
					var a : Number = bone.a, b : Number = bone.b, c : Number = bone.c, d : Number = bone.d;
					var r : Number = Math.atan2(tc, ta) + offsetRotation;
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) //
						r += MathUtils.PI2;
					r *= mixRotate;
					var cos : Number = Math.cos(r), sin : Number = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}

				if (translate) {
					var temp : Vector.<Number> = _temp;
					temp[0] = _data.offsetX;
					temp[1] = _data.offsetY;
					target.localToWorld(temp);
					bone.worldX += temp[0] * mixX;
					bone.worldY += temp[1] * mixY;
				}

				var s : Number;
				if (mixScaleX != 0) {
					s = (Math.sqrt(ta * ta + tc * tc) - 1 + _data.offsetScaleX) * mixScaleX + 1;
					bone.a *= s;
					bone.c *= s;
				}
				if (mixScaleY != 0) {
					s = (Math.sqrt(tb * tb + td * td) - 1 + _data.offsetScaleY) * mixScaleY + 1;
					bone.b *= s;
					bone.d *= s;
				}

				if (mixShearY > 0) {
					r = Math.atan2(td, tb) - Math.atan2(tc, ta);
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) //
						r += MathUtils.PI2;
					b = bone.b;
					d = bone.d;
					r = Math.atan2(d, b) + (r - MathUtils.PI / 2 + offsetShearY) * mixShearY;
					s = Math.sqrt(b * b + d * d);
					bone.b = Math.cos(r) * s;
					bone.d = Math.sin(r) * s;
				}

				bone.updateAppliedTransform();
			}
		}

		public function applyAbsoluteLocal() : void {
			var mixRotate : Number = this.mixRotate, mixX : Number = this.mixX, mixY : Number = this.mixY;
			var mixScaleX : Number = this.mixScaleX, mixScaleY : Number = this.mixScaleY, mixShearY : Number = this.mixShearY;

			var target : Bone = this.target;

			var bones : Vector.<Bone> = _bones;
			for (var i : int = 0, n : int = bones.length; i < n; i++) {
				var bone : Bone = bones[i];

				var rotation : Number = bone.arotation;
				if (mixRotate != 0) {
					var r : Number = target.arotation - rotation + _data.offsetRotation;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
					rotation += r * mixRotate;
				}

				var x : Number = bone.ax, y : Number = bone.ay;
				x += (target.ax - x + _data.offsetX) * mixX;
				y += (target.ay - y + _data.offsetY) * mixY;

				var scaleX : Number = bone.ascaleX, scaleY : Number = bone.ascaleY;
				if (mixScaleX != 0 && scaleX != 0)
					scaleX = (scaleX + (target.ascaleX - scaleX + _data.offsetScaleX) * mixScaleX) / scaleX;
				if (mixScaleY != 0 && scaleY != 0)
					scaleY = (scaleY + (target.ascaleY - scaleY + _data.offsetScaleY) * mixScaleY) / scaleY;

				var shearY : Number = bone.ashearY;
				if (mixShearY != 0) {
					r = target.ashearY - shearY + _data.offsetShearY;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
					bone.shearY += r * mixShearY;
				}

				bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		}

		public function applyRelativeLocal() : void {
			var mixRotate : Number = this.mixRotate, mixX : Number = this.mixX, mixY : Number = this.mixY;
			var mixScaleX : Number = this.mixScaleX, mixScaleY : Number = this.mixScaleY, mixShearY : Number = this.mixShearY;

			var target : Bone = this.target;

			var bones : Vector.<Bone> = _bones;
			for (var i : int = 0, n : int = bones.length; i < n; i++) {
				var bone : Bone = bones[i];

				var rotation : Number = bone.arotation + (target.arotation + _data.offsetRotation) * mixRotate;
				var x : Number = bone.ax + (target.ax + _data.offsetX) * mixX;
				var y : Number = bone.ay + (target.ay + _data.offsetY) * mixY;
				var scaleX : Number = bone.ascaleX * (((target.ascaleX - 1 + _data.offsetScaleX) * mixScaleX) + 1);
				var scaleY : Number = bone.ascaleY * (((target.ascaleY - 1 + _data.offsetScaleY) * mixScaleY) + 1);
				var shearY : Number = bone.ashearY + (target.ashearY + _data.offsetShearY) * mixShearY;

				bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		}

		public function get data() : TransformConstraintData {
			return _data;
		}

		public function get bones() : Vector.<Bone> {
			return _bones;
		}

		public function toString() : String {
			return _data.name;
		}
	}
}
