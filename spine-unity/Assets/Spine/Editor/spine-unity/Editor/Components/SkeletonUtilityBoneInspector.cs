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

// Contributed by: Mitch Thompson

#if UNITY_2019_2_OR_NEWER
#define HINGE_JOINT_NEW_BEHAVIOUR
#endif

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Spine;

namespace Spine.Unity.Editor {
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonUtilityBone)), CanEditMultipleObjects]
	public class SkeletonUtilityBoneInspector : UnityEditor.Editor {
		SerializedProperty mode, boneName, zPosition, position, rotation, scale, overrideAlpha, hierarchy, parentReference;
		GUIContent hierarchyLabel;

		//multi selected flags
		bool containsFollows, containsOverrides, multiObject;

		//single selected helpers
		SkeletonUtilityBone utilityBone;
		SkeletonUtility skeletonUtility;
		bool canCreateHingeChain = false;

		Dictionary<Slot, List<BoundingBoxAttachment>> boundingBoxTable = new Dictionary<Slot, List<BoundingBoxAttachment>>();

		void OnEnable () {
			mode = this.serializedObject.FindProperty("mode");
			boneName = this.serializedObject.FindProperty("boneName");
			zPosition = this.serializedObject.FindProperty("zPosition");
			position = this.serializedObject.FindProperty("position");
			rotation = this.serializedObject.FindProperty("rotation");
			scale = this.serializedObject.FindProperty("scale");
			overrideAlpha = this.serializedObject.FindProperty("overrideAlpha");
			hierarchy = this.serializedObject.FindProperty("hierarchy");
			hierarchyLabel = new GUIContent("Skeleton Utility Parent");
			parentReference = this.serializedObject.FindProperty("parentReference");

			utilityBone = (SkeletonUtilityBone)target;
			skeletonUtility = utilityBone.hierarchy;
			EvaluateFlags();

			if (!utilityBone.valid && skeletonUtility != null) {
				if (skeletonUtility.skeletonRenderer != null)
					skeletonUtility.skeletonRenderer.Initialize(false);
				if (skeletonUtility.skeletonGraphic != null)
					skeletonUtility.skeletonGraphic.Initialize(false);
			}

			canCreateHingeChain = CanCreateHingeChain();
			boundingBoxTable.Clear();

			if (multiObject) return;
			if (utilityBone.bone == null) return;

			var skeleton = utilityBone.bone.Skeleton;
			int slotCount = skeleton.Slots.Count;
			Skin skin = skeleton.Skin;
			if (skeleton.Skin == null)
				skin = skeleton.Data.DefaultSkin;

			for(int i = 0; i < slotCount; i++){
				Slot slot = skeletonUtility.Skeleton.Slots.Items[i];
				if (slot.Bone == utilityBone.bone) {
					var slotAttachments = new List<Skin.SkinEntry>();
					int slotIndex = skeleton.FindSlotIndex(slot.Data.Name);
					skin.GetAttachments(slotIndex, slotAttachments);

					var boundingBoxes = new List<BoundingBoxAttachment>();
					foreach (var att in slotAttachments) {
						var boundingBoxAttachment = att.Attachment as BoundingBoxAttachment;
						if (boundingBoxAttachment != null)
							boundingBoxes.Add(boundingBoxAttachment);
					}

					if (boundingBoxes.Count > 0)
						boundingBoxTable.Add(slot, boundingBoxes);
				}
			}
		}

		void EvaluateFlags () {
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

				multiObject |= (boneCount > 1);
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
						BoneSelectorContextMenu(str, ((SkeletonUtilityBone)target).hierarchy.Skeleton.Bones, "<None>", TargetBoneSelected);
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
				EditorGUILayout.PropertyField(hierarchy, hierarchyLabel);
			}

			EditorGUILayout.Space();

			using (new GUILayout.HorizontalScope()) {
				EditorGUILayout.Space();
				using (new EditorGUI.DisabledGroupScope(multiObject || !utilityBone.valid || utilityBone.bone == null || utilityBone.bone.Children.Count == 0)) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Add Child Bone", Icons.bone), GUILayout.MinWidth(120), GUILayout.Height(24)))
						BoneSelectorContextMenu("", utilityBone.bone.Children, "<Recursively>", SpawnChildBoneSelected);
				}
				using (new EditorGUI.DisabledGroupScope(multiObject || !utilityBone.valid || utilityBone.bone == null || containsOverrides)) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Add Override", Icons.poseBones), GUILayout.MinWidth(120), GUILayout.Height(24)))
						SpawnOverride();
				}
				EditorGUILayout.Space();
			}
			EditorGUILayout.Space();
			using (new GUILayout.HorizontalScope()) {
				EditorGUILayout.Space();
				using (new EditorGUI.DisabledGroupScope(multiObject || !utilityBone.valid || !canCreateHingeChain)) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Create 3D Hinge Chain", Icons.hingeChain), GUILayout.MinWidth(120), GUILayout.Height(24)))
						CreateHingeChain();
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Create 2D Hinge Chain", Icons.hingeChain), GUILayout.MinWidth(120), GUILayout.Height(24)))
						CreateHingeChain2D();
				}
				EditorGUILayout.Space();
			}

			using (new EditorGUI.DisabledGroupScope(multiObject || boundingBoxTable.Count == 0)) {
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Bounding Boxes", Icons.boundingBox), EditorStyles.boldLabel);

				foreach (var entry in boundingBoxTable){
					Slot slot = entry.Key;
					var boundingBoxes = entry.Value;

					EditorGUI.indentLevel++;
					EditorGUILayout.LabelField(slot.Data.Name);
					EditorGUI.indentLevel++;
					{
						foreach (var box in boundingBoxes) {
							using (new GUILayout.HorizontalScope()) {
								GUILayout.Space(30);
								string buttonLabel = box.IsWeighted() ? box.Name + " (!)" : box.Name;
								if (GUILayout.Button(buttonLabel, GUILayout.Width(200))) {
									utilityBone.bone.Skeleton.UpdateWorldTransform();
									var bbTransform = utilityBone.transform.Find("[BoundingBox]" + box.Name); // Use FindChild in older versions of Unity.
									if (bbTransform != null) {
										var originalCollider = bbTransform.GetComponent<PolygonCollider2D>();
										if (originalCollider != null)
											SkeletonUtility.SetColliderPointsLocal(originalCollider, slot, box);
										else
											SkeletonUtility.AddBoundingBoxAsComponent(box, slot, bbTransform.gameObject);
									} else {
										var newPolygonCollider = SkeletonUtility.AddBoundingBoxGameObject(null, box, slot, utilityBone.transform);
										bbTransform = newPolygonCollider.transform;
									}
									EditorGUIUtility.PingObject(bbTransform);
								}
							}

						}
					}
					EditorGUI.indentLevel--;
					EditorGUI.indentLevel--;
				}
			}

			BoneFollowerInspector.RecommendRigidbodyButton(utilityBone);

			serializedObject.ApplyModifiedProperties();
		}

		static void BoneSelectorContextMenu (string current, ExposedList<Bone> bones, string topValue, GenericMenu.MenuFunction2 callback) {
			var menu = new GenericMenu();

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
				var bone = (Bone)obj;
				boneName.stringValue = bone.Data.Name;
				serializedObject.ApplyModifiedProperties();
				utilityBone.Reset();
			}
		}

		void SpawnChildBoneSelected (object obj) {
			if (obj == null) {
				// Add recursively
				foreach (var bone in utilityBone.bone.Children) {
					GameObject go = skeletonUtility.SpawnBoneRecursively(bone, utilityBone.transform, utilityBone.mode, utilityBone.position, utilityBone.rotation, utilityBone.scale);
					SkeletonUtilityBone[] newUtilityBones = go.GetComponentsInChildren<SkeletonUtilityBone>();
					foreach (SkeletonUtilityBone utilBone in newUtilityBones)
						SkeletonUtilityInspector.AttachIcon(utilBone);
				}
			} else {
				var bone = (Bone)obj;
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
			if (utilityBone.GetComponent<Rigidbody>() != null || utilityBone.GetComponent<Rigidbody2D>() != null)
				return false;
			if (utilityBone.bone != null && utilityBone.bone.Children.Count == 0)
				return false;

			var rigidbodies = utilityBone.GetComponentsInChildren<Rigidbody>();
			var rigidbodies2D = utilityBone.GetComponentsInChildren<Rigidbody2D>();
			return rigidbodies.Length <= 0 && rigidbodies2D.Length <= 0;
		}

		void CreateHingeChain2D () {
			var kinematicParentUtilityBone = utilityBone.transform.parent.GetComponent<SkeletonUtilityBone>();
			if (kinematicParentUtilityBone == null) {
				UnityEditor.EditorUtility.DisplayDialog("No parent SkeletonUtilityBone found!", "Please select the first physically moving chain node, having a parent GameObject with a SkeletonUtilityBone component attached.", "OK");
				return;
			}

			float mass = 10;
			const float rotationLimit = 20.0f;

			SetSkeletonUtilityToFlipByRotation();

			kinematicParentUtilityBone.mode = SkeletonUtilityBone.Mode.Follow;
			kinematicParentUtilityBone.position = kinematicParentUtilityBone.rotation = kinematicParentUtilityBone.scale = kinematicParentUtilityBone.zPosition = true;

			GameObject commonParentObject = new GameObject(skeletonUtility.name + " HingeChain Parent " + utilityBone.name);
			var commonParentActivateOnFlip = commonParentObject.AddComponent<ActivateBasedOnFlipDirection>();
			commonParentActivateOnFlip.skeletonRenderer = skeletonUtility.skeletonRenderer;
			commonParentActivateOnFlip.skeletonGraphic = skeletonUtility.skeletonGraphic;

			// HingeChain Parent
			// Needs to be on top hierarchy level (not attached to the moving skeleton at least) for physics to apply proper momentum.
			GameObject normalChainParentObject = new GameObject("HingeChain");
			normalChainParentObject.transform.SetParent(commonParentObject.transform);
			commonParentActivateOnFlip.activeOnNormalX = normalChainParentObject;

			//var followRotationComponent = normalChainParentObject.AddComponent<FollowSkeletonUtilityRootRotation>();
			//followRotationComponent.reference = skeletonUtility.boneRoot;

			// Follower Kinematic Rigidbody
			GameObject followerKinematicObject = new GameObject(kinematicParentUtilityBone.name + " Follower");
			followerKinematicObject.transform.parent = normalChainParentObject.transform;
			var followerRigidbody = followerKinematicObject.AddComponent<Rigidbody2D>();
			followerRigidbody.mass = mass;
			followerRigidbody.isKinematic = true;
			followerKinematicObject.AddComponent<FollowLocationRigidbody2D>().reference = kinematicParentUtilityBone.transform;
			followerKinematicObject.transform.position = kinematicParentUtilityBone.transform.position;
			followerKinematicObject.transform.rotation = kinematicParentUtilityBone.transform.rotation;

			// Child Bones
			var utilityBones = utilityBone.GetComponentsInChildren<SkeletonUtilityBone>();
			var childBoneParentReference = followerKinematicObject.transform;
			for (int i = 0; i < utilityBones.Length; ++i) {
				var childBone = utilityBones[i];
				mass *= 0.75f;
				childBone.parentReference = (i == 0) ? kinematicParentUtilityBone.transform : childBoneParentReference;
				childBone.transform.SetParent(normalChainParentObject.transform, true); // we need a flat hierarchy of all Joint objects in Unity.
				AttachRigidbodyAndCollider2D(childBone);
				childBone.mode = SkeletonUtilityBone.Mode.Override;
				childBone.scale = childBone.position = childBone.zPosition = false;

				HingeJoint2D joint = childBone.gameObject.AddComponent<HingeJoint2D>();
				joint.connectedBody = childBoneParentReference.GetComponent<Rigidbody2D>();
				joint.useLimits = true;
				ApplyJoint2DAngleLimits(joint, rotationLimit, childBoneParentReference, childBone.transform);

				childBone.GetComponent<Rigidbody2D>().mass = mass;
				childBoneParentReference = childBone.transform;
			}

			Duplicate2DHierarchyForFlippedChains(normalChainParentObject, commonParentActivateOnFlip, skeletonUtility.transform, rotationLimit);
			UnityEditor.Selection.activeGameObject = commonParentObject;
		}

		void ApplyJoint2DAngleLimits (HingeJoint2D joint, float rotationLimit, Transform parentBone, Transform bone) {
		#if HINGE_JOINT_NEW_BEHAVIOUR
			float referenceAngle = (parentBone.eulerAngles.z - bone.eulerAngles.z + 360f) % 360f;
			float minAngle = referenceAngle - rotationLimit;
			float maxAngle = referenceAngle + rotationLimit;
			if (maxAngle > 270f) {
				minAngle -= 360f;
				maxAngle -= 360f;
			}
			if (minAngle < -90f) {
				minAngle += 360f;
				maxAngle += 360f;
			}
#else
			float minAngle = - rotationLimit;
			float maxAngle = rotationLimit;
#endif
			joint.limits = new JointAngleLimits2D {
				min = minAngle,
				max = maxAngle
			};
		}

		void Duplicate2DHierarchyForFlippedChains (GameObject normalChainParentObject, ActivateBasedOnFlipDirection commonParentActivateOnFlip,
													Transform skeletonUtilityRoot, float rotationLimit) {

			GameObject mirroredChain = GameObject.Instantiate(normalChainParentObject, normalChainParentObject.transform.position,
				normalChainParentObject.transform.rotation, commonParentActivateOnFlip.transform);
			mirroredChain.name = normalChainParentObject.name + " FlippedX";

			commonParentActivateOnFlip.activeOnFlippedX = mirroredChain;

			var followerKinematicObject = mirroredChain.GetComponentInChildren<FollowLocationRigidbody2D>();
			followerKinematicObject.followFlippedX = true;
			FlipBone2DHorizontal(followerKinematicObject.transform, skeletonUtilityRoot);

			var childBoneJoints = mirroredChain.GetComponentsInChildren<HingeJoint2D>();
			Transform prevRotatedChild = null;
			Transform parentTransformForAngles = followerKinematicObject.transform;
			for (int i = 0; i < childBoneJoints.Length; ++i) {
				var joint = childBoneJoints[i];
				FlipBone2DHorizontal(joint.transform, skeletonUtilityRoot);
				ApplyJoint2DAngleLimits(joint, rotationLimit, parentTransformForAngles, joint.transform);

				GameObject rotatedChild = GameObject.Instantiate(joint.gameObject, joint.transform, true);
				rotatedChild.name = joint.name + " rotated";
				var rotationEulerAngles = rotatedChild.transform.localEulerAngles;
				rotationEulerAngles.x = 180;
				rotatedChild.transform.localEulerAngles = rotationEulerAngles;
				DestroyImmediate(rotatedChild.GetComponent<HingeJoint2D>());
				DestroyImmediate(rotatedChild.GetComponent<BoxCollider2D>());
				DestroyImmediate(rotatedChild.GetComponent<Rigidbody2D>());

				DestroyImmediate(joint.gameObject.GetComponent<SkeletonUtilityBone>());

				if (i > 0) {
					var utilityBone = rotatedChild.GetComponent<SkeletonUtilityBone>();
					utilityBone.parentReference = prevRotatedChild;
				}
				prevRotatedChild = rotatedChild.transform;
				parentTransformForAngles = joint.transform;
			}

			mirroredChain.SetActive(false);
		}

		void FlipBone2DHorizontal(Transform bone, Transform mirrorPosition) {
			Vector3 position = bone.position;
			position.x = 2 * mirrorPosition.position.x - position.x; // = mirrorPosition + (mirrorPosition - bone.position)
			bone.position = position;

			Vector3 boneZ = bone.forward;
			Vector3 boneX = bone.right;
			boneX.x *= -1;

			bone.rotation = Quaternion.LookRotation(boneZ, Vector3.Cross(boneZ, boneX));
		}

		void CreateHingeChain () {
			var kinematicParentUtilityBone = utilityBone.transform.parent.GetComponent<SkeletonUtilityBone>();
			if (kinematicParentUtilityBone == null) {
				UnityEditor.EditorUtility.DisplayDialog("No parent SkeletonUtilityBone found!", "Please select the first physically moving chain node, having a parent GameObject with a SkeletonUtilityBone component attached.", "OK");
				return;
			}

			SetSkeletonUtilityToFlipByRotation();

			kinematicParentUtilityBone.mode = SkeletonUtilityBone.Mode.Follow;
			kinematicParentUtilityBone.position = kinematicParentUtilityBone.rotation = kinematicParentUtilityBone.scale = kinematicParentUtilityBone.zPosition = true;

			// HingeChain Parent
			// Needs to be on top hierarchy level (not attached to the moving skeleton at least) for physics to apply proper momentum.
			GameObject chainParentObject = new GameObject(skeletonUtility.name + " HingeChain Parent " + utilityBone.name);
			var followRotationComponent = chainParentObject.AddComponent<FollowSkeletonUtilityRootRotation>();
			followRotationComponent.reference = skeletonUtility.boneRoot;

			// Follower Kinematic Rigidbody
			GameObject followerKinematicObject = new GameObject(kinematicParentUtilityBone.name + " Follower");
			followerKinematicObject.transform.parent = chainParentObject.transform;
			var followerRigidbody = followerKinematicObject.AddComponent<Rigidbody>();
			followerRigidbody.mass = 10;
			followerRigidbody.isKinematic = true;
			followerKinematicObject.AddComponent<FollowLocationRigidbody>().reference = kinematicParentUtilityBone.transform;
			followerKinematicObject.transform.position = kinematicParentUtilityBone.transform.position;
			followerKinematicObject.transform.rotation = kinematicParentUtilityBone.transform.rotation;

			// Child Bones
			var utilityBones = utilityBone.GetComponentsInChildren<SkeletonUtilityBone>();
			var childBoneParentReference = followerKinematicObject.transform;
			foreach (var childBone in utilityBones) {
				childBone.parentReference = childBoneParentReference;
				childBone.transform.SetParent(chainParentObject.transform, true); // we need a flat hierarchy of all Joint objects in Unity.
				AttachRigidbodyAndCollider(childBone);
				childBone.mode = SkeletonUtilityBone.Mode.Override;

				HingeJoint joint = childBone.gameObject.AddComponent<HingeJoint>();
				joint.axis = Vector3.forward;
				joint.connectedBody = childBoneParentReference.GetComponent<Rigidbody>();
				joint.useLimits = true;
				joint.limits = new JointLimits {
					min = -20,
					max = 20
				};
				childBone.GetComponent<Rigidbody>().mass = childBoneParentReference.transform.GetComponent<Rigidbody>().mass * 0.75f;

				childBoneParentReference = childBone.transform;
			}
			UnityEditor.Selection.activeGameObject = chainParentObject;
		}

		void SetSkeletonUtilityToFlipByRotation () {
			if (!skeletonUtility.flipBy180DegreeRotation) {
				skeletonUtility.flipBy180DegreeRotation = true;
				Debug.Log("Set SkeletonUtility " + skeletonUtility.name + " to flip by rotation instead of negative scale (required).", skeletonUtility);
			}
		}

		static void AttachRigidbodyAndCollider (SkeletonUtilityBone utilBone, bool enableCollider = false) {
			if (utilBone.GetComponent<Collider>() == null) {
				if (utilBone.bone.Data.Length == 0) {
					SphereCollider sphere = utilBone.gameObject.AddComponent<SphereCollider>();
					sphere.radius = 0.1f;
					sphere.enabled = enableCollider;
				} else {
					float length = utilBone.bone.Data.Length;
					BoxCollider box = utilBone.gameObject.AddComponent<BoxCollider>();
					box.size = new Vector3(length, length / 3f, 0.2f);
					box.center = new Vector3(length / 2f, 0, 0);
					box.enabled = enableCollider;
				}
			}
			utilBone.gameObject.AddComponent<Rigidbody>();
		}

		static void AttachRigidbodyAndCollider2D(SkeletonUtilityBone utilBone, bool enableCollider = false) {
			if (utilBone.GetComponent<Collider2D>() == null) {
				if (utilBone.bone.Data.Length == 0) {
					var sphere = utilBone.gameObject.AddComponent<CircleCollider2D>();
					sphere.radius = 0.1f;
					sphere.enabled = enableCollider;
				}
				else {
					float length = utilBone.bone.Data.Length;
					var box = utilBone.gameObject.AddComponent<BoxCollider2D>();
					box.size = new Vector3(length, length / 3f, 0.2f);
					box.offset = new Vector3(length / 2f, 0, 0);
					box.enabled = enableCollider;
				}
			}
			utilBone.gameObject.AddComponent<Rigidbody2D>();
		}
	}
}
