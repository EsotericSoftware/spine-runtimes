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

#define SPINE_OPTIONAL_RENDEROVERRIDE
#define SPINE_OPTIONAL_MATERIALOVERRIDE
#define SPINE_OPTIONAL_ON_DEMAND_LOADING

using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Spine.Unity {
	/// <summary>Base class of animated Spine skeleton components. This component manages and renders a skeleton.</summary>
#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(MeshRenderer)), DisallowMultipleComponent]
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonRenderer-Component")]
	public class SkeletonRenderer : MonoBehaviour, ISkeletonComponent, IHasSkeletonDataAsset {
		public SkeletonDataAsset skeletonDataAsset;

		#region Initialization settings
		/// <summary>Skin name to use when the Skeleton is initialized.</summary>
		[SpineSkin(defaultAsEmptyString: true)] public string initialSkinName;

		/// <summary>Enable this parameter when overwriting the Skeleton's skin from an editor script.
		/// Otherwise any changes will be overwritten by the next inspector update.</summary>
#if UNITY_EDITOR
		public bool EditorSkipSkinSync {
			get { return editorSkipSkinSync; }
			set { editorSkipSkinSync = value; }
		}
		protected bool editorSkipSkinSync = false;

		/// <summary>Sets the MeshFilter's hide flags to DontSaveInEditor which fixes the prefab
		/// always being marked as changed, but at the cost of references to the MeshFilter by other
		/// components being lost.</summary>
		public SettingsTriState fixPrefabOverrideViaMeshFilter = SettingsTriState.UseGlobalSetting;
		public static bool fixPrefabOverrideViaMeshFilterGlobal = false;
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
				if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(meshFilter)) {
					GameObject instanceRoot = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(meshFilter);
					if (instanceRoot != null) {
						List<ObjectOverride> objectOverrides = UnityEditor.PrefabUtility.GetObjectOverrides(instanceRoot);
						foreach (ObjectOverride objectOverride in objectOverrides) {
							if (objectOverride.instanceObject == meshFilter) {
#if REVERT_HAS_OVERLOADS
								objectOverride.Revert(UnityEditor.InteractionMode.AutomatedAction);
#else
								objectOverride.Revert();
#endif
								break;
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
#endif
		/// <summary>Flip X and Y to use when the Skeleton is initialized.</summary>
		public bool initialFlipX, initialFlipY;
		#endregion

		#region Advanced Render Settings

		/// <summary>Update mode to optionally limit updates to e.g. only apply animations but not update the mesh.</summary>
		public UpdateMode UpdateMode { get { return updateMode; } set { updateMode = value; } }
		protected UpdateMode updateMode = UpdateMode.FullUpdate;

		/// <summary>Update mode used when the MeshRenderer becomes invisible
		/// (when <c>OnBecameInvisible()</c> is called). Update mode is automatically
		/// reset to <c>UpdateMode.FullUpdate</c> when the mesh becomes visible again.</summary>
		public UpdateMode updateWhenInvisible = UpdateMode.FullUpdate;

		// Submesh Separation
		/// <summary>Slot names used to populate separatorSlots list when the Skeleton is initialized. Changing this after initialization does nothing.</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("submeshSeparators")] [SerializeField] [SpineSlot] protected string[] separatorSlotNames = new string[0];

		/// <summary>Slots that determine where the render is split. This is used by components such as SkeletonRenderSeparator so that the skeleton can be rendered by two separate renderers on different GameObjects.</summary>
		[System.NonSerialized] public readonly List<Slot> separatorSlots = new List<Slot>();

		// Render Settings
		[Range(-0.1f, 0f)] public float zSpacing;
		/// <summary>Use Spine's clipping feature. If false, ClippingAttachments will be ignored.</summary>
		public bool useClipping = true;

		/// <summary>If true, triangles will not be updated. Enable this as an optimization if the skeleton does not make use of attachment swapping or hiding, or draw order keys. Otherwise, setting this to false may cause errors in rendering.</summary>
		public bool immutableTriangles = false;

		/// <summary>Multiply vertex color RGB with vertex color alpha. Set this to true if the shader used for rendering is a premultiplied alpha shader. Setting this to false disables single-batch additive slots.</summary>
		public bool pmaVertexColors = true;

		/// <summary>Clears the state of the render and skeleton when this component or its GameObject is disabled. This prevents previous state from being retained when it is enabled again. When pooling your skeleton, setting this to true can be helpful.</summary>
		public bool clearStateOnDisable = false;

		/// <summary>If true, second colors on slots will be added to the output Mesh as UV2 and UV3. A special "tint black" shader that interprets UV2 and UV3 as black point colors is required to render this properly.</summary>
		public bool tintBlack = false;

		/// <summary>If true, the renderer assumes the skeleton only requires one Material and one submesh to render. This allows the MeshGenerator to skip checking for changes in Materials. Enable this as an optimization if the skeleton only uses one Material.</summary>
		/// <remarks>This disables SkeletonRenderSeparator functionality.</remarks>
		public bool singleSubmesh = false;

#if PER_MATERIAL_PROPERTY_BLOCKS
		/// <summary> Applies only when 3+ submeshes are used (2+ materials with alternating order, e.g. "A B A").
		/// If true, GPU instancing is disabled at all materials and MaterialPropertyBlocks are assigned at each
		/// material to prevent aggressive batching of submeshes by e.g. the LWRP renderer, leading to incorrect
		/// draw order (e.g. "A1 B A2" changed to "A1A2 B").
		/// You can disable this parameter when everything is drawn correctly to save the additional performance cost.
		/// </summary>
		public bool fixDrawOrder = false;
#endif

		/// <summary>If true, the mesh generator adds normals to the output mesh. For better performance and reduced memory requirements, use a shader that assumes the desired normal.</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("calculateNormals")] public bool addNormals = false;

		/// <summary>If true, tangents are calculated every frame and added to the Mesh. Enable this when using a shader that uses lighting that requires tangents.</summary>
		public bool calculateTangents = false;

#if BUILT_IN_SPRITE_MASK_COMPONENT
		/// <summary>This enum controls the mode under which the sprite will interact with the masking system.</summary>
		/// <remarks>Interaction modes with <see cref="UnityEngine.SpriteMask"/> components are identical to Unity's <see cref="UnityEngine.SpriteRenderer"/>,
		/// see https://docs.unity3d.com/ScriptReference/SpriteMaskInteraction.html. </remarks>
		public SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None;

		[System.Serializable]
		public class SpriteMaskInteractionMaterials {
			public bool AnyMaterialCreated {
				get {
					return materialsMaskDisabled.Length > 0 ||
						materialsInsideMask.Length > 0 ||
						materialsOutsideMask.Length > 0;
				}
			}

			/// <summary>Material references for switching material sets at runtime when <see cref="SkeletonRenderer.maskInteraction"/> changes to <see cref="SpriteMaskInteraction.None"/>.</summary>
			public Material[] materialsMaskDisabled = new Material[0];
			/// <summary>Material references for switching material sets at runtime when <see cref="SkeletonRenderer.maskInteraction"/> changes to <see cref="SpriteMaskInteraction.VisibleInsideMask"/>.</summary>
			public Material[] materialsInsideMask = new Material[0];
			/// <summary>Material references for switching material sets at runtime when <see cref="SkeletonRenderer.maskInteraction"/> changes to <see cref="SpriteMaskInteraction.VisibleOutsideMask"/>.</summary>
			public Material[] materialsOutsideMask = new Material[0];
		}
		/// <summary>Material references for switching material sets at runtime when <see cref="SkeletonRenderer.maskInteraction"/> changes.</summary>
		public SpriteMaskInteractionMaterials maskMaterials = new SpriteMaskInteractionMaterials();

		/// <summary>Shader property ID used for the Stencil comparison function.</summary>
		public static readonly int STENCIL_COMP_PARAM_ID = Shader.PropertyToID("_StencilComp");
		/// <summary>Shader property value used as Stencil comparison function for <see cref="SpriteMaskInteraction.None"/>.</summary>
		public const UnityEngine.Rendering.CompareFunction STENCIL_COMP_MASKINTERACTION_NONE = UnityEngine.Rendering.CompareFunction.Always;
		/// <summary>Shader property value used as Stencil comparison function for <see cref="SpriteMaskInteraction.VisibleInsideMask"/>.</summary>
		public const UnityEngine.Rendering.CompareFunction STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE = UnityEngine.Rendering.CompareFunction.LessEqual;
		/// <summary>Shader property value used as Stencil comparison function for <see cref="SpriteMaskInteraction.VisibleOutsideMask"/>.</summary>
		public const UnityEngine.Rendering.CompareFunction STENCIL_COMP_MASKINTERACTION_VISIBLE_OUTSIDE = UnityEngine.Rendering.CompareFunction.Greater;
#if UNITY_EDITOR
		private static bool haveStencilParametersBeenFixed = false;
#endif
#endif // #if BUILT_IN_SPRITE_MASK_COMPONENT
		#endregion

		#region Overrides
#if SPINE_OPTIONAL_RENDEROVERRIDE
		// These are API for anything that wants to take over rendering for a SkeletonRenderer.
		public bool disableRenderingOnOverride = true;
		public delegate void InstructionDelegate (SkeletonRendererInstruction instruction);
		event InstructionDelegate generateMeshOverride;

		/// <summary>Allows separate code to take over rendering for this SkeletonRenderer component. The subscriber is passed a SkeletonRendererInstruction argument to determine how to render a skeleton.</summary>
		public event InstructionDelegate GenerateMeshOverride {
			add {
				generateMeshOverride += value;
				if (disableRenderingOnOverride && generateMeshOverride != null) {
					Initialize(false);
					if (meshRenderer)
						meshRenderer.enabled = false;
					updateMode = UpdateMode.FullUpdate;
				}
			}
			remove {
				generateMeshOverride -= value;
				if (disableRenderingOnOverride && generateMeshOverride == null) {
					Initialize(false);
					if (meshRenderer)
						meshRenderer.enabled = true;
				}
			}
		}

		/// <summary> Occurs after the vertex data is populated every frame, before the vertices are pushed into the mesh.</summary>
		public event Spine.Unity.MeshGeneratorDelegate OnPostProcessVertices;
#endif

#if SPINE_OPTIONAL_MATERIALOVERRIDE
		[System.NonSerialized] readonly Dictionary<Material, Material> customMaterialOverride = new Dictionary<Material, Material>();
		/// <summary>Use this Dictionary to override a Material with a different Material.</summary>
		public Dictionary<Material, Material> CustomMaterialOverride { get { return customMaterialOverride; } }
#endif

		[System.NonSerialized] readonly Dictionary<Slot, Material> customSlotMaterials = new Dictionary<Slot, Material>();
		/// <summary>Use this Dictionary to use a different Material to render specific Slots.</summary>
		public Dictionary<Slot, Material> CustomSlotMaterials { get { return customSlotMaterials; } }
		#endregion

		#region Mesh Generator
		[System.NonSerialized] readonly SkeletonRendererInstruction currentInstructions = new SkeletonRendererInstruction();
		readonly MeshGenerator meshGenerator = new MeshGenerator();
		[System.NonSerialized] readonly MeshRendererBuffers rendererBuffers = new MeshRendererBuffers();

		/// <summary>Returns the <see cref="SkeletonClipping"/> used by this renderer for use with e.g.
		/// <see cref="Skeleton.GetBounds(out float, out float, out float, out float, ref float[], SkeletonClipping)"/>
		/// </summary>
		public SkeletonClipping SkeletonClipping { get { return meshGenerator.SkeletonClipping; } }
		#endregion

		#region Cached component references
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		#endregion

		#region Skeleton
		[System.NonSerialized] public bool valid;
		[System.NonSerialized] public Skeleton skeleton;
		public Skeleton Skeleton {
			get {
				Initialize(false);
				return skeleton;
			}
		}
		#endregion

		#region Physics
		/// <seealso cref="PhysicsPositionInheritanceFactor"/>
		[SerializeField] protected Vector2 physicsPositionInheritanceFactor = Vector2.one;
		/// <seealso cref="PhysicsRotationInheritanceFactor"/>
		[SerializeField] protected float physicsRotationInheritanceFactor = 1.0f;
		/// <summary>Reference transform relative to which physics movement will be calculated, or null to use world location.</summary>
		[SerializeField] protected Transform physicsMovementRelativeTo = null;

		/// <summary>Used for applying Transform translation to skeleton PhysicsConstraints.</summary>
		protected Vector3 lastPosition;
		/// <summary>Used for applying Transform rotation to skeleton PhysicsConstraints.</summary>
		protected float lastRotation;

		/// <summary>When set to non-zero, Transform position movement in X and Y direction
		/// is applied to skeleton PhysicsConstraints, multiplied by this scale factor.
		/// Typical values are <c>Vector2.one</c> to apply XY movement 1:1,
		/// <c>Vector2(2f, 2f)</c> to apply movement with double intensity,
		/// <c>Vector2(1f, 0f)</c> to apply only horizontal movement, or
		/// <c>Vector2.zero</c> to not apply any Transform position movement at all.</summary>
		public Vector2 PhysicsPositionInheritanceFactor {
			get {
				return physicsPositionInheritanceFactor;
			}
			set {
				if (physicsPositionInheritanceFactor == Vector2.zero && value != Vector2.zero) ResetLastPosition();
				physicsPositionInheritanceFactor = value;
			}
		}

		/// <summary>When set to non-zero, Transform rotation movement is applied to skeleton PhysicsConstraints,
		/// multiplied by this scale factor. Typical values are <c>1</c> to apply movement 1:1,
		/// <c>2</c> to apply movement with double intensity, or
		/// <c>0</c> to not apply any Transform rotation movement at all.</summary>
		public float PhysicsRotationInheritanceFactor {
			get {
				return physicsRotationInheritanceFactor;
			}
			set {
				if (physicsRotationInheritanceFactor == 0f && value != 0f) ResetLastRotation();
				physicsRotationInheritanceFactor = value;
			}
		}

		/// <summary>Reference transform relative to which physics movement will be calculated, or null to use world location.</summary>
		public Transform PhysicsMovementRelativeTo {
			get {
				return physicsMovementRelativeTo;
			}
			set {
				physicsMovementRelativeTo = value;
				if (physicsPositionInheritanceFactor != Vector2.zero) ResetLastPosition();
				if (physicsRotationInheritanceFactor != 0f) ResetLastRotation();
			}
		}

		public void ResetLastPosition () {
			lastPosition = GetPhysicsTransformPosition();
		}

		public void ResetLastRotation () {
			lastRotation = GetPhysicsTransformRotation();
		}

		public void ResetLastPositionAndRotation () {
			lastPosition = GetPhysicsTransformPosition();
			lastRotation = GetPhysicsTransformRotation();
		}
		#endregion

		public delegate void SkeletonRendererDelegate (SkeletonRenderer skeletonRenderer);

		/// <summary>OnRebuild is raised after the Skeleton is successfully initialized.</summary>
		public event SkeletonRendererDelegate OnRebuild;

		/// <summary>OnMeshAndMaterialsUpdated is called at the end of LateUpdate after the Mesh and
		/// all materials have been updated.</summary>
		public event SkeletonRendererDelegate OnMeshAndMaterialsUpdated;

		public SkeletonDataAsset SkeletonDataAsset { get { return skeletonDataAsset; } } // ISkeletonComponent

		#region Runtime Instantiation
		public static T NewSpineGameObject<T> (SkeletonDataAsset skeletonDataAsset, bool quiet = false) where T : SkeletonRenderer {
			return SkeletonRenderer.AddSpineComponent<T>(new GameObject("New Spine GameObject"), skeletonDataAsset, quiet);
		}

		/// <summary>Add and prepare a Spine component that derives from SkeletonRenderer to a GameObject at runtime.</summary>
		/// <typeparam name="T">T should be SkeletonRenderer or any of its derived classes.</typeparam>
		public static T AddSpineComponent<T> (GameObject gameObject, SkeletonDataAsset skeletonDataAsset, bool quiet = false) where T : SkeletonRenderer {
			T c = gameObject.AddComponent<T>();
			if (skeletonDataAsset != null) {
				c.skeletonDataAsset = skeletonDataAsset;
				c.Initialize(false, quiet);
			}
			return c;
		}

		/// <summary>Applies MeshGenerator settings to the SkeletonRenderer and its internal MeshGenerator.</summary>
		public void SetMeshSettings (MeshGenerator.Settings settings) {
			this.calculateTangents = settings.calculateTangents;
			this.immutableTriangles = settings.immutableTriangles;
			this.pmaVertexColors = settings.pmaVertexColors;
			this.tintBlack = settings.tintBlack;
			this.useClipping = settings.useClipping;
			this.zSpacing = settings.zSpacing;

			this.meshGenerator.settings = settings;
		}
		#endregion


		public virtual void Awake () {
			Initialize(false);
			if (generateMeshOverride == null || !disableRenderingOnOverride)
				updateMode = updateWhenInvisible;
		}

#if UNITY_EDITOR && CONFIGURABLE_ENTER_PLAY_MODE
		public virtual void Start () {
			Initialize(false);
		}
#endif

#if UNITY_EDITOR
		void OnEnable () {
			if (!Application.isPlaying)
				LateUpdate();
		}
#endif

		void OnDisable () {
			if (clearStateOnDisable && valid)
				ClearState();
		}

		void OnDestroy () {
			rendererBuffers.Dispose();
			valid = false;
		}

		/// <summary>
		/// Clears the previously generated mesh and resets the skeleton's pose.</summary>
		public virtual void ClearState () {
			MeshFilter meshFilter = GetComponent<MeshFilter>();
			if (meshFilter != null) meshFilter.sharedMesh = null;
			currentInstructions.Clear();
			if (skeleton != null) skeleton.SetToSetupPose();
		}

		/// <summary>
		/// Sets a minimum buffer size for the internal MeshGenerator to prevent excess allocations during animation.
		/// </summary>
		public void EnsureMeshGeneratorCapacity (int minimumVertexCount) {
			meshGenerator.EnsureVertexCapacity(minimumVertexCount);
		}

		/// <summary>
		/// Initialize this component. Attempts to load the SkeletonData and creates the internal Skeleton object and buffers.</summary>
		/// <param name="overwrite">If set to <c>true</c>, it will overwrite internal objects if they were already generated. Otherwise, the initialized component will ignore subsequent calls to initialize.</param>
		public virtual void Initialize (bool overwrite, bool quiet = false) {
			if (valid && !overwrite)
				return;
#if UNITY_EDITOR
			if (BuildUtilities.IsInSkeletonAssetBuildPreProcessing)
				return;
#endif
			// Clear
			{
				// Note: do not reset meshFilter.sharedMesh or meshRenderer.sharedMaterial to null,
				// otherwise constant reloading will be triggered at prefabs.
				currentInstructions.Clear();
				rendererBuffers.Clear();
				meshGenerator.Begin();
				skeleton = null;
				valid = false;
			}

			if (skeletonDataAsset == null)
				return;

			SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(quiet);
			if (skeletonData == null) return;
			valid = true;

			meshFilter = GetComponent<MeshFilter>();
			if (meshFilter == null)
				meshFilter = gameObject.AddComponent<MeshFilter>();

			meshRenderer = GetComponent<MeshRenderer>();
			rendererBuffers.Initialize();

			skeleton = new Skeleton(skeletonData) {
				ScaleX = initialFlipX ? -1 : 1,
				ScaleY = initialFlipY ? -1 : 1
			};

			ResetLastPositionAndRotation();

			if (!string.IsNullOrEmpty(initialSkinName) && !string.Equals(initialSkinName, "default", System.StringComparison.Ordinal))
				skeleton.SetSkin(initialSkinName);

			separatorSlots.Clear();
			for (int i = 0; i < separatorSlotNames.Length; i++)
				separatorSlots.Add(skeleton.FindSlot(separatorSlotNames[i]));

			// Generate mesh once, required to update mesh bounds for visibility
			UpdateMode updateModeSaved = updateMode;
			updateMode = UpdateMode.FullUpdate;
			UpdateWorldTransform(Skeleton.Physics.Update);
			LateUpdate();
			updateMode = updateModeSaved;

			if (OnRebuild != null)
				OnRebuild(this);

#if UNITY_EDITOR
			if (!Application.isPlaying) {
				string errorMessage = null;
				if (!quiet && MaterialChecks.IsMaterialSetupProblematic(this, ref errorMessage))
					Debug.LogWarningFormat(this, "Problematic material setup at {0}: {1}", this.name, errorMessage);
			}
#endif
		}

		public virtual void ApplyTransformMovementToPhysics () {
			if (Application.isPlaying) {
				if (physicsPositionInheritanceFactor != Vector2.zero) {
					Vector3 position = GetPhysicsTransformPosition();
					Vector3 positionDelta = position - lastPosition;

					positionDelta = transform.InverseTransformVector(positionDelta);
					if (physicsMovementRelativeTo != null) {
						positionDelta = physicsMovementRelativeTo.TransformVector(positionDelta);
					}
					positionDelta.x *= physicsPositionInheritanceFactor.x;
					positionDelta.y *= physicsPositionInheritanceFactor.y;

					skeleton.PhysicsTranslate(positionDelta.x, positionDelta.y);
					lastPosition = position;
				}
				if (physicsRotationInheritanceFactor != 0f) {
					float rotation = GetPhysicsTransformRotation();
					skeleton.PhysicsRotate(0, 0, physicsRotationInheritanceFactor * (rotation - lastRotation));
					lastRotation = rotation;
				}
			}
		}

		protected Vector3 GetPhysicsTransformPosition () {
			if (physicsMovementRelativeTo == null) {
				return transform.position;
			} else {
				if (physicsMovementRelativeTo == transform.parent)
					return transform.localPosition;
				else
					return physicsMovementRelativeTo.InverseTransformPoint(transform.position);
			}
		}

		protected float GetPhysicsTransformRotation () {
			if (physicsMovementRelativeTo == null) {
				return this.transform.rotation.eulerAngles.z;
			} else {
				if (physicsMovementRelativeTo == this.transform.parent)
					return this.transform.localRotation.eulerAngles.z;
				else {
					Quaternion relative = Quaternion.Inverse(physicsMovementRelativeTo.rotation) * this.transform.rotation;
					return relative.eulerAngles.z;
				}
			}
		}

		protected virtual void UpdateWorldTransform (Skeleton.Physics physics) {
			skeleton.UpdateWorldTransform(physics);
		}

		/// <summary>
		/// Generates a new UnityEngine.Mesh from the internal Skeleton.</summary>
		public virtual void LateUpdate () {
			if (!valid) return;

#if UNITY_EDITOR && NEW_PREFAB_SYSTEM
			// Don't store mesh or material at the prefab, otherwise it will permanently reload
			UnityEditor.PrefabAssetType prefabType = UnityEditor.PrefabUtility.GetPrefabAssetType(this);
			if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) &&
				(prefabType == UnityEditor.PrefabAssetType.Regular || prefabType == UnityEditor.PrefabAssetType.Variant)) {
				return;
			}
			EditorUpdateMeshFilterHideFlags();
#endif

			if (updateMode != UpdateMode.FullUpdate) return;

			LateUpdateMesh();
		}

		public virtual void LateUpdateMesh () {
#if SPINE_OPTIONAL_RENDEROVERRIDE
			bool doMeshOverride = generateMeshOverride != null;
			if ((!meshRenderer || !meshRenderer.enabled) && !doMeshOverride) return;
#else
			const bool doMeshOverride = false;
			if (!meshRenderer.enabled) return;
#endif
			SkeletonRendererInstruction currentInstructions = this.currentInstructions;
			ExposedList<SubmeshInstruction> workingSubmeshInstructions = currentInstructions.submeshInstructions;
			MeshRendererBuffers.SmartMesh currentSmartMesh = rendererBuffers.GetNextMesh(); // Double-buffer for performance.

			bool updateTriangles;

			if (this.singleSubmesh) {
				// STEP 1. Determine a SmartMesh.Instruction. Split up instructions into submeshes. =============================================
				MeshGenerator.GenerateSingleSubmeshInstruction(currentInstructions, skeleton, skeletonDataAsset.atlasAssets[0].PrimaryMaterial);

				// STEP 1.9. Post-process workingInstructions. ==================================================================================
#if SPINE_OPTIONAL_MATERIALOVERRIDE
				if (customMaterialOverride.Count > 0) // isCustomMaterialOverridePopulated
					MeshGenerator.TryReplaceMaterials(workingSubmeshInstructions, customMaterialOverride);
#endif

				// STEP 2. Update vertex buffer based on verts from the attachments. ===========================================================
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

				// STEP 2. Update vertex buffer based on verts from the attachments. ===========================================================
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

			if (OnPostProcessVertices != null) OnPostProcessVertices.Invoke(this.meshGenerator.Buffers);

			// STEP 3. Move the mesh data into a UnityEngine.Mesh ===========================================================================
			Mesh currentMesh = currentSmartMesh.mesh;
			meshGenerator.FillVertexData(currentMesh);

			rendererBuffers.UpdateSharedMaterials(workingSubmeshInstructions);

			bool materialsChanged = rendererBuffers.MaterialsChangedInLastUpdate();
			if (updateTriangles) { // Check if the triangles should also be updated.
				meshGenerator.FillTriangles(currentMesh);
				meshRenderer.sharedMaterials = rendererBuffers.GetUpdatedSharedMaterialsArray();
			} else if (materialsChanged) {
				meshRenderer.sharedMaterials = rendererBuffers.GetUpdatedSharedMaterialsArray();
			}
			if (materialsChanged && (this.maskMaterials.AnyMaterialCreated)) {
				this.maskMaterials = new SpriteMaskInteractionMaterials();
			}

			meshGenerator.FillLateVertexData(currentMesh);

			// STEP 4. The UnityEngine.Mesh is ready. Set it as the MeshFilter's mesh. Store the instructions used for that mesh. ===========
			if (meshFilter)
				meshFilter.sharedMesh = currentMesh;
			currentSmartMesh.instructionUsed.Set(currentInstructions);

#if BUILT_IN_SPRITE_MASK_COMPONENT
			if (meshRenderer != null) {
				AssignSpriteMaskMaterials();
			}
#endif
#if SPINE_OPTIONAL_ON_DEMAND_LOADING
			if (Application.isPlaying)
				HandleOnDemandLoading();
#endif

#if PER_MATERIAL_PROPERTY_BLOCKS
			if (fixDrawOrder && meshRenderer.sharedMaterials.Length > 2) {
				SetMaterialSettingsToFixDrawOrder();
			}
#endif

			if (OnMeshAndMaterialsUpdated != null)
				OnMeshAndMaterialsUpdated(this);
		}

		public virtual void OnBecameVisible () {
			UpdateMode previousUpdateMode = updateMode;
			updateMode = UpdateMode.FullUpdate;

			// OnBecameVisible is called after LateUpdate()
			if (previousUpdateMode != UpdateMode.FullUpdate)
				LateUpdate();
		}

		public void OnBecameInvisible () {
			updateMode = updateWhenInvisible;
		}

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
					string[] originalNames = this.separatorSlotNames;
					foreach (string originalName in originalNames)
						detectedSeparatorNames.Add(originalName);
				}

				this.separatorSlotNames = detectedSeparatorNames.ToArray();
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

#if BUILT_IN_SPRITE_MASK_COMPONENT
		private void AssignSpriteMaskMaterials () {
#if UNITY_EDITOR
			if (!Application.isPlaying && !UnityEditor.EditorApplication.isUpdating) {
				EditorFixStencilCompParameters();
			}
#endif

			if (Application.isPlaying) {
				if (maskInteraction != SpriteMaskInteraction.None && maskMaterials.materialsMaskDisabled.Length == 0)
					maskMaterials.materialsMaskDisabled = meshRenderer.sharedMaterials;
			}

			if (maskMaterials.materialsMaskDisabled.Length > 0 && maskMaterials.materialsMaskDisabled[0] != null &&
				maskInteraction == SpriteMaskInteraction.None) {
				this.meshRenderer.materials = maskMaterials.materialsMaskDisabled;
			} else if (maskInteraction == SpriteMaskInteraction.VisibleInsideMask) {
				if (maskMaterials.materialsInsideMask.Length == 0 || maskMaterials.materialsInsideMask[0] == null) {
					if (!InitSpriteMaskMaterialsInsideMask())
						return;
				}
				this.meshRenderer.materials = maskMaterials.materialsInsideMask;
			} else if (maskInteraction == SpriteMaskInteraction.VisibleOutsideMask) {
				if (maskMaterials.materialsOutsideMask.Length == 0 || maskMaterials.materialsOutsideMask[0] == null) {
					if (!InitSpriteMaskMaterialsOutsideMask())
						return;
				}
				this.meshRenderer.materials = maskMaterials.materialsOutsideMask;
			}
		}

		private bool InitSpriteMaskMaterialsInsideMask () {
			return InitSpriteMaskMaterialsForMaskType(STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE, ref maskMaterials.materialsInsideMask);
		}

		private bool InitSpriteMaskMaterialsOutsideMask () {
			return InitSpriteMaskMaterialsForMaskType(STENCIL_COMP_MASKINTERACTION_VISIBLE_OUTSIDE, ref maskMaterials.materialsOutsideMask);
		}

		private bool InitSpriteMaskMaterialsForMaskType (UnityEngine.Rendering.CompareFunction maskFunction, ref Material[] materialsToFill) {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				return false;
			}
#endif
			Material[] originalMaterials = maskMaterials.materialsMaskDisabled;
			materialsToFill = new Material[originalMaterials.Length];
			for (int i = 0; i < originalMaterials.Length; i++) {
				Material originalMaterial = originalMaterials[i];
				if (originalMaterial == null) {
					materialsToFill[i] = null;
					continue;
				}
				Material newMaterial = new Material(originalMaterial);
				newMaterial.SetFloat(STENCIL_COMP_PARAM_ID, (int)maskFunction);
				materialsToFill[i] = newMaterial;
			}
			return true;
		}

#if UNITY_EDITOR
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
					Material material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
					if (material.HasProperty(STENCIL_COMP_PARAM_ID) && material.GetFloat(STENCIL_COMP_PARAM_ID) == 0) {
						material.SetFloat(STENCIL_COMP_PARAM_ID, (int)STENCIL_COMP_MASKINTERACTION_NONE);
					}
				}
			}
			UnityEditor.AssetDatabase.Refresh();
			UnityEditor.AssetDatabase.SaveAssets();
		}

		private bool HasAnyStencilComp0Material () {
			if (meshRenderer == null)
				return false;

			foreach (Material material in meshRenderer.sharedMaterials) {
				if (material != null && material.HasProperty(STENCIL_COMP_PARAM_ID)) {
					float currentCompValue = material.GetFloat(STENCIL_COMP_PARAM_ID);
					if (currentCompValue == 0)
						return true;
				}
			}
			return false;
		}
#endif // UNITY_EDITOR

#endif //#if BUILT_IN_SPRITE_MASK_COMPONENT

#if SPINE_OPTIONAL_ON_DEMAND_LOADING
		void HandleOnDemandLoading () {
			foreach (AtlasAssetBase atlasAsset in skeletonDataAsset.atlasAssets) {
				if (atlasAsset.TextureLoadingMode != AtlasAssetBase.LoadingMode.Normal) {
					atlasAsset.BeginCustomTextureLoading();
					for (int i = 0, count = meshRenderer.sharedMaterials.Length; i < count; ++i) {
						Material overrideMaterial = null;
						atlasAsset.RequireTexturesLoaded(meshRenderer.sharedMaterials[i], ref overrideMaterial);
						if (overrideMaterial != null)
							meshRenderer.sharedMaterials[i] = overrideMaterial;
					}
					atlasAsset.EndCustomTextureLoading();
				}
			}
		}
#endif

#if PER_MATERIAL_PROPERTY_BLOCKS
		private MaterialPropertyBlock reusedPropertyBlock;
		public static readonly int SUBMESH_DUMMY_PARAM_ID = Shader.PropertyToID("_Submesh");

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
	}
}
