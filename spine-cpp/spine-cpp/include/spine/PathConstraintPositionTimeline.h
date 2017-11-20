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

#ifndef Spine_PathConstraintPositionTimeline_h
#define Spine_PathConstraintPositionTimeline_h

#include <spine/CurveTimeline.h>

namespace Spine
{
    class PathConstraintPositionTimeline : public CurveTimeline
    {
        SPINE_RTTI_DECL;
        
//        public const int ENTRIES = 2;
//        protected const int PREV_TIME = -2, PREV_VALUE = -1;
//        protected const int VALUE = 1;
//        
//        internal int pathConstraintIndex;
//        internal float[] frames;
//        
//        override public int PropertyId {
//            get { return ((int)TimelineType.PathConstraintPosition << 24) + pathConstraintIndex; }
//        }
//        
//        public PathConstraintPositionTimeline (int frameCount)
//        : base(frameCount) {
//            frames = new float[frameCount * ENTRIES];
//        }
//        
//        public int PathConstraintIndex { return pathConstraintIndex; } set { pathConstraintIndex = inValue; }
//        public float[] Frames { return frames; } set { frames = inValue; } // time, position, ...
//        
//        /// Sets the time and value of the specified keyframe.
//        public void SetFrame (int frameIndex, float time, float value) {
//            frameIndex *= ENTRIES;
//            frames[frameIndex] = time;
//            frames[frameIndex + VALUE] = inValue;
//        }
//        
//        override public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
//            PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
//            float[] frames = _frames;
//            if (time < frames[0]) {
//                switch (pose) {
//                    case MixPose_Setup:
//                        constraint.position = constraint.data.position;
//                        return;
//                    case MixPose_Current:
//                        constraint.position += (constraint.data.position - constraint.position) * alpha;
//                        return;
//                }
//                return;
//            }
//            
//            float position;
//            if (time >= frames[frames.Length - ENTRIES]) // Time is after last frame.
//                position = frames[frames.Length + PREV_VALUE];
//            else {
//                // Interpolate between the previous frame and the current frame.
//                int frame = Animation.BinarySearch(frames, time, ENTRIES);
//                position = frames[frame + PREV_VALUE];
//                float frameTime = frames[frame];
//                float percent = GetCurvePercent(frame / ENTRIES - 1,
//                                                1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
//                
//                position += (frames[frame + VALUE] - position) * percent;
//            }
//            if (pose == MixPose_Setup)
//                constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
//            else
//                constraint.position += (position - constraint.position) * alpha;
//        }
    };
}

#endif /* Spine_PathConstraintPositionTimeline_h */
