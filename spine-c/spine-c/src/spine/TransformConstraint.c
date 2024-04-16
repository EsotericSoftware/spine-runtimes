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
#include <spine/TransformConstraint.h>
#include <spine/extension.h>

spTransformConstraint *spTransformConstraint_create(spTransformConstraintData *data, const spSkeleton *skeleton) {
	int i;
	spTransformConstraint *self = NEW(spTransformConstraint);
	self->data = data;
	self->mixRotate = data->mixRotate;
	self->mixX = data->mixX;
	self->mixY = data->mixY;
	self->mixScaleX = data->mixScaleX;
	self->mixScaleY = data->mixScaleY;
	self->mixShearY = data->mixShearY;
	self->bonesCount = data->bonesCount;
	self->bones = MALLOC(spBone *, self->bonesCount);
	for (i = 0; i < self->bonesCount; ++i)
		self->bones[i] = spSkeleton_findBone(skeleton, self->data->bones[i]->name);
	self->target = spSkeleton_findBone(skeleton, self->data->target->name);
	return self;
}

void spTransformConstraint_dispose(spTransformConstraint *self) {
	FREE(self->bones);
	FREE(self);
}

void _spTransformConstraint_applyAbsoluteWorld(spTransformConstraint *self) {
	float mixRotate = self->mixRotate, mixX = self->mixX, mixY = self->mixY, mixScaleX = self->mixScaleX,
		  mixScaleY = self->mixScaleY, mixShearY = self->mixShearY;
	int /*bool*/ translate = mixX != 0 || mixY != 0;
	spBone *target = self->target;
	float ta = target->a, tb = target->b, tc = target->c, td = target->d;
	float degRadReflect = ta * td - tb * tc > 0 ? DEG_RAD : -DEG_RAD;
	float offsetRotation = self->data->offsetRotation * degRadReflect, offsetShearY =
																			   self->data->offsetShearY * degRadReflect;
	int i;
	float a, b, c, d, r, cosine, sine, x, y, s, by;
	for (i = 0; i < self->bonesCount; ++i) {
		spBone *bone = self->bones[i];

		if (mixRotate != 0) {
			a = bone->a, b = bone->b, c = bone->c, d = bone->d;
			r = ATAN2(tc, ta) - ATAN2(c, a) + offsetRotation;
			if (r > PI) r -= PI2;
			else if (r < -PI)
				r += PI2;
			r *= mixRotate;
			cosine = COS(r);
			sine = SIN(r);
			bone->a = cosine * a - sine * c;
			bone->b = cosine * b - sine * d;
			bone->c = sine * a + cosine * c;
			bone->d = sine * b + cosine * d;
		}

		if (translate) {
			spBone_localToWorld(target, self->data->offsetX, self->data->offsetY, &x, &y);
			bone->worldX += (x - bone->worldX) * mixX;
			bone->worldY += (y - bone->worldY) * mixY;
		}

		if (mixScaleX > 0) {
			s = SQRT(bone->a * bone->a + bone->c * bone->c);
			if (s != 0) s = (s + (SQRT(ta * ta + tc * tc) - s + self->data->offsetScaleX) * mixScaleX) / s;
			bone->a *= s;
			bone->c *= s;
		}
		if (mixScaleY != 0) {
			s = SQRT(bone->b * bone->b + bone->d * bone->d);
			if (s != 0) s = (s + (SQRT(tb * tb + td * td) - s + self->data->offsetScaleY) * mixScaleY) / s;
			bone->b *= s;
			bone->d *= s;
		}

		if (mixShearY > 0) {
			b = bone->b, d = bone->d;
			by = ATAN2(d, b);
			r = ATAN2(td, tb) - ATAN2(tc, ta) - (by - ATAN2(bone->c, bone->a));
			s = SQRT(b * b + d * d);
			if (r > PI) r -= PI2;
			else if (r < -PI)
				r += PI2;
			r = by + (r + offsetShearY) * mixShearY;
			bone->b = COS(r) * s;
			bone->d = SIN(r) * s;
		}
		spBone_updateAppliedTransform(bone);
	}
}

void _spTransformConstraint_applyRelativeWorld(spTransformConstraint *self) {
	float mixRotate = self->mixRotate, mixX = self->mixX, mixY = self->mixY, mixScaleX = self->mixScaleX,
		  mixScaleY = self->mixScaleY, mixShearY = self->mixShearY;
	int /*bool*/ translate = mixX != 0 || mixY != 0;
	spBone *target = self->target;
	float ta = target->a, tb = target->b, tc = target->c, td = target->d;
	float degRadReflect = ta * td - tb * tc > 0 ? DEG_RAD : -DEG_RAD;
	float offsetRotation = self->data->offsetRotation * degRadReflect, offsetShearY =
																			   self->data->offsetShearY * degRadReflect;
	int i;
	float a, b, c, d, r, cosine, sine, x, y, s;
	for (i = 0; i < self->bonesCount; ++i) {
		spBone *bone = self->bones[i];

		if (mixRotate != 0) {
			a = bone->a, b = bone->b, c = bone->c, d = bone->d;
			r = ATAN2(tc, ta) + offsetRotation;
			if (r > PI) r -= PI2;
			else if (r < -PI)
				r += PI2;
			r *= mixRotate;
			cosine = COS(r);
			sine = SIN(r);
			bone->a = cosine * a - sine * c;
			bone->b = cosine * b - sine * d;
			bone->c = sine * a + cosine * c;
			bone->d = sine * b + cosine * d;
		}

		if (translate != 0) {
			spBone_localToWorld(target, self->data->offsetX, self->data->offsetY, &x, &y);
			bone->worldX += (x * mixX);
			bone->worldY += (y * mixY);
		}

		if (mixScaleX != 0) {
			s = (SQRT(ta * ta + tc * tc) - 1 + self->data->offsetScaleX) * mixScaleX + 1;
			bone->a *= s;
			bone->c *= s;
		}
		if (mixScaleY > 0) {
			s = (SQRT(tb * tb + td * td) - 1 + self->data->offsetScaleY) * mixScaleY + 1;
			bone->b *= s;
			bone->d *= s;
		}

		if (mixShearY > 0) {
			r = ATAN2(td, tb) - ATAN2(tc, ta);
			if (r > PI) r -= PI2;
			else if (r < -PI)
				r += PI2;
			b = bone->b, d = bone->d;
			r = ATAN2(d, b) + (r - PI / 2 + offsetShearY) * mixShearY;
			s = SQRT(b * b + d * d);
			bone->b = COS(r) * s;
			bone->d = SIN(r) * s;
		}

		spBone_updateAppliedTransform(bone);
	}
}

void _spTransformConstraint_applyAbsoluteLocal(spTransformConstraint *self) {
	float mixRotate = self->mixRotate, mixX = self->mixX, mixY = self->mixY, mixScaleX = self->mixScaleX,
		  mixScaleY = self->mixScaleY, mixShearY = self->mixShearY;
	spBone *target = self->target;
	int i;
	float rotation, r, x, y, scaleX, scaleY, shearY;

	for (i = 0; i < self->bonesCount; ++i) {
		spBone *bone = self->bones[i];

		rotation = bone->arotation;
		if (mixRotate != 0) {
			r = target->arotation - rotation + self->data->offsetRotation;
			r -= CEIL(r / 360 - 0.5) * 360;
			rotation += r * mixRotate;
		}

		x = bone->ax, y = bone->ay;
		x += (target->ax - x + self->data->offsetX) * mixX;
		y += (target->ay - y + self->data->offsetY) * mixY;

		scaleX = bone->ascaleX, scaleY = bone->ascaleY;
		if (mixScaleX != 0 && scaleX != 0)
			scaleX = (scaleX + (target->ascaleX - scaleX + self->data->offsetScaleX) * mixScaleX) / scaleX;
		if (mixScaleY != 0 && scaleY != 0)
			scaleY = (scaleY + (target->ascaleY - scaleY + self->data->offsetScaleY) * mixScaleY) / scaleY;

		shearY = bone->ashearY;
		if (mixShearY != 0) {
			r = target->ashearY - shearY + self->data->offsetShearY;
			r -= CEIL(r / 360 - 0.5) * 360;
			shearY += r * mixShearY;
		}

		spBone_updateWorldTransformWith(bone, x, y, rotation, scaleX, scaleY, bone->ashearX, shearY);
	}
}

void _spTransformConstraint_applyRelativeLocal(spTransformConstraint *self) {
	float mixRotate = self->mixRotate, mixX = self->mixX, mixY = self->mixY, mixScaleX = self->mixScaleX,
		  mixScaleY = self->mixScaleY, mixShearY = self->mixShearY;
	spBone *target = self->target;
	int i;
	float rotation, x, y, scaleX, scaleY, shearY;

	for (i = 0; i < self->bonesCount; ++i) {
		spBone *bone = self->bones[i];

		rotation = bone->arotation + (target->arotation + self->data->offsetRotation) * mixRotate;
		x = bone->ax + (target->ax + self->data->offsetX) * mixX;
		y = bone->ay + (target->ay + self->data->offsetY) * mixY;
		scaleX = bone->ascaleX * (((target->ascaleX - 1 + self->data->offsetScaleX) * mixScaleX) + 1);
		scaleY = bone->ascaleY * (((target->ascaleY - 1 + self->data->offsetScaleY) * mixScaleY) + 1);
		shearY = bone->ashearY + (target->ashearY + self->data->offsetShearY) * mixShearY;

		spBone_updateWorldTransformWith(bone, x, y, rotation, scaleX, scaleY, bone->ashearX, shearY);
	}
}

void spTransformConstraint_update(spTransformConstraint *self) {
	if (self->mixRotate == 0 && self->mixX == 0 && self->mixY == 0 && self->mixScaleX == 0 && self->mixScaleY == 0 &&
		self->mixShearY == 0)
		return;

	if (self->data->local) {
		if (self->data->relative)
			_spTransformConstraint_applyRelativeLocal(self);
		else
			_spTransformConstraint_applyAbsoluteLocal(self);

	} else {
		if (self->data->relative)
			_spTransformConstraint_applyRelativeWorld(self);
		else
			_spTransformConstraint_applyAbsoluteWorld(self);
	}
}

void spTransformConstraint_setToSetupPose(spTransformConstraint *self) {
	spTransformConstraintData *data = self->data;
	self->mixRotate = data->mixRotate;
	self->mixX = data->mixX;
	self->mixY = data->mixY;
	self->mixScaleX = data->mixScaleX;
	self->mixScaleY = data->mixScaleY;
	self->mixShearY = data->mixShearY;
}
