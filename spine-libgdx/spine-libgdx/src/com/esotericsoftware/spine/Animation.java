/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.Animation.MixBlend.*;
import static com.esotericsoftware.spine.Animation.MixDirection.*;
import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.ObjectSet;

import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.VertexAttachment;

/** Stores a list of timelines to animate a skeleton's pose over time. */
public class Animation {
	final String name;
	Array<Timeline> timelines;
	final ObjectSet<String> timelineIds = new ObjectSet();
	float duration;

	public Animation (String name, Array<Timeline> timelines, float duration) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
		this.duration = duration;
		setTimelines(timelines);
	}

	/** If the returned array or the timelines it contains are modified, {@link #setTimelines(Array)} must be called. */
	public Array<Timeline> getTimelines () {
		return timelines;
	}

	public void setTimelines (Array<Timeline> timelines) {
		if (timelines == null) throw new IllegalArgumentException("timelines cannot be null.");
		this.timelines = timelines;

		timelineIds.clear();
		for (Timeline timeline : timelines)
			timelineIds.addAll(timeline.getPropertyIds());
	}

	/** Returns true if this animation contains a timeline with any of the specified property IDs. **/
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

	/** Applies the animation's timelines to the specified skeleton.
	 * <p>
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection)}.
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
	public void apply (Skeleton skeleton, float lastTime, float time, boolean loop, Array<Event> events, float alpha,
		MixBlend blend, MixDirection direction) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		if (loop && duration != 0) {
			time %= duration;
			if (lastTime > 0) lastTime %= duration;
		}

		Object[] timelines = this.timelines.items;
		for (int i = 0, n = this.timelines.size; i < n; i++)
			((Timeline)timelines[i]).apply(skeleton, lastTime, time, events, alpha, blend, direction);
	}

	/** The animation's name, which is unique across all animations in the skeleton. */
	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}

	/** Binary search using a stride of 1.
	 * @param time Must be >= the first value in <code>frames</code>.
	 * @return The index of the first value <= <code>time</code>. */
	static int search (float[] frames, float time) {
		int n = frames.length;
		for (int i = 1; i < n; i++)
			if (frames[i] > time) return i - 1;
		return n - 1;
	}

	/** Binary search using the specified stride.
	 * @param time Must be >= the first value in <code>frames</code>.
	 * @return The index of the first value <= <code>time</code>. */
	static int search (float[] frames, float time, int step) {
		int n = frames.length;
		for (int i = step; i < n; i += step)
			if (frames[i] > time) return i - step;
		return n - step;
	}

	/** Controls how timeline values are mixed with setup pose values or current pose values when a timeline is applied with
	 * <code>alpha</code> < 1.
	 * <p>
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection)}. */
	static public enum MixBlend {
		/** Transitions from the setup value to the timeline value (the current value is not used). Before the first frame, the
		 * setup value is set. */
		setup,
		/** Transitions from the current value to the timeline value. Before the first frame, transitions from the current value to
		 * the setup value. Timelines which perform instant transitions, such as {@link DrawOrderTimeline} or
		 * {@link AttachmentTimeline}, use the setup value before the first frame.
		 * <p>
		 * <code>first</code> is intended for the first animations applied, not for animations layered on top of those. */
		first,
		/** Transitions from the current value to the timeline value. No change is made before the first frame (the current value is
		 * kept until the first frame).
		 * <p>
		 * <code>replace</code> is intended for animations layered on top of others, not for the first animations applied. */
		replace,
		/** Transitions from the current value to the current value plus the timeline value. No change is made before the first
		 * frame (the current value is kept until the first frame).
		 * <p>
		 * <code>add</code> is intended for animations layered on top of others, not for the first animations applied. Properties
		 * set by additive animations must be set manually or by another animation before applying the additive animations, else the
		 * property values will increase each time the additive animations are applied. */
		add
	}

	/** Indicates whether a timeline's <code>alpha</code> is mixing out over time toward 0 (the setup or current pose value) or
	 * mixing in toward 1 (the timeline's value). Some timelines use this to decide how values are applied.
	 * <p>
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixBlend, MixDirection)}. */
	static public enum MixDirection {
		in, out
	}

	static enum Property {
		rotate, translateX, translateY, scaleX, scaleY, shearX, shearY, //
		rgba, rgb2, //
		attachment, deform, //
		event, drawOrder, //
		ikConstraint, transformConstraint, //
		pathConstraintPosition, pathConstraintSpacing, pathConstraintMix
	}

	/** The base class for all timelines. */
	static public abstract class Timeline {
		private final String[] propertyIds;
		final float[] frames;

		/** @param propertyIds Unique identifiers for the properties the timeline modifies. */
		public Timeline (int frameCount, String... propertyIds) {
			if (propertyIds == null) throw new IllegalArgumentException("propertyIds cannot be null.");
			this.propertyIds = propertyIds;
			frames = new float[frameCount * getFrameEntries()];
		}

		/** Uniquely encodes both the type of this timeline and the skeleton property that it affects. */
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
		 *           such as {@link DrawOrderTimeline} or {@link AttachmentTimeline}. */
		abstract public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction);
	}

	/** An interface for timelines which change the property of a bone. */
	static public interface BoneTimeline {
		/** The index of the bone in {@link Skeleton#getBones()} that will be changed when this timeline is applied. */
		public int getBoneIndex ();
	}

	/** An interface for timelines which change the property of a slot. */
	static public interface SlotTimeline {
		/** The index of the slot in {@link Skeleton#getSlots()} that will be changed when this timeline is applied. */
		public int getSlotIndex ();
	}

	/** The base class for timelines that interpolate between frame values using stepped, linear, or a Bezier curve. */
	static public abstract class CurveTimeline extends Timeline {
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
				float[] newCurves = new float[size];
				arraycopy(curves, 0, newCurves, 0, size);
				curves = newCurves;
			}
		}

		/** Stores the segments for the specified Bezier curve. For timelines that modify multiple values, there may be more than
		 * one curve per frame.
		 * @param bezier The ordinal of this Bezier curve for this timeline, between 0 and <code>bezierCount - 1</code> (specified
		 *           in the constructor), inclusive.
		 * @param frame Between 0 and <code>frameCount - 1</code>, inclusive.
		 * @param value The index of the value for this frame that this curve is used for.
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
	static public abstract class CurveTimeline1 extends CurveTimeline {
		static public final int ENTRIES = 2;
		static final int VALUE = 1;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
		 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
		public CurveTimeline1 (int frameCount, int bezierCount, String... propertyIds) {
			super(frameCount, bezierCount, propertyIds);
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
			switch (curveType) {
			case LINEAR:
				float before = frames[i], value = frames[i + VALUE];
				return value + (time - before) / (frames[i + ENTRIES] - before) * (frames[i + ENTRIES + VALUE] - value);
			case STEPPED:
				return frames[i + VALUE];
			}
			return getBezierValue(time, i, VALUE, curveType - BEZIER);
		}
	}

	/** The base class for a {@link CurveTimeline} which sets two properties. */
	static public abstract class CurveTimeline2 extends CurveTimeline {
		static public final int ENTRIES = 3;
		static final int VALUE1 = 1, VALUE2 = 2;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
		 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
		public CurveTimeline2 (int frameCount, int bezierCount, String... propertyIds) {
			super(frameCount, bezierCount, propertyIds);
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
	}

	/** Changes a bone's local {@link Bone#getRotation()}. */
	static public class RotateTimeline extends CurveTimeline1 implements BoneTimeline {
		final int boneIndex;

		public RotateTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, Property.rotate.ordinal() + "|" + boneIndex);
			this.boneIndex = boneIndex;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			Bone bone = skeleton.bones.get(boneIndex);
			if (!bone.active) return;

			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					bone.rotation = bone.data.rotation;
					return;
				case first:
					bone.rotation += (bone.data.rotation - bone.rotation) * alpha;
				}
				return;
			}

			float r = getCurveValue(time);
			switch (blend) {
			case setup:
				bone.rotation = bone.data.rotation + r * alpha;
				break;
			case first:
			case replace:
				r += bone.data.rotation - bone.rotation;
				// Fall through.
			case add:
				bone.rotation += r * alpha;
			}
		}
	}

	/** Changes a bone's local {@link Bone#getX()} and {@link Bone#getY()}. */
	static public class TranslateTimeline extends CurveTimeline2 implements BoneTimeline {
		final int boneIndex;

		public TranslateTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, //
				Property.translateX.ordinal() + "|" + boneIndex, //
				Property.translateY.ordinal() + "|" + boneIndex);
			this.boneIndex = boneIndex;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			Bone bone = skeleton.bones.get(boneIndex);
			if (!bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					bone.x = bone.data.x;
					bone.y = bone.data.y;
					return;
				case first:
					bone.x += (bone.data.x - bone.x) * alpha;
					bone.y += (bone.data.y - bone.y) * alpha;
				}
				return;
			}

			float x, y;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
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
				x = getBezierValue(time, i, VALUE1, curveType - BEZIER);
				y = getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
			}

			switch (blend) {
			case setup:
				bone.x = bone.data.x + x * alpha;
				bone.y = bone.data.y + y * alpha;
				break;
			case first:
			case replace:
				bone.x += (bone.data.x + x - bone.x) * alpha;
				bone.y += (bone.data.y + y - bone.y) * alpha;
				break;
			case add:
				bone.x += x * alpha;
				bone.y += y * alpha;
			}
		}
	}

	/** Changes a bone's local {@link Bone#getScaleX()} and {@link Bone#getScaleY()}. */
	static public class ScaleTimeline extends CurveTimeline2 implements BoneTimeline {
		final int boneIndex;

		public ScaleTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, //
				Property.scaleX.ordinal() + "|" + boneIndex, //
				Property.scaleY.ordinal() + "|" + boneIndex);
			this.boneIndex = boneIndex;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			Bone bone = skeleton.bones.get(boneIndex);
			if (!bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					bone.scaleX = bone.data.scaleX;
					bone.scaleY = bone.data.scaleY;
					return;
				case first:
					bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
				}
				return;
			}

			float x, y;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
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
				x = getBezierValue(time, i, VALUE1, curveType - BEZIER);
				y = getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
			}

			if (alpha == 1) {
				if (blend == add) {
					bone.scaleX += x - bone.data.scaleX;
					bone.scaleY += y - bone.data.scaleY;
				} else {
					bone.scaleX = x;
					bone.scaleY = y;
				}
			} else {
				// Mixing out uses sign of setup or current pose, else use sign of key.
				float bx, by;
				if (direction == out) {
					switch (blend) {
					case setup:
						bx = bone.data.scaleX;
						by = bone.data.scaleY;
						bone.scaleX = bx + (Math.abs(x) * Math.signum(bx) - bx) * alpha;
						bone.scaleY = by + (Math.abs(y) * Math.signum(by) - by) * alpha;
						break;
					case first:
					case replace:
						bx = bone.scaleX;
						by = bone.scaleY;
						bone.scaleX = bx + (Math.abs(x) * Math.signum(bx) - bx) * alpha;
						bone.scaleY = by + (Math.abs(y) * Math.signum(by) - by) * alpha;
						break;
					case add:
						bx = bone.scaleX;
						by = bone.scaleY;
						bone.scaleX = bx + (Math.abs(x) * Math.signum(bx) - bone.data.scaleX) * alpha;
						bone.scaleY = by + (Math.abs(y) * Math.signum(by) - bone.data.scaleY) * alpha;
					}
				} else {
					switch (blend) {
					case setup:
						bx = Math.abs(bone.data.scaleX) * Math.signum(x);
						by = Math.abs(bone.data.scaleY) * Math.signum(y);
						bone.scaleX = bx + (x - bx) * alpha;
						bone.scaleY = by + (y - by) * alpha;
						break;
					case first:
					case replace:
						bx = Math.abs(bone.scaleX) * Math.signum(x);
						by = Math.abs(bone.scaleY) * Math.signum(y);
						bone.scaleX = bx + (x - bx) * alpha;
						bone.scaleY = by + (y - by) * alpha;
						break;
					case add:
						bx = Math.signum(x);
						by = Math.signum(y);
						bone.scaleX = Math.abs(bone.scaleX) * bx + (x - Math.abs(bone.data.scaleX) * bx) * alpha;
						bone.scaleY = Math.abs(bone.scaleY) * by + (y - Math.abs(bone.data.scaleY) * by) * alpha;
					}
				}
			}
		}
	}

	/** Changes a bone's local {@link Bone#getShearX()} and {@link Bone#getShearY()}. */
	static public class ShearTimeline extends CurveTimeline2 implements BoneTimeline {
		final int boneIndex;

		public ShearTimeline (int frameCount, int bezierCount, int boneIndex) {
			super(frameCount, bezierCount, //
				Property.shearX.ordinal() + "|" + boneIndex, //
				Property.shearY.ordinal() + "|" + boneIndex);
			this.boneIndex = boneIndex;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			Bone bone = skeleton.bones.get(boneIndex);
			if (!bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					bone.shearX = bone.data.shearX;
					bone.shearY = bone.data.shearY;
					return;
				case first:
					bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
					bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
				}
				return;
			}

			float x, y;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
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
				x = getBezierValue(time, i, VALUE1, curveType - BEZIER);
				y = getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
			}

			switch (blend) {
			case setup:
				bone.shearX = bone.data.shearX + x * alpha;
				bone.shearY = bone.data.shearY + y * alpha;
				break;
			case first:
			case replace:
				bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
				bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
				break;
			case add:
				bone.shearX += x * alpha;
				bone.shearY += y * alpha;
			}
		}
	}

	/** Changes a slot's {@link Slot#getColor()}. */
	static public class ColorTimeline extends CurveTimeline implements SlotTimeline {
		static public final int ENTRIES = 5;
		static private final int R = 1, G = 2, B = 3, A = 4;

		final int slotIndex;

		public ColorTimeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, //
				Property.rgba.ordinal() + "|" + slotIndex);
			this.slotIndex = slotIndex;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		public int getSlotIndex () {
			return slotIndex;
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

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			Slot slot = skeleton.slots.get(slotIndex);
			if (!slot.bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					slot.color.set(slot.data.color);
					return;
				case first:
					Color color = slot.color, setup = slot.data.color;
					color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha);
				}
				return;
			}

			float r, g, b, a;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
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
				r = getBezierValue(time, i, R, curveType - BEZIER);
				g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				a = getBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER);
			}

			if (alpha == 1)
				slot.color.set(r, g, b, a);
			else {
				Color color = slot.color;
				if (blend == setup) color.set(slot.data.color);
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
		}
	}

	/** Changes a slot's {@link Slot#getColor()} and {@link Slot#getDarkColor()} for two color tinting. */
	static public class TwoColorTimeline extends CurveTimeline implements SlotTimeline {
		static public final int ENTRIES = 8;
		static private final int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;

		final int slotIndex;

		public TwoColorTimeline (int frameCount, int bezierCount, int slotIndex) {
			super(frameCount, bezierCount, //
				Property.rgba.ordinal() + "|" + slotIndex, //
				Property.rgb2.ordinal() + "|" + slotIndex);
			this.slotIndex = slotIndex;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** The index of the slot in {@link Skeleton#getSlots()} that will be changed when this timeline is applied. The
		 * {@link Slot#getDarkColor()} must not be null. */
		public int getSlotIndex () {
			return slotIndex;
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

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			Slot slot = skeleton.slots.get(slotIndex);
			if (!slot.bone.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					slot.color.set(slot.data.color);
					slot.darkColor.set(slot.data.darkColor);
					return;
				case first:
					Color light = slot.color, dark = slot.darkColor, setupLight = slot.data.color, setupDark = slot.data.darkColor;
					light.add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
						(setupLight.a - light.a) * alpha);
					dark.add((setupDark.r - dark.r) * alpha, (setupDark.g - dark.g) * alpha, (setupDark.b - dark.b) * alpha, 0);
				}
				return;
			}

			float r, g, b, a, r2, g2, b2;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i >> 3];
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
				r = getBezierValue(time, i, R, curveType - BEZIER);
				g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				a = getBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER);
				r2 = getBezierValue(time, i, R2, curveType + BEZIER_SIZE * 4 - BEZIER);
				g2 = getBezierValue(time, i, G2, curveType + BEZIER_SIZE * 5 - BEZIER);
				b2 = getBezierValue(time, i, B2, curveType + BEZIER_SIZE * 6 - BEZIER);
			}

			if (alpha == 1) {
				slot.color.set(r, g, b, a);
				slot.darkColor.set(r2, g2, b2, 1);
			} else {
				Color light = slot.color, dark = slot.darkColor;
				if (blend == setup) {
					light.set(slot.data.color);
					dark.set(slot.data.darkColor);
				}
				light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
				dark.add((r2 - dark.r) * alpha, (g2 - dark.g) * alpha, (b2 - dark.b) * alpha, 0);
			}
		}
	}

	/** Changes a slot's {@link Slot#getAttachment()}. */
	static public class AttachmentTimeline extends Timeline implements SlotTimeline {
		final int slotIndex;
		final String[] attachmentNames;

		public AttachmentTimeline (int frameCount, int slotIndex) {
			super(frameCount, Property.attachment.ordinal() + "|" + slotIndex);
			this.slotIndex = slotIndex;
			attachmentNames = new String[frameCount];
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
		public void setFrame (int frame, float time, String attachmentName) {
			frames[frame] = time;
			attachmentNames[frame] = attachmentName;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			Slot slot = skeleton.slots.get(slotIndex);
			if (!slot.bone.active) return;

			if (direction == out && blend == setup) {
				String attachmentName = slot.data.attachmentName;
				slot.setAttachment(attachmentName == null ? null : skeleton.getAttachment(slotIndex, attachmentName));
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (blend == setup || blend == first) {
					String attachmentName = slot.data.attachmentName;
					slot.setAttachment(attachmentName == null ? null : skeleton.getAttachment(slotIndex, attachmentName));
				}
				return;
			}

			String attachmentName = attachmentNames[search(frames, time)];
			slot.setAttachment(attachmentName == null ? null : skeleton.getAttachment(slotIndex, attachmentName));
		}
	}

	/** Changes a slot's {@link Slot#getDeform()} to deform a {@link VertexAttachment}. */
	static public class DeformTimeline extends CurveTimeline implements SlotTimeline {
		final int slotIndex;
		final VertexAttachment attachment;
		private final float[][] vertices;

		public DeformTimeline (int frameCount, int bezierCount, int slotIndex, VertexAttachment attachment) {
			super(frameCount, bezierCount, Property.deform.ordinal() + "|" + slotIndex + "|" + attachment.getId());
			this.slotIndex = slotIndex;
			this.attachment = attachment;
			vertices = new float[frameCount][];
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		/** The attachment that will be deformed.
		 * <p>
		 * See {@link VertexAttachment#getDeformAttachment()}. */
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

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			Slot slot = skeleton.slots.get(slotIndex);
			if (!slot.bone.active) return;
			Attachment slotAttachment = slot.attachment;
			if (!(slotAttachment instanceof VertexAttachment)
				|| ((VertexAttachment)slotAttachment).getDeformAttachment() != attachment) return;

			FloatArray deformArray = slot.getDeform();
			if (deformArray.size == 0) blend = setup;

			float[][] vertices = this.vertices;
			int vertexCount = vertices[0].length;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
				switch (blend) {
				case setup:
					deformArray.clear();
					return;
				case first:
					if (alpha == 1) {
						deformArray.clear();
						return;
					}
					float[] deform = deformArray.setSize(vertexCount);
					if (vertexAttachment.getBones() == null) {
						// Unweighted vertex positions.
						float[] setupVertices = vertexAttachment.getVertices();
						for (int i = 0; i < vertexCount; i++)
							deform[i] += (setupVertices[i] - deform[i]) * alpha;
					} else {
						// Weighted deform offsets.
						alpha = 1 - alpha;
						for (int i = 0; i < vertexCount; i++)
							deform[i] *= alpha;
					}
				}
				return;
			}

			float[] deform = deformArray.setSize(vertexCount);

			if (time >= frames[frames.length - 1]) { // Time is after last frame.
				float[] lastVertices = vertices[frames.length - 1];
				if (alpha == 1) {
					if (blend == add) {
						VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
						if (vertexAttachment.getBones() == null) {
							// Unweighted vertex positions, no alpha.
							float[] setupVertices = vertexAttachment.getVertices();
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] - setupVertices[i];
						} else {
							// Weighted deform offsets, no alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i];
						}
					} else {
						// Vertex positions or deform offsets, no alpha.
						arraycopy(lastVertices, 0, deform, 0, vertexCount);
					}
				} else {
					switch (blend) {
					case setup: {
						VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
						if (vertexAttachment.getBones() == null) {
							// Unweighted vertex positions, with alpha.
							float[] setupVertices = vertexAttachment.getVertices();
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
					case first:
					case replace:
						// Vertex positions or deform offsets, with alpha.
						for (int i = 0; i < vertexCount; i++)
							deform[i] += (lastVertices[i] - deform[i]) * alpha;
						break;
					case add:
						VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
						if (vertexAttachment.getBones() == null) {
							// Unweighted vertex positions, no alpha.
							float[] setupVertices = vertexAttachment.getVertices();
							for (int i = 0; i < vertexCount; i++)
								deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
						} else {
							// Weighted deform offsets, alpha.
							for (int i = 0; i < vertexCount; i++)
								deform[i] += lastVertices[i] * alpha;
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
					VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
					if (vertexAttachment.getBones() == null) {
						// Unweighted vertex positions, no alpha.
						float[] setupVertices = vertexAttachment.getVertices();
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
				case setup: {
					VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
					if (vertexAttachment.getBones() == null) {
						// Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.getVertices();
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
				case first:
				case replace:
					// Vertex positions or deform offsets, with alpha.
					for (int i = 0; i < vertexCount; i++) {
						float prev = prevVertices[i];
						deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
					}
					break;
				case add:
					VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
					if (vertexAttachment.getBones() == null) {
						// Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.getVertices();
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
				}
			}
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
		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> firedEvents, float alpha, MixBlend blend,
			MixDirection direction) {

			if (firedEvents == null) return;

			float[] frames = this.frames;
			int frameCount = frames.length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				apply(skeleton, lastTime, Integer.MAX_VALUE, firedEvents, alpha, blend, direction);
				lastTime = -1f;
			} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return; // Time is before first frame.

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

		/** The draw order for each frame. See {@link #setFrame(int, float, int[])}. */
		public int[][] getDrawOrders () {
			return drawOrders;
		}

		/** Sets the time and draw order for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds.
		 * @param drawOrder For each slot in {@link Skeleton#slots}, the index of the slot in the new draw order. May be null to use
		 *           setup pose draw order. */
		public void setFrame (int frame, float time, int[] drawOrder) {
			frames[frame] = time;
			drawOrders[frame] = drawOrder;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			Array<Slot> drawOrder = skeleton.drawOrder;
			Array<Slot> slots = skeleton.slots;
			if (direction == out && blend == setup) {
				arraycopy(slots.items, 0, drawOrder.items, 0, slots.size);
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (blend == setup || blend == first) arraycopy(slots.items, 0, drawOrder.items, 0, slots.size);
				return;
			}

			int[] drawOrderToSetupIndex = drawOrders[search(frames, time)];
			if (drawOrderToSetupIndex == null)
				arraycopy(slots.items, 0, drawOrder.items, 0, slots.size);
			else {
				for (int i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
					drawOrder.set(i, slots.get(drawOrderToSetupIndex[i]));
			}
		}
	}

	/** Changes an IK constraint's {@link IkConstraint#getMix()}, {@link IkConstraint#getSoftness()},
	 * {@link IkConstraint#getBendDirection()}, {@link IkConstraint#getStretch()}, and {@link IkConstraint#getCompress()}. */
	static public class IkConstraintTimeline extends CurveTimeline {
		static public final int ENTRIES = 6;
		static private final int MIX = 1, SOFTNESS = 2, BEND_DIRECTION = 3, COMPRESS = 4, STRETCH = 5;

		final int ikConstraintIndex;

		public IkConstraintTimeline (int frameCount, int bezierCount, int ikConstraintIndex) {
			super(frameCount, bezierCount, Property.ikConstraint.ordinal() + "|" + ikConstraintIndex);
			this.ikConstraintIndex = ikConstraintIndex;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** The index of the IK constraint slot in {@link Skeleton#getIkConstraints()} that will be changed when this timeline is
		 * applied. */
		public int getIkConstraintIndex () {
			return ikConstraintIndex;
		}

		/** Sets the time, mix, softness, bend direction, compress, and stretch for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
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

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			IkConstraint constraint = skeleton.ikConstraints.get(ikConstraintIndex);
			if (!constraint.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					constraint.mix = constraint.data.mix;
					constraint.softness = constraint.data.softness;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
					return;
				case first:
					constraint.mix += (constraint.data.mix - constraint.mix) * alpha;
					constraint.softness += (constraint.data.softness - constraint.softness) * alpha;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
				}
				return;
			}

			float mix, softness;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
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
				mix = getBezierValue(time, i, MIX, curveType - BEZIER);
				softness = getBezierValue(time, i, SOFTNESS, curveType + BEZIER_SIZE - BEZIER);
			}

			if (blend == setup) {
				constraint.mix = constraint.data.mix + (mix - constraint.data.mix) * alpha;
				constraint.softness = constraint.data.softness + (softness - constraint.data.softness) * alpha;
				if (direction == out) {
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
				if (direction == in) {
					constraint.bendDirection = (int)frames[i + BEND_DIRECTION];
					constraint.compress = frames[i + COMPRESS] != 0;
					constraint.stretch = frames[i + STRETCH] != 0;
				}
			}
		}
	}

	/** Changes a transform constraint's {@link TransformConstraint#getRotateMix()}, {@link TransformConstraint#getTranslateMix()},
	 * {@link TransformConstraint#getScaleMix()}, and {@link TransformConstraint#getShearMix()}. */
	static public class TransformConstraintTimeline extends CurveTimeline {
		static public final int ENTRIES = 5;
		static private final int ROTATE = 1, TRANSLATE = 2, SCALE = 3, SHEAR = 4;

		final int transformConstraintIndex;

		public TransformConstraintTimeline (int frameCount, int bezierCount, int transformConstraintIndex) {
			super(frameCount, bezierCount, Property.transformConstraint.ordinal() + "|" + transformConstraintIndex);
			this.transformConstraintIndex = transformConstraintIndex;
		}

		public int getFrameEntries () {
			return ENTRIES;
		}

		/** The index of the transform constraint slot in {@link Skeleton#getTransformConstraints()} that will be changed when this
		 * timeline is applied. */
		public int getTransformConstraintIndex () {
			return transformConstraintIndex;
		}

		/** Sets the time, rotate mix, translate mix, scale mix, and shear mix for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public void setFrame (int frame, float time, float rotateMix, float translateMix, float scaleMix, float shearMix) {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + ROTATE] = rotateMix;
			frames[frame + TRANSLATE] = translateMix;
			frames[frame + SCALE] = scaleMix;
			frames[frame + SHEAR] = shearMix;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			TransformConstraint constraint = skeleton.transformConstraints.get(transformConstraintIndex);
			if (!constraint.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				TransformConstraintData data = constraint.data;
				switch (blend) {
				case setup:
					constraint.rotateMix = data.rotateMix;
					constraint.translateMix = data.translateMix;
					constraint.scaleMix = data.scaleMix;
					constraint.shearMix = data.shearMix;
					return;
				case first:
					constraint.rotateMix += (data.rotateMix - constraint.rotateMix) * alpha;
					constraint.translateMix += (data.translateMix - constraint.translateMix) * alpha;
					constraint.scaleMix += (data.scaleMix - constraint.scaleMix) * alpha;
					constraint.shearMix += (data.shearMix - constraint.shearMix) * alpha;
				}
				return;
			}

			float rotate, translate, scale, shear;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				float before = frames[i];
				rotate = frames[i + ROTATE];
				translate = frames[i + TRANSLATE];
				scale = frames[i + SCALE];
				shear = frames[i + SHEAR];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				rotate += (frames[i + ENTRIES + ROTATE] - rotate) * t;
				translate += (frames[i + ENTRIES + TRANSLATE] - translate) * t;
				scale += (frames[i + ENTRIES + SCALE] - scale) * t;
				shear += (frames[i + ENTRIES + SHEAR] - shear) * t;
				break;
			case STEPPED:
				rotate = frames[i + ROTATE];
				translate = frames[i + TRANSLATE];
				scale = frames[i + SCALE];
				shear = frames[i + SHEAR];
				break;
			default:
				rotate = getBezierValue(time, i, ROTATE, curveType - BEZIER);
				translate = getBezierValue(time, i, TRANSLATE, curveType + BEZIER_SIZE - BEZIER);
				scale = getBezierValue(time, i, TRANSLATE, curveType + BEZIER_SIZE * 2 - BEZIER);
				shear = getBezierValue(time, i, TRANSLATE, curveType + BEZIER_SIZE * 3 - BEZIER);
			}

			if (blend == setup) {
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

	/** Changes a path constraint's {@link PathConstraint#getPosition()}. */
	static public class PathConstraintPositionTimeline extends CurveTimeline1 {
		final int pathConstraintIndex;

		public PathConstraintPositionTimeline (int frameCount, int bezierCount, int pathConstraintIndex) {
			super(frameCount, bezierCount, Property.pathConstraintPosition.ordinal() + "|" + pathConstraintIndex);
			this.pathConstraintIndex = pathConstraintIndex;
		}

		/** The index of the path constraint slot in {@link Skeleton#getPathConstraints()} that will be changed when this timeline
		 * is applied. */
		public int getPathConstraintIndex () {
			return pathConstraintIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			PathConstraint constraint = skeleton.pathConstraints.get(pathConstraintIndex);
			if (!constraint.active) return;

			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					constraint.position = constraint.data.position;
					return;
				case first:
					constraint.position += (constraint.data.position - constraint.position) * alpha;
				}
				return;
			}

			float position = getCurveValue(time);
			if (blend == setup)
				constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
			else
				constraint.position += (position - constraint.position) * alpha;
		}
	}

	/** Changes a path constraint's {@link PathConstraint#getSpacing()}. */
	static public class PathConstraintSpacingTimeline extends CurveTimeline1 {
		final int pathConstraintIndex;

		public PathConstraintSpacingTimeline (int frameCount, int bezierCount, int pathConstraintIndex) {
			super(frameCount, bezierCount, Property.pathConstraintSpacing.ordinal() + "|" + pathConstraintIndex);
			this.pathConstraintIndex = pathConstraintIndex;
		}

		/** The index of the path constraint slot in {@link Skeleton#getPathConstraints()} that will be changed when this timeline
		 * is applied. */
		public int getPathConstraintIndex () {
			return pathConstraintIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			PathConstraint constraint = skeleton.pathConstraints.get(pathConstraintIndex);
			if (!constraint.active) return;

			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					constraint.spacing = constraint.data.spacing;
					return;
				case first:
					constraint.spacing += (constraint.data.spacing - constraint.spacing) * alpha;
				}
				return;
			}

			float spacing = getCurveValue(time);
			if (blend == setup)
				constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
			else
				constraint.spacing += (spacing - constraint.spacing) * alpha;
		}
	}

	/** Changes a transform constraint's {@link PathConstraint#getRotateMix()} and
	 * {@link TransformConstraint#getTranslateMix()}. */
	static public class PathConstraintMixTimeline extends CurveTimeline2 {
		final int pathConstraintIndex;

		public PathConstraintMixTimeline (int frameCount, int bezierCount, int pathConstraintIndex) {
			super(frameCount, bezierCount, Property.pathConstraintMix.ordinal() + "|" + pathConstraintIndex);
			this.pathConstraintIndex = pathConstraintIndex;
		}

		/** The index of the path constraint slot in {@link Skeleton#getPathConstraints()} that will be changed when this timeline
		 * is applied. */
		public int getPathConstraintIndex () {
			return pathConstraintIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixBlend blend,
			MixDirection direction) {

			PathConstraint constraint = skeleton.pathConstraints.get(pathConstraintIndex);
			if (!constraint.active) return;

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case setup:
					constraint.rotateMix = constraint.data.rotateMix;
					constraint.translateMix = constraint.data.translateMix;
					return;
				case first:
					constraint.rotateMix += (constraint.data.rotateMix - constraint.rotateMix) * alpha;
					constraint.translateMix += (constraint.data.translateMix - constraint.translateMix) * alpha;
				}
				return;
			}

			float rotate, translate;
			int i = search(frames, time, ENTRIES), curveType = (int)curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				float before = frames[i];
				rotate = frames[i + VALUE1];
				translate = frames[i + VALUE2];
				float t = (time - before) / (frames[i + ENTRIES] - before);
				rotate += (frames[i + ENTRIES + VALUE1] - rotate) * t;
				translate += (frames[i + ENTRIES + VALUE2] - translate) * t;
				break;
			case STEPPED:
				rotate = frames[i + VALUE1];
				translate = frames[i + VALUE2];
				break;
			default:
				rotate = getBezierValue(time, i, VALUE1, curveType - BEZIER);
				translate = getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER);
			}

			if (blend == setup) {
				constraint.rotateMix = constraint.data.rotateMix + (rotate - constraint.data.rotateMix) * alpha;
				constraint.translateMix = constraint.data.translateMix + (translate - constraint.data.translateMix) * alpha;
			} else {
				constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
				constraint.translateMix += (translate - constraint.translateMix) * alpha;
			}
		}
	}
}
