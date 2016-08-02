module spine.webgl {
    export var gl: WebGLRenderingContext;

    export function init(gl: WebGLRenderingContext) {
        if (!gl || !(gl instanceof WebGLRenderingContext)) throw Error("Expected a WebGLRenderingContext");
        spine.webgl.gl = gl;
    }
}