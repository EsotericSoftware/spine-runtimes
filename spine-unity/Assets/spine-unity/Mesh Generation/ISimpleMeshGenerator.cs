namespace Spine.Unity.MeshGeneration { 
	// Typically, each ISpineMeshGenerator implementation will handle double-buffering meshes, handling any other optimization behavior
	// and operating on assumptions (eg, only handling one skeleton, not updating triangles all the time).
	// The Scale property allows generated mesh to match external systems like Canvas referencePixelsPerUnit

	public interface ISimpleMeshGenerator {
		UnityEngine.Mesh GenerateMesh (Spine.Skeleton skeleton);
		UnityEngine.Mesh LastGeneratedMesh { get; }

		float Scale { set; }
		float ZSpacing { get; set; }

		bool AddNormals { get; set; }
		bool AddTangents { get; set; }
	}
}
