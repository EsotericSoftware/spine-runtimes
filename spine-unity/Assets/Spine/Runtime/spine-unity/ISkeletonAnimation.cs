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

#if !SPINE_AUTO_UPGRADE_COMPONENTS_OFF
#define AUTO_UPGRADE_TO_43_COMPONENTS
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
	public enum UpdateMode {
		Nothing = 0,
		OnlyAnimationStatus,
		OnlyEventTimelines = 4, // added as index 4 to keep scene behavior unchanged.
		EverythingExceptMesh = 2,
		FullUpdate,
		//Reserved 4 for OnlyEventTimelines
	};

	public enum UpdateTiming {
		ManualUpdate = 0,
		InUpdate,
		InFixedUpdate,
		InLateUpdate
	}

	public delegate void SkeletonAnimationDelegate (ISkeletonAnimation animated);
	public delegate void SkeletonRendererDelegate (ISkeletonRenderer skeletonRenderer);
	public delegate void InstructionDelegate (SkeletonRendererInstruction instruction);

	public interface ISpineComponent { }
	public static class ISpineComponentExtensions {
		public static bool IsNullOrDestroyed (this ISpineComponent component) {
			if (component == null) return true;
			return (UnityEngine.Object)component == null;
		}
	}

	/// <summary>A Spine-Unity Component that animates a Skeleton but not necessarily with a Spine.AnimationState.</summary>
	public interface ISkeletonAnimation : IHasSkeletonRenderer, ISpineComponent {
		event SkeletonAnimationDelegate OnAnimationRebuild;
		event SkeletonAnimationDelegate BeforeUpdate;
		event SkeletonAnimationDelegate BeforeApply;
		event SkeletonRendererDelegate UpdateLocal;
		event SkeletonRendererDelegate UpdateWorld;
		event SkeletonRendererDelegate UpdateComplete;
		MonoBehaviour Component { get; }
		Skeleton Skeleton { get; }

		bool IsValid { get; }
		UpdateMode UpdateMode { get; set; }
		UpdateTiming UpdateTiming { get; set; }

		void EnsureRendererEventsSubscribed ();
		void Initialize (bool overwrite, bool quiet = false, bool calledFromRendererCallback = false);
		void InitializeAnimationComponent ();
		void ClearAnimationState ();
		void ApplyAnimation (bool calledFromMainThread = true);
		CoroutineIterator ApplyAnimationSplit (CoroutineIterator coroutineIterator);
		void UpdateOncePerFrame (float deltaTime);
		void Update (float deltaTime);
		void OnBecameVisibleFromMode (UpdateMode previousUpdateMode);
	}

	/// <summary>Holds a reference to a SkeletonDataAsset.</summary>
	public interface IHasSkeletonDataAsset : ISpineComponent {
		/// <summary>Gets the SkeletonDataAsset of the Spine Component.</summary>
		SkeletonDataAsset SkeletonDataAsset { get; }
	}

	public interface IHasModifyableSkeletonDataAsset : IHasSkeletonDataAsset, ISpineComponent {
		/// <summary>Set the SkeletonDataAsset of the Spine Component.
		/// Initialize(overwrite:true) has to be called at the target component
		/// afterwards.</summary>
		new SkeletonDataAsset SkeletonDataAsset { get; set; }
	}

	/// <summary>A Spine-Unity Component that manages a Spine.Skeleton instance, instantiated from a SkeletonDataAsset.</summary>
	public interface ISkeletonComponent : IHasModifyableSkeletonDataAsset, ISpineComponent {
		/// <summary>Accesses the Spine.Skeleton instance of the Spine Component. This is equivalent to SkeletonRenderer's .skeleton.</summary>
		Skeleton Skeleton { get; set; }

		MonoBehaviour Component { get; }
	}

	/// <summary>A Spine-Unity Component that uses a Spine.AnimationState to animate its skeleton.</summary>
	public interface IAnimationStateComponent : ISpineComponent {
		/// <summary>Gets the Spine.AnimationState of the animated Spine Component. This is equivalent to SkeletonAnimation.state.</summary>
		AnimationState AnimationState { get; }

		/// <summary>If enabled, AnimationState time is advanced by Unscaled Game Time
		/// (<c>Time.unscaledDeltaTime</c> instead of the default Game Time(<c>Time.deltaTime</c>).
		/// to animate independent of game <c>Time.timeScale</c>.
		/// Instance SkeletonGraphic.timeScale and SkeletonAnimation.timeScale will still be applied.</summary>
		bool UnscaledTime { get; set; }
	}

	/// <summary>A Spine-Unity Component that holds a reference to a skeleton renderer.</summary>
	public interface IHasSkeletonRenderer : ISpineComponent {
		ISkeletonRenderer Renderer { get; }
	}

	/// <summary>A Spine-Unity Component that holds a reference to an ISkeletonComponent.</summary>
	public interface IHasSkeletonComponent : ISpineComponent {
		ISkeletonComponent SkeletonComponent { get; }
	}

	public interface ISkeletonRendererEvents {
		event SkeletonRendererDelegate UpdateLocal;
		event SkeletonRendererDelegate UpdateWorld;
		event SkeletonRendererDelegate UpdateComplete;

		/// <summary>OnRebuild is raised after the Skeleton is successfully initialized.</summary>
		event SkeletonRendererDelegate OnRebuild;

		/// <summary>OnMeshAndMaterialsUpdated is called at the end of LateUpdate after the Mesh and
		/// all materials have been updated.</summary>
		event SkeletonRendererDelegate OnMeshAndMaterialsUpdated;
	}

	public interface IThreadedRenderer {
		bool IsUpdatedExternally { get; set; }
		bool RequiresMeshBufferAssignmentMainThread { get; }
		void UpdateMeshAndMaterialsToBuffers ();
		void MainThreadPrepareLateUpdateInternal();
		void LateUpdateImplementation (bool calledFromMainThread);
	}

	// combined interfaces
	public interface ISkeletonRenderer : ISkeletonComponent, ISkeletonRendererEvents,
		IHasModifyableSkeletonDataAsset, IThreadedRenderer {

		ISkeletonAnimation Animation { get; set; }
		Dictionary<Slot, Material> CustomSlotMaterials { get; }
		bool EnableSeparatorSlots { get; set; }
		List<Slot> SeparatorSlots { get; }
		bool Freeze { get; }
		bool HasGenerateMeshOverride { get; }
		string InitialSkinName { get; set; }
		bool IsValid { get; }
		MeshGenerator.Settings MeshSettings { get; set; }
		UpdateMode UpdateMode { get; set; }
		UpdateMode UpdateWhenInvisible { get; set; }
		float MeshScale { get; }
		Vector2 PhysicsPositionInheritanceFactor { get; set; }
		float PhysicsRotationInheritanceFactor { get; set; }

#if UNITY_EDITOR
		bool EditorSkipSkinSync { get; set; }
#endif

		event Spine.Unity.InstructionDelegate GenerateMeshOverride;
		event Spine.Unity.MeshGeneratorDelegate OnPostProcessVertices;

		void ClearSkeletonState ();
		void ClearState ();
		void Initialize (bool overwrite, bool quiet = false);
		void GatherTransformMovementForPhysics ();
		void ApplyTransformMovementToPhysics();
		void AfterAnimationApplied (bool calledFromMainThread = true);
		CoroutineIterator AfterAnimationAppliedSplit (CoroutineIterator coroutineIterator);
		void LateUpdate ();
		void UpdateMesh (bool calledFromMainThread = true);
		void UpdateMaterials ();
		void FindAndApplySeparatorSlots (Func<string, bool> slotNamePredicate, bool clearExistingSeparators = true, bool updateStringArray = false);
		void FindAndApplySeparatorSlots (string startsWith, bool clearExistingSeparators = true, bool updateStringArray = false);
		void ReapplySeparatorSlotNames ();
	}

	public interface IUpgradable {
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		void UpgradeTo43 ();
#endif
	}
}
