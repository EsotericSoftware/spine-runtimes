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

#ifndef Spine_ShearTimeline_h
#define Spine_ShearTimeline_h

#include <spine/TranslateTimeline.h>

namespace Spine
{
    class ShearTimeline : public TranslateTimeline
    {
        SPINE_RTTI_DECL;
        
    public:
        virtual void apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*>& events, float alpha, MixPose pose, MixDirection direction);
        
        virtual int getPropertyId();
        
//        public ShearTimeline (int frameCount)
//        : base(frameCount) {
//        }
//
//        override public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
//            Bone bone = skeleton.bones.Items[boneIndex];
//            float[] frames = _frames;
//            if (time < frames[0]) {
//                switch (pose) {
//                    case MixPose_Setup:
//                        bone.shearX = bone.data.shearX;
//                        bone.shearY = bone.data.shearY;
//                        return;
//                    case MixPose_Current:
//                        bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
//                        bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
//                        return;
//                }
//                return;
//            }
//
//            float x, y;
//            if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
//                x = frames[frames.Length + PREV_X];
//                y = frames[frames.Length + PREV_Y];
//            } else {
//                // Interpolate between the previous frame and the current frame.
//                int frame = Animation::binarySearch(frames, time, ENTRIES);
//                x = frames[frame + PREV_X];
//                y = frames[frame + PREV_Y];
//                float frameTime = frames[frame];
//                float percent = GetCurvePercent(frame / ENTRIES - 1,
//                                                1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
//
//                x = x + (frames[frame + X] - x) * percent;
//                y = y + (frames[frame + Y] - y) * percent;
//            }
//            if (pose == MixPose_Setup) {
//                bone.shearX = bone.data.shearX + x * alpha;
//                bone.shearY = bone.data.shearY + y * alpha;
//            } else {
//                bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
//                bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
//            }
//        }
    };
}

#endif /* Spine_ShearTimeline_h */
