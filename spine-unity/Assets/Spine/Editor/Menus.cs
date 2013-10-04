/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Spine;

public class SpineEditor {
	[MenuItem("Assets/Create/Spine Atlas")]
	static public void CreateAtlas () {
		CreateAsset<AtlasAsset>("New Atlas");
	}
	
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
