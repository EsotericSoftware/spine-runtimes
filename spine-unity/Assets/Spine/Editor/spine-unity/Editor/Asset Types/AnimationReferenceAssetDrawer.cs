/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	[CustomPropertyDrawer(typeof(AnimationReferenceAsset))]
	public class AnimationReferenceAssetDrawer : PropertyDrawer {

		const string NoneString = "<None>";
		const string ReferenceAssetsFolderName = SpineEditorUtilities.ReferenceAssetsFolderName;

		static GUIStyle errorPopupStyle;
		GUIStyle ErrorPopupStyle {
			get {
				if (errorPopupStyle == null) errorPopupStyle = new GUIStyle(EditorStyles.popup);
				errorPopupStyle.normal.textColor = Color.red;
				errorPopupStyle.hover.textColor = Color.red;
				errorPopupStyle.focused.textColor = Color.red;
				errorPopupStyle.active.textColor = Color.red;
				return errorPopupStyle;
			}
		}

		SkeletonDataAsset resolvedSkeletonDataAsset;

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType != SerializedPropertyType.ObjectReference) {
				EditorGUI.LabelField(position, label.text, "ERROR: Must be an object reference.");
				return;
			}

			bool isSkeletonDataMismatch;
			SkeletonDataAsset skeletonDataAsset = ResolveSkeletonDataAsset(property, out isSkeletonDataMismatch);
			SkeletonData skeletonData = skeletonDataAsset != null ? skeletonDataAsset.GetSkeletonData(true) : null;
			label = EditorGUI.BeginProperty(position, label, property);

			if (skeletonData == null || skeletonData.Animations.Count == 0) {
				EditorGUI.PropertyField(position, property, label);
			} else {
				Rect objectFieldRect;
				Rect dropdownRect;
				SplitRect(position, out objectFieldRect, out dropdownRect);

				position = EditorGUI.PrefixLabel(position, label);
				SplitRect(position, out objectFieldRect, out dropdownRect);

				EditorGUI.PropertyField(objectFieldRect, property, GUIContent.none);

				string currentAnimationName = GetAnimationName(property);
				GUIContent dropdownLabel = string.IsNullOrEmpty(currentAnimationName) ?
					new GUIContent(NoneString, SpineEditorUtilities.Icons.animation) :
					new GUIContent(currentAnimationName, SpineEditorUtilities.Icons.animation);

				GUIStyle usedStyle =
					(isSkeletonDataMismatch && SpineEditorUtilities.Preferences.skeletonDataAssetMismatchWarning) ?
					ErrorPopupStyle : EditorStyles.popup;
				if (GUI.Button(dropdownRect, dropdownLabel, usedStyle)) {
					ShowAnimationMenu(property, skeletonDataAsset, skeletonData);
				}
			}
			EditorGUI.EndProperty();
		}

		static void SplitRect (Rect position, out Rect left, out Rect right) {
			float dropdownWidth = Mathf.Min(position.width * 0.5f, 200);
			left = new Rect(position.x, position.y, position.width - dropdownWidth - 2, position.height);
			right = new Rect(position.x + position.width - dropdownWidth, position.y, dropdownWidth, position.height);
		}

		SkeletonDataAsset ResolveSkeletonDataAsset (SerializedProperty property, out bool skeletonDataAssetMismatch) {
			skeletonDataAssetMismatch = false;
			SkeletonDataAsset expectedSkeletonDataAsset = GetSkeletonDataAssetFromGameObject(property);

			AnimationReferenceAsset currentAsset = property.objectReferenceValue as AnimationReferenceAsset;
			if (currentAsset != null && currentAsset.SkeletonDataAsset != null) {
				resolvedSkeletonDataAsset = currentAsset.SkeletonDataAsset;
				// If other SkeletonDataAsset than expected, use assigned asset but show warning color in Inspector.
				if (expectedSkeletonDataAsset && resolvedSkeletonDataAsset != expectedSkeletonDataAsset)
					skeletonDataAssetMismatch = true;
			} else {
				resolvedSkeletonDataAsset = expectedSkeletonDataAsset;
			}
			return resolvedSkeletonDataAsset;
		}

		SkeletonDataAsset GetSkeletonDataAssetFromGameObject (SerializedProperty property) {
			Object targetObject = property.serializedObject.targetObject;
			IHasSkeletonDataAsset skeletonDataAssetComponent = targetObject as IHasSkeletonDataAsset;
			if (skeletonDataAssetComponent == null) {
				Component component = targetObject as Component;
				if (component != null)
					skeletonDataAssetComponent = component.GetComponentInParent<IHasSkeletonDataAsset>();
			}
			if (skeletonDataAssetComponent != null) {
				return skeletonDataAssetComponent.SkeletonDataAsset;
			}
			return null;
		}

		static string GetAnimationName (SerializedProperty property) {
			AnimationReferenceAsset asset = property.objectReferenceValue as AnimationReferenceAsset;
			if (asset == null) return null;
			return asset.AnimationName;
		}

		void ShowAnimationMenu (SerializedProperty property, SkeletonDataAsset skeletonDataAsset, SkeletonData skeletonData) {
			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent(NoneString), property.objectReferenceValue == null, () => {
				property.objectReferenceValue = null;
				property.serializedObject.ApplyModifiedProperties();
			});

			ExposedList<Animation> animations = skeletonData.Animations;
			string currentAnimationName = GetAnimationName(property);

			for (int i = 0; i < animations.Count; i++) {
				string animName = animations.Items[i].Name;
				bool isSelected = animName == currentAnimationName;
				menu.AddItem(new GUIContent(animName), isSelected, HandleAnimationSelected,
					new AnimationSelectContext(property, skeletonDataAsset, animName));
			}

			menu.ShowAsContext();
		}

		static void HandleAnimationSelected (object contextObj) {
			AnimationSelectContext context = (AnimationSelectContext)contextObj;
			AnimationReferenceAsset asset = FindAnimationReferenceAsset(context.skeletonDataAsset, context.animationName);
			if (asset == null) {
				Debug.LogWarning(string.Format("AnimationReferenceAsset for animation '{0}' not found. " +
					"Please select the SkeletonDataAsset and in the Inspector hit 'Create Animation Reference " +
					"Assets', or manually assign an AnimationReferenceAsset if using shared AnimationReferenceAssets.",
					context.animationName), context.skeletonDataAsset);
				return;
			}
			context.property.objectReferenceValue = asset;
			context.property.serializedObject.ApplyModifiedProperties();
		}

		static AnimationReferenceAsset FindAnimationReferenceAsset (SkeletonDataAsset targetSkeletonDataAsset,
			string targetAnimationName) {

			string skeletonDataAssetPath = AssetDatabase.GetAssetPath(targetSkeletonDataAsset);
			string parentFolder = Path.GetDirectoryName(skeletonDataAssetPath);

			// Search AnimationReferenceAssetContainer sub-assets
			string skeletonDataAssetName = Path.GetFileNameWithoutExtension(skeletonDataAssetPath);
			string baseName = skeletonDataAssetName.Replace(AssetUtility.SkeletonDataSuffix, "");
			string containerPath = string.Format("{0}/{1}{2}.asset", parentFolder, baseName,
				SpineEditorUtilities.AnimationReferenceContainerSuffix);
			AnimationReferenceAsset foundAsset = FindAnimationReferenceInSubAssets(containerPath, targetSkeletonDataAsset, targetAnimationName);
			if (foundAsset != null) return foundAsset;

			// Search standalone files in same asset directory
			string dataPath = parentFolder + "/" + ReferenceAssetsFolderName;
			string safeName = AssetUtility.GetPathSafeName(targetAnimationName);
			string assetPath = string.Format("{0}/{1}.asset", dataPath, safeName);
			AnimationReferenceAsset existingAsset = AssetDatabase.LoadAssetAtPath<AnimationReferenceAsset>(assetPath);
			if (existingAsset != null) return existingAsset;

			// Global fallback: search the project for matching AnimationReferenceAsset including sub-assets
			string[] guids = AssetDatabase.FindAssets("t:AnimationReferenceAsset");
			foreach (string guid in guids) {
				string path = AssetDatabase.GUIDToAssetPath(guid);
				foundAsset = FindAnimationReferenceInSubAssets(path, targetSkeletonDataAsset, targetAnimationName);
				if (foundAsset != null) return foundAsset;
			}
			return null;
		}

		static AnimationReferenceAsset FindAnimationReferenceInSubAssets (string assetPath,
			SkeletonDataAsset targetSkeletonDataAsset, string targetAnimationName) {

			UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			foreach (UnityEngine.Object obj in allAssets) {
				AnimationReferenceAsset asset = obj as AnimationReferenceAsset;
				if (asset == null) continue;
				if (asset.SkeletonDataAsset == targetSkeletonDataAsset && asset.AnimationName == targetAnimationName)
					return asset;
			}
			return null;
		}

		struct AnimationSelectContext {
			public SerializedProperty property;
			public SkeletonDataAsset skeletonDataAsset;
			public string animationName;

			public AnimationSelectContext (SerializedProperty property, SkeletonDataAsset skeletonDataAsset, string animationName) {
				this.property = property;
				this.skeletonDataAsset = skeletonDataAsset;
				this.animationName = animationName;
			}
		}
	}
}
