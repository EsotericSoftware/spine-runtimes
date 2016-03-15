using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity.MeshGeneration {
	// ISubmeshedMeshGenerator:
	// How to use:
	// Step 1: Have a SubmeshedMeshGenerator instance, and a Spine.Skeleton
	// Step 2: Call GenerateInstruction. Pass it your Skeleton. Keep the return value (a SubmeshedMeshInstruction, you can use it in other classes too).
	// Step 3: Pass the SubmeshedMeshInstruction into GenerateMesh. You'll get a Mesh and Materials.
	// Step 4: Put the Mesh in MeshFilter. Put the Materials in MeshRenderer.sharedMaterials.
	public interface ISubmeshedMeshGenerator {
		SubmeshedMeshInstruction GenerateInstruction (Skeleton skeleton);
		MeshAndMaterials GenerateMesh (SubmeshedMeshInstruction wholeMeshInstruction);
		List<Slot> Separators { get; }
	}

	// ISubmeshSetMeshGenerator
	// How to use:
	// Step 1: Get a list of SubmeshInstruction. You can get this from SkeletonRenderer or an ISubmeshedMeshGenerator's returned SubmeshedMeshInstruction.
	// Step 2: Call AddInstruction one by one, or AddInstructions once.
	// Step 3: Call GenerateMesh. You'll get a Mesh and Materials.
	// Step 4: Put the Mesh in MeshFilter. Put the Materials in MeshRenderer.sharedMaterials.
	public interface ISubmeshSetMeshGenerator {
		MeshAndMaterials GenerateMesh (ExposedList<SubmeshInstruction> instructions, int startSubmesh, int endSubmesh);
	}

	// ISingleSubmeshGenerator
	// How to use:
	// Step 1: Have a single SubmeshInstruction
	// Step 2: 
	public interface ISingleSubmeshGenerator {
		Mesh GenerateMesh (SubmeshInstruction instruction);
	}

	/// <summary>Primarily a collection of Submesh Instructions. This constitutes instructions for how to construct a mesh containing submeshes.</summary>
	public class SubmeshedMeshInstruction {
		public readonly ExposedList<SubmeshInstruction> submeshInstructions = new ExposedList<SubmeshInstruction>();
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
	public struct SubmeshInstruction {
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
		public bool separatedBySlot;

		/// <summary>The number of slots in this SubmeshInstruction's range. Not necessarily the number of attachments.</summary>
		public int SlotCount { get { return endSlot - startSlot; } }
	}

	public static class SubmeshInstructionExtensions {
		public static void FillMaterialArray (this ExposedList<SubmeshInstruction> instructions, Material[] materialArray) {
			for (int i = 0, n = materialArray.Length; i < n; i++)
				materialArray[i] = instructions.Items[i].material;
		}

		public static Material[] GetNewMaterialArray (this ExposedList<SubmeshInstruction> instructions) {
			var materials = new Material[instructions.Count];
			instructions.FillMaterialArray(materials);
			return materials;
		}
	}

	public struct MeshAndMaterials {
		public readonly Mesh mesh;
		public readonly Material[] materials;

		public MeshAndMaterials (Mesh mesh, Material[] materials) {
			this.mesh = mesh;
			this.materials = materials;
		}

	}
}