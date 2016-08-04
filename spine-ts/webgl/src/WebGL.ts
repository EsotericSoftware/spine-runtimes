module spine.webgl {
    export var gl: WebGLRenderingContext;

    export function init(gl: WebGLRenderingContext) {
        if (!gl || !(gl instanceof WebGLRenderingContext)) throw Error("Expected a WebGLRenderingContext");
        spine.webgl.gl = gl;
    }

    export function getSourceGLBlendMode(blendMode: BlendMode, premultipliedAlpha: boolean = false) {
        switch(blendMode) {
            case BlendMode.Normal: return premultipliedAlpha? gl.ONE : gl.SRC_ALPHA;
            case BlendMode.Additive: return premultipliedAlpha? gl.ONE : gl.SRC_ALPHA;
            case BlendMode.Multiply: return gl.DST_COLOR;
            case BlendMode.Screen: return gl.ONE;
            default: throw new Error("Unknown blend mode: " + blendMode);
        }
    }

    export function getDestGLBlendMode(blendMode: BlendMode) {
        switch(blendMode) {
            case BlendMode.Normal: return gl.ONE_MINUS_SRC_ALPHA;
            case BlendMode.Additive: return gl.ONE;
            case BlendMode.Multiply: return gl.ONE_MINUS_SRC_ALPHA;
            case BlendMode.Screen: return gl.ONE_MINUS_SRC_ALPHA;
            default: throw new Error("Unknown blend mode: " + blendMode);
        }
    }
}