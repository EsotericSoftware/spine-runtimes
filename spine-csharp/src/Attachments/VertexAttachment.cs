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

using System;
using System.Collections.Generic;

namespace Spine {
	/// <summary>>An attachment with vertices that are transformed by one or more bones and can be deformed by a slot's vertices.</summary> 
	public class VertexAttachment : Attachment {
		internal int[] bones;
		internal float[] vertices;
		internal int worldVerticesLength;

		public int[] Bones { get { return bones; } set { bones = value; } }
		public float[] Vertices { get { return vertices; } set { vertices = value; } }
		public int WorldVerticesLength { get { return worldVerticesLength; } set { worldVerticesLength = value; } }

		public VertexAttachment (String name)
			: base(name) {
		}

		public void ComputeWorldVertices (Slot slot, float[] worldVertices) {
			ComputeWorldVertices(slot, 0, worldVerticesLength, worldVertices, 0);
		}

		/// <summary>Transforms local vertices to world coordinates.</summary>
		/// <param name="start">The index of the first <see cref="Vertices"/> value to transform. Each vertex has 2 values, x and y.</param>
		/// <param name="count">The number of world vertex values to output. Must be less than or equal to <see cref="WorldVerticesLength"/> - start.</param>
		/// <param name="worldVertices">The output world vertices. Must have a length greater than or equal to <paramref name="offset"/> + <paramref name="count"/>.</param>
		/// <param name="offset">The <paramref name="worldVertices"/> index to begin writing values.</param>
		public void ComputeWorldVertices (Slot slot, int start, int count, float[] worldVertices, int offset) {
			count += offset;
			Skeleton skeleton = slot.Skeleton;
			var deformArray = slot.attachmentVertices;
			float[] vertices = this.vertices;
			int[] bones = this.bones;
			if (bones == null) {
				if (deformArray.Count > 0) vertices = deformArray.Items;
				Bone bone = slot.bone;
				float x = bone.worldX, y = bone.worldY;
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				for (int vv = start, w = offset; w < count; vv += 2, w += 2) {
					float vx = vertices[vv], vy = vertices[vv + 1];
					worldVertices[w] = vx * a + vy * b + x;
					worldVertices[w + 1] = vx * c + vy * d + y;
				}
				return;
			}
			int v = 0, skip = 0;
			for (int i = 0; i < start; i += 2) {
				int n = bones[v];
				v += n + 1;
				skip += n;
			}
			Bone[] skeletonBones = skeleton.Bones.Items;
			if (deformArray.Count == 0) {
				for (int w = offset, b = skip * 3; w < count; w += 2) {
					float wx = 0, wy = 0;
					int n = bones[v++];
					n += v;
					for (; v < n; v++, b += 3) {
						Bone bone = skeletonBones[bones[v]];
						float vx = vertices[b], vy = vertices[b + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
				}
			} else {
				float[] deform = deformArray.Items;
				for (int w = offset, b = skip * 3, f = skip << 1; w < count; w += 2) {
					float wx = 0, wy = 0;
					int n = bones[v++];
					n += v;
					for (; v < n; v++, b += 3, f += 2) {
						Bone bone = skeletonBones[bones[v]];
						float vx = vertices[b] + deform[f], vy = vertices[b + 1] + deform[f + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
				}
			}
		}

		/// <summary>Returns true if a deform originally applied to the specified attachment should be applied to this attachment.</summary>
		virtual public bool ApplyDeform (VertexAttachment sourceAttachment) {
			return this == sourceAttachment;
		}			
	}
}
