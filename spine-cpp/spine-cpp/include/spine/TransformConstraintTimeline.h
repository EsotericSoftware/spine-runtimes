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

#ifndef Spine_TransformConstraintTimeline_h
#define Spine_TransformConstraintTimeline_h

#include <spine/CurveTimeline.h>

namespace Spine
{
    class TransformConstraintTimeline : public CurveTimeline
    {
        RTTI_DECL;
        
        public const int ENTRIES = 5;
        private const int PREV_TIME = -5, PREV_ROTATE = -4, PREV_TRANSLATE = -3, PREV_SCALE = -2, PREV_SHEAR = -1;
        private const int ROTATE = 1, TRANSLATE = 2, SCALE = 3, SHEAR = 4;
        
        internal int transformConstraintIndex;
        internal float[] frames;
        
        public int TransformConstraintIndex { return transformConstraintIndex; } set { transformConstraintIndex = inValue; }
        public float[] Frames { return frames; } set { frames = inValue; } // time, rotate mix, translate mix, scale mix, shear mix, ...
        
        override public int PropertyId {
            get { return ((int)TimelineType.TransformConstraint << 24) + transformConstraintIndex; }
        }
        
        public TransformConstraintTimeline (int frameCount)
        : base(frameCount) {
            frames = new float[frameCount * ENTRIES];
        }
        
        public void SetFrame (int frameIndex, float time, float rotateMix, float translateMix, float scaleMix, float shearMix) {
            frameIndex *= ENTRIES;
            frames[frameIndex] = time;
            frames[frameIndex + ROTATE] = rotateMix;
            frames[frameIndex + TRANSLATE] = translateMix;
            frames[frameIndex + SCALE] = scaleMix;
            frames[frameIndex + SHEAR] = shearMix;
        }
        
        override public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
            TransformConstraint constraint = skeleton.transformConstraints.Items[transformConstraintIndex];
            float[] frames = _frames;
            if (time < frames[0]) {
                var data = constraint.data;
                switch (pose) {
                    case MixPose_Setup:
                        constraint.rotateMix = data.rotateMix;
                        constraint.translateMix = data.translateMix;
                        constraint.scaleMix = data.scaleMix;
                        constraint.shearMix = data.shearMix;
                        return;
                    case MixPose_Current:
                        constraint.rotateMix += (data.rotateMix - constraint.rotateMix) * alpha;
                        constraint.translateMix += (data.translateMix - constraint.translateMix) * alpha;
                        constraint.scaleMix += (data.scaleMix - constraint.scaleMix) * alpha;
                        constraint.shearMix += (data.shearMix - constraint.shearMix) * alpha;
                        return;
                }
                return;
            }
            
            float rotate, translate, scale, shear;
            if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
                int i = frames.Length;
                rotate = frames[i + PREV_ROTATE];
                translate = frames[i + PREV_TRANSLATE];
                scale = frames[i + PREV_SCALE];
                shear = frames[i + PREV_SHEAR];
            } else {
                // Interpolate between the previous frame and the current frame.
                int frame = Animation.BinarySearch(frames, time, ENTRIES);
                rotate = frames[frame + PREV_ROTATE];
                translate = frames[frame + PREV_TRANSLATE];
                scale = frames[frame + PREV_SCALE];
                shear = frames[frame + PREV_SHEAR];
                float frameTime = frames[frame];
                float percent = GetCurvePercent(frame / ENTRIES - 1,
                                                1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
                
                rotate += (frames[frame + ROTATE] - rotate) * percent;
                translate += (frames[frame + TRANSLATE] - translate) * percent;
                scale += (frames[frame + SCALE] - scale) * percent;
                shear += (frames[frame + SHEAR] - shear) * percent;
            }
            if (pose == MixPose_Setup) {
                TransformConstraintData data = constraint.data;
                constraint.rotateMix = data.rotateMix + (rotate - data.rotateMix) * alpha;
                constraint.translateMix = data.translateMix + (translate - data.translateMix) * alpha;
                constraint.scaleMix = data.scaleMix + (scale - data.scaleMix) * alpha;
                constraint.shearMix = data.shearMix + (shear - data.shearMix) * alpha;
            } else {
                constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
                constraint.translateMix += (translate - constraint.translateMix) * alpha;
                constraint.scaleMix += (scale - constraint.scaleMix) * alpha;
                constraint.shearMix += (shear - constraint.shearMix) * alpha;
            }
        }
    }
}

#endif /* Spine_TransformConstraintTimeline_h */
