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

#if UNITY_2018_3 || UNITY_2019
#define NEW_PREFAB_SYSTEM
#endif

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
		
		#if !NEW_PREFAB_SYSTEM
		bool isPrefab;
		#endif

		readonly GUIContent SpawnHierarchyButtonLabel = new GUIContent("Spawn Hierarchy", Icons.skeleton);

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

			#if !NEW_PREFAB_SYSTEM
			isPrefab |= PrefabUtility.GetPrefabType(this.target) == PrefabType.Prefab;
			#endif
		}
			
		public override void OnInspectorGUI () {
			#if !NEW_PREFAB_SYSTEM
			if (isPrefab) {
				GUILayout.Label(new GUIContent("Cannot edit Prefabs", Icons.warning));
				return;
			}
			#endif

			if (!skeletonRenderer.valid) {
				GUILayout.Label(new GUIContent("Spine Component invalid. Check Skeleton Data Asset.", Icons.warning));
				return;	
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("boneRoot"), SpineInspectorUtility.TempContent("Skeleton Root"));

			bool hasRootBone = skeletonUtility.boneRoot != null;

			if (!hasRootBone)
				EditorGUILayout.HelpBox("No hierarchy found. Use Spawn Hierarchy to generate GameObjects for bones.", MessageType.Info);

			using (new EditorGUI.DisabledGroupScope(hasRootBone)) {
				if (SpineInspectorUtility.LargeCenteredButton(SpawnHierarchyButtonLabel))
					SpawnHierarchyContextMenu();
			}

			if (hasRootBone) {
				if (SpineInspectorUtility.CenteredButton(new GUIContent("Remove Hierarchy"))) {
					Undo.RegisterCompleteObjectUndo(skeletonUtility, "Remove Hierarchy");
					Undo.DestroyObjectImmediate(skeletonUtility.boneRoot.gameObject);
					skeletonUtility.boneRoot = null;
				}
			}
		}

		void SpawnHierarchyContextMenu () {
			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("Follow all bones"), false, SpawnFollowHierarchy);
			menu.AddItem(new GUIContent("Follow (Root Only)"), false, SpawnFollowHierarchyRootOnly);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Override all bones"), false, SpawnOverrideHierarchy);
			menu.AddItem(new GUIContent("Override (Root Only)"), false, SpawnOverrideHierarchyRootOnly);

			menu.ShowAsContext();
		}

		public static void AttachIcon (SkeletonUtilityBone boneComponent) {
			Skeleton skeleton = boneComponent.hierarchy.skeletonRenderer.skeleton;
			Texture2D icon = boneComponent.bone.Data.Length == 0 ? Icons.nullBone : Icons.boneNib;

			foreach (IkConstraint c in skeleton.IkConstraints)
				if (c.Target == boneComponent.bone) {
					icon = Icons.constraintNib;
					break;
				}

			typeof(EditorGUIUtility).InvokeMember("SetIconForObject", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic, null, null, new object[2] {
				boneComponent.gameObject,
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
