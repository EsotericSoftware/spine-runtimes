/*****************************************************************************
 * Automatic import and advanced preview added by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
#define SPINE_SKELETON_ANIMATOR
#define SPINE_BAKING

using System;
using System.Collections.Generic;
using UnityEditor;

#if !UNITY_4_3
using UnityEditor.AnimatedValues;
#endif
using UnityEngine;
using Spine;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonDataAsset))]
	public class SkeletonDataAssetInspector : UnityEditor.Editor {
		static bool showAnimationStateData = true;
		static bool showAnimationList = true;
		static bool showSlotList = false;
		static bool showAttachments = false;

		#if SPINE_BAKING
		static bool isBakingExpanded = false;
		static bool bakeAnimations = true;
		static bool bakeIK = true;
		static SendMessageOptions bakeEventOptions = SendMessageOptions.DontRequireReceiver;
		const string ShowBakingPrefsKey = "SkeletonDataAssetInspector_showUnity";
		#endif

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

		List<string> warnings = new List<string>();

		GUIStyle activePlayButtonStyle, idlePlayButtonStyle;

		void OnEnable () {
			SpineEditorUtilities.ConfirmInitialization();

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

			#if SPINE_BAKING
			isBakingExpanded = EditorPrefs.GetBool(ShowBakingPrefsKey, false);
			#endif

			m_skeletonDataAsset = (SkeletonDataAsset)target;
			m_skeletonDataAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_skeletonDataAsset));
			EditorApplication.update += EditorUpdate;
			m_skeletonData = m_skeletonDataAsset.GetSkeletonData(false);
			RepopulateWarnings();
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
			// Lazy initialization
			{ 
				// Accessing EditorStyles values in OnEnable during a recompile causes UnityEditor to throw null exceptions. (Unity 5.3.5)
				idlePlayButtonStyle = idlePlayButtonStyle ?? new GUIStyle(EditorStyles.toolbarButton);
				if (activePlayButtonStyle == null) {
					activePlayButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
					activePlayButtonStyle.normal.textColor = Color.red;
				}
			}

			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			#if !SPINE_TK2D
			EditorGUILayout.PropertyField(atlasAssets, true);
			#else
			using (new EditorGUI.DisabledGroupScope(spriteCollection.objectReferenceValue != null)) {
				EditorGUILayout.PropertyField(atlasAssets, true);
			}
			EditorGUILayout.LabelField("spine-tk2d", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(spriteCollection, true);
			#endif
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(skeletonJSON);
			EditorGUILayout.PropertyField(scale);
			EditorGUILayout.Space();
			if (EditorGUI.EndChangeCheck()) {
				if (serializedObject.ApplyModifiedProperties()) {
					if (m_previewUtility != null) {
						m_previewUtility.Cleanup();
						m_previewUtility = null;
					}
					RepopulateWarnings();
					OnEnable();
					return;
				}
			}

			// Some code depends on the existence of m_skeletonAnimation instance.
			// If m_skeletonAnimation is lazy-instantiated elsewhere, this can cause contents to change between Layout and Repaint events, causing GUILayout control count errors.
			InitPreview();
			if (m_skeletonData != null) {
				DrawAnimationStateInfo();
				DrawAnimationList();
				DrawSlotList();
				DrawUnityTools();
			} else {
				#if !SPINE_TK2D
				// Reimport Button
				using (new EditorGUI.DisabledGroupScope(skeletonJSON.objectReferenceValue == null)) {
					if (GUILayout.Button(new GUIContent("Attempt Reimport", SpineEditorUtilities.Icons.warning))) {
						DoReimport();
						return;
					}
				}
				#else
				EditorGUILayout.HelpBox("Couldn't load SkeletonData.", MessageType.Error);
				#endif

				// List warnings.
				foreach (var line in warnings)
					EditorGUILayout.LabelField(new GUIContent(line, SpineEditorUtilities.Icons.warning));
				
			}

			if (!Application.isPlaying)
				serializedObject.ApplyModifiedProperties();
		}

		void DrawUnityTools () {
			#if SPINE_SKELETON_ANIMATOR
			isMecanimExpanded = EditorGUILayout.Foldout(isMecanimExpanded, new GUIContent("SkeletonAnimator", SpineEditorUtilities.Icons.unityIcon));
			if (isMecanimExpanded) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(controller, new GUIContent("Controller", SpineEditorUtilities.Icons.controllerIcon));		
				if (controller.objectReferenceValue == null) {
					
					// Generate Mecanim Controller Button
					using (new GUILayout.HorizontalScope()) {
						GUILayout.Space(EditorGUIUtility.labelWidth);
						if (GUILayout.Button(new GUIContent("Generate Mecanim Controller"), GUILayout.Height(20)))
							SkeletonBaker.GenerateMecanimAnimationClips(m_skeletonDataAsset);						
					}
					EditorGUILayout.HelpBox("SkeletonAnimator is the Mecanim alternative to SkeletonAnimation.\nIt is not required.", MessageType.Info);

				} else {
					
					// Update AnimationClips button.
					using (new GUILayout.HorizontalScope()) {
						GUILayout.Space(EditorGUIUtility.labelWidth);
						if (GUILayout.Button(new GUIContent("Force Update AnimationClips"), GUILayout.Height(20)))
							SkeletonBaker.GenerateMecanimAnimationClips(m_skeletonDataAsset);				
					}

				}
				EditorGUI.indentLevel--;
			}
			#endif

			#if SPINE_BAKING
			bool pre = isBakingExpanded;
			isBakingExpanded = EditorGUILayout.Foldout(isBakingExpanded, new GUIContent("Baking", SpineEditorUtilities.Icons.unityIcon));
			if (pre != isBakingExpanded)
				EditorPrefs.SetBool(ShowBakingPrefsKey, isBakingExpanded);
			
			if (isBakingExpanded) {
				EditorGUI.indentLevel++;
				const string BakingWarningMessage =
					"WARNING!" +
					"\nBaking is NOT the same as SkeletonAnimator!" +

					"\n\nThe main use of Baking is to export Spine projects to be used without the Spine Runtime (ie: for sale on the Asset Store, or background objects that are animated only with a wind noise generator)" +

					"\n\nBaking also does not support the following:" +
					"\n\tDisabled transform inheritance" +
					"\n\tShear" +
					"\n\tColor Keys" +
					"\n\tDraw Order Keys" +
					"\n\tAll Constraint types" +

					"\n\nCurves are sampled at 60fps and are not realtime." +
					"\nPlease read SkeletonBaker.cs comments for full details.";
				EditorGUILayout.HelpBox(BakingWarningMessage, MessageType.Warning, true);

				EditorGUI.indentLevel++;
				bakeAnimations = EditorGUILayout.Toggle("Bake Animations", bakeAnimations);
				using (new EditorGUI.DisabledGroupScope(!bakeAnimations)) {
					EditorGUI.indentLevel++;
					bakeIK = EditorGUILayout.Toggle("Bake IK", bakeIK);
					bakeEventOptions = (SendMessageOptions)EditorGUILayout.EnumPopup("Event Options", bakeEventOptions);
					EditorGUI.indentLevel--;
				}

				// Bake Skin buttons.
				using (new GUILayout.HorizontalScope()) {
					if (GUILayout.Button(new GUIContent("Bake All Skins", SpineEditorUtilities.Icons.unityIcon), GUILayout.Height(32), GUILayout.Width(150)))
						SkeletonBaker.BakeToPrefab(m_skeletonDataAsset, m_skeletonData.Skins, "", bakeAnimations, bakeIK, bakeEventOptions);
					
					if (m_skeletonAnimation != null && m_skeletonAnimation.skeleton != null) {
						Skin bakeSkin = m_skeletonAnimation.skeleton.Skin;

						string skinName = "<No Skin>";
						if (bakeSkin == null) {
							skinName = "Default";
							bakeSkin = m_skeletonData.Skins.Items[0];
						} else
							skinName = m_skeletonAnimation.skeleton.Skin.Name;

						using (new GUILayout.VerticalScope()) {
							if (GUILayout.Button(new GUIContent("Bake \"" + skinName + "\"", SpineEditorUtilities.Icons.unityIcon), GUILayout.Height(32), GUILayout.Width(250)))
								SkeletonBaker.BakeToPrefab(m_skeletonDataAsset, new ExposedList<Skin>(new [] { bakeSkin }), "", bakeAnimations, bakeIK, bakeEventOptions);
							using (new GUILayout.HorizontalScope()) {
								GUILayout.Label(new GUIContent("Skins", SpineEditorUtilities.Icons.skinsRoot), GUILayout.Width(50));
								if (GUILayout.Button(skinName, EditorStyles.popup, GUILayout.Width(196))) {
									DrawSkinDropdown();
								}
							}
						}
					}
				}

				EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;
			}
			#endif
		}

		void DoReimport () {
			SpineEditorUtilities.ImportSpineContent(new string[] { AssetDatabase.GetAssetPath(skeletonJSON.objectReferenceValue) }, true);

			if (m_previewUtility != null) {
				m_previewUtility.Cleanup();
				m_previewUtility = null;
			}

			RepopulateWarnings();
			OnEnable();

			EditorUtility.SetDirty(m_skeletonDataAsset);
		}

		void DrawAnimationStateInfo () {
			showAnimationStateData = EditorGUILayout.Foldout(showAnimationStateData, "Animation State Data");
			if (!showAnimationStateData)
				return;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(defaultMix);

			var animations = new string[m_skeletonData.Animations.Count];
			for (int i = 0; i < animations.Length; i++)
				animations[i] = m_skeletonData.Animations.Items[i].Name;

			for (int i = 0; i < fromAnimation.arraySize; i++) {
				SerializedProperty from = fromAnimation.GetArrayElementAtIndex(i);
				SerializedProperty to = toAnimation.GetArrayElementAtIndex(i);
				SerializedProperty durationProp = duration.GetArrayElementAtIndex(i);
				using (new EditorGUILayout.HorizontalScope()) {
					from.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations, from.stringValue), 0), animations)];
					to.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations, to.stringValue), 0), animations)];
					durationProp.floatValue = EditorGUILayout.FloatField(durationProp.floatValue);
					if (GUILayout.Button("Delete")) {
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
			showAnimationList = EditorGUILayout.Foldout(showAnimationList, new GUIContent("Animations", SpineEditorUtilities.Icons.animationRoot));
			if (!showAnimationList)
				return;

			if (m_skeletonAnimation != null && m_skeletonAnimation.state != null) {
				if (GUILayout.Button(new GUIContent("Setup Pose", SpineEditorUtilities.Icons.skeleton), GUILayout.Width(105), GUILayout.Height(18))) {
					StopAnimation();
					m_skeletonAnimation.skeleton.SetToSetupPose();
					m_requireRefresh = true;
				}
			} else {
				EditorGUILayout.HelpBox("Animations can be previewed if you expand the Preview window below.", MessageType.Info);
			}

			EditorGUILayout.LabelField("Name", "Duration");
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
					EditorGUILayout.LabelField(new GUIContent(animation.Name, SpineEditorUtilities.Icons.animation), new GUIContent(animation.Duration.ToString("f3") + "s" + ("(" + (Mathf.RoundToInt(animation.Duration * 30)) + ")").PadLeft(12, ' ')));
				}
			}
		}

		void DrawSlotList () {
			showSlotList = EditorGUILayout.Foldout(showSlotList, new GUIContent("Slots", SpineEditorUtilities.Icons.slotRoot));

			if (!showSlotList) return;
			if (m_skeletonAnimation == null || m_skeletonAnimation.skeleton == null) return;

			EditorGUI.indentLevel++;

			try {
				showAttachments = EditorGUILayout.ToggleLeft("Show Attachments", showAttachments);
			} catch {
				return;
			}

			List<Attachment> slotAttachments = new List<Attachment>();
			List<string> slotAttachmentNames = new List<string>();
			List<string> defaultSkinAttachmentNames = new List<string>();
			var defaultSkin = m_skeletonData.Skins.Items[0];
			Skin skin = m_skeletonAnimation.skeleton.Skin ?? defaultSkin;

			for (int i = m_skeletonAnimation.skeleton.Slots.Count - 1; i >= 0; i--) {
				Slot slot = m_skeletonAnimation.skeleton.Slots.Items[i];
				EditorGUILayout.LabelField(new GUIContent(slot.Data.Name, SpineEditorUtilities.Icons.slot));
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

						Texture2D icon = null;
						var type = attachment.GetType();

						if (type == typeof(RegionAttachment))
							icon = SpineEditorUtilities.Icons.image;
						else if (type == typeof(MeshAttachment))
							icon = SpineEditorUtilities.Icons.mesh;
						else if (type == typeof(BoundingBoxAttachment))
							icon = SpineEditorUtilities.Icons.boundingBox;
						else if (type == typeof(PathAttachment))
							icon = SpineEditorUtilities.Icons.boundingBox;
						else
							icon = SpineEditorUtilities.Icons.warning;
						//JOHN: left todo: Icon for paths. Generic icon for unidentified attachments.

						// MITCH: left todo:  Waterboard Nate
						//if (name != attachment.Name)
						//icon = SpineEditorUtilities.Icons.skinPlaceholder;

						bool initialState = slot.Attachment == attachment;

						bool toggled = EditorGUILayout.ToggleLeft(new GUIContent(attachmentName, icon), slot.Attachment == attachment);

						if (!defaultSkinAttachmentNames.Contains(attachmentName)) {
							Rect skinPlaceHolderIconRect = GUILayoutUtility.GetLastRect();
							skinPlaceHolderIconRect.width = SpineEditorUtilities.Icons.skinPlaceholder.width;
							skinPlaceHolderIconRect.height = SpineEditorUtilities.Icons.skinPlaceholder.height;
							GUI.DrawTexture(skinPlaceHolderIconRect, SpineEditorUtilities.Icons.skinPlaceholder);
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

			if (skeletonJSON.objectReferenceValue == null) {
				warnings.Add("Missing Skeleton JSON");
			} else {
				if (SpineEditorUtilities.IsValidSpineData((TextAsset)skeletonJSON.objectReferenceValue) == false) {
					warnings.Add("Skeleton data file is not a valid JSON or binary file.");
				} else {
					#if !SPINE_TK2D
					bool detectedNullAtlasEntry = false;
					var atlasList = new List<Atlas>();
					for (int i = 0; i < atlasAssets.arraySize; i++) {
						if (atlasAssets.GetArrayElementAtIndex(i).objectReferenceValue == null) {
							detectedNullAtlasEntry = true;
							break;
						} else {
							atlasList.Add(((AtlasAsset)atlasAssets.GetArrayElementAtIndex(i).objectReferenceValue).GetAtlas());
						}
					}

					if (detectedNullAtlasEntry)
						warnings.Add("AtlasAsset elements cannot be Null");
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
					if (spriteCollection.objectReferenceValue == null) {
						warnings.Add("SkeletonDataAsset requires tk2DSpriteCollectionData.");
					} else {
						warnings.Add("Your sprite collection may have missing images.");
					}
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
					EventTimeline et = (EventTimeline)t;

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
				this.m_previewUtility.m_Camera.orthographic = true;
				this.m_previewUtility.m_Camera.orthographicSize = 1;
				this.m_previewUtility.m_Camera.cullingMask = -2147483648;
				this.m_previewUtility.m_Camera.nearClipPlane = 0.01f;
				this.m_previewUtility.m_Camera.farClipPlane = 1000f;
				this.CreatePreviewInstances();
			}
		}

		void CreatePreviewInstances () {
			this.DestroyPreviewInstances();

			var skeletonDataAsset = (SkeletonDataAsset)target;
			if (skeletonDataAsset.GetSkeletonData(false) == null)
				return;

			if (this.m_previewInstance == null) {
				string skinName = EditorPrefs.GetString(m_skeletonDataAssetGUID + "_lastSkin", "");
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
			// MITCH: left todo: validate json data

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

			if (UnityEngine.Event.current.type == EventType.Repaint) {
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
					m_adjustFrameEndTime = EditorApplication.timeSinceStartup + m_skeletonAnimation.state.GetCurrent(0).Mix;
			}
				
			GameObject go = this.m_previewInstance;
			Bounds bounds = go.GetComponent<Renderer>().bounds;
			m_orthoGoal = bounds.size.y;
			m_posGoal = bounds.center + new Vector3(0, 0, -10);
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
					foreach (var slot in m_skeletonAnimation.skeleton.Slots) {
						var boundingBoxAttachment = slot.Attachment as BoundingBoxAttachment;
						if (boundingBoxAttachment != null)
							DrawBoundingBox (slot, boundingBoxAttachment);
					}
				}

				go.GetComponent<Renderer>().enabled = false;
			}
				
		}

		static void DrawBoundingBox (Slot slot, BoundingBoxAttachment box) {
			if (box.Vertices.Length <= 0) return; // Handle cases where user creates a BoundingBoxAttachment but doesn't actually define it.

			var worldVerts = new float[box.Vertices.Length];
			box.ComputeWorldVertices(slot, worldVerts);

			Handles.color = Color.green;
			Vector3 lastVert = Vector3.back;
			Vector3 vert = Vector3.back;
			Vector3 firstVert = new Vector3(worldVerts[0], worldVerts[1], -1);
			for (int i = 0; i < worldVerts.Length; i += 2) {
				vert.x = worldVerts[i];
				vert.y = worldVerts[i + 1];

				if (i > 0)
					Handles.DrawLine(lastVert, vert);

				lastVert = vert;
			}

			Handles.DrawLine(lastVert, firstVert);

		}

		void EditorUpdate () {
			AdjustCamera();

			if (m_playing) {
				m_requireRefresh = true;
				Repaint();
			} else if (m_requireRefresh) {
				Repaint();
			} else {
				//only needed if using smooth menus
			}

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
				EditorGUI.DropShadowLabel(popRect, new GUIContent("Skin", SpineEditorUtilities.Icons.skinsRoot));

				popRect.y += 11;
				popRect.width = 150;
				popRect.x += 44;

				if (GUI.Button(popRect, label, EditorStyles.popup)) {
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
				int loopCount = (int)(t.Time / t.EndTime);
				float currentTime = t.Time - (t.EndTime * loopCount);

				float normalizedTime = currentTime / t.Animation.Duration;

				lineRect.x = barRect.x + (width * normalizedTime) - 0.5f;
				lineRect.width = 2;

				GUI.color = Color.red;
				GUI.DrawTexture(lineRect, EditorGUIUtility.whiteTexture);
				GUI.color = Color.white;

				for (int i = 0; i < m_animEvents.Count; i++) {
					// MITCH: left todo: Tooltip
					//Spine.Event spev = animEvents[i];

					float fr = m_animEventFrames[i];
					var evRect = new Rect(barRect);
					evRect.x = Mathf.Clamp(((fr / t.Animation.Duration) * width) - (SpineEditorUtilities.Icons._event.width / 2), barRect.x, float.MaxValue);
					evRect.width = SpineEditorUtilities.Icons._event.width;
					evRect.height = SpineEditorUtilities.Icons._event.height;
					evRect.y += SpineEditorUtilities.Icons._event.height;
					GUI.DrawTexture(evRect, SpineEditorUtilities.Icons._event);

					// MITCH: left todo:  Tooltip
//					UnityEngine.Event ev = UnityEngine.Event.current;
//					if (ev.isMouse) {
//						if (evRect.Contains(ev.mousePosition)) {
//							Rect tooltipRect = new Rect(evRect);
//							tooltipRect.width = 500;
//							tooltipRect.y -= 4;
//							tooltipRect.x += 4;
//							GUI.Label(tooltipRect, spev.Data.Name);
//						}
//					}
				}
			}
		}

		void MouseScroll (Rect position) {
			UnityEngine.Event current = UnityEngine.Event.current;
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
			return new GUIContent("Preview");
		}

		public override void OnPreviewSettings () {
			if (!m_initialized) {
				GUILayout.HorizontalSlider(0, 0, 2, GUILayout.MaxWidth(64));
			} else {
				float speed = GUILayout.HorizontalSlider(m_skeletonAnimation.timeScale, 0, 2, GUILayout.MaxWidth(64));

				//snap to nearest 0.25
				float y = speed / 0.25f;
				int q = Mathf.RoundToInt(y);
				speed = q * 0.25f;

				m_skeletonAnimation.timeScale = speed;
			}
		}

		// MITCH: left todo: Fix first-import error
		// MITCH: left todo: Update preview without thumbnail
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

			//MITCH: left todo:  Figure out why this is throwing errors on first attempt
			//		if(m_previewUtility != null){
			//			Handles.SetCamera(this.m_previewUtility.m_Camera);
			//			Handles.BeginGUI();
			//			GUI.DrawTexture(new Rect(40,60,width,height), SpineEditorUtilities.Icons.spine, ScaleMode.StretchToFill);
			//			Handles.EndGUI();
			//		}
			tex = this.m_previewUtility.EndStaticPreview();
			return tex;
		}
		#endregion

		#region Skin Dropdown Context Menu
		void DrawSkinDropdown () {
			var menu = new GenericMenu();
			foreach (Skin s in m_skeletonData.Skins)
				menu.AddItem(new GUIContent(s.Name), this.m_skeletonAnimation.skeleton.Skin == s, SetSkin, s);
			
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
