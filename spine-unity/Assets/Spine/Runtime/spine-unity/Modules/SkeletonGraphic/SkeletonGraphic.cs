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
using UnityEngine.UI;
using Spine;

namespace Spine.Unity {
	[ExecuteInEditMode, RequireComponent(typeof(CanvasRenderer), typeof(RectTransform)), DisallowMultipleComponent]
	[AddComponentMenu("Spine/SkeletonGraphic (Unity UI Canvas)")]
	public class SkeletonGraphic : MaskableGraphic, ISkeletonComponent, IAnimationStateComponent, ISkeletonAnimation, IHasSkeletonDataAsset {

		#region Inspector
		public SkeletonDataAsset skeletonDataAsset;
		public SkeletonDataAsset SkeletonDataAsset { get { return skeletonDataAsset; } }

		[SpineSkin(dataField:"skeletonDataAsset")]
		public string initialSkinName = "default";
		public bool initialFlipX, initialFlipY;

		[SpineAnimation(dataField:"skeletonDataAsset")]
		public string startingAnimation;
		public bool startingLoop;
		public float timeScale = 1f;
		public bool freeze;
		public bool unscaledTime;

		#if UNITY_EDITOR
		protected override void OnValidate () {
			// This handles Scene View preview.
			base.OnValidate ();
			if (this.IsValid) {
				if (skeletonDataAsset == null) {
					Clear();
					startingAnimation = "";
				} else if (skeletonDataAsset.GetSkeletonData(true) != skeleton.data) {
					Clear();
					Initialize(true);
					startingAnimation = "";
					if (skeletonDataAsset.atlasAssets.Length > 1 || skeletonDataAsset.atlasAssets[0].materials.Length > 1)
						Debug.LogError("Unity UI does not support multiple textures per Renderer. Your skeleton will not be rendered correctly. Recommend using SkeletonAnimation instead. This requires the use of a Screen space camera canvas.");
				} else {
					if (freeze) return;

					if (!string.IsNullOrEmpty(initialSkinName)) {
						var skin = skeleton.data.FindSkin(initialSkinName);
						if (skin != null) {
							if (skin == skeleton.data.defaultSkin)
								skeleton.SetSkin((Skin)null);
							else
								skeleton.SetSkin(skin);
						}
							
					}

					// Only provide visual feedback to inspector changes in Unity Editor Edit mode.
					if (!Application.isPlaying) {
						skeleton.flipX = this.initialFlipX;
						skeleton.flipY = this.initialFlipY;

						skeleton.SetToSetupPose();
						if (!string.IsNullOrEmpty(startingAnimation))
							skeleton.PoseWithAnimation(startingAnimation, 0f, false);
					}

				}
			} else {
				if (skeletonDataAsset != null)
					Initialize(true);
			}				
		}

		protected override void Reset () {
			base.Reset();
			if (material == null || material.shader != Shader.Find("Spine/SkeletonGraphic (Premultiply Alpha)"))
				Debug.LogWarning("SkeletonGraphic works best with the SkeletonGraphic material.");			
		}
		#endif
		#endregion

		#region Runtime Instantiation
		public static SkeletonGraphic NewSkeletonGraphicGameObject (SkeletonDataAsset skeletonDataAsset, Transform parent) {
			SkeletonGraphic sg = SkeletonGraphic.AddSkeletonGraphicComponent(new GameObject("New Spine GameObject"), skeletonDataAsset);
			if (parent != null) sg.transform.SetParent(parent, false);
			return sg;
		}

		public static SkeletonGraphic AddSkeletonGraphicComponent (GameObject gameObject, SkeletonDataAsset skeletonDataAsset) {
			var c = gameObject.AddComponent<SkeletonGraphic>();
			if (skeletonDataAsset != null) {
				c.skeletonDataAsset = skeletonDataAsset;				
				c.Initialize(false);
			}
			return c;
		}
		#endregion

		#region Internals
		// This is used by the UI system to determine what to put in the MaterialPropertyBlock.
		Texture overrideTexture;
		public Texture OverrideTexture {
			get { return overrideTexture; }
			set {
				overrideTexture = value;
				canvasRenderer.SetTexture(this.mainTexture); // Refresh canvasRenderer's texture. Make sure it handles null.
			}
		}
		public override Texture mainTexture {
			get { 
				// Fail loudly when incorrectly set up.
				if (overrideTexture != null) return overrideTexture;
				return skeletonDataAsset == null ? null : skeletonDataAsset.atlasAssets[0].materials[0].mainTexture;
			}
		}

		protected override void Awake () {
			base.Awake ();
			if (!this.IsValid) {
				Initialize(false);
				Rebuild(CanvasUpdate.PreRender);
			}
		}

		public override void Rebuild (CanvasUpdate update) {
			base.Rebuild(update);
			if (canvasRenderer.cull) return;
			if (update == CanvasUpdate.PreRender) UpdateMesh();
		}

		public virtual void Update () {
			if (freeze) return;
			Update(unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
		}

		public virtual void Update (float deltaTime) {
			if (!this.IsValid) return;

			deltaTime *= timeScale;
			skeleton.Update(deltaTime);
			state.Update(deltaTime);
			state.Apply(skeleton);

			if (UpdateLocal != null) UpdateLocal(this);

			skeleton.UpdateWorldTransform();

			if (UpdateWorld != null) { 
				UpdateWorld(this);
				skeleton.UpdateWorldTransform();
			}

			if (UpdateComplete != null) UpdateComplete(this);
		}

		public void LateUpdate () {
			if (freeze) return;
			//this.SetVerticesDirty(); // Which is better?
			UpdateMesh();
		}
		#endregion

		#region API
		protected Skeleton skeleton;
		public Skeleton Skeleton { get { return skeleton; } }
		public SkeletonData SkeletonData { get { return skeleton == null ? null : skeleton.data; } }
		public bool IsValid { get { return skeleton != null; } }

		protected Spine.AnimationState state;
		public Spine.AnimationState AnimationState { get { return state; } }

		[SerializeField] protected Spine.Unity.MeshGenerator meshGenerator = new MeshGenerator();
		public Spine.Unity.MeshGenerator MeshGenerator { get { return this.meshGenerator; } }
		DoubleBuffered<Spine.Unity.MeshRendererBuffers.SmartMesh> meshBuffers;
		SkeletonRendererInstruction currentInstructions = new SkeletonRendererInstruction();

		public Mesh GetLastMesh () {
			return meshBuffers.GetCurrent().mesh;
		}

		public event UpdateBonesDelegate UpdateLocal;
		public event UpdateBonesDelegate UpdateWorld;
		public event UpdateBonesDelegate UpdateComplete;

		/// <summary> Occurs after the vertex data populated every frame, before the vertices are pushed into the mesh.</summary>
		public event Spine.Unity.MeshGeneratorDelegate OnPostProcessVertices;

		public void Clear () {
			skeleton = null;
			canvasRenderer.Clear();
		}

		public void Initialize (bool overwrite) {
			if (this.IsValid && !overwrite) return;

			// Make sure none of the stuff is null
			if (this.skeletonDataAsset == null) return;
			var skeletonData = this.skeletonDataAsset.GetSkeletonData(false);
			if (skeletonData == null) return;

			if (skeletonDataAsset.atlasAssets.Length <= 0 || skeletonDataAsset.atlasAssets[0].materials.Length <= 0) return;

			this.state = new Spine.AnimationState(skeletonDataAsset.GetAnimationStateData());
			if (state == null) {
				Clear();
				return;
			}

			this.skeleton = new Skeleton(skeletonData) {
				flipX = this.initialFlipX,
				flipY = this.initialFlipY
			};

			meshBuffers = new DoubleBuffered<MeshRendererBuffers.SmartMesh>();
			canvasRenderer.SetTexture(this.mainTexture); // Needed for overwriting initializations.

			// Set the initial Skin and Animation
			if (!string.IsNullOrEmpty(initialSkinName))
				skeleton.SetSkin(initialSkinName);

			#if UNITY_EDITOR
			if (!string.IsNullOrEmpty(startingAnimation)) {
				if (Application.isPlaying) {
					state.SetAnimation(0, startingAnimation, startingLoop);
				} else {
					// Assume SkeletonAnimation is valid for skeletonData and skeleton. Checked above.
					var animationObject = skeletonDataAsset.GetSkeletonData(false).FindAnimation(startingAnimation);
					if (animationObject != null)
						animationObject.PoseSkeleton(skeleton, 0);
				}
				Update(0);
			}
			#else
			if (!string.IsNullOrEmpty(startingAnimation)) {
				state.SetAnimation(0, startingAnimation, startingLoop);
				Update(0);
			}
			#endif
		}

		public void UpdateMesh () {
			if (!this.IsValid) return;

			skeleton.SetColor(this.color);
			var smartMesh = meshBuffers.GetNext();
			var currentInstructions = this.currentInstructions;

			MeshGenerator.GenerateSingleSubmeshInstruction(currentInstructions, skeleton, this.material);
			bool updateTriangles = SkeletonRendererInstruction.GeometryNotEqual(currentInstructions, smartMesh.instructionUsed);

			meshGenerator.Begin();
			if (currentInstructions.hasActiveClipping) {
				meshGenerator.AddSubmesh(currentInstructions.submeshInstructions.Items[0], updateTriangles);
			} else {
				meshGenerator.BuildMeshWithArrays(currentInstructions, updateTriangles);
			}

			if (canvas != null) meshGenerator.ScaleVertexData(canvas.referencePixelsPerUnit);
			if (OnPostProcessVertices != null) OnPostProcessVertices.Invoke(this.meshGenerator.Buffers);

			var mesh = smartMesh.mesh;
			meshGenerator.FillVertexData(mesh);
			if (updateTriangles) meshGenerator.FillTrianglesSingle(mesh);
			meshGenerator.FillLateVertexData(mesh);

			canvasRenderer.SetMesh(mesh);
			smartMesh.instructionUsed.Set(currentInstructions);

			//this.UpdateMaterial(); // TODO: This allocates memory.
		}
		#endregion
	}
}
