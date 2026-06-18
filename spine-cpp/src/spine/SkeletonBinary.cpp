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

#include <spine/SkeletonBinary.h>

#include <spine/Animation.h>
#include <spine/Atlas.h>
#include <spine/AtlasAttachmentLoader.h>
#include <spine/Attachment.h>
#include <spine/CurveTimeline.h>
#include <spine/LinkedMesh.h>
#include <spine/SkeletonData.h>
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
#include <spine/SlotData.h>
#include <spine/TransformConstraintData.h>
#include <spine/TransformConstraintTimeline.h>
#include <spine/TranslateTimeline.h>
#include <spine/SequenceTimeline.h>
#include <spine/SliderData.h>
#include <spine/SliderTimeline.h>
#include <spine/SliderMixTimeline.h>
#include <spine/Version.h>

using namespace spine;

SkeletonBinary::SkeletonBinary(Atlas &atlas)
	: _attachmentLoader(new(__FILE__, __LINE__) AtlasAttachmentLoader(atlas)), _error(), _scale(1), _ownsLoader(true) {
}

SkeletonBinary::SkeletonBinary(AttachmentLoader &attachmentLoader, bool ownsLoader)
	: _attachmentLoader(&attachmentLoader), _error(), _scale(1), _ownsLoader(ownsLoader) {
}

SkeletonBinary::~SkeletonBinary() {
	ArrayUtils::deleteElements(_linkedMeshes);
	_linkedMeshes.clear();

	if (_ownsLoader) delete _attachmentLoader;
}

SkeletonData *SkeletonBinary::readSkeletonDataFile(const String &path) {
	int length;
	const char *binary = SpineExtension::readFile(path.buffer(), &length);
	if (length == 0 || !binary) {
		setError("Unable to read skeleton file: ", path.buffer());
		return NULL;
	}

	SkeletonData *skeletonData = readSkeletonData((unsigned char *) binary, length);
	SpineExtension::free(binary, __FILE__, __LINE__);

	if (skeletonData) {
		// Extract filename without extension
		const char *lastSlash = strrchr(path.buffer(), '/');
		const char *lastBackslash = strrchr(path.buffer(), '\\');
		const char *filename = path.buffer();
		if (lastSlash) filename = lastSlash + 1;
		if (lastBackslash && lastBackslash > filename) filename = lastBackslash + 1;

		String nameWithoutExtension(filename);
		const char *lastDot = strrchr(nameWithoutExtension.buffer(), '.');
		if (lastDot) {
			length = (int) (lastDot - nameWithoutExtension.buffer());
			nameWithoutExtension = nameWithoutExtension.substring(0, length);
		}
		skeletonData->_name = nameWithoutExtension;
	}

	return skeletonData;
}

SkeletonData *SkeletonBinary::readSkeletonData(const unsigned char *binary, const int length) {
	if (binary == NULL || length == 0) {
		setError("Unable to read skeleton file: ", "");
		return NULL;
	}

	ArrayUtils::deleteElements(_linkedMeshes);
	_linkedMeshes.clear();

	SkeletonData *skeletonData = new (__FILE__, __LINE__) SkeletonData();
	DataInput input(skeletonData, binary, length);
	String version;
	{// try block in Java
		long long hash = input.readLong();
		if (hash == 0) {
			skeletonData->_hash = "";
		} else {
			char buffer[32];
			snprintf(buffer, 32, "%lld", hash);
			skeletonData->_hash = String(buffer);
		}
		skeletonData->_version.own(input.readString());
		if (skeletonData->_version.isEmpty()) skeletonData->_version = "";
		version = skeletonData->_version;
		if (!skeletonData->_version.startsWith(SPINE_VERSION_STRING)) {
			String errorMsg = "Skeleton version ";
			errorMsg.append(skeletonData->_version);
			errorMsg.append(" does not match runtime version ");
			errorMsg.append(SPINE_VERSION_STRING);
			setError(errorMsg.buffer(), "");
			delete skeletonData;
			return NULL;
		}
		skeletonData->_x = input.readFloat();
		skeletonData->_y = input.readFloat();
		skeletonData->_width = input.readFloat();
		skeletonData->_height = input.readFloat();
		skeletonData->_referenceScale = input.readFloat() * this->_scale;

		bool nonessential = input.readBoolean();
		if (nonessential) {
			skeletonData->_fps = input.readFloat();
			skeletonData->_imagesPath.own(input.readString());
			skeletonData->_audioPath.own(input.readString());
		}

		int n = input.readInt(true);
		Array<char *> &strings = skeletonData->_strings.setSize(n, NULL);
		for (int i = 0; i < n; i++) strings[i] = input.readString();

		/* Bones. */
		Array<BoneData *> &bones = skeletonData->_bones.setSize(input.readInt(true), NULL);
		for (int i = 0; i < (int) bones.size(); ++i) {
			const char *name = input.readString();
			BoneData *parent = i == 0 ? 0 : bones[input.readInt(true)];
			BoneData *data = new (__FILE__, __LINE__) BoneData(i, String(name, true), parent);
			BonePose &setup = data->_setupPose;
			setup._rotation = input.readFloat();
			setup._x = input.readFloat() * _scale;
			setup._y = input.readFloat() * _scale;
			setup._scaleX = input.readFloat();
			setup._scaleY = input.readFloat();
			setup._shearX = input.readFloat();
			setup._shearY = input.readFloat();
			setup._inherit = static_cast<Inherit>(input.readByte());
			data->_length = input.readFloat() * _scale;
			data->_skinRequired = input.readBoolean();
			if (nonessential) {
				Color::rgba8888ToColor(data->getColor(), input.readInt());
				data->_icon.own(input.readString());
				data->_iconSize = input.readFloat();
				data->_iconRotation = input.readFloat();
				data->_visible = input.readBoolean();
			}
			bones[i] = data;
		}

		/* Slots. */
		Array<SlotData *> &slots = skeletonData->_slots.setSize(input.readInt(true), NULL);
		for (int i = 0; i < (int) slots.size(); ++i) {
			String slotName = String(input.readString(), true);
			BoneData *boneData = bones[input.readInt(true)];
			SlotData *data = new (__FILE__, __LINE__) SlotData(i, slotName, *boneData);
			Color::rgba8888ToColor(data->_setupPose._color, input.readInt());

			int darkColor = input.readInt();
			if (darkColor != -1) {
				Color::rgb888ToColor(data->_setupPose._darkColor, darkColor);
				data->_setupPose._hasDarkColor = true;
			}

			data->_attachmentName = input.readStringRef();
			data->_blendMode = static_cast<BlendMode>(input.readInt(true));
			if (nonessential) data->_visible = input.readBoolean();
			slots[i] = data;
		}

		/* Constraints. */
		int constraintCount = input.readInt(true);
		Array<ConstraintData *> &constraints = skeletonData->_constraints.setSize(constraintCount, NULL);
		for (int i = 0; i < constraintCount; i++) {
			String name(input.readString(), true);
			int nn;
			switch (input.readByte()) {
				case CONSTRAINT_IK: {
					IkConstraintData *data = new (__FILE__, __LINE__) IkConstraintData(name);
					Array<BoneData *> &constraintBones = data->_bones.setSize(nn = input.readInt(true), NULL);
					for (int ii = 0; ii < nn; ii++) constraintBones[ii] = bones[input.readInt(true)];
					data->_target = bones[input.readInt(true)];
					int flags = input.read();
					data->_skinRequired = (flags & 1) != 0;
					if ((flags & 2) != 0) data->_scaleYMode = static_cast<ScaleYMode>(input.read());
					IkConstraintPose &setup = data->_setupPose;
					setup._bendDirection = (flags & 4) != 0 ? -1 : 1;
					setup._compress = (flags & 8) != 0;
					setup._stretch = (flags & 16) != 0;
					if ((flags & 32) != 0) setup._mix = (flags & 64) != 0 ? input.readFloat() : 1;
					if ((flags & 128) != 0) setup._softness = input.readFloat() * _scale;
					constraints[i] = data;
					break;
				}
				case CONSTRAINT_TRANSFORM: {
					TransformConstraintData *data = new (__FILE__, __LINE__) TransformConstraintData(name);
					Array<BoneData *> &constraintBones = data->_bones.setSize(nn = input.readInt(true), NULL);
					for (int ii = 0; ii < nn; ii++) constraintBones[ii] = bones[input.readInt(true)];
					data->_source = bones[input.readInt(true)];
					int flags = input.read();
					data->_skinRequired = (flags & 1) != 0;
					data->_localSource = (flags & 2) != 0;
					data->_localTarget = (flags & 4) != 0;
					data->_additive = (flags & 8) != 0;
					data->_clamp = (flags & 16) != 0;
					Array<FromProperty *> &froms = data->_properties.setSize(nn = flags >> 5, NULL);
					for (int ii = 0, tn; ii < nn; ii++) {
						float fromScale = 1;
						FromProperty *from = NULL;
						switch (input.readByte()) {
							case 0:
								from = new (__FILE__, __LINE__) FromRotate();
								break;
							case 1:
								fromScale = _scale;
								from = new (__FILE__, __LINE__) FromX();
								break;
							case 2:
								fromScale = _scale;
								from = new (__FILE__, __LINE__) FromY();
								break;
							case 3:
								from = new (__FILE__, __LINE__) FromScaleX();
								break;
							case 4:
								from = new (__FILE__, __LINE__) FromScaleY();
								break;
							case 5:
								from = new (__FILE__, __LINE__) FromShearY();
								break;
						}
						from->_offset = input.readFloat() * fromScale;
						Array<ToProperty *> &tos = from->_to.setSize(tn = input.readByte(), NULL);
						for (int t = 0; t < tn; t++) {
							float toScale = 1;
							ToProperty *to = NULL;
							switch (input.readByte()) {
								case 0:
									to = new (__FILE__, __LINE__) ToRotate();
									break;
								case 1:
									toScale = _scale;
									to = new (__FILE__, __LINE__) ToX();
									break;
								case 2:
									toScale = _scale;
									to = new (__FILE__, __LINE__) ToY();
									break;
								case 3:
									to = new (__FILE__, __LINE__) ToScaleX();
									break;
								case 4:
									to = new (__FILE__, __LINE__) ToScaleY();
									break;
								case 5:
									to = new (__FILE__, __LINE__) ToShearY();
									break;
							}
							to->_offset = input.readFloat() * toScale;
							to->_max = input.readFloat() * toScale;
							to->_scale = input.readFloat() * toScale / fromScale;
							tos[t] = to;
						}
						froms[ii] = from;
					}
					flags = input.read();
					if ((flags & 1) != 0) data->_offsets[TransformConstraintData::ROTATION] = input.readFloat();
					if ((flags & 2) != 0) data->_offsets[TransformConstraintData::X] = input.readFloat() * _scale;
					if ((flags & 4) != 0) data->_offsets[TransformConstraintData::Y] = input.readFloat() * _scale;
					if ((flags & 8) != 0) data->_offsets[TransformConstraintData::SCALEX] = input.readFloat();
					if ((flags & 16) != 0) data->_offsets[TransformConstraintData::SCALEY] = input.readFloat();
					if ((flags & 32) != 0) data->_offsets[TransformConstraintData::SHEARY] = input.readFloat();
					flags = input.read();
					TransformConstraintPose &setup = data->_setupPose;
					if ((flags & 1) != 0) setup._mixRotate = input.readFloat();
					if ((flags & 2) != 0) setup._mixX = input.readFloat();
					if ((flags & 4) != 0) setup._mixY = input.readFloat();
					if ((flags & 8) != 0) setup._mixScaleX = input.readFloat();
					if ((flags & 16) != 0) setup._mixScaleY = input.readFloat();
					if ((flags & 32) != 0) setup._mixShearY = input.readFloat();
					constraints[i] = data;
					break;
				}
				case CONSTRAINT_PATH: {
					PathConstraintData *data = new (__FILE__, __LINE__) PathConstraintData(name);
					Array<BoneData *> &constraintBones = data->_bones.setSize(nn = input.readInt(true), NULL);
					for (int ii = 0; ii < nn; ii++) constraintBones[ii] = bones[input.readInt(true)];
					data->_slot = slots[input.readInt(true)];
					int flags = input.read();
					data->_skinRequired = (flags & 1) != 0;
					data->_positionMode = (PositionMode) ((flags >> 1) & 1);
					data->_spacingMode = (SpacingMode) ((flags >> 2) & 3);
					data->_rotateMode = (RotateMode) ((flags >> 4) & 3);
					if ((flags & 128) != 0) data->_offsetRotation = input.readFloat();
					PathConstraintPose &setup = data->_setupPose;
					setup._position = input.readFloat();
					if (data->_positionMode == PositionMode_Fixed) setup._position *= _scale;
					setup._spacing = input.readFloat();
					if (data->_spacingMode == SpacingMode_Length || data->_spacingMode == SpacingMode_Fixed) setup._spacing *= _scale;
					setup._mixRotate = input.readFloat();
					setup._mixX = input.readFloat();
					setup._mixY = input.readFloat();
					constraints[i] = data;
					break;
				}
				case CONSTRAINT_PHYSICS: {
					PhysicsConstraintData *data = new (__FILE__, __LINE__) PhysicsConstraintData(name);
					data->_bone = bones[input.readInt(true)];
					int flags = input.read();
					data->_skinRequired = (flags & 1) != 0;
					if ((flags & 2) != 0) data->_x = input.readFloat();
					if ((flags & 4) != 0) data->_y = input.readFloat();
					if ((flags & 8) != 0) data->_rotate = input.readFloat();
					if ((flags & 16) != 0) {
						float scaleX = input.readFloat();
						if (scaleX < -2) {
							data->_scaleYMode = ScaleYMode_Volume;
							scaleX = -2 - scaleX;
						} else if (scaleX < 0) {
							data->_scaleYMode = ScaleYMode_Uniform;
							scaleX = -1 - scaleX;
						}
						data->_scaleX = scaleX;
					}
					if ((flags & 32) != 0) data->_shearX = input.readFloat();
					data->_limit = ((flags & 64) != 0 ? input.readFloat() : 5000) * _scale;
					data->_step = 1.f / input.readUnsignedByte();
					PhysicsConstraintPose &setup = data->getSetupPose();
					setup._inertia = input.readFloat();
					setup._strength = input.readFloat();
					setup._damping = input.readFloat();
					setup._massInverse = (flags & 128) != 0 ? input.readFloat() : 1;
					setup._wind = input.readFloat();
					setup._gravity = input.readFloat();
					flags = input.read();
					if ((flags & 1) != 0) data->_inertiaGlobal = true;
					if ((flags & 2) != 0) data->_strengthGlobal = true;
					if ((flags & 4) != 0) data->_dampingGlobal = true;
					if ((flags & 8) != 0) data->_massGlobal = true;
					if ((flags & 16) != 0) data->_windGlobal = true;
					if ((flags & 32) != 0) data->_gravityGlobal = true;
					if ((flags & 64) != 0) data->_mixGlobal = true;
					setup._mix = (flags & 128) != 0 ? input.readFloat() : 1;
					constraints[i] = data;
					break;
				}
				case CONSTRAINT_SLIDER: {
					SliderData *data = new (__FILE__, __LINE__) SliderData(name);
					int flags = input.read();
					data->_skinRequired = (flags & 1) != 0;
					data->_loop = (flags & 2) != 0;
					data->_additive = (flags & 4) != 0;
					SliderPose &setup = data->_setupPose;
					if ((flags & 8) != 0) {
						float value = input.readFloat();
						if (nonessential && (flags & 64) != 0)
							data->_max = value;
						else
							setup._time = value;
					}
					if ((flags & 16) != 0) setup._mix = (flags & 32) != 0 ? input.readFloat() : 1;
					if ((flags & 64) != 0) {
						data->_local = (flags & 128) != 0;
						data->_bone = bones[input.readInt(true)];
						float offset = input.readFloat();
						float propertyScale = 1;
						switch (input.readByte()) {
							case 0:
								data->_property = new (__FILE__, __LINE__) FromRotate();
								break;
							case 1:
								propertyScale = _scale;
								data->_property = new (__FILE__, __LINE__) FromX();
								break;
							case 2:
								propertyScale = _scale;
								data->_property = new (__FILE__, __LINE__) FromY();
								break;
							case 3:
								data->_property = new (__FILE__, __LINE__) FromScaleX();
								break;
							case 4:
								data->_property = new (__FILE__, __LINE__) FromScaleY();
								break;
							case 5:
								data->_property = new (__FILE__, __LINE__) FromShearY();
								break;
							default:
								data->_property = NULL;
								break;
						}
						if (data->_property) data->_property->_offset = offset * propertyScale;
						data->_offset = input.readFloat();
						data->_scale = input.readFloat() / propertyScale;
					}
					constraints[i] = data;
					break;
				}
			}
		}

		/* Default skin. */
		Skin *defaultSkin = readSkin(input, *skeletonData, true, nonessential);
		if (defaultSkin) {
			skeletonData->_defaultSkin = defaultSkin;
			skeletonData->_skins.add(defaultSkin);
		}

		if (!this->getError().isEmpty()) {
			delete skeletonData;
			return NULL;
		}

		/* Skins. */
		{
			int i = (int) skeletonData->_skins.size();
			Array<Skin *> &skins = skeletonData->_skins.setSize(n = i + input.readInt(true), NULL);
			for (; i < n; i++) {
				Skin *skin = readSkin(input, *skeletonData, false, nonessential);
				if (skin)
					skins[i] = skin;
				else {
					delete skeletonData;
					return NULL;
				}
			}
		}

		/* Linked meshes. */
		Array<LinkedMesh *> &items = _linkedMeshes;
		n = (int) items.size();
		for (int i = 0; i < n; i++) {
			LinkedMesh *linkedMesh = items[i];
			Skin *skin = skeletonData->_skins[linkedMesh->_skinIndex];
			Attachment *source = skin->getAttachment(linkedMesh->_sourceIndex, linkedMesh->_source);
			if (source == NULL) {
				delete skeletonData;
				setError("Source mesh not found: ", linkedMesh->_source.buffer());
				return NULL;
			}
			linkedMesh->_mesh->setTimelineAttachment(linkedMesh->_inheritTimelines ? source : linkedMesh->_mesh);
			linkedMesh->_mesh->setSourceMesh(static_cast<MeshAttachment *>(source));
			linkedMesh->_mesh->updateSequence();
		}
		ArrayUtils::deleteElements(_linkedMeshes);
		_linkedMeshes.clear();

		/* Events. */
		int eventsCount = input.readInt(true);
		Array<EventData *> &events = skeletonData->_events.setSize(eventsCount, NULL);
		for (int i = 0; i < eventsCount; ++i) {
			EventData *eventData = new (__FILE__, __LINE__) EventData(String(input.readString(), true));
			Event &setup = eventData->_setupPose;
			setup._intValue = input.readInt(false);
			setup._floatValue = input.readFloat();
			setup._stringValue.own(input.readString());
			eventData->_audioPath.own(input.readString());
			if (!eventData->_audioPath.isEmpty()) {
				setup._volume = input.readFloat();
				setup._balance = input.readFloat();
			}
			events[i] = eventData;
		}

		/* Animations. */
		int animationsCount = input.readInt(true);
		Array<Animation *> &animations = skeletonData->_animations.setSize(animationsCount, NULL);
		for (int i = 0; i < animationsCount; ++i) {
			Animation *animation = readAnimation(input, String(input.readString(), true), *skeletonData, nonessential);
			if (!animation) {
				delete skeletonData;
				setError("Error reading animation: ", input.readString());
				return NULL;
			}
			animations[i] = animation;
		}

		for (int i = 0; i < constraintCount; i++) {
			if (constraints[i]->getRTTI().instanceOf(SliderData::rtti)) {
				SliderData *data = static_cast<SliderData *>(constraints[i]);
				data->setAnimation(*animations[input.readInt(true)]);
			}
		}
	}

	return skeletonData;
}

void SkeletonBinary::setError(const char *value1, const char *value2) {
	char message[256];
	int length;
	strcpy(message, value1);
	length = (int) strlen(value1);
	if (value2) strncat(message + length, value2, 255 - length);
	_error = String(message);
}

Skin *SkeletonBinary::readSkin(DataInput &input, SkeletonData &skeletonData, bool defaultSkin, bool nonessential) {
	Skin *skin;
	int slotCount = 0;
	if (defaultSkin) {
		slotCount = input.readInt(true);
		if (slotCount == 0) return NULL;
		skin = new (__FILE__, __LINE__) Skin("default");
	} else {
		skin = new (__FILE__, __LINE__) Skin(String(input.readString(), true));

		if (nonessential) Color::rgba8888ToColor(skin->getColor(), input.readInt());

		int n;
		Array<BoneData *> &from = skeletonData._bones;
		Array<BoneData *> &bones = skin->getBones().setSize(n = input.readInt(true), NULL);
		for (int i = 0; i < n; i++) bones[i] = from[input.readInt(true)];

		Array<ConstraintData *> &fromConstraints = skeletonData._constraints;
		Array<ConstraintData *> &constraints = skin->getConstraints().setSize(n = input.readInt(true), NULL);
		for (int i = 0; i < n; i++) constraints[i] = fromConstraints[input.readInt(true)];

		slotCount = input.readInt(true);
	}

	for (int i = 0; i < slotCount; ++i) {
		int slotIndex = input.readInt(true);
		for (int ii = 0, nn = input.readInt(true); ii < nn; ++ii) {
			String placeholder(input.readStringRef());
			Attachment *attachment = readAttachment(input, *skin, slotIndex, placeholder, skeletonData, nonessential);
			if (attachment)
				skin->setAttachment(slotIndex, placeholder, attachment);
			else {
				setError("Error reading attachment: ", placeholder.buffer());
				delete skin;
				return NULL;
			}
		}
	}
	return skin;
}

Attachment *SkeletonBinary::readAttachment(DataInput &input, Skin &skin, int slotIndex, const String &placeholder, SkeletonData &skeletonData,
										   bool nonessential) {
	float scale = _scale;

	int flags = input.readByte();
	String name = (flags & 8) != 0 ? input.readStringRef() : placeholder;
	AttachmentType type = static_cast<AttachmentType>(flags & 0x7);
	switch (type) {
		case AttachmentType_Region: {
			String path = (flags & 16) != 0 ? input.readStringRef() : name;
			int color = (flags & 32) != 0 ? input.readInt() : 0xffffffff;
			Sequence *sequence = readSequence(input, (flags & 64) != 0);
			float rotation = (flags & 128) != 0 ? input.readFloat() : 0;
			float x = input.readFloat();
			float y = input.readFloat();
			float scaleX = input.readFloat();
			float scaleY = input.readFloat();
			float width = input.readFloat();
			float height = input.readFloat();

			RegionAttachment *region = _attachmentLoader->newRegionAttachment(skin, placeholder, name, path, sequence);
			if (!region) return NULL;
			region->setPath(path);
			region->setX(x * scale);
			region->setY(y * scale);
			region->setScaleX(scaleX);
			region->setScaleY(scaleY);
			region->setRotation(rotation);
			region->setWidth(width * scale);
			region->setHeight(height * scale);
			Color::rgba8888ToColor(region->getColor(), color);
			region->updateSequence();
			return region;
		}
		case AttachmentType_Boundingbox: {
			Array<float> vertices;
			Array<int> bones;
			int verticesLength = readVertices(input, vertices, bones, (flags & 16) != 0);
			int color = nonessential ? input.readInt() : 0;

			BoundingBoxAttachment *box = _attachmentLoader->newBoundingBoxAttachment(skin, placeholder, name);
			if (!box) return NULL;
			box->setWorldVerticesLength(verticesLength);
			box->setVertices(vertices);
			box->setBones(bones);
			if (nonessential) Color::rgba8888ToColor(box->getColor(), color);
			return box;
		}
		case AttachmentType_Mesh: {
			String path = (flags & 16) != 0 ? input.readStringRef() : name;
			int color = (flags & 32) != 0 ? input.readInt() : 0xffffffff;
			Sequence *sequence = readSequence(input, (flags & 64) != 0);
			int hullLength = input.readInt(true);
			Array<float> vertices;
			Array<int> bones;
			int verticesLength = readVertices(input, vertices, bones, (flags & 128) != 0);
			Array<float> uvs;
			readFloatArray(input, verticesLength, 1, uvs);
			Array<unsigned short> triangles;
			readUnsignedShortArray(input, triangles, (verticesLength - hullLength - 2) * 3);

			Array<int> timelineSlots;
			timelineSlots.setSize(input.readInt(true), 0);
			for (size_t i = 0; i < timelineSlots.size(); ++i) timelineSlots[i] = input.readInt(true);

			Array<unsigned short> edges;
			float width = 0, height = 0;
			if (nonessential) {
				readUnsignedShortArray(input, edges, input.readInt(true));
				width = input.readFloat();
				height = input.readFloat();
			}

			MeshAttachment *mesh = _attachmentLoader->newMeshAttachment(skin, placeholder, name, path, sequence);
			if (!mesh) return NULL;
			mesh->setPath(path);
			Color::rgba8888ToColor(mesh->getColor(), color);
			mesh->setHullLength(hullLength << 1);
			mesh->setBones(bones);
			mesh->setVertices(vertices);
			mesh->setWorldVerticesLength(verticesLength);
			mesh->setRegionUVs(uvs);
			mesh->setTriangles(triangles);
			if (timelineSlots.size() > 0) mesh->setTimelineSlots(timelineSlots);
			if (nonessential) {
				mesh->setEdges(edges);
				mesh->setWidth(width * scale);
				mesh->setHeight(height * scale);
			}
			mesh->updateSequence();
			return mesh;
		}
		case AttachmentType_Linkedmesh: {
			String path = (flags & 16) != 0 ? input.readStringRef() : name;
			int color = (flags & 32) != 0 ? input.readInt() : 0xffffffff;
			Sequence *sequence = readSequence(input, (flags & 64) != 0);
			bool inheritTimelines = (flags & 128) != 0;
			int sourceIndex = input.readInt(true);
			int skinIndex = input.readInt(true);
			String source = input.readStringRef();
			float width = 0, height = 0;
			if (nonessential) {
				width = input.readFloat();
				height = input.readFloat();
			}

			MeshAttachment *mesh = _attachmentLoader->newMeshAttachment(skin, placeholder, name, path, sequence);
			if (!mesh) return NULL;
			mesh->setPath(path);
			Color::rgba8888ToColor(mesh->getColor(), color);
			if (nonessential) {
				mesh->setWidth(width * scale);
				mesh->setHeight(height * scale);
			}
			_linkedMeshes.add(new (__FILE__, __LINE__) LinkedMesh(*mesh, skinIndex, slotIndex, sourceIndex, source, inheritTimelines));
			return mesh;
		}
		case AttachmentType_Path: {
			bool closed = (flags & 16) != 0;
			bool constantSpeed = (flags & 32) != 0;
			Array<float> vertices;
			Array<int> bones;
			int verticesLength = readVertices(input, vertices, bones, (flags & 64) != 0);
			Array<float> lengths;
			readFloatArray(input, verticesLength / 6, scale, lengths);
			int color = nonessential ? input.readInt() : 0;

			PathAttachment *path = _attachmentLoader->newPathAttachment(skin, placeholder, name);
			if (!path) return NULL;
			path->setClosed(closed);
			path->setConstantSpeed(constantSpeed);
			path->setWorldVerticesLength(verticesLength);
			path->setVertices(vertices);
			path->setBones(bones);
			path->setLengths(lengths);
			if (nonessential) Color::rgba8888ToColor(path->getColor(), color);
			return path;
		}
		case AttachmentType_Point: {
			float rotation = input.readFloat();
			float x = input.readFloat();
			float y = input.readFloat();
			int color = nonessential ? input.readInt() : 0;

			PointAttachment *point = _attachmentLoader->newPointAttachment(skin, placeholder, name);
			if (!point) return NULL;
			point->setX(x * scale);
			point->setY(y * scale);
			point->setRotation(rotation);
			if (nonessential) Color::rgba8888ToColor(point->getColor(), color);
			return point;
		}
		case AttachmentType_Clipping: {
			int endSlotIndex = input.readInt(true);
			Array<float> vertices;
			Array<int> bones;
			int verticesLength = readVertices(input, vertices, bones, (flags & 16) != 0);
			int color = nonessential ? input.readInt() : 0;

			ClippingAttachment *clip = _attachmentLoader->newClippingAttachment(skin, placeholder, name);
			if (!clip) return NULL;
			clip->setEndSlot(skeletonData._slots[endSlotIndex]);
			clip->setConvex((flags & 32) != 0);
			clip->setInverse((flags & 64) != 0);
			clip->setWorldVerticesLength(verticesLength);
			clip->setVertices(vertices);
			clip->setBones(bones);
			if (nonessential) Color::rgba8888ToColor(clip->getColor(), color);
			return clip;
		}
	}
	return NULL;
}

Sequence *SkeletonBinary::readSequence(DataInput &input, bool hasPathSuffix) {
	if (!hasPathSuffix) return new (__FILE__, __LINE__) Sequence(1, false);
	Sequence *sequence = new (__FILE__, __LINE__) Sequence(input.readInt(true), true);
	sequence->setStart(input.readInt(true));
	sequence->setDigits(input.readInt(true));
	sequence->setSetupIndex(input.readInt(true));
	return sequence;
}

int SkeletonBinary::readVertices(DataInput &input, Array<float> &vertices, Array<int> &bones, bool weighted) {
	float scale = _scale;
	int vertexCount = input.readInt(true);
	int verticesLength = vertexCount << 1;
	if (!weighted) {
		readFloatArray(input, verticesLength, scale, vertices.setSize(verticesLength, 0));
		return verticesLength;
	}
	int n = input.readInt(true);
	bones.setSize(n, 0);
	vertices.setSize((n - vertexCount) * 3, 0);
	for (int b = 0, w = 0; b < n;) {
		int boneCount = input.readInt(true);
		bones[b++] = boneCount;
		for (int ii = 0; ii < boneCount; ++ii, w += 3) {
			bones[b++] = input.readInt(true);
			vertices[w] = input.readFloat() * scale;
			vertices[w + 1] = input.readFloat() * scale;
			vertices[w + 2] = input.readFloat();
		}
	}
	return verticesLength;
}

void SkeletonBinary::readFloatArray(DataInput &input, int n, float scale, Array<float> &array) {
	array.setSize(n, 0);
	int i;
	if (scale == 1) {
		for (i = 0; i < n; ++i) {
			array[i] = input.readFloat();
		}
	} else {
		for (i = 0; i < n; ++i) {
			array[i] = input.readFloat() * scale;
		}
	}
}

void SkeletonBinary::readUnsignedShortArray(DataInput &input, Array<unsigned short> &array, int n) {
	array.setSize(n, 0);
	for (int i = 0; i < n; ++i) {
		array[i] = (unsigned short) input.readInt(true);
	}
}

Animation *SkeletonBinary::readAnimation(DataInput &input, const String &name, SkeletonData &skeletonData, bool nonessential) {
	Array<Timeline *> timelines;
	Array<int> bones;
	timelines.ensureCapacity(input.readInt(true));
	float scale = _scale;

	// Slot timelines.
	for (int i = 0, n = input.readInt(true); i < n; ++i) {
		int slotIndex = input.readInt(true);
		for (int ii = 0, nn = input.readInt(true); ii < nn; ++ii) {
			int timelineType = input.readByte(), frameCount = input.readInt(true), frameLast = frameCount - 1;
			switch (timelineType) {
				case SLOT_ATTACHMENT: {
					AttachmentTimeline *timeline = new (__FILE__, __LINE__) AttachmentTimeline(frameCount, slotIndex);
					for (int frame = 0; frame < frameCount; ++frame) {
						float time = input.readFloat();
						char *attachmentName = input.readStringRef();
						timeline->setFrame(frame, time, attachmentName);
					}
					timelines.add(timeline);
					break;
				}
				case SLOT_RGBA: {
					RGBATimeline *timeline = new (__FILE__, __LINE__) RGBATimeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat();
					float r = input.read() / 255.0f, g = input.read() / 255.0f;
					float b = input.read() / 255.0f, a = input.read() / 255.0f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline->setFrame(frame, time, r, g, b, a);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float r2 = input.read() / 255.0f, g2 = input.read() / 255.0f;
						float b2 = input.read() / 255.0f, a2 = input.read() / 255.0f;
						int curveType = input.readByte();
						switch (curveType) {
							case CURVE_STEPPED:
								timeline->setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, *timeline, bezier++, frame, 0, time, time2, r, r2, 1);
								setBezier(input, *timeline, bezier++, frame, 1, time, time2, g, g2, 1);
								setBezier(input, *timeline, bezier++, frame, 2, time, time2, b, b2, 1);
								setBezier(input, *timeline, bezier++, frame, 3, time, time2, a, a2, 1);
								break;
						}
						time = time2;
						r = r2;
						g = g2;
						b = b2;
						a = a2;
					}
					timelines.add(timeline);
					break;
				}
				case SLOT_RGB: {
					RGBTimeline *timeline = new (__FILE__, __LINE__) RGBTimeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat();
					float r = input.read() / 255.0f, g = input.read() / 255.0f, b = input.read() / 255.0f;

					for (int frame = 0, bezier = 0;; frame++) {
						timeline->setFrame(frame, time, r, g, b);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float r2 = input.read() / 255.0f, g2 = input.read() / 255.0f, b2 = input.read() / 255.0f;
						int curveType = input.readByte();
						switch (curveType) {
							case CURVE_STEPPED:
								timeline->setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, *timeline, bezier++, frame, 0, time, time2, r, r2, 1);
								setBezier(input, *timeline, bezier++, frame, 1, time, time2, g, g2, 1);
								setBezier(input, *timeline, bezier++, frame, 2, time, time2, b, b2, 1);
								break;
						}
						time = time2;
						r = r2;
						g = g2;
						b = b2;
					}
					timelines.add(timeline);
					break;
				}
				case SLOT_RGBA2: {
					RGBA2Timeline *timeline = new (__FILE__, __LINE__) RGBA2Timeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat();
					float r = input.read() / 255.0f, g = input.read() / 255.0f;
					float b = input.read() / 255.0f, a = input.read() / 255.0f;
					float r2 = input.read() / 255.0f, g2 = input.read() / 255.0f, b2 = input.read() / 255.0f;

					for (int frame = 0, bezier = 0;; frame++) {
						timeline->setFrame(frame, time, r, g, b, a, r2, g2, b2);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float nr = input.read() / 255.0f, ng = input.read() / 255.0f;
						float nb = input.read() / 255.0f, na = input.read() / 255.0f;
						float nr2 = input.read() / 255.0f, ng2 = input.read() / 255.0f, nb2 = input.read() / 255.0f;
						int curveType = input.readByte();
						switch (curveType) {
							case CURVE_STEPPED:
								timeline->setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, *timeline, bezier++, frame, 0, time, time2, r, nr, 1);
								setBezier(input, *timeline, bezier++, frame, 1, time, time2, g, ng, 1);
								setBezier(input, *timeline, bezier++, frame, 2, time, time2, b, nb, 1);
								setBezier(input, *timeline, bezier++, frame, 3, time, time2, a, na, 1);
								setBezier(input, *timeline, bezier++, frame, 4, time, time2, r2, nr2, 1);
								setBezier(input, *timeline, bezier++, frame, 5, time, time2, g2, ng2, 1);
								setBezier(input, *timeline, bezier++, frame, 6, time, time2, b2, nb2, 1);
								break;
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						a = na;
						r2 = nr2;
						g2 = ng2;
						b2 = nb2;
					}
					timelines.add(timeline);
					break;
				}
				case SLOT_RGB2: {
					RGB2Timeline *timeline = new (__FILE__, __LINE__) RGB2Timeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat();
					float r = input.read() / 255.0f, g = input.read() / 255.0f, b = input.read() / 255.0f;
					float r2 = input.read() / 255.0f, g2 = input.read() / 255.0f, b2 = input.read() / 255.0f;

					for (int frame = 0, bezier = 0;; frame++) {
						timeline->setFrame(frame, time, r, g, b, r2, g2, b2);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float nr = input.read() / 255.0f, ng = input.read() / 255.0f, nb = input.read() / 255.0f;
						float nr2 = input.read() / 255.0f, ng2 = input.read() / 255.0f, nb2 = input.read() / 255.0f;
						int curveType = input.readByte();
						switch (curveType) {
							case CURVE_STEPPED:
								timeline->setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, *timeline, bezier++, frame, 0, time, time2, r, nr, 1);
								setBezier(input, *timeline, bezier++, frame, 1, time, time2, g, ng, 1);
								setBezier(input, *timeline, bezier++, frame, 2, time, time2, b, nb, 1);
								setBezier(input, *timeline, bezier++, frame, 3, time, time2, r2, nr2, 1);
								setBezier(input, *timeline, bezier++, frame, 4, time, time2, g2, ng2, 1);
								setBezier(input, *timeline, bezier++, frame, 5, time, time2, b2, nb2, 1);
								break;
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						r2 = nr2;
						g2 = ng2;
						b2 = nb2;
					}
					timelines.add(timeline);
					break;
				}
				case SLOT_ALPHA: {
					AlphaTimeline *timeline = new (__FILE__, __LINE__) AlphaTimeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat(), a = input.read() / 255.0f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline->setFrame(frame, time, a);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float a2 = input.read() / 255.0f;
						int curveType = input.readByte();
						switch (curveType) {
							case CURVE_STEPPED:
								timeline->setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, *timeline, bezier++, frame, 0, time, time2, a, a2, 1);
								break;
						}
						time = time2;
						a = a2;
					}
					timelines.add(timeline);
					break;
				}
				default: {
					ArrayUtils::deleteElements(timelines);
					setError("Invalid slot timeline type: ", String().append(timelineType).buffer());
					return NULL;
				}
			}
		}
	}

	// Bone timelines.
	int boneCount = input.readInt(true);
	bones.ensureCapacity(boneCount);
	for (int i = 0; i < boneCount; ++i) {
		int boneIndex = input.readInt(true);
		bones.add(boneIndex);
		for (int ii = 0, nn = input.readInt(true); ii < nn; ++ii) {
			int timelineType = input.readByte(), frameCount = input.readInt(true);
			if (timelineType == BONE_INHERIT) {
				InheritTimeline *timeline = new (__FILE__, __LINE__) InheritTimeline(frameCount, boneIndex);
				for (int frame = 0; frame < frameCount; frame++) {
					float time = input.readFloat();
					Inherit inherit = (Inherit) input.readByte();
					timeline->setFrame(frame, time, inherit);
				}
				timelines.add(timeline);
				continue;
			}
			int bezierCount = input.readInt(true);
			switch (timelineType) {
				case BONE_ROTATE:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) RotateTimeline(frameCount, bezierCount, boneIndex)), 1);
					break;
				case BONE_TRANSLATE:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) TranslateTimeline(frameCount, bezierCount, boneIndex)), scale);
					break;
				case BONE_TRANSLATEX:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) TranslateXTimeline(frameCount, bezierCount, boneIndex)), scale);
					break;
				case BONE_TRANSLATEY:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) TranslateYTimeline(frameCount, bezierCount, boneIndex)), scale);
					break;
				case BONE_SCALE:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) ScaleTimeline(frameCount, bezierCount, boneIndex)), 1);
					break;
				case BONE_SCALEX:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) ScaleXTimeline(frameCount, bezierCount, boneIndex)), 1);
					break;
				case BONE_SCALEY:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) ScaleYTimeline(frameCount, bezierCount, boneIndex)), 1);
					break;
				case BONE_SHEAR:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) ShearTimeline(frameCount, bezierCount, boneIndex)), 1);
					break;
				case BONE_SHEARX:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) ShearXTimeline(frameCount, bezierCount, boneIndex)), 1);
					break;
				case BONE_SHEARY:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) ShearYTimeline(frameCount, bezierCount, boneIndex)), 1);
					break;
				default: {
					ArrayUtils::deleteElements(timelines);
					setError("Invalid bone timeline type: ", String().append(timelineType).buffer());
					return NULL;
				}
			}
		}
	}

	// IK constraint timelines.
	for (int i = 0, n = input.readInt(true); i < n; i++) {
		int index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
		IkConstraintTimeline *timeline = new (__FILE__, __LINE__) IkConstraintTimeline(frameCount, input.readInt(true), index);
		int flags = input.read();
		float time = input.readFloat(), mix = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1) : 0;
		float softness = (flags & 4) != 0 ? input.readFloat() * scale : 0;
		for (int frame = 0, bezier = 0;; frame++) {
			timeline->setFrame(frame, time, mix, softness, (flags & 8) != 0 ? 1 : -1, (flags & 16) != 0, (flags & 32) != 0);
			if (frame == frameLast) break;
			flags = input.read();
			float time2 = input.readFloat(), mix2 = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1) : 0;
			float softness2 = (flags & 4) != 0 ? input.readFloat() * scale : 0;
			if ((flags & 64) != 0)
				timeline->setStepped(frame);
			else if ((flags & 128) != 0) {
				setBezier(input, *timeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
				setBezier(input, *timeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
			}
			time = time2;
			mix = mix2;
			softness = softness2;
		}
		timelines.add(timeline);
	}

	// Transform constraint timelines.
	for (int i = 0, n = input.readInt(true); i < n; ++i) {
		int index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
		TransformConstraintTimeline *timeline = new (__FILE__, __LINE__) TransformConstraintTimeline(frameCount, input.readInt(true), index);
		float time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat(),
			  mixScaleX = input.readFloat(), mixScaleY = input.readFloat(), mixShearY = input.readFloat();
		for (int frame = 0, bezier = 0;; frame++) {
			timeline->setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
			if (frame == frameLast) break;
			float time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(), mixY2 = input.readFloat(),
				  mixScaleX2 = input.readFloat(), mixScaleY2 = input.readFloat(), mixShearY2 = input.readFloat();
			int curveType = input.readByte();
			switch (curveType) {
				case CURVE_STEPPED:
					timeline->setStepped(frame);
					break;
				case CURVE_BEZIER:
					setBezier(input, *timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
					setBezier(input, *timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
					setBezier(input, *timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
					setBezier(input, *timeline, bezier++, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
					setBezier(input, *timeline, bezier++, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
					setBezier(input, *timeline, bezier++, frame, 5, time, time2, mixShearY, mixShearY2, 1);
					break;
			}
			time = time2;
			mixRotate = mixRotate2;
			mixX = mixX2;
			mixY = mixY2;
			mixScaleX = mixScaleX2;
			mixScaleY = mixScaleY2;
			mixShearY = mixShearY2;
		}
		timelines.add(timeline);
	}

	// Path constraint timelines.
	for (int i = 0, n = input.readInt(true); i < n; ++i) {
		int index = input.readInt(true);
		PathConstraintData *data = static_cast<PathConstraintData *>(skeletonData._constraints[index]);
		for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
			int type = input.readByte(), frameCount = input.readInt(true), bezierCount = input.readInt(true);
			switch (type) {
				case PATH_POSITION: {
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) PathConstraintPositionTimeline(frameCount, bezierCount, index)),
								 data->_positionMode == PositionMode_Fixed ? scale : 1);
					break;
				}
				case PATH_SPACING: {
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) PathConstraintSpacingTimeline(frameCount, bezierCount, index)),
								 data->_spacingMode == SpacingMode_Length || data->_spacingMode == SpacingMode_Fixed ? scale : 1);
					break;
				}
				case PATH_MIX: {
					PathConstraintMixTimeline *timeline = new (__FILE__, __LINE__) PathConstraintMixTimeline(frameCount, bezierCount, index);
					float time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat();
					for (int frame = 0, bezier = 0, frameLast = (int) timeline->getFrameCount() - 1;; frame++) {
						timeline->setFrame(frame, time, mixRotate, mixX, mixY);
						if (frame == frameLast) break;
						float time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(), mixY2 = input.readFloat();
						switch (input.readByte()) {
							case CURVE_STEPPED:
								timeline->setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, *timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
								setBezier(input, *timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
								setBezier(input, *timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
								break;
						}
						time = time2;
						mixRotate = mixRotate2;
						mixX = mixX2;
						mixY = mixY2;
					}
					timelines.add(timeline);
					break;
				}
				default: {
					ArrayUtils::deleteElements(timelines);
					setError("Invalid path constraint timeline type: ", String().append(type).buffer());
					return NULL;
				}
			}
		}
	}

	// Physics timelines.
	for (int i = 0, n = input.readInt(true); i < n; i++) {
		int index = input.readInt(true) - 1;
		for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
			int type = input.readByte(), frameCount = input.readInt(true);
			if (type == PHYSICS_RESET) {
				PhysicsConstraintResetTimeline *timeline = new (__FILE__, __LINE__) PhysicsConstraintResetTimeline(frameCount, index);
				for (int frame = 0; frame < frameCount; frame++) timeline->setFrame(frame, input.readFloat());
				timelines.add(timeline);
				continue;
			}
			int bezierCount = input.readInt(true);
			switch (type) {
				case PHYSICS_INERTIA:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) PhysicsConstraintInertiaTimeline(frameCount, bezierCount, index)), 1);
					break;
				case PHYSICS_STRENGTH:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) PhysicsConstraintStrengthTimeline(frameCount, bezierCount, index)), 1);
					break;
				case PHYSICS_DAMPING:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) PhysicsConstraintDampingTimeline(frameCount, bezierCount, index)), 1);
					break;
				case PHYSICS_MASS:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) PhysicsConstraintMassTimeline(frameCount, bezierCount, index)), 1);
					break;
				case PHYSICS_WIND:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) PhysicsConstraintWindTimeline(frameCount, bezierCount, index)), 1);
					break;
				case PHYSICS_GRAVITY:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) PhysicsConstraintGravityTimeline(frameCount, bezierCount, index)), 1);
					break;
				case PHYSICS_MIX:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) PhysicsConstraintMixTimeline(frameCount, bezierCount, index)), 1);
					break;
				default: {
					ArrayUtils::deleteElements(timelines);
					setError("Invalid physics constraint timeline type: ", String().append(type).buffer());
					return NULL;
				}
			}
		}
	}

	// Slider timelines.
	for (int i = 0, n = input.readInt(true); i < n; i++) {
		int index = input.readInt(true);
		for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
			int type = input.readByte(), frameCount = input.readInt(true), bezierCount = input.readInt(true);
			switch (type) {
				case SLIDER_TIME:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) SliderTimeline(frameCount, bezierCount, index)), 1);
					break;
				case SLIDER_MIX:
					readTimeline(input, timelines, *(new (__FILE__, __LINE__) SliderMixTimeline(frameCount, bezierCount, index)), 1);
					break;
				default: {
					ArrayUtils::deleteElements(timelines);
					setError("Invalid slider timeline type: ", String().append(type).buffer());
					return NULL;
				}
			}
		}
	}

	// Attachment timelines.
	for (int i = 0, n = input.readInt(true); i < n; ++i) {
		Skin *skin = skeletonData._skins[input.readInt(true)];
		for (int ii = 0, nn = input.readInt(true); ii < nn; ++ii) {
			int slotIndex = input.readInt(true);
			for (int iii = 0, nnn = input.readInt(true); iii < nnn; iii++) {
				const char *attachmentName = input.readStringRef();
				Attachment *attachment = skin->getAttachment(slotIndex, String(attachmentName));
				if (!attachment) {
					ArrayUtils::deleteElements(timelines);
					setError("Timeline attachment not found: ", attachmentName);
					return NULL;
				}
				int timelineType = input.readByte(), frameCount = input.readInt(true), frameLast = frameCount - 1;
				switch (timelineType) {
					case ATTACHMENT_DEFORM: {
						VertexAttachment *vertexAttachment = static_cast<VertexAttachment *>(attachment);
						bool weighted = vertexAttachment->_bones.size() > 0;
						Array<float> &vertices = vertexAttachment->_vertices;
						int deformLength = weighted ? (int) vertices.size() / 3 * 2 : (int) vertices.size();

						DeformTimeline *timeline = new (__FILE__, __LINE__)
							DeformTimeline(frameCount, input.readInt(true), slotIndex, *vertexAttachment);

						float time = input.readFloat();
						for (int frame = 0, bezier = 0;; ++frame) {
							Array<float> deform;
							size_t end = (size_t) input.readInt(true);
							if (end == 0) {
								if (weighted) {
									deform.setSize(deformLength, 0);
									for (int iiii = 0; iiii < deformLength; ++iiii) deform[iiii] = 0;
								} else {
									deform.clearAndAddAll(vertices);
								}
							} else {
								deform.setSize(deformLength, 0);
								size_t start = (size_t) input.readInt(true);
								end += start;
								if (scale == 1) {
									for (size_t v = start; v < end; ++v) deform[v] = input.readFloat();
								} else {
									for (size_t v = start; v < end; ++v) deform[v] = input.readFloat() * scale;
								}
								if (!weighted) {
									for (size_t v = 0, vn = deform.size(); v < vn; ++v) deform[v] += vertices[v];
								}
							}
							timeline->setFrame(frame, time, deform);
							if (frame == frameLast) break;
							float time2 = input.readFloat();
							switch (input.readByte()) {
								case CURVE_STEPPED:
									timeline->setStepped(frame);
									break;
								case CURVE_BEZIER:
									setBezier(input, *timeline, bezier++, frame, 0, time, time2, 0, 1, 1);
									break;
							}
							time = time2;
						}

						timelines.add(timeline);
						break;
					}
					case ATTACHMENT_SEQUENCE: {
						SequenceTimeline *timeline = new (__FILE__, __LINE__) SequenceTimeline(frameCount, slotIndex, *attachment);
						for (int frame = 0; frame < frameCount; frame++) {
							float time = input.readFloat();
							int modeAndIndex = input.readInt();
							float delay = input.readFloat();
							timeline->setFrame(frame, time, (SequenceMode) (modeAndIndex & 0xf), modeAndIndex >> 4, delay);
						}
						timelines.add(timeline);
						break;
					}
					default: {
						ArrayUtils::deleteElements(timelines);
						setError("Invalid attachment timeline type: ", String().append(timelineType).buffer());
						return NULL;
					}
				}
			}
		}
	}

	// Draw order timeline.
	size_t slotCount = skeletonData._slots.size();
	size_t drawOrderCount = (size_t) input.readInt(true);
	if (drawOrderCount > 0) {
		DrawOrderTimeline *timeline = new (__FILE__, __LINE__) DrawOrderTimeline(drawOrderCount);
		for (size_t i = 0; i < drawOrderCount; ++i) {
			float time = input.readFloat();
			Array<int> drawOrder;
			readDrawOrder(input, slotCount, drawOrder);
			timeline->setFrame(i, time, drawOrder.size() == 0 ? NULL : &drawOrder);
		}
		timelines.add(timeline);
	}

	// Draw order folder timelines.
	size_t folderCount = (size_t) input.readInt(true);
	for (size_t i = 0; i < folderCount; ++i) {
		size_t folderSlotCount = (size_t) input.readInt(true);
		Array<int> folderSlots;
		folderSlots.setSize(folderSlotCount, 0);
		for (size_t ii = 0; ii < folderSlotCount; ++ii) folderSlots[ii] = input.readInt(true);
		size_t keyCount = (size_t) input.readInt(true);
		DrawOrderFolderTimeline *timeline = new (__FILE__, __LINE__) DrawOrderFolderTimeline(keyCount, folderSlots, slotCount);
		for (size_t ii = 0; ii < keyCount; ++ii) {
			float time = input.readFloat();
			Array<int> drawOrder;
			readDrawOrder(input, folderSlotCount, drawOrder);
			timeline->setFrame(ii, time, drawOrder.size() == 0 ? NULL : &drawOrder);
		}
		timelines.add(timeline);
	}

	// Event timeline.
	int eventCount = input.readInt(true);
	if (eventCount > 0) {
		EventTimeline *timeline = new (__FILE__, __LINE__) EventTimeline(eventCount);
		for (int i = 0; i < eventCount; ++i) {
			float time = input.readFloat();
			EventData *eventData = skeletonData._events[input.readInt(true)];
			Event *event = new (__FILE__, __LINE__) Event(time, *eventData);
			event->_intValue = input.readInt(false);
			event->_floatValue = input.readFloat();
			const char *stringValue = input.readString();
			if (stringValue == NULL)
				event->_stringValue = eventData->_setupPose._stringValue;
			else
				event->_stringValue.own(stringValue);

			if (!eventData->_audioPath.isEmpty()) {
				event->_volume = input.readFloat();
				event->_balance = input.readFloat();
			}
			timeline->setFrame(i, *event);
		}
		timelines.add(timeline);
	}

	float duration = 0;
	for (int i = 0, n = (int) timelines.size(); i < n; i++) {
		duration = MathUtil::max(duration, (timelines[i])->getDuration());
	}
	Animation *animation = new (__FILE__, __LINE__) Animation(String(name));
	animation->setTimelines(timelines, bones);
	animation->setDuration(duration);
	if (nonessential) Color::rgba8888ToColor(animation->getColor(), input.readInt());
	return animation;
}

void SkeletonBinary::readTimeline(DataInput &input, Array<Timeline *> &timelines, CurveTimeline1 &timeline, float scale) {
	float time = input.readFloat(), value = input.readFloat() * scale;
	for (int frame = 0, bezier = 0, frameLast = (int) timeline.getFrameCount() - 1;; frame++) {
		timeline.setFrame(frame, time, value);
		if (frame == frameLast) break;
		float time2 = input.readFloat(), value2 = input.readFloat() * scale;
		switch (input.readByte()) {
			case CURVE_STEPPED:
				timeline.setStepped(frame);
				break;
			case CURVE_BEZIER:
				setBezier(input, timeline, bezier++, frame, 0, time, time2, value, value2, scale);
				break;
		}
		time = time2;
		value = value2;
	}
	timelines.add(&timeline);
}

void SkeletonBinary::readTimeline(DataInput &input, Array<Timeline *> &timelines, BoneTimeline2 &timeline, float scale) {
	float time = input.readFloat(), value1 = input.readFloat() * scale, value2 = input.readFloat() * scale;
	for (int frame = 0, bezier = 0, frameLast = (int) timeline.getFrameCount() - 1;; frame++) {
		timeline.setFrame(frame, time, value1, value2);
		if (frame == frameLast) break;
		float time2 = input.readFloat(), nvalue1 = input.readFloat() * scale, nvalue2 = input.readFloat() * scale;
		switch (input.readByte()) {
			case CURVE_STEPPED:
				timeline.setStepped(frame);
				break;
			case CURVE_BEZIER:
				setBezier(input, timeline, bezier++, frame, 0, time, time2, value1, nvalue1, scale);
				setBezier(input, timeline, bezier++, frame, 1, time, time2, value2, nvalue2, scale);
				break;
		}
		time = time2;
		value1 = nvalue1;
		value2 = nvalue2;
	}
	timelines.add(&timeline);
}

void SkeletonBinary::readDrawOrder(DataInput &input, size_t slotCount, Array<int> &drawOrder) {
	size_t changeCount = (size_t) input.readInt(true);
	drawOrder.clear();
	if (changeCount == 0) return;

	drawOrder.setSize(slotCount, 0);
	for (int i = (int) slotCount - 1; i >= 0; --i) drawOrder[i] = -1;
	Array<int> unchanged;
	unchanged.setSize(slotCount - changeCount, 0);
	size_t originalIndex = 0, unchangedIndex = 0;
	for (size_t i = 0; i < changeCount; ++i) {
		size_t slotIndex = (size_t) input.readInt(true);
		while (originalIndex != slotIndex) unchanged[unchangedIndex++] = (int) originalIndex++;
		size_t index = originalIndex;
		drawOrder[index + (size_t) input.readInt(true)] = (int) originalIndex++;
	}
	while (originalIndex < slotCount) unchanged[unchangedIndex++] = (int) originalIndex++;
	for (int i = (int) slotCount - 1; i >= 0; --i)
		if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
}

void SkeletonBinary::setBezier(DataInput &input, CurveTimeline &timeline, int bezier, int frame, int value, float time1, float time2, float value1,
							   float value2, float scale) {
	float cx1 = input.readFloat();
	float cy1 = input.readFloat();
	float cx2 = input.readFloat();
	float cy2 = input.readFloat();
	timeline.setBezier(bezier, frame, value, time1, value1, cx1, cy1 * scale, cx2, cy2 * scale, time2, value2);
}