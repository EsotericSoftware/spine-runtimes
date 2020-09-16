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

using System;
using System.Collections.Generic;

namespace Spine {

	/// <summary>
	/// A simple container for a list of timelines and a name.</summary>
	public class Animation {
		internal String name;
		internal ExposedList<Timeline> timelines;
		internal HashSet<int> timelineIds;
		internal float duration;

		public Animation (string name, ExposedList<Timeline> timelines, float duration) {
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
			if (timelines == null) throw new ArgumentNullException("timelines", "timelines cannot be null.");
			// Note: avoiding reallocations by adding all hash set entries at
			// once (EnsureCapacity() is only available in newer .Net versions).
			int[] propertyIDs = new int[timelines.Count];
			for (int i = 0; i < timelines.Count; ++i) {
				propertyIDs[i] = timelines.Items[i].PropertyId;
			}
			this.timelineIds = new HashSet<int>(propertyIDs);
			this.name = name;
			this.timelines = timelines;
			this.duration = duration;
		}

		public ExposedList<Timeline> Timelines { get { return timelines; } set { timelines = value; } }

		/// <summary>The duration of the animation in seconds, which is the highest time of all keys in the timeline.</summary>
		public float Duration { get { return duration; } set { duration = value; } }

		/// <summary>The animation's name, which is unique across all animations in the skeleton.</summary>
		public string Name { get { return name; } }

		/// <summary>Whether the timeline with the property id is contained in this animation.</summary>
		public bool HasTimeline (int id) {
			return timelineIds.Contains(id);
		}

		/// <summary>Applies all the animation's timelines to the specified skeleton.</summary>
		/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixBlend, MixDirection)"/>
		public void Apply (Skeleton skeleton, float lastTime, float time, bool loop, ExposedList<Event> events, float alpha, MixBlend blend,
							MixDirection direction) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				if (lastTime > 0) lastTime %= duration;
			}

			ExposedList<Timeline> timelines = this.timelines;
			for (int i = 0, n = timelines.Count; i < n; i++)
				timelines.Items[i].Apply(skeleton, lastTime, time, events, alpha, blend, direction);
		}

		override public string ToString () {
			return name;
		}

		/// <param name="target">After the first and before the last entry.</param>
		/// <returns>Index of first value greater than the target.</returns>
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
				if (values[current + 1] <= target)
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

	/// <summary>
	/// The interface for all timelines.</summary>
	public interface Timeline {
		/// <summary>Applies this timeline to the skeleton.</summary>
		/// <param name="skeleton">The skeleton the timeline is being applied to. This provides access to the bones, slots, and other
		///                   skeleton components the timeline may change.</param>
		///  <param name="lastTime"> The time this timeline was last applied. Timelines such as <see cref="EventTimeline"/> trigger only at specific
		///                   times rather than every frame. In that case, the timeline triggers everything between <code>lastTime</code>
		///                   (exclusive) and <code>time</code> (inclusive).</param>
		///  <param name="time"> The time within the animation. Most timelines find the key before and the key after this time so they can
		///                   interpolate between the keys.</param>
		///  <param name="events"> If any events are fired, they are added to this list. Can be null to ignore firing events or if the
		///                   timeline does not fire events.</param>
		///  <param name="alpha"> 0 applies the current or setup value (depending on <code>blend</code>). 1 applies the timeline value.
		///                   Between 0 and 1 applies a value between the current or setup value and the timeline value. By adjusting
		///                   <code>alpha</code> over time, an animation can be mixed in or out. <code>alpha</code> can also be useful to
		///                   apply animations on top of each other (layered).</param>
		///  <param name="blend"> Controls how mixing is applied when <code>alpha</code> < 1.</param>
		///  <param name="direction"> Indicates whether the timeline is mixing in or out. Used by timelines which perform instant transitions,
		///                   such as <see cref="DrawOrderTimeline"/> or <see cref="AttachmentTimeline"/>, and other such as {@link ScaleTimeline}.</param>
		void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend, MixDirection direction);
		/// <summary>Uniquely encodes both the type of this timeline and the skeleton property that it affects.</summary>
		int PropertyId { get; }
	}

	/// <summary>
	/// Controls how a timeline is mixed with the setup or current pose.</summary>
	/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixBlend, MixDirection)"/>
	public enum MixBlend {

		/// <summary> Transitions from the setup value to the timeline value (the current value is not used). Before the first key, the setup
		///           value is set.</summary>
		Setup,

		/// <summary>
		/// <para>
		/// Transitions from the current value to the timeline value. Before the first key, transitions from the current value to
		/// the setup value. Timelines which perform instant transitions, such as <see cref="DrawOrderTimeline"/> or
		/// <see cref="AttachmentTimeline"/>, use the setup value before the first key.</para>
		/// <para>
		/// <code>First</code> is intended for the first animations applied, not for animations layered on top of those.</para>
		/// </summary>
		First,

		/// <summary>
		/// <para>
		/// Transitions from the current value to the timeline value. No change is made before the first key (the current value is
		/// kept until the first key).</para>
		/// <para>
		/// <code>Replace</code> is intended for animations layered on top of others, not for the first animations applied.</para>
		/// </summary>
		Replace,

		/// <summary>
		/// <para>
		/// Transitions from the current value to the current value plus the timeline value. No change is made before the first key
		/// (the current value is kept until the first key).</para>
		/// <para>
		/// <code>Add</code> is intended for animations layered on top of others, not for the first animations applied.</para>
		/// </summary>
		Add
	}

	/// <summary>
	/// Indicates whether a timeline's <code>alpha</code> is mixing out over time toward 0 (the setup or current pose value) or
	/// mixing in toward 1 (the timeline's value).</summary>
	/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixBlend, MixDirection)"/>
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

	/// <summary>An interface for timelines which change the property of a bone.</summary>
	public interface IBoneTimeline {
		/// <summary>The index of the bone in <see cref="Skeleton.Bones"/> that will be changed.</summary>
		int BoneIndex { get; }
	}

	/// <summary>An interface for timelines which change the property of a slot.</summary>
	public interface ISlotTimeline {
		/// <summary>The index of the slot in <see cref="Skeleton.Slots"/> that will be changed.</summary>
		int SlotIndex { get; }
	}

	/// <summary>The base class for timelines that use interpolation between key frame values.</summary>
	abstract public class CurveTimeline : Timeline {
		protected const float LINEAR = 0, STEPPED = 1, BEZIER = 2;
		protected const int BEZIER_SIZE = 10 * 2 - 1;

		internal float[] curves; // type, x, y, ...
		/// <summary>The number of key frames for this timeline.</summary>
		public int FrameCount { get { return curves.Length / BEZIER_SIZE + 1; } }

		public CurveTimeline (int frameCount) {
			if (frameCount <= 0) throw new ArgumentOutOfRangeException("frameCount must be > 0: ");
			curves = new float[(frameCount - 1) * BEZIER_SIZE];
		}

		abstract public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend, MixDirection direction);

		abstract public int PropertyId { get; }

		/// <summary>Sets the specified key frame to linear interpolation.</summary>
		public void SetLinear (int frameIndex) {
			curves[frameIndex * BEZIER_SIZE] = LINEAR;
		}

		/// <summary>Sets the specified key frame to stepped interpolation.</summary>
		public void SetStepped (int frameIndex) {
			curves[frameIndex * BEZIER_SIZE] = STEPPED;
		}

		/// <summary>Returns the interpolation type for the specified key frame.</summary>
		/// <returns>Linear is 0, stepped is 1, Bezier is 2.</returns>
		public float GetCurveType (int frameIndex) {
			int index = frameIndex * BEZIER_SIZE;
			if (index == curves.Length) return LINEAR;
			float type = curves[index];
			if (type == LINEAR) return LINEAR;
			if (type == STEPPED) return STEPPED;
			return BEZIER;
		}

		/// <summary>Sets the specified key frame to Bezier interpolation. <code>cx1</code> and <code>cx2</code> are from 0 to 1,
		/// representing the percent of time between the two key frames. <code>cy1</code> and <code>cy2</code> are the percent of the
		/// difference between the key frame's values.</summary>
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

		/// <summary>Returns the interpolated percentage for the specified key frame and linear percentage.</summary>
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
					if (i == start) return curves[i + 1] * percent / x; // First point is 0,0.
					float prevX = curves[i - 2], prevY = curves[i - 1];
					return prevY + (curves[i + 1] - prevY) * (percent - prevX) / (x - prevX);
				}
			}
			float y = curves[i - 1];
			return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
		}
	}

	/// <summary>Changes a bone's local <see cref="Bone.Rotation"/>.</summary>
	public class RotateTimeline : CurveTimeline, IBoneTimeline {
		public const int ENTRIES = 2;
		internal const int PREV_TIME = -2, PREV_ROTATION = -1;
		internal const int ROTATION = 1;

		internal int boneIndex;
		internal float[] frames; // time, degrees, ...

		public RotateTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount << 1];
		}

		override public int PropertyId {
			get { return ((int)TimelineType.Rotate << 24) + boneIndex; }
		}
		/// <summary>The index of the bone in <see cref="Skeleton.Bones"/> that will be changed.</summary>
		public int BoneIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.boneIndex = value;
			}
			get {
				return boneIndex;
			}
		}
		/// <summary>The time in seconds and rotation in degrees for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } }

		/// <summary>Sets the time in seconds and the rotation in degrees for the specified key frame.</summary>
		public void SetFrame (int frameIndex, float time, float degrees) {
			frameIndex <<= 1;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATION] = degrees;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					bone.rotation = bone.data.rotation;
					return;
				case MixBlend.First:
					float r = bone.data.rotation - bone.rotation;
					bone.rotation += (r - (16384 - (int)(16384.499999999996 - r / 360)) * 360) * alpha;
					return;
				}
				return;
			}

			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				float r = frames[frames.Length + PREV_ROTATION];
				switch (blend) {
					case MixBlend.Setup:
						bone.rotation = bone.data.rotation + r * alpha;
						break;
					case MixBlend.First:
					case MixBlend.Replace:
						r += bone.data.rotation - bone.rotation;
						r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
						goto case MixBlend.Add; // Fall through.

					case MixBlend.Add:
						bone.rotation += r * alpha;
						break;
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frame = Animation.BinarySearch(frames, time, ENTRIES);
			float prevRotation = frames[frame + PREV_ROTATION];
			float frameTime = frames[frame];
			float percent = GetCurvePercent((frame >> 1) - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
			// scope for 'r' to prevent compile error.
			{
				float r = frames[frame + ROTATION] - prevRotation;
				r = prevRotation + (r - (16384 - (int)(16384.499999999996 - r / 360)) * 360) * percent;
				switch (blend) {
					case MixBlend.Setup:
						bone.rotation = bone.data.rotation + (r - (16384 - (int)(16384.499999999996 - r / 360)) * 360) * alpha;
						break;
					case MixBlend.First:
					case MixBlend.Replace:
						r += bone.data.rotation - bone.rotation;
						goto case MixBlend.Add; // Fall through.
					case MixBlend.Add:
						bone.rotation += (r - (16384 - (int)(16384.499999999996 - r / 360)) * 360) * alpha;
						break;
				}
			}
		}
	}

	/// <summary>Changes a bone's local <see cref"Bone.X"/> and <see cref"Bone.Y"/>.</summary>
	public class TranslateTimeline : CurveTimeline, IBoneTimeline {
		public const int ENTRIES = 3;
		protected const int PREV_TIME = -3, PREV_X = -2, PREV_Y = -1;
		protected const int X = 1, Y = 2;

		internal int boneIndex;
		internal float[] frames; // time, x, y, ...

		public TranslateTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		override public int PropertyId {
			get { return ((int)TimelineType.Translate << 24) + boneIndex; }
		}

		/// <summary>The index of the bone in <see cref="Skeleton.Bones"/> that will be changed.</summary>
		public int BoneIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.boneIndex = value;
			}
			get {
				return boneIndex;
			}
		}
		/// <summary>The time in seconds, x, and y values for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } }


		/// <summary>Sets the time in seconds, x, and y values for the specified key frame.</summary>
		public void SetFrame (int frameIndex, float time, float x, float y) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + X] = x;
			frames[frameIndex + Y] = y;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					bone.x = bone.data.x;
					bone.y = bone.data.y;
					return;
				case MixBlend.First:
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
			switch (blend) {
				case MixBlend.Setup:
					bone.x = bone.data.x + x * alpha;
					bone.y = bone.data.y + y * alpha;
					break;
				case MixBlend.First:
				case MixBlend.Replace:
					bone.x += (bone.data.x + x - bone.x) * alpha;
					bone.y += (bone.data.y + y - bone.y) * alpha;
					break;
				case MixBlend.Add:
					bone.x += x * alpha;
					bone.y += y * alpha;
					break;
			}
		}
	}

	/// <summary>Changes a bone's local <see cref="Bone.ScaleX"> and <see cref="Bone.ScaleY">.</summary>
	public class ScaleTimeline : TranslateTimeline, IBoneTimeline {
		public ScaleTimeline (int frameCount)
			: base(frameCount) {
		}

		override public int PropertyId {
			get { return ((int)TimelineType.Scale << 24) + boneIndex; }
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					bone.scaleX = bone.data.scaleX;
					bone.scaleY = bone.data.scaleY;
					return;
				case MixBlend.First:
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
				if (blend == MixBlend.Add) {
					bone.scaleX += x - bone.data.scaleX;
					bone.scaleY += y - bone.data.scaleY;
				} else {
					bone.scaleX = x;
					bone.scaleY = y;
				}
			} else {
				// Mixing out uses sign of setup or current pose, else use sign of key.
				float bx, by;
				if (direction == MixDirection.Out) {
					switch (blend) {
						case MixBlend.Setup:
							bx = bone.data.scaleX;
							by = bone.data.scaleY;
							bone.scaleX = bx + (Math.Abs(x) * Math.Sign(bx) - bx) * alpha;
							bone.scaleY = by + (Math.Abs(y) * Math.Sign(by) - by) * alpha;
							break;
						case MixBlend.First:
						case MixBlend.Replace:
							bx = bone.scaleX;
							by = bone.scaleY;
							bone.scaleX = bx + (Math.Abs(x) * Math.Sign(bx) - bx) * alpha;
							bone.scaleY = by + (Math.Abs(y) * Math.Sign(by) - by) * alpha;
							break;
						case MixBlend.Add:
							bx = bone.scaleX;
							by = bone.scaleY;
							bone.scaleX = bx + (Math.Abs(x) * Math.Sign(bx) - bone.data.scaleX) * alpha;
							bone.scaleY = by + (Math.Abs(y) * Math.Sign(by) - bone.data.scaleY) * alpha;
							break;
					}
				} else {
					switch (blend) {
						case MixBlend.Setup:
							bx = Math.Abs(bone.data.scaleX) * Math.Sign(x);
							by = Math.Abs(bone.data.scaleY) * Math.Sign(y);
							bone.scaleX = bx + (x - bx) * alpha;
							bone.scaleY = by + (y - by) * alpha;
							break;
						case MixBlend.First:
						case MixBlend.Replace:
							bx = Math.Abs(bone.scaleX) * Math.Sign(x);
							by = Math.Abs(bone.scaleY) * Math.Sign(y);
							bone.scaleX = bx + (x - bx) * alpha;
							bone.scaleY = by + (y - by) * alpha;
							break;
						case MixBlend.Add:
							bx = Math.Sign(x);
							by = Math.Sign(y);
							bone.scaleX = Math.Abs(bone.scaleX) * bx + (x - Math.Abs(bone.data.scaleX) * bx) * alpha;
							bone.scaleY = Math.Abs(bone.scaleY) * by + (y - Math.Abs(bone.data.scaleY) * by) * alpha;
							break;
					}
				}
			}
		}
	}

	/// <summary>Changes a bone's local <see cref="Bone.ShearX"/> and <see cref="Bone.ShearY"/>.</summary>
	public class ShearTimeline : TranslateTimeline, IBoneTimeline {
		public ShearTimeline (int frameCount)
			: base(frameCount) {
		}

		override public int PropertyId {
			get { return ((int)TimelineType.Shear << 24) + boneIndex; }
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					bone.shearX = bone.data.shearX;
					bone.shearY = bone.data.shearY;
					return;
				case MixBlend.First:
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
			switch (blend) {
				case MixBlend.Setup:
					bone.shearX = bone.data.shearX + x * alpha;
					bone.shearY = bone.data.shearY + y * alpha;
					break;
				case MixBlend.First:
				case MixBlend.Replace:
					bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
					bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
					break;
				case MixBlend.Add:
					bone.shearX += x * alpha;
					bone.shearY += y * alpha;
					break;
			}
		}
	}

	/// <summary>Changes a slot's <see cref="Slot.Color"/>.</summary>
	public class ColorTimeline : CurveTimeline, ISlotTimeline {
		public const int ENTRIES = 5;
		protected const int PREV_TIME = -5, PREV_R = -4, PREV_G = -3, PREV_B = -2, PREV_A = -1;
		protected const int R = 1, G = 2, B = 3, A = 4;

		internal int slotIndex;
		internal float[] frames; // time, r, g, b, a, ...

		public ColorTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		override public int PropertyId {
			get { return ((int)TimelineType.Color << 24) + slotIndex; }
		}

		/// <summary>The index of the slot in <see cref="Skeleton.Slots"/> that will be changed.</summary>
		public int SlotIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.slotIndex = value;
			}
			get {
				return slotIndex;
			}
		}
		/// <summary>The time in seconds, red, green, blue, and alpha values for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } }

		/// <summary>Sets the time in seconds, red, green, blue, and alpha for the specified key frame.</summary>
		public void SetFrame (int frameIndex, float time, float r, float g, float b, float a) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + R] = r;
			frames[frameIndex + G] = g;
			frames[frameIndex + B] = b;
			frames[frameIndex + A] = a;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				var slotData = slot.data;
				switch (blend) {
				case MixBlend.Setup:
					slot.r = slotData.r;
					slot.g = slotData.g;
					slot.b = slotData.b;
					slot.a = slotData.a;
					return;
				case MixBlend.First:
					slot.r += (slotData.r - slot.r) * alpha;
					slot.g += (slotData.g - slot.g) * alpha;
					slot.b += (slotData.b - slot.b) * alpha;
					slot.a += (slotData.a - slot.a) * alpha;
					slot.ClampColor();
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
				slot.ClampColor();
			} else {
				float br, bg, bb, ba;
				if (blend == MixBlend.Setup) {
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
				slot.ClampColor();
			}
		}
	}

	/// <summary>Changes a slot's <see cref="Slot.Color"/> and <see cref="Slot.DarkColor"/> for two color tinting.</summary>
	public class TwoColorTimeline : CurveTimeline, ISlotTimeline {
		public const int ENTRIES = 8;
		protected const int PREV_TIME = -8, PREV_R = -7, PREV_G = -6, PREV_B = -5, PREV_A = -4;
		protected const int PREV_R2 = -3, PREV_G2 = -2, PREV_B2 = -1;
		protected const int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;

		internal int slotIndex;
		internal float[] frames; // time, r, g, b, a, r2, g2, b2, ...

		public TwoColorTimeline (int frameCount) :
			base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		override public int PropertyId {
			get { return ((int)TimelineType.TwoColor << 24) + slotIndex; }
		}

		/// <summary> The index of the slot in <see cref="Skeleton.Slots"/> that will be changed.</summary>
		public int SlotIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.slotIndex = value;
			}
			get {
				return slotIndex;
			}
		}
		/// <summary>The time in seconds, red, green, blue, and alpha values for each key frame.</summary>
		public float[] Frames { get { return frames; } }

		/// <summary>Sets the time in seconds, light, and dark colors for the specified key frame..</summary>
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

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				var slotData = slot.data;
				switch (blend) {
				case MixBlend.Setup:
					//	slot.color.set(slot.data.color);
					//	slot.darkColor.set(slot.data.darkColor);
					slot.r = slotData.r;
					slot.g = slotData.g;
					slot.b = slotData.b;
					slot.a = slotData.a;
					slot.ClampColor();
					slot.r2 = slotData.r2;
					slot.g2 = slotData.g2;
					slot.b2 = slotData.b2;
					slot.ClampSecondColor();
					return;
				case MixBlend.First:
					slot.r += (slot.r - slotData.r) * alpha;
					slot.g += (slot.g - slotData.g) * alpha;
					slot.b += (slot.b - slotData.b) * alpha;
					slot.a += (slot.a - slotData.a) * alpha;
					slot.ClampColor();
					slot.r2 += (slot.r2 - slotData.r2) * alpha;
					slot.g2 += (slot.g2 - slotData.g2) * alpha;
					slot.b2 += (slot.b2 - slotData.b2) * alpha;
					slot.ClampSecondColor();
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
				slot.ClampColor();
				slot.r2 = r2;
				slot.g2 = g2;
				slot.b2 = b2;
				slot.ClampSecondColor();
			} else {
				float br, bg, bb, ba, br2, bg2, bb2;
				if (blend == MixBlend.Setup) {
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
				slot.ClampColor();
				slot.r2 = br2 + ((r2 - br2) * alpha);
				slot.g2 = bg2 + ((g2 - bg2) * alpha);
				slot.b2 = bb2 + ((b2 - bb2) * alpha);
				slot.ClampSecondColor();
			}
		}

	}

	/// <summary>Changes a slot's <see cref="Slot.Attachment"/>.</summary>
	public class AttachmentTimeline : Timeline, ISlotTimeline {
		internal int slotIndex;
		internal float[] frames; // time, ...
		internal string[] attachmentNames;

		public AttachmentTimeline (int frameCount) {
			frames = new float[frameCount];
			attachmentNames = new String[frameCount];
		}

		public int PropertyId {
			get { return ((int)TimelineType.Attachment << 24) + slotIndex; }
		}

		/// <summary>The number of key frames for this timeline.</summary>
		public int FrameCount { get { return frames.Length; } }

		/// <summary>The index of the slot in <see cref="Skeleton.Slots"> that will be changed.</summary>
		public int SlotIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.slotIndex = value;
			}
			get {
				return slotIndex;
			}
		}

		/// <summary>The time in seconds for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } }

		/// <summary>The attachment name for each key frame. May contain null values to clear the attachment.</summary>
		public string[] AttachmentNames { get { return attachmentNames; } set { attachmentNames = value; } }

		/// <summary>Sets the time in seconds and the attachment name for the specified key frame.</summary>
		public void SetFrame (int frameIndex, float time, String attachmentName) {
			frames[frameIndex] = time;
			attachmentNames[frameIndex] = attachmentName;
		}

		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
							MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			if (direction == MixDirection.Out) {
				if (blend == MixBlend.Setup) SetAttachment(skeleton, slot, slot.data.attachmentName);
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (blend == MixBlend.Setup || blend == MixBlend.First) SetAttachment(skeleton, slot, slot.data.attachmentName);
				return;
			}

			int frameIndex;
			if (time >= frames[frames.Length - 1]) // Time is after last frame.
				frameIndex = frames.Length - 1;
			else
				frameIndex = Animation.BinarySearch(frames, time) - 1;

			SetAttachment(skeleton, slot, attachmentNames[frameIndex]);
		}

		private void SetAttachment (Skeleton skeleton, Slot slot, string attachmentName) {
			slot.Attachment = attachmentName == null ? null : skeleton.GetAttachment(slotIndex, attachmentName);
		}
	}

	/// <summary>Changes a slot's <see cref="Slot.Deform"/> to deform a <see cref="VertexAttachment"/>.</summary>
	public class DeformTimeline : CurveTimeline, ISlotTimeline {
		internal int slotIndex;
		internal VertexAttachment attachment;
		internal float[] frames; // time, ...
		internal float[][] frameVertices;

		public DeformTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount];
			frameVertices = new float[frameCount][];
		}

		override public int PropertyId {
			get { return ((int)TimelineType.Deform << 27) + attachment.id + slotIndex; }
		}

		/// <summary>The index of the slot in <see cref="Skeleton.Slots"/> that will be changed.</summary>
		public int SlotIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.slotIndex = value;
			}
			get {
				return slotIndex;
			}
		}
		/// <summary>The attachment that will be deformed.</summary>
		public VertexAttachment Attachment { get { return attachment; } set { attachment = value; } }

		/// <summary>The time in seconds for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } }

		/// <summary>The vertices for each key frame.</summary>
		public float[][] Vertices { get { return frameVertices; } set { frameVertices = value; } }


		/// <summary>Sets the time in seconds and the vertices for the specified key frame.</summary>
		/// <param name="vertices">Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights.</param>
		public void SetFrame (int frameIndex, float time, float[] vertices) {
			frames[frameIndex] = time;
			frameVertices[frameIndex] = vertices;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			VertexAttachment vertexAttachment = slot.attachment as VertexAttachment;
			if (vertexAttachment == null || vertexAttachment.DeformAttachment != attachment) return;

			var deformArray = slot.Deform;
			if (deformArray.Count == 0) blend = MixBlend.Setup;

			float[][] frameVertices = this.frameVertices;
			int vertexCount = frameVertices[0].Length;
			float[] frames = this.frames;
			float[] deform;

			if (time < frames[0]) {  // Time is before first frame.

				switch (blend) {
				case MixBlend.Setup:
					deformArray.Clear();
					return;
				case MixBlend.First:
					if (alpha == 1) {
						deformArray.Clear();
						return;
					}

					// deformArray.SetSize(vertexCount) // Ensure size and preemptively set count.
					if (deformArray.Capacity < vertexCount) deformArray.Capacity = vertexCount;
					deformArray.Count = vertexCount;
					deform = deformArray.Items;

					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++)
							deform[i] += (setupVertices[i] - deform[i]) * alpha;
					} else {
						// Weighted deform offsets.
						alpha = 1 - alpha;
						for (int i = 0; i < vertexCount; i++)
							deform[i] *= alpha;
					}
					return;
				default:
					return;
				}

			}

			// deformArray.SetSize(vertexCount) // Ensure size and preemptively set count.
			if (deformArray.Capacity < vertexCount) deformArray.Capacity = vertexCount;
			deformArray.Count = vertexCount;
			deform = deformArray.Items;

			if (time >= frames[frames.Length - 1]) { // Time is after last frame.

				float[] lastVertices = frameVertices[frames.Length - 1];
				if (alpha == 1) {
					if (blend == MixBlend.Add) {
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, no alpha.
							float[] setupVertices = vertexAttachment.vertices;
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] - setupVertices[i];
						} else {
							// Weighted deform offsets, no alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i];
						}
					} else {
						// Vertex positions or deform offsets, no alpha.
						Array.Copy(lastVertices, 0, deform, 0, vertexCount);
					}
				} else {
					switch (blend) {
						case MixBlend.Setup: {
							if (vertexAttachment.bones == null) {
								// Unweighted vertex positions, with alpha.
								float[] setupVertices = vertexAttachment.vertices;
								for (int i = 0; i < vertexCount; i++) {
									float setup = setupVertices[i];
									deform[i] = setup + (lastVertices[i] - setup) * alpha;
								}
							} else {
								// Weighted deform offsets, with alpha.
								for (int i = 0; i < vertexCount; i++)
									deform[i] = lastVertices[i] * alpha;
							}
							break;
						}
						case MixBlend.First:
						case MixBlend.Replace:
							// Vertex positions or deform offsets, with alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] += (lastVertices[i] - deform[i]) * alpha;
							break;
						case MixBlend.Add:
							if (vertexAttachment.bones == null) {
								// Unweighted vertex positions, no alpha.
								float[] setupVertices = vertexAttachment.vertices;
								for (int i = 0; i < vertexCount; i++)
									deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
							} else {
								// Weighted deform offsets, alpha.
								for (int i = 0; i < vertexCount; i++)
									deform[i] += lastVertices[i] * alpha;
							}
							break;
					}
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
				if (blend == MixBlend.Add) {
					if (vertexAttachment.bones == null) {
						// Unweighted vertex positions, no alpha.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++) {
							float prev = prevVertices[i];
							deform[i] += prev + (nextVertices[i] - prev) * percent - setupVertices[i];
						}
					} else {
						// Weighted deform offsets, no alpha.
						for (int i = 0; i < vertexCount; i++) {
							float prev = prevVertices[i];
							deform[i] += prev + (nextVertices[i] - prev) * percent;
						}
					}
				} else {
					// Vertex positions or deform offsets, no alpha.
					for (int i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						deform[i] = prev + (nextVertices[i] - prev) * percent;
					}
				}
			} else {
				switch (blend) {
					case MixBlend.Setup: {
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, with alpha.
							float[] setupVertices = vertexAttachment.vertices;
							for (int i = 0; i < vertexCount; i++) {
								float prev = prevVertices[i], setup = setupVertices[i];
								deform[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
							}
						} else {
							// Weighted deform offsets, with alpha.
							for (int i = 0; i < vertexCount; i++) {
								float prev = prevVertices[i];
								deform[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
							}
						}
						break;
					}
					case MixBlend.First:
					case MixBlend.Replace: {
						// Vertex positions or deform offsets, with alpha.
						for (int i = 0; i < vertexCount; i++) {
							float prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
						}
						break;
					}
					case MixBlend.Add: {
						if (vertexAttachment.bones == null) {
							// Unweighted vertex positions, with alpha.
							float[] setupVertices = vertexAttachment.vertices;
							for (int i = 0; i < vertexCount; i++) {
								float prev = prevVertices[i];
								deform[i] += (prev + (nextVertices[i] - prev) * percent - setupVertices[i]) * alpha;
							}
						} else {
							// Weighted deform offsets, with alpha.
							for (int i = 0; i < vertexCount; i++) {
								float prev = prevVertices[i];
								deform[i] += (prev + (nextVertices[i] - prev) * percent) * alpha;
							}
						}
						break;
					}
				}
			}
		}
	}

	/// <summary>Fires an <see cref="Event"/> when specific animation times are reached.</summary>
	public class EventTimeline : Timeline {
		internal float[] frames; // time, ...
		private Event[] events;

		public EventTimeline (int frameCount) {
			frames = new float[frameCount];
			events = new Event[frameCount];
		}

		public int PropertyId {
			get { return ((int)TimelineType.Event << 24); }
		}

		/// <summary>The number of key frames for this timeline.</summary>
		public int FrameCount { get { return frames.Length; } }

		/// <summary>The time in seconds for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } }

		/// <summary>The event for each key frame.</summary>
		public Event[] Events { get { return events; } set { events = value; } }

		/// <summary>Sets the time in seconds and the event for the specified key frame.</summary>
		public void SetFrame (int frameIndex, Event e) {
			frames[frameIndex] = e.Time;
			events[frameIndex] = e;
		}

		/// <summary>Fires events for frames &gt; <code>lastTime</code> and &lt;= <code>time</code>.</summary>
		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
							MixDirection direction) {
			if (firedEvents == null) return;
			float[] frames = this.frames;
			int frameCount = frames.Length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				Apply(skeleton, lastTime, int.MaxValue, firedEvents, alpha, blend, direction);
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

	/// <summary>Changes a skeleton's <see cref="Skeleton.DrawOrder"/>.</summary>
	public class DrawOrderTimeline : Timeline {
		internal float[] frames; // time, ...
		private int[][] drawOrders;

		public DrawOrderTimeline (int frameCount) {
			frames = new float[frameCount];
			drawOrders = new int[frameCount][];
		}

		public int PropertyId {
			get { return ((int)TimelineType.DrawOrder << 24); }
		}

		/// <summary>The number of key frames for this timeline.</summary>
		public int FrameCount { get { return frames.Length; } }

		/// <summary>The time in seconds for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } } // time, ...

		/// <summary>The draw order for each key frame.</summary>
		/// <seealso cref="Timeline.setFrame(int, float, int[])"/>.
		public int[][] DrawOrders { get { return drawOrders; } set { drawOrders = value; } }

		/// <summary>Sets the time in seconds and the draw order for the specified key frame.</summary>
		/// <param name="drawOrder">For each slot in <see cref="Skeleton.Slots"/> the index of the new draw order. May be null to use setup pose
		///                 draw order..</param>
		public void SetFrame (int frameIndex, float time, int[] drawOrder) {
			frames[frameIndex] = time;
			drawOrders[frameIndex] = drawOrder;
		}

		public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
							MixDirection direction) {
			ExposedList<Slot> drawOrder = skeleton.drawOrder;
			ExposedList<Slot> slots = skeleton.slots;
			if (direction == MixDirection.Out) {
				if (blend == MixBlend.Setup) Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (blend == MixBlend.Setup || blend == MixBlend.First) Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
				return;
			}

			int frame;
			if (time >= frames[frames.Length - 1]) // Time is after last frame.
				frame = frames.Length - 1;
			else
				frame = Animation.BinarySearch(frames, time) - 1;

			int[] drawOrderToSetupIndex = drawOrders[frame];
			if (drawOrderToSetupIndex == null) {
				Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
			} else {
				var drawOrderItems = drawOrder.Items;
				var slotsItems = slots.Items;
				for (int i = 0, n = drawOrderToSetupIndex.Length; i < n; i++)
					drawOrderItems[i] = slotsItems[drawOrderToSetupIndex[i]];
			}
		}
	}

	/// <summary>Changes an IK constraint's <see cref="IkConstraint.Mix"/>, <see cref="IkConstraint.Softness"/>,
	/// <see cref="IkConstraint.BendDirection"/>, <see cref="IkConstraint.Stretch"/>, and <see cref="IkConstraint.Compress"/>.</summary>
	public class IkConstraintTimeline : CurveTimeline {
		public const int ENTRIES = 6;
		private const int PREV_TIME = -6, PREV_MIX = -5, PREV_SOFTNESS = -4, PREV_BEND_DIRECTION = -3, PREV_COMPRESS = -2,
			PREV_STRETCH = -1;
		private const int MIX = 1, SOFTNESS = 2, BEND_DIRECTION = 3, COMPRESS = 4, STRETCH = 5;

		internal int ikConstraintIndex;
		internal float[] frames; // time, mix, softness, bendDirection, compress, stretch, ...

		public IkConstraintTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		override public int PropertyId {
			get { return ((int)TimelineType.IkConstraint << 24) + ikConstraintIndex; }
		}

		/// <summary>The index of the IK constraint slot in <see cref="Skeleton.IkConstraints"/> that will be changed.</summary>
		public int IkConstraintIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.ikConstraintIndex = value;
			}
			get {
				return ikConstraintIndex;
			}
		}

		/// <summary>The time in seconds, mix, softness, bend direction, compress, and stretch for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } }

		/// <summary>Sets the time in seconds, mix, softness, bend direction, compress, and stretch for the specified key frame.</summary>
		public void SetFrame (int frameIndex, float time, float mix, float softness, int bendDirection, bool compress,
			bool stretch) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + MIX] = mix;
			frames[frameIndex + SOFTNESS] = softness;
			frames[frameIndex + BEND_DIRECTION] = bendDirection;
			frames[frameIndex + COMPRESS] = compress ? 1 : 0;
			frames[frameIndex + STRETCH] = stretch ? 1 : 0;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			IkConstraint constraint = skeleton.ikConstraints.Items[ikConstraintIndex];
			if (!constraint.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					constraint.mix = constraint.data.mix;
					constraint.softness = constraint.data.softness;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
					return;
				case MixBlend.First:
					constraint.mix += (constraint.data.mix - constraint.mix) * alpha;
					constraint.softness += (constraint.data.softness - constraint.softness) * alpha;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
					return;
				}
				return;
			}

			if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
				if (blend == MixBlend.Setup) {
					constraint.mix = constraint.data.mix + (frames[frames.Length + PREV_MIX] - constraint.data.mix) * alpha;
					constraint.softness = constraint.data.softness
						+ (frames[frames.Length + PREV_SOFTNESS] - constraint.data.softness) * alpha;
					if (direction == MixDirection.Out) {
						constraint.bendDirection = constraint.data.bendDirection;
						constraint.compress = constraint.data.compress;
						constraint.stretch = constraint.data.stretch;
					} else {
						constraint.bendDirection = (int)frames[frames.Length + PREV_BEND_DIRECTION];
						constraint.compress = frames[frames.Length + PREV_COMPRESS] != 0;
						constraint.stretch = frames[frames.Length + PREV_STRETCH] != 0;
					}
				} else {
					constraint.mix += (frames[frames.Length + PREV_MIX] - constraint.mix) * alpha;
					constraint.softness += (frames[frames.Length + PREV_SOFTNESS] - constraint.softness) * alpha;
					if (direction == MixDirection.In) {
						constraint.bendDirection = (int)frames[frames.Length + PREV_BEND_DIRECTION];
						constraint.compress = frames[frames.Length + PREV_COMPRESS] != 0;
						constraint.stretch = frames[frames.Length + PREV_STRETCH] != 0;
					}
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frame = Animation.BinarySearch(frames, time, ENTRIES);
			float mix = frames[frame + PREV_MIX];
			float softness = frames[frame + PREV_SOFTNESS];
			float frameTime = frames[frame];
			float percent = GetCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			if (blend == MixBlend.Setup) {
				constraint.mix = constraint.data.mix + (mix + (frames[frame + MIX] - mix) * percent - constraint.data.mix) * alpha;
				constraint.softness = constraint.data.softness
					+ (softness + (frames[frame + SOFTNESS] - softness) * percent - constraint.data.softness) * alpha;
				if (direction == MixDirection.Out) {
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
				} else {
					constraint.bendDirection = (int)frames[frame + PREV_BEND_DIRECTION];
					constraint.compress = frames[frame + PREV_COMPRESS] != 0;
					constraint.stretch = frames[frame + PREV_STRETCH] != 0;
				}
			} else {
				constraint.mix += (mix + (frames[frame + MIX] - mix) * percent - constraint.mix) * alpha;
				constraint.softness += (softness + (frames[frame + SOFTNESS] - softness) * percent - constraint.softness) * alpha;
				if (direction == MixDirection.In) {
					constraint.bendDirection = (int)frames[frame + PREV_BEND_DIRECTION];
					constraint.compress = frames[frame + PREV_COMPRESS] != 0;
					constraint.stretch = frames[frame + PREV_STRETCH] != 0;
				}
			}
		}
	}

	///	<summary>Changes a transform constraint's mixes.</summary>
	public class TransformConstraintTimeline : CurveTimeline {
		public const int ENTRIES = 5;
		private const int PREV_TIME = -5, PREV_ROTATE = -4, PREV_TRANSLATE = -3, PREV_SCALE = -2, PREV_SHEAR = -1;
		private const int ROTATE = 1, TRANSLATE = 2, SCALE = 3, SHEAR = 4;

		internal int transformConstraintIndex;
		internal float[] frames; // time, rotate mix, translate mix, scale mix, shear mix, ...

		public TransformConstraintTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		override public int PropertyId {
			get { return ((int)TimelineType.TransformConstraint << 24) + transformConstraintIndex; }
		}

		/// <summary>The index of the transform constraint slot in <see cref="Skeleton.TransformConstraints"/> that will be changed.</summary>
		public int TransformConstraintIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.transformConstraintIndex = value;
			}
			get {
				return transformConstraintIndex;
			}
		}

		/// <summary>The time in seconds, rotate mix, translate mix, scale mix, and shear mix for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } } // time, rotate mix, translate mix, scale mix, shear mix, ...

		/// <summary>The time in seconds, rotate mix, translate mix, scale mix, and shear mix for the specified key frame.</summary>
		public void SetFrame (int frameIndex, float time, float rotateMix, float translateMix, float scaleMix, float shearMix) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATE] = rotateMix;
			frames[frameIndex + TRANSLATE] = translateMix;
			frames[frameIndex + SCALE] = scaleMix;
			frames[frameIndex + SHEAR] = shearMix;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			TransformConstraint constraint = skeleton.transformConstraints.Items[transformConstraintIndex];
			if (!constraint.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				TransformConstraintData data = constraint.data;
				switch (blend) {
				case MixBlend.Setup:
					constraint.rotateMix = data.rotateMix;
					constraint.translateMix = data.translateMix;
					constraint.scaleMix = data.scaleMix;
					constraint.shearMix = data.shearMix;
					return;
				case MixBlend.First:
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
			if (blend == MixBlend.Setup) {
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

	/// <summary>Changes a path constraint's <see cref="PathConstraint.Position"/>.</summary>
	public class PathConstraintPositionTimeline : CurveTimeline {
		public const int ENTRIES = 2;
		protected const int PREV_TIME = -2, PREV_VALUE = -1;
		protected const int VALUE = 1;

		internal int pathConstraintIndex;
		internal float[] frames; // time, position, ...

		public PathConstraintPositionTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		override public int PropertyId {
			get { return ((int)TimelineType.PathConstraintPosition << 24) + pathConstraintIndex; }
		}

		/// <summary>The index of the path constraint slot in <see cref="Skeleton.PathConstraints"/> that will be changed.</summary>
		public int PathConstraintIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.pathConstraintIndex = value;
			}
			get {
				return pathConstraintIndex;
			}
		}

		/// <summary>The time in seconds and path constraint position for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } } // time, position, ...

		/// <summary>Sets the time in seconds and path constraint position for the specified key frame.</summary>
		public void SetFrame (int frameIndex, float time, float position) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + VALUE] = position;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
			if (!constraint.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					constraint.position = constraint.data.position;
					return;
				case MixBlend.First:
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
			if (blend == MixBlend.Setup)
				constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
			else
				constraint.position += (position - constraint.position) * alpha;
		}
	}

	/// <summary>Changes a path constraint's <see cref="PathConstraint.Spacing"/>.</summary>
	public class PathConstraintSpacingTimeline : PathConstraintPositionTimeline {
		public PathConstraintSpacingTimeline (int frameCount)
			: base(frameCount) {
		}

		override public int PropertyId {
			get { return ((int)TimelineType.PathConstraintSpacing << 24) + pathConstraintIndex; }
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
									MixDirection direction) {
			PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
			if (!constraint.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					constraint.spacing = constraint.data.spacing;
					return;
				case MixBlend.First:
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

			if (blend == MixBlend.Setup)
				constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
			else
				constraint.spacing += (spacing - constraint.spacing) * alpha;
		}
	}

	/// <summary>Changes a path constraint's mixes.</summary>
	public class PathConstraintMixTimeline : CurveTimeline {
		public const int ENTRIES = 3;
		private const int PREV_TIME = -3, PREV_ROTATE = -2, PREV_TRANSLATE = -1;
		private const int ROTATE = 1, TRANSLATE = 2;

		internal int pathConstraintIndex;
		internal float[] frames; // time, rotate mix, translate mix, ...

		public PathConstraintMixTimeline (int frameCount)
			: base(frameCount) {
			frames = new float[frameCount * ENTRIES];
		}

		override public int PropertyId {
			get { return ((int)TimelineType.PathConstraintMix << 24) + pathConstraintIndex; }
		}

		/// <summary>The index of the path constraint slot in <see cref="Skeleton.PathConstraints"/> that will be changed.</summary>
		public int PathConstraintIndex {
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
				this.pathConstraintIndex = value;
			}
			get {
				return pathConstraintIndex;
			}
		}

		/// <summary>The time in seconds, rotate mix, and translate mix for each key frame.</summary>
		public float[] Frames { get { return frames; } set { frames = value; } } // time, rotate mix, translate mix, ...

		/// <summary>The time in seconds, rotate mix, and translate mix for the specified key frame.</summary>
		public void SetFrame (int frameIndex, float time, float rotateMix, float translateMix) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATE] = rotateMix;
			frames[frameIndex + TRANSLATE] = translateMix;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
			if (!constraint.active) return;
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					constraint.rotateMix = constraint.data.rotateMix;
					constraint.translateMix = constraint.data.translateMix;
					return;
				case MixBlend.First:
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

			if (blend == MixBlend.Setup) {
				constraint.rotateMix = constraint.data.rotateMix + (rotate - constraint.data.rotateMix) * alpha;
				constraint.translateMix = constraint.data.translateMix + (translate - constraint.data.translateMix) * alpha;
			} else {
				constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
				constraint.translateMix += (translate - constraint.translateMix) * alpha;
			}
		}
	}
}
