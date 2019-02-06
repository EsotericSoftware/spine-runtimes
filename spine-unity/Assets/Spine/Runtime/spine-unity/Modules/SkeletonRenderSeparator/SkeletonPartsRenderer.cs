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

namespace Spine.Unity.Modules {
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
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

		public void ClearMesh () {
			LazyIntialize();
			meshFilter.sharedMesh = null;
		}

		public void RenderParts (ExposedList<SubmeshInstruction> instructions, int startSubmesh, int endSubmesh) {
			LazyIntialize();

			// STEP 1: Create instruction
			var smartMesh = buffers.GetNextMesh();
			currentInstructions.SetWithSubset(instructions, startSubmesh, endSubmesh);
			bool updateTriangles = SkeletonRendererInstruction.GeometryNotEqual(currentInstructions, smartMesh.instructionUsed);

			// STEP 2: Generate mesh buffers.
			var currentInstructionsSubmeshesItems = currentInstructions.submeshInstructions.Items;
			meshGenerator.Begin();
			if (currentInstructions.hasActiveClipping) {
				for (int i = 0; i < currentInstructions.submeshInstructions.Count; i++)
					meshGenerator.AddSubmesh(currentInstructionsSubmeshesItems[i], updateTriangles);
			} else {
				meshGenerator.BuildMeshWithArrays(currentInstructions, updateTriangles);
			}

			buffers.UpdateSharedMaterials(currentInstructions.submeshInstructions);

			// STEP 3: modify mesh.
			var mesh = smartMesh.mesh;

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
		}

		public void SetPropertyBlock (MaterialPropertyBlock block) {
			LazyIntialize();
			meshRenderer.SetPropertyBlock(block);
		}

		public static SkeletonPartsRenderer NewPartsRendererGameObject (Transform parent, string name, int sortingOrder = 0) {
			var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
			go.transform.SetParent(parent, false);
			var returnComponent = go.AddComponent<SkeletonPartsRenderer>();
			returnComponent.MeshRenderer.sortingOrder = sortingOrder;

			return returnComponent;
		}
	}
}
