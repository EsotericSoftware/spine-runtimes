

/*****************************************************************************
 * Automatic import and advanced preview added by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using System;
using System.Collections.Generic;
using UnityEditor;

#if !UNITY_4_3
using UnityEditor.AnimatedValues;
#endif
using UnityEngine;
using Spine;

[CustomEditor(typeof(SkeletonDataAsset))]
public class SkeletonDataAssetInspector : Editor {
	static bool showAnimationStateData = true;
	static bool showAnimationList = true;
	static bool showSlotList = false;
	static bool showAttachments = false;
	static bool showUnity = true;
	static bool bakeAnimations = true;
	static bool bakeIK = true;
	static SendMessageOptions bakeEventOptions = SendMessageOptions.DontRequireReceiver;

	private SerializedProperty atlasAssets, skeletonJSON, scale, fromAnimation, toAnimation, duration, defaultMix, controller;

#if SPINE_TK2D
	private SerializedProperty spriteCollection;
#endif

	private bool m_initialized = false;
	private SkeletonDataAsset m_skeletonDataAsset;
	private SkeletonData m_skeletonData;
	private string m_skeletonDataAssetGUID;
	private bool needToSerialize;

	List<string> warnings = new List<string>();
	
	void OnEnable () {

		SpineEditorUtilities.ConfirmInitialization();

		try {
			atlasAssets = serializedObject.FindProperty("atlasAssets");
			skeletonJSON = serializedObject.FindProperty("skeletonJSON");
			scale = serializedObject.FindProperty("scale");
			fromAnimation = serializedObject.FindProperty("fromAnimation");
			toAnimation = serializedObject.FindProperty("toAnimation");
			duration = serializedObject.FindProperty("duration");
			defaultMix = serializedObject.FindProperty("defaultMix");
			controller = serializedObject.FindProperty("controller");
#if SPINE_TK2D
			spriteCollection = serializedObject.FindProperty("spriteCollection");
#endif

			m_skeletonDataAsset = (SkeletonDataAsset)target;
			m_skeletonDataAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_skeletonDataAsset));

			EditorApplication.update += Update;
		} catch {


		}

		m_skeletonData = m_skeletonDataAsset.GetSkeletonData(true);

		showUnity = EditorPrefs.GetBool("SkeletonDataAssetInspector_showUnity", true);

		RepopulateWarnings();
	}

	void OnDestroy () {
		m_initialized = false;
		EditorApplication.update -= Update;
		this.DestroyPreviewInstances();
		if (this.m_previewUtility != null) {
			this.m_previewUtility.Cleanup();
			this.m_previewUtility = null;
		}
	}

	override public void OnInspectorGUI () {
		serializedObject.Update();

		EditorGUI.BeginChangeCheck();
#if !SPINE_TK2D
		EditorGUILayout.PropertyField(atlasAssets, true);
#else
		EditorGUI.BeginDisabledGroup(spriteCollection.objectReferenceValue != null);
		EditorGUILayout.PropertyField(atlasAssets, true);
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.PropertyField(spriteCollection, true);
#endif
		EditorGUILayout.PropertyField(skeletonJSON);
		EditorGUILayout.PropertyField(scale);
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


		if (m_skeletonData != null) {
			DrawAnimationStateInfo();
			DrawAnimationList();
			DrawSlotList();
			DrawUnityTools();
			
		} else {

			DrawReimportButton();
			//Show Warnings
			foreach (var str in warnings)
				EditorGUILayout.LabelField(new GUIContent(str, SpineEditorUtilities.Icons.warning));
		}

		if(!Application.isPlaying)
			serializedObject.ApplyModifiedProperties();
	}

	void DrawMecanim () {
		
		EditorGUILayout.PropertyField(controller, new GUIContent("Controller", SpineEditorUtilities.Icons.controllerIcon));		
		if (controller.objectReferenceValue == null) {
			GUILayout.BeginHorizontal();
			GUILayout.Space(32);
			if (GUILayout.Button(new GUIContent("Generate Mecanim Controller"), EditorStyles.toolbarButton, GUILayout.Width(195), GUILayout.Height(20)))
				SkeletonBaker.GenerateMecanimAnimationClips(m_skeletonDataAsset);
			//GUILayout.Label(new GUIContent("Alternative to SkeletonAnimation, not a requirement.", SpineEditorUtilities.Icons.warning));
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Alternative to SkeletonAnimation, not required", EditorStyles.miniLabel);
		}
		
	}

	void DrawUnityTools () {
		bool pre = showUnity;
		showUnity = EditorGUILayout.Foldout(showUnity, new GUIContent("Unity Tools", SpineEditorUtilities.Icons.unityIcon));
		if (pre != showUnity)
			EditorPrefs.SetBool("SkeletonDataAssetInspector_showUnity", showUnity);

		if (showUnity) {
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("SkeletonAnimator", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			DrawMecanim();
			EditorGUI.indentLevel--;
			GUILayout.Space(32);
			EditorGUILayout.LabelField("Baking", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("WARNING!\n\nBaking is NOT the same as SkeletonAnimator!\nDoes not support the following:\n\tFlipX or Y\n\tInheritScale\n\tColor Keys\n\tDraw Order Keys\n\tIK and Curves are sampled at 60fps and are not realtime.\n\tPlease read SkeletonBaker.cs comments for full details.\n\nThe main use of Baking is to export Spine projects to be used without the Spine Runtime (ie: for sale on the Asset Store, or background objects that are animated only with a wind noise generator)", MessageType.Warning, true);
			EditorGUI.indentLevel++;
			bakeAnimations = EditorGUILayout.Toggle("Bake Animations", bakeAnimations);
			EditorGUI.BeginDisabledGroup(bakeAnimations == false);
			{
				EditorGUI.indentLevel++;
				bakeIK = EditorGUILayout.Toggle("Bake IK", bakeIK);
				bakeEventOptions = (SendMessageOptions)EditorGUILayout.EnumPopup("Event Options", bakeEventOptions);
				EditorGUI.indentLevel--;
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.indentLevel++;
			GUILayout.BeginHorizontal();
			{


				if (GUILayout.Button(new GUIContent("Bake All Skins", SpineEditorUtilities.Icons.unityIcon), GUILayout.Height(32), GUILayout.Width(150)))
					SkeletonBaker.BakeToPrefab(m_skeletonDataAsset, m_skeletonData.Skins, "", bakeAnimations, bakeIK, bakeEventOptions);

				string skinName = "<No Skin>";

				if (m_skeletonAnimation != null && m_skeletonAnimation.skeleton != null) {

					Skin bakeSkin = m_skeletonAnimation.skeleton.Skin;
					if (bakeSkin == null) {
						skinName = "Default";
						bakeSkin = m_skeletonData.Skins[0];
					} else
						skinName = m_skeletonAnimation.skeleton.Skin.Name;

					bool oops = false;

					try {
						GUILayout.BeginVertical();
						if (GUILayout.Button(new GUIContent("Bake " + skinName, SpineEditorUtilities.Icons.unityIcon), GUILayout.Height(32), GUILayout.Width(250)))
							SkeletonBaker.BakeToPrefab(m_skeletonDataAsset, new List<Skin>(new Skin[] { bakeSkin }), "", bakeAnimations, bakeIK, bakeEventOptions);

						GUILayout.BeginHorizontal();
						GUILayout.Label(new GUIContent("Skins", SpineEditorUtilities.Icons.skinsRoot), GUILayout.Width(50));
						if (GUILayout.Button(skinName, EditorStyles.popup, GUILayout.Width(196))) {
							SelectSkinContext();
						}
						GUILayout.EndHorizontal();



					} catch {
						oops = true;
						//GUILayout.BeginVertical();
					}



					if (!oops)
						GUILayout.EndVertical();
				}

			}
			GUILayout.EndHorizontal();
			EditorGUI.indentLevel--;
			EditorGUI.indentLevel--;
		}


	}
	void DrawReimportButton () {
		EditorGUI.BeginDisabledGroup(skeletonJSON.objectReferenceValue == null);
		if (GUILayout.Button(new GUIContent("Attempt Reimport", SpineEditorUtilities.Icons.warning))) {
			DoReimport();
			return;
		}
		EditorGUI.EndDisabledGroup();
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

		// Animation names
		String[] animations = new String[m_skeletonData.Animations.Count];
		for (int i = 0; i < animations.Length; i++)
			animations[i] = m_skeletonData.Animations[i].Name;

		for (int i = 0; i < fromAnimation.arraySize; i++) {
			SerializedProperty from = fromAnimation.GetArrayElementAtIndex(i);
			SerializedProperty to = toAnimation.GetArrayElementAtIndex(i);
			SerializedProperty durationProp = duration.GetArrayElementAtIndex(i);
			EditorGUILayout.BeginHorizontal();
			from.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations, from.stringValue), 0), animations)];
			to.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations, to.stringValue), 0), animations)];
			durationProp.floatValue = EditorGUILayout.FloatField(durationProp.floatValue);
			if (GUILayout.Button("Delete")) {
				duration.DeleteArrayElementAtIndex(i);
				toAnimation.DeleteArrayElementAtIndex(i);
				fromAnimation.DeleteArrayElementAtIndex(i);
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space();
		if (GUILayout.Button("Add Mix")) {
			duration.arraySize++;
			toAnimation.arraySize++;
			fromAnimation.arraySize++;
		}
		EditorGUILayout.Space();
		EditorGUILayout.EndHorizontal();

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

		if (GUILayout.Button(new GUIContent("Setup Pose", SpineEditorUtilities.Icons.skeleton), GUILayout.Width(105), GUILayout.Height(18))) {
			StopAnimation();
			m_skeletonAnimation.skeleton.SetToSetupPose();
			m_requireRefresh = true;
		}

		EditorGUILayout.LabelField("Name", "Duration");
		foreach (Spine.Animation a in m_skeletonData.Animations) {
			GUILayout.BeginHorizontal();

			if (m_skeletonAnimation != null && m_skeletonAnimation.state != null) {
				if (m_skeletonAnimation.state.GetCurrent(0) != null && m_skeletonAnimation.state.GetCurrent(0).Animation == a) {
					GUI.contentColor = Color.red;
					if (GUILayout.Button("\u25BA", EditorStyles.toolbarButton, GUILayout.Width(24))) {
						StopAnimation();
					}
					GUI.contentColor = Color.white;
				} else {
					if (GUILayout.Button("\u25BA", EditorStyles.toolbarButton, GUILayout.Width(24))) {
						PlayAnimation(a.Name, true);
					}
				}
			} else {
				GUILayout.Label("?", GUILayout.Width(24));
			}
			EditorGUILayout.LabelField(new GUIContent(a.Name, SpineEditorUtilities.Icons.animation), new GUIContent(a.Duration.ToString("f3") + "s" + ("(" + (Mathf.RoundToInt(a.Duration * 30)) + ")").PadLeft(12, ' ')));
			GUILayout.EndHorizontal();
		}
	}


	void DrawSlotList () {
		showSlotList = EditorGUILayout.Foldout(showSlotList, new GUIContent("Slots", SpineEditorUtilities.Icons.slotRoot));

		if (!showSlotList)
			return;

		if (m_skeletonAnimation == null || m_skeletonAnimation.skeleton == null)
			return;

		EditorGUI.indentLevel++;
		try {
			showAttachments = EditorGUILayout.ToggleLeft("Show Attachments", showAttachments);
		} catch {
			return;
		}


		List<Attachment> slotAttachments = new List<Attachment>();
		List<string> slotAttachmentNames = new List<string>();
		List<string> defaultSkinAttachmentNames = new List<string>();
		var defaultSkin = m_skeletonData.Skins[0];
		Skin skin = m_skeletonAnimation.skeleton.Skin;
		if (skin == null) {
			skin = defaultSkin;
		}

		for (int i = m_skeletonAnimation.skeleton.Slots.Count - 1; i >= 0; i--) {
			Slot slot = m_skeletonAnimation.skeleton.Slots[i];
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
					string name = slotAttachmentNames[a];

					Texture2D icon = null;
					var type = attachment.GetType();

					if (type == typeof(RegionAttachment))
						icon = SpineEditorUtilities.Icons.image;
					else if (type == typeof(MeshAttachment))
						icon = SpineEditorUtilities.Icons.mesh;
					else if (type == typeof(BoundingBoxAttachment))
						icon = SpineEditorUtilities.Icons.boundingBox;
					else if (type == typeof(SkinnedMeshAttachment))
						icon = SpineEditorUtilities.Icons.weights;
					else
						icon = SpineEditorUtilities.Icons.warning;

					//TODO:  Waterboard Nate
					//if (name != attachment.Name)
					//icon = SpineEditorUtilities.Icons.skinPlaceholder;

					bool initialState = slot.Attachment == attachment;

					bool toggled = EditorGUILayout.ToggleLeft(new GUIContent(name, icon), slot.Attachment == attachment);

					if (!defaultSkinAttachmentNames.Contains(name)) {
						Rect skinPlaceHolderIconRect = GUILayoutUtility.GetLastRect();
						skinPlaceHolderIconRect.width = SpineEditorUtilities.Icons.skinPlaceholder.width;
						skinPlaceHolderIconRect.height = SpineEditorUtilities.Icons.skinPlaceholder.height;
						GUI.DrawTexture(skinPlaceHolderIconRect, SpineEditorUtilities.Icons.skinPlaceholder);
					}


					if (toggled != initialState) {
						if (toggled) {
							slot.Attachment = attachment;
						} else {
							slot.Attachment = null;
						}
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

		if (skeletonJSON.objectReferenceValue == null)
			warnings.Add("Missing Skeleton JSON");
		else {

			if (SpineEditorUtilities.IsValidSpineData((TextAsset)skeletonJSON.objectReferenceValue) == false) {
				warnings.Add("Skeleton data file is not a valid JSON or binary file.");
			} else {
				bool detectedNullAtlasEntry = false;
				List<Atlas> atlasList = new List<Atlas>();
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
					//get requirements
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
			}


		}
	}

	//preview window stuff
	private PreviewRenderUtility m_previewUtility;
	private GameObject m_previewInstance;
	private Vector2 previewDir;
	private SkeletonAnimation m_skeletonAnimation;
	//private SkeletonData m_skeletonData;
	private static int sliderHash = "Slider".GetHashCode();
	private float m_lastTime;
	private bool m_playing;
	private bool m_requireRefresh;
	private Color m_originColor = new Color(0.3f, 0.3f, 0.3f, 1);

	private void StopAnimation () {
		m_skeletonAnimation.state.ClearTrack(0);
		m_playing = false;
	}

	List<Spine.Event> m_animEvents = new List<Spine.Event>();
	List<float> m_animEventFrames = new List<float>();

	private void PlayAnimation (string animName, bool loop) {
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

	private void InitPreview () {
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

	private void CreatePreviewInstances () {
		this.DestroyPreviewInstances();
		if (this.m_previewInstance == null) {
			try {
				string skinName = EditorPrefs.GetString(m_skeletonDataAssetGUID + "_lastSkin", "");

				m_previewInstance = SpineEditorUtilities.InstantiateSkeletonAnimation((SkeletonDataAsset)target, skinName).gameObject;
				m_previewInstance.hideFlags = HideFlags.HideAndDontSave;
				m_previewInstance.layer = 0x1f;


				m_skeletonAnimation = m_previewInstance.GetComponent<SkeletonAnimation>();
				m_skeletonAnimation.initialSkinName = skinName;
				m_skeletonAnimation.LateUpdate();

				m_skeletonData = m_skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);

				m_previewInstance.GetComponent<Renderer>().enabled = false;

				m_initialized = true;
				AdjustCameraGoals(true);
			} catch {

			}
		}
	}

	private void DestroyPreviewInstances () {
		if (this.m_previewInstance != null) {
			DestroyImmediate(this.m_previewInstance);
			m_previewInstance = null;
		}
		m_initialized = false;
	}

	public override bool HasPreviewGUI () {
		//TODO: validate json data

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
		//TODO: implement panning
		//		this.previewDir = Drag2D(this.previewDir, r);
		MouseScroll(r);
	}

	float m_orthoGoal = 1;
	Vector3 m_posGoal = new Vector3(0, 0, -10);
	double m_adjustFrameEndTime = 0;

	private void AdjustCameraGoals (bool calculateMixTime) {
		if (this.m_previewInstance == null)
			return;

		if (calculateMixTime) {
			if (m_skeletonAnimation.state.GetCurrent(0) != null) {
				m_adjustFrameEndTime = EditorApplication.timeSinceStartup + m_skeletonAnimation.state.GetCurrent(0).Mix;
			}
		}


		GameObject go = this.m_previewInstance;

		Bounds bounds = go.GetComponent<Renderer>().bounds;
		m_orthoGoal = bounds.size.y;

		m_posGoal = bounds.center + new Vector3(0, 0, -10);
	}

	private void AdjustCameraGoals () {
		AdjustCameraGoals(false);
	}

	private void AdjustCamera () {
		if (m_previewUtility == null)
			return;


		if (EditorApplication.timeSinceStartup < m_adjustFrameEndTime) {
			AdjustCameraGoals();
		}

		float orthoSet = Mathf.Lerp(this.m_previewUtility.m_Camera.orthographicSize, m_orthoGoal, 0.1f);

		this.m_previewUtility.m_Camera.orthographicSize = orthoSet;

		float dist = Vector3.Distance(m_previewUtility.m_Camera.transform.position, m_posGoal);
		if (dist > 60f * ((SkeletonDataAsset)target).scale) {
			Vector3 pos = Vector3.Lerp(this.m_previewUtility.m_Camera.transform.position, m_posGoal, 0.1f);
			pos.x = 0;
			this.m_previewUtility.m_Camera.transform.position = pos;
			this.m_previewUtility.m_Camera.transform.rotation = Quaternion.identity;
			m_requireRefresh = true;
		}
	}

	private void DoRenderPreview (bool drawHandles) {
		GameObject go = this.m_previewInstance;

		if (m_requireRefresh && go != null) {
			go.GetComponent<Renderer>().enabled = true;

			if (EditorApplication.isPlaying) {
				//do nothing
			} else {
				m_skeletonAnimation.Update((Time.realtimeSinceStartup - m_lastTime));
			}

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
					if (slot.Attachment is BoundingBoxAttachment) {

						DrawBoundingBox(slot.Bone, (BoundingBoxAttachment)slot.Attachment);
					}
				}
			}

			go.GetComponent<Renderer>().enabled = false;
		}


	}

	void DrawBoundingBox (Bone bone, BoundingBoxAttachment box) {
		float[] worldVerts = new float[box.Vertices.Length];
		box.ComputeWorldVertices(bone, worldVerts);

		Handles.color = Color.green;
		Vector3 lastVert = Vector3.back;
		Vector3 vert = Vector3.back;
		Vector3 firstVert = new Vector3(worldVerts[0], worldVerts[1], -1);
		for (int i = 0; i < worldVerts.Length; i += 2) {
			vert.x = worldVerts[i];
			vert.y = worldVerts[i + 1];

			if (i > 0) {
				Handles.DrawLine(lastVert, vert);
			}


			lastVert = vert;
		}

		Handles.DrawLine(lastVert, firstVert);

		
		
	}

	void Update () {
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
				SelectSkinContext();
			}
		}
	}

	void SelectSkinContext () {
		GenericMenu menu = new GenericMenu();

		foreach (Skin s in m_skeletonData.Skins) {
			menu.AddItem(new GUIContent(s.Name), this.m_skeletonAnimation.skeleton.Skin == s, SetSkin, (object)s);
		}

		menu.ShowAsContext();
	}

	void SetSkin (object o) {
		Skin skin = (Skin)o;

		m_skeletonAnimation.initialSkinName = skin.Name;
		m_skeletonAnimation.Reset();
		m_requireRefresh = true;

		EditorPrefs.SetString(m_skeletonDataAssetGUID + "_lastSkin", skin.Name);
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
				//TODO: Tooltip
				//Spine.Event spev = animEvents[i];

				float fr = m_animEventFrames[i];

				Rect evRect = new Rect(barRect);
				evRect.x = Mathf.Clamp(((fr / t.Animation.Duration) * width) - (SpineEditorUtilities.Icons._event.width / 2), barRect.x, float.MaxValue);
				evRect.width = SpineEditorUtilities.Icons._event.width;
				evRect.height = SpineEditorUtilities.Icons._event.height;
				evRect.y += SpineEditorUtilities.Icons._event.height;
				GUI.DrawTexture(evRect, SpineEditorUtilities.Icons._event);


				//TODO:  Tooltip
				/*
				UnityEngine.Event ev = UnityEngine.Event.current;
				if(ev.isMouse){
					if(evRect.Contains(ev.mousePosition)){
						Rect tooltipRect = new Rect(evRect);
						tooltipRect.width = 500;
						tooltipRect.y -= 4;
						tooltipRect.x += 4;
						GUI.Label(tooltipRect, spev.Data.Name);
					}
				}
				*/
			}
		}
	}

	void MouseScroll (Rect position) {
		UnityEngine.Event current = UnityEngine.Event.current;
		int controlID = GUIUtility.GetControlID(sliderHash, FocusType.Passive);

		switch (current.GetTypeForControl(controlID)) {
			case EventType.ScrollWheel:
				if (position.Contains(current.mousePosition)) {

					m_orthoGoal += current.delta.y * ((SkeletonDataAsset)target).scale * 10;
					GUIUtility.hotControl = controlID;
					current.Use();
				}
				break;
		}

	}

	//TODO:  Implement preview panning
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

	//TODO:  Fix first-import error
	//TODO:  Update preview without thumbnail
	public override Texture2D RenderStaticPreview (string assetPath, UnityEngine.Object[] subAssets, int width, int height) {
		Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

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

		//TODO:  Figure out why this is throwing errors on first attempt
		//		if(m_previewUtility != null){
		//			Handles.SetCamera(this.m_previewUtility.m_Camera);
		//			Handles.BeginGUI();
		//			GUI.DrawTexture(new Rect(40,60,width,height), SpineEditorUtilities.Icons.spine, ScaleMode.StretchToFill);
		//			Handles.EndGUI();
		//		}
		tex = this.m_previewUtility.EndStaticPreview();
		return tex;
	}
}