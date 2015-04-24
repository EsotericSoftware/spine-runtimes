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

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Spine {
	// #region License
	// /*
	// Microsoft Public License (Ms-PL)
	// MonoGame - Copyright © 2009 The MonoGame Team
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

	/// <summary>Draws batched quads using indices.</summary>
	public class RegionBatcher {
		private const int maxBatchSize = short.MaxValue / 6; // 6 = 4 vertices unique and 2 shared, per quad
		private readonly List<RegionItem> items;
		private readonly Queue<RegionItem> freeItems;
		private VertexPositionColorTexture[] vertexArray;
		private short[] indices;

		public RegionBatcher () {
			items = new List<RegionItem>(256);
			freeItems = new Queue<RegionItem>(256);
			EnsureArrayCapacity(256);
		}

		/// <summary>Returns a pooled RegionItem.</summary>
		public RegionItem NextItem () {
			RegionItem item = freeItems.Count > 0 ? freeItems.Dequeue() : new RegionItem();
			items.Add(item);
			return item;
		}

		/// <summary>Resize and recreate the indices and vertex position color buffers.</summary>
		private void EnsureArrayCapacity (int itemCount) {
			if (indices != null && indices.Length >= 6 * itemCount) return;

			short[] newIndices = new short[6 * itemCount];
			int start = 0;
			if (indices != null) {
				indices.CopyTo(newIndices, 0);
				start = indices.Length / 6;
			}
			for (var i = start; i < itemCount; i++) {
				/* TL    TR
				 *  0----1 0,1,2,3 = index offsets for vertex indices
				 *  |    | TL,TR,BL,BR are vertex references in RegionItem.
				 *  2----3
				 * BL    BR */
				newIndices[i * 6 + 0] = (short)(i * 4);
				newIndices[i * 6 + 1] = (short)(i * 4 + 1);
				newIndices[i * 6 + 2] = (short)(i * 4 + 2);
				newIndices[i * 6 + 3] = (short)(i * 4 + 1);
				newIndices[i * 6 + 4] = (short)(i * 4 + 3);
				newIndices[i * 6 + 5] = (short)(i * 4 + 2);
			}
			indices = newIndices;

			vertexArray = new VertexPositionColorTexture[4 * itemCount];
		}

		public void Draw (GraphicsDevice device) {
			if (items.Count == 0) return;

			int itemIndex = 0;
			int itemCount = items.Count;
			while (itemCount > 0) {
				int itemsToProcess = Math.Min(itemCount, maxBatchSize);
				EnsureArrayCapacity(itemsToProcess);

				var count = 0;
				Texture2D texture = null;
				for (int i = 0; i < itemsToProcess; i++, itemIndex++) {
					RegionItem item = items[itemIndex];
					if (item.texture != texture) {
						FlushVertexArray(device, count);
						texture = item.texture;
						count = 0;
						device.Textures[0] = texture;
					}

					vertexArray[count++] = item.vertexTL;
					vertexArray[count++] = item.vertexTR;
					vertexArray[count++] = item.vertexBL;
					vertexArray[count++] = item.vertexBR;

					item.texture = null;
					freeItems.Enqueue(item);
				}
				FlushVertexArray(device, count);
				itemCount -= itemsToProcess;
			}
			items.Clear();
		}

		/// <summary>Sends the triangle list to the graphics device.</summary>
		/// <param name="start">Start index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
		/// <param name="end">End index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
		private void FlushVertexArray (GraphicsDevice device, int count) {
			if (count == 0) return;
			device.DrawUserIndexedPrimitives(
				PrimitiveType.TriangleList,
				vertexArray, 0, count,
				indices, 0, (count / 4) * 2,
				VertexPositionColorTexture.VertexDeclaration);
		}
	}

	public class RegionItem {
		public Texture2D texture;
		public VertexPositionColorTexture vertexTL;
		public VertexPositionColorTexture vertexTR;
		public VertexPositionColorTexture vertexBL;
		public VertexPositionColorTexture vertexBR;
	}
}
