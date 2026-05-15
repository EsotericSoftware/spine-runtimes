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
				return;
			case Inherit.onlyTranslation:
				var rx = (rotation + shearX) * MathUtils.degRad;
				var ry = (rotation + 90 + shearY) * MathUtils.degRad;
				a = Math.cos(rx) * scaleX;
				b = Math.cos(ry) * scaleY;
				c = Math.sin(rx) * scaleX;
				d = Math.sin(ry) * scaleY;
			case Inherit.noRotationOrReflection:
				var sx = 1 / skeleton.scaleX, sy = 1 / skeleton.scaleY;
				pa *= sx;
				pc *= sy;
				var s = pa * pa + pc * pc, prx:Float;
				if (s > 0.0001) {
					s = Math.abs(pa * pd * sy - pb * sx * pc) / s;
					pb = pc * s;
					pd = pa * s;
					prx = MathUtils.atan2Deg(pc, pa);
				} else {
					pa = 0;
					pc = 0;
					prx = 90 - MathUtils.atan2Deg(pd, pb);
				}
				var rx = (rotation + shearX - prx) * MathUtils.degRad;
				var ry = (rotation + shearY - prx + 90) * MathUtils.degRad;
				var la = Math.cos(rx) * scaleX;
				var lb = Math.cos(ry) * scaleY;
				var lc = Math.sin(rx) * scaleX;
				var ld = Math.sin(ry) * scaleY;
				a = pa * la - pb * lc;
				b = pa * lb - pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
			case Inherit.noScale, Inherit.noScaleOrReflection:
				var r = rotation * MathUtils.degRad,
					cos = Math.cos(r),
					sin = Math.sin(r);
				var za = (pa * cos + pb * sin) / skeleton.scaleX;
				var zc = (pc * cos + pd * sin) / skeleton.scaleY;
				var s = Math.sqrt(za * za + zc * zc);
				if (s > 0.00001)
					s = 1 / s;
				za *= s;
				zc *= s;
				s = Math.sqrt(za * za + zc * zc);
				if (inherit == Inherit.noScale && ((pa * pd - pb * pc < 0) != ((skeleton.scaleX < 0) != (skeleton.scaleY < 0))))
					s = -s;
				r = Math.PI / 2 + Math.atan2(zc, za);
				var zb:Float = Math.cos(r) * s;
				var zd:Float = Math.sin(r) * s;
				var rx = shearX * MathUtils.degRad;
				var ry = (90 + shearY) * MathUtils.degRad;
				var la = Math.cos(rx) * scaleX;
				var lb = Math.cos(ry) * scaleY;
				var lc = Math.sin(rx) * scaleX;
				var ld = Math.sin(ry) * scaleY;
				a = za * la + zb * lc;
				b = za * lb + zb * ld;
				c = zc * la + zd * lc;
				d = zc * lb + zd * ld;
		}
		a *= skeleton.scaleX;
		b *= skeleton.scaleX;
		c *= skeleton.scaleY;
		d *= skeleton.scaleY;
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

		if (bone.parent == null) {
			x = worldX - skeleton.x;
			y = worldY - skeleton.y;
			rotation = MathUtils.atan2Deg(c, a);
			scaleX = Math.sqrt(a * a + c * c);
			scaleY = Math.sqrt(b * b + d * d);
			shearX = 0;
			shearY = MathUtils.atan2Deg(a * b + c * d, a * d - b * c);
			return;
		}

		var parent = bone.parent.appliedPose;
		var pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		var pid:Float = 1 / (pa * pd - pb * pc);
		var ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
		var dx = worldX - parent.worldX, dy = worldY - parent.worldY;
		x = (dx * ia - dy * ib);
		y = (dy * id - dx * ic);

		var ra:Float, rb:Float, rc:Float, rd:Float;
		if (inherit == Inherit.onlyTranslation) {
			ra = a;
			rb = b;
			rc = c;
			rd = d;
		} else {
			switch (inherit) {
				case Inherit.noRotationOrReflection:
					var s = Math.abs(pa * pd - pb * pc) / (pa * pa + pc * pc);
					pb = -pc * skeleton.scaleX * s / skeleton.scaleY;
					pd = pa * skeleton.scaleY * s / skeleton.scaleX;
					pid = 1 / (pa * pd - pb * pc);
					ia = pd * pid;
					ib = pb * pid;
				case Inherit.noScale, Inherit.noScaleOrReflection:
					var r = rotation * MathUtils.degRad,
						cos = Math.cos(r),
						sin = Math.sin(r);
					pa = (pa * cos + pb * sin) / skeleton.scaleX;
					pc = (pc * cos + pd * sin) / skeleton.scaleY;
					var s = Math.sqrt(pa * pa + pc * pc);
					if (s > 0.00001)
						s = 1 / s;
					pa *= s;
					pc *= s;
					s = Math.sqrt(pa * pa + pc * pc);
					if (inherit == Inherit.noScale && (pid < 0 != ((skeleton.scaleX < 0) != (skeleton.scaleY < 0))))
						s = -s;
					r = MathUtils.PI / 2 + Math.atan2(pc, pa);
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

		shearX = 0;
		scaleX = Math.sqrt(ra * ra + rc * rc);
		if (scaleX > 0.0001) {
			var det = ra * rd - rb * rc;
			scaleY = det / scaleX;
			shearY = -MathUtils.atan2Deg(ra * rb + rc * rd, det);
			rotation = MathUtils.atan2Deg(rc, ra);
		} else {
			scaleX = 0;
			scaleY = Math.sqrt(rb * rb + rd * rd);
			shearY = 0;
			rotation = 90 - MathUtils.atan2Deg(rd, rb);
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
