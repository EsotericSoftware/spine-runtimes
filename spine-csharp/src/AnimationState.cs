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

		public void Clear () {
			previous = null;
			Animation = null;
		}

		override public String ToString () {
			return (Animation != null && Animation.Name != null) ? Animation.Name : base.ToString();
		}
	}
}
