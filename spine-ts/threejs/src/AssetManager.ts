module spine.threejs {
	export class AssetManager extends spine.AssetManager {
		constructor () {
			super((image: HTMLImageElement) => {
				return new ThreeJsTexture(image);
			});
		}
	}
}