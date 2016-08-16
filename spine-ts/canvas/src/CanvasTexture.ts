module spine.canvas {
	export class CanvasTexture extends Texture {
		constructor (image: HTMLImageElement) {
			super(image);
		}

		setFilters (minFilter: TextureFilter, magFilter: TextureFilter) { }
		setWraps (uWrap: TextureWrap, vWrap: TextureWrap) { }
		dispose () { }
	}
}