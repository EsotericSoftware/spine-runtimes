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

#ifndef Spine_ScaleTimeline_h
#define Spine_ScaleTimeline_h

#include <spine/TranslateTimeline.h>

namespace Spine
{
    class ScaleTimeline : public TranslateTimeline
    {
        RTTI_DECL;
        
//        override public int PropertyId {
//            get { return ((int)TimelineType.Scale << 24) + boneIndex; }
//        }
//
//        public ScaleTimeline (int frameCount)
//        : base(frameCount) {
//        }
//
//        override public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
//            Bone bone = skeleton.bones.Items[boneIndex];
//
//            float[] frames = _frames;
//            if (time < frames[0]) {
//                switch (pose) {
//                    case MixPose_Setup:
//                        bone.scaleX = bone.data.scaleX;
//                        bone.scaleY = bone.data.scaleY;
//                        return;
//                    case MixPose_Current:
//                        bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
//                        bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
//                        return;
//                }
//                return;
//            }
//
//            float x, y;
//            if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
//                x = frames[frames.Length + PREV_X] * bone.data.scaleX;
//                y = frames[frames.Length + PREV_Y] * bone.data.scaleY;
//            } else {
//                // Interpolate between the previous frame and the current frame.
//                int frame = Animation.BinarySearch(frames, time, ENTRIES);
//                x = frames[frame + PREV_X];
//                y = frames[frame + PREV_Y];
//                float frameTime = frames[frame];
//                float percent = GetCurvePercent(frame / ENTRIES - 1,
//                                                1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
//
//                x = (x + (frames[frame + X] - x) * percent) * bone.data.scaleX;
//                y = (y + (frames[frame + Y] - y) * percent) * bone.data.scaleY;
//            }
//            if (alpha == 1) {
//                bone.scaleX = x;
//                bone.scaleY = y;
//            } else {
//                float bx, by;
//                if (pose == MixPose_Setup) {
//                    bx = bone.data.scaleX;
//                    by = bone.data.scaleY;
//                } else {
//                    bx = bone.scaleX;
//                    by = bone.scaleY;
//                }
//                // Mixing out uses sign of setup or current pose, else use sign of key.
//                if (direction == MixDirection_Out) {
//                    x = (x >= 0 ? x : -x) * (bx >= 0 ? 1 : -1);
//                    y = (y >= 0 ? y : -y) * (by >= 0 ? 1 : -1);
//                } else {
//                    bx = (bx >= 0 ? bx : -bx) * (x >= 0 ? 1 : -1);
//                    by = (by >= 0 ? by : -by) * (y >= 0 ? 1 : -1);
//                }
//                bone.scaleX = bx + (x - bx) * alpha;
//                bone.scaleY = by + (y - by) * alpha;
//            }
//        }
    };
}

#endif /* Spine_ScaleTimeline_h */
