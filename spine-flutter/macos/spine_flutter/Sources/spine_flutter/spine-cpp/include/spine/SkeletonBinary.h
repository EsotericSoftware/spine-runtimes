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

#ifndef Spine_SkeletonBinary_h
#define Spine_SkeletonBinary_h

#include <spine/Inherit.h>
#include <spine/Array.h>
#include <spine/SpineObject.h>
#include <spine/SpineString.h>
#include <spine/Color.h>
#include <spine/BoneTimeline.h>
#include <spine/SkeletonData.h>

namespace spine {
	class SkeletonData;

	class Atlas;

	class AttachmentLoader;

	class LinkedMesh;

	class Skin;

	class Attachment;

	class VertexAttachment;

	class Animation;

	class Timeline;

	class CurveTimeline;

	class CurveTimeline1;

	class BoneTimeline2;

	class Sequence;

	class SP_API SkeletonBinary : public SpineObject {
	public:
		static const int BONE_ROTATE = 0;
		static const int BONE_TRANSLATE = 1;
		static const int BONE_TRANSLATEX = 2;
		static const int BONE_TRANSLATEY = 3;
		static const int BONE_SCALE = 4;
		static const int BONE_SCALEX = 5;
		static const int BONE_SCALEY = 6;
		static const int BONE_SHEAR = 7;
		static const int BONE_SHEARX = 8;
		static const int BONE_SHEARY = 9;
		static const int BONE_INHERIT = 10;

		static const int SLOT_ATTACHMENT = 0;
		static const int SLOT_RGBA = 1;
		static const int SLOT_RGB = 2;
		static const int SLOT_RGBA2 = 3;
		static const int SLOT_RGB2 = 4;
		static const int SLOT_ALPHA = 5;

		static const int CONSTRAINT_IK = 0;
		static const int CONSTRAINT_PATH = 1;
		static const int CONSTRAINT_TRANSFORM = 2;
		static const int CONSTRAINT_PHYSICS = 3;
		static const int CONSTRAINT_SLIDER = 4;

		static const int ATTACHMENT_DEFORM = 0;
		static const int ATTACHMENT_SEQUENCE = 1;

		static const int PATH_POSITION = 0;
		static const int PATH_SPACING = 1;
		static const int PATH_MIX = 2;

		static const int PHYSICS_INERTIA = 0;
		static const int PHYSICS_STRENGTH = 1;
		static const int PHYSICS_DAMPING = 2;
		static const int PHYSICS_MASS = 4;
		static const int PHYSICS_WIND = 5;
		static const int PHYSICS_GRAVITY = 6;
		static const int PHYSICS_MIX = 7;
		static const int PHYSICS_RESET = 8;

		static const int SLIDER_TIME = 0;
		static const int SLIDER_MIX = 1;

		static const int CURVE_LINEAR = 0;
		static const int CURVE_STEPPED = 1;
		static const int CURVE_BEZIER = 2;

		explicit SkeletonBinary(Atlas &atlas);

		explicit SkeletonBinary(AttachmentLoader &attachmentLoader, bool ownsLoader = false);

		~SkeletonBinary();

		SkeletonData *readSkeletonData(const unsigned char *binary, int length);

		SkeletonData *readSkeletonDataFile(const String &path);

		void setScale(float scale) {
			_scale = scale;
		}

		const String &getError() const {
			return _error;
		}

	private:
		struct DataInput : public SpineObject {
			const unsigned char *cursor;
			const unsigned char *end;
			SkeletonData *skeletonData;

			DataInput(SkeletonData *skeletonData, const unsigned char *data, size_t length)
				: cursor(data), end(data + length), skeletonData(skeletonData) {
			}

			inline int read() {
				return readUnsignedByte();
			}

			inline signed char readByte() {
				return (signed char) *cursor++;
			}

			inline unsigned char readUnsignedByte() {
				return (unsigned char) *cursor++;
			}

			inline bool readBoolean() {
				return readByte() != 0;
			}

			inline int readInt() {
				int result = readUnsignedByte();
				result <<= 8;
				result |= readUnsignedByte();
				result <<= 8;
				result |= readUnsignedByte();
				result <<= 8;
				result |= readUnsignedByte();
				return result;
			}

			inline long long readLong() {
				unsigned long long result = (unsigned int) readInt();
				result <<= 32;
				result |= (unsigned int) readInt();
				return (long long) result;
			}

			inline float readFloat() {
				union {
					int intValue;
					float floatValue;
				} intToFloat;
				intToFloat.intValue = readInt();
				return intToFloat.floatValue;
			}

			inline void readColor(Color &color) {
				color.r = readUnsignedByte() / 255.0f;
				color.g = readUnsignedByte() / 255.0f;
				color.b = readUnsignedByte() / 255.0f;
				color.a = readUnsignedByte() / 255.0f;
			}

			inline int readInt(bool optimizePositive) {
				unsigned char b = readUnsignedByte();
				int value = b & 0x7F;
				if (b & 0x80) {
					b = readUnsignedByte();
					value |= (b & 0x7F) << 7;
					if (b & 0x80) {
						b = readUnsignedByte();
						value |= (b & 0x7F) << 14;
						if (b & 0x80) {
							b = readUnsignedByte();
							value |= (b & 0x7F) << 21;
							if (b & 0x80) value |= (readUnsignedByte() & 0x7F) << 28;
						}
					}
				}
				if (!optimizePositive) value = (((unsigned int) value >> 1) ^ -(value & 1));
				return value;
			}

			char *readString() {
				int length = readInt(true);
				char *string;
				if (length == 0) return NULL;
				string = SpineExtension::alloc<char>(length, __FILE__, __LINE__);
				memcpy(string, cursor, length - 1);
				cursor += length - 1;
				string[length - 1] = '\0';
				return string;
			}

			char *readStringRef() {
				int index = readInt(true);
				return index == 0 ? NULL : skeletonData->_strings[index - 1];
			}
		};

		AttachmentLoader *_attachmentLoader;
		Array<LinkedMesh *> _linkedMeshes;
		String _error;
		float _scale;
		const bool _ownsLoader;

		void setError(const char *value1, const char *value2);

		Skin *readSkin(DataInput &input, SkeletonData &skeletonData, bool defaultSkin, bool nonessential);

		Attachment *readAttachment(DataInput &input, Skin &skin, int slotIndex, const String &placeholder, SkeletonData &skeletonData,
								   bool nonessential);

		Sequence *readSequence(DataInput &input, bool hasPathSuffix);

		int readVertices(DataInput &input, Array<float> &vertices, Array<int> &bones, bool weighted);

		void readFloatArray(DataInput &input, int n, float scale, Array<float> &array);

		void readUnsignedShortArray(DataInput &input, Array<unsigned short> &array, int n);

		Animation *readAnimation(DataInput &input, const String &name, SkeletonData &skeletonData, bool nonessential);

		void readTimeline(DataInput &input, Array<Timeline *> &timelines, CurveTimeline1 &timeline, float scale);

		void readTimeline(DataInput &input, Array<Timeline *> &timelines, BoneTimeline2 &timeline, float scale);

		void readDrawOrder(DataInput &input, size_t slotCount, Array<int> &drawOrder);

		void setBezier(DataInput &input, CurveTimeline &timeline, int bezier, int frame, int value, float time1, float time2, float value1,
					   float value2, float scale);
	};
}// namespace spine

#endif /* Spine_SkeletonBinary_h */
