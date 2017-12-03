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

#include <spine/SkeletonBinary.h>

#include <spine/SkeletonData.h>
#include <spine/Atlas.h>
#include <spine/AtlasAttachmentLoader.h>
#include <spine/LinkedMesh.h>
#include <spine/Skin.h>
#include <spine/Attachment.h>
#include <spine/VertexAttachment.h>
#include <spine/Animation.h>

#include <spine/Extension.h>
#include <spine/ContainerUtil.h>
#include <spine/BoneData.h>
#include <spine/SlotData.h>
#include <spine/IkConstraintData.h>
#include <spine/TransformConstraintData.h>
#include <spine/PathConstraintData.h>
#include <spine/PositionMode.h>
#include <spine/SpacingMode.h>
#include <spine/RotateMode.h>
#include <spine/AttachmentType.h>
#include <spine/RegionAttachment.h>
#include <spine/BoundingBoxAttachment.h>
#include <spine/MeshAttachment.h>
#include <spine/PathAttachment.h>
#include <spine/PointAttachment.h>
#include <spine/ClippingAttachment.h>
#include <spine/EventData.h>
#include <spine/AttachmentTimeline.h>
#include <spine/MathUtil.h>
#include <spine/ColorTimeline.h>
#include <spine/TwoColorTimeline.h>
#include <spine/RotateTimeline.h>
#include <spine/TranslateTimeline.h>
#include <spine/ScaleTimeline.h>
#include <spine/ShearTimeline.h>
#include <spine/IkConstraintTimeline.h>
#include <spine/TransformConstraintTimeline.h>
#include <spine/PathConstraintPositionTimeline.h>
#include <spine/PathConstraintSpacingTimeline.h>
#include <spine/PathConstraintPositionTimeline.h>
#include <spine/PathConstraintMixTimeline.h>
#include <spine/DeformTimeline.h>
#include <spine/DrawOrderTimeline.h>
#include <spine/EventTimeline.h>
#include <spine/Event.h>

namespace Spine
{
    const int SkeletonBinary::BONE_ROTATE = 0;
    const int SkeletonBinary::BONE_TRANSLATE = 1;
    const int SkeletonBinary::BONE_SCALE = 2;
    const int SkeletonBinary::BONE_SHEAR = 3;
    
    const int SkeletonBinary::SLOT_ATTACHMENT = 0;
    const int SkeletonBinary::SLOT_COLOR = 1;
    const int SkeletonBinary::SLOT_TWO_COLOR = 2;
    
    const int SkeletonBinary::PATH_POSITION = 0;
    const int SkeletonBinary::PATH_SPACING = 1;
    const int SkeletonBinary::PATH_MIX = 2;
    
    const int SkeletonBinary::CURVE_LINEAR = 0;
    const int SkeletonBinary::CURVE_STEPPED = 1;
    const int SkeletonBinary::CURVE_BEZIER = 2;
    
    const TransformMode SkeletonBinary::TRANSFORM_MODE_VALUES[5] = {
        TransformMode_Normal,
        TransformMode_OnlyTranslation,
        TransformMode_NoRotationOrReflection,
        TransformMode_NoScale,
        TransformMode_NoScaleOrReflection
    };
    
    SkeletonBinary::SkeletonBinary(Vector<Atlas*>& atlasArray) : _attachmentLoader(NEW(AtlasAttachmentLoader)), _error(), _scale(1), _ownsLoader(true)
    {
        new (_attachmentLoader) AtlasAttachmentLoader(atlasArray);
    }
    
    SkeletonBinary::SkeletonBinary(AttachmentLoader* attachmentLoader) : _attachmentLoader(attachmentLoader), _error(), _scale(1), _ownsLoader(false)
    {
        assert(_attachmentLoader != NULL);
    }
    
    SkeletonBinary::~SkeletonBinary()
    {
        ContainerUtil::cleanUpVectorOfPointers(_linkedMeshes);
        
        if (_ownsLoader)
        {
            DESTROY(AttachmentLoader, _attachmentLoader);
        }
    }
    
    SkeletonData* SkeletonBinary::readSkeletonData(const unsigned char* binary, const int length)
    {
        int i, ii, nonessential;
        SkeletonData* skeletonData;
        
        DataInput* input = CALLOC(DataInput, 1);
        input->cursor = binary;
        input->end = binary + length;
        
        _linkedMeshes.clear();
        
        skeletonData = NEW(SkeletonData);
        new (skeletonData) SkeletonData();
        
        char* skeletonData_hash = readString(input);
        skeletonData->_hash = std::string(skeletonData_hash);
        FREE(skeletonData_hash);
        
        char* skeletonData_version = readString(input);
        skeletonData->_version = std::string(skeletonData_version);
        FREE(skeletonData_version);
        
        skeletonData->_width = readFloat(input);
        skeletonData->_height = readFloat(input);
        
        nonessential = readBoolean(input);
        
        if (nonessential)
        {
            /* Skip images path & fps */
            readFloat(input);
            FREE(readString(input));
        }
        
        /* Bones. */
        int bonesCount = readVarint(input, 1);
        skeletonData->_bones.reserve(bonesCount);
        for (i = 0; i < bonesCount; ++i)
        {
            BoneData* data;
            int mode;
            const char* name = readString(input);
            BoneData* parent = i == 0 ? 0 : skeletonData->_bones[readVarint(input, 1)];
            
            data = NEW(BoneData);
            new (data) BoneData(i, std::string(name), parent);
            
            FREE(name);
            
            data->_rotation = readFloat(input);
            data->_x = readFloat(input) * _scale;
            data->_y = readFloat(input) * _scale;
            data->_scaleX = readFloat(input);
            data->_scaleY = readFloat(input);
            data->_shearX = readFloat(input);
            data->_shearY = readFloat(input);
            data->_length = readFloat(input) * _scale;
            
            mode = readVarint(input, 1);
            switch (mode)
            {
                case 0:
                    data->_transformMode = TransformMode_Normal;
                    break;
                case 1:
                    data->_transformMode = TransformMode_OnlyTranslation;
                    break;
                case 2:
                    data->_transformMode = TransformMode_NoRotationOrReflection;
                    break;
                case 3:
                    data->_transformMode = TransformMode_NoScale;
                    break;
                case 4:
                    data->_transformMode = TransformMode_NoScaleOrReflection;
                    break;
            }
            
            if (nonessential)
            {
                /* Skip bone color. */
                readInt(input);
            }
            
            skeletonData->_bones[i] = data;
        }

        /* Slots. */
        int slotsCount = readVarint(input, 1);
        skeletonData->_slots.reserve(slotsCount);
        for (i = 0; i < slotsCount; ++i)
        {
            int r, g, b, a;
            const char* slotName = readString(input);
            BoneData* boneData = skeletonData->_bones[readVarint(input, 1)];
            
            SlotData* slotData = NEW(SlotData);
            new (slotData) SlotData(i, std::string(slotName), *boneData);
            
            FREE(slotName);
            readColor(input, &slotData->_r, &slotData->_g, &slotData->_b, &slotData->_a);
            r = readByte(input);
            g = readByte(input);
            b = readByte(input);
            a = readByte(input);
            if (!(r == 0xff && g == 0xff && b == 0xff && a == 0xff))
            {
                slotData->_r2 = r / 255.0f;
                slotData->_g2 = g / 255.0f;
                slotData->_b2 = b / 255.0f;
            }
            char* slotData_attachmentName = readString(input);
            slotData->_attachmentName = std::string(slotData_attachmentName);
            FREE(slotData_attachmentName);
            slotData->_blendMode = static_cast<BlendMode>(readVarint(input, 1));
            
            skeletonData->_slots[i] = slotData;
        }

        /* IK constraints. */
        int ikConstraintsCount = readVarint(input, 1);
        skeletonData->_ikConstraints.reserve(ikConstraintsCount);
        for (i = 0; i < ikConstraintsCount; ++i)
        {
            const char* name = readString(input);
            
            IkConstraintData* data = NEW(IkConstraintData);
            new (data) IkConstraintData(std::string(name));
            
            data->_order = readVarint(input, 1);
            
            FREE(name);
            int bonesCount = readVarint(input, 1);
            data->_bones.reserve(bonesCount);
            for (ii = 0; ii < bonesCount; ++ii)
            {
                data->_bones[ii] = skeletonData->_bones[readVarint(input, 1)];
            }
            data->_target = skeletonData->_bones[readVarint(input, 1)];
            data->_mix = readFloat(input);
            data->_bendDirection = readSByte(input);
            
            skeletonData->_ikConstraints[i] = data;
        }

        /* Transform constraints. */
        int transformConstraintsCount = readVarint(input, 1);
        skeletonData->_transformConstraints.reserve(transformConstraintsCount);
        for (i = 0; i < transformConstraintsCount; ++i)
        {
            const char* name = readString(input);
            
            TransformConstraintData* data = NEW(TransformConstraintData);
            new (data) TransformConstraintData(std::string(name));
            
            data->_order = readVarint(input, 1);
            FREE(name);
            int bonesCount = readVarint(input, 1);
            data->_bones.reserve(bonesCount);
            for (ii = 0; ii < bonesCount; ++ii)
            {
                data->_bones[ii] = skeletonData->_bones[readVarint(input, 1)];
            }
            data->_target = skeletonData->_bones[readVarint(input, 1)];
            data->_local = readBoolean(input);
            data->_relative = readBoolean(input);
            data->_offsetRotation = readFloat(input);
            data->_offsetX = readFloat(input) * _scale;
            data->_offsetY = readFloat(input) * _scale;
            data->_offsetScaleX = readFloat(input);
            data->_offsetScaleY = readFloat(input);
            data->_offsetShearY = readFloat(input);
            data->_rotateMix = readFloat(input);
            data->_translateMix = readFloat(input);
            data->_scaleMix = readFloat(input);
            data->_shearMix = readFloat(input);
            
            skeletonData->_transformConstraints[i] = data;
        }

        /* Path constraints */
        int pathConstraintsCount = readVarint(input, 1);
        skeletonData->_pathConstraints.reserve(pathConstraintsCount);
        for (i = 0; i < pathConstraintsCount; ++i)
        {
            const char* name = readString(input);
            
            PathConstraintData* data = NEW(PathConstraintData);
            new (data) PathConstraintData(std::string(name));
            
            data->_order = readVarint(input, 1);
            FREE(name);
            
            int bonesCount = readVarint(input, 1);
            data->_bones.reserve(bonesCount);
            for (ii = 0; ii < bonesCount; ++ii)
            {
                data->_bones[ii] = skeletonData->_bones[readVarint(input, 1)];
            }
            data->_target = skeletonData->_slots[readVarint(input, 1)];
            data->_positionMode = static_cast<PositionMode>(readVarint(input, 1));
            data->_spacingMode = static_cast<SpacingMode>(readVarint(input, 1));
            data->_rotateMode = static_cast<RotateMode>(readVarint(input, 1));
            data->_offsetRotation = readFloat(input);
            data->_position = readFloat(input);
            if (data->_positionMode == PositionMode_Fixed)
            {
                data->_position *= _scale;
            }
            
            data->_spacing = readFloat(input);
            if (data->_spacingMode == SpacingMode_Length || data->_spacingMode == SpacingMode_Fixed)
            {
                data->_spacing *= _scale;
            }
            data->_rotateMix = readFloat(input);
            data->_translateMix = readFloat(input);
            
            skeletonData->_pathConstraints[i] = data;
        }

        /* Default skin. */
        skeletonData->_defaultSkin = readSkin(input, "default", skeletonData, nonessential);
        int skinsCount = readVarint(input, 1);

        if (skeletonData->_defaultSkin)
        {
            ++skinsCount;
        }

        skeletonData->_skins.reserve(skinsCount);

        if (skeletonData->_defaultSkin)
        {
            skeletonData->_skins[0] = skeletonData->_defaultSkin;
        }

        /* Skins. */
        for (i = skeletonData->_defaultSkin ? 1 : 0; i < skeletonData->_skins.size(); ++i)
        {
            const char* skinName = readString(input);
            skeletonData->_skins[i] = readSkin(input, skinName, skeletonData, nonessential);
            FREE(skinName);
        }

        /* Linked meshes. */
        for (int i = 0, n = static_cast<int>(_linkedMeshes.size()); i < n; ++i)
        {
            LinkedMesh* linkedMesh = _linkedMeshes[i];
            Skin* skin = linkedMesh->_skin.length() == 0 ? skeletonData->getDefaultSkin() : skeletonData->findSkin(linkedMesh->_skin);
            if (skin == NULL)
            {
                FREE(input);
                DESTROY(SkeletonData, skeletonData);
                setError("Skin not found: ", linkedMesh->_skin.c_str());
                return NULL;
            }
            Attachment* parent = skin->getAttachment(linkedMesh->_slotIndex, linkedMesh->_parent);
            if (parent == NULL)
            {
                FREE(input);
                DESTROY(SkeletonData, skeletonData);
                setError("Parent mesh not found: ", linkedMesh->_parent.c_str());
                return NULL;
            }
            linkedMesh->_mesh->_parentMesh = static_cast<MeshAttachment*>(parent);
            linkedMesh->_mesh->updateUVs();
        }
        _linkedMeshes.clear();

        /* Events. */
        int eventsCount = readVarint(input, 1);
        skeletonData->_events.reserve(eventsCount);
        for (i = 0; i < eventsCount; ++i)
        {
            const char* name = readString(input);
            EventData* eventData = NEW(EventData);
            new (eventData) EventData(std::string(name));
            FREE(name);
            eventData->_intValue = readVarint(input, 0);
            eventData->_floatValue = readFloat(input);
            const char* eventData_stringValue = readString(input);
            eventData->_stringValue = std::string(eventData_stringValue);
            FREE(eventData_stringValue);
            skeletonData->_events[i] = eventData;
        }

        /* Animations. */
        int animationsCount = readVarint(input, 1);
        skeletonData->_animations.reserve(animationsCount);
        for (i = 0; i < animationsCount; ++i)
        {
            const char* name = readString(input);
            Animation* animation = readAnimation(name, input, skeletonData);
            FREE(name);
            if (!animation)
            {
                FREE(input);
                DESTROY(SkeletonData, skeletonData);
                return NULL;
            }
            skeletonData->_animations[i] = animation;
        }

        FREE(input);
        
        return skeletonData;
    }
    
    SkeletonData* SkeletonBinary::readSkeletonDataFile(const char* path)
    {
        int length;
        SkeletonData* skeletonData;
        const char* binary = SPINE_EXTENSION->spineReadFile(path, &length);
        if (length == 0 || !binary)
        {
            setError("Unable to read skeleton file: ", path);
            return NULL;
        }
        skeletonData = readSkeletonData((unsigned char*)binary, length);
        FREE(binary);
        return skeletonData;
    }
    
    void SkeletonBinary::setError(const char* value1, const char* value2)
    {
        char message[256];
        int length;
        strcpy(message, value1);
        length = (int)strlen(value1);
        if (value2)
        {
            strncat(message + length, value2, 255 - length);
        }
        
        _error = std::string(message);
    }
    
    char* SkeletonBinary::readString(DataInput* input)
    {
        int length = readVarint(input, 1);
        char* string;
        if (length == 0) {
            return NULL;
        }
        string = MALLOC(char, length);
        memcpy(string, input->cursor, length - 1);
        input->cursor += length - 1;
        string[length - 1] = '\0';
        return string;
    }
    
    float SkeletonBinary::readFloat(DataInput* input)
    {
        union
        {
            int intValue;
            float floatValue;
        } intToFloat;
        
        intToFloat.intValue = readInt(input);
        
        return intToFloat.floatValue;
    }
    
    unsigned char SkeletonBinary::readByte(DataInput* input)
    {
        return *input->cursor++;
    }
    
    signed char SkeletonBinary::readSByte(DataInput* input)
    {
        return (signed char)readByte(input);
    }
    
    int SkeletonBinary::readBoolean(DataInput* input)
    {
        return readByte(input) != 0;
    }
    
    int SkeletonBinary::readInt(DataInput* input)
    {
        int result = readByte(input);
        result <<= 8;
        result |= readByte(input);
        result <<= 8;
        result |= readByte(input);
        result <<= 8;
        result |= readByte(input);
        return result;
    }
    
    void SkeletonBinary::readColor(DataInput* input, float *r, float *g, float *b, float *a)
    {
        *r = readByte(input) / 255.0f;
        *g = readByte(input) / 255.0f;
        *b = readByte(input) / 255.0f;
        *a = readByte(input) / 255.0f;
    }
    
    int SkeletonBinary::readVarint(DataInput* input, bool optimizePositive)
    {
        unsigned char b = readByte(input);
        int value = b & 0x7F;
        if (b & 0x80)
        {
            b = readByte(input);
            value |= (b & 0x7F) << 7;
            if (b & 0x80)
            {
                b = readByte(input);
                value |= (b & 0x7F) << 14;
                if (b & 0x80)
                {
                    b = readByte(input);
                    value |= (b & 0x7F) << 21;
                    if (b & 0x80) value |= (readByte(input) & 0x7F) << 28;
                }
            }
        }
        
        if (!optimizePositive)
        {
            value = (((unsigned int)value >> 1) ^ -(value & 1));
        }
        
        return value;
    }
    
    Skin* SkeletonBinary::readSkin(DataInput* input, const char* skinName, SkeletonData* skeletonData, bool nonessential)
    {
        Skin* skin = NULL;
        int slotCount = readVarint(input, 1);
        int i, ii, nn;
        if (slotCount == 0)
        {
            return NULL;
        }
        
        skin = NEW(Skin);
        new (skin) Skin(std::string(skinName));
        
        for (i = 0; i < slotCount; ++i)
        {
            int slotIndex = readVarint(input, 1);
            for (ii = 0, nn = readVarint(input, 1); ii < nn; ++ii)
            {
                const char* name = readString(input);
                Attachment* attachment = readAttachment(input, skin, slotIndex, name, skeletonData, nonessential);
                if (attachment)
                {
                    skin->addAttachment(slotIndex, std::string(name), attachment);
                }
                FREE(name);
            }
        }
        
        return skin;
    }
    
    Attachment* SkeletonBinary::readAttachment(DataInput* input, Skin* skin, int slotIndex, const char* attachmentName, SkeletonData* skeletonData, bool nonessential)
    {
        int i;
        AttachmentType type;
        const char* name = readString(input);
        int freeName = name != 0;
        if (!name)
        {
            freeName = 0;
            name = attachmentName;
        }
        
        type = static_cast<AttachmentType>(readByte(input));
        
        switch (type)
        {
            case AttachmentType_Region:
            {
                const char* path = readString(input);
                RegionAttachment* region;
                if (!path)
                {
                    path = name;
                }
                region = _attachmentLoader->newRegionAttachment(*skin, std::string(name), std::string(path));
                region->_path = std::string(path);
                region->_rotation = readFloat(input);
                region->_x = readFloat(input) * _scale;
                region->_y = readFloat(input) * _scale;
                region->_scaleX = readFloat(input);
                region->_scaleY = readFloat(input);
                region->_width = readFloat(input) * _scale;
                region->_height = readFloat(input) * _scale;
                readColor(input, &region->_r, &region->_g, &region->_b, &region->_a);
                region->updateOffset();
                
                if (freeName)
                {
                    FREE(name);
                }
                
                return region;
            }
            case AttachmentType_Boundingbox:
            {
                int vertexCount = readVarint(input, 1);
                BoundingBoxAttachment* box = _attachmentLoader->newBoundingBoxAttachment(*skin, std::string(name));
                readVertices(input, static_cast<VertexAttachment*>(box), vertexCount);
                if (nonessential)
                {
                    /* Skip color. */
                    readInt(input);
                }
                if (freeName)
                {
                    FREE(name);
                }
                
                return box;
            }
            case AttachmentType_Mesh:
            {
                int vertexCount;
                MeshAttachment* mesh;
                const char* path = readString(input);
                if (!path)
                {
                    path = name;
                }
                mesh = _attachmentLoader->newMeshAttachment(*skin, std::string(name), std::string(path));
                mesh->_path = std::string(path);
                readColor(input, &mesh->_r, &mesh->_g, &mesh->_b, &mesh->_a);
                vertexCount = readVarint(input, 1);
                Vector<float> float_array = readFloatArray(input, vertexCount << 1, 1);
                mesh->setRegionUVs(float_array);
                Vector<short> triangles = readShortArray(input);
                mesh->setTriangles(triangles);
                readVertices(input, static_cast<VertexAttachment*>(mesh), vertexCount);
                mesh->updateUVs();
                mesh->_hullLength = readVarint(input, 1) << 1;
                if (nonessential)
                {
                    Vector<short> edges = readShortArray(input);
                    mesh->setEdges(edges);
                    mesh->_width = readFloat(input) * _scale;
                    mesh->_height = readFloat(input) * _scale;
                }
                else
                {
                    mesh->_width = 0;
                    mesh->_height = 0;
                }
                
                if (freeName)
                {
                    FREE(name);
                }
                
                return mesh;
            }
            case AttachmentType_Linkedmesh:
            {
                const char* skinName;
                const char* parent;
                MeshAttachment* mesh;
                const char* path = readString(input);
                if (!path)
                {
                    path = name;
                }
                
                mesh = _attachmentLoader->newMeshAttachment(*skin, std::string(name), std::string(path));
                mesh->_path = path;
                readColor(input, &mesh->_r, &mesh->_g, &mesh->_b, &mesh->_a);
                skinName = readString(input);
                parent = readString(input);
                mesh->_inheritDeform = readBoolean(input);
                if (nonessential)
                {
                    mesh->_width = readFloat(input) * _scale;
                    mesh->_height = readFloat(input) * _scale;
                }
                
                LinkedMesh* linkedMesh = NEW(LinkedMesh);
                new (linkedMesh) LinkedMesh(mesh, std::string(skinName), slotIndex, std::string(parent));
                _linkedMeshes.push_back(linkedMesh);
                
                if (freeName)
                {
                    FREE(name);
                }
                
                FREE(skinName);
                FREE(parent);
                
                return mesh;
            }
            case AttachmentType_Path:
            {
                PathAttachment* path = _attachmentLoader->newPathAttachment(*skin, std::string(name));
                int vertexCount = 0;
                path->_closed = readBoolean(input);
                path->_constantSpeed = readBoolean(input);
                vertexCount = readVarint(input, 1);
                readVertices(input, static_cast<VertexAttachment*>(path), vertexCount);
                int lengthsLength = vertexCount / 3;
                path->_lengths.reserve(lengthsLength);
                for (i = 0; i < lengthsLength; ++i)
                {
                    path->_lengths[i] = readFloat(input) * _scale;
                }
                
                if (nonessential)
                {
                    /* Skip color. */
                    readInt(input);
                }
                
                if (freeName)
                {
                    FREE(name);
                }
                
                return path;
            }
            case AttachmentType_Point:
            {
                PointAttachment* point = _attachmentLoader->newPointAttachment(*skin, std::string(name));
                point->_rotation = readFloat(input);
                point->_x = readFloat(input) * _scale;
                point->_y = readFloat(input) * _scale;
                
                if (nonessential)
                {
                    /* Skip color. */
                    readInt(input);
                }
                
                return point;
            }
            case AttachmentType_Clipping:
            {
                int endSlotIndex = readVarint(input, 1);
                int vertexCount = readVarint(input, 1);
                ClippingAttachment* clip = _attachmentLoader->newClippingAttachment(*skin, name);
                readVertices(input, static_cast<VertexAttachment*>(clip), vertexCount);
                
                if (nonessential)
                {
                    /* Skip color. */
                    readInt(input);
                }
                
                clip->_endSlot = skeletonData->_slots[endSlotIndex];
                
                if (freeName)
                {
                    FREE(name);
                }
                
                return clip;
            }
        }
        
        if (freeName)
        {
            FREE(name);
        }
        
        return NULL;
    }
    
    void SkeletonBinary::readVertices(DataInput* input, VertexAttachment* attachment, int vertexCount)
    {
        float scale = _scale;
        int verticesLength = vertexCount << 1;
        
        if (!readBoolean(input))
        {
            attachment->setVertices(readFloatArray(input, verticesLength, scale));
            return;
        }
        
        Vertices vertices;
        vertices._bones.reserve(verticesLength * 3);
        vertices._vertices.reserve(verticesLength * 3 * 3);
        
        for (int i = 0; i < vertexCount; ++i)
        {
            int boneCount = readVarint(input, true);
            vertices._bones.push_back(boneCount);
            for (int ii = 0; ii < boneCount; ++ii)
            {
                vertices._bones.push_back(readVarint(input, true));
                vertices._vertices.push_back(readFloat(input) * scale);
                vertices._vertices.push_back(readFloat(input) * scale);
                vertices._vertices.push_back(readFloat(input));
            }
        }
        
        attachment->setVertices(vertices._vertices);
        attachment->setBones(vertices._bones);
    }
    
    Vector<float> SkeletonBinary::readFloatArray(DataInput *input, int n, float scale)
    {
        Vector<float> array;
        array.reserve(n);
        
        int i;
        if (scale == 1)
        {
            for (i = 0; i < n; ++i)
            {
                array[i] = readFloat(input);
            }
        }
        else
        {
            for (i = 0; i < n; ++i)
            {
                array[i] = readFloat(input) * scale;
            }
        }
        
        return array;
    }
    
    Vector<short> SkeletonBinary::readShortArray(DataInput *input)
    {
        int n = readVarint(input, 1);
        
        Vector<short> array;
        array.reserve(n);
        
        int i;
        for (i = 0; i < n; ++i)
        {
            array[i] = readByte(input) << 8;
            array[i] |= readByte(input);
        }
        
        return array;
    }
    
    Animation* SkeletonBinary::readAnimation(const char* name, DataInput* input, SkeletonData *skeletonData)
    {
        Vector<Timeline*> timelines;
        float scale = _scale;
        float duration = 0;

        // Slot timelines.
        for (int i = 0, n = readVarint(input, true); i < n; ++i)
        {
            int slotIndex = readVarint(input, true);
            for (int ii = 0, nn = readVarint(input, true); ii < nn; ++ii)
            {
                unsigned char timelineType = readByte(input);
                int frameCount = readVarint(input, true);
                switch (timelineType)
                {
                    case SLOT_ATTACHMENT:
                    {
                        AttachmentTimeline* timeline = NEW(AttachmentTimeline);
                        new(timeline) AttachmentTimeline(frameCount);
                        timeline->_slotIndex = slotIndex;
                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            const char* attachmentName = readString(input);
                            timeline->setFrame(frameIndex, readFloat(input), std::string(attachmentName));
                            FREE(attachmentName);
                        }
                        timelines.push_back(timeline);
                        duration = MAX(duration, timeline->_frames[frameCount - 1]);
                        break;
                    }
                    case SLOT_COLOR:
                    {
                        ColorTimeline* timeline = NEW(ColorTimeline);
                        new(timeline) ColorTimeline(frameCount);
                        timeline->_slotIndex = slotIndex;
                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            float time = readFloat(input);
                            int color = readInt(input);
                            float r = ((color & 0xff000000) >> 24) / 255.0f;
                            float g = ((color & 0x00ff0000) >> 16) / 255.0f;
                            float b = ((color & 0x0000ff00) >> 8) / 255.0f;
                            float a = ((color & 0x000000ff)) / 255.0f;
                            timeline->setFrame(frameIndex, time, r, g, b, a);
                            if (frameIndex < frameCount - 1)
                            {
                                readCurve(input, frameIndex, timeline);
                            }
                        }
                        timelines.push_back(timeline);
                        duration = MAX(duration, timeline->_frames[(frameCount - 1) * ColorTimeline::ENTRIES]);
                        break;
                    }
                    case SLOT_TWO_COLOR:
                    {
                        TwoColorTimeline* timeline = NEW(TwoColorTimeline);
                        new(timeline) TwoColorTimeline(frameCount);
                        timeline->_slotIndex = slotIndex;
                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            float time = readFloat(input);
                            int color = readInt(input);
                            float r = ((color & 0xff000000) >> 24) / 255.0f;
                            float g = ((color & 0x00ff0000) >> 16) / 255.0f;
                            float b = ((color & 0x0000ff00) >> 8) / 255.0f;
                            float a = ((color & 0x000000ff)) / 255.0f;
                            int color2 = readInt(input); // 0x00rrggbb
                            float r2 = ((color2 & 0x00ff0000) >> 16) / 255.0f;
                            float g2 = ((color2 & 0x0000ff00) >> 8) / 255.0f;
                            float b2 = ((color2 & 0x000000ff)) / 255.0f;

                            timeline->setFrame(frameIndex, time, r, g, b, a, r2, g2, b2);
                            if (frameIndex < frameCount - 1)
                            {
                                readCurve(input, frameIndex, timeline);
                            }
                        }
                        timelines.push_back(timeline);
                        duration = MAX(duration, timeline->_frames[(frameCount - 1) * TwoColorTimeline::ENTRIES]);
                        break;
                    }
                    default:
                    {
                        ContainerUtil::cleanUpVectorOfPointers(timelines);
                        setError("Invalid timeline type for a slot: ", skeletonData->_slots[slotIndex]->_name.c_str());
                        return NULL;
                    }
                }
            }
        }

        // Bone timelines.
        for (int i = 0, n = readVarint(input, true); i < n; ++i)
        {
            int boneIndex = readVarint(input, true);
            for (int ii = 0, nn = readVarint(input, true); ii < nn; ++ii)
            {
                unsigned char timelineType = readByte(input);
                int frameCount = readVarint(input, true);
                switch (timelineType)
                {
                    case BONE_ROTATE:
                    {
                        RotateTimeline* timeline = NEW(RotateTimeline);
                        new(timeline) RotateTimeline(frameCount);
                        timeline->_boneIndex = boneIndex;
                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            timeline->setFrame(frameIndex, readFloat(input), readFloat(input));
                            if (frameIndex < frameCount - 1)
                            {
                                readCurve(input, frameIndex, timeline);
                            }
                        }
                        timelines.push_back(timeline);
                        duration = MAX(duration, timeline->_frames[(frameCount - 1) * RotateTimeline::ENTRIES]);
                        break;
                    }
                    case BONE_TRANSLATE:
                    case BONE_SCALE:
                    case BONE_SHEAR:
                    {
                        TranslateTimeline* timeline;
                        float timelineScale = 1;
                        if (timelineType == BONE_SCALE)
                        {
                            timeline = NEW(ScaleTimeline);
                            new(timeline) ScaleTimeline(frameCount);
                        }
                        else if (timelineType == BONE_SHEAR)
                        {
                            timeline = NEW(ShearTimeline);
                            new(timeline) ShearTimeline(frameCount);
                        }
                        else
                        {
                            timeline = NEW(TranslateTimeline);
                            new(timeline) TranslateTimeline(frameCount);
                            timelineScale = scale;
                        }
                        timeline->_boneIndex = boneIndex;
                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            timeline->setFrame(frameIndex, readFloat(input), readFloat(input) * timelineScale, readFloat(input) * timelineScale);
                            if (frameIndex < frameCount - 1)
                            {
                                readCurve(input, frameIndex, timeline);
                            }
                        }
                        timelines.push_back(timeline);
                        duration = MAX(duration, timeline->_frames[(frameCount - 1) * TranslateTimeline::ENTRIES]);
                        break;
                    }
                    default:
                    {
                        ContainerUtil::cleanUpVectorOfPointers(timelines);
                        setError("Invalid timeline type for a bone: ", skeletonData->_bones[boneIndex]->_name.c_str());
                        return NULL;
                    }
                }
            }
        }

        // IK timelines.
        for (int i = 0, n = readVarint(input, true); i < n; ++i)
        {
            int index = readVarint(input, true);
            int frameCount = readVarint(input, true);
            IkConstraintTimeline* timeline = NEW(IkConstraintTimeline);
            new(timeline) IkConstraintTimeline(frameCount);
            timeline->_ikConstraintIndex = index;
            for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
            {
                timeline->setFrame(frameIndex, readFloat(input), readFloat(input), readSByte(input));
                if (frameIndex < frameCount - 1)
                {
                    readCurve(input, frameIndex, timeline);
                }
            }
            timelines.push_back(timeline);
            duration = MAX(duration, timeline->_frames[(frameCount - 1) * IkConstraintTimeline::ENTRIES]);
        }

        // Transform constraint timelines.
        for (int i = 0, n = readVarint(input, true); i < n; ++i)
        {
            int index = readVarint(input, true);
            int frameCount = readVarint(input, true);
            TransformConstraintTimeline* timeline = NEW(TransformConstraintTimeline);
            new(timeline) TransformConstraintTimeline(frameCount);
            timeline->_transformConstraintIndex = index;
            for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
            {
                timeline->setFrame(frameIndex, readFloat(input), readFloat(input), readFloat(input), readFloat(input), readFloat(input));
                if (frameIndex < frameCount - 1)
                {
                    readCurve(input, frameIndex, timeline);
                }
            }
            timelines.push_back(timeline);
            duration = MAX(duration, timeline->_frames[(frameCount - 1) * TransformConstraintTimeline::ENTRIES]);
        }

        // Path constraint timelines.
        for (int i = 0, n = readVarint(input, true); i < n; ++i)
        {
            int index = readVarint(input, true);
            PathConstraintData* data = skeletonData->_pathConstraints[index];
            for (int ii = 0, nn = readVarint(input, true); ii < nn; ++ii)
            {
                int timelineType = readSByte(input);
                int frameCount = readVarint(input, true);
                switch(timelineType)
                {
                    case PATH_POSITION:
                    case PATH_SPACING:
                    {
                        PathConstraintPositionTimeline* timeline;
                        float timelineScale = 1;
                        if (timelineType == PATH_SPACING)
                        {
                            timeline = NEW(PathConstraintSpacingTimeline);
                            new(timeline) PathConstraintSpacingTimeline(frameCount);
                            
                            if (data->_spacingMode == SpacingMode_Length || data->_spacingMode == SpacingMode_Fixed)
                            {
                                timelineScale = scale;
                            }
                        }
                        else
                        {
                            timeline = NEW(PathConstraintPositionTimeline);
                            new(timeline) PathConstraintPositionTimeline(frameCount);
                            
                            if (data->_positionMode == PositionMode_Fixed)
                            {
                                timelineScale = scale;
                            }
                        }
                        timeline->_pathConstraintIndex = index;
                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            timeline->setFrame(frameIndex, readFloat(input), readFloat(input) * timelineScale);
                            if (frameIndex < frameCount - 1)
                            {
                                readCurve(input, frameIndex, timeline);
                            }
                        }
                        timelines.push_back(timeline);
                        duration = MAX(duration, timeline->_frames[(frameCount - 1) * PathConstraintPositionTimeline::ENTRIES]);
                        break;
                    }
                    case PATH_MIX:
                    {
                        PathConstraintMixTimeline* timeline = NEW(PathConstraintMixTimeline);
                        new(timeline) PathConstraintMixTimeline(frameCount);
                        
                        timeline->_pathConstraintIndex = index;
                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            timeline->setFrame(frameIndex, readFloat(input), readFloat(input), readFloat(input));
                            if (frameIndex < frameCount - 1)
                            {
                                readCurve(input, frameIndex, timeline);
                            }
                        }
                        timelines.push_back(timeline);
                        duration = MAX(duration, timeline->_frames[(frameCount - 1) * PathConstraintMixTimeline::ENTRIES]);
                        break;
                    }
                }
            }
        }

        // Deform timelines.
        for (int i = 0, n = readVarint(input, true); i < n; ++i)
        {
            Skin* skin = skeletonData->_skins[readVarint(input, true)];
            for (int ii = 0, nn = readVarint(input, true); ii < nn; ++ii)
            {
                int slotIndex = readVarint(input, true);
                for (int iii = 0, nnn = readVarint(input, true); iii < nnn; iii++)
                {
                    const char* attachmentName = readString(input);
                    Attachment* baseAttachment = skin->getAttachment(slotIndex, std::string(attachmentName));
                    
                    if (!baseAttachment)
                    {
                        ContainerUtil::cleanUpVectorOfPointers(timelines);
                        setError("Attachment not found: ", attachmentName);
                        FREE(attachmentName);
                        return NULL;
                    }
                    
                    FREE(attachmentName);
                    
                    VertexAttachment* attachment = static_cast<VertexAttachment*>(baseAttachment);
                    
                    bool weighted = attachment->_bones.size() > 0;
                    Vector<float>& vertices = attachment->_vertices;
                    int deformLength = weighted ? static_cast<int>(vertices.size()) / 3 * 2 : static_cast<int>(vertices.size());

                    int frameCount = readVarint(input, true);
                    
                    DeformTimeline* timeline = NEW(DeformTimeline);
                    new(timeline) DeformTimeline(frameCount);
                    
                    timeline->_slotIndex = slotIndex;
                    timeline->_attachment = attachment;

                    for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                    {
                        float time = readFloat(input);
                        Vector<float> deform;
                        int end = readVarint(input, true);
                        if (end == 0)
                        {
                            if (weighted)
                            {
                                deform.reserve(deformLength);
                            }
                            else
                            {
                                deform = vertices;
                            }
                        }
                        else
                        {
                            deform.reserve(deformLength);
                            int start = readVarint(input, true);
                            end += start;
                            if (scale == 1)
                            {
                                for (int v = start; v < end; ++v)
                                {
                                    deform[v] = readFloat(input);
                                }
                            }
                            else
                            {
                                for (int v = start; v < end; ++v)
                                {
                                    deform[v] = readFloat(input) * scale;
                                }
                            }

                            if (!weighted)
                            {
                                for (int v = 0, vn = static_cast<int>(deform.size()); v < vn; ++v)
                                {
                                    deform[v] += vertices[v];
                                }
                            }
                        }

                        timeline->setFrame(frameIndex, time, deform);
                        if (frameIndex < frameCount - 1)
                        {
                            readCurve(input, frameIndex, timeline);
                        }
                    }

                    timelines.push_back(timeline);
                    duration = MAX(duration, timeline->_frames[frameCount - 1]);
                }
            }
        }

        // Draw order timeline.
        int drawOrderCount = readVarint(input, true);
        if (drawOrderCount > 0)
        {
            DrawOrderTimeline* timeline = NEW(DrawOrderTimeline);
            new(timeline) DrawOrderTimeline(drawOrderCount);
            
            int slotCount = static_cast<int>(skeletonData->_slots.size());
            for (int i = 0; i < drawOrderCount; ++i)
            {
                float time = readFloat(input);
                int offsetCount = readVarint(input, true);
                
                Vector<int> drawOrder;
                drawOrder.reserve(slotCount);
                for (int ii = slotCount - 1; ii >= 0; --ii)
                {
                    drawOrder[ii] = -1;
                }
                
                Vector<int> unchanged;
                unchanged.reserve(slotCount - offsetCount);
                int originalIndex = 0, unchangedIndex = 0;
                for (int ii = 0; ii < offsetCount; ++ii)
                {
                    int slotIndex = readVarint(input, true);
                    // Collect unchanged items.
                    while (originalIndex != slotIndex)
                    {
                        unchanged[unchangedIndex++] = originalIndex++;
                    }
                    // Set changed items.
                    int index = originalIndex;
                    drawOrder[index + readVarint(input, true)] = originalIndex++;
                }

                // Collect remaining unchanged items.
                while (originalIndex < slotCount)
                {
                    unchanged[unchangedIndex++] = originalIndex++;
                }

                // Fill in unchanged items.
                for (int ii = slotCount - 1; ii >= 0; --ii)
                {
                    if (drawOrder[ii] == -1)
                    {
                        drawOrder[ii] = unchanged[--unchangedIndex];
                    }
                }
                timeline->setFrame(i, time, drawOrder);
            }
            timelines.push_back(timeline);
            duration = MAX(duration, timeline->_frames[drawOrderCount - 1]);
        }

        // Event timeline.
        int eventCount = readVarint(input, true);
        if (eventCount > 0)
        {
            EventTimeline* timeline = NEW(EventTimeline);
            new(timeline) EventTimeline(eventCount);
            
            for (int i = 0; i < eventCount; ++i)
            {
                float time = readFloat(input);
                EventData* eventData = skeletonData->_events[readVarint(input, true)];
                Event* event = NEW(Event);
                new(event) Event(time, *eventData);
                
                event->_intValue = readVarint(input, false);
                event->_floatValue = readFloat(input);
                bool freeString = readBoolean(input);
                const char* event_stringValue = freeString ? readString(input) : eventData->_stringValue.c_str();
                event->_stringValue = std::string(event_stringValue);
                if (freeString)
                {
                    FREE(event_stringValue);
                }
                timeline->setFrame(i, event);
            }

            timelines.push_back(timeline);
            duration = MAX(duration, timeline->_frames[eventCount - 1]);
        }

        Animation* ret = NEW(Animation);
        new (ret) Animation(std::string(name), timelines, duration);
        
        return ret;
    }
    
    void SkeletonBinary::readCurve(DataInput* input, int frameIndex, CurveTimeline* timeline)
    {
        switch (readByte(input))
        {
            case CURVE_STEPPED:
            {
                timeline->setStepped(frameIndex);
                break;
            }
            case CURVE_BEZIER:
            {
                float cx1 = readFloat(input);
                float cy1 = readFloat(input);
                float cx2 = readFloat(input);
                float cy2 = readFloat(input);
                timeline->setCurve(frameIndex, cx1, cy1, cx2, cy2);
                break;
            }
        }
    }
}
