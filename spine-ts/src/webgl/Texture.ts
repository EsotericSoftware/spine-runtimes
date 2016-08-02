module spine.webgl {
    export class Texture implements Disposable {
        private _texture: WebGLTexture;
        private _image: HTMLImageElement;
        private _boundUnit: number = 0;

        constructor(image: HTMLImageElement, useMipMaps: boolean = false) {
            this._texture = gl.createTexture();
            this._image = image;
            this.update(useMipMaps);                   
        }

        getImage(): HTMLImageElement {
            return this._image;
        }

        setFilters(minFilter: TextureFilter, magFilter: TextureFilter) {
            this.bind();
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, minFilter);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, magFilter);            
        }

        setWraps(uWrap: TextureWrap, vWrap: TextureWrap) {
            this.bind();
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, uWrap);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, vWrap);
        }

        update(useMipMaps: boolean) {
            this.bind();
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, this._image);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, useMipMaps? gl.LINEAR_MIPMAP_LINEAR: gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
            if (useMipMaps) gl.generateMipmap(gl.TEXTURE_2D);            
        }

        bind(unit: number = 0) {
            this._boundUnit = unit;
            gl.activeTexture(gl.TEXTURE0 + unit);        
            gl.bindTexture(gl.TEXTURE_2D, this._texture);
        }

        unbind() {
            gl.activeTexture(gl.TEXTURE0 + this._boundUnit);
            gl.bindTexture(gl.TEXTURE_2D, null);
        }

        dispose() {
            gl.deleteTexture(this._texture);
        }

        public static filterFromString(text: string): TextureFilter {
            switch (text.toLowerCase()) {
                case "nearest": return TextureFilter.Nearest;
                case "linear": return TextureFilter.Linear;
                case "mipmap": return TextureFilter.MipMap;
                case "mipmapnearestnearest": return TextureFilter.MipMapNearestNearest;
                case "mipmaplinearnearest": return TextureFilter.MipMapLinearNearest;
                case "mipmapnearestlinear": return TextureFilter.MipMapNearestLinear;
                case "mipmaplinearlinear": return TextureFilter.MipMapLinearLinear;
                default: throw new Error(`Unknown texture filter ${text}`);
            }
        }

        public static wrapFromString(text: string): TextureWrap {
            switch (text.toLowerCase()) {
                case "mirroredtepeat": return TextureWrap.MirroredRepeat;
                case "clamptoedge": return TextureWrap.ClampToEdge;
                case "repeat": return TextureWrap.Repeat;
                default: throw new Error(`Unknown texture wrap ${text}`);
            }
        }
    }    

    export enum TextureFilter {
        Nearest = WebGLRenderingContext.NEAREST,
        Linear = WebGLRenderingContext.LINEAR,
        MipMap = WebGLRenderingContext.LINEAR_MIPMAP_LINEAR,
        MipMapNearestNearest = WebGLRenderingContext.NEAREST_MIPMAP_NEAREST,
        MipMapLinearNearest = WebGLRenderingContext.LINEAR_MIPMAP_NEAREST,
        MipMapNearestLinear = WebGLRenderingContext.NEAREST_MIPMAP_LINEAR,
        MipMapLinearLinear = WebGLRenderingContext.LINEAR_MIPMAP_LINEAR        
    }

    export enum TextureWrap {
        MirroredRepeat = WebGLRenderingContext.MIRRORED_REPEAT,
        ClampToEdge = WebGLRenderingContext.CLAMP_TO_EDGE,
        Repeat = WebGLRenderingContext.REPEAT
    }
}