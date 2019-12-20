/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

module spine {
	export abstract class Texture {
		protected _image: HTMLImageElement;

		constructor (image: HTMLImageElement) {
			this._image = image;
		}

		getImage (): HTMLImageElement {
			return this._image;
		}

		abstract setFilters (minFilter: TextureFilter, magFilter: TextureFilter): void;
		abstract setWraps (uWrap: TextureWrap, vWrap: TextureWrap): void;
		abstract dispose (): void;

		public static filterFromString (text: string): TextureFilter {
			switch (text.toLowerCase()) {
				case "nearest": return TextureFilter.Nearest;
				case "linear": return TextureFilter.Linear;
				case "mipmap": return TextureFilter.MipMap;
				case "mipmapnearestnearest": return TextureFilter.MipMapNearestNearest;
				case "mipmaplinearnearest": return TextureFilter.MipMapLinearNearest;
				case "mipmapnearestlinear": return TextureFilter.MipMapNearestLinear;
				case "mipmaplinearlinear": return TextureFilter.MipMapLinearLinear;
				default: throw new Error(`Unknown texture filter ${text}`);
			}
		}

		public static wrapFromString (text: string): TextureWrap {
			switch (text.toLowerCase()) {
				case "mirroredtepeat": return TextureWrap.MirroredRepeat;
				case "clamptoedge": return TextureWrap.ClampToEdge;
				case "repeat": return TextureWrap.Repeat;
				default: throw new Error(`Unknown texture wrap ${text}`);
			}
		}
	}

	export enum TextureFilter {
		Nearest = 9728, // WebGLRenderingContext.NEAREST
		Linear = 9729, // WebGLRenderingContext.LINEAR
		MipMap = 9987, // WebGLRenderingContext.LINEAR_MIPMAP_LINEAR
		MipMapNearestNearest = 9984, // WebGLRenderingContext.NEAREST_MIPMAP_NEAREST
		MipMapLinearNearest = 9985, // WebGLRenderingContext.LINEAR_MIPMAP_NEAREST
		MipMapNearestLinear = 9986, // WebGLRenderingContext.NEAREST_MIPMAP_LINEAR
		MipMapLinearLinear = 9987 // WebGLRenderingContext.LINEAR_MIPMAP_LINEAR
	}

	export enum TextureWrap {
		MirroredRepeat = 33648, // WebGLRenderingContext.MIRRORED_REPEAT
		ClampToEdge = 33071, // WebGLRenderingContext.CLAMP_TO_EDGE
		Repeat = 10497 // WebGLRenderingContext.REPEAT
	}

	export class TextureRegion {
		renderObject: any;
		u = 0; v = 0;
		u2 = 0; v2 = 0;
		width = 0; height = 0;
		rotate = false;
		offsetX = 0; offsetY = 0;
		originalWidth = 0; originalHeight = 0;
	}

	export class FakeTexture extends Texture {
		setFilters(minFilter: TextureFilter, magFilter: TextureFilter) { }
		setWraps(uWrap: TextureWrap, vWrap: TextureWrap) { }
		dispose() { }
	}
}
