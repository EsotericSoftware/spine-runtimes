/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

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
