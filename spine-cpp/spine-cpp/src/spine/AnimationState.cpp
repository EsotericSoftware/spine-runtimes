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

#include <spine/AnimationState.h>

#include <spine/Animation.h>
#include <spine/Event.h>
#include <spine/AnimationStateData.h>
#include <spine/Skeleton.h>
#include <spine/RotateTimeline.h>

#include <spine/Timeline.h>
#include <spine/SkeletonData.h>
#include <spine/Bone.h>
#include <spine/BoneData.h>
#include <spine/AttachmentTimeline.h>
#include <spine/DrawOrderTimeline.h>

#include <spine/MathUtil.h>
#include <spine/ContainerUtil.h>

namespace Spine {
    void dummyOnAnimationEventFunc(AnimationState* state, EventType type, TrackEntry* entry, Event* event = NULL) {
        // Empty
    }
    
    TrackEntry::TrackEntry() : _animation(NULL), _next(NULL), _mixingFrom(NULL), _trackIndex(0), _loop(false), _eventThreshold(0), _attachmentThreshold(0), _drawOrderThreshold(0), _animationStart(0), _animationEnd(0), _animationLast(0), _nextAnimationLast(0), _delay(0), _trackTime(0), _trackLast(0), _nextTrackLast(0), _trackEnd(0), _timeScale(1.0f), _alpha(0), _mixTime(0), _mixDuration(0), _interruptAlpha(0), _totalAlpha(0), _onAnimationEventFunc(dummyOnAnimationEventFunc) {
        // Empty
    }
    
    int TrackEntry::getTrackIndex() { return _trackIndex; }
    
    Animation* TrackEntry::getAnimation() { return _animation; }
    
    bool TrackEntry::getLoop() { return _loop; }
    void TrackEntry::setLoop(bool inValue) { _loop = inValue; }

    float TrackEntry::getDelay() { return _delay; }
    void TrackEntry::setDelay(float inValue) { _delay = inValue; }

    float TrackEntry::getTrackTime() { return _trackTime; }
    void TrackEntry::setTrackTime(float inValue) { _trackTime = inValue; }

    float TrackEntry::getTrackEnd() { return _trackEnd; }
    void TrackEntry::setTrackEnd(float inValue) { _trackEnd = inValue; }

    float TrackEntry::getAnimationStart() { return _animationStart; }
    void TrackEntry::setAnimationStart(float inValue) { _animationStart = inValue; }

    float TrackEntry::getAnimationEnd() { return _animationEnd; }
    void TrackEntry::setAnimationEnd(float inValue) { _animationEnd = inValue; }

    float TrackEntry::getAnimationLast() { return _animationLast; }
    void TrackEntry::setAnimationLast(float inValue) {
        _animationLast = inValue;
        _nextAnimationLast = inValue;
    }

    float TrackEntry::getAnimationTime() {
        if (_loop) {
            float duration = _animationEnd - _animationStart;
            if (duration == 0) {
                return _animationStart;
            }

            return fmodf(_trackTime, duration) + _animationStart;
        }

        return MIN(_trackTime + _animationStart, _animationEnd);
    }

    float TrackEntry::getTimeScale() { return _timeScale; }
    void TrackEntry::setTimeScale(float inValue) { _timeScale = inValue; }

    float TrackEntry::getAlpha() { return _alpha; }
    void TrackEntry::setAlpha(float inValue) { _alpha = inValue; }

    float TrackEntry::getEventThreshold() { return _eventThreshold; }
    void TrackEntry::setEventThreshold(float inValue) { _eventThreshold = inValue; }

    float TrackEntry::getAttachmentThreshold() { return _attachmentThreshold; }
    void TrackEntry::setAttachmentThreshold(float inValue) { _attachmentThreshold = inValue; }

    float TrackEntry::getDrawOrderThreshold() { return _drawOrderThreshold; }
    void TrackEntry::setDrawOrderThreshold(float inValue) { _drawOrderThreshold = inValue; }

    TrackEntry* TrackEntry::getNext() { return _next; }

    bool TrackEntry::isComplete() {
        return _trackTime >= _animationEnd - _animationStart;
    }

    float TrackEntry::getMixTime() { return _mixTime; }
    void TrackEntry::setMixTime(float inValue) { _mixTime = inValue; }

    float TrackEntry::getMixDuration() { return _mixDuration; }
    void TrackEntry::setMixDuration(float inValue) { _mixDuration = inValue; }

    TrackEntry* TrackEntry::getMixingFrom() { return _mixingFrom; }
    
    void TrackEntry::resetRotationDirections() {
        _timelinesRotation.clear();
    }
    
    void TrackEntry::setOnAnimationEventFunc(OnAnimationEventFunc inValue) {
        _onAnimationEventFunc = inValue;
    }
    
    TrackEntry* TrackEntry::setTimelineData(TrackEntry* to, Vector<TrackEntry*>& mixingToArray, Vector<int>& propertyIDs) {
        if (to != NULL) {
            mixingToArray.push_back(to);
        }
        
        TrackEntry* lastEntry = _mixingFrom != NULL ? _mixingFrom->setTimelineData(this, mixingToArray, propertyIDs) : this;
        
        if (to != NULL) {
            mixingToArray.erase(mixingToArray.size() - 1);
        }
        
        int mixingToLast = static_cast<int>(mixingToArray.size()) - 1;
        Vector<Timeline*>& timelines = _animation->_timelines;
        int timelinesCount = static_cast<int>(timelines.size());
        _timelineData.reserve(timelinesCount);
        _timelineData.setSize(timelinesCount);
        _timelineDipMix.clear();
        _timelineDipMix.reserve(timelinesCount);
        _timelineDipMix.setSize(timelinesCount);
        
        // outer:
        for (int i = 0; i < timelinesCount; ++i) {
            int id = timelines[i]->getPropertyId();
            if (propertyIDs.contains(id)) {
                _timelineData[i] = AnimationState::Subsequent;
            }
            else {
                propertyIDs.push_back(id);
                
                if (to == NULL || !to->hasTimeline(id)) {
                    _timelineData[i] = AnimationState::First;
                }
                else {
                    for (int ii = mixingToLast; ii >= 0; --ii) {
                        TrackEntry* entry = mixingToArray[ii];
                        if (!entry->hasTimeline(id)) {
                            if (entry->_mixDuration > 0) {
                                _timelineData[i] = AnimationState::DipMix;
                                _timelineDipMix[i] = entry;
                                goto continue_outer; // continue outer;
                            }
                            break;
                        }
                    }
                    _timelineData[i] = AnimationState::Dip;
                }
            }
        continue_outer: {}
        }
        
        return lastEntry;
    }
    
    bool TrackEntry::hasTimeline(int inId) {
        Vector<Timeline*>& timelines = _animation->_timelines;
        for (int i = 0, n = static_cast<int>(timelines.size()); i < n; ++i) {
            if (timelines[i]->getPropertyId() == inId) {
                return true;
            }
        }
        return false;
    }
    
    void TrackEntry::reset() {
        _animation = NULL;
        _next = NULL;
        _mixingFrom = NULL;
        
        _timelineData.clear();
        _timelineDipMix.clear();
        _timelinesRotation.clear();
        
        _onAnimationEventFunc = dummyOnAnimationEventFunc;
    }
    
    EventQueueEntry::EventQueueEntry(EventType eventType, TrackEntry* trackEntry, Event* event) :
    _type(eventType),
    _entry(trackEntry),
    _event(event) {
        // Empty
    }
    
    EventQueue* EventQueue::newEventQueue(AnimationState& state, Pool<TrackEntry>& trackEntryPool) {
        EventQueue* ret = NEW(EventQueue);
        new (ret) EventQueue(state, trackEntryPool);
        
        return ret;
    }
    
    EventQueueEntry* EventQueue::newEventQueueEntry(EventType eventType, TrackEntry* entry, Event* event) {
        EventQueueEntry* ret = NEW(EventQueueEntry);
        new (ret) EventQueueEntry(eventType, entry, event);
        
        return ret;
    }
    
    EventQueue::EventQueue(AnimationState& state, Pool<TrackEntry>& trackEntryPool) : _state(state), _trackEntryPool(trackEntryPool), _drainDisabled(false) {
        // Empty
    }
    
    EventQueue::~EventQueue() {
        ContainerUtil::cleanUpVectorOfPointers(_eventQueueEntries);
    }
    
    void EventQueue::start(TrackEntry* entry) {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Start, entry));
        _state._animationsChanged = true;
    }
    
    void EventQueue::interrupt(TrackEntry* entry) {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Interrupt, entry));
    }
    
    void EventQueue::end(TrackEntry* entry) {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_End, entry));
        _state._animationsChanged = true;
    }
    
    void EventQueue::dispose(TrackEntry* entry) {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Dispose, entry));
    }
    
    void EventQueue::complete(TrackEntry* entry) {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Complete, entry));
    }
    
    void EventQueue::event(TrackEntry* entry, Event* event) {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Event, entry, event));
    }
    
    /// Raises all events in the queue and drains the queue.
    void EventQueue::drain() {
        if (_drainDisabled) {
            return;
        }
        
        _drainDisabled = true;
        
        AnimationState& state = _state;
        
        // Don't cache _eventQueueEntries.size() so callbacks can queue their own events (eg, call setAnimation in AnimationState_Complete).
        for (int i = 0; i < _eventQueueEntries.size(); ++i) {
            EventQueueEntry* queueEntry = _eventQueueEntries[i];
            TrackEntry* trackEntry = queueEntry->_entry;
            
            switch (queueEntry->_type) {
                case EventType_Start:
                case EventType_Interrupt:
                case EventType_Complete:
                    trackEntry->_onAnimationEventFunc(&state, queueEntry->_type, trackEntry, NULL);
                    state._onAnimationEventFunc(&state, queueEntry->_type, trackEntry, NULL);
                    break;
                case EventType_End:
                    trackEntry->_onAnimationEventFunc(&state, queueEntry->_type, trackEntry, NULL);
                    state._onAnimationEventFunc(&state, queueEntry->_type, trackEntry, NULL);
                    /* Yes, we want to fall through here */
                case EventType_Dispose:
                    trackEntry->_onAnimationEventFunc(&state, EventType_Dispose, trackEntry, NULL);
                    state._onAnimationEventFunc(&state, EventType_Dispose, trackEntry, NULL);
                    trackEntry->reset();
                    _trackEntryPool.free(trackEntry);
                    break;
                case EventType_Event:
                    trackEntry->_onAnimationEventFunc(&state, queueEntry->_type, trackEntry, queueEntry->_event);
                    state._onAnimationEventFunc(&state, queueEntry->_type, trackEntry, queueEntry->_event);
                    break;
            }
        }
        _eventQueueEntries.clear();
        
        _drainDisabled = false;
    }
    
    const int AnimationState::Subsequent = 0;
    const int AnimationState::First = 1;
    const int AnimationState::Dip = 2;
    const int AnimationState::DipMix = 3;
    
    AnimationState::AnimationState(AnimationStateData& data) :
    _data(data),
    _queue(EventQueue::newEventQueue(*this, _trackEntryPool)),
    _animationsChanged(false),
    _onAnimationEventFunc(dummyOnAnimationEventFunc),
    _timeScale(1) {
        // Empty
    }
    
    AnimationState::~AnimationState() {
        DESTROY(EventQueue, _queue);
    }
    
    void AnimationState::update(float delta) {
        delta *= _timeScale;
        for (int i = 0, n = static_cast<int>(_tracks.size()); i < n; ++i) {
            TrackEntry* currentP = _tracks[i];
            if (currentP == NULL) {
                continue;
            }
            
            TrackEntry& current = *currentP;
            
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
            
            TrackEntry* next = current._next;
            if (next != NULL) {
                // When the next entry's delay is passed, change to the next entry, preserving leftover time.
                float nextTime = current._trackLast - next->_delay;
                if (nextTime >= 0) {
                    next->_delay = 0;
                    next->_trackTime = nextTime + (delta * next->_timeScale);
                    current._trackTime += currentDelta;
                    setCurrent(i, next, true);
                    while (next->_mixingFrom != NULL) {
                        next->_mixTime += currentDelta;
                        next = next->_mixingFrom;
                    }
                    continue;
                }
            }
            else if (current._trackLast >= current._trackEnd && current._mixingFrom == NULL) {
                // clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
                _tracks[i] = NULL;
                
                _queue->end(currentP);
                disposeNext(currentP);
                continue;
            }
            
            if (current._mixingFrom != NULL && updateMixingFrom(currentP, delta)) {
                // End mixing from entries once all have completed.
                TrackEntry* from = current._mixingFrom;
                current._mixingFrom = NULL;
                while (from != NULL) {
                    _queue->end(from);
                    from = from->_mixingFrom;
                }
            }
            
            current._trackTime += currentDelta;
        }
        
        _queue->drain();
    }
    
    bool AnimationState::apply(Skeleton& skeleton) {
        if (_animationsChanged) {
            animationsChanged();
        }
        
        bool applied = false;
        for (int i = 0, n = static_cast<int>(_tracks.size()); i < n; ++i) {
            TrackEntry* currentP = _tracks[i];
            if (currentP == NULL || currentP->_delay > 0) {
                continue;
            }
            
            TrackEntry& current = *currentP;
            
            applied = true;
            MixPose currentPose = i == 0 ? MixPose_Current : MixPose_CurrentLayered;
            
            // apply mixing from entries first.
            float mix = current._alpha;
            if (current._mixingFrom != NULL) {
                mix *= applyMixingFrom(currentP, skeleton, currentPose);
            }
            else if (current._trackTime >= current._trackEnd && current._next == NULL) {
                mix = 0; // Set to setup pose the last time the entry will be applied.
            }
            
            // apply current entry.
            float animationLast = current._animationLast, animationTime = current.getAnimationTime();
            int timelineCount = static_cast<int>(current._animation->_timelines.size());
            Vector<Timeline*>& timelines = current._animation->_timelines;
            if (mix == 1) {
                for (int ii = 0; ii < timelineCount; ++ii) {
                    timelines[ii]->apply(skeleton, animationLast, animationTime, &_events, 1, MixPose_Setup, MixDirection_In);
                }
            }
            else {
                Vector<int>& timelineData = current._timelineData;
                
                bool firstFrame = current._timelinesRotation.size() == 0;
                if (firstFrame) {
                    current._timelinesRotation.reserve(timelines.size() << 1);
                    current._timelinesRotation.setSize(timelines.size() << 1);
                }
                Vector<float>& timelinesRotation = current._timelinesRotation;
                
                for (int ii = 0; ii < timelineCount; ++ii) {
                    Timeline* timeline = timelines[ii];
                    assert(timeline);
                    
                    MixPose pose = timelineData[ii] >= AnimationState::First ? MixPose_Setup : currentPose;
                    
                    RotateTimeline* rotateTimeline = NULL;
                    if (timeline->getRTTI().derivesFrom(RotateTimeline::rtti)) {
                        rotateTimeline = static_cast<RotateTimeline*>(timeline);
                    }
                    
                    if (rotateTimeline != NULL) {
                        applyRotateTimeline(rotateTimeline, skeleton, animationTime, mix, pose, timelinesRotation, ii << 1, firstFrame);
                    }
                    else {
                        timeline->apply(skeleton, animationLast, animationTime, &_events, mix, pose, MixDirection_In);
                    }
                }
            }
            
            queueEvents(currentP, animationTime);
            _events.clear();
            current._nextAnimationLast = animationTime;
            current._nextTrackLast = current._trackTime;
        }
        
        _queue->drain();
        return applied;
    }
    
    void AnimationState::clearTracks() {
        bool oldDrainDisabled = _queue->_drainDisabled;
        _queue->_drainDisabled = true;
        for (int i = 0, n = static_cast<int>(_tracks.size()); i < n; ++i) {
            clearTrack(i);
        }
        _tracks.clear();
        _queue->_drainDisabled = oldDrainDisabled;
        _queue->drain();
    }
    
    void AnimationState::clearTrack(int trackIndex) {
        if (trackIndex >= _tracks.size()) {
            return;
        }
        
        TrackEntry* current = _tracks[trackIndex];
        if (current == NULL) {
            return;
        }
        
        _queue->end(current);
        
        disposeNext(current);
        
        TrackEntry* entry = current;
        while (true) {
            TrackEntry* from = entry->_mixingFrom;
            if (from == NULL) {
                break;
            }
            
            _queue->end(from);
            entry->_mixingFrom = NULL;
            entry = from;
        }
        
        _tracks[current->_trackIndex] = NULL;
        
        _queue->drain();
    }
    
    TrackEntry* AnimationState::setAnimation(int trackIndex, std::string animationName, bool loop) {
        Animation* animation = _data._skeletonData.findAnimation(animationName);
        assert(animation != NULL);
        
        return setAnimation(trackIndex, animation, loop);
    }
    
    TrackEntry* AnimationState::setAnimation(int trackIndex, Animation* animation, bool loop) {
        assert(animation != NULL);
        
        bool interrupt = true;
        TrackEntry* current = expandToIndex(trackIndex);
        if (current != NULL) {
            if (current->_nextTrackLast == -1) {
                // Don't mix from an entry that was never applied.
                _tracks[trackIndex] = current->_mixingFrom;
                _queue->interrupt(current);
                _queue->end(current);
                disposeNext(current);
                current = current->_mixingFrom;
                interrupt = false;
            }
            else {
                disposeNext(current);
            }
        }
        
        TrackEntry* entry = newTrackEntry(trackIndex, animation, loop, current);
        setCurrent(trackIndex, entry, interrupt);
        _queue->drain();
        
        return entry;
    }
    
    TrackEntry* AnimationState::addAnimation(int trackIndex, std::string animationName, bool loop, float delay) {
        Animation* animation = _data._skeletonData.findAnimation(animationName);
        assert(animation != NULL);
        
        return addAnimation(trackIndex, animation, loop, delay);
    }
    
    TrackEntry* AnimationState::addAnimation(int trackIndex, Animation* animation, bool loop, float delay) {
        assert(animation != NULL);
        
        TrackEntry* last = expandToIndex(trackIndex);
        if (last != NULL) {
            while (last->_next != NULL) {
                last = last->_next;
            }
        }
        
        TrackEntry* entry = newTrackEntry(trackIndex, animation, loop, last);
        
        if (last == NULL) {
            setCurrent(trackIndex, entry, true);
            _queue->drain();
        }
        else {
            last->_next = entry;
            if (delay <= 0) {
                float duration = last->_animationEnd - last->_animationStart;
                if (duration != 0) {
                    if (last->_loop) {
                        delay += duration * (1 + (int)(last->_trackTime / duration));
                    } else {
                        delay += duration;
                    }
                    delay -= _data.getMix(last->_animation, animation);
                } else {
                    delay = 0;
                }
            }
        }
        
        entry->_delay = delay;
        return entry;
    }
    
    TrackEntry* AnimationState::setEmptyAnimation(int trackIndex, float mixDuration) {
        TrackEntry* entry = setAnimation(trackIndex, AnimationState::getEmptyAnimation(), false);
        entry->_mixDuration = mixDuration;
        entry->_trackEnd = mixDuration;
        return entry;
    }
    
    TrackEntry* AnimationState::addEmptyAnimation(int trackIndex, float mixDuration, float delay) {
        if (delay <= 0) {
            delay -= mixDuration;
        }
        
        TrackEntry* entry = addAnimation(trackIndex, AnimationState::getEmptyAnimation(), false, delay);
        entry->_mixDuration = mixDuration;
        entry->_trackEnd = mixDuration;
        return entry;
    }
    
    void AnimationState::setEmptyAnimations(float mixDuration) {
        bool oldDrainDisabled = _queue->_drainDisabled;
        _queue->_drainDisabled = true;
        for (int i = 0, n = static_cast<int>(_tracks.size()); i < n; ++i) {
            TrackEntry* current = _tracks[i];
            if (current != NULL) {
                setEmptyAnimation(i, mixDuration);
            }
        }
        _queue->_drainDisabled = oldDrainDisabled;
        _queue->drain();
    }
    
    TrackEntry* AnimationState::getCurrent(int trackIndex) {
        return trackIndex >= _tracks.size() ? NULL : _tracks[trackIndex];
    }
    
    AnimationStateData& AnimationState::getData() {
        return _data;
    }
    
    Vector<TrackEntry*> AnimationState::getTracks() {
        return _tracks;
    }
    
    float AnimationState::getTimeScale() {
        return _timeScale;
    }
    
    void AnimationState::setTimeScale(float inValue) {
        _timeScale = inValue;
    }
    
    void AnimationState::setOnAnimationEventFunc(OnAnimationEventFunc inValue) {
        _onAnimationEventFunc = inValue;
    }
    
    void AnimationState::setRendererObject(void* inValue) {
        _rendererObject = inValue;
    }
    
    void* AnimationState::getRendererObject() {
        return _rendererObject;
    }
    
    Animation* AnimationState::getEmptyAnimation() {
        static Vector<Timeline*> timelines;
        static Animation ret(std::string("<empty>"), timelines, 0);
        return &ret;
    }
    
    void AnimationState::applyRotateTimeline(RotateTimeline* rotateTimeline, Skeleton& skeleton, float time, float alpha, MixPose pose, Vector<float>& timelinesRotation, int i, bool firstFrame) {
        if (firstFrame) {
            timelinesRotation[i] = 0;
        }
        
        if (alpha == 1) {
            rotateTimeline->apply(skeleton, 0, time, NULL, 1, pose, MixDirection_In);
            return;
        }
        
        Bone* bone = skeleton._bones[rotateTimeline->_boneIndex];
        Vector<float> frames = rotateTimeline->_frames;
        if (time < frames[0]) {
            if (pose == MixPose_Setup) {
                bone->_rotation = bone->_data._rotation;
            }
            return;
        }
        
        float r2;
        if (time >= frames[frames.size() - RotateTimeline::ENTRIES]) {
            // Time is after last frame.
            r2 = bone->_data._rotation + frames[frames.size() + RotateTimeline::PREV_ROTATION];
        }
        else {
            // Interpolate between the previous frame and the current frame.
            int frame = Animation::binarySearch(frames, time, RotateTimeline::ENTRIES);
            float prevRotation = frames[frame + RotateTimeline::PREV_ROTATION];
            float frameTime = frames[frame];
            float percent = rotateTimeline->getCurvePercent((frame >> 1) - 1, 1 - (time - frameTime) / (frames[frame + RotateTimeline::PREV_TIME] - frameTime));
            
            r2 = frames[frame + RotateTimeline::ROTATION] - prevRotation;
            r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
            r2 = prevRotation + r2 * percent + bone->_data._rotation;
            r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
        }
        
        // Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
        float r1 = pose == MixPose_Setup ? bone->_data._rotation : bone->_rotation;
        float total, diff = r2 - r1;
        if (diff == 0) {
            total = timelinesRotation[i];
        }
        else {
            diff -= (16384 - (int)(16384.499999999996 - diff / 360)) * 360;
            float lastTotal, lastDiff;
            if (firstFrame) {
                lastTotal = 0;
                lastDiff = diff;
            }
            else {
                lastTotal = timelinesRotation[i]; // Angle and direction of mix, including loops.
                lastDiff = timelinesRotation[i + 1]; // Difference between bones.
            }
            
            bool current = diff > 0, dir = lastTotal >= 0;
            // Detect cross at 0 (not 180).
            if (sign(lastDiff) != sign(diff) && fabs(lastDiff) <= 90) {
                // A cross after a 360 rotation is a loop.
                if (fabs(lastTotal) > 180) {
                    lastTotal += 360 * sign(lastTotal);
                }
                dir = current;
            }
            
            total = diff + lastTotal - fmod(lastTotal, 360); // Store loops as part of lastTotal.
            if (dir != current) {
                total += 360 * sign(lastTotal);
            }
            timelinesRotation[i] = total;
        }
        timelinesRotation[i + 1] = diff;
        r1 += total * alpha;
        bone->_rotation = r1 - (16384 - (int)(16384.499999999996 - r1 / 360)) * 360;
    }
    
    bool AnimationState::updateMixingFrom(TrackEntry* to, float delta) {
        TrackEntry* from = to->_mixingFrom;
        if (from == NULL) {
            return true;
        }
        
        bool finished = updateMixingFrom(from, delta);
        
        // Require mixTime > 0 to ensure the mixing from entry was applied at least once.
        if (to->_mixTime > 0 && (to->_mixTime >= to->_mixDuration || to->_timeScale == 0)) {
            // Require totalAlpha == 0 to ensure mixing is complete, unless mixDuration == 0 (the transition is a single frame).
            if (from->_totalAlpha == 0 || to->_mixDuration == 0) {
                to->_mixingFrom = from->_mixingFrom;
                to->_interruptAlpha = from->_interruptAlpha;
                _queue->end(from);
            }
            return finished;
        }
        
        from->_animationLast = from->_nextAnimationLast;
        from->_trackLast = from->_nextTrackLast;
        from->_trackTime += delta * from->_timeScale;
        to->_mixTime += delta * to->_timeScale;
        
        return false;
    }
    
    float AnimationState::applyMixingFrom(TrackEntry* to, Skeleton& skeleton, MixPose currentPose) {
        TrackEntry* from = to->_mixingFrom;
        if (from->_mixingFrom != NULL) {
            applyMixingFrom(from, skeleton, currentPose);
        }
        
        float mix;
        if (to->_mixDuration == 0) {
            // Single frame mix to undo mixingFrom changes.
            mix = 1;
            currentPose = MixPose_Setup;
        }
        else {
            mix = to->_mixTime / to->_mixDuration;
            if (mix > 1) {
                mix = 1;
            }
        }
        
        Vector<Event*>* eventBuffer = mix < from->_eventThreshold ? &_events : NULL;
        bool attachments = mix < from->_attachmentThreshold, drawOrder = mix < from->_drawOrderThreshold;
        float animationLast = from->_animationLast, animationTime = from->getAnimationTime();
        Vector<Timeline*>& timelines = from->_animation->_timelines;
        int timelineCount = static_cast<int>(timelines.size());
        Vector<int>& timelineData = from->_timelineData;
        Vector<TrackEntry*>& timelineDipMix = from->_timelineDipMix;
        
        bool firstFrame = from->_timelinesRotation.size() == 0;
        if (firstFrame) {
            // from.timelinesRotation.setSize
            from->_timelinesRotation.reserve(timelines.size() << 1);
            from->_timelinesRotation.setSize(timelines.size() << 1);
        }
        
        Vector<float>& timelinesRotation = from->_timelinesRotation;
        
        MixPose pose;
        float alphaDip = from->_alpha * to->_interruptAlpha, alphaMix = alphaDip * (1 - mix), alpha;
        from->_totalAlpha = 0;
        for (int i = 0; i < timelineCount; ++i) {
            Timeline* timeline = timelines[i];
            switch (timelineData[i]) {
                case Subsequent:
                    if (!attachments && timeline->getRTTI().derivesFrom(AttachmentTimeline::rtti)) {
                        continue;
                    }
                    if (!drawOrder && timeline->getRTTI().derivesFrom(DrawOrderTimeline::rtti)) {
                        continue;
                    }
                    
                    pose = currentPose;
                    alpha = alphaMix;
                    break;
                case First:
                    pose = MixPose_Setup;
                    alpha = alphaMix;
                    break;
                case Dip:
                    pose = MixPose_Setup;
                    alpha = alphaDip;
                    break;
                default:
                    pose = MixPose_Setup;
                    TrackEntry* dipMix = timelineDipMix[i];
                    alpha = alphaDip * MAX(0, 1 - dipMix->_mixTime / dipMix->_mixDuration);
                    break;
            }
            from->_totalAlpha += alpha;
            
            RotateTimeline* rotateTimeline = NULL;
            if (timeline->getRTTI().derivesFrom(RotateTimeline::rtti)) {
                rotateTimeline = static_cast<RotateTimeline*>(timeline);
            }
            
            if (rotateTimeline != NULL) {
                applyRotateTimeline(rotateTimeline, skeleton, animationTime, alpha, pose, timelinesRotation, i << 1, firstFrame);
            }
            else {
                timeline->apply(skeleton, animationLast, animationTime, eventBuffer, alpha, pose, MixDirection_Out);
            }
        }
        
        if (to->_mixDuration > 0) {
            queueEvents(from, animationTime);
        }
        
        _events.clear();
        from->_nextAnimationLast = animationTime;
        from->_nextTrackLast = from->_trackTime;
        
        return mix;
    }
    
    void AnimationState::queueEvents(TrackEntry* entry, float animationTime) {
        float animationStart = entry->_animationStart, animationEnd = entry->_animationEnd;
        float duration = animationEnd - animationStart;
        float trackLastWrapped = fmodf(entry->_trackLast, duration);
        
        // Queue events before complete.
        int i = 0, n = static_cast<int>(_events.size());
        for (; i < n; ++i) {
            Event* e = _events[i];
            if (e->_time < trackLastWrapped) {
                break;
            }
            if (e->_time > animationEnd) {
                // Discard events outside animation start/end.
                continue;
            }
            _queue->event(entry, e);
        }
        
        // Queue complete if completed a loop iteration or the animation.
        if (entry->_loop ? (trackLastWrapped > fmod(entry->_trackTime, duration)) : (animationTime >= animationEnd && entry->_animationLast < animationEnd)) {
            _queue->complete(entry);
        }
        
        // Queue events after complete.
        for (; i < n; ++i) {
            Event* e = _events[i];
            if (e->_time < animationStart) {
                // Discard events outside animation start/end.
                continue;
            }
            _queue->event(entry, _events[i]);
        }
    }
    
    void AnimationState::setCurrent(int index, TrackEntry* current, bool interrupt) {
        TrackEntry* from = expandToIndex(index);
        _tracks[index] = current;
        
        if (from != NULL) {
            if (interrupt) {
                _queue->interrupt(from);
            }
            
            current->_mixingFrom = from;
            current->_mixTime = 0;
            
            // Store interrupted mix percentage.
            if (from->_mixingFrom != NULL && from->_mixDuration > 0) {
                current->_interruptAlpha *= MIN(1, from->_mixTime / from->_mixDuration);
            }
            
            from->_timelinesRotation.clear(); // Reset rotation for mixing out, in case entry was mixed in.
        }
        
        _queue->start(current); // triggers animationsChanged
    }
    
    TrackEntry* AnimationState::expandToIndex(int index) {
        if (index < _tracks.size()) {
            return _tracks[index];
        }
        
        while (index >= _tracks.size()) {
            _tracks.push_back(NULL);
        }
        
        return NULL;
    }
    
    TrackEntry* AnimationState::newTrackEntry(int trackIndex, Animation* animation, bool loop, TrackEntry* last) {
        TrackEntry* entryP = _trackEntryPool.obtain(); // Pooling
        TrackEntry& entry = *entryP;
        
        entry._trackIndex = trackIndex;
        entry._animation = animation;
        entry._loop = loop;
        
        entry._eventThreshold = 0;
        entry._attachmentThreshold = 0;
        entry._drawOrderThreshold = 0;
        
        entry._animationStart = 0;
        entry._animationEnd = animation->getDuration();
        entry._animationLast = -1;
        entry._nextAnimationLast = -1;
        
        entry._delay = 0;
        entry._trackTime = 0;
        entry._trackLast = -1;
        entry._nextTrackLast = -1; // nextTrackLast == -1 signifies a TrackEntry that wasn't applied yet.
        entry._trackEnd = std::numeric_limits<float>::max(); // loop ? float.MaxValue : animation.Duration;
        entry._timeScale = 1;
        
        entry._alpha = 1;
        entry._interruptAlpha = 1;
        entry._mixTime = 0;
        entry._mixDuration = (last == NULL) ? 0 : _data.getMix(last->_animation, animation);
        
        return entryP;
    }
    
    void AnimationState::disposeNext(TrackEntry* entry) {
        TrackEntry* next = entry->_next;
        while (next != NULL) {
            _queue->dispose(next);
            next = next->_next;
        }
        entry->_next = NULL;
    }
    
    void AnimationState::animationsChanged() {
        _animationsChanged = false;
        
        _propertyIDs.clear();
        
        for (int i = 0, n = static_cast<int>(_tracks.size()); i < n; ++i) {
            TrackEntry* entry = _tracks[i];
            if (entry != NULL) {
                entry->setTimelineData(NULL, _mixingTo, _propertyIDs);
            }
        }
    }
}
