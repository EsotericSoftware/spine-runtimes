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

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Editor = UnityEditor.Editor;

	[CustomEditor(typeof(AnimationReferenceAsset))]
	public class AnimationReferenceAssetEditor : Editor {

		const string InspectorHelpText = "This is a Spine-Unity Animation Reference Asset. It serializes a reference to a SkeletonData asset and an animationName. It does not contain actual animation data. At runtime, it stores a reference to a Spine.Animation.\n\n" +
				"You can use this in your AnimationState calls instead of a string animation name or a Spine.Animation reference. Use its implicit conversion into Spine.Animation or its .Animation property.\n\n" +
				"Use AnimationReferenceAssets as an alternative to storing strings or finding animations and caching per component. This only does the lookup by string once, and allows you to store and manage animations via asset references.";

		readonly SkeletonInspectorPreview preview = new SkeletonInspectorPreview();
		FieldInfo skeletonDataAssetField = typeof(AnimationReferenceAsset).GetField("skeletonDataAsset", BindingFlags.NonPublic | BindingFlags.Instance);
		FieldInfo nameField = typeof(AnimationReferenceAsset).GetField("animationName", BindingFlags.NonPublic | BindingFlags.Instance);

		AnimationReferenceAsset ThisAnimationReferenceAsset { get { return target as AnimationReferenceAsset; } }
		SkeletonDataAsset ThisSkeletonDataAsset { get { return skeletonDataAssetField.GetValue(ThisAnimationReferenceAsset) as SkeletonDataAsset; } }
		string ThisAnimationName { get { return nameField.GetValue(ThisAnimationReferenceAsset) as string; } }

		bool changeNextFrame = false;
		SerializedProperty animationNameProperty;
		SkeletonDataAsset lastSkeletonDataAsset;
		SkeletonData lastSkeletonData;

		void OnEnable () { HandleOnEnablePreview(); }
		void OnDestroy () {
			HandleOnDestroyPreview();
			AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
			EditorApplication.update -= preview.HandleEditorUpdate;
		}

		public override void OnInspectorGUI () {
			animationNameProperty = animationNameProperty ?? serializedObject.FindProperty("animationName");
			string animationName = animationNameProperty.stringValue;

			Animation animation = null;
			if (ThisSkeletonDataAsset != null) {
				var skeletonData = ThisSkeletonDataAsset.GetSkeletonData(true);
				if (skeletonData != null) {
					animation = skeletonData.FindAnimation(animationName);
				}
			}
			bool animationNotFound = (animation == null);

			if (changeNextFrame) {
				changeNextFrame = false;

				if (ThisSkeletonDataAsset != lastSkeletonDataAsset || ThisSkeletonDataAsset.GetSkeletonData(true) != lastSkeletonData) {
					preview.Clear();
					preview.Initialize(Repaint, ThisSkeletonDataAsset, LastSkinName);

					if (animationNotFound) {
						animationNameProperty.stringValue = "";
						preview.ClearAnimationSetupPose();
					}
				}

				preview.ClearAnimationSetupPose();

				if (!string.IsNullOrEmpty(animationNameProperty.stringValue))
					preview.PlayPauseAnimation(animationNameProperty.stringValue, true);
			}

			//EditorGUILayout.HelpBox(AnimationReferenceAssetEditor.InspectorHelpText, MessageType.Info, true);
			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			DrawDefaultInspector();
			if (EditorGUI.EndChangeCheck()) {
				changeNextFrame = true;
			}

			// Draw extra info below default inspector.
			EditorGUILayout.Space();
			if (ThisSkeletonDataAsset == null) {
				EditorGUILayout.HelpBox("SkeletonDataAsset is missing.", MessageType.Error);
			} else if (string.IsNullOrEmpty(animationName)) {
				EditorGUILayout.HelpBox("No animation selected.", MessageType.Warning);
			} else if (animationNotFound) {
				EditorGUILayout.HelpBox(string.Format("Animation named {0} was not found for this Skeleton.", animationNameProperty.stringValue), MessageType.Warning);
			} else {
				using (new SpineInspectorUtility.BoxScope()) {
					if (!string.Equals(AssetUtility.GetPathSafeName(animationName), ThisAnimationReferenceAsset.name, System.StringComparison.OrdinalIgnoreCase))
						EditorGUILayout.HelpBox("Animation name value does not match this asset's name. Inspectors using this asset may be misleading.", MessageType.None);

					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(animationName, SpineEditorUtilities.Icons.animation));
					if (animation != null) {
						EditorGUILayout.LabelField(string.Format("Timelines: {0}", animation.Timelines.Count));
						EditorGUILayout.LabelField(string.Format("Duration: {0} sec", animation.Duration));
					}
				}
			}

			lastSkeletonDataAsset = ThisSkeletonDataAsset;
			lastSkeletonData = ThisSkeletonDataAsset.GetSkeletonData(true);
		}

		#region Preview Handlers
		string TargetAssetGUID { get { return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(ThisSkeletonDataAsset)); } }
		string LastSkinKey { get { return TargetAssetGUID + "_lastSkin"; } }
		string LastSkinName { get { return EditorPrefs.GetString(LastSkinKey, ""); } }

		void HandleOnEnablePreview () {
			if (ThisSkeletonDataAsset != null && ThisSkeletonDataAsset.skeletonJSON == null)
				return;
			SpineEditorUtilities.ConfirmInitialization();

			// This handles the case where the managed editor assembly is unloaded before recompilation when code changes.
			AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
			AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;

			preview.Initialize(this.Repaint, ThisSkeletonDataAsset, LastSkinName);
			preview.PlayPauseAnimation(ThisAnimationName, true);
			preview.OnSkinChanged -= HandleOnSkinChanged;
			preview.OnSkinChanged += HandleOnSkinChanged;
			EditorApplication.update -= preview.HandleEditorUpdate;
			EditorApplication.update += preview.HandleEditorUpdate;
		}

		private void OnDomainUnload (object sender, EventArgs e) {
			OnDestroy();
		}

		private void HandleOnSkinChanged (string skinName) {
			EditorPrefs.SetString(LastSkinKey, skinName);
			preview.PlayPauseAnimation(ThisAnimationName, true);
		}

		void HandleOnDestroyPreview () {
			EditorApplication.update -= preview.HandleEditorUpdate;
			preview.OnDestroy();
		}

		override public bool HasPreviewGUI () {
			if (serializedObject.isEditingMultipleObjects) return false;
			return ThisSkeletonDataAsset != null && ThisSkeletonDataAsset.GetSkeletonData(true) != null;
		}

		override public void OnInteractivePreviewGUI (Rect r, GUIStyle background) {
			preview.Initialize(this.Repaint, ThisSkeletonDataAsset);
			preview.HandleInteractivePreviewGUI(r, background);
		}

		public override GUIContent GetPreviewTitle () { return SpineInspectorUtility.TempContent("Preview"); }
		public override void OnPreviewSettings () { preview.HandleDrawSettings(); }
		public override Texture2D RenderStaticPreview (string assetPath, UnityEngine.Object[] subAssets, int width, int height) { return preview.GetStaticPreview(width, height); }
		#endregion
	}

}
