/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Spine.Unity {

	/// <summary>
	/// Experimental Editor Skeleton Player component enabling Editor playback of the
	/// selected animation outside of Play mode for SkeletonAnimation and SkeletonGraphic.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Spine/EditorSkeletonPlayer")]
	[RequireComponent(typeof(ISkeletonAnimation))]
	public class EditorSkeletonPlayer : MonoBehaviour {
		public bool playWhenSelected = true;
		public bool playWhenDeselected = true;
		public float fixedTrackTime = 0.0f;
		private IEditorSkeletonWrapper skeletonWrapper;
		private TrackEntry trackEntry;
		private string oldAnimationName;
		private bool oldLoop;
		private double oldTime;

		[DidReloadScripts]
		private static void OnReloaded () {
			// Force start when scripts are reloaded
			EditorSkeletonPlayer[] editorSpineAnimations = FindObjectsOfType<EditorSkeletonPlayer>();

			foreach (EditorSkeletonPlayer editorSpineAnimation in editorSpineAnimations)
				editorSpineAnimation.Start();
		}

		private void Reset () {
			// Note: when a skeleton has a varying number of active materials,
			// we're moving this component first in the hierarchy to still be
			// able to disable this component.
			for (int i = 0; i < 10; ++i)
				UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
		}

		private void Start () {
			if (Application.isPlaying) return;

			if (skeletonWrapper == null) {
				SkeletonAnimation skeletonAnimation;
				SkeletonGraphic skeletonGraphic;
				if (skeletonAnimation = this.GetComponent<SkeletonAnimation>())
					skeletonWrapper = new SkeletonAnimationWrapper(skeletonAnimation);
				else if (skeletonGraphic = this.GetComponent<SkeletonGraphic>())
					skeletonWrapper = new SkeletonGraphicWrapper(skeletonGraphic);
			}

			oldTime = EditorApplication.timeSinceStartup;
			EditorApplication.update += EditorUpdate;
		}

		private void OnDestroy () {
			EditorApplication.update -= EditorUpdate;
		}

		private void Update () {
			if (enabled == false || Application.isPlaying) return;
			if (skeletonWrapper == null) return;
			if (skeletonWrapper.State == null || skeletonWrapper.State.Tracks.Count == 0) return;

			TrackEntry currentEntry = skeletonWrapper.State.Tracks.Items[0];
			if (currentEntry != null && fixedTrackTime != 0) {
				currentEntry.TrackTime = fixedTrackTime;
			}
		}

		private void EditorUpdate () {
			if (enabled == false || Application.isPlaying) return;
			if (skeletonWrapper == null) return;
			if (skeletonWrapper.State == null) return;
			bool isSelected = Selection.Contains(this.gameObject);
			if (!this.playWhenSelected && isSelected) return;
			if (!this.playWhenDeselected && !isSelected) return;
			if (fixedTrackTime != 0) return;

			// Update animation
			if (oldAnimationName != skeletonWrapper.AnimationName || oldLoop != skeletonWrapper.Loop) {
				SkeletonData skeletonData = skeletonWrapper.SkeletonData;
				Spine.Animation animation = (skeletonData == null || skeletonWrapper.AnimationName == null) ?
					null : skeletonData.FindAnimation(skeletonWrapper.AnimationName);
				if (animation != null)
					trackEntry = skeletonWrapper.State.SetAnimation(0, skeletonWrapper.AnimationName, skeletonWrapper.Loop);
				else
					trackEntry = skeletonWrapper.State.SetEmptyAnimation(0, 0);
				oldAnimationName = skeletonWrapper.AnimationName;
				oldLoop = skeletonWrapper.Loop;
			}

			// Update speed
			if (trackEntry != null)
				trackEntry.TimeScale = skeletonWrapper.Speed;

			float deltaTime = (float)(EditorApplication.timeSinceStartup - oldTime);
			skeletonWrapper.Update(deltaTime);
			oldTime = EditorApplication.timeSinceStartup;

			// Force repaint to update animation smoothly
#if UNITY_2017_2_OR_NEWER
			EditorApplication.QueuePlayerLoopUpdate();
#else
			SceneView.RepaintAll();
#endif
		}

		private class SkeletonAnimationWrapper : IEditorSkeletonWrapper {
			private SkeletonAnimation skeletonAnimation;

			public SkeletonAnimationWrapper (SkeletonAnimation skeletonAnimation) {
				this.skeletonAnimation = skeletonAnimation;
			}

			public Spine.SkeletonData SkeletonData {
				get {
					if (!skeletonAnimation.SkeletonDataAsset) return null;
					return skeletonAnimation.SkeletonDataAsset.GetSkeletonData(true);
				}
			}

			public string AnimationName { get { return skeletonAnimation.AnimationName; } }
			public bool Loop { get { return skeletonAnimation.loop; } }
			public float Speed { get { return skeletonAnimation.timeScale; } }
			public Spine.AnimationState State { get { return skeletonAnimation.state; } }

			public void Update (float deltaTime) {
				skeletonAnimation.Update(deltaTime);
			}
		}

		private class SkeletonGraphicWrapper : IEditorSkeletonWrapper {
			private SkeletonGraphic skeletonGraphic;

			public SkeletonGraphicWrapper (SkeletonGraphic skeletonGraphic) {
				this.skeletonGraphic = skeletonGraphic;
			}

			public Spine.SkeletonData SkeletonData { get { return skeletonGraphic.SkeletonData; } }
			public string AnimationName { get { return skeletonGraphic.startingAnimation; } }
			public bool Loop { get { return skeletonGraphic.startingLoop; } }
			public float Speed { get { return skeletonGraphic.timeScale; } }
			public Spine.AnimationState State { get { return skeletonGraphic.AnimationState; } }

			public void Update (float deltaTime) {
				skeletonGraphic.Update(deltaTime);
			}
		}

		private interface IEditorSkeletonWrapper {
			string AnimationName { get; }
			Spine.SkeletonData SkeletonData { get; }
			bool Loop { get; }
			float Speed { get; }
			Spine.AnimationState State { get; }
			void Update (float deltaTime);
		}
	}
}
#endif
