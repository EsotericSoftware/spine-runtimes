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

package spine.attachments {
	import spine.Bone;
	import spine.Skeleton;
	import spine.Slot;

	public dynamic class VertexAttachment extends Attachment {
		private static var nextID : int = 0;
		
		public var bones : Vector.<int>;
		public var vertices : Vector.<Number>;
		public var worldVerticesLength : int;
		public var id : int = (nextID++ & 65535) << 11;

		public function VertexAttachment(name : String) {
			super(name);
		}

		/** Transforms local vertices to world coordinates.
		 * @param start The index of the first local vertex value to transform. Each vertex has 2 values, x and y.
		 * @param count The number of world vertex values to output. Must be <= {@link #getWorldVerticesLength()} - start.
		 * @param worldVertices The output world vertices. Must have a length >= offset + count.
		 * @param offset The worldVertices index to begin writing values. */
		public function computeWorldVertices(slot : Slot, start : int, count : int, worldVertices : Vector.<Number>, offset : int, stride : int) : void {
			count = offset + (count >> 1) * stride;
			var skeleton : Skeleton = slot.skeleton;
			var deformArray : Vector.<Number> = slot.attachmentVertices;
			var vertices : Vector.<Number> = this.vertices;
			var bones : Vector.<int> = this.bones;
			var deform : Vector.<Number>;

			var v : int, w : int, n : int, i : int, skip : int, b : int, f : int;
			var vx : Number, vy : Number;
			var wx : Number, wy : Number;
			var bone : Bone;

			if (bones == null) {
				if (deformArray.length > 0) vertices = deformArray;
				bone = slot.bone;
				var x : Number = bone.worldX;
				var y : Number = bone.worldY;
				var a : Number = bone.a, bb : Number = bone.b, c : Number = bone.c, d : Number = bone.d;
				for (v = start, w = offset; w < count; v += 2, w += stride) {
					vx = vertices[v]
					,
					vy = vertices[v + 1];
					worldVertices[w] = vx * a + vy * bb + x;
					worldVertices[w + 1] = vx * c + vy * d + y;
				}
				return;
			}
			v = 0
			,
			skip = 0;
			for (i = 0; i < start; i += 2) {
				n = bones[v];
				v += n + 1;
				skip += n;
			}
			var skeletonBones : Vector.<Bone> = skeleton.bones;
			if (deformArray.length == 0) {
				for (w = offset, b = skip * 3; w < count; w += stride) {
					wx = 0
					,
					wy = 0;
					n = bones[v++];
					n += v;
					for (; v < n; v++, b += 3) {
						bone = skeletonBones[bones[v]];
						vx = vertices[b];
						vy = vertices[b + 1];
						var weight : Number = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
				}
			} else {
				deform = deformArray;
				for (w = offset, b = skip * 3, f = skip << 1; w < count; w += stride) {
					wx = 0;
					wy = 0;
					n = bones[v++];
					n += v;
					for (; v < n; v++, b += 3, f += 2) {
						bone = skeletonBones[bones[v]];
						vx = vertices[b] + deform[f];
						vy = vertices[b + 1] + deform[f + 1];
						weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
				}
			}
		}

		/** Returns true if a deform originally applied to the specified attachment should be applied to this attachment. */
		public function applyDeform(sourceAttachment : VertexAttachment) : Boolean {
			return this == sourceAttachment;
		}
	}
}