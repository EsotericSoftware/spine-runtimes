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

using System;
using System.Collections.Generic;

namespace Spine {
	public class AnimationStateData {
		internal SkeletonData skeletonData;

		readonly Dictionary<AnimationPair, float> animationToMixTime = new Dictionary<AnimationPair, float>(AnimationPairComparer.Instance);
		internal float defaultMix;

		public SkeletonData SkeletonData { get { return skeletonData; } }
		public float DefaultMix { get { return defaultMix; } set { defaultMix = value; } }

		public AnimationStateData (SkeletonData skeletonData) {
			if (skeletonData == null) throw new ArgumentException ("skeletonData cannot be null.");
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
			if (from == null) throw new ArgumentNullException("from", "from cannot be null.");
			if (to == null) throw new ArgumentNullException("to", "to cannot be null.");
			AnimationPair key = new AnimationPair(from, to);
			animationToMixTime.Remove(key);
			animationToMixTime.Add(key, duration);
		}

		public float GetMix (Animation from, Animation to) {
			if (from == null) throw new ArgumentNullException("from", "from cannot be null.");
			if (to == null) throw new ArgumentNullException("to", "to cannot be null.");
			AnimationPair key = new AnimationPair(from, to);
			float duration;
			if (animationToMixTime.TryGetValue(key, out duration)) return duration;
			return defaultMix;
		}

		struct AnimationPair {
			public readonly Animation a1;
			public readonly Animation a2;

			public AnimationPair (Animation a1, Animation a2) {
				this.a1 = a1;
				this.a2 = a2;
			}

			public override string ToString () {
				return a1.name + "->" + a2.name;
			}
		}

		// Avoids boxing in the dictionary.
		class AnimationPairComparer : IEqualityComparer<AnimationPair> {
			internal static readonly AnimationPairComparer Instance = new AnimationPairComparer();

			bool IEqualityComparer<AnimationPair>.Equals (AnimationPair x, AnimationPair y) {
				return ReferenceEquals(x.a1, y.a1) && ReferenceEquals(x.a2, y.a2);
			}

			int IEqualityComparer<AnimationPair>.GetHashCode (AnimationPair obj) {
				// from Tuple.CombineHashCodes // return (((h1 << 5) + h1) ^ h2);
				int h1 = obj.a1.GetHashCode();
				return (((h1 << 5) + h1) ^ obj.a2.GetHashCode());
			}
		}
	}
}
