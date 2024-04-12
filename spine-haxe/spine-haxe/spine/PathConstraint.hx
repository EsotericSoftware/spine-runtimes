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

import spine.attachments.PathAttachment;

class PathConstraint implements Updatable {
	private static inline var NONE:Int = -1;
	private static inline var BEFORE:Int = -2;
	private static inline var AFTER:Int = -3;
	private static inline var epsilon:Float = 0.00001;

	private var _data:PathConstraintData;
	private var _bones:Array<Bone>;

	public var target:Slot;
	public var position:Float = 0;
	public var spacing:Float = 0;
	public var mixRotate:Float = 0;
	public var mixX:Float = 0;
	public var mixY:Float = 0;

	private var _spaces(default, never):Array<Float> = new Array<Float>();
	private var _positions(default, never):Array<Float> = new Array<Float>();
	private var _world(default, never):Array<Float> = new Array<Float>();
	private var _curves(default, never):Array<Float> = new Array<Float>();
	private var _lengths(default, never):Array<Float> = new Array<Float>();
	private var _segments(default, never):Array<Float> = new Array<Float>();

	public var active:Bool = false;

	public function new(data:PathConstraintData, skeleton:Skeleton) {
		if (data == null)
			throw new SpineException("data cannot be null.");
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");
		_data = data;
		_bones = new Array<Bone>();
		for (boneData in data.bones) {
			_bones.push(skeleton.findBone(boneData.name));
		}
		target = skeleton.findSlot(data.target.name);
		position = data.position;
		spacing = data.spacing;
		mixRotate = data.mixRotate;
		mixX = data.mixX;
		mixY = data.mixY;
	}

	public function isActive():Bool {
		return active;
	}

	public function setToSetupPose () {
		var data:PathConstraintData = _data;
		position = data.position;
		spacing = data.spacing;
		mixRotate = data.mixRotate;
		mixX = data.mixX;
		mixY = data.mixY;
	}

	public function update(physics:Physics):Void {
		var attachment:PathAttachment = cast(target.attachment, PathAttachment);
		if (attachment == null)
			return;
		if (mixRotate == 0 && mixX == 0 && mixY == 0)
			return;

		var data:PathConstraintData = _data;
		var fTangents:Bool = data.rotateMode == RotateMode.tangent,
			fScale:Bool = data.rotateMode == RotateMode.chainScale;
		var boneCount:Int = _bones.length;
		var spacesCount:Int = fTangents ? boneCount : boneCount + 1;
		ArrayUtils.resize(_spaces, spacesCount, 0);
		if (fScale) {
			ArrayUtils.resize(_lengths, boneCount, 0);
		}

		var bones:Array<Bone> = _bones;

		var i:Int,
			n:Int,
			bone:Bone,
			setupLength:Float,
			x:Float,
			y:Float,
			length:Float;
		switch (data.spacingMode) {
			case SpacingMode.percent:
				if (fScale) {
					n = spacesCount - 1;
					for (i in 0...n) {
						bone = bones[i];
						setupLength = bone.data.length;
						x = setupLength * bone.a;
						y = setupLength * bone.c;
						_lengths[i] = Math.sqrt(x * x + y * y);
					}
				}
				for (i in 1...spacesCount) {
					_spaces[i] = spacing;
				}
			case SpacingMode.proportional:
				var sum:Float = 0;
				i = 0;
				n = spacesCount - 1;
				while (i < n) {
					bone = bones[i];
					setupLength = bone.data.length;
					if (setupLength < PathConstraint.epsilon) {
						if (fScale)
							_lengths[i] = 0;
						_spaces[++i] = spacing;
					} else {
						x = setupLength * bone.a;
						y = setupLength * bone.c;
						length = Math.sqrt(x * x + y * y);
						if (fScale)
							_lengths[i] = length;
						_spaces[++i] = length;
						sum += length;
					}
				}
				if (sum > 0) {
					sum = spacesCount / sum * spacing;
					for (i in 1...spacesCount) {
						_spaces[i] *= sum;
					}
				}
			default:
				var lengthSpacing:Bool = data.spacingMode == SpacingMode.length;
				i = 0;
				n = spacesCount - 1;
				while (i < n) {
					bone = bones[i];
					setupLength = bone.data.length;
					if (setupLength < PathConstraint.epsilon) {
						if (fScale)
							_lengths[i] = 0;
						_spaces[++i] = spacing;
					} else {
						x = setupLength * bone.a;
						y = setupLength * bone.c;
						length = Math.sqrt(x * x + y * y);
						if (fScale)
							_lengths[i] = length;
						_spaces[++i] = (lengthSpacing ? setupLength + spacing : spacing) * length / setupLength;
					}
				}
		}

		var positions:Array<Float> = computeWorldPositions(attachment, spacesCount, fTangents);
		var boneX:Float = positions[0];
		var boneY:Float = positions[1];
		var offsetRotation:Float = data.offsetRotation;
		var tip:Bool = false;
		if (offsetRotation == 0) {
			tip = data.rotateMode == RotateMode.chain;
		} else {
			tip = false;
			var pa:Bone = target.bone;
			offsetRotation *= pa.a * pa.d - pa.b * pa.c > 0 ? MathUtils.degRad : -MathUtils.degRad;
		}

		i = 0;
		var p:Int = 3;
		while (i < boneCount) {
			var bone:Bone = bones[i];
			bone.worldX += (boneX - bone.worldX) * mixX;
			bone.worldY += (boneY - bone.worldY) * mixY;
			var x:Float = positions[p];
			var y:Float = positions[p + 1];
			var dx:Float = x - boneX;
			var dy:Float = y - boneY;
			if (fScale) {
				var length = _lengths[i];
				if (length != 0) {
					var s:Float = (Math.sqrt(dx * dx + dy * dy) / length - 1) * mixRotate + 1;
					bone.a *= s;
					bone.c *= s;
				}
			}
			boneX = x;
			boneY = y;
			if (mixRotate > 0) {
				var a:Float = bone.a,
					b:Float = bone.b,
					c:Float = bone.c,
					d:Float = bone.d,
					r:Float,
					cos:Float,
					sin:Float;
				if (fTangents) {
					r = positions[p - 1];
				} else if (_spaces[i + 1] == 0) {
					r = positions[p + 2];
				} else {
					r = Math.atan2(dy, dx);
				}
				r -= Math.atan2(c, a);
				if (tip) {
					cos = Math.cos(r);
					sin = Math.sin(r);
					var length:Float = bone.data.length;
					boneX += (length * (cos * a - sin * c) - dx) * mixRotate;
					boneY += (length * (sin * a + cos * c) - dy) * mixRotate;
				} else {
					r += offsetRotation;
				}
				if (r > Math.PI) {
					r -= (Math.PI * 2);
				} else if (r < -Math.PI) {
					r += (Math.PI * 2);
				}
				r *= mixRotate;
				cos = Math.cos(r);
				sin = Math.sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}
			bone.updateAppliedTransform();

			i++;
			p += 3;
		}
	}

	private function computeWorldPositions(path:PathAttachment, spacesCount:Int, tangents:Bool):Array<Float> {
		var position:Float = this.position;
		ArrayUtils.resize(_positions, spacesCount * 3 + 2, 0);
		var out:Array<Float> = _positions, world:Array<Float>;
		var closed:Bool = path.closed;
		var verticesLength:Int = path.worldVerticesLength;
		var curveCount:Int = Std.int(verticesLength / 6);
		var prevCurve:Int = NONE;
		var multiplier:Float, i:Int;

		if (!path.constantSpeed) {
			var lengths:Array<Float> = path.lengths;
			curveCount -= closed ? 1 : 2;
			var pathLength:Float = lengths[curveCount];
			if (data.positionMode == PositionMode.percent)
				position *= pathLength;
			switch (data.spacingMode) {
				case SpacingMode.percent:
					multiplier = pathLength;
				case SpacingMode.proportional:
					multiplier = pathLength / spacesCount;
				default:
					multiplier = 1;
			}

			ArrayUtils.resize(_world, 8, 0);
			world = _world;
			var i:Int = 0;
			var o:Int = 0;
			var curve:Int = 0;
			while (i < spacesCount) {
				var space:Float = _spaces[i] * multiplier;
				position += space;
				var p:Float = position;

				if (closed) {
					p %= pathLength;
					if (p < 0)
						p += pathLength;
					curve = 0;
				} else if (p < 0) {
					if (prevCurve != BEFORE) {
						prevCurve = BEFORE;
						path.computeWorldVertices(target, 2, 4, world, 0, 2);
					}
					addBeforePosition(p, world, 0, out, o);
					continue;
				} else if (p > pathLength) {
					if (prevCurve != AFTER) {
						prevCurve = AFTER;
						path.computeWorldVertices(target, verticesLength - 6, 4, world, 0, 2);
					}
					addAfterPosition(p - pathLength, world, 0, out, o);
					continue;
				}

				// Determine curve containing position.
				while (true) {
					var length:Float = lengths[curve];
					if (p > length) {
						curve++;
						continue;
					}
					if (curve == 0) {
						p /= length;
					} else {
						var prev:Float = lengths[curve - 1];
						p = (p - prev) / (length - prev);
					}
					break;
				}
				if (curve != prevCurve) {
					prevCurve = curve;
					if (closed && curve == curveCount) {
						path.computeWorldVertices(target, verticesLength - 4, 4, world, 0, 2);
						path.computeWorldVertices(target, 0, 4, world, 4, 2);
					} else {
						path.computeWorldVertices(target, curve * 6 + 2, 8, world, 0, 2);
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
			ArrayUtils.resize(_world, verticesLength, 0);
			world = _world;
			path.computeWorldVertices(target, 2, verticesLength - 4, world, 0, 2);
			path.computeWorldVertices(target, 0, 2, world, verticesLength - 4, 2);
			world[verticesLength - 2] = world[0];
			world[verticesLength - 1] = world[1];
		} else {
			curveCount--;
			verticesLength -= 4;
			ArrayUtils.resize(_world, verticesLength, 0);
			world = _world;
			path.computeWorldVertices(target, 2, verticesLength, world, 0, 2);
		}

		// Curve lengths.
		ArrayUtils.resize(_curves, curveCount, 0);
		var curves:Array<Float> = _curves;
		var pathLength:Float = 0;
		var x1:Float = world[0],
			y1:Float = world[1],
			cx1:Float = 0,
			cy1:Float = 0,
			cx2:Float = 0,
			cy2:Float = 0,
			x2:Float = 0,
			y2:Float = 0;
		var tmpx:Float, tmpy:Float, dddfx:Float, dddfy:Float, ddfx:Float, ddfy:Float, dfx:Float, dfy:Float;
		var i:Int = 0;
		var w:Int = 2;
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

		switch (data.spacingMode) {
			case SpacingMode.percent:
				multiplier = pathLength;
			case SpacingMode.proportional:
				multiplier = pathLength / spacesCount;
			default:
				multiplier = 1;
		}

		var segments:Array<Float> = _segments;
		var curveLength:Float = 0;
		var segment:Int;
		i = 0;
		var o:Int = 0;
		var segment:Int = 0;
		while (i < spacesCount) {
			var space = _spaces[i] * multiplier;
			position += space;
			var p = position;

			if (closed) {
				p %= pathLength;
				if (p < 0)
					p += pathLength;
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
				if (segment == 0) {
					p /= length;
				} else {
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

	public var bones(get, never):Array<Bone>;

	private function get_bones():Array<Bone> {
		return _bones;
	}

	public var data(get, never):PathConstraintData;

	private function get_data():PathConstraintData {
		return _data;
	}

	public function toString():String {
		return _data.name != null ? _data.name : "PathConstraint?";
	}
}
