module spine.webgl {
    export class Texture {
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

        update(useMipMaps: boolean) {
            this.bind();
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, this._image);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, useMipMaps? gl.LINEAR_MIPMAP_LINEAR: gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
            if (useMipMaps) gl.generateMipmap(gl.TEXTURE_2D);
            this.unbind();
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
    }
}