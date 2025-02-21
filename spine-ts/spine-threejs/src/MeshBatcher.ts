/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import * as THREE from "three"

import { ThreeJsTexture, ThreeBlendOptions } from "./ThreeJsTexture.js";
import { BlendMode } from "@esotericsoftware/spine-core";
import { SkeletonMesh } from "./SkeletonMesh.js";

export type MaterialWithMap = THREE.Material & { map: THREE.Texture | null };
export class MeshBatcher extends THREE.Mesh {
	public static MAX_VERTICES = 10920;

	// private static VERTEX_SIZE = 9;
	private vertexSize = 9;
	private vertexBuffer: THREE.InterleavedBuffer;
	private vertices: Float32Array;
	private verticesLength = 0;
	private indices: Uint16Array;
	private indicesLength = 0;
	private materialGroups: [number, number, number][] = [];

	constructor (
		maxVertices: number = MeshBatcher.MAX_VERTICES,
		private materialFactory: (parameters: THREE.MaterialParameters) => MaterialWithMap,
		private twoColorTint = true,
	) {
		super();

		if (maxVertices > MeshBatcher.MAX_VERTICES) throw new Error("Can't have more than 10920 triangles per batch: " + maxVertices);

		if (twoColorTint) {
			this.vertexSize += 3;
		}

		let vertices = this.vertices = new Float32Array(maxVertices * this.vertexSize);
		let indices = this.indices = new Uint16Array(maxVertices * 3);
		let geo = new THREE.BufferGeometry();
		let vertexBuffer = this.vertexBuffer = new THREE.InterleavedBuffer(vertices, this.vertexSize);
		vertexBuffer.usage = WebGLRenderingContext.DYNAMIC_DRAW;
		geo.setAttribute("position", new THREE.InterleavedBufferAttribute(vertexBuffer, 3, 0, false));
		geo.setAttribute("color", new THREE.InterleavedBufferAttribute(vertexBuffer, 4, 3, false));
		geo.setAttribute("uv", new THREE.InterleavedBufferAttribute(vertexBuffer, 2, 7, false));
		if (twoColorTint) {
			geo.setAttribute("darkcolor", new THREE.InterleavedBufferAttribute(vertexBuffer, 3, 9, false));
		}
		geo.setIndex(new THREE.BufferAttribute(indices, 1));
		geo.getIndex()!.usage = WebGLRenderingContext.DYNAMIC_DRAW;
		geo.drawRange.start = 0;
		geo.drawRange.count = 0;
		this.geometry = geo;
		this.material = [];
	}

	dispose () {
		this.geometry.dispose();
		if (this.material instanceof THREE.Material)
			this.material.dispose();
		else if (this.material) {
			for (let i = 0; i < this.material.length; i++) {
				let material = this.material[i];
				if (material instanceof THREE.Material)
					material.dispose();
			}
		}
	}

	clear () {
		let geo = (<THREE.BufferGeometry>this.geometry);
		geo.drawRange.start = 0;
		geo.drawRange.count = 0;
		geo.clearGroups();
		this.materialGroups = [];
		if (this.material instanceof THREE.Material) {
			const meshMaterial = this.material as MaterialWithMap;
			meshMaterial.map = null;
			meshMaterial.blending = THREE.NormalBlending;
		} else if (Array.isArray(this.material)) {
			for (let i = 0; i < this.material.length; i++) {
				const meshMaterial = this.material[i] as MaterialWithMap;
				meshMaterial.map = null;
				meshMaterial.blending = THREE.NormalBlending;
			}
		}
		return this;
	}

	begin () {
		this.verticesLength = 0;
		this.indicesLength = 0;
	}

	canBatch (numVertices: number, numIndices: number) {
		if (this.indicesLength + numIndices >= this.indices.byteLength / 2) return false;
		if (this.verticesLength / this.vertexSize + numVertices >= (this.vertices.byteLength / 4) / this.vertexSize) return false;
		return true;
	}

	batch (vertices: ArrayLike<number>, verticesLength: number, indices: ArrayLike<number>, indicesLength: number, z: number = 0) {
		let indexStart = this.verticesLength / this.vertexSize;
		let vertexBuffer = this.vertices;
		let i = this.verticesLength;
		let j = 0;
		if (this.twoColorTint) {
			for (; j < verticesLength;) {
				vertexBuffer[i++] = vertices[j++];  // x
				vertexBuffer[i++] = vertices[j++];  // y
				vertexBuffer[i++] = z;				// z

				vertexBuffer[i++] = vertices[j++];  // r
				vertexBuffer[i++] = vertices[j++];  // g
				vertexBuffer[i++] = vertices[j++];  // b
				vertexBuffer[i++] = vertices[j++];  // a

				vertexBuffer[i++] = vertices[j++];  // u
				vertexBuffer[i++] = vertices[j++];  // v

				vertexBuffer[i++] = vertices[j++];  // dark r
				vertexBuffer[i++] = vertices[j++];  // dark g
				vertexBuffer[i++] = vertices[j++];  // dark b
				j++;
			}
		} else {
			for (; j < verticesLength;) {
				vertexBuffer[i++] = vertices[j++];  // x
				vertexBuffer[i++] = vertices[j++];  // y
				vertexBuffer[i++] = z;				// z

				vertexBuffer[i++] = vertices[j++];  // r
				vertexBuffer[i++] = vertices[j++];  // g
				vertexBuffer[i++] = vertices[j++];  // b
				vertexBuffer[i++] = vertices[j++];  // a

				vertexBuffer[i++] = vertices[j++];  // u
				vertexBuffer[i++] = vertices[j++];  // v
			}
		}
		this.verticesLength = i;

		let indicesArray = this.indices;
		for (i = this.indicesLength, j = 0; j < indicesLength; i++, j++)
			indicesArray[i] = indices[j] + indexStart;
		this.indicesLength += indicesLength;
	}

	end () {
		this.vertexBuffer.needsUpdate = this.verticesLength > 0;
		this.vertexBuffer.addUpdateRange(0, this.verticesLength);
		let geo = (<THREE.BufferGeometry>this.geometry);
		this.closeMaterialGroups();
		let index = geo.getIndex();
		if (!index) throw new Error("BufferAttribute must not be null.");
		index.needsUpdate = this.indicesLength > 0;
		index.addUpdateRange(0, this.indicesLength);
		geo.drawRange.start = 0;
		geo.drawRange.count = this.indicesLength;
		geo.computeVertexNormals();
	}

	addMaterialGroup (indicesLength: number, materialGroup: number) {
		const currentGroup = this.materialGroups[this.materialGroups.length - 1];

		if (currentGroup === undefined || currentGroup[2] !== materialGroup) {
			this.materialGroups.push([this.indicesLength, indicesLength, materialGroup]);
		} else {
			currentGroup[1] += indicesLength;
		}
	}

	private closeMaterialGroups () {
		const geometry = this.geometry as THREE.BufferGeometry;
		for (let i = 0; i < this.materialGroups.length; i++) {
			const [startIndex, count, materialGroup] = this.materialGroups[i];

			geometry.addGroup(startIndex, count, materialGroup);
		}
	}

	findMaterialGroup (slotTexture: THREE.Texture, slotBlendMode: BlendMode) {
		const blendingObject = ThreeJsTexture.toThreeJsBlending(slotBlendMode);
		let group = -1;

		if (Array.isArray(this.material)) {
			for (let i = 0; i < this.material.length; i++) {
				const meshMaterial = this.material[i] as MaterialWithMap;

				if (!meshMaterial.map) {
					updateMeshMaterial(meshMaterial, slotTexture, blendingObject);
					return i;
				}

				if (meshMaterial.map === slotTexture
					&& blendingObject.blending === meshMaterial.blending
					&& (blendingObject.blendSrc === undefined || blendingObject.blendSrc === meshMaterial.blendSrc)
					&& (blendingObject.blendDst === undefined || blendingObject.blendDst === meshMaterial.blendDst)
					&& (blendingObject.blendSrcAlpha === undefined || blendingObject.blendSrcAlpha === meshMaterial.blendSrcAlpha)
					&& (blendingObject.blendDstAlpha === undefined || blendingObject.blendDstAlpha === meshMaterial.blendDstAlpha)
				) {
					return i;
				}
			}

			const meshMaterial = this.newMaterial();
			updateMeshMaterial(meshMaterial as MaterialWithMap, slotTexture, blendingObject);
			this.material.push(meshMaterial);
			group = this.material.length - 1;
		} else {
			throw new Error("MeshBatcher.material needs to be an array for geometry groups to work");
		}

		return group;
	}

	private newMaterial (): MaterialWithMap {
		const meshMaterial = this.materialFactory(SkeletonMesh.DEFAULT_MATERIAL_PARAMETERS);

		if (!('map' in meshMaterial)) {
			throw new Error("The material factory must return a material having the map property for the texture.");
		}

		if (meshMaterial instanceof SkeletonMeshMaterial) {
			return meshMaterial;
		}

		if (this.twoColorTint) {
			meshMaterial.defines = {
				...meshMaterial.defines,
				USE_SPINE_DARK_TINT: 1,
			}
		}

		meshMaterial.onBeforeCompile = spineOnBeforeCompile;

		return meshMaterial;
	}
}

const spineOnBeforeCompile = (shader: THREE.WebGLProgramParametersWithUniforms) => {

	let code;

	// VERTEX SHADER MODIFICATIONS

	// Add dark color attribute
	shader.vertexShader = `
		#if defined( USE_SPINE_DARK_TINT )
			attribute vec3 darkcolor;
		#endif
	` + shader.vertexShader;

	// Add dark color attribute
	code = `
		#if defined( USE_SPINE_DARK_TINT )
			varying vec3 v_dark;
		#endif
	`;
	shader.vertexShader = insertAfterElementInShader(shader.vertexShader, '#include <color_pars_vertex>', code);

	// Define v_dark varying
	code = `
		#if defined( USE_SPINE_DARK_TINT )
			v_dark = vec3( 1.0 );
			v_dark *= darkcolor;
		#endif
	`;
	shader.vertexShader = insertAfterElementInShader(shader.vertexShader, '#include <color_vertex>', code);


	// FRAGMENT SHADER MODIFICATIONS

	// Define v_dark varying
	code = `
		#ifdef USE_SPINE_DARK_TINT
			varying vec3 v_dark;
		#endif
	`;
	shader.fragmentShader = insertAfterElementInShader(shader.fragmentShader, '#include <color_pars_fragment>', code);

	// Replacing color_fragment with the addition of dark tint formula if twoColorTint is true
	shader.fragmentShader = shader.fragmentShader.replace(
		'#include <color_fragment>',
		`
			#ifdef USE_SPINE_DARK_TINT
				#ifdef USE_COLOR_ALPHA
						diffuseColor.a *= vColor.a;
						diffuseColor.rgb = (diffuseColor.a - diffuseColor.rgb) * v_dark.rgb + diffuseColor.rgb * vColor.rgb;
				#endif
			#else
				#ifdef USE_COLOR_ALPHA
						diffuseColor *= vColor;
				#endif
			#endif
		`
	);

	// We had to remove this because we need premultiplied blending modes, but our textures are already premultiplied
	// We could actually create a custom blending mode for Normal and Additive too
	shader.fragmentShader = shader.fragmentShader.replace('#include <premultiplied_alpha_fragment>', '');

	// We had to remove this (and don't assign a color space to the texture) otherwise we would see artifacts on texture edges
	shader.fragmentShader = shader.fragmentShader.replace('#include <colorspace_fragment>', '');

}

function insertAfterElementInShader (shader: string, elementToFind: string, codeToInsert: string) {
	const index = shader.indexOf(elementToFind);
	const beforeToken = shader.slice(0, index + elementToFind.length);
	const afterToken = shader.slice(index + elementToFind.length);
	return beforeToken + codeToInsert + afterToken;
}

function updateMeshMaterial (meshMaterial: MaterialWithMap, slotTexture: THREE.Texture, blending: ThreeBlendOptions) {
	meshMaterial.map = slotTexture;
	Object.assign(meshMaterial, blending);
	meshMaterial.needsUpdate = true;
}


export class SkeletonMeshMaterial extends THREE.ShaderMaterial {

	public get map (): THREE.Texture | null {
		return this.uniforms.map.value;
	}

	public set map (value: THREE.Texture | null) {
		this.uniforms.map.value = value;
	}

	constructor (parameters: THREE.ShaderMaterialParameters) {

		let vertexShader = `
			varying vec2 vUv;
			varying vec4 vColor;
			void main() {
				vUv = uv;
				vColor = color;
				gl_Position = projectionMatrix*modelViewMatrix*vec4(position,1.0);
			}
		`;
		let fragmentShader = `
			uniform sampler2D map;
			#ifdef USE_SPINE_ALPHATEST
			uniform float alphaTest;
			#endif
			varying vec2 vUv;
			varying vec4 vColor;
			void main(void) {
				gl_FragColor = texture2D(map, vUv)*vColor;
				#ifdef USE_SPINE_ALPHATEST
					if (gl_FragColor.a < alphaTest) discard;
				#endif
			}
		`;

		let uniforms = { map: { value: null } };
		if (parameters.uniforms) {
			uniforms = { ...parameters.uniforms, ...uniforms };
		}

		if (parameters.alphaTest && parameters.alphaTest > 0) {
			parameters.defines = { USE_SPINE_ALPHATEST: 1 };
		}

		super({
			vertexShader,
			fragmentShader,
			...parameters,
			uniforms,
		});
	}
}