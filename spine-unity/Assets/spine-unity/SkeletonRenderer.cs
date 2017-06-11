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

#define SPINE_OPTIONAL_RENDEROVERRIDE
#define SPINE_OPTIONAL_MATERIALOVERRIDE

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
	/// <summary>Renders a skeleton.</summary>
	[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), DisallowMultipleComponent]
	[HelpURL("http://esotericsoftware.com/spine-unity-documentation#Rendering")]
	public class SkeletonRenderer : MonoBehaviour, ISkeletonComponent {

		public delegate void SkeletonRendererDelegate (SkeletonRenderer skeletonRenderer);
		public event SkeletonRendererDelegate OnRebuild;

		/// <summary> Occurs after the vertex data is populated every frame, before the vertices are pushed into the mesh.</summary>
		public event Spine.Unity.MeshGeneratorDelegate OnPostProcessVertices;

		public SkeletonDataAsset skeletonDataAsset;
		public SkeletonDataAsset SkeletonDataAsset { get { return skeletonDataAsset; } } // ISkeletonComponent
		public string initialSkinName;
		public bool initialFlipX, initialFlipY;

		#region Advanced
		// Submesh Separation
		[UnityEngine.Serialization.FormerlySerializedAs("submeshSeparators")] [SpineSlot] public string[] separatorSlotNames = new string[0];
		[System.NonSerialized] public readonly List<Slot> separatorSlots = new List<Slot>();

		[Range(-0.1f, 0f)] public float zSpacing;
		//public bool renderMeshes = true;
		public bool useClipping = true;
		public bool immutableTriangles = false;
		public bool pmaVertexColors = true;
		public bool clearStateOnDisable = false;
		public bool tintBlack = false;
		public bool singleSubmesh = false;

		[UnityEngine.Serialization.FormerlySerializedAs("calculateNormals")]
		public bool addNormals;
		public bool calculateTangents;

		public bool logErrors = false;

		#if SPINE_OPTIONAL_RENDEROVERRIDE
		public bool disableRenderingOnOverride = true;
		public delegate void InstructionDelegate (SkeletonRendererInstruction instruction);
		event InstructionDelegate generateMeshOverride;
		public event InstructionDelegate GenerateMeshOverride {
			add {
				generateMeshOverride += value;
				if (disableRenderingOnOverride && generateMeshOverride != null) {
					Initialize(false);
					meshRenderer.enabled = false;
				}
			}
			remove {
				generateMeshOverride -= value;
				if (disableRenderingOnOverride && generateMeshOverride == null) {
					Initialize(false);
					meshRenderer.enabled = true;
				}
			}
		}
		#endif

		#if SPINE_OPTIONAL_MATERIALOVERRIDE
		[System.NonSerialized] readonly Dictionary<Material, Material> customMaterialOverride = new Dictionary<Material, Material>();
		public Dictionary<Material, Material> CustomMaterialOverride { get { return customMaterialOverride; } }
		#endif

		// Custom Slot Material
		[System.NonSerialized] readonly Dictionary<Slot, Material> customSlotMaterials = new Dictionary<Slot, Material>();
		public Dictionary<Slot, Material> CustomSlotMaterials { get { return customSlotMaterials; } }
		#endregion

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		[System.NonSerialized] public bool valid;
		[System.NonSerialized] public Skeleton skeleton;
		public Skeleton Skeleton {
			get {
				Initialize(false);
				return skeleton;
			}
		}
			
		[System.NonSerialized] readonly SkeletonRendererInstruction currentInstructions = new SkeletonRendererInstruction();
		readonly MeshGenerator meshGenerator = new MeshGenerator();
		[System.NonSerialized] readonly MeshRendererBuffers rendererBuffers = new MeshRendererBuffers();

		#region Runtime Instantiation
		public static T NewSpineGameObject<T> (SkeletonDataAsset skeletonDataAsset) where T : SkeletonRenderer {
			return SkeletonRenderer.AddSpineComponent<T>(new GameObject("New Spine GameObject"), skeletonDataAsset);
		}

		/// <summary>Add and prepare a Spine component that derives from SkeletonRenderer to a GameObject at runtime.</summary>
		/// <typeparam name="T">T should be SkeletonRenderer or any of its derived classes.</typeparam>
		public static T AddSpineComponent<T> (GameObject gameObject, SkeletonDataAsset skeletonDataAsset) where T : SkeletonRenderer {
			var c = gameObject.AddComponent<T>();
			if (skeletonDataAsset != null) {
				c.skeletonDataAsset = skeletonDataAsset;
				c.Initialize(false);
			}
			return c;
		}
		#endregion

		public virtual void Awake () {
			Initialize(false);
		}

		void OnDisable () {
			if (clearStateOnDisable && valid)
				ClearState();
		}

		void OnDestroy () {
			rendererBuffers.Dispose();
		}

		protected virtual void ClearState () {
			meshFilter.sharedMesh = null;
			currentInstructions.Clear();
			if (skeleton != null) skeleton.SetToSetupPose();
		}

		public virtual void Initialize (bool overwrite) {
			if (valid && !overwrite)
				return;

			// Clear
			{
				if (meshFilter != null)
					meshFilter.sharedMesh = null;

				meshRenderer = GetComponent<MeshRenderer>();
				if (meshRenderer != null) meshRenderer.sharedMaterial = null;

				currentInstructions.Clear();
				rendererBuffers.Clear();
				meshGenerator.Begin();
				skeleton = null;
				valid = false;
			}

			if (!skeletonDataAsset) {
				if (logErrors) Debug.LogError("Missing SkeletonData asset.", this);
				return;
			}

			SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
			if (skeletonData == null) return;
			valid = true;

			meshFilter = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();
			rendererBuffers.Initialize();

			skeleton = new Skeleton(skeletonData) {
				flipX = initialFlipX,
				flipY = initialFlipY
			};

			if (!string.IsNullOrEmpty(initialSkinName) && !string.Equals(initialSkinName, "default", System.StringComparison.Ordinal))
				skeleton.SetSkin(initialSkinName);

			separatorSlots.Clear();
			for (int i = 0; i < separatorSlotNames.Length; i++)
				separatorSlots.Add(skeleton.FindSlot(separatorSlotNames[i]));

			LateUpdate();

			if (OnRebuild != null)
				OnRebuild(this);
		}

		public virtual void LateUpdate () {
			if (!valid) return;

			#if SPINE_OPTIONAL_RENDEROVERRIDE
			bool doMeshOverride = generateMeshOverride != null;
			if ((!meshRenderer.enabled)	&& !doMeshOverride) return;
			#else
			const bool doMeshOverride = false;
			if (!meshRenderer.enabled) return;
			#endif
			var currentInstructions = this.currentInstructions;
			var workingSubmeshInstructions = currentInstructions.submeshInstructions;
			var currentSmartMesh = rendererBuffers.GetNextMesh(); // Double-buffer for performance.

			bool updateTriangles;

			if (this.singleSubmesh) {
				// STEP 1. Determine a SmartMesh.Instruction. Split up instructions into submeshes. =============================================
				MeshGenerator.GenerateSingleSubmeshInstruction(currentInstructions, skeleton, skeletonDataAsset.atlasAssets[0].materials[0]);

				// STEP 1.9. Post-process workingInstructions. ==================================================================================
				#if SPINE_OPTIONAL_MATERIALOVERRIDE
				if (customMaterialOverride.Count > 0) // isCustomMaterialOverridePopulated 
					MeshGenerator.TryReplaceMaterials(workingSubmeshInstructions, customMaterialOverride);
				#endif

				// STEP 2. Update vertex buffer based on verts from the attachments.  ===========================================================
				meshGenerator.settings = new MeshGenerator.Settings {
					pmaVertexColors = this.pmaVertexColors,
					zSpacing = this.zSpacing,
					useClipping = this.useClipping,
					tintBlack = this.tintBlack,
					calculateTangents = this.calculateTangents,
					addNormals = this.addNormals
				};
				meshGenerator.Begin();
				updateTriangles = SkeletonRendererInstruction.GeometryNotEqual(currentInstructions, currentSmartMesh.instructionUsed);
				if (currentInstructions.hasActiveClipping) {
					meshGenerator.AddSubmesh(workingSubmeshInstructions.Items[0], updateTriangles);
				} else {
					meshGenerator.BuildMeshWithArrays(currentInstructions, updateTriangles);
				}

			} else {
				// STEP 1. Determine a SmartMesh.Instruction. Split up instructions into submeshes. =============================================
				MeshGenerator.GenerateSkeletonRendererInstruction(currentInstructions, skeleton, customSlotMaterials, separatorSlots, doMeshOverride, this.immutableTriangles);

				// STEP 1.9. Post-process workingInstructions. ==================================================================================
				#if SPINE_OPTIONAL_MATERIALOVERRIDE
				if (customMaterialOverride.Count > 0) // isCustomMaterialOverridePopulated 
					MeshGenerator.TryReplaceMaterials(workingSubmeshInstructions, customMaterialOverride);
				#endif

				#if SPINE_OPTIONAL_RENDEROVERRIDE
				if (doMeshOverride) {
					this.generateMeshOverride(currentInstructions);
					if (disableRenderingOnOverride) return;
				}
				#endif

				updateTriangles = SkeletonRendererInstruction.GeometryNotEqual(currentInstructions, currentSmartMesh.instructionUsed);

				// STEP 2. Update vertex buffer based on verts from the attachments.  ===========================================================
				meshGenerator.settings = new MeshGenerator.Settings {
					pmaVertexColors = this.pmaVertexColors,
					zSpacing = this.zSpacing,
					useClipping = this.useClipping,
					tintBlack = this.tintBlack,
					calculateTangents = this.calculateTangents,
					addNormals = this.addNormals
				};
				meshGenerator.Begin();
				if (currentInstructions.hasActiveClipping)
					meshGenerator.BuildMesh(currentInstructions, updateTriangles);
				else
					meshGenerator.BuildMeshWithArrays(currentInstructions, updateTriangles);
			}

			if (OnPostProcessVertices != null) OnPostProcessVertices.Invoke(this.meshGenerator);

			// STEP 3. Move the mesh data into a UnityEngine.Mesh ===========================================================================
			var currentMesh = currentSmartMesh.mesh;
			meshGenerator.FillVertexData(currentMesh);
			rendererBuffers.UpdateSharedMaterials(workingSubmeshInstructions);
			if (updateTriangles) { // Check if the triangles should also be updated.
				meshGenerator.FillTriangles(currentMesh);
				meshRenderer.sharedMaterials = rendererBuffers.GetUpdatedShaderdMaterialsArray();
			} else if (rendererBuffers.MaterialsChangedInLastUpdate()) {
				meshRenderer.sharedMaterials = rendererBuffers.GetUpdatedShaderdMaterialsArray();
			}


			// STEP 4. The UnityEngine.Mesh is ready. Set it as the MeshFilter's mesh. Store the instructions used for that mesh. ===========
			meshFilter.sharedMesh = currentMesh;
			currentSmartMesh.instructionUsed.Set(currentInstructions);
		}
	}
}
