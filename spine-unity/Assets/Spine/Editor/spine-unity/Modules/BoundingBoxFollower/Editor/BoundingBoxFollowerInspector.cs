/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using UnityEditor;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(BoundingBoxFollower))]
	public class BoundingBoxFollowerInspector : UnityEditor.Editor {
		SerializedProperty skeletonRenderer, slotName, isTrigger, clearStateOnDisable;
		BoundingBoxFollower follower;
		bool rebuildRequired = false;
		bool addBoneFollower = false;
		bool sceneRepaintRequired = false;
		bool debugIsExpanded;

		GUIContent addBoneFollowerLabel;
		GUIContent AddBoneFollowerLabel {
			get {
				if (addBoneFollowerLabel == null) addBoneFollowerLabel = new GUIContent("Add Bone Follower", Icons.bone);
				return addBoneFollowerLabel;
			}
		}

		void OnEnable () {
			skeletonRenderer = serializedObject.FindProperty("skeletonRenderer");
			slotName = serializedObject.FindProperty("slotName");
			isTrigger = serializedObject.FindProperty("isTrigger");
			clearStateOnDisable = serializedObject.FindProperty("clearStateOnDisable");
			follower = (BoundingBoxFollower)target;
		}

		public override void OnInspectorGUI () {
			bool isInspectingPrefab = (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab);

			// Try to auto-assign SkeletonRenderer field.
			if (skeletonRenderer.objectReferenceValue == null) {
				var foundSkeletonRenderer = follower.GetComponentInParent<SkeletonRenderer>();
				if (foundSkeletonRenderer != null)
					Debug.Log("BoundingBoxFollower automatically assigned: " + foundSkeletonRenderer.gameObject.name);
				else if (Event.current.type == EventType.Repaint)
					Debug.Log("No Spine GameObject detected. Make sure to set this GameObject as a child of the Spine GameObject; or set BoundingBoxFollower's 'Skeleton Renderer' field in the inspector.");

				skeletonRenderer.objectReferenceValue = foundSkeletonRenderer;
				serializedObject.ApplyModifiedProperties();
			}

			var skeletonRendererValue = skeletonRenderer.objectReferenceValue as SkeletonRenderer;
			if (skeletonRendererValue != null && skeletonRendererValue.gameObject == follower.gameObject) {
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					EditorGUILayout.HelpBox("It's ideal to add BoundingBoxFollower to a separate child GameObject of the Spine GameObject.", MessageType.Warning);

					if (GUILayout.Button(new GUIContent("Move BoundingBoxFollower to new GameObject", Icons.boundingBox), GUILayout.Height(50f))) {
						AddBoundingBoxFollowerChild(skeletonRendererValue, follower);
						DestroyImmediate(follower);
						return;
					}
				}
				EditorGUILayout.Space();
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(skeletonRenderer);
			EditorGUILayout.PropertyField(slotName, new GUIContent("Slot"));
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
				if (!isInspectingPrefab)
					rebuildRequired = true;
			}

			using (new SpineInspectorUtility.LabelWidthScope(150f)) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(isTrigger);
				bool triggerChanged = EditorGUI.EndChangeCheck();

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(clearStateOnDisable, new GUIContent(clearStateOnDisable.displayName, "Enable this if you are pooling your Spine GameObject"));
				bool clearStateChanged = EditorGUI.EndChangeCheck();

				if (clearStateChanged || triggerChanged) {
					serializedObject.ApplyModifiedProperties();
					if (triggerChanged)
						foreach (var col in follower.colliderTable.Values)
							col.isTrigger = isTrigger.boolValue;
				}
			}

			if (isInspectingPrefab) {
				follower.colliderTable.Clear();
				follower.nameTable.Clear();
				EditorGUILayout.HelpBox("BoundingBoxAttachments cannot be previewed in prefabs.", MessageType.Info);

				// How do you prevent components from being saved into the prefab? No such HideFlag. DontSaveInEditor | DontSaveInBuild does not work. DestroyImmediate does not work.
				var collider = follower.GetComponent<PolygonCollider2D>();
				if (collider != null) Debug.LogWarning("Found BoundingBoxFollower collider components in prefab. These are disposed and regenerated at runtime.");

			} else {
				using (new SpineInspectorUtility.BoxScope()) {
					if (debugIsExpanded = EditorGUILayout.Foldout(debugIsExpanded, "Debug Colliders")) {
						EditorGUI.indentLevel++;
						EditorGUILayout.LabelField(string.Format("Attachment Names ({0} PolygonCollider2D)", follower.colliderTable.Count));
						EditorGUI.BeginChangeCheck();
						foreach (var kp in follower.nameTable) {
							string attachmentName = kp.Value;
							var collider = follower.colliderTable[kp.Key];
							bool isPlaceholder = attachmentName != kp.Key.Name;
							collider.enabled = EditorGUILayout.ToggleLeft(new GUIContent(!isPlaceholder ? attachmentName : string.Format("{0} [{1}]", attachmentName, kp.Key.Name), isPlaceholder ? Icons.skinPlaceholder : Icons.boundingBox), collider.enabled);
						}
						sceneRepaintRequired |= EditorGUI.EndChangeCheck();
						EditorGUI.indentLevel--;
					}
				}

			}

			bool hasBoneFollower = follower.GetComponent<BoneFollower>() != null;
			if (!hasBoneFollower) {
				bool buttonDisabled = follower.Slot == null;
				using (new EditorGUI.DisabledGroupScope(buttonDisabled)) {
					addBoneFollower |= SpineInspectorUtility.LargeCenteredButton(AddBoneFollowerLabel, true);
					EditorGUILayout.Space();
				}
			}


			if (Event.current.type == EventType.Repaint) {
				if (addBoneFollower) {
					var boneFollower = follower.gameObject.AddComponent<BoneFollower>();
					boneFollower.skeletonRenderer = skeletonRendererValue;
					boneFollower.SetBone(follower.Slot.Data.BoneData.Name);
					addBoneFollower = false;
				}

				if (sceneRepaintRequired) {
					SceneView.RepaintAll();
					sceneRepaintRequired = false;
				}

				if (rebuildRequired) {
					follower.Initialize();
					rebuildRequired = false;
				}
			}
		}

		#region Menus
		[MenuItem("CONTEXT/SkeletonRenderer/Add BoundingBoxFollower GameObject")]
		static void AddBoundingBoxFollowerChild (MenuCommand command) {
			var go = AddBoundingBoxFollowerChild((SkeletonRenderer)command.context);
			Undo.RegisterCreatedObjectUndo(go, "Add BoundingBoxFollower");
		}
		#endregion

		static GameObject AddBoundingBoxFollowerChild (SkeletonRenderer sr, BoundingBoxFollower original = null) {
			var go = new GameObject("BoundingBoxFollower");
			go.transform.SetParent(sr.transform, false);
			var newFollower = go.AddComponent<BoundingBoxFollower>();

			if (original != null) {
				newFollower.slotName = original.slotName;
				newFollower.isTrigger = original.isTrigger;
				newFollower.clearStateOnDisable = original.clearStateOnDisable;
			}

			newFollower.skeletonRenderer = sr;
			newFollower.Initialize();


			Selection.activeGameObject = go;
			EditorGUIUtility.PingObject(go);
			return go;
		}

	}

}
