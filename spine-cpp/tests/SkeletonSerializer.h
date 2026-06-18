#ifndef Spine_SkeletonSerializer_h
#define Spine_SkeletonSerializer_h

#include <spine/spine.h>
#include "JsonWriter.h"
#include <stdio.h>
#include <stdlib.h>

namespace spine {

	class SkeletonSerializer {
	private:
		HashMap<void *, String> _visitedObjects;
		HashMap<String, String> _visitedSkinEntries;
		int _nextId;
		JsonWriter _json;

	public:
		SkeletonSerializer() {
		}
		~SkeletonSerializer() {
		}

		String serializeSkeletonData(SkeletonData *data) {
			_visitedObjects.clear();
			_visitedSkinEntries.clear();
			_nextId = 1;
			_json = JsonWriter();
			writeSkeletonData(data);
			return _json.getString();
		}

		String serializeSkeleton(Skeleton *skeleton) {
			_visitedObjects.clear();
			_visitedSkinEntries.clear();
			_nextId = 1;
			_json = JsonWriter();
			writeSkeleton(skeleton);
			return _json.getString();
		}

		String serializeAnimationState(AnimationState *state) {
			_visitedObjects.clear();
			_visitedSkinEntries.clear();
			_nextId = 1;
			_json = JsonWriter();
			writeAnimationState(state);
			return _json.getString();
		}

	private:
		void writeAnimation(Animation *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<Animation-").append(name).append(">");
			} else {
				refString.append("<Animation-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("Animation");

			_json.writeName("timelines");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getTimelines().size(); i++) {
				writeTimeline(obj->getTimelines()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeName("color");
			writeColor(&obj->getColor());

			_json.writeName("bones");
			writeIntArray(obj->getBones());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeObjectEnd();
		}

		void writeAlphaTimeline(AlphaTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<AlphaTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("AlphaTimeline");

			_json.writeName("slotIndex");
			_json.writeValue(obj->getSlotIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeAttachmentTimeline(AttachmentTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<AttachmentTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("AttachmentTimeline");

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("slotIndex");
			_json.writeValue(obj->getSlotIndex());

			_json.writeName("attachmentNames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getAttachmentNames().size(); i++) {
				_json.writeValue(obj->getAttachmentNames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeDeformTimeline(DeformTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<DeformTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("DeformTimeline");

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("attachment");
			writeVertexAttachment(obj->getAttachment());

			_json.writeName("vertices");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getVertices().size(); i++) {
				Array<float> &nestedArray = obj->getVertices()[i];
				_json.writeArrayStart();
				for (size_t j = 0; j < nestedArray.size(); j++) {
					_json.writeValue(nestedArray[j]);
				}
				_json.writeArrayEnd();
			}
			_json.writeArrayEnd();

			_json.writeName("slotIndex");
			_json.writeValue(obj->getSlotIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeDrawOrderTimeline(DrawOrderTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<DrawOrderTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("DrawOrderTimeline");

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("drawOrders");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getDrawOrders().size(); i++) {
				Array<int> &nestedArray = obj->getDrawOrders()[i];
				_json.writeArrayStart();
				for (size_t j = 0; j < nestedArray.size(); j++) {
					_json.writeValue(nestedArray[j]);
				}
				_json.writeArrayEnd();
			}
			_json.writeArrayEnd();

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeEventTimeline(EventTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<EventTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("EventTimeline");

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("events");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getEvents().size(); i++) {
				writeEvent(obj->getEvents()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeIkConstraintTimeline(IkConstraintTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<IkConstraintTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("IkConstraintTimeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeInheritTimeline(InheritTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<InheritTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("InheritTimeline");

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePathConstraintMixTimeline(PathConstraintMixTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PathConstraintMixTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PathConstraintMixTimeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePathConstraintPositionTimeline(PathConstraintPositionTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PathConstraintPositionTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PathConstraintPositionTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePathConstraintSpacingTimeline(PathConstraintSpacingTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PathConstraintSpacingTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PathConstraintSpacingTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintDampingTimeline(PhysicsConstraintDampingTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraintDampingTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintDampingTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintGravityTimeline(PhysicsConstraintGravityTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraintGravityTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintGravityTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintInertiaTimeline(PhysicsConstraintInertiaTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraintInertiaTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintInertiaTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintMassTimeline(PhysicsConstraintMassTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraintMassTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintMassTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintMixTimeline(PhysicsConstraintMixTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraintMixTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintMixTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintResetTimeline(PhysicsConstraintResetTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraintResetTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintResetTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintStrengthTimeline(PhysicsConstraintStrengthTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraintStrengthTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintStrengthTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintWindTimeline(PhysicsConstraintWindTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraintWindTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintWindTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeRGB2Timeline(RGB2Timeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<RGB2Timeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("RGB2Timeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("slotIndex");
			_json.writeValue(obj->getSlotIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeRGBA2Timeline(RGBA2Timeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<RGBA2Timeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("RGBA2Timeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("slotIndex");
			_json.writeValue(obj->getSlotIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeRGBATimeline(RGBATimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<RGBATimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("RGBATimeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("slotIndex");
			_json.writeValue(obj->getSlotIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeRGBTimeline(RGBTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<RGBTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("RGBTimeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("slotIndex");
			_json.writeValue(obj->getSlotIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeRotateTimeline(RotateTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<RotateTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("RotateTimeline");

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeScaleTimeline(ScaleTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ScaleTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ScaleTimeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeScaleXTimeline(ScaleXTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ScaleXTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ScaleXTimeline");

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeScaleYTimeline(ScaleYTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ScaleYTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ScaleYTimeline");

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeSequenceTimeline(SequenceTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<SequenceTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("SequenceTimeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("slotIndex");
			_json.writeValue(obj->getSlotIndex());

			_json.writeName("attachment");
			writeAttachment(obj->getAttachment());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeShearTimeline(ShearTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ShearTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ShearTimeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeShearXTimeline(ShearXTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ShearXTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ShearXTimeline");

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeShearYTimeline(ShearYTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ShearYTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ShearYTimeline");

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeSliderMixTimeline(SliderMixTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<SliderMixTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("SliderMixTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeSliderTimeline(SliderTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<SliderTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("SliderTimeline");

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeTimeline(Timeline *obj) {
			if (obj->getRTTI().instanceOf(AlphaTimeline::rtti)) {
				writeAlphaTimeline((AlphaTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(AttachmentTimeline::rtti)) {
				writeAttachmentTimeline((AttachmentTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(DeformTimeline::rtti)) {
				writeDeformTimeline((DeformTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(DrawOrderTimeline::rtti)) {
				writeDrawOrderTimeline((DrawOrderTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(EventTimeline::rtti)) {
				writeEventTimeline((EventTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(IkConstraintTimeline::rtti)) {
				writeIkConstraintTimeline((IkConstraintTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(InheritTimeline::rtti)) {
				writeInheritTimeline((InheritTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PathConstraintMixTimeline::rtti)) {
				writePathConstraintMixTimeline((PathConstraintMixTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PathConstraintPositionTimeline::rtti)) {
				writePathConstraintPositionTimeline((PathConstraintPositionTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PathConstraintSpacingTimeline::rtti)) {
				writePathConstraintSpacingTimeline((PathConstraintSpacingTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraintDampingTimeline::rtti)) {
				writePhysicsConstraintDampingTimeline((PhysicsConstraintDampingTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraintGravityTimeline::rtti)) {
				writePhysicsConstraintGravityTimeline((PhysicsConstraintGravityTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraintInertiaTimeline::rtti)) {
				writePhysicsConstraintInertiaTimeline((PhysicsConstraintInertiaTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraintMassTimeline::rtti)) {
				writePhysicsConstraintMassTimeline((PhysicsConstraintMassTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraintMixTimeline::rtti)) {
				writePhysicsConstraintMixTimeline((PhysicsConstraintMixTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraintResetTimeline::rtti)) {
				writePhysicsConstraintResetTimeline((PhysicsConstraintResetTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraintStrengthTimeline::rtti)) {
				writePhysicsConstraintStrengthTimeline((PhysicsConstraintStrengthTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraintWindTimeline::rtti)) {
				writePhysicsConstraintWindTimeline((PhysicsConstraintWindTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(RGB2Timeline::rtti)) {
				writeRGB2Timeline((RGB2Timeline *) obj);
			} else if (obj->getRTTI().instanceOf(RGBA2Timeline::rtti)) {
				writeRGBA2Timeline((RGBA2Timeline *) obj);
			} else if (obj->getRTTI().instanceOf(RGBATimeline::rtti)) {
				writeRGBATimeline((RGBATimeline *) obj);
			} else if (obj->getRTTI().instanceOf(RGBTimeline::rtti)) {
				writeRGBTimeline((RGBTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(RotateTimeline::rtti)) {
				writeRotateTimeline((RotateTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(ScaleTimeline::rtti)) {
				writeScaleTimeline((ScaleTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(ScaleXTimeline::rtti)) {
				writeScaleXTimeline((ScaleXTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(ScaleYTimeline::rtti)) {
				writeScaleYTimeline((ScaleYTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(SequenceTimeline::rtti)) {
				writeSequenceTimeline((SequenceTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(ShearTimeline::rtti)) {
				writeShearTimeline((ShearTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(ShearXTimeline::rtti)) {
				writeShearXTimeline((ShearXTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(ShearYTimeline::rtti)) {
				writeShearYTimeline((ShearYTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(SliderMixTimeline::rtti)) {
				writeSliderMixTimeline((SliderMixTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(SliderTimeline::rtti)) {
				writeSliderTimeline((SliderTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(TransformConstraintTimeline::rtti)) {
				writeTransformConstraintTimeline((TransformConstraintTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(TranslateTimeline::rtti)) {
				writeTranslateTimeline((TranslateTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(TranslateXTimeline::rtti)) {
				writeTranslateXTimeline((TranslateXTimeline *) obj);
			} else if (obj->getRTTI().instanceOf(TranslateYTimeline::rtti)) {
				writeTranslateYTimeline((TranslateYTimeline *) obj);
			} else {
				fprintf(stderr, "Error: Unknown Timeline type\n");
				exit(1);
			}
		}

		void writeTransformConstraintTimeline(TransformConstraintTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<TransformConstraintTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("TransformConstraintTimeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("constraintIndex");
			_json.writeValue(obj->getConstraintIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeTranslateTimeline(TranslateTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<TranslateTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("TranslateTimeline");

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeTranslateXTimeline(TranslateXTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<TranslateXTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("TranslateXTimeline");

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeTranslateYTimeline(TranslateYTimeline *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<TranslateYTimeline-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("TranslateYTimeline");

			_json.writeName("boneIndex");
			_json.writeValue(obj->getBoneIndex());

			_json.writeName("frameEntries");
			_json.writeValue(obj->getFrameEntries());

			_json.writeName("propertyIds");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPropertyIds().size(); i++) {
				_json.writeValue(obj->getPropertyIds()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frames");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getFrames().size(); i++) {
				_json.writeValue(obj->getFrames()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("frameCount");
			_json.writeValue(obj->getFrameCount());

			_json.writeName("duration");
			_json.writeValue(obj->getDuration());

			_json.writeObjectEnd();
		}

		void writeAnimationState(AnimationState *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<AnimationState-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("AnimationState");

			_json.writeName("timeScale");
			_json.writeValue(obj->getTimeScale());

			_json.writeName("data");
			writeAnimationStateData(obj->getData());

			_json.writeName("tracks");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getTracks().size(); i++) {
				writeTrackEntry(obj->getTracks()[i]);
			}
			_json.writeArrayEnd();

			_json.writeObjectEnd();
		}

		void writeTrackEntry(TrackEntry *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<TrackEntry-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("TrackEntry");

			_json.writeName("trackIndex");
			_json.writeValue(obj->getTrackIndex());

			_json.writeName("animation");
			writeAnimation(obj->getAnimation());

			_json.writeName("loop");
			_json.writeValue(obj->getLoop());

			_json.writeName("delay");
			_json.writeValue(obj->getDelay());

			_json.writeName("trackTime");
			_json.writeValue(obj->getTrackTime());

			_json.writeName("trackEnd");
			_json.writeValue(obj->getTrackEnd());

			_json.writeName("trackComplete");
			_json.writeValue(obj->getTrackComplete());

			_json.writeName("animationStart");
			_json.writeValue(obj->getAnimationStart());

			_json.writeName("animationEnd");
			_json.writeValue(obj->getAnimationEnd());

			_json.writeName("animationLast");
			_json.writeValue(obj->getAnimationLast());

			_json.writeName("animationTime");
			_json.writeValue(obj->getAnimationTime());

			_json.writeName("timeScale");
			_json.writeValue(obj->getTimeScale());

			_json.writeName("alpha");
			_json.writeValue(obj->getAlpha());

			_json.writeName("eventThreshold");
			_json.writeValue(obj->getEventThreshold());

			_json.writeName("alphaAttachmentThreshold");
			_json.writeValue(obj->getAlphaAttachmentThreshold());

			_json.writeName("mixAttachmentThreshold");
			_json.writeValue(obj->getMixAttachmentThreshold());

			_json.writeName("mixDrawOrderThreshold");
			_json.writeValue(obj->getMixDrawOrderThreshold());

			_json.writeName("next");
			if (obj->getNext() == nullptr) {
				_json.writeNull();
			} else {
				writeTrackEntry(obj->getNext());
			}

			_json.writeName("previous");
			if (obj->getPrevious() == nullptr) {
				_json.writeNull();
			} else {
				writeTrackEntry(obj->getPrevious());
			}

			_json.writeName("mixTime");
			_json.writeValue(obj->getMixTime());

			_json.writeName("mixDuration");
			_json.writeValue(obj->getMixDuration());

			_json.writeName("additive");
			_json.writeValue(obj->getAdditive());

			_json.writeName("mixingFrom");
			if (obj->getMixingFrom() == nullptr) {
				_json.writeNull();
			} else {
				writeTrackEntry(obj->getMixingFrom());
			}

			_json.writeName("mixingTo");
			if (obj->getMixingTo() == nullptr) {
				_json.writeNull();
			} else {
				writeTrackEntry(obj->getMixingTo());
			}


			_json.writeName("shortestRotation");
			_json.writeValue(obj->getShortestRotation());

			_json.writeName("reverse");
			_json.writeValue(obj->getReverse());

			_json.writeObjectEnd();
		}

		void writeAnimationStateData(AnimationStateData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<AnimationStateData-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("AnimationStateData");

			_json.writeName("skeletonData");
			writeSkeletonData(obj->getSkeletonData());

			_json.writeName("defaultMix");
			_json.writeValue(obj->getDefaultMix());

			_json.writeObjectEnd();
		}

		void writeAttachment(Attachment *obj) {
			if (obj->getRTTI().instanceOf(BoundingBoxAttachment::rtti)) {
				writeBoundingBoxAttachment((BoundingBoxAttachment *) obj);
			} else if (obj->getRTTI().instanceOf(ClippingAttachment::rtti)) {
				writeClippingAttachment((ClippingAttachment *) obj);
			} else if (obj->getRTTI().instanceOf(MeshAttachment::rtti)) {
				writeMeshAttachment((MeshAttachment *) obj);
			} else if (obj->getRTTI().instanceOf(PathAttachment::rtti)) {
				writePathAttachment((PathAttachment *) obj);
			} else if (obj->getRTTI().instanceOf(PointAttachment::rtti)) {
				writePointAttachment((PointAttachment *) obj);
			} else if (obj->getRTTI().instanceOf(RegionAttachment::rtti)) {
				writeRegionAttachment((RegionAttachment *) obj);
			} else {
				fprintf(stderr, "Error: Unknown Attachment type\n");
				exit(1);
			}
		}

		void writeBone(Bone *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<Bone-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("Bone");

			_json.writeName("parent");
			if (obj->getParent() == nullptr) {
				_json.writeNull();
			} else {
				writeBone(obj->getParent());
			}

			_json.writeName("children");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getChildren().size(); i++) {
				writeBone(obj->getChildren()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("data");
			writeBoneData(obj->getData());

			_json.writeName("pose");
			writeBonePose(obj->getPose());

			_json.writeName("appliedPose");
			writeBonePose(obj->getAppliedPose());

			_json.writeObjectEnd();
		}

		void writeBoneData(BoneData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<BoneData-").append(name).append(">");
			} else {
				refString.append("<BoneData-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("BoneData");

			_json.writeName("index");
			_json.writeValue(obj->getIndex());

			_json.writeName("parent");
			if (obj->getParent() == nullptr) {
				_json.writeNull();
			} else {
				writeBoneData(obj->getParent());
			}

			_json.writeName("length");
			_json.writeValue(obj->getLength());

			_json.writeName("color");
			writeColor(obj->getColor());

			_json.writeName("icon");
			_json.writeValue(obj->getIcon());

			_json.writeName("iconSize");
			_json.writeValue(obj->getIconSize());

			_json.writeName("iconRotation");
			_json.writeValue(obj->getIconRotation());

			_json.writeName("visible");
			_json.writeValue(obj->getVisible());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeName("setupPose");
			writeBonePose(obj->getSetupPose());

			_json.writeName("skinRequired");
			_json.writeValue(obj->getSkinRequired());

			_json.writeObjectEnd();
		}

		void writeBoneLocal(BoneLocal *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<BoneLocal-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("BoneLocal");

			_json.writeName("x");
			_json.writeValue(obj->getX());

			_json.writeName("y");
			_json.writeValue(obj->getY());

			_json.writeName("rotation");
			_json.writeValue(obj->getRotation());

			_json.writeName("scaleX");
			_json.writeValue(obj->getScaleX());

			_json.writeName("scaleY");
			_json.writeValue(obj->getScaleY());

			_json.writeName("shearX");
			_json.writeValue(obj->getShearX());

			_json.writeName("shearY");
			_json.writeValue(obj->getShearY());

			_json.writeName("inherit");
			_json.writeValue([&]() -> String {
				switch (obj->getInherit()) {
					case Inherit_Normal:
						return "normal";
					case Inherit_OnlyTranslation:
						return "onlyTranslation";
					case Inherit_NoRotationOrReflection:
						return "noRotationOrReflection";
					case Inherit_NoScale:
						return "noScale";
					case Inherit_NoScaleOrReflection:
						return "noScaleOrReflection";
					default:
						return "unknown";
				}
			}());

			_json.writeObjectEnd();
		}

		void writeBonePose(BonePose *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<BonePose-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("BonePose");

			_json.writeName("a");
			_json.writeValue(obj->getA());

			_json.writeName("b");
			_json.writeValue(obj->getB());

			_json.writeName("c");
			_json.writeValue(obj->getC());

			_json.writeName("d");
			_json.writeValue(obj->getD());

			_json.writeName("worldX");
			_json.writeValue(obj->getWorldX());

			_json.writeName("worldY");
			_json.writeValue(obj->getWorldY());

			_json.writeName("worldRotationX");
			_json.writeValue(obj->getWorldRotationX());

			_json.writeName("worldRotationY");
			_json.writeValue(obj->getWorldRotationY());

			_json.writeName("worldScaleX");
			_json.writeValue(obj->getWorldScaleX());

			_json.writeName("worldScaleY");
			_json.writeValue(obj->getWorldScaleY());

			_json.writeName("x");
			_json.writeValue(obj->getX());

			_json.writeName("y");
			_json.writeValue(obj->getY());

			_json.writeName("rotation");
			_json.writeValue(obj->getRotation());

			_json.writeName("scaleX");
			_json.writeValue(obj->getScaleX());

			_json.writeName("scaleY");
			_json.writeValue(obj->getScaleY());

			_json.writeName("shearX");
			_json.writeValue(obj->getShearX());

			_json.writeName("shearY");
			_json.writeValue(obj->getShearY());

			_json.writeName("inherit");
			_json.writeValue([&]() -> String {
				switch (obj->getInherit()) {
					case Inherit_Normal:
						return "normal";
					case Inherit_OnlyTranslation:
						return "onlyTranslation";
					case Inherit_NoRotationOrReflection:
						return "noRotationOrReflection";
					case Inherit_NoScale:
						return "noScale";
					case Inherit_NoScaleOrReflection:
						return "noScaleOrReflection";
					default:
						return "unknown";
				}
			}());

			_json.writeObjectEnd();
		}

		void writeBoundingBoxAttachment(BoundingBoxAttachment *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<BoundingBoxAttachment-").append(name).append(">");
			} else {
				refString.append("<BoundingBoxAttachment-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("BoundingBoxAttachment");

			_json.writeName("color");
			writeColor(obj->getColor());

			_json.writeName("bones");
			if (obj->getBones().size() == 0) {
				_json.writeNull();
			} else {
				_json.writeArrayStart();
				for (size_t i = 0; i < obj->getBones().size(); i++) {
					_json.writeValue(obj->getBones()[i]);
				}
				_json.writeArrayEnd();
			}

			_json.writeName("vertices");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getVertices().size(); i++) {
				_json.writeValue(obj->getVertices()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("worldVerticesLength");
			_json.writeValue(obj->getWorldVerticesLength());

			_json.writeName("timelineAttachment");
			if (obj->getTimelineAttachment() == nullptr) {
				_json.writeNull();
			} else {
				writeAttachment(obj->getTimelineAttachment());
			}

			_json.writeName("timelineSlots");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getTimelineSlots().size(); i++) {
				_json.writeValue(obj->getTimelineSlots()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("id");
			_json.writeValue(obj->getId());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeObjectEnd();
		}

		void writeClippingAttachment(ClippingAttachment *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<ClippingAttachment-").append(name).append(">");
			} else {
				refString.append("<ClippingAttachment-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ClippingAttachment");

			_json.writeName("endSlot");
			if (obj->getEndSlot() == nullptr) {
				_json.writeNull();
			} else {
				writeSlotData(obj->getEndSlot());
			}

			_json.writeName("inverse");
			_json.writeValue(obj->getInverse());

			_json.writeName("convex");
			_json.writeValue(obj->getConvex());

			_json.writeName("color");
			writeColor(obj->getColor());

			_json.writeName("bones");
			if (obj->getBones().size() == 0) {
				_json.writeNull();
			} else {
				_json.writeArrayStart();
				for (size_t i = 0; i < obj->getBones().size(); i++) {
					_json.writeValue(obj->getBones()[i]);
				}
				_json.writeArrayEnd();
			}

			_json.writeName("vertices");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getVertices().size(); i++) {
				_json.writeValue(obj->getVertices()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("worldVerticesLength");
			_json.writeValue(obj->getWorldVerticesLength());

			_json.writeName("timelineAttachment");
			if (obj->getTimelineAttachment() == nullptr) {
				_json.writeNull();
			} else {
				writeAttachment(obj->getTimelineAttachment());
			}

			_json.writeName("timelineSlots");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getTimelineSlots().size(); i++) {
				_json.writeValue(obj->getTimelineSlots()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("id");
			_json.writeValue(obj->getId());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeObjectEnd();
		}

		void writeConstraint(Constraint *obj) {
			if (obj->getRTTI().instanceOf(IkConstraint::rtti)) {
				writeIkConstraint((IkConstraint *) obj);
			} else if (obj->getRTTI().instanceOf(PathConstraint::rtti)) {
				writePathConstraint((PathConstraint *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraint::rtti)) {
				writePhysicsConstraint((PhysicsConstraint *) obj);
			} else if (obj->getRTTI().instanceOf(Slider::rtti)) {
				writeSlider((Slider *) obj);
			} else if (obj->getRTTI().instanceOf(TransformConstraint::rtti)) {
				writeTransformConstraint((TransformConstraint *) obj);
			} else {
				fprintf(stderr, "Error: Unknown Constraint type\n");
				exit(1);
			}
		}

		void writeConstraintData(ConstraintData *obj) {
			if (obj->getRTTI().instanceOf(IkConstraintData::rtti)) {
				writeIkConstraintData((IkConstraintData *) obj);
			} else if (obj->getRTTI().instanceOf(PathConstraintData::rtti)) {
				writePathConstraintData((PathConstraintData *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraintData::rtti)) {
				writePhysicsConstraintData((PhysicsConstraintData *) obj);
			} else if (obj->getRTTI().instanceOf(SliderData::rtti)) {
				writeSliderData((SliderData *) obj);
			} else if (obj->getRTTI().instanceOf(TransformConstraintData::rtti)) {
				writeTransformConstraintData((TransformConstraintData *) obj);
			} else {
				fprintf(stderr, "Error: Unknown ConstraintData type\n");
				exit(1);
			}
		}

		void writeEvent(Event *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<Event-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("Event");

			_json.writeName("int");
			_json.writeValue(obj->getInt());

			_json.writeName("float");
			_json.writeValue(obj->getFloat());

			_json.writeName("string");
			_json.writeValue(obj->getString());

			_json.writeName("volume");
			_json.writeValue(obj->getVolume());

			_json.writeName("balance");
			_json.writeValue(obj->getBalance());

			_json.writeName("time");
			_json.writeValue(obj->getTime());

			_json.writeName("data");
			writeEventData(obj->getData());

			_json.writeObjectEnd();
		}

		void writeEventData(EventData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<EventData-").append(name).append(">");
			} else {
				refString.append("<EventData-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("EventData");

			Event &setup = obj->getSetupPose();
			_json.writeName("int");
			_json.writeValue(setup.getInt());

			_json.writeName("float");
			_json.writeValue(setup.getFloat());

			_json.writeName("string");
			_json.writeValue(setup.getString());

			_json.writeName("audioPath");
			_json.writeValue(obj->getAudioPath());

			_json.writeName("volume");
			_json.writeValue(setup.getVolume());

			_json.writeName("balance");
			_json.writeValue(setup.getBalance());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeObjectEnd();
		}

		void writeIkConstraint(IkConstraint *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<IkConstraint-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("IkConstraint");

			_json.writeName("bones");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getBones().size(); i++) {
				writeBonePose(obj->getBones()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("target");
			writeBone(obj->getTarget());

			_json.writeName("data");
			writeIkConstraintData(obj->getData());

			_json.writeName("pose");
			writeIkConstraintPose(obj->getPose());

			_json.writeName("appliedPose");
			writeIkConstraintPose(obj->getAppliedPose());

			_json.writeObjectEnd();
		}

		void writeIkConstraintData(IkConstraintData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<IkConstraintData-").append(name).append(">");
			} else {
				refString.append("<IkConstraintData-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("IkConstraintData");

			_json.writeName("bones");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getBones().size(); i++) {
				writeBoneData(obj->getBones()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("target");
			writeBoneData(obj->getTarget());

			_json.writeName("scaleY");
			_json.writeValue(ScaleYMode_toString(obj->getScaleYMode()));

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeName("setupPose");
			writeIkConstraintPose(obj->getSetupPose());

			_json.writeName("skinRequired");
			_json.writeValue(obj->getSkinRequired());

			_json.writeObjectEnd();
		}

		void writeIkConstraintPose(IkConstraintPose *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<IkConstraintPose-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("IkConstraintPose");

			_json.writeName("mix");
			_json.writeValue(obj->getMix());

			_json.writeName("softness");
			_json.writeValue(obj->getSoftness());

			_json.writeName("bendDirection");
			_json.writeValue(obj->getBendDirection());

			_json.writeName("compress");
			_json.writeValue(obj->getCompress());

			_json.writeName("stretch");
			_json.writeValue(obj->getStretch());

			_json.writeObjectEnd();
		}

		void writeMeshAttachment(MeshAttachment *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<MeshAttachment-").append(name).append(">");
			} else {
				refString.append("<MeshAttachment-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("MeshAttachment");

			Sequence &sequence = obj->getSequence();
			int setupIndex = sequence.getSetupIndex();
			_json.writeName("region");
			if (sequence.getRegion(setupIndex) == nullptr) {
				_json.writeNull();
			} else {
				writeTextureRegion(sequence.getRegion(setupIndex));
			}

			_json.writeName("triangles");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getTriangles().size(); i++) {
				_json.writeValue(obj->getTriangles()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("regionUVs");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getRegionUVs().size(); i++) {
				_json.writeValue(obj->getRegionUVs()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("uVs");
			_json.writeArrayStart();
			for (size_t i = 0; i < sequence.getUVs(setupIndex).size(); i++) {
				_json.writeValue(sequence.getUVs(setupIndex)[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("color");
			writeColor(obj->getColor());

			_json.writeName("path");
			_json.writeValue(obj->getPath());

			_json.writeName("hullLength");
			_json.writeValue(obj->getHullLength());

			_json.writeName("edges");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getEdges().size(); i++) {
				_json.writeValue(obj->getEdges()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("width");
			_json.writeValue(obj->getWidth());

			_json.writeName("height");
			_json.writeValue(obj->getHeight());

			_json.writeName("sequence");
			writeSequence(&obj->getSequence());

			_json.writeName("sourceMesh");
			if (obj->getSourceMesh() == nullptr) {
				_json.writeNull();
			} else {
				writeMeshAttachment(obj->getSourceMesh());
			}

			_json.writeName("bones");
			if (obj->getBones().size() == 0) {
				_json.writeNull();
			} else {
				_json.writeArrayStart();
				for (size_t i = 0; i < obj->getBones().size(); i++) {
					_json.writeValue(obj->getBones()[i]);
				}
				_json.writeArrayEnd();
			}

			_json.writeName("vertices");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getVertices().size(); i++) {
				_json.writeValue(obj->getVertices()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("worldVerticesLength");
			_json.writeValue(obj->getWorldVerticesLength());

			_json.writeName("timelineAttachment");
			if (obj->getTimelineAttachment() == nullptr) {
				_json.writeNull();
			} else {
				writeAttachment(obj->getTimelineAttachment());
			}

			_json.writeName("timelineSlots");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getTimelineSlots().size(); i++) {
				_json.writeValue(obj->getTimelineSlots()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("id");
			_json.writeValue(obj->getId());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeObjectEnd();
		}

		void writePathAttachment(PathAttachment *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<PathAttachment-").append(name).append(">");
			} else {
				refString.append("<PathAttachment-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PathAttachment");

			_json.writeName("closed");
			_json.writeValue(obj->getClosed());

			_json.writeName("constantSpeed");
			_json.writeValue(obj->getConstantSpeed());

			_json.writeName("lengths");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getLengths().size(); i++) {
				_json.writeValue(obj->getLengths()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("color");
			writeColor(obj->getColor());

			_json.writeName("bones");
			if (obj->getBones().size() == 0) {
				_json.writeNull();
			} else {
				_json.writeArrayStart();
				for (size_t i = 0; i < obj->getBones().size(); i++) {
					_json.writeValue(obj->getBones()[i]);
				}
				_json.writeArrayEnd();
			}

			_json.writeName("vertices");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getVertices().size(); i++) {
				_json.writeValue(obj->getVertices()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("worldVerticesLength");
			_json.writeValue(obj->getWorldVerticesLength());

			_json.writeName("timelineAttachment");
			if (obj->getTimelineAttachment() == nullptr) {
				_json.writeNull();
			} else {
				writeAttachment(obj->getTimelineAttachment());
			}

			_json.writeName("timelineSlots");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getTimelineSlots().size(); i++) {
				_json.writeValue(obj->getTimelineSlots()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("id");
			_json.writeValue(obj->getId());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeObjectEnd();
		}

		void writePathConstraint(PathConstraint *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PathConstraint-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PathConstraint");

			_json.writeName("bones");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getBones().size(); i++) {
				writeBonePose(obj->getBones()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("slot");
			writeSlot(obj->getSlot());

			_json.writeName("data");
			writePathConstraintData(obj->getData());

			_json.writeName("pose");
			writePathConstraintPose(obj->getPose());

			_json.writeName("appliedPose");
			writePathConstraintPose(obj->getAppliedPose());

			_json.writeObjectEnd();
		}

		void writePathConstraintData(PathConstraintData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<PathConstraintData-").append(name).append(">");
			} else {
				refString.append("<PathConstraintData-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PathConstraintData");

			_json.writeName("bones");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getBones().size(); i++) {
				writeBoneData(obj->getBones()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("slot");
			writeSlotData(obj->getSlot());

			_json.writeName("positionMode");
			_json.writeValue([&]() -> String {
				switch (obj->getPositionMode()) {
					case PositionMode_Fixed:
						return "fixed";
					case PositionMode_Percent:
						return "percent";
					default:
						return "unknown";
				}
			}());

			_json.writeName("spacingMode");
			_json.writeValue([&]() -> String {
				switch (obj->getSpacingMode()) {
					case SpacingMode_Length:
						return "length";
					case SpacingMode_Fixed:
						return "fixed";
					case SpacingMode_Percent:
						return "percent";
					case SpacingMode_Proportional:
						return "proportional";
					default:
						return "unknown";
				}
			}());

			_json.writeName("rotateMode");
			_json.writeValue([&]() -> String {
				switch (obj->getRotateMode()) {
					case RotateMode_Tangent:
						return "tangent";
					case RotateMode_Chain:
						return "chain";
					case RotateMode_ChainScale:
						return "chainScale";
					default:
						return "unknown";
				}
			}());

			_json.writeName("offsetRotation");
			_json.writeValue(obj->getOffsetRotation());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeName("setupPose");
			writePathConstraintPose(obj->getSetupPose());

			_json.writeName("skinRequired");
			_json.writeValue(obj->getSkinRequired());

			_json.writeObjectEnd();
		}

		void writePathConstraintPose(PathConstraintPose *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PathConstraintPose-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PathConstraintPose");

			_json.writeName("position");
			_json.writeValue(obj->getPosition());

			_json.writeName("spacing");
			_json.writeValue(obj->getSpacing());

			_json.writeName("mixRotate");
			_json.writeValue(obj->getMixRotate());

			_json.writeName("mixX");
			_json.writeValue(obj->getMixX());

			_json.writeName("mixY");
			_json.writeValue(obj->getMixY());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraint(PhysicsConstraint *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraint-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraint");

			_json.writeName("bone");
			writeBonePose(obj->getBone());

			_json.writeName("data");
			writePhysicsConstraintData(obj->getData());

			_json.writeName("pose");
			writePhysicsConstraintPose(obj->getPose());

			_json.writeName("appliedPose");
			writePhysicsConstraintPose(obj->getAppliedPose());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintData(PhysicsConstraintData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<PhysicsConstraintData-").append(name).append(">");
			} else {
				refString.append("<PhysicsConstraintData-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintData");

			_json.writeName("bone");
			writeBoneData(obj->getBone());

			_json.writeName("step");
			_json.writeValue(obj->getStep());

			_json.writeName("x");
			_json.writeValue(obj->getX());

			_json.writeName("y");
			_json.writeValue(obj->getY());

			_json.writeName("rotate");
			_json.writeValue(obj->getRotate());

			_json.writeName("scaleX");
			_json.writeValue(obj->getScaleX());

			_json.writeName("shearX");
			_json.writeValue(obj->getShearX());

			_json.writeName("limit");
			_json.writeValue(obj->getLimit());

			_json.writeName("inertiaGlobal");
			_json.writeValue(obj->getInertiaGlobal());

			_json.writeName("strengthGlobal");
			_json.writeValue(obj->getStrengthGlobal());

			_json.writeName("dampingGlobal");
			_json.writeValue(obj->getDampingGlobal());

			_json.writeName("massGlobal");
			_json.writeValue(obj->getMassGlobal());

			_json.writeName("windGlobal");
			_json.writeValue(obj->getWindGlobal());

			_json.writeName("gravityGlobal");
			_json.writeValue(obj->getGravityGlobal());

			_json.writeName("mixGlobal");
			_json.writeValue(obj->getMixGlobal());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeName("setupPose");
			writePhysicsConstraintPose(obj->getSetupPose());

			_json.writeName("skinRequired");
			_json.writeValue(obj->getSkinRequired());

			_json.writeObjectEnd();
		}

		void writePhysicsConstraintPose(PhysicsConstraintPose *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<PhysicsConstraintPose-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PhysicsConstraintPose");

			_json.writeName("inertia");
			_json.writeValue(obj->getInertia());

			_json.writeName("strength");
			_json.writeValue(obj->getStrength());

			_json.writeName("damping");
			_json.writeValue(obj->getDamping());

			_json.writeName("massInverse");
			_json.writeValue(obj->getMassInverse());

			_json.writeName("wind");
			_json.writeValue(obj->getWind());

			_json.writeName("gravity");
			_json.writeValue(obj->getGravity());

			_json.writeName("mix");
			_json.writeValue(obj->getMix());

			_json.writeObjectEnd();
		}

		void writePointAttachment(PointAttachment *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<PointAttachment-").append(name).append(">");
			} else {
				refString.append("<PointAttachment-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("PointAttachment");

			_json.writeName("x");
			_json.writeValue(obj->getX());

			_json.writeName("y");
			_json.writeValue(obj->getY());

			_json.writeName("rotation");
			_json.writeValue(obj->getRotation());

			_json.writeName("color");
			writeColor(obj->getColor());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeObjectEnd();
		}

		void writeRegionAttachment(RegionAttachment *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<RegionAttachment-").append(name).append(">");
			} else {
				refString.append("<RegionAttachment-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("RegionAttachment");

			Sequence &sequence = obj->getSequence();
			int setupIndex = sequence.getSetupIndex();
			_json.writeName("region");
			if (sequence.getRegion(setupIndex) == nullptr) {
				_json.writeNull();
			} else {
				writeTextureRegion(sequence.getRegion(setupIndex));
			}

			_json.writeName("offset");
			_json.writeArrayStart();
			for (size_t i = 0; i < sequence.getOffsets(setupIndex).size(); i++) {
				_json.writeValue(sequence.getOffsets(setupIndex)[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("uVs");
			_json.writeArrayStart();
			for (size_t i = 0; i < sequence.getUVs(setupIndex).size(); i++) {
				_json.writeValue(sequence.getUVs(setupIndex)[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("x");
			_json.writeValue(obj->getX());

			_json.writeName("y");
			_json.writeValue(obj->getY());

			_json.writeName("scaleX");
			_json.writeValue(obj->getScaleX());

			_json.writeName("scaleY");
			_json.writeValue(obj->getScaleY());

			_json.writeName("rotation");
			_json.writeValue(obj->getRotation());

			_json.writeName("width");
			_json.writeValue(obj->getWidth());

			_json.writeName("height");
			_json.writeValue(obj->getHeight());

			_json.writeName("color");
			writeColor(obj->getColor());

			_json.writeName("path");
			_json.writeValue(obj->getPath());

			_json.writeName("sequence");
			writeSequence(&obj->getSequence());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeObjectEnd();
		}

		void writeSequence(Sequence *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<Sequence-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("Sequence");

			_json.writeName("start");
			_json.writeValue(obj->getStart());

			_json.writeName("digits");
			_json.writeValue(obj->getDigits());

			_json.writeName("setupIndex");
			_json.writeValue(obj->getSetupIndex());

			_json.writeName("regions");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getRegions().size(); i++) {
				writeTextureRegion(obj->getRegions()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("id");
			_json.writeValue(obj->getId());

			_json.writeObjectEnd();
		}

		void writeSkeleton(Skeleton *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<Skeleton-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("Skeleton");

			_json.writeName("data");
			writeSkeletonData(obj->getData());

			_json.writeName("bones");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getBones().size(); i++) {
				writeBone(obj->getBones()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("updateCache");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getUpdateCache().size(); i++) {
				writeUpdate(obj->getUpdateCache()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("rootBone");
			writeBone(obj->getRootBone());

			_json.writeName("slots");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getSlots().size(); i++) {
				writeSlot(obj->getSlots()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("drawOrderPose");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getDrawOrder().getPose().size(); i++) {
				writeSlot(obj->getDrawOrder().getPose()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("drawOrderApplied");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getDrawOrder().getAppliedPose().size(); i++) {
				writeSlot(obj->getDrawOrder().getAppliedPose()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("skin");
			if (obj->getSkin() == nullptr) {
				_json.writeNull();
			} else {
				writeSkin(obj->getSkin());
			}

			_json.writeName("constraints");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getConstraints().size(); i++) {
				writeConstraint(obj->getConstraints()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("physicsConstraints");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getPhysicsConstraints().size(); i++) {
				writePhysicsConstraint(obj->getPhysicsConstraints()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("color");
			writeColor(obj->getColor());

			_json.writeName("scaleX");
			_json.writeValue(obj->getScaleX());

			_json.writeName("scaleY");
			_json.writeValue(obj->getScaleY());

			_json.writeName("x");
			_json.writeValue(obj->getX());

			_json.writeName("y");
			_json.writeValue(obj->getY());

			_json.writeName("windX");
			_json.writeValue(obj->getWindX());

			_json.writeName("windY");
			_json.writeValue(obj->getWindY());

			_json.writeName("gravityX");
			_json.writeValue(obj->getGravityX());

			_json.writeName("gravityY");
			_json.writeValue(obj->getGravityY());

			_json.writeName("time");
			_json.writeValue(obj->getTime());

			_json.writeObjectEnd();
		}

		void writeSkeletonData(SkeletonData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<SkeletonData-").append(name).append(">");
			} else {
				refString.append("<SkeletonData-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("SkeletonData");

			_json.writeName("bones");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getBones().size(); i++) {
				writeBoneData(obj->getBones()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("slots");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getSlots().size(); i++) {
				writeSlotData(obj->getSlots()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("defaultSkin");
			if (obj->getDefaultSkin() == nullptr) {
				_json.writeNull();
			} else {
				writeSkin(obj->getDefaultSkin());
			}

			_json.writeName("skins");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getSkins().size(); i++) {
				writeSkin(obj->getSkins()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("events");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getEvents().size(); i++) {
				writeEventData(obj->getEvents()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("animations");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getAnimations().size(); i++) {
				writeAnimation(obj->getAnimations()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("constraints");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getConstraints().size(); i++) {
				writeConstraintData(obj->getConstraints()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeName("x");
			_json.writeValue(obj->getX());

			_json.writeName("y");
			_json.writeValue(obj->getY());

			_json.writeName("width");
			_json.writeValue(obj->getWidth());

			_json.writeName("height");
			_json.writeValue(obj->getHeight());

			_json.writeName("referenceScale");
			_json.writeValue(obj->getReferenceScale());

			_json.writeName("version");
			_json.writeValue(obj->getVersion());

			_json.writeName("hash");
			_json.writeValue(obj->getHash());

			_json.writeName("imagesPath");
			_json.writeValue(obj->getImagesPath());

			_json.writeName("audioPath");
			_json.writeValue(obj->getAudioPath());

			_json.writeName("fps");
			_json.writeValue(obj->getFps());

			_json.writeObjectEnd();
		}

		void writeSkin(Skin *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<Skin-").append(name).append(">");
			} else {
				refString.append("<Skin-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);


			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("Skin");

			_json.writeName("attachments");
			_json.writeArrayStart();
			Skin::AttachmentMap::Entries entries = obj->getAttachments();
			while (entries.hasNext()) {
				Skin::AttachmentMap::Entry &entry = entries.next();
				writeSkinEntry(&entry);
			}
			_json.writeArrayEnd();

			_json.writeName("bones");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getBones().size(); i++) {
				BoneData *item = obj->getBones()[i];
				writeBoneData(item);
			}
			_json.writeArrayEnd();

			_json.writeName("constraints");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getConstraints().size(); i++) {
				ConstraintData *item = obj->getConstraints()[i];
				writeConstraintData(item);
			}
			_json.writeArrayEnd();

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeName("color");
			writeColor(&obj->getColor());

			_json.writeObjectEnd();
		}

		void writeSkinEntry(Skin::AttachmentMap::Entry *obj) {
			String placeholder = obj->_placeholder;
			String key = String().append((int) obj->_slotIndex).append(":").append(placeholder);
			if (_visitedSkinEntries.containsKey(key)) {
				_json.writeValue(_visitedSkinEntries[key]);
				return;
			}
			String refString;
			if (!placeholder.isEmpty()) {
				refString.append("<SkinEntry-").append(placeholder).append(">");
			} else {
				refString.append("<SkinEntry-").append(_nextId++).append(">");
			}
			_visitedSkinEntries.put(key, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("SkinEntry");
			_json.writeName("slotIndex");
			_json.writeValue((int) obj->_slotIndex);
			_json.writeName("name");
			_json.writeValue(obj->_placeholder);
			_json.writeName("attachment");
			writeAttachment(obj->_attachment);
			_json.writeObjectEnd();
		}

		void writeSlider(Slider *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<Slider-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("Slider");

			_json.writeName("bone");
			writeBone(obj->getBone());

			_json.writeName("data");
			writeSliderData(obj->getData());

			_json.writeName("pose");
			writeSliderPose(obj->getPose());

			_json.writeName("appliedPose");
			writeSliderPose(obj->getAppliedPose());

			_json.writeObjectEnd();
		}

		void writeSliderData(SliderData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<SliderData-").append(name).append(">");
			} else {
				refString.append("<SliderData-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("SliderData");

			_json.writeName("animation");
			writeAnimation(obj->getAnimation());

			_json.writeName("additive");
			_json.writeValue(obj->getAdditive());

			_json.writeName("loop");
			_json.writeValue(obj->getLoop());

			_json.writeName("bone");
			if (obj->getBone() == nullptr) {
				_json.writeNull();
			} else {
				writeBoneData(obj->getBone());
			}

			_json.writeName("property");
			if (obj->getProperty() == nullptr) {
				_json.writeNull();
			} else {
				writeFromProperty(obj->getProperty());
			}

			_json.writeName("offset");
			_json.writeValue(obj->getOffset());

			_json.writeName("scale");
			_json.writeValue(obj->getScale());

			_json.writeName("max");
			_json.writeValue(obj->getMax());

			_json.writeName("local");
			_json.writeValue(obj->getLocal());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeName("setupPose");
			writeSliderPose(obj->getSetupPose());

			_json.writeName("skinRequired");
			_json.writeValue(obj->getSkinRequired());

			_json.writeObjectEnd();
		}

		void writeSliderPose(SliderPose *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<SliderPose-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("SliderPose");

			_json.writeName("time");
			_json.writeValue(obj->getTime());

			_json.writeName("mix");
			_json.writeValue(obj->getMix());

			_json.writeObjectEnd();
		}

		void writeSlot(Slot *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<Slot-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("Slot");

			_json.writeName("bone");
			writeBone(obj->getBone());

			_json.writeName("data");
			writeSlotData(obj->getData());

			_json.writeName("pose");
			writeSlotPose(obj->getPose());

			_json.writeName("appliedPose");
			writeSlotPose(obj->getAppliedPose());

			_json.writeObjectEnd();
		}

		void writeSlotData(SlotData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<SlotData-").append(name).append(">");
			} else {
				refString.append("<SlotData-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("SlotData");

			_json.writeName("index");
			_json.writeValue(obj->getIndex());

			_json.writeName("boneData");
			writeBoneData(obj->getBoneData());

			_json.writeName("attachmentName");
			_json.writeValue(obj->getAttachmentName());

			_json.writeName("blendMode");
			_json.writeValue([&]() -> String {
				switch (obj->getBlendMode()) {
					case BlendMode_Normal:
						return "normal";
					case BlendMode_Additive:
						return "additive";
					case BlendMode_Multiply:
						return "multiply";
					case BlendMode_Screen:
						return "screen";
					default:
						return "unknown";
				}
			}());

			_json.writeName("visible");
			_json.writeValue(obj->getVisible());

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeName("setupPose");
			writeSlotPose(obj->getSetupPose());

			_json.writeName("skinRequired");
			_json.writeValue(obj->getSkinRequired());

			_json.writeObjectEnd();
		}

		void writeSlotPose(SlotPose *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<SlotPose-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("SlotPose");

			_json.writeName("color");
			writeColor(obj->getColor());

			_json.writeName("darkColor");
			if (obj->hasDarkColor()) {
				writeColor(&obj->getDarkColor());
			} else {
				_json.writeNull();
			}

			_json.writeName("attachment");
			if (obj->getAttachment() == nullptr) {
				_json.writeNull();
			} else {
				writeAttachment(obj->getAttachment());
			}

			_json.writeName("sequenceIndex");
			_json.writeValue(obj->getSequenceIndex());

			_json.writeName("deform");
			writeFloatArray(obj->getDeform());

			_json.writeObjectEnd();
		}

		void writeTransformConstraint(TransformConstraint *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<TransformConstraint-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("TransformConstraint");

			_json.writeName("bones");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getBones().size(); i++) {
				writeBonePose(obj->getBones()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("source");
			writeBone(obj->getSource());

			_json.writeName("data");
			writeTransformConstraintData(obj->getData());

			_json.writeName("pose");
			writeTransformConstraintPose(obj->getPose());

			_json.writeName("appliedPose");
			writeTransformConstraintPose(obj->getAppliedPose());

			_json.writeObjectEnd();
		}

		void writeTransformConstraintData(TransformConstraintData *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String name = obj->getName();
			String refString;
			if (!name.isEmpty()) {
				refString.append("<TransformConstraintData-").append(name).append(">");
			} else {
				refString.append("<TransformConstraintData-").append(_nextId++).append(">");
			}
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("TransformConstraintData");

			_json.writeName("bones");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getBones().size(); i++) {
				writeBoneData(obj->getBones()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("source");
			writeBoneData(obj->getSource());

			_json.writeName("offsetRotation");
			_json.writeValue(obj->getOffsetRotation());

			_json.writeName("offsetX");
			_json.writeValue(obj->getOffsetX());

			_json.writeName("offsetY");
			_json.writeValue(obj->getOffsetY());

			_json.writeName("offsetScaleX");
			_json.writeValue(obj->getOffsetScaleX());

			_json.writeName("offsetScaleY");
			_json.writeValue(obj->getOffsetScaleY());

			_json.writeName("offsetShearY");
			_json.writeValue(obj->getOffsetShearY());

			_json.writeName("localSource");
			_json.writeValue(obj->getLocalSource());

			_json.writeName("localTarget");
			_json.writeValue(obj->getLocalTarget());

			_json.writeName("additive");
			_json.writeValue(obj->getAdditive());

			_json.writeName("clamp");
			_json.writeValue(obj->getClamp());

			_json.writeName("properties");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->getProperties().size(); i++) {
				writeFromProperty(obj->getProperties()[i]);
			}
			_json.writeArrayEnd();

			_json.writeName("name");
			_json.writeValue(obj->getName());

			_json.writeName("setupPose");
			writeTransformConstraintPose(obj->getSetupPose());

			_json.writeName("skinRequired");
			_json.writeValue(obj->getSkinRequired());

			_json.writeObjectEnd();
		}

		void writeFromProperty(FromProperty *obj) {
			if (obj->getRTTI().instanceOf(FromRotate::rtti)) {
				writeFromRotate((FromRotate *) obj);
			} else if (obj->getRTTI().instanceOf(FromScaleX::rtti)) {
				writeFromScaleX((FromScaleX *) obj);
			} else if (obj->getRTTI().instanceOf(FromScaleY::rtti)) {
				writeFromScaleY((FromScaleY *) obj);
			} else if (obj->getRTTI().instanceOf(FromShearY::rtti)) {
				writeFromShearY((FromShearY *) obj);
			} else if (obj->getRTTI().instanceOf(FromX::rtti)) {
				writeFromX((FromX *) obj);
			} else if (obj->getRTTI().instanceOf(FromY::rtti)) {
				writeFromY((FromY *) obj);
			} else {
				fprintf(stderr, "Error: Unknown FromProperty type\n");
				exit(1);
			}
		}

		void writeFromRotate(FromRotate *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<FromRotate-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("FromRotate");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("to");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->_to.size(); i++) {
				writeToProperty(obj->_to[i]);
			}
			_json.writeArrayEnd();

			_json.writeObjectEnd();
		}

		void writeFromScaleX(FromScaleX *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<FromScaleX-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("FromScaleX");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("to");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->_to.size(); i++) {
				writeToProperty(obj->_to[i]);
			}
			_json.writeArrayEnd();

			_json.writeObjectEnd();
		}

		void writeFromScaleY(FromScaleY *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<FromScaleY-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("FromScaleY");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("to");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->_to.size(); i++) {
				writeToProperty(obj->_to[i]);
			}
			_json.writeArrayEnd();

			_json.writeObjectEnd();
		}

		void writeFromShearY(FromShearY *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<FromShearY-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("FromShearY");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("to");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->_to.size(); i++) {
				writeToProperty(obj->_to[i]);
			}
			_json.writeArrayEnd();

			_json.writeObjectEnd();
		}

		void writeFromX(FromX *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<FromX-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("FromX");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("to");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->_to.size(); i++) {
				writeToProperty(obj->_to[i]);
			}
			_json.writeArrayEnd();

			_json.writeObjectEnd();
		}

		void writeFromY(FromY *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<FromY-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("FromY");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("to");
			_json.writeArrayStart();
			for (size_t i = 0; i < obj->_to.size(); i++) {
				writeToProperty(obj->_to[i]);
			}
			_json.writeArrayEnd();

			_json.writeObjectEnd();
		}

		void writeToProperty(ToProperty *obj) {
			if (obj->getRTTI().instanceOf(ToRotate::rtti)) {
				writeToRotate((ToRotate *) obj);
			} else if (obj->getRTTI().instanceOf(ToScaleX::rtti)) {
				writeToScaleX((ToScaleX *) obj);
			} else if (obj->getRTTI().instanceOf(ToScaleY::rtti)) {
				writeToScaleY((ToScaleY *) obj);
			} else if (obj->getRTTI().instanceOf(ToShearY::rtti)) {
				writeToShearY((ToShearY *) obj);
			} else if (obj->getRTTI().instanceOf(ToX::rtti)) {
				writeToX((ToX *) obj);
			} else if (obj->getRTTI().instanceOf(ToY::rtti)) {
				writeToY((ToY *) obj);
			} else {
				fprintf(stderr, "Error: Unknown ToProperty type\n");
				exit(1);
			}
		}

		void writeToRotate(ToRotate *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ToRotate-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ToRotate");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("max");
			_json.writeValue(obj->_max);

			_json.writeName("scale");
			_json.writeValue(obj->_scale);

			_json.writeObjectEnd();
		}

		void writeToScaleX(ToScaleX *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ToScaleX-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ToScaleX");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("max");
			_json.writeValue(obj->_max);

			_json.writeName("scale");
			_json.writeValue(obj->_scale);

			_json.writeObjectEnd();
		}

		void writeToScaleY(ToScaleY *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ToScaleY-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ToScaleY");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("max");
			_json.writeValue(obj->_max);

			_json.writeName("scale");
			_json.writeValue(obj->_scale);

			_json.writeObjectEnd();
		}

		void writeToShearY(ToShearY *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ToShearY-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ToShearY");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("max");
			_json.writeValue(obj->_max);

			_json.writeName("scale");
			_json.writeValue(obj->_scale);

			_json.writeObjectEnd();
		}

		void writeToX(ToX *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ToX-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ToX");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("max");
			_json.writeValue(obj->_max);

			_json.writeName("scale");
			_json.writeValue(obj->_scale);

			_json.writeObjectEnd();
		}

		void writeToY(ToY *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<ToY-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("ToY");

			_json.writeName("offset");
			_json.writeValue(obj->_offset);

			_json.writeName("max");
			_json.writeValue(obj->_max);

			_json.writeName("scale");
			_json.writeValue(obj->_scale);

			_json.writeObjectEnd();
		}

		void writeTransformConstraintPose(TransformConstraintPose *obj) {
			if (_visitedObjects.containsKey(obj)) {
				_json.writeValue(_visitedObjects[obj]);
				return;
			}
			String refString = String("<TransformConstraintPose-").append(_nextId++).append(">");
			_visitedObjects.put(obj, refString);

			_json.writeObjectStart();
			_json.writeName("refString");
			_json.writeValue(refString);
			_json.writeName("type");
			_json.writeValue("TransformConstraintPose");

			_json.writeName("mixRotate");
			_json.writeValue(obj->getMixRotate());

			_json.writeName("mixX");
			_json.writeValue(obj->getMixX());

			_json.writeName("mixY");
			_json.writeValue(obj->getMixY());

			_json.writeName("mixScaleX");
			_json.writeValue(obj->getMixScaleX());

			_json.writeName("mixScaleY");
			_json.writeValue(obj->getMixScaleY());

			_json.writeName("mixShearY");
			_json.writeValue(obj->getMixShearY());

			_json.writeObjectEnd();
		}

		void writeUpdate(Update *obj) {
			if (obj->getRTTI().instanceOf(BonePose::rtti)) {
				writeBonePose((BonePose *) obj);
			} else if (obj->getRTTI().instanceOf(IkConstraint::rtti)) {
				writeIkConstraint((IkConstraint *) obj);
			} else if (obj->getRTTI().instanceOf(PathConstraint::rtti)) {
				writePathConstraint((PathConstraint *) obj);
			} else if (obj->getRTTI().instanceOf(PhysicsConstraint::rtti)) {
				writePhysicsConstraint((PhysicsConstraint *) obj);
			} else if (obj->getRTTI().instanceOf(Slider::rtti)) {
				writeSlider((Slider *) obj);
			} else if (obj->getRTTI().instanceOf(TransformConstraint::rtti)) {
				writeTransformConstraint((TransformConstraint *) obj);
			} else {
				fprintf(stderr, "Error: Unknown Update type\n");
				exit(1);
			}
		}

		void writeVertexAttachment(VertexAttachment *obj) {
			if (obj->getRTTI().instanceOf(BoundingBoxAttachment::rtti)) {
				writeBoundingBoxAttachment((BoundingBoxAttachment *) obj);
			} else if (obj->getRTTI().instanceOf(ClippingAttachment::rtti)) {
				writeClippingAttachment((ClippingAttachment *) obj);
			} else if (obj->getRTTI().instanceOf(MeshAttachment::rtti)) {
				writeMeshAttachment((MeshAttachment *) obj);
			} else if (obj->getRTTI().instanceOf(PathAttachment::rtti)) {
				writePathAttachment((PathAttachment *) obj);
			} else {
				fprintf(stderr, "Error: Unknown VertexAttachment type\n");
				exit(1);
			}
		}

		// Custom helper methods
		void writeColor(Color *obj) {
			if (obj == nullptr) {
				_json.writeNull();
			} else {
				_json.writeObjectStart();
				_json.writeName("r");
				_json.writeValue(obj->r);
				_json.writeName("g");
				_json.writeValue(obj->g);
				_json.writeName("b");
				_json.writeValue(obj->b);
				_json.writeName("a");
				_json.writeValue(obj->a);
				_json.writeObjectEnd();
			}
		}

		void writeColor(const Color &obj) {
			_json.writeObjectStart();
			_json.writeName("r");
			_json.writeValue(obj.r);
			_json.writeName("g");
			_json.writeValue(obj.g);
			_json.writeName("b");
			_json.writeValue(obj.b);
			_json.writeName("a");
			_json.writeValue(obj.a);
			_json.writeObjectEnd();
		}

		void writeTextureRegion(TextureRegion *obj) {
			if (obj == nullptr) {
				_json.writeNull();
			} else {
				_json.writeObjectStart();
				_json.writeName("u");
				_json.writeValue(obj->getU());
				_json.writeName("v");
				_json.writeValue(obj->getV());
				_json.writeName("u2");
				_json.writeValue(obj->getU2());
				_json.writeName("v2");
				_json.writeValue(obj->getV2());
				_json.writeName("width");
				_json.writeValue(obj->getRegionWidth());
				_json.writeName("height");
				_json.writeValue(obj->getRegionHeight());
				_json.writeObjectEnd();
			}
		}

		void writeTextureRegion(const TextureRegion &obj) {
			_json.writeObjectStart();
			_json.writeName("u");
			_json.writeValue(obj.getU());
			_json.writeName("v");
			_json.writeValue(obj.getV());
			_json.writeName("u2");
			_json.writeValue(obj.getU2());
			_json.writeName("v2");
			_json.writeValue(obj.getV2());
			_json.writeName("width");
			_json.writeValue(obj.getRegionWidth());
			_json.writeName("height");
			_json.writeValue(obj.getRegionHeight());
			_json.writeObjectEnd();
		}

		void writeIntArray(const Array<int> &obj) {
			_json.writeArrayStart();
			for (size_t i = 0; i < obj.size(); i++) {
				_json.writeValue(obj[i]);
			}
			_json.writeArrayEnd();
		}

		void writeFloatArray(const Array<float> &obj) {
			_json.writeArrayStart();
			for (size_t i = 0; i < obj.size(); i++) {
				_json.writeValue(obj[i]);
			}
			_json.writeArrayEnd();
		}

		// Reference versions of write methods
		void writeAnimation(const Animation &obj) {
			writeAnimation(const_cast<Animation *>(&obj));
		}

		void writeAlphaTimeline(const AlphaTimeline &obj) {
			writeAlphaTimeline(const_cast<AlphaTimeline *>(&obj));
		}

		void writeAttachmentTimeline(const AttachmentTimeline &obj) {
			writeAttachmentTimeline(const_cast<AttachmentTimeline *>(&obj));
		}

		void writeDeformTimeline(const DeformTimeline &obj) {
			writeDeformTimeline(const_cast<DeformTimeline *>(&obj));
		}

		void writeDrawOrderTimeline(const DrawOrderTimeline &obj) {
			writeDrawOrderTimeline(const_cast<DrawOrderTimeline *>(&obj));
		}

		void writeEventTimeline(const EventTimeline &obj) {
			writeEventTimeline(const_cast<EventTimeline *>(&obj));
		}

		void writeIkConstraintTimeline(const IkConstraintTimeline &obj) {
			writeIkConstraintTimeline(const_cast<IkConstraintTimeline *>(&obj));
		}

		void writeInheritTimeline(const InheritTimeline &obj) {
			writeInheritTimeline(const_cast<InheritTimeline *>(&obj));
		}

		void writePathConstraintMixTimeline(const PathConstraintMixTimeline &obj) {
			writePathConstraintMixTimeline(const_cast<PathConstraintMixTimeline *>(&obj));
		}

		void writePathConstraintPositionTimeline(const PathConstraintPositionTimeline &obj) {
			writePathConstraintPositionTimeline(const_cast<PathConstraintPositionTimeline *>(&obj));
		}

		void writePathConstraintSpacingTimeline(const PathConstraintSpacingTimeline &obj) {
			writePathConstraintSpacingTimeline(const_cast<PathConstraintSpacingTimeline *>(&obj));
		}

		void writePhysicsConstraintDampingTimeline(const PhysicsConstraintDampingTimeline &obj) {
			writePhysicsConstraintDampingTimeline(const_cast<PhysicsConstraintDampingTimeline *>(&obj));
		}

		void writePhysicsConstraintGravityTimeline(const PhysicsConstraintGravityTimeline &obj) {
			writePhysicsConstraintGravityTimeline(const_cast<PhysicsConstraintGravityTimeline *>(&obj));
		}

		void writePhysicsConstraintInertiaTimeline(const PhysicsConstraintInertiaTimeline &obj) {
			writePhysicsConstraintInertiaTimeline(const_cast<PhysicsConstraintInertiaTimeline *>(&obj));
		}

		void writePhysicsConstraintMassTimeline(const PhysicsConstraintMassTimeline &obj) {
			writePhysicsConstraintMassTimeline(const_cast<PhysicsConstraintMassTimeline *>(&obj));
		}

		void writePhysicsConstraintMixTimeline(const PhysicsConstraintMixTimeline &obj) {
			writePhysicsConstraintMixTimeline(const_cast<PhysicsConstraintMixTimeline *>(&obj));
		}

		void writePhysicsConstraintResetTimeline(const PhysicsConstraintResetTimeline &obj) {
			writePhysicsConstraintResetTimeline(const_cast<PhysicsConstraintResetTimeline *>(&obj));
		}

		void writePhysicsConstraintStrengthTimeline(const PhysicsConstraintStrengthTimeline &obj) {
			writePhysicsConstraintStrengthTimeline(const_cast<PhysicsConstraintStrengthTimeline *>(&obj));
		}

		void writePhysicsConstraintWindTimeline(const PhysicsConstraintWindTimeline &obj) {
			writePhysicsConstraintWindTimeline(const_cast<PhysicsConstraintWindTimeline *>(&obj));
		}

		void writeRGB2Timeline(const RGB2Timeline &obj) {
			writeRGB2Timeline(const_cast<RGB2Timeline *>(&obj));
		}

		void writeRGBA2Timeline(const RGBA2Timeline &obj) {
			writeRGBA2Timeline(const_cast<RGBA2Timeline *>(&obj));
		}

		void writeRGBATimeline(const RGBATimeline &obj) {
			writeRGBATimeline(const_cast<RGBATimeline *>(&obj));
		}

		void writeRGBTimeline(const RGBTimeline &obj) {
			writeRGBTimeline(const_cast<RGBTimeline *>(&obj));
		}

		void writeRotateTimeline(const RotateTimeline &obj) {
			writeRotateTimeline(const_cast<RotateTimeline *>(&obj));
		}

		void writeScaleTimeline(const ScaleTimeline &obj) {
			writeScaleTimeline(const_cast<ScaleTimeline *>(&obj));
		}

		void writeScaleXTimeline(const ScaleXTimeline &obj) {
			writeScaleXTimeline(const_cast<ScaleXTimeline *>(&obj));
		}

		void writeScaleYTimeline(const ScaleYTimeline &obj) {
			writeScaleYTimeline(const_cast<ScaleYTimeline *>(&obj));
		}

		void writeSequenceTimeline(const SequenceTimeline &obj) {
			writeSequenceTimeline(const_cast<SequenceTimeline *>(&obj));
		}

		void writeShearTimeline(const ShearTimeline &obj) {
			writeShearTimeline(const_cast<ShearTimeline *>(&obj));
		}

		void writeShearXTimeline(const ShearXTimeline &obj) {
			writeShearXTimeline(const_cast<ShearXTimeline *>(&obj));
		}

		void writeShearYTimeline(const ShearYTimeline &obj) {
			writeShearYTimeline(const_cast<ShearYTimeline *>(&obj));
		}

		void writeSliderMixTimeline(const SliderMixTimeline &obj) {
			writeSliderMixTimeline(const_cast<SliderMixTimeline *>(&obj));
		}

		void writeSliderTimeline(const SliderTimeline &obj) {
			writeSliderTimeline(const_cast<SliderTimeline *>(&obj));
		}

		void writeTransformConstraintTimeline(const TransformConstraintTimeline &obj) {
			writeTransformConstraintTimeline(const_cast<TransformConstraintTimeline *>(&obj));
		}

		void writeTranslateTimeline(const TranslateTimeline &obj) {
			writeTranslateTimeline(const_cast<TranslateTimeline *>(&obj));
		}

		void writeTranslateXTimeline(const TranslateXTimeline &obj) {
			writeTranslateXTimeline(const_cast<TranslateXTimeline *>(&obj));
		}

		void writeTranslateYTimeline(const TranslateYTimeline &obj) {
			writeTranslateYTimeline(const_cast<TranslateYTimeline *>(&obj));
		}

		void writeAnimationState(const AnimationState &obj) {
			writeAnimationState(const_cast<AnimationState *>(&obj));
		}

		void writeTrackEntry(const TrackEntry &obj) {
			writeTrackEntry(const_cast<TrackEntry *>(&obj));
		}

		void writeAnimationStateData(const AnimationStateData &obj) {
			writeAnimationStateData(const_cast<AnimationStateData *>(&obj));
		}

		void writeBone(const Bone &obj) {
			writeBone(const_cast<Bone *>(&obj));
		}

		void writeBoneData(const BoneData &obj) {
			writeBoneData(const_cast<BoneData *>(&obj));
		}

		void writeBoneLocal(const BoneLocal &obj) {
			writeBoneLocal(const_cast<BoneLocal *>(&obj));
		}

		void writeBonePose(const BonePose &obj) {
			writeBonePose(const_cast<BonePose *>(&obj));
		}

		void writeBoundingBoxAttachment(const BoundingBoxAttachment &obj) {
			writeBoundingBoxAttachment(const_cast<BoundingBoxAttachment *>(&obj));
		}

		void writeClippingAttachment(const ClippingAttachment &obj) {
			writeClippingAttachment(const_cast<ClippingAttachment *>(&obj));
		}

		void writeEvent(const Event &obj) {
			writeEvent(const_cast<Event *>(&obj));
		}

		void writeEventData(const EventData &obj) {
			writeEventData(const_cast<EventData *>(&obj));
		}

		void writeIkConstraint(const IkConstraint &obj) {
			writeIkConstraint(const_cast<IkConstraint *>(&obj));
		}

		void writeIkConstraintData(const IkConstraintData &obj) {
			writeIkConstraintData(const_cast<IkConstraintData *>(&obj));
		}

		void writeIkConstraintPose(const IkConstraintPose &obj) {
			writeIkConstraintPose(const_cast<IkConstraintPose *>(&obj));
		}

		void writeMeshAttachment(const MeshAttachment &obj) {
			writeMeshAttachment(const_cast<MeshAttachment *>(&obj));
		}

		void writePathAttachment(const PathAttachment &obj) {
			writePathAttachment(const_cast<PathAttachment *>(&obj));
		}

		void writePathConstraint(const PathConstraint &obj) {
			writePathConstraint(const_cast<PathConstraint *>(&obj));
		}

		void writePathConstraintData(const PathConstraintData &obj) {
			writePathConstraintData(const_cast<PathConstraintData *>(&obj));
		}

		void writePathConstraintPose(const PathConstraintPose &obj) {
			writePathConstraintPose(const_cast<PathConstraintPose *>(&obj));
		}

		void writePhysicsConstraint(const PhysicsConstraint &obj) {
			writePhysicsConstraint(const_cast<PhysicsConstraint *>(&obj));
		}

		void writePhysicsConstraintData(const PhysicsConstraintData &obj) {
			writePhysicsConstraintData(const_cast<PhysicsConstraintData *>(&obj));
		}

		void writePhysicsConstraintPose(const PhysicsConstraintPose &obj) {
			writePhysicsConstraintPose(const_cast<PhysicsConstraintPose *>(&obj));
		}

		void writePointAttachment(const PointAttachment &obj) {
			writePointAttachment(const_cast<PointAttachment *>(&obj));
		}

		void writeRegionAttachment(const RegionAttachment &obj) {
			writeRegionAttachment(const_cast<RegionAttachment *>(&obj));
		}

		void writeSequence(const Sequence &obj) {
			writeSequence(const_cast<Sequence *>(&obj));
		}

		void writeSkeleton(const Skeleton &obj) {
			writeSkeleton(const_cast<Skeleton *>(&obj));
		}

		void writeSkeletonData(const SkeletonData &obj) {
			writeSkeletonData(const_cast<SkeletonData *>(&obj));
		}

		void writeSlider(const Slider &obj) {
			writeSlider(const_cast<Slider *>(&obj));
		}

		void writeSliderData(const SliderData &obj) {
			writeSliderData(const_cast<SliderData *>(&obj));
		}

		void writeSliderPose(const SliderPose &obj) {
			writeSliderPose(const_cast<SliderPose *>(&obj));
		}

		void writeSlot(const Slot &obj) {
			writeSlot(const_cast<Slot *>(&obj));
		}

		void writeSlotData(const SlotData &obj) {
			writeSlotData(const_cast<SlotData *>(&obj));
		}

		void writeSlotPose(const SlotPose &obj) {
			writeSlotPose(const_cast<SlotPose *>(&obj));
		}

		void writeTransformConstraint(const TransformConstraint &obj) {
			writeTransformConstraint(const_cast<TransformConstraint *>(&obj));
		}

		void writeTransformConstraintData(const TransformConstraintData &obj) {
			writeTransformConstraintData(const_cast<TransformConstraintData *>(&obj));
		}

		void writeFromRotate(const FromRotate &obj) {
			writeFromRotate(const_cast<FromRotate *>(&obj));
		}

		void writeFromScaleX(const FromScaleX &obj) {
			writeFromScaleX(const_cast<FromScaleX *>(&obj));
		}

		void writeFromScaleY(const FromScaleY &obj) {
			writeFromScaleY(const_cast<FromScaleY *>(&obj));
		}

		void writeFromShearY(const FromShearY &obj) {
			writeFromShearY(const_cast<FromShearY *>(&obj));
		}

		void writeFromX(const FromX &obj) {
			writeFromX(const_cast<FromX *>(&obj));
		}

		void writeFromY(const FromY &obj) {
			writeFromY(const_cast<FromY *>(&obj));
		}

		void writeToRotate(const ToRotate &obj) {
			writeToRotate(const_cast<ToRotate *>(&obj));
		}

		void writeToScaleX(const ToScaleX &obj) {
			writeToScaleX(const_cast<ToScaleX *>(&obj));
		}

		void writeToScaleY(const ToScaleY &obj) {
			writeToScaleY(const_cast<ToScaleY *>(&obj));
		}

		void writeToShearY(const ToShearY &obj) {
			writeToShearY(const_cast<ToShearY *>(&obj));
		}

		void writeToX(const ToX &obj) {
			writeToX(const_cast<ToX *>(&obj));
		}

		void writeToY(const ToY &obj) {
			writeToY(const_cast<ToY *>(&obj));
		}

		void writeTransformConstraintPose(const TransformConstraintPose &obj) {
			writeTransformConstraintPose(const_cast<TransformConstraintPose *>(&obj));
		}

		// Reference versions of abstract type write methods
		void writeTimeline(const Timeline &obj) {
			writeTimeline(const_cast<Timeline *>(&obj));
		}

		void writeAttachment(const Attachment &obj) {
			writeAttachment(const_cast<Attachment *>(&obj));
		}

		void writeConstraint(const Constraint &obj) {
			writeConstraint(const_cast<Constraint *>(&obj));
		}

		void writeConstraintData(const ConstraintData &obj) {
			writeConstraintData(const_cast<ConstraintData *>(&obj));
		}

		void writeFromProperty(const FromProperty &obj) {
			writeFromProperty(const_cast<FromProperty *>(&obj));
		}

		void writeToProperty(const ToProperty &obj) {
			writeToProperty(const_cast<ToProperty *>(&obj));
		}

		void writeUpdate(const Update &obj) {
			writeUpdate(const_cast<Update *>(&obj));
		}

		void writeVertexAttachment(const VertexAttachment &obj) {
			writeVertexAttachment(const_cast<VertexAttachment *>(&obj));
		}
	};

}// namespace spine

#endif