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
	private Color[] colors;
	private Vector2[] uvs;
	private int cachedQuadCount;
	private float[] vertexPositions;
	private List<Material> submeshMaterials = new List<Material>();
	private List<int[]> submeshIndices = new List<int[]>();
	private Color cachedCurrentColor;
	
	
	void Awake() {
		vertexPositions = new float[8];
		submeshMaterials = new List<Material>();
		submeshIndices = new List<int[]>();
		cachedCurrentColor = new Color();
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
				
				cachedCurrentColor.a = skeleton.A * slot.A;
				cachedCurrentColor.r = skeleton.R * slot.R * slot.A;
				cachedCurrentColor.g = skeleton.G * slot.G * slot.A;
				cachedCurrentColor.b = skeleton.B * slot.B * slot.A;
				
				colors[vertexIndex] = cachedCurrentColor;
				colors[vertexIndex + 1] = cachedCurrentColor;
				colors[vertexIndex + 2] = cachedCurrentColor;
				colors[vertexIndex + 3] = cachedCurrentColor;
				
				quadIndex++;
			}
		}
		
		mesh.Clear();
		
		mesh.vertices = vertices;
		mesh.colors = colors;
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
	}
	
	private void UpdateCache() {
		int quadCount = 0;
		int drawCount = skeleton.DrawOrder.Count;
		
		for (int i = 0; i < drawCount; i++) {
			Attachment attachment = skeleton.DrawOrder[i].Attachment;
			if (attachment is RegionAttachment) quadCount++;
		}
		
		if (quadCount == cachedQuadCount) return;
		
		cachedQuadCount = quadCount;
		vertices = new Vector3[quadCount * 4];
		uvs = new Vector2[quadCount * 4];
		colors = new Color[quadCount * 4];
		
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
			Material currentMaterial = skeletonDataAsset.spritesData.GetSpriteDefinition(attachment.Name).material;
			
			if(!(attachment is RegionAttachment)) continue;
			
			if(oldMaterial == null) oldMaterial = currentMaterial;
			
			if(oldMaterial != currentMaterial) {
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
}
