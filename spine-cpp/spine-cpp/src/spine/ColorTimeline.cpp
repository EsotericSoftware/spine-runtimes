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

#include <spine/ColorTimeline.h>

#include <spine/Skeleton.h>
#include <spine/Event.h>

#include <spine/Animation.h>
#include <spine/TimelineType.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>

namespace Spine {
    RTTI_IMPL(ColorTimeline, CurveTimeline);
    
    const int ColorTimeline::ENTRIES = 5;
    const int ColorTimeline::PREV_TIME = -5;
    const int ColorTimeline::PREV_R = -4;
    const int ColorTimeline::PREV_G = -3;
    const int ColorTimeline::PREV_B = -2;
    const int ColorTimeline::PREV_A = -1;
    const int ColorTimeline::R = 1;
    const int ColorTimeline::G = 2;
    const int ColorTimeline::B = 3;
    const int ColorTimeline::A = 4;
    
    ColorTimeline::ColorTimeline(int frameCount) : CurveTimeline(frameCount), _slotIndex(0) {
		_frames.ensureCapacity(frameCount * ENTRIES);
        _frames.setSize(frameCount * ENTRIES);
    }
    
    void ColorTimeline::apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*>* pEvents, float alpha, MixPose pose, MixDirection direction) {
        Slot* slotP = skeleton._slots[_slotIndex];
        Slot& slot = *slotP;
        if (time < _frames[0]) {
            SlotData& slotData = slot._data;
            switch (pose) {
                case MixPose_Setup:
                    slot._r = slotData._r;
                    slot._g = slotData._g;
                    slot._b = slotData._b;
                    slot._a = slotData._a;
                    return;
                case MixPose_Current:
                    slot._r += (slot._r - slotData._r) * alpha;
                    slot._g += (slot._g - slotData._g) * alpha;
                    slot._b += (slot._b - slotData._b) * alpha;
                    slot._a += (slot._a - slotData._a) * alpha;
                    return;
                case MixPose_CurrentLayered:
                default:
                    return;
            }
        }
        
        float r, g, b, a;
        if (time >= _frames[_frames.size() - ENTRIES]) {
            // Time is after last frame.
            int i = static_cast<int>(_frames.size());
            r = _frames[i + PREV_R];
            g = _frames[i + PREV_G];
            b = _frames[i + PREV_B];
            a = _frames[i + PREV_A];
        }
        else {
            // Interpolate between the previous frame and the current frame.
            int frame = Animation::binarySearch(_frames, time, ENTRIES);
            r = _frames[frame + PREV_R];
            g = _frames[frame + PREV_G];
            b = _frames[frame + PREV_B];
            a = _frames[frame + PREV_A];
            float frameTime = _frames[frame];
            float percent = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (_frames[frame + PREV_TIME] - frameTime));
            
            r += (_frames[frame + R] - r) * percent;
            g += (_frames[frame + G] - g) * percent;
            b += (_frames[frame + B] - b) * percent;
            a += (_frames[frame + A] - a) * percent;
        }
        
        if (alpha == 1) {
            slot._r = r;
            slot._g = g;
            slot._b = b;
            slot._a = a;
        }
        else {
            float br, bg, bb, ba;
            if (pose == MixPose_Setup) {
                br = slot._data._r;
                bg = slot._data._g;
                bb = slot._data._b;
                ba = slot._data._a;
            }
            else {
                br = slot._r;
                bg = slot._g;
                bb = slot._b;
                ba = slot._a;
            }
            slot._r = br + ((r - br) * alpha);
            slot._g = bg + ((g - bg) * alpha);
            slot._b = bb + ((b - bb) * alpha);
            slot._a = ba + ((a - ba) * alpha);
        }
    }
    
    int ColorTimeline::getPropertyId() {
        return ((int)TimelineType_Color << 24) + _slotIndex;
    }
    
    void ColorTimeline::setFrame(int frameIndex, float time, float r, float g, float b, float a) {
        frameIndex *= ENTRIES;
        _frames[frameIndex] = time;
        _frames[frameIndex + R] = r;
        _frames[frameIndex + G] = g;
        _frames[frameIndex + B] = b;
        _frames[frameIndex + A] = a;
    }
    
    int ColorTimeline::getSlotIndex() {
        return _slotIndex;
    }
    
    void ColorTimeline::setSlotIndex(int inValue) {
        _slotIndex = inValue;
    }
    
    Vector<float>& ColorTimeline::getFrames() {
        return _frames;
    }
    
    void ColorTimeline::setFrames(Vector<float>& inValue) {
        _frames = inValue;
    }
}
