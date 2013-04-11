/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

using System;
using System.Collections.Generic;

namespace Spine {
	public class Animation {
		public String Name { get; private set; }
		public List<Timeline> Timelines { get; set; }
		public float Duration { get; set; }

		public Animation (String name, List<Timeline> timelines, float duration) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			if (timelines == null) throw new ArgumentNullException("timelines cannot be null.");
			Name = name;
			Timelines = timelines;
			Duration = duration;
		}

		/** Poses the skeleton at the specified time for this animation. */
		public void Apply (Skeleton skeleton, float time, bool loop) {
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");

			if (loop && Duration != 0) time %= Duration;

			List<Timeline> timelines = Timelines;
			for (int i = 0, n = timelines.Count; i < n; i++)
				timelines[i].Apply(skeleton, time, 1);
		}

		/** Poses the skeleton at the specified time for this animation mixed with the current pose.
		 * @param alpha The amount of this animation that affects the current pose. */
		public void Mix (Skeleton skeleton, float time, bool loop, float alpha) {
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");

			if (loop && Duration != 0) time %= Duration;

			List<Timeline> timelines = Timelines;
			for (int i = 0, n = timelines.Count; i < n; i++)
				timelines[i].Apply(skeleton, time, alpha);
		}

		/** @param target After the first and before the last entry. */
		internal static int binarySearch (float[] values, float target, int step) {
			int low = 0;
			int high = values.Length / step - 2;
			if (high == 0) return step;
			int current = (int)((uint)high >> 1);
			while (true) {
				if (values[(current + 1) * step] <= target)
					low = current + 1;
				else
					high = current;
				if (low == high) return (low + 1) * step;
				current = (int)((uint)(low + high) >> 1);
			}
		}

		internal static int linearSearch (float[] values, float target, int step) {
			for (int i = 0, last = values.Length - step; i <= last; i += step)
				if (values[i] > target) return i;
			return -1;
		}
	}

	public interface Timeline {
		/** Sets the value(s) for the specified time. */
		void Apply (Skeleton skeleton, float time, float alpha);
	}

	/** Base class for frames that use an interpolation bezier curve. */
	abstract public class CurveTimeline : Timeline {
		static protected float LINEAR = 0;
		static protected float STEPPED = -1;
		static protected int BEZIER_SEGMENTS = 10;

		private float[] curves; // dfx, dfy, ddfx, ddfy, dddfx, dddfy, ...
		public int FrameCount {
			get {
				return curves.Length / 6 + 1;
			}
		}

		public CurveTimeline (int frameCount) {
			curves = new float[(frameCount - 1) * 6];
		}

		abstract public void Apply (Skeleton skeleton, float time, float alpha);

		public void SetLinear (int frameIndex) {
			curves[frameIndex * 6] = LINEAR;
		}

		public void SetStepped (int frameIndex) {
			curves[frameIndex * 6] = STEPPED;
		}

		/** Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
	 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
	 * the difference between the keyframe's values. */
		public void SetCurve (int frameIndex, float cx1, float cy1, float cx2, float cy2) {
			float subdiv_step = 1f / BEZIER_SEGMENTS;
			float subdiv_step2 = subdiv_step * subdiv_step;
			float subdiv_step3 = subdiv_step2 * subdiv_step;
			float pre1 = 3 * subdiv_step;
			float pre2 = 3 * subdiv_step2;
			float pre4 = 6 * subdiv_step2;
			float pre5 = 6 * subdiv_step3;
			float tmp1x = -cx1 * 2 + cx2;
			float tmp1y = -cy1 * 2 + cy2;
			float tmp2x = (cx1 - cx2) * 3 + 1;
			float tmp2y = (cy1 - cy2) * 3 + 1;
			int i = frameIndex * 6;
			float[] curves = this.curves;
			curves[i] = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3;
			curves[i + 1] = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;
			curves[i + 2] = tmp1x * pre4 + tmp2x * pre5;
			curves[i + 3] = tmp1y * pre4 + tmp2y * pre5;
			curves[i + 4] = tmp2x * pre5;
			curves[i + 5] = tmp2y * pre5;
		}

		public float GetCurvePercent (int frameIndex, float percent) {
			int curveIndex = frameIndex * 6;
			float[] curves = this.curves;
			float dfx = curves[curveIndex];
			if (dfx == LINEAR) return percent;
			if (dfx == STEPPED) return 0;
			float dfy = curves[curveIndex + 1];
			float ddfx = curves[curveIndex + 2];
			float ddfy = curves[curveIndex + 3];
			float dddfx = curves[curveIndex + 4];
			float dddfy = curves[curveIndex + 5];
			float x = dfx, y = dfy;
			int i = BEZIER_SEGMENTS - 2;
			while (true) {
				if (x >= percent) {
					float lastX = x - dfx;
					float lastY = y - dfy;
					return lastY + (y - lastY) * (percent - lastX) / (x - lastX);
				}
				if (i == 0) break;
				i--;
				dfx += ddfx;
				dfy += ddfy;
				ddfx += dddfx;
				ddfy += dddfy;
				x += dfx;
				y += dfy;
			}
			return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
		}
	}

	public class RotateTimeline : CurveTimeline {
		static protected int LAST_FRAME_TIME = -2;
		static protected int FRAME_VALUE = 1;

		public int BoneIndex { get; set; }
		public float[] Frames { get; private set; } // time, value, ...

		public RotateTimeline (int frameCount)
			: base(frameCount) {
			Frames = new float[frameCount * 2];
		}

		/** Sets the time and value of the specified keyframe. */
		public void SetFrame (int frameIndex, float time, float angle) {
			frameIndex *= 2;
			Frames[frameIndex] = time;
			Frames[frameIndex + 1] = angle;
		}

		override public void Apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = Frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.Bones[BoneIndex];

			float amount;

			if (time >= frames[frames.Length - 2]) { // Time is after last frame.
				amount = bone.Data.Rotation + frames[frames.Length - 1] - bone.Rotation;
				while (amount > 180)
					amount -= 360;
				while (amount < -180)
					amount += 360;
				bone.Rotation += amount * alpha;
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 2);
			float lastFrameValue = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime);
			percent = GetCurvePercent(frameIndex / 2 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			amount = frames[frameIndex + FRAME_VALUE] - lastFrameValue;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			amount = bone.Data.Rotation + (lastFrameValue + amount * percent) - bone.Rotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			bone.Rotation += amount * alpha;
		}
	}

	public class TranslateTimeline : CurveTimeline {
		static protected int LAST_FRAME_TIME = -3;
		static protected int FRAME_X = 1;
		static protected int FRAME_Y = 2;

		public int BoneIndex { get; set; }
		public float[] Frames { get; private set; } // time, value, value, ...

		public TranslateTimeline (int frameCount)
			: base(frameCount) {
			Frames = new float[frameCount * 3];
		}

		/** Sets the time and value of the specified keyframe. */
		public void SetFrame (int frameIndex, float time, float x, float y) {
			frameIndex *= 3;
			Frames[frameIndex] = time;
			Frames[frameIndex + 1] = x;
			Frames[frameIndex + 2] = y;
		}

		override public void Apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = Frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.Bones[BoneIndex];

			if (time >= frames[frames.Length - 3]) { // Time is after last frame.
				bone.X += (bone.Data.X + frames[frames.Length - 2] - bone.X) * alpha;
				bone.Y += (bone.Data.Y + frames[frames.Length - 1] - bone.Y) * alpha;
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 3);
			float lastFrameX = frames[frameIndex - 2];
			float lastFrameY = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime);
			percent = GetCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			bone.X += (bone.Data.X + lastFrameX + (frames[frameIndex + FRAME_X] - lastFrameX) * percent - bone.X) * alpha;
			bone.Y += (bone.Data.Y + lastFrameY + (frames[frameIndex + FRAME_Y] - lastFrameY) * percent - bone.Y) * alpha;
		}
	}

	public class ScaleTimeline : TranslateTimeline {
		public ScaleTimeline (int frameCount)
			: base(frameCount) {
		}

		override public void Apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = Frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.Bones[BoneIndex];
			if (time >= frames[frames.Length - 3]) { // Time is after last frame.
				bone.ScaleX += (bone.Data.ScaleX - 1 + frames[frames.Length - 2] - bone.ScaleX) * alpha;
				bone.ScaleY += (bone.Data.ScaleY - 1 + frames[frames.Length - 1] - bone.ScaleY) * alpha;
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 3);
			float lastFrameX = frames[frameIndex - 2];
			float lastFrameY = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime);
			percent = GetCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			bone.ScaleX += (bone.Data.ScaleX - 1 + lastFrameX + (frames[frameIndex + FRAME_X] - lastFrameX) * percent - bone.ScaleX) * alpha;
			bone.ScaleY += (bone.Data.ScaleY - 1 + lastFrameY + (frames[frameIndex + FRAME_Y] - lastFrameY) * percent - bone.ScaleY) * alpha;
		}
	}

	public class ColorTimeline : CurveTimeline {
		static protected int LAST_FRAME_TIME = -5;
		static protected int FRAME_R = 1;
		static protected int FRAME_G = 2;
		static protected int FRAME_B = 3;
		static protected int FRAME_A = 4;

		public int SlotIndex { get; set; }
		public float[] Frames { get; private set; } // time, r, g, b, a, ...

		public ColorTimeline (int frameCount)
			: base(frameCount) {
			Frames = new float[frameCount * 5];
		}

		/** Sets the time and value of the specified keyframe. */
		public void setFrame (int frameIndex, float time, float r, float g, float b, float a) {
			frameIndex *= 5;
			Frames[frameIndex] = time;
			Frames[frameIndex + 1] = r;
			Frames[frameIndex + 2] = g;
			Frames[frameIndex + 3] = b;
			Frames[frameIndex + 4] = a;
		}

		override public void Apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = Frames;
			if (time < frames[0]) return; // Time is before first frame.

			Slot slot = skeleton.Slots[SlotIndex];

			if (time >= frames[frames.Length - 5]) { // Time is after last frame.
				int i = frames.Length - 1;
				slot.R = frames[i - 3];
				slot.G = frames[i - 2];
				slot.B = frames[i - 1];
				slot.A = frames[i];
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 5);
			float lastFrameR = frames[frameIndex - 4];
			float lastFrameG = frames[frameIndex - 3];
			float lastFrameB = frames[frameIndex - 2];
			float lastFrameA = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime);
			percent = GetCurvePercent(frameIndex / 5 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			float r = lastFrameR + (frames[frameIndex + FRAME_R] - lastFrameR) * percent;
			float g = lastFrameG + (frames[frameIndex + FRAME_G] - lastFrameG) * percent;
			float b = lastFrameB + (frames[frameIndex + FRAME_B] - lastFrameB) * percent;
			float a = lastFrameA + (frames[frameIndex + FRAME_A] - lastFrameA) * percent;
			if (alpha < 1) {
				slot.R += (r - slot.R) * alpha;
				slot.G += (g - slot.G) * alpha;
				slot.B += (b - slot.B) * alpha;
				slot.A += (a - slot.A) * alpha;
			} else {
				slot.R = r;
				slot.G = g;
				slot.B = b;
				slot.A = a;
			}
		}
	}

	public class AttachmentTimeline : Timeline {
		public int SlotIndex { get; set; }
		public float[] Frames { get; private set; } // time, ...
		public String[] AttachmentNames { get; private set; }
		public int FrameCount {
			get {
				return Frames.Length;
			}
		}

		public AttachmentTimeline (int frameCount) {
			Frames = new float[frameCount];
			AttachmentNames = new String[frameCount];
		}

		/** Sets the time and value of the specified keyframe. */
		public void setFrame (int frameIndex, float time, String attachmentName) {
			Frames[frameIndex] = time;
			AttachmentNames[frameIndex] = attachmentName;
		}

		public void Apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = Frames;
			if (time < frames[0]) return; // Time is before first frame.

			int frameIndex;
			if (time >= frames[frames.Length - 1]) // Time is after last frame.
				frameIndex = frames.Length - 1;
			else
				frameIndex = Animation.binarySearch(frames, time, 1) - 1;

			String attachmentName = AttachmentNames[frameIndex];
			skeleton.Slots[SlotIndex].Attachment =
				 attachmentName == null ? null : skeleton.GetAttachment(SlotIndex, attachmentName);
		}
	}
}
