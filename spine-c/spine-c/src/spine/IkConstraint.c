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

#include <float.h>
#include <spine/IkConstraint.h>
#include <spine/Skeleton.h>
#include <spine/extension.h>

spIkConstraint *spIkConstraint_create(spIkConstraintData *data, const spSkeleton *skeleton) {
	int i;

	spIkConstraint *self = NEW(spIkConstraint);
	self->data = data;
	self->bendDirection = data->bendDirection;
	self->compress = data->compress;
	self->stretch = data->stretch;
	self->mix = data->mix;
	self->softness = data->softness;

	self->bonesCount = self->data->bonesCount;
	self->bones = MALLOC(spBone *, self->bonesCount);
	for (i = 0; i < self->bonesCount; ++i)
		self->bones[i] = spSkeleton_findBone(skeleton, self->data->bones[i]->name);
	self->target = spSkeleton_findBone(skeleton, self->data->target->name);

	return self;
}

void spIkConstraint_dispose(spIkConstraint *self) {
	FREE(self->bones);
	FREE(self);
}

void spIkConstraint_update(spIkConstraint *self) {
	if (self->mix == 0) return;
	switch (self->bonesCount) {
		case 1:
			spIkConstraint_apply1(self->bones[0], self->target->worldX, self->target->worldY, self->compress,
								  self->stretch, self->data->uniform, self->mix);
			break;
		case 2:
			spIkConstraint_apply2(self->bones[0], self->bones[1], self->target->worldX, self->target->worldY,
								  self->bendDirection, self->stretch, self->data->uniform, self->softness, self->mix);
			break;
	}
}

void spIkConstraint_setToSetupPose(spIkConstraint *self) {
	self->bendDirection = self->data->bendDirection;
	self->compress = self->data->compress;
	self->stretch = self->data->stretch;
	self->softness = self->data->softness;
	self->mix = self->data->mix;
}

void spIkConstraint_apply1(spBone *bone, float targetX, float targetY, int /*boolean*/ compress, int /*boolean*/ stretch,
						   int /*boolean*/ uniform, float alpha) {
	spBone *p = bone->parent;
	float pa = p->a, pb = p->b, pc = p->c, pd = p->d;
	float rotationIK = -bone->ashearX - bone->arotation;
	float tx = 0, ty = 0, sx = 0, sy = 0, s = 0, sa = 0, sc = 0;

	switch (bone->data->inherit) {
		case SP_INHERIT_ONLYTRANSLATION:
			tx = (targetX - bone->worldX) * SIGNUM(bone->skeleton->scaleX);
			ty = (targetY - bone->worldY) * SIGNUM(bone->skeleton->scaleY);
			break;
		case SP_INHERIT_NOROTATIONORREFLECTION: {
			s = ABS(pa * pd - pb * pc) / MAX(0.0001f, pa * pa + pc * pc);
			sa = pa / bone->skeleton->scaleX;
			sc = pc / bone->skeleton->scaleY;
			pb = -sc * s * bone->skeleton->scaleX;
			pd = sa * s * bone->skeleton->scaleY;
			rotationIK += ATAN2(sc, sa) * RAD_DEG;
		}
		default: {
			float x = targetX - p->worldX, y = targetY - p->worldY;
			float d = pa * pd - pb * pc;
			if (ABS(d) <= 0.0001f) {
				tx = 0;
				ty = 0;
			} else {
				tx = (x * pd - y * pb) / d - bone->ax;
				ty = (y * pa - x * pc) / d - bone->ay;
			}
		}
	}
	rotationIK += ATAN2(ty, tx) * RAD_DEG;

	if (bone->ascaleX < 0) rotationIK += 180;
	if (rotationIK > 180) rotationIK -= 360;
	else if (rotationIK < -180)
		rotationIK += 360;
	sx = bone->ascaleX;
	sy = bone->ascaleY;
	if (compress || stretch) {
		float b, dd;
		switch (bone->data->inherit) {
			case SP_INHERIT_NOSCALE:
			case SP_INHERIT_NOSCALEORREFLECTION:
				tx = targetX - bone->worldX;
				ty = targetY - bone->worldY;
			default:;
		}
		b = bone->data->length * sx, dd = SQRT(tx * tx + ty * ty);
		if ((compress && dd < b) || ((stretch && dd > b) && (b > 0.0001f))) {
			s = (dd / b - 1) * alpha + 1;
			sx *= s;
			if (uniform) sy *= s;
		}
	}
	spBone_updateWorldTransformWith(bone, bone->ax, bone->ay, bone->arotation + rotationIK * alpha, sx,
									sy, bone->ashearX, bone->ashearY);
}

void spIkConstraint_apply2(spBone *parent, spBone *child, float targetX, float targetY, int bendDir, int /*boolean*/ stretch,
						   int /*boolean*/ uniform, float softness, float alpha) {
	float a, b, c, d;
	float px, py, psx, psy, sx, sy;
	float cx, cy, csx, cwx, cwy;
	int o1, o2, s2, u;
	spBone *pp = parent->parent;
	float tx, ty, dd, dx, dy, l1, l2, a1, a2, r, td, sd, p;
	float id, x, y;
	float aa, bb, ll, ta, c0, c1, c2;

	px = parent->ax;
	py = parent->ay;
	psx = parent->ascaleX;
	psy = parent->ascaleY;
	sx = psx;
	sy = psy;
	csx = child->ascaleX;
	if (psx < 0) {
		psx = -psx;
		o1 = 180;
		s2 = -1;
	} else {
		o1 = 0;
		s2 = 1;
	}
	if (psy < 0) {
		psy = -psy;
		s2 = -s2;
	}
	if (csx < 0) {
		csx = -csx;
		o2 = 180;
	} else
		o2 = 0;
	r = psx - psy;
	cx = child->ax;
	u = (r < 0 ? -r : r) <= 0.0001f;
	if (!u || stretch) {
		cy = 0;
		cwx = parent->a * cx + parent->worldX;
		cwy = parent->c * cx + parent->worldY;
	} else {
		cy = child->ay;
		cwx = parent->a * cx + parent->b * cy + parent->worldX;
		cwy = parent->c * cx + parent->d * cy + parent->worldY;
	}
	a = pp->a;
	b = pp->b;
	c = pp->c;
	d = pp->d;
	id = a * d - b * c;
	id = ABS(id) <= 0.0001f ? 0 : 1 / id;
	x = cwx - pp->worldX;
	y = cwy - pp->worldY;
	dx = (x * d - y * b) * id - px;
	dy = (y * a - x * c) * id - py;
	l1 = SQRT(dx * dx + dy * dy);
	l2 = child->data->length * csx;
	if (l1 < 0.0001) {
		spIkConstraint_apply1(parent, targetX, targetY, 0, stretch, 0, alpha);
		spBone_updateWorldTransformWith(child, cx, cy, 0, child->ascaleX, child->ascaleY, child->ashearX,
										child->ashearY);
		return;
	}
	x = targetX - pp->worldX;
	y = targetY - pp->worldY;
	tx = (x * d - y * b) * id - px;
	ty = (y * a - x * c) * id - py;
	dd = tx * tx + ty * ty;
	if (softness != 0) {
		softness *= psx * (csx + 1) * 0.5f;
		td = SQRT(dd);
		sd = td - l1 - l2 * psx + softness;
		if (sd > 0) {
			p = MIN(1, sd / (softness * 2)) - 1;
			p = (sd - softness * (1 - p * p)) / td;
			tx -= p * tx;
			ty -= p * ty;
			dd = tx * tx + ty * ty;
		}
	}
	if (u) {
		float cosine;
		l2 *= psx;
		cosine = (dd - l1 * l1 - l2 * l2) / (2 * l1 * l2);
		if (cosine < -1) {
			cosine = -1;
			a2 = PI * bendDir;
		} else if (cosine > 1) {
			cosine = 1;
			a2 = 0;
			if (stretch) {
				a = (SQRT(dd) / (l1 + l2) - 1) * alpha + 1;
				sx *= a;
				if (uniform) sy *= a;
			}
		} else
			a2 = ACOS(cosine) * bendDir;
		a = l1 + l2 * cosine;
		b = l2 * SIN(a2);
		a1 = ATAN2(ty * a - tx * b, tx * a + ty * b);
	} else {
		a = psx * l2;
		b = psy * l2;
		aa = a * a, bb = b * b, ll = l1 * l1, ta = ATAN2(ty, tx);
		c0 = bb * ll + aa * dd - aa * bb, c1 = -2 * bb * l1, c2 = bb - aa;
		d = c1 * c1 - 4 * c2 * c0;
		if (d >= 0) {
			float q = SQRT(d), r0, r1;
			if (c1 < 0) q = -q;
			q = -(c1 + q) * 0.5f;
			r0 = q / c2;
			r1 = c0 / q;
			r = ABS(r0) < ABS(r1) ? r0 : r1;
			y = dd - r * r;
			if (y > 0) {
				y = SQRT(y) * bendDir;
				a1 = ta - ATAN2(y, r);
				a2 = ATAN2(y / psy, (r - l1) / psx);
				goto break_outer;
			}
		}
		{
			float minAngle = PI, minX = l1 - a, minDist = minX * minX, minY = 0;
			float maxAngle = 0, maxX = l1 + a, maxDist = maxX * maxX, maxY = 0;
			c0 = -a * l1 / (aa - bb);
			if (c0 >= -1 && c0 <= 1) {
				c0 = ACOS(c0);
				x = a * COS(c0) + l1;
				y = b * SIN(c0);
				d = x * x + y * y;
				if (d < minDist) {
					minAngle = c0;
					minDist = d;
					minX = x;
					minY = y;
				}
				if (d > maxDist) {
					maxAngle = c0;
					maxDist = d;
					maxX = x;
					maxY = y;
				}
			}
			if (dd <= (minDist + maxDist) * 0.5f) {
				a1 = ta - ATAN2(minY * bendDir, minX);
				a2 = minAngle * bendDir;
			} else {
				a1 = ta - ATAN2(maxY * bendDir, maxX);
				a2 = maxAngle * bendDir;
			}
		}
	}
break_outer : {
	float os = ATAN2(cy, cx) * s2;
	float rotation = parent->arotation;
	a1 = (a1 - os) * RAD_DEG + o1 - rotation;
	if (a1 > 180) a1 -= 360;
	else if (a1 < -180)
		a1 += 360;
	spBone_updateWorldTransformWith(parent, px, py, rotation + a1 * alpha, sx, sy, 0, 0);
	rotation = child->arotation;
	a2 = ((a2 + os) * RAD_DEG - child->ashearX) * s2 + o2 - rotation;
	if (a2 > 180) a2 -= 360;
	else if (a2 < -180)
		a2 += 360;
	spBone_updateWorldTransformWith(child, cx, cy, rotation + a2 * alpha, child->ascaleX, child->ascaleY,
									child->ashearX, child->ashearY);
}
}
