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
	/// Stores a list of timelines to animate a skeleton's pose over time.
	/// <para>
	/// See <see href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</see> in the Spine
	/// Runtimes Guide.</para></summary>
	public class Animation {
		internal string name;
		internal float duration;
		internal ExposedList<Timeline> timelines;
		internal HashSet<ulong> timelineIds;
		internal ExposedList<int> bones;

		/// <summary>Creates a new animation. <see cref="timelines"/> must be set before use.</summary>
		public Animation (string name) {
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
			this.name = name;
		}

		/// <summary>
		/// If this list or the timelines it contains are modified, the timelines must be set again to recompute the animation's bone
		/// indices and timeline property IDs.
		/// <para>See <see cref="SetTimelines(ExposedList{Timeline}, ExposedList{int})"/>.</para>
		/// </summary>
		public ExposedList<Timeline> Timelines {
			get { return timelines; }
		}

		/// <summary>
		/// Sets the <see cref="timelines"/> and <see cref="bones"/>.
		/// </summary>
		public void SetTimelines (ExposedList<Timeline> timelines, ExposedList<int> bones) {
			if (timelines == null) throw new ArgumentNullException("timelines", "timelines cannot be null.");
			if (bones == null) throw new ArgumentNullException("bones", "bones cannot be null.");
			this.timelines = timelines;
			this.bones = bones;

			int n = timelines.Count;
			// Note: Difference to libgdx reference implementation.
			// Avoiding reallocations by adding all hash set entries at
			// once (EnsureCapacity() is only available in newer .Net versions).
			// Reference implementation:
			// if (timelineIds == null)
			//     timelineIds = new LongSet(n << 1);
			// else
			//     timelineIds.clear(n << 1);
			// Timeline[] items = timelines.items;
			// for (int i = 0; i < n; i++)
			//     timelineIds.addAll(items[i].propertyIds);
			Timeline[] items = timelines.Items;
			int idCount = 0;
			for (int i = 0; i < n; ++i)
				idCount += items[i].propertyIds.Length;
			var propertyIds = new ulong[idCount];
			int currentId = 0;
			for (int i = 0; i < n; ++i) {
				Timeline timeline = items[i];
				ulong[] ids = items[i].propertyIds;
				for (int ii = 0, idsLength = ids.Length; ii < idsLength; ++ii)
					propertyIds[currentId++] = ids[ii];
			}
			this.timelineIds = new HashSet<ulong>(propertyIds);
			// End of difference to reference implementation
		}

		/// <summary>Returns true if this animation contains a timeline with any of the specified property IDs.
		/// <para>See <see cref="Timeline.PropertyIds"/>.</para></summary>
		public bool HasTimeline (ulong[] propertyIds) {
			foreach (ulong id in propertyIds)
				if (timelineIds.Contains(id)) return true;
			return false;
		}

		/// <summary>The duration of the animation in seconds, which is usually the highest time of all frames in the timelines. The duration is
		/// used to know when the animation has completed and, for animations that repeat, when it should loop back to the start.</summary>
		public float Duration { get { return duration; } set { duration = value; } }

		/// <summary><see cref="Skeleton.Bones"/> indices that this animation's timelines modify.
		/// <para>See <see cref="SetTimelines(ExposedList{Timeline}, ExposedList{int})"/> and <see cref="IBoneTimeline.BoneIndex"/>.</para>
		/// </summary>
		public ExposedList<int> Bones {
			get { return bones; }
		}

		/// <summary>Applies the animation's timelines to the specified skeleton.</summary>
		/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, bool, bool, bool, bool)"/>
		/// <seealso href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations in the Spine
		/// Runtimes Guide.</seealso>
		/// <param name="skeleton">The skeleton the animation is applied to. This provides access to the bones, slots, and other skeleton
		///					components the timelines may change.</param>
		/// <param name="lastTime">The last time in seconds this animation was applied. Some timelines trigger only at discrete times, in which
		/// 				case all keys are triggered between <c>lastTime</c> (exclusive) and <c>time</c> (inclusive). Pass -1
		/// 				the first time an animation is applied to ensure frame 0 is triggered.</param>
		/// <param name="time">The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and
		/// 				interpolate between the frame values.</param>
		/// <param name="loop">True if <c>time</c> beyond the <see cref="Duration"/> repeats the animation, else the last frame is used.</param>
		/// <param name="events">If any events are fired, they are added to this list. Pass null to ignore fired events or if no timelines fire
		/// 				events.</param>
		/// <param name="alpha">0 applies setup or current values (depending on <c>fromSetup</c>), 1 uses timeline values, and
		/// 				intermediate values interpolate between them.Adjusting<c> alpha</c> over time can mix an animation in or
		/// 				out.</param>
		/// <param name="fromSetup">If true, <c>alpha</c> transitions between setup and timeline values, setup values are used before the
		/// 				first frame (current values are not used). If false, <c>alpha</c> transitions between current and timeline
		/// 				values, no change is made before the first frame.</param>
		/// <param name="add">If true, for timelines that support it, their values are added to the setup or current values (depending on
		/// 				<c>fromSetup</c>).</param>
		/// <param name="mixOut">True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant transitions.</param>
		/// <param name="appliedPose">True to modify <see cref="Posed.AppliedPose"/>, else <see cref="Posed.Pose"/> is modified.</param>
		public void Apply (Skeleton skeleton, float lastTime, float time, bool loop, ExposedList<Event> events, float alpha,
							bool fromSetup, bool add, bool mixOut, bool appliedPose) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				if (lastTime > 0) lastTime %= duration;
			}

			Timeline[] timelines = this.timelines.Items;
			for (int i = 0, n = this.timelines.Count; i < n; i++)
				timelines[i].Apply(skeleton, lastTime, time, events, alpha, fromSetup, add, mixOut, appliedPose);
		}

		/// <summary>The animation's name, unique across all animations in the skeleton.
		/// <para>See <see cref="SkeletonData.FindAnimation(string)"/>.</para></summary>
		public string Name { get { return name; } }

		override public string ToString () {
			return name;
		}
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
	/// The base class for all timelines.
	/// <para>
	/// See <see href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</see> in the Spine
	/// Runtimes Guide.</para></summary>
	public abstract class Timeline {
		internal readonly ulong[] propertyIds;
		internal readonly float[] frames;
		internal bool additive, instant;

		/// <param name="propertyIds">Unique identifiers for the properties the timeline modifies.</param>
		public Timeline (int frameCount, params ulong[] propertyIds) {
			if (propertyIds == null) throw new System.ArgumentNullException("propertyIds", "propertyIds cannot be null.");
			this.propertyIds = propertyIds;
			frames = new float[frameCount * FrameEntries];
		}

		/// <summary>Uniquely encodes both the type of this timeline and the skeleton properties that it affects.</summary>
		public ulong[] PropertyIds {
			get { return propertyIds; }
		}

		/// <summary>The time in seconds and any other values for each frame.</summary>
		public float[] Frames {
			get { return frames; }
		}

		/// <summary>The number of values stored per frame.</summary>
		public virtual int FrameEntries {
			get { return 1; }
		}

		/// <summary>The number of frames in this timeline.</summary>
		public virtual int FrameCount {
			get { return frames.Length / FrameEntries; }
		}

		/// <summary>The duration of the timeline in seconds, which is usually the highest time of all frames in the timeline.</summary>
		public float Duration {
			get {
				return frames[frames.Length - FrameEntries];
			}
		}

		/// <summary>True if this timeline supports additive blending.</summary>
		public bool Additive {
			get {
				return additive;
			}
		}

		/// <summary>True if this timeline sets values instantaneously and does not support interpolation between frames.</summary>
		public bool Instant {
			get {
				return instant;
			}
		}

		/// <summary>Applies this timeline to the skeleton.</summary>
		/// <seealso href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations in the Spine
		/// Runtimes Guide.</seealso>
		/// <param name="skeleton">The skeleton the timeline is applied to. This provides access to the bones, slots, and other skeleton
		/// 				components the timelines may change.</param>
		/// <param name="lastTime">The last time in seconds this timeline was applied. Some timelines trigger only at discrete times, in
		/// 				which case all keys are triggered between<c> lastTime</c> (exclusive) and <c>time</c> (inclusive).
		/// 				Pass -1 the first time a timeline is applied to ensure frame 0 is triggered.
		/// <param name="time">The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and
		/// 				interpolate between the frame values.</param>
		/// <param name="events">If any events are fired, they are added to this list. Pass null to ignore fired events or if no timelines
		/// 				fire events.</param>
		/// <param name="alpha">0 applies setup or current values (depending on <c>fromSetup</c>), 1 uses timeline values, and
		/// 				intermediate values interpolate between them.Adjusting<c> alpha</c> over time can mix a timeline in or
		/// 				out.</param>
		/// <param name="blend">Controls how mixing is applied when <c>alpha</c> &lt; 1.</param>
		/// <param name="direction">Indicates whether the timeline is mixing in or out. Used by timelines which perform instant transitions,
		///                   such as <see cref="DrawOrderTimeline"/> or <see cref="AttachmentTimeline"/>, and other such as <see cref="ScaleTimeline"/>.</param>
		/// <param name="fromSetup">If true, <c>alpha</c> transitions between setup and timeline values, setup values are used before
		///                   the first frame (current values are not used). If false, <c>alpha</c> transitions between current and
		///                   timeline values, no change is made before the first frame.</param>
		/// <param name="add">If true, for timelines that support it, their values are added to the setup or current values (depending on
		///                   <c>fromSetup</c>).</param>
		/// <param name="mixOut">True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant
		///                   transitions.</param>
		/// <param name="appliedPose">True to modify <see cref="Posed.AppliedPose"/>, else <see cref="Posed.Pose"/> is modified.</param>
		public abstract void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha,
			bool fromSetup, bool add, bool mixOut, bool appliedPose);

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

	/// <summary>An interface for timelines that change a slot's properties.</summary>
	public interface ISlotTimeline {
		/// <summary>The index of the slot in <see cref="Skeleton.Slots"/> that will be changed when this timeline is applied.</summary>
		int SlotIndex { get; }
	}

	/// <summary>The base class for timelines that interpolate between frame values using stepped, linear, or a Bezier curve.</summary>
	public abstract class CurveTimeline : Timeline {
		public const int LINEAR = 0, STEPPED = 1, BEZIER = 2;
		/// <summary>The number of values stored for each 10 segment Bezier curve.</summary>
		public const int BEZIER_SIZE = 18;

		internal float[] curves;
		/// <summary>The number of key frames for this timeline.</summary>

		/// <param name="bezierCount">The maximum number of Bezier curves. See <see cref="Shrink(int)"/>.</param>
		/// <param name="propertyIds">Unique identifiers for the properties the timeline modifies.</param>
		public CurveTimeline (int frameCount, int bezierCount, params ulong[] propertyIds)
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

	/// <summary>The base class for a <see cref="CurveTimeline"/> that sets one property with a curve.</summary>
	public abstract class CurveTimeline1 : CurveTimeline {
		public const int ENTRIES = 2;
		internal const int VALUE = 1;

		/// <param name="bezierCount">The maximum number of Bezier curves. See <see cref="Shrink(int)"/>.</param>
		/// <param name="propertyIds">Unique identifiers for the properties the timeline modifies.</param>
		public CurveTimeline1 (int frameCount, int bezierCount, ulong propertyId)
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

		/// <summary>Returns the interpolated value for properties relative to the setup value. The timeline value is added to the setup
		/// value, rather than replacing it.</summary>
		/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList{Event}, float, bool, bool, bool, bool)"/>
		/// <param name="current">The current value for the property.</param>
		/// <param name="setup">The setup value for the property.</param>
		public float GetRelativeValue (float time, float alpha, bool fromSetup, bool add, float current, float setup) {
			if (time < frames[0]) return fromSetup ? setup : current;
			float value = GetCurveValue(time);
			return fromSetup ? setup + value * alpha : current + (add ? value : value + setup - current) * alpha;
		}

		/// <summary>Returns the interpolated value for properties set as absolute values. The timeline value replaces the setup value,
		/// rather than being relative to it.</summary>
		/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList{Event}, float, bool, bool, bool, bool)"/>
		/// <param name="current">The current value for the property.</param>
		/// <param name="setup">The setup value for the property.</param>
		public float GetAbsoluteValue (float time, float alpha, bool fromSetup, bool add, float current, float setup) {
			if (time < frames[0]) return fromSetup ? setup : current;
			float value = GetCurveValue(time);
			return fromSetup ? setup + (value - setup) * alpha : current + (add ? value : value - current) * alpha;
		}

		/// <summary>Returns the interpolated value for properties set as absolute values, using the specified timeline value rather than
		/// calling <see cref="GetCurveValue(float)"/>.</summary>
		/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList{Event}, float, bool, bool, bool, bool)"/>
		/// <param name="current">The current value for the property.</param>
		/// <param name="setup">The setup value for the property.</param>
		/// <param name="value">The timeline value to apply.</param>
		public float GetAbsoluteValue (float time, float alpha, bool fromSetup, bool add, float current, float setup,
			float value) {
			if (time < frames[0]) return fromSetup ? setup : current;
			return fromSetup ? setup + (value - setup) * alpha : current + (add ? value : value - current) * alpha;
		}

		/// <summary>Returns the interpolated value for scale properties. The timeline and setup values are multiplied and sign adjusted.</summary>
		/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList{Event}, float, bool, bool, bool, bool)"/>
		/// <param name="current">The current value for the property.</param>
		/// <param name="setup">The setup value for the property.</param>
		public float GetScaleValue (float time, float alpha, bool fromSetup, bool add, bool mixOut, float current,
			float setup) {
			if (time < frames[0]) return fromSetup ? setup : current;
			float value = GetCurveValue(time) * setup;
			if (alpha == 1 && !add) return value;
			float baseValue = fromSetup ? setup : current;
			if (add) return baseValue + (value - setup) * alpha;
			if (mixOut) return baseValue + (Math.Abs(value) * Math.Sign(baseValue) - baseValue) * alpha;
			baseValue = Math.Abs(baseValue) * Math.Sign(value);
			return baseValue + (value - baseValue) * alpha;
		}
	}

	/// <summary>An interface for timelines that change a bone's properties.</summary>
	public interface IBoneTimeline {
		/// <summary>The index of the bone in <see cref="Skeleton.Bones"/> that is changed by this timeline.</summary>
		int BoneIndex { get; }
	}

	/// <summary>The base class for timelines that change 1 bone property with a curve.</summary>
	public abstract class BoneTimeline1 : CurveTimeline1, IBoneTimeline {
		readonly int boneIndex;

		public BoneTimeline1 (int frameCount, int bezierCount, int boneIndex, ulong property)
			: base(frameCount, bezierCount, property << 53 | (uint)boneIndex) {
			this.boneIndex = boneIndex;
			additive = true;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup,
			bool add, bool mixOut, bool appliedPose) {

			Bone bone = skeleton.bones.Items[boneIndex];
			if (bone.active) Apply(appliedPose ? bone.appliedPose : bone.pose, bone.data.setupPose, time, alpha, fromSetup, add, mixOut);
		}

		abstract protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add,
			bool mixOut);
	}

	/// <summary>The base class for timelines that change two bone properties with a curve.</summary>
	public abstract class BoneTimeline2 : CurveTimeline, IBoneTimeline {
		public const int ENTRIES = 3;
		internal const int VALUE1 = 1, VALUE2 = 2;

		readonly int boneIndex;

		/// <param name="bezierCount">The maximum number of Bezier curves. See <see cref="Shrink(int)"/>.</param>
		public BoneTimeline2 (int frameCount, int bezierCount, int boneIndex, ulong property1, ulong property2)
			: base(frameCount, bezierCount, property1 << 53 | (uint)boneIndex, property2 << 53 | (uint)boneIndex) {
			this.boneIndex = boneIndex;
			additive = true;
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
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup, bool add,
			bool mixOut, bool appliedPose) {

			Bone bone = skeleton.bones.Items[boneIndex];
			if (bone.active) Apply(appliedPose ? bone.appliedPose : bone.pose, bone.data.setupPose, time, alpha, fromSetup, add, mixOut);
		}

		abstract protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add,
			bool mixOut);
	}

	/// <summary>Changes <see cref="BonePose.Rotation"/>.</summary>
	public class RotateTimeline : BoneTimeline1, IBoneTimeline {
		public RotateTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.Rotate) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add,
			bool mixOut) {
			pose.rotation = GetRelativeValue(time, alpha, fromSetup, add, pose.rotation, setup.rotation);
		}
	}

	/// <summary>Changes <see cref="BonePose.X"/> and <see cref="BonePose.Y"/>.</summary>
	public class TranslateTimeline : BoneTimeline2 {
		public TranslateTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.X, (ulong)Property.Y) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add, bool mixOut) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					pose.x = setup.x;
					pose.y = setup.y;
				}
				return;
			}

			float x, y;
			// note: reference implementation has code inlined, we re-use GetCurveValue code for root motion.
			GetCurveValue(out x, out y, time);

			if (fromSetup) {
				pose.x = setup.x + x * alpha;
				pose.y = setup.y + y * alpha;
			} else if (add) {
				pose.x += x * alpha;
				pose.y += y * alpha;
			} else {
				pose.x += (setup.x + x - pose.x) * alpha;
				pose.y += (setup.y + y - pose.y) * alpha;
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

	/// <summary>Changes <see cref="BonePose.X"/>.</summary>
	public class TranslateXTimeline : BoneTimeline1 {
		public TranslateXTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.X) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add, bool mixOut) {
			pose.x = GetRelativeValue(time, alpha, fromSetup, add, pose.x, setup.x);
		}
	}

	/// <summary>Changes <see cref="BonePose.Y"/>.</summary>
	public class TranslateYTimeline : BoneTimeline1 {
		public TranslateYTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.Y) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add, bool mixOut) {
			pose.y = GetRelativeValue(time, alpha, fromSetup, add, pose.y, setup.y);
		}
	}

	/// <summary>Changes <see cref="BonePose.ScaleX"/> and <see cref="BonePose.ScaleY"/>.</summary>
	public class ScaleTimeline : BoneTimeline2 {

		public ScaleTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.ScaleX, (ulong)Property.ScaleY) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add, bool mixOut) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					pose.scaleX = setup.scaleX;
					pose.scaleY = setup.scaleY;
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

			if (alpha == 1 && !add) {
				pose.scaleX = x;
				pose.scaleY = y;
			} else {
				float bx, by;
				if (fromSetup) {
					bx = setup.scaleX;
					by = setup.scaleY;
				} else {
					bx = pose.scaleX;
					by = pose.scaleY;
				}
				if (add) {
					pose.scaleX = bx + (x - setup.scaleX) * alpha;
					pose.scaleY = by + (y - setup.scaleY) * alpha;
				} else if (mixOut) {
					pose.scaleX = bx + (Math.Abs(x) * Math.Sign(bx) - bx) * alpha;
					pose.scaleY = by + (Math.Abs(y) * Math.Sign(by) - by) * alpha;
				} else {
					bx = Math.Abs(bx) * Math.Sign(x);
					by = Math.Abs(by) * Math.Sign(y);
					pose.scaleX = bx + (x - bx) * alpha;
					pose.scaleY = by + (y - by) * alpha;
				}
			}
		}
	}

	/// <summary>Changes <see cref="BonePose.ScaleX"/>.</summary>
	public class ScaleXTimeline : BoneTimeline1 {
		public ScaleXTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.ScaleX) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add,
			bool mixOut) {
			pose.scaleX = GetScaleValue(time, alpha, fromSetup, add, mixOut, pose.scaleX, setup.scaleX);
		}
	}

	/// <summary>Changes <see cref="BonePose.ScaleY"/>.</summary>
	public class ScaleYTimeline : BoneTimeline1 {
		public ScaleYTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.ScaleY) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add,
			bool mixOut) {
			pose.scaleY = GetScaleValue(time, alpha, fromSetup, add, mixOut, pose.scaleY, setup.scaleY);
		}
	}

	/// <summary>Changes <see cref="BonePose.ShearX"/> and <see cref="BonePose.ShearY"/>.</summary>
	public class ShearTimeline : BoneTimeline2 {
		public ShearTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.ShearX, (ulong)Property.ShearY) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add, bool mixOut) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					pose.shearX = setup.shearX;
					pose.shearY = setup.shearY;
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

			if (fromSetup) {
				pose.shearX = setup.shearX + x * alpha;
				pose.shearY = setup.shearY + y * alpha;
			} else if (add) {
				pose.shearX += x * alpha;
				pose.shearY += y * alpha;
			} else {
				pose.shearX += (setup.shearX + x - pose.shearX) * alpha;
				pose.shearY += (setup.shearY + y - pose.shearY) * alpha;
			}
		}
	}

	/// <summary>Changes <see cref="BonePose.ShearX"/>.</summary>
	public class ShearXTimeline : BoneTimeline1 {
		public ShearXTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.ShearX) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add,
			bool mixOut) {
			pose.shearX = GetRelativeValue(time, alpha, fromSetup, add, pose.shearX, setup.shearX);
		}
	}

	/// <summary>Changes <see cref="BonePose.ShearY"/>.</summary>
	public class ShearYTimeline : BoneTimeline1 {
		public ShearYTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, boneIndex, (ulong)Property.ShearY) {
		}

		override protected void Apply (BonePose pose, BonePose setup, float time, float alpha, bool fromSetup, bool add,
			bool mixOut) {
			pose.shearY = GetRelativeValue(time, alpha, fromSetup, add, pose.shearY, setup.shearY);
		}
	}

	/// <summary>Changes <see cref="BonePose.Inherit"/>.</summary>
	public class InheritTimeline : Timeline, IBoneTimeline {
		public const int ENTRIES = 2;
		private const int INHERIT = 1;

		readonly int boneIndex;

		public InheritTimeline (int frameCount, int boneIndex)
			: base(frameCount, (ulong)Property.Inherit << 53 | (uint)boneIndex) {
			this.boneIndex = boneIndex;
			instant = true;
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
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup, bool add,
									bool mixOut, bool appliedPose) {

			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;
			BonePose pose = appliedPose ? bone.appliedPose : bone.pose;

			if (mixOut) {
				if (fromSetup) pose.inherit = bone.data.setupPose.inherit;
			} else {
				float[] frames = this.frames;
				if (time < frames[0]) {
					if (fromSetup) pose.inherit = bone.data.setupPose.inherit;
				} else
					pose.inherit = InheritEnum.Values[(int)frames[Search(frames, time, ENTRIES) + INHERIT]];
			}
		}
	}

	/// <summary>The base class for timelines that change any number of slot properties with a curve.</summary>
	public abstract class SlotCurveTimeline : CurveTimeline, ISlotTimeline {
		readonly int slotIndex;

		public SlotCurveTimeline (int frameCount, int bezierCount, int slotIndex, params ulong[] propertyIds)
			: base(frameCount, bezierCount, propertyIds) {
			this.slotIndex = slotIndex;
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup, bool add,
			bool mixOut, bool appliedPose) {

			Slot slot = skeleton.slots.Items[slotIndex];
			if (slot.bone.active) Apply(slot, appliedPose ? slot.appliedPose : slot.pose, time, alpha, fromSetup, add);
		}

		abstract protected void Apply (Slot slot, SlotPose pose, float time, float alpha, bool fromSetup, bool add);
	}

	/// <summary>Changes a slot's <see cref="SlotPose.GetColor()">color</see>.</summary>
	public class RGBATimeline : SlotCurveTimeline {
		public const int ENTRIES = 5;
		private const int R = 1, G = 2, B = 3, A = 4;

		public RGBATimeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, slotIndex, //
				(ulong)Property.RGB << 53 | (uint)slotIndex, //
				(ulong)Property.Alpha << 53 | (uint)slotIndex) {
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

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, bool fromSetup, bool add) {
			Color32F color = pose.GetColor();
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					color = slot.data.setupPose.GetColor();
					pose.SetColor(color); // required due to Color being a struct
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
				if (fromSetup) {
					Color32F setup = slot.data.setupPose.GetColor();
					color = new Color32F(setup.r + (r - setup.r) * alpha, setup.g + (g - setup.g) * alpha, setup.b + (b - setup.b) * alpha,
						setup.a + (a - setup.a) * alpha);
				} else
					color += new Color32F((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
			color.Clamp();
			pose.SetColor(color); // see above
		}
	}

	/// <summary>Changes RGB for a slot's <see cref="SlotPose.GetColor()">color</see>.</summary>
	public class RGBTimeline : SlotCurveTimeline {
		public const int ENTRIES = 4;
		private const int R = 1, G = 2, B = 3;

		public RGBTimeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, slotIndex, (ulong)Property.RGB << 53 | (uint)slotIndex) {
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

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, bool fromSetup, bool add) {
			Color32F color = pose.GetColor();
			float r, g, b;
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					Color32F setup = slot.data.setupPose.GetColor();
					color.r = setup.r;
					color.g = setup.g;
					color.b = setup.b;
					pose.SetColor(color); // required due to Color being a struct
				}
				return;
			}

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
				if (fromSetup) {
					Color32F setup = slot.data.setupPose.GetColor();
					r = setup.r + (r - setup.r) * alpha;
					g = setup.g + (g - setup.g) * alpha;
					b = setup.b + (b - setup.b) * alpha;
				} else {
					r = color.r + (r - color.r) * alpha;
					g = color.g + (g - color.g) * alpha;
					b = color.b + (b - color.b) * alpha;
				}
			}


			color.r = r;
			color.g = g;
			color.b = b;
			color.ClampRGB();
			pose.SetColor(color); // see above
		}
	}

	/// <summary>Changes alpha for a slot's <see cref="SlotPose.GetColor()">color</see>.</summary>
	public class AlphaTimeline : CurveTimeline1, ISlotTimeline {
		readonly int slotIndex;

		public AlphaTimeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, (ulong)Property.Alpha << 53 | (uint)slotIndex) {
			this.slotIndex = slotIndex;
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup,
			bool add, bool mixOut, bool appliedPose) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;

			SlotPose pose = (appliedPose ? slot.appliedPose : slot.pose);
			Color32F color = pose.GetColor();
			float a;
			float[] frames = this.frames;
			if (time < frames[0]) {
				Color32F setup = slot.data.setupPose.GetColor();
				if (fromSetup) {
					color.a = setup.a;
					pose.SetColor(color); // required due to Color being a struct
				}
				return;
			}

			a = GetCurveValue(time);
			if (alpha != 1) {
				if (fromSetup) {
					Color32F setup = slot.data.setupPose.GetColor();
					a = setup.a + (a - setup.a) * alpha;
				} else
					a = color.a + (a - color.a) * alpha;
			}
			color.a = MathUtils.Clamp01(a);
			pose.SetColor(color); // see above
		}
	}

	/// <summary>Changes a slot's <see cref="SlotPose.GetColor()">color</see> and
	/// <see cref="SlotPose.GetDarkColor()">dark color</see> for two color tinting.</summary>
	public class RGBA2Timeline : SlotCurveTimeline {
		public const int ENTRIES = 8;
		protected const int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;

		public RGBA2Timeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, slotIndex, //
				(ulong)Property.RGB << 53 | (uint)slotIndex, //
				(ulong)Property.Alpha << 53 | (uint)slotIndex, //
				(ulong)Property.RGB2 << 53 | (uint)slotIndex) {
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

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, bool fromSetup, bool add) {
			Color32F light = pose.GetColor();
			Color32F? darkOptional = pose.GetDarkColor();
			float r2, g2, b2;
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					SlotPose setup = slot.data.setupPose;
					Color32F setupLight = setup.GetColor();
					Color32F? setupDarkOptional = setup.GetDarkColor();
					pose.SetColor(setupLight); // required due to Color being a struct
					pose.SetDarkColor(setupDarkOptional);
				}
				return;
			}

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
			} else if (fromSetup) {
				SlotPose setupPose = slot.data.setupPose;
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
			Color32F darkValue = new Color32F(r2, g2, b2);
			darkValue.ClampRGB();
			pose.SetDarkColor(darkValue);
		}
	}

	/// <summary>Changes RGB for a slot's <see cref="SlotPose.GetColor()">color</see> and
	/// <see cref="SlotPose.GetDarkColor()">dark color</see> for two color tinting.</summary>
	public class RGB2Timeline : SlotCurveTimeline {
		public const int ENTRIES = 7;
		protected const int R = 1, G = 2, B = 3, R2 = 4, G2 = 5, B2 = 6;

		public RGB2Timeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, slotIndex, //
				(ulong)Property.RGB << 53 | (uint)slotIndex, //
				(ulong)Property.RGB2 << 53 | (uint)slotIndex) {
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

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, bool fromSetup, bool add) {
			Color32F light = pose.GetColor();
			Color32F? darkOptional = pose.GetDarkColor();
			float r, g, b, r2, g2, b2;
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					SlotPose setup = slot.data.setupPose;
					Color32F setupLight = setup.GetColor();
					Color32F? setupDarkOptional = setup.GetDarkColor();
					Color32F dark = darkOptional.Value;
					Color32F setupDark = setupDarkOptional.Value;

					light.r = setupLight.r;
					light.g = setupLight.g;
					light.b = setupLight.b;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;

					pose.SetColor(light); // required due to Color being a struct
					pose.SetDarkColor(dark);
				}
				return;
			}

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
				if (fromSetup) {
					SlotPose setupPose = slot.data.setupPose;
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

	/// <summary>Changes <see cref="SlotPose.Attachment"/>.</summary>
	public class AttachmentTimeline : Timeline, ISlotTimeline {
		readonly int slotIndex;
		readonly string[] attachmentNames;

		public AttachmentTimeline (int frameCount, int slotIndex)
			: base(frameCount, (ulong)Property.Attachment << 53 | (uint)slotIndex) {
			this.slotIndex = slotIndex;
			attachmentNames = new String[frameCount];
			instant = true;
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

		public override void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup, bool add,
							bool mixOut, bool appliedPose) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			SlotPose pose = appliedPose ? slot.appliedPose : slot.pose;

			if (mixOut || time < this.frames[0]) {
				if (fromSetup) SetAttachment(skeleton, pose, slot.data.attachmentName);
			} else
				SetAttachment(skeleton, pose, attachmentNames[Search(this.frames, time)]);
		}

		private void SetAttachment (Skeleton skeleton, SlotPose pose, string attachmentName) {
			pose.Attachment = attachmentName == null ? null : skeleton.GetAttachment(slotIndex, attachmentName);
		}
	}

	/// <summary>Changes <see cref="SlotPose.Deform"/> to deform a <see cref="VertexAttachment"/>.</summary>
	public class DeformTimeline : SlotCurveTimeline {
		readonly VertexAttachment attachment;
		internal float[][] vertices;

		public DeformTimeline (int frameCount, int bezierCount, int slotIndex, VertexAttachment attachment)
			: base(frameCount, bezierCount, slotIndex,
				  (ulong)Property.Deform << 53 | (ulong)(uint)slotIndex << 32 | (uint)attachment.Id) {
			this.attachment = attachment;
			vertices = new float[frameCount][];
			additive = true;
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

		override protected void Apply (Slot slot, SlotPose pose, float time, float alpha, bool fromSetup, bool add) {
			VertexAttachment vertexAttachment = pose.attachment as VertexAttachment;
			if (vertexAttachment == null || vertexAttachment.TimelineAttachment != attachment) return;

			ExposedList<float> deformArray = pose.deform;
			if (deformArray.Count == 0) fromSetup = true;

			float[][] vertices = this.vertices;
			int vertexCount = vertices[0].Length;

			float[] deform;

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) deformArray.Clear();
				return;
			}

			// Ensure size and preemptively set count.
			if (deformArray.Capacity < vertexCount) deformArray.Capacity = vertexCount;
			deformArray.Count = vertexCount;
			deform = deformArray.Items;

			if (time >= frames[frames.Length - 1]) { // Time is after last frame.
				float[] lastVertices = vertices[frames.Length - 1];
				if (alpha == 1) {
					if (add && !fromSetup) {
						if (vertexAttachment.bones == null) { // Unweighted vertex positions, no alpha.
							float[] setupVertices = vertexAttachment.vertices;
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] - setupVertices[i];
						} else { // Weighted deform offsets, no alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i];
						}
					} else // Vertex positions or deform offsets, no alpha.
						Array.Copy(lastVertices, 0, deform, 0, vertexCount);
				} else if (fromSetup) {
					if (vertexAttachment.bones == null) { // Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++) {
							float setup = setupVertices[i];
							deform[i] = setup + (lastVertices[i] - setup) * alpha;
						}
					} else { // Weighted deform offsets, alpha.
						for (int i = 0; i < vertexCount; i++)
							deform[i] = lastVertices[i] * alpha;
					}
				} else if (add) {
					if (vertexAttachment.bones == null) { // Unweighted vertex positions, no alpha.
						float[] setupVertices = vertexAttachment.vertices;
						for (int i = 0; i < vertexCount; i++)
							deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
					} else { // Weighted deform offsets, alpha.
						for (int i = 0; i < vertexCount; i++)
							deform[i] += lastVertices[i] * alpha;
					}
				} else { // Vertex positions or deform offsets, with alpha.
					for (int i = 0; i < vertexCount; i++)
						deform[i] += (lastVertices[i] - deform[i]) * alpha;
				}
				return;
			}

			int frame = Search(frames, time);
			float percent = GetCurvePercent(time, frame);
			float[] prevVertices = vertices[frame];
			float[] nextVertices = vertices[frame + 1];

			if (alpha == 1) {
				if (add && !fromSetup) {
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
			} else if (fromSetup) {
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
			} else if (add) {
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
			} else { // Vertex positions or deform offsets, with alpha.
				for (int i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
				}
			}
		}
	}

	/// <summary>Changes <see cref="SlotPose.SequenceIndex"/> for an attachment's <see cref="Sequence"/>.</summary>
	public class SequenceTimeline : Timeline, ISlotTimeline {
		public const int ENTRIES = 3;
		private const int MODE = 1, DELAY = 2;

		readonly int slotIndex;
		readonly IHasSequence attachment;

		public SequenceTimeline (int frameCount, int slotIndex, Attachment attachment)
			: base(frameCount, (ulong)Property.Sequence << 53 | (ulong)(uint)slotIndex << 32
				  | (uint)((IHasSequence)attachment).Sequence.Id) {
			this.slotIndex = slotIndex;
			this.attachment = (IHasSequence)attachment;
			instant = true;
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <summary>The attachment for which <see cref="SlotPose.SequenceIndex"/> will be set.</summary>
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

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup,
			bool add, bool mixOut, bool appliedPose) {

			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			SlotPose pose = appliedPose ? slot.appliedPose : slot.pose;

			Attachment slotAttachment = pose.attachment;
			IHasSequence hasSequence = slotAttachment as IHasSequence;
			if ((hasSequence == null) || slotAttachment.TimelineAttachment != attachment) return;

			if (mixOut) {
				if (fromSetup) pose.SequenceIndex = -1;
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) pose.SequenceIndex = -1;
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
		new readonly static ulong[] propertyIds = { (ulong)Property.Event };
		readonly Event[] events;

		public EventTimeline (int frameCount)
			: base(frameCount, propertyIds) {
			events = new Event[frameCount];
			instant = true;
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
			bool fromSetup, bool add, bool mixOut, bool appliedPose) {

			if (firedEvents == null) return;

			float[] frames = this.frames;
			int frameCount = frames.Length;

			if (lastTime > time) { // Apply after lastTime for looped animations.
				Apply(null, lastTime, int.MaxValue, firedEvents, 0, false, false, false, false);
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

	/// <summary>Changes the <see cref="Skeleton.DrawOrder"/>.</summary>
	public class DrawOrderTimeline : Timeline {
		internal static readonly ulong propertyID = (ulong)Property.DrawOrder;
		new internal static readonly ulong[] propertyIds = { propertyID };

		readonly int[][] drawOrders;

		public DrawOrderTimeline (int frameCount)
			: base(frameCount, propertyIds) {
			drawOrders = new int[frameCount][];
			instant = true;
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
		public override void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup,
			bool add, bool mixOut, bool appliedPose) {
			Slot[] pose = (appliedPose ? skeleton.drawOrder.appliedPose : skeleton.drawOrder.pose).Items;
			Slot[] setup = skeleton.slots.Items;

			if (mixOut || time < frames[0]) {
				if (fromSetup) Array.Copy(setup, 0, pose, 0, skeleton.slots.Count);
				return;
			}

			int[] order = drawOrders[Search(frames, time)];
			if (order == null)
				Array.Copy(setup, 0, pose, 0, skeleton.slots.Count);
			else {
				for (int i = 0, n = order.Length; i < n; i++)
					pose[i] = setup[order[i]];
			}
		}
	}


	/// <summary>Changes a subset of the <see cref="Skeleton.DrawOrder">draw order</see>.</summary>
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
			instant = true;
		}

		static private ulong[] PropertyIdsFromSlots (int[] slots) {
			int n = slots.Length;
			var ids = new ulong[n];
			for (int i = 0; i < n; i++)
				ids[i] = (ulong)(uint)slots[i];
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

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup,
			bool add, bool mixOut, bool appliedPose) {
			Slot[] pose = (appliedPose ? skeleton.drawOrder.appliedPose : skeleton.drawOrder.pose).Items;
			Slot[] setup = skeleton.slots.Items;

			if (mixOut || time < frames[0]) {
				if (fromSetup) Setup(pose, setup);
			} else {
				int[] order = drawOrders[Search(frames, time)];
				if (order == null)
					Setup(pose, setup);
				else {
					bool[] inFolder = this.inFolder;
					int[] slots = this.slots;
					for (int i = 0, found = 0, done = slots.Length; ; i++) {
						if (inFolder[pose[i].data.index]) {
							pose[i] = setup[slots[order[found]]];
							if (++found == done) break;
						}
					}
				}
			}
		}

		private void Setup (Slot[] pose, Slot[] setup) {
			bool[] inFolder = this.inFolder;
			int[] slots = this.slots;
			for (int i = 0, found = 0, done = slots.Length; ; i++) {
				if (inFolder[pose[i].data.index]) {
					pose[i] = setup[slots[found]];
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

	/// <summary>Changes <see cref="IkConstraintPose.Mix"/>, <see cref="IkConstraintPose.Softness"/>,
	/// <see cref="IkConstraintPose.BendDirection"/>, <see cref="IkConstraintPose.Stretch"/>, and
	/// <see cref="IkConstraintPose.Compress"/>.</summary>
	public class IkConstraintTimeline : CurveTimeline, IConstraintTimeline {
		public const int ENTRIES = 6;
		private const int MIX = 1, SOFTNESS = 2, BEND_DIRECTION = 3, COMPRESS = 4, STRETCH = 5;

		readonly int constraintIndex;

		public IkConstraintTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, (ulong)Property.IkConstraint << 53 | (uint)constraintIndex) {
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
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup, bool add,
									bool mixOut, bool appliedPose) {
			var constraint = (IkConstraint)skeleton.constraints.Items[constraintIndex];
			if (!constraint.active) return;
			IkConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					IkConstraintPose setup = constraint.data.setupPose;
					pose.mix = setup.mix;
					pose.softness = setup.softness;
					pose.bendDirection = setup.bendDirection;
					pose.compress = setup.compress;
					pose.stretch = setup.stretch;
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

			IkConstraintPose basePose = fromSetup ? constraint.data.setupPose : pose;
			pose.mix = basePose.mix + (mix - basePose.mix) * alpha;
			pose.softness = basePose.softness + (softness - basePose.softness) * alpha;
			if (mixOut) {
				if (fromSetup) {
					pose.bendDirection = basePose.bendDirection;
					pose.compress = basePose.compress;
					pose.stretch = basePose.stretch;

				}
			} else {
				pose.bendDirection = (int)frames[i + BEND_DIRECTION];
				pose.compress = frames[i + COMPRESS] != 0;
				pose.stretch = frames[i + STRETCH] != 0;
			}
		}
	}

	/// <summary>Changes <see cref="TransformConstraintPose.MixRotate"/>, <see cref="TransformConstraintPose.MixX"/>,
	/// <see cref="TransformConstraintPose.MixY"/>, <see cref="TransformConstraintPose.MixScaleX"/>,
	/// <see cref="TransformConstraintPose.MixScaleY"/>, and <see cref="TransformConstraintPose.MixShearY"/></summary>
	public class TransformConstraintTimeline : CurveTimeline, IConstraintTimeline {
		public const int ENTRIES = 7;
		private const int ROTATE = 1, X = 2, Y = 3, SCALEX = 4, SCALEY = 5, SHEARY = 6;

		readonly int constraintIndex;

		public TransformConstraintTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, (ulong)Property.TransformConstraint << 53 | (uint)constraintIndex) {
			this.constraintIndex = constraintIndex;
			additive = true;
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
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup, bool add,
									bool mixOut, bool appliedPose) {
			var constraint = (TransformConstraint)skeleton.constraints.Items[constraintIndex];
			if (!constraint.active) return;
			TransformConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					TransformConstraintPose setup = constraint.data.setupPose;
					pose.mixRotate = setup.mixRotate;
					pose.mixX = setup.mixX;
					pose.mixY = setup.mixY;
					pose.mixScaleX = setup.mixScaleX;
					pose.mixScaleY = setup.mixScaleY;
					pose.mixShearY = setup.mixShearY;
				}
				return;
			}

			// note: reference implementation has code inlined, we re-use GetCurveValue code for root motion.
			float rotate, x, y, scaleX, scaleY, shearY;
			GetCurveValue(out rotate, out x, out y, out scaleX, out scaleY, out shearY, time);

			TransformConstraintPose basePose = fromSetup ? constraint.data.setupPose : pose;
			if (add) {
				pose.mixRotate = basePose.mixRotate + rotate * alpha;
				pose.mixX = basePose.mixX + x * alpha;
				pose.mixY = basePose.mixY + y * alpha;
				pose.mixScaleX = basePose.mixScaleX + scaleX * alpha;
				pose.mixScaleY = basePose.mixScaleY + scaleY * alpha;
				pose.mixShearY = basePose.mixShearY + shearY * alpha;
			} else {
				pose.mixRotate = basePose.mixRotate + (rotate - basePose.mixRotate) * alpha;
				pose.mixX = basePose.mixX + (x - basePose.mixX) * alpha;
				pose.mixY = basePose.mixY + (y - basePose.mixY) * alpha;
				pose.mixScaleX = basePose.mixScaleX + (scaleX - basePose.mixScaleX) * alpha;
				pose.mixScaleY = basePose.mixScaleY + (scaleY - basePose.mixScaleY) * alpha;
				pose.mixShearY = basePose.mixShearY + (shearY - basePose.mixShearY) * alpha;
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

	/// <summary>The base class for timelines that change 1 constraint property with a curve.</summary>
	abstract public class ConstraintTimeline1 : CurveTimeline1, IConstraintTimeline {
		internal readonly int constraintIndex;

		public ConstraintTimeline1 (int frameCount, int bezierCount, int constraintIndex, Property property)
			: base(frameCount, bezierCount, (ulong)property << 53 | (uint)constraintIndex) {
			this.constraintIndex = constraintIndex;
		}

		public int ConstraintIndex {
			get {
				return constraintIndex;
			}
		}
	}

	/// <summary>Changes <see cref="PathConstraintPose.Position"/>.</summary>
	public class PathConstraintPositionTimeline : ConstraintTimeline1 {

		public PathConstraintPositionTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PathConstraintPosition) {
			additive = true;
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup,
			bool add, bool mixOut, bool appliedPose) {
			var constraint = (PathConstraint)skeleton.constraints.Items[constraintIndex];
			if (constraint.active) {
				PathConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
				pose.position = GetAbsoluteValue(time, alpha, fromSetup, add, pose.position, constraint.data.setupPose.position);
			}
		}
	}

	/// <summary>Changes <see cref="PathConstraintPose.Spacing"/>.</summary>
	public class PathConstraintSpacingTimeline : ConstraintTimeline1 {

		public PathConstraintSpacingTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.PathConstraintSpacing) {
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup, bool add,
									bool mixOut, bool appliedPose) {

			var constraint = (PathConstraint)skeleton.constraints.Items[constraintIndex];
			if (constraint.active) {
				PathConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
				pose.spacing = GetAbsoluteValue(time, alpha, fromSetup, false, pose.spacing, constraint.data.setupPose.spacing);
			}
		}
	}

	/// <summary>Changes <see cref="PathConstraintPose.MixRotate"/>, <see cref="PathConstraintPose.MixX"/>, and
	/// <see cref="PathConstraintPose.MixY"/>.</summary>
	public class PathConstraintMixTimeline : CurveTimeline, IConstraintTimeline {
		public const int ENTRIES = 4;
		private const int ROTATE = 1, X = 2, Y = 3;

		readonly int constraintIndex;

		public PathConstraintMixTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, (ulong)Property.PathConstraintMix << 53 | (uint)constraintIndex) {
			this.constraintIndex = constraintIndex;
			additive = true;
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
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup,
			bool add, bool mixOut, bool appliedPose) {
			var constraint = (PathConstraint)skeleton.constraints.Items[constraintIndex];
			if (!constraint.active) return;
			PathConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					PathConstraintPose setup = constraint.data.setupPose;
					pose.mixRotate = setup.mixRotate;
					pose.mixX = setup.mixX;
					pose.mixY = setup.mixY;
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

			PathConstraintPose basePose = fromSetup ? constraint.data.setupPose : pose;
			if (add) {
				pose.mixRotate = basePose.mixRotate + rotate * alpha;
				pose.mixX = basePose.mixX + x * alpha;
				pose.mixY = basePose.mixY + y * alpha;
			} else {
				pose.mixRotate = basePose.mixRotate + (rotate - basePose.mixRotate) * alpha;
				pose.mixX = basePose.mixX + (x - basePose.mixX) * alpha;
				pose.mixY = basePose.mixY + (y - basePose.mixY) * alpha;
			}
		}
	}

	/// <summary>The base class for most <see cref="PhysicsConstraint"/> timelines.</summary>
	public abstract class PhysicsConstraintTimeline : ConstraintTimeline1 {
		/// <param name="constraintIndex">-1 for all physics constraints in the skeleton.</param>
		public PhysicsConstraintTimeline (int frameCount, int bezierCount, int constraintIndex, Property property)
			: base(frameCount, bezierCount, constraintIndex, property) {
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup, bool add,
			bool mixOut, bool appliedPose) {
			if (add && !additive) add = false;
			if (constraintIndex == -1) {
				float value = time >= frames[0] ? GetCurveValue(time) : 0;
				PhysicsConstraint[] constraints = skeleton.physics.Items;
				for (int i = 0, n = skeleton.physics.Count; i < n; i++) {
					PhysicsConstraint constraint = constraints[i];
					if (constraint.active && Global(constraint.data)) {
						PhysicsConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
						Set(pose, GetAbsoluteValue(time, alpha, fromSetup, add, Get(pose), Get(constraint.data.setupPose), value));
					}
				}
			} else {
				var constraint = (PhysicsConstraint)skeleton.constraints.Items[constraintIndex];
				if (constraint.active) {
					PhysicsConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
					Set(pose, GetAbsoluteValue(time, alpha, fromSetup, add, Get(pose), Get(constraint.data.setupPose)));
				}
			}
		}

		abstract protected float Get (PhysicsConstraintPose pose);

		abstract protected void Set (PhysicsConstraintPose pose, float value);

		abstract protected bool Global (PhysicsConstraintData constraint);
	}

	/// <summary>Changes <see cref="PhysicsConstraintPose.Inertia"/>.</summary>
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

	/// <summary>Changes <see cref="PhysicsConstraintPose.Strength"/>.</summary>
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

	/// <summary>Changes <see cref="PhysicsConstraintPose.Damping"/>.</summary>
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

	/// <summary>Changes <see cref="PhysicsConstraintPose.MassInverse"/>. The timeline values are not inverted.</summary>
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

	/// <summary>Changes <see cref="PhysicsConstraintPose.Wind"/>.</summary>
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

	/// <summary>Changes <see cref="PhysicsConstraintPose.Gravity"/>.</summary>
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

	/// <summary>Changes <see cref="PhysicsConstraintPose.Mix"/>.</summary>
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
		new internal static readonly ulong[] propertyIds = { (ulong)Property.PhysicsConstraintReset };

		readonly int constraintIndex;

		/// <param name="constraintIndex">-1 for all physics constraints in the skeleton.</param>
		public PhysicsConstraintResetTimeline (int frameCount, int constraintIndex)
			: base(frameCount, propertyIds) {
			this.constraintIndex = constraintIndex;
			instant = true;
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
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, bool fromSetup, bool add,
									bool mixOut, bool appliedPose) {

			PhysicsConstraint constraint = null;
			if (constraintIndex != -1) {
				constraint = (PhysicsConstraint)skeleton.constraints.Items[constraintIndex];
				if (!constraint.active) return;
			}

			float[] frames = this.frames;

			if (lastTime > time) { // Apply after lastTime for looped animations.
				Apply(skeleton, lastTime, int.MaxValue, null, alpha, false, false, false, false);
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
	/// Changes <see cref="SliderPose.Time"/>.
	/// </summary>
	public class SliderTimeline : ConstraintTimeline1 {
		public SliderTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.SliderTime) {
			additive = true;
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup,
			bool add, bool mixOut, bool appliedPose) {

			var constraint = (Slider)skeleton.constraints.Items[constraintIndex];
			if (constraint.active) {
				SliderPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
				pose.time = GetAbsoluteValue(time, alpha, fromSetup, add, pose.time, constraint.data.setupPose.time);
			}
		}
	}

	/// <summary>
	/// Changes <see cref="SliderPose.Mix"/>.
	/// </summary>
	public class SliderMixTimeline : ConstraintTimeline1 {
		public SliderMixTimeline (int frameCount, int bezierCount, int constraintIndex)
			: base(frameCount, bezierCount, constraintIndex, Property.SliderMix) {
			additive = true;
		}

		/// <param name="events">May be null.</param>
		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, bool fromSetup,
			bool add, bool mixOut, bool appliedPose) {

			var constraint = (Slider)skeleton.constraints.Items[constraintIndex];
			if (constraint.active) {
				SliderPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
				pose.mix = GetAbsoluteValue(time, alpha, fromSetup, add, pose.mix, constraint.data.setupPose.mix);
			}
		}
	}
}
