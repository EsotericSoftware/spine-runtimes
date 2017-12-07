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

#include <spine/Skeleton.h>

#include <spine/SkeletonData.h>
#include <spine/Bone.h>
#include <spine/Updatable.h>
#include <spine/Slot.h>
#include <spine/IkConstraint.h>
#include <spine/PathConstraint.h>
#include <spine/TransformConstraint.h>
#include <spine/Skin.h>
#include <spine/Attachment.h>

#include <spine/BoneData.h>
#include <spine/SlotData.h>
#include <spine/IkConstraintData.h>
#include <spine/TransformConstraintData.h>
#include <spine/PathConstraintData.h>
#include <spine/RegionAttachment.h>
#include <spine/MeshAttachment.h>
#include <spine/PathAttachment.h>

#include <spine/ContainerUtil.h>
#include <spine/Extension.h>

namespace Spine
{
    Skeleton::Skeleton(SkeletonData& skeletonData) :
    _data(skeletonData),
    _skin(NULL),
    _r(1),
    _g(1),
    _b(1),
    _a(1),
    _time(0),
    _flipX(false),
    _flipY(false),
    _x(0),
    _y(0)
    {
        _bones.reserve(_data.getBones().size());
        for (BoneData** i = _data.getBones().begin(); i != _data.getBones().end(); ++i)
        {
            BoneData* data = (*i);
            
            Bone* bone;
            if (data->getParent() == NULL)
            {
                bone = NEW(Bone);
                new (bone) Bone(*data, *this, NULL);
            }
            else
            {
                Bone* parent = _bones[data->getParent()->getIndex()];
                bone = NEW(Bone);
                new (bone) Bone(*data, *this, parent);
                parent->getChildren().push_back(bone);
            }
            
            _bones.push_back(bone);
        }
        
        _slots.reserve(_data.getSlots().size());
        _drawOrder.reserve(_data.getSlots().size());
        for (SlotData** i = _data.getSlots().begin(); i != _data.getSlots().end(); ++i)
        {
            SlotData* data = (*i);
            
            Bone* bone = _bones[data->getBoneData().getIndex()];
            Slot* slot = NEW(Slot);
            new (slot) Slot(*data, *bone);
            
            _slots.push_back(slot);
            _drawOrder.push_back(slot);
        }
        
        _ikConstraints.reserve(_data.getIkConstraints().size());
        for (IkConstraintData** i = _data.getIkConstraints().begin(); i != _data.getIkConstraints().end(); ++i)
        {
            IkConstraintData* data = (*i);
            
            IkConstraint* constraint = NEW(IkConstraint);
            new (constraint) IkConstraint(*data, *this);
            
            _ikConstraints.push_back(constraint);
        }
        
        _transformConstraints.reserve(_data.getTransformConstraints().size());
        for (TransformConstraintData** i = _data.getTransformConstraints().begin(); i != _data.getTransformConstraints().end(); ++i)
        {
            TransformConstraintData* data = (*i);
            
            TransformConstraint* constraint = NEW(TransformConstraint);
            new (constraint) TransformConstraint(*data, *this);
            
            _transformConstraints.push_back(constraint);
        }
        
        _pathConstraints.reserve(_data.getPathConstraints().size());
        for (PathConstraintData** i = _data.getPathConstraints().begin(); i != _data.getPathConstraints().end(); ++i)
        {
            PathConstraintData* data = (*i);
            
            PathConstraint* constraint = NEW(PathConstraint);
            new (constraint) PathConstraint(*data, *this);
            
            _pathConstraints.push_back(constraint);
        }
        
        updateCache();
        updateWorldTransform();
    }
    
    Skeleton::~Skeleton()
    {
        ContainerUtil::cleanUpVectorOfPointers(_bones);
        ContainerUtil::cleanUpVectorOfPointers(_slots);
        ContainerUtil::cleanUpVectorOfPointers(_ikConstraints);
        ContainerUtil::cleanUpVectorOfPointers(_transformConstraints);
        ContainerUtil::cleanUpVectorOfPointers(_pathConstraints);
    }
    
    void Skeleton::updateCache()
    {
        _updateCache.clear();
        _updateCacheReset.clear();
        
        for (int i = 0, n = static_cast<int>(_bones.size()); i < n; ++i)
        {
            _bones[i]->_sorted = false;
        }
        
        int ikCount = static_cast<int>(_ikConstraints.size());
        int transformCount = static_cast<int>(_transformConstraints.size());
        int pathCount = static_cast<int>(_pathConstraints.size());
        
        int constraintCount = ikCount + transformCount + pathCount;
        
        for (int i = 0; i < constraintCount; ++i)
        {
            bool gotoNextConstraintCount = false;
            
            for (int ii = 0; ii < ikCount; ++ii)
            {
                IkConstraint* constraint = _ikConstraints[ii];
                if (constraint->getData().getOrder() == i)
                {
                    sortIkConstraint(constraint);
                    
                    gotoNextConstraintCount = true;
                    break;
                }
            }
            
            if (gotoNextConstraintCount)
            {
                break;
            }
            
            for (int ii = 0; ii < transformCount; ++ii)
            {
                TransformConstraint* constraint = _transformConstraints[ii];
                if (constraint->getData().getOrder() == i)
                {
                    sortTransformConstraint(constraint);
                    
                    gotoNextConstraintCount = true;
                    break;
                }
            }
            
            if (gotoNextConstraintCount)
            {
                break;
            }
            
            for (int ii = 0; ii < pathCount; ++ii)
            {
                PathConstraint* constraint = _pathConstraints[ii];
                if (constraint->getData().getOrder() == i)
                {
                    sortPathConstraint(constraint);
                    
                    gotoNextConstraintCount = true;
                    break;
                }
            }
            
            if (gotoNextConstraintCount)
            {
                break;
            }
        }
        
        for (int i = 0, n = static_cast<int>(_bones.size()); i < n; ++i)
        {
            sortBone(_bones[i]);
        }
    }
    
    void Skeleton::updateWorldTransform()
    {
        for (int i = 0, n = static_cast<int>(_updateCacheReset.size()); i < n; ++i)
        {
            Bone* boneP = _updateCacheReset[i];
            Bone& bone = *boneP;
            bone._ax = bone._x;
            bone._ay = bone._y;
            bone._arotation = bone._rotation;
            bone._ascaleX = bone._scaleX;
            bone._ascaleY = bone._scaleY;
            bone._ashearX = bone._shearX;
            bone._ashearY = bone._shearY;
            bone._appliedValid = true;
        }
        
        for (int i = 0, n = static_cast<int>(_updateCache.size()); i < n; ++i)
        {
            _updateCache[i]->update();
        }
    }
    
    void Skeleton::setToSetupPose()
    {
        setBonesToSetupPose();
        setSlotsToSetupPose();
    }
    
    void Skeleton::setBonesToSetupPose()
    {
        for (int i = 0, n = static_cast<int>(_bones.size()); i < n; ++i)
        {
            _bones[i]->setToSetupPose();
        }
        
        for (int i = 0, n = static_cast<int>(_ikConstraints.size()); i < n; ++i)
        {
            IkConstraint* constraintP = _ikConstraints[i];
            IkConstraint& constraint = *constraintP;
            
            constraint._bendDirection = constraint._data._bendDirection;
            constraint._mix = constraint._data._mix;
        }
        
        for (int i = 0, n = static_cast<int>(_transformConstraints.size()); i < n; ++i)
        {
            TransformConstraint* constraintP = _transformConstraints[i];
            TransformConstraint& constraint = *constraintP;
            TransformConstraintData& constraintData = constraint._data;
            
            constraint._rotateMix = constraintData._rotateMix;
            constraint._translateMix = constraintData._translateMix;
            constraint._scaleMix = constraintData._scaleMix;
            constraint._shearMix = constraintData._shearMix;
        }
        
        for (int i = 0, n = static_cast<int>(_pathConstraints.size()); i < n; ++i)
        {
            PathConstraint* constraintP = _pathConstraints[i];
            PathConstraint& constraint = *constraintP;
            PathConstraintData& constraintData = constraint._data;
            
            constraint._position = constraintData._position;
            constraint._spacing = constraintData._spacing;
            constraint._rotateMix = constraintData._rotateMix;
            constraint._translateMix = constraintData._translateMix;
        }
    }
    
    void Skeleton::setSlotsToSetupPose()
    {
        _drawOrder.clear();
        for (int i = 0, n = static_cast<int>(_slots.size()); i < n; ++i)
        {
            _drawOrder.push_back(_slots[i]);
        }
        
        for (int i = 0, n = static_cast<int>(_slots.size()); i < n; ++i)
        {
            _slots[i]->setToSetupPose();
        }
    }
    
    Bone* Skeleton::findBone(std::string boneName)
    {
        return ContainerUtil::findWithDataName(_bones, boneName);
    }
    
    int Skeleton::findBoneIndex(std::string boneName)
    {
        return ContainerUtil::findIndexWithDataName(_bones, boneName);
    }
    
    Slot* Skeleton::findSlot(std::string slotName)
    {
        return ContainerUtil::findWithDataName(_slots, slotName);
    }
    
    int Skeleton::findSlotIndex(std::string slotName)
    {
        return ContainerUtil::findIndexWithDataName(_slots, slotName);
    }
    
    void Skeleton::setSkin(std::string skinName)
    {
        Skin* foundSkin = _data.findSkin(skinName);
        
        assert(foundSkin != NULL);
        
        setSkin(foundSkin);
    }
    
    void Skeleton::setSkin(Skin* newSkin)
    {
        if (newSkin != NULL)
        {
            if (_skin != NULL)
            {
                Skeleton& thisRef = *this;
                newSkin->attachAll(thisRef, *_skin);
            }
            else
            {
                for (int i = 0, n = static_cast<int>(_slots.size()); i < n; ++i)
                {
                    Slot* slotP = _slots[i];
                    Slot& slot = *slotP;
                    std::string name = slot._data.getAttachmentName();
                    if (name.length() > 0)
                    {
                        Attachment* attachment = newSkin->getAttachment(i, name);
                        if (attachment != NULL)
                        {
                            slot.setAttachment(attachment);
                        }
                    }
                }
            }
        }
        
        _skin = newSkin;
    }
    
    Attachment* Skeleton::getAttachment(std::string slotName, std::string attachmentName)
    {
        return getAttachment(_data.findSlotIndex(slotName), attachmentName);
    }
    
    Attachment* Skeleton::getAttachment(int slotIndex, std::string attachmentName)
    {
        assert(attachmentName.length() > 0);
        
        if (_skin != NULL)
        {
            Attachment* attachment = _skin->getAttachment(slotIndex, attachmentName);
            if (attachment != NULL)
            {
                return attachment;
            }
        }
        
        return _data.getDefaultSkin() != NULL ? _data.getDefaultSkin()->getAttachment(slotIndex, attachmentName) : NULL;
    }
    
    void Skeleton::setAttachment(std::string slotName, std::string attachmentName)
    {
        assert(slotName.length() > 0);
        
        for (int i = 0, n = static_cast<int>(_slots.size()); i < n; ++i)
        {
            Slot* slot = _slots[i];
            if (slot->_data.getName() == slotName)
            {
                Attachment* attachment = NULL;
                if (attachmentName.length() > 0)
                {
                    attachment = getAttachment(i, attachmentName);
                    
                    assert(attachment != NULL);
                }
                
                slot->setAttachment(attachment);
                
                return;
            }
        }
        
        printf("Slot not found: %s", slotName.c_str());
        
        assert(false);
    }
    
    IkConstraint* Skeleton::findIkConstraint(std::string constraintName)
    {
        assert(constraintName.length() > 0);
        
        for (int i = 0, n = static_cast<int>(_ikConstraints.size()); i < n; ++i)
        {
            IkConstraint* ikConstraint = _ikConstraints[i];
            if (ikConstraint->_data.getName() == constraintName)
            {
                return ikConstraint;
            }
        }
        return NULL;
    }
    
    TransformConstraint* Skeleton::findTransformConstraint(std::string constraintName)
    {
        assert(constraintName.length() > 0);
        
        for (int i = 0, n = static_cast<int>(_transformConstraints.size()); i < n; ++i)
        {
            TransformConstraint* transformConstraint = _transformConstraints[i];
            if (transformConstraint->_data.getName() == constraintName)
            {
                return transformConstraint;
            }
        }
        
        return NULL;
    }
    
    PathConstraint* Skeleton::findPathConstraint(std::string constraintName)
    {
        assert(constraintName.length() > 0);
        
        for (int i = 0, n = static_cast<int>(_pathConstraints.size()); i < n; ++i)
        {
            PathConstraint* constraint = _pathConstraints[i];
            if (constraint->_data.getName() == constraintName)
            {
                return constraint;
            }
        }
        
        return NULL;
    }
    
    void Skeleton::update(float delta)
    {
        _time += delta;
    }
    
    void Skeleton::getBounds(float& outX, float& outY, float& outWidth, float& outHeight, Vector<float>& outVertexBuffer)
    {
        float minX = std::numeric_limits<float>::max();
        float minY = std::numeric_limits<float>::max();
        float maxX = std::numeric_limits<float>::min();
        float maxY = std::numeric_limits<float>::min();
        
        for (Slot** i = _drawOrder.begin(); i != _drawOrder.end(); ++i)
        {
            Slot* slot = (*i);
            int verticesLength = 0;
            Attachment* attachment = slot->getAttachment();
            
            if (attachment != NULL && attachment->getRTTI().derivesFrom(RegionAttachment::rtti))
            {
                RegionAttachment* regionAttachment = static_cast<RegionAttachment*>(attachment);

                verticesLength = 8;
                if (outVertexBuffer.size() < 8)
                {
                    outVertexBuffer.reserve(8);
                    outVertexBuffer.setSize(8);
                }
                regionAttachment->computeWorldVertices(slot->getBone(), outVertexBuffer, 0);
            }
            else if (attachment != NULL && attachment->getRTTI().derivesFrom(MeshAttachment::rtti))
            {
                MeshAttachment* mesh = static_cast<MeshAttachment*>(attachment);

                verticesLength = mesh->getWorldVerticesLength();
                if (outVertexBuffer.size() < verticesLength)
                {
                    outVertexBuffer.reserve(verticesLength);
                    outVertexBuffer.setSize(verticesLength);
                }

                mesh->computeWorldVertices(*slot, 0, verticesLength, outVertexBuffer, 0);
            }
            
            for (int ii = 0; ii < verticesLength; ii += 2)
            {
                float vx = outVertexBuffer[ii];
                float vy = outVertexBuffer[ii + 1];
                
                minX = MIN(minX, vx);
                minY = MIN(minY, vy);
                maxX = MAX(maxX, vx);
                maxY = MAX(maxY, vy);
            }
        }
        
        outX = minX;
        outY = minY;
        outWidth = maxX - minX;
        outHeight = maxY - minY;
    }
    
    Bone* Skeleton::getRootBone()
    {
        return _bones.size() == 0 ? NULL : _bones[0];
    }
    
    const SkeletonData& Skeleton::getData()
    {
        return _data;
    }
    
    Vector<Bone*>& Skeleton::getBones()
    {
        return _bones;
    }
    
    Vector<Updatable*>& Skeleton::getUpdateCacheList()
    {
        return _updateCache;
    }
    
    Vector<Slot*>& Skeleton::getSlots()
    {
        return _slots;
    }
    
    Vector<Slot*>& Skeleton::getDrawOrder()
    {
        return _drawOrder;
    }
    
    Vector<IkConstraint*>& Skeleton::getIkConstraints()
    {
        return _ikConstraints;
    }
    
    Vector<PathConstraint*>& Skeleton::getPathConstraints()
    {
        return _pathConstraints;
    }
    
    Vector<TransformConstraint*>& Skeleton::getTransformConstraints()
    {
        return _transformConstraints;
    }
    
    Skin* Skeleton::getSkin()
    {
        return _skin;
    }
    
    float Skeleton::getR()
    {
        return _r;
    }
    
    void Skeleton::setR(float inValue)
    {
        _r = inValue;
    }
    
    float Skeleton::getG()
    {
        return _g;
    }
    
    void Skeleton::setG(float inValue)
    {
        _g = inValue;
    }
    
    float Skeleton::getB()
    {
        return _b;
    }
    
    void Skeleton::setB(float inValue)
    {
        _b = inValue;
    }
    
    float Skeleton::getA()
    {
        return _a;
    }
    
    void Skeleton::setA(float inValue)
    {
        _a = inValue;
    }
    
    float Skeleton::getTime()
    {
        return _time;
    }
    
    void Skeleton::setTime(float inValue)
    {
        _time = inValue;
    }
    
    float Skeleton::getX()
    {
        return _x;
    }
    
    void Skeleton::setX(float inValue)
    {
        _x = inValue;
    }
    
    float Skeleton::getY()
    {
        return _y;
    }
    
    void Skeleton::setY(float inValue)
    {
        _y = inValue;
    }
    
    bool Skeleton::getFlipX()
    {
        return _flipX;
    }
    
    void Skeleton::setFlipX(float inValue)
    {
        _flipX = inValue;
    }
    
    bool Skeleton::getFlipY()
    {
        return _flipY;
    }
    
    void Skeleton::setFlipY(float inValue)
    {
        _flipY = inValue;
    }
    
    void Skeleton::sortIkConstraint(IkConstraint* constraint)
    {
        Bone* target = constraint->getTarget();
        sortBone(target);
        
        Vector<Bone*>& constrained = constraint->getBones();
        Bone* parent = constrained[0];
        sortBone(parent);
        
        if (constrained.size() > 1)
        {
            Bone* child = constrained[constrained.size() - 1];
            if (!_updateCache.contains(child))
            {
                _updateCacheReset.push_back(child);
            }
        }
        
        _updateCache.push_back(constraint);
        
        sortReset(parent->getChildren());
        constrained[constrained.size() - 1]->_sorted = true;
    }
    
    void Skeleton::sortPathConstraint(PathConstraint* constraint)
    {
        Slot* slot = constraint->getTarget();
        int slotIndex = slot->_data.getIndex();
        Bone& slotBone = slot->_bone;
        
        if (_skin != NULL)
        {
            sortPathConstraintAttachment(_skin, slotIndex, slotBone);
        }
        
        if (_data._defaultSkin != NULL && _data._defaultSkin != _skin)
        {
            sortPathConstraintAttachment(_data._defaultSkin, slotIndex, slotBone);
        }
        
        for (int ii = 0, nn = static_cast<int>(_data._skins.size()); ii < nn; ++ii)
        {
            sortPathConstraintAttachment(_data._skins[ii], slotIndex, slotBone);
        }
        
        Attachment* attachment = slot->_attachment;
        if (attachment != NULL && attachment->getRTTI().derivesFrom(PathAttachment::rtti))
        {
            sortPathConstraintAttachment(attachment, slotBone);
        }
        
        Vector<Bone*>& constrained = constraint->getBones();
        int boneCount = static_cast<int>(constrained.size());
        for (int i = 0; i < boneCount; ++i)
        {
            sortBone(constrained[i]);
        }
        
        _updateCache.push_back(constraint);
        
        for (int i = 0; i < boneCount; ++i)
        {
            sortReset(constrained[i]->getChildren());
        }
        
        for (int i = 0; i < boneCount; ++i)
        {
            constrained[i]->_sorted = true;
        }
    }
    
    void Skeleton::sortTransformConstraint(TransformConstraint* constraint)
    {
        sortBone(constraint->getTarget());
        
        Vector<Bone*>& constrained = constraint->getBones();
        int boneCount = static_cast<int>(constrained.size());
        if (constraint->_data.isLocal())
        {
            for (int i = 0; i < boneCount; ++i)
            {
                Bone* child = constrained[i];
                sortBone(child->getParent());
                if (!_updateCache.contains(child))
                {
                    _updateCacheReset.push_back(child);
                }
            }
        }
        else
        {
            for (int i = 0; i < boneCount; ++i)
            {
                sortBone(constrained[i]);
            }
        }
        
        _updateCache.push_back(constraint);
        
        for (int i = 0; i < boneCount; ++i)
        {
            sortReset(constrained[i]->getChildren());
        }
        
        for (int i = 0; i < boneCount; ++i)
        {
            constrained[i]->_sorted = true;
        }
    }
    
    void Skeleton::sortPathConstraintAttachment(Skin* skin, int slotIndex, Bone& slotBone)
    {
        HashMap<Skin::AttachmentKey, Attachment*, Skin::HashAttachmentKey>& attachments = skin->getAttachments();
        
        for (typename HashMap<Skin::AttachmentKey, Attachment*, Skin::HashAttachmentKey>::Iterator i = attachments.begin(); i != attachments.end(); ++i)
        {
            Skin::AttachmentKey key = i.first();
            if (key._slotIndex == slotIndex)
            {
                Attachment* value = i.second();
                sortPathConstraintAttachment(value, slotBone);
            }
        }
    }
    
    void Skeleton::sortPathConstraintAttachment(Attachment* attachment, Bone& slotBone)
    {
        if (attachment == NULL || attachment->getRTTI().derivesFrom(PathAttachment::rtti))
        {
            return;
        }
        
        PathAttachment* pathAttachment = static_cast<PathAttachment*>(attachment);
        Vector<int>& pathBonesRef = pathAttachment->getBones();
        Vector<int> pathBones = pathBonesRef;
        if (pathBones.size() == 0)
        {
            sortBone(&slotBone);
        }
        else
        {
            for (int i = 0, n = static_cast<int>(pathBones.size()); i < n;)
            {
                int nn = pathBones[i++];
                nn += i;
                while (i < nn)
                {
                    sortBone(_bones[pathBones[i++]]);
                }
            }
        }
    }
    
    void Skeleton::sortBone(Bone* bone)
    {
        assert(bone != NULL);
        
        if (bone->_sorted)
        {
            return;
        }
        
        Bone* parent = bone->_parent;
        if (parent != NULL)
        {
            sortBone(parent);
        }
        
        bone->_sorted = true;
        
        _updateCache.push_back(bone);
    }
    
    void Skeleton::sortReset(Vector<Bone*>& bones)
    {
        for (Bone** i = bones.begin(); i != bones.end(); ++i)
        {
            Bone* bone = (*i);
            if (bone->_sorted)
            {
                sortReset(bone->getChildren());
            }
            
            bone->_sorted = false;
        }
    }
}
