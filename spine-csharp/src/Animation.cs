/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections.Generic;

namespace Spine {
	public class Animation {
		internal ExposedList<Timeline> timelines;
		internal float duration;
		internal String name;

		public String Name { get { return name; } }
		public ExposedList<Timeline> Timelines { get { return timelines; } set { timelines = value; } }
		public float Duration { get { return duration; } set { duration = value; } }

		public Animation (String name, ExposedList<Timeline> timelines, float duration) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			if (timelines == null) throw new ArgumentNullException("timelines cannot be null.");
			this.name = name;
			this.timelines = timelines;
			this.duration = duration;
		}

		/// <summary>Poses the skeleton at the specified time for this animation.</summary>
		/// <param name="lastTime">The last time the animation was applied.</param>
		/// <param name="events">Any triggered events are added.</param>
		public void Apply (Skeleton skeleton, float lastTime, float time, bool loop, ExposedList<Event> events) {
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				lastTime %= duration;
			}

			ExposedList<Timeline> timelines = this.timelines;
			for (int i = 0, n = timelines.Count; i < n; i++)
				timelines.Items[i].Apply(skeleton, lastTime, time, events, 1);
		}

		/// <summary>Poses the skeleton at the specified time for this animation mixed with the current pose.</summary>
		/// <param name="lastTime">The last time the animation was applied.</param>
		/// <param name="events">Any triggered events are added.</param>
		/// <param name="alpha">The amount of this animation that affects the current pose.</param>
		public void Mix (Skeleton skeleton, float lastTime, float time, bool loop, ExposedList<Event> events, float alpha) {
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				lastTime %= duration;
			}

			ExposedList<Timeline> timelines = this.timelines;
			for (int i = 0, n = timelines.Count; i < n; i++)
				timelines.Items[i].Apply(skeleton, lastTime, time, events, alpha);
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

		/// <param name="target">After the first and before the last entry.</param>
		internal static int binarySearch (float[] values, float target) {
			int low = 0;
			int high = values.Length - 2;
			if (high == 0) return 1;
			int current = (int)((uint)high >> 1);
			while (true) {
				if (values[(current + 1)] <= target)
					low = current + 1;
				else
					high = current;
				if (low == high) return (low + 1);
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
		/// <param name="events">May be null to not collect fired events.</param>
		void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha);
	}

	/// <summary>Base class for frames that use an interpolation bezier curve.</summary>
	abstract public class CurveTimeline : Timeline {
		protected const float LINEAR = 0, STEPPED = 1, BEZIER = 2;
		protected const int BEZIER_SEGMENTS = 10, BEZIER_SIZE = BEZIER_SEGMENTS * 2 - 1;

		private float[] curves; // type, x, y, ...
		public int FrameCount { get { return curves.Length / BEZIER_SIZE + 1; } }

		public CurveTimeline (int frameCount) {
			curves = new float[(frameCount - 1) * BEZIER_SIZE];
		}

		abstract public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha);

		public void SetLinear (int frameIndex) {
			curves[frameIndex * BEZIER_SIZE] = LINEAR;
		}

		public void SetStepped (int frameIndex) {
			curves[frameIndex * BEZIER_SIZE] = STEPPED;
		}

		/// <summary>Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
		/// cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
		/// the difference between the keyframe's values.</summary>
		public void SetCurve (int frameIndex, float cx1, float cy1, float cx2, float cy2) {
			float subdiv1 = 1f / BEZIER_SEGMENTS, subdiv2 = subdiv1 * subdiv1, subdiv3 = subdiv2 * subdiv1;
			float pre1 = 3 * subdiv1, pre2 = 3 * subdiv2, pre4 = 6 * subdiv2, pre5 = 6 * subdiv3;
			float tmp1x = -cx1 * 2 + cx2, tmp1y = -cy1 * 2 + cy2, tmp2x = (cx1 - cx2) * 3 + 1, tmp2y = (cy1 - cy2) * 3 + 1;
			float dfx = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv3, dfy = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv3;
			float ddfx = tmp1x * pre4 + tmp2x * pre5, ddfy = tmp1y * pre4 + tmp2y * pre5;
			float dddfx = tmp2x * pre5, dddfy = tmp2y * pre5;

			int i = frameIndex * BEZIER_SIZE;
			float[] curves = this.curves;
			curves[i++] = BEZIER;

			float x = dfx, y = dfy;
			for (int n = i + BEZIER_SIZE - 1; i < n; i += 2) {
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

		public float GetCurvePercent (int frameIndex, float percent) {
			float[] curves = this.curves;
			int i = frameIndex * BEZIER_SIZE;
			float type = curves[i];
			if (type == LINEAR) return percent;
			if (type == STEPPED) return 0;
			i++;
			float x = 0;
			for (int start = i, n = i + BEZIER_SIZE - 1; i < n; i += 2) {
				x = curves[i];
				if (x >= percent) {
					float prevX, prevY;
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
			float y = curves[i - 1];
			return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
		}
		public float GetCurveType (int frameIndex) {
			return curves[frameIndex * BEZIER_SIZE];
		}
	}

	public class RotateTimeline : CurveTimeline {
		protected const int PREV_FRAME_TIME = -2;
		protected const int FRAME_VALUE = 1;

		internal int boneIndex;
		internal float[] frames;

		public int BoneIndex { get { return boneIndex; } set { boneIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, value, ...

		public RotateTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount << 1];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float angle) {
			frameIndex *= 2;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = angle;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.bones.Items[boneIndex];

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

			// Interpolate between the previous frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 2);
			float prevFrameValue = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime);
			percent = GetCurvePercent((frameIndex >> 1) - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			amount = frames[frameIndex + FRAME_VALUE] - prevFrameValue;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			amount = bone.data.rotation + (prevFrameValue + amount * percent) - bone.rotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			bone.rotation += amount * alpha;
		}
	}

	public class TranslateTimeline : CurveTimeline {
		protected const int PREV_FRAME_TIME = -3;
		protected const int FRAME_X = 1;
		protected const int FRAME_Y = 2;

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

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.bones.Items[boneIndex];

			if (time >= frames[frames.Length - 3]) { // Time is after last frame.
				bone.x += (bone.data.x + frames[frames.Length - 2] - bone.x) * alpha;
				bone.y += (bone.data.y + frames[frames.Length - 1] - bone.y) * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 3);
			float prevFrameX = frames[frameIndex - 2];
			float prevFrameY = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime);
			percent = GetCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			bone.x += (bone.data.x + prevFrameX + (frames[frameIndex + FRAME_X] - prevFrameX) * percent - bone.x) * alpha;
			bone.y += (bone.data.y + prevFrameY + (frames[frameIndex + FRAME_Y] - prevFrameY) * percent - bone.y) * alpha;
		}
	}

	public class ScaleTimeline : TranslateTimeline {
		public ScaleTimeline (int frameCount)
			: base(frameCount) {
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.bones.Items[boneIndex];
			if (time >= frames[frames.Length - 3]) { // Time is after last frame.
				bone.scaleX += (bone.data.scaleX * frames[frames.Length - 2] - bone.scaleX) * alpha;
				bone.scaleY += (bone.data.scaleY * frames[frames.Length - 1] - bone.scaleY) * alpha;
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 3);
			float prevFrameX = frames[frameIndex - 2];
			float prevFrameY = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime);
			percent = GetCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			bone.scaleX += (bone.data.scaleX * (prevFrameX + (frames[frameIndex + FRAME_X] - prevFrameX) * percent) - bone.scaleX) * alpha;
			bone.scaleY += (bone.data.scaleY * (prevFrameY + (frames[frameIndex + FRAME_Y] - prevFrameY) * percent) - bone.scaleY) * alpha;
		}
	}

	public class ColorTimeline : CurveTimeline {
		protected const int PREV_FRAME_TIME = -5;
		protected const int FRAME_R = 1;
		protected const int FRAME_G = 2;
		protected const int FRAME_B = 3;
		protected const int FRAME_A = 4;

		internal int slotIndex;
		internal float[] frames;

		public int SlotIndex { get { return slotIndex; } set { slotIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, r, g, b, a, ...

		public ColorTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * 5];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float r, float g, float b, float a) {
			frameIndex *= 5;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = r;
			frames[frameIndex + 2] = g;
			frames[frameIndex + 3] = b;
			frames[frameIndex + 4] = a;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			float r, g, b, a;
			if (time >= frames[frames.Length - 5]) {
				// Time is after last frame.
				int i = frames.Length - 1;
				r = frames[i - 3];
				g = frames[i - 2];
				b = frames[i - 1];
				a = frames[i];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frameIndex = Animation.binarySearch(frames, time, 5);
				float prevFrameR = frames[frameIndex - 4];
				float prevFrameG = frames[frameIndex - 3];
				float prevFrameB = frames[frameIndex - 2];
				float prevFrameA = frames[frameIndex - 1];
				float frameTime = frames[frameIndex];
				float percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime);
				percent = GetCurvePercent(frameIndex / 5 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

				r = prevFrameR + (frames[frameIndex + FRAME_R] - prevFrameR) * percent;
				g = prevFrameG + (frames[frameIndex + FRAME_G] - prevFrameG) * percent;
				b = prevFrameB + (frames[frameIndex + FRAME_B] - prevFrameB) * percent;
				a = prevFrameA + (frames[frameIndex + FRAME_A] - prevFrameA) * percent;
			}
			Slot slot = skeleton.slots.Items[slotIndex];
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
		public void SetFrame (int frameIndex, float time, String attachmentName) {
			frames[frameIndex] = time;
			attachmentNames[frameIndex] = attachmentName;
		}

		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (lastTime > time) Apply(skeleton, lastTime, int.MaxValue, null, 0);
				return;
			} else if (lastTime > time) //
				lastTime = -1;

			int frameIndex = (time >= frames[frames.Length - 1] ? frames.Length : Animation.binarySearch(frames, time)) - 1;
			if (frames[frameIndex] < lastTime) return;

			String attachmentName = attachmentNames[frameIndex];
			skeleton.slots.Items[slotIndex].Attachment =
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
		public void SetFrame (int frameIndex, float time, Event e) {
			frames[frameIndex] = time;
			events[frameIndex] = e;
		}

		/// <summary>Fires events for frames > lastTime and <= time.</summary>
		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			if (firedEvents == null) return;
			float[] frames = this.frames;
			int frameCount = frames.Length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				Apply(skeleton, lastTime, int.MaxValue, firedEvents, alpha);
				lastTime = -1f;
			} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return; // Time is before first frame.

			int frameIndex;
			if (lastTime < frames[0])
				frameIndex = 0;
			else {
				frameIndex = Animation.binarySearch(frames, lastTime);
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
		public void SetFrame (int frameIndex, float time, int[] drawOrder) {
			frames[frameIndex] = time;
			drawOrders[frameIndex] = drawOrder;
		}

		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			int frameIndex;
			if (time >= frames[frames.Length - 1]) // Time is after last frame.
				frameIndex = frames.Length - 1;
			else
				frameIndex = Animation.binarySearch(frames, time) - 1;

			ExposedList<Slot> drawOrder = skeleton.drawOrder;
			ExposedList<Slot> slots = skeleton.slots;
			int[] drawOrderToSetupIndex = drawOrders[frameIndex];
			if (drawOrderToSetupIndex == null) {
				drawOrder.Clear();
				for (int i = 0, n = slots.Count; i < n; i++)
					drawOrder.Add(slots.Items[i]);
			} else {
				for (int i = 0, n = drawOrderToSetupIndex.Length; i < n; i++)
					drawOrder.Items[i] = slots.Items[drawOrderToSetupIndex[i]];
			}
		}
	}

	public class FFDTimeline : CurveTimeline {
		internal int slotIndex;
		internal float[] frames;
		private float[][] frameVertices;
		internal Attachment attachment;

		public int SlotIndex { get { return slotIndex; } set { slotIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, ...
		public float[][] Vertices { get { return frameVertices; } set { frameVertices = value; } }
		public Attachment Attachment { get { return attachment; } set { attachment = value; } }

		public FFDTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount];
			frameVertices = new float[frameCount][];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float[] vertices) {
			frames[frameIndex] = time;
			frameVertices[frameIndex] = vertices;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (slot.attachment != attachment) return;

			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			float[][] frameVertices = this.frameVertices;
			int vertexCount = frameVertices[0].Length;

			float[] vertices = slot.attachmentVertices;
			if (vertices.Length < vertexCount) {
				vertices = new float[vertexCount];
				slot.attachmentVertices = vertices;
			}
			if (vertices.Length != vertexCount) alpha = 1; // Don't mix from uninitialized slot vertices.
			slot.attachmentVerticesCount = vertexCount;

			if (time >= frames[frames.Length - 1]) { // Time is after last frame.
				float[] lastVertices = frameVertices[frames.Length - 1];
				if (alpha < 1) {
					for (int i = 0; i < vertexCount; i++) {
						float vertex = vertices[i];
						vertices[i] = vertex + (lastVertices[i] - vertex) * alpha;
					}
				} else
					Array.Copy(lastVertices, 0, vertices, 0, vertexCount);
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time);
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex - 1] - frameTime);
			percent = GetCurvePercent(frameIndex - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			float[] prevVertices = frameVertices[frameIndex - 1];
			float[] nextVertices = frameVertices[frameIndex];

			if (alpha < 1) {
				for (int i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					float vertex = vertices[i];
					vertices[i] = vertex + (prev + (nextVertices[i] - prev) * percent - vertex) * alpha;
				}
			} else {
				for (int i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					vertices[i] = prev + (nextVertices[i] - prev) * percent;
				}
			}
		}
	}

	public class IkConstraintTimeline : CurveTimeline {
		private const int PREV_FRAME_TIME = -3;
		private const int PREV_FRAME_MIX = -2;
		private const int PREV_FRAME_BEND_DIRECTION = -1;
		private const int FRAME_MIX = 1;

		internal int ikConstraintIndex;
		internal float[] frames;

		public int IkConstraintIndex { get { return ikConstraintIndex; } set { ikConstraintIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, mix, bendDirection, ...

		public IkConstraintTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * 3];
		}

		/** Sets the time, mix and bend direction of the specified keyframe. */
		public void SetFrame (int frameIndex, float time, float mix, int bendDirection) {
			frameIndex *= 3;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = mix;
			frames[frameIndex + 2] = bendDirection;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			IkConstraint ikConstraint = skeleton.ikConstraints.Items[ikConstraintIndex];

			if (time >= frames[frames.Length - 3]) { // Time is after last frame.
				ikConstraint.mix += (frames[frames.Length - 2] - ikConstraint.mix) * alpha;
				ikConstraint.bendDirection = (int)frames[frames.Length - 1];
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frameIndex = Animation.binarySearch(frames, time, 3);
			float prevFrameMix = frames[frameIndex + PREV_FRAME_MIX];
			float frameTime = frames[frameIndex];
			float percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime);
			percent = GetCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

			float mix = prevFrameMix + (frames[frameIndex + FRAME_MIX] - prevFrameMix) * percent;
			ikConstraint.mix += (mix - ikConstraint.mix) * alpha;
			ikConstraint.bendDirection = (int)frames[frameIndex + PREV_FRAME_BEND_DIRECTION];
		}
	}

	public class FlipXTimeline : Timeline {
		internal int boneIndex;
		internal float[] frames;

		public int BoneIndex { get { return boneIndex; } set { boneIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, flip, ...
		public int FrameCount { get { return frames.Length >> 1; } }

		public FlipXTimeline (int frameCount) {
			frames = new float[frameCount << 1];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, bool flip) {
			frameIndex *= 2;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = flip ? 1 : 0;
		}

		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (lastTime > time) Apply(skeleton, lastTime, int.MaxValue, null, 0);
				return;
			} else if (lastTime > time) //
				lastTime = -1;

			int frameIndex = (time >= frames[frames.Length - 2] ? frames.Length : Animation.binarySearch(frames, time, 2)) - 2;
			if (frames[frameIndex] < lastTime) return;

			SetFlip(skeleton.bones.Items[boneIndex], frames[frameIndex + 1] != 0);
		}

		virtual protected void SetFlip (Bone bone, bool flip) {
			bone.flipX = flip;
		}
	}

	public class FlipYTimeline : FlipXTimeline {
		public FlipYTimeline (int frameCount)
			: base(frameCount) {
		}

		override protected void SetFlip (Bone bone, bool flip) {
			bone.flipY = flip;
		}
	}
}
