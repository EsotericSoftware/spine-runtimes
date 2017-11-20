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

#ifndef Spine_PathConstraintMixTimeline_h
#define Spine_PathConstraintMixTimeline_h

#include <spine/CurveTimeline.h>

namespace Spine
{
    class PathConstraintMixTimeline : public CurveTimeline
    {
        SPINE_RTTI_DECL;
        
//        public const int ENTRIES = 3;
//        private const int PREV_TIME = -3, PREV_ROTATE = -2, PREV_TRANSLATE = -1;
//        private const int ROTATE = 1, TRANSLATE = 2;
//        
//        internal int pathConstraintIndex;
//        internal float[] frames;
//        
//        public int PathConstraintIndex { return pathConstraintIndex; } set { pathConstraintIndex = inValue; }
//        public float[] Frames { return frames; } set { frames = inValue; } // time, rotate mix, translate mix, ...
//        
//        override public int PropertyId {
//            get { return ((int)TimelineType.PathConstraintMix << 24) + pathConstraintIndex; }
//        }
//        
//        public PathConstraintMixTimeline (int frameCount)
//        : base(frameCount) {
//            frames = new float[frameCount * ENTRIES];
//        }
//        
//        /// Sets the time and mixes of the specified keyframe.
//        public void SetFrame (int frameIndex, float time, float rotateMix, float translateMix) {
//            frameIndex *= ENTRIES;
//            frames[frameIndex] = time;
//            frames[frameIndex + ROTATE] = rotateMix;
//            frames[frameIndex + TRANSLATE] = translateMix;
//        }
//        
//        override public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
//            PathConstraint constraint = skeleton.pathConstraints.Items[pathConstraintIndex];
//            float[] frames = _frames;
//            if (time < frames[0]) {
//                switch (pose) {
//                    case MixPose_Setup:
//                        constraint.rotateMix = constraint.data.rotateMix;
//                        constraint.translateMix = constraint.data.translateMix;
//                        return;
//                    case MixPose_Current:
//                        constraint.rotateMix += (constraint.data.rotateMix - constraint.rotateMix) * alpha;
//                        constraint.translateMix += (constraint.data.translateMix - constraint.translateMix) * alpha;
//                        return;
//                }
//                return;
//            }
//            
//            float rotate, translate;
//            if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
//                rotate = frames[frames.Length + PREV_ROTATE];
//                translate = frames[frames.Length + PREV_TRANSLATE];
//            } else {
//                // Interpolate between the previous frame and the current frame.
//                int frame = Animation.BinarySearch(frames, time, ENTRIES);
//                rotate = frames[frame + PREV_ROTATE];
//                translate = frames[frame + PREV_TRANSLATE];
//                float frameTime = frames[frame];
//                float percent = GetCurvePercent(frame / ENTRIES - 1,
//                                                1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
//                
//                rotate += (frames[frame + ROTATE] - rotate) * percent;
//                translate += (frames[frame + TRANSLATE] - translate) * percent;
//            }
//            
//            if (pose == MixPose_Setup) {
//                constraint.rotateMix = constraint.data.rotateMix + (rotate - constraint.data.rotateMix) * alpha;
//                constraint.translateMix = constraint.data.translateMix + (translate - constraint.data.translateMix) * alpha;
//            } else {
//                constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
//                constraint.translateMix += (translate - constraint.translateMix) * alpha;
//            }
//        }
    };
}

#endif /* Spine_PathConstraintMixTimeline_h */
