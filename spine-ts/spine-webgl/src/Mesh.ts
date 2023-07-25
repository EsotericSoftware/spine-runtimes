/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import { Disposable, Restorable } from "@esotericsoftware/spine-core";
import { Shader } from "./Shader";
import { ManagedWebGLRenderingContext } from "./WebGL";


export class Mesh implements Disposable, Restorable {
	private context: ManagedWebGLRenderingContext;
	private vertices: Float32Array;
	private verticesBuffer: WebGLBuffer | null = null;
	private verticesLength = 0;
	private dirtyVertices = false;
	private indices: Uint16Array;
	private indicesBuffer: WebGLBuffer | null = null;
	private indicesLength = 0;
	private dirtyIndices = false;
	private elementsPerVertex = 0;

	getAttributes (): VertexAttribute[] { return this.attributes; }

	maxVertices (): number { return this.vertices.length / this.elementsPerVertex; }
	numVertices (): number { return this.verticesLength / this.elementsPerVertex; }
	setVerticesLength (length: number) {
		this.dirtyVertices = true;
		this.verticesLength = length;
	}
	getVertices (): Float32Array { return this.vertices; }

	maxIndices (): number { return this.indices.length; }
	numIndices (): number { return this.indicesLength; }
	setIndicesLength (length: number) {
		this.dirtyIndices = true;
		this.indicesLength = length;
	}
	getIndices (): Uint16Array { return this.indices };

	getVertexSizeInFloats (): number {
		let size = 0;
		for (var i = 0; i < this.attributes.length; i++) {
			let attribute = this.attributes[i];
			size += attribute.numElements;
		}
		return size;
	}

	constructor (context: ManagedWebGLRenderingContext | WebGLRenderingContext, private attributes: VertexAttribute[], maxVertices: number, maxIndices: number) {
		this.context = context instanceof ManagedWebGLRenderingContext ? context : new ManagedWebGLRenderingContext(context);
		this.elementsPerVertex = 0;
		for (let i = 0; i < attributes.length; i++) {
			this.elementsPerVertex += attributes[i].numElements;
		}
		this.vertices = new Float32Array(maxVertices * this.elementsPerVertex);
		this.indices = new Uint16Array(maxIndices);
		this.context.addRestorable(this);
	}

	setVertices (vertices: Array<number>) {
		this.dirtyVertices = true;
		if (vertices.length > this.vertices.length) throw Error("Mesh can't store more than " + this.maxVertices() + " vertices");
		this.vertices.set(vertices, 0);
		this.verticesLength = vertices.length;
	}

	setIndices (indices: Array<number>) {
		this.dirtyIndices = true;
		if (indices.length > this.indices.length) throw Error("Mesh can't store more than " + this.maxIndices() + " indices");
		this.indices.set(indices, 0);
		this.indicesLength = indices.length;
	}

	draw (shader: Shader, primitiveType: number) {
		this.drawWithOffset(shader, primitiveType, 0, this.indicesLength > 0 ? this.indicesLength : this.verticesLength / this.elementsPerVertex);
	}

	drawWithOffset (shader: Shader, primitiveType: number, offset: number, count: number) {
		let gl = this.context.gl;
		if (this.dirtyVertices || this.dirtyIndices) this.update();
		this.bind(shader);
		if (this.indicesLength > 0) {
			gl.drawElements(primitiveType, count, gl.UNSIGNED_SHORT, offset * 2);
		} else {
			gl.drawArrays(primitiveType, offset, count);
		}
		this.unbind(shader);
	}

	bind (shader: Shader) {
		let gl = this.context.gl;
		gl.bindBuffer(gl.ARRAY_BUFFER, this.verticesBuffer);
		let offset = 0;
		for (let i = 0; i < this.attributes.length; i++) {
			let attrib = this.attributes[i];
			let location = shader.getAttributeLocation(attrib.name);
			gl.enableVertexAttribArray(location);
			gl.vertexAttribPointer(location, attrib.numElements, gl.FLOAT, false, this.elementsPerVertex * 4, offset * 4);
			offset += attrib.numElements;
		}
		if (this.indicesLength > 0) gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.indicesBuffer);
	}

	unbind (shader: Shader) {
		let gl = this.context.gl;
		for (let i = 0; i < this.attributes.length; i++) {
			let attrib = this.attributes[i];
			let location = shader.getAttributeLocation(attrib.name);
			gl.disableVertexAttribArray(location);
		}
		gl.bindBuffer(gl.ARRAY_BUFFER, null);
		if (this.indicesLength > 0) gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, null);
	}

	private update () {
		let gl = this.context.gl;
		if (this.dirtyVertices) {
			if (!this.verticesBuffer) {
				this.verticesBuffer = gl.createBuffer();
			}
			gl.bindBuffer(gl.ARRAY_BUFFER, this.verticesBuffer);
			gl.bufferData(gl.ARRAY_BUFFER, this.vertices.subarray(0, this.verticesLength), gl.DYNAMIC_DRAW);
			this.dirtyVertices = false;
		}

		if (this.dirtyIndices) {
			if (!this.indicesBuffer) {
				this.indicesBuffer = gl.createBuffer();
			}
			gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.indicesBuffer);
			gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, this.indices.subarray(0, this.indicesLength), gl.DYNAMIC_DRAW);
			this.dirtyIndices = false;
		}
	}

	restore () {
		this.verticesBuffer = null;
		this.indicesBuffer = null;
		this.update();
	}

	dispose () {
		this.context.removeRestorable(this);
		let gl = this.context.gl;
		gl.deleteBuffer(this.verticesBuffer);
		gl.deleteBuffer(this.indicesBuffer);
	}
}

export class VertexAttribute {
	constructor (public name: string, public type: VertexAttributeType, public numElements: number) { }
}

export class Position2Attribute extends VertexAttribute {
	constructor () {
		super(Shader.POSITION, VertexAttributeType.Float, 2);
	}
}

export class Position3Attribute extends VertexAttribute {
	constructor () {
		super(Shader.POSITION, VertexAttributeType.Float, 3);
	}
}

export class TexCoordAttribute extends VertexAttribute {
	constructor (unit: number = 0) {
		super(Shader.TEXCOORDS + (unit == 0 ? "" : unit), VertexAttributeType.Float, 2);
	}
}

export class ColorAttribute extends VertexAttribute {
	constructor () {
		super(Shader.COLOR, VertexAttributeType.Float, 4);
	}
}

export class Color2Attribute extends VertexAttribute {
	constructor () {
		super(Shader.COLOR2, VertexAttributeType.Float, 4);
	}
}

export enum VertexAttributeType {
	Float
}
