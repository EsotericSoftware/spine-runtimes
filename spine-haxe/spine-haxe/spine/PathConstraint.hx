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

import spine.Skeleton;
import spine.attachments.Attachment;
import spine.attachments.PathAttachment;

/** Stores the current pose for a path constraint. A path constraint adjusts the rotation, translation, and scale of the
 * constrained bones so they follow a PathAttachment.
 *
 * @see https://esotericsoftware.com/spine-path-constraints Path constraints in the Spine User Guide */
class PathConstraint extends Constraint<PathConstraint, PathConstraintData, PathConstraintPose> {
	private static inline var NONE = -1;
	private static inline var BEFORE = -2;
	private static inline var AFTER = -3;
	private static inline var epsilon = 0.00001;

	/** The bones that will be modified by this path constraint. */
	public final bones:Array<BonePose>;

	/** The slot whose path attachment will be used to constrained the bones. */
	public var slot:Slot;

	private final spaces = new Array<Float>();
	private final positions = new Array<Float>();
	private final world = new Array<Float>();
	private final curves = new Array<Float>();
	private final lengths = new Array<Float>();
	private final segments = new Array<Float>();

	public function new(data:PathConstraintData, skeleton:Skeleton) {
		super(data, new PathConstraintPose(), new PathConstraintPose());
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");

		bones = new Array<BonePose>();
		for (boneData in data.bones)
			bones.push(skeleton.bones[boneData.index].constrainedPose);

		slot = skeleton.slots[data.slot.index];
	}

	public function copy(skeleton:Skeleton) {
		var copy = new PathConstraint(data, skeleton);
		copy.pose.set(pose);
		return copy;
	}

	/** Applies the constraint to the constrained bones. */
	public function update(skeleton:Skeleton, physics:Physics):Void {
		var attachment = slot.appliedPose.attachment;
		if (!Std.isOfType(attachment, PathAttachment))
			return;
		var pathAttachment = cast(attachment, PathAttachment);

		var p = appliedPose;
		var mixRotate = p.mixRotate, mixX = p.mixX, mixY = p.mixY;
		if (mixRotate == 0 && mixX == 0 && mixY == 0)
			return;

		var data = data;
		var fTangents = data.rotateMode == RotateMode.tangent,
			fScale = data.rotateMode == RotateMode.chainScale;
		var boneCount = bones.length,
			spacesCount = fTangents ? boneCount : boneCount + 1;
		ArrayUtils.resize(spaces, spacesCount, 0);
		if (fScale)
			ArrayUtils.resize(lengths, boneCount, 0);
		var spacing = p.spacing;

		var bones = bones;
		switch (data.spacingMode) {
			case SpacingMode.percent:
				if (fScale) {
					var n = spacesCount - 1;
					for (i in 0...n) {
						var bone = bones[i];
						var setupLength:Float = bone.bone.data.length;
						var x = setupLength * bone.a, y = setupLength * bone.c;
						lengths[i] = Math.sqrt(x * x + y * y);
					}
				}
				for (i in 1...spacesCount)
					spaces[i] = spacing;
			case SpacingMode.proportional:
				var sum = 0.;
				var i = 0, n = spacesCount - 1;
				while (i < n) {
					var bone = bones[i];
					var setupLength:Float = bone.bone.data.length;
					if (setupLength < PathConstraint.epsilon) {
						if (fScale)
							lengths[i] = 0;
						spaces[++i] = spacing;
					} else {
						var x = setupLength * bone.a, y = setupLength * bone.c;
						var length = Math.sqrt(x * x + y * y);
						if (fScale)
							lengths[i] = length;
						spaces[++i] = length;
						sum += length;
					}
				}
				if (sum > 0) {
					sum = spacesCount / sum * spacing;
					for (i in 1...spacesCount)
						spaces[i] *= sum;
				}
			default:
				var lengthSpacing = data.spacingMode == SpacingMode.length;
				var i = 0, n = spacesCount - 1;
				while (i < n) {
					var bone = bones[i];
					var setupLength = bone.bone.data.length;
					if (setupLength < PathConstraint.epsilon) {
						if (fScale)
							lengths[i] = 0;
						spaces[++i] = spacing;
					} else {
						var x = setupLength * bone.a, y = setupLength * bone.c;
						var length = Math.sqrt(x * x + y * y);
						if (fScale)
							lengths[i] = length;
						spaces[++i] = (lengthSpacing ? Math.max(0, setupLength + spacing) : spacing) * length / setupLength;
					}
				}
		}

		var positions = computeWorldPositions(skeleton, pathAttachment, spacesCount, fTangents);
		var boneX = positions[0],
			boneY = positions[1],
			offsetRotation = data.offsetRotation;
		var tip = false;
		if (offsetRotation == 0)
			tip = data.rotateMode == RotateMode.chain;
		else {
			tip = false;
			var bone = slot.bone.appliedPose;
			offsetRotation *= bone.a * bone.d - bone.b * bone.c > 0 ? MathUtils.degRad : -MathUtils.degRad;
		}
		var i = 0, ip = 3, u = skeleton._update;
		while (i < boneCount) {
			var bone = bones[i];
			bone.worldX += (boneX - bone.worldX) * mixX;
			bone.worldY += (boneY - bone.worldY) * mixY;
			var x = positions[ip], y = positions[ip + 1], dx = x - boneX, dy = y - boneY;
			if (fScale) {
				var length = lengths[i];
				if (length >= epsilon) {
					var s = (Math.sqrt(dx * dx + dy * dy) / length - 1) * mixRotate + 1;
					bone.a *= s;
					bone.c *= s;
				}
			}
			boneX = x;
			boneY = y;
			if (mixRotate > 0) {
				var a = bone.a, b = bone.b, c = bone.c, d = bone.d, r:Float, cos:Float, sin:Float;
				if (fTangents)
					r = positions[ip - 1];
				else if (spaces[i + 1] < epsilon)
					r = positions[ip + 2];
				else
					r = Math.atan2(dy, dx);
				r -= Math.atan2(c, a);
				if (tip) {
					cos = Math.cos(r);
					sin = Math.sin(r);
					var length = bone.bone.data.length;
					boneX += (length * (cos * a - sin * c) - dx) * mixRotate;
					boneY += (length * (sin * a + cos * c) - dy) * mixRotate;
				} else
					r += offsetRotation;
				if (r > Math.PI)
					r -= (Math.PI * 2);
				else if (r < -Math.PI) //
					r += (Math.PI * 2);
				r *= mixRotate;
				cos = Math.cos(r);
				sin = Math.sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}
			bone.modifyWorld(u);
			i++;
			ip += 3;
		}
	}

	private function computeWorldPositions(skeleton:Skeleton, path:PathAttachment, spacesCount:Int, tangents:Bool):Array<Float> {
		var position = appliedPose.position;
		ArrayUtils.resize(positions, spacesCount * 3 + 2, 0);
		var out:Array<Float> = positions, world = new Array<Float>();
		var closed = path.closed;
		var verticesLength = path.worldVerticesLength,
			curveCount = Std.int(verticesLength / 6),
			prevCurve = NONE;

		if (!path.constantSpeed) {
			var lengths = path.lengths;
			curveCount -= closed ? 1 : 2;
			var pathLength = lengths[curveCount];

			if (data.positionMode == PositionMode.percent)
				position *= pathLength;

			var multiplier:Float;
			switch (data.spacingMode) {
				case SpacingMode.percent:
					multiplier = pathLength;
				case SpacingMode.proportional:
					multiplier = pathLength / spacesCount;
				default:
					multiplier = 1;
			}

			ArrayUtils.resize(world, 8, 0);
			var i = 0, o = 0, curve = 0;
			while (i < spacesCount) {
				var space = spaces[i] * multiplier;
				position += space;
				var p = position;

				if (closed) {
					p %= pathLength;
					if (p < 0)
						p += pathLength;
					curve = 0;
				} else if (p < 0) {
					if (prevCurve != BEFORE) {
						prevCurve = BEFORE;
						path.computeWorldVertices(skeleton, slot, 2, 4, world, 0, 2);
					}
					addBeforePosition(p, world, 0, out, o);
					continue;
				} else if (p > pathLength) {
					if (prevCurve != AFTER) {
						prevCurve = AFTER;
						path.computeWorldVertices(skeleton, slot, verticesLength - 6, 4, world, 0, 2);
					}
					addAfterPosition(p - pathLength, world, 0, out, o);
					continue;
				}

				// Determine curve containing position.
				while (true) {
					var length = lengths[curve];
					if (p > length) {
						curve++;
						continue;
					}
					if (curve == 0)
						p /= length;
					else {
						var prev = lengths[curve - 1];
						p = (p - prev) / (length - prev);
					}
					break;
				}
				if (curve != prevCurve) {
					prevCurve = curve;
					if (closed && curve == curveCount) {
						path.computeWorldVertices(skeleton, slot, verticesLength - 4, 4, world, 0, 2);
						path.computeWorldVertices(skeleton, slot, 0, 4, world, 4, 2);
					} else {
						path.computeWorldVertices(skeleton, slot, curve * 6 + 2, 8, world, 0, 2);
					}
				}
				addCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], out, o, tangents || (i > 0 && space == 0));
				i++;
				o += 3;
			}
			return out;
		}

		// World vertices.
		if (closed) {
			verticesLength += 2;
			ArrayUtils.resize(world, verticesLength, 0);
			path.computeWorldVertices(skeleton, slot, 2, verticesLength - 4, world, 0, 2);
			path.computeWorldVertices(skeleton, slot, 0, 2, world, verticesLength - 4, 2);
			world[verticesLength - 2] = world[0];
			world[verticesLength - 1] = world[1];
		} else {
			curveCount--;
			verticesLength -= 4;
			ArrayUtils.resize(world, verticesLength, 0);
			path.computeWorldVertices(skeleton, slot, 2, verticesLength, world, 0, 2);
		}

		// Curve lengths.
		ArrayUtils.resize(curves, curveCount, 0);
		var curves:Array<Float> = curves;
		var pathLength:Float = 0;
		var x1 = world[0], y1 = world[1], cx1 = 0., cy1 = 0., cx2 = 0., cy2 = 0., x2 = 0., y2 = 0.;
		var tmpx:Float, tmpy:Float, dddfx:Float, dddfy:Float, ddfx:Float, ddfy:Float, dfx:Float, dfy:Float;
		var i = 0, w = 2;
		while (i < curveCount) {
			cx1 = world[w];
			cy1 = world[w + 1];
			cx2 = world[w + 2];
			cy2 = world[w + 3];
			x2 = world[w + 4];
			y2 = world[w + 5];
			tmpx = (x1 - cx1 * 2 + cx2) * 0.1875;
			tmpy = (y1 - cy1 * 2 + cy2) * 0.1875;
			dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.09375;
			dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.09375;
			ddfx = tmpx * 2 + dddfx;
			ddfy = tmpy * 2 + dddfy;
			dfx = (cx1 - x1) * 0.75 + tmpx + dddfx * 0.16666667;
			dfy = (cy1 - y1) * 0.75 + tmpy + dddfy * 0.16666667;
			pathLength += Math.sqrt(dfx * dfx + dfy * dfy);
			dfx += ddfx;
			dfy += ddfy;
			ddfx += dddfx;
			ddfy += dddfy;
			pathLength += Math.sqrt(dfx * dfx + dfy * dfy);
			dfx += ddfx;
			dfy += ddfy;
			pathLength += Math.sqrt(dfx * dfx + dfy * dfy);
			dfx += ddfx + dddfx;
			dfy += ddfy + dddfy;
			pathLength += Math.sqrt(dfx * dfx + dfy * dfy);
			curves[i] = pathLength;
			x1 = x2;
			y1 = y2;

			i++;
			w += 6;
		}

		if (data.positionMode == PositionMode.percent)
			position *= pathLength;

		var multiplier:Float;
		switch (data.spacingMode) {
			case SpacingMode.percent:
				multiplier = pathLength;
			case SpacingMode.proportional:
				multiplier = pathLength / spacesCount;
			default:
				multiplier = 1;
		}

		var segments = segments;
		var curveLength = 0.;
		var i = 0, o = 0, curve = 0, segment = 0;
		while (i < spacesCount) {
			var space = spaces[i] * multiplier;
			position += space;
			var p = position;

			if (closed) {
				p %= pathLength;
				if (p < 0)
					p += pathLength;
				curve = 0;
				segment = 0;
			} else if (p < 0) {
				addBeforePosition(p, world, 0, out, o);
				i++;
				o += 3;
				continue;
			} else if (p > pathLength) {
				addAfterPosition(p - pathLength, world, verticesLength - 4, out, o);
				i++;
				o += 3;
				continue;
			}

			// Determine curve containing position.
			var curve = 0;
			while (true) {
				var length = curves[curve];
				if (p > length) {
					curve++;
					continue;
				}
				if (curve == 0) {
					p /= length;
				} else {
					var prev = curves[curve - 1];
					p = (p - prev) / (length - prev);
				}
				break;
			}

			// Curve segment lengths.
			if (curve != prevCurve) {
				prevCurve = curve;
				var ii:Int = curve * 6;
				x1 = world[ii];
				y1 = world[ii + 1];
				cx1 = world[ii + 2];
				cy1 = world[ii + 3];
				cx2 = world[ii + 4];
				cy2 = world[ii + 5];
				x2 = world[ii + 6];
				y2 = world[ii + 7];
				tmpx = (x1 - cx1 * 2 + cx2) * 0.03;
				tmpy = (y1 - cy1 * 2 + cy2) * 0.03;
				dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006;
				dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006;
				ddfx = tmpx * 2 + dddfx;
				ddfy = tmpy * 2 + dddfy;
				dfx = (cx1 - x1) * 0.3 + tmpx + dddfx * 0.16666667;
				dfy = (cy1 - y1) * 0.3 + tmpy + dddfy * 0.16666667;
				curveLength = Math.sqrt(dfx * dfx + dfy * dfy);
				segments[0] = curveLength;
				for (ii in 1...8) {
					dfx += ddfx;
					dfy += ddfy;
					ddfx += dddfx;
					ddfy += dddfy;
					curveLength += Math.sqrt(dfx * dfx + dfy * dfy);
					segments[ii] = curveLength;
				}
				dfx += ddfx;
				dfy += ddfy;
				curveLength += Math.sqrt(dfx * dfx + dfy * dfy);
				segments[8] = curveLength;
				dfx += ddfx + dddfx;
				dfy += ddfy + dddfy;
				curveLength += Math.sqrt(dfx * dfx + dfy * dfy);
				segments[9] = curveLength;
				segment = 0;
			}

			// Weight by segment length.
			p *= curveLength;
			while (true) {
				var length = segments[segment];
				if (p > length) {
					segment++;
					continue;
				}
				if (segment == 0)
					p /= length;
				else {
					var prev = segments[segment - 1];
					p = segment + (p - prev) / (length - prev);
				}
				break;
			}
			addCurvePosition(p * 0.1, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents || (i > 0 && space == 0));

			i++;
			o += 3;
		}
		return out;
	}

	private function addBeforePosition(p:Float, temp:Array<Float>, i:Int, out:Array<Float>, o:Int):Void {
		var x1:Float = temp[i];
		var y1:Float = temp[i + 1];
		var dx:Float = temp[i + 2] - x1;
		var dy:Float = temp[i + 3] - y1;
		var r:Float = Math.atan2(dy, dx);
		out[o] = x1 + p * Math.cos(r);
		out[o + 1] = y1 + p * Math.sin(r);
		out[o + 2] = r;
	}

	private function addAfterPosition(p:Float, temp:Array<Float>, i:Int, out:Array<Float>, o:Int):Void {
		var x1:Float = temp[i + 2];
		var y1:Float = temp[i + 3];
		var dx:Float = x1 - temp[i];
		var dy:Float = y1 - temp[i + 1];
		var r:Float = Math.atan2(dy, dx);
		out[o] = x1 + p * Math.cos(r);
		out[o + 1] = y1 + p * Math.sin(r);
		out[o + 2] = r;
	}

	private function addCurvePosition(p:Float, x1:Float, y1:Float, cx1:Float, cy1:Float, cx2:Float, cy2:Float, x2:Float, y2:Float, out:Array<Float>, o:Int,
			tangents:Bool):Void {
		if (p == 0 || Math.isNaN(p)) {
			out[o] = x1;
			out[o + 1] = y1;
			out[o + 2] = Math.atan2(cy1 - y1, cx1 - x1);
			return;
		}
		var tt:Float = p * p;
		var ttt:Float = tt * p;
		var u:Float = 1 - p;
		var uu:Float = u * u;
		var uuu:Float = uu * u;
		var ut:Float = u * p;
		var ut3:Float = ut * 3;
		var uut3:Float = u * ut3;
		var utt3:Float = ut3 * p;
		var x:Float = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt,
			y:Float = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
		out[o] = x;
		out[o + 1] = y;
		if (tangents) {
			if (p < 0.001) {
				out[o + 2] = Math.atan2(cy1 - y1, cx1 - x1);
			} else {
				out[o + 2] = Math.atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
			}
		}
	}

	public function sort(skeleton:Skeleton) {
		var slotIndex = slot.data.index;
		var slotBone = slot.bone;
		if (skeleton.skin != null)
			sortPathSlot(skeleton, skeleton.skin, slotIndex, slotBone);
		if (skeleton.data.defaultSkin != null && skeleton.data.defaultSkin != skeleton.skin)
			sortPathSlot(skeleton, skeleton.data.defaultSkin, slotIndex, slotBone);
		sortPath(skeleton, slot.pose.attachment, slotBone);
		var boneCount = bones.length;
		for (i in 0...boneCount) {
			var bone = bones[i].bone;
			skeleton.sortBone(bone);
			skeleton.constrained(bone);
		}
		skeleton._updateCache.push(this);
		for (i in 0...boneCount)
			skeleton.sortReset(bones[i].bone.children);
		for (i in 0...boneCount)
			bones[i].bone.sorted = true;
	}

	public function sortPathSlot(skeleton:Skeleton, skin:Skin, slotIndex:Int, slotBone:Bone) {
		var entries = skin.getAttachments();
		for (entry in entries) {
			if (entry.slotIndex == slotIndex)
				sortPath(skeleton, entry.attachment, slotBone);
		}
	}

	private function sortPath(skeleton:Skeleton, attachment:Attachment, slotBone:Bone) {
		if (!(Std.isOfType(attachment, PathAttachment)))
			return;
		var pathBones = cast(attachment, PathAttachment).bones;
		if (pathBones == null)
			skeleton.sortBone(slotBone);
		else {
			var bones = skeleton.bones;
			var i = 0, n = pathBones.length;
			while (i < n) {
				var nn = pathBones[i++];
				nn += i;
				while (i < nn)
					skeleton.sortBone(bones[pathBones[i++]]);
			}
		}
	}

	override public function isSourceActive():Bool {
		return slot.bone.active;
	}
}
