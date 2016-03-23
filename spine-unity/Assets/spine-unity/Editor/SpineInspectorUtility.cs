/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Spine.Unity.Editor {
	public static class SpineInspectorUtility {

		public static string Pluralize (int n, string singular, string plural) {
			return n + " " + (n == 1 ? singular : plural);
		}

		public static string PluralThenS (int n) {
			return n == 1 ? "" : "s";
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

			public SerializedSortingProperties (Renderer r) {
				renderer = new SerializedObject(r);
				sortingLayerID = renderer.FindProperty("m_SortingLayerID");
				sortingOrder = renderer.FindProperty("m_SortingOrder");
			}

			public void ApplyModifiedProperties () {
				renderer.ApplyModifiedProperties();
			}
		}

		public static void SortingPropertyFields (SerializedSortingProperties prop, bool applyModifiedProperties) {
			if (applyModifiedProperties) {
				EditorGUI.BeginChangeCheck();
				SortingPropertyFields(prop.sortingLayerID, prop.sortingOrder);
				if(EditorGUI.EndChangeCheck()) {
					prop.ApplyModifiedProperties();
					EditorUtility.SetDirty(prop.renderer.targetObject);
				}
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
