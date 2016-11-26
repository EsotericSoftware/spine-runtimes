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

		public static void PropertyFieldWideLabel (SerializedProperty property, GUIContent label = null, float minimumLabelWidth = 150) {
			EditorGUIUtility.labelWidth = minimumLabelWidth;
			EditorGUILayout.PropertyField(property, label ?? new GUIContent(property.displayName, property.tooltip));
			EditorGUIUtility.labelWidth = 0; // Resets to default
		}

		public static void PropertyFieldFitLabel (SerializedProperty property, GUIContent label = null, float extraSpace = 5f) {
			label = label ?? new GUIContent(property.displayName, property.tooltip);
			float width = GUI.skin.label.CalcSize(new GUIContent(label.text)).x + extraSpace;
			if (label.image != null)
				width += EditorGUIUtility.singleLineHeight;
			PropertyFieldWideLabel(property, label, width);

		}

		public static bool UndoRedoPerformed (UnityEngine.Event current) {
			return current.type == EventType.ValidateCommand && current.commandName == "UndoRedoPerformed";
		}

		#region Layout Scopes
		static GUIStyle grayMiniLabel;
		public static GUIStyle GrayMiniLabel {
			get {
				if (grayMiniLabel == null) {
					grayMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
					grayMiniLabel.alignment = TextAnchor.UpperLeft;
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
						var p = boxScopeStyle.padding;
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
		const float CenterButtonHeight = 35f;
		static GUIStyle spineButtonStyle;
		static GUIStyle SpineButtonStyle {
			get {
				if (spineButtonStyle == null) {
					spineButtonStyle = new GUIStyle(GUI.skin.button);
					spineButtonStyle.name = "Spine Button";
					spineButtonStyle.padding = new RectOffset(10, 10, 10, 10);
				}
				return spineButtonStyle;
			}
		}

		public static bool LargeCenteredButton (string label, bool sideSpace = true) {
			if (sideSpace) {
				bool clicked;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					clicked = GUILayout.Button(label, SpineButtonStyle, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
				return clicked;
			} else {
				return GUILayout.Button(label, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
			}
		}

		public static bool LargeCenteredButton (GUIContent content, bool sideSpace = true) {
			if (sideSpace) {
				bool clicked;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					clicked = GUILayout.Button(content, SpineButtonStyle, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
				return clicked;
			} else {
				return GUILayout.Button(content, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
			}
		}

		public static bool CenteredButton (GUIContent content, float height = 20f, bool sideSpace = true) {
			if (sideSpace) {
				bool clicked;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					clicked = GUILayout.Button(content, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(height));
					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
				return clicked;
			} else {
				return GUILayout.Button(content, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(height));
			}
		}
		#endregion

		#region Multi-Editing Helpers
		public static bool TargetsUseSameData (SerializedObject so) {
			if (so.isEditingMultipleObjects) {
				int n = so.targetObjects.Length;
				var first = so.targetObjects[0] as ISkeletonComponent;
				for (int i = 1; i < n; i++) {
					var sr = so.targetObjects[i] as ISkeletonComponent;
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

			/// <summary>
			/// Initializes a new instance of the
			/// <see cref="Spine.Unity.Editor.SpineInspectorUtility.SerializedSortingProperties"/> struct.
			/// </summary>
			/// <param name="rendererSerializedObject">SerializedObject of the renderer. Use 
			/// <see cref="Spine.Unity.Editor.SpineInspectorUtility.GetRenderersSerializedObject"/> to easily generate this.</param>
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
