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

#include <string>
#include <vector>

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
    
    class Skeleton
    {
    public:
        SkeletonData* getData();
        std::vector<Bone*> getBones();
        std::vector<Updatable*> getUpdateCacheList();
        std::vector<Slot*> getSlots();
        std::vector<Slot*> getDrawOrder();
        std::vector<IkConstraint*> getIkConstraints();
        std::vector<PathConstraint*> getPathConstraints();
        std::vector<TransformConstraint*> getTransformConstraints();
        
        Skin* getSkin { get { return skin; } set { skin = value; } }
        float getR { get { return r; } set { r = value; } }
        float getG { get { return g; } set { g = value; } }
        float getB { get { return b; } set { b = value; } }
        float getA { get { return a; } set { a = value; } }
        float getTime { get { return time; } set { time = value; } }
        float getX { get { return x; } set { x = value; } }
        float getY { get { return y; } set { y = value; } }
        bool getFlipX { get { return flipX; } set { flipX = value; } }
        bool getFlipY { get { return flipY; } set { flipY = value; } }
        
        Bone* getRootBone()
        {
            return _bones.size() == 0 ? nullptr : _bones.at(0);
            //get { return bones.Count == 0 ? null : bones.Items[0]; }
        }
        
        Skeleton(const SkeletonData& data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "data cannot be null.");
            }
            
            this.data = data;
            
            bones = new std::vector<Bone>(data.bones.Count);
            foreach (BoneData boneData in data.bones)
            {
                Bone bone;
                if (boneData.parent == null)
                {
                    bone = new Bone(boneData, this, null);
                }
                else
                {
                    Bone parent = bones.Items[boneData.parent.index];
                    bone = new Bone(boneData, this, parent);
                    parent.children.Add(bone);
                }
                bones.Add(bone);
            }
            
            slots = new std::vector<Slot>(data.slots.Count);
            drawOrder = new std::vector<Slot>(data.slots.Count);
            foreach (SlotData slotData in data.slots)
            {
                Bone bone = bones.Items[slotData.boneData.index];
                Slot slot = new Slot(slotData, bone);
                slots.Add(slot);
                drawOrder.Add(slot);
            }
            
            ikConstraints = new std::vector<IkConstraint>(data.ikConstraints.Count);
            foreach (IkConstraintData ikConstraintData in data.ikConstraints)
            ikConstraints.Add(new IkConstraint(ikConstraintData, this));
            
            transformConstraints = new std::vector<TransformConstraint>(data.transformConstraints.Count);
            foreach (TransformConstraintData transformConstraintData in data.transformConstraints)
            transformConstraints.Add(new TransformConstraint(transformConstraintData, this));
            
            pathConstraints = new std::vector<PathConstraint> (data.pathConstraints.Count);
            foreach (PathConstraintData pathConstraintData in data.pathConstraints)
            pathConstraints.Add(new PathConstraint(pathConstraintData, this));
            
            updateCache();
            updateWorldTransform();
        }
        
        ~Skeleton()
        {
            // TODO
        }
        
        /// Caches information about bones and constraints. Must be called if bones, constraints or weighted path attachments are added
        /// or removed.
        void updateCache()
        {
            std::vector<IUpdatable> updateCache = this.updateCache;
            updateCache.Clear();
            this.updateCacheReset.Clear();
            
            std::vector<Bone> bones = this.bones;
            for (int i = 0, n = bones.Count; i < n; i++)
            {
                bones.Items[i].sorted = false;
            }
            
            std::vector<IkConstraint> ikConstraints = this.ikConstraints;
            var transformConstraints = this.transformConstraints;
            var pathConstraints = this.pathConstraints;
            int ikCount = IkConstraints.Count, transformCount = transformConstraints.Count, pathCount = pathConstraints.Count;
            int constraintCount = ikCount + transformCount + pathCount;
            //outer:
            for (int i = 0; i < constraintCount; i++)
            {
                for (int ii = 0; ii < ikCount; ii++)
                {
                    IkConstraint constraint = ikConstraints.Items[ii];
                    if (constraint.data.order == i)
                    {
                        sortIkConstraint(constraint);
                        goto continue_outer; //continue outer;
                    }
                }
                for (int ii = 0; ii < transformCount; ii++)
                {
                    TransformConstraint constraint = transformConstraints.Items[ii];
                    if (constraint.data.order == i)
                    {
                        sortTransformConstraint(constraint);
                        goto continue_outer; //continue outer;
                    }
                }
                for (int ii = 0; ii < pathCount; ii++)
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
            
            for (int i = 0, n = bones.Count; i < n; i++)
            {
                sortBone(bones.Items[i]);
            }
        }
        
        /// updates the world transform for each bone and applies constraints.
        void updateWorldTransform()
        {
            var updateCacheReset = this.updateCacheReset;
            var updateCacheResetItems = updateCacheReset.Items;
            for (int i = 0, n = updateCacheReset.Count; i < n; i++)
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
            
            var updateItems = this.updateCache.Items;
            for (int i = 0, n = updateCache.Count; i < n; i++)
            {
                updateItems[i].update();
            }
        }
        
        /// Sets the bones, constraints, and slots to their setup pose values.
        void setToSetupPose()
        {
            setBonesToSetupPose();
            setSlotsToSetupPose();
        }
        
        /// Sets the bones and constraints to their setup pose values.
        void setBonesToSetupPose()
        {
            var bonesItems = this.bones.Items;
            for (int i = 0, n = bones.Count; i < n; i++)
            {
                bonesItems[i].setToSetupPose();
            }
            
            var ikConstraintsItems = this.ikConstraints.Items;
            for (int i = 0, n = ikConstraints.Count; i < n; i++)
            {
                IkConstraint constraint = ikConstraintsItems[i];
                constraint.bendDirection = constraint.data.bendDirection;
                constraint.mix = constraint.data.mix;
            }
            
            var transformConstraintsItems = this.transformConstraints.Items;
            for (int i = 0, n = transformConstraints.Count; i < n; i++)
            {
                TransformConstraint constraint = transformConstraintsItems[i];
                TransformConstraintData constraintData = constraint.data;
                constraint.rotateMix = constraintData.rotateMix;
                constraint.translateMix = constraintData.translateMix;
                constraint.scaleMix = constraintData.scaleMix;
                constraint.shearMix = constraintData.shearMix;
            }
            
            var pathConstraintItems = this.pathConstraints.Items;
            for (int i = 0, n = pathConstraints.Count; i < n; i++)
            {
                PathConstraint constraint = pathConstraintItems[i];
                PathConstraintData constraintData = constraint.data;
                constraint.position = constraintData.position;
                constraint.spacing = constraintData.spacing;
                constraint.rotateMix = constraintData.rotateMix;
                constraint.translateMix = constraintData.translateMix;
            }
        }
        
        void setSlotsToSetupPose()
        {
            var slots = this.slots;
            var slotsItems = slots.Items;
            drawOrder.Clear();
            
            for (int i = 0, n = slots.Count; i < n; i++)
            {
                drawOrder.Add(slotsItems[i]);
            }
            
            for (int i = 0, n = slots.Count; i < n; i++)
            {
                slotsItems[i].setToSetupPose();
            }
        }
        
        /// May be null.
        Bone findBone(std::string boneName)
        {
            if (boneName == null)
            {
                throw new ArgumentNullException("boneName", "boneName cannot be null.");
            }
            
            var bones = this.bones;
            var bonesItems = bones.Items;
            for (int i = 0, n = bones.Count; i < n; i++)
            {
                Bone bone = bonesItems[i];
                if (bone.data.name == boneName)
                {
                    return bone;
                }
            }
            
            return null;
        }
        
        /// -1 if the bone was not found.
        int findBoneIndex(std::string boneName)
        {
            if (boneName == null) throw new ArgumentNullException("boneName", "boneName cannot be null.");
            var bones = this.bones;
            var bonesItems = bones.Items;
            for (int i = 0, n = bones.Count; i < n; i++)
            {
                if (bonesItems[i].data.name == boneName)
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        /// May be null.
        Slot findSlot(std::string slotName)
        {
            if (slotName == null)
            {
                throw new ArgumentNullException("slotName", "slotName cannot be null.");
            }
            
            var slots = this.slots;
            var slotsItems = slots.Items;
            for (int i = 0, n = slots.Count; i < n; i++)
            {
                Slot slot = slotsItems[i];
                if (slot.data.name == slotName)
                {
                    return slot;
                }
            }
            
            return null;
        }
        
        /// -1 if the bone was not found.
        int findSlotIndex(std::string slotName)
        {
            if (slotName == null)
            {
                throw new ArgumentNullException("slotName", "slotName cannot be null.");
            }
            
            var slots = this.slots;
            var slotsItems = slots.Items;
            for (int i = 0, n = slots.Count; i < n; i++)
            {
                if (slotsItems[i].data.name.Equals(slotName))
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        /// Sets a skin by name (see setSkin).
        void setSkin(std::string skinName)
        {
            Skin foundSkin = data.FindSkin(skinName);
            if (foundSkin == null)
            {
                throw new ArgumentException("Skin not found: " + skinName, "skinName");
            }
            
            setSkin(foundSkin);
        }
        
        ///
        /// <para>Attachments from the new skin are attached if the corresponding attachment from the old skin was attached.
        /// If there was no old skin, each slot's setup mode attachment is attached from the new skin.</para>
        /// <para>After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
        /// <see cref="Skeleton.setSlotsToSetupPose()"/>.
        /// Also, often <see cref="AnimationState.Apply(Skeleton)"/> is called before the next time the
        /// skeleton is rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin.</para>
        ///
        /// @param newSkin May be null.
        void setSkin(Skin newSkin)
        {
            if (newSkin != null)
            {
                if (skin != null)
                {
                    newSkin.AttachAll(this, skin);
                }
                else
                {
                    std::vector<Slot> slots = this.slots;
                    for (int i = 0, n = slots.Count; i < n; i++)
                    {
                        Slot slot = slots.Items[i];
                        std::string name = slot.data.attachmentName;
                        if (name != null)
                        {
                            Attachment attachment = newSkin.getAttachment(i, name);
                            if (attachment != null)
                            {
                                slot.Attachment = attachment;
                            }
                        }
                    }
                }
            }
            
            skin = newSkin;
        }
        
        /// May be null.
        Attachment getAttachment(std::string slotName, std::string attachmentName)
        {
            return getAttachment(data.findSlotIndex(slotName), attachmentName);
        }
        
        /// May be null.
        Attachment getAttachment(int slotIndex, std::string attachmentName)
        {
            if (attachmentName == null)
            {
                throw new ArgumentNullException("attachmentName", "attachmentName cannot be null.");
            }
            
            if (skin != null)
            {
                Attachment attachment = skin.getAttachment(slotIndex, attachmentName);
                if (attachment != null)
                {
                    return attachment;
                }
            }
            
            return data.defaultSkin != null ? data.defaultSkin.getAttachment(slotIndex, attachmentName) : null;
        }
        
        /// @param attachmentName May be null.
        void setAttachment(std::string slotName, std::string attachmentName)
        {
            if (slotName == null)
            {
                throw new ArgumentNullException("slotName", "slotName cannot be null.");
            }
            
            std::vector<Slot> slots = this.slots;
            for (int i = 0, n = slots.Count; i < n; i++)
            {
                Slot slot = slots.Items[i];
                if (slot.data.name == slotName)
                {
                    Attachment attachment = null;
                    if (attachmentName != null)
                    {
                        attachment = getAttachment(i, attachmentName);
                        if (attachment == null)
                        {
                            throw new Exception("Attachment not found: " + attachmentName + ", for slot: " + slotName);
                        }
                    }
                    
                    slot.Attachment = attachment;
                    
                    return;
                }
            }
            
            throw new Exception("Slot not found: " + slotName);
        }
        
        /// May be null.
        IkConstraint findIkConstraint(std::string constraintName)
        {
            if (constraintName == null)
            {
                throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
            }
            
            std::vector<IkConstraint> ikConstraints = this.ikConstraints;
            for (int i = 0, n = ikConstraints.Count; i < n; i++)
            {
                IkConstraint ikConstraint = ikConstraints.Items[i];
                if (ikConstraint.data.name == constraintName) return ikConstraint;
            }
            
            return null;
        }
        
        /// May be null.
        TransformConstraint findTransformConstraint(std::string constraintName)
        {
            if (constraintName == null)
            {
                throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
            }
            
            std::vector<TransformConstraint> transformConstraints = this.transformConstraints;
            for (int i = 0, n = transformConstraints.Count; i < n; i++)
            {
                TransformConstraint transformConstraint = transformConstraints.Items[i];
                if (transformConstraint.data.name == constraintName) return transformConstraint;
            }
            
            return null;
        }
        
        /// May be null.
        PathConstraint findPathConstraint(std::string constraintName)
        {
            if (constraintName == null)
            {
                throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
            }
            
            std::vector<PathConstraint> pathConstraints = this.pathConstraints;
            for (int i = 0, n = pathConstraints.Count; i < n; i++)
            {
                PathConstraint constraint = pathConstraints.Items[i];
                if (constraint.data.name.Equals(constraintName)) return constraint;
            }
            
            return null;
        }
        
        void update(float delta)
        {
            time += delta;
        }
        
        /// Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
        /// @param x The horizontal distance between the skeleton origin and the left side of the AABB.
        /// @param y The vertical distance between the skeleton origin and the bottom side of the AABB.
        /// @param width The width of the AABB
        /// @param height The height of the AABB.
        /// @param vertexBuffer Reference to hold a float[]. May be a null reference. This method will assign it a new float[] with the appropriate size as needed.
        void getBounds(out float x, out float y, out float width, out float height, ref float[] vertexBuffer)
        {
            float[] temp = vertexBuffer;
            temp = temp ?? new float[8];
            var drawOrderItems = this.drawOrder.Items;
            float minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            for (int i = 0, n = this.drawOrder.Count; i < n; i++)
            {
                Slot slot = drawOrderItems[i];
                int verticesLength = 0;
                float[] vertices = null;
                Attachment attachment = slot.attachment;
                var regionAttachment = attachment as RegionAttachment;
                if (regionAttachment != null)
                {
                    verticesLength = 8;
                    vertices = temp;
                    if (vertices.Length < 8) vertices = temp = new float[8];
                    regionAttachment.ComputeWorldVertices(slot.bone, temp, 0);
                }
                else
                {
                    var meshAttachment = attachment as MeshAttachment;
                    if (meshAttachment != null)
                    {
                        MeshAttachment mesh = meshAttachment;
                        verticesLength = mesh.WorldVerticesLength;
                        vertices = temp;
                        if (vertices.Length < verticesLength)
                        {
                            vertices = temp = new float[verticesLength];
                        }
                        
                        mesh.ComputeWorldVertices(slot, 0, verticesLength, temp, 0);
                    }
                }
                
                if (vertices != null)
                {
                    for (int ii = 0; ii < verticesLength; ii += 2)
                    {
                        float vx = vertices[ii], vy = vertices[ii + 1];
                        minX = Math.Min(minX, vx);
                        minY = Math.Min(minY, vy);
                        maxX = Math.Max(maxX, vx);
                        maxY = Math.Max(maxY, vy);
                    }
                }
            }
            
            x = minX;
            y = minY;
            width = maxX - minX;
            height = maxY - minY;
            vertexBuffer = temp;
        }
        
    private:
        SkeletonData* _data;
        std::vector<Bone*> _bones;
        std::vector<Slot*> _slots;
        std::vector<Slot*> _drawOrder;
        std::vector<IkConstraint*> _ikConstraints;
        std::vector<TransformConstraint*> _transformConstraints;
        std::vector<PathConstraint*> _pathConstraints;
        std::vector<IUpdatable*> _updateCache = new std::vector<IUpdatable>();
        std::vector<Bone*> _updateCacheReset = new std::vector<Bone>();
        Skin _skin;
        float _r = 1, _g = 1, _b = 1, _a = 1;
        float _time;
        bool _flipX, _flipY;
        float _x, _y;
        
        void sortIkConstraint(IkConstraint constraint)
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
        
        void sortPathConstraint(PathConstraint constraint)
        {
            Slot slot = constraint.target;
            int slotIndex = slot.data.index;
            Bone slotBone = slot.bone;
            if (skin != null)
            {
                sortPathConstraintAttachment(skin, slotIndex, slotBone);
            }
            
            if (data.defaultSkin != null && data.defaultSkin != skin)
            {
                sortPathConstraintAttachment(data.defaultSkin, slotIndex, slotBone);
            }
            
            for (int ii = 0, nn = data.skins.Count; ii < nn; ii++)
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
            for (int i = 0; i < boneCount; i++)
            {
                sortBone(constrained.Items[i]);
            }
            
            updateCache.Add(constraint);
            
            for (int i = 0; i < boneCount; i++)
            {
                sortReset(constrained.Items[i].children);
            }
            
            for (int i = 0; i < boneCount; i++)
            {
                constrained.Items[i].sorted = true;
            }
        }
        
        void sortTransformConstraint(TransformConstraint constraint)
        {
            sortBone(constraint.target);
            
            var constrained = constraint.bones;
            int boneCount = constrained.Count;
            if (constraint.data.local)
            {
                for (int i = 0; i < boneCount; i++)
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
                for (int i = 0; i < boneCount; i++)
                {
                    sortBone(constrained.Items[i]);
                }
            }
            
            updateCache.Add(constraint);
            
            for (int i = 0; i < boneCount; i++)
            {
                sortReset(constrained.Items[i].children);
            }
            for (int i = 0; i < boneCount; i++)
            {
                constrained.Items[i].sorted = true;
            }
        }
        
        void sortPathConstraintAttachment(Skin skin, int slotIndex, Bone slotBone)
        {
            foreach (var entry in skin.Attachments)
            {
                if (entry.Key.slotIndex == slotIndex)
                {
                    sortPathConstraintAttachment(entry.Value, slotBone);
                }
            }
        }
        
        void sortPathConstraintAttachment(Attachment attachment, Bone slotBone)
        {
            if (!(attachment is PathAttachment)) return;
            int[] pathBones = ((PathAttachment)attachment).bones;
            if (pathBones == null)
            {
                sortBone(slotBone);
            }
            else
            {
                var bones = this.bones;
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
        
        void sortBone(Bone bone)
        {
            if (bone.sorted)
            {
                return;
            }
            
            Bone parent = bone.parent;
            if (parent != null) sortBone(parent);
            bone.sorted = true;
            updateCache.Add(bone);
        }
        
        static void sortReset(std::vector<Bone*>& bones)
        {
            var bonesItems = bones.Items;
            for (int i = 0, n = bones.Count; i < n; i++)
            {
                Bone bone = bonesItems[i];
                if (bone.sorted)
                {
                    sortReset(bone.children);
                }
                
                bone.sorted = false;
            }
        }
    };
}

#endif /* Spine_Skeleton_h */
