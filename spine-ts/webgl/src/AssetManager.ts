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

module spine.webgl {
	export class AssetManager implements Disposable {
		private _assets: Map<string | Texture> = {};        
		private _errors: Map<string> = {};
		private _toLoad = 0;
		private _loaded = 0;
		
		loadText(path: string, 
				 success: (path: string, text: string) => void, 
				 error: (path: string, error: string) => void) {
			this._toLoad++;
			let request = new XMLHttpRequest();
			request.onreadystatechange = () => {
				if (request.readyState == XMLHttpRequest.DONE) {
					if (request.status >= 200 && request.status < 300) {
						if (success) success(path, request.responseText);                        
						this._assets[path] = request.responseText;
					} else {
						if (error) error(path, `Couldn't load text ${path}: status ${request.status}, ${request.responseText}`);                        
						this._errors[path] = `Couldn't load text ${path}: status ${request.status}, ${request.responseText}`;
					}
					this._toLoad--;
					this._loaded++;
				}
			};
			request.open("GET", path, true);
			request.send();
		}        

		loadTexture(path: string,
				  success: (path: string, image: HTMLImageElement) => void, 
				  error: (path: string, error: string) => void) {
			this._toLoad++;
			let img = new Image();
			img.src = path;
			img.onload = (ev) => {
				if (success) success(path, img);
				let texture = new Texture(img);
				this._assets[path] = texture; 
				this._toLoad--;
				this._loaded++;
			}
			img.onerror = (ev) => {
				this._errors[path] =  `Couldn't load image ${path}`;
				this._toLoad--;
				this._loaded++;
			}
		}

		get(path: string) {
			return this._assets[path];
		}

		remove(path: string) {
			let asset = this._assets[path];
			if (asset instanceof Texture) {
				asset.dispose();
			}
			this._assets[path] = null; 
		}

		removeAll() {
			for (var key in this._assets) {
				let asset = this._assets[key];
				if (asset instanceof Texture) asset.dispose();                
			}            
			this._assets = {};
		}

		isLoadingComplete(): boolean {
			return this._toLoad == 0;
		}

		toLoad(): number {
			return this._toLoad;
		}

		loaded(): number {
			return this._loaded;
		}

		dispose() {
			this.removeAll();
		}  
	}
}
