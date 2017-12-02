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

#include <spine/ScaleTimeline.h>

#include <spine/Skeleton.h>
#include <spine/Event.h>

#include <spine/Animation.h>
#include <spine/TimelineType.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>
#include <spine/Bone.h>
#include <spine/BoneData.h>

namespace Spine
{
    RTTI_IMPL(ScaleTimeline, TranslateTimeline);
    
    ScaleTimeline::ScaleTimeline(int frameCount) : TranslateTimeline(frameCount)
    {
        // Empty
    }
    
    void ScaleTimeline::apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*>* pEvents, float alpha, MixPose pose, MixDirection direction)
    {
        Bone* boneP = skeleton._bones[_boneIndex];
        Bone& bone = *boneP;
        
        if (time < _frames[0])
        {
            switch (pose)
            {
                case MixPose_Setup:
                    bone._scaleX = bone._data._scaleX;
                    bone._scaleY = bone._data._scaleY;
                    return;
                case MixPose_Current:
                    bone._scaleX += (bone._data._scaleX - bone._scaleX) * alpha;
                    bone._scaleY += (bone._data._scaleY - bone._scaleY) * alpha;
                    return;
                case MixPose_CurrentLayered:
                default:
                    return;
            }
        }
        
        float x, y;
        if (time >= _frames[_frames.size() - ENTRIES])
        {
            // Time is after last frame.
            x = _frames[_frames.size() + PREV_X] * bone._data._scaleX;
            y = _frames[_frames.size() + PREV_Y] * bone._data._scaleY;
        }
        else
        {
            // Interpolate between the previous frame and the current frame.
            int frame = Animation::binarySearch(_frames, time, ENTRIES);
            x = _frames[frame + PREV_X];
            y = _frames[frame + PREV_Y];
            float frameTime = _frames[frame];
            float percent = getCurvePercent(frame / ENTRIES - 1,
                                            1 - (time - frameTime) / (_frames[frame + PREV_TIME] - frameTime));
            
            x = (x + (_frames[frame + X] - x) * percent) * bone._data._scaleX;
            y = (y + (_frames[frame + Y] - y) * percent) * bone._data._scaleY;
        }
        
        if (alpha == 1)
        {
            bone._scaleX = x;
            bone._scaleY = y;
        }
        else
        {
            float bx, by;
            if (pose == MixPose_Setup)
            {
                bx = bone._data._scaleX;
                by = bone._data._scaleY;
            }
            else
            {
                bx = bone._scaleX;
                by = bone._scaleY;
            }
            // Mixing out uses sign of setup or current pose, else use sign of key.
            if (direction == MixDirection_Out)
            {
                x = (x >= 0 ? x : -x) * (bx >= 0 ? 1 : -1);
                y = (y >= 0 ? y : -y) * (by >= 0 ? 1 : -1);
            }
            else
            {
                bx = (bx >= 0 ? bx : -bx) * (x >= 0 ? 1 : -1);
                by = (by >= 0 ? by : -by) * (y >= 0 ? 1 : -1);
            }
            bone._scaleX = bx + (x - bx) * alpha;
            bone._scaleY = by + (y - by) * alpha;
        }
    }
    
    int ScaleTimeline::getPropertyId()
    {
        return ((int)TimelineType_Scale << 24) + _boneIndex;
    }
}
