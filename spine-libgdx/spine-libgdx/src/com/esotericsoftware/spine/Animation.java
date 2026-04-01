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

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.LongSet;
import com.badlogic.gdx.utils.Null;

import com.esotericsoftware.spine.BoneData.Inherit;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.HasSequence;
import com.esotericsoftware.spine.attachments.Sequence;
import com.esotericsoftware.spine.attachments.Sequence.SequenceMode;
import com.esotericsoftware.spine.attachments.VertexAttachment;

/** Stores a list of timelines to animate a skeleton's pose over time.
 * <p>
 * See <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine Runtimes
 * Guide. */
public class Animation {
	final String name;
	float duration;
	Array<Timeline> timelines;
	LongSet timelineIds;
	IntArray bones;

	/** Creates a new animation. {@link #timelines} must be set before use. */
	public Animation (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
	}

	/** If this list or the timelines it contains are modified, the timelines must be set again to recompute the animation's bone
	 * indices and timeline property IDs.
	 * <p>
	 * See {@link #setTimelines(Array, IntArray)}. */
	public Array<Timeline> getTimelines () {
		return timelines;
	}

	/** Sets the {@link #timelines} and {@link #bones}. */
	public void setTimelines (Array<Timeline> timelines, IntArray bones) {
		if (timelines == null) throw new IllegalArgumentException("timelines cannot be null.");
		if (bones == null) throw new IllegalArgumentException("bones cannot be null.");
		this.timelines = timelines;
		this.bones = bones;

		int n = timelines.size;
		if (timelineIds == null)
			timelineIds = new LongSet(n << 1);
		else
			timelineIds.clear(n << 1);
		Timeline[] items = timelines.items;
		for (int i = 0; i < n; i++)
			timelineIds.addAll(items[i].propertyIds);
	}

	/** Returns true if this animation contains a timeline with any of the specified property IDs.
	 * <p>
	 * See {@link Timeline#propertyIds}. */
	public boolean hasTimeline (long[] propertyIds) {
		for (long id : propertyIds)
			if (timelineIds.contains(id)) return true;
		return false;
	}

	/** The duration of the animation in seconds, which is usually the highest time of all frames in the timelines. The duration is
	 * used to know when the animation has completed and, for animations that repeat, when it should loop back to the start. */
	public float getDuration () {
		return duration;
	}

	public void setDuration (float duration) {
		this.duration = duration;
	}

	/** {@link Skeleton#bones} indices that this animation's timelines modify.
	 * <p>
	 * See {@link #setTimelines(Array, IntArray)} and {@link BoneTimeline#getBoneIndex()}. */
	public IntArray getBones () {
		return bones;
	}

	/** Applies the animation's timelines to the specified skeleton.
	 * <p>
	 * See {@link Timeline#apply(Skeleton, float, float, Array, float, boolean, boolean, boolean, boolean)} and
	 * <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine Runtimes
	 * Guide.
	 * @param skeleton The skeleton the animation is applied to. This provides access to the bones, slots, and other skeleton
	 *           components the timelines may change.
	 * @param lastTime The last time in seconds this animation was applied. Some timelines trigger only at discrete times, in which
	 *           case all keys are triggered between <code>lastTime</code> (exclusive) and <code>time</code> (inclusive). Pass -1
	 *           the first time an animation is applied to ensure frame 0 is triggered.
	 * @param time The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and
	 *           interpolate between the frame values.
	 * @param loop True if <code>time</code> beyond the {@link #duration} repeats the animation, else the last frame is used.
	 * @param events If any events are fired, they are added to this list. Pass null to ignore fired events or if no timelines fire
	 *           events.
	 * @param alpha 0 applies setup or current values (depending on <code>fromSetup</code>), 1 uses timeline values, and
	 *           intermediate values interpolate between them. Adjusting <code>alpha</code> over time can mix an animation in or
	 *           out.
	 * @param fromSetup If true, <code>alpha</code> transitions between setup and timeline values, setup values are used before the
	 *           first frame (current values are not used). If false, <code>alpha</code> transitions between current and timeline
	 *           values, no change is made before the first frame.
	 * @param add If true, for timelines that support it, their values are added to the setup or current values (depending on
	 *           <code>fromSetup</code>).
	 * @param out True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant transitions.
	 * @param appliedPose True to modify {@link Posed#appliedPose}, else {@link Posed#pose} is modified. */
	public void apply (Skeleton skeleton, float lastTime, float time, boolean loop, @Null Array<Event> events, float alpha,
		boolean fromSetup, boolean add, boolean out, boolean appliedPose) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		if (loop && duration != 0) {
			time %= duration;
			if (lastTime > 0) lastTime %= duration;
		}

		Timeline[] timelines = this.timelines.items;
		for (int i = 0, n = this.timelines.size; i < n; i++)
			timelines[i].apply(skeleton, lastTime, time, events, alpha, fromSetup, add, out, appliedPose);
	}

	/** The animation's name, unique across all animations in the skeleton.
	 * <p>
	 * See {@link SkeletonData#findAnimation(String)}. */
	public String getName () {
		return name;
	}

	public String toString () {
		return name;
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

	/** The base class for all timelines.
	 * <p>
	 * See <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine
	 * Runtimes Guide. */
	static abstract public class Timeline {
		final long[] propertyIds;
		final float[] frames;
		boolean additive, instant;

		/** @param propertyIds Unique identifiers for the properties the timeline modifies. */
		public Timeline (int frameCount, long... propertyIds) {
			if (propertyIds == null) throw new IllegalArgumentException("propertyIds cannot be null.");
			this.propertyIds = propertyIds;
			frames = new float[frameCount * getFrameEntries()];
		}

		/** Uniquely encodes both the type of this timeline and the skeleton properties that it affects. */
		public long[] getPropertyIds () {
			return propertyIds;
		}

		/** The time in seconds and any other values for each frame. */
		public float[] getFrames () {
			return frames;
		}

		/** The number of values stored per frame. */
		public int getFrameEntries () {
			return 1;
		}

		/** The number of frames in this timeline. */
		public int getFrameCount () {
			return frames.length / getFrameEntries();
		}

		/** The duration of the timeline in seconds, which is usually the highest time of all frames in the timeline. */
		public float getDuration () {
			return frames[frames.length - getFrameEntries()];
		}

		/** True if this timeline supports additive blending. */
		public boolean getAdditive () {
			return additive;
		}

		/** True if this timeline sets values instantaneously and does not support interpolation between frames. */
		public boolean getInstant () {
			return instant;
		}

		/** Applies this timeline to the skeleton.
		 * <p>
		 * See <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine
		 * Runtimes Guide.
		 * @param skeleton The skeleton the timeline is applied to. This provides access to the bones, slots, and other skeleton
		 *           components the timelines may change.
		 * @param lastTime The last time in seconds this timeline was applied. Some timelines trigger only at discrete times, in
		 *           which case all keys are triggered between <code>lastTime</code> (exclusive) and <code>time</code> (inclusive).
		 *           Pass -1 the first time a timeline is applied to ensure frame 0 is triggered.
		 * @param time The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and
		 *           interpolate between the frame values.
		 * @param events If any events are fired, they are added to this list. Pass null to ignore fired events or if no timelines
		 *           fire events.
		 * @param alpha 0 applies setup or current values (depending on <code>fromSetup</code>), 1 uses timeline values, and
		 *           intermediate values interpolate between them. Adjusting <code>alpha</code> over time can mix a timeline in or
		 *           out.
		 * @param fromSetup If true, <code>alpha</code> transitions between setup and timeline values, setup values are used before
		 *           the first frame (current values are not used). If false, <code>alpha</code> transitions between current and
		 *           timeline values, no change is made before the first frame.
		 * @param add If true, for timelines that support it, their values are added to the setup or current values (depending on
		 *           <code>fromSetup</code>).
		 * @param out True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant
		 *           transitions.
		 * @param appliedPose True to modify {@link Posed#appliedPose}, else {@link Posed#pose} is modified. */
		abstract public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha,
			boolean fromSetup, boolean add, boolean out, boolean appliedPose);

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

	/** An interface for timelines that change a slot's properties. */
	static public interface SlotTimeline {
		/** The index of the slot in {@link Skeleton#slots} that will be changed when this timeline is applied. */
		public int getSlotIndex ();
	}

	/** The base class for timelines that interpolate between frame values using stepped, linear, or a Bezier curve. */
	static abstract public class CurveTimeline extends Timeline {
		static public final int LINEAR = 0, STEPPED = 1, BEZIER = 2;
		/** The number of values stored for each 10 segment Bezier curve. */
		static public final int BEZIER_SIZE = 18;

		float[] curves;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
		 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
		public CurveTimeline (int frameCount, int bezierCount, long... propertyIds) {
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
		 * @param frameIndex The index into {@link #frames} for the values of the frame before <code>time</code>.
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

	/** The base class for a {@link CurveTimeline} that sets one property with a curve. */
	static abstract public class CurveTimeline1 extends CurveTimeline {
		static public final int ENTRIES = 2;
		static final int VALUE = 1;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
		 * @param propertyId Unique identifier for the property the timeline modifies. */
		public CurveTimeline1 (int frameCount, int bezierCount, long propertyId) {
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

		/** Returns the interpolated value for properties relative to the setup value. The timeline value is added to the setup
		 * value, rather than replacing it.
		 * <p>
		 * See {@link Timeline#apply(Skeleton, float, float, Array, float, boolean, boolean, boolean, boolean)}.
		 * @param current The current value for the property.
		 * @param setup The setup value for the property. */
		public float getRelativeValue (float time, float alpha, boolean fromSetup, boolean add, float current, float setup) {
			if (time < frames[0]) return fromSetup ? setup : current;
			float value = getCurveValue(time);
			return fromSetup ? setup + value * alpha : current + (add ? value : value + setup - current) * alpha;
		}

		/** Returns the interpolated value for properties set as absolute values. The timeline value replaces the setup value,
		 * rather than being relative to it.
		 * <p>
		 * See {@link Timeline#apply(Skeleton, float, float, Array, float, boolean, boolean, boolean, boolean)}.
		 * @param current The current value for the property.
		 * @param setup The setup value for the property. */
		public float getAbsoluteValue (float time, float alpha, boolean fromSetup, boolean add, float current, float setup) {
			if (time < frames[0]) return fromSetup ? setup : current;
			float value = getCurveValue(time);
			return fromSetup ? setup + (value - setup) * alpha : current + (add ? value : value - current) * alpha;
		}

		/** Returns the interpolated value for properties set as absolute values, using the specified timeline value rather than
		 * calling {@link #getCurveValue(float)}.
		 * <p>
		 * See {@link Timeline#apply(Skeleton, float, float, Array, float, boolean, boolean, boolean, boolean)}.
		 * @param current The current value for the property.
		 * @param setup The setup value for the property.
		 * @param value The timeline value to apply. */
		public float getAbsoluteValue (float time, float alpha, boolean fromSetup, boolean add, float current, float setup,
			float value) {
			if (time < frames[0]) return fromSetup ? setup : current;
			return fromSetup ? setup + (value - setup) * alpha : current + (add ? value : value - current) * alpha;
		}

		/** Returns the interpolated value for scale properties. The timeline and setup values are multiplied and sign adjusted.
		 * <p>
		 * See {@link Timeline#apply(Skeleton, float, float, Array, float, boolean, boolean, boolean, boolean)}.
		 * @param current The current value for the property.
		 * @param setup The setup value for the property. */
		public float getScaleValue (float time, float alpha, boolean fromSetup, boolean add, boolean out, float current,
			float setup) {
			if (time < frames[0]) return fromSetup ? setup : current;
			float value = getCurveValue(time) * setup;
			if (alpha == 1 && !add) return value;
			float base = fromSetup ? setup : current;
			if (add) return base + (value - setup) * alpha;
			if (out) return base + (Math.abs(value) * Math.signum(base) - base) * alpha;
			base = Math.abs(base) * Math.signum(value);
			return base + (value - base) * alpha;
		}
	}

	/** An interface for timelines that change a bone's properties. */
	static public interface BoneTimeline {
		/** The index of the bone in {@link Skeleton#bones} that is changed by this timeline. */
		public int getBoneIndex ();
	}

	/** The base class for timelines that change 1 bone property with a curve. */
	static abstract public class BoneTimeline1 extends CurveTimeline1 implements BoneTimeline {
		final int boneIndex;

		public BoneTimeline1 (int frameCount, int bezierCount, int boneIndex, long property) {
			super(frameCount, bezierCount, property << 53 | boneIndex);
			this.boneIndex = boneIndex;
			additive = true;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			Bone bone = skeleton.bones.items[boneIndex];
			if (bone.active)
				apply(appliedPose ? bone.appliedPose : bone.pose, bone.data.setupPose, time, alpha, fromSetup, add, out);
		}

		abstract protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add,
			boolean out);
	}

	/** The base class for timelines that change two bone properties with a curve. */
	static abstract public class BoneTimeline2 extends CurveTimeline implements BoneTimeline {
		static public final int ENTRIES = 3;
		static final int VALUE1 = 1, VALUE2 = 2;

		final int boneIndex;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}. */
		public BoneTimeline2 (int frameCount, int bezierCount, int boneIndex, long property1, long property2) {
			super(frameCount, bezierCount, property1 << 53 | boneIndex, property2 << 53 | +boneIndex);
			this.boneIndex = boneIndex;
			additive = true;
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

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			Bone bone = skeleton.bones.items[boneIndex];
			if (bone.active)
				apply(appliedPose ? bone.appliedPose : bone.pose, bone.data.setupPose, time, alpha, fromSetup, add, out);
		}

		abstract protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add,
			boolean out);
	}

	/** Changes {@link BonePose#rotation}. */
	static public class RotateTimeline extends BoneTimeline1 {
		public RotateTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.rotate.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			pose.rotation = getRelativeValue(time, alpha, fromSetup, add, pose.rotation, setup.rotation);
		}
	}

	/** Changes {@link BonePose#x} and {@link BonePose#y}. */
	static public class TranslateTimeline extends BoneTimeline2 {
		public TranslateTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.x.ordinal(), Property.y.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					pose.x = setup.x;
					pose.y = setup.y;
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
	}

	/** Changes {@link BonePose#x}. */
	static public class TranslateXTimeline extends BoneTimeline1 {
		public TranslateXTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.x.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			pose.x = getRelativeValue(time, alpha, fromSetup, add, pose.x, setup.x);
		}
	}

	/** Changes {@link BonePose#y}. */
	static public class TranslateYTimeline extends BoneTimeline1 {
		public TranslateYTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.y.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			pose.y = getRelativeValue(time, alpha, fromSetup, add, pose.y, setup.y);
		}
	}

	/** Changes {@link BonePose#scaleX} and {@link BonePose#scaleY}. */
	static public class ScaleTimeline extends BoneTimeline2 {
		public ScaleTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.scaleX.ordinal(), Property.scaleY.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					pose.scaleX = setup.scaleX;
					pose.scaleY = setup.scaleY;
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
				} else if (out) {
					pose.scaleX = bx + (Math.abs(x) * Math.signum(bx) - bx) * alpha;
					pose.scaleY = by + (Math.abs(y) * Math.signum(by) - by) * alpha;
				} else {
					bx = Math.abs(bx) * Math.signum(x);
					by = Math.abs(by) * Math.signum(y);
					pose.scaleX = bx + (x - bx) * alpha;
					pose.scaleY = by + (y - by) * alpha;
				}
			}
		}
	}

	/** Changes {@link BonePose#scaleX}. */
	static public class ScaleXTimeline extends BoneTimeline1 {
		public ScaleXTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.scaleX.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			pose.scaleX = getScaleValue(time, alpha, fromSetup, add, out, pose.scaleX, setup.scaleX);
		}
	}

	/** Changes {@link BonePose#scaleY}. */
	static public class ScaleYTimeline extends BoneTimeline1 {
		public ScaleYTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.scaleY.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			pose.scaleY = getScaleValue(time, alpha, fromSetup, add, out, pose.scaleY, setup.scaleY);
		}
	}

	/** Changes {@link BonePose#shearX} and {@link BonePose#shearY}. */
	static public class ShearTimeline extends BoneTimeline2 {
		public ShearTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.shearX.ordinal(), Property.shearY.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					pose.shearX = setup.shearX;
					pose.shearY = setup.shearY;
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

	/** Changes {@link BonePose#shearX}. */
	static public class ShearXTimeline extends BoneTimeline1 {
		public ShearXTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.shearX.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			pose.shearX = getRelativeValue(time, alpha, fromSetup, add, pose.shearX, setup.shearX);
		}
	}

	/** Changes {@link BonePose#shearY}. */
	static public class ShearYTimeline extends BoneTimeline1 {
		public ShearYTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, boneIndex, Property.shearY.ordinal());
		}

		protected void apply (BonePose pose, BonePose setup, float time, float alpha, boolean fromSetup, boolean add, boolean out) {
			pose.shearY = getRelativeValue(time, alpha, fromSetup, add, pose.shearY, setup.shearY);
		}
	}

	/** Changes {@link BonePose#inherit}. */
	static public class InheritTimeline extends Timeline implements BoneTimeline {
		static public final int ENTRIES = 2;
		static private final int INHERIT = 1;

		final int boneIndex;

		public InheritTimeline (int frameCount, int boneIndex) {
			super(frameCount, Property.inherit.ordinal() << 53 | boneIndex);
			this.boneIndex = boneIndex;
			instant = true;
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

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			Bone bone = skeleton.bones.items[boneIndex];
			if (!bone.active) return;
			BonePose pose = appliedPose ? bone.appliedPose : bone.pose;

			if (out) {
				if (fromSetup) pose.inherit = bone.data.setupPose.inherit;
			} else {
				float[] frames = this.frames;
				if (time < frames[0]) {
					if (fromSetup) pose.inherit = bone.data.setupPose.inherit;
				} else
					pose.inherit = Inherit.values[(int)frames[search(frames, time, ENTRIES) + INHERIT]];
			}
		}
	}

	/** The base class for timelines that change any number of slot properties with a curve. */
	static abstract public class SlotCurveTimeline extends CurveTimeline implements SlotTimeline {
		final int slotIndex;

		public SlotCurveTimeline (int frameCount, int bezierCount, int slotIndex, long... propertyIds) {
			super(frameCount, bezierCount, propertyIds);
			this.slotIndex = slotIndex;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			Slot slot = skeleton.slots.items[slotIndex];
			if (slot.bone.active) apply(slot, appliedPose ? slot.appliedPose : slot.pose, time, alpha, fromSetup, add);
		}

		abstract protected void apply (Slot slot, SlotPose pose, float time, float alpha, boolean fromSetup, boolean add);
	}

	/** Changes {@link SlotPose#color}. */
	static public class RGBATimeline extends SlotCurveTimeline {
		static public final int ENTRIES = 5;
		static private final int R = 1, G = 2, B = 3, A = 4;

		public RGBATimeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, slotIndex, //
				Property.rgb.ordinal() << 53 | slotIndex, //
				Property.alpha.ordinal() << 53 | slotIndex);
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

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, boolean fromSetup, boolean add) {
			Color color = pose.color;
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) color.set(slot.data.setupPose.color);
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
				if (fromSetup) {
					Color setup = slot.data.setupPose.color;
					color.set(setup.r + (r - setup.r) * alpha, setup.g + (g - setup.g) * alpha, setup.b + (b - setup.b) * alpha,
						setup.a + (a - setup.a) * alpha);
				} else
					color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
		}
	}

	/** Changes RGB for a slot's {@link SlotPose#color}. */
	static public class RGBTimeline extends SlotCurveTimeline {
		static public final int ENTRIES = 4;
		static private final int R = 1, G = 2, B = 3;

		public RGBTimeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, slotIndex, Property.rgb.ordinal() << 53 | slotIndex);
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

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, boolean fromSetup, boolean add) {
			Color color = pose.color;
			float r, g, b;
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					Color setup = slot.data.setupPose.color;
					color.r = setup.r;
					color.g = setup.g;
					color.b = setup.b;
				}
				return;
			}

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
				if (fromSetup) {
					Color setup = slot.data.setupPose.color;
					r = setup.r + (r - setup.r) * alpha;
					g = setup.g + (g - setup.g) * alpha;
					b = setup.b + (b - setup.b) * alpha;
				} else {
					r = color.r + (r - color.r) * alpha;
					g = color.g + (g - color.g) * alpha;
					b = color.b + (b - color.b) * alpha;
				}
			}
			color.r = r < 0 ? 0 : (r > 1 ? 1 : r);
			color.g = g < 0 ? 0 : (g > 1 ? 1 : g);
			color.b = b < 0 ? 0 : (b > 1 ? 1 : b);
		}
	}

	/** Changes alpha for a slot's {@link SlotPose#color}. */
	static public class AlphaTimeline extends CurveTimeline1 implements SlotTimeline {
		final int slotIndex;

		public AlphaTimeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, Property.alpha.ordinal() << 53 | slotIndex);
			this.slotIndex = slotIndex;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			Slot slot = skeleton.slots.items[slotIndex];
			if (!slot.bone.active) return;

			Color color = (appliedPose ? slot.appliedPose : slot.pose).color;
			float a;
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) color.a = slot.data.setupPose.color.a;
				return;
			}

			a = getCurveValue(time);
			if (alpha != 1) {
				if (fromSetup) {
					Color setup = slot.data.setupPose.color;
					a = setup.a + (a - setup.a) * alpha;
				} else
					a = color.a + (a - color.a) * alpha;
			}
			color.a = a < 0 ? 0 : (a > 1 ? 1 : a);
		}
	}

	/** Changes {@link SlotPose#color} and {@link SlotPose#darkColor} for two color tinting. */
	static public class RGBA2Timeline extends SlotCurveTimeline {
		static public final int ENTRIES = 8;
		static private final int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;

		public RGBA2Timeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, slotIndex, //
				Property.rgb.ordinal() << 53 | slotIndex, //
				Property.alpha.ordinal() << 53 | slotIndex, //
				Property.rgb2.ordinal() << 53 | slotIndex);
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

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, boolean fromSetup, boolean add) {
			Color light = pose.color, dark = pose.darkColor;
			float r2, g2, b2;
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					SlotPose setup = slot.data.setupPose;
					light.set(setup.color);
					Color setupDark = setup.darkColor;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
				}
				return;
			}

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
			else if (fromSetup) {
				SlotPose setupPose = slot.data.setupPose;
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
			dark.r = r2 < 0 ? 0 : (r2 > 1 ? 1 : r2);
			dark.g = g2 < 0 ? 0 : (g2 > 1 ? 1 : g2);
			dark.b = b2 < 0 ? 0 : (b2 > 1 ? 1 : b2);
		}
	}

	/** Changes RGB for a slot's {@link SlotPose#color} and {@link SlotPose#darkColor} for two color tinting. */
	static public class RGB2Timeline extends SlotCurveTimeline {
		static public final int ENTRIES = 7;
		static private final int R = 1, G = 2, B = 3, R2 = 4, G2 = 5, B2 = 6;

		public RGB2Timeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, slotIndex, //
				Property.rgb.ordinal() << 53 | slotIndex, //
				Property.rgb2.ordinal() << 53 | slotIndex);
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

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, boolean fromSetup, boolean add) {
			Color light = pose.color, dark = pose.darkColor;
			float r, g, b, r2, g2, b2;
			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) {
					SlotPose setup = slot.data.setupPose;
					Color setupLight = setup.color, setupDark = setup.darkColor;
					light.r = setupLight.r;
					light.g = setupLight.g;
					light.b = setupLight.b;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
				}
				return;
			}

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
				if (fromSetup) {
					SlotPose setupPose = slot.data.setupPose;
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
			light.r = r < 0 ? 0 : (r > 1 ? 1 : r);
			light.g = g < 0 ? 0 : (g > 1 ? 1 : g);
			light.b = b < 0 ? 0 : (b > 1 ? 1 : b);
			dark.r = r2 < 0 ? 0 : (r2 > 1 ? 1 : r2);
			dark.g = g2 < 0 ? 0 : (g2 > 1 ? 1 : g2);
			dark.b = b2 < 0 ? 0 : (b2 > 1 ? 1 : b2);
		}
	}

	/** Changes {@link SlotPose#attachment}. */
	static public class AttachmentTimeline extends Timeline implements SlotTimeline {
		final int slotIndex;
		final String[] attachmentNames;

		public AttachmentTimeline (int frameCount, int slotIndex) {
			super(frameCount, Property.attachment.ordinal() << 53 | slotIndex);
			this.slotIndex = slotIndex;
			attachmentNames = new String[frameCount];
			instant = true;
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

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			Slot slot = skeleton.slots.items[slotIndex];
			if (!slot.bone.active) return;
			SlotPose pose = appliedPose ? slot.appliedPose : slot.pose;

			if (out || time < this.frames[0]) {
				if (fromSetup) setAttachment(skeleton, pose, slot.data.attachmentName);
			} else
				setAttachment(skeleton, pose, attachmentNames[search(this.frames, time)]);
		}

		private void setAttachment (Skeleton skeleton, SlotPose pose, @Null String attachmentName) {
			pose.setAttachment(attachmentName == null ? null : skeleton.getAttachment(slotIndex, attachmentName));
		}
	}

	/** Changes {@link SlotPose#deform} to deform a {@link VertexAttachment}. */
	static public class DeformTimeline extends SlotCurveTimeline {
		final VertexAttachment attachment;
		private final float[][] vertices;

		public DeformTimeline (int frameCount, int bezierCount, int slotIndex, VertexAttachment attachment) {
			super(frameCount, bezierCount, slotIndex, Property.deform.ordinal() << 53 | slotIndex << 32 | +attachment.getId());
			this.attachment = attachment;
			vertices = new float[frameCount][];
			additive = true;
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

		protected void apply (Slot slot, SlotPose pose, float time, float alpha, boolean fromSetup, boolean add) {
			if (!(pose.attachment instanceof VertexAttachment vertexAttachment)
				|| vertexAttachment.getTimelineAttachment() != attachment) return;

			FloatArray deformArray = pose.deform;
			if (deformArray.size == 0) fromSetup = true;

			float[][] vertices = this.vertices;
			int vertexCount = vertices[0].length;

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) deformArray.clear();
				return;
			}

			float[] deform = deformArray.setSize(vertexCount);

			if (time >= frames[frames.length - 1]) { // Time is after last frame.
				float[] lastVertices = vertices[frames.length - 1];
				if (alpha == 1) {
					if (add && !fromSetup) {
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
				} else if (fromSetup) {
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
				} else if (add) {
					if (vertexAttachment.getBones() == null) { // Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.getVertices();
						for (int i = 0; i < vertexCount; i++)
							deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
					} else { // Weighted deform offsets, with alpha.
						for (int i = 0; i < vertexCount; i++)
							deform[i] += lastVertices[i] * alpha;
					}
				} else { // Vertex positions or deform offsets, with alpha.
					for (int i = 0; i < vertexCount; i++)
						deform[i] += (lastVertices[i] - deform[i]) * alpha;
				}
				return;
			}

			int frame = search(frames, time);
			float percent = getCurvePercent(time, frame);
			float[] prevVertices = vertices[frame];
			float[] nextVertices = vertices[frame + 1];

			if (alpha == 1) {
				if (add && !fromSetup) {
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
			} else if (fromSetup) {
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
			} else if (add) {
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
			} else { // Vertex positions or deform offsets, with alpha.
				for (int i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
				}
			}
		}
	}

	/** Changes {@link SlotPose#sequenceIndex} for an attachment's {@link Sequence}. */
	static public class SequenceTimeline extends Timeline implements SlotTimeline {
		static public final int ENTRIES = 3;
		static private final int MODE = 1, DELAY = 2;

		final int slotIndex;
		final HasSequence attachment;

		public SequenceTimeline (int frameCount, int slotIndex, Attachment attachment) {
			super(frameCount, Property.sequence.ordinal() << 53 | slotIndex << 32 | ((HasSequence)attachment).getSequence().getId());
			this.slotIndex = slotIndex;
			this.attachment = (HasSequence)attachment;
			instant = true;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		/** The attachment for which {@link SlotPose#sequenceIndex} will be set.
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

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			Slot slot = skeleton.slots.items[slotIndex];
			if (!slot.bone.active) return;
			SlotPose pose = appliedPose ? slot.appliedPose : slot.pose;

			Attachment slotAttachment = pose.attachment;
			if (!(slotAttachment instanceof HasSequence hasSequence) || slotAttachment.getTimelineAttachment() != attachment) return;

			if (out) {
				if (fromSetup) pose.setSequenceIndex(-1);
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) {
				if (fromSetup) pose.setSequenceIndex(-1);
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
		static private final long[] propertyIds = {Property.event.ordinal()};

		private final Event[] events;

		public EventTimeline (int frameCount) {
			super(frameCount, propertyIds);
			events = new Event[frameCount];
			instant = true;
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
		public void apply (@Null Skeleton skeleton, float lastTime, float time, @Null Array<Event> firedEvents, float alpha,
			boolean fromSetup, boolean add, boolean out, boolean appliedPose) {
			if (firedEvents == null) return;

			float[] frames = this.frames;
			int frameCount = frames.length;

			if (lastTime > time) { // Apply after lastTime for looped animations.
				apply(null, lastTime, Integer.MAX_VALUE, firedEvents, 0, false, false, false, false);
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

	/** Changes the {@link Skeleton#getDrawOrder()}. */
	static public class DrawOrderTimeline extends Timeline {
		static final long propertyID = Property.drawOrder.ordinal();
		static private final long[] propertyIds = {propertyID};

		private final int[][] drawOrders;

		public DrawOrderTimeline (int frameCount) {
			super(frameCount, propertyIds);
			drawOrders = new int[frameCount][];
			instant = true;
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

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			Slot[] pose = (appliedPose ? skeleton.drawOrder.appliedPose : skeleton.drawOrder.pose).items;
			Slot[] setup = skeleton.slots.items;
			if (out || time < frames[0]) {
				if (fromSetup) arraycopy(setup, 0, pose, 0, skeleton.slots.size);
				return;
			}

			int[] order = drawOrders[search(frames, time)];
			if (order == null)
				arraycopy(setup, 0, pose, 0, skeleton.slots.size);
			else {
				for (int i = 0, n = order.length; i < n; i++)
					pose[i] = setup[order[i]];
			}
		}
	}

	/** Changes a subset of the {@link Skeleton#getDrawOrder() draw order}. */
	static public class DrawOrderFolderTimeline extends Timeline {
		private final int[] slots;
		private final boolean[] inFolder;
		private final int[][] drawOrders;

		/** @param slots {@link Skeleton#slots} indices controlled by this timeline, in setup order.
		 * @param slotCount The maximum number of slots in the skeleton. */
		public DrawOrderFolderTimeline (int frameCount, int[] slots, int slotCount) {
			super(frameCount, propertyIds(slots));
			this.slots = slots;
			drawOrders = new int[frameCount][];
			inFolder = new boolean[slotCount];
			for (int i : slots)
				inFolder[i] = true;
			instant = true;
		}

		static private long[] propertyIds (int[] slots) {
			int n = slots.length;
			var ids = new long[n];
			for (int i = 0; i < n; i++)
				ids[i] = slots[i];
			return ids;
		}

		public int getFrameCount () {
			return frames.length;
		}

		/** The {@link Skeleton#slots} indices that this timeline affects, in setup order. */
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
		 * @param drawOrder Ordered {@link #slots} indices, or null to use setup pose order. */
		public void setFrame (int frame, float time, @Null int[] drawOrder) {
			frames[frame] = time;
			drawOrders[frame] = drawOrder;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			Slot[] pose = (appliedPose ? skeleton.drawOrder.appliedPose : skeleton.drawOrder.pose).items;
			Slot[] setup = skeleton.slots.items;
			if (out || time < frames[0]) {
				if (fromSetup) setup(pose, setup);
			} else {
				int[] order = drawOrders[search(frames, time)];
				if (order == null)
					setup(pose, setup);
				else {
					boolean[] inFolder = this.inFolder;
					int[] slots = this.slots;
					for (int i = 0, found = 0, done = slots.length;; i++) {
						if (inFolder[pose[i].data.index]) {
							pose[i] = setup[slots[order[found]]];
							if (++found == done) break;
						}
					}
				}
			}
		}

		private void setup (Slot[] pose, Slot[] setup) {
			boolean[] inFolder = this.inFolder;
			int[] slots = this.slots;
			for (int i = 0, found = 0, done = slots.length;; i++) {
				if (inFolder[pose[i].data.index]) {
					pose[i] = setup[slots[found]];
					if (++found == done) break;
				}
			}
		}
	}

	static public interface ConstraintTimeline {
		/** The index of the constraint in {@link Skeleton#constraints} that will be changed when this timeline is applied, or -1 if
		 * a specific constraint will not be changed. */
		public int getConstraintIndex ();
	}

	/** Changes {@link IkConstraintPose#mix}, {@link IkConstraintPose#softness}, {@link IkConstraintPose#bendDirection},
	 * {@link IkConstraintPose#stretch}, and {@link IkConstraintPose#compress}. */
	static public class IkConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
		static public final int ENTRIES = 6;
		static private final int MIX = 1, SOFTNESS = 2, BEND_DIRECTION = 3, COMPRESS = 4, STRETCH = 5;

		final int constraintIndex;

		public IkConstraintTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, Property.ikConstraint.ordinal() << 53 | constraintIndex);
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

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			var constraint = (IkConstraint)skeleton.constraints.items[constraintIndex];
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

			IkConstraintPose base = fromSetup ? constraint.data.setupPose : pose;
			pose.mix = base.mix + (mix - base.mix) * alpha;
			pose.softness = base.softness + (softness - base.softness) * alpha;
			if (out) {
				if (fromSetup) {
					pose.bendDirection = base.bendDirection;
					pose.compress = base.compress;
					pose.stretch = base.stretch;
				}
			} else {
				pose.bendDirection = (int)frames[i + BEND_DIRECTION];
				pose.compress = frames[i + COMPRESS] != 0;
				pose.stretch = frames[i + STRETCH] != 0;
			}
		}
	}

	/** Changes {@link TransformConstraintPose#mixRotate}, {@link TransformConstraintPose#mixX},
	 * {@link TransformConstraintPose#mixY}, {@link TransformConstraintPose#mixScaleX}, {@link TransformConstraintPose#mixScaleY},
	 * and {@link TransformConstraintPose#mixShearY}. */
	static public class TransformConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
		static public final int ENTRIES = 7;
		static private final int ROTATE = 1, X = 2, Y = 3, SCALEX = 4, SCALEY = 5, SHEARY = 6;

		final int constraintIndex;

		public TransformConstraintTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, Property.transformConstraint.ordinal() << 53 | constraintIndex);
			this.constraintIndex = constraintIndex;
			additive = true;
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

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			var constraint = (TransformConstraint)skeleton.constraints.items[constraintIndex];
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

			TransformConstraintPose base = fromSetup ? constraint.data.setupPose : pose;
			if (add) {
				pose.mixRotate = base.mixRotate + rotate * alpha;
				pose.mixX = base.mixX + x * alpha;
				pose.mixY = base.mixY + y * alpha;
				pose.mixScaleX = base.mixScaleX + scaleX * alpha;
				pose.mixScaleY = base.mixScaleY + scaleY * alpha;
				pose.mixShearY = base.mixShearY + shearY * alpha;
			} else {
				pose.mixRotate = base.mixRotate + (rotate - base.mixRotate) * alpha;
				pose.mixX = base.mixX + (x - base.mixX) * alpha;
				pose.mixY = base.mixY + (y - base.mixY) * alpha;
				pose.mixScaleX = base.mixScaleX + (scaleX - base.mixScaleX) * alpha;
				pose.mixScaleY = base.mixScaleY + (scaleY - base.mixScaleY) * alpha;
				pose.mixShearY = base.mixShearY + (shearY - base.mixShearY) * alpha;
			}
		}
	}

	/** The base class for timelines that change 1 constraint property with a curve. */
	static abstract public class ConstraintTimeline1 extends CurveTimeline1 implements ConstraintTimeline {
		final int constraintIndex;

		public ConstraintTimeline1 (int frameCount, int bezierCount, int constraintIndex, long property) {
			super(frameCount, bezierCount, property << 53 | constraintIndex);
			this.constraintIndex = constraintIndex;
		}

		public int getConstraintIndex () {
			return constraintIndex;
		}
	}

	/** Changes {@link PathConstraintPose#position}. */
	static public class PathConstraintPositionTimeline extends ConstraintTimeline1 {
		public PathConstraintPositionTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.pathConstraintPosition.ordinal());
			additive = true;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			var constraint = (PathConstraint)skeleton.constraints.items[constraintIndex];
			if (constraint.active) {
				PathConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
				pose.position = getAbsoluteValue(time, alpha, fromSetup, add, pose.position, constraint.data.setupPose.position);
			}
		}
	}

	/** Changes {@link PathConstraintPose#spacing}. */
	static public class PathConstraintSpacingTimeline extends ConstraintTimeline1 {
		public PathConstraintSpacingTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.pathConstraintSpacing.ordinal());
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			var constraint = (PathConstraint)skeleton.constraints.items[constraintIndex];
			if (constraint.active) {
				PathConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
				pose.spacing = getAbsoluteValue(time, alpha, fromSetup, false, pose.spacing, constraint.data.setupPose.spacing);
			}
		}
	}

	/** Changes {@link PathConstraintPose#mixRotate}, {@link PathConstraintPose#mixX}, and {@link PathConstraintPose#mixY}. */
	static public class PathConstraintMixTimeline extends CurveTimeline implements ConstraintTimeline {
		static public final int ENTRIES = 4;
		static private final int ROTATE = 1, X = 2, Y = 3;

		final int constraintIndex;

		public PathConstraintMixTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, Property.pathConstraintMix.ordinal() << 53 | constraintIndex);
			this.constraintIndex = constraintIndex;
			additive = true;
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

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			var constraint = (PathConstraint)skeleton.constraints.items[constraintIndex];
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

			PathConstraintPose base = fromSetup ? constraint.data.setupPose : pose;
			if (add) {
				pose.mixRotate = base.mixRotate + rotate * alpha;
				pose.mixX = base.mixX + x * alpha;
				pose.mixY = base.mixY + y * alpha;
			} else {
				pose.mixRotate = base.mixRotate + (rotate - base.mixRotate) * alpha;
				pose.mixX = base.mixX + (x - base.mixX) * alpha;
				pose.mixY = base.mixY + (y - base.mixY) * alpha;
			}
		}
	}

	/** The base class for most {@link PhysicsConstraint} timelines. */
	static abstract public class PhysicsConstraintTimeline extends ConstraintTimeline1 {
		/** @param constraintIndex -1 for all physics constraints in the skeleton. */
		public PhysicsConstraintTimeline (int frameCount, int bezierCount, int constraintIndex, long property) {
			super(frameCount, bezierCount, constraintIndex, property);
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			if (add && !additive) add = false;
			if (constraintIndex == -1) {
				float value = time >= frames[0] ? getCurveValue(time) : 0;
				PhysicsConstraint[] constraints = skeleton.physics.items;
				for (int i = 0, n = skeleton.physics.size; i < n; i++) {
					PhysicsConstraint constraint = constraints[i];
					if (constraint.active && global(constraint.data)) {
						PhysicsConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
						set(pose, getAbsoluteValue(time, alpha, fromSetup, add, get(pose), get(constraint.data.setupPose), value));
					}
				}
			} else {
				var constraint = (PhysicsConstraint)skeleton.constraints.items[constraintIndex];
				if (constraint.active) {
					PhysicsConstraintPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
					set(pose, getAbsoluteValue(time, alpha, fromSetup, add, get(pose), get(constraint.data.setupPose)));
				}
			}
		}

		abstract protected float get (PhysicsConstraintPose pose);

		abstract protected void set (PhysicsConstraintPose pose, float value);

		abstract protected boolean global (PhysicsConstraintData constraint);
	}

	/** Changes {@link PhysicsConstraintPose#inertia}. */
	static public class PhysicsConstraintInertiaTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintInertiaTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintInertia.ordinal());
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

	/** Changes {@link PhysicsConstraintPose#strength}. */
	static public class PhysicsConstraintStrengthTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintStrengthTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintStrength.ordinal());
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

	/** Changes {@link PhysicsConstraintPose#damping}. */
	static public class PhysicsConstraintDampingTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintDampingTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintDamping.ordinal());
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

	/** Changes {@link PhysicsConstraintPose#massInverse}. The timeline values are not inverted. */
	static public class PhysicsConstraintMassTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintMassTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintMass.ordinal());
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

	/** Changes {@link PhysicsConstraintPose#wind}. */
	static public class PhysicsConstraintWindTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintWindTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintWind.ordinal());
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

	/** Changes {@link PhysicsConstraintPose#gravity}. */
	static public class PhysicsConstraintGravityTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintGravityTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintGravity.ordinal());
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

	/** Changes {@link PhysicsConstraintPose#mix}. */
	static public class PhysicsConstraintMixTimeline extends PhysicsConstraintTimeline {
		public PhysicsConstraintMixTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.physicsConstraintMix.ordinal());
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
		static private final long[] propertyIds = {Property.physicsConstraintReset.ordinal()};

		final int constraintIndex;

		/** @param constraintIndex -1 for all physics constraints in the skeleton. */
		public PhysicsConstraintResetTimeline (int frameCount, int constraintIndex) {
			super(frameCount, propertyIds);
			this.constraintIndex = constraintIndex;
			instant = true;
		}

		/** The index of the physics constraint in {@link Skeleton#constraints} that will be reset when this timeline is applied, or
		 * -1 if all physics constraints in the skeleton will be reset. */
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
		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			PhysicsConstraint constraint = null;
			if (constraintIndex != -1) {
				constraint = (PhysicsConstraint)skeleton.constraints.items[constraintIndex];
				if (!constraint.active) return;
			}

			float[] frames = this.frames;

			if (lastTime > time) { // Apply after lastTime for looped animations.
				apply(skeleton, lastTime, Integer.MAX_VALUE, null, alpha, false, false, false, false);
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

	/** Changes {@link SliderPose#time}. */
	static public class SliderTimeline extends ConstraintTimeline1 {
		public SliderTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.sliderTime.ordinal());
			additive = true;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			var constraint = (Slider)skeleton.constraints.items[constraintIndex];
			if (constraint.active) {
				SliderPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
				pose.time = getAbsoluteValue(time, alpha, fromSetup, add, pose.time, constraint.data.setupPose.time);
			}
		}
	}

	/** Changes {@link SliderPose#mix}. */
	static public class SliderMixTimeline extends ConstraintTimeline1 {
		public SliderMixTimeline (int frameCount, int bezierCount, int constraintIndex) {
			super(frameCount, bezierCount, constraintIndex, Property.sliderMix.ordinal());
			additive = true;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, @Null Array<Event> events, float alpha, boolean fromSetup,
			boolean add, boolean out, boolean appliedPose) {
			var constraint = (Slider)skeleton.constraints.items[constraintIndex];
			if (constraint.active) {
				SliderPose pose = appliedPose ? constraint.appliedPose : constraint.pose;
				pose.mix = getAbsoluteValue(time, alpha, fromSetup, add, pose.mix, constraint.data.setupPose.mix);
			}
		}
	}
}
