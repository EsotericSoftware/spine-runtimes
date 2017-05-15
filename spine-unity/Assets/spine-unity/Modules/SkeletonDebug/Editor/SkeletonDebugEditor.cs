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

// With contributions from: Mitch Thompson

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Spine.Unity.Modules;
using UnityEditor.AnimatedValues;

namespace Spine.Unity.Editor {
	using Editor = UnityEditor.Editor;
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonDebug))]
	public class SkeletonDebugEditor : Editor {

		static AnimBool showSlotsTree = new AnimBool(false);
		static AnimBool showSkeleton = new AnimBool(true);
		static AnimBool showConstraintsTree = new AnimBool(false);

		protected static bool showBoneNames, showPaths = true, showShapes = true, showConstraints = true;

		GUIContent SlotsRootLabel, SkeletonRootLabel;
		GUIStyle BoldFoldoutStyle;

		SkeletonDebug skeletonDebug;
		SkeletonRenderer skeletonRenderer;
		Skeleton skeleton;

		Skin activeSkin;

		bool isPrefab;

		readonly Dictionary<Slot, List<Attachment>> attachmentTable = new Dictionary<Slot, List<Attachment>>();

		#region Menus
		[MenuItem("CONTEXT/SkeletonRenderer/Debug with SkeletonDebug", false, 5000)]
		static void AddSkeletonDebug (MenuCommand command) {
			var go = ((SkeletonRenderer)command.context).gameObject;
			go.AddComponent<SkeletonDebug>();
			Undo.RegisterCreatedObjectUndo(go, "Add SkeletonDebug");
		}
		#endregion

		void OnEnable () {
			Initialize();
		}

		void Initialize () {
			if (SlotsRootLabel == null) {
				SlotsRootLabel = new GUIContent("Slots", Icons.slotRoot);
				SkeletonRootLabel = new GUIContent("Skeleton", Icons.skeleton);
				BoldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
				BoldFoldoutStyle.fontStyle = FontStyle.Bold;
				BoldFoldoutStyle.stretchWidth = true;
				BoldFoldoutStyle.fixedWidth = 0;
			}

			if (skeleton == null) {
				skeletonDebug = (SkeletonDebug)target;
				skeletonRenderer = skeletonDebug.GetComponent<SkeletonRenderer>();
				skeletonRenderer.Initialize(false);
				skeletonRenderer.LateUpdate();
				skeleton = skeletonRenderer.skeleton;
			}

			if (attachmentTable.Count == 0) UpdateAttachments();

			if (!skeletonRenderer.valid) return;
			isPrefab |= PrefabUtility.GetPrefabType(this.target) == PrefabType.Prefab;
		}

		public void OnSceneGUI () {
			var transform = skeletonRenderer.transform;
			if (skeleton == null) return;
			if (isPrefab) return;

			if (showPaths) SpineHandles.DrawPaths(transform, skeleton);
			if (showConstraints) SpineHandles.DrawConstraints(transform, skeleton);
			if (showBoneNames) SpineHandles.DrawBoneNames(transform, skeleton);
			if (showShapes) SpineHandles.DrawBoundingBoxes(transform, skeleton);
		}

		public override void OnInspectorGUI () {
			Initialize();

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

			EditorGUILayout.HelpBox("This is a debug component. Changes are not serialized.", MessageType.Info);

			using (new SpineInspectorUtility.BoxScope()) {

				// Skeleton
				showSkeleton.target = EditorGUILayout.Foldout(showSkeleton.target, SkeletonRootLabel, BoldFoldoutStyle);
				if (showSkeleton.faded > 0) {
					using (new EditorGUILayout.FadeGroupScope(showSkeleton.faded)) {
						using (new SpineInspectorUtility.IndentScope()) {
							EditorGUI.BeginChangeCheck();
							skeleton.SetColor(EditorGUILayout.ColorField(".R .G .B .A", skeleton.GetColor()));
							skeleton.FlipX = EditorGUILayout.ToggleLeft(".FlipX", skeleton.FlipX);
							skeleton.FlipY = EditorGUILayout.ToggleLeft(".FlipY", skeleton.FlipY);

							EditorGUILayout.Space();
							using (new SpineInspectorUtility.LabelWidthScope()) {
								showBoneNames = EditorGUILayout.Toggle("Show Bone Names", showBoneNames);
								showPaths = EditorGUILayout.Toggle("Show Paths", showPaths);
								showShapes = EditorGUILayout.Toggle("Show Shapes", showShapes);
								showConstraints = EditorGUILayout.Toggle("Show Constraints", showConstraints);
							}
							requireRepaint |= EditorGUI.EndChangeCheck();
						}
					}
				}

				// Slots
				int preSlotsIndent = EditorGUI.indentLevel;
				showSlotsTree.target = EditorGUILayout.Foldout(showSlotsTree.target, SlotsRootLabel, BoldFoldoutStyle);
				if (showSlotsTree.faded > 0) {
					using (new EditorGUILayout.FadeGroupScope(showSlotsTree.faded)) {
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
				EditorGUI.indentLevel = preSlotsIndent;

				// Constraints
				const string NoneText = "<none>";
				showConstraintsTree.target = EditorGUILayout.Foldout(showConstraintsTree.target, "Constraints", BoldFoldoutStyle);
				if (showConstraintsTree.faded > 0) {
					using (new EditorGUILayout.FadeGroupScope(showConstraintsTree.faded)) {
						using (new SpineInspectorUtility.IndentScope()) {
							const float MixMin = 0f;
							const float MixMax = 1f;

							EditorGUILayout.LabelField(string.Format("IK Constraints ({0})", skeleton.IkConstraints.Count), EditorStyles.boldLabel);
							using (new SpineInspectorUtility.IndentScope()) {
								if (skeleton.IkConstraints.Count > 0) {
									foreach (var c in skeleton.IkConstraints) {
										EditorGUILayout.LabelField(c.Data.Name);

										EditorGUI.BeginChangeCheck();
										c.BendDirection = EditorGUILayout.Toggle("Bend Direction Positive", c.BendDirection > 0) ? 1 : -1;
										c.Mix = EditorGUILayout.Slider("Mix", c.Mix, MixMin, MixMax);
										if (EditorGUI.EndChangeCheck())	requireRepaint = true;

										EditorGUILayout.Space();
									}

								} else {
									EditorGUILayout.LabelField(NoneText);
								}
							}

							EditorGUILayout.LabelField(string.Format("Transform Constraints ({0})", skeleton.TransformConstraints.Count), EditorStyles.boldLabel);
							using (new SpineInspectorUtility.IndentScope()) {
								if (skeleton.TransformConstraints.Count > 0) {
									foreach (var c in skeleton.TransformConstraints) {
										EditorGUILayout.LabelField(c.Data.Name);

										EditorGUI.BeginChangeCheck();
										c.TranslateMix = EditorGUILayout.Slider("TranslateMix", c.TranslateMix, MixMin, MixMax);
										c.RotateMix = EditorGUILayout.Slider("RotateMix", c.RotateMix, MixMin, MixMax);
										c.ScaleMix = EditorGUILayout.Slider("ScaleMix", c.ScaleMix, MixMin, MixMax);
										c.ShearMix = EditorGUILayout.Slider("ShearMix", c.ShearMix, MixMin, MixMax);
										if (EditorGUI.EndChangeCheck()) requireRepaint = true;

										EditorGUILayout.Space();
									}
								} else {
									EditorGUILayout.LabelField(NoneText);
								}
							}

							EditorGUILayout.LabelField(string.Format("Path Constraints ({0})", skeleton.PathConstraints.Count), EditorStyles.boldLabel);
							using (new SpineInspectorUtility.IndentScope()) {
								if (skeleton.PathConstraints.Count > 0) {
									foreach (var c in skeleton.PathConstraints) {
										EditorGUILayout.LabelField(c.Data.Name);
										EditorGUILayout.LabelField("PositionMode." + c.Data.PositionMode);
										EditorGUILayout.LabelField("SpacingMode." + c.Data.SpacingMode);
										EditorGUILayout.LabelField("RotateMode." + c.Data.RotateMode);

										EditorGUI.BeginChangeCheck();
										c.RotateMix = EditorGUILayout.Slider("RotateMix", c.RotateMix, MixMin, MixMax);
										c.TranslateMix = EditorGUILayout.Slider("TranslateMix", c.TranslateMix, MixMin, MixMax);
										c.Position = EditorGUILayout.FloatField("Position", c.Position);
										c.Spacing = EditorGUILayout.FloatField("Spacing", c.Spacing);
										if (EditorGUI.EndChangeCheck()) requireRepaint = true;

										EditorGUILayout.Space();
									}

								} else {
									EditorGUILayout.LabelField(NoneText);
								}
							}
						}
					}
				}

				if (showSlotsTree.isAnimating || showSkeleton.isAnimating || showConstraintsTree.isAnimating)
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
	


	}
}
