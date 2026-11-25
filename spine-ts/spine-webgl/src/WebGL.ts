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

import type { Disposable, Restorable } from "@esotericsoftware/spine-core";

export class ManagedWebGLRenderingContext implements Disposable, Restorable {
	public canvas: HTMLCanvasElement | OffscreenCanvas;
	public gl: WebGLRenderingContext;
	private restorables = [] as Restorable[];

	constructor (canvasOrContext: HTMLCanvasElement | WebGLRenderingContext, contextConfig: WebGLContextAttributes = { alpha: true }) {
		if (!((canvasOrContext instanceof WebGLRenderingContext) || (typeof WebGL2RenderingContext !== 'undefined' && canvasOrContext instanceof WebGL2RenderingContext))) {
			const canvas: HTMLCanvasElement = canvasOrContext;
			this.gl = <WebGLRenderingContext>(canvas.getContext("webgl2", contextConfig) || canvas.getContext("webgl", contextConfig));
			this.canvas = canvas;
			canvas.addEventListener("webglcontextlost", this.contextLostHandler);
			canvas.addEventListener("webglcontextrestored", this.contextRestoredHandler);
		} else {
			this.gl = canvasOrContext;
			this.canvas = this.gl.canvas;
		}
	}

	private contextLostHandler = (e: Event) => {
		if (e) e.preventDefault();
	}

	private contextRestoredHandler = () => {
		this.restore();
	}

	restore (): void {
		for (let i = 0, n = this.restorables.length; i < n; i++)
			this.restorables[i].restore();
	}

	dispose (): void {
		this.canvas.removeEventListener("webglcontextlost", this.contextLostHandler);
		this.canvas.removeEventListener("webglcontextrestored", this.contextRestoredHandler);
	}

	addRestorable (restorable: Restorable) {
		this.restorables.push(restorable);
	}

	removeRestorable (restorable: Restorable) {
		const index = this.restorables.indexOf(restorable);
		if (index > -1) this.restorables.splice(index, 1);
	}

}
