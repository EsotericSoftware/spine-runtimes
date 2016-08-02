declare module spine.webgl {
    class AssetManager {
        private _assets;
        private _errors;
        private _toLoad;
        private _loaded;
        loadText(path: string, success: (path: string, text: string) => void, error: (path: string, error: string) => void): void;
        loadTexture(path: string, success: (path: string, image: HTMLImageElement) => void, error: (path: string, error: string) => void): void;
        get(path: string): string | Texture;
        remove(path: string): void;
        removeAll(): void;
        isLoadingComplete(): boolean;
        toLoad(): number;
        loaded(): number;
    }
}
declare module spine.webgl {
    let M00: number;
    let M01: number;
    let M02: number;
    let M03: number;
    let M10: number;
    let M11: number;
    let M12: number;
    let M13: number;
    let M20: number;
    let M21: number;
    let M22: number;
    let M23: number;
    let M30: number;
    let M31: number;
    let M32: number;
    let M33: number;
    class Matrix4 {
        temp: Float32Array;
        values: Float32Array;
        constructor();
        set(values: Float32Array | Array<number>): Matrix4;
        transpose(): Matrix4;
        identity(): Matrix4;
        invert(): Matrix4;
        determinant(): number;
        translate(x: number, y: number, z: number): Matrix4;
        copy(): Matrix4;
        projection(near: number, far: number, fovy: number, aspectRatio: number): Matrix4;
        ortho2d(x: number, y: number, width: number, height: number): Matrix4;
        ortho(left: number, right: number, bottom: number, top: number, near: number, far: number): Matrix4;
        multiply(matrix: Matrix4): Matrix4;
        multiplyLeft(matrix: Matrix4): Matrix4;
    }
}
declare module spine.webgl {
    class Mesh {
        private _attributes;
        private _vertices;
        private _verticesBuffer;
        private _numVertices;
        private _dirtyVertices;
        private _indices;
        private _indicesBuffer;
        private _numIndices;
        private _dirtyIndices;
        private _elementsPerVertex;
        attributes(): VertexAttribute[];
        maxVertices(): number;
        numVertices(): number;
        maxIndices(): number;
        numIndices(): number;
        constructor(_attributes: VertexAttribute[], maxVertices: number, maxIndices: number);
        setVertices(vertices: Array<number>): void;
        setIndices(indices: Array<number>): void;
        render(shader: Shader, primitiveType: number): void;
        renderWithOffset(shader: Shader, primitiveType: number, offset: number, count: number): void;
        bind(shader: Shader): void;
        unbind(shader: Shader): void;
        private update();
    }
    class VertexAttribute {
        name: string;
        type: VertexAttributeType;
        numElements: number;
        constructor(name: string, type: VertexAttributeType, numElements: number);
    }
    class Position2Attribute extends VertexAttribute {
        constructor();
    }
    class Position3Attribute extends VertexAttribute {
        constructor();
    }
    class TexCoordAttribute extends VertexAttribute {
        constructor(unit?: number);
    }
    class ColorAttribute extends VertexAttribute {
        constructor();
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
        static MVP_MATRIX: string;
        static POSITION: string;
        static COLOR: string;
        static TEXCOORDS: string;
        static SAMPLER: string;
        private _vs;
        private _fs;
        private _program;
        private _tmp2x2;
        private _tmp3x3;
        private _tmp4x4;
        program(): WebGLProgram;
        vertexShader(): string;
        fragmentShader(): string;
        constructor(_vertexShader: string, _fragmentShader: string);
        private compile();
        private compileShader(type, source);
        private compileProgram(vs, fs);
        bind(): void;
        unbind(): void;
        setUniformi(uniform: string, value: number): void;
        setUniformf(uniform: string, value: number): void;
        setUniform2f(uniform: string, value: number, value2: number): void;
        setUniform3f(uniform: string, value: number, value2: number, value3: number): void;
        setUniform4f(uniform: string, value: number, value2: number, value3: number, value4: number): void;
        setUniform2x2f(uniform: string, value: Array<number> | Float32Array): void;
        setUniform3x3f(uniform: string, value: Array<number> | Float32Array): void;
        setUniform4x4f(uniform: string, value: Array<number> | Float32Array): void;
        getUniformLocation(uniform: string): WebGLUniformLocation;
        getAttributeLocation(attribute: string): number;
        dispose(): void;
        static newColoredTextured(): Shader;
        static newColored(): Shader;
    }
}
declare module spine.webgl {
    class Texture {
        private _texture;
        private _image;
        private _boundUnit;
        constructor(image: HTMLImageElement, useMipMaps?: boolean);
        getImage(): HTMLImageElement;
        update(useMipMaps: boolean): void;
        bind(unit?: number): void;
        unbind(): void;
        dispose(): void;
    }
}
declare module spine.webgl {
    class Vector3 {
        x: number;
        y: number;
        z: number;
        set(x: number, y: number, z: number): Vector3;
        add(v: Vector3): Vector3;
        sub(v: Vector3): Vector3;
        scale(s: number): Vector3;
        normalize(): Vector3;
        cross(v: Vector3): Vector3;
        multiply(matrix: Matrix4): Vector3;
        project(matrix: Matrix4): Vector3;
        dot(v: Vector3): number;
        length(): number;
        distance(v: Vector3): number;
    }
}
declare module spine.webgl {
    interface Map<T> {
        [key: string]: T;
    }
    var gl: WebGLRenderingContext;
    function init(gl: WebGLRenderingContext): void;
}
