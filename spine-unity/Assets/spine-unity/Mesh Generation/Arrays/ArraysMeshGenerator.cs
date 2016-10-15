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

#define SPINE_OPTIONAL_NORMALS
using UnityEngine;

namespace Spine.Unity.MeshGeneration {
	public class ArraysMeshGenerator {
		#region Settings
		public bool PremultiplyVertexColors { get; set; }
		protected bool addNormals;
		public bool AddNormals { get { return addNormals; } set { addNormals = value; } }
		protected bool addTangents;
		public bool AddTangents { get { return addTangents; } set { addTangents = value; } }
		#endregion

		protected float[] attachmentVertexBuffer = new float[8];
		protected Vector3[] meshVertices;
		protected Color32[] meshColors32;
		protected Vector2[] meshUVs;

		#if SPINE_OPTIONAL_NORMALS
		protected Vector3[] meshNormals;
		#endif
		protected Vector4[] meshTangents;
		protected Vector2[] tempTanBuffer;

		public void TryAddNormalsTo (Mesh mesh, int targetVertexCount) {
			#if SPINE_OPTIONAL_NORMALS
			if (addNormals) {
				bool verticesWasResized = this.meshNormals == null || meshNormals.Length < targetVertexCount;
				if (verticesWasResized) {
					this.meshNormals = new Vector3[targetVertexCount];
					Vector3 fixedNormal = new Vector3(0, 0, -1f);
					Vector3[] normals = this.meshNormals;
					for (int i = 0; i < targetVertexCount; i++)
						normals[i] = fixedNormal;
				}

				mesh.normals = this.meshNormals;
			}
			#endif
		}

		/// <summary>Ensures the sizes of the passed array references. If they are not the correct size, a new array will be assigned to the references.</summary>
		/// <returns><c>true</c>, if a resize occurred, <c>false</c> otherwise.</returns>
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

		/// <summary>Fills Unity vertex data buffers with verts from the Spine Skeleton.</summary>
		/// <param name="skeleton">Spine.Skeleton source of the drawOrder array</param>
		/// <param name="startSlot">Slot index of the first slot.</param>
		/// <param name="endSlot">The index bounding the slot list. [endSlot - 1] is the last slot to be added.</param>
		/// <param name="zSpacing">Spacing along the z-axis between attachments.</param>
		/// <param name="pmaColors">If set to <c>true</c>, vertex colors will be premultiplied. This will also enable additive.</param>
		/// <param name="verts">Vertex positions array. </param>
		/// <param name="uvs">Vertex UV array.</param>
		/// <param name="colors">Vertex color array (Color32).</param>
		/// <param name="vertexIndex">A reference to the running vertex index. This is used when more than one submesh is to be added.</param>
		/// <param name="tempVertBuffer">A temporary vertex position buffer for attachment position values.</param>
		/// <param name="boundsMin">Reference to the running calculated minimum bounds.</param>
		/// <param name="boundsMax">Reference to the running calculated maximum bounds.</param>
		/// <param name = "renderMeshes">Include MeshAttachments. If false, it will ignore MeshAttachments.</param>
		public static void FillVerts (Skeleton skeleton, int startSlot, int endSlot, float zSpacing, bool pmaColors, Vector3[] verts, Vector2[] uvs, Color32[] colors, ref int vertexIndex, ref float[] tempVertBuffer, ref Vector3 boundsMin, ref Vector3 boundsMax, bool renderMeshes = true) {
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

					if (x1 < bmin.x) bmin.x = x1; // Potential first attachment bounds initialization. Initial min should not block initial max. Same for Y below.
					if (x1 > bmax.x) bmax.x = x1;
					if (x2 < bmin.x) bmin.x = x2;
					else if (x2 > bmax.x) bmax.x = x2;
					if (x3 < bmin.x) bmin.x = x3;
					else if (x3 > bmax.x) bmax.x = x3;
					if (x4 < bmin.x) bmin.x = x4;
					else if (x4 > bmax.x) bmax.x = x4;

					if (y1 < bmin.y) bmin.y = y1;
					if (y1 > bmax.y) bmax.y = y1;
					if (y2 < bmin.y) bmin.y = y2;
					else if (y2 > bmax.y) bmax.y = y2;
					if (y3 < bmin.y) bmin.y = y3;
					else if (y3 > bmax.y) bmax.y = y3;
					if (y4 < bmin.y) bmin.y = y4;
					else if (y4 > bmax.y) bmax.y = y4;

					vi += 4;
				} else if (renderMeshes) {
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

						// Potential first attachment bounds initialization. See conditions in RegionAttachment logic.
						if (vi == vertexIndex) {
							// Initial min should not block initial max.
							// vi == vertexIndex does not always mean the bounds are fresh. It could be a submesh. Do not nuke old values by omitting the check.
							// Should know that this is the first attachment in the submesh. slotIndex == startSlot could be an empty slot.
							float fx = tempVerts[0], fy = tempVerts[1];
							if (fx < bmin.x) bmin.x = fx;
							if (fx > bmax.x) bmax.x = fx;
							if (fy < bmin.y) bmin.y = fy;
							if (fy > bmax.y) bmax.y = fy;
						}

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


		/// <summary>Fills a submesh triangle buffer array.</summary>
		/// <param name="skeleton">Spine.Skeleton source of draw order slots.</param>
		/// <param name="triangleCount">The target triangle count.</param>
		/// <param name="firstVertex">First vertex of this submesh.</param>
		/// <param name="startSlot">Start slot.</param>
		/// <param name="endSlot">End slot.</param>
		/// <param name="triangleBuffer">The triangle buffer array to be filled. This reference will be replaced in case the triangle values don't fit.</param>
		/// <param name="isLastSubmesh">If set to <c>true</c>, the triangle buffer is allowed to be larger than needed.</param>
		public static void FillTriangles (ref int[] triangleBuffer, Skeleton skeleton, int triangleCount, int firstVertex, int startSlot, int endSlot, bool isLastSubmesh) {
			int trianglesCapacity = triangleBuffer.Length;
			int[] tris = triangleBuffer;

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

			var skeletonDrawOrderItems = skeleton.drawOrder.Items;
			for (int i = startSlot, n = endSlot, ti = 0, afv = firstVertex; i < n; i++) {			
				var attachment = skeletonDrawOrderItems[i].attachment;

				// RegionAttachment
				if (attachment is RegionAttachment) {
					tris[ti] = afv;
					tris[ti + 1] = afv + 2;
					tris[ti + 2] = afv + 1;
					tris[ti + 3] = afv + 2;
					tris[ti + 4] = afv + 3;
					tris[ti + 5] = afv + 1;
					ti += 6;
					afv += 4;
					continue;
				}

				// MeshAttachment
				var meshAttachment = attachment as MeshAttachment;
				if (meshAttachment != null) {
					int[] attachmentTriangles = meshAttachment.triangles;
					for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii++, ti++)
						tris[ti] = afv + attachmentTriangles[ii];

					afv += meshAttachment.worldVerticesLength >> 1; // length/2;
				}

			}
		}

		public static void FillTrianglesQuads (ref int[] triangleBuffer, ref int storedTriangleCount, ref int storedFirstVertex, int instructionsFirstVertex, int instructionTriangleCount, bool isLastSubmesh) {
			int trianglesCapacity = triangleBuffer.Length;

			if (isLastSubmesh && trianglesCapacity > instructionTriangleCount) {
				for (int i = instructionTriangleCount; i < trianglesCapacity; i++)
					triangleBuffer[i] = 0;
				storedTriangleCount = instructionTriangleCount;
			} else if (trianglesCapacity != instructionTriangleCount) {
				triangleBuffer = new int[instructionTriangleCount];
				storedTriangleCount = 0;
			}

			// Use stored quad triangles if possible.
			int[] tris = triangleBuffer;
			if (storedFirstVertex != instructionsFirstVertex || storedTriangleCount < instructionTriangleCount) { //|| storedTriangleCount == 0
				storedTriangleCount = instructionTriangleCount;
				storedFirstVertex = instructionsFirstVertex;
				int afv = instructionsFirstVertex; // attachment first vertex
				for (int ti = 0; ti < instructionTriangleCount; ti += 6, afv += 4) {
					tris[ti] = afv;
					tris[ti + 1] = afv + 2;
					tris[ti + 2] = afv + 1;
					tris[ti + 3] = afv + 2;
					tris[ti + 4] = afv + 3;
					tris[ti + 5] = afv + 1;
				}
			}
		}

		/// <summary>Creates a UnityEngine.Bounds struct from minimum and maximum value vectors.</summary>
		public static Bounds ToBounds (Vector3 boundsMin, Vector3 boundsMax) {
			Vector3 size = (boundsMax - boundsMin);
			return new Bounds((boundsMin + (size * 0.5f)), size);
		}

		#region TangentSolver2D
		// Thanks to contributions from forum user ToddRivers

		/// <summary>Step 1 of solving tangents. Ensure you have buffers of the correct size.</summary>
		/// <param name="tangentBuffer">Eventual Vector4[] tangent buffer to assign to Mesh.tangents.</param>
		/// <param name="tempTanBuffer">Temporary Vector2 buffer for calculating directions.</param>
		/// <param name="vertexCount">Number of vertices that require tangents (or the size of the vertex array)</param>
		public static void SolveTangents2DEnsureSize (ref Vector4[] tangentBuffer, ref Vector2[] tempTanBuffer, int vertexCount) {
			if (tangentBuffer == null || tangentBuffer.Length < vertexCount)
				tangentBuffer = new Vector4[vertexCount];
			
			if (tempTanBuffer == null || tempTanBuffer.Length < vertexCount * 2)
				tempTanBuffer = new Vector2[vertexCount * 2]; // two arrays in one.
		}

		/// <summary>Step 2 of solving tangents. Fills (part of) a temporary tangent-solution buffer based on the vertices and uvs defined by a submesh's triangle buffer. Only needs to be called once for single-submesh meshes.</summary>
		/// <param name="tempTanBuffer">A temporary Vector3[] for calculating tangents.</param>
		/// <param name="vertices">The mesh's current vertex position buffer.</param>
		/// <param name="triangles">The mesh's current triangles buffer.</param>
		/// <param name="uvs">The mesh's current uvs buffer.</param>
		/// <param name="vertexCount">Number of vertices that require tangents (or the size of the vertex array)</param>
		/// <param name = "triangleCount">The number of triangle indexes in the triangle array to be used.</param>
		public static void SolveTangents2DTriangles (Vector2[] tempTanBuffer, int[] triangles, int triangleCount, Vector3[] vertices, Vector2[] uvs, int vertexCount) {
			Vector2 sdir;
			Vector2 tdir;
			for (int t = 0; t < triangleCount; t += 3) {
				int i1 = triangles[t + 0];
				int i2 = triangles[t + 1];
				int i3 = triangles[t + 2];

				Vector3 v1 = vertices[i1];
				Vector3 v2 = vertices[i2];
				Vector3 v3 = vertices[i3];

				Vector2 w1 = uvs[i1];
				Vector2 w2 = uvs[i2];
				Vector2 w3 = uvs[i3];

				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;

				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;

				float div = s1 * t2 - s2 * t1;
				float r = (div == 0f) ? 0f : 1f / div;

				sdir.x = (t2 * x1 - t1 * x2) * r;
				sdir.y = (t2 * y1 - t1 * y2) * r;
				tempTanBuffer[i1] = tempTanBuffer[i2] = tempTanBuffer[i3] = sdir;

				tdir.x = (s1 * x2 - s2 * x1) * r;
				tdir.y = (s1 * y2 - s2 * y1) * r;
				tempTanBuffer[vertexCount + i1] = tempTanBuffer[vertexCount + i2] = tempTanBuffer[vertexCount + i3] = tdir;
			}
		}

		/// <summary>Step 3 of solving tangents. Fills a Vector4[] tangents array according to values calculated in step 2.</summary>
		/// <param name="tangents">A Vector4[] that will eventually be used to set Mesh.tangents</param>
		/// <param name="tempTanBuffer">A temporary Vector3[] for calculating tangents.</param>
		/// <param name="vertexCount">Number of vertices that require tangents (or the size of the vertex array)</param>
		public static void SolveTangents2DBuffer (Vector4[] tangents, Vector2[] tempTanBuffer, int vertexCount) {

			Vector4 tangent;
			tangent.z = 0;
			for (int i = 0; i < vertexCount; ++i) {
				Vector2 t = tempTanBuffer[i]; 

				// t.Normalize() (aggressively inlined). Even better if offloaded to GPU via vertex shader.
				float magnitude = Mathf.Sqrt(t.x * t.x + t.y * t.y);
				if (magnitude > 1E-05) {
					float reciprocalMagnitude = 1f/magnitude;
					t.x *= reciprocalMagnitude;
					t.y *= reciprocalMagnitude;
				}

				Vector2 t2 = tempTanBuffer[vertexCount + i];
				tangent.x = t.x;
				tangent.y = t.y;
				//tangent.z = 0;
				tangent.w = (t.y * t2.x > t.x * t2.y) ? 1 : -1; // 2D direction calculation. Used for binormals.
				tangents[i] = tangent;
			}

		}
		#endregion

		#region SubmeshTriangleBuffer
		public class SubmeshTriangleBuffer {
			public int[] triangles;
			public int triangleCount; // for last/single submeshes with potentially zeroed triangles.
			public int firstVertex = -1; // for !renderMeshes.

			public SubmeshTriangleBuffer () { }

			public SubmeshTriangleBuffer (int triangleCount) {
				triangles = new int[triangleCount];
			}
		}
		#endregion

	}
}
