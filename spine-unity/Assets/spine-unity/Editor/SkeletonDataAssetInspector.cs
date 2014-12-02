/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
	private SerializedProperty atlasAsset, skeletonJSON, scale, fromAnimation, toAnimation, duration, defaultMix;
	private bool showAnimationStateData = true;
	
	#if UNITY_4_3
	private bool m_showAnimationList = true;
	#else
	private AnimBool m_showAnimationList = new AnimBool(true);
	#endif
	
	private bool m_initialized = false;
	private SkeletonDataAsset m_skeletonDataAsset;
	private string m_skeletonDataAssetGUID;
	
	void OnEnable () {
		try {

			atlasAsset = serializedObject.FindProperty("atlasAsset");
			skeletonJSON = serializedObject.FindProperty("skeletonJSON");
			scale = serializedObject.FindProperty("scale");
			fromAnimation = serializedObject.FindProperty("fromAnimation");
			toAnimation = serializedObject.FindProperty("toAnimation");
			duration = serializedObject.FindProperty("duration");
			defaultMix = serializedObject.FindProperty("defaultMix");
			
			m_skeletonDataAsset = (SkeletonDataAsset)target;
			m_skeletonDataAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_skeletonDataAsset));
			
			EditorApplication.update += Update;

		} catch {


		}
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
		SkeletonDataAsset asset = (SkeletonDataAsset)target;
		
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(atlasAsset);
		EditorGUILayout.PropertyField(skeletonJSON);
		EditorGUILayout.PropertyField(scale);
		if (EditorGUI.EndChangeCheck()) {
			if (m_previewUtility != null) {
				m_previewUtility.Cleanup();
				m_previewUtility = null;
			}
		}
		
		SkeletonData skeletonData = asset.GetSkeletonData(asset.atlasAsset == null || asset.skeletonJSON == null);
		if (skeletonData != null) {
			showAnimationStateData = EditorGUILayout.Foldout(showAnimationStateData, "Animation State Data");
			if (showAnimationStateData) {
				EditorGUILayout.PropertyField(defaultMix);
				
				// Animation names
				String[] animations = new String[skeletonData.Animations.Count];
				for (int i = 0; i < animations.Length; i++)
					animations[i] = skeletonData.Animations[i].Name;
				
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
			}
			
			if (GUILayout.Button(new GUIContent("Setup Pose", SpineEditorUtilities.Icons.skeleton), GUILayout.Width(105), GUILayout.Height(18))) {
				StopAnimation();
				m_skeletonAnimation.skeleton.SetToSetupPose();
				m_requireRefresh = true;
			}
			
			#if UNITY_4_3
			m_showAnimationList = EditorGUILayout.Foldout(m_showAnimationList, new GUIContent("Animations", SpineEditorUtilities.Icons.animationRoot));
			if(m_showAnimationList){
			#else
			m_showAnimationList.target = EditorGUILayout.Foldout(m_showAnimationList.target, new GUIContent("Animations", SpineEditorUtilities.Icons.animationRoot));
			if (EditorGUILayout.BeginFadeGroup(m_showAnimationList.faded)) {
				#endif
					
					
					
					
				EditorGUILayout.LabelField("Name", "Duration");
				foreach (Spine.Animation a in skeletonData.Animations) {
					GUILayout.BeginHorizontal();
						
					if (m_skeletonAnimation != null && m_skeletonAnimation.state != null) {
						if (m_skeletonAnimation.state.GetCurrent(0) != null && m_skeletonAnimation.state.GetCurrent(0).Animation == a) {
							GUI.contentColor = Color.black;
							if (GUILayout.Button("\u25BA", GUILayout.Width(24))) {
								StopAnimation();
							}
							GUI.contentColor = Color.white;
						} else {
							if (GUILayout.Button("\u25BA", GUILayout.Width(24))) {
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
			#if !UNITY_4_3
			EditorGUILayout.EndFadeGroup();
			#endif
		}
			
		if (!Application.isPlaying) {
			if (serializedObject.ApplyModifiedProperties() ||
				(UnityEngine.Event.current.type == EventType.ValidateCommand && UnityEngine.Event.current.commandName == "UndoRedoPerformed")
				    ) {
				asset.Reset();
			}
		}
	}
		
	//preview window stuff
	private PreviewRenderUtility m_previewUtility;
	private GameObject m_previewInstance;
	private Vector2 previewDir;
	private SkeletonAnimation m_skeletonAnimation;
	private SkeletonData m_skeletonData;
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
			this.m_previewUtility.m_Camera.isOrthoGraphic = true;
			this.m_previewUtility.m_Camera.orthographicSize = 1;
			this.m_previewUtility.m_Camera.cullingMask = -2147483648;
			this.CreatePreviewInstances();
		}
	}
		
	private void CreatePreviewInstances () {
		this.DestroyPreviewInstances();
		if (this.m_previewInstance == null) {
			string skinName = EditorPrefs.GetString(m_skeletonDataAssetGUID + "_lastSkin", "");
				
			m_previewInstance = SpineEditorUtilities.SpawnAnimatedSkeleton((SkeletonDataAsset)target, skinName).gameObject;
			m_previewInstance.hideFlags = HideFlags.HideAndDontSave;
			m_previewInstance.layer = 0x1f;
				
				
			m_skeletonAnimation = m_previewInstance.GetComponent<SkeletonAnimation>();
			m_skeletonAnimation.initialSkinName = skinName;
			m_skeletonAnimation.LateUpdate();
				
			m_skeletonData = m_skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);
				
			m_previewInstance.renderer.enabled = false;
				
			m_initialized = true;
			AdjustCameraGoals(true);
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
		if (calculateMixTime) {
			if (m_skeletonAnimation.state.GetCurrent(0) != null) {
				m_adjustFrameEndTime = EditorApplication.timeSinceStartup + m_skeletonAnimation.state.GetCurrent(0).Mix;
			}
		}
			
			
		GameObject go = this.m_previewInstance;
		Bounds bounds = go.renderer.bounds;
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
			
		if (m_requireRefresh) {
			go.renderer.enabled = true;
				
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
			go.renderer.enabled = false;
		}
			
			
	}
		
	void Update () {
		AdjustCamera();
			
		if (m_playing) {
			m_requireRefresh = true;
			Repaint();
		} else if (m_requireRefresh) {
				Repaint();
			} else {
				#if !UNITY_4_3
				if (m_showAnimationList.isAnimating)
					Repaint();
				#endif
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