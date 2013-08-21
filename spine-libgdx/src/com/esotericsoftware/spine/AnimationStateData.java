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

import com.badlogic.gdx.utils.ObjectFloatMap;

/** Stores mixing times between animations. */
public class AnimationStateData {
	private final SkeletonData skeletonData;
	final ObjectFloatMap<Key> animationToMixTime = new ObjectFloatMap();
	final Key tempKey = new Key();
	float defaultMix;

	public AnimationStateData (SkeletonData skeletonData) {
		this.skeletonData = skeletonData;
	}

	public SkeletonData getSkeletonData () {
		return skeletonData;
	}

	public void setMix (String fromName, String toName, float duration) {
		Animation from = skeletonData.findAnimation(fromName);
		if (from == null) throw new IllegalArgumentException("Animation not found: " + fromName);
		Animation to = skeletonData.findAnimation(toName);
		if (to == null) throw new IllegalArgumentException("Animation not found: " + toName);
		setMix(from, to, duration);
	}

	public void setMix (Animation from, Animation to, float duration) {
		if (from == null) throw new IllegalArgumentException("from cannot be null.");
		if (to == null) throw new IllegalArgumentException("to cannot be null.");
		Key key = new Key();
		key.a1 = from;
		key.a2 = to;
		animationToMixTime.put(key, duration);
	}

	public float getMix (Animation from, Animation to) {
		tempKey.a1 = from;
		tempKey.a2 = to;
		float time = animationToMixTime.get(tempKey, Float.MIN_VALUE);
		if (time == Float.MIN_VALUE) return defaultMix;
		return time;
	}

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
	}
}
