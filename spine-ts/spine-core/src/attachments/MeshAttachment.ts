/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

import type { TextureRegion } from "../Texture.js";
import { TextureAtlasRegion } from "../TextureAtlas.js";
import { Color, type NumberArrayLike, Utils } from "../Utils.js";
import { type Attachment, VertexAttachment } from "./Attachment.js";
import type { HasSequence } from "./HasSequence.js";
import type { Sequence } from "./Sequence.js";

/** An attachment that displays a textured mesh. A mesh has hull vertices and internal vertices within the hull. Holes are not
 * supported. Each vertex has UVs (texture coordinates) and triangles that are used to map an image on to the mesh.
 *
 * See [Mesh attachments](http://esotericsoftware.com/spine-meshes) in the Spine User Guide. */
export class MeshAttachment extends VertexAttachment implements HasSequence {
	readonly sequence: Sequence;

	/** The UV pair for each vertex, normalized within the texture region. */
	regionUVs: NumberArrayLike = [];

	/** Triplets of vertex indices which describe the mesh's triangulation. */
	triangles: Array<number> = [];

	/** The number of entries at the beginning of {@link vertices} that make up the mesh hull. */
	hullLength: number = 0;

	/** The name of the texture region for this attachment. */
	path?: string;

	/** The color to tint the mesh. */
	color = new Color(1, 1, 1, 1);

	private parentMesh: MeshAttachment | null = null;

	/** Vertex index pairs describing edges for controlling triangulation, or null if nonessential data was not exported. Mesh
	 * triangles do not never cross edges. Triangulation is not performed at runtime. */
	edges: Array<number> = [];

	/** The width of the mesh's image. Available only when nonessential data was exported. */
	width: number = 0;

	/** The height of the mesh's image. Available only when nonessential data was exported. */
	height: number = 0;

	tempColor = new Color(0, 0, 0, 0);

	constructor (name: string, sequence: Sequence) {
		super(name);
		this.sequence = sequence;
	}

	copy (): Attachment {
		if (this.parentMesh) return this.newLinkedMesh();

		const copy = new MeshAttachment(this.name, this.sequence.copy());
		copy.path = this.path;
		copy.color.setFromColor(this.color);

		this.copyTo(copy);
		copy.regionUVs = [];
		Utils.arrayCopy(this.regionUVs, 0, copy.regionUVs, 0, this.regionUVs.length);
		copy.triangles = [];
		Utils.arrayCopy(this.triangles, 0, copy.triangles, 0, this.triangles.length);
		copy.hullLength = this.hullLength;

		// Nonessential.
		if (this.edges) {
			copy.edges = [];
			Utils.arrayCopy(this.edges, 0, copy.edges, 0, this.edges.length);
		}
		copy.width = this.width;
		copy.height = this.height;

		return copy;
	}

	updateSequence () {
		this.sequence.update(this);
	}

	/** The parent mesh if this is a linked mesh, else null. A linked mesh shares the {@link bones}, {@link vertices},
	 * {@link regionUVs}, {@link triangles}, {@link hullLength}, {@link edges}, {@link width}, and {@link height} with the
	 * parent mesh, but may have a different {@link name} or {@link path}, and therefore a different texture region. */
	getParentMesh () {
		return this.parentMesh;
	}

	/** @param parentMesh May be null. */
	setParentMesh (parentMesh: MeshAttachment) {
		this.parentMesh = parentMesh;
		if (parentMesh) {
			this.bones = parentMesh.bones;
			this.vertices = parentMesh.vertices;
			this.worldVerticesLength = parentMesh.worldVerticesLength;
			this.regionUVs = parentMesh.regionUVs;
			this.triangles = parentMesh.triangles;
			this.hullLength = parentMesh.hullLength;
			this.worldVerticesLength = parentMesh.worldVerticesLength
		}
	}

	/** Returns a new mesh with the {@link parentMesh} set to this mesh's parent mesh, if any, else to this mesh. **/
	newLinkedMesh (): MeshAttachment {
		const copy = new MeshAttachment(this.name, this.sequence.copy());
		copy.timelineAttachment = this.timelineAttachment;
		copy.path = this.path;
		copy.color.setFromColor(this.color);
		copy.setParentMesh(this.parentMesh ? this.parentMesh : this);
		copy.updateSequence();
		return copy;
	}

	/** Computes {@link Sequence.getUVs | UVs} for a mesh attachment.
	 * @param uvs Output array for the computed UVs, same length as regionUVs. */
	static computeUVs (region: TextureRegion | null, regionUVs: NumberArrayLike, uvs: NumberArrayLike): void {
		if (!region) throw new Error("Region not set.");
		const n = uvs.length;
		let u = region.u, v = region.v, width = 0, height = 0;
		if (region instanceof TextureAtlasRegion) {
			const page = region.page;
			const textureWidth = page.width, textureHeight = page.height;
			switch (region.degrees) {
				case 90:
					u -= (region.originalHeight - region.offsetY - region.height) / textureWidth;
					v -= (region.originalWidth - region.offsetX - region.width) / textureHeight;
					width = region.originalHeight / textureWidth;
					height = region.originalWidth / textureHeight;
					for (let i = 0; i < n; i += 2) {
						uvs[i] = u + regionUVs[i + 1] * width;
						uvs[i + 1] = v + (1 - regionUVs[i]) * height;
					}
					return;
				case 180:
					u -= (region.originalWidth - region.offsetX - region.width) / textureWidth;
					v -= region.offsetY / textureHeight;
					width = region.originalWidth / textureWidth;
					height = region.originalHeight / textureHeight;
					for (let i = 0; i < n; i += 2) {
						uvs[i] = u + (1 - regionUVs[i]) * width;
						uvs[i + 1] = v + (1 - regionUVs[i + 1]) * height;
					}
					return;
				case 270:
					u -= region.offsetY / textureWidth;
					v -= region.offsetX / textureHeight;
					width = region.originalHeight / textureWidth;
					height = region.originalWidth / textureHeight;
					for (let i = 0; i < n; i += 2) {
						uvs[i] = u + (1 - regionUVs[i + 1]) * width;
						uvs[i + 1] = v + regionUVs[i] * height;
					}
					return;
				default:
					u -= region.offsetX / textureWidth;
					v -= (region.originalHeight - region.offsetY - region.height) / textureHeight;
					width = region.originalWidth / textureWidth;
					height = region.originalHeight / textureHeight;
			}
		} else if (!region) {
			u = v = 0;
			width = height = 1;
		} else {
			width = region.u2 - u;
			height = region.v2 - v;
		}

		for (let i = 0; i < n; i += 2) {
			uvs[i] = u + regionUVs[i] * width;
			uvs[i + 1] = v + regionUVs[i + 1] * height;
		}
	}
}
