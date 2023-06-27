import { TextureAtlas } from "@esotericsoftware/spine-core";
import { SpineTexture } from "../SpineTexture";
import type { AssetExtension, LoadAsset, Loader } from "@pixi/assets";
import { LoaderParserPriority, checkExtension } from "@pixi/assets";
import type { Texture } from "@pixi/core";
import { ExtensionType, settings, utils, BaseTexture, extensions } from "@pixi/core";

type RawAtlas = string;

const spineTextureAtlasLoader: AssetExtension<RawAtlas | TextureAtlas, ISpineAtlasMetadata> = {
	extension: ExtensionType.Asset,

	loader: {
		extension: {
			type: ExtensionType.LoadParser,
			priority: LoaderParserPriority.Normal,
			name: "spineTextureAtlasLoader",
		},

		test(url: string): boolean {
			return checkExtension(url, ".atlas");
		},

		async load(url: string): Promise<RawAtlas> {
			const response = await settings.ADAPTER.fetch(url);

			const txt = await response.text();

			return txt;
		},

		testParse(asset: unknown, options: LoadAsset): Promise<boolean> {
			const isExtensionRight = checkExtension(options.src, ".atlas");
			const isString = typeof asset === "string";

			return Promise.resolve(isExtensionRight && isString);
		},

		unload(atlas: TextureAtlas) {
			atlas.dispose();
		},

		async parse(asset: RawAtlas, options: LoadAsset, loader: Loader): Promise<TextureAtlas> {
			const metadata: ISpineAtlasMetadata = options.data || {};
			let basePath = utils.path.dirname(options.src);

			if (basePath && basePath.lastIndexOf("/") !== basePath.length - 1) {
				basePath += "/";
			}

			// Retval is going to be a texture atlas. However we need to wait for it's callback to resolve this promise.
			const retval = new TextureAtlas(asset);

			// If the user gave me only one texture, that one is assumed to be the "first" texture in the atlas
			if (metadata.images instanceof BaseTexture || typeof metadata.images === "string") {
				const pixiTexture = metadata.images;
				metadata.images = {} as Record<string, BaseTexture | string>;
				metadata.images[retval.pages[0].name] = pixiTexture;
			}

			// we will wait for all promises for the textures at the same time at the end.
			const textureLoadingPromises = [];

			// fill the pages
			for (const page of retval.pages) {
				const pageName = page.name;
				const providedPage = metadata?.images ? metadata.images[pageName] : undefined;
				if (providedPage instanceof BaseTexture) {
					page.setTexture(SpineTexture.from(providedPage));
				} else {
					const url: string = providedPage ?? utils.path.normalize([...basePath.split(utils.path.sep), pageName].join(utils.path.sep));
					const pixiPromise = loader.load<Texture>({ src: url, data: metadata.imageMetadata }).then((texture) => {
						page.setTexture(SpineTexture.from(texture.baseTexture));
					});
					textureLoadingPromises.push(pixiPromise);
				}
			}

			await Promise.all(textureLoadingPromises);

			return retval;
		},
	},
} as AssetExtension<RawAtlas | TextureAtlas, ISpineAtlasMetadata>;

extensions.add(spineTextureAtlasLoader);

export interface ISpineAtlasMetadata {
	// If you are downloading an .atlas file, this metadata will go to the Texture loader
	imageMetadata?: any;
	// If you already have atlas pages loaded as pixi textures and want to use that to create the atlas, you can pass them here
	images?: BaseTexture | string | Record<string, BaseTexture | string>;
}
