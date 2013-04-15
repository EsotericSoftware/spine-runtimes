using System;
using System.Collections.Generic;

namespace Spine {
	public class AnimationStateData {
		public SkeletonData SkeletonData { get; private set; }
		private Dictionary<KeyValuePair<Animation, Animation>, float> animationToMixTime = new Dictionary<KeyValuePair<Animation, Animation>, float>();

		public AnimationStateData (SkeletonData skeletonData) {
			SkeletonData = skeletonData;
		}

		public void SetMix (String fromName, String toName, float duration) {
			Animation from = SkeletonData.FindAnimation(fromName);
			if (from == null) throw new ArgumentException("Animation not found: " + fromName);
			Animation to = SkeletonData.FindAnimation(toName);
			if (to == null) throw new ArgumentException("Animation not found: " + toName);
			SetMix(from, to, duration);
		}

		public void SetMix (Animation from, Animation to, float duration) {
			if (from == null) throw new ArgumentNullException("from cannot be null.");
			if (to == null) throw new ArgumentNullException("to cannot be null.");
			animationToMixTime.Add(new KeyValuePair<Animation, Animation>(from, to), duration);
		}

		public float GetMix (Animation from, Animation to) {
			KeyValuePair<Animation, Animation> key = new KeyValuePair<Animation, Animation>(from, to);
			float duration;
			animationToMixTime.TryGetValue(key, out duration);
			return duration;
		}
	}
}
