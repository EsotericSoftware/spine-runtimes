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
	private Mesh mesh;
	private Vector3[] vertices;
	private Color[] colors;
	private Vector2[] uvs;
	private int[] triangles;
	private int quadCount;
	private float[] vertexPositions = new float[8];

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

		skeleton = new Skeleton(skeletonDataAsset.GetSkeletonData(false));

		if (initialSkinName != null && initialSkinName.Length > 0) {
			skeleton.SetSkin(initialSkinName);
			skeleton.SetSlotsToSetupPose();
		}
	}
	
	public virtual void UpdateSkeleton () {
		skeleton.Update(Time.deltaTime * timeScale);
		skeleton.UpdateWorldTransform();
	}
	
	public virtual void Update () {
		// Clear fields if missing information to render.
		if (skeletonDataAsset == null || skeletonDataAsset.GetSkeletonData(false) == null) {
			Clear();
			return;
		}
		
		// Initialize fields.
		if (skeleton == null || skeleton.Data != skeletonDataAsset.GetSkeletonData(false))
			Initialize();

		UpdateSkeleton();

		// Count quads.
		int quadCount = 0;
		List<Slot> drawOrder = skeleton.DrawOrder;
		for (int i = 0, n = drawOrder.Count; i < n; i++) {
			Slot slot = drawOrder[i];
			Attachment attachment = slot.Attachment;
			if (attachment is RegionAttachment)
				quadCount++;
		}

		// Ensure mesh data is the right size.
		if (quadCount != this.quadCount) {
			this.quadCount = quadCount;
			vertices = new Vector3[quadCount * 4];
			colors = new Color[quadCount * 4];
			uvs = new Vector2[quadCount * 4];
			triangles = new int[quadCount * 6];
			mesh.Clear();
		}

		// Setup mesh.
		float[] vertexPositions = this.vertexPositions;
		int quadIndex = 0;
		Color color = new Color();
		for (int i = 0, n = drawOrder.Count; i < n; i++) {
			Slot slot = drawOrder[i];
			Attachment attachment = slot.Attachment;
			if (attachment is RegionAttachment) {
				RegionAttachment regionAttachment = (RegionAttachment)attachment;
				
				regionAttachment.ComputeVertices(slot.Bone, vertexPositions);
				int vertexIndex = quadIndex * 4;
				vertices[vertexIndex] = new Vector3(vertexPositions[RegionAttachment.X1], vertexPositions[RegionAttachment.Y1], 0);
				vertices[vertexIndex + 1] = new Vector3(vertexPositions[RegionAttachment.X4], vertexPositions[RegionAttachment.Y4], 0);
				vertices[vertexIndex + 2] = new Vector3(vertexPositions[RegionAttachment.X2], vertexPositions[RegionAttachment.Y2], 0);
				vertices[vertexIndex + 3] = new Vector3(vertexPositions[RegionAttachment.X3], vertexPositions[RegionAttachment.Y3], 0);
				
				color.a = skeleton.A * slot.A;
				color.r = skeleton.R * slot.R * color.a;
				color.g = skeleton.G * slot.G * color.a;
				color.b = skeleton.B * slot.B * color.a;
				colors[vertexIndex] = color;
				colors[vertexIndex + 1] = color;
				colors[vertexIndex + 2] = color;
				colors[vertexIndex + 3] = color;

				float[] regionUVs = regionAttachment.UVs;
				uvs[vertexIndex] = new Vector2(regionUVs[RegionAttachment.X1], 1 - regionUVs[RegionAttachment.Y1]);
				uvs[vertexIndex + 1] = new Vector2(regionUVs[RegionAttachment.X4], 1 - regionUVs[RegionAttachment.Y4]);
				uvs[vertexIndex + 2] = new Vector2(regionUVs[RegionAttachment.X2], 1 - regionUVs[RegionAttachment.Y2]);
				uvs[vertexIndex + 3] = new Vector2(regionUVs[RegionAttachment.X3], 1 - regionUVs[RegionAttachment.Y3]);

				int index = quadIndex * 6;
				triangles[index] = vertexIndex;
				triangles[index + 1] = vertexIndex + 2;
				triangles[index + 2] = vertexIndex + 1;
				triangles[index + 3] = vertexIndex + 2;
				triangles[index + 4] = vertexIndex + 3;
				triangles[index + 5] = vertexIndex + 1;

				quadIndex++;
			}
		}
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		renderer.sharedMaterial = skeletonDataAsset.atlasAsset.material;
	}

	public virtual void OnEnable () {
		Update();
	}

	public virtual void OnDisable () {
		if (Application.isEditor)
			Clear();
	}

	public virtual void Reset () {
		Update();
	}
}
