/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#include <spine/AnimationState.h>
#include <spine/Animation.h>
#include <spine/AnimationStateData.h>
#include <spine/AttachmentTimeline.h>
#include <spine/Bone.h>
#include <spine/BoneData.h>
#include <spine/DrawOrderFolderTimeline.h>
#include <spine/DrawOrderTimeline.h>
#include <spine/Event.h>
#include <spine/EventTimeline.h>
#include <spine/Interpolation.h>
#include <spine/RotateTimeline.h>
#include <spine/Skeleton.h>
#include <spine/SkeletonData.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>

#include <float.h>

using namespace spine;

#ifdef SPINE_USE_STD_FUNCTION
void dummyOnAnimationEventFunc(AnimationState *state, spine::EventType type, TrackEntry *entry, Event *event) {
	SP_UNUSED(state);
	SP_UNUSED(type);
	SP_UNUSED(entry);
	SP_UNUSED(event);
}

#define SP_ANIMATION_LISTENER_USER_DATA_CTOR
#define SP_ANIMATION_LISTENER_CLEAR_USER_DATA()
#define SP_INVOKE_ANIMATION_LISTENER(listener, state, type, entry, event, userData) listener(&state, type, entry, event)
#else
void dummyOnAnimationEventFunc(AnimationState *state, spine::EventType type, TrackEntry *entry, Event *event, void *userData) {
	SP_UNUSED(state);
	SP_UNUSED(type);
	SP_UNUSED(entry);
	SP_UNUSED(event);
	SP_UNUSED(userData);
}

#define SP_ANIMATION_LISTENER_USER_DATA_CTOR _listenerUserData(NULL),
#define SP_ANIMATION_LISTENER_CLEAR_USER_DATA() _listenerUserData = NULL
#define SP_INVOKE_ANIMATION_LISTENER(listener, state, type, entry, event, userData) listener(&state, type, entry, event, userData)
#endif

TrackEntry::TrackEntry()
	: _animation(NULL), _previous(NULL), _next(NULL), _mixingFrom(NULL), _mixingTo(0), _trackIndex(0), _loop(false), _additive(false),
	  _reverse(false), _shortestRotation(false), _keepHold(false), _eventThreshold(0), _mixAttachmentThreshold(0), _alphaAttachmentThreshold(0),
	  _mixDrawOrderThreshold(0), _animationStart(0), _animationEnd(0), _animationLast(0), _nextAnimationLast(0), _delay(0), _trackTime(0),
	  _trackLast(0), _nextTrackLast(0), _trackEnd(0), _timeScale(1.0f), _alpha(0), _mixTime(0), _mixDuration(0), _totalAlpha(0),
	  _mixInterpolation(&Interpolation::linear()), _listener(dummyOnAnimationEventFunc), SP_ANIMATION_LISTENER_USER_DATA_CTOR _listenerObject(NULL),
	  _state(NULL) {
}

TrackEntry::~TrackEntry() {
}

int TrackEntry::getTrackIndex() {
	return _trackIndex;
}

Animation &TrackEntry::getAnimation() {
	return *_animation;
}

void TrackEntry::setAnimation(Animation &animation) {
	_animation = &animation;
}

TrackEntry *TrackEntry::getPrevious() {
	return _previous;
}

bool TrackEntry::getLoop() {
	return _loop;
}

void TrackEntry::setLoop(bool inValue) {
	_loop = inValue;
}

bool TrackEntry::getAdditive() {
	return _additive;
}

void TrackEntry::setAdditive(bool inValue) {
	_additive = inValue;
}

bool TrackEntry::getReverse() {
	return _reverse;
}

void TrackEntry::setReverse(bool inValue) {
	_reverse = inValue;
}

bool TrackEntry::getShortestRotation() {
	return _shortestRotation;
}

void TrackEntry::setShortestRotation(bool inValue) {
	_shortestRotation = inValue;
}

float TrackEntry::getDelay() {
	return _delay;
}

void TrackEntry::setDelay(float inValue) {
	if (inValue < 0) {
		return;
	}
	_delay = inValue;
}

float TrackEntry::getTrackTime() {
	return _trackTime;
}

void TrackEntry::setTrackTime(float inValue) {
	_trackTime = inValue;
}

float TrackEntry::getTrackEnd() {
	return _trackEnd;
}

void TrackEntry::setTrackEnd(float inValue) {
	_trackEnd = inValue;
}

float TrackEntry::getAnimationStart() {
	return _animationStart;
}

void TrackEntry::setAnimationStart(float inValue) {
	_animationStart = inValue;
}

float TrackEntry::getAnimationEnd() {
	return _animationEnd;
}

void TrackEntry::setAnimationEnd(float inValue) {
	_animationEnd = inValue;
}

float TrackEntry::getAnimationLast() {
	return _animationLast;
}

void TrackEntry::setAnimationLast(float inValue) {
	_animationLast = inValue;
	_nextAnimationLast = inValue;
}

float TrackEntry::getAnimationTime() {
	if (!_loop) return MathUtil::min(_trackTime + _animationStart, _animationEnd);
	float duration = _animationEnd - _animationStart;
	if (duration == 0) return _animationStart;
	return MathUtil::fmod(_trackTime, duration) + _animationStart;
}

float TrackEntry::getTimeScale() {
	return _timeScale;
}

void TrackEntry::setTimeScale(float inValue) {
	_timeScale = inValue;
}

float TrackEntry::getAlpha() {
	return _alpha;
}

void TrackEntry::setAlpha(float inValue) {
	_alpha = inValue;
}

float TrackEntry::getEventThreshold() {
	return _eventThreshold;
}

void TrackEntry::setEventThreshold(float inValue) {
	_eventThreshold = inValue;
}

float TrackEntry::getMixAttachmentThreshold() {
	return _mixAttachmentThreshold;
}

void TrackEntry::setMixAttachmentThreshold(float inValue) {
	_mixAttachmentThreshold = inValue;
}

float TrackEntry::getAlphaAttachmentThreshold() {
	return _alphaAttachmentThreshold;
}

void TrackEntry::setAlphaAttachmentThreshold(float inValue) {
	_alphaAttachmentThreshold = inValue;
}

float TrackEntry::getMixDrawOrderThreshold() {
	return _mixDrawOrderThreshold;
}

void TrackEntry::setMixDrawOrderThreshold(float inValue) {
	_mixDrawOrderThreshold = inValue;
}

TrackEntry *TrackEntry::getNext() {
	return _next;
}

bool TrackEntry::isComplete() {
	return _trackTime >= _animationEnd - _animationStart;
}

float TrackEntry::getMixTime() {
	return _mixTime;
}

void TrackEntry::setMixTime(float inValue) {
	_mixTime = inValue;
}

float TrackEntry::getMixDuration() {
	return _mixDuration;
}

void TrackEntry::setMixDuration(float inValue) {
	_mixDuration = inValue;
}

void TrackEntry::setMixDuration(float mixDuration, float delay) {
	_mixDuration = mixDuration;
	if (delay <= 0) delay = _previous == nullptr ? 0 : MathUtil::max(delay + _previous->getTrackComplete() - mixDuration, 0.0f);
	this->_delay = delay;
}

Interpolation &TrackEntry::getMixInterpolation() {
	return *_mixInterpolation;
}

void TrackEntry::setMixInterpolation(Interpolation &mixInterpolation) {
	_mixInterpolation = &mixInterpolation;
}

float TrackEntry::mix() {
	if (_mixDuration == 0) return 1;
	float mix = _mixTime / _mixDuration;
	if (mix >= 1) return 1;
	if (_mixInterpolation == &Interpolation::linear()) return mix;
	mix = _mixInterpolation->apply(mix);
	if (mix < 0) return 0;
	if (mix > 1) return 1;
	return mix;
}

TrackEntry *TrackEntry::getMixingFrom() {
	return _mixingFrom;
}

TrackEntry *TrackEntry::getMixingTo() {
	return _mixingTo;
}

void TrackEntry::resetRotationDirections() {
	_timelinesRotation.clear();
}

#ifdef SPINE_USE_STD_FUNCTION
void TrackEntry::setListener(AnimationStateListener inValue) {
	_listener = inValue;
	_listenerObject = NULL;
}
#else
void TrackEntry::setListener(AnimationStateListener inValue, void *userData) {
	_listener = inValue;
	_listenerUserData = userData;
	_listenerObject = NULL;
}
#endif

void TrackEntry::setListener(AnimationStateListenerObject *inValue) {
	_listener = dummyOnAnimationEventFunc;
	SP_ANIMATION_LISTENER_CLEAR_USER_DATA();
	_listenerObject = inValue;
}

void TrackEntry::reset() {
	_animation = NULL;
	_previous = NULL;
	_next = NULL;
	_mixingFrom = NULL;
	_mixingTo = NULL;
	_mixInterpolation = &Interpolation::linear();

	setRendererObject(NULL);

	_timelineMode.clear();
	_timelineHoldMix.clear();
	_timelinesRotation.clear();

	_listener = dummyOnAnimationEventFunc;
	SP_ANIMATION_LISTENER_CLEAR_USER_DATA();
	_listenerObject = NULL;
}

float TrackEntry::getTrackComplete() {
	float duration = _animationEnd - _animationStart;
	if (duration != 0) {
		if (_loop) return duration * (1 + (int) (_trackTime / duration));// Completion of next loop.
		if (_trackTime < duration) return duration;                      // Before duration.
	}
	return _trackTime;// Next update.
}

bool TrackEntry::wasApplied() {
	return _nextTrackLast != -1;
}

bool TrackEntry::isEmptyAnimation() {
	return _animation == AnimationState::getEmptyAnimation();
}

EventQueueEntry::EventQueueEntry(EventType eventType, TrackEntry *trackEntry, Event *event) : _type(eventType), _entry(trackEntry), _event(event) {
}

EventQueue *EventQueue::newEventQueue(AnimationState &state) {
	return new (__FILE__, __LINE__) EventQueue(state);
}

EventQueueEntry EventQueue::newEventQueueEntry(EventType eventType, TrackEntry *entry, Event *event) {
	return EventQueueEntry(eventType, entry, event);
}

EventQueue::EventQueue(AnimationState &state) : _state(state), _drainDisabled(false) {
}

EventQueue::~EventQueue() {
}

void EventQueue::start(TrackEntry *entry) {
	_eventQueueEntries.add(newEventQueueEntry(EventType_Start, entry));
	_state._animationsChanged = true;
}

void EventQueue::interrupt(TrackEntry *entry) {
	_eventQueueEntries.add(newEventQueueEntry(EventType_Interrupt, entry));
}

void EventQueue::end(TrackEntry *entry) {
	_eventQueueEntries.add(newEventQueueEntry(EventType_End, entry));
	_state._animationsChanged = true;
}

void EventQueue::dispose(TrackEntry *entry) {
	_eventQueueEntries.add(newEventQueueEntry(EventType_Dispose, entry));
}

void EventQueue::complete(TrackEntry *entry) {
	_eventQueueEntries.add(newEventQueueEntry(EventType_Complete, entry));
}

void EventQueue::event(TrackEntry *entry, Event *event) {
	_eventQueueEntries.add(newEventQueueEntry(EventType_Event, entry, event));
}

/// Raises all events in the queue and drains the queue.
void EventQueue::drain() {
	if (_drainDisabled) {
		return;
	}

	_drainDisabled = true;

	AnimationState &state = _state;

	// Don't cache _eventQueueEntries.size() so callbacks can queue their own events (eg, call setAnimation in AnimationState_Complete).
	for (size_t i = 0; i < _eventQueueEntries.size(); ++i) {
		EventQueueEntry queueEntry = _eventQueueEntries[i];
		TrackEntry *trackEntry = queueEntry._entry;

		switch (queueEntry._type) {
			case EventType_Start:
			case EventType_Interrupt:
			case EventType_Complete:
				if (!trackEntry->_listenerObject)
					SP_INVOKE_ANIMATION_LISTENER(trackEntry->_listener, state, queueEntry._type, trackEntry, NULL, trackEntry->_listenerUserData);
				else
					trackEntry->_listenerObject->callback(&state, queueEntry._type, trackEntry, NULL);
				if (!state._listenerObject)
					SP_INVOKE_ANIMATION_LISTENER(state._listener, state, queueEntry._type, trackEntry, NULL, state._listenerUserData);
				else
					state._listenerObject->callback(&state, queueEntry._type, trackEntry, NULL);
				break;
			case EventType_End:
				if (!trackEntry->_listenerObject)
					SP_INVOKE_ANIMATION_LISTENER(trackEntry->_listener, state, queueEntry._type, trackEntry, NULL, trackEntry->_listenerUserData);
				else
					trackEntry->_listenerObject->callback(&state, queueEntry._type, trackEntry, NULL);
				if (!state._listenerObject)
					SP_INVOKE_ANIMATION_LISTENER(state._listener, state, queueEntry._type, trackEntry, NULL, state._listenerUserData);
				else
					state._listenerObject->callback(&state, queueEntry._type, trackEntry, NULL);
				/* Fall through. */
			case EventType_Dispose:
				if (!trackEntry->_listenerObject)
					SP_INVOKE_ANIMATION_LISTENER(trackEntry->_listener, state, EventType_Dispose, trackEntry, NULL, trackEntry->_listenerUserData);
				else
					trackEntry->_listenerObject->callback(&state, EventType_Dispose, trackEntry, NULL);
				if (!state._listenerObject)
					SP_INVOKE_ANIMATION_LISTENER(state._listener, state, EventType_Dispose, trackEntry, NULL, state._listenerUserData);
				else
					state._listenerObject->callback(&state, EventType_Dispose, trackEntry, NULL);

				if (!_state.getManualTrackEntryDisposal()) _state.disposeTrackEntry(trackEntry);
				break;
			case EventType_Event:
				if (!trackEntry->_listenerObject)
					SP_INVOKE_ANIMATION_LISTENER(trackEntry->_listener, state, queueEntry._type, trackEntry, queueEntry._event,
												 trackEntry->_listenerUserData);
				else
					trackEntry->_listenerObject->callback(&state, queueEntry._type, trackEntry, queueEntry._event);
				if (!state._listenerObject)
					SP_INVOKE_ANIMATION_LISTENER(state._listener, state, queueEntry._type, trackEntry, queueEntry._event, state._listenerUserData);
				else
					state._listenerObject->callback(&state, queueEntry._type, trackEntry, queueEntry._event);
				break;
		}
	}
	_eventQueueEntries.clear();

	_drainDisabled = false;
}

AnimationState::AnimationState(AnimationStateData &data)
	: _data(&data), _queue(EventQueue::newEventQueue(*this)), _animationsChanged(false), _listener(dummyOnAnimationEventFunc),
	  SP_ANIMATION_LISTENER_USER_DATA_CTOR _listenerObject(NULL), _unkeyedState(0), _timeScale(1), _manualTrackEntryDisposal(false) {
}

AnimationState::~AnimationState() {
	for (size_t i = 0; i < _tracks.size(); i++) {
		TrackEntry *entry = _tracks[i];
		if (entry) {
			TrackEntry *from = entry->_mixingFrom;
			while (from) {
				TrackEntry *curr = from;
				from = curr->_mixingFrom;
				delete curr;
			}
			TrackEntry *next = entry->_next;
			while (next) {
				TrackEntry *curr = next;
				next = curr->_next;
				delete curr;
			}
			delete entry;
		}
	}
	delete _queue;
}

void AnimationState::update(float delta) {
	delta *= _timeScale;
	for (size_t i = 0, n = _tracks.size(); i < n; ++i) {
		TrackEntry *currentP = _tracks[i];
		if (currentP == NULL) {
			continue;
		}

		TrackEntry &current = *currentP;

		current._animationLast = current._nextAnimationLast;
		current._trackLast = current._nextTrackLast;

		float currentDelta = delta * current._timeScale;

		if (current._delay > 0) {
			current._delay -= currentDelta;
			if (current._delay > 0) {
				continue;
			}
			currentDelta = -current._delay;
			current._delay = 0;
		}

		TrackEntry *next = current._next;
		if (next != NULL) {
			// When the next entry's delay is passed, change to the next entry, preserving leftover time.
			float nextTime = current._trackLast - next->_delay;
			if (nextTime >= 0) {
				next->_delay = 0;
				next->_trackTime += current._timeScale == 0 ? 0 : (nextTime / current._timeScale + delta) * next->_timeScale;
				current._trackTime += currentDelta;
				setTrack(i, next, true);
				while (next->_mixingFrom != NULL) {
					next->_mixTime += delta;
					next = next->_mixingFrom;
				}
				continue;
			}
		} else if (current._trackLast >= current._trackEnd && current._mixingFrom == NULL) {
			// clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
			_tracks[i] = NULL;

			_queue->end(currentP);
			clearNext(currentP);
			continue;
		}

		if (current._mixingFrom != NULL && updateMixingFrom(currentP, delta)) {
			// End mixing from entries once all have completed.
			TrackEntry *from = current._mixingFrom;
			current._mixingFrom = NULL;
			if (from != NULL) from->_mixingTo = NULL;
			while (from != NULL) {
				_queue->end(from);
				from = from->_mixingFrom;
			}
		}

		current._trackTime += currentDelta;
	}

	_queue->drain();
}

bool AnimationState::apply(Skeleton &skeleton) {
	if (_animationsChanged) {
		animationsChanged();
	}

	bool applied = false;
	for (size_t i = 0, n = _tracks.size(); i < n; ++i) {
		TrackEntry *currentP = _tracks[i];
		if (currentP == NULL || currentP->_delay > 0) {
			continue;
		}

		TrackEntry &current = *currentP;

		applied = true;

		// Apply mixing from entries first.
		float alpha = current._alpha;
		if (current._mixingFrom != NULL) {
			alpha *= applyMixingFrom(currentP, skeleton);
		} else if (current._trackTime >= current._trackEnd && current._next == NULL) {
			alpha = 0;// Set to setup pose the last time the entry will be applied.
		}

		// Apply current entry.
		float animationLast = current._animationLast, animationTime = current.getAnimationTime();
		float applyTime = animationTime;
		Array<Event *> *applyEvents = &_events;
		if (current._reverse) {
			applyTime = current._animation->getDuration() - applyTime;
			applyEvents = NULL;
		}
		size_t timelineCount = current._animation->_timelines.size();
		Array<Timeline *> &timelines = current._animation->_timelines;
		if (i == 0 && alpha == 1) {
			for (size_t ii = 0; ii < timelineCount; ++ii) {
				Timeline *timeline = timelines[ii];
				if (timeline->getRTTI().isExactly(AttachmentTimeline::rtti))
					applyAttachmentTimeline(static_cast<AttachmentTimeline *>(timeline), skeleton, applyTime, MixFrom_Setup, true);
				else
					timeline->apply(skeleton, animationLast, applyTime, applyEvents, alpha, MixFrom_Setup, false, false, false);
			}
		} else {
			Array<int> &timelineMode = current._timelineMode;
			bool retainAttachments = alpha >= current._alphaAttachmentThreshold;
			bool add = current._additive, shortestRotation = add || current._shortestRotation;
			bool firstFrame = !shortestRotation && current._timelinesRotation.size() != timelines.size() << 1;
			if (firstFrame) current._timelinesRotation.setSize(timelines.size() << 1, 0);
			Array<float> &timelinesRotation = current._timelinesRotation;

			for (size_t ii = 0; ii < timelineCount; ++ii) {
				Timeline *timeline = timelines[ii];
				assert(timeline);

				MixFrom mixFrom = (MixFrom) (timelineMode[ii] & Mode);

				if (!shortestRotation && timeline->getRTTI().isExactly(RotateTimeline::rtti))
					applyRotateTimeline(static_cast<RotateTimeline *>(timeline), skeleton, applyTime, alpha, mixFrom, timelinesRotation, ii << 1,
										firstFrame);
				else if (timeline->getRTTI().isExactly(AttachmentTimeline::rtti))
					applyAttachmentTimeline(static_cast<AttachmentTimeline *>(timeline), skeleton, applyTime, mixFrom, retainAttachments);
				else
					timeline->apply(skeleton, animationLast, applyTime, applyEvents, alpha, mixFrom, add, false, false);
			}
		}

		if (current._reverse) eventsReverse(currentP, animationLast, animationTime);
		queueEvents(currentP, animationTime);
		_events.clear();
		current._nextAnimationLast = animationTime;
		current._nextTrackLast = current._trackTime;
	}

	int setupState = _unkeyedState + AttachSetup;
	Array<Slot *> &slots = skeleton.getSlots();
	for (int i = 0, n = (int) slots.size(); i < n; i++) {
		Slot *slot = slots[i];
		if (slot->_attachmentState == setupState) {
			const String &attachmentName = slot->getData().getAttachmentName();
			slot->_pose.setAttachment(attachmentName.isEmpty() ? NULL : skeleton.getAttachment(slot->getData().getIndex(), attachmentName));
		}
	}
	_unkeyedState += 2;

	_queue->drain();
	return applied;
}

void AnimationState::clearTracks() {
	bool oldDrainDisabled = _queue->_drainDisabled;
	_queue->_drainDisabled = true;
	for (size_t i = 0, n = _tracks.size(); i < n; ++i) clearTrack(i);
	_tracks.clear();
	_queue->_drainDisabled = oldDrainDisabled;
	_queue->drain();
}

void AnimationState::clearTrack(size_t trackIndex) {
	if (trackIndex >= _tracks.size()) return;

	TrackEntry *current = _tracks[trackIndex];
	if (current == NULL) return;

	_queue->end(current);

	clearNext(current);

	TrackEntry *entry = current;
	while (true) {
		TrackEntry *from = entry->_mixingFrom;
		if (from == NULL) break;

		_queue->end(from);
		entry->_mixingFrom = NULL;
		entry->_mixingTo = NULL;
		entry = from;
	}

	_tracks[current->_trackIndex] = NULL;

	_queue->drain();
}

TrackEntry &AnimationState::setAnimation(size_t trackIndex, const String &animationName, bool loop) {
	Animation *animation = _data->_skeletonData->findAnimation(animationName);
	assert(animation != NULL);
	return setAnimation(trackIndex, *animation, loop);
}

TrackEntry &AnimationState::setAnimation(size_t trackIndex, Animation &animation, bool loop) {
	bool interrupt = true;
	TrackEntry *current = expandToIndex(trackIndex);
	if (current != NULL) {
		if (current->_nextTrackLast == -1 && current->_animation == &animation) {
			// Don't mix from an entry that was never applied.
			_tracks[trackIndex] = current->_mixingFrom;
			_queue->interrupt(current);
			_queue->end(current);
			clearNext(current);
			current = current->_mixingFrom;
			interrupt = false;
		} else {
			clearNext(current);
		}
	}

	TrackEntry *entry = newTrackEntry(trackIndex, &animation, loop, current);
	setTrack(trackIndex, entry, interrupt);
	_queue->drain();

	return *entry;
}

TrackEntry &AnimationState::addAnimation(size_t trackIndex, const String &animationName, bool loop, float delay) {
	Animation *animation = _data->_skeletonData->findAnimation(animationName);
	assert(animation != NULL);
	return addAnimation(trackIndex, *animation, loop, delay);
}

TrackEntry &AnimationState::addAnimation(size_t trackIndex, Animation &animation, bool loop, float delay) {
	TrackEntry *last = expandToIndex(trackIndex);
	if (last != NULL) {
		while (last->_next != NULL) last = last->_next;
	}

	TrackEntry *entry = newTrackEntry(trackIndex, &animation, loop, last);

	if (last == NULL) {
		setTrack(trackIndex, entry, true);
		_queue->drain();
		if (delay < 0) delay = 0;
	} else {
		last->_next = entry;
		entry->_previous = last;
		if (delay <= 0) delay = MathUtil::max(delay + last->getTrackComplete() - entry->_mixDuration, 0.0f);
	}

	entry->_delay = delay;
	return *entry;
}

TrackEntry &AnimationState::setEmptyAnimation(size_t trackIndex, float mixDuration) {
	TrackEntry &entry = setAnimation(trackIndex, *AnimationState::getEmptyAnimation(), false);
	entry._mixDuration = mixDuration;
	entry._trackEnd = mixDuration;
	return entry;
}

TrackEntry &AnimationState::addEmptyAnimation(size_t trackIndex, float mixDuration, float delay) {
	TrackEntry &entry = addAnimation(trackIndex, *AnimationState::getEmptyAnimation(), false, delay);
	if (delay <= 0) entry._delay = MathUtil::max(entry._delay + entry._mixDuration - mixDuration, 0.0f);
	entry._mixDuration = mixDuration;
	entry._trackEnd = mixDuration;
	return entry;
}

void AnimationState::setEmptyAnimations(float mixDuration) {
	bool oldDrainDisabled = _queue->_drainDisabled;
	_queue->_drainDisabled = true;
	for (size_t i = 0, n = _tracks.size(); i < n; ++i) {
		TrackEntry *current = _tracks[i];
		if (current != NULL) {
			setEmptyAnimation(i, mixDuration);
		}
	}
	_queue->_drainDisabled = oldDrainDisabled;
	_queue->drain();
}

TrackEntry *AnimationState::getTrack(size_t trackIndex) {
	return trackIndex >= _tracks.size() ? NULL : _tracks[trackIndex];
}

AnimationStateData &AnimationState::getData() {
	return *_data;
}

Array<TrackEntry *> &AnimationState::getTracks() {
	return _tracks;
}

float AnimationState::getTimeScale() {
	return _timeScale;
}

void AnimationState::setTimeScale(float inValue) {
	_timeScale = inValue;
}

#ifdef SPINE_USE_STD_FUNCTION
void AnimationState::setListener(AnimationStateListener inValue) {
	_listener = inValue;
	_listenerObject = NULL;
}
#else
void AnimationState::setListener(AnimationStateListener inValue, void *userData) {
	_listener = inValue;
	_listenerUserData = userData;
	_listenerObject = NULL;
}
#endif

void AnimationState::setListener(AnimationStateListenerObject *inValue) {
	_listener = dummyOnAnimationEventFunc;
	SP_ANIMATION_LISTENER_CLEAR_USER_DATA();
	_listenerObject = inValue;
}

void AnimationState::disableQueue() {
	_queue->_drainDisabled = true;
}

void AnimationState::enableQueue() {
	_queue->_drainDisabled = false;
}

void AnimationState::setManualTrackEntryDisposal(bool inValue) {
	_manualTrackEntryDisposal = inValue;
}

bool AnimationState::getManualTrackEntryDisposal() {
	return _manualTrackEntryDisposal;
}

void AnimationState::disposeTrackEntry(TrackEntry *entry) {
	entry->reset();
	_trackEntryPool.free(entry);
}

Animation *AnimationState::getEmptyAnimation() {
	static Array<Timeline *> timelines;
	static Array<int> bones;
	static Animation ret(String("<empty>"));
	static bool initialized = false;
	if (!initialized) {
		ret.setTimelines(timelines, bones);
		initialized = true;
	}
	return &ret;
}

void AnimationState::applyAttachmentTimeline(AttachmentTimeline *attachmentTimeline, Skeleton &skeleton, float time, MixFrom from, bool retain) {
	Slot *slot = skeleton.getSlots()[attachmentTimeline->getSlotIndex()];
	if (!slot->getBone().isActive()) return;
	if (!retain && slot->_attachmentState == _unkeyedState + AttachRetain) return;

	bool setup = time < attachmentTimeline->getFrames()[0];
	const String *name = NULL;
	if (!setup) {
		name = &attachmentTimeline->getAttachmentNames()[Animation::search(attachmentTimeline->getFrames(), time)];
		setup = !retain && name->isEmpty();
	}
	if (setup) {
		if (from == MixFrom_Current) return;
		name = &slot->getData().getAttachmentName();
	}
	slot->_pose.setAttachment(name->isEmpty() ? NULL : skeleton.getAttachment(slot->getData().getIndex(), *name));
	if (retain)
		slot->_attachmentState = _unkeyedState + AttachRetain;
	else if (!setup)
		slot->_attachmentState = _unkeyedState + AttachSetup;
}


void AnimationState::applyRotateTimeline(RotateTimeline *rotateTimeline, Skeleton &skeleton, float time, float alpha, MixFrom from,
										 Array<float> &timelinesRotation, size_t i, bool firstFrame) {
	if (firstFrame) timelinesRotation[i] = 0;

	if (alpha == 1) {
		static_cast<Timeline *>(rotateTimeline)->apply(skeleton, 0, time, NULL, 1, from, false, false, false);
		return;
	}

	Bone *bone = skeleton._bones[rotateTimeline->_boneIndex];
	if (!bone->isActive()) return;
	BonePose &pose = bone->_pose;
	BonePose &setup = bone->_data._setupPose;
	Array<float> &frames = rotateTimeline->_frames;
	float r1, r2;
	if (time < frames[0]) {
		switch (from) {
			case MixFrom_Setup:
				pose._rotation = setup._rotation;
				return;
			case MixFrom_Current:
				return;
			case MixFrom_First:
				break;
		}
		r1 = pose._rotation;
		r2 = setup._rotation;
	} else {
		r1 = from == MixFrom_Setup ? setup._rotation : pose._rotation;
		r2 = setup._rotation + rotateTimeline->getCurveValue(time);
	}

	// Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
	float total, diff = r2 - r1;
	diff -= MathUtil::ceil(diff / 360 - 0.5) * 360;
	if (diff == 0) {
		total = timelinesRotation[i];
	} else {
		float lastTotal, lastDiff;
		if (firstFrame) {
			lastTotal = 0;
			lastDiff = diff;
		} else {
			lastTotal = timelinesRotation[i];
			lastDiff = timelinesRotation[i + 1];
		}
		float loops = lastTotal - MathUtil::fmod(lastTotal, 360.f);
		total = diff + loops;
		bool current = diff >= 0, dir = lastTotal >= 0;
		if (MathUtil::abs(lastDiff) <= 90 && MathUtil::sign(lastDiff) != MathUtil::sign(diff)) {
			if (MathUtil::abs(lastTotal - loops) > 180) {
				total += 360.f * MathUtil::sign(lastTotal);
				dir = current;
			} else if (loops != 0)
				total -= 360.f * MathUtil::sign(lastTotal);
			else
				dir = current;
		}
		if (dir != current) {
			total += 360 * MathUtil::sign(lastTotal);
		}
		timelinesRotation[i] = total;
	}
	timelinesRotation[i + 1] = diff;
	pose._rotation = r1 + total * alpha;
}

bool AnimationState::updateMixingFrom(TrackEntry *to, float delta) {
	TrackEntry *from = to->_mixingFrom;
	if (from == NULL) {
		return true;
	}

	bool finished = updateMixingFrom(from, delta);

	from->_animationLast = from->_nextAnimationLast;
	from->_trackLast = from->_nextTrackLast;

	// The from entry was applied at least once and the mix is complete.
	if (to->_nextTrackLast != -1 && to->_mixTime >= to->_mixDuration) {
		// Mixing is complete for all entries before the from entry or the mix is instantaneous.
		if (from->_totalAlpha == 0 || to->_mixDuration == 0) {
			to->_mixingFrom = from->_mixingFrom;
			if (from->_mixingFrom) from->_mixingFrom->_mixingTo = to;
			if (from->_totalAlpha == 0) {
				for (TrackEntry *next = to; next->_mixingTo != NULL; next = next->_mixingTo) next->_keepHold = true;
			}
			_queue->end(from);
		}
		return finished;
	}

	from->_trackTime += delta * from->_timeScale;
	to->_mixTime += delta;

	return false;
}

float AnimationState::applyMixingFrom(TrackEntry *to, Skeleton &skeleton) {
	TrackEntry *from = to->_mixingFrom;
	float fromMix = from->_mixingFrom != NULL ? applyMixingFrom(from, skeleton) : 1;
	float mix = to->mix();

	float a = from->_alpha * fromMix, keep = 1 - mix * to->_alpha;
	float alphaMix = a * (1 - mix), alphaHold = keep > 0 ? alphaMix / keep : a;

	Array<Timeline *> &timelines = from->_animation->_timelines;
	size_t timelineCount = timelines.size();
	Array<int> &timelineMode = from->_timelineMode;
	Array<TrackEntry *> &timelineHoldMix = from->_timelineHoldMix;

	bool retainAttachments = mix < from->_mixAttachmentThreshold, drawOrder = mix < from->_mixDrawOrderThreshold;
	bool add = from->_additive, shortestRotation = add || from->_shortestRotation;
	bool firstFrame = !shortestRotation && from->_timelinesRotation.size() != timelines.size() << 1;
	if (firstFrame) from->_timelinesRotation.setSize(timelines.size() << 1, 0);
	Array<float> &timelinesRotation = from->_timelinesRotation;

	float animationLast = from->_animationLast, animationTime = from->getAnimationTime();
	float applyTime = animationTime;
	Array<Event *> *events = NULL;
	if (from->_reverse)
		applyTime = from->_animation->_duration - applyTime;
	else if (mix < from->_eventThreshold)
		events = &_events;

	from->_totalAlpha = 0;
	for (size_t i = 0; i < timelineCount; i++) {
		Timeline *timeline = timelines[i];
		int mode = timelineMode[i];
		MixFrom mixFrom = (MixFrom) (mode & Mode);
		float alpha;
		if ((mode & Hold) != 0) {
			TrackEntry *holdMix = timelineHoldMix[i];
			alpha = holdMix == NULL ? alphaHold : alphaHold * (1 - holdMix->mix());
		} else {
			if (!drawOrder && timeline->getRTTI().isExactly(DrawOrderTimeline::rtti) && mixFrom == MixFrom_Current) continue;
			alpha = alphaMix;
		}
		from->_totalAlpha += alpha;
		if (!shortestRotation && timeline->getRTTI().isExactly(RotateTimeline::rtti)) {
			applyRotateTimeline((RotateTimeline *) timeline, skeleton, applyTime, alpha, mixFrom, timelinesRotation, i << 1, firstFrame);
		} else if (timeline->getRTTI().isExactly(AttachmentTimeline::rtti)) {
			applyAttachmentTimeline(static_cast<AttachmentTimeline *>(timeline), skeleton, applyTime, mixFrom,
									retainAttachments && alpha >= from->_alphaAttachmentThreshold);
		} else {
			bool out = !drawOrder || !timeline->getRTTI().isExactly(DrawOrderTimeline::rtti) || mixFrom == MixFrom_Current;
			timeline->apply(skeleton, animationLast, applyTime, events, alpha, mixFrom, add, out, false);
		}
	}

	if (from->_reverse && mix < from->_eventThreshold) eventsReverse(from, animationLast, animationTime);
	if (to->_mixDuration > 0) {
		queueEvents(from, animationTime);
	}

	_events.clear();

	from->_nextAnimationLast = animationTime;
	from->_nextTrackLast = from->_trackTime;
	return mix;
}

void AnimationState::queueEvents(TrackEntry *entry, float animationTime) {
	float animationStart = entry->_animationStart, animationEnd = entry->_animationEnd, duration = animationEnd - animationStart;
	bool reverse = entry->_reverse;
	float split = duration != 0 ? MathUtil::fmod(entry->_trackLast, duration) : 0;
	if (reverse) split = duration - split;

	// Queue events before complete.
	size_t i = 0, n = _events.size();
	for (; i < n; ++i) {
		Event *e = _events[i];
		if ((e->_time < split) != reverse) break;
		if (e->_time >= animationStart && e->_time <= animationEnd) _queue->event(entry, e);
	}

	// Queue complete if completed a loop iteration or the animation.
	bool complete = false;
	if (entry->_loop) {
		if (duration == 0)
			complete = true;
		else {
			int cycles = (int) (entry->_trackTime / duration);
			complete = cycles > 0 && cycles > (int) (entry->_trackLast / duration);
		}
	} else {
		complete = animationTime >= animationEnd && entry->_animationLast < animationEnd;
	}
	if (complete) _queue->complete(entry);

	// Queue events after complete.
	for (; i < n; ++i) {
		Event *e = _events[i];
		if (e->_time >= animationStart && e->_time <= animationEnd) _queue->event(entry, e);
	}
}

void AnimationState::eventsReverse(TrackEntry *entry, float animationLast, float animationTime) {
	float duration = entry->_animation->getDuration(), from = duration - animationLast, to = duration - animationTime;
	Array<Timeline *> &timelines = entry->_animation->_timelines;
	for (size_t i = 0, n = timelines.size(); i < n; i++) {
		if (!timelines[i]->getRTTI().isExactly(EventTimeline::rtti)) continue;
		EventTimeline *eventTimeline = static_cast<EventTimeline *>(timelines[i]);
		Array<Event *> &timelineEvents = eventTimeline->getEvents();
		Array<float> &frames = eventTimeline->getFrames();
		int frameCount = (int) frames.size();
		if (from >= to) {
			for (int ii = 0; ii < frameCount; ii++) {
				if (frames[ii] < to) continue;
				if (frames[ii] >= from) break;
				_events.add(timelineEvents[ii]);
			}
		} else {
			for (int ii = 0; ii < frameCount; ii++) {
				if (frames[ii] >= from) break;
				_events.add(timelineEvents[ii]);
			}
			int ii = 0;
			for (; ii < frameCount; ii++)
				if (frames[ii] >= to) break;
			for (; ii < frameCount; ii++) _events.add(timelineEvents[ii]);
		}
	}
}

void AnimationState::setTrack(size_t index, TrackEntry *current, bool interrupt) {
	TrackEntry *from = expandToIndex(index);
	_tracks[index] = current;
	current->_previous = NULL;

	if (from != NULL) {
		from->_next = NULL;
		if (interrupt) _queue->interrupt(from);

		current->_mixingFrom = from;
		from->_mixingTo = current;
		current->_mixTime = 0;
		from->_timelinesRotation.clear();// Reset rotation for mixing out, in case entry was mixed in.
	}

	_queue->start(current);// triggers animationsChanged
}

TrackEntry *AnimationState::expandToIndex(size_t index) {
	if (index < _tracks.size()) return _tracks[index];
	while (index >= _tracks.size()) _tracks.add(NULL);
	return NULL;
}

TrackEntry *AnimationState::newTrackEntry(size_t trackIndex, Animation *animation, bool loop, TrackEntry *last) {
	TrackEntry *entryP = _trackEntryPool.obtain();// Pooling
	TrackEntry &entry = *entryP;
	entry.setAnimationState(this);

	entry._trackIndex = (int) trackIndex;
	entry._animation = animation;
	entry._loop = loop;

	entry._additive = false;
	entry._reverse = false;
	entry._shortestRotation = false;

	entry._eventThreshold = 0;
	entry._alphaAttachmentThreshold = 0;
	entry._mixAttachmentThreshold = 0;
	entry._mixDrawOrderThreshold = 0;

	entry._animationStart = 0;
	entry._animationEnd = animation->getDuration();
	entry._animationLast = -1;
	entry._nextAnimationLast = -1;

	entry._delay = 0;
	entry._trackTime = 0;
	entry._trackLast = -1;
	entry._nextTrackLast = -1;// nextTrackLast == -1 signifies a TrackEntry that wasn't applied yet.
	entry._trackEnd = FLT_MAX;// loop ? float.MaxValue : animation.Duration;
	entry._timeScale = 1;

	entry._alpha = 1;
	entry._mixTime = 0;
	entry._mixDuration = (last == NULL) ? 0 : _data->getMix(*last->_animation, *animation);
	entry._mixInterpolation = &Interpolation::linear();
	entry._totalAlpha = 0;
	entry._keepHold = false;

	return entryP;
}

void AnimationState::clearNext(TrackEntry *entry) {
	TrackEntry *next = entry->_next;
	while (next != NULL) {
		_queue->dispose(next);
		next = next->_next;
	}
	entry->_next = NULL;
}

void AnimationState::animationsChanged() {
	_animationsChanged = false;

	for (size_t i = 0, n = _tracks.size(); i < n; ++i) {
		TrackEntry *track = _tracks[i];
		if (!track) continue;
		TrackEntry *entry = track;

		while (entry->_mixingFrom != NULL) entry = entry->_mixingFrom;

		do {
			computeHold(entry, track);
			entry = entry->_mixingTo;
		} while (entry != NULL);
	}
	_propertyIDs.clear();
}

void AnimationState::computeHold(TrackEntry *entry, TrackEntry *track) {
	Array<Timeline *> &timelines = entry->_animation->_timelines;
	size_t timelinesCount = timelines.size();
	Array<int> &timelineMode = entry->_timelineMode;
	timelineMode.setSize(timelinesCount, 0);
	entry->_timelineHoldMix.clear();
	Array<TrackEntry *> &timelineHoldMix = entry->_timelineHoldMix;
	timelineHoldMix.setSize(timelinesCount, NULL);
	bool add = entry->_additive, keepHold = entry->_keepHold;
	TrackEntry *to = entry->_mixingTo;

	for (size_t i = 0; i < timelinesCount; ++i) {
		Timeline *timeline = timelines[i];
		Array<PropertyId> &ids = timeline->getPropertyIds();
		int mixFrom = from(track, timeline, ids);

		if (add && timeline->getAdditive()) {
			timelineMode[i] = mixFrom;
			continue;
		}

		// Hold if the next entry will overwrite this property.
		int mode;
		if (to == NULL || timeline->getInstant() || (to->_additive && timeline->getAdditive()) || !to->_animation->hasTimeline(ids))
			mode = mixFrom;
		else {
			mode = mixFrom | Hold;
			// Find next entry that doesn't overwrite this property. Its mix fades out the hold.
			for (TrackEntry *next = to->_mixingTo; next != NULL; next = next->_mixingTo) {
				if ((next->_additive && timeline->getAdditive()) || !next->_animation->hasTimeline(ids)) {
					if (next->_mixDuration > 0) timelineHoldMix[i] = next;
					break;
				}
			}
		}
		if (keepHold) mode = (mode & ~Hold) | (timelineMode[i] & Hold);
		timelineMode[i] = mode;
	}
}

int AnimationState::from(TrackEntry *track, Timeline *timeline, Array<PropertyId> &ids) {
	int mixFrom = Setup;
	for (size_t i = 0, n = ids.size(); i < n; i++) {
		TrackEntry *owner = _propertyIDs.putMissing(ids[i], track);
		if (owner != NULL) {
			if (owner != track) {
				while (++i < n) _propertyIDs.putMissing(ids[i], track);
				return Current;
			}
			mixFrom = First;
		}
	}
	if (timeline->getRTTI().isExactly(DrawOrderFolderTimeline::rtti)) {
		TrackEntry **first = _propertyIDs.get(DrawOrderTimeline::getPropertyId());
		if (first != NULL) return *first != track ? Current : First;
	}
	return mixFrom;
}
