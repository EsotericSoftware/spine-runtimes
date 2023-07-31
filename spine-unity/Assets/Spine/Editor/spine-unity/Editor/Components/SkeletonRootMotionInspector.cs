/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
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

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonRootMotion))]
	[CanEditMultipleObjects]
	public class SkeletonRootMotionInspector : SkeletonRootMotionBaseInspector {
		protected SerializedProperty animationTrackFlags;
		protected GUIContent animationTrackFlagsLabel;

		string[] TrackNames;

		protected override void OnEnable () {
			base.OnEnable();

			animationTrackFlags = serializedObject.FindProperty("animationTrackFlags");
			animationTrackFlagsLabel = new UnityEngine.GUIContent("Animation Tracks",
				"Animation tracks to apply root motion at. Defaults to the first" +
				" animation track (index 0).");
		}

		override public void OnInspectorGUI () {

			base.MainPropertyFields();
			AnimationTracksPropertyField();

			base.OptionalPropertyFields();
			serializedObject.ApplyModifiedProperties();
		}

		protected void AnimationTracksPropertyField () {

			if (TrackNames == null) {
				InitTrackNames();

			}

			animationTrackFlags.intValue = EditorGUILayout.MaskField(
				animationTrackFlagsLabel, animationTrackFlags.intValue, TrackNames);
		}

		protected void InitTrackNames () {
			int numEntries = 32;
			TrackNames = new string[numEntries];
			for (int i = 0; i < numEntries; ++i) {
				TrackNames[i] = string.Format("Track {0}", i);
			}
		}
	}
}
