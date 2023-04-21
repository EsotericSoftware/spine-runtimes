export * from "./require-shim"
export * from "./SpinePlugin"
export * from "./SpineGameObject"
export * from "./mixins"
export * from "@esotericsoftware/spine-core";
export * from "@esotericsoftware/spine-webgl";
import { SpinePlugin } from "./SpinePlugin";
(window as any).spine = { SpinePlugin: SpinePlugin };
(window as any)["spine.SpinePlugin"] = SpinePlugin;

import { SpineGameObject, SpineGameObjectBoundsProvider } from "./SpineGameObject";

declare global {
    namespace Phaser.Loader {
        export interface LoaderPlugin {
            spineJson(key: string, url: string, xhrSettings?: Phaser.Types.Loader.XHRSettingsObject): LoaderPlugin;
            spineBinary(key: string, url: string, xhrSettings?: Phaser.Types.Loader.XHRSettingsObject): LoaderPlugin;
            spineAtlas(key: string, url: string, premultipliedAlpha?: boolean, xhrSettings?: Phaser.Types.Loader.XHRSettingsObject): LoaderPlugin;
        }
    }

    namespace Phaser.GameObjects {
        export interface GameObjectFactory {
            spine(x: number, y: number, dataKey: string, atlasKey: string, boundsProvider?: SpineGameObjectBoundsProvider): SpineGameObject;
        }

        export interface GameObjectCreator {
            spine(config: any, addToScene: boolean): SpineGameObject;
        }
    }
}
