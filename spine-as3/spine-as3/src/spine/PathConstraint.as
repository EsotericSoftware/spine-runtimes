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
	import spine.attachments.PathAttachment;

	public class PathConstraint implements Constraint {
		private static const NONE : int = -1, BEFORE : int = -2, AFTER : int = -3;
		internal var _data : PathConstraintData;
		internal var _bones : Vector.<Bone>;
		public var target : Slot;
		public var position : Number, spacing : Number, rotateMix : Number, translateMix : Number;
		internal const _spaces : Vector.<Number> = new Vector.<Number>();
		internal const _positions : Vector.<Number> = new Vector.<Number>();
		internal const _world : Vector.<Number> = new Vector.<Number>();
		internal const _curves : Vector.<Number> = new Vector.<Number>();
		internal const _lengths : Vector.<Number> = new Vector.<Number>();
		internal const _segments : Vector.<Number> = new Vector.<Number>(10);

		public function PathConstraint(data : PathConstraintData, skeleton : Skeleton) {
			if (data == null) throw new ArgumentError("data cannot be null.");
			if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");
			_data = data;
			_bones = new Vector.<Bone>();
			for each (var boneData : BoneData in data.bones)
				_bones.push(skeleton.findBone(boneData.name));
			target = skeleton.findSlot(data.target.name);
			position = data.position;
			spacing = data.spacing;
			rotateMix = data.rotateMix;
			translateMix = data.translateMix;
		}

		public function apply() : void {
			update();
		}

		public function update() : void {
			var attachment : PathAttachment = target.attachment as PathAttachment;
			if (attachment == null) return;

			var rotateMix : Number = this.rotateMix, translateMix : Number = this.translateMix;
			var translate : Boolean = translateMix > 0, rotate : Boolean = rotateMix > 0;
			if (!translate && !rotate) return;

			var data : PathConstraintData = this._data;
			var spacingMode : SpacingMode = data.spacingMode;
			var lengthSpacing : Boolean = spacingMode == SpacingMode.length;
			var rotateMode : RotateMode = data.rotateMode;
			var tangents : Boolean = rotateMode == RotateMode.tangent, scale : Boolean = rotateMode == RotateMode.chainScale;
			var boneCount : int = this._bones.length, spacesCount : int = tangents ? boneCount : boneCount + 1;
			var bones : Vector.<Bone> = this._bones;
			this._spaces.length = spacesCount;
			var spaces : Vector.<Number> = this._spaces, lengths : Vector.<Number> = null;
			var spacing : Number = this.spacing;
			if (scale || lengthSpacing) {
				if (scale) {
					this._lengths.length = boneCount;
					lengths = this._lengths;
				}
				for (var i : int = 0, n : int = spacesCount - 1; i < n;) {
					var bone : Bone = bones[i];
					var setupLength : Number = bone.data.length;
					if (setupLength == 0) setupLength = 0.000000001;
					var x : Number = setupLength * bone.a, y : Number = setupLength * bone.c;
					var length : Number = Math.sqrt(x * x + y * y);
					if (scale) lengths[i] = length;
					spaces[++i] = (lengthSpacing ? setupLength + spacing : spacing) * length / setupLength;
				}
			} else {
				for (i = 1; i < spacesCount; i++)
					spaces[i] = spacing;
			}

			var positions : Vector.<Number> = computeWorldPositions(attachment, spacesCount, tangents, data.positionMode == PositionMode.percent, spacingMode == SpacingMode.percent);
			var boneX : Number = positions[0], boneY : Number = positions[1], offsetRotation : Number = data.offsetRotation;
			var tip : Boolean = false;
			if (offsetRotation == 0)
				tip = rotateMode == RotateMode.chain;
			else {
				tip = false;
				var pa : Bone = target.bone;
				offsetRotation *= pa.a * pa.d - pa.b * pa.c > 0 ? MathUtils.degRad : -MathUtils.degRad;
			}
			var p : Number;
			for (i = 0, p = 3; i < boneCount; i++, p += 3) {
				bone = bones[i];
				bone.worldX += (boneX - bone.worldX) * translateMix;
				bone.worldY += (boneY - bone.worldY) * translateMix;
				x = positions[p];
				y = positions[p + 1];
				var dx : Number = x - boneX, dy : Number = y - boneY;
				if (scale) {
					length = lengths[i];
					if (length != 0) {
						var s : Number = (Math.sqrt(dx * dx + dy * dy) / length - 1) * rotateMix + 1;
						bone.a *= s;
						bone.c *= s;
					}
				}
				boneX = x;
				boneY = y;
				if (rotate) {
					var a : Number = bone.a, b : Number = bone.b, c : Number = bone.c, d : Number = bone.d, r : Number, cos : Number, sin : Number;
					if (tangents)
						r = positions[p - 1];
					else if (spaces[i + 1] == 0)
						r = positions[p + 2];
					else
						r = Math.atan2(dy, dx);
					r -= Math.atan2(c, a);
					if (tip) {
						cos = Math.cos(r);
						sin = Math.sin(r);
						length = bone.data.length;
						boneX += (length * (cos * a - sin * c) - dx) * rotateMix;
						boneY += (length * (sin * a + cos * c) - dy) * rotateMix;
					} else {
						r += offsetRotation;
					}
					if (r > Math.PI)
						r -= (Math.PI * 2);
					else if (r < -Math.PI) //
						r += (Math.PI * 2);
					r *= rotateMix;
					cos = Math.cos(r);
					sin = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}
				bone.appliedValid = false;
			}
		}

		protected function computeWorldPositions(path : PathAttachment, spacesCount : int, tangents : Boolean, percentPosition : Boolean, percentSpacing : Boolean) : Vector.<Number> {
			var target : Slot = this.target;
			var position : Number = this.position;
			var spaces : Vector.<Number> = this._spaces;
			this._positions.length = spacesCount * 3 + 2;
			var out : Vector.<Number> = this._positions, world : Vector.<Number>;
			var closed : Boolean = path.closed;
			var verticesLength : int = path.worldVerticesLength, curveCount : int = verticesLength / 6, prevCurve : int = NONE;

			if (!path.constantSpeed) {
				var lengths : Vector.<Number> = path.lengths;
				curveCount -= closed ? 1 : 2;
				var pathLength : Number = lengths[curveCount];
				if (percentPosition) position *= pathLength;
				if (percentSpacing) {
					for (var i : int = 0; i < spacesCount; i++)
						spaces[i] *= pathLength;
				}
				this._world.length = 8;
				world = this._world;
				var o : int, curve : int;
				for (i = 0, o = 0, curve = 0; i < spacesCount; i++, o += 3) {
					var space : Number = spaces[i];
					position += space;
					var p : Number = position;

					if (closed) {
						p %= pathLength;
						if (p < 0) p += pathLength;
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
					for (;; curve++) {
						var length : Number = lengths[curve];
						if (p > length) continue;
						if (curve == 0)
							p /= length;
						else {
							var prev : Number = lengths[curve - 1];
							p = (p - prev) / (length - prev);
						}
						break;
					}
					if (curve != prevCurve) {
						prevCurve = curve;
						if (closed && curve == curveCount) {
							path.computeWorldVertices(target, verticesLength - 4, 4, world, 0, 2);
							path.computeWorldVertices(target, 0, 4, world, 4, 2);
						} else
							path.computeWorldVertices(target, curve * 6 + 2, 8, world, 0, 2);
					}
					addCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], out, o, tangents || (i > 0 && space == 0));
				}
				return out;
			}

			// World vertices.
			if (closed) {
				verticesLength += 2;
				this._world.length = verticesLength;
				world = this._world;
				path.computeWorldVertices(target, 2, verticesLength - 4, world, 0, 2);
				path.computeWorldVertices(target, 0, 2, world, verticesLength - 4, 2);
				world[verticesLength - 2] = world[0];
				world[verticesLength - 1] = world[1];
			} else {
				curveCount--;
				verticesLength -= 4;
				this._world.length = verticesLength;
				world = this._world;
				path.computeWorldVertices(target, 2, verticesLength, world, 0, 2);
			}

			// Curve lengths.
			this._curves.length = curveCount;
			var curves : Vector.<Number> = this._curves;
			pathLength = 0;
			var x1 : Number = world[0], y1 : Number = world[1], cx1 : Number = 0, cy1 : Number = 0, cx2 : Number = 0, cy2 : Number = 0, x2 : Number = 0, y2 : Number = 0;
			var tmpx : Number, tmpy : Number, dddfx : Number, dddfy : Number, ddfx : Number, ddfy : Number, dfx : Number, dfy : Number;
			var w : int;
			for (i = 0, w = 2; i < curveCount; i++, w += 6) {
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
			}
			if (percentPosition) position *= pathLength;
			if (percentSpacing) {
				for (i = 0; i < spacesCount; i++)
					spaces[i] *= pathLength;
			}

			var segments : Vector.<Number> = this._segments;
			var curveLength : Number = 0;
			var segment : int;
			for (i = 0, o = 0, curve = 0, segment = 0; i < spacesCount; i++, o += 3) {
				space = spaces[i];
				position += space;
				p = position;

				if (closed) {
					p %= pathLength;
					if (p < 0) p += pathLength;
					curve = 0;
				} else if (p < 0) {
					addBeforePosition(p, world, 0, out, o);
					continue;
				} else if (p > pathLength) {
					addAfterPosition(p - pathLength, world, verticesLength - 4, out, o);
					continue;
				}

				// Determine curve containing position.
				for (;; curve++) {
					length = curves[curve];
					if (p > length) continue;
					if (curve == 0)
						p /= length;
					else {
						prev = curves[curve - 1];
						p = (p - prev) / (length - prev);
					}
					break;
				}

				// Curve segment lengths.
				if (curve != prevCurve) {
					prevCurve = curve;
					var ii : int = curve * 6;
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
					for (ii = 1; ii < 8; ii++) {
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
				for (;; segment++) {
					length = segments[segment];
					if (p > length) continue;
					if (segment == 0)
						p /= length;
					else {
						prev = segments[segment - 1];
						p = segment + (p - prev) / (length - prev);
					}
					break;
				}
				addCurvePosition(p * 0.1, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents || (i > 0 && space == 0));
			}
			return out;
		}

		private function addBeforePosition(p : Number, temp : Vector.<Number>, i : int, out : Vector.<Number>, o : int) : void {
			var x1 : Number = temp[i], y1 : Number = temp[i + 1], dx : Number = temp[i + 2] - x1, dy : Number = temp[i + 3] - y1, r : Number = Math.atan2(dy, dx);
			out[o] = x1 + p * Math.cos(r);
			out[o + 1] = y1 + p * Math.sin(r);
			out[o + 2] = r;
		}

		private function addAfterPosition(p : Number, temp : Vector.<Number>, i : int, out : Vector.<Number>, o : int) : void {
			var x1 : Number = temp[i + 2], y1 : Number = temp[i + 3], dx : Number = x1 - temp[i], dy : Number = y1 - temp[i + 1], r : Number = Math.atan2(dy, dx);
			out[o] = x1 + p * Math.cos(r);
			out[o + 1] = y1 + p * Math.sin(r);
			out[o + 2] = r;
		}

		private function addCurvePosition(p : Number, x1 : Number, y1 : Number, cx1 : Number, cy1 : Number, cx2 : Number, cy2 : Number, x2 : Number, y2 : Number, out : Vector.<Number>, o : int, tangents : Boolean) : void {
			if (p == 0 || isNaN(p)) p = 0.0001;
			var tt : Number = p * p, ttt : Number = tt * p, u : Number = 1 - p, uu : Number = u * u, uuu : Number = uu * u;
			var ut : Number = u * p, ut3 : Number = ut * 3, uut3 : Number = u * ut3, utt3 : Number = ut3 * p;
			var x : Number = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y : Number = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
			out[o] = x;
			out[o + 1] = y;
			if (tangents) out[o + 2] = Math.atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
		}

		public function get bones() : Vector.<Bone> {
			return _bones;
		}

		public function get data() : PathConstraintData {
			return _data;
		}

		public function getOrder() : Number {
			return _data.order;
		}

		public function toString() : String {
			return _data.name;
		}
	}
}