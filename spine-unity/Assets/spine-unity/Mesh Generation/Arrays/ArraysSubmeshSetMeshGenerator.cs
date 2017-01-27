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

using UnityEngine;

namespace Spine.Unity.MeshGeneration {
	public class ArraysSubmeshSetMeshGenerator : ArraysMeshGenerator, ISubmeshSetMeshGenerator {
		#region Settings
		public float ZSpacing { get; set; }
		#endregion

		readonly DoubleBuffered<SmartMesh> doubleBufferedSmartMesh = new DoubleBuffered<SmartMesh>();
		readonly ExposedList<SubmeshInstruction> currentInstructions = new ExposedList<SubmeshInstruction>();
		readonly ExposedList<Attachment> attachmentBuffer = new ExposedList<Attachment>();
		readonly ExposedList<SubmeshTriangleBuffer> submeshBuffers = new ExposedList<SubmeshTriangleBuffer>();
		Material[] sharedMaterials = new Material[0];

		/// <summary>
		/// Generates a mesh based on a subset of instructions.
		/// </summary>
		/// <returns>A UnityEngine.Mesh.</returns>
		/// <param name="instructions">A list of SubmeshInstructions.</param>
		/// <param name="startSubmesh">The index of the starting submesh.</param>
		/// <param name="endSubmesh">The exclusive upper bound of the last submesh to be included.</param>
		public MeshAndMaterials GenerateMesh (ExposedList<SubmeshInstruction> instructions, int startSubmesh, int endSubmesh, float scale = 1f) {
			// STEP 0: Prepare instructions.
			var paramItems = instructions.Items;
			currentInstructions.Clear(false);
			for (int i = startSubmesh, n = endSubmesh; i < n; i++) {
				this.currentInstructions.Add(paramItems[i]);
			}
			var smartMesh = doubleBufferedSmartMesh.GetNext();
			var mesh = smartMesh.mesh;
			int submeshCount = currentInstructions.Count;
			var currentInstructionsItems = currentInstructions.Items;
			int vertexCount = 0;
			for (int i = 0; i < submeshCount; i++) {
				currentInstructionsItems[i].firstVertexIndex = vertexCount;// Ensure current instructions have correct cached values.
				vertexCount += currentInstructionsItems[i].vertexCount; // vertexCount will also be used for the rest of this method.
			}

			// STEP 1: Ensure correct buffer sizes.
			bool vertBufferResized = ArraysMeshGenerator.EnsureSize(vertexCount, ref this.meshVertices, ref this.meshUVs, ref this.meshColors32); 
			bool submeshBuffersResized = ArraysMeshGenerator.EnsureTriangleBuffersSize(submeshBuffers, submeshCount, currentInstructionsItems);

			// STEP 2: Update buffers based on Skeleton.

			// Initial values for manual Mesh Bounds calculation
			Vector3 meshBoundsMin;
			Vector3 meshBoundsMax;
			float zSpacing = this.ZSpacing;
			if (vertexCount <= 0) {
				meshBoundsMin = new Vector3(0, 0, 0);
				meshBoundsMax = new Vector3(0, 0, 0);
			} else {
				meshBoundsMin.x = int.MaxValue;
				meshBoundsMin.y = int.MaxValue;
				meshBoundsMax.x = int.MinValue;
				meshBoundsMax.y = int.MinValue;

				int endSlot = currentInstructionsItems[submeshCount - 1].endSlot;
				if (zSpacing > 0f) {
					meshBoundsMin.z = 0f;
					meshBoundsMax.z = zSpacing * endSlot;
				} else {
					meshBoundsMin.z = zSpacing * endSlot;
					meshBoundsMax.z = 0f;
				}
			}
				
			// For each submesh, add vertex data from attachments.
			var workingAttachments = this.attachmentBuffer;
			workingAttachments.Clear(false);
			int vertexIndex = 0; // modified by FillVerts
			for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++) {
				var currentInstruction = currentInstructionsItems[submeshIndex];
				int startSlot = currentInstruction.startSlot;
				int endSlot = currentInstruction.endSlot;
				var skeleton = currentInstruction.skeleton;
				var skeletonDrawOrderItems = skeleton.DrawOrder.Items;
				for (int i = startSlot; i < endSlot; i++) {
					var ca = skeletonDrawOrderItems[i].attachment;
					if (ca != null) workingAttachments.Add(ca); // Includes BoundingBoxes. This is ok.
				}
				ArraysMeshGenerator.FillVerts(skeleton, startSlot, endSlot, zSpacing, this.PremultiplyVertexColors, this.meshVertices, this.meshUVs, this.meshColors32, ref vertexIndex, ref this.attachmentVertexBuffer, ref meshBoundsMin, ref meshBoundsMax);
			}

			bool structureDoesntMatch = vertBufferResized || submeshBuffersResized || smartMesh.StructureDoesntMatch(workingAttachments, currentInstructions);
			for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++) {
				var currentInstruction = currentInstructionsItems[submeshIndex];
				if (structureDoesntMatch) {
					var currentBuffer = submeshBuffers.Items[submeshIndex];
					bool isLastSubmesh = (submeshIndex == submeshCount - 1);
					ArraysMeshGenerator.FillTriangles(ref currentBuffer.triangles, currentInstruction.skeleton, currentInstruction.triangleCount, currentInstruction.firstVertexIndex, currentInstruction.startSlot, currentInstruction.endSlot, isLastSubmesh);
					currentBuffer.triangleCount = currentInstruction.triangleCount;
					currentBuffer.firstVertex = currentInstruction.firstVertexIndex;
				}
			}

			if (structureDoesntMatch) {
				mesh.Clear();
				this.sharedMaterials = currentInstructions.GetUpdatedMaterialArray(this.sharedMaterials);
			}

			if (scale != 1f) {
				for (int i = 0; i < vertexCount; i++) {
					meshVertices[i].x *= scale;
					meshVertices[i].y *= scale;
					//meshVertices[i].z *= scale;
				}
					
			}

			// STEP 3: Assign the buffers into the Mesh.
			smartMesh.Set(this.meshVertices, this.meshUVs, this.meshColors32, workingAttachments, currentInstructions);
			mesh.bounds = ArraysMeshGenerator.ToBounds(meshBoundsMin, meshBoundsMax);


			if (structureDoesntMatch) {
				// Push new triangles if doesn't match.
				mesh.subMeshCount = submeshCount;
				for (int i = 0; i < submeshCount; i++)
					mesh.SetTriangles(submeshBuffers.Items[i].triangles, i);			

				this.TryAddNormalsTo(mesh, vertexCount);
			}

			if (addTangents) { 
				SolveTangents2DEnsureSize(ref this.meshTangents, ref this.tempTanBuffer, vertexCount);

				for (int i = 0, n = submeshCount; i < n; i++) {
					var submesh = submeshBuffers.Items[i];
					SolveTangents2DTriangles(this.tempTanBuffer, submesh.triangles, submesh.triangleCount, meshVertices, meshUVs, vertexCount);
				}
					
				SolveTangents2DBuffer(this.meshTangents, this.tempTanBuffer, vertexCount);
			}
				
			return new MeshAndMaterials(smartMesh.mesh, sharedMaterials);
		}

		#region Types
		// A SmartMesh is a Mesh (with submeshes) that knows what attachments and instructions were used to generate it.
		class SmartMesh {
			public readonly Mesh mesh = SpineMesh.NewMesh();
			readonly ExposedList<Attachment> attachmentsUsed = new ExposedList<Attachment>();
			readonly ExposedList<SubmeshInstruction> instructionsUsed = new ExposedList<SubmeshInstruction>();

			public void Set (Vector3[] verts, Vector2[] uvs, Color32[] colors, ExposedList<Attachment> attachments, ExposedList<SubmeshInstruction> instructions) {
				mesh.vertices = verts;
				mesh.uv = uvs;
				mesh.colors32 = colors;

				attachmentsUsed.Clear(false);
				attachmentsUsed.GrowIfNeeded(attachments.Capacity);
				attachmentsUsed.Count = attachments.Count;
				attachments.CopyTo(attachmentsUsed.Items);

				instructionsUsed.Clear(false);
				instructionsUsed.GrowIfNeeded(instructions.Capacity);
				instructionsUsed.Count = instructions.Count;
				instructions.CopyTo(instructionsUsed.Items);
			}

			public bool StructureDoesntMatch (ExposedList<Attachment> attachments, ExposedList<SubmeshInstruction> instructions) {
				// Check count inequality.
				if (attachments.Count != this.attachmentsUsed.Count) return true;
				if (instructions.Count != this.instructionsUsed.Count) return true;

				// Check each attachment.
				var attachmentsPassed = attachments.Items;
				var myAttachments = this.attachmentsUsed.Items;
				for (int i = 0, n = attachmentsUsed.Count; i < n; i++)
					if (attachmentsPassed[i] != myAttachments[i]) return true;

				// Check each submesh for equal arrangement.
				var instructionListItems = instructions.Items;
				var myInstructions = this.instructionsUsed.Items;
				for (int i = 0, n = this.instructionsUsed.Count; i < n; i++) {
					var lhs = instructionListItems[i];
					var rhs = myInstructions[i];
					if (
						lhs.material.GetInstanceID() != rhs.material.GetInstanceID() ||
						lhs.startSlot != rhs.startSlot ||
						lhs.endSlot != rhs.endSlot ||
						lhs.triangleCount != rhs.triangleCount ||
						lhs.vertexCount != rhs.vertexCount ||
						lhs.firstVertexIndex != rhs.firstVertexIndex
					) return true;
				}

				return false;
			}
		}
		#endregion
	}

}
