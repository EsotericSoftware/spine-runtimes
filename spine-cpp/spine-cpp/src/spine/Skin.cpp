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

#include <spine/Skin.h>

#include <spine/Attachment.h>
#include <spine/Skeleton.h>

#include <spine/Slot.h>

#include <assert.h>

namespace Spine
{
    Skin::AttachmentKey::AttachmentKey(int slotIndex, std::string name) :
    _slotIndex(slotIndex),
    _name(name)
    {
        // Empty
    }
    
    bool Skin::AttachmentKey::operator==(const AttachmentKey &other) const
    {
        return _slotIndex == other._slotIndex && _name == other._name;
    }
    
    Skin::Skin(std::string name) : _name(name)
    {
        assert(_name.length() > 0);
    }
    
    void Skin::addAttachment(int slotIndex, std::string name, Attachment* attachment)
    {
        assert(attachment);
        
        _attachments[AttachmentKey(slotIndex, name)] = attachment;
    }
    
    Attachment* Skin::getAttachment(int slotIndex, std::string name)
    {
        HashMap<AttachmentKey, Attachment*, HashAttachmentKey>::Iterator i = _attachments.find(AttachmentKey(slotIndex, name));
        
        Attachment* ret = NULL;
        
        if (i != _attachments.end())
        {
            ret = i.second();
        }
        
        return ret;
    }
    
    void Skin::findNamesForSlot(int slotIndex, SimpleArray<std::string>& names)
    {
        for (HashMap<AttachmentKey, Attachment*, HashAttachmentKey>::Iterator i = _attachments.begin(); i != _attachments.end(); ++i)
        {
            if (i.first()._slotIndex == slotIndex)
            {
                names.push_back(i.first()._name);
            }
        }
    }
    
    void Skin::findAttachmentsForSlot(int slotIndex, SimpleArray<Attachment*>& attachments)
    {
        for (HashMap<AttachmentKey, Attachment*, HashAttachmentKey>::Iterator i = _attachments.begin(); i != _attachments.end(); ++i)
        {
            if (i.first()._slotIndex == slotIndex)
            {
                attachments.push_back(i.second());
            }
        }
    }
    
    const std::string& Skin::getName()
    {
        return _name;
    }
    
    HashMap<Skin::AttachmentKey, Attachment*, HashAttachmentKey>& Skin::getAttachments()
    {
        return _attachments;
    }
    
    void Skin::attachAll(Skeleton& skeleton, Skin& oldSkin)
    {
        for (HashMap<AttachmentKey, Attachment*, HashAttachmentKey>::Iterator i = oldSkin.getAttachments().begin(); i != oldSkin.getAttachments().end(); ++i)
        {
            int slotIndex = i.first()._slotIndex;
            Slot* slot = skeleton.getSlots().at(slotIndex);
            
            if (slot->getAttachment() == i.second())
            {
                Attachment* attachment = NULL;
                if ((attachment = getAttachment(slotIndex, i.first()._name)))
                {
                    slot->setAttachment(attachment);
                }
            }
        }
    }
}
