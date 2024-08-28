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

import { Texture } from "./Texture.js";
import { TextureAtlas } from "./TextureAtlas.js";
import { Disposable, StringMap } from "./Utils.js";

export class AssetManagerBase implements Disposable {
	private pathPrefix: string = "";
	private textureLoader: (image: HTMLImageElement | ImageBitmap) => Texture;
	private downloader: Downloader;
	private assets: StringMap<any> = {};
	private assetsLoaded: StringMap<Promise<any>> = {};
	private errors: StringMap<string> = {};
	private toLoad = 0;
	private loaded = 0;

	constructor (textureLoader: (image: HTMLImageElement | ImageBitmap) => Texture, pathPrefix: string = "", downloader: Downloader = new Downloader()) {
		this.textureLoader = textureLoader;
		this.pathPrefix = pathPrefix;
		this.downloader = downloader;
	}

	private start (path: string): string {
		this.toLoad++;
		return this.pathPrefix + path;
	}

	private success (callback: (path: string, data: any) => void, path: string, asset: any) {
		this.toLoad--;
		this.loaded++;
		this.assets[path] = asset;
		if (callback) callback(path, asset);
	}

	private error (callback: (path: string, message: string) => void, path: string, message: string) {
		this.toLoad--;
		this.loaded++;
		this.errors[path] = message;
		if (callback) callback(path, message);
	}

	loadAll () {
		let promise = new Promise((resolve: (assetManager: AssetManagerBase) => void, reject: (errors: StringMap<string>) => void) => {
			let check = () => {
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

		this.assetsLoaded[path] = new Promise<any>((resolve, reject) => {
			this.downloader.downloadBinary(path, (data: Uint8Array): void => {
				// setTimeout(() => this.success(success, path, data), 10000);
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

			this.assetsLoaded[path] = new Promise<any>((resolve, reject) => {
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

	// TODO: refactor assetsLoaded and assets (we should probably merge them)
	reuseAssets(path: string,
		success: (path: string, data: any) => void = () => { },
		error: (path: string, message: string) => void = () => { }) {
		const loadedStatus = this.assetsLoaded[path];
		const alreadyExistsOrLoading = loadedStatus !== undefined;
		if (alreadyExistsOrLoading) {
			loadedStatus
				.then(data => this.success(success, path, data))
				.catch(errorMsg => this.error(error, path, errorMsg));
		}
		return alreadyExistsOrLoading;
	}

	loadTexture (path: string,
		success: (path: string, texture: Texture) => void = () => { },
		error: (path: string, message: string) => void = () => { }) {

			path = this.start(path);

			if (this.reuseAssets(path, success, error)) return;

			this.assetsLoaded[path] = new Promise<any>((resolve, reject) => {
				let isBrowser = !!(typeof window !== 'undefined' && typeof navigator !== 'undefined' && window.document);
				let isWebWorker = !isBrowser; // && typeof importScripts !== 'undefined';
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
							const texture = this.textureLoader(bitmap)
							this.success(success, path, texture);
							resolve(texture);
						};
					});
				} else {
					let image = new Image();
					image.crossOrigin = "anonymous";
					image.onload = () => {
						const texture = this.textureLoader(image)
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

	loadTextureAtlas (path: string,
		success: (path: string, atlas: TextureAtlas) => void = () => { },
		error: (path: string, message: string) => void = () => { },
		fileAlias?: { [keyword: string]: string }
	) {
		let index = path.lastIndexOf("/");
		let parent = index >= 0 ? path.substring(0, index + 1) : "";
		path = this.start(path);

		if (this.reuseAssets(path, success, error)) return;

		this.assetsLoaded[path] = new Promise<any>((resolve, reject) => {
			this.downloader.downloadText(path, (atlasText: string): void => {
				try {
					let atlas = new TextureAtlas(atlasText);
					let toLoad = atlas.pages.length, abort = false;
					for (let page of atlas.pages) {
						this.loadTexture(!fileAlias ? parent + page.name : fileAlias[page.name!],
							(imagePath: string, texture: Texture) => {
								if (!abort) {
									page.setTexture(texture);
									if (--toLoad == 0) {
										this.success(success, path, atlas);
										resolve(atlas);
									}
								}
							},
							(imagePath: string, message: string) => {
								if (!abort) {
									const errorMsg = `Couldn't load texture atlas ${path} page image: ${imagePath}`;
									this.error(error, path, errorMsg);
									reject(errorMsg);
								}
								abort = true;
							}
						);
					}
				} catch (e) {
					const errorMsg = `Couldn't parse texture atlas ${path}: ${(e as any).message}`;
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

	loadTextureAtlasButNoTextures (path: string,
		success: (path: string, atlas: TextureAtlas) => void = () => { },
		error: (path: string, message: string) => void = () => { },
		fileAlias?: { [keyword: string]: string }
	) {
		path = this.start(path);

		if (this.reuseAssets(path, success, error)) return;

		this.assetsLoaded[path] = new Promise<any>((resolve, reject) => {
			this.downloader.downloadText(path, (atlasText: string): void => {
				try {
					const atlas = new TextureAtlas(atlasText);
					this.success(success, path, atlas);
					resolve(atlas);
				} catch (e) {
					const errorMsg = `Couldn't parse texture atlas ${path}: ${(e as any).message}`;
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

	get (path: string) {
		return this.assets[this.pathPrefix + path];
	}

	require (path: string) {
		path = this.pathPrefix + path;
		let asset = this.assets[path];
		if (asset) return asset;
		let error = this.errors[path];
		throw Error("Asset not found: " + path + (error ? "\n" + error : ""));
	}

	remove (path: string) {
		path = this.pathPrefix + path;
		let asset = this.assets[path];
		if ((<any>asset).dispose) (<any>asset).dispose();
		delete this.assets[path];
		return asset;
	}

	removeAll () {
		for (let key in this.assets) {
			let asset = this.assets[key];
			if ((<any>asset).dispose) (<any>asset).dispose();
		}
		this.assets = {};
	}

	isLoadingComplete (): boolean {
		return this.toLoad == 0;
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

	hasErrors () {
		return Object.keys(this.errors).length > 0;
	}

	getErrors () {
		return this.errors;
	}
}

export class Downloader {
	private callbacks: StringMap<Array<Function>> = {};
	rawDataUris: StringMap<string> = {};
	cacheTextures: Record<string, Texture> = {};

	dataUriToString (dataUri: string) {
		if (!dataUri.startsWith("data:")) {
			throw new Error("Not a data URI.");
		}

		let base64Idx = dataUri.indexOf("base64,");
		if (base64Idx != -1) {
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
		for (var i = 0; i < len; i++) {
			bytes[i] = binary_string.charCodeAt(i);
		}
		return bytes;
	}

	dataUriToUint8Array (dataUri: string) {
		if (!dataUri.startsWith("data:")) {
			throw new Error("Not a data URI.");
		}

		let base64Idx = dataUri.indexOf("base64,");
		if (base64Idx == -1) throw new Error("Not a binary data URI.");
		base64Idx += "base64,".length;
		return this.base64ToUint8Array(dataUri.substr(base64Idx));
	}

	downloadText (url: string, success: (data: string) => void, error: (status: number, responseText: string) => void) {
		if (this.start(url, success, error)) return;
		if (this.rawDataUris[url]) {
			try {
				let dataUri = this.rawDataUris[url];
				this.finish(url, 200, this.dataUriToString(dataUri));
			} catch (e) {
				this.finish(url, 400, JSON.stringify(e));
			}
			return;
		}
		let request = new XMLHttpRequest();
		request.overrideMimeType("text/html");
		request.open("GET", url, true);
		let done = () => {
			this.finish(url, request.status, request.responseText);
		};
		request.onload = done;
		request.onerror = done;
		request.send();
	}

	downloadJson (url: string, success: (data: object) => void, error: (status: number, responseText: string) => void) {
		this.downloadText(url, (data: string): void => {
			success(JSON.parse(data));
		}, error);
	}

	downloadBinary (url: string, success: (data: Uint8Array) => void, error: (status: number, responseText: string) => void) {
		if (this.start(url, success, error)) return;
		if (this.rawDataUris[url]) {
			try {
				let dataUri = this.rawDataUris[url];
				this.finish(url, 200, this.dataUriToUint8Array(dataUri));
			} catch (e) {
				this.finish(url, 400, JSON.stringify(e));
			}
			return;
		}
		let request = new XMLHttpRequest();
		request.open("GET", url, true);
		request.responseType = "arraybuffer";
		let onerror = () => {
			this.finish(url, request.status, request.response);
		};
		request.onload = () => {
			if (request.status == 200 || request.status == 0)
				this.finish(url, 200, new Uint8Array(request.response as ArrayBuffer));
			else
				onerror();
		};
		request.onerror = onerror;
		request.send();
	}

	private start (url: string, success: any, error: any) {
		let callbacks = this.callbacks[url];
		try {
			if (callbacks) return true;
			this.callbacks[url] = callbacks = [];
		} finally {
			callbacks.push(success, error);
		}
	}

	private finish (url: string, status: number, data: any) {
		let callbacks = this.callbacks[url];
		delete this.callbacks[url];
		let args = status == 200 || status == 0 ? [data] : [status, data];
		for (let i = args.length - 1, n = callbacks.length; i < n; i += 2)
			callbacks[i].apply(null, args);
	}
}
