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

class Bone implements Updatable {
	static public var yDown:Bool = false;

	private var _data:BoneData;
	private var _skeleton:Skeleton;
	private var _parent:Bone;
	private var _children:Array<Bone> = new Array<Bone>();

	public var x:Float = 0;
	public var y:Float = 0;
	public var rotation:Float = 0;
	public var scaleX:Float = 0;
	public var scaleY:Float = 0;
	public var shearX:Float = 0;
	public var shearY:Float = 0;
	public var ax:Float = 0;
	public var ay:Float = 0;
	public var arotation:Float = 0;
	public var ascaleX:Float = 0;
	public var ascaleY:Float = 0;
	public var ashearX:Float = 0;
	public var ashearY:Float = 0;
	public var a:Float = 0;
	public var b:Float = 0;
	public var c:Float = 0;
	public var d:Float = 0;
	public var worldX:Float = 0;
	public var worldY:Float = 0;
	public var sorted:Bool = false;
	public var active:Bool = false;

	/** @param parent May be null. */
	public function new(data:BoneData, skeleton:Skeleton, parent:Bone) {
		if (data == null)
			throw new SpineException("data cannot be null.");
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");
		_data = data;
		_skeleton = skeleton;
		_parent = parent;
		setToSetupPose();
	}

	public function isActive():Bool {
		return active;
	}

	/** Same as updateWorldTransform(). This method exists for Bone to implement Updatable. */
	public function update():Void {
		updateWorldTransformWith(ax, ay, arotation, ascaleX, ascaleY, ashearX, ashearY);
	}

	/** Computes the world SRT using the parent bone and this bone's local SRT. */
	public function updateWorldTransform():Void {
		updateWorldTransformWith(x, y, rotation, scaleX, scaleY, shearX, shearY);
	}

	/** Computes the world SRT using the parent bone and the specified local SRT. */
	public function updateWorldTransformWith(x:Float, y:Float, rotation:Float, scaleX:Float, scaleY:Float, shearX:Float, shearY:Float):Void {
		ax = x;
		ay = y;
		arotation = rotation;
		ascaleX = scaleX;
		ascaleY = scaleY;
		ashearX = shearX;
		ashearY = shearY;

		var rotationY:Float = 0;
		var la:Float = 0;
		var lb:Float = 0;
		var lc:Float = 0;
		var ld:Float = 0;
		var sin:Float = 0;
		var cos:Float = 0;
		var s:Float = 0;
		var sx:Float = skeleton.scaleX;
		var sy:Float = skeleton.scaleY * (yDown ? -1 : 1);

		var parent:Bone = _parent;
		if (parent == null) {
			// Root bone.
			rotationY = rotation + 90 + shearY;
			a = MathUtils.cosDeg(rotation + shearX) * scaleX * sx;
			b = MathUtils.cosDeg(rotationY) * scaleY * sx;
			c = MathUtils.sinDeg(rotation + shearX) * scaleX * sy;
			d = MathUtils.sinDeg(rotationY) * scaleY * sy;
			worldX = x * sx + skeleton.x;
			worldY = y * sy + skeleton.y;
			return;
		}

		var pa:Float = parent.a,
			pb:Float = parent.b,
			pc:Float = parent.c,
			pd:Float = parent.d;
		worldX = pa * x + pb * y + parent.worldX;
		worldY = pc * x + pd * y + parent.worldY;

		switch (data.transformMode) {
			case TransformMode.normal:
				rotationY = rotation + 90 + shearY;
				la = MathUtils.cosDeg(rotation + shearX) * scaleX;
				lb = MathUtils.cosDeg(rotationY) * scaleY;
				lc = MathUtils.sinDeg(rotation + shearX) * scaleX;
				ld = MathUtils.sinDeg(rotationY) * scaleY;
				a = pa * la + pb * lc;
				b = pa * lb + pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
				return;
			case TransformMode.onlyTranslation:
				rotationY = rotation + 90 + shearY;
				a = MathUtils.cosDeg(rotation + shearX) * scaleX;
				b = MathUtils.cosDeg(rotationY) * scaleY;
				c = MathUtils.sinDeg(rotation + shearX) * scaleX;
				d = MathUtils.sinDeg(rotationY) * scaleY;
			case TransformMode.noRotationOrReflection:
				s = pa * pa + pc * pc;
				var prx:Float = 0;
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
				var rx:Float = rotation + shearX - prx;
				var ry:Float = rotation + shearY - prx + 90;
				la = MathUtils.cosDeg(rx) * scaleX;
				lb = MathUtils.cosDeg(ry) * scaleY;
				lc = MathUtils.sinDeg(rx) * scaleX;
				ld = MathUtils.sinDeg(ry) * scaleY;
				a = pa * la - pb * lc;
				b = pa * lb - pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
			case TransformMode.noScale, TransformMode.noScaleOrReflection:
				cos = MathUtils.cosDeg(rotation);
				sin = MathUtils.sinDeg(rotation);
				var za:Float = (pa * cos + pb * sin) / sx;
				var zc:Float = (pc * cos + pd * sin) / sy;
				s = Math.sqrt(za * za + zc * zc);
				if (s > 0.00001)
					s = 1 / s;
				za *= s;
				zc *= s;
				s = Math.sqrt(za * za + zc * zc);
				if (data.transformMode == TransformMode.noScale && ((pa * pd - pb * pc < 0) != ((sx < 0) != (sy < 0)))) {
					s = -s;
				}
				var r:Float = Math.PI / 2 + Math.atan2(zc, za);
				var zb:Float = Math.cos(r) * s;
				var zd:Float = Math.sin(r) * s;
				la = MathUtils.cosDeg(shearX) * scaleX;
				lb = MathUtils.cosDeg(90 + shearY) * scaleY;
				lc = MathUtils.sinDeg(shearX) * scaleX;
				ld = MathUtils.sinDeg(90 + shearY) * scaleY;
				a = za * la + zb * lc;
				b = za * lb + zb * ld;
				c = zc * la + zd * lc;
				d = zc * lb + zd * ld;
		}
		a *= sx;
		b *= sx;
		c *= sy;
		d *= sy;
	}

	public function setToSetupPose():Void {
		x = data.x;
		y = data.y;
		rotation = data.rotation;
		scaleX = data.scaleX;
		scaleY = data.scaleY;
		shearX = data.shearX;
		shearY = data.shearY;
	}

	public var data(get, never):BoneData;

	private function get_data():BoneData {
		return _data;
	}

	public var skeleton(get, never):Skeleton;

	private function get_skeleton():Skeleton {
		return _skeleton;
	}

	public var parent(get, never):Bone;

	private function get_parent():Bone {
		return _parent;
	}

	public var children(get, never):Array<Bone>;

	private function get_children():Array<Bone> {
		return _children;
	}

	public var worldRotationX(get, never):Float;

	private function get_worldRotationX():Float {
		return Math.atan2(c, a) * MathUtils.radDeg;
	}

	public var worldRotationY(get, never):Float;

	private function get_worldRotationY():Float {
		return Math.atan2(d, b) * MathUtils.radDeg;
	}

	public var worldScaleX(get, never):Float;

	private function get_worldScaleX():Float {
		return Math.sqrt(a * a + c * c);
	}

	public var worldScaleY(get, never):Float;

	private function get_worldScaleY():Float {
		return Math.sqrt(b * b + d * d);
	}

	/** Computes the individual applied transform values from the world transform. This can be useful to perform processing using
	 * the applied transform after the world transform has been modified directly (eg, by a constraint).
	 * <p>
	 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. */
	public function updateAppliedTransform():Void {
		var parent:Bone = parent;
		if (parent == null) {
			ax = worldX - skeleton.x;
			ay = worldY - skeleton.y;
			arotation = Math.atan2(c, a) * MathUtils.radDeg;
			ascaleX = Math.sqrt(a * a + c * c);
			ascaleY = Math.sqrt(b * b + d * d);
			ashearX = 0;
			ashearY = Math.atan2(a * b + c * d, a * d - b * c) * MathUtils.radDeg;
			return;
		}
		var pa:Float = parent.a,
			pb:Float = parent.b,
			pc:Float = parent.c,
			pd:Float = parent.d;
		var pid:Float = 1 / (pa * pd - pb * pc);
		var dx:Float = worldX - parent.worldX,
			dy:Float = worldY - parent.worldY;
		ax = (dx * pd * pid - dy * pb * pid);
		ay = (dy * pa * pid - dx * pc * pid);
		var ia:Float = pid * pd;
		var id:Float = pid * pa;
		var ib:Float = pid * pb;
		var ic:Float = pid * pc;
		var ra:Float = ia * a - ib * c;
		var rb:Float = ia * b - ib * d;
		var rc:Float = id * c - ic * a;
		var rd:Float = id * d - ic * b;
		ashearX = 0;
		ascaleX = Math.sqrt(ra * ra + rc * rc);
		if (scaleX > 0.0001) {
			var det:Float = ra * rd - rb * rc;
			ascaleY = det / ascaleX;
			ashearY = Math.atan2(ra * rb + rc * rd, det) * MathUtils.radDeg;
			arotation = Math.atan2(rc, ra) * MathUtils.radDeg;
		} else {
			ascaleX = 0;
			ascaleY = Math.sqrt(rb * rb + rd * rd);
			ashearY = 0;
			arotation = 90 - Math.atan2(rd, rb) * MathUtils.radDeg;
		}
	}

	public function worldToLocal(world:Array<Float>):Void {
		var a:Float = a, b:Float = b, c:Float = c, d:Float = d;
		var invDet:Float = 1 / (a * d - b * c);
		var x:Float = world[0] - worldX, y:Float = world[1] - worldY;
		world[0] = (x * d * invDet - y * b * invDet);
		world[1] = (y * a * invDet - x * c * invDet);
	}

	public function localToWorld(local:Array<Float>):Void {
		var localX:Float = local[0], localY:Float = local[1];
		local[0] = localX * a + localY * b + worldX;
		local[1] = localX * c + localY * d + worldY;
	}

	public function worldToLocalRotation(worldRotation:Float):Float {
		var sin:Float = MathUtils.sinDeg(worldRotation),
			cos:Float = MathUtils.cosDeg(worldRotation);
		return Math.atan2(a * sin - c * cos, d * cos - b * sin) * MathUtils.radDeg + rotation - shearX;
	}

	public function localToWorldRotation(localRotation:Float):Float {
		localRotation -= rotation - shearX;
		var sin:Float = MathUtils.sinDeg(localRotation),
			cos:Float = MathUtils.cosDeg(localRotation);
		return Math.atan2(cos * c + sin * d, cos * a + sin * b) * MathUtils.radDeg;
	}

	public function rotateWorld(degrees:Float):Void {
		var a:Float = this.a,
			b:Float = this.b,
			c:Float = this.c,
			d:Float = this.d;
		var cos:Float = MathUtils.cosDeg(degrees),
			sin:Float = MathUtils.sinDeg(degrees);
		this.a = cos * a - sin * c;
		this.b = cos * b - sin * d;
		this.c = sin * a + cos * c;
		this.d = sin * b + cos * d;
	}

	public function toString():String {
		return data.name;
	}
}
