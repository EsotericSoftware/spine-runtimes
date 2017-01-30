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
		}

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
