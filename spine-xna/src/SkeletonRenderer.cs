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
	/// <summary>Draws region and mesh attachments.</summary>
	public class SkeletonRenderer {
		private const int TL = 0;
		private const int TR = 1;
		private const int BL = 2;
		private const int BR = 3;

		SkeletonClipping clipper = new SkeletonClipping();
		GraphicsDevice device;
		MeshBatcher batcher;
		public MeshBatcher Batcher { get { return batcher; } }
		RasterizerState rasterizerState;
		float[] vertices = new float[8];
		int[] quadTriangles = { 0, 1, 2, 2, 3, 0 };
		BlendState defaultBlendState;

		Effect effect;
		public Effect Effect { get { return effect; } set { effect = value; } }
		public IVertexEffect VertexEffect { get; set; }

		private bool premultipliedAlpha;
		public bool PremultipliedAlpha { get { return premultipliedAlpha; } set { premultipliedAlpha = value; } }

		/// <summary>Attachments are rendered back to front in the x/y plane by the SkeletonRenderer.
		/// Each attachment is offset by a customizable z-spacing value on the z-axis to avoid z-fighting
		/// in shaders with ZWrite enabled. Typical values lie in the range [-0.1, 0].</summary>
		private float zSpacing = 0.0f;
		public float ZSpacing { get { return zSpacing; } set { zSpacing = value; } }

		public SkeletonRenderer (GraphicsDevice device) {
			this.device = device;

			batcher = new MeshBatcher();

			var basicEffect = new BasicEffect(device);
			basicEffect.World = Matrix.Identity;
			basicEffect.View = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up);
			basicEffect.TextureEnabled = true;
			basicEffect.VertexColorEnabled = true;
			effect = basicEffect;

			rasterizerState = new RasterizerState();
			rasterizerState.CullMode = CullMode.None;

			Bone.yDown = true;
		}

		public void Begin () {
			defaultBlendState = premultipliedAlpha ? BlendState.AlphaBlend : BlendState.NonPremultiplied;

			device.RasterizerState = rasterizerState;
			device.BlendState = defaultBlendState;
		}

		public void End () {
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				batcher.Draw(device);
			}
			batcher.AfterLastDrawPass();
		}

		public void Draw(Skeleton skeleton) {
			var drawOrder = skeleton.DrawOrder;
			var drawOrderItems = skeleton.DrawOrder.Items;
			float skeletonR = skeleton.R, skeletonG = skeleton.G, skeletonB = skeleton.B, skeletonA = skeleton.A;
			Color color = new Color();

			if (VertexEffect != null) VertexEffect.Begin(skeleton);

			for (int i = 0, n = drawOrder.Count; i < n; i++) {
				Slot slot = drawOrderItems[i];
				Attachment attachment = slot.Attachment;
				float attachmentZOffset = zSpacing * i;

				float attachmentColorR, attachmentColorG, attachmentColorB, attachmentColorA;
				object textureObject = null;
				int verticesCount = 0;
				float[] vertices = this.vertices;
				int indicesCount = 0;
				int[] indices = null;
				float[] uvs = null;

				if (attachment is RegionAttachment) {
					RegionAttachment regionAttachment = (RegionAttachment)attachment;
					attachmentColorR = regionAttachment.R; attachmentColorG = regionAttachment.G; attachmentColorB = regionAttachment.B; attachmentColorA = regionAttachment.A;
					AtlasRegion region = (AtlasRegion)regionAttachment.RendererObject;
					textureObject = region.page.rendererObject;
					verticesCount = 4;
					regionAttachment.ComputeWorldVertices(slot.Bone, vertices, 0, 2);
					indicesCount = 6;
					indices = quadTriangles;
					uvs = regionAttachment.UVs;
				}
				else if (attachment is MeshAttachment) {
					MeshAttachment mesh = (MeshAttachment)attachment;
					attachmentColorR = mesh.R; attachmentColorG = mesh.G; attachmentColorB = mesh.B; attachmentColorA = mesh.A;
					AtlasRegion region = (AtlasRegion)mesh.RendererObject;
					textureObject = region.page.rendererObject;
					int vertexCount = mesh.WorldVerticesLength;
					if (vertices.Length < vertexCount) vertices = new float[vertexCount];
					verticesCount = vertexCount >> 1;
					mesh.ComputeWorldVertices(slot, vertices);
					indicesCount = mesh.Triangles.Length;
					indices = mesh.Triangles;
					uvs = mesh.UVs;
				}
				else if (attachment is ClippingAttachment) {
					ClippingAttachment clip = (ClippingAttachment)attachment;
					clipper.ClipStart(slot, clip);
					continue;
				}
				else {
					continue;
				}

				// set blend state
				BlendState blend = slot.Data.BlendMode == BlendMode.Additive ? BlendState.Additive : defaultBlendState;
				if (device.BlendState != blend) {
					//End();
					//device.BlendState = blend;
				}

				// calculate color
				float a = skeletonA * slot.A * attachmentColorA;
				if (premultipliedAlpha) {
					color = new Color(
							skeletonR * slot.R * attachmentColorR * a,
							skeletonG * slot.G * attachmentColorG * a,
							skeletonB * slot.B * attachmentColorB * a, a);
				}
				else {
					color = new Color(
							skeletonR * slot.R * attachmentColorR,
							skeletonG * slot.G * attachmentColorG,
							skeletonB * slot.B * attachmentColorB, a);
				}

				Color darkColor = new Color();
				if (slot.HasSecondColor) {
					if (premultipliedAlpha) {
						darkColor = new Color(slot.R2 * a, slot.G2 * a, slot.B2 * a);
					} else {
						darkColor = new Color(slot.R2 * a, slot.G2 * a, slot.B2 * a);
					}
				}
				darkColor.A = premultipliedAlpha ? (byte)255 : (byte)0;

				// clip
				if (clipper.IsClipping) {
					clipper.ClipTriangles(vertices, verticesCount << 1, indices, indicesCount, uvs);
					vertices = clipper.ClippedVertices.Items;
					verticesCount = clipper.ClippedVertices.Count >> 1;
					indices = clipper.ClippedTriangles.Items;
					indicesCount = clipper.ClippedTriangles.Count;
					uvs = clipper.ClippedUVs.Items;
				}

				if (verticesCount == 0 || indicesCount == 0)
					continue;

				// submit to batch
				MeshItem item = batcher.NextItem(verticesCount, indicesCount);
				if (textureObject is Texture2D)
					item.texture = (Texture2D) textureObject;
				else {
					item.textureLayers = (Texture2D[]) textureObject;
					item.texture = item.textureLayers[0];
				}
				for (int ii = 0, nn = indicesCount; ii < nn; ii++) {
					item.triangles[ii] = indices[ii];
				}
				VertexPositionColorTextureColor[] itemVertices = item.vertices;
				for (int ii = 0, v = 0, nn = verticesCount << 1; v < nn; ii++, v += 2) {
					itemVertices[ii].Color = color;
					itemVertices[ii].Color2 = darkColor;
					itemVertices[ii].Position.X = vertices[v];
					itemVertices[ii].Position.Y = vertices[v + 1];
					itemVertices[ii].Position.Z = attachmentZOffset;
					itemVertices[ii].TextureCoordinate.X = uvs[v];
					itemVertices[ii].TextureCoordinate.Y = uvs[v + 1];
					if (VertexEffect != null) VertexEffect.Transform(ref itemVertices[ii]);
				}

				clipper.ClipEnd(slot);
			}
			clipper.ClipEnd();
			if (VertexEffect != null) VertexEffect.End();
		}
	}
}
