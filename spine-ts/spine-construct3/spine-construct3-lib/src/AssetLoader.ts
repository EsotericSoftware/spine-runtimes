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

import { AtlasAttachmentLoader, SkeletonBinary, type SkeletonData, SkeletonJson, TextureAtlas, type TextureAtlasPage } from "@esotericsoftware/spine-core";
import { C3TextureEditor, C3TextureRuntime } from "./C3Texture";


interface CacheEntry<T> {
	data?: T;
	promise: Promise<T>;
	refCount: number;
}

type ResourceCache<T> = Map<string, CacheEntry<T>>;
type RuntimeCacheType = "skeleton" | "atlas" | "texture";

export class AssetLoader {

	private static CacheSkeleton: ResourceCache<SkeletonData> = new Map();
	private static CacheAtlas: ResourceCache<TextureAtlas> = new Map();
	private static CacheTexture: ResourceCache<C3TextureRuntime> = new Map();
	private static retainAllUnusedRuntimeResources = false;
	private static retainedRuntimeResourceKeys = new Set<string>();

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
		if (!projectFile) throw new Error(`Atlas file not found with the given SID: ${sid}`);

		const blob = projectFile.GetBlob();
		const content = await blob.text();

		const path = projectFile.GetPath();
		const basePath = path.substring(0, path.lastIndexOf("/") + 1);
		const textureAtlas = new TextureAtlas(content);
		await Promise.all(textureAtlas.pages.map(async page => {
			const texture = await this.loadSpineTextureEditor(basePath + page.name, page.pma, instance);
			if (texture) {
				const spineTexture = new C3TextureEditor(texture, renderer, page);
				page.setTexture(spineTexture);
			}
			return texture;
		}));

		return { basePath, textureAtlas };
	}

	public async loadSpineTextureEditor (pageName: string, pma = false, instance: SDK.IWorldInstance) {
		const projectFile = instance.GetProject().GetProjectFileByExportPath(pageName);
		if (!projectFile) {
			throw new Error(`An error occurred while loading the texture: ${pageName}`);
		}

		const content = projectFile.GetBlob();
		return AssetLoader.createImageBitmapFromBlob(content, pma);
	}

	public getCachedRuntimeSkeletonAndAtlas (skeletonPath: string, atlasPath: string, scale = 1) {
		const skeletonKey = AssetLoader.getSkeletonCacheKey(skeletonPath, atlasPath, scale);
		const skeletonEntry = AssetLoader.CacheSkeleton.get(skeletonKey);
		const atlasEntry = AssetLoader.CacheAtlas.get(atlasPath);
		if (!skeletonEntry?.data || !atlasEntry?.data) return null;

		skeletonEntry.refCount++;
		atlasEntry.refCount++;
		return {
			skeletonData: skeletonEntry.data,
			textureAtlas: atlasEntry.data,
		};
	}

	public async loadSkeletonRuntime (path: string, atlasPath: string, textureAtlas: TextureAtlas, scale = 1, instance: IRuntime) {
		const loadPromise = (async () => {
			const fullPath = await instance.assets.getProjectFileUrl(path);
			if (!fullPath) throw new Error(`Cannot find project file url for: ${path}`);

			const atlasLoader = new AtlasAttachmentLoader(textureAtlas);

			let skeletonData: SkeletonData;
			const isBinary = path.endsWith(".skel");
			if (isBinary) {
				const content = await instance.assets.fetchArrayBuffer(fullPath);
				if (!content) throw new Error(`Cannot fetch array buffer for: ${fullPath}`);

				const skeletonLoader = new SkeletonBinary(atlasLoader);
				skeletonLoader.scale = scale;
				skeletonData = skeletonLoader.readSkeletonData(content);
			} else {
				const content = await instance.assets.fetchJson(fullPath);
				if (!content) throw new Error(`Cannot fetch json for: ${fullPath}`);

				const skeletonLoader = new SkeletonJson(atlasLoader);
				skeletonLoader.scale = scale;
				skeletonData = skeletonLoader.readSkeletonData(content);
			}
			return skeletonData;
		});

		return this.loadRuntimeResource(AssetLoader.getSkeletonCacheKey(path, atlasPath, scale), AssetLoader.CacheSkeleton, loadPromise);
	}

	public async loadAtlasRuntime (path: string, instance: IRuntime, renderer: IRenderer) {
		const loadPromise = (async () => {
			const fullPath = await instance.assets.getProjectFileUrl(path);
			if (!fullPath) throw new Error(`Cannot find project file url for: ${path}`);

			const content = await instance.assets.fetchText(fullPath);
			if (!content) throw new Error(`Cannot fetch text for: ${fullPath}`);

			const basePath = path.substring(0, path.lastIndexOf("/") + 1);
			const textureAtlas = new TextureAtlas(content);
			await Promise.all(textureAtlas.pages.map(async page => {
				const texture = await this.loadSpineTextureRuntime(basePath, page, instance, renderer);
				if (texture) page.setTexture(texture);
				return texture;
			}));

			return textureAtlas;
		});

		return this.loadRuntimeResource(path, AssetLoader.CacheAtlas, loadPromise);
	}

	public async loadSpineTextureRuntime (basePath: string, page: TextureAtlasPage, instance: IRuntime, renderer: IRenderer) {
		const cacheKey = basePath + page.name;

		const loadPromise = (async () => {
			const fullPath = await instance.assets.getProjectFileUrl(cacheKey);
			if (!fullPath) throw new Error(`Cannot find project file url for: ${cacheKey}`);

			const content = await instance.assets.fetchBlob(fullPath);
			if (!content) throw new Error(`Cannot fetch blob for: ${fullPath}`);

			const image = await AssetLoader.createImageBitmapFromBlob(content, page.pma);
			if (!image) throw new Error(`Cannot create image bitmap for: ${fullPath}`);

			return new C3TextureRuntime(image, renderer, page);
		});

		return this.loadRuntimeResource(cacheKey, AssetLoader.CacheTexture, loadPromise);
	}

	public releaseInstanceResources (skeletonPath: string, atlasPath: string, loaderScale: number) {
		this.releaseResource("skeleton", AssetLoader.CacheSkeleton, AssetLoader.getSkeletonCacheKey(skeletonPath, atlasPath, loaderScale));

		const atlasEntry = AssetLoader.CacheAtlas.get(atlasPath);
		if (atlasEntry) {
			this.releaseResource("atlas", AssetLoader.CacheAtlas, atlasPath, async () => {
				const basePath = atlasPath.substring(0, atlasPath.lastIndexOf("/") + 1);
				for (const page of (await atlasEntry.promise).pages) {
					const textureKey = basePath + page.name;
					this.releaseResource("texture", AssetLoader.CacheTexture, textureKey, (texture) => {
						texture?.dispose();
					});
				}
			});
		}
	}

	public retainInstanceResources (skeletonPath: string, atlasPath: string, loaderScale: number, retained: boolean) {
		const skeletonKey = AssetLoader.getSkeletonCacheKey(skeletonPath, atlasPath, loaderScale);
		this.setRuntimeResourceRetained("skeleton", skeletonKey, retained);
		this.setRuntimeResourceRetained("atlas", atlasPath, retained);

		const atlasEntry = AssetLoader.CacheAtlas.get(atlasPath);
		if (atlasEntry) {
			const basePath = atlasPath.substring(0, atlasPath.lastIndexOf("/") + 1);
			atlasEntry.promise.then(textureAtlas => {
				for (const page of textureAtlas.pages)
					this.setRuntimeResourceRetained("texture", basePath + page.name, retained);
			});
		}
	}

	public setAllRuntimeResourcesRetained (retained: boolean) {
		AssetLoader.retainAllUnusedRuntimeResources = retained;
		if (!retained) this.releaseAllUnusedRuntimeResources();
	}

	public releaseRetainedInstanceResources (skeletonPath: string, atlasPath: string, loaderScale: number) {
		this.retainInstanceResources(skeletonPath, atlasPath, loaderScale, false);
	}

	public releaseAllUnusedRuntimeResources () {
		AssetLoader.retainedRuntimeResourceKeys.clear();
		for (const key of Array.from(AssetLoader.CacheSkeleton.keys()))
			this.deleteResourceIfUnused("skeleton", AssetLoader.CacheSkeleton, key, undefined, true);
		for (const key of Array.from(AssetLoader.CacheAtlas.keys()))
			this.deleteAtlasResourceIfUnused(key, true);
		for (const key of Array.from(AssetLoader.CacheTexture.keys()))
			this.deleteResourceIfUnused("texture", AssetLoader.CacheTexture, key, texture => texture?.dispose(), true);
	}

	private setRuntimeResourceRetained (type: RuntimeCacheType, key: string, retained: boolean) {
		const retainKey = AssetLoader.getRetainKey(type, key);
		if (retained) {
			AssetLoader.retainedRuntimeResourceKeys.add(retainKey);
		} else {
			AssetLoader.retainedRuntimeResourceKeys.delete(retainKey);
			switch (type) {
				case "skeleton": this.deleteResourceIfUnused(type, AssetLoader.CacheSkeleton, key); break;
				case "atlas": this.deleteAtlasResourceIfUnused(key); break;
				case "texture": this.deleteResourceIfUnused(type, AssetLoader.CacheTexture, key, texture => texture?.dispose()); break;
			}
		}
	}

	private releaseResource<T> (type: RuntimeCacheType, cache: ResourceCache<T>, key: string, disposer?: (data?: T) => void) {
		const entry = cache.get(key);
		if (!entry) return;

		entry.refCount--;
		this.deleteResourceIfUnused(type, cache, key, disposer);
	}

	private deleteAtlasResourceIfUnused (key: string, force = false) {
		const atlasEntry = AssetLoader.CacheAtlas.get(key);
		if (!atlasEntry) return;

		this.deleteResourceIfUnused("atlas", AssetLoader.CacheAtlas, key, async () => {
			const basePath = key.substring(0, key.lastIndexOf("/") + 1);
			for (const page of (await atlasEntry.promise).pages)
				this.releaseResource("texture", AssetLoader.CacheTexture, basePath + page.name, texture => texture?.dispose());
		}, force);
	}

	private deleteResourceIfUnused<T> (type: RuntimeCacheType, cache: ResourceCache<T>, key: string, disposer?: (data?: T) => void, force = false) {
		const entry = cache.get(key);
		if (!entry || entry.refCount > 0) return;
		if (!force && (AssetLoader.retainAllUnusedRuntimeResources || AssetLoader.retainedRuntimeResourceKeys.has(AssetLoader.getRetainKey(type, key)))) return;

		if (disposer) disposer(entry.data);
		cache.delete(key);
	}

	private static getRetainKey (type: RuntimeCacheType, key: string) {
		return `${type}:${key}`;
	}

	private static getSkeletonCacheKey (skeletonPath: string, atlasPath: string, scale: number) {
		return `${skeletonPath}|atlas${atlasPath}|scale${scale}`;
	}

	private async loadRuntimeResource<T> (cacheKey: string, resourceCache: ResourceCache<T>, loader: () => Promise<T>): Promise<T> {
		const entry = this.getFromCache(resourceCache, cacheKey);
		if (entry) return entry.promise;
		const result = loader();
		this.addToCache(resourceCache, cacheKey, result);
		return result;
	}

	private addToCache<T> (cache: ResourceCache<T>, cacheKey: string, promise: Promise<T>) {
		const cacheEntry: CacheEntry<T> = { promise, refCount: 1 };
		cache.set(cacheKey, cacheEntry);
		promise.then(
			data => {
				if (cache.get(cacheKey) === cacheEntry) cacheEntry.data = data;
			},
			() => {
				if (cache.get(cacheKey) === cacheEntry) cache.delete(cacheKey);
			}
		);
	}

	private getFromCache<T> (cache: ResourceCache<T>, cacheKey: string) {
		const entry = cache.get(cacheKey);
		if (!entry) return undefined;

		entry.refCount++;
		return entry;
	}

	static async createImageBitmapFromBlob (blob: Blob, pma: boolean): Promise<ImageBitmap> {
		try {
			return createImageBitmap(blob, { premultiplyAlpha: pma ? "none" : "premultiply" });
		} catch (e) {
			console.error("Failed to create ImageBitmap from blob:", e);
			throw e;
		}
	}

}
