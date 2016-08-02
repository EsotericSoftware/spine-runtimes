/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
#if (UNITY_5_0 || UNITY_5_1 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
#define PREUNITY_5_2
#endif

using UnityEngine;
using UnityEngine.UI;
using Spine;

namespace Spine.Unity {
	[ExecuteInEditMode, RequireComponent(typeof(CanvasRenderer), typeof(RectTransform)), DisallowMultipleComponent]
	[AddComponentMenu("Spine/SkeletonGraphic (Unity UI Canvas)")]
	public class SkeletonGraphic : MaskableGraphic, ISkeletonComponent, IAnimationStateComponent, ISkeletonAnimation {

		#region Inspector
		public SkeletonDataAsset skeletonDataAsset;
		public SkeletonDataAsset SkeletonDataAsset { get { return skeletonDataAsset; } }

		[SpineSkin(dataField:"skeletonDataAsset")]
		public string initialSkinName = "default";

		[SpineAnimation(dataField:"skeletonDataAsset")]
		public string startingAnimation;
		public bool startingLoop;
		public float timeScale = 1f;
		public bool freeze;

		#if UNITY_EDITOR
		protected override void OnValidate () {
			// This handles Scene View preview.
			base.OnValidate ();
			#if !PREUNITY_5_2
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
					skeleton.SetToSetupPose();
					if (!string.IsNullOrEmpty(startingAnimation))
						skeleton.PoseWithAnimation(startingAnimation, 0f, false);
				}
			} else {
				if (skeletonDataAsset != null)
					Initialize(true);
			}
			#else
			Debug.LogWarning("SkeletonGraphic requres Unity 5.2 or higher.\nUnityEngine.UI 5.1 and below does not accept meshes and can't be used to render Spine skeletons. You may delete the SkeletonGraphic folder under `Modules` if you want to exclude it from your project." );
			#endif
				
		}

		protected override void Reset () {
			base.Reset();
			if (material == null || material.shader != Shader.Find("Spine/SkeletonGraphic (Premultiply Alpha)"))
				Debug.LogWarning("SkeletonGraphic works best with the SkeletonGraphic material.");			
		}
		#endif
		#endregion

		#if !PREUNITY_5_2
		#region Internals
		// This is used by the UI system to determine what to put in the MaterialPropertyBlock.
		public override Texture mainTexture {
			get { 
				// Fail loudly when incorrectly set up.
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
			Update(Time.deltaTime);
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

		// This is any object that can give you a mesh when you give it a skeleton to render.
		protected Spine.Unity.MeshGeneration.ISimpleMeshGenerator spineMeshGenerator;
		public Spine.Unity.MeshGeneration.ISimpleMeshGenerator SpineMeshGenerator { get { return this.spineMeshGenerator; } }

		public event UpdateBonesDelegate UpdateLocal;
		public event UpdateBonesDelegate UpdateWorld;
		public event UpdateBonesDelegate UpdateComplete;

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

			this.skeleton = new Skeleton(skeletonData);
			this.spineMeshGenerator = new Spine.Unity.MeshGeneration.ArraysSimpleMeshGenerator(); // You can switch this out with any other implementer of Spine.Unity.MeshGeneration.ISimpleMeshGenerator

			// Set the initial Skin and Animation
			if (!string.IsNullOrEmpty(initialSkinName))
				skeleton.SetSkin(initialSkinName);

			if (!string.IsNullOrEmpty(startingAnimation))
				state.SetAnimation(0, startingAnimation, startingLoop);
		}

		public void UpdateMesh () {
			if (this.IsValid) {
				skeleton.SetColor(this.color);
				if (canvas != null)
					spineMeshGenerator.Scale = canvas.referencePixelsPerUnit; //JOHN: left a todo: move this to a listener to of the canvas?

				canvasRenderer.SetMesh(spineMeshGenerator.GenerateMesh(skeleton));
				//this.UpdateMaterial(); // TODO: This allocates memory.
			}
		}
		#endregion
		#else
		public Skeleton Skeleton { get { return null; } }
		public AnimationState AnimationState { get { return null; } }
		public event UpdateBonesDelegate UpdateLocal, UpdateWorld, UpdateComplete;
		public void LateUpdate () { }
		#endif
	}
}