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

#ifndef Spine_ColorTimeline_h
#define Spine_ColorTimeline_h

#include <spine/CurveTimeline.h>

namespace Spine
{
    class ColorTimeline : public CurveTimeline
    {
        RTTI_DECL;
        
//        public const int ENTRIES = 5;
//        protected const int PREV_TIME = -5, PREV_R = -4, PREV_G = -3, PREV_B = -2, PREV_A = -1;
//        protected const int R = 1, G = 2, B = 3, A = 4;
//
//        internal int slotIndex;
//        internal float[] frames;
//
//        public int SlotIndex { return slotIndex; } set { slotIndex = inValue; }
//        public float[] Frames { return frames; } set { frames = inValue; } // time, r, g, b, a, ...
//
//        override public int PropertyId {
//            get { return ((int)TimelineType.Color << 24) + slotIndex; }
//        }
//
//        public ColorTimeline (int frameCount)
//        : base(frameCount) {
//            frames = new float[frameCount * ENTRIES];
//        }
//
//        /// Sets the time and value of the specified keyframe.
//        public void SetFrame (int frameIndex, float time, float r, float g, float b, float a) {
//            frameIndex *= ENTRIES;
//            frames[frameIndex] = time;
//            frames[frameIndex + R] = r;
//            frames[frameIndex + G] = g;
//            frames[frameIndex + B] = b;
//            frames[frameIndex + A] = a;
//        }
//
//        override public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
//            Slot slot = skeleton.slots.Items[slotIndex];
//            float[] frames = _frames;
//            if (time < frames[0]) {
//                var slotData = slot.data;
//                switch (pose) {
//                    case MixPose_Setup:
//                        slot.r = slotData.r;
//                        slot.g = slotData.g;
//                        slot.b = slotData.b;
//                        slot.a = slotData.a;
//                        return;
//                    case MixPose_Current:
//                        slot.r += (slot.r - slotData.r) * alpha;
//                        slot.g += (slot.g - slotData.g) * alpha;
//                        slot.b += (slot.b - slotData.b) * alpha;
//                        slot.a += (slot.a - slotData.a) * alpha;
//                        return;
//                }
//                return;
//            }
//
//            float r, g, b, a;
//            if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
//                int i = frames.Length;
//                r = frames[i + PREV_R];
//                g = frames[i + PREV_G];
//                b = frames[i + PREV_B];
//                a = frames[i + PREV_A];
//            } else {
//                // Interpolate between the previous frame and the current frame.
//                int frame = Animation.BinarySearch(frames, time, ENTRIES);
//                r = frames[frame + PREV_R];
//                g = frames[frame + PREV_G];
//                b = frames[frame + PREV_B];
//                a = frames[frame + PREV_A];
//                float frameTime = frames[frame];
//                float percent = GetCurvePercent(frame / ENTRIES - 1,
//                                                1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
//
//                r += (frames[frame + R] - r) * percent;
//                g += (frames[frame + G] - g) * percent;
//                b += (frames[frame + B] - b) * percent;
//                a += (frames[frame + A] - a) * percent;
//            }
//            if (alpha == 1) {
//                slot.r = r;
//                slot.g = g;
//                slot.b = b;
//                slot.a = a;
//            } else {
//                float br, bg, bb, ba;
//                if (pose == MixPose_Setup) {
//                    br = slot.data.r;
//                    bg = slot.data.g;
//                    bb = slot.data.b;
//                    ba = slot.data.a;
//                } else {
//                    br = slot.r;
//                    bg = slot.g;
//                    bb = slot.b;
//                    ba = slot.a;
//                }
//                slot.r = br + ((r - br) * alpha);
//                slot.g = bg + ((g - bg) * alpha);
//                slot.b = bb + ((b - bb) * alpha);
//                slot.a = ba + ((a - ba) * alpha);
//            }
//        }
    };
}

#endif /* Spine_ColorTimeline_h */
