import { SPINE_ATLAS_CACHE_KEY, SPINE_FILE_TYPE, SPINE_LOADER_TYPE } from "./keys";

export class SpineFile extends Phaser.Loader.MultiFile {
    constructor(loader: Phaser.Loader.LoaderPlugin, key: string, jsonURL: string, atlasURL: string, premultipliedAlpha: boolean = false, jsonXhrSettings: Phaser.Types.Loader.XHRSettingsObject,  atlasXhrSettings: Phaser.Types.Loader.XHRSettingsObject) {
        let json = new Phaser.Loader.FileTypes.JSONFile(loader, key, jsonURL, jsonXhrSettings);
        let atlas = new Phaser.Loader.FileTypes.TextFile(loader, key, atlasURL, atlasXhrSettings);
        atlas.cache = loader.cacheManager.custom[SPINE_ATLAS_CACHE_KEY];
        super(loader, SPINE_FILE_TYPE, key, [json, atlas]);
    }

    addToCache() {
    }
}