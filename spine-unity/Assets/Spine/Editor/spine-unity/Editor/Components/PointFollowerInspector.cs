/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	using Editor = UnityEditor.Editor;
	using Event = UnityEngine.Event;

	[CustomEditor(typeof(PointFollower)), CanEditMultipleObjects]
	public class PointFollowerInspector : Editor {
		SerializedProperty slotName, pointAttachmentName, skeletonRenderer, followZPosition, followBoneRotation, followSkeletonFlip;
		PointFollower targetPointFollower;
		bool needsReset;

		#region Context Menu Item
		[MenuItem("CONTEXT/SkeletonRenderer/Add PointFollower GameObject")]
		static void AddBoneFollowerGameObject (MenuCommand cmd) {
			SkeletonRenderer skeletonRenderer = cmd.context as SkeletonRenderer;
			GameObject go = EditorInstantiation.NewGameObject("PointFollower", true);
			Transform t = go.transform;
			t.SetParent(skeletonRenderer.transform);
			t.localPosition = Vector3.zero;

			PointFollower f = go.AddComponent<PointFollower>();
			f.skeletonRenderer = skeletonRenderer;

			EditorGUIUtility.PingObject(t);

			Undo.RegisterCreatedObjectUndo(go, "Add PointFollower");
		}

		// Validate
		[MenuItem("CONTEXT/SkeletonRenderer/Add PointFollower GameObject", true)]
		static bool ValidateAddBoneFollowerGameObject (MenuCommand cmd) {
			SkeletonRenderer skeletonRenderer = cmd.context as SkeletonRenderer;
			return skeletonRenderer.valid;
		}
		#endregion

		void OnEnable () {
			skeletonRenderer = serializedObject.FindProperty("skeletonRenderer");
			slotName = serializedObject.FindProperty("slotName");
			pointAttachmentName = serializedObject.FindProperty("pointAttachmentName");

			targetPointFollower = (PointFollower)target;
			if (targetPointFollower.skeletonRenderer != null)
				targetPointFollower.skeletonRenderer.Initialize(false);

			if (!targetPointFollower.IsValid || needsReset) {
				targetPointFollower.Initialize();
				targetPointFollower.LateUpdate();
				needsReset = false;
				SceneView.RepaintAll();
			}
		}

		public void OnSceneGUI () {
			PointFollower tbf = target as PointFollower;
			SkeletonRenderer skeletonRendererComponent = tbf.skeletonRenderer;
			if (skeletonRendererComponent == null)
				return;

			Skeleton skeleton = skeletonRendererComponent.skeleton;
			Transform skeletonTransform = skeletonRendererComponent.transform;

			if (string.IsNullOrEmpty(pointAttachmentName.stringValue)) {
				// Draw all active PointAttachments in the current skin
				Skin currentSkin = skeleton.Skin;
				if (currentSkin != skeleton.Data.DefaultSkin) DrawPointsInSkin(skeleton.Data.DefaultSkin, skeleton, skeletonTransform);
				if (currentSkin != null) DrawPointsInSkin(currentSkin, skeleton, skeletonTransform);
			} else {
				Slot slot = skeleton.FindSlot(slotName.stringValue);
				if (slot != null) {
					int slotIndex = slot.Data.Index;
					PointAttachment point = skeleton.GetAttachment(slotIndex, pointAttachmentName.stringValue) as PointAttachment;
					if (point != null) {
						DrawPointAttachmentWithLabel(point, slot.Bone, skeletonTransform);
					}
				}
			}
		}

		static void DrawPointsInSkin (Skin skin, Skeleton skeleton, Transform transform) {
			foreach (Skin.SkinEntry skinEntry in skin.Attachments) {
				PointAttachment attachment = skinEntry.Attachment as PointAttachment;
				if (attachment != null) {
					Slot slot = skeleton.Slots.Items[skinEntry.SlotIndex];
					DrawPointAttachmentWithLabel(attachment, slot.Bone, transform);
				}
			}
		}

		static void DrawPointAttachmentWithLabel (PointAttachment point, Bone bone, Transform transform) {
			Vector3 labelOffset = new Vector3(0f, -0.2f, 0f);
			SpineHandles.DrawPointAttachment(bone, point, transform);
			Handles.Label(labelOffset + point.GetWorldPosition(bone, transform), point.Name, SpineHandles.PointNameStyle);
		}

		override public void OnInspectorGUI () {
			if (serializedObject.isEditingMultipleObjects) {
				if (needsReset) {
					needsReset = false;
					foreach (Object o in targets) {
						BoneFollower bf = (BoneFollower)o;
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
				targetPointFollower.Initialize();
				targetPointFollower.LateUpdate();
				needsReset = false;
				SceneView.RepaintAll();
			}
			serializedObject.Update();

			DrawDefaultInspector();

			// Find Renderer
			if (skeletonRenderer.objectReferenceValue == null) {
				SkeletonRenderer parentRenderer = targetPointFollower.GetComponentInParent<SkeletonRenderer>();
				if (parentRenderer != null && parentRenderer.gameObject != targetPointFollower.gameObject) {
					skeletonRenderer.objectReferenceValue = parentRenderer;
					Debug.Log("Inspector automatically assigned PointFollower.SkeletonRenderer");
				}
			}

			SkeletonRenderer skeletonRendererReference = skeletonRenderer.objectReferenceValue as SkeletonRenderer;
			if (skeletonRendererReference != null) {
				if (skeletonRendererReference.gameObject == targetPointFollower.gameObject) {
					skeletonRenderer.objectReferenceValue = null;
					EditorUtility.DisplayDialog("Invalid assignment.", "PointFollower can only follow a skeleton on a separate GameObject.\n\nCreate a new GameObject for your PointFollower, or choose a SkeletonRenderer from a different GameObject.", "Ok");
				}
			}

			if (!targetPointFollower.IsValid) {
				needsReset = true;
			}

			Event current = Event.current;
			bool wasUndo = (current.type == EventType.ValidateCommand && current.commandName == "UndoRedoPerformed");
			if (wasUndo)
				targetPointFollower.Initialize();

			serializedObject.ApplyModifiedProperties();
		}
	}

}
