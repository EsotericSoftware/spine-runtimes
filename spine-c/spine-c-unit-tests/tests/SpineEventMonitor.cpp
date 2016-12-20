#include "SpineEventMonitor.h" 

#include "spine/spine.h"
#include "KString.h"

#include "KMemory.h" // Last include

SpineEventMonitor::SpineEventMonitor(spAnimationState* _pAnimationState /*= nullptr*/)
{
	bLogging = false;
	RegisterListener(_pAnimationState);
}

SpineEventMonitor::~SpineEventMonitor()
{
	pAnimState = 0;
}

void SpineEventMonitor::RegisterListener(spAnimationState * _pAnimationState)
{
	if (_pAnimationState) {
		_pAnimationState->rendererObject = this;
		_pAnimationState->listener = (spAnimationStateListener)&SpineEventMonitor::spineAnimStateHandler;
	}
	pAnimState = _pAnimationState;
}

bool SpineEventMonitor::isAnimationPlaying()
{
	if (pAnimState) 
		return spAnimationState_getCurrent(pAnimState, 0) != 0;
	return false;
}

void SpineEventMonitor::spineAnimStateHandler(spAnimationState * state, int type, spTrackEntry * entry, spEvent * event)
{
	if (state && state->rendererObject) {
		SpineEventMonitor* pEventMonitor = (SpineEventMonitor*)state->rendererObject;
		pEventMonitor->OnSpineAnimationStateEvent(state, type, entry, event);
	}
}

void SpineEventMonitor::OnSpineAnimationStateEvent(spAnimationState * state, int type, spTrackEntry * trackEntry, spEvent * event)
{
	const char* eventName = 0;
	if (state == pAnimState) { // only monitor ours
		switch(type)
		{
		case SP_ANIMATION_START: eventName = "SP_ANIMATION_START"; break;
		case SP_ANIMATION_INTERRUPT: eventName = "SP_ANIMATION_INTERRUPT"; break;
		case SP_ANIMATION_END: eventName = "SP_ANIMATION_END"; break;
		case SP_ANIMATION_COMPLETE: eventName = "SP_ANIMATION_COMPLETE"; break;
		case SP_ANIMATION_DISPOSE: eventName = "SP_ANIMATION_DISPOSE"; break;
		case SP_ANIMATION_EVENT: eventName = "SP_ANIMATION_EVENT"; break;
		default:
			break;
		}

		if (bLogging && eventName && trackEntry && trackEntry->animation && trackEntry->animation->name)
			KOutputDebug(DEBUGLVL, "[%s : '%s']\n", eventName,  trackEntry->animation->name);//*/
	}
}



InterruptMonitor::InterruptMonitor(spAnimationState * _pAnimationState):
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
InterruptMonitor& InterruptMonitor::AddInterruptEvent(int theEventType, spTrackEntry * theTrackEntry)
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
	ev.mEventType = SP_ANIMATION_EVENT;
	ev.mEventName = theEventTriggerName;
	mEventStack.push_back(ev);
	return *this;
}

void InterruptMonitor::OnSpineAnimationStateEvent(spAnimationState * state, int type, spTrackEntry * trackEntry, spEvent * event)
{
	SpineEventMonitor::OnSpineAnimationStateEvent(state, type, trackEntry, event);

	if (mEventStackCursor < mEventStack.size()) {
		if (mEventStack[mEventStackCursor].matches(state, type, trackEntry, event))
			++mEventStackCursor;

		if (mEventStackCursor >= mEventStack.size()) {
			bForceInterrupt = true;
			OnMatchingComplete();
		}
	}
}

inline bool InterruptMonitor::InterruptEvent::matches(spAnimationState * state, int type, spTrackEntry * trackEntry, spEvent * event) {

	// Must match spEventType {SP_ANIMATION_START, SP_ANIMATION_INTERRUPT, SP_ANIMATION_END, SP_ANIMATION_COMPLETE, SP_ANIMATION_DISPOSE, SP_ANIMATION_EVENT }
	if (mEventType == type) {

		// Looking for specific TrackEntry by pointer
		if (mTrackEntry != 0) {
			return mTrackEntry == trackEntry;
		}

		// looking for Animation Track by name
		if (!mAnimName.empty()) {
			if (trackEntry && trackEntry->animation && trackEntry->animation->name) {
				if (CompareNoCase(trackEntry->animation->name, mAnimName) == 0) {
					return true;
				}
			}
			return false;
		}

		// looking for Event String Text
		if (!mEventName.empty()) {
			if (event && event->stringValue) {
				return (CompareNoCase(event->stringValue, mEventName) == 0);
			}
			return false;
		}

		return true; // waiting for ANY spEventType that matches
	}
	return false;
}
