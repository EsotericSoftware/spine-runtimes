declare module spine.webgl {
    class AssetLoader {
    }
}
declare module spine.webgl {
    class Mesh {
        private _attributes;
        constructor(_attributes: VertexAttribute[]);
        attributes(): VertexAttribute[];
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
        constructor(_vertexShader: string, _fragmentShader: string);
        private compile();
        private compileShader(type, source);
        private compileProgram(vs, fs);
        dispose(): void;
        program(): WebGLProgram;
        vertexShader(): string;
        fragmentShader(): string;
        static newDefaultShader(): Shader;
    }
}
declare module spine.webgl {
    var gl: WebGLRenderingContext;
    function init(gl: WebGLRenderingContext): void;
}
