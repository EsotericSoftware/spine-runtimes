/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
