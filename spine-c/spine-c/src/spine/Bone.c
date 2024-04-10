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

#include <spine/Bone.h>
#include <spine/extension.h>
#include <stdio.h>

static int yDown;

void spBone_setYDown(int value) {
	yDown = value;
}

int spBone_isYDown(void) {
	return yDown;
}

spBone *spBone_create(spBoneData *data, spSkeleton *skeleton, spBone *parent) {
	spBone *self = NEW(spBone);
	self->data = data;
	self->skeleton = skeleton;
	self->parent = parent;
	self->a = 1.0f;
	self->d = 1.0f;
    self->active = -1;
    self->inherit = SP_INHERIT_NORMAL;
	spBone_setToSetupPose(self);
	return self;
}

void spBone_dispose(spBone *self) {
	FREE(self->children);
	FREE(self);
}

void spBone_update(spBone *self) {
	spBone_updateWorldTransformWith(self, self->ax, self->ay, self->arotation, self->ascaleX, self->ascaleY, self->ashearX, self->ashearY);
}

void spBone_updateWorldTransform(spBone *self) {
	spBone_updateWorldTransformWith(self, self->x, self->y, self->rotation, self->scaleX, self->scaleY, self->shearX,
									self->shearY);
}

void spBone_updateWorldTransformWith(spBone *self, float x, float y, float rotation, float scaleX, float scaleY,
									 float shearX, float shearY) {
    float cosine, sine;
    float pa, pb, pc, pd;
	spBone *parent = self->parent;
	float sx = self->skeleton->scaleX;
	float sy = self->skeleton->scaleY * (spBone_isYDown() ? -1 : 1);

	self->ax = x;
	self->ay = y;
	self->arotation = rotation;
	self->ascaleX = scaleX;
	self->ascaleY = scaleY;
	self->ashearX = shearX;
	self->ashearY = shearY;

	if (!parent) { /* Root bone. */
		float rotationY = rotation + 90 + shearY;
		self->a = COS_DEG(rotation + shearX) * scaleX * sx;
		self->b = COS_DEG(rotationY) * scaleY * sx;
		self->c = SIN_DEG(rotation + shearX) * scaleX * sy;
		self->d = SIN_DEG(rotationY) * scaleY * sy;
		self->worldX = x * sx + self->skeleton->x;
		self->worldY = y * sy + self->skeleton->y;
		return;
	}

	pa = parent->a;
	pb = parent->b;
	pc = parent->c;
	pd = parent->d;

	self->worldX = pa * x + pb * y + parent->worldX;
	self->worldY = pc * x + pd * y + parent->worldY;

	switch (self->data->inherit) {
		case SP_INHERIT_NORMAL: {
			float rotationY = rotation + 90 + shearY;
			float la = COS_DEG(rotation + shearX) * scaleX;
			float lb = COS_DEG(rotationY) * scaleY;
			float lc = SIN_DEG(rotation + shearX) * scaleX;
			float ld = SIN_DEG(rotationY) * scaleY;
			self->a = pa * la + pb * lc;
			self->b = pa * lb + pb * ld;
			self->c = pc * la + pd * lc;
			self->d = pc * lb + pd * ld;
			return;
		}
		case SP_INHERIT_ONLYTRANSLATION: {
			float rotationY = rotation + 90 + shearY;
			self->a = COS_DEG(rotation + shearX) * scaleX;
			self->b = COS_DEG(rotationY) * scaleY;
			self->c = SIN_DEG(rotation + shearX) * scaleX;
			self->d = SIN_DEG(rotationY) * scaleY;
			break;
		}
		case SP_INHERIT_NOROTATIONORREFLECTION: {
			float s = pa * pa + pc * pc;
			float prx, rx, ry, la, lb, lc, ld;
			if (s > 0.0001f) {
				s = ABS(pa * pd - pb * pc) / s;
				pa /= self->skeleton->scaleX;
				pc /= self->skeleton->scaleY;
				pb = pc * s;
				pd = pa * s;
				prx = ATAN2(pc, pa) * RAD_DEG;
			} else {
				pa = 0;
				pc = 0;
				prx = 90 - ATAN2(pd, pb) * RAD_DEG;
			}
			rx = rotation + shearX - prx;
			ry = rotation + shearY - prx + 90;
			la = COS_DEG(rx) * scaleX;
			lb = COS_DEG(ry) * scaleY;
			lc = SIN_DEG(rx) * scaleX;
			ld = SIN_DEG(ry) * scaleY;
			self->a = pa * la - pb * lc;
			self->b = pa * lb - pb * ld;
			self->c = pc * la + pd * lc;
			self->d = pc * lb + pd * ld;
			break;
		}
		case SP_INHERIT_NOSCALE:
		case SP_INHERIT_NOSCALEORREFLECTION: {
			float za, zc, s;
			float r, zb, zd, la, lb, lc, ld;
			cosine = COS_DEG(rotation);
			sine = SIN_DEG(rotation);
			za = (pa * cosine + pb * sine) / sx;
			zc = (pc * cosine + pd * sine) / sy;
			s = SQRT(za * za + zc * zc);
			if (s > 0.00001f) s = 1 / s;
			za *= s;
			zc *= s;
			s = SQRT(za * za + zc * zc);
			if (self->data->inherit == SP_INHERIT_NOSCALE && (pa * pd - pb * pc < 0) != (sx < 0 != sy < 0))
				s = -s;
			r = PI / 2 + ATAN2(zc, za);
			zb = COS(r) * s;
			zd = SIN(r) * s;
			la = COS_DEG(shearX) * scaleX;
			lb = COS_DEG(90 + shearY) * scaleY;
			lc = SIN_DEG(shearX) * scaleX;
			ld = SIN_DEG(90 + shearY) * scaleY;
			self->a = za * la + zb * lc;
			self->b = za * lb + zb * ld;
			self->c = zc * la + zd * lc;
			self->d = zc * lb + zd * ld;
		}
	}

	self->a *= sx;
	self->b *= sx;
	self->c *= sy;
	self->d *= sy;
}

void spBone_setToSetupPose(spBone *self) {
	self->x = self->data->x;
	self->y = self->data->y;
	self->rotation = self->data->rotation;
	self->scaleX = self->data->scaleX;
	self->scaleY = self->data->scaleY;
	self->shearX = self->data->shearX;
	self->shearY = self->data->shearY;
}

float spBone_getWorldRotationX(spBone *self) {
	return ATAN2(self->c, self->a) * RAD_DEG;
}

float spBone_getWorldRotationY(spBone *self) {
	return ATAN2(self->d, self->b) * RAD_DEG;
}

float spBone_getWorldScaleX(spBone *self) {
	return SQRT(self->a * self->a + self->c * self->c);
}

float spBone_getWorldScaleY(spBone *self) {
	return SQRT(self->b * self->b + self->d * self->d);
}

/** Computes the individual applied transform values from the world transform. This can be useful to perform processing using
 * the applied transform after the world transform has been modified directly (eg, by a constraint).
 * <p>
 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. */
void spBone_updateAppliedTransform(spBone *self) {
	float pa, pb, pc, pd;
	float pid;
	float ia, ib, ic, id;
	float dx, dy;
	float ra, rb, rc, rd;
	float s, r, sa, sc;
	float cosine, sine;

	spBone *parent = self->parent;
	if (!parent) {
		self->ax = self->worldX - self->skeleton->x;
		self->ay = self->worldY - self->skeleton->y;
		self->arotation = ATAN2(self->c, self->a) * RAD_DEG;
		self->ascaleX = SQRT(self->a * self->a + self->c * self->c);
		self->ascaleY = SQRT(self->b * self->b + self->d * self->d);
		self->ashearX = 0;
		self->ashearY = ATAN2(self->a * self->b + self->c * self->d, self->a * self->d - self->b * self->c) * RAD_DEG;
		return;
	}

	pa = parent->a, pb = parent->b, pc = parent->c, pd = parent->d;
	pid = 1 / (pa * pd - pb * pc);
	ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
	dx = self->worldX - parent->worldX, dy = self->worldY - parent->worldY;
	self->ax = (dx * ia - dy * ib);
	self->ay = (dy * id - dx * ic);

	if (self->data->inherit == SP_INHERIT_ONLYTRANSLATION) {
		ra = self->a;
		rb = self->b;
		rc = self->c;
		rd = self->d;
	} else {
		switch (self->data->inherit) {
			case SP_INHERIT_NOROTATIONORREFLECTION: {
				s = ABS(pa * pd - pb * pc) / (pa * pa + pc * pc);
				sa = pa / self->skeleton->scaleX;
				sc = pc / self->skeleton->scaleY;
				pb = -sc * s * self->skeleton->scaleX;
				pd = sa * s * self->skeleton->scaleY;
				pid = 1 / (pa * pd - pb * pc);
				ia = pd * pid;
				ib = pb * pid;
				break;
			}
			case SP_INHERIT_NOSCALE:
			case SP_INHERIT_NOSCALEORREFLECTION: {
				cosine = COS_DEG(self->rotation), sine = SIN_DEG(self->rotation);
				pa = (pa * cosine + pb * sine) / self->skeleton->scaleX;
				pc = (pc * cosine + pd * sine) / self->skeleton->scaleY;
				s = SQRT(pa * pa + pc * pc);
				if (s > 0.00001f) s = 1 / s;
				pa *= s;
				pc *= s;
				s = SQRT(pa * pa + pc * pc);
				if (self->data->inherit == SP_INHERIT_NOSCALE &&
					pid < 0 != (self->skeleton->scaleX < 0 != self->skeleton->scaleY < 0))
					s = -s;
				r = PI / 2 + ATAN2(pc, pa);
				pb = COS(r) * s;
				pd = SIN(r) * s;
				pid = 1 / (pa * pd - pb * pc);
				ia = pd * pid;
				ib = pb * pid;
				ic = pc * pid;
				id = pa * pid;
				break;
			}
			case SP_INHERIT_ONLYTRANSLATION:
			case SP_INHERIT_NORMAL:
				break;
		}
		ra = ia * self->a - ib * self->c;
		rb = ia * self->b - ib * self->d;
		rc = id * self->c - ic * self->a;
		rd = id * self->d - ic * self->b;
	}

	self->ashearX = 0;
	self->ascaleX = SQRT(ra * ra + rc * rc);
	if (self->ascaleX > 0.0001f) {
		float det = ra * rd - rb * rc;
		self->ascaleY = det / self->ascaleX;
		self->ashearY = -ATAN2(ra * rb + rc * rd, det) * RAD_DEG;
		self->arotation = ATAN2(rc, ra) * RAD_DEG;
	} else {
		self->ascaleX = 0;
		self->ascaleY = SQRT(rb * rb + rd * rd);
		self->ashearY = 0;
		self->arotation = 90 - ATAN2(rd, rb) * RAD_DEG;
	}
}

void spBone_worldToLocal(spBone *self, float worldX, float worldY, float *localX, float *localY) {
	float invDet = 1 / (self->a * self->d - self->b * self->c);
	float x = worldX - self->worldX, y = worldY - self->worldY;
	*localX = (x * self->d * invDet - y * self->b * invDet);
	*localY = (y * self->a * invDet - x * self->c * invDet);
}

void spBone_localToWorld(spBone *self, float localX, float localY, float *worldX, float *worldY) {
	float x = localX, y = localY;
	*worldX = x * self->a + y * self->b + self->worldX;
	*worldY = x * self->c + y * self->d + self->worldY;
}

float spBone_worldToLocalRotation(spBone *self, float worldRotation) {
	float sine, cosine;
	sine = SIN_DEG(worldRotation);
	cosine = COS_DEG(worldRotation);
	return ATAN2(self->a * sine - self->c * cosine, self->d * cosine - self->b * sine) * RAD_DEG + self->rotation -
		   self->shearX;
}

float spBone_localToWorldRotation(spBone *self, float localRotation) {
	float sine, cosine;
	localRotation -= self->rotation - self->shearX;
	sine = SIN_DEG(localRotation);
	cosine = COS_DEG(localRotation);
	return ATAN2(cosine * self->c + sine * self->d, cosine * self->a + sine * self->b) * RAD_DEG;
}

void spBone_rotateWorld(spBone *self, float degrees) {
	float a = self->a, b = self->b, c = self->c, d = self->d;
	float cosine = COS_DEG(degrees), sine = SIN_DEG(degrees);
	self->a = cosine * a - sine * c;
	self->b = cosine * b - sine * d;
	self->c = sine * a + cosine * c;
	self->d = sine * b + cosine * d;
}
