/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

#if UNITY_2019_3_OR_NEWER
#define CONFIGURABLE_ENTER_PLAY_MODE
#endif

#if !SPINE_DISABLE_THREADING
#define USE_THREADED_ANIMATION_UPDATE
#endif

#if !SPINE_AUTO_UPGRADE_COMPONENTS_OFF
#define AUTO_UPGRADE_TO_43_COMPONENTS
#endif

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[System.Serializable]
	public abstract class SkeletonAnimationBase : MonoBehaviour,
		ISkeletonAnimation, ISkeletonComponent, IHasSkeletonRenderer,
		ISkeletonRendererEvents, IHasModifyableSkeletonDataAsset, IUpgradable {

		[SerializeField] protected UpdateTiming updateTiming = UpdateTiming.InUpdate;
		protected int frameOfLastUpdate = -1;
		protected ISkeletonRenderer skeletonRenderer;
		protected bool skipUpdate = false;

		public UpdateTiming UpdateTiming {
			get { return updateTiming; }
			set {
				if (updateTiming == value) return;
#if USE_THREADED_ANIMATION_UPDATE
				if (Application.isPlaying && UsesThreadedAnimation && isUpdatedExternally) {
					SkeletonUpdateSystem system = SkeletonUpdateSystem.Instance;
					if (system) {
						// unregister from old update timing mode, register for new one.
						system.UnregisterFromUpdate(updateTiming, this);
						system.RegisterForUpdate(value, this);
					}
				}
#endif
				updateTiming = value;
			}
		}

#if UNITY_EDITOR
		protected bool requiresEditorUpdate = false;
#endif
#if USE_THREADED_ANIMATION_UPDATE
		#region Threaded update system
		protected static float externalDeltaTime = 0f;
		protected static float unscaledDeltaTime = 0f;

		[SerializeField] protected SettingsTriState threadedAnimation = SettingsTriState.UseGlobalSetting;
		protected bool isUpdatedExternally = false;

		public static float ExternalDeltaTime {
			get { return externalDeltaTime; }
			set { externalDeltaTime = value; }
		}
		public static float ExternalUnscaledDeltaTime {
			get { return unscaledDeltaTime; }
			set { unscaledDeltaTime = value; }
		}
		public virtual float UsedExternalDeltaTime { get { return ExternalDeltaTime; } }

		public bool IsUpdatedExternally {
			get { return isUpdatedExternally; }
			set { isUpdatedExternally = value; }
		}

		protected bool UsesThreadedAnimation {
			get { return threadedAnimation == SettingsTriState.Enable || RuntimeSettings.UseThreadedAnimation; }
		}

		public SettingsTriState ThreadedAnimation {
			get { return threadedAnimation; }
			set {
				if (threadedAnimation == value) return;
				threadedAnimation = value;

				SkeletonUpdateSystem system = SkeletonUpdateSystem.Instance;
				if (system) {
					if (threadedAnimation == SettingsTriState.Enable || RuntimeSettings.UseThreadedAnimation)
						system.RegisterForUpdate(updateTiming, this);
					else
						system.UnregisterFromUpdate(updateTiming, this);
				}
			}
		}
		#endregion

#if UNITY_EDITOR
		static bool applicationIsPlaying = false;
		// ApplicationIsPlaying for threaded access. Unfortunately Application.isPlaying throws
		// when called from worker thread.
		public static bool ApplicationIsPlaying {
			get { return applicationIsPlaying; }
			set { applicationIsPlaying = value; }
		}
#endif
#else
		protected bool UseThreading {
			get { return false; }
			set { }
		}
#if UNITY_EDITOR
		public static bool ApplicationIsPlaying {
			get { return Application.isPlaying; }
			set { }
		}
#endif
#endif

		#region Interface Implementation
		public UnityEngine.MonoBehaviour Component { get { return this; } }

		public ISkeletonRenderer Renderer {
			get {
				if (skeletonRenderer == null) {
					skeletonRenderer = this.GetComponent<ISkeletonRenderer>();
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
					if (skeletonRenderer == null) {
						UpgradeTo43();
						skeletonRenderer = this.GetComponent<ISkeletonRenderer>();
					}
#endif
				}
				return skeletonRenderer;
			}
		}
		public Skeleton Skeleton {
			get { return Renderer.Skeleton; }
			set { Renderer.Skeleton = value; }
		}
		public Skeleton skeleton {
			get { return Renderer.Skeleton; }
			set { Renderer.Skeleton = value; }
		}
		public SkeletonDataAsset SkeletonDataAsset {
			get { return Renderer.SkeletonDataAsset; }
			set { Renderer.SkeletonDataAsset = value; }
		}
		public SkeletonDataAsset skeletonDataAsset {
			get { return Renderer.SkeletonDataAsset; }
			set { Renderer.SkeletonDataAsset = value; }
		}

		protected event SkeletonAnimationDelegate _BeforeUpdate, _BeforeApply, _OnAnimationRebuild;

		/// <summary>OnAnimationRebuild is raised after the SkeletonAnimation component is successfully initialized.</summary>
		public event SkeletonAnimationDelegate OnAnimationRebuild { add { _OnAnimationRebuild += value; } remove { _OnAnimationRebuild -= value; } }

		/// <summary>
		/// Occurs before the animation state is updated.
		/// Use this callback when you want to change the skeleton state before animation state is updated.
		/// </summary>
		public event SkeletonAnimationDelegate BeforeUpdate { add { _BeforeUpdate += value; } remove { _BeforeUpdate -= value; } }

		/// <summary>
		/// Occurs before the animations are applied.
		/// Use this callback when you want to change the skeleton state before animations are applied on top.
		/// </summary>
		public event SkeletonAnimationDelegate BeforeApply { add { _BeforeApply += value; } remove { _BeforeApply -= value; } }

		/// <summary>A compatibility wrapper for <see cref="SkeletonRenderer.UpdateLocal"/></summary>
		public event SkeletonRendererDelegate UpdateLocal { add { Renderer.UpdateLocal += value; } remove { Renderer.UpdateLocal -= value; } }
		/// <summary>A compatibility wrapper for <see cref="Renderer.UpdateWorld"/></summary>
		public event SkeletonRendererDelegate UpdateWorld { add { Renderer.UpdateWorld += value; } remove { Renderer.UpdateWorld -= value; } }
		/// <summary>A compatibility wrapper for <see cref="Renderer.UpdateComplete"/></summary>
		public event SkeletonRendererDelegate UpdateComplete { add { Renderer.UpdateComplete += value; } remove { Renderer.UpdateComplete -= value; } }
		/// <summary>A compatibility wrapper for <see cref="Renderer.OnRebuild"/></summary>
		public event SkeletonRendererDelegate OnRebuild { add { Renderer.OnRebuild += value; } remove { Renderer.OnRebuild -= value; } }
		/// <summary>A compatibility wrapper for <see cref="Renderer.OnMeshAndMaterialsUpdated"/></summary>
		public event SkeletonRendererDelegate OnMeshAndMaterialsUpdated { add { Renderer.OnMeshAndMaterialsUpdated += value; } remove { Renderer.OnMeshAndMaterialsUpdated -= value; } }

		/// <summary>Update mode to optionally limit updates to e.g. only apply animations but not update the mesh.</summary>
		public UpdateMode UpdateMode { get { return Renderer.UpdateMode; } set { Renderer.UpdateMode = value; } }

		public abstract bool IsValid { get; }
		#endregion

		public virtual void Awake () {
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
			UpgradeTo43();
#endif
#if UNITY_EDITOR
			SkeletonAnimationBase.ApplicationIsPlaying = Application.isPlaying;
#endif
			EnsureRendererEventsSubscribed();
		}

		public void EnsureRendererEventsSubscribed () {
			Renderer.OnRebuild -= OnRendererRebuild;
			Renderer.OnRebuild += OnRendererRebuild;
		}

#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		// compatibility layer between 4.1 and 4.2, automatically transfer serialized attributes.
		public abstract void UpgradeTo43 ();
#endif

		/// <summary>
		/// Initialize the associated renderer component and subsequently this animation component.
		/// Creates the internal Spine objects and buffers.</summary>
		/// <param name="overwrite">If set to <c>true</c>, force overwrite an already initialized object.</param>
		public virtual void Initialize (bool overwrite, bool quiet = false, bool calledFromRendererCallback = false) {
#if UNITY_EDITOR
			if (BuildUtilities.IsInSkeletonAssetBuildPreProcessing)
				return;
#endif
			if (!calledFromRendererCallback) {
				if (skeletonRenderer == null)
					skeletonRenderer = this.GetComponent<ISkeletonRenderer>();
				if (!skeletonRenderer.IsValid || !this.IsValid || overwrite)
					skeletonRenderer.Initialize(overwrite, quiet);
			}
			if (this.IsValid && !overwrite)
				return;
			InitializeAnimationComponent();

			if (_OnAnimationRebuild != null)
				_OnAnimationRebuild(this);
		}

		/// <summary>
		/// Manually initializes just this animation component without initializing the associated renderer component.
		/// The renderer component has to be initialized before calling this method, which happens automatically via
		/// renderer component Awake, or when needed earlier by calling <see cref="Initialize(bool, bool, bool)"/> on
		/// this animation component which initializes both components in the correct order.
		/// </summary>
		public virtual void InitializeAnimationComponent () {
			if (skeletonRenderer == null)
				skeletonRenderer = this.GetComponent<ISkeletonRenderer>();
#if UNITY_EDITOR
			if (!ApplicationIsPlaying && skeletonRenderer != null && skeletonRenderer.Skeleton != null)
				skeletonRenderer.Skeleton.SetupPose();
			requiresEditorUpdate = false;
#endif
		}

		/// <summary>
		/// Clears the previously generated mesh, resets the skeleton's pose, and clears all previously active animations.</summary>
		public void ClearState () {
			skeletonRenderer.ClearSkeletonState();
			ClearAnimationState();
		}

		public virtual void ClearAnimationState () { }

		public void OnEnable () {
			if (skeletonRenderer == null)
				skeletonRenderer = this.GetComponent<SkeletonRenderer>();

#if USE_THREADED_ANIMATION_UPDATE
			if (Application.isPlaying && UsesThreadedAnimation && !isUpdatedExternally) {
				SkeletonUpdateSystem system = SkeletonUpdateSystem.Instance;
				if (system)
					system.RegisterForUpdate(updateTiming, this);
			}
#endif
		}

#if USE_THREADED_ANIMATION_UPDATE
		public void OnDisable () {
			if (Application.isPlaying && UsesThreadedAnimation) {
				SkeletonUpdateSystem system = SkeletonUpdateSystem.Instance;
				if (system)
					system.UnregisterFromUpdate(updateTiming, this);
			}
		}
#endif

		protected void OnRendererRebuild (ISkeletonRenderer skeletonRenderer) {
			Initialize(overwrite: true, quiet: false, calledFromRendererCallback: true); //InitializeAnimationComponent();
		}

#if UNITY_EDITOR
		protected void OnValidate () {
			if (!Application.isPlaying) {
				Renderer.OnRebuild -= OnRendererRebuild;
				Renderer.OnRebuild += OnRendererRebuild;
				requiresEditorUpdate = true;
			} else if (Time.frameCount != 0) {
				// OnValidate is called once when starting play mode in the Editor, don't trigger re-init then.
				requiresEditorUpdate = true;
			}
		}
#endif

		protected virtual void Update () {
#if UNITY_EDITOR
			if (requiresEditorUpdate)
				InitializeAnimationComponent();
			if (!ApplicationIsPlaying) {
				if (!IsValid)
					Initialize(false);
				Update(0f);
				return;
			}
#endif
#if USE_THREADED_ANIMATION_UPDATE
			if (isUpdatedExternally) return;
#endif
			if (updateTiming != UpdateTiming.InUpdate) return;
			UpdateOncePerFrame(DeltaTime);
		}

		protected virtual void FixedUpdate () {
#if USE_THREADED_ANIMATION_UPDATE
			if (isUpdatedExternally) return;
#endif
			if (updateTiming != UpdateTiming.InFixedUpdate) return;
			UpdateOncePerFrame(DeltaTime);
		}

		protected virtual void LateUpdate () {
#if USE_THREADED_ANIMATION_UPDATE
			if (isUpdatedExternally) return;
#endif
			if (updateTiming != UpdateTiming.InLateUpdate) return;
			UpdateOncePerFrame(DeltaTime);
		}

		/// <summary>Calls <see cref="Update()"/> if it has not yet been called this frame.</summary>
		public virtual void UpdateOncePerFrame (float deltaTime) {
			if (frameOfLastUpdate != Time.frameCount) {
				MainThreadBeforeUpdateInternal();
				UpdateInternal(deltaTime, Time.frameCount, calledFromOnlyMainThread: true);
			}
		}

		/// <summary>
		/// Main thread update part preparing properties only accessible from main thread.
		/// To be followed by a potentially threaded call to <see cref="UpdateInternal"/>.
		/// </summary>
		public virtual void MainThreadBeforeUpdateInternal () {
			if (skeletonRenderer == null || !skeletonRenderer.IsValid || skeletonRenderer.Freeze || !this.IsValid
				|| skeletonRenderer.UpdateMode < UpdateMode.OnlyAnimationStatus) {
				skipUpdate = true;
				return;
			}
			skipUpdate = false;
			skeletonRenderer.GatherTransformMovementForPhysics();
			if (_BeforeUpdate != null)
				_BeforeUpdate(this);
		}

		public virtual void MainThreadAfterUpdateInternal () { }

#if USE_THREADED_ANIMATION_UPDATE
		public virtual void UpdateExternal (int currentFrameCount, bool calledFromOnlyMainThread = true) {
			UpdateInternal(UsedExternalDeltaTime, currentFrameCount, calledFromOnlyMainThread);
		}
#endif
		public virtual void UpdateInternal (float deltaTime, int currentFrameCount, bool calledFromOnlyMainThread = true) {
			if (skipUpdate)
				return;

			frameOfLastUpdate = currentFrameCount;
			UpdateAnimationStatus(deltaTime);
			skeletonRenderer.ApplyTransformMovementToPhysics();

			if (skeletonRenderer.UpdateMode == UpdateMode.OnlyAnimationStatus)
				return;

			ApplyAnimation(calledFromOnlyMainThread);
		}

#if USE_THREADED_ANIMATION_UPDATE
		/// <summary>Progresses the AnimationState according to the given deltaTime, and applies it to the Skeleton.
		/// Use Time.deltaTime to update manually. Use deltaTime 0 to update without progressing the time.</summary>
		public virtual CoroutineIterator UpdateInternalSplit (CoroutineIterator coroutineIterator,
			int currentFrameCount) {

			if (coroutineIterator.IsDone)
				return CoroutineIterator.Done;

			const int StateBits = 1;
			const uint StateMask = (1 << StateBits) - 1;
			switch (coroutineIterator.State(StateMask)) {
			case 0:
				if (skipUpdate)
					return CoroutineIterator.Done;
				frameOfLastUpdate = currentFrameCount;
				UpdateAnimationStatus(UsedExternalDeltaTime);
				skeletonRenderer.ApplyTransformMovementToPhysics();

				if (skeletonRenderer.UpdateMode == UpdateMode.OnlyAnimationStatus)
					return CoroutineIterator.Done;
				goto case 1;
			case 1:
				return ApplyAnimationSplit(coroutineIterator.ToNestedCall(StateBits))
					.FromNestedCall(1, StateBits);
			default:
				Debug.LogError(string.Format(
					"Internal coroutine logic error: SkeletonAnimationBase.UpdateInternalSplit state was {0}.",
					coroutineIterator.State(StateMask)), this);
				return CoroutineIterator.Done;
			}
		}
#endif

		/// <summary>Progresses the AnimationState according to the given deltaTime, and applies it to the Skeleton.
		/// Use Time.deltaTime to update manually. Use deltaTime 0 to update without progressing the time.</summary>
		public virtual void Update (float deltaTime) {
			MainThreadBeforeUpdateInternal();
			UpdateInternal(deltaTime, Time.frameCount, calledFromOnlyMainThread: true);
		}

		public virtual void ApplyAnimation (bool calledFromMainThread = true) {
			if (_BeforeApply != null)
				_BeforeApply(this);

			ApplyStateToSkeleton(calledFromMainThread);
			skeletonRenderer.AfterAnimationApplied(calledFromMainThread);
		}

		public virtual CoroutineIterator ApplyAnimationSplit (CoroutineIterator coroutineIterator) {
			if (coroutineIterator.IsDone)
				return CoroutineIterator.Done;

			const int StateBits = 1;
			const uint StateMask = (1 << StateBits) - 1;
			switch (coroutineIterator.State(StateMask)) {
			case 0:
				if (_BeforeApply != null)
					_BeforeApply(this);

				ApplyStateToSkeleton(calledFromMainThread: false);
				goto case 1;
			case 1:
				return skeletonRenderer.AfterAnimationAppliedSplit(coroutineIterator.ToNestedCall(StateBits))
					.FromNestedCall(1, StateBits);
			default:
				Debug.LogError(string.Format(
					"Internal coroutine logic error: SkeletonAnimationBase.ApplyAnimationSplit state was {0}.",
					coroutineIterator.State(StateMask)), this);
				return CoroutineIterator.Done;
			}
		}

		public void OnBecameVisibleFromMode (UpdateMode previousUpdateMode) {
			// OnBecameVisible is called after Update and LateUpdate(),
			// so update if previousUpdateMode didn't already update this frame.
			if (previousUpdateMode != UpdateMode.FullUpdate &&
				previousUpdateMode != UpdateMode.EverythingExceptMesh)
				Update(0);
		}

		protected virtual float DeltaTime { get { return Time.deltaTime; } }

		protected abstract void UpdateAnimationStatus (float deltaTime);
		protected abstract void ApplyStateToSkeleton (bool calledFromMainThread = true);
	}
}
