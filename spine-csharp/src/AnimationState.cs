/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
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
		private float timeScale = 1;

		public AnimationStateData Data { get { return data; } }
		public float TimeScale { get { return timeScale; } set { timeScale = value; } }

		public event EventHandler<StartEndArgs> Start;
		public event EventHandler<StartEndArgs> End;
		public event EventHandler<EventTriggeredArgs> Event;
		public event EventHandler<CompleteArgs> Complete;

		public AnimationState (AnimationStateData data) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			this.data = data;
		}

		public void Update (float delta) {
			delta *= timeScale;
			for (int i = 0; i < tracks.Count; i++) {
				TrackEntry current = tracks[i];
				if (current == null) continue;

				float trackDelta = delta * current.timeScale;
				float time = current.time + trackDelta;
				float endTime = current.endTime;

				current.time = time;
				if (current.previous != null) {
					current.previous.time += trackDelta;
					current.mixTime += trackDelta;
				}

				// Check if completed the animation or a loop iteration.
				if (current.loop ? (current.lastTime % endTime > time % endTime) : (current.lastTime < endTime && time >= endTime)) {
					int count = (int)(time / endTime);
					current.OnComplete(this, i, count);
					if (Complete != null) Complete(this, new CompleteArgs(i, count));
				}

				TrackEntry next = current.next;
				if (next != null) {
					if (time - trackDelta >= next.delay) SetCurrent(i, next);
				} else {
					// End non-looping animation when it reaches its end time and there is no next entry.
					if (!current.loop && current.lastTime >= current.endTime) ClearTrack(i);
				}
			}
		}

		public void Apply (Skeleton skeleton) {
			List<Event> events = this.events;

			for (int i = 0; i < tracks.Count; i++) {
				TrackEntry current = tracks[i];
				if (current == null) continue;

				events.Clear();

				float time = current.time;
				bool loop = current.loop;
				if (!loop && time > current.endTime) time = current.endTime;

				TrackEntry previous = current.previous;
				if (previous == null)
					current.animation.Apply(skeleton, current.lastTime, time, loop, events);
				else {
					float previousTime = previous.time;
					if (!previous.loop && previousTime > previous.endTime) previousTime = previous.endTime;
					previous.animation.Apply(skeleton, previousTime, previousTime, previous.loop, null);

					float alpha = current.mixTime / current.mixDuration;
					if (alpha >= 1) {
						alpha = 1;
						current.previous = null;
					}
					current.animation.Mix(skeleton, current.lastTime, time, loop, events, alpha);
				}

				for (int ii = 0, nn = events.Count; ii < nn; ii++) {
					Event e = events[ii];
					current.OnEvent(this, i, e);
					if (Event != null) Event(this, new EventTriggeredArgs(i, e));
				}

				current.lastTime = current.time;
			}
		}

		public void ClearTracks () {
			for (int i = 0, n = tracks.Count; i < n; i++)
				ClearTrack(i);
			tracks.Clear();
		}

		public void ClearTrack (int trackIndex) {
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
				TrackEntry previous = current.previous;
				current.previous = null;

				current.OnEnd(this, index);
				if (End != null) End(this, new StartEndArgs(index));

				entry.mixDuration = data.GetMix(current.animation, entry.animation);
				if (entry.mixDuration > 0) {
					entry.mixTime = 0;
					// If a mix is in progress, mix from the closest animation.
					if (previous != null && current.mixTime / current.mixDuration < 0.5f)
						entry.previous = previous;
					else
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

		/// <summary>Set the current animation. Any queued animations are cleared.</summary>
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

		/// <summary>Adds an animation to be played delay seconds after the current or last queued animation.</summary>
		/// <param name="delay">May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay.</param>
		public TrackEntry AddAnimation (int trackIndex, Animation animation, bool loop, float delay) {
			TrackEntry entry = new TrackEntry();
			entry.animation = animation;
			entry.loop = loop;
			entry.time = 0;
			entry.endTime = animation.Duration;

			TrackEntry last = ExpandToIndex(trackIndex);
			if (last != null) {
				while (last.next != null)
					last = last.next;
				last.next = entry;
			} else
				tracks[trackIndex] = entry;

			if (delay <= 0) {
				if (last != null)
					delay += last.endTime - data.GetMix(last.animation, animation);
				else
					delay = 0;
			}
			entry.delay = delay;

			return entry;
		}

		/// <returns>May be null.</returns>
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
		internal float delay, time, lastTime = -1, endTime, timeScale = 1;
		internal float mixTime, mixDuration;

		public Animation Animation { get { return animation; } }
		public float Delay { get { return delay; } set { delay = value; } }
		public float Time { get { return time; } set { time = value; } }
		public float LastTime { get { return lastTime; } set { lastTime = value; } }
		public float EndTime { get { return endTime; } set { endTime = value; } }
		public float TimeScale { get { return timeScale; } set { timeScale = value; } }
		public bool Loop { get { return loop; } set { loop = value; } }

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

		override public String ToString () {
			return animation == null ? "<none>" : animation.name;
		}
	}
}
