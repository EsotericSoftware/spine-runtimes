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
		private AnimationStateData data;
		private QueuedAnimation current, previous;
		private float mixTime, mixDuration;
		private List<QueuedAnimation> queue = new List<QueuedAnimation>();
		private List<Event> events = new List<Event>();

		public AnimationStateData Data { get { return data; } }
		public List<QueuedAnimation> Queue { get { return queue; } }
		public QueuedAnimation Current { get { return current; } }
		public Animation Animation {
			get { return current != null ? current.animation : null; }
		}
		public float Time {
			get { return current != null ? current.time : 0; }
			set { if (current != null) current.Time = value; }
		}
		public bool Loop {
			get { return current != null ? current.loop : false; }
			set { if (current != null) current.Loop = value; }
		}

		public event EventHandler Start;
		public event EventHandler End;
		public event EventHandler<EventTriggeredArgs> Event;
		public event EventHandler<CompleteArgs> Complete;

		public AnimationState (AnimationStateData data) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			this.data = data;
		}

		public void Update (float delta) {
			QueuedAnimation current = this.current;
			if (current == null) return;

			float time = current.time;
			float duration = current.endTime;

			current.time = time + delta;
			if (previous != null) {
				previous.time += delta;
				mixTime += delta;
			}

			// Check if completed the animation or a loop iteration.
			if (current.loop ? (current.lastTime % duration > time % duration) : (current.lastTime < duration && time >= duration)) {
				int count = (int)(time / duration);
				current.OnComplete(this, count);
				if (Complete != null) Complete(this, new CompleteArgs(count));
			}

			if (queue.Count > 0) {
				QueuedAnimation entry = queue[0];
				if (time >= entry.delay) {
					if (entry.animation == null)
						ClearAnimation();
					else {
						SetAnimationEntry(entry);
						queue.RemoveAt(0);
					}
				}
			}
		}

		public void Apply (Skeleton skeleton) {
			QueuedAnimation current = this.current;
			if (current == null) return;

			List<Event> events = this.events;
			events.Clear();

			QueuedAnimation previous = this.previous;
			if (previous != null) {
				previous.animation.Apply(skeleton, int.MaxValue, previous.time, previous.loop, null);
				float alpha = mixTime / mixDuration;
				if (alpha >= 1) {
					alpha = 1;
					this.previous = null;
				}
				current.animation.Mix(skeleton, current.lastTime, current.time, current.loop, events, alpha);
			} else
				current.animation.Apply(skeleton, current.lastTime, current.time, current.loop, events);

			foreach (Event e in events) {
				current.OnEvent(this, e);
				if (Event != null) Event(this, new EventTriggeredArgs(e));
			}

			current.lastTime = current.time;
		}

		public void ClearAnimation () {
			previous = null;
			current = null;
			queue.Clear();
		}

		private void SetAnimationEntry (QueuedAnimation entry) {
			previous = null;

			QueuedAnimation current = this.current;
			if (current != null) {
				current.OnEnd(this);
				if (End != null) End(this, EventArgs.Empty);

				mixDuration = data.GetMix(current.animation, entry.animation);
				if (mixDuration > 0) {
					mixTime = 0;
					previous = current;
				}
			}
			this.current = entry;

			entry.OnStart(this);
			if (Start != null) Start(this, EventArgs.Empty);
		}

		public QueuedAnimation SetAnimation (String animationName, bool loop) {
			Animation animation = data.skeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName);
			return SetAnimation(animation, loop);
		}

		/** Set the current animation. Any queued animations are cleared. */
		public QueuedAnimation SetAnimation (Animation animation, bool loop) {
			queue.Clear();
			QueuedAnimation entry = new QueuedAnimation();
			entry.animation = animation;
			entry.loop = loop;
			entry.time = 0;
			entry.endTime = animation.Duration;
			SetAnimationEntry(entry);
			return entry;
		}

		public QueuedAnimation AddAnimation (String animationName, bool loop) {
			return AddAnimation(animationName, loop, 0);
		}

		public QueuedAnimation AddAnimation (String animationName, bool loop, float delay) {
			Animation animation = data.skeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName);
			return AddAnimation(animation, loop, delay);
		}

		public QueuedAnimation AddAnimation (Animation animation, bool loop) {
			return AddAnimation(animation, loop, 0);
		}

		/** Adds an animation to be played delay seconds after the current or last queued animation.
		 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay. */
		public QueuedAnimation AddAnimation (Animation animation, bool loop, float delay) {
			QueuedAnimation entry = new QueuedAnimation();
			entry.animation = animation;
			entry.loop = loop;
			entry.time = 0;
			entry.endTime = animation != null ? animation.Duration : 0;

			if (delay <= 0) {
				QueuedAnimation previousEntry = queue.Count > 0 ? queue[queue.Count - 1] : current;
				if (previousEntry != null) {
					delay += previousEntry.endTime;
					if (animation != null) delay += -data.GetMix(previousEntry.animation, animation);
				} else
					delay = 0;
			}
			entry.delay = delay;

			queue.Add(entry);
			return entry;
		}

		/** Returns true if no animation is set or if the current time is greater than the animation duration, regardless of looping. */
		public bool IsComplete () {
			return current == null || current.time >= current.endTime;
		}

		override public String ToString () {
			if (current == null || current.animation == null) return "<none>";
			return current.animation.Name;
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

	public class QueuedAnimation {
		internal Animation animation;
		internal bool loop;
		internal float delay, time, lastTime, endTime;

		public Animation Animation { get { return animation; } }
		public bool Loop { get { return loop; } set { loop = value; } }
		public float Delay { get { return delay; } set { delay = value; } }
		public float EndTime { get { return EndTime; } set { EndTime = value; } }

		public float Time {
			get { return time; }
			set {
				time = value;
				if (lastTime < value) lastTime = value;
			}
		}

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
