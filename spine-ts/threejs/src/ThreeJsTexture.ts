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

module spine.threejs {
	export class ThreeJsTexture extends Texture {
		texture: THREE.Texture;

		constructor (image: HTMLImageElement) {
			super(image);
			this.texture = new THREE.Texture(image);
			this.texture.flipY = false;
			this.texture.needsUpdate = true;
		}

		setFilters (minFilter: TextureFilter, magFilter: TextureFilter) {
			this.texture.minFilter = ThreeJsTexture.toThreeJsTextureFilter(minFilter);
			this.texture.magFilter = ThreeJsTexture.toThreeJsTextureFilter(magFilter);
		}

		setWraps (uWrap: TextureWrap, vWrap: TextureWrap) {
			this.texture.wrapS = ThreeJsTexture.toThreeJsTextureWrap(uWrap);
			this.texture.wrapT = ThreeJsTexture.toThreeJsTextureWrap(vWrap);
		}

		dispose () {
			this.texture.dispose();
		}

		static toThreeJsTextureFilter(filter: TextureFilter) {
			if (filter === TextureFilter.Linear) return THREE.LinearFilter;
			else if (filter === TextureFilter.MipMap) return THREE.LinearMipMapLinearFilter; // also includes TextureFilter.MipMapLinearLinear
			else if (filter === TextureFilter.MipMapLinearNearest) return THREE.LinearMipMapNearestFilter;
			else if (filter === TextureFilter.MipMapNearestLinear) return THREE.NearestMipMapLinearFilter;
			else if (filter === TextureFilter.MipMapNearestNearest) return THREE.NearestMipMapNearestFilter;
			else if (filter === TextureFilter.Nearest) return THREE.NearestFilter;
			else throw new Error("Unknown texture filter: " + filter);
		}

		static toThreeJsTextureWrap(wrap: TextureWrap) {
			if (wrap === TextureWrap.ClampToEdge) return THREE.ClampToEdgeWrapping;
			else if (wrap === TextureWrap.MirroredRepeat) return THREE.MirroredRepeatWrapping;
			else if (wrap === TextureWrap.Repeat) return THREE.RepeatWrapping;
			else throw new Error("Unknown texture wrap: " + wrap);
		}
	}
}
