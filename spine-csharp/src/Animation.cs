/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License, Professional License, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
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
 *****************************************************************************/

using System;
using System.Collections.Generic;

namespace Spine {
	public class Animation {
		internal List<Timeline> timelines;
		internal float duration;
		internal String name;

		public String Name { get { return name; } }
		public List<Timeline> Timelines { get { return timelines; } set { timelines = value; } }
		public float Duration { get { return duration; } set { duration = value; } }

		public Animation (String name, List<Timeline> timelines, float duration) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			if (timelines == null) throw new ArgumentNullException("timelines cannot be null.");
			this.name = name;
			this.timelines = timelines;
			this.duration = duration;
		}

		/// <summary>Poses the skeleton at the specified time for this animation.</summary>
		/// <param name="lastTime">The last time the animation was applied.</param>
		/// <param name="events">Any triggered events are added.</param>
		public void Apply (Skeleton skeleton, float lastTime, float time, bool loop, List<Event> events) {
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				lastTime %= duration;
			}

			List<Timeline> timelines = this.timelines;
			for (int i = 0, n = timelines.Count; i < n; i++)
				timelines[i].Apply(skeleton, lastTime, time, events, 1);
		}

		/// <summary>Poses the skeleton at the specified time for this animation mixed with the current pose.</summary>
		/// <param name="lastTime">The last time the animation was applied.</param>
		/// <param name="events">Any triggered events are added.</param>
		/// <param name="alpha">The amount of this animation that affects the current pose.</param>
		public void Mix (Skeleton skeleton, float lastTime, float time, bool loop, List<Event> events, float alpha) {
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				lastTime %= duration;
			}

			List<Timeline> timelines = this.timelines;
			for (int i = 0, n = timelines.Count; i < n; i++)
				timelines[i].Apply(skeleton, lastTime, time, events, alpha);
		}

		/// <param name="target">After the first and before the last entry.</param>
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
		/// <summary>Sets the value(s) for the specified time.</summary>
		void Apply (Skeleton skeleton, float lastTime, float time, List<Event> firedEvents, float alpha);
	}

	/// <summary>Base class for frames that use an interpolation bezier curve.</summary>
	abstract public class CurveTimeline : Timeline {
		static protected float LINEAR = 0;
		static protected float STEPPED = -1;
		static protected int BEZIER_SEGMENTS = 10;

		private float[] curves; // dfx, dfy, ddfx, ddfy, dddfx, dddfy, ...
		public int FrameCount { get { return curves.Length / 6 + 1; } }

		public CurveTimeline (int frameCount) {
			curves = new float[(frameCount - 1) * 6];
		}

		abstract public void Apply (Skeleton skeleton, float lastTime, float time, List<Event> firedEvents, float alpha);

		public void SetLinear (int frameIndex) {
			curves[frameIndex * 6] = LINEAR;
		}

		public void SetStepped (int frameIndex) {
			curves[frameIndex * 6] = STEPPED;
		}

		/// <summary>Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
	   /// cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
		/// the difference between the keyframe's values.</summary>
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

		internal int boneIndex;
		internal float[] frames;

		public int BoneIndex { get { return boneIndex; } set { boneIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, value, ...

		public RotateTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * 2];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float angle) {
			frameIndex *= 2;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = angle;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, List<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.bones[boneIndex];

			float amount;

			if (time >= frames[frames.Length - 2]) { // Time is after last frame.
				amount = bone.data.rotation + frames[frames.Length - 1] - bone.rotation;
				while (amount > 180)
					amount -= 360;
				while (amount < -180)
					amount += 360;
				bone.rotation += amount * alpha;
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
			amount = bone.data.rotation + (lastFrameValue + amount * percent) - bone.rotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			bone.rotation += amount * alpha;
		}
	}

	public class TranslateTimeline : CurveTimeline {
		static protected int LAST_FRAME_TIME = -3;
		static protected int FRAME_X = 1;
		static protected int FRAME_Y = 2;

		internal int boneIndex;
		internal float[] frames;

		public int BoneIndex { get { return boneIndex; } set { boneIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, value, value, ...

		public TranslateTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * 3];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float x, float y) {
			frameIndex *= 3;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = x;
			frames[frameIndex + 2] = y;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, List<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.bones[boneIndex];

			if (time >= frames[frames.Length - 3]) { // Time is after last frame.
				bone.x += (bone.data.x + frames[frames.Length - 2] - bone.x) * alpha;
				bone.y += (bone.data.y + frames[frames.Length - 1] - bone.y) * alpha;
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 3);
			float lastFrameX = frames[frameIndex - 2];
			float lastFrameY = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime);
			percent = GetCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			bone.x += (bone.data.x + lastFrameX + (frames[frameIndex + FRAME_X] - lastFrameX) * percent - bone.x) * alpha;
			bone.y += (bone.data.y + lastFrameY + (frames[frameIndex + FRAME_Y] - lastFrameY) * percent - bone.y) * alpha;
		}
	}

	public class ScaleTimeline : TranslateTimeline {
		public ScaleTimeline (int frameCount)
			: base(frameCount) {
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, List<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.bones[boneIndex];
			if (time >= frames[frames.Length - 3]) { // Time is after last frame.
				bone.scaleX += (bone.data.scaleX - 1 + frames[frames.Length - 2] - bone.scaleX) * alpha;
				bone.scaleY += (bone.data.scaleY - 1 + frames[frames.Length - 1] - bone.scaleY) * alpha;
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 3);
			float lastFrameX = frames[frameIndex - 2];
			float lastFrameY = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime);
			percent = GetCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			bone.scaleX += (bone.data.scaleX - 1 + lastFrameX + (frames[frameIndex + FRAME_X] - lastFrameX) * percent - bone.scaleX) * alpha;
			bone.scaleY += (bone.data.scaleY - 1 + lastFrameY + (frames[frameIndex + FRAME_Y] - lastFrameY) * percent - bone.scaleY) * alpha;
		}
	}

	public class ColorTimeline : CurveTimeline {
		static protected int LAST_FRAME_TIME = -5;
		static protected int FRAME_R = 1;
		static protected int FRAME_G = 2;
		static protected int FRAME_B = 3;
		static protected int FRAME_A = 4;

		internal int slotIndex;
		internal float[] frames;

		public int SlotIndex { get { return slotIndex; } set { slotIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, r, g, b, a, ...

		public ColorTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * 5];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void setFrame (int frameIndex, float time, float r, float g, float b, float a) {
			frameIndex *= 5;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = r;
			frames[frameIndex + 2] = g;
			frames[frameIndex + 3] = b;
			frames[frameIndex + 4] = a;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, List<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Slot slot = skeleton.slots[slotIndex];

			if (time >= frames[frames.Length - 5]) { // Time is after last frame.
				int i = frames.Length - 1;
				slot.r = frames[i - 3];
				slot.g = frames[i - 2];
				slot.b = frames[i - 1];
				slot.a = frames[i];
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
				slot.r += (r - slot.r) * alpha;
				slot.g += (g - slot.g) * alpha;
				slot.b += (b - slot.b) * alpha;
				slot.a += (a - slot.a) * alpha;
			} else {
				slot.r = r;
				slot.g = g;
				slot.b = b;
				slot.a = a;
			}
		}
	}

	public class AttachmentTimeline : Timeline {
		internal int slotIndex;
		internal float[] frames;
		private String[] attachmentNames;

		public int SlotIndex { get { return slotIndex; } set { slotIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, ...
		public String[] AttachmentNames { get { return attachmentNames; } set { attachmentNames = value; } }
		public int FrameCount { get { return frames.Length; } }

		public AttachmentTimeline (int frameCount) {
			frames = new float[frameCount];
			attachmentNames = new String[frameCount];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void setFrame (int frameIndex, float time, String attachmentName) {
			frames[frameIndex] = time;
			attachmentNames[frameIndex] = attachmentName;
		}

		public void Apply (Skeleton skeleton, float lastTime, float time, List<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			int frameIndex;
			if (time >= frames[frames.Length - 1]) // Time is after last frame.
				frameIndex = frames.Length - 1;
			else
				frameIndex = Animation.binarySearch(frames, time, 1) - 1;

			String attachmentName = attachmentNames[frameIndex];
			skeleton.slots[slotIndex].Attachment =
				 attachmentName == null ? null : skeleton.GetAttachment(slotIndex, attachmentName);
		}
	}

	public class EventTimeline : Timeline {
		internal float[] frames;
		private Event[] events;

		public float[] Frames { get { return frames; } set { frames = value; } } // time, ...
		public Event[] Events { get { return events; } set { events = value; } }
		public int FrameCount { get { return frames.Length; } }

		public EventTimeline (int frameCount) {
			frames = new float[frameCount];
			events = new Event[frameCount];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void setFrame (int frameIndex, float time, Event e) {
			frames[frameIndex] = time;
			events[frameIndex] = e;
		}

		public void Apply (Skeleton skeleton, float lastTime, float time, List<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			int frameCount = frames.Length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				Apply(skeleton, lastTime, int.MaxValue, firedEvents, alpha);
				lastTime = 0;
			}

			int frameIndex;
			if (lastTime <= frames[0] || frameCount == 1)
				frameIndex = 0;
			else {
				frameIndex = Animation.binarySearch(frames, lastTime, 1);
				float frame = frames[frameIndex];
				while (frameIndex > 0) { // Fire multiple events with the same frame.
					if (frames[frameIndex - 1] != frame) break;
					frameIndex--;
				}
			}
			for (; frameIndex < frameCount && time >= frames[frameIndex]; frameIndex++)
				firedEvents.Add(events[frameIndex]);
		}
	}

	public class DrawOrderTimeline : Timeline {
		internal float[] frames;
		private int[][] drawOrders;

		public float[] Frames { get { return frames; } set { frames = value; } } // time, ...
		public int[][] DrawOrders { get { return drawOrders; } set { drawOrders = value; } }
		public int FrameCount { get { return frames.Length; } }

		public DrawOrderTimeline (int frameCount) {
			frames = new float[frameCount];
			drawOrders = new int[frameCount][];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		/// <param name="drawOrder">May be null to use bind pose draw order.</param>
		public void setFrame (int frameIndex, float time, int[] drawOrder) {
			frames[frameIndex] = time;
			drawOrders[frameIndex] = drawOrder;
		}

		public void Apply (Skeleton skeleton, float lastTime, float time, List<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			int frameIndex;
			if (time >= frames[frames.Length - 1]) // Time is after last frame.
				frameIndex = frames.Length - 1;
			else
				frameIndex = Animation.binarySearch(frames, time, 1) - 1;

			List<Slot> drawOrder = skeleton.drawOrder;
			List<Slot> slots = skeleton.slots;
			int[] drawOrderToSetupIndex = drawOrders[frameIndex];
			if (drawOrderToSetupIndex == null) {
				drawOrder.Clear();
				drawOrder.AddRange(slots);
			} else {
				for (int i = 0, n = drawOrderToSetupIndex.Length; i < n; i++)
					drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		}
	}
}
