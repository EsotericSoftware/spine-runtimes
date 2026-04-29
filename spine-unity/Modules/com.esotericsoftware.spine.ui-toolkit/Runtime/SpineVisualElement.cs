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

using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UIVertex = UnityEngine.UIElements.Vertex;

namespace Spine.Unity {

	public class UITKBlendModeMaterialsAttribute : PropertyAttribute {
		public readonly string dataField;
		public UITKBlendModeMaterialsAttribute (string dataField = "skeletonDataAsset") {
			this.dataField = dataField;
		}
	}

	public class BoundsFromAnimationAttribute : PropertyAttribute {

		public readonly string animationField;
		public readonly string dataField;
		public readonly string skinField;

		public BoundsFromAnimationAttribute (string animationField, string skinField, string dataField = "skeletonDataAsset") {
			this.animationField = animationField;
			this.skinField = skinField;
			this.dataField = dataField;
		}
	}

	[UxmlObject]
	[System.Serializable]
	public partial class UITKBlendModeMaterials {
		[UxmlAttribute]
		public Material normalMaterial;
		[UxmlAttribute]
		public Material additiveMaterial;
		[UxmlAttribute]
		public Material multiplyMaterial;
		[UxmlAttribute]
		public Material screenMaterial;
	}

	[UxmlElement]
	public partial class SpineVisualElement : VisualElement {

		[UxmlAttribute]
		public SkeletonDataAsset SkeletonDataAsset {
			get { return skeletonDataAsset; }
			set {
				if (skeletonDataAsset == value) return;
				skeletonDataAsset = value;
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					Initialize(true);
				}
#endif
			}
		}
		public SkeletonDataAsset skeletonDataAsset;

		[SpineSkin(dataField: "SkeletonDataAsset", defaultAsEmptyString: true, avoidGenericMenu: true)]
		[UxmlAttribute]
		public string InitialSkinName {
			get { return initialSkinName; }
			set {
				if (initialSkinName == value) return;
				initialSkinName = value;
#if UNITY_EDITOR
				if (!Application.isPlaying)
					Initialize(true);
#endif
			}
		}
		public string initialSkinName;

		[SpineAnimation(dataField: "SkeletonDataAsset", avoidGenericMenu: true)]
		[UxmlAttribute]
		public string StartingAnimation {
			get { return startingAnimation; }
			set {
				if (startingAnimation == value) return;
				startingAnimation = value;
#if UNITY_EDITOR
				if (!Application.isPlaying)
					Initialize(true);
#endif
			}
		}
		public string startingAnimation = "";
		[UxmlAttribute] public bool startingLoop { get; set; } = true;
		[UxmlAttribute] public float timeScale { get; set; } = 1.0f;
		[UxmlAttribute] public bool unscaledTime { get; set; }
		[UxmlAttribute] public bool freeze { get; set; }

		[UxmlAttribute] public bool MultipleMaterials {
			get { return supportMultipleMaterials; }
			set {
				if (supportMultipleMaterials == value) return;
				supportMultipleMaterials = value;
				if (!supportMultipleMaterials) {
					RemoveMultiMaterialRendererElements();
				} else {
					Update(0);
				}
			}
		}
		public bool supportMultipleMaterials = true;

		[UxmlObjectReference("blend-mode-materials")]
		[UITKBlendModeMaterials(dataField: "SkeletonDataAsset")]
		public UITKBlendModeMaterials blendModeMaterials = new UITKBlendModeMaterials();

		/// <summary>Flip indices of back-faces to correct winding order during mesh generation.
		/// UI Elements otherwise does not draw back-faces.</summary>
		[UxmlAttribute] public bool flipBackFaces { get; set; } = true;

		[SpineAnimation(dataField: "SkeletonDataAsset", avoidGenericMenu: true)]
		[UxmlAttribute]
		public string BoundsAnimation {
			get { return boundsAnimation; }
			set {
				boundsAnimation = value;
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					if (!this.IsValid)
						Initialize(true);
					else {
						UpdateAnimation();
					}
				}
#endif
			}
		}
		public string boundsAnimation = "";

		[UxmlAttribute]
		[BoundsFromAnimation(animationField: "BoundsAnimation",
			skinField: "InitialSkinName", dataField: "SkeletonDataAsset")]
		public Bounds ReferenceBounds {
			get { return referenceMeshBounds; }
			set {
				if (referenceMeshBounds == value) return;
#if UNITY_EDITOR
				if (!Application.isPlaying && (value.size.x == 0 || value.size.y == 0)) return;
#endif
				referenceMeshBounds = value;
				if (!this.IsValid) return;

				for (int i = 0, count = rendererElements.Count; i < count; ++i)
					AdjustOffsetScaleToMeshBounds(rendererElements.Items[i]);
			}
		}
		public Bounds referenceMeshBounds;

		public AnimationState AnimationState {
			get {
				Initialize(false);
				return state;
			}
		}
		
		/// <summary>Update mode to optionally limit updates to e.g. only apply animations but not update the mesh.</summary>
		public UpdateMode UpdateMode { get { return updateMode; } set { updateMode = value; } }
		protected UpdateMode updateMode = UpdateMode.FullUpdate;

		protected AnimationState state = null;
		protected Skeleton skeleton = null;
		protected SkeletonRendererInstruction currentInstructions = new();
		protected Spine.Unity.MeshGeneratorUIElements meshGenerator = new MeshGeneratorUIElements();

		protected ExposedList<VisualElement> rendererElements = new ExposedList<VisualElement>(1);
		IVisualElementScheduledItem scheduledItem;
		protected float scale = 100;
		protected float offsetX, offsetY;

		bool IsValid { get { return skeleton != null; } }

		public SpineVisualElement () {
			RegisterCallback<AttachToPanelEvent>(OnAttachedCallback);
			RegisterCallback<DetachFromPanelEvent>(OnDetatchedCallback);

			AddRendererElement();
		}

		protected void SetActiveRendererCount (int count) {
			if (count == rendererElements.Count)
				return;
			else if (count > rendererElements.Count) {
				int oldCount = rendererElements.Count;
				int reactivateCount = Math.Min(count, rendererElements.Capacity);
				for (int i = oldCount; i < reactivateCount; ++i) {
					EnableRenderElement(rendererElements.Items[i]);
				}
				rendererElements.EnsureCapacity(count);
				for (int i = reactivateCount; i < count; ++i) {
					AddRendererElement();
				}
			} else { // new count < old count
				for (int i = count, oldCount = rendererElements.Count; i < oldCount; ++i)
					DisableRenderElement(rendererElements.Items[i]);
			}
			rendererElements.Count = count;
		}

		protected VisualElement AddRendererElement () {
			VisualElement rendererElement = new VisualElement();
			int index = rendererElements.Count;
			rendererElement.generateVisualContent += (context) => GenerateVisualContents(context, index);
			rendererElement.pickingMode = PickingMode.Ignore;
			rendererElement.style.position = Position.Absolute;
			rendererElement.style.top = 0;
			rendererElement.style.left = 0;
			rendererElement.style.bottom = 0;
			rendererElement.style.right = 0;
			Add(rendererElement);

			rendererElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			rendererElements.Add(rendererElement);
			return rendererElement;
		}

		protected void EnableRenderElement (VisualElement rendererElement) {
			rendererElement.enabledSelf = true;
			rendererElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
		}

		protected void DisableRenderElement (VisualElement rendererElement) {
			rendererElement.enabledSelf = false;
			rendererElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
		}

		protected void RemoveMultiMaterialRendererElements () {
			for (int i = rendererElements.Capacity - 1; i > 0; --i) {
				rendererElements.Items[i].RemoveFromHierarchy();
			}
			rendererElements.Count = 1;
			rendererElements.TrimExcess();
		}

		void OnGeometryChanged (GeometryChangedEvent evt) {
			if (!this.IsValid) return;
			if (referenceMeshBounds.size.x == 0 || referenceMeshBounds.size.y == 0) {
				AdjustReferenceMeshBounds();
			}
			for (int i = 0, count = rendererElements.Count; i < count; ++i)
				AdjustOffsetScaleToMeshBounds(rendererElements.Items[i]);
		}

		void OnAttachedCallback (AttachToPanelEvent evt) {
			Initialize(false);
		}

		void OnDetatchedCallback (DetachFromPanelEvent evt) {
			ClearElement();
		}

		public void ClearElement () {
			skeleton = null;
			DisposeUISubmeshes();
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
			MarkAllDirtyAndRepaint();
		}

		public virtual void Update (float deltaTime) {
			if (!this.IsValid) return;

			if (updateMode < UpdateMode.OnlyAnimationStatus)
				return;
			UpdateAnimationStatus(deltaTime);

			if (updateMode == UpdateMode.OnlyAnimationStatus)
				return;
			ApplyAnimation();
			PrepareInstructionsAndRenderers();
		}

		protected void UpdateAnimationStatus (float deltaTime) {
			deltaTime *= timeScale;
			state.Update(deltaTime);
			skeleton.Update(deltaTime);
		}

		protected void ApplyAnimation () {
			if (updateMode != UpdateMode.OnlyEventTimelines)
				state.Apply(skeleton);
			else
				state.ApplyEventTimelinesOnly(skeleton);

			skeleton.UpdateWorldTransform(Physics.Update);
		}

		public void Initialize (bool overwrite) {
			if (this.IsValid && !overwrite) return;

			if (this.SkeletonDataAsset == null) return;
			var skeletonData = this.SkeletonDataAsset.GetSkeletonData(false);
			if (skeletonData == null) return;

			if (SkeletonDataAsset.atlasAssets.Length <= 0 || SkeletonDataAsset.atlasAssets[0].MaterialCount <= 0) return;

			this.state = new Spine.AnimationState(SkeletonDataAsset.GetAnimationStateData());
			if (state == null) {
				Clear();
				return;
			}

			this.skeleton = new Skeleton(skeletonData) {
				ScaleX = 1,
				ScaleY = -1
			};

			// Set the initial Skin and Animation
			if (!string.IsNullOrEmpty(initialSkinName)) {
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					if (skeletonData.FindSkin(initialSkinName) == null) {
						initialSkinName = "default";
					}
				}
#endif
				skeleton.SetSkin(initialSkinName);
			}

			string displayedAnimation = Application.isPlaying ? startingAnimation : boundsAnimation;
			if (!string.IsNullOrEmpty(displayedAnimation)) {
				var animationObject = skeletonData.FindAnimation(displayedAnimation);
				if (animationObject != null) {
					state.SetAnimation(0, animationObject, startingLoop);
				}
			}
			if (referenceMeshBounds.size.x == 0 || referenceMeshBounds.size.y == 0) {
				AdjustReferenceMeshBounds();
				for (int i = 0, count = rendererElements.Count; i < count; ++i)
					AdjustOffsetScaleToMeshBounds(rendererElements.Items[i]);
			}

			if (scheduledItem == null)
				scheduledItem = schedule.Execute(Update).Every(1);

			if (!Application.isPlaying)
				Update(0.0f);

			MarkAllDirtyAndRepaint();
		}

		protected void MarkAllDirtyAndRepaint () {
			for (int i = 0, count = rendererElements.Count; i < count; ++i) {
				var rendererElement = rendererElements.Items[i];
				if (rendererElement != null) rendererElement.MarkDirtyRepaint();
			}
		}

		protected void UpdateAnimation () {
			this.state.ClearTracks();
			skeleton.SetupPose();

			string displayedAnimation = Application.isPlaying ? startingAnimation : boundsAnimation;
			if (!string.IsNullOrEmpty(displayedAnimation)) {
				var animationObject = SkeletonDataAsset.GetSkeletonData(false).FindAnimation(displayedAnimation);
				if (animationObject != null) {
					state.SetAnimation(0, animationObject, startingLoop);
				}
			}
			if (referenceMeshBounds.size.x == 0 || referenceMeshBounds.size.y == 0) {
				AdjustReferenceMeshBounds();
				for (int i = 0, count = rendererElements.Count; i < count; ++i)
					AdjustOffsetScaleToMeshBounds(rendererElements.Items[i]);
			}
			Update(0.0f);
			MarkAllDirtyAndRepaint();
		}

		protected void PrepareInstructionsAndRenderers () {
			MeshGeneratorUIElements.GenerateSkeletonRendererInstruction(currentInstructions, skeleton, null,
				null, false, false);

			int submeshCount = currentInstructions.submeshInstructions.Count;
			PrepareUISubmeshCount(submeshCount);

			if (supportMultipleMaterials) {
				SetActiveRendererCount(submeshCount);
				if (supportMultipleMaterials) {
					for (int i = 0, count = rendererElements.Count; i < count; ++i) {
						AssignBlendModeMaterial(i, currentInstructions.submeshInstructions.Items[i].material);
					}
				}
			} else if (rendererElements.Count > 0) {
				if (blendModeMaterials != null && blendModeMaterials.normalMaterial)
					rendererElements.Items[0].style.unityMaterial = blendModeMaterials.normalMaterial;
				else
					rendererElements.Items[0].style.unityMaterial = null;
			}
		}

		protected class UISubmesh {
			public NativeArray<UIVertex>? vertices = null;
			public NativeArray<ushort>? indices = null;

			public NativeSlice<UIVertex> verticesSlice;
			public NativeSlice<ushort> indicesSlice;
		}
		protected readonly ExposedList<UISubmesh> uiSubmeshes = new ExposedList<UISubmesh>();

		protected void GenerateVisualContents (MeshGenerationContext context, int rendererElementIndex) {
			if (!this.IsValid) return;
			if (!context.visualElement.enabledInHierarchy) return;

			int submeshesPerRenderer = supportMultipleMaterials ? 1 : currentInstructions.submeshInstructions.Count;
			int submeshOffset = rendererElementIndex;

			meshGenerator.settings.pmaVertexColors = false;
			for (int i = submeshOffset; i < submeshOffset + submeshesPerRenderer; i++) {
				var submeshInstructionItem = currentInstructions.submeshInstructions.Items[i];
				UISubmesh uiSubmesh = uiSubmeshes.Items[i];

				meshGenerator.Begin();
				meshGenerator.AddSubmesh(submeshInstructionItem);

				PrepareUISubmesh(uiSubmesh, meshGenerator.VertexCount, meshGenerator.SubmeshIndexCount(0));
				if (flipBackFaces)
					meshGenerator.FlipBackfaceWindingOrder();
				meshGenerator.FillVertexData(ref uiSubmesh.verticesSlice);
				meshGenerator.FillTrianglesSingleSubmesh(ref uiSubmesh.indicesSlice);
			
				var submeshMaterial = submeshInstructionItem.material;
				Texture usedTexture = submeshMaterial.mainTexture;
				FillContext(context, uiSubmesh, usedTexture);
			}
		}

		protected void AssignBlendModeMaterial (int rendererElementIndex, Material originalSubmeshMaterial) {
			if (skeletonDataAsset == null) return;
			VisualElement rendererElement = rendererElements.Items[rendererElementIndex];
			if (blendModeMaterials == null) {
				rendererElement.style.unityMaterial = null;
				return;
			}
			BlendModeMaterials requiredBlendModeMaterials = skeletonDataAsset.blendModeMaterials;
			if (!requiredBlendModeMaterials.RequiresBlendModeMaterials) {
				rendererElement.style.unityMaterial = blendModeMaterials.normalMaterial;
				return;
			}
			Material material = null;
			BlendMode blendMode = requiredBlendModeMaterials.BlendModeForMaterial(originalSubmeshMaterial);
			if (blendMode == BlendMode.Normal)
				material = blendModeMaterials.normalMaterial;
			else if (blendMode == BlendMode.Additive)
				material = blendModeMaterials.additiveMaterial;
			else if (blendMode == BlendMode.Multiply)
				material = blendModeMaterials.multiplyMaterial;
			else if (blendMode == BlendMode.Screen)
				material = blendModeMaterials.screenMaterial;
			rendererElement.style.unityMaterial = material;
		}

		protected void PrepareUISubmeshCount (int targetCount) {
			int oldCount = uiSubmeshes.Count;
			uiSubmeshes.EnsureCapacity(targetCount);
			for (int i = oldCount; i < targetCount; ++i) {
				uiSubmeshes.Add(new UISubmesh());
			}
		}

		protected void PrepareUISubmesh (UISubmesh uiSubmesh, int vertexCount, int indexCount) {
			bool shallReallocateVertices = uiSubmesh.vertices == null || uiSubmesh.vertices.Value.Length < vertexCount;
			if (shallReallocateVertices) {
				int allocationCount = vertexCount;
				if (uiSubmesh.vertices != null) {
					allocationCount = Math.Max(vertexCount, 2 * uiSubmesh.vertices.Value.Length);
					uiSubmesh.vertices.Value.Dispose();
				}
				uiSubmesh.vertices = new NativeArray<UIVertex>(allocationCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			}
			if (shallReallocateVertices || uiSubmesh.verticesSlice.Length != vertexCount) {
				uiSubmesh.verticesSlice = new NativeSlice<UIVertex>(uiSubmesh.vertices.Value, 0, vertexCount);
			}

			bool shallReallocateIndices = uiSubmesh.indices == null || uiSubmesh.indices.Value.Length < indexCount;
			if (shallReallocateIndices) {
				int allocationCount = indexCount;
				if (uiSubmesh.indices != null) {
					allocationCount = Math.Max(indexCount, uiSubmesh.indices.Value.Length * 2);
					uiSubmesh.indices.Value.Dispose();
				}
				uiSubmesh.indices = new NativeArray<ushort>(allocationCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			}
			if (shallReallocateIndices || uiSubmesh.indicesSlice.Length != indexCount) {
				uiSubmesh.indicesSlice = new NativeSlice<ushort>(uiSubmesh.indices.Value, 0, indexCount);
			}
		}

		protected void DisposeUISubmeshes () {
			for (int i = 0, count = uiSubmeshes.Count; i < count; ++i) {
				UISubmesh uiSubmesh = uiSubmeshes.Items[i];
				if (uiSubmesh.vertices != null) uiSubmesh.vertices.Value.Dispose();
				if (uiSubmesh.indices != null) uiSubmesh.indices.Value.Dispose();
			}
			uiSubmeshes.Clear();
		}

		void FillContext (MeshGenerationContext context, UISubmesh submesh, Texture texture) {
			MeshWriteData meshWriteData = context.Allocate(submesh.verticesSlice.Length, submesh.indicesSlice.Length, texture);

			meshWriteData.SetAllVertices(submesh.verticesSlice);
			meshWriteData.SetAllIndices(submesh.indicesSlice);
		}

		public void AdjustReferenceMeshBounds () {
			if (skeleton == null)
				return;

			// Need one update to obtain valid mesh bounds
			Update(0.0f);
			MeshGeneratorUIElements.GenerateSkeletonRendererInstruction(currentInstructions, skeleton,
				null, null, false, false);
			int submeshCount = currentInstructions.submeshInstructions.Count;
			meshGenerator.Begin();

			for (int i = 0; i < submeshCount; i++) {
				var submeshInstructionItem = currentInstructions.submeshInstructions.Items[i];
				meshGenerator.AddSubmesh(submeshInstructionItem);
			}
			Bounds meshBounds = meshGenerator.GetMeshBounds();
			if (meshBounds.extents.x == 0 || meshBounds.extents.y == 0) {
				ReferenceBounds = new Bounds(Vector3.zero, Vector3.one * 2f);
			} else {
				ReferenceBounds = meshBounds;
			}
		}

		void AdjustOffsetScaleToMeshBounds (VisualElement visualElement) {
			if (visualElement == null) return;
			Rect targetRect = visualElement.layout;
			if (float.IsNaN(targetRect.width)) return;

			float xScale = targetRect.width / referenceMeshBounds.size.x;
			float yScale = targetRect.height / referenceMeshBounds.size.y;
			this.scale = Math.Min(xScale, yScale);
			float targetOffsetX = targetRect.width / 2;
			float targetOffsetY = targetRect.height / 2;
			this.offsetX = targetOffsetX - referenceMeshBounds.center.x * this.scale;
			this.offsetY = targetOffsetY - referenceMeshBounds.center.y * this.scale;

			visualElement.style.translate = new StyleTranslate(new Translate(offsetX, offsetY, 0));
			visualElement.style.transformOrigin = new TransformOrigin(0, 0, 0);
			visualElement.style.scale = new Scale(new Vector3(scale, scale, 1));
		}
	}
}
