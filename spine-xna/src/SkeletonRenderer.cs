/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Spine {
	public class SkeletonRenderer {
		GraphicsDevice device;
		SpriteBatcher batcher;
		BasicEffect effect;
		RasterizerState rasterizerState;
		public BlendState BlendState { get; set; }

		public SkeletonRenderer (GraphicsDevice device) {
			this.device = device;

			batcher = new SpriteBatcher();

			effect = new BasicEffect(device);
			effect.World = Matrix.Identity;
			effect.View = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up);
			effect.TextureEnabled = true;
			effect.VertexColorEnabled = true;

			rasterizerState = new RasterizerState();
			rasterizerState.CullMode = CullMode.None;

			BlendState = BlendState.AlphaBlend;

			Bone.yDown = true;
		}

		public void Begin () {
			device.RasterizerState = rasterizerState;
			device.BlendState = BlendState;

			effect.Projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, 1, 0);
		}

		public void End () {
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				batcher.Draw(device);
			}
		}

		public void Draw (Skeleton skeleton) {
			List<Slot> drawOrder = skeleton.DrawOrder;
			for (int i = 0, n = drawOrder.Count; i < n; i++) {
				Slot slot = drawOrder[i];
				Attachment attachment = slot.Attachment;
				if (attachment is RegionAttachment) {
					RegionAttachment regionAttachment = (RegionAttachment)attachment;

					SpriteBatchItem item = batcher.CreateBatchItem();
					item.Texture = (Texture2D)regionAttachment.Region.page.texture;

					byte r = (byte)(skeleton.R * slot.R * 255);
					byte g = (byte)(skeleton.G * slot.G * 255);
					byte b = (byte)(skeleton.B * slot.B * 255);
					byte a = (byte)(skeleton.A * slot.A * 255);
					item.vertexTL.Color.R = r;
					item.vertexTL.Color.G = g;
					item.vertexTL.Color.B = b;
					item.vertexTL.Color.A = a;
					item.vertexBL.Color.R = r;
					item.vertexBL.Color.G = g;
					item.vertexBL.Color.B = b;
					item.vertexBL.Color.A = a;
					item.vertexBR.Color.R = r;
					item.vertexBR.Color.G = g;
					item.vertexBR.Color.B = b;
					item.vertexBR.Color.A = a;
					item.vertexTR.Color.R = r;
					item.vertexTR.Color.G = g;
					item.vertexTR.Color.B = b;
					item.vertexTR.Color.A = a;

					regionAttachment.UpdateVertices(slot.Bone);
					float[] vertices = regionAttachment.Vertices;
					item.vertexTL.Position.X = vertices[RegionAttachment.X1];
					item.vertexTL.Position.Y = vertices[RegionAttachment.Y1];
					item.vertexTL.Position.Z = 0;
					item.vertexBL.Position.X = vertices[RegionAttachment.X2];
					item.vertexBL.Position.Y = vertices[RegionAttachment.Y2];
					item.vertexBL.Position.Z = 0;
					item.vertexBR.Position.X = vertices[RegionAttachment.X3];
					item.vertexBR.Position.Y = vertices[RegionAttachment.Y3];
					item.vertexBR.Position.Z = 0;
					item.vertexTR.Position.X = vertices[RegionAttachment.X4];
					item.vertexTR.Position.Y = vertices[RegionAttachment.Y4];
					item.vertexTR.Position.Z = 0;

					AtlasRegion region = regionAttachment.Region;
					if (region.rotate) {
						item.vertexBL.TextureCoordinate.X = region.u;
						item.vertexBL.TextureCoordinate.Y = region.v2;
						item.vertexBR.TextureCoordinate.X = region.u;
						item.vertexBR.TextureCoordinate.Y = region.v;
						item.vertexTR.TextureCoordinate.X = region.u2;
						item.vertexTR.TextureCoordinate.Y = region.v;
						item.vertexTL.TextureCoordinate.X = region.u2;
						item.vertexTL.TextureCoordinate.Y = region.v2;
					} else {
						item.vertexTL.TextureCoordinate.X = region.u;
						item.vertexTL.TextureCoordinate.Y = region.v2;
						item.vertexBL.TextureCoordinate.X = region.u;
						item.vertexBL.TextureCoordinate.Y = region.v;
						item.vertexBR.TextureCoordinate.X = region.u2;
						item.vertexBR.TextureCoordinate.Y = region.v;
						item.vertexTR.TextureCoordinate.X = region.u2;
						item.vertexTR.TextureCoordinate.Y = region.v2;
					}
				}
			}
		}
	}
}
