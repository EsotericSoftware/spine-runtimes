import type { AssetExtension, LoadAsset } from "@pixi/assets";
import { LoaderParserPriority, checkExtension } from "@pixi/assets";
import { ExtensionType, settings, extensions } from "@pixi/core";

type SkeletonJsonAsset = any;
type SkeletonBinaryAsset = Uint8Array;

function isJson(resource: any): resource is SkeletonJsonAsset {
	return resource.hasOwnProperty("bones");
}

function isBuffer(resource: any): resource is SkeletonBinaryAsset {
	return resource instanceof Uint8Array;
}

const spineLoaderExtension: AssetExtension<SkeletonJsonAsset | SkeletonBinaryAsset> = {
	extension: ExtensionType.Asset,

	loader: {
		extension: {
			type: ExtensionType.LoadParser,
			priority: LoaderParserPriority.Normal,
		},

		test(url) {
			return checkExtension(url, ".skel");
		},

		async load(url: string): Promise<SkeletonBinaryAsset> {
			const response = await settings.ADAPTER.fetch(url);

			const buffer = new Uint8Array(await response.arrayBuffer());

			return buffer;
		},
		testParse(asset: unknown, options: LoadAsset): Promise<boolean> {
			const isJsonSpineModel = checkExtension(options.src, ".json") && isJson(asset);
			const isBinarySpineModel = checkExtension(options.src, ".skel") && isBuffer(asset);

			return Promise.resolve(isJsonSpineModel || isBinarySpineModel);
		},
	},
} as AssetExtension<SkeletonJsonAsset | SkeletonBinaryAsset>;

extensions.add(spineLoaderExtension);
