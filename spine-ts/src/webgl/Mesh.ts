module spine.webgl {
    export class Mesh {        
        private _vertices:Float32Array;
        private _indices:Int16Array;
        private _elementsPerVertex: number;

        attributes(): VertexAttribute[] { return this._attributes; }
        vertices(): Float32Array { return this._vertices; }
        maxVertices(): number { return this._vertices.length / this._elementsPerVertex; }
        indices(): Int16Array { return this._indices; }
        maxIndices(): number { return this._indices.length; }

        constructor(private _attributes: VertexAttribute[], maxVertices: number, maxIndices: number) {
            this._elementsPerVertex = 0;
            for (var i = 0; i < _attributes.length; i++) {
                this._elementsPerVertex += _attributes[i].numElements;
            }
            this._vertices = new Float32Array(maxVertices * this._elementsPerVertex);
            this._indices = new Int16Array(maxIndices);          
        }

        renderWithOffset(shader: Shader, primitiveType: number, offset: number, count: number) {

        }
    }

    export class VertexAttribute {      
        constructor(public name: string, public type: VertexAttributeType, public numElements: number) { }
    }

    export enum VertexAttributeType {
        Float
    }
}