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

import type { Texture } from "./Texture.js";
import { TextureAtlas } from "./TextureAtlas.js";
import type { Disposable, StringMap } from "./Utils.js";

type AssetData = (Uint8Array | string | Texture | TextureAtlas | object) & Partial<Disposable>;
type AssetCallback<T extends AssetData> = (path: string, data: T) => void;
type ErrorCallback = (path: string, message: string) => void;

export class AssetManagerBase implements Disposable {
	private pathPrefix: string = "";
	private textureLoader: (image: HTMLImageElement | ImageBitmap) => Texture;
	private downloader: Downloader;
	private cache: AssetCache;
	private errors: StringMap<string> = {};
	private toLoad = 0;
	private loaded = 0;

	constructor (textureLoader: (image: HTMLImageElement | ImageBitmap) => Texture, pathPrefix: string = "", downloader = new Downloader(), cache = new AssetCache()) {
		this.textureLoader = textureLoader;
		this.pathPrefix = pathPrefix;
		this.downloader = downloader;
		this.cache = cache;
	}

	private start (path: string): string {
		this.toLoad++;
		return this.pathPrefix + path;
	}

	private success<T extends AssetData> (callback: AssetCallback<T>, path: string, asset: T) {
		this.toLoad--;
		this.loaded++;
		this.cache.assets[path] = asset;
		this.cache.assetsRefCount[path] = (this.cache.assetsRefCount[path] || 0) + 1;
		if (callback) callback(path, asset);
	}

	private error (callback: (path: string, message: string) => void, path: string, message: string) {
		this.toLoad--;
		this.loaded++;
		this.errors[path] = message;
		if (callback) callback(path, message);
	}

	loadAll () {
		const promise = new Promise((resolve: (assetManager: AssetManagerBase) => void, reject: (errors: StringMap<string>) => void) => {
			const check = () => {
				if (this.isLoadingComplete()) {
					if (this.hasErrors()) reject(this.errors);
					else resolve(this);
					return;
				}
				requestAnimationFrame(check);
			}
			requestAnimationFrame(check);
		});
		return promise;
	}

	setRawDataURI (path: string, data: string) {
		this.downloader.rawDataUris[this.pathPrefix + path] = data;
	}

	loadBinary (path: string,
		success: (path: string, binary: Uint8Array) => void = () => { },
		error: (path: string, message: string) => void = () => { }) {
		path = this.start(path);

		if (this.reuseAssets(path, success, error)) return;

		this.cache.assetsLoaded[path] = new Promise<Uint8Array>((resolve, reject) => {
			this.downloader.downloadBinary(path, (data: Uint8Array): void => {
				this.success(success, path, data);
				resolve(data);
			}, (status: number, responseText: string): void => {
				const errorMsg = `Couldn't load binary ${path}: status ${status}, ${responseText}`;
				this.error(error, path, errorMsg);
				reject(errorMsg);
			});
		});
	}

	loadText (path: string,
		success: (path: string, text: string) => void = () => { },
		error: (path: string, message: string) => void = () => { }) {
		path = this.start(path);

		this.downloader.downloadText(path, (data: string): void => {
			this.success(success, path, data);
		}, (status: number, responseText: string): void => {
			this.error(error, path, `Couldn't load text ${path}: status ${status}, ${responseText}`);
		});
	}

	loadJson (path: string,
		success: (path: string, object: object) => void = () => { },
		error: (path: string, message: string) => void = () => { }) {
		path = this.start(path);

		if (this.reuseAssets(path, success, error)) return;

		this.cache.assetsLoaded[path] = new Promise<object>((resolve, reject) => {
			this.downloader.downloadJson(path, (data: object): void => {
				this.success(success, path, data);
				resolve(data);
			}, (status: number, responseText: string): void => {
				const errorMsg = `Couldn't load JSON ${path}: status ${status}, ${responseText}`;
				this.error(error, path, errorMsg);
				reject(errorMsg);
			});
		});
	}


	reuseAssets<T extends AssetData> (
		path: string,
		success: AssetCallback<T> = () => { },
		error: ErrorCallback = () => { }
	) {
		const loadedStatus = this.cache.getAsset(path);
		const alreadyExistsOrLoading = loadedStatus !== undefined;
		if (alreadyExistsOrLoading) {
			this.cache.assetsLoaded[path] = loadedStatus
				.then(data => {
					// necessary when user preloads an image into the cache.
					// texture loader is not avaiable in the cache, so we transform in GLTexture at first use
					data = (data instanceof Image || data instanceof ImageBitmap) ? this.textureLoader(data) as T : data;
					this.success(success, path, data as T);
					return data;
				})
				.catch(errorMsg => {
					this.error(error, path, errorMsg);
					return undefined;
				});
		}
		return alreadyExistsOrLoading;
	}

	loadTexture (
		path: string,
		success: AssetCallback<Texture> = () => { },
		error: ErrorCallback = () => { }
	) {

		path = this.start(path);

		if (this.reuseAssets(path, success, error)) return;

		this.cache.assetsLoaded[path] = new Promise<Texture>((resolve, reject) => {
			const isBrowser = !!(typeof window !== 'undefined' && typeof navigator !== 'undefined' && window.document);
			const isWebWorker = !isBrowser; // && typeof importScripts !== 'undefined';
			if (isWebWorker) {
				fetch(path, { mode: <RequestMode>"cors" }).then((response) => {
					if (response.ok) return response.blob();
					const errorMsg = `Couldn't load image: ${path}`;
					this.error(error, path, `Couldn't load image: ${path}`);
					reject(errorMsg);
				}).then((blob) => {
					return blob ? createImageBitmap(blob, { premultiplyAlpha: "none", colorSpaceConversion: "none" }) : null;
				}).then((bitmap) => {
					if (bitmap) {
						const texture = this.createTexture(path, bitmap);
						this.success(success, path, texture);
						resolve(texture);
					};
				});
			} else {
				const image = new Image();
				image.crossOrigin = "anonymous";
				image.onload = () => {
					const texture = this.createTexture(path, image);
					this.success(success, path, texture);
					resolve(texture);
				};
				image.onerror = () => {
					const errorMsg = `Couldn't load image: ${path}`;
					this.error(error, path, errorMsg);
					reject(errorMsg);
				};
				if (this.downloader.rawDataUris[path]) path = this.downloader.rawDataUris[path];
				image.src = path;
			}
		});
	}

	loadTextureAtlas (
		path: string,
		success: AssetCallback<TextureAtlas> = () => { },
		error: ErrorCallback = () => { },
		fileAlias?: { [keyword: string]: string }
	) {
		const index = path.lastIndexOf("/");
		const parent = index >= 0 ? path.substring(0, index + 1) : "";
		path = this.start(path);

		if (this.reuseAssets(path, success, error)) return;

		this.cache.assetsLoaded[path] = new Promise<TextureAtlas>((resolve, reject) => {
			this.downloader.downloadText(path, (atlasText: string): void => {
				try {
					const atlas = this.createTextureAtlas(path, atlasText);
					let toLoad = atlas.pages.length, abort = false;
					for (const page of atlas.pages) {
						this.loadTexture(!fileAlias ? parent + page.name : fileAlias[page.name],
							(imagePath: string, texture: Texture) => {
								if (!abort) {
									page.setTexture(texture);
									if (--toLoad === 0) {
										this.success(success, path, atlas);
										resolve(atlas);
									}
								}
							},
							(imagePath: string, message: string) => {
								if (!abort) {
									const errorMsg = `Couldn't load texture ${path} page image: ${imagePath}`;
									this.error(error, path, errorMsg);
									reject(errorMsg);
								}
								abort = true;
							}
						);
					}
				} catch (e: unknown) {
					const errorMsg = `Couldn't parse texture atlas ${path}: ${(e as Error).message}`;
					this.error(error, path, errorMsg);
					reject(errorMsg);
				}
			}, (status: number, responseText: string): void => {
				const errorMsg = `Couldn't load texture atlas ${path}: status ${status}, ${responseText}`;
				this.error(error, path, errorMsg);
				reject(errorMsg);
			});
		});
	}

	loadTextureAtlasButNoTextures (
		path: string,
		success: AssetCallback<TextureAtlas> = () => { },
		error: ErrorCallback = () => { },
	) {
		path = this.start(path);

		if (this.reuseAssets(path, success, error)) return;

		this.cache.assetsLoaded[path] = new Promise<TextureAtlas>((resolve, reject) => {
			this.downloader.downloadText(path, (atlasText: string): void => {
				try {
					const atlas = this.createTextureAtlas(path, atlasText);
					this.success(success, path, atlas);
					resolve(atlas);
				} catch (e) {
					const errorMsg = `Couldn't parse texture atlas ${path}: ${(e as Error).message}`;
					this.error(error, path, errorMsg);
					reject(errorMsg);
				}
			}, (status: number, responseText: string): void => {
				const errorMsg = `Couldn't load texture atlas ${path}: status ${status}, ${responseText}`;
				this.error(error, path, errorMsg);
				reject(errorMsg);
			});
		});
	}

	// Promisified versions of load function
	async loadBinaryAsync (path: string) {
		return new Promise((resolve, reject) => {
			this.loadBinary(path,
				(_, binary) => resolve(binary),
				(_, message) => reject(message),
			);
		});
	}

	async loadJsonAsync (path: string) {
		return new Promise((resolve, reject) => {
			this.loadJson(path,
				(_, object) => resolve(object),
				(_, message) => reject(message),
			);
		});
	}

	async loadTextureAsync (path: string) {
		return new Promise<Texture>((resolve, reject) => {
			this.loadTexture(path,
				(_, texture) => resolve(texture),
				(_, message) => reject(message),
			);
		});
	}

	async loadTextureAtlasAsync (path: string) {
		return new Promise((resolve, reject) => {
			this.loadTextureAtlas(path,
				(_, atlas) => resolve(atlas),
				(_, message) => reject(message),
			);
		});
	}

	async loadTextureAtlasButNoTexturesAsync (path: string) {
		return new Promise<TextureAtlas>((resolve, reject) => {
			this.loadTextureAtlasButNoTextures(path,
				(_, atlas) => resolve(atlas),
				(_, message) => reject(message),
			);
		});
	}

	setCache (cache: AssetCache) {
		this.cache = cache;
	}

	get (path: string) {
		return this.cache.assets[this.pathPrefix + path];
	}

	require (path: string) {
		path = this.pathPrefix + path;
		const asset = this.cache.assets[path];
		if (asset) return asset;
		const error = this.errors[path];
		throw Error(`Asset not found: ${path}${error ? `\n${error}` : ""}`);
	}

	remove (path: string) {
		path = this.pathPrefix + path;
		const asset = this.cache.assets[path];
		if (asset.dispose) asset.dispose();
		delete this.cache.assets[path];
		delete this.cache.assetsRefCount[path];
		delete this.cache.assetsLoaded[path];
		return asset;
	}

	removeAll () {
		for (const path in this.cache.assets) {
			const asset = this.cache.assets[path];
			if (asset.dispose) asset.dispose();
		}
		this.cache.assets = {};
		this.cache.assetsLoaded = {};
		this.cache.assetsRefCount = {};
	}

	isLoadingComplete (): boolean {
		return this.toLoad === 0;
	}

	getToLoad (): number {
		return this.toLoad;
	}

	getLoaded (): number {
		return this.loaded;
	}

	dispose () {
		this.removeAll();
	}

	// dispose asset only if it's not used by others
	disposeAsset (path: string) {
		const asset = this.cache.assets[path];
		if (asset instanceof TextureAtlas) {
			asset.dispose();
			return;
		}
		this.disposeAssetInternal(path);
	}

	hasErrors () {
		return Object.keys(this.errors).length > 0;
	}

	getErrors () {
		return this.errors;
	}

	private disposeAssetInternal (path: string) {
		if (this.cache.assetsRefCount[path] > 0 && --this.cache.assetsRefCount[path] === 0) {
			return this.remove(path);
		}
	}

	private createTextureAtlas (path: string, atlasText: string): TextureAtlas {
		const atlas = new TextureAtlas(atlasText);
		atlas.dispose = () => {
			if (this.cache.assetsRefCount[path] <= 0) return;
			this.disposeAssetInternal(path);
			for (const page of atlas.pages) {
				page.texture?.dispose();
			}
		}
		return atlas;
	}

	private createTexture (path: string, image: HTMLImageElement | ImageBitmap): Texture {
		const texture = this.textureLoader(image);
		const textureDispose = texture.dispose.bind(texture);
		texture.dispose = () => {
			if (this.disposeAssetInternal(path)) textureDispose();
		}
		return texture;
	}
}

export class AssetCache {
	public assets: StringMap<AssetData> = {};
	public assetsRefCount: StringMap<number> = {};
	public assetsLoaded: StringMap<Promise<AssetData | undefined>> = {};

	static AVAILABLE_CACHES = new Map<string, AssetCache>();
	static getCache (id: string) {
		const cache = AssetCache.AVAILABLE_CACHES.get(id);
		if (cache) return cache;

		const newCache = new AssetCache();
		AssetCache.AVAILABLE_CACHES.set(id, newCache);
		return newCache;
	}

	async addAsset<T extends AssetData> (path: string, asset: T): Promise<T> {
		this.assetsLoaded[path] = Promise.resolve(asset);
		this.assets[path] = asset;
		return asset;
	}

	getAsset<T extends AssetData> (path: string): Promise<T> | undefined {
		return this.assetsLoaded[path] as Promise<T> | undefined;
	}
}

type DownloaderSuccessCallback<T extends AssetData = AssetData> = (data: T) => void;
type DownloaderErrorCallback = (status: number, responseText: string) => void;

export class Downloader {
	private callbacks: StringMap<Array<DownloaderSuccessCallback | DownloaderErrorCallback>> = {};
	rawDataUris: StringMap<string> = {};

	dataUriToString (dataUri: string) {
		if (!dataUri.startsWith("data:")) {
			throw new Error("Not a data URI.");
		}

		let base64Idx = dataUri.indexOf("base64,");
		if (base64Idx !== -1) {
			base64Idx += "base64,".length;
			return atob(dataUri.substr(base64Idx));
		} else {
			return dataUri.substr(dataUri.indexOf(",") + 1);
		}
	}

	base64ToUint8Array (base64: string) {
		var binary_string = window.atob(base64);
		var len = binary_string.length;
		var bytes = new Uint8Array(len);
		for (let i = 0; i < len; i++) {
			bytes[i] = binary_string.charCodeAt(i);
		}
		return bytes;
	}

	dataUriToUint8Array (dataUri: string) {
		if (!dataUri.startsWith("data:")) {
			throw new Error("Not a data URI.");
		}

		let base64Idx = dataUri.indexOf("base64,");
		if (base64Idx === -1) throw new Error("Not a binary data URI.");
		base64Idx += "base64,".length;
		return this.base64ToUint8Array(dataUri.substr(base64Idx));
	}

	downloadText (url: string, success: DownloaderSuccessCallback<string>, error: DownloaderErrorCallback) {
		if (this.start(url, success, error)) return;

		const rawDataUri = this.rawDataUris[url];
		// we assume if a "." is included in a raw data uri, it is used to rewrite an asset URL
		if (rawDataUri && !rawDataUri.includes(".")) {
			try {
				this.finish(url, 200, this.dataUriToString(rawDataUri));
			} catch (e) {
				this.finish(url, 400, JSON.stringify(e));
			}
			return;
		}

		const request = new XMLHttpRequest();
		request.overrideMimeType("text/html");
		request.open("GET", rawDataUri ? rawDataUri : url, true);
		const done = () => {
			this.finish(url, request.status, request.responseText);
		};
		request.onload = done;
		request.onerror = done;
		request.send();
	}

	downloadJson (url: string, success: DownloaderSuccessCallback<object>, error: DownloaderErrorCallback) {
		this.downloadText(url, (data: string): void => {
			success(JSON.parse(data));
		}, error);
	}

	downloadBinary (url: string, success: (data: Uint8Array) => void, error: DownloaderErrorCallback) {
		if (this.start(url, success, error)) return;

		const rawDataUri = this.rawDataUris[url];
		// we assume if a "." is included in a raw data uri, it is used to rewrite an asset URL
		if (rawDataUri && !rawDataUri.includes(".")) {
			try {
				this.finish(url, 200, this.dataUriToUint8Array(rawDataUri));
			} catch (e) {
				this.finish(url, 400, JSON.stringify(e));
			}
			return;
		}

		const request = new XMLHttpRequest();
		request.open("GET", rawDataUri ? rawDataUri : url, true);
		request.responseType = "arraybuffer";
		const onerror = () => {
			this.finish(url, request.status, request.response);
		};
		request.onload = () => {
			if (request.status === 200 || request.status === 0)
				this.finish(url, 200, new Uint8Array(request.response as ArrayBuffer));
			else
				onerror();
		};
		request.onerror = onerror;
		request.send();
	}

	private start<T extends AssetData> (url: string, success: DownloaderSuccessCallback<T>, error: DownloaderErrorCallback) {
		let callbacks = this.callbacks[url];
		try {
			if (callbacks) return true;
			this.callbacks[url] = callbacks = [];
		} finally {
			callbacks.push(success as DownloaderSuccessCallback<AssetData>, error);
		}
	}

	private finish (url: string, status: number, data: AssetData) {
		const callbacks = this.callbacks[url];
		delete this.callbacks[url];
		if (status === 200 || status === 0) {
			for (let i = 0, n = callbacks.length; i < n; i += 2)
				(callbacks[i] as DownloaderSuccessCallback)(data);
		} else {
			for (let i = 1, n = callbacks.length; i < n; i += 2)
				(callbacks[i] as DownloaderErrorCallback)(status, data as string);
		}
	}
}
