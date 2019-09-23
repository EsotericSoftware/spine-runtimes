/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine.webgl {
	export class ManagedWebGLRenderingContext {
		public canvas: HTMLCanvasElement | OffscreenCanvas;
		public gl: WebGLRenderingContext;
		private restorables = new Array<Restorable>();

		constructor(canvasOrContext: HTMLCanvasElement | WebGLRenderingContext, contextConfig: any = { alpha: "true" }) {
			if (canvasOrContext instanceof HTMLCanvasElement) {
				let canvas = canvasOrContext;
				this.gl = <WebGLRenderingContext> (canvas.getContext("webgl2", contextConfig) || canvas.getContext("webgl", contextConfig));
				this.canvas = canvas;
				canvas.addEventListener("webglcontextlost", (e: any) => {
					let event = <WebGLContextEvent>e;
					if (e) {
						e.preventDefault();
					}
				});

				canvas.addEventListener("webglcontextrestored", (e: any) => {
					for (let i = 0, n = this.restorables.length; i < n; i++) {
						this.restorables[i].restore();
					}
				});
			} else {
				this.gl = canvasOrContext;
				this.canvas = this.gl.canvas;
			}
		}

		addRestorable(restorable: Restorable) {
			this.restorables.push(restorable);
		}

		removeRestorable(restorable: Restorable) {
			let index = this.restorables.indexOf(restorable);
			if (index > -1) this.restorables.splice(index, 1);
		}
	}

	export class WebGLBlendModeConverter {
		static ZERO = 0;
		static ONE = 1;
		static SRC_COLOR = 0x0300;
		static ONE_MINUS_SRC_COLOR = 0x0301;
		static SRC_ALPHA = 0x0302;
		static ONE_MINUS_SRC_ALPHA = 0x0303;
		static DST_ALPHA = 0x0304;
		static ONE_MINUS_DST_ALPHA = 0x0305;
		static DST_COLOR = 0x0306

		static getDestGLBlendMode (blendMode: BlendMode) {
			switch(blendMode) {
				case BlendMode.Normal: return WebGLBlendModeConverter.ONE_MINUS_SRC_ALPHA;
				case BlendMode.Additive: return WebGLBlendModeConverter.ONE;
				case BlendMode.Multiply: return WebGLBlendModeConverter.ONE_MINUS_SRC_ALPHA;
				case BlendMode.Screen: return WebGLBlendModeConverter.ONE_MINUS_SRC_ALPHA;
				default: throw new Error("Unknown blend mode: " + blendMode);
			}
		}

		static getSourceGLBlendMode (blendMode: BlendMode, premultipliedAlpha: boolean = false) {
			switch(blendMode) {
				case BlendMode.Normal: return premultipliedAlpha? WebGLBlendModeConverter.ONE : WebGLBlendModeConverter.SRC_ALPHA;
				case BlendMode.Additive: return premultipliedAlpha? WebGLBlendModeConverter.ONE : WebGLBlendModeConverter.SRC_ALPHA;
				case BlendMode.Multiply: return WebGLBlendModeConverter.DST_COLOR;
				case BlendMode.Screen: return WebGLBlendModeConverter.ONE;
				default: throw new Error("Unknown blend mode: " + blendMode);
			}
		}
	}
}
