/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

#if UNITY_2018_1_OR_NEWER
#define PER_MATERIAL_PROPERTY_BLOCKS
#endif

#if UNITY_2017_1_OR_NEWER
#define BUILT_IN_SPRITE_MASK_COMPONENT
#endif

#if UNITY_2019_3_OR_NEWER
#define CONFIGURABLE_ENTER_PLAY_MODE
#endif

#if UNITY_2020_1_OR_NEWER
#define REVERT_HAS_OVERLOADS
#endif

#if !SPINE_DISABLE_THREADING
#define USE_THREADED_SKELETON_UPDATE
#endif

#if !SPINE_AUTO_UPGRADE_COMPONENTS_OFF
#define AUTO_UPGRADE_TO_43_COMPONENTS
#endif

#define SPINE_OPTIONAL_ON_DEMAND_LOADING

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Spine.Unity {

	[DefaultExecutionOrder(1)]
	/// <summary>
	/// Component for managing and rendering a Spine skeleton using a standard MeshRenderer.
	/// </summary>
#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(MeshRenderer)), DisallowMultipleComponent]
	[HelpURL("https://esotericsoftware.com/spine-unity-main-components#SkeletonRenderer-Component")]
	public partial class SkeletonRenderer : MonoBehaviour, ISkeletonRenderer, IHasSkeletonRenderer, IUpgradable {
		// Note: Partial class. As a workaround for single inheritance limitations, this class
		// is split into a common code part (what would be the base class) and a specific code part.
		// This file covers specific attributes, properties and methods.
		// Common ISkeletonRenderer members can be found in SkeletonRenderer.Common.cs.

		#region SkeletonRenderer specific code
		#region Events
		/// <summary>OnMeshAndMaterialsUpdated is called at the end of LateUpdate after the Mesh and
		/// all materials have been updated.</summary>
		public event SkeletonRendererDelegate OnMeshAndMaterialsUpdated;
		#endregion Events

		#region Attributes
		// Component References
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		// Mesh Generation
		/// <summary>If true, the renderer assumes the skeleton only requires one Material and one submesh to render. This allows the MeshGenerator to skip checking for changes in Materials. Enable this as an optimization if the skeleton only uses one Material.</summary>
		/// <remarks>This disables SkeletonRenderSeparator functionality.</remarks>
		public bool singleSubmesh = false;

#if USE_THREADED_SKELETON_UPDATE
		protected MeshGenerator meshGenerator = null;
#else
		protected readonly MeshGenerator meshGenerator = new MeshGenerator();
#endif
		[System.NonSerialized] readonly MeshRendererBuffers rendererBuffers = new MeshRendererBuffers();

		[System.NonSerialized] readonly Dictionary<Material, Material> customMaterialOverride = new Dictionary<Material, Material>();

#if PER_MATERIAL_PROPERTY_BLOCKS
		/// <summary> Applies only when 3+ submeshes are used (2+ materials with alternating order, e.g. "A B A").
		/// If true, GPU instancing is disabled at all materials and MaterialPropertyBlocks are assigned at each
		/// material to prevent aggressive batching of submeshes by e.g. the LWRP renderer, leading to incorrect
		/// draw order (e.g. "A1 B A2" changed to "A1A2 B").
		/// You can disable this parameter when everything is drawn correctly to save the additional performance cost.
		/// </summary>
		public bool fixDrawOrder = false;
#endif

#if BUILT_IN_SPRITE_MASK_COMPONENT
		/// <seealso cref="MaskInteraction"/>
		[SerializeField] protected SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None;

		/// <summary>Cached reference to the already setup material override set at the respective
		/// SkeletonDataAsset.atlasAssets array entry.</summary>
		[System.NonSerialized] public MaterialOverrideSet[] insideMaskMaterials = null;
		[System.NonSerialized] public MaterialOverrideSet[] outsideMaskMaterials = null;

		/// <summary>Shader property ID used for the Stencil comparison function.</summary>
		public static readonly int STENCIL_COMP_PARAM_ID = Shader.PropertyToID("_StencilComp");
		/// <summary>Shader property value used as Stencil comparison function for <see cref="SpriteMaskInteraction.None"/>.</summary>
		public const UnityEngine.Rendering.CompareFunction STENCIL_COMP_MASKINTERACTION_NONE = UnityEngine.Rendering.CompareFunction.Always;
		/// <summary>Shader property value used as Stencil comparison function for <see cref="SpriteMaskInteraction.VisibleInsideMask"/>.</summary>
		public const UnityEngine.Rendering.CompareFunction STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE = UnityEngine.Rendering.CompareFunction.LessEqual;
		/// <summary>Shader property value used as Stencil comparison function for <see cref="SpriteMaskInteraction.VisibleOutsideMask"/>.</summary>
		public const UnityEngine.Rendering.CompareFunction STENCIL_COMP_MASKINTERACTION_VISIBLE_OUTSIDE = UnityEngine.Rendering.CompareFunction.Greater;

		public const string MATERIAL_OVERRIDE_SET_INSIDE_MASK_NAME = "InsideMask";
		public const string MATERIAL_OVERRIDE_SET_OUTSIDE_MASK_NAME = "OutsideMask";
#if UNITY_EDITOR
		private static bool haveStencilParametersBeenFixed = false;
#endif
#endif // #if BUILT_IN_SPRITE_MASK_COMPONENT

#if PER_MATERIAL_PROPERTY_BLOCKS
		private MaterialPropertyBlock reusedPropertyBlock;
		public static readonly int SUBMESH_DUMMY_PARAM_ID = Shader.PropertyToID("_Submesh");
#endif

#if UNITY_EDITOR
		/// <summary>Sets the MeshFilter's hide flags to DontSaveInEditor which fixes the prefab
		/// always being marked as changed, but at the cost of references to the MeshFilter by other
		/// components being lost.</summary>
		public SettingsTriState fixPrefabOverrideViaMeshFilter = SettingsTriState.UseGlobalSetting;
		public static bool fixPrefabOverrideViaMeshFilterGlobal = false;
#endif
		#endregion Attributes

		#region Properties
		#region General Properties
#if BUILT_IN_SPRITE_MASK_COMPONENT
		/// <summary>This enum controls the mode under which the sprite will interact with the masking system.</summary>
		/// <remarks>Interaction modes with <see cref="UnityEngine.SpriteMask"/> components are identical to Unity's <see cref="UnityEngine.SpriteRenderer"/>,
		/// see https://docs.unity3d.com/ScriptReference/SpriteMaskInteraction.html. </remarks>
		public SpriteMaskInteraction MaskInteraction {
			set {
				if (maskInteraction == value) return;
				maskInteraction = value;
				materialsNeedUpdate = true;
			}
			get { return maskInteraction; }
		}
#endif
		/// <summary>Use this Dictionary to override a Material with a different Material.</summary>
		public Dictionary<Material, Material> CustomMaterialOverride {
			get { materialsNeedUpdate = true; return customMaterialOverride; }
		}

		/// <summary>Returns the <see cref="SkeletonClipping"/> used by this renderer for use with e.g.
		/// <see cref="Skeleton.GetBounds(out float, out float, out float, out float, ref float[], SkeletonClipping)"/>
		/// </summary>
		public SkeletonClipping SkeletonClipping { get { return meshGenerator.SkeletonClipping; } }
		public bool Freeze { get { return false; } }
		public float MeshScale { get { return 1; } }
		protected bool UsesSingleSubmesh { get { return singleSubmesh; } }
		protected bool NeedsToGenerateMesh { get { return (meshRenderer && meshRenderer.enabled) || HasGenerateMeshOverride; } }
#if USE_THREADED_SKELETON_UPDATE
		public virtual bool NeedsMainThreadRendererPreparation {
			get { return generateMeshOverride != null || OnInstructionsPrepared != null; }
		}
#endif
		#endregion General Properties

		#region Render Settings Compatibility Properties
		public float zSpacing { get { return meshSettings.zSpacing; } set { meshSettings.zSpacing = value; } }

		/// <summary>Use Spine's clipping feature. If false, ClippingAttachments will be ignored.</summary>
		public bool useClipping { get { return meshSettings.useClipping; } set { meshSettings.useClipping = value; } }

		/// <summary>If true, triangles will not be updated. Enable this as an optimization if the skeleton does not make use of attachment swapping or hiding, or draw order keys. Otherwise, setting this to false may cause errors in rendering.</summary>
		public bool immutableTriangles { get { return meshSettings.immutableTriangles; } set { meshSettings.immutableTriangles = value; } }

		/// <summary>Multiply vertex color RGB with vertex color alpha. Set this to true if the shader used for rendering is a premultiplied alpha shader. Setting this to false disables single-batch additive slots.</summary>
		public bool pmaVertexColors { get { return meshSettings.pmaVertexColors; } set { meshSettings.pmaVertexColors = value; } }

		/// <summary>If true, second colors on slots will be added to the output Mesh as UV2 and UV3. A special "tint black" shader that interprets UV2 and UV3 as black point colors is required to render this properly.</summary>
		public bool tintBlack { get { return meshSettings.tintBlack; } set { meshSettings.tintBlack = value; } }

		/// <summary>If true, the mesh generator adds normals to the output mesh. For better performance and reduced memory requirements, use a shader that assumes the desired normal.</summary>
		public bool addNormals { get { return meshSettings.addNormals; } set { meshSettings.addNormals = value; } }

		/// <summary>If true, tangents are calculated every frame and added to the Mesh. Enable this when using a shader that uses lighting that requires tangents.</summary>
		public bool calculateTangents { get { return meshSettings.calculateTangents; } set { meshSettings.calculateTangents = value; } }
		#endregion Render Settings Compatibility Properties
		#endregion Properties

		#region Methods
		#region Lifecycle
		public virtual void Awake () {
			InitializeMainThreadID();
#if UNITY_EDITOR
			SkeletonRenderer.ApplicationIsPlaying = Application.isPlaying;
#endif
			MeshGenerator.InitializeGlobalSettings();
#if USE_THREADED_SKELETON_UPDATE
			if (meshGenerator == null)
				meshGenerator = new MeshGenerator();
#endif
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
			if (!Application.isPlaying && !wasDeprecatedTransferred) {
				UpgradeTo43();
			}
#endif
#if UNITY_EDITOR && BUILT_IN_SPRITE_MASK_COMPONENT
			EditorFixStencilCompParameters();
#endif

			Initialize(false);
			if (generateMeshOverride == null || !disableRenderingOnOverride)
				updateMode = updateWhenInvisible;
		}

#if UNITY_EDITOR && CONFIGURABLE_ENTER_PLAY_MODE
		public virtual void Start () {
			Initialize(false);
		}
#endif
		/// <summary>
		/// Initialize this component. Attempts to load the SkeletonData and creates the internal Skeleton object and buffers.</summary>
		/// <param name="overwrite">If set to <c>true</c>, it will overwrite internal objects if they were already generated. Otherwise, the initialized component will ignore subsequent calls to initialize.</param>
		public virtual void Initialize (bool overwrite, bool quiet = false) {
			if (valid && !overwrite)
				return;

			skeletonAnimation = this.GetComponent<ISkeletonAnimation>();
			if (skeletonAnimation != null)
				skeletonAnimation.EnsureRendererEventsSubscribed();

#if UNITY_EDITOR
			if (BuildUtilities.IsInSkeletonAssetBuildPreProcessing)
				return;
#endif
			meshFilter = GetComponent<MeshFilter>();
			if (meshFilter == null)
				meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();

			InitializeCommon(overwrite, quiet);
#if UNITY_EDITOR
			requiresEditorUpdate = false;
#endif
		}

		public void Clear () {
			ClearMeshAtRenderer();
			ClearCommon();
		}

		/// <summary>
		/// Clears the previously generated mesh and resets the skeleton's pose.
		/// Also clears the animation state when a SkeletonAnimation component is
		/// associated with this SkeletonRenderer</summary>
		public virtual void ClearState () {
			ClearSkeletonState();
			skeletonAnimation.ClearAnimationState();
		}

		/// <summary>
		/// Clears the previously generated mesh and resets the skeleton's pose.</summary>
		public virtual void ClearSkeletonState () {
			ClearMeshAtRenderer();
			currentInstructions.Clear();
			if (skeleton != null) skeleton.SetupPose();
		}

#if UNITY_EDITOR || USE_THREADED_SKELETON_UPDATE
		void OnEnable () {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				LateUpdate();
			}
#endif
#if USE_THREADED_SKELETON_UPDATE
			if (Application.isPlaying && UsesThreadedMeshGeneration && !isUpdatedExternally) {
				SkeletonUpdateSystem system = SkeletonUpdateSystem.Instance;
				if (system)
					system.RegisterForUpdate(this);
			}
#endif
		}
#endif // UNITY_EDITOR || USE_THREADED_SKELETON_UPDATE

		void OnDisable () {
#if USE_THREADED_SKELETON_UPDATE
			if (Application.isPlaying && UsesThreadedMeshGeneration) {
				SkeletonUpdateSystem system = SkeletonUpdateSystem.Instance;
				if (system)
					system.UnregisterFromUpdate(this);
			}
#endif

			if (clearStateOnDisable && valid)
				ClearState();
		}

		void OnDestroy () {
			rendererBuffers.Dispose();
			valid = false;
		}

		public void OnBecameVisible () {
			UpdateMode previousUpdateMode = updateMode;
			updateMode = UpdateMode.FullUpdate;
			// OnBecameVisible is called after Update and LateUpdate(),
			// so update if previousUpdateMode didn't already update this frame.
			if (previousUpdateMode != UpdateMode.FullUpdate) {
				if (skeletonAnimation != null)
					skeletonAnimation.OnBecameVisibleFromMode(previousUpdateMode);
				LateUpdate();
			}
		}

		public void OnBecameInvisible () {
			updateMode = updateWhenInvisible;
		}

		public void LateUpdate () {
#if USE_THREADED_SKELETON_UPDATE
			if (isUpdatedExternally) return;
#endif
			LateUpdateImplementation();
		}
		#endregion Lifecycle

		#region Mesh Generation
		/// <summary>Applies MeshGenerator settings to the SkeletonRenderer and its internal MeshGenerator.</summary>
		public void SetMeshSettings (MeshGenerator.Settings settings) {
			meshSettings = settings;
		}

		protected void ClearMeshGenerator () {
#if USE_THREADED_SKELETON_UPDATE
			if (meshGenerator == null)
				meshGenerator = new MeshGenerator();
#endif
			meshGenerator.Begin();
		}

		protected Mesh GetCurrentMesh () {
			return rendererBuffers.GetCurrentMesh().mesh;
		}

		protected bool RequiresMultipleSubmeshesByDrawOrder () {
			if (!valid)
				return false;
			return MeshGenerator.RequiresMultipleSubmeshesByDrawOrder(skeleton);
		}

		public void PrepareInstructionsAndRenderers () {
			if (UsesSingleSubmesh) {
				MeshGenerator.GenerateSingleSubmeshInstruction(currentInstructions, skeleton, skeletonDataAsset.atlasAssets[0].PrimaryMaterial);
			} else {
				GenerateSkeletonRendererInstructions();
			}
			if (OnInstructionsPrepared != null)
				OnInstructionsPrepared(this.currentInstructions);
		}

		protected virtual void GenerateSkeletonRendererInstructions () {
			MeshGenerator.GenerateSkeletonRendererInstruction(currentInstructions, skeleton, customSlotMaterials,
				enableSeparatorSlots ? separatorSlots : null,
				enableSeparatorSlots ? separatorSlots.Count > 0 : false,
				meshSettings.immutableTriangles);
		}

		protected virtual bool FillBuffersFromSubmeshInstructions (ExposedList<SubmeshInstruction> workingSubmeshInstructions,
			MeshRendererBuffers.SmartMesh currentSmartMesh, bool updateTriangles) {
			return FillSingleBufferFromInstructions(workingSubmeshInstructions, currentSmartMesh, updateTriangles);
		}

		/// <returns>True if any mesh has been filled with data, false otherwise.</returns>
		protected virtual bool FillSingleBufferFromInstructions (ExposedList<SubmeshInstruction> workingSubmeshInstructions,
			MeshRendererBuffers.SmartMesh currentSmartMesh, bool updateTriangles) {

			meshGenerator.settings = meshSettings;
			meshGenerator.Begin();

			if (!currentInstructions.hasActiveClipping)
				meshGenerator.BuildMeshWithArrays(currentInstructions, updateTriangles);
			else if (UsesSingleSubmesh)
				meshGenerator.AddSubmesh(workingSubmeshInstructions.Items[0], updateTriangles);
			else
				meshGenerator.BuildMesh(currentInstructions, updateTriangles);

			currentSmartMesh.instructionUsed.Set(currentInstructions);
			IssueOnPostProcessVertices(meshGenerator.Buffers);
			return true;
		}

		protected virtual bool FillMeshFromBuffers (MeshRendererBuffers.SmartMesh currentSmartMesh, bool updateTriangles) {
			Mesh currentMesh = currentSmartMesh.mesh;
			meshGenerator.FillVertexData(currentMesh);
			if (updateTriangles) { // Check if the triangles should also be updated.
				meshGenerator.FillTriangles(currentMesh);
			}
			meshGenerator.FillLateVertexData(currentMesh);
			return true;
		}
		#endregion Mesh Generation

		#region Renderer Assignment
		protected void EnableRenderers () {
			if (meshRenderer)
				meshRenderer.enabled = true;
		}

		protected void DisableRenderers () {
			if (meshRenderer)
				meshRenderer.enabled = false;
		}

		protected virtual void AssignMeshAtRenderer (ExposedList<SubmeshInstruction> workingSubmeshInstructions,
			MeshRendererBuffers.SmartMesh currentSmartMesh) {

			Mesh currentMesh = currentSmartMesh.mesh;
			AssignMeshAtRenderer(currentMesh);
		}

		protected virtual void AssignMeshAtRenderer (UnityEngine.Mesh mesh) {
			if (meshFilter != null)
				meshFilter.sharedMesh = mesh;

			if (meshRenderer != null) {
				meshRenderer.sharedMaterials = rendererBuffers.sharedMaterials;
			}
#if PER_MATERIAL_PROPERTY_BLOCKS
			if (fixDrawOrder && meshRenderer.sharedMaterials.Length > 2) {
				SetMaterialSettingsToFixDrawOrder();
			}
#endif
		}

		protected void ClearMeshAtRenderer () {
			if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
			if (meshFilter != null) meshFilter.sharedMesh = null;
		}
		#endregion Renderer Assignment

		#region Separator Slots
		public void FindAndApplySeparatorSlots (string startsWith, bool clearExistingSeparators = true, bool updateStringArray = false) {
			if (string.IsNullOrEmpty(startsWith)) return;

			FindAndApplySeparatorSlots(
				(slotName) => slotName.StartsWith(startsWith),
				clearExistingSeparators,
				updateStringArray
				);
		}

		public void FindAndApplySeparatorSlots (System.Func<string, bool> slotNamePredicate, bool clearExistingSeparators = true, bool updateStringArray = false) {
			if (slotNamePredicate == null) return;
			if (!valid) return;

			if (clearExistingSeparators)
				separatorSlots.Clear();

			ExposedList<Slot> slots = skeleton.Slots;
			foreach (Slot slot in slots) {
				if (slotNamePredicate.Invoke(slot.Data.Name))
					separatorSlots.Add(slot);
			}

			if (updateStringArray) {
				List<string> detectedSeparatorNames = new List<string>();
				foreach (Slot slot in skeleton.Slots) {
					string slotName = slot.Data.Name;
					if (slotNamePredicate.Invoke(slotName))
						detectedSeparatorNames.Add(slotName);
				}
				if (!clearExistingSeparators) {
					string[] originalNames = separatorSlotNames;
					foreach (string originalName in originalNames)
						detectedSeparatorNames.Add(originalName);
				}
				separatorSlotNames = detectedSeparatorNames.ToArray();
			}
		}

		public void ReapplySeparatorSlotNames () {
			if (!valid)
				return;

			separatorSlots.Clear();
			for (int i = 0, n = separatorSlotNames.Length; i < n; i++) {
				Slot slot = skeleton.FindSlot(separatorSlotNames[i]);
				if (slot != null) {
					separatorSlots.Add(slot);
				}
#if UNITY_EDITOR
				else if (!string.IsNullOrEmpty(separatorSlotNames[i])) {
					Debug.LogWarning(separatorSlotNames[i] + " is not a slot in " + skeletonDataAsset.skeletonJSON.name);
				}
#endif
			}
		}
		#endregion Separator Slots

		#region Material Configuration
		protected virtual void UpdateUsedMaterialsForRenderers (ExposedList<SubmeshInstruction> instructions) {
			rendererBuffers.UpdateSharedMaterialsArray();
			ConfigureMaterials(instructions);
			materialsNeedUpdate = false;
		}

		public virtual void ConfigureMaterials (Material[] sharedMaterials, ExposedList<SubmeshInstruction> instructions) {
			if (customMaterialOverride.Count > 0) {
				for (int i = 0, count = sharedMaterials.Length; i < count; ++i) {
					Material material = sharedMaterials[i];
					if (material == null) continue;

					Material overrideMaterial;
					if (customMaterialOverride.TryGetValue(material, out overrideMaterial))
						sharedMaterials[i] = overrideMaterial;
				}
			}

#if BUILT_IN_SPRITE_MASK_COMPONENT
			if (maskInteraction == SpriteMaskInteraction.VisibleInsideMask) {
				if (insideMaskMaterials == null || insideMaskMaterials.Length == 0) {
					InitSpriteMaskMaterialsMaskMode(ref insideMaskMaterials, this,
						MATERIAL_OVERRIDE_SET_INSIDE_MASK_NAME, STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE);
					if (insideMaskMaterials == null) return;
				}
				foreach (MaterialOverrideSet overrideSet in insideMaskMaterials) {
					overrideSet.ApplyOverrideTo(sharedMaterials);
				}
			} else if (maskInteraction == SpriteMaskInteraction.VisibleOutsideMask) {
				if (outsideMaskMaterials == null || outsideMaskMaterials.Length == 0) {
					InitSpriteMaskMaterialsMaskMode(ref outsideMaskMaterials, this,
						MATERIAL_OVERRIDE_SET_OUTSIDE_MASK_NAME, STENCIL_COMP_MASKINTERACTION_VISIBLE_OUTSIDE);
					if (outsideMaskMaterials == null) return;
				}
				foreach (MaterialOverrideSet overrideSet in outsideMaskMaterials) {
					overrideSet.ApplyOverrideTo(sharedMaterials);
				}
			}
#endif
		}

		protected virtual void ConfigureMaterials (ExposedList<SubmeshInstruction> instructions) {
			ConfigureMaterials(rendererBuffers.sharedMaterials, instructions);
		}

#if BUILT_IN_SPRITE_MASK_COMPONENT
		private void InitSpriteMaskMaterialsMaskMode (ref MaterialOverrideSet[] maskMaterials,
			SkeletonRenderer skeletonRenderer, string overrideSetName,
			UnityEngine.Rendering.CompareFunction maskFunction) {

			AtlasAssetBase[] atlasAssets = skeletonRenderer.skeletonDataAsset.atlasAssets;
			int atlasAssetCount = atlasAssets.Length;
			if (maskMaterials == null || maskMaterials.Length != atlasAssetCount)
				maskMaterials = new MaterialOverrideSet[atlasAssetCount];

			for (int i = 0, n = atlasAssetCount; i < n; ++i) {
				AtlasAssetBase atlasAsset = atlasAssets[i];
				maskMaterials[i] = atlasAsset.GetMaterialOverrideSet(overrideSetName);
				if (maskMaterials[i] == null) {
#if UNITY_EDITOR
					// Editor script shall create assets.
					if (!Application.isPlaying) {
						maskMaterials = null;
						return;
					}
#endif
					maskMaterials[i] = InitSpriteMaskOverrideSet(
						atlasAsset, overrideSetName, maskFunction);
				}
			}
		}

		private static MaterialOverrideSet InitSpriteMaskOverrideSet (
			AtlasAssetBase atlasAsset, string overrideSetName, UnityEngine.Rendering.CompareFunction maskFunction) {

			MaterialOverrideSet overrideSet = atlasAsset.AddMaterialOverrideSet(overrideSetName);
			foreach (Material originalMaterial in atlasAsset.Materials) {
				if (originalMaterial == null)
					continue;
				Material maskMaterial = new Material(originalMaterial);
				maskMaterial.name += overrideSetName;
				maskMaterial.SetFloat(SkeletonRenderer.STENCIL_COMP_PARAM_ID, (int)maskFunction);
				overrideSet.AddOverride(originalMaterial, maskMaterial);
			}
			return overrideSet;
		}
#endif //#if BUILT_IN_SPRITE_MASK_COMPONENT

#if PER_MATERIAL_PROPERTY_BLOCKS
		/// <summary>
		/// This method was introduced as a workaround for too aggressive submesh draw call batching,
		/// leading to incorrect draw order when 3+ materials are used at submeshes in alternating order.
		/// Otherwise, e.g. when using Lightweight Render Pipeline, deliberately separated draw calls
		/// "A1 B A2" are reordered to "A1A2 B", regardless of batching-related project settings.
		/// </summary>
		private void SetMaterialSettingsToFixDrawOrder () {
			if (reusedPropertyBlock == null) reusedPropertyBlock = new MaterialPropertyBlock();
			bool hasPerRendererBlock = meshRenderer.HasPropertyBlock();
			if (hasPerRendererBlock) {
				meshRenderer.GetPropertyBlock(reusedPropertyBlock);
			}

			for (int i = 0; i < meshRenderer.sharedMaterials.Length; ++i) {
				if (!meshRenderer.sharedMaterials[i])
					continue;

				if (!hasPerRendererBlock) meshRenderer.GetPropertyBlock(reusedPropertyBlock, i);
				// Note: this parameter shall not exist at any shader, then Unity will create separate
				// material instances (not in terms of memory cost or leakage).
				reusedPropertyBlock.SetFloat(SUBMESH_DUMMY_PARAM_ID, i);
				meshRenderer.SetPropertyBlock(reusedPropertyBlock, i);

				meshRenderer.sharedMaterials[i].enableInstancing = false;
			}
		}
#endif

#if SPINE_OPTIONAL_ON_DEMAND_LOADING
		void HandleOnDemandLoading () {
			foreach (AtlasAssetBase atlasAsset in skeletonDataAsset.atlasAssets) {
				if (atlasAsset.TextureLoadingMode != AtlasAssetBase.LoadingMode.Normal) {
					atlasAsset.BeginCustomTextureLoading();
					Material[] materials = rendererBuffers.sharedMaterials;
					for (int i = 0, count = materials.Length; i < count; ++i) {
						Material overrideMaterial = null;
						atlasAsset.RequireTexturesLoaded(materials[i], ref overrideMaterial);
						if (overrideMaterial != null)
							materials[i] = overrideMaterial;
					}
					atlasAsset.EndCustomTextureLoading();
				}
			}
		}
#endif
		#endregion Material Configuration

		#region Runtime Instantiation
		public static SkeletonComponents<Renderer, Animation> NewSpineGameObject<Renderer, Animation> (
			SkeletonDataAsset skeletonDataAsset, bool quiet = false)
			where Renderer : MonoBehaviour, ISkeletonRenderer
			where Animation : SkeletonAnimationBase {

			return SkeletonRenderer.AddSpineComponents<Renderer, Animation>(new GameObject("New Spine GameObject"), skeletonDataAsset, quiet);
		}

		/// <summary>Add and prepare a Spine animation component and the default
		/// SkeletonRenderer components to a GameObject at runtime.</summary>
		/// <typeparam name="Animation">Animation should be SkeletonAnimation, SkeletonMecanim or any custom derived class.</typeparam>
		public static SkeletonComponents<Renderer, Animation> AddSpineComponents<Renderer, Animation> (
			GameObject gameObject, SkeletonDataAsset skeletonDataAsset, bool quiet = false)
			where Renderer : MonoBehaviour, ISkeletonRenderer
			where Animation : SkeletonAnimationBase {

			Renderer rendererComponent = gameObject.AddComponent<Renderer>();
			if (skeletonDataAsset != null) {
				rendererComponent.SkeletonDataAsset = skeletonDataAsset;
				rendererComponent.Initialize(false, quiet);
			}
			Animation animationComponent = gameObject.AddComponent<Animation>();
			if (skeletonDataAsset != null) {
				animationComponent.Initialize(false, quiet);
			}
			rendererComponent.Animation = animationComponent;
			return new SkeletonComponents<Renderer, Animation>(rendererComponent, animationComponent);
		}
		#endregion Runtime Instantiation

		#region Internal Methods
#if USE_THREADED_SKELETON_UPDATE
		public virtual void MainThreadPrepareLateUpdateInternal () {
			if (!valid) return;
			if (updateMode != UpdateMode.FullUpdate && wasMeshUpdatedAfterInit) return;

			if (NeedsMainThreadRendererPreparation)
				PrepareInstructionsAndRenderers();

			if (generateMeshOverride != null)
				generateMeshOverride(currentInstructions);
		}
#else
		public virtual void MainThreadPrepareLateUpdateInternal () {
		}
#endif

		/// <summary>
		/// Generates a new UnityEngine.Mesh from the internal Skeleton.</summary>
		public virtual void LateUpdateImplementation (bool calledFromMainThread = true) {
			if (calledFromMainThread) {
#if UNITY_EDITOR && NEW_PREFAB_SYSTEM
				// Don't store mesh or material at the prefab, otherwise it will permanently reload
				UnityEditor.PrefabAssetType prefabType = UnityEditor.PrefabUtility.GetPrefabAssetType(this);
				if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) &&
					(prefabType == UnityEditor.PrefabAssetType.Regular || prefabType == UnityEditor.PrefabAssetType.Variant)) {
					return;
				}
				EditorUpdateMeshFilterHideFlags();
#endif

#if UNITY_EDITOR
				if (!Application.isPlaying && requiresEditorUpdate) {
					Initialize(true);
				}
#endif
			}

			if (!valid) return;

			// instantiation can happen from Update() after this component, leading to a missing Update() call.
			if (calledFromMainThread && skeletonAnimation != null)
				skeletonAnimation.UpdateOncePerFrame(0);

			// Generate mesh once, required to update mesh bounds for visibility
			if (updateMode != UpdateMode.FullUpdate && wasMeshUpdatedAfterInit) return;
			if (calledFromMainThread && !NeedsToGenerateMesh) return;
			UpdateMesh(calledFromMainThread);
		}
		#endregion Internal Methods
		#region Editor Methods
#if UNITY_EDITOR
		protected void OnValidate () {
			requiresEditorUpdate = true;
		}

		// revert each prefab override only once each editor-frame.
		private static int lastPrefabRevertFrame = -1;
		private static HashSet<MeshFilter> revertedPrefabMeshes = new HashSet<MeshFilter>();
		private static bool preventReentrance = false;

		public void EditorUpdateMeshFilterHideFlags () {
			if (!meshFilter) {
				meshFilter = GetComponent<MeshFilter>();
				if (meshFilter == null)
					meshFilter = gameObject.AddComponent<MeshFilter>();
			}

			bool dontSaveInEditor = false;
			if (fixPrefabOverrideViaMeshFilter == SettingsTriState.Enable ||
				(fixPrefabOverrideViaMeshFilter == SettingsTriState.UseGlobalSetting &&
				fixPrefabOverrideViaMeshFilterGlobal))
				dontSaveInEditor = true;

			if (dontSaveInEditor) {
#if NEW_PREFAB_SYSTEM
				int currentFrame = Time.frameCount;
				if (lastPrefabRevertFrame != currentFrame) {
					lastPrefabRevertFrame = currentFrame;
					revertedPrefabMeshes.Clear();
				}

				if (!preventReentrance && UnityEditor.PrefabUtility.IsPartOfAnyPrefab(meshFilter)) {
					if (!revertedPrefabMeshes.Contains(meshFilter)) {
						GameObject instanceRoot = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(meshFilter);
						if (instanceRoot != null) {
							UnityEditor.PropertyModification[] mods = UnityEditor.PrefabUtility.GetPropertyModifications(instanceRoot);
							bool hasMeshOverride = false;
							if (mods != null) {
								foreach (var mod in mods) {
									if (mod.target == meshFilter && mod.propertyPath == "m_Mesh") {
										hasMeshOverride = true;
										break;
									}
								}
							}
							if (hasMeshOverride) {
								preventReentrance = true;
								try {
									List<ObjectOverride> objectOverrides = UnityEditor.PrefabUtility.GetObjectOverrides(instanceRoot);
									foreach (ObjectOverride objectOverride in objectOverrides) {
										if (objectOverride.instanceObject == meshFilter) {
#if REVERT_HAS_OVERLOADS
											objectOverride.Revert(UnityEditor.InteractionMode.AutomatedAction);
#else
											objectOverride.Revert();
#endif
											revertedPrefabMeshes.Add(meshFilter);
											break;
										}
									}
								} finally {
									preventReentrance = false;
								}
							}
						}
					}
				}
#endif
				meshFilter.hideFlags = HideFlags.DontSaveInEditor;
			} else {
				meshFilter.hideFlags = HideFlags.None;
			}
		}

		private void EditorFixStencilCompParameters () {
			if (!haveStencilParametersBeenFixed && HasAnyStencilComp0Material()) {
				haveStencilParametersBeenFixed = true;
				FixAllProjectMaterialsStencilCompParameters();
			}
		}

		private void FixAllProjectMaterialsStencilCompParameters () {
			string[] materialGUIDS = UnityEditor.AssetDatabase.FindAssets("t:material");
			foreach (string guid in materialGUIDS) {
				string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				if (!string.IsNullOrEmpty(path)) {
					Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
					if (mat.HasProperty(STENCIL_COMP_PARAM_ID) && mat.GetFloat(STENCIL_COMP_PARAM_ID) == 0) {
						mat.SetFloat(STENCIL_COMP_PARAM_ID, (int)STENCIL_COMP_MASKINTERACTION_NONE);
					}
				}
			}
			UnityEditor.AssetDatabase.Refresh();
			UnityEditor.AssetDatabase.SaveAssets();
		}

		private bool HasAnyStencilComp0Material () {
			if (meshRenderer == null)
				return false;

			foreach (Material mat in meshRenderer.sharedMaterials) {
				if (mat != null && mat.HasProperty(STENCIL_COMP_PARAM_ID)) {
					float currentCompValue = mat.GetFloat(STENCIL_COMP_PARAM_ID);
					if (currentCompValue == 0)
						return true;
				}
			}
			return false;
		}
#endif // UNITY_EDITOR

		#endregion Editor Methods
		#region Transfer of Deprecated Fields

#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		public void UpgradeTo43 () {
			TransferDeprecatedFields();
		}

		protected virtual void TransferDeprecatedFields () {
			wasDeprecatedTransferred = true;
			// only transfer once, let SkeletonAnimation transfer properties if present
			if (this.Animation != null) return;

			meshSettings.zSpacing = this.zSpacingDeprecated;
			meshSettings.useClipping = this.useClippingDeprecated;
			meshSettings.immutableTriangles = this.immutableTrianglesDeprecated;
			meshSettings.pmaVertexColors = this.pmaVertexColorsDeprecated;
			meshSettings.tintBlack = this.tintBlackDeprecated;
			meshSettings.addNormals = this.addNormalsDeprecated;
			meshSettings.calculateTangents = this.calculateTangentsDeprecated;
		}

		[SerializeField] protected bool wasDeprecatedTransferred = false;
		[FormerlySerializedAs("zSpacing")] [SerializeField] private float zSpacingDeprecated = 0f;
		[FormerlySerializedAs("useClipping")] [SerializeField] private bool useClippingDeprecated = true;
		[FormerlySerializedAs("immutableTriangles")] [SerializeField] private bool immutableTrianglesDeprecated = false;
		[FormerlySerializedAs("pmaVertexColors")] [SerializeField] private bool pmaVertexColorsDeprecated = true;
		[FormerlySerializedAs("tintBlack")] [SerializeField] private bool tintBlackDeprecated = false;
		[FormerlySerializedAs("calculateNormals"),
			FormerlySerializedAs("addNormals")]
		[SerializeField] private bool addNormalsDeprecated = false;
		[FormerlySerializedAs("calculateTangents")] [SerializeField] private bool calculateTangentsDeprecated = false;
#endif // UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		#endregion Transfer of Deprecated Fields
		#endregion Methods
		#endregion SkeletonRenderer specific code
	}

	public struct SkeletonComponents<Renderer, Animation> {
		public Renderer skeletonRenderer;
		public Animation skeletonAnimation;

		public SkeletonComponents (Renderer skeletonRenderer, Animation skeletonAnimation) {
			this.skeletonRenderer = skeletonRenderer;
			this.skeletonAnimation = skeletonAnimation;
		}
	}
}
