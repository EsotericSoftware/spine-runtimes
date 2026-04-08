/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#if (UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_WSA || UNITY_WP8 || UNITY_WP8_1)
#define IS_UNITY
#endif

using System;

namespace Spine {
#if IS_UNITY
	using Color32F = UnityEngine.Color;
#endif

	/// <summary>Attachment that displays a texture region using a mesh.</summary>
	public class MeshAttachment : VertexAttachment, IHasSequence {
		internal readonly Sequence sequence;
		internal float[] regionUVs;
		internal int[] triangles;
		internal int hullLength;
		internal string path;
		// Color is a struct, set to protected to prevent
		// Color color = slot.color; color.a = 0.5;
		// modifying just a copy of the struct instead of the original
		// object as in reference implementation.
		protected Color32F color = new Color32F(1, 1, 1, 1);
		private MeshAttachment parentMesh;

		public int HullLength { get { return hullLength; } set { hullLength = value; } }

		/// <summary>The UV pair for each vertex, normalized within the texture region.</summary>
		public float[] RegionUVs { get { return regionUVs; } set { regionUVs = value; } }
		/// <summary>Triplets of vertex indices which describe the mesh's triangulation.</summary>
		public int[] Triangles { get { return triangles; } set { triangles = value; } }

		public Color32F GetColor () {
			return color;
		}

		public void SetColor (Color32F color) {
			this.color = color;
		}

		public void SetColor (float r, float g, float b, float a) {
			color = new Color32F(r, g, b, a);
		}

		public string Path { get { return path; } set { path = value; } }
		public Sequence Sequence { get { return sequence; } }

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
		/// <summary>
		/// Vertex index pairs describing edges for controlling triangulation, or null if nonessential data was not exported. Mesh
		/// triangles do not cross edges. Triangulation is not performed at runtime.
		/// </summary>
		public int[] Edges { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }

		public MeshAttachment (string name, Sequence sequence)
			: base(name) {
			if (sequence == null) throw new ArgumentException("sequence cannot be null.", "sequence");
			this.sequence = sequence;
		}

		/// <summary>Copy constructor. Use <see cref="NewLinkedMesh"/> if the other mesh is a linked mesh.</summary>
		protected MeshAttachment (MeshAttachment other)
			: base(other) {

			if (parentMesh != null) throw new ArgumentException("Use newLinkedMesh to copy a linked mesh.");

			path = other.path;
			color = other.color;

			regionUVs = new float[other.regionUVs.Length];
			Array.Copy(other.regionUVs, 0, regionUVs, 0, regionUVs.Length);

			triangles = new int[other.triangles.Length];
			Array.Copy(other.triangles, 0, triangles, 0, triangles.Length);

			hullLength = other.hullLength;
			sequence = new Sequence(other.sequence);

			// Nonessential.
			if (other.Edges != null) {
				Edges = new int[other.Edges.Length];
				Array.Copy(other.Edges, 0, Edges, 0, Edges.Length);
			}
			Width = other.Width;
			Height = other.Height;
		}

		public void UpdateSequence () {
			sequence.Update(this);
		}

		/// <summary>Returns a new mesh with this mesh set as the <see cref="ParentMesh"/>.
		public MeshAttachment NewLinkedMesh () {
			var mesh = new MeshAttachment(Name, new Sequence(sequence));

			mesh.timelineAttachment = timelineAttachment;
			mesh.path = path;
			mesh.color = color;
			mesh.ParentMesh = parentMesh != null ? parentMesh : this;
			mesh.UpdateSequence();
			return mesh;
		}

		public override Attachment Copy () {
			return parentMesh != null ? NewLinkedMesh() : new MeshAttachment(this);
		}

		/// <summary>
		/// Computes <see cref="Sequence.GetUVs(int)">UVs</see> for a mesh attachment.
		/// </summary>
		/// <param name="uvs">Output array for the computed UVs, same length as regionUVs.</param>
		internal static void ComputeUVs (TextureRegion region, float[] regionUVs, float[] uvs) {
			int n = uvs.Length;
			float u, v, width, height;
			AtlasRegion r = region as AtlasRegion;
			if (r != null) {
				u = r.u;
				v = r.v;
				float textureWidth = region.width / (region.u2 - region.u);
				float textureHeight = region.height / (region.v2 - region.v);
				switch (r.degrees) {
				case 90: {
					u -= (r.originalHeight - r.offsetY - r.packedWidth) / textureWidth;
					v -= (r.originalWidth - r.offsetX - r.packedHeight) / textureHeight;
					width = r.originalHeight / textureWidth;
					height = r.originalWidth / textureHeight;
					for (int i = 0; i < n; i += 2) {
						uvs[i] = u + regionUVs[i + 1] * width;
						uvs[i + 1] = v + (1 - regionUVs[i]) * height;
					}
					return;
				}
				case 180: {
					u -= (r.originalWidth - r.offsetX - r.packedWidth) / textureWidth;
					v -= r.offsetY / textureHeight;
					width = r.originalWidth / textureWidth;
					height = r.originalHeight / textureHeight;
					for (int i = 0; i < n; i += 2) {
						uvs[i] = u + (1 - regionUVs[i]) * width;
						uvs[i + 1] = v + (1 - regionUVs[i + 1]) * height;
					}
					return;
				}
				case 270: {
					u -= r.offsetY / textureWidth;
					v -= r.offsetX / textureHeight;
					width = r.originalHeight / textureWidth;
					height = r.originalWidth / textureHeight;
					for (int i = 0; i < n; i += 2) {
						uvs[i] = u + (1 - regionUVs[i + 1]) * width;
						uvs[i + 1] = v + regionUVs[i] * height;
					}
					return;
				}
				default: {
					u -= r.offsetX / textureWidth;
					v -= (r.originalHeight - r.offsetY - r.packedHeight) / textureHeight;
					width = r.originalWidth / textureWidth;
					height = r.originalHeight / textureHeight;
					break;
				}
				}
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
	}
}
