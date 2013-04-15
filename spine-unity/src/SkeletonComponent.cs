using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkeletonComponent : MonoBehaviour {
	public SkeletonDataAsset skeletonDataAsset;
	public Skeleton skeleton;
	public String animationName;
	public bool loop;
	public float timeScale = 1;
	public Spine.AnimationState state;
	private Mesh mesh;
	private Vector3[] vertices;
	private Vector2[] uvs;
	private int[] triangles;
	private int quadCount;

	public void Clear () {
		GetComponent<MeshFilter>().mesh = null;
		DestroyImmediate(mesh);
		mesh = null;
		renderer.sharedMaterial = null;
		skeleton = null;
	}

	public void Initialize () {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.name = "Skeleton Mesh";
		mesh.hideFlags = HideFlags.HideAndDontSave;

		state = new Spine.AnimationState(skeletonDataAsset.GetAnimationStateData());

		skeleton = new Skeleton(skeletonDataAsset.GetSkeletonData(false));
	}
	public void Update () {
		// Clear fields if missing information to render.
		if (skeletonDataAsset == null || skeletonDataAsset.GetSkeletonData(false) == null) {
			Clear();
			return;
		}

		// Initialize fields.
		if (skeleton == null || skeleton.Data != skeletonDataAsset.GetSkeletonData(false))
			Initialize();

		// Keep AnimationState in sync with animationName and loop fields.
		if (animationName == null && state.Animation != null)
			state.Clear();
		else if (state.Animation == null || animationName != state.Animation.Name) {
			Spine.Animation animation = skeleton.Data.FindAnimation(animationName);
			if (animation != null)
				state.SetAnimation(animation, loop);
		}
		state.Loop = loop;

		// Apply animation.
		state.Update(Time.deltaTime * timeScale);
		state.Apply(skeleton);
		skeleton.UpdateWorldTransform();

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
			uvs = new Vector2[quadCount * 4];
			triangles = new int[quadCount * 6];
		}

		// Setup mesh.
		int quadIndex = 0;
		for (int i = 0, n = drawOrder.Count; i < n; i++) {
			Slot slot = drawOrder[i];
			Attachment attachment = slot.Attachment;
			if (attachment is RegionAttachment) {
				RegionAttachment regionAttachment = (RegionAttachment)attachment;
				
				regionAttachment.UpdateVertices(slot.Bone);
				float[] regionVertices = regionAttachment.Vertices;
				int vertexIndex = quadIndex * 4;
				vertices[vertexIndex] = new Vector3(regionVertices[RegionAttachment.X1], regionVertices[RegionAttachment.Y1], 0);
				vertices[vertexIndex + 1] = new Vector3(regionVertices[RegionAttachment.X4], regionVertices[RegionAttachment.Y4], 0);
				vertices[vertexIndex + 2] = new Vector3(regionVertices[RegionAttachment.X2], regionVertices[RegionAttachment.Y2], 0);
				vertices[vertexIndex + 3] = new Vector3(regionVertices[RegionAttachment.X3], regionVertices[RegionAttachment.Y3], 0);

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
		mesh.uv = uvs;
		mesh.triangles = triangles;

		renderer.sharedMaterial = skeletonDataAsset.atlasAsset.material;
	}

	void OnEnable () {
		Update();
	}

	void OnDisable () {
		if (Application.isEditor)
			Clear();
	}

	void Reset () {
		Update();
	}
}
