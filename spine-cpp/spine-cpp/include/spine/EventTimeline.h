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

#ifndef Spine_EventTimeline_h
#define Spine_EventTimeline_h

#include <spine/Timeline.h>

namespace Spine
{
    class EventTimeline : public Timeline
    {
        RTTI_DECL;
        
        virtual void apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*>& events, float alpha, MixPose pose, MixDirection direction);
        
        virtual int getPropertyId();
        
//        internal float[] frames;
//        private Event[] events;
//        
//        public float[] Frames { return frames; } set { frames = inValue; } // time, ...
//        public Event[] Events { return events; } set { events = inValue; }
//        public int FrameCount { return frames.Length; }
//
//        
//        public EventTimeline (int frameCount) {
//            frames = new float[frameCount];
//            events = new Event[frameCount];
//        }
//        
//        /// Sets the time and value of the specified keyframe.
//        public void setFrame (int frameIndex, Event e) {
//            frames[frameIndex] = e.Time;
//            events[frameIndex] = e;
//        }
//        
//        /// Fires events for frames &gt; lastTime and &lt;= time.
//        public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
//            if (firedEvents == NULL) return;
//            float[] frames = _frames;
//            int frameCount = frames.Length;
//            
//            if (lastTime > time) { // Fire events after last time for looped animations.
//                Apply(skeleton, lastTime, int.MaxValue, firedEvents, alpha, pose, direction);
//                lastTime = -1f;
//            } else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
//                return;
//            if (time < frames[0]) return; // Time is before first frame.
//            
//            int frame;
//            if (lastTime < frames[0])
//                frame = 0;
//            else {
//                frame = Animation::binarySearch(frames, lastTime);
//                float frameTime = frames[frame];
//                while (frame > 0) { // Fire multiple events with the same frame.
//                    if (frames[frame - 1] != frameTime) break;
//                    frame--;
//                }
//            }
//            for (; frame < frameCount && time >= frames[frame]; frame++)
//                firedEvents.push_back(events[frame]);
//        }
    };
}

#endif /* Spine_EventTimeline_h */
