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

import { Input } from "./Input";
import { OrthoCamera } from "./Camera";
import { Vector3 } from "./Vector3";

export class CameraController {
	constructor (public canvas: HTMLElement, public camera: OrthoCamera) {
		let cameraX = 0, cameraY = 0, cameraZoom = 0;
		let mouseX = 0, mouseY = 0;
		let lastX = 0, lastY = 0;
		let initialZoom = 0;

		new Input(canvas).addListener({
			down: (x: number, y: number) => {
				cameraX = camera.position.x;
				cameraY = camera.position.y;
				mouseX = lastX = x;
				mouseY = lastY = y;
				initialZoom = camera.zoom;
			},
			dragged: (x: number, y: number) => {
				let deltaX = x - mouseX;
				let deltaY = y - mouseY;
				let originWorld = camera.screenToWorld(new Vector3(0, 0), canvas.clientWidth, canvas.clientHeight);
				let deltaWorld = camera.screenToWorld(new Vector3(deltaX, deltaY), canvas.clientWidth, canvas.clientHeight).sub(originWorld);
				camera.position.set(cameraX - deltaWorld.x, cameraY - deltaWorld.y, 0);
				camera.update();
				lastX = x;
				lastY = y;
			},
			wheel: (delta: number) => {
				let zoomAmount = delta / 200 * camera.zoom;
				let newZoom = camera.zoom + zoomAmount;
				if (newZoom > 0) {
					let x = 0, y = 0;
					if (delta < 0) {
						x = lastX; y = lastY;
					} else {
						let viewCenter = new Vector3(canvas.clientWidth / 2 + 15, canvas.clientHeight / 2);
						let mouseToCenterX = lastX - viewCenter.x;
						let mouseToCenterY = canvas.clientHeight - 1 - lastY - viewCenter.y;
						x = viewCenter.x - mouseToCenterX;
						y = canvas.clientHeight - 1 - viewCenter.y + mouseToCenterY;
					}
					let oldDistance = camera.screenToWorld(new Vector3(x, y), canvas.clientWidth, canvas.clientHeight);
					camera.zoom = newZoom;
					camera.update();
					let newDistance = camera.screenToWorld(new Vector3(x, y), canvas.clientWidth, canvas.clientHeight);
					camera.position.add(oldDistance.sub(newDistance));
					camera.update();
				}
			},
			zoom: (initialDistance, distance) => {
				let newZoom = initialDistance / distance;
				camera.zoom = initialZoom * newZoom;
			},
			up: (x: number, y: number) => {
				lastX = x;
				lastY = y;
			},
			moved: (x: number, y: number) => {
				lastX = x;
				lastY = y;
			},
		});
	}
}
