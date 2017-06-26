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

package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.Animation.MixDirection.*;
import static com.esotericsoftware.spine.Animation.MixPose.*;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.VertexAttachment;

/** A simple container for a list of timelines and a name. */
public class Animation {
	final String name;
	final Array<Timeline> timelines;
	float duration;

	public Animation (String name, Array<Timeline> timelines, float duration) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		if (timelines == null) throw new IllegalArgumentException("timelines cannot be null.");
		this.name = name;
		this.timelines = timelines;
		this.duration = duration;
	}

	public Array<Timeline> getTimelines () {
		return timelines;
	}

	/** The duration of the animation in seconds, which is the highest time of all keys in the timeline. */
	public float getDuration () {
		return duration;
	}

	public void setDuration (float duration) {
		this.duration = duration;
	}

	/** Applies all the animation's timelines to the specified skeleton.
	 * <p>
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixPose, MixDirection)}. */
	public void apply (Skeleton skeleton, float lastTime, float time, boolean loop, Array<Event> events, float alpha, MixPose pose,
		MixDirection direction) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		if (loop && duration != 0) {
			time %= duration;
			if (lastTime > 0) lastTime %= duration;
		}

		Array<Timeline> timelines = this.timelines;
		for (int i = 0, n = timelines.size; i < n; i++)
			timelines.get(i).apply(skeleton, lastTime, time, events, alpha, pose, direction);
	}

	/** The animation's name, which is unique within the skeleton. */
	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}

	/** @param target After the first and before the last value.
	 * @return index of first value greater than the target. */
	static int binarySearch (float[] values, float target, int step) {
		int low = 0;
		int high = values.length / step - 2;
		if (high == 0) return step;
		int current = high >>> 1;
		while (true) {
			if (values[(current + 1) * step] <= target)
				low = current + 1;
			else
				high = current;
			if (low == high) return (low + 1) * step;
			current = (low + high) >>> 1;
		}
	}

	/** @param target After the first and before the last value.
	 * @return index of first value greater than the target. */
	static int binarySearch (float[] values, float target) {
		int low = 0;
		int high = values.length - 2;
		if (high == 0) return 1;
		int current = high >>> 1;
		while (true) {
			if (values[current + 1] <= target)
				low = current + 1;
			else
				high = current;
			if (low == high) return low + 1;
			current = (low + high) >>> 1;
		}
	}

	static int linearSearch (float[] values, float target, int step) {
		for (int i = 0, last = values.length - step; i <= last; i += step)
			if (values[i] > target) return i;
		return -1;
	}

	/** The interface for all timelines. */
	static public interface Timeline {
		/** Applies this timeline to the skeleton.
		 * @param skeleton The skeleton the timeline is being applied to. This provides access to the bones, slots, and other
		 *           skeleton components the timeline may change.
		 * @param lastTime The time this timeline was last applied. Timelines such as {@link EventTimeline} trigger only at specific
		 *           times rather than every frame. In that case, the timeline triggers everything between <code>lastTime</code>
		 *           (exclusive) and <code>time</code> (inclusive).
		 * @param time The time within the animation. Most timelines find the key before and the key after this time so they can
		 *           interpolate between the keys.
		 * @param events If any events are fired, they are added to this list. Can be null to ignore firing events or if the
		 *           timeline does not fire events.
		 * @param alpha 0 applies the current or setup pose value (depending on <code>setupPose</code>). 1 applies the timeline
		 *           value. Between 0 and 1 applies a value between the current or setup pose and the timeline value. By adjusting
		 *           <code>alpha</code> over time, an animation can be mixed in or out. <code>alpha</code> can also be useful to
		 *           apply animations on top of each other (layered).
		 * @param pose Controls how mixing is applied when <code>alpha</code> < 1.
		 * @param direction Indicates whether the timeline is mixing in or out. Used by timelines which perform instant transitions,
		 *           such as {@link DrawOrderTimeline} or {@link AttachmentTimeline}. */
		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction);

		/** Uniquely encodes both the type of this timeline and the skeleton property that it affects. */
		public int getPropertyId ();
	}

	/** Controls how a timeline is mixed with the setup or current pose.
	 * <p>
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixPose, MixDirection)}. */
	static public enum MixPose {
		/** The timeline value is mixed with the setup pose (the current pose is not used). */
		setup,
		/** The timeline value is mixed with the current pose. The setup pose is used as the timeline value before the first key,
		 * except for timelines which perform instant transitions, such as {@link DrawOrderTimeline} or
		 * {@link AttachmentTimeline}. */
		current,
		/** The timeline value is mixed with the current pose. No change is made before the first key (the current pose is kept
		 * until the first key). */
		currentLayered
	}

	/** Indicates whether a timeline's <code>alpha</code> is mixing out over time toward 0 (the setup or current pose) or mixing in
	 * toward 1 (the timeline's pose).
	 * <p>
	 * See Timeline {@link Timeline#apply(Skeleton, float, float, Array, float, MixPose, MixDirection)}. */
	static public enum MixDirection {
		in, out
	}

	static private enum TimelineType {
		rotate, translate, scale, shear, //
		attachment, color, deform, //
		event, drawOrder, //
		ikConstraint, transformConstraint, //
		pathConstraintPosition, pathConstraintSpacing, pathConstraintMix, //
		twoColor
	}

	/** The base class for timelines that use interpolation between key frame values. */
	abstract static public class CurveTimeline implements Timeline {
		static public final float LINEAR = 0, STEPPED = 1, BEZIER = 2;
		static private final int BEZIER_SIZE = 10 * 2 - 1;

		private final float[] curves; // type, x, y, ...

		public CurveTimeline (int frameCount) {
			if (frameCount <= 0) throw new IllegalArgumentException("frameCount must be > 0: " + frameCount);
			curves = new float[(frameCount - 1) * BEZIER_SIZE];
		}

		/** The number of key frames for this timeline. */
		public int getFrameCount () {
			return curves.length / BEZIER_SIZE + 1;
		}

		/** Sets the specified key frame to linear interpolation. */
		public void setLinear (int frameIndex) {
			curves[frameIndex * BEZIER_SIZE] = LINEAR;
		}

		/** Sets the specified key frame to stepped interpolation. */
		public void setStepped (int frameIndex) {
			curves[frameIndex * BEZIER_SIZE] = STEPPED;
		}

		/** Returns the interpolation type for the specified key frame.
		 * @return Linear is 0, stepped is 1, Bezier is 2. */
		public float getCurveType (int frameIndex) {
			int index = frameIndex * BEZIER_SIZE;
			if (index == curves.length) return LINEAR;
			float type = curves[index];
			if (type == LINEAR) return LINEAR;
			if (type == STEPPED) return STEPPED;
			return BEZIER;
		}

		/** Sets the specified key frame to Bezier interpolation. <code>cx1</code> and <code>cx2</code> are from 0 to 1,
		 * representing the percent of time between the two key frames. <code>cy1</code> and <code>cy2</code> are the percent of the
		 * difference between the key frame's values. */
		public void setCurve (int frameIndex, float cx1, float cy1, float cx2, float cy2) {
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

		/** Returns the interpolated percentage for the specified key frame and linear percentage. */
		public float getCurvePercent (int frameIndex, float percent) {
			percent = MathUtils.clamp(percent, 0, 1);
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

	/** Changes a bone's local {@link Bone#getRotation()}. */
	static public class RotateTimeline extends CurveTimeline {
		static public final int ENTRIES = 2;
		static final int PREV_TIME = -2, PREV_ROTATION = -1;
		static final int ROTATION = 1;

		int boneIndex;
		final float[] frames; // time, degrees, ...

		public RotateTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount << 1];
		}

		public int getPropertyId () {
			return (TimelineType.rotate.ordinal() << 24) + boneIndex;
		}

		public void setBoneIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.boneIndex = index;
		}

		/** The index of the bone in {@link Skeleton#getBones()} that will be changed. */
		public int getBoneIndex () {
			return boneIndex;
		}

		/** The time in seconds and rotation in degrees for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** Sets the time in seconds and the rotation in degrees for the specified key frame. */
		public void setFrame (int frameIndex, float time, float degrees) {
			frameIndex <<= 1;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATION] = degrees;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			Bone bone = skeleton.bones.get(boneIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					bone.rotation = bone.data.rotation;
					return;
				case current:
					float r = bone.data.rotation - bone.rotation;
					r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
					bone.rotation += r * alpha;
				}
				return;
			}

			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				if (pose == setup)
					bone.rotation = bone.data.rotation + frames[frames.length + PREV_ROTATION] * alpha;
				else {
					float r = bone.data.rotation + frames[frames.length + PREV_ROTATION] - bone.rotation;
					r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360; // Wrap within -180 and 180.
					bone.rotation += r * alpha;
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frame = binarySearch(frames, time, ENTRIES);
			float prevRotation = frames[frame + PREV_ROTATION];
			float frameTime = frames[frame];
			float percent = getCurvePercent((frame >> 1) - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			float r = frames[frame + ROTATION] - prevRotation;
			r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
			r = prevRotation + r * percent;
			if (pose == setup) {
				r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
				bone.rotation = bone.data.rotation + r * alpha;
			} else {
				r = bone.data.rotation + r - bone.rotation;
				r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
				bone.rotation += r * alpha;
			}
		}
	}

	/** Changes a bone's local {@link Bone#getX()} and {@link Bone#getY()}. */
	static public class TranslateTimeline extends CurveTimeline {
		static public final int ENTRIES = 3;
		static final int PREV_TIME = -3, PREV_X = -2, PREV_Y = -1;
		static final int X = 1, Y = 2;

		int boneIndex;
		final float[] frames; // time, x, y, ...

		public TranslateTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * ENTRIES];
		}

		public int getPropertyId () {
			return (TimelineType.translate.ordinal() << 24) + boneIndex;
		}

		public void setBoneIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.boneIndex = index;
		}

		/** The index of the bone in {@link Skeleton#getBones()} that will be changed. */
		public int getBoneIndex () {
			return boneIndex;
		}

		/** The time in seconds, x, and y values for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** Sets the time in seconds, x, and y values for the specified key frame. */
		public void setFrame (int frameIndex, float time, float x, float y) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + X] = x;
			frames[frameIndex + Y] = y;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			Bone bone = skeleton.bones.get(boneIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					bone.x = bone.data.x;
					bone.y = bone.data.y;
					return;
				case current:
					bone.x += (bone.data.x - bone.x) * alpha;
					bone.y += (bone.data.y - bone.y) * alpha;
				}
				return;
			}

			float x, y;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				x = frames[frames.length + PREV_X];
				y = frames[frames.length + PREV_Y];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = binarySearch(frames, time, ENTRIES);
				x = frames[frame + PREV_X];
				y = frames[frame + PREV_Y];
				float frameTime = frames[frame];
				float percent = getCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				x += (frames[frame + X] - x) * percent;
				y += (frames[frame + Y] - y) * percent;
			}
			if (pose == setup) {
				bone.x = bone.data.x + x * alpha;
				bone.y = bone.data.y + y * alpha;
			} else {
				bone.x += (bone.data.x + x - bone.x) * alpha;
				bone.y += (bone.data.y + y - bone.y) * alpha;
			}
		}
	}

	/** Changes a bone's local {@link Bone#getScaleX()} and {@link Bone#getScaleY()}. */
	static public class ScaleTimeline extends TranslateTimeline {
		public ScaleTimeline (int frameCount) {
			super(frameCount);
		}

		public int getPropertyId () {
			return (TimelineType.scale.ordinal() << 24) + boneIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			Bone bone = skeleton.bones.get(boneIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					bone.scaleX = bone.data.scaleX;
					bone.scaleY = bone.data.scaleY;
					return;
				case current:
					bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
				}
				return;
			}

			float x, y;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				x = frames[frames.length + PREV_X] * bone.data.scaleX;
				y = frames[frames.length + PREV_Y] * bone.data.scaleY;
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = binarySearch(frames, time, ENTRIES);
				x = frames[frame + PREV_X];
				y = frames[frame + PREV_Y];
				float frameTime = frames[frame];
				float percent = getCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				x = (x + (frames[frame + X] - x) * percent) * bone.data.scaleX;
				y = (y + (frames[frame + Y] - y) * percent) * bone.data.scaleY;
			}
			if (alpha == 1) {
				bone.scaleX = x;
				bone.scaleY = y;
			} else {
				float bx, by;
				if (pose == setup) {
					bx = bone.data.scaleX;
					by = bone.data.scaleY;
				} else {
					bx = bone.scaleX;
					by = bone.scaleY;
				}
				// Mixing out uses sign of setup or current pose, else use sign of key.
				if (direction == out) {
					x = Math.abs(x) * Math.signum(bx);
					y = Math.abs(y) * Math.signum(by);
				} else {
					bx = Math.abs(bx) * Math.signum(x);
					by = Math.abs(by) * Math.signum(y);
				}
				bone.scaleX = bx + (x - bx) * alpha;
				bone.scaleY = by + (y - by) * alpha;
			}
		}
	}

	/** Changes a bone's local {@link Bone#getShearX()} and {@link Bone#getShearY()}. */
	static public class ShearTimeline extends TranslateTimeline {
		public ShearTimeline (int frameCount) {
			super(frameCount);
		}

		public int getPropertyId () {
			return (TimelineType.shear.ordinal() << 24) + boneIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			Bone bone = skeleton.bones.get(boneIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					bone.shearX = bone.data.shearX;
					bone.shearY = bone.data.shearY;
					return;
				case current:
					bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
					bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
				}
				return;
			}

			float x, y;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				x = frames[frames.length + PREV_X];
				y = frames[frames.length + PREV_Y];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = binarySearch(frames, time, ENTRIES);
				x = frames[frame + PREV_X];
				y = frames[frame + PREV_Y];
				float frameTime = frames[frame];
				float percent = getCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				x = x + (frames[frame + X] - x) * percent;
				y = y + (frames[frame + Y] - y) * percent;
			}
			if (pose == setup) {
				bone.shearX = bone.data.shearX + x * alpha;
				bone.shearY = bone.data.shearY + y * alpha;
			} else {
				bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
				bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
			}
		}
	}

	/** Changes a slot's {@link Slot#getColor()}. */
	static public class ColorTimeline extends CurveTimeline {
		static public final int ENTRIES = 5;
		static private final int PREV_TIME = -5, PREV_R = -4, PREV_G = -3, PREV_B = -2, PREV_A = -1;
		static private final int R = 1, G = 2, B = 3, A = 4;

		int slotIndex;
		private final float[] frames; // time, r, g, b, a, ...

		public ColorTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * ENTRIES];
		}

		public int getPropertyId () {
			return (TimelineType.color.ordinal() << 24) + slotIndex;
		}

		public void setSlotIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.slotIndex = index;
		}

		/** The index of the slot in {@link Skeleton#getSlots()} that will be changed. */
		public int getSlotIndex () {
			return slotIndex;
		}

		/** The time in seconds, red, green, blue, and alpha values for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** Sets the time in seconds, red, green, blue, and alpha for the specified key frame. */
		public void setFrame (int frameIndex, float time, float r, float g, float b, float a) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + R] = r;
			frames[frameIndex + G] = g;
			frames[frameIndex + B] = b;
			frames[frameIndex + A] = a;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			Slot slot = skeleton.slots.get(slotIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					slot.color.set(slot.data.color);
					return;
				case current:
					Color color = slot.color, setup = slot.data.color;
					color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha);
				}
				return;
			}

			float r, g, b, a;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				int i = frames.length;
				r = frames[i + PREV_R];
				g = frames[i + PREV_G];
				b = frames[i + PREV_B];
				a = frames[i + PREV_A];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = binarySearch(frames, time, ENTRIES);
				r = frames[frame + PREV_R];
				g = frames[frame + PREV_G];
				b = frames[frame + PREV_B];
				a = frames[frame + PREV_A];
				float frameTime = frames[frame];
				float percent = getCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				r += (frames[frame + R] - r) * percent;
				g += (frames[frame + G] - g) * percent;
				b += (frames[frame + B] - b) * percent;
				a += (frames[frame + A] - a) * percent;
			}
			if (alpha == 1)
				slot.color.set(r, g, b, a);
			else {
				Color color = slot.color;
				if (pose == setup) color.set(slot.data.color);
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
		}
	}

	/** Changes a slot's {@link Slot#getColor()} and {@link Slot#getDarkColor()} for two color tinting. */
	static public class TwoColorTimeline extends CurveTimeline {
		static public final int ENTRIES = 8;
		static private final int PREV_TIME = -8, PREV_R = -7, PREV_G = -6, PREV_B = -5, PREV_A = -4;
		static private final int PREV_R2 = -3, PREV_G2 = -2, PREV_B2 = -1;
		static private final int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;

		int slotIndex;
		private final float[] frames; // time, r, g, b, a, r2, g2, b2, ...

		public TwoColorTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * ENTRIES];
		}

		public int getPropertyId () {
			return (TimelineType.twoColor.ordinal() << 24) + slotIndex;
		}

		public void setSlotIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.slotIndex = index;
		}

		/** The index of the slot in {@link Skeleton#getSlots()} that will be changed. */
		public int getSlotIndex () {
			return slotIndex;
		}

		/** The time in seconds, red, green, blue, and alpha values for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** Sets the time in seconds, light, and dark colors for the specified key frame. */
		public void setFrame (int frameIndex, float time, float r, float g, float b, float a, float r2, float g2, float b2) {
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

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			Slot slot = skeleton.slots.get(slotIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					slot.color.set(slot.data.color);
					slot.darkColor.set(slot.data.darkColor);
					return;
				case current:
					Color light = slot.color, dark = slot.darkColor, setupLight = slot.data.color, setupDark = slot.data.darkColor;
					light.add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
						(setupLight.a - light.a) * alpha);
					dark.add((setupDark.r - dark.r) * alpha, (setupDark.g - dark.g) * alpha, (setupDark.b - dark.b) * alpha, 0);
				}
				return;
			}

			float r, g, b, a, r2, g2, b2;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				int i = frames.length;
				r = frames[i + PREV_R];
				g = frames[i + PREV_G];
				b = frames[i + PREV_B];
				a = frames[i + PREV_A];
				r2 = frames[i + PREV_R2];
				g2 = frames[i + PREV_G2];
				b2 = frames[i + PREV_B2];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = binarySearch(frames, time, ENTRIES);
				r = frames[frame + PREV_R];
				g = frames[frame + PREV_G];
				b = frames[frame + PREV_B];
				a = frames[frame + PREV_A];
				r2 = frames[frame + PREV_R2];
				g2 = frames[frame + PREV_G2];
				b2 = frames[frame + PREV_B2];
				float frameTime = frames[frame];
				float percent = getCurvePercent(frame / ENTRIES - 1,
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
				slot.color.set(r, g, b, a);
				slot.darkColor.set(r2, g2, b2, 1);
			} else {
				Color light = slot.color, dark = slot.darkColor;
				if (pose == setup) {
					light.set(slot.data.color);
					dark.set(slot.data.darkColor);
				}
				light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
				dark.add((r2 - dark.r) * alpha, (g2 - dark.g) * alpha, (b2 - dark.b) * alpha, 0);
			}
		}
	}

	/** Changes a slot's {@link Slot#getAttachment()}. */
	static public class AttachmentTimeline implements Timeline {
		int slotIndex;
		final float[] frames; // time, ...
		final String[] attachmentNames;

		public AttachmentTimeline (int frameCount) {
			frames = new float[frameCount];
			attachmentNames = new String[frameCount];
		}

		public int getPropertyId () {
			return (TimelineType.attachment.ordinal() << 24) + slotIndex;
		}

		/** The number of key frames for this timeline. */
		public int getFrameCount () {
			return frames.length;
		}

		public void setSlotIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.slotIndex = index;
		}

		/** The index of the slot in {@link Skeleton#getSlots()} that will be changed. */
		public int getSlotIndex () {
			return slotIndex;
		}

		/** The time in seconds for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** The attachment name for each key frame. May contain null values to clear the attachment. */
		public String[] getAttachmentNames () {
			return attachmentNames;
		}

		/** Sets the time in seconds and the attachment name for the specified key frame. */
		public void setFrame (int frameIndex, float time, String attachmentName) {
			frames[frameIndex] = time;
			attachmentNames[frameIndex] = attachmentName;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			Slot slot = skeleton.slots.get(slotIndex);
			if (direction == out && pose == setup) {
				String attachmentName = slot.data.attachmentName;
				slot.setAttachment(attachmentName == null ? null : skeleton.getAttachment(slotIndex, attachmentName));
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (pose == setup) {
					String attachmentName = slot.data.attachmentName;
					slot.setAttachment(attachmentName == null ? null : skeleton.getAttachment(slotIndex, attachmentName));
				}
				return;
			}

			int frameIndex;
			if (time >= frames[frames.length - 1]) // Time is after last frame.
				frameIndex = frames.length - 1;
			else
				frameIndex = binarySearch(frames, time) - 1;

			String attachmentName = attachmentNames[frameIndex];
			slot.setAttachment(attachmentName == null ? null : skeleton.getAttachment(slotIndex, attachmentName));
		}
	}

	/** Changes a slot's {@link Slot#getAttachmentVertices()} to deform a {@link VertexAttachment}. */
	static public class DeformTimeline extends CurveTimeline {
		static private float[] zeros = new float[64];

		int slotIndex;
		VertexAttachment attachment;
		private final float[] frames; // time, ...
		private final float[][] frameVertices;

		public DeformTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount];
			frameVertices = new float[frameCount][];
		}

		public int getPropertyId () {
			return (TimelineType.deform.ordinal() << 27) + attachment.getId() + slotIndex;
		}

		public void setSlotIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.slotIndex = index;
		}

		/** The index of the slot in {@link Skeleton#getSlots()} that will be changed. */
		public int getSlotIndex () {
			return slotIndex;
		}

		public void setAttachment (VertexAttachment attachment) {
			this.attachment = attachment;
		}

		/** The attachment that will be deformed. */
		public VertexAttachment getAttachment () {
			return attachment;
		}

		/** The time in seconds for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** The vertices for each key frame. */
		public float[][] getVertices () {
			return frameVertices;
		}

		/** Sets the time in seconds and the vertices for the specified key frame.
		 * @param vertices Vertex positions for an unweighted VertexAttachment, or deform offsets if it has weights. */
		public void setFrame (int frameIndex, float time, float[] vertices) {
			frames[frameIndex] = time;
			frameVertices[frameIndex] = vertices;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			Slot slot = skeleton.slots.get(slotIndex);
			Attachment slotAttachment = slot.attachment;
			if (!(slotAttachment instanceof VertexAttachment) || !((VertexAttachment)slotAttachment).applyDeform(attachment)) return;

			FloatArray verticesArray = slot.getAttachmentVertices();
			float[][] frameVertices = this.frameVertices;
			int vertexCount = frameVertices[0].length;
			float[] vertices = verticesArray.setSize(vertexCount);

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
				switch (pose) {
				case setup:
					float[] zeroVertices;
					if (vertexAttachment.getBones() == null) {
						// Unweighted vertex positions (setup pose).
						zeroVertices = vertexAttachment.getVertices();
					} else {
						// Weighted deform offsets (zeros).
						zeroVertices = zeros;
						if (zeroVertices.length < vertexCount) zeros = zeroVertices = new float[vertexCount];
					}
					System.arraycopy(zeroVertices, 0, vertices, 0, vertexCount);
					return;
				case current:
					if (alpha == 1) break;
					if (vertexAttachment.getBones() == null) {
						// Unweighted vertex positions.
						float[] setupVertices = vertexAttachment.getVertices();
						for (int i = 0; i < vertexCount; i++)
							vertices[i] += (setupVertices[i] - vertices[i]) * alpha;
					} else {
						// Weighted deform offsets.
						alpha = 1 - alpha;
						for (int i = 0; i < vertexCount; i++)
							vertices[i] *= alpha;
					}
				}
				return;
			}

			if (time >= frames[frames.length - 1]) { // Time is after last frame.
				float[] lastVertices = frameVertices[frames.length - 1];
				if (alpha == 1) {
					// Vertex positions or deform offsets, no alpha.
					System.arraycopy(lastVertices, 0, vertices, 0, vertexCount);
				} else if (pose == setup) {
					VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
					if (vertexAttachment.getBones() == null) {
						// Unweighted vertex positions, with alpha.
						float[] setupVertices = vertexAttachment.getVertices();
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
			int frame = binarySearch(frames, time);
			float[] prevVertices = frameVertices[frame - 1];
			float[] nextVertices = frameVertices[frame];
			float frameTime = frames[frame];
			float percent = getCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime));

			if (alpha == 1) {
				// Vertex positions or deform offsets, no alpha.
				for (int i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					vertices[i] = prev + (nextVertices[i] - prev) * percent;
				}
			} else if (pose == setup) {
				VertexAttachment vertexAttachment = (VertexAttachment)slotAttachment;
				if (vertexAttachment.getBones() == null) {
					// Unweighted vertex positions, with alpha.
					float[] setupVertices = vertexAttachment.getVertices();
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

	/** Fires an {@link Event} when specific animation times are reached. */
	static public class EventTimeline implements Timeline {
		private final float[] frames; // time, ...
		private final Event[] events;

		public EventTimeline (int frameCount) {
			frames = new float[frameCount];
			events = new Event[frameCount];
		}

		public int getPropertyId () {
			return TimelineType.event.ordinal() << 24;
		}

		/** The number of key frames for this timeline. */
		public int getFrameCount () {
			return frames.length;
		}

		/** The time in seconds for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** The event for each key frame. */
		public Event[] getEvents () {
			return events;
		}

		/** Sets the time in seconds and the event for the specified key frame. */
		public void setFrame (int frameIndex, Event event) {
			frames[frameIndex] = event.time;
			events[frameIndex] = event;
		}

		/** Fires events for frames > <code>lastTime</code> and <= <code>time</code>. */
		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> firedEvents, float alpha, MixPose pose,
			MixDirection direction) {

			if (firedEvents == null) return;
			float[] frames = this.frames;
			int frameCount = frames.length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				apply(skeleton, lastTime, Integer.MAX_VALUE, firedEvents, alpha, pose, direction);
				lastTime = -1f;
			} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return; // Time is before first frame.

			int frame;
			if (lastTime < frames[0])
				frame = 0;
			else {
				frame = binarySearch(frames, lastTime);
				float frameTime = frames[frame];
				while (frame > 0) { // Fire multiple events with the same frame.
					if (frames[frame - 1] != frameTime) break;
					frame--;
				}
			}
			for (; frame < frameCount && time >= frames[frame]; frame++)
				firedEvents.add(events[frame]);
		}
	}

	/** Changes a skeleton's {@link Skeleton#getDrawOrder()}. */
	static public class DrawOrderTimeline implements Timeline {
		private final float[] frames; // time, ...
		private final int[][] drawOrders;

		public DrawOrderTimeline (int frameCount) {
			frames = new float[frameCount];
			drawOrders = new int[frameCount][];
		}

		public int getPropertyId () {
			return TimelineType.drawOrder.ordinal() << 24;
		}

		/** The number of key frames for this timeline. */
		public int getFrameCount () {
			return frames.length;
		}

		/** The time in seconds for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** The draw order for each key frame. See {@link #setFrame(int, float, int[])}. */
		public int[][] getDrawOrders () {
			return drawOrders;
		}

		/** Sets the time in seconds and the draw order for the specified key frame.
		 * @param drawOrder For each slot in {@link Skeleton#slots}, the index of the new draw order. May be null to use setup pose
		 *           draw order. */
		public void setFrame (int frameIndex, float time, int[] drawOrder) {
			frames[frameIndex] = time;
			drawOrders[frameIndex] = drawOrder;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			Array<Slot> drawOrder = skeleton.drawOrder;
			Array<Slot> slots = skeleton.slots;
			if (direction == out && pose == setup) {
				System.arraycopy(slots.items, 0, drawOrder.items, 0, slots.size);
				return;
			}

			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (pose == setup) System.arraycopy(slots.items, 0, drawOrder.items, 0, slots.size);
				return;
			}

			int frame;
			if (time >= frames[frames.length - 1]) // Time is after last frame.
				frame = frames.length - 1;
			else
				frame = binarySearch(frames, time) - 1;

			int[] drawOrderToSetupIndex = drawOrders[frame];
			if (drawOrderToSetupIndex == null)
				System.arraycopy(slots.items, 0, drawOrder.items, 0, slots.size);
			else {
				for (int i = 0, n = drawOrderToSetupIndex.length; i < n; i++)
					drawOrder.set(i, slots.get(drawOrderToSetupIndex[i]));
			}
		}
	}

	/** Changes an IK constraint's {@link IkConstraint#getMix()} and {@link IkConstraint#getBendDirection()}. */
	static public class IkConstraintTimeline extends CurveTimeline {
		static public final int ENTRIES = 3;
		static private final int PREV_TIME = -3, PREV_MIX = -2, PREV_BEND_DIRECTION = -1;
		static private final int MIX = 1, BEND_DIRECTION = 2;

		int ikConstraintIndex;
		private final float[] frames; // time, mix, bendDirection, ...

		public IkConstraintTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * ENTRIES];
		}

		public int getPropertyId () {
			return (TimelineType.ikConstraint.ordinal() << 24) + ikConstraintIndex;
		}

		public void setIkConstraintIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.ikConstraintIndex = index;
		}

		/** The index of the IK constraint slot in {@link Skeleton#getIkConstraints()} that will be changed. */
		public int getIkConstraintIndex () {
			return ikConstraintIndex;
		}

		/** The time in seconds, mix, and bend direction for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** Sets the time in seconds, mix, and bend direction for the specified key frame. */
		public void setFrame (int frameIndex, float time, float mix, int bendDirection) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + MIX] = mix;
			frames[frameIndex + BEND_DIRECTION] = bendDirection;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			IkConstraint constraint = skeleton.ikConstraints.get(ikConstraintIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					constraint.mix = constraint.data.mix;
					constraint.bendDirection = constraint.data.bendDirection;
					return;
				case current:
					constraint.mix += (constraint.data.mix - constraint.mix) * alpha;
					constraint.bendDirection = constraint.data.bendDirection;
				}
				return;
			}

			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				if (pose == setup) {
					constraint.mix = constraint.data.mix + (frames[frames.length + PREV_MIX] - constraint.data.mix) * alpha;
					constraint.bendDirection = direction == out ? constraint.data.bendDirection
						: (int)frames[frames.length + PREV_BEND_DIRECTION];
				} else {
					constraint.mix += (frames[frames.length + PREV_MIX] - constraint.mix) * alpha;
					if (direction == in) constraint.bendDirection = (int)frames[frames.length + PREV_BEND_DIRECTION];
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			int frame = binarySearch(frames, time, ENTRIES);
			float mix = frames[frame + PREV_MIX];
			float frameTime = frames[frame];
			float percent = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			if (pose == setup) {
				constraint.mix = constraint.data.mix + (mix + (frames[frame + MIX] - mix) * percent - constraint.data.mix) * alpha;
				constraint.bendDirection = direction == out ? constraint.data.bendDirection
					: (int)frames[frame + PREV_BEND_DIRECTION];
			} else {
				constraint.mix += (mix + (frames[frame + MIX] - mix) * percent - constraint.mix) * alpha;
				if (direction == in) constraint.bendDirection = (int)frames[frame + PREV_BEND_DIRECTION];
			}
		}
	}

	/** Changes a transform constraint's mixes. */
	static public class TransformConstraintTimeline extends CurveTimeline {
		static public final int ENTRIES = 5;
		static private final int PREV_TIME = -5, PREV_ROTATE = -4, PREV_TRANSLATE = -3, PREV_SCALE = -2, PREV_SHEAR = -1;
		static private final int ROTATE = 1, TRANSLATE = 2, SCALE = 3, SHEAR = 4;

		int transformConstraintIndex;
		private final float[] frames; // time, rotate mix, translate mix, scale mix, shear mix, ...

		public TransformConstraintTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * ENTRIES];
		}

		public int getPropertyId () {
			return (TimelineType.transformConstraint.ordinal() << 24) + transformConstraintIndex;
		}

		public void setTransformConstraintIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.transformConstraintIndex = index;
		}

		/** The index of the transform constraint slot in {@link Skeleton#getTransformConstraints()} that will be changed. */
		public int getTransformConstraintIndex () {
			return transformConstraintIndex;
		}

		/** The time in seconds, rotate mix, translate mix, scale mix, and shear mix for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** The time in seconds, rotate mix, translate mix, scale mix, and shear mix for the specified key frame. */
		public void setFrame (int frameIndex, float time, float rotateMix, float translateMix, float scaleMix, float shearMix) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATE] = rotateMix;
			frames[frameIndex + TRANSLATE] = translateMix;
			frames[frameIndex + SCALE] = scaleMix;
			frames[frameIndex + SHEAR] = shearMix;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			TransformConstraint constraint = skeleton.transformConstraints.get(transformConstraintIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				TransformConstraintData data = constraint.data;
				switch (pose) {
				case setup:
					constraint.rotateMix = data.rotateMix;
					constraint.translateMix = data.translateMix;
					constraint.scaleMix = data.scaleMix;
					constraint.shearMix = data.shearMix;
					return;
				case current:
					constraint.rotateMix += (data.rotateMix - constraint.rotateMix) * alpha;
					constraint.translateMix += (data.translateMix - constraint.translateMix) * alpha;
					constraint.scaleMix += (data.scaleMix - constraint.scaleMix) * alpha;
					constraint.shearMix += (data.shearMix - constraint.shearMix) * alpha;
				}
				return;
			}

			float rotate, translate, scale, shear;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				int i = frames.length;
				rotate = frames[i + PREV_ROTATE];
				translate = frames[i + PREV_TRANSLATE];
				scale = frames[i + PREV_SCALE];
				shear = frames[i + PREV_SHEAR];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = binarySearch(frames, time, ENTRIES);
				rotate = frames[frame + PREV_ROTATE];
				translate = frames[frame + PREV_TRANSLATE];
				scale = frames[frame + PREV_SCALE];
				shear = frames[frame + PREV_SHEAR];
				float frameTime = frames[frame];
				float percent = getCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				rotate += (frames[frame + ROTATE] - rotate) * percent;
				translate += (frames[frame + TRANSLATE] - translate) * percent;
				scale += (frames[frame + SCALE] - scale) * percent;
				shear += (frames[frame + SHEAR] - shear) * percent;
			}
			if (pose == setup) {
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
	static public class PathConstraintPositionTimeline extends CurveTimeline {
		static public final int ENTRIES = 2;
		static final int PREV_TIME = -2, PREV_VALUE = -1;
		static final int VALUE = 1;

		int pathConstraintIndex;

		final float[] frames; // time, position, ...

		public PathConstraintPositionTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * ENTRIES];
		}

		public int getPropertyId () {
			return (TimelineType.pathConstraintPosition.ordinal() << 24) + pathConstraintIndex;
		}

		public void setPathConstraintIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.pathConstraintIndex = index;
		}

		/** The index of the path constraint slot in {@link Skeleton#getPathConstraints()} that will be changed. */
		public int getPathConstraintIndex () {
			return pathConstraintIndex;
		}

		/** The time in seconds and path constraint position for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** Sets the time in seconds and path constraint position for the specified key frame. */
		public void setFrame (int frameIndex, float time, float position) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + VALUE] = position;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			PathConstraint constraint = skeleton.pathConstraints.get(pathConstraintIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					constraint.position = constraint.data.position;
					return;
				case current:
					constraint.position += (constraint.data.position - constraint.position) * alpha;
				}
				return;
			}

			float position;
			if (time >= frames[frames.length - ENTRIES]) // Time is after last frame.
				position = frames[frames.length + PREV_VALUE];
			else {
				// Interpolate between the previous frame and the current frame.
				int frame = binarySearch(frames, time, ENTRIES);
				position = frames[frame + PREV_VALUE];
				float frameTime = frames[frame];
				float percent = getCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				position += (frames[frame + VALUE] - position) * percent;
			}
			if (pose == setup)
				constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
			else
				constraint.position += (position - constraint.position) * alpha;
		}
	}

	/** Changes a path constraint's {@link PathConstraint#getSpacing()}. */
	static public class PathConstraintSpacingTimeline extends PathConstraintPositionTimeline {
		public PathConstraintSpacingTimeline (int frameCount) {
			super(frameCount);
		}

		public int getPropertyId () {
			return (TimelineType.pathConstraintSpacing.ordinal() << 24) + pathConstraintIndex;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			PathConstraint constraint = skeleton.pathConstraints.get(pathConstraintIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					constraint.spacing = constraint.data.spacing;
					return;
				case current:
					constraint.spacing += (constraint.data.spacing - constraint.spacing) * alpha;
				}
				return;
			}

			float spacing;
			if (time >= frames[frames.length - ENTRIES]) // Time is after last frame.
				spacing = frames[frames.length + PREV_VALUE];
			else {
				// Interpolate between the previous frame and the current frame.
				int frame = binarySearch(frames, time, ENTRIES);
				spacing = frames[frame + PREV_VALUE];
				float frameTime = frames[frame];
				float percent = getCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				spacing += (frames[frame + VALUE] - spacing) * percent;
			}

			if (pose == setup)
				constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
			else
				constraint.spacing += (spacing - constraint.spacing) * alpha;
		}
	}

	/** Changes a path constraint's mixes. */
	static public class PathConstraintMixTimeline extends CurveTimeline {
		static public final int ENTRIES = 3;
		static private final int PREV_TIME = -3, PREV_ROTATE = -2, PREV_TRANSLATE = -1;
		static private final int ROTATE = 1, TRANSLATE = 2;

		int pathConstraintIndex;

		private final float[] frames; // time, rotate mix, translate mix, ...

		public PathConstraintMixTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * ENTRIES];
		}

		public int getPropertyId () {
			return (TimelineType.pathConstraintMix.ordinal() << 24) + pathConstraintIndex;
		}

		public void setPathConstraintIndex (int index) {
			if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
			this.pathConstraintIndex = index;
		}

		/** The index of the path constraint slot in {@link Skeleton#getPathConstraints()} that will be changed. */
		public int getPathConstraintIndex () {
			return pathConstraintIndex;
		}

		/** The time in seconds, rotate mix, and translate mix for each key frame. */
		public float[] getFrames () {
			return frames;
		}

		/** The time in seconds, rotate mix, and translate mix for the specified key frame. */
		public void setFrame (int frameIndex, float time, float rotateMix, float translateMix) {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATE] = rotateMix;
			frames[frameIndex + TRANSLATE] = translateMix;
		}

		public void apply (Skeleton skeleton, float lastTime, float time, Array<Event> events, float alpha, MixPose pose,
			MixDirection direction) {

			PathConstraint constraint = skeleton.pathConstraints.get(pathConstraintIndex);
			float[] frames = this.frames;
			if (time < frames[0]) { // Time is before first frame.
				switch (pose) {
				case setup:
					constraint.rotateMix = constraint.data.rotateMix;
					constraint.translateMix = constraint.data.translateMix;
					return;
				case current:
					constraint.rotateMix += (constraint.data.rotateMix - constraint.rotateMix) * alpha;
					constraint.translateMix += (constraint.data.translateMix - constraint.translateMix) * alpha;
				}
				return;
			}

			float rotate, translate;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				rotate = frames[frames.length + PREV_ROTATE];
				translate = frames[frames.length + PREV_TRANSLATE];
			} else {
				// Interpolate between the previous frame and the current frame.
				int frame = binarySearch(frames, time, ENTRIES);
				rotate = frames[frame + PREV_ROTATE];
				translate = frames[frame + PREV_TRANSLATE];
				float frameTime = frames[frame];
				float percent = getCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				rotate += (frames[frame + ROTATE] - rotate) * percent;
				translate += (frames[frame + TRANSLATE] - translate) * percent;
			}

			if (pose == setup) {
				constraint.rotateMix = constraint.data.rotateMix + (rotate - constraint.data.rotateMix) * alpha;
				constraint.translateMix = constraint.data.translateMix + (translate - constraint.data.translateMix) * alpha;
			} else {
				constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
				constraint.translateMix += (translate - constraint.translateMix) * alpha;
			}
		}
	}
}
