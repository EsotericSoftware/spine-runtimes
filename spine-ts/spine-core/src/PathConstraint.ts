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

import type { Attachment } from "./attachments/Attachment.js";
import { PathAttachment } from "./attachments/PathAttachment.js";
import type { Bone } from "./Bone.js";
import type { BonePose } from "./BonePose.js";
import { Constraint } from "./Constraint.js";
import { type PathConstraintData, PositionMode, RotateMode, SpacingMode } from "./PathConstraintData.js";
import { PathConstraintPose } from "./PathConstraintPose.js";
import type { Physics } from "./Physics.js";
import type { Skeleton } from "./Skeleton.js";
import type { Skin } from "./Skin.js";
import type { Slot } from "./Slot.js";
import { MathUtils, Utils } from "./Utils.js";


/** Stores the current pose for a path constraint. A path constraint adjusts the rotation, translation, and scale of the
 * constrained bones so they follow a {@link PathAttachment}.
 *
 * See [Path constraints](http://esotericsoftware.com/spine-path-constraints) in the Spine User Guide. */
export class PathConstraint extends Constraint<PathConstraint, PathConstraintData, PathConstraintPose> {
	static NONE = -1; static BEFORE = -2; static AFTER = -3;
	static epsilon = 0.00001;

	/** The path constraint's setup pose data. */
	data: PathConstraintData;

	/** The bones that will be modified by this path constraint. */
	bones: Array<BonePose>;

	/** The slot whose path attachment will be used to constrained the bones. */
	slot: Slot;

	spaces = [] as number[]; positions = [] as number[];
	world = [] as number[]; curves = [] as number[]; lengths = [] as number[];
	segments = [] as number[];

	constructor (data: PathConstraintData, skeleton: Skeleton) {
		super(data, new PathConstraintPose(), new PathConstraintPose());
		if (!skeleton) throw new Error("skeleton cannot be null.");
		this.data = data;

		this.bones = [] as BonePose[];
		for (const boneData of this.data.bones)
			this.bones.push(skeleton.bones[boneData.index].constrained);

		this.slot = skeleton.slots[data.slot.index];
	}

	public copy (skeleton: Skeleton) {
		var copy = new PathConstraint(this.data, skeleton);
		copy.pose.set(this.pose);
		return copy;
	}

	update (skeleton: Skeleton, physics: Physics) {
		const attachment = this.slot.applied.attachment;
		if (!(attachment instanceof PathAttachment)) return;

		const p = this.applied;
		const mixRotate = p.mixRotate, mixX = p.mixX, mixY = p.mixY;
		if (mixRotate === 0 && mixX === 0 && mixY === 0) return;

		const data = this.data;
		const tangents = data.rotateMode === RotateMode.Tangent, scale = data.rotateMode === RotateMode.ChainScale;

		const bones = this.bones;
		const boneCount = bones.length, spacesCount = tangents ? boneCount : boneCount + 1;
		const spaces = Utils.setArraySize(this.spaces, spacesCount), lengths: Array<number> = scale ? this.lengths = Utils.setArraySize(this.lengths, boneCount) : [];
		const spacing = p.spacing;

		switch (data.spacingMode) {
			case SpacingMode.Percent:
				if (scale) {
					for (let i = 0, n = spacesCount - 1; i < n; i++) {
						const bone = bones[i];
						const setupLength = bone.bone.data.length;
						const x = setupLength * bone.a, y = setupLength * bone.c;
						lengths[i] = Math.sqrt(x * x + y * y);
					}
				}
				Utils.arrayFill(spaces, 1, spacesCount, spacing);
				break;
			case SpacingMode.Proportional: {
				let sum = 0;
				for (let i = 0, n = spacesCount - 1; i < n;) {
					const bone = bones[i];
					const setupLength = bone.bone.data.length;
					if (setupLength < PathConstraint.epsilon) {
						if (scale) lengths[i] = 0;
						spaces[++i] = spacing;
					} else {
						const x = setupLength * bone.a, y = setupLength * bone.c;
						const length = Math.sqrt(x * x + y * y);
						if (scale) lengths[i] = length;
						spaces[++i] = length;
						sum += length;
					}
				}
				if (sum > 0) {
					sum = spacesCount / sum * spacing;
					for (let i = 1; i < spacesCount; i++)
						spaces[i] *= sum;
				}
				break;
			}
			default: {
				const lengthSpacing = data.spacingMode === SpacingMode.Length;
				for (let i = 0, n = spacesCount - 1; i < n;) {
					const bone = bones[i];
					const setupLength = bone.bone.data.length;
					if (setupLength < PathConstraint.epsilon) {
						if (scale) lengths[i] = 0;
						spaces[++i] = spacing;
					} else {
						const x = setupLength * bone.a, y = setupLength * bone.c;
						const length = Math.sqrt(x * x + y * y);
						if (scale) lengths[i] = length;
						spaces[++i] = (lengthSpacing ? Math.max(0, setupLength + spacing) : spacing) * length / setupLength;
					}
				}
			}
		}

		const positions = this.computeWorldPositions(skeleton, attachment, spacesCount, tangents);
		let boneX = positions[0], boneY = positions[1], offsetRotation = data.offsetRotation;
		let tip = false;
		if (offsetRotation === 0)
			tip = data.rotateMode === RotateMode.Chain;
		else {
			tip = false;
			const bone = this.slot.bone.applied;
			offsetRotation *= bone.a * bone.d - bone.b * bone.c > 0 ? MathUtils.degRad : -MathUtils.degRad;
		}
		for (let i = 0, ip = 3, u = skeleton._update; i < boneCount; i++, ip += 3) {
			const bone = bones[i];
			bone.worldX += (boneX - bone.worldX) * mixX;
			bone.worldY += (boneY - bone.worldY) * mixY;
			const x = positions[ip], y = positions[ip + 1], dx = x - boneX, dy = y - boneY;
			if (scale) {
				const length = lengths[i];
				if (length !== 0) {
					const s = (Math.sqrt(dx * dx + dy * dy) / length - 1) * mixRotate + 1;
					bone.a *= s;
					bone.c *= s;
				}
			}
			boneX = x;
			boneY = y;
			if (mixRotate > 0) {
				let a = bone.a, b = bone.b, c = bone.c, d = bone.d, r = 0, cos = 0, sin = 0;
				if (tangents)
					r = positions[ip - 1];
				else if (spaces[i + 1] === 0)
					r = positions[ip + 2];
				else
					r = Math.atan2(dy, dx);
				r -= Math.atan2(c, a);
				if (tip) {
					cos = Math.cos(r);
					sin = Math.sin(r);
					const length = bone.bone.data.length;
					boneX += (length * (cos * a - sin * c) - dx) * mixRotate;
					boneY += (length * (sin * a + cos * c) - dy) * mixRotate;
				} else {
					r += offsetRotation;
				}
				if (r > MathUtils.PI)
					r -= MathUtils.PI2;
				else if (r < -MathUtils.PI) //
					r += MathUtils.PI2;
				r *= mixRotate;
				cos = Math.cos(r);
				sin = Math.sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}
			bone.modifyWorld(u);
		}
	}

	computeWorldPositions (skeleton: Skeleton, path: PathAttachment, spacesCount: number, tangents: boolean) {
		const slot = this.slot;
		let position = this.applied.position;
		let spaces = this.spaces, out = Utils.setArraySize(this.positions, spacesCount * 3 + 2), world: Array<number> = this.world;
		const closed = path.closed;
		let verticesLength = path.worldVerticesLength, curveCount = verticesLength / 6, prevCurve = PathConstraint.NONE;

		if (!path.constantSpeed) {
			const lengths = path.lengths;
			curveCount -= closed ? 1 : 2;
			const pathLength = lengths[curveCount];
			if (this.data.positionMode === PositionMode.Percent) position *= pathLength;

			let multiplier: number;
			switch (this.data.spacingMode) {
				case SpacingMode.Percent: multiplier = pathLength; break;
				case SpacingMode.Proportional: multiplier = pathLength / spacesCount; break;
				default: multiplier = 1;
			}

			world = Utils.setArraySize(this.world, 8);
			for (let i = 0, o = 0, curve = 0; i < spacesCount; i++, o += 3) {
				const space = spaces[i] * multiplier;
				position += space;
				let p = position;

				if (closed) {
					p %= pathLength;
					if (p < 0) p += pathLength;
					curve = 0;
				} else if (p < 0) {
					if (prevCurve !== PathConstraint.BEFORE) {
						prevCurve = PathConstraint.BEFORE;
						path.computeWorldVertices(skeleton, slot, 2, 4, world, 0, 2);
					}
					this.addBeforePosition(p, world, 0, out, o);
					continue;
				} else if (p > pathLength) {
					if (prevCurve !== PathConstraint.AFTER) {
						prevCurve = PathConstraint.AFTER;
						path.computeWorldVertices(skeleton, slot, verticesLength - 6, 4, world, 0, 2);
					}
					this.addAfterPosition(p - pathLength, world, 0, out, o);
					continue;
				}

				// Determine curve containing position.
				for (; ; curve++) {
					const length = lengths[curve];
					if (p > length) continue;
					if (curve === 0)
						p /= length;
					else {
						const prev = lengths[curve - 1];
						p = (p - prev) / (length - prev);
					}
					break;
				}
				if (curve !== prevCurve) {
					prevCurve = curve;
					if (closed && curve === curveCount) {
						path.computeWorldVertices(skeleton, slot, verticesLength - 4, 4, world, 0, 2);
						path.computeWorldVertices(skeleton, slot, 0, 4, world, 4, 2);
					} else
						path.computeWorldVertices(skeleton, slot, curve * 6 + 2, 8, world, 0, 2);
				}
				this.addCurvePosition(p, world[0], world[1], world[2], world[3], world[4], world[5], world[6], world[7], out, o,
					tangents || (i > 0 && space === 0));
			}
			return out;
		}

		// World vertices.
		if (closed) {
			verticesLength += 2;
			world = Utils.setArraySize(this.world, verticesLength);
			path.computeWorldVertices(skeleton, slot, 2, verticesLength - 4, world, 0, 2);
			path.computeWorldVertices(skeleton, slot, 0, 2, world, verticesLength - 4, 2);
			world[verticesLength - 2] = world[0];
			world[verticesLength - 1] = world[1];
		} else {
			curveCount--;
			verticesLength -= 4;
			world = Utils.setArraySize(this.world, verticesLength);
			path.computeWorldVertices(skeleton, slot, 2, verticesLength, world, 0, 2);
		}

		// Curve lengths.
		const curves = Utils.setArraySize(this.curves, curveCount);
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

		if (this.data.positionMode === PositionMode.Percent) position *= pathLength;

		let multiplier: number;
		switch (this.data.spacingMode) {
			case SpacingMode.Percent: multiplier = pathLength; break;
			case SpacingMode.Proportional: multiplier = pathLength / spacesCount; break;
			default: multiplier = 1;
		}

		const segments = this.segments;
		let curveLength = 0;
		for (let i = 0, o = 0, curve = 0, segment = 0; i < spacesCount; i++, o += 3) {
			const space = spaces[i] * multiplier;
			position += space;
			let p = position;

			if (closed) {
				p %= pathLength;
				if (p < 0) p += pathLength;
				curve = 0;
				segment = 0;
			} else if (p < 0) {
				this.addBeforePosition(p, world, 0, out, o);
				continue;
			} else if (p > pathLength) {
				this.addAfterPosition(p - pathLength, world, verticesLength - 4, out, o);
				continue;
			}

			// Determine curve containing position.
			for (; ; curve++) {
				const length = curves[curve];
				if (p > length) continue;
				if (curve === 0)
					p /= length;
				else {
					const prev = curves[curve - 1];
					p = (p - prev) / (length - prev);
				}
				break;
			}

			// Curve segment lengths.
			if (curve !== prevCurve) {
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
			for (; ; segment++) {
				const length = segments[segment];
				if (p > length) continue;
				if (segment === 0)
					p /= length;
				else {
					const prev = segments[segment - 1];
					p = segment + (p - prev) / (length - prev);
				}
				break;
			}
			this.addCurvePosition(p * 0.1, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents || (i > 0 && space === 0));
		}
		return out;
	}

	addBeforePosition (p: number, temp: Array<number>, i: number, out: Array<number>, o: number) {
		const x1 = temp[i], y1 = temp[i + 1], dx = temp[i + 2] - x1, dy = temp[i + 3] - y1, r = Math.atan2(dy, dx);
		out[o] = x1 + p * Math.cos(r);
		out[o + 1] = y1 + p * Math.sin(r);
		out[o + 2] = r;
	}

	addAfterPosition (p: number, temp: Array<number>, i: number, out: Array<number>, o: number) {
		const x1 = temp[i + 2], y1 = temp[i + 3], dx = x1 - temp[i], dy = y1 - temp[i + 1], r = Math.atan2(dy, dx);
		out[o] = x1 + p * Math.cos(r);
		out[o + 1] = y1 + p * Math.sin(r);
		out[o + 2] = r;
	}

	addCurvePosition (p: number, x1: number, y1: number, cx1: number, cy1: number, cx2: number, cy2: number, x2: number, y2: number,
		out: Array<number>, o: number, tangents: boolean) {
		if (p === 0 || Number.isNaN(p)) {
			out[o] = x1;
			out[o + 1] = y1;
			out[o + 2] = Math.atan2(cy1 - y1, cx1 - x1);
			return;
		}
		const tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
		const ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
		const x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
		out[o] = x;
		out[o + 1] = y;
		if (tangents) {
			if (p < 0.001)
				out[o + 2] = Math.atan2(cy1 - y1, cx1 - x1);
			else
				out[o + 2] = Math.atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
		}
	}

	sort (skeleton: Skeleton) {
		const slotIndex = this.slot.data.index;
		const slotBone = this.slot.bone;
		if (skeleton.skin != null) this.sortPathSlot(skeleton, skeleton.skin, slotIndex, slotBone);
		if (skeleton.data.defaultSkin != null && skeleton.data.defaultSkin !== skeleton.skin)
			this.sortPathSlot(skeleton, skeleton.data.defaultSkin, slotIndex, slotBone);
		this.sortPath(skeleton, this.slot.pose.attachment, slotBone);
		const bones = this.bones;
		const boneCount = this.bones.length;
		for (let i = 0; i < boneCount; i++) {
			const bone = bones[i].bone;
			skeleton.sortBone(bone);
			skeleton.constrained(bone);
		}
		skeleton._updateCache.push(this);
		for (let i = 0; i < boneCount; i++)
			skeleton.sortReset(bones[i].bone.children);
		for (let i = 0; i < boneCount; i++)
			bones[i].bone.sorted = true;
	}

	private sortPathSlot (skeleton: Skeleton, skin: Skin, slotIndex: number, slotBone: Bone) {
		const entries = skin.getAttachments();
		for (let i = 0, n = entries.length; i < n; i++) {
			const entry = entries[i];
			if (entry.slotIndex === slotIndex) this.sortPath(skeleton, entry.attachment, slotBone);
		}
	}

	private sortPath (skeleton: Skeleton, attachment: Attachment | null, slotBone: Bone) {
		if (!(attachment instanceof PathAttachment)) return;
		const pathBones = attachment.bones;
		if (pathBones == null)
			skeleton.sortBone(slotBone);
		else {
			const bones = skeleton.bones;
			for (let i = 0, n = pathBones.length; i < n;) {
				let nn = pathBones[i++];
				nn += i;
				while (i < nn)
					skeleton.sortBone(bones[pathBones[i++]]);
			}
		}
	}

	isSourceActive () {
		return this.slot.bone.active;
	}
}
