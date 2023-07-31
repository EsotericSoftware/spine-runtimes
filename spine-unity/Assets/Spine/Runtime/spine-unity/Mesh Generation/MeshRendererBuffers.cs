/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

// Not for optimization. Do not disable.
#define SPINE_TRIANGLECHECK // Avoid calling SetTriangles at the cost of checking for mesh differences (vertex counts, memberwise attachment list compare) every frame.
//#define SPINE_DEBUG

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
	/// <summary>A double-buffered Mesh, and a shared material array, bundled for use by Spine components that need to push a Mesh and materials to a Unity MeshRenderer and MeshFilter.</summary>
	public class MeshRendererBuffers : IDisposable {
		DoubleBuffered<SmartMesh> doubleBufferedMesh;
		internal readonly ExposedList<Material> submeshMaterials = new ExposedList<Material>();
		internal Material[] sharedMaterials = new Material[0];

		public void Initialize () {
			if (doubleBufferedMesh != null) {
				doubleBufferedMesh.GetNext().Clear();
				doubleBufferedMesh.GetNext().Clear();
				submeshMaterials.Clear();
			} else {
				doubleBufferedMesh = new DoubleBuffered<SmartMesh>();
			}
		}

		/// <summary>Returns a sharedMaterials array for use on a MeshRenderer.</summary>
		/// <returns></returns>
		public Material[] GetUpdatedSharedMaterialsArray () {
			if (submeshMaterials.Count == sharedMaterials.Length)
				submeshMaterials.CopyTo(sharedMaterials);
			else
				sharedMaterials = submeshMaterials.ToArray();

			return sharedMaterials;
		}

		/// <summary>Returns true if the materials were modified since the buffers were last updated.</summary>
		public bool MaterialsChangedInLastUpdate () {
			int newSubmeshMaterials = submeshMaterials.Count;
			Material[] sharedMaterials = this.sharedMaterials;
			if (newSubmeshMaterials != sharedMaterials.Length) return true;

			Material[] submeshMaterialsItems = submeshMaterials.Items;
			for (int i = 0; i < newSubmeshMaterials; i++)
				if (!Material.ReferenceEquals(submeshMaterialsItems[i], sharedMaterials[i])) return true; //if (submeshMaterialsItems[i].GetInstanceID() != sharedMaterials[i].GetInstanceID()) return true;

			return false;
		}

		/// <summary>Updates the internal shared materials array with the given instruction list.</summary>
		public void UpdateSharedMaterials (ExposedList<SubmeshInstruction> instructions) {
			int newSize = instructions.Count;
			{ //submeshMaterials.Resize(instructions.Count);
				if (newSize > submeshMaterials.Items.Length)
					Array.Resize(ref submeshMaterials.Items, newSize);
				submeshMaterials.Count = newSize;
			}

			Material[] submeshMaterialsItems = submeshMaterials.Items;
			SubmeshInstruction[] instructionsItems = instructions.Items;
			for (int i = 0; i < newSize; i++)
				submeshMaterialsItems[i] = instructionsItems[i].material;
		}

		public SmartMesh GetNextMesh () {
			return doubleBufferedMesh.GetNext();
		}

		public void Clear () {
			sharedMaterials = new Material[0];
			submeshMaterials.Clear();
		}

		public void Dispose () {
			if (doubleBufferedMesh == null) return;
			doubleBufferedMesh.GetNext().Dispose();
			doubleBufferedMesh.GetNext().Dispose();
			doubleBufferedMesh = null;
		}

		///<summary>This is a Mesh that also stores the instructions SkeletonRenderer generated for it.</summary>
		public class SmartMesh : IDisposable {
			public Mesh mesh = SpineMesh.NewSkeletonMesh();
			public SkeletonRendererInstruction instructionUsed = new SkeletonRendererInstruction();

			public void Clear () {
				mesh.Clear();
				instructionUsed.Clear();
			}

			public void Dispose () {
				if (mesh != null) {
#if UNITY_EDITOR
					if (Application.isEditor && !Application.isPlaying)
						UnityEngine.Object.DestroyImmediate(mesh);
					else
						UnityEngine.Object.Destroy(mesh);
#else
					UnityEngine.Object.Destroy(mesh);
#endif
				}
				mesh = null;
			}
		}
	}
}
