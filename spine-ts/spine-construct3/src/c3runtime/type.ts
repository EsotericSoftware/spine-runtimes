
import type { SDKInstanceClass } from "./instance.ts";

const C3 = globalThis.C3;

C3.Plugins.EsotericSoftware_SpineConstruct3.Type = class DrawingType extends globalThis.ISDKObjectTypeBase<SDKInstanceClass>
{
	constructor()
	{
		super();
	}


	// async _loadTextures(renderer: IRenderer)
	// {
	// 	return renderer.loadTextureForImageInfo(this.getImageInfo(), {
	// 		sampling: this.runtime.sampling
	// 	});
	// }

	// async _loadTextures(renderer: IRenderer)
	// {
	// 	return renderer.loadTextureForImageInfo(this.getImageInfo(), {
	// 		sampling: this.runtime.sampling
	// 	});
	// }

	// getImageInfo(): IImageInfo
	// {
	// 	return {
	// 		width: 1,
	// 		height: 1,
	// 		getSize(): Vec2Arr { return [1, 1]; },
	// 		getTexture(renderer: IRenderer): ITexture | null { return null; },
	// 		getTexRect(): DOMRect { return new DOMRect(0, 0, 1, 1); }
	// 	};
	// }

	// _onCreate()
	// {
	// 	this.runtime.assets.loadImageAsset(this.getImageInfo());
	// }


	// _releaseTextures(renderer: IRenderer)
	// {
	// 	renderer.releaseTextureForImageInfo(this.getImageInfo());
	// }
};
