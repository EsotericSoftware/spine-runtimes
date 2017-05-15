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
using UnityEditor.AnimatedValues;

namespace Spine.Unity.Editor {
	using Editor = UnityEditor.Editor;
	using Icons = SpineEditorUtilities.Icons;

	public class SkeletonDebugWindow : EditorWindow {

		const bool IsUtilityWindow = true;

		[MenuItem("CONTEXT/SkeletonRenderer/Open Skeleton Debug Window", false, 5000)]
		public static void Init () {
			var window = EditorWindow.GetWindow<SkeletonDebugWindow>(IsUtilityWindow);
			window.minSize = new Vector2(330f, 360f);
			window.maxSize = new Vector2(600f, 4000f);
			window.titleContent = new GUIContent("Skeleton Debug", Icons.spine);
			window.Show();
			window.OnSelectionChange();
		}


		static AnimBool showSkeleton = new AnimBool(true);
		static AnimBool showSlotsTree = new AnimBool(false);
		static AnimBool showConstraintsTree = new AnimBool(false);
		static AnimBool showDrawOrderTree = new AnimBool(false);
		static AnimBool showEventDataTree = new AnimBool(false);
		static AnimBool showInspectBoneTree = new AnimBool(false);

		Vector2 scrollPos;

		GUIContent SlotsRootLabel, SkeletonRootLabel;
		GUIStyle BoldFoldoutStyle;

		public SkeletonRenderer skeletonRenderer;
		Skeleton skeleton;
		Skin activeSkin;
		bool isPrefab;

		SerializedProperty bpo;
		Bone bone;

		[SpineBone(dataField:"skeletonRenderer")]
		public string boneName;

		readonly Dictionary<Slot, List<Attachment>> attachmentTable = new Dictionary<Slot, List<Attachment>>();

		void OnSelectionChange () {
			bool noSkeletonRenderer = false;

			var selectedObject = Selection.activeGameObject;
			if (selectedObject == null) {
				noSkeletonRenderer = true;
			} else {
				var selectedSkeletonRenderer = selectedObject.GetComponent<SkeletonRenderer>();
				if (selectedSkeletonRenderer == null) {
					noSkeletonRenderer = true;
				} else if (skeletonRenderer != selectedSkeletonRenderer) {
					skeletonRenderer = selectedSkeletonRenderer;
					skeletonRenderer.Initialize(false);
					skeletonRenderer.LateUpdate();
					skeleton = skeletonRenderer.skeleton;
					isPrefab |= PrefabUtility.GetPrefabType(selectedObject) == PrefabType.Prefab;
					UpdateAttachments();
				}
			} 

			if (noSkeletonRenderer) {
				skeletonRenderer = null;
				skeleton = null;
				attachmentTable.Clear();
				isPrefab = false;
				boneName = string.Empty;
				bone = null;
			}				

			Repaint();
		}

		void OnGUI () {
			if (SlotsRootLabel == null) {
				SlotsRootLabel = new GUIContent("Slots", Icons.slotRoot);
				SkeletonRootLabel = new GUIContent("Skeleton", Icons.skeleton);
				BoldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
				BoldFoldoutStyle.fontStyle = FontStyle.Bold;
				BoldFoldoutStyle.stretchWidth = true;
				BoldFoldoutStyle.fixedWidth = 0;
			}

			bool requireRepaint = false;
			EditorGUILayout.Space();
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField("Debug Selection", skeletonRenderer, typeof(SkeletonRenderer), true);
			EditorGUI.EndDisabledGroup();

			if (skeleton == null || skeletonRenderer == null || !skeletonRenderer.valid) return;

			if (isPrefab) {
				GUILayout.Label(new GUIContent("Cannot edit Prefabs", Icons.warning));
				return;
			}

			if (!skeletonRenderer.valid) {
				GUILayout.Label(new GUIContent("Spine Component invalid. Check Skeleton Data Asset.", Icons.warning));
				return;	
			}

			if (activeSkin != skeleton.Skin)
				UpdateAttachments();

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

			using (new SpineInspectorUtility.BoxScope(false)) {
				if (SpineInspectorUtility.CenteredButton(new GUIContent("Skeleton.SetToSetupPose()"))) {
					skeleton.SetToSetupPose();
					requireRepaint = true;
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.LabelField("Scene View", EditorStyles.boldLabel);
				using (new SpineInspectorUtility.LabelWidthScope()) {
					SkeletonRendererInspector.showBoneNames = EditorGUILayout.Toggle("Show Bone Names", SkeletonRendererInspector.showBoneNames);
					SkeletonRendererInspector.showPaths = EditorGUILayout.Toggle("Show Paths", SkeletonRendererInspector.showPaths);
					SkeletonRendererInspector.showShapes = EditorGUILayout.Toggle("Show Shapes", SkeletonRendererInspector.showShapes);
					SkeletonRendererInspector.showConstraints = EditorGUILayout.Toggle("Show Constraints", SkeletonRendererInspector.showConstraints);
				}
				requireRepaint |= EditorGUI.EndChangeCheck();


				// Skeleton
				showSkeleton.target = EditorGUILayout.Foldout(showSkeleton.target, SkeletonRootLabel, BoldFoldoutStyle);
				if (showSkeleton.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showSkeleton.faded)) {
							EditorGUI.BeginChangeCheck();

							// Flip
							EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
							EditorGUILayout.LabelField("Flip", GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 20f));
							skeleton.FlipX = EditorGUILayout.ToggleLeft(".FlipX", skeleton.FlipX, GUILayout.MaxWidth(70f));
							skeleton.FlipY = EditorGUILayout.ToggleLeft(".FlipY", skeleton.FlipY, GUILayout.MaxWidth(70f));
							GUILayout.EndHorizontal();

							// Color
							skeleton.SetColor(EditorGUILayout.ColorField(".R .G .B .A", skeleton.GetColor()));

							requireRepaint |= EditorGUI.EndChangeCheck();
						}
					}
				}

				// Bone
				showInspectBoneTree.target = EditorGUILayout.Foldout(showInspectBoneTree.target, new GUIContent("Bone", Icons.bone), BoldFoldoutStyle);
				if (showInspectBoneTree.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showInspectBoneTree.faded)) {
							if (bpo == null) bpo = new SerializedObject(this).FindProperty("boneName");
							EditorGUILayout.PropertyField(bpo);
							if (!string.IsNullOrEmpty(bpo.stringValue)) {
								if (bone == null || bone.Data.Name != bpo.stringValue) {
									bone = skeleton.FindBone(bpo.stringValue);
								}

								if (bone != null) {
									using (new EditorGUI.DisabledGroupScope(true)) {
										var boneParent = bone.Parent;
										if (boneParent != null) EditorGUILayout.TextField("parent", boneParent.Data.Name);
										EditorGUILayout.Space();

										EditorGUILayout.Slider("Local Rotation", bone.Rotation, -180f, 180f);
										EditorGUILayout.Vector2Field("Local Position", new Vector2(bone.X, bone.Y));
										EditorGUILayout.Vector2Field("Local Scale", new Vector2(bone.ScaleX, bone.ScaleY));
										EditorGUILayout.Vector2Field("Local Shear", new Vector2(bone.ShearX, bone.ShearY));
//										EditorGUILayout.Space();
//										EditorGUILayout.LabelField("LocalToWorld Matrix");
//										EditorGUILayout.Vector2Field("AB", new Vector2(bone.A, bone.B));
//										EditorGUILayout.Vector2Field("CD", new Vector2(bone.C, bone.D));
									}
								}
								requireRepaint = true;
							} else {
								bone = null;
							}
						}
					}
				}

				// Slots
				int preSlotsIndent = EditorGUI.indentLevel;
				showSlotsTree.target = EditorGUILayout.Foldout(showSlotsTree.target, SlotsRootLabel, BoldFoldoutStyle);
				if (showSlotsTree.faded > 0) {
					using (new EditorGUILayout.FadeGroupScope(showSlotsTree.faded)) {
						if (SpineInspectorUtility.CenteredButton(new GUIContent("Skeleton.SetSlotsToSetupPose()"))) {
							skeleton.SetSlotsToSetupPose();
							requireRepaint = true;
						}

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
				showConstraintsTree.target = EditorGUILayout.Foldout(showConstraintsTree.target, new GUIContent("Constraints", Icons.constraintRoot), BoldFoldoutStyle);
				if (showConstraintsTree.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showConstraintsTree.faded)) {
							const float MixMin = 0f;
							const float MixMax = 1f;

							EditorGUILayout.LabelField(new GUIContent(string.Format("IK Constraints ({0})", skeleton.IkConstraints.Count), Icons.constraintIK), EditorStyles.boldLabel);
							using (new SpineInspectorUtility.IndentScope()) {
								if (skeleton.IkConstraints.Count > 0) {
									foreach (var c in skeleton.IkConstraints) {
										EditorGUILayout.LabelField(new GUIContent(c.Data.Name, Icons.constraintIK));

										EditorGUI.BeginChangeCheck();
										c.Mix = EditorGUILayout.Slider("Mix", c.Mix, MixMin, MixMax);
										c.BendDirection = EditorGUILayout.Toggle("Bend Direction +", c.BendDirection > 0) ? 1 : -1;
										if (EditorGUI.EndChangeCheck())	requireRepaint = true;

										EditorGUILayout.Space();
									}

								} else {
									EditorGUILayout.LabelField(NoneText);
								}
							}

							EditorGUILayout.LabelField(new GUIContent(string.Format("Transform Constraints ({0})", skeleton.TransformConstraints.Count), Icons.constraintTransform), EditorStyles.boldLabel);
							using (new SpineInspectorUtility.IndentScope()) {
								if (skeleton.TransformConstraints.Count > 0) {
									foreach (var c in skeleton.TransformConstraints) {
										EditorGUILayout.LabelField(new GUIContent(c.Data.Name, Icons.constraintTransform));

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

							EditorGUILayout.LabelField(new GUIContent(string.Format("Path Constraints ({0})", skeleton.PathConstraints.Count), Icons.constraintPath), EditorStyles.boldLabel);
							using (new SpineInspectorUtility.IndentScope()) {
								if (skeleton.PathConstraints.Count > 0) {
									foreach (var c in skeleton.PathConstraints) {
										EditorGUILayout.LabelField(new GUIContent(c.Data.Name, Icons.constraintPath));
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

				showDrawOrderTree.target = EditorGUILayout.Foldout(showDrawOrderTree.target, new GUIContent("Draw Order and Separators", Icons.slotRoot), BoldFoldoutStyle);
				if (showDrawOrderTree.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showDrawOrderTree.faded)) {

							if (Application.isPlaying) {
								foreach (var slot in skeleton.DrawOrder) {
									if (skeletonRenderer.separatorSlots.Contains(slot))	EditorGUILayout.LabelField("------");
									EditorGUILayout.LabelField(new GUIContent(slot.Data.Name, Icons.slot), GUILayout.ExpandWidth(false));
								}
							} else {
								foreach (var slot in skeleton.DrawOrder) {
									var slotNames = skeletonRenderer.separatorSlotNames;
									for (int i = 0, n = slotNames.Length; i < n; i++) {
										if (string.Equals(slotNames[i], slot.Data.Name, System.StringComparison.Ordinal)) {
											EditorGUILayout.LabelField("------");
											break;
										}
									}
									EditorGUILayout.LabelField(new GUIContent(slot.Data.Name, Icons.slot), GUILayout.ExpandWidth(false));
								}
							}
								
						}
					}
				}

				showEventDataTree.target = EditorGUILayout.Foldout(showEventDataTree.target, new GUIContent("Events", Icons.userEvent), BoldFoldoutStyle);
				if (showEventDataTree.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showEventDataTree.faded)) {
							if (skeleton.Data.Events.Count > 0) {
								foreach (var e in skeleton.Data.Events) {
									EditorGUILayout.LabelField(new GUIContent(e.Name, Icons.userEvent));
								}
							} else {
								EditorGUILayout.LabelField(NoneText);
							}
						}
					}
				}

				if (IsAnimating(showSlotsTree, showSkeleton, showConstraintsTree, showDrawOrderTree, showEventDataTree, showInspectBoneTree))
					Repaint();
			}

			if (requireRepaint) {
				skeletonRenderer.LateUpdate();
				Repaint();
				SceneView.RepaintAll();
			}

			EditorGUILayout.EndScrollView();
		}

		static bool IsAnimating (params AnimBool[] animBools) {
			foreach (var a in animBools)
				if (a.isAnimating) return true;
			return false;
		}

		void UpdateAttachments () {
			//skeleton = skeletonRenderer.skeleton;
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
