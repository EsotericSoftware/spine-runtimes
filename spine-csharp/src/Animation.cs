/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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
	/// Stores a list of timelines to animate a skeleton's pose over time.</summary>
	public class Animation {
		internal String name;
		internal ExposedList<Timeline> timelines;
		internal HashSet<string> timelineIds;
		internal float duration;

		public Animation (string name, ExposedList<Timeline> timelines, float duration) {
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");

			this.name = name;
			SetTimelines(timelines);
			this.duration = duration;
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
			int timelinesCount = timelines.Count;
			Timeline[] timelinesItems = timelines.Items;
			for (int t = 0; t < timelinesCount; ++t)
				idCount += timelinesItems[t].PropertyIds.Length;
			string[] propertyIds = new string[idCount];
			int currentId = 0;
			for (int t = 0; t < timelinesCount; ++t) {
				string[] ids = timelinesItems[t].PropertyIds;
				for (int i = 0, idsLength = ids.Length; i < idsLength; ++i)
					propertyIds[currentId++] = ids[i];
			}
			this.timelineIds = new HashSet<string>(propertyIds);
		}

		/// <summary>The duration of the animation in seconds, which is usually the highest time of all frames in the timeline. The duration is
		/// used to know when it has completed and when it should loop back to the start.</summary>
		public float Duration { get { return duration; } set { duration = value; } }

		/// <summary>The animation's name, which is unique across all animations in the skeleton.</summary>
		public string Name { get { return name; } }

		/// <summary>Returns true if this animation contains a timeline with any of the specified property IDs.</summary>
		public bool HasTimeline (string[] propertyIds) {
			foreach (string id in propertyIds)
				if (timelineIds.Contains(id)) return true;
			return false;
		}

		/// <summary>Applies the animation's timelines to the specified skeleton.</summary>
		/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixBlend, MixDirection)"/>
		/// <param name="skeleton">The skeleton the animation is being applied to. This provides access to the bones, slots, and other skeleton
		///					components the timelines may change.</param>
		/// <param name="lastTime">The last time in seconds this animation was applied. Some timelines trigger only at specific times rather
		///					than every frame. Pass -1 the first time an animation is applied to ensure frame 0 is triggered.</param>
		/// <param name="time"> The time in seconds the skeleton is being posed for. Most timelines find the frame before and the frame after
		///					this time and interpolate between the frame values. If beyond the <see cref="Duration"/> and <code>loop</code> is
		///					true then the animation will repeat, else the last frame will be applied.</param>
		/// <param name="loop">If true, the animation repeats after the <see cref="Duration"/>.</param>
		/// <param name="events">If any events are fired, they are added to this list. Can be null to ignore fired events or if no timelines
		///					fire events.</param>
		/// <param name="alpha"> 0 applies the current or setup values (depending on <code>blend</code>). 1 applies the timeline values. Between
		///					0 and 1 applies values between the current or setup values and the timeline values. By adjusting
		///					<code>alpha</code> over time, an animation can be mixed in or out. <code>alpha</code> can also be useful to apply
		///					animations on top of each other (layering).</param>
		/// <param name="blend">Controls how mixing is applied when <code>alpha</code> < 1.</param>
		/// <param name="direction">Indicates whether the timelines are mixing in or out. Used by timelines which perform instant transitions,
		///					such as <see cref="DrawOrderTimeline"/> or <see cref="AttachmentTimeline"/>.</param>
		public void Apply (Skeleton skeleton, float lastTime, float time, bool loop, ExposedList<Event> events, float alpha,
							MixBlend blend, MixDirection direction) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				if (lastTime > 0) lastTime %= duration;
			}

			Timeline[] timelines = this.timelines.Items;
			for (int i = 0, n = this.timelines.Count; i < n; i++)
				timelines[i].Apply(skeleton, lastTime, time, events, alpha, blend, direction);
		}

		override public string ToString () {
			return name;
		}
	}

	/// <summary>
	/// Controls how timeline values are mixed with setup pose values or current pose values when a timeline is applied with
	/// <code>alpha</code> < 1.</summary>
	/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixBlend, MixDirection)"/>
	public enum MixBlend {
		/// <summary> Transitions from the setup value to the timeline value (the current value is not used). Before the first frame, the
		///           setup value is set.</summary>
		Setup,

		/// <summary>
		/// <para>
		/// Transitions from the current value to the timeline value. Before the first frame, transitions from the current value to
		/// the setup value. Timelines which perform instant transitions, such as <see cref="DrawOrderTimeline"/> or
		/// <see cref="AttachmentTimeline"/>, use the setup value before the first frame.</para>
		/// <para>
		/// <code>First</code> is intended for the first animations applied, not for animations layered on top of those.</para>
		/// </summary>
		First,

		/// <summary>
		/// <para>
		/// Transitions from the current value to the timeline value. No change is made before the first frame (the current value is
		/// kept until the first frame).</para>
		/// <para>
		/// <code>Replace</code> is intended for animations layered on top of others, not for the first animations applied.</para>
		/// </summary>
		Replace,

		/// <summary>
		/// <para>
		/// Transitions from the current value to the current value plus the timeline value. No change is made before the first frame
		/// (the current value is kept until the first frame).</para>
		/// <para>
		/// <code>Add</code> is intended for animations layered on top of others, not for the first animations applied. Properties
		/// set by additive animations must be set manually or by another animation before applying the additive animations, else the
		/// property values will increase each time the additive animations are applied.
		/// </para>
		/// </summary>
		Add
	}

	/// <summary>
	/// Indicates whether a timeline's <code>alpha</code> is mixing out over time toward 0 (the setup or current pose value) or
	/// mixing in toward 1 (the timeline's value). Some timelines use this to decide how values are applied.</summary>
	/// <seealso cref="Timeline.Apply(Skeleton, float, float, ExposedList, float, MixBlend, MixDirection)"/>
	public enum MixDirection {
		In,
		Out
	}

	internal enum Property {
		Rotate = 0, X, Y, ScaleX, ScaleY, ShearX, ShearY, //
		RGB, Alpha, RGB2, //
		Attachment, Deform, //
		Event, DrawOrder, //
		IkConstraint, TransformConstraint, //
		PathConstraintPosition, PathConstraintSpacing, PathConstraintMix, //
		Sequence
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
		public int FrameCount {
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
		///					<code>lastTime</code> (exclusive) and <code>time</code> (inclusive). Pass -1 the first time an animation is
		///					 applied to ensure frame 0 is triggered.</param>
		/// <param name="time">The time in seconds that the skeleton is being posed for. Most timelines find the frame before and the frame
		///					after this time and interpolate between the frame values.If beyond the last frame, the last frame will be
		///					applied.</param>
		/// <param name="events">If any events are fired, they are added to this list. Can be null to ignore fired events or if the timeline
		///					does not fire events.</param>
		/// <param name="alpha">0 applies the current or setup value (depending on <code>blend</code>). 1 applies the timeline value.
		///					Between 0 and 1 applies a value between the current or setup value and the timeline value.By adjusting
		///					<code>alpha</code> over time, an animation can be mixed in or out. <code>alpha</code> can also be useful to
		///					apply animations on top of each other (layering).</param>
		/// <param name="blend">Controls how mixing is applied when <code>alpha</code> < 1.</param>
		/// <param name="direction">Indicates whether the timeline is mixing in or out. Used by timelines which perform instant transitions,
		///                   such as <see cref="DrawOrderTimeline"/> or <see cref="AttachmentTimeline"/>, and other such as <see cref="ScaleTimeline"/>.</param>
		public abstract void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha,
			MixBlend blend, MixDirection direction);

		/// <summary>Search using a stride of 1.</summary>
		/// <param name="time">Must be >= the first value in <code>frames</code>.</param>
		/// <returns>The index of the first value <= <code>time</code>.</returns>
		internal static int Search (float[] frames, float time) {
			int n = frames.Length;
			for (int i = 1; i < n; i++)
				if (frames[i] > time) return i - 1;
			return n - 1;
		}

		/// <summary>Search using the specified stride.</summary>
		/// <param name="time">Must be >= the first value in <code>frames</code>.</param>
		/// <returns>The index of the first value <= <code>time</code>.</returns>
		internal static int Search (float[] frames, float time, int step) {
			int n = frames.Length;
			for (int i = step; i < n; i += step)
				if (frames[i] > time) return i - step;
			return n - step;
		}
	}

	/// <summary>An interface for timelines which change the property of a bone.</summary>
	public interface IBoneTimeline {
		/// <summary>The index of the bone in <see cref="Skeleton.Bones"/> that will be changed when this timeline is applied.</summary>
		int BoneIndex { get; }
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
		/// <param name="frame">Between 0 and <code>frameCount - 1</code>, inclusive.</param>
		public void SetLinear (int frame) {
			curves[frame] = LINEAR;
		}

		/// <summary>Sets the specified frame to stepped interpolation.</summary>
		/// <param name="frame">Between 0 and <code>frameCount - 1</code>, inclusive.</param>
		public void SetStepped (int frame) {
			curves[frame] = STEPPED;
		}

		/// <summary>Returns the interpolation type for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount - 1</code>, inclusive.</param>
		/// <returns><see cref="LINEAR"/>, <see cref="STEPPED"/> or <see cref="BEZIER"/> + the index of the Bezier segments.</returns>
		public float GetCurveType (int frame) {
			return (int)curves[frame];
		}

		/// <summary>Shrinks the storage for Bezier curves, for use when <code>bezierCount</code> (specified in the constructor) was larger
		/// than the actual number of Bezier curves.</summary>
		public void Shrink (int bezierCount) {
			int size = FrameCount + bezierCount * BEZIER_SIZE;
			if (curves.Length > size) {
				float[] newCurves = new float[size];
				Array.Copy(curves, 0, newCurves, 0, size);
				curves = newCurves;
			}
		}

		/// <summary>
		/// Stores the segments for the specified Bezier curve. For timelines that modify multiple values, there may be more than
		/// one curve per frame.</summary>
		/// <param name="bezier">The ordinal of this Bezier curve for this timeline, between 0 and <code>bezierCount - 1</code> (specified
		///					in the constructor), inclusive.</param>
		/// <param name="frame">Between 0 and <code>frameCount - 1</code>, inclusive.</param>
		/// <param name="value">The index of the value for the frame this curve is used for.</param>
		/// <param name="time1">The time for the first key.</param>
		/// <param name="value1">The value for the first key.</param>
		/// <param name="cx1">The time for the first Bezier handle.</param>
		/// <param name="cy1">The value for the first Bezier handle.</param>
		/// <param name="cx2">The time of the second Bezier handle.</param>
		/// <param name="cy2">The value for the second Bezier handle.</param>
		/// <param name="time2">The time for the second key.</param>
		/// <param name="value2">The value for the second key.</param>
		public void SetBezier (int bezier, int frame, int value, float time1, float value1, float cx1, float cy1, float cx2,
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
		/// <param name="frameIndex">The index into <see cref="Frames"/> for the values of the frame before <code>time</code>.</param>
		/// <param name="valueOffset">The offset from <code>frameIndex</code> to the value this curve is used for.</param>
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
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
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
	}

	/// <summary>The base class for a <see cref="CurveTimeline"/> which sets two properties.</summary>
	public abstract class CurveTimeline2 : CurveTimeline {
		public const int ENTRIES = 3;
		internal const int VALUE1 = 1, VALUE2 = 2;

		/// <param name="bezierCount">The maximum number of Bezier curves. See <see cref="Shrink(int)"/>.</param>
		/// <param name="propertyIds">Unique identifiers for the properties the timeline modifies.</param>
		public CurveTimeline2 (int frameCount, int bezierCount, string propertyId1, string propertyId2)
			: base(frameCount, bezierCount, propertyId1, propertyId2) {
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		/// <summary>Sets the time and values for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float value1, float value2) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + VALUE1] = value1;
			frames[frame + VALUE2] = value2;
		}
	}

	/// <summary>Changes a bone's local <see cref="Bone.Rotation"/>.</summary>
	public class RotateTimeline : CurveTimeline1, IBoneTimeline {
		readonly int boneIndex;

		public RotateTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, (int)Property.Rotate + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;

			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					bone.rotation = bone.data.rotation;
					return;
				case MixBlend.First:
					bone.rotation += (bone.data.rotation - bone.rotation) * alpha;
					return;
				}
				return;
			}

			float r = GetCurveValue(time);
			switch (blend) {
			case MixBlend.Setup:
				bone.rotation = bone.data.rotation + r * alpha;
				break;
			case MixBlend.First:
			case MixBlend.Replace:
				r += bone.data.rotation - bone.rotation;
				goto case MixBlend.Add; // Fall through.
			case MixBlend.Add:
				bone.rotation += r * alpha;
				break;
			}
		}
	}

	/// <summary>Changes a bone's local <see cref"Bone.X"/> and <see cref"Bone.Y"/>.</summary>
	public class TranslateTimeline : CurveTimeline2, IBoneTimeline {
		readonly int boneIndex;

		public TranslateTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, //
				(int)Property.X + "|" + boneIndex, //
				(int)Property.Y + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
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
			GetCurveValue(out x, out y, time); // note: reference implementation has code inlined

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

	/// <summary>Changes a bone's local <see cref"Bone.X"/>.</summary>
	public class TranslateXTimeline : CurveTimeline1, IBoneTimeline {
		readonly int boneIndex;

		public TranslateXTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, (int)Property.X + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
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
					return;
				case MixBlend.First:
					bone.x += (bone.data.x - bone.x) * alpha;
					return;
				}
				return;
			}

			float x = GetCurveValue(time);
			switch (blend) {
			case MixBlend.Setup:
				bone.x = bone.data.x + x * alpha;
				break;
			case MixBlend.First:
			case MixBlend.Replace:
				bone.x += (bone.data.x + x - bone.x) * alpha;
				break;
			case MixBlend.Add:
				bone.x += x * alpha;
				break;
			}
		}
	}

	/// <summary>Changes a bone's local <see cref"Bone.Y"/>.</summary>
	public class TranslateYTimeline : CurveTimeline1, IBoneTimeline {
		readonly int boneIndex;

		public TranslateYTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, (int)Property.Y + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					bone.y = bone.data.y;
					return;
				case MixBlend.First:
					bone.y += (bone.data.y - bone.y) * alpha;
					return;
				}
				return;
			}

			float y = GetCurveValue(time);
			switch (blend) {
			case MixBlend.Setup:
				bone.y = bone.data.y + y * alpha;
				break;
			case MixBlend.First:
			case MixBlend.Replace:
				bone.y += (bone.data.y + y - bone.y) * alpha;
				break;
			case MixBlend.Add:
				bone.y += y * alpha;
				break;
			}
		}
	}

	/// <summary>Changes a bone's local <see cref="Bone.ScaleX"> and <see cref="Bone.ScaleY">.</summary>
	public class ScaleTimeline : CurveTimeline2, IBoneTimeline {
		readonly int boneIndex;

		public ScaleTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, //
				(int)Property.ScaleX + "|" + boneIndex, //
				(int)Property.ScaleY + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
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
			x *= bone.data.scaleX;
			y *= bone.data.scaleY;

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
						bone.scaleX += (x - bone.data.scaleX) * alpha;
						bone.scaleY += (y - bone.data.scaleY) * alpha;
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
						bone.scaleX += (x - bone.data.scaleX) * alpha;
						bone.scaleY += (y - bone.data.scaleY) * alpha;
						break;
					}
				}
			}
		}
	}

	/// <summary>Changes a bone's local <see cref="Bone.ScaleX">.</summary>
	public class ScaleXTimeline : CurveTimeline1, IBoneTimeline {
		readonly int boneIndex;

		public ScaleXTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, (int)Property.ScaleX + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
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
					return;
				case MixBlend.First:
					bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
					return;
				}
				return;
			}

			float x = GetCurveValue(time) * bone.data.scaleX;
			if (alpha == 1) {
				if (blend == MixBlend.Add)
					bone.scaleX += x - bone.data.scaleX;
				else
					bone.scaleX = x;
			} else {
				// Mixing out uses sign of setup or current pose, else use sign of key.
				float bx;
				if (direction == MixDirection.Out) {
					switch (blend) {
					case MixBlend.Setup:
						bx = bone.data.scaleX;
						bone.scaleX = bx + (Math.Abs(x) * Math.Sign(bx) - bx) * alpha;
						break;
					case MixBlend.First:
					case MixBlend.Replace:
						bx = bone.scaleX;
						bone.scaleX = bx + (Math.Abs(x) * Math.Sign(bx) - bx) * alpha;
						break;
					case MixBlend.Add:
						bone.scaleX += (x - bone.data.scaleX) * alpha;
						break;
					}
				} else {
					switch (blend) {
					case MixBlend.Setup:
						bx = Math.Abs(bone.data.scaleX) * Math.Sign(x);
						bone.scaleX = bx + (x - bx) * alpha;
						break;
					case MixBlend.First:
					case MixBlend.Replace:
						bx = Math.Abs(bone.scaleX) * Math.Sign(x);
						bone.scaleX = bx + (x - bx) * alpha;
						break;
					case MixBlend.Add:
						bone.scaleX += (x - bone.data.scaleX) * alpha;
						break;
					}
				}
			}
		}
	}

	/// <summary>Changes a bone's local <see cref="Bone.ScaleY">.</summary>
	public class ScaleYTimeline : CurveTimeline1, IBoneTimeline {
		readonly int boneIndex;

		public ScaleYTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, (int)Property.ScaleY + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					bone.scaleY = bone.data.scaleY;
					return;
				case MixBlend.First:
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
					return;
				}
				return;
			}

			float y = GetCurveValue(time) * bone.data.scaleY;
			if (alpha == 1) {
				if (blend == MixBlend.Add)
					bone.scaleY += y - bone.data.scaleY;
				else
					bone.scaleY = y;
			} else {
				// Mixing out uses sign of setup or current pose, else use sign of key.
				float by;
				if (direction == MixDirection.Out) {
					switch (blend) {
					case MixBlend.Setup:
						by = bone.data.scaleY;
						bone.scaleY = by + (Math.Abs(y) * Math.Sign(by) - by) * alpha;
						break;
					case MixBlend.First:
					case MixBlend.Replace:
						by = bone.scaleY;
						bone.scaleY = by + (Math.Abs(y) * Math.Sign(by) - by) * alpha;
						break;
					case MixBlend.Add:
						bone.scaleY += (y - bone.data.scaleY) * alpha;
						break;
					}
				} else {
					switch (blend) {
					case MixBlend.Setup:
						by = Math.Abs(bone.data.scaleY) * Math.Sign(y);
						bone.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.First:
					case MixBlend.Replace:
						by = Math.Abs(bone.scaleY) * Math.Sign(y);
						bone.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.Add:
						bone.scaleY += (y - bone.data.scaleY) * alpha;
						break;
					}
				}
			}
		}
	}

	/// <summary>Changes a bone's local <see cref="Bone.ShearX"/> and <see cref="Bone.ShearY"/>.</summary>
	public class ShearTimeline : CurveTimeline2, IBoneTimeline {
		readonly int boneIndex;

		public ShearTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, //
				(int)Property.ShearX + "|" + boneIndex, //
				(int)Property.ShearY + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
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

	/// <summary>Changes a bone's local <see cref="Bone.ShearX"/>.</summary>
	public class ShearXTimeline : CurveTimeline1, IBoneTimeline {
		readonly int boneIndex;

		public ShearXTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, (int)Property.ShearX + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
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
					return;
				case MixBlend.First:
					bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
					return;
				}
				return;
			}

			float x = GetCurveValue(time);
			switch (blend) {
			case MixBlend.Setup:
				bone.shearX = bone.data.shearX + x * alpha;
				break;
			case MixBlend.First:
			case MixBlend.Replace:
				bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
				break;
			case MixBlend.Add:
				bone.shearX += x * alpha;
				break;
			}
		}
	}

	/// <summary>Changes a bone's local <see cref="Bone.ShearY"/>.</summary>
	public class ShearYTimeline : CurveTimeline1, IBoneTimeline {
		readonly int boneIndex;

		public ShearYTimeline (int frameCount, int bezierCount, int boneIndex)
			: base(frameCount, bezierCount, (int)Property.ShearY + "|" + boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int BoneIndex {
			get {
				return boneIndex;
			}
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Bone bone = skeleton.bones.Items[boneIndex];
			if (!bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					bone.shearY = bone.data.shearY;
					return;
				case MixBlend.First:
					bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
					return;
				}
				return;
			}

			float y = GetCurveValue(time);
			switch (blend) {
			case MixBlend.Setup:
				bone.shearY = bone.data.shearY + y * alpha;
				break;
			case MixBlend.First:
			case MixBlend.Replace:
				bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
				break;
			case MixBlend.Add:
				bone.shearY += y * alpha;
				break;
			}
		}
	}

	/// <summary>Changes a slot's <see cref="Slot.Color"/>.</summary>
	public class RGBATimeline : CurveTimeline, ISlotTimeline {
		public const int ENTRIES = 5;
		protected const int R = 1, G = 2, B = 3, A = 4;

		readonly int slotIndex;

		public RGBATimeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, //
				(int)Property.RGB + "|" + slotIndex, //
				(int)Property.Alpha + "|" + slotIndex) {
			this.slotIndex = slotIndex;
		}
		public override int FrameEntries {
			get { return ENTRIES; }
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <summary>Sets the time and color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float r, float g, float b, float a) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
			frames[frame + A] = a;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				SlotData setup = slot.data;
				switch (blend) {
				case MixBlend.Setup:
					slot.r = setup.r;
					slot.g = setup.g;
					slot.b = setup.b;
					slot.a = setup.a;
					return;
				case MixBlend.First:
					slot.r += (setup.r - slot.r) * alpha;
					slot.g += (setup.g - slot.g) * alpha;
					slot.b += (setup.b - slot.b) * alpha;
					slot.a += (setup.a - slot.a) * alpha;
					slot.ClampColor();
					return;
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
				slot.r = r;
				slot.g = g;
				slot.b = b;
				slot.a = a;
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
				slot.r = br + (r - br) * alpha;
				slot.g = bg + (g - bg) * alpha;
				slot.b = bb + (b - bb) * alpha;
				slot.a = ba + (a - ba) * alpha;
			}
			slot.ClampColor();
		}
	}

	/// <summary>Changes the RGB for a slot's <see cref="Slot.Color"/>.</summary>
	public class RGBTimeline : CurveTimeline, ISlotTimeline {
		public const int ENTRIES = 4;
		protected const int R = 1, G = 2, B = 3;

		readonly int slotIndex;

		public RGBTimeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, //
				(int)Property.RGB + "|" + slotIndex) {
			this.slotIndex = slotIndex;
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <summary>Sets the time and color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float r, float g, float b) {
			frame <<= 2;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				SlotData setup = slot.data;
				switch (blend) {
				case MixBlend.Setup:
					slot.r = setup.r;
					slot.g = setup.g;
					slot.b = setup.b;
					return;
				case MixBlend.First:
					slot.r += (setup.r - slot.r) * alpha;
					slot.g += (setup.g - slot.g) * alpha;
					slot.b += (setup.b - slot.b) * alpha;
					slot.ClampColor();
					return;
				}
				return;
			}

			float r, g, b;
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

			if (alpha == 1) {
				slot.r = r;
				slot.g = g;
				slot.b = b;
			} else {
				float br, bg, bb;
				if (blend == MixBlend.Setup) {
					SlotData setup = slot.data;
					br = setup.r;
					bg = setup.g;
					bb = setup.b;
				} else {
					br = slot.r;
					bg = slot.g;
					bb = slot.b;
				}
				slot.r = br + (r - br) * alpha;
				slot.g = bg + (g - bg) * alpha;
				slot.b = bb + (b - bb) * alpha;
			}
			slot.ClampColor();
		}
	}

	/// <summary>Changes the alpha for a slot's <see cref="Slot.Color"/>.</summary>
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

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				SlotData setup = slot.data;
				switch (blend) {
				case MixBlend.Setup:
					slot.a = setup.a;
					return;
				case MixBlend.First:
					slot.a += (setup.a - slot.a) * alpha;
					slot.ClampColor();
					return;
				}
				return;
			}

			float a = GetCurveValue(time);
			if (alpha == 1)
				slot.a = a;
			else {
				if (blend == MixBlend.Setup) slot.a = slot.data.a;
				slot.a += (a - slot.a) * alpha;
			}
			slot.ClampColor();
		}
	}

	/// <summary>Changes a slot's <see cref="Slot.Color"/> and <see cref="Slot.DarkColor"/> for two color tinting.</summary>
	public class RGBA2Timeline : CurveTimeline, ISlotTimeline {
		public const int ENTRIES = 8;
		protected const int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;

		readonly int slotIndex;

		public RGBA2Timeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, //
				(int)Property.RGB + "|" + slotIndex, //
				(int)Property.Alpha + "|" + slotIndex, //
				(int)Property.RGB2 + "|" + slotIndex) {
			this.slotIndex = slotIndex;
		}

		public override int FrameEntries {
			get {
				return ENTRIES;
			}
		}

		/// <summary>
		/// The index of the slot in <see cref="Skeleton.Slots"/> that will be changed when this timeline is applied. The
		/// <see cref="Slot"/> must have a dark color available.</summary>
		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <summary>Sets the time, light color, and dark color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
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

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				SlotData setup = slot.data;
				switch (blend) {
				case MixBlend.Setup:
					slot.r = setup.r;
					slot.g = setup.g;
					slot.b = setup.b;
					slot.a = setup.a;
					slot.ClampColor();
					slot.r2 = setup.r2;
					slot.g2 = setup.g2;
					slot.b2 = setup.b2;
					slot.ClampSecondColor();
					return;
				case MixBlend.First:
					slot.r += (slot.r - setup.r) * alpha;
					slot.g += (slot.g - setup.g) * alpha;
					slot.b += (slot.b - setup.b) * alpha;
					slot.a += (slot.a - setup.a) * alpha;
					slot.ClampColor();
					slot.r2 += (slot.r2 - setup.r2) * alpha;
					slot.g2 += (slot.g2 - setup.g2) * alpha;
					slot.b2 += (slot.b2 - setup.b2) * alpha;
					slot.ClampSecondColor();
					return;
				}
				return;
			}

			float r, g, b, a, r2, g2, b2;
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
				slot.r = r;
				slot.g = g;
				slot.b = b;
				slot.a = a;
				slot.r2 = r2;
				slot.g2 = g2;
				slot.b2 = b2;
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
				slot.r = br + (r - br) * alpha;
				slot.g = bg + (g - bg) * alpha;
				slot.b = bb + (b - bb) * alpha;
				slot.a = ba + (a - ba) * alpha;
				slot.r2 = br2 + (r2 - br2) * alpha;
				slot.g2 = bg2 + (g2 - bg2) * alpha;
				slot.b2 = bb2 + (b2 - bb2) * alpha;
			}
			slot.ClampColor();
			slot.ClampSecondColor();
		}
	}

	/// <summary>Changes the RGB for a slot's <see cref="Slot.Color"/> and <see cref="Slot.DarkColor"/> for two color tinting.</summary>
	public class RGB2Timeline : CurveTimeline, ISlotTimeline {
		public const int ENTRIES = 7;
		protected const int R = 1, G = 2, B = 3, R2 = 4, G2 = 5, B2 = 6;

		readonly int slotIndex;

		public RGB2Timeline (int frameCount, int bezierCount, int slotIndex)
			: base(frameCount, bezierCount, //
				(int)Property.RGB + "|" + slotIndex, //
				(int)Property.RGB2 + "|" + slotIndex) {
			this.slotIndex = slotIndex;
		}

		public override int FrameEntries {
			get {
				return ENTRIES;
			}
		}

		/// <summary>
		/// The index of the slot in <see cref="Skeleton.Slots"/> that will be changed when this timeline is applied. The
		/// <see cref="Slot"/> must have a dark color available.</summary>
		public int SlotIndex {
			get {
				return slotIndex;
			}
		}

		/// <summary>Sets the time, light color, and dark color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
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

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				SlotData setup = slot.data;
				switch (blend) {
				case MixBlend.Setup:
					slot.r = setup.r;
					slot.g = setup.g;
					slot.b = setup.b;
					slot.ClampColor();
					slot.r2 = setup.r2;
					slot.g2 = setup.g2;
					slot.b2 = setup.b2;
					slot.ClampSecondColor();
					return;
				case MixBlend.First:
					slot.r += (slot.r - setup.r) * alpha;
					slot.g += (slot.g - setup.g) * alpha;
					slot.b += (slot.b - setup.b) * alpha;
					slot.ClampColor();
					slot.r2 += (slot.r2 - setup.r2) * alpha;
					slot.g2 += (slot.g2 - setup.g2) * alpha;
					slot.b2 += (slot.b2 - setup.b2) * alpha;
					slot.ClampSecondColor();
					return;
				}
				return;
			}

			float r, g, b, r2, g2, b2;
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

			if (alpha == 1) {
				slot.r = r;
				slot.g = g;
				slot.b = b;
				slot.r2 = r2;
				slot.g2 = g2;
				slot.b2 = b2;
			} else {
				float br, bg, bb, br2, bg2, bb2;
				if (blend == MixBlend.Setup) {
					SlotData setup = slot.data;
					br = setup.r;
					bg = setup.g;
					bb = setup.b;
					br2 = setup.r2;
					bg2 = setup.g2;
					bb2 = setup.b2;
				} else {
					br = slot.r;
					bg = slot.g;
					bb = slot.b;
					br2 = slot.r2;
					bg2 = slot.g2;
					bb2 = slot.b2;
				}
				slot.r = br + (r - br) * alpha;
				slot.g = bg + (g - bg) * alpha;
				slot.b = bb + (b - bb) * alpha;
				slot.r2 = br2 + (r2 - br2) * alpha;
				slot.g2 = bg2 + (g2 - bg2) * alpha;
				slot.b2 = bb2 + (b2 - bb2) * alpha;
			}
			slot.ClampColor();
			slot.ClampSecondColor();
		}
	}

	/// <summary>Changes a slot's <see cref="Slot.Attachment"/>.</summary>
	public class AttachmentTimeline : Timeline, ISlotTimeline {
		readonly int slotIndex;
		readonly string[] attachmentNames;

		public AttachmentTimeline (int frameCount, int slotIndex)
			: base(frameCount, (int)Property.Attachment + "|" + slotIndex) {
			this.slotIndex = slotIndex;
			attachmentNames = new String[frameCount];
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
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, String attachmentName) {
			frames[frame] = time;
			attachmentNames[frame] = attachmentName;
		}

		public override void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
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

			SetAttachment(skeleton, slot, attachmentNames[Search(frames, time)]);
		}

		private void SetAttachment (Skeleton skeleton, Slot slot, string attachmentName) {
			slot.Attachment = attachmentName == null ? null : skeleton.GetAttachment(slotIndex, attachmentName);
		}
	}

	/// <summary>Changes a slot's <see cref="Slot.Deform"/> to deform a <see cref="VertexAttachment"/>.</summary>
	public class DeformTimeline : CurveTimeline, ISlotTimeline {
		readonly int slotIndex;
		readonly VertexAttachment attachment;
		internal float[][] vertices;

		public DeformTimeline (int frameCount, int bezierCount, int slotIndex, VertexAttachment attachment)
			: base(frameCount, bezierCount, (int)Property.Deform + "|" + slotIndex + "|" + attachment.Id) {
			this.slotIndex = slotIndex;
			this.attachment = attachment;
			vertices = new float[frameCount][];
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
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
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		/// <param name="vertices">Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights.</param>
		public void SetFrame (int frame, float time, float[] vertices) {
			frames[frame] = time;
			this.vertices[frame] = vertices;
		}

		/// <param name="value1">Ignored (0 is used for a deform timeline).</param>
		/// <param name="value2">Ignored (1 is used for a deform timeline).</param>
		public void setBezier (int bezier, int frame, int value, float time1, float value1, float cx1, float cy1, float cx2,
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
		/// <param name="frame">The frame before <code>time</code>.</param>
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

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {

			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			VertexAttachment vertexAttachment = slot.attachment as VertexAttachment;
			if (vertexAttachment == null || vertexAttachment.TimelineAttachment != attachment) return;

			ExposedList<float> deformArray = slot.deform;
			if (deformArray.Count == 0) blend = MixBlend.Setup;

			float[][] vertices = this.vertices;
			int vertexCount = vertices[0].Length;

			float[] deform;

			float[] frames = this.frames;
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

					// Ensure size and preemptively set count.
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

			int frame = Search(frames, time);
			float percent = GetCurvePercent(time, frame);
			float[] prevVertices = vertices[frame];
			float[] nextVertices = vertices[frame + 1];

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
				case MixBlend.Add:
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

	/// <summary>Fires an <see cref="Event"/> when specific animation times are reached.</summary>
	public class EventTimeline : Timeline {
		readonly static string[] propertyIds = { ((int)Property.Event).ToString() };
		readonly Event[] events;

		public EventTimeline (int frameCount)
			: base(frameCount, propertyIds) {
			events = new Event[frameCount];
		}

		/// <summary>The event for each frame.</summary>
		public Event[] Events {
			get {
				return events;
			}
		}

		/// <summary>Sets the time and event for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
		public void SetFrame (int frame, Event e) {
			frames[frame] = e.time;
			events[frame] = e;
		}

		/// <summary>Fires events for frames &gt; <code>lastTime</code> and &lt;= <code>time</code>.</summary>
		public override void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha,
			MixBlend blend, MixDirection direction) {

			if (firedEvents == null) return;

			float[] frames = this.frames;
			int frameCount = frames.Length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				Apply(skeleton, lastTime, int.MaxValue, firedEvents, alpha, blend, direction);
				lastTime = -1f;
			} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return; // Time is before first frame.

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
		static readonly string[] propertyIds = { ((int)Property.DrawOrder).ToString() };

		readonly int[][] drawOrders;

		public DrawOrderTimeline (int frameCount)
			: base(frameCount, propertyIds) {
			drawOrders = new int[frameCount][];
		}

		/// <summary>The draw order for each frame. </summary>
		/// <seealso cref="Timeline.SetFrame(int, float, int[])"/>.
		public int[][] DrawOrders {
			get {
				return drawOrders;
			}
		}

		/// <summary>Sets the time and draw order for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		/// <param name="drawOrder">For each slot in <see cref="Skeleton.Slots"/>, the index of the slot in the new draw order. May be null to use
		///					 setup pose draw order.</param>
		public void SetFrame (int frame, float time, int[] drawOrder) {
			frames[frame] = time;
			drawOrders[frame] = drawOrder;
		}

		public override void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
							MixDirection direction) {

			if (direction == MixDirection.Out) {
				if (blend == MixBlend.Setup) Array.Copy(skeleton.slots.Items, 0, skeleton.drawOrder.Items, 0, skeleton.slots.Count);
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
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

	/// <summary>Changes an IK constraint's <see cref="IkConstraint.Mix"/>, <see cref="IkConstraint.Softness"/>,
	/// <see cref="IkConstraint.BendDirection"/>, <see cref="IkConstraint.Stretch"/>, and <see cref="IkConstraint.Compress"/>.</summary>
	public class IkConstraintTimeline : CurveTimeline {
		public const int ENTRIES = 6;
		private const int MIX = 1, SOFTNESS = 2, BEND_DIRECTION = 3, COMPRESS = 4, STRETCH = 5;

		readonly int ikConstraintIndex;

		public IkConstraintTimeline (int frameCount, int bezierCount, int ikConstraintIndex)
			: base(frameCount, bezierCount, (int)Property.IkConstraint + "|" + ikConstraintIndex) {
			this.ikConstraintIndex = ikConstraintIndex;
		}

		public override int FrameEntries {
			get {
				return ENTRIES;
			}
		}

		/// <summary>The index of the IK constraint slot in <see cref="Skeleton.IkConstraints"/> that will be changed when this timeline is
		/// applied.</summary>
		public int IkConstraintIndex {
			get {
				return ikConstraintIndex;
			}
		}

		/// <summary>Sets the time, mix, softness, bend direction, compress, and stretch for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
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
				constraint.mix = constraint.data.mix + (mix - constraint.data.mix) * alpha;
				constraint.softness = constraint.data.softness + (softness - constraint.data.softness) * alpha;
				if (direction == MixDirection.Out) {
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
				} else {
					constraint.bendDirection = (int)frames[i + BEND_DIRECTION];
					constraint.compress = frames[i + COMPRESS] != 0;
					constraint.stretch = frames[i + STRETCH] != 0;
				}
			} else {
				constraint.mix += (mix - constraint.mix) * alpha;
				constraint.softness += (softness - constraint.softness) * alpha;
				if (direction == MixDirection.In) {
					constraint.bendDirection = (int)frames[i + BEND_DIRECTION];
					constraint.compress = frames[i + COMPRESS] != 0;
					constraint.stretch = frames[i + STRETCH] != 0;
				}
			}
		}
	}

	///	<summary>Changes a transform constraint's mixes.</summary>
	public class TransformConstraintTimeline : CurveTimeline {
		public const int ENTRIES = 7;
		private const int ROTATE = 1, X = 2, Y = 3, SCALEX = 4, SCALEY = 5, SHEARY = 6;

		readonly int transformConstraintIndex;

		public TransformConstraintTimeline (int frameCount, int bezierCount, int transformConstraintIndex)
			: base(frameCount, bezierCount, (int)Property.TransformConstraint + "|" + transformConstraintIndex) {
			this.transformConstraintIndex = transformConstraintIndex;
		}

		public override int FrameEntries {
			get {
				return ENTRIES;
			}
		}

		/// <summary>The index of the transform constraint slot in <see cref="Skeleton.TransformConstraints"/> that will be changed when this
		/// timeline is applied.</summary>
		public int TransformConstraintIndex {
			get {
				return transformConstraintIndex;
			}
		}

		/// <summary>Sets the time, rotate mix, translate mix, scale mix, and shear mix for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
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

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			TransformConstraint constraint = skeleton.transformConstraints.Items[transformConstraintIndex];
			if (!constraint.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				TransformConstraintData data = constraint.data;
				switch (blend) {
				case MixBlend.Setup:
					constraint.mixRotate = data.mixRotate;
					constraint.mixX = data.mixX;
					constraint.mixY = data.mixY;
					constraint.mixScaleX = data.mixScaleX;
					constraint.mixScaleY = data.mixScaleY;
					constraint.mixShearY = data.mixShearY;
					return;
				case MixBlend.First:
					constraint.mixRotate += (data.mixRotate - constraint.mixRotate) * alpha;
					constraint.mixX += (data.mixX - constraint.mixX) * alpha;
					constraint.mixY += (data.mixY - constraint.mixY) * alpha;
					constraint.mixScaleX += (data.mixScaleX - constraint.mixScaleX) * alpha;
					constraint.mixScaleY += (data.mixScaleY - constraint.mixScaleY) * alpha;
					constraint.mixShearY += (data.mixShearY - constraint.mixShearY) * alpha;
					return;
				}
				return;
			}

			float rotate, x, y, scaleX, scaleY, shearY;
			GetCurveValue(out rotate, out x, out y, out scaleX, out scaleY, out shearY, time);

			if (blend == MixBlend.Setup) {
				TransformConstraintData data = constraint.data;
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

	/// <summary>Changes a path constraint's <see cref="PathConstraint.Position"/>.</summary>
	public class PathConstraintPositionTimeline : CurveTimeline1 {
		readonly int pathConstraintIndex;

		public PathConstraintPositionTimeline (int frameCount, int bezierCount, int pathConstraintIndex)
			: base(frameCount, bezierCount, (int)Property.PathConstraintPosition + "|" + pathConstraintIndex) {
			this.pathConstraintIndex = pathConstraintIndex;
		}

		/// <summary>The index of the path constraint slot in <see cref="Skeleton.PathConstraints"/> that will be changed when this timeline
		/// is applied.</summary>
		public int PathConstraintIndex {
			get {
				return pathConstraintIndex;
			}
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
			if (!constraint.active) return;

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

			float position = GetCurveValue(time);
			if (blend == MixBlend.Setup)
				constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
			else
				constraint.position += (position - constraint.position) * alpha;
		}
	}

	/// <summary>Changes a path constraint's <see cref="PathConstraint.Spacing"/>.</summary>
	public class PathConstraintSpacingTimeline : CurveTimeline1 {
		readonly int pathConstraintIndex;

		public PathConstraintSpacingTimeline (int frameCount, int bezierCount, int pathConstraintIndex)
			: base(frameCount, bezierCount, (int)Property.PathConstraintSpacing + "|" + pathConstraintIndex) {
			this.pathConstraintIndex = pathConstraintIndex;
		}

		/// <summary>The index of the path constraint slot in <see cref="Skeleton.PathConstraints"/> that will be changed when this timeline
		/// is applied.</summary>
		public int PathConstraintIndex {
			get {
				return pathConstraintIndex;
			}
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

			float spacing = GetCurveValue(time);
			if (blend == MixBlend.Setup)
				constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
			else
				constraint.spacing += (spacing - constraint.spacing) * alpha;
		}
	}

	/// <summary> Changes a transform constraint's <see cref="PathConstraint.MixRotate"/>, <see cref="PathConstraint.MixX"/>, and
	/// <see cref="PathConstraint.MixY"/>.</summary>
	public class PathConstraintMixTimeline : CurveTimeline {
		public const int ENTRIES = 4;
		private const int ROTATE = 1, X = 2, Y = 3;

		readonly int pathConstraintIndex;

		public PathConstraintMixTimeline (int frameCount, int bezierCount, int pathConstraintIndex)
			: base(frameCount, bezierCount, (int)Property.PathConstraintMix + "|" + pathConstraintIndex) {
			this.pathConstraintIndex = pathConstraintIndex;
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		/// <summary>The index of the path constraint slot in <see cref="Skeleton.PathConstraints"/> that will be changed when this timeline
		/// is applied.</summary>
		public int PathConstraintIndex {
			get {
				return pathConstraintIndex;
			}
		}

		/// <summary>Sets the time and color for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
		/// <param name="time">The frame time in seconds.</param>
		public void SetFrame (int frame, float time, float mixRotate, float mixX, float mixY) {
			frame <<= 2;
			frames[frame] = time;
			frames[frame + ROTATE] = mixRotate;
			frames[frame + X] = mixX;
			frames[frame + Y] = mixY;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
									MixDirection direction) {
			PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
			if (!constraint.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					constraint.mixRotate = constraint.data.mixRotate;
					constraint.mixX = constraint.data.mixX;
					constraint.mixY = constraint.data.mixY;
					return;
				case MixBlend.First:
					constraint.mixRotate += (constraint.data.mixRotate - constraint.mixRotate) * alpha;
					constraint.mixX += (constraint.data.mixX - constraint.mixX) * alpha;
					constraint.mixY += (constraint.data.mixY - constraint.mixY) * alpha;
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
				PathConstraintData data = constraint.data;
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

	/// <summary>Changes a slot's <see cref="Slot.SequenceIndex"/> for an attachment's <see cref="Sequence"/>.</summary>
	public class SequenceTimeline : Timeline, ISlotTimeline {
		public const int ENTRIES = 3;
		private const int MODE = 1, DELAY = 2;

		readonly int slotIndex;
		readonly IHasTextureRegion attachment;

		public SequenceTimeline (int frameCount, int slotIndex, Attachment attachment)
			: base(frameCount, (int)Property.Sequence + "|" + slotIndex + "|" + ((IHasTextureRegion)attachment).Sequence.Id) {
			this.slotIndex = slotIndex;
			this.attachment = (IHasTextureRegion)attachment;
		}

		public override int FrameEntries {
			get { return ENTRIES; }
		}

		public int SlotIndex {
			get {
				return slotIndex;
			}
		}
		public Attachment Attachment {
			get {
				return (Attachment)attachment;
			}
		}

		/// <summary>Sets the time, mode, index, and frame time for the specified frame.</summary>
		/// <param name="frame">Between 0 and <code>frameCount</code>, inclusive.</param>
		/// <param name="time">Seconds between frames.</param>
		public void SetFrame (int frame, float time, SequenceMode mode, int index, float delay) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + MODE] = (int)mode | (index << 4);
			frames[frame + DELAY] = delay;
		}

		override public void Apply (Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend,
			MixDirection direction) {

			Slot slot = skeleton.slots.Items[slotIndex];
			if (!slot.bone.active) return;
			Attachment slotAttachment = slot.attachment;
			if (slotAttachment != attachment) {
				VertexAttachment vertexAttachment = slotAttachment as VertexAttachment;
				if ((vertexAttachment == null)
					|| vertexAttachment.TimelineAttachment != attachment) return;
			}
			Sequence sequence = ((IHasTextureRegion)slotAttachment).Sequence;
			if (sequence == null) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (blend == MixBlend.Setup || blend == MixBlend.First) slot.SequenceIndex = -1;
				return;
			}

			int i = Search(frames, time, ENTRIES);
			float before = frames[i];
			int modeAndIndex = (int)frames[i + MODE];
			float delay = frames[i + DELAY];

			int index = modeAndIndex >> 4, count = sequence.Regions.Length;
			SequenceMode mode = (SequenceMode)(modeAndIndex & 0xf);
			if (mode != SequenceMode.Hold) {
				index += (int)((time - before) / delay + 0.00001f);
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
			slot.SequenceIndex = index;
		}
	}
}
