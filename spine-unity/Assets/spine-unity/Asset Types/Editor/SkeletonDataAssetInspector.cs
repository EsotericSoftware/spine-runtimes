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

// Contributed by: Mitch Thompson

#define SPINE_SKELETON_ANIMATOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Spine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonDataAsset)), CanEditMultipleObjects]
	public class SkeletonDataAssetInspector : UnityEditor.Editor {
		static bool showAnimationStateData = true;
		static bool showAnimationList = true;
		static bool showSlotList = false;
		static bool showAttachments = false;

		SerializedProperty atlasAssets, skeletonJSON, scale, fromAnimation, toAnimation, duration, defaultMix;
		#if SPINE_TK2D
		SerializedProperty spriteCollection;
		#endif

		#if SPINE_SKELETON_ANIMATOR
		static bool isMecanimExpanded = false;
		SerializedProperty controller;
		#endif

		bool m_initialized = false;
		SkeletonDataAsset m_skeletonDataAsset;
		SkeletonData m_skeletonData;
		string m_skeletonDataAssetGUID;
		bool needToSerialize;

		readonly List<string> warnings = new List<string>();

		GUIStyle activePlayButtonStyle, idlePlayButtonStyle;
		readonly GUIContent DefaultMixLabel = new GUIContent("Default Mix Duration", "Sets 'SkeletonDataAsset.defaultMix' in the asset and 'AnimationState.data.defaultMix' at runtime load time.");

		void OnEnable () {
			SpineEditorUtilities.ConfirmInitialization();
			m_skeletonDataAsset = (SkeletonDataAsset)target;

			atlasAssets = serializedObject.FindProperty("atlasAssets");
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
			atlasAssets.isExpanded = false;
			spriteCollection = serializedObject.FindProperty("spriteCollection");
			#else
			atlasAssets.isExpanded = true;
			#endif

			m_skeletonDataAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_skeletonDataAsset));

			EditorApplication.update -= EditorUpdate;
			EditorApplication.update += EditorUpdate;

			RepopulateWarnings();
			if (m_skeletonDataAsset.skeletonJSON == null) {
				m_skeletonData = null;
				return;	
			}

			m_skeletonData = warnings.Count == 0 ? m_skeletonDataAsset.GetSkeletonData(false) : null;
		}

		void OnDestroy () {
			m_initialized = false;
			EditorApplication.update -= EditorUpdate;
			this.DestroyPreviewInstances();
			if (this.m_previewUtility != null) {
				this.m_previewUtility.Cleanup();
				this.m_previewUtility = null;
			}
		}

		override public void OnInspectorGUI () {
			if (serializedObject.isEditingMultipleObjects) {
				using (new SpineInspectorUtility.BoxScope()) {
					EditorGUILayout.LabelField("SkeletonData", EditorStyles.boldLabel);
					EditorGUILayout.PropertyField(skeletonJSON, SpineInspectorUtility.TempContent(skeletonJSON.displayName, Icons.spine));
					EditorGUILayout.PropertyField(scale);
				}

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

				using (new SpineInspectorUtility.BoxScope()) {
					EditorGUILayout.LabelField("Mix Settings", EditorStyles.boldLabel);
					SpineInspectorUtility.PropertyFieldWideLabel(defaultMix, DefaultMixLabel, 160);
					EditorGUILayout.Space();
				}
				return;
			}

			{ 
				// Lazy initialization because accessing EditorStyles values in OnEnable during a recompile causes UnityEditor to throw null exceptions. (Unity 5.3.5)
				idlePlayButtonStyle = idlePlayButtonStyle ?? new GUIStyle(EditorStyles.miniButton);
				if (activePlayButtonStyle == null) {
					activePlayButtonStyle = new GUIStyle(idlePlayButtonStyle);
					activePlayButtonStyle.normal.textColor = Color.red;
				}
			}

			serializedObject.Update();

			EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(target.name + " (SkeletonDataAsset)", Icons.spine), EditorStyles.whiteLargeLabel);
			if (m_skeletonData != null) {
				EditorGUILayout.LabelField("(Drag and Drop to instantiate.)", EditorStyles.miniLabel);
			}

			EditorGUI.BeginChangeCheck();

			// SkeletonData
			using (new SpineInspectorUtility.BoxScope()) {
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.LabelField("SkeletonData", EditorStyles.boldLabel);
					if (m_skeletonData != null) {
						var sd = m_skeletonData;
						string m = string.Format("{8} - {0} {1}\nBones: {2}\nConstraints: \n {5} IK \n {6} Path \n {7} Transform\n\nSlots: {3}\nSkins: {4}\n\nAnimations: {9}",
							sd.Version, string.IsNullOrEmpty(sd.Version) ? "" : "export          ", sd.Bones.Count, sd.Slots.Count, sd.Skins.Count, sd.IkConstraints.Count, sd.PathConstraints.Count, sd.TransformConstraints.Count, skeletonJSON.objectReferenceValue.name, sd.Animations.Count);
						EditorGUILayout.LabelField(GUIContent.none, new GUIContent(Icons.info, m), GUILayout.Width(30f));
					}
				}

				EditorGUILayout.PropertyField(skeletonJSON, SpineInspectorUtility.TempContent(skeletonJSON.displayName, Icons.spine));
				EditorGUILayout.PropertyField(scale);
			}

//			if (m_skeletonData != null) {
//				if (SpineInspectorUtility.CenteredButton(new GUIContent("Instantiate", Icons.spine, "Creates a new Spine GameObject in the active scene using this Skeleton Data.\nYou can also instantiate by dragging the SkeletonData asset from Project view into Scene View.")))
//					SpineEditorUtilities.ShowInstantiateContextMenu(this.m_skeletonDataAsset, Vector3.zero);
//			}

			// Atlas
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

			if (EditorGUI.EndChangeCheck()) {
				if (serializedObject.ApplyModifiedProperties()) {
					if (m_previewUtility != null) {
						m_previewUtility.Cleanup();
						m_previewUtility = null;
					}
					m_skeletonDataAsset.Clear();
					m_skeletonData = null;
					OnEnable(); // Should call RepopulateWarnings.
					return;
				}
			}

			// Some code depends on the existence of m_skeletonAnimation instance.
			// If m_skeletonAnimation is lazy-instantiated elsewhere, this can cause contents to change between Layout and Repaint events, causing GUILayout control count errors.
			InitPreview();

			if (m_skeletonData != null) {
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
				// Reimport Button
				using (new EditorGUI.DisabledGroupScope(skeletonJSON.objectReferenceValue == null)) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Attempt Reimport", Icons.warning))) {
						DoReimport();
					}
				}
				#else
				EditorGUILayout.HelpBox("Couldn't load SkeletonData.", MessageType.Error);
				#endif

				// List warnings.
				foreach (var line in warnings)
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(line, Icons.warning));
			}

			if (!Application.isPlaying)
				serializedObject.ApplyModifiedProperties();
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
								SkeletonBaker.GenerateMecanimAnimationClips(m_skeletonDataAsset);						
						}
						EditorGUILayout.HelpBox("SkeletonAnimator is the Mecanim alternative to SkeletonAnimation.\nIt is not required.", MessageType.Info);

					} else {

						// Update AnimationClips button.
						using (new GUILayout.HorizontalScope()) {
							GUILayout.Space(EditorGUIUtility.labelWidth);
							if (GUILayout.Button(SpineInspectorUtility.TempContent("Force Update AnimationClips"), GUILayout.Height(20)))
								SkeletonBaker.GenerateMecanimAnimationClips(m_skeletonDataAsset);				
						}

					}
					EditorGUI.indentLevel--;
				}
			}
			#endif
		}

		void DoReimport () {
			SpineEditorUtilities.ImportSpineContent(new string[] { AssetDatabase.GetAssetPath(skeletonJSON.objectReferenceValue) }, true);
			if (m_previewUtility != null) {
				m_previewUtility.Cleanup();
				m_previewUtility = null;
			}

			OnEnable(); // Should call RepopulateWarnings.
			EditorUtility.SetDirty(m_skeletonDataAsset);
		}

		void DrawAnimationStateInfo () {
			using (new SpineInspectorUtility.IndentScope())
				showAnimationStateData = EditorGUILayout.Foldout(showAnimationStateData, "Animation State Data");

			if (!showAnimationStateData)
				return;

			EditorGUI.BeginChangeCheck();
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

			if (EditorGUI.EndChangeCheck()) {
				m_skeletonDataAsset.FillStateData();
				EditorUtility.SetDirty(m_skeletonDataAsset);
				serializedObject.ApplyModifiedProperties();
				needToSerialize = true;
			}
		}

		void DrawAnimationList () {
			showAnimationList = EditorGUILayout.Foldout(showAnimationList, SpineInspectorUtility.TempContent(string.Format("Animations [{0}]", m_skeletonData.Animations.Count), Icons.animationRoot));
			if (!showAnimationList)
				return;

			if (m_skeletonAnimation != null && m_skeletonAnimation.state != null) {
				if (GUILayout.Button(SpineInspectorUtility.TempContent("Setup Pose", Icons.skeleton), GUILayout.Width(105), GUILayout.Height(18))) {
					StopAnimation();
					m_skeletonAnimation.skeleton.SetToSetupPose();
					m_requireRefresh = true;
				}
			} else {
				EditorGUILayout.HelpBox("Animations can be previewed if you expand the Preview window below.", MessageType.Info);
			}

			EditorGUILayout.LabelField("Name", "      Duration");
			foreach (Spine.Animation animation in m_skeletonData.Animations) {
				using (new GUILayout.HorizontalScope()) {
					if (m_skeletonAnimation != null && m_skeletonAnimation.state != null) {
						var activeTrack = m_skeletonAnimation.state.GetCurrent(0);
						if (activeTrack != null && activeTrack.Animation == animation) {
							if (GUILayout.Button("\u25BA", activePlayButtonStyle, GUILayout.Width(24))) {
								StopAnimation();
							}
						} else {
							if (GUILayout.Button("\u25BA", idlePlayButtonStyle, GUILayout.Width(24))) {
								PlayAnimation(animation.Name, true);
							}
						}
					} else {
						GUILayout.Label("-", GUILayout.Width(24));
					}
					EditorGUILayout.LabelField(new GUIContent(animation.Name, Icons.animation), SpineInspectorUtility.TempContent(animation.Duration.ToString("f3") + "s" + ("(" + (Mathf.RoundToInt(animation.Duration * 30)) + ")").PadLeft(12, ' ')));
				}
			}
		}

		void DrawSlotList () {
			showSlotList = EditorGUILayout.Foldout(showSlotList, SpineInspectorUtility.TempContent("Slots", Icons.slotRoot));

			if (!showSlotList) return;
			if (m_skeletonAnimation == null || m_skeletonAnimation.skeleton == null) return;

			EditorGUI.indentLevel++;
			showAttachments = EditorGUILayout.ToggleLeft("Show Attachments", showAttachments);
			var slotAttachments = new List<Attachment>();
			var slotAttachmentNames = new List<string>();
			var defaultSkinAttachmentNames = new List<string>();
			var defaultSkin = m_skeletonData.Skins.Items[0];
			Skin skin = m_skeletonAnimation.skeleton.Skin ?? defaultSkin;
			var slotsItems = m_skeletonAnimation.skeleton.Slots.Items;

			for (int i = m_skeletonAnimation.skeleton.Slots.Count - 1; i >= 0; i--) {
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
							m_requireRefresh = true;
						}
					}
					EditorGUI.indentLevel--;
				}
			}
			EditorGUI.indentLevel--;
		}
			
		void RepopulateWarnings () {
			warnings.Clear();

			// Clear null entries.
			{
				bool hasNulls = false;
				foreach (var a in m_skeletonDataAsset.atlasAssets) {
					if (a == null) {
						hasNulls = true;
						break;
					}
				}
				if (hasNulls) {
					var trimmedAtlasAssets = new List<AtlasAsset>();
					foreach (var a in m_skeletonDataAsset.atlasAssets) {
						if (a != null) trimmedAtlasAssets.Add(a);
					}
					m_skeletonDataAsset.atlasAssets = trimmedAtlasAssets.ToArray();
				}
				serializedObject.Update();
			}

			if (skeletonJSON.objectReferenceValue == null) {
				warnings.Add("Missing Skeleton JSON");
			} else {
				if (SpineEditorUtilities.IsSpineData((TextAsset)skeletonJSON.objectReferenceValue) == false) {
					warnings.Add("Skeleton data file is not a valid JSON or binary file.");
				} else {
					#if !SPINE_TK2D
					bool detectedNullAtlasEntry = false;
					var atlasList = new List<Atlas>();
					var actualAtlasAssets = m_skeletonDataAsset.atlasAssets;
					for (int i = 0; i < actualAtlasAssets.Length; i++) {
						if (m_skeletonDataAsset.atlasAssets[i] == null) {
							detectedNullAtlasEntry = true;
							break;
						} else {
							atlasList.Add(actualAtlasAssets[i].GetAtlas());
						}
					}

					if (detectedNullAtlasEntry)
						warnings.Add("AtlasAsset elements should not be null.");
					else {
						// Get requirements.
						var missingPaths = SpineEditorUtilities.GetRequiredAtlasRegions(AssetDatabase.GetAssetPath((TextAsset)skeletonJSON.objectReferenceValue));

						foreach (var atlas in atlasList) {
							for (int i = 0; i < missingPaths.Count; i++) {
								if (atlas.FindRegion(missingPaths[i]) != null) {
									missingPaths.RemoveAt(i);
									i--;
								}
							}
						}

						foreach (var str in missingPaths)
							warnings.Add("Missing Region: '" + str + "'");
						
					}
					#else
					if (spriteCollection.objectReferenceValue == null)
						warnings.Add("SkeletonDataAsset requires tk2DSpriteCollectionData.");
//					else
//						warnings.Add("Your sprite collection may have missing images.");
					#endif
				}
			}
		}

		#region Preview Window
		PreviewRenderUtility m_previewUtility;
		GameObject m_previewInstance;
		Vector2 previewDir;
		SkeletonAnimation m_skeletonAnimation;
		static readonly int SliderHash = "Slider".GetHashCode();
		float m_lastTime;
		bool m_playing;
		bool m_requireRefresh;
		Color m_originColor = new Color(0.3f, 0.3f, 0.3f, 1);

		void StopAnimation () {
			if (m_skeletonAnimation == null) {
				Debug.LogWarning("Animation was stopped but preview doesn't exist. It's possible that the Preview Panel is closed.");
			}

			m_skeletonAnimation.state.ClearTrack(0);
			m_playing = false;
		}

		List<Spine.Event> m_animEvents = new List<Spine.Event>();
		List<float> m_animEventFrames = new List<float>();

		void PlayAnimation (string animName, bool loop) {
			m_animEvents.Clear();
			m_animEventFrames.Clear();

			m_skeletonAnimation.state.SetAnimation(0, animName, loop);

			Spine.Animation a = m_skeletonAnimation.state.GetCurrent(0).Animation;
			foreach (Timeline t in a.Timelines) {
				if (t.GetType() == typeof(EventTimeline)) {
					var et = (EventTimeline)t;
					for (int i = 0; i < et.Events.Length; i++) {
						m_animEvents.Add(et.Events[i]);
						m_animEventFrames.Add(et.Frames[i]);
					}
				}
			}

			m_playing = true;
		}

		void InitPreview () {
			if (this.m_previewUtility == null) {
				this.m_lastTime = Time.realtimeSinceStartup;
				this.m_previewUtility = new PreviewRenderUtility(true);
				var c = this.m_previewUtility.m_Camera;
				c.orthographic = true;
				c.orthographicSize = 1;
				c.cullingMask = -2147483648;
				c.nearClipPlane = 0.01f;
				c.farClipPlane = 1000f;
				this.CreatePreviewInstances();
			}
		}

		void CreatePreviewInstances () {
			this.DestroyPreviewInstances();

			if (warnings.Count > 0) {
				m_skeletonDataAsset.Clear();
				return;
			}

			var skeletonDataAsset = (SkeletonDataAsset)target;
			if (skeletonDataAsset.GetSkeletonData(false) == null)
				return;

			if (this.m_previewInstance == null) {
				string skinName = EditorPrefs.GetString(m_skeletonDataAssetGUID + "_lastSkin", "");

				try {
					m_previewInstance = SpineEditorUtilities.InstantiateSkeletonAnimation(skeletonDataAsset, skinName).gameObject;

					if (m_previewInstance != null) {
						m_previewInstance.hideFlags = HideFlags.HideAndDontSave;
						m_previewInstance.layer = 0x1f;
						m_skeletonAnimation = m_previewInstance.GetComponent<SkeletonAnimation>();
						m_skeletonAnimation.initialSkinName = skinName;
						m_skeletonAnimation.LateUpdate();
						m_skeletonData = m_skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);
						m_previewInstance.GetComponent<Renderer>().enabled = false;
						m_initialized = true;
					}

					AdjustCameraGoals(true);
				} catch {
					DestroyPreviewInstances();
				}

			}
		}

		void DestroyPreviewInstances () {
			if (this.m_previewInstance != null) {
				DestroyImmediate(this.m_previewInstance);
				m_previewInstance = null;
			}
			m_initialized = false;
		}

		public override bool HasPreviewGUI () {			
			if (serializedObject.isEditingMultipleObjects) {
				// JOHN: Implement multi-preview.
				return false;
			}

			for (int i = 0; i < atlasAssets.arraySize; i++) {
				var prop = atlasAssets.GetArrayElementAtIndex(i);
				if (prop.objectReferenceValue == null)
					return false;
			}

			return skeletonJSON.objectReferenceValue != null;
		}

		Texture m_previewTex = new Texture();

		public override void OnInteractivePreviewGUI (Rect r, GUIStyle background) {
			this.InitPreview();

			if (Event.current.type == EventType.Repaint) {
				if (m_requireRefresh) {
					this.m_previewUtility.BeginPreview(r, background);
					this.DoRenderPreview(true);
					this.m_previewTex = this.m_previewUtility.EndPreview();
					m_requireRefresh = false;
				}
				if (this.m_previewTex != null)
					GUI.DrawTexture(r, m_previewTex, ScaleMode.StretchToFill, false);
			}

			DrawSkinToolbar(r);
			NormalizedTimeBar(r);
			// MITCH: left a todo: Implement panning
			// this.previewDir = Drag2D(this.previewDir, r);
			MouseScroll(r);
		}

		float m_orthoGoal = 1;
		Vector3 m_posGoal = new Vector3(0, 0, -10);
		double m_adjustFrameEndTime = 0;

		void AdjustCameraGoals (bool calculateMixTime) {
			if (this.m_previewInstance == null)
				return;

			if (calculateMixTime) {
				if (m_skeletonAnimation.state.GetCurrent(0) != null)
					m_adjustFrameEndTime = EditorApplication.timeSinceStartup + m_skeletonAnimation.state.GetCurrent(0).Alpha;
			}
				
			GameObject go = this.m_previewInstance;
			Bounds bounds = go.GetComponent<Renderer>().bounds;
			m_orthoGoal = bounds.size.y;
			m_posGoal = bounds.center + new Vector3(0, 0, -10f);
		}

		void AdjustCameraGoals () {
			AdjustCameraGoals(false);
		}

		void AdjustCamera () {
			if (m_previewUtility == null)
				return;

			if (EditorApplication.timeSinceStartup < m_adjustFrameEndTime)
				AdjustCameraGoals();

			float orthoSet = Mathf.Lerp(this.m_previewUtility.m_Camera.orthographicSize, m_orthoGoal, 0.1f);

			this.m_previewUtility.m_Camera.orthographicSize = orthoSet;

			float dist = Vector3.Distance(m_previewUtility.m_Camera.transform.position, m_posGoal);
			if(dist > 0f) {
				Vector3 pos = Vector3.Lerp(this.m_previewUtility.m_Camera.transform.position, m_posGoal, 0.1f);
				pos.x = 0;
				this.m_previewUtility.m_Camera.transform.position = pos;
				this.m_previewUtility.m_Camera.transform.rotation = Quaternion.identity;
				m_requireRefresh = true;
			}
		}

		void DoRenderPreview (bool drawHandles) {
			GameObject go = this.m_previewInstance;

			if (m_requireRefresh && go != null) {
				go.GetComponent<Renderer>().enabled = true;

				if (!EditorApplication.isPlaying)
					m_skeletonAnimation.Update((Time.realtimeSinceStartup - m_lastTime));

				m_lastTime = Time.realtimeSinceStartup;

				if (!EditorApplication.isPlaying)
					m_skeletonAnimation.LateUpdate();

				if (drawHandles) {			
					Handles.SetCamera(m_previewUtility.m_Camera);
					Handles.color = m_originColor;

					Handles.DrawLine(new Vector3(-1000 * m_skeletonDataAsset.scale, 0, 0), new Vector3(1000 * m_skeletonDataAsset.scale, 0, 0));
					Handles.DrawLine(new Vector3(0, 1000 * m_skeletonDataAsset.scale, 0), new Vector3(0, -1000 * m_skeletonDataAsset.scale, 0));
				}

				this.m_previewUtility.m_Camera.Render();

				if (drawHandles) {
					Handles.SetCamera(m_previewUtility.m_Camera);
					SpineHandles.DrawBoundingBoxes(m_skeletonAnimation.transform, m_skeletonAnimation.skeleton);
					if (showAttachments) SpineHandles.DrawPaths(m_skeletonAnimation.transform, m_skeletonAnimation.skeleton);
				}

				go.GetComponent<Renderer>().enabled = false;
			}
				
		}

		void EditorUpdate () {
			AdjustCamera();

			if (m_playing) {
				m_requireRefresh = true;
				Repaint();
			} else if (m_requireRefresh) {
				Repaint();
			} 
			//else {
				//only needed if using smooth menus
			//}

			if (needToSerialize) {
				needToSerialize = false;
				serializedObject.ApplyModifiedProperties();
			}
		}

		void DrawSkinToolbar (Rect r) {
			if (m_skeletonAnimation == null)
				return;

			if (m_skeletonAnimation.skeleton != null) {
				string label = (m_skeletonAnimation.skeleton != null && m_skeletonAnimation.skeleton.Skin != null) ? m_skeletonAnimation.skeleton.Skin.Name : "default";

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
		}
			
		void NormalizedTimeBar (Rect r) {
			if (m_skeletonAnimation == null)
				return;

			Rect barRect = new Rect(r);
			barRect.height = 32;
			barRect.x += 4;
			barRect.width -= 4;

			GUI.Box(barRect, "");

			Rect lineRect = new Rect(barRect);
			float width = lineRect.width;
			TrackEntry t = m_skeletonAnimation.state.GetCurrent(0);

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

				for (int i = 0; i < m_animEvents.Count; i++) {
					float fr = m_animEventFrames[i];
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
							tooltipRect.width = tooltipStyle.CalcSize(new GUIContent(m_animEvents[i].Data.Name)).x;
							tooltipRect.y -= 4;
							tooltipRect.x += 4;
							GUI.Label(tooltipRect,  m_animEvents[i].Data.Name, tooltipStyle);
							GUI.tooltip = m_animEvents[i].Data.Name;
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
					m_orthoGoal += current.delta.y * 0.06f;
					m_orthoGoal = Mathf.Max(0.01f, m_orthoGoal);
					GUIUtility.hotControl = controlID;
					current.Use();
				}
				break;
			}
		}

		// MITCH: left todo:  Implement preview panning
		/*
		static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
		{
			int controlID = GUIUtility.GetControlID(sliderHash, FocusType.Passive);
			UnityEngine.Event current = UnityEngine.Event.current;
			switch (current.GetTypeForControl(controlID))
			{
			case EventType.MouseDown:
				if (position.Contains(current.mousePosition) && (position.width > 50f))
				{
					GUIUtility.hotControl = controlID;
					current.Use();
					EditorGUIUtility.SetWantsMouseJumping(1);
				}
				return scrollPosition;
				
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
				}
				EditorGUIUtility.SetWantsMouseJumping(0);
				return scrollPosition;
				
			case EventType.MouseMove:
				return scrollPosition;
				
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID)
				{
					scrollPosition -= (Vector2) (((current.delta * (!current.shift ? ((float) 1) : ((float) 3))) / Mathf.Min(position.width, position.height)) * 140f);
					scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
					current.Use();
					GUI.changed = true;
				}
				return scrollPosition;
			}
			return scrollPosition;
		}
		*/

		public override GUIContent GetPreviewTitle () {
			return SpineInspectorUtility.TempContent("Preview");
		}

		public override void OnPreviewSettings () {
			const float SliderWidth = 100;
			if (!m_initialized) {
				GUILayout.HorizontalSlider(0, 0, 2, GUILayout.MaxWidth(SliderWidth));
			} else {
				float speed = GUILayout.HorizontalSlider(m_skeletonAnimation.timeScale, 0, 2, GUILayout.MaxWidth(SliderWidth));

				const float SliderSnap = 0.25f;
				float y = speed / SliderSnap;
				int q = Mathf.RoundToInt(y);
				speed = q * SliderSnap;

				m_skeletonAnimation.timeScale = speed;
			}
		}


		public override Texture2D RenderStaticPreview (string assetPath, UnityEngine.Object[] subAssets, int width, int height) {
			var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

			this.InitPreview();
			if (this.m_previewUtility.m_Camera == null)
				return null;

			m_requireRefresh = true;
			this.DoRenderPreview(false);
			AdjustCameraGoals(false);
			this.m_previewUtility.m_Camera.orthographicSize = m_orthoGoal / 2;
			this.m_previewUtility.m_Camera.transform.position = m_posGoal;
			this.m_previewUtility.BeginStaticPreview(new Rect(0, 0, width, height));
			this.DoRenderPreview(false);
			tex = this.m_previewUtility.EndStaticPreview();
			return tex;
		}
		#endregion

		#region Skin Dropdown Context Menu
		void DrawSkinDropdown () {
			var menu = new GenericMenu();
			foreach (Skin s in m_skeletonData.Skins)
				menu.AddItem(new GUIContent(s.Name, Icons.skin), this.m_skeletonAnimation.skeleton.Skin == s, SetSkin, s);
			
			menu.ShowAsContext();
		}

		void SetSkin (object o) {
			Skin skin = (Skin)o;
			m_skeletonAnimation.initialSkinName = skin.Name;
			m_skeletonAnimation.Initialize(true);
			m_requireRefresh = true;
			EditorPrefs.SetString(m_skeletonDataAssetGUID + "_lastSkin", skin.Name);
		}
		#endregion
	}
		
}
