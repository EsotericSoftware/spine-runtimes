/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/Skeleton.h>
#include <spine/PhysicsConstraint.h>
#include <spine/extension.h>

spPhysicsConstraint *spPhysicsConstraint_create(spPhysicsConstraintData *data, spSkeleton *skeleton) {
	spPhysicsConstraint *self = NEW(spPhysicsConstraint);
	self->data = data;
	self->skeleton = skeleton;
	self->bone = skeleton->bones[data->bone->index];
	self->inertia = data->inertia;
	self->strength = data->strength;
	self->damping = data->damping;
	self->massInverse = data->massInverse;
	self->wind = data->wind;
	self->gravity = data->gravity;
	self->mix = data->mix;

	self->reset = -1;
	self->ux = 0;
	self->uy = 0;
	self->cx = 0;
	self->tx = 0;
	self->ty = 0;
	self->xOffset = 0;
	self->xVelocity = 0;
	self->yOffset = 0;
	self->yVelocity = 0;
	self->rotateOffset = 0;
	self->rotateVelocity = 0;
	self->scaleOffset = 0;
	self->scaleVelocity = 0;
	self->active = 0;
	self->remaining = 0;
	self->lastTime = 0;
	return self;
}

void spPhysicsConstraint_dispose(spPhysicsConstraint *self) {
	FREE(self);
}

void spPhysicsConstraint_reset(spPhysicsConstraint *self) {
	self->remaining = 0;
	self->lastTime = self->skeleton->time;
	self->reset = -1;
	self->xOffset = 0;
	self->xVelocity = 0;
	self->yOffset = 0;
	self->yVelocity = 0;
	self->rotateOffset = 0;
	self->rotateVelocity = 0;
	self->scaleOffset = 0;
	self->scaleVelocity = 0;
}

void spPhysicsConstraint_setToSetupPose(spPhysicsConstraint *self) {
	self->inertia = self->data->inertia;
	self->strength = self->data->strength;
	self->damping = self->data->damping;
	self->massInverse = self->data->massInverse;
	self->wind = self->data->wind;
	self->gravity = self->data->gravity;
	self->mix = self->data->mix;
}

void spPhysicsConstraint_update(spPhysicsConstraint *self, spPhysics physics) {
	float mix = self->mix;
	if (mix == 0) return;

	int x = self->data->x > 0;
	int y = self->data->y > 0;
	int rotateOrShearX = self->data->rotate > 0 || self->data->shearX > 0;
	int scaleX = self->data->scaleX > 0;

	spBone *bone = self->bone;
	float l = bone->data->length;

	switch (physics) {
		case SP_PHYSICS_NONE:
			return;
		case SP_PHYSICS_RESET:
			spPhysicsConstraint_reset(self);
			// Fall through.
		case SP_PHYSICS_UPDATE: {
			float delta = MAX(self->skeleton->time - self->lastTime, 0.0f);
			self->remaining += delta;
			self->lastTime = self->skeleton->time;

			float bx = bone->worldX, by = bone->worldY;
			if (self->reset) {
				self->reset = 0;
				self->ux = bx;
				self->uy = by;
			} else {
				// float a = self->remaining, i = self->inertia, q = self->data->limit * delta, t = self->data->step, f = self->skeleton->data->referenceScale, d = -1;
				float a = self->remaining, i = self->inertia, t = self->data->step, f = self->skeleton->data->referenceScale;
				float qx = self->data->limit * delta, qy = qx * ABS(self->skeleton->scaleX);
				qx *= ABS(self->skeleton->scaleY);
				if (x || y) {
					if (x) {
						float u = (self->ux - bx) * i;
						self->xOffset += u > qx ? qx : u < -qx ? -qx
															   : u;
						self->ux = bx;
					}
					if (y) {
						float u = (self->uy - by) * i;
						self->yOffset += u > qy ? qy : u < -qy ? -qy
															   : u;
						self->uy = by;
					}
					if (a >= t) {
						float d = POW(self->damping, 60 * t);
						float m = self->massInverse * t, e = self->strength, w = self->wind * f * self->skeleton->scaleX, g = -(self->gravity) * f * self->skeleton->scaleY;
						do {
							if (x) {
								self->xVelocity += (w - self->xOffset * e) * m;
								self->xOffset += self->xVelocity * t;
								self->xVelocity *= d;
							}
							if (y) {
								self->yVelocity -= (g + self->yOffset * e) * m;
								self->yOffset += self->yVelocity * t;
								self->yVelocity *= d;
							}
							a -= t;
						} while (a >= t);
					}
					if (x) bone->worldX += self->xOffset * mix * self->data->x;
					if (y) bone->worldY += self->yOffset * mix * self->data->y;
				}

				if (rotateOrShearX || scaleX) {
					float ca = ATAN2(bone->c, bone->a), c, s, mr = 0;
					float dx = self->cx - bone->worldX, dy = self->cy - bone->worldY;
					if (dx > qx)
						dx = qx;
					else if (dx < -qx)//
						dx = -qx;
					if (dy > qy)
						dy = qy;
					else if (dy < -qy)//
						dy = -qy;
					if (rotateOrShearX) {
						mr = (self->data->rotate + self->data->shearX) * mix;
						float r = ATAN2(dy + self->ty, dx + self->tx) - ca - self->rotateOffset * mr;
						self->rotateOffset += (r - CEIL(r * INV_PI2 - 0.5f) * PI2) * i;
						r = self->rotateOffset * mr + ca;
						c = COS(r);
						s = SIN(r);
						if (scaleX) {
							r = l * spBone_getWorldScaleX(bone);
							if (r > 0) self->scaleOffset += (dx * c + dy * s) * i / r;
						}
					} else {
						c = COS(ca);
						s = SIN(ca);
						float r = l * spBone_getWorldScaleX(bone);
						if (r > 0) self->scaleOffset += (dx * c + dy * s) * i / r;
					}
					a = self->remaining;
					if (a >= t) {
						float m = self->massInverse * t, e = self->strength, w = self->wind, g = self->gravity, h = l / f;
						float d = POW(self->damping, 60 * t);
						while (-1) {
							a -= t;
							if (scaleX) {
								self->scaleVelocity += (w * c - g * s - self->scaleOffset * e) * m;
								self->scaleOffset += self->scaleVelocity * t;
								self->scaleVelocity *= d;
							}
							if (rotateOrShearX) {
								self->rotateVelocity -= ((w * s + g * c) * h + self->rotateOffset * e) * m;
								self->rotateOffset += self->rotateVelocity * t;
								self->rotateVelocity *= d;
								if (a < t) break;
								float r = self->rotateOffset * mr + ca;
								c = COS(r);
								s = SIN(r);
							} else if (a < t)//
								break;
						}
					}
				}
				self->remaining = a;
			}

			self->cx = bone->worldX;
			self->cy = bone->worldY;
			break;
		}
		case SP_PHYSICS_POSE: {
			if (x) bone->worldX += self->xOffset * mix * self->data->x;
			if (y) bone->worldY += self->yOffset * mix * self->data->y;
			break;
		}
	}

	if (rotateOrShearX) {
		float o = self->rotateOffset * mix, s = 0, c = 0, a = 0;
		if (self->data->shearX > 0) {
			float r = 0;
			if (self->data->rotate > 0) {
				r = o * self->data->rotate;
				s = SIN(r);
				c = COS(r);
				a = bone->b;
				bone->b = c * a - s * bone->d;
				bone->d = s * a + c * bone->d;
			}
			r += o * self->data->shearX;
			s = SIN(r);
			c = COS(r);
			a = bone->a;
			bone->a = c * a - s * bone->c;
			bone->c = s * a + c * bone->c;
		} else {
			o *= self->data->rotate;
			s = SIN(o);
			c = COS(o);
			a = bone->a;
			bone->a = c * a - s * bone->c;
			bone->c = s * a + c * bone->c;
			a = bone->b;
			bone->b = c * a - s * bone->d;
			bone->d = s * a + c * bone->d;
		}
	}
	if (scaleX) {
		float s = 1 + self->scaleOffset * mix * self->data->scaleX;
		bone->a *= s;
		bone->c *= s;
	}
	if (physics != SP_PHYSICS_POSE) {
		self->tx = l * bone->a;
		self->ty = l * bone->c;
	}
	spBone_updateAppliedTransform(bone);
}

void spPhysicsConstraint_rotate(spPhysicsConstraint *self, float x, float y, float degrees) {
	float r = degrees * DEG_RAD, cosine = COS(r), sine = SIN(r);
	float dx = self->cx - x, dy = self->cy - y;
	spPhysicsConstraint_translate(self, dx * cosine - dy * sine - dx, dx * sine + dy * cosine - dy);
}

void spPhysicsConstraint_translate(spPhysicsConstraint *self, float x, float y) {
	self->ux -= x;
	self->uy -= y;
	self->cx -= x;
	self->cy -= y;
}
