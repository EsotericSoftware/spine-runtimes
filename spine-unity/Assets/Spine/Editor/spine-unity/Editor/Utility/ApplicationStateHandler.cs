/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#pragma warning disable 0219

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	[InitializeOnLoad]
	public static class ApplicationStateHandler {

		static ApplicationStateHandler () {
			Initialize();
		}

		static void Initialize () {
#if NEWPLAYMODECALLBACKS
			EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
			EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
			OnPlaymodeStateChanged(PlayModeStateChange.EnteredEditMode);
#else
			EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
			EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
			OnPlaymodeStateChanged();
#endif
		}

#if NEWPLAYMODECALLBACKS
		internal static void OnPlaymodeStateChanged (PlayModeStateChange stateChange) {
			bool isPlaying = stateChange == PlayModeStateChange.EnteredPlayMode ||
				stateChange == PlayModeStateChange.ExitingEditMode;
#else
		internal static void OnPlaymodeStateChanged () {
			bool isPlaying = false;
			if (EditorApplication.isPaused ||
				EditorApplication.isPlaying ||
				EditorApplication.isPlayingOrWillChangePlaymode) isPlaying = true;
#endif
			UpdateApplicationStateToPlaying(isPlaying);
		}

		public static void UpdateApplicationStateToPlaying (bool isPlaying) {
			SkeletonRenderer.ApplicationIsPlaying = isPlaying;
			SkeletonGraphic.ApplicationIsPlaying = isPlaying;
			SkeletonAnimationBase.ApplicationIsPlaying = isPlaying;
		}
	}
}
