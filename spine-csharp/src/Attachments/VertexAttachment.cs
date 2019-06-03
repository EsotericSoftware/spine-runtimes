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

using System;

namespace Spine {
	/// <summary>>An attachment with vertices that are transformed by one or more bones and can be deformed by a slot's
	/// <see cref="Slot.Deform"/>.</summary> 
	public abstract class VertexAttachment : Attachment {
		static int nextID = 0;
		static readonly Object nextIdLock = new Object();

		internal readonly int id;
		internal int[] bones;
		internal float[] vertices;
		internal int worldVerticesLength;
		internal VertexAttachment deformAttachment;

		/// <summary>Gets a unique ID for this attachment.</summary>
		public int Id { get { return id; } }
		public int[] Bones { get { return bones; } set { bones = value; } }
		public float[] Vertices { get { return vertices; } set { vertices = value; } }
		public int WorldVerticesLength { get { return worldVerticesLength; } set { worldVerticesLength = value; } }
		///<summary>Deform keys for the deform attachment are also applied to this attachment.
		/// May be null if no deform keys should be applied.</summary>
		public VertexAttachment DeformAttachment { get { return deformAttachment; } set { deformAttachment = value; } }

		public VertexAttachment (string name)
			: base(name) {

			deformAttachment = this;
			lock (VertexAttachment.nextIdLock) {
				id = (VertexAttachment.nextID++ & 65535) << 11;
			}
		}

		public void ComputeWorldVertices (Slot slot, float[] worldVertices) {
			ComputeWorldVertices(slot, 0, worldVerticesLength, worldVertices, 0);
		}

		/// <summary>
		/// Transforms the attachment's local <see cref="Vertices"/> to world coordinates. If the slot's <see cref="Slot.Deform"/> is
		/// not empty, it is used to deform the vertices.
		/// <para />
		/// See <a href="http://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
		/// Runtimes Guide.
		/// </summary>
		/// <param name="start">The index of the first <see cref="Vertices"/> value to transform. Each vertex has 2 values, x and y.</param>
		/// <param name="count">The number of world vertex values to output. Must be less than or equal to <see cref="WorldVerticesLength"/> - start.</param>
		/// <param name="worldVertices">The output world vertices. Must have a length greater than or equal to <paramref name="offset"/> + <paramref name="count"/>.</param>
		/// <param name="offset">The <paramref name="worldVertices"/> index to begin writing values.</param>
		/// <param name="stride">The number of <paramref name="worldVertices"/> entries between the value pairs written.</param>
		public void ComputeWorldVertices (Slot slot, int start, int count, float[] worldVertices, int offset, int stride = 2) {
			count = offset + (count >> 1) * stride;
			Skeleton skeleton = slot.bone.skeleton;
			var deformArray = slot.deform;
			float[] vertices = this.vertices;
			int[] bones = this.bones;
			if (bones == null) {
				if (deformArray.Count > 0) vertices = deformArray.Items;
				Bone bone = slot.bone;
				float x = bone.worldX, y = bone.worldY;
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				for (int vv = start, w = offset; w < count; vv += 2, w += stride) {
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
			var skeletonBones = skeleton.bones.Items;
			if (deformArray.Count == 0) {
				for (int w = offset, b = skip * 3; w < count; w += stride) {
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
				for (int w = offset, b = skip * 3, f = skip << 1; w < count; w += stride) {
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

		///<summary>Does not copy id (generated) or name (set on construction).</summary>
		internal void CopyTo (VertexAttachment attachment) {
			if (bones != null) {
				attachment.bones = new int[bones.Length];
				Array.Copy(bones, 0, attachment.bones, 0, bones.Length);
			}
			else
				attachment.bones = null;
			
			if (vertices != null) {
				attachment.vertices = new float[vertices.Length];
				Array.Copy(vertices, 0, attachment.vertices, 0, vertices.Length);
			}
			else
				attachment.vertices = null;
			
			attachment.worldVerticesLength = worldVerticesLength;
			attachment.deformAttachment = deformAttachment;
		}
	}
}
