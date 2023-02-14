/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

//#define BAKE_ALL_BUTTON
//#define REGION_BAKING_MESH

using Spine;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;

	[CustomEditor(typeof(SpineAtlasAsset)), CanEditMultipleObjects]
	public class SpineAtlasAssetInspector : UnityEditor.Editor {
		SerializedProperty atlasFile, materials;
		SpineAtlasAsset atlasAsset;

		GUIContent spriteSlicesLabel;
		GUIContent SpriteSlicesLabel {
			get {
				if (spriteSlicesLabel == null) {
					spriteSlicesLabel = new GUIContent(
						"Apply Regions as Texture Sprite Slices",
						SpineEditorUtilities.Icons.unity,
						"Adds Sprite slices to atlas texture(s). " +
						"Updates existing slices if ones with matching names exist. \n\n" +
						"If your atlas was exported with Premultiply Alpha, " +
						"your SpriteRenderer should use the generated Spine _Material asset (or any Material with a PMA shader) instead of Sprites-Default.");
				}
				return spriteSlicesLabel;
			}
		}

		static List<AtlasRegion> GetRegions (Atlas atlas) {
			FieldInfo regionsField = SpineInspectorUtility.GetNonPublicField(typeof(Atlas), "regions");
			return (List<AtlasRegion>)regionsField.GetValue(atlas);
		}

		void OnEnable () {
			SpineEditorUtilities.ConfirmInitialization();
			atlasFile = serializedObject.FindProperty("atlasFile");
			materials = serializedObject.FindProperty("materials");
			materials.isExpanded = true;
			atlasAsset = (SpineAtlasAsset)target;
#if REGION_BAKING_MESH
			UpdateBakedList();
#endif
		}

#if REGION_BAKING_MESH
		private List<bool> baked;
		private List<GameObject> bakedObjects;

		void UpdateBakedList () {
			AtlasAsset asset = (AtlasAsset)target;
			baked = new List<bool>();
			bakedObjects = new List<GameObject>();
			if (atlasFile.objectReferenceValue != null) {
				List<AtlasRegion> regions = this.Regions;
				string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
				string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath);
				string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);
				for (int i = 0; i < regions.Count; i++) {
					AtlasRegion region = regions[i];
					string bakedPrefabPath = Path.Combine(bakedDirPath, AssetUtility.GetPathSafeRegionName(region) + ".prefab").Replace("\\", "/");
					GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(bakedPrefabPath, typeof(GameObject));
					baked.Add(prefab != null);
					bakedObjects.Add(prefab);
				}
			}
		}
#endif

		override public void OnInspectorGUI () {
			if (serializedObject.isEditingMultipleObjects) {
				DrawDefaultInspector();
				return;
			}

			serializedObject.Update();
			atlasAsset = (atlasAsset == null) ? (SpineAtlasAsset)target : atlasAsset;
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(atlasFile);
			EditorGUILayout.PropertyField(materials, true);
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
				atlasAsset.Clear();
				atlasAsset.GetAtlas();
			}

			if (materials.arraySize == 0) {
				EditorGUILayout.HelpBox("No materials", MessageType.Error);
				return;
			}

			for (int i = 0; i < materials.arraySize; i++) {
				SerializedProperty prop = materials.GetArrayElementAtIndex(i);
				Material material = (Material)prop.objectReferenceValue;
				if (material == null) {
					EditorGUILayout.HelpBox("Materials cannot be null.", MessageType.Error);
					return;
				}
			}

			EditorGUILayout.Space();
			if (SpineInspectorUtility.LargeCenteredButton(SpineInspectorUtility.TempContent("Set Mipmap Bias to " + SpinePreferences.DEFAULT_MIPMAPBIAS, tooltip: "This may help textures with mipmaps be less blurry when used for 2D sprites."))) {
				foreach (Material m in atlasAsset.materials) {
					Texture texture = m.mainTexture;
					string texturePath = AssetDatabase.GetAssetPath(texture.GetInstanceID());
					TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);
					importer.mipMapBias = SpinePreferences.DEFAULT_MIPMAPBIAS;
					EditorUtility.SetDirty(texture);
				}
				Debug.Log("Texture mipmap bias set to " + SpinePreferences.DEFAULT_MIPMAPBIAS);
			}

			EditorGUILayout.Space();
			if (atlasFile.objectReferenceValue != null) {
				if (SpineInspectorUtility.LargeCenteredButton(SpriteSlicesLabel)) {
					Atlas atlas = atlasAsset.GetAtlas();
					foreach (Material m in atlasAsset.materials)
						UpdateSpriteSlices(m.mainTexture, atlas);
				}
			}

			EditorGUILayout.Space();

#if REGION_BAKING_MESH
			if (atlasFile.objectReferenceValue != null) {
				Atlas atlas = asset.GetAtlas();
				FieldInfo field = typeof(Atlas).GetField("regions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic);
				List<AtlasRegion> regions = (List<AtlasRegion>)field.GetValue(atlas);
				EditorGUILayout.LabelField(new GUIContent("Region Baking", SpineEditorUtilities.Icons.unityIcon));
				EditorGUI.indentLevel++;
				AtlasPage lastPage = null;
				for (int i = 0; i < regions.Count; i++) {
					if (lastPage != regions[i].page) {
						if (lastPage != null) {
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
						}
						lastPage = regions[i].page;
						Material mat = ((Material)lastPage.rendererObject);
						if (mat != null) {
							GUILayout.BeginHorizontal();
							{
								EditorGUI.BeginDisabledGroup(true);
								EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(250));
								EditorGUI.EndDisabledGroup();
							}
							GUILayout.EndHorizontal();

						} else {
							EditorGUILayout.LabelField(new GUIContent("Page missing material!", SpineEditorUtilities.Icons.warning));
						}
					}
					GUILayout.BeginHorizontal();
					{
						//EditorGUILayout.ToggleLeft(baked[i] ? "" : regions[i].name, baked[i]);
						bool result = baked[i] ? EditorGUILayout.ToggleLeft("", baked[i], GUILayout.Width(24)) : EditorGUILayout.ToggleLeft("    " + regions[i].name, baked[i]);
						if(baked[i]){
							EditorGUILayout.ObjectField(bakedObjects[i], typeof(GameObject), false, GUILayout.Width(250));
						}
						if (result && !baked[i]) {
							//bake
							baked[i] = true;
							bakedObjects[i] = SpineEditorUtilities.BakeRegion(atlasAsset, regions[i]);
							EditorGUIUtility.PingObject(bakedObjects[i]);
						} else if (!result && baked[i]) {
							//unbake
							bool unbakeResult = EditorUtility.DisplayDialog("Delete Baked Region", "Do you want to delete the prefab for " + regions[i].name, "Yes", "Cancel");
							switch (unbakeResult) {
							case true:
								//delete
								string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
								string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath);
								string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);
								string bakedPrefabPath = Path.Combine(bakedDirPath, SpineEditorUtilities.GetPathSafeRegionName(regions[i]) + ".prefab").Replace("\\", "/");
								AssetDatabase.DeleteAsset(bakedPrefabPath);
								baked[i] = false;
								break;
							case false:
								//do nothing
								break;
							}
						}
					}
					GUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel--;

#if BAKE_ALL_BUTTON
				// Check state
				bool allBaked = true;
				bool allUnbaked = true;
				for (int i = 0; i < regions.Count; i++) {
					allBaked &= baked[i];
					allUnbaked &= !baked[i];
				}

				if (!allBaked && GUILayout.Button("Bake All")) {
					for (int i = 0; i < regions.Count; i++) {
						if (!baked[i]) {
							baked[i] = true;
							bakedObjects[i] = SpineEditorUtilities.BakeRegion(atlasAsset, regions[i]);
						}
					}

				} else if (!allUnbaked && GUILayout.Button("Unbake All")) {
					bool unbakeResult = EditorUtility.DisplayDialog("Delete All Baked Regions", "Are you sure you want to unbake all region prefabs? This cannot be undone.", "Yes", "Cancel");
					switch (unbakeResult) {
					case true:
						//delete
						for (int i = 0; i < regions.Count; i++) {
							if (baked[i]) {
								string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
								string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath);
								string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);
								string bakedPrefabPath = Path.Combine(bakedDirPath, SpineEditorUtilities.GetPathSafeRegionName(regions[i]) + ".prefab").Replace("\\", "/");
								AssetDatabase.DeleteAsset(bakedPrefabPath);
								baked[i] = false;
							}
						}
						break;
					case false:
						//do nothing
						break;
					}

				}
#endif

			}
#else
			if (atlasFile.objectReferenceValue != null) {


				int baseIndent = EditorGUI.indentLevel;

				List<AtlasRegion> regions = SpineAtlasAssetInspector.GetRegions(atlasAsset.GetAtlas());
				int regionsCount = regions.Count;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.LabelField("Atlas Regions", EditorStyles.boldLabel);
					EditorGUILayout.LabelField(string.Format("{0} regions total", regionsCount));
				}
				AtlasPage lastPage = null;
				for (int i = 0; i < regionsCount; i++) {
					if (lastPage != regions[i].page) {
						if (lastPage != null) {
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
						}
						lastPage = regions[i].page;
						Material mat = ((Material)lastPage.rendererObject);
						if (mat != null) {
							EditorGUI.indentLevel = baseIndent;
							using (new GUILayout.HorizontalScope())
							using (new EditorGUI.DisabledGroupScope(true))
								EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(250));
							EditorGUI.indentLevel = baseIndent + 1;
						} else {
							EditorGUILayout.HelpBox("Page missing material!", MessageType.Warning);
						}
					}

					string regionName = regions[i].name;
					Texture2D icon = SpineEditorUtilities.Icons.image;
					if (regionName.EndsWith(" ")) {
						regionName = string.Format("'{0}'", regions[i].name);
						icon = SpineEditorUtilities.Icons.warning;
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(regionName, icon, "Region name ends with whitespace. This may cause errors. Please check your source image filenames."));
					} else {
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(regionName, icon));
					}

				}
				EditorGUI.indentLevel = baseIndent;
			}
#endif

			if (serializedObject.ApplyModifiedProperties() || SpineInspectorUtility.UndoRedoPerformed(Event.current))
				atlasAsset.Clear();
		}

		static public void UpdateSpriteSlices (Texture texture, Atlas atlas) {
			string texturePath = AssetDatabase.GetAssetPath(texture.GetInstanceID());
			TextureImporter t = (TextureImporter)TextureImporter.GetAtPath(texturePath);
			t.spriteImportMode = SpriteImportMode.Multiple;
			SpriteMetaData[] spriteSheet = t.spritesheet;
			List<SpriteMetaData> sprites = new List<SpriteMetaData>(spriteSheet);

			List<AtlasRegion> regions = SpineAtlasAssetInspector.GetRegions(atlas);
			int updatedCount = 0;
			int addedCount = 0;

			foreach (AtlasRegion r in regions) {
				string pageName = System.IO.Path.GetFileNameWithoutExtension(r.page.name);
				string textureName = texture.name;
				bool pageMatch = string.Equals(pageName, textureName, StringComparison.Ordinal);

				//				if (pageMatch) {
				//					int pw = r.page.width;
				//					int ph = r.page.height;
				//					bool mismatchSize = pw != texture.width || pw > t.maxTextureSize || ph != texture.height || ph > t.maxTextureSize;
				//					if (mismatchSize)
				//						Debug.LogWarningFormat("Size mismatch found.\nExpected atlas size is {0}x{1}. Texture Import Max Size of texture '{2}'({4}x{5}) is currently set to {3}.", pw, ph, texture.name, t.maxTextureSize, texture.width, texture.height);
				//				}

				int spriteIndex = pageMatch ? sprites.FindIndex(
					(s) => string.Equals(s.name, r.name, StringComparison.Ordinal)
				) : -1;
				bool spriteNameMatchExists = spriteIndex >= 0;

				if (pageMatch) {
					Rect spriteRect = new Rect();

					if (r.degrees == 90) {
						spriteRect.width = r.height;
						spriteRect.height = r.width;
					} else {
						spriteRect.width = r.width;
						spriteRect.height = r.height;
					}
					spriteRect.x = r.x;
					spriteRect.y = r.page.height - spriteRect.height - r.y;

					if (spriteNameMatchExists) {
						SpriteMetaData s = sprites[spriteIndex];
						s.rect = spriteRect;
						sprites[spriteIndex] = s;
						updatedCount++;
					} else {
						sprites.Add(new SpriteMetaData {
							name = r.name,
							pivot = new Vector2(0.5f, 0.5f),
							rect = spriteRect
						});
						addedCount++;
					}
				}

			}

			t.spritesheet = sprites.ToArray();
			EditorUtility.SetDirty(t);
			AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
			EditorGUIUtility.PingObject(texture);
			Debug.Log(string.Format("Applied sprite slices to {2}. {0} added. {1} updated.", addedCount, updatedCount, texture.name));
		}
	}

}
