#if (UNITY_5_0 || UNITY_5_1 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
#define PREUNITY_5_2
#endif

using UnityEngine;
using System.Collections;

using UnityEditor;
using Spine;

[CustomEditor(typeof(SkeletonGraphic))]
public class SkeletonGraphicInspector : Editor {
	SerializedProperty material_, color_;
	SerializedProperty skeletonDataAsset_, initialSkinName_;
	SerializedProperty startingAnimation_, startingLoop_, timeScale_, freeze_;
	#if !PREUNITY_5_2
	SerializedProperty raycastTarget_;

	SkeletonGraphic thisSkeletonGraphic;

	void OnEnable () {
		var so = this.serializedObject;
		thisSkeletonGraphic = target as SkeletonGraphic;

		// MaskableGraphic
		material_ = so.FindProperty("m_Material");
		color_ = so.FindProperty("m_Color");
		raycastTarget_ = so.FindProperty("m_RaycastTarget");

		// SkeletonRenderer
		skeletonDataAsset_ = so.FindProperty("skeletonDataAsset");
		initialSkinName_ = so.FindProperty("initialSkinName");

		// SkeletonAnimation
		startingAnimation_ = so.FindProperty("startingAnimation");
		startingLoop_ = so.FindProperty("startingLoop");
		timeScale_ = so.FindProperty("timeScale");
		freeze_ = so.FindProperty("freeze");
	}


	public override void OnInspectorGUI () {

		var s = thisSkeletonGraphic;
		s.skeletonDataAsset = SkeletonGraphicInspector.ObjectField<SkeletonDataAsset>(skeletonDataAsset_);
		s.material = SkeletonGraphicInspector.ObjectField<Material>(material_);

		EditorGUI.BeginChangeCheck();
		thisSkeletonGraphic.color = EditorGUILayout.ColorField(color_.displayName, color_.colorValue);
		if (EditorGUI.EndChangeCheck())
			SkeletonGraphicInspector.ForceUpdateHack(thisSkeletonGraphic.transform);

		if (thisSkeletonGraphic.skeletonDataAsset == null) {
			EditorGUILayout.HelpBox("You need to assign a SkeletonDataAsset first.", MessageType.Info);
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
			return;
		}

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(initialSkinName_);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(startingAnimation_);
		s.startingLoop = SkeletonGraphicInspector.BoolField(startingLoop_);
		s.timeScale = EditorGUILayout.FloatField(timeScale_.displayName, timeScale_.floatValue);
		EditorGUILayout.Space();
		s.freeze = SkeletonGraphicInspector.BoolField(freeze_);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
		s.raycastTarget = SkeletonGraphicInspector.BoolField(raycastTarget_);

	}

	#region HAX - Thanks, Unity
	// colors weren't updating in realtime in the custom inspector.
	// Why the hell do I have to do this??
	/// <summary>Use this when scene repaint and proper explicit update methods don't work.</summary>
	public static void ForceUpdateHack (Transform t) {
		var origValue = t.localScale;
		t.localScale = new Vector3(11f, 22f, 33f);
		t.localScale = origValue;
	}

	// Hack for Unity 5.3 problem with PropertyField
	public static T ObjectField<T> (SerializedProperty property) where T : UnityEngine.Object {
		return (T)EditorGUILayout.ObjectField(property.displayName, property.objectReferenceValue, typeof(T), false);
	}

	public static bool BoolField (SerializedProperty property) {
		return EditorGUILayout.Toggle(property.displayName, property.boolValue);
	}
	#endregion

	#region Menus
	[MenuItem ("CONTEXT/SkeletonGraphic/Match RectTransform with Mesh Bounds")]
	static void MatchRectTransformWithBounds (MenuCommand command) {
		var skeletonGraphic = (SkeletonGraphic)command.context;
		var mesh =  skeletonGraphic.SpineMeshGenerator.LastGeneratedMesh;

		var bounds = mesh.bounds;
		var size = bounds.size;
		var center = bounds.center;
		var p = new Vector2(
			0.5f - (center.x / size.x),
			0.5f - (center.y / size.y)
		);

		skeletonGraphic.rectTransform.sizeDelta = size;
		skeletonGraphic.rectTransform.pivot = p;
	}

	public static Material DefaultSkeletonGraphicMaterial {
		get {
			var guids = AssetDatabase.FindAssets("SkeletonGraphicDefault t:material"); if (guids.Length <= 0) return null;
			var firstAssetPath = AssetDatabase.GUIDToAssetPath(guids[0]); if (string.IsNullOrEmpty(firstAssetPath)) return null;
			var firstMaterial = AssetDatabase.LoadAssetAtPath<Material>(firstAssetPath);
			return firstMaterial;
		}
	}

	[MenuItem("GameObject/Spine/SkeletonGraphic (UnityUI)", false, 10)]
	static public void SkeletonGraphicCreateMenuItem () {
		var parentGameObject = Selection.activeObject as GameObject;
		var parentTransform = parentGameObject == null ? null : parentGameObject.GetComponent<RectTransform>();

		if (parentTransform == null) {
			Debug.LogWarning("Your new SkeletonGraphic will not be visible until it is placed under a Canvas");
		}

		var gameObject = NewSkeletonGraphicGameObject("New SkeletonGraphic");
		gameObject.transform.SetParent(parentTransform, false);
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = gameObject;
		EditorGUIUtility.PingObject(Selection.activeObject);
	}

	[MenuItem("Assets/Spine/Instantiate (UnityUI)", false, 0)]
	static void InstantiateSkeletonGraphic () {
		Object[] arr = Selection.objects;
		foreach (Object o in arr) {
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
			string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

			InstantiateSkeletonGraphic((SkeletonDataAsset)o, skinName);
			SceneView.RepaintAll();
		}
	}

	[MenuItem("Assets/Spine/Instantiate (UnityUI)", true, 0)]
	static bool ValidateInstantiateSkeletonGraphic () {
		Object[] arr = Selection.objects;

		if (arr.Length == 0)
			return false;

		foreach (var selected in arr) {
			if (selected.GetType() != typeof(SkeletonDataAsset))
				return false;
		}

		return true;
	}

	public static SkeletonGraphic InstantiateSkeletonGraphic (SkeletonDataAsset skeletonDataAsset, string skinName) {
		return InstantiateSkeletonGraphic(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
	}

	public static SkeletonGraphic InstantiateSkeletonGraphic (SkeletonDataAsset skeletonDataAsset, Skin skin = null) {
		string spineGameObjectName = string.Format("SkeletonGraphic ({0})", skeletonDataAsset.name.Replace("_SkeletonData", ""));
		var go = NewSkeletonGraphicGameObject(spineGameObjectName);
		var graphic = go.GetComponent<SkeletonGraphic>();
		graphic.skeletonDataAsset = skeletonDataAsset;

		SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

		if (data == null) {
			for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
				string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
				skeletonDataAsset.atlasAssets[i] = (AtlasAsset)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAsset));
			}

			data = skeletonDataAsset.GetSkeletonData(true);
		}

		if (skin == null)
			skin = data.DefaultSkin;

		if (skin == null)
			skin = data.Skins.Items[0];

		graphic.Initialize(false);
		graphic.Skeleton.SetSkin(skin);
		graphic.initialSkinName = skin.Name;
		graphic.Skeleton.UpdateWorldTransform();
		graphic.UpdateMesh();

		return graphic;
	}

	static GameObject NewSkeletonGraphicGameObject (string gameObjectName) {
		var go = new GameObject(gameObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(SkeletonGraphic));
		var graphic = go.GetComponent<SkeletonGraphic>();
		graphic.material = SkeletonGraphicInspector.DefaultSkeletonGraphicMaterial;
		return go;
	}
	#endregion
	#endif
}
