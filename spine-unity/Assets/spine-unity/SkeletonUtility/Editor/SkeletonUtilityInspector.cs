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
using UnityEditor.AnimatedValues;
using System.Collections.Generic;
using Spine;
using System.Reflection;

namespace Spine.Unity.Editor {
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonUtility))]
	public class SkeletonUtilityInspector : UnityEditor.Editor {

		SkeletonUtility skeletonUtility;
		Skeleton skeleton;
		SkeletonRenderer skeletonRenderer;
		Skin activeSkin;

		bool isPrefab;

		Dictionary<Slot, List<Attachment>> attachmentTable = new Dictionary<Slot, List<Attachment>>();

		GUIContent SpawnHierarchyButtonLabel = new GUIContent("Spawn Hierarchy", Icons.skeleton);
		GUIContent SlotsRootLabel = new GUIContent("Slots", Icons.slotRoot);
		static AnimBool showSlots = new AnimBool(false);
		static bool debugSkeleton = false;

		void OnEnable () {
			skeletonUtility = (SkeletonUtility)target;
			skeletonRenderer = skeletonUtility.GetComponent<SkeletonRenderer>();
			skeleton = skeletonRenderer.Skeleton;

			if (skeleton == null) {
				skeletonRenderer.Initialize(false);
				skeletonRenderer.LateUpdate();
				skeleton = skeletonRenderer.skeleton;
			}

			if (!skeletonRenderer.valid) return;

			UpdateAttachments();
			isPrefab |= PrefabUtility.GetPrefabType(this.target) == PrefabType.Prefab;
		}
			
		public override void OnInspectorGUI () {
			bool requireRepaint = false;
			if (skeletonRenderer.skeleton != skeleton || activeSkin != skeleton.Skin) {
				UpdateAttachments();
			}

			if (isPrefab) {
				GUILayout.Label(new GUIContent("Cannot edit Prefabs", Icons.warning));
				return;
			}

			if (!skeletonRenderer.valid) {
				GUILayout.Label(new GUIContent("Spine Component invalid. Check Skeleton Data Asset.", Icons.warning));
				return;	
			}

			skeletonUtility.boneRoot = (Transform)EditorGUILayout.ObjectField("Bone Root", skeletonUtility.boneRoot, typeof(Transform), true);

			using (new EditorGUI.DisabledGroupScope(skeletonUtility.boneRoot != null)) {
				if (SpineInspectorUtility.LargeCenteredButton(SpawnHierarchyButtonLabel))
					SpawnHierarchyContextMenu();
			}

			using (new SpineInspectorUtility.BoxScope()) {
				debugSkeleton = EditorGUILayout.Foldout(debugSkeleton, "Debug Skeleton");

				if (debugSkeleton) {
					EditorGUI.BeginChangeCheck();
					skeleton.FlipX = EditorGUILayout.ToggleLeft("skeleton.FlipX", skeleton.FlipX);
					skeleton.FlipY = EditorGUILayout.ToggleLeft("skeleton.FlipY", skeleton.FlipY);
					requireRepaint |= EditorGUI.EndChangeCheck();

//					foreach (var t in skeleton.IkConstraints)
//						EditorGUILayout.LabelField(t.Data.Name + " " + t.Mix + " " + t.Target.Data.Name);

					showSlots.target = EditorGUILayout.Foldout(showSlots.target, SlotsRootLabel);
					if (showSlots.faded > 0) {
						using (new EditorGUILayout.FadeGroupScope(showSlots.faded)) {
							int baseIndent = EditorGUI.indentLevel;
							foreach (KeyValuePair<Slot, List<Attachment>> pair in attachmentTable) {
								Slot slot = pair.Key;

								using (new EditorGUILayout.HorizontalScope()) {
									EditorGUI.indentLevel = baseIndent + 1;
									EditorGUILayout.LabelField(new GUIContent(slot.Data.Name, Icons.slot), GUILayout.ExpandWidth(false));
									EditorGUI.BeginChangeCheck();
									Color c = EditorGUILayout.ColorField(new Color(slot.R, slot.G, slot.B, slot.A), GUILayout.Width(60));
									if (EditorGUI.EndChangeCheck()) {
										slot.SetColor(c);
										requireRepaint = true;
									}
								}

								foreach (var attachment in pair.Value) {
									GUI.contentColor = slot.Attachment == attachment ? Color.white : Color.grey;
									EditorGUI.indentLevel = baseIndent + 2;
									var icon = Icons.GetAttachmentIcon(attachment);
									bool isAttached = (attachment == slot.Attachment);
									bool swap = EditorGUILayout.ToggleLeft(new GUIContent(attachment.Name, icon), attachment == slot.Attachment);
									if (isAttached != swap) {
										slot.Attachment = isAttached ? null : attachment;
										requireRepaint = true;
									}
									GUI.contentColor = Color.white;
								}
							}
						}
					}


				}

				if (showSlots.isAnimating)
					Repaint();
			}

			if (requireRepaint) {
				skeletonRenderer.LateUpdate();
				SceneView.RepaintAll();
			}
		}

		void UpdateAttachments () {
			skeleton = skeletonRenderer.skeleton;
			Skin defaultSkin = skeleton.Data.DefaultSkin;
			Skin skin = skeleton.Skin ?? defaultSkin;
			bool notDefaultSkin = skin != defaultSkin;

			attachmentTable.Clear();
			for (int i = skeleton.Slots.Count - 1; i >= 0; i--) {
				var attachments = new List<Attachment>();
				attachmentTable.Add(skeleton.Slots.Items[i], attachments);
				skin.FindAttachmentsForSlot(i, attachments); // Add skin attachments.
				if (notDefaultSkin) defaultSkin.FindAttachmentsForSlot(i, attachments); // Add default skin attachments.
			}

			activeSkin = skeleton.Skin;
		}

//		void SpawnHierarchyButton (string label, string tooltip, SkeletonUtilityBone.Mode mode, bool pos, bool rot, bool sca, params GUILayoutOption[] options) {
//			GUIContent content = new GUIContent(label, tooltip);
//			if (GUILayout.Button(content, options)) {
//				if (skeletonUtility.skeletonRenderer == null)
//					skeletonUtility.skeletonRenderer = skeletonUtility.GetComponent<SkeletonRenderer>();
//
//				if (skeletonUtility.boneRoot != null) {
//					return;
//				}
//
//				skeletonUtility.SpawnHierarchy(mode, pos, rot, sca);
//
//				SkeletonUtilityBone[] boneComps = skeletonUtility.GetComponentsInChildren<SkeletonUtilityBone>();
//				foreach (SkeletonUtilityBone b in boneComps) 
//					AttachIcon(b);
//			}
//		}

		void SpawnHierarchyContextMenu () {
			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent("Follow"), false, SpawnFollowHierarchy);
			menu.AddItem(new GUIContent("Follow (Root Only)"), false, SpawnFollowHierarchyRootOnly);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Override"), false, SpawnOverrideHierarchy);
			menu.AddItem(new GUIContent("Override (Root Only)"), false, SpawnOverrideHierarchyRootOnly);

			menu.ShowAsContext();
		}

		public static void AttachIcon (SkeletonUtilityBone utilityBone) {
			Skeleton skeleton = utilityBone.skeletonUtility.skeletonRenderer.skeleton;
			Texture2D icon = utilityBone.bone.Data.Length == 0 ? Icons.nullBone : Icons.boneNib;

			foreach (IkConstraint c in skeleton.IkConstraints)
				if (c.Target == utilityBone.bone) {
					icon = Icons.constraintNib;
					break;
				}

			typeof(EditorGUIUtility).InvokeMember("SetIconForObject", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic, null, null, new object[2] {
				utilityBone.gameObject,
				icon
			});
		}

		static void AttachIconsToChildren (Transform root) {
			if (root != null) {
				var utilityBones = root.GetComponentsInChildren<SkeletonUtilityBone>();
				foreach (var utilBone in utilityBones)
					AttachIcon(utilBone);
			}
		}

		void SpawnFollowHierarchy () {
			Selection.activeGameObject = skeletonUtility.SpawnHierarchy(SkeletonUtilityBone.Mode.Follow, true, true, true);
			AttachIconsToChildren(skeletonUtility.boneRoot);
		}

		void SpawnFollowHierarchyRootOnly () {
			Selection.activeGameObject = skeletonUtility.SpawnRoot(SkeletonUtilityBone.Mode.Follow, true, true, true);
			AttachIconsToChildren(skeletonUtility.boneRoot);
		}

		void SpawnOverrideHierarchy () {
			Selection.activeGameObject = skeletonUtility.SpawnHierarchy(SkeletonUtilityBone.Mode.Override, true, true, true);
			AttachIconsToChildren(skeletonUtility.boneRoot);
		}

		void SpawnOverrideHierarchyRootOnly () {
			Selection.activeGameObject = skeletonUtility.SpawnRoot(SkeletonUtilityBone.Mode.Override, true, true, true);
			AttachIconsToChildren(skeletonUtility.boneRoot);
		}
	}

}
