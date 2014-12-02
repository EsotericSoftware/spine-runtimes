/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

/*****************************************************************************
 * Skeleton Utility created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using UnityEngine;
using UnityEditor;

#if UNITY_4_3
//nothing
#else
using UnityEditor.AnimatedValues;
#endif
using System.Collections;
using System.Collections.Generic;
using Spine;

using System.Reflection;

[CustomEditor(typeof(SkeletonUtility))]
public class SkeletonUtilityInspector : Editor {

	public static void AttachIcon (SkeletonUtilityBone utilityBone) {
		Skeleton skeleton = utilityBone.skeletonUtility.skeletonRenderer.skeleton;
		Texture2D icon;
		if (utilityBone.bone.Data.Length == 0)
			icon = SpineEditorUtilities.Icons.nullBone;
		else
			icon = SpineEditorUtilities.Icons.boneNib;
		
		foreach (IkConstraint c in skeleton.IkConstraints) {
			if (c.Target == utilityBone.bone) {
				icon = SpineEditorUtilities.Icons.constraintNib;
				break;
			}
		}
		
		
		
		typeof(EditorGUIUtility).InvokeMember("SetIconForObject", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic, null, null, new object[2] {
			utilityBone.gameObject,
			icon
		});
	}

	static void AttachIconsToChildren (Transform root) {
		if (root != null) {
			var utilityBones = root.GetComponentsInChildren<SkeletonUtilityBone>();
			foreach (var utilBone in utilityBones) {
				AttachIcon(utilBone);
			}
		}
	}

	static SkeletonUtilityInspector () {
		#if UNITY_4_3
		showSlots = false;
		#else
		showSlots = new AnimBool(false);
		#endif
	}

	SkeletonUtility skeletonUtility;
	Skeleton skeleton;
	SkeletonRenderer skeletonRenderer;
	Transform transform;
	bool isPrefab;
	Dictionary<Slot, List<Attachment>> attachmentTable = new Dictionary<Slot, List<Attachment>>();


	//GUI stuff
#if UNITY_4_3
	static bool showSlots;
#else
	static AnimBool showSlots;
#endif

	void OnEnable () {
		skeletonUtility = (SkeletonUtility)target;
		skeletonRenderer = skeletonUtility.GetComponent<SkeletonRenderer>();
		skeleton = skeletonRenderer.skeleton;
		transform = skeletonRenderer.transform;

		if (skeleton == null) {
			skeletonRenderer.Reset();
			skeletonRenderer.LateUpdate();

			skeleton = skeletonRenderer.skeleton;
		}

		UpdateAttachments();

		if (PrefabUtility.GetPrefabType(this.target) == PrefabType.Prefab)
			isPrefab = true;

	}

	void OnDestroy () {

	}

	void OnSceneGUI () {
		if (skeleton == null) {
			OnEnable();
			return;
		}

		float flipRotation = skeleton.FlipX ? -1 : 1;

		foreach (Bone b in skeleton.Bones) {
			Vector3 vec = transform.TransformPoint(new Vector3(b.WorldX, b.WorldY, 0));

			Quaternion rot = Quaternion.Euler(0, 0, b.WorldRotation * flipRotation);
			Vector3 forward = transform.TransformDirection(rot * Vector3.right);
			forward *= flipRotation;

			SpineEditorUtilities.Icons.boneMaterial.SetPass(0);
			Graphics.DrawMeshNow(SpineEditorUtilities.Icons.boneMesh, Matrix4x4.TRS(vec, Quaternion.LookRotation(transform.forward, forward), Vector3.one * b.Data.Length * b.WorldScaleX));
		}
	}

	void UpdateAttachments () {
		attachmentTable = new Dictionary<Slot, List<Attachment>>();
		Skin skin = skeleton.Skin;

		if (skin == null) {
			skin = skeletonRenderer.skeletonDataAsset.GetSkeletonData(true).DefaultSkin;
		}

		for (int i = skeleton.Slots.Count-1; i >= 0; i--) {
			List<Attachment> attachments = new List<Attachment>();
			skin.FindAttachmentsForSlot(i, attachments);

			attachmentTable.Add(skeleton.Slots[i], attachments);
		}
	}

	void SpawnHierarchyButton (string label, string tooltip, SkeletonUtilityBone.Mode mode, bool pos, bool rot, bool sca, params GUILayoutOption[] options) {
		GUIContent content = new GUIContent(label, tooltip);
		if (GUILayout.Button(content, options)) {
			if (skeletonUtility.skeletonRenderer == null)
				skeletonUtility.skeletonRenderer = skeletonUtility.GetComponent<SkeletonRenderer>();

			if (skeletonUtility.boneRoot != null) {
				return;
			}

			skeletonUtility.SpawnHierarchy(mode, pos, rot, sca);

			SkeletonUtilityBone[] boneComps = skeletonUtility.GetComponentsInChildren<SkeletonUtilityBone>();
			foreach (SkeletonUtilityBone b in boneComps) 
				AttachIcon(b);
		}
	}

	public override void OnInspectorGUI () {
		if (isPrefab) {
			GUILayout.Label(new GUIContent("Cannot edit Prefabs", SpineEditorUtilities.Icons.warning));
			return;
		}

		skeletonUtility.boneRoot = (Transform)EditorGUILayout.ObjectField("Bone Root", skeletonUtility.boneRoot, typeof(Transform), true);

		GUILayout.BeginHorizontal();
		EditorGUI.BeginDisabledGroup(skeletonUtility.boneRoot != null);
		{
			if (GUILayout.Button(new GUIContent("Spawn Hierarchy", SpineEditorUtilities.Icons.skeleton), GUILayout.Width(150), GUILayout.Height(24)))
				SpawnHierarchyContextMenu();
		}
		EditorGUI.EndDisabledGroup();

		if (GUILayout.Button(new GUIContent("Spawn Submeshes", SpineEditorUtilities.Icons.subMeshRenderer), GUILayout.Width(150), GUILayout.Height(24)))
			skeletonUtility.SpawnSubRenderers(true);
		GUILayout.EndHorizontal();

		EditorGUI.BeginChangeCheck();
		skeleton.FlipX = EditorGUILayout.ToggleLeft("Flip X", skeleton.FlipX);
		skeleton.FlipY = EditorGUILayout.ToggleLeft("Flip Y", skeleton.FlipY);
		if (EditorGUI.EndChangeCheck()) {
			skeletonRenderer.LateUpdate();
			SceneView.RepaintAll();
		}

#if UNITY_4_3
		showSlots = EditorGUILayout.Foldout(showSlots, "Slots");
#else
		showSlots.target = EditorGUILayout.Foldout(showSlots.target, "Slots");
		if (EditorGUILayout.BeginFadeGroup(showSlots.faded)) {
#endif
			foreach (KeyValuePair<Slot, List<Attachment>> pair in attachmentTable) {

				Slot slot = pair.Key;

				EditorGUILayout.BeginHorizontal();
				EditorGUI.indentLevel = 1;
				EditorGUILayout.LabelField(new GUIContent(slot.Data.Name, SpineEditorUtilities.Icons.slot), GUILayout.ExpandWidth(false));

				EditorGUI.BeginChangeCheck();
				Color c = EditorGUILayout.ColorField(new Color(slot.R, slot.G, slot.B, slot.A), GUILayout.Width(60));

				if (EditorGUI.EndChangeCheck()) {
					slot.SetColor(c);
					skeletonRenderer.LateUpdate();
				}

				EditorGUILayout.EndHorizontal();



				foreach (Attachment attachment in pair.Value) {

					if (slot.Attachment == attachment) {
						GUI.contentColor = Color.white;
					} else {
						GUI.contentColor = Color.grey;
					}

					EditorGUI.indentLevel = 2;
					bool isAttached = attachment == slot.Attachment;

					Texture2D icon = null;

					if (attachment is MeshAttachment || attachment is SkinnedMeshAttachment)
						icon = SpineEditorUtilities.Icons.mesh;
					else
						icon = SpineEditorUtilities.Icons.image;

					bool swap = EditorGUILayout.ToggleLeft(new GUIContent(attachment.Name, icon), attachment == slot.Attachment);

					if (!isAttached && swap) {
						slot.Attachment = attachment;
						skeletonRenderer.LateUpdate();
					} else if (isAttached && !swap) {
							slot.Attachment = null;
							skeletonRenderer.LateUpdate();
						}

					GUI.contentColor = Color.white;
				}
			}
			#if UNITY_4_3

#else
		}
		EditorGUILayout.EndFadeGroup();
		if (showSlots.isAnimating)
			Repaint();
#endif
	}

	void SpawnHierarchyContextMenu () {
		GenericMenu menu = new GenericMenu();

		menu.AddItem(new GUIContent("Follow"), false, SpawnFollowHierarchy);
		menu.AddItem(new GUIContent("Follow (Root Only)"), false, SpawnFollowHierarchyRootOnly);
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Override"), false, SpawnOverrideHierarchy);
		menu.AddItem(new GUIContent("Override (Root Only)"), false, SpawnOverrideHierarchyRootOnly);

		menu.ShowAsContext();
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
