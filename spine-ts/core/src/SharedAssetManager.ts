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
	class Assets {
		clientId: string;
		toLoad = new Array<string>();
		assets: Map<any> = {};
		textureLoader: (image: HTMLImageElement) => any;

		constructor(clientId: string) {
			this.clientId = clientId;
		}

		loaded() {
			let i = 0;
			for (let v in this.assets) i++;
			return i;
		}
	}

	export class SharedAssetManager implements Disposable {
		private pathPrefix: string;
		private clientAssets: Map<Assets> = {};
		private queuedAssets: Map<string> = {};
		private rawAssets: Map<any> = {}
		private errors: Map<string> = {};

		constructor (pathPrefix: string = "") {
			this.pathPrefix = pathPrefix;
		}

		private queueAsset(clientId: string, textureLoader: (image: HTMLImageElement) => any, path: string): boolean {
			let clientAssets = this.clientAssets[clientId];
			if (clientAssets === null || clientAssets === undefined) {
				clientAssets = new Assets(clientId);
				this.clientAssets[clientId] = clientAssets;
			}
			if (textureLoader !== null) clientAssets.textureLoader = textureLoader;
			clientAssets.toLoad.push(path);

			// check if already queued, in which case we can skip actual
			// loading
			if (this.queuedAssets[path] === path) {
				return false;
			} else {
				this.queuedAssets[path] = path;
				return true;
			}
		}

		loadText(clientId: string, path: string) {
			path = this.pathPrefix + path;
			if (!this.queueAsset(clientId, null, path)) return;
			let request = new XMLHttpRequest();
			request.overrideMimeType("text/html");
			request.onreadystatechange = () => {
				if (request.readyState == XMLHttpRequest.DONE) {
					if (request.status >= 200 && request.status < 300) {
						this.rawAssets[path] = request.responseText;
					} else {
						this.errors[path] = `Couldn't load text ${path}: status ${request.status}, ${request.responseText}`;
					}
				}
			};
			request.open("GET", path, true);
			request.send();
		}

		loadJson(clientId: string, path: string) {
			path = this.pathPrefix + path;
			if (!this.queueAsset(clientId, null, path)) return;
			let request = new XMLHttpRequest();
			request.overrideMimeType("text/html");
			request.onreadystatechange = () => {
				if (request.readyState == XMLHttpRequest.DONE) {
					if (request.status >= 200 && request.status < 300) {
						this.rawAssets[path] = JSON.parse(request.responseText);
					} else {
						this.errors[path] = `Couldn't load text ${path}: status ${request.status}, ${request.responseText}`;
					}
				}
			};
			request.open("GET", path, true);
			request.send();
		}

		loadTexture (clientId: string, textureLoader: (image: HTMLImageElement) => any, path: string) {
			path = this.pathPrefix + path;
			if (!this.queueAsset(clientId, textureLoader, path)) return;

			let img = new Image();
			img.crossOrigin = "anonymous";
			img.onload = (ev) => {
				this.rawAssets[path] = img;
			}
			img.onerror = (ev) => {
				this.errors[path] = `Couldn't load image ${path}`;
			}
			img.src = path;
		}

		get (clientId: string, path: string) {
			path = this.pathPrefix + path;
			let clientAssets = this.clientAssets[clientId];
			if (clientAssets === null || clientAssets === undefined) return true;
			return clientAssets.assets[path];
		}

		private updateClientAssets(clientAssets: Assets): void {
			for (let i = 0; i < clientAssets.toLoad.length; i++) {
				let path = clientAssets.toLoad[i];
				let asset = clientAssets.assets[path];
				if (asset === null || asset === undefined) {
					let rawAsset = this.rawAssets[path];
					if (rawAsset === null || rawAsset === undefined) continue;
					if (rawAsset instanceof HTMLImageElement) {
						clientAssets.assets[path] = clientAssets.textureLoader(<HTMLImageElement>rawAsset);
					} else {
						clientAssets.assets[path] = rawAsset;
					}
				}
			}
		}

		isLoadingComplete (clientId: string): boolean {
			let clientAssets = this.clientAssets[clientId];
			if (clientAssets === null || clientAssets === undefined) return true;
			this.updateClientAssets(clientAssets);
			return clientAssets.toLoad.length == clientAssets.loaded();

		}

		/*remove (clientId: string, path: string) {
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
		}*/

		dispose () {
			// this.removeAll();
		}

		hasErrors() {
			return Object.keys(this.errors).length > 0;
		}

		getErrors() {
			return this.errors;
		}
	}
}
