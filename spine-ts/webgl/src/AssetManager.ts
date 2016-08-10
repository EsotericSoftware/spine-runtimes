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
