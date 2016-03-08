using UnityEngine;
using System.Collections;

namespace Spine.Unity {
	public class ArraysSingleSubmeshGenerator : ISingleSubmeshGenerator {

		public float zSpacing = 0f;

		bool premultiplyVertexColors = true;
		public bool PremultiplyVertexColors { get { return this.premultiplyVertexColors; } set { this.premultiplyVertexColors = value; } }

		public Mesh GenerateMesh (SubmeshInstruction instruction) {
			float zSpacing = this.zSpacing;
			float[] attVertBuffer = this.attachmentVertexBuffer;
			Vector2[] uvs = this.meshUVs;
			Color32[] colors32 = this.meshColors32;
			Color32 color;
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

			int vertexIndex = 0;
			var skeleton = instruction.skeleton;
			var skeletonDrawOrderItems = skeleton.DrawOrder.Items;
			float a = skeleton.a * 255, r = skeleton.r, g = skeleton.g, b = skeleton.b;

			// Push verts in this submesh
			for (int slotIndex = instruction.startSlot, endSlot = instruction.endSlot; slotIndex < endSlot; slotIndex++) {
				var slot = skeletonDrawOrderItems[slotIndex];
				var attachment = slot.attachment;
				float z = slotIndex * zSpacing;

				var regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {
					attachmentList.Add(attachment);
					regionAttachment.ComputeWorldVertices(slot.bone, attVertBuffer);

					float x1 = attVertBuffer[RegionAttachment.X1], y1 = attVertBuffer[RegionAttachment.Y1];
					float x2 = attVertBuffer[RegionAttachment.X2], y2 = attVertBuffer[RegionAttachment.Y2];
					float x3 = attVertBuffer[RegionAttachment.X3], y3 = attVertBuffer[RegionAttachment.Y3];
					float x4 = attVertBuffer[RegionAttachment.X4], y4 = attVertBuffer[RegionAttachment.Y4];
					vertices[vertexIndex].x = x1; vertices[vertexIndex].y = y1; vertices[vertexIndex].z = z;
					vertices[vertexIndex + 1].x = x4; vertices[vertexIndex + 1].y = y4; vertices[vertexIndex + 1].z = z;
					vertices[vertexIndex + 2].x = x2; vertices[vertexIndex + 2].y = y2; vertices[vertexIndex + 2].z = z;
					vertices[vertexIndex + 3].x = x3; vertices[vertexIndex + 3].y = y3;	vertices[vertexIndex + 3].z = z;

					color.a = (byte)(a * slot.a * regionAttachment.a);
					if (this.premultiplyVertexColors) {
						color.r = (byte)(r * slot.r * regionAttachment.r * color.a); color.g = (byte)(g * slot.g * regionAttachment.g * color.a); color.b = (byte)(b * slot.b * regionAttachment.b * color.a);
					} else {
						color.r = (byte)(r * slot.r * regionAttachment.r); color.g = (byte)(g * slot.g * regionAttachment.g); color.b = (byte)(b * slot.b * regionAttachment.b);
					}
					if (slot.data.blendMode == BlendMode.additive) color.a = 0;
					colors32[vertexIndex] = color; colors32[vertexIndex + 1] = color; colors32[vertexIndex + 2] = color; colors32[vertexIndex + 3] = color;

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
					var meshAttachment = attachment as MeshAttachment;
					if (meshAttachment != null) {
						attachmentList.Add(attachment);
						int meshVertexCount = meshAttachment.vertices.Length;
						if (attVertBuffer.Length < meshVertexCount) this.attachmentVertexBuffer = attVertBuffer = new float[meshVertexCount];
						meshAttachment.ComputeWorldVertices(slot, attVertBuffer);

						color.a = (byte)(a * slot.a * meshAttachment.a);
						if (this.premultiplyVertexColors) {
							color.r = (byte)(r * slot.r * meshAttachment.r * color.a);
							color.g = (byte)(g * slot.g * meshAttachment.g * color.a);
							color.b = (byte)(b * slot.b * meshAttachment.b * color.a);
						} else {
							color.r = (byte)(r * slot.r * meshAttachment.r);
							color.g = (byte)(g * slot.g * meshAttachment.g);
							color.b = (byte)(b * slot.b * meshAttachment.b);
						}
						if (slot.data.blendMode == BlendMode.additive) color.a = 0;

						float[] attachmentUVs = meshAttachment.uvs;
						for (int iii = 0; iii < meshVertexCount; iii += 2) {
							float x = attVertBuffer[iii], y = attVertBuffer[iii + 1];
							vertices[vertexIndex].x = x; vertices[vertexIndex].y = y; vertices[vertexIndex].z = z;
							colors32[vertexIndex] = color; uvs[vertexIndex].x = attachmentUVs[iii]; uvs[vertexIndex].y = attachmentUVs[iii + 1];

							if (x < meshBoundsMin.x) meshBoundsMin.x = x;
							else if (x > meshBoundsMax.x) meshBoundsMax.x = x;

							if (y < meshBoundsMin.y) meshBoundsMin.y = y;
							else if (y > meshBoundsMax.y) meshBoundsMax.y = y;

							vertexIndex++;
						}
					} else {
						var weightedMeshAttachment = attachment as WeightedMeshAttachment;
						if (weightedMeshAttachment != null) {
							attachmentList.Add(attachment);
							int meshVertexCount = weightedMeshAttachment.uvs.Length;
							if (attVertBuffer.Length < meshVertexCount) this.attachmentVertexBuffer = attVertBuffer = new float[meshVertexCount];
							weightedMeshAttachment.ComputeWorldVertices(slot, attVertBuffer);

							color.a = (byte)(a * slot.a * weightedMeshAttachment.a);
							if (this.premultiplyVertexColors) {
								color.r = (byte)(r * slot.r * weightedMeshAttachment.r * color.a);
								color.g = (byte)(g * slot.g * weightedMeshAttachment.g * color.a);
								color.b = (byte)(b * slot.b * weightedMeshAttachment.b * color.a);
							} else {
								color.r = (byte)(r * slot.r * weightedMeshAttachment.r);
								color.g = (byte)(g * slot.g * weightedMeshAttachment.g);
								color.b = (byte)(b * slot.b * weightedMeshAttachment.b);
							}
							if (slot.data.blendMode == BlendMode.additive) color.a = 0;

							float[] attachmentUVs = weightedMeshAttachment.uvs;
							for (int iii = 0; iii < meshVertexCount; iii += 2) {
								float x = attVertBuffer[iii], y = attVertBuffer[iii + 1];
								vertices[vertexIndex].x = x; vertices[vertexIndex].y = y; vertices[vertexIndex].z = z;
								colors32[vertexIndex] = color;
								uvs[vertexIndex].x = attachmentUVs[iii]; uvs[vertexIndex].y = attachmentUVs[iii + 1];

								if (x < meshBoundsMin.x) meshBoundsMin.x = x;
								else if (x > meshBoundsMax.x) meshBoundsMax.x = x;
								if (y < meshBoundsMin.y) meshBoundsMin.y = y;
								else if (y > meshBoundsMax.y) meshBoundsMax.y = y;

								vertexIndex++;
							}
						}
					}
				}
			}

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
