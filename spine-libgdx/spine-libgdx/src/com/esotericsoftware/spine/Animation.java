/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.Animation.MixBlend.*;
import static com.esotericsoftware.spine.Animation.MixDirection.*;
import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.IntSet;
import com.badlogic.gdx.utils.Null;
import com.badlogic.gdx.utils.ObjectSet;

import com.esotericsoftware.spine.BoneData.Inherit;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.HasSequence;
import com.esotericsoftware.spine.attachments.Sequence;
import com.esotericsoftware.spine.attachments.Sequence.SequenceMode;
import com.esotericsoftware.spine.attachments.VertexAttachment;

/** Stores a list of timelines to animate a skeleton's pose over time. */
public class Animation {
	final String name;
	float duration;
	Array<Timeline> timelines;
	final ObjectSet<String> timelineIds;
	final IntArray bones;

	public Animation (String name, Array<Timeline> timelines, float duration) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
		this.duration = duration;
		int n = timelines.size << 1;
		timelineIds = new ObjectSet(n);
		bones = new IntArray(n);
		setTimelines(timelines);
	}

	/** If the returned array or the timelines it contains are modified, {@link #setTimelines(Array)} must be called. */
	public Array<Timeline> getTimelines () {
		return timelines;
	}

	public void setTimelines (Array<Timeline> timelines) {
		if (timelines == null) throw new IllegalArgumentException("timelines cannot be null.");
		this.timelines = timelines;

		int n = timelines.size;
		timelineIds.clear(n << 1);
		bones.clear();
		var boneSet = new IntSet();
		Timeline[] items = timelines.items;
		for (int i = 0; i < n; i++) {
			Timeline timeline = items[i];
			timelineIds.addAll(timeline.getPropertyIds());
			if (timeline instanceof BoneTimeline boneTimeline && boneSet.add(boneTimeline.getBoneIndex()))
				bones.add(boneTimeline.getBoneIndex());
		}
		bones.shrink();
	}

	/** Returns true if this animation contains a timeline with any of the specified property IDs. */
	public boolean hasTimeline (String[] propertyIds) {
		for (String id : propertyIds)
			if (timelineIds.contains(id)) return true;
		return false;
	}

	/** The duration of the animation in seconds, which is usually the highest time of all frames in the timeline. The duration is
	 * used to know when it has completed and when it should loop back to the start. */
	public float getDuration () {
		return duration;
	}

	public void setDuration (float duration) {
		this.duration = duration;
	}

	public IntArray getBones () {
		return bones;
	}

	/** Applies the animation's timelines to the specified skeleton.
	 * <p>
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection, boolean)}.
	 * @param skeleton The skeleton the animation is being applied to. This provides access to the bones, slots, and other skeleton
	 *           components the timelines may change.
	 * @param lastTime The last time in seconds this animation was applied. Some timelines trigger only at specific times rather
	 *           than every frame. Pass -1 the first time an animation is applied to ensure frame 0 is triggered.
	 * @param time The time in seconds the skeleton is being posed for. Most timelines find the frame before and the frame after
	 *           this time and interpolate between the frame values. If beyond the {@link #getDuration()} and <code>loop</code> is
	 *           true then the animation will repeat, else the last frame will be applied.
	 * @param loop If true, the animation repeats after the {@link #getDuration()}.
	 * @param events If any events are fired, they are added to this list. Can be null to ignore fired events or if no timelines
	 *           fire events.
	 * @param alpha 0 applies the current or setup values (depending on <code>blend</code>). 1 applies the timeline values. Between
	 *           0 and 1 applies values between the current or setup values and the timeline values. By adjusting
	 *           <code>alpha</code> over time, an animation can be mixed in or out. <code>alpha</code> can also be useful to apply
	 *           animations on top of each other (layering).
	 * @param blend Controls how mixing is applied when <code>alpha</code> < 1.
	 * @param direction Indicates whether the timelines are mixing in or out. Used by timelines which perform instant transitions,
	 *           such as {@link DrawOrderTimeline} or {@link AttachmentTimeline}. */
	public void apply (Skeleton skeleton, float lastTime, float time, boolean loop, @Null Array<Event> events, float alpha,
		MixBlend blend, MixDirection direction, boolean appliedPose) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		if (loop && duration != 0) {
			time %= duration;
			if (lastTime > 0) lastTime %= duration;
		}

		Timeline[] timelines = this.timelines.items;
		for (int i = 0, n = this.timelines.size; i < n; i++)
			timelines[i].apply(skeleton, lastTime, time, events, alpha, blend, direction, appliedPose);
	}

	/** The animation's name, which is unique across all animations in the skeleton. */
	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}

	/** Controls how timeline values are mixed with setup pose values or current pose values when a timeline is applied with
	 * <code>alpha</code> < 1.
	 * <p>
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection, boolean)}. */
	static public enum MixBlend {
		/** Transitions between the setup and timeline values (the current value is not used). Before the first frame, the setup
		 * value is used.
		 * <p>
		 * <code>setup</code> is intended to transition to or from the setup pose, not for animations layered on top of others. */
		setup,
		/** Transitions between the current and timeline values. Before the first frame, transitions between the current and setup
		 * values. Timelines which perform instant transitions, such as {@link DrawOrderTimeline} or {@link AttachmentTimeline}, use
		 * the setup value before the first frame.
		 * <p>
		 * <code>first</code> is intended for the first animations applied, not for animations layered on top of others. */
		first,
		/** Transitions between the current and timeline values. No change is made before the first frame.
		 * <p>
		 * <code>replace</code> is intended for animations layered on top of others, not for the first animations applied. */
		replace,
		/** Transitions between the current value and the current plus timeline values. No change is made before the first frame.
		 * <p>
		 * <code>add</code> is intended for animations layered on top of others, not for the first animations applied.
		 * <p>
		 * Properties set by additive animations must be set manually or by another animation before applying the additive
		 * animations, else the property values will increase each time the additive animations are applied. */
		add
	}

	/** Indicates whether a timeline's <code>alpha</code> is mixing out over time toward 0 (the setup or current pose value) or
	 * mixing in toward 1 (the timeline's value). Some timelines use this to decide how values are applied.
	 * <p>
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection, boolean)}. */
	static public enum MixDirection {
		in, out
	}

	static private enum Property {
		rotate, x, y, scaleX, scaleY, shearX, shearY, inherit, //
		rgb, alpha, rgb2, //
		attachment, deform, //
		event, drawOrder, //
		ikConstraint, transformConstraint, //
		pathConstraintPosition, pathConstraintSpacing, pathConstraintMix, //
		physicsConstraintInertia, physicsConstraintStrength, physicsConstraintDamping, physicsConstraintMass, //
		physicsConstraintWind, physicsConstraintGravity, physicsConstraintMix, physicsConstraintReset, //
		sequence, //
		sliderTime, sliderMix
	}

	/** The base class for all timelines. */
	static abstract public class Timeline {
		private final String[] propertyIds;
		final float[] frames;

		/** @param propertyIds Unique identifiers for the properties the timeline modifies. */
		public Timeline (int frameCount, String... propertyIds) {
			if (propertyIds == null) throw new IllegalArgumentException("propertyIds cannot be null.");
			this.propertyIds = propertyIds;
			frames = new float[frameCount * getFrameEntries()];
		}

		/** Uniquely encodes both the type of this timeline and the skeleton properties that it affects. */
		public String[] getPropertyIds () {
			return propertyIds;
		}

		/** The time in seconds and any other values for each frame. */
		public float[] getFrames () {
			return frames;
		}

		/** The number of entries stored per frame. */
		public int getFrameEntries () {
			return 1;
		}

		/** The number of frames for this timeline. */
		public int getFrameCount () {
			return frames.length / getFrameEntries();
		}

		public float getDuration () {
			return frames[frames.length - getFrameEntries()];
		}

		/** Applies this timeline to the skeleton.
		 * @param skeleton The skeleton to which the timeline is being applied. This provides access to the bones, slots, and other
		 *           skeleton components that the timeline may change.
		 * @param lastTime The last time in seconds this timeline was applied. Timelines such as {@link EventTimeline} trigger only
		 *           at specific times rather than every frame. In that case, the timeline triggers everything between
		 *           <code>lastTime</code> (exclusive) and <code>time</code> (inclusive). Pass -1 the first time an animation is
		 *           applied to ensure frame 0 is triggered.
		 * @param time The time in seconds that the skeleton is being posed for. Most timelines find the frame before and the frame
		 *           after this time and interpolate between the frame values. If beyond the last frame, the last frame will be
		 *           applied.
		 * @param events If any events are fired, they are added to this list. Can be null to ignore fired events or if the timeline
		 *           does not fire events.
		 * @param alpha 0 applies the current or setup value (depending on <code>blend</code>). 1 applies the timeline value.
		 *           Between 0 and 1 applies a value between the current or setup value and the timeline value. By adjusting
		 *           <code>alpha</code> over time, an animation can be mixed in or out. <code>alpha</code> can also be useful to
		 *           apply animations on top of each other (layering).
		 * @param blend Controls how mixing is applied when <code>alpha</code> < 1.
		 * @param direction Indicates whether the timeline is mixing in or out. Used by timelines which perform instant transitions,
		 *           such as {@link DrawOrderTimeline} or {@link AttachmentTimeline}, and others such as {@link ScaleTimeline}.
		 * @param appliedPose True to to modify the applied pose. */
		abstract public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha,
			MixBlend blend, MixDirection direction, boolean appliedPose);

		/** Linear search using a stride of 1.
		 * @param time Must be >= the first value in <code>frames</code>.
		 * @return The index of the first value <= <code>time</code>. */
		static int search (float[] frames, float time) {
			int n = frames.length;
			for (int i = 1; i < n; i++)
				if (frames[i] > time) return i - 1;
			return n - 1;
		}

		/** Linear search using the specified stride.
		 * @param time Must be >= the first value in <code>frames</code>.
		 * @return The index of the first value <= <code>time</code>. */
		static int search (float[] frames, float time, int step) {
			int n = frames.length;
			for (int i = step; i < n; i += step)
				if (frames[i] > time) return i - step;
			return n - step;
		}
	}

	/** An interface for timelines which change the property of a slot. */
	static public interface SlotTimeline {
		/** The index of the slot in {@link Skeleton#getSlots()} that will be changed when this timeline is applied. */
		public int getSlotIndex ();
	}

	/** The base class for timelines that interpolate between frame values using stepped, linear, or a Bezier curve. */
	static abstract public class CurveTimeline extends Timeline {
		static public final int LINEAR = 0, STEPPED = 1, BEZIER = 2, BEZIER_SIZE = 18;

		float[] curves;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
		 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
		public CurveTimeline (int frameCount, int bezierCount, String... propertyIds) {
			super(frameCount, propertyIds);
			curves = new float[frameCount + bezierCount * BEZIER_SIZE];
			curves[frameCount - 1] = STEPPED;
		}

		/** Sets the specified frame to linear interpolation.
		 * @param frame Between 0 and <code>frameCount - 1</code>, inclusive. */
		public void setLinear (int frame) {
			curves[frame] = LINEAR;
		}

		/** Sets the specified frame to stepped interpolation.
		 * @param frame Between 0 and <code>frameCount - 1</code>, inclusive. */
		public void setStepped (int frame) {
			curves[frame] = STEPPED;
		}

		/** Returns the interpolation type for the specified frame.
		 * @param frame Between 0 and <code>frameCount - 1</code>, inclusive.
		 * @return {@link #LINEAR}, {@link #STEPPED}, or {@link #BEZIER} + the index of the Bezier segments. */
		public int getCurveType (int frame) {
			return (int)curves[frame];
		}

		/** Shrinks the storage for Bezier curves, for use when <code>bezierCount</code> (specified in the constructor) was larger
		 * than the actual number of Bezier curves. */
		public void shrink (int bezierCount) {
			int size = getFrameCount() + bezierCount * BEZIER_SIZE;
			if (curves.length > size) {
				var newCurves = new float[size];
				arraycopy(curves, 0, newCurves, 0, size);
				curves = newCurves;
			}
		}

		/** Stores the segments for the specified Bezier curve. For timelines that modify multiple values, there may be more than
		 * one curve per frame.
		 * @param bezier The ordinal of this Bezier curve for this timeline, between 0 and <code>bezierCount - 1</code> (specified
		 *           in the constructor), inclusive.
		 * @param frame Between 0 and <code>frameCount - 1</code>, inclusive.
		 * @param value The index of the value for the frame this curve is used for.
		 * @param time1 The time for the first key.
		 * @param value1 The value for the first key.
		 * @param cx1 The time for the first Bezier handle.
		 * @param cy1 The value for the first Bezier handle.
		 * @param cx2 The time of the second Bezier handle.
		 * @param cy2 The value for the second Bezier handle.
		 * @param time2 The time for the second key.
		 * @param value2 The value for the second key. */
		public void setBezier (int bezier, int frame, int value, float time1, float value1, float cx1, float cy1, float cx2,
			float cy2, float time2, float value2) {
			float[] curves = this.curves;
			int i = getFrameCount() + bezier * BEZIER_SIZE;
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

		/** Returns the Bezier interpolated value for the specified time.
		 * @param frameIndex The index into {@link #getFrames()} for the values of the frame before <code>time</code>.
		 * @param valueOffset The offset from <code>frameIndex</code> to the value this curve is used for.
		 * @param i The index of the Bezier segments. See {@link #getCurveType(int)}. */
		public float getBezierValue (float time, int frameIndex, int valueOffset, int i) {
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
			frameIndex += getFrameEntries();
			float x = curves[n - 2], y = curves[n - 1];
			return y + (time - x) / (frames[frameIndex] - x) * (frames[frameIndex + valueOffset] - y);
		}
	}

	/** The base class for a {@link CurveTimeline} that sets one property. */
	static abstract public class CurveTimeline1 extends CurveTimeline {
		static public final int ENTRIES = 2;
		static final int VALUE = 1;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
		 * @param propertyId Unique identifier for the property the timeline modifies. */
		public CurveTimeline1 (int frameCount, int bezierCount, String propertyId) {
			super(frameCount, bezierCount, propertyId);
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** Sets the time and value for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, float value) {
			frame <<= 1;
			frames[frame] = time;
			frames[frame + VALUE] = value;
		}

		/** Returns the interpolated value for the specified time. */
		public float getCurveValue (float time) {
			float[] frames = this.frames;
			int i = frames.length - 2;
			for (int ii = 2; ii <= i; ii += 2) {
				if (frames[ii] > time) {
					i = ii - 2;
					break;
				}
			}

			int curveType = (int)curves[i >> 1];
			return switch (curveType) {
			case LINEAR -> {
				float before = frames[i], value = frames[i + VALUE];
				yield value + (time - before) / (frames[i + ENTRIES] - before) * (frames[i + ENTRIES + VALUE] - value);
			}
			case STEPPED -> frames[i + VALUE];
			default -> getBezierValue(time, i, VALUE, curveType - BEZIER);
			};
		}

		public float getRelativeValue (float time, float alpha, MixBlend blend, float current, float setup) {
			if (time < frames[0]) {
				return switch (blend) {
				case setup -> setup;
				case first -> current + (setup - current) * alpha;
				default -> current;
				};
			}
			float value = getCurveValue(time);
			return switch (blend) {
			case setup -> setup + value * alpha;
			case first, replace -> current + (value + setup - current) * alpha;
			case add -> current + value * alpha;
			};
		}

		public float getAbsoluteValue (float time, float alpha, MixBlend blend, float current, float setup) {
			if (time < frames[0]) {
				return switch (blend) {
				case setup -> setup;
				case first -> current + (setup - current) * alpha;
				default -> current;
				};
			}
			float value = getCurveValue(time);
			return switch (blend) {
			case setup -> setup + (value - setup) * alpha;
			case first, replace -> current + (value - current) * alpha;
			case add -> current + value * alpha;
			};
		}

		public float getAbsoluteValue (float time, float alpha, MixBlend blend, float current, float setup, float value) {
			if (time < frames[0]) {
				return switch (blend) {
				case setup -> setup;
				case first -> current + (setup - current) * alpha;
				default -> current;
				};
			}
			return switch (blend) {
			case setup -> setup + (value - setup) * alpha;
			case first, replace -> current + (value - current) * alpha;
			case add -> current + value * alpha;
			};
		}

		public float getScaleValue (float time, float alpha, MixBlend blend, MixDirection direction, float current, float setup) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				return switch (blend) {
				case setup -> setup;
				case first -> current + (setup - current) * alpha;
				default -> current;
				};
			}
			float value = getCurveValue(time) * setup;
			if (alpha == 1) return blend == add ? current + value - setup : value;
			// Mixing out uses sign of setup or current pose, else use sign of key.
			if (direction == out) {
				switch (blend) {
				case setup:
					return setup + (Math.abs(value) * Math.signum(setup) - setup) * alpha;
				case first:
				case replace:
					return current + (Math.abs(value) * Math.signum(current) - current) * alpha;
				}
			} else {
				float s;
				switch (blend) {
				case setup:
					s = Math.abs(setup) * Math.signum(value);
					return s + (value - s) * alpha;
				case first:
				case replace:
					s = Math.abs(current) * Math.signum(value);
					return s + (value - s) * alpha;
				}
			}
			return current + (value - setup) * alpha;
		}
	}

	/** The base class for a {@link CurveTimeline} that is a {@link BoneTimeline} and sets two properties. */
	static abstract public class BoneTimeline2 extends CurveTimeline implements BoneTimeline {
		static public final int ENTRIES = 3;
		static final int VALUE1 = 1, VALUE2 = 2;

		final int boneIndex;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}. */
		public BoneTimeline2 (int frameCount, int bezierCount, int boneIndex, Property property1, Property property2) {
			super(frameCount, bezierCount, property1.ordinal() + "|" + boneIndex, property2.ordinal() + "|" + boneIndex);
			this.boneIndex = boneIndex;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** Sets the time and values for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, float value1, float value2) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + VALUE1] = value1;
			frames[frame + VALUE2] = value2;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			Bone bone = skeleton.bones.items[boneIndex];
			if (bone.active) apply(appliedPose ? bone.applied : bone.pose, bone.data.setup, time, alpha, blend, direction);
		}

		abstract protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend,
			MixDirection direction);
	}

	/** An interface for timelines which change the property of a bone. */
	static public interface BoneTimeline {
		/** The index of the bone in {@link Skeleton#getBones()} that will be changed when this timeline is applied. */
		public int getBoneIndex ();
	}

	static abstract public class BoneTimeline1 extends CurveTimeline1 implements BoneTimeline {
		final int boneIndex;

		public BoneTimeline1 (int frameCount, int bezierCount, int boneIndex, Property property) {
			super(frameCount, bezierCount, property.ordinal() + "|" + boneIndex);
			this.boneIndex = boneIndex;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			Bone bone = skeleton.bones.items[boneIndex];
			if (bone.active) apply(appliedPose ? bone.applied : bone.pose, bone.data.setup, time, alpha, blend, direction);
		}

		abstract protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend,
			MixDirection direction);
	}

	/** Changes a bone's local {@link BoneLocal#getRotation()}. */
	static public class RotateTimeline extends BoneTimeline1 {
		public RotateTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.rotate);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.rotation = getRelativeValue(time, alpha, blend, pose.rotation, setup.rotation);
		}
	}

	/** Changes a bone's local {@link BoneLocal#getX()} and {@link BoneLocal#getY()}. */
	static public class TranslateTimeline extends BoneTimeline2 {
		public TranslateTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.x, Property.y);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case setup -> {
					pose.x = setup.x;
					pose.y = setup.y;
				}
				case first -> {
					pose.x += (setup.x - pose.x) * alpha;
					pose.y += (setup.y - pose.y) * alpha;
				}
				}
				return;
			}

			float x, y;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR -> {
				float before = frames[i];
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				x += (frames[i + ENTRIES + VALUE1] - x) * t;
				y += (frames[i + ENTRIES + VALUE2] - y) * t;
			}
			case STEPPED -> {
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
			}
			default -> {
				x = getBezierValue(time, i, VALUE1, curveType - BEZIER);
				y = getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
			}
			}

			switch (blend) {
			case setup -> {
				pose.x = setup.x + x * alpha;
				pose.y = setup.y + y * alpha;
			}
			case first, replace -> {
				pose.x += (setup.x + x - pose.x) * alpha;
				pose.y += (setup.y + y - pose.y) * alpha;
			}
			case add -> {
				pose.x += x * alpha;
				pose.y += y * alpha;
			}
			}
		}
	}

	/** Changes a bone's local {@link BoneLocal#getX()}. */
	static public class TranslateXTimeline extends BoneTimeline1 {
		public TranslateXTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.x);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.x = getRelativeValue(time, alpha, blend, pose.x, setup.x);
		}
	}

	/** Changes a bone's local {@link BoneLocal#getY()}. */
	static public class TranslateYTimeline extends BoneTimeline1 {
		public TranslateYTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.y);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.y = getRelativeValue(time, alpha, blend, pose.y, setup.y);
		}
	}

	/** Changes a bone's local {@link BoneLocal#getScaleX()} and {@link BoneLocal#getScaleY()}. */
	static public class ScaleTimeline extends BoneTimeline2 {
		public ScaleTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.scaleX, Property.scaleY);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case setup -> {
					pose.scaleX = setup.scaleX;
					pose.scaleY = setup.scaleY;
				}
				case first -> {
					pose.scaleX += (setup.scaleX - pose.scaleX) * alpha;
					pose.scaleY += (setup.scaleY - pose.scaleY) * alpha;
				}
				}
				return;
			}

			float x, y;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR -> {
				float before = frames[i];
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				x += (frames[i + ENTRIES + VALUE1] - x) * t;
				y += (frames[i + ENTRIES + VALUE2] - y) * t;
			}
			case STEPPED -> {
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
			}
			default -> {
				x = getBezierValue(time, i, VALUE1, curveType - BEZIER);
				y = getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
			}
			}
			x *= setup.scaleX;
			y *= setup.scaleY;

			if (alpha == 1) {
				if (blend == add) {
					pose.scaleX += x - setup.scaleX;
					pose.scaleY += y - setup.scaleY;
				} else {
					pose.scaleX = x;
					pose.scaleY = y;
				}
			} else {
				// Mixing out uses sign of setup or current pose, else use sign of key.
				float bx, by;
				if (direction == out) {
					switch (blend) {
					case setup -> {
						bx = setup.scaleX;
						by = setup.scaleY;
						pose.scaleX = bx + (Math.abs(x) * Math.signum(bx) - bx) * alpha;
						pose.scaleY = by + (Math.abs(y) * Math.signum(by) - by) * alpha;
					}
					case first, replace -> {
						bx = pose.scaleX;
						by = pose.scaleY;
						pose.scaleX = bx + (Math.abs(x) * Math.signum(bx) - bx) * alpha;
						pose.scaleY = by + (Math.abs(y) * Math.signum(by) - by) * alpha;
					}
					case add -> {
						pose.scaleX += (x - setup.scaleX) * alpha;
						pose.scaleY += (y - setup.scaleY) * alpha;
					}
					}
				} else {
					switch (blend) {
					case setup -> {
						bx = Math.abs(setup.scaleX) * Math.signum(x);
						by = Math.abs(setup.scaleY) * Math.signum(y);
						pose.scaleX = bx + (x - bx) * alpha;
						pose.scaleY = by + (y - by) * alpha;
					}
					case first, replace -> {
						bx = Math.abs(pose.scaleX) * Math.signum(x);
						by = Math.abs(pose.scaleY) * Math.signum(y);
						pose.scaleX = bx + (x - bx) * alpha;
						pose.scaleY = by + (y - by) * alpha;
					}
					case add -> {
						pose.scaleX += (x - setup.scaleX) * alpha;
						pose.scaleY += (y - setup.scaleY) * alpha;
					}
					}
				}
			}
		}
	}

	/** Changes a bone's local {@link BoneLocal#getScaleX()}. */
	static public class ScaleXTimeline extends BoneTimeline1 {
		public ScaleXTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.scaleX);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.scaleX = getScaleValue(time, alpha, blend, direction, pose.scaleX, setup.scaleX);
		}
	}

	/** Changes a bone's local {@link BoneLocal#getScaleY()}. */
	static public class ScaleYTimeline extends BoneTimeline1 {
		public ScaleYTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.scaleY);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.scaleY = getScaleValue(time, alpha, blend, direction, pose.scaleY, setup.scaleY);
		}
	}

	/** Changes a bone's local {@link BoneLocal#getShearX()} and {@link BoneLocal#getShearY()}. */
	static public class ShearTimeline extends BoneTimeline2 {
		public ShearTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.shearX, Property.shearY);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case setup -> {
					pose.shearX = setup.shearX;
					pose.shearY = setup.shearY;
				}
				case first -> {
					pose.shearX += (setup.shearX - pose.shearX) * alpha;
					pose.shearY += (setup.shearY - pose.shearY) * alpha;
				}
				}
				return;
			}

			float x, y;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR -> {
				float before = frames[i];
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				x += (frames[i + ENTRIES + VALUE1] - x) * t;
				y += (frames[i + ENTRIES + VALUE2] - y) * t;
			}
			case STEPPED -> {
				x = frames[i + VALUE1];
				y = frames[i + VALUE2];
			}
			default -> {
				x = getBezierValue(time, i, VALUE1, curveType - BEZIER);
				y = getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
			}
			}

			switch (blend) {
			case setup -> {
				pose.shearX = setup.shearX + x * alpha;
				pose.shearY = setup.shearY + y * alpha;
			}
			case first, replace -> {
				pose.shearX += (setup.shearX + x - pose.shearX) * alpha;
				pose.shearY += (setup.shearY + y - pose.shearY) * alpha;
			}
			case add -> {
				pose.shearX += x * alpha;
				pose.shearY += y * alpha;
			}
			}
		}
	}

	/** Changes a bone's local {@link BoneLocal#getShearX()}. */
	static public class ShearXTimeline extends BoneTimeline1 {
		public ShearXTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.shearX);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.shearX = getRelativeValue(time, alpha, blend, pose.shearX, setup.shearX);
		}
	}

	/** Changes a bone's local {@link BoneLocal#getShearY()}. */
	static public class ShearYTimeline extends BoneTimeline1 {
		public ShearYTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.shearY);
		}

		protected void apply (BoneLocal pose, BoneLocal setup, float time, float alpha, MixBlend blend, MixDirection direction) {
			pose.shearY = getRelativeValue(time, alpha, blend, pose.shearY, setup.shearY);
		}
	}

	/** Changes a bone's {@link BoneLocal#getInherit()}. */
	static public class InheritTimeline extends Timeline implements BoneTimeline {
		static public final int ENTRIES = 2;
		static private final int INHERIT = 1;

		final int boneIndex;

		public InheritTimeline (int frameCount, int boneIndex) {
			super(frameCount, Property.inherit.ordinal() + "|" + boneIndex);
			this.boneIndex = boneIndex;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** Sets the inherit transform mode for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, Inherit inherit) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + INHERIT] = inherit.ordinal();
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			Bone bone = skeleton.bones.items[boneIndex];
			if (!bone.active) return;
			BoneLocal pose = appliedPose ? bone.applied : bone.pose;

			if (direction == out) {
				if (blend == setup) pose.inherit = bone.data.setup.inherit;
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (blend == setup || blend == first) pose.inherit = bone.data.setup.inherit;
			} else
				pose.inherit = Inherit.values[(int)frames[search(frames, time, ENTRIES) + INHERIT]];
		}
	}

	static abstract public class SlotCurveTimeline extends CurveTimeline implements SlotTimeline {
		final int slotIndex;

		public SlotCurveTimeline (int frameCount, int bezierCount, int slotIndex, String... propertyIds) {
			super(frameCount, bezierCount, propertyIds);
			this.slotIndex = slotIndex;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			Slot slot = skeleton.slots.items[slotIndex];
			if (slot.bone.active) apply(slot, appliedPose ? slot.applied : slot.pose, time, alpha, blend);
		}

		abstract protected void apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend);
	}

	/** Changes a slot's {@link SlotPose#getColor()}. */
	static public class RGBATimeline extends SlotCurveTimeline {
		static public final int ENTRIES = 5;
		static private final int R = 1, G = 2, B = 3, A = 4;

		public RGBATimeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, slotIndex, //
				Property.rgb.ordinal() + "|" + slotIndex, //
				Property.alpha.ordinal() + "|" + slotIndex);
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** Sets the time and color for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, float r, float g, float b, float a) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
			frames[frame + A] = a;
		}

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			Color color = pose.color;
			float[] frames = this.frames;
			if (time < frames[0]) {
				Color setup = slot.data.setup.color;
				switch (blend) {
				case setup -> color.set(setup);
				case first -> color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
					(setup.a - color.a) * alpha);
				}
				return;
			}

			float r, g, b, a;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR -> {
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
			}
			case STEPPED -> {
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				a = frames[i + A];
			}
			default -> {
				r = getBezierValue(time, i, R, curveType - BEZIER);
				g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				a = getBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER);
			}
			}

			if (alpha == 1)
				color.set(r, g, b, a);
			else {
				if (blend == setup) {
					Color setup = slot.data.setup.color;
					color.set(setup.r + (r - setup.r) * alpha, setup.g + (g - setup.g) * alpha, setup.b + (b - setup.b) * alpha,
						setup.a + (a - setup.a) * alpha);
				} else
					color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
		}
	}

	/** Changes the RGB for a slot's {@link SlotPose#getColor()}. */
	static public class RGBTimeline extends SlotCurveTimeline {
		static public final int ENTRIES = 4;
		static private final int R = 1, G = 2, B = 3;

		public RGBTimeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, slotIndex, Property.rgb.ordinal() + "|" + slotIndex);
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** Sets the time and color for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, float r, float g, float b) {
			frame <<= 2;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
		}

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			Color color = pose.color;
			float r, g, b;
			float[] frames = this.frames;
			if (time < frames[0]) {
				Color setup = slot.data.setup.color;
				switch (blend) {
				case setup:
					color.r = setup.r;
					color.g = setup.g;
					color.b = setup.b;
					// Fall through.
				default:
					return;
				case first:
					r = color.r + (setup.r - color.r) * alpha;
					g = color.g + (setup.g - color.g) * alpha;
					b = color.b + (setup.b - color.b) * alpha;
				}
			} else {
				int i = search(frames, time, ENTRIES), curveType = (int)curves[i >> 2];
				switch (curveType) {
				case LINEAR -> {
					float before = frames[i];
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					float t = (time - before) / (frames[i + ENTRIES] - before);
					r += (frames[i + ENTRIES + R] - r) * t;
					g += (frames[i + ENTRIES + G] - g) * t;
					b += (frames[i + ENTRIES + B] - b) * t;
				}
				case STEPPED -> {
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
				}
				default -> {
					r = getBezierValue(time, i, R, curveType - BEZIER);
					g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
					b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				}
				}

				if (alpha != 1) {
					if (blend == setup) {
						Color setup = slot.data.setup.color;
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
			color.r = r < 0 ? 0 : (r > 1 ? 1 : r);
			color.g = g < 0 ? 0 : (g > 1 ? 1 : g);
			color.b = b < 0 ? 0 : (b > 1 ? 1 : b);
		}
	}

	/** Changes the alpha for a slot's {@link SlotPose#getColor()}. */
	static public class AlphaTimeline extends CurveTimeline1 implements SlotTimeline {
		final int slotIndex;

		public AlphaTimeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, Property.alpha.ordinal() + "|" + slotIndex);
			this.slotIndex = slotIndex;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			Slot slot = skeleton.slots.items[slotIndex];
			if (!slot.bone.active) return;

			Color color = (appliedPose ? slot.applied : slot.pose).color;
			float a;
			float[] frames = this.frames;
			if (time < frames[0]) {
				Color setup = slot.data.setup.color;
				switch (blend) {
				case setup:
					color.a = setup.a;
					// Fall through.
				default:
					return;
				case first:
					a = color.a + (setup.a - color.a) * alpha;
				}
			} else {
				a = getCurveValue(time);
				if (alpha != 1) {
					if (blend == setup) {
						Color setup = slot.data.setup.color;
						a = setup.a + (a - setup.a) * alpha;
					} else
						a = color.a + (a - color.a) * alpha;
				}
			}
			color.a = a < 0 ? 0 : (a > 1 ? 1 : a);
		}
	}

	/** Changes a slot's {@link SlotPose#getColor()} and {@link SlotPose#getDarkColor()} for two color tinting. */
	static public class RGBA2Timeline extends SlotCurveTimeline {
		static public final int ENTRIES = 8;
		static private final int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;

		public RGBA2Timeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, slotIndex, //
				Property.rgb.ordinal() + "|" + slotIndex, //
				Property.alpha.ordinal() + "|" + slotIndex, //
				Property.rgb2.ordinal() + "|" + slotIndex);
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** Sets the time, light color, and dark color for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, float r, float g, float b, float a, float r2, float g2, float b2) {
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

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			Color light = pose.color, dark = pose.darkColor;
			float r2, g2, b2;
			float[] frames = this.frames;
			if (time < frames[0]) {
				SlotPose setup = slot.data.setup;
				Color setupLight = setup.color, setupDark = setup.darkColor;
				switch (blend) {
				case setup:
					light.set(setupLight);
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
					// Fall through.
				default:
					return;
				case first:
					light.add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
						(setupLight.a - light.a) * alpha);
					r2 = dark.r + (setupDark.r - dark.r) * alpha;
					g2 = dark.g + (setupDark.g - dark.g) * alpha;
					b2 = dark.b + (setupDark.b - dark.b) * alpha;
				}
			} else {
				float r, g, b, a;
				int i = search(frames, time, ENTRIES), curveType = (int)curves[i >> 3];
				switch (curveType) {
				case LINEAR -> {
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
				}
				case STEPPED -> {
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					a = frames[i + A];
					r2 = frames[i + R2];
					g2 = frames[i + G2];
					b2 = frames[i + B2];
				}
				default -> {
					r = getBezierValue(time, i, R, curveType - BEZIER);
					g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
					b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
					a = getBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER);
					r2 = getBezierValue(time, i, R2, curveType + BEZIER_SIZE * 4 - BEZIER);
					g2 = getBezierValue(time, i, G2, curveType + BEZIER_SIZE * 5 - BEZIER);
					b2 = getBezierValue(time, i, B2, curveType + BEZIER_SIZE * 6 - BEZIER);
				}
				}

				if (alpha == 1)
					light.set(r, g, b, a);
				else if (blend == setup) {
					SlotPose setupPose = slot.data.setup;
					Color setup = setupPose.color;
					light.set(setup.r + (r - setup.r) * alpha, setup.g + (g - setup.g) * alpha, setup.b + (b - setup.b) * alpha,
						setup.a + (a - setup.a) * alpha);
					setup = setupPose.darkColor;
					r2 = setup.r + (r2 - setup.r) * alpha;
					g2 = setup.g + (g2 - setup.g) * alpha;
					b2 = setup.b + (b2 - setup.b) * alpha;
				} else {
					light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
					r2 = dark.r + (r2 - dark.r) * alpha;
					g2 = dark.g + (g2 - dark.g) * alpha;
					b2 = dark.b + (b2 - dark.b) * alpha;
				}
			}
			dark.r = r2 < 0 ? 0 : (r2 > 1 ? 1 : r2);
			dark.g = g2 < 0 ? 0 : (g2 > 1 ? 1 : g2);
			dark.b = b2 < 0 ? 0 : (b2 > 1 ? 1 : b2);
		}
	}

	/** Changes the RGB for a slot's {@link SlotPose#getColor()} and {@link SlotPose#getDarkColor()} for two color tinting. */
	static public class RGB2Timeline extends SlotCurveTimeline {
		static public final int ENTRIES = 7;
		static private final int R = 1, G = 2, B = 3, R2 = 4, G2 = 5, B2 = 6;

		public RGB2Timeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, slotIndex, //
				Property.rgb.ordinal() + "|" + slotIndex, //
				Property.rgb2.ordinal() + "|" + slotIndex);
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** Sets the time, light color, and dark color for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, float r, float g, float b, float r2, float g2, float b2) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
			frames[frame + R2] = r2;
			frames[frame + G2] = g2;
			frames[frame + B2] = b2;
		}

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			Color light = pose.color, dark = pose.darkColor;
			float r, g, b, r2, g2, b2;
			float[] frames = this.frames;
			if (time < frames[0]) {
				SlotPose setup = slot.data.setup;
				Color setupLight = setup.color, setupDark = setup.darkColor;
				switch (blend) {
				case setup:
					light.r = setupLight.r;
					light.g = setupLight.g;
					light.b = setupLight.b;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
					// Fall through.
				default:
					return;
				case first:
					r = light.r + (setupLight.r - light.r) * alpha;
					g = light.g + (setupLight.g - light.g) * alpha;
					b = light.b + (setupLight.b - light.b) * alpha;
					r2 = dark.r + (setupDark.r - dark.r) * alpha;
					g2 = dark.g + (setupDark.g - dark.g) * alpha;
					b2 = dark.b + (setupDark.b - dark.b) * alpha;
				}
			} else {
				int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
				switch (curveType) {
				case LINEAR -> {
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
				}
				case STEPPED -> {
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					r2 = frames[i + R2];
					g2 = frames[i + G2];
					b2 = frames[i + B2];
				}
				default -> {
					r = getBezierValue(time, i, R, curveType - BEZIER);
					g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
					b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
					r2 = getBezierValue(time, i, R2, curveType + BEZIER_SIZE * 3 - BEZIER);
					g2 = getBezierValue(time, i, G2, curveType + BEZIER_SIZE * 4 - BEZIER);
					b2 = getBezierValue(time, i, B2, curveType + BEZIER_SIZE * 5 - BEZIER);
				}
				}

				if (alpha != 1) {
					if (blend == setup) {
						SlotPose setupPose = slot.data.setup;
						Color setup = setupPose.color;
						r = setup.r + (r - setup.r) * alpha;
						g = setup.g + (g - setup.g) * alpha;
						b = setup.b + (b - setup.b) * alpha;
						setup = setupPose.darkColor;
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
			light.r = r < 0 ? 0 : (r > 1 ? 1 : r);
			light.g = g < 0 ? 0 : (g > 1 ? 1 : g);
			light.b = b < 0 ? 0 : (b > 1 ? 1 : b);
			dark.r = r2 < 0 ? 0 : (r2 > 1 ? 1 : r2);
			dark.g = g2 < 0 ? 0 : (g2 > 1 ? 1 : g2);
			dark.b = b2 < 0 ? 0 : (b2 > 1 ? 1 : b2);
		}
	}

	/** Changes a slot's {@link SlotPose#getAttachment()}. */
	static public class AttachmentTimeline extends Timeline implements SlotTimeline {
		final int slotIndex;
		final String[] attachmentNames;

		public AttachmentTimeline (int frameCount, int slotIndex) {
			super(frameCount, Property.attachment.ordinal() + "|" + slotIndex);
			this.slotIndex = slotIndex;
			attachmentNames = new String[frameCount];
		}

		public int getFrameCount () {
			return frames.length;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		/** The attachment name for each frame. May contain null values to clear the attachment. */
		public String[] getAttachmentNames () {
			return attachmentNames;
		}

		/** Sets the time and attachment name for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, @Null String attachmentName) {
			frames[frame] = time;
			attachmentNames[frame] = attachmentName;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			Slot slot = skeleton.slots.items[slotIndex];
			if (!slot.bone.active) return;
			SlotPose pose = appliedPose ? slot.applied : slot.pose;

			if (direction == out) {
				if (blend == setup) setAttachment(skeleton, pose, slot.data.attachmentName);
			} else if (time < this.frames[0]) {
				if (blend == setup || blend == first) setAttachment(skeleton, pose, slot.data.attachmentName);
			} else
				setAttachment(skeleton, pose, attachmentNames[search(this.frames, time)]);
		}

		private void setAttachment (Skeleton skeleton, SlotPose pose, @Null String attachmentName) {
			pose.setAttachment(attachmentName == null ? null : skeleton.getAttachment(slotIndex, attachmentName));
		}
	}

	/** Changes a slot's {@link SlotPose#getDeform()} to deform a {@link VertexAttachment}. */
	static public class DeformTimeline extends SlotCurveTimeline {
		final VertexAttachment attachment;
		private final float[][] vertices;

		public DeformTimeline (int frameCount, int bezierCount, int slotIndex, VertexAttachment attachment) {
			super(frameCount, bezierCount, slotIndex, Property.deform.ordinal() + "|" + slotIndex + "|" + attachment.getId());
			this.attachment = attachment;
			vertices = new float[frameCount][];
		}

		public int getFrameCount () {
			return frames.length;
		}

		/** The attachment that will be deformed.
		 * <p>
		 * See {@link VertexAttachment#getTimelineAttachment()}. */
		public VertexAttachment getAttachment () {
			return attachment;
		}

		/** The vertices for each frame. */
		public float[][] getVertices () {
			return vertices;
		}

		/** Sets the time and vertices for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds.
		 * @param vertices Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights. */
		public void setFrame (int frame, float time, float[] vertices) {
			frames[frame] = time;
			this.vertices[frame] = vertices;
		}

		/** @param value1 Ignored (0 is used for a deform timeline).
		 * @param value2 Ignored (1 is used for a deform timeline). */
		public void setBezier (int bezier, int frame, int value, float time1, float value1, float cx1, float cy1, float cx2,
			float cy2, float time2, float value2) {
			float[] curves = this.curves;
			int i = getFrameCount() + bezier * BEZIER_SIZE;
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

		/** Returns the interpolated percentage for the specified time.
		 * @param frame The frame before <code>time</code>. */
		private float getCurvePercent (float time, int frame) {
			float[] curves = this.curves;
			int i = (int)curves[frame];
			switch (i) {
			case LINEAR:
				float x = frames[frame];
				return (time - x) / (frames[frame + getFrameEntries()] - x);
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
			float x = curves[n - 2], y = curves[n - 1];
			return y + (1 - y) * (time - x) / (frames[frame + getFrameEntries()] - x);
		}

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, MixBlend blend) {
			if (!(pose.attachment instanceof VertexAttachment vertexAttachment)
				|| vertexAttachment.getTimelineAttachment() != attachment) return;

			FloatArray deformArray = pose.deform;
			if (deformArray.size == 0) blend = setup;

			float[][] vertices = this.vertices;
			int vertexCount = vertices[0].length;

			float[] frames = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case setup -> deformArray.clear();
				case first -> {
					if (alpha == 1) {
						deformArray.clear();
						return;
					}
					float[] deform = deformArray.setSize(vertexCount);
					if (vertexAttachment.getBones() == null) { // Unweighted vertex positions.
						float[] setupVertices = vertexAttachment.getVertices();
						for (int i = 0; i < vertexCount; i++)
							deform[i] += (setupVertices[i] - deform[i]) * alpha;
					} else { // Weighted deform offsets.
						alpha = 1 - alpha;
						for (int i = 0; i < vertexCount; i++)
							deform[i] *= alpha;
					}
				}
				}
				return;
			}

			float[] deform = deformArray.setSize(vertexCount);

			if (time >= frames[frames.length - 1]) { // Time is after last frame.
				float[] lastVertices = vertices[frames.length - 1];
				if (alpha == 1) {
					if (blend == add) {
						if (vertexAttachment.getBones() == null) { // Unweighted vertex positions, no alpha.
							float[] setupVertices = vertexAttachment.getVertices();
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] - setupVertices[i];
						} else { // Weighted deform offsets, no alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i];
						}
					} else // Vertex positions or deform offsets, no alpha.
						arraycopy(lastVertices, 0, deform, 0, vertexCount);
				} else {
					switch (blend) {
					case setup -> {
						if (vertexAttachment.getBones() == null) { // Unweighted vertex positions, with alpha.
							float[] setupVertices = vertexAttachment.getVertices();
							for (int i = 0; i < vertexCount; i++) {
								float setup = setupVertices[i];
								deform[i] = setup + (lastVertices[i] - setup) * alpha;
							}
						} else { // Weighted deform offsets, with alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] = lastVertices[i] * alpha;
						}
					}
					case first, replace -> { // Vertex positions or deform offsets, with alpha.
						for (int i = 0; i < vertexCount; i++)
							deform[i] += (lastVertices[i] - deform[i]) * alpha;
					}
					case add -> {
						if (vertexAttachment.getBones() == null) { // Unweighted vertex positions, no alpha.
							float[] setupVertices = vertexAttachment.getVertices();
							for (int i = 0; i < vertexCount; i++)
								deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
						} else { // Weighted deform offsets, alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] * alpha;
						}
					}
					}
				}
				return;
			}

			int frame = search(frames, time);
			float percent = getCurvePercent(time, frame);
			float[] prevVertices = vertices[frame];
			float[] nextVertices = vertices[frame + 1];

			if (alpha == 1) {
				if (blend == add) {
					if (vertexAttachment.getBones() == null) { // Unweighted vertex positions, no alpha.
						float[] setupVertices = vertexAttachment.getVertices();
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
				} else if (percent == 0)
					arraycopy(prevVertices, 0, deform, 0, vertexCount);
				else { // Vertex positions or deform offsets, no alpha.
					for (int i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						deform[i] = prev + (nextVertices[i] - prev) * percent;
					}
				}
			} else {
				switch (blend) {
				case setup -> {
					if (vertexAttachment.getBones() == null) { // Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.getVertices();
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
				}
				case first, replace -> {// Vertex positions or deform offsets, with alpha.
					for (int i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
					}
				}
				case add -> {
					if (vertexAttachment.getBones() == null) { // Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.getVertices();
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
				}
				}
			}
		}
	}

	/** Changes a slot's {@link SlotPose#getSequenceIndex()} for an attachment's {@link Sequence}. */
	static public class SequenceTimeline extends Timeline implements SlotTimeline {
		static public final int ENTRIES = 3;
		static private final int MODE = 1, DELAY = 2;

		final int slotIndex;
		final HasSequence attachment;

		public SequenceTimeline (int frameCount, int slotIndex, Attachment attachment) {
			super(frameCount, Property.sequence.ordinal() + "|" + slotIndex + "|" + ((HasSequence)attachment).getSequence().getId());
			this.slotIndex = slotIndex;
			this.attachment = (HasSequence)attachment;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		/** The attachment for which the {@link SlotPose#getSequenceIndex()} will be set.
		 * <p>
		 * See {@link VertexAttachment#getTimelineAttachment()}. */
		public Attachment getAttachment () {
			return (Attachment)attachment;
		}

		/** Sets the time, mode, index, and frame time for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param delay Seconds between frames. */
		public void setFrame (int frame, float time, SequenceMode mode, int index, float delay) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + MODE] = mode.ordinal() | (index << 4);
			frames[frame + DELAY] = delay;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			Slot slot = skeleton.slots.items[slotIndex];
			if (!slot.bone.active) return;
			SlotPose pose = appliedPose ? slot.applied : slot.pose;

			Attachment slotAttachment = pose.attachment;
			if (!(slotAttachment instanceof HasSequence hasSequence) || slotAttachment.getTimelineAttachment() != attachment) return;

			if (direction == out) {
				if (blend == setup) pose.setSequenceIndex(-1);
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (blend == setup || blend == first) pose.setSequenceIndex(-1);
				return;
			}

			int i = search(frames, time, ENTRIES);
			float before = frames[i];
			int modeAndIndex = (int)frames[i + MODE];
			float delay = frames[i + DELAY];

			int index = modeAndIndex >> 4, count = hasSequence.getSequence().getRegions().length;
			SequenceMode mode = SequenceMode.values[modeAndIndex & 0xf];
			if (mode != SequenceMode.hold) {
				index += (time - before) / delay + 0.0001f;
				switch (mode) {
				case once -> index = Math.min(count - 1, index);
				case loop -> index %= count;
				case pingpong -> {
					int n = (count << 1) - 2;
					index = n == 0 ? 0 : index % n;
					if (index >= count) index = n - index;
				}
				case onceReverse -> index = Math.max(count - 1 - index, 0);
				case loopReverse -> index = count - 1 - (index % count);
				case pingpongReverse -> {
					int n = (count << 1) - 2;
					index = n == 0 ? 0 : (index + count - 1) % n;
					if (index >= count) index = n - index;
				}
				}
			}
			pose.setSequenceIndex(index);
		}
	}

	/** Fires an {@link Event} when specific animation times are reached. */
	static public class EventTimeline extends Timeline {
		static private final String[] propertyIds = {Integer.toString(Property.event.ordinal())};

		private final Event[] events;

		public EventTimeline (int frameCount) {
			super(frameCount, propertyIds);
			events = new Event[frameCount];
		}

		public int getFrameCount () {
			return frames.length;
		}

		/** The event for each frame. */
		public Event[] getEvents () {
			return events;
		}

		/** Sets the time and event for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive. */
		public void setFrame (int frame, Event event) {
			frames[frame] = event.time;
			events[frame] = event;
		}

		/** Fires events for frames > <code>lastTime</code> and <= <code>time</code>. */
		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> firedEvents, float alpha,
			MixBlend blend, MixDirection direction, boolean appliedPose) {

			if (firedEvents == null) return;

			float[] frames = this.frames;
			int frameCount = frames.length;

			if (lastTime > time) { // Apply after lastTime for looped animations.
				apply(skeleton, lastTime, Integer.MAX_VALUE, firedEvents, alpha, blend, direction, appliedPose);
				lastTime = -1f;
			} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return;

			int i;
			if (lastTime < frames[0])
				i = 0;
			else {
				i = search(frames, lastTime) + 1;
				float frameTime = frames[i];
				while (i > 0) { // Fire multiple events with the same frame.
					if (frames[i - 1] != frameTime) break;
					i--;
				}
			}
			for (; i < frameCount && time >= frames[i]; i++)
				firedEvents.add(events[i]);
		}
	}

	/** Changes a skeleton's {@link Skeleton#getDrawOrder()}. */
	static public class DrawOrderTimeline extends Timeline {
		static private final String[] propertyIds = {Integer.toString(Property.drawOrder.ordinal())};

		private final int[][] drawOrders;

		public DrawOrderTimeline (int frameCount) {
			super(frameCount, propertyIds);
			drawOrders = new int[frameCount][];
		}

		public int getFrameCount () {
			return frames.length;
		}

		/** The draw order for each frame. See {@link #setFrame(int, float, int[])}. */
		public int[][] getDrawOrders () {
			return drawOrders;
		}

		/** Sets the time and draw order for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds.
		 * @param drawOrder Ordered {@link Skeleton#slots} indices, or null to use setup pose order. */
		public void setFrame (int frame, float time, @Null int[] drawOrder) {
			frames[frame] = time;
			drawOrders[frame] = drawOrder;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			if (direction == out) {
				if (blend == setup) arraycopy(skeleton.slots.items, 0, skeleton.drawOrder.items, 0, skeleton.slots.size);
				return;
			}

			if (time < frames[0]) {
				if (blend == setup || blend == first)
					arraycopy(skeleton.slots.items, 0, skeleton.drawOrder.items, 0, skeleton.slots.size);
				return;
			}

			int[] drawOrderToSetupIndex = drawOrders[search(frames, time)];
			if (drawOrderToSetupIndex == null)
				arraycopy(skeleton.slots.items, 0, skeleton.drawOrder.items, 0, skeleton.slots.size);
			else {
				Slot[] slots = skeleton.slots.items;
				Slot[] drawOrder = skeleton.drawOrder.items;
				for (int i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
					drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		}
	}

	/** Changes a subset of a skeleton's {@link Skeleton#getDrawOrder()}. */
	static public class DrawOrderFolderTimeline extends Timeline {
		private final int[] slots;
		private final boolean[] inFolder;
		private final int[][] drawOrders;

		/** @param slots {@link Skeleton#getSlots()} indices controlled by this timeline, in setup order.
		 * @param slotCount The maximum number of slots in the skeleton. */
		public DrawOrderFolderTimeline (int frameCount, int[] slots, int slotCount) {
			super(frameCount, DrawOrderTimeline.propertyIds);
			this.slots = slots;
			drawOrders = new int[frameCount][];
			inFolder = new boolean[slotCount];
			for (int i : slots)
				inFolder[i] = true;
		}

		public int getFrameCount () {
			return frames.length;
		}

		/** The {@link Skeleton#getSlots()} indices that this timeline affects, in setup order. */
		public int[] getSlots () {
			return slots;
		}

		/** The draw order for each frame. See {@link #setFrame(int, float, int[])}. */
		public int[][] getDrawOrders () {
			return drawOrders;
		}

		/** Sets the time and draw order for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds.
		 * @param drawOrder Ordered {@link #getSlots()} indices, or null to use setup pose order. */
		public void setFrame (int frame, float time, @Null int[] drawOrder) {
			frames[frame] = time;
			drawOrders[frame] = drawOrder;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			if (direction == out) {
				if (blend == setup) setup(skeleton);
			} else if (time < frames[0]) {
				if (blend == setup || blend == first) setup(skeleton);
			} else {
				int[] order = drawOrders[search(frames, time)];
				if (order == null)
					setup(skeleton);
				else
					apply(skeleton, order);
			}
		}

		private void setup (Skeleton skeleton) {
			boolean[] inFolder = this.inFolder;
			Slot[] drawOrder = skeleton.drawOrder.items, allSlots = skeleton.slots.items;
			int[] slots = this.slots;
			for (int i = 0, found = 0, done = slots.length;; i++) {
				if (inFolder[drawOrder[i].data.index]) {
					drawOrder[i] = allSlots[slots[found]];
					if (++found == done) break;
				}
			}
		}

		private void apply (Skeleton skeleton, int[] order) {
			boolean[] inFolder = this.inFolder;
			Slot[] drawOrder = skeleton.drawOrder.items, allSlots = skeleton.slots.items;
			int[] slots = this.slots;
			for (int i = 0, found = 0, done = slots.length;; i++) {
				if (inFolder[drawOrder[i].data.index]) {
					drawOrder[i] = allSlots[slots[order[found]]];
					if (++found == done) break;
				}
			}
		}
	}

	static public interface ConstraintTimeline {
		/** The index of the constraint in {@link Skeleton#getConstraints()} that will be changed when this timeline is applied, or
		 * -1 if a specific constraint will not be changed. */
		public int getConstraintIndex ();
	}

	/** Changes an IK constraint's {@link IkConstraintPose#getMix()}, {@link IkConstraintPose#getSoftness()},
	 * {@link IkConstraintPose#getBendDirection()}, {@link IkConstraintPose#getStretch()}, and
	 * {@link IkConstraintPose#getCompress()}. */
	static public class IkConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
		static public final int ENTRIES = 6;
		static private final int MIX = 1, SOFTNESS = 2, BEND_DIRECTION = 3, COMPRESS = 4, STRETCH = 5;

		final int constraintIndex;

		public IkConstraintTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, Property.ikConstraint.ordinal() + "|" + constraintIndex);
			this.constraintIndex = constraintIndex;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		public int getConstraintIndex () {
			return constraintIndex;
		}

		/** Sets the time, mix, softness, bend direction, compress, and stretch for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds.
		 * @param bendDirection 1 or -1. */
		public void setFrame (int frame, float time, float mix, float softness, int bendDirection, boolean compress,
			boolean stretch) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + MIX] = mix;
			frames[frame + SOFTNESS] = softness;
			frames[frame + BEND_DIRECTION] = bendDirection;
			frames[frame + COMPRESS] = compress ? 1 : 0;
			frames[frame + STRETCH] = stretch ? 1 : 0;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			var constraint = (IkConstraint)skeleton.constraints.items[constraintIndex];
			if (!constraint.active) return;
			IkConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;

			float[] frames = this.frames;
			if (time < frames[0]) {
				IkConstraintPose setup = constraint.data.setup;
				switch (blend) {
				case setup -> {
					pose.mix = setup.mix;
					pose.softness = setup.softness;
					pose.bendDirection = setup.bendDirection;
					pose.compress = setup.compress;
					pose.stretch = setup.stretch;
				}
				case first -> {
					pose.mix += (setup.mix - pose.mix) * alpha;
					pose.softness += (setup.softness - pose.softness) * alpha;
					pose.bendDirection = setup.bendDirection;
					pose.compress = setup.compress;
					pose.stretch = setup.stretch;
				}
				}
				return;
			}

			float mix, softness;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR -> {
				float before = frames[i];
				mix = frames[i + MIX];
				softness = frames[i + SOFTNESS];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				mix += (frames[i + ENTRIES + MIX] - mix) * t;
				softness += (frames[i + ENTRIES + SOFTNESS] - softness) * t;
			}
			case STEPPED -> {
				mix = frames[i + MIX];
				softness = frames[i + SOFTNESS];
			}
			default -> {
				mix = getBezierValue(time, i, MIX, curveType - BEZIER);
				softness = getBezierValue(time, i, SOFTNESS, curveType + BEZIER_SIZE - BEZIER);
			}
			}

			if (blend == setup) {
				IkConstraintPose setup = constraint.data.setup;
				pose.mix = setup.mix + (mix - setup.mix) * alpha;
				pose.softness = setup.softness + (softness - setup.softness) * alpha;
				if (direction == out) {
					pose.bendDirection = setup.bendDirection;
					pose.compress = setup.compress;
					pose.stretch = setup.stretch;
					return;
				}
			} else {
				pose.mix += (mix - pose.mix) * alpha;
				pose.softness += (softness - pose.softness) * alpha;
				if (direction == out) return;
			}
			pose.bendDirection = (int)frames[i + BEND_DIRECTION];
			pose.compress = frames[i + COMPRESS] != 0;
			pose.stretch = frames[i + STRETCH] != 0;
		}
	}

	/** Changes a transform constraint's {@link TransformConstraintPose#getMixRotate()}, {@link TransformConstraintPose#getMixX()},
	 * {@link TransformConstraintPose#getMixY()}, {@link TransformConstraintPose#getMixScaleX()},
	 * {@link TransformConstraintPose#getMixScaleY()}, and {@link TransformConstraintPose#getMixShearY()}. */
	static public class TransformConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
		static public final int ENTRIES = 7;
		static private final int ROTATE = 1, X = 2, Y = 3, SCALEX = 4, SCALEY = 5, SHEARY = 6;

		final int constraintIndex;

		public TransformConstraintTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, Property.transformConstraint.ordinal() + "|" + constraintIndex);
			this.constraintIndex = constraintIndex;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		public int getConstraintIndex () {
			return constraintIndex;
		}

		/** Sets the time, rotate mix, translate mix, scale mix, and shear mix for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, float mixRotate, float mixX, float mixY, float mixScaleX, float mixScaleY,
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

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			var constraint = (TransformConstraint)skeleton.constraints.items[constraintIndex];
			if (!constraint.active) return;
			TransformConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;

			float[] frames = this.frames;
			if (time < frames[0]) {
				TransformConstraintPose setup = constraint.data.setup;
				switch (blend) {
				case setup -> {
					pose.mixRotate = setup.mixRotate;
					pose.mixX = setup.mixX;
					pose.mixY = setup.mixY;
					pose.mixScaleX = setup.mixScaleX;
					pose.mixScaleY = setup.mixScaleY;
					pose.mixShearY = setup.mixShearY;
				}
				case first -> {
					pose.mixRotate += (setup.mixRotate - pose.mixRotate) * alpha;
					pose.mixX += (setup.mixX - pose.mixX) * alpha;
					pose.mixY += (setup.mixY - pose.mixY) * alpha;
					pose.mixScaleX += (setup.mixScaleX - pose.mixScaleX) * alpha;
					pose.mixScaleY += (setup.mixScaleY - pose.mixScaleY) * alpha;
					pose.mixShearY += (setup.mixShearY - pose.mixShearY) * alpha;
				}
				}
				return;
			}

			float rotate, x, y, scaleX, scaleY, shearY;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR -> {
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
			}
			case STEPPED -> {
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				scaleX = frames[i + SCALEX];
				scaleY = frames[i + SCALEY];
				shearY = frames[i + SHEARY];
			}
			default -> {
				rotate = getBezierValue(time, i, ROTATE, curveType - BEZIER);
				x = getBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER);
				y = getBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER);
				scaleX = getBezierValue(time, i, SCALEX, curveType + BEZIER_SIZE * 3 - BEZIER);
				scaleY = getBezierValue(time, i, SCALEY, curveType + BEZIER_SIZE * 4 - BEZIER);
				shearY = getBezierValue(time, i, SHEARY, curveType + BEZIER_SIZE * 5 - BEZIER);
			}
			}

			switch (blend) {
			case setup -> {
				TransformConstraintPose setup = constraint.data.setup;
				pose.mixRotate = setup.mixRotate + (rotate - setup.mixRotate) * alpha;
				pose.mixX = setup.mixX + (x - setup.mixX) * alpha;
				pose.mixY = setup.mixY + (y - setup.mixY) * alpha;
				pose.mixScaleX = setup.mixScaleX + (scaleX - setup.mixScaleX) * alpha;
				pose.mixScaleY = setup.mixScaleY + (scaleY - setup.mixScaleY) * alpha;
				pose.mixShearY = setup.mixShearY + (shearY - setup.mixShearY) * alpha;
			}
			case first, replace -> {
				pose.mixRotate += (rotate - pose.mixRotate) * alpha;
				pose.mixX += (x - pose.mixX) * alpha;
				pose.mixY += (y - pose.mixY) * alpha;
				pose.mixScaleX += (scaleX - pose.mixScaleX) * alpha;
				pose.mixScaleY += (scaleY - pose.mixScaleY) * alpha;
				pose.mixShearY += (shearY - pose.mixShearY) * alpha;
			}
			case add -> {
				pose.mixRotate += rotate * alpha;
				pose.mixX += x * alpha;
				pose.mixY += y * alpha;
				pose.mixScaleX += scaleX * alpha;
				pose.mixScaleY += scaleY * alpha;
				pose.mixShearY += shearY * alpha;
			}
			}
		}
	}

	static abstract public class ConstraintTimeline1 extends CurveTimeline1 implements ConstraintTimeline {
		final int constraintIndex;

		public ConstraintTimeline1 (int frameCount, int bezierCount, int constraintIndex, Property property) {
			super(frameCount, bezierCount, property.ordinal() + "|" + constraintIndex);
			this.constraintIndex = constraintIndex;
		}

		public int getConstraintIndex () {
			return constraintIndex;
		}
	}

	/** Changes a path constraint's {@link PathConstraintPose#getPosition()}. */
	static public class PathConstraintPositionTimeline extends ConstraintTimeline1 {
		public PathConstraintPositionTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.pathConstraintPosition);
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			var constraint = (PathConstraint)skeleton.constraints.items[constraintIndex];
			if (constraint.active) {
				PathConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;
				pose.position = getAbsoluteValue(time, alpha, blend, pose.position, constraint.data.setup.position);
			}
		}
	}

	/** Changes a path constraint's {@link PathConstraintPose#getSpacing()}. */
	static public class PathConstraintSpacingTimeline extends ConstraintTimeline1 {
		public PathConstraintSpacingTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.pathConstraintSpacing);
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			var constraint = (PathConstraint)skeleton.constraints.items[constraintIndex];
			if (constraint.active) {
				PathConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;
				pose.spacing = getAbsoluteValue(time, alpha, blend == add ? replace : blend, pose.spacing,
					constraint.data.setup.spacing);
			}
		}
	}

	/** Changes a path constraint's {@link PathConstraintPose#getMixRotate()}, {@link PathConstraintPose#getMixX()}, and
	 * {@link PathConstraintPose#getMixY()}. */
	static public class PathConstraintMixTimeline extends CurveTimeline implements ConstraintTimeline {
		static public final int ENTRIES = 4;
		static private final int ROTATE = 1, X = 2, Y = 3;

		final int constraintIndex;

		public PathConstraintMixTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, Property.pathConstraintMix.ordinal() + "|" + constraintIndex);
			this.constraintIndex = constraintIndex;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		public int getConstraintIndex () {
			return constraintIndex;
		}

		/** Sets the time and color for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, float mixRotate, float mixX, float mixY) {
			frame <<= 2;
			frames[frame] = time;
			frames[frame + ROTATE] = mixRotate;
			frames[frame + X] = mixX;
			frames[frame + Y] = mixY;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			var constraint = (PathConstraint)skeleton.constraints.items[constraintIndex];
			if (!constraint.active) return;
			PathConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;

			float[] frames = this.frames;
			if (time < frames[0]) {
				PathConstraintPose setup = constraint.data.setup;
				switch (blend) {
				case setup -> {
					pose.mixRotate = setup.mixRotate;
					pose.mixX = setup.mixX;
					pose.mixY = setup.mixY;
				}
				case first -> {
					pose.mixRotate += (setup.mixRotate - pose.mixRotate) * alpha;
					pose.mixX += (setup.mixX - pose.mixX) * alpha;
					pose.mixY += (setup.mixY - pose.mixY) * alpha;
				}
				}
				return;
			}

			float rotate, x, y;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i >> 2];
			switch (curveType) {
			case LINEAR -> {
				float before = frames[i];
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				rotate += (frames[i + ENTRIES + ROTATE] - rotate) * t;
				x += (frames[i + ENTRIES + X] - x) * t;
				y += (frames[i + ENTRIES + Y] - y) * t;
			}
			case STEPPED -> {
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
			}
			default -> {
				rotate = getBezierValue(time, i, ROTATE, curveType - BEZIER);
				x = getBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER);
				y = getBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER);
			}
			}

			if (blend == setup) {
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

	/** The base class for most {@link PhysicsConstraint} timelines. */
	static abstract public class PhysicsConstraintTimeline extends ConstraintTimeline1 {
		boolean additive;

		/** @param constraintIndex -1 for all physics constraints in the skeleton. */
		public PhysicsConstraintTimeline (int frameCount, int bezierCount, int constraintIndex, Property property) {
			super(frameCount, bezierCount, constraintIndex, property);
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			if (blend == add && !additive) blend = replace;
			if (constraintIndex == -1) {
				float value = time >= frames[0] ? getCurveValue(time) : 0;
				PhysicsConstraint[] constraints = skeleton.physics.items;
				for (int i = 0, n = skeleton.physics.size; i < n; i++) {
					PhysicsConstraint constraint = constraints[i];
					if (constraint.active && global(constraint.data)) {
						PhysicsConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;
						set(pose, getAbsoluteValue(time, alpha, blend, get(pose), get(constraint.data.setup), value));
					}
				}
			} else {
				var constraint = (PhysicsConstraint)skeleton.constraints.items[constraintIndex];
				if (constraint.active) {
					PhysicsConstraintPose pose = appliedPose ? constraint.applied : constraint.pose;
					set(pose, getAbsoluteValue(time, alpha, blend, get(pose), get(constraint.data.setup)));
				}
			}
		}

		abstract protected float get (PhysicsConstraintPose pose);

		abstract protected void set (PhysicsConstraintPose pose, float value);

		abstract protected boolean global (PhysicsConstraintData constraint);
	}

	/** Changes a physics constraint's {@link PhysicsConstraintPose#getInertia()}. */
	static public class PhysicsConstraintInertiaTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintInertiaTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintInertia);
		}

		protected float get (PhysicsConstraintPose pose) {
			return pose.inertia;
		}

		protected void set (PhysicsConstraintPose pose, float value) {
			pose.inertia = value;
		}

		protected boolean global (PhysicsConstraintData constraint) {
			return constraint.inertiaGlobal;
		}
	}

	/** Changes a physics constraint's {@link PhysicsConstraintPose#getStrength()}. */
	static public class PhysicsConstraintStrengthTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintStrengthTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintStrength);
		}

		protected float get (PhysicsConstraintPose pose) {
			return pose.strength;
		}

		protected void set (PhysicsConstraintPose pose, float value) {
			pose.strength = value;
		}

		protected boolean global (PhysicsConstraintData constraint) {
			return constraint.strengthGlobal;
		}
	}

	/** Changes a physics constraint's {@link PhysicsConstraintPose#getDamping()}. */
	static public class PhysicsConstraintDampingTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintDampingTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintDamping);
		}

		protected float get (PhysicsConstraintPose pose) {
			return pose.damping;
		}

		protected void set (PhysicsConstraintPose pose, float value) {
			pose.damping = value;
		}

		protected boolean global (PhysicsConstraintData constraint) {
			return constraint.dampingGlobal;
		}
	}

	/** Changes a physics constraint's {@link PhysicsConstraintPose#getMassInverse()}. The timeline values are not inverted. */
	static public class PhysicsConstraintMassTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintMassTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintMass);
		}

		protected float get (PhysicsConstraintPose pose) {
			return 1 / pose.massInverse;
		}

		protected void set (PhysicsConstraintPose pose, float value) {
			pose.massInverse = 1 / value;
		}

		protected boolean global (PhysicsConstraintData constraint) {
			return constraint.massGlobal;
		}
	}

	/** Changes a physics constraint's {@link PhysicsConstraintPose#getWind()}. */
	static public class PhysicsConstraintWindTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintWindTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintWind);
			additive = true;
		}

		protected float get (PhysicsConstraintPose pose) {
			return pose.wind;
		}

		protected void set (PhysicsConstraintPose pose, float value) {
			pose.wind = value;
		}

		protected boolean global (PhysicsConstraintData constraint) {
			return constraint.windGlobal;
		}
	}

	/** Changes a physics constraint's {@link PhysicsConstraintPose#getGravity()}. */
	static public class PhysicsConstraintGravityTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintGravityTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintGravity);
			additive = true;
		}

		protected float get (PhysicsConstraintPose pose) {
			return pose.gravity;
		}

		protected void set (PhysicsConstraintPose pose, float value) {
			pose.gravity = value;
		}

		protected boolean global (PhysicsConstraintData constraint) {
			return constraint.gravityGlobal;
		}
	}

	/** Changes a physics constraint's {@link PhysicsConstraintPose#getMix()}. */
	static public class PhysicsConstraintMixTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintMixTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintMix);
		}

		protected float get (PhysicsConstraintPose pose) {
			return pose.mix;
		}

		protected void set (PhysicsConstraintPose pose, float value) {
			pose.mix = value;
		}

		protected boolean global (PhysicsConstraintData constraint) {
			return constraint.mixGlobal;
		}
	}

	/** Resets a physics constraint when specific animation times are reached. */
	static public class PhysicsConstraintResetTimeline extends Timeline implements ConstraintTimeline {
		static private final String[] propertyIds = {Integer.toString(Property.physicsConstraintReset.ordinal())};

		final int constraintIndex;

		/** @param constraintIndex -1 for all physics constraints in the skeleton. */
		public PhysicsConstraintResetTimeline (int frameCount, int constraintIndex) {
			super(frameCount, propertyIds);
			this.constraintIndex = constraintIndex;
		}

		/** The index of the physics constraint in {@link Skeleton#getConstraints()} that will be reset when this timeline is
		 * applied, or -1 if all physics constraints in the skeleton will be reset. */
		public int getConstraintIndex () {
			return constraintIndex;
		}

		public int getFrameCount () {
			return frames.length;
		}

		/** Sets the time for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive. */
		public void setFrame (int frame, float time) {
			frames[frame] = time;
		}

		/** Resets the physics constraint when frames > <code>lastTime</code> and <= <code>time</code>. */
		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> firedEvents, float alpha,
			MixBlend blend, MixDirection direction, boolean appliedPose) {

			PhysicsConstraint constraint = null;
			if (constraintIndex != -1) {
				constraint = (PhysicsConstraint)skeleton.constraints.items[constraintIndex];
				if (!constraint.active) return;
			}

			float[] frames = this.frames;

			if (lastTime > time) { // Apply after lastTime for looped animations.
				apply(skeleton, lastTime, Integer.MAX_VALUE, null, alpha, blend, direction, appliedPose);
				lastTime = -1f;
			} else if (lastTime >= frames[frames.length - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return;

			if (lastTime < frames[0] || time >= frames[search(frames, lastTime) + 1]) {
				if (constraint != null)
					constraint.reset(skeleton);
				else {
					PhysicsConstraint[] constraints = skeleton.physics.items;
					for (int i = 0, n = skeleton.physics.size; i < n; i++) {
						constraint = constraints[i];
						if (constraint.active) constraint.reset(skeleton);
					}
				}
			}
		}
	}

	/** Changes a slider's {@link SliderPose#getTime()}. */
	static public class SliderTimeline extends ConstraintTimeline1 {
		public SliderTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.sliderTime);
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			var constraint = (Slider)skeleton.constraints.items[constraintIndex];
			if (constraint.active) {
				SliderPose pose = appliedPose ? constraint.applied : constraint.pose;
				pose.time = getAbsoluteValue(time, alpha, blend, pose.time, constraint.data.setup.time);
			}
		}
	}

	/** Changes a slider's {@link SliderPose#getMix()}. */
	static public class SliderMixTimeline extends ConstraintTimeline1 {
		public SliderMixTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.sliderMix);
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction, boolean appliedPose) {

			var constraint = (Slider)skeleton.constraints.items[constraintIndex];
			if (constraint.active) {
				SliderPose pose = appliedPose ? constraint.applied : constraint.pose;
				pose.mix = getAbsoluteValue(time, alpha, blend, pose.mix, constraint.data.setup.mix);
			}
		}
	}
}
