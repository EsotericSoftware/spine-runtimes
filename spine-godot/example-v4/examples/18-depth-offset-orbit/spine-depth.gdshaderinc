
// These uniforms are supplied by SpineSprite3D, including custom shader passes.
uniform bool spine_camera_relative_depth = true;
uniform float spine_alpha_cutoff = 0.001;
vec3 spine_apply_camera_depth(vec3 position, vec3 origin, vec3 plane_normal, vec3 camera_position) {
	if (spine_camera_relative_depth && dot(camera_position - origin, plane_normal) < 0.0) position.z = -position.z;
	return position;
}
void spine_apply_alpha_cutoff(float alpha) {
	if (alpha <= 0.0 || alpha < spine_alpha_cutoff) discard;
}
