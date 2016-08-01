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

namespace Spine.Unity.MeshGeneration {
	public class ArraysSimpleMeshGenerator : ArraysMeshGenerator, ISimpleMeshGenerator {
		#region Settings
		protected float scale = 1f;
		public float Scale { get { return scale; } set { scale = value; } }
		public float ZSpacing { get; set; }
		#endregion

		protected Mesh lastGeneratedMesh;
		public Mesh LastGeneratedMesh {	get { return lastGeneratedMesh; } }

		readonly DoubleBufferedMesh doubleBufferedMesh = new DoubleBufferedMesh();
		int[] triangles;

		public Mesh GenerateMesh (Skeleton skeleton) {
			int totalVertexCount = 0; // size of vertex arrays
			int totalTriangleCount = 0; // size of index array

			// STEP 1 : GenerateInstruction(). Count verts and tris to determine array sizes.
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
					var meshAttachment = attachment as MeshAttachment;
					if (meshAttachment != null) {
						attachmentVertexCount = meshAttachment.worldVerticesLength >> 1;
						attachmentTriangleCount = meshAttachment.triangles.Length;
					} else {
						continue;
					}
				}
				totalTriangleCount += attachmentTriangleCount;
				totalVertexCount += attachmentVertexCount;
			}

			// STEP 2 : Ensure buffers are the correct size
			ArraysMeshGenerator.EnsureSize(totalVertexCount, ref this.meshVertices, ref this.meshUVs, ref this.meshColors32);
			this.triangles = this.triangles ?? new int[totalTriangleCount];
				
			// STEP 3 : Update vertex buffer
			const float zFauxHalfThickness = 0.01f;	// Somehow needs this thickness for bounds to work properly in some cases (eg, Unity UI clipping)
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
				meshBoundsMin.z = -zFauxHalfThickness * scale;
				meshBoundsMax.z = zFauxHalfThickness * scale;

				int vertexIndex = 0;
				ArraysMeshGenerator.FillVerts(skeleton, 0, drawOrderCount, this.ZSpacing, this.PremultiplyVertexColors, this.meshVertices, this.meshUVs, this.meshColors32, ref vertexIndex, ref this.attachmentVertexBuffer, ref meshBoundsMin, ref meshBoundsMax);

				// Apply scale to vertices
				meshBoundsMax.x *= scale; meshBoundsMax.y *= scale;
				meshBoundsMin.x *= scale; meshBoundsMax.y *= scale;
				var vertices = this.meshVertices;
				for (int i = 0; i < totalVertexCount; i++) {
					Vector3 p = vertices[i];
					p.x *= scale;
					p.y *= scale;
					vertices[i] = p;
				}
			}
				
			// Step 4 : Update Triangles buffer
			ArraysMeshGenerator.FillTriangles(ref this.triangles, skeleton, totalTriangleCount, 0, 0, drawOrderCount, true);

			// Step 5 : Update Mesh with buffers
			var mesh = doubleBufferedMesh.GetNextMesh();
			mesh.vertices = this.meshVertices;
			mesh.colors32 = meshColors32;
			mesh.uv = meshUVs;
			mesh.bounds = ArraysMeshGenerator.ToBounds(meshBoundsMin, meshBoundsMax);
			mesh.triangles = triangles;
			TryAddNormalsTo(mesh, totalVertexCount);

			if (addTangents) { 
				SolveTangents2DEnsureSize(ref this.meshTangents, ref this.tempTanBuffer, totalVertexCount);
				SolveTangents2DTriangles(this.tempTanBuffer, triangles, totalTriangleCount, meshVertices, meshUVs, totalVertexCount);
				SolveTangents2DBuffer(this.meshTangents, this.tempTanBuffer, totalVertexCount);
			}

			lastGeneratedMesh = mesh;
			return mesh;
		}

	}

}
