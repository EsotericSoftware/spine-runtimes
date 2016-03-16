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
#if (UNITY_5_0 || UNITY_5_1 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
#define PREUNITY_5_2
#endif

using UnityEngine;
using System.Collections.Generic;

#if !(PREUNITY_5_2)
namespace Spine.Unity.MeshGeneration {
	/// <summary>This is for testing and educational purposes only. This takes about 10 times longer to render than the Arrays implementations.</summary>
	public class VertexHelperSpineMeshGenerator : ISimpleMeshGenerator {

		public float scale = 1f;
		public float Scale { get { return scale; } set { scale = value; } }

		public bool premultiplyAlpha = true;
		public bool PremultiplyAlpha { get { return premultiplyAlpha; } set { premultiplyAlpha = value; } }

		public int CurrentVertexCount { get { return this.positions.Count; } }

		private Mesh lastGeneratedMesh;
		public Mesh LastGeneratedMesh {	get { return lastGeneratedMesh; } }

		public Mesh GenerateMesh (Skeleton skeleton) {
			skeletonColor.r = skeleton.r;
			skeletonColor.g = skeleton.g;
			skeletonColor.b = skeleton.b;
			skeletonColor.a = skeleton.a;

			ClearBuffers();
			var drawOrderItems = skeleton.drawOrder.Items;
			for (int i = 0, n = skeleton.drawOrder.Count; i < n; i++) {
				AddSlot(drawOrderItems[i]);
			}

			Mesh currentMesh = doubleBufferedMesh.GetNextMesh();
			FillMesh(currentMesh);
			lastGeneratedMesh = currentMesh;
			return currentMesh;
		}

		#region Internals
		private DoubleBufferedMesh doubleBufferedMesh = new DoubleBufferedMesh();
		protected Color skeletonColor = Color.white;
		protected List<Vector3> positions = new List<Vector3>();
		//protected List<Color> colors = new List<Color>(); // 5.3 mesh.SetColors(Color) is broken in UI.
		protected List<Color32> colors32 = new List<Color32>();
		protected List<Vector2> uvs = new List<Vector2>();
		protected List<int> indices = new List<int>();
		protected List<Vector3> normals = new List<Vector3>();

		// Buffers
		protected float[] tempVertices = new float[8];

		static readonly Vector3 DefaultNormal = Vector3.back;
		const float Z = 0f;

		protected void FillMesh (Mesh mesh) {
			mesh.Clear();

			if (positions.Count > 65000)
				throw new System.ArgumentException("Mesh cannot have more than 65000 vertices.");	// Limitation based on UnityEngine.UI.VertexHelper

			mesh.SetVertices(positions);
			//mesh.SetColors(colors); // 5.3 mesh.SetColors(Color) is broken in UI.
			mesh.SetColors(colors32);
			mesh.SetUVs(0, uvs);
			mesh.SetNormals(normals);
			mesh.SetTriangles(indices, 0);
			mesh.RecalculateBounds();
		}

		protected void ClearBuffers () {
			positions.Clear();
			//colors.Clear(); // 5.3 mesh.SetColors(Color) is broken.
			colors32.Clear();
			uvs.Clear();
			indices.Clear();
			normals.Clear();
		}

		protected void AddVert (Vector3 position, Color color, Vector2 uv) {
			positions.Add(position);
			//colors.Add(color); // 5.3 mesh.SetColors(Color) is broken in UI.
			Color32 c; c.r = (byte)(color.r * 255); c.g = (byte)(color.g * 255); c.b = (byte)(color.b * 255); c.a = (byte)(color.a * 255);
			colors32.Add(c);
			uvs.Add(uv);
			normals.Add(DefaultNormal);
		}

		protected void AddTriangle (int id0, int id1, int id2) {
			indices.Add(id0);
			indices.Add(id1);
			indices.Add(id2);
		}

		protected void AddSlot (Slot slot) {
			var a = slot.attachment;

			var regionAttachment = a as RegionAttachment;
			if (regionAttachment != null) {
				AddAttachment(slot, regionAttachment);
				return;
			}

			var meshAttachment = a as MeshAttachment;
			if (meshAttachment != null) {
				AddAttachment(slot, meshAttachment);
				return;
			}

			var skinnedMeshAttachment = a as WeightedMeshAttachment;
			if (skinnedMeshAttachment != null) {
				AddAttachment(slot, skinnedMeshAttachment);
				return;
			}
		}

		// RegionAttachment
		protected void AddAttachment (Slot slot, RegionAttachment attachment) {
			var tempVertices = this.tempVertices;
			attachment.ComputeWorldVertices(slot.bone, tempVertices);

			float[] regionUVs = attachment.uvs;

			Color color = skeletonColor;
			color.r = color.r * attachment.r * slot.r;
			color.g = color.g * attachment.g * slot.g;
			color.b = color.b * attachment.b * slot.b;
			color.a = color.a * attachment.a * slot.a;
			if (premultiplyAlpha) {
				color.r *= color.a; color.g *= color.a; color.b *= color.a;
				if (slot.data.blendMode == BlendMode.additive) color.a = 0;
			}

			int fv = positions.Count; // first vertex index
			AddVert(new Vector3(tempVertices[RegionAttachment.X1] * scale, tempVertices[RegionAttachment.Y1] * scale), color, new Vector2(regionUVs[RegionAttachment.X1], regionUVs[RegionAttachment.Y1]));
			AddVert(new Vector3(tempVertices[RegionAttachment.X4] * scale, tempVertices[RegionAttachment.Y4] * scale), color, new Vector2(regionUVs[RegionAttachment.X4], regionUVs[RegionAttachment.Y4]));
			AddVert(new Vector3(tempVertices[RegionAttachment.X2] * scale, tempVertices[RegionAttachment.Y2] * scale), color, new Vector2(regionUVs[RegionAttachment.X2], regionUVs[RegionAttachment.Y2]));
			AddVert(new Vector3(tempVertices[RegionAttachment.X3] * scale, tempVertices[RegionAttachment.Y3] * scale), color, new Vector2(regionUVs[RegionAttachment.X3], regionUVs[RegionAttachment.Y3]));

			AddTriangle(fv, fv+2, fv+1);
			AddTriangle(fv+2, fv+3, fv+1);
		}

		// MeshAttachment
		protected void AddAttachment (Slot slot, MeshAttachment attachment) {
			var tempVertices = this.tempVertices;
			var meshUVs = attachment.uvs;
			int meshVertexCount = attachment.vertices.Length;

			if (tempVertices.Length < meshVertexCount)
				this.tempVertices = tempVertices = new float[meshVertexCount];
			attachment.ComputeWorldVertices(slot, tempVertices);

			Color color = skeletonColor;
			color.r = color.r * attachment.r * slot.r;
			color.g = color.g * attachment.g * slot.g;
			color.b = color.b * attachment.b * slot.b;
			color.a = color.a * attachment.a * slot.a;
			if (premultiplyAlpha) {
				color.r *= color.a; color.g *= color.a; color.b *= color.a;
				if (slot.data.blendMode == BlendMode.additive) color.a = 0;
			}

			int fv = positions.Count; // first vertex index
			for (int ii = 0; ii < meshVertexCount; ii += 2)
				AddVert(new Vector3(tempVertices[ii], tempVertices[ii + 1]) * scale, color, new Vector2(meshUVs[ii], meshUVs[ii + 1]));

			var attachmentTriangles = attachment.triangles;
			for (int ii = 0, n = attachmentTriangles.Length; ii < n; ii++)
				indices.Add(attachmentTriangles[ii] + fv);
		}

		// SkinnedMeshAttachment
		protected void AddAttachment (Slot slot, WeightedMeshAttachment attachment) {
			var tempVertices = this.tempVertices;
			float[] meshUVs = attachment.uvs;

			int meshVertexCount = attachment.uvs.Length;
			if (tempVertices.Length < meshVertexCount)
				this.tempVertices = tempVertices = new float[meshVertexCount];
			attachment.ComputeWorldVertices(slot, tempVertices);

			Color color = skeletonColor;
			color.r = color.r * attachment.r * slot.r;
			color.g = color.g * attachment.g * slot.g;
			color.b = color.b * attachment.b * slot.b;
			color.a = color.a * attachment.a * slot.a;
			if (premultiplyAlpha) {
				color.r *= color.a; color.g *= color.a; color.b *= color.a;
				if (slot.data.blendMode == BlendMode.additive) color.a = 0;
			}

			int fv = positions.Count; // first vertex index
			for (int ii = 0; ii < meshVertexCount; ii += 2)
				AddVert(new Vector3(tempVertices[ii], tempVertices[ii + 1]) * scale, color, new Vector2(meshUVs[ii], meshUVs[ii + 1]));

			var attachmentTriangles = attachment.triangles;
			for (int ii = 0, n = attachmentTriangles.Length; ii < n; ii++)
				indices.Add(attachmentTriangles[ii] + fv);
		}
		#endregion

	}

}
#endif
