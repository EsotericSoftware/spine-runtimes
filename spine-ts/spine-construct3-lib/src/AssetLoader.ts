/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

import { AtlasAttachmentLoader, SkeletonBinary, SkeletonJson, TextureAtlas } from "@esotericsoftware/spine-core";
import { C3Texture, C3TextureEditor } from "./C3Texture";


export class AssetLoader {

	public async loadSkeletonEditor (sid: number, textureAtlas: TextureAtlas, scale = 1, instance: SDK.IWorldInstance) {
		const projectFile = instance.GetProject().GetProjectFileBySID(sid);
		if (!projectFile) return null;

		const blob = projectFile.GetBlob();
		const atlasLoader = new AtlasAttachmentLoader(textureAtlas);

		const isBinary = projectFile.GetName().endsWith(".skel");
		if (isBinary) {
			const skeletonFile = await blob.arrayBuffer();
			const skeletonLoader = new SkeletonBinary(atlasLoader);
			skeletonLoader.scale = scale;
			return skeletonLoader.readSkeletonData(skeletonFile);
		}

		const skeletonFile = await blob.text();
		const skeletonLoader = new SkeletonJson(atlasLoader);
		skeletonLoader.scale = scale;
		return skeletonLoader.readSkeletonData(skeletonFile);
	}

	public async loadAtlasEditor (sid: number, instance: SDK.IWorldInstance, renderer: SDK.Gfx.IWebGLRenderer) {
		const projectFile = instance.GetProject().GetProjectFileBySID(sid);
		if (!projectFile) return null;

		const blob = projectFile.GetBlob();
		const content = await blob.text();

		const textureAtlas = new TextureAtlas(content);
		await Promise.all(textureAtlas.pages.map(async page => {
			const texture = await this.loadSpineTextureEditor(page.name, page.pma, instance);
			if (texture) {
				const spineTexture = new C3TextureEditor(texture, renderer, page);
				page.setTexture(spineTexture);
			}
			return texture;
		}));

		return textureAtlas;
	}

	public async loadSpineTextureEditor (pageName: string, pma = false, instance: SDK.IWorldInstance) {
		const projectFile = instance.GetProject().GetProjectFileByExportPath(pageName);
		if (!projectFile) {
			throw new Error(`An error occured while loading the texture: ${pageName}`);
		}

		const content = projectFile.GetBlob();
		return AssetLoader.createImageBitmapFromBlob(content, pma);
	}

	public async loadSkeletonRuntime (path: string, textureAtlas: TextureAtlas, scale = 1, instance: IRuntime) {
		const fullPath = await instance.assets.getProjectFileUrl(path);
		if (!fullPath) return null;

		const atlasLoader = new AtlasAttachmentLoader(textureAtlas);

		const isBinary = path.endsWith(".skel");
		if (isBinary) {
			const content = await instance.assets.fetchArrayBuffer(fullPath);
			if (!content) return null;
			const skeletonLoader = new SkeletonBinary(atlasLoader);
			skeletonLoader.scale = scale;
			return skeletonLoader.readSkeletonData(content);
		}
		const content = await instance.assets.fetchJson(fullPath);
		if (!content) return null;
		const skeletonLoader = new SkeletonJson(atlasLoader);
		skeletonLoader.scale = scale;
		return skeletonLoader.readSkeletonData(content);
	}

	public async loadAtlasRuntime (path: string, instance: IRuntime, renderer: IRenderer) {
		const fullPath = await instance.assets.getProjectFileUrl(path);
		if (!fullPath) return null;

		const content = await instance.assets.fetchText(fullPath);
		if (!content) return null;

		const textureAtlas = new TextureAtlas(content);
		await Promise.all(textureAtlas.pages.map(async page => {
			const texture = await this.loadSpineTextureRuntime(page.name, page.pma, instance);
			if (texture) {
				const spineTexture = new C3Texture(texture, renderer, page);
				page.setTexture(spineTexture);
			}
			return texture;
		}));
		return textureAtlas;
	}

	public async loadSpineTextureRuntime (pageName: string, pma = false, instance: IRuntime) {
		const fullPath = await instance.assets.getProjectFileUrl(pageName);
		if (!fullPath) return null;

		const content = await instance.assets.fetchBlob(fullPath);
		if (!content) return null;

		return AssetLoader.createImageBitmapFromBlob(content, pma);
	}

	static async createImageBitmapFromBlob (blob: Blob, pma: boolean): Promise<ImageBitmap | null> {
		try {
			// pma parameters seems to do not matter here. It matters in C3 Texture creation
			return createImageBitmap(blob, { premultiplyAlpha: pma ? "none" : "premultiply" });
		} catch (e) {
			console.error("Failed to create ImageBitmap from blob:", e);
			return null;
		}
	}

}


