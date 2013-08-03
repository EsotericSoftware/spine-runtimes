using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Spine;

public class Menus {
	[MenuItem("Assets/Create/Spine SkeletonData")]
	static public void CreateSkeletonData () {
		CreateAsset<SkeletonDataAsset>("New SkeletonData");
	}
	
	static private void CreateAsset <T> (String path) where T : ScriptableObject {
		try {
			path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject)) + "/" + path;
		} catch (Exception) {
			path = "Assets/" + path;
		}
		ScriptableObject asset = ScriptableObject.CreateInstance<T>();
		AssetDatabase.CreateAsset(asset, path + ".asset");
		AssetDatabase.SaveAssets();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}

	[MenuItem("GameObject/Create Other/Spine SkeletonComponent")]
	static public void CreateSkeletonComponentGameObject () {
		GameObject gameObject = new GameObject("New SkeletonComponent", typeof(SkeletonComponent));
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = gameObject;
	}

	[MenuItem("GameObject/Create Other/Spine SkeletonAnimation")]
	static public void CreateSkeletonAnimationGameObject () {
		GameObject gameObject = new GameObject("New SkeletonAnimation", typeof(SkeletonAnimation));
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = gameObject;
	}
	
	[MenuItem("Component/Spine SkeletonComponent")]
	static public void CreateSkeletonComponent () {
		Selection.activeGameObject.AddComponent(typeof(SkeletonComponent));
	}
	
	[MenuItem("Component/Spine SkeletonAnimation")]
	static public void CreateSkeletonAnimation () {
		Selection.activeGameObject.AddComponent(typeof(SkeletonAnimation));
	}
	
	[MenuItem("Component/Spine SkeletonComponent", true)]
	static public bool ValidateCreateSkeletonComponent () {
		return Selection.activeGameObject != null
			&& Selection.activeGameObject.GetComponent(typeof(SkeletonComponent)) == null
			&& Selection.activeGameObject.GetComponent(typeof(SkeletonAnimation)) == null;
	}

	[MenuItem("Component/Spine SkeletonAnimation", true)]
	static public bool ValidateCreateSkeletonAnimation () {
		return ValidateCreateSkeletonComponent();
	}
}
