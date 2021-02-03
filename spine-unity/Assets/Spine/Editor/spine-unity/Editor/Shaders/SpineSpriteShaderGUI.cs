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
using Spine.Unity;

using SpineInspectorUtility = Spine.Unity.Editor.SpineInspectorUtility;

public class SpineSpriteShaderGUI : SpineShaderWithOutlineGUI {
	static readonly string kShaderVertexLit = "Spine/Sprite/Vertex Lit";
	static readonly string kShaderPixelLit = "Spine/Sprite/Pixel Lit";
	static readonly string kShaderUnlit = "Spine/Sprite/Unlit";

	static readonly string kShaderVertexLitOutline = "Spine/Outline/Sprite/Vertex Lit";
	static readonly string kShaderPixelLitOutline = "Spine/Outline/Sprite/Pixel Lit";
	static readonly string kShaderUnlitOutline = "Spine/Outline/Sprite/Unlit";

	static readonly string kShaderLitLW = "Lightweight Render Pipeline/Spine/Sprite";
	static readonly string kShaderLitURP = "Universal Render Pipeline/Spine/Sprite";
	static readonly string kShaderLitURP2D = "Universal Render Pipeline/2D/Spine/Sprite";
	static readonly int kSolidQueue = 2000;
	static readonly int kAlphaTestQueue = 2450;
	static readonly int kTransparentQueue = 3000;

	private enum eBlendMode {
		PreMultipliedAlpha,
		StandardAlpha,
		Opaque,
		Additive,
		SoftAdditive,
		Multiply,
		Multiplyx2,
	};

	private enum eLightMode {
		VertexLit,
		PixelLit,
		Unlit,
		LitLightweight,
		LitUniversal,
		LitUniversal2D
	};

	private enum eCulling {
		Off = 0,
		Front = 1,
		Back = 2,
	};

	private enum eNormalsMode {
		MeshNormals = -1,
		FixedNormalsViewSpace = 0,
		FixedNormalsModelSpace = 1,
		FixedNormalsWorldSpace = 2
	};

	private enum eDiffuseRampMode {
		NoRampSpecified = -1,
		FullRangeHard = 0,
		FullRangeSoft = 1,
		OldHard = 2,
		OldSoft = 3,

		DefaultRampMode = OldHard
	};

	MaterialProperty _mainTexture = null;
	MaterialProperty _color = null;
	MaterialProperty _maskTexture = null;

	MaterialProperty _pixelSnap = null;

	MaterialProperty _writeToDepth = null;
	MaterialProperty _depthAlphaCutoff = null;
	MaterialProperty _shadowAlphaCutoff = null;
	MaterialProperty _renderQueue = null;
	MaterialProperty _culling = null;
	MaterialProperty _customRenderQueue = null;

	MaterialProperty _overlayColor = null;
	MaterialProperty _hue = null;
	MaterialProperty _saturation = null;
	MaterialProperty _brightness = null;

	MaterialProperty _rimPower = null;
	MaterialProperty _rimColor = null;

	MaterialProperty _bumpMap = null;
	MaterialProperty _bumpScale = null;
	MaterialProperty _diffuseRamp = null;
	MaterialProperty _fixedNormal = null;

	MaterialProperty _blendTexture = null;
	MaterialProperty _blendTextureLerp = null;

	MaterialProperty _emissionMap = null;
	MaterialProperty _emissionColor = null;
	MaterialProperty _emissionPower = null;

	MaterialProperty _metallic = null;
	MaterialProperty _metallicGlossMap = null;
	MaterialProperty _smoothness = null;
	MaterialProperty _smoothnessScale = null;

	static GUIContent _albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
	static GUIContent _maskText = new GUIContent("Light Mask", "Light mask texture (secondary Sprite texture)");
	static GUIContent _altAlbedoText = new GUIContent("Secondary Albedo", "When a secondary albedo texture is set the albedo will be a blended mix of the two textures based on the blend value.");
	static GUIContent _metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A)");
	static GUIContent _smoothnessText = new GUIContent("Smoothness", "Smoothness value");
	static GUIContent _smoothnessScaleText = new GUIContent("Smoothness", "Smoothness scale factor");
	static GUIContent _normalMapText = new GUIContent("Normal Map", "Normal Map");
	static GUIContent _emissionText = new GUIContent("Emission", "Emission (RGB)");
	static GUIContent _emissionPowerText = new GUIContent("Emission Power");
	static GUIContent _emissionToggleText = new GUIContent("Emission", "Enable Emission.");
	static GUIContent _diffuseRampText = new GUIContent("Diffuse Ramp", "A black and white gradient can be used to create a 'Toon Shading' effect.");
	static GUIContent _depthText = new GUIContent("Write to Depth", "Write to Depth Buffer by clipping alpha.");
	static GUIContent _depthAlphaCutoffText = new GUIContent("Depth Alpha Cutoff", "Threshold for depth write alpha cutoff");
	static GUIContent _shadowAlphaCutoffText = new GUIContent("Shadow Alpha Cutoff", "Threshold for shadow alpha cutoff");
	static GUIContent _receiveShadowsText = new GUIContent("Receive Shadows", "When enabled, other GameObjects can cast shadows onto this GameObject. 'Write to Depth' has to be enabled in Lightweight RP.");
	static GUIContent _fixedNormalText = new GUIContent("Fixed Normals", "If this is ticked instead of requiring mesh normals a Fixed Normal will be used instead (it's quicker and can result in better looking lighting effects on 2d objects).");
	static GUIContent _fixedNormalDirectionText = new GUIContent("Fixed Normal Direction", "Should normally be (0,0,1) if in view-space or (0,0,-1) if in model-space.");
	static GUIContent _adjustBackfaceTangentText = new GUIContent("Adjust Back-face Tangents", "Tick only if you are going to rotate the sprite to face away from the camera, the tangents will be flipped when this is the case to make lighting correct.");
	static GUIContent _sphericalHarmonicsText = new GUIContent("Light Probes & Ambient", "Enable to use spherical harmonics to aplpy ambient light and/or light probes. In vertex-lit mode this will be approximated from scenes ambient trilight settings.");
	static GUIContent _lightingModeText = new GUIContent("Lighting Mode", "Lighting Mode");
	static GUIContent[] _lightingModeOptions = {
		new GUIContent("Vertex Lit"),
		new GUIContent("Pixel Lit"),
		new GUIContent("Unlit"),
		new GUIContent("Lit Lightweight"),
		new GUIContent("Lit Universal"),
		new GUIContent("Lit Universal2D")
	};
	static GUIContent _blendModeText = new GUIContent("Blend Mode", "Blend Mode");
	static GUIContent[] _blendModeOptions = {
		new GUIContent("Pre-Multiplied Alpha"),
		new GUIContent("Standard Alpha"),
		new GUIContent("Opaque"),
		new GUIContent("Additive"),
		new GUIContent("Soft Additive"),
		new GUIContent("Multiply"),
		new GUIContent("Multiply x2")
	};
	static GUIContent _rendererQueueText = new GUIContent("Render Queue Offset");
	static GUIContent _cullingModeText = new GUIContent("Culling Mode");
	static GUIContent[] _cullingModeOptions = { new GUIContent("Off"), new GUIContent("Front"), new GUIContent("Back") };
	static GUIContent _pixelSnapText = new GUIContent("Pixel Snap");
	//static GUIContent _customRenderTypetagsText = new GUIContent("Use Custom RenderType tags");
	static GUIContent _fixedNormalSpaceText = new GUIContent("Fixed Normal Space");
	static GUIContent[] _fixedNormalSpaceOptions = { new GUIContent("View-Space"), new GUIContent("Model-Space"), new GUIContent("World-Space") };
	static GUIContent _rimLightingToggleText = new GUIContent("Rim Lighting", "Enable Rim Lighting.");
	static GUIContent _rimColorText = new GUIContent("Rim Color");
	static GUIContent _rimPowerText = new GUIContent("Rim Power");
	static GUIContent _specularToggleText = new GUIContent("Specular", "Enable Specular.");
	static GUIContent _colorAdjustmentToggleText = new GUIContent("Color Adjustment", "Enable material color adjustment.");
	static GUIContent _colorAdjustmentColorText = new GUIContent("Overlay Color");
	static GUIContent _colorAdjustmentHueText = new GUIContent("Hue");
	static GUIContent _colorAdjustmentSaturationText = new GUIContent("Saturation");
	static GUIContent _colorAdjustmentBrightnessText = new GUIContent("Brightness");
	static GUIContent _fogToggleText = new GUIContent("Fog", "Enable Fog rendering on this renderer.");
	static GUIContent _meshRequiresTangentsText = new GUIContent("Note: Material requires a mesh with tangents.");
	static GUIContent _meshRequiresNormalsText = new GUIContent("Note: Material requires a mesh with normals.");
	static GUIContent _meshRequiresNormalsAndTangentsText = new GUIContent("Note: Material requires a mesh with Normals and Tangents.");
	static GUIContent[] _fixedDiffuseRampModeOptions = { new GUIContent("Hard"), new GUIContent("Soft"), new GUIContent("Old Hard"), new GUIContent("Old Soft") };

	const string _primaryMapsText = "Main Maps";
	const string _depthLabelText = "Depth";
	const string _shadowsText = "Shadows";
	const string _customRenderType = "Use Custom RenderType";

	#region ShaderGUI

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties) {
		FindProperties(properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
		_materialEditor = materialEditor;
		ShaderPropertiesGUI();
	}

	public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader) {
		base.AssignNewShaderToMaterial(material, oldShader, newShader);

		//If not originally a sprite shader set default keywords
		if (oldShader.name != kShaderVertexLit && oldShader.name != kShaderPixelLit && oldShader.name != kShaderUnlit &&
			oldShader.name != kShaderVertexLitOutline && oldShader.name != kShaderPixelLitOutline && oldShader.name != kShaderUnlitOutline &&
			oldShader.name != kShaderLitLW &&
			oldShader.name != kShaderLitURP &&
			oldShader.name != kShaderLitURP2D) {
			SetDefaultSpriteKeywords(material, newShader);
		}

		SetMaterialKeywords(material);
	}

	#endregion

	#region Virtual Interface

	protected override void FindProperties (MaterialProperty[] props) {
		base.FindProperties(props);

		_mainTexture = FindProperty("_MainTex", props);
		_maskTexture = FindProperty("_MaskTex", props, false);
		_color = FindProperty("_Color", props);

		_pixelSnap = FindProperty("PixelSnap", props);

		_writeToDepth = FindProperty("_ZWrite", props, false);
		_depthAlphaCutoff = FindProperty("_Cutoff", props);
		_shadowAlphaCutoff = FindProperty("_ShadowAlphaCutoff", props);
		_renderQueue = FindProperty("_RenderQueue", props);
		_culling = FindProperty("_Cull", props);
		_customRenderQueue = FindProperty("_CustomRenderQueue", props);

		_bumpMap = FindProperty("_BumpMap", props, false);
		_bumpScale = FindProperty("_BumpScale", props, false);
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

		_emissionMap = FindProperty("_EmissionMap", props, false);
		_emissionColor = FindProperty("_EmissionColor", props, false);
		_emissionPower = FindProperty("_EmissionPower", props, false);

		_metallic = FindProperty("_Metallic", props, false);
		_metallicGlossMap = FindProperty("_MetallicGlossMap", props, false);
		_smoothness = FindProperty("_Glossiness", props, false);
		_smoothnessScale = FindProperty("_GlossMapScale", props, false);
	}

	static bool BoldToggleField (GUIContent label, bool value) {
		FontStyle origFontStyle = EditorStyles.label.fontStyle;
		EditorStyles.label.fontStyle = FontStyle.Bold;
		value = EditorGUILayout.Toggle(label, value, EditorStyles.toggle);
		EditorStyles.label.fontStyle = origFontStyle;
		return value;
	}

	protected virtual void ShaderPropertiesGUI () {
		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;

		RenderMeshInfoBox();

		// Detect any changes to the material
		bool dataChanged = RenderModes();

		GUILayout.Label(_primaryMapsText, EditorStyles.boldLabel);
		{
			dataChanged |= RenderTextureProperties();
		}

		GUILayout.Label(_depthLabelText, EditorStyles.boldLabel);
		{
			dataChanged |= RenderDepthProperties();
		}

		GUILayout.Label(_shadowsText, EditorStyles.boldLabel);
		{
			dataChanged |= RenderShadowsProperties();
		}

		if (_metallic != null) {
			dataChanged |= RenderSpecularProperties();
		}

		if (_emissionMap != null && _emissionColor != null) {
			dataChanged |= RenderEmissionProperties();
		}

		if (_fixedNormal != null) {
			dataChanged |= RenderNormalsProperties();
		}

		if (_fixedNormal != null) {
			dataChanged |= RenderSphericalHarmonicsProperties();
		}

		{
			dataChanged |= RenderFogProperties();
		}

		{
			dataChanged |= RenderColorProperties();
		}

		if (_rimColor != null) {
			dataChanged |= RenderRimLightingProperties();
		}

		{
			EditorGUILayout.Space();
			RenderStencilProperties();
		}

		{
			EditorGUILayout.Space();
			RenderOutlineProperties();
		}

		if (dataChanged) {
			MaterialChanged(_materialEditor);
		}
	}

	protected virtual bool RenderModes () {
		bool dataChanged = false;

		//Lighting Mode
		{
			EditorGUI.BeginChangeCheck();

			eLightMode lightMode = GetMaterialLightMode((Material)_materialEditor.target);
			EditorGUI.showMixedValue = false;
			foreach (Material material in _materialEditor.targets) {
				if (lightMode != GetMaterialLightMode(material)) {
					EditorGUI.showMixedValue = true;
					break;
				}
			}

			lightMode = (eLightMode)EditorGUILayout.Popup(_lightingModeText, (int)lightMode, _lightingModeOptions);
			if (EditorGUI.EndChangeCheck()) {
				foreach (Material material in _materialEditor.targets) {
					switch (lightMode) {
					case eLightMode.VertexLit:
						if (material.shader.name != kShaderVertexLit)
							_materialEditor.SetShader(Shader.Find(kShaderVertexLit), false);
						break;
					case eLightMode.PixelLit:
						if (material.shader.name != kShaderPixelLit)
							_materialEditor.SetShader(Shader.Find(kShaderPixelLit), false);
						break;
					case eLightMode.Unlit:
						if (material.shader.name != kShaderUnlit)
							_materialEditor.SetShader(Shader.Find(kShaderUnlit), false);
						break;
					case eLightMode.LitLightweight:
						if (material.shader.name != kShaderLitLW)
							_materialEditor.SetShader(Shader.Find(kShaderLitLW), false);
						break;
					case eLightMode.LitUniversal:
						if (material.shader.name != kShaderLitURP)
							_materialEditor.SetShader(Shader.Find(kShaderLitURP), false);
						break;
					case eLightMode.LitUniversal2D:
						if (material.shader.name != kShaderLitURP2D)
							_materialEditor.SetShader(Shader.Find(kShaderLitURP2D), false);
						break;
					}
				}

				dataChanged = true;
			}
		}

		//Blend Mode
		{
			eBlendMode blendMode = GetMaterialBlendMode((Material)_materialEditor.target);
			EditorGUI.showMixedValue = false;
			foreach (Material material in _materialEditor.targets) {
				if (blendMode != GetMaterialBlendMode(material)) {
					EditorGUI.showMixedValue = true;
					break;
				}
			}

			EditorGUI.BeginChangeCheck();
			blendMode = (eBlendMode)EditorGUILayout.Popup(_blendModeText, (int)blendMode, _blendModeOptions);
			if (EditorGUI.EndChangeCheck()) {
				foreach (Material mat in _materialEditor.targets) {
					SetBlendMode(mat, blendMode);
				}

				dataChanged = true;
			}

			if (QualitySettings.activeColorSpace == ColorSpace.Linear &&
				!EditorGUI.showMixedValue && blendMode == eBlendMode.PreMultipliedAlpha) {
				EditorGUILayout.HelpBox(MaterialChecks.kPMANotSupportedLinearMessage, MessageType.Error, true);
			}
		}

		EditorGUI.BeginDisabledGroup(true);
		_materialEditor.RenderQueueField();
		EditorGUI.EndDisabledGroup();

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = _renderQueue.hasMixedValue;
		int renderQueue = EditorGUILayout.IntField(_rendererQueueText, (int)_renderQueue.floatValue);
		if (EditorGUI.EndChangeCheck()) {
			SetInt("_RenderQueue", renderQueue);
			dataChanged = true;
		}

		EditorGUI.BeginChangeCheck();
		var culling = (eCulling)Mathf.RoundToInt(_culling.floatValue);
		EditorGUI.showMixedValue = _culling.hasMixedValue;
		culling = (eCulling)EditorGUILayout.Popup(_cullingModeText, (int)culling, _cullingModeOptions);
		if (EditorGUI.EndChangeCheck()) {
			SetInt("_Cull", (int)culling);
			dataChanged = true;
		}

		EditorGUI.showMixedValue = false;

		EditorGUI.BeginChangeCheck();
		_materialEditor.ShaderProperty(_pixelSnap, _pixelSnapText);
		dataChanged |= EditorGUI.EndChangeCheck();

		return dataChanged;
	}

	protected virtual bool RenderTextureProperties () {
		bool dataChanged = false;

		EditorGUI.BeginChangeCheck();

		_materialEditor.TexturePropertySingleLine(_albedoText, _mainTexture, _color);

		if (_bumpMap != null)
			_materialEditor.TexturePropertySingleLine(_normalMapText, _bumpMap, _bumpMap.textureValue != null ? _bumpScale : null);

		if (_maskTexture != null)
			_materialEditor.TexturePropertySingleLine(_maskText, _maskTexture);

		dataChanged |= RenderDiffuseRampProperties();

		dataChanged |= EditorGUI.EndChangeCheck();

		if (_blendTexture != null) {
			EditorGUI.BeginChangeCheck();
			_materialEditor.TexturePropertySingleLine(_altAlbedoText, _blendTexture, _blendTextureLerp);
			if (EditorGUI.EndChangeCheck()) {
				SetKeyword(_materialEditor, "_TEXTURE_BLEND", _blendTexture != null);
				dataChanged = true;
			}
		}

		EditorGUI.BeginChangeCheck();
		_materialEditor.TextureScaleOffsetProperty(_mainTexture);
		dataChanged |= EditorGUI.EndChangeCheck();

		EditorGUI.showMixedValue = false;

		return dataChanged;
	}

	protected virtual bool RenderDepthProperties () {
		bool dataChanged = false;

		EditorGUI.BeginChangeCheck();

		bool showDepthAlphaCutoff = true;
		// e.g. Pixel Lit shader always has ZWrite enabled
		if (_writeToDepth != null) {
			bool mixedValue = _writeToDepth.hasMixedValue;
			EditorGUI.showMixedValue = mixedValue;
			bool writeTodepth = EditorGUILayout.Toggle(_depthText, _writeToDepth.floatValue != 0.0f);

			if (EditorGUI.EndChangeCheck()) {
				SetInt("_ZWrite", writeTodepth ? 1 : 0);
				_depthAlphaCutoff.floatValue = writeTodepth ? 0.5f : 0.0f;
				mixedValue = false;
				dataChanged = true;
			}

			showDepthAlphaCutoff = writeTodepth && !mixedValue && GetMaterialBlendMode((Material)_materialEditor.target) != eBlendMode.Opaque;
		}
		if (showDepthAlphaCutoff) {
			EditorGUI.BeginChangeCheck();
			_materialEditor.RangeProperty(_depthAlphaCutoff, _depthAlphaCutoffText.text);
			dataChanged |= EditorGUI.EndChangeCheck();
		}

		{
			bool useCustomRenderType = _customRenderQueue.floatValue > 0.0f;
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = _customRenderQueue.hasMixedValue;
			useCustomRenderType = EditorGUILayout.Toggle(_customRenderType, useCustomRenderType);
			if (EditorGUI.EndChangeCheck()) {
				dataChanged = true;

				_customRenderQueue.floatValue = useCustomRenderType ? 1.0f : 0.0f;

				foreach (Material material in _materialEditor.targets) {
					eBlendMode blendMode = GetMaterialBlendMode(material);

					switch (blendMode) {
					case eBlendMode.Opaque:
						{
							SetRenderType(material, "Opaque", useCustomRenderType);
						}
						break;
					default:
						{
							bool zWrite = HasZWriteEnabled(material);
							SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderType);
						}
						break;
					}
				}
			}
		}

		EditorGUI.showMixedValue = false;

		return dataChanged;
	}

	protected virtual bool RenderNormalsProperties () {
		bool dataChanged = false;

		eNormalsMode normalsMode = GetMaterialNormalsMode((Material)_materialEditor.target);
		bool mixedNormalsMode = false;
		foreach (Material material in _materialEditor.targets) {
			if (normalsMode != GetMaterialNormalsMode(material)) {
				mixedNormalsMode = true;
				break;
			}
		}

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedNormalsMode;
		bool fixedNormals = BoldToggleField(_fixedNormalText, normalsMode != eNormalsMode.MeshNormals);

		if (EditorGUI.EndChangeCheck()) {
			normalsMode = fixedNormals ? eNormalsMode.FixedNormalsViewSpace : eNormalsMode.MeshNormals;
			SetNormalsMode(_materialEditor, normalsMode, false);
			_fixedNormal.vectorValue = new Vector4(0.0f, 0.0f, normalsMode == eNormalsMode.FixedNormalsViewSpace ? 1.0f : -1.0f, 1.0f);
			mixedNormalsMode = false;
			dataChanged = true;
		}

		if (fixedNormals) {
			//Show drop down for normals space
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = mixedNormalsMode;
			normalsMode = (eNormalsMode)EditorGUILayout.Popup(_fixedNormalSpaceText, (int)normalsMode, _fixedNormalSpaceOptions);
			if (EditorGUI.EndChangeCheck()) {
				SetNormalsMode((Material)_materialEditor.target, normalsMode, GetMaterialFixedNormalsBackfaceRenderingOn((Material)_materialEditor.target));

				foreach (Material material in _materialEditor.targets) {
					SetNormalsMode(material, normalsMode, GetMaterialFixedNormalsBackfaceRenderingOn(material));
				}

				//Reset fixed normal to default (Vector3.forward for model-space, -Vector3.forward for view-space).
				_fixedNormal.vectorValue = new Vector4(0.0f, 0.0f, normalsMode == eNormalsMode.FixedNormalsViewSpace ? 1.0f : -1.0f, 1.0f);

				mixedNormalsMode = false;
				dataChanged = true;
			}

			//Show fixed normal
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = _fixedNormal.hasMixedValue;
			Vector3 normal = EditorGUILayout.Vector3Field(_fixedNormalDirectionText, _fixedNormal.vectorValue);
			if (EditorGUI.EndChangeCheck()) {
				_fixedNormal.vectorValue = new Vector4(normal.x, normal.y, normal.z, 1.0f);
				dataChanged = true;
			}

			//Show adjust for back face rendering
			{
				bool fixBackFaceRendering = GetMaterialFixedNormalsBackfaceRenderingOn((Material)_materialEditor.target);
				bool mixedBackFaceRendering = false;
				foreach (Material material in _materialEditor.targets) {
					if (fixBackFaceRendering != GetMaterialFixedNormalsBackfaceRenderingOn(material)) {
						mixedBackFaceRendering = true;
						break;
					}
				}

				EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue = mixedBackFaceRendering;
				bool backRendering = EditorGUILayout.Toggle(_adjustBackfaceTangentText, fixBackFaceRendering);

				if (EditorGUI.EndChangeCheck()) {
					SetNormalsMode(_materialEditor, normalsMode, backRendering);
					dataChanged = true;
				}
			}

		}

		EditorGUI.showMixedValue = false;

		return dataChanged;
	}

	protected virtual bool RenderDiffuseRampProperties () {
		bool dataChanged = false;

		eDiffuseRampMode rampMode = GetMaterialDiffuseRampMode((Material)_materialEditor.target);
		bool mixedRampMode = false;
		foreach (Material material in _materialEditor.targets) {
			if (rampMode != GetMaterialDiffuseRampMode(material)) {
				mixedRampMode = true;
				break;
			}
		}

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedRampMode;
		EditorGUILayout.BeginHorizontal();

		if (_diffuseRamp != null)
			_materialEditor.TexturePropertySingleLine(_diffuseRampText, _diffuseRamp);

		if (EditorGUI.EndChangeCheck()) {
			if (rampMode == eDiffuseRampMode.NoRampSpecified)
				rampMode = eDiffuseRampMode.DefaultRampMode;

			SetDiffuseRampMode(_materialEditor, rampMode);
			mixedRampMode = false;
			dataChanged = true;
		}

		if (_diffuseRamp != null && _diffuseRamp.textureValue != null) {
			//Show drop down for ramp mode
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = mixedRampMode;
			rampMode = (eDiffuseRampMode)EditorGUILayout.Popup((int)rampMode, _fixedDiffuseRampModeOptions);
			if (EditorGUI.EndChangeCheck()) {
				SetDiffuseRampMode(_materialEditor, rampMode);
				mixedRampMode = false;
				dataChanged = true;
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUI.showMixedValue = false;

		return dataChanged;
	}

	protected virtual bool RenderShadowsProperties () {
		bool dataChanged = false;

		EditorGUI.BeginChangeCheck();
		_materialEditor.RangeProperty(_shadowAlphaCutoff, _shadowAlphaCutoffText.text);
		dataChanged = EditorGUI.EndChangeCheck();
		bool areMixedShaders = false;
		bool hasReceiveShadowsParameter = IsLWRPShader(_materialEditor, out areMixedShaders) ||
			IsURP3DShader(_materialEditor, out areMixedShaders);

		if (hasReceiveShadowsParameter) {
			EditorGUI.BeginChangeCheck();
			bool mixedValue;
			bool enableReceive = !IsKeywordEnabled(_materialEditor, "_RECEIVE_SHADOWS_OFF", out mixedValue);
			EditorGUI.showMixedValue = mixedValue;
			enableReceive = EditorGUILayout.Toggle(_receiveShadowsText, enableReceive);

			EditorGUI.showMixedValue = false;

			if (EditorGUI.EndChangeCheck()) {
				SetKeyword(_materialEditor, "_RECEIVE_SHADOWS_OFF", !enableReceive);
				dataChanged = true;
			}
		}

		return dataChanged;
	}

	protected virtual bool RenderSphericalHarmonicsProperties () {

		bool areMixedShaders = false;
		bool isLWRPShader = IsLWRPShader(_materialEditor, out areMixedShaders);
		bool isURP3DShader = IsURP3DShader(_materialEditor, out areMixedShaders);
		bool isURP2DShader = IsURP2DShader(_materialEditor, out areMixedShaders);
		bool hasSHParameter = !(isLWRPShader || isURP3DShader || isURP2DShader);
		if (!hasSHParameter)
			return false;

		EditorGUI.BeginChangeCheck();
		bool mixedValue;
		bool enabled = IsKeywordEnabled(_materialEditor, "_SPHERICAL_HARMONICS", out mixedValue);
		EditorGUI.showMixedValue = mixedValue;
		enabled = BoldToggleField(_sphericalHarmonicsText, enabled);
		EditorGUI.showMixedValue = false;

		if (EditorGUI.EndChangeCheck()) {
			SetKeyword(_materialEditor, "_SPHERICAL_HARMONICS", enabled);
			return true;
		}

		return false;
	}

	protected virtual bool RenderFogProperties () {

		bool areMixedShaders = false;
		bool isURP2DShader = IsURP2DShader(_materialEditor, out areMixedShaders);

		if (isURP2DShader && !areMixedShaders)
			return false;

		EditorGUI.BeginChangeCheck();
		bool mixedValue;
		bool fog = IsKeywordEnabled(_materialEditor, "_FOG", out mixedValue);
		EditorGUI.showMixedValue = mixedValue;
		fog = BoldToggleField(_fogToggleText, fog);
		EditorGUI.showMixedValue = false;

		if (EditorGUI.EndChangeCheck()) {
			SetKeyword(_materialEditor, "_FOG", fog);
			return true;
		}

		return false;
	}

	protected virtual bool RenderColorProperties () {
		bool dataChanged = false;

		EditorGUI.BeginChangeCheck();
		bool mixedValue;
		bool colorAdjust = IsKeywordEnabled(_materialEditor, "_COLOR_ADJUST", out mixedValue);
		EditorGUI.showMixedValue = mixedValue;
		colorAdjust = BoldToggleField(_colorAdjustmentToggleText, colorAdjust);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck()) {
			SetKeyword(_materialEditor, "_COLOR_ADJUST", colorAdjust);
			mixedValue = false;
			dataChanged = true;
		}

		if (colorAdjust && !mixedValue) {
			EditorGUI.BeginChangeCheck();
			_materialEditor.ColorProperty(_overlayColor, _colorAdjustmentColorText.text);
			_materialEditor.RangeProperty(_hue, _colorAdjustmentHueText.text);
			_materialEditor.RangeProperty(_saturation, _colorAdjustmentSaturationText.text);
			_materialEditor.RangeProperty(_brightness, _colorAdjustmentBrightnessText.text);
			dataChanged |= EditorGUI.EndChangeCheck();
		}

		return dataChanged;
	}

	protected virtual bool RenderSpecularProperties () {
		bool dataChanged = false;

		bool mixedSpecularValue;
		bool specular = IsKeywordEnabled(_materialEditor, "_SPECULAR", out mixedSpecularValue);
		bool mixedSpecularGlossMapValue;
		bool specularGlossMap = IsKeywordEnabled(_materialEditor, "_SPECULAR_GLOSSMAP", out mixedSpecularGlossMapValue);
		bool mixedValue = mixedSpecularValue || mixedSpecularGlossMapValue;

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedValue;
		bool specularEnabled = BoldToggleField(_specularToggleText, specular || specularGlossMap);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck()) {
			foreach (Material material in _materialEditor.targets) {
				bool hasGlossMap = material.GetTexture("_MetallicGlossMap") != null;
				SetKeyword(material, "_SPECULAR", specularEnabled && !hasGlossMap);
				SetKeyword(material, "_SPECULAR_GLOSSMAP", specularEnabled && hasGlossMap);
			}

			mixedValue = false;
			dataChanged = true;
		}

		if (specularEnabled && !mixedValue) {
			EditorGUI.BeginChangeCheck();
			bool hasGlossMap = _metallicGlossMap.textureValue != null;
			_materialEditor.TexturePropertySingleLine(_metallicMapText, _metallicGlossMap, hasGlossMap ? null : _metallic);
			if (EditorGUI.EndChangeCheck()) {
				hasGlossMap = _metallicGlossMap.textureValue != null;
				SetKeyword(_materialEditor, "_SPECULAR", !hasGlossMap);
				SetKeyword(_materialEditor, "_SPECULAR_GLOSSMAP", hasGlossMap);

				dataChanged = true;
			}

			const int indentation = 2;
			_materialEditor.ShaderProperty(hasGlossMap ? _smoothnessScale : _smoothness, hasGlossMap ? _smoothnessScaleText : _smoothnessText, indentation);
		}

		return dataChanged;
	}

	protected virtual bool RenderEmissionProperties () {
		bool dataChanged = false;

		bool mixedValue;
		bool emission = IsKeywordEnabled(_materialEditor, "_EMISSION", out mixedValue);

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedValue;
		emission = BoldToggleField(_emissionToggleText, emission);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck()) {
			SetKeyword(_materialEditor, "_EMISSION", emission);
			mixedValue = false;
			dataChanged = true;
		}

		if (emission && !mixedValue) {
			EditorGUI.BeginChangeCheck();

#if UNITY_2018_1_OR_NEWER
			_materialEditor.TexturePropertyWithHDRColor(_emissionText, _emissionMap, _emissionColor, true);
#else
			_materialEditor.TexturePropertyWithHDRColor(_emissionText, _emissionMap, _emissionColor, new ColorPickerHDRConfig(0, 1, 0.01010101f, 3), true);
#endif
			_materialEditor.FloatProperty(_emissionPower, _emissionPowerText.text);
			dataChanged |= EditorGUI.EndChangeCheck();
		}

		return dataChanged;
	}

	protected virtual bool RenderRimLightingProperties () {
		bool dataChanged = false;

		bool mixedValue;
		bool rimLighting = IsKeywordEnabled(_materialEditor, "_RIM_LIGHTING", out mixedValue);

		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = mixedValue;
		rimLighting = BoldToggleField(_rimLightingToggleText, rimLighting);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck()) {
			SetKeyword(_materialEditor, "_RIM_LIGHTING", rimLighting);
			mixedValue = false;
			dataChanged = true;
		}

		if (rimLighting && !mixedValue) {
			EditorGUI.BeginChangeCheck();
			_materialEditor.ColorProperty(_rimColor, _rimColorText.text);
			_materialEditor.FloatProperty(_rimPower, _rimPowerText.text);
			dataChanged |= EditorGUI.EndChangeCheck();
		}

		return dataChanged;
	}

	#endregion

	#region Private Functions

	void RenderMeshInfoBox () {
		var material = (Material)_materialEditor.target;
		bool requiresNormals = _fixedNormal != null && GetMaterialNormalsMode(material) == eNormalsMode.MeshNormals;
		bool requiresTangents = material.HasProperty("_BumpMap") && material.GetTexture("_BumpMap") != null;

		if (requiresNormals || requiresTangents) {
			GUILayout.Label(requiresNormals && requiresTangents ? _meshRequiresNormalsAndTangentsText : requiresNormals ? _meshRequiresNormalsText : _meshRequiresTangentsText, GUI.skin.GetStyle("helpBox"));
		}
	}

	void SetInt (string propertyName, int value) {
		foreach (Material material in _materialEditor.targets) {
			material.SetInt(propertyName, value);
		}
	}

	void SetDefaultSpriteKeywords (Material material, Shader shader) {
		//Disable emission by default (is set on by default in standard shader)
		SetKeyword(material, "_EMISSION", false);
		//Start with preMultiply alpha by default
		SetBlendMode(material, eBlendMode.PreMultipliedAlpha);
		SetDiffuseRampMode(material, eDiffuseRampMode.DefaultRampMode);
		//Start with mesh normals by default
		SetNormalsMode(material, eNormalsMode.MeshNormals, false);
		if (_fixedNormal != null)
			_fixedNormal.vectorValue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
		//Start with spherical harmonics disabled?
		SetKeyword(material, "_SPHERICAL_HARMONICS", false);
		//Start with specular disabled
		SetKeyword(material, "_SPECULAR", false);
		SetKeyword(material, "_SPECULAR_GLOSSMAP", false);
		//Start with Culling disabled
		material.SetInt("_Cull", (int)eCulling.Off);
		//Start with Z writing disabled
		if (material.HasProperty("_ZWrite"))
			material.SetInt("_ZWrite", 0);
	}

	//Z write is on then

	static void SetRenderType (Material material, string renderType, bool useCustomRenderQueue) {
		//Want a check box to say if should use Sprite render queue (for custom writing depth and normals)
		bool zWrite = HasZWriteEnabled(material);

		if (useCustomRenderQueue) {
			//If sprite has fixed normals then assign custom render type so we can write its correct normal with soft edges
			eNormalsMode normalsMode = GetMaterialNormalsMode(material);

			switch (normalsMode) {
			case eNormalsMode.FixedNormalsViewSpace:
				renderType = "SpriteViewSpaceFixedNormal";
				break;
			case eNormalsMode.FixedNormalsModelSpace:
				renderType = "SpriteModelSpaceFixedNormal";
				break;
			case eNormalsMode.MeshNormals:
				{
					//If sprite doesn't write to depth assign custom render type so we can write its depth with soft edges
					if (!zWrite) {
						renderType = "Sprite";
					}
				}
				break;
			}
		}

		//If we don't write to depth set tag so custom shaders can write to depth themselves
		material.SetOverrideTag("AlphaDepth", zWrite ? "False" : "True");

		material.SetOverrideTag("RenderType", renderType);
	}

	static void SetMaterialKeywords (Material material) {
		eBlendMode blendMode = GetMaterialBlendMode(material);
		SetBlendMode(material, blendMode);

		bool zWrite = HasZWriteEnabled(material);
		bool clipAlpha = zWrite && blendMode != eBlendMode.Opaque && material.GetFloat("_Cutoff") > 0.0f;
		SetKeyword(material, "_ALPHA_CLIP", clipAlpha);

		bool normalMap = material.HasProperty("_BumpMap") && material.GetTexture("_BumpMap") != null;
		SetKeyword(material, "_NORMALMAP", normalMap);

		bool diffuseRamp = material.HasProperty("_DiffuseRamp") && material.GetTexture("_DiffuseRamp") != null;
		SetKeyword(material, "_DIFFUSE_RAMP", diffuseRamp);

		bool blendTexture = material.HasProperty("_BlendTex") && material.GetTexture("_BlendTex") != null;
		SetKeyword(material, "_TEXTURE_BLEND", blendTexture);
	}

	static void MaterialChanged (MaterialEditor materialEditor) {
		foreach (Material material in materialEditor.targets)
			SetMaterialKeywords(material);
	}

	static void SetKeyword (MaterialEditor m, string keyword, bool state) {
		foreach (Material material in m.targets) {
			SetKeyword(material, keyword, state);
		}
	}

	static void SetKeyword (Material m, string keyword, bool state) {
		if (state)
			m.EnableKeyword(keyword);
		else
			m.DisableKeyword(keyword);
	}

	static bool IsLWRPShader (MaterialEditor editor, out bool mixedValue) {
		return IsShaderType(kShaderLitLW, editor, out mixedValue);
	}

	static bool IsURP3DShader (MaterialEditor editor, out bool mixedValue) {
		return IsShaderType(kShaderLitURP, editor, out mixedValue);
	}

	static bool IsURP2DShader (MaterialEditor editor, out bool mixedValue) {
		return IsShaderType(kShaderLitURP2D, editor, out mixedValue);
	}

	static bool IsShaderType (string shaderType, MaterialEditor editor, out bool mixedValue) {

		mixedValue = false;
		bool isAnyTargetTypeShader = false;
		foreach (Material material in editor.targets) {
			if (material.shader.name == shaderType) {
				isAnyTargetTypeShader = true;
			}
			else if (isAnyTargetTypeShader) {
				mixedValue = true;
			}
		}
		return isAnyTargetTypeShader;
	}

	static bool IsKeywordEnabled (MaterialEditor editor, string keyword, out bool mixedValue) {
		bool keywordEnabled = ((Material)editor.target).IsKeywordEnabled(keyword);
		mixedValue = false;

		foreach (Material material in editor.targets) {
			if (material.IsKeywordEnabled(keyword) != keywordEnabled) {
				mixedValue = true;
				break;
			}
		}

		return keywordEnabled;
	}

	static eLightMode GetMaterialLightMode (Material material) {
		if (material.shader.name == kShaderPixelLit ||
			material.shader.name == kShaderPixelLitOutline) {
			return eLightMode.PixelLit;
		}
		else if (material.shader.name == kShaderUnlit ||
				material.shader.name == kShaderUnlitOutline) {
			return eLightMode.Unlit;
		}
		else if (material.shader.name == kShaderLitLW) {
			return eLightMode.LitLightweight;
		}
		else if (material.shader.name == kShaderLitURP) {
			return eLightMode.LitUniversal;
		}
		else if (material.shader.name == kShaderLitURP2D) {
			return eLightMode.LitUniversal2D;
		}
		else { // if (material.shader.name == kShaderVertexLit || kShaderVertexLitOutline)
			return eLightMode.VertexLit;
		}
	}

	static eBlendMode GetMaterialBlendMode (Material material) {
		if (material.IsKeywordEnabled("_ALPHABLEND_ON"))
			return eBlendMode.StandardAlpha;
		if (material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON"))
			return eBlendMode.PreMultipliedAlpha;
		if (material.IsKeywordEnabled("_MULTIPLYBLEND"))
			return eBlendMode.Multiply;
		if (material.IsKeywordEnabled("_MULTIPLYBLEND_X2"))
			return eBlendMode.Multiplyx2;
		if (material.IsKeywordEnabled("_ADDITIVEBLEND"))
			return eBlendMode.Additive;
		if (material.IsKeywordEnabled("_ADDITIVEBLEND_SOFT"))
			return eBlendMode.SoftAdditive;

		return eBlendMode.Opaque;
	}

	static void SetBlendMode (Material material, eBlendMode blendMode) {
		SetKeyword(material, "_ALPHABLEND_ON", blendMode == eBlendMode.StandardAlpha);
		SetKeyword(material, "_ALPHAPREMULTIPLY_ON", blendMode == eBlendMode.PreMultipliedAlpha);
		SetKeyword(material, "_MULTIPLYBLEND", blendMode == eBlendMode.Multiply);
		SetKeyword(material, "_MULTIPLYBLEND_X2", blendMode == eBlendMode.Multiplyx2);
		SetKeyword(material, "_ADDITIVEBLEND", blendMode == eBlendMode.Additive);
		SetKeyword(material, "_ADDITIVEBLEND_SOFT", blendMode == eBlendMode.SoftAdditive);

		int renderQueue;
		bool useCustomRenderQueue = material.GetFloat("_CustomRenderQueue") > 0.0f;

		switch (blendMode) {
		case eBlendMode.Opaque:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				SetRenderType(material, "Opaque", useCustomRenderQueue);
				renderQueue = kSolidQueue;
			}
			break;
		case eBlendMode.Additive:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		case eBlendMode.SoftAdditive:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		case eBlendMode.Multiply:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		case eBlendMode.Multiplyx2:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		default:
			{
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				bool zWrite = HasZWriteEnabled(material);
				SetRenderType(material, zWrite ? "TransparentCutout" : "Transparent", useCustomRenderQueue);
				renderQueue = zWrite ? kAlphaTestQueue : kTransparentQueue;
			}
			break;
		}

		material.renderQueue = renderQueue + material.GetInt("_RenderQueue");
		material.SetOverrideTag("IgnoreProjector", blendMode == eBlendMode.Opaque ? "False" : "True");
	}

	static eNormalsMode GetMaterialNormalsMode (Material material) {
		if (material.IsKeywordEnabled("_FIXED_NORMALS_VIEWSPACE") || material.IsKeywordEnabled("_FIXED_NORMALS_VIEWSPACE_BACKFACE"))
			return eNormalsMode.FixedNormalsViewSpace;
		if (material.IsKeywordEnabled("_FIXED_NORMALS_WORLDSPACE"))
			return eNormalsMode.FixedNormalsWorldSpace;
		if (material.IsKeywordEnabled("_FIXED_NORMALS_MODELSPACE") || material.IsKeywordEnabled("_FIXED_NORMALS_MODELSPACE_BACKFACE"))
			return eNormalsMode.FixedNormalsModelSpace;

		return eNormalsMode.MeshNormals;
	}


	static void SetNormalsMode (MaterialEditor materialEditor, eNormalsMode normalsMode, bool allowBackFaceRendering) {
		foreach (Material material in materialEditor.targets) {
			SetNormalsMode(material, normalsMode, allowBackFaceRendering);
		}
	}

	static void SetNormalsMode (Material material, eNormalsMode normalsMode, bool allowBackFaceRendering) {
		SetKeyword(material, "_FIXED_NORMALS_VIEWSPACE", normalsMode == eNormalsMode.FixedNormalsViewSpace && !allowBackFaceRendering);
		SetKeyword(material, "_FIXED_NORMALS_VIEWSPACE_BACKFACE", normalsMode == eNormalsMode.FixedNormalsViewSpace && allowBackFaceRendering);
		SetKeyword(material, "_FIXED_NORMALS_WORLDSPACE", normalsMode == eNormalsMode.FixedNormalsWorldSpace);
		SetKeyword(material, "_FIXED_NORMALS_MODELSPACE", normalsMode == eNormalsMode.FixedNormalsModelSpace && !allowBackFaceRendering);
		SetKeyword(material, "_FIXED_NORMALS_MODELSPACE_BACKFACE", normalsMode == eNormalsMode.FixedNormalsModelSpace && allowBackFaceRendering);
	}

	static bool GetMaterialFixedNormalsBackfaceRenderingOn (Material material) {
		return material.IsKeywordEnabled("_FIXED_NORMALS_VIEWSPACE_BACKFACE") || material.IsKeywordEnabled("_FIXED_NORMALS_MODELSPACE_BACKFACE");
	}

	static eDiffuseRampMode GetMaterialDiffuseRampMode (Material material) {
		if (material.IsKeywordEnabled("_FULLRANGE_HARD_RAMP"))
			return eDiffuseRampMode.FullRangeHard;
		if (material.IsKeywordEnabled("_FULLRANGE_SOFT_RAMP"))
			return eDiffuseRampMode.FullRangeSoft;
		if (material.IsKeywordEnabled("_OLD_HARD_RAMP"))
			return eDiffuseRampMode.OldHard;
		if (material.IsKeywordEnabled("_OLD_SOFT_RAMP"))
			return eDiffuseRampMode.OldSoft;

		return eDiffuseRampMode.NoRampSpecified;
	}

	static void SetDiffuseRampMode (MaterialEditor materialEditor, eDiffuseRampMode rampMode) {
		foreach (Material material in materialEditor.targets) {
			SetDiffuseRampMode(material, rampMode);
		}
	}

	static void SetDiffuseRampMode (Material material, eDiffuseRampMode rampMode) {
		SetKeyword(material, "_FULLRANGE_HARD_RAMP", rampMode == eDiffuseRampMode.FullRangeHard);
		SetKeyword(material, "_FULLRANGE_SOFT_RAMP", rampMode == eDiffuseRampMode.FullRangeSoft);
		SetKeyword(material, "_OLD_HARD_RAMP", rampMode == eDiffuseRampMode.OldHard);
		SetKeyword(material, "_OLD_SOFT_RAMP", rampMode == eDiffuseRampMode.OldSoft);
	}

	static bool HasZWriteEnabled (Material material) {
		if (material.HasProperty("_ZWrite")) {
			return material.GetFloat("_ZWrite") > 0.0f;
		}
		else return true; // Pixel Lit shader always has _ZWrite enabled.
	}
	#endregion
}
