#include "skeleton.h"
#include <spine/spine.h>

using namespace spine;

spine_skeleton spine_skeleton_create(spine_skeleton_data skeletonData) {
	return (spine_skeleton) new (__FILE__, __LINE__) Skeleton(*((SkeletonData *) skeletonData));
}

void spine_skeleton_dispose(spine_skeleton self) {
	delete (Skeleton *) self;
}

void spine_skeleton_update_cache(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	_self->updateCache();
}

void spine_skeleton_print_update_cache(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	_self->printUpdateCache();
}

void spine_skeleton_constrained(spine_skeleton self, spine_posed object) {
	Skeleton *_self = (Skeleton *) self;
	_self->constrained(*((Posed *) object));
}

void spine_skeleton_sort_bone(spine_skeleton self, /*@null*/ spine_bone bone) {
	Skeleton *_self = (Skeleton *) self;
	_self->sortBone((Bone *) bone);
}

void spine_skeleton_sort_reset(spine_array_bone bones) {
	Skeleton::sortReset(*((Array<Bone *> *) bones));
}

void spine_skeleton_update_world_transform(spine_skeleton self, spine_physics physics) {
	Skeleton *_self = (Skeleton *) self;
	_self->updateWorldTransform((Physics) physics);
}

void spine_skeleton_setup_pose(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	_self->setupPose();
}

void spine_skeleton_setup_pose_bones(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	_self->setupPoseBones();
}

void spine_skeleton_setup_pose_slots(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	_self->setupPoseSlots();
}

spine_skeleton_data spine_skeleton_get_data(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_skeleton_data) &_self->getData();
}

spine_array_bone spine_skeleton_get_bones(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_array_bone) &_self->getBones();
}

spine_array_update spine_skeleton_get_update_cache(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_array_update) &_self->getUpdateCache();
}

/*@null*/ spine_bone spine_skeleton_get_root_bone(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_bone) _self->getRootBone();
}

/*@null*/ spine_bone spine_skeleton_find_bone(spine_skeleton self, const char *boneName) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_bone) _self->findBone(String(boneName));
}

spine_array_slot spine_skeleton_get_slots(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_array_slot) &_self->getSlots();
}

/*@null*/ spine_slot spine_skeleton_find_slot(spine_skeleton self, const char *slotName) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_slot) _self->findSlot(String(slotName));
}

spine_draw_order spine_skeleton_get_draw_order(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_draw_order) &_self->getDrawOrder();
}

/*@null*/ spine_skin spine_skeleton_get_skin(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_skin) _self->getSkin();
}

void spine_skeleton_set_skin_1(spine_skeleton self, const char *skinName) {
	Skeleton *_self = (Skeleton *) self;
	_self->setSkin(String(skinName));
}

void spine_skeleton_set_skin_2(spine_skeleton self, /*@null*/ spine_skin newSkin) {
	Skeleton *_self = (Skeleton *) self;
	_self->setSkin((Skin *) newSkin);
}

/*@null*/ spine_attachment spine_skeleton_get_attachment_1(spine_skeleton self, const char *slotName, const char *placeholder) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_attachment) _self->getAttachment(String(slotName), String(placeholder));
}

/*@null*/ spine_attachment spine_skeleton_get_attachment_2(spine_skeleton self, int slotIndex, const char *placeholder) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_attachment) _self->getAttachment(slotIndex, String(placeholder));
}

void spine_skeleton_set_attachment(spine_skeleton self, const char *slotName, const char *placeholder) {
	Skeleton *_self = (Skeleton *) self;
	_self->setAttachment(String(slotName), String(placeholder));
}

spine_array_constraint spine_skeleton_get_constraints(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_array_constraint) &_self->getConstraints();
}

spine_array_physics_constraint spine_skeleton_get_physics_constraints(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_array_physics_constraint) &_self->getPhysicsConstraints();
}

void spine_skeleton_get_bounds_1(spine_skeleton self, float *outX, float *outY, float *outWidth, float *outHeight) {
	Skeleton *_self = (Skeleton *) self;
	_self->getBounds(*outX, *outY, *outWidth, *outHeight);
}

void spine_skeleton_get_bounds_2(spine_skeleton self, float *outX, float *outY, float *outWidth, float *outHeight, spine_array_float outVertexBuffer,
								 /*@null*/ spine_skeleton_clipping clipping) {
	Skeleton *_self = (Skeleton *) self;
	_self->getBounds(*outX, *outY, *outWidth, *outHeight, *((Array<float> *) outVertexBuffer), (SkeletonClipping *) clipping);
}

spine_color spine_skeleton_get_color(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return (spine_color) &_self->getColor();
}

void spine_skeleton_set_color_1(spine_skeleton self, spine_color color) {
	Skeleton *_self = (Skeleton *) self;
	_self->setColor(*((Color *) color));
}

void spine_skeleton_set_color_2(spine_skeleton self, float r, float g, float b, float a) {
	Skeleton *_self = (Skeleton *) self;
	_self->setColor(r, g, b, a);
}

float spine_skeleton_get_scale_x(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return _self->getScaleX();
}

void spine_skeleton_set_scale_x(spine_skeleton self, float inValue) {
	Skeleton *_self = (Skeleton *) self;
	_self->setScaleX(inValue);
}

float spine_skeleton_get_scale_y(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return _self->getScaleY();
}

void spine_skeleton_set_scale_y(spine_skeleton self, float inValue) {
	Skeleton *_self = (Skeleton *) self;
	_self->setScaleY(inValue);
}

void spine_skeleton_set_scale(spine_skeleton self, float scaleX, float scaleY) {
	Skeleton *_self = (Skeleton *) self;
	_self->setScale(scaleX, scaleY);
}

float spine_skeleton_get_x(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return _self->getX();
}

void spine_skeleton_set_x(spine_skeleton self, float inValue) {
	Skeleton *_self = (Skeleton *) self;
	_self->setX(inValue);
}

float spine_skeleton_get_y(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return _self->getY();
}

void spine_skeleton_set_y(spine_skeleton self, float inValue) {
	Skeleton *_self = (Skeleton *) self;
	_self->setY(inValue);
}

void spine_skeleton_set_position(spine_skeleton self, float x, float y) {
	Skeleton *_self = (Skeleton *) self;
	_self->setPosition(x, y);
}

void spine_skeleton_get_position(spine_skeleton self, float *x, float *y) {
	Skeleton *_self = (Skeleton *) self;
	_self->getPosition(*x, *y);
}

float spine_skeleton_get_wind_x(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return _self->getWindX();
}

void spine_skeleton_set_wind_x(spine_skeleton self, float windX) {
	Skeleton *_self = (Skeleton *) self;
	_self->setWindX(windX);
}

float spine_skeleton_get_wind_y(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return _self->getWindY();
}

void spine_skeleton_set_wind_y(spine_skeleton self, float windY) {
	Skeleton *_self = (Skeleton *) self;
	_self->setWindY(windY);
}

float spine_skeleton_get_gravity_x(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return _self->getGravityX();
}

void spine_skeleton_set_gravity_x(spine_skeleton self, float gravityX) {
	Skeleton *_self = (Skeleton *) self;
	_self->setGravityX(gravityX);
}

float spine_skeleton_get_gravity_y(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return _self->getGravityY();
}

void spine_skeleton_set_gravity_y(spine_skeleton self, float gravityY) {
	Skeleton *_self = (Skeleton *) self;
	_self->setGravityY(gravityY);
}

void spine_skeleton_physics_translate(spine_skeleton self, float x, float y) {
	Skeleton *_self = (Skeleton *) self;
	_self->physicsTranslate(x, y);
}

void spine_skeleton_physics_rotate(spine_skeleton self, float x, float y, float degrees) {
	Skeleton *_self = (Skeleton *) self;
	_self->physicsRotate(x, y, degrees);
}

float spine_skeleton_get_time(spine_skeleton self) {
	Skeleton *_self = (Skeleton *) self;
	return _self->getTime();
}

void spine_skeleton_set_time(spine_skeleton self, float time) {
	Skeleton *_self = (Skeleton *) self;
	_self->setTime(time);
}

void spine_skeleton_update(spine_skeleton self, float delta) {
	Skeleton *_self = (Skeleton *) self;
	_self->update(delta);
}
