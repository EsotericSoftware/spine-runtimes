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

#ifndef Spine_SlotData_h
#define Spine_SlotData_h

#include <spine/BlendMode.h>

#include <string>

namespace Spine
{
    class BoneData;
    
    class SlotData
    {
    public:
        SlotData(int index, std::string name, BoneData& boneData);
        
        const int getIndex();
        
        const std::string& getName();
        
        BoneData& getBoneData();
        
        float getR();
        void setR(float inValue);
        float getG();
        void setG(float inValue);
        float getB();
        void setB(float inValue);
        float getA();
        void setA(float inValue);
        
        float getR2();
        void setR2(float inValue);
        float getG2();
        void setG2(float inValue);
        float getB2();
        void setB2(float inValue);
        bool hasSecondColor();
        void setHasSecondColor(bool inValue);
        
        /// May be empty.
        std::string getAttachmentName();
        void setAttachmentName(std::string inValue);
        
        BlendMode getBlendMode();
        void setBlendMode(BlendMode inValue);
        
    private:
        const int _index;
        const std::string _name;
        BoneData& _boneData;
        float _r, _g, _b, _a;
        float _r2, _g2, _b2;
        bool _hasSecondColor;
        std::string _attachmentName;
        BlendMode _blendMode;
    };
}

#endif /* Spine_SlotData_h */
