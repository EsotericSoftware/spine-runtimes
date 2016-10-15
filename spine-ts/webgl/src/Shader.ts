/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine.webgl {
	export class Shader implements Disposable {
		public static MVP_MATRIX = "u_projTrans";
		public static POSITION = "a_position";
		public static COLOR = "a_color";
		public static TEXCOORDS = "a_texCoords";
		public static SAMPLER = "u_texture";

		private gl: WebGLRenderingContext;
		private vs: WebGLShader = null;
		private fs: WebGLShader = null;
		private program: WebGLProgram = null;
		private tmp2x2: Float32Array = new Float32Array(2 * 2);
		private tmp3x3: Float32Array = new Float32Array(3 * 3);
		private tmp4x4: Float32Array = new Float32Array(4 * 4);

		public getProgram () { return this.program; }
		public getVertexShader () { return this.vertexShader; }
		public getFragmentShader () { return this.fragmentShader; }

		constructor (gl: WebGLRenderingContext, private vertexShader: string, private fragmentShader: string) {
			this.gl = gl;
			this.compile();
		}

		private compile () {
			let gl = this.gl;
			try {
				this.vs = this.compileShader(gl.VERTEX_SHADER, this.vertexShader);
				this.fs = this.compileShader(gl.FRAGMENT_SHADER, this.fragmentShader);
				this.program = this.compileProgram(this.vs, this.fs);
			} catch (e) {
				this.dispose();
				throw e;
			}
		}

		private compileShader (type: number, source: string) {
			let gl = this.gl;
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

		private compileProgram (vs: WebGLShader, fs: WebGLShader) {
			let gl = this.gl;
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

		public bind () {
			this.gl.useProgram(this.program);
		}

		public unbind () {
			this.gl.useProgram(null);
		}

		public setUniformi (uniform: string, value: number) {
			this.gl.uniform1i(this.getUniformLocation(uniform), value);
		}

		public setUniformf (uniform: string, value: number) {
			this.gl.uniform1f(this.getUniformLocation(uniform), value);
		}

		public setUniform2f (uniform: string, value: number, value2: number) {
			this.gl.uniform2f(this.getUniformLocation(uniform), value, value2);
		}

		public setUniform3f (uniform: string, value: number, value2: number, value3: number) {
			this.gl.uniform3f(this.getUniformLocation(uniform), value, value2, value3);
		}

		public setUniform4f (uniform: string, value: number, value2: number, value3: number, value4: number) {
			this.gl.uniform4f(this.getUniformLocation(uniform), value, value2, value3, value4);
		}

		public setUniform2x2f (uniform: string, value: ArrayLike<number>) {
			let gl = this.gl;
			this.tmp2x2.set(value);
			gl.uniformMatrix2fv(this.getUniformLocation(uniform), false, this.tmp2x2);
		}

		public setUniform3x3f (uniform: string, value: ArrayLike<number>) {
			let gl = this.gl;
			this.tmp3x3.set(value);
			gl.uniformMatrix3fv(this.getUniformLocation(uniform), false, this.tmp3x3);
		}

		public setUniform4x4f (uniform: string, value: ArrayLike<number>) {
			let gl = this.gl;
			this.tmp4x4.set(value);
			gl.uniformMatrix4fv(this.getUniformLocation(uniform), false, this.tmp4x4);
		}

		public getUniformLocation (uniform: string): WebGLUniformLocation {
			let gl = this.gl;
			let location = gl.getUniformLocation(this.program, uniform);
			if (!location) throw new Error(`Couldn't find location for uniform ${uniform}`);
			return location;
		}

		public getAttributeLocation (attribute: string): number {
			let gl = this.gl;
			let location = gl.getAttribLocation(this.program, attribute);
			if (location == -1) throw new Error(`Couldn't find location for attribute ${attribute}`);
			return location;
		}

		public dispose () {
			let gl = this.gl;
			if (this.vs) {
				gl.deleteShader(this.vs);
				this.vs = null;
			}

			if (this.fs) {
				gl.deleteShader(this.fs);
				this.fs = null;
			}

			if (this.program) {
				gl.deleteProgram(this.program);
				this.program = null;
			}
		}

		public static newColoredTextured (gl: WebGLRenderingContext): Shader {
			let vs = `
				attribute vec4 ${Shader.POSITION};
				attribute vec4 ${Shader.COLOR};
				attribute vec2 ${Shader.TEXCOORDS};
				uniform mat4 ${Shader.MVP_MATRIX};
				varying vec4 v_color;
				varying vec2 v_texCoords;

				void main () {
					v_color = ${Shader.COLOR};
					v_texCoords = ${Shader.TEXCOORDS};
					gl_Position = ${Shader.MVP_MATRIX} * ${Shader.POSITION};
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

				void main () {
					gl_FragColor = v_color * texture2D(u_texture, v_texCoords);
				}
			`;

			return new Shader(gl, vs, fs);
		}

		public static newColored (gl: WebGLRenderingContext): Shader {
			let vs = `
				attribute vec4 ${Shader.POSITION};
				attribute vec4 ${Shader.COLOR};
				uniform mat4 ${Shader.MVP_MATRIX};
				varying vec4 v_color;

				void main () {
					v_color = ${Shader.COLOR};
					gl_Position = ${Shader.MVP_MATRIX} * ${Shader.POSITION};
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

				void main () {
					gl_FragColor = v_color;
				}
			`;

			return new Shader(gl, vs, fs);
		}
	}
}
