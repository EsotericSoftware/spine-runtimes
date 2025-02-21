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
	float pa, pb, pc, pd;
	float sx = self->skeleton->scaleX;
	float sy = self->skeleton->scaleY * (spBone_isYDown() ? -1 : 1);
	spBone *parent = self->parent;

	self->ax = x;
	self->ay = y;
	self->arotation = rotation;
	self->ascaleX = scaleX;
	self->ascaleY = scaleY;
	self->ashearX = shearX;
	self->ashearY = shearY;

	if (!parent) { /* Root bone. */
		float rx = (rotation + shearX) * DEG_RAD;
		float ry = (rotation + 90 + shearY) * DEG_RAD;
		self->a = COS(rx) * scaleX * sx;
		self->b = COS(ry) * scaleY * sx;
		self->c = SIN(rx) * scaleX * sy;
		self->d = SIN(ry) * scaleY * sy;
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

	switch (self->inherit) {
		case SP_INHERIT_NORMAL: {
			float rx = (rotation + shearX) * DEG_RAD;
			float ry = (rotation + 90 + shearY) * DEG_RAD;
			float la = COS(rx) * scaleX;
			float lb = COS(ry) * scaleY;
			float lc = SIN(rx) * scaleX;
			float ld = SIN(ry) * scaleY;
			self->a = pa * la + pb * lc;
			self->b = pa * lb + pb * ld;
			self->c = pc * la + pd * lc;
			self->d = pc * lb + pd * ld;
			return;
		}
		case SP_INHERIT_ONLYTRANSLATION: {
			float rx = (rotation + shearX) * DEG_RAD;
			float ry = (rotation + 90 + shearY) * DEG_RAD;
			self->a = COS(rx) * scaleX;
			self->b = COS(ry) * scaleY;
			self->c = SIN(rx) * scaleX;
			self->d = SIN(ry) * scaleY;
			break;
		}
		case SP_INHERIT_NOROTATIONORREFLECTION: {
			float s = pa * pa + pc * pc;
			float prx;
			if (s > 0.0001f) {
				s = ABS(pa * pd - pb * pc) / s;
				pa /= sx;
				pc /= sy;
				pb = pc * s;
				pd = pa * s;
				prx = ATAN2DEG(pc, pa);
			} else {
				pa = 0;
				pc = 0;
				prx = 90 - ATAN2DEG(pd, pb);
			}
			float rx = (rotation + shearX - prx) * DEG_RAD;
			float ry = (rotation + shearY - prx + 90) * DEG_RAD;
			float la = COS(rx) * scaleX;
			float lb = COS(ry) * scaleY;
			float lc = SIN(rx) * scaleX;
			float ld = SIN(ry) * scaleY;
			self->a = pa * la - pb * lc;
			self->b = pa * lb - pb * ld;
			self->c = pc * la + pd * lc;
			self->d = pc * lb + pd * ld;
			break;
		}
		case SP_INHERIT_NOSCALE:
		case SP_INHERIT_NOSCALEORREFLECTION: {
			rotation *= DEG_RAD;
			float cosine = COS(rotation);
			float sine = SIN(rotation);
			float za = (pa * cosine + pb * sine) / sx;
			float zc = (pc * cosine + pd * sine) / sy;
			float s = SQRT(za * za + zc * zc);
			if (s > 0.00001f) s = 1 / s;
			za *= s;
			zc *= s;
			s = SQRT(za * za + zc * zc);
			if (self->inherit == SP_INHERIT_NOSCALE &&
				(pa * pd - pb * pc < 0) != ((sx < 0) != (sy < 0)))
				s = -s;
			rotation = PI / 2 + ATAN2(zc, za);
			float zb = COS(rotation) * s;
			float zd = SIN(rotation) * s;
			shearX *= DEG_RAD;
			shearY = (90 + shearY) * DEG_RAD;
			float la = COS(shearX) * scaleX;
			float lb = COS(shearY) * scaleY;
			float lc = SIN(shearX) * scaleX;
			float ld = SIN(shearY) * scaleY;
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
	self->inherit = self->data->inherit;
}

float spBone_getWorldRotationX(spBone *self) {
	return ATAN2DEG(self->c, self->a);
}

float spBone_getWorldRotationY(spBone *self) {
	return ATAN2DEG(self->d, self->b);
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
	float s, sa, sc;
	float cosine, sine;

	float yDownScale = spBone_isYDown() ? -1 : 1;

	spBone *parent = self->parent;
	if (!parent) {
		self->ax = self->worldX - self->skeleton->x;
		self->ay = self->worldY - self->skeleton->y;
		self->arotation = ATAN2DEG(self->c, self->a);
		self->ascaleX = SQRT(self->a * self->a + self->c * self->c);
		self->ascaleY = SQRT(self->b * self->b + self->d * self->d);
		self->ashearX = 0;
		self->ashearY = ATAN2DEG(self->a * self->b + self->c * self->d, self->a * self->d - self->b * self->c);
		return;
	}

	pa = parent->a, pb = parent->b, pc = parent->c, pd = parent->d;
	pid = 1 / (pa * pd - pb * pc);
	ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
	dx = self->worldX - parent->worldX, dy = self->worldY - parent->worldY;
	self->ax = (dx * ia - dy * ib);
	self->ay = (dy * id - dx * ic);

	if (self->inherit == SP_INHERIT_ONLYTRANSLATION) {
		ra = self->a;
		rb = self->b;
		rc = self->c;
		rd = self->d;
	} else {
		switch (self->inherit) {
			case SP_INHERIT_NOROTATIONORREFLECTION: {
				s = ABS(pa * pd - pb * pc) / (pa * pa + pc * pc);
				sa = pa / self->skeleton->scaleX;
				sc = pc / self->skeleton->scaleY * yDownScale;
				pb = -sc * s * self->skeleton->scaleX;
				pd = sa * s * self->skeleton->scaleY * yDownScale;
				pid = 1 / (pa * pd - pb * pc);
				ia = pd * pid;
				ib = pb * pid;
				break;
			}
			case SP_INHERIT_NOSCALE:
			case SP_INHERIT_NOSCALEORREFLECTION: {
				float r = self->rotation * DEG_RAD;
				cosine = COS(r), sine = SIN(r);
				pa = (pa * cosine + pb * sine) / self->skeleton->scaleX;
				pc = (pc * cosine + pd * sine) / self->skeleton->scaleY * yDownScale;
				s = SQRT(pa * pa + pc * pc);
				if (s > 0.00001) s = 1 / s;
				pa *= s;
				pc *= s;
				s = SQRT(pa * pa + pc * pc);
				if (self->inherit == SP_INHERIT_NOSCALE &&
					(pid < 0) != ((self->skeleton->scaleX < 0) != ((self->skeleton->scaleY * yDownScale) < 0)))
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
		self->ashearY = -ATAN2DEG(ra * rb + rc * rd, det);
		self->arotation = ATAN2DEG(rc, ra);
	} else {
		self->ascaleX = 0;
		self->ascaleY = SQRT(rb * rb + rd * rd);
		self->ashearY = 0;
		self->arotation = 90 - ATAN2DEG(rd, rb);
	}
}

void spBone_worldToLocal(spBone *self, float worldX, float worldY, float *localX, float *localY) {
	float invDet = 1 / (self->a * self->d - self->b * self->c);
	float x = worldX - self->worldX, y = worldY - self->worldY;
	*localX = (x * self->d * invDet - y * self->b * invDet);
	*localY = (y * self->a * invDet - x * self->c * invDet);
}

void spBone_worldToParent(spBone *self, float worldX, float worldY, float *localX, float *localY) {
	if (self->parent == NULL) {
		*localX = worldX;
		*localY = worldY;
	} else {
		spBone_worldToLocal(self->parent, worldX, worldY, localX, localY);
	}
}

void spBone_localToWorld(spBone *self, float localX, float localY, float *worldX, float *worldY) {
	float x = localX, y = localY;
	*worldX = x * self->a + y * self->b + self->worldX;
	*worldY = x * self->c + y * self->d + self->worldY;
}

void spBone_parentToWorld(spBone *self, float localX, float localY, float *worldX, float *worldY) {
	if (self->parent == NULL) {
		*worldX = localX;
		*worldY = localY;
	} else {
		spBone_localToWorld(self->parent, localX, localY, worldX, worldY);
	}
}

float spBone_worldToLocalRotation(spBone *self, float worldRotation) {
	worldRotation *= DEG_RAD;
	float sine = SIN(worldRotation), cosine = COS(worldRotation);
	return ATAN2DEG(self->a * sine - self->c * cosine, self->d * cosine - self->b * sine) + self->rotation - self->shearX;
}

float spBone_localToWorldRotation(spBone *self, float localRotation) {
	localRotation = (localRotation - self->rotation - self->shearX) * DEG_RAD;
	float sine = SIN(localRotation), cosine = COS(localRotation);
	return ATAN2DEG(cosine * self->c + sine * self->d, cosine * self->a + sine * self->b);
}

void spBone_rotateWorld(spBone *self, float degrees) {
	degrees *= DEG_RAD;
	float sine = SIN(degrees), cosine = COS(degrees);
	float ra = self->a, rb = self->b;
	self->a = cosine * ra - sine * self->c;
	self->b = cosine * rb - sine * self->d;
	self->c = sine * ra + cosine * self->c;
	self->d = sine * rb + cosine * self->d;
}
