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

#ifndef Spine_Skeleton_h
#define Spine_Skeleton_h

#include <spine/Vector.h>
#include <spine/MathUtil.h>

#include <string>
#include <limits> // std::numeric_limits

namespace Spine
{
    class SkeletonData;
    class Bone;
    class Updatable;
    class Slot;
    class IkConstraint;
    class PathConstraint;
    class TransformConstraint;
    class Skin;
    class Attachment;
    
    class Skeleton
    {
        friend class SkeletonBounds;
        friend class SkeletonClipping;
        
        friend class AttachmentTimeline;
        friend class ColorTimeline;
        friend class DeformTimeline;
        friend class DrawOrderTimeline;
        friend class EventTimeline;
        friend class IkConstraintTimeline;
        friend class PathConstraintMixTimeline;
        friend class PathConstraintPositionTimeline;
        friend class PathConstraintSpacingTimeline;
        friend class ScaleTimeline;
        friend class ShearTimeline;
        friend class TransformConstraintTimeline;
        friend class TranslateTimeline;
        friend class TwoColorTimeline;
        
    public:
        Skeleton(SkeletonData& data);
        
        ~Skeleton();
        
        /// Caches information about bones and constraints. Must be called if bones, constraints or weighted path attachments are added
        /// or removed.
        void updateCache();
        
        /// Updates the world transform for each bone and applies constraints.
        void updateWorldTransform();
        
        /// Sets the bones, constraints, and slots to their setup pose values.
        void setToSetupPose();
        
        /// Sets the bones and constraints to their setup pose values.
        void setBonesToSetupPose();
        
        void setSlotsToSetupPose();
        
        /// @return May be NULL.
        Bone* findBone(std::string boneName);
        
        /// @return -1 if the bone was not found.
        int findBoneIndex(std::string boneName);
        
        /// @return May be NULL.
        Slot* findSlot(std::string slotName);
        
        /// @return -1 if the bone was not found.
        int findSlotIndex(std::string slotName);
        
        /// Sets a skin by name (see setSkin).
        void setSkin(std::string skinName);
        
        /// Attachments from the new skin are attached if the corresponding attachment from the old skin was attached.
        /// If there was no old skin, each slot's setup mode attachment is attached from the new skin.
        /// After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
        /// See Skeleton::setSlotsToSetupPose()
        /// Also, often AnimationState::apply(Skeleton&) is called before the next time the
        /// skeleton is rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin.
        ///
        /// @param newSkin May be NULL.
        void setSkin(Skin* newSkin);
        
        /// @return May be NULL.
        Attachment* getAttachment(std::string slotName, std::string attachmentName);
        
        /// @return May be NULL.
        Attachment* getAttachment(int slotIndex, std::string attachmentName);
        
        /// @param attachmentName May be empty.
        void setAttachment(std::string slotName, std::string attachmentName);
        
        /// @return May be NULL.
        IkConstraint* findIkConstraint(std::string constraintName);
        
        /// @return May be NULL.
        TransformConstraint* findTransformConstraint(std::string constraintName);
        
        /// @return May be NULL.
        PathConstraint* findPathConstraint(std::string constraintName);
        
        void update(float delta);
        
        /// Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
        /// @param outX The horizontal distance between the skeleton origin and the left side of the AABB.
        /// @param outY The vertical distance between the skeleton origin and the bottom side of the AABB.
        /// @param outWidth The width of the AABB
        /// @param outHeight The height of the AABB.
        /// @param outVertexBuffer Reference to hold a Vector of floats. This method will assign it with new floats as needed.
        void getBounds(float& outX, float& outY, float& outWidth, float& outHeight, Vector<float>& outVertexBuffer);
        
        Bone* getRootBone();
        
        const SkeletonData& getData();
        Vector<Bone*>& getBones();
        Vector<Updatable*>& getUpdateCacheList();
        Vector<Slot*>& getSlots();
        Vector<Slot*>& getDrawOrder();
        Vector<IkConstraint*>& getIkConstraints();
        Vector<PathConstraint*>& getPathConstraints();
        Vector<TransformConstraint*>& getTransformConstraints();
        
        Skin* getSkin();
        float getR();
        void setR(float inValue);
        float getG();
        void setG(float inValue);
        float getB();
        void setB(float inValue);
        float getA();
        void setA(float inValue);
        float getTime();
        void setTime(float inValue);
        float getX();
        void setX(float inValue);
        float getY();
        void setY(float inValue);
        bool getFlipX();
        void setFlipX(float inValue);
        bool getFlipY();
        void setFlipY(float inValue);
        
    private:
        SkeletonData& _data;
        Vector<Bone*> _bones;
        Vector<Slot*> _slots;
        Vector<Slot*> _drawOrder;
        Vector<IkConstraint*> _ikConstraints;
        Vector<TransformConstraint*> _transformConstraints;
        Vector<PathConstraint*> _pathConstraints;
        Vector<Updatable*> _updateCache;
        Vector<Bone*> _updateCacheReset;
        Skin* _skin;
        float _r, _g, _b, _a;
        float _time;
        bool _flipX, _flipY;
        float _x, _y;
        
        void sortIkConstraint(IkConstraint* constraint);
        
        void sortPathConstraint(PathConstraint* constraint);
        
        void sortTransformConstraint(TransformConstraint* constraint);
        
        void sortPathConstraintAttachment(Skin* skin, int slotIndex, Bone& slotBone);
        
        void sortPathConstraintAttachment(Attachment* attachment, Bone& slotBone);
        
        void sortBone(Bone* bone);
        
        static void sortReset(Vector<Bone*>& bones);
    };
}

#endif /* Spine_Skeleton_h */
