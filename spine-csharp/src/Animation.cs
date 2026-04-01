/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#if UNITY_5_3_OR_NEWER
#define IS_UNITY
#endif

using System;
using System.Collections.Generic;

namespace Spine {
#if IS_UNITY
	using Color32F = UnityEngine.Color;
#endif

	/// <summary>
	/// Stores a list of timelines to animate a skeleton's pose over time.</summary>
	public class Animation {
		internal string name;
		internal float duration;
		internal ExposedList<Timeline> timelines;
		internal HashSet<string> timelineIds;
		internal readonly ExposedList<int> bones;

		public Animation (string name, ExposedList<Timeline> timelines, float duration) {
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");

			this.name = name;
			this.duration = duration;
			int n = timelines.Count << 1;
			// note: not needed: timelineIds = new HashSet<string>(n);
			bones = new ExposedList<int>(n);
			SetTimelines(timelines);
		}

		public ExposedList<Timeline> Timelines {
			get { return timelines; }
			set { SetTimelines(value); }
		}

		public void SetTimelines (ExposedList<Timeline> timelines) {
			if (timelines == null) throw new ArgumentNullException("timelines", "timelines cannot be null.");
			this.timelines = timelines;

			// Note: avoiding reallocations by adding all hash set entries at
			// once (EnsureCapacity() is only available in newer .Net versions).
			int idCount = 0;
			int n = timelines.Count;
			// not needed: timelineIds.Clear(n << 1);
			bones.Clear();
			var boneSet = new HashSet<int>();
			Timeline[] items = timelines.Items;
			for (int i = 0; i < n; ++i)
				idCount += items[i].PropertyIds.Length;
			var propertyIds = new string[idCount];
			int currentId = 0;
			for (int i = 0; i < n; ++i) {
				Timeline timeline = items[i];
				string[] ids = items[i].PropertyIds;
				for (int ii = 0, idsLength = ids.Length; ii < idsLength; ++ii)
					propertyIds[currentId++] = ids[ii];

				IBoneTimeline boneTimeline = timeline as IBoneTimeline;
				if (boneTimeline != null && boneSet.Add(boneTimeline.BoneIndex))
					bones.Add(boneTimeline.BoneIndex);
			}
			this.timelineIds = new HashSet<string>(propertyIds);
			bones.TrimExcess();
		}

		/// <summary>Returns true if this animation contains a timeline with any of the specified property IDs.</summary>
		public bool HasTimeline (string[] propertyIds) {
			foreach (string id in propertyIds)
				if (timelineIds.Contains(id)) return true;
			return false;
		}

		/// <summary>The duration of the animation in seconds, which is usually the highest time of all frames in the timeline. The duration is
		/// used to know when it has completed and when it should loop back to the start.</summary>
		public float Duration { get { return duration; } set { duration = value; } }


		/// <summary>Applies the animation's timelines to the specified skeleton.</summary>
		/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixBlend, MixDirection, bool)"/>
		/// <param name="skeleton">The skeleton the animation is being applied to. This provides access to the bones, slots, and other skeleton
		///					components the timelines may change.</param>
		/// <param name="lastTime">The last time in seconds this animation was applied. Some timelines trigger only at specific times rather
		///					than every frame. Pass -1 the first time an animation is applied to ensure frame 0 is triggered.</param>
		/// <param name="time"> The time in seconds the skeleton is being posed for. Most timelines find the frame before and the frame after
		///					this time and interpolate between the frame values. If beyond the <see cref="Duration"/> and <c>loop</c> is
		///					true then the animation will repeat, else the last frame will be applied.</param>
		/// <param name="loop">If true, the animation repeats after the <see cref="Duration"/>.</param>
		/// <param name="events">If any events are fired, they are added to this list. Can be null to ignore fired events or if no timelines
		///					fire events.</param>
		/// <param name="alpha"> 0 applies the current or setup values (depending on <c>blend</c>). 1 applies the timeline values. Between
		///					0 and 1 applies values between the current or setup values and the timeline values. By adjusting
		///					<c>alpha</c> over time, an animation can be mixed in or out. <c>alpha</c> can also be useful to apply
		///					animations on top of each other (layering).</param>
		/// <param name="blend">Controls how mixing is applied when <c>alpha</c> &lt; 1.</param>
		/// <param name="direction">Indicates whether the timelines are mixing in or out. Used by timelines which perform instant transitions,
		///					such as <see cref="DrawOrderTimeline"/> or <see cref="AttachmentTimeline"/>.</param>
		public void Apply (Skeleton skeleton, float lastTime, float time, bool loop, ExposedList<Event> events, float alpha,
							MixBlend blend, MixDirection direction, bool appliedPose) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				if (lastTime > 0) lastTime %= duration;
			}

			Timeline[] timelines = this.timelines.Items;
			for (int i = 0, n = this.timelines.Count; i < n; i++)
				timelines[i].Apply(skeleton, lastTime, time, events, alpha, blend, direction, appliedPose);
		}

		/// <summary>The animation's name, which is unique across all animations in the skeleton.</summary>
		public string Name { get { return name; } }

		override public string ToString () {
			return name;
		}
	}

	/// <summary>
	/// Controls how timeline values are mixed with setup pose values or current pose values when a timeline is applied with
	/// <c>alpha</c> &lt; 1.</summary>
	/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixBlend, MixDirection, bool)"/>
	public enum MixBlend {
		/// <summary>
		/// <para>Transitions between the setup and timeline values (the current value is not used). Before the first frame, the setup
		/// value is used.</para>
		/// <para>
		/// <c>Setup</c> is intended to transition to or from the setup pose, not for animations layered on top of others.</para></summary>
		Setup,

		/// <summary>
		/// <para>
		/// Transitions between the current and timeline values. Before the first frame, transitions between the current and setup
		/// values. Timelines which perform instant transitions, such as <see cref="DrawOrderTimeline"/> or <see cref="AttachmentTimeline"/>, use
		/// the setup value before the first frame.</para>
		/// <para>
		/// <c>First</c> is intended for the first animations applied, not for animations layered on top of others.</para>
		/// </summary>
		First,

		/// <summary>
		/// <para>
		/// Transitions between the current and timeline values. No change is made before the first frame.</para>
		/// <para>
		/// <c>Replace</c> is intended for animations layered on top of others, not for the first animations applied.</para>
		/// </summary>
		Replace,

		/// <summary>
		/// <para>
		/// Transitions between the current value and the current plus timeline values. No change is made before the first frame.</para>
		/// <para>
		/// <c>Add</c> is intended for animations layered on top of others, not for the first animations applied.</para>
		/// <para>
		/// Properties set by additive animations must be set manually or by another animation before applying the additive
		/// animations, else the property values will increase each time the additive animations are applied.</para>
		/// </summary>
		Add
	}

	/// <summary>
	/// Indicates whether a timeline's <c>alpha</c> is mixing out over time toward 0 (the setup or current pose value) or
	/// mixing in toward 1 (the timeline's value). Some timelines use this to decide how values are applied.</summary>
	/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixBlend, MixDirection, bool)"/>
	public enum MixDirection {
		In,
		Out
	}

	public enum Property {
		Rotate = 0, X, Y, ScaleX, ScaleY, ShearX, ShearY, Inherit, //
		RGB, Alpha, RGB2, //
		Attachment, Deform, //
		Event, DrawOrder, //
		IkConstraint, TransformConstraint, //
		PathConstraintPosition, PathConstraintSpacing, PathConstraintMix, //
		PhysicsConstraintInertia, PhysicsConstraintStrength, PhysicsConstraintDamping, PhysicsConstraintMass, //
		PhysicsConstraintWind, PhysicsConstraintGravity, PhysicsConstraintMix, PhysicsConstraintReset, //
		Sequence, //
		SliderTime, SliderMix
	}

	/// <summary>
	/// The base class for all timelines.</summary>
	public abstract class Timeline {
		private readonly string[] propertyIds;
		internal readonly float[] frames;

		/// <param name="propertyIds">Unique identifiers for the properties the timeline modifies.</param>
		public Timeline (int frameCount, params string[] propertyIds) {
			if (propertyIds == null) throw new System.ArgumentNullException("propertyIds", "propertyIds cannot be null.");
			this.propertyIds = propertyIds;
			frames = new float[frameCount * FrameEntries];
		}

		/// <summary>Uniquely encodes both the type of this timeline and the skeleton properties that it affects.</summary>
		public string[] PropertyIds {
			get { return propertyIds; }
		}

		/// <summary>The time in seconds and any other values for each frame.</summary>
		public float[] Frames {
			get { return frames; }
		}

		/// <summary>The number of entries stored per frame.</summary>
		public virtual int FrameEntries {
			get { return 1; }
		}

		/// <summary>The number of frames for this timeline.</summary>
		public virtual int FrameCount {
			get { return frames.Length / FrameEntries; }
		}

		public float Duration {
			get {
				return frames[frames.Length - FrameEntries];
			}
		}

		/// <summary>Applies this timeline to the skeleton.</summary>
		/// <param name="skeleton">The skeleton the timeline is being applied to. This provides access to the bones, slots, and other
		///					skeleton components the timeline may change.</param>
		/// <param name="lastTime">The time this timeline was last applied. Timelines such as <see cref="EventTimeline"/> trigger only
		///					at specific times rather than every frame. In that case, the timeline triggers everything between
		///					<c>lastTime</c> (exclusive) and <c>time</c> (inclusive). Pass -1 the first time an animation is
		///					 applied to ensure frame 0 is triggered.</param>
		/// <param name="time">The time in seconds that the skeleton is being posed for. Most timelines find the frame before and the frame
		///					after this time and interpolate between the frame values.If beyond the last frame, the last frame will be
		///					applied.</param>
		/// <param name="events">If any events are fired, they are added to this list. Can be null to ignore fired events or if the timeline
		///					does not fire events.</param>
		/// <param name="alpha">0 applies the current or setup value (depending on <c>blend</c>). 1 applies the timeline value.
		///					Between 0 and 1 applies a value between the current or setup value and the timeline value.By adjusting
		///					<c>alpha</c> over time, an animation can be mixed in or out. <c>alpha</c> can also be useful to
		///					apply animations on top of each other (layering).</param>
		/// <param name="blend">Controls how mixing is applied when <c>alpha</c> &lt; 1.</param>
		/// <param name="direction">Indicates whether the timeline is mixing in or out. Used by timelines which perform instant transitions,
		///                   such as <see cref="DrawOrderTimeline"/> or <see cref="AttachmentTimeline"/>, and other such as <see cref="ScaleTimeline"/>.</param>
		/// <param name="appliedPose">True to modify the applied pose.</param>
		public abstract void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha,
			MixBlend blend, MixDirection direction, bool appliedPose);

		/// <summary>Linear search using a stride of 1.</summary>
		/// <param name="time">Must be >= the first value in <c>frames</c>.</param>
		/// <returns>The index of the first value <= <c>time</c>.</returns>
		internal static int Search (float[] frames, float time) {
			int n = frames.Length;
			for (int i = 1; i < n; i++)
				if (frames[i] > time) return i - 1;
			return n - 1;
		}

		/// <summary>Search using the specified stride.</summary>
		/// <param name="time">Must be >= the first value in <c>frames</c>.</param>
		/// <returns>The index of the first value <= <c>time</c>.</returns>
		internal static int Search (float[] frames, float time, int step) {
			int n = frames.Length;
			for (int i = step; i < n; i += step)
				if (frames[i] > time) return i - step;
			return n - step;
		}
	}

	/// <summary>An interface for timelines which change the property of a slot.</summary>
	public interface ISlotTimeline {
		/// <summary>The index of the slot in <see cref="Skeleton.Slots"/> that will be changed when this timeline is applied.</summary>
		int SlotIndex { get; }
	}

	/// <summary>The base class for timelines that interpolate between frame values using stepped, linear, or a Bezier curve.</summary>
	public abstract class CurveTimeline : Timeline {
		public const int LINEAR = 0, STEPPED = 1, BEZIER = 2, BEZIER_SIZE = 18;

		internal float[] curves;
		/// <summary>The number of key frames for this timeline.</summary>

		/// <param name="bezierCount">The maximum number of Bezier curves. See <see cref="Shrink(int)"/>.</param>
		/// <param name="propertyIds">Unique identifiers for the properties the timeline modifies.</param>
		public CurveTimeline (int frameCount, int bezierCount, params string[] propertyIds)
			: base(frameCount, propertyIds) {
			curves = new float[frameCount + bezierCount * BEZIER_SIZE];
			curves[frameCount - 1] = STEPPED;
		}

		/// <summary>Sets the specified frame to linear interpolation.</summary>
		/// <param name="frame">Between 0 and <c>frameCount - 1</c>, inclusive.</param>
		public void SetLinear (int frame) {
			curves[frame] = LINEAR;
		}

		/// <summary>Sets the specified frame to stepped interpolation.</summary>
		/// <param name="frame">Between 0 and <c>frameCount - 1</c>, inclusive.</param>
		public void SetStepped (int frame) {
			curves[frame] = STEPPED;
		}

		/// <summary>Returns the interpolation type for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount - 1</c>, inclusive.</param>
		/// <returns><see cref="LINEAR"/>, <see cref="STEPPED"/> or <see cref="BEZIER"/> + the index of the Bezier segments.</returns>
		public float GetCurveType (int frame) {
			return (int)curves[frame];
		}

		/// <summary>Shrinks the storage for Bezier curves, for use when <c>bezierCount</c> (specified in the constructor) was larger
		/// than the actual number of Bezier curves.</summary>
		public void Shrink (int bezierCount) {
			int size = FrameCount + bezierCount * BEZIER_SIZE;
			if (curves.Length > size) {
				var newCurves = new float[size];
				Array.Copy(curves, 0, newCurves, 0, size);
				curves = newCurves;
			}
		}

		/// <summary>
		/// Stores the segments for the specified Bezier curve. For timelines that modify multiple values, there may be more than
		/// one curve per frame.</summary>
		/// <param name="bezier">The ordinal of this Bezier curve for this timeline, between 0 and <c>bezierCount - 1</c> (specified
		///					in the constructor), inclusive.</param>
		/// <param name="frame">Between 0 and <c>frameCount - 1</c>, inclusive.</param>
		/// <param name="value">The index of the value for the frame this curve is used for.</param>
		/// <param name="time1">The time for the first key.</param>
		/// <param name="value1">The value for the first key.</param>
		/// <param name="cx1">The time for the first Bezier handle.</param>
		/// <param name="cy1">The value for the first Bezier handle.</param>
		/// <param name="cx2">The time of the second Bezier handle.</param>
		/// <param name="cy2">The value for the second Bezier handle.</param>
		/// <param name="time2">The time for the second key.</param>
		/// <param name="value2">The value for the second key.</param>
		public virtual void SetBezier (int bezier, int frame, int value, float time1, float value1, float cx1, float cy1, float cx2,
			float cy2, float time2, float value2) {

			float[] curves = this.curves;
			int i = FrameCount + bezier * BEZIER_SIZE;
			if (value == 0) curves[frame] = BEZIER + i;
			float tmpx = (time1 - cx1 * 2 + cx2) * 0.03f, tmpy = (value1 - cy1 * 2 + cy2) * 0.03f;
			float dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006f, dddy = ((cy1 - cy2) * 3 - value1 + value2) * 0.006f;
			float ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
			float dx = (cx1 - time1) * 0.3f + tmpx + dddx * 0.16666667f, dy = (cy1 - value1) * 0.3f + tmpy + dddy * 0.16666667f;
			float x = time1 + dx, y = value1 + dy;
			for (int n = i + BEZIER_SIZE; i < n; i += 2) {
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

		/// <summary>
		/// Returns the Bezier interpolated value for the specified time.</summary>
		/// <param name="frameIndex">The index into <see cref="Frames"/> for the values of the frame before <c>time</c>.</param>
		/// <param name="valueOffset">The offset from <c>frameIndex</c> to the value this curve is used for.</param>
		/// <param name="i">The index of the Bezier segments. See <see cref="GetCurveType(int)"/>.</param>
		public float GetBezierValue (float time, int frameIndex, int valueOffset, int i) {
			float[] curves = this.curves;
			if (curves[i] > time) {
				float x = frames[frameIndex], y = frames[frameIndex + valueOffset];
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
			}
			int n = i + BEZIER_SIZE;
			for (i += 2; i < n; i += 2) {
				if (curves[i] >= time) {
					float x = curves[i - 2], y = curves[i - 1];
					return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
				}
			}
			frameIndex += FrameEntries;
			{ // scope added to prevent compile error "float x and y declared in enclosing scope"
				float x = curves[n - 2], y = curves[n - 1];
				return y + (time - x) / (frames[frameIndex] - x) * (frames[frameIndex + valueOffset] - y);
			}
		}
	}

	/// <summary>The base class for a <see cref="CurveTimeline"/> that sets one property.</summary>
	public abstract class CurveTimeline1 : CurveTimeline {
		public const int ENTRIES = 2;
		internal const int VALUE = 1;

		/// <param name="bezierCount">The maximum number of Bezier curves. See <see cref="Shrink(int)"/>.</param>
		/// <param name="propertyIds">Unique identifiers for the properties the timeline modifies.</param>
		public CurveTimeline1 (int frameCount, int bezierCount, string propertyId)
			: base(frameCount, bezierCount, propertyId) {
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		/// <summary>Sets the time and value for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds</param>
		public void SetFrame (int frame, float time, float value) {
			frame <<= 1;
			frames[frame] = time;
			frames[frame + VALUE] = value;
		}

		/// <summary>Returns the interpolated value for the specified time.</summary>
		public float GetCurveValue (float time) {
			float[] frames = this.frames;
			int i = frames.Length - 2;
			for (int ii = 2; ii <= i; ii += 2) {
				if (frames[ii] > time) {
					i = ii - 2;
					break;
				}
			}

			int curveType = (int)curves[i >> 1];
			switch (curveType) {
			case LINEAR:
				float before = frames[i], value = frames[i + VALUE];
				return value + (time - before) / (frames[i + ENTRIES] - before) * (frames[i + ENTRIES + VALUE] - value);
			case STEPPED:
				return frames[i + VALUE];
			}
			return GetBezierValue(time, i, VALUE, curveType - BEZIER);
		}

		public float GetRelativeValue (float time, float alpha, MixBlend blend, float current, float setup) {
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.Setup:
					return setup;
				case MixBlend.First:
					return current + (setup - current) * alpha;
				}
				return current;
			}
			float value = GetCurveValue(time);
			switch (blend) {
			case MixBlend.Setup:
				return setup + value * alpha;
			case MixBlend.First:
			case MixBlend.Replace:
				value += setup - current;
				break;
			case MixBlend.Add:
				return current + value * alpha;
			}
			return current + value * alpha;
		}

		public float GetAbsoluteValue (float time, float alpha, MixBlend blend, float current, float setup) {
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.Setup:
					return setup;
				case MixBlend.First:
					return current + (setup - current) * alpha;
				}
				return current;
			}
			float value = GetCurveValue(time);
			switch (blend) {
			case MixBlend.Setup:
				return setup + (value - setup) * alpha;
			case MixBlend.First:
			case MixBlend.Replace:
			default:
				return current + (value - current) * alpha;
			case MixBlend.Add:
				return current + value * alpha;
			}
		}

		public float GetAbsoluteValue (float time, float alpha, MixBlend blend, float current, float setup, float value) {
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.Setup:
					return setup;
				case MixBlend.First:
					return current + (setup - current) * alpha;
				}
				return current;
			}
			switch (blend) {
			case MixBlend.Setup:
				return setup + (value - setup) * alpha;
			case MixBlend.First:
			case MixBlend.Replace:
			default:
				return current + (value - current) * alpha;
			case MixBlend.Add:
				return current + value * alpha;
			}
		}

		public float GetScaleValue (float time, float alpha, MixBlend blend, MixDirection direction, float current, float setup) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.Setup:
					return setup;
				case MixBlend.First:
					return current + (setup - current) * alpha;
				}
				return current;
			}
			float value = GetCurveValue(time) * setup;
			if (alpha == 1) {
				if (blend == MixBlend.Add) return current + value - setup;
				return value;
			}
			// Mixing out uses sign of setup or current pose, else use sign of key.
			if (direction == MixDirection.Out) {
				switch (blend) {
				case MixBlend.Setup:
					return setup + (Math.Abs(value) * Math.Sign(setup) - setup) * alpha;
				case MixBlend.First:
				case MixBlend.Replace:
					return current + (Math.Abs(value) * Math.Sign(current) - current) * alpha;
				}
			} else {
				float s;
				switch (blend) {
				case MixBlend.Setup:
					s = Math.Abs(setup) * Math.Sign(value);
					return s + (value - s) * alpha;
				case MixBlend.First:
				case MixBlend.Replace:
					s = Math.Abs(current) * Math.Sign(value);
					return s + (value - s) * alpha;
				}
			}
			return current + (value - setup) * alpha;
		}
	}

	/// <summary>The base class for a <see cref="CurveTimeline"/> that is a <see cref="BoneTimeline"/> and sets two properties.</summary>
	public abstract class BoneTimeline2 : CurveTimeline, IBoneTimeline {
		public const int ENTRIES = 3;
		internal const int VALUE1 = 1, VALUE2 = 2;

		readonly int boneIndex;

		/// <param name="bezierCount">The maximum number of Bezier curves. See <see cref="Shrink(int)"/>.</param>
		public BoneTimeline2 (int frameCount, int bezierCount, int boneIndex, Property property1, Property property2)
			: base(frameCount, bezierCount, (int)property1 + "|" + boneIndex, (int)property2 + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		/// <summary>Sets the time and values for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float value1, float value2) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + VALUE1] = value1;
			frames[frame + VALUE2] = value2;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
			MixDirection direction, bool appliedPose) {

			Bone bone = skeleton.bones.Items[boneIndex];
			if (bone.active) Apply(appliedPose ? bone.applied : bone.pose, bone.data.setup, time, alpha, blend, direction);
		}

		abstract protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend,
			MixDirection direction);
	}

	/// <summary>An interface for timelines which change the property of a bone.</summary>
	public interface IBoneTimeline {
		/// <summary>The index of the bone in <see cref="Skeleton.Bones"/> that will be changed when this timeline is applied.</summary>
		int BoneIndex { get; }
	}

	public abstract class BoneTimeline1 : CurveTimeline1, IBoneTimeline {
		readonly int boneIndex;

		public BoneTimeline1 (int frameCount, int bezierCount, int boneIndex, Property property)
			: base(frameCount, bezierCount, (int)property + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
			MixDirection direction, bool appliedPose) {

			Bone bone = skeleton.bones.Items[boneIndex];
			if (bone.active) Apply(appliedPose ? bone.applied : bone.pose, bone.data.setup, time, alpha, blend, direction);
		}

		abstract protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend,
			MixDirection direction);
	}

	/// <summary>Changes a bone's local <see cref="BoneLocal.Rotation"/>.</summary>
	public class RotateTimeline : BoneTimeline1, IBoneTimeline {
		public RotateTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.Rotate) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.rotation = GetRelativeValue(time, alpha, blend, pose.Rotation, setup.rotation);
		}
	}

	/// <summary>Changes a bone's local <see cref"BoneLocal.X"/> and <see cref"BonePose.Y"/>.</summary>
	public class TranslateTimeline : BoneTimeline2 {
		public TranslateTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.X, Property.Y) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.Setup:
					pose.x = setup.x;
					pose.y = setup.y;
					return;
				case MixBlend.First:
					pose.x += (setup.x - pose.x) * alpha;
					pose.y += (setup.y - pose.y) * alpha;
					break;
				}
				return;
			}

			float x, y;
			// note: reference implementation has code inlined, we re-use GetCurveValue code for root motion.
			GetCurveValue(out x, out y, time);

			switch (blend) {
			case MixBlend.Setup:
				pose.x = setup.x + x * alpha;
				pose.y = setup.y + y * alpha;
				break;
			case MixBlend.First:
			case MixBlend.Replace:
				pose.x += (setup.x + x - pose.x) * alpha;
				pose.y += (setup.y + y - pose.y) * alpha;
				break;
			case MixBlend.Add:
				pose.x += x * alpha;
				pose.y += y * alpha;
				break;
			}
		}

		public void GetCurveValue (out float x, out float y, float time) {
			int i = Search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				float before = frames[i];
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				x += (frames[i + ENTRIES + VALUE1] - x) * t;
				y += (frames[i + ENTRIES + VALUE2] - y) * t;
				break;
			case STEPPED:
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
				break;
			default:
				x = GetBezierValue(time, i, VALUE1, curveType - BEZIER);
				y = GetBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
				break;
			}
		}
	}

	/// <summary>Changes a bone's local <see cref"BoneLocal.X"/>.</summary>
	public class TranslateXTimeline : BoneTimeline1 {
		public TranslateXTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.X) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.x = GetRelativeValue(time, alpha, blend, pose.x, setup.x);
		}
	}

	/// <summary>Changes a bone's local <see cref"BoneLocal.Y"/>.</summary>
	public class TranslateYTimeline : BoneTimeline1 {
		public TranslateYTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.Y) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.y = GetRelativeValue(time, alpha, blend, pose.y, setup.y);
		}
	}

	/// <summary>Changes a bone's local <see cref="BoneLocal.ScaleX"/> and <see cref="BoneLocal.ScaleY"/>.</summary>
	public class ScaleTimeline : BoneTimeline2 {

		public ScaleTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.ScaleX, Property.ScaleY) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.Setup:
					pose.scaleX = setup.scaleX;
					pose.scaleY = setup.scaleY;
					return;
				case MixBlend.First:
					pose.scaleX += (setup.scaleX - pose.scaleX) * alpha;
					pose.scaleY += (setup.scaleY - pose.scaleY) * alpha;
					return;
				}
				return;
			}

			float x, y;
			int i = Search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				float before = frames[i];
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				x += (frames[i + ENTRIES + VALUE1] - x) * t;
				y += (frames[i + ENTRIES + VALUE2] - y) * t;
				break;
			case STEPPED:
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
				break;
			default:
				x = GetBezierValue(time, i, VALUE1, curveType - BEZIER);
				y = GetBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
				break;
			}
			x *= setup.scaleX;
			y *= setup.scaleY;

			if (alpha == 1) {
				if (blend == MixBlend.Add) {
					pose.scaleX += x - setup.scaleX;
					pose.scaleY += y - setup.scaleY;
				} else {
					pose.scaleX = x;
					pose.scaleY = y;
				}
			} else {
				// Mixing out uses sign of setup or current pose, else use sign of key.
				float bx, by;
				if (direction == MixDirection.Out) {
					switch (blend) {
					case MixBlend.Setup:
						bx = setup.scaleX;
						by = setup.scaleY;
						pose.scaleX = bx + (Math.Abs(x) * Math.Sign(bx) - bx) * alpha;
						pose.scaleY = by + (Math.Abs(y) * Math.Sign(by) - by) * alpha;
						break;
					case MixBlend.First:
					case MixBlend.Replace:
						bx = pose.scaleX;
						by = pose.scaleY;
						pose.scaleX = bx + (Math.Abs(x) * Math.Sign(bx) - bx) * alpha;
						pose.scaleY = by + (Math.Abs(y) * Math.Sign(by) - by) * alpha;
						break;
					case MixBlend.Add:
						pose.scaleX += (x - setup.scaleX) * alpha;
						pose.scaleY += (y - setup.scaleY) * alpha;
						break;
					}
				} else {
					switch (blend) {
					case MixBlend.Setup:
						bx = Math.Abs(setup.scaleX) * Math.Sign(x);
						by = Math.Abs(setup.scaleY) * Math.Sign(y);
						pose.scaleX = bx + (x - bx) * alpha;
						pose.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.First:
					case MixBlend.Replace:
						bx = Math.Abs(pose.scaleX) * Math.Sign(x);
						by = Math.Abs(pose.scaleY) * Math.Sign(y);
						pose.scaleX = bx + (x - bx) * alpha;
						pose.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.Add:
						pose.scaleX += (x - setup.scaleX) * alpha;
						pose.scaleY += (y - setup.scaleY) * alpha;
						break;
					}
				}
			}
		}
	}

	/// <summary>Changes a bone's local <see cref="BoneLocal.ScaleX"/>.</summary>
	public class ScaleXTimeline : BoneTimeline1 {
		public ScaleXTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.ScaleX) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.scaleX = GetScaleValue(time, alpha, blend, direction, pose.scaleX, setup.scaleX);
		}
	}

	/// <summary>Changes a bone's local <see cref="BoneLocal.ScaleY"/>.</summary>
	public class ScaleYTimeline : BoneTimeline1 {
		public ScaleYTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.ScaleY) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.scaleY = GetScaleValue(time, alpha, blend, direction, pose.scaleY, setup.scaleY);
		}
	}

	/// <summary>Changes a bone's local <see cref="BoneLocal.ShearX"/> and <see cref="BoneLocal.ShearY"/>.</summary>
	public class ShearTimeline : BoneTimeline2 {
		public ShearTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.ShearX, Property.ShearY) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.Setup:
					pose.shearX = setup.shearX;
					pose.shearY = setup.shearY;
					return;
				case MixBlend.First:
					pose.shearX += (setup.shearX - pose.shearX) * alpha;
					pose.shearY += (setup.shearY - pose.shearY) * alpha;
					return;
				}
				return;
			}

			float x, y;
			int i = Search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				float before = frames[i];
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				x += (frames[i + ENTRIES + VALUE1] - x) * t;
				y += (frames[i + ENTRIES + VALUE2] - y) * t;
				break;
			case STEPPED:
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
				break;
			default:
				x = GetBezierValue(time, i, VALUE1, curveType - BEZIER);
				y = GetBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
				break;
			}

			switch (blend) {
			case MixBlend.Setup:
				pose.shearX = setup.shearX + x * alpha;
				pose.shearY = setup.shearY + y * alpha;
				break;
			case MixBlend.First:
			case MixBlend.Replace:
				pose.shearX += (setup.shearX + x - pose.shearX) * alpha;
				pose.shearY += (setup.shearY + y - pose.shearY) * alpha;
				break;
			case MixBlend.Add:
				pose.shearX += x * alpha;
				pose.shearY += y * alpha;
				break;
			}
		}
	}

	/// <summary>Changes a bone's local <see cref="BoneLocal.ShearX"/>.</summary>
	public class ShearXTimeline : BoneTimeline1 {
		public ShearXTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.ShearX) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.shearX = GetRelativeValue(time, alpha, blend, pose.shearX, setup.shearX);
		}
	}

	/// <summary>Changes a bone's local <see cref="BoneLocal.ShearY"/>.</summary>
	public class ShearYTimeline : BoneTimeline1 {
		public ShearYTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, Property.ShearY) {
		}

		override protected void Apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.shearY = GetRelativeValue(time, alpha, blend, pose.shearY, setup.shearY);
		}
	}

	/// <summary>Changes a bone's <see cref="BoneLocal.Inherit"/>.</summary>
	public class InheritTimeline : Timeline, IBoneTimeline {
		public const int ENTRIES = 2;
		private const int INHERIT = 1;

		readonly int boneIndex;

		public InheritTimeline (int frameCount, int boneIndex)
			: base(frameCount, (int)Property.Inherit + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		/// <summary>Sets the inherit transform mode for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, Inherit inherit) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + INHERIT] = (int)inherit;
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {

			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;
			BoneLocal pose = appliedPose ? bone.applied : bone.pose;

			if (direction == MixDirection.Out) {
				if (blend == MixBlend.Setup) pose.inherit = bone.data.setup.inherit;
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (blend == MixBlend.Setup || blend == MixBlend.First) pose.inherit = bone.data.setup.inherit;
			} else
				pose.inherit = InheritEnum.Values[(int)frames[Search(frames, time, ENTRIES) + INHERIT]];
		}
	}

	public abstract class SlotCurveTimeline : CurveTimeline, ISlotTimeline {
		readonly int slotIndex;

		public SlotCurveTimeline (int frameCount, int bezierCount, int slotIndex, params string[] propertyIds)
			: base(frameCount, bezierCount, propertyIds) {
			this.slotIndex = slotIndex;
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
			MixDirection direction, bool appliedPose) {

			Slot slot = skeleton.slots.Items[slotIndex];
			if (slot.bone.active) Apply(slot, appliedPose ? slot.applied : slot.pose, time, alpha, blend);
		}

		abstract protected void Apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend);
	}

	/// <summary>Changes a slot's <see cref="SlotPose.GetColor"/>.</summary>
	public class RGBATimeline : SlotCurveTimeline {
		public const int ENTRIES = 5;
		private const int R = 1, G = 2, B = 3, A = 4;

		public RGBATimeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, slotIndex, //
				(int)Property.RGB + "|" + slotIndex, //
				(int)Property.Alpha + "|" + slotIndex) {
		}
		public override int FrameEntries {
			get { return ENTRIES; }
		}

		/// <summary>Sets the time and color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float r, float g, float b, float a) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
			frames[frame + A] = a;
		}

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			Color32F color = pose.GetColor();
			float[] frames = this.frames;
			if (time < frames[0]) {
				Color32F setup = slot.data.setup.GetColor();
				switch (blend) {
				case MixBlend.Setup:
					color = setup;
					pose.SetColor(color); // required due to Color being a struct
					return;
				case MixBlend.First:
					color += new Color32F((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha);
					color.Clamp();
					pose.SetColor(color); // see above
					break;
				}
				return;
			}

			float r, g, b, a;
			int i = Search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				float before = frames[i];
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				a = frames[i + A];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				r += (frames[i + ENTRIES + R] - r) * t;
				g += (frames[i + ENTRIES + G] - g) * t;
				b += (frames[i + ENTRIES + B] - b) * t;
				a += (frames[i + ENTRIES + A] - a) * t;
				break;
			case STEPPED:
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				a = frames[i + A];
				break;
			default:
				r = GetBezierValue(time, i, R, curveType - BEZIER);
				g = GetBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = GetBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				a = GetBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER);
				break;
			}

			if (alpha == 1) {
				color = new Color32F(r, g, b, a);
			} else {
				if (blend == MixBlend.Setup) {
					Color32F setup = slot.data.setup.GetColor();
					color = new Color32F(setup.r + (r - setup.r) * alpha, setup.g + (g - setup.g) * alpha, setup.b + (b - setup.b) * alpha,
						setup.a + (a - setup.a) * alpha);
				} else
					color += new Color32F((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
			color.Clamp();
			pose.SetColor(color); // see above
		}
	}

	/// <summary>Changes the RGB for a slot's <see cref="SlotPose.GetColor"/>.</summary>
	public class RGBTimeline : SlotCurveTimeline {
		public const int ENTRIES = 4;
		private const int R = 1, G = 2, B = 3;

		public RGBTimeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, slotIndex, (int)Property.RGB + "|" + slotIndex) {
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		/// <summary>Sets the time and color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float r, float g, float b) {
			frame <<= 2;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
		}

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			Color32F color = pose.GetColor();
			float r, g, b;
			float[] frames = this.frames;
			if (time < frames[0]) {
				Color32F setup = slot.data.setup.GetColor();
				switch (blend) {
				case MixBlend.Setup:
					color.r = setup.r;
					color.g = setup.g;
					color.b = setup.b;
					pose.SetColor(color); // required due to Color being a struct
					goto default; // Fall through.
				default:
					return;
				case MixBlend.First:
					r = color.r + (setup.r - color.r) * alpha;
					g = color.g + (setup.g - color.g) * alpha;
					b = color.b + (setup.b - color.b) * alpha;
					break;
				}
			} else {
				int i = Search(frames, time, ENTRIES), curveType = (int)curves[i >> 2];
				switch (curveType) {
				case LINEAR:
					float before = frames[i];
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					float t = (time - before) / (frames[i + ENTRIES] - before);
					r += (frames[i + ENTRIES + R] - r) * t;
					g += (frames[i + ENTRIES + G] - g) * t;
					b += (frames[i + ENTRIES + B] - b) * t;
					break;
				case STEPPED:
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					break;
				default:
					r = GetBezierValue(time, i, R, curveType - BEZIER);
					g = GetBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
					b = GetBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
					break;
				}

				if (alpha != 1) {
					if (blend == MixBlend.Setup) {
						Color32F setup = slot.data.setup.GetColor();
						r = setup.r + (r - setup.r) * alpha;
						g = setup.g + (g - setup.g) * alpha;
						b = setup.b + (b - setup.b) * alpha;
					} else {
						r = color.r + (r - color.r) * alpha;
						g = color.g + (g - color.g) * alpha;
						b = color.b + (b - color.b) * alpha;
					}
				}
			}
			color.r = r;
			color.g = g;
			color.b = b;
			color.ClampRGB();
			pose.SetColor(color); // see above
		}
	}

	/// <summary>Changes the alpha for a slot's <see cref="SlotPose.GetColor"/>.</summary>
	public class AlphaTimeline : CurveTimeline1, ISlotTimeline {
		readonly int slotIndex;

		public AlphaTimeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, (int)Property.Alpha + "|" + slotIndex) {
			this.slotIndex = slotIndex;
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;

			SlotPose pose = (appliedPose ? slot.applied : slot.pose);
			Color32F color = pose.GetColor();
			float a;
			float[] frames = this.frames;
			if (time < frames[0]) {
				Color32F setup = slot.data.setup.GetColor();
				switch (blend) {
				case MixBlend.Setup:
					color.a = setup.a;
					pose.SetColor(color); // required due to Color being a struct
					goto default; // Fall through.
				default:
					return;
				case MixBlend.First:
					a = color.a + (setup.a - color.a) * alpha;
					break;
				}
			} else {
				a = GetCurveValue(time);
				if (alpha != 1) {
					if (blend == MixBlend.Setup) {
						Color32F setup = slot.data.setup.GetColor();
						a = setup.a + (a - setup.a) * alpha;
					} else
						a = color.a + (a - color.a) * alpha;
				}
			}
			color.a = MathUtils.Clamp01(a);
			pose.SetColor(color); // see above
		}
	}

	/// <summary>Changes a slot's <see cref="SlotPose.Color"/> and <see cref="SlotPose.DarkColor"/> for two color tinting.</summary>
	public class RGBA2Timeline : SlotCurveTimeline {
		public const int ENTRIES = 8;
		protected const int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;

		public RGBA2Timeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, slotIndex, //
				(int)Property.RGB + "|" + slotIndex, //
				(int)Property.Alpha + "|" + slotIndex, //
				(int)Property.RGB2 + "|" + slotIndex) {
		}

		public override int FrameEntries {
			get {
				return ENTRIES;
			}
		}

		/// <summary>Sets the time, light color, and dark color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float r, float g, float b, float a, float r2, float g2, float b2) {
			frame <<= 3;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
			frames[frame + A] = a;
			frames[frame + R2] = r2;
			frames[frame + G2] = g2;
			frames[frame + B2] = b2;
		}

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			Color32F light = pose.GetColor();
			Color32F? darkOptional = pose.GetDarkColor();
			float r2, g2, b2;
			float[] frames = this.frames;
			if (time < frames[0]) {
				SlotPose setup = slot.data.setup;
				Color32F setupLight = setup.GetColor();
				Color32F? setupDarkOptional = setup.GetDarkColor();
				switch (blend) {
				case MixBlend.Setup:
					pose.SetColor(setupLight); // required due to Color being a struct
					pose.SetDarkColor(setupDarkOptional);
					goto default; // Fall through.
				default:
					return;
				case MixBlend.First:
					light += new Color32F((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
						(setupLight.a - light.a) * alpha);
					light.Clamp();
					pose.SetColor(light); // required due to Color being a struct

					Color32F dark = darkOptional.Value;
					Color32F setupDark = setupDarkOptional.Value;
					r2 = dark.r + (setupDark.r - dark.r) * alpha;
					g2 = dark.g + (setupDark.g - dark.g) * alpha;
					b2 = dark.b + (setupDark.b - dark.b) * alpha;
					break;
				}
			} else {
				float r, g, b, a;
				int i = Search(frames, time, ENTRIES), curveType = (int)curves[i >> 3];
				switch (curveType) {
				case LINEAR:
					float before = frames[i];
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					a = frames[i + A];
					r2 = frames[i + R2];
					g2 = frames[i + G2];
					b2 = frames[i + B2];
					float t = (time - before) / (frames[i + ENTRIES] - before);
					r += (frames[i + ENTRIES + R] - r) * t;
					g += (frames[i + ENTRIES + G] - g) * t;
					b += (frames[i + ENTRIES + B] - b) * t;
					a += (frames[i + ENTRIES + A] - a) * t;
					r2 += (frames[i + ENTRIES + R2] - r2) * t;
					g2 += (frames[i + ENTRIES + G2] - g2) * t;
					b2 += (frames[i + ENTRIES + B2] - b2) * t;
					break;
				case STEPPED:
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					a = frames[i + A];
					r2 = frames[i + R2];
					g2 = frames[i + G2];
					b2 = frames[i + B2];
					break;
				default:
					r = GetBezierValue(time, i, R, curveType - BEZIER);
					g = GetBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
					b = GetBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
					a = GetBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER);
					r2 = GetBezierValue(time, i, R2, curveType + BEZIER_SIZE * 4 - BEZIER);
					g2 = GetBezierValue(time, i, G2, curveType + BEZIER_SIZE * 5 - BEZIER);
					b2 = GetBezierValue(time, i, B2, curveType + BEZIER_SIZE * 6 - BEZIER);
					break;
				}

				if (alpha == 1) {
					light = new Color32F(r, g, b, a);
					light.Clamp();
					pose.SetColor(light); // required due to Color being a struct
				} else if (blend == MixBlend.Setup) {
					SlotPose setupPose = slot.data.setup;
					Color32F setup = setupPose.GetColor();
					light = new Color32F(setup.r + (r - setup.r) * alpha, setup.g + (g - setup.g) * alpha, setup.b + (b - setup.b) * alpha,
						setup.a + (a - setup.a) * alpha);
					light.Clamp();
					pose.SetColor(light); // see above

					Color32F? setupDark = setupPose.GetDarkColor();
					setup = setupDark.Value;
					r2 = setup.r + (r2 - setup.r) * alpha;
					g2 = setup.g + (g2 - setup.g) * alpha;
					b2 = setup.b + (b2 - setup.b) * alpha;
				} else {
					light += new Color32F((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
					light.Clamp();
					pose.SetColor(light); // see above
					Color32F dark = darkOptional.Value;
					r2 = dark.r + (r2 - dark.r) * alpha;
					g2 = dark.g + (g2 - dark.g) * alpha;
					b2 = dark.b + (b2 - dark.b) * alpha;
				}
			}
			Color32F darkValue = new Color32F(r2, g2, b2);
			darkValue.ClampRGB();
			pose.SetDarkColor(darkValue);
		}
	}

	/// <summary>Changes the RGB for a slot's <see cref="SlotPose.Color"/> and <see cref="SlotPose.DarkColor"/> for two color tinting.</summary>
	public class RGB2Timeline : SlotCurveTimeline {
		public const int ENTRIES = 7;
		protected const int R = 1, G = 2, B = 3, R2 = 4, G2 = 5, B2 = 6;

		public RGB2Timeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, slotIndex, //
				(int)Property.RGB + "|" + slotIndex, //
				(int)Property.RGB2 + "|" + slotIndex) {
		}

		public override int FrameEntries {
			get {
				return ENTRIES;
			}
		}

		/// <summary>Sets the time, light color, and dark color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float r, float g, float b, float r2, float g2, float b2) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
			frames[frame + R2] = r2;
			frames[frame + G2] = g2;
			frames[frame + B2] = b2;
		}

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			Color32F light = pose.GetColor();
			Color32F? darkOptional = pose.GetDarkColor();
			float r, g, b, r2, g2, b2;
			float[] frames = this.frames;
			if (time < frames[0]) {
				SlotPose setup = slot.data.setup;
				Color32F setupLight = setup.GetColor();
				Color32F? setupDarkOptional = setup.GetDarkColor();
				Color32F dark = darkOptional.Value;
				Color32F setupDark = setupDarkOptional.Value;

				switch (blend) {
				case MixBlend.Setup:
					light.r = setupLight.r;
					light.g = setupLight.g;
					light.b = setupLight.b;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;

					pose.SetColor(light); // required due to Color being a struct
					pose.SetDarkColor(dark);
					goto default; // Fall through.
				default:
					return;
				case MixBlend.First:
					r = light.r + (setupLight.r - light.r) * alpha;
					g = light.g + (setupLight.g - light.g) * alpha;
					b = light.b + (setupLight.b - light.b) * alpha;
					r2 = dark.r + (setupDark.r - dark.r) * alpha;
					g2 = dark.g + (setupDark.g - dark.g) * alpha;
					b2 = dark.b + (setupDark.b - dark.b) * alpha;
					break;
				}
			} else {
				int i = Search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
				switch (curveType) {
				case LINEAR:
					float before = frames[i];
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					r2 = frames[i + R2];
					g2 = frames[i + G2];
					b2 = frames[i + B2];
					float t = (time - before) / (frames[i + ENTRIES] - before);
					r += (frames[i + ENTRIES + R] - r) * t;
					g += (frames[i + ENTRIES + G] - g) * t;
					b += (frames[i + ENTRIES + B] - b) * t;
					r2 += (frames[i + ENTRIES + R2] - r2) * t;
					g2 += (frames[i + ENTRIES + G2] - g2) * t;
					b2 += (frames[i + ENTRIES + B2] - b2) * t;
					break;
				case STEPPED:
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					r2 = frames[i + R2];
					g2 = frames[i + G2];
					b2 = frames[i + B2];
					break;
				default:
					r = GetBezierValue(time, i, R, curveType - BEZIER);
					g = GetBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
					b = GetBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
					r2 = GetBezierValue(time, i, R2, curveType + BEZIER_SIZE * 3 - BEZIER);
					g2 = GetBezierValue(time, i, G2, curveType + BEZIER_SIZE * 4 - BEZIER);
					b2 = GetBezierValue(time, i, B2, curveType + BEZIER_SIZE * 5 - BEZIER);
					break;
				}

				if (alpha != 1) {
					Color32F dark = darkOptional.Value;
					if (blend == MixBlend.Setup) {
						SlotPose setupPose = slot.data.setup;
						Color32F setup = setupPose.GetColor();
						r = setup.r + (r - setup.r) * alpha;
						g = setup.g + (g - setup.g) * alpha;
						b = setup.b + (b - setup.b) * alpha;

						Color32F? setupDarkOptional = setupPose.GetDarkColor();
						setup = setupDarkOptional.Value;
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
			}
			light.r = r;
			light.g = g;
			light.b = b;
			light.ClampRGB();
			Color32F darkValue = new Color32F(r2, g2, b2);
			darkValue.ClampRGB();

			pose.SetColor(light); // see above
			pose.SetDarkColor(darkValue);
		}
	}

	/// <summary>Changes a slot's <see cref="SlotPose.Attachment"/>.</summary>
	public class AttachmentTimeline : Timeline, ISlotTimeline {
		readonly int slotIndex;
		readonly string[] attachmentNames;

		public AttachmentTimeline (int frameCount, int slotIndex)
			: base(frameCount, (int)Property.Attachment + "|" + slotIndex) {
			this.slotIndex = slotIndex;
			attachmentNames = new String[frameCount];
		}

		override public int FrameCount {
			get { return frames.Length; }
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <summary>The attachment name for each frame. May contain null values to clear the attachment. </summary>
		public string[] AttachmentNames {
			get {
				return attachmentNames;
			}
		}

		/// <summary>Sets the time and attachment name for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, String attachmentName) {
			frames[frame] = time;
			attachmentNames[frame] = attachmentName;
		}

		public override void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
							MixDirection direction, bool appliedPose) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			SlotPose pose = appliedPose ? slot.applied : slot.pose;

			if (direction == MixDirection.Out) {
				if (blend == MixBlend.Setup) SetAttachment(skeleton, pose, slot.data.attachmentName);
			} else if (time < this.frames[0]) {
				if (blend == MixBlend.Setup || blend == MixBlend.First) SetAttachment(skeleton, pose, slot.data.attachmentName);
			} else
				SetAttachment(skeleton, pose, attachmentNames[Search(this.frames, time)]);
		}

		private void SetAttachment (Skeleton skeleton, SlotPose pose, string attachmentName) {
			pose.Attachment = attachmentName == null ? null : skeleton.GetAttachment(slotIndex, attachmentName);
		}
	}

	/// <summary>Changes a slot's <see cref="SlotPose.Deform"/> to deform a <see cref="VertexAttachment"/>.</summary>
	public class DeformTimeline : SlotCurveTimeline {
		readonly VertexAttachment attachment;
		internal float[][] vertices;

		public DeformTimeline (int frameCount, int bezierCount, int slotIndex, VertexAttachment attachment)
			: base(frameCount, bezierCount, slotIndex, (int)Property.Deform + "|" + slotIndex + "|" + attachment.Id) {
			this.attachment = attachment;
			vertices = new float[frameCount][];
		}

		override public int FrameCount {
			get { return frames.Length; }
		}

		/// <summary>The attachment that will be deformed.</summary>
		/// <seealso cref="VertexAttachment.TimelineAttachment"/>
		public VertexAttachment Attachment {
			get {
				return attachment;
			}
		}

		/// <summary>The vertices for each frame.</summary>
		public float[][] Vertices {
			get {
				return vertices;
			}
		}

		/// <summary>Sets the time and vertices for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		/// <param name="vertices">Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights.</param>
		public void SetFrame (int frame, float time, float[] vertices) {
			frames[frame] = time;
			this.vertices[frame] = vertices;
		}

		/// <param name="value1">Ignored (0 is used for a deform timeline).</param>
		/// <param name="value2">Ignored (1 is used for a deform timeline).</param>
		override public void SetBezier (int bezier, int frame, int value, float time1, float value1, float cx1, float cy1, float cx2,
			float cy2, float time2, float value2) {
			float[] curves = this.curves;
			int i = FrameCount + bezier * BEZIER_SIZE;
			if (value == 0) curves[frame] = BEZIER + i;
			float tmpx = (time1 - cx1 * 2 + cx2) * 0.03f, tmpy = cy2 * 0.03f - cy1 * 0.06f;
			float dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006f, dddy = (cy1 - cy2 + 0.33333333f) * 0.018f;
			float ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
			float dx = (cx1 - time1) * 0.3f + tmpx + dddx * 0.16666667f, dy = cy1 * 0.3f + tmpy + dddy * 0.16666667f;
			float x = time1 + dx, y = dy;
			for (int n = i + BEZIER_SIZE; i < n; i += 2) {
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

		/// <summary>Returns the interpolated percentage for the specified time.</summary>
		/// <param name="frame">The frame before <c>time</c>.</param>
		private float GetCurvePercent (float time, int frame) {
			float[] curves = this.curves;
			int i = (int)curves[frame];
			switch (i) {
			case LINEAR:
				float x = frames[frame];
				return (time - x) / (frames[frame + FrameEntries] - x);
			case STEPPED:
				return 0;
			}
			i -= BEZIER;
			if (curves[i] > time) {
				float x = frames[frame];
				return curves[i + 1] * (time - x) / (curves[i] - x);
			}
			int n = i + BEZIER_SIZE;
			for (i += 2; i < n; i += 2) {
				if (curves[i] >= time) {
					float x = curves[i - 2], y = curves[i - 1];
					return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
				}
			}
			{ // scope added to prevent compile error "float x and y declared in enclosing scope"
				float x = curves[n - 2], y = curves[n - 1];
				return y + (1 - y) * (time - x) / (frames[frame + FrameEntries] - x);
			}
		}

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			VertexAttachment vertexAttachment = pose.attachment as VertexAttachment;
			if (vertexAttachment == null || vertexAttachment.TimelineAttachment != attachment) return;

			ExposedList<float> deformArray = pose.deform;
			if (deformArray.Count == 0) blend = MixBlend.Setup;

			float[][] vertices = this.vertices;
			int vertexCount = vertices[0].Length;

			float[] deform;

			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.Setup:
					deformArray.Clear();
					return;
				case MixBlend.First:
					if (alpha == 1) {
						deformArray.Clear();
						return;
					}

					// Ensure size and preemptively set count.
					if (deformArray.Capacity < vertexCount) deformArray.Capacity = vertexCount;
					deformArray.Count = vertexCount;
					deform = deformArray.Items;

					if (vertexAttachment.bones == null) { // Unweighted vertex positions.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++)
							deform[i] += (setupVertices[i] - deform[i]) * alpha;
					} else { // Weighted deform offsets.
						alpha = 1 - alpha;
						for (int i = 0; i < vertexCount; i++)
							deform[i] *= alpha;
					}
					return;
				}
				return;
			}

			// Ensure size and preemptively set count.
			if (deformArray.Capacity < vertexCount) deformArray.Capacity = vertexCount;
			deformArray.Count = vertexCount;
			deform = deformArray.Items;

			if (time >= frames[frames.Length - 1]) { // Time is after last frame.
				float[] lastVertices = vertices[frames.Length - 1];
				if (alpha == 1) {
					if (blend == MixBlend.Add) {
						if (vertexAttachment.bones == null) { // Unweighted vertex positions, no alpha.
							float[] setupVertices = vertexAttachment.vertices;
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] - setupVertices[i];
						} else { // Weighted deform offsets, no alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i];
						}
					} else { // Vertex positions or deform offsets, no alpha.
						Array.Copy(lastVertices, 0, deform, 0, vertexCount);
					}
				} else {
					switch (blend) {
					case MixBlend.Setup: {
						if (vertexAttachment.bones == null) { // Unweighted vertex positions, with alpha.
							float[] setupVertices = vertexAttachment.vertices;
							for (int i = 0; i < vertexCount; i++) {
								float setup = setupVertices[i];
								deform[i] = setup + (lastVertices[i] - setup) * alpha;
							}
						} else { // Weighted deform offsets, with alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] = lastVertices[i] * alpha;
						}
						break;
					}
					case MixBlend.First:
					case MixBlend.Replace: // Vertex positions or deform offsets, with alpha.
						for (int i = 0; i < vertexCount; i++)
							deform[i] += (lastVertices[i] - deform[i]) * alpha;
						break;
					case MixBlend.Add:
						if (vertexAttachment.bones == null) { // Unweighted vertex positions, no alpha.
							float[] setupVertices = vertexAttachment.vertices;
							for (int i = 0; i < vertexCount; i++)
								deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
						} else { // Weighted deform offsets, alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] * alpha;
						}
						break;
					}
				}
				return;
			}

			int frame = Search(frames, time);
			float percent = GetCurvePercent(time, frame);
			float[] prevVertices = vertices[frame];
			float[] nextVertices = vertices[frame + 1];

			if (alpha == 1) {
				if (blend == MixBlend.Add) {
					if (vertexAttachment.bones == null) { // Unweighted vertex positions, no alpha.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++) {
							float prev = prevVertices[i];
							deform[i] += prev + (nextVertices[i] - prev) * percent - setupVertices[i];
						}
					} else { // Weighted deform offsets, no alpha.
						for (int i = 0; i < vertexCount; i++) {
							float prev = prevVertices[i];
							deform[i] += prev + (nextVertices[i] - prev) * percent;
						}
					}
				} else if (percent == 0) {
					Array.Copy(prevVertices, 0, deform, 0, vertexCount);
				} else { // Vertex positions or deform offsets, no alpha.
					for (int i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						deform[i] = prev + (nextVertices[i] - prev) * percent;
					}
				}
			} else {
				switch (blend) {
				case MixBlend.Setup: {
					if (vertexAttachment.bones == null) { // Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++) {
							float prev = prevVertices[i], setup = setupVertices[i];
							deform[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
						}
					} else { // Weighted deform offsets, with alpha.
						for (int i = 0; i < vertexCount; i++) {
							float prev = prevVertices[i];
							deform[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
						}
					}
					break;
				}
				case MixBlend.First:
				case MixBlend.Replace: { // Vertex positions or deform offsets, with alpha.
					for (int i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
					}
					break;
				}
				case MixBlend.Add:
					if (vertexAttachment.bones == null) { // Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++) {
							float prev = prevVertices[i];
							deform[i] += (prev + (nextVertices[i] - prev) * percent - setupVertices[i]) * alpha;
						}
					} else { // Weighted deform offsets, with alpha.
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

	/// <summary>Changes a slot's <see cref="SlotPose.SequenceIndex"/> for an attachment's <see cref="Sequence"/>.</summary>
	public class SequenceTimeline : Timeline, ISlotTimeline {
		public const int ENTRIES = 3;
		private const int MODE = 1, DELAY = 2;

		readonly int slotIndex;
		readonly IHasSequence attachment;

		public SequenceTimeline (int frameCount, int slotIndex, Attachment attachment)
			: base(frameCount, (int)Property.Sequence + "|" + slotIndex + "|" + ((IHasSequence)attachment).Sequence.Id) {
			this.slotIndex = slotIndex;
			this.attachment = (IHasSequence)attachment;
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <summary>The attachment for which the <see cref="SlotPose.SequenceIndex"/> will be set.</summary>
		/// <seealso cref="Attachment.TimelineAttachment"/>.
		public Attachment Attachment {
			get {
				return (Attachment)attachment;
			}
		}

		/// <summary>Sets the time, mode, index, and frame time for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="delay">Seconds between frames.</param>
		public void SetFrame (int frame, float time, SequenceMode mode, int index, float delay) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + MODE] = (int)mode | (index << 4);
			frames[frame + DELAY] = delay;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
			MixDirection direction, bool appliedPose) {

			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			SlotPose pose = appliedPose ? slot.applied : slot.pose;

			Attachment slotAttachment = pose.attachment;
			IHasSequence hasSequence = slotAttachment as IHasSequence;
			if ((hasSequence == null) || slotAttachment.TimelineAttachment != attachment) return;

			if (direction == MixDirection.Out) {
				if (blend == MixBlend.Setup) pose.SequenceIndex = -1;
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (blend == MixBlend.Setup || blend == MixBlend.First) pose.SequenceIndex = -1;
				return;
			}

			int i = Search(frames, time, ENTRIES);
			float before = frames[i];
			int modeAndIndex = (int)frames[i + MODE];
			float delay = frames[i + DELAY];

			int index = modeAndIndex >> 4, count = hasSequence.Sequence.Regions.Length;
			SequenceMode mode = (SequenceMode)(modeAndIndex & 0xf);
			if (mode != SequenceMode.Hold) {
				index += (int)((time - before) / delay + 0.0001f);
				switch (mode) {
				case SequenceMode.Once:
					index = Math.Min(count - 1, index);
					break;
				case SequenceMode.Loop:
					index %= count;
					break;
				case SequenceMode.Pingpong: {
					int n = (count << 1) - 2;
					index = n == 0 ? 0 : index % n;
					if (index >= count) index = n - index;
					break;
				}
				case SequenceMode.OnceReverse:
					index = Math.Max(count - 1 - index, 0);
					break;
				case SequenceMode.LoopReverse:
					index = count - 1 - (index % count);
					break;
				case SequenceMode.PingpongReverse: {
					int n = (count << 1) - 2;
					index = n == 0 ? 0 : (index + count - 1) % n;
					if (index >= count) index = n - index;
					break;
				} // end case
				}
			}
			pose.SequenceIndex = index;
		}
	}

	/// <summary>Fires an <see cref="Event"/> when specific animation times are reached.</summary>
	public class EventTimeline : Timeline {
		readonly static string[] propertyIds = { ((int)Property.Event).ToString() };
		readonly Event[] events;

		public EventTimeline (int frameCount)
			: base(frameCount, propertyIds) {
			events = new Event[frameCount];
		}

		override public int FrameCount {
			get { return frames.Length; }
		}

		/// <summary>The event for each frame.</summary>
		public Event[] Events {
			get {
				return events;
			}
		}

		/// <summary>Sets the time and event for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		public void SetFrame (int frame, Event e) {
			frames[frame] = e.time;
			events[frame] = e;
		}

		/// <summary>Fires events for frames &gt; <c>lastTime</c> and &lt;= <c>time</c>.</summary>
		public override void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha,
			MixBlend blend, MixDirection direction, bool appliedPose) {

			if (firedEvents == null) return;

			float[] frames = this.frames;
			int frameCount = frames.Length;

			if (lastTime > time) { // Apply after lastTime for looped animations.
				Apply(skeleton, lastTime, int.MaxValue, firedEvents, alpha, blend, direction, appliedPose);
				lastTime = -1f;
			} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return;

			int i;
			if (lastTime < frames[0])
				i = 0;
			else {
				i = Search(frames, lastTime) + 1;
				float frameTime = frames[i];
				while (i > 0) { // Fire multiple events with the same frame.
					if (frames[i - 1] != frameTime) break;
					i--;
				}
			}
			for (; i < frameCount && time >= frames[i]; i++)
				firedEvents.Add(events[i]);
		}
	}

	/// <summary>Changes a skeleton's <see cref="Skeleton.DrawOrder"/>.</summary>
	public class DrawOrderTimeline : Timeline {
		internal static readonly string propertyID = ((int)Property.DrawOrder).ToString();
		internal static readonly string[] propertyIds = { propertyID };

		readonly int[][] drawOrders;

		public DrawOrderTimeline (int frameCount)
			: base(frameCount, propertyIds) {
			drawOrders = new int[frameCount][];
		}

		override public int FrameCount {
			get { return frames.Length; }
		}

		/// <summary>The draw order for each frame. </summary>
		/// <seealso cref="Timeline.SetFrame(int, float, int[])"/>.
		public int[][] DrawOrders {
			get { return drawOrders; }
		}

		/// <summary>Sets the time and draw order for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		/// <param name="drawOrder">Ordered <see cref="Skeleton.Slots"/> indices, or null to use setup pose order.</param>
		public void SetFrame (int frame, float time, int[] drawOrder) {
			frames[frame] = time;
			drawOrders[frame] = drawOrder;
		}

		/// <param name="events">May be null.</param>
		public override void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
							MixDirection direction, bool appliedPose) {

			if (direction == MixDirection.Out) {
				if (blend == MixBlend.Setup) Array.Copy(skeleton.slots.Items, 0, skeleton.drawOrder.Items, 0, skeleton.slots.Count);
				return;
			}

			if (time < frames[0]) {
				if (blend == MixBlend.Setup || blend == MixBlend.First) Array.Copy(skeleton.slots.Items, 0, skeleton.drawOrder.Items, 0, skeleton.slots.Count);
				return;
			}

			int[] drawOrderToSetupIndex = drawOrders[Search(frames, time)];
			if (drawOrderToSetupIndex == null)
				Array.Copy(skeleton.slots.Items, 0, skeleton.drawOrder.Items, 0, skeleton.slots.Count);
			else {
				Slot[] slots = skeleton.slots.Items;
				Slot[] drawOrder = skeleton.drawOrder.Items;
				for (int i = 0, n = drawOrderToSetupIndex.Length; i < n; i++)
					drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		}
	}


	/// <summary>Changes a subset of a skeleton's <see cref="Skeleton.DrawOrder"/>.</summary>
	public class DrawOrderFolderTimeline : Timeline {
		private readonly int[] slots;
		private readonly bool[] inFolder;
		private readonly int[][] drawOrders;

		/// <param name="slots"><see cref="Skeleton.Slots"/> indices controlled by this timeline, in setup order.</param>
		/// <param name="slotCount">The maximum number of slots in the skeleton.</param>
		public DrawOrderFolderTimeline (int frameCount, int[] slots, int slotCount)
			: base(frameCount, PropertyIdsFromSlots(slots)) {
			this.slots = slots;
			drawOrders = new int[frameCount][];
			inFolder = new bool[slotCount];
			foreach (int i in slots)
				inFolder[i] = true;
		}

		static private string[] PropertyIdsFromSlots (int[] slots) {
			int n = slots.Length;
			string[] ids = new string[n];
			for (int i = 0; i < n; i++)
				ids[i] = "d" + slots[i];
			return ids;
		}

		override public int FrameCount {
			get { return frames.Length; }
		}

		/// <summary>The <see cref="Skeleton.Slots"/> indices that this timeline affects, in setup order.</summary>
		public int[] Slots {
			get { return slots; }
		}

		/// <summary>The draw order for each frame. </summary>
		/// <seealso cref="Timeline.SetFrame(int, float, int[])"/>.
		public int[][] DrawOrders {
			get { return drawOrders; }
		}

		/// <summary>Sets the time and draw order for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		/// <param name="drawOrder">Ordered <see cref="Skeleton.Slots"/> indices, or null to use setup pose order.</param>
		public void SetFrame (int frame, float time, int[] drawOrder) {
			frames[frame] = time;
			drawOrders[frame] = drawOrder;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
			MixDirection direction, bool appliedPose) {

			if (direction == MixDirection.Out) {
				if (blend == MixBlend.Setup) setup(skeleton);
			} else if (time < frames[0]) {
				if (blend == MixBlend.Setup || blend == MixBlend.First) setup(skeleton);
			} else {
				int[] order = drawOrders[Search(frames, time)];
				if (order == null)
					setup(skeleton);
				else
					Apply(skeleton, order);
			}
		}

		private void setup (Skeleton skeleton) {
			bool[] inFolder = this.inFolder;
			Slot[] drawOrder = skeleton.drawOrder.Items, allSlots = skeleton.slots.Items;
			int[] slots = this.slots;
			for (int i = 0, found = 0, done = slots.Length; ; i++) {
				if (inFolder[drawOrder[i].data.index]) {
					drawOrder[i] = allSlots[slots[found]];
					if (++found == done) break;
				}
			}
		}

		private void Apply (Skeleton skeleton, int[] order) {
			bool[] inFolder = this.inFolder;
			Slot[] drawOrder = skeleton.drawOrder.Items, allSlots = skeleton.slots.Items;
			int[] slots = this.slots;
			for (int i = 0, found = 0, done = slots.Length; ; i++) {
				if (inFolder[drawOrder[i].data.index]) {
					drawOrder[i] = allSlots[slots[order[found]]];
					if (++found == done) break;
				}
			}
		}
	}

	public interface IConstraintTimeline {
		/// <summary>
		/// The index of the constraint in <see cref="Skeleton.Constraints"/> that will be changed when this timeline is applied.
		/// </summary>
		int ConstraintIndex { get; }
	}

	/// <summary>Changes an IK constraint's <see cref="IkConstraintPose.Mix"/>, <see cref="IkConstraintPose.Softness"/>,
	/// <see cref="IkConstraintPose.BendDirection"/>, <see cref="IkConstraintPose.Stretch"/>, and
	/// <see cref="IkConstraintPose.Compress"/>.</summary>
	public class IkConstraintTimeline : CurveTimeline, IConstraintTimeline {
		public const int ENTRIES = 6;
		private const int MIX = 1, SOFTNESS = 2, BEND_DIRECTION = 3, COMPRESS = 4, STRETCH = 5;

		readonly int constraintIndex;

		public IkConstraintTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, (int)Property.IkConstraint + "|" + constraintIndex) {
			this.constraintIndex = constraintIndex;
		}

		public override int FrameEntries {
			get {
				return ENTRIES;
			}
		}

		public int ConstraintIndex {
			get {
				return constraintIndex;
			}
		}

		/// <summary>Sets the time, mix, softness, bend direction, compress, and stretch for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		/// <param name="bendDirection">1 or -1.</param>
		public void SetFrame (int frame, float time, float mix, float softness, int bendDirection, bool compress,
			bool stretch) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + MIX] = mix;
			frames[frame + SOFTNESS] = softness;
			frames[frame + BEND_DIRECTION] = bendDirection;
			frames[frame + COMPRESS] = compress ? 1 : 0;
			frames[frame + STRETCH] = stretch ? 1 : 0;
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {
			var constraint = (IkConstraint)skeleton.constraints.Items[constraintIndex];
			if (!constraint.active) return;
			IkConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;

			float[] frames = this.frames;
			if (time < frames[0]) {
				IkConstraintPose setup = constraint.data.setup;
				switch (blend) {
				case MixBlend.Setup:
					pose.mix = setup.mix;
					pose.softness = setup.softness;
					pose.bendDirection = setup.bendDirection;
					pose.compress = setup.compress;
					pose.stretch = setup.stretch;
					break;
				case MixBlend.First:
					pose.mix += (setup.mix - pose.mix) * alpha;
					pose.softness += (setup.softness - pose.softness) * alpha;
					pose.bendDirection = setup.bendDirection;
					pose.compress = setup.compress;
					pose.stretch = setup.stretch;
					break;
				}
				return;
			}

			float mix, softness;
			int i = Search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				float before = frames[i];
				mix = frames[i + MIX];
				softness = frames[i + SOFTNESS];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				mix += (frames[i + ENTRIES + MIX] - mix) * t;
				softness += (frames[i + ENTRIES + SOFTNESS] - softness) * t;
				break;
			case STEPPED:
				mix = frames[i + MIX];
				softness = frames[i + SOFTNESS];
				break;
			default:
				mix = GetBezierValue(time, i, MIX, curveType - BEZIER);
				softness = GetBezierValue(time, i, SOFTNESS, curveType + BEZIER_SIZE - BEZIER);
				break;
			}

			if (blend == MixBlend.Setup) {
				IkConstraintPose setup = constraint.data.setup;
				pose.mix = setup.mix + (mix - setup.mix) * alpha;
				pose.softness = setup.softness + (softness - setup.softness) * alpha;
				if (direction == MixDirection.Out) {
					pose.bendDirection = setup.bendDirection;
					pose.compress = setup.compress;
					pose.stretch = setup.stretch;
					return;
				}
			} else {
				pose.mix += (mix - pose.mix) * alpha;
				pose.softness += (softness - pose.softness) * alpha;
				if (direction == MixDirection.Out) return;
			}
			pose.bendDirection = (int)frames[i + BEND_DIRECTION];
			pose.compress = frames[i + COMPRESS] != 0;
			pose.stretch = frames[i + STRETCH] != 0;
		}
	}

	/// <summary>Changes a transform constraint's <see cref="TransformConstraintPose.MixRotate"/>, <see cref="TransformConstraintPose.MixX"/>,
	/// <see cref="TransformConstraintPose.MixY"/>, <see cref="TransformConstraintPose.MixScaleX"/>,
	/// <see cref="TransformConstraintPose.MixScaleY"/>, and <see cref="TransformConstraintPose.MixShearY"/></summary>
	public class TransformConstraintTimeline : CurveTimeline, IConstraintTimeline {
		public const int ENTRIES = 7;
		private const int ROTATE = 1, X = 2, Y = 3, SCALEX = 4, SCALEY = 5, SHEARY = 6;

		readonly int constraintIndex;

		public TransformConstraintTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, (int)Property.TransformConstraint + "|" + constraintIndex) {
			this.constraintIndex = constraintIndex;
		}

		public override int FrameEntries {
			get {
				return ENTRIES;
			}
		}

		public int ConstraintIndex {
			get {
				return constraintIndex;
			}
		}

		/// <summary>Sets the time, rotate mix, translate mix, scale mix, and shear mix for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float mixRotate, float mixX, float mixY, float mixScaleX, float mixScaleY,
			float mixShearY) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + ROTATE] = mixRotate;
			frames[frame + X] = mixX;
			frames[frame + Y] = mixY;
			frames[frame + SCALEX] = mixScaleX;
			frames[frame + SCALEY] = mixScaleY;
			frames[frame + SHEARY] = mixShearY;
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {
			var constraint = (TransformConstraint)skeleton.constraints.Items[constraintIndex];
			if (!constraint.active) return;
			TransformConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;

			float[] frames = this.frames;
			if (time < frames[0]) {
				TransformConstraintPose setup = constraint.data.setup;
				switch (blend) {
				case MixBlend.Setup:
					pose.mixRotate = setup.mixRotate;
					pose.mixX = setup.mixX;
					pose.mixY = setup.mixY;
					pose.mixScaleX = setup.mixScaleX;
					pose.mixScaleY = setup.mixScaleY;
					pose.mixShearY = setup.mixShearY;
					return;
				case MixBlend.First:
					pose.mixRotate += (setup.mixRotate - pose.mixRotate) * alpha;
					pose.mixX += (setup.mixX - pose.mixX) * alpha;
					pose.mixY += (setup.mixY - pose.mixY) * alpha;
					pose.mixScaleX += (setup.mixScaleX - pose.mixScaleX) * alpha;
					pose.mixScaleY += (setup.mixScaleY - pose.mixScaleY) * alpha;
					pose.mixShearY += (setup.mixShearY - pose.mixShearY) * alpha;
					return;
				}
				return;
			}

			// note: reference implementation has code inlined, we re-use GetCurveValue code for root motion.
			float rotate, x, y, scaleX, scaleY, shearY;
			GetCurveValue(out rotate, out x, out y, out scaleX, out scaleY, out shearY, time);

			switch (blend) {
			case MixBlend.Setup: {
				TransformConstraintPose setup = constraint.data.setup;
				pose.mixRotate = setup.mixRotate + (rotate - setup.mixRotate) * alpha;
				pose.mixX = setup.mixX + (x - setup.mixX) * alpha;
				pose.mixY = setup.mixY + (y - setup.mixY) * alpha;
				pose.mixScaleX = setup.mixScaleX + (scaleX - setup.mixScaleX) * alpha;
				pose.mixScaleY = setup.mixScaleY + (scaleY - setup.mixScaleY) * alpha;
				pose.mixShearY = setup.mixShearY + (shearY - setup.mixShearY) * alpha;
				break;
			}
			case MixBlend.First:
			case MixBlend.Replace: {
				pose.mixRotate += (rotate - pose.mixRotate) * alpha;
				pose.mixX += (x - pose.mixX) * alpha;
				pose.mixY += (y - pose.mixY) * alpha;
				pose.mixScaleX += (scaleX - pose.mixScaleX) * alpha;
				pose.mixScaleY += (scaleY - pose.mixScaleY) * alpha;
				pose.mixShearY += (shearY - pose.mixShearY) * alpha;
				break;
			}
			case MixBlend.Add: {
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

		public void GetCurveValue (out float rotate, out float x, out float y,
			out float scaleX, out float scaleY, out float shearY, float time) {

			float[] frames = this.frames;
			int i = Search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				float before = frames[i];
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				scaleX = frames[i + SCALEX];
				scaleY = frames[i + SCALEY];
				shearY = frames[i + SHEARY];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				rotate += (frames[i + ENTRIES + ROTATE] - rotate) * t;
				x += (frames[i + ENTRIES + X] - x) * t;
				y += (frames[i + ENTRIES + Y] - y) * t;
				scaleX += (frames[i + ENTRIES + SCALEX] - scaleX) * t;
				scaleY += (frames[i + ENTRIES + SCALEY] - scaleY) * t;
				shearY += (frames[i + ENTRIES + SHEARY] - shearY) * t;
				break;
			case STEPPED:
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				scaleX = frames[i + SCALEX];
				scaleY = frames[i + SCALEY];
				shearY = frames[i + SHEARY];
				break;
			default:
				rotate = GetBezierValue(time, i, ROTATE, curveType - BEZIER);
				x = GetBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER);
				y = GetBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER);
				scaleX = GetBezierValue(time, i, SCALEX, curveType + BEZIER_SIZE * 3 - BEZIER);
				scaleY = GetBezierValue(time, i, SCALEY, curveType + BEZIER_SIZE * 4 - BEZIER);
				shearY = GetBezierValue(time, i, SHEARY, curveType + BEZIER_SIZE * 5 - BEZIER);
				break;
			}
		}
	}

	abstract public class ConstraintTimeline1 : CurveTimeline1, IConstraintTimeline {
		internal readonly int constraintIndex;

		public ConstraintTimeline1 (int frameCount, int bezierCount, int constraintIndex, Property property)
			: base(frameCount, bezierCount, (int)property + "|" + constraintIndex) {
			this.constraintIndex = constraintIndex;
		}

		public int ConstraintIndex {
			get {
				return constraintIndex;
			}
		}
	}

	/// <summary>Changes a path constraint's <see cref="PathConstraintPose.Position"/>.</summary>
	public class PathConstraintPositionTimeline : ConstraintTimeline1 {

		public PathConstraintPositionTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PathConstraintPosition) {
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {
			var constraint = (PathConstraint)skeleton.constraints.Items[constraintIndex];
			if (constraint.active) {
				PathConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;
				pose.position = GetAbsoluteValue(time, alpha, blend, pose.position, constraint.data.setup.position);
			}
		}
	}

	/// <summary>Changes a path constraint's <see cref="PathConstraintPose.Spacing"/>.</summary>
	public class PathConstraintSpacingTimeline : ConstraintTimeline1 {

		public PathConstraintSpacingTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PathConstraintSpacing) {
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {

			var constraint = (PathConstraint)skeleton.constraints.Items[constraintIndex];
			if (constraint.active) {
				PathConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;
				pose.spacing = GetAbsoluteValue(time, alpha, blend == MixBlend.Add ? MixBlend.Replace : blend, pose.spacing,
					constraint.data.setup.spacing);
			}
		}
	}

	/// <summary>Changes a path constraint's <see cref="PathConstraintPose.MixRotate"/>, <see cref="PathConstraintPose.MixX"/>, and
	/// <see cref="PathConstraintPose.MixY"/>.</summary>
	public class PathConstraintMixTimeline : CurveTimeline, IConstraintTimeline {
		public const int ENTRIES = 4;
		private const int ROTATE = 1, X = 2, Y = 3;

		readonly int constraintIndex;

		public PathConstraintMixTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, (int)Property.PathConstraintMix + "|" + constraintIndex) {
			this.constraintIndex = constraintIndex;
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		public int ConstraintIndex {
			get {
				return constraintIndex;
			}
		}

		/// <summary>Sets the time and color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float mixRotate, float mixX, float mixY) {
			frame <<= 2;
			frames[frame] = time;
			frames[frame + ROTATE] = mixRotate;
			frames[frame + X] = mixX;
			frames[frame + Y] = mixY;
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {
			var constraint = (PathConstraint)skeleton.constraints.Items[constraintIndex];
			if (!constraint.active) return;
			PathConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;

			float[] frames = this.frames;
			if (time < frames[0]) {
				PathConstraintPose setup = constraint.data.setup;
				switch (blend) {
				case MixBlend.Setup:
					pose.mixRotate = setup.mixRotate;
					pose.mixX = setup.mixX;
					pose.mixY = setup.mixY;
					return;
				case MixBlend.First:
					pose.mixRotate += (setup.mixRotate - pose.mixRotate) * alpha;
					pose.mixX += (setup.mixX - pose.mixX) * alpha;
					pose.mixY += (setup.mixY - pose.mixY) * alpha;
					return;
				}
				return;
			}

			float rotate, x, y;
			int i = Search(frames, time, ENTRIES), curveType = (int)curves[i >> 2];
			switch (curveType) {
			case LINEAR:
				float before = frames[i];
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				rotate += (frames[i + ENTRIES + ROTATE] - rotate) * t;
				x += (frames[i + ENTRIES + X] - x) * t;
				y += (frames[i + ENTRIES + Y] - y) * t;
				break;
			case STEPPED:
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				break;
			default:
				rotate = GetBezierValue(time, i, ROTATE, curveType - BEZIER);
				x = GetBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER);
				y = GetBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER);
				break;
			}

			if (blend == MixBlend.Setup) {
				PathConstraintPose setup = constraint.data.setup;
				pose.mixRotate = setup.mixRotate + (rotate - setup.mixRotate) * alpha;
				pose.mixX = setup.mixX + (x - setup.mixX) * alpha;
				pose.mixY = setup.mixY + (y - setup.mixY) * alpha;
			} else {
				pose.mixRotate += (rotate - pose.mixRotate) * alpha;
				pose.mixX += (x - pose.mixX) * alpha;
				pose.mixY += (y - pose.mixY) * alpha;
			}
		}
	}

	/// <summary>The base class for most <see cref="PhysicsConstraint"/> timelines.</summary>
	public abstract class PhysicsConstraintTimeline : ConstraintTimeline1 {
		internal bool additive;

		/// <param name="constraintIndex">-1 for all physics constraints in the skeleton.</param>
		public PhysicsConstraintTimeline (int frameCount, int bezierCount, int constraintIndex, Property property)
			: base(frameCount, bezierCount, constraintIndex, property) {
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
			MixDirection direction, bool appliedPose) {

			if (blend == MixBlend.Add && !additive) blend = MixBlend.Replace;
			if (constraintIndex == -1) {
				float value = time >= frames[0] ? GetCurveValue(time) : 0;
				PhysicsConstraint[] constraints = skeleton.physics.Items;
				for (int i = 0, n = skeleton.physics.Count; i < n; i++) {
					PhysicsConstraint constraint = constraints[i];
					if (constraint.active && Global(constraint.data)) {
						PhysicsConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;
						Set(pose, GetAbsoluteValue(time, alpha, blend, Get(pose), Get(constraint.data.setup), value));
					}
				}
			} else {
				var constraint = (PhysicsConstraint)skeleton.constraints.Items[constraintIndex];
				if (constraint.active) {
					PhysicsConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;
					Set(pose, GetAbsoluteValue(time, alpha, blend, Get(pose), Get(constraint.data.setup)));
				}
			}
		}

		abstract protected float Get (PhysicsConstraintPose pose);

		abstract protected void Set (PhysicsConstraintPose pose, float value);

		abstract protected bool Global (PhysicsConstraintData constraint);
	}

	/// <summary>Changes a physics constraint's <see cref="PhysicsConstraintPose.Inertia"/>.</summary>
	public class PhysicsConstraintInertiaTimeline : PhysicsConstraintTimeline {
		public PhysicsConstraintInertiaTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PhysicsConstraintInertia) {
		}

		override protected float Get (PhysicsConstraintPose pose) {
			return pose.inertia;
		}

		override protected void Set (PhysicsConstraintPose pose, float value) {
			pose.inertia = value;
		}

		override protected bool Global (PhysicsConstraintData constraint) {
			return constraint.inertiaGlobal;
		}
	}

	/// <summary>Changes a physics constraint's <see cref="PhysicsConstraintPose.Strength"/>.</summary>
	public class PhysicsConstraintStrengthTimeline : PhysicsConstraintTimeline {
		public PhysicsConstraintStrengthTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PhysicsConstraintStrength) {
		}

		override protected float Get (PhysicsConstraintPose pose) {
			return pose.strength;
		}

		override protected void Set (PhysicsConstraintPose pose, float value) {
			pose.strength = value;
		}

		override protected bool Global (PhysicsConstraintData constraint) {
			return constraint.strengthGlobal;
		}
	}

	/// <summary>Changes a physics constraint's <see cref="PhysicsConstraintPose.Damping"/>.</summary>
	public class PhysicsConstraintDampingTimeline : PhysicsConstraintTimeline {
		public PhysicsConstraintDampingTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PhysicsConstraintDamping) {
		}

		override protected float Get (PhysicsConstraintPose pose) {
			return pose.damping;
		}

		override protected void Set (PhysicsConstraintPose pose, float value) {
			pose.damping = value;
		}

		override protected bool Global (PhysicsConstraintData constraint) {
			return constraint.dampingGlobal;
		}
	}

	/// <summary>Changes a physics constraint's <see cref="PhysicsConstraintPose.MassInverse"/>. The timeline values are not inverted.</summary>
	public class PhysicsConstraintMassTimeline : PhysicsConstraintTimeline {
		public PhysicsConstraintMassTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PhysicsConstraintMass) {
		}

		override protected float Get (PhysicsConstraintPose pose) {
			return 1 / pose.massInverse;
		}

		override protected void Set (PhysicsConstraintPose pose, float value) {
			pose.massInverse = 1 / value;
		}

		override protected bool Global (PhysicsConstraintData constraint) {
			return constraint.massGlobal;
		}
	}

	/// <summary>Changes a physics constraint's <see cref="PhysicsConstraintPose.Wind"/>.</summary>
	public class PhysicsConstraintWindTimeline : PhysicsConstraintTimeline {
		public PhysicsConstraintWindTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PhysicsConstraintWind) {
			additive = true;
		}

		override protected float Get (PhysicsConstraintPose pose) {
			return pose.wind;
		}

		override protected void Set (PhysicsConstraintPose pose, float value) {
			pose.wind = value;
		}

		override protected bool Global (PhysicsConstraintData constraint) {
			return constraint.windGlobal;
		}
	}

	/// <summary>Changes a physics constraint's <see cref="PhysicsConstraintPose.Gravity"/>.</summary>
	public class PhysicsConstraintGravityTimeline : PhysicsConstraintTimeline {
		public PhysicsConstraintGravityTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PhysicsConstraintGravity) {
			additive = true;
		}

		override protected float Get (PhysicsConstraintPose pose) {
			return pose.gravity;
		}

		override protected void Set (PhysicsConstraintPose pose, float value) {
			pose.gravity = value;
		}

		override protected bool Global (PhysicsConstraintData constraint) {
			return constraint.gravityGlobal;
		}
	}

	/// <summary>Changes a physics constraint's <see cref="PhysicsConstraintPose.Mix"/>.</summary>
	public class PhysicsConstraintMixTimeline : PhysicsConstraintTimeline {
		public PhysicsConstraintMixTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PhysicsConstraintMix) {
		}

		override protected float Get (PhysicsConstraintPose pose) {
			return pose.mix;
		}

		override protected void Set (PhysicsConstraintPose pose, float value) {
			pose.mix = value;
		}

		override protected bool Global (PhysicsConstraintData constraint) {
			return constraint.mixGlobal;
		}
	}

	/// <summary>Resets a physics constraint when specific animation times are reached.</summary>
	public class PhysicsConstraintResetTimeline : Timeline, IConstraintTimeline {
		static readonly string[] propertyIds = { ((int)Property.PhysicsConstraintReset).ToString() };

		readonly int constraintIndex;

		/// <param name="constraintIndex">-1 for all physics constraints in the skeleton.</param>
		public PhysicsConstraintResetTimeline (int frameCount, int constraintIndex)
			: base(frameCount, propertyIds) {
			this.constraintIndex = constraintIndex;
		}

		/// <summary>The index of the physics constraint in <see cref="Skeleton.Constraints"/> that will be reset when this timeline is
		/// applied, or -1 if all physics constraints in the skeleton will be reset.</summary>
		public int ConstraintIndex {
			get {
				return constraintIndex;
			}
		}

		override public int FrameCount {
			get { return frames.Length; }
		}

		/// <summary>Sets the time for the specified frame.</summary>
		/// <param name="frame">Between 0 and <c>frameCount</c>, inclusive.</param>
		public void SetFrame (int frame, float time) {
			frames[frame] = time;
		}

		/// <summary>Resets the physics constraint when frames > <c>lastTime</c> and <= <c>time</c>.</summary>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {

			PhysicsConstraint constraint = null;
			if (constraintIndex != -1) {
				constraint = (PhysicsConstraint)skeleton.constraints.Items[constraintIndex];
				if (!constraint.active) return;
			}

			float[] frames = this.frames;

			if (lastTime > time) { // Apply after lastTime for looped animations.
				Apply(skeleton, lastTime, int.MaxValue, null, alpha, blend, direction, appliedPose);
				lastTime = -1f;
			} else if (lastTime >= frames[frames.Length - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return;

			if (lastTime < frames[0] || time >= frames[Search(frames, lastTime) + 1]) {
				if (constraint != null)
					constraint.Reset(skeleton);
				else {
					PhysicsConstraint[] constraints = skeleton.physics.Items;
					for (int i = 0, n = skeleton.physics.Count; i < n; i++) {
						constraint = constraints[i];
						if (constraint.active) constraint.Reset(skeleton);
					}
				}
			}
		}
	}

	/// <summary>
	/// Changes a slider's <see cref="SliderPose.Time"/>.
	/// </summary>
	public class SliderTimeline : ConstraintTimeline1 {
		public SliderTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.SliderTime) {
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
									MixDirection direction, bool appliedPose) {

			var constraint = (Slider)skeleton.constraints.Items[constraintIndex];
			if (constraint.active) {
				SliderPose pose = appliedPose ? constraint.applied : constraint.pose;
				pose.time = GetAbsoluteValue(time, alpha, blend, pose.time, constraint.data.setup.time);
			}
		}
	}

	/// <summary>
	/// Changes a slider's <see cref="SliderPose.Mix"/>.
	/// </summary>
	public class SliderMixTimeline : ConstraintTimeline1 {
		public SliderMixTimeline (int frameCount, int bezierCount, int constraintIndex)
		: base(frameCount, bezierCount, constraintIndex, Property.SliderMix) {
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend,
										MixDirection direction, bool appliedPose) {

			var constraint = (Slider)skeleton.constraints.Items[constraintIndex];
			if (constraint.active) {
				SliderPose pose = appliedPose ? constraint.applied : constraint.pose;
				pose.mix = GetAbsoluteValue(time, alpha, blend, pose.mix, constraint.data.setup.mix);
			}
		}
	}
}
