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
using Spine.Unity;

using SpineInspectorUtility = Spine.Unity.Editor.SpineInspectorUtility;

public class SpineShaderWithOutlineGUI : ShaderGUI {

	protected MaterialEditor _materialEditor;
	bool _showAdvancedOutlineSettings = false;

	MaterialProperty _OutlineWidth = null;
	MaterialProperty _OutlineColor = null;
	MaterialProperty _OutlineReferenceTexWidth = null;
	MaterialProperty _ThresholdEnd = null;
	MaterialProperty _OutlineSmoothness = null;
	MaterialProperty _Use8Neighbourhood = null;
	MaterialProperty _OutlineMipLevel = null;

	static GUIContent _EnableOutlineText = new GUIContent("Outline", "Enable outline rendering. Draws an outline by sampling 4 or 8 neighbourhood pixels at a given distance specified via 'Outline Width'.");
	static GUIContent _OutlineWidthText = new GUIContent("Outline Width", "");
	static GUIContent _OutlineColorText = new GUIContent("Outline Color", "");
	static GUIContent _OutlineReferenceTexWidthText = new GUIContent("Reference Texture Width", "");
	static GUIContent _ThresholdEndText = new GUIContent("Outline Threshold", "");
	static GUIContent _OutlineSmoothnessText = new GUIContent("Outline Smoothness", "");
	static GUIContent _Use8NeighbourhoodText = new GUIContent("Sample 8 Neighbours", "");
	static GUIContent _OutlineMipLevelText = new GUIContent("Outline Mip Level", "");

	static GUIContent _OutlineAdvancedText = new GUIContent("Advanced", "");

	protected const string ShaderOutlineNamePrefix = "Spine/Outline/";
	protected const string ShaderNormalNamePrefix = "Spine/";

	#region ShaderGUI

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties) {
		FindProperties(properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
		_materialEditor = materialEditor;

		base.OnGUI(materialEditor, properties);
		EditorGUILayout.Space();
		RenderOutlineProperties();
	}

	#endregion

	#region Virtual Interface

	protected virtual void FindProperties (MaterialProperty[] props) {
		_OutlineWidth = FindProperty("_OutlineWidth", props);
		_OutlineReferenceTexWidth = FindProperty("_OutlineReferenceTexWidth", props);
		_OutlineColor = FindProperty("_OutlineColor", props);
		_ThresholdEnd = FindProperty("_ThresholdEnd", props);
		_OutlineSmoothness = FindProperty("_OutlineSmoothness", props);
		_Use8Neighbourhood = FindProperty("_Use8Neighbourhood", props);
		_OutlineMipLevel = FindProperty("_OutlineMipLevel", props);
	}

	protected virtual void RenderOutlineProperties () {

		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;

		bool mixedValue;
		bool isOutlineEnabled = IsOutlineEnabled(_materialEditor, out mixedValue);
		EditorGUI.showMixedValue = mixedValue;
		EditorGUI.BeginChangeCheck();

		var origFontStyle = EditorStyles.label.fontStyle;
		EditorStyles.label.fontStyle = FontStyle.Bold;
		isOutlineEnabled = EditorGUILayout.Toggle(_EnableOutlineText, isOutlineEnabled);
		EditorStyles.label.fontStyle = origFontStyle;
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck()) {
			foreach (Material material in _materialEditor.targets) {
				SwitchShaderToOutlineSettings(material, isOutlineEnabled);
			}
		}

		if (isOutlineEnabled) {
			_materialEditor.ShaderProperty(_OutlineWidth, _OutlineWidthText);
			_materialEditor.ShaderProperty(_OutlineColor, _OutlineColorText);

			_showAdvancedOutlineSettings = EditorGUILayout.Foldout(_showAdvancedOutlineSettings, _OutlineAdvancedText);
			if (_showAdvancedOutlineSettings) {
				using (new SpineInspectorUtility.IndentScope()) {
					_materialEditor.ShaderProperty(_OutlineReferenceTexWidth, _OutlineReferenceTexWidthText);
					_materialEditor.ShaderProperty(_ThresholdEnd, _ThresholdEndText);
					_materialEditor.ShaderProperty(_OutlineSmoothness, _OutlineSmoothnessText);
					_materialEditor.ShaderProperty(_Use8Neighbourhood, _Use8NeighbourhoodText);
					_materialEditor.ShaderProperty(_OutlineMipLevel, _OutlineMipLevelText);
				}
			}
		}
	}

	#endregion

	#region Private Functions

	void SwitchShaderToOutlineSettings (Material material, bool enableOutline) {

		var shaderName = material.shader.name;
		bool isSetToOutlineShader = shaderName.StartsWith(ShaderOutlineNamePrefix);
		if (isSetToOutlineShader && !enableOutline) {
			shaderName = shaderName.Replace(ShaderOutlineNamePrefix, ShaderNormalNamePrefix);
			_materialEditor.SetShader(Shader.Find(shaderName), false);
			return;
		}
		else if (!isSetToOutlineShader && enableOutline) {
			shaderName = shaderName.Replace(ShaderNormalNamePrefix, ShaderOutlineNamePrefix);
			_materialEditor.SetShader(Shader.Find(shaderName), false);
			return;
		}
	}

	static bool IsOutlineEnabled (MaterialEditor editor, out bool mixedValue) {
		mixedValue = false;
		bool isAnyEnabled = false;
		foreach (Material material in editor.targets) {
			if (material.shader.name.StartsWith(ShaderOutlineNamePrefix)) {
				isAnyEnabled = true;
			}
			else if (isAnyEnabled) {
				mixedValue = true;
			}
		}
		return isAnyEnabled;
	}

	static bool BoldToggleField (GUIContent label, bool value) {
		FontStyle origFontStyle = EditorStyles.label.fontStyle;
		EditorStyles.label.fontStyle = FontStyle.Bold;
		value = EditorGUILayout.Toggle(label, value, EditorStyles.toggle);
		EditorStyles.label.fontStyle = origFontStyle;
		return value;
	}

	#endregion
}
