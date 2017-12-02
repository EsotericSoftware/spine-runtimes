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

#include <spine/Extension.h>
#include <spine/ContainerUtil.h>
#include <spine/BoneData.h>

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
            
            skeletonData->_bones.push_back(data);
        }

        /* Slots. */
//        skeletonData->slotsCount = readVarint(input, 1);
//        skeletonData->slots = MALLOC(spSlotData*, skeletonData->slotsCount);
//        for (i = 0; i < skeletonData->slotsCount; ++i)
//        {
//            int r, g, b, a;
//            const char* slotName = readString(input);
//            spBoneData* boneData = skeletonData->bones[readVarint(input, 1)];
//            /* TODO Avoid copying of slotName */
//            spSlotData* slotData = spSlotData_create(i, slotName, boneData);
//            FREE(slotName);
//            readColor(input, &slotData->color.r, &slotData->color.g, &slotData->color.b, &slotData->color.a);
//            r = readByte(input);
//            g = readByte(input);
//            b = readByte(input);
//            a = readByte(input);
//            if (!(r == 0xff && g == 0xff && b == 0xff && a == 0xff))
//            {
//                slotData->darkColor = spColor_create();
//                spColor_setFromFloats(slotData->darkColor, r / 255.0f, g / 255.0f, b / 255.0f, 1);
//            }
//            slotData->attachmentName = readString(input);
//            slotData->blendMode = (spBlendMode)readVarint(input, 1);
//            skeletonData->slots[i] = slotData;
//        }
//
//        /* IK constraints. */
//        skeletonData->ikConstraintsCount = readVarint(input, 1);
//        skeletonData->ikConstraints = MALLOC(spIkConstraintData*, skeletonData->ikConstraintsCount);
//        for (i = 0; i < skeletonData->ikConstraintsCount; ++i)
//        {
//            const char* name = readString(input);
//            /* TODO Avoid copying of name */
//            spIkConstraintData* data = spIkConstraintData_create(name);
//            data->order = readVarint(input, 1);
//            FREE(name);
//            data->bonesCount = readVarint(input, 1);
//            data->bones = MALLOC(spBoneData*, data->bonesCount);
//            for (ii = 0; ii < data->bonesCount; ++ii)
//                data->bones[ii] = skeletonData->bones[readVarint(input, 1)];
//            data->target = skeletonData->bones[readVarint(input, 1)];
//            data->mix = readFloat(input);
//            data->bendDirection = readSByte(input);
//            skeletonData->ikConstraints[i] = data;
//        }
//
//        /* Transform constraints. */
//        skeletonData->transformConstraintsCount = readVarint(input, 1);
//        skeletonData->transformConstraints = MALLOC(spTransformConstraintData*, skeletonData->transformConstraintsCount);
//        for (i = 0; i < skeletonData->transformConstraintsCount; ++i)
//        {
//            const char* name = readString(input);
//            /* TODO Avoid copying of name */
//            spTransformConstraintData* data = spTransformConstraintData_create(name);
//            data->order = readVarint(input, 1);
//            FREE(name);
//            data->bonesCount = readVarint(input, 1);
//            CONST_CAST(spBoneData**, data->bones) = MALLOC(spBoneData*, data->bonesCount);
//            for (ii = 0; ii < data->bonesCount; ++ii)
//            {
//                data->bones[ii] = skeletonData->bones[readVarint(input, 1)];
//            }
//            data->target = skeletonData->bones[readVarint(input, 1)];
//            data->local = readBoolean(input);
//            data->relative = readBoolean(input);
//            data->offsetRotation = readFloat(input);
//            data->offsetX = readFloat(input) * _scale;
//            data->offsetY = readFloat(input) * _scale;
//            data->offsetScaleX = readFloat(input);
//            data->offsetScaleY = readFloat(input);
//            data->offsetShearY = readFloat(input);
//            data->rotateMix = readFloat(input);
//            data->translateMix = readFloat(input);
//            data->scaleMix = readFloat(input);
//            data->shearMix = readFloat(input);
//            skeletonData->transformConstraints[i] = data;
//        }
//
//        /* Path constraints */
//        skeletonData->pathConstraintsCount = readVarint(input, 1);
//        skeletonData->pathConstraints = MALLOC(spPathConstraintData*, skeletonData->pathConstraintsCount);
//        for (i = 0; i < skeletonData->pathConstraintsCount; ++i)
//        {
//            const char* name = readString(input);
//            /* TODO Avoid copying of name */
//            spPathConstraintData* data = spPathConstraintData_create(name);
//            data->order = readVarint(input, 1);
//            FREE(name);
//            data->bonesCount = readVarint(input, 1);
//            CONST_CAST(spBoneData**, data->bones) = MALLOC(spBoneData*, data->bonesCount);
//            for (ii = 0; ii < data->bonesCount; ++ii)
//            {
//                data->bones[ii] = skeletonData->bones[readVarint(input, 1)];
//            }
//            data->target = skeletonData->slots[readVarint(input, 1)];
//            data->positionMode = (spPositionMode)readVarint(input, 1);
//            data->spacingMode = (spSpacingMode)readVarint(input, 1);
//            data->rotateMode = (spRotateMode)readVarint(input, 1);
//            data->offsetRotation = readFloat(input);
//            data->position = readFloat(input);
//            if (data->positionMode == SP_POSITION_MODE_FIXED) data->position *= _scale;
//            data->spacing = readFloat(input);
//            if (data->spacingMode == SP_SPACING_MODE_LENGTH || data->spacingMode == SP_SPACING_MODE_FIXED) data->spacing *= _scale;
//            data->rotateMix = readFloat(input);
//            data->translateMix = readFloat(input);
//            skeletonData->pathConstraints[i] = data;
//        }
//
//        /* Default skin. */
//        skeletonData->defaultSkin = spSkeletonBinary_readSkin(self, input, "default", skeletonData, nonessential);
//        skeletonData->skinsCount = readVarint(input, 1);
//
//        if (skeletonData->defaultSkin)
//        {
//            ++skeletonData->skinsCount;
//        }
//
//        skeletonData->skins = MALLOC(spSkin*, skeletonData->skinsCount);
//
//        if (skeletonData->defaultSkin)
//        {
//            skeletonData->skins[0] = skeletonData->defaultSkin;
//        }
//
//        /* Skins. */
//        for (i = skeletonData->defaultSkin ? 1 : 0; i < skeletonData->skinsCount; ++i)
//        {
//            const char* skinName = readString(input);
//            /* TODO Avoid copying of skinName */
//            skeletonData->skins[i] = spSkeletonBinary_readSkin(self, input, skinName, skeletonData, nonessential);
//            FREE(skinName);
//        }
//
//        /* Linked meshes. */
//        for (i = 0; i < internal->linkedMeshCount; ++i)
//        {
//            _spLinkedMesh* linkedMesh = internal->linkedMeshes + i;
//            spSkin* skin = !linkedMesh->skin ? skeletonData->defaultSkin : spSkeletonData_findSkin(skeletonData, linkedMesh->skin);
//            spAttachment* parent;
//            if (!skin)
//            {
//                FREE(input);
//                spSkeletonData_dispose(skeletonData);
//                _spSkeletonBinary_setError(self, "Skin not found: ", linkedMesh->skin);
//                return 0;
//            }
//            parent = spSkin_getAttachment(skin, linkedMesh->slotIndex, linkedMesh->parent);
//            if (!parent)
//            {
//                FREE(input);
//                spSkeletonData_dispose(skeletonData);
//                _spSkeletonBinary_setError(self, "Parent mesh not found: ", linkedMesh->parent);
//                return 0;
//            }
//            spMeshAttachment_setParentMesh(linkedMesh->mesh, SUB_CAST(spMeshAttachment, parent));
//            spMeshAttachment_updateUVs(linkedMesh->mesh);
//            spAttachmentLoader_configureAttachment(self->attachmentLoader, SUPER(SUPER(linkedMesh->mesh)));
//        }
//
//        /* Events. */
//        skeletonData->eventsCount = readVarint(input, 1);
//        skeletonData->events = MALLOC(spEventData*, skeletonData->eventsCount);
//        for (i = 0; i < skeletonData->eventsCount; ++i)
//        {
//            const char* name = readString(input);
//            /* TODO Avoid copying of skinName */
//            spEventData* eventData = spEventData_create(name);
//            FREE(name);
//            eventData->intValue = readVarint(input, 0);
//            eventData->floatValue = readFloat(input);
//            eventData->stringValue = readString(input);
//            skeletonData->events[i] = eventData;
//        }
//
//        /* Animations. */
//        skeletonData->animationsCount = readVarint(input, 1);
//        skeletonData->animations = MALLOC(spAnimation*, skeletonData->animationsCount);
//        for (i = 0; i < skeletonData->animationsCount; ++i)
//        {
//            const char* name = readString(input);
//            spAnimation* animation = _spSkeletonBinary_readAnimation(self, name, input, skeletonData);
//            FREE(name);
//            if (!animation)
//            {
//                FREE(input);
//                spSkeletonData_dispose(skeletonData);
//                return 0;
//            }
//            skeletonData->animations[i] = animation;
//        }

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
            return 0;
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
            return 0;
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
}
