/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Spine {
	public struct VertexPositionColorTextureColor : IVertexType {
		public Vector3 Position;
		public Color Color;
		public Vector2 TextureCoordinate;
		public Color Color2;

		public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
		(
			new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
			new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
			new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
			new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 1)
		);

		VertexDeclaration IVertexType.VertexDeclaration {
			get { return VertexDeclaration; }
		}
	}

	// #region License
	// /*
	// Microsoft Public License (Ms-PL)
	// MonoGame - Copyright � 2009 The MonoGame Team
	//
	// All rights reserved.
	//
	// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
	// accept the license, do not use the software.
	//
	// 1. Definitions
	// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under
	// U.S. copyright law.
	//
	// A "contribution" is the original software, or any additions or changes to the software.
	// A "contributor" is any person that distributes its contribution under this license.
	// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
	//
	// 2. Grant of Rights
	// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3,
	// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
	// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3,
	// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
	//
	// 3. Conditions and Limitations
	// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
	// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software,
	// your patent license from such contributor to the software ends automatically.
	// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution
	// notices that are present in the software.
	// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including
	// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object
	// code form, you may only do so under a license that complies with this license.
	// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
	// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
	// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
	// purpose and non-infringement.
	// */
	// #endregion License
	//

	/// <summary>Draws batched meshes.</summary>
	public class MeshBatcher {
		private readonly List<MeshItem> items;
		private readonly Queue<MeshItem> freeItems;
		private VertexPositionColorTextureColor[] vertexArray = { };
		private short[] triangles = { };

		public MeshBatcher () {
			items = new List<MeshItem>(256);
			freeItems = new Queue<MeshItem>(256);
			EnsureCapacity(256, 512);
		}

		/// <summary>Returns a pooled MeshItem.</summary>
		public MeshItem NextItem (int vertexCount, int triangleCount) {
			MeshItem item = freeItems.Count > 0 ? freeItems.Dequeue() : new MeshItem();
			if (item.vertices.Length < vertexCount) item.vertices = new VertexPositionColorTextureColor[vertexCount];
			if (item.triangles.Length < triangleCount) item.triangles = new int[triangleCount];
			item.vertexCount = vertexCount;
			item.triangleCount = triangleCount;
			items.Add(item);
			return item;
		}

		private void EnsureCapacity (int vertexCount, int triangleCount) {
			if (vertexArray.Length < vertexCount) vertexArray = new VertexPositionColorTextureColor[vertexCount];
			if (triangles.Length < triangleCount) triangles = new short[triangleCount];
		}

		public void Draw (GraphicsDevice device) {
			if (items.Count == 0) return;

			int itemCount = items.Count;
			int vertexCount = 0, triangleCount = 0;
			for (int i = 0; i < itemCount; i++) {
				MeshItem item = items[i];
				vertexCount += item.vertexCount;
				triangleCount += item.triangleCount;
			}
			EnsureCapacity(vertexCount, triangleCount);

			vertexCount = 0;
			triangleCount = 0;
			Texture2D lastTexture = null;
			for (int i = 0; i < itemCount; i++) {
				MeshItem item = items[i];
				int itemVertexCount = item.vertexCount;

				if (item.texture != lastTexture || vertexCount + itemVertexCount > short.MaxValue) {
					FlushVertexArray(device, vertexCount, triangleCount);
					vertexCount = 0;
					triangleCount = 0;
					lastTexture = item.texture;
					device.Textures[0] = lastTexture;
					if (item.textureLayers != null) {
						for (int layer = 1; layer < item.textureLayers.Length; ++layer)
							device.Textures[layer] = item.textureLayers[layer];
					}
				}

				int[] itemTriangles = item.triangles;
				int itemTriangleCount = item.triangleCount;
				for (int ii = 0, t = triangleCount; ii < itemTriangleCount; ii++, t++)
					triangles[t] = (short)(itemTriangles[ii] + vertexCount);
				triangleCount += itemTriangleCount;

				Array.Copy(item.vertices, 0, vertexArray, vertexCount, itemVertexCount);
				vertexCount += itemVertexCount;
			}
			FlushVertexArray(device, vertexCount, triangleCount);
		}

		public void AfterLastDrawPass () {
			int itemCount = items.Count;
			for (int i = 0; i < itemCount; i++) {
				var item = items[i];
				item.texture = null;
				freeItems.Enqueue(item);
			}
			items.Clear();
		}

		private void FlushVertexArray (GraphicsDevice device, int vertexCount, int triangleCount) {
			if (vertexCount == 0) return;
			device.DrawUserIndexedPrimitives(
				PrimitiveType.TriangleList,
				vertexArray, 0, vertexCount,
				triangles, 0, triangleCount / 3,
				VertexPositionColorTextureColor.VertexDeclaration);
		}
	}

	public class MeshItem {
		public Texture2D texture = null;
		public Texture2D[] textureLayers = null;
		public int vertexCount, triangleCount;
		public VertexPositionColorTextureColor[] vertices = { };
		public int[] triangles = { };
	}
}
