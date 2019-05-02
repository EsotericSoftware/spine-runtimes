/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	/// <summary>Attachment that displays a texture region using a mesh.</summary>
	public class MeshAttachment : VertexAttachment, IHasRendererObject {
		internal float regionOffsetX, regionOffsetY, regionWidth, regionHeight, regionOriginalWidth, regionOriginalHeight;
		private MeshAttachment parentMesh;
		internal float[] uvs, regionUVs;
		internal int[] triangles;
		internal float r = 1, g = 1, b = 1, a = 1;
		internal int hulllength;
		internal bool inheritDeform;

		public int HullLength { get { return hulllength; } set { hulllength = value; } }
		public float[] RegionUVs { get { return regionUVs; } set { regionUVs = value; } }
		/// <summary>The UV pair for each vertex, normalized within the entire texture. <seealso cref="MeshAttachment.UpdateUVs"/></summary>
		public float[] UVs { get { return uvs; } set { uvs = value; } }
		public int[] Triangles { get { return triangles; } set { triangles = value; } }

		public float R { get { return r; } set { r = value; } }
		public float G { get { return g; } set { g = value; } }
		public float B { get { return b; } set { b = value; } }
		public float A { get { return a; } set { a = value; } }

		public string Path { get; set; }
		public object RendererObject { get; set; }
		public float RegionU { get; set; }
		public float RegionV { get; set; }
		public float RegionU2 { get; set; }
		public float RegionV2 { get; set; }
		public bool RegionRotate { get; set; }
		public float RegionOffsetX { get { return regionOffsetX; } set { regionOffsetX = value; } }
		public float RegionOffsetY { get { return regionOffsetY; } set { regionOffsetY = value; } } // Pixels stripped from the bottom left, unrotated.
		public float RegionWidth { get { return regionWidth; } set { regionWidth = value; } }
		public float RegionHeight { get { return regionHeight; } set { regionHeight = value; } } // Unrotated, stripped size.
		public float RegionOriginalWidth { get { return regionOriginalWidth; } set { regionOriginalWidth = value; } }
		public float RegionOriginalHeight { get { return regionOriginalHeight; } set { regionOriginalHeight = value; } } // Unrotated, unstripped size.

		public bool InheritDeform { get { return inheritDeform; } set { inheritDeform = value; } }

		public MeshAttachment ParentMesh {
			get { return parentMesh; }
			set {
				parentMesh = value;
				if (value != null) {
					bones = value.bones;
					vertices = value.vertices;
					worldVerticesLength = value.worldVerticesLength;
					regionUVs = value.regionUVs;
					triangles = value.triangles;
					HullLength = value.HullLength;
					Edges = value.Edges;
					Width = value.Width;
					Height = value.Height;
				}
			}
		}

		// Nonessential.
		public int[] Edges { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }

		public MeshAttachment (string name)
			: base(name) {
		}

		public void UpdateUVs () {
			float[] regionUVs = this.regionUVs;
			if (this.uvs == null || this.uvs.Length != regionUVs.Length) this.uvs = new float[regionUVs.Length];
			float[] uvs = this.uvs;

			if (RegionRotate) {
				float textureHeight = this.regionWidth / (RegionV2 - RegionV);
				float textureWidth = this.regionHeight / (RegionU2 - RegionU);
				float u = RegionU - (RegionOriginalHeight - RegionOffsetY - RegionHeight) / textureWidth;
				float v = RegionV - (RegionOriginalWidth - RegionOffsetX - RegionWidth) / textureHeight;
				float width = RegionOriginalHeight / textureWidth;
				float height = RegionOriginalWidth / textureHeight;

				for (int i = 0, n = uvs.Length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i + 1] * width;
					uvs[i + 1] = v + height - regionUVs[i] * height;
				}
			} else {
				float textureWidth = this.regionWidth / (RegionU2 - RegionU);
				float textureHeight = this.regionHeight / (RegionV2 - RegionV);
				float u = RegionU - RegionOffsetX / textureWidth;
				float v = RegionV - (RegionOriginalHeight - RegionOffsetY - RegionHeight) / textureHeight;
				float width = RegionOriginalWidth / textureWidth;
				float height = RegionOriginalHeight / textureHeight;

				for (int i = 0, n = uvs.Length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i] * width;
					uvs[i + 1] = v + regionUVs[i + 1] * height;
				}
			}
		}

		override public bool ApplyDeform (VertexAttachment sourceAttachment) {
			return this == sourceAttachment || (inheritDeform && parentMesh == sourceAttachment);
		}
	}
}
