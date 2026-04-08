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

#if UNITY_2017_1_OR_NEWER
#define BUILT_IN_SPRITE_MASK_COMPONENT
#endif

#if !SPINE_DISABLE_THREADING
#define USE_THREADED_ANIMATION_UPDATE
#endif

#if !SPINE_AUTO_UPGRADE_COMPONENTS_OFF
#define AUTO_UPGRADE_TO_43_COMPONENTS
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Spine.Unity {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[AddComponentMenu("Spine/SkeletonAnimation")]
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonAnimation-Component")]
	public class SkeletonAnimation : SkeletonAnimationBase, IAnimationStateComponent, IUpgradable {

		#region Serialized state and Beginner API
		[FormerlySerializedAs("_animationName")] [SerializeField] [SpineAnimation] protected string animationName = "";

		/// <summary>Whether or not <see cref="AnimationName"/> should loop. This only applies to the initial animation specified in the inspector, or any subsequent Animations played through .AnimationName. Animations set through state.SetAnimation are unaffected.</summary>
		public bool loop;

		/// <summary>
		/// The rate at which animations progress over time. 1 means 100%. 0.5 means 50%.</summary>
		/// <remarks>AnimationState and TrackEntry also have their own timeScale. These are combined multiplicatively.</remarks>
		public float timeScale = 1;

		/// <summary>If enabled, AnimationState time is advanced by Unscaled Game Time
		/// (<c>Time.unscaledDeltaTime</c> instead of the default Game Time(<c>Time.deltaTime</c>).
		/// to animate independent of game <c>Time.timeScale</c>.
		/// Instance timeScale will still be applied.</summary>
		public bool unscaledTime;
		#endregion

		#region Animation State Callbacks on Main Thread
#if USE_THREADED_ANIMATION_UPDATE
		public event AnimationState.TrackEntryDelegate mainThreadStart, mainThreadInterrupt, mainThreadEnd, mainThreadDispose, mainThreadComplete;
		public event AnimationState.TrackEntryEventDelegate mainThreadEvent;

		protected struct EventQueueEntry {
			public EventType type;
			public TrackEntry entry;
			public Event e;

			public EventQueueEntry (EventType eventType, TrackEntry trackEntry, Event e = null) {
				this.type = eventType;
				this.entry = trackEntry;
				this.e = e;
			}
		}
		protected enum EventType {
			Start, Interrupt, End, Dispose, Complete, Event
		}
		protected List<EventQueueEntry> mainThreadEventQueue = null;

		protected void DrainThreadedEventQueue () {
			for (int i = 0; i < mainThreadEventQueue.Count; i++) {
				EventQueueEntry queueEntry = mainThreadEventQueue[i];
				TrackEntry trackEntry = queueEntry.entry;

				switch (queueEntry.type) {
				case EventType.Start:
					if (mainThreadStart != null) mainThreadStart(trackEntry);
					break;
				case EventType.Interrupt:
					if (mainThreadInterrupt != null) mainThreadInterrupt(trackEntry);
					break;
				case EventType.End:
					if (mainThreadEnd != null) mainThreadEnd(trackEntry);
					break;
				case EventType.Dispose:
					if (mainThreadDispose != null) mainThreadDispose(trackEntry);
					break;
				case EventType.Complete:
					if (mainThreadComplete != null) mainThreadComplete(trackEntry);
					break;
				case EventType.Event:
					if (mainThreadEvent != null) mainThreadEvent(trackEntry, queueEntry.e);
					break;
				}
			}
			mainThreadEventQueue.Clear();
		}

		private void ThreadedStart (TrackEntry entry) {
			if (!UsesThreadedAnimation) {
				if (mainThreadStart != null) mainThreadStart(entry);
				return;
			}
			mainThreadEventQueue.Add(new EventQueueEntry(EventType.Start, entry));
		}
		private void ThreadedInterrupt (TrackEntry entry) {
			if (!UsesThreadedAnimation) {
				if (mainThreadInterrupt != null) mainThreadInterrupt(entry);
				return;
			}
			mainThreadEventQueue.Add(new EventQueueEntry(EventType.Interrupt, entry));
		}
		private void ThreadedEnd (TrackEntry entry) {
			if (!UsesThreadedAnimation) {
				if (mainThreadEnd != null) mainThreadEnd(entry);
				return;
			}
			mainThreadEventQueue.Add(new EventQueueEntry(EventType.End, entry));
		}
		private void ThreadedDispose (TrackEntry entry) {
			if (!UsesThreadedAnimation) {
				if (mainThreadDispose != null) mainThreadDispose(entry);
				return;
			}
			mainThreadEventQueue.Add(new EventQueueEntry(EventType.Dispose, entry));
		}
		private void ThreadedComplete (TrackEntry entry) {
			if (!UsesThreadedAnimation) {
				if (mainThreadComplete != null) mainThreadComplete(entry);
				return;
			}
			mainThreadEventQueue.Add(new EventQueueEntry(EventType.Complete, entry));
		}
		private void ThreadedEvent (TrackEntry entry, Event e) {
			if (!UsesThreadedAnimation) {
				if (mainThreadEvent != null) mainThreadEvent(entry, e);
				return;
			}
			mainThreadEventQueue.Add(new EventQueueEntry(EventType.Event, entry, e));
		}

		public event AnimationState.TrackEntryDelegate MainThreadStart {
			add {
				if (mainThreadEventQueue == null) mainThreadEventQueue = new List<EventQueueEntry>();
				mainThreadStart += value;
				this.AnimationState.Start -= ThreadedStart;
				this.AnimationState.Start += ThreadedStart;
			}
			remove {
				mainThreadStart -= value;
				this.AnimationState.Start -= ThreadedStart;
			}
		}
		public event AnimationState.TrackEntryDelegate MainThreadInterrupt {
			add {
				if (mainThreadEventQueue == null) mainThreadEventQueue = new List<EventQueueEntry>();
				mainThreadInterrupt += value;
				this.AnimationState.Interrupt -= ThreadedInterrupt;
				this.AnimationState.Interrupt += ThreadedInterrupt;
			}
			remove {
				mainThreadInterrupt -= value;
				this.AnimationState.Interrupt -= ThreadedInterrupt;
			}
		}
		public event AnimationState.TrackEntryDelegate MainThreadEnd {
			add {
				if (mainThreadEventQueue == null) mainThreadEventQueue = new List<EventQueueEntry>();
				mainThreadEnd += value;
				this.AnimationState.End -= ThreadedEnd;
				this.AnimationState.End += ThreadedEnd;
			}
			remove {
				mainThreadEnd -= value;
				this.AnimationState.End -= ThreadedEnd;
			}
		}
		public event AnimationState.TrackEntryDelegate MainThreadDispose {
			add {
				if (mainThreadEventQueue == null) mainThreadEventQueue = new List<EventQueueEntry>();
				mainThreadDispose += value;
				this.AnimationState.Dispose -= ThreadedDispose;
				this.AnimationState.Dispose += ThreadedDispose;
			}
			remove {
				mainThreadDispose -= value;
				this.AnimationState.Dispose -= ThreadedDispose;
			}
		}
		public event AnimationState.TrackEntryDelegate MainThreadComplete {
			add {
				if (mainThreadEventQueue == null) mainThreadEventQueue = new List<EventQueueEntry>();
				mainThreadComplete += value;
				this.AnimationState.Complete -= ThreadedComplete;
				this.AnimationState.Complete += ThreadedComplete;
			}
			remove {
				mainThreadComplete -= value;
				this.AnimationState.Complete -= ThreadedComplete;
			}
		}
		public event AnimationState.TrackEntryEventDelegate MainThreadEvent {
			add {
				if (mainThreadEventQueue == null) mainThreadEventQueue = new List<EventQueueEntry>();
				mainThreadEvent += value;
				this.AnimationState.Event -= ThreadedEvent;
				this.AnimationState.Event += ThreadedEvent;
			}
			remove {
				mainThreadEvent -= value;
				this.AnimationState.Event -= ThreadedEvent;
			}
		}

		public override void MainThreadAfterUpdateInternal () {
			if (mainThreadEventQueue != null)
				DrainThreadedEventQueue();
			base.MainThreadAfterUpdateInternal();
		}
#else // USE_THREADED_ANIMATION_UPDATE
		public event AnimationState.TrackEntryDelegate MainThreadStart {
			add { this.AnimationState.Start += value; }
			remove { this.AnimationState.Start -= value; }
		}
		public event AnimationState.TrackEntryDelegate MainThreadInterrupt {
			add { this.AnimationState.Interrupt += value; }
			remove { this.AnimationState.Interrupt -= value; }
		}
		public event AnimationState.TrackEntryDelegate MainThreadEnd {
			add { this.AnimationState.End += value; }
			remove { this.AnimationState.End -= value; }
		}
		public event AnimationState.TrackEntryDelegate MainThreadDispose {
			add { this.AnimationState.Dispose += value; }
			remove { this.AnimationState.Dispose -= value; }
		}
		public event AnimationState.TrackEntryDelegate MainThreadComplete {
			add { this.AnimationState.Complete += value; }
			remove { this.AnimationState.Complete -= value; }
		}
		public event AnimationState.TrackEntryEventDelegate MainThreadEvent {
			add { this.AnimationState.Event += value; }
			remove { this.AnimationState.Event -= value; }
		}
#endif // USE_THREADED_ANIMATION_UPDATE
		#endregion

		protected Spine.AnimationState state;

		/// <summary>
		/// This is the Spine.AnimationState object of this SkeletonAnimation. You can control animations through it.
		/// Note that this object, like .skeleton, is not guaranteed to exist in Awake. Do all accesses and caching to it in Start</summary>
		public Spine.AnimationState AnimationState {
			get {
				Initialize(false);
				return state;
			}
			set { state = value; }
		}

		public override bool IsValid {
			get { return skeletonRenderer != null && skeletonRenderer.IsValid && state != null; }
		}

		public bool UnscaledTime { get { return unscaledTime; } set { unscaledTime = value; } }

		#region Serialized state and Beginner API
		/// <summary>
		/// Setting this property sets the animation of the skeleton. If invalid, it will store the animation name for the next time the skeleton is properly initialized.
		/// Getting this property gets the name of the currently playing animation. If invalid, it will return the last stored animation name set through this property.</summary>
		public string AnimationName {
			get {
				if (!this.IsValid) {
					return animationName;
				} else {
					TrackEntry entry = state.GetCurrent(0);
					return entry == null ? null : entry.Animation.Name;
				}
			}
			set {
				Initialize(false);
				if (!IsValid) {
					animationName = value;
					return;
				}

				if (animationName == value) {
					TrackEntry entry = state.GetCurrent(0);
					if (entry != null && entry.Loop == loop)
						return;
				}
				animationName = value;

				if (string.IsNullOrEmpty(value)) {
					state.ClearTrack(0);
				} else {
					SkeletonData skeletonData = skeletonRenderer.SkeletonDataAsset.GetSkeletonData(false);
					if (skeletonData == null)
						return;
					Spine.Animation animationObject = skeletonData.FindAnimation(value);
					if (animationObject != null)
						state.SetAnimation(0, animationObject, loop);
				}
			}
		}
		#endregion

		/// <summary>
		/// Clears the previously generated mesh, resets the skeleton's pose, and clears all previously active animations.</summary>
		public override void ClearAnimationState () {
			if (state != null) state.ClearTracks();
		}

		public override void InitializeAnimationComponent () {
			base.InitializeAnimationComponent();
			if (!skeletonRenderer.IsValid)
				return;

			AnimationStateData data = skeletonRenderer.SkeletonDataAsset.GetAnimationStateData();

#if UNITY_EDITOR
			AnimationState oldAnimationState = state;
#endif
			state = new Spine.AnimationState(data);
			state.Dispose += OnAnimationDisposed;
			if (state == null)
				return;
#if UNITY_EDITOR
			if (oldAnimationState != null)
				state.AssignEventSubscribersFrom(oldAnimationState);
#endif
			UpdateInitialAnimation();
		}

		protected virtual void OnAnimationDisposed (TrackEntry entry) {
			// when updateMode disables applying animations, still ensure animations are mixed out
			UpdateMode updateMode = skeletonRenderer.UpdateMode;
			if (updateMode != UpdateMode.FullUpdate &&
				updateMode != UpdateMode.EverythingExceptMesh) {
				entry.Animation.Apply(skeleton, 0, 0, false, null, 0f, true, false, true, false);
			}
		}

		public virtual void UpdateInitialAnimation () {
			state.ClearTrack(0);
			if (!string.IsNullOrEmpty(animationName)) {
				SkeletonData skeletonData = skeletonRenderer.SkeletonDataAsset.GetSkeletonData(false);
				if (skeletonData == null)
					return;
				Spine.Animation animation = skeletonData.FindAnimation(animationName);
				if (animation != null) {
					state.SetAnimation(0, animation, loop);
#if UNITY_EDITOR
					if (!ApplicationIsPlaying)
						Update(0f);
#endif
				}
			}
		}

#if USE_THREADED_ANIMATION_UPDATE
		public override float UsedExternalDeltaTime {
			get {
				return unscaledTime ? ExternalUnscaledDeltaTime : ExternalDeltaTime;
			}
		}
#endif
		protected override float DeltaTime {
			get {
				return unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
			}
		}

		protected override void UpdateAnimationStatus (float deltaTime) {
			deltaTime *= timeScale;
			if (state != null) {
				state.Update(deltaTime);
				skeleton.Update(deltaTime);
#if UNITY_EDITOR
				if (ApplicationIsPlaying)
					UpdatePropertyToCurrentAnimationEditor();
#endif
				if (skeletonRenderer.UpdateMode == UpdateMode.OnlyAnimationStatus) {
					state.ApplyEventTimelinesOnly(skeleton, issueEvents: false);
				}
			}
		}

		protected override void ApplyStateToSkeleton (bool calledFromMainThread) {
			if (skeletonRenderer.UpdateMode != UpdateMode.OnlyEventTimelines)
				state.Apply(skeletonRenderer.Skeleton);
			else
				state.ApplyEventTimelinesOnly(skeletonRenderer.Skeleton, issueEvents: true);
		}

#if UNITY_EDITOR
		protected void UpdatePropertyToCurrentAnimationEditor () {
			if (state.Tracks.Count == 0 || state.Tracks.Items[0] == null)
				return;
			Animation currentAnimation = state.Tracks.Items[0].Animation;
			animationName = currentAnimation == null ? "<None>" : currentAnimation.Name;
		}
#endif



		#region Runtime Instantiation
		/// <summary>Adds and prepares SkeletonAnimation and SkeletonRenderer components to a GameObject at runtime.</summary>
		/// <returns>A struct referencing the newly instantiated SkeletonAnimation and SkeletonRenderer components.</returns>
		public static SkeletonComponents<SkeletonRenderer, SkeletonAnimation> AddToGameObject (
			GameObject gameObject, SkeletonDataAsset skeletonDataAsset, bool quiet = false) {

			return Spine.Unity.SkeletonRenderer.AddSpineComponents<SkeletonRenderer, SkeletonAnimation>(
				gameObject, skeletonDataAsset, quiet);
		}

		/// <summary>Instantiates a new UnityEngine.GameObject and adds SkeletonAnimation and SkeletonRenderer components to it.</summary>
		/// <returns>A struct referencing the newly instantiated SkeletonAnimation and SkeletonRenderer components.</returns>
		public static SkeletonComponents<SkeletonRenderer, SkeletonAnimation> NewSkeletonAnimationGameObject (SkeletonDataAsset skeletonDataAsset,
			bool quiet = false) {
			return Spine.Unity.SkeletonRenderer.NewSpineGameObject<SkeletonRenderer, SkeletonAnimation>(
				skeletonDataAsset, quiet);
		}
		#endregion

		#region Transfer of Deprecated Fields
#if UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		// compatibility layer between 4.1 and 4.2, automatically transfer serialized attributes.
		public override void UpgradeTo43 () {
			if (!Application.isPlaying && !wasDeprecatedTransferred) {
				UpgradeTo43Components();
				TransferDeprecatedFields();
				InitializeAnimationComponent();
			}
		}

		protected void UpgradeTo43Components () {
			if (gameObject.GetComponent<SkeletonRenderer>() == null &&
				gameObject.GetComponent<SkeletonGraphic>() == null) {
				gameObject.AddComponent<SkeletonRenderer>();
				EditorBridge.RequestMarkDirty(gameObject);
				Debug.Log(string.Format("{0}: Auto-migrated old SkeletonAnimation component to split SkeletonAnimation + SkeletonRenderer components.",
					gameObject.name), gameObject);
			}
		}

		/// <summary>Transfer of former base class SkeletonRenderer parameters.</summary>
		protected void TransferDeprecatedFields () {
			wasDeprecatedTransferred = true;

			SkeletonRenderer skeletonRenderer = gameObject.GetComponent<SkeletonRenderer>();
			if (skeletonRenderer == null)
				return;

			skeletonRenderer.skeletonDataAsset = this.skeletonDataAssetDeprecated;
			skeletonRenderer.initialSkinName = this.initialSkinNameDeprecated;
			skeletonRenderer.EditorSkipSkinSync = this.editorSkipSkinSyncDeprecated;
			skeletonRenderer.initialFlipX = this.initialFlipXDeprecated;
			skeletonRenderer.initialFlipY = this.initialFlipYDeprecated;
			skeletonRenderer.UpdateMode = this.updateModeDeprecated;
			skeletonRenderer.updateWhenInvisible = this.updateWhenInvisibleDeprecated;
			skeletonRenderer.separatorSlotNames = this.separatorSlotNamesDeprecated;

			skeletonRenderer.MeshSettings.zSpacing = this.zSpacingDeprecated;
			skeletonRenderer.MeshSettings.useClipping = this.useClippingDeprecated;
			skeletonRenderer.MeshSettings.immutableTriangles = this.immutableTrianglesDeprecated;
			skeletonRenderer.MeshSettings.pmaVertexColors = this.pmaVertexColorsDeprecated;
			skeletonRenderer.MeshSettings.tintBlack = this.tintBlackDeprecated;
			skeletonRenderer.MeshSettings.addNormals = this.addNormalsDeprecated;
			skeletonRenderer.MeshSettings.calculateTangents = this.calculateTangentsDeprecated;

			skeletonRenderer.clearStateOnDisable = this.clearStateOnDisableDeprecated;
			skeletonRenderer.singleSubmesh = this.singleSubmeshDeprecated;
			skeletonRenderer.MaskInteraction = this.maskInteractionDeprecated;
		}

		[SerializeField] protected bool wasDeprecatedTransferred = false;
		// SkeletonRenderer former base class parameters
		[FormerlySerializedAs("skeletonDataAsset")] [SerializeField] private SkeletonDataAsset skeletonDataAssetDeprecated;

		[FormerlySerializedAs("initialSkinName")] [SpineSkin(defaultAsEmptyString: true)] [SerializeField] private string initialSkinNameDeprecated;
		[FormerlySerializedAs("editorSkipSkinSync")] [SerializeField] private bool editorSkipSkinSyncDeprecated = false;
		[FormerlySerializedAs("initialFlipX")] [SerializeField] private bool initialFlipXDeprecated = false;
		[FormerlySerializedAs("initialFlipY")] [SerializeField] private bool initialFlipYDeprecated = false;
		[FormerlySerializedAs("updateMode")] [SerializeField] private UpdateMode updateModeDeprecated = UpdateMode.FullUpdate;
		[FormerlySerializedAs("updateWhenInvisible")] [SerializeField] private UpdateMode updateWhenInvisibleDeprecated = UpdateMode.FullUpdate;
		[UnityEngine.Serialization.FormerlySerializedAs("submeshSeparators"),
			UnityEngine.Serialization.FormerlySerializedAs("separatorSlotNames")]
		[SerializeField] private string[] separatorSlotNamesDeprecated = new string[0];

		[FormerlySerializedAs("zSpacing")] [SerializeField] private float zSpacingDeprecated = 0f;
		[FormerlySerializedAs("useClipping")] [SerializeField] private bool useClippingDeprecated = true;
		[FormerlySerializedAs("immutableTriangles")] [SerializeField] private bool immutableTrianglesDeprecated = false;
		[FormerlySerializedAs("pmaVertexColors")] [SerializeField] private bool pmaVertexColorsDeprecated = true;
		[FormerlySerializedAs("clearStateOnDisable")] [SerializeField] private bool clearStateOnDisableDeprecated = false;
		[FormerlySerializedAs("tintBlack")] [SerializeField] private bool tintBlackDeprecated = false;
		[FormerlySerializedAs("singleSubmesh")] [SerializeField] private bool singleSubmeshDeprecated = false;
		[FormerlySerializedAs("calculateNormals"),
			FormerlySerializedAs("addNormals")]
		[SerializeField] private bool addNormalsDeprecated = false;
		[FormerlySerializedAs("calculateTangents")] [SerializeField] private bool calculateTangentsDeprecated = false;

#if BUILT_IN_SPRITE_MASK_COMPONENT
		[FormerlySerializedAs("maskInteraction")] [SerializeField] private SpriteMaskInteraction maskInteractionDeprecated = SpriteMaskInteraction.None;
#endif // BUILT_IN_SPRITE_MASK_COMPONENT
#endif // UNITY_EDITOR && AUTO_UPGRADE_TO_43_COMPONENTS
		#endregion
	}
}
