using UnityEngine;
using System.Collections;
using Spine;

namespace Spine.Unity.MeshGeneration {
	public class ArraysSubmeshSetMeshGenerator : ISubmeshSetMeshGenerator {
		public float zSpacing = 0f;
		public bool premultiplyVertexColors = true;

		public MeshAndMaterials GenerateMesh (ExposedList<SubmeshInstruction> instructions, int startSubmesh, int endSubmesh) {
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
				// Ensure current instructions have correct cached values.
				currentInstructionsItems[i].firstVertexIndex = vertexCount;

				// vertexCount will also be used for the rest of this method.
				vertexCount += currentInstructionsItems[i].vertexCount;
			}
				
			float[] attVertBuffer = this.attachmentVertexBuffer;
			Vector2[] uvs = this.meshUVs;
			Color32[] colors32 = this.meshColors32;

			// Ensure correct buffer sizes.
			Vector3[] vertices = this.meshVertices;

			bool newVertices = vertices == null || vertexCount > vertices.Length;
			if (newVertices) {
				this.meshVertices = vertices = new Vector3[vertexCount];
				this.meshColors32 = colors32 = new Color32[vertexCount];
				this.meshUVs = uvs = new Vector2[vertexCount];
			} else {
				var zero = Vector3.zero;
				for (int i = vertexCount, n = this.meshVertices.Length; i < n; i++)
					vertices[i] = zero;
			}

			bool newSubmeshBuffers = submeshBuffers.Count < submeshCount;
			if (newSubmeshBuffers) {
				submeshBuffers.GrowIfNeeded(submeshCount);
				for (int i = submeshBuffers.Count; submeshBuffers.Count < submeshCount; i++) {
					submeshBuffers.Add(new SubmeshTriangleBuffer(currentInstructionsItems[i].triangleCount));
				}
			}

			Vector3 meshBoundsMin;
			Vector3 meshBoundsMax;

			float zSpacing = this.zSpacing;
			// Initial values for manual Mesh Bounds calculation
			if (vertexCount <= 0) {
				meshBoundsMin = new Vector3(0, 0, 0);
				meshBoundsMax = new Vector3(0, 0, 0);
			} else {
				meshBoundsMin.x = int.MaxValue;
				meshBoundsMin.y = int.MaxValue;
				meshBoundsMax.x = int.MinValue;
				meshBoundsMax.y = int.MinValue;

				if (zSpacing > 0f) {
					meshBoundsMin.z = 0f;
					meshBoundsMax.z = zSpacing * (currentInstructionsItems[submeshCount - 1].endSlot);
				} else {
					meshBoundsMin.z = zSpacing * (currentInstructionsItems[submeshCount - 1].endSlot);
					meshBoundsMax.z = 0f;
				}
			}

			bool structureDoesntMatch = newVertices || newSubmeshBuffers || smartMesh.StructureDoesntMatch(currentAttachments, currentInstructions);

			if (structureDoesntMatch) {
				mesh.Clear();

				if (submeshCount == sharedMaterials.Length)
					currentInstructions.FillMaterialArray(this.sharedMaterials);
				else
					this.sharedMaterials = currentInstructions.GetNewMaterialArray();
			}

			currentAttachments.Clear(false);
			int vertexIndex = 0;
			// For each submesh, add vertex data from attachments.
			for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++) {
				var currentSubmeshInstruction = currentInstructionsItems[submeshIndex];
				var skeleton = currentSubmeshInstruction.skeleton;

//				for (int slotIndex = currentSubmeshInstruction.startSlot, endSlot = currentSubmeshInstruction.endSlot; slotIndex < endSlot; slotIndex++) {
//					var slot = skeletonDrawOrderItems[slotIndex];
//					var attachment = slot.attachment;
//					float z = slotIndex * zSpacing;
//
//					var regionAttachment = attachment as RegionAttachment;
//					if (regionAttachment != null) {
//						regionAttachment.ComputeWorldVertices(slot.bone, attVertBuffer);
//
//						float x1 = attVertBuffer[RegionAttachment.X1], y1 = attVertBuffer[RegionAttachment.Y1];
//						float x2 = attVertBuffer[RegionAttachment.X2], y2 = attVertBuffer[RegionAttachment.Y2];
//						float x3 = attVertBuffer[RegionAttachment.X3], y3 = attVertBuffer[RegionAttachment.Y3];
//						float x4 = attVertBuffer[RegionAttachment.X4], y4 = attVertBuffer[RegionAttachment.Y4];
//						vertices[vertexIndex].x = x1; vertices[vertexIndex].y = y1; vertices[vertexIndex].z = z;
//						vertices[vertexIndex + 1].x = x4; vertices[vertexIndex + 1].y = y4; vertices[vertexIndex + 1].z = z;
//						vertices[vertexIndex + 2].x = x2; vertices[vertexIndex + 2].y = y2; vertices[vertexIndex + 2].z = z;
//						vertices[vertexIndex + 3].x = x3; vertices[vertexIndex + 3].y = y3;	vertices[vertexIndex + 3].z = z;
//
//						if (premultiplyVertexColors) {
//							color.a = (byte)(a * slot.a * regionAttachment.a);
//							color.r = (byte)(r * slot.r * regionAttachment.r * color.a);
//							color.g = (byte)(g * slot.g * regionAttachment.g * color.a);
//							color.b = (byte)(b * slot.b * regionAttachment.b * color.a);
//							if (slot.data.blendMode == BlendMode.additive) color.a = 0;
//						} else {
//							color.a = (byte)(a * slot.a * regionAttachment.a);
//							color.r = (byte)(r * slot.r * regionAttachment.r * 255);
//							color.g = (byte)(g * slot.g * regionAttachment.g * 255);
//							color.b = (byte)(b * slot.b * regionAttachment.b * 255);
//						}
//
//						colors32[vertexIndex] = color; colors32[vertexIndex + 1] = color; colors32[vertexIndex + 2] = color; colors32[vertexIndex + 3] = color;
//
//						float[] regionUVs = regionAttachment.uvs;
//						uvs[vertexIndex].x = regionUVs[RegionAttachment.X1]; uvs[vertexIndex].y = regionUVs[RegionAttachment.Y1];
//						uvs[vertexIndex + 1].x = regionUVs[RegionAttachment.X4]; uvs[vertexIndex + 1].y = regionUVs[RegionAttachment.Y4];
//						uvs[vertexIndex + 2].x = regionUVs[RegionAttachment.X2]; uvs[vertexIndex + 2].y = regionUVs[RegionAttachment.Y2];
//						uvs[vertexIndex + 3].x = regionUVs[RegionAttachment.X3]; uvs[vertexIndex + 3].y = regionUVs[RegionAttachment.Y3];
//
//						// Calculate min/max X
//						if (x1 < meshBoundsMin.x) meshBoundsMin.x = x1;
//						else if (x1 > meshBoundsMax.x) meshBoundsMax.x = x1;
//						if (x2 < meshBoundsMin.x) meshBoundsMin.x = x2;
//						else if (x2 > meshBoundsMax.x) meshBoundsMax.x = x2;
//						if (x3 < meshBoundsMin.x) meshBoundsMin.x = x3;
//						else if (x3 > meshBoundsMax.x) meshBoundsMax.x = x3;
//						if (x4 < meshBoundsMin.x) meshBoundsMin.x = x4;
//						else if (x4 > meshBoundsMax.x) meshBoundsMax.x = x4;
//
//						// Calculate min/max Y
//						if (y1 < meshBoundsMin.y) meshBoundsMin.y = y1;
//						else if (y1 > meshBoundsMax.y) meshBoundsMax.y = y1;
//						if (y2 < meshBoundsMin.y) meshBoundsMin.y = y2;
//						else if (y2 > meshBoundsMax.y) meshBoundsMax.y = y2;
//						if (y3 < meshBoundsMin.y) meshBoundsMin.y = y3;
//						else if (y3 > meshBoundsMax.y) meshBoundsMax.y = y3;
//						if (y4 < meshBoundsMin.y) meshBoundsMin.y = y4;
//						else if (y4 > meshBoundsMax.y) meshBoundsMax.y = y4;
//
//						currentAttachments.Add(regionAttachment);
//						vertexIndex += 4;
//					} else {
//						var meshAttachment = attachment as MeshAttachment;
//						if (meshAttachment != null) {
//							int meshVertexCount = meshAttachment.vertices.Length;
//							if (attVertBuffer.Length < meshVertexCount) this.attachmentVertexBuffer = attVertBuffer = new float[meshVertexCount];
//							meshAttachment.ComputeWorldVertices(slot, attVertBuffer);
//
//							if (premultiplyVertexColors) {
//								color.a = (byte)(a * slot.a * meshAttachment.a);
//								color.r = (byte)(r * slot.r * meshAttachment.r * color.a);
//								color.g = (byte)(g * slot.g * meshAttachment.g * color.a);
//								color.b = (byte)(b * slot.b * meshAttachment.b * color.a);
//								if (slot.data.blendMode == BlendMode.additive) color.a = 0;
//							} else {
//								color.a = (byte)(a * slot.a * meshAttachment.a);
//								color.r = (byte)(r * slot.r * meshAttachment.r * 255);
//								color.g = (byte)(g * slot.g * meshAttachment.g * 255);
//								color.b = (byte)(b * slot.b * meshAttachment.b * 255);
//							}
//
//							float[] attachmentUVs = meshAttachment.uvs;
//							for (int iii = 0; iii < meshVertexCount; iii += 2) {
//								float x = attVertBuffer[iii], y = attVertBuffer[iii + 1];
//								vertices[vertexIndex].x = x; vertices[vertexIndex].y = y; vertices[vertexIndex].z = z;
//								colors32[vertexIndex] = color; uvs[vertexIndex].x = attachmentUVs[iii]; uvs[vertexIndex].y = attachmentUVs[iii + 1];
//
//								if (x < meshBoundsMin.x) meshBoundsMin.x = x;
//								else if (x > meshBoundsMax.x) meshBoundsMax.x = x;
//
//								if (y < meshBoundsMin.y) meshBoundsMin.y = y;
//								else if (y > meshBoundsMax.y) meshBoundsMax.y = y;
//
//								currentAttachments.Add(meshAttachment);
//								vertexIndex++;
//							}
//						} else {
//							var weightedMeshAttachment = attachment as WeightedMeshAttachment;
//							if (weightedMeshAttachment != null) {
//								int meshVertexCount = weightedMeshAttachment.uvs.Length;
//								if (attVertBuffer.Length < meshVertexCount) this.attachmentVertexBuffer = attVertBuffer = new float[meshVertexCount];
//								weightedMeshAttachment.ComputeWorldVertices(slot, attVertBuffer);
//
//								if (premultiplyVertexColors) {
//									color.a = (byte)(a * slot.a * weightedMeshAttachment.a);
//									color.r = (byte)(r * slot.r * weightedMeshAttachment.r * color.a);
//									color.g = (byte)(g * slot.g * weightedMeshAttachment.g * color.a);
//									color.b = (byte)(b * slot.b * weightedMeshAttachment.b * color.a);
//									if (slot.data.blendMode == BlendMode.additive) color.a = 0;
//								} else {
//									color.a = (byte)(a * slot.a * weightedMeshAttachment.a);
//									color.r = (byte)(r * slot.r * weightedMeshAttachment.r * 255);
//									color.g = (byte)(g * slot.g * weightedMeshAttachment.g * 255);
//									color.b = (byte)(b * slot.b * weightedMeshAttachment.b * 255);
//								}
//
//								float[] attachmentUVs = weightedMeshAttachment.uvs;
//								for (int iii = 0; iii < meshVertexCount; iii += 2) {
//									float x = attVertBuffer[iii], y = attVertBuffer[iii + 1];
//									vertices[vertexIndex].x = x; vertices[vertexIndex].y = y; vertices[vertexIndex].z = z;
//									colors32[vertexIndex] = color;
//									uvs[vertexIndex].x = attachmentUVs[iii]; uvs[vertexIndex].y = attachmentUVs[iii + 1];
//
//									if (x < meshBoundsMin.x) meshBoundsMin.x = x;
//									else if (x > meshBoundsMax.x) meshBoundsMax.x = x;
//									if (y < meshBoundsMin.y) meshBoundsMin.y = y;
//									else if (y > meshBoundsMax.y) meshBoundsMax.y = y;
//
//									currentAttachments.Add(weightedMeshAttachment);
//									vertexIndex++;
//								}
//							}
//						}
//					}
//				}
				var skeletonDrawOrderItems = skeleton.DrawOrder.Items;
				int startSlot = currentSubmeshInstruction.startSlot;
				int endSlot = currentSubmeshInstruction.endSlot;

				for (int i = startSlot, n = currentSubmeshInstruction.endSlot; i < n; i++) {
					var ca = skeletonDrawOrderItems[i].attachment;
					if (ca != null) {
						// Includes BoundingBoxes. This is ok.
						currentAttachments.Add(ca);
					}
				}

				ArraysBuffers.Fill(skeleton, startSlot, endSlot, zSpacing, this.premultiplyVertexColors, vertices, uvs, colors32, ref vertexIndex, ref attVertBuffer, ref meshBoundsMin, ref meshBoundsMax);

				// Push triangles in this submesh
				if (structureDoesntMatch) {
					smartMesh.mesh.Clear(); // rebuild triangle array.

					var currentSubmesh = submeshBuffers.Items[submeshIndex];
					bool isLastSubmesh = (submeshIndex == submeshCount - 1);

					int triangleCount = currentSubmesh.triangleCount = currentSubmeshInstruction.triangleCount;
					int trianglesCapacity = currentSubmesh.triangles.Length;

					int[] triangles = currentSubmesh.triangles;
					if (isLastSubmesh) {
						if (trianglesCapacity > triangleCount) {
							for (int i = triangleCount; i < trianglesCapacity; i++)
								triangles[i] = 0;
						}
					} else if (trianglesCapacity != triangleCount) {
						triangles = currentSubmesh.triangles = new int[triangleCount];
						currentSubmesh.triangleCount = 0;					 
					}

					// Iterate through submesh slots and store the triangles. 
					int triangleIndex = 0;
					int afv = currentSubmeshInstruction.firstVertexIndex; // attachment first vertex

					for (int i = currentSubmeshInstruction.startSlot, n = currentSubmeshInstruction.endSlot; i < n; i++) {			
						var attachment = skeletonDrawOrderItems[i].attachment;

						if (attachment is RegionAttachment) {
							triangles[triangleIndex] = afv; triangles[triangleIndex + 1] = afv + 2; triangles[triangleIndex + 2] = afv + 1;
							triangles[triangleIndex + 3] = afv + 2; triangles[triangleIndex + 4] = afv + 3; triangles[triangleIndex + 5] = afv + 1;

							triangleIndex += 6;
							afv += 4;
						} else {
							int[] attachmentTriangles;
							int attachmentVertexCount;
							var meshAttachment = attachment as MeshAttachment;
							if (meshAttachment != null) {
								attachmentVertexCount = meshAttachment.vertices.Length >> 1; //  length/2
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
								triangles[triangleIndex] = afv + attachmentTriangles[ii];

							afv += attachmentVertexCount;
						}
					} // Done adding current submesh triangles
				}
			}

			this.attachmentVertexBuffer = attVertBuffer;
			Vector3 meshBoundsExtents = (meshBoundsMax - meshBoundsMin);
			Vector3 meshCenter = meshBoundsMin + meshBoundsExtents * 0.5f;

			smartMesh.Set(this.meshVertices, this.meshUVs, this.meshColors32, currentAttachments, currentInstructions);
			mesh.bounds = new Bounds(meshCenter, meshBoundsExtents);

			if (structureDoesntMatch) {
				// push new triangles if doesn't match.
				mesh.subMeshCount = submeshCount;
				for (int i = 0; i < submeshCount; i++)
					mesh.SetTriangles(submeshBuffers.Items[i].triangles, i);			
			}
				
			return new MeshAndMaterials(smartMesh.mesh, sharedMaterials);
		}

		#region Internals
		readonly DoubleBuffered<SmartMesh> doubleBufferedSmartMesh = new DoubleBuffered<SmartMesh>();
		readonly ExposedList<SubmeshInstruction> currentInstructions = new ExposedList<SubmeshInstruction>();
		readonly ExposedList<Attachment> currentAttachments = new ExposedList<Attachment>();

		float[] attachmentVertexBuffer = new float[8];
		Vector3[] meshVertices;
		Color32[] meshColors32;
		Vector2[] meshUVs;
		Material[] sharedMaterials = new Material[0];
		readonly ExposedList<SubmeshTriangleBuffer> submeshBuffers = new ExposedList<SubmeshTriangleBuffer>();
		#endregion

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

				//Debug.Log("structure matched");
				return false;
			}
		}
		#endregion
	}

}
