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

#include <spine/MathUtil.h>
#include <spine/ContainerUtil.h>

namespace Spine
{
    void dummyOnAnimationEventFunc(AnimationState& state, EventType type, TrackEntry* entry, Event* event = NULL)
    {
        // Empty
    }
    
    TrackEntry::TrackEntry() : _animation(NULL), _next(NULL), _mixingFrom(NULL), _trackIndex(0), _loop(false), _eventThreshold(0), _attachmentThreshold(0), _drawOrderThreshold(0), _animationStart(0), _animationEnd(0), _animationLast(0), _nextAnimationLast(0), _delay(0), _trackTime(0), _trackLast(0), _nextTrackLast(0), _trackEnd(0), _timeScale(1.0f), _alpha(0), _mixTime(0), _mixDuration(0), _interruptAlpha(0), _totalAlpha(0), _onAnimationEventFunc(dummyOnAnimationEventFunc)
    {
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
    void TrackEntry::setAnimationLast(float inValue)
    {
        _animationLast = inValue;
        _nextAnimationLast = inValue;
    }

    float TrackEntry::getAnimationTime()
    {
        if (_loop)
        {
            float duration = _animationEnd - _animationStart;
            if (duration == 0)
            {
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

    bool TrackEntry::isComplete()
    {
        return _trackTime >= _animationEnd - _animationStart;
    }

    float TrackEntry::getMixTime() { return _mixTime; }
    void TrackEntry::setMixTime(float inValue) { _mixTime = inValue; }

    float TrackEntry::getMixDuration() { return _mixDuration; }
    void TrackEntry::setMixDuration(float inValue) { _mixDuration = inValue; }

    TrackEntry* TrackEntry::getMixingFrom() { return _mixingFrom; }
    
    void TrackEntry::resetRotationDirections()
    {
        _timelinesRotation.clear();
    }
    
    void TrackEntry::setOnAnimationEventFunc(OnAnimationEventFunc inValue)
    {
        _onAnimationEventFunc = inValue;
    }
    
    TrackEntry* TrackEntry::setTimelineData(TrackEntry* to, Vector<TrackEntry*>& mixingToArray, Vector<int>& propertyIDs)
    {
        if (to != NULL)
        {
            mixingToArray.push_back(to);
        }
        
        TrackEntry* lastEntry = _mixingFrom != NULL ? _mixingFrom->setTimelineData(this, mixingToArray, propertyIDs) : this;
        
        if (to != NULL)
        {
            mixingToArray.erase(mixingToArray.size() - 1);
        }
        
        int mixingToLast = static_cast<int>(mixingToArray.size()) - 1;
        Vector<Timeline*>& timelines = _animation->_timelines;
        int timelinesCount = static_cast<int>(timelines.size());
        _timelineData.reserve(timelinesCount);
        _timelineDipMix.clear();
        _timelineDipMix.reserve(timelinesCount);
        
        // outer:
        for (int i = 0; i < timelinesCount; ++i)
        {
            int id = timelines[i]->getPropertyId();
            if (propertyIDs.contains(id))
            {
                _timelineData[i] = AnimationState::Subsequent;
            }
            else
            {
                propertyIDs.push_back(id);
                
                if (to == NULL || !to->hasTimeline(id))
                {
                    _timelineData[i] = AnimationState::First;
                }
                else
                {
                    for (int ii = mixingToLast; ii >= 0; --ii)
                    {
                        TrackEntry* entry = mixingToArray[ii];
                        if (!entry->hasTimeline(id))
                        {
                            if (entry->_mixDuration > 0)
                            {
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
    
    bool TrackEntry::hasTimeline(int inId)
    {
        Vector<Timeline*>& timelines = _animation->_timelines;
        for (int i = 0, n = static_cast<int>(timelines.size()); i < n; ++i)
        {
            if (timelines[i]->getPropertyId() == inId)
            {
                return true;
            }
        }
        return false;
    }
    
    void TrackEntry::reset()
    {
        _animation = NULL;
        _next = NULL;
        _mixingFrom = NULL;
        
        _timelineData.clear();
        _timelineDipMix.clear();
        _timelinesRotation.clear();
        
        _onAnimationEventFunc = NULL;
    }
    
    EventQueueEntry::EventQueueEntry(EventType eventType, TrackEntry* trackEntry, Event* event) :
    _type(eventType),
    _entry(trackEntry),
    _event(event)
    {
        // Empty
    }
    
    EventQueue* EventQueue::newEventQueue(AnimationState& state, Pool<TrackEntry>& trackEntryPool)
    {
        EventQueue* ret = NEW(EventQueue);
        new (ret) EventQueue(state, trackEntryPool);
        
        return ret;
    }
    
    EventQueueEntry* EventQueue::newEventQueueEntry(EventType eventType, TrackEntry* entry, Event* event)
    {
        EventQueueEntry* ret = NEW(EventQueueEntry);
        new (ret) EventQueueEntry(eventType, entry, event);
        
        return ret;
    }
    
    EventQueue::EventQueue(AnimationState& state, Pool<TrackEntry>& trackEntryPool) : _state(state), _trackEntryPool(trackEntryPool), _drainDisabled(false)
    {
        // Empty
    }
    
    EventQueue::~EventQueue()
    {
        ContainerUtil::cleanUpVectorOfPointers(_eventQueueEntries);
    }
    
    void EventQueue::start(TrackEntry* entry)
    {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Start, entry));
        _state._animationsChanged = true;
    }
    
    void EventQueue::interrupt(TrackEntry* entry)
    {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Interrupt, entry));
    }
    
    void EventQueue::end(TrackEntry* entry)
    {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_End, entry));
        _state._animationsChanged = true;
    }
    
    void EventQueue::dispose(TrackEntry* entry)
    {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Dispose, entry));
    }
    
    void EventQueue::complete(TrackEntry* entry)
    {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Complete, entry));
    }
    
    void EventQueue::event(TrackEntry* entry, Event* event)
    {
        _eventQueueEntries.push_back(newEventQueueEntry(EventType_Event, entry, event));
    }
    
    /// Raises all events in the queue and drains the queue.
    void EventQueue::drain()
    {
        if (_drainDisabled)
        {
            return;
        }
        
        _drainDisabled = true;
        
        AnimationState& state = _state;
        
        // Don't cache _eventQueueEntries.size() so callbacks can queue their own events (eg, call setAnimation in AnimationState_Complete).
        for (int i = 0; i < _eventQueueEntries.size(); ++i)
        {
            EventQueueEntry* queueEntry = _eventQueueEntries[i];
            TrackEntry* trackEntry = queueEntry->_entry;
            
            switch (queueEntry->_type)
            {
                case EventType_Start:
                case EventType_Interrupt:
                case EventType_Complete:
                    trackEntry->_onAnimationEventFunc(state, queueEntry->_type, trackEntry, NULL);
                    state._onAnimationEventFunc(state, queueEntry->_type, trackEntry, NULL);
                    break;
                case EventType_End:
                    trackEntry->_onAnimationEventFunc(state, queueEntry->_type, trackEntry, NULL);
                    state._onAnimationEventFunc(state, queueEntry->_type, trackEntry, NULL);
                    /* Yes, we want to fall through here */
                case EventType_Dispose:
                    trackEntry->_onAnimationEventFunc(state, EventType_Dispose, trackEntry, NULL);
                    state._onAnimationEventFunc(state, EventType_Dispose, trackEntry, NULL);
                    _trackEntryPool.free(trackEntry);
                    break;
                case EventType_Event:
                    trackEntry->_onAnimationEventFunc(state, queueEntry->_type, trackEntry, queueEntry->_event);
                    state._onAnimationEventFunc(state, queueEntry->_type, trackEntry, queueEntry->_event);
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
    _onAnimationEventFunc(dummyOnAnimationEventFunc),
    _timeScale(1)
    {
        // Empty
    }
    
    AnimationState::~AnimationState()
    {
        DESTROY(EventQueue, _queue);
    }
    
    void AnimationState::setOnAnimationEventFunc(OnAnimationEventFunc inValue)
    {
        _onAnimationEventFunc = inValue;
    }
    
    Animation& AnimationState::getEmptyAnimation()
    {
        static Vector<Timeline*> timelines;
        static Animation ret(std::string("<empty>"), timelines, 0);
        return ret;
    }
}
