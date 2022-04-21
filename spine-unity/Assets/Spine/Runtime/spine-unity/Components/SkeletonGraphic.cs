/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spine.Unity {
#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(CanvasRenderer), typeof(RectTransform)), DisallowMultipleComponent]
	[AddComponentMenu("Spine/SkeletonGraphic (Unity UI Canvas)")]
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonGraphic-Component")]
	public class SkeletonGraphic : MaskableGraphic, ISkeletonComponent, IAnimationStateComponent, ISkeletonAnimation, IHasSkeletonDataAsset {

		#region Inspector
		public SkeletonDataAsset skeletonDataAsset;
		public SkeletonDataAsset SkeletonDataAsset { get { return skeletonDataAsset; } }

		public Material additiveMaterial;
		public Material multiplyMaterial;
		public Material screenMaterial;

		[SpineSkin(dataField: "skeletonDataAsset", defaultAsEmptyString: true)]
		public string initialSkinName;
		public bool initialFlipX, initialFlipY;

		[SpineAnimation(dataField: "skeletonDataAsset")]
		public string startingAnimation;
		public bool startingLoop;
		public float timeScale = 1f;
		public bool freeze;

		/// <summary>Update mode to optionally limit updates to e.g. only apply animations but not update the mesh.</summary>
		public UpdateMode UpdateMode { get { return updateMode; } set { updateMode = value; } }
		protected UpdateMode updateMode = UpdateMode.FullUpdate;

		/// <summary>Update mode used when the MeshRenderer becomes invisible
		/// (when <c>OnBecameInvisible()</c> is called). Update mode is automatically
		/// reset to <c>UpdateMode.FullUpdate</c> when the mesh becomes visible again.</summary>
		public UpdateMode updateWhenInvisible = UpdateMode.FullUpdate;

		public bool unscaledTime;
		public bool allowMultipleCanvasRenderers = false;
		public List<CanvasRenderer> canvasRenderers = new List<CanvasRenderer>();
		protected List<SkeletonSubmeshGraphic> submeshGraphics = new List<SkeletonSubmeshGraphic>();
		protected int usedRenderersCount = 0;

		// Submesh Separation
		public const string SeparatorPartGameObjectName = "Part";
		/// <summary>Slot names used to populate separatorSlots list when the Skeleton is initialized. Changing this after initialization does nothing.</summary>
		[SerializeField] [SpineSlot] protected string[] separatorSlotNames = new string[0];

		/// <summary>Slots that determine where the render is split. This is used by components such as SkeletonRenderSeparator so that the skeleton can be rendered by two separate renderers on different GameObjects.</summary>
		[System.NonSerialized] public readonly List<Slot> separatorSlots = new List<Slot>();
		public bool enableSeparatorSlots = false;
		[SerializeField] protected List<Transform> separatorParts = new List<Transform>();
		public List<Transform> SeparatorParts { get { return separatorParts; } }
		public bool updateSeparatorPartLocation = true;

		private bool wasUpdatedAfterInit = true;
		private Texture baseTexture = null;

#if UNITY_EDITOR
		protected override void OnValidate () {
			// This handles Scene View preview.
			base.OnValidate();
			if (this.IsValid) {
				if (skeletonDataAsset == null) {
					Clear();
				} else if (skeletonDataAsset.skeletonJSON == null) {
					Clear();
				} else if (skeletonDataAsset.GetSkeletonData(true) != skeleton.Data) {
					Clear();
					Initialize(true);
					if (!allowMultipleCanvasRenderers && (skeletonDataAsset.atlasAssets.Length > 1 || skeletonDataAsset.atlasAssets[0].MaterialCount > 1))
						Debug.LogError("Unity UI does not support multiple textures per Renderer. Please enable 'Advanced - Multiple CanvasRenderers' to generate the required CanvasRenderer GameObjects. Otherwise your skeleton will not be rendered correctly.", this);
				} else {
					if (freeze) return;

					if (!string.IsNullOrEmpty(initialSkinName)) {
						var skin = skeleton.Data.FindSkin(initialSkinName);
						if (skin != null) {
							if (skin == skeleton.Data.DefaultSkin)
								skeleton.SetSkin((Skin)null);
							else
								skeleton.SetSkin(skin);
						}

					}

					// Only provide visual feedback to inspector changes in Unity Editor Edit mode.
					if (!Application.isPlaying) {
						skeleton.ScaleX = this.initialFlipX ? -1 : 1;
						skeleton.ScaleY = this.initialFlipY ? -1 : 1;

						state.ClearTrack(0);
						skeleton.SetToSetupPose();
						if (!string.IsNullOrEmpty(startingAnimation)) {
							state.SetAnimation(0, startingAnimation, startingLoop);
							Update(0f);
						}
					}
				}
			} else {
				// Under some circumstances (e.g. sometimes on the first import) OnValidate is called
				// before SpineEditorUtilities.ImportSpineContent, causing an unnecessary exception.
				// The (skeletonDataAsset.skeletonJSON != null) condition serves to prevent this exception.
				if (skeletonDataAsset != null && skeletonDataAsset.skeletonJSON != null)
					Initialize(true);
			}
		}

		protected override void Reset () {

			base.Reset();
			if (material == null || material.shader != Shader.Find("Spine/SkeletonGraphic"))
				Debug.LogWarning("SkeletonGraphic works best with the SkeletonGraphic material.");
		}
#endif
		#endregion

		#region Runtime Instantiation
		/// <summary>Create a new GameObject with a SkeletonGraphic component.</summary>
		/// <param name="material">Material for the canvas renderer to use. Usually, the default SkeletonGraphic material will work.</param>
		public static SkeletonGraphic NewSkeletonGraphicGameObject (SkeletonDataAsset skeletonDataAsset, Transform parent, Material material) {
			var sg = SkeletonGraphic.AddSkeletonGraphicComponent(new GameObject("New Spine GameObject"), skeletonDataAsset, material);
			if (parent != null) sg.transform.SetParent(parent, false);
			return sg;
		}

		/// <summary>Add a SkeletonGraphic component to a GameObject.</summary>
		/// <param name="material">Material for the canvas renderer to use. Usually, the default SkeletonGraphic material will work.</param>
		public static SkeletonGraphic AddSkeletonGraphicComponent (GameObject gameObject, SkeletonDataAsset skeletonDataAsset, Material material) {
			var c = gameObject.AddComponent<SkeletonGraphic>();
			if (skeletonDataAsset != null) {
				c.material = material;
				c.skeletonDataAsset = skeletonDataAsset;
				c.Initialize(false);
			}
			return c;
		}
		#endregion

		#region Overrides
		[System.NonSerialized] readonly Dictionary<Texture, Texture> customTextureOverride = new Dictionary<Texture, Texture>();
		/// <summary>Use this Dictionary to override a Texture with a different Texture.</summary>
		public Dictionary<Texture, Texture> CustomTextureOverride { get { return customTextureOverride; } }

		[System.NonSerialized] readonly Dictionary<Texture, Material> customMaterialOverride = new Dictionary<Texture, Material>();
		/// <summary>Use this Dictionary to override the Material where the Texture was used at the original atlas.</summary>
		public Dictionary<Texture, Material> CustomMaterialOverride { get { return customMaterialOverride; } }

		// This is used by the UI system to determine what to put in the MaterialPropertyBlock.
		Texture overrideTexture;
		public Texture OverrideTexture {
			get { return overrideTexture; }
			set {
				overrideTexture = value;
				canvasRenderer.SetTexture(this.mainTexture); // Refresh canvasRenderer's texture. Make sure it handles null.
			}
		}
		#endregion

		#region Internals
		public override Texture mainTexture {
			get {
				if (overrideTexture != null) return overrideTexture;
				return baseTexture;
			}
		}

		protected override void Awake () {

			base.Awake();
			this.onCullStateChanged.AddListener(OnCullStateChanged);

			SyncSubmeshGraphicsWithCanvasRenderers();
			if (!this.IsValid) {
#if UNITY_EDITOR
				// workaround for special import case of open scene where OnValidate and Awake are
				// called in wrong order, before setup of Spine assets.
				if (!Application.isPlaying) {
					if (this.skeletonDataAsset != null && this.skeletonDataAsset.skeletonJSON == null)
						return;
				}
#endif
				Initialize(false);
				Rebuild(CanvasUpdate.PreRender);
			}
		}

		protected override void OnDestroy () {
			Clear();
			base.OnDestroy();
		}

		public override void Rebuild (CanvasUpdate update) {
			base.Rebuild(update);
			if (canvasRenderer.cull) return;
			if (update == CanvasUpdate.PreRender) UpdateMesh(keepRendererCount: true);
			if (allowMultipleCanvasRenderers) canvasRenderer.Clear();
		}

		protected override void OnDisable () {
			base.OnDisable();
			foreach (var canvasRenderer in canvasRenderers) {
				canvasRenderer.Clear();
			}
		}

		public virtual void Update () {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				Update(0f);
				return;
			}
#endif

			if (freeze) return;
			Update(unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
		}

		public virtual void Update (float deltaTime) {
			if (!this.IsValid) return;

			wasUpdatedAfterInit = true;
			if (updateMode < UpdateMode.OnlyAnimationStatus)
				return;

			UpdateAnimationStatus(deltaTime);

			if (updateMode == UpdateMode.OnlyAnimationStatus) {
				state.ApplyEventTimelinesOnly(skeleton, issueEvents: false);
				return;
			}
			ApplyAnimation();
		}

		protected void SyncSubmeshGraphicsWithCanvasRenderers () {
			submeshGraphics.Clear();

#if UNITY_EDITOR
			if (!Application.isPlaying)
				DestroyOldRawImages();
#endif
			foreach (var canvasRenderer in canvasRenderers) {
				var submeshGraphic = canvasRenderer.GetComponent<SkeletonSubmeshGraphic>();
				if (submeshGraphic == null) {
					submeshGraphic = canvasRenderer.gameObject.AddComponent<SkeletonSubmeshGraphic>();
					submeshGraphic.maskable = this.maskable;
					submeshGraphic.raycastTarget = false;
				}
				submeshGraphics.Add(submeshGraphic);
			}
		}

		protected void UpdateAnimationStatus (float deltaTime) {
			deltaTime *= timeScale;
			skeleton.Update(deltaTime);
			state.Update(deltaTime);
		}

		protected void ApplyAnimation () {
			if (BeforeApply != null)
				BeforeApply(this);

			if (updateMode != UpdateMode.OnlyEventTimelines)
				state.Apply(skeleton);
			else
				state.ApplyEventTimelinesOnly(skeleton, issueEvents: true);

			if (UpdateLocal != null)
				UpdateLocal(this);

			skeleton.UpdateWorldTransform();

			if (UpdateWorld != null) {
				UpdateWorld(this);
				skeleton.UpdateWorldTransform();
			}

			if (UpdateComplete != null)
				UpdateComplete(this);
		}

		public void LateUpdate () {
			// instantiation can happen from Update() after this component, leading to a missing Update() call.
			if (!wasUpdatedAfterInit) Update(0);
			if (freeze) return;
			if (updateMode != UpdateMode.FullUpdate) return;

			UpdateMesh();
		}

		protected void OnCullStateChanged (bool culled) {
			if (culled)
				OnBecameInvisible();
			else
				OnBecameVisible();
		}

		public void OnBecameVisible () {
			updateMode = UpdateMode.FullUpdate;
		}

		public void OnBecameInvisible () {
			updateMode = updateWhenInvisible;
		}

		public void ReapplySeparatorSlotNames () {
			if (!IsValid)
				return;

			separatorSlots.Clear();
			for (int i = 0, n = separatorSlotNames.Length; i < n; i++) {
				string slotName = separatorSlotNames[i];
				if (slotName == "")
					continue;
				var slot = skeleton.FindSlot(slotName);
				if (slot != null) {
					separatorSlots.Add(slot);
				}
#if UNITY_EDITOR
				else {
					Debug.LogWarning(slotName + " is not a slot in " + skeletonDataAsset.skeletonJSON.name);
				}
#endif
			}
			UpdateSeparatorPartParents();
		}
		#endregion

		#region API
		protected Skeleton skeleton;
		public Skeleton Skeleton {
			get {
				Initialize(false);
				return skeleton;
			}
			set {
				skeleton = value;
			}
		}
		public SkeletonData SkeletonData { get { return skeleton == null ? null : skeleton.Data; } }
		public bool IsValid { get { return skeleton != null; } }

		public delegate void SkeletonRendererDelegate (SkeletonGraphic skeletonGraphic);

		/// <summary>OnRebuild is raised after the Skeleton is successfully initialized.</summary>
		public event SkeletonRendererDelegate OnRebuild;

		/// <summary>OnMeshAndMaterialsUpdated is at the end of LateUpdate after the Mesh and
		/// all materials have been updated.</summary>
		public event SkeletonRendererDelegate OnMeshAndMaterialsUpdated;

		protected Spine.AnimationState state;
		public Spine.AnimationState AnimationState {
			get {
				Initialize(false);
				return state;
			}
		}

		[SerializeField] protected Spine.Unity.MeshGenerator meshGenerator = new MeshGenerator();
		public Spine.Unity.MeshGenerator MeshGenerator { get { return this.meshGenerator; } }
		DoubleBuffered<Spine.Unity.MeshRendererBuffers.SmartMesh> meshBuffers;
		SkeletonRendererInstruction currentInstructions = new SkeletonRendererInstruction();
		readonly ExposedList<Mesh> meshes = new ExposedList<Mesh>();

		public Mesh GetLastMesh () {
			return meshBuffers.GetCurrent().mesh;
		}

		public bool MatchRectTransformWithBounds () {
			if (!wasUpdatedAfterInit) Update(0);
			UpdateMesh();

			if (!this.allowMultipleCanvasRenderers)
				return MatchRectTransformSingleRenderer();
			else
				return MatchRectTransformMultipleRenderers();
		}

		protected bool MatchRectTransformSingleRenderer () {
			Mesh mesh = this.GetLastMesh();
			if (mesh == null) {
				return false;
			}
			if (mesh.vertexCount == 0) {
				this.rectTransform.sizeDelta = new Vector2(50f, 50f);
				this.rectTransform.pivot = new Vector2(0.5f, 0.5f);
				return false;
			}
			mesh.RecalculateBounds();
			SetRectTransformBounds(mesh.bounds);
			return true;
		}

		protected bool MatchRectTransformMultipleRenderers () {
			bool anyBoundsAdded = false;
			Bounds combinedBounds = new Bounds();
			for (int i = 0; i < canvasRenderers.Count; ++i) {
				var canvasRenderer = canvasRenderers[i];
				if (!canvasRenderer.gameObject.activeSelf)
					continue;

				Mesh mesh = meshes.Items[i];
				if (mesh == null || mesh.vertexCount == 0)
					continue;

				mesh.RecalculateBounds();
				var bounds = mesh.bounds;
				if (anyBoundsAdded)
					combinedBounds.Encapsulate(bounds);
				else {
					anyBoundsAdded = true;
					combinedBounds = bounds;
				}
			}

			if (!anyBoundsAdded) {
				this.rectTransform.sizeDelta = new Vector2(50f, 50f);
				this.rectTransform.pivot = new Vector2(0.5f, 0.5f);
				return false;
			}

			SetRectTransformBounds(combinedBounds);
			return true;
		}

		private void SetRectTransformBounds (Bounds combinedBounds) {
			var size = combinedBounds.size;
			var center = combinedBounds.center;
			var p = new Vector2(
				0.5f - (center.x / size.x),
				0.5f - (center.y / size.y)
			);

			this.rectTransform.sizeDelta = size;
			this.rectTransform.pivot = p;

			foreach (var submeshGraphic in submeshGraphics) {
				submeshGraphic.rectTransform.sizeDelta = size;
				submeshGraphic.rectTransform.pivot = p;
			}
		}

		public event UpdateBonesDelegate BeforeApply;
		public event UpdateBonesDelegate UpdateLocal;
		public event UpdateBonesDelegate UpdateWorld;
		public event UpdateBonesDelegate UpdateComplete;

		/// <summary> Occurs after the vertex data populated every frame, before the vertices are pushed into the mesh.</summary>
		public event Spine.Unity.MeshGeneratorDelegate OnPostProcessVertices;

		public void Clear () {
			skeleton = null;
			canvasRenderer.Clear();

			for (int i = 0; i < canvasRenderers.Count; ++i)
				canvasRenderers[i].Clear();
			DestroyMeshes();
			DisposeMeshBuffers();
		}

		public void TrimRenderers () {
			var newList = new List<CanvasRenderer>();
			foreach (var canvasRenderer in canvasRenderers) {
				if (canvasRenderer.gameObject.activeSelf) {
					newList.Add(canvasRenderer);
				} else {
					if (Application.isEditor && !Application.isPlaying)
						DestroyImmediate(canvasRenderer.gameObject);
					else
						Destroy(canvasRenderer.gameObject);
				}
			}
			canvasRenderers = newList;
			SyncSubmeshGraphicsWithCanvasRenderers();
		}

		public void Initialize (bool overwrite) {
			if (this.IsValid && !overwrite) return;
#if UNITY_EDITOR
			if (BuildUtilities.IsInSkeletonAssetBuildPreProcessing)
				return;
#endif
			if (this.skeletonDataAsset == null) return;
			var skeletonData = this.skeletonDataAsset.GetSkeletonData(false);
			if (skeletonData == null) return;

			if (skeletonDataAsset.atlasAssets.Length <= 0 || skeletonDataAsset.atlasAssets[0].MaterialCount <= 0) return;

			this.state = new Spine.AnimationState(skeletonDataAsset.GetAnimationStateData());
			if (state == null) {
				Clear();
				return;
			}

			this.skeleton = new Skeleton(skeletonData) {
				ScaleX = this.initialFlipX ? -1 : 1,
				ScaleY = this.initialFlipY ? -1 : 1
			};

			InitMeshBuffers();
			baseTexture = skeletonDataAsset.atlasAssets[0].PrimaryMaterial.mainTexture;
			canvasRenderer.SetTexture(this.mainTexture); // Needed for overwriting initializations.

			// Set the initial Skin and Animation
			if (!string.IsNullOrEmpty(initialSkinName))
				skeleton.SetSkin(initialSkinName);

			separatorSlots.Clear();
			for (int i = 0; i < separatorSlotNames.Length; i++)
				separatorSlots.Add(skeleton.FindSlot(separatorSlotNames[i]));

			wasUpdatedAfterInit = false;
			if (!string.IsNullOrEmpty(startingAnimation)) {
				var animationObject = skeletonDataAsset.GetSkeletonData(false).FindAnimation(startingAnimation);
				if (animationObject != null) {
					state.SetAnimation(0, animationObject, startingLoop);
#if UNITY_EDITOR
					if (!Application.isPlaying)
						Update(0f);
#endif
				}
			}

			if (OnRebuild != null)
				OnRebuild(this);
		}

		public void UpdateMesh (bool keepRendererCount = false) {
			if (!this.IsValid) return;

			skeleton.SetColor(this.color);

			var currentInstructions = this.currentInstructions;
			if (!this.allowMultipleCanvasRenderers) {
				UpdateMeshSingleCanvasRenderer();
			} else {
				UpdateMeshMultipleCanvasRenderers(currentInstructions, keepRendererCount);
			}

			if (OnMeshAndMaterialsUpdated != null)
				OnMeshAndMaterialsUpdated(this);
		}

		public bool HasMultipleSubmeshInstructions () {
			if (!IsValid)
				return false;
			return MeshGenerator.RequiresMultipleSubmeshesByDrawOrder(skeleton);
		}
		#endregion

		protected void InitMeshBuffers () {
			if (meshBuffers != null) {
				meshBuffers.GetNext().Clear();
				meshBuffers.GetNext().Clear();
			} else {
				meshBuffers = new DoubleBuffered<MeshRendererBuffers.SmartMesh>();
			}
		}

		protected void DisposeMeshBuffers () {
			if (meshBuffers != null) {
				meshBuffers.GetNext().Dispose();
				meshBuffers.GetNext().Dispose();
				meshBuffers = null;
			}
		}

		protected void UpdateMeshSingleCanvasRenderer () {
			if (canvasRenderers.Count > 0)
				DisableUnusedCanvasRenderers(usedCount: 0);

			var smartMesh = meshBuffers.GetNext();
			MeshGenerator.GenerateSingleSubmeshInstruction(currentInstructions, skeleton, null);
			bool updateTriangles = SkeletonRendererInstruction.GeometryNotEqual(currentInstructions, smartMesh.instructionUsed);

			meshGenerator.Begin();
			if (currentInstructions.hasActiveClipping && currentInstructions.submeshInstructions.Count > 0) {
				meshGenerator.AddSubmesh(currentInstructions.submeshInstructions.Items[0], updateTriangles);
			} else {
				meshGenerator.BuildMeshWithArrays(currentInstructions, updateTriangles);
			}

			if (canvas != null) meshGenerator.ScaleVertexData(canvas.referencePixelsPerUnit);
			if (OnPostProcessVertices != null) OnPostProcessVertices.Invoke(this.meshGenerator.Buffers);

			var mesh = smartMesh.mesh;
			meshGenerator.FillVertexData(mesh);
			if (updateTriangles) meshGenerator.FillTriangles(mesh);
			meshGenerator.FillLateVertexData(mesh);

			canvasRenderer.SetMesh(mesh);
			smartMesh.instructionUsed.Set(currentInstructions);

			if (currentInstructions.submeshInstructions.Count > 0) {
				var material = currentInstructions.submeshInstructions.Items[0].material;
				if (material != null && baseTexture != material.mainTexture) {
					baseTexture = material.mainTexture;
					if (overrideTexture == null)
						canvasRenderer.SetTexture(this.mainTexture);
				}
			}

			//this.UpdateMaterial(); // note: This would allocate memory.
			usedRenderersCount = 0;
		}

		protected void UpdateMeshMultipleCanvasRenderers (SkeletonRendererInstruction currentInstructions, bool keepRendererCount) {
			MeshGenerator.GenerateSkeletonRendererInstruction(currentInstructions, skeleton, null,
				enableSeparatorSlots ? separatorSlots : null,
				enableSeparatorSlots ? separatorSlots.Count > 0 : false,
				false);

			int submeshCount = currentInstructions.submeshInstructions.Count;
			if (keepRendererCount && submeshCount != usedRenderersCount)
				return;
			EnsureCanvasRendererCount(submeshCount);
			EnsureMeshesCount(submeshCount);
			EnsureSeparatorPartCount();

			var c = canvas;
			float scale = (c == null) ? 100 : c.referencePixelsPerUnit;

			// Generate meshes.
			var meshesItems = meshes.Items;
			bool useOriginalTextureAndMaterial = (customMaterialOverride.Count == 0 && customTextureOverride.Count == 0);
			int separatorSlotGroupIndex = 0;
			Transform parent = this.separatorSlots.Count == 0 ? this.transform : this.separatorParts[0];

			if (updateSeparatorPartLocation) {
				for (int p = 0; p < this.separatorParts.Count; ++p) {
					separatorParts[p].position = this.transform.position;
					separatorParts[p].rotation = this.transform.rotation;
				}
			}

			BlendModeMaterials blendModeMaterials = skeletonDataAsset.blendModeMaterials;
			bool hasBlendModeMaterials = blendModeMaterials.RequiresBlendModeMaterials;
#if HAS_CULL_TRANSPARENT_MESH
			bool mainCullTransparentMesh = this.canvasRenderer.cullTransparentMesh;
#endif
			bool pmaVertexColors = meshGenerator.settings.pmaVertexColors;
			int targetSiblingIndex = 0;
			for (int i = 0; i < submeshCount; i++) {
				var submeshInstructionItem = currentInstructions.submeshInstructions.Items[i];
				meshGenerator.Begin();
				meshGenerator.AddSubmesh(submeshInstructionItem);

				Mesh targetMesh = meshesItems[i];
				meshGenerator.ScaleVertexData(scale);
				if (OnPostProcessVertices != null) OnPostProcessVertices.Invoke(this.meshGenerator.Buffers);
				meshGenerator.FillVertexData(targetMesh);
				meshGenerator.FillTriangles(targetMesh);
				meshGenerator.FillLateVertexData(targetMesh);

				var submeshMaterial = submeshInstructionItem.material;
				var canvasRenderer = canvasRenderers[i];
				if (i >= usedRenderersCount) {
					canvasRenderer.gameObject.SetActive(true);
				}
				canvasRenderer.SetMesh(targetMesh);
				canvasRenderer.materialCount = 1;

				if (canvasRenderer.transform.parent != parent.transform) {
					canvasRenderer.transform.SetParent(parent.transform, false);
					canvasRenderer.transform.localPosition = Vector3.zero;
				}
				canvasRenderer.transform.SetSiblingIndex(targetSiblingIndex++);
				if (submeshInstructionItem.forceSeparate) {
					targetSiblingIndex = 0;
					parent = separatorParts[++separatorSlotGroupIndex];
				}

				SkeletonSubmeshGraphic submeshGraphic = submeshGraphics[i];
				if (useOriginalTextureAndMaterial) {
					Texture usedTexture = submeshMaterial.mainTexture;
					if (!hasBlendModeMaterials)
						canvasRenderer.SetMaterial(this.materialForRendering, usedTexture);
					else {
						bool allowCullTransparentMesh = true;
						BlendMode blendMode = blendModeMaterials.BlendModeForMaterial(submeshMaterial);
						Material usedMaterial = this.materialForRendering;
						if (blendMode == BlendMode.Normal) {
							if (submeshInstructionItem.hasPMAAdditiveSlot)
								allowCullTransparentMesh = false;
						} else if (blendMode == BlendMode.Additive) {
							if (pmaVertexColors)
								allowCullTransparentMesh = false;
							else if (additiveMaterial)
								usedMaterial = additiveMaterial;
						} else if (blendMode == BlendMode.Multiply && multiplyMaterial)
							usedMaterial = multiplyMaterial;
						else if (blendMode == BlendMode.Screen && screenMaterial)
							usedMaterial = screenMaterial;

						usedMaterial = submeshGraphic.GetModifiedMaterial(usedMaterial);
						canvasRenderer.SetMaterial(usedMaterial, usedTexture);
#if HAS_CULL_TRANSPARENT_MESH
						canvasRenderer.cullTransparentMesh = allowCullTransparentMesh ?
							mainCullTransparentMesh : false;
#endif
					}
				} else {
					var originalTexture = submeshMaterial.mainTexture;
					Material usedMaterial;
					Texture usedTexture;
					if (!customMaterialOverride.TryGetValue(originalTexture, out usedMaterial))
						usedMaterial = material;
					if (!customTextureOverride.TryGetValue(originalTexture, out usedTexture))
						usedTexture = originalTexture;

					usedMaterial = submeshGraphic.GetModifiedMaterial(usedMaterial);
					canvasRenderer.SetMaterial(usedMaterial, usedTexture);
				}
			}

			DisableUnusedCanvasRenderers(usedCount: submeshCount);
			usedRenderersCount = submeshCount;
		}

		protected void EnsureCanvasRendererCount (int targetCount) {
#if UNITY_EDITOR
			RemoveNullCanvasRenderers();
#endif
			int currentCount = canvasRenderers.Count;
			for (int i = currentCount; i < targetCount; ++i) {
				var go = new GameObject(string.Format("Renderer{0}", i), typeof(RectTransform));
				go.transform.SetParent(this.transform, false);
				go.transform.localPosition = Vector3.zero;
				var canvasRenderer = go.AddComponent<CanvasRenderer>();
				canvasRenderers.Add(canvasRenderer);
				var submeshGraphic = go.AddComponent<SkeletonSubmeshGraphic>();
				submeshGraphic.maskable = this.maskable;
				submeshGraphic.raycastTarget = false;
				submeshGraphics.Add(submeshGraphic);
			}
		}

		protected void DisableUnusedCanvasRenderers (int usedCount) {
#if UNITY_EDITOR
			RemoveNullCanvasRenderers();
#endif
			for (int i = usedCount; i < canvasRenderers.Count; i++) {
				canvasRenderers[i].Clear();
				canvasRenderers[i].gameObject.SetActive(false);
			}
		}

#if UNITY_EDITOR
		private void RemoveNullCanvasRenderers () {
			if (Application.isEditor && !Application.isPlaying) {
				for (int i = canvasRenderers.Count - 1; i >= 0; --i) {
					if (canvasRenderers[i] == null) {
						canvasRenderers.RemoveAt(i);
					}
				}
			}
		}

		private void DestroyOldRawImages () {
			foreach (var canvasRenderer in canvasRenderers) {
				var oldRawImage = canvasRenderer.GetComponent<RawImage>();
				if (oldRawImage != null) {
					DestroyImmediate(oldRawImage);
				}
			}
		}
#endif

		protected void EnsureMeshesCount (int targetCount) {
			int oldCount = meshes.Count;
			meshes.EnsureCapacity(targetCount);
			for (int i = oldCount; i < targetCount; i++)
				meshes.Add(SpineMesh.NewSkeletonMesh());
		}

		protected void DestroyMeshes () {
			foreach (var mesh in meshes) {
#if UNITY_EDITOR
				if (Application.isEditor && !Application.isPlaying)
					UnityEngine.Object.DestroyImmediate(mesh);
				else
					UnityEngine.Object.Destroy(mesh);
#else
				UnityEngine.Object.Destroy(mesh);
#endif
			}
			meshes.Clear();
		}

		protected void EnsureSeparatorPartCount () {
#if UNITY_EDITOR
			RemoveNullSeparatorParts();
#endif
			int targetCount = separatorSlots.Count + 1;
			if (targetCount == 1)
				return;

#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying) {
				for (int i = separatorParts.Count - 1; i >= 0; --i) {
					if (separatorParts[i] == null) {
						separatorParts.RemoveAt(i);
					}
				}
			}
#endif
			int currentCount = separatorParts.Count;
			for (int i = currentCount; i < targetCount; ++i) {
				var go = new GameObject(string.Format("{0}[{1}]", SeparatorPartGameObjectName, i), typeof(RectTransform));
				go.transform.SetParent(this.transform, false);
				go.transform.localPosition = Vector3.zero;
				separatorParts.Add(go.transform);
			}
		}

		protected void UpdateSeparatorPartParents () {
			int usedCount = separatorSlots.Count + 1;
			if (usedCount == 1) {
				usedCount = 0; // placed directly at the SkeletonGraphic parent
				for (int i = 0; i < canvasRenderers.Count; ++i) {
					var canvasRenderer = canvasRenderers[i];
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
			if (Application.isEditor && !Application.isPlaying) {
				for (int i = separatorParts.Count - 1; i >= 0; --i) {
					if (separatorParts[i] == null) {
						separatorParts.RemoveAt(i);
					}
				}
			}
		}
#endif
	}
}
