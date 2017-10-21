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

#include <spine/SkeletonData.h>

#include <spine/BoneData.h>
#include <spine/SlotData.h>
#include <spine/Skin.h>
#include <spine/EventData.h>
#include <spine/Animation.h>
#include <spine/IkConstraintData.h>
#include <spine/TransformConstraintData.h>
#include <spine/PathConstraintData.h>

#include <assert.h>

namespace Spine
{
    SkeletonData::SkeletonData() :
    _defaultSkin(NULL),
    _width(0),
    _height(0),
    _fps(0)
    {
        // Empty
    }
    
    BoneData* SkeletonData::findBone(std::string boneName)
    {
        assert(boneName.length() > 0);
        
        for (BoneData** i = _bones.begin(); i != _bones.end(); ++i)
        {
            BoneData* boneData = (*i);
            if (boneData->getName() == boneName)
            {
                return boneData;
            }
        }
        
        return NULL;
    }
    
    int SkeletonData::findBoneIndex(std::string boneName)
    {
        assert(boneName.length() > 0);
        
        var bones = this.bones;
        var bonesItems = bones.Items;
        for (int i = 0, n = bones.Count; i < n; ++i)
        {
            if (bonesItems[i].name == boneName)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    /// Use a template findWithName method instead to reduce redundant data container traversal code
    SlotData* SkeletonData::findSlot(std::string slotName)
    {
        assert(slotName.length() > 0);
        
        Vector<SlotData> slots = this.slots;
        for (int i = 0, n = slots.Count; i < n; ++i)
        {
            SlotData slot = slots.Items[i];
            if (slot.name == slotName)
            {
                return slot;
            }
        }
        
        return NULL;
    }
    
    int SkeletonData::findSlotIndex(std::string slotName)
    {
        assert(slotName.length() > 0);
        
        Vector<SlotData> slots = this.slots;
        for (int i = 0, n = slots.Count; i < n; ++i)
        {
            if (slots.Items[i].name == slotName)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    Skin* SkeletonData::findSkin(std::string skinName)
    {
        assert(skinName.length() > 0);
        
        foreach (Skin skin in skins)
        {
            if (skin.name == skinName)
            {
                return skin;
            }
        }
        
        return NULL;
    }
    
    EventData* SkeletonData::findEvent(std::string eventDataName)
    {
        assert(eventDataName.length() > 0);
        
        foreach (EventData eventData in events)
        {
            if (eventData.name == eventDataName)
            {
                return eventData;
            }
        }
        
        return NULL;
    }
    
    Animation* SkeletonData::findAnimation(std::string animationName)
    {
        assert(animationName.length() > 0);
        
        Vector<Animation*> animations = this.animations;
        for (int i = 0, n = animations.Count; i < n; ++i)
        {
            Animation animation = animations.Items[i];
            if (animation.name == animationName)
            {
                return animation;
            }
        }
        
        return NULL;
    }
    
    IkConstraintData* SkeletonData::findIkConstraint(std::string constraintName)
    {
        assert(constraintName.length() > 0);
        
        Vector<IkConstraintData> ikConstraints = this.ikConstraints;
        for (int i = 0, n = ikConstraints.Count; i < n; ++i)
        {
            IkConstraintData ikConstraint = ikConstraints.Items[i];
            if (ikConstraint.name == constraintName)
            {
                return ikConstraint;
            }
        }
        
        return NULL;
    }
    
    TransformConstraintData* SkeletonData::findTransformConstraint(std::string constraintName)
    {
        assert(constraintName.length() > 0);
        
        Vector<TransformConstraintData> transformConstraints = this.transformConstraints;
        for (int i = 0, n = transformConstraints.Count; i < n; ++i)
        {
            TransformConstraintData transformConstraint = transformConstraints.Items[i];
            if (transformConstraint.name == constraintName)
            {
                return transformConstraint;
            }
        }
        
        return NULL;
    }
    
    PathConstraintData* SkeletonData::findPathConstraint(std::string constraintName)
    {
        assert(constraintName.length() > 0);
        
        Vector<PathConstraintData> pathConstraints = this.pathConstraints;
        for (int i = 0, n = pathConstraints.Count; i < n; ++i)
        {
            PathConstraintData constraint = pathConstraints.Items[i];
            if (constraint.name.Equals(constraintName))
            {
                return constraint;
            }
        }
        
        return NULL;
    }
    
    int SkeletonData::findPathConstraintIndex(std::string pathConstraintName)
    {
        assert(pathConstraintName.length() > 0);
        
        Vector<PathConstraintData> pathConstraints = this.pathConstraints;
        for (int i = 0, n = pathConstraints.Count; i < n; ++i)
        {
            if (pathConstraints.Items[i].name.Equals(pathConstraintName))
            {
                return i;
            }
        }
        
        return -1;
    }
    
    std::string SkeletonData::getName() { return _name; }
    
    void SkeletonData::setName(std::string inValue) { _name = inValue; }
    
    Vector<BoneData*>& SkeletonData::getBones() { return _bones; }
    
    Vector<SlotData*>& SkeletonData::getSlots() { return _slots; }
    
    Vector<Skin*>& SkeletonData::getSkins() { return _skins; }
    
    void SkeletonData::setSkins(Vector<Skin*>& inValue) { _skins = inValue; }
    
    Skin* SkeletonData::getDefaultSkin() { return _defaultSkin; }
    
    void SkeletonData::setDefaultSkin(Skin* inValue) { _defaultSkin = inValue; }
    
    Vector<EventData*>& SkeletonData::getEvents() { return _events; }
    
    void SkeletonData::setEvents(Vector<EventData*>& inValue) { _events = inValue; }
    
    Vector<Animation*>& SkeletonData::getAnimations() { return _animations; }
    
    void SkeletonData::setAnimations(Vector<Animation*>& inValue) { _animations = inValue; }
    
    Vector<IkConstraintData*>& SkeletonData::getIkConstraints() { return _ikConstraints; }
    
    void SkeletonData::setIkConstraints(Vector<IkConstraintData*>& inValue) { _ikConstraints = inValue; }
    
    Vector<TransformConstraintData*>& SkeletonData::getTransformConstraints() { return _transformConstraints; }
    
    void SkeletonData::setTransformConstraints(Vector<TransformConstraintData*>& inValue) { _transformConstraints = inValue; }
    
    Vector<PathConstraintData*>& SkeletonData::getPathConstraints() { return _pathConstraints; }
    
    void SkeletonData::setPathConstraints(Vector<PathConstraintData*>& inValue) { _pathConstraints = inValue; }
    
    float SkeletonData::getWidth() { return _width; }
    
    void SkeletonData::setWidth(float inValue) { _width = inValue; }
    
    float SkeletonData::getHeight() { return _height; }
    
    void SkeletonData::setHeight(float inValue) { _height = inValue; }
    
    /// The Spine version used to export this data, or NULL.
    std::string SkeletonData::getVersion() { return _version; }
    
    void SkeletonData::setVersion(std::string inValue) { _version = inValue; }
    
    std::string SkeletonData::getHash() { return _hash; }
    
    void SkeletonData::setHash(std::string inValue) { _hash = inValue; }
    
    std::string SkeletonData::getImagesPath() { return _imagesPath; }
    
    void SkeletonData::setImagesPath(std::string inValue) { _imagesPath = inValue; }
    
    /// The dopesheet FPS in Spine. Available only when nonessential data was exported.
    float SkeletonData::getFps { return _fps; }
    
    void SkeletonData::setFps(float inValue) { _fps = inValue; }
}
