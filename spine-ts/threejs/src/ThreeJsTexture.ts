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
