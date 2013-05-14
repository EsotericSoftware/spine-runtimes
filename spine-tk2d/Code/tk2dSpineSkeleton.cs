using UnityEngine;
using Spine;

// TODO: multiple atlas support
// TODO: split skeleton and animation components
// TODO: add events in animation component

/*
 */
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class tk2dSpineSkeleton : MonoBehaviour, tk2dRuntime.ISpriteCollectionForceBuild {
	
	/*
	 */
	public tk2dSpineSkeletonDataAsset skeletonDataAsset;
	public Skeleton skeleton;
	
	public string animationName;
	public bool loop;
	public float animationSpeed = 1;
	public Spine.AnimationState state;
	
	/*
	 */
	private Mesh mesh;
	private Vector3[] vertices;
	private Color[] colors;
	private Vector2[] uvs;
	private int[] triangles;
	private int cachedQuadCount;
	private float[] vertexPositions;
	
	/*
	 */
	void Awake() {
		vertexPositions = new float[8];
	}
	
	/*
	 */
	void Update() {
		SkeletonData skeletonData = (skeletonDataAsset != null) ? skeletonDataAsset.GetSkeletonData() : null;
		
		if(skeletonData == null) {
			Clear();
			return;
		}
		
		if(skeleton == null || skeleton.Data != skeletonData) Initialize();
		
		UpdateAnimation();
		UpdateSkeleton();
		UpdateCache();
		UpdateMesh();
	}
	
	/*
	 */
	private void Clear() {
		GetComponent<MeshFilter>().mesh = null;
		DestroyImmediate(mesh);
		mesh = null;
		
		renderer.sharedMaterial = null;
		
		skeleton = null;
		state = null;
	}
	
	/*
	 */
	private void Initialize() {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.name = "tk2dSkeleton Mesh";
		mesh.hideFlags = HideFlags.HideAndDontSave;
		
		state = new Spine.AnimationState(skeletonDataAsset.GetAnimationStateData());
		skeleton = new Skeleton(skeletonDataAsset.GetSkeletonData());
	}
	
	/*
	 */
	private void UpdateMesh() {
		int quadIndex = 0;
		int drawCount = skeleton.DrawOrder.Count;
		Color currentColor = new Color();
		
		for(int i = 0; i < drawCount; i++) {
			Slot slot = skeleton.DrawOrder[i];
			Attachment attachment = slot.Attachment;
			
			if(attachment is RegionAttachment) {
				RegionAttachment regionAttachment = attachment as RegionAttachment;
				
				regionAttachment.ComputeVertices(slot.Bone,vertexPositions);
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
				
				currentColor.a = skeleton.A * slot.A;
				currentColor.r = skeleton.R * slot.R * slot.A;
				currentColor.g = skeleton.G * slot.G * slot.A;
				currentColor.b = skeleton.B * slot.B * slot.A;
				
				colors[vertexIndex] = currentColor;
				colors[vertexIndex + 1] = currentColor;
				colors[vertexIndex + 2] = currentColor;
				colors[vertexIndex + 3] = currentColor;
				
				int index = quadIndex * 6;
				triangles[index + 0] = vertexIndex;
				triangles[index + 1] = vertexIndex + 2;
				triangles[index + 2] = vertexIndex + 1;
				triangles[index + 3] = vertexIndex + 2;
				triangles[index + 4] = vertexIndex + 3;
				triangles[index + 5] = vertexIndex + 1;
				
				quadIndex++;
			}
		}
		
		mesh.Clear();
		
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		
		if (skeletonDataAsset.sprites.normalGenerationMode != tk2dSpriteCollection.NormalGenerationMode.None) {
			mesh.RecalculateNormals();

			if (skeletonDataAsset.sprites.normalGenerationMode == tk2dSpriteCollection.NormalGenerationMode.NormalsAndTangents) {
				Vector4[] tangents = new Vector4[mesh.normals.Length];
				for (int t = 0; t < tangents.Length; ++t) {
					tangents[t] = new Vector4(1, 0, 0, 1);
				}
				mesh.tangents = tangents;
			}
		}
		
		renderer.sharedMaterial = skeletonDataAsset.sprites.spriteCollection.inst.materials[0];
	}
	
	/*
	 */
	private void UpdateCache() {
		int quadCount = 0;
		int drawCount = skeleton.DrawOrder.Count;
		
		for(int i = 0; i < drawCount; i++) {
			Attachment attachment = skeleton.DrawOrder[i].Attachment;
			if(attachment is RegionAttachment) quadCount++;
		}
		
		if(quadCount == cachedQuadCount) return;
		
		cachedQuadCount = quadCount;
		vertices = new Vector3[quadCount * 4];
		uvs = new Vector2[quadCount * 4];
		colors = new Color[quadCount * 4];
		triangles = new int[quadCount * 6];
	}
	
	/*
	 */
	private void UpdateSkeleton() {
		skeleton.Update(Time.deltaTime * animationSpeed);
		skeleton.UpdateWorldTransform();
	}
	
	/*
	 */
	private void UpdateAnimation() {
		
		// Check if we need to stop current animation
		if(state.Animation != null && animationName == null) {
			state.ClearAnimation();
		}
		
		// Check for different animation name or animation end
		else if(state.Animation == null || animationName != state.Animation.Name) {
			Spine.Animation animation = skeleton.Data.FindAnimation(animationName);
			if(animation != null) state.SetAnimation(animation,loop);
		}
		
		state.Loop = loop;
		
		// Update animation
		state.Update(Time.deltaTime * animationSpeed);
		state.Apply(skeleton);
	}

	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection) {
		return skeletonDataAsset.sprites.spriteCollection == spriteCollection;
	}

	public void ForceBuild() {
		skeletonDataAsset.ForceUpdate();
		skeleton = new Skeleton(skeletonDataAsset.GetSkeletonData());
		
		UpdateMesh();
	}
}
