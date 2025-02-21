/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2024, Esoteric Software LLC
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

using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UIVertex = UnityEngine.UIElements.Vertex;

namespace Spine.Unity {

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

	[UxmlElement]
	public partial class SpineVisualElement : VisualElement {

		[UxmlAttribute]
		public SkeletonDataAsset SkeletonDataAsset {
			get { return skeletonDataAsset; }
			set {
				if (skeletonDataAsset == value) return;
				skeletonDataAsset = value;
#if UNITY_EDITOR
				if (!Application.isPlaying)
					Initialize(true);
#endif
			}
		}
		public SkeletonDataAsset skeletonDataAsset;

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

		[UxmlAttribute] public bool startingLoop { get; set; } = true;
		[UxmlAttribute] public float timeScale { get; set; } = 1.0f;

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

				AdjustOffsetScaleToMeshBounds(rendererElement);
			}
		}
		public Bounds referenceMeshBounds;

		public AnimationState AnimationState {
			get {
				Initialize(false);
				return state;
			}
		}
		[UxmlAttribute]
		public bool freeze { get; set; }
		[UxmlAttribute]
		public bool unscaledTime { get; set; }

		/// <summary>Update mode to optionally limit updates to e.g. only apply animations but not update the mesh.</summary>
		public UpdateMode UpdateMode { get { return updateMode; } set { updateMode = value; } }
		protected UpdateMode updateMode = UpdateMode.FullUpdate;

		protected AnimationState state = null;
		protected Skeleton skeleton = null;
		protected SkeletonRendererInstruction currentInstructions = new();// to match existing code better
		protected Spine.Unity.MeshGeneratorUIElements meshGenerator = new MeshGeneratorUIElements();

		protected VisualElement rendererElement;
		IVisualElementScheduledItem scheduledItem;
		protected float scale = 100;
		protected float offsetX, offsetY;

		bool IsValid { get { return skeleton != null; } }

		public SpineVisualElement () {
			RegisterCallback<AttachToPanelEvent>(OnAttachedCallback);
			RegisterCallback<DetachFromPanelEvent>(OnDetatchedCallback);

			rendererElement = new VisualElement();
			rendererElement.generateVisualContent += GenerateVisualContents;
			rendererElement.pickingMode = PickingMode.Ignore;
			rendererElement.style.position = Position.Absolute;
			rendererElement.style.top = 0;
			rendererElement.style.left = 0;
			rendererElement.style.bottom = 0;
			rendererElement.style.right = 0;
			Add(rendererElement);

			rendererElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
		}

		void OnGeometryChanged (GeometryChangedEvent evt) {
			if (!this.IsValid) return;
			if (referenceMeshBounds.size.x == 0 || referenceMeshBounds.size.y == 0) {
				AdjustReferenceMeshBounds();
			}
			AdjustOffsetScaleToMeshBounds(rendererElement);
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
			rendererElement.MarkDirtyRepaint();
		}

		public virtual void Update (float deltaTime) {
			if (!this.IsValid) return;

			if (updateMode < UpdateMode.OnlyAnimationStatus)
				return;
			UpdateAnimationStatus(deltaTime);

			if (updateMode == UpdateMode.OnlyAnimationStatus)
				return;
			ApplyAnimation();
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

			skeleton.UpdateWorldTransform(Skeleton.Physics.Update);
		}

		void Initialize (bool overwrite) {
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
			if (!string.IsNullOrEmpty(initialSkinName))
				skeleton.SetSkin(initialSkinName);

			string displayedAnimation = Application.isPlaying ? startingAnimation : boundsAnimation;
			if (!string.IsNullOrEmpty(displayedAnimation)) {
				var animationObject = skeletonData.FindAnimation(displayedAnimation);
				if (animationObject != null) {
					state.SetAnimation(0, animationObject, startingLoop);
				}
			}
			if (referenceMeshBounds.size.x == 0 || referenceMeshBounds.size.y == 0) {
				AdjustReferenceMeshBounds();
				AdjustOffsetScaleToMeshBounds(rendererElement);
			}

			if (scheduledItem == null)
				scheduledItem = schedule.Execute(Update).Every(1);

			if (!Application.isPlaying)
				Update(0.0f);

			rendererElement.MarkDirtyRepaint();
		}

		protected void UpdateAnimation () {
			this.state.ClearTracks();
			skeleton.SetToSetupPose();

			string displayedAnimation = Application.isPlaying ? startingAnimation : boundsAnimation;
			if (!string.IsNullOrEmpty(displayedAnimation)) {
				var animationObject = SkeletonDataAsset.GetSkeletonData(false).FindAnimation(displayedAnimation);
				if (animationObject != null) {
					state.SetAnimation(0, animationObject, startingLoop);
				}
			}
			if (referenceMeshBounds.size.x == 0 || referenceMeshBounds.size.y == 0) {
				AdjustReferenceMeshBounds();
				AdjustOffsetScaleToMeshBounds(rendererElement);
			}
			Update(0.0f);

			rendererElement.MarkDirtyRepaint();
		}

		protected class UISubmesh {
			public NativeArray<UIVertex>? vertices = null;
			public NativeArray<ushort>? indices = null;

			public NativeSlice<UIVertex> verticesSlice;
			public NativeSlice<ushort> indicesSlice;
		}
		protected readonly ExposedList<UISubmesh> uiSubmeshes = new ExposedList<UISubmesh>();

		protected void GenerateVisualContents (MeshGenerationContext context) {
			if (!this.IsValid) return;

			MeshGeneratorUIElements.GenerateSkeletonRendererInstruction(currentInstructions, skeleton, null,
				null,
				 false,
				false);

			int submeshCount = currentInstructions.submeshInstructions.Count;
			PrepareUISubmeshCount(submeshCount);

			// Generate meshes.
			for (int i = 0; i < submeshCount; i++) {
				var submeshInstructionItem = currentInstructions.submeshInstructions.Items[i];
				UISubmesh uiSubmesh = uiSubmeshes.Items[i];

				meshGenerator.Begin();
				meshGenerator.AddSubmesh(submeshInstructionItem);
				// clipping is done, vertex counts are final.

				PrepareUISubmesh(uiSubmesh, meshGenerator.VertexCount, meshGenerator.SubmeshIndexCount(0));
				meshGenerator.FillVertexData(ref uiSubmesh.verticesSlice);
				meshGenerator.FillTrianglesSingleSubmesh(ref uiSubmesh.indicesSlice);

				var submeshMaterial = submeshInstructionItem.material;

				Texture usedTexture = submeshMaterial.mainTexture;

				FillContext(context, uiSubmesh, usedTexture);
			}
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
