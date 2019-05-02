/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export class MeshAttachment extends VertexAttachment {
		region: TextureRegion;
		path: string;
		regionUVs: ArrayLike<number>; uvs: ArrayLike<number>;
		triangles: Array<number>;
		color = new Color(1, 1, 1, 1);
		hullLength: number;
		private parentMesh: MeshAttachment;
		inheritDeform = false;
		tempColor = new Color(0, 0, 0, 0);

		constructor (name: string) {
			super(name);
		}

		updateUVs () {
			let regionUVs = this.regionUVs;
			if (this.uvs == null || this.uvs.length != regionUVs.length) this.uvs = Utils.newFloatArray(regionUVs.length);
			let uvs = this.uvs;
			let n = this.uvs.length;
			let u = this.region.u, v = this.region.v, width = 0, height = 0;
			if (this.region instanceof TextureAtlasRegion) {
				let region = this.region;
				let textureWidth = region.texture.getImage().width, textureHeight = region.texture.getImage().height;
				switch(region.degrees) {
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
				}
				u -= region.offsetX / textureWidth;
				v -= (region.originalHeight - region.offsetY - region.height) / textureHeight;
				width = region.originalWidth / textureWidth;
				height = region.originalHeight / textureHeight;
			} else if (this.region == null) {
				u = v = 0;
				width = height = 1;
			} else {
				width = this.region.u2 - u;
				height = this.region.v2 - v;
			}

			for (let i = 0; i < n; i += 2) {
				uvs[i] = u + regionUVs[i] * width;
				uvs[i + 1] = v + regionUVs[i + 1] * height;
			}
		}

		/*updateUVs () {
			let u = 0, v = 0, width = 0, height = 0;
			if (this.region == null) {
				u = v = 0;
				width = height = 1;
			} else {
				u = this.region.u;
				v = this.region.v;
				width = this.region.u2 - u;
				height = this.region.v2 - v;
			}
			let regionUVs = this.regionUVs;
			if (this.uvs == null || this.uvs.length != regionUVs.length) this.uvs = Utils.newFloatArray(regionUVs.length);
			let uvs = this.uvs;
			if (this.region.rotate) {
				for (let i = 0, n = uvs.length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i + 1] * width;
					uvs[i + 1] = v + height - regionUVs[i] * height;
				}
			} else {
				for (let i = 0, n = uvs.length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i] * width;
					uvs[i + 1] = v + regionUVs[i + 1] * height;
				}
			}
		}*/

		applyDeform (sourceAttachment: VertexAttachment): boolean {
			return this == sourceAttachment || (this.inheritDeform && this.parentMesh == sourceAttachment);
		}

		getParentMesh () {
			return this.parentMesh;
		}

		/** @param parentMesh May be null. */
		setParentMesh (parentMesh: MeshAttachment) {
			this.parentMesh = parentMesh;
			if (parentMesh != null) {
				this.bones = parentMesh.bones;
				this.vertices = parentMesh.vertices;
				this.worldVerticesLength = parentMesh.worldVerticesLength;
				this.regionUVs = parentMesh.regionUVs;
				this.triangles = parentMesh.triangles;
				this.hullLength = parentMesh.hullLength;
				this.worldVerticesLength = parentMesh.worldVerticesLength
			}
		}
	}

}
