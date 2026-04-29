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

import type { Attachment, VertexAttachment } from "./attachments/Attachment.js";
import type { HasSequence } from "./attachments/HasSequence.js";
import { SequenceMode, SequenceModeValues } from "./attachments/Sequence.js";
import type { Bone } from "./Bone.js";
import type { Inherit } from "./BoneData.js";
import type { BonePose } from "./BonePose.js";
import type { Event } from "./Event.js";
import type { IkConstraint } from "./IkConstraint.js";
import type { IkConstraintPose } from "./IkConstraintPose.js";
import type { PathConstraint } from "./PathConstraint.js";
import type { PathConstraintPose } from "./PathConstraintPose.js";
import type { PhysicsConstraint } from "./PhysicsConstraint.js";
import type { PhysicsConstraintData } from "./PhysicsConstraintData.js";
import type { PhysicsConstraintPose } from "./PhysicsConstraintPose.js";
import type { Posed } from "./Posed.js";
import type { Skeleton } from "./Skeleton.js";
import type { SkeletonData } from "./SkeletonData.js";
import type { Slider } from "./Slider.js";
import type { SliderPose } from "./SliderPose.js";
import type { Slot } from "./Slot.js";
import type { SlotPose } from "./SlotPose.js";
import type { TransformConstraint } from "./TransformConstraint.js";
import type { TransformConstraintPose } from "./TransformConstraintPose.js";
import { Color, type NumberArrayLike, StringSet, Utils } from "./Utils.js";

/** Stores a list of timelines to animate a skeleton's pose over time.
 *
 * See <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine Runtimes
 * Guide. */
export class Animation {
	/** The animation's name, unique across all animations in the skeleton.
	 *
	 * See {@link SkeletonData.findAnimation}. */
	readonly name: string;

	/** The duration of the animation in seconds, which is usually the highest time of all frames in the timelines. The duration is
	 * used to know when the animation has completed and, for animations that repeat, when it should loop back to the start. */
	timelines: Array<Timeline> = [];

	readonly timelineIds: StringSet;

	/** {@link Skeleton.getBones} indices that this animation's timelines modify.
	 *
	 * See {@link BoneTimeline.bones}. */
	readonly bones: Array<number>;

	// Nonessential.
	/** The color of the animation as it was in Spine, or a default color if nonessential data was not exported. */
	readonly color = new Color(1, 1, 1, 1);

	/** The duration of the animation in seconds, which is usually the highest time of all frames in the timeline. The duration is
	 * used to know when it has completed and when it should loop back to the start. */
	duration: number;

	constructor (name: string, timelines: Array<Timeline>, duration: number) {
		if (!name) throw new Error("name cannot be null.");
		this.name = name;
		this.duration = duration;
		this.timelineIds = new StringSet();
		this.bones = [] as number[];
		this.setTimelines(timelines);
	}

	setTimelines (timelines: Array<Timeline>) {
		if (!timelines) throw new Error("timelines cannot be null.");
		this.timelines = timelines;

		const n = timelines.length;
		this.timelineIds.clear();
		this.bones.length = 0;
		const boneSet = new Set();
		const items = timelines;
		for (let i = 0; i < n; i++) {
			const timeline = items[i];
			this.timelineIds.addAll(timeline.propertyIds);
			if (isBoneTimeline(timeline) && boneSet.add(timeline.boneIndex))
				this.bones.push(timeline.boneIndex);
		}
	}

	/** Returns true if this animation contains a timeline with any of the specified property IDs.
	 *
	 * See {@link Timeline.propertyIds}. */
	hasTimeline (ids: string[]): boolean {
		for (let i = 0; i < ids.length; i++)
			if (this.timelineIds.contains(ids[i])) return true;
		return false;
	}

	/** Applies the animation's timelines to the specified skeleton.
	 *
	 * See {@link Timeline.apply} and
	 * <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine Runtimes
	 * Guide.
	 * @param skeleton The skeleton the animation is applied to. This provides access to the bones, slots, and other skeleton
	 *           components the timelines may change.
	 * @param lastTime The last time in seconds this animation was applied. Some timelines trigger only at discrete times, in which
	 *           case all keys are triggered between `lastTime` (exclusive) and `time` (inclusive). Pass -1
	 *           the first time an animation is applied to ensure frame 0 is triggered.
	 * @param time The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and
	 *           interpolate between the frame values.
	 * @param loop True if `time` beyond the {@link duration} repeats the animation, else the last frame is used.
	 * @param events If any events are fired, they are added to this list. Pass null to ignore fired events or if no timelines fire
	 *           events.
	 * @param alpha 0 applies setup or current values (depending on `fromSetup`), 1 uses timeline values, and
	 *           intermediate values interpolate between them. Adjusting `alpha` over time can mix an animation in or
	 *           out.
	 * @param fromSetup If true, `alpha` transitions between setup and timeline values, setup values are used before the
	 *           first frame (current values are not used). If false, `alpha` transitions between current and timeline
	 *           values, no change is made before the first frame.
	 * @param add If true, for timelines that support it, their values are added to the setup or current values (depending on
	 *           `fromSetup`).
	 * @param out True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant transitions.
	 * @param appliedPose True to modify {@link Posed.appliedPose}, else {@link Posed.pose} is modified. */
	apply (skeleton: Skeleton, lastTime: number, time: number, loop: boolean, events: Array<Event> | null, alpha: number,
		fromSetup: boolean, add: boolean, out: boolean, appliedPose: boolean) {
		if (!skeleton) throw new Error("skeleton cannot be null.");

		if (loop && this.duration !== 0) {
			time %= this.duration;
			if (lastTime > 0) lastTime %= this.duration;
		}

		const timelines = this.timelines;
		for (let i = 0, n = timelines.length; i < n; i++)
			timelines[i].apply(skeleton, lastTime, time, events, alpha, fromSetup, add, out, appliedPose);
	}
}


export enum Property {
	rotate,
	x,
	y,
	scaleX,
	scaleY,
	shearX,
	shearY,
	inherit,
	rgb,
	alpha,
	rgb2,
	attachment,
	deform,
	event,
	drawOrder,
	ikConstraint,
	transformConstraint,
	pathConstraintPosition,
	pathConstraintSpacing,
	pathConstraintMix,
	physicsConstraintInertia,
	physicsConstraintStrength,
	physicsConstraintDamping,
	physicsConstraintMass,
	physicsConstraintWind,
	physicsConstraintGravity,
	physicsConstraintMix,
	physicsConstraintReset,
	sequence,
	sliderTime,
	sliderMix,
}

/** The base class for all timelines.
 *
 * See <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine
 * Runtimes Guide. */
export abstract class Timeline {
	readonly propertyIds: string[];
	readonly frames: NumberArrayLike;

	/** True if this timeline supports additive blending. */
	additive = false;

	/** True if this timeline sets values instantaneously and does not support interpolation between frames. */
	instant = false;

	constructor (frameCount: number, ...propertyIds: string[]) {
		this.propertyIds = propertyIds;
		this.frames = Utils.newFloatArray(frameCount * this.getFrameEntries());
	}

	getPropertyIds () {
		return this.propertyIds;
	}

	/** The number of values stored per frame. */
	getFrameEntries (): number {
		return 1;
	}

	/** The number of frames in this timeline. */
	getFrameCount () {
		return this.frames.length / this.getFrameEntries();
	}

	/** The duration of the timeline in seconds, which is usually the highest time of all frames in the timeline. */
	getDuration (): number {
		return this.frames[this.frames.length - this.getFrameEntries()];
	}

	/** Applies this timeline to the skeleton.
	 *
	 * See <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine
	 * Runtimes Guide.
	 * @param skeleton The skeleton the timeline is applied to. This provides access to the bones, slots, and other skeleton
	 *           components the timelines may change.
	 * @param lastTime The last time in seconds this timeline was applied. Some timelines trigger only at discrete times, in
	 *           which case all keys are triggered between `lastTime` (exclusive) and `time` (inclusive).
	 *           Pass -1 the first time a timeline is applied to ensure frame 0 is triggered.
	 * @param time The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and
	 *           interpolate between the frame values.
	 * @param events If any events are fired, they are added to this list. Pass null to ignore fired events or if no timelines
	 *           fire events.
	 * @param alpha 0 applies setup or current values (depending on `fromSetup`), 1 uses timeline values, and
	 *           intermediate values interpolate between them. Adjusting `alpha` over time can mix a timeline in or
	 *           out.
	 * @param fromSetup If true, `alpha` transitions between setup and timeline values, setup values are used before
	 *           the first frame (current values are not used). If false, `alpha` transitions between current and
	 *           timeline values, no change is made before the first frame.
	 * @param add If true, for timelines that support it, their values are added to the setup or current values (depending on
	 *           `fromSetup`).
	 * @param out True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant
	 *           transitions.
	 * @param appliedPose True to modify {@link Posed.appliedPose}, else {@link Posed.pose} is modified. */
	abstract apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event> | null, alpha: number,
		fromSetup: boolean, add: boolean, out: boolean, appliedPose: boolean): void;

	/** Linear search using the specified stride (default 1).
	 * @param time Must be >= the first value in `frames`.
	 * @return The index of the first value <= `time`. */
	static search (frames: NumberArrayLike, time: number, step = 1) {
		const n = frames.length;
		for (let i = step; i < n; i += step)
			if (frames[i] > time) return i - step;
		return n - step;
	}
}

/** An interface for timelines that change a slot's properties. */
export interface SlotTimeline {
	/** The index of the slot in {@link Skeleton.slots} that will be changed when this timeline is applied. */
	slotIndex: number;
}

export function isSlotTimeline (obj: Timeline & Partial<SlotTimeline>): obj is Timeline & SlotTimeline {
	return typeof obj === 'object' && obj !== null && typeof obj.slotIndex === 'number';
}

/** The base class for timelines that interpolate between frame values using stepped, linear, or a Bezier curve. */
export abstract class CurveTimeline extends Timeline {
	protected curves: NumberArrayLike; // type, x, y, ...

	constructor (frameCount: number, bezierCount: number, ...propertyIds: string[]) {
		super(frameCount, ...propertyIds);
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

	/** Shrinks the storage for Bezier curves, for use when `bezierCount` (specified in the constructor) was larger
	 * than the actual number of Bezier curves. */
	shrink (bezierCount: number) {
		const size = this.getFrameCount() + bezierCount * 18/*BEZIER_SIZE*/;
		if (this.curves.length > size) {
			const newCurves = Utils.newFloatArray(size);
			Utils.arrayCopy(this.curves, 0, newCurves, 0, size);
			this.curves = newCurves;
		}
	}

	/** Stores the segments for the specified Bezier curve. For timelines that modify multiple values, there may be more than
	 * one curve per frame.
	 * @param bezier The ordinal of this Bezier curve for this timeline, between 0 and `bezierCount - 1` (specified
	 *           in the constructor), inclusive.
	 * @param frame Between 0 and `frameCount - 1`, inclusive.
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
		const curves = this.curves;
		let i = this.getFrameCount() + bezier * 18/*BEZIER_SIZE*/;
		if (value === 0) curves[frame] = 2/*BEZIER*/ + i;
		const tmpx = (time1 - cx1 * 2 + cx2) * 0.03, tmpy = (value1 - cy1 * 2 + cy2) * 0.03;
		const dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy = ((cy1 - cy2) * 3 - value1 + value2) * 0.006;
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
	 * @param frameIndex The index into {@link frames} for the values of the frame before `time`.
	 * @param valueOffset The offset from `frameIndex` to the value this curve is used for.
	 * @param i The index of the Bezier segments. See {@link getCurveType}. */
	getBezierValue (time: number, frameIndex: number, valueOffset: number, i: number) {
		const curves = this.curves;
		if (curves[i] > time) {
			const x = this.frames[frameIndex], y = this.frames[frameIndex + valueOffset];
			return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
		}
		const n = i + 18/*BEZIER_SIZE*/;
		for (i += 2; i < n; i += 2) {
			if (curves[i] >= time) {
				const x = curves[i - 2], y = curves[i - 1];
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
			}
		}
		frameIndex += this.getFrameEntries();
		const x = curves[n - 2], y = curves[n - 1];
		return y + (time - x) / (this.frames[frameIndex] - x) * (this.frames[frameIndex + valueOffset] - y);
	}
}

/** The base class for a {@link CurveTimeline} that sets one property with a curve. */
export abstract class CurveTimeline1 extends CurveTimeline {
	constructor (frameCount: number, bezierCount: number, propertyId: string) {
		super(frameCount, bezierCount, propertyId);
	}

	getFrameEntries () {
		return 2/*ENTRIES*/;
	}

	/** Sets the time and value for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive.
	 * @param time The frame time in seconds. */
	setFrame (frame: number, time: number, value: number) {
		frame <<= 1;
		this.frames[frame] = time;
		this.frames[frame + 1/*VALUE*/] = value;
	}

	/** Returns the interpolated value for the specified time. */
	getCurveValue (time: number) {
		const frames = this.frames;
		let i = frames.length - 2;
		for (let ii = 2; ii <= i; ii += 2) {
			if (frames[ii] > time) {
				i = ii - 2;
				break;
			}
		}

		const curveType = this.curves[i >> 1];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i], value = frames[i + 1/*VALUE*/];
				return value + (time - before) / (frames[i + 2/*ENTRIES*/] - before) * (frames[i + 2/*ENTRIES*/ + 1/*VALUE*/] - value);
			}
			case 1/*STEPPED*/:
				return frames[i + 1/*VALUE*/];
		}
		return this.getBezierValue(time, i, 1/*VALUE*/, curveType - 2/*BEZIER*/);
	}

	/** Returns the interpolated value for properties relative to the setup value. The timeline value is added to the setup
	 * value, rather than replacing it.
	 *
	 * See {@link Timeline.apply}.
	 * @param current The current value for the property.
	 * @param setup The setup value for the property. */
	getRelativeValue (time: number, alpha: number, fromSetup: boolean, add: boolean, current: number, setup: number) {
		if (time < this.frames[0]) return fromSetup ? setup : current;
		const value = this.getCurveValue(time);
		return fromSetup ? setup + value * alpha : current + (add ? value : value + setup - current) * alpha;
	}

	/** Returns the interpolated value for properties set as absolute values. The timeline value replaces the setup value,
	 * rather than being relative to it.
	 *
	 * See {@link Timeline.apply}.
	 * @param current The current value for the property.
	 * @param setup The setup value for the property. */
	getAbsoluteValue (time: number, alpha: number, fromSetup: boolean, add: boolean, current: number, setup: number): number;

	/** Returns the interpolated value for properties set as absolute values, using the specified timeline value rather than
	 * calling {@link getCurveValue}.
	 *
	 * See {@link Timeline.apply}.
	 * @param current The current value for the property.
	 * @param setup The setup value for the property.
	 * @param value The timeline value to apply. */
	getAbsoluteValue (time: number, alpha: number, fromSetup: boolean, add: boolean, current: number, setup: number, value: number): number;

	getAbsoluteValue (time: number, alpha: number, fromSetup: boolean, add: boolean, current: number, setup: number, value?: number) {
		if (value === undefined)
			return this.getAbsoluteValue1(time, alpha, fromSetup, add, current, setup);
		else
			return this.getAbsoluteValue2(time, alpha, fromSetup, add, current, setup, value);
	}

	private getAbsoluteValue1 (time: number, alpha: number, fromSetup: boolean, add: boolean, current: number, setup: number) {
		if (time < this.frames[0]) return fromSetup ? setup : current;
		const value = this.getCurveValue(time);
		return fromSetup ? setup + (add ? value : value - setup) * alpha : current + (add ? value : value - current) * alpha;
	}

	private getAbsoluteValue2 (time: number, alpha: number, fromSetup: boolean, add: boolean, current: number, setup: number, value: number) {
		if (time < this.frames[0]) return fromSetup ? setup : current;
		return fromSetup ? setup + (add ? value : value - setup) * alpha : current + (add ? value : value - current) * alpha;
	}

	/** Returns the interpolated value for scale properties. The timeline and setup values are multiplied and sign adjusted.
	 *
	 * See {@link Timeline.apply}.
	 * @param current The current value for the property.
	 * @param setup The setup value for the property. */
	getScaleValue (time: number, alpha: number, fromSetup: boolean, add: boolean, out: boolean, current: number, setup: number) {
		if (time < this.frames[0]) return fromSetup ? setup : current;
		const value = this.getCurveValue(time) * setup;
		if (alpha === 1 && !add) return value;
		let base = fromSetup ? setup : current;
		if (add) return base + (value - setup) * alpha;
		if (out) return base + (Math.abs(value) * Math.sign(base) - base) * alpha;
		base = Math.abs(base) * Math.sign(value);
		return base + (value - base) * alpha;
	}
}

/** An interface for timelines that change a bone's properties. */
export interface BoneTimeline {
	/** The index of the bone in {@link Skeleton.bones} that is changed by this timeline. */
	boneIndex: number;
}

export function isBoneTimeline (obj: Timeline & Partial<BoneTimeline>): obj is Timeline & BoneTimeline {
	return typeof obj === 'object' && obj !== null && typeof obj.boneIndex === 'number';
}

/** The base class for timelines that change 1 bone property with a curve. */
export abstract class BoneTimeline1 extends CurveTimeline1 implements BoneTimeline {
	readonly boneIndex: number;

	constructor (frameCount: number, bezierCount: number, boneIndex: number, property: Property) {
		super(frameCount, bezierCount, `${property}|${boneIndex}`);
		this.boneIndex = boneIndex;
		this.additive = true;
	}

	public apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event> | null, alpha: number,
		fromSetup: boolean, add: boolean, out: boolean, appliedPose: boolean) {
		const bone = skeleton.bones[this.boneIndex];
		if (bone.active)
			this.apply1(appliedPose ? bone.appliedPose : bone.pose, bone.data.setupPose, time, alpha, fromSetup, add, out);
	}

	protected abstract apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean): void;
}

/** The base class for timelines that change two bone properties with a curve. */
export abstract class BoneTimeline2 extends CurveTimeline implements BoneTimeline {
	readonly boneIndex;

	/** @param bezierCount The maximum number of Bezier curves. See {@link shrink}.
	 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
	constructor (frameCount: number, bezierCount: number, boneIndex: number, property1: Property, property2: Property) {
		super(frameCount, bezierCount, `${property1}|${boneIndex}`, `${property2}|${boneIndex}`);
		this.boneIndex = boneIndex;
		this.additive = true;
	}

	getFrameEntries () {
		return 3/*ENTRIES*/;
	}

	/** Sets the time and values for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive.
	 * @param time The frame time in seconds. */
	setFrame (frame: number, time: number, value1: number, value2: number) {
		frame *= 3/*ENTRIES*/;
		this.frames[frame] = time;
		this.frames[frame + 1/*VALUE1*/] = value1;
		this.frames[frame + 2/*VALUE2*/] = value2;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event> | null, alpha: number,
		fromSetup: boolean, add: boolean, out: boolean, appliedPose: boolean): void {
		const bone = skeleton.bones[this.boneIndex];
		if (bone.active)
			this.apply1(appliedPose ? bone.appliedPose : bone.pose, bone.data.setupPose, time, alpha, fromSetup, add, out);
	}

	protected abstract apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean,): void;
}

/** Changes {@link BonePose.rotation}. */
export class RotateTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.rotate);
	}

	apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean) {
		pose.rotation = this.getRelativeValue(time, alpha, fromSetup, add, pose.rotation, setup.rotation);
	}
}

/** Changes {@link BonePose.x} and {@link BonePose.y}. */
export class TranslateTimeline extends BoneTimeline2 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.x, Property.y);
	}

	apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean) {
		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) {
				pose.x = setup.x;
				pose.y = setup.y;
			}
			return;
		}

		let x = 0, y = 0;
		const i = Timeline.search(frames, time, 3/*ENTRIES*/);
		const curveType = this.curves[i / 3/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				const t = (time - before) / (frames[i + 3/*ENTRIES*/] - before);
				x += (frames[i + 3/*ENTRIES*/ + 1/*VALUE1*/] - x) * t;
				y += (frames[i + 3/*ENTRIES*/ + 2/*VALUE2*/] - y) * t;
				break;
			}
			case 1/*STEPPED*/:
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				break;
			default:
				x = this.getBezierValue(time, i, 1/*VALUE1*/, curveType - 2/*BEZIER*/);
				y = this.getBezierValue(time, i, 2/*VALUE2*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
		}

		if (fromSetup) {
			pose.x = setup.x + x * alpha;
			pose.y = setup.y + y * alpha;
		} else if (add) {
			pose.x += x * alpha;
			pose.y += y * alpha;
		} else {
			pose.x += (setup.x + x - pose.x) * alpha;
			pose.y += (setup.y + y - pose.y) * alpha;
		}
	}
}

/** Changes {@link BonePose.x}. */
export class TranslateXTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.x);
	}

	protected apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean) {
		pose.x = this.getRelativeValue(time, alpha, fromSetup, add, pose.x, setup.x);
	}
}

/** Changes {@link BonePose.y}. */
export class TranslateYTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.y);
	}

	protected apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean) {
		pose.y = this.getRelativeValue(time, alpha, fromSetup, add, pose.y, setup.y);
	}
}

/** Changes {@link BonePose.scaleX} and {@link BonePose.scaleY}. */
export class ScaleTimeline extends BoneTimeline2 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.scaleX, Property.scaleY);
	}

	protected apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean) {
		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) {
				pose.scaleX = setup.scaleX;
				pose.scaleY = setup.scaleY;
			}
			return;
		}

		let x: number, y: number;
		const i = Timeline.search(frames, time, 3/*ENTRIES*/);
		const curveType = this.curves[i / 3/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				const t = (time - before) / (frames[i + 3/*ENTRIES*/] - before);
				x += (frames[i + 3/*ENTRIES*/ + 1/*VALUE1*/] - x) * t;
				y += (frames[i + 3/*ENTRIES*/ + 2/*VALUE2*/] - y) * t;
				break;
			}
			case 1/*STEPPED*/:
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				break;
			default:
				x = this.getBezierValue(time, i, 1/*VALUE1*/, curveType - 2/*BEZIER*/);
				y = this.getBezierValue(time, i, 2/*VALUE2*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
		}
		x *= setup.scaleX;
		y *= setup.scaleY;

		if (alpha === 1 && !add) {
			pose.scaleX = x;
			pose.scaleY = y;
		} else {
			let bx = 0, by = 0;
			if (fromSetup) {
				bx = setup.scaleX;
				by = setup.scaleY;
			} else {
				bx = pose.scaleX;
				by = pose.scaleY;
			}
			if (add) {
				pose.scaleX = bx + (x - setup.scaleX) * alpha;
				pose.scaleY = by + (y - setup.scaleY) * alpha;
			} else if (out) {
				pose.scaleX = bx + (Math.abs(x) * Math.sign(bx) - bx) * alpha;
				pose.scaleY = by + (Math.abs(y) * Math.sign(by) - by) * alpha;
			} else {
				bx = Math.abs(bx) * Math.sign(x);
				by = Math.abs(by) * Math.sign(y);
				pose.scaleX = bx + (x - bx) * alpha;
				pose.scaleY = by + (y - by) * alpha;
			}
		}
	}
}

/** Changes a {@link BonePose.scaleX}. */
export class ScaleXTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.scaleX);
	}

	protected apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean) {
		pose.scaleX = this.getScaleValue(time, alpha, fromSetup, add, out, pose.scaleX, setup.scaleX);
	}
}

/** Changes a {@link BonePose.scaleY}. */
export class ScaleYTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.scaleY);
	}

	protected apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean) {
		pose.scaleY = this.getScaleValue(time, alpha, fromSetup, add, out, pose.scaleY, setup.scaleY);
	}
}

/** Changes {@link Bone.shearX} and {@link Bone.shearY}. */
export class ShearTimeline extends BoneTimeline2 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.shearX, Property.shearY);
	}

	protected apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean, out: boolean,) {
		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) {
				pose.shearX = setup.shearX;
				pose.shearY = setup.shearY;
			}
			return;
		}

		let x = 0, y = 0;
		const i = Timeline.search(frames, time, 3/*ENTRIES*/);
		const curveType = this.curves[i / 3/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				const t = (time - before) / (frames[i + 3/*ENTRIES*/] - before);
				x += (frames[i + 3/*ENTRIES*/ + 1/*VALUE1*/] - x) * t;
				y += (frames[i + 3/*ENTRIES*/ + 2/*VALUE2*/] - y) * t;
				break;
			}
			case 1/*STEPPED*/:
				x = frames[i + 1/*VALUE1*/];
				y = frames[i + 2/*VALUE2*/];
				break;
			default:
				x = this.getBezierValue(time, i, 1/*VALUE1*/, curveType - 2/*BEZIER*/);
				y = this.getBezierValue(time, i, 2/*VALUE2*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
		}

		if (fromSetup) {
			pose.shearX = setup.shearX + x * alpha;
			pose.shearY = setup.shearY + y * alpha;
		} else if (add) {
			pose.shearX += x * alpha;
			pose.shearY += y * alpha;
		} else {
			pose.shearX += (setup.shearX + x - pose.shearX) * alpha;
			pose.shearY += (setup.shearY + y - pose.shearY) * alpha;
		}
	}
}

/** Changes {@link Bone.shearX} and {@link Bone.shearY}. */
export class ShearXTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.shearX);
	}

	protected apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean) {
		pose.shearX = this.getRelativeValue(time, alpha, fromSetup, add, pose.shearX, setup.shearX);
	}
}

/** Changes {@link Bone.shearX} and {@link Bone.shearY}. */
export class ShearYTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.shearY);
	}

	protected apply1 (pose: BonePose, setup: BonePose, time: number, alpha: number, fromSetup: boolean, add: boolean,
		out: boolean) {
		pose.shearY = this.getRelativeValue(time, alpha, fromSetup, add, pose.shearY, setup.shearY);
	}
}

/** Changes {@link BonePose.inherit}. */
export class InheritTimeline extends Timeline implements BoneTimeline {
	readonly boneIndex: number;

	constructor (frameCount: number, boneIndex: number) {
		super(frameCount, `${Property.inherit}|${boneIndex}`);
		this.boneIndex = boneIndex;
		this.instant = true;
	}

	public getFrameEntries () {
		return 2/*ENTRIES*/;
	}

	/** Sets the inherit transform mode for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive.
	 * @param time The frame time in seconds. */
	public setFrame (frame: number, time: number, inherit: Inherit) {
		frame *= 2/*ENTRIES*/;
		this.frames[frame] = time;
		this.frames[frame + 1/*INHERIT*/] = inherit;
	}

	public apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;
		const pose = appliedPose ? bone.appliedPose : bone.pose;

		if (out) {
			if (fromSetup) pose.inherit = bone.data.setupPose.inherit;
		} else {
			const frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) pose.inherit = bone.data.setupPose.inherit;
			} else
				pose.inherit = this.frames[Timeline.search(frames, time, 2/*ENTRIES*/) + 1/*INHERIT*/];
		}
	}
}
/** The base class for timelines that change any number of slot properties with a curve. */
export abstract class SlotCurveTimeline extends CurveTimeline implements SlotTimeline {
	readonly slotIndex: number;

	constructor (frameCount: number, bezierCount: number, slotIndex: number, ...propertyIds: string[]) {
		super(frameCount, bezierCount, ...propertyIds);
		this.slotIndex = slotIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const slot = skeleton.slots[this.slotIndex];
		if (slot.bone.active) this.apply1(slot, appliedPose ? slot.appliedPose : slot.pose, time, alpha, fromSetup, add);
	}

	protected abstract apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, fromSetup: boolean, add: boolean): void;
}

/** Changes {@link SlotPose.color}. */
export class RGBATimeline extends SlotCurveTimeline {
	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, slotIndex, //
			`${Property.rgb}|${slotIndex}`, //
			`${Property.alpha}|${slotIndex}`);
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

	protected apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, fromSetup: boolean, add: boolean) {
		const color = pose.color;
		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) color.setFromColor(slot.data.setupPose.color);
			return;
		}

		let r = 0, g = 0, b = 0, a = 0;
		const i = Timeline.search(frames, time, 5/*ENTRIES*/);
		const curveType = this.curves[i / 5/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				a = frames[i + 4/*A*/];
				const t = (time - before) / (frames[i + 5/*ENTRIES*/] - before);
				r += (frames[i + 5/*ENTRIES*/ + 1/*R*/] - r) * t;
				g += (frames[i + 5/*ENTRIES*/ + 2/*G*/] - g) * t;
				b += (frames[i + 5/*ENTRIES*/ + 3/*B*/] - b) * t;
				a += (frames[i + 5/*ENTRIES*/ + 4/*A*/] - a) * t;
				break;
			}
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
		if (alpha === 1)
			color.set(r, g, b, a);
		else {
			if (fromSetup) {
				const setup = slot.data.setupPose.color;
				color.set(setup.r + (r - setup.r) * alpha, setup.g + (g - setup.g) * alpha, setup.b + (b - setup.b) * alpha,
					setup.a + (a - setup.a) * alpha);
			} else
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
		}
	}
}

/** Changes RGB for a slot's {@link SlotPose.color}. */
export class RGBTimeline extends SlotCurveTimeline {
	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, slotIndex, `${Property.rgb}|${slotIndex}`);
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

	protected apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, fromSetup: boolean, add: boolean) {
		const color = pose.color;
		let r = 0, g = 0, b = 0;
		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) {
				const setup = slot.data.setupPose.color;
				color.r = setup.r;
				color.g = setup.g;
				color.b = setup.b;
			}
			return;
		}

		const i = Timeline.search(frames, time, 4/*ENTRIES*/);
		const curveType = this.curves[i >> 2];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				const t = (time - before) / (frames[i + 4/*ENTRIES*/] - before);
				r += (frames[i + 4/*ENTRIES*/ + 1/*R*/] - r) * t;
				g += (frames[i + 4/*ENTRIES*/ + 2/*G*/] - g) * t;
				b += (frames[i + 4/*ENTRIES*/ + 3/*B*/] - b) * t;
				break;
			}
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
		if (alpha !== 1) {
			if (fromSetup) {
				const setup = slot.data.setupPose.color;
				r = setup.r + (r - setup.r) * alpha;
				g = setup.g + (g - setup.g) * alpha;
				b = setup.b + (b - setup.b) * alpha;
			} else {
				r = color.r + (r - color.r) * alpha;
				g = color.g + (g - color.g) * alpha;
				b = color.b + (b - color.b) * alpha;
			}
		}
		color.r = r < 0 ? 0 : (r > 1 ? 1 : r);
		color.g = g < 0 ? 0 : (g > 1 ? 1 : g);
		color.b = b < 0 ? 0 : (b > 1 ? 1 : b);
	}
}

/** Changes alpha for a slot's {@link SlotPose.color}. */
export class AlphaTimeline extends CurveTimeline1 implements SlotTimeline {
	slotIndex = 0;

	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, `${Property.alpha}|${slotIndex}`);
		this.slotIndex = slotIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;

		const color = (appliedPose ? slot.appliedPose : slot.pose).color;
		let a = 0;
		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) color.a = slot.data.setupPose.color.a;
			return;
		}

		a = this.getCurveValue(time);
		if (alpha !== 1) {
			if (fromSetup) {
				const setup = slot.data.setupPose.color;
				a = setup.a + (a - setup.a) * alpha;
			} else
				a = color.a + (a - color.a) * alpha;
		}
		color.a = a < 0 ? 0 : (a > 1 ? 1 : a);
	}
}

/** Changes {@link SlotPose.color} and {@link SlotPose.darkColor} for two color tinting. */
export class RGBA2Timeline extends SlotCurveTimeline {
	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, slotIndex, //
			`${Property.rgb}|${slotIndex}`, //
			`${Property.alpha}|${slotIndex}`, //
			`${Property.rgb2}|${slotIndex}`);
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

	protected apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, fromSetup: boolean, add: boolean) {
		// biome-ignore lint/style/noNonNullAssertion: reference runtime
		const light = pose.color, dark = pose.darkColor!;
		let r2 = 0, g2 = 0, b2 = 0
		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) {
				const setup = slot.data.setupPose;
				light.setFromColor(setup.color);
				// biome-ignore lint/style/noNonNullAssertion: reference runtime
				const setupDark = setup.darkColor!;
				dark.r = setupDark.r;
				dark.g = setupDark.g;
				dark.b = setupDark.b;
			}
			return;
		}

		let r = 0, g = 0, b = 0, a = 0;
		const i = Timeline.search(frames, time, 8/*ENTRIES*/);
		const curveType = this.curves[i >> 3];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				a = frames[i + 4/*A*/];
				r2 = frames[i + 5/*R2*/];
				g2 = frames[i + 6/*G2*/];
				b2 = frames[i + 7/*B2*/];
				const t = (time - before) / (frames[i + 8/*ENTRIES*/] - before);
				r += (frames[i + 8/*ENTRIES*/ + 1/*R*/] - r) * t;
				g += (frames[i + 8/*ENTRIES*/ + 2/*G*/] - g) * t;
				b += (frames[i + 8/*ENTRIES*/ + 3/*B*/] - b) * t;
				a += (frames[i + 8/*ENTRIES*/ + 4/*A*/] - a) * t;
				r2 += (frames[i + 8/*ENTRIES*/ + 5/*R2*/] - r2) * t;
				g2 += (frames[i + 8/*ENTRIES*/ + 6/*G2*/] - g2) * t;
				b2 += (frames[i + 8/*ENTRIES*/ + 7/*B2*/] - b2) * t;
				break;
			}
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

		if (alpha === 1)
			light.set(r, g, b, a);
		else if (fromSetup) {
			const setupPose = slot.data.setupPose;
			let setup = setupPose.color;
			light.set(setup.r + (r - setup.r) * alpha, setup.g + (g - setup.g) * alpha, setup.b + (b - setup.b) * alpha,
				setup.a + (a - setup.a) * alpha);
			// biome-ignore lint/style/noNonNullAssertion: reference runtime
			setup = setupPose.darkColor!;
			r2 = setup.r + (r2 - setup.r) * alpha;
			g2 = setup.g + (g2 - setup.g) * alpha;
			b2 = setup.b + (b2 - setup.b) * alpha;
		} else {
			light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
			r2 = dark.r + (r2 - dark.r) * alpha;
			g2 = dark.g + (g2 - dark.g) * alpha;
			b2 = dark.b + (b2 - dark.b) * alpha;
		}
		dark.r = r2 < 0 ? 0 : (r2 > 1 ? 1 : r2);
		dark.g = g2 < 0 ? 0 : (g2 > 1 ? 1 : g2);
		dark.b = b2 < 0 ? 0 : (b2 > 1 ? 1 : b2);
	}
}

/** Changes {@link SlotPose.color} and {@link SlotPose.darkColor} for two color tinting. */
export class RGB2Timeline extends SlotCurveTimeline {
	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, slotIndex, //
			`${Property.rgb}|${slotIndex}`, //
			`${Property.rgb2}|${slotIndex}`);
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

	protected apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, fromSetup: boolean, add: boolean) {
		// biome-ignore lint/style/noNonNullAssertion: reference runtime
		const light = pose.color, dark = pose.darkColor!;
		let r = 0, g = 0, b = 0, r2 = 0, g2 = 0, b2 = 0
		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) {
				const setup = slot.data.setupPose;
				// biome-ignore lint/style/noNonNullAssertion: reference runtime
				const setupLight = setup.color, setupDark = setup.darkColor!;
				light.r = setupLight.r;
				light.g = setupLight.g;
				light.b = setupLight.b;
				dark.r = setupDark.r;
				dark.g = setupDark.g;
				dark.b = setupDark.b;
			}
			return;
		}

		const i = Timeline.search(frames, time, 7/*ENTRIES*/);
		const curveType = this.curves[i / 7/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				r = frames[i + 1/*R*/];
				g = frames[i + 2/*G*/];
				b = frames[i + 3/*B*/];
				r2 = frames[i + 4/*R2*/];
				g2 = frames[i + 5/*G2*/];
				b2 = frames[i + 6/*B2*/];
				const t = (time - before) / (frames[i + 7/*ENTRIES*/] - before);
				r += (frames[i + 7/*ENTRIES*/ + 1/*R*/] - r) * t;
				g += (frames[i + 7/*ENTRIES*/ + 2/*G*/] - g) * t;
				b += (frames[i + 7/*ENTRIES*/ + 3/*B*/] - b) * t;
				r2 += (frames[i + 7/*ENTRIES*/ + 4/*R2*/] - r2) * t;
				g2 += (frames[i + 7/*ENTRIES*/ + 5/*G2*/] - g2) * t;
				b2 += (frames[i + 7/*ENTRIES*/ + 6/*B2*/] - b2) * t;
				break;
			}
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

		if (alpha !== 1) {
			if (fromSetup) {
				const setupPose = slot.data.setupPose;
				let setup = setupPose.color;
				r = setup.r + (r - setup.r) * alpha;
				g = setup.g + (g - setup.g) * alpha;
				b = setup.b + (b - setup.b) * alpha;
				// biome-ignore lint/style/noNonNullAssertion: reference runtime
				setup = setupPose.darkColor!;
				r2 = setup.r + (r2 - setup.r) * alpha;
				g2 = setup.g + (g2 - setup.g) * alpha;
				b2 = setup.b + (b2 - setup.b) * alpha;
			} else {
				r = light.r + (r - light.r) * alpha;
				g = light.g + (g - light.g) * alpha;
				b = light.b + (b - light.b) * alpha;
				r2 = dark.r + (r2 - dark.r) * alpha;
				g2 = dark.g + (g2 - dark.g) * alpha;
				b2 = dark.b + (b2 - dark.b) * alpha;
			}
		}
		light.r = r < 0 ? 0 : (r > 1 ? 1 : r);
		light.g = g < 0 ? 0 : (g > 1 ? 1 : g);
		light.b = b < 0 ? 0 : (b > 1 ? 1 : b);
		dark.r = r2 < 0 ? 0 : (r2 > 1 ? 1 : r2);
		dark.g = g2 < 0 ? 0 : (g2 > 1 ? 1 : g2);
		dark.b = b2 < 0 ? 0 : (b2 > 1 ? 1 : b2);
	}
}

/** Changes {@link SlotPose.ttachment}. */
export class AttachmentTimeline extends Timeline implements SlotTimeline {
	slotIndex = 0;

	/** The attachment name for each key frame. May contain null values to clear the attachment. */
	attachmentNames: Array<string | null>;

	constructor (frameCount: number, slotIndex: number) {
		super(frameCount, `${Property.attachment}|${slotIndex}`);
		this.slotIndex = slotIndex;
		this.attachmentNames = new Array<string>(frameCount);
		this.instant = true;
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time in seconds and the attachment name for the specified key frame. */
	setFrame (frame: number, time: number, attachmentName: string | null) {
		this.frames[frame] = time;
		this.attachmentNames[frame] = attachmentName;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;
		const pose = appliedPose ? slot.appliedPose : slot.pose;

		if (out || time < this.frames[0]) {
			if (fromSetup) this.setAttachment(skeleton, pose, slot.data.attachmentName);
		} else
			this.setAttachment(skeleton, pose, this.attachmentNames[Timeline.search(this.frames, time)]);
	}

	setAttachment (skeleton: Skeleton, pose: SlotPose, attachmentName: string | null) {
		pose.setAttachment(!attachmentName ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
	}
}

/** Changes {@link SlotPose.deform} to deform a {@link VertexAttachment}. */
export class DeformTimeline extends CurveTimeline implements SlotTimeline {
	readonly slotIndex: number;

	/** The attachment that will be deformed.
	 *
	 * See {@link VertexAttachment.getTimelineAttachment}. */
	readonly attachment: VertexAttachment;

	/** The vertices for each key frame. */
	vertices: Array<NumberArrayLike>;

	constructor (frameCount: number, bezierCount: number, slotIndex: number, attachment: VertexAttachment) {
		super(frameCount, bezierCount, `${Property.deform}|${slotIndex}|${attachment.id}`);
		this.slotIndex = slotIndex;
		this.attachment = attachment;
		this.vertices = new Array<NumberArrayLike>(frameCount);
		this.additive = true;
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time and vertices for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive.
	 * @param time The frame time in seconds.
	 * @param vertices Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights. */
	setFrame (frame: number, time: number, vertices: NumberArrayLike) {
		this.frames[frame] = time;
		this.vertices[frame] = vertices;
	}

	/** @param value1 Ignored (0 is used for a deform timeline).
	 * @param value2 Ignored (1 is used for a deform timeline). */
	setBezier (bezier: number, frame: number, value: number, time1: number, value1: number, cx1: number, cy1: number, cx2: number,
		cy2: number, time2: number, value2: number) {
		const curves = this.curves;
		let i = this.getFrameCount() + bezier * 18/*BEZIER_SIZE*/;
		if (value === 0) curves[frame] = 2/*BEZIER*/ + i;
		const tmpx = (time1 - cx1 * 2 + cx2) * 0.03, tmpy = cy2 * 0.03 - cy1 * 0.06;
		const dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy = (cy1 - cy2 + 0.33333333) * 0.018;
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
		const curves = this.curves;
		let i = curves[frame];
		switch (i) {
			case 0/*LINEAR*/: {
				const x = this.frames[frame];
				return (time - x) / (this.frames[frame + this.getFrameEntries()] - x);
			}
			case 1/*STEPPED*/:
				return 0;
		}
		i -= 2/*BEZIER*/;
		if (curves[i] > time) {
			const x = this.frames[frame];
			return curves[i + 1] * (time - x) / (curves[i] - x);
		}
		const n = i + 18/*BEZIER_SIZE*/;
		for (i += 2; i < n; i += 2) {
			if (curves[i] >= time) {
				const x = curves[i - 2], y = curves[i - 1];
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
			}
		}
		const x = curves[n - 2], y = curves[n - 1];
		return y + (1 - y) * (time - x) / (this.frames[frame + this.getFrameEntries()] - x);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Event[] | null, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {
		const slots = skeleton.slots;
		if (!this.attachment.isTimelineActive(slots, this.slotIndex, appliedPose)) return;
		const timelineSlots = this.attachment.timelineSlots;

		const frames = this.frames;
		if (time < frames[0]) {
			this.applyBeforeFirst(slots[this.slotIndex], appliedPose, fromSetup);
			for (const slotIndex of timelineSlots)
				this.applyBeforeFirst(slots[slotIndex], appliedPose, fromSetup);
			return;
		}

		let v1: NumberArrayLike, v2: NumberArrayLike | null;
		let percent: number;
		if (time >= frames[frames.length - 1]) {
			percent = 0;
			v1 = this.vertices[frames.length - 1];
			v2 = null;
		} else {
			const frame = Timeline.search(frames, time);
			percent = this.getCurvePercent(time, frame);
			v1 = this.vertices[frame];
			v2 = this.vertices[frame + 1];
		}

		const vertexCount = this.vertices[0].length;
		this.applyToSlot(slots[this.slotIndex], appliedPose, v1, v2, percent, vertexCount, alpha, fromSetup, add);
		for (const slotIndex of timelineSlots)
			this.applyToSlot(slots[slotIndex], appliedPose, v1, v2, percent, vertexCount, alpha, fromSetup, add);
	}

	private applyToSlot (slot: Slot, appliedPose: boolean, v1: NumberArrayLike, v2: NumberArrayLike | null, percent: number, vertexCount: number,
		alpha: number, fromSetup: boolean, add: boolean) {
		if (!slot.bone.active) return;
		const pose = appliedPose ? slot.appliedPose : slot.pose;
		if (pose.attachment === null || pose.attachment.timelineAttachment !== this.attachment) return;

		const vertexAttachment = pose.attachment as VertexAttachment;
		const deform = pose.deform;
		if (deform.length === 0) fromSetup = true;
		deform.length = vertexCount;

		if (v2 === null) { // Time is after last frame.
			if (alpha === 1) {
				if (add && !fromSetup) {
					if (!vertexAttachment.bones) { // Unweighted vertex positions, no alpha.
						const setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++)
							deform[i] += v1[i] - setupVertices[i];
					} else { // Weighted deform offsets, no alpha.
						for (let i = 0; i < vertexCount; i++)
							deform[i] += v1[i];
					}
				} else // Vertex positions or deform offsets, no alpha.
					Utils.arrayCopy(v1, 0, deform, 0, vertexCount);
			} else if (fromSetup) {
				if (!vertexAttachment.bones) { // Unweighted vertex positions, with alpha.
					const setupVertices = vertexAttachment.vertices;
					for (let i = 0; i < vertexCount; i++) {
						const setup = setupVertices[i];
						deform[i] = setup + (v1[i] - setup) * alpha;
					}
				} else { // Weighted deform offsets, with alpha.
					for (let i = 0; i < vertexCount; i++)
						deform[i] = v1[i] * alpha;
				}
			} else if (add) {
				if (!vertexAttachment.bones) { // Unweighted vertex positions, no alpha.
					const setupVertices = vertexAttachment.vertices;
					for (let i = 0; i < vertexCount; i++)
						deform[i] += (v1[i] - setupVertices[i]) * alpha;
				} else { // Weighted deform offsets, alpha.
					for (let i = 0; i < vertexCount; i++)
						deform[i] += v1[i] * alpha;
				}
			} else { // Vertex positions or deform offsets, with alpha.
				for (let i = 0; i < vertexCount; i++)
					deform[i] += (v1[i] - deform[i]) * alpha;
			}
		} else { // Between frames.
			if (alpha === 1) {
				if (add && !fromSetup) {
					if (!vertexAttachment.bones) { // Unweighted vertex positions, no alpha.
						const setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++) {
							const prev = v1[i];
							deform[i] += prev + (v2[i] - prev) * percent - setupVertices[i];
						}
					} else { // Weighted deform offsets, no alpha.
						for (let i = 0; i < vertexCount; i++) {
							const prev = v1[i];
							deform[i] += prev + (v2[i] - prev) * percent;
						}
					}
				} else if (percent === 0)
					Utils.arrayCopy(v1, 0, deform, 0, vertexCount)
				else { // Vertex positions or deform offsets, no alpha.
					for (let i = 0; i < vertexCount; i++) {
						const prev = v1[i];
						deform[i] = prev + (v2[i] - prev) * percent;
					}
				}
			} else if (fromSetup) {
				if (!vertexAttachment.bones) { // Unweighted vertex positions, with alpha.
					const setupVertices = vertexAttachment.vertices;
					for (let i = 0; i < vertexCount; i++) {
						const prev = v1[i], setup = setupVertices[i];
						deform[i] = setup + (prev + (v2[i] - prev) * percent - setup) * alpha;
					}
				} else { // Weighted deform offsets, with alpha.
					for (let i = 0; i < vertexCount; i++) {
						const prev = v1[i];
						deform[i] = (prev + (v2[i] - prev) * percent) * alpha;
					}
				}
			} else if (add) {
				if (!vertexAttachment.bones) { // Unweighted vertex positions, with alpha.
					const setupVertices = vertexAttachment.vertices;
					for (let i = 0; i < vertexCount; i++) {
						const prev = v1[i];
						deform[i] += (prev + (v2[i] - prev) * percent - setupVertices[i]) * alpha;
					}
				} else { // Weighted deform offsets, with alpha.
					for (let i = 0; i < vertexCount; i++) {
						const prev = v1[i];
						deform[i] += (prev + (v2[i] - prev) * percent) * alpha;
					}
				}
			} else {
				for (let i = 0; i < vertexCount; i++) {
					const prev = v1[i];
					deform[i] += (prev + (v2[i] - prev) * percent - deform[i]) * alpha;
				}
			}
		}
	}

	private applyBeforeFirst (slot: Slot, appliedPose: boolean, fromSetup: boolean) {
		if (!slot.bone.active) return;
		const pose = appliedPose ? slot.appliedPose : slot.pose;
		if (pose.attachment == null || pose.attachment.timelineAttachment !== this.attachment) return;
		if (pose.deform.length === 0) fromSetup = true;
		if (fromSetup) pose.deform.length = 0;
	}
}

/** Changes {@link Slot.getSequenceIndex} for an attachment's {@link Sequence}. */
export class SequenceTimeline extends Timeline implements SlotTimeline {
	static ENTRIES = 3;
	static MODE = 1;
	static DELAY = 2;

	readonly slotIndex: number;
	readonly attachment: Attachment;

	constructor (frameCount: number, slotIndex: number, attachment: Attachment) {
		super(frameCount, `${Property.sequence}|${slotIndex}|${(attachment as unknown as HasSequence).sequence.id}`);
		this.slotIndex = slotIndex;
		this.attachment = attachment;
		this.instant = true;
	}

	getFrameEntries () {
		return SequenceTimeline.ENTRIES;
	}

	getSlotIndex () {
		return this.slotIndex;
	}

	/** The attachment for which the {@link SlotPose.sequenceIndex} will be set.
	 *
	 * See {@link VertexAttachment.timelineAttachment}. */
	getAttachment () {
		return this.attachment;
	}

	/** Sets the time, mode, index, and frame time for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive.
	 * @param time Seconds between frames. */
	setFrame (frame: number, time: number, mode: SequenceMode, index: number, delay: number) {
		const frames = this.frames;
		frame *= SequenceTimeline.ENTRIES;
		frames[frame] = time;
		frames[frame + SequenceTimeline.MODE] = mode | (index << 4);
		frames[frame + SequenceTimeline.DELAY] = delay;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {
		const slots = skeleton.slots;
		if (!this.attachment.isTimelineActive(slots, this.slotIndex, appliedPose)) return;
		const timelineSlots = this.attachment.timelineSlots;

		const frames = this.frames;
		if (out || time < frames[0]) {
			if (fromSetup) {
				this.setupPose(slots[this.slotIndex], appliedPose);
				for (const slotIndex of timelineSlots)
					this.setupPose(slots[slotIndex], appliedPose)
			}
			return;
		}

		const i = Timeline.search(frames, time, SequenceTimeline.ENTRIES);
		const before = frames[i];
		const modeAndIndex = frames[i + SequenceTimeline.MODE];
		const delay = frames[i + SequenceTimeline.DELAY];

		this.applyToSlot(slots[this.slotIndex], appliedPose, time, before, modeAndIndex, delay);
		for (const slotIndex of timelineSlots)
			this.applyToSlot(slots[slotIndex], appliedPose, time, before, modeAndIndex, delay);
	}

	private setupPose (slot: Slot, appliedPose: boolean) {
		if (!slot.bone.active) return;
		const pose = appliedPose ? slot.appliedPose : slot.pose;
		if (pose.attachment === null || pose.attachment.timelineAttachment !== this.attachment) return;
		pose.sequenceIndex = -1;
	}

	private applyToSlot (slot: Slot, appliedPose: boolean, time: number, before: number, modeAndIndex: number, delay: number) {
		if (!slot.bone.active) return;
		const pose = appliedPose ? slot.appliedPose : slot.pose;
		if (pose.attachment === null || pose.attachment.timelineAttachment !== this.attachment) return;

		let index = modeAndIndex >> 4, count = (pose.attachment as unknown as HasSequence).sequence.regions.length;
		const mode = SequenceModeValues[modeAndIndex & 0xf];
		if (mode !== SequenceMode.hold) {
			index += (((time - before) / delay + 0.00001) | 0);
			switch (mode) {
				case SequenceMode.once: index = Math.min(count - 1, index); break;
				case SequenceMode.loop: index %= count; break;
				case SequenceMode.pingpong: {
					const n = (count << 1) - 2;
					index = n === 0 ? 0 : index % n;
					if (index >= count) index = n - index;
					break;
				}
				case SequenceMode.onceReverse: index = Math.max(count - 1 - index, 0); break;
				case SequenceMode.loopReverse: index = count - 1 - (index % count); break;
				case SequenceMode.pingpongReverse: {
					const n = (count << 1) - 2;
					index = n === 0 ? 0 : (index + count - 1) % n;
					if (index >= count) index = n - index;
				}
			}
		}
		pose.sequenceIndex = index;
	}
}

/** Fires an {@link Event} when specific animation times are reached. */
export class EventTimeline extends Timeline {
	static propertyIds = [`${Property.event}`];

	/** The event for each key frame. */
	events: Array<Event>;

	constructor (frameCount: number) {
		super(frameCount, ...EventTimeline.propertyIds);
		this.events = new Array<Event>(frameCount);
		this.instant = true;
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
	apply (skeleton: Skeleton | null, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number,
		fromSetup: boolean, add: boolean, out: boolean, appliedPose: boolean) {

		if (!firedEvents) return;

		const frames = this.frames;
		const frameCount = this.frames.length;

		if (lastTime > time) { // Apply after lastTime for looped animations.
			this.apply(null, lastTime, Number.MAX_VALUE, firedEvents, 0, false, false, false, false);
			lastTime = -1;
		} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
			return;
		if (time < frames[0]) return;

		let i = 0;
		if (lastTime < frames[0])
			i = 0;
		else {
			i = Timeline.search(frames, lastTime) + 1;
			const frameTime = frames[i];
			while (i > 0) { // Fire multiple events with the same frame.
				if (frames[i - 1] !== frameTime) break;
				i--;
			}
		}
		for (; i < frameCount && time >= frames[i]; i++)
			firedEvents.push(this.events[i]);
	}
}

/** Changes the {@link Skeleton.getDrawOrder}. */
export class DrawOrderTimeline extends Timeline {
	static readonly propertyID = `${Property.drawOrder}`;
	static propertyIds = [DrawOrderTimeline.propertyID];

	/** The draw order for each key frame. See {@link setFrame}. */
	private readonly drawOrders: Array<Array<number> | null>;

	constructor (frameCount: number) {
		super(frameCount, ...DrawOrderTimeline.propertyIds);
		this.drawOrders = new Array<Array<number> | null>(frameCount);
		this.instant = true;
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time in seconds and the draw order for the specified key frame.
	 * @param drawOrder Ordered {@link Skeleton.slots} indices, or null to use setup pose
	 *           draw order. */
	setFrame (frame: number, time: number, drawOrder: Array<number> | null) {
		this.frames[frame] = time;
		this.drawOrders[frame] = drawOrder;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number,
		fromSetup: boolean, add: boolean, out: boolean, appliedPose: boolean) {
		const pose = (appliedPose ? skeleton.drawOrder.appliedPose : skeleton.drawOrder.pose);
		const setup = skeleton.slots;
		if (out || time < this.frames[0]) {
			if (fromSetup) Utils.arrayCopy(setup, 0, pose, 0, skeleton.slots.length);
			return;
		}

		const order = this.drawOrders[Timeline.search(this.frames, time)];
		if (!order)
			Utils.arrayCopy(setup, 0, pose, 0, skeleton.slots.length);
		else {
			for (let i = 0, n = order.length; i < n; i++)
				pose[i] = setup[order[i]];
		}
	}
}

/** Changes a subset of the {@link Skeleton.getDrawOrder | draw order}. */
export class DrawOrderFolderTimeline extends Timeline {
	private readonly slots: number[];
	private readonly inFolder: boolean[];
	private readonly drawOrders: Array<Array<number> | null>;

	/** @param slots {@link Skeleton.slots} indices controlled by this timeline, in setup order.
	 * @param slotCount The maximum number of slots in the skeleton. */
	constructor (frameCount: number, slots: number[], slotCount: number) {
		super(frameCount, ...DrawOrderFolderTimeline.propertyIds(slots));
		this.slots = slots;
		this.drawOrders = new Array(frameCount);
		this.inFolder = new Array(slotCount);
		for (const i of slots)
			this.inFolder[i] = true;
		this.instant = true;
	}

	private static propertyIds (slots: number[]): string[] {
		const n = slots.length;
		const ids = new Array(n);
		for (let i = 0; i < n; i++)
			ids[i] = `d${slots[i]}`;
		return ids;
	}

	getFrameCount (): number {
		return this.frames.length;
	}

	/** The {@link Skeleton.getSlots} indices that this timeline affects, in setup order. */
	getSlots (): number[] {
		return this.slots;
	}

	/** The draw order for each frame. See {@link setFrame}. */
	getDrawOrders (): Array<Array<number> | null> {
		return this.drawOrders;
	}

	/** Sets the time and draw order for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive.
	 * @param time The frame time in seconds.
	 * @param drawOrder Ordered {@link getSlots} indices, or null to use setup pose order. */
	setFrame (frame: number, time: number, drawOrder: Array<number> | null): void {
		this.frames[frame] = time;
		this.drawOrders[frame] = drawOrder;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean): void {
		const pose = (appliedPose ? skeleton.drawOrder.appliedPose : skeleton.drawOrder.pose);
		const setup = skeleton.slots;
		if (out || time < this.frames[0]) {
			if (fromSetup) this.setup(pose, setup);
		} else {
			const order = this.drawOrders[Timeline.search(this.frames, time)];
			if (!order)
				this.setup(pose, setup);
			else {
				const inFolder = this.inFolder;
				const slots = this.slots;
				for (let i = 0, found = 0, done = slots.length; ; i++) {
					if (inFolder[pose[i].data.index]) {
						pose[i] = setup[slots[order[found]]];
						if (++found === done) break;
					}
				}
			}
		}
	}

	private setup (pose: Slot[], setup: Slot[]): void {
		const { inFolder, slots } = this;
		for (let i = 0, found = 0, done = slots.length; ; i++) {
			if (inFolder[pose[i].data.index]) {
				pose[i] = setup[slots[found]];
				if (++found === done) break;
			}
		}
	}
}

export interface ConstraintTimeline {
	/** The index of the constraint in {@link Skeleton.constraints} that will be changed when this timeline is applied, or -1 if
	 * a specific constraint will not be changed. */
	readonly constraintIndex: number;
}

export function isConstraintTimeline (obj: Timeline & Partial<ConstraintTimeline>): obj is Timeline & ConstraintTimeline {
	return typeof obj === 'object' && obj !== null && typeof obj.constraintIndex === 'number';
}

/** Changes {@link IkConstraintPose.mix)}, {@link IkConstraintPose.softness},
 * {@link IkConstraintPose.bendDirection}, {@link IkConstraintPose.stretch}, and
 * {@link IkConstraintPose.compress}. */
export class IkConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
	readonly constraintIndex: number = 0;

	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, `${Property.ikConstraint}|${constraintIndex}`);
		this.constraintIndex = constraintIndex;
	}

	getFrameEntries () {
		return 6/*ENTRIES*/;
	}

	/** Sets the time, mix, softness, bend direction, compress, and stretch for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive.
	 * @param time The frame time in seconds.
	 * @param bendDirection 1 or -1. */
	setFrame (frame: number, time: number, mix: number, softness: number, bendDirection: number, compress: boolean, stretch: boolean) {
		frame *= 6/*ENTRIES*/;
		this.frames[frame] = time;
		this.frames[frame + 1/*MIX*/] = mix;
		this.frames[frame + 2/*SOFTNESS*/] = softness;
		this.frames[frame + 3/*BEND_DIRECTION*/] = bendDirection;
		this.frames[frame + 4/*COMPRESS*/] = compress ? 1 : 0;
		this.frames[frame + 5/*STRETCH*/] = stretch ? 1 : 0;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex] as IkConstraint;
		if (!constraint.active) return;
		const pose = appliedPose ? constraint.appliedPose : constraint.pose;

		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) {
				const setup = constraint.data.setupPose;
				pose.mix = setup.mix;
				pose.softness = setup.softness;
				pose.bendDirection = setup.bendDirection;
				pose.compress = setup.compress;
				pose.stretch = setup.stretch;
			}
			return;
		}

		let mix = 0, softness = 0;
		const i = Timeline.search(frames, time, 6/*ENTRIES*/)
		const curveType = this.curves[i / 6/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				mix = frames[i + 1/*MIX*/];
				softness = frames[i + 2/*SOFTNESS*/];
				const t = (time - before) / (frames[i + 6/*ENTRIES*/] - before);
				mix += (frames[i + 6/*ENTRIES*/ + 1/*MIX*/] - mix) * t;
				softness += (frames[i + 6/*ENTRIES*/ + 2/*SOFTNESS*/] - softness) * t;
				break;
			}
			case 1/*STEPPED*/:
				mix = frames[i + 1/*MIX*/];
				softness = frames[i + 2/*SOFTNESS*/];
				break;
			default:
				mix = this.getBezierValue(time, i, 1/*MIX*/, curveType - 2/*BEZIER*/);
				softness = this.getBezierValue(time, i, 2/*SOFTNESS*/, curveType + 18/*BEZIER_SIZE*/ - 2/*BEZIER*/);
		}

		const base = fromSetup ? constraint.data.setupPose : pose;
		pose.mix = base.mix + (mix - base.mix) * alpha;
		pose.softness = base.softness + (softness - base.softness) * alpha;
		if (out) {
			if (fromSetup) {
				pose.bendDirection = base.bendDirection;
				pose.compress = base.compress;
				pose.stretch = base.stretch;
			}
		} else {
			pose.bendDirection = frames[i + 3/*BEND_DIRECTION*/];
			pose.compress = frames[i + 4/*COMPRESS*/] !== 0;
			pose.stretch = frames[i + 5/*STRETCH*/] !== 0;
		}
	}
}

/** Changes {@link TransformConstraintPose.mixRotate}, {@link TransformConstraintPose.mixX},
 * {@link TransformConstraintPose.mixY}, {@link TransformConstraintPose.mixScaleX},
 * {@link TransformConstraintPose.mixScaleY}, and {@link TransformConstraintPose.mixShearY}. */
export class TransformConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
	/** The index of the transform constraint slot in {@link Skeleton.transformConstraints} that will be changed. */
	constraintIndex: number = 0;

	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, `${Property.transformConstraint}|${constraintIndex}`);
		this.constraintIndex = constraintIndex;
		this.additive = true;
	}

	getFrameEntries () {
		return 7/*ENTRIES*/;
	}

	/** Sets the time, rotate mix, translate mix, scale mix, and shear mix for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive.
	 * @param time The frame time in seconds. */
	setFrame (frame: number, time: number, mixRotate: number, mixX: number, mixY: number, mixScaleX: number, mixScaleY: number,
		mixShearY: number) {
		const frames = this.frames;
		frame *= 7/*ENTRIES*/;
		frames[frame] = time;
		frames[frame + 1/*ROTATE*/] = mixRotate;
		frames[frame + 2/*X*/] = mixX;
		frames[frame + 3/*Y*/] = mixY;
		frames[frame + 4/*SCALEX*/] = mixScaleX;
		frames[frame + 5/*SCALEY*/] = mixScaleY;
		frames[frame + 6/*SHEARY*/] = mixShearY;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex] as TransformConstraint;
		if (!constraint.active) return;
		const pose = appliedPose ? constraint.appliedPose : constraint.pose;

		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) {
				const setup = constraint.data.setupPose;
				pose.mixRotate = setup.mixRotate;
				pose.mixX = setup.mixX;
				pose.mixY = setup.mixY;
				pose.mixScaleX = setup.mixScaleX;
				pose.mixScaleY = setup.mixScaleY;
				pose.mixShearY = setup.mixShearY;
			}
			return;
		}

		let rotate: number, x: number, y: number, scaleX: number, scaleY: number, shearY: number;
		const i = Timeline.search(frames, time, 7/*ENTRIES*/);
		const curveType = this.curves[i / 7/*ENTRIES*/];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				rotate = frames[i + 1/*ROTATE*/];
				x = frames[i + 2/*X*/];
				y = frames[i + 3/*Y*/];
				scaleX = frames[i + 4/*SCALEX*/];
				scaleY = frames[i + 5/*SCALEY*/];
				shearY = frames[i + 6/*SHEARY*/];
				const t = (time - before) / (frames[i + 7/*ENTRIES*/] - before);
				rotate += (frames[i + 7/*ENTRIES*/ + 1/*ROTATE*/] - rotate) * t;
				x += (frames[i + 7/*ENTRIES*/ + 2/*X*/] - x) * t;
				y += (frames[i + 7/*ENTRIES*/ + 3/*Y*/] - y) * t;
				scaleX += (frames[i + 7/*ENTRIES*/ + 4/*SCALEX*/] - scaleX) * t;
				scaleY += (frames[i + 7/*ENTRIES*/ + 5/*SCALEY*/] - scaleY) * t;
				shearY += (frames[i + 7/*ENTRIES*/ + 6/*SHEARY*/] - shearY) * t;
				break;
			}
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

		const base = fromSetup ? constraint.data.setupPose : pose;
		if (add) {
			pose.mixRotate = base.mixRotate + rotate * alpha;
			pose.mixX = base.mixX + x * alpha;
			pose.mixY = base.mixY + y * alpha;
			pose.mixScaleX = base.mixScaleX + scaleX * alpha;
			pose.mixScaleY = base.mixScaleY + scaleY * alpha;
			pose.mixShearY = base.mixShearY + shearY * alpha;
		} else {
			pose.mixRotate = base.mixRotate + (rotate - base.mixRotate) * alpha;
			pose.mixX = base.mixX + (x - base.mixX) * alpha;
			pose.mixY = base.mixY + (y - base.mixY) * alpha;
			pose.mixScaleX = base.mixScaleX + (scaleX - base.mixScaleX) * alpha;
			pose.mixScaleY = base.mixScaleY + (scaleY - base.mixScaleY) * alpha;
			pose.mixShearY = base.mixShearY + (shearY - base.mixShearY) * alpha;
		}
	}
}

/** The base class for timelines that change 1 constraint property with a curve. */
export abstract class ConstraintTimeline1 extends CurveTimeline1 implements ConstraintTimeline {
	readonly constraintIndex: number;

	constructor (frameCount: number, bezierCount: number, constraintIndex: number, property: Property) {
		super(frameCount, bezierCount, `${property}|${constraintIndex}`);
		this.constraintIndex = constraintIndex;
	}
}

/** Changes {@link PathConstraintPose.position}. */
export class PathConstraintPositionTimeline extends ConstraintTimeline1 {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.pathConstraintPosition);
		this.additive = true;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex] as PathConstraint;
		if (constraint.active) {
			const pose = appliedPose ? constraint.appliedPose : constraint.pose;
			pose.position = this.getAbsoluteValue(time, alpha, fromSetup, add, pose.position, constraint.data.setupPose.position);
		}
	}
}

/** Changes {@link PathConstraintPose.spacing}. */
export class PathConstraintSpacingTimeline extends ConstraintTimeline1 {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.pathConstraintSpacing);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex] as PathConstraint;
		if (constraint.active) {
			const pose = appliedPose ? constraint.appliedPose : constraint.pose;
			pose.spacing = this.getAbsoluteValue(time, alpha, fromSetup, false, pose.spacing,
				constraint.data.setupPose.spacing);
		}
	}
}

/** Changes {@link PathConstraint.mixRotate}, {@link PathConstraint.mixX}, and
 * {@link PathConstraint.mixY}. */
export class PathConstraintMixTimeline extends CurveTimeline implements ConstraintTimeline {
	readonly constraintIndex: number;

	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, `${Property.pathConstraintMix}|${constraintIndex}`);
		this.constraintIndex = constraintIndex;
	}

	getFrameEntries () {
		return 4/*ENTRIES*/;
	}

	/** Sets the time and color for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive.
	 * @param time The frame time in seconds. */
	setFrame (frame: number, time: number, mixRotate: number, mixX: number, mixY: number) {
		const frames = this.frames;
		frame <<= 2;
		frames[frame] = time;
		frames[frame + 1/*ROTATE*/] = mixRotate;
		frames[frame + 2/*X*/] = mixX;
		frames[frame + 3/*Y*/] = mixY;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex] as PathConstraint;
		if (!constraint.active) return;
		const pose = appliedPose ? constraint.appliedPose : constraint.pose;

		const frames = this.frames;
		if (time < frames[0]) {
			if (fromSetup) {
				const setup = constraint.data.setupPose;
				pose.mixRotate = setup.mixRotate;
				pose.mixX = setup.mixX;
				pose.mixY = setup.mixY;
			}
			return;
		}

		let rotate: number, x: number, y: number;
		const i = Timeline.search(frames, time, 4/*ENTRIES*/);
		const curveType = this.curves[i >> 2];
		switch (curveType) {
			case 0/*LINEAR*/: {
				const before = frames[i];
				rotate = frames[i + 1/*ROTATE*/];
				x = frames[i + 2/*X*/];
				y = frames[i + 3/*Y*/];
				const t = (time - before) / (frames[i + 4/*ENTRIES*/] - before);
				rotate += (frames[i + 4/*ENTRIES*/ + 1/*ROTATE*/] - rotate) * t;
				x += (frames[i + 4/*ENTRIES*/ + 2/*X*/] - x) * t;
				y += (frames[i + 4/*ENTRIES*/ + 3/*Y*/] - y) * t;
				break;
			}
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

		const base = fromSetup ? constraint.data.setupPose : pose;
		if (add) {
			pose.mixRotate = base.mixRotate + rotate * alpha;
			pose.mixX = base.mixX + x * alpha;
			pose.mixY = base.mixY + y * alpha;
		} else {
			pose.mixRotate = base.mixRotate + (rotate - base.mixRotate) * alpha;
			pose.mixX = base.mixX + (x - base.mixX) * alpha;
			pose.mixY = base.mixY + (y - base.mixY) * alpha;
		}
	}
}

/** The base class for most {@link PhysicsConstraint} timelines. */
export abstract class PhysicsConstraintTimeline extends ConstraintTimeline1 {
	/** @param constraintIndex -1 for all physics constraints in the skeleton. */
	constructor (frameCount: number, bezierCount: number, constraintIndex: number, property: number) {
		super(frameCount, bezierCount, constraintIndex, property);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		if (add && !this.additive) add = false;
		if (this.constraintIndex === -1) {
			const value = time >= this.frames[0] ? this.getCurveValue(time) : 0;
			const constraints = skeleton.physics;
			for (const constraint of constraints) {
				if (constraint.active && this.global(constraint.data)) {
					const pose = appliedPose ? constraint.appliedPose : constraint.pose;
					this.set(pose, this.getAbsoluteValue(time, alpha, fromSetup, add, this.get(pose), this.get(constraint.data.setupPose), value));
				}
			}
		} else {
			const constraint = skeleton.constraints[this.constraintIndex] as PhysicsConstraint;
			if (constraint.active) {
				const pose = appliedPose ? constraint.appliedPose : constraint.pose;
				this.set(pose, this.getAbsoluteValue(time, alpha, fromSetup, add, this.get(pose), this.get(constraint.data.setupPose)));
			}
		}
	}

	abstract get (pose: PhysicsConstraintPose): number;

	abstract set (pose: PhysicsConstraintPose, value: number): void;

	abstract global (constraint: PhysicsConstraintData): boolean;
}

/** Changes {@link PhysicsConstraintPose.inertia}. */
export class PhysicsConstraintInertiaTimeline extends PhysicsConstraintTimeline {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintInertia);
	}

	get (pose: PhysicsConstraintPose): number {
		return pose.inertia;
	}

	set (pose: PhysicsConstraintPose, value: number): void {
		pose.inertia = value;
	}

	global (constraint: PhysicsConstraintData): boolean {
		return constraint.inertiaGlobal;
	}
}

/** Changes {@link PhysicsConstraintPose.strength}. */
export class PhysicsConstraintStrengthTimeline extends PhysicsConstraintTimeline {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintStrength);
	}
	get (pose: PhysicsConstraintPose): number {
		return pose.strength;
	}

	set (pose: PhysicsConstraintPose, value: number): void {
		pose.strength = value;
	}

	global (constraint: PhysicsConstraintData): boolean {
		return constraint.strengthGlobal;
	}
}

/** Changes {@link PhysicsConstraintPose.damping}. */
export class PhysicsConstraintDampingTimeline extends PhysicsConstraintTimeline {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintDamping);
	}

	get (pose: PhysicsConstraintPose): number {
		return pose.damping;
	}

	set (pose: PhysicsConstraintPose, value: number): void {
		pose.damping = value;
	}

	global (constraint: PhysicsConstraintData): boolean {
		return constraint.dampingGlobal;
	}
}

/** Changes {@link PhysicsConstraintPose.massInverse}. The timeline values are not inverted. */
export class PhysicsConstraintMassTimeline extends PhysicsConstraintTimeline {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintMass);
	}

	get (pose: PhysicsConstraintPose): number {
		return 1 / pose.massInverse;
	}

	set (pose: PhysicsConstraintPose, value: number): void {
		pose.massInverse = 1 / value;
	}

	global (constraint: PhysicsConstraintData): boolean {
		return constraint.massGlobal;
	}
}

/** Changes {@link PhysicsConstraintPose.wind}. */
export class PhysicsConstraintWindTimeline extends PhysicsConstraintTimeline {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintWind);
		this.additive = true;
	}

	get (pose: PhysicsConstraintPose): number {
		return pose.wind;
	}

	set (pose: PhysicsConstraintPose, value: number): void {
		pose.wind = value;
	}

	global (constraint: PhysicsConstraintData): boolean {
		return constraint.windGlobal;
	}
}

/** Changes {@link PhysicsConstraintPose.gravity}. */
export class PhysicsConstraintGravityTimeline extends PhysicsConstraintTimeline {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintGravity);
		this.additive = true;
	}

	get (pose: PhysicsConstraintPose): number {
		return pose.gravity;
	}

	set (pose: PhysicsConstraintPose, value: number): void {
		pose.gravity = value;
	}

	global (constraint: PhysicsConstraintData): boolean {
		return constraint.gravityGlobal;
	}
}

/** Changes {@link PhysicsConstraintPose.mix}. */
export class PhysicsConstraintMixTimeline extends PhysicsConstraintTimeline {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintMix);
	}

	get (pose: PhysicsConstraintPose): number {
		return pose.mix;
	}

	set (pose: PhysicsConstraintPose, value: number): void {
		pose.mix = value;
	}

	global (constraint: PhysicsConstraintData): boolean {
		return constraint.mixGlobal;
	}
}

/** Resets a physics constraint when specific animation times are reached. */
export class PhysicsConstraintResetTimeline extends Timeline implements ConstraintTimeline {
	private static propertyIds: string[] = [Property.physicsConstraintReset.toString()];

	/** The index of the physics constraint in {@link Skeleton.contraints} that will be reset when this timeline is
	* applied, or -1 if all physics constraints in the skeleton will be reset. */
	readonly constraintIndex: number;

	/** @param constraintIndex -1 for all physics constraints in the skeleton. */
	constructor (frameCount: number, constraintIndex: number) {
		super(frameCount, ...PhysicsConstraintResetTimeline.propertyIds);
		this.constraintIndex = constraintIndex;
		this.instant = true;
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time for the specified frame.
	 * @param frame Between 0 and `frameCount`, inclusive. */
	setFrame (frame: number, time: number) {
		this.frames[frame] = time;
	}

	/** Resets the physics constraint when frames > `lastTime` and <= `time`. */
	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		let constraint: PhysicsConstraint | undefined;
		if (this.constraintIndex !== -1) {
			constraint = skeleton.constraints[this.constraintIndex] as PhysicsConstraint;
			if (!constraint.active) return;
		}

		const frames = this.frames;

		if (lastTime > time) { // Apply after lastTime for looped animations.
			this.apply(skeleton, lastTime, Number.MAX_VALUE, [], alpha, false, false, false, false);
			lastTime = -1;
		} else if (lastTime >= frames[frames.length - 1]) // Last time is after last frame.
			return;
		if (time < frames[0]) return;

		if (lastTime < frames[0] || time >= frames[Timeline.search(frames, lastTime) + 1]) {
			if (constraint != null)
				constraint.reset(skeleton);
			else {
				for (const constraint of skeleton.physics) {
					if (constraint.active) constraint.reset(skeleton);
				}
			}
		}
	}
}

/** Changes {@link SliderPose.time}. */
export class SliderTimeline extends ConstraintTimeline1 {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.sliderTime);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex] as Slider;
		if (constraint.active) {
			const pose = appliedPose ? constraint.appliedPose : constraint.pose;
			pose.time = this.getAbsoluteValue(time, alpha, fromSetup, add, pose.time, constraint.data.setupPose.time);
		}
	}
}

/** Changes {@link SliderPose.mix}. */
export class SliderMixTimeline extends ConstraintTimeline1 {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.sliderMix);
		this.additive = true;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, fromSetup: boolean,
		add: boolean, out: boolean, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex] as Slider;
		if (constraint.active) {
			const pose = appliedPose ? constraint.appliedPose : constraint.pose;
			pose.mix = this.getAbsoluteValue(time, alpha, fromSetup, add, pose.mix, constraint.data.setupPose.mix);
		}
	}
}
