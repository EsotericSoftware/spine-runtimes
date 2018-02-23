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

#include <spine/IkConstraintTimeline.h>

#include <spine/Skeleton.h>
#include <spine/Event.h>

#include <spine/Animation.h>
#include <spine/TimelineType.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>
#include <spine/IkConstraint.h>
#include <spine/IkConstraintData.h>

namespace Spine {
    RTTI_IMPL(IkConstraintTimeline, CurveTimeline);
    
    const int IkConstraintTimeline::ENTRIES = 3;
    const int IkConstraintTimeline::PREV_TIME = -3;
    const int IkConstraintTimeline::PREV_MIX = -2;
    const int IkConstraintTimeline::PREV_BEND_DIRECTION = -1;
    const int IkConstraintTimeline::MIX = 1;
    const int IkConstraintTimeline::BEND_DIRECTION = 2;
    
    IkConstraintTimeline::IkConstraintTimeline(int frameCount) : CurveTimeline(frameCount), _ikConstraintIndex(0) {
        _frames.setSize(frameCount * ENTRIES);
    }
    
    void IkConstraintTimeline::apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*>* pEvents, float alpha, MixPose pose, MixDirection direction) {
        IkConstraint* constraintP = skeleton._ikConstraints[_ikConstraintIndex];
        IkConstraint& constraint = *constraintP;
        if (time < _frames[0]) {
            switch (pose) {
                case MixPose_Setup:
                    constraint._mix = constraint._data._mix;
                    constraint._bendDirection = constraint._data._bendDirection;
                    return;
                case MixPose_Current:
                    constraint._mix += (constraint._data._mix - constraint._mix) * alpha;
                    constraint._bendDirection = constraint._data._bendDirection;
                    return;
                case MixPose_CurrentLayered:
                default:
                    return;
            }
        }
        
        if (time >= _frames[_frames.size() - ENTRIES]) {
            // Time is after last frame.
            if (pose == MixPose_Setup) {
                constraint._mix = constraint._data._mix + (_frames[_frames.size() + PREV_MIX] - constraint._data._mix) * alpha;
                constraint._bendDirection = direction == MixDirection_Out ? constraint._data._bendDirection
                : (int)_frames[_frames.size() + PREV_BEND_DIRECTION];
            }
            else {
                constraint._mix += (_frames[_frames.size() + PREV_MIX] - constraint._mix) * alpha;
                if (direction == MixDirection_In) {
                    constraint._bendDirection = (int)_frames[_frames.size() + PREV_BEND_DIRECTION];
                }
            }
            return;
        }
        
        // Interpolate between the previous frame and the current frame.
        int frame = Animation::binarySearch(_frames, time, ENTRIES);
        float mix = _frames[frame + PREV_MIX];
        float frameTime = _frames[frame];
        float percent = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (_frames[frame + PREV_TIME] - frameTime));
        
        if (pose == MixPose_Setup) {
            constraint._mix = constraint._data._mix + (mix + (_frames[frame + MIX] - mix) * percent - constraint._data._mix) * alpha;
            constraint._bendDirection = direction == MixDirection_Out ? constraint._data._bendDirection : (int)_frames[frame + PREV_BEND_DIRECTION];
        }
        else {
            constraint._mix += (mix + (_frames[frame + MIX] - mix) * percent - constraint._mix) * alpha;
            if (direction == MixDirection_In) {
                constraint._bendDirection = (int)_frames[frame + PREV_BEND_DIRECTION];
            }
        }
    }
    
    int IkConstraintTimeline::getPropertyId() {
        return ((int)TimelineType_IkConstraint << 24) + _ikConstraintIndex;
    }
    
    void IkConstraintTimeline::setFrame(int frameIndex, float time, float mix, int bendDirection) {
        frameIndex *= ENTRIES;
        _frames[frameIndex] = time;
        _frames[frameIndex + MIX] = mix;
        _frames[frameIndex + BEND_DIRECTION] = bendDirection;
    }
}
