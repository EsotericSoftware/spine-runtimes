

/*****************************************************************************
 * Skeleton Utility created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Spine;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonUtilityBone)), CanEditMultipleObjects]
	public class SkeletonUtilityBoneInspector : UnityEditor.Editor {
		SerializedProperty mode, boneName, zPosition, position, rotation, scale, overrideAlpha, parentReference;

		//multi selected flags
		bool containsFollows, containsOverrides, multiObject;

		//single selected helpers
		SkeletonUtilityBone utilityBone;
		SkeletonUtility skeletonUtility;
		bool canCreateHingeChain = false;

		Dictionary<Slot, List<BoundingBoxAttachment>> boundingBoxTable = new Dictionary<Slot, List<BoundingBoxAttachment>>();
		string currentSkinName = "";

		void OnEnable () {
			mode = this.serializedObject.FindProperty("mode");
			boneName = this.serializedObject.FindProperty("boneName");
			zPosition = this.serializedObject.FindProperty("zPosition");
			position = this.serializedObject.FindProperty("position");
			rotation = this.serializedObject.FindProperty("rotation");
			scale = this.serializedObject.FindProperty("scale");
			overrideAlpha = this.serializedObject.FindProperty("overrideAlpha");
			parentReference = this.serializedObject.FindProperty("parentReference");

			EvaluateFlags();

			if (utilityBone.valid == false && skeletonUtility != null && skeletonUtility.skeletonRenderer != null)
				skeletonUtility.skeletonRenderer.Initialize(false);

			canCreateHingeChain = CanCreateHingeChain();

			boundingBoxTable.Clear();

			if (multiObject)
				return;

			if (utilityBone.bone == null)
				return;

			var skeleton = utilityBone.bone.Skeleton;
			int slotCount = skeleton.Slots.Count;
			Skin skin = skeleton.Skin;
			if (skeleton.Skin == null)
				skin = skeleton.Data.DefaultSkin;

			currentSkinName = skin.Name;
			for(int i = 0; i < slotCount; i++){
				Slot slot = skeletonUtility.skeletonRenderer.skeleton.Slots.Items[i];
				if (slot.Bone == utilityBone.bone) {
					var slotAttachments = new List<Attachment>();
					skin.FindAttachmentsForSlot(skeleton.FindSlotIndex(slot.Data.Name), slotAttachments);
					var boundingBoxes = new List<BoundingBoxAttachment>();
					foreach (var att in slotAttachments) {
						var boundingBoxAttachment = att as BoundingBoxAttachment;
						if (boundingBoxAttachment != null)
							boundingBoxes.Add(boundingBoxAttachment);
					}

					if (boundingBoxes.Count > 0) {
						boundingBoxTable.Add(slot, boundingBoxes);
					}
				}
			}

		}

		void EvaluateFlags () {
			utilityBone = (SkeletonUtilityBone)target;
			skeletonUtility = utilityBone.skeletonUtility;

			if (Selection.objects.Length == 1) {
				containsFollows = utilityBone.mode == SkeletonUtilityBone.Mode.Follow;
				containsOverrides = utilityBone.mode == SkeletonUtilityBone.Mode.Override;
			} else {
				int boneCount = 0;
				foreach (Object o in Selection.objects) {
					var go = o as GameObject;
					if (go != null) {
						SkeletonUtilityBone sub = go.GetComponent<SkeletonUtilityBone>();
						if (sub != null) {
							boneCount++;
							containsFollows |= (sub.mode == SkeletonUtilityBone.Mode.Follow);
							containsOverrides |= (sub.mode == SkeletonUtilityBone.Mode.Override);
						}
					}
				}

				if (boneCount > 1)
					multiObject = true;
			}
		}

		public override void OnInspectorGUI () {
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(mode);
			if (EditorGUI.EndChangeCheck()) {
				containsOverrides = mode.enumValueIndex == 1;
				containsFollows = mode.enumValueIndex == 0;
			}

			using (new EditorGUI.DisabledGroupScope(multiObject)) {
				string str = boneName.stringValue;
				if (str == "")
					str = "<None>";
				if (multiObject)
					str = "<Multiple>";

				using (new GUILayout.HorizontalScope()) {
					EditorGUILayout.PrefixLabel("Bone");
					if (GUILayout.Button(str, EditorStyles.popup)) {
						BoneSelectorContextMenu(str, ((SkeletonUtilityBone)target).skeletonUtility.skeletonRenderer.skeleton.Bones, "<None>", TargetBoneSelected);
					}
				}
			}

			EditorGUILayout.PropertyField(zPosition);
			EditorGUILayout.PropertyField(position);
			EditorGUILayout.PropertyField(rotation);
			EditorGUILayout.PropertyField(scale);

			using (new EditorGUI.DisabledGroupScope(containsFollows)) {
				EditorGUILayout.PropertyField(overrideAlpha);
				EditorGUILayout.PropertyField(parentReference);
			}

			EditorGUILayout.Space();

			using (new GUILayout.HorizontalScope()) {
				using (new EditorGUI.DisabledGroupScope(multiObject || !utilityBone.valid || utilityBone.bone == null || utilityBone.bone.Children.Count == 0)) {
					if (GUILayout.Button(new GUIContent("Add Child", SpineEditorUtilities.Icons.bone), GUILayout.Width(150), GUILayout.Height(24)))
						BoneSelectorContextMenu("", utilityBone.bone.Children, "<Recursively>", SpawnChildBoneSelected);
				}
				using (new EditorGUI.DisabledGroupScope(multiObject || !utilityBone.valid || utilityBone.bone == null || containsOverrides)) {
					if (GUILayout.Button(new GUIContent("Add Override", SpineEditorUtilities.Icons.poseBones), GUILayout.Width(150), GUILayout.Height(24)))
						SpawnOverride();
				}
				using (new EditorGUI.DisabledGroupScope(multiObject || !utilityBone.valid || !canCreateHingeChain)) {
					if (GUILayout.Button(new GUIContent("Create Hinge Chain", SpineEditorUtilities.Icons.hingeChain), GUILayout.Width(150), GUILayout.Height(24)))
						CreateHingeChain();
				}
			}

			using (new EditorGUI.DisabledGroupScope(multiObject || boundingBoxTable.Count == 0)) {
				EditorGUILayout.LabelField(new GUIContent("Bounding Boxes", SpineEditorUtilities.Icons.boundingBox), EditorStyles.boldLabel);

				foreach(var entry in boundingBoxTable){
					EditorGUI.indentLevel++;
					EditorGUILayout.LabelField(entry.Key.Data.Name);
					EditorGUI.indentLevel++;
					{
						foreach (var box in entry.Value) {
							using (new GUILayout.HorizontalScope()) {
								GUILayout.Space(30);
								if (GUILayout.Button(box.Name, GUILayout.Width(200))) {
									var child = utilityBone.transform.FindChild("[BoundingBox]" + box.Name);
									if (child != null) {
										var originalCollider = child.GetComponent<PolygonCollider2D>();
										var updatedCollider = SkeletonUtility.AddBoundingBoxAsComponent(box, child.gameObject, originalCollider.isTrigger);
										originalCollider.points = updatedCollider.points;
										if (EditorApplication.isPlaying)
											Destroy(updatedCollider);
										else
											DestroyImmediate(updatedCollider);
									} else {
										utilityBone.AddBoundingBox(currentSkinName, entry.Key.Data.Name, box.Name);
									}

								}
							}

						}
					}
					EditorGUI.indentLevel--;
					EditorGUI.indentLevel--;
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		static void BoneSelectorContextMenu (string current, ExposedList<Bone> bones, string topValue, GenericMenu.MenuFunction2 callback) {
			GenericMenu menu = new GenericMenu();

			if (topValue != "")
				menu.AddItem(new GUIContent(topValue), current == topValue, callback, null);

			for (int i = 0; i < bones.Count; i++)
				menu.AddItem(new GUIContent(bones.Items[i].Data.Name), bones.Items[i].Data.Name == current, callback, bones.Items[i]);

			menu.ShowAsContext();

		}

		void TargetBoneSelected (object obj) {
			if (obj == null) {
				boneName.stringValue = "";
				serializedObject.ApplyModifiedProperties();
			} else {
				Bone bone = (Bone)obj;
				boneName.stringValue = bone.Data.Name;
				serializedObject.ApplyModifiedProperties();

				utilityBone.Reset();
			}
		}

		void SpawnChildBoneSelected (object obj) {
			if (obj == null) {
				//add recursively
				foreach (var bone in utilityBone.bone.Children) {
					GameObject go = skeletonUtility.SpawnBoneRecursively(bone, utilityBone.transform, utilityBone.mode, utilityBone.position, utilityBone.rotation, utilityBone.scale);
					SkeletonUtilityBone[] newUtilityBones = go.GetComponentsInChildren<SkeletonUtilityBone>();
					foreach (SkeletonUtilityBone utilBone in newUtilityBones)
						SkeletonUtilityInspector.AttachIcon(utilBone);
				}
			} else {
				Bone bone = (Bone)obj;
				GameObject go = skeletonUtility.SpawnBone(bone, utilityBone.transform, utilityBone.mode, utilityBone.position, utilityBone.rotation, utilityBone.scale);
				SkeletonUtilityInspector.AttachIcon(go.GetComponent<SkeletonUtilityBone>());
				Selection.activeGameObject = go;
				EditorGUIUtility.PingObject(go);
			}
		}

		void SpawnOverride () {
			GameObject go = skeletonUtility.SpawnBone(utilityBone.bone, utilityBone.transform.parent, SkeletonUtilityBone.Mode.Override, utilityBone.position, utilityBone.rotation, utilityBone.scale);
			go.name = go.name + " [Override]";
			SkeletonUtilityInspector.AttachIcon(go.GetComponent<SkeletonUtilityBone>());
			Selection.activeGameObject = go;
			EditorGUIUtility.PingObject(go);
		}

		bool CanCreateHingeChain () {
			if (utilityBone == null)
				return false;
			if (utilityBone.GetComponent<Rigidbody>() != null)
				return false;
			if (utilityBone.bone != null && utilityBone.bone.Children.Count == 0)
				return false;

			Rigidbody[] rigidbodies = utilityBone.GetComponentsInChildren<Rigidbody>();

			if (rigidbodies.Length > 0)
				return false;

			return true;
		}

		void CreateHingeChain () {
			var utilBoneArr = utilityBone.GetComponentsInChildren<SkeletonUtilityBone>();

			foreach (var utilBone in utilBoneArr) {
				AttachRigidbody(utilBone);
			}

			utilityBone.GetComponent<Rigidbody>().isKinematic = true;

			foreach (var utilBone in utilBoneArr) {
				if (utilBone == utilityBone)
					continue;

				utilBone.mode = SkeletonUtilityBone.Mode.Override;

				HingeJoint joint = utilBone.gameObject.AddComponent<HingeJoint>();
				joint.axis = Vector3.forward;
				joint.connectedBody = utilBone.transform.parent.GetComponent<Rigidbody>();
				joint.useLimits = true;
				joint.limits = new JointLimits {
					min = -20,
					max = 20
				};
				utilBone.GetComponent<Rigidbody>().mass = utilBone.transform.parent.GetComponent<Rigidbody>().mass * 0.75f;
			}
		}

		static void AttachRigidbody (SkeletonUtilityBone utilBone) {
			if (utilBone.GetComponent<Collider>() == null) {
				if (utilBone.bone.Data.Length == 0) {
					SphereCollider sphere = utilBone.gameObject.AddComponent<SphereCollider>();
					sphere.radius = 0.1f;
				} else {
					float length = utilBone.bone.Data.Length;
					BoxCollider box = utilBone.gameObject.AddComponent<BoxCollider>();
					box.size = new Vector3(length, length / 3f, 0.2f);
					box.center = new Vector3(length / 2f, 0, 0);
				}
			}

			utilBone.gameObject.AddComponent<Rigidbody>();
		}
	}

}
