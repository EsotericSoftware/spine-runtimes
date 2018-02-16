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

#include <spine/ContainerUtil.h>

#include <assert.h>

namespace Spine {
    SkeletonData::SkeletonData() :
    _name(),
    _defaultSkin(NULL),
    _width(0),
    _height(0),
    _version(),
    _hash(),
    _fps(0),
    _imagesPath() {
        // Empty
    }
    
    SkeletonData::~SkeletonData() {
        ContainerUtil::cleanUpVectorOfPointers(_bones);
        ContainerUtil::cleanUpVectorOfPointers(_slots);
        ContainerUtil::cleanUpVectorOfPointers(_skins);
        
        _defaultSkin = NULL;
        
        ContainerUtil::cleanUpVectorOfPointers(_events);
        ContainerUtil::cleanUpVectorOfPointers(_animations);
        ContainerUtil::cleanUpVectorOfPointers(_ikConstraints);
        ContainerUtil::cleanUpVectorOfPointers(_transformConstraints);
        ContainerUtil::cleanUpVectorOfPointers(_pathConstraints);
    }
    
    BoneData* SkeletonData::findBone(const String& boneName) {
        return ContainerUtil::findWithName(_bones, boneName);
    }
    
    int SkeletonData::findBoneIndex(const String& boneName) {
        return ContainerUtil::findIndexWithName(_bones, boneName);
    }
    
    SlotData* SkeletonData::findSlot(const String& slotName) {
        return ContainerUtil::findWithName(_slots, slotName);
    }
    
    int SkeletonData::findSlotIndex(const String& slotName) {
        return ContainerUtil::findIndexWithName(_slots, slotName);
    }
    
    Skin* SkeletonData::findSkin(const String& skinName) {
        return ContainerUtil::findWithName(_skins, skinName);
    }
    
    EventData* SkeletonData::findEvent(const String& eventDataName) {
        return ContainerUtil::findWithName(_events, eventDataName);
    }
    
    Animation* SkeletonData::findAnimation(const String& animationName) {
        return ContainerUtil::findWithName(_animations, animationName);
    }
    
    IkConstraintData* SkeletonData::findIkConstraint(const String& constraintName) {
        return ContainerUtil::findWithName(_ikConstraints, constraintName);
    }
    
    TransformConstraintData* SkeletonData::findTransformConstraint(const String& constraintName) {
        return ContainerUtil::findWithName(_transformConstraints, constraintName);
    }
    
    PathConstraintData* SkeletonData::findPathConstraint(const String& constraintName) {
        return ContainerUtil::findWithName(_pathConstraints, constraintName);
    }
    
    int SkeletonData::findPathConstraintIndex(const String& pathConstraintName) {
        return ContainerUtil::findIndexWithName(_pathConstraints, pathConstraintName);
    }
    
    const String& SkeletonData::getName() {
        return _name;
    }
    
    void SkeletonData::setName(const String& inValue) {
        _name = inValue;
    }
    
    Vector<BoneData*>& SkeletonData::getBones() {
        return _bones;
    }
    
    Vector<SlotData*>& SkeletonData::getSlots() {
        return _slots;
    }
    
    Vector<Skin*>& SkeletonData::getSkins() {
        return _skins;
    }
    
    void SkeletonData::setSkins(Vector<Skin*>& inValue) {
        _skins = inValue;
    }
    
    Skin* SkeletonData::getDefaultSkin() {
        return _defaultSkin;
    }
    
    void SkeletonData::setDefaultSkin(Skin* inValue) {
        _defaultSkin = inValue;
    }
    
    Vector<EventData*>& SkeletonData::getEvents() {
        return _events;
    }
    
    void SkeletonData::setEvents(Vector<EventData*>& inValue) {
        _events = inValue;
    }
    
    Vector<Animation*>& SkeletonData::getAnimations() {
        return _animations;
    }
    
    void SkeletonData::setAnimations(Vector<Animation*>& inValue) {
        _animations = inValue;
    }
    
    Vector<IkConstraintData*>& SkeletonData::getIkConstraints() {
        return _ikConstraints;
    }
    
    void SkeletonData::setIkConstraints(Vector<IkConstraintData*>& inValue) {
        _ikConstraints = inValue;
    }
    
    Vector<TransformConstraintData*>& SkeletonData::getTransformConstraints() {
        return _transformConstraints;
    }
    
    void SkeletonData::setTransformConstraints(Vector<TransformConstraintData*>& inValue) {
        _transformConstraints = inValue;
    }
    
    Vector<PathConstraintData*>& SkeletonData::getPathConstraints() {
        return _pathConstraints;
    }
    
    void SkeletonData::setPathConstraints(Vector<PathConstraintData*>& inValue) {
        _pathConstraints = inValue;
    }
    
    float SkeletonData::getWidth() {
        return _width;
    }
    
    void SkeletonData::setWidth(float inValue) {
        _width = inValue;
    }
    
    float SkeletonData::getHeight() {
        return _height;
    }
    
    void SkeletonData::setHeight(float inValue) {
        _height = inValue;
    }
    
    const String& SkeletonData::getVersion() {
        return _version;
    }
    
    void SkeletonData::setVersion(const String& inValue) {
        _version = inValue;
    }
    
    const String& SkeletonData::getHash() {
        return _hash;
    }
    
    void SkeletonData::setHash(const String& inValue) {
        _hash = inValue;
    }
    
    const String& SkeletonData::getImagesPath() {
        return _imagesPath;
    }
    
    void SkeletonData::setImagesPath(const String& inValue) {
        _imagesPath = inValue;
    }
    
    float SkeletonData::getFps() {
        return _fps;
    }
    
    void SkeletonData::setFps(float inValue) {
        _fps = inValue;
    }
}
