module spine.threejs {
	export class ThreeJsTexture extends Texture {
		texture: THREE.Texture;

		constructor (image: HTMLImageElement) {
			super(image);
			this.texture = new THREE.Texture(image);
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
			else if (filter === TextureFilter.MipMap) return THREE.LinearMipMapLinearFilter;
			else if (filter === TextureFilter.MipMapLinearLinear) return THREE.LinearMipMapLinearFilter;
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