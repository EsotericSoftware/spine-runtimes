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

#ifndef Spine_AttachmentTimeline_h
#define Spine_AttachmentTimeline_h

#include <spine/Timeline.h>

#include <spine/Vector.h>
#include <spine/MixPose.h>
#include <spine/MixDirection.h>

#include <string>

namespace Spine
{
    class Skeleton;
    class Event;
    
    class AttachmentTimeline : public Timeline
    {
        SPINE_RTTI_DECL;
        
    public:
        AttachmentTimeline(int frameCount)
        {
            _frames.reserve(frameCount);
            _attachmentNames.reserve(frameCount);
        }
        
//        virtual int getPropertyId()
//        {
//            return ((int)TimelineType_Attachment << 24) + slotIndex;
//        }
//        
//        /// Sets the time and value of the specified keyframe.
//        void setFrame(int frameIndex, float time, std::string attachmentName)
//        {
//            frames[frameIndex] = time;
//            attachmentNames[frameIndex] = attachmentName;
//        }
//        
//        void apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*> firedEvents, float alpha, MixPose pose, MixDirection direction)
//        {
//            std::string attachmentName;
//            Slot slot = skeleton.slots.Items[slotIndex];
//            if (direction == MixDirection_Out && pose == MixPose_Setup)
//            {
//                attachmentName = slot.data.attachmentName;
//                slot.Attachment = attachmentName == NULL ? NULL : skeleton.getAttachment(slotIndex, attachmentName);
//                return;
//            }
//            
//            float[] frames = _frames;
//            if (time < frames[0])
//            {
//                // Time is before first frame.
//                if (pose == MixPose_Setup)
//                {
//                    attachmentName = slot.data.attachmentName;
//                    slot.Attachment = attachmentName == NULL ? NULL : skeleton.getAttachment(slotIndex, attachmentName);
//                }
//                return;
//            }
//            
//            int frameIndex;
//            if (time >= frames[frames.Length - 1]) // Time is after last frame.
//            {
//                frameIndex = frames.Length - 1;
//            }
//            else
//            {
//                frameIndex = Animation::binarySearch(frames, time, 1) - 1;
//            }
//            
//            attachmentName = attachmentNames[frameIndex];
//            slot.Attachment = attachmentName == NULL ? NULL : skeleton.getAttachment(slotIndex, attachmentName);
//        }
//        
//        int getSlotIndex() { return _slotIndex; }
//        void setSlotIndex(int inValue) { _slotIndex = inValue; }
//        Vector<float>& getFrames() { return _frames; }
//        void setFrames(Vector<float>& inValue) { _frames = inValue; } // time, ...
//        Vector<std::string> getAttachmentNames() { return _attachmentNames; }
//        set { attachmentNames = inValue; }
//        int getFrameCount() { return frames.Length; }
        
    private:
        int _slotIndex;
        Vector<float> _frames;
        Vector<std::string> _attachmentNames;
    };
}

#endif /* Spine_AttachmentTimeline_h */
