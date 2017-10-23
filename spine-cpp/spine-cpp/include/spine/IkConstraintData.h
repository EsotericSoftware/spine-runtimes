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

#ifndef Spine_IkConstraintData_h
#define Spine_IkConstraintData_h

#include <spine/Vector.h>

#include <string>

namespace Spine
{
    class BoneData;
    
    class IkConstraintData
    {
    public:
        IkConstraintData(std::string name);
        
        /// The IK constraint's name, which is unique within the skeleton.
        const std::string& getName();
        
        int getOrder();
        void setOrder(int inValue);
        
        /// The bones that are constrained by this IK Constraint.
        Vector<BoneData*>& getBones();
        
        /// The bone that is the IK target.
        BoneData* getTarget();
        void setTarget(BoneData* inValue);
        
        /// Controls the bend direction of the IK bones, either 1 or -1.
        int getBendDirection();
        void setBendDirection(int inValue);
        
        float getMix();
        void setMix(float inValue);
        
    private:
        const std::string _name;
        int _order;
        Vector<BoneData*> _bones;
        BoneData* _target;
        int _bendDirection;
        float _mix;
    };
}

#endif /* Spine_IkConstraintData_h */
