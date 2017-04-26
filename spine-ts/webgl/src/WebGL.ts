/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine.webgl {
	export class ManagedWebGLRenderingContext {
		public canvas: HTMLCanvasElement;
		public gl: WebGLRenderingContext;
		private restorables = new Array<Restorable>();

		constructor(canvasOrContext: HTMLCanvasElement | WebGLRenderingContext, contextConfig: any = { alpha: "true" }) {
			if (canvasOrContext instanceof HTMLCanvasElement) {
				let canvas = canvasOrContext;
				this.gl = <WebGLRenderingContext> (canvas.getContext("webgl", contextConfig) || canvas.getContext("experimental-webgl", contextConfig));
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
