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

#include <spine/PhysicsConstraintData.h>
#include <spine/extension.h>

spPhysicsConstraintData *spPhysicsConstraintData_create(const char *name) {
	spPhysicsConstraintData *self = NEW(spPhysicsConstraintData);
	MALLOC_STR(self->name, name);
	self->bone = NULL;
	self->x = 0;
	self->y = 0;
	self->rotate = 0;
	self->scaleX = 0;
	self->shearX = 0;
	self->limit = 0;
	self->step = 0;
	self->inertia = 0;
	self->strength = 0;
	self->damping = 0;
	self->massInverse = 0;
	self->wind = 0;
	self->gravity = 0;
	self->mix = 0;
	self->inertiaGlobal = 0;
	self->strengthGlobal = 0;
	self->dampingGlobal = 0;
	self->massGlobal = 0;
	self->windGlobal = 0;
	self->gravityGlobal = 0;
	self->mixGlobal = 0;
	return self;
}

void spPhysicsConstraintData_dispose(spPhysicsConstraintData *self) {
	FREE(self->name);
	FREE(self);
}
