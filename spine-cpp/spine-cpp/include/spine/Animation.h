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

#ifndef Spine_Animation_h
#define Spine_Animation_h

#include <spine/Vector.h>
#include <spine/MixPose.h>
#include <spine/MixDirection.h>

#include <string>

namespace Spine
{
    class Timeline;
    class Skeleton;
    class Event;
    
    class Animation
    {
        friend class RotateTimeline;
        
    public:
        Animation(std::string name, Vector<Timeline*>& timelines, float duration);
        
        /// Applies all the animation's timelines to the specified skeleton.
        /// See also Timeline::apply(Skeleton&, float, float, Vector, float, MixPose, MixDirection)
        void apply(Skeleton& skeleton, float lastTime, float time, bool loop, Vector<Event*>& events, float alpha, MixPose pose, MixDirection direction);
        
        std::string getName();
        
        Vector<Timeline*> getTimelines();
        
        void setTimelines(Vector<Timeline*> inValue);
        
        float getDuration();
        
        void setDuration(float inValue);
        
    private:
        Vector<Timeline*> _timelines;
        float _duration;
        std::string _name;
        
        /// @param target After the first and before the last entry.
        static int binarySearch(Vector<float>& values, float target, int step);
        
        /// @param target After the first and before the last entry.
        static int binarySearch(Vector<float>& values, float target);
        
        static int linearSearch(Vector<float>& values, float target, int step);
    };
}

#endif /* Spine_Animation_h */
