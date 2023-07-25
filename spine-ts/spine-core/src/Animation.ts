/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { VertexAttachment, Attachment } from "./attachments/Attachment";
import { IkConstraint } from "./IkConstraint";
import { PathConstraint } from "./PathConstraint";
import { Skeleton } from "./Skeleton";
import { Slot } from "./Slot";
import { TransformConstraint } from "./TransformConstraint";
import { StringSet, Utils, MathUtils, NumberArrayLike } from "./Utils";
import { Event } from "./Event";
import { HasTextureRegion } from "./attachments/HasTextureRegion";
import { SequenceMode, SequenceModeValues } from "./attachments/Sequence";

/** A simple container for a list of timelines and a name. */
export class Animation {
	/** The animation's name, which is unique across all animations in the skeleton. */
	name: string;
	timelines: Array<Timeline> = [];
	timelineIds: StringSet = new StringSet();

	/** The duration of the animation in seconds, which is the highest time of all keys in the timeline. */
	duration: number;

	constructor (name: string, timelines: Array<Timeline>, duration: number) {
		if (!name) throw new Error("name cannot be null.");
		this.name = name;
		this.setTimelines(timelines);
		this.duration = duration;
	}

	setTimelines (timelines: Array<Timeline>) {
		if (!timelines) throw new Error("timelines cannot be null.");
		this.timelines = timelines;
		this.timelineIds.clear();
		for (var i = 0; i < timelines.length; i++)
			this.timelineIds.addAll(timelines[i].getPropertyIds());
	}

	hasTimeline (ids: string[]): boolean {
		for (let i = 0; i < ids.length; i++)
			if (this.timelineIds.contains(ids[i])) return true;
		return false;
	}

	/** Applies all the animation's timelines to the specified skeleton.
	 *
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection)}.
	 * @param loop If true, the animation repeats after {@link #getDuration()}.
	 * @param events May be null to ignore fired events. */
	apply (skeleton: Skeleton, lastTime: number, time: number, loop: boolean, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		if (!skeleton) throw new Error("skeleton cannot be null.");

		if (loop && this.duration != 0) {
			time %= this.duration;
			if (lastTime > 0) lastTime %= this.duration;
		}

		let timelines = this.timelines;
		for (let i = 0, n = timelines.length; i < n; i++)
			timelines[i].apply(skeleton, lastTime, time, events, alpha, blend, direction);
	}
}

/** Controls how a timeline value is mixed with the setup pose value or current pose value when a timeline's `alpha`
 * < 1.
 *
 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection)}. */
export enum MixBlend {
	/** Transitions from the setup value to the timeline value (the current value is not used). Before the first key, the setup
	 * value is set. */
	setup,
	/** Transitions from the current value to the timeline value. Before the first key, transitions from the current value to
	 * the setup value. Timelines which perform instant transitions, such as {@link DrawOrderTimeline} or
	 * {@link AttachmentTimeline}, use the setup value before the first key.
	 *
	 * `first` is intended for the first animations applied, not for animations layered on top of those. */
	first,
	/** Transitions from the current value to the timeline value. No change is made before the first key (the current value is
	 * kept until the first key).
	 *
	 * `replace` is intended for animations layered on top of others, not for the first animations applied. */
	replace,
	/** Transitions from the current value to the current value plus the timeline value. No change is made before the first key
	 * (the current value is kept until the first key).
	 *
	 * `add` is intended for animations layered on top of others, not for the first animations applied. Properties
	 * keyed by additive animations must be set manually or by another animation before applying the additive animations, else
	 * the property values will increase continually. */
	add
}

/** Indicates whether a timeline's `alpha` is mixing out over time toward 0 (the setup or current pose value) or
 * mixing in toward 1 (the timeline's value).
 *
 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection)}. */
export enum MixDirection {
	mixIn, mixOut
}

const Property = {
	rotate: 0,
	x: 1,
	y: 2,
	scaleX: 3,
	scaleY: 4,
	shearX: 5,
	shearY: 6,

	rgb: 7,
	alpha: 8,
	rgb2: 9,

	attachment: 10,
	deform: 11,

	event: 12,
	drawOrder: 13,

	ikConstraint: 14,
	transformConstraint: 15,

	pathConstraintPosition: 16,
	pathConstraintSpacing: 17,
	pathConstraintMix: 18,

	sequence: 19
}

/** The interface for all timelines. */
export abstract class Timeline {
	propertyIds: string[];
	frames: NumberArrayLike;

	constructor (frameCount: number, propertyIds: string[]) {
		this.propertyIds = propertyIds;
		this.frames = Utils.newFloatArray(frameCount * this.getFrameEntries());
	}

	getPropertyIds () {
		return this.propertyIds;
	}

	getFrameEntries (): number {
		return 1;
	}

	getFrameCount () {
		return this.frames.length / this.getFrameEntries();
	}

	getDuration (): number {
		return this.frames[this.frames.length - this.getFrameEntries()];
	}

	abstract apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event> | null, alpha: number, blend: MixBlend, direction: MixDirection): void;

	static search1 (frames: NumberArrayLike, time: number) {
		let n = frames.length;
		for (let i = 1; i < n; i++)
			if (frames[i] > time) return i - 1;
		return n - 1;
	}

	static search (frames: NumberArrayLike, time: number, step: number) {
		let n = frames.length;
		for (let i = step; i < n; i += step)
			if (frames[i] > time) return i - step;
		return n - step;
	}
}

export interface BoneTimeline {
	/** The index of the bone in {@link Skeleton#bones} that will be changed. */
	boneIndex: number;
}

export interface SlotTimeline {
	/** The index of the slot in {@link Skeleton#slots} that will be changed. */
	slotIndex: number;
}

/** The base class for timelines that use interpolation between key frame values. */
export abstract class CurveTimeline extends Timeline {
	protected curves: NumberArrayLike; // type, x, y, ...

	constructor (frameCount: number, bezierCount: number, propertyIds: string[]) {
		super(frameCount, propertyIds);
		this.curves = Utils.newFloatArray(frameCount + bezierCount * 18/*BEZIER_SIZE*/);
		this.curves[frameCount - 1] = 1/*STEPPED*/;
	}

	/** Sets the specified key frame to linear interpolation. */
	setLinear (frame: number) {
		this.curves[frame] = 0/*LINEAR*/;
	}

	/** Sets the specified key frame to stepped interpolation. */
	setStepped (frame: number) {
		this.curves[frame] = 1/*STEPPED*/;
	}

	/** Shrinks the storage for Bezier curves, for use when <code>bezierCount</code> (specified in the constructor) was larger
	 * than the actual number of Bezier curves. */
	shrink (bezierCount: number) {
		let size = this.getFrameCount() + bezierCount * 18/*BEZIER_SIZE*/;
		if (this.curves.length > size) {
			let newCurves = Utils.newFloatArray(size);
			Utils.arrayCopy(this.curves, 0, newCurves, 0, size);
			this.curves = newCurves;
		}
	}

	/** Stores the segments for the specified Bezier curve. For timelines that modify multiple values, there may be more than
	 * one curve per frame.
	 * @param bezier The ordinal of this Bezier curve for this timeline, between 0 and <code>bezierCount - 1</code> (specified
	 *           in the constructor), inclusive.
	 * @param frame Between 0 and <code>frameCount - 1</code>, inclusive.
	 * @param value The index of the value for this frame that this curve is used for.
	 * @param time1 The time for the first key.
	 * @param value1 The value for the first key.
	 * @param cx1 The time for the first Bezier handle.
	 * @param cy1 The value for the first Bezier handle.
	 * @param cx2 The time of the second Bezier handle.
	 * @param cy2 The value for the second Bezier handle.
	 * @param time2 The time for the second key.
	 * @param value2 The value for the second key. */
	setBezier (bezier: number, frame: number, value: number, time1: number, value1: number, cx1: number, cy1: number, cx2: number,
		cy2: number, time2: number, value2: number) {
		let curves = this.curves;
		let i = this.getFrameCount() + bezier * 18/*BEZIER_SIZE*/;
		if (value == 0) curves[frame] = 2/*BEZIER*/ + i;
		let tmpx = (time1 - cx1 * 2 + cx2) * 0.03, tmpy = (value1 - cy1 * 2 + cy2) * 0.03;
		let dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy = ((cy1 - cy2) * 3 - value1 + value2) * 0.006;
		let ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
		let dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667, dy = (cy1 - value1) * 0.3 + tmpy + dddy * 0.16666667;
		let x = time1 + dx, y = value1 + dy;
		for (let n = i + 18/*BEZIER_SIZE*/; i < n; i += 2) {
			curves[i] = x;
			curves[i + 1] = y;
			dx += ddx;
			dy += ddy;
			ddx += dddx;
			ddy += dddy;
			x += dx;
			y += dy;
		}
	}

	/** Returns the Bezier interpolated value for the specified time.
	 * @param frameIndex The index into {@link #getFrames()} for the values of the frame before <code>time</code>.
	 * @param valueOffset The offset from <code>frameIndex</code> to the value this curve is used for.
	 * @param i The index of the Bezier segments. See {@link #getCurveType(int)}. */
	getBezierValue (time: number, frameIndex: number, valueOffset: number, i: number) {
		let curves = this.curves;
		if (curves[i] > time) {
			let x = this.frames[frameIndex], y = this.frames[frameIndex + valueOffset];
			return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
		}
		let n = i + 18/*BEZIER_SIZE*/;
		for (i += 2; i < n; i += 2) {
			if (curves[i] >= time) {
				let x = curves[i - 2], y = curves[i - 1];
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
			}
		}
		frameIndex += this.getFrameEntries();
		let x = curves[n - 2], y = curves[n - 1];
		return y + (time - x) / (this.frames[frameIndex] - x) * (this.frames[frameIndex + valueOffset] - y);
	}
}

export abstract class CurveTimeline1 extends CurveTimeline {
	constructor (frameCount: number, bezierCount: number, propertyId: string) {
		super(frameCount, bezierCount, [propertyId]);
	}

	getFrameEntries () {
		return 2/*ENTRIES*/;
	}

	/** Sets the time and value for the specified frame.
	 * @param frame Between 0 and <code>frameCount</code>, inclusive.
	 * @param time The frame time in seconds. */
	setFrame (frame: number, time: number, value: number) {
		frame <<= 1;
		this.frames[frame] = time;
		this.frames[frame + 1/*VALUE*/] = value;
	}

	/** Returns the interpolated value for the specified time. */
	getCurveValue (time: number) {
		let frames = this.frames;
		let i = frames.length - 2;
		for (let ii = 2; ii <= i; ii += 2) {
			if (frames[ii] > time) {
				i = ii - 2;
				break;
			}
		}

		let curveType = this.curves[i >> 1];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i], value = frames[i + 1/*VALUE*/];
				return value + (time - before) / (frames[i + 2/*ENTRIES*/] - before) * (frames[i + 2/*ENTRIES*/ + 1/*VALUE*/] - value);
			case 1/*STEPPED*/:
				return frames[i + 1/*VALUE*/];
		}
		return this.getBezierValue(time, i, 1/*VALUE*/, curveType - 2/*BEZIER*/);
	}
}

/** The base class for a {@link CurveTimeline} which sets two properties. */
export abstract class CurveTimeline2 extends CurveTimeline {
	/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
	 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
	constructor (frameCount: number, bezierCount: number, propertyId1: string, propertyId2: string) {
		super(frameCount, bezierCount, [propertyId1, propertyId2]);
	}

	getFrameEntries () {
		return 3/*ENTRIES*/;
	}

	/** Sets the time and values for the specified frame.
	 * @param frame Between 0 and <code>frameCount</code>, inclusive.
	 * @param time The frame time in seconds. */
	setFrame (frame: number, time: number, value1: number, value2: number) {
		frame *= 3/*ENTRIES*/;
		this.frames[frame] = time;
		this.frames[frame + 1/*VALUE1*/] = value1;
		this.frames[frame + 2/*VALUE2*/] = value2;
	}
}

/** Changes a bone's local {@link Bone#rotation}. */
export class RotateTimeline extends CurveTimeline1 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, Property.rotate + "|" + boneIndex);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event> | null, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.rotation = bone.data.rotation;
					return;
				case MixBlend.first:
					bone.rotation += (bone.data.rotation - bone.rotation) * alpha;
			}
			return;
		}

		let r = this.getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				bone.rotation = bone.data.rotation + r * alpha;
				break;
			case MixBlend.first:
			case MixBlend.replace:
				r += bone.data.rotation - bone.rotation;
			case MixBlend.add:
				bone.rotation += r * alpha;
		}
	}
}

/** Changes a bone's local {@link Bone#x} and {@link Bone#y}. */
export class TranslateTimeline extends CurveTimeline2 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount,
			Property.x + "|" + boneIndex,
			Property.y + "|" + boneIndex,
		);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.x = bone.data.x;
					bone.y = bone.data.y;
					return;
				case MixBlend.first:
					bone.x += (bone.data.x - bone.x) * alpha;
					bone.y += (bone.data.y - bone.y) * alpha;
			}
			return;
		}

		let x = 0, y = 0;
		let i = Timeline.search(frames, time, 3/*ENTRIES*/);
		let curveType = this.curves[i / 3/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				let t = (time - before) / (frames[i + 3/*ENTRIES*/] - before);
				x += (frames[i + 3/*ENTRIES*/ + 1/*VALUE1*/] - x) * t;
				y += (frames[i + 3/*ENTRIES*/ + 2/*VALUE2*/] - y) * t;
				break;
			case 1/*STEPPED*/:
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				break;
			default:
				x = this.getBezierValue(time, i, 1/*VALUE1*/, curveType - 2/*BEZIER*/);
				y = this.getBezierValue(time, i, 2/*VALUE2*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
		}

		switch (blend) {
			case MixBlend.setup:
				bone.x = bone.data.x + x * alpha;
				bone.y = bone.data.y + y * alpha;
				break;
			case MixBlend.first:
			case MixBlend.replace:
				bone.x += (bone.data.x + x - bone.x) * alpha;
				bone.y += (bone.data.y + y - bone.y) * alpha;
				break;
			case MixBlend.add:
				bone.x += x * alpha;
				bone.y += y * alpha;
		}
	}
}

/** Changes a bone's local {@link Bone#x}. */
export class TranslateXTimeline extends CurveTimeline1 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, Property.x + "|" + boneIndex);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.x = bone.data.x;
					return;
				case MixBlend.first:
					bone.x += (bone.data.x - bone.x) * alpha;
			}
			return;
		}

		let x = this.getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				bone.x = bone.data.x + x * alpha;
				break;
			case MixBlend.first:
			case MixBlend.replace:
				bone.x += (bone.data.x + x - bone.x) * alpha;
				break;
			case MixBlend.add:
				bone.x += x * alpha;
		}
	}
}

/** Changes a bone's local {@link Bone#x}. */
export class TranslateYTimeline extends CurveTimeline1 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, Property.y + "|" + boneIndex);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.y = bone.data.y;
					return;
				case MixBlend.first:
					bone.y += (bone.data.y - bone.y) * alpha;
			}
			return;
		}

		let y = this.getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				bone.y = bone.data.y + y * alpha;
				break;
			case MixBlend.first:
			case MixBlend.replace:
				bone.y += (bone.data.y + y - bone.y) * alpha;
				break;
			case MixBlend.add:
				bone.y += y * alpha;
		}
	}
}

/** Changes a bone's local {@link Bone#scaleX)} and {@link Bone#scaleY}. */
export class ScaleTimeline extends CurveTimeline2 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount,
			Property.scaleX + "|" + boneIndex,
			Property.scaleY + "|" + boneIndex
		);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.scaleX = bone.data.scaleX;
					bone.scaleY = bone.data.scaleY;
					return;
				case MixBlend.first:
					bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
			}
			return;
		}

		let x, y;
		let i = Timeline.search(frames, time, 3/*ENTRIES*/);
		let curveType = this.curves[i / 3/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				let t = (time - before) / (frames[i + 3/*ENTRIES*/] - before);
				x += (frames[i + 3/*ENTRIES*/ + 1/*VALUE1*/] - x) * t;
				y += (frames[i + 3/*ENTRIES*/ + 2/*VALUE2*/] - y) * t;
				break;
			case 1/*STEPPED*/:
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				break;
			default:
				x = this.getBezierValue(time, i, 1/*VALUE1*/, curveType - 2/*BEZIER*/);
				y = this.getBezierValue(time, i, 2/*VALUE2*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
		}
		x *= bone.data.scaleX;
		y *= bone.data.scaleY;

		if (alpha == 1) {
			if (blend == MixBlend.add) {
				bone.scaleX += x - bone.data.scaleX;
				bone.scaleY += y - bone.data.scaleY;
			} else {
				bone.scaleX = x;
				bone.scaleY = y;
			}
		} else {
			let bx = 0, by = 0;
			if (direction == MixDirection.mixOut) {
				switch (blend) {
					case MixBlend.setup:
						bx = bone.data.scaleX;
						by = bone.data.scaleY;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						bx = bone.scaleX;
						by = bone.scaleY;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
						break;
					case MixBlend.add:
						bone.scaleX += (x - bone.data.scaleX) * alpha;
						bone.scaleY += (y - bone.data.scaleY) * alpha;
				}
			} else {
				switch (blend) {
					case MixBlend.setup:
						bx = Math.abs(bone.data.scaleX) * MathUtils.signum(x);
						by = Math.abs(bone.data.scaleY) * MathUtils.signum(y);
						bone.scaleX = bx + (x - bx) * alpha;
						bone.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						bx = Math.abs(bone.scaleX) * MathUtils.signum(x);
						by = Math.abs(bone.scaleY) * MathUtils.signum(y);
						bone.scaleX = bx + (x - bx) * alpha;
						bone.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.add:
						bone.scaleX += (x - bone.data.scaleX) * alpha;
						bone.scaleY += (y - bone.data.scaleY) * alpha;
				}
			}
		}
	}
}

/** Changes a bone's local {@link Bone#scaleX)} and {@link Bone#scaleY}. */
export class ScaleXTimeline extends CurveTimeline1 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, Property.scaleX + "|" + boneIndex);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.scaleX = bone.data.scaleX;
					return;
				case MixBlend.first:
					bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
			}
			return;
		}

		let x = this.getCurveValue(time) * bone.data.scaleX;
		if (alpha == 1) {
			if (blend == MixBlend.add)
				bone.scaleX += x - bone.data.scaleX;
			else
				bone.scaleX = x;
		} else {
			// Mixing out uses sign of setup or current pose, else use sign of key.
			let bx = 0;
			if (direction == MixDirection.mixOut) {
				switch (blend) {
					case MixBlend.setup:
						bx = bone.data.scaleX;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						bx = bone.scaleX;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
						break;
					case MixBlend.add:
						bone.scaleX += (x - bone.data.scaleX) * alpha;
				}
			} else {
				switch (blend) {
					case MixBlend.setup:
						bx = Math.abs(bone.data.scaleX) * MathUtils.signum(x);
						bone.scaleX = bx + (x - bx) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						bx = Math.abs(bone.scaleX) * MathUtils.signum(x);
						bone.scaleX = bx + (x - bx) * alpha;
						break;
					case MixBlend.add:
						bone.scaleX += (x - bone.data.scaleX) * alpha;
				}
			}
		}
	}
}

/** Changes a bone's local {@link Bone#scaleX)} and {@link Bone#scaleY}. */
export class ScaleYTimeline extends CurveTimeline1 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, Property.scaleY + "|" + boneIndex);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.scaleY = bone.data.scaleY;
					return;
				case MixBlend.first:
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
			}
			return;
		}

		let y = this.getCurveValue(time) * bone.data.scaleY;
		if (alpha == 1) {
			if (blend == MixBlend.add)
				bone.scaleY += y - bone.data.scaleY;
			else
				bone.scaleY = y;
		} else {
			// Mixing out uses sign of setup or current pose, else use sign of key.
			let by = 0;
			if (direction == MixDirection.mixOut) {
				switch (blend) {
					case MixBlend.setup:
						by = bone.data.scaleY;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						by = bone.scaleY;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
						break;
					case MixBlend.add:
						bone.scaleY += (y - bone.data.scaleY) * alpha;
				}
			} else {
				switch (blend) {
					case MixBlend.setup:
						by = Math.abs(bone.data.scaleY) * MathUtils.signum(y);
						bone.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						by = Math.abs(bone.scaleY) * MathUtils.signum(y);
						bone.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.add:
						bone.scaleY += (y - bone.data.scaleY) * alpha;
				}
			}
		}
	}
}

/** Changes a bone's local {@link Bone#shearX} and {@link Bone#shearY}. */
export class ShearTimeline extends CurveTimeline2 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount,
			Property.shearX + "|" + boneIndex,
			Property.shearY + "|" + boneIndex
		);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.shearX = bone.data.shearX;
					bone.shearY = bone.data.shearY;
					return;
				case MixBlend.first:
					bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
					bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
			}
			return;
		}

		let x = 0, y = 0;
		let i = Timeline.search(frames, time, 3/*ENTRIES*/);
		let curveType = this.curves[i / 3/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				let t = (time - before) / (frames[i + 3/*ENTRIES*/] - before);
				x += (frames[i + 3/*ENTRIES*/ + 1/*VALUE1*/] - x) * t;
				y += (frames[i + 3/*ENTRIES*/ + 2/*VALUE2*/] - y) * t;
				break;
			case 1/*STEPPED*/:
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				break;
			default:
				x = this.getBezierValue(time, i, 1/*VALUE1*/, curveType - 2/*BEZIER*/);
				y = this.getBezierValue(time, i, 2/*VALUE2*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
		}

		switch (blend) {
			case MixBlend.setup:
				bone.shearX = bone.data.shearX + x * alpha;
				bone.shearY = bone.data.shearY + y * alpha;
				break;
			case MixBlend.first:
			case MixBlend.replace:
				bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
				bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
				break;
			case MixBlend.add:
				bone.shearX += x * alpha;
				bone.shearY += y * alpha;
		}
	}
}

/** Changes a bone's local {@link Bone#shearX} and {@link Bone#shearY}. */
export class ShearXTimeline extends CurveTimeline1 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, Property.shearX + "|" + boneIndex);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.shearX = bone.data.shearX;
					return;
				case MixBlend.first:
					bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
			}
			return;
		}

		let x = this.getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				bone.shearX = bone.data.shearX + x * alpha;
				break;
			case MixBlend.first:
			case MixBlend.replace:
				bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
				break;
			case MixBlend.add:
				bone.shearX += x * alpha;
		}
	}
}

/** Changes a bone's local {@link Bone#shearX} and {@link Bone#shearY}. */
export class ShearYTimeline extends CurveTimeline1 implements BoneTimeline {
	boneIndex = 0;

	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, Property.shearY + "|" + boneIndex);
		this.boneIndex = boneIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.shearY = bone.data.shearY;
					return;
				case MixBlend.first:
					bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
			}
			return;
		}

		let y = this.getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				bone.shearY = bone.data.shearY + y * alpha;
				break;
			case MixBlend.first:
			case MixBlend.replace:
				bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
				break;
			case MixBlend.add:
				bone.shearY += y * alpha;
		}
	}
}

/** Changes a slot's {@link Slot#color}. */
export class RGBATimeline extends CurveTimeline implements SlotTimeline {
	slotIndex = 0;

	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, [
			Property.rgb + "|" + slotIndex,
			Property.alpha + "|" + slotIndex
		]);
		this.slotIndex = slotIndex;
	}

	getFrameEntries () {
		return 5/*ENTRIES*/;
	}

	/** Sets the time in seconds, red, green, blue, and alpha for the specified key frame. */
	setFrame (frame: number, time: number, r: number, g: number, b: number, a: number) {
		frame *= 5/*ENTRIES*/;
		this.frames[frame] = time;
		this.frames[frame + 1/*R*/] = r;
		this.frames[frame + 2/*G*/] = g;
		this.frames[frame + 3/*B*/] = b;
		this.frames[frame + 4/*A*/] = a;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;

		let frames = this.frames;
		let color = slot.color;
		if (time < frames[0]) {
			let setup = slot.data.color;
			switch (blend) {
				case MixBlend.setup:
					color.setFromColor(setup);
					return;
				case MixBlend.first:
					color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha);
			}
			return;
		}

		let r = 0, g = 0, b = 0, a = 0;
		let i = Timeline.search(frames, time, 5/*ENTRIES*/);
		let curveType = this.curves[i / 5/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				a = frames[i + 4/*A*/];
				let t = (time - before) / (frames[i + 5/*ENTRIES*/] - before);
				r += (frames[i + 5/*ENTRIES*/ + 1/*R*/] - r) * t;
				g += (frames[i + 5/*ENTRIES*/ + 2/*G*/] - g) * t;
				b += (frames[i + 5/*ENTRIES*/ + 3/*B*/] - b) * t;
				a += (frames[i + 5/*ENTRIES*/ + 4/*A*/] - a) * t;
				break;
			case 1/*STEPPED*/:
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				a = frames[i + 4/*A*/];
				break;
			default:
				r = this.getBezierValue(time, i, 1/*R*/, curveType - 2/*BEZIER*/);
				g = this.getBezierValue(time, i, 2/*G*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
				b = this.getBezierValue(time, i, 3/*B*/, curveType + 18/*BEZIER_SIZE*/ * 2 - 2/*BEZIER*/);
				a = this.getBezierValue(time, i, 4/*A*/, curveType + 18/*BEZIER_SIZE*/ * 3 - 2/*BEZIER*/);
		}
		if (alpha == 1)
			color.set(r, g, b, a);
		else {
			if (blend == MixBlend.setup) color.setFromColor(slot.data.color);
			color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
		}
	}
}

/** Changes a slot's {@link Slot#color}. */
export class RGBTimeline extends CurveTimeline implements SlotTimeline {
	slotIndex = 0;

	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, [
			Property.rgb + "|" + slotIndex
		]);
		this.slotIndex = slotIndex;
	}

	getFrameEntries () {
		return 4/*ENTRIES*/;
	}

	/** Sets the time in seconds, red, green, blue, and alpha for the specified key frame. */
	setFrame (frame: number, time: number, r: number, g: number, b: number) {
		frame <<= 2;
		this.frames[frame] = time;
		this.frames[frame + 1/*R*/] = r;
		this.frames[frame + 2/*G*/] = g;
		this.frames[frame + 3/*B*/] = b;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;

		let frames = this.frames;
		let color = slot.color;
		if (time < frames[0]) {
			let setup = slot.data.color;
			switch (blend) {
				case MixBlend.setup:
					color.r = setup.r;
					color.g = setup.g;
					color.b = setup.b;
					return;
				case MixBlend.first:
					color.r += (setup.r - color.r) * alpha;
					color.g += (setup.g - color.g) * alpha;
					color.b += (setup.b - color.b) * alpha;
			}
			return;
		}

		let r = 0, g = 0, b = 0;
		let i = Timeline.search(frames, time, 4/*ENTRIES*/);
		let curveType = this.curves[i >> 2];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				let t = (time - before) / (frames[i + 4/*ENTRIES*/] - before);
				r += (frames[i + 4/*ENTRIES*/ + 1/*R*/] - r) * t;
				g += (frames[i + 4/*ENTRIES*/ + 2/*G*/] - g) * t;
				b += (frames[i + 4/*ENTRIES*/ + 3/*B*/] - b) * t;
				break;
			case 1/*STEPPED*/:
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				break;
			default:
				r = this.getBezierValue(time, i, 1/*R*/, curveType - 2/*BEZIER*/);
				g = this.getBezierValue(time, i, 2/*G*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
				b = this.getBezierValue(time, i, 3/*B*/, curveType + 18/*BEZIER_SIZE*/ * 2 - 2/*BEZIER*/);
		}
		if (alpha == 1) {
			color.r = r;
			color.g = g;
			color.b = b;
		} else {
			if (blend == MixBlend.setup) {
				let setup = slot.data.color;
				color.r = setup.r;
				color.g = setup.g;
				color.b = setup.b;
			}
			color.r += (r - color.r) * alpha;
			color.g += (g - color.g) * alpha;
			color.b += (b - color.b) * alpha;
		}
	}
}

/** Changes a bone's local {@link Bone#shearX} and {@link Bone#shearY}. */
export class AlphaTimeline extends CurveTimeline1 implements SlotTimeline {
	slotIndex = 0;

	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, Property.alpha + "|" + slotIndex);
		this.slotIndex = slotIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;

		let color = slot.color;
		if (time < this.frames[0]) { // Time is before first frame.
			let setup = slot.data.color;
			switch (blend) {
				case MixBlend.setup:
					color.a = setup.a;
					return;
				case MixBlend.first:
					color.a += (setup.a - color.a) * alpha;
			}
			return;
		}

		let a = this.getCurveValue(time);
		if (alpha == 1)
			color.a = a;
		else {
			if (blend == MixBlend.setup) color.a = slot.data.color.a;
			color.a += (a - color.a) * alpha;
		}
	}
}

/** Changes a slot's {@link Slot#color} and {@link Slot#darkColor} for two color tinting. */
export class RGBA2Timeline extends CurveTimeline implements SlotTimeline {
	slotIndex = 0;

	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, [
			Property.rgb + "|" + slotIndex,
			Property.alpha + "|" + slotIndex,
			Property.rgb2 + "|" + slotIndex
		]);
		this.slotIndex = slotIndex;
	}

	getFrameEntries () {
		return 8/*ENTRIES*/;
	}

	/** Sets the time in seconds, light, and dark colors for the specified key frame. */
	setFrame (frame: number, time: number, r: number, g: number, b: number, a: number, r2: number, g2: number, b2: number) {
		frame <<= 3;
		this.frames[frame] = time;
		this.frames[frame + 1/*R*/] = r;
		this.frames[frame + 2/*G*/] = g;
		this.frames[frame + 3/*B*/] = b;
		this.frames[frame + 4/*A*/] = a;
		this.frames[frame + 5/*R2*/] = r2;
		this.frames[frame + 6/*G2*/] = g2;
		this.frames[frame + 7/*B2*/] = b2;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;

		let frames = this.frames;
		let light = slot.color, dark = slot.darkColor!;
		if (time < frames[0]) {
			let setupLight = slot.data.color, setupDark = slot.data.darkColor!;
			switch (blend) {
				case MixBlend.setup:
					light.setFromColor(setupLight);
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
					return;
				case MixBlend.first:
					light.add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
						(setupLight.a - light.a) * alpha);
					dark.r += (setupDark.r - dark.r) * alpha;
					dark.g += (setupDark.g - dark.g) * alpha;
					dark.b += (setupDark.b - dark.b) * alpha;
			}
			return;
		}

		let r = 0, g = 0, b = 0, a = 0, r2 = 0, g2 = 0, b2 = 0;
		let i = Timeline.search(frames, time, 8/*ENTRIES*/);
		let curveType = this.curves[i >> 3];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				a = frames[i + 4/*A*/];
				r2 = frames[i + 5/*R2*/];
				g2 = frames[i + 6/*G2*/];
				b2 = frames[i + 7/*B2*/];
				let t = (time - before) / (frames[i + 8/*ENTRIES*/] - before);
				r += (frames[i + 8/*ENTRIES*/ + 1/*R*/] - r) * t;
				g += (frames[i + 8/*ENTRIES*/ + 2/*G*/] - g) * t;
				b += (frames[i + 8/*ENTRIES*/ + 3/*B*/] - b) * t;
				a += (frames[i + 8/*ENTRIES*/ + 4/*A*/] - a) * t;
				r2 += (frames[i + 8/*ENTRIES*/ + 5/*R2*/] - r2) * t;
				g2 += (frames[i + 8/*ENTRIES*/ + 6/*G2*/] - g2) * t;
				b2 += (frames[i + 8/*ENTRIES*/ + 7/*B2*/] - b2) * t;
				break;
			case 1/*STEPPED*/:
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				a = frames[i + 4/*A*/];
				r2 = frames[i + 5/*R2*/];
				g2 = frames[i + 6/*G2*/];
				b2 = frames[i + 7/*B2*/];
				break;
			default:
				r = this.getBezierValue(time, i, 1/*R*/, curveType - 2/*BEZIER*/);
				g = this.getBezierValue(time, i, 2/*G*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
				b = this.getBezierValue(time, i, 3/*B*/, curveType + 18/*BEZIER_SIZE*/ * 2 - 2/*BEZIER*/);
				a = this.getBezierValue(time, i, 4/*A*/, curveType + 18/*BEZIER_SIZE*/ * 3 - 2/*BEZIER*/);
				r2 = this.getBezierValue(time, i, 5/*R2*/, curveType + 18/*BEZIER_SIZE*/ * 4 - 2/*BEZIER*/);
				g2 = this.getBezierValue(time, i, 6/*G2*/, curveType + 18/*BEZIER_SIZE*/ * 5 - 2/*BEZIER*/);
				b2 = this.getBezierValue(time, i, 7/*B2*/, curveType + 18/*BEZIER_SIZE*/ * 6 - 2/*BEZIER*/);
		}

		if (alpha == 1) {
			light.set(r, g, b, a);
			dark.r = r2;
			dark.g = g2;
			dark.b = b2;
		} else {
			if (blend == MixBlend.setup) {
				light.setFromColor(slot.data.color);
				let setupDark = slot.data.darkColor!;
				dark.r = setupDark.r;
				dark.g = setupDark.g;
				dark.b = setupDark.b;
			}
			light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
			dark.r += (r2 - dark.r) * alpha;
			dark.g += (g2 - dark.g) * alpha;
			dark.b += (b2 - dark.b) * alpha;
		}
	}
}

/** Changes a slot's {@link Slot#color} and {@link Slot#darkColor} for two color tinting. */
export class RGB2Timeline extends CurveTimeline implements SlotTimeline {
	slotIndex = 0;

	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, [
			Property.rgb + "|" + slotIndex,
			Property.rgb2 + "|" + slotIndex
		]);
		this.slotIndex = slotIndex;
	}

	getFrameEntries () {
		return 7/*ENTRIES*/;
	}

	/** Sets the time in seconds, light, and dark colors for the specified key frame. */
	setFrame (frame: number, time: number, r: number, g: number, b: number, r2: number, g2: number, b2: number) {
		frame *= 7/*ENTRIES*/;
		this.frames[frame] = time;
		this.frames[frame + 1/*R*/] = r;
		this.frames[frame + 2/*G*/] = g;
		this.frames[frame + 3/*B*/] = b;
		this.frames[frame + 4/*R2*/] = r2;
		this.frames[frame + 5/*G2*/] = g2;
		this.frames[frame + 6/*B2*/] = b2;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;

		let frames = this.frames;
		let light = slot.color, dark = slot.darkColor!;
		if (time < frames[0]) {
			let setupLight = slot.data.color, setupDark = slot.data.darkColor!;
			switch (blend) {
				case MixBlend.setup:
					light.r = setupLight.r;
					light.g = setupLight.g;
					light.b = setupLight.b;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
					return;
				case MixBlend.first:
					light.r += (setupLight.r - light.r) * alpha;
					light.g += (setupLight.g - light.g) * alpha;
					light.b += (setupLight.b - light.b) * alpha;
					dark.r += (setupDark.r - dark.r) * alpha;
					dark.g += (setupDark.g - dark.g) * alpha;
					dark.b += (setupDark.b - dark.b) * alpha;
			}
			return;
		}

		let r = 0, g = 0, b = 0, a = 0, r2 = 0, g2 = 0, b2 = 0;
		let i = Timeline.search(frames, time, 7/*ENTRIES*/);
		let curveType = this.curves[i / 7/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				r2 = frames[i + 4/*R2*/];
				g2 = frames[i + 5/*G2*/];
				b2 = frames[i + 6/*B2*/];
				let t = (time - before) / (frames[i + 7/*ENTRIES*/] - before);
				r += (frames[i + 7/*ENTRIES*/ + 1/*R*/] - r) * t;
				g += (frames[i + 7/*ENTRIES*/ + 2/*G*/] - g) * t;
				b += (frames[i + 7/*ENTRIES*/ + 3/*B*/] - b) * t;
				r2 += (frames[i + 7/*ENTRIES*/ + 4/*R2*/] - r2) * t;
				g2 += (frames[i + 7/*ENTRIES*/ + 5/*G2*/] - g2) * t;
				b2 += (frames[i + 7/*ENTRIES*/ + 6/*B2*/] - b2) * t;
				break;
			case 1/*STEPPED*/:
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				r2 = frames[i + 4/*R2*/];
				g2 = frames[i + 5/*G2*/];
				b2 = frames[i + 6/*B2*/];
				break;
			default:
				r = this.getBezierValue(time, i, 1/*R*/, curveType - 2/*BEZIER*/);
				g = this.getBezierValue(time, i, 2/*G*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
				b = this.getBezierValue(time, i, 3/*B*/, curveType + 18/*BEZIER_SIZE*/ * 2 - 2/*BEZIER*/);
				r2 = this.getBezierValue(time, i, 4/*R2*/, curveType + 18/*BEZIER_SIZE*/ * 3 - 2/*BEZIER*/);
				g2 = this.getBezierValue(time, i, 5/*G2*/, curveType + 18/*BEZIER_SIZE*/ * 4 - 2/*BEZIER*/);
				b2 = this.getBezierValue(time, i, 6/*B2*/, curveType + 18/*BEZIER_SIZE*/ * 5 - 2/*BEZIER*/);
		}

		if (alpha == 1) {
			light.r = r;
			light.g = g;
			light.b = b;
			dark.r = r2;
			dark.g = g2;
			dark.b = b2;
		} else {
			if (blend == MixBlend.setup) {
				let setupLight = slot.data.color, setupDark = slot.data.darkColor!;
				light.r = setupLight.r;
				light.g = setupLight.g;
				light.b = setupLight.b;
				dark.r = setupDark.r;
				dark.g = setupDark.g;
				dark.b = setupDark.b;
			}
			light.r += (r - light.r) * alpha;
			light.g += (g - light.g) * alpha;
			light.b += (b - light.b) * alpha;
			dark.r += (r2 - dark.r) * alpha;
			dark.g += (g2 - dark.g) * alpha;
			dark.b += (b2 - dark.b) * alpha;
		}
	}
}

/** Changes a slot's {@link Slot#attachment}. */
export class AttachmentTimeline extends Timeline implements SlotTimeline {
	slotIndex = 0;

	/** The attachment name for each key frame. May contain null values to clear the attachment. */
	attachmentNames: Array<string | null>;

	constructor (frameCount: number, slotIndex: number) {
		super(frameCount, [
			Property.attachment + "|" + slotIndex
		]);
		this.slotIndex = slotIndex;
		this.attachmentNames = new Array<string>(frameCount);
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time in seconds and the attachment name for the specified key frame. */
	setFrame (frame: number, time: number, attachmentName: string | null) {
		this.frames[frame] = time;
		this.attachmentNames[frame] = attachmentName;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;

		if (direction == MixDirection.mixOut) {
			if (blend == MixBlend.setup) this.setAttachment(skeleton, slot, slot.data.attachmentName);
			return;
		}

		if (time < this.frames[0]) {
			if (blend == MixBlend.setup || blend == MixBlend.first) this.setAttachment(skeleton, slot, slot.data.attachmentName);
			return;
		}

		this.setAttachment(skeleton, slot, this.attachmentNames[Timeline.search1(this.frames, time)]);
	}

	setAttachment (skeleton: Skeleton, slot: Slot, attachmentName: string | null) {
		slot.setAttachment(!attachmentName ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
	}
}

/** Changes a slot's {@link Slot#deform} to deform a {@link VertexAttachment}. */
export class DeformTimeline extends CurveTimeline implements SlotTimeline {
	slotIndex = 0;

	/** The attachment that will be deformed. */
	attachment: VertexAttachment;

	/** The vertices for each key frame. */
	vertices: Array<NumberArrayLike>;

	constructor (frameCount: number, bezierCount: number, slotIndex: number, attachment: VertexAttachment) {
		super(frameCount, bezierCount, [
			Property.deform + "|" + slotIndex + "|" + attachment.id
		]);
		this.slotIndex = slotIndex;
		this.attachment = attachment;
		this.vertices = new Array<NumberArrayLike>(frameCount);
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time in seconds and the vertices for the specified key frame.
	 * @param vertices Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights. */
	setFrame (frame: number, time: number, vertices: NumberArrayLike) {
		this.frames[frame] = time;
		this.vertices[frame] = vertices;
	}

	/** @param value1 Ignored (0 is used for a deform timeline).
	 * @param value2 Ignored (1 is used for a deform timeline). */
	setBezier (bezier: number, frame: number, value: number, time1: number, value1: number, cx1: number, cy1: number, cx2: number,
		cy2: number, time2: number, value2: number) {
		let curves = this.curves;
		let i = this.getFrameCount() + bezier * 18/*BEZIER_SIZE*/;
		if (value == 0) curves[frame] = 2/*BEZIER*/ + i;
		let tmpx = (time1 - cx1 * 2 + cx2) * 0.03, tmpy = cy2 * 0.03 - cy1 * 0.06;
		let dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy = (cy1 - cy2 + 0.33333333) * 0.018;
		let ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
		let dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667, dy = cy1 * 0.3 + tmpy + dddy * 0.16666667;
		let x = time1 + dx, y = dy;
		for (let n = i + 18/*BEZIER_SIZE*/; i < n; i += 2) {
			curves[i] = x;
			curves[i + 1] = y;
			dx += ddx;
			dy += ddy;
			ddx += dddx;
			ddy += dddy;
			x += dx;
			y += dy;
		}
	}

	getCurvePercent (time: number, frame: number) {
		let curves = this.curves;
		let i = curves[frame];
		switch (i) {
			case 0/*LINEAR*/:
				let x = this.frames[frame];
				return (time - x) / (this.frames[frame + this.getFrameEntries()] - x);
			case 1/*STEPPED*/:
				return 0;
		}
		i -= 2/*BEZIER*/;
		if (curves[i] > time) {
			let x = this.frames[frame];
			return curves[i + 1] * (time - x) / (curves[i] - x);
		}
		let n = i + 18/*BEZIER_SIZE*/;
		for (i += 2; i < n; i += 2) {
			if (curves[i] >= time) {
				let x = curves[i - 2], y = curves[i - 1];
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
			}
		}
		let x = curves[n - 2], y = curves[n - 1];
		return y + (1 - y) * (time - x) / (this.frames[frame + this.getFrameEntries()] - x);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let slot: Slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;
		let slotAttachment: Attachment | null = slot.getAttachment();
		if (!slotAttachment) return;
		if (!(slotAttachment instanceof VertexAttachment) || (<VertexAttachment>slotAttachment).timelineAttachment != this.attachment) return;

		let deform: Array<number> = slot.deform;
		if (deform.length == 0) blend = MixBlend.setup;

		let vertices = this.vertices;
		let vertexCount = vertices[0].length;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					deform.length = 0;
					return;
				case MixBlend.first:
					if (alpha == 1) {
						deform.length = 0;
						return;
					}
					deform.length = vertexCount;
					let vertexAttachment = <VertexAttachment>slotAttachment;
					if (!vertexAttachment.bones) {
						// Unweighted vertex positions.
						let setupVertices = vertexAttachment.vertices;
						for (var i = 0; i < vertexCount; i++)
							deform[i] += (setupVertices[i] - deform[i]) * alpha;
					} else {
						// Weighted deform offsets.
						alpha = 1 - alpha;
						for (var i = 0; i < vertexCount; i++)
							deform[i] *= alpha;
					}
			}
			return;
		}

		deform.length = vertexCount;
		if (time >= frames[frames.length - 1]) { // Time is after last frame.
			let lastVertices = vertices[frames.length - 1];
			if (alpha == 1) {
				if (blend == MixBlend.add) {
					let vertexAttachment = slotAttachment as VertexAttachment;
					if (!vertexAttachment.bones) {
						// Unweighted vertex positions, with alpha.
						let setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++)
							deform[i] += lastVertices[i] - setupVertices[i];
					} else {
						// Weighted deform offsets, with alpha.
						for (let i = 0; i < vertexCount; i++)
							deform[i] += lastVertices[i];
					}
				} else
					Utils.arrayCopy(lastVertices, 0, deform, 0, vertexCount);
			} else {
				switch (blend) {
					case MixBlend.setup: {
						let vertexAttachment = slotAttachment as VertexAttachment;
						if (!vertexAttachment.bones) {
							// Unweighted vertex positions, with alpha.
							let setupVertices = vertexAttachment.vertices;
							for (let i = 0; i < vertexCount; i++) {
								let setup = setupVertices[i];
								deform[i] = setup + (lastVertices[i] - setup) * alpha;
							}
						} else {
							// Weighted deform offsets, with alpha.
							for (let i = 0; i < vertexCount; i++)
								deform[i] = lastVertices[i] * alpha;
						}
						break;
					}
					case MixBlend.first:
					case MixBlend.replace:
						for (let i = 0; i < vertexCount; i++)
							deform[i] += (lastVertices[i] - deform[i]) * alpha;
						break;
					case MixBlend.add:
						let vertexAttachment = slotAttachment as VertexAttachment;
						if (!vertexAttachment.bones) {
							// Unweighted vertex positions, with alpha.
							let setupVertices = vertexAttachment.vertices;
							for (let i = 0; i < vertexCount; i++)
								deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
						} else {
							// Weighted deform offsets, with alpha.
							for (let i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] * alpha;
						}
				}
			}
			return;
		}

		// Interpolate between the previous frame and the current frame.
		let frame = Timeline.search1(frames, time);
		let percent = this.getCurvePercent(time, frame);
		let prevVertices = vertices[frame];
		let nextVertices = vertices[frame + 1];

		if (alpha == 1) {
			if (blend == MixBlend.add) {
				let vertexAttachment = slotAttachment as VertexAttachment;
				if (!vertexAttachment.bones) {
					// Unweighted vertex positions, with alpha.
					let setupVertices = vertexAttachment.vertices;
					for (let i = 0; i < vertexCount; i++) {
						let prev = prevVertices[i];
						deform[i] += prev + (nextVertices[i] - prev) * percent - setupVertices[i];
					}
				} else {
					// Weighted deform offsets, with alpha.
					for (let i = 0; i < vertexCount; i++) {
						let prev = prevVertices[i];
						deform[i] += prev + (nextVertices[i] - prev) * percent;
					}
				}
			} else {
				for (let i = 0; i < vertexCount; i++) {
					let prev = prevVertices[i];
					deform[i] = prev + (nextVertices[i] - prev) * percent;
				}
			}
		} else {
			switch (blend) {
				case MixBlend.setup: {
					let vertexAttachment = slotAttachment as VertexAttachment;
					if (!vertexAttachment.bones) {
						// Unweighted vertex positions, with alpha.
						let setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++) {
							let prev = prevVertices[i], setup = setupVertices[i];
							deform[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (let i = 0; i < vertexCount; i++) {
							let prev = prevVertices[i];
							deform[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
						}
					}
					break;
				}
				case MixBlend.first:
				case MixBlend.replace:
					for (let i = 0; i < vertexCount; i++) {
						let prev = prevVertices[i];
						deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
					}
					break;
				case MixBlend.add:
					let vertexAttachment = slotAttachment as VertexAttachment;
					if (!vertexAttachment.bones) {
						// Unweighted vertex positions, with alpha.
						let setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++) {
							let prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent - setupVertices[i]) * alpha;
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (let i = 0; i < vertexCount; i++) {
							let prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent) * alpha;
						}
					}
			}
		}
	}
}

/** Fires an {@link Event} when specific animation times are reached. */
export class EventTimeline extends Timeline {
	static propertyIds = ["" + Property.event];

	/** The event for each key frame. */
	events: Array<Event>;

	constructor (frameCount: number) {
		super(frameCount, EventTimeline.propertyIds);

		this.events = new Array<Event>(frameCount);
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time in seconds and the event for the specified key frame. */
	setFrame (frame: number, event: Event) {
		this.frames[frame] = event.time;
		this.events[frame] = event;
	}

	/** Fires events for frames > `lastTime` and <= `time`. */
	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		if (!firedEvents) return;

		let frames = this.frames;
		let frameCount = this.frames.length;

		if (lastTime > time) { // Fire events after last time for looped animations.
			this.apply(skeleton, lastTime, Number.MAX_VALUE, firedEvents, alpha, blend, direction);
			lastTime = -1;
		} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
			return;
		if (time < frames[0]) return; // Time is before first frame.

		let i = 0;
		if (lastTime < frames[0])
			i = 0;
		else {
			i = Timeline.search1(frames, lastTime) + 1;
			let frameTime = frames[i];
			while (i > 0) { // Fire multiple events with the same frame.
				if (frames[i - 1] != frameTime) break;
				i--;
			}
		}
		for (; i < frameCount && time >= frames[i]; i++)
			firedEvents.push(this.events[i]);
	}
}

/** Changes a skeleton's {@link Skeleton#drawOrder}. */
export class DrawOrderTimeline extends Timeline {
	static propertyIds = ["" + Property.drawOrder];

	/** The draw order for each key frame. See {@link #setFrame(int, float, int[])}. */
	drawOrders: Array<Array<number> | null>;

	constructor (frameCount: number) {
		super(frameCount, DrawOrderTimeline.propertyIds);
		this.drawOrders = new Array<Array<number> | null>(frameCount);
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time in seconds and the draw order for the specified key frame.
	 * @param drawOrder For each slot in {@link Skeleton#slots}, the index of the new draw order. May be null to use setup pose
	 *           draw order. */
	setFrame (frame: number, time: number, drawOrder: Array<number> | null) {
		this.frames[frame] = time;
		this.drawOrders[frame] = drawOrder;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		if (direction == MixDirection.mixOut) {
			if (blend == MixBlend.setup) Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
			return;
		}

		if (time < this.frames[0]) {
			if (blend == MixBlend.setup || blend == MixBlend.first) Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
			return;
		}

		let idx = Timeline.search1(this.frames, time);
		let drawOrderToSetupIndex = this.drawOrders[idx];
		if (!drawOrderToSetupIndex)
			Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
		else {
			let drawOrder: Array<Slot> = skeleton.drawOrder;
			let slots: Array<Slot> = skeleton.slots;
			for (let i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
				drawOrder[i] = slots[drawOrderToSetupIndex[i]];
		}
	}
}

/** Changes an IK constraint's {@link IkConstraint#mix}, {@link IkConstraint#softness},
 * {@link IkConstraint#bendDirection}, {@link IkConstraint#stretch}, and {@link IkConstraint#compress}. */
export class IkConstraintTimeline extends CurveTimeline {
	/** The index of the IK constraint slot in {@link Skeleton#ikConstraints} that will be changed. */
	ikConstraintIndex: number = 0;

	constructor (frameCount: number, bezierCount: number, ikConstraintIndex: number) {
		super(frameCount, bezierCount, [
			Property.ikConstraint + "|" + ikConstraintIndex
		]);
		this.ikConstraintIndex = ikConstraintIndex;
	}

	getFrameEntries () {
		return 6/*ENTRIES*/;
	}

	/** Sets the time in seconds, mix, softness, bend direction, compress, and stretch for the specified key frame. */
	setFrame (frame: number, time: number, mix: number, softness: number, bendDirection: number, compress: boolean, stretch: boolean) {
		frame *= 6/*ENTRIES*/;
		this.frames[frame] = time;
		this.frames[frame + 1/*MIX*/] = mix;
		this.frames[frame + 2/*SOFTNESS*/] = softness;
		this.frames[frame + 3/*BEND_DIRECTION*/] = bendDirection;
		this.frames[frame + 4/*COMPRESS*/] = compress ? 1 : 0;
		this.frames[frame + 5/*STRETCH*/] = stretch ? 1 : 0;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let constraint: IkConstraint = skeleton.ikConstraints[this.ikConstraintIndex];
		if (!constraint.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					constraint.mix = constraint.data.mix;
					constraint.softness = constraint.data.softness;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
					return;
				case MixBlend.first:
					constraint.mix += (constraint.data.mix - constraint.mix) * alpha;
					constraint.softness += (constraint.data.softness - constraint.softness) * alpha;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
			}
			return;
		}

		let mix = 0, softness = 0;
		let i = Timeline.search(frames, time, 6/*ENTRIES*/)
		let curveType = this.curves[i / 6/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				mix = frames[i + 1/*MIX*/];
				softness = frames[i + 2/*SOFTNESS*/];
				let t = (time - before) / (frames[i + 6/*ENTRIES*/] - before);
				mix += (frames[i + 6/*ENTRIES*/ + 1/*MIX*/] - mix) * t;
				softness += (frames[i + 6/*ENTRIES*/ + 2/*SOFTNESS*/] - softness) * t;
				break;
			case 1/*STEPPED*/:
				mix = frames[i + 1/*MIX*/];
				softness = frames[i + 2/*SOFTNESS*/];
				break;
			default:
				mix = this.getBezierValue(time, i, 1/*MIX*/, curveType - 2/*BEZIER*/);
				softness = this.getBezierValue(time, i, 2/*SOFTNESS*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
		}

		if (blend == MixBlend.setup) {
			constraint.mix = constraint.data.mix + (mix - constraint.data.mix) * alpha;
			constraint.softness = constraint.data.softness + (softness - constraint.data.softness) * alpha;

			if (direction == MixDirection.mixOut) {
				constraint.bendDirection = constraint.data.bendDirection;
				constraint.compress = constraint.data.compress;
				constraint.stretch = constraint.data.stretch;
			} else {
				constraint.bendDirection = frames[i + 3/*BEND_DIRECTION*/];
				constraint.compress = frames[i + 4/*COMPRESS*/] != 0;
				constraint.stretch = frames[i + 5/*STRETCH*/] != 0;
			}
		} else {
			constraint.mix += (mix - constraint.mix) * alpha;
			constraint.softness += (softness - constraint.softness) * alpha;
			if (direction == MixDirection.mixIn) {
				constraint.bendDirection = frames[i + 3/*BEND_DIRECTION*/];
				constraint.compress = frames[i + 4/*COMPRESS*/] != 0;
				constraint.stretch = frames[i + 5/*STRETCH*/] != 0;
			}
		}
	}
}

/** Changes a transform constraint's {@link TransformConstraint#rotateMix}, {@link TransformConstraint#translateMix},
 * {@link TransformConstraint#scaleMix}, and {@link TransformConstraint#shearMix}. */
export class TransformConstraintTimeline extends CurveTimeline {
	/** The index of the transform constraint slot in {@link Skeleton#transformConstraints} that will be changed. */
	transformConstraintIndex: number = 0;

	constructor (frameCount: number, bezierCount: number, transformConstraintIndex: number) {
		super(frameCount, bezierCount, [
			Property.transformConstraint + "|" + transformConstraintIndex
		]);
		this.transformConstraintIndex = transformConstraintIndex;
	}

	getFrameEntries () {
		return 7/*ENTRIES*/;
	}

	/** The time in seconds, rotate mix, translate mix, scale mix, and shear mix for the specified key frame. */
	setFrame (frame: number, time: number, mixRotate: number, mixX: number, mixY: number, mixScaleX: number, mixScaleY: number,
		mixShearY: number) {
		let frames = this.frames;
		frame *= 7/*ENTRIES*/;
		frames[frame] = time;
		frames[frame + 1/*ROTATE*/] = mixRotate;
		frames[frame + 2/*X*/] = mixX;
		frames[frame + 3/*Y*/] = mixY;
		frames[frame + 4/*SCALEX*/] = mixScaleX;
		frames[frame + 5/*SCALEY*/] = mixScaleY;
		frames[frame + 6/*SHEARY*/] = mixShearY;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let constraint: TransformConstraint = skeleton.transformConstraints[this.transformConstraintIndex];
		if (!constraint.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			let data = constraint.data;
			switch (blend) {
				case MixBlend.setup:
					constraint.mixRotate = data.mixRotate;
					constraint.mixX = data.mixX;
					constraint.mixY = data.mixY;
					constraint.mixScaleX = data.mixScaleX;
					constraint.mixScaleY = data.mixScaleY;
					constraint.mixShearY = data.mixShearY;
					return;
				case MixBlend.first:
					constraint.mixRotate += (data.mixRotate - constraint.mixRotate) * alpha;
					constraint.mixX += (data.mixX - constraint.mixX) * alpha;
					constraint.mixY += (data.mixY - constraint.mixY) * alpha;
					constraint.mixScaleX += (data.mixScaleX - constraint.mixScaleX) * alpha;
					constraint.mixScaleY += (data.mixScaleY - constraint.mixScaleY) * alpha;
					constraint.mixShearY += (data.mixShearY - constraint.mixShearY) * alpha;
			}
			return;
		}

		let rotate, x, y, scaleX, scaleY, shearY;
		let i = Timeline.search(frames, time, 7/*ENTRIES*/);
		let curveType = this.curves[i / 7/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				rotate = frames[i + 1/*ROTATE*/];
				x = frames[i + 2/*X*/];
				y = frames[i + 3/*Y*/];
				scaleX = frames[i + 4/*SCALEX*/];
				scaleY = frames[i + 5/*SCALEY*/];
				shearY = frames[i + 6/*SHEARY*/];
				let t = (time - before) / (frames[i + 7/*ENTRIES*/] - before);
				rotate += (frames[i + 7/*ENTRIES*/ + 1/*ROTATE*/] - rotate) * t;
				x += (frames[i + 7/*ENTRIES*/ + 2/*X*/] - x) * t;
				y += (frames[i + 7/*ENTRIES*/ + 3/*Y*/] - y) * t;
				scaleX += (frames[i + 7/*ENTRIES*/ + 4/*SCALEX*/] - scaleX) * t;
				scaleY += (frames[i + 7/*ENTRIES*/ + 5/*SCALEY*/] - scaleY) * t;
				shearY += (frames[i + 7/*ENTRIES*/ + 6/*SHEARY*/] - shearY) * t;
				break;
			case 1/*STEPPED*/:
				rotate = frames[i + 1/*ROTATE*/];
				x = frames[i + 2/*X*/];
				y = frames[i + 3/*Y*/];
				scaleX = frames[i + 4/*SCALEX*/];
				scaleY = frames[i + 5/*SCALEY*/];
				shearY = frames[i + 6/*SHEARY*/];
				break;
			default:
				rotate = this.getBezierValue(time, i, 1/*ROTATE*/, curveType - 2/*BEZIER*/);
				x = this.getBezierValue(time, i, 2/*X*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
				y = this.getBezierValue(time, i, 3/*Y*/, curveType + 18/*BEZIER_SIZE*/ * 2 - 2/*BEZIER*/);
				scaleX = this.getBezierValue(time, i, 4/*SCALEX*/, curveType + 18/*BEZIER_SIZE*/ * 3 - 2/*BEZIER*/);
				scaleY = this.getBezierValue(time, i, 5/*SCALEY*/, curveType + 18/*BEZIER_SIZE*/ * 4 - 2/*BEZIER*/);
				shearY = this.getBezierValue(time, i, 6/*SHEARY*/, curveType + 18/*BEZIER_SIZE*/ * 5 - 2/*BEZIER*/);
		}

		if (blend == MixBlend.setup) {
			let data = constraint.data;
			constraint.mixRotate = data.mixRotate + (rotate - data.mixRotate) * alpha;
			constraint.mixX = data.mixX + (x - data.mixX) * alpha;
			constraint.mixY = data.mixY + (y - data.mixY) * alpha;
			constraint.mixScaleX = data.mixScaleX + (scaleX - data.mixScaleX) * alpha;
			constraint.mixScaleY = data.mixScaleY + (scaleY - data.mixScaleY) * alpha;
			constraint.mixShearY = data.mixShearY + (shearY - data.mixShearY) * alpha;
		} else {
			constraint.mixRotate += (rotate - constraint.mixRotate) * alpha;
			constraint.mixX += (x - constraint.mixX) * alpha;
			constraint.mixY += (y - constraint.mixY) * alpha;
			constraint.mixScaleX += (scaleX - constraint.mixScaleX) * alpha;
			constraint.mixScaleY += (scaleY - constraint.mixScaleY) * alpha;
			constraint.mixShearY += (shearY - constraint.mixShearY) * alpha;
		}
	}
}

/** Changes a path constraint's {@link PathConstraint#position}. */
export class PathConstraintPositionTimeline extends CurveTimeline1 {
	/** The index of the path constraint slot in {@link Skeleton#pathConstraints} that will be changed. */
	pathConstraintIndex: number = 0;

	constructor (frameCount: number, bezierCount: number, pathConstraintIndex: number) {
		super(frameCount, bezierCount, Property.pathConstraintPosition + "|" + pathConstraintIndex);
		this.pathConstraintIndex = pathConstraintIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];
		if (!constraint.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					constraint.position = constraint.data.position;
					return;
				case MixBlend.first:
					constraint.position += (constraint.data.position - constraint.position) * alpha;
			}
			return;
		}

		let position = this.getCurveValue(time);

		if (blend == MixBlend.setup)
			constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
		else
			constraint.position += (position - constraint.position) * alpha;
	}
}

/** Changes a path constraint's {@link PathConstraint#spacing}. */
export class PathConstraintSpacingTimeline extends CurveTimeline1 {
	/** The index of the path constraint slot in {@link Skeleton#getPathConstraints()} that will be changed. */
	pathConstraintIndex = 0;

	constructor (frameCount: number, bezierCount: number, pathConstraintIndex: number) {
		super(frameCount, bezierCount, Property.pathConstraintSpacing + "|" + pathConstraintIndex);
		this.pathConstraintIndex = pathConstraintIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];
		if (!constraint.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					constraint.spacing = constraint.data.spacing;
					return;
				case MixBlend.first:
					constraint.spacing += (constraint.data.spacing - constraint.spacing) * alpha;
			}
			return;
		}

		let spacing = this.getCurveValue(time);

		if (blend == MixBlend.setup)
			constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
		else
			constraint.spacing += (spacing - constraint.spacing) * alpha;
	}
}

/** Changes a transform constraint's {@link PathConstraint#getMixRotate()}, {@link PathConstraint#getMixX()}, and
 * {@link PathConstraint#getMixY()}. */
export class PathConstraintMixTimeline extends CurveTimeline {
	/** The index of the path constraint slot in {@link Skeleton#getPathConstraints()} that will be changed. */
	pathConstraintIndex = 0;

	constructor (frameCount: number, bezierCount: number, pathConstraintIndex: number) {
		super(frameCount, bezierCount, [
			Property.pathConstraintMix + "|" + pathConstraintIndex
		]);
		this.pathConstraintIndex = pathConstraintIndex;
	}

	getFrameEntries () {
		return 4/*ENTRIES*/;
	}

	setFrame (frame: number, time: number, mixRotate: number, mixX: number, mixY: number) {
		let frames = this.frames;
		frame <<= 2;
		frames[frame] = time;
		frames[frame + 1/*ROTATE*/] = mixRotate;
		frames[frame + 2/*X*/] = mixX;
		frames[frame + 3/*Y*/] = mixY;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];
		if (!constraint.active) return;

		let frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					constraint.mixRotate = constraint.data.mixRotate;
					constraint.mixX = constraint.data.mixX;
					constraint.mixY = constraint.data.mixY;
					return;
				case MixBlend.first:
					constraint.mixRotate += (constraint.data.mixRotate - constraint.mixRotate) * alpha;
					constraint.mixX += (constraint.data.mixX - constraint.mixX) * alpha;
					constraint.mixY += (constraint.data.mixY - constraint.mixY) * alpha;
			}
			return;
		}

		let rotate, x, y;
		let i = Timeline.search(frames, time, 4/*ENTRIES*/);
		let curveType = this.curves[i >> 2];
		switch (curveType) {
			case 0/*LINEAR*/:
				let before = frames[i];
				rotate = frames[i + 1/*ROTATE*/];
				x = frames[i + 2/*X*/];
				y = frames[i + 3/*Y*/];
				let t = (time - before) / (frames[i + 4/*ENTRIES*/] - before);
				rotate += (frames[i + 4/*ENTRIES*/ + 1/*ROTATE*/] - rotate) * t;
				x += (frames[i + 4/*ENTRIES*/ + 2/*X*/] - x) * t;
				y += (frames[i + 4/*ENTRIES*/ + 3/*Y*/] - y) * t;
				break;
			case 1/*STEPPED*/:
				rotate = frames[i + 1/*ROTATE*/];
				x = frames[i + 2/*X*/];
				y = frames[i + 3/*Y*/];
				break;
			default:
				rotate = this.getBezierValue(time, i, 1/*ROTATE*/, curveType - 2/*BEZIER*/);
				x = this.getBezierValue(time, i, 2/*X*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
				y = this.getBezierValue(time, i, 3/*Y*/, curveType + 18/*BEZIER_SIZE*/ * 2 - 2/*BEZIER*/);
		}

		if (blend == MixBlend.setup) {
			let data = constraint.data;
			constraint.mixRotate = data.mixRotate + (rotate - data.mixRotate) * alpha;
			constraint.mixX = data.mixX + (x - data.mixX) * alpha;
			constraint.mixY = data.mixY + (y - data.mixY) * alpha;
		} else {
			constraint.mixRotate += (rotate - constraint.mixRotate) * alpha;
			constraint.mixX += (x - constraint.mixX) * alpha;
			constraint.mixY += (y - constraint.mixY) * alpha;
		}
	}
}

/** Changes a slot's {@link Slot#getSequenceIndex()} for an attachment's {@link Sequence}. */
export class SequenceTimeline extends Timeline implements SlotTimeline {
	static ENTRIES = 3;
	static MODE = 1;
	static DELAY = 2;

	slotIndex: number;
	attachment: HasTextureRegion;

	constructor (frameCount: number, slotIndex: number, attachment: HasTextureRegion) {
		super(frameCount, [
			Property.sequence + "|" + slotIndex + "|" + attachment.sequence!.id
		]);
		this.slotIndex = slotIndex;
		this.attachment = attachment;
	}

	getFrameEntries () {
		return SequenceTimeline.ENTRIES;
	}

	getSlotIndex () {
		return this.slotIndex;
	}

	getAttachment () {
		return this.attachment as unknown as Attachment;
	}

	/** Sets the time, mode, index, and frame time for the specified frame.
	 * @param frame Between 0 and <code>frameCount</code>, inclusive.
	 * @param time Seconds between frames. */
	setFrame (frame: number, time: number, mode: SequenceMode, index: number, delay: number) {
		let frames = this.frames;
		frame *= SequenceTimeline.ENTRIES;
		frames[frame] = time;
		frames[frame + SequenceTimeline.MODE] = mode | (index << 4);
		frames[frame + SequenceTimeline.DELAY] = delay;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
		let slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;
		let slotAttachment = slot.attachment;
		let attachment = this.attachment as unknown as Attachment;
		if (slotAttachment != attachment) {
			if (!(slotAttachment instanceof VertexAttachment)
				|| (slotAttachment as VertexAttachment).timelineAttachment != attachment) return;
		}

		let frames = this.frames;
		if (time < frames[0]) { // Time is before first frame.
			if (blend == MixBlend.setup || blend == MixBlend.first) slot.sequenceIndex = -1;
			return;
		}

		let i = Timeline.search(frames, time, SequenceTimeline.ENTRIES);
		let before = frames[i];
		let modeAndIndex = frames[i + SequenceTimeline.MODE];
		let delay = frames[i + SequenceTimeline.DELAY];

		if (!this.attachment.sequence) return;
		let index = modeAndIndex >> 4, count = this.attachment.sequence!.regions.length;
		let mode = SequenceModeValues[modeAndIndex & 0xf];
		if (mode != SequenceMode.hold) {
			index += (((time - before) / delay + 0.00001) | 0);
			switch (mode) {
				case SequenceMode.once:
					index = Math.min(count - 1, index);
					break;
				case SequenceMode.loop:
					index %= count;
					break;
				case SequenceMode.pingpong: {
					let n = (count << 1) - 2;
					index = n == 0 ? 0 : index % n;
					if (index >= count) index = n - index;
					break;
				}
				case SequenceMode.onceReverse:
					index = Math.max(count - 1 - index, 0);
					break;
				case SequenceMode.loopReverse:
					index = count - 1 - (index % count);
					break;
				case SequenceMode.pingpongReverse: {
					let n = (count << 1) - 2;
					index = n == 0 ? 0 : (index + count - 1) % n;
					if (index >= count) index = n - index;
				}
			}
		}
		slot.sequenceIndex = index;
	}
}