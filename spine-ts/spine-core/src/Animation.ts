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

import { type Attachment, VertexAttachment } from "./attachments/Attachment.js";
import type { HasTextureRegion } from "./attachments/HasTextureRegion.js";
import { SequenceMode, SequenceModeValues } from "./attachments/Sequence.js";
import type { Inherit } from "./BoneData.js";
import type { BoneLocal } from "./BoneLocal.js";
import type { Event } from "./Event.js";
import { PathConstraint } from "./PathConstraint.js";
import type { PhysicsConstraint } from "./PhysicsConstraint.js";
import type { PhysicsConstraintData } from "./PhysicsConstraintData.js";
import type { PhysicsConstraintPose } from "./PhysicsConstraintPose.js";
import type { Skeleton } from "./Skeleton.js";
import type { Slot } from "./Slot.js";
import type { SlotPose } from "./SlotPose.js";
import { MathUtils, type NumberArrayLike, StringSet, Utils } from "./Utils.js";

/** A simple container for a list of timelines and a name. */
export class Animation {
	/** The animation's name, which is unique across all animations in the skeleton. */
	readonly name: string;

	/** If the returned array or the timelines it contains are modified, {@link setTimelines()} must be called. */
	timelines: Array<Timeline> = [];

	readonly timelineIds: StringSet;
	readonly bones: Array<number>;

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
			this.timelineIds.addAll(timeline.getPropertyIds());
			if (isBoneTimeline(timeline) && boneSet.add(timeline.boneIndex))
				this.bones.push(timeline.boneIndex);
		}
	}

	hasTimeline (ids: string[]): boolean {
		for (let i = 0; i < ids.length; i++)
			if (this.timelineIds.contains(ids[i])) return true;
		return false;
	}

	/** Applies the animation's timelines to the specified skeleton.
	 *
	 * See Timeline {@link Timeline.apply}.
	 * @param skeleton The skeleton the animation is being applied to. This provides access to the bones, slots, and other skeleton
	 *           components the timelines may change.
	 * @param lastTime The last time in seconds this animation was applied. Some timelines trigger only at specific times rather
	 *           than every frame. Pass -1 the first time an animation is applied to ensure frame 0 is triggered.
	 * @param time The time in seconds the skeleton is being posed for. Most timelines find the frame before and the frame after
	 *           this time and interpolate between the frame values. If beyond the {@link duration} and <code>loop</code> is
	 *           true then the animation will repeat, else the last frame will be applied.
	 * @param loop If true, the animation repeats after the {@link duration}.
	 * @param events If any events are fired, they are added to this list. Can be null to ignore fired events or if no timelines
	 *           fire events.
	 * @param alpha 0 applies the current or setup values (depending on <code>blend</code>). 1 applies the timeline values. Between
	 *           0 and 1 applies values between the current or setup values and the timeline values. By adjusting
	 *           <code>alpha</code> over time, an animation can be mixed in or out. <code>alpha</code> can also be useful to apply
	 *           animations on top of each other (layering).
	 * @param blend Controls how mixing is applied when <code>alpha</code> < 1.
	 * @param direction Indicates whether the timelines are mixing in or out. Used by timelines which perform instant transitions,
	 *           such as {@link DrawOrderTimeline} or {@link AttachmentTimeline}.
	 * @param appliedPose True to to modify the applied pose. */
	apply (skeleton: Skeleton, lastTime: number, time: number, loop: boolean, events: Array<Event> | null, alpha: number,
		blend: MixBlend, direction: MixDirection, appliedPose: boolean) {
		if (!skeleton) throw new Error("skeleton cannot be null.");

		if (loop && this.duration !== 0) {
			time %= this.duration;
			if (lastTime > 0) lastTime %= this.duration;
		}

		const timelines = this.timelines;
		for (let i = 0, n = timelines.length; i < n; i++)
			timelines[i].apply(skeleton, lastTime, time, events, alpha, blend, direction, appliedPose);
	}
}

/** Controls how a timeline value is mixed with the setup pose value or current pose value when a timeline's `alpha`
 * < 1.
 *
 * See Timeline {@link Timeline.apply}. */
export enum MixBlend {
	/** Transitions between the setup and timeline values (the current value is not used). Before the first frame, the setup
	 * value is used.
	 * <p>
	 * <code>setup</code> is intended to transition to or from the setup pose, not for animations layered on top of others. */
	setup,
	/** Transitions between the current and timeline values. Before the first frame, transitions between the current and setup
	 * values. Timelines which perform instant transitions, such as {@link DrawOrderTimeline} or {@link AttachmentTimeline}, use
	 * the setup value before the first frame.
	 * <p>
	 * <code>first</code> is intended for the first animations applied, not for animations layered on top of others. */
	first,
	/** Transitions between the current and timeline values. No change is made before the first frame.
	 * <p>
	 * <code>replace</code> is intended for animations layered on top of others, not for the first animations applied. */
	replace,
	/** Transitions between the current value and the current plus timeline values. No change is made before the first frame.
	 * <p>
	 * <code>add</code> is intended for animations layered on top of others, not for the first animations applied.
	 * <p>
	 * Properties set by additive animations must be set manually or by another animation before applying the additive
	 * animations, else the property values will increase each time the additive animations are applied. */
	add
}

/** Indicates whether a timeline's `alpha` is mixing out over time toward 0 (the setup or current pose value) or
 * mixing in toward 1 (the timeline's value).
 *
 * See Timeline {@link Timeline#apply}. */
export enum MixDirection {
	in, out
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

/** The interface for all timelines. */
export abstract class Timeline {
	propertyIds: string[];
	frames: NumberArrayLike;

	constructor (frameCount: number, ...propertyIds: string[]) {
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

	/** Applies this timeline to the skeleton.
	 * @param skeleton The skeleton to which the timeline is being applied. This provides access to the bones, slots, and other
	 *           skeleton components that the timeline may change.
	 * @param lastTime The last time in seconds this timeline was applied. Timelines such as {@link EventTimeline} trigger only
	 *           at specific times rather than every frame. In that case, the timeline triggers everything between
	 *           <code>lastTime</code> (exclusive) and <code>time</code> (inclusive). Pass -1 the first time an animation is
	 *           applied to ensure frame 0 is triggered.
	 * @param time The time in seconds that the skeleton is being posed for. Most timelines find the frame before and the frame
	 *           after this time and interpolate between the frame values. If beyond the last frame, the last frame will be
	 *           applied.
	 * @param events If any events are fired, they are added to this list. Can be null to ignore fired events or if the timeline
	 *           does not fire events.
	 * @param alpha 0 applies the current or setup value (depending on <code>blend</code>). 1 applies the timeline value.
	 *           Between 0 and 1 applies a value between the current or setup value and the timeline value. By adjusting
	 *           <code>alpha</code> over time, an animation can be mixed in or out. <code>alpha</code> can also be useful to
	 *           apply animations on top of each other (layering).
	 * @param blend Controls how mixing is applied when <code>alpha</code> < 1.
	 * @param direction Indicates whether the timeline is mixing in or out. Used by timelines which perform instant transitions,
	 *           such as {@link DrawOrderTimeline} or {@link AttachmentTimeline}, and others such as {@link ScaleTimeline}.
	 * @param appliedPose True to to modify the applied pose. */
	abstract apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event> | null, alpha: number,
		blend: MixBlend, direction: MixDirection, appliedPose: boolean): void;

	/** Linear search using the specified stride (default 1).
	 * @param time Must be >= the first value in <code>frames</code>.
	 * @return The index of the first value <= <code>time</code>. */
	static search (frames: NumberArrayLike, time: number, step = 1) {
		const n = frames.length;
		for (let i = step; i < n; i += step)
			if (frames[i] > time) return i - step;
		return n - step;
	}
}

/** An interface for timelines which change the property of a slot. */
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

	/** Shrinks the storage for Bezier curves, for use when <code>bezierCount</code> (specified in the constructor) was larger
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
	 * @param frameIndex The index into {@link #getFrames()} for the values of the frame before <code>time</code>.
	 * @param valueOffset The offset from <code>frameIndex</code> to the value this curve is used for.
	 * @param i The index of the Bezier segments. See {@link #getCurveType(int)}. */
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

export abstract class CurveTimeline1 extends CurveTimeline {
	constructor (frameCount: number, bezierCount: number, propertyId: string) {
		super(frameCount, bezierCount, propertyId);
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

	getRelativeValue (time: number, alpha: number, blend: MixBlend, current: number, setup: number) {
		if (time < this.frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					return setup;
				case MixBlend.first:
					return current + (setup - current) * alpha;
			}
			return current;
		}
		const value = this.getCurveValue(time);
		switch (blend) {
			case MixBlend.setup: return setup + value * alpha;
			case MixBlend.first:
			case MixBlend.replace: return current + (value + setup - current) * alpha;
			case MixBlend.add: return current + value * alpha;
		}
	}

	getAbsoluteValue (time: number, alpha: number, blend: MixBlend, current: number, setup: number, value?: number) {
		if (value === undefined)
			return this.getAbsoluteValue1(time, alpha, blend, current, setup);
		else
			return this.getAbsoluteValue2(time, alpha, blend, current, setup, value);
	}

	private getAbsoluteValue1 (time: number, alpha: number, blend: MixBlend, current: number, setup: number) {
		if (time < this.frames[0]) {
			switch (blend) {
				case MixBlend.setup: return setup;
				case MixBlend.first: return current + (setup - current) * alpha;
				default: return current;
			}
		}
		const value = this.getCurveValue(time);
		switch (blend) {
			case MixBlend.setup: return setup + (value - setup) * alpha;
			case MixBlend.first:
			case MixBlend.replace: return current + (value - current) * alpha;
			case MixBlend.add: return current + value * alpha;
		}
	}

	private getAbsoluteValue2 (time: number, alpha: number, blend: MixBlend, current: number, setup: number, value: number) {
		if (time < this.frames[0]) {
			switch (blend) {
				case MixBlend.setup: return setup;
				case MixBlend.first: return current + (setup - current) * alpha;
				default: return current;
			}
		}
		switch (blend) {
			case MixBlend.setup: return setup + (value - setup) * alpha;
			case MixBlend.first:
			case MixBlend.replace: return current + (value - current) * alpha;
			case MixBlend.add: return current + value * alpha;
		}
	}

	getScaleValue (time: number, alpha: number, blend: MixBlend, direction: MixDirection, current: number, setup: number) {
		const frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup: return setup;
				case MixBlend.first: return current + (setup - current) * alpha;
				default: return current;
			}
		}
		const value = this.getCurveValue(time) * setup;
		if (alpha === 1) return blend === MixBlend.add ? current + value - setup : value;
		// Mixing out uses sign of setup or current pose, else use sign of key.
		if (direction === MixDirection.out) {
			switch (blend) {
				case MixBlend.setup:
					return setup + (Math.abs(value) * MathUtils.signum(setup) - setup) * alpha;
				case MixBlend.first:
				case MixBlend.replace:
					return current + (Math.abs(value) * MathUtils.signum(current) - current) * alpha;
			}
		} else {
			let s = 0;
			switch (blend) {
				case MixBlend.setup:
					s = Math.abs(setup) * MathUtils.signum(value);
					return s + (value - s) * alpha;
				case MixBlend.first:
				case MixBlend.replace:
					s = Math.abs(current) * MathUtils.signum(value);
					return s + (value - s) * alpha;
			}
		}
		return current + (value - setup) * alpha;
	}
}

/** The base class for a {@link CurveTimeline} that is a {@link BoneTimeline} and sets two properties. */
export abstract class BoneTimeline2 extends CurveTimeline implements BoneTimeline {
	readonly boneIndex;

	/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
	 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
	constructor (frameCount: number, bezierCount: number, boneIndex: number, property1: Property, property2: Property) {
		super(frameCount, bezierCount, `${property1}|${boneIndex}`, `${property2}|${boneIndex}`);
		this.boneIndex = boneIndex;
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

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event> | null, alpha: number,
		blend: MixBlend, direction: MixDirection, appliedPose: boolean): void {
		const bone = skeleton.bones[this.boneIndex];
		if (bone.active) this.apply1(appliedPose ? bone.applied : bone.pose, bone.data.setup, time, alpha, blend, direction);
	}

	protected abstract apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend,
		direction: MixDirection): void;
}

export interface BoneTimeline {
	/** The index of the bone in {@link Skeleton.bones} that will be changed when this timeline is applied. */
	boneIndex: number;
}

export function isBoneTimeline (obj: Timeline & Partial<BoneTimeline>): obj is Timeline & BoneTimeline {
	return typeof obj === 'object' && obj !== null && typeof obj.boneIndex === 'number';
}

export abstract class BoneTimeline1 extends CurveTimeline1 implements BoneTimeline {
	readonly boneIndex: number;

	constructor (frameCount: number, bezierCount: number, boneIndex: number, property: Property) {
		super(frameCount, bezierCount, `${property}|${boneIndex}`);
		this.boneIndex = boneIndex;
	}

	public apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event> | null, alpha: number,
		blend: MixBlend, direction: MixDirection, appliedPose: boolean) {

		const bone = skeleton.bones[this.boneIndex];
		if (bone.active) this.apply1(appliedPose ? bone.applied : bone.pose, bone.data.setup, time, alpha, blend, direction);
	}

	protected abstract apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend,
		direction: MixDirection): void;
}

/** Changes a bone's local {@link Bone#rotation}. */
export class RotateTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.rotate);
	}

	apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		pose.rotation = this.getRelativeValue(time, alpha, blend, pose.rotation, setup.rotation);
	}
}

/** Changes a bone's local {@link BoneLocal.x} and {@link BoneLocal.y}. */
export class TranslateTimeline extends BoneTimeline2 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.x, Property.y);
	}

	apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		const frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					pose.x = setup.x;
					pose.y = setup.y;
					return;
				case MixBlend.first:
					pose.x += (setup.x - pose.x) * alpha;
					pose.y += (setup.y - pose.y) * alpha;
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

		switch (blend) {
			case MixBlend.setup:
				pose.x = setup.x + x * alpha;
				pose.y = setup.y + y * alpha;
				break;
			case MixBlend.first:
			case MixBlend.replace:
				pose.x += (setup.x + x - pose.x) * alpha;
				pose.y += (setup.y + y - pose.y) * alpha;
				break;
			case MixBlend.add:
				pose.x += x * alpha;
				pose.y += y * alpha;
		}
	}
}

/** Changes a bone's local {@link BoneLocal.x}. */
export class TranslateXTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.x);
	}

	protected apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		pose.x = this.getRelativeValue(time, alpha, blend, pose.x, setup.x);
	}
}

/** Changes a bone's local {@link BoneLocal.y}. */
export class TranslateYTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.y);
	}

	protected apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		pose.y = this.getRelativeValue(time, alpha, blend, pose.y, setup.y);
	}
}

/** Changes a bone's local {@link BoneLocal.scaleX} and {@link BoneLocal.scaleY}. */
export class ScaleTimeline extends BoneTimeline2 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.scaleX, Property.scaleY);
	}

	protected apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		const frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					pose.scaleX = setup.scaleX;
					pose.scaleY = setup.scaleY;
					return;
				case MixBlend.first:
					pose.scaleX += (setup.scaleX - pose.scaleX) * alpha;
					pose.scaleY += (setup.scaleY - pose.scaleY) * alpha;
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

		if (alpha === 1) {
			if (blend === MixBlend.add) {
				pose.scaleX += x - setup.scaleX;
				pose.scaleY += y - setup.scaleY;
			} else {
				pose.scaleX = x;
				pose.scaleY = y;
			}
		} else {
			let bx = 0, by = 0;
			if (direction === MixDirection.out) {
				switch (blend) {
					case MixBlend.setup:
						bx = setup.scaleX;
						by = setup.scaleY;
						pose.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
						pose.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						bx = pose.scaleX;
						by = pose.scaleY;
						pose.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
						pose.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
						break;
					case MixBlend.add:
						pose.scaleX += (x - setup.scaleX) * alpha;
						pose.scaleY += (y - setup.scaleY) * alpha;
				}
			} else {
				switch (blend) {
					case MixBlend.setup:
						bx = Math.abs(setup.scaleX) * MathUtils.signum(x);
						by = Math.abs(setup.scaleY) * MathUtils.signum(y);
						pose.scaleX = bx + (x - bx) * alpha;
						pose.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						bx = Math.abs(pose.scaleX) * MathUtils.signum(x);
						by = Math.abs(pose.scaleY) * MathUtils.signum(y);
						pose.scaleX = bx + (x - bx) * alpha;
						pose.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.add:
						pose.scaleX += (x - setup.scaleX) * alpha;
						pose.scaleY += (y - setup.scaleY) * alpha;
				}
			}
		}
	}
}

/** Changes a bone's local {@link BoneLocal.scaleX}. */
export class ScaleXTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.scaleX);
	}

	protected apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		pose.scaleX = this.getScaleValue(time, alpha, blend, direction, pose.scaleX, setup.scaleX);
	}
}

/** Changes a bone's local {@link BoneLocal.scaleY}. */
export class ScaleYTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.scaleY);
	}

	protected apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		pose.scaleY = this.getScaleValue(time, alpha, blend, direction, pose.scaleY, setup.scaleY);
	}
}

/** Changes a bone's local {@link Bone#shearX} and {@link Bone#shearY}. */
export class ShearTimeline extends BoneTimeline2 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.shearX, Property.shearY);
	}

	protected apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		const frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					pose.shearX = setup.shearX;
					pose.shearY = setup.shearY;
					return;
				case MixBlend.first:
					pose.shearX += (setup.shearX - pose.shearX) * alpha;
					pose.shearY += (setup.shearY - pose.shearY) * alpha;
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

		switch (blend) {
			case MixBlend.setup:
				pose.shearX = setup.shearX + x * alpha;
				pose.shearY = setup.shearY + y * alpha;
				break;
			case MixBlend.first:
			case MixBlend.replace:
				pose.shearX += (setup.shearX + x - pose.shearX) * alpha;
				pose.shearY += (setup.shearY + y - pose.shearY) * alpha;
				break;
			case MixBlend.add:
				pose.shearX += x * alpha;
				pose.shearY += y * alpha;
		}
	}
}

/** Changes a bone's local {@link Bone#shearX} and {@link Bone#shearY}. */
export class ShearXTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.shearX);
	}

	protected apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		pose.shearX = this.getRelativeValue(time, alpha, blend, pose.shearX, setup.shearX);
	}
}

/** Changes a bone's local {@link Bone#shearX} and {@link Bone#shearY}. */
export class ShearYTimeline extends BoneTimeline1 {
	constructor (frameCount: number, bezierCount: number, boneIndex: number) {
		super(frameCount, bezierCount, boneIndex, Property.shearY);
	}

	protected apply1 (pose: BoneLocal, setup: BoneLocal, time: number, alpha: number, blend: MixBlend, direction: MixDirection) {
		pose.shearY = this.getRelativeValue(time, alpha, blend, pose.shearY, setup.shearY);
	}
}

/** Changes a bone's {@link BoneLocal.inherit}. */
export class InheritTimeline extends Timeline implements BoneTimeline {
	readonly boneIndex: number;

	constructor (frameCount: number, boneIndex: number) {
		super(frameCount, `${Property.inherit}|${boneIndex}`);
		this.boneIndex = boneIndex;
	}

	public getFrameEntries () {
		return 2/*ENTRIES*/;
	}

	/** Sets the inherit transform mode for the specified frame.
	 * @param frame Between 0 and <code>frameCount</code>, inclusive.
	 * @param time The frame time in seconds. */
	public setFrame (frame: number, time: number, inherit: Inherit) {
		frame *= 2/*ENTRIES*/;
		this.frames[frame] = time;
		this.frames[frame + 1/*INHERIT*/] = inherit;
	}

	public apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const bone = skeleton.bones[this.boneIndex];
		if (!bone.active) return;
		const pose = appliedPose ? bone.applied : bone.pose;

		if (direction === MixDirection.out) {
			if (blend === MixBlend.setup) pose.inherit = bone.data.setup.inherit;
			return;
		}

		const frames = this.frames;
		if (time < frames[0]) {
			if (blend === MixBlend.setup || blend === MixBlend.first) pose.inherit = bone.data.setup.inherit;
		} else
			pose.inherit = this.frames[Timeline.search(frames, time, 2/*ENTRIES*/) + 1/*INHERIT*/];
	}
}

export abstract class SlotCurveTimeline extends CurveTimeline implements SlotTimeline {
	readonly slotIndex: number;

	constructor (frameCount: number, bezierCount: number, slotIndex: number, ...propertyIds: string[]) {
		super(frameCount, bezierCount, ...propertyIds);
		this.slotIndex = slotIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const slot = skeleton.slots[this.slotIndex];
		if (slot.bone.active) this.apply1(slot, appliedPose ? slot.applied : slot.pose, time, alpha, blend);
	}

	protected abstract apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, blend: MixBlend): void;
}

/** Changes a slot's {@link SlotPose.color}. */
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

	protected apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, blend: MixBlend) {
		const frames = this.frames;
		const color = pose.color;
		if (time < frames[0]) {
			const setup = slot.data.setup.color;
			switch (blend) {
				case MixBlend.setup: color.setFromColor(setup); break;
				case MixBlend.first: color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
					(setup.a - color.a) * alpha); break;
			}
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
			if (blend === MixBlend.setup) color.setFromColor(slot.data.setup.color);
			color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
		}
	}
}

/** Changes the RGB for a slot's {@link SlotPose.color}. */
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

	protected apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, blend: MixBlend) {
		const frames = this.frames;
		const color = pose.color;
		if (time < frames[0]) {
			const setup = slot.data.setup.color;
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
		if (alpha === 1) {
			color.r = r;
			color.g = g;
			color.b = b;
		} else {
			if (blend === MixBlend.setup) {
				const setup = slot.data.setup.color;
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

/** Changes the alpha for a slot's {@link SlotPose.color}. */
export class AlphaTimeline extends CurveTimeline1 implements SlotTimeline {
	slotIndex = 0;

	constructor (frameCount: number, bezierCount: number, slotIndex: number) {
		super(frameCount, bezierCount, `${Property.alpha}|${slotIndex}`);
		this.slotIndex = slotIndex;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;

		const color = (appliedPose ? slot.applied : slot.pose).color;
		const frames = this.frames;
		if (time < frames[0]) {
			const setup = slot.data.setup.color;
			switch (blend) {
				case MixBlend.setup: color.a = setup.a; break;
				case MixBlend.first: color.a += (setup.a - color.a) * alpha; break;
			}
			return;
		}

		const a = this.getCurveValue(time);
		if (alpha === 1)
			color.a = a;
		else {
			if (blend === MixBlend.setup) color.a = slot.data.setup.color.a;
			color.a += (a - color.a) * alpha;
		}
	}
}

/** Changes a slot's {@link SlotPose.color} and {@link SlotPose.darkColor} for two color tinting. */
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

	protected apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, blend: MixBlend) {
		const frames = this.frames;
		// biome-ignore lint/style/noNonNullAssertion: expected behavior from reference runtime
		const light = pose.color, dark = pose.darkColor!;
		if (time < frames[0]) {
			const setup = slot.data.setup;
			// biome-ignore lint/style/noNonNullAssertion: expected behavior from reference runtime
			const setupLight = setup.color, setupDark = setup.darkColor!;
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

		if (alpha === 1) {
			light.set(r, g, b, a);
			dark.r = r2;
			dark.g = g2;
			dark.b = b2;
		} else {
			if (blend === MixBlend.setup) {
				const setup = slot.data.setup;
				light.setFromColor(setup.color);
				// biome-ignore lint/style/noNonNullAssertion: expected behavior from reference runtime
				const setupDark = setup.darkColor!;
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

/** Changes a slot's {@link SlotPose.color} and {@link SlotPose.darkColor} for two color tinting. */
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

	protected apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, blend: MixBlend) {
		const frames = this.frames;
		// biome-ignore lint/style/noNonNullAssertion: expected behavior from reference runtime
		const light = pose.color, dark = pose.darkColor!;
		if (time < frames[0]) {
			const setup = slot.data.setup;
			// biome-ignore lint/style/noNonNullAssertion: expected behavior from reference runtime
			const setupLight = setup.color, setupDark = setup.darkColor!;
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

		let r = 0, g = 0, b = 0, r2 = 0, g2 = 0, b2 = 0;
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

		if (alpha === 1) {
			light.r = r;
			light.g = g;
			light.b = b;
			dark.r = r2;
			dark.g = g2;
			dark.b = b2;
		} else {
			if (blend === MixBlend.setup) {
				const setup = slot.data.setup;
				// biome-ignore lint/style/noNonNullAssertion: expected behavior from reference runtime
				const setupLight = setup.color, setupDark = setup.darkColor!;
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

/** Changes a slot's {@link SlotPose.ttachment}. */
export class AttachmentTimeline extends Timeline implements SlotTimeline {
	slotIndex = 0;

	/** The attachment name for each key frame. May contain null values to clear the attachment. */
	attachmentNames: Array<string | null>;

	constructor (frameCount: number, slotIndex: number) {
		super(frameCount, `${Property.attachment}|${slotIndex}`);
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

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;
		const pose = appliedPose ? slot.applied : slot.pose;

		if (direction === MixDirection.out) {
			if (blend === MixBlend.setup) this.setAttachment(skeleton, pose, slot.data.attachmentName);
		} else if (time < this.frames[0]) {
			if (blend === MixBlend.setup || blend === MixBlend.first) this.setAttachment(skeleton, pose, slot.data.attachmentName);
		} else
			this.setAttachment(skeleton, pose, this.attachmentNames[Timeline.search(this.frames, time)]);
	}

	setAttachment (skeleton: Skeleton, pose: SlotPose, attachmentName: string | null) {
		pose.setAttachment(!attachmentName ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
	}
}

/** Changes a slot's {@link SlotPose.deform} to deform a {@link VertexAttachment}. */
export class DeformTimeline extends SlotCurveTimeline {
	/** The attachment that will be deformed.
	 *
	 * See {@link VertexAttachment.getTimelineAttachment()}. */
	readonly attachment: VertexAttachment;

	/** The vertices for each key frame. */
	vertices: Array<NumberArrayLike>;

	constructor (frameCount: number, bezierCount: number, slotIndex: number, attachment: VertexAttachment) {
		super(frameCount, bezierCount, slotIndex, `${Property.deform}|${slotIndex}|${attachment.id}`);
		this.attachment = attachment;
		this.vertices = new Array<NumberArrayLike>(frameCount);
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time and vertices for the specified frame.
	 * @param frame Between 0 and <code>frameCount</code>, inclusive.
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

	protected apply1 (slot: Slot, pose: SlotPose, time: number, alpha: number, blend: MixBlend) {
		if (!(pose.attachment instanceof VertexAttachment)) return;
		const vertexAttachment = pose.attachment;
		if (vertexAttachment.timelineAttachment !== this.attachment) return;

		const deform = pose.deform;
		if (deform.length === 0) blend = MixBlend.setup;

		const vertices = this.vertices;
		const vertexCount = vertices[0].length;

		const frames = this.frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					deform.length = 0;
					return;
				case MixBlend.first:
					if (alpha === 1) {
						deform.length = 0;
						return;
					}
					deform.length = vertexCount;
					if (!vertexAttachment.bones) { // Unweighted vertex positions.
						const setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++)
							deform[i] += (setupVertices[i] - deform[i]) * alpha;
					} else { // Weighted deform offsets.
						alpha = 1 - alpha;
						for (let i = 0; i < vertexCount; i++)
							deform[i] *= alpha;
					}
			}
			return;
		}

		deform.length = vertexCount;
		if (time >= frames[frames.length - 1]) { // Time is after last frame.
			const lastVertices = vertices[frames.length - 1];
			if (alpha === 1) {
				if (blend === MixBlend.add) {
					if (!vertexAttachment.bones) { // Unweighted vertex positions, no alpha.
						const setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++)
							deform[i] += lastVertices[i] - setupVertices[i];
					} else { // Weighted deform offsets, no alpha.
						for (let i = 0; i < vertexCount; i++)
							deform[i] += lastVertices[i];
					}
				} else // Vertex positions or deform offsets, no alpha.
					Utils.arrayCopy(lastVertices, 0, deform, 0, vertexCount);
			} else {
				switch (blend) {
					case MixBlend.setup: {
						if (!vertexAttachment.bones) { // Unweighted vertex positions, with alpha.
							const setupVertices = vertexAttachment.vertices;
							for (let i = 0; i < vertexCount; i++) {
								const setup = setupVertices[i];
								deform[i] = setup + (lastVertices[i] - setup) * alpha;
							}
						} else { // Weighted deform offsets, with alpha.
							for (let i = 0; i < vertexCount; i++)
								deform[i] = lastVertices[i] * alpha;
						}
						break;
					}
					case MixBlend.first:
					case MixBlend.replace: // Vertex positions or deform offsets, with alpha.
						for (let i = 0; i < vertexCount; i++)
							deform[i] += (lastVertices[i] - deform[i]) * alpha;
						break;
					case MixBlend.add:
						if (!vertexAttachment.bones) { // Unweighted vertex positions, no alpha.
							const setupVertices = vertexAttachment.vertices;
							for (let i = 0; i < vertexCount; i++)
								deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
						} else { // Weighted deform offsets, alpha.
							for (let i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] * alpha;
						}
				}
			}
			return;
		}

		const frame = Timeline.search(frames, time);
		const percent = this.getCurvePercent(time, frame);
		const prevVertices = vertices[frame];
		const nextVertices = vertices[frame + 1];

		if (alpha === 1) {
			if (blend === MixBlend.add) {
				if (!vertexAttachment.bones) { // Unweighted vertex positions, no alpha.
					const setupVertices = vertexAttachment.vertices;
					for (let i = 0; i < vertexCount; i++) {
						const prev = prevVertices[i];
						deform[i] += prev + (nextVertices[i] - prev) * percent - setupVertices[i];
					}
				} else { // Weighted deform offsets, no alpha.
					for (let i = 0; i < vertexCount; i++) {
						const prev = prevVertices[i];
						deform[i] += prev + (nextVertices[i] - prev) * percent;
					}
				}
			} else if (percent === 0)
				Utils.arrayCopy(prevVertices, 0, deform, 0, vertexCount)
			else { // Vertex positions or deform offsets, no alpha.
				for (let i = 0; i < vertexCount; i++) {
					const prev = prevVertices[i];
					deform[i] = prev + (nextVertices[i] - prev) * percent;
				}
			}
		} else {
			switch (blend) {
				case MixBlend.setup: {
					if (!vertexAttachment.bones) { // Unweighted vertex positions, with alpha.
						const setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++) {
							const prev = prevVertices[i], setup = setupVertices[i];
							deform[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
						}
					} else { // Weighted deform offsets, with alpha.
						for (let i = 0; i < vertexCount; i++) {
							const prev = prevVertices[i];
							deform[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
						}
					}
					break;
				}
				case MixBlend.first:
				case MixBlend.replace: // Vertex positions or deform offsets, with alpha.
					for (let i = 0; i < vertexCount; i++) {
						const prev = prevVertices[i];
						deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
					}
					break;
				case MixBlend.add:
					if (!vertexAttachment.bones) { // Unweighted vertex positions, with alpha.
						const setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++) {
							const prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent - setupVertices[i]) * alpha;
						}
					} else { // Weighted deform offsets, with alpha.
						for (let i = 0; i < vertexCount; i++) {
							const prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent) * alpha;
						}
					}
			}
		}
	}
}

/** Changes a slot's {@link Slot#getSequenceIndex()} for an attachment's {@link Sequence}. */
export class SequenceTimeline extends Timeline implements SlotTimeline {
	static ENTRIES = 3;
	static MODE = 1;
	static DELAY = 2;

	readonly slotIndex: number;
	readonly attachment: HasTextureRegion;

	constructor (frameCount: number, slotIndex: number, attachment: HasTextureRegion) {
		// biome-ignore lint/style/noNonNullAssertion: expected behavior from reference runtime
		super(frameCount, `${Property.sequence}|${slotIndex}|${attachment.sequence!.id}`);
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
		const frames = this.frames;
		frame *= SequenceTimeline.ENTRIES;
		frames[frame] = time;
		frames[frame + SequenceTimeline.MODE] = mode | (index << 4);
		frames[frame + SequenceTimeline.DELAY] = delay;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const slot = skeleton.slots[this.slotIndex];
		if (!slot.bone.active) return;
		const pose = appliedPose ? slot.applied : slot.pose;

		const slotAttachment = pose.attachment;
		const attachment = this.attachment as unknown as Attachment;
		if (slotAttachment !== attachment) {
			if (!(slotAttachment instanceof VertexAttachment)
				|| slotAttachment.timelineAttachment !== attachment) return;
		}

		const sequence = (slotAttachment as unknown as HasTextureRegion).sequence;
		if (!sequence) return;

		if (direction === MixDirection.out) {
			if (blend === MixBlend.setup) pose.sequenceIndex = -1;
			return;
		}

		const frames = this.frames;
		if (time < frames[0]) {
			if (blend === MixBlend.setup || blend === MixBlend.first) pose.sequenceIndex = -1;
			return;
		}

		const i = Timeline.search(frames, time, SequenceTimeline.ENTRIES);
		const before = frames[i];
		const modeAndIndex = frames[i + SequenceTimeline.MODE];
		const delay = frames[i + SequenceTimeline.DELAY];

		let index = modeAndIndex >> 4, count = sequence.regions.length;
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
	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number,
		blend: MixBlend, direction: MixDirection, appliedPose: boolean) {

		if (!firedEvents) return;

		const frames = this.frames;
		const frameCount = this.frames.length;

		if (lastTime > time) { // Apply after lastTime for looped animations.
			this.apply(skeleton, lastTime, Number.MAX_VALUE, firedEvents, alpha, blend, direction, appliedPose);
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

/** Changes a skeleton's {@link Skeleton#drawOrder}. */
export class DrawOrderTimeline extends Timeline {
	static propertyIds = [`${Property.drawOrder}`];

	/** The draw order for each key frame. See {@link #setFrame(int, float, int[])}. */
	drawOrders: Array<Array<number> | null>;

	constructor (frameCount: number) {
		super(frameCount, ...DrawOrderTimeline.propertyIds);
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

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number,
		blend: MixBlend, direction: MixDirection, appliedPose: boolean) {

		if (direction === MixDirection.out) {
			if (blend === MixBlend.setup) Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
			return;
		}

		if (time < this.frames[0]) {
			if (blend === MixBlend.setup || blend === MixBlend.first) Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
			return;
		}

		const idx = Timeline.search(this.frames, time);
		const drawOrderToSetupIndex = this.drawOrders[idx];
		if (!drawOrderToSetupIndex)
			Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
		else {
			const drawOrder: Array<Slot> = skeleton.drawOrder;
			const slots: Array<Slot> = skeleton.slots;
			for (let i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
				drawOrder[i] = slots[drawOrderToSetupIndex[i]];
		}
	}
}

export interface ConstraintTimeline {
	/** The index of the constraint in {@link Skeleton.constraints} that will be changed when this timeline is applied, or
	 * -1 if a specific constraint will not be changed. */
	readonly constraintIndex: number;
}

export function isConstraintTimeline (obj: Timeline & Partial<ConstraintTimeline>): obj is Timeline & ConstraintTimeline {
	return typeof obj === 'object' && obj !== null && typeof obj.constraintIndex === 'number';
}

/** Changes an IK constraint's {@link IkConstraintPose.mix)}, {@link IkConstraintPose.softness},
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
	 * @param frame Between 0 and <code>frameCount</code>, inclusive.
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

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex];
		if (!constraint.active) return;
		const pose = appliedPose ? constraint.applied : constraint.pose;

		const frames = this.frames;
		if (time < frames[0]) {
			const setup = constraint.data.setup;
			switch (blend) {
				case MixBlend.setup:
					pose.mix = setup.mix;
					pose.softness = setup.softness;
					pose.bendDirection = setup.bendDirection;
					pose.compress = setup.compress;
					pose.stretch = setup.stretch;
					return;
				case MixBlend.first:
					pose.mix += (setup.mix - pose.mix) * alpha;
					pose.softness += (setup.softness - pose.softness) * alpha;
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

		switch (blend) {
			case MixBlend.setup: {
				const setup = constraint.data.setup;
				pose.mix = setup.mix + (mix - setup.mix) * alpha;
				pose.softness = setup.softness + (softness - setup.softness) * alpha;
				if (direction === MixDirection.out) {
					pose.bendDirection = setup.bendDirection;
					pose.compress = setup.compress;
					pose.stretch = setup.stretch;
					return;
				}
				break;
			}
			case MixBlend.first:
			case MixBlend.replace:
				pose.mix += (mix - pose.mix) * alpha;
				pose.softness += (softness - pose.softness) * alpha;
				if (direction === MixDirection.out) return;
				break;
			case MixBlend.add:
				pose.mix += mix * alpha;
				pose.softness += softness * alpha;
				if (direction === MixDirection.out) return;
				break;
		}
		pose.bendDirection = frames[i + 3/*BEND_DIRECTION*/];
		pose.compress = frames[i + 4/*COMPRESS*/] !== 0;
		pose.stretch = frames[i + 5/*STRETCH*/] !== 0;
	}
}

/** Changes a transform constraint's {@link TransformConstraintPose.mixRotate}, {@link TransformConstraintPose.mixX},
 * {@link TransformConstraintPose.mixY}, {@link TransformConstraintPose.mixScaleX},
 * {@link TransformConstraintPose.mixScaleY}, and {@link TransformConstraintPose.mixShearY}. */
export class TransformConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
	/** The index of the transform constraint slot in {@link Skeleton.transformConstraints} that will be changed. */
	constraintIndex: number = 0;

	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, `${Property.transformConstraint}|${constraintIndex}`);
		this.constraintIndex = constraintIndex;
	}

	getFrameEntries () {
		return 7/*ENTRIES*/;
	}

	/** Sets the time, rotate mix, translate mix, scale mix, and shear mix for the specified frame.
	 * @param frame Between 0 and <code>frameCount</code>, inclusive.
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

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex];
		if (!constraint.active) return;
		const pose = appliedPose ? constraint.applied : constraint.pose;

		const frames = this.frames;
		if (time < frames[0]) {
			const setup = constraint.data.setup;
			switch (blend) {
				case MixBlend.setup:
					pose.mixRotate = setup.mixRotate;
					pose.mixX = setup.mixX;
					pose.mixY = setup.mixY;
					pose.mixScaleX = setup.mixScaleX;
					pose.mixScaleY = setup.mixScaleY;
					pose.mixShearY = setup.mixShearY;
					return;
				case MixBlend.first:
					pose.mixRotate += (setup.mixRotate - pose.mixRotate) * alpha;
					pose.mixX += (setup.mixX - pose.mixX) * alpha;
					pose.mixY += (setup.mixY - pose.mixY) * alpha;
					pose.mixScaleX += (setup.mixScaleX - pose.mixScaleX) * alpha;
					pose.mixScaleY += (setup.mixScaleY - pose.mixScaleY) * alpha;
					pose.mixShearY += (setup.mixShearY - pose.mixShearY) * alpha;
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

		switch (blend) {
			case MixBlend.setup: {
				const setup = constraint.data.setup;
				pose.mixRotate = setup.mixRotate + (rotate - setup.mixRotate) * alpha;
				pose.mixX = setup.mixX + (x - setup.mixX) * alpha;
				pose.mixY = setup.mixY + (y - setup.mixY) * alpha;
				pose.mixScaleX = setup.mixScaleX + (scaleX - setup.mixScaleX) * alpha;
				pose.mixScaleY = setup.mixScaleY + (scaleY - setup.mixScaleY) * alpha;
				pose.mixShearY = setup.mixShearY + (shearY - setup.mixShearY) * alpha;
				break;
			}
			case MixBlend.first:
			case MixBlend.replace:
				pose.mixRotate += (rotate - pose.mixRotate) * alpha;
				pose.mixX += (x - pose.mixX) * alpha;
				pose.mixY += (y - pose.mixY) * alpha;
				pose.mixScaleX += (scaleX - pose.mixScaleX) * alpha;
				pose.mixScaleY += (scaleY - pose.mixScaleY) * alpha;
				pose.mixShearY += (shearY - pose.mixShearY) * alpha;
				break;
			case MixBlend.add:
				pose.mixRotate += rotate * alpha;
				pose.mixX += x * alpha;
				pose.mixY += y * alpha;
				pose.mixScaleX += scaleX * alpha;
				pose.mixScaleY += scaleY * alpha;
				pose.mixShearY += shearY * alpha;
				break;
		}
	}
}

export abstract class ConstraintTimeline1 extends CurveTimeline1 implements ConstraintTimeline {
	readonly constraintIndex: number;

	constructor (frameCount: number, bezierCount: number, constraintIndex: number, property: Property) {
		super(frameCount, bezierCount, `${property}|${constraintIndex}`);
		this.constraintIndex = constraintIndex;
	}
}

/** Changes a path constraint's {@link PathConstraintPose.position}. */
export class PathConstraintPositionTimeline extends ConstraintTimeline1 {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.pathConstraintPosition);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex];
		if (constraint.active) {
			const pose = appliedPose ? constraint.applied : constraint.pose;
			pose.position = this.getAbsoluteValue(time, alpha, blend, pose.position, constraint.data.setup.position);
		}
	}
}

/** Changes a path constraint's {@link PathConstraintPose.spacing}. */
export class PathConstraintSpacingTimeline extends ConstraintTimeline1 {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.pathConstraintSpacing);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex];
		if (constraint.active) {
			const pose = appliedPose ? constraint.applied : constraint.pose;
			pose.spacing = this.getAbsoluteValue(time, alpha, blend, pose.spacing, constraint.data.setup.spacing);
		}
	}
}

/** Changes a transform constraint's {@link PathConstraint.mixRotate()}, {@link PathConstraint.mixX()}, and
 * {@link PathConstraint.mixY()}. */
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
	 * @param frame Between 0 and <code>frameCount</code>, inclusive.
	 * @param time The frame time in seconds. */
	setFrame (frame: number, time: number, mixRotate: number, mixX: number, mixY: number) {
		const frames = this.frames;
		frame <<= 2;
		frames[frame] = time;
		frames[frame + 1/*ROTATE*/] = mixRotate;
		frames[frame + 2/*X*/] = mixX;
		frames[frame + 3/*Y*/] = mixY;
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex];
		if (!constraint.active) return;
		const pose = appliedPose ? constraint.applied : constraint.pose;

		const frames = this.frames;
		if (time < frames[0]) {
			const setup = constraint.data.setup;
			switch (blend) {
				case MixBlend.setup:
					pose.mixRotate = setup.mixRotate;
					pose.mixX = setup.mixX;
					pose.mixY = setup.mixY;
					return;
				case MixBlend.first:
					pose.mixRotate += (setup.mixRotate - pose.mixRotate) * alpha;
					pose.mixX += (setup.mixX - pose.mixX) * alpha;
					pose.mixY += (setup.mixY - pose.mixY) * alpha;
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

		switch (blend) {
			case MixBlend.setup: {
				const setup = constraint.data.setup;
				pose.mixRotate = setup.mixRotate + (rotate - setup.mixRotate) * alpha;
				pose.mixX = setup.mixX + (x - setup.mixX) * alpha;
				pose.mixY = setup.mixY + (y - setup.mixY) * alpha;
				break;
			}
			case MixBlend.first:
			case MixBlend.replace:
				pose.mixRotate += (rotate - pose.mixRotate) * alpha;
				pose.mixX += (x - pose.mixX) * alpha;
				pose.mixY += (y - pose.mixY) * alpha;
				break;
			case MixBlend.add:
				pose.mixRotate += rotate * alpha;
				pose.mixX += x * alpha;
				pose.mixY += y * alpha;
				break;
		}
	}
}

/** The base class for most {@link PhysicsConstraint} timelines. */
export abstract class PhysicsConstraintTimeline extends ConstraintTimeline1 {
	/** @param constraintIndex -1 for all physics constraints in the skeleton. */
	constructor (frameCount: number, bezierCount: number, constraintIndex: number, property: number) {
		super(frameCount, bezierCount, constraintIndex, property);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		if (this.constraintIndex === -1) {
			const value = time >= this.frames[0] ? this.getCurveValue(time) : 0;
			const constraints = skeleton.physics;
			for (const constraint of constraints) {
				if (constraint.active && this.global(constraint.data)) {
					const pose = appliedPose ? constraint.applied : constraint.pose;
					this.set(pose, this.getAbsoluteValue(time, alpha, blend, this.get(pose), this.get(constraint.data.setup), value));
				}
			}
		} else {
			const constraint = skeleton.constraints[this.constraintIndex];
			if (constraint.active) {
				const pose = appliedPose ? constraint.applied : constraint.pose;
				this.set(pose, this.getAbsoluteValue(time, alpha, blend, this.get(pose), this.get(constraint.data.setup)));
			}
		}
	}

	abstract get (pose: PhysicsConstraintPose): number;

	abstract set (pose: PhysicsConstraintPose, value: number): void;

	abstract global (constraint: PhysicsConstraintData): boolean;
}

/** Changes a physics constraint's {@link PhysicsConstraintPose.inertia}. */
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

/** Changes a physics constraint's {@link PhysicsConstraintPose.strength}. */
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

/** Changes a physics constraint's {@link PhysicsConstraintPose.damping}. */
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

/** Changes a physics constraint's {@link PhysicsConstraintPose.massInverse}. The timeline values are not inverted. */
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

/** Changes a physics constraint's {@link PhysicsConstraintPose.wind}. */
export class PhysicsConstraintWindTimeline extends PhysicsConstraintTimeline {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintWind);
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

/** Changes a physics constraint's {@link PhysicsConstraintPose.gravity}. */
export class PhysicsConstraintGravityTimeline extends PhysicsConstraintTimeline {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintGravity);
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

/** Changes a physics constraint's {@link PhysicsConstraintPose.mix}. */
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
	}

	getFrameCount () {
		return this.frames.length;
	}

	/** Sets the time for the specified frame.
	 * @param frame Between 0 and <code>frameCount</code>, inclusive. */
	setFrame (frame: number, time: number) {
		this.frames[frame] = time;
	}

	/** Resets the physics constraint when frames > <code>lastTime</code> and <= <code>time</code>. */
	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		let constraint: PhysicsConstraint | undefined;
		if (this.constraintIndex !== -1) {
			constraint = skeleton.constraints[this.constraintIndex] as PhysicsConstraint;
			if (!constraint.active) return;
		}

		const frames = this.frames;

		if (lastTime > time) { // Apply after lastTime for looped animations.
			this.apply(skeleton, lastTime, Number.MAX_VALUE, [], alpha, blend, direction, appliedPose);
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

/** Changes a slider's {@link SliderPose.time()}. */
export class SliderTimeline extends ConstraintTimeline1 {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.sliderTime);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex];
		if (constraint.active) {
			const pose = appliedPose ? constraint.applied : constraint.pose;
			pose.time = this.getAbsoluteValue(time, alpha, blend, pose.time, constraint.data.setup.time);
		}
	}
}

/** Changes a slider's {@link SliderPose.mix()}. */
export class SliderMixTimeline extends ConstraintTimeline1 {
	constructor (frameCount: number, bezierCount: number, constraintIndex: number) {
		super(frameCount, bezierCount, constraintIndex, Property.sliderMix);
	}

	apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend,
		direction: MixDirection, appliedPose: boolean) {

		const constraint = skeleton.constraints[this.constraintIndex];
		if (constraint.active) {
			const pose = appliedPose ? constraint.applied : constraint.pose;
			pose.mix = this.getAbsoluteValue(time, alpha, blend, pose.mix, constraint.data.setup.mix);
		}
	}
}
