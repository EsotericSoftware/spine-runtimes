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
	public class AnimationState {
		static readonly Animation EmptyAnimation = new Animation("<empty>", new ExposedList<Timeline>(), 0);
		internal const int Subsequent = 0, First = 1, Hold = 2, HoldMix = 3;

		protected AnimationStateData data;

		private readonly Pool<TrackEntry> trackEntryPool = new Pool<TrackEntry>();
		private readonly ExposedList<TrackEntry> tracks = new ExposedList<TrackEntry>();
		private readonly ExposedList<Event> events = new ExposedList<Event>();
		private readonly EventQueue queue; // Initialized by constructor.

		private readonly HashSet<int> propertyIDs = new HashSet<int>();
		private bool animationsChanged;

		private float timeScale = 1;

		public AnimationStateData Data { get { return data; } }

		/// <summary>A list of tracks that have animations, which may contain nulls.</summary>
		public ExposedList<TrackEntry> Tracks { get { return tracks; } }
		public float TimeScale { get { return timeScale; } set { timeScale = value; } }

		public delegate void TrackEntryDelegate (TrackEntry trackEntry);
		public TrackEntryDelegate Start { get; set; }
		public TrackEntryDelegate Interrupt { get; set; }
		public TrackEntryDelegate End { get; set; }
		public TrackEntryDelegate Dispose { get; set; }
		public TrackEntryDelegate Complete { get; set; }

		public delegate void TrackEntryEventDelegate (TrackEntry trackEntry, Event e);
		public TrackEntryEventDelegate Event { get; set; }

		public AnimationState(AnimationStateData data) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			this.data = data;
			this.queue = new EventQueue(
				this,
				delegate { this.animationsChanged = true; },
				trackEntryPool
			);
		}

		/// <summary>
		/// Increments the track entry trackTimes, setting queued animations as current if needed</summary>
		/// <param name="delta">delta time</param>
		public void Update (float delta) {
			delta *= timeScale;
			var tracksItems = tracks.Items;
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry current = tracksItems[i];
				if (current == null) continue;

				current.animationLast = current.nextAnimationLast;
				current.trackLast = current.nextTrackLast;

				float currentDelta = delta * current.timeScale;

				if (current.delay > 0) {
					current.delay -= currentDelta;
					if (current.delay > 0) continue;
					currentDelta = -current.delay;
					current.delay = 0;
				}

				TrackEntry next = current.next;
				if (next != null) {
					// When the next entry's delay is passed, change to the next entry, preserving leftover time.
					float nextTime = current.trackLast - next.delay;
					if (nextTime >= 0) {
						next.delay = 0;
						next.trackTime = current.timeScale == 0 ? 0 : (nextTime / current.timeScale + delta) * next.timeScale;
						current.trackTime += currentDelta;
						SetCurrent(i, next, true);
						while (next.mixingFrom != null) {
							next.mixTime += delta;
							next = next.mixingFrom;
						}
						continue;
					}
				} else if (current.trackLast >= current.trackEnd && current.mixingFrom == null) {
					// Clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
					tracksItems[i] = null;
					queue.End(current);
					DisposeNext(current);
					continue;
				}
				if (current.mixingFrom != null && UpdateMixingFrom(current, delta)) {
					// End mixing from entries once all have completed.
					var from = current.mixingFrom;
					current.mixingFrom = null;
					if (from != null) from.mixingTo = null;
					while (from != null) {
						queue.End(from);
						from = from.mixingFrom;
					}
				}

				current.trackTime += currentDelta;
			}

			queue.Drain();
		}

		/// <summary>Returns true when all mixing from entries are complete.</summary>
		private bool UpdateMixingFrom (TrackEntry to, float delta) {
			TrackEntry from = to.mixingFrom;
			if (from == null) return true;

			bool finished = UpdateMixingFrom(from, delta);

			from.animationLast = from.nextAnimationLast;
			from.trackLast = from.nextTrackLast;

			// Require mixTime > 0 to ensure the mixing from entry was applied at least once.
			if (to.mixTime > 0 && to.mixTime >= to.mixDuration) {
				// Require totalAlpha == 0 to ensure mixing is complete, unless mixDuration == 0 (the transition is a single frame).
				if (from.totalAlpha == 0 || to.mixDuration == 0) {
					to.mixingFrom = from.mixingFrom;
					if (from.mixingFrom != null) from.mixingFrom.mixingTo = to;
					to.interruptAlpha = from.interruptAlpha;
					queue.End(from);
				}
				return finished;
			}

			from.trackTime += delta * from.timeScale;
			to.mixTime += delta;
			return false;
		}

		/// <summary>
		/// Poses the skeleton using the track entry animations. There are no side effects other than invoking listeners, so the
		/// animation state can be applied to multiple skeletons to pose them identically.</summary>
		public bool Apply (Skeleton skeleton) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			if (animationsChanged) AnimationsChanged();

			var events = this.events;
			bool applied = false;
			var tracksItems = tracks.Items;
			for (int i = 0, m = tracks.Count; i < m; i++) {
				TrackEntry current = tracksItems[i];
				if (current == null || current.delay > 0) continue;
				applied = true;

				// Track 0 animations aren't for layering, so do not show the previously applied animations before the first key.
				MixBlend blend = i == 0 ? MixBlend.First : current.mixBlend;

				// Apply mixing from entries first.
				float mix = current.alpha;
				if (current.mixingFrom != null)
					mix *= ApplyMixingFrom(current, skeleton, blend);
				else if (current.trackTime >= current.trackEnd && current.next == null) //
					mix = 0; // Set to setup pose the last time the entry will be applied.

				// Apply current entry.
				float animationLast = current.animationLast, animationTime = current.AnimationTime;
				int timelineCount = current.animation.timelines.Count;
				var timelines = current.animation.timelines;
				var timelinesItems = timelines.Items;
				if (i == 0 && (mix == 1 || blend == MixBlend.Add)) {
					for (int ii = 0; ii < timelineCount; ii++)
						timelinesItems[ii].Apply(skeleton, animationLast, animationTime, events, mix, blend, MixDirection.In);
				} else {
					var timelineMode = current.timelineMode.Items;

					bool firstFrame = current.timelinesRotation.Count == 0;
					if (firstFrame) current.timelinesRotation.EnsureCapacity(timelines.Count << 1);
					var timelinesRotation = current.timelinesRotation.Items;

					for (int ii = 0; ii < timelineCount; ii++) {
						Timeline timeline = timelinesItems[ii];
						MixBlend timelineBlend = timelineMode[ii] >= AnimationState.Subsequent ? blend : MixBlend.Setup;
						var rotateTimeline = timeline as RotateTimeline;
						if (rotateTimeline != null)
							ApplyRotateTimeline(rotateTimeline, skeleton, animationTime, mix, timelineBlend, timelinesRotation, ii << 1, firstFrame);
						else
							timeline.Apply(skeleton, animationLast, animationTime, events, mix, timelineBlend, MixDirection.In);
					}
				}
				QueueEvents(current, animationTime);
				events.Clear(false);
				current.nextAnimationLast = animationTime;
				current.nextTrackLast = current.trackTime;
			}

			queue.Drain();
			return applied;
		}

		private float ApplyMixingFrom (TrackEntry to, Skeleton skeleton, MixBlend blend) {
			TrackEntry from = to.mixingFrom;
			if (from.mixingFrom != null) ApplyMixingFrom(from, skeleton, blend);

			float mix;
			if (to.mixDuration == 0) { // Single frame mix to undo mixingFrom changes.
				mix = 1;
				if (blend == MixBlend.First) blend = MixBlend.Setup; // Tracks > 0 are transparent and can't reset to setup pose.
			} else {
				mix = to.mixTime / to.mixDuration;
				if (mix > 1) mix = 1;
				if (blend != MixBlend.First) blend = from.mixBlend; // Track 0 ignores frack mix blend.
			}

			var eventBuffer = mix < from.eventThreshold ? this.events : null;
			bool attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;
			float animationLast = from.animationLast, animationTime = from.AnimationTime;
			var timelines = from.animation.timelines;
			int timelineCount = timelines.Count;
			var timelinesItems = timelines.Items;
			float alphaHold = from.alpha * to.interruptAlpha, alphaMix = alphaHold * (1 - mix);

			if (blend == MixBlend.Add) {
				for (int i = 0; i < timelineCount; i++)
					timelinesItems[i].Apply(skeleton, animationLast, animationTime, eventBuffer, alphaMix, blend, MixDirection.Out);
			} else {
				var timelineMode = from.timelineMode.Items;
				var timelineHoldMix = from.timelineHoldMix.Items;

				bool firstFrame = from.timelinesRotation.Count == 0;
				if (firstFrame)	from.timelinesRotation.Resize(timelines.Count << 1); // from.timelinesRotation.setSize
				var timelinesRotation = from.timelinesRotation.Items;

				from.totalAlpha = 0;
				for (int i = 0; i < timelineCount; i++) {
					Timeline timeline = timelinesItems[i];
					MixDirection direction = MixDirection.Out;
					MixBlend timelineBlend;
					float alpha;
					switch (timelineMode[i]) {
						case AnimationState.Subsequent:
							if (!attachments && timeline is AttachmentTimeline) continue;
							if (!drawOrder && timeline is DrawOrderTimeline) continue;
							timelineBlend = blend;
							alpha = alphaMix;
							break;
						case AnimationState.First:
							timelineBlend = MixBlend.Setup;
							alpha = alphaMix;
							break;
						case AnimationState.Hold:
							timelineBlend = MixBlend.Setup;
							alpha = alphaHold;
							break;
						default:
							timelineBlend = MixBlend.Setup;
							TrackEntry holdMix = timelineHoldMix[i];
							alpha = alphaHold * Math.Max(0, 1 - holdMix.mixTime / holdMix.mixDuration);
							break;
					}
					from.totalAlpha += alpha;

					var rotateTimeline = timeline as RotateTimeline;
					if (rotateTimeline != null) {
						ApplyRotateTimeline(rotateTimeline, skeleton, animationTime, alpha, timelineBlend, timelinesRotation, i << 1, firstFrame);
					} else {
						if (timelineBlend == MixBlend.Setup) {
							if (timeline is AttachmentTimeline) {
								if (attachments) direction = MixDirection.In;
							} else if (timeline is DrawOrderTimeline) {
								if (drawOrder) {
									direction = MixDirection.In;
								}
							}
						}

						timeline.Apply(skeleton, animationLast, animationTime, eventBuffer, alpha, timelineBlend, direction);
					}
				}
			}

			if (to.mixDuration > 0) QueueEvents(from, animationTime);
			this.events.Clear(false);
			from.nextAnimationLast = animationTime;
			from.nextTrackLast = from.trackTime;

			return mix;
		}

		static private void ApplyRotateTimeline (RotateTimeline rotateTimeline, Skeleton skeleton, float time, float alpha, MixBlend blend,
			float[] timelinesRotation, int i, bool firstFrame) {

			if (firstFrame) timelinesRotation[i] = 0;

			if (alpha == 1) {
				rotateTimeline.Apply(skeleton, 0, time, null, 1, blend, MixDirection.In);
				return;
			}

			Bone bone = skeleton.bones.Items[rotateTimeline.boneIndex];
			float[] frames = rotateTimeline.frames;
			float r1, r2;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
					case MixBlend.Setup:
						bone.rotation = bone.data.rotation;
						return;
					default:
						return;
					case MixBlend.First:
						r1 = bone.rotation;
						r2 = bone.data.rotation;
						break;
				}
			} else {
				r1 = blend == MixBlend.Setup ? bone.data.rotation : bone.rotation;
				if (time >= frames[frames.Length - RotateTimeline.ENTRIES]) // Time is after last frame.
					r2 = bone.data.rotation + frames[frames.Length + RotateTimeline.PREV_ROTATION];
				else {
					// Interpolate between the previous frame and the current frame.
					int frame = Animation.BinarySearch(frames, time, RotateTimeline.ENTRIES);
					float prevRotation = frames[frame + RotateTimeline.PREV_ROTATION];
					float frameTime = frames[frame];
					float percent = rotateTimeline.GetCurvePercent((frame >> 1) - 1,
						1 - (time - frameTime) / (frames[frame + RotateTimeline.PREV_TIME] - frameTime));

					r2 = frames[frame + RotateTimeline.ROTATION] - prevRotation;
					r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
					r2 = prevRotation + r2 * percent + bone.data.rotation;
					r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
				}
			}

			// Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
			float total, diff = r2 - r1;
			diff -= (16384 - (int)(16384.499999999996 - diff / 360)) * 360;
			if (diff == 0) {
				total = timelinesRotation[i];
			} else {
				float lastTotal, lastDiff;
				if (firstFrame) {
					lastTotal = 0;
					lastDiff = diff;
				} else {
					lastTotal = timelinesRotation[i]; // Angle and direction of mix, including loops.
					lastDiff = timelinesRotation[i + 1]; // Difference between bones.
				}
				bool current = diff > 0, dir = lastTotal >= 0;
				// Detect cross at 0 (not 180).
				if (Math.Sign(lastDiff) != Math.Sign(diff) && Math.Abs(lastDiff) <= 90) {
					// A cross after a 360 rotation is a loop.
					if (Math.Abs(lastTotal) > 180) lastTotal += 360 * Math.Sign(lastTotal);
					dir = current;
				}
				total = diff + lastTotal - lastTotal % 360; // Store loops as part of lastTotal.
				if (dir != current) total += 360 * Math.Sign(lastTotal);
				timelinesRotation[i] = total;
			}
			timelinesRotation[i + 1] = diff;
			r1 += total * alpha;
			bone.rotation = r1 - (16384 - (int)(16384.499999999996 - r1 / 360)) * 360;
		}

		private void QueueEvents (TrackEntry entry, float animationTime) {
			float animationStart = entry.animationStart, animationEnd = entry.animationEnd;
			float duration = animationEnd - animationStart;
			float trackLastWrapped = entry.trackLast % duration;

			// Queue events before complete.
			var events = this.events;
			var eventsItems = events.Items;
			int i = 0, n = events.Count;
			for (; i < n; i++) {
				var e = eventsItems[i];
				if (e.time < trackLastWrapped) break;
				if (e.time > animationEnd) continue; // Discard events outside animation start/end.
				queue.Event(entry, e);
			}

			// Queue complete if completed a loop iteration or the animation.
			bool complete = false;
			if (entry.loop)
				complete = duration == 0 || (trackLastWrapped > entry.trackTime % duration);
			else
				complete = animationTime >= animationEnd && entry.animationLast < animationEnd;
			if (complete) queue.Complete(entry);

			// Queue events after complete.
			for (; i < n; i++) {
				Event e = eventsItems[i];
				if (e.time < animationStart) continue; // Discard events outside animation start/end.
				queue.Event(entry, eventsItems[i]);
			}
		}

		/// <summary>
		/// Removes all animations from all tracks, leaving skeletons in their previous pose.
		/// It may be desired to use <see cref="AnimationState.SetEmptyAnimations(float)"/> to mix the skeletons back to the setup pose,
		/// rather than leaving them in their previous pose.</summary>
		public void ClearTracks () {
			bool oldDrainDisabled = queue.drainDisabled;
			queue.drainDisabled = true;
			for (int i = 0, n = tracks.Count; i < n; i++) {
				ClearTrack(i);
			}
			tracks.Clear();
			queue.drainDisabled = oldDrainDisabled;
			queue.Drain();
		}

		/// <summary>
		/// Removes all animations from the tracks, leaving skeletons in their previous pose.
		/// It may be desired to use <see cref="AnimationState.SetEmptyAnimations(float)"/> to mix the skeletons back to the setup pose,
		/// rather than leaving them in their previous pose.</summary>
		public void ClearTrack (int trackIndex) {
			if (trackIndex >= tracks.Count) return;
			TrackEntry current = tracks.Items[trackIndex];
			if (current == null) return;

			queue.End(current);

			DisposeNext(current);

			TrackEntry entry = current;
			while (true) {
				TrackEntry from = entry.mixingFrom;
				if (from == null) break;
				queue.End(from);
				entry.mixingFrom = null;
				entry.mixingTo = null;
				entry = from;
			}

			tracks.Items[current.trackIndex] = null;

			queue.Drain();
		}

		/// <summary>Sets the active TrackEntry for a given track number.</summary>
		private void SetCurrent (int index, TrackEntry current, bool interrupt) {
			TrackEntry from = ExpandToIndex(index);
			tracks.Items[index] = current;

			if (from != null) {
				if (interrupt) queue.Interrupt(from);
				current.mixingFrom = from;
				from.mixingTo = current;
				current.mixTime = 0;

				// Store interrupted mix percentage.
				if (from.mixingFrom != null && from.mixDuration > 0)
					current.interruptAlpha *= Math.Min(1, from.mixTime / from.mixDuration);

				from.timelinesRotation.Clear(); // Reset rotation for mixing out, in case entry was mixed in.
			}

			queue.Start(current); // triggers AnimationsChanged
		}


		/// <summary>Sets an animation by name. <seealso cref="SetAnimation(int, Animation, bool)" /></summary>
		public TrackEntry SetAnimation (int trackIndex, string animationName, bool loop) {
			Animation animation = data.skeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName, "animationName");
			return SetAnimation(trackIndex, animation, loop);
		}

		/// <summary>Sets the current animation for a track, discarding any queued animations.</summary>
		/// <param name="loop">If true, the animation will repeat.
		/// If false, it will not, instead its last frame is applied if played beyond its duration.
		/// In either case <see cref="TrackEntry.TrackEnd"/> determines when the track is cleared. </param>
		/// <returns>
		/// A track entry to allow further customization of animation playback. References to the track entry must not be kept
		/// after <see cref="AnimationState.Dispose"/>.</returns>
		public TrackEntry SetAnimation (int trackIndex, Animation animation, bool loop) {
			if (animation == null) throw new ArgumentNullException("animation", "animation cannot be null.");
			bool interrupt = true;
			TrackEntry current = ExpandToIndex(trackIndex);
			if (current != null) {
				if (current.nextTrackLast == -1) {
					// Don't mix from an entry that was never applied.
					tracks.Items[trackIndex] = current.mixingFrom;
					queue.Interrupt(current);
					queue.End(current);
					DisposeNext(current);
					current = current.mixingFrom;
					interrupt = false;
				} else {
					DisposeNext(current);
				}
			}
			TrackEntry entry = NewTrackEntry(trackIndex, animation, loop, current);
			SetCurrent(trackIndex, entry, interrupt);
			queue.Drain();
			return entry;
		}

		/// <summary>Queues an animation by name.</summary>
		/// <seealso cref="AddAnimation(int, Animation, bool, float)" />
		public TrackEntry AddAnimation (int trackIndex, string animationName, bool loop, float delay) {
			Animation animation = data.skeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName, "animationName");
			return AddAnimation(trackIndex, animation, loop, delay);
		}

		/// <summary>Adds an animation to be played delay seconds after the current or last queued animation
		/// for a track. If the track is empty, it is equivalent to calling <see cref="SetAnimation"/>.</summary>
		/// <param name="delay">
		/// delay Seconds to begin this animation after the start of the previous animation. If  &lt;= 0, uses the duration of the
		/// previous track entry minus any mix duration (from the <see cref="AnimationStateData"/>) plus the
		/// specified<code>delay</code> (ie the mix ends at (<code>delay</code> = 0) or before (<code>delay</code> < 0) the previous
		/// track entry duration). If the previous entry is looping, its next loop completion is used
		/// instead of the duration.
		/// </param>
		/// <returns>A track entry to allow further customization of animation playback. References to the track entry must not be kept
		/// after <see cref="AnimationState.Dispose"/></returns>
		public TrackEntry AddAnimation (int trackIndex, Animation animation, bool loop, float delay) {
			if (animation == null) throw new ArgumentNullException("animation", "animation cannot be null.");

			TrackEntry last = ExpandToIndex(trackIndex);
			if (last != null) {
				while (last.next != null)
					last = last.next;
			}

			TrackEntry entry = NewTrackEntry(trackIndex, animation, loop, last);

			if (last == null) {
				SetCurrent(trackIndex, entry, true);
				queue.Drain();
			} else {
				last.next = entry;
				if (delay <= 0) {
					float duration = last.animationEnd - last.animationStart;
					if (duration != 0) {
						if (last.loop) {
							delay += duration * (1 + (int)(last.trackTime / duration)); // Completion of next loop.
						} else {
							delay += Math.Max(duration, last.trackTime); // After duration, else next update.
						}
						delay -= data.GetMix(last.animation, animation);
					} else
						delay = last.trackTime; // Next update.
				}
			}

			entry.delay = delay;
			return entry;
		}

		/// <summary>
		/// Sets an empty animation for a track, discarding any queued animations, and mixes to it over the specified mix duration.</summary>
		public TrackEntry SetEmptyAnimation (int trackIndex, float mixDuration) {
			TrackEntry entry = SetAnimation(trackIndex, AnimationState.EmptyAnimation, false);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		}

		/// <summary>
		/// Adds an empty animation to be played after the current or last queued animation for a track, and mixes to it over the
		/// specified mix duration.</summary>
		/// <returns>
		/// A track entry to allow further customization of animation playback. References to the track entry must not be kept after <see cref="AnimationState.Dispose"/>.
		/// </returns>
		/// <param name="trackIndex">Track number.</param>
		/// <param name="mixDuration">Mix duration.</param>
		/// <param name="delay">Seconds to begin this animation after the start of the previous animation. May be &lt;= 0 to use the animation
		/// duration of the previous track minus any mix duration plus the negative delay.</param>
		public TrackEntry AddEmptyAnimation (int trackIndex, float mixDuration, float delay) {
			if (delay <= 0) delay -= mixDuration;
			TrackEntry entry = AddAnimation(trackIndex, AnimationState.EmptyAnimation, false, delay);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		}

		/// <summary>
		/// Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix duration.</summary>
		public void SetEmptyAnimations (float mixDuration) {
			bool oldDrainDisabled = queue.drainDisabled;
			queue.drainDisabled = true;
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry current = tracks.Items[i];
				if (current != null) SetEmptyAnimation(i, mixDuration);
			}
			queue.drainDisabled = oldDrainDisabled;
			queue.Drain();
		}

		private TrackEntry ExpandToIndex (int index) {
			if (index < tracks.Count) return tracks.Items[index];
			while (index >= tracks.Count)
				tracks.Add(null);
			return null;
		}

		/// <summary>Object-pooling version of new TrackEntry. Obtain an unused TrackEntry from the pool and clear/initialize its values.</summary>
		/// <param name="last">May be null.</param>
		private TrackEntry NewTrackEntry (int trackIndex, Animation animation, bool loop, TrackEntry last) {
			TrackEntry entry = trackEntryPool.Obtain(); // Pooling
			entry.trackIndex = trackIndex;
			entry.animation = animation;
			entry.loop = loop;
			entry.holdPrevious = false;

			entry.eventThreshold = 0;
			entry.attachmentThreshold = 0;
			entry.drawOrderThreshold = 0;

			entry.animationStart = 0;
			entry.animationEnd = animation.Duration;
			entry.animationLast = -1;
			entry.nextAnimationLast = -1;

			entry.delay = 0;
			entry.trackTime = 0;
			entry.trackLast = -1;
			entry.nextTrackLast = -1; // nextTrackLast == -1 signifies a TrackEntry that wasn't applied yet.
			entry.trackEnd = float.MaxValue; // loop ? float.MaxValue : animation.Duration;
			entry.timeScale = 1;

			entry.alpha = 1;
			entry.interruptAlpha = 1;
			entry.mixTime = 0;
			entry.mixDuration = (last == null) ? 0 : data.GetMix(last.animation, animation);
			return entry;
		}

		/// <summary>Dispose all track entries queued after the given TrackEntry.</summary>
		private void DisposeNext (TrackEntry entry) {
			TrackEntry next = entry.next;
			while (next != null) {
				queue.Dispose(next);
				next = next.next;
			}
			entry.next = null;
		}

		private void AnimationsChanged () {
			animationsChanged = false;

			propertyIDs.Clear();

			var tracksItems = tracks.Items;
			for (int i = 0, n = tracks.Count; i < n; i++) {
				var entry = tracksItems[i];
				if (entry == null) continue;
				// Move to last entry, then iterate in reverse (the order animations are applied).
				while (entry.mixingFrom != null)
					entry = entry.mixingFrom;

				do {
					if (entry.mixingTo == null || entry.mixBlend != MixBlend.Add) SetTimelineModes(entry);
					entry = entry.mixingTo;
				} while (entry != null);

			}
		}

		private void SetTimelineModes (TrackEntry entry) {
			TrackEntry to = entry.mixingTo;
			var timelines = entry.animation.timelines.Items;
			int timelinesCount = entry.animation.timelines.Count;
			var timelineMode = entry.timelineMode.Resize(timelinesCount).Items; //timelineMode.setSize(timelinesCount);
			entry.timelineHoldMix.Clear();
			var timelineHoldMix = entry.timelineHoldMix.Resize(timelinesCount).Items; //timelineHoldMix.setSize(timelinesCount);
			var propertyIDs = this.propertyIDs;

			if (to != null && to.holdPrevious) {
				for (int i = 0; i < timelinesCount; i++) {
					propertyIDs.Add(timelines[i].PropertyId);
					timelineMode[i] = AnimationState.Hold;
				}
				return;
			}

			// outer:
			for (int i = 0; i < timelinesCount; i++) {
				int id = timelines[i].PropertyId;
				if (!propertyIDs.Add(id))
					timelineMode[i] = AnimationState.Subsequent;
				else if (to == null || !HasTimeline(to, id))
					timelineMode[i] = AnimationState.First;
				else {
					for (TrackEntry next = to.mixingTo; next != null; next = next.mixingTo) {
						if (HasTimeline(next, id)) continue;
						if (next.mixDuration > 0) {
							timelineMode[i] = AnimationState.HoldMix;
							timelineHoldMix[i] = next;
							goto continue_outer; // continue outer;
						}
						break;
					}
					timelineMode[i] = AnimationState.Hold;
				}
				continue_outer: {}
			}
		}

		static bool HasTimeline (TrackEntry entry, int id) {
			var timelines = entry.animation.timelines.Items;
			for (int i = 0, n = entry.animation.timelines.Count; i < n; i++)
				if (timelines[i].PropertyId == id) return true;
			return false;
		}

		/// <returns>The track entry for the animation currently playing on the track, or null if no animation is currently playing.</returns>
		public TrackEntry GetCurrent (int trackIndex) {
			return (trackIndex >= tracks.Count) ? null : tracks.Items[trackIndex];
		}

		override public string ToString () {
			var buffer = new System.Text.StringBuilder();
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry entry = tracks.Items[i];
				if (entry == null) continue;
				if (buffer.Length > 0) buffer.Append(", ");
				buffer.Append(entry.ToString());
			}
			return buffer.Length == 0 ? "<none>" : buffer.ToString();
		}

		internal void OnStart (TrackEntry entry) { if (Start != null) Start(entry); }
		internal void OnInterrupt (TrackEntry entry) { if (Interrupt != null) Interrupt(entry); }
		internal void OnEnd (TrackEntry entry) { if (End != null) End(entry); }
		internal void OnDispose (TrackEntry entry) { if (Dispose != null) Dispose(entry); }
		internal void OnComplete (TrackEntry entry) { if (Complete != null) Complete(entry); }
		internal void OnEvent (TrackEntry entry, Event e) { if (Event != null) Event(entry, e); }
	}

	/// <summary>State for the playback of an animation.</summary>
	public class TrackEntry : Pool<TrackEntry>.IPoolable {
		internal Animation animation;

		internal TrackEntry next, mixingFrom, mixingTo;
		internal int trackIndex;

		internal bool loop, holdPrevious;
		internal float eventThreshold, attachmentThreshold, drawOrderThreshold;
		internal float animationStart, animationEnd, animationLast, nextAnimationLast;
		internal float delay, trackTime, trackLast, nextTrackLast, trackEnd, timeScale = 1f;
		internal float alpha, mixTime, mixDuration, interruptAlpha, totalAlpha;
		internal MixBlend mixBlend = MixBlend.Replace;
		internal readonly ExposedList<int> timelineMode = new ExposedList<int>();
		internal readonly ExposedList<TrackEntry> timelineHoldMix = new ExposedList<TrackEntry>();
		internal readonly ExposedList<float> timelinesRotation = new ExposedList<float>();

		// IPoolable.Reset()
		public void Reset () {
			next = null;
			mixingFrom = null;
			mixingTo = null;
			animation = null;
			timelineMode.Clear();
			timelineHoldMix.Clear();
			timelinesRotation.Clear();

			Start = null;
			Interrupt = null;
			End = null;
			Dispose = null;
			Complete = null;
			Event = null;
		}

		/// <summary>The index of the track where this entry is either current or queued.</summary>
		public int TrackIndex { get { return trackIndex; } }

		/// <summary>The animation to apply for this track entry.</summary>
		public Animation Animation { get { return animation; } }

		/// <summary>
		/// If true, the animation will repeat. If false, it will not, instead its last frame is applied if played beyond its duration.</summary>
		public bool Loop { get { return loop; } set { loop = value; } }

		///<summary>
		/// Seconds to postpone playing the animation. When a track entry is the current track entry, delay postpones incrementing
		/// the track time. When a track entry is queued, delay is the time from the start of the previous animation to when the
		/// track entry will become the current track entry. <see cref="TrackEntry.TimeScale"/> affects the delay.</summary>
		public float Delay { get { return delay; } set { delay = value; } }

		/// <summary>
		/// Current time in seconds this track entry has been the current track entry. The track time determines
		/// <see cref="TrackEntry.AnimationTime"/>. The track time can be set to start the animation at a time other than 0, without affecting looping.</summary>
		public float TrackTime { get { return trackTime; } set { trackTime = value; } }

		/// <summary>
		/// The track time in seconds when this animation will be removed from the track. Defaults to the animation duration for
		/// non-looping animations and to <see cref="int.MaxValue"/> for looping animations. If the track end time is reached and no
		/// other animations are queued for playback, and mixing from any previous animations is complete, properties keyed by the animation,
		/// are set to the setup pose and the track is cleared.
		///
		/// It may be desired to use <see cref="AnimationState.AddEmptyAnimation(int, float, float)"/> to mix the properties back to the
		/// setup pose over time, rather than have it happen instantly.
		/// </summary>
		public float TrackEnd { get { return trackEnd; } set { trackEnd = value; } }

		/// <summary>
		/// Seconds when this animation starts, both initially and after looping. Defaults to 0.
		///
		/// When changing the animation start time, it often makes sense to set <see cref="TrackEntry.AnimationLast"/> to the same value to
		/// prevent timeline keys before the start time from triggering.
		/// </summary>
		public float AnimationStart { get { return animationStart; } set { animationStart = value; } }

		/// <summary>
		/// Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animations will
		/// loop back to <see cref="TrackEntry.AnimationStart"/> at this time. Defaults to the animation duration.</summary>
		public float AnimationEnd { get { return animationEnd; } set { animationEnd = value; } }

		/// <summary>
		/// The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
		/// animation is applied, event timelines will fire all events between the animation last time (exclusive) and animation time
		/// (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this animation is applied.</summary>
		public float AnimationLast {
			get { return animationLast; }
			set {
				animationLast = value;
				nextAnimationLast = value;
			}
		}

		/// <summary>
		/// Uses <see cref="TrackEntry.TrackTime"/> to compute the animation time between <see cref="TrackEntry.AnimationStart"/>. and
		/// <see cref="TrackEntry.AnimationEnd"/>. When the track time is 0, the animation time is equal to the animation start time.
		/// </summary>
		public float AnimationTime {
			get {
				if (loop) {
					float duration = animationEnd - animationStart;
					if (duration == 0) return animationStart;
					return (trackTime % duration) + animationStart;
				}
				return Math.Min(trackTime + animationStart, animationEnd);
			}
		}

		/// <summary>
		/// Multiplier for the delta time when the animation state is updated, causing time for all animations and mixes to play slower or
		/// faster. Defaults to 1.
		///
		/// <see cref="TrackEntry.MixTime"/> is not affected by track entry time scale, so <see cref="TrackEntry.MixDuration"/> may need to
		// be adjusted to match the animation speed.
		///
		/// When using <see cref="AnimationState.AddAnimation(int, Animation, boolean, float)"> with a <code>delay</code> <= 0, note the
		/// {<see cref="TrackEntry.Delay"/> is set using the mix duration from the <see cref="AnimationStateData"/>, assuming time scale to be 1. If
		/// the time scale is not 1, the delay may need to be adjusted.
		/// </summary>
		public float TimeScale { get { return timeScale; } set { timeScale = value; } }

		/// <summary>
		/// Values less than 1 mix this animation with the last skeleton pose. Defaults to 1, which overwrites the last skeleton pose with
		/// this animation.
		///
		/// Typically track 0 is used to completely pose the skeleton, then alpha can be used on higher tracks. It doesn't make sense
		/// to use alpha on track 0 if the skeleton pose is from the last frame render.
		/// </summary>
		public float Alpha { get { return alpha; } set { alpha = value; } }

		/// <summary>
		/// When the mix percentage (mix time / mix duration) is less than the event threshold, event timelines for the animation
		/// being mixed out will be applied. Defaults to 0, so event timelines are not applied for an animation being mixed out.</summary>
		public float EventThreshold { get { return eventThreshold; } set { eventThreshold = value; } }

		/// <summary>
		/// When the mix percentage (mix time / mix duration) is less than the attachment threshold, attachment timelines for the
		/// animation being mixed out will be applied. Defaults to 0, so attachment timelines are not applied for an animation being
		/// mixed out.</summary>
		public float AttachmentThreshold { get { return attachmentThreshold; } set { attachmentThreshold = value; } }

		/// <summary>
		/// When the mix percentage (mix time / mix duration) is less than the draw order threshold, draw order timelines for the
		/// animation being mixed out will be applied. Defaults to 0, so draw order timelines are not applied for an animation being
		/// mixed out.
		/// </summary>
		public float DrawOrderThreshold { get { return drawOrderThreshold; } set { drawOrderThreshold = value; } }

		/// <summary>
		/// The animation queued to start after this animation, or null.</summary>
		public TrackEntry Next { get { return next; } }

		/// <summary>
		/// Returns true if at least one loop has been completed.</summary>
		public bool IsComplete {
			get { return trackTime >= animationEnd - animationStart; }
		}

		/// <summary>
		/// Seconds from 0 to the mix duration when mixing from the previous animation to this animation. May be slightly more than
		/// <see cref="TrackEntry.MixDuration"/> when the mix is complete.</summary>
		public float MixTime { get { return mixTime; } set { mixTime = value; } }

		/// <summary>
		/// Seconds for mixing from the previous animation to this animation. Defaults to the value provided by
		/// <see cref="AnimationStateData"/> based on the animation before this animation (if any).
		///
		/// The mix duration can be set manually rather than use the value from AnimationStateData.GetMix.
		/// In that case, the mixDuration must be set before <see cref="AnimationState.Update(float)"/> is next called.
		/// <para>
		/// When using <seealso cref="AnimationState.AddAnimation(int, Animation, bool, float)"/> with a
		/// <code>delay</code> less than or equal to 0, note the <seealso cref="Delay"/> is set using the mix duration from the <see cref=" AnimationStateData"/>,
		/// not a mix duration set afterward.
		/// </para>
		///
		/// </summary>
		public float MixDuration { get { return mixDuration; } set { mixDuration = value; } }

		/// <summary>
		/// Controls how properties keyed in the animation are mixed with lower tracks. Defaults to {@link MixBlend#replace}, which
		/// replaces the values from the lower tracks with the animation values. {@link MixBlend#add} adds the animation values to
		/// the values from the lower tracks.
		/// <para>
		/// The<code> mixBlend</code> can be set for a new track entry only before
		/// <code>AnimationState.Apply(Skeleton)</code> is first called.
		/// </para>
		/// </summary>
		public MixBlend MixBlend { get { return mixBlend; } set { mixBlend = value; } }

		/// <summary>
		/// The track entry for the previous animation when mixing from the previous animation to this animation, or null if no
		/// mixing is currently occuring. When mixing from multiple animations, MixingFrom makes up a linked list.</summary>
		public TrackEntry MixingFrom { get { return mixingFrom; } }

		/// <summary>
		/// If true, when mixing from the previous animation to this animation, the previous animation is applied as normal instead of being mixed out.
		///
		/// When mixing between animations that key the same property, if a lower track also keys that property then the value will briefly dip toward the lower track value during the mix. This happens because the first animation mixes from 100% to 0% while the second animation mixes from 0% to 100%. Setting HoldPrevious to true applies the first animation at 100% during the mix so the lower track value is overwritten. Such dipping does not occur on the lowest track which keys the property, only when a higher track also keys the property.
		///
		/// Snapping will occur if HoldPrevious is true and this animation does not key all the same properties as the previous animation.
		/// </summary>
		public bool HoldPrevious { get { return holdPrevious; } set { holdPrevious = value; } }

		public event AnimationState.TrackEntryDelegate Start, Interrupt, End, Dispose, Complete;
		public event AnimationState.TrackEntryEventDelegate Event;
		internal void OnStart () { if (Start != null) Start(this); }
		internal void OnInterrupt () { if (Interrupt != null) Interrupt(this); }
		internal void OnEnd () { if (End != null) End(this); }
		internal void OnDispose () { if (Dispose != null) Dispose(this); }
		internal void OnComplete () { if (Complete != null) Complete(this); }
		internal void OnEvent (Event e) { if (Event != null) Event(this, e); }

		/// <summary>
		/// <para>
		/// Resets the rotation directions for mixing this entry's rotate timelines. This can be useful to avoid bones rotating the
		/// long way around when using <see cref="alpha"/> and starting animations on other tracks.
		/// </para>
		/// <para>
		/// Mixing with <see cref="MixBlend.Replace"/> involves finding a rotation between two others, which has two possible solutions:
		/// the short way or the long way around.The two rotations likely change over time, so which direction is the short or long
		/// way also changes.If the short way was always chosen, bones would flip to the other side when that direction became the
		/// long way. TrackEntry chooses the short way the first time it is applied and remembers that direction.
		/// </para>
		/// </summary>
		public void ResetRotationDirections () {
			timelinesRotation.Clear();
		}

		override public string ToString () {
			return animation == null ? "<none>" : animation.name;
		}
	}

	class EventQueue {
		private readonly List<EventQueueEntry> eventQueueEntries = new List<EventQueueEntry>();
		internal bool drainDisabled;

		private readonly AnimationState state;
		private readonly Pool<TrackEntry> trackEntryPool;
		internal event Action AnimationsChanged;

		internal EventQueue (AnimationState state, Action HandleAnimationsChanged, Pool<TrackEntry> trackEntryPool) {
			this.state = state;
			this.AnimationsChanged += HandleAnimationsChanged;
			this.trackEntryPool = trackEntryPool;
		}

		struct EventQueueEntry {
			public EventType type;
			public TrackEntry entry;
			public Event e;

			public EventQueueEntry (EventType eventType, TrackEntry trackEntry, Event e = null) {
				this.type = eventType;
				this.entry = trackEntry;
				this.e = e;
			}
		}

		enum EventType {
			Start, Interrupt, End, Dispose, Complete, Event
		}

		internal void Start (TrackEntry entry) {
			eventQueueEntries.Add(new EventQueueEntry(EventType.Start, entry));
			if (AnimationsChanged != null) AnimationsChanged();
		}

		internal void Interrupt (TrackEntry entry) {
			eventQueueEntries.Add(new EventQueueEntry(EventType.Interrupt, entry));
		}

		internal void End (TrackEntry entry) {
			eventQueueEntries.Add(new EventQueueEntry(EventType.End, entry));
			if (AnimationsChanged != null) AnimationsChanged();
		}

		internal void Dispose (TrackEntry entry) {
			eventQueueEntries.Add(new EventQueueEntry(EventType.Dispose, entry));
		}

		internal void Complete (TrackEntry entry) {
			eventQueueEntries.Add(new EventQueueEntry(EventType.Complete, entry));
		}

		internal void Event (TrackEntry entry, Event e) {
			eventQueueEntries.Add(new EventQueueEntry(EventType.Event, entry, e));
		}

		/// <summary>Raises all events in the queue and drains the queue.</summary>
		internal void Drain () {
			if (drainDisabled) return;
			drainDisabled = true;

			var entries = this.eventQueueEntries;
			AnimationState state = this.state;

			// Don't cache entries.Count so callbacks can queue their own events (eg, call SetAnimation in AnimationState_Complete).
			for (int i = 0; i < entries.Count; i++) {
				var queueEntry = entries[i];
				TrackEntry trackEntry = queueEntry.entry;

				switch (queueEntry.type) {
				case EventType.Start:
					trackEntry.OnStart();
					state.OnStart(trackEntry);
					break;
				case EventType.Interrupt:
					trackEntry.OnInterrupt();
					state.OnInterrupt(trackEntry);
					break;
				case EventType.End:
					trackEntry.OnEnd();
					state.OnEnd(trackEntry);
					goto case EventType.Dispose; // Fall through. (C#)
				case EventType.Dispose:
					trackEntry.OnDispose();
					state.OnDispose(trackEntry);
					trackEntryPool.Free(trackEntry); // Pooling
					break;
				case EventType.Complete:
					trackEntry.OnComplete();
					state.OnComplete(trackEntry);
					break;
				case EventType.Event:
					trackEntry.OnEvent(queueEntry.e);
					state.OnEvent(trackEntry, queueEntry.e);
					break;
				}
			}
			eventQueueEntries.Clear();

			drainDisabled = false;
		}

		internal void Clear () {
			eventQueueEntries.Clear();
		}
	}

	public class Pool<T> where T : class, new() {
		public readonly int max;
		readonly Stack<T> freeObjects;

		public int Count { get { return freeObjects.Count; } }
		public int Peak { get; private set; }

		public Pool (int initialCapacity = 16, int max = int.MaxValue) {
			freeObjects = new Stack<T>(initialCapacity);
			this.max = max;
		}

		public T Obtain () {
			return freeObjects.Count == 0 ? new T() : freeObjects.Pop();
		}

		public void Free (T obj) {
			if (obj == null) throw new ArgumentNullException("obj", "obj cannot be null");
			if (freeObjects.Count < max) {
				freeObjects.Push(obj);
				Peak = Math.Max(Peak, freeObjects.Count);
			}
			Reset(obj);
		}

//		protected void FreeAll (List<T> objects) {
//			if (objects == null) throw new ArgumentNullException("objects", "objects cannot be null.");
//			var freeObjects = this.freeObjects;
//			int max = this.max;
//			for (int i = 0; i < objects.Count; i++) {
//				T obj = objects[i];
//				if (obj == null) continue;
//				if (freeObjects.Count < max) freeObjects.Push(obj);
//				Reset(obj);
//			}
//			Peak = Math.Max(Peak, freeObjects.Count);
//		}

		public void Clear () {
			freeObjects.Clear();
		}

		protected void Reset (T obj) {
			var poolable = obj as IPoolable;
			if (poolable != null) poolable.Reset();
		}

		public interface IPoolable {
			void Reset ();
		}
	}

}
