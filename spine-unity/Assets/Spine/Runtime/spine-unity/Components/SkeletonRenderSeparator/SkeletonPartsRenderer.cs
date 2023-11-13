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

using UnityEngine;

namespace Spine.Unity {
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonRenderSeparator")]
	public class SkeletonPartsRenderer : MonoBehaviour {

		#region Properties
		MeshGenerator meshGenerator;
		public MeshGenerator MeshGenerator {
			get {
				LazyIntialize();
				return meshGenerator;
			}
		}

		MeshRenderer meshRenderer;
		public MeshRenderer MeshRenderer {
			get {
				LazyIntialize();
				return meshRenderer;
			}
		}

		MeshFilter meshFilter;
		public MeshFilter MeshFilter {
			get {
				LazyIntialize();
				return meshFilter;
			}
		}
		#endregion

		#region Callback Delegates
		public delegate void SkeletonPartsRendererDelegate (SkeletonPartsRenderer skeletonPartsRenderer);

		/// <summary>OnMeshAndMaterialsUpdated is called at the end of LateUpdate after the Mesh and
		/// all materials have been updated.</summary>
		public event SkeletonPartsRendererDelegate OnMeshAndMaterialsUpdated;
		#endregion

		MeshRendererBuffers buffers;
		SkeletonRendererInstruction currentInstructions = new SkeletonRendererInstruction();


		void LazyIntialize () {
			if (buffers == null) {
				buffers = new MeshRendererBuffers();
				buffers.Initialize();

				if (meshGenerator != null) return;
				meshGenerator = new MeshGenerator();
				meshFilter = GetComponent<MeshFilter>();
				meshRenderer = GetComponent<MeshRenderer>();
				currentInstructions.Clear();
			}
		}

		void OnDestroy () {
			if (buffers != null) buffers.Dispose();
		}

		public void ClearMesh () {
			LazyIntialize();
			meshFilter.sharedMesh = null;
		}

		public void RenderParts (ExposedList<SubmeshInstruction> instructions, int startSubmesh, int endSubmesh) {
			LazyIntialize();

			// STEP 1: Create instruction
			MeshRendererBuffers.SmartMesh smartMesh = buffers.GetNextMesh();
			currentInstructions.SetWithSubset(instructions, startSubmesh, endSubmesh);
			bool updateTriangles = SkeletonRendererInstruction.GeometryNotEqual(currentInstructions, smartMesh.instructionUsed);

			// STEP 2: Generate mesh buffers.
			SubmeshInstruction[] currentInstructionsSubmeshesItems = currentInstructions.submeshInstructions.Items;
			meshGenerator.Begin();
			if (currentInstructions.hasActiveClipping) {
				for (int i = 0; i < currentInstructions.submeshInstructions.Count; i++)
					meshGenerator.AddSubmesh(currentInstructionsSubmeshesItems[i], updateTriangles);
			} else {
				meshGenerator.BuildMeshWithArrays(currentInstructions, updateTriangles);
			}

			buffers.UpdateSharedMaterials(currentInstructions.submeshInstructions);

			// STEP 3: modify mesh.
			Mesh mesh = smartMesh.mesh;

			if (meshGenerator.VertexCount <= 0) { // Clear an empty mesh
				updateTriangles = false;
				mesh.Clear();
			} else {
				meshGenerator.FillVertexData(mesh);
				if (updateTriangles) {
					meshGenerator.FillTriangles(mesh);
					meshRenderer.sharedMaterials = buffers.GetUpdatedSharedMaterialsArray();
				} else if (buffers.MaterialsChangedInLastUpdate()) {
					meshRenderer.sharedMaterials = buffers.GetUpdatedSharedMaterialsArray();
				}
				meshGenerator.FillLateVertexData(mesh);
			}

			meshFilter.sharedMesh = mesh;
			smartMesh.instructionUsed.Set(currentInstructions);

			if (OnMeshAndMaterialsUpdated != null)
				OnMeshAndMaterialsUpdated(this);
		}

		public void SetPropertyBlock (MaterialPropertyBlock block) {
			LazyIntialize();
			meshRenderer.SetPropertyBlock(block);
		}

		public static SkeletonPartsRenderer NewPartsRendererGameObject (Transform parent, string name, int sortingOrder = 0) {
			GameObject go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
			go.transform.SetParent(parent, false);
			SkeletonPartsRenderer returnComponent = go.AddComponent<SkeletonPartsRenderer>();
			returnComponent.MeshRenderer.sortingOrder = sortingOrder;

			return returnComponent;
		}
	}
}
