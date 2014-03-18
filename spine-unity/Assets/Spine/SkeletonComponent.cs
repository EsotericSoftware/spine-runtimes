/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

/** Renders a skeleton. Extend to apply animations, get bones and manipulate them, etc. */
[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("Spine/SkeletonComponent")]
public class SkeletonComponent : MonoBehaviour {
	public SkeletonDataAsset skeletonDataAsset;
	public Skeleton skeleton;
	public String initialSkinName;
	public float timeScale = 1;
	public bool calculateNormals;
	public bool calculateTangents;
	public float zSpacing;
	private MeshFilter meshFilter;
	private Mesh mesh, mesh1, mesh2;
	private bool useMesh1;
	private float[] vertexPositions = new float[8];
	private int lastVertexCount;
	private Vector3[] vertices;
	private Color32[] colors;
	private Vector2[] uvs;
	private Material[] sharedMaterials = new Material[0];
	private List<Material> submeshMaterials = new List<Material>();
	private List<Submesh> submeshes = new List<Submesh>();

	/// <summary>False if Initialize needs to be called.</summary>
	public bool Initialized {
		get {
			if (skeletonDataAsset == null) return true;
			SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
			if (skeletonData == null) return true;
			return skeleton != null && skeleton.Data == skeletonData;
		}
	}

	public virtual void Clear () {
		if (meshFilter != null) meshFilter.sharedMesh = null;
		if (mesh != null) DestroyImmediate(mesh);
		if (renderer != null) renderer.sharedMaterial = null;
		mesh = null;
		mesh1 = null;
		mesh2 = null;
		lastVertexCount = 0;
		vertices = null;
		colors = null;
		uvs = null;
		sharedMaterials = new Material[0];
		submeshMaterials.Clear();
		submeshes.Clear();
		skeleton = null;
	}

	public virtual void Initialize () {
		if (Initialized) return;

		meshFilter = GetComponent<MeshFilter>();
		mesh1 = newMesh();
		mesh2 = newMesh();

		vertices = new Vector3[0];

		skeleton = new Skeleton(skeletonDataAsset.GetSkeletonData(false));

		if (initialSkinName != null && initialSkinName.Length > 0 && initialSkinName != "default") {
			skeleton.SetSkin(initialSkinName);
			skeleton.SetSlotsToSetupPose();
		}
	}
	
	private Mesh newMesh () {
		Mesh mesh = new Mesh();
		mesh.name = "Skeleton Mesh";
		mesh.hideFlags = HideFlags.HideAndDontSave;
		mesh.MarkDynamic();
		return mesh;
	}

	public virtual void UpdateSkeleton (float deltaTime) {
		skeleton.Update(deltaTime * timeScale);
		skeleton.UpdateWorldTransform();
	}
	
	public virtual void Update () {
		if (skeletonDataAsset == null) {
			Clear();
			return;
		}

		SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);

		if (skeletonData == null) {
			Clear();
			return;
		}
		
		// Initialize fields.
		if (skeleton == null || skeleton.Data != skeletonData)
			Initialize();

		UpdateSkeleton(Time.deltaTime);

		// Count quads and submeshes.
		int quadCount = 0, submeshQuadCount = 0;
		Material lastMaterial = null;
		submeshMaterials.Clear();
		List<Slot> drawOrder = skeleton.DrawOrder;
		for (int i = 0, n = drawOrder.Count; i < n; i++) {
			RegionAttachment regionAttachment = drawOrder[i].Attachment as RegionAttachment;
			if (regionAttachment == null)
				continue;
		
			// Add submesh when material changes.
			Material material = (Material)((AtlasRegion)regionAttachment.RendererObject).page.rendererObject;
			if (lastMaterial != material && lastMaterial != null) {
				addSubmesh(lastMaterial, quadCount, submeshQuadCount, false);
				submeshQuadCount = 0;
			} 
			lastMaterial = material;

			quadCount++;
			submeshQuadCount++;
		}
		addSubmesh(lastMaterial, quadCount, submeshQuadCount, true);
		
		// Set materials.
		if (submeshMaterials.Count == sharedMaterials.Length)
			submeshMaterials.CopyTo(sharedMaterials);
		else
			sharedMaterials = submeshMaterials.ToArray();
		renderer.sharedMaterials = sharedMaterials;
		
		// Double buffer mesh.
		Mesh mesh = useMesh1 ? mesh1 : mesh2;
		meshFilter.sharedMesh = mesh;

		// Ensure mesh data is the right size.
		Vector3[] vertices = this.vertices;
		int vertexCount = quadCount * 4;
		bool newTriangles = vertexCount > vertices.Length;
		if (newTriangles) {
			// Not enough vertices, increase size.
			this.vertices = vertices = new Vector3[vertexCount];
			this.colors = new Color32[vertexCount];
			this.uvs = new Vector2[vertexCount];
			mesh1.Clear();
			mesh2.Clear();
		} else {
			// Too many vertices, zero the extra.
			Vector3 zero = Vector3.zero;
			for (int i = vertexCount, n = lastVertexCount; i < n; i++)
				vertices[i] = zero;
		}
		lastVertexCount = vertexCount;

		// Setup mesh.
		float[] vertexPositions = this.vertexPositions;
		Vector2[] uvs = this.uvs;
		Color32[] colors = this.colors;
		int vertexIndex = 0;
		Color32 color = new Color32();
		float a = skeleton.A * 255, r = skeleton.R, g = skeleton.G, b = skeleton.B, zSpacing = this.zSpacing;
		for (int i = 0, n = drawOrder.Count; i < n; i++) {
			Slot slot = drawOrder[i];
			RegionAttachment regionAttachment = slot.Attachment as RegionAttachment;
			if (regionAttachment == null)
				continue;
			
			regionAttachment.ComputeWorldVertices(skeleton.X, skeleton.Y, slot.Bone, vertexPositions);
			
			float z = i * zSpacing;
			vertices[vertexIndex] = new Vector3(vertexPositions[RegionAttachment.X1], vertexPositions[RegionAttachment.Y1], z);
			vertices[vertexIndex + 1] = new Vector3(vertexPositions[RegionAttachment.X4], vertexPositions[RegionAttachment.Y4], z);
			vertices[vertexIndex + 2] = new Vector3(vertexPositions[RegionAttachment.X2], vertexPositions[RegionAttachment.Y2], z);
			vertices[vertexIndex + 3] = new Vector3(vertexPositions[RegionAttachment.X3], vertexPositions[RegionAttachment.Y3], z);
			
			color.a = (byte)(a * slot.A);
			color.r = (byte)(r * slot.R * color.a);
			color.g = (byte)(g * slot.G * color.a);
			color.b = (byte)(b * slot.B * color.a);
			colors[vertexIndex] = color;
			colors[vertexIndex + 1] = color;
			colors[vertexIndex + 2] = color;
			colors[vertexIndex + 3] = color;

			float[] regionUVs = regionAttachment.UVs;
			uvs[vertexIndex] = new Vector2(regionUVs[RegionAttachment.X1], 1 - regionUVs[RegionAttachment.Y1]);
			uvs[vertexIndex + 1] = new Vector2(regionUVs[RegionAttachment.X4], 1 - regionUVs[RegionAttachment.Y4]);
			uvs[vertexIndex + 2] = new Vector2(regionUVs[RegionAttachment.X2], 1 - regionUVs[RegionAttachment.Y2]);
			uvs[vertexIndex + 3] = new Vector2(regionUVs[RegionAttachment.X3], 1 - regionUVs[RegionAttachment.Y3]);

			vertexIndex += 4;
		}
		
		mesh.vertices = vertices;
		mesh.colors32 = colors;
		mesh.uv = uvs;
		
		int submeshCount = submeshMaterials.Count;
		mesh.subMeshCount = submeshCount;
		for (int i = 0; i < submeshCount; ++i)
			mesh.SetTriangles(submeshes[i].indexes, i);
		mesh.RecalculateBounds();

		if (newTriangles && calculateNormals) {
			Vector3[] normals = new Vector3[vertexCount];
			Vector3 normal = new Vector3(0, 0, -1);
			for (int i = 0; i < vertexCount; i++)
				normals[i] = normal;
			(useMesh1 ? mesh2 : mesh1).vertices = vertices; // Set other mesh vertices.
			mesh1.normals = normals;
			mesh2.normals = normals;

			if (calculateTangents) {
				Vector4[] tangents = new Vector4[vertexCount];
				Vector3 tangent = new Vector3(0, 0, 1);
				for (int i = 0; i < vertexCount; i++)
					tangents[i] = tangent;
				mesh1.tangents = tangents;
				mesh2.tangents = tangents;
			}
		}

		useMesh1 = !useMesh1;
	}
	
	/** Adds a material. Adds submesh indexes if existing indexes aren't sufficient. */
	private void addSubmesh (Material material, int endQuadCount, int submeshQuadCount, bool lastSubmesh) {
		int submeshIndex = submeshMaterials.Count;
		submeshMaterials.Add(material);

		int indexCount = submeshQuadCount * 6;
		int vertexIndex = (endQuadCount - submeshQuadCount) * 4;

		if (submeshes.Count <= submeshIndex) submeshes.Add(new Submesh());
		Submesh submesh = submeshes[submeshIndex];

		int[] indexes = submesh.indexes;
		if (lastSubmesh && submesh.indexCount > indexCount) {
			// Last submesh may have more indices than required, so zero indexes to the end.
			submesh.indexCount = indexCount;
			for (int i = indexCount, n = indexes.Length; i < n; i++)
				indexes[i] = 0;
		} else if (indexes.Length != indexCount) {
			// Reallocate indexes if not the right size.
			submesh.indexes = indexes = new int[indexCount];
			submesh.indexCount = 0;
		}

		// Set indexes if not already set.
		if (submesh.firstVertex != vertexIndex || submesh.indexCount < indexCount) {
			submesh.indexCount = indexCount;
			submesh.firstVertex = vertexIndex;
			for (int i = 0; i < indexCount; i += 6, vertexIndex += 4) {
				indexes[i] = vertexIndex;
				indexes[i + 1] = vertexIndex + 2;
				indexes[i + 2] = vertexIndex + 1;
				indexes[i + 3] = vertexIndex + 2;
				indexes[i + 4] = vertexIndex + 3;
				indexes[i + 5] = vertexIndex + 1;
			}
		}
	}
	
	public virtual void OnEnable () {
		Initialize();
	}

	public virtual void Reset () {
		Initialize();
	}
	
#if UNITY_EDITOR
	void OnDrawGizmos() {
		// Make selection easier by drawing a clear gizmo over the skeleton.
		if (vertices == null) return;
		Vector3 gizmosCenter = new Vector3();
		Vector3 gizmosSize = new Vector3();
		Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0f);
		Vector3 max = new Vector3(float.MinValue, float.MinValue, 0f);
		foreach (Vector3 vert in vertices) {
			min = Vector3.Min (min, vert);
			max = Vector3.Max (max, vert);
		}
		float width = max.x - min.x;
		float height = max.y - min.y;
		gizmosCenter = new Vector3(min.x + (width / 2f), min.y + (height / 2f), 0f);
		gizmosSize	= new Vector3(width, height, 1f);
		Gizmos.color = Color.clear;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawCube(gizmosCenter, gizmosSize);
	}
#endif
}

class Submesh {
	public int[] indexes = new int[0];
	public int firstVertex = -1;
	public int indexCount;
}
