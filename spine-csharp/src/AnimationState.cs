using System;
using System.Collections.Generic;

namespace Spine {
	public class AnimationState {
		public AnimationStateData Data { get; private set; }
		public Animation Animation { get; private set; }
		public float Time { get; set; }
		public bool Loop { get; set; }
		private Animation previous;
		float previousTime;
		bool previousLoop;
		float mixTime, mixDuration;

		public AnimationState (AnimationStateData data) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			Data = data;
		}

		public void Update (float delta) {
			Time += delta;
			previousTime += delta;
			mixTime += delta;
		}

		public void Apply (Skeleton skeleton) {
			if (Animation == null) return;
			if (previous != null) {
				previous.Apply(skeleton, previousTime, previousLoop);
				float alpha = mixTime / mixDuration;
				if (alpha >= 1) {
					alpha = 1;
					previous = null;
				}
				Animation.Mix(skeleton, Time, Loop, alpha);
			} else
				Animation.Apply(skeleton, Time, Loop);
		}

		public void SetAnimation (String animationName, bool loop) {
			Animation animation = Data.SkeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName);
			SetAnimation(animation, loop);
		}

		public void SetAnimation (Animation animation, bool loop) {
			previous = null;
			if (animation != null && Animation != null) {
				mixDuration = Data.GetMix(Animation, animation);
				if (mixDuration > 0) {
					mixTime = 0;
					previous = Animation;
					previousTime = Time;
					previousLoop = Loop;
				}
			}
			Animation = animation;
			Loop = loop;
			Time = 0;
		}

		override public String ToString () {
			return (Animation != null && Animation.Name != null) ? Animation.Name : base.ToString();
		}
	}
}
