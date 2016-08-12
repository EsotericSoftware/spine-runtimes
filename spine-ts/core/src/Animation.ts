/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.5
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

		apply (skeleton: Skeleton, lastTime: number, time: number, loop: boolean, events: Array<Event>) {
			if (skeleton == null) throw new Error("skeleton cannot be null.");

			if (loop && this.duration != 0) {
				time %= this.duration;
				if (lastTime > 0) lastTime %= this.duration;
			}

			let timelines = this.timelines;
			for (let i = 0, n = timelines.length; i < n; i++)
				timelines[i].apply(skeleton, lastTime, time, events, 1);
		}

		mix (skeleton: Skeleton, lastTime: number, time: number, loop: boolean, events: Array<Event>, alpha: number) {
			if (skeleton == null) throw new Error("skeleton cannot be null.");

			if (loop && this.duration != 0) {
				time %= this.duration;
				if (lastTime > 0) lastTime %= this.duration;
			}

			let timelines = this.timelines;
			for (let i = 0, n = timelines.length; i < n; i++)
				timelines[i].apply(skeleton, lastTime, time, events, alpha);
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
		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
	}

	export abstract class CurveTimeline implements Timeline {
		static LINEAR = 0; static STEPPED = 1; static BEZIER = 2;
		static BEZIER_SIZE = 10 * 2 - 1;

		private curves: ArrayLike<number>; // type, x, y, ...

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

		abstract apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number): void;
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

		/** Sets the time and angle of the specified keyframe. */
		setFrame (frameIndex: number, time: number, degrees: number) {
			frameIndex <<= 1;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + RotateTimeline.ROTATION] = degrees;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let bone = skeleton.bones[this.boneIndex];

			if (time >= frames[frames.length - RotateTimeline.ENTRIES]) { // Time is after last frame.
				let amount = bone.data.rotation + frames[frames.length + RotateTimeline.PREV_ROTATION] - bone.rotation;
				while (amount > 180)
					amount -= 360;
				while (amount < -180)
					amount += 360;
				bone.rotation += amount * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, RotateTimeline.ENTRIES);
			let prevRotation = frames[frame + RotateTimeline.PREV_ROTATION];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent((frame >> 1) - 1,
				1 - (time - frameTime) / (frames[frame + RotateTimeline.PREV_TIME] - frameTime));

			let amount = frames[frame + RotateTimeline.ROTATION] - prevRotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			amount = bone.data.rotation + (prevRotation + amount * percent) - bone.rotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			bone.rotation += amount * alpha;
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

		/** Sets the time and value of the specified keyframe. */
		setFrame (frameIndex: number, time: number, x: number, y: number) {
			frameIndex *= TranslateTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TranslateTimeline.X] = x;
			this.frames[frameIndex + TranslateTimeline.Y] = y;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let bone = skeleton.bones[this.boneIndex];

			if (time >= frames[frames.length - TranslateTimeline.ENTRIES]) { // Time is after last frame.
				bone.x += (bone.data.x + frames[frames.length + TranslateTimeline.PREV_X] - bone.x) * alpha;
				bone.y += (bone.data.y + frames[frames.length + TranslateTimeline.PREV_Y] - bone.y) * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, TranslateTimeline.ENTRIES);
			let prevX = frames[frame + TranslateTimeline.PREV_X];
			let prevY = frames[frame + TranslateTimeline.PREV_Y];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame / TranslateTimeline.ENTRIES - 1,
				1 - (time - frameTime) / (frames[frame + TranslateTimeline.PREV_TIME] - frameTime));

			bone.x += (bone.data.x + prevX + (frames[frame + TranslateTimeline.X] - prevX) * percent - bone.x) * alpha;
			bone.y += (bone.data.y + prevY + (frames[frame + TranslateTimeline.Y] - prevY) * percent - bone.y) * alpha;
		}
	}

	export class ScaleTimeline extends TranslateTimeline {
		constructor (frameCount: number) {
			super(frameCount);
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let bone = skeleton.bones[this.boneIndex];
			if (time >= frames[frames.length - ScaleTimeline.ENTRIES]) { // Time is after last frame.
				bone.scaleX += (bone.data.scaleX * frames[frames.length + ScaleTimeline.PREV_X] - bone.scaleX) * alpha;
				bone.scaleY += (bone.data.scaleY * frames[frames.length + ScaleTimeline.PREV_Y] - bone.scaleY) * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, ScaleTimeline.ENTRIES);
			let prevX = frames[frame + ScaleTimeline.PREV_X];
			let prevY = frames[frame + ScaleTimeline.PREV_Y];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame / ScaleTimeline.ENTRIES - 1,
				1 - (time - frameTime) / (frames[frame + ScaleTimeline.PREV_TIME] - frameTime));

			bone.scaleX += (bone.data.scaleX * (prevX + (frames[frame + ScaleTimeline.X] - prevX) * percent) - bone.scaleX) * alpha;
			bone.scaleY += (bone.data.scaleY * (prevY + (frames[frame + ScaleTimeline.Y] - prevY) * percent) - bone.scaleY) * alpha;
		}
	}

	export class ShearTimeline extends TranslateTimeline {
		constructor (frameCount: number) {
			super(frameCount);
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let bone = skeleton.bones[this.boneIndex];
			if (time >= frames[frames.length - ShearTimeline.ENTRIES]) { // Time is after last frame.
				bone.shearX += (bone.data.shearX + frames[frames.length + ShearTimeline.PREV_X] - bone.shearX) * alpha;
				bone.shearY += (bone.data.shearY + frames[frames.length + ShearTimeline.PREV_Y] - bone.shearY) * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, ShearTimeline.ENTRIES);
			let prevX = frames[frame + ShearTimeline.PREV_X];
			let prevY = frames[frame + ShearTimeline.PREV_Y];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame / ShearTimeline.ENTRIES - 1,
				1 - (time - frameTime) / (frames[frame + ShearTimeline.PREV_TIME] - frameTime));

			bone.shearX += (bone.data.shearX + (prevX + (frames[frame + ShearTimeline.X] - prevX) * percent) - bone.shearX) * alpha;
			bone.shearY += (bone.data.shearY + (prevY + (frames[frame + ShearTimeline.Y] - prevY) * percent) - bone.shearY) * alpha;
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

		/** Sets the time and value of the specified keyframe. */
		setFrame (frameIndex: number, time: number, r: number, g: number, b: number, a: number) {
			frameIndex *= ColorTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + ColorTimeline.R] = r;
			this.frames[frameIndex + ColorTimeline.G] = g;
			this.frames[frameIndex + ColorTimeline.B] = b;
			this.frames[frameIndex + ColorTimeline.A] = a;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

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
			let color: Color = skeleton.slots[this.slotIndex].color;
			if (alpha < 1)
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			else
				color.set(r, g, b, a);
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

		getFrameCount () {
			return this.frames.length;
		}

		/** Sets the time and value of the specified keyframe. */
		setFrame (frameIndex: number, time: number, attachmentName: string) {
			this.frames[frameIndex] = time;
			this.attachmentNames[frameIndex] = attachmentName;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, events: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

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

	export class EventTimeline implements Timeline {
		frames: ArrayLike<number>; // time, ...
		events: Array<Event>;

		constructor (frameCount: number) {
			this.frames = Utils.newFloatArray(frameCount);
			this.events = new Array<Event>(frameCount);
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
		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number) {
			if (firedEvents == null) return;
			let frames = this.frames;
			let frameCount = this.frames.length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				this.apply(skeleton, lastTime, Number.MAX_VALUE, firedEvents, alpha);
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

		getFrameCount () {
			return this.frames.length;
		}

		/** Sets the time of the specified keyframe.
		 * @param drawOrder May be null to use bind pose draw order. */
		setFrame (frameIndex: number, time: number, drawOrder: Array<number>) {
			this.frames[frameIndex] = time;
			this.drawOrders[frameIndex] = drawOrder;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let frame = 0;
			if (time >= frames[frames.length - 1]) // Time is after last frame.
				frame = frames.length - 1;
			else
				frame = Animation.binarySearch(frames, time) - 1;

			let drawOrder: Array<Slot> = skeleton.drawOrder;
			let slots: Array<Slot> = skeleton.slots;
			let drawOrderToSetupIndex = this.drawOrders[frame];
			if (drawOrderToSetupIndex == null)
				Utils.arrayCopy(slots, 0, drawOrder, 0, slots.length);
			else {
				for (let i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
					drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		}
	}

	export class DeformTimeline extends CurveTimeline {
		frames: ArrayLike<number>; // time, ...
		frameVertices: Array<ArrayLike<number>>;
		slotIndex: number;
		attachment: VertexAttachment;

		constructor (frameCount: number) {
			super(frameCount);
			this.frames = Utils.newFloatArray(frameCount);
			this.frameVertices = new Array<ArrayLike<number>>(frameCount);
		}

		/** Sets the time of the specified keyframe. */
		setFrame (frameIndex: number, time: number, vertices: ArrayLike<number>) {
			this.frames[frameIndex] = time;
			this.frameVertices[frameIndex] = vertices;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number) {
			let slot: Slot = skeleton.slots[this.slotIndex];
			let slotAttachment: Attachment = slot.getAttachment();
			if (!(slotAttachment instanceof VertexAttachment) || !(<VertexAttachment>slotAttachment).applyDeform(this.attachment)) return;

			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let frameVertices = this.frameVertices;
			let vertexCount = frameVertices[0].length;

			let verticesArray: Array<number> = slot.attachmentVertices;
			if (verticesArray.length != vertexCount) alpha = 1; // Don't mix from uninitialized slot vertices.
			let vertices: Array<number> = Utils.setArraySize(verticesArray, vertexCount);

			if (time >= frames[frames.length - 1]) { // Time is after last frame.
				let lastVertices = frameVertices[frames.length - 1];
				if (alpha < 1) {
					for (let i = 0; i < vertexCount; i++)
						vertices[i] += (lastVertices[i] - vertices[i]) * alpha;
				} else
					Utils.arrayCopy(lastVertices, 0, vertices, 0, vertexCount);
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time);
			let prevVertices = frameVertices[frame - 1];
			let nextVertices = frameVertices[frame];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime));

			if (alpha < 1) {
				for (let i = 0; i < vertexCount; i++) {
					let prev = prevVertices[i];
					vertices[i] += (prev + (nextVertices[i] - prev) * percent - vertices[i]) * alpha;
				}
			} else {
				for (let i = 0; i < vertexCount; i++) {
					let prev = prevVertices[i];
					vertices[i] = prev + (nextVertices[i] - prev) * percent;
				}
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

		/** Sets the time, mix and bend direction of the specified keyframe. */
		setFrame (frameIndex: number, time: number, mix: number, bendDirection: number) {
			frameIndex *= IkConstraintTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + IkConstraintTimeline.MIX] = mix;
			this.frames[frameIndex + IkConstraintTimeline.BEND_DIRECTION] = bendDirection;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let constraint: IkConstraint = skeleton.ikConstraints[this.ikConstraintIndex];

			if (time >= frames[frames.length - IkConstraintTimeline.ENTRIES]) { // Time is after last frame.
				constraint.mix += (frames[frames.length + IkConstraintTimeline.PREV_MIX] - constraint.mix) * alpha;
				constraint.bendDirection = Math.floor(frames[frames.length + IkConstraintTimeline.PREV_BEND_DIRECTION]);
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, IkConstraintTimeline.ENTRIES);
			let mix = frames[frame + IkConstraintTimeline.PREV_MIX];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame / IkConstraintTimeline.ENTRIES - 1,
				1 - (time - frameTime) / (frames[frame + IkConstraintTimeline.PREV_TIME] - frameTime));

			constraint.mix += (mix + (frames[frame + IkConstraintTimeline.MIX] - mix) * percent - constraint.mix) * alpha;
			constraint.bendDirection = Math.floor(frames[frame + IkConstraintTimeline.PREV_BEND_DIRECTION]);
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

		/** Sets the time and mixes of the specified keyframe. */
		setFrame (frameIndex: number, time: number, rotateMix: number, translateMix: number, scaleMix: number, shearMix: number) {
			frameIndex *= TransformConstraintTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TransformConstraintTimeline.ROTATE] = rotateMix;
			this.frames[frameIndex + TransformConstraintTimeline.TRANSLATE] = translateMix;
			this.frames[frameIndex + TransformConstraintTimeline.SCALE] = scaleMix;
			this.frames[frameIndex + TransformConstraintTimeline.SHEAR] = shearMix;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let constraint: TransformConstraint = skeleton.transformConstraints[this.transformConstraintIndex];

			if (time >= frames[frames.length - TransformConstraintTimeline.ENTRIES]) { // Time is after last frame.
				let i = frames.length;
				constraint.rotateMix += (frames[i + TransformConstraintTimeline.PREV_ROTATE] - constraint.rotateMix) * alpha;
				constraint.translateMix += (frames[i + TransformConstraintTimeline.PREV_TRANSLATE] - constraint.translateMix) * alpha;
				constraint.scaleMix += (frames[i + TransformConstraintTimeline.PREV_SCALE] - constraint.scaleMix) * alpha;
				constraint.shearMix += (frames[i + TransformConstraintTimeline.PREV_SHEAR] - constraint.shearMix) * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, TransformConstraintTimeline.ENTRIES);
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame / TransformConstraintTimeline.ENTRIES - 1,
				1 - (time - frameTime) / (frames[frame + TransformConstraintTimeline.PREV_TIME] - frameTime));

			let rotate = frames[frame + TransformConstraintTimeline.PREV_ROTATE];
			let translate = frames[frame + TransformConstraintTimeline.PREV_TRANSLATE];
			let scale = frames[frame + TransformConstraintTimeline.PREV_SCALE];
			let shear = frames[frame + TransformConstraintTimeline.PREV_SHEAR];
			constraint.rotateMix += (rotate + (frames[frame + TransformConstraintTimeline.ROTATE] - rotate) * percent - constraint.rotateMix) * alpha;
			constraint.translateMix += (translate + (frames[frame + TransformConstraintTimeline.TRANSLATE] - translate) * percent - constraint.translateMix)
				* alpha;
			constraint.scaleMix += (scale + (frames[frame + TransformConstraintTimeline.SCALE] - scale) * percent - constraint.scaleMix) * alpha;
			constraint.shearMix += (shear + (frames[frame + TransformConstraintTimeline.SHEAR] - shear) * percent - constraint.shearMix) * alpha;
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

		/** Sets the time and value of the specified keyframe. */
		setFrame (frameIndex: number, time: number, value: number) {
			frameIndex *= PathConstraintPositionTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + PathConstraintPositionTimeline.VALUE] = value;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];

			if (time >= frames[frames.length - PathConstraintPositionTimeline.ENTRIES]) { // Time is after last frame.
				let i = frames.length;
				constraint.position += (frames[i + PathConstraintPositionTimeline.PREV_VALUE] - constraint.position) * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, PathConstraintPositionTimeline.ENTRIES);
			let position = frames[frame + PathConstraintPositionTimeline.PREV_VALUE];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame / PathConstraintPositionTimeline.ENTRIES - 1,
				1 - (time - frameTime) / (frames[frame + PathConstraintPositionTimeline.PREV_TIME] - frameTime));

			constraint.position += (position + (frames[frame + PathConstraintPositionTimeline.VALUE] - position) * percent - constraint.position) * alpha;
		}
	}

	export class PathConstraintSpacingTimeline extends PathConstraintPositionTimeline {
		constructor (frameCount: number) {
			super(frameCount);
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];

			if (time >= frames[frames.length - PathConstraintSpacingTimeline.ENTRIES]) { // Time is after last frame.
				let i = frames.length;
				constraint.spacing += (frames[i + PathConstraintSpacingTimeline.PREV_VALUE] - constraint.spacing) * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, PathConstraintSpacingTimeline.ENTRIES);
			let spacing = frames[frame + PathConstraintSpacingTimeline.PREV_VALUE];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame / PathConstraintSpacingTimeline.ENTRIES - 1,
				1 - (time - frameTime) / (frames[frame + PathConstraintSpacingTimeline.PREV_TIME] - frameTime));

			constraint.spacing += (spacing + (frames[frame + PathConstraintSpacingTimeline.VALUE] - spacing) * percent - constraint.spacing) * alpha;
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

		/** Sets the time and mixes of the specified keyframe. */
		setFrame (frameIndex: number, time: number, rotateMix: number, translateMix: number) {
			frameIndex *= PathConstraintMixTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + PathConstraintMixTimeline.ROTATE] = rotateMix;
			this.frames[frameIndex + PathConstraintMixTimeline.TRANSLATE] = translateMix;
		}

		apply (skeleton: Skeleton, lastTime: number, time: number, firedEvents: Array<Event>, alpha: number) {
			let frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			let constraint: PathConstraint = skeleton.pathConstraints[this.pathConstraintIndex];

			if (time >= frames[frames.length - PathConstraintMixTimeline.ENTRIES]) { // Time is after last frame.
				let i = frames.length;
				constraint.rotateMix += (frames[i + PathConstraintMixTimeline.PREV_ROTATE] - constraint.rotateMix) * alpha;
				constraint.translateMix += (frames[i + PathConstraintMixTimeline.PREV_TRANSLATE] - constraint.translateMix) * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			let frame = Animation.binarySearch(frames, time, PathConstraintMixTimeline.ENTRIES);
			let rotate = frames[frame + PathConstraintMixTimeline.PREV_ROTATE];
			let translate = frames[frame + PathConstraintMixTimeline.PREV_TRANSLATE];
			let frameTime = frames[frame];
			let percent = this.getCurvePercent(frame / PathConstraintMixTimeline.ENTRIES - 1,
				1 - (time - frameTime) / (frames[frame + PathConstraintMixTimeline.PREV_TIME] - frameTime));

			constraint.rotateMix += (rotate + (frames[frame + PathConstraintMixTimeline.ROTATE] - rotate) * percent - constraint.rotateMix) * alpha;
			constraint.translateMix += (translate + (frames[frame + PathConstraintMixTimeline.TRANSLATE] - translate) * percent - constraint.translateMix)
				* alpha;
		}
	}
}
