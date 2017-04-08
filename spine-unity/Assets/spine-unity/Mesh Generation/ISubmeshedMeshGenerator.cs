/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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

		float ZSpacing { get; set; }
		bool PremultiplyVertexColors { get; set; }
		bool AddNormals { get; set; }
		bool AddTangents { get; set; }
	}

	// ISubmeshSetMeshGenerator
	// How to use:
	// Step 1: Get a list of SubmeshInstruction. You can get this from SkeletonRenderer or an ISubmeshedMeshGenerator's returned SubmeshedMeshInstruction.
	// Step 2: Call AddInstruction one by one, or AddInstructions once.
	// Step 3: Call GenerateMesh. You'll get a Mesh and Materials.
	// Step 4: Put the Mesh in MeshFilter. Put the Materials in MeshRenderer.sharedMaterials.
	public interface ISubmeshSetMeshGenerator {
		MeshAndMaterials GenerateMesh (ExposedList<SubmeshInstruction> instructions, int startSubmesh, int endSubmesh, float scale = 1f);

		float ZSpacing { get; set; }
		bool PremultiplyVertexColors { get; set; }
		bool AddNormals { get; set; }
		bool AddTangents { get; set; }
	}

	/// <summary>Primarily a collection of Submesh Instructions. This constitutes instructions for how to construct a mesh containing submeshes.</summary>
	public class SubmeshedMeshInstruction {
		public readonly ExposedList<SubmeshInstruction> submeshInstructions = new ExposedList<SubmeshInstruction>();
		public readonly ExposedList<Attachment> attachmentList = new ExposedList<Attachment>();
		public int vertexCount = -1;

		/// <summary>Returns a material array of the SubmeshedMeshInstruction. Fills the passed array if it's the correct size. Creates a new array if it's a different size.</summary>
		public Material[] GetUpdatedMaterialArray (Material[] materials) {
			return submeshInstructions.GetUpdatedMaterialArray(materials);
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
		public bool forceSeparate;

		/// <summary>The number of slots in this SubmeshInstruction's range. Not necessarily the number of attachments.</summary>
		public int SlotCount { get { return endSlot - startSlot; } }
	}

	public static class SubmeshInstructionExtensions {
		/// <summary>Returns a material array of the instructions. Fills the passed array if it's the correct size. Creates a new array if it's a different size.</summary>
		public static Material[] GetUpdatedMaterialArray (this ExposedList<SubmeshInstruction> instructions, Material[] materials) {
			int submeshCount = instructions.Count;

			if (submeshCount != materials.Length)
				materials = new Material[submeshCount];

			for (int i = 0, n = materials.Length; i < n; i++)
				materials[i] = instructions.Items[i].material;

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
