/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
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

using System;

namespace Spine {
	/// <summary>Attachment that displays a texture region using a mesh.</summary>
	public class MeshAttachment : VertexAttachment, IHasTextureRegion {
		internal TextureRegion region;
		internal string path;
		internal float[] regionUVs, uvs;
		internal int[] triangles;
		internal float r = 1, g = 1, b = 1, a = 1;
		internal int hullLength;
		private MeshAttachment parentMesh;
		private Sequence sequence;

		public TextureRegion Region {
			get { return region; }
			set {
				if (value == null) throw new ArgumentNullException("region", "region cannot be null.");
				region = value;
			}
		}
		public int HullLength { get { return hullLength; } set { hullLength = value; } }
		public float[] RegionUVs { get { return regionUVs; } set { regionUVs = value; } }
		/// <summary>The UV pair for each vertex, normalized within the entire texture.
		/// <seealso cref="MeshAttachment.UpdateRegion"/></summary>
		public float[] UVs { get { return uvs; } set { uvs = value; } }
		public int[] Triangles { get { return triangles; } set { triangles = value; } }

		public float R { get { return r; } set { r = value; } }
		public float G { get { return g; } set { g = value; } }
		public float B { get { return b; } set { b = value; } }
		public float A { get { return a; } set { a = value; } }

		public string Path { get { return path; } set { path = value; } }
		public Sequence Sequence { get { return sequence; } set { sequence = value; } }

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

		/// <summary>Copy constructor. Use <see cref="NewLinkedMesh"/> if the other mesh is a linked mesh.</summary>
		protected MeshAttachment (MeshAttachment other)
			: base(other) {

			if (parentMesh != null) throw new ArgumentException("Use newLinkedMesh to copy a linked mesh.");

			region = other.region;
			path = other.path;
			r = other.r;
			g = other.g;
			b = other.b;
			a = other.a;

			regionUVs = new float[other.regionUVs.Length];
			Array.Copy(other.regionUVs, 0, regionUVs, 0, regionUVs.Length);

			uvs = new float[other.uvs.Length];
			Array.Copy(other.uvs, 0, uvs, 0, uvs.Length);

			triangles = new int[other.triangles.Length];
			Array.Copy(other.triangles, 0, triangles, 0, triangles.Length);

			hullLength = other.hullLength;
			sequence = other.sequence == null ? null : new Sequence(other.sequence);

			// Nonessential.
			if (other.Edges != null) {
				Edges = new int[other.Edges.Length];
				Array.Copy(other.Edges, 0, Edges, 0, Edges.Length);
			}
			Width = other.Width;
			Height = other.Height;
		}


		public void UpdateRegion () {
			float[] regionUVs = this.regionUVs;
			if (this.uvs == null || this.uvs.Length != regionUVs.Length) this.uvs = new float[regionUVs.Length];
			float[] uvs = this.uvs;
			int n = uvs.Length;
			float u, v, width, height;

			if (region is AtlasRegion) {
				u = this.region.u;
				v = this.region.v;
				AtlasRegion region = (AtlasRegion)this.region;
				// Note: difference from reference implementation.
				// Covers rotation since region.width and height are already setup accordingly.
				float textureWidth = this.region.width / (region.u2 - region.u);
				float textureHeight = this.region.height / (region.v2 - region.v);
				switch (region.degrees) {
				case 90:
					u -= (region.originalHeight - region.offsetY - region.packedWidth) / textureWidth;
					v -= (region.originalWidth - region.offsetX - region.packedHeight) / textureHeight;
					width = region.originalHeight / textureWidth;
					height = region.originalWidth / textureHeight;
					for (int i = 0; i < n; i += 2) {
						uvs[i] = u + regionUVs[i + 1] * width;
						uvs[i + 1] = v + (1 - regionUVs[i]) * height;
					}
					return;
				case 180:
					u -= (region.originalWidth - region.offsetX - region.packedWidth) / textureWidth;
					v -= region.offsetY / textureHeight;
					width = region.originalWidth / textureWidth;
					height = region.originalHeight / textureHeight;
					for (int i = 0; i < n; i += 2) {
						uvs[i] = u + (1 - regionUVs[i]) * width;
						uvs[i + 1] = v + (1 - regionUVs[i + 1]) * height;
					}
					return;
				case 270:
					u -= region.offsetY / textureWidth;
					v -= region.offsetX / textureHeight;
					width = region.originalHeight / textureWidth;
					height = region.originalWidth / textureHeight;
					for (int i = 0; i < n; i += 2) {
						uvs[i] = u + (1 - regionUVs[i + 1]) * width;
						uvs[i + 1] = v + regionUVs[i] * height;
					}
					return;
				}
				u -= region.offsetX / textureWidth;
				v -= (region.originalHeight - region.offsetY - region.packedHeight) / textureHeight;
				width = region.originalWidth / textureWidth;
				height = region.originalHeight / textureHeight;
			} else if (region == null) {
				u = v = 0;
				width = height = 1;
			} else {
				u = region.u;
				v = region.v;
				width = region.u2 - u;
				height = region.v2 - v;
			}
			for (int i = 0; i < n; i += 2) {
				uvs[i] = u + regionUVs[i] * width;
				uvs[i + 1] = v + regionUVs[i + 1] * height;
			}
		}

		/// <summary>If the attachment has a <see cref="Sequence"/>, the region may be changed.</summary>
		override public void ComputeWorldVertices (Slot slot, int start, int count, float[] worldVertices, int offset, int stride = 2) {
			if (sequence != null) sequence.Apply(slot, this);
			base.ComputeWorldVertices(slot, start, count, worldVertices, offset, stride);
		}

		///<summary>Returns a new mesh with this mesh set as the <see cref="ParentMesh"/>.
		public MeshAttachment NewLinkedMesh () {
			MeshAttachment mesh = new MeshAttachment(Name);

			mesh.timelineAttachment = timelineAttachment;
			mesh.region = region;
			mesh.path = path;
			mesh.r = r;
			mesh.g = g;
			mesh.b = b;
			mesh.a = a;
			mesh.ParentMesh = parentMesh != null ? parentMesh : this;
			if (mesh.Region != null) mesh.UpdateRegion();
			return mesh;
		}

		public override Attachment Copy () {
			return parentMesh != null ? NewLinkedMesh() : new MeshAttachment(this);
		}
	}
}
