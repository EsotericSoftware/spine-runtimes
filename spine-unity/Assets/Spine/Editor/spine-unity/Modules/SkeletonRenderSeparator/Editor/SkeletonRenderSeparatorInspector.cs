/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using Spine.Unity;
using Spine.Unity.Editor;

namespace Spine.Unity.Examples {
	
	[CustomEditor(typeof(SkeletonRenderSeparator))]
	public class SkeletonRenderSeparatorInspector : UnityEditor.Editor {
		SkeletonRenderSeparator component;

		// Properties
		SerializedProperty skeletonRenderer_, copyPropertyBlock_, copyMeshRendererFlags_, partsRenderers_;
		static bool partsRenderersExpanded = false;

		// For separator field.
		SerializedObject skeletonRendererSerializedObject;
		SerializedProperty separatorNamesProp;
		static bool skeletonRendererExpanded = true;
		bool slotsReapplyRequired = false;
		bool partsRendererInitRequired = false;

		void OnEnable () {
			if (component == null)
				component = target as SkeletonRenderSeparator;

			skeletonRenderer_ = serializedObject.FindProperty("skeletonRenderer");
			copyPropertyBlock_ = serializedObject.FindProperty("copyPropertyBlock");
			copyMeshRendererFlags_ = serializedObject.FindProperty("copyMeshRendererFlags");

			var partsRenderers = component.partsRenderers;
			partsRenderers_ = serializedObject.FindProperty("partsRenderers");
			partsRenderers_.isExpanded = partsRenderersExpanded ||	// last state
				partsRenderers.Contains(null) ||	// null items found
				partsRenderers.Count < 1 ||			// no parts renderers
				(skeletonRenderer_.objectReferenceValue != null && SkeletonRendererSeparatorCount + 1 > partsRenderers.Count); // not enough parts renderers
		}

		int SkeletonRendererSeparatorCount {
			get {
				if (Application.isPlaying)
					return component.SkeletonRenderer.separatorSlots.Count;
				else
					return separatorNamesProp == null ? 0 : separatorNamesProp.arraySize;
			}
		}

		public override void OnInspectorGUI () {

			// Restore mesh part for undo logic after undo of "Add Parts Renderer".
			// Triggers regeneration and assignment of the mesh filter's mesh.
			if (component.GetComponent<MeshFilter>() && component.GetComponent<MeshFilter>().sharedMesh == null) {
				component.OnDisable();
				component.OnEnable();
			}

			var componentRenderers = component.partsRenderers;
			int totalParts;

			using (new SpineInspectorUtility.LabelWidthScope()) {
				bool componentEnabled = component.enabled;
				bool checkBox = EditorGUILayout.Toggle("Enable Separator", componentEnabled);
				if (checkBox != componentEnabled)
					component.enabled = checkBox;
				if (component.SkeletonRenderer.disableRenderingOnOverride && !component.enabled)
					EditorGUILayout.HelpBox("By default, SkeletonRenderer's MeshRenderer is disabled while the SkeletonRenderSeparator takes over rendering. It is re-enabled when SkeletonRenderSeparator is disabled.", MessageType.Info);

				EditorGUILayout.PropertyField(copyPropertyBlock_);
				EditorGUILayout.PropertyField(copyMeshRendererFlags_);
			}

			// SkeletonRenderer Box
			using (new SpineInspectorUtility.BoxScope(false)) {
				// Fancy SkeletonRenderer foldout reference field
				{
					EditorGUI.indentLevel++;
					EditorGUI.BeginChangeCheck();
					var foldoutSkeletonRendererRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
					EditorGUI.PropertyField(foldoutSkeletonRendererRect, skeletonRenderer_);
					if (EditorGUI.EndChangeCheck())
						serializedObject.ApplyModifiedProperties();
					if (component.SkeletonRenderer != null) {
						skeletonRendererExpanded = EditorGUI.Foldout(foldoutSkeletonRendererRect, skeletonRendererExpanded, "");
					}
					EditorGUI.indentLevel--;
				}

				int separatorCount = 0;
				EditorGUI.BeginChangeCheck();
				if (component.SkeletonRenderer != null) {
					// Separators from SkeletonRenderer
					{
						bool skeletonRendererMismatch = skeletonRendererSerializedObject != null && skeletonRendererSerializedObject.targetObject != component.SkeletonRenderer;
						if (separatorNamesProp == null || skeletonRendererMismatch) {
							if (component.SkeletonRenderer != null) {
								skeletonRendererSerializedObject = new SerializedObject(component.SkeletonRenderer);
								separatorNamesProp = skeletonRendererSerializedObject.FindProperty("separatorSlotNames");
								separatorNamesProp.isExpanded = true;
							}
						}
							
						if (separatorNamesProp != null) {
							if (skeletonRendererExpanded) {
								EditorGUI.indentLevel++;
								SkeletonRendererInspector.SeparatorsField(separatorNamesProp);
								EditorGUI.indentLevel--;
							}
							separatorCount = this.SkeletonRendererSeparatorCount;
						}
					}

					if (SkeletonRendererSeparatorCount == 0) {
						EditorGUILayout.HelpBox("Separators are empty. Change the size to 1 and choose a slot if you want the render to be separated.", MessageType.Info);
					}
				}

				if (EditorGUI.EndChangeCheck()) {
					skeletonRendererSerializedObject.ApplyModifiedProperties();

					if (!Application.isPlaying)
						slotsReapplyRequired = true;
				}
					

				totalParts = separatorCount + 1;
				var counterStyle = skeletonRendererExpanded ? EditorStyles.label : EditorStyles.miniLabel;
				EditorGUILayout.LabelField(string.Format("{0}: separates into {1}.", SpineInspectorUtility.Pluralize(separatorCount, "separator", "separators"), SpineInspectorUtility.Pluralize(totalParts, "part", "parts") ), counterStyle);
			}

			// Parts renderers
			using (new SpineInspectorUtility.BoxScope(false)) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(this.partsRenderers_, true);
				EditorGUI.indentLevel--;

				// Null items warning
				bool nullItemsFound = componentRenderers.Contains(null);
				if (nullItemsFound)
					EditorGUILayout.HelpBox("Some items in the parts renderers list are null and may cause problems.\n\nYou can right-click on that element and choose 'Delete Array Element' to remove it.", MessageType.Warning);

				// (Button) Match Separators count
				if (separatorNamesProp != null) {
					int currentRenderers = 0;
					foreach (var r in componentRenderers) {
						if (r != null)
							currentRenderers++;
					}
					int extraRenderersNeeded = totalParts - currentRenderers;

					if (component.enabled && component.SkeletonRenderer != null && extraRenderersNeeded > 0) {
						EditorGUILayout.HelpBox(string.Format("Insufficient parts renderers. Some parts will not be rendered."), MessageType.Warning);
						string addMissingLabel = string.Format("Add the missing renderer{1} ({0}) ", extraRenderersNeeded, SpineInspectorUtility.PluralThenS(extraRenderersNeeded));
						if (GUILayout.Button(addMissingLabel, GUILayout.Height(40f))) {
							AddPartsRenderer(extraRenderersNeeded);
							DetectOrphanedPartsRenderers(component);
							partsRendererInitRequired = true;
						}
					}
				}
					
				if (partsRenderers_.isExpanded != partsRenderersExpanded) partsRenderersExpanded = partsRenderers_.isExpanded;
				if (partsRenderers_.isExpanded) {
					using (new EditorGUILayout.HorizontalScope()) {
						// (Button) Destroy Renderers button
						if (componentRenderers.Count > 0) {
							if (GUILayout.Button("Clear Parts Renderers")) {
								// Do you really want to destroy all?
								Undo.RegisterCompleteObjectUndo(component, "Clear Parts Renderers");
								if (EditorUtility.DisplayDialog("Destroy Renderers", "Do you really want to destroy all the Parts Renderer GameObjects in the list?", "Destroy", "Cancel")) {						
									foreach (var r in componentRenderers) {
										if (r != null)
											Undo.DestroyObjectImmediate(r.gameObject);
									}
									componentRenderers.Clear();
									// Do you also want to destroy orphans? (You monster.)
									DetectOrphanedPartsRenderers(component);
								}
							}
						}

						// (Button) Add Part Renderer button
						if (GUILayout.Button("Add Parts Renderer")) {
							AddPartsRenderer(1);
							partsRendererInitRequired = true;
						}
					}
				}
			}

			serializedObject.ApplyModifiedProperties();

			if (partsRendererInitRequired) {
				Undo.RegisterCompleteObjectUndo(component.GetComponent<MeshRenderer>(), "Add Parts Renderers");
				component.OnEnable();
				partsRendererInitRequired = false;
			}

			if (slotsReapplyRequired && UnityEngine.Event.current.type == EventType.Repaint) {
				component.SkeletonRenderer.ReapplySeparatorSlotNames();
				component.SkeletonRenderer.LateUpdate();
				SceneView.RepaintAll();
				slotsReapplyRequired = false;
			}
		}

		public void AddPartsRenderer (int count) {
			var componentRenderers = component.partsRenderers;
			bool emptyFound = componentRenderers.Contains(null);
			if (emptyFound) {
				bool userClearEntries = EditorUtility.DisplayDialog("Empty entries found", "Null entries found. Do you want to remove null entries before adding the new renderer? ", "Clear Empty Entries", "Don't Clear");
				if (userClearEntries) componentRenderers.RemoveAll(x => x == null);
			}

			Undo.RegisterCompleteObjectUndo(component, "Add Parts Renderers");
			for (int i = 0; i < count; i++) {
				int index = componentRenderers.Count;
				var smr = SkeletonPartsRenderer.NewPartsRendererGameObject(component.transform, index.ToString());
				Undo.RegisterCreatedObjectUndo(smr.gameObject, "New Parts Renderer GameObject.");
				componentRenderers.Add(smr);

				// increment renderer sorting order.
				if (index == 0) continue;
				var prev = componentRenderers[index - 1]; if (prev == null) continue;

				var prevMeshRenderer = prev.GetComponent<MeshRenderer>();
				var currentMeshRenderer = smr.GetComponent<MeshRenderer>();
				if (prevMeshRenderer == null || currentMeshRenderer == null) continue;

				int prevSortingLayer = prevMeshRenderer.sortingLayerID;
				int prevSortingOrder = prevMeshRenderer.sortingOrder;
				currentMeshRenderer.sortingLayerID = prevSortingLayer;
				currentMeshRenderer.sortingOrder = prevSortingOrder + SkeletonRenderSeparator.DefaultSortingOrderIncrement;
			}

		}

		/// <summary>Detects orphaned parts renderers and offers to delete them.</summary>
		public void DetectOrphanedPartsRenderers (SkeletonRenderSeparator component) {
			var children = component.GetComponentsInChildren<SkeletonPartsRenderer>();

			var orphans = new System.Collections.Generic.List<SkeletonPartsRenderer>();
			foreach (var r in children) {
				if (!component.partsRenderers.Contains(r))
					orphans.Add(r);
			}

			if (orphans.Count > 0) {
				if (EditorUtility.DisplayDialog("Destroy Submesh Renderers", "Unassigned renderers were found. Do you want to delete them? (These may belong to another Render Separator in the same hierarchy. If you don't have another Render Separator component in the children of this GameObject, it's likely safe to delete. Warning: This operation cannot be undone.)", "Delete", "Cancel")) {
					foreach (var o in orphans) {
						Undo.DestroyObjectImmediate(o.gameObject);
					}
				}
			}
		}

		#region SkeletonRenderer Context Menu Item
		[MenuItem ("CONTEXT/SkeletonRenderer/Add Skeleton Render Separator")]
		static void AddRenderSeparatorComponent (MenuCommand cmd) {
			var skeletonRenderer = cmd.context as SkeletonRenderer;
			var newComponent = skeletonRenderer.gameObject.AddComponent<SkeletonRenderSeparator>();

			Undo.RegisterCreatedObjectUndo(newComponent, "Add SkeletonRenderSeparator");
		}

		// Validate
		[MenuItem ("CONTEXT/SkeletonRenderer/Add Skeleton Render Separator", true)]
		static bool ValidateAddRenderSeparatorComponent (MenuCommand cmd) {
			var skeletonRenderer = cmd.context as SkeletonRenderer;
			var separator = skeletonRenderer.GetComponent<SkeletonRenderSeparator>();
			bool separatorNotOnObject = separator == null;
			return separatorNotOnObject;
		}
		#endregion

	}
}
