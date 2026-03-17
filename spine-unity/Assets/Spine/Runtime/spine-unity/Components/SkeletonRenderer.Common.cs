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

#if UNITY_2019_3_OR_NEWER
#define CONFIGURABLE_ENTER_PLAY_MODE
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

	// Partial class: covers common identical attributes, properties and methods shared across all
	// ISkeletonRenderer subclasses. This is a workaround for single inheritance limitations and covers code which
	// would otherwise be located in a base-class of SkeletonRenderer.
	public partial class SkeletonRenderer : MonoBehaviour, ISkeletonRenderer, IHasSkeletonRenderer {

		// Identical code shared by ISkeletonRenderer subclasses as a workaround for single inheritance limitations.
		#region Identical common ISkeletonRenderer code
		#region ISkeletonRenderer Attributes
		// Core Attributes
		public ISkeletonAnimation skeletonAnimation;
		public SkeletonDataAsset skeletonDataAsset;
		[System.NonSerialized] public Skeleton skeleton;
		[System.NonSerialized] public bool valid;

		protected bool wasMeshUpdatedAfterInit = false;
		protected bool updateTriangles = true;

		// Initialization Settings
		/// <summary>Skin name to use when the Skeleton is initialized.</summary>
		[SpineSkin(dataField: "skeletonDataAsset", defaultAsEmptyString: true)] public string initialSkinName;
		/// <summary>Flip X and Y to use when the Skeleton is initialized.</summary>
		public bool initialFlipX, initialFlipY;

		// Render Settings
		[SerializeField] protected MeshGenerator.Settings meshSettings = MeshGenerator.Settings.Default;
		[System.NonSerialized] protected readonly SkeletonRendererInstruction currentInstructions = new SkeletonRendererInstruction();

		/// <summary>Update mode to optionally limit updates to e.g. only apply animations but not update the mesh.</summary>
		protected UpdateMode updateMode = UpdateMode.FullUpdate;
		/// <summary>Update mode used when the MeshRenderer becomes invisible
		/// (when <c>OnBecameInvisible()</c> is called). Update mode is automatically
		/// reset to <c>UpdateMode.FullUpdate</c> when the mesh becomes visible again.</summary>
		public UpdateMode updateWhenInvisible = UpdateMode.FullUpdate;
		/// <summary>Clears the state of the render and skeleton when this component or its GameObject is disabled. This prevents previous state from being retained when it is enabled again. When pooling your skeleton, setting this to true can be helpful.</summary>
		public bool clearStateOnDisable = false;

		// Submesh Separation
		/// <summary>Slot names used to populate separatorSlots list when the Skeleton is initialized. Changing this after initialization does nothing.</summary>
		[SpineSlot] public string[] separatorSlotNames = new string[0];
		/// <summary>Slots that determine where the render is split. This is used by components such as SkeletonRenderSeparator so that the skeleton can be rendered by two separate renderers on different GameObjects.</summary>
		[System.NonSerialized] public readonly List<Slot> separatorSlots = new List<Slot>();
		public bool enableSeparatorSlots = false;

		// Overrides Attributes
		// These are API for anything that wants to take over rendering for a SkeletonRenderer.
		public bool disableRenderingOnOverride = true;
		event InstructionDelegate generateMeshOverride;
		[System.NonSerialized] readonly Dictionary<Slot, Material> customSlotMaterials = new Dictionary<Slot, Material>();
		[System.NonSerialized] protected bool materialsNeedUpdate = false;

		// Physics Attributes
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
		/// <summary>Position delta for threaded processing as Transform access from worker thread is not allowed.</summary>
		protected Vector3 positionDelta;
		/// <summary>Rotation delta for threaded processing as Transform access from worker thread is not allowed.</summary>
		protected float rotationDelta;

		// Threaded Update System Attributes
#if USE_THREADED_SKELETON_UPDATE
		protected static int mainThreadID = -1;

		[SerializeField] protected SettingsTriState threadedMeshGeneration = SettingsTriState.UseGlobalSetting;
		protected bool isUpdatedExternally = false;
		protected bool requiresMeshBufferAssignmentMainThread = false;
#if UNITY_EDITOR
		static bool applicationIsPlaying = false;
#endif
#endif // USE_THREADED_SKELETON_UPDATE

#if UNITY_EDITOR
		/// <summary>Enable this parameter when overwriting the Skeleton's skin from an editor script.
		/// Otherwise any changes will be overwritten by the next inspector update.</summary>
		protected bool editorSkipSkinSync = false;
		protected bool requiresEditorUpdate = false;
#endif
		#endregion ISkeletonRenderer Attributes

		#region ISkeletonRenderer Properties
		public ISkeletonRenderer Renderer { get { return this; } }
		public UnityEngine.MonoBehaviour Component { get { return this; } }
		public ISkeletonAnimation Animation { get { return skeletonAnimation; } set { skeletonAnimation = value; } }
		public SkeletonDataAsset SkeletonDataAsset {
			get { return skeletonDataAsset; }
			set { skeletonDataAsset = value; }
		}
		public Skeleton Skeleton {
			get {
				Initialize(false);
				return skeleton;
			}
			set {
				skeleton = value;
			}
		}
		public SkeletonData SkeletonData {
			get {
				Initialize(false);
				return skeleton == null ? null : skeleton.Data;
			}
		}
		public bool IsValid { get { return valid; } }

		public string InitialSkinName { get { return initialSkinName; } set { initialSkinName = value; } }
		/// <summary>Flip X to use when the Skeleton is initialized.</summary>
		public bool InitialFlipX { get { return initialFlipX; } set { initialFlipX = value; } }
		/// <summary>Flip Y to use when the Skeleton is initialized.</summary>
		public bool InitialFlipY { get { return initialFlipY; } set { initialFlipY = value; } }

		/// <summary>Update mode to optionally limit updates to e.g. only apply animations but not update the mesh.</summary>
		public UpdateMode UpdateMode { get { return updateMode; } set { updateMode = value; } }
		/// <summary>Update mode used when the MeshRenderer becomes invisible
		/// (when <c>OnBecameInvisible()</c> is called). Update mode is automatically
		/// reset to <c>UpdateMode.FullUpdate</c> when the mesh becomes visible again.</summary>
		public UpdateMode UpdateWhenInvisible { get { return updateWhenInvisible; } set { updateWhenInvisible = value; } }

		public MeshGenerator.Settings MeshSettings { get { return meshSettings; } set { meshSettings = value; } }

		// Submesh Separation
		/// <summary>Slots that determine where the render is split. This is used by components such as SkeletonRenderSeparator so that the skeleton can be rendered by two separate renderers on different GameObjects.</summary>
		public List<Slot> SeparatorSlots { get { return separatorSlots; } }
		public bool EnableSeparatorSlots { get { return enableSeparatorSlots; } set { enableSeparatorSlots = value; } }

		// Overrides Properties
		public bool HasGenerateMeshOverride { get { return generateMeshOverride != null; } }

		/// <summary>Allows separate code to take over rendering for this SkeletonRenderer component. The subscriber is passed a SkeletonRendererInstruction argument to determine how to render a skeleton.</summary>
		public event InstructionDelegate GenerateMeshOverride {
			add {
				generateMeshOverride += value;
				if (disableRenderingOnOverride && generateMeshOverride != null) {
					Initialize(false);
					DisableRenderers();
					updateMode = UpdateMode.FullUpdate;
				}
			}
			remove {
				generateMeshOverride -= value;
				if (disableRenderingOnOverride && generateMeshOverride == null) {
					Initialize(false);
					EnableRenderers();
				}
			}
		}

		/// <summary>Use this Dictionary to use a different Material to render specific Slots.</summary>
		public Dictionary<Slot, Material> CustomSlotMaterials {
			get { materialsNeedUpdate = true; return customSlotMaterials; }
		}

		// Physics Properties
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

		// Threaded Update System Properties
#if USE_THREADED_SKELETON_UPDATE
		public bool IsUpdatedExternally {
			get { return isUpdatedExternally; }
			set { isUpdatedExternally = value; }
		}

		protected bool UsesThreadedMeshGeneration {
			get { return threadedMeshGeneration == SettingsTriState.Enable || RuntimeSettings.UseThreadedMeshGeneration; }
		}

		public bool RequiresMeshBufferAssignmentMainThread {
			get { return requiresMeshBufferAssignmentMainThread; }
		}

		public SettingsTriState ThreadedMeshGeneration {
			get { return threadedMeshGeneration; }
			set {
				if (threadedMeshGeneration == value) return;
				threadedMeshGeneration = value;

				SkeletonUpdateSystem system = SkeletonUpdateSystem.Instance;
				if (system) {
					if (threadedMeshGeneration == SettingsTriState.Enable || RuntimeSettings.UseThreadedMeshGeneration)
						system.RegisterForUpdate(this);
					else
						system.UnregisterFromUpdate(this);
				}
			}
		}

#if UNITY_EDITOR
		// ApplicationIsPlaying for threaded access. Unfortunately Application.isPlaying throws
		// when called from worker thread.
		public static bool ApplicationIsPlaying {
			get { return applicationIsPlaying; }
			set { applicationIsPlaying = value; }
		}
#endif
#else // USE_THREADED_SKELETON_UPDATE
		public bool IsUpdatedExternally {
			get { return false; }
			set { }
		}
#if UNITY_EDITOR
		public static bool ApplicationIsPlaying {
			get { return Application.isPlaying; }
			set { }
		}
#endif
		public bool RequiresMeshBufferAssignmentMainThread { get { return true; } }
#endif // USE_THREADED_SKELETON_UPDATE

#if UNITY_EDITOR
		/// <summary>Enable this parameter when overwriting the Skeleton's skin from an editor script.
		/// Otherwise any changes will be overwritten by the next inspector update.</summary>
		public bool EditorSkipSkinSync {
			get { return editorSkipSkinSync; }
			set { editorSkipSkinSync = value; }
		}
#endif
		#endregion ISkeletonRenderer Properties

		#region ISkeletonRenderer Methods
		protected virtual void ClearCommon () {
			// Note: do not reset meshFilter.sharedMesh or meshRenderer.sharedMaterial to null,
			// otherwise constant reloading will be triggered at prefabs.
			currentInstructions.Clear();
			rendererBuffers.Clear();
			ClearMeshGenerator();
			skeleton = null;
			valid = false;

			if (skeletonAnimation != null)
				skeletonAnimation.ClearAnimationState();
		}

		protected virtual void InitializeCommon (bool overwrite, bool quiet = false) {
			if (valid && !overwrite)
				return;

			if (skeletonDataAsset == null || skeletonDataAsset.skeletonJSON == null) {
				Clear();
				return;
			} else if (skeleton != null && skeletonDataAsset.GetSkeletonData(true) != skeleton.Data) {
				Clear();
			} else if (this.Freeze) {
				return;
			} else {
				ClearCommon();
			}

			SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(quiet);
			if (skeletonData == null) return;

			rendererBuffers.Initialize();

			skeleton = new Skeleton(skeletonData) {
				ScaleX = initialFlipX ? -1 : 1,
				ScaleY = initialFlipY ? -1 : 1
			};
			valid = true;

			ResetLastPositionAndRotation();
			AssignInitialSkin();

			separatorSlots.Clear();
			for (int i = 0; i < separatorSlotNames.Length; i++)
				separatorSlots.Add(skeleton.FindSlot(separatorSlotNames[i]));

			AfterAnimationApplied();
			wasMeshUpdatedAfterInit = false;

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

		protected virtual void AssignInitialSkin () {
			if (string.IsNullOrEmpty(initialSkinName) || string.Equals(initialSkinName, "default", System.StringComparison.Ordinal))
				skeleton.SetSkin((Skin)null);
			else
				skeleton.SetSkin(initialSkinName);
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

		/// <summary>
		/// Gathers Transform movement for later application in <see cref="ApplyTransformMovementToPhysics"/>.
		/// Must be called in main thread.
		/// </summary>
		public virtual void GatherTransformMovementForPhysics () {
#if UNITY_EDITOR
			bool isPlaying = ApplicationIsPlaying;
#else
			bool isPlaying = true;
#endif
			if (isPlaying) {
				if (physicsPositionInheritanceFactor != Vector2.zero) {
					Vector3 position = GetPhysicsTransformPosition();
					positionDelta = (position - lastPosition) / this.MeshScale;

					positionDelta = transform.InverseTransformVector(positionDelta);
					if (physicsMovementRelativeTo != null) {
						positionDelta = physicsMovementRelativeTo.TransformVector(positionDelta);
					}
					positionDelta.x *= physicsPositionInheritanceFactor.x;
					positionDelta.y *= physicsPositionInheritanceFactor.y;

					lastPosition = position;
				}
				if (physicsRotationInheritanceFactor != 0f) {
					float rotation = GetPhysicsTransformRotation();
					rotationDelta = rotation - lastRotation;

					lastRotation = rotation;
				}
			}
		}

		/// <summary>
		/// Applies position and rotation Transform movement previously gathered via
		/// <see cref="GatherTransformMovementForPhysics"/>. May be called in worker thread.
		/// </summary>
		public virtual void ApplyTransformMovementToPhysics () {
			if (physicsPositionInheritanceFactor != Vector2.zero) {
				skeleton.PhysicsTranslate(positionDelta.x, positionDelta.y);
			}
			if (physicsRotationInheritanceFactor != 0f) {
				skeleton.PhysicsRotate(0, 0, physicsRotationInheritanceFactor * rotationDelta);
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

		public virtual void AfterAnimationApplied (bool calledFromMainThread = true) {
			if (_UpdateLocal != null)
				_UpdateLocal(this);

			if (_UpdateWorld == null) {
				UpdateWorldTransform(Physics.Update);
			} else {
				UpdateWorldTransform(Physics.Pose);
				_UpdateWorld(this);
				UpdateWorldTransform(Physics.Update);
			}

			if (calledFromMainThread && _UpdateComplete != null) {
				_UpdateComplete(this);
			}
		}

		public CoroutineIterator AfterAnimationAppliedSplit (CoroutineIterator coroutineIterator) {
			if (coroutineIterator.IsDone)
				return CoroutineIterator.Done;

			/*
			0:
			if (_UpdateLocal != null) {
				yield return true; // continue in main thread
			1:
				_UpdateLocal(this);
				yield return false; // continue in worker thread
			}

			2:
			if (_UpdateWorld == null) {
				UpdateWorldTransform(Physics.Update);
				// goto 5
			} else {
				UpdateWorldTransform(Physics.Pose);
				yield return true; // continue in main thread
			3:
				_UpdateWorld(this);
				yield return false; // continue in worker thread
			4:
				UpdateWorldTransform(Physics.Update);
				// goto 5
			}

			5:
			if (_UpdateComplete != null) {
				yield return true; // continue in main thread
			6:
				_UpdateComplete(this);
				// last call, no need to switch back to worker thread.
			}
			*/
			const int StateBits = 3;
			const uint StateMask = (1 << StateBits) - 1;
			switch (coroutineIterator.State(StateMask)) {
			case 0:
				if (_UpdateLocal != null) {
					AssertIsWorkerThread();
					return coroutineIterator.YieldReturnAtState(1, StateMask);
				} else {
					goto case 2;
				}
			case 1:
				AssertIsMainThread();
				_UpdateLocal(this);
				return coroutineIterator.YieldReturnAtState(2, StateMask);
			case 2:
				if (_UpdateWorld == null) {
					AssertIsWorkerThread();
					UpdateWorldTransform(Physics.Update);
					goto case 5;
				} else {
					AssertIsWorkerThread();
					UpdateWorldTransform(Physics.Pose);
					return coroutineIterator.YieldReturnAtState(3, StateMask);
				}
			case 3:
				AssertIsMainThread();
				_UpdateWorld(this);
				return coroutineIterator.YieldReturnAtState(4, StateMask);
			case 4:
				AssertIsWorkerThread();
				UpdateWorldTransform(Physics.Update);
				goto case 5;
			case 5:
				if (_UpdateComplete != null) {
					AssertIsWorkerThread();
					return coroutineIterator.YieldReturnAtState(6, StateMask);
				} else {
					return CoroutineIterator.Done;
				}
			case 6:
				AssertIsMainThread();
				_UpdateComplete(this);
				return CoroutineIterator.Done;
			default:
				Debug.LogError(string.Format(
					"Internal coroutine logic error: SkeletonRenderer.AfterAnimationAppliedSplit state was {0}.",
					coroutineIterator.State(StateMask)), this);
				return CoroutineIterator.Done;
			}
		}

		protected void IssueOnPostProcessVertices (MeshGeneratorBuffers buffers) {
			if (OnPostProcessVertices != null)
				OnPostProcessVertices.Invoke(buffers);
		}

		protected virtual void UpdateWorldTransform (Physics physics) {
			skeleton.UpdateWorldTransform(physics);
		}

		public virtual void UpdateMesh (bool calledFromMainThread = true) {
#if USE_THREADED_SKELETON_UPDATE
			bool canPrepareInstructions = calledFromMainThread || !NeedsMainThreadRendererPreparation;
#else
			bool canPrepareInstructions = true;
#endif
			if (canPrepareInstructions)
				PrepareInstructionsAndRenderers();

			updateTriangles = UpdateBuffersToInstructions(calledFromMainThread);
#if USE_THREADED_SKELETON_UPDATE
			if (calledFromMainThread) {
				requiresMeshBufferAssignmentMainThread = false;
				UpdateMeshAndMaterialsToBuffers();
			} else {
				requiresMeshBufferAssignmentMainThread = true;
			}
#else
			UpdateMeshAndMaterialsToBuffers();
#endif
		}

		/// <returns>True if triangles (indices array) need to be updated.</returns>
		public virtual bool UpdateBuffersToInstructions (bool calledFromMainThread = true) {
			if (!valid || currentInstructions.rawVertexCount < 0) return false;
			wasMeshUpdatedAfterInit = true;

			if (this.generateMeshOverride != null) {
				if (calledFromMainThread)
					this.generateMeshOverride(currentInstructions);
				if (disableRenderingOnOverride) return false;
			}

			ExposedList<SubmeshInstruction> workingSubmeshInstructions = currentInstructions.submeshInstructions;
			MeshRendererBuffers.SmartMesh currentSmartMesh = rendererBuffers.GetNextMesh(); // Double-buffer for performance.

			// Update vertex buffers based on vertices from the attachments and assign buffers to a target UnityEngine.Mesh.
			bool updateTriangles = SkeletonRendererInstruction.GeometryNotEqual(currentInstructions, currentSmartMesh.instructionUsed, calledFromMainThread);
			FillBuffersFromSubmeshInstructions(workingSubmeshInstructions, currentSmartMesh, updateTriangles);
			return updateTriangles;
		}

		public virtual void UpdateMeshAndMaterialsToBuffers () {
			ExposedList<SubmeshInstruction> workingSubmeshInstructions = currentInstructions.submeshInstructions;
			MeshRendererBuffers.SmartMesh currentSmartMesh = rendererBuffers.GetCurrentMesh();
			UpdateMeshAndMaterialsToBuffers(workingSubmeshInstructions, currentSmartMesh, updateTriangles);
		}

		protected virtual void UpdateMeshAndMaterialsToBuffers (
			ExposedList<SubmeshInstruction> workingSubmeshInstructions, MeshRendererBuffers.SmartMesh currentSmartMesh,
			bool updateTriangles) {

			FillMeshFromBuffers(currentSmartMesh, updateTriangles);

			bool materialsChanged;
			rendererBuffers.GatherMaterialsFromInstructions(workingSubmeshInstructions, out materialsChanged);
			if (materialsChanged || materialsNeedUpdate) {
				UpdateUsedMaterialsForRenderers(workingSubmeshInstructions);
			}

#if SPINE_OPTIONAL_ON_DEMAND_LOADING
			if (Application.isPlaying)
				HandleOnDemandLoading();
#endif

			// The UnityEngine.Mesh is ready. Set it as the MeshFilter's mesh. Store the instructions used for that mesh.
			AssignMeshAtRenderer(workingSubmeshInstructions, currentSmartMesh);

			if (OnMeshAndMaterialsUpdated != null)
				OnMeshAndMaterialsUpdated(this);
		}

		public virtual void UpdateMaterials () {
			UpdateUsedMaterialsForRenderers(currentInstructions.submeshInstructions);
		}

		// Threading Asserts
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		protected void InitializeMainThreadID () {
#if USE_THREADED_SKELETON_UPDATE
			if (mainThreadID == -1)
				mainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void AssertIsMainThread () {
#if USE_THREADED_SKELETON_UPDATE
			if (System.Threading.Thread.CurrentThread.ManagedThreadId != mainThreadID)
				Debug.LogError("AssertIsMainThread failed: worker thread calling main thread code. Thread ID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
#endif
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void AssertIsWorkerThread () {
#if USE_THREADED_SKELETON_UPDATE
			if (System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadID)
				Debug.LogError("AssertIsWorkerThread failed: main thread calling worker thread code! Thread ID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
#endif
		}
		#endregion ISkeletonRenderer Methods

		#region ISkeletonRenderer Events
		protected event SkeletonRendererDelegate _UpdateLocal;
		protected event SkeletonRendererDelegate _UpdateWorld;
		protected event SkeletonRendererDelegate _UpdateComplete;

		/// <summary>
		/// Occurs after the animations are applied and before world space values are resolved.
		/// Use this callback when you want to set bone local values.
		/// </summary>
		public event SkeletonRendererDelegate UpdateLocal { add { _UpdateLocal += value; } remove { _UpdateLocal -= value; } }

		/// <summary>
		/// Occurs after the Skeleton's bone world space values are resolved (including all constraints).
		/// Using this callback will cause the world space values to be solved an extra time.
		/// Use this callback if want to use bone world space values, and also set bone local values.</summary>
		public event SkeletonRendererDelegate UpdateWorld { add { _UpdateWorld += value; } remove { _UpdateWorld -= value; } }

		/// <summary>
		/// Occurs after the Skeleton's bone world space values are resolved (including all constraints).
		/// Use this callback if you want to use bone world space values, but don't intend to modify bone local values.
		/// This callback can also be used when setting world position and the bone matrix.</summary>
		public event SkeletonRendererDelegate UpdateComplete { add { _UpdateComplete += value; } remove { _UpdateComplete -= value; } }

		/// <summary> Occurs after the vertex data is populated every frame, before the vertices are pushed into the mesh.</summary>
		public event Spine.Unity.MeshGeneratorDelegate OnPostProcessVertices;

		/// <summary>OnRebuild is raised after the Skeleton is successfully initialized.</summary>
		public event SkeletonRendererDelegate OnRebuild;

		/// <summary>OnInstructionsPrepared is raised at the end of <c>LateUpdate</c> after render instructions
		/// are done, target renderers are prepared, and the mesh is ready to be generated.</summary>
		public event InstructionDelegate OnInstructionsPrepared;
		#endregion ISkeletonRenderer Events
		#endregion Identical common ISkeletonRenderer code
		// End of identical code shared by ISkeletonRenderer subclasses as a workaround for single inheritance limitations.
	}
}
