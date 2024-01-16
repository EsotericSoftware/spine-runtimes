/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import Phaser from "phaser";
import { SPINE_ATLAS_CACHE_KEY, SPINE_GAME_OBJECT_TYPE, SPINE_SKELETON_DATA_FILE_TYPE, SPINE_ATLAS_FILE_TYPE, SPINE_SKELETON_FILE_CACHE_KEY as SPINE_SKELETON_DATA_CACHE_KEY } from "./keys.js";
import { AtlasAttachmentLoader, GLTexture, SceneRenderer, Skeleton, SkeletonBinary, SkeletonData, SkeletonJson, TextureAtlas } from "@esotericsoftware/spine-webgl"
import { SpineGameObject, SpineGameObjectBoundsProvider } from "./SpineGameObject.js";
import { CanvasTexture, SkeletonRenderer } from "@esotericsoftware/spine-canvas";

/**
 * Configuration object used when creating {@link SpineGameObject} instances via a scene's
 * {@link GameObjectCreator} (`Scene.make`).
 */
export interface SpineGameObjectConfig extends Phaser.Types.GameObjects.GameObjectConfig {
	/** The x-position of the object, optional, default: 0 */
	x?: number,
	/** The y-position of the object, optional, default: 0 */
	y?: number,
	/** The skeleton data key */
	dataKey: string,
	/** The atlas key */
	atlasKey: string
	/** The bounds provider, optional, default: `SetupPoseBoundsProvider` */
	boundsProvider?: SpineGameObjectBoundsProvider
}

/**
 * {@link ScenePlugin} implementation adding Spine Runtime capabilities to a scene.
 *
 * The scene's {@link LoaderPlugin} (`Scene.load`) gets these additional functions:
 * * `spineBinary(key: string, url: string, xhrSettings?: XHRSettingsObject)`: loads a skeleton binary `.skel` file from the `url`.
 * * `spineJson(key: string, url: string, xhrSettings?: XHRSettingsObject)`: loads a skeleton binary `.skel` file from the `url`.
 * * `spineAtlas(key: string, url: string, premultipliedAlpha: boolean = true, xhrSettings?: XHRSettingsObject)`: loads a texture atlas `.atlas` file from the `url` as well as its correponding texture atlas page images.
 *
 * The scene's {@link GameObjectFactory} (`Scene.add`) gets these additional functions:
 * * `spine(x: number, y: number, dataKey: string, atlasKey: string, boundsProvider: SpineGameObjectBoundsProvider = SetupPoseBoundsProvider())`:
 *    creates a new {@link SpineGameObject} from the data and atlas at position `(x, y)`, using the {@link BoundsProvider} to calculate its bounding box. The object is automatically added to the scene.
 *
 * The scene's {@link GameObjectCreator} (`Scene.make`) gets these additional functions:
 * * `spine(config: SpineGameObjectConfig)`: creates a new {@link SpineGameObject} from the given configuration object.
 *
 * The plugin has additional public methods to work with Spine Runtime core API objects:
 * * `getAtlas(atlasKey: string)`: returns the {@link TextureAtlas} instance for the given atlas key.
 * * `getSkeletonData(skeletonDataKey: string)`: returns the {@link SkeletonData} instance for the given skeleton data key.
 * * `createSkeleton(skeletonDataKey: string, atlasKey: string, premultipliedAlpha: boolean = true)`: creates a new {@link Skeleton} instance from the given skeleton data and atlas key.
 * * `isPremultipliedAlpha(atlasKey: string)`: returns `true` if the atlas with the given key has premultiplied alpha.
 */
export class SpinePlugin extends Phaser.Plugins.ScenePlugin {
	game: Phaser.Game;
	private isWebGL: boolean;
	gl: WebGLRenderingContext | null;
	static gameWebGLRenderer: SceneRenderer | null = null;
	get webGLRenderer (): SceneRenderer | null {
		return SpinePlugin.gameWebGLRenderer;
	}
	canvasRenderer: SkeletonRenderer | null;
	phaserRenderer: Phaser.Renderer.Canvas.CanvasRenderer | Phaser.Renderer.WebGL.WebGLRenderer;
	private skeletonDataCache: Phaser.Cache.BaseCache;
	private atlasCache: Phaser.Cache.BaseCache;

	constructor (scene: Phaser.Scene, pluginManager: Phaser.Plugins.PluginManager, pluginKey: string) {
		super(scene, pluginManager, pluginKey);
		this.game = pluginManager.game;
		this.isWebGL = this.game.config.renderType === 2;
		this.gl = this.isWebGL ? (this.game.renderer as Phaser.Renderer.WebGL.WebGLRenderer).gl : null;
		this.phaserRenderer = this.game.renderer;
		this.canvasRenderer = null;
		this.skeletonDataCache = this.game.cache.addCustom(SPINE_SKELETON_DATA_CACHE_KEY);
		this.atlasCache = this.game.cache.addCustom(SPINE_ATLAS_CACHE_KEY);

		let skeletonJsonFileCallback = function (this: any, key: string,
			url: string,
			xhrSettings: Phaser.Types.Loader.XHRSettingsObject) {
			let file = new SpineSkeletonDataFile(this as any, key, url, SpineSkeletonDataFileType.json, xhrSettings);
			this.addFile(file.files);
			return this;
		};
		pluginManager.registerFileType("spineJson", skeletonJsonFileCallback, scene);

		let skeletonBinaryFileCallback = function (this: any, key: string,
			url: string,
			xhrSettings: Phaser.Types.Loader.XHRSettingsObject) {
			let file = new SpineSkeletonDataFile(this as any, key, url, SpineSkeletonDataFileType.binary, xhrSettings);
			this.addFile(file.files);
			return this;
		};
		pluginManager.registerFileType("spineBinary", skeletonBinaryFileCallback, scene);

		let atlasFileCallback = function (this: any, key: string,
			url: string,
			premultipliedAlpha: boolean,
			xhrSettings: Phaser.Types.Loader.XHRSettingsObject) {
			let file = new SpineAtlasFile(this as any, key, url, premultipliedAlpha, xhrSettings);
			this.addFile(file.files);
			return this;
		};
		pluginManager.registerFileType("spineAtlas", atlasFileCallback, scene);

		let self = this;
		let addSpineGameObject = function (this: Phaser.GameObjects.GameObjectFactory, x: number, y: number, dataKey: string, atlasKey: string, boundsProvider: SpineGameObjectBoundsProvider) {
			let gameObject = new SpineGameObject(this.scene, self, x, y, dataKey, atlasKey, boundsProvider);
			this.displayList.add(gameObject);
			this.updateList.add(gameObject);
			return gameObject;
		};

		let makeSpineGameObject = function (this: Phaser.GameObjects.GameObjectFactory, config: SpineGameObjectConfig, addToScene: boolean = false) {
			let x = config.x ? config.x : 0;
			let y = config.y ? config.y : 0;
			let boundsProvider = config.boundsProvider ? config.boundsProvider : undefined;
			let gameObject = new SpineGameObject(this.scene, self, x, y, config.dataKey, config.atlasKey, boundsProvider);
			if (addToScene !== undefined) {
				config.add = addToScene;
			}
			return Phaser.GameObjects.BuildGameObject(this.scene, gameObject, config);
		}
		pluginManager.registerGameObject((window as any).SPINE_GAME_OBJECT_TYPE ? (window as any).SPINE_GAME_OBJECT_TYPE : SPINE_GAME_OBJECT_TYPE, addSpineGameObject, makeSpineGameObject);
	}

	static rendererId = 0;
	boot () {
		Skeleton.yDown = true;
		if (this.isWebGL) {
			if (!SpinePlugin.gameWebGLRenderer) {
				SpinePlugin.gameWebGLRenderer = new SceneRenderer((this.game.renderer! as Phaser.Renderer.WebGL.WebGLRenderer).canvas, this.gl!, true);
			}
			this.onResize();
			this.game.scale.on(Phaser.Scale.Events.RESIZE, this.onResize, this);
		} else {
			if (!this.canvasRenderer) {
				this.canvasRenderer = new SkeletonRenderer(this.scene!.sys.context);
			}
		}

		var eventEmitter = this.systems!.events;
		eventEmitter.once('shutdown', this.shutdown, this);
		eventEmitter.once('destroy', this.destroy, this);
		this.game.events.once('destroy', this.gameDestroy, this);
	}

	onResize () {
		var phaserRenderer = this.game.renderer;
		var sceneRenderer = this.webGLRenderer;

		if (phaserRenderer && sceneRenderer) {
			var viewportWidth = phaserRenderer.width;
			var viewportHeight = phaserRenderer.height;
			sceneRenderer.camera.position.x = viewportWidth / 2;
			sceneRenderer.camera.position.y = viewportHeight / 2;
			sceneRenderer.camera.up.y = -1;
			sceneRenderer.camera.direction.z = 1;
			sceneRenderer.camera.setViewport(viewportWidth, viewportHeight);
		}
	}

	shutdown () {
		this.systems!.events.off("shutdown", this.shutdown, this);
		if (this.isWebGL) {
			this.game.scale.off(Phaser.Scale.Events.RESIZE, this.onResize, this);
		}
	}

	destroy () {
		this.shutdown()
	}

	gameDestroy () {
		this.pluginManager.removeGameObject((window as any).SPINE_GAME_OBJECT_TYPE ? (window as any).SPINE_GAME_OBJECT_TYPE : SPINE_GAME_OBJECT_TYPE, true, true);
		if (this.webGLRenderer) this.webGLRenderer.dispose();
		SpinePlugin.gameWebGLRenderer = null;
	}

	/** Returns the TextureAtlas instance for the given key */
	getAtlas (atlasKey: string) {
		let atlas: TextureAtlas;
		if (this.atlasCache.exists(atlasKey)) {
			atlas = this.atlasCache.get(atlasKey);
		} else {
			let atlasFile = this.game.cache.text.get(atlasKey) as { data: string, premultipliedAlpha: boolean };
			atlas = new TextureAtlas(atlasFile.data);
			if (this.isWebGL) {
				let gl = this.gl!;
				if (GLTexture.DISABLE_UNPACK_PREMULTIPLIED_ALPHA_WEBGL) gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, false);
				for (let atlasPage of atlas.pages) {
					atlasPage.setTexture(new GLTexture(gl, this.game.textures.get(atlasKey + "!" + atlasPage.name).getSourceImage() as HTMLImageElement | ImageBitmap, false));
				}
			} else {
				for (let atlasPage of atlas.pages) {
					atlasPage.setTexture(new CanvasTexture(this.game.textures.get(atlasKey + "!" + atlasPage.name).getSourceImage() as HTMLImageElement | ImageBitmap));
				}
			}
			this.atlasCache.add(atlasKey, atlas);
		}
		return atlas;
	}

	/** Returns whether the TextureAtlas uses premultiplied alpha */
	isAtlasPremultiplied (atlasKey: string) {
		let atlasFile = this.game.cache.text.get(atlasKey);
		if (!atlasFile) return false;
		return atlasFile.premultipliedAlpha;
	}

	/** Returns the SkeletonData instance for the given data and atlas key */
	getSkeletonData (dataKey: string, atlasKey: string) {
		const atlas = this.getAtlas(atlasKey)
		const combinedKey = dataKey + atlasKey;
		let skeletonData: SkeletonData;
		if (this.skeletonDataCache.exists(combinedKey)) {
			skeletonData = this.skeletonDataCache.get(combinedKey);
		} else {
			if (this.game.cache.json.exists(dataKey)) {
				let jsonFile = this.game.cache.json.get(dataKey) as any;
				let json = new SkeletonJson(new AtlasAttachmentLoader(atlas));
				skeletonData = json.readSkeletonData(jsonFile);
			} else {
				let binaryFile = this.game.cache.binary.get(dataKey) as ArrayBuffer;
				let binary = new SkeletonBinary(new AtlasAttachmentLoader(atlas));
				skeletonData = binary.readSkeletonData(new Uint8Array(binaryFile));
			}
			this.skeletonDataCache.add(combinedKey, skeletonData);
		}
		return skeletonData;
	}

	/** Creates a new Skeleton instance from the data and atlas. */
	createSkeleton (dataKey: string, atlasKey: string) {
		return new Skeleton(this.getSkeletonData(dataKey, atlasKey));
	}
}

enum SpineSkeletonDataFileType {
	json,
	binary
}

interface SpineSkeletonDataFileConfig {
	key: string;
	url: string;
	type: "spineJson" | "spineBinary";
	xhrSettings?: Phaser.Types.Loader.XHRSettingsObject
}

class SpineSkeletonDataFile extends Phaser.Loader.MultiFile {
	constructor (loader: Phaser.Loader.LoaderPlugin, key: string | SpineSkeletonDataFileConfig, url?: string, public fileType?: SpineSkeletonDataFileType, xhrSettings?: Phaser.Types.Loader.XHRSettingsObject) {
		if (typeof key !== "string") {
			const config = key;
			key = config.key;
			url = config.url;
			fileType = config.type === "spineJson" ? SpineSkeletonDataFileType.json : SpineSkeletonDataFileType.binary;
			xhrSettings = config.xhrSettings;
		}
		let file = null;
		let isJson = fileType == SpineSkeletonDataFileType.json;
		if (isJson) {
			file = new Phaser.Loader.FileTypes.JSONFile(loader, {
				key: key,
				url: url,
				extension: "json",
				xhrSettings: xhrSettings,
			} as Phaser.Types.Loader.FileTypes.JSONFileConfig);
		} else {
			file = new Phaser.Loader.FileTypes.BinaryFile(loader, {
				key: key,
				url: url,
				extension: "skel",
				xhrSettings: xhrSettings,
			} as Phaser.Types.Loader.FileTypes.BinaryFileConfig);
		}
		super(loader, SPINE_SKELETON_DATA_FILE_TYPE, key, [file]);
	}

	onFileComplete (file: Phaser.Loader.File) {
		this.pending--;
	}

	addToCache () {
		if (this.isReadyToProcess()) this.files[0].addToCache();
	}
}

interface SpineAtlasFileConfig {
	key: string;
	url: string;
	premultipliedAlpha?: boolean;
	xhrSettings?: Phaser.Types.Loader.XHRSettingsObject;
}

class SpineAtlasFile extends Phaser.Loader.MultiFile {
	constructor (loader: Phaser.Loader.LoaderPlugin, key: string | SpineAtlasFileConfig, url?: string, public premultipliedAlpha: boolean = true, xhrSettings?: Phaser.Types.Loader.XHRSettingsObject) {
		if (typeof key !== "string") {
			const config = key;
			key = config.key;
			url = config.url;
			premultipliedAlpha = config.premultipliedAlpha ?? true;
			xhrSettings = config.xhrSettings;
		}

		super(loader, SPINE_ATLAS_FILE_TYPE, key, [
			new Phaser.Loader.FileTypes.TextFile(loader, {
				key: key,
				url: url,
				xhrSettings: xhrSettings,
				extension: "atlas"
			})
		]);
	}

	onFileComplete (file: Phaser.Loader.File) {
		if (this.files.indexOf(file) != -1) {
			this.pending--;

			if (file.type == "text") {
				var lines = file.data.split(/\r\n|\r|\n/);
				let textures = [];
				textures.push(lines[0]);
				for (var t = 1; t < lines.length; t++) {
					var line = lines[t];
					if (line.trim() === '' && t < lines.length - 1) {
						line = lines[t + 1];
						textures.push(line);
					}
				}

				let basePath = file.src.match(/^.*\//) ?? "";
				for (var i = 0; i < textures.length; i++) {
					var url = basePath + textures[i];
					var key = file.key + "!" + textures[i];
					var image = new Phaser.Loader.FileTypes.ImageFile(this.loader, key, url);

					if (!this.loader.keyExists(image)) {
						this.addToMultiFile(image);
						this.loader.addFile(image);
					}
				}
			}
		}
	}

	addToCache () {
		if (this.isReadyToProcess()) {
			let textureManager = this.loader.textureManager;
			for (let file of this.files) {
				if (file.type == "image") {
					if (!textureManager.exists(file.key)) {
						textureManager.addImage(file.key, file.data);
					}
				} else {
					file.data = {
						data: file.data,
						premultipliedAlpha: this.premultipliedAlpha || file.data.indexOf("pma: true") >= 0 || file.data.indexOf("pma:true") >= 0
					};
					file.addToCache();
				}
			}
		}
	}
}
