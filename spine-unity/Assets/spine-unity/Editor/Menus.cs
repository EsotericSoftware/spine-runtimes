/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	public static class Menus {
		[MenuItem("Assets/Create/Spine Atlas")]
		static public void CreateAtlas () {
			CreateAsset<AtlasAsset>("New Atlas");
		}

		[MenuItem("Assets/Create/Spine SkeletonData")]
		static public void CreateSkeletonData () {
			CreateAsset<SkeletonDataAsset>("New SkeletonData");
		}

		static private void CreateAsset <T> (String name) where T : ScriptableObject {
			var dir = "Assets/";
			var selected = Selection.activeObject;
			if (selected != null) {
				var assetDir = AssetDatabase.GetAssetPath(selected.GetInstanceID());
				if (assetDir.Length > 0 && Directory.Exists(assetDir))
					dir = assetDir + "/";
			}
			ScriptableObject asset = ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(asset, dir + name + ".asset");
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}

		[MenuItem("GameObject/Spine/SkeletonRenderer", false, 10)]
		static public void CreateSkeletonRendererGameObject () {
			CreateSpineGameObject<SkeletonRenderer>("New SkeletonRenderer");
		}

		[MenuItem("GameObject/Spine/SkeletonAnimation", false, 10)]
		static public void CreateSkeletonAnimationGameObject () {
			CreateSpineGameObject<SkeletonAnimation>("New SkeletonAnimation");
		}

		static public void CreateSpineGameObject<T> (string name) where T : MonoBehaviour {
			var parentGameObject = Selection.activeObject as GameObject;
			var parentTransform = parentGameObject == null ? null : parentGameObject.transform;

			var gameObject = new GameObject("New SkeletonRenderer", typeof(T));
			gameObject.transform.SetParent(parentTransform, false);
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = gameObject;
			EditorGUIUtility.PingObject(Selection.activeObject);
		}
	}
}
