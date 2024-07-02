/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2024, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2018_1_OR_NEWER
#define HAS_NATIVE_COLLECTIONS
#endif

using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace Spine.Unity {

	/// <summary>Holds several methods to prepare and generate a UnityEngine mesh based on a skeleton. Contains buffers needed to perform the operation, and serializes settings for mesh generation.</summary>
	[System.Serializable]
	public class MeshGeneratorUIElements : MeshGenerator {

#if HAS_NATIVE_COLLECTIONS
		public void FillVertexData (ref NativeSlice<UnityEngine.UIElements.Vertex> uiVertices) {

			var vertexBufferItems = vertexBuffer.Items;
			var uvBufferItems = uvBuffer.Items;
			var colorBufferItems = colorBuffer.Items;
			int vertexBufferLength = vertexBufferItems.Length;

			int vertexCount = vertexBuffer.Count;
			// Zero the extra.
			{
				var vector3zero = Vector3.zero;
				for (int i = vertexCount; i < vertexBufferLength; i++)
					vertexBufferItems[i] = vector3zero;
			}

			// Set the vertex buffer.
			{
				for (int i = 0; i < vertexCount; i++) {
					UnityEngine.UIElements.Vertex uiVertex = uiVertices[i];
					uiVertex.position = vertexBufferItems[i];
					uiVertex.tint = colorBufferItems[i];
					uiVertex.uv = uvBufferItems[i];
					uiVertices[i] = uiVertex;
				}
			}
		}

		public void FillTrianglesSingleSubmesh (ref NativeSlice<ushort> uiIndices, int submeshIndex = 0) {
			if (submeshes.Count == 0)
				return;

			var srcIndices32List = submeshes.Items[submeshIndex];
			int[] srcIndices32 = srcIndices32List.Items;
			int srcIndicesCount = srcIndices32List.Count;
			int dstIndicesCount = uiIndices.Length;
			for (int i = 0; i < srcIndicesCount; ++i)
				uiIndices[i] = (ushort)srcIndices32[i];
			for (int i = srcIndicesCount; i < dstIndicesCount; ++i)
				uiIndices[i] = 0;
		}
#endif // HAS_NATIVE_COLLECTIONS
	}
}
