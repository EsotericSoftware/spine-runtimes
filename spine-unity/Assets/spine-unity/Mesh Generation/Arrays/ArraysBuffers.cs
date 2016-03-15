using UnityEngine;
using System.Collections;

namespace Spine.Unity.MeshGeneration {
	public static class ArraysBuffers {
		
		public static void Fill (Skeleton skeleton, int startSlot, int endSlot, float zSpacing, bool pmaColors, Vector3[] vertices, Vector2[] uvs, Color32[] colors32, ref int vertexIndex, ref float[] attVertBuffer, ref Vector3 meshBoundsMin, ref Vector3 meshBoundsMax) {
			Color32 color;
			var skeletonDrawOrderItems = skeleton.DrawOrder.Items;
			float a = skeleton.a * 255, r = skeleton.r, g = skeleton.g, b = skeleton.b;

			// drawOrder[endSlot] is excluded
			for (int slotIndex = startSlot; slotIndex < endSlot; slotIndex++) {
				var slot = skeletonDrawOrderItems[slotIndex];
				var attachment = slot.attachment;
				float z = slotIndex * zSpacing;

				var regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {
					regionAttachment.ComputeWorldVertices(slot.bone, attVertBuffer);

					float x1 = attVertBuffer[RegionAttachment.X1], y1 = attVertBuffer[RegionAttachment.Y1];
					float x2 = attVertBuffer[RegionAttachment.X2], y2 = attVertBuffer[RegionAttachment.Y2];
					float x3 = attVertBuffer[RegionAttachment.X3], y3 = attVertBuffer[RegionAttachment.Y3];
					float x4 = attVertBuffer[RegionAttachment.X4], y4 = attVertBuffer[RegionAttachment.Y4];
					vertices[vertexIndex].x = x1; vertices[vertexIndex].y = y1; vertices[vertexIndex].z = z;
					vertices[vertexIndex + 1].x = x4; vertices[vertexIndex + 1].y = y4; vertices[vertexIndex + 1].z = z;
					vertices[vertexIndex + 2].x = x2; vertices[vertexIndex + 2].y = y2; vertices[vertexIndex + 2].z = z;
					vertices[vertexIndex + 3].x = x3; vertices[vertexIndex + 3].y = y3;	vertices[vertexIndex + 3].z = z;

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
						int meshVertexCount = meshAttachment.vertices.Length;
						if (attVertBuffer.Length < meshVertexCount) attVertBuffer = new float[meshVertexCount];
						meshAttachment.ComputeWorldVertices(slot, attVertBuffer);

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
							int meshVertexCount = weightedMeshAttachment.uvs.Length;
							if (attVertBuffer.Length < meshVertexCount) attVertBuffer = new float[meshVertexCount];
							weightedMeshAttachment.ComputeWorldVertices(slot, attVertBuffer);

							if (pmaColors) {
								color.a = (byte)(a * slot.a * weightedMeshAttachment.a);
								color.r = (byte)(r * slot.r * weightedMeshAttachment.r * color.a);
								color.g = (byte)(g * slot.g * weightedMeshAttachment.g * color.a);
								color.b = (byte)(b * slot.b * weightedMeshAttachment.b * color.a);
								if (slot.data.blendMode == BlendMode.additive) color.a = 0;
							} else {
								color.a = (byte)(a * slot.a * weightedMeshAttachment.a);
								color.r = (byte)(r * slot.r * weightedMeshAttachment.r * 255);
								color.g = (byte)(g * slot.g * weightedMeshAttachment.g * 255);
								color.b = (byte)(b * slot.b * weightedMeshAttachment.b * 255);
							}

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
		} // Fill(...)
			

	}
}
