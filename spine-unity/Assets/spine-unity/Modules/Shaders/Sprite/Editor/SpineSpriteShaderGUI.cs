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

using System;
using UnityEngine;
using UnityEditor;

using SpineInspectorUtility = Spine.Unity.Editor.SpineInspectorUtility;

public class SpineSpriteShaderGUI : ShaderGUI {
	
	#region Constants
	const string ShaderVertexLit = "Spine/Sprite/Vertex Lit";
	const string ShaderPixelLit = "Spine/Sprite/Pixel Lit";
	const string ShaderUnlit = "Spine/Sprite/Unlit";

	const int SolidQueue = 2000;
	const int AlphaTestQueue = 2450;
	const int TransparentQueue = 3000;

	const string PremultipledAlpha = "_ALPHAPREMULTIPLY_ON";
	const string Multiply = "_MULTIPLYBLEND";
	const string Multiply2x = "_MULTIPLYBLEND_X2";
	const string Additive = "_ADDITIVEBLEND";
	const string SoftAdditive = "_ADDITIVEBLEND_SOFT";

	const string _FIXED_NORMALS = "_FIXED_NORMALS";
	const string _FIXED_NORMALS_BACK_RENDERING = "_FIXED_NORMALS_BACK_RENDERING";

	const string _SPHERICAL_HARMONICS = "_SPHERICAL_HARMONICS";

	enum eBlendMode {
		PreMultipliedAlpha,
		StandardAlpha,
		Solid,
		Additive,
		SoftAdditive,
		Multiply,
		Multiplyx2,
	}

	enum eLightMode { VertexLit, PixelLit, Unlit, }

	enum eCulling {
		Off = 0,
		Back = 2,
		Front = 1,
	}
	#endregion

	MaterialProperty _mainTexture = null;
	MaterialProperty _color = null;

	MaterialProperty _emissionMap = null;
	MaterialProperty _emissionColor = null;
	MaterialProperty _emissionPower = null;

	MaterialProperty _writeToDepth = null;
	MaterialProperty _depthAlphaCutoff = null;
	MaterialProperty _shadowAlphaCutoff = null;
	MaterialProperty _renderQueue = null;
	MaterialProperty _culling = null;

	MaterialProperty _overlayColor = null;
	MaterialProperty _hue = null;
	MaterialProperty _saturation = null;
	MaterialProperty _brightness = null;

	MaterialProperty _rimPower = null;
	MaterialProperty _rimColor = null;

	MaterialEditor _materialEditor;

	//Normals
	MaterialProperty _bumpMap = null;
	MaterialProperty _diffuseRamp = null;
	MaterialProperty _fixedNormal = null;

	//Blend texture
	MaterialProperty _blendTexture = null;
	MaterialProperty _blendTextureLerp = null;

	bool _firstTimeApply = true;
	eLightMode _lightMode;
	static bool showAdvanced = false;

	#region ShaderGUI
	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)	{
		FindProperties(properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
		_materialEditor = materialEditor;
		Material material = materialEditor.target as Material;

		ShaderPropertiesGUI(material);

		// Make sure that needed keywords are set up if we're switching some existing
		// material to a standard shader.
		if (_firstTimeApply) {
			SetMaterialKeywords(material);
			SetLightModeFromShader(material);
			_firstTimeApply = false;
		}
	}

	public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader) {
		base.AssignNewShaderToMaterial(material, oldShader, newShader);

		if (!(oldShader == Shader.Find(ShaderPixelLit) || oldShader == Shader.Find(ShaderVertexLit) || oldShader == Shader.Find(ShaderUnlit)))
			SetDefaultSpriteKeywords(material, newShader);

		SetMaterialKeywords(material);
		SetLightModeFromShader(material);
	}


	#endregion

	#region Virtual Interface
	protected virtual void FindProperties (MaterialProperty[] props) {
		_mainTexture = FindProperty("_MainTex", props);
		_color = FindProperty("_Color", props);

		_emissionMap = FindProperty("_EmissionMap", props, false);
		_emissionColor = FindProperty("_EmissionColor", props, false);
		_emissionPower = FindProperty("_EmissionPower", props, false);

		_writeToDepth = FindProperty("_ZWrite", props);
		_depthAlphaCutoff = FindProperty("_Cutoff", props);
		_shadowAlphaCutoff = FindProperty("_ShadowAlphaCutoff", props);
		_renderQueue = FindProperty("_RenderQueue", props);
		_culling = FindProperty("_Cull", props);

		_bumpMap = FindProperty("_BumpMap", props, false);
		_diffuseRamp = FindProperty("_DiffuseRamp", props, false);
		_fixedNormal = FindProperty("_FixedNormal", props, false);
		_blendTexture = FindProperty("_BlendTex", props, false);
		_blendTextureLerp = FindProperty("_BlendAmount", props, false);

		_overlayColor = FindProperty("_OverlayColor", props, false);
		_hue = FindProperty("_Hue", props, false);
		_saturation = FindProperty("_Saturation", props, false);
		_brightness = FindProperty("_Brightness", props, false);

		_rimPower = FindProperty("_RimPower", props, false);
		_rimColor = FindProperty("_RimColor", props, false);
	}

	protected virtual void ShaderPropertiesGUI (Material material) {
		using (new EditorGUILayout.HorizontalScope()) {
			GUILayout.FlexibleSpace();
			var showAdvancedLabel = new GUIContent("Show Advanced", "Show extra options under all sections. This only affects the inspector. The Material's resulting shader is still compiled/optimized based on what features you actually use and don't use.");
			float lw = GUI.skin.toggle.CalcSize(showAdvancedLabel).x;

			EditorGUIUtility.labelWidth = lw;
			showAdvanced = EditorGUILayout.Toggle(showAdvancedLabel, showAdvanced);
			EditorGUIUtility.labelWidth = 0f;
		}

		EditorGUILayout.Space();

		EditorGUI.BeginChangeCheck();
		{
			LightingModePopup();
			BlendModePopup();

			if (showAdvanced) {
				EditorGUILayout.Space();
				EditorGUI.BeginChangeCheck();
				int renderQueue = EditorGUILayout.IntSlider("Renderer Queue Offset", (int)_renderQueue.floatValue, 0, 49);
				if (EditorGUI.EndChangeCheck()) material.SetInt("_RenderQueue", renderQueue);

				EditorGUI.BeginChangeCheck();
				eCulling culling = (eCulling)Mathf.RoundToInt(_culling.floatValue);
				culling = (eCulling)EditorGUILayout.EnumPopup("Culling", culling);
				if (EditorGUI.EndChangeCheck()) material.SetInt("_Cull", (int)culling);

				EditorGUI.BeginChangeCheck();
				bool fog = EditorGUILayout.Toggle("Use fog", material.IsKeywordEnabled("_FOG"));
				if (EditorGUI.EndChangeCheck()) SetKeyword(material, "_FOG", fog);

				EditorGUI.BeginChangeCheck();
				bool enabled = EditorGUILayout.Toggle(
					new GUIContent(
						"Use Spherical Harmonics",
						"Enable to use spherical harmonics to calculate ambient light / light probes. In vertex-lit mode this will be approximated from scenes ambient trilight settings."),
					material.IsKeywordEnabled(_SPHERICAL_HARMONICS)
				);
				if (EditorGUI.EndChangeCheck())
					SetKeyword(material, _SPHERICAL_HARMONICS, enabled);
			}

			using (new SpineInspectorUtility.BoxScope())
				RenderTextureProperties("Main Maps", material);

			if (showAdvanced) {
				using (new SpineInspectorUtility.BoxScope()) {
					Heading("Depth and Cast Shadow");
					EditorGUI.BeginChangeCheck();
					bool writeTodepth = EditorGUILayout.Toggle(new GUIContent("Write to Depth", "Write to Depth Buffer by clipping alpha."), _writeToDepth.floatValue != 0.0f);
					if (EditorGUI.EndChangeCheck())
						material.SetInt("_ZWrite", writeTodepth ? 1 : 0);

					if (writeTodepth)
						_materialEditor.RangeProperty(_depthAlphaCutoff, "Depth Alpha Cutoff");

					EditorGUILayout.Space();

					_materialEditor.RangeProperty(_shadowAlphaCutoff, "Shadow Alpha Cutoff");
				}

				if (_fixedNormal != null) {
					using (new SpineInspectorUtility.BoxScope()) {
						Heading("Normals");
						bool meshNormals = UseMeshNormalsCheckbox(material);
						if (!meshNormals) {
							Vector3 normal;
							EditorGUI.BeginChangeCheck();
							normal = showAdvanced ? EditorGUILayout.Vector3Field(new GUIContent("Fixed Normal", "Defined in Camera Space. Should normally be (0,0,-1)."), _fixedNormal.vectorValue) : (Vector3)_fixedNormal.vectorValue;
							if (EditorGUI.EndChangeCheck())
								_fixedNormal.vectorValue = new Vector4(normal.x, normal.y, normal.z, 1.0f);

							bool backRendering;
							EditorGUI.BeginChangeCheck();
							if (showAdvanced) {
								backRendering = EditorGUILayout.Toggle(new GUIContent("Fixed Normal Back Rendering", "Tick only if you are going to rotate the sprite to face away from the camera, the fixed normal will be flipped to compensate."), 
									material.IsKeywordEnabled(_FIXED_NORMALS_BACK_RENDERING));
							} else {
								backRendering = material.IsKeywordEnabled(_FIXED_NORMALS_BACK_RENDERING);
							}
							if (EditorGUI.EndChangeCheck()) {
								SetKeyword(material, _FIXED_NORMALS_BACK_RENDERING, backRendering);
								SetKeyword(material, _FIXED_NORMALS, !backRendering);
							}
						}
					}
				}
			} else {
				using (new SpineInspectorUtility.BoxScope()) {
					EditorGUI.BeginChangeCheck();
					bool writeTodepth = EditorGUILayout.Toggle(new GUIContent("Write to Depth", "Write to Depth Buffer by clipping alpha."), _writeToDepth.floatValue != 0.0f);
					if (EditorGUI.EndChangeCheck())
						material.SetInt("_ZWrite", writeTodepth ? 1 : 0);

					if (_fixedNormal != null)
						UseMeshNormalsCheckbox(material);
				}
			}

			using (new SpineInspectorUtility.BoxScope())
				RenderColorProperties("Color Adjustment", material);

			if (_emissionMap != null && _emissionColor != null && _rimColor != null) {
				Heading("Extra Lighting");

				if (_emissionMap != null && _emissionColor != null)
					using (new SpineInspectorUtility.BoxScope())
						RenderEmissionProperties(material);

				if (_rimColor != null)
					using (new SpineInspectorUtility.BoxScope())
						RenderRimLightingProperties(material);
			}
		}
		if (EditorGUI.EndChangeCheck())	{
			MaterialChanged(material);
		}
	}

	protected virtual void RenderTextureProperties (string label, Material material) {
		if (showAdvanced)
			Heading(label);

		_materialEditor.TexturePropertySingleLine(new GUIContent(showAdvanced ? "Albedo" : "Main Texture"), _mainTexture, _color);

		if (_bumpMap != null)
			_materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), _bumpMap);

		if (showAdvanced) {
			if (_blendTexture != null) {
				EditorGUI.BeginChangeCheck();
				_materialEditor.TexturePropertySingleLine(new GUIContent("Blend Texture", "When a blend texture is set the albedo will be a mix of the blend texture and main texture based on the blend amount."), _blendTexture, _blendTextureLerp);
				if (EditorGUI.EndChangeCheck())
					SetKeyword(material, "_TEXTURE_BLEND", _blendTexture != null);
			}
		}

		if (_diffuseRamp != null) {
			EditorGUILayout.Space();
			_materialEditor.TexturePropertySingleLine(new GUIContent("Diffuse Ramp", "A gradient can be used to create a 'Toon Shading' effect."), _diffuseRamp);
		}

		//		if (showAdvanced)
		//			_materialEditor.TextureScaleOffsetProperty(_mainTexture);
	}

	bool UseMeshNormalsCheckbox (Material material) {
		EditorGUI.BeginChangeCheck();
		bool fixedNormals = material.IsKeywordEnabled(_FIXED_NORMALS);
		bool fixedNormalsBackRendering = material.IsKeywordEnabled(_FIXED_NORMALS_BACK_RENDERING);
		bool meshNormals = EditorGUILayout.Toggle(new GUIContent("Use Mesh Normals", "If this is unticked, a Fixed Normal value will be used instead of the vertex normals on the mesh. Using a fixed normal is better for performance and can result in better looking lighting effects on 2d objects."), 
			!fixedNormals && !fixedNormalsBackRendering);
		if (EditorGUI.EndChangeCheck()) {
			SetKeyword(material, _FIXED_NORMALS, meshNormals ? false : fixedNormalsBackRendering ? false : true);
			SetKeyword(material, _FIXED_NORMALS_BACK_RENDERING, meshNormals ? false : fixedNormalsBackRendering);
		}
		return meshNormals;
	}

	protected virtual void RenderColorProperties (string label, Material material) {
		if (ToggleHeadingKeyword(label, material, "_COLOR_ADJUST")) {
			_materialEditor.ColorProperty(_overlayColor, "Overlay Color");
			EditorGUILayout.Space();
			using (new SpineInspectorUtility.IndentScope()) {
				_materialEditor.RangeProperty(_hue, "Hue");
				_materialEditor.RangeProperty(_saturation, "Saturation");
				_materialEditor.RangeProperty(_brightness, "Brightness");
			}
		}
	}

	protected virtual void RenderEmissionProperties (Material material) {
		if (ToggleHeadingKeyword("Emission", material, "_EMISSION")) {
			_materialEditor.TexturePropertyWithHDRColor(new GUIContent("Emission"), _emissionMap, _emissionColor, new ColorPickerHDRConfig(0,1, 0.01010101f, 3), true);
			_materialEditor.FloatProperty(_emissionPower, "Emission Power");
		}
	}

	protected virtual void RenderRimLightingProperties (Material material) {
		if (ToggleHeadingKeyword("Rim Lighting", material, "_RIM_LIGHTING")) {
			_materialEditor.ColorProperty(_rimColor, "Rim Color");
			_materialEditor.FloatProperty(_rimPower, "Rim Power");
		}
	}
	#endregion

	static void SetDefaultSpriteKeywords (Material material, Shader shader) {
		SetKeyword(material, "_EMISSION", false); // Disabled. standard shader sets this on by default.
		SetKeyword(material, PremultipledAlpha, true); // PMA by default
		SetKeyword(material, _FIXED_NORMALS, true); // Fixed normals by default, best for Spine.
		SetKeyword(material, _SPHERICAL_HARMONICS, true);
	}

	void SetLightModeFromShader (Material material) {
		if (material.shader.name == ShaderPixelLit)
			_lightMode = eLightMode.PixelLit;
		else if (material.shader.name == ShaderUnlit)
			_lightMode = eLightMode.Unlit;
		else
			_lightMode = eLightMode.VertexLit;
	}

	static void SetRenderQueue (Material material, string queue) {
		bool meshNormal = true;

		if (material.HasProperty("_FixedNormal")) {
			bool fixedNormals = material.IsKeywordEnabled(_FIXED_NORMALS);
			bool fixedNormalsBackRendering = material.IsKeywordEnabled(_FIXED_NORMALS_BACK_RENDERING);
			meshNormal = !fixedNormals && !fixedNormalsBackRendering;
		}

		material.SetOverrideTag("RenderType", meshNormal ? queue : "Sprite");
	}

	static void SetMaterialKeywords (Material material) {
		bool normalMap = material.HasProperty("_BumpMap") && material.GetTexture("_BumpMap") != null;
		SetKeyword(material, "_NORMALMAP", normalMap);

		bool zWrite = material.GetFloat("_ZWrite") > 0.0f;
		bool clipAlpha = zWrite && material.GetFloat("_Cutoff") > 0.0f;
		SetKeyword(material, "_ALPHA_CLIP", clipAlpha);

		bool diffuseRamp = material.HasProperty("_DiffuseRamp") && material.GetTexture("_DiffuseRamp") != null;
		SetKeyword(material, "_DIFFUSE_RAMP", diffuseRamp);

		bool blendTexture = material.HasProperty("_BlendTex") && material.GetTexture("_BlendTex") != null;
		SetKeyword(material, "_TEXTURE_BLEND", blendTexture);

		eBlendMode blendMode = GetMaterialBlendMode(material);
		SetBlendMode(material, blendMode);
	}

	static eBlendMode GetMaterialBlendMode (Material material) {
		if (material.IsKeywordEnabled(PremultipledAlpha))
			return eBlendMode.PreMultipliedAlpha;
		if (material.IsKeywordEnabled(Multiply))
			return eBlendMode.Multiply;
		if (material.IsKeywordEnabled(Multiply2x))
			return eBlendMode.Multiplyx2;
		if (material.IsKeywordEnabled(Additive))
			return eBlendMode.Additive;
		if (material.IsKeywordEnabled(SoftAdditive))
			return eBlendMode.SoftAdditive;

		return eBlendMode.StandardAlpha;
	}

	void BlendModePopup () {
		var material = _materialEditor.target as Material;
		eBlendMode blendMode = GetMaterialBlendMode(material);
		EditorGUI.BeginChangeCheck();
		blendMode = (eBlendMode)EditorGUILayout.Popup("Blend Mode", (int)blendMode, Enum.GetNames(typeof(eBlendMode)));

		if (EditorGUI.EndChangeCheck())
			SetBlendMode(material, blendMode);
	}

	static void SetBlendMode (Material material, eBlendMode blendMode) {
		SetKeyword(material, PremultipledAlpha, blendMode == eBlendMode.PreMultipliedAlpha);
		SetKeyword(material, Multiply, blendMode == eBlendMode.Multiply);
		SetKeyword(material, Multiply2x, blendMode == eBlendMode.Multiplyx2);
		SetKeyword(material, Additive, blendMode == eBlendMode.Additive);
		SetKeyword(material, SoftAdditive, blendMode == eBlendMode.SoftAdditive);

		int renderQueue;

		switch (blendMode) {
		case eBlendMode.Solid:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				SetRenderQueue(material, "Opaque");
				renderQueue = SolidQueue;
			}
			break;
		case eBlendMode.Additive:
			{ 
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
				SetRenderQueue(material, "Transparent");
				renderQueue = TransparentQueue;
			}
			break;
		case eBlendMode.SoftAdditive:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
				SetRenderQueue(material, "Transparent");
				renderQueue = TransparentQueue;
			}
			break;
		case eBlendMode.Multiply:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
				SetRenderQueue(material, "Transparent");
				renderQueue = TransparentQueue;
			}
			break;
		case eBlendMode.Multiplyx2:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
				SetRenderQueue(material, "Transparent");
				renderQueue = TransparentQueue;
			}
			break;
		case eBlendMode.PreMultipliedAlpha:
		case eBlendMode.StandardAlpha:
		default:
			{
				bool zWrite = material.GetFloat("_ZWrite") > 0.0f;
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				SetRenderQueue(material, zWrite ? "TransparentCutout" : "Transparent");
				renderQueue = zWrite ? AlphaTestQueue : TransparentQueue;
			}
			break;
		}

		material.renderQueue = renderQueue + material.GetInt("_RenderQueue");
	}

	static void Heading (string label) {
		GUILayout.Label(label, EditorStyles.boldLabel);
	}

	static bool ToggleHeadingKeyword (string label, Material material, string keyword) {
		int i = EditorGUI.indentLevel;
		var o = EditorStyles.label.fontStyle;
		EditorGUI.indentLevel = 0;
		EditorStyles.label.fontStyle = FontStyle.Bold;

		EditorGUI.BeginChangeCheck();
		bool r = EditorGUILayout.Toggle(new GUIContent(label, string.Format("This checkbox sets shader keyword: '{0}', which causes the Material to use extra shader features.", keyword)), material.IsKeywordEnabled(keyword));
		if (EditorGUI.EndChangeCheck())
			SetKeyword(material, keyword, r);

		EditorStyles.label.fontStyle = o;
		EditorGUI.indentLevel = i;
		return r;
	}

	static void MaterialChanged (Material material) {
		SetMaterialKeywords(material);
	}

	static void SetKeyword (Material m, string keyword, bool state) {
		if (state)
			m.EnableKeyword(keyword);
		else
			m.DisableKeyword(keyword);
	}

	void LightingModePopup () {
		EditorGUI.BeginChangeCheck();
		_lightMode = (eLightMode)EditorGUILayout.Popup("Lighting Mode", (int)_lightMode, Enum.GetNames(typeof(eLightMode)));
		if (EditorGUI.EndChangeCheck()) {
			var material = _materialEditor.target as Material;

			switch (_lightMode) {
			case eLightMode.VertexLit:
				if (material.shader.name != ShaderVertexLit)
					_materialEditor.SetShader(Shader.Find(ShaderVertexLit), false);
				break;
			case eLightMode.PixelLit:
				if (material.shader.name != ShaderPixelLit)
					_materialEditor.SetShader(Shader.Find(ShaderPixelLit), false);
				break;
			case eLightMode.Unlit:
				if (material.shader.name != ShaderUnlit)
					_materialEditor.SetShader(Shader.Find(ShaderUnlit), false);
				break;
			}

			MaterialChanged(material);
		}
	}


}
