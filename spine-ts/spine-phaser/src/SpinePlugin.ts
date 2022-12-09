/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import Phaser from "phaser";
import { SPINE_ATLAS_CACHE_KEY, SPINE_FILE_TYPE, SPINE_TEXTURE_CACHE_KEY } from "./keys";
import { SceneRenderer, SkeletonDebugRenderer, SkeletonRenderer } from "@esotericsoftware/spine-webgl"
import { SpineFile } from "./SpineFile";

export class SpinePlugin extends Phaser.Plugins.ScenePlugin {
    game: Phaser.Game;
    isWebGL: boolean;
    atlasCache: Phaser.Cache.BaseCache;
    spineTextureCache: Phaser.Cache.BaseCache;
    jsonCache: Phaser.Cache.BaseCache;
    textures: Phaser.Textures.TextureManager;
    gl: WebGLRenderingContext | null;
    phaserRenderer: Phaser.Renderer.Canvas.CanvasRenderer | Phaser.Renderer.WebGL.WebGLRenderer | null;
    sceneRenderer: SceneRenderer | null;
    skeletonRenderer: SkeletonRenderer | null;
    skeletonDebugRenderer: SkeletonDebugRenderer | null;

    constructor(scene: Phaser.Scene, pluginManager: Phaser.Plugins.PluginManager, pluginKey: string) {
        super(scene, pluginManager, pluginKey);
        var game = this.game = pluginManager.game;
        this.isWebGL = this.game.config.renderType === 2;
        this.atlasCache = this.game.cache.addCustom(SPINE_ATLAS_CACHE_KEY);
        this.spineTextureCache = this.game.cache.addCustom(SPINE_TEXTURE_CACHE_KEY);
        this.jsonCache = this.game.cache.json;
        this.textures = this.game.textures;
        this.gl = this.isWebGL ? (this.game.renderer as Phaser.Renderer.WebGL.WebGLRenderer).gl : null;
        this.phaserRenderer = this.game.renderer;
        this.sceneRenderer = null;
        this.skeletonRenderer = null;
        this.skeletonDebugRenderer = null;

        if (!this.phaserRenderer) {
            this.phaserRenderer = {
                width: game.scale.width,
                height: game.scale.height,
                preRender: () => { },
                postRender: () => { },
                render: () => { },
                destroy: () => { }
            } as unknown as Phaser.Renderer.Canvas.CanvasRenderer;
        }

        let fileCallback = function (this: any, key: string | Phaser.Types.Loader.FileTypes.JSONFileConfig | Phaser.Types.Loader.FileTypes.JSONFileConfig[],
            jsonURL: string,
            atlasURL: string | string[],
            premultipliedAlpha: boolean,
            jsonXhrSettings: Phaser.Types.Loader.XHRSettingsObject,
            atlasXhrSettings: Phaser.Types.Loader.XHRSettingsObject) {
                let file = new SpineFile(this as any, key, jsonURL, atlasURL, premultipliedAlpha, jsonXhrSettings, atlasXhrSettings);
                this.addFile(file.files);
                return this;
            return this;
        };
        pluginManager.registerFileType(SPINE_FILE_TYPE, fileCallback, scene);

    }

    boot() {
        // FIXME
    }

    getNumbers(count: number) {
        let numbers = [];
        for (let i = 0; i < count; i++)
            numbers.push(i);
        return numbers;
    }
}