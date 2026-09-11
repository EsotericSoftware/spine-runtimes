// This include is generated from SpineSprite3D.get_depth_shader_code().
#include "res://examples/18-depth-offset-orbit/spine-depth.gdshaderinc"
// A template-owned opt-out illustrates a shader that does not follow the policy.
uniform sampler2D spine_texture : repeat_disable;
uniform bool spine_premultiplied_alpha = false;
uniform bool apply_camera_depth = true;
varying vec4 spine_light_color;
varying vec3 spine_dark_color;

vec3 spine_srgb_to_linear(vec3 color) {
	return mix(pow((color + vec3(0.055)) / 1.055, vec3(2.4)), color / 12.92, lessThanEqual(color, vec3(0.04045)));
}

void vertex() {
	if (apply_camera_depth)
		VERTEX = spine_apply_camera_depth(VERTEX, MODEL_MATRIX[3].xyz, MODEL_NORMAL_MATRIX[2], INV_VIEW_MATRIX[3].xyz);
	spine_light_color = CUSTOM0;
	spine_dark_color = CUSTOM1.rgb;
	if (!OUTPUT_IS_SRGB) {
		spine_light_color.rgb = spine_srgb_to_linear(spine_light_color.rgb);
		spine_dark_color = spine_srgb_to_linear(spine_dark_color);
	}
}

void fragment() {
	vec4 tex = texture(spine_texture, UV);
	float alpha = tex.a * spine_light_color.a;
	spine_apply_alpha_cutoff(alpha);
	if (spine_premultiplied_alpha) tex.rgb = tex.a > 0.000001 ? tex.rgb / tex.a : vec3(0.0);
	if (!OUTPUT_IS_SRGB) tex.rgb = spine_srgb_to_linear(tex.rgb);
	ALBEDO = tex.rgb * spine_light_color.rgb + (vec3(1.0) - tex.rgb) * spine_dark_color;
	ALPHA = alpha;
}
