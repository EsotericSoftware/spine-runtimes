module spine.webgl {
    export class Shader {
        public static POSITION = "a_position";
        public static COLOR = "a_color";
        public static TEXCOORD = "a_texCoords";

        private _vs: WebGLShader = null;
        private _fs: WebGLShader = null;
        private _program: WebGLProgram = null;        

        constructor(private _vertexShader: string, private _fragmentShader: string) {           
            this.compile();
        }

        private compile() {
            let gl = spine.webgl.gl;
            try {
                this._vs = this.compileShader(gl.VERTEX_SHADER, this._vertexShader);
                this._fs = this.compileShader(gl.FRAGMENT_SHADER, this._fragmentShader);
                this._program = this.compileProgram(this._vs, this._fs);
            } catch (e) {
                this.dispose();
                throw e;
            }
        }

        private compileShader(type: number, source: string) {
            let shader = gl.createShader(type);
            gl.shaderSource(shader, source);
            gl.compileShader(shader);
            if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
                let error = "Couldn't compile shader: " + gl.getShaderInfoLog(shader);
                gl.deleteShader(shader);
                throw new Error(error);
            }
            return shader;
        }

        private compileProgram(vs: WebGLShader, fs: WebGLShader) {
            let program = gl.createProgram();
            gl.attachShader(program, vs);
            gl.attachShader(program, fs);
            gl.linkProgram(program);

            if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
                let error = "Couldn't compile shader program: " + gl.getProgramInfoLog(program);
                gl.deleteProgram(program);
                throw new Error(error);
            }
            return program;     
        }

        public dispose() {
            if (this._vs) {
                gl.deleteShader(this._vs);
                this._vs = null;
            }

            if (this._fs) {
                gl.deleteShader(this._fs);
                this._fs = null;
            }

            if (this._program) {
                gl.deleteProgram(this._program);
                this._program = null;
            }
        }

        public program() { return this._program; }
        public vertexShader() { return this._vertexShader; }
        public fragmentShader() { return this._fragmentShader; }

        public static newDefaultShader(): Shader {
            let vs = "attribute vec4 " + Shader.POSITION + ";\n" //
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

            let fs = "#ifdef GL_ES\n" //
			+ "#define LOWP lowp\n" //
			+ "precision mediump float;\n" //
			+ "#else\n" //
			+ "#define LOWP \n" //
			+ "#endif\n" //
			+ "varying LOWP vec4 v_color;\n" //
			+ "varying vec2 v_texCoords;\n" //
			+ "uniform sampler2D u_texture;\n" //
			+ "void main()\n"//
			+ "{\n" //
			+ "  gl_FragColor = v_color * texture2D(u_texture, v_texCoords);\n" //
			+ "}";

            return new Shader(vs, fs);
        }
    }
}