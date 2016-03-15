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

namespace Spine.Unity.MeshGeneration {
	public class ArraysSimpleMeshGenerator : ISimpleMeshGenerator {
		#region Settings
		protected float scale = 1f;
		public float Scale {
			get { return scale; }
			set { scale = value; }
		}

		public bool renderMeshes = true;

		public bool premultiplyVertexColors = true;
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
			const float zSpacing = 0;
			const float zFauxHalfThickness = 0.01f;	// Somehow needs this thickness for bounds to work properly in some cases (eg, Unity UI clipping)
			float[] tempVertices = this.tempVertices;
			Vector2[] uvs = this.uvs;
			Color32[] colors = this.colors;

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

				int vertexIndex = 0;
				ArraysBuffers.Fill(skeleton, 0, drawOrderCount, zSpacing, this.premultiplyVertexColors, vertices, uvs, colors, ref vertexIndex, ref tempVertices, ref meshBoundsMin, ref meshBoundsMax);
				this.tempVertices = tempVertices;

				// Apply scale to vertices
				for (int i = 0; i < totalVertexCount; i++) {
					var v = vertices[i];
					v.x *= scale;
					v.y *= scale;
					vertices[i] = v;
				}

				meshBoundsMax.x *= scale;
				meshBoundsMax.y *= scale;
				meshBoundsMin.x *= scale;
				meshBoundsMax.y *= scale;

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
