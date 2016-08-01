var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var AssetLoader = (function () {
            function AssetLoader() {
            }
            return AssetLoader;
        }());
        webgl.AssetLoader = AssetLoader;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var Mesh = (function () {
            function Mesh(_attributes, maxVertices, maxIndices) {
                this._attributes = _attributes;
                this._elementsPerVertex = 0;
                for (var i = 0; i < _attributes.length; i++) {
                    this._elementsPerVertex += _attributes[i].numElements;
                }
                this._vertices = new Float32Array(maxVertices * this._elementsPerVertex);
                this._indices = new Int16Array(maxIndices);
            }
            Mesh.prototype.attributes = function () { return this._attributes; };
            Mesh.prototype.vertices = function () { return this._vertices; };
            Mesh.prototype.maxVertices = function () { return this._vertices.length / this._elementsPerVertex; };
            Mesh.prototype.indices = function () { return this._indices; };
            Mesh.prototype.maxIndices = function () { return this._indices.length; };
            Mesh.prototype.renderWithOffset = function (shader, primitiveType, offset, count) {
            };
            return Mesh;
        }());
        webgl.Mesh = Mesh;
        var VertexAttribute = (function () {
            function VertexAttribute(name, type, numElements) {
                this.name = name;
                this.type = type;
                this.numElements = numElements;
            }
            return VertexAttribute;
        }());
        webgl.VertexAttribute = VertexAttribute;
        (function (VertexAttributeType) {
            VertexAttributeType[VertexAttributeType["Float"] = 0] = "Float";
        })(webgl.VertexAttributeType || (webgl.VertexAttributeType = {}));
        var VertexAttributeType = webgl.VertexAttributeType;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var PolygonBatch = (function () {
            function PolygonBatch() {
            }
            return PolygonBatch;
        }());
        webgl.PolygonBatch = PolygonBatch;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var Shader = (function () {
            function Shader(_vertexShader, _fragmentShader) {
                this._vertexShader = _vertexShader;
                this._fragmentShader = _fragmentShader;
                this._vs = null;
                this._fs = null;
                this._program = null;
                this.compile();
            }
            Shader.prototype.program = function () { return this._program; };
            Shader.prototype.vertexShader = function () { return this._vertexShader; };
            Shader.prototype.fragmentShader = function () { return this._fragmentShader; };
            Shader.prototype.compile = function () {
                var gl = spine.webgl.gl;
                try {
                    this._vs = this.compileShader(gl.VERTEX_SHADER, this._vertexShader);
                    this._fs = this.compileShader(gl.FRAGMENT_SHADER, this._fragmentShader);
                    this._program = this.compileProgram(this._vs, this._fs);
                }
                catch (e) {
                    this.dispose();
                    throw e;
                }
            };
            Shader.prototype.compileShader = function (type, source) {
                var shader = webgl.gl.createShader(type);
                webgl.gl.shaderSource(shader, source);
                webgl.gl.compileShader(shader);
                if (!webgl.gl.getShaderParameter(shader, webgl.gl.COMPILE_STATUS)) {
                    var error = "Couldn't compile shader: " + webgl.gl.getShaderInfoLog(shader);
                    webgl.gl.deleteShader(shader);
                    throw new Error(error);
                }
                return shader;
            };
            Shader.prototype.compileProgram = function (vs, fs) {
                var program = webgl.gl.createProgram();
                webgl.gl.attachShader(program, vs);
                webgl.gl.attachShader(program, fs);
                webgl.gl.linkProgram(program);
                if (!webgl.gl.getProgramParameter(program, webgl.gl.LINK_STATUS)) {
                    var error = "Couldn't compile shader program: " + webgl.gl.getProgramInfoLog(program);
                    webgl.gl.deleteProgram(program);
                    throw new Error(error);
                }
                return program;
            };
            Shader.prototype.dispose = function () {
                if (this._vs) {
                    webgl.gl.deleteShader(this._vs);
                    this._vs = null;
                }
                if (this._fs) {
                    webgl.gl.deleteShader(this._fs);
                    this._fs = null;
                }
                if (this._program) {
                    webgl.gl.deleteProgram(this._program);
                    this._program = null;
                }
            };
            Shader.newDefaultShader = function () {
                var vs = "attribute vec4 " + Shader.POSITION + ";\n" //
                    + "attribute vec4 " + Shader.COLOR + ";\n" //
                    + "attribute vec2 " + Shader.TEXCOORD + ";\n" //
                    + "uniform mat4 u_projTrans;\n" //
                    + "varying vec4 v_color;\n" //
                    + "varying vec2 v_texCoords;\n" //
                    + "\n" //
                    + "void main()\n" //
                    + "{\n" //
                    + "   v_color = " + Shader.COLOR + ";\n" //
                    + "   v_color.a = v_color.a * (255.0/254.0);\n" //
                    + "   v_texCoords = " + Shader.TEXCOORD + ";\n" //
                    + "   gl_Position =  u_projTrans * " + Shader.POSITION + ";\n" //
                    + "}\n";
                var fs = "#ifdef GL_ES\n" //
                    + "#define LOWP lowp\n" //
                    + "precision mediump float;\n" //
                    + "#else\n" //
                    + "#define LOWP \n" //
                    + "#endif\n" //
                    + "varying LOWP vec4 v_color;\n" //
                    + "varying vec2 v_texCoords;\n" //
                    + "uniform sampler2D u_texture;\n" //
                    + "void main()\n" //
                    + "{\n" //
                    + "  gl_FragColor = v_color * texture2D(u_texture, v_texCoords);\n" //
                    + "}";
                return new Shader(vs, fs);
            };
            Shader.POSITION = "a_position";
            Shader.COLOR = "a_color";
            Shader.TEXCOORD = "a_texCoords";
            return Shader;
        }());
        webgl.Shader = Shader;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        function init(gl) {
            if (!gl || !(gl instanceof WebGLRenderingContext))
                throw Error("Expected a WebGLRenderingContext");
            spine.webgl.gl = gl;
        }
        webgl.init = init;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
//# sourceMappingURL=spine.js.map