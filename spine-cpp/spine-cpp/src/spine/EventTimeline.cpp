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

#include <spine/EventTimeline.h>

#include <spine/Skeleton.h>
#include <spine/Event.h>

#include <spine/Animation.h>
#include <spine/TimelineType.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>
#include <spine/Event.h>
#include <spine/ContainerUtil.h>

namespace Spine {
    RTTI_IMPL(EventTimeline, Timeline);
    
    EventTimeline::EventTimeline(int frameCount) : Timeline() {
        _frames.reserve(frameCount);
        _events.reserve(frameCount);
        
        _frames.setSize(frameCount);
        _events.setSize(frameCount);
    }
    
    EventTimeline::~EventTimeline() {
        ContainerUtil::cleanUpVectorOfPointers(_events);
    }
    
    void EventTimeline::apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*>* pEvents, float alpha, MixPose pose, MixDirection direction) {
        if (pEvents == NULL) {
            return;
        }
        
        Vector<Event*>& events = *pEvents;
        
        if (events.size() == 0) {
            return;
        }
        
        int frameCount = static_cast<int>(_frames.size());
        
        if (lastTime > time) {
            // Fire events after last time for looped animations.
            apply(skeleton, lastTime, std::numeric_limits<int>::max(), pEvents, alpha, pose, direction);
            lastTime = -1.0f;
        }
        else if (lastTime >= _frames[frameCount - 1]) {
            // Last time is after last frame.
            return;
        }
       
        if (time < _frames[0]) {
            return; // Time is before first frame.
        }
        
        int frame;
        if (lastTime < _frames[0]) {
            frame = 0;
        }
        else {
            frame = Animation::binarySearch(_frames, lastTime);
            float frameTime = _frames[frame];
            while (frame > 0) {
                // Fire multiple events with the same frame.
                if (_frames[frame - 1] != frameTime) {
                    break;
                }
                frame--;
            }
        }
        
        for (; frame < frameCount && time >= _frames[frame]; ++frame) {
            events.push_back(_events[frame]);
        }
    }
    
    int EventTimeline::getPropertyId() {
        return ((int)TimelineType_Event << 24);
    }
    
    void EventTimeline::setFrame(int frameIndex, Event* event) {
        _frames[frameIndex] = event->getTime();
        _events[frameIndex] = event;
    }
    
    Vector<float> EventTimeline::getFrames() { return _frames; }
    void EventTimeline::setFrames(Vector<float>& inValue) { _frames = inValue; } // time, ...
    Vector<Event*>& EventTimeline::getEvents() { return _events; }
    void EventTimeline::setEvents(Vector<Event*>& inValue) { _events = inValue; }
    int EventTimeline::getFrameCount() { return static_cast<int>(_frames.size()); }
}
