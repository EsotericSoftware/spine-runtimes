/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.utils.Array;

public class Animation {
	final String name;
	private final Array<Timeline> timelines;
	private float duration;

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

	/** Returns the duration of the animation in seconds. */
	public float getDuration () {
		return duration;
	}

	public void setDuration (float duration) {
		this.duration = duration;
	}

	/** Poses the skeleton at the specified time for this animation. */
	public void apply (Skeleton skeleton, float time, boolean loop) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		if (loop && duration != 0) time %= duration;

		Array<Timeline> timelines = this.timelines;
		for (int i = 0, n = timelines.size; i < n; i++)
			timelines.get(i).apply(skeleton, time, 1);
	}

	/** Poses the skeleton at the specified time for this animation mixed with the current pose.
	 * @param alpha The amount of this animation that affects the current pose. */
	public void mix (Skeleton skeleton, float time, boolean loop, float alpha) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		if (loop && duration != 0) time %= duration;

		Array<Timeline> timelines = this.timelines;
		for (int i = 0, n = timelines.size; i < n; i++)
			timelines.get(i).apply(skeleton, time, alpha);
	}

	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}

	/** @param target After the first and before the last entry. */
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

	static int linearSearch (float[] values, float target, int step) {
		for (int i = 0, last = values.length - step; i <= last; i += step)
			if (values[i] > target) return i;
		return -1;
	}

	static public interface Timeline {
		/** Sets the value(s) for the specified time. */
		public void apply (Skeleton skeleton, float time, float alpha);
	}

	/** Base class for frames that use an interpolation bezier curve. */
	abstract static public class CurveTimeline implements Timeline {
		static public final float LINEAR = 0;
		static public final float STEPPED = -1;
		static public final float BEZIER = -2;
		static private final int BEZIER_SEGMENTS = 10;

		private final float[] curves; // dfx, dfy, ddfx, ddfy, dddfx, dddfy, ...

		public CurveTimeline (int frameCount) {
			curves = new float[(frameCount - 1) * 6];
		}

		public int getFrameCount () {
			return curves.length / 6 + 1;
		}

		public void setLinear (int frameIndex) {
			curves[frameIndex * 6] = LINEAR;
		}

		public void setStepped (int frameIndex) {
			curves[frameIndex * 6] = STEPPED;
		}

		public float getCurveType (int frameIndex) {
			int index = frameIndex * 6;
			if (index == curves.length) return LINEAR;
			float type = curves[index];
			if (type == LINEAR) return LINEAR;
			if (type == STEPPED) return STEPPED;
			return BEZIER;
		}

		/** Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
		 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
		 * the difference between the keyframe's values. */
		public void setCurve (int frameIndex, float cx1, float cy1, float cx2, float cy2) {
			float subdiv_step = 1f / BEZIER_SEGMENTS;
			float subdiv_step2 = subdiv_step * subdiv_step;
			float subdiv_step3 = subdiv_step2 * subdiv_step;
			float pre1 = 3 * subdiv_step;
			float pre2 = 3 * subdiv_step2;
			float pre4 = 6 * subdiv_step2;
			float pre5 = 6 * subdiv_step3;
			float tmp1x = -cx1 * 2 + cx2;
			float tmp1y = -cy1 * 2 + cy2;
			float tmp2x = (cx1 - cx2) * 3 + 1;
			float tmp2y = (cy1 - cy2) * 3 + 1;
			int i = frameIndex * 6;
			float[] curves = this.curves;
			curves[i] = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3;
			curves[i + 1] = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;
			curves[i + 2] = tmp1x * pre4 + tmp2x * pre5;
			curves[i + 3] = tmp1y * pre4 + tmp2y * pre5;
			curves[i + 4] = tmp2x * pre5;
			curves[i + 5] = tmp2y * pre5;
		}

		public float getCurvePercent (int frameIndex, float percent) {
			int curveIndex = frameIndex * 6;
			float[] curves = this.curves;
			float dfx = curves[curveIndex];
			if (dfx == LINEAR) return percent;
			if (dfx == STEPPED) return 0;
			float dfy = curves[curveIndex + 1];
			float ddfx = curves[curveIndex + 2];
			float ddfy = curves[curveIndex + 3];
			float dddfx = curves[curveIndex + 4];
			float dddfy = curves[curveIndex + 5];
			float x = dfx, y = dfy;
			int i = BEZIER_SEGMENTS - 2;
			while (true) {
				if (x >= percent) {
					float lastX = x - dfx;
					float lastY = y - dfy;
					return lastY + (y - lastY) * (percent - lastX) / (x - lastX);
				}
				if (i == 0) break;
				i--;
				dfx += ddfx;
				dfy += ddfy;
				ddfx += dddfx;
				ddfy += dddfy;
				x += dfx;
				y += dfy;
			}
			return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
		}
	}

	static public class RotateTimeline extends CurveTimeline {
		static private final int LAST_FRAME_TIME = -2;
		static private final int FRAME_VALUE = 1;

		private int boneIndex;
		private final float[] frames; // time, angle, ...

		public RotateTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * 2];
		}

		public void setBoneIndex (int boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public float[] getFrames () {
			return frames;
		}

		/** Sets the time and angle of the specified keyframe. */
		public void setFrame (int frameIndex, float time, float angle) {
			frameIndex *= 2;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = angle;
		}

		public void apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.bones.get(boneIndex);

			if (time >= frames[frames.length - 2]) { // Time is after last frame.
				float amount = bone.data.rotation + frames[frames.length - 1] - bone.rotation;
				while (amount > 180)
					amount -= 360;
				while (amount < -180)
					amount += 360;
				bone.rotation += amount * alpha;
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = binarySearch(frames, time, 2);
			float lastFrameValue = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = MathUtils.clamp(1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime), 0, 1);
			percent = getCurvePercent(frameIndex / 2 - 1, percent);

			float amount = frames[frameIndex + FRAME_VALUE] - lastFrameValue;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			amount = bone.data.rotation + (lastFrameValue + amount * percent) - bone.rotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			bone.rotation += amount * alpha;
		}
	}

	static public class TranslateTimeline extends CurveTimeline {
		static final int LAST_FRAME_TIME = -3;
		static final int FRAME_X = 1;
		static final int FRAME_Y = 2;

		int boneIndex;
		final float[] frames; // time, x, y, ...

		public TranslateTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * 3];
		}

		public void setBoneIndex (int boneIndex) {
			this.boneIndex = boneIndex;
		}

		public int getBoneIndex () {
			return boneIndex;
		}

		public float[] getFrames () {
			return frames;
		}

		/** Sets the time and value of the specified keyframe. */
		public void setFrame (int frameIndex, float time, float x, float y) {
			frameIndex *= 3;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = x;
			frames[frameIndex + 2] = y;
		}

		public void apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.bones.get(boneIndex);

			if (time >= frames[frames.length - 3]) { // Time is after last frame.
				bone.x += (bone.data.x + frames[frames.length - 2] - bone.x) * alpha;
				bone.y += (bone.data.y + frames[frames.length - 1] - bone.y) * alpha;
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = binarySearch(frames, time, 3);
			float lastFrameX = frames[frameIndex - 2];
			float lastFrameY = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = MathUtils.clamp(1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime), 0, 1);
			percent = getCurvePercent(frameIndex / 3 - 1, percent);

			bone.x += (bone.data.x + lastFrameX + (frames[frameIndex + FRAME_X] - lastFrameX) * percent - bone.x) * alpha;
			bone.y += (bone.data.y + lastFrameY + (frames[frameIndex + FRAME_Y] - lastFrameY) * percent - bone.y) * alpha;
		}
	}

	static public class ScaleTimeline extends TranslateTimeline {
		public ScaleTimeline (int frameCount) {
			super(frameCount);
		}

		public void apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Bone bone = skeleton.bones.get(boneIndex);
			if (time >= frames[frames.length - 3]) { // Time is after last frame.
				bone.scaleX += (bone.data.scaleX - 1 + frames[frames.length - 2] - bone.scaleX) * alpha;
				bone.scaleY += (bone.data.scaleY - 1 + frames[frames.length - 1] - bone.scaleY) * alpha;
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = binarySearch(frames, time, 3);
			float lastFrameX = frames[frameIndex - 2];
			float lastFrameY = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = MathUtils.clamp(1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime), 0, 1);
			percent = getCurvePercent(frameIndex / 3 - 1, percent);

			bone.scaleX += (bone.data.scaleX - 1 + lastFrameX + (frames[frameIndex + FRAME_X] - lastFrameX) * percent - bone.scaleX)
				* alpha;
			bone.scaleY += (bone.data.scaleY - 1 + lastFrameY + (frames[frameIndex + FRAME_Y] - lastFrameY) * percent - bone.scaleY)
				* alpha;
		}
	}

	static public class ColorTimeline extends CurveTimeline {
		static private final int LAST_FRAME_TIME = -5;
		static private final int FRAME_R = 1;
		static private final int FRAME_G = 2;
		static private final int FRAME_B = 3;
		static private final int FRAME_A = 4;

		private int slotIndex;
		private final float[] frames; // time, r, g, b, a, ...

		public ColorTimeline (int frameCount) {
			super(frameCount);
			frames = new float[frameCount * 5];
		}

		public void setSlotIndex (int slotIndex) {
			this.slotIndex = slotIndex;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		public float[] getFrames () {
			return frames;
		}

		/** Sets the time and value of the specified keyframe. */
		public void setFrame (int frameIndex, float time, float r, float g, float b, float a) {
			frameIndex *= 5;
			frames[frameIndex] = time;
			frames[frameIndex + 1] = r;
			frames[frameIndex + 2] = g;
			frames[frameIndex + 3] = b;
			frames[frameIndex + 4] = a;
		}

		public void apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			Color color = skeleton.slots.get(slotIndex).color;

			if (time >= frames[frames.length - 5]) { // Time is after last frame.
				int i = frames.length - 1;
				float r = frames[i - 3];
				float g = frames[i - 2];
				float b = frames[i - 1];
				float a = frames[i];
				color.set(r, g, b, a);
				return;
			}

			// Interpolate between the last frame and the current frame.
			int frameIndex = binarySearch(frames, time, 5);
			float lastFrameR = frames[frameIndex - 4];
			float lastFrameG = frames[frameIndex - 3];
			float lastFrameB = frames[frameIndex - 2];
			float lastFrameA = frames[frameIndex - 1];
			float frameTime = frames[frameIndex];
			float percent = MathUtils.clamp(1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime), 0, 1);
			percent = getCurvePercent(frameIndex / 5 - 1, percent);

			float r = lastFrameR + (frames[frameIndex + FRAME_R] - lastFrameR) * percent;
			float g = lastFrameG + (frames[frameIndex + FRAME_G] - lastFrameG) * percent;
			float b = lastFrameB + (frames[frameIndex + FRAME_B] - lastFrameB) * percent;
			float a = lastFrameA + (frames[frameIndex + FRAME_A] - lastFrameA) * percent;
			if (alpha < 1)
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			else
				color.set(r, g, b, a);
		}
	}

	static public class AttachmentTimeline implements Timeline {
		private int slotIndex;
		private final float[] frames; // time, ...
		private final String[] attachmentNames;

		public AttachmentTimeline (int frameCount) {
			frames = new float[frameCount];
			attachmentNames = new String[frameCount];
		}

		public int getFrameCount () {
			return frames.length;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		public void setSlotIndex (int slotIndex) {
			this.slotIndex = slotIndex;
		}

		public float[] getFrames () {
			return frames;
		}

		public String[] getAttachmentNames () {
			return attachmentNames;
		}

		/** Sets the time and value of the specified keyframe. */
		public void setFrame (int frameIndex, float time, String attachmentName) {
			frames[frameIndex] = time;
			attachmentNames[frameIndex] = attachmentName;
		}

		public void apply (Skeleton skeleton, float time, float alpha) {
			float[] frames = this.frames;
			if (time < frames[0]) return; // Time is before first frame.

			int frameIndex;
			if (time >= frames[frames.length - 1]) // Time is after last frame.
				frameIndex = frames.length - 1;
			else
				frameIndex = binarySearch(frames, time, 1) - 1;

			String attachmentName = attachmentNames[frameIndex];
			skeleton.slots.get(slotIndex).setAttachment(
				attachmentName == null ? null : skeleton.getAttachment(slotIndex, attachmentName));
		}
	}
}
