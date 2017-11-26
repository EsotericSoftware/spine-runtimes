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

#include <spine/MathUtil.h>

namespace Spine
{
    TrackEntry::TrackEntry()
    {
        
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
    
//    event AnimationState.TrackEntryDelegate Start, Interrupt, End, Dispose, Complete;
//    event AnimationState.TrackEntryEventDelegate Event;
    
    void TrackEntry::resetRotationDirections()
    {
        _timelinesRotation.clear();
    }
}
