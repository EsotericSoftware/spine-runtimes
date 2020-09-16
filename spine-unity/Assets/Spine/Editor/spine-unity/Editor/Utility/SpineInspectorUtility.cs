/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace Spine.Unity.Editor {
	public static class SpineInspectorUtility {

		public static string Pluralize (int n, string singular, string plural) {
			return n + " " + (n == 1 ? singular : plural);
		}

		public static string PluralThenS (int n) {
			return n == 1 ? "" : "s";
		}

		public static string EmDash {
			get { return "\u2014"; }
		}

		static GUIContent tempContent;
		internal static GUIContent TempContent (string text, Texture2D image = null, string tooltip = null) {
			if (tempContent == null) tempContent = new GUIContent();
			tempContent.text = text;
			tempContent.image = image;
			tempContent.tooltip = tooltip;
			return tempContent;
		}

		public static void PropertyFieldWideLabel (SerializedProperty property, GUIContent label = null, float minimumLabelWidth = 150) {
			EditorGUIUtility.labelWidth = minimumLabelWidth;
			EditorGUILayout.PropertyField(property, label ?? TempContent(property.displayName, null, property.tooltip));
			EditorGUIUtility.labelWidth = 0; // Resets to default
		}

		public static void PropertyFieldFitLabel (SerializedProperty property, GUIContent label = null, float extraSpace = 5f) {
			label = label ?? TempContent(property.displayName, null, property.tooltip);
			float width = GUI.skin.label.CalcSize(TempContent(label.text)).x + extraSpace;
			if (label.image != null)
				width += EditorGUIUtility.singleLineHeight;
			PropertyFieldWideLabel(property, label, width);
		}

		/// <summary>Multi-edit-compatible version of EditorGUILayout.ToggleLeft(SerializedProperty)</summary>
		public static void ToggleLeftLayout (SerializedProperty property, GUIContent label = null, float width = 120f) {
			if (label == null) label = SpineInspectorUtility.TempContent(property.displayName, tooltip: property.tooltip);

			if (property.hasMultipleDifferentValues) {
				bool previousShowMixedValue = EditorGUI.showMixedValue;
				EditorGUI.showMixedValue = true;

				bool clicked = EditorGUILayout.ToggleLeft(label, property.boolValue, GUILayout.Width(width));
				if (clicked) property.boolValue = true; // Set all values to true when clicked.

				EditorGUI.showMixedValue = previousShowMixedValue;
			} else {
				property.boolValue = EditorGUILayout.ToggleLeft(label, property.boolValue, GUILayout.Width(width));
			}
		}

		/// <summary>Multi-edit-compatible version of EditorGUILayout.ToggleLeft(SerializedProperty)</summary>
		public static void ToggleLeft (Rect rect, SerializedProperty property, GUIContent label = null) {
			if (label == null) label = SpineInspectorUtility.TempContent(property.displayName, tooltip: property.tooltip);

			if (property.hasMultipleDifferentValues) {
				bool previousShowMixedValue = EditorGUI.showMixedValue;
				EditorGUI.showMixedValue = true;

				bool clicked = EditorGUI.ToggleLeft(rect, label, property.boolValue);
				if (clicked) property.boolValue = true; // Set all values to true when clicked.

				EditorGUI.showMixedValue = previousShowMixedValue;
			} else {
				property.boolValue = EditorGUI.ToggleLeft(rect, label, property.boolValue);
			}
		}

		public static bool UndoRedoPerformed (UnityEngine.Event current) {
			return current.type == EventType.ValidateCommand && current.commandName == "UndoRedoPerformed";
		}

		public static Texture2D UnityIcon<T>() {
			return EditorGUIUtility.ObjectContent(null, typeof(T)).image as Texture2D;
		}

		public static Texture2D UnityIcon(System.Type type) {
			return EditorGUIUtility.ObjectContent(null, type).image as Texture2D;
		}

		public static FieldInfo GetNonPublicField (System.Type type, string fieldName) {
			return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		}

		#region SerializedProperty Helpers
		public static SerializedProperty FindBaseOrSiblingProperty (this SerializedProperty property, string propertyName) {
			if (string.IsNullOrEmpty(propertyName)) return null;

			SerializedProperty relativeProperty = property.serializedObject.FindProperty(propertyName); // baseProperty

			// If base property is not found, look for the sibling property.
			if (relativeProperty == null) {
				string propertyPath = property.propertyPath;
				int localPathLength = property.name.Length;

				string newPropertyPath = propertyPath.Remove(propertyPath.Length - localPathLength, localPathLength) + propertyName;
				relativeProperty = property.serializedObject.FindProperty(newPropertyPath);

				// If a direct sibling property was not found, try to find the sibling of the array.
				if (relativeProperty == null && property.isArray) {
					int propertyPathLength = propertyPath.Length;

					int dotCount = 0;
					const int SiblingOfListDotCount = 3;
					for (int i = 1; i < propertyPathLength; i++) {
						if (propertyPath[propertyPathLength - i] == '.') {
							dotCount++;
							if (dotCount >= SiblingOfListDotCount) {
								localPathLength = i - 1;
								break;
							}
						}
					}

					newPropertyPath = propertyPath.Remove(propertyPath.Length - localPathLength, localPathLength) + propertyName;
					relativeProperty = property.serializedObject.FindProperty(newPropertyPath);
				}
			}

			return relativeProperty;
		}
		#endregion

		#region Layout Scopes
		static GUIStyle grayMiniLabel;
		public static GUIStyle GrayMiniLabel {
			get {
				if (grayMiniLabel == null) {
					grayMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
						alignment = TextAnchor.UpperLeft
					};
				}
				return grayMiniLabel;
			}
		}

		public class LabelWidthScope : System.IDisposable {
			public LabelWidthScope (float minimumLabelWidth = 190f) {
				EditorGUIUtility.labelWidth = minimumLabelWidth;
			}

			public void Dispose () {
				EditorGUIUtility.labelWidth = 0f;
			}
		}

		public class IndentScope : System.IDisposable {
			public IndentScope () { EditorGUI.indentLevel++; }
			public void Dispose () { EditorGUI.indentLevel--; }
		}

		public class BoxScope : System.IDisposable {
			readonly bool indent;

			static GUIStyle boxScopeStyle;
			public static GUIStyle BoxScopeStyle {
				get {
					if (boxScopeStyle == null) {
						boxScopeStyle = new GUIStyle(EditorStyles.helpBox);
						RectOffset p = boxScopeStyle.padding; // RectOffset is a class
						p.right += 6;
						p.top += 1;
						p.left += 3;
					}

					return boxScopeStyle;
				}
			}

			public BoxScope (bool indent = true) {
				this.indent = indent;
				EditorGUILayout.BeginVertical(BoxScopeStyle);
				if (indent) EditorGUI.indentLevel++;
			}

			public void Dispose () {
				if (indent) EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
			}
		}
		#endregion

		#region Button
		const float CenterButtonMaxWidth = 270f;
		const float CenterButtonHeight = 30f;
		static GUIStyle spineButtonStyle;
		static GUIStyle SpineButtonStyle {
			get {
				if (spineButtonStyle == null) {
					spineButtonStyle = new GUIStyle(GUI.skin.button);
					spineButtonStyle.padding = new RectOffset(10, 10, 10, 10);
				}
				return spineButtonStyle;
			}
		}

		public static bool LargeCenteredButton (string label, bool sideSpace = true, float maxWidth = CenterButtonMaxWidth) {
			if (sideSpace) {
				bool clicked;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					clicked = GUILayout.Button(label, SpineButtonStyle, GUILayout.MaxWidth(maxWidth), GUILayout.Height(CenterButtonHeight));
					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
				return clicked;
			} else {
				return GUILayout.Button(label, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
			}
		}

		public static bool LargeCenteredButton (GUIContent content, bool sideSpace = true, float maxWidth = CenterButtonMaxWidth) {
			if (sideSpace) {
				bool clicked;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					clicked = GUILayout.Button(content, SpineButtonStyle, GUILayout.MaxWidth(maxWidth), GUILayout.Height(CenterButtonHeight));
					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
				return clicked;
			} else {
				return GUILayout.Button(content, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
			}
		}

		public static bool CenteredButton (GUIContent content, float height = 20f, bool sideSpace = true, float maxWidth = CenterButtonMaxWidth) {
			if (sideSpace) {
				bool clicked;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					clicked = GUILayout.Button(content, GUILayout.MaxWidth(maxWidth), GUILayout.Height(height));
					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
				return clicked;
			} else {
				return GUILayout.Button(content, GUILayout.MaxWidth(maxWidth), GUILayout.Height(height));
			}
		}
		#endregion

		#region Multi-Editing Helpers
		public static bool TargetsUseSameData (SerializedObject so) {
			if (so.isEditingMultipleObjects) {
				int n = so.targetObjects.Length;
				var first = so.targetObjects[0] as IHasSkeletonDataAsset;
				for (int i = 1; i < n; i++) {
					var sr = so.targetObjects[i] as IHasSkeletonDataAsset;
					if (sr != null && sr.SkeletonDataAsset != first.SkeletonDataAsset)
						return false;
				}
			}
			return true;
		}

		public static SerializedObject GetRenderersSerializedObject (SerializedObject serializedObject) {
			if (serializedObject.isEditingMultipleObjects) {
				var renderers = new List<Object>();
				foreach (var o in serializedObject.targetObjects) {
					var component = o as Component;
					if (component != null) {
						var renderer = component.GetComponent<Renderer>();
						if (renderer != null)
							renderers.Add(renderer);
					}
				}
				return new SerializedObject(renderers.ToArray());
			} else {
				var component = serializedObject.targetObject as Component;
				if (component != null) {
					var renderer = component.GetComponent<Renderer>();
					if (renderer != null)
						return new SerializedObject(renderer);
				}
			}

			return null;
		}
		#endregion

		#region Sorting Layer Field Helpers
		static readonly GUIContent SortingLayerLabel = new GUIContent("Sorting Layer", "MeshRenderer.sortingLayerID");
		static readonly GUIContent OrderInLayerLabel = new GUIContent("Order in Layer", "MeshRenderer.sortingOrder");

		static MethodInfo m_SortingLayerFieldMethod;
		static MethodInfo SortingLayerFieldMethod {
			get {
				if (m_SortingLayerFieldMethod == null)
					m_SortingLayerFieldMethod = typeof(EditorGUILayout).GetMethod("SortingLayerField", BindingFlags.Static | BindingFlags.NonPublic, null, new [] { typeof(GUIContent), typeof(SerializedProperty), typeof(GUIStyle) }, null);

				return m_SortingLayerFieldMethod;
			}
		}

		public struct SerializedSortingProperties {
			public SerializedObject renderer;
			public SerializedProperty sortingLayerID;
			public SerializedProperty sortingOrder;

			public SerializedSortingProperties (Renderer r) : this(new SerializedObject(r)) {}
			public SerializedSortingProperties (Object[] renderers) : this(new SerializedObject(renderers)) {}

			public SerializedSortingProperties (SerializedObject rendererSerializedObject) {
				renderer = rendererSerializedObject;
				sortingLayerID = renderer.FindProperty("m_SortingLayerID");
				sortingOrder = renderer.FindProperty("m_SortingOrder");
			}

			public void ApplyModifiedProperties () {
				renderer.ApplyModifiedProperties();

				// SetDirty
				if (renderer.isEditingMultipleObjects)
					foreach (var o in renderer.targetObjects)
						EditorUtility.SetDirty(o);
				else
					EditorUtility.SetDirty(renderer.targetObject);
			}
		}

		public static void SortingPropertyFields (SerializedSortingProperties prop, bool applyModifiedProperties) {
			if (applyModifiedProperties)
				EditorGUI.BeginChangeCheck();

			if (SpineInspectorUtility.SortingLayerFieldMethod != null && prop.sortingLayerID != null)
				SpineInspectorUtility.SortingLayerFieldMethod.Invoke(null, new object[] { SortingLayerLabel, prop.sortingLayerID, EditorStyles.popup } );
			else
				EditorGUILayout.PropertyField(prop.sortingLayerID);

			EditorGUILayout.PropertyField(prop.sortingOrder, OrderInLayerLabel);

			if (applyModifiedProperties && EditorGUI.EndChangeCheck())
				prop.ApplyModifiedProperties();
		}
		#endregion
	}
}
