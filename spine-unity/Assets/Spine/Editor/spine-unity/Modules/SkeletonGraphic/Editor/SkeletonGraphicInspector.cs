/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using UnityEditor;
using Spine;

namespace Spine.Unity.Editor {

	[InitializeOnLoad]
	[CustomEditor(typeof(SkeletonGraphic))]
	[CanEditMultipleObjects]
	public class SkeletonGraphicInspector : UnityEditor.Editor {
		SerializedProperty material, color;
		SerializedProperty skeletonDataAsset, initialSkinName;
		SerializedProperty startingAnimation, startingLoop, timeScale, freeze, unscaledTime, tintBlack;
		SerializedProperty initialFlipX, initialFlipY;
		SerializedProperty meshGeneratorSettings;
		SerializedProperty raycastTarget;

		SkeletonGraphic thisSkeletonGraphic;

		void OnEnable () {
			var so = this.serializedObject;
			thisSkeletonGraphic = target as SkeletonGraphic;

			// MaskableGraphic
			material = so.FindProperty("m_Material");
			color = so.FindProperty("m_Color");
			raycastTarget = so.FindProperty("m_RaycastTarget");

			// SkeletonRenderer
			skeletonDataAsset = so.FindProperty("skeletonDataAsset");
			initialSkinName = so.FindProperty("initialSkinName");

			initialFlipX = so.FindProperty("initialFlipX");
			initialFlipY = so.FindProperty("initialFlipY");

			// SkeletonAnimation
			startingAnimation = so.FindProperty("startingAnimation");
			startingLoop = so.FindProperty("startingLoop");
			timeScale = so.FindProperty("timeScale");
			unscaledTime = so.FindProperty("unscaledTime");
			freeze = so.FindProperty("freeze");

			meshGeneratorSettings = so.FindProperty("meshGenerator").FindPropertyRelative("settings");
			meshGeneratorSettings.isExpanded = SkeletonRendererInspector.advancedFoldout;
		}

		public override void OnInspectorGUI () {
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(skeletonDataAsset);
			EditorGUILayout.PropertyField(material);
			EditorGUILayout.PropertyField(color);

			if (thisSkeletonGraphic.skeletonDataAsset == null) {
				EditorGUILayout.HelpBox("You need to assign a SkeletonDataAsset first.", MessageType.Info);
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				return;
			}
			using (new SpineInspectorUtility.BoxScope()) {
				EditorGUILayout.PropertyField(meshGeneratorSettings, SpineInspectorUtility.TempContent("Advanced..."), includeChildren: true);
				SkeletonRendererInspector.advancedFoldout = meshGeneratorSettings.isExpanded;
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(initialSkinName);
			{
				var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
				EditorGUI.PrefixLabel(rect, SpineInspectorUtility.TempContent("Initial Flip"));
				rect.x += EditorGUIUtility.labelWidth;
				rect.width = 30f;
				SpineInspectorUtility.ToggleLeft(rect, initialFlipX, SpineInspectorUtility.TempContent("X", tooltip: "initialFlipX"));
				rect.x += 35f;
				SpineInspectorUtility.ToggleLeft(rect, initialFlipY, SpineInspectorUtility.TempContent("Y", tooltip: "initialFlipY"));
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(startingAnimation);
			EditorGUILayout.PropertyField(startingLoop);
			EditorGUILayout.PropertyField(timeScale);
			EditorGUILayout.PropertyField(unscaledTime, SpineInspectorUtility.TempContent(unscaledTime.displayName, tooltip: "If checked, this will use Time.unscaledDeltaTime to make this update independent of game Time.timeScale. Instance SkeletonGraphic.timeScale will still be applied."));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(freeze);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(raycastTarget);

			bool wasChanged = EditorGUI.EndChangeCheck();

			if (wasChanged)
				serializedObject.ApplyModifiedProperties();
		}

		#region Menus
		[MenuItem("CONTEXT/SkeletonGraphic/Match RectTransform with Mesh Bounds")]
		static void MatchRectTransformWithBounds (MenuCommand command) {
			var skeletonGraphic = (SkeletonGraphic)command.context;
			Mesh mesh = skeletonGraphic.GetLastMesh();
			if (mesh == null) {
				Debug.Log("Mesh was not previously generated.");
				return;
			}

			if (mesh.vertexCount == 0) {
				skeletonGraphic.rectTransform.sizeDelta = new Vector2(50f, 50f);
				skeletonGraphic.rectTransform.pivot = new Vector2(0.5f, 0.5f);
				return;
			}

			mesh.RecalculateBounds();
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

		[MenuItem("GameObject/Spine/SkeletonGraphic (UnityUI)", false, 15)]
		static public void SkeletonGraphicCreateMenuItem () {
			var parentGameObject = Selection.activeObject as GameObject;
			var parentTransform = parentGameObject == null ? null : parentGameObject.GetComponent<RectTransform>();

			if (parentTransform == null)
				Debug.LogWarning("Your new SkeletonGraphic will not be visible until it is placed under a Canvas");

			var gameObject = NewSkeletonGraphicGameObject("New SkeletonGraphic");
			gameObject.transform.SetParent(parentTransform, false);
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = gameObject;
			EditorGUIUtility.PingObject(Selection.activeObject);
		}

		// SpineEditorUtilities.InstantiateDelegate. Used by drag and drop.
		public static Component SpawnSkeletonGraphicFromDrop (SkeletonDataAsset data) {
			return InstantiateSkeletonGraphic(data);
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
					skeletonDataAsset.atlasAssets[i] = (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAssetBase));
				}

				data = skeletonDataAsset.GetSkeletonData(true);
			}

			skin = skin ?? data.DefaultSkin ?? data.Skins.Items[0];
			graphic.MeshGenerator.settings.zSpacing = SpineEditorUtilities.Preferences.defaultZSpacing;

			graphic.startingLoop = SpineEditorUtilities.Preferences.defaultInstantiateLoop;
			graphic.Initialize(false);
			if (skin != null) graphic.Skeleton.SetSkin(skin);
			graphic.initialSkinName = skin.Name;
			graphic.Skeleton.UpdateWorldTransform();
			graphic.UpdateMesh();

			return graphic;
		}

		static GameObject NewSkeletonGraphicGameObject (string gameObjectName) {
			var go = SpineEditorUtilities.EditorInstantiation.NewGameObject(gameObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(SkeletonGraphic));
			var graphic = go.GetComponent<SkeletonGraphic>();
			graphic.material = SkeletonGraphicInspector.DefaultSkeletonGraphicMaterial;
			return go;
		}

		public static Material DefaultSkeletonGraphicMaterial {
			get {
				var guids = AssetDatabase.FindAssets("SkeletonGraphicDefault t:material");
				if (guids.Length <= 0) return null;

				var firstAssetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
				if (string.IsNullOrEmpty(firstAssetPath)) return null;

				var firstMaterial = AssetDatabase.LoadAssetAtPath<Material>(firstAssetPath);
				return firstMaterial;
			}
		}

		#endregion
	}
}
