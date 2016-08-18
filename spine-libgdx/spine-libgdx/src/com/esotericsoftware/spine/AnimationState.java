/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.Pool;
import com.badlogic.gdx.utils.Pool.Poolable;
import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.Timeline;

/** Stores state for an animation and automatically mixes between animations. */
public class AnimationState {
	private AnimationStateData data;
	private Array<TrackEntry> tracks = new Array();
	private final Array<Event> events = new Array();
	private final EventQueue queue = new EventQueue();
	final Array<AnimationStateListener> listeners = new Array();
	private float timeScale = 1;
	private float eventThreshold, attachmentThreshold, drawOrderThreshold;

	final Pool<TrackEntry> trackEntryPool = new Pool() {
		protected Object newObject () {
			return new TrackEntry();
		}
	};

	/** Creates an uninitialized AnimationState. The animation state data must be set before use. */
	public AnimationState () {
	}

	public AnimationState (AnimationStateData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
	}

	public void update (float delta) {
		delta *= timeScale;
		for (int i = 0; i < tracks.size; i++) {
			TrackEntry current = tracks.get(i);
			if (current == null) continue;

			float currentDelta = delta * current.timeScale;

			TrackEntry next = current.next;
			if (next != null) {
				// When the next entry's delay is passed, change to it.
				float nextTime = current.lastTime - next.delay;
				if (nextTime >= 0) {
					next.time = nextTime + delta * next.timeScale;
					current.time += currentDelta;
					setCurrent(i, next);
					queue.drain();
					if (next.previous != null) next.mixTime += currentDelta;
					continue;
				}
			} else if (!current.loop && current.lastTime >= current.endTime) {
				// End non-looping animation when it reaches its end time and there is no next entry.
				clearTrack(i);
				continue;
			}

			current.time += currentDelta;
			if (current.previous != null) {
				float previousDelta = delta * current.previous.timeScale;
				current.previous.time += previousDelta;
				current.mixTime += previousDelta;
			}
		}
	}

	public void apply (Skeleton skeleton) {
		Array<Event> events = this.events;

		for (int i = 0; i < tracks.size; i++) {
			TrackEntry current = tracks.get(i);
			if (current == null) continue;

			float time = current.time, lastTime = current.lastTime, endTime = current.endTime, mix = current.alpha;
			boolean loop = current.loop;
			if (!loop && time > endTime) time = endTime;

			if (current.previous != null) {
				mix *= current.mixTime / current.mixDuration;
				applyPrevious(current.previous, skeleton, mix);
				if (mix >= 1) {
					mix = 1;
					queue.end(current.previous);
					current.previous = null;
				}
			}
			current.animation.mix(skeleton, lastTime, time, loop, events, mix);
			queueEvents(current, lastTime, time, endTime);

			current.lastTime = current.time;
		}

		queue.drain();
	}

	private void applyPrevious (TrackEntry previous, Skeleton skeleton, float mix) {
		float previousTime = previous.time;
		if (!previous.loop && previousTime > previous.endTime) previousTime = previous.endTime;

		float lastTime = previous.lastTime, time = previousTime, alpha = previous.alpha;
		Animation animation = previous.animation;
		if (previous.loop && animation.duration != 0) {
			time %= animation.duration;
			if (lastTime > 0) lastTime %= animation.duration;
		}

		Array<Event> events = mix < previous.eventThreshold ? this.events : null;

		Array<Timeline> timelines = animation.timelines;
		boolean attachments = mix < previous.attachmentThreshold, drawOrder = mix < previous.drawOrderThreshold;
		if (attachments && drawOrder) {
			for (int i = 0, n = timelines.size; i < n; i++)
				timelines.get(i).apply(skeleton, lastTime, time, events, alpha);
		} else {
			for (int i = 0, n = timelines.size; i < n; i++) {
				Timeline timeline = timelines.get(i);
				if (!attachments && timeline instanceof AttachmentTimeline) continue;
				if (!drawOrder && timeline instanceof DrawOrderTimeline) continue;
				timeline.apply(skeleton, lastTime, time, events, alpha);
			}
		}

		queueEvents(previous, previous.lastTime, previousTime, previous.endTime);
		previous.lastTime = previousTime;
	}

	private void queueEvents (TrackEntry entry, float lastTime, float time, float endTime) {
		Array<Event> events = this.events;
		int n = events.size;

		// Queue events before complete.
		float lastTimeWrapped = lastTime % endTime;
		int i = 0;
		for (; i < n; i++) {
			Event event = events.get(i);
			if (events.get(i).time < lastTimeWrapped) break;
			queue.event(entry, event);
		}

		// Queue complete if completed the animation or a loop iteration.
		if (entry.loop ? (lastTime % endTime > time % endTime) : (lastTime < endTime && time >= endTime))
			queue.complete(entry, (int)(time / endTime));

		// Queue events after complete.
		for (; i < n; i++)
			queue.event(entry, events.get(i));
		events.clear();
	}

	public void clearTracks () {
		for (int i = 0, n = tracks.size; i < n; i++)
			clearTrack(i);
		tracks.clear();
	}

	public void clearTrack (int trackIndex) {
		if (trackIndex >= tracks.size) return;
		TrackEntry current = tracks.get(trackIndex);
		if (current == null) return;
		freeAll(current.next);

		queue.end(current);
		if (current.previous != null) queue.end(current.previous);
		queue.drain();

		tracks.set(trackIndex, null);
	}

	/** @param entry May be null. */
	private void freeAll (TrackEntry entry) {
		while (entry != null) {
			TrackEntry next = entry.next;
			trackEntryPool.free(entry);
			entry = next;
		}
	}

	private TrackEntry expandToIndex (int index) {
		if (index < tracks.size) return tracks.get(index);
		tracks.ensureCapacity(index - tracks.size + 1);
		tracks.size = index + 1;
		return null;
	}

	private void setCurrent (int index, TrackEntry entry) {
		TrackEntry current = expandToIndex(index);

		tracks.set(index, entry);

		queue.start(entry);

		if (current != null) {
			TrackEntry previous = current.previous;
			current.previous = null;

			queue.interrupt(current);

			entry.mixDuration = data.getMix(current.animation, entry.animation);
			if (entry.mixDuration > 0) {
				entry.mixTime = 0;
				// If a mix is in progress, mix from the closest animation.
				if (previous != null && current.mixTime / current.mixDuration < 0.5f) {
					entry.previous = previous;
					previous = current;
				} else {
					entry.previous = current;
				}
			} else
				queue.end(current);

			if (previous != null) queue.end(previous);
		}

		queue.drain();
	}

	/** @see #setAnimation(int, Animation, boolean) */
	public TrackEntry setAnimation (int trackIndex, String animationName, boolean loop) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}

	/** Set the current animation. Any queued animations are cleared. */
	public TrackEntry setAnimation (int trackIndex, Animation animation, boolean loop) {
		TrackEntry current = expandToIndex(trackIndex);
		if (current != null) freeAll(current.next);

		TrackEntry entry = trackEntryPool.obtain();
		entry.animation = animation;
		entry.loop = loop;
		entry.endTime = animation.getDuration();
		entry.eventThreshold = eventThreshold;
		entry.attachmentThreshold = attachmentThreshold;
		entry.drawOrderThreshold = drawOrderThreshold;

		setCurrent(trackIndex, entry);
		queue.drain();
		return entry;
	}

	/** {@link #addAnimation(int, Animation, boolean, float)} */
	public TrackEntry addAnimation (int trackIndex, String animationName, boolean loop, float delay) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return addAnimation(trackIndex, animation, loop, delay);
	}

	/** Adds an animation to be played delay seconds after the current or last queued animation.
	 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay. */
	public TrackEntry addAnimation (int trackIndex, Animation animation, boolean loop, float delay) {
		TrackEntry entry = trackEntryPool.obtain();
		entry.animation = animation;
		entry.loop = loop;
		entry.endTime = animation.getDuration();
		entry.eventThreshold = eventThreshold;
		entry.attachmentThreshold = attachmentThreshold;
		entry.drawOrderThreshold = drawOrderThreshold;

		TrackEntry last = expandToIndex(trackIndex);
		if (last != null) {
			while (last.next != null)
				last = last.next;
			last.next = entry;
		} else
			tracks.set(trackIndex, entry);

		if (delay <= 0) {
			if (last != null)
				delay += last.endTime - data.getMix(last.animation, animation);
			else
				delay = 0;
		}
		entry.delay = delay;

		return entry;
	}

	/** @return May be null. */
	public TrackEntry getCurrent (int trackIndex) {
		if (trackIndex >= tracks.size) return null;
		return tracks.get(trackIndex);
	}

	/** Adds a listener to receive events for all animations. */
	public void addListener (AnimationStateListener listener) {
		if (listener == null) throw new IllegalArgumentException("listener cannot be null.");
		listeners.add(listener);
	}

	/** Removes the listener added with {@link #addListener(AnimationStateListener)}. */
	public void removeListener (AnimationStateListener listener) {
		listeners.removeValue(listener, true);
	}

	public void clearListeners () {
		listeners.clear();
	}

	public void clearEvents () {
		queue.clear();
	}

	public float getTimeScale () {
		return timeScale;
	}

	public void setTimeScale (float timeScale) {
		this.timeScale = timeScale;
	}

	public float getEventThreshold () {
		return eventThreshold;
	}

	public void setEventThreshold (float eventThreshold) {
		this.eventThreshold = eventThreshold;
	}

	public float getAttachmentThreshold () {
		return attachmentThreshold;
	}

	public void setAttachmentThreshold (float attachmentThreshold) {
		this.attachmentThreshold = attachmentThreshold;
	}

	public float getDrawOrderThreshold () {
		return drawOrderThreshold;
	}

	public void setDrawOrderThreshold (float drawOrderThreshold) {
		this.drawOrderThreshold = drawOrderThreshold;
	}

	public AnimationStateData getData () {
		return data;
	}

	public void setData (AnimationStateData data) {
		this.data = data;
	}

	/** Returns the list of tracks that have animations, which may contain nulls. */
	public Array<TrackEntry> getTracks () {
		return tracks;
	}

	public String toString () {
		StringBuilder buffer = new StringBuilder(64);
		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry entry = tracks.get(i);
			if (entry == null) continue;
			if (buffer.length() > 0) buffer.append(", ");
			buffer.append(entry.toString());
		}
		if (buffer.length() == 0) return "<none>";
		return buffer.toString();
	}

	static public class TrackEntry implements Poolable {
		int index;
		TrackEntry next, previous;
		Animation animation;
		boolean loop;
		float delay, time, lastTime = -1, endTime, timeScale = 1;
		float eventThreshold, attachmentThreshold, drawOrderThreshold;
		float mixTime, mixDuration;
		AnimationStateListener listener;
		float alpha = 1;

		public void reset () {
			next = null;
			previous = null;
			animation = null;
			listener = null;
			timeScale = 1;
			lastTime = -1; // Trigger events on frame zero.
			time = 0;
		}

		public Animation getAnimation () {
			return animation;
		}

		public void setAnimation (Animation animation) {
			this.animation = animation;
		}

		public boolean getLoop () {
			return loop;
		}

		public void setLoop (boolean loop) {
			this.loop = loop;
		}

		public float getDelay () {
			return delay;
		}

		public void setDelay (float delay) {
			this.delay = delay;
		}

		public float getTime () {
			return time;
		}

		public void setTime (float time) {
			this.time = time;
		}

		public float getEndTime () {
			return endTime;
		}

		public void setEndTime (float endTime) {
			this.endTime = endTime;
		}

		public AnimationStateListener getListener () {
			return listener;
		}

		public void setListener (AnimationStateListener listener) {
			this.listener = listener;
		}

		public float getLastTime () {
			return lastTime;
		}

		public void setLastTime (float lastTime) {
			this.lastTime = lastTime;
		}

		public float getMix () {
			return alpha;
		}

		public void setMix (float mix) {
			this.alpha = mix;
		}

		public float getTimeScale () {
			return timeScale;
		}

		public void setTimeScale (float timeScale) {
			this.timeScale = timeScale;
		}

		public float getEventThreshold () {
			return eventThreshold;
		}

		public void setEventThreshold (float eventThreshold) {
			this.eventThreshold = eventThreshold;
		}

		public float getAttachmentThreshold () {
			return attachmentThreshold;
		}

		public void setAttachmentThreshold (float attachmentThreshold) {
			this.attachmentThreshold = attachmentThreshold;
		}

		public float getDrawOrderThreshold () {
			return drawOrderThreshold;
		}

		public void setDrawOrderThreshold (float drawOrderThreshold) {
			this.drawOrderThreshold = drawOrderThreshold;
		}

		public TrackEntry getNext () {
			return next;
		}

		public void setNext (TrackEntry next) {
			this.next = next;
		}

		/** Returns true if the current time is greater than the end time, regardless of looping. */
		public boolean isComplete () {
			return time >= endTime;
		}

		public int getTrackIndex () {
			return index;
		}

		public String toString () {
			return animation == null ? "<none>" : animation.name;
		}
	}

	class EventQueue {
		static private final int START = -3, EVENT = -2, INTERRUPT = -1, END = 0;

		boolean draining;
		final Array objects = new Array();
		final IntArray eventTypes = new IntArray(); // If > 0 it's loop count for a complete event.

		public void start (TrackEntry entry) {
			objects.add(entry);
			eventTypes.add(START);
		}

		public void event (TrackEntry entry, Event event) {
			objects.add(entry);
			objects.add(event);
			eventTypes.add(EVENT);
		}

		public void complete (TrackEntry entry, int loopCount) {
			objects.add(entry);
			eventTypes.add(loopCount);
		}

		public void interrupt (TrackEntry entry) {
			objects.add(entry);
			eventTypes.add(INTERRUPT);
		}

		public void end (TrackEntry entry) {
			objects.add(entry);
			eventTypes.add(END);
		}

		public void drain () {
			if (draining) return; // Not reentrant.
			draining = true;

			Array objects = this.objects;
			IntArray eventTypes = this.eventTypes;
			Array<AnimationStateListener> listeners = AnimationState.this.listeners;
			for (int e = 0, o = 0; e < eventTypes.size; e++, o++) {
				TrackEntry entry = (TrackEntry)objects.get(o);
				int eventType = eventTypes.get(e);
				switch (eventType) {
				case START:
					if (entry.listener != null) entry.listener.end(entry);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).start(entry);
					break;
				case EVENT:
					Event event = (Event)objects.get(++o);
					if (entry.listener != null) entry.listener.event(entry, event);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).event(entry, event);
					break;
				case INTERRUPT:
					if (entry.listener != null) entry.listener.end(entry);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).interrupt(entry);
					break;
				case END:
					if (entry.listener != null) entry.listener.end(entry);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).end(entry);
					trackEntryPool.free(entry);
					break;
				default:
					if (entry.listener != null) entry.listener.complete(entry, eventType);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).complete(entry, eventType);
				}
			}
			clear();

			draining = false;
		}

		public void clear () {
			objects.clear();
			eventTypes.clear();
		}
	}

	static public interface AnimationStateListener {
		/** Invoked when this animation triggers an event. */
		public void event (TrackEntry entry, Event event);

		/** Invoked every time this animation completes a loop.
		 * @param loopCount The number of times the animation reached the end. */
		public void complete (TrackEntry entry, int loopCount);

		/** Invoked just after this animation is set. */
		public void start (TrackEntry entry);

		/** Invoked just after another animation is set. */
		public void interrupt (TrackEntry entry);

		/** Invoked when this animation will no longer be applied. */
		public void end (TrackEntry entry);
	}

	static public abstract class AnimationStateAdapter implements AnimationStateListener {
		public void event (TrackEntry entry, Event event) {
		}

		public void complete (TrackEntry entry, int loopCount) {
		}

		public void start (TrackEntry entry) {
		}

		public void interrupt (TrackEntry entry) {
		}

		public void end (TrackEntry entry) {
		}
	}
}
