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

// Contributed by: Mitch Thompson

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Spine;

namespace Spine.Unity.Editor {
	using Icons = SpineEditorUtilities.Icons;

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
		//string currentSkinName = "";

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

			if (!utilityBone.valid && skeletonUtility != null && skeletonUtility.skeletonRenderer != null)
				skeletonUtility.skeletonRenderer.Initialize(false);

			canCreateHingeChain = CanCreateHingeChain();
			boundingBoxTable.Clear();

			if (multiObject) return;
			if (utilityBone.bone == null) return;

			var skeleton = utilityBone.bone.Skeleton;
			int slotCount = skeleton.Slots.Count;
			Skin skin = skeleton.Skin;
			if (skeleton.Skin == null)
				skin = skeleton.Data.DefaultSkin;

			//currentSkinName = skin.Name;
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

					if (boundingBoxes.Count > 0)
						boundingBoxTable.Add(slot, boundingBoxes);
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
				EditorGUILayout.Space();
				using (new EditorGUI.DisabledGroupScope(multiObject || !utilityBone.valid || utilityBone.bone == null || utilityBone.bone.Children.Count == 0)) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Add Child", Icons.bone), GUILayout.MinWidth(120), GUILayout.Height(24)))
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
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Create Hinge Chain", Icons.hingeChain), GUILayout.Width(150), GUILayout.Height(24)))
						CreateHingeChain();
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
			if (utilityBone.GetComponent<Rigidbody>() != null)
				return false;
			if (utilityBone.bone != null && utilityBone.bone.Children.Count == 0)
				return false;

			Rigidbody[] rigidbodies = utilityBone.GetComponentsInChildren<Rigidbody>();

			return rigidbodies.Length <= 0;
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
