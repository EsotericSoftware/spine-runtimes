using System.Collections.Generic;
using UnityEngine;
using Spine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class tk2dSpineSkeleton : MonoBehaviour, tk2dRuntime.ISpriteCollectionForceBuild {
	public tk2dSpineSkeletonDataAsset skeletonDataAsset;
	public Skeleton skeleton;
	
	private Mesh mesh;
	private Vector3[] vertices;
	private Color32[] colors;
	private Vector2[] uvs;
	private int cachedQuadCount;
	private float[] vertexPositions;
	private List<Material> submeshMaterials = new List<Material>();
	private List<int[]> submeshIndices = new List<int[]>();
	
	void Awake() {
		vertexPositions = new float[8];
		submeshMaterials = new List<Material>();
		submeshIndices = new List<int[]>();
	}
	
	void Start () {
		Initialize();
	}
	
	void Update () {
		SkeletonData skeletonData = skeletonDataAsset == null ? null : skeletonDataAsset.GetSkeletonData();
		if (skeletonData == null) {
			Clear();
			return;
		}
		
		if (skeleton == null || skeleton.Data != skeletonData) Initialize();
		
		skeleton.UpdateWorldTransform();
		
		UpdateCache();
		UpdateMesh();
	}
	
	private void Clear() {
		GetComponent<MeshFilter>().mesh = null;
		DestroyImmediate(mesh);
		mesh = null;
		
		skeleton = null;
	}
	
	private void Initialize() {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.name = "tk2dSkeleton Mesh";
		mesh.hideFlags = HideFlags.HideAndDontSave;
		
		if(skeletonDataAsset != null) {
			skeleton = new Skeleton(skeletonDataAsset.GetSkeletonData());
		}
	}
	
	private void UpdateMesh() {
		int quadIndex = 0;
		int drawCount = skeleton.DrawOrder.Count;
		
		Color32 color = new Color32();
		for (int i = 0; i < drawCount; i++) {
			Slot slot = skeleton.DrawOrder[i];
			Attachment attachment = slot.Attachment;
			
			if (attachment is RegionAttachment) {
				RegionAttachment regionAttachment = attachment as RegionAttachment;
				
				regionAttachment.ComputeVertices(skeleton.X, skeleton.Y, slot.Bone, vertexPositions);
				int vertexIndex = quadIndex * 4;
				
				vertices[vertexIndex + 0] = new Vector3(vertexPositions[RegionAttachment.X1],vertexPositions[RegionAttachment.Y1],0);
				vertices[vertexIndex + 1] = new Vector3(vertexPositions[RegionAttachment.X4],vertexPositions[RegionAttachment.Y4],0);
				vertices[vertexIndex + 2] = new Vector3(vertexPositions[RegionAttachment.X2],vertexPositions[RegionAttachment.Y2],0);
				vertices[vertexIndex + 3] = new Vector3(vertexPositions[RegionAttachment.X3],vertexPositions[RegionAttachment.Y3],0);
				
				float[] regionUVs = regionAttachment.UVs;
				uvs[vertexIndex + 0] = new Vector2(regionUVs[RegionAttachment.X1],regionUVs[RegionAttachment.Y1]);
				uvs[vertexIndex + 1] = new Vector2(regionUVs[RegionAttachment.X4],regionUVs[RegionAttachment.Y4]);
				uvs[vertexIndex + 2] = new Vector2(regionUVs[RegionAttachment.X2],regionUVs[RegionAttachment.Y2]);
				uvs[vertexIndex + 3] = new Vector2(regionUVs[RegionAttachment.X3],regionUVs[RegionAttachment.Y3]);
				
				color.a = (byte)(skeleton.A * slot.A * 255);
				color.r = (byte)(skeleton.R * slot.R * color.a);
				color.g = (byte)(skeleton.G * slot.G * color.a);
				color.b = (byte)(skeleton.B * slot.B * color.a);
				
				colors[vertexIndex] = color;
				colors[vertexIndex + 1] = color;
				colors[vertexIndex + 2] = color;
				colors[vertexIndex + 3] = color;
				
				quadIndex++;
			}
		}
		
		mesh.Clear();
		
		mesh.vertices = vertices;
		mesh.colors32 = colors;
		mesh.uv = uvs;
		
		mesh.subMeshCount = submeshIndices.Count;
		for(int i = 0; i < mesh.subMeshCount; ++i) {
			mesh.SetTriangles(submeshIndices[i],i);
		}
		
		if (skeletonDataAsset.normalGenerationMode != tk2dSpriteCollection.NormalGenerationMode.None) {
			mesh.RecalculateNormals();
			
			if (skeletonDataAsset.normalGenerationMode == tk2dSpriteCollection.NormalGenerationMode.NormalsAndTangents) {
				Vector4[] tangents = new Vector4[mesh.normals.Length];
				for (int i = 0; i < tangents.Length; i++) {
					tangents[i] = new Vector4(1, 0, 0, 1);
				}
				mesh.tangents = tangents;
			}
		}
		
#if UNITY_EDITOR
		UpdateEditorGizmo();
#endif
	}
	
	private void UpdateCache() {
		int quadCount = 0;
		int drawCount = skeleton.DrawOrder.Count;
		
		for (int i = 0; i < drawCount; i++) {
			Attachment attachment = skeleton.DrawOrder[i].Attachment;
			if (attachment is RegionAttachment) quadCount++;
		}
		
#if UNITY_EDITOR
		if (mesh.subMeshCount == submeshIndices.Count)
#endif
		if (quadCount == cachedQuadCount) return;
		
		cachedQuadCount = quadCount;
		vertices = new Vector3[quadCount * 4];
		uvs = new Vector2[quadCount * 4];
		colors = new Color32[quadCount * 4];
		
		UpdateSubmeshCache();
	}
	
	private void UpdateSubmeshCache() {
		submeshIndices.Clear();
		submeshMaterials.Clear();
		
		Material oldMaterial = null;
		List<int> currentSubmesh = new List<int>();
		int quadIndex = 0;
		
		int drawCount = skeleton.DrawOrder.Count;
		for (int i = 0; i < drawCount; i++) {
			Attachment attachment = skeleton.DrawOrder[i].Attachment;
			if (!(attachment is RegionAttachment)) continue;
			Material currentMaterial = skeletonDataAsset.spritesData.GetSpriteDefinition(attachment.Name).material;
			
			if (oldMaterial == null) oldMaterial = currentMaterial;
			
			if (oldMaterial != currentMaterial) {
				submeshIndices.Add(currentSubmesh.ToArray());
				submeshMaterials.Add(oldMaterial);
				currentSubmesh.Clear();
			}
			
			int vertexIndex = quadIndex * 4;
			
			currentSubmesh.Add(vertexIndex);
			currentSubmesh.Add(vertexIndex + 2);
			currentSubmesh.Add(vertexIndex + 1);
			currentSubmesh.Add(vertexIndex + 2);
			currentSubmesh.Add(vertexIndex + 3);
			currentSubmesh.Add(vertexIndex + 1);
			
			quadIndex++;
			
			oldMaterial = currentMaterial;
		}
		
		submeshIndices.Add(currentSubmesh.ToArray());
		submeshMaterials.Add(oldMaterial);
		
		renderer.sharedMaterials = submeshMaterials.ToArray();
	}
	
	
	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection) {
		return skeletonDataAsset.spritesData == spriteCollection;
	}

	public void ForceBuild() {
		skeletonDataAsset.ForceUpdate();
		skeleton = new Skeleton(skeletonDataAsset.GetSkeletonData());
		
		UpdateSubmeshCache();
		UpdateMesh();
	}
	
#region Unity Editor
#if UNITY_EDITOR
	Vector3 gizmosCenter = new Vector3();
	Vector3 gizmosSize = new Vector3();
	Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0f);
	Vector3 max = new Vector3(float.MinValue, float.MinValue, 0f);

	void UpdateEditorGizmo() {
		//determine the minimums and maximums
		foreach (Vector3 vert in vertices) {
			min = Vector3.Min(min, vert);
			max = Vector3.Max(max, vert);
		}
		float width = max.x - min.x;
		float height = max.y - min.y;
		gizmosCenter = new Vector3(min.x + (width / 2f), min.y + (height / 2f), 0f);
		gizmosSize = new Vector3(width, height, 1f);
	}
	void OnDrawGizmos() {
		Gizmos.color = Color.clear;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawCube(gizmosCenter, gizmosSize);
	}
#endif
#endregion
}
