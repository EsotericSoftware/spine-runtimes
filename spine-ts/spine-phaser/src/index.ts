export * from "./require-shim.js"
export * from "./SpinePlugin.js"
export * from "./SpineGameObject.js"
export * from "./mixins.js"
export * from "@esotericsoftware/spine-core";
export * from "@esotericsoftware/spine-webgl";
import { SpineGameObjectConfig, SpinePlugin } from "./SpinePlugin.js";
(window as any).spine = { SpinePlugin: SpinePlugin };
(window as any)["spine.SpinePlugin"] = SpinePlugin;

import { SpineGameObject, SpineGameObjectBoundsProvider } from "./SpineGameObject.js";

declare global {
	namespace Phaser.Loader {
		export interface LoaderPlugin {
			spineJson (key: string, url: string, xhrSettings?: Phaser.Types.Loader.XHRSettingsObject): LoaderPlugin;
			spineBinary (key: string, url: string, xhrSettings?: Phaser.Types.Loader.XHRSettingsObject): LoaderPlugin;
			spineAtlas (key: string, url: string, premultipliedAlpha?: boolean, xhrSettings?: Phaser.Types.Loader.XHRSettingsObject): LoaderPlugin;
		}
	}

	namespace Phaser.GameObjects {
		export interface GameObjectFactory {
			spine (x: number, y: number, dataKey: string, atlasKey: string, boundsProvider?: SpineGameObjectBoundsProvider): SpineGameObject;
		}

		export interface GameObjectCreator {
			spine (config: SpineGameObjectConfig, addToScene?: boolean): SpineGameObject;
		}
	}

	namespace Phaser {
		export interface Scene {
			spine: SpinePlugin;
		}
	}
}
