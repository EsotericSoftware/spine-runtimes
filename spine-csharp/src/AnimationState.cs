/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections.Generic;

namespace Spine {

	/// <summary>
	/// <para>
	/// Applies animations over time, queues animations for later playback, mixes (crossfading) between animations, and applies
	/// multiple animations on top of each other (layering).</para>
	/// <para>
	/// See <a href='http://esotericsoftware.com/spine-applying-animations/'>Applying Animations</a> in the Spine Runtimes Guide.</para>
	/// </summary>
	public class AnimationState {
		internal static readonly Animation EmptyAnimation = new Animation("<empty>", new ExposedList<Timeline>(), 0);

		/// 1) A previously applied timeline has set this property.<para />
		/// Result: Mix from the current pose to the timeline pose.
		internal const int Subsequent = 0;
		/// 1) This is the first timeline to set this property.<para />
		/// 2) The next track entry applied after this one does not have a timeline to set this property.<para />
		/// Result: Mix from the setup pose to the timeline pose.
		internal const int First = 1;
		/// 1) A previously applied timeline has set this property.<br>
		/// 2) The next track entry to be applied does have a timeline to set this property.<br>
		/// 3) The next track entry after that one does not have a timeline to set this property.<br>
		/// Result: Mix from the current pose to the timeline pose, but do not mix out. This avoids "dipping" when crossfading
		/// animations that key the same property. A subsequent timeline will set this property using a mix.
		internal const int HoldSubsequent = 2;
		/// 1) This is the first timeline to set this property.<para />
		/// 2) The next track entry to be applied does have a timeline to set this property.<para />
		/// 3) The next track entry after that one does not have a timeline to set this property.<para />
		/// Result: Mix from the setup pose to the timeline pose, but do not mix out. This avoids "dipping" when crossfading animations
		/// that key the same property. A subsequent timeline will set this property using a mix.
		internal const int HoldFirst = 3;
		/// 1) This is the first timeline to set this property.<para />
		/// 2) The next track entry to be applied does have a timeline to set this property.<para />
		/// 3) The next track entry after that one does have a timeline to set this property.<para />
		/// 4) timelineHoldMix stores the first subsequent track entry that does not have a timeline to set this property.<para />
		/// Result: The same as HOLD except the mix percentage from the timelineHoldMix track entry is used. This handles when more than
		/// 2 track entries in a row have a timeline that sets the same property.<para />
		/// Eg, A -> B -> C -> D where A, B, and C have a timeline setting same property, but D does not. When A is applied, to avoid
		/// "dipping" A is not mixed out, however D (the first entry that doesn't set the property) mixing in is used to mix out A
		/// (which affects B and C). Without using D to mix out, A would be applied fully until mixing completes, then snap to the mixed
		/// out position.
		internal const int HoldMix = 4;

		internal const int Setup = 1, Current = 2;

		protected AnimationStateData data;
		private readonly ExposedList<TrackEntry> tracks = new ExposedList<TrackEntry>();
		private readonly ExposedList<Event> events = new ExposedList<Event>();
		// difference to libgdx reference: delegates are used for event callbacks instead of 'final SnapshotArray<AnimationStateListener> listeners'.
		internal void OnStart (TrackEntry entry) { if (Start != null) Start(entry); }
		internal void OnInterrupt (TrackEntry entry) { if (Interrupt != null) Interrupt(entry); }
		internal void OnEnd (TrackEntry entry) { if (End != null) End(entry); }
		internal void OnDispose (TrackEntry entry) { if (Dispose != null) Dispose(entry); }
		internal void OnComplete (TrackEntry entry) { if (Complete != null) Complete(entry); }
		internal void OnEvent (TrackEntry entry, Event e) { if (Event != null) Event(entry, e); }

		public delegate void TrackEntryDelegate (TrackEntry trackEntry);
		/// <summary>See <see href="http://esotericsoftware.com/spine-api-reference#AnimationStateListener-Methods">
		/// API Reference documentation pages here</see> for details. Usage in C# and spine-unity is explained
		/// <see href="http://esotericsoftware.com/spine-unity#Processing-AnimationState-Events">here</see>
		/// on the spine-unity documentation pages.</summary>
		public event TrackEntryDelegate Start, Interrupt, End, Dispose, Complete;

		public delegate void TrackEntryEventDelegate (TrackEntry trackEntry, Event e);
		public event TrackEntryEventDelegate Event;

		public void AssignEventSubscribersFrom (AnimationState src) {
			Event = src.Event;
			Start = src.Start;
			Interrupt = src.Interrupt;
			End = src.End;
			Dispose = src.Dispose;
			Complete = src.Complete;
		}

		public void AddEventSubscribersFrom (AnimationState src) {
			Event += src.Event;
			Start += src.Start;
			Interrupt += src.Interrupt;
			End += src.End;
			Dispose += src.Dispose;
			Complete += src.Complete;
		}

		// end of difference
		private readonly EventQueue queue; // Initialized by constructor.
		private readonly HashSet<string> propertyIds = new HashSet<string>();
		private bool animationsChanged;
		private float timeScale = 1;
		private int unkeyedState;

		private readonly Pool<TrackEntry> trackEntryPool = new Pool<TrackEntry>();

		public AnimationState (AnimationStateData data) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			this.data = data;
			this.queue = new EventQueue(
				this,
				delegate { this.animationsChanged = true; },
				trackEntryPool
			);
		}

		/// <summary>
		/// Increments the track entry <see cref="TrackEntry.TrackTime"/>, setting queued animations as current if needed.</summary>
		/// <param name="delta">delta time</param>
		public void Update (float delta) {
			delta *= timeScale;
			TrackEntry[] tracksItems = tracks.Items;
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
						next.trackTime += current.timeScale == 0 ? 0 : (nextTime / current.timeScale + delta) * next.timeScale;
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
					ClearNext(current);
					continue;
				}
				if (current.mixingFrom != null && UpdateMixingFrom(current, delta)) {
					// End mixing from entries once all have completed.
					TrackEntry from = current.mixingFrom;
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
		/// Poses the skeleton using the track entry animations.  The animation state is not changed, so can be applied to multiple
		/// skeletons to pose them identically.</summary>
		/// <returns>True if any animations were applied.</returns>
		public bool Apply (Skeleton skeleton) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			if (animationsChanged) AnimationsChanged();

			ExposedList<Event> events = this.events;
			bool applied = false;
			TrackEntry[] tracksItems = tracks.Items;
			for (int i = 0, n = tracks.Count; i < n; i++) {
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
				float animationLast = current.animationLast, animationTime = current.AnimationTime, applyTime = animationTime;
				ExposedList<Event> applyEvents = events;
				if (current.reverse) {
					applyTime = current.animation.duration - applyTime;
					applyEvents = null;
				}

				int timelineCount = current.animation.timelines.Count;
				Timeline[] timelines = current.animation.timelines.Items;
				if ((i == 0 && mix == 1) || blend == MixBlend.Add) {
					for (int ii = 0; ii < timelineCount; ii++) {
						Timeline timeline = timelines[ii];
						if (timeline is AttachmentTimeline)
							ApplyAttachmentTimeline((AttachmentTimeline)timeline, skeleton, applyTime, blend, true);
						else
							timeline.Apply(skeleton, animationLast, applyTime, applyEvents, mix, blend, MixDirection.In);
					}
				} else {
					int[] timelineMode = current.timelineMode.Items;

					bool shortestRotation = current.shortestRotation;
					bool firstFrame = !shortestRotation && current.timelinesRotation.Count != timelineCount << 1;
					if (firstFrame) current.timelinesRotation.Resize(timelineCount << 1);
					float[] timelinesRotation = current.timelinesRotation.Items;

					for (int ii = 0; ii < timelineCount; ii++) {
						Timeline timeline = timelines[ii];
						MixBlend timelineBlend = timelineMode[ii] == AnimationState.Subsequent ? blend : MixBlend.Setup;
						var rotateTimeline = timeline as RotateTimeline;
						if (!shortestRotation && rotateTimeline != null)
							ApplyRotateTimeline(rotateTimeline, skeleton, applyTime, mix, timelineBlend, timelinesRotation,
												ii << 1, firstFrame);
						else if (timeline is AttachmentTimeline)
							ApplyAttachmentTimeline((AttachmentTimeline)timeline, skeleton, applyTime, blend, true);
						else
							timeline.Apply(skeleton, animationLast, applyTime, applyEvents, mix, timelineBlend, MixDirection.In);
					}
				}
				QueueEvents(current, animationTime);
				events.Clear(false);
				current.nextAnimationLast = animationTime;
				current.nextTrackLast = current.trackTime;
			}

			// Set slots attachments to the setup pose, if needed. This occurs if an animation that is mixing out sets attachments so
			// subsequent timelines see any deform, but the subsequent timelines don't set an attachment (eg they are also mixing out or
			// the time is before the first key).
			int setupState = unkeyedState + Setup;
			Slot[] slots = skeleton.slots.Items;
			for (int i = 0, n = skeleton.slots.Count; i < n; i++) {
				Slot slot = slots[i];
				if (slot.attachmentState == setupState) {
					string attachmentName = slot.data.attachmentName;
					slot.Attachment = (attachmentName == null ? null : skeleton.GetAttachment(slot.data.index, attachmentName));
				}
			}
			unkeyedState += 2; // Increasing after each use avoids the need to reset attachmentState for every slot.

			queue.Drain();
			return applied;
		}

		/// <summary>Version of <see cref="Apply"/> only applying and updating time at
		/// EventTimelines for lightweight off-screen updates.</summary>
		/// <param name="issueEvents">When set to false, only animation times of TrackEntries are updated.</param>
		// Note: This method is not part of the libgdx reference implementation.
		public bool ApplyEventTimelinesOnly (Skeleton skeleton, bool issueEvents = true) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");

			ExposedList<Event> events = this.events;
			bool applied = false;
			TrackEntry[] tracksItems = tracks.Items;
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry current = tracksItems[i];
				if (current == null || current.delay > 0) continue;
				applied = true;

				// Apply mixing from entries first.
				if (current.mixingFrom != null) ApplyMixingFromEventTimelinesOnly(current, skeleton, issueEvents);

				// Apply current entry.
				float animationLast = current.animationLast, animationTime = current.AnimationTime;

				if (issueEvents) {
					int timelineCount = current.animation.timelines.Count;
					Timeline[] timelines = current.animation.timelines.Items;
					for (int ii = 0; ii < timelineCount; ii++) {
						Timeline timeline = timelines[ii];
						if (timeline is EventTimeline)
							timeline.Apply(skeleton, animationLast, animationTime, events, 1.0f, MixBlend.Setup, MixDirection.In);
					}
					QueueEvents(current, animationTime);
					events.Clear(false);
				}
				current.nextAnimationLast = animationTime;
				current.nextTrackLast = current.trackTime;
			}

			if (issueEvents)
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
				if (blend != MixBlend.First) blend = from.mixBlend; // Track 0 ignores track mix blend.
			}

			bool attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;
			int timelineCount = from.animation.timelines.Count;
			Timeline[] timelines = from.animation.timelines.Items;
			float alphaHold = from.alpha * to.interruptAlpha, alphaMix = alphaHold * (1 - mix);
			float animationLast = from.animationLast, animationTime = from.AnimationTime, applyTime = animationTime;
			ExposedList<Event> events = null;
			if (from.reverse)
				applyTime = from.animation.duration - applyTime;
			else {
				if (mix < from.eventThreshold) events = this.events;
			}

			if (blend == MixBlend.Add) {
				for (int i = 0; i < timelineCount; i++)
					timelines[i].Apply(skeleton, animationLast, applyTime, events, alphaMix, blend, MixDirection.Out);
			} else {
				int[] timelineMode = from.timelineMode.Items;
				TrackEntry[] timelineHoldMix = from.timelineHoldMix.Items;

				bool shortestRotation = from.shortestRotation;
				bool firstFrame = !shortestRotation && from.timelinesRotation.Count != timelineCount << 1;
				if (firstFrame) from.timelinesRotation.Resize(timelineCount << 1);
				float[] timelinesRotation = from.timelinesRotation.Items;

				from.totalAlpha = 0;
				for (int i = 0; i < timelineCount; i++) {
					Timeline timeline = timelines[i];
					MixDirection direction = MixDirection.Out;
					MixBlend timelineBlend;
					float alpha;
					switch (timelineMode[i]) {
					case AnimationState.Subsequent:
						if (!drawOrder && timeline is DrawOrderTimeline) continue;
						timelineBlend = blend;
						alpha = alphaMix;
						break;
					case AnimationState.First:
						timelineBlend = MixBlend.Setup;
						alpha = alphaMix;
						break;
					case AnimationState.HoldSubsequent:
						timelineBlend = blend;
						alpha = alphaHold;
						break;
					case AnimationState.HoldFirst:
						timelineBlend = MixBlend.Setup;
						alpha = alphaHold;
						break;
					default: // HoldMix
						timelineBlend = MixBlend.Setup;
						TrackEntry holdMix = timelineHoldMix[i];
						alpha = alphaHold * Math.Max(0, 1 - holdMix.mixTime / holdMix.mixDuration);
						break;
					}
					from.totalAlpha += alpha;
					var rotateTimeline = timeline as RotateTimeline;
					if (!shortestRotation && rotateTimeline != null) {
						ApplyRotateTimeline(rotateTimeline, skeleton, applyTime, alpha, timelineBlend, timelinesRotation, i << 1,
							firstFrame);
					} else if (timeline is AttachmentTimeline) {
						ApplyAttachmentTimeline((AttachmentTimeline)timeline, skeleton, applyTime, timelineBlend, attachments);
					} else {
						if (drawOrder && timeline is DrawOrderTimeline && timelineBlend == MixBlend.Setup)
							direction = MixDirection.In;
						timeline.Apply(skeleton, animationLast, applyTime, events, alpha, timelineBlend, direction);
					}
				}
			}

			if (to.mixDuration > 0) QueueEvents(from, animationTime);
			this.events.Clear(false);
			from.nextAnimationLast = animationTime;
			from.nextTrackLast = from.trackTime;

			return mix;
		}

		/// <summary>Version of <see cref="ApplyMixingFrom"/> only applying and updating time at
		/// EventTimelines for lightweight off-screen updates.</summary>
		/// <param name="issueEvents">When set to false, only animation times of TrackEntries are updated.</param>
		// Note: This method is not part of the libgdx reference implementation.
		private float ApplyMixingFromEventTimelinesOnly (TrackEntry to, Skeleton skeleton, bool issueEvents) {
			TrackEntry from = to.mixingFrom;
			if (from.mixingFrom != null) ApplyMixingFromEventTimelinesOnly(from, skeleton, issueEvents);


			float mix;
			if (to.mixDuration == 0) { // Single frame mix to undo mixingFrom changes.
				mix = 1;
			} else {
				mix = to.mixTime / to.mixDuration;
				if (mix > 1) mix = 1;
			}

			ExposedList<Event> eventBuffer = mix < from.eventThreshold ? this.events : null;
			if (eventBuffer == null) return mix;

			float animationLast = from.animationLast, animationTime = from.AnimationTime;
			if (issueEvents) {
				int timelineCount = from.animation.timelines.Count;
				Timeline[] timelines = from.animation.timelines.Items;
				for (int i = 0; i < timelineCount; i++) {
					Timeline timeline = timelines[i];
					if (timeline is EventTimeline)
						timeline.Apply(skeleton, animationLast, animationTime, eventBuffer, 0, MixBlend.Setup, MixDirection.Out);
				}

				if (to.mixDuration > 0) QueueEvents(from, animationTime);
				this.events.Clear(false);
			}
			from.nextAnimationLast = animationTime;
			from.nextTrackLast = from.trackTime;

			return mix;
		}

		/// <summary> Applies the attachment timeline and sets <see cref="Slot.attachmentState"/>.</summary>
		/// <param name="attachments">False when: 1) the attachment timeline is mixing out, 2) mix < attachmentThreshold, and 3) the timeline
		/// is not the last timeline to set the slot's attachment. In that case the timeline is applied only so subsequent
		/// timelines see any deform.</param>
		private void ApplyAttachmentTimeline (AttachmentTimeline timeline, Skeleton skeleton, float time, MixBlend blend,
			bool attachments) {

			Slot slot = skeleton.slots.Items[timeline.SlotIndex];
			if (!slot.bone.active) return;

			float[] frames = timeline.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (blend == MixBlend.Setup || blend == MixBlend.First)
					SetAttachment(skeleton, slot, slot.data.attachmentName, attachments);
			} else
				SetAttachment(skeleton, slot, timeline.AttachmentNames[Timeline.Search(frames, time)], attachments);

			// If an attachment wasn't set (ie before the first frame or attachments is false), set the setup attachment later.
			if (slot.attachmentState <= unkeyedState) slot.attachmentState = unkeyedState + Setup;
		}

		private void SetAttachment (Skeleton skeleton, Slot slot, String attachmentName, bool attachments) {
			slot.Attachment = attachmentName == null ? null : skeleton.GetAttachment(slot.data.index, attachmentName);
			if (attachments) slot.attachmentState = unkeyedState + Current;
		}

		/// <summary>
		/// Applies the rotate timeline, mixing with the current pose while keeping the same rotation direction chosen as the shortest
		/// the first time the mixing was applied.</summary>
		static private void ApplyRotateTimeline (RotateTimeline timeline, Skeleton skeleton, float time, float alpha, MixBlend blend,
			float[] timelinesRotation, int i, bool firstFrame) {

			if (firstFrame) timelinesRotation[i] = 0;

			if (alpha == 1) {
				timeline.Apply(skeleton, 0, time, null, 1, blend, MixDirection.In);
				return;
			}

			Bone bone = skeleton.bones.Items[timeline.BoneIndex];
			if (!bone.active) return;

			float[] frames = timeline.frames;
			float r1, r2;
			if (time < frames[0]) { // Time is before first frame.
				switch (blend) {
				case MixBlend.Setup:
					bone.rotation = bone.data.rotation;
					goto default; // Fall through.
				default:
					return;
				case MixBlend.First:
					r1 = bone.rotation;
					r2 = bone.data.rotation;
					break;
				}
			} else {
				r1 = blend == MixBlend.Setup ? bone.data.rotation : bone.rotation;
				r2 = bone.data.rotation + timeline.GetCurveValue(time);
			}

			// Mix between rotations using the direction of the shortest route on the first frame.
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
			bone.rotation = r1 + total * alpha;
		}

		private void QueueEvents (TrackEntry entry, float animationTime) {
			float animationStart = entry.animationStart, animationEnd = entry.animationEnd;
			float duration = animationEnd - animationStart;
			float trackLastWrapped = entry.trackLast % duration;

			// Queue events before complete.
			Event[] eventsItems = this.events.Items;
			int i = 0, n = events.Count;
			for (; i < n; i++) {
				Event e = eventsItems[i];
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
		/// <para>Removes all animations from all tracks, leaving skeletons in their current pose.</para>
		/// <para>
		/// It may be desired to use <see cref="AnimationState.SetEmptyAnimations(float)"/> to mix the skeletons back to the setup pose,
		/// rather than leaving them in their current pose.</para>
		/// </summary>
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
		/// <para>Removes all animations from the track, leaving skeletons in their current pose.</para>
		/// <para>
		/// It may be desired to use <see cref="AnimationState.SetEmptyAnimation(int, float)"/> to mix the skeletons back to the setup pose,
		/// rather than leaving them in their current pose.</para>
		/// </summary>
		public void ClearTrack (int trackIndex) {
			if (trackIndex >= tracks.Count) return;
			TrackEntry current = tracks.Items[trackIndex];
			if (current == null) return;

			queue.End(current);

			ClearNext(current);

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
			current.previous = null;

			if (from != null) {
				if (interrupt) queue.Interrupt(from);
				current.mixingFrom = from;
				from.mixingTo = current;
				current.mixTime = 0;

				// Store the interrupted mix percentage.
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

		/// <summary>Sets the current animation for a track, discarding any queued animations. If the formerly current track entry was never
		/// applied to a skeleton, it is replaced (not mixed from).</summary>
		/// <param name="loop">If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
		///          duration. In either case<see cref="TrackEntry.TrackEnd"/> determines when the track is cleared.</param>
		/// <returns> A track entry to allow further customization of animation playback. References to the track entry must not be kept
		///          after the <see cref="AnimationState.Dispose"/> event occurs.</returns>
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
					ClearNext(current);
					current = current.mixingFrom;
					interrupt = false; // mixingFrom is current again, but don't interrupt it twice.
				} else
					ClearNext(current);
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

		/// <summary>Adds an animation to be played after the current or last queued animation for a track. If the track is empty, it is
		/// equivalent to calling <see cref="SetAnimation(int, Animation, bool)"/>.</summary>
		/// <param name="delay">
		/// If &gt; 0, sets <see cref="TrackEntry.Delay"/>. If &lt;= 0, the delay set is the duration of the previous track entry
		/// minus any mix duration (from the <see cref="AnimationStateData"/> plus the specified <code>Delay</code> (ie the mix
		/// ends at (<code>Delay</code> = 0) or before (<code>Delay</code> &lt; 0) the previous track entry duration). If the
		/// previous entry is looping, its next loop completion is used instead of its duration.
		/// </param>
		/// <returns>A track entry to allow further customization of animation playback. References to the track entry must not be kept
		/// after the <see cref="AnimationState.Dispose"/> event occurs.</returns>
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
				entry.previous = last;
				if (delay <= 0) delay += last.TrackComplete - entry.mixDuration;
			}

			entry.delay = delay;
			return entry;
		}

		/// <summary>
		/// <para>Sets an empty animation for a track, discarding any queued animations, and sets the track entry's
		/// <see cref="TrackEntry.getMixDuration()"/>. An empty animation has no timelines and serves as a placeholder for mixing in or out.</para>
		/// <para>
		/// Mixing out is done by setting an empty animation with a mix duration using either <see cref="AnimationState.SetEmptyAnimation(int, float)"/>,
		/// <see cref="AnimationState.SetEmptyAnimations(float)"/>, or <see cref="AnimationState.AddEmptyAnimation(int, float, float)"/>. Mixing to an empty animation causes
		/// the previous animation to be applied less and less over the mix duration. Properties keyed in the previous animation
		/// transition to the value from lower tracks or to the setup pose value if no lower tracks key the property. A mix duration of
		/// 0 still mixes out over one frame.</para>
		/// <para>
		/// Mixing in is done by first setting an empty animation, then adding an animation using
		/// <see cref="AnimationState.AddAnimation(int, Animation, bool, float)"/> with the desired delay (an empty animation has a duration of 0) and on
		/// the returned track entry, set the <see cref="TrackEntry.SetMixDuration(float)"/>. Mixing from an empty animation causes the new
		/// animation to be applied more and more over the mix duration. Properties keyed in the new animation transition from the value
		/// from lower tracks or from the setup pose value if no lower tracks key the property to the value keyed in the new
		/// animation.</para></summary>
		public TrackEntry SetEmptyAnimation (int trackIndex, float mixDuration) {
			TrackEntry entry = SetAnimation(trackIndex, AnimationState.EmptyAnimation, false);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		}

		/// <summary>
		/// Adds an empty animation to be played after the current or last queued animation for a track, and sets the track entry's
		/// <see cref="TrackEntry.MixDuration"/>. If the track is empty, it is equivalent to calling
		/// <see cref="AnimationState.SetEmptyAnimation(int, float)"/>.</summary>
		/// <seealso cref="AnimationState.SetEmptyAnimation(int, float)"/>
		/// <param name="trackIndex">Track number.</param>
		/// <param name="mixDuration">Mix duration.</param>
		/// <param name="delay">If &gt; 0, sets <see cref="TrackEntry.Delay"/>. If &lt;= 0, the delay set is the duration of the previous track entry
		/// minus any mix duration plus the specified <code>Delay</code> (ie the mix ends at (<code>Delay</code> = 0) or
		/// before (<code>Delay</code> &lt; 0) the previous track entry duration). If the previous entry is looping, its next
		/// loop completion is used instead of its duration.</param>
		/// <returns> A track entry to allow further customization of animation playback. References to the track entry must not be kept
		/// after the <see cref="AnimationState.Dispose"/> event occurs.
		/// </returns>
		public TrackEntry AddEmptyAnimation (int trackIndex, float mixDuration, float delay) {
			TrackEntry entry = AddAnimation(trackIndex, AnimationState.EmptyAnimation, false, delay);
			if (delay <= 0) entry.delay += entry.mixDuration - mixDuration;
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		}

		/// <summary>
		/// Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix
		/// duration.</summary>
		public void SetEmptyAnimations (float mixDuration) {
			bool oldDrainDisabled = queue.drainDisabled;
			queue.drainDisabled = true;
			TrackEntry[] tracksItems = tracks.Items;
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry current = tracksItems[i];
				if (current != null) SetEmptyAnimation(current.trackIndex, mixDuration);
			}
			queue.drainDisabled = oldDrainDisabled;
			queue.Drain();
		}

		private TrackEntry ExpandToIndex (int index) {
			if (index < tracks.Count) return tracks.Items[index];
			tracks.Resize(index + 1);
			return null;
		}

		/// <summary>Object-pooling version of new TrackEntry. Obtain an unused TrackEntry from the pool and clear/initialize its values.</summary>
		/// <param name="last">May be null.</param>
		private TrackEntry NewTrackEntry (int trackIndex, Animation animation, bool loop, TrackEntry last) {
			TrackEntry entry = trackEntryPool.Obtain();
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
			entry.nextTrackLast = -1;
			entry.trackEnd = float.MaxValue;
			entry.timeScale = 1;

			entry.alpha = 1;
			entry.interruptAlpha = 1;
			entry.mixTime = 0;
			entry.mixDuration = last == null ? 0 : data.GetMix(last.animation, animation);
			entry.mixBlend = MixBlend.Replace;
			return entry;
		}

		/// <summary>Removes the <see cref="TrackEntry.Next">next entry</see> and all entries after it for the specified entry.</summary>
		public void ClearNext (TrackEntry entry) {
			TrackEntry next = entry.next;
			while (next != null) {
				queue.Dispose(next);
				next = next.next;
			}
			entry.next = null;
		}

		private void AnimationsChanged () {
			animationsChanged = false;

			// Process in the order that animations are applied.
			propertyIds.Clear();
			int n = tracks.Count;
			TrackEntry[] tracksItems = tracks.Items;
			for (int i = 0; i < n; i++) {
				TrackEntry entry = tracksItems[i];
				if (entry == null) continue;
				while (entry.mixingFrom != null) // Move to last entry, then iterate in reverse.
					entry = entry.mixingFrom;
				do {
					if (entry.mixingTo == null || entry.mixBlend != MixBlend.Add) ComputeHold(entry);
					entry = entry.mixingTo;
				} while (entry != null);
			}
		}

		private void ComputeHold (TrackEntry entry) {
			TrackEntry to = entry.mixingTo;
			Timeline[] timelines = entry.animation.timelines.Items;
			int timelinesCount = entry.animation.timelines.Count;
			int[] timelineMode = entry.timelineMode.Resize(timelinesCount).Items;
			entry.timelineHoldMix.Clear();
			TrackEntry[] timelineHoldMix = entry.timelineHoldMix.Resize(timelinesCount).Items;
			HashSet<string> propertyIds = this.propertyIds;

			if (to != null && to.holdPrevious) {
				for (int i = 0; i < timelinesCount; i++)
					timelineMode[i] = propertyIds.AddAll(timelines[i].PropertyIds) ? AnimationState.HoldFirst : AnimationState.HoldSubsequent;

				return;
			}

			// outer:
			for (int i = 0; i < timelinesCount; i++) {
				Timeline timeline = timelines[i];
				String[] ids = timeline.PropertyIds;
				if (!propertyIds.AddAll(ids))
					timelineMode[i] = AnimationState.Subsequent;
				else if (to == null || timeline is AttachmentTimeline || timeline is DrawOrderTimeline
						|| timeline is EventTimeline || !to.animation.HasTimeline(ids)) {
					timelineMode[i] = AnimationState.First;
				} else {
					for (TrackEntry next = to.mixingTo; next != null; next = next.mixingTo) {
						if (next.animation.HasTimeline(ids)) continue;
						if (next.mixDuration > 0) {
							timelineMode[i] = AnimationState.HoldMix;
							timelineHoldMix[i] = next;
							goto continue_outer; // continue outer;
						}
						break;
					}
					timelineMode[i] = AnimationState.HoldFirst;
				}
				continue_outer: { }
			}
		}

		/// <returns>The track entry for the animation currently playing on the track, or null if no animation is currently playing.</returns>
		public TrackEntry GetCurrent (int trackIndex) {
			if (trackIndex >= tracks.Count) return null;
			return tracks.Items[trackIndex];
		}

		/// <summary> Discards all listener notifications that have not yet been delivered. This can be useful to call from an
		/// AnimationState event subscriber when it is known that further notifications that may have been already queued for delivery
		/// are not wanted because new animations are being set.
		public void ClearListenerNotifications () {
			queue.Clear();
		}

		/// <summary>
		/// <para>Multiplier for the delta time when the animation state is updated, causing time for all animations and mixes to play slower
		/// or faster. Defaults to 1.</para>
		/// <para>
		/// See TrackEntry <see cref="TrackEntry.TimeScale"/> for affecting a single animation.</para>
		/// </summary>
		public float TimeScale { get { return timeScale; } set { timeScale = value; } }

		/// <summary>The AnimationStateData to look up mix durations.</summary>
		public AnimationStateData Data {
			get {
				return data;
			}
			set {
				if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
				this.data = value;
			}
		}

		/// <summary>A list of tracks that have animations, which may contain nulls.</summary>
		public ExposedList<TrackEntry> Tracks { get { return tracks; } }

		override public string ToString () {
			var buffer = new System.Text.StringBuilder();
			TrackEntry[] tracksItems = tracks.Items;
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry entry = tracksItems[i];
				if (entry == null) continue;
				if (buffer.Length > 0) buffer.Append(", ");
				buffer.Append(entry.ToString());
			}
			if (buffer.Length == 0) return "<none>";
			return buffer.ToString();
		}
	}

	/// <summary>
	/// <para>
	/// Stores settings and other state for the playback of an animation on an <see cref="AnimationState"/> track.</para>
	/// <para>
	/// References to a track entry must not be kept after the <see cref="AnimationStateListener.Dispose(TrackEntry)"/> event occurs.</para>
	/// </summary>
	public class TrackEntry : Pool<TrackEntry>.IPoolable {
		internal Animation animation;

		internal TrackEntry previous, next, mixingFrom, mixingTo;
		// difference to libgdx reference: delegates are used for event callbacks instead of 'AnimationStateListener listener'.
		/// <summary>See <see href="http://esotericsoftware.com/spine-api-reference#AnimationStateListener-Methods">
		/// API Reference documentation pages here</see> for details. Usage in C# and spine-unity is explained
		/// <see href="http://esotericsoftware.com/spine-unity#Processing-AnimationState-Events">here</see>
		/// on the spine-unity documentation pages.</summary>
		public event AnimationState.TrackEntryDelegate Start, Interrupt, End, Dispose, Complete;
		public event AnimationState.TrackEntryEventDelegate Event;
		internal void OnStart () { if (Start != null) Start(this); }
		internal void OnInterrupt () { if (Interrupt != null) Interrupt(this); }
		internal void OnEnd () { if (End != null) End(this); }
		internal void OnDispose () { if (Dispose != null) Dispose(this); }
		internal void OnComplete () { if (Complete != null) Complete(this); }
		internal void OnEvent (Event e) { if (Event != null) Event(this, e); }

		internal int trackIndex;

		internal bool loop, holdPrevious, reverse, shortestRotation;
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
			previous = null;
			next = null;
			mixingFrom = null;
			mixingTo = null;
			animation = null;
			// replaces 'listener = null;' since delegates are used for event callbacks
			Start = null;
			Interrupt = null;
			End = null;
			Dispose = null;
			Complete = null;
			Event = null;
			timelineMode.Clear();
			timelineHoldMix.Clear();
			timelinesRotation.Clear();
		}

		/// <summary>The index of the track where this entry is either current or queued.</summary>
		/// <seealso cref="AnimationState.GetCurrent(int)"/>
		public int TrackIndex { get { return trackIndex; } }

		/// <summary>The animation to apply for this track entry.</summary>
		public Animation Animation { get { return animation; } }

		/// <summary>
		/// If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
		/// duration.</summary>
		public bool Loop { get { return loop; } set { loop = value; } }

		///<summary>
		/// <para>
		/// Seconds to postpone playing the animation. When this track entry is the current track entry, <code>Delay</code>
		/// postpones incrementing the <see cref="TrackEntry.TrackTime"/>. When this track entry is queued, <code>Delay</code> is the time from
		/// the start of the previous animation to when this track entry will become the current track entry (ie when the previous
		/// track entry <see cref="TrackEntry.TrackTime"/> &gt;= this track entry's <code>Delay</code>).</para>
		/// <para>
		/// <see cref="TrackEntry.TimeScale"/> affects the delay.</para>
		/// <para>
		/// When using <see cref="AnimationState.AddAnimation(int, Animation, bool, float)"/> with a <code>delay</code> <= 0, the delay
		/// is set using the mix duration from the <see cref="AnimationStateData"/>. If <see cref="mixDuration"/> is set afterward, the delay
		/// may need to be adjusted.</summary>
		public float Delay { get { return delay; } set { delay = value; } }

		/// <summary>
		/// Current time in seconds this track entry has been the current track entry. The track time determines
		/// <see cref="TrackEntry.AnimationTime"/>. The track time can be set to start the animation at a time other than 0, without affecting
		/// looping.</summary>
		public float TrackTime { get { return trackTime; } set { trackTime = value; } }

		/// <summary>
		/// <para>
		/// The track time in seconds when this animation will be removed from the track. Defaults to the highest possible float
		/// value, meaning the animation will be applied until a new animation is set or the track is cleared. If the track end time
		/// is reached, no other animations are queued for playback, and mixing from any previous animations is complete, then the
		/// properties keyed by the animation are set to the setup pose and the track is cleared.</para>
		/// <para>
		/// It may be desired to use <see cref="AnimationState.AddEmptyAnimation(int, float, float)"/>  rather than have the animation
		/// abruptly cease being applied.</para>
		/// </summary>
		public float TrackEnd { get { return trackEnd; } set { trackEnd = value; } }

		/// <summary>
		/// If this track entry is non-looping, the track time in seconds when <see cref="AnimationEnd"/> is reached, or the current
		/// <see cref="TrackTime"/> if it has already been reached. If this track entry is looping, the track time when this
		/// animation will reach its next <see cref="AnimationEnd"/> (the next loop completion).</summary>
		public float TrackComplete {
			get {
				float duration = animationEnd - animationStart;
				if (duration != 0) {
					if (loop) return duration * (1 + (int)(trackTime / duration)); // Completion of next loop.
					if (trackTime < duration) return duration; // Before duration.
				}
				return trackTime; // Next update.
			}
		}

		/// <summary>
		/// <para>
		/// Seconds when this animation starts, both initially and after looping. Defaults to 0.</para>
		/// <para>
		/// When changing the <code>AnimationStart</code> time, it often makes sense to set <see cref="TrackEntry.AnimationLast"> to the same
		/// value to prevent timeline keys before the start time from triggering.</para>
		/// </summary>
		public float AnimationStart { get { return animationStart; } set { animationStart = value; } }

		/// <summary>
		/// Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animations will
		/// loop back to <see cref="TrackEntry.AnimationStart"/> at this time. Defaults to the animation <see cref="Animation.Duration"/>.
		///</summary>
		public float AnimationEnd { get { return animationEnd; } set { animationEnd = value; } }

		/// <summary>
		/// The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
		/// animation is applied, event timelines will fire all events between the <code>AnimationLast</code> time (exclusive) and
		/// <code>AnimationTime</code> (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this animation
		/// is applied.</summary>
		public float AnimationLast {
			get { return animationLast; }
			set {
				animationLast = value;
				nextAnimationLast = value;
			}
		}

		/// <summary>
		/// Uses <see cref="TrackEntry.TrackTime"/> to compute the <code>AnimationTime</code>. When the <code>TrackTime</code> is 0, the
		/// <code>AnimationTime</code> is equal to the <code>AnimationStart</code> time.
		/// <para>
		/// The <code>animationTime</code> is between <see cref="AnimationStart"/> and <see cref="AnimationEnd"/>, except if this
		/// track entry is non-looping and <see cref="AnimationEnd"/> is >= to the animation <see cref="Animation.Duration"/>, then
		/// <code>animationTime</code> continues to increase past <see cref="AnimationEnd"/>.</para>
		/// </summary>
		public float AnimationTime {
			get {
				if (loop) {
					float duration = animationEnd - animationStart;
					if (duration == 0) return animationStart;
					return (trackTime % duration) + animationStart;
				}
				float animationTime = trackTime + animationStart;
				return animationEnd >= animation.duration ? animationTime : Math.Min(animationTime, animationEnd);
			}
		}

		/// <summary>
		/// <para>
		/// Multiplier for the delta time when this track entry is updated, causing time for this animation to pass slower or
		/// faster. Defaults to 1.</para>
		/// <para>
		/// Values < 0 are not supported. To play an animation in reverse, use <see cref="Reverse"/>.
		/// <para>
		/// <see cref="TrackEntry.MixTime"/> is not affected by track entry time scale, so <see cref="TrackEntry.MixDuration"/> may need to be adjusted to
		/// match the animation speed.</para>
		/// <para>
		/// When using <see cref="AnimationState.AddAnimation(int, Animation, bool, float)"> with a <code>Delay</code> <= 0, the
		/// <see cref="TrackEntry.Delay"/> is set using the mix duration from the <see cref="AnimationStateData"/>, assuming time scale to be 1. If
		/// the time scale is not 1, the delay may need to be adjusted.</para>
		/// <para>
		/// See AnimationState <see cref="AnimationState.TimeScale"/> for affecting all animations.</para>
		/// </summary>
		public float TimeScale { get { return timeScale; } set { timeScale = value; } }

		/// <summary>
		/// <para>
		/// Values < 1 mix this animation with the skeleton's current pose (usually the pose resulting from lower tracks). Defaults
		/// to 1, which overwrites the skeleton's current pose with this animation.</para>
		/// <para>
		/// Typically track 0 is used to completely pose the skeleton, then alpha is used on higher tracks. It doesn't make sense to
		/// use alpha on track 0 if the skeleton pose is from the last frame render.</para>
		/// </summary>
		public float Alpha { get { return alpha; } set { alpha = value; } }

		public float InterruptAlpha { get { return interruptAlpha; } }

		/// <summary>
		/// When the mix percentage (<see cref="TrackEntry.MixTime"/> / <see cref="TrackEntry.MixDuration"/>) is less than the
		/// <code>EventThreshold</code>, event timelines are applied while this animation is being mixed out. Defaults to 0, so event
		/// timelines are not applied while this animation is being mixed out.
		/// </summary>
		public float EventThreshold { get { return eventThreshold; } set { eventThreshold = value; } }

		/// <summary>
		/// When the mix percentage (<see cref="TrackEntry.MixTime"/> / <see cref="TrackEntry.MixDuration"/>) is less than the
		/// <code>AttachmentThreshold</code>, attachment timelines are applied while this animation is being mixed out. Defaults to
		/// 0, so attachment timelines are not applied while this animation is being mixed out.
		///</summary>
		public float AttachmentThreshold { get { return attachmentThreshold; } set { attachmentThreshold = value; } }

		/// <summary>
		/// When the mix percentage (<see cref="TrackEntry.MixTime"/> / <see cref="TrackEntry.MixDuration"/>) is less than the
		/// <code>DrawOrderThreshold</code>, draw order timelines are applied while this animation is being mixed out. Defaults to 0,
		/// so draw order timelines are not applied while this animation is being mixed out.
		/// </summary>
		public float DrawOrderThreshold { get { return drawOrderThreshold; } set { drawOrderThreshold = value; } }

		/// <summary>
		/// The animation queued to start after this animation, or null if there is none. <code>next</code> makes up a doubly linked
		/// list.
		/// <para>
		/// See <see cref="AnimationState.ClearNext(TrackEntry)"/> to truncate the list.</para></summary>
		public TrackEntry Next { get { return next; } }

		/// <summary>
		/// The animation queued to play before this animation, or null. <code>previous</code> makes up a doubly linked list.</summary>
		public TrackEntry Previous { get { return previous; } }

		/// <summary>
		/// Returns true if at least one loop has been completed.</summary>
		/// <seealso cref="TrackEntry.Complete"/>
		public bool IsComplete {
			get { return trackTime >= animationEnd - animationStart; }
		}

		/// <summary>
		/// Seconds from 0 to the <see cref="TrackEntry.MixDuration"/> when mixing from the previous animation to this animation. May be
		/// slightly more than <code>MixDuration</code> when the mix is complete.</summary>
		public float MixTime { get { return mixTime; } set { mixTime = value; } }

		/// <summary>
		/// <para>
		/// Seconds for mixing from the previous animation to this animation. Defaults to the value provided by AnimationStateData
		/// <see cref="AnimationStateData.GetMix(Animation, Animation)"/> based on the animation before this animation (if any).</para>
		/// <para>
		/// The <code>MixDuration</code> can be set manually rather than use the value from
		/// <see cref="AnimationStateData.GetMix(Animation, Animation)"/>. In that case, the <code>MixDuration</code> can be set for a new
		/// track entry only before <see cref="AnimationState.Update(float)"/> is first called.</para>
		/// <para>
		/// When using <seealso cref="AnimationState.AddAnimation(int, Animation, bool, float)"/> with a <code>Delay</code> &lt;= 0, the
		/// <see cref="TrackEntry.Delay"/> is set using the mix duration from the <see cref=" AnimationStateData"/>. If <code>mixDuration</code> is set
		/// afterward, the delay may need to be adjusted. For example:
		/// <code>entry.Delay = entry.previous.TrackComplete - entry.MixDuration;</code>
		/// </para></summary>
		public float MixDuration { get { return mixDuration; } set { mixDuration = value; } }

		/// <summary>
		/// <para>
		/// Controls how properties keyed in the animation are mixed with lower tracks. Defaults to <see cref="MixBlend.Replace"/>.
		/// </para><para>
		/// Track entries on track 0 ignore this setting and always use <see cref="MixBlend.First"/>.
		/// </para><para>
		///  The <code>MixBlend</code> can be set for a new track entry only before <see cref="AnimationState.Apply(Skeleton)"/> is first
		///  called.</para>
		/// </summary>
		public MixBlend MixBlend { get { return mixBlend; } set { mixBlend = value; } }

		/// <summary>
		/// The track entry for the previous animation when mixing from the previous animation to this animation, or null if no
		/// mixing is currently occuring. When mixing from multiple animations, <code>MixingFrom</code> makes up a linked list.</summary>
		public TrackEntry MixingFrom { get { return mixingFrom; } }

		/// <summary>
		/// The track entry for the next animation when mixing from this animation to the next animation, or null if no mixing is
		/// currently occuring. When mixing to multiple animations, <code>MixingTo</code> makes up a linked list.</summary>
		public TrackEntry MixingTo { get { return mixingTo; } }

		/// <summary>
		/// <para>
		/// If true, when mixing from the previous animation to this animation, the previous animation is applied as normal instead
		/// of being mixed out.</para>
		/// <para>
		/// When mixing between animations that key the same property, if a lower track also keys that property then the value will
		/// briefly dip toward the lower track value during the mix. This happens because the first animation mixes from 100% to 0%
		/// while the second animation mixes from 0% to 100%. Setting <code>HoldPrevious</code> to true applies the first animation
		/// at 100% during the mix so the lower track value is overwritten. Such dipping does not occur on the lowest track which
		/// keys the property, only when a higher track also keys the property.</para>
		/// <para>
		/// Snapping will occur if <code>HoldPrevious</code> is true and this animation does not key all the same properties as the
		/// previous animation.</para>
		/// </summary>
		public bool HoldPrevious { get { return holdPrevious; } set { holdPrevious = value; } }

		/// <summary>
		/// If true, the animation will be applied in reverse. Events are not fired when an animation is applied in reverse.</summary>
		public bool Reverse { get { return reverse; } set { reverse = value; } }

		/// <summary><para>
		/// If true, mixing rotation between tracks always uses the shortest rotation direction. If the rotation is animated, the
		/// shortest rotation direction may change during the mix.
		/// </para><para>
		/// If false, the shortest rotation direction is remembered when the mix starts and the same direction is used for the rest
		/// of the mix. Defaults to false.</para></summary>
		public bool ShortestRotation { get { return shortestRotation; } set { shortestRotation = value; } }

		/// <summary>Returns true if this entry is for the empty animation. See <see cref="AnimationState.SetEmptyAnimation(int, float)"/>,
		/// <see cref="AnimationState.AddEmptyAnimation(int, float, float)"/>, and <see cref="AnimationState.SetEmptyAnimations(float)"/>.
		public bool IsEmptyAnimation { get { return animation == AnimationState.EmptyAnimation; } }

		/// <summary>
		/// <para>
		/// Resets the rotation directions for mixing this entry's rotate timelines. This can be useful to avoid bones rotating the
		/// long way around when using <see cref="alpha"/> and starting animations on other tracks.</para>
		/// <para>
		/// Mixing with <see cref="MixBlend.Replace"/> involves finding a rotation between two others, which has two possible solutions:
		/// the short way or the long way around. The two rotations likely change over time, so which direction is the short or long
		/// way also changes. If the short way was always chosen, bones would flip to the other side when that direction became the
		/// long way. TrackEntry chooses the short way the first time it is applied and remembers that direction.</para>
		/// </summary>
		public void ResetRotationDirections () {
			timelinesRotation.Clear();
		}

		override public string ToString () {
			return animation == null ? "<none>" : animation.name;
		}

		// Note: This method is required by SpineAnimationStateMixerBehaviour,
		// which is part of the timeline extension package. Thus the internal member variable
		// nextTrackLast is not accessible. We favor providing this method
		// over exposing nextTrackLast as public property, which would rather confuse users.
		public void AllowImmediateQueue () {
			if (nextTrackLast < 0) nextTrackLast = 0;
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

			List<EventQueueEntry> eventQueueEntries = this.eventQueueEntries;
			AnimationState state = this.state;

			// Don't cache eventQueueEntries.Count so callbacks can queue their own events (eg, call SetAnimation in AnimationState_Complete).
			for (int i = 0; i < eventQueueEntries.Count; i++) {
				EventQueueEntry queueEntry = eventQueueEntries[i];
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
					trackEntryPool.Free(trackEntry);
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
	}

	class Pool<T> where T : class, new() {
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

	public static class HashSetExtensions {
		public static bool AddAll<T> (this HashSet<T> set, T[] addSet) {
			bool anyItemAdded = false;
			for (int i = 0, n = addSet.Length; i < n; ++i) {
				T item = addSet[i];
				anyItemAdded |= set.Add(item);
			}
			return anyItemAdded;
		}
	}
}
