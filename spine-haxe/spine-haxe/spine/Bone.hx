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
	public var inherit:Inherit = Inherit.normal;
	public var sorted:Bool = false;
	public var active:Bool = false;

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
	public function update(physics:Physics):Void {
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
			var rx:Float = (rotation + shearX) * MathUtils.degRad;
			var ry:Float = (rotation + 90 + shearY) * MathUtils.degRad;
			a = Math.cos(rx) * scaleX * sx;
			b = Math.cos(ry) * scaleY * sx;
			c = Math.sin(rx) * scaleX * sy;
			d = Math.sin(ry) * scaleY * sy;
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

		switch (data.inherit) {
			case Inherit.normal:
				var rx:Float = (rotation + shearX) * MathUtils.degRad;
				var ry:Float = (rotation + 90 + shearY) * MathUtils.degRad;
				la = Math.cos(rx) * scaleX;
				lb = Math.cos(ry) * scaleY;
				lc = Math.sin(rx) * scaleX;
				ld = Math.sin(ry) * scaleY;
				a = pa * la + pb * lc;
				b = pa * lb + pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
				return;
			case Inherit.onlyTranslation:
				var rx:Float = (rotation + shearX) * MathUtils.degRad;
				var ry:Float = (rotation + 90 + shearY) * MathUtils.degRad;
				a = Math.cos(rx) * scaleX;
				b = Math.cos(ry) * scaleY;
				c = Math.sin(rx) * scaleX;
				d = Math.sin(ry) * scaleY;
			case Inherit.noRotationOrReflection:
				var sx:Float = 1 / skeleton.scaleX;
				var sy:Float = 1 / skeleton.scaleY;
				pa *= sx;
				pc *= sy;
				s = pa * pa + pc * pc;
				var prx:Float = 0;
				if (s > 0.0001) {
					s = Math.abs(pa * pd * sy - pb * sx * pc) / s;
					pb = pc * s;
					pd = pa * s;
					prx = Math.atan2(pc, pa) * MathUtils.radDeg;
				} else {
					pa = 0;
					pc = 0;
					prx = 90 - Math.atan2(pd, pb) * MathUtils.radDeg;
				}
				var rx:Float = (rotation + shearX - prx) * MathUtils.degRad;
				var ry:Float = (rotation + shearY - prx + 90) * MathUtils.degRad;
				la = Math.cos(rx) * scaleX;
				lb = Math.cos(ry) * scaleY;
				lc = Math.sin(rx) * scaleX;
				ld = Math.sin(ry) * scaleY;
				a = pa * la - pb * lc;
				b = pa * lb - pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
			case Inherit.noScale, Inherit.noScaleOrReflection:
				rotation *= MathUtils.degRad;
				cos = Math.cos(rotation);
				sin = Math.sin(rotation);
				var za:Float = (pa * cos + pb * sin) / sx;
				var zc:Float = (pc * cos + pd * sin) / sy;
				s = Math.sqrt(za * za + zc * zc);
				if (s > 0.00001)
					s = 1 / s;
				za *= s;
				zc *= s;
				s = Math.sqrt(za * za + zc * zc);
				if (data.inherit == Inherit.noScale && ((pa * pd - pb * pc < 0) != ((sx < 0) != (sy < 0)))) {
					s = -s;
				}
				rotation = Math.PI / 2 + Math.atan2(zc, za);
				var zb:Float = Math.cos(rotation) * s;
				var zd:Float = Math.sin(rotation) * s;
				shearX *= MathUtils.degRad;
				shearY = (90 + shearY) * MathUtils.degRad;
				la = Math.cos(shearX) * scaleX;
				lb = Math.cos(shearY) * scaleY;
				lc = Math.sin(shearX) * scaleX;
				ld = Math.sin(shearY) * scaleY;
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
		inherit = data.inherit;
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
		var ia:Float = pd * pid,
			ib:Float = pb * pid,
			ic:Float = pc * pid,
			id:Float = pa * pid;
		var dx:Float = worldX - parent.worldX,
			dy:Float = worldY - parent.worldY;
		ax = (dx * ia - dy * ib);
		ay = (dy * id - dx * ic);
		var ra:Float, rb:Float, rc:Float, rd:Float;
		if (inherit == Inherit.onlyTranslation) {
			ra = a;
			rb = b;
			rc = c;
			rd = d;
		} else {
			switch (inherit) {
				case Inherit.noRotationOrReflection:
					var s:Float = Math.abs(pa * pd - pb * pc) / (pa * pa + pc * pc);
					pb = -pc * skeleton.scaleX * s / skeleton.scaleY;
					pd = pa * skeleton.scaleY * s / skeleton.scaleX;
					pid = 1 / (pa * pd - pb * pc);
					ia = pd * pid;
					ib = pb * pid;
				case Inherit.noScale | Inherit.noScaleOrReflection:
					var cos:Float = MathUtils.cosDeg(rotation), sin:Float = MathUtils.sinDeg(rotation);
					pa = (pa * cos + pb * sin) / skeleton.scaleX;
					pc = (pc * cos + pd * sin) / skeleton.scaleY;
					var s:Float = Math.sqrt(pa * pa + pc * pc);
					if (s > 0.00001) s = 1 / s;
					pa *= s;
					pc *= s;
					s = Math.sqrt(pa * pa + pc * pc);
					if (inherit == Inherit.noScale && pid < 0 != ((skeleton.scaleX < 0) != (skeleton.scaleY < 0))) s = -s;
					var r:Float = MathUtils.PI / 2 + Math.atan2(pc, pa);
					pb = Math.cos(r) * s;
					pd = Math.sin(r) * s;
					pid = 1 / (pa * pd - pb * pc);
					ia = pd * pid;
					ib = pb * pid;
					ic = pc * pid;
					id = pa * pid;
			}
			ra = ia * a - ib * c;
			rb = ia * b - ib * d;
			rc = id * c - ic * a;
			rd = id * d - ic * b;
		}

		ashearX = 0;
		ascaleX = Math.sqrt(ra * ra + rc * rc);
		if (scaleX > 0.0001) {
			var det:Float = ra * rd - rb * rc;
			ascaleY = det / ascaleX;
			ashearY = -Math.atan2(ra * rb + rc * rd, det) * MathUtils.radDeg;
			arotation = Math.atan2(rc, ra) * MathUtils.radDeg;
		} else {
			ascaleX = 0;
			ascaleY = Math.sqrt(rb * rb + rd * rd);
			ashearY = 0;
			arotation = 90 - Math.atan2(rd, rb) * MathUtils.radDeg;
		}
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

	private function worldToParent(world: Array<Float>):Array<Float> {
		if (world == null)
			throw new SpineException("world cannot be null.");
		return parent == null ? world : parent.worldToLocal(world);
	}

	private function parentToWorld(world: Array<Float>):Array<Float> {
		if (world == null)
			throw new SpineException("world cannot be null.");
		return parent == null ? world : parent.localToWorld(world);
	}

	public function worldToLocal(world:Array<Float>):Array<Float> {
		var a:Float = a, b:Float = b, c:Float = c, d:Float = d;
		var invDet:Float = 1 / (a * d - b * c);
		var x:Float = world[0] - worldX, y:Float = world[1] - worldY;
		world[0] = (x * d * invDet - y * b * invDet);
		world[1] = (y * a * invDet - x * c * invDet);
		return world;
	}

	public function localToWorld(local:Array<Float>):Array<Float> {
		var localX:Float = local[0], localY:Float = local[1];
		local[0] = localX * a + localY * b + worldX;
		local[1] = localX * c + localY * d + worldY;
		return local;
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
		degrees *= MathUtils.degRad;
		var sin:Float = Math.sin(degrees), cos:Float = Math.cos(degrees);
		var ra:Float = a, rb:Float = b;
		a = cos * ra - sin * c;
		b = cos * rb - sin * d;
		c = sin * ra + cos * c;
		d = sin * rb + cos * d;
	}

	public function toString():String {
		return data.name;
	}
}
