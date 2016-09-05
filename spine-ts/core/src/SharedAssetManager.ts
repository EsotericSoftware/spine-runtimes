/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.5
 * 
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
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
			var i = 0;
			for (var v in this.assets) i++;
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
			var clientAssets = this.clientAssets[clientId];
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
			img.src = path;			
			img.crossOrigin = "anonymous";
			img.onload = (ev) => {								
				this.rawAssets[path] = img;				
			}
			img.onerror = (ev) => {				
				this.errors[path] =  `Couldn't load image ${path}`;				
			}
		}

		get (clientId: string, path: string) {
			path = this.pathPrefix + path;
			var clientAssets = this.clientAssets[clientId];
			if (clientAssets === null || clientAssets === undefined) return true;
			return clientAssets.assets[path];			
		}

		private updateClientAssets(clientAssets: Assets): void {
			for (var i = 0; i < clientAssets.toLoad.length; i++) {
				var path = clientAssets.toLoad[i];
				var asset = clientAssets.assets[path];
				if (asset === null || asset === undefined) {
					var rawAsset = this.rawAssets[path];
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
			var clientAssets = this.clientAssets[clientId];
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
