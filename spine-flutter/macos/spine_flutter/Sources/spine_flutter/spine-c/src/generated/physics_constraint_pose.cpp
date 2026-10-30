#include "physics_constraint_pose.h"
#include <spine/spine.h>

using namespace spine;

spine_physics_constraint_pose spine_physics_constraint_pose_create(void) {
	return (spine_physics_constraint_pose) new (__FILE__, __LINE__) PhysicsConstraintPose();
}

void spine_physics_constraint_pose_dispose(spine_physics_constraint_pose self) {
	delete (PhysicsConstraintPose *) self;
}

void spine_physics_constraint_pose_set(spine_physics_constraint_pose self, spine_physics_constraint_pose pose) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	_self->set(*((PhysicsConstraintPose *) pose));
}

float spine_physics_constraint_pose_get_inertia(spine_physics_constraint_pose self) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	return _self->getInertia();
}

void spine_physics_constraint_pose_set_inertia(spine_physics_constraint_pose self, float inertia) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	_self->setInertia(inertia);
}

float spine_physics_constraint_pose_get_strength(spine_physics_constraint_pose self) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	return _self->getStrength();
}

void spine_physics_constraint_pose_set_strength(spine_physics_constraint_pose self, float strength) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	_self->setStrength(strength);
}

float spine_physics_constraint_pose_get_damping(spine_physics_constraint_pose self) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	return _self->getDamping();
}

void spine_physics_constraint_pose_set_damping(spine_physics_constraint_pose self, float damping) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	_self->setDamping(damping);
}

float spine_physics_constraint_pose_get_mass_inverse(spine_physics_constraint_pose self) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	return _self->getMassInverse();
}

void spine_physics_constraint_pose_set_mass_inverse(spine_physics_constraint_pose self, float massInverse) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	_self->setMassInverse(massInverse);
}

float spine_physics_constraint_pose_get_wind(spine_physics_constraint_pose self) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	return _self->getWind();
}

void spine_physics_constraint_pose_set_wind(spine_physics_constraint_pose self, float wind) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	_self->setWind(wind);
}

float spine_physics_constraint_pose_get_gravity(spine_physics_constraint_pose self) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	return _self->getGravity();
}

void spine_physics_constraint_pose_set_gravity(spine_physics_constraint_pose self, float gravity) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	_self->setGravity(gravity);
}

float spine_physics_constraint_pose_get_mix(spine_physics_constraint_pose self) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	return _self->getMix();
}

void spine_physics_constraint_pose_set_mix(spine_physics_constraint_pose self, float mix) {
	PhysicsConstraintPose *_self = (PhysicsConstraintPose *) self;
	_self->setMix(mix);
}
