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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(BoundingBoxFollower))]
	public class BoundingBoxFollowerInspector : UnityEditor.Editor {
		SerializedProperty skeletonRenderer, slotName,
			isTrigger, usedByEffector, usedByComposite, clearStateOnDisable;
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

		void InitializeEditor () {
			skeletonRenderer = serializedObject.FindProperty("skeletonRenderer");
			slotName = serializedObject.FindProperty("slotName");
			isTrigger = serializedObject.FindProperty("isTrigger");
			usedByEffector = serializedObject.FindProperty("usedByEffector");
			usedByComposite = serializedObject.FindProperty("usedByComposite");
			clearStateOnDisable = serializedObject.FindProperty("clearStateOnDisable");
			follower = (BoundingBoxFollower)target;
		}

		public override void OnInspectorGUI () {

#if !NEW_PREFAB_SYSTEM
			bool isInspectingPrefab = (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab);
#else
			bool isInspectingPrefab = false;
#endif

			// Note: when calling InitializeEditor() in OnEnable, it throws exception
			// "SerializedObjectNotCreatableException: Object at index 0 is null".
			InitializeEditor();

			// Try to auto-assign SkeletonRenderer field.
			if (skeletonRenderer.objectReferenceValue == null) {
				SkeletonRenderer foundSkeletonRenderer = follower.GetComponentInParent<SkeletonRenderer>();
				if (foundSkeletonRenderer != null)
					Debug.Log("BoundingBoxFollower automatically assigned: " + foundSkeletonRenderer.gameObject.name);
				else if (Event.current.type == EventType.Repaint)
					Debug.Log("No Spine GameObject detected. Make sure to set this GameObject as a child of the Spine GameObject; or set BoundingBoxFollower's 'Skeleton Renderer' field in the inspector.");

				skeletonRenderer.objectReferenceValue = foundSkeletonRenderer;
				serializedObject.ApplyModifiedProperties();
				InitializeEditor();
			}

			SkeletonRenderer skeletonRendererValue = skeletonRenderer.objectReferenceValue as SkeletonRenderer;
			if (skeletonRendererValue != null && skeletonRendererValue.gameObject == follower.gameObject) {
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					EditorGUILayout.HelpBox("It's ideal to add BoundingBoxFollower to a separate child GameObject of the Spine GameObject.", MessageType.Warning);

					if (GUILayout.Button(new GUIContent("Move BoundingBoxFollower to new GameObject", Icons.boundingBox), GUILayout.Height(30f))) {
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
				InitializeEditor();
#if !NEW_PREFAB_SYSTEM
				if (!isInspectingPrefab)
					rebuildRequired = true;
#endif
			}

			using (new SpineInspectorUtility.LabelWidthScope(150f)) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(isTrigger);
				EditorGUILayout.PropertyField(usedByEffector);
				EditorGUILayout.PropertyField(usedByComposite);
				bool colliderParamChanged = EditorGUI.EndChangeCheck();

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(clearStateOnDisable, new GUIContent(clearStateOnDisable.displayName, "Enable this if you are pooling your Spine GameObject"));
				bool clearStateChanged = EditorGUI.EndChangeCheck();

				if (clearStateChanged || colliderParamChanged) {
					serializedObject.ApplyModifiedProperties();
					InitializeEditor();
					if (colliderParamChanged)
						foreach (PolygonCollider2D col in follower.colliderTable.Values) {
							col.isTrigger = isTrigger.boolValue;
							col.usedByEffector = usedByEffector.boolValue;
							col.usedByComposite = usedByComposite.boolValue;
						}
				}
			}

			if (isInspectingPrefab) {
				follower.colliderTable.Clear();
				follower.nameTable.Clear();
				EditorGUILayout.HelpBox("BoundingBoxAttachments cannot be previewed in prefabs.", MessageType.Info);

				// How do you prevent components from being saved into the prefab? No such HideFlag. DontSaveInEditor | DontSaveInBuild does not work. DestroyImmediate does not work.
				PolygonCollider2D collider = follower.GetComponent<PolygonCollider2D>();
				if (collider != null) Debug.LogWarning("Found BoundingBoxFollower collider components in prefab. These are disposed and regenerated at runtime.");

			} else {
				using (new SpineInspectorUtility.BoxScope()) {
					if (debugIsExpanded = EditorGUILayout.Foldout(debugIsExpanded, "Debug Colliders")) {
						EditorGUI.indentLevel++;
						EditorGUILayout.LabelField(string.Format("Attachment Names ({0} PolygonCollider2D)", follower.colliderTable.Count));
						EditorGUI.BeginChangeCheck();
						foreach (KeyValuePair<BoundingBoxAttachment, string> pair in follower.nameTable) {
							string attachmentName = pair.Value;
							PolygonCollider2D collider = follower.colliderTable[pair.Key];
							bool isPlaceholder = attachmentName != pair.Key.Name;
							collider.enabled = EditorGUILayout.ToggleLeft(new GUIContent(!isPlaceholder ? attachmentName : string.Format("{0} [{1}]", attachmentName, pair.Key.Name), isPlaceholder ? Icons.skinPlaceholder : Icons.boundingBox), collider.enabled);
						}
						sceneRepaintRequired |= EditorGUI.EndChangeCheck();
						EditorGUI.indentLevel--;
					}
				}

			}

			if (follower.Slot == null)
				follower.Initialize(false);
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
					BoneFollower boneFollower = follower.gameObject.AddComponent<BoneFollower>();
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
			GameObject go = AddBoundingBoxFollowerChild((SkeletonRenderer)command.context);
			Undo.RegisterCreatedObjectUndo(go, "Add BoundingBoxFollower");
		}

		[MenuItem("CONTEXT/SkeletonRenderer/Add all BoundingBoxFollower GameObjects")]
		static void AddAllBoundingBoxFollowerChildren (MenuCommand command) {
			List<GameObject> objects = AddAllBoundingBoxFollowerChildren((SkeletonRenderer)command.context);
			foreach (GameObject go in objects)
				Undo.RegisterCreatedObjectUndo(go, "Add BoundingBoxFollower");
		}
		#endregion

		public static GameObject AddBoundingBoxFollowerChild (SkeletonRenderer skeletonRenderer,
			BoundingBoxFollower original = null, string name = "BoundingBoxFollower",
			string slotName = null) {

			GameObject go = EditorInstantiation.NewGameObject(name, true);
			go.transform.SetParent(skeletonRenderer.transform, false);
			BoundingBoxFollower newFollower = go.AddComponent<BoundingBoxFollower>();

			if (original != null) {
				newFollower.slotName = original.slotName;
				newFollower.isTrigger = original.isTrigger;
				newFollower.usedByEffector = original.usedByEffector;
				newFollower.usedByComposite = original.usedByComposite;
				newFollower.clearStateOnDisable = original.clearStateOnDisable;
			}
			if (slotName != null)
				newFollower.slotName = slotName;

			newFollower.skeletonRenderer = skeletonRenderer;
			newFollower.Initialize();

			Selection.activeGameObject = go;
			EditorGUIUtility.PingObject(go);
			return go;
		}

		public static List<GameObject> AddAllBoundingBoxFollowerChildren (
			SkeletonRenderer skeletonRenderer, BoundingBoxFollower original = null) {

			List<GameObject> createdGameObjects = new List<GameObject>();
			foreach (Skin skin in skeletonRenderer.Skeleton.Data.Skins) {
				ICollection<Skin.SkinEntry> attachments = skin.Attachments;
				foreach (Skin.SkinEntry entry in attachments) {
					BoundingBoxAttachment boundingBoxAttachment = entry.Attachment as BoundingBoxAttachment;
					if (boundingBoxAttachment == null)
						continue;
					int slotIndex = entry.SlotIndex;
					Slot slot = skeletonRenderer.Skeleton.Slots.Items[slotIndex];
					string slotName = slot.Data.Name;
					GameObject go = AddBoundingBoxFollowerChild(skeletonRenderer,
						original, boundingBoxAttachment.Name, slotName);
					BoneFollower boneFollower = go.AddComponent<BoneFollower>();
					boneFollower.skeletonRenderer = skeletonRenderer;
					boneFollower.SetBone(slot.Data.BoneData.Name);
					createdGameObjects.Add(go);
				}
			}
			return createdGameObjects;
		}
	}

}
