declare module spine.webgl {
    class AssetLoader {
    }
}
declare module spine.webgl {
    class Mesh {
        private _attributes;
        private _vertices;
        private _indices;
        private _elementsPerVertex;
        attributes(): VertexAttribute[];
        vertices(): Float32Array;
        maxVertices(): number;
        indices(): Int16Array;
        maxIndices(): number;
        constructor(_attributes: VertexAttribute[], maxVertices: number, maxIndices: number);
        renderWithOffset(shader: Shader, primitiveType: number, offset: number, count: number): void;
    }
    class VertexAttribute {
        name: string;
        type: VertexAttributeType;
        numElements: number;
        constructor(name: string, type: VertexAttributeType, numElements: number);
    }
    enum VertexAttributeType {
        Float = 0,
    }
}
declare module spine.webgl {
    class PolygonBatch {
    }
}
declare module spine.webgl {
    class Shader {
        private _vertexShader;
        private _fragmentShader;
        static POSITION: string;
        static COLOR: string;
        static TEXCOORD: string;
        private _vs;
        private _fs;
        private _program;
        program(): WebGLProgram;
        vertexShader(): string;
        fragmentShader(): string;
        constructor(_vertexShader: string, _fragmentShader: string);
        private compile();
        private compileShader(type, source);
        private compileProgram(vs, fs);
        dispose(): void;
        static newDefaultShader(): Shader;
    }
}
declare module spine.webgl {
    var gl: WebGLRenderingContext;
    function init(gl: WebGLRenderingContext): void;
}
