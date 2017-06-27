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
	export class Animation {
		name: string;
		timelines: Array<Timeline>;
		duration: number;

		constructor (name: string, timelines: Array<Timeline>, duration: number) {
			if (name == null) throw new Error("name cannot be null.");
			if (timelines == null) throw new Error("timelines cannot be null.");
			this.name = name;
			this.timelines = timelines;
			this.duration = duration;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, loop: boolean, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			if (skeleton == null) throw new Error("skeleton cannot be null.");

			if (loop && this.duration != 0) {
				time %= this.duration;
				if (lastTime > 0) lastTime %= this.duration;
			}

			let timelines = this.timelines;
			for (let i = 0, n = timelines.length; i < n; i++)
				timelines[i].apply(skeleton, lastTime, time, events, alpha, pose, direction);
		}

		static binarySearch (values: ArrayLike<number>, target: number, step: number = 1) {
			let low = 0;
			let high = values.length / step - 2;
			if (high == 0) return step;
			let current = high >>> 1;
			while (true) {
				if (values[(current + 1) * step] <= target)
					low = current + 1;
				else
					high = current;
				if (low == high) return (low + 1) * step;
				current = (low + high) >>> 1;
			}
		}

		static linearSearch (values: ArrayLike<number>, target: number, step: number) {
			for (let i = 0, last = values.length - step; i <= last; i += step)
				if (values[i] > target) return i;
			return -1;
		}
	}

	export interface Timeline {
		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection): void;
		getPropertyId (): number;
	}

	export enum MixPose {
		setup,
		current,
		currentLayered
	}

	export enum MixDirection {
		in, out
	}

	export enum TimelineType {
		rotate, translate, scale, shear,
		attachment, color, deform,
		event, drawOrder,
		ikConstraint, transformConstraint,
		pathConstraintPosition, pathConstraintSpacing, pathConstraintMix,
		twoColor
	}

	export abstract class CurveTimeline implements Timeline {
		static LINEAR = 0; static STEPPED = 1; static BEZIER = 2;
		static BEZIER_SIZE = 10 * 2 - 1;

		private curves: ArrayLike<number>; // type, x, y, ...

		abstract getPropertyId(): number;

		constructor (frameCount: number) {
			if (frameCount <= 0) throw new Error("frameCount must be > 0: " + frameCount);
			this.curves = Utils.newFloatArray((frameCount - 1) * CurveTimeline.BEZIER_SIZE);
		}

		getFrameCount () {
			return this.curves.length / CurveTimeline.BEZIER_SIZE + 1;
		}

		setLinear (frameIndex: number) {
			this.curves[frameIndex * CurveTimeline.BEZIER_SIZE] = CurveTimeline.LINEAR;
		}

		setStepped (frameIndex: number) {
			this.curves[frameIndex * CurveTimeline.BEZIER_SIZE] = CurveTimeline.STEPPED;
		}

		getCurveType (frameIndex: number): number {
			let index = frameIndex * CurveTimeline.BEZIER_SIZE;
			if (index == this.curves.length) return CurveTimeline.LINEAR;
			let type = this.curves[index];
			if (type == CurveTimeline.LINEAR) return CurveTimeline.LINEAR;
			if (type == CurveTimeline.STEPPED) return CurveTimeline.STEPPED;
			return CurveTimeline.BEZIER;
		}

		/** Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
		 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
		 * the difference between the keyframe's values. */
		setCurve (frameIndex: number, cx1: number, cy1: number, cx2: number, cy2: number) {
			let tmpx = (-cx1 * 2 + cx2) * 0.03, tmpy = (-cy1 * 2 + cy2) * 0.03;
			let dddfx = ((cx1 - cx2) * 3 + 1) * 0.006, dddfy = ((cy1 - cy2) * 3 + 1) * 0.006;
			let ddfx = tmpx * 2 + dddfx, ddfy = tmpy * 2 + dddfy;
			let dfx = cx1 * 0.3 + tmpx + dddfx * 0.16666667, dfy = cy1 * 0.3 + tmpy + dddfy * 0.16666667;

			let i = frameIndex * CurveTimeline.BEZIER_SIZE;
			let curves = this.curves;
			curves[i++] = CurveTimeline.BEZIER;

			let x = dfx, y = dfy;
			for (let n = i + CurveTimeline.BEZIER_SIZE - 1; i < n; i += 2) {
				curves[i] = x;
				curves[i + 1] = y;
				dfx += ddfx;
				dfy += ddfy;
				ddfx += dddfx;
				ddfy += dddfy;
				x += dfx;
				y += dfy;
			}
		}

		getCurvePercent (frameIndex: number, percent: number) {
			percent = MathUtils.clamp(percent, 0, 1);
			let curves = this.curves;
			let i = frameIndex * CurveTimeline.BEZIER_SIZE;
			let type = curves[i];
			if (type == CurveTimeline.LINEAR) return percent;
			if (type == CurveTimeline.STEPPED) return 0;
			i++;
			let x = 0;
			for (let start = i, n = i + CurveTimeline.BEZIER_SIZE - 1; i < n; i += 2) {
				x = curves[i];
				if (x >= percent) {
					let prevX: number, prevY: number;
					if (i == start) {
						prevX = 0;
						prevY = 0;
					} else {
						prevX = curves[i - 2];
						prevY = curves[i - 1];
					}
					return prevY + (curves[i + 1] - prevY) * (percent - prevX) / (x - prevX);
				}
			}
			let y = curves[i - 1];
			return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
		}

		abstract apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection): void;
	}

	export class RotateTimeline extends CurveTimeline {
		static ENTRIES = 2;
		static PREV_TIME = -2; static PREV_ROTATION = -1;
		static ROTATION = 1;

		boneIndex: number;
		frames: ArrayLike<number>; // time, degrees, ...

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount << 1);
		}

		getPropertyId () {
			return (TimelineType.rotate << 24) + this.boneIndex;
		}

		/** Sets the time and angle of the specified keyframe. */
		setFrame (frameIndex: number, time: number, degrees: number) {
			frameIndex <<= 1;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + RotateTimeline.ROTATION] = degrees;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					bone.rotation = bone.data.rotation;
					return;
				case MixPose.current:
					let r = bone.data.rotation - bone.rotation;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
					bone.rotation += r * alpha;
				}
				return;
			}

			if (time >= frames[frames.length - RotateTimeline.ENTRIES]) { // Time is after last frame.
				if (pose == MixPose.setup)
					bone.rotation = bone.data.rotation + frames[frames.length + RotateTimeline.PREV_ROTATION] * alpha;
				else {
					let r = bone.data.rotation + frames[frames.length + RotateTimeline.PREV_ROTATION] - bone.rotation;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360; // Wrap within -180 and 180.
					bone.rotation += r * alpha;
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, RotateTimeline.ENTRIES);
			let prevRotation = frames[frame + RotateTimeline.PREV_ROTATION];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent((frame >> 1) - 1,
				1 - (time - frameTime) / (frames[frame + RotateTimeline.PREV_TIME] - frameTime));

			let r = frames[frame + RotateTimeline.ROTATION] - prevRotation;
			r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
			r = prevRotation + r * percent;
			if (pose == MixPose.setup) {
				r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
				bone.rotation = bone.data.rotation + r * alpha;
			} else {
				r = bone.data.rotation + r - bone.rotation;
				r -= (16384 - ((16384.499999999996 - r / 360) |0)) * 360;
				bone.rotation += r * alpha;
			}
		}
	}

	export class TranslateTimeline extends CurveTimeline {
		static ENTRIES = 3;
		static PREV_TIME = -3; static PREV_X = -2; static PREV_Y = -1;
		static X = 1; static Y = 2;

		boneIndex: number;
		frames: ArrayLike<number>; // time, x, y, ...

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount * TranslateTimeline.ENTRIES);
		}

		getPropertyId () {
			return (TimelineType.translate << 24) + this.boneIndex;
		}

		/** Sets the time and value of the specified keyframe. */
		setFrame (frameIndex: number, time: number, x: number, y: number) {
			frameIndex *= TranslateTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TranslateTimeline.X] = x;
			this.frames[frameIndex + TranslateTimeline.Y] = y;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					bone.x = bone.data.x;
					bone.y = bone.data.y;
					return;
				case MixPose.current:
					bone.x += (bone.data.x - bone.x) * alpha;
					bone.y += (bone.data.y - bone.y) * alpha;
				}
				return;
			}

			let x = 0, y = 0;
			if (time >= frames[frames.length - TranslateTimeline.ENTRIES]) { // Time is after last frame.
				x = frames[frames.length + TranslateTimeline.PREV_X];
				y = frames[frames.length + TranslateTimeline.PREV_Y];
			} else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, TranslateTimeline.ENTRIES);
				x = frames[frame + TranslateTimeline.PREV_X];
				y = frames[frame + TranslateTimeline.PREV_Y];
				let frameTime = frames[frame];
				let percent = this.getCurvePercent(frame / TranslateTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + TranslateTimeline.PREV_TIME] - frameTime));

				x += (frames[frame + TranslateTimeline.X] - x) * percent;
				y += (frames[frame + TranslateTimeline.Y] - y) * percent;
			}
			if (pose == MixPose.setup) {
				bone.x = bone.data.x + x * alpha;
				bone.y = bone.data.y + y * alpha;
			} else {
				bone.x += (bone.data.x + x - bone.x) * alpha;
				bone.y += (bone.data.y + y - bone.y) * alpha;
			}
		}
	}

	export class ScaleTimeline extends TranslateTimeline {
		constructor (frameCount: number) {
			super(frameCount);
		}

		getPropertyId () {
			return (TimelineType.scale << 24) + this.boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					bone.scaleX = bone.data.scaleX;
					bone.scaleY = bone.data.scaleY;
					return;
				case MixPose.current:
					bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
				}
				return;
			}

			let x = 0, y = 0;
			if (time >= frames[frames.length - ScaleTimeline.ENTRIES]) { // Time is after last frame.
				x = frames[frames.length + ScaleTimeline.PREV_X] * bone.data.scaleX;
				y = frames[frames.length + ScaleTimeline.PREV_Y] * bone.data.scaleY;
			} else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, ScaleTimeline.ENTRIES);
				x = frames[frame + ScaleTimeline.PREV_X];
				y = frames[frame + ScaleTimeline.PREV_Y];
				let frameTime = frames[frame];
				let percent = this.getCurvePercent(frame / ScaleTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + ScaleTimeline.PREV_TIME] - frameTime));

				x = (x + (frames[frame + ScaleTimeline.X] - x) * percent) * bone.data.scaleX;
				y = (y + (frames[frame + ScaleTimeline.Y] - y) * percent) * bone.data.scaleY;
			}
			if (alpha == 1) {
				bone.scaleX = x;
				bone.scaleY = y;
			} else {
				let bx = 0, by = 0;
				if (pose == MixPose.setup) {
					bx = bone.data.scaleX;
					by = bone.data.scaleY;
				} else {
					bx = bone.scaleX;
					by = bone.scaleY;
				}
				// Mixing out uses sign of setup or current pose, else use sign of key.
				if (direction == MixDirection.out) {
					x = Math.abs(x) * MathUtils.signum(bx);
					y = Math.abs(y) * MathUtils.signum(by);
				} else {
					bx = Math.abs(bx) * MathUtils.signum(x);
					by = Math.abs(by) * MathUtils.signum(y);
				}
				bone.scaleX = bx + (x - bx) * alpha;
				bone.scaleY = by + (y - by) * alpha;
			}
		}
	}

	export class ShearTimeline extends TranslateTimeline {
		constructor (frameCount: number) {
			super(frameCount);
		}

		getPropertyId () {
			return (TimelineType.shear << 24) + this.boneIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let frames = this.frames;

			let bone = skeleton.bones[this.boneIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					bone.shearX = bone.data.shearX;
					bone.shearY = bone.data.shearY;
					return;
				case MixPose.current:
					bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
					bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
				}
				return;
			}

			let x = 0, y = 0;
			if (time >= frames[frames.length - ShearTimeline.ENTRIES]) { // Time is after last frame.
				x = frames[frames.length + ShearTimeline.PREV_X];
				y = frames[frames.length + ShearTimeline.PREV_Y];
			} else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, ShearTimeline.ENTRIES);
				x = frames[frame + ShearTimeline.PREV_X];
				y = frames[frame + ShearTimeline.PREV_Y];
				let frameTime = frames[frame];
				let percent = this.getCurvePercent(frame / ShearTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + ShearTimeline.PREV_TIME] - frameTime));

				x = x + (frames[frame + ShearTimeline.X] - x) * percent;
				y = y + (frames[frame + ShearTimeline.Y] - y) * percent;
			}
			if (pose == MixPose.setup) {
				bone.shearX = bone.data.shearX + x * alpha;
				bone.shearY = bone.data.shearY + y * alpha;
			} else {
				bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
				bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
			}
		}
	}

	export class ColorTimeline extends CurveTimeline {
		static ENTRIES = 5;
		static PREV_TIME = -5; static PREV_R = -4; static PREV_G = -3; static PREV_B = -2; static PREV_A = -1;
		static R = 1; static G = 2; static B = 3; static A = 4;

		slotIndex: number;
		frames: ArrayLike<number>; // time, r, g, b, a, ...

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount * ColorTimeline.ENTRIES);
		}

		getPropertyId () {
			return (TimelineType.color << 24) + this.slotIndex;
		}

		/** Sets the time and value of the specified keyframe. */
		setFrame (frameIndex: number, time: number, r: number, g: number, b: number, a: number) {
			frameIndex *= ColorTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + ColorTimeline.R] = r;
			this.frames[frameIndex + ColorTimeline.G] = g;
			this.frames[frameIndex + ColorTimeline.B] = b;
			this.frames[frameIndex + ColorTimeline.A] = a;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let slot = skeleton.slots[this.slotIndex];
			let frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					slot.color.setFromColor(slot.data.color);
					return;
				case MixPose.current:
					let color = slot.color, setup = slot.data.color;
					color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha);
				}
				return;
			}

			let r = 0, g = 0, b = 0, a = 0;
			if (time >= frames[frames.length - ColorTimeline.ENTRIES]) { // Time is after last frame.
				let i = frames.length;
				r = frames[i + ColorTimeline.PREV_R];
				g = frames[i + ColorTimeline.PREV_G];
				b = frames[i + ColorTimeline.PREV_B];
				a = frames[i + ColorTimeline.PREV_A];
			} else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, ColorTimeline.ENTRIES);
				r = frames[frame + ColorTimeline.PREV_R];
				g = frames[frame + ColorTimeline.PREV_G];
				b = frames[frame + ColorTimeline.PREV_B];
				a = frames[frame + ColorTimeline.PREV_A];
				let frameTime = frames[frame];
				let percent = this.getCurvePercent(frame / ColorTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + ColorTimeline.PREV_TIME] - frameTime));

				r += (frames[frame + ColorTimeline.R] - r) * percent;
				g += (frames[frame + ColorTimeline.G] - g) * percent;
				b += (frames[frame + ColorTimeline.B] - b) * percent;
				a += (frames[frame + ColorTimeline.A] - a) * percent;
			}
			if (alpha == 1)
				slot.color.set(r, g, b, a);
			else {
				let color = slot.color;
				if (pose == MixPose.setup) color.setFromColor(slot.data.color);
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
		}
	}

	export class TwoColorTimeline extends CurveTimeline {
		static ENTRIES = 8;
		static PREV_TIME = -8; static PREV_R = -7; static PREV_G = -6; static PREV_B = -5; static PREV_A = -4;
		static PREV_R2 = -3; static PREV_G2 = -2; static PREV_B2 = -1;
		static R = 1; static G = 2; static B = 3; static A = 4; static R2 = 5; static G2 = 6; static B2 = 7;

		slotIndex: number;
		frames: ArrayLike<number>; // time, r, g, b, a, r2, g2, b2, ...

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount * TwoColorTimeline.ENTRIES);
		}

		getPropertyId () {
			return (TimelineType.twoColor << 24) + this.slotIndex;
		}

		/** Sets the time and value of the specified keyframe. */
		setFrame (frameIndex: number, time: number, r: number, g: number, b: number, a: number, r2: number, g2: number, b2: number) {
			frameIndex *= TwoColorTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TwoColorTimeline.R] = r;
			this.frames[frameIndex + TwoColorTimeline.G] = g;
			this.frames[frameIndex + TwoColorTimeline.B] = b;
			this.frames[frameIndex + TwoColorTimeline.A] = a;
			this.frames[frameIndex + TwoColorTimeline.R2] = r2;
			this.frames[frameIndex + TwoColorTimeline.G2] = g2;
			this.frames[frameIndex + TwoColorTimeline.B2] = b2;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let slot = skeleton.slots[this.slotIndex];
			let frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					slot.color.setFromColor(slot.data.color);
					slot.darkColor.setFromColor(slot.data.darkColor);
					return;
				case MixPose.current:
					let light = slot.color, dark = slot.darkColor, setupLight = slot.data.color, setupDark = slot.data.darkColor;
					light.add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
						(setupLight.a - light.a) * alpha);
					dark.add((setupDark.r - dark.r) * alpha, (setupDark.g - dark.g) * alpha, (setupDark.b - dark.b) * alpha, 0);
				}
				return;
			}

			let r = 0, g = 0, b = 0, a = 0, r2 = 0, g2 = 0, b2 = 0;
			if (time >= frames[frames.length - TwoColorTimeline.ENTRIES]) { // Time is after last frame.
				let i = frames.length;
				r = frames[i + TwoColorTimeline.PREV_R];
				g = frames[i + TwoColorTimeline.PREV_G];
				b = frames[i + TwoColorTimeline.PREV_B];
				a = frames[i + TwoColorTimeline.PREV_A];
				r2 = frames[i + TwoColorTimeline.PREV_R2];
				g2 = frames[i + TwoColorTimeline.PREV_G2];
				b2 = frames[i + TwoColorTimeline.PREV_B2];
			} else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, TwoColorTimeline.ENTRIES);
				r = frames[frame + TwoColorTimeline.PREV_R];
				g = frames[frame + TwoColorTimeline.PREV_G];
				b = frames[frame + TwoColorTimeline.PREV_B];
				a = frames[frame + TwoColorTimeline.PREV_A];
				r2 = frames[frame + TwoColorTimeline.PREV_R2];
				g2 = frames[frame + TwoColorTimeline.PREV_G2];
				b2 = frames[frame + TwoColorTimeline.PREV_B2];
				let frameTime = frames[frame];
				let percent = this.getCurvePercent(frame / TwoColorTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + TwoColorTimeline.PREV_TIME] - frameTime));

				r += (frames[frame + TwoColorTimeline.R] - r) * percent;
				g += (frames[frame + TwoColorTimeline.G] - g) * percent;
				b += (frames[frame + TwoColorTimeline.B] - b) * percent;
				a += (frames[frame + TwoColorTimeline.A] - a) * percent;
				r2 += (frames[frame + TwoColorTimeline.R2] - r2) * percent;
				g2 += (frames[frame + TwoColorTimeline.G2] - g2) * percent;
				b2 += (frames[frame + TwoColorTimeline.B2] - b2) * percent;
			}
			if (alpha == 1) {
				slot.color.set(r, g, b, a);
				slot.darkColor.set(r2, g2, b2, 1);
			} else {
				let light = slot.color, dark = slot.darkColor;
				if (pose == MixPose.setup) {
					light.setFromColor(slot.data.color);
					dark.setFromColor(slot.data.darkColor);
				}
				light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
				dark.add((r2 - dark.r) * alpha, (g2 - dark.g) * alpha, (b2 - dark.b) * alpha, 0);
			}
		}
	}

	export class AttachmentTimeline implements Timeline {
		slotIndex: number;
		frames: ArrayLike<number> // time, ...
		attachmentNames: Array<string>;

		constructor (frameCount: number) {
			this.frames = Utils.newFloatArray(frameCount);
			this.attachmentNames = new Array<string>(frameCount);
		}

		getPropertyId () {
			return (TimelineType.attachment << 24) + this.slotIndex;
		}

		getFrameCount () {
			return this.frames.length;
		}

		/** Sets the time and value of the specified keyframe. */
		setFrame (frameIndex: number, time: number, attachmentName: string) {
			this.frames[frameIndex] = time;
			this.attachmentNames[frameIndex] = attachmentName;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let slot = skeleton.slots[this.slotIndex];
			if (direction == MixDirection.out && pose == MixPose.setup) {
				let attachmentName = slot.data.attachmentName;
				slot.setAttachment(attachmentName == null ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
				return;
			}

			let frames = this.frames;
			if (time < frames[0]) {
				if (pose == MixPose.setup) {
					let attachmentName = slot.data.attachmentName;
					slot.setAttachment(attachmentName == null ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
				}
				return;
			}

			let frameIndex = 0;
			if (time >= frames[frames.length - 1]) // Time is after last frame.
				frameIndex = frames.length - 1;
			else
				frameIndex = Animation.binarySearch(frames, time, 1) - 1;

			let attachmentName = this.attachmentNames[frameIndex];
			skeleton.slots[this.slotIndex]
				.setAttachment(attachmentName == null ? null : skeleton.getAttachment(this.slotIndex, attachmentName));
		}
	}

	var zeros : ArrayLike<number> = null;

	export class DeformTimeline extends CurveTimeline {
		slotIndex: number;
		attachment: VertexAttachment;
		frames: ArrayLike<number>; // time, ...
		frameVertices: Array<ArrayLike<number>>;

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount);
			this.frameVertices = new Array<ArrayLike<number>>(frameCount);
			if (zeros == null) zeros = Utils.newFloatArray(64);
		}

		getPropertyId () {
			return (TimelineType.deform << 27) + + this.attachment.id + this.slotIndex;
		}

		/** Sets the time of the specified keyframe. */
		setFrame (frameIndex: number, time: number, vertices: ArrayLike<number>) {
			this.frames[frameIndex] = time;
			this.frameVertices[frameIndex] = vertices;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let slot: Slot = skeleton.slots[this.slotIndex];
			let slotAttachment: Attachment = slot.getAttachment();
			if (!(slotAttachment instanceof VertexAttachment) || !(<VertexAttachment>slotAttachment).applyDeform(this.attachment)) return;

			let verticesArray: Array<number> = slot.attachmentVertices;
			let frameVertices = this.frameVertices;
			let vertexCount = frameVertices[0].length;
			let vertices: Array<number> = Utils.setArraySize(verticesArray, vertexCount);

			let frames = this.frames;
			if (time < frames[0]) {
				let vertexAttachment = <VertexAttachment>slotAttachment;
				switch (pose) {
				case MixPose.setup:
					var zeroVertices: ArrayLike<number>;
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions (setup pose).
						zeroVertices = vertexAttachment.vertices;
					} else {
						// Weighted deform offsets (zeros).
						zeroVertices = zeros;
						if (zeroVertices.length < vertexCount) zeros = zeroVertices = Utils.newFloatArray(vertexCount);
					}
					Utils.arrayCopy(zeroVertices, 0, vertices, 0, vertexCount);
					return;
				case MixPose.current:
					if (alpha == 1) break;
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions.
						var setupVertices = vertexAttachment.vertices;
						for (var i = 0; i < vertexCount; i++)
							vertices[i] += (setupVertices[i] - vertices[i]) * alpha;
					} else {
						// Weighted deform offsets.
						alpha = 1 - alpha;
						for (var i = 0; i < vertexCount; i++)
							vertices[i] *= alpha;
					}
				}
				return;
			}

			if (time >= frames[frames.length - 1]) { // Time is after last frame.
				let lastVertices = frameVertices[frames.length - 1];
				if (alpha == 1) {
					Utils.arrayCopy(lastVertices, 0, vertices, 0, vertexCount);
				} else if (pose == MixPose.setup) {
					let vertexAttachment = slotAttachment as VertexAttachment;
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions, with alpha.
						let setupVertices = vertexAttachment.vertices;
						for (let i = 0; i < vertexCount; i++) {
							let setup = setupVertices[i];
							vertices[i] = setup + (lastVertices[i] - setup) * alpha;
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (let i = 0; i < vertexCount; i++)
							vertices[i] = lastVertices[i] * alpha;
					}
				} else {
					for (let i = 0; i < vertexCount; i++)
						vertices[i] += (lastVertices[i] - vertices[i]) * alpha;
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time);
			let prevVertices = frameVertices[frame - 1];
			let nextVertices = frameVertices[frame];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime));

			if (alpha == 1) {
				for (let i = 0; i < vertexCount; i++) {
					let prev = prevVertices[i];
					vertices[i] = prev + (nextVertices[i] - prev) * percent;
				}
			} else if (pose == MixPose.setup) {
				let vertexAttachment = slotAttachment as VertexAttachment;
				if (vertexAttachment.bones == null) {
					// Unweighted vertex positions, with alpha.
					let setupVertices = vertexAttachment.vertices;
					for (let i = 0; i < vertexCount; i++) {
						let prev = prevVertices[i], setup = setupVertices[i];
						vertices[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
					}
				} else {
					// Weighted deform offsets, with alpha.
					for (let i = 0; i < vertexCount; i++) {
						let prev = prevVertices[i];
						vertices[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
					}
				}
			} else {
				// Vertex positions or deform offsets, with alpha.
				for (let i = 0; i < vertexCount; i++) {
					let prev = prevVertices[i];
					vertices[i] += (prev + (nextVertices[i] - prev) * percent - vertices[i]) * alpha;
				}
			}
		}
	}

	export class EventTimeline implements Timeline {
		frames: ArrayLike<number>; // time, ...
		events: Array<Event>;

		constructor (frameCount: number) {
			this.frames = Utils.newFloatArray(frameCount);
			this.events = new Array<Event>(frameCount);
		}

		getPropertyId () {
			return TimelineType.event << 24;
		}

		getFrameCount () {
			return this.frames.length;
		}

		/** Sets the time of the specified keyframe. */
		setFrame (frameIndex: number, event: Event) {
			this.frames[frameIndex] = event.time;
			this.events[frameIndex] = event;
		}

		/** Fires events for frames > lastTime and <= time. */
		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			if (firedEvents == null) return;
			let frames = this.frames;
			let frameCount = this.frames.length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				this.apply(skeleton, lastTime, Number.MAX_VALUE, firedEvents, alpha, pose, direction);
				lastTime = -1;
			} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return; // Time is before first frame.

			let frame = 0;
			if (lastTime < frames[0])
				frame = 0;
			else {
				frame = Animation.binarySearch(frames, lastTime);
				let frameTime = frames[frame];
				while (frame > 0) { // Fire multiple events with the same frame.
					if (frames[frame - 1] != frameTime) break;
					frame--;
				}
			}
			for (; frame < frameCount && time >= frames[frame]; frame++)
				firedEvents.push(this.events[frame]);
		}
	}

	export class DrawOrderTimeline implements Timeline {
		frames: ArrayLike<number>; // time, ...
		drawOrders: Array<Array<number>>;

		constructor (frameCount: number) {
			this.frames = Utils.newFloatArray(frameCount);
			this.drawOrders = new Array<Array<number>>(frameCount);
		}

		getPropertyId () {
			return TimelineType.drawOrder << 24;
		}

		getFrameCount () {
			return this.frames.length;
		}

		/** Sets the time of the specified keyframe.
		 * @param drawOrder May be null to use bind pose draw order. */
		setFrame (frameIndex: number, time: number, drawOrder: Array<number>) {
			this.frames[frameIndex] = time;
			this.drawOrders[frameIndex] = drawOrder;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let drawOrder: Array<Slot> = skeleton.drawOrder;
			let slots: Array<Slot> = skeleton.slots;
			if (direction == MixDirection.out && pose == MixPose.setup) {
				Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
				return;
			}

			let frames = this.frames;
			if (time < frames[0]) {
				if (pose == MixPose.setup) Utils.arrayCopy(skeleton.slots, 0, skeleton.drawOrder, 0, skeleton.slots.length);
				return;
			}

			let frame = 0;
			if (time >= frames[frames.length - 1]) // Time is after last frame.
				frame = frames.length - 1;
			else
				frame = Animation.binarySearch(frames, time) - 1;

			let drawOrderToSetupIndex = this.drawOrders[frame];
			if (drawOrderToSetupIndex == null)
				Utils.arrayCopy(slots, 0, drawOrder, 0, slots.length);
			else {
				for (let i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
					drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		}
	}

	export class IkConstraintTimeline extends CurveTimeline {
		static ENTRIES = 3;
		static PREV_TIME = -3; static PREV_MIX = -2; static PREV_BEND_DIRECTION = -1;
		static MIX = 1; static BEND_DIRECTION = 2;

		ikConstraintIndex: number;
		frames: ArrayLike<number>; // time, mix, bendDirection, ...

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount * IkConstraintTimeline.ENTRIES);
		}

		getPropertyId () {
			return (TimelineType.ikConstraint << 24) + this.ikConstraintIndex;
		}

		/** Sets the time, mix and bend direction of the specified keyframe. */
		setFrame (frameIndex: number, time: number, mix: number, bendDirection: number) {
			frameIndex *= IkConstraintTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + IkConstraintTimeline.MIX] = mix;
			this.frames[frameIndex + IkConstraintTimeline.BEND_DIRECTION] = bendDirection;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let frames = this.frames;
			let constraint: IkConstraint = skeleton.ikConstraints[this.ikConstraintIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					constraint.mix = constraint.data.mix;
					constraint.bendDirection = constraint.data.bendDirection;
					return;
				case MixPose.current:
					constraint.mix += (constraint.data.mix - constraint.mix) * alpha;
					constraint.bendDirection = constraint.data.bendDirection;
				}
				return;
			}

			if (time >= frames[frames.length - IkConstraintTimeline.ENTRIES]) { // Time is after last frame.
				if (pose == MixPose.setup) {
					constraint.mix = constraint.data.mix + (frames[frames.length + IkConstraintTimeline.PREV_MIX] - constraint.data.mix) * alpha;
					constraint.bendDirection = direction == MixDirection.out ? constraint.data.bendDirection
						: frames[frames.length + IkConstraintTimeline.PREV_BEND_DIRECTION];
				} else {
					constraint.mix += (frames[frames.length + IkConstraintTimeline.PREV_MIX] - constraint.mix) * alpha;
					if (direction == MixDirection.in) constraint.bendDirection = frames[frames.length + IkConstraintTimeline.PREV_BEND_DIRECTION];
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, IkConstraintTimeline.ENTRIES);
			let mix = frames[frame + IkConstraintTimeline.PREV_MIX];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame / IkConstraintTimeline.ENTRIES - 1,
				1 - (time - frameTime) / (frames[frame + IkConstraintTimeline.PREV_TIME] - frameTime));

			if (pose == MixPose.setup) {
				constraint.mix = constraint.data.mix + (mix + (frames[frame + IkConstraintTimeline.MIX] - mix) * percent - constraint.data.mix) * alpha;
				constraint.bendDirection = direction == MixDirection.out ? constraint.data.bendDirection : frames[frame + IkConstraintTimeline.PREV_BEND_DIRECTION];
			} else {
				constraint.mix += (mix + (frames[frame + IkConstraintTimeline.MIX] - mix) * percent - constraint.mix) * alpha;
				if (direction == MixDirection.in) constraint.bendDirection = frames[frame + IkConstraintTimeline.PREV_BEND_DIRECTION];
			}
		}
	}

	export class TransformConstraintTimeline extends CurveTimeline {
		static ENTRIES = 5;
		static PREV_TIME = -5; static PREV_ROTATE = -4; static PREV_TRANSLATE = -3; static PREV_SCALE = -2; static PREV_SHEAR = -1;
		static ROTATE = 1; static TRANSLATE = 2; static SCALE = 3; static SHEAR = 4;

		transformConstraintIndex: number;
		frames: ArrayLike<number>; // time, rotate mix, translate mix, scale mix, shear mix, ...

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount * TransformConstraintTimeline.ENTRIES);
		}

		getPropertyId () {
			return (TimelineType.transformConstraint << 24) + this.transformConstraintIndex;
		}

		/** Sets the time and mixes of the specified keyframe. */
		setFrame (frameIndex: number, time: number, rotateMix: number, translateMix: number, scaleMix: number, shearMix: number) {
			frameIndex *= TransformConstraintTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TransformConstraintTimeline.ROTATE] = rotateMix;
			this.frames[frameIndex + TransformConstraintTimeline.TRANSLATE] = translateMix;
			this.frames[frameIndex + TransformConstraintTimeline.SCALE] = scaleMix;
			this.frames[frameIndex + TransformConstraintTimeline.SHEAR] = shearMix;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let frames = this.frames;

			let constraint: TransformConstraint = skeleton.transformConstraints[this.transformConstraintIndex];
			if (time < frames[0]) {
				let data = constraint.data;
				switch (pose) {
				case MixPose.setup:
					constraint.rotateMix = data.rotateMix;
					constraint.translateMix = data.translateMix;
					constraint.scaleMix = data.scaleMix;
					constraint.shearMix = data.shearMix;
					return;
				case MixPose.current:
					constraint.rotateMix += (data.rotateMix - constraint.rotateMix) * alpha;
					constraint.translateMix += (data.translateMix - constraint.translateMix) * alpha;
					constraint.scaleMix += (data.scaleMix - constraint.scaleMix) * alpha;
					constraint.shearMix += (data.shearMix - constraint.shearMix) * alpha;
				}
				return;
			}

			let rotate = 0, translate = 0, scale = 0, shear = 0;
			if (time >= frames[frames.length - TransformConstraintTimeline.ENTRIES]) { // Time is after last frame.
				let i = frames.length;
				rotate = frames[i + TransformConstraintTimeline.PREV_ROTATE];
				translate = frames[i + TransformConstraintTimeline.PREV_TRANSLATE];
				scale = frames[i + TransformConstraintTimeline.PREV_SCALE];
				shear = frames[i + TransformConstraintTimeline.PREV_SHEAR];
			} else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, TransformConstraintTimeline.ENTRIES);
				rotate = frames[frame + TransformConstraintTimeline.PREV_ROTATE];
				translate = frames[frame + TransformConstraintTimeline.PREV_TRANSLATE];
				scale = frames[frame + TransformConstraintTimeline.PREV_SCALE];
				shear = frames[frame + TransformConstraintTimeline.PREV_SHEAR];
				let frameTime = frames[frame];
				let percent = this.getCurvePercent(frame / TransformConstraintTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + TransformConstraintTimeline.PREV_TIME] - frameTime));

				rotate += (frames[frame + TransformConstraintTimeline.ROTATE] - rotate) * percent;
				translate += (frames[frame + TransformConstraintTimeline.TRANSLATE] - translate) * percent;
				scale += (frames[frame + TransformConstraintTimeline.SCALE] - scale) * percent;
				shear += (frames[frame + TransformConstraintTimeline.SHEAR] - shear) * percent;
			}
			if (pose == MixPose.setup) {
				let data = constraint.data;
				constraint.rotateMix = data.rotateMix + (rotate - data.rotateMix) * alpha;
				constraint.translateMix = data.translateMix + (translate - data.translateMix) * alpha;
				constraint.scaleMix = data.scaleMix + (scale - data.scaleMix) * alpha;
				constraint.shearMix = data.shearMix + (shear - data.shearMix) * alpha;
			} else {
				constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
				constraint.translateMix += (translate - constraint.translateMix) * alpha;
				constraint.scaleMix += (scale - constraint.scaleMix) * alpha;
				constraint.shearMix += (shear - constraint.shearMix) * alpha;
			}
		}
	}

	export class PathConstraintPositionTimeline extends CurveTimeline {
		static ENTRIES = 2;
		static PREV_TIME = -2; static PREV_VALUE = -1;
		static VALUE = 1;

		pathConstraintIndex: number;

		frames: ArrayLike<number>; // time, position, ...

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount * PathConstraintPositionTimeline.ENTRIES);
		}

		getPropertyId () {
			return (TimelineType.pathConstraintPosition << 24) + this.pathConstraintIndex;
		}

		/** Sets the time and value of the specified keyframe. */
		setFrame (frameIndex: number, time: number, value: number) {
			frameIndex *= PathConstraintPositionTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + PathConstraintPositionTimeline.VALUE] = value;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let frames = this.frames;
			let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					constraint.position = constraint.data.position;
					return;
				case MixPose.current:
					constraint.position += (constraint.data.position - constraint.position) * alpha;
				}
				return;
			}

			let position = 0;
			if (time >= frames[frames.length - PathConstraintPositionTimeline.ENTRIES]) // Time is after last frame.
				position = frames[frames.length + PathConstraintPositionTimeline.PREV_VALUE];
			else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, PathConstraintPositionTimeline.ENTRIES);
				position = frames[frame + PathConstraintPositionTimeline.PREV_VALUE];
				let frameTime = frames[frame];
				let percent = this.getCurvePercent(frame / PathConstraintPositionTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PathConstraintPositionTimeline.PREV_TIME] - frameTime));

				position += (frames[frame + PathConstraintPositionTimeline.VALUE] - position) * percent;
			}
			if (pose == MixPose.setup)
				constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
			else
				constraint.position += (position - constraint.position) * alpha;
		}
	}

	export class PathConstraintSpacingTimeline extends PathConstraintPositionTimeline {
		constructor (frameCount: number) {
			super(frameCount);
		}

		getPropertyId () {
			return (TimelineType.pathConstraintSpacing << 24) + this.pathConstraintIndex;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let frames = this.frames;
			let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					constraint.spacing = constraint.data.spacing;
					return;
				case MixPose.current:
					constraint.spacing += (constraint.data.spacing - constraint.spacing) * alpha;
				}
				return;
			}

			let spacing = 0;
			if (time >= frames[frames.length - PathConstraintSpacingTimeline.ENTRIES]) // Time is after last frame.
				spacing = frames[frames.length + PathConstraintSpacingTimeline.PREV_VALUE];
			else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, PathConstraintSpacingTimeline.ENTRIES);
				spacing = frames[frame + PathConstraintSpacingTimeline.PREV_VALUE];
				let frameTime = frames[frame];
				let percent = this.getCurvePercent(frame / PathConstraintSpacingTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PathConstraintSpacingTimeline.PREV_TIME] - frameTime));

				spacing += (frames[frame + PathConstraintSpacingTimeline.VALUE] - spacing) * percent;
			}

			if (pose == MixPose.setup)
				constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
			else
				constraint.spacing += (spacing - constraint.spacing) * alpha;
		}
	}

	export class PathConstraintMixTimeline extends CurveTimeline {
		static ENTRIES = 3;
		static PREV_TIME = -3; static PREV_ROTATE = -2; static PREV_TRANSLATE = -1;
		static ROTATE = 1; static TRANSLATE = 2;

		pathConstraintIndex: number;

		frames: ArrayLike<number>; // time, rotate mix, translate mix, ...

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount * PathConstraintMixTimeline.ENTRIES);
		}

		getPropertyId () {
			return (TimelineType.pathConstraintMix << 24) + this.pathConstraintIndex;
		}

		/** Sets the time and mixes of the specified keyframe. */
		setFrame (frameIndex: number, time: number, rotateMix: number, translateMix: number) {
			frameIndex *= PathConstraintMixTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + PathConstraintMixTimeline.ROTATE] = rotateMix;
			this.frames[frameIndex + PathConstraintMixTimeline.TRANSLATE] = translateMix;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number, pose: MixPose, direction: MixDirection) {
			let frames = this.frames;
			let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];

			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					constraint.rotateMix = constraint.data.rotateMix;
					constraint.translateMix = constraint.data.translateMix;
					return;
				case MixPose.current:
					constraint.rotateMix += (constraint.data.rotateMix - constraint.rotateMix) * alpha;
					constraint.translateMix += (constraint.data.translateMix - constraint.translateMix) * alpha;
				}
				return;
			}

			let rotate = 0, translate = 0;
			if (time >= frames[frames.length - PathConstraintMixTimeline.ENTRIES]) { // Time is after last frame.
				rotate = frames[frames.length + PathConstraintMixTimeline.PREV_ROTATE];
				translate = frames[frames.length + PathConstraintMixTimeline.PREV_TRANSLATE];
			} else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, PathConstraintMixTimeline.ENTRIES);
				rotate = frames[frame + PathConstraintMixTimeline.PREV_ROTATE];
				translate = frames[frame + PathConstraintMixTimeline.PREV_TRANSLATE];
				let frameTime = frames[frame];
				let percent = this.getCurvePercent(frame / PathConstraintMixTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PathConstraintMixTimeline.PREV_TIME] - frameTime));

				rotate += (frames[frame + PathConstraintMixTimeline.ROTATE] - rotate) * percent;
				translate += (frames[frame + PathConstraintMixTimeline.TRANSLATE] - translate) * percent;
			}

			if (pose == MixPose.setup) {
				constraint.rotateMix = constraint.data.rotateMix + (rotate - constraint.data.rotateMix) * alpha;
				constraint.translateMix = constraint.data.translateMix + (translate - constraint.data.translateMix) * alpha;
			} else {
				constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
				constraint.translateMix += (translate - constraint.translateMix) * alpha;
			}
		}
	}
}
