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

#include <spine/ContainerUtil.h>

namespace Spine
{
    Skeleton::Skeleton(SkeletonData& data) :
    _data(data),
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
                bone = new Bone(*data, *this, NULL);
            }
            else
            {
                Bone* parent = _bones[data->getParent()->getIndex()];
                bone = new Bone(*data, *this, parent);
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
            Slot* slot = new Slot(*data, *bone);
            
            _slots.push_back(slot);
            _drawOrder.push_back(slot);
        }
        
        _ikConstraints.reserve(_data.getIkConstraints().size());
        for (IkConstraintData** i = _data.getIkConstraints().begin(); i != _data.getIkConstraints().end(); ++i)
        {
            IkConstraintData* data = (*i);
            
            _ikConstraints.push_back(new IkConstraint(*data, *this));
        }
        
        _transformConstraints.reserve(_data.getTransformConstraints().size());
        for (TransformConstraintData** i = _data.getTransformConstraints().begin(); i != _data.getTransformConstraints().end(); ++i)
        {
            TransformConstraintData* data = (*i);
            
            _transformConstraints.push_back(new TransformConstraint(*data, *this));
        }
        
        _pathConstraints.reserve(_data.getPathConstraints().size());
        for (PathConstraintData** i = _data.getPathConstraints().begin(); i != _data.getPathConstraints().end(); ++i)
        {
            PathConstraintData* data = (*i);
            
            _pathConstraints.push_back(new PathConstraint(*data, *this));
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
        Vector<Updatable> updateCache = _updateCache;
        updateCache.Clear();
        _updateCacheReset.Clear();
        
        Vector<Bone> bones = _bones;
        for (int i = 0, n = bones.Count; i < n; ++i)
        {
            bones.Items[i].sorted = false;
        }
        
        Vector<IkConstraint> ikConstraints = _ikConstraints;
        var transformConstraints = _transformConstraints;
        var pathConstraints = _pathConstraints;
        int ikCount = IkConstraints.Count, transformCount = transformConstraints.Count, pathCount = pathConstraints.Count;
        int constraintCount = ikCount + transformCount + pathCount;
        //outer:
        for (int i = 0; i < constraintCount; ++i)
        {
            for (int ii = 0; ii < ikCount; ++ii)
            {
                IkConstraint constraint = ikConstraints.Items[ii];
                if (constraint.data.order == i)
                {
                    sortIkConstraint(constraint);
                    goto continue_outer; //continue outer;
                }
            }
            for (int ii = 0; ii < transformCount; ++ii)
            {
                TransformConstraint constraint = transformConstraints.Items[ii];
                if (constraint.data.order == i)
                {
                    sortTransformConstraint(constraint);
                    goto continue_outer; //continue outer;
                }
            }
            for (int ii = 0; ii < pathCount; ++ii)
            {
                PathConstraint constraint = pathConstraints.Items[ii];
                if (constraint.data.order == i)
                {
                    sortPathConstraint(constraint);
                    goto continue_outer; //continue outer;
                }
            }
        continue_outer: {}
        }
        
        for (int i = 0, n = bones.Count; i < n; ++i)
        {
            sortBone(bones.Items[i]);
        }
    }
    
    void Skeleton::updateWorldTransform()
    {
        var updateCacheReset = _updateCacheReset;
        var updateCacheResetItems = updateCacheReset.Items;
        for (int i = 0, n = updateCacheReset.Count; i < n; ++i)
        {
            Bone bone = updateCacheResetItems[i];
            bone.ax = bone.x;
            bone.ay = bone.y;
            bone.arotation = bone.rotation;
            bone.ascaleX = bone.scaleX;
            bone.ascaleY = bone.scaleY;
            bone.ashearX = bone.shearX;
            bone.ashearY = bone.shearY;
            bone.appliedValid = true;
        }
        
        var updateItems = _updateCache.Items;
        for (int i = 0, n = updateCache.Count; i < n; ++i)
        {
            updateItems[i].update();
        }
    }
    
    void Skeleton::setToSetupPose()
    {
        setBonesToSetupPose();
        setSlotsToSetupPose();
    }
    
    void Skeleton::setBonesToSetupPose()
    {
        var bonesItems = _bones.Items;
        for (int i = 0, n = bones.Count; i < n; ++i)
        {
            bonesItems[i].setToSetupPose();
        }
        
        var ikConstraintsItems = _ikConstraints.Items;
        for (int i = 0, n = ikConstraints.Count; i < n; ++i)
        {
            IkConstraint constraint = ikConstraintsItems[i];
            constraint.bendDirection = constraint.data.bendDirection;
            constraint.mix = constraint.data.mix;
        }
        
        var transformConstraintsItems = _transformConstraints.Items;
        for (int i = 0, n = transformConstraints.Count; i < n; ++i)
        {
            TransformConstraint constraint = transformConstraintsItems[i];
            TransformConstraintData constraintData = constraint.data;
            constraint.rotateMix = constraintData.rotateMix;
            constraint.translateMix = constraintData.translateMix;
            constraint.scaleMix = constraintData.scaleMix;
            constraint.shearMix = constraintData.shearMix;
        }
        
        var pathConstraintItems = _pathConstraints.Items;
        for (int i = 0, n = pathConstraints.Count; i < n; ++i)
        {
            PathConstraint constraint = pathConstraintItems[i];
            PathConstraintData constraintData = constraint.data;
            constraint.position = constraintData.position;
            constraint.spacing = constraintData.spacing;
            constraint.rotateMix = constraintData.rotateMix;
            constraint.translateMix = constraintData.translateMix;
        }
    }
    
    void Skeleton::setSlotsToSetupPose()
    {
        var slots = _slots;
        var slotsItems = slots.Items;
        drawOrder.Clear();
        for (int i = 0, n = slots.Count; i < n; ++i)
        {
            drawOrder.Add(slotsItems[i]);
        }
        
        for (int i = 0, n = slots.Count; i < n; ++i)
        {
            slotsItems[i].setToSetupPose();
        }
    }
    
    Bone* Skeleton::findBone(std::string boneName)
    {
        assert(boneName.length() > 0);
        
        var bones = _bones;
        var bonesItems = bones.Items;
        for (int i = 0, n = bones.Count; i < n; ++i)
        {
            Bone bone = bonesItems[i];
            if (bone.data.name == boneName)
            {
                return bone;
            }
        }
        
        return NULL;
    }
    
    int Skeleton::findBoneIndex(std::string boneName)
    {
        assert(boneName.length() > 0);
        
        var bones = _bones;
        var bonesItems = bones.Items;
        for (int i = 0, n = bones.Count; i < n; ++i)
        {
            if (bonesItems[i].data.name == boneName)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    Slot* Skeleton::findSlot(std::string slotName)
    {
        assert(slotName.length() > 0);
        
        var slots = _slots;
        var slotsItems = slots.Items;
        for (int i = 0, n = slots.Count; i < n; ++i)
        {
            Slot slot = slotsItems[i];
            if (slot.data.name == slotName)
            {
                return slot;
            }
        }
        
        return NULL;
    }
    
    int Skeleton::findSlotIndex(std::string slotName)
    {
        assert(slotName.length() > 0);
        
        var slots = _slots;
        var slotsItems = slots.Items;
        for (int i = 0, n = slots.Count; i < n; ++i)
        {
            if (slotsItems[i].data.name.Equals(slotName))
            {
                return i;
            }
        }
        
        return -1;
    }
    
    void Skeleton::setSkin(std::string skinName)
    {
        Skin foundSkin = data.FindSkin(skinName);
        
        assert(foundSkin != NULL);
        
        setSkin(foundSkin);
    }
    
    void Skeleton::setSkin(Skin newSkin)
    {
        if (newSkin != NULL)
        {
            if (skin != NULL)
            {
                newSkin.AttachAll(this, skin);
            }
            else
            {
                Vector<Slot> slots = _slots;
                for (int i = 0, n = slots.Count; i < n; ++i)
                {
                    Slot slot = slots.Items[i];
                    std::string name = slot.data.attachmentName;
                    if (name != NULL)
                    {
                        Attachment attachment = newSkin.getAttachment(i, name);
                        if (attachment != NULL)
                        {
                            slot.Attachment = attachment;
                        }
                    }
                }
            }
        }
        skin = newSkin;
    }
    
    Attachment* Skeleton::getAttachment(std::string slotName, std::string attachmentName)
    {
        return getAttachment(data.findSlotIndex(slotName), attachmentName);
    }
    
    Attachment* Skeleton::getAttachment(int slotIndex, std::string attachmentName)
    {
        assert(attachmentName.length() > 0);
        
        if (skin != NULL)
        {
            Attachment attachment = skin.getAttachment(slotIndex, attachmentName);
            if (attachment != NULL)
            {
                return attachment;
            }
        }
        
        return data.defaultSkin != NULL ? data.defaultSkin.getAttachment(slotIndex, attachmentName) : NULL;
    }
    
    void Skeleton::setAttachment(std::string slotName, std::string attachmentName)
    {
        assert(slotName.length() > 0);
        
        Vector<Slot> slots = _slots;
        for (int i = 0, n = slots.Count; i < n; ++i)
        {
            Slot slot = slots.Items[i];
            if (slot.data.name == slotName)
            {
                Attachment attachment = NULL;
                if (attachmentName != NULL)
                {
                    attachment = getAttachment(i, attachmentName);
                    
                    assert(attachment != NULL);
                }
                
                slot.Attachment = attachment;
                
                return;
            }
        }
        
        printf("Slot not found: %s" + slotName.c_str());
        
        assert(false);
    }
    
    IkConstraint* Skeleton::findIkConstraint(std::string constraintName)
    {
        assert(constraintName.length() > 0);
        
        Vector<IkConstraint> ikConstraints = _ikConstraints;
        for (int i = 0, n = ikConstraints.Count; i < n; ++i)
        {
            IkConstraint ikConstraint = ikConstraints.Items[i];
            if (ikConstraint.data.name == constraintName)
            {
                return ikConstraint;
            }
        }
        return NULL;
    }
    
    TransformConstraint* Skeleton::findTransformConstraint(std::string constraintName)
    {
        assert(constraintName.length() > 0);
        
        Vector<TransformConstraint> transformConstraints = _transformConstraints;
        for (int i = 0, n = transformConstraints.Count; i < n; ++i)
        {
            TransformConstraint transformConstraint = transformConstraints.Items[i];
            if (transformConstraint.data.name == constraintName)
            {
                return transformConstraint;
            }
        }
        
        return NULL;
    }
    
    PathConstraint* Skeleton::findPathConstraint(std::string constraintName)
    {
        assert(constraintName.length() > 0);
        
        Vector<PathConstraint> pathConstraints = _pathConstraints;
        for (int i = 0, n = pathConstraints.Count; i < n; ++i)
        {
            PathConstraint constraint = pathConstraints.Items[i];
            if (constraint.data.name.Equals(constraintName))
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
        
        for (Slot* i = _drawOrder.begin(); i != _drawOrder.end(); ++i)
        {
            Slot* slot = i;
            int verticesLength = 0;
            Attachment* attachment = slot->getAttachment();
            
            if (attachment != NULL && attachment->getRTTI().derivesFrom(RegionAttachment::rtti))
            {
                RegionAttachment* regionAttachment = static_cast<RegionAttachment*>(attachment);
                
                verticesLength = 8;
                if (vertexBuffer.size() < 8)
                {
                    vertexBuffer.reserve(8);
                }
                regionAttachment->computeWorldVertices(slot->getBone(), vertexBuffer, 0);
            }
            else if (attachment != NULL && attachment->getRTTI().derivesFrom(MeshAttachment::rtti))
            {
                MeshAttachment* mesh = static_cast<MeshAttachment*>(attachment);
                
                verticesLength = mesh->getWorldVerticesLength();
                if (vertexBuffer.size() < verticesLength)
                {
                    vertexBuffer.reserve(verticesLength);
                }
                
                mesh->computeWorldVertices(slot, 0, verticesLength, vertexBuffer, 0);
            }
            
            for (int ii = 0; ii < verticesLength; ii += 2)
            {
                float vx = vertexBuffer[ii];
                float vy = vertexBuffer[ii + 1];
                
                minX = MIN(minX, vx);
                minY = MIN(minY, vy);
                maxX = MAX(maxX, vx);
                maxY = MAX(maxY, vy);
            }
        }
        
        x = minX;
        y = minY;
        width = maxX - minX;
        height = maxY - minY;
    }
    
    Bone* Skeleton::getRootBone()
    {
        return _bones.size() == 0 ? NULL : &_bones[0];
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
    
    void Skeleton::setSkin(Skin* inValue)
    {
        _skin = inValue;
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
        Bone target = constraint.target;
        sortBone(target);
        
        var constrained = constraint.bones;
        Bone parent = constrained.Items[0];
        sortBone(parent);
        
        if (constrained.Count > 1)
        {
            Bone child = constrained.Items[constrained.Count - 1];
            if (!updateCache.Contains(child))
            {
                updateCacheReset.Add(child);
            }
        }
        
        updateCache.Add(constraint);
        
        sortReset(parent.children);
        constrained.Items[constrained.Count - 1].sorted = true;
    }
    
    void Skeleton::sortPathConstraint(PathConstraint* constraint)
    {
        Slot slot = constraint.target;
        int slotIndex = slot.data.index;
        Bone slotBone = slot.bone;
        
        if (skin != NULL)
        {
            sortPathConstraintAttachment(skin, slotIndex, slotBone);
        }
        
        if (data.defaultSkin != NULL && data.defaultSkin != skin)
        {
            sortPathConstraintAttachment(data.defaultSkin, slotIndex, slotBone);
        }
        
        for (int ii = 0, nn = data.skins.Count; ii < nn; ++ii)
        {
            sortPathConstraintAttachment(data.skins.Items[ii], slotIndex, slotBone);
        }
        
        Attachment attachment = slot.attachment;
        if (attachment is PathAttachment)
        {
            sortPathConstraintAttachment(attachment, slotBone);
        }
        
        var constrained = constraint.bones;
        int boneCount = constrained.Count;
        for (int i = 0; i < boneCount; ++i)
        {
            sortBone(constrained.Items[i]);
        }
        
        updateCache.Add(constraint);
        
        for (int i = 0; i < boneCount; ++i)
        {
            sortReset(constrained.Items[i].children);
        }
        
        for (int i = 0; i < boneCount; ++i)
        {
            constrained.Items[i].sorted = true;
        }
    }
    
    void Skeleton::sortTransformConstraint(TransformConstraint* constraint)
    {
        sortBone(constraint.target);
        
        var constrained = constraint.bones;
        int boneCount = constrained.Count;
        if (constraint.data.local)
        {
            for (int i = 0; i < boneCount; ++i)
            {
                Bone child = constrained.Items[i];
                sortBone(child.parent);
                if (!updateCache.Contains(child))
                {
                    updateCacheReset.Add(child);
                }
            }
        }
        else
        {
            for (int i = 0; i < boneCount; ++i)
            {
                sortBone(constrained.Items[i]);
            }
        }
        
        updateCache.Add(constraint);
        
        for (int i = 0; i < boneCount; ++i)
        {
            sortReset(constrained.Items[i].children);
        }
        for (int i = 0; i < boneCount; ++i)
        {
            constrained.Items[i].sorted = true;
        }
    }
    
    void Skeleton::sortPathConstraintAttachment(Skin* skin, int slotIndex, Bone* slotBone)
    {
        foreach (var entry in skin.Attachments)
        {
            if (entry.Key.slotIndex == slotIndex)
            {
                sortPathConstraintAttachment(entry.Value, slotBone);
            }
        }
    }
    
    void Skeleton::sortPathConstraintAttachment(Attachment* attachment, Bone* slotBone)
    {
        if (!(attachment is PathAttachment))
        {
            return;
        }
        
        int[] pathBones = ((PathAttachment)attachment).bones;
        if (pathBones == NULL)
        {
            sortBone(slotBone);
        }
        else
        {
            var bones = _bones;
            for (int i = 0, n = pathBones.Length; i < n;)
            {
                int nn = pathBones[i++];
                nn += i;
                while (i < nn)
                {
                    sortBone(bones.Items[pathBones[i++]]);
                }
            }
        }
    }
    
    void Skeleton::sortBone(Bone* bone)
    {
        if (bone.sorted)
        {
            return;
        }
        
        Bone parent = bone.parent;
        if (parent != NULL)
        {
            sortBone(parent);
        }
        
        bone.sorted = true;
        updateCache.Add(bone);
    }
    
    void Skeleton::sortReset(Vector<Bone*>& bones)
    {
        for (Bone* i = bones.begin(); i != bones.end(); ++i)
        {
            Bone* bone = i;
            if (bone->isSorted())
            {
                sortReset(bone->getChildren());
            }
            
            bone->setSorted(false);
        }
    }
}
