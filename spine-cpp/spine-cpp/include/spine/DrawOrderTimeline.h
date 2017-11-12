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

#ifndef Spine_DrawOrderTimeline_h
#define Spine_DrawOrderTimeline_h

#include <spine/Timeline.h>

namespace Spine
{
    class DrawOrderTimeline : public Timeline
    {
        RTTI_DECL;
        
        internal float[] frames;
        private int[][] drawOrders;
        
        public float[] Frames { return frames; } set { frames = inValue; } // time, ...
        public int[][] DrawOrders { return drawOrders; } set { drawOrders = inValue; }
        public int FrameCount { return frames.Length; }
        
        public int PropertyId {
            get { return ((int)TimelineType.DrawOrder << 24); }
        }
        
        public DrawOrderTimeline (int frameCount) {
            frames = new float[frameCount];
            drawOrders = new int[frameCount][];
        }
        
        /// Sets the time and value of the specified keyframe.
        /// <param name="drawOrder">May be NULL to use bind pose draw order.</param>
        public void SetFrame (int frameIndex, float time, int[] drawOrder) {
            frames[frameIndex] = time;
            drawOrders[frameIndex] = drawOrder;
        }
        
        public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
            Vector<Slot> drawOrder = skeleton.drawOrder;
            Vector<Slot> slots = skeleton.slots;
            if (direction == MixDirection_Out && pose == MixPose_Setup) {
                Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
                return;
            }
            
            float[] frames = _frames;
            if (time < frames[0]) {
                if (pose == MixPose_Setup) Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
                return;
            }
            
            int frame;
            if (time >= frames[frames.Length - 1]) // Time is after last frame.
                frame = frames.Length - 1;
            else
                frame = Animation.BinarySearch(frames, time) - 1;
            
            int[] drawOrderToSetupIndex = drawOrders[frame];
            if (drawOrderToSetupIndex == NULL) {
                drawOrder.Clear();
                for (int i = 0, n = slots.Count; i < n; i++)
                    drawOrder.Add(slots.Items[i]);
            } else {
                var drawOrderItems = drawOrder.Items;
                var slotsItems = slots.Items;
                for (int i = 0, n = drawOrderToSetupIndex.Length; i < n; i++)
                    drawOrderItems[i] = slotsItems[drawOrderToSetupIndex[i]];
            }
        }
    };
}

#endif /* Spine_DrawOrderTimeline_h */
