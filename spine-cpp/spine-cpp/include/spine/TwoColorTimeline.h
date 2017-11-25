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

#ifndef Spine_TwoColorTimeline_h
#define Spine_TwoColorTimeline_h

#include <spine/CurveTimeline.h>

namespace Spine
{
    class TwoColorTimeline : public CurveTimeline
    {
        SPINE_RTTI_DECL;
        
        virtual void apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*>& events, float alpha, MixPose pose, MixDirection direction);
        
        virtual int getPropertyId();
        
//        public const int ENTRIES = 8;
//        protected const int PREV_TIME = -8, PREV_R = -7, PREV_G = -6, PREV_B = -5, PREV_A = -4;
//        protected const int PREV_R2 = -3, PREV_G2 = -2, PREV_B2 = -1;
//        protected const int R = 1, G = 2, B = 3, A = 4, R2 = 5, G2 = 6, B2 = 7;
//
//        internal float[] frames; // time, r, g, b, a, r2, g2, b2, ...
//        public float[] Frames { return frames; }
//
//        internal int slotIndex;
//        public int SlotIndex {
//            get { return slotIndex; }
//            set {
//                if (value < 0) throw new ArgumentOutOfRangeException("index must be >= 0.");
//                    slotIndex = inValue;
//            }
//        }
//
//        override public int PropertyId {
//            get { return ((int)TimelineType.TwoColor << 24) + slotIndex; }
//        }
//
//        public TwoColorTimeline (int frameCount) :
//        base(frameCount) {
//            frames = new float[frameCount * ENTRIES];
//        }
//
//        /// Sets the time and value of the specified keyframe.
//        public void SetFrame (int frameIndex, float time, float r, float g, float b, float a, float r2, float g2, float b2) {
//            frameIndex *= ENTRIES;
//            frames[frameIndex] = time;
//            frames[frameIndex + R] = r;
//            frames[frameIndex + G] = g;
//            frames[frameIndex + B] = b;
//            frames[frameIndex + A] = a;
//            frames[frameIndex + R2] = r2;
//            frames[frameIndex + G2] = g2;
//            frames[frameIndex + B2] = b2;
//        }
//
//        override public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
//            Slot slot = skeleton.slots.Items[slotIndex];
//            float[] frames = _frames;
//            if (time < frames[0]) { // Time is before first frame.
//                var slotData = slot.data;
//                switch (pose) {
//                    case MixPose_Setup:
//                        //    slot.color.set(slot.data.color);
//                        //    slot.darkColor.set(slot.data.darkColor);
//                        slot.r = slotData.r;
//                        slot.g = slotData.g;
//                        slot.b = slotData.b;
//                        slot.a = slotData.a;
//                        slot.r2 = slotData.r2;
//                        slot.g2 = slotData.g2;
//                        slot.b2 = slotData.b2;
//                        return;
//                    case MixPose_Current:
//                        slot.r += (slot.r - slotData.r) * alpha;
//                        slot.g += (slot.g - slotData.g) * alpha;
//                        slot.b += (slot.b - slotData.b) * alpha;
//                        slot.a += (slot.a - slotData.a) * alpha;
//                        slot.r2 += (slot.r2 - slotData.r2) * alpha;
//                        slot.g2 += (slot.g2 - slotData.g2) * alpha;
//                        slot.b2 += (slot.b2 - slotData.b2) * alpha;
//                        return;
//                }
//                return;
//            }
//
//            float r, g, b, a, r2, g2, b2;
//            if (time >= frames[frames.Length - ENTRIES]) { // Time is after last frame.
//                int i = frames.Length;
//                r = frames[i + PREV_R];
//                g = frames[i + PREV_G];
//                b = frames[i + PREV_B];
//                a = frames[i + PREV_A];
//                r2 = frames[i + PREV_R2];
//                g2 = frames[i + PREV_G2];
//                b2 = frames[i + PREV_B2];
//            } else {
//                // Interpolate between the previous frame and the current frame.
//                int frame = Animation.BinarySearch(frames, time, ENTRIES);
//                r = frames[frame + PREV_R];
//                g = frames[frame + PREV_G];
//                b = frames[frame + PREV_B];
//                a = frames[frame + PREV_A];
//                r2 = frames[frame + PREV_R2];
//                g2 = frames[frame + PREV_G2];
//                b2 = frames[frame + PREV_B2];
//                float frameTime = frames[frame];
//                float percent = GetCurvePercent(frame / ENTRIES - 1,
//                                                1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
//
//                r += (frames[frame + R] - r) * percent;
//                g += (frames[frame + G] - g) * percent;
//                b += (frames[frame + B] - b) * percent;
//                a += (frames[frame + A] - a) * percent;
//                r2 += (frames[frame + R2] - r2) * percent;
//                g2 += (frames[frame + G2] - g2) * percent;
//                b2 += (frames[frame + B2] - b2) * percent;
//            }
//            if (alpha == 1) {
//                slot.r = r;
//                slot.g = g;
//                slot.b = b;
//                slot.a = a;
//                slot.r2 = r2;
//                slot.g2 = g2;
//                slot.b2 = b2;
//            } else {
//                float br, bg, bb, ba, br2, bg2, bb2;
//                if (pose == MixPose_Setup) {
//                    br = slot.data.r;
//                    bg = slot.data.g;
//                    bb = slot.data.b;
//                    ba = slot.data.a;
//                    br2 = slot.data.r2;
//                    bg2 = slot.data.g2;
//                    bb2 = slot.data.b2;
//                } else {
//                    br = slot.r;
//                    bg = slot.g;
//                    bb = slot.b;
//                    ba = slot.a;
//                    br2 = slot.r2;
//                    bg2 = slot.g2;
//                    bb2 = slot.b2;
//                }
//                slot.r = br + ((r - br) * alpha);
//                slot.g = bg + ((g - bg) * alpha);
//                slot.b = bb + ((b - bb) * alpha);
//                slot.a = ba + ((a - ba) * alpha);
//                slot.r2 = br2 + ((r2 - br2) * alpha);
//                slot.g2 = bg2 + ((g2 - bg2) * alpha);
//                slot.b2 = bb2 + ((b2 - bb2) * alpha);
//            }
//        }
    };
}

#endif /* Spine_TwoColorTimeline_h */
