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
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

/// <summary>Renders a skeleton.</summary>
[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkeletonRenderer : MonoBehaviour {

	public delegate void SkeletonRendererDelegate (SkeletonRenderer skeletonRenderer);

	public SkeletonRendererDelegate OnReset;
	[System.NonSerialized]
	public bool valid;
	[System.NonSerialized]
	public Skeleton skeleton;
	public SkeletonDataAsset skeletonDataAsset;
	public String initialSkinName;
	public bool calculateNormals, calculateTangents;
	public float zSpacing;
	public bool renderMeshes = true, immutableTriangles;
	public bool frontFacing;
	public bool logErrors = false;

	[SpineSlot]
	public string[] submeshSeparators = new string[0];

	[HideInInspector]
	public List<Slot> submeshSeparatorSlots = new List<Slot>();

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;
	private Mesh mesh1, mesh2;
	private bool useMesh1;
	private float[] tempVertices = new float[8];
	private int lastVertexCount;
	private Vector3[] vertices;
	private Color32[] colors;
	private Vector2[] uvs;
	private Material[] sharedMaterials = new Material[0];
	private readonly List<Material> submeshMaterials = new List<Material>();
	private readonly List<Submesh> submeshes = new List<Submesh>();
	private SkeletonUtilitySubmeshRenderer[] submeshRenderers;

	public virtual void Reset () {
		if (meshFilter != null)
			meshFilter.sharedMesh = null;

		meshRenderer = GetComponent<MeshRenderer>();
		if (meshRenderer != null) meshRenderer.sharedMaterial = null;

		if (mesh1 != null) {
			if (Application.isPlaying)
				Destroy(mesh1);
			else
				DestroyImmediate(mesh1);
		}

		if (mesh2 != null) {
			if (Application.isPlaying)
				Destroy(mesh2);
			else
				DestroyImmediate(mesh2);
		}

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

		valid = false;
		if (!skeletonDataAsset) {
			if (logErrors)
				Debug.LogError("Missing SkeletonData asset.", this);

			return;
		}
		SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
		if (skeletonData == null)
			return;
		valid = true;

		meshFilter = GetComponent<MeshFilter>();
		mesh1 = newMesh();
		mesh2 = newMesh();
		vertices = new Vector3[0];

		skeleton = new Skeleton(skeletonData);
		if (initialSkinName != null && initialSkinName.Length > 0 && initialSkinName != "default")
			skeleton.SetSkin(initialSkinName);

		submeshSeparatorSlots.Clear();
		for (int i = 0; i < submeshSeparators.Length; i++) {
			submeshSeparatorSlots.Add(skeleton.FindSlot(submeshSeparators[i]));
		}

		CollectSubmeshRenderers();

		LateUpdate();

		if (OnReset != null)
			OnReset(this);
	}

	public void CollectSubmeshRenderers () {
		submeshRenderers = GetComponentsInChildren<SkeletonUtilitySubmeshRenderer>();
	}

	public virtual void Awake () {
		Reset();
	}

	public virtual void OnDestroy () {
		if (mesh1 != null) {
			if (Application.isPlaying)
				Destroy(mesh1);
			else
				DestroyImmediate(mesh1);
		}

		if (mesh2 != null) {
			if (Application.isPlaying)
				Destroy(mesh2);
			else
				DestroyImmediate(mesh2);
		}

		mesh1 = null;
		mesh2 = null;
	}

	private Mesh newMesh () {
		Mesh mesh = new Mesh();
		mesh.name = "Skeleton Mesh";
		mesh.hideFlags = HideFlags.HideAndDontSave;
		mesh.MarkDynamic();
		return mesh;
	}

	public virtual void LateUpdate () {
		if (!valid)
			return;
		// Count vertices and submesh triangles.
		int vertexCount = 0;
		int submeshTriangleCount = 0, submeshFirstVertex = 0, submeshStartSlotIndex = 0;
		Material lastMaterial = null;
		submeshMaterials.Clear();
		List<Slot> drawOrder = skeleton.DrawOrder;
		int drawOrderCount = drawOrder.Count;
		bool renderMeshes = this.renderMeshes;
		for (int i = 0; i < drawOrderCount; i++) {
			Slot slot = drawOrder[i];
			Attachment attachment = slot.attachment;

			object rendererObject;
			int attachmentVertexCount, attachmentTriangleCount;

			if (attachment is RegionAttachment) {
				rendererObject = ((RegionAttachment)attachment).RendererObject;
				attachmentVertexCount = 4;
				attachmentTriangleCount = 6;
			} else {
				if (!renderMeshes)
					continue;
				if (attachment is MeshAttachment) {
					MeshAttachment meshAttachment = (MeshAttachment)attachment;
					rendererObject = meshAttachment.RendererObject;
					attachmentVertexCount = meshAttachment.vertices.Length >> 1;
					attachmentTriangleCount = meshAttachment.triangles.Length;
				} else if (attachment is SkinnedMeshAttachment) {
					SkinnedMeshAttachment meshAttachment = (SkinnedMeshAttachment)attachment;
					rendererObject = meshAttachment.RendererObject;
					attachmentVertexCount = meshAttachment.uvs.Length >> 1;
					attachmentTriangleCount = meshAttachment.triangles.Length;
				} else
					continue;
			}

			// Populate submesh when material changes.
#if !SPINE_TK2D
			Material material = (Material)((AtlasRegion)rendererObject).page.rendererObject;
#else
			Material material = (rendererObject.GetType() == typeof(Material)) ? (Material)rendererObject : (Material)((AtlasRegion)rendererObject).page.rendererObject;
#endif

			if ((lastMaterial != material && lastMaterial != null) || submeshSeparatorSlots.Contains(slot)) {
				AddSubmesh(lastMaterial, submeshStartSlotIndex, i, submeshTriangleCount, submeshFirstVertex, false);
				submeshTriangleCount = 0;
				submeshFirstVertex = vertexCount;
				submeshStartSlotIndex = i;
			}
			lastMaterial = material;

			submeshTriangleCount += attachmentTriangleCount;
			vertexCount += attachmentVertexCount;
		}
		AddSubmesh(lastMaterial, submeshStartSlotIndex, drawOrderCount, submeshTriangleCount, submeshFirstVertex, true);

		// Set materials.
		if (submeshMaterials.Count == sharedMaterials.Length)
			submeshMaterials.CopyTo(sharedMaterials);
		else
			sharedMaterials = submeshMaterials.ToArray();
		meshRenderer.sharedMaterials = sharedMaterials;

		// Ensure mesh data is the right size.
		Vector3[] vertices = this.vertices;
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
		float[] tempVertices = this.tempVertices;
		Vector2[] uvs = this.uvs;
		Color32[] colors = this.colors;
		int vertexIndex = 0;
		Color32 color = new Color32();
		float zSpacing = this.zSpacing;
		float a = skeleton.a * 255, r = skeleton.r, g = skeleton.g, b = skeleton.b;
		for (int i = 0; i < drawOrderCount; i++) {
			Slot slot = drawOrder[i];
			Attachment attachment = slot.attachment;
			if (attachment is RegionAttachment) {
				RegionAttachment regionAttachment = (RegionAttachment)attachment;
				regionAttachment.ComputeWorldVertices(slot.bone, tempVertices);

				float z = i * zSpacing;
				vertices[vertexIndex] = new Vector3(tempVertices[RegionAttachment.X1], tempVertices[RegionAttachment.Y1], z);
				vertices[vertexIndex + 1] = new Vector3(tempVertices[RegionAttachment.X4], tempVertices[RegionAttachment.Y4], z);
				vertices[vertexIndex + 2] = new Vector3(tempVertices[RegionAttachment.X2], tempVertices[RegionAttachment.Y2], z);
				vertices[vertexIndex + 3] = new Vector3(tempVertices[RegionAttachment.X3], tempVertices[RegionAttachment.Y3], z);

				color.a = (byte)(a * slot.a * regionAttachment.a);
				color.r = (byte)(r * slot.r * regionAttachment.r * color.a);
				color.g = (byte)(g * slot.g * regionAttachment.g * color.a);
				color.b = (byte)(b * slot.b * regionAttachment.b * color.a);
				if (slot.data.blendMode == BlendMode.additive) color.a = 0;
				colors[vertexIndex] = color;
				colors[vertexIndex + 1] = color;
				colors[vertexIndex + 2] = color;
				colors[vertexIndex + 3] = color;

				float[] regionUVs = regionAttachment.uvs;
				uvs[vertexIndex] = new Vector2(regionUVs[RegionAttachment.X1], regionUVs[RegionAttachment.Y1]);
				uvs[vertexIndex + 1] = new Vector2(regionUVs[RegionAttachment.X4], regionUVs[RegionAttachment.Y4]);
				uvs[vertexIndex + 2] = new Vector2(regionUVs[RegionAttachment.X2], regionUVs[RegionAttachment.Y2]);
				uvs[vertexIndex + 3] = new Vector2(regionUVs[RegionAttachment.X3], regionUVs[RegionAttachment.Y3]);

				vertexIndex += 4;
			} else {
				if (!renderMeshes)
					continue;
				if (attachment is MeshAttachment) {
					MeshAttachment meshAttachment = (MeshAttachment)attachment;
					int meshVertexCount = meshAttachment.vertices.Length;
					if (tempVertices.Length < meshVertexCount)
						this.tempVertices = tempVertices = new float[meshVertexCount];
					meshAttachment.ComputeWorldVertices(slot, tempVertices);

					color.a = (byte)(a * slot.a * meshAttachment.a);
					color.r = (byte)(r * slot.r * meshAttachment.r * color.a);
					color.g = (byte)(g * slot.g * meshAttachment.g * color.a);
					color.b = (byte)(b * slot.b * meshAttachment.b * color.a);
					if (slot.data.blendMode == BlendMode.additive) color.a = 0;

					float[] meshUVs = meshAttachment.uvs;
					float z = i * zSpacing;
					for (int ii = 0; ii < meshVertexCount; ii += 2, vertexIndex++) {
						vertices[vertexIndex] = new Vector3(tempVertices[ii], tempVertices[ii + 1], z);
						colors[vertexIndex] = color;
						uvs[vertexIndex] = new Vector2(meshUVs[ii], meshUVs[ii + 1]);
					}
				} else if (attachment is SkinnedMeshAttachment) {
					SkinnedMeshAttachment meshAttachment = (SkinnedMeshAttachment)attachment;
					int meshVertexCount = meshAttachment.uvs.Length;
					if (tempVertices.Length < meshVertexCount)
						this.tempVertices = tempVertices = new float[meshVertexCount];
					meshAttachment.ComputeWorldVertices(slot, tempVertices);

					color.a = (byte)(a * slot.a * meshAttachment.a);
					color.r = (byte)(r * slot.r * meshAttachment.r * color.a);
					color.g = (byte)(g * slot.g * meshAttachment.g * color.a);
					color.b = (byte)(b * slot.b * meshAttachment.b * color.a);
					if (slot.data.blendMode == BlendMode.additive) color.a = 0;

					float[] meshUVs = meshAttachment.uvs;
					float z = i * zSpacing;
					for (int ii = 0; ii < meshVertexCount; ii += 2, vertexIndex++) {
						vertices[vertexIndex] = new Vector3(tempVertices[ii], tempVertices[ii + 1], z);
						colors[vertexIndex] = color;
						uvs[vertexIndex] = new Vector2(meshUVs[ii], meshUVs[ii + 1]);
					}
				}
			}
		}

		// Double buffer mesh.
		Mesh mesh = useMesh1 ? mesh1 : mesh2;
		meshFilter.sharedMesh = mesh;

		mesh.vertices = vertices;
		mesh.colors32 = colors;
		mesh.uv = uvs;

		int submeshCount = submeshMaterials.Count;
		mesh.subMeshCount = submeshCount;
		for (int i = 0; i < submeshCount; ++i)
			mesh.SetTriangles(submeshes[i].triangles, i);
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

		if (submeshRenderers.Length > 0) {
			foreach (var submeshRenderer in submeshRenderers) {
				if (submeshRenderer.submeshIndex < sharedMaterials.Length)
					submeshRenderer.SetMesh(meshRenderer, useMesh1 ? mesh1 : mesh2, sharedMaterials[submeshRenderer.submeshIndex]);
				else
					submeshRenderer.GetComponent<Renderer>().enabled = false;
			}
		}

		useMesh1 = !useMesh1;
	}

	/** Stores vertices and triangles for a single material. */
	private void AddSubmesh (Material material, int startSlot, int endSlot, int triangleCount, int firstVertex, bool lastSubmesh) {
		
		int submeshIndex = submeshMaterials.Count;
		submeshMaterials.Add(material);

		if (submeshes.Count <= submeshIndex)
			submeshes.Add(new Submesh());
		else if (immutableTriangles)
			return;

		Submesh submesh = submeshes[submeshIndex];

		int[] triangles = submesh.triangles;
		int trianglesCapacity = triangles.Length;
		if (lastSubmesh && trianglesCapacity > triangleCount) {
			// Last submesh may have more triangles than required, so zero triangles to the end.
			for (int i = triangleCount; i < trianglesCapacity; i++)
				triangles[i] = 0;
			submesh.triangleCount = triangleCount;
		} else if (trianglesCapacity != triangleCount) {
			// Reallocate triangles when not the exact size needed.
			submesh.triangles = triangles = new int[triangleCount];
			submesh.triangleCount = 0;
		}

		if (!renderMeshes && !frontFacing) {
			// Use stored triangles if possible.
			if (submesh.firstVertex != firstVertex || submesh.triangleCount < triangleCount) {
				submesh.triangleCount = triangleCount;
				submesh.firstVertex = firstVertex;
				int drawOrderIndex = 0;
				for (int i = 0; i < triangleCount; i += 6, firstVertex += 4, drawOrderIndex++) {
					triangles[i] = firstVertex;
					triangles[i + 1] = firstVertex + 2;
					triangles[i + 2] = firstVertex + 1;
					triangles[i + 3] = firstVertex + 2;
					triangles[i + 4] = firstVertex + 3;
					triangles[i + 5] = firstVertex + 1;
				}
			}
			return;
		}

		// Store triangles.
		List<Slot> drawOrder = skeleton.DrawOrder;
		for (int i = startSlot, triangleIndex = 0; i < endSlot; i++) {
			Slot slot = drawOrder[i];
			Attachment attachment = slot.attachment;
			Bone bone = slot.bone;
			bool flip = frontFacing && ((bone.WorldFlipX != bone.WorldFlipY) != (Mathf.Sign(bone.WorldScaleX) != Mathf.Sign(bone.WorldScaleY)));

			if (attachment is RegionAttachment) {
				if (!flip) {
					triangles[triangleIndex] = firstVertex;
					triangles[triangleIndex + 1] = firstVertex + 2;
					triangles[triangleIndex + 2] = firstVertex + 1;
					triangles[triangleIndex + 3] = firstVertex + 2;
					triangles[triangleIndex + 4] = firstVertex + 3;
					triangles[triangleIndex + 5] = firstVertex + 1;
				} else {
					triangles[triangleIndex] = firstVertex + 1;
					triangles[triangleIndex + 1] = firstVertex + 2;
					triangles[triangleIndex + 2] = firstVertex;
					triangles[triangleIndex + 3] = firstVertex + 1;
					triangles[triangleIndex + 4] = firstVertex + 3;
					triangles[triangleIndex + 5] = firstVertex + 2;
				}

				triangleIndex += 6;
				firstVertex += 4;
				continue;
			}
			int[] attachmentTriangles;
			int attachmentVertexCount;
			if (attachment is MeshAttachment) {
				MeshAttachment meshAttachment = (MeshAttachment)attachment;
				attachmentVertexCount = meshAttachment.vertices.Length >> 1;
				attachmentTriangles = meshAttachment.triangles;
			} else if (attachment is SkinnedMeshAttachment) {
				SkinnedMeshAttachment meshAttachment = (SkinnedMeshAttachment)attachment;
				attachmentVertexCount = meshAttachment.uvs.Length >> 1;
				attachmentTriangles = meshAttachment.triangles;
			} else
				continue;

			if (flip) {
				for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii += 3, triangleIndex += 3) {
					triangles[triangleIndex + 2] = firstVertex + attachmentTriangles[ii];
					triangles[triangleIndex + 1] = firstVertex + attachmentTriangles[ii + 1];
					triangles[triangleIndex] = firstVertex + attachmentTriangles[ii + 2];
				}
			} else {
				for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii++, triangleIndex++) {
					triangles[triangleIndex] = firstVertex + attachmentTriangles[ii];
				}
			}

			firstVertex += attachmentVertexCount;
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmos () {
		// Make selection easier by drawing a clear gizmo over the skeleton.
		if (vertices == null) return;
		Vector3 gizmosCenter = new Vector3();
		Vector3 gizmosSize = new Vector3();
		Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0f);
		Vector3 max = new Vector3(float.MinValue, float.MinValue, 0f);
		foreach (Vector3 vert in vertices) {
			min = Vector3.Min(min, vert);
			max = Vector3.Max(max, vert);
		}
		float width = max.x - min.x;
		float height = max.y - min.y;
		gizmosCenter = new Vector3(min.x + (width / 2f), min.y + (height / 2f), 0f);
		gizmosSize = new Vector3(width, height, 1f);
		Gizmos.color = Color.clear;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawCube(gizmosCenter, gizmosSize);
	}
#endif
}

class Submesh {
	public int[] triangles = new int[0];
	public int triangleCount;
	public int firstVertex = -1;
}
