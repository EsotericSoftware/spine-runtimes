using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity {
	public interface ISubmeshedMeshGenerator {
		/// <summary>Generates instructions for how to generate the submeshed mesh based on the given state of the
		/// skeleton. The returned instructions can be used to generate a whole submeshed mesh or individual submeshes.</summary>
		SubmeshedMeshInstructions GenerateInstructions (Skeleton skeleton);

		/// <summary>Returns a SubmeshedMesh (a mesh and a material array coupled in a struct). 
		/// Call GenerateInstructions to get the SubmeshedMeshInstructions to pass into this.</summary>
		SubmeshedMesh GenerateMesh (SubmeshedMeshInstructions wholeMeshInstruction);

		/// <summary>A list of slots that mark the end of a submesh. The slot after it will be the start of a new submesh.</summary>
		List<Slot> Separators { get; }
	}

	public interface ISingleSubmeshGenerator {
		void FillMesh (SubmeshInstructions instructions, Mesh meshToFill);
	}

	/// <summary>A Submeshed mesh is a return type so the mesh with
	/// multiple submeshes can be coupled with a material array to render its submeshes.</summary>
	public struct SubmeshedMesh {
		public readonly Mesh mesh;
		public readonly Material[] materials;
		public SubmeshedMesh (Mesh mesh, Material[] materials) {
			this.mesh = mesh;
			this.materials = materials;
		}
	}

	/// <summary>Primarily a collection of Submesh Instructions. This constitutes instructions for how to construct a mesh containing submeshes.</summary>
	public class SubmeshedMeshInstructions {
		public readonly ExposedList<SubmeshInstructions> submeshInstructions = new ExposedList<SubmeshInstructions>();
		public readonly ExposedList<Attachment> attachmentList = new ExposedList<Attachment>();
		public int vertexCount = -1;

		/// <summary>Allocates a new material array to render this mesh and its constituent submeshes.</summary>
		public Material[] GetNewMaterialArray () {
			var materials = new Material[submeshInstructions.Count];
			FillMaterialArray(materials);
			return materials;
		}

		/// <summary>Fills a given array with the materials needed to render this submeshed mesh.</summary>
		public void FillMaterialArray (Material[] materialArray) {
			var instructionsItems = submeshInstructions.Items;
			for (int i = 0, n = materialArray.Length; i < n; i++)
				materialArray[i] = instructionsItems[i].material;
		}
	}

	/// <summary>Instructions for how to generate a mesh or submesh out of a range of slots in a given skeleton.</summary>
	public struct SubmeshInstructions {
		public Skeleton skeleton;
		public int startSlot;
		public int endSlot;

		// Cached values because they are determined in the process of generating instructions,
		// but could otherwise be pulled from accessing attachments, checking materials and counting tris and verts.
		public Material material;
		public int triangleCount;
		public int vertexCount;

		// Vertex index offset. Used by submesh generation if part of a bigger mesh.
		public int firstVertexIndex;
	}
}