#include "SpineEventMonitor.h" 

#include "KString.h"

#include <spine/Animation.h>
#include <spine/Event.h>

#include "KMemory.h" // Last include

using namespace Spine;

SpineEventMonitor::SpineEventMonitor(AnimationState* _pAnimationState /*= nullptr*/)
{
	bLogging = false;
	RegisterListener(_pAnimationState);
}

SpineEventMonitor::~SpineEventMonitor()
{
	pAnimState = 0;
}

void SpineEventMonitor::RegisterListener(AnimationState * _pAnimationState)
{
    if (_pAnimationState)
    {
        _pAnimationState->setRendererObject(this);
        _pAnimationState->setOnAnimationEventFunc(&SpineEventMonitor::spineAnimStateHandler);
    }
    pAnimState = _pAnimationState;
}

bool SpineEventMonitor::isAnimationPlaying()
{
    if (pAnimState)
    {
        return pAnimState->getCurrent(0) != NULL;
    }
	return false;
}

void SpineEventMonitor::spineAnimStateHandler(AnimationState* state, EventType type, TrackEntry* entry, Event* event)
{
    if (state && state->getRendererObject())
    {
        SpineEventMonitor* pEventMonitor = (SpineEventMonitor*)state->getRendererObject();
        pEventMonitor->OnSpineAnimationStateEvent(state, type, entry, event);
    }
}

void SpineEventMonitor::OnSpineAnimationStateEvent(AnimationState* state, EventType type, TrackEntry* entry, Event* event)
{
    const char* eventName = 0;
    if (state == pAnimState)
    {
        // only monitor ours
        switch(type)
        {
        case EventType_Start: eventName = "EventType_Start"; break;
        case EventType_Interrupt: eventName = "EventType_Interrupt"; break;
        case EventType_End: eventName = "EventType_End"; break;
        case EventType_Complete: eventName = "EventType_Complete"; break;
        case EventType_Dispose: eventName = "EventType_Dispose"; break;
        case EventType_Event: eventName = "EventType_Event"; break;
        default:
            break;
        }

        if (bLogging && eventName && entry && entry->getAnimation())
            KOutputDebug(DEBUGLVL, "[%s : '%s']\n", eventName, entry->getAnimation()->getName().c_str());//*/
    }
}

InterruptMonitor::InterruptMonitor(AnimationState * _pAnimationState):
	SpineEventMonitor(_pAnimationState)
{
	bForceInterrupt = false;
	mEventStackCursor = 0; // cursor used to track events
}

bool InterruptMonitor::isAnimationPlaying()
{
	return !bForceInterrupt && SpineEventMonitor::isAnimationPlaying();
}

// Stops the animation on any occurance of the spEventType
InterruptMonitor& InterruptMonitor::AddInterruptEvent(int theEventType)
{
	InterruptEvent ev;
	ev.mEventType = theEventType;
	mEventStack.push_back(ev);
	return *this;
}

// Stops the animation when the [spEventType : 'animationName'] occurs
InterruptMonitor& InterruptMonitor::AddInterruptEvent(int theEventType, const std::string & theAnimationName)
{
	InterruptEvent ev;
	ev.mEventType = theEventType;
	ev.mAnimName = theAnimationName;
	mEventStack.push_back(ev);
	return *this;
}

// stops the first encounter of spEventType on the specified TrackEntry
InterruptMonitor& InterruptMonitor::AddInterruptEvent(int theEventType, TrackEntry * theTrackEntry)
{
	InterruptEvent ev;
	ev.mEventType = theEventType;
	ev.mTrackEntry = theTrackEntry;
	mEventStack.push_back(ev);
	return *this;
}

// Stops on the first SP_ANIMATION_EVENT with the string payload of 'theEventTriggerName'
InterruptMonitor& InterruptMonitor::AddInterruptEventTrigger(const std::string & theEventTriggerName)
{
	InterruptEvent ev;
    ev.mEventType = EventType_Event;
	ev.mEventName = theEventTriggerName;
	mEventStack.push_back(ev);
	return *this;
}

void InterruptMonitor::OnSpineAnimationStateEvent(AnimationState * state, EventType type, TrackEntry * trackEntry, Event * event)
{
	SpineEventMonitor::OnSpineAnimationStateEvent(state, type, trackEntry, event);

	if (mEventStackCursor < mEventStack.size())
    {
		if (mEventStack[mEventStackCursor].matches(state, type, trackEntry, event))
        {
            ++mEventStackCursor;
        }

		if (mEventStackCursor >= mEventStack.size())
        {
			bForceInterrupt = true;
			OnMatchingComplete();
		}
	}
}

inline bool InterruptMonitor::InterruptEvent::matches(AnimationState * state, EventType type, TrackEntry * trackEntry, Event * event)
{
	// Must match EventType {EventType_Start, EventType_Interrupt, EventType_End, EventType_Complete, EventType_Dispose, EventType_Event }
    if (mEventType == type)
    {
        // Looking for specific TrackEntry by pointer
        if (mTrackEntry != 0)
        {
            return mTrackEntry == trackEntry;
        }

        // looking for Animation Track by name
        if (!mAnimName.empty())
        {
            if (trackEntry && trackEntry->getAnimation())
            {
                if (CompareNoCase(trackEntry->getAnimation()->getName(), mAnimName) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        // looking for Event String Text
        if (!mEventName.empty())
        {
            if (event)
            {
                return (CompareNoCase(event->getStringValue(), mEventName) == 0);
            }
            return false;
        }

        return true; // waiting for ANY spEventType that matches
    }
	return false;
}
