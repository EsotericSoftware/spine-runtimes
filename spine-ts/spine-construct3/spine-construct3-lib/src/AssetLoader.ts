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
	users: Set<object>;
}

type ResourceCache<T> = Map<string, CacheEntry<T>>;
type RuntimeCacheType = "skeleton" | "atlas";

export class AssetLoader {

	private static CacheSkeleton: ResourceCache<SkeletonData> = new Map();
	private static CacheAtlas: ResourceCache<TextureAtlas> = new Map();
	private static retainAllUnusedRuntimeResources = false;
	private static retainedRuntimeResourceKeys = new Set<string>();

	public async loadSkeletonEditor (sid: number, textureAtlas: TextureAtlas, scale = 1, instance: SDK.IWorldInstance) {
		const projectFile = instance.GetProject().GetProjectFileBySID(sid);
		if (!projectFile) throw new Error(`Skeleton file not found with the given SID: ${sid}`);
		if (!/\.(json|skel)$/i.test(projectFile.GetName()))
			throw new Error(`Invalid Spine skeleton file: ${projectFile.GetName()}`);
		if (!Number.isFinite(scale) || scale <= 0)
			throw new Error("Spine loader scale must be a finite number greater than zero.");

		const blob = projectFile.GetBlob();
		const atlasLoader = new AtlasAttachmentLoader(textureAtlas);

		const isBinary = projectFile.GetName().toLowerCase().endsWith(".skel");
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
		if (!projectFile.GetName().toLowerCase().endsWith(".atlas"))
			throw new Error(`Invalid Spine atlas file: ${projectFile.GetName()}`);

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

	public getCachedRuntimeSkeletonAndAtlas (skeletonPath: string, atlasPath: string, owner: object, scale = 1) {
		const skeletonKey = AssetLoader.getSkeletonCacheKey(skeletonPath, atlasPath, scale);
		const skeletonEntry = AssetLoader.CacheSkeleton.get(skeletonKey);
		const atlasEntry = AssetLoader.CacheAtlas.get(atlasPath);
		if (!skeletonEntry?.data || !atlasEntry?.data) return null;

		skeletonEntry.users.add(owner);
		atlasEntry.users.add(owner);
		return {
			skeletonData: skeletonEntry.data,
			textureAtlas: atlasEntry.data,
		};
	}

	public loadSkeletonRuntime (path: string, atlasPath: string, textureAtlas: TextureAtlas, owner: object, scale = 1, instance: IRuntime) {
		if (!/\.(json|skel)$/i.test(path)) throw new Error(`Invalid Spine skeleton file: ${path}`);
		if (!atlasPath.toLowerCase().endsWith(".atlas")) throw new Error(`Invalid Spine atlas file: ${atlasPath}`);
		if (!Number.isFinite(scale) || scale <= 0)
			throw new Error("Spine loader scale must be a finite number greater than zero.");

		const loadPromise = (async () => {
			const fullPath = await instance.assets.getProjectFileUrl(path);
			if (!fullPath) throw new Error(`Cannot find project file url for: ${path}`);

			const atlasLoader = new AtlasAttachmentLoader(textureAtlas);

			let skeletonData: SkeletonData;
			const isBinary = path.toLowerCase().endsWith(".skel");
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

		return this.loadRuntimeResource(AssetLoader.getSkeletonCacheKey(path, atlasPath, scale), AssetLoader.CacheSkeleton, owner, loadPromise);
	}

	public loadAtlasRuntime (path: string, owner: object, instance: IRuntime, renderer: IRenderer) {
		if (!path.toLowerCase().endsWith(".atlas")) throw new Error(`Invalid Spine atlas file: ${path}`);

		const loadPromise = (async () => {
			const fullPath = await instance.assets.getProjectFileUrl(path);
			if (!fullPath) throw new Error(`Cannot find project file url for: ${path}`);

			const content = await instance.assets.fetchText(fullPath);
			if (!content) throw new Error(`Cannot fetch text for: ${fullPath}`);

			const basePath = path.substring(0, path.lastIndexOf("/") + 1);
			const textureAtlas = new TextureAtlas(content);
			await Promise.all(textureAtlas.pages.map(async page => {
				const texture = await this.loadSpineTextureRuntime(basePath, page, instance, renderer);
				page.setTexture(texture);
			}));
			return textureAtlas;
		});

		return this.loadRuntimeResource(path, AssetLoader.CacheAtlas, owner, loadPromise);
	}

	public async loadSpineTextureRuntime (basePath: string, page: TextureAtlasPage, instance: IRuntime, renderer: IRenderer) {
		const texturePath = basePath + page.name;
		const fullPath = await instance.assets.getProjectFileUrl(texturePath);
		if (!fullPath) throw new Error(`Cannot find project file url for: ${texturePath}`);

		const content = await instance.assets.fetchBlob(fullPath);
		if (!content) throw new Error(`Cannot fetch blob for: ${fullPath}`);

		const image = await AssetLoader.createImageBitmapFromBlob(content, page.pma);
		return new C3TextureRuntime(image, renderer, page);
	}

	public releaseInstanceResources (owner: object) {
		for (const key of Array.from(AssetLoader.CacheSkeleton.keys()))
			this.releaseResource("skeleton", AssetLoader.CacheSkeleton, key, owner);
		for (const key of Array.from(AssetLoader.CacheAtlas.keys()))
			this.releaseResource("atlas", AssetLoader.CacheAtlas, key, owner, textureAtlas => textureAtlas.dispose());
	}

	public retainInstanceResources (skeletonPath: string, atlasPath: string, loaderScale: number, retained: boolean) {
		const skeletonKey = AssetLoader.getSkeletonCacheKey(skeletonPath, atlasPath, loaderScale);
		this.setRuntimeResourceRetained("skeleton", skeletonKey, retained);
		this.setRuntimeResourceRetained("atlas", atlasPath, retained);
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
			this.deleteResourceIfUnused("atlas", AssetLoader.CacheAtlas, key, textureAtlas => textureAtlas.dispose(), true);
	}

	private setRuntimeResourceRetained (type: RuntimeCacheType, key: string, retained: boolean) {
		const retainKey = AssetLoader.getRetainKey(type, key);
		if (retained) {
			AssetLoader.retainedRuntimeResourceKeys.add(retainKey);
		} else {
			AssetLoader.retainedRuntimeResourceKeys.delete(retainKey);
			if (type === "skeleton") {
				this.deleteResourceIfUnused(type, AssetLoader.CacheSkeleton, key);
			} else {
				this.deleteResourceIfUnused(type, AssetLoader.CacheAtlas, key, textureAtlas => textureAtlas.dispose());
			}
		}
	}

	private releaseResource<T> (type: RuntimeCacheType, cache: ResourceCache<T>, key: string, owner: object, disposer?: (data: T) => void) {
		const entry = cache.get(key);
		if (!entry?.users.delete(owner)) return;
		this.deleteResourceIfUnused(type, cache, key, disposer);
	}

	private deleteResourceIfUnused<T> (type: RuntimeCacheType, cache: ResourceCache<T>, key: string, disposer?: (data: T) => void, force = false) {
		const entry = cache.get(key);
		if (!entry || entry.users.size > 0) return;
		if (!force && (AssetLoader.retainAllUnusedRuntimeResources || AssetLoader.retainedRuntimeResourceKeys.has(AssetLoader.getRetainKey(type, key)))) return;

		cache.delete(key);
		if (disposer) void entry.promise.then(disposer, () => { });
	}

	private static getRetainKey (type: RuntimeCacheType, key: string) {
		return `${type}:${key}`;
	}

	private static getSkeletonCacheKey (skeletonPath: string, atlasPath: string, scale: number) {
		return `${skeletonPath}|atlas${atlasPath}|scale${scale}`;
	}

	private loadRuntimeResource<T> (cacheKey: string, resourceCache: ResourceCache<T>, owner: object, loader: () => Promise<T>): Promise<T> {
		const cachedEntry = resourceCache.get(cacheKey);
		if (cachedEntry) {
			cachedEntry.users.add(owner);
			return cachedEntry.promise;
		}

		const promise = loader();
		const cacheEntry: CacheEntry<T> = { promise, users: new Set([owner]) };
		resourceCache.set(cacheKey, cacheEntry);
		promise.then(
			data => {
				if (resourceCache.get(cacheKey) === cacheEntry) cacheEntry.data = data;
			},
			() => {
				if (resourceCache.get(cacheKey) === cacheEntry) resourceCache.delete(cacheKey);
			}
		);
		return promise;
	}

	static createImageBitmapFromBlob (blob: Blob, pma: boolean): Promise<ImageBitmap> {
		return createImageBitmap(blob, { premultiplyAlpha: pma ? "none" : "premultiply" });
	}

}
