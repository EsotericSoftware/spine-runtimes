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

#include <spine/SkeletonJson.h>

#include <stdio.h>

#include <spine/CurveTimeline.h>
#include <spine/VertexAttachment.h>
#include <spine/Json.h>
#include <spine/SkeletonData.h>
#include <spine/Atlas.h>
#include <spine/AtlasAttachmentLoader.h>
#include <spine/LinkedMesh.h>

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
#include <spine/Vertices.h>

#if defined(WIN32) || defined(_WIN32) || defined(__WIN32) && !defined(__CYGWIN__)
#define strdup _strdup
#endif

namespace Spine
{
    SkeletonJson::SkeletonJson(Vector<Atlas*>& atlasArray) : _attachmentLoader(NEW(AtlasAttachmentLoader)), _scale(1), _ownsLoader(true)
    {
        new (_attachmentLoader) AtlasAttachmentLoader(atlasArray);
    }
    
    SkeletonJson::SkeletonJson(AttachmentLoader* attachmentLoader) : _attachmentLoader(attachmentLoader), _scale(1), _ownsLoader(false)
    {
        assert(_attachmentLoader != NULL);
    }
    
    SkeletonJson::~SkeletonJson()
    {
        ContainerUtil::cleanUpVectorOfPointers(_linkedMeshes);
        
        if (_ownsLoader)
        {
            DESTROY(AttachmentLoader, _attachmentLoader);
        }
    }
    
    SkeletonData* SkeletonJson::readSkeletonDataFile(const char* path)
    {
        int length;
        SkeletonData* skeletonData;
        const char* json = SPINE_EXTENSION->spineReadFile(path, &length);
        if (length == 0 || !json)
        {
            setError(NULL, "Unable to read skeleton file: ", path);
            return NULL;
        }
        
        skeletonData = readSkeletonData(json);
        
        FREE(json);
        
        return skeletonData;
    }
    
    SkeletonData* SkeletonJson::readSkeletonData(const char* json)
    {
        int i, ii;
        SkeletonData* skeletonData;
        Json *root, *skeleton, *bones, *boneMap, *ik, *transform, *path, *slots, *skins, *animations, *events;

        _error.clear();
        _linkedMeshes.clear();

        root = NEW(Json);
        new (root) Json(json);
        
        if (!root)
        {
            setError(root, "Invalid skeleton JSON: ", Json::getError());
            return NULL;
        }

        skeletonData = NEW(SkeletonData);
        new (skeletonData) SkeletonData();

//        skeleton = Json::getItem(root, "skeleton");
//        if (skeleton) {
//            MALLOC_STR(skeletonData->hash, Json_getString(skeleton, "hash", 0));
//            MALLOC_STR(skeletonData->version, Json_getString(skeleton, "spine", 0));
//            skeletonData->width = Json_getFloat(skeleton, "width", 0);
//            skeletonData->height = Json_getFloat(skeleton, "height", 0);
//        }
//
//        /* Bones. */
//        bones = Json::getItem(root, "bones");
//        skeletonData->bones = MALLOC(spBoneData*, bones->_size);
//        for (boneMap = bones->_child, i = 0; boneMap; boneMap = boneMap->_next, ++i) {
//            spBoneData* data;
//            const char* transformMode;
//
//            spBoneData* parent = 0;
//            const char* parentName = Json_getString(boneMap, "parent", 0);
//            if (parentName) {
//                parent = spSkeletonData_findBone(skeletonData, parentName);
//                if (!parent) {
//                    spSkeletonData_dispose(skeletonData);
//                    setError(root, "Parent bone not found: ", parentName);
//                    return NULL;
//                }
//            }
//
//            data = spBoneData_create(skeletonData->bonesCount, Json_getString(boneMap, "name", 0), parent);
//            data->length = Json_getFloat(boneMap, "length", 0) * _scale;
//            data->x = Json_getFloat(boneMap, "x", 0) * _scale;
//            data->y = Json_getFloat(boneMap, "y", 0) * _scale;
//            data->rotation = Json_getFloat(boneMap, "rotation", 0);
//            data->scaleX = Json_getFloat(boneMap, "scaleX", 1);
//            data->scaleY = Json_getFloat(boneMap, "scaleY", 1);
//            data->shearX = Json_getFloat(boneMap, "shearX", 0);
//            data->shearY = Json_getFloat(boneMap, "shearY", 0);
//            transformMode = Json_getString(boneMap, "transform", "normal");
//            data->transformMode = SP_TRANSFORMMODE_NORMAL;
//            if (strcmp(transformMode, "normal") == 0)
//                data->transformMode = SP_TRANSFORMMODE_NORMAL;
//            if (strcmp(transformMode, "onlyTranslation") == 0)
//                data->transformMode = SP_TRANSFORMMODE_ONLYTRANSLATION;
//            if (strcmp(transformMode, "noRotationOrReflection") == 0)
//                data->transformMode = SP_TRANSFORMMODE_NOROTATIONORREFLECTION;
//            if (strcmp(transformMode, "noScale") == 0)
//                data->transformMode = SP_TRANSFORMMODE_NOSCALE;
//            if (strcmp(transformMode, "noScaleOrReflection") == 0)
//                data->transformMode = SP_TRANSFORMMODE_NOSCALEORREFLECTION;
//
//            skeletonData->bones[i] = data;
//            skeletonData->bonesCount++;
//        }
//
//        /* Slots. */
//        slots = Json::getItem(root, "slots");
//        if (slots) {
//            Json *slotMap;
//            skeletonData->slotsCount = slots->_size;
//            skeletonData->slots = MALLOC(spSlotData*, slots->_size);
//            for (slotMap = slots->_child, i = 0; slotMap; slotMap = slotMap->_next, ++i) {
//                spSlotData* data;
//                const char* color;
//                const char* dark;
//                Json *item;
//
//                const char* boneName = Json_getString(slotMap, "bone", 0);
//                spBoneData* boneData = spSkeletonData_findBone(skeletonData, boneName);
//                if (!boneData) {
//                    spSkeletonData_dispose(skeletonData);
//                    setError(root, "Slot bone not found: ", boneName);
//                    return NULL;
//                }
//
//                data = spSlotData_create(i, Json_getString(slotMap, "name", 0), boneData);
//
//                color = Json_getString(slotMap, "color", 0);
//                if (color) {
//                    spColor_setFromFloats(&data->color,
//                                          toColor(color, 0),
//                                          toColor(color, 1),
//                                          toColor(color, 2),
//                                          toColor(color, 3));
//                }
//
//                dark = Json_getString(slotMap, "dark", 0);
//                if (dark) {
//                    data->darkColor = spColor_create();
//                    spColor_setFromFloats(data->darkColor,
//                                          toColor(dark, 0),
//                                          toColor(dark, 1),
//                                          toColor(dark, 2),
//                                          toColor(dark, 3));
//                }
//
//                item = Json::getItem(slotMap, "attachment");
//                if (item) spSlotData_setAttachmentName(data, item->_valueString);
//
//                item = Json::getItem(slotMap, "blend");
//                if (item) {
//                    if (strcmp(item->_valueString, "additive") == 0)
//                        data->blendMode = SP_BLEND_MODE_ADDITIVE;
//                    else if (strcmp(item->_valueString, "multiply") == 0)
//                        data->blendMode = SP_BLEND_MODE_MULTIPLY;
//                    else if (strcmp(item->_valueString, "screen") == 0)
//                        data->blendMode = SP_BLEND_MODE_SCREEN;
//                }
//
//                skeletonData->slots[i] = data;
//            }
//        }
//
//        /* IK constraints. */
//        ik = Json::getItem(root, "ik");
//        if (ik) {
//            Json *constraintMap;
//            skeletonData->ikConstraintsCount = ik->_size;
//            skeletonData->ikConstraints = MALLOC(spIkConstraintData*, ik->_size);
//            for (constraintMap = ik->_child, i = 0; constraintMap; constraintMap = constraintMap->_next, ++i) {
//                const char* targetName;
//
//                spIkConstraintData* data = spIkConstraintData_create(Json_getString(constraintMap, "name", 0));
//                data->order = Json_getInt(constraintMap, "order", 0);
//
//                boneMap = Json::getItem(constraintMap, "bones");
//                data->bonesCount = boneMap->_size;
//                data->bones = MALLOC(spBoneData*, boneMap->_size);
//                for (boneMap = boneMap->_child, ii = 0; boneMap; boneMap = boneMap->_next, ++ii) {
//                    data->bones[ii] = spSkeletonData_findBone(skeletonData, boneMap->_valueString);
//                    if (!data->bones[ii]) {
//                        spSkeletonData_dispose(skeletonData);
//                        setError(root, "IK bone not found: ", boneMap->_valueString);
//                        return NULL;
//                    }
//                }
//
//                targetName = Json_getString(constraintMap, "target", 0);
//                data->target = spSkeletonData_findBone(skeletonData, targetName);
//                if (!data->target) {
//                    spSkeletonData_dispose(skeletonData);
//                    setError(root, "Target bone not found: ", boneMap->_name);
//                    return NULL;
//                }
//
//                data->bendDirection = Json_getInt(constraintMap, "bendPositive", 1) ? 1 : -1;
//                data->mix = Json_getFloat(constraintMap, "mix", 1);
//
//                skeletonData->ikConstraints[i] = data;
//            }
//        }
//
//        /* Transform constraints. */
//        transform = Json::getItem(root, "transform");
//        if (transform) {
//            Json *constraintMap;
//            skeletonData->transformConstraintsCount = transform->_size;
//            skeletonData->transformConstraints = MALLOC(spTransformConstraintData*, transform->_size);
//            for (constraintMap = transform->_child, i = 0; constraintMap; constraintMap = constraintMap->_next, ++i) {
//                const char* name;
//
//                spTransformConstraintData* data = spTransformConstraintData_create(Json_getString(constraintMap, "name", 0));
//                data->order = Json_getInt(constraintMap, "order", 0);
//
//                boneMap = Json::getItem(constraintMap, "bones");
//                data->bonesCount = boneMap->_size;
//                CONST_CAST(spBoneData**, data->bones) = MALLOC(spBoneData*, boneMap->_size);
//                for (boneMap = boneMap->_child, ii = 0; boneMap; boneMap = boneMap->_next, ++ii) {
//                    data->bones[ii] = spSkeletonData_findBone(skeletonData, boneMap->_valueString);
//                    if (!data->bones[ii]) {
//                        spSkeletonData_dispose(skeletonData);
//                        setError(root, "Transform bone not found: ", boneMap->_valueString);
//                        return NULL;
//                    }
//                }
//
//                name = Json_getString(constraintMap, "target", 0);
//                data->target = spSkeletonData_findBone(skeletonData, name);
//                if (!data->target) {
//                    spSkeletonData_dispose(skeletonData);
//                    setError(root, "Target bone not found: ", boneMap->_name);
//                    return NULL;
//                }
//
//                data->local = Json_getInt(constraintMap, "local", 0);
//                data->relative = Json_getInt(constraintMap, "relative", 0);
//                data->offsetRotation = Json_getFloat(constraintMap, "rotation", 0);
//                data->offsetX = Json_getFloat(constraintMap, "x", 0) * _scale;
//                data->offsetY = Json_getFloat(constraintMap, "y", 0) * _scale;
//                data->offsetScaleX = Json_getFloat(constraintMap, "scaleX", 0);
//                data->offsetScaleY = Json_getFloat(constraintMap, "scaleY", 0);
//                data->offsetShearY = Json_getFloat(constraintMap, "shearY", 0);
//
//                data->rotateMix = Json_getFloat(constraintMap, "rotateMix", 1);
//                data->translateMix = Json_getFloat(constraintMap, "translateMix", 1);
//                data->scaleMix = Json_getFloat(constraintMap, "scaleMix", 1);
//                data->shearMix = Json_getFloat(constraintMap, "shearMix", 1);
//
//                skeletonData->transformConstraints[i] = data;
//            }
//        }
//
//        /* Path constraints */
//        path = Json::getItem(root, "path");
//        if (path) {
//            Json *constraintMap;
//            skeletonData->pathConstraintsCount = path->_size;
//            skeletonData->pathConstraints = MALLOC(spPathConstraintData*, path->_size);
//            for (constraintMap = path->_child, i = 0; constraintMap; constraintMap = constraintMap->_next, ++i) {
//                const char* name;
//                const char* item;
//
//                spPathConstraintData* data = spPathConstraintData_create(Json_getString(constraintMap, "name", 0));
//                data->order = Json_getInt(constraintMap, "order", 0);
//
//                boneMap = Json::getItem(constraintMap, "bones");
//                data->bonesCount = boneMap->_size;
//                CONST_CAST(spBoneData**, data->bones) = MALLOC(spBoneData*, boneMap->_size);
//                for (boneMap = boneMap->_child, ii = 0; boneMap; boneMap = boneMap->_next, ++ii) {
//                    data->bones[ii] = spSkeletonData_findBone(skeletonData, boneMap->_valueString);
//                    if (!data->bones[ii]) {
//                        spSkeletonData_dispose(skeletonData);
//                        setError(root, "Path bone not found: ", boneMap->_valueString);
//                        return NULL;
//                    }
//                }
//
//                name = Json_getString(constraintMap, "target", 0);
//                data->target = spSkeletonData_findSlot(skeletonData, name);
//                if (!data->target) {
//                    spSkeletonData_dispose(skeletonData);
//                    setError(root, "Target slot not found: ", boneMap->_name);
//                    return NULL;
//                }
//
//                item = Json_getString(constraintMap, "positionMode", "percent");
//                if (strcmp(item, "fixed") == 0) data->positionMode = SP_POSITION_MODE_FIXED;
//                else if (strcmp(item, "percent") == 0) data->positionMode = SP_POSITION_MODE_PERCENT;
//
//                item = Json_getString(constraintMap, "spacingMode", "length");
//                if (strcmp(item, "length") == 0) data->spacingMode = SP_SPACING_MODE_LENGTH;
//                else if (strcmp(item, "fixed") == 0) data->spacingMode = SP_SPACING_MODE_FIXED;
//                else if (strcmp(item, "percent") == 0) data->spacingMode = SP_SPACING_MODE_PERCENT;
//
//                item = Json_getString(constraintMap, "rotateMode", "tangent");
//                if (strcmp(item, "tangent") == 0) data->rotateMode = SP_ROTATE_MODE_TANGENT;
//                else if (strcmp(item, "chain") == 0) data->rotateMode = SP_ROTATE_MODE_CHAIN;
//                else if (strcmp(item, "chainScale") == 0) data->rotateMode = SP_ROTATE_MODE_CHAIN_SCALE;
//
//                data->offsetRotation = Json_getFloat(constraintMap, "rotation", 0);
//                data->position = Json_getFloat(constraintMap, "position", 0);
//                if (data->positionMode == SP_POSITION_MODE_FIXED) data->position *= _scale;
//                data->spacing = Json_getFloat(constraintMap, "spacing", 0);
//                if (data->spacingMode == SP_SPACING_MODE_LENGTH || data->spacingMode == SP_SPACING_MODE_FIXED) data->spacing *= _scale;
//                data->rotateMix = Json_getFloat(constraintMap, "rotateMix", 1);
//                data->translateMix = Json_getFloat(constraintMap, "translateMix", 1);
//
//                skeletonData->pathConstraints[i] = data;
//            }
//        }
//
//        /* Skins. */
//        skins = Json::getItem(root, "skins");
//        if (skins) {
//            Json *skinMap;
//            skeletonData->skins = MALLOC(spSkin*, skins->_size);
//            for (skinMap = skins->_child, i = 0; skinMap; skinMap = skinMap->_next, ++i) {
//                Json *attachmentsMap;
//                Json *curves;
//                spSkin *skin = spSkin_create(skinMap->_name);
//
//                skeletonData->skins[skeletonData->skinsCount++] = skin;
//                if (strcmp(skinMap->_name, "default") == 0) skeletonData->defaultSkin = skin;
//
//                for (attachmentsMap = skinMap->_child; attachmentsMap; attachmentsMap = attachmentsMap->_next) {
//                    int slotIndex = spSkeletonData_findSlotIndex(skeletonData, attachmentsMap->_name);
//                    Json *attachmentMap;
//
//                    for (attachmentMap = attachmentsMap->_child; attachmentMap; attachmentMap = attachmentMap->_next) {
//                        spAttachment* attachment;
//                        const char* skinAttachmentName = attachmentMap->_name;
//                        const char* attachmentName = Json_getString(attachmentMap, "name", skinAttachmentName);
//                        const char* attachmentPath = Json_getString(attachmentMap, "path", attachmentName);
//                        const char* color;
//                        Json* entry;
//
//                        const char* typeString = Json_getString(attachmentMap, "type", "region");
//                        spAttachmentType type;
//                        if (strcmp(typeString, "region") == 0)
//                            type = SP_ATTACHMENT_REGION;
//                        else if (strcmp(typeString, "mesh") == 0)
//                            type = SP_ATTACHMENT_MESH;
//                        else if (strcmp(typeString, "linkedmesh") == 0)
//                            type = SP_ATTACHMENT_LINKED_MESH;
//                        else if (strcmp(typeString, "boundingbox") == 0)
//                            type = SP_ATTACHMENT_BOUNDING_BOX;
//                        else if (strcmp(typeString, "path") == 0)
//                            type = SP_ATTACHMENT_PATH;
//                        else if    (strcmp(typeString, "clipping") == 0)
//                            type = SP_ATTACHMENT_CLIPPING;
//                        else {
//                            spSkeletonData_dispose(skeletonData);
//                            setError(root, "Unknown attachment type: ", typeString);
//                            return NULL;
//                        }
//
//                        attachment = spAttachmentLoader_createAttachment(_attachmentLoader, skin, type, attachmentName, attachmentPath);
//                        if (!attachment) {
//                            if (_attachmentLoader->error1) {
//                                spSkeletonData_dispose(skeletonData);
//                                setError(root, _attachmentLoader->error1, _attachmentLoader->error2);
//                                return NULL;
//                            }
//                            continue;
//                        }
//
//                        switch (attachment->_type) {
//                            case SP_ATTACHMENT_REGION: {
//                                spRegionAttachment* region = SUB_CAST(spRegionAttachment, attachment);
//                                if (path) MALLOC_STR(region->path, attachmentPath);
//                                region->x = Json_getFloat(attachmentMap, "x", 0) * _scale;
//                                region->y = Json_getFloat(attachmentMap, "y", 0) * _scale;
//                                region->scaleX = Json_getFloat(attachmentMap, "scaleX", 1);
//                                region->scaleY = Json_getFloat(attachmentMap, "scaleY", 1);
//                                region->rotation = Json_getFloat(attachmentMap, "rotation", 0);
//                                region->width = Json_getFloat(attachmentMap, "width", 32) * _scale;
//                                region->height = Json_getFloat(attachmentMap, "height", 32) * _scale;
//
//                                color = Json_getString(attachmentMap, "color", 0);
//                                if (color) {
//                                    spColor_setFromFloats(&region->color,
//                                                          toColor(color, 0),
//                                                          toColor(color, 1),
//                                                          toColor(color, 2),
//                                                          toColor(color, 3));
//                                }
//
//                                spRegionAttachment_updateOffset(region);
//
//                                spAttachmentLoader_configureAttachment(_attachmentLoader, attachment);
//                                break;
//                            }
//                            case SP_ATTACHMENT_MESH:
//                            case SP_ATTACHMENT_LINKED_MESH: {
//                                spMeshAttachment* mesh = SUB_CAST(spMeshAttachment, attachment);
//
//                                MALLOC_STR(mesh->path, attachmentPath);
//
//                                color = Json_getString(attachmentMap, "color", 0);
//                                if (color) {
//                                    spColor_setFromFloats(&mesh->color,
//                                                          toColor(color, 0),
//                                                          toColor(color, 1),
//                                                          toColor(color, 2),
//                                                          toColor(color, 3));
//                                }
//
//                                mesh->width = Json_getFloat(attachmentMap, "width", 32) * _scale;
//                                mesh->height = Json_getFloat(attachmentMap, "height", 32) * _scale;
//
//                                entry = Json::getItem(attachmentMap, "parent");
//                                if (!entry) {
//                                    int verticesLength;
//                                    entry = Json::getItem(attachmentMap, "triangles");
//                                    mesh->trianglesCount = entry->_size;
//                                    mesh->triangles = MALLOC(unsigned short, entry->_size);
//                                    for (entry = entry->_child, ii = 0; entry; entry = entry->_next, ++ii)
//                                        mesh->triangles[ii] = (unsigned short)entry->_valueInt;
//
//                                    entry = Json::getItem(attachmentMap, "uvs");
//                                    verticesLength = entry->_size;
//                                    mesh->regionUVs = MALLOC(float, verticesLength);
//                                    for (entry = entry->_child, ii = 0; entry; entry = entry->_next, ++ii)
//                                        mesh->regionUVs[ii] = entry->_valueFloat;
//
//                                    _readVertices(self, attachmentMap, SUPER(mesh), verticesLength);
//
//                                    spMeshAttachment_updateUVs(mesh);
//
//                                    mesh->hullLength = Json_getInt(attachmentMap, "hull", 0);
//
//                                    entry = Json::getItem(attachmentMap, "edges");
//                                    if (entry) {
//                                        mesh->edgesCount = entry->_size;
//                                        mesh->edges = MALLOC(int, entry->_size);
//                                        for (entry = entry->_child, ii = 0; entry; entry = entry->_next, ++ii)
//                                            mesh->edges[ii] = entry->_valueInt;
//                                    }
//
//                                    spAttachmentLoader_configureAttachment(_attachmentLoader, attachment);
//                                } else {
//                                    mesh->inheritDeform = Json_getInt(attachmentMap, "deform", 1);
//                                    _spSkeletonJson_addLinkedMesh(self, SUB_CAST(spMeshAttachment, attachment), Json_getString(attachmentMap, "skin", 0), slotIndex,
//                                                                  entry->_valueString);
//                                }
//                                break;
//                            }
//                            case SP_ATTACHMENT_BOUNDING_BOX: {
//                                spBoundingBoxAttachment* box = SUB_CAST(spBoundingBoxAttachment, attachment);
//                                int vertexCount = Json_getInt(attachmentMap, "vertexCount", 0) << 1;
//                                _readVertices(self, attachmentMap, SUPER(box), vertexCount);
//                                box->super.verticesCount = vertexCount;
//                                spAttachmentLoader_configureAttachment(_attachmentLoader, attachment);
//                                break;
//                            }
//                            case SP_ATTACHMENT_PATH: {
//                                spPathAttachment* pathAttatchment = SUB_CAST(spPathAttachment, attachment);
//                                int vertexCount = 0;
//                                pathAttatchment->closed = Json_getInt(attachmentMap, "closed", 0);
//                                pathAttatchment->constantSpeed = Json_getInt(attachmentMap, "constantSpeed", 1);
//                                vertexCount = Json_getInt(attachmentMap, "vertexCount", 0);
//                                _readVertices(self, attachmentMap, SUPER(pathAttatchment), vertexCount << 1);
//
//                                pathAttatchment->lengthsLength = vertexCount / 3;
//                                pathAttatchment->lengths = MALLOC(float, pathAttatchment->lengthsLength);
//
//                                curves = Json::getItem(attachmentMap, "lengths");
//                                for (curves = curves->_child, ii = 0; curves; curves = curves->_next, ++ii) {
//                                    pathAttatchment->lengths[ii] = curves->_valueFloat * _scale;
//                                }
//                                break;
//                            }
//                            case SP_ATTACHMENT_POINT: {
//                                spPointAttachment* point = SUB_CAST(spPointAttachment, attachment);
//                                point->x = Json_getFloat(attachmentMap, "x", 0) * _scale;
//                                point->y = Json_getFloat(attachmentMap, "y", 0) * _scale;
//                                point->rotation = Json_getFloat(attachmentMap, "rotation", 0);
//
//                                color = Json_getString(attachmentMap, "color", 0);
//                                if (color) {
//                                    spColor_setFromFloats(&point->color,
//                                                          toColor(color, 0),
//                                                          toColor(color, 1),
//                                                          toColor(color, 2),
//                                                          toColor(color, 3));
//                                }
//                                break;
//                            }
//                            case SP_ATTACHMENT_CLIPPING: {
//                                spClippingAttachment* clip = SUB_CAST(spClippingAttachment, attachment);
//                                int vertexCount = 0;
//                                const char* end = Json_getString(attachmentMap, "end", 0);
//                                if (end) {
//                                    spSlotData* slot = spSkeletonData_findSlot(skeletonData, end);
//                                    clip->endSlot = slot;
//                                }
//                                vertexCount = Json_getInt(attachmentMap, "vertexCount", 0) << 1;
//                                _readVertices(self, attachmentMap, SUPER(clip), vertexCount);
//                                spAttachmentLoader_configureAttachment(_attachmentLoader, attachment);
//                                break;
//                            }
//                        }
//
//                        spSkin_addAttachment(skin, slotIndex, skinAttachmentName, attachment);
//                    }
//                }
//            }
//        }
//
//        /* Linked meshes. */
//        for (i = 0; i < internal->linkedMeshCount; i++) {
//            spAttachment* parent;
//            _spLinkedMesh* linkedMesh = internal->linkedMeshes + i;
//            spSkin* skin = !linkedMesh->skin ? skeletonData->defaultSkin : spSkeletonData_findSkin(skeletonData, linkedMesh->skin);
//            if (!skin) {
//                spSkeletonData_dispose(skeletonData);
//                setError(root, "Skin not found: ", linkedMesh->skin);
//                return NULL;
//            }
//            parent = spSkin_getAttachment(skin, linkedMesh->slotIndex, linkedMesh->parent);
//            if (!parent) {
//                spSkeletonData_dispose(skeletonData);
//                setError(root, "Parent mesh not found: ", linkedMesh->parent);
//                return NULL;
//            }
//            spMeshAttachment_setParentMesh(linkedMesh->mesh, SUB_CAST(spMeshAttachment, parent));
//            spMeshAttachment_updateUVs(linkedMesh->mesh);
//            spAttachmentLoader_configureAttachment(_attachmentLoader, SUPER(SUPER(linkedMesh->mesh)));
//        }
//
//        /* Events. */
//        events = Json::getItem(root, "events");
//        if (events) {
//            Json *eventMap;
//            const char* stringValue;
//            skeletonData->eventsCount = events->_size;
//            skeletonData->events = MALLOC(spEventData*, events->_size);
//            for (eventMap = events->_child, i = 0; eventMap; eventMap = eventMap->_next, ++i) {
//                spEventData* eventData = spEventData_create(eventMap->_name);
//                eventData->intValue = Json_getInt(eventMap, "int", 0);
//                eventData->floatValue = Json_getFloat(eventMap, "float", 0);
//                stringValue = Json_getString(eventMap, "string", 0);
//                if (stringValue) MALLOC_STR(eventData->stringValue, stringValue);
//                skeletonData->events[i] = eventData;
//            }
//        }
//
//        /* Animations. */
//        animations = Json::getItem(root, "animations");
//        if (animations) {
//            Json *animationMap;
//            skeletonData->animations = MALLOC(spAnimation*, animations->_size);
//            for (animationMap = animations->_child; animationMap; animationMap = animationMap->_next) {
//                spAnimation* animation = _spSkeletonJson_readAnimation(self, animationMap, skeletonData);
//                if (!animation) {
//                    spSkeletonData_dispose(skeletonData);
//                    return NULL;
//                }
//                skeletonData->animations[skeletonData->animationsCount++] = animation;
//            }
//        }
//
//        Json_dispose(root);
        return skeletonData;
    }
    
    float SkeletonJson::toColor(const char* value, int index)
    {
        char digits[3];
        char *error;
        int color;

        if (index >= strlen(value) / 2)
        {
            return -1;
        }

        value += index * 2;

        digits[0] = *value;
        digits[1] = *(value + 1);
        digits[2] = '\0';
        color = (int)strtoul(digits, &error, 16);
        if (*error != 0)
        {
            return -1;
        }
        
        return color / (float)255;
    }
    
    void SkeletonJson::readCurve(Json* frame, CurveTimeline* timeline, int frameIndex)
    {
        Json* curve = Json::getItem(frame, "curve");
        if (!curve)
        {
            return;
        }
        if (curve->_type == Json::JSON_STRING && strcmp(curve->_valueString, "stepped") == 0)
        {
            timeline->setStepped(frameIndex);
        }
        else if (curve->_type == Json::JSON_ARRAY)
        {
            Json* child0 = curve->_child;
            Json* child1 = child0->_next;
            Json* child2 = child1->_next;
            Json* child3 = child2->_next;
            timeline->setCurve(frameIndex, child0->_valueFloat, child1->_valueFloat, child2->_valueFloat, child3->_valueFloat);
        }
    }
    
    Animation* SkeletonJson::readAnimation(Json* root, SkeletonData *skeletonData)
    {
        Vector<Timeline*> timelines;
        float scale = _scale;
        float duration = 0;
        
        int frameIndex;
        Json* valueMap;
        int timelinesCount = 0;

        Json* bones = Json::getItem(root, "bones");
        Json* slots = Json::getItem(root, "slots");
        Json* ik = Json::getItem(root, "ik");
        Json* transform = Json::getItem(root, "transform");
        Json* paths = Json::getItem(root, "paths");
        Json* deform = Json::getItem(root, "deform");
        Json* drawOrder = Json::getItem(root, "drawOrder");
        Json* events = Json::getItem(root, "events");
        Json *boneMap, *slotMap, *constraintMap;
        if (!drawOrder)
        {
            drawOrder = Json::getItem(root, "draworder");
        }

        for (boneMap = bones ? bones->_child : 0; boneMap; boneMap = boneMap->_next)
        {
            timelinesCount += boneMap->_size;
        }
        for (slotMap = slots ? slots->_child : 0; slotMap; slotMap = slotMap->_next)
        {
            timelinesCount += slotMap->_size;
        }
        timelinesCount += ik ? ik->_size : 0;
        timelinesCount += transform ? transform->_size : 0;
        for (constraintMap = paths ? paths->_child : 0; constraintMap; constraintMap = constraintMap->_next)
        {
            timelinesCount += constraintMap->_size;
        }
        for (constraintMap = deform ? deform->_child : 0; constraintMap; constraintMap = constraintMap->_next)
        {
            for (slotMap = constraintMap->_child; slotMap; slotMap = slotMap->_next)
            {
                timelinesCount += slotMap->_size;
            }
        }
        if (drawOrder)
        {
            ++timelinesCount;
        }
        if (events)
        {
            ++timelinesCount;
        }

        /* Slot timelines. */
        for (slotMap = slots ? slots->_child : 0; slotMap; slotMap = slotMap->_next)
        {
//            Json *timelineMap;
//
//            int slotIndex = skeletonData->findSlotIndex(slotMap->_name);
//            if (slotIndex == -1)
//            {
//                setError(root, "Slot not found: ", slotMap->_name);
//                return NULL;
//            }
//
//            for (timelineMap = slotMap->_child; timelineMap; timelineMap = timelineMap->_next)
//            {
//                if (strcmp(timelineMap->_name, "attachment") == 0)
//                {
//                    spAttachmentTimeline *timeline = spAttachmentTimeline_create(timelineMap->_size);
//                    timeline->slotIndex = slotIndex;
//
//                    for (valueMap = timelineMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//                    {
//                        Json* name = Json::getItem(valueMap, "name");
//                        spAttachmentTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), name->_type == Json_NULL ? 0 : name->_valueString);
//                    }
//                    timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//                    duration = MAX(duration, timeline->frames[timelineMap->_size - 1]);
//
//                }
//                else if (strcmp(timelineMap->_name, "color") == 0)
//                {
//                    spColorTimeline *timeline = spColorTimeline_create(timelineMap->_size);
//                    timeline->slotIndex = slotIndex;
//
//                    for (valueMap = timelineMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//                    {
//                        const char* s = Json_getString(valueMap, "color", 0);
//                        spColorTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), toColor(s, 0), toColor(s, 1), toColor(s, 2), toColor(s, 3));
//                        readCurve(valueMap, SUPER(timeline), frameIndex);
//                    }
//                    timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//                    duration = MAX(duration, timeline->frames[(timelineMap->_size - 1) * COLOR_ENTRIES]);
//
//                }
//                else if (strcmp(timelineMap->_name, "twoColor") == 0)
//                {
//                    spTwoColorTimeline *timeline = spTwoColorTimeline_create(timelineMap->_size);
//                    timeline->slotIndex = slotIndex;
//
//                    for (valueMap = timelineMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//                    {
//                        const char* s = Json_getString(valueMap, "light", 0);
//                        const char* ds = Json_getString(valueMap, "dark", 0);
//                        spTwoColorTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), toColor(s, 0), toColor(s, 1), toColor(s, 2),
//                                                    toColor(s, 3), toColor(ds, 0), toColor(ds, 1), toColor(ds, 2));
//                        readCurve(valueMap, SUPER(timeline), frameIndex);
//                    }
//                    timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//                    duration = MAX(duration, timeline->frames[(timelineMap->_size - 1) * TWOCOLOR_ENTRIES]);
//                }
//                else
//                {
//                    setError(root, "Invalid timeline type for a slot: ", timelineMap->_name);
//                    return NULL;
//                }
//            }
        }

        /* Bone timelines. */
        for (boneMap = bones ? bones->_child : 0; boneMap; boneMap = boneMap->_next)
        {
//            Json *timelineMap;
//
//            int boneIndex = spSkeletonData_findBoneIndex(skeletonData, boneMap->_name);
//            if (boneIndex == -1)
//            {
//                setError(root, "Bone not found: ", boneMap->_name);
//                return NULL;
//            }
//
//            for (timelineMap = boneMap->_child; timelineMap; timelineMap = timelineMap->_next)
//            {
//                if (strcmp(timelineMap->_name, "rotate") == 0)
//                {
//                    spRotateTimeline *timeline = spRotateTimeline_create(timelineMap->_size);
//                    timeline->boneIndex = boneIndex;
//
//                    for (valueMap = timelineMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//                    {
//                        spRotateTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), Json_getFloat(valueMap, "angle", 0));
//                        readCurve(valueMap, SUPER(timeline), frameIndex);
//                    }
//                    timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//                    duration = MAX(duration, timeline->frames[(timelineMap->_size - 1) * ROTATE_ENTRIES]);
//                }
//                else
//                {
//                    int isScale = strcmp(timelineMap->_name, "scale") == 0;
//                    int isTranslate = strcmp(timelineMap->_name, "translate") == 0;
//                    int isShear = strcmp(timelineMap->_name, "shear") == 0;
//                    if (isScale || isTranslate || isShear)
//                    {
//                        float timelineScale = isTranslate ? _scale: 1;
//                        spTranslateTimeline *timeline = 0;
//                        if (isScale)
//                        {
//                            timeline = spScaleTimeline_create(timelineMap->_size);
//                        }
//                        else if (isTranslate)
//                        {
//                            timeline = spTranslateTimeline_create(timelineMap->_size);
//                        }
//                        else if (isShear)
//                        {
//                            timeline = spShearTimeline_create(timelineMap->_size);
//                        }
//                        timeline->boneIndex = boneIndex;
//
//                        for (valueMap = timelineMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//                        {
//                            spTranslateTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), Json_getFloat(valueMap, "x", 0) * timelineScale, Json_getFloat(valueMap, "y", 0) * timelineScale);
//                            readCurve(valueMap, SUPER(timeline), frameIndex);
//                        }
//
//                        timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//                        duration = MAX(duration, timeline->frames[(timelineMap->_size - 1) * TRANSLATE_ENTRIES]);
//                    }
//                    else
//                    {
//                        setError(root, "Invalid timeline type for a bone: ", timelineMap->_name);
//                        return NULL;
//                    }
//                }
//            }
        }

        /* IK constraint timelines. */
        for (constraintMap = ik ? ik->_child : 0; constraintMap; constraintMap = constraintMap->_next)
        {
//            spIkConstraintData* constraint = spSkeletonData_findIkConstraint(skeletonData, constraintMap->_name);
//            spIkConstraintTimeline* timeline = spIkConstraintTimeline_create(constraintMap->_size);
//            for (frameIndex = 0; frameIndex < skeletonData->ikConstraintsCount; ++frameIndex)
//            {
//                if (constraint == skeletonData->ikConstraints[frameIndex])
//                {
//                    timeline->ikConstraintIndex = frameIndex;
//                    break;
//                }
//            }
//            for (valueMap = constraintMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//            {
//                spIkConstraintTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), Json_getFloat(valueMap, "mix", 1), Json_getInt(valueMap, "bendPositive", 1) ? 1 : -1);
//                readCurve(valueMap, SUPER(timeline), frameIndex);
//            }
//            timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//            duration = MAX(duration, timeline->frames[(constraintMap->_size - 1) * IKCONSTRAINT_ENTRIES]);
        }

        /* Transform constraint timelines. */
        for (constraintMap = transform ? transform->_child : 0; constraintMap; constraintMap = constraintMap->_next)
        {
//            spTransformConstraintData* constraint = spSkeletonData_findTransformConstraint(skeletonData, constraintMap->_name);
//            spTransformConstraintTimeline* timeline = spTransformConstraintTimeline_create(constraintMap->_size);
//            for (frameIndex = 0; frameIndex < skeletonData->transformConstraintsCount; ++frameIndex)
//            {
//                if (constraint == skeletonData->transformConstraints[frameIndex])
//                {
//                    timeline->transformConstraintIndex = frameIndex;
//                    break;
//                }
//            }
//            for (valueMap = constraintMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//            {
//                spTransformConstraintTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), Json_getFloat(valueMap, "rotateMix", 1), Json_getFloat(valueMap, "translateMix", 1), Json_getFloat(valueMap, "scaleMix", 1), Json_getFloat(valueMap, "shearMix", 1));
//                readCurve(valueMap, SUPER(timeline), frameIndex);
//            }
//            timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//            duration = MAX(duration, timeline->frames[(constraintMap->_size - 1) * TRANSFORMCONSTRAINT_ENTRIES]);
        }

        /** Path constraint timelines. */
        for (constraintMap = paths ? paths->_child : 0; constraintMap; constraintMap = constraintMap->_next)
        {
//            int constraintIndex, i;
//            Json* timelineMap;
//
//            spPathConstraintData* data = spSkeletonData_findPathConstraint(skeletonData, constraintMap->_name);
//            if (!data)
//            {
//                setError(root, "Path constraint not found: ", constraintMap->_name);
//                return NULL;
//            }
//
//            for (i = 0; i < skeletonData->pathConstraintsCount; i++)
//            {
//                if (skeletonData->pathConstraints[i] == data)
//                {
//                    constraintIndex = i;
//                    break;
//                }
//            }
//
//            for (timelineMap = constraintMap->_child; timelineMap; timelineMap = timelineMap->_next)
//            {
//                const char* timelineName = timelineMap->_name;
//                if (strcmp(timelineName, "position") == 0 || strcmp(timelineName, "spacing") == 0)
//                {
//                    spPathConstraintPositionTimeline* timeline;
//                    float timelineScale = 1;
//                    if (strcmp(timelineName, "spacing") == 0)
//                    {
//                        timeline = (spPathConstraintPositionTimeline*)spPathConstraintSpacingTimeline_create(timelineMap->_size);
//                        if (data->spacingMode == SP_SPACING_MODE_LENGTH || data->spacingMode == SP_SPACING_MODE_FIXED)
//                        {
//                            timelineScale = _scale;
//                        }
//                    }
//                    else
//                    {
//                        timeline = spPathConstraintPositionTimeline_create(timelineMap->_size);
//                        if (data->positionMode == SP_POSITION_MODE_FIXED)
//                        {
//                            timelineScale = _scale;
//                        }
//                    }
//
//                    timeline->pathConstraintIndex = constraintIndex;
//                    for (valueMap = timelineMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//                    {
//                        spPathConstraintPositionTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), Json_getFloat(valueMap, timelineName, 0) * timelineScale);
//                        readCurve(valueMap, SUPER(timeline), frameIndex);
//                    }
//                    timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//                    duration = MAX(duration, timeline->frames[(timelineMap->_size - 1) * PATHCONSTRAINTPOSITION_ENTRIES]);
//                }
//                else if (strcmp(timelineName, "mix") == 0)
//                {
//                    spPathConstraintMixTimeline* timeline = spPathConstraintMixTimeline_create(timelineMap->_size);
//                    timeline->pathConstraintIndex = constraintIndex;
//                    for (valueMap = timelineMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//                    {
//                        spPathConstraintMixTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), Json_getFloat(valueMap, "rotateMix", 1), Json_getFloat(valueMap, "translateMix", 1));
//                        readCurve(valueMap, SUPER(timeline), frameIndex);
//                    }
//                    timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//                    duration = MAX(duration, timeline->frames[(timelineMap->_size - 1) * PATHCONSTRAINTMIX_ENTRIES]);
//                }
//            }
        }

        /* Deform timelines. */
        for (constraintMap = deform ? deform->_child : 0; constraintMap; constraintMap = constraintMap->_next)
        {
//            spSkin* skin = spSkeletonData_findSkin(skeletonData, constraintMap->_name);
//            for (slotMap = constraintMap->_child; slotMap; slotMap = slotMap->_next)
//            {
//                int slotIndex = spSkeletonData_findSlotIndex(skeletonData, slotMap->_name);
//                Json* timelineMap;
//                for (timelineMap = slotMap->_child; timelineMap; timelineMap = timelineMap->_next)
//                {
//                    float* tempDeform;
//                    spDeformTimeline *timeline;
//                    int weighted, deformLength;
//
//                    spVertexAttachment* attachment = SUB_CAST(spVertexAttachment, spSkin_getAttachment(skin, slotIndex, timelineMap->_name));
//                    if (!attachment)
//                    {
//                        setError(root, "Attachment not found: ", timelineMap->_name);
//                        return NULL;
//                    }
//                    weighted = attachment->bones != 0;
//                    deformLength = weighted ? attachment->verticesCount / 3 * 2 : attachment->verticesCount;
//                    tempDeform = MALLOC(float, deformLength);
//
//                    timeline = spDeformTimeline_create(timelineMap->_size, deformLength);
//                    timeline->slotIndex = slotIndex;
//                    timeline->attachment = SUPER(attachment);
//
//                    for (valueMap = timelineMap->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//                    {
//                        Json* vertices = Json::getItem(valueMap, "vertices");
//                        float* deform2;
//                        if (!vertices)
//                        {
//                            if (weighted)
//                            {
//                                deform2 = tempDeform;
//                                memset(deform, 0, sizeof(float) * deformLength);
//                            }
//                            else
//                            {
//                                deform2 = attachment->vertices;
//                            }
//                        }
//                        else
//                        {
//                            int v, start = Json_getInt(valueMap, "offset", 0);
//                            Json* vertex;
//                            deform2 = tempDeform;
//                            memset(deform, 0, sizeof(float) * start);
//                            if (_scale == 1)
//                            {
//                                for (vertex = vertices->_child, v = start; vertex; vertex = vertex->_next, ++v)
//                                {
//                                    deform2[v] = vertex->_valueFloat;
//                                }
//                            }
//                            else
//                            {
//                                for (vertex = vertices->_child, v = start; vertex; vertex = vertex->_next, ++v)
//                                {
//                                    deform2[v] = vertex->_valueFloat * _scale;
//                                }
//                            }
//                            memset(deform + v, 0, sizeof(float) * (deformLength - v));
//                            if (!weighted)
//                            {
//                                float* verticesAttachment = attachment->vertices;
//                                for (v = 0; v < deformLength; ++v)
//                                {
//                                    deform2[v] += verticesAttachment[v];
//                                }
//                            }
//                        }
//                        spDeformTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), deform2);
//                        readCurve(valueMap, SUPER(timeline), frameIndex);
//                    }
//                    FREE(tempDeform);
//
//                    timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//                    duration = MAX(duration, timeline->frames[timelineMap->_size - 1]);
//                }
//            }
        }

        /* Draw order timeline. */
        if (drawOrder)
        {
//            spDrawOrderTimeline* timeline = spDrawOrderTimeline_create(drawOrder->_size, skeletonData->slotsCount);
//            for (valueMap = drawOrder->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//            {
//                int ii;
//                int* drawOrder2 = 0;
//                Json* offsets = Json::getItem(valueMap, "offsets");
//                if (offsets)
//                {
//                    Json* offsetMap;
//                    int* unchanged = MALLOC(int, skeletonData->slotsCount - offsets->_size);
//                    int originalIndex = 0, unchangedIndex = 0;
//
//                    drawOrder2 = MALLOC(int, skeletonData->slotsCount);
//                    for (ii = skeletonData->slotsCount - 1; ii >= 0; --ii)
//                    {
//                        drawOrder2[ii] = -1;
//                    }
//
//                    for (offsetMap = offsets->_child; offsetMap; offsetMap = offsetMap->_next)
//                    {
//                        int slotIndex = spSkeletonData_findSlotIndex(skeletonData, Json_getString(offsetMap, "slot", 0));
//                        if (slotIndex == -1)
//                        {
//                            setError(root, "Slot not found: ", Json_getString(offsetMap, "slot", 0));
//                            return NULL;
//                        }
//                        /* Collect unchanged items. */
//                        while (originalIndex != slotIndex)
//                        {
//                            unchanged[unchangedIndex++] = originalIndex++;
//                        }
//                        /* Set changed items. */
//                        drawOrder2[originalIndex + Json_getInt(offsetMap, "offset", 0)] = originalIndex;
//                        originalIndex++;
//                    }
//                    /* Collect remaining unchanged items. */
//                    while (originalIndex < skeletonData->slotsCount)
//                    {
//                        unchanged[unchangedIndex++] = originalIndex++;
//                    }
//                    /* Fill in unchanged items. */
//                    for (ii = skeletonData->slotsCount - 1; ii >= 0; ii--)
//                    {
//                        if (drawOrder2[ii] == -1)
//                        {
//                            drawOrder2[ii] = unchanged[--unchangedIndex];
//                        }
//                    }
//                    FREE(unchanged);
//                }
//                spDrawOrderTimeline_setFrame(timeline, frameIndex, Json_getFloat(valueMap, "time", 0), drawOrder2);
//                FREE(drawOrder2);
//            }
//            timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//            duration = MAX(duration, timeline->frames[drawOrder->_size - 1]);
        }

        /* Event timeline. */
        if (events)
        {
//            spEventTimeline* timeline = spEventTimeline_create(events->_size);
//            for (valueMap = events->_child, frameIndex = 0; valueMap; valueMap = valueMap->_next, ++frameIndex)
//            {
//                spEvent* event;
//                const char* stringValue;
//                spEventData* eventData = spSkeletonData_findEvent(skeletonData, Json_getString(valueMap, "name", 0));
//                if (!eventData)
//                {
//                    setError(root, "Event not found: ", Json_getString(valueMap, "name", 0));
//                    return NULL;
//                }
//                event = spEvent_create(Json_getFloat(valueMap, "time", 0), eventData);
//                event->intValue = Json_getInt(valueMap, "int", eventData->intValue);
//                event->floatValue = Json_getFloat(valueMap, "float", eventData->floatValue);
//                stringValue = Json_getString(valueMap, "string", eventData->stringValue);
//                if (stringValue)
//                {
//                    MALLOC_STR(event->stringValue, stringValue);
//                }
//                spEventTimeline_setFrame(timeline, frameIndex, event);
//            }
//            timelines[timelinesCount++] = SUPER_CAST(spTimeline, timeline);
//            duration = MAX(duration, timeline->frames[events->_size - 1]);
        }
        
        Animation* ret = NEW(Animation);
        new (ret) Animation(std::string(root->_name), timelines, duration);

        return ret;
    }
    
    void SkeletonJson::readVertices(Json* attachmentMap, VertexAttachment* attachment, int verticesLength)
    {
        Json* entry;
        int i, n, nn, entrySize;
        Vector<float> vertices;
        
        attachment->setWorldVerticesLength(verticesLength);

        entry = Json::getItem(attachmentMap, "vertices");
        entrySize = entry->_size;
        vertices.reserve(entrySize);
        for (entry = entry->_child, i = 0; entry; entry = entry->_next, ++i)
        {
            vertices[i] = entry->_valueFloat;
        }

        if (verticesLength == entrySize)
        {
            if (_scale != 1)
            {
                for (i = 0; i < entrySize; ++i)
                {
                    vertices[i] *= _scale;
                }
            }
            
            attachment->setVertices(vertices);
            return;
        }

        Vertices bonesAndWeights;
        bonesAndWeights._bones.reserve(verticesLength * 3);
        bonesAndWeights._vertices.reserve(verticesLength * 3 * 3);

        for (i = 0, n = entrySize; i < n;)
        {
            int boneCount = (int)vertices[i++];
            bonesAndWeights._bones.push_back(boneCount);
            for (nn = i + boneCount * 4; i < nn; i += 4)
            {
                bonesAndWeights._bones.push_back((int)vertices[i]);
                bonesAndWeights._vertices.push_back(vertices[i + 1] * _scale);
                bonesAndWeights._vertices.push_back(vertices[i + 2] * _scale);
                bonesAndWeights._vertices.push_back(vertices[i + 3]);
            }
        }

        attachment->setVertices(bonesAndWeights._vertices);
        attachment->setBones(bonesAndWeights._bones);
    }
    
    void SkeletonJson::setError(Json* root, const char* value1, const char* value2)
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
        
        if (root)
        {
            DESTROY(Json, root);
        }
    }
}
