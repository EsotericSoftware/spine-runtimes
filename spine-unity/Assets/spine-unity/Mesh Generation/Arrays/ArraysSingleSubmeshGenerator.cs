using UnityEngine;
using System.Collections;

namespace Spine.Unity.MeshGeneration {
	public class ArraysSingleSubmeshGenerator : ISingleSubmeshGenerator {

		public float zSpacing = 0f;

		bool premultiplyVertexColors = true;
		public bool PremultiplyVertexColors { get { return this.premultiplyVertexColors; } set { this.premultiplyVertexColors = value; } }

		public Mesh GenerateMesh (SubmeshInstruction instruction) {
			float zSpacing = this.zSpacing;
			float[] attVertBuffer = this.attachmentVertexBuffer;
			Vector2[] uvs = this.meshUVs;
			Color32[] colors32 = this.meshColors32;
			var attachmentList = this.attachmentListBuffer;
			attachmentList.Clear();

			// Ensure correct buffer sizes.
			Vector3[] vertices = this.meshVertices;

			int instructionVertexCount = instruction.vertexCount;
			bool newVertices = vertices == null || instructionVertexCount > vertices.Length;
			if (newVertices) {
				this.meshVertices = vertices = new Vector3[instructionVertexCount];
				this.meshColors32 = colors32 = new Color32[instructionVertexCount];
				this.meshUVs = uvs = new Vector2[instructionVertexCount];
			} else {
				var zero = Vector3.zero;
				for (int i = instructionVertexCount, n = this.meshVertices.Length; i < n; i++)
					vertices[i] = zero;
			}
				
			Vector3 meshBoundsMin;
			Vector3 meshBoundsMax;

			int attachmentCount = instruction.endSlot - instruction.startSlot;

			// Initial values for manual Mesh Bounds calculation
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
				
			var skeleton = instruction.skeleton;
			int vertexIndex = 0;
			ArraysBuffers.Fill(skeleton, instruction.startSlot, instruction.endSlot, this.zSpacing, this.premultiplyVertexColors, vertices, uvs, colors32, ref vertexIndex, ref attVertBuffer, ref meshBoundsMin, ref meshBoundsMax);
			this.attachmentVertexBuffer = attVertBuffer;

			var smartMesh = this.doubleBufferedSmartMesh.GetNext();
			var mesh = smartMesh.mesh;

			bool structureDoesntMatch = newVertices || smartMesh.StructureDoesntMatch(attachmentList, instruction);

			// Push triangles in this submesh
			if (structureDoesntMatch) {
				mesh.Clear();

				int triangleCount = instruction.triangleCount;

				int[] thisTriangles = this.triangles;
				if (triangles == null || triangles.Length < triangleCount) {
					this.triangles = thisTriangles = new int[triangleCount];
				} else if (triangles.Length > triangleCount) {
					for (int i = triangleCount; i < triangles.Length; i++)
						thisTriangles[i] = 0;
				}

				// Iterate through submesh slots and store the triangles. 
				int triangleIndex = 0;
				int afv = 0; // attachment first vertex, for single submesh, don't use instructions.firstVertexIndex
				var skeletonDrawOrderItems = skeleton.drawOrder.Items;
				for (int i = instruction.startSlot, n = instruction.endSlot; i < n; i++) {			
					var attachment = skeletonDrawOrderItems[i].attachment;

					if (attachment is RegionAttachment) {
						thisTriangles[triangleIndex] = afv; thisTriangles[triangleIndex + 1] = afv + 2; thisTriangles[triangleIndex + 2] = afv + 1;
						thisTriangles[triangleIndex + 3] = afv + 2; thisTriangles[triangleIndex + 4] = afv + 3; thisTriangles[triangleIndex + 5] = afv + 1;

						triangleIndex += 6;
						afv += 4;
					} else {
						int[] attachmentTriangles;
						int attachmentVertexCount;
						var meshAttachment = attachment as MeshAttachment;
						if (meshAttachment != null) {
							attachmentVertexCount = meshAttachment.vertices.Length >> 1; // length/2
							attachmentTriangles = meshAttachment.triangles;
						} else {
							var weightedMeshAttachment = attachment as WeightedMeshAttachment;
							if (weightedMeshAttachment != null) {
								attachmentVertexCount = weightedMeshAttachment.uvs.Length >> 1; // length/2
								attachmentTriangles = weightedMeshAttachment.triangles;
							} else
								continue;
						}

						for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii++, triangleIndex++)
							thisTriangles[triangleIndex] = afv + attachmentTriangles[ii];

						afv += attachmentVertexCount;
					}
				} // Done adding current submesh triangles
			}
				
			Vector3 meshBoundsExtents = (meshBoundsMax - meshBoundsMin);
			Vector3 meshCenter = meshBoundsMin + meshBoundsExtents * 0.5f;

			smartMesh.Set(this.meshVertices, this.meshUVs, this.meshColors32, attachmentList, instruction);
			mesh.bounds = new Bounds(meshCenter, meshBoundsExtents);

			if (structureDoesntMatch) {				
				mesh.triangles = triangles;
			}

			return smartMesh.mesh;
		}

		readonly DoubleBuffered<ArraysSingleSubmeshGenerator.SmartMesh> doubleBufferedSmartMesh = new DoubleBuffered<SmartMesh>();
		readonly ExposedList<Attachment> attachmentListBuffer = new ExposedList<Attachment>();

		float[] attachmentVertexBuffer = new float[8];
		Vector3[] meshVertices;
		Color32[] meshColors32;
		Vector2[] meshUVs;
		int[] triangles;

		class SmartMesh {
			public readonly Mesh mesh = SpineMesh.NewMesh();
			SubmeshInstruction instructionsUsed;
			readonly ExposedList<Attachment> attachmentsUsed = new ExposedList<Attachment>();

			public void Set (Vector3[] verts, Vector2[] uvs, Color32[] colors, ExposedList<Attachment> attachmentList, SubmeshInstruction instructions) {
				mesh.vertices = verts;
				mesh.uv = uvs;
				mesh.colors32 = colors;
				instructionsUsed = instructions;

				attachmentsUsed.Clear();
				attachmentsUsed.GrowIfNeeded(attachmentList.Capacity);
				attachmentsUsed.Count = attachmentList.Count;
				attachmentList.CopyTo(attachmentsUsed.Items);
			}

			public bool StructureDoesntMatch (ExposedList<Attachment> attachmentList, SubmeshInstruction instructions) {
				// Check each submesh instructions for equal arrangement.
				var thisInstructions = instructionsUsed;
				if (
					instructions.skeleton != thisInstructions.skeleton ||
					instructions.material.GetInstanceID() != thisInstructions.material.GetInstanceID() ||
					instructions.startSlot != thisInstructions.startSlot ||
					instructions.endSlot != thisInstructions.endSlot ||
					instructions.triangleCount != thisInstructions.triangleCount ||
					instructions.vertexCount != thisInstructions.vertexCount
				) return true;
				//Debug.Log("structure matched");

				// Check count inequality.
				if (attachmentList.Count != this.attachmentsUsed.Count) return true;
				var attachmentsPassed = attachmentList.Items;
				var myAttachments = this.attachmentsUsed.Items;
				for (int i = 0, n = attachmentsUsed.Count; i < n; i++)
					if (attachmentsPassed[i] != myAttachments[i]) return true;

				//Debug.Log("attachments matched");

				return false;
			}
		}
	}
}
