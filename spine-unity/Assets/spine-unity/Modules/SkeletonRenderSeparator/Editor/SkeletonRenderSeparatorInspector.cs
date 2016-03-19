using UnityEngine;
using UnityEditor;

namespace Spine.Unity {
	
	[CustomEditor(typeof(SkeletonRenderSeparator))]
	public class SkeletonRenderSeparatorInspector : Editor {
		SkeletonRenderSeparator component;

		// Properties
		SerializedProperty skeletonRenderer_, copyPropertyBlock_, copyMeshRendererFlags_, partsRenderers_;

		// For separator field.
		SerializedObject skeletonRendererSerializedObject;
		SerializedProperty separatorNamesProp;
		bool separatorExpanded = true;
		System.Func<int, string, string, string> Plural = SpineInspectorUtility.Pluralize;

		void OnEnable () {
			if (component == null)
				component = target as SkeletonRenderSeparator;

			skeletonRenderer_ = serializedObject.FindProperty("skeletonRenderer");
			copyPropertyBlock_ = serializedObject.FindProperty("copyPropertyBlock");
			copyMeshRendererFlags_ = serializedObject.FindProperty("copyMeshRendererFlags");
			partsRenderers_ = serializedObject.FindProperty("partsRenderers");
			partsRenderers_.isExpanded = true;
		}

		public override void OnInspectorGUI () {
			// TODO: Add Undo support
			var componentRenderers = component.partsRenderers;
			int separatorCount = 0;
			int totalParts;

			bool componentEnabled = component.enabled;
			bool checkBox = EditorGUILayout.Toggle("Enable Separator", componentEnabled);
			if (checkBox != componentEnabled) {
				component.enabled = checkBox;
			}

			EditorGUILayout.PropertyField(copyPropertyBlock_);
			EditorGUILayout.PropertyField(copyMeshRendererFlags_);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				// Fancy SkeletonRenderer foldout reference field
				{
					EditorGUI.indentLevel++;
					EditorGUI.BeginChangeCheck();
					var foldoutSkeletonRendererRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
					EditorGUI.PropertyField(foldoutSkeletonRendererRect, skeletonRenderer_);
					if (EditorGUI.EndChangeCheck())
						serializedObject.ApplyModifiedProperties();
					if (component.SkeletonRenderer != null) {
						separatorExpanded = EditorGUI.Foldout(foldoutSkeletonRendererRect, separatorExpanded, "");
					}
					EditorGUI.indentLevel--;
				}

				EditorGUI.BeginChangeCheck();
				if (component.SkeletonRenderer != null) {
					// SubmeshSeparators from SkeletonRenderer
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
							if (separatorExpanded) {
								EditorGUI.indentLevel++;
								SkeletonRendererInspector.SeparatorsField(separatorNamesProp);
								EditorGUI.indentLevel--;
							}
								
							if (Application.isPlaying)
								separatorCount = component.SkeletonRenderer.separatorSlots.Count;
							else
								separatorCount = separatorNamesProp.arraySize;

						}
					}

					if (separatorCount == 0) {
						EditorGUILayout.HelpBox("Separators are empty. Change the size to 1 and choose a slot if you want the render to be separated.", MessageType.Info);
					}
				}
				if (EditorGUI.EndChangeCheck())
					skeletonRendererSerializedObject.ApplyModifiedProperties();

				totalParts = separatorCount + 1;
				var counterStyle = separatorExpanded ? EditorStyles.label : EditorStyles.miniLabel;
				EditorGUILayout.LabelField(string.Format("{0}: separates into {1}.", Plural(separatorCount, "separator", "separators"), Plural(totalParts, "part", "parts") ), counterStyle);
			}

			// Parts renderers
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(this.partsRenderers_, true);
				EditorGUI.indentLevel--;

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
						//var addMissingContentButtonContent = new GUIContent("Add", GUIUtility.)
						if (GUILayout.Button(addMissingLabel, GUILayout.Height(40f))) {
							AddPartsRenderer(extraRenderersNeeded);
							DetectOrphanedPartsRenderers(component);
						}
					}
				}

				using (new EditorGUILayout.HorizontalScope()) {
					// (Button) Destroy Renderers button
					if (componentRenderers.Count > 0) {
						if (GUILayout.Button("Clear Parts Renderers")) {
							// Do you really want to destroy all?
							if (EditorUtility.DisplayDialog("Destroy Renderers", "Do you really want to destroy all the Parts Renderer GameObjects in the list? (Undo will not work.)", "Destroy", "Cancel")) {						
								foreach (var r in componentRenderers) {
									if (r != null)
										DestroyImmediate(r.gameObject, allowDestroyingAssets: false);
								}
								componentRenderers.Clear();
								// Do you also want to destroy orphans? (You monster.)
								DetectOrphanedPartsRenderers(component);
							}
						}
					}

					// (Button) Add Part Renderer button
					if (GUILayout.Button("Add (1) Parts Renderer"))
						AddPartsRenderer(1);				
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		public void AddPartsRenderer (int count) {
			var componentRenderers = component.partsRenderers;
			bool emptyFound = componentRenderers.Exists(x => x == null);
			if (emptyFound) {
				bool userClearEntries = EditorUtility.DisplayDialog("Empty entries found", "Null entries found. Do you want to remove null entries before adding the new renderer? ", "Clear Empty Entries", "Don't Clear");
				if (userClearEntries) componentRenderers.RemoveAll(x => x == null);
			}

			for (int i = 0; i < count; i++) {
				int index = componentRenderers.Count;
				var smr = SkeletonPartsRenderer.NewPartsRendererGameObject(component.transform, index.ToString());
				componentRenderers.Add(smr);
				EditorGUIUtility.PingObject(smr);

				// increment renderer sorting order.
				if (index != 0) {
					var prev = componentRenderers[index - 1];
					if (prev != null) {
						var prevMeshRenderer = prev.GetComponent<MeshRenderer>();
						var currentMeshRenderer = smr.GetComponent<MeshRenderer>();
						if (prevMeshRenderer != null && currentMeshRenderer != null) {
							int prevSortingLayer = prevMeshRenderer.sortingLayerID;
							int prevSortingOrder = prevMeshRenderer.sortingOrder;

							currentMeshRenderer.sortingLayerID = prevSortingLayer;
							currentMeshRenderer.sortingOrder = prevSortingOrder + SkeletonRenderSeparator.DefaultSortingOrderIncrement;
						}
					}
				}
			}

		}

		/// <summary>Detects orphaned parts renderers and offers to delete them.</summary>
		public void DetectOrphanedPartsRenderers (SkeletonRenderSeparator component) {
			var children = component.GetComponentsInChildren<SkeletonPartsRenderer>();

			var orphans = new System.Collections.Generic.List<SkeletonPartsRenderer>();
			foreach (var r in children) {
				if (!component.partsRenderers.Contains(r)) {
					orphans.Add(r);
				}
			}

			if (orphans.Count > 0) {
				if (EditorUtility.DisplayDialog("Destroy Submesh Renderers", "Unassigned renderers were found. Do you want to delete them? (These may belong to another Render Separator in the same hierarchy. If you don't have another Render Separator component in the children of this GameObject, it's likely safe to delete. Warning: This operation cannot be undone.)", "Delete", "Cancel")) {
					foreach (var o in orphans) {
						DestroyImmediate(o.gameObject, allowDestroyingAssets: false);
					}
				}
			}
		}


		#region SkeletonRenderer Context Menu Item
		[MenuItem ("CONTEXT/SkeletonRenderer/Add Skeleton Render Separator")]
		static void AddRenderSeparatorComponent (MenuCommand cmd) {
			var skeletonRenderer = cmd.context as SkeletonRenderer;
			skeletonRenderer.gameObject.AddComponent<SkeletonRenderSeparator>();
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
