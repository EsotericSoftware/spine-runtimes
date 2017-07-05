using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Spine.Unity;

namespace Spine.Unity.Editor {

	using Editor = UnityEditor.Editor;
	using Event = UnityEngine.Event;

	[CustomEditor(typeof(BoneFollowerGraphic)), CanEditMultipleObjects]
	public class BoneFollowerGraphicInspector : Editor {
		
		SerializedProperty boneName, skeletonGraphic, followZPosition, followBoneRotation, followLocalScale, followSkeletonFlip;
		BoneFollowerGraphic targetBoneFollower;
		bool needsReset;

		#region Context Menu Item
		[MenuItem ("CONTEXT/SkeletonGraphic/Add BoneFollower GameObject")]
		static void AddBoneFollowerGameObject (MenuCommand cmd) {
			var skeletonGraphic = cmd.context as SkeletonGraphic;
			var go = new GameObject("BoneFollower", typeof(RectTransform));
			var t = go.transform;
			t.SetParent(skeletonGraphic.transform);
			t.localPosition = Vector3.zero;

			var f = go.AddComponent<BoneFollowerGraphic>();
			f.skeletonGraphic = skeletonGraphic;
			f.SetBone(skeletonGraphic.Skeleton.RootBone.Data.Name);

			EditorGUIUtility.PingObject(t);

			Undo.RegisterCreatedObjectUndo(go, "Add BoneFollowerGraphic");
		}

		// Validate
		[MenuItem ("CONTEXT/SkeletonGraphic/Add BoneFollower GameObject", true)]
		static bool ValidateAddBoneFollowerGameObject (MenuCommand cmd) {
			var skeletonGraphic = cmd.context as SkeletonGraphic;
			return skeletonGraphic.IsValid;
		}
		#endregion

		void OnEnable () {
			skeletonGraphic = serializedObject.FindProperty("skeletonGraphic");
			boneName = serializedObject.FindProperty("boneName");
			followBoneRotation = serializedObject.FindProperty("followBoneRotation");
			followZPosition = serializedObject.FindProperty("followZPosition");
			followLocalScale = serializedObject.FindProperty("followLocalScale");
			followSkeletonFlip = serializedObject.FindProperty("followSkeletonFlip");

			targetBoneFollower = (BoneFollowerGraphic)target;
			if (targetBoneFollower.SkeletonGraphic != null)
				targetBoneFollower.SkeletonGraphic.Initialize(false);

			if (!targetBoneFollower.valid || needsReset) {
				targetBoneFollower.Initialize();
				targetBoneFollower.LateUpdate();
				needsReset = false;
				SceneView.RepaintAll();
			}
		}

		public void OnSceneGUI () {
			var tbf = target as BoneFollowerGraphic;
			var skeletonGraphicComponent = tbf.SkeletonGraphic;
			if (skeletonGraphicComponent == null) return;

			var transform = skeletonGraphicComponent.transform;
			var skeleton = skeletonGraphicComponent.Skeleton;
			var canvas = skeletonGraphicComponent.canvas;
			float positionScale = canvas == null ? 1f : skeletonGraphicComponent.canvas.referencePixelsPerUnit;

			if (string.IsNullOrEmpty(boneName.stringValue)) {
				SpineHandles.DrawBones(transform, skeleton, positionScale);
				SpineHandles.DrawBoneNames(transform, skeleton, positionScale);
				Handles.Label(tbf.transform.position, "No bone selected", EditorStyles.helpBox);
			} else {
				var targetBone = tbf.bone;
				if (targetBone == null) return;
				
				SpineHandles.DrawBoneWireframe(transform, targetBone, SpineHandles.TransformContraintColor, positionScale);
				Handles.Label(targetBone.GetWorldPosition(transform, positionScale), targetBone.Data.Name, SpineHandles.BoneNameStyle);
			}
		}

		override public void OnInspectorGUI () {
			if (serializedObject.isEditingMultipleObjects) {
				if (needsReset) {
					needsReset = false;
					foreach (var o in targets) {
						var bf = (BoneFollower)o;
						bf.Initialize();
						bf.LateUpdate();
					}
					SceneView.RepaintAll();
				}

				EditorGUI.BeginChangeCheck();
				DrawDefaultInspector();
				needsReset |= EditorGUI.EndChangeCheck();
				return;
			}

			if (needsReset && Event.current.type == EventType.Layout) {
				targetBoneFollower.Initialize();
				targetBoneFollower.LateUpdate();
				needsReset = false;
				SceneView.RepaintAll();
			}
			serializedObject.Update();

			// Find Renderer
			if (skeletonGraphic.objectReferenceValue == null) {
				SkeletonGraphic parentRenderer = targetBoneFollower.GetComponentInParent<SkeletonGraphic>();
				if (parentRenderer != null && parentRenderer.gameObject != targetBoneFollower.gameObject) {
					skeletonGraphic.objectReferenceValue = parentRenderer;
					Debug.Log("Inspector automatically assigned BoneFollowerGraphic.SkeletonGraphic");
				}
			}

			EditorGUILayout.PropertyField(skeletonGraphic);
			var skeletonGraphicComponent = skeletonGraphic.objectReferenceValue as SkeletonGraphic;
			if (skeletonGraphicComponent != null) {
				if (skeletonGraphicComponent.gameObject == targetBoneFollower.gameObject) {
					skeletonGraphic.objectReferenceValue = null;
					EditorUtility.DisplayDialog("Invalid assignment.", "BoneFollowerGraphic can only follow a skeleton on a separate GameObject.\n\nCreate a new GameObject for your BoneFollower, or choose a SkeletonGraphic from a different GameObject.", "Ok");
				}
			}

			if (!targetBoneFollower.valid) {
				needsReset = true;
			}

			if (targetBoneFollower.valid) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(boneName);
				needsReset |= EditorGUI.EndChangeCheck();

				EditorGUILayout.PropertyField(followBoneRotation);
				EditorGUILayout.PropertyField(followZPosition);
				EditorGUILayout.PropertyField(followLocalScale);
				EditorGUILayout.PropertyField(followSkeletonFlip);

				//BoneFollowerInspector.RecommendRigidbodyButton(targetBoneFollower);
			} else {
				var boneFollowerSkeletonGraphic = targetBoneFollower.skeletonGraphic;
				if (boneFollowerSkeletonGraphic == null) {
					EditorGUILayout.HelpBox("SkeletonGraphic is unassigned. Please assign a SkeletonRenderer (SkeletonAnimation or SkeletonAnimator).", MessageType.Warning);
				} else {
					boneFollowerSkeletonGraphic.Initialize(false);

					if (boneFollowerSkeletonGraphic.skeletonDataAsset == null)
						EditorGUILayout.HelpBox("Assigned SkeletonGraphic does not have SkeletonData assigned to it.", MessageType.Warning);

					if (!boneFollowerSkeletonGraphic.IsValid)
						EditorGUILayout.HelpBox("Assigned SkeletonGraphic is invalid. Check target SkeletonGraphic, its SkeletonDataAsset or the console for other errors.", MessageType.Warning);
				}
			}

			var current = Event.current;
			bool wasUndo = (current.type == EventType.ValidateCommand && current.commandName == "UndoRedoPerformed");
			if (wasUndo)
				targetBoneFollower.Initialize();

			serializedObject.ApplyModifiedProperties();
		}

	}
}