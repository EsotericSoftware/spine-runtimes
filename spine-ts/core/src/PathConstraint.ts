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

module spine {
	export class PathConstraint implements Constraint {
		static NONE = -1; static BEFORE = -2; static AFTER = -3;

		data: PathConstraintData;
		bones: Array<Bone>;
		target: Slot;
		position = 0; spacing = 0; rotateMix = 0; translateMix = 0;

		spaces = new Array<number>(); positions = new Array<number>();
		world = new Array<number>(); curves = new Array<number>(); lengths = new Array<number>();
		segments = new Array<number>();

		constructor (data: PathConstraintData, skeleton: Skeleton) {
			if (data == null) throw new Error("data cannot be null.");
			if (skeleton == null) throw new Error("skeleton cannot be null.");
			this.data = data;
			this.bones = new Array<Bone>();
			for (let i = 0, n = data.bones.length; i < n; i++)
				this.bones.push(skeleton.findBone(data.bones[i].name));
			this.target = skeleton.findSlot(data.target.name);
			this.position = data.position;
			this.spacing = data.spacing;
			this.rotateMix = data.rotateMix;
			this.translateMix = data.translateMix;
		}

		apply () {
			this.update();
		}

		update () {
			let attachment = this.target.getAttachment();
			if (!(attachment instanceof PathAttachment)) return;

			let rotateMix = this.rotateMix, translateMix = this.translateMix;
			let translate = translateMix > 0, rotate = rotateMix > 0;
			if (!translate && !rotate) return;

			let data = this.data;
			let spacingMode = data.spacingMode;
			let lengthSpacing = spacingMode == SpacingMode.Length;
			let rotateMode = data.rotateMode;
			let tangents = rotateMode == RotateMode.Tangent, scale = rotateMode == RotateMode.ChainScale;
			let boneCount = this.bones.length, spacesCount = tangents ? boneCount : boneCount + 1;
			let bones = this.bones;
			let spaces = Utils.setArraySize(this.spaces, spacesCount), lengths: Array<number> = null;
			let spacing = this.spacing;
			if (scale || lengthSpacing) {
				if (scale) lengths = Utils.setArraySize(this.lengths, boneCount);
				for (let i = 0, n = spacesCount - 1; i < n;) {
					let bone = bones[i];
					let length = bone.data.length, x = length * bone.a, y = length * bone.c;
					length = Math.sqrt(x * x + y * y);
					if (scale) lengths[i] = length;
					spaces[++i] = lengthSpacing ? Math.max(0, length + spacing) : spacing;
				}
			} else {
				for (let i = 1; i < spacesCount; i++)
					spaces[i] = spacing;
			}

			let positions = this.computeWorldPositions(<PathAttachment>attachment, spacesCount, tangents,
				data.positionMode == PositionMode.Percent, spacingMode == SpacingMode.Percent);
			let boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
			let tip = false;
			if (offsetRotation == 0)
				tip = rotateMode == RotateMode.Chain;
			else {
				tip = false;
				let p = this.target.bone;
				offsetRotation *= p.a * p.d - p.b * p.c > 0 ? MathUtils.degRad : -MathUtils.degRad;
			}
			for (let i = 0, p = 3; i < boneCount; i++, p += 3) {
				let bone = bones[i];
				bone.worldX += (boneX - bone.worldX) * translateMix;
				bone.worldY += (boneY - bone.worldY) * translateMix;
				let x = positions[p], y = positions[p + 1], dx = x - boneX, dy = y - boneY;
				if (scale) {
					let length = lengths[i];
					if (length != 0) {
						let s = (Math.sqrt(dx * dx + dy * dy) / length - 1) * rotateMix + 1;
						bone.a *= s;
						bone.c *= s;
					}
				}
				boneX = x;
				boneY = y;
				if (rotate) {
					let a = bone.a, b = bone.b, c = bone.c, d = bone.d, r = 0, cos = 0, sin = 0;
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
						let length = bone.data.length;
						boneX += (length * (cos * a - sin * c) - dx) * rotateMix;
						boneY += (length * (sin * a + cos * c) - dy) * rotateMix;
					} else {
						r += offsetRotation;
					}
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) //
						r += MathUtils.PI2;
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

		computeWorldPositions (path: PathAttachment, spacesCount: number, tangents: boolean, percentPosition: boolean,
			percentSpacing: boolean) {
			let target = this.target;
			let position = this.position;
			let spaces = this.spaces, out = Utils.setArraySize(this.positions, spacesCount * 3 + 2), world: Array<number> = null;
			let closed = path.closed;
			let verticesLength = path.worldVerticesLength, curveCount = verticesLength / 6, prevCurve = PathConstraint.NONE;

			if (!path.constantSpeed) {
				let lengths = path.lengths;
				curveCount -= closed ? 1 : 2;
				let pathLength = lengths[curveCount];
				if (percentPosition) position *= pathLength;
				if (percentSpacing) {
					for (let i = 0; i < spacesCount; i++)
						spaces[i] *= pathLength;
				}
				world = Utils.setArraySize(this.world, 8);
				for (let i = 0, o = 0, curve = 0; i < spacesCount; i++, o += 3) {
					let space = spaces[i];
					position += space;
					let p = position;

					if (closed) {
						p %= pathLength;
						if (p < 0) p += pathLength;
						curve = 0;
					} else if (p < 0) {
						if (prevCurve != PathConstraint.BEFORE) {
							prevCurve = PathConstraint.BEFORE;
							path.computeWorldVerticesWith(target, 2, 4, world, 0);
						}
						this.addBeforePosition(p, world, 0, out, o);
						continue;
					} else if (p > pathLength) {
						if (prevCurve != PathConstraint.AFTER) {
							prevCurve = PathConstraint.AFTER;
							path.computeWorldVerticesWith(target, verticesLength - 6, 4, world, 0);
						}
						this.addAfterPosition(p - pathLength, world, 0, out, o);
						continue;
					}

					// Determine curve containing position.
					for (;; curve++) {
						let length = lengths[curve];
						if (p > length) continue;
						if (curve == 0)
							p /= length;
						else {
							let prev = lengths[curve - 1];
							p = (p - prev) / (length - prev);
						}
						break;
					}
					if (curve != prevCurve) {
						prevCurve = curve;
						if (closed && curve == curveCount) {
							path.computeWorldVerticesWith(target, verticesLength - 4, 4, world, 0);
							path.computeWorldVerticesWith(target, 0, 4, world, 4);
						} else
							path.computeWorldVerticesWith(target, curve * 6 + 2, 8, world, 0);
					}
					this.addCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], out, o,
						tangents || (i > 0 && space == 0));
				}
				return out;
			}

			// World vertices.
			if (closed) {
				verticesLength += 2;
				world = Utils.setArraySize(this.world, verticesLength);
				path.computeWorldVerticesWith(target, 2, verticesLength - 4, world, 0);
				path.computeWorldVerticesWith(target, 0, 2, world, verticesLength - 4);
				world[verticesLength - 2] = world[0];
				world[verticesLength - 1] = world[1];
			} else {
				curveCount--;
				verticesLength -= 4;
				world = Utils.setArraySize(this.world, verticesLength);
				path.computeWorldVerticesWith(target, 2, verticesLength, world, 0);
			}

			// Curve lengths.
			let curves = Utils.setArraySize(this.curves, curveCount);
			let pathLength = 0;
			let x1 = world[0], y1 = world[1], cx1 = 0, cy1 = 0, cx2 = 0, cy2 = 0, x2 = 0, y2 = 0;
			let tmpx = 0, tmpy = 0, dddfx = 0, dddfy = 0, ddfx = 0, ddfy = 0, dfx = 0, dfy = 0;
			for (let i = 0, w = 2; i < curveCount; i++, w += 6) {
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
				for (let i = 0; i < spacesCount; i++)
					spaces[i] *= pathLength;
			}

			let segments = this.segments;
			let curveLength = 0;
			for (let i = 0, o = 0, curve = 0, segment = 0; i < spacesCount; i++, o += 3) {
				let space = spaces[i];
				position += space;
				let p = position;

				if (closed) {
					p %= pathLength;
					if (p < 0) p += pathLength;
					curve = 0;
				} else if (p < 0) {
					this.addBeforePosition(p, world, 0, out, o);
					continue;
				} else if (p > pathLength) {
					this.addAfterPosition(p - pathLength, world, verticesLength - 4, out, o);
					continue;
				}

				// Determine curve containing position.
				for (;; curve++) {
					let length = curves[curve];
					if (p > length) continue;
					if (curve == 0)
						p /= length;
					else {
						let prev = curves[curve - 1];
						p = (p - prev) / (length - prev);
					}
					break;
				}

				// Curve segment lengths.
				if (curve != prevCurve) {
					prevCurve = curve;
					let ii = curve * 6;
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
					let length = segments[segment];
					if (p > length) continue;
					if (segment == 0)
						p /= length;
					else {
						let prev = segments[segment - 1];
						p = segment + (p - prev) / (length - prev);
					}
					break;
				}
				this.addCurvePosition(p * 0.1, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents || (i > 0 && space == 0));
			}
			return out;
		}

		addBeforePosition (p: number, temp: Array<number>, i: number, out: Array<number>, o: number) {
			let x1 = temp[i], y1 = temp[i + 1], dx = temp[i + 2] - x1, dy = temp[i + 3] - y1, r = Math.atan2(dy, dx);
			out[o] = x1 + p * Math.cos(r);
			out[o + 1] = y1 + p * Math.sin(r);
			out[o + 2] = r;
		}

		addAfterPosition (p: number, temp: Array<number>, i: number, out: Array<number>, o: number) {
			let x1 = temp[i + 2], y1 = temp[i + 3], dx = x1 - temp[i], dy = y1 - temp[i + 1], r = Math.atan2(dy, dx);
			out[o] = x1 + p * Math.cos(r);
			out[o + 1] = y1 + p * Math.sin(r);
			out[o + 2] = r;
		}

		addCurvePosition (p: number, x1: number, y1: number, cx1: number, cy1: number, cx2: number, cy2: number, x2: number, y2: number,
			out: Array<number>, o: number, tangents: boolean) {
			if (p == 0 || isNaN(p)) p = 0.0001;
			let tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
			let ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
			let x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
			out[o] = x;
			out[o + 1] = y;
			if (tangents) out[o + 2] = Math.atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
		}

		getOrder () {
			return this.data.order;
		}
	}
}
