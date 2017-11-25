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

#ifndef Spine_TranslateTimeline_h
#define Spine_TranslateTimeline_h

#include <spine/CurveTimeline.h>

#include <spine/Animation.h>
#include <spine/TimelineType.h>

namespace Spine
{
    class TranslateTimeline : public CurveTimeline
    {
        SPINE_RTTI_DECL;
        
        virtual void apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*>& events, float alpha, MixPose pose, MixDirection direction);
        
        virtual int getPropertyId();
        
//        public const int ENTRIES = 3;
//        protected const int PREV_TIME = -3, PREV_X = -2, PREV_Y = -1;
//        protected const int X = 1, Y = 2;
//
//        internal int boneIndex;
//        internal float[] frames;
//
//        public int getBoneIndex { return boneIndex; } set { boneIndex = inValue; }
//        public Vector<float> getFrames { return frames; } set { frames = inValue; } // time, value, value, ...
//
//        virtual int getPropertyId()
//        {
//            return ((int)TimelineType_Translate << 24) + boneIndex;
//        }
//
//        public TranslateTimeline(int frameCount) : CurveTimeline(frameCount)
//        {
//            frames = new float[frameCount * ENTRIES];
//        }
//
//        /// Sets the time and value of the specified keyframe.
//        public void setFrame(int frameIndex, float time, float x, float y)
//        {
//            frameIndex *= ENTRIES;
//            frames[frameIndex] = time;
//            frames[frameIndex + X] = x;
//            frames[frameIndex + Y] = y;
//        }
//
//        override public void apply(Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction)
//        {
//            Bone bone = skeleton.bones.Items[boneIndex];
//
//            float[] frames = _frames;
//            if (time < frames[0])
//            {
//                switch (pose)
//                {
//                    case MixPose_Setup:
//                        bone.x = bone.data.x;
//                        bone.y = bone.data.y;
//                        return;
//                    case MixPose_Current:
//                        bone.x += (bone.data.x - bone.x) * alpha;
//                        bone.y += (bone.data.y - bone.y) * alpha;
//                        return;
//                }
//                return;
//            }
//
//            float x, y;
//            if (time >= frames[frames.Length - ENTRIES])
//            {
//                // Time is after last frame.
//                x = frames[frames.Length + PREV_X];
//                y = frames[frames.Length + PREV_Y];
//            }
//            else
//            {
//                // Interpolate between the previous frame and the current frame.
//                int frame = Animation::binarySearch(frames, time, ENTRIES);
//                x = frames[frame + PREV_X];
//                y = frames[frame + PREV_Y];
//                float frameTime = frames[frame];
//                float percent = GetCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
//
//                x += (frames[frame + X] - x) * percent;
//                y += (frames[frame + Y] - y) * percent;
//            }
//
//            if (pose == MixPose_Setup)
//            {
//                bone.x = bone.data.x + x * alpha;
//                bone.y = bone.data.y + y * alpha;
//            }
//            else
//            {
//                bone.x += (bone.data.x + x - bone.x) * alpha;
//                bone.y += (bone.data.y + y - bone.y) * alpha;
//            }
//        }
    };
}

#endif /* Spine_TranslateTimeline_h */
