module spine.webgl {
    export class Shader {
        public static MVP_MATRIX = "u_projTrans";
        public static POSITION = "a_position";
        public static COLOR = "a_color";
        public static TEXCOORDS = "a_texCoords";
        public static SAMPLER = "u_texture";
                
        private _vs: WebGLShader = null;
        private _fs: WebGLShader = null;
        private _program: WebGLProgram = null;
        private _tmp2x2: Float32Array = new Float32Array(2 * 2);
        private _tmp3x3: Float32Array = new Float32Array(3 * 3);
        private _tmp4x4: Float32Array = new Float32Array(4 * 4);

        public program() { return this._program; }
        public vertexShader() { return this._vertexShader; }
        public fragmentShader() { return this._fragmentShader; }        

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

        public bind() {            
            gl.useProgram(this._program);
        }

        public unbind() {
            gl.useProgram(null);            
        }

        public setUniformi(uniform: string, value: number) {
            gl.uniform1i(this.getUniformLocation(uniform), value);
        }

        public setUniformf(uniform: string, value: number) {
            gl.uniform1f(this.getUniformLocation(uniform), value);
        }

        public setUniform2f(uniform: string, value: number, value2: number) {
            gl.uniform2f(this.getUniformLocation(uniform), value, value2);
        }

        public setUniform3f(uniform: string, value: number, value2: number, value3: number) {
            gl.uniform3f(this.getUniformLocation(uniform), value, value2, value3);
        }

        public setUniform4f(uniform: string, value: number, value2: number, value3: number, value4: number) {
            gl.uniform4f(this.getUniformLocation(uniform), value, value2, value3, value4);
        }

        public setUniform2x2f(uniform: string, value: Array<number> | Float32Array) {
            this._tmp2x2.set(value);
            gl.uniformMatrix2fv(this.getUniformLocation(uniform), false, this._tmp2x2);
        }

        public setUniform3x3f(uniform: string, value: Array<number> | Float32Array ) {
            this._tmp3x3.set(value);
            gl.uniformMatrix3fv(this.getUniformLocation(uniform), false, this._tmp3x3);
        }

        public setUniform4x4f(uniform: string, value: Array<number> | Float32Array) {
            this._tmp4x4.set(value);
            gl.uniformMatrix4fv(this.getUniformLocation(uniform), false, this._tmp4x4);
        }

        public getUniformLocation(uniform: string): WebGLUniformLocation {
            let location = gl.getUniformLocation(this._program, uniform);
            if (!location) throw new Error(`Couldn't find location for uniform ${uniform}`);
            return location;
        }

        public getAttributeLocation(attribute: string): number {
            let location = gl.getAttribLocation(this._program, attribute);
            if (location == -1) throw new Error(`Couldn't find location for attribute ${attribute}`);
            return location;
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

        public static newColoredTextured(): Shader {
            let vs = `
                attribute vec4 ${Shader.POSITION};
                attribute vec4 ${Shader.COLOR};
                attribute vec2 ${Shader.TEXCOORDS};
                uniform mat4 ${Shader.MVP_MATRIX};
                varying vec4 v_color;
                varying vec2 v_texCoords;
            
                void main() {                    
                    v_color = ${Shader.COLOR};                    
                    v_texCoords = ${Shader.TEXCOORDS};
                    gl_Position =  ${Shader.MVP_MATRIX} * ${Shader.POSITION};
                }
            `;

            let fs = `
                #ifdef GL_ES
			        #define LOWP lowp
			        precision mediump float;
			    #else
			        #define LOWP 
			    #endif
			    varying LOWP vec4 v_color;
			    varying vec2 v_texCoords;
			    uniform sampler2D u_texture;

			    void main() {			    
			        gl_FragColor = v_color * texture2D(u_texture, v_texCoords);
			    }
            `;

            return new Shader(vs, fs);
        }

        public static newColored(): Shader {
            let vs = `
                attribute vec4 ${Shader.POSITION};
                attribute vec4 ${Shader.COLOR};            
                uniform mat4 ${Shader.MVP_MATRIX};
                varying vec4 v_color;                
            
                void main() {                    
                    v_color = ${Shader.COLOR};                    
                    gl_Position =  ${Shader.MVP_MATRIX} * ${Shader.POSITION};
                }
            `;

            let fs = `
                #ifdef GL_ES
			        #define LOWP lowp
			        precision mediump float;
			    #else
			        #define LOWP
			    #endif
			    varying LOWP vec4 v_color;			    			    

			    void main() {			    
			        gl_FragColor = v_color;
			    }
            `;

            return new Shader(vs, fs);
        }
    }
}