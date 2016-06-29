/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
#define SPINE_OPTIONAL_NORMALS
using UnityEngine;

namespace Spine.Unity.MeshGeneration {
	public class ArraysMeshGenerator {
		#region Settings
		protected bool premultiplyVertexColors = true;
		public bool PremultiplyVertexColors { get { return this.premultiplyVertexColors; } set { this.premultiplyVertexColors = value; } }
		#endregion

		protected float[] attachmentVertexBuffer = new float[8];
		protected Vector3[] meshVertices;
		protected Color32[] meshColors32;
		protected Vector2[] meshUVs;


		protected bool generateNormals = false;
		public bool GenerateNormals {
			get { return generateNormals; }
			set { generateNormals = value; }
		}

		Vector3[] meshNormals;

		public void TryAddNormalsTo (Mesh mesh, int targetVertexCount) {
			#if SPINE_OPTIONAL_NORMALS
			if (generateNormals) {
				bool verticesWasResized = this.meshNormals == null || targetVertexCount > meshNormals.Length;
				if (verticesWasResized) {
					this.meshNormals = new Vector3[targetVertexCount];
					Vector3 normal = new Vector3(0, 0, -1);
					Vector3[] normals = this.meshNormals;
					for (int i = 0; i < targetVertexCount; i++)
						normals[i] = normal;
				}

				mesh.normals = this.meshNormals;
			}
			#endif
		}


		public static bool EnsureSize (int targetVertexCount, ref Vector3[] vertices, ref Vector2[] uvs, ref Color32[] colors) {
			Vector3[] verts = vertices;
			bool verticesWasResized = verts == null || targetVertexCount > verts.Length;
			if (verticesWasResized) {
				// Not enough space, increase size.
				vertices = new Vector3[targetVertexCount];
				colors = new Color32[targetVertexCount];
				uvs = new Vector2[targetVertexCount];
			} else {
				// Too many vertices, zero the extra.
				Vector3 zero = Vector3.zero;
				for (int i = targetVertexCount, n = verts.Length; i < n; i++)
					verts[i] = zero;
			}
			return verticesWasResized;
		}

		public static bool EnsureTriangleBuffersSize (ExposedList<SubmeshTriangleBuffer> submeshBuffers, int targetSubmeshCount, SubmeshInstruction[] instructionItems) {
			bool submeshBuffersWasResized = submeshBuffers.Count < targetSubmeshCount;
			if (submeshBuffersWasResized) {
				submeshBuffers.GrowIfNeeded(targetSubmeshCount - submeshBuffers.Count);
				for (int i = submeshBuffers.Count; submeshBuffers.Count < targetSubmeshCount; i++)
					submeshBuffers.Add(new SubmeshTriangleBuffer(instructionItems[i].triangleCount));
			}
			return submeshBuffersWasResized;
		}

		/// <summary>
		/// Fills vertex arrays.
		/// </summary>
		/// <param name="skeleton">Spine.Skeleton source of the drawOrder array</param>
		/// <param name="startSlot">Slot index of the first slot.</param>
		/// <param name="endSlot">The index bounding the slot list. endSlot - 1 is the last slot to be added.</param>
		/// <param name="zSpacing">Spacing along the z-axis between attachments.</param>
		/// <param name="pmaColors">If set to <c>true</c>, vertex colors will be premultiplied. This will also enable additive.</param>
		/// <param name="verts">Vertex positions array. </param>
		/// <param name="uvs">Vertex UV array.</param>
		/// <param name="colors">Vertex color array (Color32).</param>
		/// <param name="vertexIndex">A reference to the running vertex index. This is used when more than one submesh is to be added.</param>
		/// <param name="tempVertBuffer">A temporary vertex position buffer for attachment position values.</param>
		/// <param name="boundsMin">Reference to the running calculated minimum bounds.</param>
		/// <param name="boundsMax">Reference to the running calculated maximum bounds.</param>
		public static void FillVerts (Skeleton skeleton, int startSlot, int endSlot, float zSpacing, bool pmaColors, Vector3[] verts, Vector2[] uvs, Color32[] colors, ref int vertexIndex, ref float[] tempVertBuffer, ref Vector3 boundsMin, ref Vector3 boundsMax) {
			Color32 color;
			var skeletonDrawOrderItems = skeleton.DrawOrder.Items;
			float a = skeleton.a * 255, r = skeleton.r, g = skeleton.g, b = skeleton.b;

			int vi = vertexIndex;
			var tempVerts = tempVertBuffer;
			Vector3 bmin = boundsMin;
			Vector3 bmax = boundsMax;

			// drawOrder[endSlot] is excluded
			for (int slotIndex = startSlot; slotIndex < endSlot; slotIndex++) {
				var slot = skeletonDrawOrderItems[slotIndex];
				var attachment = slot.attachment;
				float z = slotIndex * zSpacing;

				var regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {
					regionAttachment.ComputeWorldVertices(slot.bone, tempVerts);

					float x1 = tempVerts[RegionAttachment.X1], y1 = tempVerts[RegionAttachment.Y1];
					float x2 = tempVerts[RegionAttachment.X2], y2 = tempVerts[RegionAttachment.Y2];
					float x3 = tempVerts[RegionAttachment.X3], y3 = tempVerts[RegionAttachment.Y3];
					float x4 = tempVerts[RegionAttachment.X4], y4 = tempVerts[RegionAttachment.Y4];
					verts[vi].x = x1; verts[vi].y = y1; verts[vi].z = z;
					verts[vi + 1].x = x4; verts[vi + 1].y = y4; verts[vi + 1].z = z;
					verts[vi + 2].x = x2; verts[vi + 2].y = y2; verts[vi + 2].z = z;
					verts[vi + 3].x = x3; verts[vi + 3].y = y3;	verts[vi + 3].z = z;

					if (pmaColors) {
						color.a = (byte)(a * slot.a * regionAttachment.a);
						color.r = (byte)(r * slot.r * regionAttachment.r * color.a);
						color.g = (byte)(g * slot.g * regionAttachment.g * color.a);
						color.b = (byte)(b * slot.b * regionAttachment.b * color.a);
						if (slot.data.blendMode == BlendMode.additive) color.a = 0;
					} else {
						color.a = (byte)(a * slot.a * regionAttachment.a);
						color.r = (byte)(r * slot.r * regionAttachment.r * 255);
						color.g = (byte)(g * slot.g * regionAttachment.g * 255);
						color.b = (byte)(b * slot.b * regionAttachment.b * 255);
					}

					colors[vi] = color; colors[vi + 1] = color; colors[vi + 2] = color; colors[vi + 3] = color;

					float[] regionUVs = regionAttachment.uvs;
					uvs[vi].x = regionUVs[RegionAttachment.X1]; uvs[vi].y = regionUVs[RegionAttachment.Y1];
					uvs[vi + 1].x = regionUVs[RegionAttachment.X4]; uvs[vi + 1].y = regionUVs[RegionAttachment.Y4];
					uvs[vi + 2].x = regionUVs[RegionAttachment.X2]; uvs[vi + 2].y = regionUVs[RegionAttachment.Y2];
					uvs[vi + 3].x = regionUVs[RegionAttachment.X3]; uvs[vi + 3].y = regionUVs[RegionAttachment.Y3];

					// Calculate min/max X
					if (x1 < bmin.x) bmin.x = x1;
					else if (x1 > bmax.x) bmax.x = x1;
					if (x2 < bmin.x) bmin.x = x2;
					else if (x2 > bmax.x) bmax.x = x2;
					if (x3 < bmin.x) bmin.x = x3;
					else if (x3 > bmax.x) bmax.x = x3;
					if (x4 < bmin.x) bmin.x = x4;
					else if (x4 > bmax.x) bmax.x = x4;

					// Calculate min/max Y
					if (y1 < bmin.y) bmin.y = y1;
					else if (y1 > bmax.y) bmax.y = y1;
					if (y2 < bmin.y) bmin.y = y2;
					else if (y2 > bmax.y) bmax.y = y2;
					if (y3 < bmin.y) bmin.y = y3;
					else if (y3 > bmax.y) bmax.y = y3;
					if (y4 < bmin.y) bmin.y = y4;
					else if (y4 > bmax.y) bmax.y = y4;

					vi += 4;
				} else {
					var meshAttachment = attachment as MeshAttachment;
					if (meshAttachment != null) {
						int meshVertexCount = meshAttachment.worldVerticesLength;
						if (tempVerts.Length < meshVertexCount) tempVerts = new float[meshVertexCount];
						meshAttachment.ComputeWorldVertices(slot, tempVerts);

						if (pmaColors) {
							color.a = (byte)(a * slot.a * meshAttachment.a);
							color.r = (byte)(r * slot.r * meshAttachment.r * color.a);
							color.g = (byte)(g * slot.g * meshAttachment.g * color.a);
							color.b = (byte)(b * slot.b * meshAttachment.b * color.a);
							if (slot.data.blendMode == BlendMode.additive) color.a = 0;
						} else {
							color.a = (byte)(a * slot.a * meshAttachment.a);
							color.r = (byte)(r * slot.r * meshAttachment.r * 255);
							color.g = (byte)(g * slot.g * meshAttachment.g * 255);
							color.b = (byte)(b * slot.b * meshAttachment.b * 255);
						}

						float[] attachmentUVs = meshAttachment.uvs;
						for (int iii = 0; iii < meshVertexCount; iii += 2) {
							float x = tempVerts[iii], y = tempVerts[iii + 1];
							verts[vi].x = x; verts[vi].y = y; verts[vi].z = z;
							colors[vi] = color; uvs[vi].x = attachmentUVs[iii]; uvs[vi].y = attachmentUVs[iii + 1];

							if (x < bmin.x) bmin.x = x;
							else if (x > bmax.x) bmax.x = x;

							if (y < bmin.y) bmin.y = y;
							else if (y > bmax.y) bmax.y = y;

							vi++;
						}
					}
				}
			}

			// ref return values
			vertexIndex = vi;
			tempVertBuffer = tempVerts;
			boundsMin = bmin;
			boundsMax = bmax;
		}


		/// <summary>
		/// Fills a submesh triangle buffer array.
		/// </summary>
		/// <param name="skeleton">Spine.Skeleton source of draw order slots.</param>
		/// <param name="triangleCount">The target triangle count.</param>
		/// <param name="firstVertex">First vertex of this submesh.</param>
		/// <param name="startSlot">Start slot.</param>
		/// <param name="endSlot">End slot.</param>
		/// <param name="triangleBuffer">The triangle buffer array to be filled. This reference will be replaced in case the triangle values don't fit.</param>
		/// <param name="bufferTriangleCount">The current triangle count of the submesh buffer. This is not always equal to triangleBuffer.Length because for last submeshes, length may be larger than needed.</param>
		/// <param name="isLastSubmesh">If set to <c>true</c>, the triangle buffer is allowed to be larger than needed.</param>
		public static void FillTriangles (Skeleton skeleton, int triangleCount, int firstVertex, int startSlot, int endSlot, ref int[] triangleBuffer, bool isLastSubmesh) {
			int trianglesCapacity = triangleBuffer.Length;
			var tris = triangleBuffer;

			// Ensure triangleBuffer size.
			if (isLastSubmesh) {
				if (trianglesCapacity > triangleCount) {
					for (int i = triangleCount; i < trianglesCapacity; i++)
						tris[i] = 0;
				} else if (trianglesCapacity < triangleCount) {
					triangleBuffer = tris = new int[triangleCount];
				}
			} else if (trianglesCapacity != triangleCount) {
				triangleBuffer = tris = new int[triangleCount];
			}

			// Iterate through submesh slots and store the triangles. 
			int triangleIndex = 0;
			int afv = firstVertex; // attachment first vertex
			var skeletonDrawOrderItems = skeleton.drawOrder.Items;
			for (int i = startSlot, n = endSlot; i < n; i++) {			
				var attachment = skeletonDrawOrderItems[i].attachment;

				if (attachment is RegionAttachment) {
					tris[triangleIndex] = afv; tris[triangleIndex + 1] = afv + 2; tris[triangleIndex + 2] = afv + 1;
					tris[triangleIndex + 3] = afv + 2; tris[triangleIndex + 4] = afv + 3; tris[triangleIndex + 5] = afv + 1;
					triangleIndex += 6;
					afv += 4;
				} else {
					int[] attachmentTriangles;
					int attachmentVertexCount;
					var meshAttachment = attachment as MeshAttachment;
					if (meshAttachment != null) {
						attachmentVertexCount = meshAttachment.worldVerticesLength >> 1; //  length/2
						attachmentTriangles = meshAttachment.triangles;
						for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii++, triangleIndex++)
							tris[triangleIndex] = afv + attachmentTriangles[ii];

						afv += attachmentVertexCount;
					}
				}
			} // Done adding current submesh triangles
		}


		public static Bounds ToBounds (Vector3 boundsMin, Vector3 boundsMax) {
			Vector3 size = (boundsMax - boundsMin);
			Vector3 center = boundsMin + size * 0.5f;
			return new Bounds(center, size);
		}

		#region SubmeshTriangleBuffer
		public class SubmeshTriangleBuffer {
			public int[] triangles;
			//public int triangleCount;

			public SubmeshTriangleBuffer (int triangleCount) {
				triangles = new int[triangleCount];
			}
		}
		#endregion

	}
}

