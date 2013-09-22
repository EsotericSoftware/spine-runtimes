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
	public class AnimationState {
		public AnimationStateData Data { get; private set; }
		public Animation Animation { get; private set; }

		private float time;
		public float Time {
			get { return time; }
			set {
				time = value;
				currentLastTime = value - 0.00001f;
			}
		}

		private float currentLastTime;
		public bool Loop { get; set; }
		private Animation previous;
		private float previousTime;
		private bool previousLoop;
		private QueueEntry currentQueueEntry;
		private float mixTime, mixDuration;
		private List<Event> events = new List<Event>();
		private List<QueueEntry> queue = new List<QueueEntry>();

		public event EventHandler Start;
		public event EventHandler End;
		public event EventHandler<EventTriggeredArgs> Event;
		public event EventHandler<CompleteArgs> Complete;

		public AnimationState (AnimationStateData data) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			Data = data;
		}

		public void Update (float delta) {
			time += delta;
			previousTime += delta;
			mixTime += delta;

			if (Animation != null) {
				float duration = Animation.Duration;
				if (Loop ? (currentLastTime % duration > time % duration)
					: (currentLastTime < duration && time >= duration)) {
					int count = (int)(time / duration);
					if (currentQueueEntry != null) currentQueueEntry.OnComplete(this, count);
					if (Complete != null) Complete(this, new CompleteArgs(count));
				}
			}

			if (queue.Count > 0) {
				QueueEntry entry = queue[0];
				if (time >= entry.delay) {
					SetAnimationInternal(entry.animation, entry.loop, entry);
					queue.RemoveAt(0);
				}
			}
		}

		public void Apply (Skeleton skeleton) {
			if (Animation == null) return;

			List<Event> events = this.events;
			events.Clear();

			if (previous != null) {
				previous.Apply(skeleton, int.MaxValue, previousTime, previousLoop, null);
				float alpha = mixTime / mixDuration;
				if (alpha >= 1) {
					alpha = 1;
					previous = null;
				}
				Animation.Mix(skeleton, currentLastTime, time, Loop, events, alpha);
			} else
				Animation.Apply(skeleton, currentLastTime, time, Loop, events);

			if (Event != null || currentQueueEntry != null) {
				foreach (Event e in events) {
					if (currentQueueEntry != null) currentQueueEntry.OnEvent(this, e);
					if (Event != null) Event(this, new EventTriggeredArgs(e));
				}
			}

			currentLastTime = time;
		}

		public void ClearAnimation () {
			previous = null;
			Animation = null;
			queue.Clear();
		}

		private void SetAnimationInternal (Animation animation, bool loop, QueueEntry entry) {
			previous = null;
			if (Animation != null) {
				if (currentQueueEntry != null) currentQueueEntry.OnEnd(this);
				if (End != null) End(this, EventArgs.Empty);

				if (animation != null) {
					mixDuration = Data.GetMix(Animation, animation);
					if (mixDuration > 0) {
						mixTime = 0;
						previous = Animation;
						previousTime = time;
						previousLoop = Loop;
					}
				}
			}
			Animation = animation;
			Loop = loop;
			time = 0;
			currentLastTime = 0;
			currentQueueEntry = entry;

			if (currentQueueEntry != null) currentQueueEntry.OnStart(this);
			if (Start != null) Start(this, EventArgs.Empty);
		}

		public void SetAnimation (String animationName, bool loop) {
			Animation animation = Data.SkeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName);
			SetAnimation(animation, loop);
		}

		/** Set the current animation. Any queued animations are cleared and the current animation time is set to 0.
		 * @param animation May be null.
		 * @param listener May be null. */
		public void SetAnimation (Animation animation, bool loop) {
			queue.Clear();
			SetAnimationInternal(animation, loop, null);
		}

		public QueueEntry AddAnimation (String animationName, bool loop) {
			return AddAnimation(animationName, loop, 0);
		}

		public QueueEntry AddAnimation (String animationName, bool loop, float delay) {
			Animation animation = Data.SkeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName);
			return AddAnimation(animation, loop, delay);
		}

		public QueueEntry AddAnimation (Animation animation, bool loop) {
			return AddAnimation(animation, loop, 0);
		}

		/** Adds an animation to be played delay seconds after the current or last queued animation.
		 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay. */
		public QueueEntry AddAnimation (Animation animation, bool loop, float delay) {
			QueueEntry entry = new QueueEntry();
			entry.animation = animation;
			entry.loop = loop;

			if (delay <= 0) {
				Animation previousAnimation = queue.Count == 0 ? Animation : queue[queue.Count - 1].animation;
				if (previousAnimation != null)
					delay = previousAnimation.Duration - Data.GetMix(previousAnimation, animation) + delay;
				else
					delay = 0;
			}
			entry.delay = delay;

			queue.Add(entry);
			return entry;
		}

		/** Returns true if no animation is set or if the current time is greater than the animation duration, regardless of looping. */
		public bool IsComplete () {
			return Animation == null || time >= Animation.Duration;
		}

		override public String ToString () {
			return (Animation != null && Animation.Name != null) ? Animation.Name : base.ToString();
		}
	}

	public class EventTriggeredArgs : EventArgs {
		public Event Event { get; private set; }

		public EventTriggeredArgs (Event e) {
			Event = e;
		}
	}

	public class CompleteArgs : EventArgs {
		public int LoopCount { get; private set; }

		public CompleteArgs (int loopCount) {
			LoopCount = loopCount;
		}
	}

	public class QueueEntry {
		internal Spine.Animation animation;
		internal bool loop;
		internal float delay;

		public event EventHandler Start;
		public event EventHandler End;
		public event EventHandler<EventTriggeredArgs> Event;
		public event EventHandler<CompleteArgs> Complete;

		internal void OnStart (AnimationState state) {
			if (Start != null) Start(state, EventArgs.Empty);
		}

		internal void OnEnd (AnimationState state) {
			if (End != null) End(state, EventArgs.Empty);
		}

		internal void OnEvent (AnimationState state, Event e) {
			if (Event != null) Event(state, new EventTriggeredArgs(e));
		}

		internal void OnComplete (AnimationState state, int loopCount) {
			if (Complete != null) Complete(state, new CompleteArgs(loopCount));
		}
	}
}
