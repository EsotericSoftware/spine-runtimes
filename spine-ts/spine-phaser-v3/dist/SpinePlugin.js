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
import * as Phaser from "phaser";
import { SPINE_ATLAS_CACHE_KEY, SPINE_GAME_OBJECT_TYPE, SPINE_SKELETON_DATA_FILE_TYPE, SPINE_ATLAS_FILE_TYPE, SPINE_SKELETON_FILE_CACHE_KEY as SPINE_SKELETON_DATA_CACHE_KEY } from "./keys.js";
import { AtlasAttachmentLoader, GLTexture, SceneRenderer, Skeleton, SkeletonBinary, SkeletonJson, TextureAtlas } from "@esotericsoftware/spine-webgl";
import { SpineGameObject } from "./SpineGameObject.js";
import { CanvasTexture, SkeletonRenderer } from "@esotericsoftware/spine-canvas";
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
    game;
    isWebGL;
    gl;
    static gameWebGLRenderer = null;
    get webGLRenderer() {
        return SpinePlugin.gameWebGLRenderer;
    }
    canvasRenderer;
    phaserRenderer;
    skeletonDataCache;
    atlasCache;
    constructor(scene, pluginManager, pluginKey) {
        super(scene, pluginManager, pluginKey);
        this.game = pluginManager.game;
        this.isWebGL = this.game.config.renderType === 2;
        this.gl = this.isWebGL ? this.game.renderer.gl : null;
        this.phaserRenderer = this.game.renderer;
        this.canvasRenderer = null;
        this.skeletonDataCache = this.game.cache.addCustom(SPINE_SKELETON_DATA_CACHE_KEY);
        this.atlasCache = this.game.cache.addCustom(SPINE_ATLAS_CACHE_KEY);
        let skeletonJsonFileCallback = function (key, url, xhrSettings) {
            let file = new SpineSkeletonDataFile(this, key, url, SpineSkeletonDataFileType.json, xhrSettings);
            this.addFile(file.files);
            return this;
        };
        pluginManager.registerFileType("spineJson", skeletonJsonFileCallback, scene);
        let skeletonBinaryFileCallback = function (key, url, xhrSettings) {
            let file = new SpineSkeletonDataFile(this, key, url, SpineSkeletonDataFileType.binary, xhrSettings);
            this.addFile(file.files);
            return this;
        };
        pluginManager.registerFileType("spineBinary", skeletonBinaryFileCallback, scene);
        let atlasFileCallback = function (key, url, premultipliedAlpha, xhrSettings) {
            let file = new SpineAtlasFile(this, key, url, premultipliedAlpha, xhrSettings);
            this.addFile(file.files);
            return this;
        };
        pluginManager.registerFileType("spineAtlas", atlasFileCallback, scene);
        let addSpineGameObject = function (x, y, dataKey, atlasKey, boundsProvider) {
            if (this.scene.sys.renderer instanceof Phaser.Renderer.WebGL.WebGLRenderer) {
                this.scene.sys.renderer.pipelines.clear();
            }
            const spinePlugin = this.scene.sys[pluginKey];
            let gameObject = new SpineGameObject(this.scene, spinePlugin, x, y, dataKey, atlasKey, boundsProvider);
            this.displayList.add(gameObject);
            this.updateList.add(gameObject);
            if (this.scene.sys.renderer instanceof Phaser.Renderer.WebGL.WebGLRenderer) {
                this.scene.sys.renderer.pipelines.rebind();
            }
            return gameObject;
        };
        let makeSpineGameObject = function (config, addToScene = false) {
            if (this.scene.sys.renderer instanceof Phaser.Renderer.WebGL.WebGLRenderer) {
                this.scene.sys.renderer.pipelines.clear();
            }
            let x = config.x ? config.x : 0;
            let y = config.y ? config.y : 0;
            let boundsProvider = config.boundsProvider ? config.boundsProvider : undefined;
            const spinePlugin = this.scene.sys[pluginKey];
            let gameObject = new SpineGameObject(this.scene, spinePlugin, x, y, config.dataKey, config.atlasKey, boundsProvider);
            if (addToScene !== undefined) {
                config.add = addToScene;
            }
            if (this.scene.sys.renderer instanceof Phaser.Renderer.WebGL.WebGLRenderer) {
                this.scene.sys.renderer.pipelines.rebind();
            }
            return Phaser.GameObjects.BuildGameObject(this.scene, gameObject, config);
        };
        pluginManager.registerGameObject(window.SPINE_GAME_OBJECT_TYPE ? window.SPINE_GAME_OBJECT_TYPE : SPINE_GAME_OBJECT_TYPE, addSpineGameObject, makeSpineGameObject);
    }
    static rendererId = 0;
    boot() {
        Skeleton.yDown = true;
        if (this.isWebGL) {
            if (!SpinePlugin.gameWebGLRenderer) {
                SpinePlugin.gameWebGLRenderer = new SceneRenderer(this.game.renderer.canvas, this.gl, true);
            }
            this.onResize();
            this.game.scale.on(Phaser.Scale.Events.RESIZE, this.onResize, this);
        }
        else {
            if (!this.canvasRenderer) {
                this.canvasRenderer = new SkeletonRenderer(this.scene.sys.context);
            }
        }
        var eventEmitter = this.systems.events;
        eventEmitter.once('shutdown', this.shutdown, this);
        eventEmitter.once('destroy', this.destroy, this);
        this.game.events.once('destroy', this.gameDestroy, this);
    }
    onResize() {
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
    shutdown() {
        this.systems.events.off("shutdown", this.shutdown, this);
        if (this.isWebGL) {
            this.game.scale.off(Phaser.Scale.Events.RESIZE, this.onResize, this);
        }
    }
    destroy() {
        this.shutdown();
    }
    gameDestroy() {
        this.pluginManager.removeGameObject(window.SPINE_GAME_OBJECT_TYPE ? window.SPINE_GAME_OBJECT_TYPE : SPINE_GAME_OBJECT_TYPE, true, true);
        if (this.webGLRenderer)
            this.webGLRenderer.dispose();
        SpinePlugin.gameWebGLRenderer = null;
    }
    /** Returns the TextureAtlas instance for the given key */
    getAtlas(atlasKey) {
        let atlas;
        if (this.atlasCache.exists(atlasKey)) {
            atlas = this.atlasCache.get(atlasKey);
        }
        else {
            let atlasFile = this.game.cache.text.get(atlasKey);
            atlas = new TextureAtlas(atlasFile.data);
            if (this.isWebGL) {
                let gl = this.gl;
                const phaserUnpackPmaValue = gl.getParameter(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL);
                if (phaserUnpackPmaValue)
                    gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, false);
                for (let atlasPage of atlas.pages) {
                    atlasPage.setTexture(new GLTexture(gl, this.game.textures.get(atlasKey + "!" + atlasPage.name).getSourceImage(), false));
                }
                if (phaserUnpackPmaValue)
                    gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, true);
            }
            else {
                for (let atlasPage of atlas.pages) {
                    atlasPage.setTexture(new CanvasTexture(this.game.textures.get(atlasKey + "!" + atlasPage.name).getSourceImage()));
                }
            }
            this.atlasCache.add(atlasKey, atlas);
        }
        return atlas;
    }
    /** Returns whether the TextureAtlas uses premultiplied alpha */
    isAtlasPremultiplied(atlasKey) {
        let atlasFile = this.game.cache.text.get(atlasKey);
        if (!atlasFile)
            return false;
        return atlasFile.premultipliedAlpha;
    }
    /** Returns the SkeletonData instance for the given data and atlas key */
    getSkeletonData(dataKey, atlasKey) {
        const atlas = this.getAtlas(atlasKey);
        const combinedKey = dataKey + atlasKey;
        let skeletonData;
        if (this.skeletonDataCache.exists(combinedKey)) {
            skeletonData = this.skeletonDataCache.get(combinedKey);
        }
        else {
            if (this.game.cache.json.exists(dataKey)) {
                let jsonFile = this.game.cache.json.get(dataKey);
                let json = new SkeletonJson(new AtlasAttachmentLoader(atlas));
                skeletonData = json.readSkeletonData(jsonFile);
            }
            else {
                let binaryFile = this.game.cache.binary.get(dataKey);
                let binary = new SkeletonBinary(new AtlasAttachmentLoader(atlas));
                skeletonData = binary.readSkeletonData(new Uint8Array(binaryFile));
            }
            this.skeletonDataCache.add(combinedKey, skeletonData);
        }
        return skeletonData;
    }
    /** Creates a new Skeleton instance from the data and atlas. */
    createSkeleton(dataKey, atlasKey) {
        return new Skeleton(this.getSkeletonData(dataKey, atlasKey));
    }
}
var SpineSkeletonDataFileType;
(function (SpineSkeletonDataFileType) {
    SpineSkeletonDataFileType[SpineSkeletonDataFileType["json"] = 0] = "json";
    SpineSkeletonDataFileType[SpineSkeletonDataFileType["binary"] = 1] = "binary";
})(SpineSkeletonDataFileType || (SpineSkeletonDataFileType = {}));
class SpineSkeletonDataFile extends Phaser.Loader.MultiFile {
    fileType;
    constructor(loader, key, url, fileType, xhrSettings) {
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
            });
        }
        else {
            file = new Phaser.Loader.FileTypes.BinaryFile(loader, {
                key: key,
                url: url,
                extension: "skel",
                xhrSettings: xhrSettings,
            });
        }
        super(loader, SPINE_SKELETON_DATA_FILE_TYPE, key, [file]);
        this.fileType = fileType;
    }
    onFileComplete(file) {
        this.pending--;
    }
    addToCache() {
        if (this.isReadyToProcess())
            this.files[0].addToCache();
    }
}
class SpineAtlasFile extends Phaser.Loader.MultiFile {
    premultipliedAlpha;
    constructor(loader, key, url, premultipliedAlpha, xhrSettings) {
        if (typeof key !== "string") {
            const config = key;
            key = config.key;
            url = config.url;
            premultipliedAlpha = config.premultipliedAlpha;
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
        this.premultipliedAlpha = premultipliedAlpha;
    }
    onFileComplete(file) {
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
    addToCache() {
        if (this.isReadyToProcess()) {
            let textureManager = this.loader.textureManager;
            for (let file of this.files) {
                if (file.type == "image") {
                    if (!textureManager.exists(file.key)) {
                        textureManager.addImage(file.key, file.data);
                    }
                }
                else {
                    this.premultipliedAlpha = this.premultipliedAlpha ?? (file.data.indexOf("pma: true") >= 0 || file.data.indexOf("pma:true") >= 0);
                    file.data = {
                        data: file.data,
                        premultipliedAlpha: this.premultipliedAlpha,
                    };
                    file.addToCache();
                }
            }
        }
    }
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiU3BpbmVQbHVnaW4uanMiLCJzb3VyY2VSb290IjoiIiwic291cmNlcyI6WyIuLi9zcmMvU3BpbmVQbHVnaW4udHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7OzsrRUEyQitFO0FBRS9FLE9BQU8sS0FBSyxNQUFNLE1BQU0sUUFBUSxDQUFDO0FBQ2pDLE9BQU8sRUFBRSxxQkFBcUIsRUFBRSxzQkFBc0IsRUFBRSw2QkFBNkIsRUFBRSxxQkFBcUIsRUFBRSw2QkFBNkIsSUFBSSw2QkFBNkIsRUFBRSxNQUFNLFdBQVcsQ0FBQztBQUNoTSxPQUFPLEVBQUUscUJBQXFCLEVBQUUsU0FBUyxFQUFFLGFBQWEsRUFBRSxRQUFRLEVBQUUsY0FBYyxFQUFnQixZQUFZLEVBQUUsWUFBWSxFQUFFLE1BQU0sK0JBQStCLENBQUE7QUFDbkssT0FBTyxFQUFFLGVBQWUsRUFBaUMsTUFBTSxzQkFBc0IsQ0FBQztBQUN0RixPQUFPLEVBQUUsYUFBYSxFQUFFLGdCQUFnQixFQUFFLE1BQU0sZ0NBQWdDLENBQUM7QUFtQmpGOzs7Ozs7Ozs7Ozs7Ozs7Ozs7OztHQW9CRztBQUNILE1BQU0sT0FBTyxXQUFZLFNBQVEsTUFBTSxDQUFDLE9BQU8sQ0FBQyxXQUFXO0lBQzFELElBQUksQ0FBYztJQUNWLE9BQU8sQ0FBVTtJQUN6QixFQUFFLENBQStCO0lBQ2pDLE1BQU0sQ0FBQyxpQkFBaUIsR0FBeUIsSUFBSSxDQUFDO0lBQ3RELElBQUksYUFBYTtRQUNoQixPQUFPLFdBQVcsQ0FBQyxpQkFBaUIsQ0FBQztJQUN0QyxDQUFDO0lBQ0QsY0FBYyxDQUEwQjtJQUN4QyxjQUFjLENBQThFO0lBQ3BGLGlCQUFpQixDQUF5QjtJQUMxQyxVQUFVLENBQXlCO0lBRTNDLFlBQWEsS0FBbUIsRUFBRSxhQUEyQyxFQUFFLFNBQWlCO1FBQy9GLEtBQUssQ0FBQyxLQUFLLEVBQUUsYUFBYSxFQUFFLFNBQVMsQ0FBQyxDQUFDO1FBQ3ZDLElBQUksQ0FBQyxJ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TyxDQUFDLFVBQVUsQ0FBQyxJQUFJLENBQUMsQ0FBQyxDQUFDO29CQUNqSSxJQUFJLENBQUMsSUFBSSxHQUFHO3dCQUNYLElBQUksRUFBRSxJQUFJLENBQUMsSUFBSTt3QkFDZixrQkFBa0IsRUFBRSxJQUFJLENBQUMsa0JBQWtCO3FCQUMzQyxDQUFDO29CQUNGLElBQUksQ0FBQyxVQUFVLEVBQUUsQ0FBQztnQkFDbkIsQ0FBQztZQUNGLENBQUM7UUFDRixDQUFDO0lBQ0YsQ0FBQztDQUNEIiwic291cmNlc0NvbnRlbnQiOlsiLyoqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKlxuICogU3BpbmUgUnVudGltZXMgTGljZW5zZSBBZ3JlZW1lbnRcbiAqIExhc3QgdXBkYXRlZCBKdWx5IDI4LCAyMDIzLiBSZXBsYWNlcyBhbGwgcHJpb3IgdmVyc2lvbnMuXG4gKlxuICogQ29weXJpZ2h0IChjKSAyMDEzLTIwMjMsIEVzb3RlcmljIFNvZnR3YXJlIExMQ1xuICpcbiAqIEludGVncmF0aW9uIG9mIHRoZSBTcGluZSBSdW50aW1lcyBpbnRvIHNvZnR3YXJlIG9yIG90aGVyd2lzZSBjcmVhdGluZ1xuICogZGVyaXZhdGl2ZSB3b3JrcyBvZiB0aGUgU3BpbmUgUnVudGltZXMgaXMgcGVybWl0dGVkIHVuZGVyIHRoZSB0ZXJtcyBhbmRcbiAqIGNvbmRpdGlvbnMgb2YgU2VjdGlvbiAyIG9mIHRoZSBTcGluZSBFZGl0b3IgTGljZW5zZSBBZ3JlZW1lbnQ6XG4gKiBodHRwOi8vZXNvdGVyaWNzb2Z0d2FyZS5jb20vc3BpbmUtZWRpdG9yLWxpY2Vuc2VcbiAqXG4gKiBPdGhlcndpc2UsIGl0IGlzIHBlcm1pdHRlZCB0byBpbnRlZ3JhdGUgdGhlIFNwaW5lIFJ1bnRpbWVzIGludG8gc29mdHdhcmUgb3JcbiAqIG90aGVyd2lzZSBjcmVhdGUgZGVyaXZhdGl2ZSB3b3JrcyBvZiB0aGUgU3BpbmUgUnVudGltZXMgKGNvbGxlY3RpdmVseSxcbiAqIFwiUHJvZHVjdHNcIiksIHByb3ZpZGVkIHRoYXQgZWFjaCB1c2VyIG9mIHRoZSBQcm9kdWN0cyBtdXN0IG9idGFpbiB0aGVpciBvd25cbiAqIFNwaW5lIEVkaXRvciBsaWNlbnNlIGFuZCByZWRpc3RyaWJ1dGlvbiBvZiB0aGUgUHJvZHVjdHMgaW4gYW55IGZvcm0gbXVzdFxuICogaW5jbHVkZSB0aGlzIGxpY2Vuc2UgYW5kIGNvcHlyaWdodCBub3RpY2UuXG4gKlxuICogVEhFIFNQSU5FIFJVTlRJTUVTIEFSRSBQUk9WSURFRCBCWSBFU09URVJJQyBTT0ZUV0FSRSBMTEMgXCJBUyBJU1wiIEFORCBBTllcbiAqIEVYUFJFU1MgT1IgSU1QTElFRCBXQVJSQU5USUVTLCBJTkNMVURJTkcsIEJVVCBOT1QgTElNSVRFRCBUTywgVEhFIElNUExJRURcbiAqIFdBUlJBTlRJRVMgT0YgTUVSQ0hBTlRBQklMSVRZIEFORCBGSVRORVNTIEZPUiBBIFBBUlRJQ1VMQVIgUFVSUE9TRSBBUkVcbiAqIERJU0NMQUlNRUQuIElOIE5PIEVWRU5UIFNIQUxMIEVTT1RFUklDIFNPRlRXQVJFIExMQyBCRSBMSUFCTEUgRk9SIEFOWVxuICogRElSRUNULCBJTkRJUkVDVCwgSU5DSURFTlRBTCwgU1BFQ0lBTCwgRVhFTVBMQVJZLCBPUiBDT05TRVFVRU5USUFMIERBTUFHRVNcbiAqIChJTkNMVURJTkcsIEJVVCBOT1QgTElNSVRFRCBUTywgUFJPQ1VSRU1FTlQgT0YgU1VCU1RJVFVURSBHT09EUyBPUiBTRVJWSUNFUyxcbiAqIEJVU0lORVNTIElOVEVSUlVQVElPTiwgT1IgTE9TUyBPRiBVU0UsIERBVEEsIE9SIFBST0ZJVFMpIEhPV0VWRVIgQ0FVU0VEIEFORFxuICogT04gQU5ZIFRIRU9SWSBPRiBMSUFCSUxJVFksIFdIRVRIRVIgSU4gQ09OVFJBQ1QsIFNUUklDVCBMSUFCSUxJVFksIE9SIFRPUlRcbiAqIChJTkNMVURJTkcgTkVHTElHRU5DRSBPUiBPVEhFUldJU0UpIEFSSVNJTkcgSU4gQU5ZIFdBWSBPVVQgT0YgVEhFIFVTRSBPRiBUSEVcbiAqIFNQSU5FIFJVTlRJTUVTLCBFVkVOIElGIEFEVklTRUQgT0YgVEhFIFBPU1NJQklMSVRZIE9GIFNVQ0ggREFNQUdFLlxuICoqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqL1xuXG5pbXBvcnQgKiBhcyBQaGFzZXIgZnJvbSBcInBoYXNlclwiO1xuaW1wb3J0IHsgU1BJTkVfQVRMQVNfQ0FDSEVfS0VZLCBTUElORV9HQU1FX09CSkVDVF9UWVBFLCBTUElORV9TS0VMRVRPTl9EQVRBX0ZJTEVfVFlQRSwgU1BJTkVfQVRMQVNfRklMRV9UWVBFLCBTUElORV9TS0VMRVRPTl9GSUxFX0NBQ0hFX0tFWSBhcyBTUElORV9TS0VMRVRPTl9EQVRBX0NBQ0hFX0tFWSB9IGZyb20gXCIuL2tleXMuanNcIjtcbmltcG9ydCB7IEF0bGFzQXR0YWNobWVudExvYWRlciwgR0xUZXh0dXJlLCBTY2VuZVJlbmRlcmVyLCBTa2VsZXRvbiwgU2tlbGV0b25CaW5hcnksIFNrZWxldG9uRGF0YSwgU2tlbGV0b25Kc29uLCBUZXh0dXJlQXRsYXMgfSBmcm9tIFwiQGVzb3Rlcmljc29mdHdhcmUvc3BpbmUtd2ViZ2xcIlxuaW1wb3J0IHsgU3BpbmVHYW1lT2JqZWN0LCBTcGluZUdhbWVPYmplY3RCb3VuZHNQcm92aWRlciB9IGZyb20gXCIuL1NwaW5lR2FtZU9iamVjdC5qc1wiO1xuaW1wb3J0IHsgQ2FudmFzVGV4dHVyZSwgU2tlbGV0b25SZW5kZXJlciB9IGZyb20gXCJAZXNvdGVyaWNzb2Z0d2FyZS9zcGluZS1jYW52YXNcIjtcblxuLyoqXG4gKiBDb25maWd1cmF0aW9uIG9iamVjdCB1c2VkIHdoZW4gY3JlYXRpbmcge0BsaW5rIFNwaW5lR2FtZU9iamVjdH0gaW5zdGFuY2VzIHZpYSBhIHNjZW5lJ3NcbiAqIHtAbGluayBHYW1lT2JqZWN0Q3JlYXRvcn0gKGBTY2VuZS5tYWtlYCkuXG4gKi9cbmV4cG9ydCBpbnRlcmZhY2UgU3BpbmVHYW1lT2JqZWN0Q29uZmlnIGV4dGVuZHMgUGhhc2VyLlR5cGVzLkdhbWVPYmplY3RzLkdhbWVPYmplY3RDb25maWcge1xuXHQvKiogVGhlIHgtcG9zaXRpb24gb2YgdGhlIG9iamVjdCwgb3B0aW9uYWwsIGRlZmF1bHQ6IDAgKi9cblx0eD86IG51bWJlcixcblx0LyoqIFRoZSB5LXBvc2l0aW9uIG9mIHRoZSBvYmplY3QsIG9wdGlvbmFsLCBkZWZhdWx0OiAwICovXG5cdHk/OiBudW1iZXIsXG5cdC8qKiBUaGUgc2tlbGV0b24gZGF0YSBrZXkgKi9cblx0ZGF0YUtleTogc3RyaW5nLFxuXHQvKiogVGhlIGF0bGFzIGtleSAqL1xuXHRhdGxhc0tleTogc3RyaW5nXG5cdC8qKiBUaGUgYm91bmRzIHByb3ZpZGVyLCBvcHRpb25hbCwgZGVmYXVsdDogYFNldHVwUG9zZUJvdW5kc1Byb3ZpZGVyYCAqL1xuXHRib3VuZHNQcm92aWRlcj86IFNwaW5lR2FtZU9iamVjdEJvdW5kc1Byb3ZpZGVyXG59XG5cbi8qKlxuICoge0BsaW5rIFNjZW5lUGx1Z2lufSBpbXBsZW1lbnRhdGlvbiBhZGRpbmcgU3BpbmUgUnVudGltZSBjYXBhYmlsaXRpZXMgdG8gYSBzY2VuZS5cbiAqXG4gKiBUaGUgc2NlbmUncyB7QGxpbmsgTG9hZGVyUGx1Z2lufSAoYFNjZW5lLmxvYWRgKSBnZXRzIHRoZXNlIGFkZGl0aW9uYWwgZnVuY3Rpb25zOlxuICogKiBgc3BpbmVCaW5hcnkoa2V5OiBzdHJpbmcsIHVybDogc3RyaW5nLCB4aHJTZXR0aW5ncz86IFhIUlNldHRpbmdzT2JqZWN0KWA6IGxvYWRzIGEgc2tlbGV0b24gYmluYXJ5IGAuc2tlbGAgZmlsZSBmcm9tIHRoZSBgdXJsYC5cbiAqICogYHNwaW5lSnNvbihrZXk6IHN0cmluZywgdXJsOiBzdHJpbmcsIHhoclNldHRpbmdzPzogWEhSU2V0dGluZ3NPYmplY3QpYDogbG9hZHMgYSBza2VsZXRvbiBiaW5hcnkgYC5za2VsYCBmaWxlIGZyb20gdGhlIGB1cmxgLlxuICogKiBgc3BpbmVBdGxhcyhrZXk6IHN0cmluZywgdXJsOiBzdHJpbmcsIHByZW11bHRpcGxpZWRBbHBoYTogYm9vbGVhbiA9IHRydWUsIHhoclNldHRpbmdzPzogWEhSU2V0dGluZ3NPYmplY3QpYDogbG9hZHMgYSB0ZXh0dXJlIGF0bGFzIGAuYXRsYXNgIGZpbGUgZnJvbSB0aGUgYHVybGAgYXMgd2VsbCBhcyBpdHMgY29ycmVwb25kaW5nIHRleHR1cmUgYXRsYXMgcGFnZSBpbWFnZXMuXG4gKlxuICogVGhlIHNjZW5lJ3Mge0BsaW5rIEdhbWVPYmplY3RGYWN0b3J5fSAoYFNjZW5lLmFkZGApIGdldHMgdGhlc2UgYWRkaXRpb25hbCBmdW5jdGlvbnM6XG4gKiAqIGBzcGluZSh4OiBudW1iZXIsIHk6IG51bWJlciwgZGF0YUtleTogc3RyaW5nLCBhdGxhc0tleTogc3RyaW5nLCBib3VuZHNQcm92aWRlcjogU3BpbmVHYW1lT2JqZWN0Qm91bmRzUHJvdmlkZXIgPSBTZXR1cFBvc2VCb3VuZHNQcm92aWRlcigpKWA6XG4gKiAgICBjcmVhdGVzIGEgbmV3IHtAbGluayBTcGluZUdhbWVPYmplY3R9IGZyb20gdGhlIGRhdGEgYW5kIGF0bGFzIGF0IHBvc2l0aW9uIGAoeCwgeSlgLCB1c2luZyB0aGUge0BsaW5rIEJvdW5kc1Byb3ZpZGVyfSB0byBjYWxjdWxhdGUgaXRzIGJvdW5kaW5nIGJveC4gVGhlIG9iamVjdCBpcyBhdXRvbWF0aWNhbGx5IGFkZGVkIHRvIHRoZSBzY2VuZS5cbiAqXG4gKiBUaGUgc2NlbmUncyB7QGxpbmsgR2FtZU9iamVjdENyZWF0b3J9IChgU2NlbmUubWFrZWApIGdldHMgdGhlc2UgYWRkaXRpb25hbCBmdW5jdGlvbnM6XG4gKiAqIGBzcGluZShjb25maWc6IFNwaW5lR2FtZU9iamVjdENvbmZpZylgOiBjcmVhdGVzIGEgbmV3IHtAbGluayBTcGluZUdhbWVPYmplY3R9IGZyb20gdGhlIGdpdmVuIGNvbmZpZ3VyYXRpb24gb2JqZWN0LlxuICpcbiAqIFRoZSBwbHVnaW4gaGFzIGFkZGl0aW9uYWwgcHVibGljIG1ldGhvZHMgdG8gd29yayB3aXRoIFNwaW5lIFJ1bnRpbWUgY29yZSBBUEkgb2JqZWN0czpcbiAqICogYGdldEF0bGFzKGF0bGFzS2V5OiBzdHJpbmcpYDogcmV0dXJucyB0aGUge0BsaW5rIFRleHR1cmVBdGxhc30gaW5zdGFuY2UgZm9yIHRoZSBnaXZlbiBhdGxhcyBrZXkuXG4gKiAqIGBnZXRTa2VsZXRvbkRhdGEoc2tlbGV0b25EYXRhS2V5OiBzdHJpbmcpYDogcmV0dXJucyB0aGUge0BsaW5rIFNrZWxldG9uRGF0YX0gaW5zdGFuY2UgZm9yIHRoZSBnaXZlbiBza2VsZXRvbiBkYXRhIGtleS5cbiAqICogYGNyZWF0ZVNrZWxldG9uKHNrZWxldG9uRGF0YUtleTogc3RyaW5nLCBhdGxhc0tleTogc3RyaW5nLCBwcmVtdWx0aXBsaWVkQWxwaGE6IGJvb2xlYW4gPSB0cnVlKWA6IGNyZWF0ZXMgYSBuZXcge0BsaW5rIFNrZWxldG9ufSBpbnN0YW5jZSBmcm9tIHRoZSBnaXZlbiBza2VsZXRvbiBkYXRhIGFuZCBhdGxhcyBrZXkuXG4gKiAqIGBpc1ByZW11bHRpcGxpZWRBbHBoYShhdGxhc0tleTogc3RyaW5nKWA6IHJldHVybnMgYHRydWVgIGlmIHRoZSBhdGxhcyB3aXRoIHRoZSBnaXZlbiBrZXkgaGFzIHByZW11bHRpcGxpZWQgYWxwaGEuXG4gKi9cbmV4cG9ydCBjbGFzcyBTcGluZVBsdWdpbiBleHRlbmRzIFBoYXNlci5QbHVnaW5zLlNjZW5lUGx1Z2luIHtcblx0Z2FtZTogUGhhc2VyLkdhbWU7XG5cdHByaXZhdGUgaXNXZWJHTDogYm9vbGVhbjtcblx0Z2w6IFdlYkdMUmVuZGVyaW5nQ29udGV4dCB8IG51bGw7XG5cdHN0YXRpYyBnYW1lV2ViR0xSZW5kZXJlcjogU2NlbmVSZW5kZXJlciB8IG51bGwgPSBudWxsO1xuXHRnZXQgd2ViR0xSZW5kZXJlciAoKTogU2NlbmVSZW5kZXJlciB8IG51bGwge1xuXHRcdHJldHVybiBTcGluZVBsdWdpbi5nYW1lV2ViR0xSZW5kZXJlcjtcblx0fVxuXHRjYW52YXNSZW5kZXJlcjogU2tlbGV0b25SZW5kZXJlciB8IG51bGw7XG5cdHBoYXNlclJlbmRlcmVyOiBQaGFzZXIuUmVuZGVyZXIuQ2FudmFzLkNhbnZhc1JlbmRlcmVyIHwgUGhhc2VyLlJlbmRlcmVyLldlYkdMLldlYkdMUmVuZGVyZXI7XG5cdHByaXZhdGUgc2tlbGV0b25EYXRhQ2FjaGU6IFBoYXNlci5DYWNoZS5CYXNlQ2FjaGU7XG5cdHByaXZhdGUgYXRsYXNDYWNoZTogUGhhc2VyLkNhY2hlLkJhc2VDYWNoZTtcblxuXHRjb25zdHJ1Y3RvciAoc2NlbmU6IFBoYXNlci5TY2VuZSwgcGx1Z2luTWFuYWdlcjogUGhhc2VyLlBsdWdpbnMuUGx1Z2luTWFuYWdlciwgcGx1Z2luS2V5OiBzdHJpbmcpIHtcblx0XHRzdXBlcihzY2VuZSwgcGx1Z2luTWFuYWdlciwgcGx1Z2luS2V5KTtcblx0XHR0aGlzLmdhbWUgPSBwbHVnaW5NYW5hZ2VyLmdhbWU7XG5cdFx0dGhpcy5pc1dlYkdMID0gdGhpcy5nYW1lLmNvbmZpZy5yZW5kZXJUeXBlID09PSAyO1xuXHRcdHRoaXMuZ2wgPSB0aGlzLmlzV2ViR0wgPyAodGhpcy5nYW1lLnJlbmRlcmVyIGFzIFBoYXNlci5SZW5kZXJlci5XZWJHTC5XZWJHTFJlbmRlcmVyKS5nbCA6IG51bGw7XG5cdFx0dGhpcy5waGFzZXJSZW5kZXJlciA9IHRoaXMuZ2FtZS5yZW5kZXJlcjtcblx0XHR0aGlzLmNhbnZhc1JlbmRlcmVyID0gbnVsbDtcblx0XHR0aGlzLnNrZWxldG9uRGF0YUNhY2hlID0gdGhpcy5nYW1lLmNhY2hlLmFkZEN1c3RvbShTUElORV9TS0VMRVRPTl9EQVRBX0NBQ0hFX0tFWSk7XG5cdFx0dGhpcy5hdGxhc0NhY2hlID0gdGhpcy5nYW1lLmNhY2hlLmFkZEN1c3RvbShTUElORV9BVExBU19DQUNIRV9LRVkpO1xuXG5cdFx0bGV0IHNrZWxldG9uSnNvbkZpbGVDYWxsYmFjayA9IGZ1bmN0aW9uICh0aGlzOiBhbnksIGtleTogc3RyaW5nLFxuXHRcdFx0dXJsOiBzdHJpbmcsXG5cdFx0XHR4aHJTZXR0aW5nczogUGhhc2VyLlR5cGVzLkxvYWRlci5YSFJTZXR0aW5nc09iamVjdCkge1xuXHRcdFx0bGV0IGZpbGUgPSBuZXcgU3BpbmVTa2VsZXRvbkRhdGFGaWxlKHRoaXMgYXMgYW55LCBrZXksIHVybCwgU3BpbmVTa2VsZXRvbkRhdGFGaWxlVHlwZS5qc29uLCB4aHJTZXR0aW5ncyk7XG5cdFx0XHR0aGlzLmFkZEZpbGUoZmlsZS5maWxlcyk7XG5cdFx0XHRyZXR1cm4gdGhpcztcblx0XHR9O1xuXHRcdHBsdWdpbk1hbmFnZXIucmVnaXN0ZXJGaWxlVHlwZShcInNwaW5lSnNvblwiLCBza2VsZXRvbkpzb25GaWxlQ2FsbGJhY2ssIHNjZW5lKTtcblxuXHRcdGxldCBza2VsZXRvbkJpbmFyeUZpbGVDYWxsYmFjayA9IGZ1bmN0aW9uICh0aGlzOiBhbnksIGtleTogc3RyaW5nLFxuXHRcdFx0dXJsOiBzdHJpbmcsXG5cdFx0XHR4aHJTZXR0aW5nczogUGhhc2VyLlR5cGVzLkxvYWRlci5YSFJTZXR0aW5nc09iamVjdCkge1xuXHRcdFx0bGV0IGZpbGUgPSBuZXcgU3BpbmVTa2VsZXRvbkRhdGFGaWxlKHRoaXMgYXMgYW55LCBrZXksIHVybCwgU3BpbmVTa2VsZXRvbkRhdGFGaWxlVHlwZS5iaW5hcnksIHhoclNldHRpbmdzKTtcblx0XHRcdHRoaXMuYWRkRmlsZShmaWxlLmZpbGVzKTtcblx0XHRcdHJldHVybiB0aGlzO1xuXHRcdH07XG5cdFx0cGx1Z2luTWFuYWdlci5yZWdpc3RlckZpbGVUeXBlKFwic3BpbmVCaW5hcnlcIiwgc2tlbGV0b25CaW5hcnlGaWxlQ2FsbGJhY2ssIHNjZW5lKTtcblxuXHRcdGxldCBhdGxhc0ZpbGVDYWxsYmFjayA9IGZ1bmN0aW9uICh0aGlzOiBhbnksIGtleTogc3RyaW5nLFxuXHRcdFx0dXJsOiBzdHJpbmcsXG5cdFx0XHRwcmVtdWx0aXBsaWVkQWxwaGE6IGJvb2xlYW4sXG5cdFx0XHR4aHJTZXR0aW5nczogUGhhc2VyLlR5cGVzLkxvYWRlci5YSFJTZXR0aW5nc09iamVjdCkge1xuXHRcdFx0bGV0IGZpbGUgPSBuZXcgU3BpbmVBdGxhc0ZpbGUodGhpcyBhcyBhbnksIGtleSwgdXJsLCBwcmVtdWx0aXBsaWVkQWxwaGEsIHhoclNldHRpbmdzKTtcblx0XHRcdHRoaXMuYWRkRmlsZShmaWxlLmZpbGVzKTtcblx0XHRcdHJldHVybiB0aGlzO1xuXHRcdH07XG5cdFx0cGx1Z2luTWFuYWdlci5yZWdpc3RlckZpbGVUeXBlKFwic3BpbmVBdGxhc1wiLCBhdGxhc0ZpbGVDYWxsYmFjaywgc2NlbmUpO1xuXG5cdFx0bGV0IGFkZFNwaW5lR2FtZU9iamVjdCA9IGZ1bmN0aW9uICh0aGlzOiBQaGFzZXIuR2FtZU9iamVjdHMuR2FtZU9iamVjdEZhY3RvcnksIHg6IG51bWJlciwgeTogbnVtYmVyLCBkYXRhS2V5OiBzdHJpbmcsIGF0bGFzS2V5OiBzdHJpbmcsIGJvdW5kc1Byb3ZpZGVyOiBTcGluZUdhbWVPYmplY3RCb3VuZHNQcm92aWRlcikge1xuXHRcdFx0aWYgKHRoaXMuc2NlbmUuc3lzLnJlbmRlcmVyIGluc3RhbmNlb2YgUGhhc2VyLlJlbmRlcmVyLldlYkdMLldlYkdMUmVuZGVyZXIpIHtcblx0XHRcdFx0dGhpcy5zY2VuZS5zeXMucmVuZGVyZXIucGlwZWxpbmVzLmNsZWFyKCk7XG5cdFx0XHR9XG5cblx0XHRcdGNvbnN0IHNwaW5lUGx1Z2luID0gKHRoaXMuc2NlbmUuc3lzIGFzIGFueSlbcGx1Z2luS2V5XSBhcyBTcGluZVBsdWdpbjtcblx0XHRcdGxldCBnYW1lT2JqZWN0ID0gbmV3IFNwaW5lR2FtZU9iamVjdCh0aGlzLnNjZW5lLCBzcGluZVBsdWdpbiwgeCwgeSwgZGF0YUtleSwgYXRsYXNLZXksIGJvdW5kc1Byb3ZpZGVyKTtcblx0XHRcdHRoaXMuZGlzcGxheUxpc3QuYWRkKGdhbWVPYmplY3QpO1xuXHRcdFx0dGhpcy51cGRhdGVMaXN0LmFkZChnYW1lT2JqZWN0KTtcblxuXHRcdFx0aWYgKHRoaXMuc2NlbmUuc3lzLnJlbmRlcmVyIGluc3RhbmNlb2YgUGhhc2VyLlJlbmRlcmVyLldlYkdMLldlYkdMUmVuZGVyZXIpIHtcblx0XHRcdFx0dGhpcy5zY2VuZS5zeXMucmVuZGVyZXIucGlwZWxpbmVzLnJlYmluZCgpO1xuXHRcdFx0fVxuXG5cdFx0XHRyZXR1cm4gZ2FtZU9iamVjdDtcblx0XHR9O1xuXG5cdFx0bGV0IG1ha2VTcGluZUdhbWVPYmplY3QgPSBmdW5jdGlvbiAodGhpczogUGhhc2VyLkdhbWVPYmplY3RzLkdhbWVPYmplY3RGYWN0b3J5LCBjb25maWc6IFNwaW5lR2FtZU9iamVjdENvbmZpZywgYWRkVG9TY2VuZTogYm9vbGVhbiA9IGZhbHNlKSB7XG5cdFx0XHRpZiAodGhpcy5zY2VuZS5zeXMucmVuZGVyZXIgaW5zdGFuY2VvZiBQaGFzZXIuUmVuZGVyZXIuV2ViR0wuV2ViR0xSZW5kZXJlcikge1xuXHRcdFx0XHR0aGlzLnNjZW5lLnN5cy5yZW5kZXJlci5waXBlbGluZXMuY2xlYXIoKTtcblx0XHRcdH1cblxuXHRcdFx0bGV0IHggPSBjb25maWcueCA/IGNvbmZpZy54IDogMDtcblx0XHRcdGxldCB5ID0gY29uZmlnLnkgPyBjb25maWcueSA6IDA7XG5cdFx0XHRsZXQgYm91bmRzUHJvdmlkZXIgPSBjb25maWcuYm91bmRzUHJvdmlkZXIgPyBjb25maWcuYm91bmRzUHJvdmlkZXIgOiB1bmRlZmluZWQ7XG5cblx0XHRcdGNvbnN0IHNwaW5lUGx1Z2luID0gKHRoaXMuc2NlbmUuc3lzIGFzIGFueSlbcGx1Z2luS2V5XSBhcyBTcGluZVBsdWdpbjtcblx0XHRcdGxldCBnYW1lT2JqZWN0ID0gbmV3IFNwaW5lR2FtZU9iamVjdCh0aGlzLnNjZW5lLCBzcGluZVBsdWdpbiwgeCwgeSwgY29uZmlnLmRhdGFLZXksIGNvbmZpZy5hdGxhc0tleSwgYm91bmRzUHJvdmlkZXIpO1xuXHRcdFx0aWYgKGFkZFRvU2NlbmUgIT09IHVuZGVmaW5lZCkge1xuXHRcdFx0XHRjb25maWcuYWRkID0gYWRkVG9TY2VuZTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHRoaXMuc2NlbmUuc3lzLnJlbmRlcmVyIGluc3RhbmNlb2YgUGhhc2VyLlJlbmRlcmVyLldlYkdMLldlYkdMUmVuZGVyZXIpIHtcblx0XHRcdFx0dGhpcy5zY2VuZS5zeXMucmVuZGVyZXIucGlwZWxpbmVzLnJlYmluZCgpO1xuXHRcdFx0fVxuXG5cdFx0XHRyZXR1cm4gUGhhc2VyLkdhbWVPYmplY3RzLkJ1aWxkR2FtZU9iamVjdCh0aGlzLnNjZW5lLCBnYW1lT2JqZWN0LCBjb25maWcpO1xuXHRcdH1cblx0XHRwbHVnaW5NYW5hZ2VyLnJlZ2lzdGVyR2FtZU9iamVjdCgod2luZG93IGFzIGFueSkuU1BJTkVfR0FNRV9PQkpFQ1RfVFlQRSA/ICh3aW5kb3cgYXMgYW55KS5TUElORV9HQU1FX09CSkVDVF9UWVBFIDogU1BJTkVfR0FNRV9PQkpFQ1RfVFlQRSwgYWRkU3BpbmVHYW1lT2JqZWN0LCBtYWtlU3BpbmVHYW1lT2JqZWN0KTtcblx0fVxuXG5cdHN0YXRpYyByZW5kZXJlcklkID0gMDtcblx0Ym9vdCAoKSB7XG5cdFx0U2tlbGV0b24ueURvd24gPSB0cnVlO1xuXHRcdGlmICh0aGlzLmlzV2ViR0wpIHtcblx0XHRcdGlmICghU3BpbmVQbHVnaW4uZ2FtZVdlYkdMUmVuZGVyZXIpIHtcblx0XHRcdFx0U3BpbmVQbHVnaW4uZ2FtZVdlYkdMUmVuZGVyZXIgPSBuZXcgU2NlbmVSZW5kZXJlcigodGhpcy5nYW1lLnJlbmRlcmVyISBhcyBQaGFzZXIuUmVuZGVyZXIuV2ViR0wuV2ViR0xSZW5kZXJlcikuY2FudmFzLCB0aGlzLmdsISwgdHJ1ZSk7XG5cdFx0XHR9XG5cdFx0XHR0aGlzLm9uUmVzaXplKCk7XG5cdFx0XHR0aGlzLmdhbWUuc2NhbGUub24oUGhhc2VyLlNjYWxlLkV2ZW50cy5SRVNJWkUsIHRoaXMub25SZXNpemUsIHRoaXMpO1xuXHRcdH0gZWxzZSB7XG5cdFx0XHRpZiAoIXRoaXMuY2FudmFzUmVuZGVyZXIpIHtcblx0XHRcdFx0dGhpcy5jYW52YXNSZW5kZXJlciA9IG5ldyBTa2VsZXRvblJlbmRlcmVyKHRoaXMuc2NlbmUhLnN5cy5jb250ZXh0KTtcblx0XHRcdH1cblx0XHR9XG5cblx0XHR2YXIgZXZlbnRFbWl0dGVyID0gdGhpcy5zeXN0ZW1zIS5ldmVudHM7XG5cdFx0ZXZlbnRFbWl0dGVyLm9uY2UoJ3NodXRkb3duJywgdGhpcy5zaHV0ZG93biwgdGhpcyk7XG5cdFx0ZXZlbnRFbWl0dGVyLm9uY2UoJ2Rlc3Ryb3knLCB0aGlzLmRlc3Ryb3ksIHRoaXMpO1xuXHRcdHRoaXMuZ2FtZS5ldmVudHMub25jZSgnZGVzdHJveScsIHRoaXMuZ2FtZURlc3Ryb3ksIHRoaXMpO1xuXHR9XG5cblx0b25SZXNpemUgKCkge1xuXHRcdHZhciBwaGFzZXJSZW5kZXJlciA9IHRoaXMuZ2FtZS5yZW5kZXJlcjtcblx0XHR2YXIgc2NlbmVSZW5kZXJlciA9IHRoaXMud2ViR0xSZW5kZXJlcjtcblxuXHRcdGlmIChwaGFzZXJSZW5kZXJlciAmJiBzY2VuZVJlbmRlcmVyKSB7XG5cdFx0XHR2YXIgdmlld3BvcnRXaWR0aCA9IHBoYXNlclJlbmRlcmVyLndpZHRoO1xuXHRcdFx0dmFyIHZpZXdwb3J0SGVpZ2h0ID0gcGhhc2VyUmVuZGVyZXIuaGVpZ2h0O1xuXHRcdFx0c2NlbmVSZW5kZXJlci5jYW1lcmEucG9zaXRpb24ueCA9IHZpZXdwb3J0V2lkdGggLyAyO1xuXHRcdFx0c2NlbmVSZW5kZXJlci5jYW1lcmEucG9zaXRpb24ueSA9IHZpZXdwb3J0SGVpZ2h0IC8gMjtcblx0XHRcdHNjZW5lUmVuZGVyZXIuY2FtZXJhLnVwLnkgPSAtMTtcblx0XHRcdHNjZW5lUmVuZGVyZXIuY2FtZXJhLmRpcmVjdGlvbi56ID0gMTtcblx0XHRcdHNjZW5lUmVuZGVyZXIuY2FtZXJhLnNldFZpZXdwb3J0KHZpZXdwb3J0V2lkdGgsIHZpZXdwb3J0SGVpZ2h0KTtcblx0XHR9XG5cdH1cblxuXHRzaHV0ZG93biAoKSB7XG5cdFx0dGhpcy5zeXN0ZW1zIS5ldmVudHMub2ZmKFwic2h1dGRvd25cIiwgdGhpcy5zaHV0ZG93biwgdGhpcyk7XG5cdFx0aWYgKHRoaXMuaXNXZWJHTCkge1xuXHRcdFx0dGhpcy5nYW1lLnNjYWxlLm9mZihQaGFzZXIuU2NhbGUuRXZlbnRzLlJFU0laRSwgdGhpcy5vblJlc2l6ZSwgdGhpcyk7XG5cdFx0fVxuXHR9XG5cblx0ZGVzdHJveSAoKSB7XG5cdFx0dGhpcy5zaHV0ZG93bigpXG5cdH1cblxuXHRnYW1lRGVzdHJveSAoKSB7XG5cdFx0dGhpcy5wbHVnaW5NYW5hZ2VyLnJlbW92ZUdhbWVPYmplY3QoKHdpbmRvdyBhcyBhbnkpLlNQSU5FX0dBTUVfT0JKRUNUX1RZUEUgPyAod2luZG93IGFzIGFueSkuU1BJTkVfR0FNRV9PQkpFQ1RfVFlQRSA6IFNQSU5FX0dBTUVfT0JKRUNUX1RZUEUsIHRydWUsIHRydWUpO1xuXHRcdGlmICh0aGlzLndlYkdMUmVuZGVyZXIpIHRoaXMud2ViR0xSZW5kZXJlci5kaXNwb3NlKCk7XG5cdFx0U3BpbmVQbHVnaW4uZ2FtZVdlYkdMUmVuZGVyZXIgPSBudWxsO1xuXHR9XG5cblx0LyoqIFJldHVybnMgdGhlIFRleHR1cmVBdGxhcyBpbnN0YW5jZSBmb3IgdGhlIGdpdmVuIGtleSAqL1xuXHRnZXRBdGxhcyAoYXRsYXNLZXk6IHN0cmluZykge1xuXHRcdGxldCBhdGxhczogVGV4dHVyZUF0bGFzO1xuXHRcdGlmICh0aGlzLmF0bGFzQ2FjaGUuZXhpc3RzKGF0bGFzS2V5KSkge1xuXHRcdFx0YXRsYXMgPSB0aGlzLmF0bGFzQ2FjaGUuZ2V0KGF0bGFzS2V5KTtcblx0XHR9IGVsc2Uge1xuXHRcdFx0bGV0IGF0bGFzRmlsZSA9IHRoaXMuZ2FtZS5jYWNoZS50ZXh0LmdldChhdGxhc0tleSkgYXMgeyBkYXRhOiBzdHJpbmcsIHByZW11bHRpcGxpZWRBbHBoYTogYm9vbGVhbiB9O1xuXHRcdFx0YXRsYXMgPSBuZXcgVGV4dHVyZUF0bGFzKGF0bGFzRmlsZS5kYXRhKTtcblx0XHRcdGlmICh0aGlzLmlzV2ViR0wpIHtcblx0XHRcdFx0bGV0IGdsID0gdGhpcy5nbCE7XG5cdFx0XHRcdGNvbnN0IHBoYXNlclVucGFja1BtYVZhbHVlID0gZ2wuZ2V0UGFyYW1ldGVyKGdsLlVOUEFDS19QUkVNVUxUSVBMWV9BTFBIQV9XRUJHTCk7XG5cdFx0XHRcdGlmIChwaGFzZXJVbnBhY2tQbWFWYWx1ZSkgZ2wucGl4ZWxTdG9yZWkoZ2wuVU5QQUNLX1BSRU1VTFRJUExZX0FMUEhBX1dFQkdMLCBmYWxzZSk7XG5cdFx0XHRcdGZvciAobGV0IGF0bGFzUGFnZSBvZiBhdGxhcy5wYWdlcykge1xuXHRcdFx0XHRcdGF0bGFzUGFnZS5zZXRUZXh0dXJlKG5ldyBHTFRleHR1cmUoZ2wsIHRoaXMuZ2FtZS50ZXh0dXJlcy5nZXQoYXRsYXNLZXkgKyBcIiFcIiArIGF0bGFzUGFnZS5uYW1lKS5nZXRTb3VyY2VJbWFnZSgpIGFzIEhUTUxJbWFnZUVsZW1lbnQgfCBJbWFnZUJpdG1hcCwgZmFsc2UpKTtcblx0XHRcdFx0fVxuXHRcdFx0XHRpZiAocGhhc2VyVW5wYWNrUG1hVmFsdWUpIGdsLnBpeGVsU3RvcmVpKGdsLlVOUEFDS19QUkVNVUxUSVBMWV9BTFBIQV9XRUJHTCwgdHJ1ZSk7XG5cdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHRmb3IgKGxldCBhdGxhc1BhZ2Ugb2YgYXRsYXMucGFnZXMpIHtcblx0XHRcdFx0XHRhdGxhc1BhZ2Uuc2V0VGV4dHVyZShuZXcgQ2FudmFzVGV4dHVyZSh0aGlzLmdhbWUudGV4dHVyZXMuZ2V0KGF0bGFzS2V5ICsgXCIhXCIgKyBhdGxhc1BhZ2UubmFtZSkuZ2V0U291cmNlSW1hZ2UoKSBhcyBIVE1MSW1hZ2VFbGVtZW50IHwgSW1hZ2VCaXRtYXApKTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdFx0dGhpcy5hdGxhc0NhY2hlLmFkZChhdGxhc0tleSwgYXRsYXMpO1xuXHRcdH1cblx0XHRyZXR1cm4gYXRsYXM7XG5cdH1cblxuXHQvKiogUmV0dXJucyB3aGV0aGVyIHRoZSBUZXh0dXJlQXRsYXMgdXNlcyBwcmVtdWx0aXBsaWVkIGFscGhhICovXG5cdGlzQXRsYXNQcmVtdWx0aXBsaWVkIChhdGxhc0tleTogc3RyaW5nKSB7XG5cdFx0bGV0IGF0bGFzRmlsZSA9IHRoaXMuZ2FtZS5jYWNoZS50ZXh0LmdldChhdGxhc0tleSk7XG5cdFx0aWYgKCFhdGxhc0ZpbGUpIHJldHVybiBmYWxzZTtcblx0XHRyZXR1cm4gYXRsYXNGaWxlLnByZW11bHRpcGxpZWRBbHBoYTtcblx0fVxuXG5cdC8qKiBSZXR1cm5zIHRoZSBTa2VsZXRvbkRhdGEgaW5zdGFuY2UgZm9yIHRoZSBnaXZlbiBkYXRhIGFuZCBhdGxhcyBrZXkgKi9cblx0Z2V0U2tlbGV0b25EYXRhIChkYXRhS2V5OiBzdHJpbmcsIGF0bGFzS2V5OiBzdHJpbmcpIHtcblx0XHRjb25zdCBhdGxhcyA9IHRoaXMuZ2V0QXRsYXMoYXRsYXNLZXkpXG5cdFx0Y29uc3QgY29tYmluZWRLZXkgPSBkYXRhS2V5ICsgYXRsYXNLZXk7XG5cdFx0bGV0IHNrZWxldG9uRGF0YTogU2tlbGV0b25EYXRhO1xuXHRcdGlmICh0aGlzLnNrZWxldG9uRGF0YUNhY2hlLmV4aXN0cyhjb21iaW5lZEtleSkpIHtcblx0XHRcdHNrZWxldG9uRGF0YSA9IHRoaXMuc2tlbGV0b25EYXRhQ2FjaGUuZ2V0KGNvbWJpbmVkS2V5KTtcblx0XHR9IGVsc2Uge1xuXHRcdFx0aWYgKHRoaXMuZ2FtZS5jYWNoZS5qc29uLmV4aXN0cyhkYXRhS2V5KSkge1xuXHRcdFx0XHRsZXQganNvbkZpbGUgPSB0aGlzLmdhbWUuY2FjaGUuanNvbi5nZXQoZGF0YUtleSkgYXMgYW55O1xuXHRcdFx0XHRsZXQganNvbiA9IG5ldyBTa2VsZXRvbkpzb24obmV3IEF0bGFzQXR0YWNobWVudExvYWRlcihhdGxhcykpO1xuXHRcdFx0XHRza2VsZXRvbkRhdGEgPSBqc29uLnJlYWRTa2VsZXRvbkRhdGEoanNvbkZpbGUpO1xuXHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0bGV0IGJpbmFyeUZpbGUgPSB0aGlzLmdhbWUuY2FjaGUuYmluYXJ5LmdldChkYXRhS2V5KSBhcyBBcnJheUJ1ZmZlcjtcblx0XHRcdFx0bGV0IGJpbmFyeSA9IG5ldyBTa2VsZXRvbkJpbmFyeShuZXcgQXRsYXNBdHRhY2htZW50TG9hZGVyKGF0bGFzKSk7XG5cdFx0XHRcdHNrZWxldG9uRGF0YSA9IGJpbmFyeS5yZWFkU2tlbGV0b25EYXRhKG5ldyBVaW50OEFycmF5KGJpbmFyeUZpbGUpKTtcblx0XHRcdH1cblx0XHRcdHRoaXMuc2tlbGV0b25EYXRhQ2FjaGUuYWRkKGNvbWJpbmVkS2V5LCBza2VsZXRvbkRhdGEpO1xuXHRcdH1cblx0XHRyZXR1cm4gc2tlbGV0b25EYXRhO1xuXHR9XG5cblx0LyoqIENyZWF0ZXMgYSBuZXcgU2tlbGV0b24gaW5zdGFuY2UgZnJvbSB0aGUgZGF0YSBhbmQgYXRsYXMuICovXG5cdGNyZWF0ZVNrZWxldG9uIChkYXRhS2V5OiBzdHJpbmcsIGF0bGFzS2V5OiBzdHJpbmcpIHtcblx0XHRyZXR1cm4gbmV3IFNrZWxldG9uKHRoaXMuZ2V0U2tlbGV0b25EYXRhKGRhdGFLZXksIGF0bGFzS2V5KSk7XG5cdH1cbn1cblxuZW51bSBTcGluZVNrZWxldG9uRGF0YUZpbGVUeXBlIHtcblx0anNvbixcblx0YmluYXJ5XG59XG5cbmludGVyZmFjZSBTcGluZVNrZWxldG9uRGF0YUZpbGVDb25maWcge1xuXHRrZXk6IHN0cmluZztcblx0dXJsOiBzdHJpbmc7XG5cdHR5cGU6IFwic3BpbmVKc29uXCIgfCBcInNwaW5lQmluYXJ5XCI7XG5cdHhoclNldHRpbmdzPzogUGhhc2VyLlR5cGVzLkxvYWRlci5YSFJTZXR0aW5nc09iamVjdFxufVxuXG5jbGFzcyBTcGluZVNrZWxldG9uRGF0YUZpbGUgZXh0ZW5kcyBQaGFzZXIuTG9hZGVyLk11bHRpRmlsZSB7XG5cdGNvbnN0cnVjdG9yIChsb2FkZXI6IFBoYXNlci5Mb2FkZXIuTG9hZGVyUGx1Z2luLCBrZXk6IHN0cmluZyB8IFNwaW5lU2tlbGV0b25EYXRhRmlsZUNvbmZpZywgdXJsPzogc3RyaW5nLCBwdWJsaWMgZmlsZVR5cGU/OiBTcGluZVNrZWxldG9uRGF0YUZpbGVUeXBlLCB4aHJTZXR0aW5ncz86IFBoYXNlci5UeXBlcy5Mb2FkZXIuWEhSU2V0dGluZ3NPYmplY3QpIHtcblx0XHRpZiAodHlwZW9mIGtleSAhPT0gXCJzdHJpbmdcIikge1xuXHRcdFx0Y29uc3QgY29uZmlnID0ga2V5O1xuXHRcdFx0a2V5ID0gY29uZmlnLmtleTtcblx0XHRcdHVybCA9IGNvbmZpZy51cmw7XG5cdFx0XHRmaWxlVHlwZSA9IGNvbmZpZy50eXBlID09PSBcInNwaW5lSnNvblwiID8gU3BpbmVTa2VsZXRvbkRhdGFGaWxlVHlwZS5qc29uIDogU3BpbmVTa2VsZXRvbkRhdGFGaWxlVHlwZS5iaW5hcnk7XG5cdFx0XHR4aHJTZXR0aW5ncyA9IGNvbmZpZy54aHJTZXR0aW5ncztcblx0XHR9XG5cdFx0bGV0IGZpbGUgPSBudWxsO1xuXHRcdGxldCBpc0pzb24gPSBmaWxlVHlwZSA9PSBTcGluZVNrZWxldG9uRGF0YUZpbGVUeXBlLmpzb247XG5cdFx0aWYgKGlzSnNvbikge1xuXHRcdFx0ZmlsZSA9IG5ldyBQaGFzZXIuTG9hZGVyLkZpbGVUeXBlcy5KU09ORmlsZShsb2FkZXIsIHtcblx0XHRcdFx0a2V5OiBrZXksXG5cdFx0XHRcdHVybDogdXJsLFxuXHRcdFx0XHRleHRlbnNpb246IFwianNvblwiLFxuXHRcdFx0XHR4aHJTZXR0aW5nczogeGhyU2V0dGluZ3MsXG5cdFx0XHR9IGFzIFBoYXNlci5UeXBlcy5Mb2FkZXIuRmlsZVR5cGVzLkpTT05GaWxlQ29uZmlnKTtcblx0XHR9IGVsc2Uge1xuXHRcdFx0ZmlsZSA9IG5ldyBQaGFzZXIuTG9hZGVyLkZpbGVUeXBlcy5CaW5hcnlGaWxlKGxvYWRlciwge1xuXHRcdFx0XHRrZXk6IGtleSxcblx0XHRcdFx0dXJsOiB1cmwsXG5cdFx0XHRcdGV4dGVuc2lvbjogXCJza2VsXCIsXG5cdFx0XHRcdHhoclNldHRpbmdzOiB4aHJTZXR0aW5ncyxcblx0XHRcdH0gYXMgUGhhc2VyLlR5cGVzLkxvYWRlci5GaWxlVHlwZXMuQmluYXJ5RmlsZUNvbmZpZyk7XG5cdFx0fVxuXHRcdHN1cGVyKGxvYWRlciwgU1BJTkVfU0tFTEVUT05fREFUQV9GSUxFX1RZUEUsIGtleSwgW2ZpbGVdKTtcblx0fVxuXG5cdG9uRmlsZUNvbXBsZXRlIChmaWxlOiBQaGFzZXIuTG9hZGVyLkZpbGUpIHtcblx0XHR0aGlzLnBlbmRpbmctLTtcblx0fVxuXG5cdGFkZFRvQ2FjaGUgKCkge1xuXHRcdGlmICh0aGlzLmlzUmVhZHlUb1Byb2Nlc3MoKSkgdGhpcy5maWxlc1swXS5hZGRUb0NhY2hlKCk7XG5cdH1cbn1cblxuaW50ZXJmYWNlIFNwaW5lQXRsYXNGaWxlQ29uZmlnIHtcblx0a2V5OiBzdHJpbmc7XG5cdHVybDogc3RyaW5nO1xuXHRwcmVtdWx0aXBsaWVkQWxwaGE/OiBib29sZWFuO1xuXHR4aHJTZXR0aW5ncz86IFBoYXNlci5UeXBlcy5Mb2FkZXIuWEhSU2V0dGluZ3NPYmplY3Q7XG59XG5cbmNsYXNzIFNwaW5lQXRsYXNGaWxlIGV4dGVuZHMgUGhhc2VyLkxvYWRlci5NdWx0aUZpbGUge1xuXHRjb25zdHJ1Y3RvciAobG9hZGVyOiBQaGFzZXIuTG9hZGVyLkxvYWRlclBsdWdpbiwga2V5OiBzdHJpbmcgfCBTcGluZUF0bGFzRmlsZUNvbmZpZywgdXJsPzogc3RyaW5nLCBwdWJsaWMgcHJlbXVsdGlwbGllZEFscGhhPzogYm9vbGVhbiwgeGhyU2V0dGluZ3M/OiBQaGFzZXIuVHlwZXMuTG9hZGVyLlhIUlNldHRpbmdzT2JqZWN0KSB7XG5cdFx0aWYgKHR5cGVvZiBrZXkgIT09IFwic3RyaW5nXCIpIHtcblx0XHRcdGNvbnN0IGNvbmZpZyA9IGtleTtcblx0XHRcdGtleSA9IGNvbmZpZy5rZXk7XG5cdFx0XHR1cmwgPSBjb25maWcudXJsO1xuXHRcdFx0cHJlbXVsdGlwbGllZEFscGhhID0gY29uZmlnLnByZW11bHRpcGxpZWRBbHBoYTtcblx0XHRcdHhoclNldHRpbmdzID0gY29uZmlnLnhoclNldHRpbmdzO1xuXHRcdH1cblxuXHRcdHN1cGVyKGxvYWRlciwgU1BJTkVfQVRMQVNfRklMRV9UWVBFLCBrZXksIFtcblx0XHRcdG5ldyBQaGFzZXIuTG9hZGVyLkZpbGVUeXBlcy5UZXh0RmlsZShsb2FkZXIsIHtcblx0XHRcdFx0a2V5OiBrZXksXG5cdFx0XHRcdHVybDogdXJsLFxuXHRcdFx0XHR4aHJTZXR0aW5nczogeGhyU2V0dGluZ3MsXG5cdFx0XHRcdGV4dGVuc2lvbjogXCJhdGxhc1wiXG5cdFx0XHR9KVxuXHRcdF0pO1xuXHR9XG5cblx0b25GaWxlQ29tcGxldGUgKGZpbGU6IFBoYXNlci5Mb2FkZXIuRmlsZSkge1xuXHRcdGlmICh0aGlzLmZpbGVzLmluZGV4T2YoZmlsZSkgIT0gLTEpIHtcblx0XHRcdHRoaXMucGVuZGluZy0tO1xuXG5cdFx0XHRpZiAoZmlsZS50eXBlID09IFwidGV4dFwiKSB7XG5cdFx0XHRcdHZhciBsaW5lcyA9IGZpbGUuZGF0YS5zcGxpdCgvXFxyXFxufFxccnxcXG4vKTtcblx0XHRcdFx0bGV0IHRleHR1cmVzID0gW107XG5cdFx0XHRcdHRleHR1cmVzLnB1c2gobGluZXNbMF0pO1xuXHRcdFx0XHRmb3IgKHZhciB0ID0gMTsgdCA8IGxpbmVzLmxlbmd0aDsgdCsrKSB7XG5cdFx0XHRcdFx0dmFyIGxpbmUgPSBsaW5lc1t0XTtcblx0XHRcdFx0XHRpZiAobGluZS50cmltKCkgPT09ICcnICYmIHQgPCBsaW5lcy5sZW5ndGggLSAxKSB7XG5cdFx0XHRcdFx0XHRsaW5lID0gbGluZXNbdCArIDFdO1xuXHRcdFx0XHRcdFx0dGV4dHVyZXMucHVzaChsaW5lKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdH1cblxuXHRcdFx0XHRsZXQgYmFzZVBhdGggPSBmaWxlLnNyYy5tYXRjaCgvXi4qXFwvLykgPz8gXCJcIjtcblx0XHRcdFx0Zm9yICh2YXIgaSA9IDA7IGkgPCB0ZXh0dXJlcy5sZW5ndGg7IGkrKykge1xuXHRcdFx0XHRcdHZhciB1cmwgPSBiYXNlUGF0aCArIHRleHR1cmVzW2ldO1xuXHRcdFx0XHRcdHZhciBrZXkgPSBmaWxlLmtleSArIFwiIVwiICsgdGV4dHVyZXNbaV07XG5cdFx0XHRcdFx0dmFyIGltYWdlID0gbmV3IFBoYXNlci5Mb2FkZXIuRmlsZVR5cGVzLkltYWdlRmlsZSh0aGlzLmxvYWRlciwga2V5LCB1cmwpO1xuXG5cdFx0XHRcdFx0aWYgKCF0aGlzLmxvYWRlci5rZXlFeGlzdHMoaW1hZ2UpKSB7XG5cdFx0XHRcdFx0XHR0aGlzLmFkZFRvTXVsdGlGaWxlKGltYWdlKTtcblx0XHRcdFx0XHRcdHRoaXMubG9hZGVyLmFkZEZpbGUoaW1hZ2UpO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdH1cblx0fVxuXG5cdGFkZFRvQ2FjaGUgKCkge1xuXHRcdGlmICh0aGlzLmlzUmVhZHlUb1Byb2Nlc3MoKSkge1xuXHRcdFx0bGV0IHRleHR1cmVNYW5hZ2VyID0gdGhpcy5sb2FkZXIudGV4dHVyZU1hbmFnZXI7XG5cdFx0XHRmb3IgKGxldCBmaWxlIG9mIHRoaXMuZmlsZXMpIHtcblx0XHRcdFx0aWYgKGZpbGUudHlwZSA9PSBcImltYWdlXCIpIHtcblx0XHRcdFx0XHRpZiAoIXRleHR1cmVNYW5hZ2VyLmV4aXN0cyhmaWxlLmtleSkpIHtcblx0XHRcdFx0XHRcdHRleHR1cmVNYW5hZ2VyLmFkZEltYWdlKGZpbGUua2V5LCBmaWxlLmRhdGEpO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0XHR0aGlzLnByZW11bHRpcGxpZWRBbHBoYSA9IHRoaXMucHJlbXVsdGlwbGllZEFscGhhID8/IChmaWxlLmRhdGEuaW5kZXhPZihcInBtYTogdHJ1ZVwiKSA+PSAwIHx8IGZpbGUuZGF0YS5pbmRleE9mKFwicG1hOnRydWVcIikgPj0gMCk7XG5cdFx0XHRcdFx0ZmlsZS5kYXRhID0ge1xuXHRcdFx0XHRcdFx0ZGF0YTogZmlsZS5kYXRhLFxuXHRcdFx0XHRcdFx0cHJlbXVsdGlwbGllZEFscGhhOiB0aGlzLnByZW11bHRpcGxpZWRBbHBoYSxcblx0XHRcdFx0XHR9O1xuXHRcdFx0XHRcdGZpbGUuYWRkVG9DYWNoZSgpO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fVxuXHR9XG59XG4iXX0=