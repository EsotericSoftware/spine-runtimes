module spine.webgl {
    export class Mesh {        
        constructor(private _attributes: VertexAttribute[]) {            
        }

        attributes(): VertexAttribute[] { return this._attributes; }
    }

    export class VertexAttribute {        
        constructor(public name: string, public type: VertexAttributeType, public numElements: number) { }
    }

    export enum VertexAttributeType {
        Float
    }
}