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
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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

import { type Bone, MathUtils, Physics } from "@esotericsoftware/spine-core";
import * as Phaser from "phaser";
import type { SpineGameObject } from "../SpineGameObject.js";
import type { SpineGameObjectRenderer } from "./SpineGameObjectRenderer.js";

export class SpineCanvasRenderer implements SpineGameObjectRenderer {
	readonly type = "spine-canvas";

	renderWebGL () {
	}

	renderCanvas (
		renderer: Phaser.Renderer.Canvas.CanvasRenderer,
		src: SpineGameObject,
		camera: Phaser.Cameras.Scene2D.Camera,
		parentMatrix?: Phaser.GameObjects.Components.TransformMatrix
	): void {
		const skeletonRenderer = src.plugin.canvasRenderer;
		if (!skeletonRenderer) return;

		const context = renderer.currentContext;
		(skeletonRenderer as unknown as { ctx: CanvasRenderingContext2D }).ctx = context;

		camera.addToRenderList(src);
		const transform = Phaser.GameObjects.GetCalcMatrix(src, camera, parentMatrix).calc;
		const skeleton = src.skeleton;
		const offsetX = src.renderOffsetX;
		const offsetY = src.renderOffsetY;
		skeleton.x = transform.tx + offsetX * transform.a + offsetY * transform.c;
		skeleton.y = transform.ty + offsetX * transform.b + offsetY * transform.d;
		skeleton.scaleX = transform.scaleX;
		skeleton.scaleY = transform.scaleY;
		const root = skeleton.getRootBone() as Bone;
		root.appliedPose.rotation = -MathUtils.radiansToDegrees * transform.rotationNormalized;
		skeleton.updateWorldTransform(Physics.update);

		context.save();
		context.globalAlpha *= src.alpha;
		skeletonRenderer.draw(skeleton);
		context.restore();
	}

	// disposed by SpinePlugin.
	preDestroy () {
	}
}
