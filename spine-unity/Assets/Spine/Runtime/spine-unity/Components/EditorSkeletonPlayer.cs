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

#if UNITY_2022_2_OR_NEWER
#define USE_FIND_OBJECTS_BY_TYPE
#endif

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
	[RequireComponent(typeof(SkeletonAnimation))]
	public class EditorSkeletonPlayer : MonoBehaviour {
		public bool playWhenSelected = true;
		public bool playWhenDeselected = true;
		public float fixedTrackTime = 0.0f;
		private SkeletonAnimation skeletonAnimation;
		private TrackEntry trackEntry;
		private string oldAnimationName;
		private bool oldLoop;
		private double oldTime;

		[DidReloadScripts]
		private static void OnReloaded () {
			// Force start when scripts are reloaded
#if USE_FIND_OBJECTS_BY_TYPE
			EditorSkeletonPlayer[] editorSpineAnimations = FindObjectsByType<EditorSkeletonPlayer>(FindObjectsSortMode.None);
#else
			EditorSkeletonPlayer[] editorSpineAnimations = FindObjectsOfType<EditorSkeletonPlayer>();
#endif

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

			if (skeletonAnimation == null) {
				skeletonAnimation = this.GetComponent<SkeletonAnimation>();
			}

			oldTime = EditorApplication.timeSinceStartup;
			EditorApplication.update += EditorUpdate;
		}

		private void OnDestroy () {
			EditorApplication.update -= EditorUpdate;
		}

		private void Update () {
			if (enabled == false || Application.isPlaying) return;
			if (skeletonAnimation == null) return;
			AnimationState animationState = skeletonAnimation.AnimationState;
			if (animationState == null || animationState.Tracks.Count == 0) return;

			TrackEntry currentEntry = animationState.Tracks.Items[0];
			if (currentEntry != null && fixedTrackTime != 0) {
				currentEntry.TrackTime = fixedTrackTime;
			}
		}

		private void EditorUpdate () {
			if (enabled == false || Application.isPlaying) return;
			if (skeletonAnimation == null) return;
			AnimationState animationState = skeletonAnimation.AnimationState;
			if (animationState == null) return;
			bool isSelected = Selection.Contains(this.gameObject);
			if (!this.playWhenSelected && isSelected) return;
			if (!this.playWhenDeselected && !isSelected) return;
			if (fixedTrackTime != 0) return;

			// Update animation
			string animationName = skeletonAnimation.AnimationName;
			bool loop = skeletonAnimation.loop;
			if (oldAnimationName != animationName || oldLoop != loop) {
				SkeletonData skeletonData = skeletonAnimation.Skeleton.Data;
				Spine.Animation animation = (skeletonData == null || animationName == null) ?
					null : skeletonData.FindAnimation(animationName);
				if (animation != null)
					trackEntry = animationState.SetAnimation(0, animationName, loop);
				else
					trackEntry = animationState.SetEmptyAnimation(0, 0);
				oldAnimationName = animationName;
				oldLoop = loop;
			}

			// Update speed
			if (trackEntry != null)
				trackEntry.TimeScale = skeletonAnimation.timeScale;

			float deltaTime = (float)(EditorApplication.timeSinceStartup - oldTime);
			skeletonAnimation.Update(deltaTime);
			skeletonAnimation.Renderer.UpdateMesh();
			oldTime = EditorApplication.timeSinceStartup;

			// Force repaint to update animation smoothly
#if UNITY_2017_2_OR_NEWER
			EditorApplication.QueuePlayerLoopUpdate();
#else
			SceneView.RepaintAll();
#endif
		}
	}
}
#endif
