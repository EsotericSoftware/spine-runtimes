/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	using Editor = UnityEditor.Editor;
	using Icons = SpineEditorUtilities.Icons;

	public class SpriteAtlasImportWindow : EditorWindow {
		const bool IsUtilityWindow = false;

		[MenuItem("Window/Spine/SpriteAtlas Import", false, 5000)]
		public static void Init (MenuCommand command) {
			var window = EditorWindow.GetWindow<SpriteAtlasImportWindow>(IsUtilityWindow);
			window.minSize = new Vector2(284f, 256f);
			window.maxSize = new Vector2(500f, 256f);
			window.titleContent = new GUIContent("Spine SpriteAtlas Import", Icons.spine);
			window.Show();
		}

		public UnityEngine.U2D.SpriteAtlas spriteAtlasAsset;
		public TextAsset skeletonDataFile;
		public SpineSpriteAtlasAsset spineSpriteAtlasAsset;

		SerializedObject so;

		void OnEnable () {
			if (!SpineSpriteAtlasAsset.AnySpriteAtlasNeedsRegionsLoaded())
				return;
			EditorApplication.update -= SpineSpriteAtlasAsset.UpdateWhenEditorPlayModeStarted;
			EditorApplication.update += SpineSpriteAtlasAsset.UpdateWhenEditorPlayModeStarted;
		}

		void OnDisable () {
			EditorApplication.update -= SpineSpriteAtlasAsset.UpdateWhenEditorPlayModeStarted;
		}

		void OnGUI () {
			so = so ?? new SerializedObject(this);

			EditorGUIUtility.wideMode = true;
			EditorGUILayout.LabelField("Spine SpriteAtlas Import", EditorStyles.boldLabel);

			using (new SpineInspectorUtility.BoxScope()) {
				EditorGUI.BeginChangeCheck();
				var spriteAtlasAssetProperty = so.FindProperty("spriteAtlasAsset");
				EditorGUILayout.PropertyField(spriteAtlasAssetProperty, new GUIContent("SpriteAtlas", EditorGUIUtility.IconContent("SpriteAtlas Icon").image));
				if (EditorGUI.EndChangeCheck()) {
					so.ApplyModifiedProperties();
					if (spriteAtlasAsset != null) {
						if (AssetUtility.SpriteAtlasSettingsNeedAdjustment(spriteAtlasAsset)) {
							AssetUtility.AdjustSpriteAtlasSettings(spriteAtlasAsset);
						}
						GenerateAssetsFromSpriteAtlas(spriteAtlasAsset);
					}
				}

				var spineSpriteAtlasAssetProperty = so.FindProperty("spineSpriteAtlasAsset");
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(spineSpriteAtlasAssetProperty, new GUIContent("SpineSpriteAtlasAsset", EditorGUIUtility.IconContent("ScriptableObject Icon").image));
				if (spineSpriteAtlasAssetProperty.objectReferenceValue == null) {
					spineSpriteAtlasAssetProperty.objectReferenceValue = spineSpriteAtlasAsset = FindSpineSpriteAtlasAsset(spriteAtlasAsset);
				}
				if (EditorGUI.EndChangeCheck()) {
					so.ApplyModifiedProperties();
				}
				EditorGUILayout.Space();

				using (new EditorGUI.DisabledScope(spineSpriteAtlasAsset == null)) {
					if (SpineInspectorUtility.LargeCenteredButton(new GUIContent("Load regions by entering Play mode"))) {
						GenerateAssetsFromSpriteAtlas(spriteAtlasAsset);
						SpineSpriteAtlasAsset.UpdateByStartingEditorPlayMode();
					}
				}

				using (new SpineInspectorUtility.BoxScope()) {
					if (spriteAtlasAsset == null) {
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Please assign SpriteAtlas file.", Icons.warning), GUILayout.Height(46));
					} else if (spineSpriteAtlasAsset == null || spineSpriteAtlasAsset.RegionsNeedLoading) {
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Please hit 'Load regions ..' to load\nregion info. Play mode is started\nand stopped automatically.", Icons.warning), GUILayout.Height(54));
					} else {
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("SpriteAtlas imported\nsuccessfully.", Icons.spine), GUILayout.Height(46));
					}
				}
			}

			bool isAtlasComplete = (spineSpriteAtlasAsset != null && !spineSpriteAtlasAsset.RegionsNeedLoading);
			bool canImportSkeleton = (spriteAtlasAsset != null && skeletonDataFile != null);
			using (new SpineInspectorUtility.BoxScope()) {

				using (new EditorGUI.DisabledScope(!isAtlasComplete)) {
					var skeletonDataAssetProperty = so.FindProperty("skeletonDataFile");
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(skeletonDataAssetProperty, SpineInspectorUtility.TempContent("Skeleton json/skel file", Icons.spine));
					if (EditorGUI.EndChangeCheck()) {
						so.ApplyModifiedProperties();
					}
					EditorGUILayout.Space();
				}
				using (new EditorGUI.DisabledScope(!canImportSkeleton)) {
					if (SpineInspectorUtility.LargeCenteredButton(new GUIContent("Import Skeleton"))) {
						//AssetUtility.IngestSpriteAtlas(spriteAtlasAsset, null);
						string skeletonPath = AssetDatabase.GetAssetPath(skeletonDataFile);
						string[] skeletons = new string[] { skeletonPath };
						AssetUtility.ImportSpineContent(skeletons, null);
					}
				}
			}
		}

		void GenerateAssetsFromSpriteAtlas (UnityEngine.U2D.SpriteAtlas spriteAtlasAsset) {
			AssetUtility.IngestSpriteAtlas(spriteAtlasAsset, null);
			string texturePath;
			if (AssetUtility.GeneratePngFromSpriteAtlas(spriteAtlasAsset, out texturePath)) {
				Debug.Log(string.Format("Generated SpriteAtlas texture '{0}'", texturePath), spriteAtlasAsset);
			}
		}

		SpineSpriteAtlasAsset FindSpineSpriteAtlasAsset (UnityEngine.U2D.SpriteAtlas spriteAtlasAsset) {
			string path = AssetDatabase.GetAssetPath(spriteAtlasAsset).Replace(".spriteatlas", AssetUtility.SpriteAtlasSuffix + ".asset");
			if (System.IO.File.Exists(path)) {
				return AssetDatabase.LoadAssetAtPath<SpineSpriteAtlasAsset>(path);
			}
			return null;
		}

		SkeletonDataAsset FindSkeletonDataAsset (TextAsset skeletonDataFile) {
			string path = AssetDatabase.GetAssetPath(skeletonDataFile);
			path = path.Replace(".json", AssetUtility.SkeletonDataSuffix + ".asset");
			path = path.Replace(".skel.bytes", AssetUtility.SkeletonDataSuffix + ".asset");
			if (System.IO.File.Exists(path)) {
				return AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
			}
			return null;
		}
	}
}
