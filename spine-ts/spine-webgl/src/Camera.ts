/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { Matrix4 } from "./Matrix4";
import { Vector3 } from "./Vector3";

export class OrthoCamera {
	position = new Vector3(0, 0, 0);
	direction = new Vector3(0, 0, -1);
	up = new Vector3(0, 1, 0);
	near = 0;
	far = 100;
	zoom = 1;
	viewportWidth = 0;
	viewportHeight = 0;
	projectionView = new Matrix4();
	inverseProjectionView = new Matrix4();
	projection = new Matrix4();
	view = new Matrix4();

	constructor (viewportWidth: number, viewportHeight: number) {
		this.viewportWidth = viewportWidth;
		this.viewportHeight = viewportHeight;
		this.update();
	}

	update () {
		let projection = this.projection;
		let view = this.view;
		let projectionView = this.projectionView;
		let inverseProjectionView = this.inverseProjectionView;
		let zoom = this.zoom, viewportWidth = this.viewportWidth, viewportHeight = this.viewportHeight;
		projection.ortho(zoom * (-viewportWidth / 2), zoom * (viewportWidth / 2),
			zoom * (-viewportHeight / 2), zoom * (viewportHeight / 2),
			this.near, this.far);
		view.lookAt(this.position, this.direction, this.up);
		projectionView.set(projection.values);
		projectionView.multiply(view);
		inverseProjectionView.set(projectionView.values).invert();
	}

	screenToWorld (screenCoords: Vector3, screenWidth: number, screenHeight: number) {
		let x = screenCoords.x, y = screenHeight - screenCoords.y - 1;
		screenCoords.x = (2 * x) / screenWidth - 1;
		screenCoords.y = (2 * y) / screenHeight - 1;
		screenCoords.z = (2 * screenCoords.z) - 1;
		screenCoords.project(this.inverseProjectionView);
		return screenCoords;
	}

	worldToScreen (worldCoords: Vector3, screenWidth: number, screenHeight: number) {
		worldCoords.project(this.projectionView);
		worldCoords.x = screenWidth * (worldCoords.x + 1) / 2;
		worldCoords.y = screenHeight * (worldCoords.y + 1) / 2;
		worldCoords.z = (worldCoords.z + 1) / 2;
		return worldCoords;
	}

	setViewport (viewportWidth: number, viewportHeight: number) {
		this.viewportWidth = viewportWidth;
		this.viewportHeight = viewportHeight;
	}
}
