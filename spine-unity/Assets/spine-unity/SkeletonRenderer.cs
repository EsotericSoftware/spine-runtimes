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
using UnityEngine;
using Spine;

/// <summary>Renders a skeleton.</summary>
[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkeletonRenderer : MonoBehaviour {

	public delegate void SkeletonRendererDelegate (SkeletonRenderer skeletonRenderer);
	public SkeletonRendererDelegate OnReset;

	public SkeletonDataAsset skeletonDataAsset;
	public String initialSkinName;

	#region Advanced
	public bool calculateNormals, calculateTangents;
	public float zSpacing;
	public bool renderMeshes = true, immutableTriangles;
	public bool frontFacing;
	public bool logErrors = false;

	// Submesh Separation
	[SpineSlot] public string[] submeshSeparators = new string[0];
	[HideInInspector] public List<Slot> submeshSeparatorSlots = new List<Slot>();
	#endregion

	[System.NonSerialized] public bool valid;
	[System.NonSerialized] public Skeleton skeleton;

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	private Mesh mesh1, mesh2;
	private bool useMesh1;

	private float[] tempVertices = new float[8];
	private Vector3[] vertices;
	private Color32[] colors;
	private Vector2[] uvs;
	private Material[] sharedMaterials = new Material[0];

	private MeshState meshState = new MeshState();
	private readonly ExposedList<Material> submeshMaterials = new ExposedList<Material>();
	private readonly ExposedList<Submesh> submeshes = new ExposedList<Submesh>();
	private SkeletonUtilitySubmeshRenderer[] submeshRenderers;

	#region Runtime Instantiation
	/// <summary>Add and prepare a Spine component that derives from SkeletonRenderer to a GameObject at runtime.</summary>
	/// <typeparam name="T">T should be SkeletonRenderer or any of its derived classes.</typeparam>
	public static T AddSpineComponent<T> (GameObject gameObject, SkeletonDataAsset skeletonDataAsset) where T : SkeletonRenderer {
		
		var c = gameObject.AddComponent<T>();

		if (skeletonDataAsset != null) {
			c.skeletonDataAsset = skeletonDataAsset;
			c.Reset(); // TODO: Method name will change.
		}

		return c;
	}

	public static T NewSpineGameObject<T> (SkeletonDataAsset skeletonDataAsset) where T : SkeletonRenderer {
		return SkeletonRenderer.AddSpineComponent<T>(new GameObject("New Spine GameObject"), skeletonDataAsset);
	}
	#endregion

	public virtual void Awake () {
		Reset();
	}

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

		meshState = new MeshState();
		mesh1 = null;
		mesh2 = null;
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
		meshRenderer = GetComponent<MeshRenderer>();
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

	private static Mesh newMesh () {
		Mesh mesh = new Mesh();
		mesh.name = "Skeleton Mesh";
		mesh.hideFlags = HideFlags.HideAndDontSave;
		mesh.MarkDynamic();
		return mesh;
	}

	public virtual void LateUpdate () {
		if (!valid)
			return;

		// Exit early if there is nothing to render
		if (!meshRenderer.enabled && submeshRenderers.Length == 0)
			return;

		// Count vertices and submesh triangles.
		int vertexCount = 0;

		int submeshTriangleCount = 0, submeshFirstVertex = 0, submeshStartSlotIndex = 0;
		Material lastMaterial = null;
		ExposedList<Slot> drawOrder = skeleton.drawOrder;
		var drawOrderItems = drawOrder.Items;
		int drawOrderCount = drawOrder.Count;
		int submeshSeparatorSlotsCount = submeshSeparatorSlots.Count;
		bool renderMeshes = this.renderMeshes;

		// Clear last state of attachments and submeshes
		MeshState.SingleMeshState workingState = meshState.buffer;
		var workingAttachments = workingState.attachments;
		workingAttachments.Clear(true);
		workingState.UpdateAttachmentCount(drawOrderCount);
		var workingAttachmentsItems = workingAttachments.Items;					// Make sure to not add to or remove from ExposedList inside the loop below

		var workingFlips = workingState.attachmentsFlipState;
		var workingFlipsItems = workingState.attachmentsFlipState.Items;		// Make sure to not add to or remove from ExposedList inside the loop below

		var workingSubmeshArguments = workingState.addSubmeshArguments;	// Items array should not be cached. There is dynamic writing to this object.
		workingSubmeshArguments.Clear(false);

		MeshState.SingleMeshState storedState = useMesh1 ? meshState.stateMesh1 : meshState.stateMesh2;
		var storedAttachments = storedState.attachments;
		var storedAttachmentsItems = storedAttachments.Items;		// Make sure to not add to or remove from ExposedList inside the loop below

		var storedFlips = storedState.attachmentsFlipState;
		var storedFlipsItems = storedFlips.Items;					// Make sure to not add to or remove from ExposedList inside the loop below

		bool mustUpdateMeshStructure = storedState.requiresUpdate ||	// Force update if the mesh was cleared. (prevents flickering due to incorrect state)
			drawOrder.Count != storedAttachments.Count ||				// Number of slots changed (when does this happen?)
			immutableTriangles != storedState.immutableTriangles;		// Immutable Triangles flag changed.

		for (int i = 0; i < drawOrderCount; i++) {
			Slot slot = drawOrderItems[i];
			Bone bone = slot.bone;
			Attachment attachment = slot.attachment;

			object rendererObject; // An AtlasRegion in plain Spine-Unity. Spine-TK2D hooks into TK2D's system. eventual source of Material object.
			int attachmentVertexCount, attachmentTriangleCount;

			// Handle flipping for normals (for lighting).
			bool worldScaleIsSameSigns = ((bone.worldScaleY >= 0f) == (bone.worldScaleX >= 0f));
			bool flip = frontFacing && ((bone.worldFlipX != bone.worldFlipY) == worldScaleIsSameSigns); // TODO: bone flipX and flipY will be removed in Spine 3.0

			workingFlipsItems[i] = flip;
			workingAttachmentsItems[i] = attachment;

			mustUpdateMeshStructure = mustUpdateMeshStructure ||	// Always prefer short circuited or. || and not |=.
				(attachment != storedAttachmentsItems[i]) || 		// Attachment order changed. // This relies on the drawOrder.Count != storedAttachments.Count check above as a bounds check.
				(flip != storedFlipsItems[i]);						// Flip states changed.

			var regionAttachment = attachment as RegionAttachment;
			if (regionAttachment != null) {
				rendererObject = regionAttachment.RendererObject;
				attachmentVertexCount = 4;
				attachmentTriangleCount = 6;
			} else {
				if (!renderMeshes)
					continue;
				var meshAttachment = attachment as MeshAttachment;
				if (meshAttachment != null) {
					rendererObject = meshAttachment.RendererObject;
					attachmentVertexCount = meshAttachment.vertices.Length >> 1;
					attachmentTriangleCount = meshAttachment.triangles.Length;
				} else {
					var skinnedMeshAttachment = attachment as SkinnedMeshAttachment;
					if (skinnedMeshAttachment != null) {
						rendererObject = skinnedMeshAttachment.RendererObject;
						attachmentVertexCount = skinnedMeshAttachment.uvs.Length >> 1;
						attachmentTriangleCount = skinnedMeshAttachment.triangles.Length;
					} else
						continue;
				}
			}

			#if !SPINE_TK2D
			Material material = (Material)((AtlasRegion)rendererObject).page.rendererObject;
			#else
			Material material = (rendererObject.GetType() == typeof(Material)) ? (Material)rendererObject : (Material)((AtlasRegion)rendererObject).page.rendererObject;
			#endif

			// Populate submesh when material changes. (or when forced to separate by a submeshSeparator)
			if ((lastMaterial != null && lastMaterial.GetInstanceID() != material.GetInstanceID()) ||
				(submeshSeparatorSlotsCount > 0 && submeshSeparatorSlots.Contains(slot))) {

				workingSubmeshArguments.Add(
					new MeshState.AddSubmeshArguments {
						material = lastMaterial,
						startSlot = submeshStartSlotIndex,
						endSlot = i,
						triangleCount = submeshTriangleCount,
						firstVertex = submeshFirstVertex,
						isLastSubmesh = false
					}
				);

				submeshTriangleCount = 0;
				submeshFirstVertex = vertexCount;
				submeshStartSlotIndex = i;
			}
			lastMaterial = material;

			submeshTriangleCount += attachmentTriangleCount;
			vertexCount += attachmentVertexCount;
		}


		workingSubmeshArguments.Add(
			new MeshState.AddSubmeshArguments {
				material = lastMaterial,
				startSlot = submeshStartSlotIndex,
				endSlot = drawOrderCount,
				triangleCount = submeshTriangleCount,
				firstVertex = submeshFirstVertex,
				isLastSubmesh = true
			}
		);

		mustUpdateMeshStructure = mustUpdateMeshStructure ||
			this.sharedMaterials.Length != workingSubmeshArguments.Count ||		// Material array changed in size
			CheckIfMustUpdateMeshStructure(workingSubmeshArguments);			// Submesh Argument Array changed.

		// CheckIfMustUpdateMaterialArray (workingMaterials, sharedMaterials)
		if (!mustUpdateMeshStructure) {
			// Narrow phase material array check.
			var workingMaterials = workingSubmeshArguments.Items;
			for (int i = 0, n = sharedMaterials.Length; i < n; i++) {
				if (this.sharedMaterials[i] != workingMaterials[i].material) {	// Bounds check is implied above.
					mustUpdateMeshStructure = true;
					break;
				}
			}
		}

		// NOT ELSE

		if (mustUpdateMeshStructure) {
			this.submeshMaterials.Clear();

			var workingSubmeshArgumentsItems = workingSubmeshArguments.Items;
			for (int i = 0, n = workingSubmeshArguments.Count; i < n; i++) {
				AddSubmesh(workingSubmeshArgumentsItems[i], workingFlips);
			}

			// Set materials.
			if (submeshMaterials.Count == sharedMaterials.Length)
				submeshMaterials.CopyTo(sharedMaterials);
			else
				sharedMaterials = submeshMaterials.ToArray();

			meshRenderer.sharedMaterials = sharedMaterials;
		}


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
			meshState.stateMesh1.requiresUpdate = true;
			meshState.stateMesh2.requiresUpdate = true;

		} else {
			// Too many vertices, zero the extra.
			Vector3 zero = Vector3.zero;
			for (int i = vertexCount, n = meshState.vertexCount; i < n; i++)
				vertices[i] = zero;
		}
		meshState.vertexCount = vertexCount;

		// Setup mesh.
		float zSpacing = this.zSpacing;
		float[] tempVertices = this.tempVertices;
		Vector2[] uvs = this.uvs;
		Color32[] colors = this.colors;
		int vertexIndex = 0;
		Color32 color;
		float a = skeleton.a * 255, r = skeleton.r, g = skeleton.g, b = skeleton.b;

		Vector3 meshBoundsMin;
		Vector3 meshBoundsMax;
		if (vertexCount == 0) {
			meshBoundsMin = new Vector3(0, 0, 0);
			meshBoundsMax = new Vector3(0, 0, 0);
		} else {
			meshBoundsMin.x = int.MaxValue;
			meshBoundsMin.y = int.MaxValue;
			meshBoundsMax.x = int.MinValue;
			meshBoundsMax.y = int.MinValue;
			if (zSpacing > 0f) {
				meshBoundsMin.z = 0f;
				meshBoundsMax.z = zSpacing * (drawOrderCount - 1);
			} else {
				meshBoundsMin.z = zSpacing * (drawOrderCount - 1);
				meshBoundsMax.z = 0f;
			}
			int i = 0;
			do {
				Slot slot = drawOrderItems[i];
				Attachment attachment = slot.attachment;
				RegionAttachment regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {
					regionAttachment.ComputeWorldVertices(slot.bone, tempVertices);

					float z = i * zSpacing;
					float x1 = tempVertices[RegionAttachment.X1], y1 = tempVertices[RegionAttachment.Y1];
					float x2 = tempVertices[RegionAttachment.X2], y2 = tempVertices[RegionAttachment.Y2];
					float x3 = tempVertices[RegionAttachment.X3], y3 = tempVertices[RegionAttachment.Y3];
					float x4 = tempVertices[RegionAttachment.X4], y4 = tempVertices[RegionAttachment.Y4];
					vertices[vertexIndex].x = x1;
					vertices[vertexIndex].y = y1;
					vertices[vertexIndex].z = z;
					vertices[vertexIndex + 1].x = x4;
					vertices[vertexIndex + 1].y = y4;
					vertices[vertexIndex + 1].z = z;
					vertices[vertexIndex + 2].x = x2;
					vertices[vertexIndex + 2].y = y2;
					vertices[vertexIndex + 2].z = z;
					vertices[vertexIndex + 3].x = x3;
					vertices[vertexIndex + 3].y = y3;
					vertices[vertexIndex + 3].z = z;

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
					uvs[vertexIndex].x = regionUVs[RegionAttachment.X1];
					uvs[vertexIndex].y = regionUVs[RegionAttachment.Y1];
					uvs[vertexIndex + 1].x = regionUVs[RegionAttachment.X4];
					uvs[vertexIndex + 1].y = regionUVs[RegionAttachment.Y4];
					uvs[vertexIndex + 2].x = regionUVs[RegionAttachment.X2];
					uvs[vertexIndex + 2].y = regionUVs[RegionAttachment.Y2];
					uvs[vertexIndex + 3].x = regionUVs[RegionAttachment.X3];
					uvs[vertexIndex + 3].y = regionUVs[RegionAttachment.Y3];

					// Calculate min/max X
					if (x1 < meshBoundsMin.x)
						meshBoundsMin.x = x1;
					else if (x1 > meshBoundsMax.x)
						meshBoundsMax.x = x1;
					if (x2 < meshBoundsMin.x)
						meshBoundsMin.x = x2;
					else if (x2 > meshBoundsMax.x)
						meshBoundsMax.x = x2;
					if (x3 < meshBoundsMin.x)
						meshBoundsMin.x = x3;
					else if (x3 > meshBoundsMax.x)
						meshBoundsMax.x = x3;
					if (x4 < meshBoundsMin.x)
						meshBoundsMin.x = x4;
					else if (x4 > meshBoundsMax.x)
						meshBoundsMax.x = x4;

					// Calculate min/max Y
					if (y1 < meshBoundsMin.y)
						meshBoundsMin.y = y1;
					else if (y1 > meshBoundsMax.y)
						meshBoundsMax.y = y1;
					if (y2 < meshBoundsMin.y)
						meshBoundsMin.y = y2;
					else if (y2 > meshBoundsMax.y)
						meshBoundsMax.y = y2;
					if (y3 < meshBoundsMin.y)
						meshBoundsMin.y = y3;
					else if (y3 > meshBoundsMax.y)
						meshBoundsMax.y = y3;
					if (y4 < meshBoundsMin.y)
						meshBoundsMin.y = y4;
					else if (y4 > meshBoundsMax.y)
						meshBoundsMax.y = y4;

					vertexIndex += 4;
				} else {
					if (!renderMeshes)
						continue;
					MeshAttachment meshAttachment = attachment as MeshAttachment;
					if (meshAttachment != null) {
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
							float x = tempVertices[ii], y = tempVertices[ii + 1];
							vertices[vertexIndex].x = x;
							vertices[vertexIndex].y = y;
							vertices[vertexIndex].z = z;
							colors[vertexIndex] = color;
							uvs[vertexIndex].x = meshUVs[ii];
							uvs[vertexIndex].y = meshUVs[ii + 1];

							if (x < meshBoundsMin.x)
								meshBoundsMin.x = x;
							else if (x > meshBoundsMax.x)
								meshBoundsMax.x = x;
							if (y < meshBoundsMin.y)
								meshBoundsMin.y = y;
							else if (y > meshBoundsMax.y)
								meshBoundsMax.y = y;
						}
					} else {
						SkinnedMeshAttachment skinnedMeshAttachment = attachment as SkinnedMeshAttachment;
						if (skinnedMeshAttachment != null) {
							int meshVertexCount = skinnedMeshAttachment.uvs.Length;
							if (tempVertices.Length < meshVertexCount)
								this.tempVertices = tempVertices = new float[meshVertexCount];
							skinnedMeshAttachment.ComputeWorldVertices(slot, tempVertices);

							color.a = (byte)(a * slot.a * skinnedMeshAttachment.a);
							color.r = (byte)(r * slot.r * skinnedMeshAttachment.r * color.a);
							color.g = (byte)(g * slot.g * skinnedMeshAttachment.g * color.a);
							color.b = (byte)(b * slot.b * skinnedMeshAttachment.b * color.a);
							if (slot.data.blendMode == BlendMode.additive) color.a = 0;

							float[] meshUVs = skinnedMeshAttachment.uvs;
							float z = i * zSpacing;
							for (int ii = 0; ii < meshVertexCount; ii += 2, vertexIndex++) {
								float x = tempVertices[ii], y = tempVertices[ii + 1];
								vertices[vertexIndex].x = x;
								vertices[vertexIndex].y = y;
								vertices[vertexIndex].z = z;
								colors[vertexIndex] = color;
								uvs[vertexIndex].x = meshUVs[ii];
								uvs[vertexIndex].y = meshUVs[ii + 1];

								if (x < meshBoundsMin.x)
									meshBoundsMin.x = x;
								else if (x > meshBoundsMax.x)
									meshBoundsMax.x = x;
								if (y < meshBoundsMin.y)
									meshBoundsMin.y = y;
								else if (y > meshBoundsMax.y)
									meshBoundsMax.y = y;
							}
						}
					}
				}
			} while (++i < drawOrderCount);
		}

		// Double buffer mesh.
		Mesh mesh = useMesh1 ? mesh1 : mesh2;
		meshFilter.sharedMesh = mesh;

		mesh.vertices = vertices;
		mesh.colors32 = colors;
		mesh.uv = uvs;

		if (mustUpdateMeshStructure) {
			int submeshCount = submeshMaterials.Count;
			mesh.subMeshCount = submeshCount;
			for (int i = 0; i < submeshCount; ++i)
				mesh.SetTriangles(submeshes.Items[i].triangles, i);

			// Done updating mesh.
			storedState.requiresUpdate = false;
		}

		Vector3 meshBoundsExtents = meshBoundsMax - meshBoundsMin;
		Vector3 meshBoundsCenter = meshBoundsMin + meshBoundsExtents * 0.5f;
		mesh.bounds = new Bounds(meshBoundsCenter, meshBoundsExtents);

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
			
		// Update previous state
		storedState.immutableTriangles = immutableTriangles;

		storedAttachments.Clear(true);
		storedAttachments.GrowIfNeeded(workingAttachments.Capacity);
		storedAttachments.Count = workingAttachments.Count;
		workingAttachments.CopyTo(storedAttachments.Items);

		storedFlips.GrowIfNeeded(workingFlips.Capacity);
		storedFlips.Count = workingFlips.Count;
		workingFlips.CopyTo(storedFlips.Items);

		storedState.addSubmeshArguments.GrowIfNeeded(workingSubmeshArguments.Capacity);
		storedState.addSubmeshArguments.Count = workingSubmeshArguments.Count;
		workingSubmeshArguments.CopyTo(storedState.addSubmeshArguments.Items);


		// Submesh Renderers
		if (submeshRenderers.Length > 0) {
			for (int i = 0; i < submeshRenderers.Length; i++) {
				SkeletonUtilitySubmeshRenderer submeshRenderer = submeshRenderers[i];
				if (submeshRenderer.submeshIndex < sharedMaterials.Length) {
					submeshRenderer.SetMesh(meshRenderer, useMesh1 ? mesh1 : mesh2, sharedMaterials[submeshRenderer.submeshIndex]);
				} else {
					submeshRenderer.GetComponent<Renderer>().enabled = false;
				}
			}
		}

		useMesh1 = !useMesh1;
	}

	private bool CheckIfMustUpdateMeshStructure (ExposedList<MeshState.AddSubmeshArguments> workingAddSubmeshArguments) {
		#if UNITY_EDITOR
		if (!Application.isPlaying)
			return true;
		#endif

		// Check if any mesh settings were changed
		MeshState.SingleMeshState currentMeshState = useMesh1 ? meshState.stateMesh1 : meshState.stateMesh2;

		// Check if submesh structures has changed
		ExposedList<MeshState.AddSubmeshArguments> addSubmeshArgumentsCurrentMesh = currentMeshState.addSubmeshArguments;
		int submeshCount = workingAddSubmeshArguments.Count;
		if (addSubmeshArgumentsCurrentMesh.Count != submeshCount)
			return true;

		for (int i = 0; i < submeshCount; i++) {
			if (!addSubmeshArgumentsCurrentMesh.Items[i].Equals(ref workingAddSubmeshArguments.Items[i]))
				return true;
		}

		return false;
	}

	private void AddSubmesh (MeshState.AddSubmeshArguments submeshArguments, ExposedList<bool> flipStates) { //submeshArguments is a struct, so it's ok.
		int submeshIndex = submeshMaterials.Count;
		submeshMaterials.Add(submeshArguments.material);

		if (submeshes.Count <= submeshIndex)
			submeshes.Add(new Submesh());
		else if (immutableTriangles)
			return;

		Submesh currentSubmesh = submeshes.Items[submeshIndex];
		int[] triangles = currentSubmesh.triangles;

		int triangleCount = submeshArguments.triangleCount;
		int firstVertex = submeshArguments.firstVertex;

		int trianglesCapacity = triangles.Length;
		if (submeshArguments.isLastSubmesh && trianglesCapacity > triangleCount) {
			// Last submesh may have more triangles than required, so zero triangles to the end.
			for (int i = triangleCount; i < trianglesCapacity; i++) {
				triangles[i] = 0;
			}
			currentSubmesh.triangleCount = triangleCount;

		} else if (trianglesCapacity != triangleCount) {
			// Reallocate triangles when not the exact size needed.
			currentSubmesh.triangles = triangles = new int[triangleCount];
			currentSubmesh.triangleCount = 0;
		}

		if (!this.renderMeshes && !this.frontFacing) {
			// Use stored triangles if possible.
			if (currentSubmesh.firstVertex != firstVertex || currentSubmesh.triangleCount < triangleCount) { //|| currentSubmesh.triangleCount == 0
				currentSubmesh.triangleCount = triangleCount;
				currentSubmesh.firstVertex = firstVertex;

				for (int i = 0; i < triangleCount; i += 6, firstVertex += 4) {
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

		// Iterate through all slots and store their triangles. 

		var drawOrderItems = skeleton.DrawOrder.Items;	// Make sure to not modify ExposedList inside the loop below
		var flipStatesItems = flipStates.Items;			// Make sure to not modify ExposedList inside the loop below

		int triangleIndex = 0; // Modified by loop
		for (int i = submeshArguments.startSlot, n = submeshArguments.endSlot; i < n; i++) {			
			Attachment attachment = drawOrderItems[i].attachment;

			bool flip = flipStatesItems[i];

			// Add RegionAttachment triangles
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

			// Add (Skinned)MeshAttachment triangles
			int[] attachmentTriangles;
			int attachmentVertexCount;
			var meshAttachment = attachment as MeshAttachment;
			if (meshAttachment != null) {
				attachmentVertexCount = meshAttachment.vertices.Length >> 1; //  length/2
				attachmentTriangles = meshAttachment.triangles;
			} else {
				var skinnedMeshAttachment = attachment as SkinnedMeshAttachment;
				if (skinnedMeshAttachment != null) {
					attachmentVertexCount = skinnedMeshAttachment.uvs.Length >> 1; // length/2
					attachmentTriangles = skinnedMeshAttachment.triangles;
				} else
					continue;
			}

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
		meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null) return;

		Mesh mesh = meshFilter.sharedMesh;
		if (mesh == null) return;

		Bounds meshBounds = mesh.bounds;
		Gizmos.color = Color.clear;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawCube(meshBounds.center, meshBounds.size);
	}
	#endif

	private class MeshState {
		public int vertexCount;
		public readonly SingleMeshState buffer = new SingleMeshState();
		public readonly SingleMeshState stateMesh1 = new SingleMeshState();
		public readonly SingleMeshState stateMesh2 = new SingleMeshState();

		public class SingleMeshState {
			public bool immutableTriangles;
			public bool requiresUpdate;
			public readonly ExposedList<Attachment> attachments = new ExposedList<Attachment>();
			public readonly ExposedList<bool> attachmentsFlipState = new ExposedList<bool>();
			public readonly ExposedList<AddSubmeshArguments> addSubmeshArguments = new ExposedList<AddSubmeshArguments>();

			public void UpdateAttachmentCount (int attachmentCount) {
				attachmentsFlipState.GrowIfNeeded(attachmentCount);
				attachmentsFlipState.Count = attachmentCount;

				attachments.GrowIfNeeded(attachmentCount);
				attachments.Count = attachmentCount;
			}
		}

		public struct AddSubmeshArguments {
			public Material material;
			public int startSlot;
			public int endSlot;
			public int triangleCount;
			public int firstVertex;
			public bool isLastSubmesh;

			public bool Equals (ref AddSubmeshArguments other) {
				return
					//!ReferenceEquals(material, null) &&
					//!ReferenceEquals(other.material, null) &&
					//material.GetInstanceID() == other.material.GetInstanceID() &&
					startSlot == other.startSlot &&
					endSlot == other.endSlot &&
					triangleCount == other.triangleCount &&
					firstVertex == other.firstVertex;
			}
		}
	}
}

class Submesh {
	public int[] triangles = new int[0];
	public int triangleCount;
	public int firstVertex = -1;
}
