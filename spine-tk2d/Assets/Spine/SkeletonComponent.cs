/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
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
 *****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

/** Renders a skeleton. Extend to apply animations, get bones and manipulate them, etc. */
[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkeletonComponent : MonoBehaviour {
	public SkeletonDataAsset skeletonDataAsset;
	public Skeleton skeleton;
	public String initialSkinName;
	public float timeScale = 1;
	public bool calculateNormals;
	public bool calculateTangents;
	private Mesh mesh;
	private float[] vertexPositions = new float[8];
	private int lastVertexCount;
	private Vector3[] vertices;
	private Color32[] colors;
	private Vector2[] uvs;
	private Material[] sharedMaterials = new Material[0];
	private List<Material> submeshMaterials = new List<Material>();
	private List<Submesh> submeshes = new List<Submesh>();
	private Vector4[] tangents = new Vector4[0];

	public virtual void Clear () {
		GetComponent<MeshFilter>().mesh = null;
		DestroyImmediate(mesh);
		mesh = null;
		renderer.sharedMaterial = null;
		skeleton = null;
	}

	public virtual void Initialize () {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.name = "Skeleton Mesh";
		mesh.hideFlags = HideFlags.HideAndDontSave;
		mesh.MarkDynamic();

		vertices = new Vector3[0];

		skeleton = new Skeleton(skeletonDataAsset.GetSkeletonData(false));

		if (initialSkinName != null && initialSkinName.Length > 0 && initialSkinName != "default") {
			skeleton.SetSkin(initialSkinName);
			skeleton.SetSlotsToSetupPose();
		}
	}
	
	public virtual void UpdateSkeleton () {
		skeleton.Update(Time.deltaTime * timeScale);
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

		UpdateSkeleton();

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
			Material material = (Material)regionAttachment.RendererObject;
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

		// Ensure mesh data is the right size.
		Mesh mesh = this.mesh;
		Vector3[] vertices = this.vertices;
		int vertexCount = quadCount * 4;
		bool newTriangles = vertexCount > vertices.Length;
		if (newTriangles) {
			// Not enough vertices, increase size.
			this.vertices = vertices = new Vector3[vertexCount];
			this.colors = new Color32[vertexCount];
			this.uvs = new Vector2[vertexCount];
			mesh.Clear();
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
		float a = skeleton.A * 255, r = skeleton.R, g = skeleton.G, b = skeleton.B;
		for (int i = 0, n = drawOrder.Count; i < n; i++) {
			Slot slot = drawOrder[i];
			RegionAttachment regionAttachment = slot.Attachment as RegionAttachment;
			if (regionAttachment == null)
				continue;
			
			regionAttachment.ComputeWorldVertices(skeleton.X, skeleton.Y, slot.Bone, vertexPositions);
			
			vertices[vertexIndex] = new Vector3(vertexPositions[RegionAttachment.X1], vertexPositions[RegionAttachment.Y1], 0);
			vertices[vertexIndex + 1] = new Vector3(vertexPositions[RegionAttachment.X4], vertexPositions[RegionAttachment.Y4], 0);
			vertices[vertexIndex + 2] = new Vector3(vertexPositions[RegionAttachment.X2], vertexPositions[RegionAttachment.Y2], 0);
			vertices[vertexIndex + 3] = new Vector3(vertexPositions[RegionAttachment.X3], vertexPositions[RegionAttachment.Y3], 0);
			
			color.a = (byte)(a * slot.A);
			color.r = (byte)(r * slot.R * color.a);
			color.g = (byte)(g * slot.G * color.a);
			color.b = (byte)(b * slot.B * color.a);
			colors[vertexIndex] = color;
			colors[vertexIndex + 1] = color;
			colors[vertexIndex + 2] = color;
			colors[vertexIndex + 3] = color;

			float[] regionUVs = regionAttachment.UVs;
			uvs[vertexIndex] = new Vector2(regionUVs[RegionAttachment.X1], regionUVs[RegionAttachment.Y1]);
			uvs[vertexIndex + 1] = new Vector2(regionUVs[RegionAttachment.X4], regionUVs[RegionAttachment.Y4]);
			uvs[vertexIndex + 2] = new Vector2(regionUVs[RegionAttachment.X2], regionUVs[RegionAttachment.Y2]);
			uvs[vertexIndex + 3] = new Vector2(regionUVs[RegionAttachment.X3], regionUVs[RegionAttachment.Y3]);

			vertexIndex += 4;
		}
		mesh.vertices = vertices;
		mesh.colors32 = colors;
		mesh.uv = uvs;
		
		int submeshCount = submeshes.Count;
		mesh.subMeshCount = submeshCount;
		for (int i = 0; i < submeshCount; ++i)
			mesh.SetTriangles(submeshes[i].indexes, i);

		if (calculateNormals) {
			mesh.RecalculateNormals();
			if (calculateTangents) {
				Vector4[] tangents = this.tangents;
				int count = mesh.normals.Length;
				if (tangents.Length != count) {
					this.tangents = tangents = new Vector4[count];
					for (int i = 0; i < count; i++)
						tangents[i] = new Vector4(1, 0, 0, 1);
				}
				mesh.tangents = tangents;
			}
		}
	}
	
	/** Adds a material. Adds submesh indexes if existing indexes aren't sufficient. */
	private void addSubmesh (Material material, int endQuadCount, int submeshQuadCount, bool lastSubmesh) {
		int submeshIndex = submeshMaterials.Count;
		submeshMaterials.Add(material);

		int indexCount = submeshQuadCount * 6;
		int vertexIndex = (endQuadCount - submeshQuadCount) * 4;
		
		if (submeshes.Count <= submeshIndex) submeshes.Add(new Submesh());
		Submesh submesh = submeshes[submeshIndex];
		
		// Allocate indexes if not the right size, allowing last submesh to have more than required.
		int[] indexes = submesh.indexes;
		if (lastSubmesh ? (indexes.Length < indexCount) : (indexes.Length != indexCount)) {
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

		// Last submesh may have more indices than required, so zero indexes to the end.
		if (lastSubmesh && submesh.indexCount != indexCount) {
			submesh.indexCount = indexCount;
			for (int i = indexCount, n = indexes.Length; i < n; i++)
				indexes[i] = 0;
		}
	}
	
	public virtual void OnEnable () {
		Update();
	}

	public virtual void Reset () {
		Update();
	}
	
#if UNITY_EDITOR
	void OnDrawGizmos() {
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
