#include "physics_constraint_data.h"
#include <spine/spine.h>

using namespace spine;

spine_physics_constraint_data spine_physics_constraint_data_create(const char *name) {
	return (spine_physics_constraint_data) new (__FILE__, __LINE__) PhysicsConstraintData(String(name));
}

void spine_physics_constraint_data_dispose(spine_physics_constraint_data self) {
	delete (PhysicsConstraintData *) self;
}

spine_rtti spine_physics_constraint_data_get_rtti(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_constraint spine_physics_constraint_data_create_method(spine_physics_constraint_data self, spine_skeleton skeleton) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return (spine_constraint) &_self->create(*((Skeleton *) skeleton));
}

spine_bone_data spine_physics_constraint_data_get_bone(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return (spine_bone_data) &_self->getBone();
}

void spine_physics_constraint_data_set_bone(spine_physics_constraint_data self, spine_bone_data bone) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setBone(*((BoneData *) bone));
}

float spine_physics_constraint_data_get_step(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getStep();
}

void spine_physics_constraint_data_set_step(spine_physics_constraint_data self, float step) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setStep(step);
}

float spine_physics_constraint_data_get_x(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getX();
}

void spine_physics_constraint_data_set_x(spine_physics_constraint_data self, float x) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setX(x);
}

float spine_physics_constraint_data_get_y(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getY();
}

void spine_physics_constraint_data_set_y(spine_physics_constraint_data self, float y) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setY(y);
}

float spine_physics_constraint_data_get_rotate(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getRotate();
}

void spine_physics_constraint_data_set_rotate(spine_physics_constraint_data self, float rotate) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setRotate(rotate);
}

float spine_physics_constraint_data_get_scale_x(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getScaleX();
}

void spine_physics_constraint_data_set_scale_x(spine_physics_constraint_data self, float scaleX) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setScaleX(scaleX);
}

float spine_physics_constraint_data_get_shear_x(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getShearX();
}

void spine_physics_constraint_data_set_shear_x(spine_physics_constraint_data self, float shearX) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setShearX(shearX);
}

float spine_physics_constraint_data_get_limit(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getLimit();
}

void spine_physics_constraint_data_set_limit(spine_physics_constraint_data self, float limit) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setLimit(limit);
}

spine_scale_y_mode spine_physics_constraint_data_get_scale_y_mode(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return (spine_scale_y_mode) _self->getScaleYMode();
}

void spine_physics_constraint_data_set_scale_y_mode(spine_physics_constraint_data self, spine_scale_y_mode scaleYMode) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setScaleYMode((ScaleYMode) scaleYMode);
}

bool spine_physics_constraint_data_get_inertia_global(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getInertiaGlobal();
}

void spine_physics_constraint_data_set_inertia_global(spine_physics_constraint_data self, bool inertiaGlobal) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setInertiaGlobal(inertiaGlobal);
}

bool spine_physics_constraint_data_get_strength_global(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getStrengthGlobal();
}

void spine_physics_constraint_data_set_strength_global(spine_physics_constraint_data self, bool strengthGlobal) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setStrengthGlobal(strengthGlobal);
}

bool spine_physics_constraint_data_get_damping_global(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getDampingGlobal();
}

void spine_physics_constraint_data_set_damping_global(spine_physics_constraint_data self, bool dampingGlobal) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setDampingGlobal(dampingGlobal);
}

bool spine_physics_constraint_data_get_mass_global(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getMassGlobal();
}

void spine_physics_constraint_data_set_mass_global(spine_physics_constraint_data self, bool massGlobal) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setMassGlobal(massGlobal);
}

bool spine_physics_constraint_data_get_wind_global(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getWindGlobal();
}

void spine_physics_constraint_data_set_wind_global(spine_physics_constraint_data self, bool windGlobal) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setWindGlobal(windGlobal);
}

bool spine_physics_constraint_data_get_gravity_global(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getGravityGlobal();
}

void spine_physics_constraint_data_set_gravity_global(spine_physics_constraint_data self, bool gravityGlobal) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setGravityGlobal(gravityGlobal);
}

bool spine_physics_constraint_data_get_mix_global(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getMixGlobal();
}

void spine_physics_constraint_data_set_mix_global(spine_physics_constraint_data self, bool mixGlobal) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setMixGlobal(mixGlobal);
}

const char *spine_physics_constraint_data_get_name(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getName().buffer();
}

bool spine_physics_constraint_data_get_skin_required(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return _self->getSkinRequired();
}

spine_physics_constraint_pose spine_physics_constraint_data_get_setup_pose(spine_physics_constraint_data self) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	return (spine_physics_constraint_pose) &_self->getSetupPose();
}

void spine_physics_constraint_data_set_skin_required(spine_physics_constraint_data self, bool skinRequired) {
	PhysicsConstraintData *_self = (PhysicsConstraintData *) self;
	_self->setSkinRequired(skinRequired);
}

spine_rtti spine_physics_constraint_data_rtti(void) {
	return (spine_rtti) &PhysicsConstraintData::rtti;
}
