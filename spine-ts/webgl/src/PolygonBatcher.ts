module spine.webgl {
    export class PolygonBatcher {
        private _drawCalls = 0;
        private _drawing = false;
        private _mesh: Mesh;
        private _shader: Shader = null;
        private _lastTexture: Texture = null;
        private _verticesLength: number = 0;
        private _indicesLength: number = 0;
        private _srcBlend: number = gl.SRC_ALPHA;
        private _dstBlend: number = gl.ONE_MINUS_SRC_ALPHA;

        constructor(maxVertices: number = 10920) {
            if (maxVertices > 10920) throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);
            this._mesh = new Mesh([new Position2Attribute(), new ColorAttribute(), new TexCoordAttribute()], maxVertices, maxVertices * 3);

        }

        begin(shader: Shader) {
            if (this._drawing) throw new Error("PolygonBatch is already drawing. Call PolygonBatch.end() before calling PolygonBatch.begin()");
            this._drawCalls = 0;
            this._shader = shader;
            this._lastTexture = null;
            this._drawing = true;

            gl.enable(gl.BLEND);
            gl.blendFunc(this._srcBlend, this._dstBlend);      
        }

        setBlendMode(srcBlend: number, dstBlend: number) {
            this._srcBlend = srcBlend;
            this._dstBlend = dstBlend;
            if (this._drawing) {
               this.flush();
               gl.blendFunc(this._srcBlend, this._dstBlend);
            }            
        }

        draw (texture: Texture, vertices: Array<number>, indices: Array<number>) {
            if (texture != this._lastTexture) {
                this.flush();
                this._lastTexture = texture;
                texture.bind();
            } else if (this._verticesLength + vertices.length > this._mesh.vertices().length ||
                       this._indicesLength + indices.length > this._mesh.indices().length) {
                this.flush();
            }
            
            let indexStart = this._mesh.numVertices();
            this._mesh.vertices().set(vertices, this._verticesLength);
            this._verticesLength += vertices.length;
            this._mesh.setVerticesLength(this._verticesLength)

            
            let indicesArray = this._mesh.indices();
            for (let i = this._indicesLength, j = 0; j < indices.length; i++, j++) {
                indicesArray[i] = indices[j] + indexStart;
            }
            this._indicesLength += indices.length;
            this._mesh.setIndicesLength(this._indicesLength);            
        }

        private flush() {
            if (this._verticesLength == 0) return;

            this._mesh.draw(this._shader, gl.TRIANGLES);

            this._verticesLength = 0;
            this._indicesLength = 0;
            this._mesh.setVerticesLength(0);
            this._mesh.setIndicesLength(0);
            this._drawCalls++;
        }

        end() {
            if (!this._drawing) throw new Error("PolygonBatch is not drawing. Call PolygonBatch.begin() before calling PolygonBatch.end()");
            if (this._verticesLength > 0 || this._indicesLength > 0) this.flush();
            this._shader = null;
            this._lastTexture = null;
            this._drawing = false;

            gl.disable(gl.BLEND);     
        }

        drawCalls() { return this._drawCalls; }
    }
}