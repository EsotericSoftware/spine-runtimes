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

using Spine.Unity;
using UnityEditor;
using UnityEngine;
using SpineInspectorUtility = Spine.Unity.Editor.SpineInspectorUtility;

public class SpineShaderWithOutlineGUI : ShaderGUI {

	protected MaterialEditor _materialEditor;
	bool _showAdvancedOutlineSettings = false;
	bool _showStencilSettings = false;

	MaterialProperty _OutlineWidth = null;
	MaterialProperty _OutlineColor = null;
	MaterialProperty _OutlineReferenceTexWidth = null;
	MaterialProperty _ThresholdEnd = null;
	MaterialProperty _OutlineSmoothness = null;
	MaterialProperty _Use8Neighbourhood = null;
	MaterialProperty _OutlineOpaqueAlpha = null;
	MaterialProperty _OutlineMipLevel = null;
	MaterialProperty _StencilComp = null;
	MaterialProperty _StencilRef = null;

	static GUIContent _EnableOutlineText = new GUIContent("Outline", "Enable outline rendering. Draws an outline by sampling 4 or 8 neighbourhood pixels at a given distance specified via 'Outline Width'.");
	static GUIContent _OutlineWidthText = new GUIContent("Outline Width", "");
	static GUIContent _OutlineColorText = new GUIContent("Outline Color", "");
	static GUIContent _OutlineReferenceTexWidthText = new GUIContent("Reference Texture Width", "");
	static GUIContent _ThresholdEndText = new GUIContent("Outline Threshold", "");
	static GUIContent _OutlineSmoothnessText = new GUIContent("Outline Smoothness", "");
	static GUIContent _Use8NeighbourhoodText = new GUIContent("Sample 8 Neighbours", "");
	static GUIContent _OutlineOpaqueAlphaText = new GUIContent("Opaque Alpha", "If a pixel's alpha value is above this threshold it will not receive any outline color overlay. Use to exclude problematic semi-transparent areas.");
	static GUIContent _OutlineMipLevelText = new GUIContent("Outline Mip Level", "");
	static GUIContent _StencilCompText = new GUIContent("Stencil Comparison", "");
	static GUIContent _StencilRefText = new GUIContent("Stencil Reference", "");

	static GUIContent _OutlineAdvancedText = new GUIContent("Advanced", "");
	static GUIContent _ShowStencilText = new GUIContent("Stencil", "Stencil parameters for mask interaction.");

	protected const string ShaderOutlineNamePrefix = "Spine/Outline/";
	protected const string ShaderNormalNamePrefix = "Spine/";
	protected const string ShaderWithoutStandardVariantSuffix = "OutlineOnly";

	#region ShaderGUI

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties) {
		FindProperties(properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
		_materialEditor = materialEditor;

		base.OnGUI(materialEditor, properties);
		EditorGUILayout.Space();
		RenderStencilProperties();
		EditorGUILayout.Space();
		RenderOutlineProperties();
	}

	#endregion

	#region Virtual Interface

	protected virtual void FindProperties (MaterialProperty[] props) {

		_OutlineWidth = FindProperty("_OutlineWidth", props, false);
		_OutlineReferenceTexWidth = FindProperty("_OutlineReferenceTexWidth", props, false);
		_OutlineColor = FindProperty("_OutlineColor", props, false);
		_ThresholdEnd = FindProperty("_ThresholdEnd", props, false);
		_OutlineSmoothness = FindProperty("_OutlineSmoothness", props, false);
		_Use8Neighbourhood = FindProperty("_Use8Neighbourhood", props, false);
		_OutlineOpaqueAlpha = FindProperty("_OutlineOpaqueAlpha", props, false);
		_OutlineMipLevel = FindProperty("_OutlineMipLevel", props, false);

		_StencilComp = FindProperty("_StencilComp", props, false);
		_StencilRef = FindProperty("_StencilRef", props, false);
		if (_StencilRef == null)
			_StencilRef = FindProperty("_Stencil", props, false);
	}

	protected virtual void RenderStencilProperties () {
		if (_StencilComp == null)
			return; // not a shader supporting custom stencil operations

		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;
		_showStencilSettings = EditorGUILayout.Foldout(_showStencilSettings, _ShowStencilText);
		if (_showStencilSettings) {
			using (new SpineInspectorUtility.IndentScope()) {
				_materialEditor.ShaderProperty(_StencilComp, _StencilCompText);
				_materialEditor.ShaderProperty(_StencilRef, _StencilRefText);
			}
		}
	}

	protected virtual void RenderOutlineProperties () {

		if (_OutlineWidth == null)
			return; // not an outline shader

		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;

		bool mixedValue;
		bool hasOutlineVariant = !IsShaderWithoutStandardVariantShader(_materialEditor, out mixedValue);
		bool isOutlineEnabled = true;
		if (hasOutlineVariant) {
			isOutlineEnabled = IsOutlineEnabled(_materialEditor, out mixedValue);
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
		} else {
			var origFontStyle = EditorStyles.label.fontStyle;
			EditorStyles.label.fontStyle = FontStyle.Bold;
			EditorGUILayout.LabelField(_EnableOutlineText);
			EditorStyles.label.fontStyle = origFontStyle;
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
					_materialEditor.ShaderProperty(_OutlineOpaqueAlpha, _OutlineOpaqueAlphaText);
					_materialEditor.ShaderProperty(_OutlineMipLevel, _OutlineMipLevelText);
				}
			}
		}
	}

	#endregion

	#region Private Functions

	void SwitchShaderToOutlineSettings (Material material, bool enableOutline) {

		var shaderName = material.shader.name;
		bool isSetToOutlineShader = shaderName.Contains(ShaderOutlineNamePrefix);
		if (isSetToOutlineShader && !enableOutline) {
			shaderName = shaderName.Replace(ShaderOutlineNamePrefix, ShaderNormalNamePrefix);
			_materialEditor.SetShader(Shader.Find(shaderName), false);
			return;
		} else if (!isSetToOutlineShader && enableOutline) {
			shaderName = shaderName.Replace(ShaderNormalNamePrefix, ShaderOutlineNamePrefix);
			_materialEditor.SetShader(Shader.Find(shaderName), false);
			return;
		}
	}

	static bool IsOutlineEnabled (MaterialEditor editor, out bool mixedValue) {
		mixedValue = false;
		bool isAnyEnabled = false;
		foreach (Material material in editor.targets) {
			if (material.shader.name.Contains(ShaderOutlineNamePrefix)) {
				isAnyEnabled = true;
			} else if (isAnyEnabled) {
				mixedValue = true;
			}
		}
		return isAnyEnabled;
	}

	static bool IsShaderWithoutStandardVariantShader (MaterialEditor editor, out bool mixedValue) {
		mixedValue = false;
		bool isAnyShaderWithoutVariant = false;
		foreach (Material material in editor.targets) {
			if (material.shader.name.Contains(ShaderWithoutStandardVariantSuffix)) {
				isAnyShaderWithoutVariant = true;
			} else if (isAnyShaderWithoutVariant) {
				mixedValue = true;
			}
		}
		return isAnyShaderWithoutVariant;
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
