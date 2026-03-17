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

#if UNITY_2018_2_OR_NEWER
#define HAS_CULL_TRANSPARENT_MESH
#endif

#if !SPINE_DISABLE_THREADING
#define USE_THREADED_SKELETON_UPDATE
#endif

#if !SPINE_AUTO_UPGRADE_COMPONENTS_OFF
#define AUTO_UPGRADE_TO_43_COMPONENTS
#endif

#define SPINE_OPTIONAL_ON_DEMAND_LOADING

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Spine.Unity {
	[System.Serializable]
	public class SkeletonGraphicParams {

	}

	[DefaultExecutionOrder(1)]
#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(CanvasRenderer), typeof(RectTransform)), DisallowMultipleComponent]
	[AddComponentMenu("Spine/SkeletonGraphic (Unity UI Canvas)")]
	[HelpURL("https://esotericsoftware.com/spine-unity-main-components#SkeletonGraphic-Component")]
	public partial class SkeletonGraphic : MaskableGraphic, ISkeletonRenderer, IUpgradable {
		// Note: Partial class. As a workaround for single inheritance limitations, this class
		// is split into a common code part (what would be the base class) and a specific code part.
		// This file covers specific attributes, properties and methods.
		// Common ISkeletonRenderer members can be found in SkeletonGraphic.Common.cs.

		#region Events
		/// <summary>OnMeshAndMaterialsUpdated is raised at the end of <c>Rebuild</c> after the Mesh and
		/// all materials have been updated. Note that some Unity API calls are not permitted to be issued from
		/// <c>Rebuild</c>, so you may want to subscribe to <see cref="OnInstructionsPrepared"/> instead
		/// from where you can issue such preparation calls.</summary>
		public event SkeletonRendererDelegate OnMeshAndMaterialsUpdated;
		#endregion Events

		#region Types and Constants
		public enum LayoutMode {
			None = 0,
			WidthControlsHeight,
			HeightControlsWidth,
			FitInParent,
			EnvelopeParent
		}

		public const string SeparatorPartGameObjectName = "Part";
		#endregion

		#region Attributes
		/// <summary>Own color to replace <c>Graphic.m_Color</c>.</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("m_Color")]
		[SerializeField] protected Color m_SkeletonColor = Color.white;

		public bool freeze;

		public bool allowMultipleCanvasRenderers = false;
		public bool updateSeparatorPartLocation = true;
		public bool updateSeparatorPartScale = false;

		public Material additiveMaterial;
		public Material multiplyMaterial;
		public Material screenMaterial;

		// Layout
		protected float meshScale = 1f;
		protected Vector2 meshOffset = Vector2.zero;
		public SkeletonGraphic.LayoutMode layoutScaleMode = SkeletonGraphic.LayoutMode.None;
		[SerializeField] protected Vector2 referenceSize = Vector2.one;
		/// <summary>Offset relative to the pivot position, before potential layout scale is applied.</summary>
		[SerializeField] protected Vector2 pivotOffset = Vector2.zero;
		[SerializeField] protected float referenceScale = 1f;
		[SerializeField] protected float layoutScale = 1f;
#if UNITY_EDITOR
		internal SkeletonGraphic.LayoutMode previousLayoutScaleMode = SkeletonGraphic.LayoutMode.None;
		[SerializeField] protected Vector2 rectTransformSize = Vector2.zero;
		[SerializeField] protected bool editReferenceRect = false;
		protected bool previousEditReferenceRect = false;
#endif
#if USE_THREADED_SKELETON_UPDATE
		protected Vector2 threadedRectTransformSize = Vector2.zero;
		protected float canvasReferencePixelsPerUnit = 100f;
#endif

		// Mesh Generation
#if USE_THREADED_SKELETON_UPDATE
		protected ExposedList<MeshGenerator> meshGenerators = new ExposedList<MeshGenerator> { null };
#else
		protected ExposedList<MeshGenerator> meshGenerators = new ExposedList<MeshGenerator> { new MeshGenerator() };
#endif

		[System.NonSerialized] readonly MeshRendererBuffers rendererBuffers = new MeshRendererBuffers();
		public List<CanvasRenderer> canvasRenderers = new List<CanvasRenderer>();
		protected List<SkeletonSubmeshGraphic> submeshGraphics = new List<SkeletonSubmeshGraphic>();
		protected int usedRenderersCount = 0;

		/// <summary>Multiple mesh targets for generating each mesh into a separate Mesh instead of into multiple submeshes.</summary>
		protected readonly ExposedList<Mesh> meshes = new ExposedList<Mesh>();

		readonly ExposedList<Texture> usedTextures = new ExposedList<Texture>();
		[System.NonSerialized] readonly Dictionary<Texture, Texture> customTextureOverride = new Dictionary<Texture, Texture>();
		[System.NonSerialized] readonly Dictionary<Texture, Material> customMaterialOverride = new Dictionary<Texture, Material>();

		[SerializeField] protected List<Transform> separatorParts = new List<Transform>();

		/// <summary>When true, no meshes and materials are assigned at CanvasRenderers if the used override
		/// AssignMeshOverrideSingleRenderer or AssignMeshOverrideMultipleRenderers is non-null.</summary>
		public bool disableMeshAssignmentOnOverride = true;
		/// <summary>Delegate type for overriding mesh and material assignment,
		/// used when <c>allowMultipleCanvasRenderers</c> is false.</summary>
		/// <param name="mesh">Mesh normally assigned at the main CanvasRenderer.</param>
		/// <param name="graphicMaterial">Material normally assigned at the main CanvasRenderer.</param>
		/// <param name="texture">Texture normally assigned at the main CanvasRenderer.</param>
		public delegate void MeshAssignmentDelegateSingle (Mesh mesh, Material graphicMaterial, Texture texture);
		/// <param name="meshCount">Number of meshes. Don't use <c>meshes.Length</c> as this might be higher
		/// due to pre-allocated entries.</param>
		/// <param name="meshes">Mesh array where each element is normally assigned to one of the <c>canvasRenderers</c>.</param>
		/// <param name="graphicMaterials">Material array where each element is normally assigned to one of the <c>canvasRenderers</c>.</param>
		/// <param name="textures">Texture array where each element is normally assigned to one of the <c>canvasRenderers</c>.</param>
		public delegate void MeshAssignmentDelegateMultiple (int meshCount, Mesh[] meshes, Material[] graphicMaterials, Texture[] textures);
		event MeshAssignmentDelegateSingle assignMeshOverrideSingle;
		event MeshAssignmentDelegateMultiple assignMeshOverrideMultiple;
		#endregion Attributes

		#region Properties
		/// <summary>Sets the color of the skeleton. Does not call <see cref="Rebuild"/> and <see cref="UpdateMesh"/>
		/// unnecessarily as <c>Graphic.color</c> would otherwise do.</summary>
		override public Color color { get { return m_SkeletonColor; } set { m_SkeletonColor = value; } }

		public override Texture mainTexture {
			get {
				if (usedTextures.Items.Length > 0)
					return usedTextures.Items[0];
				else
					return base.mainTexture;
			}
		}

		public Texture OverrideTexture {
			get {
				Texture overrideTexture;
				customTextureOverride.TryGetValue(Texture2D.whiteTexture, out overrideTexture);
				return overrideTexture;
			}
			set {
				customTextureOverride.Add(Texture2D.whiteTexture, value);
				SetMainRendererTexture(value);
				materialsNeedUpdate = true;
			}
		}

		protected bool HasMaterialOrTextureOverride {
			get { return (customMaterialOverride.Count > 0 || customTextureOverride.Count > 0); }
		}

		/// <summary>Use this Dictionary to override a Texture with a different Texture.</summary>
		public Dictionary<Texture, Texture> CustomTextureOverride {
			get { materialsNeedUpdate = true; return customTextureOverride; }
		}

		/// <summary>Use this Dictionary to override the Material where the Texture was used at the original atlas.</summary>
		public Dictionary<Texture, Material> CustomMaterialOverride {
			get { materialsNeedUpdate = true; return customMaterialOverride; }
		}

		public List<Transform> SeparatorParts { get { return separatorParts; } }

		/// <summary>Allows separate code to take over mesh and material assignment for this SkeletonGraphic component.
		/// Used when <c>allowMultipleCanvasRenderers</c> is false.</summary>
		public event MeshAssignmentDelegateSingle AssignMeshOverrideSingleRenderer {
			add {
				assignMeshOverrideSingle += value;
				if (disableMeshAssignmentOnOverride && assignMeshOverrideSingle != null) {
					Initialize(false);
				}
			}
			remove {
				assignMeshOverrideSingle -= value;
				if (disableMeshAssignmentOnOverride && assignMeshOverrideSingle == null) {
					Initialize(false);
				}
			}
		}
		/// <summary>Allows separate code to take over mesh and material assignment for this SkeletonGraphic component.
		/// Used when <c>allowMultipleCanvasRenderers</c> is true.</summary>
		public event MeshAssignmentDelegateMultiple AssignMeshOverrideMultipleRenderers {
			add {
				assignMeshOverrideMultiple += value;
				if (disableMeshAssignmentOnOverride && assignMeshOverrideMultiple != null) {
					Initialize(false);
				}
			}
			remove {
				assignMeshOverrideMultiple -= value;
				if (disableMeshAssignmentOnOverride && assignMeshOverrideMultiple == null) {
					Initialize(false);
				}
			}
		}
		public ExposedList<Mesh> MeshesMultipleCanvasRenderers { get { return meshes; } }
		public Material[] MaterialsMultipleCanvasRenderers { get { return rendererBuffers.sharedMaterials; } }
		public ExposedList<Texture> TexturesMultipleCanvasRenderers { get { return usedTextures; } }

#if USE_THREADED_SKELETON_UPDATE
		public float CanvasReferencePixelsPerUnit {
			get {
				return canvasReferencePixelsPerUnit;
			}
		}

		public virtual bool NeedsMainThreadRendererPreparation {
			get {
				return allowMultipleCanvasRenderers || canvasRenderers.Count > 0 ||
				  generateMeshOverride != null || OnInstructionsPrepared != null;
			}
		}
#else
		public float CanvasReferencePixelsPerUnit {
			get {
				return (canvas == null) ? 100 : canvas.referencePixelsPerUnit;
			}
		}
#endif

		/// <summary>Returns the <see cref="SkeletonClipping"/> used by this renderer for use with e.g.
		/// <see cref="Skeleton.GetBounds(out float, out float, out float, out float, ref float[], SkeletonClipping)"/>
		/// </summary>
		public SkeletonClipping SkeletonClipping { get { return meshGenerators.Items[0].SkeletonClipping; } }
		public bool Freeze { get { return freeze; } set { freeze = value; } }
		public float MeshScale { get { return meshScale; } }
		public Vector2 MeshOffset { get { return meshOffset; } }
		protected bool UsesSingleSubmesh { get { return !allowMultipleCanvasRenderers; } }
		protected bool NeedsToGenerateMesh { get { return !freeze; } }
#if UNITY_EDITOR
		public bool EditReferenceRect { get { return editReferenceRect; } set { editReferenceRect = value; } }
		public Vector2 RectTransformSize { get { return rectTransformSize; } }
#else
		protected const bool EditReferenceRect = false;
#endif
		#endregion Properties

		#region Methods
		#region Lifecycle
		protected override void Awake () {
			base.Awake();

			InitializeMainThreadID();
#if UNITY_EDITOR
			SkeletonGraphic.ApplicationIsPlaying = Application.isPlaying;
#endif
			MeshGenerator.InitializeGlobalSettings();
#if USE_THREADED_SKELETON_UPDATE
			if (meshGenerators.Items[0] == null)
				meshGenerators.Items[0] = new MeshGenerator();
			canvasReferencePixelsPerUnit = (canvas == null) ? 100 : canvas.referencePixelsPerUnit;
#endif
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
			if (!Application.isPlaying && !wasDeprecatedTransferred) {
				UpgradeTo43();
			}
#endif
			this.onCullStateChanged.AddListener(OnCullStateChanged);
			this.m_OnDirtyMaterialCallback += () => { materialsNeedUpdate = true; };

			SyncSubmeshGraphicsWithCanvasRenderers();
			if (!valid) {
#if UNITY_EDITOR
				// workaround for special import case of open scene where OnValidate and Awake are
				// called in wrong order, before setup of Spine assets.
				if (!Application.isPlaying) {
					if (this.skeletonDataAsset != null && this.skeletonDataAsset.skeletonJSON == null)
						return;
				}
#endif
				Initialize(false);
				updateMode = updateWhenInvisible;
				if (!valid)
					return;
			}
			Rebuild(CanvasUpdate.PreRender);
#if UNITY_EDITOR
			InitLayoutScaleParameters();
#endif
		}

		public void Initialize (bool overwrite, bool quiet = false) {
			if (valid && !overwrite)
				return;

			skeletonAnimation = this.GetComponent<ISkeletonAnimation>();
			if (skeletonAnimation != null)
				skeletonAnimation.EnsureRendererEventsSubscribed();

			InitializeCommon(overwrite, quiet);
			SetMaterialDirty();

			if (valid) {
				EnsureUsedTexturesCount(1);
				usedTextures.Items[0] = skeletonDataAsset.atlasAssets[0].PrimaryMaterial.mainTexture;
				canvasRenderer.SetTexture(this.mainTexture); // Needed for overwriting initializations.
			}

#if UNITY_EDITOR
			requiresEditorUpdate = false;
#endif
		}

		public void Clear () {
			ClearMeshAtRenderer();
			ClearCommon();
			DestroyMeshes();
			usedTextures.Clear();
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

		protected override void OnEnable () {
			base.OnEnable();

#if USE_THREADED_SKELETON_UPDATE
			if (Application.isPlaying && UsesThreadedMeshGeneration && !isUpdatedExternally) {
				SkeletonUpdateSystem system = SkeletonUpdateSystem.Instance;
				if (system)
					system.RegisterForUpdate(this);
			}
#endif
		}

		protected override void OnDisable () {
			base.OnDisable();

#if USE_THREADED_SKELETON_UPDATE
			if (Application.isPlaying && UsesThreadedMeshGeneration) {
				SkeletonUpdateSystem system = SkeletonUpdateSystem.Instance;
				if (system)
					system.UnregisterFromUpdate(this);
			}
#endif

			if (clearStateOnDisable && valid)
				ClearState();
			foreach (CanvasRenderer canvasRenderer in canvasRenderers) {
				canvasRenderer.Clear();
			}
		}

		protected override void OnDestroy () {
			ClearSkeletonState();
			rendererBuffers.Dispose();
			valid = false;
		}

		protected void OnCullStateChanged (bool culled) {
			if (culled)
				OnBecameInvisible();
			else
				OnBecameVisible();
		}

		protected void OnBecameVisible () {
			UpdateMode previousUpdateMode = updateMode;
			updateMode = UpdateMode.FullUpdate;
			// OnBecameVisible is called after Update and LateUpdate()
			if (previousUpdateMode != UpdateMode.FullUpdate) {
				if (skeletonAnimation != null)
					skeletonAnimation.UpdateOncePerFrame(0);
				LateUpdate();
			}
		}

		protected void OnBecameInvisible () {
			updateMode = updateWhenInvisible;
		}

		public void LateUpdate () {
#if USE_THREADED_SKELETON_UPDATE
			if (isUpdatedExternally) return;
#endif
			LateUpdateImplementation();
		}

		/// <summary>Triggered by SetVerticesDirty and SetMaterialDirty, avoiding duplicate update calls.</summary>
		public override void Rebuild (CanvasUpdate update) {
			base.Rebuild(update);
			if (!valid) return;
			if (canvasRenderer.cull) return;
			if (update == CanvasUpdate.PreRender) {
				PrepareInstructionsAndRenderers(isInRebuild: true);
				updateTriangles = UpdateBuffersToInstructions(true);
#if USE_THREADED_SKELETON_UPDATE
				requiresMeshBufferAssignmentMainThread = false;
				SetCurrentRectSize();
#endif
				UpdateMeshAndMaterialsToBuffers();
			}
			if (allowMultipleCanvasRenderers) canvasRenderer.Clear();
		}
		#endregion Lifecycle

		#region Layout
		public bool GetMeshBoundsSingleRenderer (ref Bounds bounds) {
			Mesh mesh = GetCurrentMesh();
			if (mesh == null || mesh.vertexCount == 0 || mesh.bounds.size == Vector3.zero) {
				return false;
			}
			mesh.RecalculateBounds();
			bounds = mesh.bounds;
			return true;
		}

		public bool GetMeshBoundsMultipleRenderers (ref Bounds bounds) {
			bool anyBoundsAdded = false;
			Mesh[] meshItems = meshes.Items;
			Bounds combinedBounds = new Bounds();
			for (int i = 0; i < meshes.Count; ++i) {
				Mesh mesh = meshItems[i];
				if (mesh == null || mesh.vertexCount == 0)
					continue;

				mesh.RecalculateBounds();
				Bounds meshBounds = mesh.bounds;
				if (anyBoundsAdded)
					combinedBounds.Encapsulate(meshBounds);
				else {
					anyBoundsAdded = true;
					combinedBounds = meshBounds;
				}
			}

			if (!anyBoundsAdded || combinedBounds.size == Vector3.zero) {
				return false;
			}
			bounds = combinedBounds;
			return true;
		}

		public bool MatchRectTransformWithBounds () {
			if (skeletonAnimation != null)
				skeletonAnimation.UpdateOncePerFrame(0);
			UpdateMesh();
			Vector2 defaultSize = new Vector2(50f, 50f);
			Bounds bounds = new Bounds(defaultSize * 0.5f, defaultSize);
			bool boundsSet;
			if (!allowMultipleCanvasRenderers)
				boundsSet = GetMeshBoundsSingleRenderer(ref bounds);
			else
				boundsSet = GetMeshBoundsMultipleRenderers(ref bounds);
			SetRectTransformBounds(bounds);
			return boundsSet;
		}

		private void SetRectTransformBounds (Bounds combinedBounds) {
			Vector3 size = combinedBounds.size;
			Vector3 center = combinedBounds.center;
			Vector2 p = new Vector2(
				0.5f - (center.x / size.x),
				0.5f - (center.y / size.y)
			);

			SetRectTransformSize(this, size);
			this.rectTransform.pivot = p;

			foreach (Transform separatorPart in separatorParts) {
				RectTransform separatorTransform = separatorPart.GetComponent<RectTransform>();
				if (separatorTransform) {
					SetRectTransformSize(separatorTransform, size);
					separatorTransform.pivot = p;
				}
			}
			foreach (SkeletonSubmeshGraphic submeshGraphic in submeshGraphics) {
				SetRectTransformSize(submeshGraphic, size);
				submeshGraphic.rectTransform.pivot = p;
			}

			this.referenceSize = size;
			referenceScale = referenceScale * layoutScale;
			layoutScale = 1f;
		}

		public static void SetRectTransformSize (Graphic target, Vector2 size) {
			SetRectTransformSize(target.rectTransform, size);
		}

		public static void SetRectTransformSize (RectTransform targetRectTransform, Vector2 size) {
			Vector2 parentSize = Vector2.zero;
			if (targetRectTransform.parent != null) {
				RectTransform parentTransform = targetRectTransform.parent.GetComponent<RectTransform>();
				if (parentTransform)
					parentSize = parentTransform.rect.size;
			}
			Vector2 anchorAreaSize = Vector2.Scale(targetRectTransform.anchorMax - targetRectTransform.anchorMin, parentSize);
			targetRectTransform.sizeDelta = size - anchorAreaSize;
		}

		public void SetScaledPivotOffset (Vector2 pivotOffsetScaled) {
			pivotOffset = pivotOffsetScaled / GetLayoutScale(layoutScaleMode);
		}

		protected float GetLayoutScale (LayoutMode mode) {
			Vector2 currentSize = GetCurrentRectSize();
			mode = GetEffectiveLayoutMode(mode);

			if (mode == LayoutMode.WidthControlsHeight) {
				return currentSize.x / referenceSize.x;
			} else if (mode == LayoutMode.HeightControlsWidth) {
				return currentSize.y / referenceSize.y;
			}
			return 1f;
		}

		/// <summary>
		/// <c>LayoutMode FitInParent</c> and <c>EnvelopeParent</c> actually result in
		/// <c>HeightControlsWidth</c> or <c>WidthControlsHeight</c> depending on the actual vs reference aspect ratio.
		/// This method returns the respective <c>LayoutMode</c> of the two for any given input <c>mode</c>.
		/// </summary>
		protected LayoutMode GetEffectiveLayoutMode (LayoutMode mode) {
			Vector2 currentSize = GetCurrentRectSize();
			float referenceAspect = referenceSize.x / referenceSize.y;
			float frameAspect = currentSize.x / currentSize.y;
			if (mode == LayoutMode.FitInParent)
				mode = frameAspect > referenceAspect ? LayoutMode.HeightControlsWidth : LayoutMode.WidthControlsHeight;
			else if (mode == LayoutMode.EnvelopeParent)
				mode = frameAspect > referenceAspect ? LayoutMode.WidthControlsHeight : LayoutMode.HeightControlsWidth;
			return mode;
		}

#if USE_THREADED_SKELETON_UPDATE
		public void SetCurrentRectSize () {
			threadedRectTransformSize = this.rectTransform.rect.size; ;
		}

		private Vector2 GetCurrentRectSize () {
			if (!IsUpdatedExternally)
				return this.rectTransform.rect.size;
			else
				return threadedRectTransformSize;
		}
#else
		private Vector2 GetCurrentRectSize () {
			return this.rectTransform.rect.size;
		}
#endif
		#endregion Layout

		#region Mesh Generation
		protected void ClearMeshGenerator () {
#if USE_THREADED_SKELETON_UPDATE
			if (meshGenerators.Items[0] == null)
				meshGenerators.Items[0] = new MeshGenerator();
#endif
			meshGenerators.Items[0].Begin();
		}

		public Mesh GetCurrentMesh () {
			return rendererBuffers.GetCurrentMesh().mesh;
		}

		public bool HasMultipleSubmeshInstructions () {
			return RequiresMultipleSubmeshesByDrawOrder();
		}

		protected bool RequiresMultipleSubmeshesByDrawOrder () {
			if (!valid)
				return false;
			return MeshGenerator.RequiresMultipleSubmeshesByDrawOrder(skeleton);
		}

		public void PrepareInstructionsAndRenderers (bool isInRebuild = false) {
			if (UsesSingleSubmesh) {
				MeshGenerator.GenerateSingleSubmeshInstruction(currentInstructions, skeleton, skeletonDataAsset.atlasAssets[0].PrimaryMaterial);
				if (canvasRenderers.Count > 0)
					DisableUnusedCanvasRenderers(usedCount: 0, isInRebuild: isInRebuild);
				usedRenderersCount = 0;
			} else {
				GenerateSkeletonRendererInstructions();

				int submeshCount = currentInstructions.submeshInstructions.Count;
				EnsureCanvasRendererCount(submeshCount);
				EnsureMeshesCount(submeshCount);
				EnsureUsedTexturesCount(submeshCount);
				EnsureSeparatorPartCount();
				PrepareRendererGameObjects(currentInstructions, isInRebuild);
			}
			if (OnInstructionsPrepared != null)
				OnInstructionsPrepared(this.currentInstructions);
		}

		protected virtual void GenerateSkeletonRendererInstructions () {
			MeshGenerator.GenerateSkeletonRendererInstruction(currentInstructions, skeleton, customSlotMaterials,
				enableSeparatorSlots ? separatorSlots : null,
				enableSeparatorSlots ? separatorSlots.Count > 0 : false,
				false);
		}

		protected void EnsureCanvasRendererCount (int targetCount) {
#if UNITY_EDITOR
			RemoveNullCanvasRenderers();
			if (!ApplicationIsPlaying) {
				if (canvasRenderers.Count != submeshGraphics.Count)
					SyncSubmeshGraphicsWithCanvasRenderers();
			}
#endif
			int currentCount = canvasRenderers.Count;
			for (int i = currentCount; i < targetCount; ++i) {
				GameObject go = new GameObject(string.Format("Renderer{0}", i), typeof(RectTransform));
				go.transform.SetParent(this.transform, false);
				go.transform.localPosition = Vector3.zero;
				CanvasRenderer canvasRenderer = go.AddComponent<CanvasRenderer>();
				canvasRenderers.Add(canvasRenderer);
				SkeletonSubmeshGraphic submeshGraphic = go.AddComponent<SkeletonSubmeshGraphic>();
				submeshGraphic.maskable = this.maskable;
				submeshGraphic.raycastTarget = false;
				submeshGraphic.rectTransform.pivot = rectTransform.pivot;
				submeshGraphic.rectTransform.anchorMin = Vector2.zero;
				submeshGraphic.rectTransform.anchorMax = Vector2.one;
				submeshGraphic.rectTransform.sizeDelta = Vector2.zero;
				submeshGraphics.Add(submeshGraphic);
			}
		}

		protected void EnsureMeshesCount (int targetCount) {
			int oldCount = meshes.Count;
			meshes.EnsureCapacity(targetCount);
			for (int i = oldCount; i < targetCount; i++) {
				meshes.Add(SpineMesh.NewSkeletonMesh());
			}
		}

		protected void DestroyMeshes () {
			foreach (Mesh mesh in meshes) {
#if UNITY_EDITOR
				if (!ApplicationIsPlaying)
					UnityEngine.Object.DestroyImmediate(mesh);
				else
					UnityEngine.Object.Destroy(mesh);
#else
				UnityEngine.Object.Destroy(mesh);
#endif
			}
			meshes.Clear();
		}

		protected void EnsureGeneratorCount (int targetCount) {
			int oldCount = meshGenerators.Count;
			meshGenerators.EnsureCapacity(targetCount);
			for (int i = oldCount; i < targetCount; i++) {
				meshGenerators.Add(new MeshGenerator());
			}
		}

		protected void EnsureUsedTexturesCount (int targetCount) {
			int oldCount = usedTextures.Count;
			usedTextures.EnsureCapacity(targetCount);
			for (int i = oldCount; i < targetCount; i++) {
				usedTextures.Add(null);
			}
		}

		protected void PrepareRendererGameObjects (SkeletonRendererInstruction currentInstructions,
			bool isInRebuild = false) {

			int submeshCount = currentInstructions.submeshInstructions.Count;
			DisableUnusedCanvasRenderers(usedCount: submeshCount, isInRebuild: isInRebuild);

			Transform parent = this.separatorParts.Count == 0 ? this.transform : this.separatorParts[0];
			if (updateSeparatorPartLocation) {
				for (int p = 0; p < this.separatorParts.Count; ++p) {
					Transform separatorPart = separatorParts[p];
					if (separatorPart == null) continue;
					separatorPart.position = this.transform.position;
					separatorPart.rotation = this.transform.rotation;
				}
			}
			if (updateSeparatorPartScale) {
				Vector3 targetScale = this.transform.lossyScale;
				for (int p = 0; p < this.separatorParts.Count; ++p) {
					Transform separatorPart = separatorParts[p];
					if (separatorPart == null) continue;
					Transform partParent = separatorPart.parent;
					Vector3 parentScale = partParent == null ? Vector3.one : partParent.lossyScale;
					separatorPart.localScale = new Vector3(
						parentScale.x == 0f ? 1f : targetScale.x / parentScale.x,
						parentScale.y == 0f ? 1f : targetScale.y / parentScale.y,
						parentScale.z == 0f ? 1f : targetScale.z / parentScale.z);
				}
			}

			int separatorSlotGroupIndex = 0;
			int targetSiblingIndex = 0;
			for (int i = 0; i < submeshCount; i++) {
				CanvasRenderer canvasRenderer = canvasRenderers[i];
				if (canvasRenderer != null) {
					if (i >= usedRenderersCount)
						canvasRenderer.gameObject.SetActive(true);

					if (canvasRenderer.transform.parent != parent.transform && !isInRebuild)
						canvasRenderer.transform.SetParent(parent.transform, false);

					canvasRenderer.transform.SetSiblingIndex(targetSiblingIndex++);
				}

				SkeletonSubmeshGraphic submeshGraphic = submeshGraphics[i];
				if (submeshGraphic != null) {
					RectTransform dstTransform = submeshGraphic.rectTransform;
					dstTransform.localPosition = Vector3.zero;
					dstTransform.pivot = rectTransform.pivot;
					dstTransform.anchorMin = Vector2.zero;
					dstTransform.anchorMax = Vector2.one;
					dstTransform.sizeDelta = Vector2.zero;
				}

				SubmeshInstruction submeshInstructionItem = currentInstructions.submeshInstructions.Items[i];
				if (submeshInstructionItem.forceSeparate) {
					targetSiblingIndex = 0;
					parent = separatorParts[++separatorSlotGroupIndex];
				}
			}
			usedRenderersCount = submeshCount;
		}

		protected void DisableUnusedCanvasRenderers (int usedCount, bool isInRebuild = false) {
#if UNITY_EDITOR
			RemoveNullCanvasRenderers();
#endif
			for (int i = usedCount; i < canvasRenderers.Count; i++) {
				canvasRenderers[i].Clear();
				if (!isInRebuild) // rebuild does not allow disabling Graphic and thus removing it from rebuild list.
					canvasRenderers[i].gameObject.SetActive(false);
			}
		}

#if UNITY_EDITOR
		private void RemoveNullCanvasRenderers () {
			if (!ApplicationIsPlaying) {
				for (int i = canvasRenderers.Count - 1; i >= 0; --i) {
					if (canvasRenderers[i] == null) {
						canvasRenderers.RemoveAt(i);
						submeshGraphics.RemoveAt(i);
					}
				}
			}
		}

		private void DestroyOldRawImages () {
			foreach (CanvasRenderer canvasRenderer in canvasRenderers) {
				RawImage oldRawImage = canvasRenderer.GetComponent<RawImage>();
				if (oldRawImage != null) {
					DestroyImmediate(oldRawImage);
				}
			}
		}
#endif

		/// <returns>True if any mesh has been filled with data, false otherwise.</returns>
		protected virtual bool FillBuffersFromSubmeshInstructions (ExposedList<SubmeshInstruction> workingSubmeshInstructions,
			MeshRendererBuffers.SmartMesh currentSmartMesh, bool updateTriangles) {

			if (UsesSingleSubmesh) {
				return FillSingleBufferFromInstructions(workingSubmeshInstructions, currentSmartMesh, updateTriangles);
			} else {
				int submeshCount = currentInstructions.submeshInstructions.Count;
#if UNITY_EDITOR
				if (!ApplicationIsPlaying) {
					if (canvasRenderers.Count != submeshGraphics.Count)
						SyncSubmeshGraphicsWithCanvasRenderers();
				}
#endif
				currentSmartMesh.instructionUsed.Set(currentInstructions);
				EnsureGeneratorCount(submeshCount);

				// Generate meshes.
				UpdateMeshScaleAndOffset();

				for (int i = 0; i < submeshCount; i++) {
					MeshGenerator meshGenerator = meshGenerators.Items[i];
					meshGenerator.settings = meshSettings;

					SubmeshInstruction submeshInstructionItem = currentInstructions.submeshInstructions.Items[i];
					meshGenerator.Begin();
					meshGenerator.AddSubmesh(submeshInstructionItem);
					ScaleAndOffsetGeneratorMesh(i);

					IssueOnPostProcessVertices(meshGenerator.Buffers);
				}
				return true;
			}
		}

		/// <returns>True if any mesh has been filled with data, false otherwise.</returns>
		protected virtual bool FillSingleBufferFromInstructions (ExposedList<SubmeshInstruction> workingSubmeshInstructions,
			MeshRendererBuffers.SmartMesh currentSmartMesh, bool updateTriangles) {

			UpdateMeshScaleAndOffset();
			MeshGenerator meshGenerator = meshGenerators.Items[0];
			meshGenerator.settings = meshSettings;
			meshGenerator.Begin();

			if (!currentInstructions.hasActiveClipping)
				meshGenerator.BuildMeshWithArrays(currentInstructions, updateTriangles);
			else // if (UsesSingleSubmesh) is always true here in SkeletonGraphic, ensured by caller
				meshGenerator.AddSubmesh(workingSubmeshInstructions.Items[0], updateTriangles);

			ScaleAndOffsetGeneratorMesh(0);

			currentSmartMesh.instructionUsed.Set(currentInstructions);
			IssueOnPostProcessVertices(meshGenerator.Buffers);
			return true;
		}

		protected virtual bool FillMeshFromBuffers (MeshRendererBuffers.SmartMesh currentSmartMesh, bool updateTriangles) {
			if (UsesSingleSubmesh) {
				MeshGenerator meshGenerator = meshGenerators.Items[0];
				Mesh currentMesh = currentSmartMesh.mesh;
				meshGenerator.FillVertexData(currentMesh);
				if (updateTriangles) { // Check if the triangles should also be updated.
					meshGenerator.FillTriangles(currentMesh);
				}
				meshGenerator.FillLateVertexData(currentMesh);
			} else {
				int submeshCount = currentInstructions.submeshInstructions.Count;
				EnsureMeshesCount(submeshCount);

				Mesh[] meshesItems = meshes.Items;
				for (int i = 0; i < submeshCount; i++) {
					MeshGenerator meshGenerator = meshGenerators.Items[i];
					Mesh targetMesh = meshesItems[i];
					meshGenerator.FillVertexData(targetMesh);
					meshGenerator.FillTriangles(targetMesh);
					meshGenerator.FillLateVertexData(targetMesh);
				}
			}
			return true;
		}

		protected void UpdateMeshScaleAndOffset () {
			meshScale = CanvasReferencePixelsPerUnit;
			if (layoutScaleMode != LayoutMode.None) {
				meshScale *= referenceScale;
				layoutScale = GetLayoutScale(layoutScaleMode);
				if (!EditReferenceRect) {
					meshScale *= layoutScale;
				}
				meshOffset = pivotOffset * layoutScale;
			} else {
				meshOffset = pivotOffset;
			}
		}

		protected void ScaleAndOffsetGeneratorMesh (int submeshIndex = 0) {
			if (meshOffset == Vector2.zero) {
				if (meshScale != 1.0f)
					meshGenerators.Items[submeshIndex].ScaleVertexData(meshScale);
			} else {
				meshGenerators.Items[submeshIndex].ScaleAndOffsetVertexData(meshScale, meshOffset);
			}
		}
		#endregion Mesh Generation

		#region Renderer Assignment
		protected void EnableRenderers () { }
		protected void DisableRenderers () { }

		public void TrimRenderers () {
			List<CanvasRenderer> newList = new List<CanvasRenderer>();
			foreach (CanvasRenderer canvasRenderer in canvasRenderers) {
				if (canvasRenderer.gameObject.activeSelf) {
					newList.Add(canvasRenderer);
				} else {
#if UNITY_EDITOR
					if (!ApplicationIsPlaying)
						DestroyImmediate(canvasRenderer.gameObject);
					else
						Destroy(canvasRenderer.gameObject);
#else
					Destroy(canvasRenderer.gameObject);
#endif
				}
			}
			canvasRenderers = newList;
			SyncSubmeshGraphicsWithCanvasRenderers();
		}

		protected void SyncSubmeshGraphicsWithCanvasRenderers () {
			submeshGraphics.Clear();

#if UNITY_EDITOR
			if (!Application.isPlaying)
				DestroyOldRawImages();
#endif
			foreach (CanvasRenderer canvasRenderer in canvasRenderers) {
				SkeletonSubmeshGraphic submeshGraphic = canvasRenderer.GetComponent<SkeletonSubmeshGraphic>();
				if (submeshGraphic == null) {
					submeshGraphic = canvasRenderer.gameObject.AddComponent<SkeletonSubmeshGraphic>();
					submeshGraphic.maskable = this.maskable;
					submeshGraphic.raycastTarget = false;
				}
				submeshGraphics.Add(submeshGraphic);
			}
		}

		protected virtual void AssignMeshAtRenderer (ExposedList<SubmeshInstruction> workingSubmeshInstructions,
			MeshRendererBuffers.SmartMesh currentSmartMesh) {

			if (!allowMultipleCanvasRenderers) {
				AssignMeshAtRenderer(currentSmartMesh.mesh);
			} else {
				AssignMeshesAtMultipleCanvasRenderers(
					workingSubmeshInstructions.Count, this.meshes.Items);
			}
		}

		protected virtual void AssignMeshAtRenderer (UnityEngine.Mesh mesh) {
			if (assignMeshOverrideSingle != null)
				assignMeshOverrideSingle(mesh, this.canvasRenderer.GetMaterial(), this.mainTexture);

			bool assignAtCanvasRenderer = (assignMeshOverrideSingle == null || !disableMeshAssignmentOnOverride);
			if (assignAtCanvasRenderer)
				canvasRenderer.SetMesh(mesh);
			else
				canvasRenderer.SetMesh(null);

			if (assignAtCanvasRenderer)
				canvasRenderer.SetTexture(usedTextures.Items[0]);
		}

		protected void AssignMeshesAtMultipleCanvasRenderers (
			int numMeshes, UnityEngine.Mesh[] meshes) {

			Material[] usedMaterials = rendererBuffers.sharedMaterials;
			Texture[] usedTextureItems = usedTextures.Items;
			bool assignAtCanvasRenderer = (assignMeshOverrideSingle == null || !disableMeshAssignmentOnOverride);

			for (int i = 0; i < numMeshes; i++) {
				CanvasRenderer currentRenderer = canvasRenderers[i];
				if (assignMeshOverrideSingle == null || !disableMeshAssignmentOnOverride)
					currentRenderer.SetMesh(meshes[i]);
				else
					currentRenderer.SetMesh(null);

				currentRenderer.materialCount = 1;
				if (assignAtCanvasRenderer)
					currentRenderer.SetMaterial(usedMaterials[i], usedTextureItems[i]);
			}
			if (assignMeshOverrideMultiple != null)
				assignMeshOverrideMultiple(numMeshes, meshes, usedMaterials, usedTextureItems);
		}

		protected void ClearMeshAtRenderer () {
			canvasRenderer.Clear();
			for (int i = 0; i < canvasRenderers.Count; ++i)
				canvasRenderers[i].Clear();
		}

		protected void SetMainRendererTexture (Texture texture) {
			canvasRenderer.SetTexture(texture); // Refresh canvasRenderer's texture. Make sure it handles null.
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
			if (!valid) return;

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

		protected void EnsureSeparatorPartCount () {
#if UNITY_EDITOR
			RemoveNullSeparatorParts();
#endif
			int targetCount = separatorSlots.Count + 1;
			if (targetCount == 1)
				return;

#if UNITY_EDITOR
			if (!ApplicationIsPlaying) {
				for (int i = separatorParts.Count - 1; i >= 0; --i) {
					if (separatorParts[i] == null) {
						separatorParts.RemoveAt(i);
					}
				}
			}
#endif
			int currentCount = separatorParts.Count;
			for (int i = currentCount; i < targetCount; ++i) {
				GameObject go = new GameObject(string.Format("{0}[{1}]", SeparatorPartGameObjectName, i), typeof(RectTransform));
				go.transform.SetParent(this.transform, false);

				RectTransform dstTransform = go.transform.GetComponent<RectTransform>();
				dstTransform.localPosition = Vector3.zero;
				dstTransform.pivot = rectTransform.pivot;
				dstTransform.anchorMin = Vector2.zero;
				dstTransform.anchorMax = Vector2.one;
				dstTransform.sizeDelta = Vector2.zero;

				separatorParts.Add(go.transform);
			}
		}

		protected void UpdateSeparatorPartParents () {
			int usedCount = separatorSlots.Count + 1;
			if (usedCount == 1) {
				usedCount = 0; // placed directly at the SkeletonGraphic parent
				for (int i = 0; i < canvasRenderers.Count; ++i) {
					CanvasRenderer canvasRenderer = canvasRenderers[i];
					if (canvasRenderer.transform.parent.name.Contains(SeparatorPartGameObjectName)) {
						canvasRenderer.transform.SetParent(this.transform, false);
						canvasRenderer.transform.localPosition = Vector3.zero;
					}
				}
			}
			for (int i = 0; i < separatorParts.Count; ++i) {
				bool isUsed = i < usedCount;
				separatorParts[i].gameObject.SetActive(isUsed);
			}
		}

#if UNITY_EDITOR
		private void RemoveNullSeparatorParts () {
			if (!ApplicationIsPlaying) {
				for (int i = separatorParts.Count - 1; i >= 0; --i) {
					if (separatorParts[i] == null) {
						separatorParts.RemoveAt(i);
					}
				}
			}
		}
#endif
		#endregion Separator Slots

		#region Material Configuration
		protected virtual void UpdateUsedMaterialsForRenderers (ExposedList<SubmeshInstruction> instructions) {
			rendererBuffers.UpdateSharedMaterialsArray();
			ConfigureMaterialsAndTextures(instructions);
			materialsNeedUpdate = false;
		}

		protected virtual void ConfigureMaterialsAndTextures (ExposedList<SubmeshInstruction> instructions) {
			Material[] sharedMaterials = rendererBuffers.sharedMaterials;
			Texture[] usedTextureItems = usedTextures.Items;
			SubmeshInstruction[] instructionItems = instructions.Items;

			for (int i = 0, count = sharedMaterials.Length; i < count; ++i) {
				usedTextureItems[i] = instructionItems[i].material.mainTexture;
				sharedMaterials[i] = this.materialForRendering;
			}

			BlendModeMaterials blendModeMaterials = skeletonDataAsset.blendModeMaterials;
			bool hasBlendModeMaterials = blendModeMaterials.RequiresBlendModeMaterials;
			bool hasMaterialOrTextureOverride = HasMaterialOrTextureOverride;
			bool hasPMAAdditiveSlots = HasPMAAdditiveSlots(instructions);
			MeshGenerator meshGenerator = meshGenerators.Items[0];
			bool pmaVertexColors = meshGenerator.settings.pmaVertexColors;
#if HAS_CULL_TRANSPARENT_MESH
			bool mainCullTransparentMesh = this.canvasRenderer.cullTransparentMesh;
#endif
			if (HasMaterialOrTextureOverride || hasBlendModeMaterials || hasPMAAdditiveSlots) {
				for (int i = 0, count = sharedMaterials.Length; i < count; ++i) {
					Texture originalTexture = instructionItems[i].material.mainTexture;

					if (hasMaterialOrTextureOverride) {
						Material replacementMaterial;
						Texture replacementTexture;
						if (customMaterialOverride.TryGetValue(originalTexture, out replacementMaterial))
							sharedMaterials[i] = replacementMaterial;
						if (customTextureOverride.TryGetValue(originalTexture, out replacementTexture) ||
							customTextureOverride.TryGetValue(Texture2D.whiteTexture, out replacementTexture)) // white texture entry = replace-all
							usedTextureItems[i] = replacementTexture;
					}
					if (hasBlendModeMaterials || hasPMAAdditiveSlots) {
						bool allowCullTransparentMesh = true;
						Material blendModeMaterial = GetBlendModeMaterial(instructionItems[i], blendModeMaterials,
							pmaVertexColors, ref allowCullTransparentMesh);
						if (blendModeMaterial != null)
							sharedMaterials[i] = blendModeMaterial;
#if HAS_CULL_TRANSPARENT_MESH
						if (!UsesSingleSubmesh)
							canvasRenderers[i].cullTransparentMesh = allowCullTransparentMesh ?
								mainCullTransparentMesh : false;
#endif
					}
				}
			}

			if (!UsesSingleSubmesh) {
				for (int i = 0, count = sharedMaterials.Length; i < count; ++i) {
					sharedMaterials[i] = submeshGraphics[i].UpdateModifiedMaterial(sharedMaterials[i]);
				}
			}
		}

		/// <returns>True if any element of the given <c>instructions</c> list has
		/// <see cref="SubmeshInstruction.hasPMAAdditiveSlot"/> set, false otherwise.</returns>
		protected bool HasPMAAdditiveSlots (ExposedList<SubmeshInstruction> instructions) {
			SubmeshInstruction[] items = instructions.Items;
			for (int i = 0, count = instructions.Count; i < count; ++i) {
				if (items[i].hasPMAAdditiveSlot)
					return true;
			}
			return false;
		}

		/// <returns>The respective blend mode material, or null if no blend mode material is required.</returns>
		protected Material GetBlendModeMaterial (SubmeshInstruction instruction, BlendModeMaterials blendModeMaterials,
			bool pmaVertexColors, ref bool allowCullTransparentMesh) {

			BlendMode blendMode = blendModeMaterials.BlendModeForMaterial(instruction.material);
			Material blendModeMaterial = null;
			if (blendMode == BlendMode.Normal) {
				if (instruction.hasPMAAdditiveSlot)
					allowCullTransparentMesh = false;
			} else if (blendMode == BlendMode.Additive) {
				if (pmaVertexColors)
					allowCullTransparentMesh = false;
				else if (additiveMaterial)
					blendModeMaterial = additiveMaterial;
			} else if (blendMode == BlendMode.Multiply && multiplyMaterial)
				blendModeMaterial = multiplyMaterial;
			else if (blendMode == BlendMode.Screen && screenMaterial)
				blendModeMaterial = screenMaterial;
			return blendModeMaterial;
		}

		/// <summary>
		/// Fills the output parameter <c>outMaterial</c> with the respective
		/// override material where it applies, or to <c>originalMaterial</c>
		/// when no override is set.</summary>
		/// <returns>True if any override for the given <c>originalTexture</c>
		/// was found, false otherwise.</returns>
		public bool GetOverrideMaterial (Texture originalTexture, Material originalMaterial,
			out Material outMaterial) {

			if (!customMaterialOverride.TryGetValue(originalTexture, out outMaterial)) {
				outMaterial = originalMaterial;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Fills the output parameter <c>outTexture</c> with the respective
		/// override texture where it applies, or to <c>originalTexture</c>
		/// when no override is set.</summary>
		/// <returns>True if any override for the given <c>originalTexture</c>
		/// was found, false otherwise.</returns>
		public bool GetOverrideTexture (Texture originalTexture, out Texture outTexture) {
			// Note below: white Texture entry = replace-all
			if (!customTextureOverride.TryGetValue(originalTexture, out outTexture) &&
				!customTextureOverride.TryGetValue(Texture2D.whiteTexture, out outTexture)) {
				outTexture = originalTexture;
				return false;
			}
			return true;
		}

#if SPINE_OPTIONAL_ON_DEMAND_LOADING
		void HandleOnDemandLoading () {
			foreach (AtlasAssetBase atlasAsset in skeletonDataAsset.atlasAssets) {
				if (atlasAsset.TextureLoadingMode != AtlasAssetBase.LoadingMode.Normal) {
					atlasAsset.BeginCustomTextureLoading();

					Texture[] textureItems = usedTextures.Items;
					for (int i = 0, count = usedTextures.Count; i < count; ++i) {
						Texture loadedTexture = null;
						atlasAsset.RequireTextureLoaded(textureItems[i], ref loadedTexture, null);
						if (loadedTexture)
							usedTextures.Items[i] = loadedTexture;
					}
					atlasAsset.EndCustomTextureLoading();
				}
			}
		}
#endif
		#endregion Material Configuration

		#region Runtime Instantiation
		/// <summary>Create a new GameObject with SkeletonGraphic and SkeletonAnimations components.</summary>
		/// <param name="material">Material for the canvas renderer to use. Usually, the default SkeletonGraphic material
		/// can be used.</param>
		public static SkeletonComponents<SkeletonGraphic, SkeletonAnimation> NewSkeletonGraphicGameObject (
			SkeletonDataAsset skeletonDataAsset, Transform parent, Material material, bool quiet = false) {
			return NewSkeletonGraphicGameObject<SkeletonAnimation>(skeletonDataAsset, parent, material, quiet);
		}

		/// <summary>Create a new GameObject with SkeletonGraphic and a Spine animation component.</summary>
		/// <param name="material">Material for the canvas renderer to use. Usually, the default SkeletonGraphic material
		/// can be used.</param>
		/// <typeparam name="Animation">Animation should be SkeletonAnimation, SkeletonMecanim or any custom derived class.</typeparam>
		public static SkeletonComponents<SkeletonGraphic, Animation> NewSkeletonGraphicGameObject<Animation> (
			SkeletonDataAsset skeletonDataAsset, Transform parent, Material material, bool quiet = false)
			where Animation : SkeletonAnimationBase {

			SkeletonComponents<SkeletonGraphic, Animation> components
				= SkeletonGraphic.AddSkeletonGraphicAnimationComponents<Animation>(
					new GameObject("New Spine GameObject"), skeletonDataAsset, material, quiet);
			if (parent != null) components.skeletonRenderer.transform.SetParent(parent, false);
			return components;
		}

		/// <summary>Add a SkeletonGraphic component to a GameObject.</summary>
		/// <param name="material">Material for the canvas renderer to use. Usually, the default SkeletonGraphic material will work.</param>
		public static SkeletonGraphic AddSkeletonGraphicRenderingComponent (GameObject gameObject, SkeletonDataAsset skeletonDataAsset, Material material) {
			SkeletonGraphic skeletonGraphic = gameObject.AddComponent<SkeletonGraphic>();
			if (skeletonDataAsset != null) {
				skeletonGraphic.material = material;
				skeletonGraphic.skeletonDataAsset = skeletonDataAsset;
				skeletonGraphic.Initialize(false);
			}
#if HAS_CULL_TRANSPARENT_MESH
			CanvasRenderer canvasRenderer = gameObject.GetComponent<CanvasRenderer>();
			if (canvasRenderer) canvasRenderer.cullTransparentMesh = false;
#endif
			return skeletonGraphic;
		}

		/// <summary>Add SkeletonGraphic and a Spine animation component to a GameObject.</summary>
		/// <param name="material">Material for the canvas renderer to use. Usually, the default SkeletonGraphic material will work.</param>
		public static SkeletonComponents<SkeletonGraphic, SkeletonAnimation> AddSkeletonGraphicAnimationComponents (
			GameObject gameObject, SkeletonDataAsset skeletonDataAsset, Material material, bool quiet = false) {

			return AddSkeletonGraphicAnimationComponents<SkeletonAnimation>(gameObject, skeletonDataAsset, material, quiet);
		}

		/// <summary>Add SkeletonGraphic and SkeletonAnimation components to a GameObject.</summary>
		/// <param name="material">Material for the canvas renderer to use. Usually, the default SkeletonGraphic material will work.</param>
		/// <typeparam name="Animation">Animation should be SkeletonAnimation, SkeletonMecanim or any custom derived class.</typeparam>
		public static SkeletonComponents<SkeletonGraphic, Animation> AddSkeletonGraphicAnimationComponents<Animation> (
			GameObject gameObject, SkeletonDataAsset skeletonDataAsset, Material material, bool quiet = false)
			where Animation : SkeletonAnimationBase {

			SkeletonGraphic skeletonGraphic = gameObject.AddComponent<SkeletonGraphic>();
			if (skeletonDataAsset != null) {
				skeletonGraphic.material = material;
				skeletonGraphic.skeletonDataAsset = skeletonDataAsset;
				skeletonGraphic.Initialize(false, quiet);
			}
			Animation animationComponent = gameObject.AddComponent<Animation>();
			if (skeletonDataAsset != null) {
				animationComponent.Initialize(false, quiet);
			}
			skeletonGraphic.Animation = animationComponent;

#if HAS_CULL_TRANSPARENT_MESH
			CanvasRenderer canvasRenderer = gameObject.GetComponent<CanvasRenderer>();
			if (canvasRenderer) canvasRenderer.cullTransparentMesh = false;
#endif
			return new SkeletonComponents<SkeletonGraphic, Animation>(skeletonGraphic, animationComponent);
		}
		#endregion Runtime Instantiation

		#region Internal Methods
#if USE_THREADED_SKELETON_UPDATE
		public virtual void MainThreadPrepareLateUpdateInternal () {
			canvasReferencePixelsPerUnit = (canvas == null) ? 100 : canvas.referencePixelsPerUnit;

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
#if UNITY_EDITOR
				UpdateReferenceRectSizes();
				if (!Application.isPlaying && requiresEditorUpdate) {
					Initialize(true);
					IssueEditorWarnings();
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
		protected override void OnValidate () {
			base.OnValidate();
			requiresEditorUpdate = true;
		}

		protected override void Reset () {
			base.Reset();
			if (material == null || material.shader != Shader.Find("Spine/SkeletonGraphic"))
				Debug.LogWarning("SkeletonGraphic works best with the SkeletonGraphic material.");
		}

		protected void InitLayoutScaleParameters () {
			previousLayoutScaleMode = layoutScaleMode;
		}

		protected void UpdateReferenceRectSizes () {
			if (rectTransformSize == Vector2.zero)
				rectTransformSize = GetCurrentRectSize();

			HandleChangedEditReferenceRect();

			if (layoutScaleMode != previousLayoutScaleMode) {
				if (layoutScaleMode != LayoutMode.None) {
					SetRectTransformSize(this, rectTransformSize);
				} else {
					rectTransformSize = referenceSize / referenceScale;
					referenceScale = 1f;
					SetRectTransformSize(this, rectTransformSize);
				}
			}
			if (editReferenceRect || layoutScaleMode == LayoutMode.None)
				referenceSize = GetCurrentRectSize();

			previousLayoutScaleMode = layoutScaleMode;
		}

		protected void HandleChangedEditReferenceRect () {
			if (editReferenceRect == previousEditReferenceRect) return;
			previousEditReferenceRect = editReferenceRect;

			if (editReferenceRect) {
				rectTransformSize = GetCurrentRectSize();
				ResetRectToReferenceRectSize();
			} else {
				SetRectTransformSize(this, rectTransformSize);
			}
		}

		public void ResetRectToReferenceRectSize () {
			referenceScale *= GetLayoutScale(previousLayoutScaleMode);
			float referenceAspect = referenceSize.x / referenceSize.y;
			Vector2 newSize = GetCurrentRectSize();

			LayoutMode mode = GetEffectiveLayoutMode(previousLayoutScaleMode);
			if (mode == LayoutMode.WidthControlsHeight)
				newSize.y = newSize.x / referenceAspect;
			else if (mode == LayoutMode.HeightControlsWidth)
				newSize.x = newSize.y * referenceAspect;
			SetRectTransformSize(this, newSize);
		}

		public Vector2 GetReferenceRectSize () {
			return referenceSize * GetLayoutScale(layoutScaleMode);
		}

		public Vector2 GetPivotOffset () {
			return pivotOffset;
		}

		public Vector2 GetScaledPivotOffset () {
			return pivotOffset * GetLayoutScale(layoutScaleMode);
		}

		protected void IssueEditorWarnings () {
			if (!allowMultipleCanvasRenderers &&
			(skeletonDataAsset.atlasAssets.Length > 1 || skeletonDataAsset.atlasAssets[0].MaterialCount > 1))
				Debug.LogError(string.Format("'{0}': Unity UI does not support multiple textures per Renderer. " +
					"Please enable 'Advanced - Multiple CanvasRenderers' to generate the required CanvasRenderer GameObjects. " +
					"Otherwise your skeleton will not be rendered correctly.", this.name), this);
		}
#endif
		#endregion Editor Methods

		#region Transfer of Deprecated Fields
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		// compatibility layer for new split animation component architecture,
		// automatically transfer serialized attributes.
		public void UpgradeTo43 () {
			UpgradeTo43Components();
			TransferDeprecatedFields();
		}

		protected void UpgradeTo43Components () {
			if (gameObject.GetComponent<SkeletonAnimation>() == null) {
				gameObject.AddComponent<SkeletonAnimation>();
				EditorBridge.RequestMarkDirty(gameObject);
				Debug.Log(string.Format("{0}: Auto-migrated old SkeletonGraphic component to split SkeletonAnimation + SkeletonGraphic components.",
					gameObject.name), gameObject);
			}
		}

		protected virtual void TransferDeprecatedFields () {
			wasDeprecatedTransferred = true;
			if (meshGeneratorDeprecated == null) return;
			meshSettings = meshGeneratorDeprecated.settings;

			Initialize(false);
			skeletonAnimation.Initialize(false);

			SkeletonAnimation skeletonAnimationComponent = skeletonAnimation as SkeletonAnimation;
			if (skeletonAnimationComponent) {
				skeletonAnimationComponent.AnimationName = startingAnimationDeprecated;
				skeletonAnimationComponent.loop = startingLoopDeprecated;
				skeletonAnimationComponent.timeScale = timeScaleDeprecated;
				skeletonAnimationComponent.unscaledTime = unscaledTimeDeprecated;
			}
		}

		[SerializeField] protected bool wasDeprecatedTransferred = false;

		[FormerlySerializedAs("meshGenerator")] [SerializeField] private MeshGenerator meshGeneratorDeprecated;
		[FormerlySerializedAs("startingAnimation")] [SerializeField] private string startingAnimationDeprecated;
		[FormerlySerializedAs("startingLoop")] [SerializeField] private bool startingLoopDeprecated;
		[FormerlySerializedAs("timeScale")] [SerializeField] private float timeScaleDeprecated = 1;
		[FormerlySerializedAs("unscaledTime")] [SerializeField] private bool unscaledTimeDeprecated = false;
#endif // UNITY_EDITOR
		#endregion Transfer of Deprecated Fields
		#endregion Methods
	}
}
