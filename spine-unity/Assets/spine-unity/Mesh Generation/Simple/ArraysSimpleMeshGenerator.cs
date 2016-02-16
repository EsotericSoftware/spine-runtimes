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

using UnityEngine;
using System.Collections;

namespace Spine.Unity {
	public class ArraysSimpleMeshGenerator : ISimpleMeshGenerator {
		#region Settings
		protected float scale = 1f;
		public float Scale {
			get { return scale; }
			set { scale = value; }
		}

		public bool renderMeshes = true;
		#endregion

		#region Buffers
		readonly DoubleBufferedMesh doubleBufferedMesh = new DoubleBufferedMesh();
		private float[] tempVertices = new float[8];
		private Vector3[] vertices;
		private Color32[] colors;
		private Vector2[] uvs;
		private int[] triangles;
		#endregion

		private Mesh lastGeneratedMesh;
		public Mesh LastGeneratedMesh {	get { return lastGeneratedMesh; } }

		public Mesh GenerateMesh (Skeleton skeleton) {
			int totalVertexCount = 0; // size of vertex arrays
			int totalTriangleCount = 0; // size of index array

			// Step 1 : Count verts and tris to determine array sizes.
			//
			var drawOrderItems = skeleton.drawOrder.Items;
			int drawOrderCount = skeleton.drawOrder.Count;
			for (int i = 0; i < drawOrderCount; i++) {
				Slot slot = drawOrderItems[i];
				Attachment attachment = slot.attachment;
				int attachmentVertexCount, attachmentTriangleCount;
				var regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {					
					attachmentVertexCount = 4;
					attachmentTriangleCount = 6;
				} else {
					if (!renderMeshes) continue;
					var meshAttachment = attachment as MeshAttachment;
					if (meshAttachment != null) {
						attachmentVertexCount = meshAttachment.vertices.Length >> 1;
						attachmentTriangleCount = meshAttachment.triangles.Length;
					} else {
						var skinnedMeshAttachment = attachment as WeightedMeshAttachment;
						if (skinnedMeshAttachment != null) {
							attachmentVertexCount = skinnedMeshAttachment.uvs.Length >> 1;
							attachmentTriangleCount = skinnedMeshAttachment.triangles.Length;
						} else
							continue;
					}
				}
				totalTriangleCount += attachmentTriangleCount;
				totalVertexCount += attachmentVertexCount;
			}


			// Step 2 : Prepare vertex arrays.
			//
			Vector3[] vertices = this.vertices;
			bool verticesDontFit = vertices == null || totalVertexCount > vertices.Length;
			if (verticesDontFit) {
				// Not enough space, increase size.
				this.vertices = vertices = new Vector3[totalVertexCount];
				this.colors = new Color32[totalVertexCount];
				this.uvs = new Vector2[totalVertexCount];

			} else {
				// Too many vertices, zero the extra.
				Vector3 zero = Vector3.zero;
				for (int i = totalVertexCount, n = vertices.Length; i < n; i++)
					vertices[i] = zero;
			}
				

			// Step 3 : Push vertices to arrays
			//
			const float z = 0;
			const float zFauxHalfThickness = 0.01f;	// Somehow needs this thickness for bounds to work properly in some cases (eg, Unity UI clipping)
			float[] tempVertices = this.tempVertices;
			Vector2[] uvs = this.uvs;
			Color32[] colors = this.colors;
			int vertexIndex = 0;
			Color32 color;
			float a = skeleton.a * 255, r = skeleton.r, g = skeleton.g, b = skeleton.b;

			Vector3 meshBoundsMin;
			Vector3 meshBoundsMax;
			if (totalVertexCount == 0) {
				meshBoundsMin = new Vector3(0, 0, 0);
				meshBoundsMax = new Vector3(0, 0, 0);
			} else {
				meshBoundsMin.x = int.MaxValue;
				meshBoundsMin.y = int.MaxValue;
				meshBoundsMax.x = int.MinValue;
				meshBoundsMax.y = int.MinValue;
				meshBoundsMin.z = -zFauxHalfThickness;
				meshBoundsMax.z = zFauxHalfThickness;

				int i = 0;
				do {
					Slot slot = drawOrderItems[i];
					Attachment attachment = slot.attachment;
					var regionAttachment = attachment as RegionAttachment;
					if (regionAttachment != null) {
						regionAttachment.ComputeWorldVertices(slot.bone, tempVertices);

						float x1 = tempVertices[RegionAttachment.X1], y1 = tempVertices[RegionAttachment.Y1];
						float x2 = tempVertices[RegionAttachment.X2], y2 = tempVertices[RegionAttachment.Y2];
						float x3 = tempVertices[RegionAttachment.X3], y3 = tempVertices[RegionAttachment.Y3];
						float x4 = tempVertices[RegionAttachment.X4], y4 = tempVertices[RegionAttachment.Y4];
						vertices[vertexIndex].x = x1 * scale; vertices[vertexIndex].y = y1 * scale;	vertices[vertexIndex].z = z;
						vertices[vertexIndex + 1].x = x4 * scale; vertices[vertexIndex + 1].y = y4 * scale; vertices[vertexIndex + 1].z = z;
						vertices[vertexIndex + 2].x = x2 * scale; vertices[vertexIndex + 2].y = y2 * scale; vertices[vertexIndex + 2].z = z;
						vertices[vertexIndex + 3].x = x3 * scale; vertices[vertexIndex + 3].y = y3 * scale; vertices[vertexIndex + 3].z = z;

						color.a = (byte)(a * slot.a * regionAttachment.a);
						color.r = (byte)(r * slot.r * regionAttachment.r * color.a);
						color.g = (byte)(g * slot.g * regionAttachment.g * color.a);
						color.b = (byte)(b * slot.b * regionAttachment.b * color.a);
						if (slot.data.blendMode == BlendMode.additive) color.a = 0;
						colors[vertexIndex] = color; colors[vertexIndex + 1] = color; colors[vertexIndex + 2] = color; colors[vertexIndex + 3] = color;

						float[] regionUVs = regionAttachment.uvs;
						uvs[vertexIndex].x = regionUVs[RegionAttachment.X1]; uvs[vertexIndex].y = regionUVs[RegionAttachment.Y1];
						uvs[vertexIndex + 1].x = regionUVs[RegionAttachment.X4]; uvs[vertexIndex + 1].y = regionUVs[RegionAttachment.Y4];
						uvs[vertexIndex + 2].x = regionUVs[RegionAttachment.X2]; uvs[vertexIndex + 2].y = regionUVs[RegionAttachment.Y2];
						uvs[vertexIndex + 3].x = regionUVs[RegionAttachment.X3]; uvs[vertexIndex + 3].y = regionUVs[RegionAttachment.Y3];

						// Calculate min/max X
						if (x1 < meshBoundsMin.x) meshBoundsMin.x = x1;
						else if (x1 > meshBoundsMax.x) meshBoundsMax.x = x1;
						if (x2 < meshBoundsMin.x) meshBoundsMin.x = x2;
						else if (x2 > meshBoundsMax.x) meshBoundsMax.x = x2;
						if (x3 < meshBoundsMin.x) meshBoundsMin.x = x3;
						else if (x3 > meshBoundsMax.x) meshBoundsMax.x = x3;
						if (x4 < meshBoundsMin.x) meshBoundsMin.x = x4;
						else if (x4 > meshBoundsMax.x) meshBoundsMax.x = x4;

						// Calculate min/max Y
						if (y1 < meshBoundsMin.y) meshBoundsMin.y = y1;
						else if (y1 > meshBoundsMax.y) meshBoundsMax.y = y1;
						if (y2 < meshBoundsMin.y) meshBoundsMin.y = y2;
						else if (y2 > meshBoundsMax.y) meshBoundsMax.y = y2;
						if (y3 < meshBoundsMin.y) meshBoundsMin.y = y3;
						else if (y3 > meshBoundsMax.y) meshBoundsMax.y = y3;
						if (y4 < meshBoundsMin.y) meshBoundsMin.y = y4;
						else if (y4 > meshBoundsMax.y) meshBoundsMax.y = y4;

						vertexIndex += 4;
					} else {
						if (!renderMeshes) continue;
						var meshAttachment = attachment as MeshAttachment;
						if (meshAttachment != null) {
							int meshVertexCount = meshAttachment.vertices.Length;
							if (tempVertices.Length < meshVertexCount)
								this.tempVertices = tempVertices = new float[meshVertexCount];
							meshAttachment.ComputeWorldVertices(slot, tempVertices);

							color.a = (byte)(a * slot.a * meshAttachment.a);
							color.r = (byte)(r * slot.r * meshAttachment.r * color.a);
							color.g = (byte)(g * slot.g * meshAttachment.g * color.a);
							color.b = (byte)(b * slot.b * meshAttachment.b * color.a);
							if (slot.data.blendMode == BlendMode.additive) color.a = 0;

							float[] meshUVs = meshAttachment.uvs;
							for (int ii = 0; ii < meshVertexCount; ii += 2, vertexIndex++) {
								float x = tempVertices[ii], y = tempVertices[ii + 1];
								vertices[vertexIndex].x = x * scale; vertices[vertexIndex].y = y * scale; vertices[vertexIndex].z = z;
								colors[vertexIndex] = color;
								uvs[vertexIndex].x = meshUVs[ii]; uvs[vertexIndex].y = meshUVs[ii + 1];

								if (x < meshBoundsMin.x) meshBoundsMin.x = x;
								else if (x > meshBoundsMax.x) meshBoundsMax.x = x;

								if (y < meshBoundsMin.y) meshBoundsMin.y = y;
								else if (y > meshBoundsMax.y) meshBoundsMax.y = y;
							}
						} else {
							var skinnedMeshAttachment = attachment as WeightedMeshAttachment;
							if (skinnedMeshAttachment != null) {
								int meshVertexCount = skinnedMeshAttachment.uvs.Length;
								if (tempVertices.Length < meshVertexCount)
									this.tempVertices = tempVertices = new float[meshVertexCount];
								skinnedMeshAttachment.ComputeWorldVertices(slot, tempVertices);

								color.a = (byte)(a * slot.a * skinnedMeshAttachment.a);
								color.r = (byte)(r * slot.r * skinnedMeshAttachment.r * color.a);
								color.g = (byte)(g * slot.g * skinnedMeshAttachment.g * color.a);
								color.b = (byte)(b * slot.b * skinnedMeshAttachment.b * color.a);
								if (slot.data.blendMode == BlendMode.additive) color.a = 0;

								float[] meshUVs = skinnedMeshAttachment.uvs;
								for (int ii = 0; ii < meshVertexCount; ii += 2, vertexIndex++) {
									float x = tempVertices[ii], y = tempVertices[ii + 1];
									vertices[vertexIndex].x = x * scale; vertices[vertexIndex].y = y * scale; vertices[vertexIndex].z = z;
									colors[vertexIndex] = color;
									uvs[vertexIndex].x = meshUVs[ii]; uvs[vertexIndex].y = meshUVs[ii + 1];

									if (x < meshBoundsMin.x) meshBoundsMin.x = x;
									else if (x > meshBoundsMax.x) meshBoundsMax.x = x;

									if (y < meshBoundsMin.y) meshBoundsMin.y = y;
									else if (y > meshBoundsMax.y) meshBoundsMax.y = y;
								}
							}
						}
					}
				} while (++i < drawOrderCount);
			}


			// Step 3 : Ensure correct triangle array size
			// 
			var triangles = this.triangles;
			bool trianglesDontFit = triangles == null || totalTriangleCount > triangles.Length;
			if (trianglesDontFit) {
				// Not enough space, increase size
				this.triangles = triangles = new int[totalTriangleCount];
			} else {				
				// Too many indices, zero the extra.
				for (int i = totalTriangleCount, n = triangles.Length; i < n; i++)
					triangles[i] = 0;
			}


			// Step 4 : Push triangles to triangle array.
			//
			int triangleArrayIndex = 0; // next triangle index. modified by loop
			int firstAttachmentVertex = 0;
			for (int i = 0, n = drawOrderCount; i < n; i++) {			
				Attachment attachment = drawOrderItems[i].attachment;

				if (attachment is RegionAttachment) {
					triangles[triangleArrayIndex] = firstAttachmentVertex;
					triangles[triangleArrayIndex + 1] = firstAttachmentVertex + 2;
					triangles[triangleArrayIndex + 2] = firstAttachmentVertex + 1;
					triangles[triangleArrayIndex + 3] = firstAttachmentVertex + 2;
					triangles[triangleArrayIndex + 4] = firstAttachmentVertex + 3;
					triangles[triangleArrayIndex + 5] = firstAttachmentVertex + 1;

					triangleArrayIndex += 6;
					firstAttachmentVertex += 4;
					continue;
				} else {
					if (!renderMeshes) continue;
					int[] attachmentTriangles;
					int attachmentVertexCount;
					var meshAttachment = attachment as MeshAttachment;
					if (meshAttachment != null) {
						attachmentVertexCount = meshAttachment.vertices.Length >> 1; //  length/2
						attachmentTriangles = meshAttachment.triangles;
					} else {
						var skinnedMeshAttachment = attachment as WeightedMeshAttachment;
						if (skinnedMeshAttachment != null) {
							attachmentVertexCount = skinnedMeshAttachment.uvs.Length >> 1; // length/2
							attachmentTriangles = skinnedMeshAttachment.triangles;
						} else
							continue;
					}

					for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii++, triangleArrayIndex++)
						triangles[triangleArrayIndex] = firstAttachmentVertex + attachmentTriangles[ii];

					firstAttachmentVertex += attachmentVertexCount;
				}
			}


			// Step 5 : Push Data To Mesh
			//
			var mesh = doubleBufferedMesh.GetNextMesh();
			mesh.vertices = vertices;
			mesh.colors32 = colors;
			mesh.uv = uvs;

			Vector3 meshBoundsExtents = (meshBoundsMax - meshBoundsMin) * scale;
			Vector3 meshCenter = (meshBoundsMin * scale) + meshBoundsExtents * 0.5f;
			mesh.bounds = new Bounds(meshCenter, meshBoundsExtents);

			mesh.SetTriangles(triangles, 0);

			lastGeneratedMesh = mesh;
			return mesh;
		}

	}

}
