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
	export class SkeletonRenderer {
		static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];

		premultipliedAlpha = false;
		private gl: WebGLRenderingContext;

		constructor (gl: WebGLRenderingContext) {
			this.gl = gl;
		}

		draw (batcher: PolygonBatcher, skeleton: Skeleton) {
			let premultipliedAlpha = this.premultipliedAlpha;
			let blendMode: BlendMode = null;

			let vertices: ArrayLike<number> = null;
			let triangles: Array<number> = null;
			let drawOrder = skeleton.drawOrder;
			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let slot = drawOrder[i];
				let attachment = slot.getAttachment();
				let texture: GLTexture = null;
				if (attachment instanceof RegionAttachment) {
					let region = <RegionAttachment>attachment;
					vertices = region.updateWorldVertices(slot, premultipliedAlpha);
					triangles = SkeletonRenderer.QUAD_TRIANGLES;
					texture = <GLTexture>(<TextureAtlasRegion>region.region.renderObject).texture;

				} else if (attachment instanceof MeshAttachment) {
					let mesh = <MeshAttachment>attachment;
					vertices = mesh.updateWorldVertices(slot, premultipliedAlpha);
					triangles = mesh.triangles;
					texture = <GLTexture>(<TextureAtlasRegion>mesh.region.renderObject).texture;
				} else continue;

				if (texture != null) {
					let slotBlendMode = slot.data.blendMode;
					if (slotBlendMode != blendMode) {
						blendMode = slotBlendMode;
						batcher.setBlendMode(getSourceGLBlendMode(this.gl, blendMode, premultipliedAlpha), getDestGLBlendMode(this.gl, blendMode));
					}
					batcher.draw(texture, vertices, triangles);
				}
			}
		}
	}
}
