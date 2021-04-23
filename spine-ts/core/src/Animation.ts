/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

module spine {

	/** A simple container for a list of timelines and a name. */
	export class Animation {
		/** The animation's name, which is unique across all animations in the skeleton. */
		name: string;
		timelines: Array<Timeline>;
		timelineIds: StringSet;

		/** The duration of the animation in seconds, which is the highest time of all keys in the timeline. */
		duration: number;

		constructor (name: string, timelines: Array<Timeline>, duration: number) {
			if (name == null) throw new Error("name cannot be null.");
			if (timelines == null) throw new Error("timelines cannot be null.");
			this.name = name;
			this.timelines = timelines;
			this.timelineIds = new StringSet();
			for (var i = 0; i < timelines.length; i++)
				this.timelineIds.addAll(timelines[i].getPropertyIds());
			this.duration = duration;
		}

		hasTimeline(ids: string[]) {
			for (let i = 0; i < ids.length; i++) {
				if (this.timelineIds.contains(ids[i])) return true;
			}
			return false;
		}

		/** Applies all the animation's timelines to the specified skeleton.
		 *
		 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection)}.
		 * @param loop If true, the animation repeats after {@link #getDuration()}.
		 * @param events May be null to ignore fired events. */
		apply (skeleton: Skeleton, lastTime: number, time: number, loop: boolean, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			if (skeleton == null) throw new Error("skeleton cannot be null.");

			if (loop && this.duration != 0) {
				time %= this.duration;
				if (lastTime > 0) lastTime %= this.duration;
			}

			let timelines = this.timelines;
			for (let i = 0, n = timelines.length; i < n; i++)
				timelines[i].apply(skeleton, lastTime, time, events, alpha, blend, direction);
		}

		static search (frames: ArrayLike<number>, time: number) {
			let n = frames.length;
			for (let i = 1; i < n; i++)
				if (frames[i] > time) return i - 1;
			return n - 1;
		}

		static search2 (values: ArrayLike<number>, time: number, step: number) {
			let n = values.length;
			for (let i = step; i < n; i += step)
				if (values[i] > time) return i - step;
			return n - step;
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

	export enum Property {
		rotate, x, y, scaleX, scaleY, shearX, shearY, //
		rgb, alpha, rgb2, //
		attachment, deform, //
		event, drawOrder, //
		ikConstraint, transformConstraint, //
		pathConstraintPosition, pathConstraintSpacing, pathConstraintMix

	}

	/** The interface for all timelines. */
	export abstract class Timeline {
		propertyIds: string[];
		frames: ArrayLike<number>;

		constructor(frameCount: number, propertyIds: string[]) {
			this.propertyIds = propertyIds;
			this.frames = Utils.newFloatArray(frameCount * this.getFrameEntries());
		}

		getPropertyIds () {
			return this.propertyIds;
		}

		abstract getFrameEntries (): number;

		getFrameCount () {
			return this.frames.length / this.getFrameEntries();
		}

		getDuration (): number {
			return this.frames[this.frames.length - this.getFrameEntries()];
		}

		abstract apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection): void;
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
		static LINEAR = 0; static STEPPED = 1; static BEZIER = 2;
		static BEZIER_SIZE = 18;

		protected curves: ArrayLike<number>; // type, x, y, ...

		constructor (frameCount: number, bezierCount: number, propertyIds: string[]) {
			super(frameCount, propertyIds);
			this.curves = Utils.newFloatArray(frameCount + bezierCount * CurveTimeline.BEZIER_SIZE);
			this.curves[frameCount - 1] = CurveTimeline.STEPPED;
		}

		/** Sets the specified key frame to linear interpolation. */
		setLinear (frame: number) {
			this.curves[frame] = CurveTimeline.LINEAR;
		}

		/** Sets the specified key frame to stepped interpolation. */
		setStepped (frame: number) {
			this.curves[frame] = CurveTimeline.STEPPED;
		}

		/** Shrinks the storage for Bezier curves, for use when <code>bezierCount</code> (specified in the constructor) was larger
		 * than the actual number of Bezier curves. */
		shrink (bezierCount: number) {
			let size = this.getFrameCount() + bezierCount * CurveTimeline.BEZIER_SIZE;
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
			let i = this.getFrameCount() + bezier * CurveTimeline.BEZIER_SIZE;
			if (value == 0) curves[frame] = CurveTimeline.BEZIER + i;
			let tmpx = (time1 - cx1 * 2 + cx2) * 0.03, tmpy = (value1 - cy1 * 2 + cy2) * 0.03;
			let dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy = ((cy1 - cy2) * 3 - value1 + value2) * 0.006;
			let ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
			let dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667, dy = (cy1 - value1) * 0.3 + tmpy + dddy * 0.16666667;
			let x = time1 + dx, y = value1 + dy;
			for (let n = i + CurveTimeline.BEZIER_SIZE; i < n; i += 2) {
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
			let frames = this.frames;
			if (curves[i] > time) {
				let x = frames[frameIndex], y = frames[frameIndex + valueOffset];
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
			}
			let n = i + CurveTimeline.BEZIER_SIZE;
			for (i += 2; i < n; i += 2) {
				if (curves[i] >= time) {
					let x = curves[i - 2], y = curves[i - 1];
					return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
				}
			}
			frameIndex += this.getFrameEntries();
			let x = curves[n - 2], y = curves[n - 1];
			return y + (time - x) / (frames[frameIndex] - x) * (frames[frameIndex + valueOffset] - y);
		}
	}

	export abstract class CurveTimeline1 extends CurveTimeline {
		static ENTRIES = 2;
		static VALUE = 1;

		constructor(frameCount: number, bezierCount: number, propertyIds: string[]) {
			super(frameCount, bezierCount, propertyIds);
		}

		getFrameEntries() {
			return CurveTimeline1.ENTRIES;
		}

		/** Sets the time and value for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		setFrame (frame: number, time: number, value: number) {
			frame <<= 1;
			this.frames[frame] = time;
			this.frames[frame + CurveTimeline1.VALUE] = value;
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
			case CurveTimeline.LINEAR:
				let before = frames[i], value = frames[i + CurveTimeline1.VALUE];
				return value + (time - before) / (frames[i + CurveTimeline1.ENTRIES] - before) * (frames[i + CurveTimeline1.ENTRIES + CurveTimeline1.VALUE] - value);
			case CurveTimeline.STEPPED:
				return frames[i + CurveTimeline1.VALUE];
			}
			return this.getBezierValue(time, i, CurveTimeline1.VALUE, curveType - CurveTimeline1.BEZIER);
		}
	}

	/** The base class for a {@link CurveTimeline} which sets two properties. */
	export abstract class CurveTimeline2 extends CurveTimeline {
		static ENTRIES = 3;
		static VALUE1 = 1;
		static VALUE2 = 2;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
		 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
		constructor (frameCount: number, bezierCount: number, propertyIds: string[]) {
			super(frameCount, bezierCount, propertyIds);
		}

		getFrameEntries () {
			return CurveTimeline2.ENTRIES;
		}

		/** Sets the time and values for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		setFrame (frame: number, time: number, value1: number, value2: number) {
			frame *= CurveTimeline2.ENTRIES;
			let frames = this.frames;
			frames[frame] = time;
			frames[frame + CurveTimeline2.VALUE1] = value1;
			frames[frame + CurveTimeline2.VALUE2] = value2;
		}
	}

	/** Changes a bone's local {@link Bone#rotation}. */
	export class RotateTimeline extends CurveTimeline1 implements BoneTimeline {
		boneIndex = 0;

		constructor (frameCount: number, bezierCount: number, boneIndex: number) {
			super(frameCount, bezierCount, [
				Property.rotate + "|" + boneIndex
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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
			super(frameCount, bezierCount, [
				Property.x + "|" + boneIndex,
				Property.y + "|" + boneIndex,
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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
			let i = Animation.search2(frames, time, CurveTimeline2.ENTRIES);
			let curveType = this.curves[i / CurveTimeline2.ENTRIES];
			switch (curveType) {
			case CurveTimeline.LINEAR:
				let before = frames[i];
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
				let t = (time - before) / (frames[i + CurveTimeline2.ENTRIES] - before);
				x += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE1] - x) * t;
				y += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE2] - y) * t;
				break;
			case CurveTimeline.STEPPED:
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
				break;
			default:
				x = this.getBezierValue(time, i, CurveTimeline2.VALUE1, curveType - CurveTimeline.BEZIER);
				y = this.getBezierValue(time, i, CurveTimeline2.VALUE2, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
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
			super(frameCount, bezierCount, [
				Property.x + "|" + boneIndex
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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
			super(frameCount, bezierCount, [
				Property.y + "|" + boneIndex
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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
			super(frameCount, bezierCount, [
				Property.scaleX + "|" + boneIndex,
				Property.scaleY + "|" + boneIndex
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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

			let x = 0, y = 0;
			let i = Animation.search2(frames, time, CurveTimeline2.ENTRIES);
			let curveType = this.curves[i / CurveTimeline2.ENTRIES];
			switch (curveType) {
			case CurveTimeline.LINEAR:
				let before = frames[i];
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
				let t = (time - before) / (frames[i + CurveTimeline2.ENTRIES] - before);
				x += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE1] - x) * t;
				y += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE2] - y) * t;
				break;
			case CurveTimeline.STEPPED:
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
				break;
			default:
				x = this.getBezierValue(time, i, CurveTimeline2.VALUE1, curveType - CurveTimeline2.BEZIER);
				y = this.getBezierValue(time, i, CurveTimeline2.VALUE2, curveType + CurveTimeline2.BEZIER_SIZE - CurveTimeline2.BEZIER);
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
						bx = bone.scaleX;
						by = bone.scaleY;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bone.data.scaleX) * alpha;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - bone.data.scaleY) * alpha;
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
						bx = MathUtils.signum(x);
						by = MathUtils.signum(y);
						bone.scaleX = Math.abs(bone.scaleX) * bx + (x - Math.abs(bone.data.scaleX) * bx) * alpha;
						bone.scaleY = Math.abs(bone.scaleY) * by + (y - Math.abs(bone.data.scaleY) * by) * alpha;
					}
				}
			}
		}
	}

	/** Changes a bone's local {@link Bone#scaleX)} and {@link Bone#scaleY}. */
	export class ScaleXTimeline extends CurveTimeline1 implements BoneTimeline {
		boneIndex = 0;

		constructor (frameCount: number, bezierCount: number, boneIndex: number) {
			super(frameCount, bezierCount, [
				Property.scaleX + "|" + boneIndex
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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
						bx = bone.scaleX;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bone.data.scaleX) * alpha;
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
						bx = MathUtils.signum(x);
						bone.scaleX = Math.abs(bone.scaleX) * bx + (x - Math.abs(bone.data.scaleX) * bx) * alpha;
					}
				}
			}
		}
	}

	/** Changes a bone's local {@link Bone#scaleX)} and {@link Bone#scaleY}. */
	export class ScaleYTimeline extends CurveTimeline1 implements BoneTimeline {
		boneIndex = 0;

		constructor (frameCount: number, bezierCount: number, boneIndex: number) {
			super(frameCount, bezierCount, [
				Property.scaleY + "|" + boneIndex
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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
						by = bone.scaleY;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - bone.data.scaleY) * alpha;
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
						by = MathUtils.signum(y);
						bone.scaleY = Math.abs(bone.scaleY) * by + (y - Math.abs(bone.data.scaleY) * by) * alpha;
					}
				}
			}
		}
	}

	/** Changes a bone's local {@link Bone#shearX} and {@link Bone#shearY}. */
	export class ShearTimeline extends CurveTimeline2 implements BoneTimeline {
		boneIndex = 0;

		constructor (frameCount: number, bezierCount: number, boneIndex: number) {
			super(frameCount, bezierCount, [
				Property.shearX + "|" + boneIndex,
				Property.shearY + "|" + boneIndex
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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
			let i = Animation.search2(frames, time, CurveTimeline2.ENTRIES);
			let curveType = this.curves[i / CurveTimeline2.ENTRIES];
			switch (curveType) {
			case CurveTimeline2.LINEAR:
				let before = frames[i];
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
				let t = (time - before) / (frames[i + CurveTimeline2.ENTRIES] - before);
				x += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE1] - x) * t;
				y += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE2] - y) * t;
				break;
			case CurveTimeline2.STEPPED:
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
				break;
			default:
				x = this.getBezierValue(time, i, CurveTimeline2.VALUE1, curveType - CurveTimeline2.BEZIER);
				y = this.getBezierValue(time, i, CurveTimeline2.VALUE2, curveType + CurveTimeline2.BEZIER_SIZE - CurveTimeline2.BEZIER);
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
			super(frameCount, bezierCount, [
				Property.shearX + "|" + boneIndex
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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
			super(frameCount, bezierCount, [
				Property.shearY + "|" + boneIndex
			]);
			this.boneIndex = boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (!bone.active) return;

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
		static ENTRIES = 5;

		static R = 1; static G = 2; static B = 3; static A = 4;

		slotIndex = 0;

		constructor (frameCount: number, bezierCount: number, slotIndex: number) {
			super(frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex,
				Property.alpha + "|" + slotIndex
			]);
			this.slotIndex = slotIndex;
		}

		getFrameEntries () {
			return RGBATimeline.ENTRIES;
		}

		/** Sets the time in seconds, red, green, blue, and alpha for the specified key frame. */
		setFrame (frame: number, time: number, r: number, g: number, b: number, a: number) {
			frame *= RGBATimeline.ENTRIES;
			this.frames[frame] = time;
			this.frames[frame + RGBATimeline.R] = r;
			this.frames[frame + RGBATimeline.G] = g;
			this.frames[frame + RGBATimeline.B] = b;
			this.frames[frame + RGBATimeline.A] = a;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active) return;

			let frames = this.frames;
			if (time < frames[0]) {
				let color = slot.color, setup = slot.data.color;
				switch (blend) {
				case MixBlend.setup:
					color.setFromColor(slot.data.color);
					return;
				case MixBlend.first:
					color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha);
				}
				return;
			}

			let r = 0, g = 0, b = 0, a = 0;
			let i = Animation.search2(frames, time, RGBATimeline.ENTRIES);
			let curveType = this.curves[i / RGBATimeline.ENTRIES];
			switch (curveType) {
			case RGBATimeline.LINEAR:
				let before = frames[i];
				r = frames[i + RGBATimeline.R];
				g = frames[i + RGBATimeline.G];
				b = frames[i + RGBATimeline.B];
				a = frames[i + RGBATimeline.A];
				let t = (time - before) / (frames[i + RGBATimeline.ENTRIES] - before);
				r += (frames[i + RGBATimeline.ENTRIES + RGBATimeline.R] - r) * t;
				g += (frames[i + RGBATimeline.ENTRIES + RGBATimeline.G] - g) * t;
				b += (frames[i + RGBATimeline.ENTRIES + RGBATimeline.B] - b) * t;
				a += (frames[i + RGBATimeline.ENTRIES + RGBATimeline.A] - a) * t;
				break;
			case RGBATimeline.STEPPED:
				r = frames[i + RGBATimeline.R];
				g = frames[i + RGBATimeline.G];
				b = frames[i + RGBATimeline.B];
				a = frames[i + RGBATimeline.A];
				break;
			default:
				r = this.getBezierValue(time, i, RGBATimeline.R, curveType - RGBATimeline.BEZIER);
				g = this.getBezierValue(time, i, RGBATimeline.G, curveType + RGBATimeline.BEZIER_SIZE - RGBATimeline.BEZIER);
				b = this.getBezierValue(time, i, RGBATimeline.B, curveType + RGBATimeline.BEZIER_SIZE * 2 - RGBATimeline.BEZIER);
				a = this.getBezierValue(time, i, RGBATimeline.A, curveType + RGBATimeline.BEZIER_SIZE * 3 - RGBATimeline.BEZIER);
			}
			let color = slot.color;
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
		static ENTRIES = 4;

		static R = 1; static G = 2; static B = 3;

		slotIndex = 0;

		constructor (frameCount: number, bezierCount: number, slotIndex: number) {
			super(frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex
			]);
			this.slotIndex = slotIndex;
		}

		getFrameEntries () {
			return RGBTimeline.ENTRIES;
		}

		/** Sets the time in seconds, red, green, blue, and alpha for the specified key frame. */
		setFrame (frame: number, time: number, r: number, g: number, b: number) {
			frame *= RGBTimeline.ENTRIES;
			this.frames[frame] = time;
			this.frames[frame + RGBTimeline.R] = r;
			this.frames[frame + RGBTimeline.G] = g;
			this.frames[frame + RGBTimeline.B] = b;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active) return;

			let frames = this.frames;
			if (time < frames[0]) {
				let color = slot.color, setup = slot.data.color;
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
			let i = Animation.search2(frames, time, RGBTimeline.ENTRIES);
			let curveType = this.curves[i / RGBTimeline.ENTRIES];
			switch (curveType) {
			case RGBTimeline.LINEAR:
				let before = frames[i];
				r = frames[i + RGBTimeline.R];
				g = frames[i + RGBTimeline.G];
				b = frames[i + RGBTimeline.B];
				let t = (time - before) / (frames[i + RGBTimeline.ENTRIES] - before);
				r += (frames[i + RGBTimeline.ENTRIES + RGBTimeline.R] - r) * t;
				g += (frames[i + RGBTimeline.ENTRIES + RGBTimeline.G] - g) * t;
				b += (frames[i + RGBTimeline.ENTRIES + RGBTimeline.B] - b) * t;
				break;
			case RGBATimeline.STEPPED:
				r = frames[i + RGBTimeline.R];
				g = frames[i + RGBTimeline.G];
				b = frames[i + RGBTimeline.B];
				break;
			default:
				r = this.getBezierValue(time, i, RGBTimeline.R, curveType - RGBTimeline.BEZIER);
				g = this.getBezierValue(time, i, RGBTimeline.G, curveType + RGBTimeline.BEZIER_SIZE - RGBTimeline.BEZIER);
				b = this.getBezierValue(time, i, RGBTimeline.B, curveType + RGBTimeline.BEZIER_SIZE * 2 - RGBTimeline.BEZIER);
			}
			let color = slot.color;
			if (alpha == 1) {
				color.r = r;
				color.g = g;
				color.b = b;
			} else {
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
			super(frameCount, bezierCount, [
				Property.alpha + "|" + slotIndex
			]);
			this.slotIndex = slotIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active) return;

			if (time < frames[0]) { // Time is before first frame.
				let color = slot.color, setup = slot.data.color;
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
				slot.color.a = a;
			else {
				if (blend == MixBlend.setup) slot.color.a = slot.data.color.a;
				slot.color.a += (a - slot.color.a) * alpha;
			}
		}
	}

	/** Changes a slot's {@link Slot#color} and {@link Slot#darkColor} for two color tinting. */
	export class RGBA2Timeline extends CurveTimeline implements SlotTimeline{
		static ENTRIES = 8;

		static R = 1; static G = 2; static B = 3; static A = 4; static R2 = 5; static G2 = 6; static B2 = 7;

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
			return RGBA2Timeline.ENTRIES;
		}

		/** Sets the time in seconds, light, and dark colors for the specified key frame. */
		setFrame (frame: number, time: number, r: number, g: number, b: number, a: number, r2: number, g2: number, b2: number) {
			frame *= RGBA2Timeline.ENTRIES;
			this.frames[frame] = time;
			this.frames[frame + RGBA2Timeline.R] = r;
			this.frames[frame + RGBA2Timeline.G] = g;
			this.frames[frame + RGBA2Timeline.B] = b;
			this.frames[frame + RGBA2Timeline.A] = a;
			this.frames[frame + RGBA2Timeline.R2] = r2;
			this.frames[frame + RGBA2Timeline.G2] = g2;
			this.frames[frame + RGBA2Timeline.B2] = b2;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active) return;

			let frames = this.frames;
			if (time < frames[0]) {
				let light = slot.color, dark = slot.darkColor, setupLight = slot.data.color, setupDark = slot.data.darkColor;
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
			let i = Animation.search2(frames, time, RGBA2Timeline.ENTRIES);
			let curveType = this.curves[i >> 3];
			switch (curveType) {
			case RGBA2Timeline.LINEAR:
				let before = frames[i];
				r = frames[i + RGBA2Timeline.R];
				g = frames[i + RGBA2Timeline.G];
				b = frames[i + RGBA2Timeline.B];
				a = frames[i + RGBA2Timeline.A];
				r2 = frames[i + RGBA2Timeline.R2];
				g2 = frames[i + RGBA2Timeline.G2];
				b2 = frames[i + RGBA2Timeline.B2];
				let t = (time - before) / (frames[i + RGBA2Timeline.ENTRIES] - before);
				r += (frames[i + RGBA2Timeline.ENTRIES + RGBA2Timeline.R] - r) * t;
				g += (frames[i + RGBA2Timeline.ENTRIES + RGBA2Timeline.G] - g) * t;
				b += (frames[i + RGBA2Timeline.ENTRIES + RGBA2Timeline.B] - b) * t;
				a += (frames[i + RGBA2Timeline.ENTRIES + RGBA2Timeline.A] - a) * t;
				r2 += (frames[i + RGBA2Timeline.ENTRIES + RGBA2Timeline.R2] - r2) * t;
				g2 += (frames[i + RGBA2Timeline.ENTRIES + RGBA2Timeline.G2] - g2) * t;
				b2 += (frames[i + RGBA2Timeline.ENTRIES + RGBA2Timeline.B2] - b2) * t;
				break;
			case RGBA2Timeline.STEPPED:
				r = frames[i + RGBA2Timeline.R];
				g = frames[i + RGBA2Timeline.G];
				b = frames[i + RGBA2Timeline.B];
				a = frames[i + RGBA2Timeline.A];
				r2 = frames[i + RGBA2Timeline.R2];
				g2 = frames[i + RGBA2Timeline.G2];
				b2 = frames[i + RGBA2Timeline.B2];
				break;
			default:
				r = this.getBezierValue(time, i, RGBA2Timeline.R, curveType - RGBA2Timeline.BEZIER);
				g = this.getBezierValue(time, i, RGBA2Timeline.G, curveType + RGBA2Timeline.BEZIER_SIZE - RGBA2Timeline.BEZIER);
				b = this.getBezierValue(time, i, RGBA2Timeline.B, curveType + RGBA2Timeline.BEZIER_SIZE * 2 - RGBA2Timeline.BEZIER);
				a = this.getBezierValue(time, i, RGBA2Timeline.A, curveType + RGBA2Timeline.BEZIER_SIZE * 3 - RGBA2Timeline.BEZIER);
				r2 = this.getBezierValue(time, i, RGBA2Timeline.R2, curveType + RGBA2Timeline.BEZIER_SIZE * 4 - RGBA2Timeline.BEZIER);
				g2 = this.getBezierValue(time, i, RGBA2Timeline.G2, curveType + RGBA2Timeline.BEZIER_SIZE * 5 - RGBA2Timeline.BEZIER);
				b2 = this.getBezierValue(time, i, RGBA2Timeline.B2, curveType + RGBA2Timeline.BEZIER_SIZE * 6 - RGBA2Timeline.BEZIER);
			}

			let light = slot.color, dark = slot.darkColor;
			if (alpha == 1) {
				light.set(r, g, b, a);
				dark.r = r2;
				dark.g = g2;
				dark.b = b2;
			} else {
				if (blend == MixBlend.setup) {
					light.setFromColor(slot.data.color);
					dark.setFromColor(slot.data.darkColor);
				}
				light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
				dark.r += (r2 - dark.r) * alpha;
				dark.g += (g2 - dark.g) * alpha;
				dark.b += (b2 - dark.b) * alpha;
			}
		}
	}

	/** Changes a slot's {@link Slot#color} and {@link Slot#darkColor} for two color tinting. */
	export class RGB2Timeline extends CurveTimeline implements SlotTimeline{
		static ENTRIES = 7;

		static R = 1; static G = 2; static B = 3; static R2 = 4; static G2 = 5; static B2 = 6;

		slotIndex = 0;

		constructor (frameCount: number, bezierCount: number, slotIndex: number) {
			super(frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex,
				Property.rgb2 + "|" + slotIndex
			]);
			this.slotIndex = slotIndex;
		}

		getFrameEntries () {
			return RGB2Timeline.ENTRIES;
		}

		/** Sets the time in seconds, light, and dark colors for the specified key frame. */
		setFrame (frame: number, time: number, r: number, g: number, b: number, r2: number, g2: number, b2: number) {
			frame *= RGB2Timeline.ENTRIES;
			this.frames[frame] = time;
			this.frames[frame + RGB2Timeline.R] = r;
			this.frames[frame + RGB2Timeline.G] = g;
			this.frames[frame + RGB2Timeline.B] = b;
			this.frames[frame + RGB2Timeline.R2] = r2;
			this.frames[frame + RGB2Timeline.G2] = g2;
			this.frames[frame + RGB2Timeline.B2] = b2;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active) return;

			let frames = this.frames;
			if (time < frames[0]) {
				let light = slot.color, dark = slot.darkColor, setupLight = slot.data.color, setupDark = slot.data.darkColor;
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
			let i = Animation.search2(frames, time, RGB2Timeline.ENTRIES);
			let curveType = this.curves[i >> 3];
			switch (curveType) {
			case RGB2Timeline.LINEAR:
				let before = frames[i];
				r = frames[i + RGB2Timeline.R];
				g = frames[i + RGB2Timeline.G];
				b = frames[i + RGB2Timeline.B];
				r2 = frames[i + RGB2Timeline.R2];
				g2 = frames[i + RGB2Timeline.G2];
				b2 = frames[i + RGB2Timeline.B2];
				let t = (time - before) / (frames[i + RGB2Timeline.ENTRIES] - before);
				r += (frames[i + RGB2Timeline.ENTRIES + RGB2Timeline.R] - r) * t;
				g += (frames[i + RGB2Timeline.ENTRIES + RGB2Timeline.G] - g) * t;
				b += (frames[i + RGB2Timeline.ENTRIES + RGB2Timeline.B] - b) * t;
				r2 += (frames[i + RGB2Timeline.ENTRIES + RGB2Timeline.R2] - r2) * t;
				g2 += (frames[i + RGB2Timeline.ENTRIES + RGB2Timeline.G2] - g2) * t;
				b2 += (frames[i + RGB2Timeline.ENTRIES + RGB2Timeline.B2] - b2) * t;
				break;
			case RGB2Timeline.STEPPED:
				r = frames[i + RGB2Timeline.R];
				g = frames[i + RGB2Timeline.G];
				b = frames[i + RGB2Timeline.B];
				r2 = frames[i + RGB2Timeline.R2];
				g2 = frames[i + RGB2Timeline.G2];
				b2 = frames[i + RGB2Timeline.B2];
				break;
			default:
				r = this.getBezierValue(time, i, RGB2Timeline.R, curveType - RGB2Timeline.BEZIER);
				g = this.getBezierValue(time, i, RGB2Timeline.G, curveType + RGB2Timeline.BEZIER_SIZE - RGB2Timeline.BEZIER);
				b = this.getBezierValue(time, i, RGB2Timeline.B, curveType + RGB2Timeline.BEZIER_SIZE * 2 - RGB2Timeline.BEZIER);
				r2 = this.getBezierValue(time, i, RGB2Timeline.R2, curveType + RGB2Timeline.BEZIER_SIZE * 3 - RGB2Timeline.BEZIER);
				g2 = this.getBezierValue(time, i, RGB2Timeline.G2, curveType + RGB2Timeline.BEZIER_SIZE * 4 - RGB2Timeline.BEZIER);
				b2 = this.getBezierValue(time, i, RGB2Timeline.B2, curveType + RGB2Timeline.BEZIER_SIZE * 5 - RGB2Timeline.BEZIER);
			}

			let light = slot.color, dark = slot.darkColor;
			if (alpha == 1) {
				light.r = r;
				light.g = g;
				light.b = b;
				dark.r = r2;
				dark.g = g2;
				dark.b = b2;
			} else {
				if (blend == MixBlend.setup) {
					let setupLight = slot.data.color, setupDark = slot.data.darkColor;
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
		attachmentNames: Array<string>;

		constructor (frameCount: number, slotIndex: number) {
			super(frameCount, [
				Property.attachment + "|" + slotIndex
			]);
			this.slotIndex = slotIndex;
			this.attachmentNames = new Array<string>(frameCount);
		}

		getFrameEntries () {
			return 1;
		}

		/** The number of key frames for this timeline. */
		getFrameCount () {
			return this.frames.length;
		}

		/** Sets the time in seconds and the attachment name for the specified key frame. */
		setFrame (frame: number, time: number, attachmentName: string) {
			this.frames[frame] = time;
			this.attachmentNames[frame] = attachmentName;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active) return;

			if (direction == MixDirection.mixOut) {
				if (blend == MixBlend.setup)
					this.setAttachment(skeleton, slot, slot.data.attachmentName);
				return;
			}

			let frames = this.frames;
			if (time < frames[0]) {
				if (blend == MixBlend.setup || blend == MixBlend.first) this.setAttachment(skeleton, slot, slot.data.attachmentName);
				return;
			}

			this.setAttachment(skeleton, slot, this.attachmentNames[Animation.search(frames, time)]);
		}

		setAttachment(skeleton: Skeleton, slot: Slot, attachmentName: string) {
			slot.setAttachment(attachmentName == null ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
		}
	}

	let zeros : ArrayLike<number> = null;

	/** Changes a slot's {@link Slot#deform} to deform a {@link VertexAttachment}. */
	export class DeformTimeline extends CurveTimeline implements SlotTimeline {
		slotIndex = 0;

		/** The attachment that will be deformed. */
		attachment: VertexAttachment;

		/** The vertices for each key frame. */
		vertices: Array<ArrayLike<number>>;

		constructor (frameCount: number, bezierCount: number, slotIndex: number, attachment: VertexAttachment) {
			super(frameCount, bezierCount, [
				Property.deform + "|" + slotIndex + "|" + attachment.id
			]);
			this.slotIndex = slotIndex;
			this.attachment = attachment;
			this.vertices = new Array<ArrayLike<number>>(frameCount);
			if (zeros == null) zeros = Utils.newFloatArray(64);
		}

		getFrameEntries () {
			return 1;
		}

		/** Sets the time in seconds and the vertices for the specified key frame.
		 * @param vertices Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights. */
		setFrame (frame: number, time: number, vertices: ArrayLike<number>) {
			this.frames[frame] = time;
			this.vertices[frame] = vertices;
		}

		/** @param value1 Ignored (0 is used for a deform timeline).
		 * @param value2 Ignored (1 is used for a deform timeline). */
		setBezier (bezier: number, frame: number, value: number, time1: number, value1: number, cx1: number, cy1: number, cx2: number,
			cy2: number, time2: number, value2: number) {
			let curves = this.curves;
			let i = this.getFrameCount() + bezier * DeformTimeline.BEZIER_SIZE;
			if (value == 0) curves[frame] = DeformTimeline.BEZIER + i;
			let tmpx = (time1 - cx1 * 2 + cx2) * 0.03, tmpy = cy2 * 0.03 - cy1 * 0.06;
			let dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy = (cy1 - cy2 + 0.33333333) * 0.018;
			let ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
			let dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667, dy = cy1 * 0.3 + tmpy + dddy * 0.16666667;
			let x = time1 + dx, y = dy;
			for (let n = i + DeformTimeline.BEZIER_SIZE; i < n; i += 2) {
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
			let frames = this.frames;
			let i = curves[frame];
			switch (i) {
			case DeformTimeline.LINEAR:
				let x = frames[frame];
				return (time - x) / (frames[frame + this.getFrameEntries()] - x);
			case DeformTimeline.STEPPED:
				return 0;
			}
			i -= DeformTimeline.BEZIER;
			if (curves[i] > time) {
				let x = frames[frame];
				return curves[i + 1] * (time - x) / (curves[i] - x);
			}
			let n = i + DeformTimeline.BEZIER_SIZE;
			for (i += 2; i < n; i += 2) {
				if (curves[i] >= time) {
					let x = curves[i - 2], y = curves[i - 1];
					return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
				}
			}
			let x = curves[n - 2], y = curves[n - 1];
			return y + (1 - y) * (time - x) / (frames[frame + this.getFrameEntries()] - x);
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let slot: Slot = skeleton.slots[this.slotIndex];
			if (!slot.bone.active) return;
			let slotAttachment: Attachment = slot.getAttachment();
			if (!(slotAttachment instanceof VertexAttachment) || !((<VertexAttachment>slotAttachment).deformAttachment == this.attachment)) return;

			let deformArray: Array<number> = slot.deform;
			if (deformArray.length == 0) blend = MixBlend.setup;

			let vertices = this.vertices;
			let vertexCount = vertices[0].length;

			let frames = this.frames;
			if (time < frames[0]) {
				let vertexAttachment = <VertexAttachment>slotAttachment;
				switch (blend) {
				case MixBlend.setup:
					deformArray.length = 0;
					return;
				case MixBlend.first:
					if (alpha == 1) {
						deformArray.length = 0;
						break;
					}
					let deform: Array<number> = Utils.setArraySize(deformArray, vertexCount);
					if (vertexAttachment.bones == null) {
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

			let deform: Array<number> = Utils.setArraySize(deformArray, vertexCount);
			if (time >= frames[frames.length - 1]) { // Time is after last frame.
				let lastVertices = vertices[frames.length - 1];
				if (alpha == 1) {
					if (blend == MixBlend.add) {
						let vertexAttachment = slotAttachment as VertexAttachment;
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, with alpha.
							let setupVertices = vertexAttachment.vertices;
							for (let i = 0; i < vertexCount; i++) {
								deform[i] += lastVertices[i] - setupVertices[i];
							}
						} else {
							// Weighted deform offsets, with alpha.
							for (let i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i];
						}
					} else {
						Utils.arrayCopy(lastVertices, 0, deform, 0, vertexCount);
					}
				} else {
					switch (blend) {
					case MixBlend.setup: {
						let vertexAttachment = slotAttachment as VertexAttachment;
						if (vertexAttachment.bones == null) {
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
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, with alpha.
							let setupVertices = vertexAttachment.vertices;
							for (let i = 0; i < vertexCount; i++) {
								deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
							}
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
			let frame = Animation.search(frames, time);
			let percent = this.getCurvePercent(time, frame);
			let prevVertices = vertices[frame];
			let nextVertices = vertices[frame + 1];

			if (alpha == 1) {
				if (blend == MixBlend.add) {
					let vertexAttachment = slotAttachment as VertexAttachment;
					if (vertexAttachment.bones == null) {
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
					if (vertexAttachment.bones == null) {
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
					if (vertexAttachment.bones == null) {
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
		static propertyIds = [ "" + Property.event ];

		/** The event for each key frame. */
		events: Array<Event>;

		constructor (frameCount: number) {
			super(frameCount, EventTimeline.propertyIds);

			this.events = new Array<Event>(frameCount);
		}

		getFrameEntries () {
			return 1;
		}

		/** Sets the time in seconds and the event for the specified key frame. */
		setFrame (frame: number, event: Event) {
			this.frames[frame] = event.time;
			this.events[frame] = event;
		}

		/** Fires events for frames > `lastTime` and <= `time`. */
		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			if (firedEvents == null) return;

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
				i = Animation.search(frames, lastTime) + 1;
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
		static propertyIds = [ "" + Property.drawOrder ];

		/** The draw order for each key frame. See {@link #setFrame(int, float, int[])}. */
		drawOrders: Array<Array<number>>;

		constructor (frameCount: number) {
			super(frameCount, DrawOrderTimeline.propertyIds);
			this.drawOrders = new Array<Array<number>>(frameCount);
		}

		getFrameEntries () {
			return 1;
		}

		/** Sets the time in seconds and the draw order for the specified key frame.
		 * @param drawOrder For each slot in {@link Skeleton#slots}, the index of the new draw order. May be null to use setup pose
		 *           draw order. */
		setFrame (frame: number, time: number, drawOrder: Array<number>) {
			this.frames[frame] = time;
			this.drawOrders[frame] = drawOrder;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let drawOrder: Array<Slot> = skeleton.drawOrder;
			let slots: Array<Slot> = skeleton.slots;
			if (direction == MixDirection.mixOut) {
				if (blend == MixBlend.setup) Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
				return;
			}

			let frames = this.frames;
			if (time < frames[0]) {
				if (blend == MixBlend.setup || blend == MixBlend.first) Utils.arrayCopy(skeleton.slots, 0, drawOrder, 0, skeleton.slots.length);
				return;
			}

			let drawOrderToSetupIndex = this.drawOrders[Animation.search(frames, time)];
			if (drawOrderToSetupIndex == null)
				Utils.arrayCopy(slots, 0, drawOrder, 0, slots.length);
			else {
				for (let i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
					drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		}
	}

	/** Changes an IK constraint's {@link IkConstraint#mix}, {@link IkConstraint#softness},
	 * {@link IkConstraint#bendDirection}, {@link IkConstraint#stretch}, and {@link IkConstraint#compress}. */
	export class IkConstraintTimeline extends CurveTimeline {
		static ENTRIES = 6;

		static MIX = 1; static SOFTNESS = 2; static BEND_DIRECTION = 3; static COMPRESS = 4; static STRETCH = 5;

		/** The index of the IK constraint slot in {@link Skeleton#ikConstraints} that will be changed. */
		ikConstraintIndex: number;

		constructor (frameCount: number, bezierCount: number, ikConstraintIndex: number) {
			super(frameCount, bezierCount, [
				Property.ikConstraint + "|" + ikConstraintIndex
			]);
			this.ikConstraintIndex = ikConstraintIndex;
		}

		getFrameEntries () {
			return IkConstraintTimeline.ENTRIES;
		}

		/** Sets the time in seconds, mix, softness, bend direction, compress, and stretch for the specified key frame. */
		setFrame (frame: number, time: number, mix: number, softness: number, bendDirection: number, compress: boolean, stretch: boolean) {
			frame *= IkConstraintTimeline.ENTRIES;
			this.frames[frame] = time;
			this.frames[frame + IkConstraintTimeline.MIX] = mix;
			this.frames[frame + IkConstraintTimeline.SOFTNESS] = softness;
			this.frames[frame + IkConstraintTimeline.BEND_DIRECTION] = bendDirection;
			this.frames[frame + IkConstraintTimeline.COMPRESS] = compress ? 1 : 0;
			this.frames[frame + IkConstraintTimeline.STRETCH] = stretch ? 1 : 0;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;
			let constraint: IkConstraint = skeleton.ikConstraints[this.ikConstraintIndex];
			if (!constraint.active) return;

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
			let i = Animation.search2(frames, time, IkConstraintTimeline.ENTRIES)
			let curveType = this.curves[i / IkConstraintTimeline.ENTRIES];
			switch (curveType) {
			case IkConstraintTimeline.LINEAR:
				let before = frames[i];
				mix = frames[i + IkConstraintTimeline.MIX];
				softness = frames[i + IkConstraintTimeline.SOFTNESS];
				let t = (time - before) / (frames[i + IkConstraintTimeline.ENTRIES] - before);
				mix += (frames[i + IkConstraintTimeline.ENTRIES + IkConstraintTimeline.MIX] - mix) * t;
				softness += (frames[i + IkConstraintTimeline.ENTRIES + IkConstraintTimeline.SOFTNESS] - softness) * t;
				break;
			case IkConstraintTimeline.STEPPED:
				mix = frames[i + IkConstraintTimeline.MIX];
				softness = frames[i + IkConstraintTimeline.SOFTNESS];
				break;
			default:
				mix = this.getBezierValue(time, i, IkConstraintTimeline.MIX, curveType - IkConstraintTimeline.BEZIER);
				softness = this.getBezierValue(time, i, IkConstraintTimeline.SOFTNESS, curveType + IkConstraintTimeline.BEZIER_SIZE - IkConstraintTimeline.BEZIER);
			}

			if (blend == MixBlend.setup) {
				constraint.mix = constraint.data.mix + (mix - constraint.data.mix) * alpha;
				constraint.softness = constraint.data.softness + (softness - constraint.data.softness) * alpha;

				if (direction == MixDirection.mixOut) {
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
				} else {
					constraint.bendDirection = frames[i + IkConstraintTimeline.BEND_DIRECTION];
					constraint.compress = frames[i + IkConstraintTimeline.COMPRESS] != 0;
					constraint.stretch = frames[i + IkConstraintTimeline.STRETCH] != 0;
				}
			} else {
				constraint.mix += (mix - constraint.mix) * alpha;
				constraint.softness += (softness - constraint.softness) * alpha;
				if (direction == MixDirection.mixIn) {
					constraint.bendDirection = frames[i + IkConstraintTimeline.BEND_DIRECTION];
					constraint.compress = frames[i + IkConstraintTimeline.COMPRESS] != 0;
					constraint.stretch = frames[i + IkConstraintTimeline.STRETCH] != 0;
				}
			}
		}
	}

	/** Changes a transform constraint's {@link TransformConstraint#rotateMix}, {@link TransformConstraint#translateMix},
	 * {@link TransformConstraint#scaleMix}, and {@link TransformConstraint#shearMix}. */
	export class TransformConstraintTimeline extends CurveTimeline {
		static ENTRIES = 7;

		static ROTATE = 1; static X = 2; static Y = 3; static SCALEX = 4; static SCALEY = 5; static SHEARY = 6;

		/** The index of the transform constraint slot in {@link Skeleton#transformConstraints} that will be changed. */
		transformConstraintIndex: number;

		constructor (frameCount: number, bezierCount: number, transformConstraintIndex: number) {
			super(frameCount, bezierCount, [
				Property.transformConstraint + "|" + transformConstraintIndex
			]);
			this.transformConstraintIndex = transformConstraintIndex;
		}

		getFrameEntries () {
			return TransformConstraintTimeline.ENTRIES;
		}

		/** The time in seconds, rotate mix, translate mix, scale mix, and shear mix for the specified key frame. */
		setFrame (frame: number, time: number, mixRotate: number, mixX: number, mixY: number, mixScaleX: number, mixScaleY: number,
			mixShearY: number) {
			let frames = this.frames;
			frame *= TransformConstraintTimeline.ENTRIES;
			this.frames[frame] = time;
			frames[frame + TransformConstraintTimeline.ROTATE] = mixRotate;
			frames[frame + TransformConstraintTimeline.X] = mixX;
			frames[frame + TransformConstraintTimeline.Y] = mixY;
			frames[frame + TransformConstraintTimeline.SCALEX] = mixScaleX;
			frames[frame + TransformConstraintTimeline.SCALEY] = mixScaleY;
			frames[frame + TransformConstraintTimeline.SHEARY] = mixShearY;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;

			let constraint: TransformConstraint = skeleton.transformConstraints[this.transformConstraintIndex];
			if (!constraint.active) return;

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
			let i = Animation.search2(frames, time, TransformConstraintTimeline.ENTRIES);
			let curveType = this.curves[i / TransformConstraintTimeline.ENTRIES];
			let ROTATE = TransformConstraintTimeline.ROTATE;
			let X = TransformConstraintTimeline.X;
			let Y = TransformConstraintTimeline.Y;
			let SCALEX = TransformConstraintTimeline.SCALEX;
			let SCALEY = TransformConstraintTimeline.SCALEY;
			let SHEARY = TransformConstraintTimeline.SHEARY;
			let ENTRIES = TransformConstraintTimeline.ENTRIES;
			let BEZIER = TransformConstraintTimeline.BEZIER;
			let BEZIER_SIZE = TransformConstraintTimeline.BEZIER_SIZE;
			switch (curveType) {
			case TransformConstraintTimeline.LINEAR:
				let before = frames[i];
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				scaleX = frames[i + SCALEX];
				scaleY = frames[i + SCALEY];
				shearY = frames[i + SHEARY];
				let t = (time - before) / (frames[i + ENTRIES] - before);
				rotate += (frames[i + ENTRIES + ROTATE] - rotate) * t;
				x += (frames[i + ENTRIES + X] - x) * t;
				y += (frames[i + ENTRIES + Y] - y) * t;
				scaleX += (frames[i + ENTRIES + SCALEX] - scaleX) * t;
				scaleY += (frames[i + ENTRIES + SCALEY] - scaleY) * t;
				shearY += (frames[i + ENTRIES + SHEARY] - shearY) * t;
				break;
			case TransformConstraintTimeline.STEPPED:
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				scaleX = frames[i + SCALEX];
				scaleY = frames[i + SCALEY];
				shearY = frames[i + SHEARY];
				break;
			default:
				rotate = this.getBezierValue(time, i, ROTATE, curveType - BEZIER);
				x = this.getBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER);
				y = this.getBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER);
				scaleX = this.getBezierValue(time, i, SCALEX, curveType + BEZIER_SIZE * 3 - BEZIER);
				scaleY = this.getBezierValue(time, i, SCALEY, curveType + BEZIER_SIZE * 4 - BEZIER);
				shearY = this.getBezierValue(time, i, SHEARY, curveType + BEZIER_SIZE * 5 - BEZIER);
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
		pathConstraintIndex: number;

		constructor (frameCount: number, bezierCount: number, pathConstraintIndex: number) {
			super(frameCount, bezierCount, [
				Property.pathConstraintPosition + "|" + pathConstraintIndex
			]);
			this.pathConstraintIndex = pathConstraintIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;
			let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (!constraint.active) return;

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
			super(frameCount, bezierCount, [
				Property.pathConstraintSpacing + "|" + pathConstraintIndex
			]);
			this.pathConstraintIndex = pathConstraintIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;
			let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (!constraint.active) return;

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
		static ENTRIES = 4;
		static ROTATE = 1; static X = 2; static Y = 3;

		constructor (frameCount: number, bezierCount: number, pathConstraintIndex: number) {
			super(frameCount, bezierCount, [
				Property.pathConstraintMix + "|" + pathConstraintIndex
			]);
			this.pathConstraintIndex = pathConstraintIndex;
		}

		getFrameEntries() {
			return PathConstraintMixTimeline.ENTRIES;
		}

		setFrame (frame: number, time: number, mixRotate: number, mixX: number, mixY: number) {
			let frames = this.frames;
			frame <<= 2;
			frames[frame] = time;
			frames[frame + PathConstraintMixTimeline.ROTATE] = mixRotate;
			frames[frame + PathConstraintMixTimeline.X] = mixX;
			frames[frame + PathConstraintMixTimeline.Y] = mixY;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, blend: MixBlend, direction: MixDirection) {
			let frames = this.frames;
			let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (!constraint.active) return;

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
			let i = Animation.search2(frames, time, PathConstraintMixTimeline.ENTRIES);
			let curveType = this.curves[i >> 2];
			switch (curveType) {
			case PathConstraintMixTimeline.LINEAR:
				let before = frames[i];
				rotate = frames[i + PathConstraintMixTimeline.ROTATE];
				x = frames[i + PathConstraintMixTimeline.X];
				y = frames[i + PathConstraintMixTimeline.Y];
				let t = (time - before) / (frames[i + PathConstraintMixTimeline.ENTRIES] - before);
				rotate += (frames[i + PathConstraintMixTimeline.ENTRIES + PathConstraintMixTimeline.ROTATE] - rotate) * t;
				x += (frames[i + PathConstraintMixTimeline.ENTRIES + PathConstraintMixTimeline.X] - x) * t;
				y += (frames[i + PathConstraintMixTimeline.ENTRIES + PathConstraintMixTimeline.Y] - y) * t;
				break;
			case PathConstraintMixTimeline.STEPPED:
				rotate = frames[i + PathConstraintMixTimeline.ROTATE];
				x = frames[i + PathConstraintMixTimeline.X];
				y = frames[i + PathConstraintMixTimeline.Y];
				break;
			default:
				rotate = this.getBezierValue(time, i, PathConstraintMixTimeline.ROTATE, curveType - PathConstraintMixTimeline.BEZIER);
				x = this.getBezierValue(time, i, PathConstraintMixTimeline.X, curveType + PathConstraintMixTimeline.BEZIER_SIZE - PathConstraintMixTimeline.BEZIER);
				y = this.getBezierValue(time, i, PathConstraintMixTimeline.Y, curveType + PathConstraintMixTimeline.BEZIER_SIZE * 2 - PathConstraintMixTimeline.BEZIER);
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
}
