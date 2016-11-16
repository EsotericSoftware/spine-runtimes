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

import com.badlogic.gdx.utils.ObjectFloatMap;
import com.esotericsoftware.spine.AnimationState.TrackEntry;

/** Stores mix (crossfade) durations to be applied when {@link AnimationState} animations are changed. */
public class AnimationStateData {
	final SkeletonData skeletonData;
	final ObjectFloatMap<Key> animationToMixTime = new ObjectFloatMap();
	final Key tempKey = new Key();
	float defaultMix;

	public AnimationStateData (SkeletonData skeletonData) {
		if (skeletonData == null) throw new IllegalArgumentException("skeletonData cannot be null.");
		this.skeletonData = skeletonData;
	}

	/** The SkeletonData to look up animations when they are specified by name. */
	public SkeletonData getSkeletonData () {
		return skeletonData;
	}

	/** Sets a mix duration by animation name.
	 * <p>
	 * See {@link #setMix(Animation, Animation, float)}. */
	public void setMix (String fromName, String toName, float duration) {
		Animation from = skeletonData.findAnimation(fromName);
		if (from == null) throw new IllegalArgumentException("Animation not found: " + fromName);
		Animation to = skeletonData.findAnimation(toName);
		if (to == null) throw new IllegalArgumentException("Animation not found: " + toName);
		setMix(from, to, duration);
	}

	/** Sets the mix duration when changing from the specified animation to the other.
	 * <p>
	 * See {@link TrackEntry#mixDuration}. */
	public void setMix (Animation from, Animation to, float duration) {
		if (from == null) throw new IllegalArgumentException("from cannot be null.");
		if (to == null) throw new IllegalArgumentException("to cannot be null.");
		Key key = new Key();
		key.a1 = from;
		key.a2 = to;
		animationToMixTime.put(key, duration);
	}

	/** Returns the mix duration to use when changing from the specified animation to the other, or the {@link #getDefaultMix()} if
	 * no mix duration has been set. */
	public float getMix (Animation from, Animation to) {
		if (from == null) throw new IllegalArgumentException("from cannot be null.");
		if (to == null) throw new IllegalArgumentException("to cannot be null.");
		tempKey.a1 = from;
		tempKey.a2 = to;
		return animationToMixTime.get(tempKey, defaultMix);
	}

	/** The mix duration to use when no mix duration has been defined between two animations. */
	public float getDefaultMix () {
		return defaultMix;
	}

	public void setDefaultMix (float defaultMix) {
		this.defaultMix = defaultMix;
	}

	static class Key {
		Animation a1, a2;

		public int hashCode () {
			return 31 * (31 + a1.hashCode()) + a2.hashCode();
		}

		public boolean equals (Object obj) {
			if (this == obj) return true;
			if (obj == null) return false;
			Key other = (Key)obj;
			if (a1 == null) {
				if (other.a1 != null) return false;
			} else if (!a1.equals(other.a1)) return false;
			if (a2 == null) {
				if (other.a2 != null) return false;
			} else if (!a2.equals(other.a2)) return false;
			return true;
		}

		public String toString () {
			return a1.name + "->" + a2.name;
		}
	}
}
