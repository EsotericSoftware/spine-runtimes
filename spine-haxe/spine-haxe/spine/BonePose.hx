/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package spine;

/** The applied local pose and world transform for a bone. This is the Bone.pose with constraints applied and the world
 * transform computed by Skeleton.updateWorldTransform(Physics) and updateWorldTransform(Skeleton). */
class BonePose implements Pose<BonePose> implements Update {
	public var bone:Bone;

	/** The local x translation. */
	public var x:Float = 0;

	/** The local y translation. */
	public var y:Float = 0;

	/** The local rotation in degrees, counter clockwise. */
	public var rotation:Float = 0;

	/** The local scaleX. */
	public var scaleX:Float = 0;

	/** The local scaleY. */
	public var scaleY:Float = 0;

	/** The local shearX. */
	public var shearX:Float = 0;

	/** The local shearY. */
	public var shearY:Float = 0;

	/** Determines how parent world transforms affect this bone. */
	public var inherit(default, set):Inherit;

	function set_inherit(value:Inherit):Inherit {
		if (value == null)
			throw new SpineException("inherit cannot be null.");
		inherit = value;
		return value;
	}

	/** Part of the world transform matrix for the X axis. If changed, updateAppliedTransform() should be called. */
	public var a:Float = 0;

	/** Part of the world transform matrix for the Y axis. If changed, updateAppliedTransform() should be called. */
	public var b:Float = 0;

	/** Part of the world transform matrix for the X axis. If changed, updateAppliedTransform() should be called. */
	public var c:Float = 0;

	/** Part of the world transform matrix for the Y axis. If changed, updateAppliedTransform() should be called. */
	public var d:Float = 0;

	/** The world X position. If changed, updateAppliedTransform() should be called. */
	public var worldX:Float = 0;

	/** The world Y position. If changed, updateAppliedTransform() should be called. */
	public var worldY:Float = 0;

	public var world:Int;
	public var local:Int;

	public function new() {}

	public function set(pose:BonePose):Void {
		if (pose == null)
			throw new SpineException("pose cannot be null.");
		x = pose.x;
		y = pose.y;
		rotation = pose.rotation;
		scaleX = pose.scaleX;
		scaleY = pose.scaleY;
		shearX = pose.shearX;
		shearY = pose.shearY;
		inherit = pose.inherit;
	}

	/** Called by Skeleton.updateCache() to compute the world transform, if needed. */
	public function update(skeleton:Skeleton, physics:Physics):Void {
		if (world != skeleton._update)
			updateWorldTransform(skeleton);
	}

	/** Computes the world transform using the parent bone's applied pose and this pose. Child bones are not updated.
	 *
	 * @see https://esotericsoftware.com/spine-runtime-skeletons#World-transforms World transforms in the Spine Runtimes Guide
	 */
	public function updateWorldTransform(skeleton:Skeleton):Void {
		if (local == skeleton._update)
			updateLocalTransform(skeleton);
		else
			world = skeleton._update;

		if (bone.parent == null) { // Root bone.
			var sx = skeleton.scaleX, sy = skeleton.scaleY;
			var rx = (rotation + shearX) * MathUtils.degRad;
			var ry = (rotation + 90 + shearY) * MathUtils.degRad;
			a = Math.cos(rx) * scaleX * sx;
			b = Math.cos(ry) * scaleY * sx;
			c = Math.sin(rx) * scaleX * sy;
			d = Math.sin(ry) * scaleY * sy;
			worldX = x * sx + skeleton.x;
			worldY = y * sy + skeleton.y;
			return;
		}

		var parent = bone.parent.appliedPose;
		var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		worldX = pa * x + pb * y + parent.worldX;
		worldY = pc * x + pd * y + parent.worldY;

		switch (inherit) {
			case Inherit.normal:
				var rx = (rotation + shearX) * MathUtils.degRad;
				var ry = (rotation + 90 + shearY) * MathUtils.degRad;
				var la = Math.cos(rx) * scaleX;
				var lb = Math.cos(ry) * scaleY;
				var lc = Math.sin(rx) * scaleX;
				var ld = Math.sin(ry) * scaleY;
				a = pa * la + pb * lc;
				b = pa * lb + pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
			case Inherit.onlyTranslation:
				var sx = skeleton.scaleX, sy = skeleton.scaleY;
				var rx = (rotation + shearX) * MathUtils.degRad;
				var ry = (rotation + 90 + shearY) * MathUtils.degRad;
				a = Math.cos(rx) * scaleX * sx;
				b = Math.cos(ry) * scaleY * sx;
				c = Math.sin(rx) * scaleX * sy;
				d = Math.sin(ry) * scaleY * sy;
			case Inherit.noRotationOrReflection:
				var sx = skeleton.scaleX, sy = skeleton.scaleY, sxi = 1 / sx, syi = 1 / sy;
				pa *= sxi;
				pc *= syi;
				var s = pa * pa + pc * pc, r:Float;
				if (s > MathUtils.epsilon2) {
					s = Math.abs(pa * pd * syi - pb * sxi * pc) / s;
					pb = pc * s;
					pd = pa * s;
					r = rotation - MathUtils.atan2Deg(pc, pa);
				} else {
					pa = 0;
					pc = 0;
					r = rotation - 90 + MathUtils.atan2Deg(pd, pb);
				}
				var rx = (r + shearX) * MathUtils.degRad;
				var ry = (r + shearY + 90) * MathUtils.degRad;
				var la = Math.cos(rx) * scaleX;
				var lb = Math.cos(ry) * scaleY;
				var lc = Math.sin(rx) * scaleX;
				var ld = Math.sin(ry) * scaleY;
				a = (pa * la - pb * lc) * sx;
				b = (pa * lb - pb * ld) * sx;
				c = (pc * la + pd * lc) * sy;
				d = (pc * lb + pd * ld) * sy;
			case Inherit.noScale, Inherit.noScaleOrReflection:
				var sx = skeleton.scaleX, sy = skeleton.scaleY, sxi = 1 / sx, syi = 1 / sy;
				var r = rotation * MathUtils.degRad,
					cos = Math.cos(r),
					sin = Math.sin(r);
				var za = (pa * cos + pb * sin) * sxi;
				var zc = (pc * cos + pd * sin) * syi;
				var s = 1 / Math.sqrt(za * za + zc * zc);
				za *= s;
				zc *= s;
				var zb = -zc, zd = za;
				if (inherit == Inherit.noScale && ((pa * pd - pb * pc < 0) != ((sx < 0) != (sy < 0)))) {
					zb = -zb;
					zd = -zd;
				}
				var rx = shearX * MathUtils.degRad;
				var ry = (90 + shearY) * MathUtils.degRad;
				var la = Math.cos(rx) * scaleX;
				var lb = Math.cos(ry) * scaleY;
				var lc = Math.sin(rx) * scaleX;
				var ld = Math.sin(ry) * scaleY;
				a = (za * la + zb * lc) * sx;
				b = (za * lb + zb * ld) * sx;
				c = (zc * la + zd * lc) * sy;
				d = (zc * lb + zd * ld) * sy;
		}
	}

	/** Computes the applied transform values from the world transform.
	 *
	 * If the world transform is modified (by a constraint, rotateWorld(), etc) then this method should be called so
	 * the applied transform matches the world transform. The applied transform may be needed by other code (eg to apply another
	 * constraint).
	 *
	 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. The applied transform after
	 * calling this method is equivalent to the local transform used to compute the world transform, but may not be identical. */
	public function updateLocalTransform(skeleton:Skeleton):Void {
		local = 0;
		world = skeleton._update;

		var sx = skeleton.scaleX, sy = skeleton.scaleY;
		if (bone.parent == null) {
			var sxi = 1 / sx, syi = 1 / sy;
			x = (worldX - skeleton.x) * sxi;
			y = (worldY - skeleton.y) * syi;
			set5(a * sxi, b * sxi, c * syi, d * syi, 0);
			return;
		}

		var parent = bone.parent.appliedPose;
		var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		var pad = pa * pd - pb * pc, pid = 1 / pad;
		var ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
		var dx = worldX - parent.worldX, dy = worldY - parent.worldY;
		x = dx * ia - dy * ib;
		y = dy * id - dx * ic;

		switch (inherit) {
			case Inherit.normal:
				set5(ia * a - ib * c, ia * b - ib * d, id * c - ic * a, id * d - ic * b, 0);
			case Inherit.onlyTranslation:
				var sxi = 1 / sx, syi = 1 / sy;
				set5(a * sxi, b * sxi, c * syi, d * syi, 0);
			case Inherit.noRotationOrReflection:
				var sxi = 1 / sx, syi = 1 / sy;
				pa *= sxi;
				pc *= syi;
				var wa = a * sxi, wb = b * sxi, wc = c * syi, wd = d * syi;
				var s = 1 / (pa * pa + pc * pc),
					det = 1 / Math.abs(pad * sxi * syi);
				set5((pa * wa + pc * wc) * s, (pa * wb + pc * wd) * s, (pa * wc - pc * wa) * det, (pa * wd - pc * wb) * det, MathUtils.atan2Deg(pc, pa));
			case Inherit.noScale, Inherit.noScaleOrReflection:
				var sxi = 1 / sx, syi = 1 / sy;
				var wa = a * sxi, wb = b * sxi, wc = c * syi, wd = d * syi;
				var tx = pd * a - pb * c, ty = pa * c - pc * a;
				if (pad < 0) {
					tx = -tx;
					ty = -ty;
				}
				var r = MathUtils.atan2Deg(ty, tx);
				rotation = r;
				r *= MathUtils.degRad;
				var cos = Math.cos(r), sin = Math.sin(r);
				var za = (pa * cos + pb * sin) * sxi;
				var zc = (pc * cos + pd * sin) * syi;
				var s = 1 / Math.sqrt(za * za + zc * zc);
				za *= s;
				zc *= s;
				var si = inherit == Inherit.noScale && ((pad < 0) != ((sx < 0) != (sy < 0))) ? -1 : 1;
				set4(za * wa + zc * wc, za * wb + zc * wd, (za * wc - zc * wa) * si, (za * wd - zc * wb) * si);
		}
	}

	private function set4(ra:Float, rb:Float, rc:Float, rd:Float):Void {
		var x = ra * ra + rc * rc, y = rb * rb + rd * rd;
		if (x > MathUtils.epsilon2) {
			shearX = MathUtils.atan2Deg(rc, ra);
			scaleX = Math.sqrt(x);
		} else {
			shearX = 0;
			scaleX = 0;
		}
		scaleY = Math.sqrt(y);
		if (y > MathUtils.epsilon2) {
			shearY = MathUtils.atan2Deg(rd, rb);
			if (ra * rd - rb * rc < 0) {
				scaleY = -scaleY;
				shearY += 90;
			} else
				shearY -= 90;
			if (shearY > 180)
				shearY -= 360;
			else if (shearY <= -180) //
				shearY += 360;
		} else
			shearY = 0;
	}

	private function set5(ra:Float, rb:Float, rc:Float, rd:Float, ro:Float):Void {
		shearX = 0;
		var x = ra * ra + rc * rc, y = rb * rb + rd * rd;
		if (x > MathUtils.epsilon2) {
			var r = MathUtils.atan2Deg(rc, ra);
			rotation = r + ro;
			scaleX = Math.sqrt(x);
			scaleY = Math.sqrt(y);
			if (y > MathUtils.epsilon2) {
				shearY = MathUtils.atan2Deg(rd, rb);
				if (ra * rd - rb * rc < 0) {
					scaleY = -scaleY;
					shearY += 90 - r;
				} else
					shearY -= 90 + r;
				if (shearY > 180)
					shearY -= 360;
				else if (shearY <= -180) //
					shearY += 360;
			} else
				shearY = 0;
		} else {
			scaleX = 0;
			scaleY = Math.sqrt(y);
			shearY = 0;
			rotation = y > MathUtils.epsilon2 ? MathUtils.atan2Deg(rd, rb) - 90 + ro : ro;
		}
	}

	/** If the world transform has been modified and the local transform no longer matches, updateLocalTransform(Skeleton)
	 * is called. */
	public function validateLocalTransform(skeleton:Skeleton) {
		if (local == skeleton._update)
			updateLocalTransform(skeleton);
	}

	public function modifyLocal(skeleton:Skeleton) {
		if (local == skeleton._update)
			updateLocalTransform(skeleton);
		world = 0;
		resetWorld(skeleton._update);
	}

	public function modifyWorld(update:Int) {
		local = update;
		world = update;
		resetWorld(update);
	}

	private function resetWorld(update:Int) {
		var children = bone.children;
		for (i in 0...bone.children.length) {
			var child = children[i].appliedPose;
			if (child.world == update) {
				child.world = 0;
				child.local = 0;
				child.resetWorld(update);
			}
		}
	}

	/** The world rotation for the X axis, calculated using a and c. */
	public var worldRotationX(get, never):Float;

	private function get_worldRotationX():Float {
		return MathUtils.atan2Deg(c, a);
	}

	/** The world rotation for the Y axis, calculated using b and d. */
	public var worldRotationY(get, never):Float;

	private function get_worldRotationY():Float {
		return MathUtils.atan2Deg(d, b);
	}

	/** The magnitude (always positive) of the world scale X, calculated using a and c. */
	public var worldScaleX(get, never):Float;

	private function get_worldScaleX():Float {
		return Math.sqrt(a * a + c * c);
	}

	/** The magnitude (always positive) of the world scale Y, calculated using b and d. */
	public var worldScaleY(get, never):Float;

	private function get_worldScaleY():Float {
		return Math.sqrt(b * b + d * d);
	}

	/** Transforms a point from world coordinates to the bone's local coordinates. */
	public function worldToLocal(world:Array<Float>):Array<Float> {
		var a:Float = a, b:Float = b, c:Float = c, d:Float = d;
		var invDet:Float = 1 / (a * d - b * c);
		var x:Float = world[0] - worldX, y:Float = world[1] - worldY;
		world[0] = (x * d * invDet - y * b * invDet);
		world[1] = (y * a * invDet - x * c * invDet);
		return world;
	}

	/** Transforms a point from the bone's local coordinates to world coordinates. */
	public function localToWorld(local:Array<Float>):Array<Float> {
		var localX:Float = local[0], localY:Float = local[1];
		local[0] = localX * a + localY * b + worldX;
		local[1] = localX * c + localY * d + worldY;
		return local;
	}

	/** Transforms a point from world coordinates to the parent bone's local coordinates. */
	public function worldToParent(world:Array<Float>):Array<Float> {
		if (world == null)
			throw new SpineException("world cannot be null.");
		return bone.parent == null ? world : bone.parent.appliedPose.worldToLocal(world);
	}

	/** Transforms a point from the parent bone's coordinates to world coordinates. */
	public function parentToWorld(world:Array<Float>):Array<Float> {
		if (world == null)
			throw new SpineException("world cannot be null.");
		return bone.parent == null ? world : bone.parent.appliedPose.localToWorld(world);
	}

	/** Transforms a world rotation to a local rotation. */
	public function worldToLocalRotation(worldRotation:Float):Float {
		var sin:Float = MathUtils.sinDeg(worldRotation),
			cos:Float = MathUtils.cosDeg(worldRotation);
		return Math.atan2(a * sin - c * cos, d * cos - b * sin) * MathUtils.radDeg + rotation - shearX;
	}

	/** Transforms a local rotation to a world rotation. */
	public function localToWorldRotation(localRotation:Float):Float {
		localRotation -= rotation + shearX;
		var sin:Float = MathUtils.sinDeg(localRotation),
			cos:Float = MathUtils.cosDeg(localRotation);
		return Math.atan2(cos * c + sin * d, cos * a + sin * b) * MathUtils.radDeg;
	}

	/** Rotates the world transform the specified amount.
	 *
	 * After changes are made to the world transform, updateAppliedTransform() should be called and
	 * update() will need to be called on any child bones, recursively. */
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
		return bone.data.name;
	}
}
