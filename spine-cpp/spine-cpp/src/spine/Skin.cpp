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

#include <assert.h>

namespace Spine
{
    Skin::Skin(std::string name) : _name(name)
    {
        assert(_name.length() > 0);
    }
    
    void Skin::addAttachment(int slotIndex, std::string name, Attachment* attachment)
    {
        assert(attachment);
        
        _attachments[AttachmentKey(slotIndex, name)] = attachment;
    }
    
    /// Returns the attachment for the specified slot index and name, or null.
    Attachment* Skin::getAttachment(int slotIndex, std::string name)
    {
        std::iterator<AttachmentKey, Attachment*> q = _attachments.find(AttachmentKey(slotIndex, name));
        
        Attachment* ret = nullptr;
        
        if (q != _attachments.end())
        {
            ret = q->second;
        }
        
        return ret;
    }
    
    struct sum
    {
        sum(int * t):total(t){};
        int * total;
        
        void operator()(AttachmentKey key)
        {
            *total+=element;
        }
    };
    
    /// Finds the skin keys for a given slot. The results are added to the passed vector names.
    /// @param slotIndex The target slotIndex. To find the slot index, use Skeleton::findSlotIndex or SkeletonData::findSlotIndex
    /// @param names Found skin key names will be added to this vector.
    void Skin::findNamesForSlot(int slotIndex, std::vector<std::string>& names)
    {
        foreach (AttachmentKey key in attachments.Keys)
        if (key.slotIndex == slotIndex) names.Add(key.name);
    }
    
    /// Finds the attachments for a given slot. The results are added to the passed List(Attachment).
    /// @param slotIndex The target slotIndex. To find the slot index, use Skeleton::findSlotIndex or SkeletonData::findSlotIndex
    /// @param attachments Found Attachments will be added to this vector.
    void Skin::findAttachmentsForSlot(int slotIndex, std::vector<Attachment*>& attachments)
    {
        foreach (KeyValuePair<AttachmentKey, Attachment> entry in this.attachments)
        if (entry.Key.slotIndex == slotIndex) attachments.Add(entry.Value);
    }
    
    const std::string& Skin::getName()
    {
        //
    }
    
    std::unordered_map<AttachmentKey, Attachment*>& Skin::getAttachments()
    {
        //
    }
    
    void Skin::attachAll(Skeleton& skeleton, Skin& oldSkin)
    {
        foreach (KeyValuePair<AttachmentKey, Attachment> entry in oldSkin.attachments)
        {
            int slotIndex = entry.Key.slotIndex;
            Slot slot = skeleton.slots.Items[slotIndex];
            if (slot.Attachment == entry.Value) {
                Attachment attachment = GetAttachment(slotIndex, entry.Key.name);
                if (attachment != null) slot.Attachment = attachment;
            }
        }
    }
    
    AttachmentKey::AttachmentKey(int slotIndex, std::string name) :
    _slotIndex(slotIndex),
    _name(name)
    {
        // Empty
    }
    
    bool AttachmentKey::operator==(const AttachmentKey &other) const
    {
        return _slotIndex == other._slotIndex && _name == other._name;
    }
    
    std::ostream& operator <<(std::ostream& os, const Skin& ref)
    {
        os << ref.getName();
        
        return os;
    }
}
