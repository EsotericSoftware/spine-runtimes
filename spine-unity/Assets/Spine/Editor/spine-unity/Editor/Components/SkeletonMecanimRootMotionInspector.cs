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

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonMecanimRootMotion))]
	[CanEditMultipleObjects]
	public class SkeletonMecanimRootMotionInspector : SkeletonRootMotionBaseInspector {
		protected SerializedProperty mecanimLayerFlags;

		protected GUIContent mecanimLayersLabel;

		protected override void OnEnable () {
			base.OnEnable();
			mecanimLayerFlags = serializedObject.FindProperty("mecanimLayerFlags");

			mecanimLayersLabel = new UnityEngine.GUIContent("Mecanim Layers", "Mecanim layers to apply root motion at. Defaults to the first Mecanim layer.");
		}

		override public void OnInspectorGUI () {

			base.MainPropertyFields();
			MecanimLayerMaskPropertyField();

			base.OptionalPropertyFields();
			serializedObject.ApplyModifiedProperties();
		}

		protected string[] GetLayerNames () {
			int maxLayerCount = 0;
			int maxIndex = 0;
			for (int i = 0; i < targets.Length; ++i) {
				SkeletonMecanim skeletonMecanim = ((SkeletonMecanimRootMotion)targets[i]).SkeletonMecanim;
				int count = skeletonMecanim.Translator.MecanimLayerCount;
				if (count > maxLayerCount) {
					maxLayerCount = count;
					maxIndex = i;
				}
			}
			if (maxLayerCount == 0)
				return new string[0];
			SkeletonMecanim skeletonMecanimMaxLayers = ((SkeletonMecanimRootMotion)targets[maxIndex]).SkeletonMecanim;
			return skeletonMecanimMaxLayers.Translator.MecanimLayerNames;
		}

		protected void MecanimLayerMaskPropertyField () {
			string[] layerNames = GetLayerNames();
			if (layerNames.Length > 0)
				mecanimLayerFlags.intValue = EditorGUILayout.MaskField(
					mecanimLayersLabel, mecanimLayerFlags.intValue, layerNames);
		}
	}
}
