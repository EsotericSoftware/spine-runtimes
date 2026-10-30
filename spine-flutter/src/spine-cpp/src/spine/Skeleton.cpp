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

#include <spine/Skeleton.h>

#include <spine/Attachment.h>
#include <spine/Bone.h>
#include <spine/BonePose.h>
#include <spine/IkConstraint.h>
#include <spine/PathConstraint.h>
#include <spine/PhysicsConstraint.h>
#include <spine/SkeletonData.h>
#include <spine/Skin.h>
#include <spine/Slider.h>
#include <spine/Slot.h>
#include <spine/TransformConstraint.h>

#include <spine/BoneData.h>
#include <spine/ClippingAttachment.h>
#include <spine/IkConstraintData.h>
#include <spine/MeshAttachment.h>
#include <spine/PathAttachment.h>
#include <spine/PathConstraintData.h>
#include <spine/PhysicsConstraintData.h>
#include <spine/RegionAttachment.h>
#include <spine/SkeletonClipping.h>
#include <spine/SlotData.h>
#include <spine/TransformConstraintData.h>

#include <spine/ArrayUtils.h>

#include <float.h>

using namespace spine;

Skeleton::Skeleton(SkeletonData &skeletonData)
	: _data(skeletonData), _drawOrder(_slots), _skin(NULL), _color(1, 1, 1, 1), _x(0), _y(0), _scaleX(1), _scaleY(1), _windX(1), _windY(0),
	  _gravityX(0), _gravityY(1), _time(0), _update(0) {

	_bones.ensureCapacity(_data.getBones().size());
	for (size_t i = 0; i < _data.getBones().size(); ++i) {
		BoneData *data = _data.getBones()[i];

		Bone *bone;
		if (data->getParent() == NULL) {
			bone = new (__FILE__, __LINE__) Bone(*data, NULL);
		} else {
			Bone *parent = _bones[data->getParent()->getIndex()];
			bone = new (__FILE__, __LINE__) Bone(*data, parent);
			parent->getChildren().add(bone);
		}

		_bones.add(bone);
	}

	_slots.ensureCapacity(_data.getSlots().size());
	for (size_t i = 0; i < _data.getSlots().size(); ++i) {
		SlotData *data = _data.getSlots()[i];
		Slot *slot = new (__FILE__, __LINE__) Slot(*data, *this);
		_slots.add(slot);
	}
	_drawOrder.setupPose();

	_physics.ensureCapacity(8);
	_constraints.ensureCapacity(_data.getConstraints().size());
	for (size_t i = 0; i < _data.getConstraints().size(); ++i) {
		ConstraintData *constraintData = _data.getConstraints()[i];
		Constraint *constraint = &constraintData->create(*this);
		if (constraint->getRTTI().instanceOf(PhysicsConstraint::rtti)) {
			_physics.add(static_cast<PhysicsConstraint *>(constraint));
		}
		_constraints.add(constraint);
	}

	updateCache();
}

Skeleton::~Skeleton() {
	ArrayUtils::deleteElements(_bones);
	ArrayUtils::deleteElements(_slots);
	ArrayUtils::deleteElements(_constraints);
}

void Skeleton::updateCache() {
	_updateCache.clear();
	_resetCache.clear();

	_drawOrder.unconstrained();
	Slot **slots = _slots.buffer();
	for (size_t i = 0, n = _slots.size(); i < n; i++) {
		slots[i]->unconstrained();
	}

	size_t boneCount = _bones.size();
	Bone **bones = _bones.buffer();
	for (size_t i = 0; i < boneCount; i++) {
		Bone *bone = bones[i];
		bone->_sorted = bone->_data.getSkinRequired();
		bone->_active = !bone->_sorted;
		bone->unconstrained();
	}

	if (_skin) {
		Array<BoneData *> &skinBones = _skin->getBones();
		for (size_t i = 0, n = skinBones.size(); i < n; i++) {
			Bone *bone = _bones[skinBones[i]->getIndex()];
			do {
				bone->_sorted = false;
				bone->_active = true;
				bone = bone->_parent;
			} while (bone);
		}
	}

	Constraint **constraints = _constraints.buffer();
	size_t n = _constraints.size();
	for (size_t i = 0; i < n; i++) {
		constraints[i]->unconstrained();
	}
	for (size_t i = 0; i < n; i++) {
		Constraint *constraint = constraints[i];
		constraint->_active = constraint->isSourceActive() &&
			((!constraint->getData().getSkinRequired()) || (_skin && _skin->_constraints.contains(&constraint->getData())));
		if (constraint->_active) constraint->sort(*this);
	}

	for (size_t i = 0; i < boneCount; i++) {
		sortBone(bones[i]);
	}

	Update **updateCache = _updateCache.buffer();
	n = _updateCache.size();
	for (size_t i = 0; i < n; i++) {
		const RTTI &rtti = updateCache[i]->getRTTI();
		if (rtti.instanceOf(Bone::rtti)) {
			Bone *bone = (Bone *) (updateCache[i]);
			updateCache[i] = bone->_appliedPose;
		}
	}
}

void Skeleton::printUpdateCache() {
	for (size_t i = 0; i < _updateCache.size(); i++) {
		Update *updatable = _updateCache[i];
		if (updatable->getRTTI().isExactly(Bone::rtti)) {
			printf("bone %s\n", ((Bone *) updatable)->getData().getName().buffer());
		} else if (updatable->getRTTI().isExactly(TransformConstraint::rtti)) {
			printf("transform constraint %s\n", ((TransformConstraint *) updatable)->getData().getName().buffer());
		} else if (updatable->getRTTI().isExactly(IkConstraint::rtti)) {
			printf("ik constraint %s\n", ((IkConstraint *) updatable)->getData().getName().buffer());
		} else if (updatable->getRTTI().isExactly(PathConstraint::rtti)) {
			printf("path constraint %s\n", ((PathConstraint *) updatable)->getData().getName().buffer());
		} else if (updatable->getRTTI().isExactly(PhysicsConstraint::rtti)) {
			printf("physics constraint %s\n", ((PhysicsConstraint *) updatable)->getData().getName().buffer());
		} else if (updatable->getRTTI().isExactly(Slider::rtti)) {
			printf("slider %s\n", ((Slider *) updatable)->getData().getName().buffer());
		}
	}
}

void Skeleton::constrained(Posed &object) {
	if (object.isPoseEqualToApplied()) {
		object.constrained();
		_resetCache.add(&object);
	}
}

void Skeleton::sortBone(Bone *bone) {
	if (bone->_sorted || !bone->_active) return;
	Bone *parent = bone->_parent;
	if (parent != NULL) sortBone(parent);
	bone->_sorted = true;
	_updateCache.add((Update *) bone);
}

void Skeleton::sortReset(Array<Bone *> &bones) {
	Bone **items = bones.buffer();
	for (size_t i = 0, n = bones.size(); i < n; i++) {
		Bone *bone = items[i];
		if (bone->_active) {
			if (bone->_sorted) sortReset(bone->getChildren());
			bone->_sorted = false;
		}
	}
}

void Skeleton::updateWorldTransform(Physics physics) {
	_update++;

	if (_drawOrder._appliedPose == &_drawOrder._constrainedPose) _drawOrder.reset();
	Posed **resetCache = _resetCache.buffer();
	for (size_t i = 0, n = _resetCache.size(); i < n; i++) {
		resetCache[i]->resetConstrained();
	}

	Update **updateCache = _updateCache.buffer();
	for (size_t i = 0, n = _updateCache.size(); i < n; i++) {
		updateCache[i]->update(*this, physics);
	}
}

void Skeleton::setupPose() {
	setupPoseBones();
	setupPoseSlots();
}

void Skeleton::setupPoseBones() {
	Bone **bones = _bones.buffer();
	for (size_t i = 0, n = _bones.size(); i < n; ++i) {
		bones[i]->setupPose();
	}

	Constraint **constraints = _constraints.buffer();
	for (size_t i = 0, n = _constraints.size(); i < n; ++i) {
		constraints[i]->setupPose();
	}
}

void Skeleton::setupPoseSlots() {
	Slot **slots = _slots.buffer();
	size_t n = _slots.size();
	_drawOrder.setupPose();
	for (size_t i = 0; i < n; ++i) {
		slots[i]->setupPose();
	}
}

SkeletonData &Skeleton::getData() {
	return _data;
}

Array<Bone *> &Skeleton::getBones() {
	return _bones;
}

Array<Update *> &Skeleton::getUpdateCache() {
	return _updateCache;
}

Bone *Skeleton::getRootBone() {
	return _bones.size() == 0 ? NULL : _bones[0];
}

Bone *Skeleton::findBone(const String &boneName) {
	if (boneName.isEmpty()) return NULL;
	Bone **bones = _bones.buffer();
	for (size_t i = 0, n = _bones.size(); i < n; i++) {
		if (bones[i]->_data.getName() == boneName) return bones[i];
	}
	return NULL;
}

Array<Slot *> &Skeleton::getSlots() {
	return _slots;
}

Slot *Skeleton::findSlot(const String &slotName) {
	if (slotName.isEmpty()) return NULL;
	Slot **slots = _slots.buffer();
	for (size_t i = 0, n = _slots.size(); i < n; i++) {
		if (slots[i]->_data.getName() == slotName) return slots[i];
	}
	return NULL;
}

DrawOrder &Skeleton::getDrawOrder() {
	return _drawOrder;
}

Skin *Skeleton::getSkin() {
	return _skin;
}

void Skeleton::setSkin(const String &skinName) {
	Skin *skin = skinName.isEmpty() ? NULL : _data.findSkin(skinName);
	if (skin == NULL) return;
	setSkin(skin);
}

void Skeleton::setSkin(Skin *newSkin) {
	if (_skin == newSkin) return;
	if (newSkin != NULL) {
		if (_skin != NULL) {
			newSkin->attachAll(*this, *_skin);
		} else {
			Slot **slots = _slots.buffer();
			for (size_t i = 0, n = _slots.size(); i < n; ++i) {
				Slot *slot = slots[i];
				const String &name = slot->_data.getAttachmentName();
				if (name.length() > 0) {
					Attachment *attachment = newSkin->getAttachment(i, name);
					if (attachment != NULL) {
						slot->_pose.setAttachment(attachment);
					}
				}
			}
		}
	}
	_skin = newSkin;
	updateCache();
}

Attachment *Skeleton::getAttachment(const String &slotName, const String &placeholder) {
	SlotData *slot = _data.findSlot(slotName);
	if (slot == NULL) return NULL;
	return getAttachment(slot->getIndex(), placeholder);
}

Attachment *Skeleton::getAttachment(int slotIndex, const String &placeholder) {
	if (placeholder.isEmpty()) return NULL;
	if (_skin != NULL) {
		Attachment *attachment = _skin->getAttachment(slotIndex, placeholder);
		if (attachment != NULL) return attachment;
	}
	if (_data.getDefaultSkin() != NULL) return _data.getDefaultSkin()->getAttachment(slotIndex, placeholder);
	return NULL;
}

void Skeleton::setAttachment(const String &slotName, const String &placeholder) {
	if (slotName.isEmpty()) return;
	Slot *slot = findSlot(slotName);
	if (slot == NULL) return;
	Attachment *attachment = NULL;
	if (!placeholder.isEmpty()) {
		attachment = getAttachment(slot->_data.getIndex(), placeholder);
		if (attachment == NULL) return;
	}
	slot->_pose.setAttachment(attachment);
}

Array<Constraint *> &Skeleton::getConstraints() {
	return _constraints;
}

Array<PhysicsConstraint *> &Skeleton::getPhysicsConstraints() {
	return _physics;
}

void Skeleton::getBounds(float &outX, float &outY, float &outWidth, float &outHeight) {
	Array<float> outVertexBuffer;
	getBounds(outX, outY, outWidth, outHeight, outVertexBuffer, NULL);
}

void Skeleton::getBounds(float &outX, float &outY, float &outWidth, float &outHeight, Array<float> &outVertexBuffer, SkeletonClipping *clipper) {
	static unsigned short quadIndices[] = {0, 1, 2, 2, 3, 0};
	float minX = FLT_MAX;
	float minY = FLT_MAX;
	float maxX = -FLT_MAX;
	float maxY = -FLT_MAX;

	Array<Slot *> &drawOrder = _drawOrder.getAppliedPose();
	Slot **drawOrderSlots = drawOrder.buffer();
	for (size_t i = 0, n = drawOrder.size(); i < n; ++i) {
		Slot *slot = drawOrderSlots[i];
		if (!slot->_bone._active) continue;
		size_t verticesLength = 0;
		float *vertices = NULL;
		unsigned short *triangles = NULL;
		size_t trianglesLength = 0;
		Attachment *attachment = slot->getAppliedPose().getAttachment();

		if (attachment != NULL) {
			if (attachment->getRTTI().instanceOf(RegionAttachment::rtti)) {
				RegionAttachment *regionAttachment = static_cast<RegionAttachment *>(attachment);
				verticesLength = 8;
				outVertexBuffer.setSize(8, 0);
				regionAttachment->computeWorldVertices(*slot, regionAttachment->getOffsets(slot->getAppliedPose()), outVertexBuffer, 0, 2);
				vertices = outVertexBuffer.buffer();
				triangles = quadIndices;
				trianglesLength = 6;
			} else if (attachment->getRTTI().instanceOf(MeshAttachment::rtti)) {
				MeshAttachment *mesh = static_cast<MeshAttachment *>(attachment);
				verticesLength = mesh->getWorldVerticesLength();
				outVertexBuffer.setSize(verticesLength, 0);
				mesh->computeWorldVertices(*this, *slot, 0, verticesLength, outVertexBuffer.buffer(), 0, 2);
				vertices = outVertexBuffer.buffer();
				triangles = mesh->getTriangles().buffer();
				trianglesLength = mesh->getTriangles().size();
			} else if (attachment->getRTTI().instanceOf(ClippingAttachment::rtti) && clipper != NULL) {
				clipper->clipEnd(*slot);
				clipper->clipStart(*this, *slot, static_cast<ClippingAttachment *>(attachment));
				continue;
			}

			if (vertices != NULL) {
				if (clipper != NULL && clipper->isClipping() && clipper->clipTriangles(vertices, triangles, trianglesLength)) {
					vertices = clipper->getClippedVertices().buffer();
					verticesLength = clipper->getClippedVertices().size();
				}
				for (size_t ii = 0; ii < verticesLength; ii += 2) {
					float x = vertices[ii], y = vertices[ii + 1];
					minX = MathUtil::min(minX, x);
					minY = MathUtil::min(minY, y);
					maxX = MathUtil::max(maxX, x);
					maxY = MathUtil::max(maxY, y);
				}
			}
		}
		if (clipper != NULL) clipper->clipEnd(*slot);
	}
	if (clipper != NULL) clipper->clipEnd();

	outX = minX;
	outY = minY;
	outWidth = maxX - minX;
	outHeight = maxY - minY;
}

Color &Skeleton::getColor() {
	return _color;
}

void Skeleton::setColor(Color &color) {
	_color.set(color.r, color.g, color.b, color.a);
}

void Skeleton::setColor(float r, float g, float b, float a) {
	_color.set(r, g, b, a);
}

float Skeleton::getScaleX() {
	return _scaleX;
}

void Skeleton::setScaleX(float inValue) {
	_scaleX = inValue;
}

float Skeleton::getScaleY() {
	return _scaleY * (Bone::isYDown() ? -1 : 1);
}

void Skeleton::setScaleY(float inValue) {
	_scaleY = inValue;
}

void Skeleton::setScale(float scaleX, float scaleY) {
	_scaleX = scaleX;
	_scaleY = scaleY;
}

float Skeleton::getX() {
	return _x;
}

void Skeleton::setX(float inValue) {
	_x = inValue;
}

float Skeleton::getY() {
	return _y;
}

void Skeleton::setY(float inValue) {
	_y = inValue;
}

void Skeleton::setPosition(float x, float y) {
	_x = x;
	_y = y;
}

void Skeleton::getPosition(float &x, float &y) {
	x = _x;
	y = _y;
}

float Skeleton::getWindX() {
	return _windX;
}

void Skeleton::setWindX(float windX) {
	_windX = windX;
}

float Skeleton::getWindY() {
	return _windY;
}

void Skeleton::setWindY(float windY) {
	_windY = windY;
}

float Skeleton::getGravityX() {
	return _gravityX;
}

void Skeleton::setGravityX(float gravityX) {
	_gravityX = gravityX;
}

float Skeleton::getGravityY() {
	return _gravityY;
}

void Skeleton::setGravityY(float gravityY) {
	_gravityY = gravityY;
}

void Skeleton::physicsTranslate(float x, float y) {
	PhysicsConstraint **constraints = _physics.buffer();
	for (size_t i = 0, n = _physics.size(); i < n; i++) {
		constraints[i]->translate(x, y);
	}
}

void Skeleton::physicsRotate(float x, float y, float degrees) {
	PhysicsConstraint **constraints = _physics.buffer();
	for (size_t i = 0, n = _physics.size(); i < n; i++) {
		constraints[i]->rotate(x, y, degrees);
	}
}

float Skeleton::getTime() {
	return _time;
}

void Skeleton::setTime(float time) {
	_time = time;
}

void Skeleton::update(float delta) {
	_time += delta;
}