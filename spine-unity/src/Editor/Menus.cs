using System;
using UnityEditor;
using UnityEngine;
using Spine;

public class SpineEditor {
	[MenuItem("Assets/Create/Spine Atlas")]
	static public void CreateAtlas () {
		CreateAsset<AtlasAsset>("Assets/New Spine Atlas");
	}
	
	[MenuItem("Assets/Create/Spine Skeleton Data")]
	static public void CreateSkeletonData () {
		CreateAsset<SkeletonDataAsset>("Assets/New Spine Skeleton Data");
	}
	
	static private void CreateAsset <T> (String path) where T : ScriptableObject {
		ScriptableObject asset = ScriptableObject.CreateInstance<T>();
		AssetDatabase.CreateAsset(asset, path + ".asset");
		AssetDatabase.SaveAssets();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}

	[MenuItem("GameObject/Create Other/Spine Skeleton")]
	static public void CreateSkeletonGameObject () {
		GameObject gameObject = new GameObject("New Spine Skeleton", typeof(SkeletonComponent));
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = gameObject;
	}
	
	[MenuItem("Component/Spine Skeleton")]
	static public void CreateSkeletonComponent () {
		Selection.activeGameObject.AddComponent(typeof(SkeletonComponent));
	}
	
	[MenuItem("Component/Spine Skeleton", true)]
	static public bool ValidateCreateSkeletonComponent () {
		return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent(typeof(SkeletonComponent)) == null;
	}
}