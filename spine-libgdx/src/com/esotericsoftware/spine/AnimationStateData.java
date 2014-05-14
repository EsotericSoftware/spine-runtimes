/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
		return animationToMixTime.get(tempKey, defaultMix);
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
