/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

// With contributions from: Mitch Thompson

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#else
#define NO_PREFAB_MESH
#endif

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Editor = UnityEditor.Editor;
	using Icons = SpineEditorUtilities.Icons;

	public class SkeletonDebugWindow : EditorWindow {

		const bool IsUtilityWindow = true;
		internal static bool showBoneNames, showPaths = true, showShapes = true, showConstraints = true;

		[MenuItem("CONTEXT/SkeletonRenderer/Open Skeleton Debug Window", false, 5000)]
		public static void Init () {
			SkeletonDebugWindow window = EditorWindow.GetWindow<SkeletonDebugWindow>(IsUtilityWindow);
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
		static AnimBool showDataTree = new AnimBool(false);
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

		[SpineBone(dataField: "skeletonRenderer")]
		public string boneName;

		readonly Dictionary<Slot, List<Skin.SkinEntry>> attachmentTable = new Dictionary<Slot, List<Skin.SkinEntry>>();

		static bool staticLostValues = true;

		void OnSceneGUI (SceneView sceneView) {
			if (skeleton == null || skeletonRenderer == null || !skeletonRenderer.valid || isPrefab)
				return;

			Transform transform = skeletonRenderer.transform;
			if (showPaths) SpineHandles.DrawPaths(transform, skeleton);
			if (showConstraints) SpineHandles.DrawConstraints(transform, skeleton);
			if (showBoneNames) SpineHandles.DrawBoneNames(transform, skeleton);
			if (showShapes) SpineHandles.DrawBoundingBoxes(transform, skeleton);

			if (bone != null) {
				SpineHandles.DrawBone(skeletonRenderer.transform, bone, 1.5f, Color.cyan);
				Handles.Label(bone.GetWorldPosition(skeletonRenderer.transform) + (Vector3.down * 0.15f), bone.Data.Name, SpineHandles.BoneNameStyle);
			}
		}

		void OnSelectionChange () {
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= this.OnSceneGUI;
			SceneView.duringSceneGui += this.OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif

			bool noSkeletonRenderer = false;

			GameObject selectedObject = Selection.activeGameObject;
			if (selectedObject == null) {
				noSkeletonRenderer = true;
			} else {
				SkeletonRenderer selectedSkeletonRenderer = selectedObject.GetComponent<SkeletonRenderer>();
				if (selectedSkeletonRenderer == null) {
					noSkeletonRenderer = true;
				} else if (skeletonRenderer != selectedSkeletonRenderer) {

					bone = null;
					if (skeletonRenderer != null && skeletonRenderer.SkeletonDataAsset != selectedSkeletonRenderer.SkeletonDataAsset)
						boneName = null;

					skeletonRenderer = selectedSkeletonRenderer;
					skeletonRenderer.Initialize(false);
					skeletonRenderer.LateUpdate();
					skeleton = skeletonRenderer.skeleton;
#if NEW_PREFAB_SYSTEM
					isPrefab = false;
#else
					isPrefab |= PrefabUtility.GetPrefabType(selectedObject) == PrefabType.Prefab;
#endif
					UpdateAttachments();
				}
			}

			if (noSkeletonRenderer) Clear();
			Repaint();
		}

		void Clear () {
			skeletonRenderer = null;
			skeleton = null;
			attachmentTable.Clear();
			isPrefab = false;
			boneName = string.Empty;
			bone = null;
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= this.OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif
		}

		void OnDestroy () {
			Clear();
		}

		static void FalseDropDown (string label, string stringValue, Texture2D icon = null, bool disabledGroup = false) {
			if (disabledGroup) EditorGUI.BeginDisabledGroup(true);
			Rect pos = EditorGUILayout.GetControlRect(true);
			pos = EditorGUI.PrefixLabel(pos, SpineInspectorUtility.TempContent(label));
			GUI.Button(pos, SpineInspectorUtility.TempContent(stringValue, icon), EditorStyles.popup);
			if (disabledGroup) EditorGUI.EndDisabledGroup();
		}

		// Window GUI
		void OnGUI () {
			bool requireRepaint = false;

			if (staticLostValues) {
				Clear();
				OnSelectionChange();
				staticLostValues = false;
				requireRepaint = true;
			}

			if (SlotsRootLabel == null) {
				SlotsRootLabel = new GUIContent("Slots", Icons.slotRoot);
				SkeletonRootLabel = new GUIContent("Skeleton", Icons.skeleton);
				BoldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
				BoldFoldoutStyle.fontStyle = FontStyle.Bold;
				BoldFoldoutStyle.stretchWidth = true;
				BoldFoldoutStyle.fixedWidth = 0;
			}


			EditorGUILayout.Space();
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField(SpineInspectorUtility.TempContent("Debug Selection", Icons.spine), skeletonRenderer, typeof(SkeletonRenderer), true);
			EditorGUI.EndDisabledGroup();

			if (skeleton == null || skeletonRenderer == null) {
				EditorGUILayout.HelpBox("No SkeletonRenderer Spine GameObject selected.", MessageType.Info);
				return;
			}

			if (isPrefab) {
				EditorGUILayout.HelpBox("SkeletonDebug only debugs Spine GameObjects in the scene.", MessageType.Warning);
				return;
			}

			if (!skeletonRenderer.valid) {
				EditorGUILayout.HelpBox("Spine Component is invalid. Check SkeletonData Asset.", MessageType.Error);
				return;
			}

			if (activeSkin != skeleton.Skin)
				UpdateAttachments();

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

			using (new SpineInspectorUtility.BoxScope(false)) {
				if (SpineInspectorUtility.CenteredButton(SpineInspectorUtility.TempContent("Skeleton.SetToSetupPose()"))) {
					skeleton.SetToSetupPose();
					requireRepaint = true;
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.LabelField("Scene View", EditorStyles.boldLabel);
				using (new SpineInspectorUtility.LabelWidthScope()) {
					showBoneNames = EditorGUILayout.Toggle("Show Bone Names", showBoneNames);
					showPaths = EditorGUILayout.Toggle("Show Paths", showPaths);
					showShapes = EditorGUILayout.Toggle("Show Shapes", showShapes);
					showConstraints = EditorGUILayout.Toggle("Show Constraints", showConstraints);
				}
				requireRepaint |= EditorGUI.EndChangeCheck();


				// Skeleton
				showSkeleton.target = EditorGUILayout.Foldout(showSkeleton.target, SkeletonRootLabel, BoldFoldoutStyle);
				if (showSkeleton.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showSkeleton.faded)) {
							EditorGUI.BeginChangeCheck();

							EditorGUI.BeginDisabledGroup(true);
							FalseDropDown(".Skin", skeleton.Skin != null ? skeletonRenderer.Skeleton.Skin.Name : "<None>", Icons.skin);
							EditorGUI.EndDisabledGroup();

							// Flip
							skeleton.ScaleX = EditorGUILayout.DelayedFloatField(".ScaleX", skeleton.ScaleX);
							skeleton.ScaleY = EditorGUILayout.DelayedFloatField(".ScaleY", skeleton.ScaleY);
							//EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
							////EditorGUILayout.LabelField("Scale", GUILayout.Width(EditorGUIUtility.labelWidth - 20f));
							//GUILayout.EndHorizontal();

							// Color
							skeleton.SetColor(EditorGUILayout.ColorField(".R .G .B .A", skeleton.GetColor()));

							requireRepaint |= EditorGUI.EndChangeCheck();
						}
					}
				}

				// Bone
				showInspectBoneTree.target = EditorGUILayout.Foldout(showInspectBoneTree.target, SpineInspectorUtility.TempContent("Bone", Icons.bone), BoldFoldoutStyle);
				if (showInspectBoneTree.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showInspectBoneTree.faded)) {
							showBoneNames = EditorGUILayout.Toggle("Show Bone Names", showBoneNames);
							if (bpo == null) bpo = new SerializedObject(this).FindProperty("boneName");
							EditorGUILayout.PropertyField(bpo, SpineInspectorUtility.TempContent("Bone"));
							if (!string.IsNullOrEmpty(bpo.stringValue)) {
								if (bone == null || bone.Data.Name != bpo.stringValue) {
									bone = skeleton.FindBone(bpo.stringValue);
								}

								if (bone != null) {
									using (new EditorGUI.DisabledGroupScope(true)) {
										bool wm = EditorGUIUtility.wideMode;
										EditorGUIUtility.wideMode = true;
										EditorGUILayout.Slider("Local Rotation", ViewRound(bone.Rotation), -180f, 180f);
										EditorGUILayout.Vector2Field("Local Position", RoundVector2(bone.X, bone.Y));
										EditorGUILayout.Vector2Field("Local Scale", RoundVector2(bone.ScaleX, bone.ScaleY));
										EditorGUILayout.Vector2Field("Local Shear", RoundVector2(bone.ShearX, bone.ShearY));

										EditorGUILayout.Space();

										Bone boneParent = bone.Parent;
										if (boneParent != null) FalseDropDown("Parent", boneParent.Data.Name, Icons.bone);

										const string RoundFormat = "0.##";
										float lw = EditorGUIUtility.labelWidth;
										float fw = EditorGUIUtility.fieldWidth;
										EditorGUIUtility.labelWidth *= 0.25f;
										EditorGUIUtility.fieldWidth *= 0.5f;
										EditorGUILayout.LabelField("LocalToWorld");

										EditorGUILayout.BeginHorizontal();
										EditorGUILayout.Space();
										EditorGUILayout.TextField(".A", bone.A.ToString(RoundFormat));
										EditorGUILayout.TextField(".B", bone.B.ToString(RoundFormat));
										EditorGUILayout.EndHorizontal();
										EditorGUILayout.BeginHorizontal();
										EditorGUILayout.Space();
										EditorGUILayout.TextField(".C", bone.C.ToString(RoundFormat));
										EditorGUILayout.TextField(".D", bone.D.ToString(RoundFormat));
										EditorGUILayout.EndHorizontal();

										EditorGUIUtility.labelWidth = lw * 0.5f;
										EditorGUILayout.BeginHorizontal();
										EditorGUILayout.Space();
										EditorGUILayout.Space();
										EditorGUILayout.TextField(".WorldX", bone.WorldX.ToString(RoundFormat));
										EditorGUILayout.TextField(".WorldY", bone.WorldY.ToString(RoundFormat));
										EditorGUILayout.EndHorizontal();

										EditorGUIUtility.labelWidth = lw;
										EditorGUIUtility.fieldWidth = fw;
										EditorGUIUtility.wideMode = wm;

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
						if (SpineInspectorUtility.CenteredButton(SpineInspectorUtility.TempContent("Skeleton.SetSlotsToSetupPose()"))) {
							skeleton.SetSlotsToSetupPose();
							requireRepaint = true;
						}

						int baseIndent = EditorGUI.indentLevel;
						foreach (KeyValuePair<Slot, List<Skin.SkinEntry>> pair in attachmentTable) {
							Slot slot = pair.Key;

							using (new EditorGUILayout.HorizontalScope()) {
								EditorGUI.indentLevel = baseIndent + 1;
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(slot.Data.Name, Icons.slot), GUILayout.ExpandWidth(false));
								EditorGUI.BeginChangeCheck();
								Color c = EditorGUILayout.ColorField(new Color(slot.R, slot.G, slot.B, slot.A), GUILayout.Width(60));
								if (EditorGUI.EndChangeCheck()) {
									slot.SetColor(c);
									requireRepaint = true;
								}
							}

							foreach (Skin.SkinEntry skinEntry in pair.Value) {
								Attachment attachment = skinEntry.Attachment;
								GUI.contentColor = slot.Attachment == attachment ? Color.white : Color.grey;
								EditorGUI.indentLevel = baseIndent + 2;
								Texture2D icon = Icons.GetAttachmentIcon(attachment);
								bool isAttached = (attachment == slot.Attachment);
								bool swap = EditorGUILayout.ToggleLeft(SpineInspectorUtility.TempContent(attachment.Name, icon), attachment == slot.Attachment);
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
				showConstraintsTree.target = EditorGUILayout.Foldout(showConstraintsTree.target, SpineInspectorUtility.TempContent("Constraints", Icons.constraintRoot), BoldFoldoutStyle);
				if (showConstraintsTree.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showConstraintsTree.faded)) {
							const float MixMin = 0f;
							const float MixMax = 1f;
							EditorGUI.BeginChangeCheck();
							showConstraints = EditorGUILayout.Toggle("Show Constraints", showConstraints);
							requireRepaint |= EditorGUI.EndChangeCheck();

							EditorGUILayout.Space();

							EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(string.Format("IK Constraints ({0})", skeleton.IkConstraints.Count), Icons.constraintIK), EditorStyles.boldLabel);
							using (new SpineInspectorUtility.IndentScope()) {
								if (skeleton.IkConstraints.Count > 0) {
									foreach (IkConstraint c in skeleton.IkConstraints) {
										EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(c.Data.Name, Icons.constraintIK));
										FalseDropDown("Goal", c.Data.Target.Name, Icons.bone, true);
										using (new EditorGUI.DisabledGroupScope(true)) {
											EditorGUILayout.Toggle(SpineInspectorUtility.TempContent("Data.Uniform", tooltip: "Uniformly scales a bone when Ik stretches or compresses."), c.Data.Uniform);
										}

										EditorGUI.BeginChangeCheck();
										c.Mix = EditorGUILayout.Slider("Mix", c.Mix, MixMin, MixMax);
										c.BendDirection = EditorGUILayout.Toggle(SpineInspectorUtility.TempContent("Bend Clockwise", tooltip: "IkConstraint.BendDirection == 1 if clockwise; -1 if counterclockwise."), c.BendDirection > 0) ? 1 : -1;
										c.Compress = EditorGUILayout.Toggle(SpineInspectorUtility.TempContent("Compress", tooltip: "Compress single bone IK when the target too close. Not applied when parent bone has nonuniform scale."), c.Compress);
										c.Stretch = EditorGUILayout.Toggle(SpineInspectorUtility.TempContent("Stretch", tooltip: "Stretch the parent bone when the target is out of range. Not applied when parent bone has nonuniform scale."), c.Stretch);
										if (EditorGUI.EndChangeCheck()) requireRepaint = true;

										EditorGUILayout.Space();
									}

								} else {
									EditorGUILayout.LabelField(NoneText);
								}
							}

							EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(string.Format("Transform Constraints ({0})", skeleton.TransformConstraints.Count), Icons.constraintTransform), EditorStyles.boldLabel);
							using (new SpineInspectorUtility.IndentScope()) {
								if (skeleton.TransformConstraints.Count > 0) {
									foreach (TransformConstraint c in skeleton.TransformConstraints) {
										EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(c.Data.Name, Icons.constraintTransform));
										EditorGUI.BeginDisabledGroup(true);
										FalseDropDown("Goal", c.Data.Target.Name, Icons.bone);
										EditorGUI.EndDisabledGroup();

										EditorGUI.BeginChangeCheck();
										c.MixX = EditorGUILayout.Slider("Mix Translate X", c.MixX, MixMin, MixMax);
										c.MixY = EditorGUILayout.Slider("Mix Translate Y", c.MixY, MixMin, MixMax);
										c.MixRotate = EditorGUILayout.Slider("Mix Rotate", c.MixRotate, MixMin, MixMax);
										c.MixScaleX = EditorGUILayout.Slider("Mix Scale X", c.MixScaleX, MixMin, MixMax);
										c.MixScaleY = EditorGUILayout.Slider("Mix Scale Y", c.MixScaleY, MixMin, MixMax);
										c.MixShearY = EditorGUILayout.Slider("Mix Shear Y", c.MixShearY, MixMin, MixMax);
										if (EditorGUI.EndChangeCheck()) requireRepaint = true;

										EditorGUILayout.Space();
									}
								} else {
									EditorGUILayout.LabelField(NoneText);
								}
							}

							EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(string.Format("Path Constraints ({0})", skeleton.PathConstraints.Count), Icons.constraintPath), EditorStyles.boldLabel);

							EditorGUI.BeginChangeCheck();
							showPaths = EditorGUILayout.Toggle("Show Paths", showPaths);
							requireRepaint |= EditorGUI.EndChangeCheck();

							using (new SpineInspectorUtility.IndentScope()) {
								if (skeleton.PathConstraints.Count > 0) {
									foreach (PathConstraint c in skeleton.PathConstraints) {
										EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(c.Data.Name, Icons.constraintPath));
										EditorGUI.BeginDisabledGroup(true);
										FalseDropDown("Path Slot", c.Data.Target.Name, Icons.slot);
										Attachment activeAttachment = c.Target.Attachment;
										FalseDropDown("Active Path", activeAttachment != null ? activeAttachment.Name : "<None>", activeAttachment is PathAttachment ? Icons.path : null);
										EditorGUILayout.LabelField("PositionMode." + c.Data.PositionMode);
										EditorGUILayout.LabelField("SpacingMode." + c.Data.SpacingMode);
										EditorGUILayout.LabelField("RotateMode." + c.Data.RotateMode);
										EditorGUI.EndDisabledGroup();

										EditorGUI.BeginChangeCheck();
										c.MixRotate = EditorGUILayout.Slider("Mix Rotate", c.MixRotate, MixMin, MixMax);
										c.MixX = EditorGUILayout.Slider("Mix Translate X", c.MixX, MixMin, MixMax);
										c.MixY = EditorGUILayout.Slider("Mix Translate Y", c.MixY, MixMin, MixMax);
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

				showDrawOrderTree.target = EditorGUILayout.Foldout(showDrawOrderTree.target, SpineInspectorUtility.TempContent("Draw Order and Separators", Icons.slotRoot), BoldFoldoutStyle);
				if (showDrawOrderTree.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showDrawOrderTree.faded)) {

							const string SeparatorString = "------------- v SEPARATOR v -------------";

							if (Application.isPlaying) {
								foreach (Slot slot in skeleton.DrawOrder) {
									if (skeletonRenderer.separatorSlots.Contains(slot)) EditorGUILayout.LabelField(SeparatorString);

									using (new EditorGUI.DisabledScope(!slot.Bone.Active)) {
										EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(slot.Data.Name, Icons.slot), GUILayout.ExpandWidth(false));
									}
								}
							} else {
								foreach (Slot slot in skeleton.DrawOrder) {
									string[] slotNames = SkeletonRendererInspector.GetSeparatorSlotNames(skeletonRenderer);
									for (int i = 0, n = slotNames.Length; i < n; i++) {
										if (string.Equals(slotNames[i], slot.Data.Name, System.StringComparison.Ordinal)) {
											EditorGUILayout.LabelField(SeparatorString);
											break;
										}
									}
									using (new EditorGUI.DisabledScope(!slot.Bone.Active)) {
										EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(slot.Data.Name, Icons.slot), GUILayout.ExpandWidth(false));
									}
								}
							}

						}
					}
				}

				showEventDataTree.target = EditorGUILayout.Foldout(showEventDataTree.target, SpineInspectorUtility.TempContent("Events", Icons.userEvent), BoldFoldoutStyle);
				if (showEventDataTree.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showEventDataTree.faded)) {
							if (skeleton.Data.Events.Count > 0) {
								foreach (EventData e in skeleton.Data.Events) {
									EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(e.Name, Icons.userEvent));
								}
							} else {
								EditorGUILayout.LabelField(NoneText);
							}
						}
					}
				}

				showDataTree.target = EditorGUILayout.Foldout(showDataTree.target, SpineInspectorUtility.TempContent("Data Counts", Icons.spine), BoldFoldoutStyle);
				if (showDataTree.faded > 0) {
					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.FadeGroupScope(showDataTree.faded)) {
							using (new SpineInspectorUtility.LabelWidthScope()) {
								SkeletonData skeletonData = skeleton.Data;
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Bones", Icons.bone, "Skeleton.Data.Bones"), new GUIContent(skeletonData.Bones.Count.ToString()));
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Slots", Icons.slotRoot, "Skeleton.Data.Slots"), new GUIContent(skeletonData.Slots.Count.ToString()));
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Skins", Icons.skinsRoot, "Skeleton.Data.Skins"), new GUIContent(skeletonData.Skins.Count.ToString()));
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Events", Icons.userEvent, "Skeleton.Data.Events"), new GUIContent(skeletonData.Events.Count.ToString()));
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("IK Constraints", Icons.constraintIK, "Skeleton.Data.IkConstraints"), new GUIContent(skeletonData.IkConstraints.Count.ToString()));
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Transform Constraints", Icons.constraintTransform, "Skeleton.Data.TransformConstraints"), new GUIContent(skeletonData.TransformConstraints.Count.ToString()));
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Path Constraints", Icons.constraintPath, "Skeleton.Data.PathConstraints"), new GUIContent(skeletonData.PathConstraints.Count.ToString()));
							}
						}
					}
				}

				if (IsAnimating(showSlotsTree, showSkeleton, showConstraintsTree, showDrawOrderTree, showEventDataTree, showInspectBoneTree, showDataTree))
					Repaint();
			}

			if (requireRepaint) {
				skeletonRenderer.LateUpdate();
				Repaint();
				SceneView.RepaintAll();
			}

			EditorGUILayout.EndScrollView();
		}

		static float ViewRound (float x) {
			const float Factor = 100f;
			const float Divisor = 1f / Factor;
			return Mathf.Round(x * Factor) * Divisor;
		}

		static Vector2 RoundVector2 (float x, float y) {
			const float Factor = 100f;
			const float Divisor = 1f / Factor;
			return new Vector2(Mathf.Round(x * Factor) * Divisor, Mathf.Round(y * Factor) * Divisor);
		}

		static bool IsAnimating (params AnimBool[] animBools) {
			foreach (AnimBool a in animBools)
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
				List<Skin.SkinEntry> attachments = new List<Skin.SkinEntry>();
				attachmentTable.Add(skeleton.Slots.Items[i], attachments);
				// Add skin attachments.
				skin.GetAttachments(i, attachments);
				if (notDefaultSkin && defaultSkin != null) // Add default skin attachments.
					defaultSkin.GetAttachments(i, attachments);
			}

			activeSkin = skeleton.Skin;
		}
	}
}
