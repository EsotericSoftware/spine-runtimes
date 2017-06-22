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

using System;
using System.Collections.Generic;

namespace Spine {
	public class Animation {
		internal ExposedList<Timeline> timelines;
		internal float duration;
		internal String name;

		public string Name { get { return name; } }
		public ExposedList<Timeline> Timelines { get { return timelines; } set { timelines = value; } }
		public float Duration { get { return duration; } set { duration = value; } }

		public Animation (string name, ExposedList<Timeline> timelines, float duration) {
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
			if (timelines == null) throw new ArgumentNullException("timelines", "timelines cannot be null.");
			this.name = name;
			this.timelines = timelines;
			this.duration = duration;
		}

		/// <summary>Applies all the animation's timelines to the specified skeleton.</summary>
		/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixPose, MixDirection)"/>
		public void Apply (Skeleton skeleton, float lastTime, float time, bool loop, ExposedList<Event> events, float alpha, MixPose pose, MixDirection direction) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				if (lastTime > 0) lastTime %= duration;
			}

			ExposedList<Timeline> timelines = this.timelines;
			for (int i = 0, n = timelines.Count; i < n; i++)
				timelines.Items[i].Apply(skeleton, lastTime, time, events, alpha, pose, direction);
		}

		/// <param name="target">After the first and before the last entry.</param>
		internal static int BinarySearch (float[] values, float target, int step) {
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
		internal static int BinarySearch (float[] values, float target) {
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

		internal static int LinearSearch (float[] values, float target, int step) {
			for (int i = 0, last = values.Length - step; i <= last; i += step)
				if (values[i] > target) return i;
			return -1;
		}
	}

	public interface Timeline {
		/// <summary>Sets the value(s) for the specified time.</summary>
		/// <param name="skeleton">The skeleton the timeline is being applied to. This provides access to the bones, slots, and other skeleton components the timeline may change.</param>
		/// <param name="lastTime">lastTime The time this timeline was last applied. Timelines such as EventTimeline trigger only at specific times rather than every frame. In that case, the timeline triggers everything between lastTime (exclusive) and <code>time</code> (inclusive).</param>
		/// <param name="time">The time within the animation. Most timelines find the key before and the key after this time so they can interpolate between the keys.</param>
		/// <param name="events">If any events are fired, they are added to this list. Can be null to ignore firing events or if the timeline does not fire events. May be null.</param>
		/// <param name="alpha">alpha 0 applies the current or setup pose value (depending on pose parameter). 1 applies the timeline 
		/// 	value. Between 0 and 1 applies a value between the current or setup pose and the timeline value. By adjusting
		/// 	alpha over time, an animation can be mixed in or out. <code>alpha</code> can also be useful to
		/// 	 apply animations on top of each other (layered).</param>
		/// <param name="pose">Controls how mixing is applied when alpha is than 1.</param>
		/// <param name="direction">Indicates whether the timeline is mixing in or out. Used by timelines which perform instant transitions such as DrawOrderTimeline and AttachmentTimeline.</param>
		void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixPose pose, MixDirection direction);
		int PropertyId { get; }
	}

	/// <summary>
	/// Controls how a timeline is mixed with the setup or current pose.</summary>
	/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixPose, MixDirection)"/>
	public enum MixPose {
		/// <summary> The timeline value is mixed with the setup pose (the current pose is not used).</summary>
		Setup,
		/// <summary> The timeline value is mixed with the current pose. The setup pose is used as the timeline value before the first key,
		/// except for timelines which perform instant transitions, such as DrawOrderTimeline or AttachmentTimeline.</summary>
		Current,
		/// <summary> The timeline value is mixed with the current pose. No change is made before the first key (the current pose is kept until the first key).</summary>
		CurrentLayered
	}

	/// <summary>
	/// Indicates whether a timeline's <code>alpha</code> is mixing out over time toward 0 (the setup or current pose) or mixing in toward 1 (the timeline's pose).</summary>
	/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixPose, MixDirection)"/>
	public enum MixDirection {
		In,
		Out
	}

	internal enum TimelineType {
		Rotate = 0, Translate, Scale, Shear, //
		Attachment, Color, Deform, //
		Event, DrawOrder, //
		IkConstraint, TransformConstraint, //
		PathConstraintPosition, PathConstraintSpacing, PathConstraintMix, //
		TwoColor
	}

	/// <summary>Base class for frames that use an interpolation bezier curve.</summary>
	abstract public class CurveTimeline : Timeline {
		protected const float LINEAR = 0, STEPPED = 1, BEZIER = 2;
		protected const int BEZIER_SIZE = 10 * 2 - 1;

		private float[] curves; // type, x, y, ...
		public int FrameCount { get { return curves.Length / BEZIER_SIZE + 1; } }

		public CurveTimeline (int frameCount) {
			if (frameCount <= 0) throw new ArgumentException("frameCount must be > 0: " + frameCount, "frameCount");
			curves = new float[(frameCount - 1) * BEZIER_SIZE];
		}

		abstract public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction);

		abstract public int PropertyId { get; }

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
			float tmpx = (-cx1 * 2 + cx2) * 0.03f, tmpy = (-cy1 * 2 + cy2) * 0.03f;
			float dddfx = ((cx1 - cx2) * 3 + 1) * 0.006f, dddfy = ((cy1 - cy2) * 3 + 1) * 0.006f;
			float ddfx = tmpx * 2 + dddfx, ddfy = tmpy * 2 + dddfy;
			float dfx = cx1 * 0.3f + tmpx + dddfx * 0.16666667f, dfy = cy1 * 0.3f + tmpy + dddfy * 0.16666667f;

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
			percent = MathUtils.Clamp (percent, 0, 1);
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
		public const int ENTRIES = 2;
		internal const int PREV_TIME = -2, PREV_ROTATION = -1;
		internal const int ROTATION = 1;

		internal int boneIndex;
		internal float[] frames;

		public int BoneIndex { get { return boneIndex; } set { boneIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, angle, ...

		override public int PropertyId {
			get { return ((int)TimelineType.Rotate << 24) + boneIndex; }
		}

		public RotateTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount << 1];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float degrees) {
			frameIndex <<= 1;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATION] = degrees;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];

			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.Setup:
					bone.rotation = bone.data.rotation;
					return;
				case MixPose.Current:
					float rr = bone.data.rotation - bone.rotation;
					rr -= (16384 - (int)(16384.499999999996 - rr / 360)) * 360;
					bone.rotation += rr * alpha;
					return;
				}
				return;
			}

			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				if (pose == MixPose.Setup) {
					bone.rotation = bone.data.rotation + frames[frames.Length + PREV_ROTATION] * alpha;
				} else {
					float rr = bone.data.rotation + frames[frames.Length + PREV_ROTATION] - bone.rotation;
					rr -= (16384 - (int)(16384.499999999996 - rr / 360)) * 360; // Wrap within -180 and 180.
					bone.rotation += rr * alpha;
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frame = Animation.BinarySearch(frames, time, ENTRIES);
			float prevRotation = frames[frame + PREV_ROTATION];
			float frameTime = frames[frame];
			float percent = GetCurvePercent((frame >> 1) - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			float r = frames[frame + ROTATION] - prevRotation;
			r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
			r = prevRotation + r * percent;
			if (pose == MixPose.Setup) {
				r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
				bone.rotation = bone.data.rotation + r * alpha;
			} else {
				r = bone.data.rotation + r - bone.rotation;
				r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
				bone.rotation += r * alpha;
			}
		}
	}

	public class TranslateTimeline : CurveTimeline {
		public const int ENTRIES = 3;
		protected const int PREV_TIME = -3, PREV_X = -2, PREV_Y = -1;
		protected const int X = 1, Y = 2;

		internal int boneIndex;
		internal float[] frames;

		public int BoneIndex { get { return boneIndex; } set { boneIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, value, value, ...

		override public int PropertyId {
			get { return ((int)TimelineType.Translate << 24) + boneIndex; }
		}

		public TranslateTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float x, float y) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + X] = x;
			frames[frameIndex + Y] = y;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];

			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.Setup:
					bone.x = bone.data.x;
					bone.y = bone.data.y;
					return;
				case MixPose.Current:
					bone.x += (bone.data.x - bone.x) * alpha;
					bone.y += (bone.data.y - bone.y) * alpha;
					return;
				}
				return;
			}

			float x, y;
			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				x = frames[frames.Length + PREV_X];
				y = frames[frames.Length + PREV_Y];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, ENTRIES);
				x = frames[frame + PREV_X];
				y = frames[frame + PREV_Y];
				float frameTime = frames[frame];
				float percent = GetCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				x += (frames[frame + X] - x) * percent;
				y += (frames[frame + Y] - y) * percent;
			}
			if (pose == MixPose.Setup) {
				bone.x = bone.data.x + x * alpha;
				bone.y = bone.data.y + y * alpha;
			} else {
				bone.x += (bone.data.x + x - bone.x) * alpha;
				bone.y += (bone.data.y + y - bone.y) * alpha;
			}
		}
	}

	public class ScaleTimeline : TranslateTimeline {
		override public int PropertyId {
			get { return ((int)TimelineType.Scale << 24) + boneIndex; }
		}

		public ScaleTimeline (int frameCount)
			: base(frameCount) {
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];

			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.Setup:
					bone.scaleX = bone.data.scaleX;
					bone.scaleY = bone.data.scaleY;
					return;
				case MixPose.Current:
					bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
					return;
				}
				return;
			}

			float x, y;
			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				x = frames[frames.Length + PREV_X] * bone.data.scaleX;
				y = frames[frames.Length + PREV_Y] * bone.data.scaleY;
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, ENTRIES);
				x = frames[frame + PREV_X];
				y = frames[frame + PREV_Y];
				float frameTime = frames[frame];
				float percent = GetCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				x = (x + (frames[frame + X] - x) * percent) * bone.data.scaleX;
				y = (y + (frames[frame + Y] - y) * percent) * bone.data.scaleY;
			}
			if (alpha == 1) {
				bone.scaleX = x;
				bone.scaleY = y;
			} else {
				float bx, by;
				if (pose == MixPose.Setup) {
					bx = bone.data.scaleX;
					by = bone.data.scaleY;
				} else {
					bx = bone.scaleX;
					by = bone.scaleY;
				}
				// Mixing out uses sign of setup or current pose, else use sign of key.
				if (direction == MixDirection.Out) {
					x = Math.Abs(x) * Math.Sign(bx);
					y = Math.Abs(y) * Math.Sign(by);
				} else {
					bx = Math.Abs(bx) * Math.Sign(x);
					by = Math.Abs(by) * Math.Sign(y);
				}
				bone.scaleX = bx + (x - bx) * alpha;
				bone.scaleY = by + (y - by) * alpha;
			}
		}
	}

	public class ShearTimeline : TranslateTimeline {
		override public int PropertyId {
			get { return ((int)TimelineType.Shear << 24) + boneIndex; }
		}

		public ShearTimeline (int frameCount)
			: base(frameCount) {
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.Setup:
					bone.shearX = bone.data.shearX;
					bone.shearY = bone.data.shearY;
					return;
				case MixPose.Current:
					bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
					bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
					return;
				}
				return;
			}

			float x, y;
			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				x = frames[frames.Length + PREV_X];
				y = frames[frames.Length + PREV_Y];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, ENTRIES);
				x = frames[frame + PREV_X];
				y = frames[frame + PREV_Y];
				float frameTime = frames[frame];
				float percent = GetCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				x = x + (frames[frame + X] - x) * percent;
				y = y + (frames[frame + Y] - y) * percent;
			}
			if (pose == MixPose.Setup) {
				bone.shearX = bone.data.shearX + x * alpha;
				bone.shearY = bone.data.shearY + y * alpha;
			} else {
				bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
				bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
			}
		}
	}

	public class ColorTimeline : CurveTimeline {
		public const int ENTRIES = 5;
		protected const int PREV_TIME = -5, PREV_R = -4, PREV_G = -3, PREV_B = -2, PREV_A = -1;
		protected const int R = 1, G = 2, B = 3, A = 4;

		internal int slotIndex;
		internal float[] frames;

		public int SlotIndex { get { return slotIndex; } set { slotIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, r, g, b, a, ...

		override public int PropertyId {
			get { return ((int)TimelineType.Color << 24) + slotIndex; }
		}

		public ColorTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float r, float g, float b, float a) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + R] = r;
			frames[frameIndex + G] = g;
			frames[frameIndex + B] = b;
			frames[frameIndex + A] = a;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			float[] frames = this.frames;
			if (time < frames[0]) {
				var slotData = slot.data;
				switch (pose) {
				case MixPose.Setup:
					slot.r = slotData.r;
					slot.g = slotData.g;
					slot.b = slotData.b;
					slot.a = slotData.a;
					return;
				case MixPose.Current:
					slot.r += (slot.r - slotData.r) * alpha;
					slot.g += (slot.g - slotData.g) * alpha;
					slot.b += (slot.b - slotData.b) * alpha;
					slot.a += (slot.a - slotData.a) * alpha;
					return;
				}
				return;
			}

			float r, g, b, a;
			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				int i = frames.Length;
				r = frames[i + PREV_R];
				g = frames[i + PREV_G];
				b = frames[i + PREV_B];
				a = frames[i + PREV_A];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, ENTRIES);
				r = frames[frame + PREV_R];
				g = frames[frame + PREV_G];
				b = frames[frame + PREV_B];
				a = frames[frame + PREV_A];
				float frameTime = frames[frame];
				float percent = GetCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				r += (frames[frame + R] - r) * percent;
				g += (frames[frame + G] - g) * percent;
				b += (frames[frame + B] - b) * percent;
				a += (frames[frame + A] - a) * percent;
			}
			if (alpha == 1) {
				slot.r = r;
				slot.g = g;
				slot.b = b;
				slot.a = a;
			} else {
				float br, bg, bb, ba;
				if (pose == MixPose.Setup) {
					br = slot.data.r;
					bg = slot.data.g;
					bb = slot.data.b;
					ba = slot.data.a;
				} else {
					br = slot.r;
					bg = slot.g;
					bb = slot.b;
					ba = slot.a;
				}
				slot.r = br + ((r - br) * alpha);
				slot.g = bg + ((g - bg) * alpha);
				slot.b = bb + ((b - bb) * alpha);
				slot.a = ba + ((a - ba) * alpha);
			}
		}
	}

	public class TwoColorTimeline : CurveTimeline {
		public const int ENTRIES = 8;
		protected const int PREV_TIME = -8, PREV_R = -7, PREV_G = -6, PREV_B = -5, PREV_A = -4;
		protected const int PREV_R2 = -3, PREV_G2 = -2, PREV_B2 = -1;
		protected const int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;

		internal float[] frames; // time, r, g, b, a, r2, g2, b2, ...
		public float[] Frames { get { return frames; } }

		internal int slotIndex;
		public int SlotIndex {
			get { return slotIndex; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				slotIndex = value;
			}
		}

		override public int PropertyId {
			get { return ((int)TimelineType.TwoColor << 24) + slotIndex; }
		}

		public TwoColorTimeline (int frameCount) :
			base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float r, float g, float b, float a, float r2, float g2, float b2) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + R] = r;
			frames[frameIndex + G] = g;
			frames[frameIndex + B] = b;
			frames[frameIndex + A] = a;
			frames[frameIndex + R2] = r2;
			frames[frameIndex + G2] = g2;
			frames[frameIndex + B2] = b2;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				var slotData = slot.data;
				switch (pose) {
				case MixPose.Setup:
					//	slot.color.set(slot.data.color);
					//	slot.darkColor.set(slot.data.darkColor);
					slot.r = slotData.r;
					slot.g = slotData.g;
					slot.b = slotData.b;
					slot.a = slotData.a;
					slot.r2 = slotData.r2;
					slot.g2 = slotData.g2;
					slot.b2 = slotData.b2;
					return;
				case MixPose.Current:
					slot.r += (slot.r - slotData.r) * alpha;
					slot.g += (slot.g - slotData.g) * alpha;
					slot.b += (slot.b - slotData.b) * alpha;
					slot.a += (slot.a - slotData.a) * alpha;
					slot.r2 += (slot.r2 - slotData.r2) * alpha;
					slot.g2 += (slot.g2 - slotData.g2) * alpha;
					slot.b2 += (slot.b2 - slotData.b2) * alpha;
					return;
				}
				return;
			}

			float r, g, b, a, r2, g2, b2;
			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				int i = frames.Length;
				r = frames[i + PREV_R];
				g = frames[i + PREV_G];
				b = frames[i + PREV_B];
				a = frames[i + PREV_A];
				r2 = frames[i + PREV_R2];
				g2 = frames[i + PREV_G2];
				b2 = frames[i + PREV_B2];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, ENTRIES);
				r = frames[frame + PREV_R];
				g = frames[frame + PREV_G];
				b = frames[frame + PREV_B];
				a = frames[frame + PREV_A];
				r2 = frames[frame + PREV_R2];
				g2 = frames[frame + PREV_G2];
				b2 = frames[frame + PREV_B2];
				float frameTime = frames[frame];
				float percent = GetCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				r += (frames[frame + R] - r) * percent;
				g += (frames[frame + G] - g) * percent;
				b += (frames[frame + B] - b) * percent;
				a += (frames[frame + A] - a) * percent;
				r2 += (frames[frame + R2] - r2) * percent;
				g2 += (frames[frame + G2] - g2) * percent;
				b2 += (frames[frame + B2] - b2) * percent;
			}
			if (alpha == 1) {
				slot.r = r;
				slot.g = g;
				slot.b = b;
				slot.a = a;
				slot.r2 = r2;
				slot.g2 = g2;
				slot.b2 = b2;
			} else {
				float br, bg, bb, ba, br2, bg2, bb2;
				if (pose == MixPose.Setup) {
					br = slot.data.r;
					bg = slot.data.g;
					bb = slot.data.b;
					ba = slot.data.a;
					br2 = slot.data.r2;
					bg2 = slot.data.g2;
					bb2 = slot.data.b2;
				} else {
					br = slot.r;
					bg = slot.g;
					bb = slot.b;
					ba = slot.a;
					br2 = slot.r2;
					bg2 = slot.g2;
					bb2 = slot.b2;
				}
				slot.r = br + ((r - br) * alpha);
				slot.g = bg + ((g - bg) * alpha);
				slot.b = bb + ((b - bb) * alpha);
				slot.a = ba + ((a - ba) * alpha);
				slot.r2 = br2 + ((r2 - br2) * alpha);
				slot.g2 = bg2 + ((g2 - bg2) * alpha);
				slot.b2 = bb2 + ((b2 - bb2) * alpha);
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

		public int PropertyId {
			get { return ((int)TimelineType.Attachment << 24) + slotIndex; }
		}

		public AttachmentTimeline (int frameCount) {
			frames = new float[frameCount];
			attachmentNames = new String[frameCount];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, String attachmentName) {
			frames[frameIndex] = time;
			attachmentNames[frameIndex] = attachmentName;
		}

		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			string attachmentName;
			Slot slot = skeleton.slots.Items[slotIndex];
			if (direction == MixDirection.Out && pose == MixPose.Setup) {
				attachmentName = slot.data.attachmentName;
				slot.Attachment = attachmentName == null ? null : skeleton.GetAttachment(slotIndex, attachmentName);
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (pose == MixPose.Setup) {
					attachmentName = slot.data.attachmentName;
					slot.Attachment = attachmentName == null ? null : skeleton.GetAttachment(slotIndex, attachmentName);
				}
				return;
			}

			int frameIndex;
			if (time >= frames[frames.Length - 1]) // Time is after last frame.
				frameIndex = frames.Length - 1;
			else
				frameIndex = Animation.BinarySearch(frames, time, 1) - 1;

			attachmentName = attachmentNames[frameIndex];
			slot.Attachment = attachmentName == null ? null : skeleton.GetAttachment(slotIndex, attachmentName);
		}
	}

	public class DeformTimeline : CurveTimeline {
		static float[] zeros = new float[64];

		internal int slotIndex;
		internal float[] frames;
		internal float[][] frameVertices;
		internal VertexAttachment attachment;

		public int SlotIndex { get { return slotIndex; } set { slotIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, ...
		public float[][] Vertices { get { return frameVertices; } set { frameVertices = value; } }
		public VertexAttachment Attachment { get { return attachment; } set { attachment = value; } }

		override public int PropertyId {
			get { return ((int)TimelineType.Deform << 24) + attachment.id + slotIndex; }
		}

		public DeformTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount];
			frameVertices = new float[frameCount][];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float[] vertices) {
			frames[frameIndex] = time;
			frameVertices[frameIndex] = vertices;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			VertexAttachment vertexAttachment = slot.attachment as VertexAttachment;
			if (vertexAttachment == null || !vertexAttachment.ApplyDeform(attachment)) return;

			var verticesArray = slot.attachmentVertices;
			float[][] frameVertices = this.frameVertices;
			int vertexCount = frameVertices[0].Length;
			if (verticesArray.Capacity < vertexCount) verticesArray.Capacity = vertexCount;	// verticesArray.SetSize(vertexCount) // Ensure size and preemptively set count.
			verticesArray.Count = vertexCount;
			float[] vertices = verticesArray.Items;

			float[] frames = this.frames;
			if (time < frames[0]) {
				
				switch (pose) {
				case MixPose.Setup:
					float[] zeroVertices;
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions (setup pose).
						zeroVertices = vertexAttachment.vertices;
					} else {
						// Weighted deform offsets (zeros).
						zeroVertices = DeformTimeline.zeros;
						if (zeroVertices.Length < vertexCount) DeformTimeline.zeros = zeroVertices = new float[vertexCount];
					}
					Array.Copy(zeroVertices, 0, vertices, 0, vertexCount);
					return;
				case MixPose.Current:
					if (alpha == 1) return;
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++)
							vertices[i] += (setupVertices[i] - vertices[i]) * alpha;
					} else {
						// Weighted deform offsets.
						alpha = 1 - alpha;
						for (int i = 0; i < vertexCount; i++)
							vertices[i] *= alpha;
					}
					return;
				default:
					return;
				}

			}

			if (time >= frames[frames.Length - 1]) { // Time is after last frame.
				float[] lastVertices = frameVertices[frames.Length - 1];
				if (alpha == 1) {
					// Vertex positions or deform offsets, no alpha.
					Array.Copy(lastVertices, 0, vertices, 0, vertexCount);
				} else if (pose == MixPose.Setup) {
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++) {
							float setup = setupVertices[i];
							vertices[i] = setup + (lastVertices[i] - setup) * alpha;
						}
					} else {
						// Weighted deform offsets, with alpha.
						for (int i = 0; i < vertexCount; i++)
							vertices[i] = lastVertices[i] * alpha;
					}
				} else {
					// Vertex positions or deform offsets, with alpha.
					for (int i = 0; i < vertexCount; i++)
						vertices[i] += (lastVertices[i] - vertices[i]) * alpha;
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frame = Animation.BinarySearch(frames, time);
			float[] prevVertices = frameVertices[frame - 1];
			float[] nextVertices = frameVertices[frame];
			float frameTime = frames[frame];
			float percent = GetCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime));

			if (alpha == 1) {
				// Vertex positions or deform offsets, no alpha.
				for (int i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					vertices[i] = prev + (nextVertices[i] - prev) * percent;
				}
			} else if (pose == MixPose.Setup) {
				if (vertexAttachment.bones == null) {
					// Unweighted vertex positions, with alpha.
					var setupVertices = vertexAttachment.vertices;
					for (int i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i], setup = setupVertices[i];
						vertices[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
					}
				} else {
					// Weighted deform offsets, with alpha.
					for (int i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						vertices[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
					}
				}
			} else {
				// Vertex positions or deform offsets, with alpha.
				for (int i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					vertices[i] += (prev + (nextVertices[i] - prev) * percent - vertices[i]) * alpha;
				}
			}
		}
	}

	public class EventTimeline : Timeline {
		internal float[] frames;
		private Event[] events;

		public float[] Frames { get { return frames; } set { frames = value; } } // time, ...
		public Event[] Events { get { return events; } set { events = value; } }
		public int FrameCount { get { return frames.Length; } }

		public int PropertyId {
			get { return ((int)TimelineType.Event << 24); }
		}

		public EventTimeline (int frameCount) {
			frames = new float[frameCount];
			events = new Event[frameCount];
		}

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, Event e) {
			frames[frameIndex] = e.Time;
			events[frameIndex] = e;
		}

		/// <summary>Fires events for frames &gt; lastTime and &lt;= time.</summary>
		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			if (firedEvents == null) return;
			float[] frames = this.frames;
			int frameCount = frames.Length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				Apply(skeleton, lastTime, int.MaxValue, firedEvents, alpha, pose, direction);
				lastTime = -1f;
			} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return; // Time is before first frame.

			int frame;
			if (lastTime < frames[0])
				frame = 0;
			else {
				frame = Animation.BinarySearch(frames, lastTime);
				float frameTime = frames[frame];
				while (frame > 0) { // Fire multiple events with the same frame.
					if (frames[frame - 1] != frameTime) break;
					frame--;
				}
			}
			for (; frame < frameCount && time >= frames[frame]; frame++)
				firedEvents.Add(events[frame]);
		}
	}

	public class DrawOrderTimeline : Timeline {
		internal float[] frames;
		private int[][] drawOrders;

		public float[] Frames { get { return frames; } set { frames = value; } } // time, ...
		public int[][] DrawOrders { get { return drawOrders; } set { drawOrders = value; } }
		public int FrameCount { get { return frames.Length; } }

		public int PropertyId {
			get { return ((int)TimelineType.DrawOrder << 24); }
		}

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

		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			ExposedList<Slot> drawOrder = skeleton.drawOrder;
			ExposedList<Slot> slots = skeleton.slots;
			if (direction == MixDirection.Out && pose == MixPose.Setup) {
				Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (pose == MixPose.Setup) Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
				return;
			}

			int frame;
			if (time >= frames[frames.Length - 1]) // Time is after last frame.
				frame = frames.Length - 1;
			else
				frame = Animation.BinarySearch(frames, time) - 1;
			
			int[] drawOrderToSetupIndex = drawOrders[frame];
			if (drawOrderToSetupIndex == null) {
				drawOrder.Clear();
				for (int i = 0, n = slots.Count; i < n; i++)
					drawOrder.Add(slots.Items[i]);
			} else {
				var drawOrderItems = drawOrder.Items;
				var slotsItems = slots.Items;
				for (int i = 0, n = drawOrderToSetupIndex.Length; i < n; i++)
					drawOrderItems[i] = slotsItems[drawOrderToSetupIndex[i]];
			}
		}
	}

	public class IkConstraintTimeline : CurveTimeline {
		public const int ENTRIES = 3;
		private const int PREV_TIME = -3, PREV_MIX = -2, PREV_BEND_DIRECTION = -1;
		private const int MIX = 1, BEND_DIRECTION = 2;

		internal int ikConstraintIndex;
		internal float[] frames;

		public int IkConstraintIndex { get { return ikConstraintIndex; } set { ikConstraintIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, mix, bendDirection, ...

		override public int PropertyId {
			get { return ((int)TimelineType.IkConstraint << 24) + ikConstraintIndex; }
		}

		public IkConstraintTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}
			
		/// <summary>Sets the time, mix and bend direction of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float mix, int bendDirection) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + MIX] = mix;
			frames[frameIndex + BEND_DIRECTION] = bendDirection;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			IkConstraint constraint = skeleton.ikConstraints.Items[ikConstraintIndex];
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.Setup:
					constraint.mix = constraint.data.mix;
					constraint.bendDirection = constraint.data.bendDirection;
					return;
				case MixPose.Current:
					constraint.mix += (constraint.data.mix - constraint.mix) * alpha;
					constraint.bendDirection = constraint.data.bendDirection;
					return;
				}
				return;
			}

			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				if (pose == MixPose.Setup) {
					constraint.mix = constraint.data.mix + (frames[frames.Length + PREV_MIX] - constraint.data.mix) * alpha;
					constraint.bendDirection = direction == MixDirection.Out ? constraint.data.bendDirection
						: (int)frames[frames.Length + PREV_BEND_DIRECTION];
				} else {
					constraint.mix += (frames[frames.Length + PREV_MIX] - constraint.mix) * alpha;
					if (direction == MixDirection.In) constraint.bendDirection = (int)frames[frames.Length + PREV_BEND_DIRECTION];
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frame = Animation.BinarySearch(frames, time, ENTRIES);
			float mix = frames[frame + PREV_MIX];
			float frameTime = frames[frame];
			float percent = GetCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			if (pose == MixPose.Setup) {
				constraint.mix = constraint.data.mix + (mix + (frames[frame + MIX] - mix) * percent - constraint.data.mix) * alpha;
				constraint.bendDirection = direction == MixDirection.Out ? constraint.data.bendDirection : (int)frames[frame + PREV_BEND_DIRECTION];
			} else {
				constraint.mix += (mix + (frames[frame + MIX] - mix) * percent - constraint.mix) * alpha;
				if (direction == MixDirection.In) constraint.bendDirection = (int)frames[frame + PREV_BEND_DIRECTION];
			}
		}
	}

	public class TransformConstraintTimeline : CurveTimeline {
		public const int ENTRIES = 5;
		private const int PREV_TIME = -5, PREV_ROTATE = -4, PREV_TRANSLATE = -3, PREV_SCALE = -2, PREV_SHEAR = -1;
		private const int ROTATE = 1, TRANSLATE = 2, SCALE = 3, SHEAR = 4;

		internal int transformConstraintIndex;
		internal float[] frames;

		public int TransformConstraintIndex { get { return transformConstraintIndex; } set { transformConstraintIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, rotate mix, translate mix, scale mix, shear mix, ...

		override public int PropertyId {
			get { return ((int)TimelineType.TransformConstraint << 24) + transformConstraintIndex; }
		}

		public TransformConstraintTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		public void SetFrame (int frameIndex, float time, float rotateMix, float translateMix, float scaleMix, float shearMix) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATE] = rotateMix;
			frames[frameIndex + TRANSLATE] = translateMix;
			frames[frameIndex + SCALE] = scaleMix;
			frames[frameIndex + SHEAR] = shearMix;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			TransformConstraint constraint = skeleton.transformConstraints.Items[transformConstraintIndex];
			float[] frames = this.frames;
			if (time < frames[0]) {
				var data = constraint.data;
				switch (pose) {
				case MixPose.Setup:
					constraint.rotateMix = data.rotateMix;
					constraint.translateMix = data.translateMix;
					constraint.scaleMix = data.scaleMix;
					constraint.shearMix = data.shearMix;
					return;
				case MixPose.Current:
					constraint.rotateMix += (data.rotateMix - constraint.rotateMix) * alpha;
					constraint.translateMix += (data.translateMix - constraint.translateMix) * alpha;
					constraint.scaleMix += (data.scaleMix - constraint.scaleMix) * alpha;
					constraint.shearMix += (data.shearMix - constraint.shearMix) * alpha;
					return;
				}
				return;
			}

			float rotate, translate, scale, shear;
			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				int i = frames.Length;
				rotate = frames[i + PREV_ROTATE];
				translate = frames[i + PREV_TRANSLATE];
				scale = frames[i + PREV_SCALE];
				shear = frames[i + PREV_SHEAR];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, ENTRIES);
				rotate = frames[frame + PREV_ROTATE];
				translate = frames[frame + PREV_TRANSLATE];
				scale = frames[frame + PREV_SCALE];
				shear = frames[frame + PREV_SHEAR];
				float frameTime = frames[frame];
				float percent = GetCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				rotate += (frames[frame + ROTATE] - rotate) * percent;
				translate += (frames[frame + TRANSLATE] - translate) * percent;
				scale += (frames[frame + SCALE] - scale) * percent;
				shear += (frames[frame + SHEAR] - shear) * percent;
			}
			if (pose == MixPose.Setup) {
				TransformConstraintData data = constraint.data;
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

	public class PathConstraintPositionTimeline : CurveTimeline {
		public const int ENTRIES = 2;
		protected const int PREV_TIME = -2, PREV_VALUE = -1;
		protected const int VALUE = 1;

		internal int pathConstraintIndex;
		internal float[] frames;

		override public int PropertyId {
			get { return ((int)TimelineType.PathConstraintPosition << 24) + pathConstraintIndex; }
		}

		public PathConstraintPositionTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		public int PathConstraintIndex { get { return pathConstraintIndex; } set { pathConstraintIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, position, ...

		/// <summary>Sets the time and value of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float value) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + VALUE] = value;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.Setup:
					constraint.position = constraint.data.position;
					return;
				case MixPose.Current:
					constraint.position += (constraint.data.position - constraint.position) * alpha;
					return;
				}
				return;
			}

			float position;
			if (time >= frames[frames.Length - ENTRIES]) // Time is after last frame.
				position = frames[frames.Length + PREV_VALUE];
			else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, ENTRIES);
				position = frames[frame + PREV_VALUE];
				float frameTime = frames[frame];
				float percent = GetCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				position += (frames[frame + VALUE] - position) * percent;
			}
			if (pose == MixPose.Setup)
				constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
			else
				constraint.position += (position - constraint.position) * alpha;
		}
	}

	public class PathConstraintSpacingTimeline : PathConstraintPositionTimeline {
		override public int PropertyId {
			get { return ((int)TimelineType.PathConstraintSpacing << 24) + pathConstraintIndex; }
		}

		public PathConstraintSpacingTimeline (int frameCount)
			: base(frameCount) {
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.Setup:
					constraint.spacing = constraint.data.spacing;
					return;
				case MixPose.Current:
					constraint.spacing += (constraint.data.spacing - constraint.spacing) * alpha;
					return;
				}
				return;
			}

			float spacing;
			if (time >= frames[frames.Length - ENTRIES]) // Time is after last frame.
				spacing = frames[frames.Length + PREV_VALUE];
			else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, ENTRIES);
				spacing = frames[frame + PREV_VALUE];
				float frameTime = frames[frame];
				float percent = GetCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				spacing += (frames[frame + VALUE] - spacing) * percent;
			}

			if (pose == MixPose.Setup)
				constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
			else
				constraint.spacing += (spacing - constraint.spacing) * alpha;
		}
	}

	public class PathConstraintMixTimeline : CurveTimeline {
		public const int ENTRIES = 3;
		private const int PREV_TIME = -3, PREV_ROTATE = -2, PREV_TRANSLATE = -1;
		private const int ROTATE = 1, TRANSLATE = 2;

		internal int pathConstraintIndex;
		internal float[] frames;

		public int PathConstraintIndex { get { return pathConstraintIndex; } set { pathConstraintIndex = value; } }
		public float[] Frames { get { return frames; } set { frames = value; } } // time, rotate mix, translate mix, ...

		override public int PropertyId {
			get { return ((int)TimelineType.PathConstraintMix << 24) + pathConstraintIndex; }
		}

		public PathConstraintMixTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}			

		/// <summary>Sets the time and mixes of the specified keyframe.</summary>
		public void SetFrame (int frameIndex, float time, float rotateMix, float translateMix) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATE] = rotateMix;
			frames[frameIndex + TRANSLATE] = translateMix;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
			PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.Setup:
					constraint.rotateMix = constraint.data.rotateMix;
					constraint.translateMix = constraint.data.translateMix;
					return;
				case MixPose.Current:
					constraint.rotateMix += (constraint.data.rotateMix - constraint.rotateMix) * alpha;
					constraint.translateMix += (constraint.data.translateMix - constraint.translateMix) * alpha;
					return;
				}
				return;
			}

			float rotate, translate;
			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				rotate = frames[frames.Length + PREV_ROTATE];
				translate = frames[frames.Length + PREV_TRANSLATE];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, ENTRIES);
				rotate = frames[frame + PREV_ROTATE];
				translate = frames[frame + PREV_TRANSLATE];
				float frameTime = frames[frame];
				float percent = GetCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				rotate += (frames[frame + ROTATE] - rotate) * percent;
				translate += (frames[frame + TRANSLATE] - translate) * percent;
			}

			if (pose == MixPose.Setup) {
				constraint.rotateMix = constraint.data.rotateMix + (rotate - constraint.data.rotateMix) * alpha;
				constraint.translateMix = constraint.data.translateMix + (translate - constraint.data.translateMix) * alpha;
			} else {
				constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
				constraint.translateMix += (translate - constraint.translateMix) * alpha;
			}
		}
	}
}
