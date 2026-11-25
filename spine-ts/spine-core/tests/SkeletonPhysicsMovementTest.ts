#!/usr/bin/env npx -y tsx

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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import assert from "node:assert/strict";
import { type Skeleton, SkeletonPhysicsMovement } from "../src/index.js";

const translations: Array<[number, number]> = [];
const rotations: number[] = [];
const skeleton = {
	physicsTranslate: (x: number, y: number) => translations.push([x, y]),
	physicsRotate: (_x: number, _y: number, degrees: number) => rotations.push(degrees),
} as unknown as Skeleton;

const host = { x: 0, y: 0, z: 0, rotation: 0 };
const rotationReads: boolean[] = [];
const movement = new SkeletonPhysicsMovement(skeleton, {
	readTransform: (out, readRotation) => {
		rotationReads.push(readRotation);
		out.x = host.x;
		out.y = host.y;
		out.z = host.z;
		if (readRotation) out.rotation = host.rotation;
	},
	// Simulates a skeleton plane rotated 90 degrees around world Y:
	// world Z maps to skeleton-local X, while world Y remains local Y.
	worldToSkeleton: point => {
		point.x = point.z;
	},
}, {
	positionInheritanceX: 1,
	positionInheritanceY: 1,
	rotationInheritance: 1,
});

movement.applyTransformMovement();
assert.deepEqual(translations, []);
assert.deepEqual(rotations, []);
assert.deepEqual(rotationReads, [true]);

host.z = 6;
host.rotation = 190;
movement.applyTransformMovement();
assert.deepEqual(translations, [[6, 0]]);
assert.deepEqual(rotations, [-170]);

movement.rotationInheritance = 0;
host.z = 10;
host.rotation = 200;
movement.applyTransformMovement();
assert.deepEqual(translations.at(-1), [4, 0]);
assert.equal(rotationReads.at(-1), false);
assert.deepEqual(rotations, [-170]);

const readsBeforeReset = rotationReads.length;
movement.resetTransform();
assert.equal(rotationReads.length, readsBeforeReset + 1);
assert.equal(rotationReads.at(-1), true);

function testPositionResetEstablishesRotationBaseline (resetPosition: (movement: SkeletonPhysicsMovement) => void) {
	const inheritedRotations: number[] = [];
	const resetSkeleton = {
		physicsTranslate: () => { },
		physicsRotate: (_x: number, _y: number, degrees: number) => inheritedRotations.push(degrees),
	} as unknown as Skeleton;
	const resetHost = { x: 0, y: 0, z: 0, rotation: 45 };
	const resetMovement = new SkeletonPhysicsMovement(resetSkeleton, {
		readTransform: (out, readRotation) => {
			out.x = resetHost.x;
			out.y = resetHost.y;
			out.z = resetHost.z;
			if (readRotation) out.rotation = resetHost.rotation;
		},
		worldToSkeleton: () => { },
	}, { rotationInheritance: 1 });

	resetPosition(resetMovement);
	resetHost.rotation = 50;
	resetMovement.applyTransformMovement();
	assert.deepEqual(inheritedRotations, [5]);
}

testPositionResetEstablishesRotationBaseline(movement => movement.resetPosition());
testPositionResetEstablishesRotationBaseline(movement => movement.setPositionInheritance(1, 1));

console.log("SkeletonPhysicsMovement tests passed");
