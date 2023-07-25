/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2022, Esoteric Software LLC
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

using Spine.Unity.Playables;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Spine.Unity.Editor {
#if UNITY_2019_1_OR_NEWER
	[CustomTimelineEditor(typeof(SpineAnimationStateGraphicTrack))]
	[CanEditMultipleObjects]
	public class SpineAnimationStateGraphicTrackInspector : TrackEditor {

		public override TrackDrawOptions GetTrackOptions (TrackAsset track, UnityEngine.Object binding) {
			TrackDrawOptions options = base.GetTrackOptions(track, binding);
			options.icon = SpineEditorUtilities.Icons.skeletonDataAssetIcon;
			options.trackColor = new Color(255 / 255.0f, 64 / 255.0f, 1 / 255.0f);
			return options;
		}
	}
#else
	[CustomEditor(typeof(SpineAnimationStateGraphicTrack))]
	[CanEditMultipleObjects]
	public class SpineAnimationStateGraphicTrackInspector : UnityEditor.Editor {

		protected SerializedProperty trackIndexProperty = null;

		public void OnEnable () {
			trackIndexProperty = serializedObject.FindProperty("trackIndex");
		}

		public override void OnInspectorGUI () {
			serializedObject.Update();
			EditorGUILayout.PropertyField(trackIndexProperty);
			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}
