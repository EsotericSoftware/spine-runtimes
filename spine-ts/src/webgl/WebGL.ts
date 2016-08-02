module spine.webgl {
    export interface Map<T> {
        [key: string]: T;
    }

    export var gl: WebGLRenderingContext;

    export function init(gl: WebGLRenderingContext) {
        if (!gl || !(gl instanceof WebGLRenderingContext)) throw Error("Expected a WebGLRenderingContext");
        spine.webgl.gl = gl;
    }
}