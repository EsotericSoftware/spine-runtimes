/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#define SPINE_SKELETON_ANIMATOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Spine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;
	using Icons = SpineEditorUtilities.Icons;
	using Animation = Spine.Animation;

	[CustomEditor(typeof(SkeletonDataAsset)), CanEditMultipleObjects]
	public class SkeletonDataAssetInspector : UnityEditor.Editor {
		internal static bool showAnimationStateData = true;
		internal static bool showAnimationList = true;
		internal static bool showSlotList = false;
		internal static bool showAttachments = false;

		SerializedProperty atlasAssets, skeletonJSON, scale, fromAnimation, toAnimation, duration, defaultMix;
		#if SPINE_TK2D
		SerializedProperty spriteCollection;
		#endif

		#if SPINE_SKELETON_ANIMATOR
		static bool isMecanimExpanded = false;
		SerializedProperty controller;
		#endif

		SkeletonDataAsset targetSkeletonDataAsset;
		SkeletonData targetSkeletonData;
		string targetSkeletonDataAssetGUIDString;

		readonly List<string> warnings = new List<string>();
		readonly SkeletonInspectorPreview preview = new SkeletonInspectorPreview();

		GUIStyle activePlayButtonStyle, idlePlayButtonStyle;
		readonly GUIContent DefaultMixLabel = new GUIContent("Default Mix Duration", "Sets 'SkeletonDataAsset.defaultMix' in the asset and 'AnimationState.data.defaultMix' at runtime load time.");

		string LastSkinKey { get { return targetSkeletonDataAssetGUIDString + "_lastSkin"; } }
		string LastSkinName { get { return EditorPrefs.GetString(LastSkinKey, ""); } }

		void OnEnable () {
			InitializeEditor();
		}

		void InitializeEditor () {
			SpineEditorUtilities.ConfirmInitialization();
			targetSkeletonDataAsset = (SkeletonDataAsset)target;
			targetSkeletonDataAssetGUIDString = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(targetSkeletonDataAsset));

			bool newAtlasAssets = atlasAssets == null;
			if (newAtlasAssets) atlasAssets = serializedObject.FindProperty("atlasAssets");
			skeletonJSON = serializedObject.FindProperty("skeletonJSON");
			scale = serializedObject.FindProperty("scale");
			fromAnimation = serializedObject.FindProperty("fromAnimation");
			toAnimation = serializedObject.FindProperty("toAnimation");
			duration = serializedObject.FindProperty("duration");
			defaultMix = serializedObject.FindProperty("defaultMix");

			#if SPINE_SKELETON_ANIMATOR
			controller = serializedObject.FindProperty("controller");
			#endif

			#if SPINE_TK2D
			if (newAtlasAssets) atlasAssets.isExpanded = false;
			spriteCollection = serializedObject.FindProperty("spriteCollection");
			#else
			// Analysis disable once ConvertIfToOrExpression
			if (newAtlasAssets) atlasAssets.isExpanded = true;
			#endif

			EditorApplication.update -= EditorUpdate;
			EditorApplication.update += EditorUpdate;
			preview.OnSkinChanged -= HandlePreviewSkinChanged;
			preview.OnSkinChanged += HandlePreviewSkinChanged;

			PopulateWarnings();
			if (targetSkeletonDataAsset.skeletonJSON == null) {
				targetSkeletonData = null;
				return;	
			}

			targetSkeletonData = warnings.Count == 0 ? targetSkeletonDataAsset.GetSkeletonData(false) : null;

			if (targetSkeletonData != null && warnings.Count <= 0) {
				preview.Initialize(targetSkeletonDataAsset, this.LastSkinName);
			}
				
		}

		void EditorUpdate () {
			preview.AdjustCamera();

			if (preview.IsPlayingAnimation) {
				preview.RefreshOnNextUpdate();
				Repaint();
			} else if (preview.requiresRefresh) {
				Repaint();
			} // else // needed if using smooth menus

		}

		void Clear () {
			preview.Clear();
			targetSkeletonDataAsset.Clear();
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

				using (new SpineInspectorUtility.BoxScope()) {
					DrawAtlasAssetsFields();
					HandleAtlasAssetsNulls();
				}

				if (changeCheck.changed) {
					if (serializedObject.ApplyModifiedProperties()) {
						this.Clear();
						this.InitializeEditor();
						return;
					}
				}
			}

			// Unity Quirk: Some code depends on valid preview. If preview is initialized elsewhere, this can cause contents to change between Layout and Repaint events, causing GUILayout control count errors.
			if (warnings.Count <= 0)
				preview.Initialize(targetSkeletonDataAsset, this.LastSkinName);

			if (targetSkeletonData != null) {
				GUILayout.Space(20f);

				using (new SpineInspectorUtility.BoxScope(false)) {
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Mix Settings", Icons.animationRoot), EditorStyles.boldLabel);
					DrawAnimationStateInfo();
					EditorGUILayout.Space();
				}

				EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
				DrawAnimationList();
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

		void OnInspectorGUIMulti () {
			
			// Skeleton data file field.
			using (new SpineInspectorUtility.BoxScope()) {
				EditorGUILayout.LabelField("SkeletonData", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(skeletonJSON, SpineInspectorUtility.TempContent(skeletonJSON.displayName, Icons.spine));
				EditorGUILayout.PropertyField(scale);
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
			EditorGUILayout.PropertyField(scale);
		}

		void DrawAtlasAssetsFields () {
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
				}
				else {
					EditorGUILayout.HelpBox("Atlas array should not have null entries!", MessageType.Error);
					if (SpineInspectorUtility.CenteredButton(SpineInspectorUtility.TempContent("Remove null entries"))) {
						var trimmedAtlasAssets = new List<AtlasAsset>();
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

				// Do not use EditorGUIUtility.indentLevel. It will add spaces on every field.
				for (int i = 0; i < fromAnimation.arraySize; i++) {
					SerializedProperty from = fromAnimation.GetArrayElementAtIndex(i);
					SerializedProperty to = toAnimation.GetArrayElementAtIndex(i);
					SerializedProperty durationProp = duration.GetArrayElementAtIndex(i);
					using (new EditorGUILayout.HorizontalScope()) {
						GUILayout.Space(16f);
						EditorGUILayout.PropertyField(from, GUIContent.none);
						EditorGUILayout.PropertyField(to, GUIContent.none);
						durationProp.floatValue = EditorGUILayout.FloatField(durationProp.floatValue, GUILayout.MinWidth(25f), GUILayout.MaxWidth(60f));
						if (GUILayout.Button("Delete", EditorStyles.miniButton)) {
							duration.DeleteArrayElementAtIndex(i);
							toAnimation.DeleteArrayElementAtIndex(i);
							fromAnimation.DeleteArrayElementAtIndex(i);
						}
					}
				}

				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					if (GUILayout.Button("Add Mix")) {
						duration.arraySize++;
						toAnimation.arraySize++;
						fromAnimation.arraySize++;
					}
					EditorGUILayout.Space();
				}

				if (cc.changed) {
					targetSkeletonDataAsset.FillStateData();
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
			bool nonessential = targetSkeletonData.ImagesPath != null; // Currently the only way to determine if skeleton data has nonessential data. (Spine 3.6)
			float fps = targetSkeletonData.Fps;
			if (nonessential && fps == 0) fps = 30;

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
					string frameCountString = (fps > 0) ? ("(" + (Mathf.RoundToInt(animation.Duration * fps)) + ")").PadLeft(12, ' ') : string.Empty;
					EditorGUILayout.LabelField(new GUIContent(animation.Name, Icons.animation), SpineInspectorUtility.TempContent(animation.Duration.ToString("f3") + "s" + frameCountString));
				}
			}
		}

		void DrawSlotList () {
			showSlotList = EditorGUILayout.Foldout(showSlotList, SpineInspectorUtility.TempContent("Slots", Icons.slotRoot));

			if (!showSlotList) return;
			if (!preview.IsValid) return;

			EditorGUI.indentLevel++;
			showAttachments = EditorGUILayout.ToggleLeft("Show Attachments", showAttachments);
			var slotAttachments = new List<Attachment>();
			var slotAttachmentNames = new List<string>();
			var defaultSkinAttachmentNames = new List<string>();
			var defaultSkin = targetSkeletonData.Skins.Items[0];
			Skin skin = preview.Skeleton.Skin ?? defaultSkin;
			var slotsItems = preview.Skeleton.Slots.Items;

			for (int i = preview.Skeleton.Slots.Count - 1; i >= 0; i--) {
				Slot slot = slotsItems[i];
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(slot.Data.Name, Icons.slot));
				if (showAttachments) {
					
					EditorGUI.indentLevel++;
					slotAttachments.Clear();
					slotAttachmentNames.Clear();
					defaultSkinAttachmentNames.Clear();

					skin.FindNamesForSlot(i, slotAttachmentNames);
					skin.FindAttachmentsForSlot(i, slotAttachments);

					if (skin != defaultSkin) {
						defaultSkin.FindNamesForSlot(i, defaultSkinAttachmentNames);
						defaultSkin.FindNamesForSlot(i, slotAttachmentNames);
						defaultSkin.FindAttachmentsForSlot(i, slotAttachments);
					} else {
						defaultSkin.FindNamesForSlot(i, defaultSkinAttachmentNames);
					}

					for (int a = 0; a < slotAttachments.Count; a++) {
						Attachment attachment = slotAttachments[a];
						string attachmentName = slotAttachmentNames[a];
						Texture2D icon = Icons.GetAttachmentIcon(attachment);
						bool initialState = slot.Attachment == attachment;
						bool toggled = EditorGUILayout.ToggleLeft(SpineInspectorUtility.TempContent(attachmentName, icon), slot.Attachment == attachment);

						if (!defaultSkinAttachmentNames.Contains(attachmentName)) {
							Rect skinPlaceHolderIconRect = GUILayoutUtility.GetLastRect();
							skinPlaceHolderIconRect.width = Icons.skinPlaceholder.width;
							skinPlaceHolderIconRect.height = Icons.skinPlaceholder.height;
							GUI.DrawTexture(skinPlaceHolderIconRect, Icons.skinPlaceholder);
						}

						if (toggled != initialState) {
							slot.Attachment = toggled ? attachment : null;
							preview.RefreshOnNextUpdate();
						}
					}
					EditorGUI.indentLevel--;
				}
			}
			EditorGUI.indentLevel--;
		}

		void DrawUnityTools () {
			#if SPINE_SKELETON_ANIMATOR
			using (new SpineInspectorUtility.BoxScope()) {
				isMecanimExpanded = EditorGUILayout.Foldout(isMecanimExpanded, SpineInspectorUtility.TempContent("SkeletonAnimator", SpineInspectorUtility.UnityIcon<SceneAsset>()));
				if (isMecanimExpanded) {
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(controller, SpineInspectorUtility.TempContent("Controller", SpineInspectorUtility.UnityIcon<Animator>()));		
					if (controller.objectReferenceValue == null) {

						// Generate Mecanim Controller Button
						using (new GUILayout.HorizontalScope()) {
							GUILayout.Space(EditorGUIUtility.labelWidth);
							if (GUILayout.Button(SpineInspectorUtility.TempContent("Generate Mecanim Controller"), GUILayout.Height(20)))
								SkeletonBaker.GenerateMecanimAnimationClips(targetSkeletonDataAsset);						
						}
						EditorGUILayout.HelpBox("SkeletonAnimator is the Mecanim alternative to SkeletonAnimation.\nIt is not required.", MessageType.Info);

					} else {

						// Update AnimationClips button.
						using (new GUILayout.HorizontalScope()) {
							GUILayout.Space(EditorGUIUtility.labelWidth);
							if (GUILayout.Button(SpineInspectorUtility.TempContent("Force Update AnimationClips"), GUILayout.Height(20)))
								SkeletonBaker.GenerateMecanimAnimationClips(targetSkeletonDataAsset);				
						}

					}
					EditorGUI.indentLevel--;
				}
			}
			#endif
		}

		void DrawWarningList () {
			foreach (var line in warnings)
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(line, Icons.warning));
		}

		void PopulateWarnings () {
			warnings.Clear();

			if (skeletonJSON.objectReferenceValue == null) {
				warnings.Add("Missing Skeleton JSON");
			} else {
				var fieldValue = (TextAsset)skeletonJSON.objectReferenceValue;
				if (!SpineEditorUtilities.IsSpineData(fieldValue)) {
					warnings.Add("Skeleton data file is not a valid JSON or binary file.");
				} else {
					#if SPINE_TK2D
					bool searchForSpineAtlasAssets = true;
					bool isSpriteCollectionNull = spriteCollection.objectReferenceValue == null;
					if (!isSpriteCollectionNull) searchForSpineAtlasAssets = false;
					#else
					// Analysis disable once ConvertToConstant.Local
					bool searchForSpineAtlasAssets = true;
					#endif

					if (searchForSpineAtlasAssets) {
						bool detectedNullAtlasEntry = false;
						var atlasList = new List<Atlas>();
						var actualAtlasAssets = targetSkeletonDataAsset.atlasAssets;

						for (int i = 0; i < actualAtlasAssets.Length; i++) {
							if (targetSkeletonDataAsset.atlasAssets[i] == null) {
								detectedNullAtlasEntry = true;
								break;
							} else {
								atlasList.Add(actualAtlasAssets[i].GetAtlas());
							}
						}

						if (detectedNullAtlasEntry) {
							warnings.Add("AtlasAsset elements should not be null.");
						} else {
							var missingPaths = SpineEditorUtilities.GetRequiredAtlasRegions(AssetDatabase.GetAssetPath(skeletonJSON.objectReferenceValue));

							foreach (var atlas in atlasList) {
								for (int i = 0; i < missingPaths.Count; i++) {
									if (atlas.FindRegion(missingPaths[i]) != null) {
										missingPaths.RemoveAt(i);
										i--;
									}
								}
							}

							#if SPINE_TK2D
							if (missingPaths.Count > 0)
								warnings.Add("Missing regions. SkeletonDataAsset requires tk2DSpriteCollectionData or Spine AtlasAssets.");
							#endif

							foreach (var str in missingPaths)
								warnings.Add("Missing Region: '" + str + "'");

						}
					}

				}
			}
		}

		void DoReimport () {
			SpineEditorUtilities.ImportSpineContent(new [] { AssetDatabase.GetAssetPath(skeletonJSON.objectReferenceValue) }, true);
			preview.Clear();
			InitializeEditor();
			EditorUtility.SetDirty(targetSkeletonDataAsset);
		}

		void HandlePreviewSkinChanged (string skinName) {
			EditorPrefs.SetString(LastSkinKey, skinName);
		}

		void OnDestroy () {
			EditorApplication.update -= EditorUpdate;
			preview.OnDestroy();
		}

		#region Preview Handlers
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

		override public GUIContent GetPreviewTitle () {
			return SpineInspectorUtility.TempContent("Preview");
		}

		override public void OnInteractivePreviewGUI (Rect r, GUIStyle background) {
			if (warnings.Count <= 0) {
				preview.Initialize(targetSkeletonDataAsset, this.LastSkinName);
				preview.HandleInteractivePreviewGUI(r, background);
			}
		}

		public override void OnPreviewSettings () {
			const float SliderWidth = 150;
			const float SliderSnap = 0.25f;
			const float SliderMin = 0f;
			const float SliderMax = 2f;

			if (preview.IsValid) {
				float timeScale = GUILayout.HorizontalSlider(preview.TimeScale, SliderMin, SliderMax, GUILayout.MaxWidth(SliderWidth));
				timeScale = Mathf.RoundToInt(timeScale/SliderSnap) * SliderSnap;
				preview.TimeScale = timeScale;
			}
		}

		public override Texture2D RenderStaticPreview (string assetPath, UnityEngine.Object[] subAssets, int width, int height) {
			return preview.GetStaticPreview(width, height);
		}
		#endregion
	}

	class SkeletonInspectorPreview {
		Color OriginColor = new Color(0.3f, 0.3f, 0.3f, 1);
		static readonly int SliderHash = "Slider".GetHashCode();

		SkeletonDataAsset skeletonDataAsset;
		SkeletonData skeletonData;

		SkeletonAnimation skeletonAnimation;
		GameObject previewGameObject;
		internal bool requiresRefresh;
		float animationLastTime;

		public event Action<string> OnSkinChanged;

		Texture previewTexture = new Texture();
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

		float cameraOrthoGoal = 1;
		Vector3 cameraPositionGoal = new Vector3(0, 0, -10);
		double cameraAdjustEndFrame = 0;

		List<Spine.Event> currentAnimationEvents = new List<Spine.Event>();
		List<float> currentAnimationEventTimes = new List<float>();

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

		public TrackEntry ActiveTrack {
			get { return IsValid ? skeletonAnimation.AnimationState.GetCurrent(0) : null; }
		}

		public void Initialize (SkeletonDataAsset skeletonDataAsset, string skinName = "") {
			if (skeletonDataAsset == null) return;
			if (skeletonDataAsset.GetSkeletonData(false) == null)
				return;

			this.skeletonDataAsset = skeletonDataAsset;
			this.skeletonData = skeletonDataAsset.GetSkeletonData(false);

			if (previewRenderUtility == null) {
				previewRenderUtility = new PreviewRenderUtility(true);
				animationLastTime = Time.realtimeSinceStartup;

				const int PreviewLayer = 31;
				const int PreviewCameraCullingMask = 1 << PreviewLayer;

				{
					var c = this.PreviewUtilityCamera;
					c.orthographic = true;
					c.orthographicSize = 1;
					c.cullingMask = PreviewCameraCullingMask;
					c.nearClipPlane = 0.01f;
					c.farClipPlane = 1000f;	
				}

				DestroyPreviewGameObject();

				if (previewGameObject == null) {
					try {
						previewGameObject = SpineEditorUtilities.InstantiateSkeletonAnimation(skeletonDataAsset, skinName).gameObject;

						if (previewGameObject != null) {
							previewGameObject.hideFlags = HideFlags.HideAndDontSave;
							previewGameObject.layer = PreviewLayer;
							skeletonAnimation = previewGameObject.GetComponent<SkeletonAnimation>();
							skeletonAnimation.initialSkinName = skinName;
							skeletonAnimation.LateUpdate();
							previewGameObject.GetComponent<Renderer>().enabled = false;
						}

						AdjustCameraGoals(true);
					} catch {
						DestroyPreviewGameObject();
					}

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
			if (skeletonAnimation == null) {
				Debug.LogWarning("Animation was stopped but preview doesn't exist. It's possible that the Preview Panel is closed.");
			}

			var targetAnimation = skeletonData.FindAnimation(animationName);
			if (targetAnimation != null) {
				var currentTrack = skeletonAnimation.AnimationState.GetCurrent(0);
				bool isEmpty = (currentTrack == null);
				bool isNewAnimation = isEmpty || currentTrack.Animation != targetAnimation;

				var skeleton = skeletonAnimation.Skeleton;
				var animationState = skeletonAnimation.AnimationState;

				if (isEmpty) {
					skeleton.SetToSetupPose();
					animationState.SetAnimation(0, targetAnimation, loop);
				} else {					
					var sameAnimation = (currentTrack.Animation == targetAnimation);
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
				Debug.LogFormat("Something went wrong. The Spine.Animation named '{0}' was not found.", animationName);
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
			DrawTimeBar(r);
			MouseScroll(r);
		}

		void AdjustCameraGoals (bool calculateMixTime = false) {
			if (previewGameObject == null)
				return;

			if (calculateMixTime) {
				if (skeletonAnimation.AnimationState.GetCurrent(0) != null)
					cameraAdjustEndFrame = EditorApplication.timeSinceStartup + skeletonAnimation.AnimationState.GetCurrent(0).Alpha;
			}

			Bounds bounds = previewGameObject.GetComponent<Renderer>().bounds;
			cameraOrthoGoal = bounds.size.y;
			cameraPositionGoal = bounds.center + new Vector3(0, 0, -10f);
		}

		public void AdjustCamera () {
			if (previewRenderUtility == null)
				return;

			if (EditorApplication.timeSinceStartup < cameraAdjustEndFrame)
				AdjustCameraGoals();

			var c = this.PreviewUtilityCamera;
			float orthoSet = Mathf.Lerp(c.orthographicSize, cameraOrthoGoal, 0.1f);

			c.orthographicSize = orthoSet;

			float dist = Vector3.Distance(c.transform.position, cameraPositionGoal);
			if(dist > 0f) {
				Vector3 pos = Vector3.Lerp(c.transform.position, cameraPositionGoal, 0.1f);
				pos.x = 0;
				c.transform.position = pos;
				c.transform.rotation = Quaternion.identity;
				RefreshOnNextUpdate();
			}
		}

		public Texture2D GetStaticPreview (int width, int height) {
			var c = this.PreviewUtilityCamera;
			if (c == null) return null;

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
			if (this.PreviewUtilityCamera.activeTexture == null || this.PreviewUtilityCamera.targetTexture == null )
				return;

			GameObject go = previewGameObject;

			if (requiresRefresh && go != null) {
				go.GetComponent<Renderer>().enabled = true;

				if (!EditorApplication.isPlaying)
					skeletonAnimation.Update((Time.realtimeSinceStartup - animationLastTime));

				animationLastTime = Time.realtimeSinceStartup;

				if (!EditorApplication.isPlaying)
					skeletonAnimation.LateUpdate();

				var thisPreviewUtilityCamera = this.PreviewUtilityCamera;

				if (drawHandles) {			
					Handles.SetCamera(thisPreviewUtilityCamera);
					Handles.color = OriginColor;

					float scale = skeletonDataAsset.scale;
					Handles.DrawLine(new Vector3(-1000 * scale, 0, 0), new Vector3(1000 * scale, 0, 0));
					Handles.DrawLine(new Vector3(0, 1000 * scale, 0), new Vector3(0, -1000 * scale, 0));
				}

				thisPreviewUtilityCamera.Render();

				if (drawHandles) {
					Handles.SetCamera(thisPreviewUtilityCamera);
					SpineHandles.DrawBoundingBoxes(skeletonAnimation.transform, skeletonAnimation.skeleton);
					if (SkeletonDataAssetInspector.showAttachments) SpineHandles.DrawPaths(skeletonAnimation.transform, skeletonAnimation.skeleton);
				}

				go.GetComponent<Renderer>().enabled = false;
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
			OnSkinChanged(skin.Name);
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
			float width = lineRect.width;
			TrackEntry t = skeletonAnimation.AnimationState.GetCurrent(0);

			if (t != null) {
				int loopCount = (int)(t.TrackTime / t.TrackEnd);
				float currentTime = t.TrackTime - (t.TrackEnd * loopCount);
				float normalizedTime = currentTime / t.Animation.Duration;
				float wrappedTime = normalizedTime % 1;

				lineRect.x = barRect.x + (width * wrappedTime) - 0.5f;
				lineRect.width = 2;

				GUI.color = Color.red;
				GUI.DrawTexture(lineRect, EditorGUIUtility.whiteTexture);
				GUI.color = Color.white;

				for (int i = 0; i < currentAnimationEvents.Count; i++) {
					float fr = currentAnimationEventTimes[i];
					var evRect = new Rect(barRect);
					evRect.x = Mathf.Clamp(((fr / t.Animation.Duration) * width) - (Icons.userEvent.width / 2), barRect.x, float.MaxValue);
					evRect.width = Icons.userEvent.width;
					evRect.height = Icons.userEvent.height;
					evRect.y += Icons.userEvent.height;
					GUI.DrawTexture(evRect, Icons.userEvent);

					Event ev = Event.current;
					if (ev.type == EventType.Repaint) {
						if (evRect.Contains(ev.mousePosition)) {
							Rect tooltipRect = new Rect(evRect);
							GUIStyle tooltipStyle = EditorStyles.helpBox;
							tooltipRect.width = tooltipStyle.CalcSize(new GUIContent(currentAnimationEvents[i].Data.Name)).x;
							tooltipRect.y -= 4;
							tooltipRect.x += 4;
							GUI.Label(tooltipRect,  currentAnimationEvents[i].Data.Name, tooltipStyle);
							GUI.tooltip = currentAnimationEvents[i].Data.Name;
						}
					}
				}
			}
		}

		void MouseScroll (Rect position) {
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
	}


}
