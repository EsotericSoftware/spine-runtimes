/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

module spine {
	export class AssetManager implements Disposable {
		private pathPrefix: string;
		private textureLoader: (image: HTMLImageElement | ImageBitmap) => any;
		private downloader: Downloader;
		private assets: Map<any> = {};
		private errors: Map<string> = {};
		private toLoad = 0;
		private loaded = 0;

		constructor (textureLoader: (image: HTMLImageElement | ImageBitmap) => any, pathPrefix: string = "", downloader: Downloader = null) {
			this.textureLoader = textureLoader;
			this.pathPrefix = pathPrefix;
			this.downloader = downloader || new Downloader();
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

		setRawDataURI(path: string, data: string) {
			this.downloader.rawDataUris[this.pathPrefix + path] = data;
		}

		loadBinary(path: string,
			success: (path: string, binary: Uint8Array) => void = null,
			error: (path: string, message: string) => void = null) {
			path = this.start(path);

			this.downloader.downloadBinary(path, (data: Uint8Array): void => {
				this.success(success, path, data);
			}, (status: number, responseText: string): void => {
				this.error(error, path, `Couldn't load binary ${path}: status ${status}, ${responseText}`);
			});
		}

		loadText(path: string,
			success: (path: string, text: string) => void = null,
			error: (path: string, message: string) => void = null) {
			path = this.start(path);

			this.downloader.downloadText(path, (data: string): void => {
				this.success(success, path, data);
			}, (status: number, responseText: string): void => {
				this.error(error, path, `Couldn't load text ${path}: status ${status}, ${responseText}`);
			});
		}

		loadJson(path: string,
			success: (path: string, object: object) => void = null,
			error: (path: string, message: string) => void = null) {
			path = this.start(path);

			this.downloader.downloadJson(path, (data: object): void => {
				this.success(success, path, data);
			}, (status: number, responseText: string): void => {
				this.error(error, path, `Couldn't load JSON ${path}: status ${status}, ${responseText}`);
			});
		}

		loadTexture (path: string,
			success: (path: string, image: HTMLImageElement | ImageBitmap) => void = null,
			error: (path: string, message: string) => void = null) {
			path = this.start(path);

			let isBrowser = !!(typeof window !== 'undefined' && typeof navigator !== 'undefined' && window.document);
			let isWebWorker = !isBrowser && typeof importScripts !== 'undefined';
			if (isWebWorker) {
				fetch(path, {mode: <RequestMode>"cors"}).then( (response) => {
					if (response.ok) return response.blob();
					this.error(error, path, `Couldn't load image: ${path}`);
					return null;
				}).then( (blob) => {
					return blob ? createImageBitmap(blob, { premultiplyAlpha: "none", colorSpaceConversion: "none" }) : null;
				}).then( (bitmap) => {
					if (bitmap) this.success(success, path, this.textureLoader(bitmap));
				});
			} else {
				let image = new Image();
				image.crossOrigin = "anonymous";
				image.onload = () => {
					this.success(success, path, this.textureLoader(image));
				};
				image.onerror = () => {
					this.error(error, path, `Couldn't load image: ${path}`);
				};
				if (this.downloader.rawDataUris[path]) path = this.downloader.rawDataUris[path];
				image.src = path;
			}
		}

		loadTextureAtlas (path: string,
			success: (path: string, atlas: TextureAtlas) => void = null,
			error: (path: string, message: string) => void = null
		) {
			let parent = path.lastIndexOf("/") >= 0 ? path.substring(0, path.lastIndexOf("/")) : "";
			path = this.start(path);

			this.downloader.downloadText(path, (atlasData: string): void => {
				let pagesLoaded = 0;
				let atlasPages = new Array<string>();
				try {
					let atlas = new TextureAtlas(atlasData, (path: string) => {
						atlasPages.push(parent == "" ? path : parent + "/" + path);
						let image = document.createElement("img") as HTMLImageElement;
						image.width = 16;
						image.height = 16;
						return new FakeTexture(image);
					});
					for (let atlasPage of atlasPages) {
						let pageLoadError = false;
						this.loadTexture(atlasPage, (imagePath: string, image: HTMLImageElement | ImageBitmap) => {
							pagesLoaded++;
							if (pagesLoaded == atlasPages.length) {
								if (!pageLoadError) {
									try {
										this.success(success, path, new TextureAtlas(atlasData, (path: string) => {
											return this.get(parent == "" ? path : parent + "/" + path);
										}));
									} catch (e) {
										this.error(error, path, `Couldn't load texture atlas ${path}: ${e.message}`);
									}
								} else
									this.error(error, path, `Couldn't load texture atlas ${path} page: ${imagePath}`);
							}
						}, (imagePath: string, errorMessage: string) => {
							pageLoadError = true;
							pagesLoaded++;
							if (pagesLoaded == atlasPages.length)
								this.error(error, path, `Couldn't load texture atlas ${path} page: ${imagePath}`);
						});
					}
				} catch (e) {
					this.error(error, path, `Couldn't load texture atlas ${path}: ${e.message}`);
				}
			}, (status: number, responseText: string): void => {
				this.error(error, path, `Couldn't load texture atlas ${path}: status ${status}, ${responseText}`);
			});
		}

		get (path: string) {
			return this.assets[this.pathPrefix + path];
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

		hasErrors() {
			return Object.keys(this.errors).length > 0;
		}

		getErrors() {
			return this.errors;
		}
	}

	export class Downloader {
		private callbacks: Map<Array<Function>> = {};
		rawDataUris: Map<string> = {};

		downloadText (url: string, success: (data: string) => void, error: (status: number, responseText: string) => void) {
			if (this.rawDataUris[url]) url = this.rawDataUris[url];
			if (this.start(url, success, error)) return;
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
			if (this.rawDataUris[url]) url = this.rawDataUris[url];
			if (this.start(url, success, error)) return;
			let request = new XMLHttpRequest();
			request.open("GET", url, true);
			request.responseType = "arraybuffer";
			let onerror = () => {
				this.finish(url, request.status, request.responseText);
			};
			request.onload = () => {
				if (request.status == 200)
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
			let args = status == 200 ? [data] : [status, data];
			for (let i = args.length - 1, n = callbacks.length; i < n; i += 2)
				callbacks[i].apply(null, args);
		}
	}
}
