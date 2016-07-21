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
using System.Collections.Generic;

namespace Spine.Unity.MeshGeneration {
	/// <summary>
	/// Arrays submeshed mesh generator.
	/// </summary>
	public class ArraysSubmeshedMeshGenerator : ArraysMeshGenerator, ISubmeshedMeshGenerator {

		readonly List<Slot> separators = new List<Slot>();
		public List<Slot> Separators { get { return this.separators; } }

		#region Settings
		public float ZSpacing { get; set; }
		#endregion

		readonly DoubleBuffered<SmartMesh> doubleBufferedSmartMesh = new DoubleBuffered<SmartMesh>();
		readonly SubmeshedMeshInstruction currentInstructions = new SubmeshedMeshInstruction();
		readonly ExposedList<SubmeshTriangleBuffer> submeshBuffers = new ExposedList<SubmeshTriangleBuffer>();
		Material[] sharedMaterials = new Material[0];

		public SubmeshedMeshInstruction GenerateInstruction (Skeleton skeleton) {
			if (skeleton == null) throw new System.ArgumentNullException("skeleton");

			// Count vertices and submesh triangles.
			int runningVertexCount = 0;

			int submeshTriangleCount = 0;
			int submeshFirstVertex = 0;
			int submeshVertexCount = 0;
			int submeshStartSlotIndex = 0;
			Material lastMaterial = null;

			var drawOrder = skeleton.drawOrder;
			var drawOrderItems = drawOrder.Items;
			int drawOrderCount = drawOrder.Count;
			int separatorCount = separators.Count;

			var instructionList = this.currentInstructions.submeshInstructions;
			instructionList.Clear(false);

			currentInstructions.attachmentList.Clear(false);

			for (int i = 0; i < drawOrderCount; i++) {
				var slot = drawOrderItems[i];
				var attachment = slot.attachment;

				object rendererObject; // An AtlasRegion in plain Spine-Unity. eventual source of Material object.
				int attachmentVertexCount, attachmentTriangleCount;

				var regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {
					rendererObject = regionAttachment.RendererObject;
					attachmentVertexCount = 4;
					attachmentTriangleCount = 6;
				} else {
					var meshAttachment = attachment as MeshAttachment;
					if (meshAttachment != null) {
						rendererObject = meshAttachment.RendererObject;
						attachmentVertexCount = meshAttachment.worldVerticesLength >> 1;
						attachmentTriangleCount = meshAttachment.triangles.Length;
					} else {
						continue;
					}
				}

				var attachmentMaterial = (Material)((AtlasRegion)rendererObject).page.rendererObject;

				// Populate submesh when material changes. (or when forced to separate by a submeshSeparator)
				bool separatedBySlot  = ( separatorCount > 0 && separators.Contains(slot) );
				if (( runningVertexCount > 0 && lastMaterial.GetInstanceID() != attachmentMaterial.GetInstanceID() ) ||	separatedBySlot) {

					instructionList.Add(
						new SubmeshInstruction {
							skeleton = skeleton,
							material = lastMaterial,
							triangleCount = submeshTriangleCount,
							vertexCount = submeshVertexCount,
							startSlot = submeshStartSlotIndex,
							endSlot = i,
							firstVertexIndex = submeshFirstVertex,
							forceSeparate = separatedBySlot
						}
					);

					// Prepare for next submesh
					submeshTriangleCount = 0;
					submeshVertexCount = 0;
					submeshFirstVertex = runningVertexCount;
					submeshStartSlotIndex = i;
				}
				lastMaterial = attachmentMaterial;

				submeshTriangleCount += attachmentTriangleCount;
				submeshVertexCount += attachmentVertexCount;
				runningVertexCount += attachmentVertexCount;

				currentInstructions.attachmentList.Add(attachment);
			}

			instructionList.Add(
				new SubmeshInstruction {
					skeleton = skeleton,
					material = lastMaterial,
					triangleCount = submeshTriangleCount,
					vertexCount = submeshVertexCount,
					startSlot = submeshStartSlotIndex,
					endSlot = drawOrderCount,
					firstVertexIndex = submeshFirstVertex,
					forceSeparate = false
				}
			);

			currentInstructions.vertexCount = runningVertexCount;
			return currentInstructions;
		}

		// ISubmeshedMeshGenerator.GenerateMesh
		/// <summary>Generates a mesh based on SubmeshedMeshInstructions</summary>
		public MeshAndMaterials GenerateMesh (SubmeshedMeshInstruction meshInstructions) {
			var smartMesh = doubleBufferedSmartMesh.GetNext();
			var mesh = smartMesh.mesh;
			int submeshCount = meshInstructions.submeshInstructions.Count;
			var instructionList = meshInstructions.submeshInstructions;

			// STEP 1: Ensure correct buffer sizes.
			int vertexCount = meshInstructions.vertexCount;
			bool submeshBuffersResized = ArraysMeshGenerator.EnsureTriangleBuffersSize(submeshBuffers, submeshCount, instructionList.Items);
			bool vertBufferResized = ArraysMeshGenerator.EnsureSize(vertexCount, ref this.meshVertices, ref this.meshUVs, ref this.meshColors32);
			Vector3[] vertices = this.meshVertices;

			// STEP 2: Update buffers based on Skeleton.
			float zSpacing = this.ZSpacing;
			Vector3 meshBoundsMin;
			Vector3 meshBoundsMax;
			int attachmentCount = meshInstructions.attachmentList.Count;
			if (attachmentCount <= 0) {
				meshBoundsMin = new Vector3(0, 0, 0);
				meshBoundsMax = new Vector3(0, 0, 0);
			} else {
				meshBoundsMin.x = int.MaxValue;
				meshBoundsMin.y = int.MaxValue;
				meshBoundsMax.x = int.MinValue;
				meshBoundsMax.y = int.MinValue;

				if (zSpacing > 0f) {
					meshBoundsMin.z = 0f;
					meshBoundsMax.z = zSpacing * (attachmentCount - 1);
				} else {
					meshBoundsMin.z = zSpacing * (attachmentCount - 1);
					meshBoundsMax.z = 0f;
				}
			}
			bool structureDoesntMatch = vertBufferResized || submeshBuffersResized || smartMesh.StructureDoesntMatch(meshInstructions);
			// For each submesh, add vertex data from attachments. Also triangles, but only if needed.
			int vertexIndex = 0; // modified by FillVerts
			for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++) {
				var submeshInstruction = instructionList.Items[submeshIndex];
				int start = submeshInstruction.startSlot;
				int end = submeshInstruction.endSlot;
				var skeleton = submeshInstruction.skeleton;
				ArraysMeshGenerator.FillVerts(skeleton, start, end, zSpacing, this.PremultiplyVertexColors, vertices, this.meshUVs, this.meshColors32, ref vertexIndex, ref this.attachmentVertexBuffer, ref meshBoundsMin, ref meshBoundsMax);
				if (structureDoesntMatch) {
					var currentBuffer = submeshBuffers.Items[submeshIndex];
					bool isLastSubmesh = (submeshIndex == submeshCount - 1);
					ArraysMeshGenerator.FillTriangles(ref currentBuffer.triangles, skeleton, submeshInstruction.triangleCount, submeshInstruction.firstVertexIndex, start, end, isLastSubmesh);
					currentBuffer.triangleCount = submeshInstruction.triangleCount;
					currentBuffer.firstVertex = submeshInstruction.firstVertexIndex;
				}
			}

			if (structureDoesntMatch) {
				mesh.Clear();
				this.sharedMaterials = meshInstructions.GetUpdatedMaterialArray(this.sharedMaterials);
			}

			// STEP 3: Assign the buffers into the Mesh.
			smartMesh.Set(this.meshVertices, this.meshUVs, this.meshColors32, meshInstructions);
			mesh.bounds = ArraysMeshGenerator.ToBounds(meshBoundsMin, meshBoundsMax);

			if (structureDoesntMatch) {
				// Push new triangles if doesn't match.
				mesh.subMeshCount = submeshCount;
				for (int i = 0; i < submeshCount; i++)
					mesh.SetTriangles(submeshBuffers.Items[i].triangles, i);			

				TryAddNormalsTo(mesh, vertexCount);
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

			public void Set (Vector3[] verts, Vector2[] uvs, Color32[] colors, SubmeshedMeshInstruction instruction) {
				mesh.vertices = verts;
				mesh.uv = uvs;
				mesh.colors32 = colors;

				attachmentsUsed.Clear(false);
				attachmentsUsed.GrowIfNeeded(instruction.attachmentList.Capacity);
				attachmentsUsed.Count = instruction.attachmentList.Count;
				instruction.attachmentList.CopyTo(attachmentsUsed.Items);

				instructionsUsed.Clear(false);
				instructionsUsed.GrowIfNeeded(instruction.submeshInstructions.Capacity);
				instructionsUsed.Count = instruction.submeshInstructions.Count;
				instruction.submeshInstructions.CopyTo(instructionsUsed.Items);
			}

			public bool StructureDoesntMatch (SubmeshedMeshInstruction instructions) {
				// Check count inequality.
				if (instructions.attachmentList.Count != this.attachmentsUsed.Count) return true;
				if (instructions.submeshInstructions.Count != this.instructionsUsed.Count) return true;

				// Check each attachment.
				var attachmentsPassed = instructions.attachmentList.Items;
				var myAttachments = this.attachmentsUsed.Items;
				for (int i = 0, n = attachmentsUsed.Count; i < n; i++)
					if (attachmentsPassed[i] != myAttachments[i]) return true;

				// Check each submesh for equal arrangement.
				var instructionListItems = instructions.submeshInstructions.Items;
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

				//Debug.Log("structure matched");
				return false;
			}
		}
		#endregion
	}

}