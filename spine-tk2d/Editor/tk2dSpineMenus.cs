using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Spine;

/*
 */
public class tk2dSpineMenus {
	
	/*
	 */
	[MenuItem("Assets/Create/tk2d/Spine Skeleton Data")]
	static public void CreateSkeletonData() {
		string path = "";
		try {
			path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject)) + "/";
		} catch (Exception) {
			path = "Assets/";
		}
		
		ScriptableObject asset = ScriptableObject.CreateInstance<tk2dSpineSkeletonDataAsset>();
		AssetDatabase.CreateAsset(asset,path + "New Spine Skeleton Data.asset");
		AssetDatabase.SaveAssets();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}
	
	/*
	 */
	[MenuItem("GameObject/Create Other/tk2d/Spine Skeleton")]
	static public void CreateSkeletonGameObject() {
		GameObject gameObject = new GameObject("New tk2d Spine Skeleton",typeof(tk2dSpineSkeleton));
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = gameObject;
	}
	
	/*
	 */
	[MenuItem("Component/2D Toolkit/Spine Skeleton")]
	static public void CreateSkeletonComponent() {
		Selection.activeGameObject.AddComponent(typeof(tk2dSpineSkeleton));
	}
	
	/*
	 */
	[MenuItem("Component/2d Toolkit/Spine Skeleton",true)]
	static public bool ValidateCreateSkeletonComponent() {
		return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent(typeof(tk2dSpineSkeleton)) == null;
	}
}
