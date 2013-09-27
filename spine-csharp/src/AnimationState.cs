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
using System.Text;

namespace Spine {
	public class AnimationState {
		private AnimationStateData data;
		private List<TrackEntry> tracks = new List<TrackEntry>();
		private List<Event> events = new List<Event>();

		public AnimationStateData Data { get { return data; } }

		public event EventHandler<StartEndArgs> Start;
		public event EventHandler<StartEndArgs> End;
		public event EventHandler<EventTriggeredArgs> Event;
		public event EventHandler<CompleteArgs> Complete;

		public AnimationState (AnimationStateData data) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			this.data = data;
		}

		public void Update (float delta) {
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry current = tracks[i];
				if (current == null) continue;

				float time = current.time + delta;
				float endTime = current.endTime;

				current.time = time;
				if (current.previous != null) {
					current.previous.time += delta;
					current.mixTime += delta;
				}

				// Check if completed the animation or a loop iteration.
				if (current.loop ? (current.lastTime % endTime > time % endTime) : (current.lastTime < endTime && time >= endTime)) {
					int count = (int)(time / endTime);
					current.OnComplete(this, i, count);
					if (Complete != null) Complete(this, new CompleteArgs(i, count));
				}

				TrackEntry next = current.next;
				if (next != null && time >= next.delay) {
					if (next.animation != null)
						SetCurrent(i, next);
					else
						Clear(i);
				}
			}
		}

		public void Apply (Skeleton skeleton) {
			List<Event> events = this.events;

			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry current = tracks[i];
				if (current == null) continue;

				events.Clear();

				TrackEntry previous = current.previous;
				if (previous == null)
					current.animation.Apply(skeleton, current.lastTime, current.time, current.loop, events);
				else {
					previous.animation.Apply(skeleton, int.MaxValue, previous.time, previous.loop, null);
					float alpha = current.mixTime / current.mixDuration;
					if (alpha >= 1) {
						alpha = 1;
						current.previous = null;
					}
					current.animation.Mix(skeleton, current.lastTime, current.time, current.loop, events, alpha);
				}

				for (int ii = 0, nn = events.Count; ii < nn; ii++) {
					Event e = events[ii];
					current.OnEvent(this, i, e);
					if (Event != null) Event(this, new EventTriggeredArgs(i, e));
				}

				current.lastTime = current.time;
			}
		}

		public void Clear () {
			for (int i = 0, n = tracks.Count; i < n; i++)
				Clear(i);
			tracks.Clear();
		}

		public void Clear (int trackIndex) {
			if (trackIndex >= tracks.Count) return;
			TrackEntry current = tracks[trackIndex];
			if (current == null) return;

			current.OnEnd(this, trackIndex);
			if (End != null) End(this, new StartEndArgs(trackIndex));

			tracks[trackIndex] = null;
		}

		private TrackEntry ExpandToIndex (int index) {
			if (index < tracks.Count) return tracks[index];
			while (index >= tracks.Count)
				tracks.Add(null);
			return null;
		}

		private void SetCurrent (int index, TrackEntry entry) {
			TrackEntry current = ExpandToIndex(index);
			if (current != null) {
				current.previous = null;

				current.OnEnd(this, index);
				if (End != null) End(this, new StartEndArgs(index));

				entry.mixDuration = data.GetMix(current.animation, entry.animation);
				if (entry.mixDuration > 0) {
					entry.mixTime = 0;
					entry.previous = current;
				}
			}

			tracks[index] = entry;

			entry.OnStart(this, index);
			if (Start != null) Start(this, new StartEndArgs(index));
		}

		public TrackEntry SetAnimation (int trackIndex, String animationName, bool loop) {
			Animation animation = data.skeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName);
			return SetAnimation(trackIndex, animation, loop);
		}

		/// <summary>
		/// Set the current animation. Any queued animations are cleared. 
		/// </summary>
		/// <param name="trackIndex"></param>
		/// <param name="animation"></param>
		/// <param name="loop"></param>
		/// <returns></returns>
		public TrackEntry SetAnimation (int trackIndex, Animation animation, bool loop) {
			TrackEntry entry = new TrackEntry();
			entry.animation = animation;
			entry.loop = loop;
			entry.time = 0;
			entry.endTime = animation.Duration;
			SetCurrent(trackIndex, entry);
			return entry;
		}

		public TrackEntry AddAnimation (int trackIndex, String animationName, bool loop, float delay) {
			Animation animation = data.skeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName);
			return AddAnimation(trackIndex, animation, loop, delay);
		}

		/// <summary>
		/// Adds an animation to be played delay seconds after the current or last queued animation.
		/// </summary>
		/// <param name="trackIndex"></param>
		/// <param name="animation"></param>
		/// <param name="loop"></param>
		/// <param name="delay"> May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay.</param>
		/// <returns></returns>
		public TrackEntry AddAnimation (int trackIndex, Animation animation, bool loop, float delay) {
			TrackEntry entry = new TrackEntry();
			entry.animation = animation;
			entry.loop = loop;
			entry.time = 0;
			entry.endTime = animation != null ? animation.Duration : 0;

			TrackEntry last = ExpandToIndex(trackIndex);
			if (last != null) {
				while (last.next != null)
					last = last.next;
				last.next = entry;
			} else
				tracks[trackIndex] = entry;

			if (delay <= 0) {
				if (last != null) {
					delay += last.endTime;
					if (animation != null) delay -= data.GetMix(last.animation, animation);
				} else
					delay = 0;
			}
			entry.delay = delay;

			return entry;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="trackIndex"></param>
		/// <returns> Current track entry, may be Null</returns>
		public TrackEntry GetCurrent (int trackIndex) {
			if (trackIndex >= tracks.Count) return null;
			return tracks[trackIndex];
		}

		override public String ToString () {
			StringBuilder buffer = new StringBuilder();
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry entry = tracks[i];
				if (entry == null) continue;
				if (buffer.Length > 0) buffer.Append(", ");
				buffer.Append(entry.ToString());
			}
			if (buffer.Length == 0) return "<none>";
			return buffer.ToString();
		}
	}

	public class EventTriggeredArgs : EventArgs {
		public int TrackIndex { get; private set; }
		public Event Event { get; private set; }

		public EventTriggeredArgs (int trackIndex, Event e) {
			TrackIndex = trackIndex;
			Event = e;
		}
	}

	public class CompleteArgs : EventArgs {
		public int TrackIndex { get; private set; }
		public int LoopCount { get; private set; }

		public CompleteArgs (int trackIndex, int loopCount) {
			TrackIndex = trackIndex;
			LoopCount = loopCount;
		}
	}

	public class StartEndArgs : EventArgs {
		public int TrackIndex { get; private set; }

		public StartEndArgs (int trackIndex) {
			TrackIndex = trackIndex;
		}
	}

	public class TrackEntry {
		internal TrackEntry next, previous;
		internal Animation animation;
		internal bool loop;
		internal float delay, time, lastTime, endTime;
		internal float mixTime, mixDuration;

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

		public event EventHandler<StartEndArgs> Start;
		public event EventHandler<StartEndArgs> End;
		public event EventHandler<EventTriggeredArgs> Event;
		public event EventHandler<CompleteArgs> Complete;

		internal void OnStart (AnimationState state, int index) {
			if (Start != null) Start(state, new StartEndArgs(index));
		}

		internal void OnEnd (AnimationState state, int index) {
			if (End != null) End(state, new StartEndArgs(index));
		}

		internal void OnEvent (AnimationState state, int index, Event e) {
			if (Event != null) Event(state, new EventTriggeredArgs(index, e));
		}

		internal void OnComplete (AnimationState state, int index, int loopCount) {
			if (Complete != null) Complete(state, new CompleteArgs(index, loopCount));
		}
	}
}
