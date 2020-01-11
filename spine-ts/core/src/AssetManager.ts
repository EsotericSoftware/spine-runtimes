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
		private textureLoader: (image: HTMLImageElement) => any;
		private assets: Map<any> = {};
		private errors: Map<string> = {};
		private toLoad = 0;
		private loaded = 0;
		private rawDataUris: Map<string> = {};

		constructor (textureLoader: (image: HTMLImageElement) => any, pathPrefix: string = "") {
			this.textureLoader = textureLoader;
			this.pathPrefix = pathPrefix;
		}

		private downloadText (url: string, success: (data: string) => void, error: (status: number, responseText: string) => void) {
			let request = new XMLHttpRequest();
			request.overrideMimeType("text/html");
			if (this.rawDataUris[url]) url = this.rawDataUris[url];
			request.open("GET", url, true);
			request.onload = () => {
				if (request.status == 200) {
					success(request.responseText);
				} else {
					error(request.status, request.responseText);
				}
			}
			request.onerror = () => {
				error(request.status, request.responseText);
			}
			request.send();
		}

		private downloadBinary (url: string, success: (data: Uint8Array) => void, error: (status: number, responseText: string) => void) {
			let request = new XMLHttpRequest();
			if (this.rawDataUris[url]) url = this.rawDataUris[url];
			request.open("GET", url, true);
			request.responseType = "arraybuffer";
			request.onload = () => {
				if (request.status == 200) {
					success(new Uint8Array(request.response as ArrayBuffer));
				} else {
					error(request.status, request.responseText);
				}
			}
			request.onerror = () => {
				error(request.status, request.responseText);
			}
			request.send();
		}

		setRawDataURI(path: string, data: string) {
			this.rawDataUris[this.pathPrefix + path] = data;
		}

		loadBinary(path: string,
			success: (path: string, binary: Uint8Array) => void = null,
			error: (path: string, error: string) => void = null) {
			path = this.pathPrefix + path;
			this.toLoad++;

			this.downloadBinary(path, (data: Uint8Array): void => {
				this.assets[path] = data;
				if (success) success(path, data);
				this.toLoad--;
				this.loaded++;
			}, (state: number, responseText: string): void => {
				this.errors[path] = `Couldn't load binary ${path}: status ${status}, ${responseText}`;
				if (error) error(path, `Couldn't load binary ${path}: status ${status}, ${responseText}`);
				this.toLoad--;
				this.loaded++;
			});
		}

		loadText(path: string,
			success: (path: string, text: string) => void = null,
			error: (path: string, error: string) => void = null) {
			path = this.pathPrefix + path;
			this.toLoad++;

			this.downloadText(path, (data: string): void => {
				this.assets[path] = data;
				if (success) success(path, data);
				this.toLoad--;
				this.loaded++;
			}, (state: number, responseText: string): void => {
				this.errors[path] = `Couldn't load text ${path}: status ${status}, ${responseText}`;
				if (error) error(path, `Couldn't load text ${path}: status ${status}, ${responseText}`);
				this.toLoad--;
				this.loaded++;
			});
		}

		loadTexture (path: string,
			success: (path: string, image: HTMLImageElement) => void = null,
			error: (path: string, error: string) => void = null) {
			path = this.pathPrefix + path;
			let storagePath = path;
			this.toLoad++;
			let img = new Image();
			img.crossOrigin = "anonymous";
			img.onload = (ev) => {
				let texture = this.textureLoader(img);
				this.assets[storagePath] = texture;
				this.toLoad--;
				this.loaded++;
				if (success) success(path, img);
			}
			img.onerror = (ev) => {
				this.errors[path] = `Couldn't load image ${path}`;
				this.toLoad--;
				this.loaded++;
				if (error) error(path, `Couldn't load image ${path}`);
			}
			if (this.rawDataUris[path]) path = this.rawDataUris[path];
			img.src = path;
		}

		loadTextureAtlas (path: string,
			success: (path: string, atlas: TextureAtlas) => void = null,
			error: (path: string, error: string) => void = null
		) {
			let parent = path.lastIndexOf("/") >= 0 ? path.substring(0, path.lastIndexOf("/")) : "";
			path = this.pathPrefix + path;
			this.toLoad++;

			this.downloadText(path, (atlasData: string): void => {
				let pagesLoaded: any = { count: 0 };
				let atlasPages = new Array<string>();
				try {
					let atlas = new TextureAtlas(atlasData, (path: string) => {
						atlasPages.push(parent == "" ? path : parent + "/" + path);
						let image = document.createElement("img") as HTMLImageElement;
						image.width = 16;
						image.height = 16;
						return new FakeTexture(image);
					});
				} catch (e) {
					let ex = e as Error;
					this.errors[path] = `Couldn't load texture atlas ${path}: ${ex.message}`;
					if (error) error(path, `Couldn't load texture atlas ${path}: ${ex.message}`);
					this.toLoad--;
					this.loaded++;
					return;
				}

				for (let atlasPage of atlasPages) {
					let pageLoadError = false;
					this.loadTexture(atlasPage, (imagePath: string, image: HTMLImageElement) => {
						pagesLoaded.count++;

						if (pagesLoaded.count == atlasPages.length) {
							if (!pageLoadError) {
								try {
									let atlas = new TextureAtlas(atlasData, (path: string) => {
										return this.get(parent == "" ? path : parent + "/" + path);
									});
									this.assets[path] = atlas;
									if (success) success(path, atlas);
									this.toLoad--;
									this.loaded++;
								} catch (e) {
									let ex = e as Error;
									this.errors[path] = `Couldn't load texture atlas ${path}: ${ex.message}`;
									if (error) error(path, `Couldn't load texture atlas ${path}: ${ex.message}`);
									this.toLoad--;
									this.loaded++;
								}
							} else {
								this.errors[path] = `Couldn't load texture atlas page ${imagePath}} of atlas ${path}`;
								if (error) error(path, `Couldn't load texture atlas page ${imagePath} of atlas ${path}`);
								this.toLoad--;
								this.loaded++;
							}
						}
					}, (imagePath: string, errorMessage: string) => {
						pageLoadError = true;
						pagesLoaded.count++;

						if (pagesLoaded.count == atlasPages.length) {
							this.errors[path] = `Couldn't load texture atlas page ${imagePath}} of atlas ${path}`;
							if (error) error(path, `Couldn't load texture atlas page ${imagePath} of atlas ${path}`);
							this.toLoad--;
							this.loaded++;
						}
					});
				}
			}, (state: number, responseText: string): void => {
				this.errors[path] = `Couldn't load texture atlas ${path}: status ${status}, ${responseText}`;
				if (error) error(path, `Couldn't load texture atlas ${path}: status ${status}, ${responseText}`);
				this.toLoad--;
				this.loaded++;
			});
		}

		get (path: string) {
			path = this.pathPrefix + path;
			return this.assets[path];
		}

		remove (path: string) {
			path = this.pathPrefix + path;
			let asset = this.assets[path];
			if ((<any>asset).dispose) (<any>asset).dispose();
			this.assets[path] = null;
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
}
