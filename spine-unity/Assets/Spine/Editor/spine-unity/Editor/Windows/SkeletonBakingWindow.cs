/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Spine.Unity.Editor {

	using Editor = UnityEditor.Editor;
	using Icons = SpineEditorUtilities.Icons;

	public class SkeletonBakingWindow : EditorWindow {
		const bool IsUtilityWindow = true;

		[MenuItem("CONTEXT/SkeletonDataAsset/Skeleton Baking", false, 5000)]
		public static void Init (MenuCommand command) {
			var window = EditorWindow.GetWindow<SkeletonBakingWindow>(IsUtilityWindow);
			window.minSize = new Vector2(330f, 530f);
			window.maxSize = new Vector2(600f, 1000f);
			window.titleContent = new GUIContent("Skeleton Baking", Icons.spine);
			window.skeletonDataAsset = command.context as SkeletonDataAsset;
			window.Show();
		}

		public SkeletonDataAsset skeletonDataAsset;
		[SpineSkin(dataField:"skeletonDataAsset")]
		public string skinToBake = "default";

		// Settings
		bool bakeAnimations = false;
		bool bakeIK = true;
		SendMessageOptions bakeEventOptions;

		SerializedObject so;
		Skin bakeSkin;


		void DataAssetChanged () {
			bakeSkin = null;
		}

		void OnGUI () {
			so = so ?? new SerializedObject(this);

			EditorGUIUtility.wideMode = true;
			EditorGUILayout.LabelField("Spine Skeleton Prefab Baking", EditorStyles.boldLabel);

			const string BakingWarningMessage = "\nSkeleton baking is not the primary use case for Spine skeletons." +
				"\nUse baking if you have specialized uses, such as simplified skeletons with movement driven by physics." +

				"\n\nBaked Skeletons do not support the following:" +
				"\n\tDisabled rotation or scale inheritance" +
				"\n\tLocal Shear" +
				"\n\tAll Constraint types" +
				"\n\tWeighted mesh verts with more than 4 bound bones" +

				"\n\nBaked Animations do not support the following:" +
				"\n\tMesh Deform Keys" +
				"\n\tColor Keys" +
				"\n\tDraw Order Keys" +

				"\n\nAnimation Curves are sampled at 60fps and are not realtime." +
				"\nConstraint animations are also baked into animation curves." +
				"\nSee SkeletonBaker.cs comments for full details.\n";

			EditorGUILayout.HelpBox(BakingWarningMessage, MessageType.Info, true);

			EditorGUI.BeginChangeCheck();
			var skeletonDataAssetProperty = so.FindProperty("skeletonDataAsset");
			EditorGUILayout.PropertyField(skeletonDataAssetProperty, SpineInspectorUtility.TempContent("SkeletonDataAsset", Icons.spine));
			if (EditorGUI.EndChangeCheck()) {
				so.ApplyModifiedProperties();
				DataAssetChanged();
			}
			EditorGUILayout.Space();

			if (skeletonDataAsset == null) return;
			var skeletonData = skeletonDataAsset.GetSkeletonData(false);
			if (skeletonData == null) return;
			bool hasExtraSkins = skeletonData.Skins.Count > 1;

			using (new SpineInspectorUtility.BoxScope(false)) {
				EditorGUILayout.LabelField(skeletonDataAsset.name, EditorStyles.boldLabel);
				using (new SpineInspectorUtility.IndentScope()) {
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Bones: " + skeletonData.Bones.Count, Icons.bone));
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Slots: " + skeletonData.Slots.Count, Icons.slotRoot));

					if (hasExtraSkins) {
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Skins: " + skeletonData.Skins.Count, Icons.skinsRoot));
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Current skin attachments: " + (bakeSkin == null ? 0 : bakeSkin.Attachments.Count), Icons.skinPlaceholder));
					} else if (skeletonData.Skins.Count == 1) {
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Skins: 1 (only default Skin)", Icons.skinsRoot));
					}

					int totalAttachments = 0;
					foreach (var s in skeletonData.Skins)
						totalAttachments += s.Attachments.Count;
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Total Attachments: " + totalAttachments, Icons.genericAttachment));
				}
			}
			using (new SpineInspectorUtility.BoxScope(false)) {
				EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Animations: " + skeletonData.Animations.Count, Icons.animation));

				using (new SpineInspectorUtility.IndentScope()) {
					bakeAnimations = EditorGUILayout.Toggle(SpineInspectorUtility.TempContent("Bake Animations", Icons.animationRoot), bakeAnimations);
					using (new EditorGUI.DisabledScope(!bakeAnimations)) {
						using (new SpineInspectorUtility.IndentScope()) {
							bakeIK = EditorGUILayout.Toggle(SpineInspectorUtility.TempContent("Bake IK", Icons.constraintIK), bakeIK);
							bakeEventOptions = (SendMessageOptions)EditorGUILayout.EnumPopup(SpineInspectorUtility.TempContent("Event Options", Icons.userEvent), bakeEventOptions);
						}
					}
				}
			}
			EditorGUILayout.Space();

			if (!string.IsNullOrEmpty(skinToBake) && UnityEngine.Event.current.type == EventType.Repaint)
				bakeSkin = skeletonData.FindSkin(skinToBake) ?? skeletonData.DefaultSkin;

			var prefabIcon = EditorGUIUtility.FindTexture("PrefabModel Icon");

			if (hasExtraSkins) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(so.FindProperty("skinToBake"));
				if (EditorGUI.EndChangeCheck()) {
					so.ApplyModifiedProperties();
					Repaint();
				}

				if (SpineInspectorUtility.LargeCenteredButton(SpineInspectorUtility.TempContent(string.Format("Bake Skeleton with Skin ({0})", (bakeSkin == null ? "default" : bakeSkin.Name)), prefabIcon))) {
					SkeletonBaker.BakeToPrefab(skeletonDataAsset, new ExposedList<Skin>(new[] { bakeSkin }), "", bakeAnimations, bakeIK, bakeEventOptions);
				}

				if (SpineInspectorUtility.LargeCenteredButton(SpineInspectorUtility.TempContent(string.Format("Bake All ({0} skins)", skeletonData.Skins.Count), prefabIcon))) {
					SkeletonBaker.BakeToPrefab(skeletonDataAsset, skeletonData.Skins, "", bakeAnimations, bakeIK, bakeEventOptions);
				}
			} else {
				if (SpineInspectorUtility.LargeCenteredButton(SpineInspectorUtility.TempContent("Bake Skeleton", prefabIcon))) {
					SkeletonBaker.BakeToPrefab(skeletonDataAsset, new ExposedList<Skin>(new[] { bakeSkin }), "", bakeAnimations, bakeIK, bakeEventOptions);
				}

			}

		}
	}
}
