/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/SkeletonJson.h>

#include <spine/Atlas.h>
#include <spine/AtlasAttachmentLoader.h>
#include <spine/CurveTimeline.h>
#include <spine/Json.h>
#include <spine/LinkedMesh.h>
#include <spine/SkeletonData.h>
#include <spine/Attachment.h>
#include <spine/Skin.h>
#include <spine/VertexAttachment.h>

#include <spine/AttachmentTimeline.h>
#include <spine/AttachmentType.h>
#include <spine/BoneData.h>
#include <spine/BoundingBoxAttachment.h>
#include <spine/ClippingAttachment.h>
#include <spine/ColorTimeline.h>
#include <spine/ArrayUtils.h>
#include <spine/DeformTimeline.h>
#include <spine/DrawOrderFolderTimeline.h>
#include <spine/DrawOrderTimeline.h>
#include <spine/Event.h>
#include <spine/EventData.h>
#include <spine/EventTimeline.h>
#include <spine/IkConstraintData.h>
#include <spine/IkConstraintTimeline.h>
#include <spine/Inherit.h>
#include <spine/InheritTimeline.h>
#include <spine/MeshAttachment.h>
#include <spine/PathAttachment.h>
#include <spine/PathConstraintData.h>
#include <spine/PathConstraintMixTimeline.h>
#include <spine/PathConstraintPositionTimeline.h>
#include <spine/PathConstraintSpacingTimeline.h>
#include <spine/PhysicsConstraintData.h>
#include <spine/PhysicsConstraintTimeline.h>
#include <spine/PointAttachment.h>
#include <spine/RegionAttachment.h>
#include <spine/RotateTimeline.h>
#include <spine/ScaleTimeline.h>
#include <spine/ShearTimeline.h>
#include <spine/Skin.h>
#include <spine/SlotData.h>
#include <spine/TransformConstraintData.h>
#include <spine/TransformConstraintTimeline.h>
#include <spine/TranslateTimeline.h>
#include <spine/SequenceTimeline.h>
#include <spine/Version.h>
#include <spine/SliderData.h>
#include <spine/SliderPose.h>
#include <spine/SliderTimeline.h>
#include <spine/BonePose.h>
#include <spine/PathConstraintPose.h>
#include <spine/SliderMixTimeline.h>

using namespace spine;

class SP_API Vertices : public SpineObject {
public:
	Array<int> _bones;
	Array<float> _weights;
};

#define SKELETON_JSON_ERROR(root, message, value)                                                                                                    \
	do {                                                                                                                                             \
		delete skeletonData;                                                                                                                         \
		setError(root, message, value);                                                                                                              \
		return NULL;                                                                                                                                 \
	} while (0)

static FromProperty *fromProperty(const char *type) {
	if (strcmp(type, "rotate") == 0)
		return new FromRotate();
	else if (strcmp(type, "x") == 0)
		return new FromX();
	else if (strcmp(type, "y") == 0)
		return new FromY();
	else if (strcmp(type, "scaleX") == 0)
		return new FromScaleX();
	else if (strcmp(type, "scaleY") == 0)
		return new FromScaleY();
	else if (strcmp(type, "shearY") == 0)
		return new FromShearY();
	else
		return NULL;
}

static float propertyScale(const char *type, float scale) {
	if (strcmp(type, "x") == 0 || strcmp(type, "y") == 0)
		return scale;
	else
		return 1;
}

SkeletonJson::SkeletonJson(Atlas &atlas) : _attachmentLoader(new(__FILE__, __LINE__) AtlasAttachmentLoader(atlas)), _scale(1), _ownsLoader(true) {
}

SkeletonJson::SkeletonJson(AttachmentLoader &attachmentLoader, bool ownsLoader)
	: _attachmentLoader(&attachmentLoader), _scale(1), _ownsLoader(ownsLoader) {
}

SkeletonJson::~SkeletonJson() {
	ArrayUtils::deleteElements(_linkedMeshes);

	if (_ownsLoader) delete _attachmentLoader;
}

SkeletonData *SkeletonJson::readSkeletonDataFile(const String &path) {
	int length;
	SkeletonData *skeletonData;
	const char *json = SpineExtension::readFile(path, &length);
	if (length == 0 || !json) {
		setError(NULL, "Unable to read skeleton file: ", path);
		return NULL;
	}

	skeletonData = readSkeletonData(json);
	SpineExtension::free(json, __FILE__, __LINE__);

	if (skeletonData) {
		// Extract filename without extension from path
		int lastSlash = path.lastIndexOf('/');
		int lastBackslash = path.lastIndexOf('\\');
		int start = 0;
		if (lastSlash != -1) start = lastSlash + 1;
		if (lastBackslash != -1 && lastBackslash > start) start = lastBackslash + 1;

		int lastDot = path.lastIndexOf('.');
		if (lastDot != -1 && lastDot > start) {
			skeletonData->_name = path.substring(start, lastDot - start);
		} else {
			skeletonData->_name = path.substring(start);
		}
	}

	return skeletonData;
}

SkeletonData *SkeletonJson::readSkeletonData(const char *json) {
	SkeletonData *skeletonData;
	Json *root, *skeleton, *bones, *constraints, *slots, *skins, *animations, *events;

	_error = "";
	_linkedMeshes.clear();

	root = new (__FILE__, __LINE__) Json(json);

	if (!root) {
		setError(NULL, "Invalid skeleton JSON: ", Json::getError());
		return NULL;
	}

	skeletonData = new (__FILE__, __LINE__) SkeletonData();

	skeleton = Json::getItem(root, "skeleton");
	if (skeleton) {
		skeletonData->_hash = Json::getString(skeleton, "hash", 0);
		skeletonData->_version = Json::getString(skeleton, "spine", 0);
		if (!skeletonData->_version.startsWith(SPINE_VERSION_STRING)) {
			char errorMsg[255];
			snprintf(errorMsg, 255, "Skeleton version %s does not match runtime version %s", skeletonData->_version.buffer(), SPINE_VERSION_STRING);
			SKELETON_JSON_ERROR(NULL, errorMsg, "");
		}
		skeletonData->_x = Json::getFloat(skeleton, "x", 0);
		skeletonData->_y = Json::getFloat(skeleton, "y", 0);
		skeletonData->_width = Json::getFloat(skeleton, "width", 0);
		skeletonData->_height = Json::getFloat(skeleton, "height", 0);
		skeletonData->_referenceScale = Json::getFloat(skeleton, "referenceScale", 100) * _scale;
		skeletonData->_fps = Json::getFloat(skeleton, "fps", 30);
		skeletonData->_audioPath = Json::getString(skeleton, "audio", 0);
		skeletonData->_imagesPath = Json::getString(skeleton, "images", 0);
	}

	/* Bones. */
	bones = Json::getItem(root, "bones");
	if (bones) {
		skeletonData->_bones.setSize(bones->_size, 0);
		Json *boneMap = bones->_child;
		for (int bonesCount = 0; boneMap; boneMap = boneMap->_next, ++bonesCount) {
			BoneData *parent = 0;
			const char *parentName = Json::getString(boneMap, "parent", 0);
			if (parentName) {
				parent = skeletonData->findBone(parentName);
				if (!parent) SKELETON_JSON_ERROR(root, "Parent bone not found: ", parentName);
			}
			BoneData *data = new (__FILE__, __LINE__) BoneData(bonesCount, Json::getString(boneMap, "name", 0), parent);
			data->_length = Json::getFloat(boneMap, "length", 0) * _scale;
			BonePose &setup = data->_setupPose;
			setup._x = Json::getFloat(boneMap, "x", 0) * _scale;
			setup._y = Json::getFloat(boneMap, "y", 0) * _scale;
			setup._rotation = Json::getFloat(boneMap, "rotation", 0);
			setup._scaleX = Json::getFloat(boneMap, "scaleX", 1);
			setup._scaleY = Json::getFloat(boneMap, "scaleY", 1);
			setup._shearX = Json::getFloat(boneMap, "shearX", 0);
			setup._shearY = Json::getFloat(boneMap, "shearY", 0);
			setup._inherit = Inherit_valueOf(Json::getString(boneMap, "inherit", "normal"));
			data->_skinRequired = Json::getBoolean(boneMap, "skin", false);

			const char *color = Json::getString(boneMap, "color", NULL);
			if (color) Color::valueOf(color, data->getColor());

			data->_icon = Json::getString(boneMap, "icon", "");
			data->_iconSize = Json::getFloat(boneMap, "iconSize", 1);
			data->_iconRotation = Json::getFloat(boneMap, "iconRotation", 0);
			data->_visible = Json::getBoolean(boneMap, "visible", true);

			skeletonData->_bones[bonesCount] = data;
		}
	}

	/* Slots. */
	slots = Json::getItem(root, "slots");
	if (slots) {
		skeletonData->_slots.setSize(slots->_size, 0);
		Json *slotMap = slots->_child;
		for (int slotCount = 0; slotMap; slotMap = slotMap->_next, ++slotCount) {
			String slotName = String(Json::getString(slotMap, "name", 0));
			const char *boneName = Json::getString(slotMap, "bone", 0);
			BoneData *boneData = skeletonData->findBone(boneName);
			if (!boneData) SKELETON_JSON_ERROR(root, "Slot bone not found: ", boneName);

			SlotData *data = new (__FILE__, __LINE__) SlotData(slotCount, slotName, *boneData);

			const char *color = Json::getString(slotMap, "color", 0);
			if (color) Color::valueOf(color, data->_setupPose.getColor());

			const char *dark = Json::getString(slotMap, "dark", 0);
			if (dark) {
				data->_setupPose._darkColor = Color::valueOf(dark);
				data->_setupPose._hasDarkColor = true;
			}

			data->setAttachmentName(Json::getString(slotMap, "attachment", NULL));
			data->_blendMode = BlendMode_valueOf(Json::getString(slotMap, "blend", "normal"));
			data->_visible = Json::getBoolean(slotMap, "visible", true);
			skeletonData->_slots[slotCount] = data;
		}
	}

	/* Constraints. */
	constraints = Json::getItem(root, "constraints");
	if (constraints) {
		for (Json *constraintMap = constraints->_child; constraintMap; constraintMap = constraintMap->_next) {
			const char *name = Json::getString(constraintMap, "name", 0);
			bool skinRequired = Json::getBoolean(constraintMap, "skin", false);
			const char *type = Json::getString(constraintMap, "type", 0);
			if (strcmp(type, "ik") == 0) {
				IkConstraintData *data = new (__FILE__, __LINE__) IkConstraintData(name);
				data->setSkinRequired(skinRequired);

				Json *entry = Json::getItem(constraintMap, "bones");
				data->_bones.setSize(entry->_size, 0);
				entry = entry->_child;
				for (int boneCount = 0; entry; entry = entry->_next, ++boneCount) {
					data->_bones[boneCount] = skeletonData->findBone(entry->_valueString);
					if (!data->_bones[boneCount]) SKELETON_JSON_ERROR(root, "IK bone not found: ", entry->_valueString);
				}

				const char *targetName = Json::getString(constraintMap, "target", 0);
				data->_target = skeletonData->findBone(targetName);
				if (!data->_target) SKELETON_JSON_ERROR(root, "IK target bone not found: ", targetName);

				const char *scaleY = Json::getString(constraintMap, "scaleY", NULL);
				if (scaleY) data->_scaleYMode = ScaleYMode_valueOf(scaleY);
				IkConstraintPose &setup = data->_setupPose;
				setup._mix = Json::getFloat(constraintMap, "mix", 1);
				setup._softness = Json::getFloat(constraintMap, "softness", 0) * _scale;
				setup._bendDirection = Json::getBoolean(constraintMap, "bendPositive", true) ? 1 : -1;
				setup._compress = Json::getBoolean(constraintMap, "compress", false);
				setup._stretch = Json::getBoolean(constraintMap, "stretch", false);

				skeletonData->_constraints.add(data);
			} else if (strcmp(type, "transform") == 0) {
				TransformConstraintData *data = new (__FILE__, __LINE__) TransformConstraintData(name);
				data->setSkinRequired(skinRequired);

				Json *entry = Json::getItem(constraintMap, "bones");
				data->_bones.setSize(entry->_size, 0);
				entry = entry->_child;
				for (int boneCount = 0; entry; entry = entry->_next, ++boneCount) {
					data->_bones[boneCount] = skeletonData->findBone(entry->_valueString);
					if (!data->_bones[boneCount]) SKELETON_JSON_ERROR(root, "Transform constraint bone not found: ", entry->_valueString);
				}

				const char *sourceName = Json::getString(constraintMap, "source", 0);
				data->_source = skeletonData->findBone(sourceName);
				if (!data->_source) SKELETON_JSON_ERROR(root, "Transform constraint source bone not found: ", sourceName);

				data->_localSource = Json::getBoolean(constraintMap, "localSource", false);
				data->_localTarget = Json::getBoolean(constraintMap, "localTarget", false);
				data->_additive = Json::getBoolean(constraintMap, "additive", false);
				data->_clamp = Json::getBoolean(constraintMap, "clamp", false);

				bool rotate = false, x = false, y = false, scaleX = false, scaleY = false, shearY = false;
				Json *properties = Json::getItem(constraintMap, "properties");
				if (properties) {
					for (Json *fromEntry = properties->_child; fromEntry; fromEntry = fromEntry->_next) {
						FromProperty *from = fromProperty(fromEntry->_name);
						if (!from) SKELETON_JSON_ERROR(root, "Invalid transform constraint from property: ", fromEntry->_name);
						float fromScale = propertyScale(fromEntry->_name, _scale);
						from->_offset = Json::getFloat(fromEntry, "offset", 0) * fromScale;
						Json *toEntry = Json::getItem(fromEntry, "to");
						if (toEntry) {
							for (toEntry = toEntry->_child; toEntry; toEntry = toEntry->_next) {
								ToProperty *to = NULL;
								float toScale = 1;

								if (strcmp(toEntry->_name, "rotate") == 0) {
									rotate = true;
									to = new ToRotate();
								} else if (strcmp(toEntry->_name, "x") == 0) {
									x = true;
									to = new ToX();
									toScale = _scale;
								} else if (strcmp(toEntry->_name, "y") == 0) {
									y = true;
									to = new ToY();
									toScale = _scale;
								} else if (strcmp(toEntry->_name, "scaleX") == 0) {
									scaleX = true;
									to = new ToScaleX();
								} else if (strcmp(toEntry->_name, "scaleY") == 0) {
									scaleY = true;
									to = new ToScaleY();
								} else if (strcmp(toEntry->_name, "shearY") == 0) {
									shearY = true;
									to = new ToShearY();
								} else {
									SKELETON_JSON_ERROR(root, "Invalid transform constraint to property: ", toEntry->_name);
								}

								to->_offset = Json::getFloat(toEntry, "offset", 0) * toScale;
								to->_max = Json::getFloat(toEntry, "max", 1) * toScale;
								to->_scale = Json::getFloat(toEntry, "scale", 1) * toScale / fromScale;
								from->_to.add(to);
							}
						}

						if (from->_to.size() > 0)
							data->_properties.add(from);
						else
							delete from;
					}
				}

				data->_offsets[TransformConstraintData::ROTATION] = Json::getFloat(constraintMap, "rotation", 0);
				data->_offsets[TransformConstraintData::X] = Json::getFloat(constraintMap, "x", 0) * _scale;
				data->_offsets[TransformConstraintData::Y] = Json::getFloat(constraintMap, "y", 0) * _scale;
				data->_offsets[TransformConstraintData::SCALEX] = Json::getFloat(constraintMap, "scaleX", 0);
				data->_offsets[TransformConstraintData::SCALEY] = Json::getFloat(constraintMap, "scaleY", 0);
				data->_offsets[TransformConstraintData::SHEARY] = Json::getFloat(constraintMap, "shearY", 0);

				TransformConstraintPose &setup = data->_setupPose;
				if (rotate) setup._mixRotate = Json::getFloat(constraintMap, "mixRotate", 1);
				if (x) setup._mixX = Json::getFloat(constraintMap, "mixX", 1);
				if (y) setup._mixY = Json::getFloat(constraintMap, "mixY", setup._mixX);
				if (scaleX) setup._mixScaleX = Json::getFloat(constraintMap, "mixScaleX", 1);
				if (scaleY) setup._mixScaleY = Json::getFloat(constraintMap, "mixScaleY", setup._mixScaleX);
				if (shearY) setup._mixShearY = Json::getFloat(constraintMap, "mixShearY", 1);

				skeletonData->_constraints.add(data);
			} else if (strcmp(type, "path") == 0) {
				PathConstraintData *data = new (__FILE__, __LINE__) PathConstraintData(name);
				data->setSkinRequired(skinRequired);

				Json *entry = Json::getItem(constraintMap, "bones");
				data->_bones.setSize(entry->_size, 0);
				entry = entry->_child;
				for (int boneCount = 0; entry; entry = entry->_next, ++boneCount) {
					data->_bones[boneCount] = skeletonData->findBone(entry->_valueString);
					if (!data->_bones[boneCount]) SKELETON_JSON_ERROR(root, "Path bone not found: ", entry->_valueString);
				}

				const char *slotName = Json::getString(constraintMap, "slot", 0);
				data->_slot = skeletonData->findSlot(slotName);
				if (!data->_slot) SKELETON_JSON_ERROR(root, "Path slot not found: ", slotName);

				data->_positionMode = PositionMode_valueOf(Json::getString(constraintMap, "positionMode", "percent"));
				data->_spacingMode = SpacingMode_valueOf(Json::getString(constraintMap, "spacingMode", "length"));
				data->_rotateMode = RotateMode_valueOf(Json::getString(constraintMap, "rotateMode", "tangent"));
				data->_offsetRotation = Json::getFloat(constraintMap, "rotation", 0);
				PathConstraintPose &setup = data->_setupPose;
				setup._position = Json::getFloat(constraintMap, "position", 0);
				if (data->_positionMode == PositionMode_Fixed) setup._position *= _scale;
				setup._spacing = Json::getFloat(constraintMap, "spacing", 0);
				if (data->_spacingMode == SpacingMode_Length || data->_spacingMode == SpacingMode_Fixed) setup._spacing *= _scale;
				setup._mixRotate = Json::getFloat(constraintMap, "mixRotate", 1);
				setup._mixX = Json::getFloat(constraintMap, "mixX", 1);
				setup._mixY = Json::getFloat(constraintMap, "mixY", setup._mixX);

				skeletonData->_constraints.add(data);
			} else if (strcmp(type, "physics") == 0) {
				PhysicsConstraintData *data = new (__FILE__, __LINE__) PhysicsConstraintData(name);
				data->setSkinRequired(skinRequired);

				const char *boneName = Json::getString(constraintMap, "bone", 0);
				data->_bone = skeletonData->findBone(boneName);
				if (!data->_bone) SKELETON_JSON_ERROR(root, "Physics bone not found: ", boneName);

				data->_x = Json::getFloat(constraintMap, "x", 0);
				data->_y = Json::getFloat(constraintMap, "y", 0);
				data->_rotate = Json::getFloat(constraintMap, "rotate", 0);
				data->_scaleX = Json::getFloat(constraintMap, "scaleX", 0);
				const char *scaleY = Json::getString(constraintMap, "scaleY", NULL);
				if (scaleY) data->_scaleYMode = ScaleYMode_valueOf(scaleY);
				data->_shearX = Json::getFloat(constraintMap, "shearX", 0);
				data->_limit = Json::getFloat(constraintMap, "limit", 5000) * _scale;
				data->_step = 1.0f / Json::getInt(constraintMap, "fps", 60);
				PhysicsConstraintPose &setup = data->_setupPose;
				setup._inertia = Json::getFloat(constraintMap, "inertia", 0.5f);
				setup._strength = Json::getFloat(constraintMap, "strength", 100);
				setup._damping = Json::getFloat(constraintMap, "damping", 0.85f);
				setup._massInverse = 1.0f / Json::getFloat(constraintMap, "mass", 1);
				setup._wind = Json::getFloat(constraintMap, "wind", 0);
				setup._gravity = Json::getFloat(constraintMap, "gravity", 0);
				setup._mix = Json::getFloat(constraintMap, "mix", 1);
				data->_inertiaGlobal = Json::getBoolean(constraintMap, "inertiaGlobal", false);
				data->_strengthGlobal = Json::getBoolean(constraintMap, "strengthGlobal", false);
				data->_dampingGlobal = Json::getBoolean(constraintMap, "dampingGlobal", false);
				data->_massGlobal = Json::getBoolean(constraintMap, "massGlobal", false);
				data->_windGlobal = Json::getBoolean(constraintMap, "windGlobal", false);
				data->_gravityGlobal = Json::getBoolean(constraintMap, "gravityGlobal", false);
				data->_mixGlobal = Json::getBoolean(constraintMap, "mixGlobal", false);

				skeletonData->_constraints.add(data);
			} else if (strcmp(type, "slider") == 0) {
				SliderData *data = new (__FILE__, __LINE__) SliderData(name);
				data->setSkinRequired(skinRequired);
				data->_additive = Json::getBoolean(constraintMap, "additive", false);
				data->_loop = Json::getBoolean(constraintMap, "loop", false);
				data->_setupPose._mix = Json::getFloat(constraintMap, "mix", 1);

				const char *boneName = Json::getString(constraintMap, "bone", NULL);
				if (boneName != NULL) {
					data->_bone = skeletonData->findBone(boneName);
					if (!data->_bone) SKELETON_JSON_ERROR(root, "Slider bone not found: ", boneName);
					const char *property = Json::getString(constraintMap, "property", 0);
					data->_property = fromProperty(property);
					if (data->_property) {
						float propertyScaleValue = propertyScale(property, _scale);
						data->_property->_offset = Json::getFloat(constraintMap, "from", 0) * propertyScaleValue;
						data->_offset = Json::getFloat(constraintMap, "to", 0);
						data->_scale = Json::getFloat(constraintMap, "scale", 1) / propertyScaleValue;
						data->_max = Json::getFloat(constraintMap, "max", 0);
						data->_local = Json::getBoolean(constraintMap, "local", false);
					}
				} else
					data->_setupPose._time = Json::getFloat(constraintMap, "time", 0);

				skeletonData->_constraints.add(data);
			}
		}
	}

	/* Skins. */
	skins = Json::getItem(root, "skins");
	if (skins) {
		for (Json *skinMap = skins->_child; skinMap; skinMap = skinMap->_next) {
			Skin *skin = new (__FILE__, __LINE__) Skin(Json::getString(skinMap, "name", ""));
			Json *item = Json::getItem(skinMap, "bones");
			if (item) {
				for (Json *entry = item->_child; entry; entry = entry->_next) {
					BoneData *data = skeletonData->findBone(entry->_valueString);
					if (!data) SKELETON_JSON_ERROR(root, "Skin bone not found: ", entry->_valueString);
					skin->getBones().add(data);
				}
			}
			item = Json::getItem(skinMap, "ik");
			if (item) {
				for (Json *entry = item->_child; entry; entry = entry->_next) {
					IkConstraintData *data = skeletonData->findConstraint<IkConstraintData>(entry->_valueString);
					if (!data) SKELETON_JSON_ERROR(root, "Skin IK constraint not found: ", entry->_valueString);
					skin->getConstraints().add(data);
				}
			}
			item = Json::getItem(skinMap, "transform");
			if (item) {
				for (Json *entry = item->_child; entry; entry = entry->_next) {
					TransformConstraintData *data = skeletonData->findConstraint<TransformConstraintData>(entry->_valueString);
					if (!data) SKELETON_JSON_ERROR(root, "Skin transform constraint not found: ", entry->_valueString);
					skin->getConstraints().add(data);
				}
			}
			item = Json::getItem(skinMap, "path");
			if (item) {
				for (Json *entry = item->_child; entry; entry = entry->_next) {
					PathConstraintData *data = skeletonData->findConstraint<PathConstraintData>(entry->_valueString);
					if (!data) SKELETON_JSON_ERROR(root, "Skin path constraint not found: ", entry->_valueString);
					skin->getConstraints().add(data);
				}
			}
			item = Json::getItem(skinMap, "physics");
			if (item) {
				for (Json *entry = item->_child; entry; entry = entry->_next) {
					PhysicsConstraintData *data = skeletonData->findConstraint<PhysicsConstraintData>(entry->_valueString);
					if (!data) SKELETON_JSON_ERROR(root, "Skin physics constraint not found: ", entry->_valueString);
					skin->getConstraints().add(data);
				}
			}
			item = Json::getItem(skinMap, "slider");
			if (item) {
				for (Json *entry = item->_child; entry; entry = entry->_next) {
					SliderData *data = skeletonData->findConstraint<SliderData>(entry->_valueString);
					if (!data) SKELETON_JSON_ERROR(root, "Skin slider not found: ", entry->_valueString);
					skin->getConstraints().add(data);
				}
			}

			Json *attachments = Json::getItem(skinMap, "attachments");
			if (attachments)
				for (Json *slotEntry = attachments->_child; slotEntry; slotEntry = slotEntry->_next) {
					SlotData *slot = skeletonData->findSlot(slotEntry->_name);
					if (!slot) SKELETON_JSON_ERROR(root, "Skin slot not found: ", slotEntry->_name);
					for (Json *entry = slotEntry->_child; entry; entry = entry->_next) {
						Attachment *attachment = readAttachment(entry, skin, slot->getIndex(), entry->_name, skeletonData);
						if (attachment)
							skin->setAttachment(slot->getIndex(), entry->_name, attachment);
						else
							SKELETON_JSON_ERROR(root, "Error reading attachment: ", entry->_name);
					}
				}

			const char *color = Json::getString(skinMap, "color", NULL);
			if (color) Color::valueOf(color, skin->getColor());

			skeletonData->_skins.add(skin);
			if (strcmp(skin->getName().buffer(), "default") == 0) skeletonData->_defaultSkin = skin;
		}
	}

	/* Linked meshes. */
	int n = (int) _linkedMeshes.size();
	for (int i = 0; i < n; ++i) {
		LinkedMesh *linkedMesh = _linkedMeshes[i];
		Skin *skin = linkedMesh->_skin.length() == 0 ? skeletonData->getDefaultSkin() : skeletonData->findSkin(linkedMesh->_skin);
		if (skin == NULL) SKELETON_JSON_ERROR(root, "Skin not found: ", linkedMesh->_skin.buffer());
		Attachment *source = skin->getAttachment(linkedMesh->_sourceIndex, linkedMesh->_source);
		if (source == NULL) SKELETON_JSON_ERROR(root, "Source mesh not found: ", linkedMesh->_source.buffer());
		linkedMesh->_mesh->setTimelineAttachment(linkedMesh->_inheritTimelines ? source : linkedMesh->_mesh);
		linkedMesh->_mesh->setSourceMesh(static_cast<MeshAttachment *>(source));
		linkedMesh->_mesh->updateSequence();
		if (linkedMesh->_inheritTimelines && linkedMesh->_slotIndex != linkedMesh->_sourceIndex) {
			Array<int> &timelineSlots = source->getTimelineSlots();
			bool found = false;
			for (size_t ii = 0; ii < timelineSlots.size(); ++ii) {
				if ((size_t) timelineSlots[ii] == linkedMesh->_slotIndex) {
					found = true;
					break;
				}
			}
			if (!found) timelineSlots.add((int) linkedMesh->_slotIndex);
		}
	}
	ArrayUtils::deleteElements(_linkedMeshes);
	_linkedMeshes.clear();

	/* Events. */
	events = Json::getItem(root, "events");
	if (events) {
		skeletonData->_events.setSize(events->_size, 0);
		int eventIndex = 0;
		for (Json *eventMap = events->_child; eventMap; eventMap = eventMap->_next) {
			EventData *eventData = new (__FILE__, __LINE__) EventData(String(eventMap->_name));
			Event &setup = eventData->_setupPose;
			setup._intValue = Json::getInt(eventMap, "int", 0);
			setup._floatValue = Json::getFloat(eventMap, "float", 0);
			setup._stringValue = Json::getString(eventMap, "string", 0);
			eventData->_audioPath = Json::getString(eventMap, "audio", 0);
			if (eventData->_audioPath != NULL) {
				setup._volume = Json::getFloat(eventMap, "volume", 1);
				setup._balance = Json::getFloat(eventMap, "balance", 0);
			}
			skeletonData->_events[eventIndex] = eventData;
			eventIndex++;
		}
	}

	/* Animations. */
	animations = Json::getItem(root, "animations");
	if (animations) {
		skeletonData->_animations.setSize(animations->_size, 0);
		int animationsIndex = 0;
		for (Json *animationMap = animations->_child; animationMap; animationMap = animationMap->_next) {
			Animation *animation = readAnimation(animationMap, skeletonData);
			if (!animation) SKELETON_JSON_ERROR(root, "Error reading animation: ", animationMap->_name);
			skeletonData->_animations[animationsIndex++] = animation;
		}
	}

	/* Slider animations. */
	if (constraints) {
		for (Json *constraintMap = constraints->_child; constraintMap; constraintMap = constraintMap->_next) {
			const char *type = Json::getString(constraintMap, "type", 0);
			if (strcmp(type, "slider") == 0) {
				SliderData *data = skeletonData->findConstraint<SliderData>(Json::getString(constraintMap, "name", NULL));
				const char *animationName = Json::getString(constraintMap, "animation", NULL);
				if (animationName) {
					data->_animation = skeletonData->findAnimation(animationName);
					if (!data->_animation) SKELETON_JSON_ERROR(root, "Slider animation not found: ", animationName);
				}
			}
		}
	}

	delete root;

	return skeletonData;
}

Attachment *SkeletonJson::readAttachment(Json *map, Skin *skin, int slotIndex, const char *placeholder, SkeletonData *skeletonData) {
	float scale = _scale;
	const char *name = Json::getString(map, "name", placeholder);

	const char *typeStr = Json::getString(map, "type", "region");
	AttachmentType type = AttachmentType_valueOf(typeStr);
	switch (type) {
		case AttachmentType_Region: {
			const char *path = Json::getString(map, "path", name);
			Json *sequenceJson = Json::getItem(map, "sequence");
			Sequence *sequence = readSequence(sequenceJson);
			RegionAttachment *region = _attachmentLoader->newRegionAttachment(*skin, placeholder, name, path, sequence);
			if (!region) return NULL;
			region->_path = path;
			region->setX(Json::getFloat(map, "x", 0) * scale);
			region->setY(Json::getFloat(map, "y", 0) * scale);
			region->setScaleX(Json::getFloat(map, "scaleX", 1));
			region->setScaleY(Json::getFloat(map, "scaleY", 1));
			region->setRotation(Json::getFloat(map, "rotation", 0));
			region->setWidth(Json::getFloat(map, "width", 0) * scale);
			region->setHeight(Json::getFloat(map, "height", 0) * scale);

			const char *color = Json::getString(map, "color", NULL);
			if (color) Color::valueOf(color, region->getColor());

			region->updateSequence();
			return region;
		}
		case AttachmentType_Boundingbox: {
			BoundingBoxAttachment *box = _attachmentLoader->newBoundingBoxAttachment(*skin, placeholder, name);
			if (!box) return NULL;
			readVertices(map, box, Json::getInt(map, "vertexCount", 0) << 1);

			const char *color = Json::getString(map, "color", NULL);
			if (color) Color::valueOf(color, box->getColor());
			return box;
		}
		case AttachmentType_Mesh:
		case AttachmentType_Linkedmesh: {
			const char *path = Json::getString(map, "path", name);
			Sequence *sequence = readSequence(Json::getItem(map, "sequence"));
			MeshAttachment *mesh = _attachmentLoader->newMeshAttachment(*skin, placeholder, name, path, sequence);
			if (!mesh) return NULL;
			mesh->_path = path;

			const char *color = Json::getString(map, "color", NULL);
			if (color) Color::valueOf(color, mesh->getColor());

			mesh->setWidth(Json::getFloat(map, "width", 0) * scale);
			mesh->setHeight(Json::getFloat(map, "height", 0) * scale);

			const char *source = Json::getString(map, "source", NULL);
			if (source) {
				int sourceIndex = slotIndex;
				const char *slot = Json::getString(map, "slot", NULL);
				if (slot != NULL) {
					SlotData *sourceSlot = skeletonData->findSlot(slot);
					if (!sourceSlot) {
						setError(NULL, "Source mesh slot not found: ", slot);
						return NULL;
					}
					sourceIndex = sourceSlot->getIndex();
				}
				LinkedMesh *linkedMesh = new (__FILE__, __LINE__)
					LinkedMesh(*mesh, Json::getString(map, "skin", NULL), slotIndex, sourceIndex, source, Json::getBoolean(map, "timelines", true));
				_linkedMeshes.add(linkedMesh);
				return mesh;
			}

			Array<float> uvs;
			if (!Json::asFloatArray(Json::getItem(map, "uvs"), uvs)) return NULL;
			readVertices(map, mesh, uvs.size());
			Array<unsigned short> triangles;
			if (!Json::asUnsignedShortArray(Json::getItem(map, "triangles"), triangles)) return NULL;
			mesh->_triangles.clearAndAddAll(triangles);
			mesh->_regionUVs.clearAndAddAll(uvs);

			if (Json::getInt(map, "hull", 0)) mesh->setHullLength(Json::getInt(map, "hull", 0) << 1);
			Array<unsigned short> edges;
			Json::asUnsignedShortArray(Json::getItem(map, "edges"), edges);
			if (edges.size() > 0) mesh->_edges.clearAndAddAll(edges);

			mesh->updateSequence();
			return mesh;
		}
		case AttachmentType_Path: {
			PathAttachment *path = _attachmentLoader->newPathAttachment(*skin, placeholder, name);
			if (!path) return NULL;
			path->setClosed(Json::getBoolean(map, "closed", false));
			path->setConstantSpeed(Json::getBoolean(map, "constantSpeed", true));

			int vertexCount = Json::getInt(map, "vertexCount", 0);
			readVertices(map, path, vertexCount << 1);

			if (!Json::asFloatArray(Json::getItem(map, "lengths"), path->_lengths)) return NULL;
			for (int i = 0; i < (int) path->_lengths.size(); i++) path->_lengths[i] *= scale;

			const char *color = Json::getString(map, "color", NULL);
			if (color) Color::valueOf(color, path->getColor());
			return path;
		}
		case AttachmentType_Point: {
			PointAttachment *point = _attachmentLoader->newPointAttachment(*skin, placeholder, name);
			if (!point) return NULL;
			point->setX(Json::getFloat(map, "x", 0) * scale);
			point->setY(Json::getFloat(map, "y", 0) * scale);
			point->setRotation(Json::getFloat(map, "rotation", 0));

			const char *color = Json::getString(map, "color", NULL);
			if (color) Color::valueOf(color, point->getColor());
			return point;
		}
		case AttachmentType_Clipping: {
			ClippingAttachment *clip = _attachmentLoader->newClippingAttachment(*skin, placeholder, name);
			if (!clip) return NULL;

			const char *end = Json::getString(map, "end", NULL);
			if (end) {
				SlotData *slot = skeletonData->findSlot(end);
				if (!slot) return NULL;
				clip->setEndSlot(slot);
			}

			clip->setConvex(Json::getBoolean(map, "convex", false));
			clip->setInverse(Json::getBoolean(map, "inverse", false));

			readVertices(map, clip, Json::getInt(map, "vertexCount", 0) << 1);

			const char *color = Json::getString(map, "color", NULL);
			if (color) Color::valueOf(color, clip->getColor());
			return clip;
		}
		default:
			return NULL;
	}
}

Sequence *SkeletonJson::readSequence(Json *item) {
	if (item == NULL) return new (__FILE__, __LINE__) Sequence(1, false);
	Sequence *sequence = new (__FILE__, __LINE__) Sequence(Json::getInt(item, "count", 0), true);
	sequence->_start = Json::getInt(item, "start", 1);
	sequence->_digits = Json::getInt(item, "digits", 0);
	sequence->_setupIndex = Json::getInt(item, "setup", 0);
	return sequence;
}

void SkeletonJson::readVertices(Json *map, VertexAttachment *attachment, size_t verticesLength) {
	attachment->setWorldVerticesLength(verticesLength);
	Array<float> vertices;
	if (!Json::asFloatArray(Json::getItem(map, "vertices"), vertices)) {
		return;
	}
	if (verticesLength == vertices.size()) {
		if (_scale != 1) {
			for (int i = 0; i < (int) vertices.size(); ++i) vertices[i] *= _scale;
		}
		attachment->getVertices().clearAndAddAll(vertices);
		return;
	}

	Vertices bonesAndWeights;
	bonesAndWeights._weights.ensureCapacity(verticesLength * 3 * 3);
	bonesAndWeights._bones.ensureCapacity(verticesLength * 3);
	for (int i = 0, n = (int) vertices.size(); i < n;) {
		int boneCount = (int) vertices[i++];
		bonesAndWeights._bones.add(boneCount);
		for (int nn = i + (boneCount << 2); i < nn; i += 4) {
			bonesAndWeights._bones.add((int) vertices[i]);
			bonesAndWeights._weights.add(vertices[i + 1] * _scale);
			bonesAndWeights._weights.add(vertices[i + 2] * _scale);
			bonesAndWeights._weights.add(vertices[i + 3]);
		}
	}
	attachment->getBones().clearAndAddAll(bonesAndWeights._bones);
	attachment->getVertices().clearAndAddAll(bonesAndWeights._weights);
}

Animation *SkeletonJson::readAnimation(Json *map, SkeletonData *skeletonData) {
	Array<Timeline *> timelines;
	Array<int> bones;

	// Slot timelines.
	for (Json *slotMap = Json::getItem(map, "slots") ? Json::getItem(map, "slots")->_child : NULL; slotMap; slotMap = slotMap->_next) {
		int slotIndex = findSlotIndex(skeletonData, slotMap->_name, timelines);
		if (slotIndex == -1) return NULL;

		for (Json *timelineMap = slotMap->_child; timelineMap; timelineMap = timelineMap->_next) {
			Json *keyMap = timelineMap->_child;
			if (keyMap == NULL) continue;

			int frames = timelineMap->_size;
			if (strcmp(timelineMap->_name, "attachment") == 0) {
				AttachmentTimeline *timeline = new (__FILE__, __LINE__) AttachmentTimeline(frames, slotIndex);
				for (int frame = 0; keyMap; keyMap = keyMap->_next, ++frame) {
					timeline->setFrame(frame, Json::getFloat(keyMap, "time", 0),
									   Json::getItem(keyMap, "name") ? Json::getItem(keyMap, "name")->_valueString : NULL);
				}
				timelines.add(timeline);

			} else if (strcmp(timelineMap->_name, "rgba") == 0) {
				RGBATimeline *timeline = new (__FILE__, __LINE__) RGBATimeline(frames, frames << 2, slotIndex);
				float time = Json::getFloat(keyMap, "time", 0);
				Color color;
				Color::valueOf(Json::getString(keyMap, "color", 0), color);

				for (int frame = 0, bezier = 0;; ++frame) {
					timeline->setFrame(frame, time, color.r, color.g, color.b, color.a);
					Json *nextMap = keyMap->_next;
					if (!nextMap) {
						break;
					}
					float time2 = Json::getFloat(nextMap, "time", 0);
					Color newColor;
					Color::valueOf(Json::getString(nextMap, "color", 0), newColor);
					Json *curve = Json::getItem(keyMap, "curve");
					if (curve) {
						bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, color.a, newColor.a, 1);
					}
					time = time2;
					color = newColor;
					keyMap = nextMap;
				}
				timelines.add(timeline);
			} else if (strcmp(timelineMap->_name, "rgb") == 0) {
				RGBTimeline *timeline = new (__FILE__, __LINE__) RGBTimeline(frames, frames * 3, slotIndex);
				float time = Json::getFloat(keyMap, "time", 0);
				const char *colorStr = Json::getString(keyMap, "color", 0);
				Color color;
				if (colorStr && strlen(colorStr) >= 6) {
					color.r = Color::parseHex(colorStr, 0);
					color.g = Color::parseHex(colorStr, 1);
					color.b = Color::parseHex(colorStr, 2);
				}

				for (int frame = 0, bezier = 0;; ++frame) {
					timeline->setFrame(frame, time, color.r, color.g, color.b);
					Json *nextMap = keyMap->_next;
					if (!nextMap) {
						break;
					}
					float time2 = Json::getFloat(nextMap, "time", 0);
					const char *colorStr2 = Json::getString(nextMap, "color", 0);
					Color newColor;
					if (colorStr2 && strlen(colorStr2) >= 6) {
						newColor.r = Color::parseHex(colorStr2, 0);
						newColor.g = Color::parseHex(colorStr2, 1);
						newColor.b = Color::parseHex(colorStr2, 2);
					}
					Json *curve = Json::getItem(keyMap, "curve");
					if (curve) {
						bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
					}
					time = time2;
					color = newColor;
					keyMap = nextMap;
				}
				timelines.add(timeline);
			} else if (strcmp(timelineMap->_name, "alpha") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) AlphaTimeline(frames, frames, slotIndex), 0, 1);
			} else if (strcmp(timelineMap->_name, "rgba2") == 0) {
				RGBA2Timeline *timeline = new (__FILE__, __LINE__) RGBA2Timeline(frames, frames * 7, slotIndex);
				float time = Json::getFloat(keyMap, "time", 0);
				Color color, color2;
				Color::valueOf(Json::getString(keyMap, "light", 0), color);
				const char *darkStr = Json::getString(keyMap, "dark", 0);
				if (darkStr && strlen(darkStr) >= 6) {
					color2.r = Color::parseHex(darkStr, 0);
					color2.g = Color::parseHex(darkStr, 1);
					color2.b = Color::parseHex(darkStr, 2);
				}

				for (int frame = 0, bezier = 0;; ++frame) {
					timeline->setFrame(frame, time, color.r, color.g, color.b, color.a, color2.r, color2.g, color2.b);
					Json *nextMap = keyMap->_next;
					if (!nextMap) {
						break;
					}
					float time2 = Json::getFloat(nextMap, "time", 0);
					Color newColor, newColor2;
					Color::valueOf(Json::getString(nextMap, "light", 0), newColor);
					const char *darkStr2 = Json::getString(nextMap, "dark", 0);
					if (darkStr2 && strlen(darkStr2) >= 6) {
						newColor2.r = Color::parseHex(darkStr2, 0);
						newColor2.g = Color::parseHex(darkStr2, 1);
						newColor2.b = Color::parseHex(darkStr2, 2);
					}
					Json *curve = Json::getItem(keyMap, "curve");
					if (curve) {
						bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, color.a, newColor.a, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, color2.r, newColor2.r, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, color2.g, newColor2.g, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 6, time, time2, color2.b, newColor2.b, 1);
					}
					time = time2;
					color = newColor;
					color2 = newColor2;
					keyMap = nextMap;
				}
				timelines.add(timeline);
			} else if (strcmp(timelineMap->_name, "rgb2") == 0) {
				RGB2Timeline *timeline = new (__FILE__, __LINE__) RGB2Timeline(frames, frames * 6, slotIndex);
				float time = Json::getFloat(keyMap, "time", 0);
				const char *lightStr = Json::getString(keyMap, "light", 0);
				Color color, color2;
				if (lightStr && strlen(lightStr) >= 6) {
					color.r = Color::parseHex(lightStr, 0);
					color.g = Color::parseHex(lightStr, 1);
					color.b = Color::parseHex(lightStr, 2);
				}
				const char *darkStr = Json::getString(keyMap, "dark", 0);
				if (darkStr && strlen(darkStr) >= 6) {
					color2.r = Color::parseHex(darkStr, 0);
					color2.g = Color::parseHex(darkStr, 1);
					color2.b = Color::parseHex(darkStr, 2);
				}

				for (int frame = 0, bezier = 0;; ++frame) {
					timeline->setFrame(frame, time, color.r, color.g, color.b, color2.r, color2.g, color2.b);
					Json *nextMap = keyMap->_next;
					if (!nextMap) {
						break;
					}
					float time2 = Json::getFloat(nextMap, "time", 0);
					const char *lightStr2 = Json::getString(nextMap, "light", 0);
					Color newColor, newColor2;
					if (lightStr2 && strlen(lightStr2) >= 6) {
						newColor.r = Color::parseHex(lightStr2, 0);
						newColor.g = Color::parseHex(lightStr2, 1);
						newColor.b = Color::parseHex(lightStr2, 2);
					}
					const char *darkStr2 = Json::getString(nextMap, "dark", 0);
					if (darkStr2 && strlen(darkStr2) >= 6) {
						newColor2.r = Color::parseHex(darkStr2, 0);
						newColor2.g = Color::parseHex(darkStr2, 1);
						newColor2.b = Color::parseHex(darkStr2, 2);
					}
					Json *curve = Json::getItem(keyMap, "curve");
					if (curve) {
						bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, color2.r, newColor2.r, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, color2.g, newColor2.g, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, color2.b, newColor2.b, 1);
					}
					time = time2;
					color = newColor;
					color2 = newColor2;
					keyMap = nextMap;
				}
				timelines.add(timeline);
			} else {
				ArrayUtils::deleteElements(timelines);
				return NULL;
			}
		}
	}

	// Bone timelines.
	Json *boneMaps = Json::getItem(map, "bones");
	bones.ensureCapacity(boneMaps ? boneMaps->_size : 0);
	for (Json *boneMap = boneMaps ? boneMaps->_child : NULL; boneMap; boneMap = boneMap->_next) {
		int boneIndex = ArrayUtils::findIndexWithName(skeletonData->_bones, boneMap->_name);
		if (boneIndex == -1) {
			ArrayUtils::deleteElements(timelines);
			return NULL;
		}
		bones.add(boneIndex);

		for (Json *timelineMap = boneMap->_child; timelineMap; timelineMap = timelineMap->_next) {
			Json *keyMap = timelineMap->_child;
			if (keyMap == NULL) continue;

			int frames = timelineMap->_size;
			if (strcmp(timelineMap->_name, "rotate") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) RotateTimeline(frames, frames, boneIndex), 0, 1);
			} else if (strcmp(timelineMap->_name, "translate") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) TranslateTimeline(frames, frames << 1, boneIndex), "x", "y", 0, _scale);
			} else if (strcmp(timelineMap->_name, "translatex") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) TranslateXTimeline(frames, frames, boneIndex), 0, _scale);
			} else if (strcmp(timelineMap->_name, "translatey") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) TranslateYTimeline(frames, frames, boneIndex), 0, _scale);
			} else if (strcmp(timelineMap->_name, "scale") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) ScaleTimeline(frames, frames << 1, boneIndex), "x", "y", 1, 1);
			} else if (strcmp(timelineMap->_name, "scalex") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) ScaleXTimeline(frames, frames, boneIndex), 1, 1);
			} else if (strcmp(timelineMap->_name, "scaley") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) ScaleYTimeline(frames, frames, boneIndex), 1, 1);
			} else if (strcmp(timelineMap->_name, "shear") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) ShearTimeline(frames, frames << 1, boneIndex), "x", "y", 0, 1);
			} else if (strcmp(timelineMap->_name, "shearx") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) ShearXTimeline(frames, frames, boneIndex), 0, 1);
			} else if (strcmp(timelineMap->_name, "sheary") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) ShearYTimeline(frames, frames, boneIndex), 0, 1);
			} else if (strcmp(timelineMap->_name, "inherit") == 0) {
				InheritTimeline *timeline = new (__FILE__, __LINE__) InheritTimeline(frames, boneIndex);
				for (int frame = 0; keyMap; keyMap = keyMap->_next, frame++) {
					float time = Json::getFloat(keyMap, "time", 0);
					Inherit inherit = Inherit_valueOf(Json::getString(keyMap, "inherit", "normal"));
					timeline->setFrame(frame, time, inherit);
				}
				timelines.add(timeline);
			} else {
				ArrayUtils::deleteElements(timelines);
				return NULL;
			}
		}
	}

	// IK constraint timelines.
	for (Json *timelineMap = Json::getItem(map, "ik") ? Json::getItem(map, "ik")->_child : NULL; timelineMap; timelineMap = timelineMap->_next) {
		Json *keyMap = timelineMap->_child;
		if (keyMap == NULL) continue;
		IkConstraintData *constraint = skeletonData->findConstraint<IkConstraintData>(timelineMap->_name);
		if (!constraint) {
			ArrayUtils::deleteElements(timelines);
			return NULL;
		}
		int constraintIndex = skeletonData->_constraints.indexOf(constraint);
		IkConstraintTimeline *timeline = new (__FILE__, __LINE__) IkConstraintTimeline(timelineMap->_size, timelineMap->_size << 1, constraintIndex);
		float time = Json::getFloat(keyMap, "time", 0);
		float mix = Json::getFloat(keyMap, "mix", 1), softness = Json::getFloat(keyMap, "softness", 0) * _scale;
		for (int frame = 0, bezier = 0;; frame++) {
			timeline->setFrame(frame, time, mix, softness, Json::getBoolean(keyMap, "bendPositive", true) ? 1 : -1,
							   Json::getBoolean(keyMap, "compress", false), Json::getBoolean(keyMap, "stretch", false));
			Json *nextMap = keyMap->_next;
			if (!nextMap) {
				break;
			}

			float time2 = Json::getFloat(nextMap, "time", 0);
			float mix2 = Json::getFloat(nextMap, "mix", 1), softness2 = Json::getFloat(nextMap, "softness", 0) * _scale;
			Json *curve = Json::getItem(keyMap, "curve");
			if (curve) {
				bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mix, mix2, 1);
				bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, softness, softness2, _scale);
			}

			time = time2;
			mix = mix2;
			softness = softness2;
			keyMap = nextMap;
		}

		timelines.add(timeline);
	}

	// Transform constraint timelines.
	for (Json *timelineMap = Json::getItem(map, "transform") ? Json::getItem(map, "transform")->_child : NULL; timelineMap;
		 timelineMap = timelineMap->_next) {
		Json *keyMap = timelineMap->_child;
		if (keyMap == NULL) continue;
		TransformConstraintData *constraint = skeletonData->findConstraint<TransformConstraintData>(timelineMap->_name);
		if (!constraint) {
			ArrayUtils::deleteElements(timelines);
			return NULL;
		}
		int constraintIndex = skeletonData->_constraints.indexOf(constraint);
		TransformConstraintTimeline *timeline = new (__FILE__, __LINE__)
			TransformConstraintTimeline(timelineMap->_size, timelineMap->_size * 6, constraintIndex);
		float time = Json::getFloat(keyMap, "time", 0);
		float mixRotate = Json::getFloat(keyMap, "mixRotate", 1);
		float mixX = Json::getFloat(keyMap, "mixX", 1), mixY = Json::getFloat(keyMap, "mixY", mixX);
		float mixScaleX = Json::getFloat(keyMap, "mixScaleX", 1), mixScaleY = Json::getFloat(keyMap, "mixScaleY", 1);
		float mixShearY = Json::getFloat(keyMap, "mixShearY", 1);
		for (int frame = 0, bezier = 0;; frame++) {
			timeline->setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
			Json *nextMap = keyMap->_next;
			if (!nextMap) {
				break;
			}
			float time2 = Json::getFloat(nextMap, "time", 0);
			float mixRotate2 = Json::getFloat(nextMap, "mixRotate", 1);
			float mixX2 = Json::getFloat(nextMap, "mixX", 1), mixY2 = Json::getFloat(nextMap, "mixY", mixX2);
			float mixScaleX2 = Json::getFloat(nextMap, "mixScaleX", 1), mixScaleY2 = Json::getFloat(nextMap, "mixScaleY", 1);
			float mixShearY2 = Json::getFloat(nextMap, "mixShearY", 1);
			Json *curve = Json::getItem(keyMap, "curve");
			if (curve) {
				bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
				bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
				bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
				bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
				bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
				bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, mixShearY, mixShearY2, 1);
			}

			time = time2;
			mixRotate = mixRotate2;
			mixX = mixX2;
			mixY = mixY2;
			mixScaleX = mixScaleX2;
			mixScaleY = mixScaleY2;
			mixShearY = mixShearY2;
			keyMap = nextMap;
		}

		timelines.add(timeline);
	}

	// Path constraint timelines.
	for (Json *constraintMap = Json::getItem(map, "path") ? Json::getItem(map, "path")->_child : NULL; constraintMap;
		 constraintMap = constraintMap->_next) {
		PathConstraintData *constraint = skeletonData->findConstraint<PathConstraintData>(constraintMap->_name);
		if (!constraint) {
			ArrayUtils::deleteElements(timelines);
			return NULL;
		}
		int index = skeletonData->_constraints.indexOf(constraint);
		for (Json *timelineMap = constraintMap->_child; timelineMap; timelineMap = timelineMap->_next) {
			Json *keyMap = timelineMap->_child;
			if (keyMap == NULL) continue;

			int frames = timelineMap->_size;
			const char *timelineName = timelineMap->_name;
			if (strcmp(timelineName, "position") == 0) {
				PathConstraintPositionTimeline *timeline = new (__FILE__, __LINE__) PathConstraintPositionTimeline(frames, frames, index);
				readTimeline(timelines, keyMap, timeline, 0, constraint->_positionMode == PositionMode_Fixed ? _scale : 1);
			} else if (strcmp(timelineName, "spacing") == 0) {
				CurveTimeline1 *timeline = new (__FILE__, __LINE__) PathConstraintSpacingTimeline(frames, frames, index);
				readTimeline(timelines, keyMap, timeline, 0,
							 constraint->_spacingMode == SpacingMode_Length || constraint->_spacingMode == SpacingMode_Fixed ? _scale : 1);
			} else if (strcmp(timelineName, "mix") == 0) {
				PathConstraintMixTimeline *timeline = new (__FILE__, __LINE__) PathConstraintMixTimeline(frames, frames * 3, index);
				float time = Json::getFloat(keyMap, "time", 0);
				float mixRotate = Json::getFloat(keyMap, "mixRotate", 1);
				float mixX = Json::getFloat(keyMap, "mixX", 1);
				float mixY = Json::getFloat(keyMap, "mixY", mixX);
				for (int frame = 0, bezier = 0;; frame++) {
					timeline->setFrame(frame, time, mixRotate, mixX, mixY);
					Json *nextMap = keyMap->_next;
					if (!nextMap) {
						break;
					}
					float time2 = Json::getFloat(nextMap, "time", 0);
					float mixRotate2 = Json::getFloat(nextMap, "mixRotate", 1);
					float mixX2 = Json::getFloat(nextMap, "mixX", 1), mixY2 = Json::getFloat(nextMap, "mixY", mixX2);
					Json *curve = Json::getItem(keyMap, "curve");
					if (curve != NULL) {
						bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
					}
					time = time2;
					mixRotate = mixRotate2;
					mixX = mixX2;
					mixY = mixY2;
					keyMap = nextMap;
				}
				timelines.add(timeline);
			}
		}
	}

	// Physics constraint timelines.
	for (Json *constraintMap = Json::getItem(map, "physics") ? Json::getItem(map, "physics")->_child : NULL; constraintMap;
		 constraintMap = constraintMap->_next) {
		int index = -1;
		if (constraintMap->_name && strlen(constraintMap->_name) > 0) {
			PhysicsConstraintData *constraint = skeletonData->findConstraint<PhysicsConstraintData>(constraintMap->_name);
			if (!constraint) {
				ArrayUtils::deleteElements(timelines);
				return NULL;
			}
			index = skeletonData->_constraints.indexOf(constraint);
		}
		for (Json *timelineMap = constraintMap->_child; timelineMap; timelineMap = timelineMap->_next) {
			Json *keyMap = timelineMap->_child;
			if (keyMap == NULL) continue;

			int frames = timelineMap->_size;
			const char *timelineName = timelineMap->_name;
			if (strcmp(timelineName, "reset") == 0) {
				PhysicsConstraintResetTimeline *timeline = new (__FILE__, __LINE__) PhysicsConstraintResetTimeline(frames, index);
				for (int frame = 0; keyMap != NULL; keyMap = keyMap->_next, frame++) timeline->setFrame(frame, Json::getFloat(keyMap, "time", 0));
				timelines.add(timeline);
				continue;
			}

			CurveTimeline1 *timeline = NULL;
			float defaultValue = 0;
			if (strcmp(timelineName, "inertia") == 0) {
				timeline = new (__FILE__, __LINE__) PhysicsConstraintInertiaTimeline(frames, frames, index);
			} else if (strcmp(timelineName, "strength") == 0) {
				timeline = new (__FILE__, __LINE__) PhysicsConstraintStrengthTimeline(frames, frames, index);
			} else if (strcmp(timelineName, "damping") == 0) {
				timeline = new (__FILE__, __LINE__) PhysicsConstraintDampingTimeline(frames, frames, index);
			} else if (strcmp(timelineName, "mass") == 0) {
				timeline = new (__FILE__, __LINE__) PhysicsConstraintMassTimeline(frames, frames, index);
			} else if (strcmp(timelineName, "wind") == 0) {
				timeline = new (__FILE__, __LINE__) PhysicsConstraintWindTimeline(frames, frames, index);
			} else if (strcmp(timelineName, "gravity") == 0) {
				timeline = new (__FILE__, __LINE__) PhysicsConstraintGravityTimeline(frames, frames, index);
			} else if (strcmp(timelineName, "mix") == 0) {
				defaultValue = 1;
				timeline = new (__FILE__, __LINE__) PhysicsConstraintMixTimeline(frames, frames, index);
			} else {
				continue;
			}
			readTimeline(timelines, keyMap, timeline, defaultValue, 1);
		}
	}

	// Slider timelines.
	for (Json *constraintMap = Json::getItem(map, "slider") ? Json::getItem(map, "slider")->_child : NULL; constraintMap;
		 constraintMap = constraintMap->_next) {
		SliderData *constraint = skeletonData->findConstraint<SliderData>(constraintMap->_name);
		if (!constraint) {
			ArrayUtils::deleteElements(timelines);
			return NULL;
		}
		int index = skeletonData->_constraints.indexOf(constraint);
		for (Json *timelineMap = constraintMap->_child; timelineMap; timelineMap = timelineMap->_next) {
			Json *keyMap = timelineMap->_child;
			if (keyMap == NULL) continue;

			int frames = timelineMap->_size;
			if (strcmp(timelineMap->_name, "time") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) SliderTimeline(frames, frames, index), 1, 1);
			} else if (strcmp(timelineMap->_name, "mix") == 0) {
				readTimeline(timelines, keyMap, new (__FILE__, __LINE__) SliderMixTimeline(frames, frames, index), 1, 1);
			}
		}
	}

	// Attachment timelines.
	for (Json *attachmentsMap = Json::getItem(map, "attachments") ? Json::getItem(map, "attachments")->_child : NULL; attachmentsMap;
		 attachmentsMap = attachmentsMap->_next) {
		Skin *skin = skeletonData->findSkin(attachmentsMap->_name);
		if (!skin) {
			ArrayUtils::deleteElements(timelines);
			return NULL;
		}
		for (Json *slotMap = attachmentsMap->_child; slotMap; slotMap = slotMap->_next) {
			SlotData *slot = skeletonData->findSlot(slotMap->_name);
			if (!slot) {
				ArrayUtils::deleteElements(timelines);
				setError(NULL, "Attachment slot not found: ", slotMap->_name);
				return NULL;
			}
			int slotIndex = slot->getIndex();
			for (Json *attachmentMap = slotMap->_child; attachmentMap; attachmentMap = attachmentMap->_next) {
				Attachment *attachment = skin->getAttachment(slotIndex, attachmentMap->_name);
				if (!attachment) {
					ArrayUtils::deleteElements(timelines);
					return NULL;
				}
				for (Json *timelineMap = attachmentMap->_child; timelineMap; timelineMap = timelineMap->_next) {
					Json *keyMap = timelineMap->_child;
					int frames = timelineMap->_size;
					String timelineName = timelineMap->_name;
					if (timelineName == "deform") {
						VertexAttachment *vertexAttachment = static_cast<VertexAttachment *>(attachment);
						bool weighted = vertexAttachment->_bones.size() != 0;
						Array<float> &vertices = vertexAttachment->_vertices;
						int deformLength = weighted ? (int) vertices.size() / 3 * 2 : (int) vertices.size();

						DeformTimeline *timeline = new (__FILE__, __LINE__) DeformTimeline(frames, frames, slotIndex, *vertexAttachment);
						float time = Json::getFloat(keyMap, "time", 0);
						for (int frame = 0, bezier = 0;; frame++) {
							Array<float> deform;
							Json *verticesValue = Json::getItem(keyMap, "vertices");
							if (!verticesValue) {
								if (weighted) {
									deform.setSize(deformLength, 0);
								} else {
									deform.clearAndAddAll(vertexAttachment->_vertices);
								}
							} else {
								deform.setSize(deformLength, 0);
								int i, start = Json::getInt(keyMap, "offset", 0);
								Json *vertex;
								for (vertex = verticesValue->_child, i = start; vertex; vertex = vertex->_next, ++i) {
									deform[i] = vertex->_valueFloat;
								}
								if (_scale != 1) {
									for (vertex = verticesValue->_child, i = start; vertex; vertex = vertex->_next, ++i) {
										deform[i] *= _scale;
									}
								}
								if (!weighted) {
									for (i = 0; i < deformLength; ++i) {
										deform[i] += vertices[i];
									}
								}
							}

							timeline->setFrame(frame, time, deform);
							Json *nextMap = keyMap->_next;
							if (!nextMap) {
								break;
							}
							float time2 = Json::getFloat(nextMap, "time", 0);
							Json *curve = Json::getItem(keyMap, "curve");
							if (curve) bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, 0, 1, 1);
							time = time2;
							keyMap = nextMap;
						}
						timelines.add(timeline);
					} else if (timelineName == "sequence") {
						SequenceTimeline *timeline = new (__FILE__, __LINE__) SequenceTimeline(frames, slotIndex, *attachment);
						float lastDelay = 0;
						for (int frame = 0; keyMap != NULL; keyMap = keyMap->_next, frame++) {
							float delay = Json::getFloat(keyMap, "delay", lastDelay);
							timeline->setFrame(frame, Json::getFloat(keyMap, "time", 0),
											   SequenceMode_valueOf(Json::getString(keyMap, "mode", "hold")), Json::getInt(keyMap, "index", 0),
											   delay);
							lastDelay = delay;
						}
						timelines.add(timeline);
					}
				}
			}
		}
	}

	// Draw order timeline.
	Json *drawOrder = Json::getItem(map, "drawOrder");
	if (drawOrder) {
		DrawOrderTimeline *timeline = new (__FILE__, __LINE__) DrawOrderTimeline(drawOrder->_size);
		int slotCount = (int) skeletonData->_slots.size();
		int frame = 0;
		for (Json *keyMap = drawOrder->_child; keyMap; keyMap = keyMap->_next, ++frame) {
			Array<int> drawOrder2;
			if (!readDrawOrder(skeletonData, keyMap, slotCount, NULL, drawOrder2)) {
				ArrayUtils::deleteElements(timelines);
				return NULL;
			}
			timeline->setFrame(frame, Json::getFloat(keyMap, "time", 0), drawOrder2.size() == 0 ? NULL : &drawOrder2);
		}
		timelines.add(timeline);
	}

	// Draw order folder timelines.
	Json *drawOrderFolder = Json::getItem(map, "drawOrderFolder");
	if (drawOrderFolder) {
		for (Json *timelineMap = drawOrderFolder->_child; timelineMap; timelineMap = timelineMap->_next) {
			Json *slotEntry = Json::getItem(timelineMap, "slots");
			Array<int> folderSlots;
			folderSlots.setSize(slotEntry ? slotEntry->_size : 0, 0);
			int ii = 0;
			for (Json *entry = slotEntry ? slotEntry->_child : NULL; entry; entry = entry->_next, ++ii) {
				SlotData *slot = skeletonData->findSlot(entry->_valueString);
				if (!slot) {
					ArrayUtils::deleteElements(timelines);
					setError(NULL, "Draw order folder slot not found: ", entry->_valueString);
					return NULL;
				}
				folderSlots[ii] = slot->getIndex();
			}
			Json *keyMap = Json::getItem(timelineMap, "keys");
			DrawOrderFolderTimeline *timeline = new (__FILE__, __LINE__)
				DrawOrderFolderTimeline(keyMap ? keyMap->_size : 0, folderSlots, skeletonData->_slots.size());
			int frame = 0;
			for (Json *entry = keyMap ? keyMap->_child : NULL; entry; entry = entry->_next, ++frame) {
				Array<int> folderDrawOrder;
				if (!readDrawOrder(skeletonData, entry, (int) folderSlots.size(), &folderSlots, folderDrawOrder)) {
					ArrayUtils::deleteElements(timelines);
					return NULL;
				}
				timeline->setFrame(frame, Json::getFloat(entry, "time", 0), folderDrawOrder.size() == 0 ? NULL : &folderDrawOrder);
			}
			timelines.add(timeline);
		}
	}

	// Event timeline.
	Json *events = Json::getItem(map, "events");
	if (events) {
		EventTimeline *timeline = new (__FILE__, __LINE__) EventTimeline(events->_size);
		int frame = 0;
		for (Json *keyMap = events->_child; keyMap; keyMap = keyMap->_next, ++frame) {
			EventData *eventData = skeletonData->findEvent(Json::getString(keyMap, "name", 0));
			if (!eventData) {
				ArrayUtils::deleteElements(timelines);
				return NULL;
			}
			Event &setup = eventData->_setupPose;
			Event *event = new (__FILE__, __LINE__) Event(Json::getFloat(keyMap, "time", 0), *eventData);
			event->_intValue = Json::getInt(keyMap, "int", setup._intValue);
			event->_floatValue = Json::getFloat(keyMap, "float", setup._floatValue);
			event->_stringValue = Json::getString(keyMap, "string", setup._stringValue.buffer());
			if (!eventData->_audioPath.isEmpty()) {
				event->_volume = Json::getFloat(keyMap, "volume", setup._volume);
				event->_balance = Json::getFloat(keyMap, "balance", setup._balance);
			}
			timeline->setFrame(frame, *event);
		}
		timelines.add(timeline);
	}

	float duration = 0;
	for (size_t i = 0; i < timelines.size(); i++) duration = MathUtil::max(duration, timelines[i]->getDuration());
	Animation *animation = new (__FILE__, __LINE__) Animation(String(map->_name));
	animation->setTimelines(timelines, bones);
	animation->setDuration(duration);
	const char *color = Json::getString(map, "color", NULL);
	if (color) Color::valueOf(color, animation->getColor());
	return animation;
}

void SkeletonJson::readTimeline(Array<Timeline *> &timelines, Json *keyMap, CurveTimeline1 *timeline, float defaultValue, float scale) {
	float time = Json::getFloat(keyMap, "time", 0), value = Json::getFloat(keyMap, "value", defaultValue) * scale;
	for (int frame = 0, bezier = 0;; frame++) {
		timeline->setFrame(frame, time, value);
		Json *nextMap = keyMap->_next;
		if (!nextMap) {
			timelines.add(timeline);
			return;
		}
		float time2 = Json::getFloat(nextMap, "time", 0);
		float value2 = Json::getFloat(nextMap, "value", defaultValue) * scale;
		Json *curve = Json::getItem(keyMap, "curve");
		if (curve != NULL) bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
		time = time2;
		value = value2;
		keyMap = nextMap;
	}
}

void SkeletonJson::readTimeline(Array<Timeline *> &timelines, Json *keyMap, BoneTimeline2 *timeline, const char *name1, const char *name2,
								float defaultValue, float scale) {
	float time = Json::getFloat(keyMap, "time", 0);
	float value1 = Json::getFloat(keyMap, name1, defaultValue) * scale, value2 = Json::getFloat(keyMap, name2, defaultValue) * scale;
	for (int frame = 0, bezier = 0;; frame++) {
		timeline->setFrame(frame, time, value1, value2);
		Json *nextMap = keyMap->_next;
		if (!nextMap) {
			timelines.add(timeline);
			return;
		}
		float time2 = Json::getFloat(nextMap, "time", 0);
		float nvalue1 = Json::getFloat(nextMap, name1, defaultValue) * scale, nvalue2 = Json::getFloat(nextMap, name2, defaultValue) * scale;
		Json *curve = Json::getItem(keyMap, "curve");
		if (curve != NULL) {
			bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
			bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
		}
		time = time2;
		value1 = nvalue1;
		value2 = nvalue2;
		keyMap = nextMap;
	}
}

int SkeletonJson::readCurve(Json *curve, CurveTimeline *timeline, int bezier, int frame, int value, float time1, float time2, float value1,
							float value2, float scale) {
	if (curve->_type == Json::JSON_STRING) {
		if (strcmp(curve->_valueString, "stepped") == 0) timeline->setStepped(frame);
		return bezier;
	}
	curve = Json::getItem(curve, value << 2);
	float cx1 = curve->_valueFloat;
	curve = curve->_next;
	float cy1 = curve->_valueFloat * scale;
	curve = curve->_next;
	float cx2 = curve->_valueFloat;
	curve = curve->_next;
	float cy2 = curve->_valueFloat * scale;
	setBezier(timeline, frame, value, bezier, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
	return bezier + 1;
}

void SkeletonJson::setBezier(CurveTimeline *timeline, int frame, int value, int bezier, float time1, float value1, float cx1, float cy1, float cx2,
							 float cy2, float time2, float value2) {
	timeline->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

int SkeletonJson::findSlotIndex(SkeletonData *skeletonData, const String &slotName, Array<Timeline *> timelines) {
	int slotIndex = ArrayUtils::findIndexWithName(skeletonData->getSlots(), slotName);
	if (slotIndex == -1) {
		ArrayUtils::deleteElements(timelines);
		setError(NULL, "Slot not found: ", slotName);
	}
	return slotIndex;
}

bool SkeletonJson::readDrawOrder(SkeletonData *skeletonData, Json *keyMap, int slotCount, const Array<int> *folderSlots, Array<int> &drawOrder) {
	Json *changes = Json::getItem(keyMap, "offsets");
	drawOrder.clear();
	if (changes == NULL) return true;

	drawOrder.setSize(slotCount, 0);
	for (int i = slotCount - 1; i >= 0; i--) drawOrder[i] = -1;
	Array<int> unchanged;
	unchanged.setSize(slotCount - changes->_size, 0);
	int originalIndex = 0, unchangedIndex = 0;
	for (Json *offsetMap = changes->_child; offsetMap; offsetMap = offsetMap->_next) {
		const char *slotName = Json::getString(offsetMap, "slot", 0);
		SlotData *slot = skeletonData->findSlot(slotName);
		if (slot == NULL) {
			setError(NULL, "Draw order slot not found: ", slotName);
			return false;
		}
		int index;
		if (folderSlots == NULL) {
			index = slot->getIndex();
		} else {
			index = -1;
			for (int i = 0; i < slotCount; i++) {
				if ((*folderSlots)[i] == slot->getIndex()) {
					index = i;
					break;
				}
			}
			if (index == -1) {
				setError(NULL, "Slot not in folder: ", slotName);
				return false;
			}
		}
		while (originalIndex != index) unchanged[unchangedIndex++] = originalIndex++;
		int drawOrderIndex = originalIndex;
		drawOrder[drawOrderIndex + Json::getInt(offsetMap, "offset", 0)] = originalIndex++;
	}
	while (originalIndex < slotCount) unchanged[unchangedIndex++] = originalIndex++;
	for (int i = slotCount - 1; i >= 0; i--)
		if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
	return true;
}

void SkeletonJson::setError(Json *root, const String &value1, const String &value2) {
	_error = String(value1).append(value2);
	delete root;
}
