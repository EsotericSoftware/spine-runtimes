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

			const string BakingWarningMessage = "\nThe main use of Baking is to export Spine projects to be used without the Spine Runtime (ie: for sale on the Asset Store, or background objects that are animated only with a wind noise generator)" +

				"\n\nBaking does not support the following:" +
				"\n\tDisabled transform inheritance" +
				"\n\tShear" +
				"\n\tColor Keys" +
				"\n\tDraw Order Keys" +
				"\n\tAll Constraint types" +

				"\n\nCurves are sampled at 60fps and are not realtime." +
				"\nPlease read SkeletonBaker.cs comments for full details.\n";
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

			using (new SpineInspectorUtility.BoxScope(false)) {
				EditorGUILayout.LabelField(skeletonDataAsset.name, EditorStyles.boldLabel);
				using (new SpineInspectorUtility.IndentScope()) {
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Bones: " + skeletonData.Bones.Count, Icons.bone));

					int totalAttachments = 0;
					foreach (var s in skeletonData.Skins) totalAttachments += s.Attachments.Count;

					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Total Attachments: " + totalAttachments, Icons.genericAttachment));
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Current skin attachments: " + (bakeSkin == null ? 0 : bakeSkin.Attachments.Count), Icons.skinPlaceholder));
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Animations: " + skeletonData.Animations.Count, Icons.animation));
				}
			}
			using (new SpineInspectorUtility.BoxScope(false)) {
				EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
				using (new SpineInspectorUtility.IndentScope()) {
					bakeAnimations = EditorGUILayout.Toggle(SpineInspectorUtility.TempContent("Bake Animations", Icons.animationRoot), bakeAnimations);
					bakeIK = EditorGUILayout.Toggle(SpineInspectorUtility.TempContent("Bake IK", Icons.constraintIK), bakeIK);
					bakeEventOptions = (SendMessageOptions)EditorGUILayout.EnumPopup(SpineInspectorUtility.TempContent("Event Options", Icons.userEvent), bakeEventOptions);
				}
			}
			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(so.FindProperty("skinToBake"));
			if (EditorGUI.EndChangeCheck()) {
				so.ApplyModifiedProperties();
				Repaint();
			}

			if (!string.IsNullOrEmpty(skinToBake) && UnityEngine.Event.current.type == EventType.Repaint)
				bakeSkin = skeletonData.FindSkin(skinToBake) ?? skeletonData.DefaultSkin;
			
			var prefabIcon = EditorGUIUtility.FindTexture("PrefabModel Icon");

			if (SpineInspectorUtility.LargeCenteredButton(SpineInspectorUtility.TempContent(string.Format("Bake Skin ({0})", (bakeSkin == null ? "default" : bakeSkin.Name)), prefabIcon))) {
				SkeletonBaker.BakeToPrefab(skeletonDataAsset, new ExposedList<Skin>(new [] { bakeSkin }), "", bakeAnimations, bakeIK, bakeEventOptions);
			}


			if (skeletonData.Skins.Count > 1) {
				if (SpineInspectorUtility.LargeCenteredButton(SpineInspectorUtility.TempContent(string.Format("Bake All ({0} skins)", skeletonData.Skins.Count), prefabIcon))) {
					SkeletonBaker.BakeToPrefab(skeletonDataAsset, skeletonData.Skins, "", bakeAnimations, bakeIK, bakeEventOptions);
				}
			}

		}
	}
}
