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

#include <spine/Slot.h>

#include <spine/SlotData.h>
#include <spine/Bone.h>
#include <spine/Skeleton.h>
#include <spine/Attachment.h>

namespace Spine
{
    Slot::Slot(SlotData& data, Bone& bone) :
    _slotData(data),
    _bone(bone),
    _skeleton(bone.getSkeletion()),
    _r(1),
    _g(1),
    _b(1),
    _a(1),
    _r2(0),
    _g2(0),
    _b2(0),
    _hasSecondColor(false),
    _attachment(NULL),
    _attachmentTime(0)
    {
        setToSetupPose();
    }
    
    void Slot::setToSetupPose()
    {
        _r = _slotData.getR();
        _g = _slotData.getG();
        _b = _slotData.getB();
        _a = _slotData.getA();
        
        std::string attachmentName = _slotData.getAttachmentName();
        if (attachmentName.length() > 0)
        {
            _attachment = NULL;
            setAttachment(_skeleton.getAttachment(_slotData.getIndex(), attachmentName));
        }
        else
        {
            setAttachment(NULL);
        }
    }
    
    const SlotData& Slot::getSlotData()
    {
        return _slotData;
    }
    
    const Bone& Slot::getBone()
    {
        return _bone;
    }
    
    const Skeleton& Slot::getSkeleton()
    {
        return _skeleton;
    }
    
    float Slot::getR()
    {
        return _r;
    }
    
    void Slot::setR(float inValue)
    {
        _r = inValue;
    }
    
    float Slot::getG()
    {
        return _g;
    }
    
    void Slot::setG(float inValue)
    {
        _g = inValue;
    }
    
    float Slot::getB()
    {
        return _b;
    }
    
    void Slot::setB(float inValue)
    {
        _b = inValue;
    }
    
    float Slot::getA()
    {
        return _a;
    }
    
    void Slot::setA(float inValue)
    {
        _a = inValue;
    }
    
    float Slot::getR2()
    {
        return _r2;
    }
    
    void Slot::setR2(float inValue)
    {
        _r2 = inValue;
    }
    
    float Slot::getG2()
    {
        return _g2;
    }
    
    void Slot::setG2(float inValue)
    {
        _g2 = inValue;
    }
    
    float Slot::getB2()
    {
        return _b2;
    }
    
    void Slot::setB2(float inValue)
    {
        _b2 = inValue;
    }
    
    bool Slot::hasSecondColor()
    {
        return _hasSecondColor;
    }
    
    void Slot::setHasSecondColor(bool inValue)
    {
        _hasSecondColor = inValue;
    }
    
    Attachment* Slot::getAttachment()
    {
        return _attachment;
    }
    
    void Slot::setAttachment(Attachment* inValue)
    {
        if (_attachment == inValue)
        {
            return;
        }
        
        _attachment = inValue;
        _attachmentTime = _skeleton.getTime();
        _attachmentVertices.clear();
    }
    
    float Slot::getAttachmentTime()
    {
        return _skeleton.getTime() - _attachmentTime;
    }
    
    void Slot::setAttachmentTime(float inValue)
    {
        _attachmentTime = _skeleton.getTime() - inValue;
    }
    
    Vector<float>& Slot::getAttachmentVertices()
    {
        return _attachmentVertices;
    }
    
    void Slot::setAttachmentVertices(Vector<float> inValue)
    {
        _attachmentVertices = inValue;
    }
}
