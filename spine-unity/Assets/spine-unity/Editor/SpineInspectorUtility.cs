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
			using (new EditorGUILayout.HorizontalScope()) {
				GUILayout.Label(label ?? new GUIContent(property.displayName, property.tooltip), GUILayout.MinWidth(minimumLabelWidth));
				//GUILayout.FlexibleSpace();
				EditorGUILayout.PropertyField(property, GUIContent.none, true, GUILayout.MinWidth(100));
			}
		}

		#region Sorting Layer Field Helpers
		static readonly GUIContent SortingLayerLabel = new GUIContent("Sorting Layer");
		static readonly GUIContent OrderInLayerLabel = new GUIContent("Order in Layer");

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
				this.SetDirty();
			}

			internal void SetDirty () {
				if (renderer.isEditingMultipleObjects)
					foreach (var o in renderer.targetObjects)
						EditorUtility.SetDirty(o);
				else
					EditorUtility.SetDirty(renderer.targetObject);
			}
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

		public static bool TargetsUseSameData (SerializedObject so) {
			bool multi = so.isEditingMultipleObjects;
			if (multi) {
				int n = so.targetObjects.Length;
				var first = so.targetObjects[0] as SkeletonRenderer;
				for (int i = 1; i < n; i++) {
					var sr = so.targetObjects[i] as SkeletonRenderer;
					if (sr != null && sr.skeletonDataAsset != first.skeletonDataAsset)
						return false;
				}
			}
			return true;
		}

		public static void SortingPropertyFields (SerializedSortingProperties prop, bool applyModifiedProperties) {
			if (applyModifiedProperties) {
				EditorGUI.BeginChangeCheck();
				SortingPropertyFields(prop.sortingLayerID, prop.sortingOrder);
				if(EditorGUI.EndChangeCheck())
					prop.ApplyModifiedProperties();
			} else {
				SortingPropertyFields(prop.sortingLayerID, prop.sortingOrder);
			}
		}

		public static void SortingPropertyFields (SerializedProperty m_SortingLayerID, SerializedProperty m_SortingOrder) {
			if (SpineInspectorUtility.SortingLayerFieldMethod != null && m_SortingLayerID != null) {
				SpineInspectorUtility.SortingLayerFieldMethod.Invoke(null, new object[] { SortingLayerLabel, m_SortingLayerID, EditorStyles.popup } );
			} else {
				EditorGUILayout.PropertyField(m_SortingLayerID);
			}

			EditorGUILayout.PropertyField(m_SortingOrder, OrderInLayerLabel);
		}
		#endregion
	}
}
