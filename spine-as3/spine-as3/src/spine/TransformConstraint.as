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

public class TransformConstraint implements Constraint {
	internal var _data:TransformConstraintData;
	internal var _bones:Vector.<Bone>;
	public var target:Bone;
	public var rotateMix:Number;	
	public var translateMix:Number;
	public var scaleMix:Number;
	public var shearMix:Number;
	internal var _temp:Vector.<Number> = new Vector.<Number>(2);

	public function TransformConstraint (data:TransformConstraintData, skeleton:Skeleton) {
		if (data == null) throw new ArgumentError("data cannot be null.");
		if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");
		_data = data;
		rotateMix = data.rotateMix;
		translateMix = data.translateMix;
		scaleMix = data.scaleMix;
		shearMix = data.shearMix;
		_bones = new Vector.<Bone>();		
		for each (var boneData:BoneData in data.bones)
			_bones.push(skeleton.findBone(boneData.name));		
		target = skeleton.findBone(data.target._name);
	}		

	public function apply () : void {
		update();
	}

	public function update () : void {
		var rotateMix:Number = this.rotateMix, translateMix:Number = this.translateMix, scaleMix:Number = this.scaleMix, shearMix:Number = this.shearMix;
		var target:Bone = this.target;
		var ta:Number = target.a, tb:Number = target.b, tc:Number = target.c, td:Number = target.d;
		var degRadReflect:Number = ta * td - tb * tc > 0 ? MathUtils.degRad : -MathUtils.degRad;
		var offsetRotation:Number = data.offsetRotation * degRadReflect;
		var offsetShearY:Number = data.offsetShearY * degRadReflect;
		var bones:Vector.<Bone> = this._bones;
		for (var i:int = 0, n:int = bones.length; i < n; i++) {
			var bone:Bone = bones[i];
			var modified:Boolean = false;

			if (rotateMix != 0) {
				var a:Number = bone.a, b:Number = bone.b, c:Number = bone.c, d:Number = bone.d;
				var r:Number = Math.atan2(tc, ta) - Math.atan2(c, a) + offsetRotation;
				if (r > Math.PI)
					r -= Math.PI * 2;
				else if (r < -Math.PI) r += Math.PI * 2;
				r *= rotateMix;
				var cos:Number = Math.cos(r), sin:Number = Math.sin(r);
				bone._a = cos * a - sin * c;
				bone._b = cos * b - sin * d;
				bone._c = sin * a + cos * c;
				bone._d = sin * b + cos * d;
				modified = true;
			}

			if (translateMix != 0) {
				_temp[0] = data.offsetX;
				_temp[1] = data.offsetY;
				target.localToWorld(_temp);
				bone._worldX += (_temp[0] - bone.worldX) * translateMix;
				bone._worldY += (_temp[1] - bone.worldY) * translateMix;
				modified = true;
			}

			if (scaleMix > 0) {
				var s:Number = Math.sqrt(bone.a * bone.a + bone.c * bone.c);
				var ts:Number = Math.sqrt(ta * ta + tc * tc);
				if (s > 0.00001) s = (s + (ts - s + data.offsetScaleX) * scaleMix) / s;
				bone._a *= s;
				bone._c *= s;
				s = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
				ts = Math.sqrt(tb * tb + td * td);
				if (s > 0.00001) s = (s + (ts - s + data.offsetScaleY) * scaleMix) / s;
				bone._b *= s;
				bone._d *= s;
				modified = true;
			}

			if (shearMix > 0) {
				b = bone.b, d = bone.d;
				var by:Number = Math.atan2(d, b);
				r = Math.atan2(td, tb) - Math.atan2(tc, ta) - (by - Math.atan2(bone.c, bone.a));
				if (r > Math.PI)
					r -= Math.PI * 2;
				else if (r < -Math.PI) r += Math.PI * 2;
				r = by + (r + offsetShearY) * shearMix;
				s = Math.sqrt(b * b + d * d);
				bone._b = Math.cos(r) * s;
				bone._d = Math.sin(r) * s;
				modified = true;
			}
			
			if (modified) bone.appliedValid = false;
		}
	}
	
	public function getOrder () : Number {
		return _data.order;
	}

	public function get data () : TransformConstraintData {
		return _data;
	}
	
	public function get bones () : Vector.<Bone> {
		return _bones;
	}

	public function toString () : String {
		return _data._name;
	}
}

}
