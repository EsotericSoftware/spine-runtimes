/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
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
 *****************************************************************************/

using System;
using System.Collections.Generic;

namespace Spine {
	public class AnimationStateData {
		internal SkeletonData skeletonData;
		private Dictionary<KeyValuePair<Animation, Animation>, float> animationToMixTime = new Dictionary<KeyValuePair<Animation, Animation>, float>();
		internal float defaultMix;

		public SkeletonData SkeletonData { get { return skeletonData; } }
		public float DefaultMix { get { return defaultMix; } set { defaultMix = value; } }

		public AnimationStateData (SkeletonData skeletonData) {
			this.skeletonData = skeletonData;
		}

		public void SetMix (String fromName, String toName, float duration) {
			Animation from = skeletonData.FindAnimation(fromName);
			if (from == null) throw new ArgumentException("Animation not found: " + fromName);
			Animation to = skeletonData.FindAnimation(toName);
			if (to == null) throw new ArgumentException("Animation not found: " + toName);
			SetMix(from, to, duration);
		}

		public void SetMix (Animation from, Animation to, float duration) {
			if (from == null) throw new ArgumentNullException("from cannot be null.");
			if (to == null) throw new ArgumentNullException("to cannot be null.");
			KeyValuePair<Animation, Animation> key = new KeyValuePair<Animation, Animation>(from, to);
			animationToMixTime.Remove(key);
			animationToMixTime.Add(key, duration);
		}

		public float GetMix (Animation from, Animation to) {
			KeyValuePair<Animation, Animation> key = new KeyValuePair<Animation, Animation>(from, to);
			float duration;
			if (animationToMixTime.TryGetValue(key, out duration)) return duration;
			return defaultMix;
		}
	}
}
