/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

#define SPINE_SKELETON_MECANIM

#if (UNITY_2017_4 || UNITY_2018_1_OR_NEWER)
#define SPINE_UNITY_2018_PREVIEW_API
#endif


using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using CompatibilityProblemInfo = Spine.Unity.SkeletonDataCompatibility.CompatibilityProblemInfo;

namespace Spine.Unity.Editor {
	using Animation = Spine.Animation;
	using Event = UnityEngine.Event;
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonDataAsset)), CanEditMultipleObjects]
	public class SkeletonDataAssetInspector : UnityEditor.Editor {
		internal static bool showAnimationStateData = true;
		internal static bool showAnimationList = true;
		internal static bool showSlotList = false;
		internal static bool showAttachments = false;

		SerializedProperty atlasAssets, skeletonJSON, scale, fromAnimation, toAnimation, duration, defaultMix;
		SerializedProperty skeletonDataModifiers;
		SerializedProperty blendModeMaterials;
#if SPINE_TK2D
		SerializedProperty spriteCollection;
#endif

#if SPINE_SKELETON_MECANIM
		static bool isMecanimExpanded = false;
		SerializedProperty controller;
#endif

		SkeletonDataAsset targetSkeletonDataAsset;
		SkeletonData targetSkeletonData;

		readonly List<string> warnings = new List<string>();
		CompatibilityProblemInfo compatibilityProblemInfo = null;
		readonly SkeletonInspectorPreview preview = new SkeletonInspectorPreview();
		bool requiresReload = false;

		GUIStyle activePlayButtonStyle, idlePlayButtonStyle;
		readonly GUIContent DefaultMixLabel = new GUIContent("Default Mix Duration", "Sets 'SkeletonDataAsset.defaultMix' in the asset and 'AnimationState.data.defaultMix' at runtime load time.");

		string TargetAssetGUID { get { return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(targetSkeletonDataAsset)); } }
		string LastSkinKey { get { return TargetAssetGUID + "_lastSkin"; } }
		string LastSkinName { get { return EditorPrefs.GetString(LastSkinKey, ""); } }

		void OnEnable () {
			InitializeEditor();
		}

		void OnDestroy () {
			HandleOnDestroyPreview();
			AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
			EditorApplication.update -= preview.HandleEditorUpdate;
		}

		private void OnDomainUnload (object sender, EventArgs e) {
			OnDestroy();
		}

		public void UpdateSkeletonData () {
			preview.Clear();
			InitializeEditor();
			if (targetSkeletonDataAsset)
				EditorUtility.SetDirty(targetSkeletonDataAsset);
		}

		void InitializeEditor () {
			SpineEditorUtilities.ConfirmInitialization();
			targetSkeletonDataAsset = (SkeletonDataAsset)target;

			bool newAtlasAssets = atlasAssets == null;
			if (newAtlasAssets) atlasAssets = serializedObject.FindProperty("atlasAssets");
			skeletonJSON = serializedObject.FindProperty("skeletonJSON");
			scale = serializedObject.FindProperty("scale");
			fromAnimation = serializedObject.FindProperty("fromAnimation");
			toAnimation = serializedObject.FindProperty("toAnimation");
			duration = serializedObject.FindProperty("duration");
			defaultMix = serializedObject.FindProperty("defaultMix");

			skeletonDataModifiers = serializedObject.FindProperty("skeletonDataModifiers");
			blendModeMaterials = serializedObject.FindProperty("blendModeMaterials");

#if SPINE_SKELETON_MECANIM
			controller = serializedObject.FindProperty("controller");
#endif

#if SPINE_TK2D
			if (newAtlasAssets) atlasAssets.isExpanded = false;
			spriteCollection = serializedObject.FindProperty("spriteCollection");
#else
			// Analysis disable once ConvertIfToOrExpression
			if (newAtlasAssets) atlasAssets.isExpanded = true;
#endif

			// This handles the case where the managed editor assembly is unloaded before recompilation when code changes.
			AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
			AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;

			EditorApplication.update -= preview.HandleEditorUpdate;
			EditorApplication.update += preview.HandleEditorUpdate;
			preview.OnSkinChanged -= HandlePreviewSkinChanged;
			preview.OnSkinChanged += HandlePreviewSkinChanged;

			PopulateWarnings();
			if (targetSkeletonDataAsset.skeletonJSON == null) {
				targetSkeletonData = null;
				return;
			}

			targetSkeletonData = NoProblems() ? targetSkeletonDataAsset.GetSkeletonData(false) : null;

			if (targetSkeletonData != null && NoProblems()) {
				preview.Initialize(this.Repaint, targetSkeletonDataAsset, this.LastSkinName);
			}

		}

		void Clear () {
			preview.Clear();
			SpineEditorUtilities.ClearSkeletonDataAsset(targetSkeletonDataAsset);
			targetSkeletonData = null;
		}

		override public void OnInspectorGUI () {
			// Multi-Editing
			if (serializedObject.isEditingMultipleObjects) {
				OnInspectorGUIMulti();
				return;
			}

			{ // Lazy initialization because accessing EditorStyles values in OnEnable during a recompile causes UnityEditor to throw null exceptions. (Unity 5.3.5)
				idlePlayButtonStyle = idlePlayButtonStyle ?? new GUIStyle(EditorStyles.miniButton);
				if (activePlayButtonStyle == null) {
					activePlayButtonStyle = new GUIStyle(idlePlayButtonStyle);
					activePlayButtonStyle.normal.textColor = Color.red;
				}
			}

			serializedObject.Update();

			// Header
			EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(target.name + " (SkeletonDataAsset)", Icons.spine), EditorStyles.whiteLargeLabel);
			if (targetSkeletonData != null) EditorGUILayout.LabelField("(Drag and Drop to instantiate.)", EditorStyles.miniLabel);

			// Main Serialized Fields
			using (var changeCheck = new EditorGUI.ChangeCheckScope()) {
				using (new SpineInspectorUtility.BoxScope())
					DrawSkeletonDataFields();

				if (compatibilityProblemInfo != null)
					return;

				using (new SpineInspectorUtility.BoxScope()) {
					DrawAtlasAssetsFields();
					HandleAtlasAssetsNulls();
				}

				if (changeCheck.changed) {
					if (serializedObject.ApplyModifiedProperties() || requiresReload) {
						this.Clear();
						this.InitializeEditor();

						if (SpineEditorUtilities.Preferences.autoReloadSceneSkeletons && NoProblems())
							SpineEditorUtilities.DataReloadHandler.ReloadSceneSkeletonComponents(targetSkeletonDataAsset);
						return;
					}
				}
			}

			// Unity Quirk: Some code depends on valid preview. If preview is initialized elsewhere, this can cause contents to change between Layout and Repaint events, causing GUILayout control count errors.
			if (NoProblems())
				preview.Initialize(this.Repaint, targetSkeletonDataAsset, this.LastSkinName);

			if (targetSkeletonData != null) {
				GUILayout.Space(20f);

				using (new SpineInspectorUtility.BoxScope(false)) {
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Mix Settings", Icons.animationRoot), EditorStyles.boldLabel);
					DrawAnimationStateInfo();
					EditorGUILayout.Space();
				}

				EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
				DrawAnimationList();
				if (targetSkeletonData.Animations.Count > 0) {
					const string AnimationReferenceButtonText = "Create Animation Reference Assets";
					const string AnimationReferenceTooltipText = "AnimationReferenceAsset acts as Unity asset for a reference to a Spine.Animation. This can be used in inspectors.\n\nIt serializes a reference to a SkeletonData asset and an animationName.\n\nAt runtime, a reference to its Spine.Animation is loaded and cached into the object to be used as needed. This skips the need to find and cache animation references in individual MonoBehaviours.";
					if (GUILayout.Button(SpineInspectorUtility.TempContent(AnimationReferenceButtonText, Icons.animationRoot, AnimationReferenceTooltipText), GUILayout.Width(250), GUILayout.Height(26))) {
						CreateAnimationReferenceAssets();
					}
				}
				EditorGUILayout.Space();
				DrawSlotList();
				EditorGUILayout.Space();

				DrawUnityTools();

			} else {
#if !SPINE_TK2D
				// Draw Reimport Button
				using (new EditorGUI.DisabledGroupScope(skeletonJSON.objectReferenceValue == null)) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Attempt Reimport", Icons.warning)))
						DoReimport();
				}
#else
				EditorGUILayout.HelpBox("Couldn't load SkeletonData.", MessageType.Error);
#endif

				DrawWarningList();
			}

			if (!Application.isPlaying)
				serializedObject.ApplyModifiedProperties();
		}

		void CreateAnimationReferenceAssets () {
			const string AssetFolderName = "ReferenceAssets";
			string parentFolder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(targetSkeletonDataAsset));
			string dataPath = parentFolder + "/" + AssetFolderName;
			if (!AssetDatabase.IsValidFolder(dataPath)) {
				AssetDatabase.CreateFolder(parentFolder, AssetFolderName);
			}

			FieldInfo nameField = typeof(AnimationReferenceAsset).GetField("animationName", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo skeletonDataAssetField = typeof(AnimationReferenceAsset).GetField("skeletonDataAsset", BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var animation in targetSkeletonData.Animations) {
				string assetPath = string.Format("{0}/{1}.asset", dataPath, AssetUtility.GetPathSafeName(animation.Name));
				AnimationReferenceAsset existingAsset = AssetDatabase.LoadAssetAtPath<AnimationReferenceAsset>(assetPath);
				if (existingAsset == null) {
					AnimationReferenceAsset newAsset = ScriptableObject.CreateInstance<AnimationReferenceAsset>();
					skeletonDataAssetField.SetValue(newAsset, targetSkeletonDataAsset);
					nameField.SetValue(newAsset, animation.Name);
					AssetDatabase.CreateAsset(newAsset, assetPath);
				}
			}

			var folderObject = AssetDatabase.LoadAssetAtPath(dataPath, typeof(UnityEngine.Object));
			if (folderObject != null) {
				Selection.activeObject = folderObject;
				EditorGUIUtility.PingObject(folderObject);
			}
		}

		void OnInspectorGUIMulti () {

			// Skeleton data file field.
			using (new SpineInspectorUtility.BoxScope()) {
				EditorGUILayout.LabelField("SkeletonData", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(skeletonJSON, SpineInspectorUtility.TempContent(skeletonJSON.displayName, Icons.spine));
				EditorGUILayout.DelayedFloatField(scale); //EditorGUILayout.PropertyField(scale);
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(skeletonDataModifiers, true);

				DrawBlendModeMaterialProperties();
			}

			// Texture source field.
			using (new SpineInspectorUtility.BoxScope()) {
				EditorGUILayout.LabelField("Atlas", EditorStyles.boldLabel);
#if !SPINE_TK2D
				EditorGUILayout.PropertyField(atlasAssets, true);
#else
				using (new EditorGUI.DisabledGroupScope(spriteCollection.objectReferenceValue != null)) {
					EditorGUILayout.PropertyField(atlasAssets, true);
				}
				EditorGUILayout.LabelField("spine-tk2d", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(spriteCollection, true);
#endif
			}

			// Mix settings.
			using (new SpineInspectorUtility.BoxScope()) {
				EditorGUILayout.LabelField("Mix Settings", EditorStyles.boldLabel);
				SpineInspectorUtility.PropertyFieldWideLabel(defaultMix, DefaultMixLabel, 160);
				EditorGUILayout.Space();
			}

		}

		void DrawBlendModeMaterialProperties () {
			if (skeletonDataModifiers.arraySize > 0) {
				EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
				EditorGUILayout.PrefixLabel("Blend Modes");
				if (GUILayout.Button(new GUIContent("Upgrade", "Upgrade BlendModeMaterialAsset to built-in BlendModeMaterials."), EditorStyles.miniButton, GUILayout.Width(65f))) {
					foreach (SkeletonDataAsset skeletonData in targets) {
						BlendModeMaterialsUtility.UpgradeBlendModeMaterials(skeletonData);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(blendModeMaterials, true);
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
				foreach (SkeletonDataAsset skeletonData in targets) {
					BlendModeMaterialsUtility.UpdateBlendModeMaterials(skeletonData);
				}
			}
		}

		void DrawSkeletonDataFields () {
			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.LabelField("SkeletonData", EditorStyles.boldLabel);
				if (targetSkeletonData != null) {
					var sd = targetSkeletonData;
					string m = string.Format("{8} - {0} {1}\nBones: {2}\nConstraints: \n {5} IK \n {6} Path \n {7} Transform\n\nSlots: {3}\nSkins: {4}\n\nAnimations: {9}",
						sd.Version, string.IsNullOrEmpty(sd.Version) ? "" : "export          ", sd.Bones.Count, sd.Slots.Count, sd.Skins.Count, sd.IkConstraints.Count, sd.PathConstraints.Count, sd.TransformConstraints.Count, skeletonJSON.objectReferenceValue.name, sd.Animations.Count);
					EditorGUILayout.LabelField(GUIContent.none, new GUIContent(Icons.info, m), GUILayout.Width(30f));
				}
			}
			EditorGUILayout.PropertyField(skeletonJSON, SpineInspectorUtility.TempContent(skeletonJSON.displayName, Icons.spine));

			if (compatibilityProblemInfo != null) {
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(compatibilityProblemInfo.DescriptionString(), Icons.warning), GUILayout.Height(52));
				return;
			}

			EditorGUILayout.DelayedFloatField(scale); //EditorGUILayout.PropertyField(scale);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(skeletonDataModifiers, true);

			DrawBlendModeMaterialProperties();
		}

		void DrawAtlasAssetsFields () {
			EditorGUILayout.LabelField("Atlas", EditorStyles.boldLabel);

			using (var changeCheck = new EditorGUI.ChangeCheckScope()) {
#if !SPINE_TK2D
				EditorGUILayout.PropertyField(atlasAssets, true);
#else
				using (new EditorGUI.DisabledGroupScope(spriteCollection.objectReferenceValue != null)) {
					EditorGUILayout.PropertyField(atlasAssets, true);
				}
				EditorGUILayout.LabelField("spine-tk2d", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(spriteCollection, true);
#endif
				if (atlasAssets.arraySize == 0)
					EditorGUILayout.HelpBox("AtlasAssets array is empty. Skeleton's attachments will load without being mapped to images.", MessageType.Info);

				if (changeCheck.changed) {
					requiresReload = true;
				}
			}
		}

		void HandleAtlasAssetsNulls () {
			bool hasNulls = false;
			foreach (var a in targetSkeletonDataAsset.atlasAssets) {
				if (a == null) {
					hasNulls = true;
					break;
				}
			}
			if (hasNulls) {
				if (targetSkeletonDataAsset.atlasAssets.Length == 1) {
					EditorGUILayout.HelpBox("Atlas array cannot have null entries!", MessageType.None);
				} else {
					EditorGUILayout.HelpBox("Atlas array should not have null entries!", MessageType.Error);
					if (SpineInspectorUtility.CenteredButton(SpineInspectorUtility.TempContent("Remove null entries"))) {
						var trimmedAtlasAssets = new List<AtlasAssetBase>();
						foreach (var a in targetSkeletonDataAsset.atlasAssets) {
							if (a != null)
								trimmedAtlasAssets.Add(a);
						}
						targetSkeletonDataAsset.atlasAssets = trimmedAtlasAssets.ToArray();
						serializedObject.Update();
					}
				}
			}
		}

		void DrawAnimationStateInfo () {
			using (new SpineInspectorUtility.IndentScope())
				showAnimationStateData = EditorGUILayout.Foldout(showAnimationStateData, "Animation State Data");

			if (!showAnimationStateData)
				return;

			using (var cc = new EditorGUI.ChangeCheckScope()) {
				using (new SpineInspectorUtility.IndentScope())
					SpineInspectorUtility.PropertyFieldWideLabel(defaultMix, DefaultMixLabel, 160);


				if (fromAnimation.arraySize > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						EditorGUILayout.LabelField("Custom Mix Durations", EditorStyles.boldLabel);
					}

					for (int i = 0; i < fromAnimation.arraySize; i++) {
						SerializedProperty from = fromAnimation.GetArrayElementAtIndex(i);
						SerializedProperty to = toAnimation.GetArrayElementAtIndex(i);
						SerializedProperty durationProp = duration.GetArrayElementAtIndex(i);
						using (new EditorGUILayout.HorizontalScope()) {
							GUILayout.Space(16f); // Space instead of EditorGUIUtility.indentLevel. indentLevel will add the space on every field.
							EditorGUILayout.PropertyField(from, GUIContent.none);
							//EditorGUILayout.LabelField(">", EditorStyles.miniLabel, GUILayout.Width(9f));
							EditorGUILayout.PropertyField(to, GUIContent.none);
							//GUILayout.Space(5f);
							durationProp.floatValue = EditorGUILayout.FloatField(durationProp.floatValue, GUILayout.MinWidth(25f), GUILayout.MaxWidth(60f));
							if (GUILayout.Button("Delete", EditorStyles.miniButton)) {
								duration.DeleteArrayElementAtIndex(i);
								toAnimation.DeleteArrayElementAtIndex(i);
								fromAnimation.DeleteArrayElementAtIndex(i);
							}
						}
					}
				}

				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					if (GUILayout.Button("Add Custom Mix")) {
						duration.arraySize++;
						toAnimation.arraySize++;
						fromAnimation.arraySize++;
					}
					EditorGUILayout.Space();
				}

				if (cc.changed) {
					targetSkeletonDataAsset.FillStateData(quiet: true);
					EditorUtility.SetDirty(targetSkeletonDataAsset);
					serializedObject.ApplyModifiedProperties();
				}
			}
		}

		void DrawAnimationList () {
			showAnimationList = EditorGUILayout.Foldout(showAnimationList, SpineInspectorUtility.TempContent(string.Format("Animations [{0}]", targetSkeletonData.Animations.Count), Icons.animationRoot));
			if (!showAnimationList)
				return;

			bool isPreviewWindowOpen = preview.IsValid;

			if (isPreviewWindowOpen) {
				if (GUILayout.Button(SpineInspectorUtility.TempContent("Setup Pose", Icons.skeleton), GUILayout.Width(105), GUILayout.Height(18))) {
					preview.ClearAnimationSetupPose();
					preview.RefreshOnNextUpdate();
				}
			} else {
				EditorGUILayout.HelpBox("Animations can be previewed if you expand the Preview window below.", MessageType.Info);
			}

			EditorGUILayout.LabelField("Name", "      Duration");
			//bool nonessential = targetSkeletonData.ImagesPath != null; // Currently the only way to determine if skeleton data has nonessential data. (Spine 3.6)
			//float fps = targetSkeletonData.Fps;
			//if (nonessential && fps == 0) fps = 30;

			var activeTrack = preview.ActiveTrack;
			foreach (Animation animation in targetSkeletonData.Animations) {
				using (new GUILayout.HorizontalScope()) {
					if (isPreviewWindowOpen) {
						bool active = activeTrack != null && activeTrack.Animation == animation;
						//bool sameAndPlaying = active && activeTrack.TimeScale > 0f;
						if (GUILayout.Button("\u25BA", active ? activePlayButtonStyle : idlePlayButtonStyle, GUILayout.Width(24))) {
							preview.PlayPauseAnimation(animation.Name, true);
							activeTrack = preview.ActiveTrack;
						}
					} else {
						GUILayout.Label("-", GUILayout.Width(24));
					}
					//string frameCountString = (fps > 0) ? ("(" + (Mathf.RoundToInt(animation.Duration * fps)) + ")").PadLeft(12, ' ') : string.Empty;
					//EditorGUILayout.LabelField(new GUIContent(animation.Name, Icons.animation), SpineInspectorUtility.TempContent(animation.Duration.ToString("f3") + "s" + frameCountString));
					string durationString = animation.Duration.ToString("f3");
					EditorGUILayout.LabelField(new GUIContent(animation.Name, Icons.animation), SpineInspectorUtility.TempContent(durationString + "s", tooltip: string.Format("{0} seconds\n{1} timelines", durationString, animation.Timelines.Count)));
				}
			}
		}

		void DrawSlotList () {
			showSlotList = EditorGUILayout.Foldout(showSlotList, SpineInspectorUtility.TempContent("Slots", Icons.slotRoot));

			if (!showSlotList) return;
			if (!preview.IsValid) return;

			var defaultSkin = targetSkeletonData.DefaultSkin;
			Skin skin = preview.Skeleton.Skin ?? defaultSkin;

			using (new SpineInspectorUtility.IndentScope()) {

				using (new EditorGUILayout.HorizontalScope()) {
					showAttachments = EditorGUILayout.ToggleLeft("Show Attachments", showAttachments, GUILayout.MaxWidth(150f));
					if (showAttachments) {
						if (skin != null) {
							int attachmentCount = skin.Attachments.Count;
							EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(string.Format("{0} ({1} attachment{2})", skin.Name, attachmentCount, SpineInspectorUtility.PluralThenS(attachmentCount)), Icons.skin));
						}

					}
				}

				var slotAttachments = new List<Skin.SkinEntry>();
				var defaultSkinAttachments = new List<Skin.SkinEntry>();
				var slotsItems = preview.Skeleton.Slots.Items;
				for (int i = preview.Skeleton.Slots.Count - 1; i >= 0; i--) {
					Slot slot = slotsItems[i];
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(slot.Data.Name, Icons.slot));
					if (showAttachments) {
						slotAttachments.Clear();
						defaultSkinAttachments.Clear();

						using (new SpineInspectorUtility.IndentScope()) {
							{
								skin.GetAttachments(i, slotAttachments);
								if (defaultSkin != null) {
									if (skin != defaultSkin) {
										defaultSkin.GetAttachments(i, slotAttachments);
										defaultSkin.GetAttachments(i, defaultSkinAttachments);
									} else {
										defaultSkin.GetAttachments(i, defaultSkinAttachments);
									}
								}
							}

							for (int a = 0; a < slotAttachments.Count; a++) {
								var skinEntry = slotAttachments[a];
								Attachment attachment = skinEntry.Attachment;
								string attachmentName = skinEntry.Name;
								bool attachmentIsFromSkin = !defaultSkinAttachments.Contains(skinEntry);

								Texture2D attachmentTypeIcon = Icons.GetAttachmentIcon(attachment);
								bool initialState = slot.Attachment == attachment;

								Texture2D iconToUse = attachmentIsFromSkin ? Icons.skinPlaceholder : attachmentTypeIcon;
								bool toggled = EditorGUILayout.ToggleLeft(SpineInspectorUtility.TempContent(attachmentName, iconToUse), slot.Attachment == attachment, GUILayout.MinWidth(150f));

								if (attachmentIsFromSkin) {
									Rect extraIconRect = GUILayoutUtility.GetLastRect();
									extraIconRect.x += extraIconRect.width - (attachmentTypeIcon.width * 2f);
									extraIconRect.width = attachmentTypeIcon.width;
									extraIconRect.height = attachmentTypeIcon.height;
									GUI.DrawTexture(extraIconRect, attachmentTypeIcon);
								}

								if (toggled != initialState) {
									slot.Attachment = toggled ? attachment : null;
									preview.RefreshOnNextUpdate();
								}
							}
						}

					}
				}
			}

		}

		void DrawUnityTools () {
#if SPINE_SKELETON_MECANIM
			using (new SpineInspectorUtility.BoxScope()) {
				isMecanimExpanded = EditorGUILayout.Foldout(isMecanimExpanded, SpineInspectorUtility.TempContent("SkeletonMecanim", SpineInspectorUtility.UnityIcon<SceneAsset>()));
				if (isMecanimExpanded) {
					using (new SpineInspectorUtility.IndentScope()) {
						EditorGUILayout.PropertyField(controller, SpineInspectorUtility.TempContent("Controller", SpineInspectorUtility.UnityIcon<Animator>()));
						if (controller.objectReferenceValue == null) {

							// Generate Mecanim Controller Button
							using (new GUILayout.HorizontalScope()) {
								GUILayout.Space(EditorGUIUtility.labelWidth);
								if (GUILayout.Button(SpineInspectorUtility.TempContent("Generate Mecanim Controller"), GUILayout.Height(20)))
									SkeletonBaker.GenerateMecanimAnimationClips(targetSkeletonDataAsset);
							}
							EditorGUILayout.HelpBox("SkeletonMecanim is the Mecanim alternative to SkeletonAnimation.\nIt is not required.", MessageType.Info);

						} else {

							// Update AnimationClips button.
							using (new GUILayout.HorizontalScope()) {
								GUILayout.Space(EditorGUIUtility.labelWidth);
								if (GUILayout.Button(SpineInspectorUtility.TempContent("Force Update AnimationClips"), GUILayout.Height(20)))
									SkeletonBaker.GenerateMecanimAnimationClips(targetSkeletonDataAsset);
							}

						}
					}
				}
			}
#endif
		}

		void DrawWarningList () {
			foreach (string line in warnings)
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(line, Icons.warning));
		}

		void PopulateWarnings () {
			warnings.Clear();
			compatibilityProblemInfo = null;

			if (skeletonJSON.objectReferenceValue == null) {
				warnings.Add("Missing Skeleton JSON");
			} else {
				var fieldValue = (TextAsset)skeletonJSON.objectReferenceValue;
				string problemDescription = null;
				if (!AssetUtility.IsSpineData(fieldValue, out compatibilityProblemInfo, ref problemDescription)) {
					if (problemDescription != null)
						warnings.Add(problemDescription);
					else
						warnings.Add("Skeleton data file is not a valid Spine JSON or binary file.");
				} else {
#if SPINE_TK2D
					bool searchForSpineAtlasAssets = (compatibilityProblemInfo == null);
					bool isSpriteCollectionNull = spriteCollection.objectReferenceValue == null;
					if (!isSpriteCollectionNull) searchForSpineAtlasAssets = false;
#else
					// Analysis disable once ConvertToConstant.Local
					bool searchForSpineAtlasAssets = (compatibilityProblemInfo == null);
#endif

					if (searchForSpineAtlasAssets) {
						bool detectedNullAtlasEntry = false;
						var atlasList = new List<Atlas>();
						var actualAtlasAssets = targetSkeletonDataAsset.atlasAssets;

						for (int i = 0; i < actualAtlasAssets.Length; i++) {
							if (actualAtlasAssets[i] == null) {
								detectedNullAtlasEntry = true;
								break;
							} else {
								if (actualAtlasAssets[i].MaterialCount > 0)
									atlasList.Add(actualAtlasAssets[i].GetAtlas());
							}
						}

						if (detectedNullAtlasEntry) {
							warnings.Add("AtlasAsset elements should not be null.");
						} else {
							List<string> missingPaths = null;
							if (atlasAssets.arraySize > 0) {
								missingPaths = AssetUtility.GetRequiredAtlasRegions(AssetDatabase.GetAssetPath(skeletonJSON.objectReferenceValue));
								foreach (var atlas in atlasList) {
									if (atlas == null)
										continue;
									for (int i = 0; i < missingPaths.Count; i++) {
										if (atlas.FindRegionIgnoringNumberSuffix(missingPaths[i]) != null) {
											missingPaths.RemoveAt(i);
											i--;
										}
									}
								}

#if SPINE_TK2D
								if (missingPaths.Count > 0)
									warnings.Add("Missing regions. SkeletonData asset requires tk2DSpriteCollectionData or Spine AtlasAssets.");
#endif
							}

							if (missingPaths != null) {
								foreach (string missingRegion in missingPaths)
									warnings.Add(string.Format("Missing Region: '{0}'", missingRegion));
							}

						}
					}

				}
			}
		}

		void DoReimport () {
			AssetUtility.ImportSpineContent(new[] { AssetDatabase.GetAssetPath(skeletonJSON.objectReferenceValue) }, null, true);
			preview.Clear();
			InitializeEditor();
			EditorUtility.SetDirty(targetSkeletonDataAsset);
		}

		void HandlePreviewSkinChanged (string skinName) {
			EditorPrefs.SetString(LastSkinKey, skinName);
		}

		bool NoProblems () {
			return warnings.Count == 0 && compatibilityProblemInfo == null;
		}

		#region Preview Handlers
		void HandleOnDestroyPreview () {
			EditorApplication.update -= preview.HandleEditorUpdate;
			preview.OnDestroy();
		}

		override public bool HasPreviewGUI () {
			if (serializedObject.isEditingMultipleObjects)
				return false;

			for (int i = 0; i < atlasAssets.arraySize; i++) {
				var prop = atlasAssets.GetArrayElementAtIndex(i);
				if (prop.objectReferenceValue == null)
					return false;
			}

			return skeletonJSON.objectReferenceValue != null;
		}

		override public void OnInteractivePreviewGUI (Rect r, GUIStyle background) {
			if (NoProblems()) {
				preview.Initialize(this.Repaint, targetSkeletonDataAsset, this.LastSkinName);
				preview.HandleInteractivePreviewGUI(r, background);
			}
		}

		override public GUIContent GetPreviewTitle () { return SpineInspectorUtility.TempContent("Preview"); }
		public override void OnPreviewSettings () { preview.HandleDrawSettings(); }
		public override Texture2D RenderStaticPreview (string assetPath, UnityEngine.Object[] subAssets, int width, int height) { return preview.GetStaticPreview(width, height); }
		#endregion
	}

	internal class SkeletonInspectorPreview {
		Color OriginColor = new Color(0.3f, 0.3f, 0.3f, 1);
		static readonly int SliderHash = "Slider".GetHashCode();

		SkeletonDataAsset skeletonDataAsset;
		SkeletonData skeletonData;

		SkeletonAnimation skeletonAnimation;
		GameObject previewGameObject;
		internal bool requiresRefresh;
		float animationLastTime;

		static float CurrentTime { get { return (float)EditorApplication.timeSinceStartup; } }

		Action Repaint;
		public event Action<string> OnSkinChanged;

		Texture previewTexture;
		PreviewRenderUtility previewRenderUtility;
		Camera PreviewUtilityCamera {
			get {
				if (previewRenderUtility == null) return null;
#if UNITY_2017_1_OR_NEWER
				return previewRenderUtility.camera;
#else
				return previewRenderUtility.m_Camera;
#endif
			}
		}

		static Vector3 lastCameraPositionGoal;
		static float lastCameraOrthoGoal;
		float cameraOrthoGoal = 1;
		Vector3 cameraPositionGoal = new Vector3(0, 0, -10);
		double cameraAdjustEndFrame = 0;

		List<Spine.Event> currentAnimationEvents = new List<Spine.Event>();
		List<float> currentAnimationEventTimes = new List<float>();
		List<SpineEventTooltip> currentAnimationEventTooltips = new List<SpineEventTooltip>();

		public bool IsValid { get { return skeletonAnimation != null && skeletonAnimation.valid; } }

		public Skeleton Skeleton { get { return IsValid ? skeletonAnimation.Skeleton : null; } }

		public float TimeScale {
			get { return IsValid ? skeletonAnimation.timeScale : 1f; }
			set { if (IsValid) skeletonAnimation.timeScale = value; }
		}

		public bool IsPlayingAnimation {
			get {
				if (!IsValid) return false;
				var currentTrack = skeletonAnimation.AnimationState.GetCurrent(0);
				return currentTrack != null && currentTrack.TimeScale > 0;
			}
		}

		public TrackEntry ActiveTrack { get { return IsValid ? skeletonAnimation.AnimationState.GetCurrent(0) : null; } }

		public Vector3 PreviewCameraPosition {
			get { return PreviewUtilityCamera.transform.position; }
			set { PreviewUtilityCamera.transform.position = value; }
		}

		public void HandleDrawSettings () {
			const float SliderWidth = 150;
			const float SliderSnap = 0.25f;
			const float SliderMin = 0f;
			const float SliderMax = 2f;

			if (IsValid) {
				float timeScale = GUILayout.HorizontalSlider(TimeScale, SliderMin, SliderMax, GUILayout.MaxWidth(SliderWidth));
				timeScale = Mathf.RoundToInt(timeScale / SliderSnap) * SliderSnap;
				TimeScale = timeScale;
			}
		}

		public void HandleEditorUpdate () {
			AdjustCamera();
			if (IsPlayingAnimation) {
				RefreshOnNextUpdate();
				Repaint();
			} else if (requiresRefresh) {
				Repaint();
			}
		}

		public void Initialize (Action repaintCallback, SkeletonDataAsset skeletonDataAsset, string skinName = "") {
			if (skeletonDataAsset == null) return;
			if (skeletonDataAsset.GetSkeletonData(false) == null) {
				DestroyPreviewGameObject();
				return;
			}

			this.Repaint = repaintCallback;
			this.skeletonDataAsset = skeletonDataAsset;
			this.skeletonData = skeletonDataAsset.GetSkeletonData(false);

			if (skeletonData == null) {
				DestroyPreviewGameObject();
				return;
			}

			const int PreviewLayer = 30;
			const int PreviewCameraCullingMask = 1 << PreviewLayer;

			if (previewRenderUtility == null) {
				previewRenderUtility = new PreviewRenderUtility(true);
				animationLastTime = CurrentTime;

				{
					var c = this.PreviewUtilityCamera;
					c.orthographic = true;
					c.cullingMask = PreviewCameraCullingMask;
					c.nearClipPlane = 0.01f;
					c.farClipPlane = 1000f;
					c.orthographicSize = lastCameraOrthoGoal;
					c.transform.position = lastCameraPositionGoal;
				}

				DestroyPreviewGameObject();
			}

			if (previewGameObject == null) {
				try {
					previewGameObject = EditorInstantiation.InstantiateSkeletonAnimation(skeletonDataAsset, skinName, useObjectFactory: false).gameObject;

					if (previewGameObject != null) {
						previewGameObject.hideFlags = HideFlags.HideAndDontSave;
						previewGameObject.layer = PreviewLayer;
						skeletonAnimation = previewGameObject.GetComponent<SkeletonAnimation>();
						skeletonAnimation.initialSkinName = skinName;
						skeletonAnimation.LateUpdate();
						previewGameObject.GetComponent<Renderer>().enabled = false;

#if SPINE_UNITY_2018_PREVIEW_API
						previewRenderUtility.AddSingleGO(previewGameObject);
#endif
					}

					if (this.ActiveTrack != null) cameraAdjustEndFrame = EditorApplication.timeSinceStartup + skeletonAnimation.AnimationState.GetCurrent(0).Alpha;
					AdjustCameraGoals();
				} catch {
					DestroyPreviewGameObject();
				}

				RefreshOnNextUpdate();
			}
		}

		public void HandleInteractivePreviewGUI (Rect r, GUIStyle background) {
			if (Event.current.type == EventType.Repaint) {
				if (requiresRefresh) {
					previewRenderUtility.BeginPreview(r, background);
					DoRenderPreview(true);
					previewTexture = previewRenderUtility.EndPreview();
					requiresRefresh = false;
				}
				if (previewTexture != null)
					GUI.DrawTexture(r, previewTexture, ScaleMode.StretchToFill, false);
			}

			DrawSkinToolbar(r);
			//DrawSetupPoseButton(r);
			DrawTimeBar(r);
			HandleMouseScroll(r);
		}

		public Texture2D GetStaticPreview (int width, int height) {
			var c = this.PreviewUtilityCamera;
			if (c == null)
				return null;

			RefreshOnNextUpdate();
			AdjustCameraGoals();
			c.orthographicSize = cameraOrthoGoal / 2;
			c.transform.position = cameraPositionGoal;
			previewRenderUtility.BeginStaticPreview(new Rect(0, 0, width, height));
			DoRenderPreview(false);
			var tex = previewRenderUtility.EndStaticPreview();

			return tex;
		}

		public void DoRenderPreview (bool drawHandles) {
			if (this.PreviewUtilityCamera.activeTexture == null || this.PreviewUtilityCamera.targetTexture == null)
				return;

			GameObject go = previewGameObject;
			if (requiresRefresh && go != null) {
				var renderer = go.GetComponent<Renderer>();
				renderer.enabled = true;


				if (!EditorApplication.isPlaying) {
					float current = CurrentTime;
					float deltaTime = (current - animationLastTime);
					skeletonAnimation.Update(deltaTime);
					animationLastTime = current;
					skeletonAnimation.LateUpdate();
				}

				var thisPreviewUtilityCamera = this.PreviewUtilityCamera;

				if (drawHandles) {
					Handles.SetCamera(thisPreviewUtilityCamera);
					Handles.color = OriginColor;

					// Draw Cross
					float scale = skeletonDataAsset.scale;
					float cl = 1000 * scale;
					Handles.DrawLine(new Vector3(-cl, 0), new Vector3(cl, 0));
					Handles.DrawLine(new Vector3(0, cl), new Vector3(0, -cl));
				}

				thisPreviewUtilityCamera.Render();

				if (drawHandles) {
					Handles.SetCamera(thisPreviewUtilityCamera);
					SpineHandles.DrawBoundingBoxes(skeletonAnimation.transform, skeletonAnimation.skeleton);
					if (SkeletonDataAssetInspector.showAttachments)
						SpineHandles.DrawPaths(skeletonAnimation.transform, skeletonAnimation.skeleton);
				}

				renderer.enabled = false;
			}
		}

		public void AdjustCamera () {
			if (previewRenderUtility == null)
				return;

			if (CurrentTime < cameraAdjustEndFrame)
				AdjustCameraGoals();

			lastCameraPositionGoal = cameraPositionGoal;
			lastCameraOrthoGoal = cameraOrthoGoal;

			var c = this.PreviewUtilityCamera;
			float orthoSet = Mathf.Lerp(c.orthographicSize, cameraOrthoGoal, 0.1f);

			c.orthographicSize = orthoSet;

			float dist = Vector3.Distance(c.transform.position, cameraPositionGoal);
			if (dist > 0f) {
				Vector3 pos = Vector3.Lerp(c.transform.position, cameraPositionGoal, 0.1f);
				pos.x = 0;
				c.transform.position = pos;
				c.transform.rotation = Quaternion.identity;
				RefreshOnNextUpdate();
			}
		}

		void AdjustCameraGoals () {
			if (previewGameObject == null) return;

			Bounds bounds = previewGameObject.GetComponent<Renderer>().bounds;
			cameraOrthoGoal = bounds.size.y;
			cameraPositionGoal = bounds.center + new Vector3(0, 0, -10f);
		}

		void HandleMouseScroll (Rect position) {
			Event current = Event.current;
			int controlID = GUIUtility.GetControlID(SliderHash, FocusType.Passive);
			switch (current.GetTypeForControl(controlID)) {
			case EventType.ScrollWheel:
				if (position.Contains(current.mousePosition)) {
					cameraOrthoGoal += current.delta.y * 0.06f;
					cameraOrthoGoal = Mathf.Max(0.01f, cameraOrthoGoal);
					GUIUtility.hotControl = controlID;
					current.Use();
				}
				break;
			}
		}

		public void RefreshOnNextUpdate () {
			requiresRefresh = true;
		}

		public void ClearAnimationSetupPose () {
			if (skeletonAnimation == null) {
				Debug.LogWarning("Animation was stopped but preview doesn't exist. It's possible that the Preview Panel is closed.");
			}

			skeletonAnimation.AnimationState.ClearTracks();
			skeletonAnimation.Skeleton.SetToSetupPose();
		}

		public void PlayPauseAnimation (string animationName, bool loop) {
			if (skeletonData == null) return;

			if (skeletonAnimation == null) {
				//Debug.LogWarning("Animation was stopped but preview doesn't exist. It's possible that the Preview Panel is closed.");
				return;
			}

			if (!skeletonAnimation.valid) return;

			if (string.IsNullOrEmpty(animationName)) {
				skeletonAnimation.Skeleton.SetToSetupPose();
				skeletonAnimation.AnimationState.ClearTracks();
				return;
			}

			var targetAnimation = skeletonData.FindAnimation(animationName);
			if (targetAnimation != null) {
				var currentTrack = this.ActiveTrack;
				bool isEmpty = (currentTrack == null);
				bool isNewAnimation = isEmpty || currentTrack.Animation != targetAnimation;

				var skeleton = skeletonAnimation.Skeleton;
				var animationState = skeletonAnimation.AnimationState;

				if (isEmpty) {
					skeleton.SetToSetupPose();
					animationState.SetAnimation(0, targetAnimation, loop);
				} else {
					bool sameAnimation = (currentTrack.Animation == targetAnimation);
					if (sameAnimation) {
						currentTrack.TimeScale = (currentTrack.TimeScale == 0) ? 1f : 0f; // pause/play
					} else {
						currentTrack.TimeScale = 1f;
						animationState.SetAnimation(0, targetAnimation, loop);
					}
				}

				if (isNewAnimation) {
					currentAnimationEvents.Clear();
					currentAnimationEventTimes.Clear();
					foreach (Timeline timeline in targetAnimation.Timelines) {
						var eventTimeline = timeline as EventTimeline;
						if (eventTimeline != null) {
							for (int i = 0; i < eventTimeline.Events.Length; i++) {
								currentAnimationEvents.Add(eventTimeline.Events[i]);
								currentAnimationEventTimes.Add(eventTimeline.Frames[i]);
							}
						}
					}
				}
			} else {
				Debug.LogFormat("The Spine.Animation named '{0}' was not found for this Skeleton.", animationName);
			}

		}

		void DrawSkinToolbar (Rect r) {
			if (!this.IsValid) return;

			var skeleton = this.Skeleton;
			string label = (skeleton.Skin != null) ? skeleton.Skin.Name : "default";

			Rect popRect = new Rect(r);
			popRect.y += 32;
			popRect.x += 4;
			popRect.height = 24;
			popRect.width = 40;
			EditorGUI.DropShadowLabel(popRect, SpineInspectorUtility.TempContent("Skin"));

			popRect.y += 11;
			popRect.width = 150;
			popRect.x += 44;

			if (GUI.Button(popRect, SpineInspectorUtility.TempContent(label, Icons.skin), EditorStyles.popup)) {
				DrawSkinDropdown();
			}
		}

		void DrawSetupPoseButton (Rect r) {
			if (!this.IsValid)
				return;

			var skeleton = this.Skeleton;

			Rect popRect = new Rect(r);
			popRect.y += 64;
			popRect.x += 4;
			popRect.height = 24;
			popRect.width = 40;

			//popRect.y += 11;
			popRect.width = 150;
			//popRect.x += 44;

			if (GUI.Button(popRect, SpineInspectorUtility.TempContent("Reset to SetupPose", Icons.skeleton))) {
				ClearAnimationSetupPose();
				RefreshOnNextUpdate();
			}
		}

		void DrawSkinDropdown () {
			var menu = new GenericMenu();
			foreach (Skin s in skeletonData.Skins)
				menu.AddItem(new GUIContent(s.Name, Icons.skin), skeletonAnimation.skeleton.Skin == s, HandleSkinDropdownSelection, s);

			menu.ShowAsContext();
		}

		void HandleSkinDropdownSelection (object o) {
			Skin skin = (Skin)o;
			skeletonAnimation.initialSkinName = skin.Name;
			skeletonAnimation.Initialize(true);
			RefreshOnNextUpdate();
			if (OnSkinChanged != null) OnSkinChanged(skin.Name);
		}

		void DrawTimeBar (Rect r) {
			if (skeletonAnimation == null)
				return;

			Rect barRect = new Rect(r);
			barRect.height = 32;
			barRect.x += 4;
			barRect.width -= 4;

			GUI.Box(barRect, "");

			Rect lineRect = new Rect(barRect);
			float lineRectWidth = lineRect.width;
			TrackEntry t = skeletonAnimation.AnimationState.GetCurrent(0);

			if (t != null && Icons.userEvent != null) { // when changing to play mode, Icons.userEvent  will not be reset
				int loopCount = (int)(t.TrackTime / t.TrackEnd);
				float currentTime = t.TrackTime - (t.TrackEnd * loopCount);
				float normalizedTime = currentTime / t.Animation.Duration;
				float wrappedTime = normalizedTime % 1f;

				lineRect.x = barRect.x + (lineRectWidth * wrappedTime) - 0.5f;
				lineRect.width = 2;

				GUI.color = Color.red;
				GUI.DrawTexture(lineRect, EditorGUIUtility.whiteTexture);
				GUI.color = Color.white;

				currentAnimationEventTooltips = currentAnimationEventTooltips ?? new List<SpineEventTooltip>();
				currentAnimationEventTooltips.Clear();
				for (int i = 0; i < currentAnimationEvents.Count; i++) {
					float eventTime = currentAnimationEventTimes[i];
					var userEventIcon = Icons.userEvent;
					float iconX = Mathf.Max(((eventTime / t.Animation.Duration) * lineRectWidth) - (userEventIcon.width / 2), barRect.x);
					float iconY = barRect.y + userEventIcon.height;
					var evRect = new Rect(barRect) {
						x = iconX,
						y = iconY,
						width = userEventIcon.width,
						height = userEventIcon.height
					};
					GUI.DrawTexture(evRect, userEventIcon);
					Event ev = Event.current;
					if (ev.type == EventType.Repaint) {
						if (evRect.Contains(ev.mousePosition)) {
							string eventName = currentAnimationEvents[i].Data.Name;
							Rect tooltipRect = new Rect(evRect) {
								width = EditorStyles.helpBox.CalcSize(new GUIContent(eventName)).x
							};
							tooltipRect.y -= 4;
							tooltipRect.y -= tooltipRect.height * currentAnimationEventTooltips.Count; // Avoid several overlapping tooltips.
							tooltipRect.x += 4;

							// Handle tooltip overflowing to the right.
							float rightEdgeOverflow = (tooltipRect.x + tooltipRect.width) - (barRect.x + barRect.width);
							if (rightEdgeOverflow > 0)
								tooltipRect.x -= rightEdgeOverflow;

							currentAnimationEventTooltips.Add(new SpineEventTooltip { rect = tooltipRect, text = eventName });
						}
					}
				}

				// Draw tooltips.
				for (int i = 0; i < currentAnimationEventTooltips.Count; i++) {
					GUI.Label(currentAnimationEventTooltips[i].rect, currentAnimationEventTooltips[i].text, EditorStyles.helpBox);
					GUI.tooltip = currentAnimationEventTooltips[i].text;
				}
			}
		}

		public void OnDestroy () {
			DisposePreviewRenderUtility();
			DestroyPreviewGameObject();
		}

		public void Clear () {
			DisposePreviewRenderUtility();
			DestroyPreviewGameObject();
		}

		void DisposePreviewRenderUtility () {
			if (previewRenderUtility != null) {
				previewRenderUtility.Cleanup();
				previewRenderUtility = null;
			}
		}

		void DestroyPreviewGameObject () {
			if (previewGameObject != null) {
				GameObject.DestroyImmediate(previewGameObject);
				previewGameObject = null;
			}
		}

		internal struct SpineEventTooltip {
			public Rect rect;
			public string text;
		}
	}

}
