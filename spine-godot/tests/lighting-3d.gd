extends SceneTree

var failures: Array[String] = []
var fixture_dir := "user://spine-lighting-3d-%d" % OS.get_process_id()
var compressed_fixture_dir := "res://.godot/spine-lighting-3d-%d" % OS.get_process_id()
var scene: Node3D
var sprite: SpineSprite3D
var light: OmniLight3D

func _init():
	call_deferred("_run")

func _check(condition: bool, message: String):
	if not condition:
		failures.append(message)

func _settle():
	for i in 6:
		await process_frame
	RenderingServer.force_draw(false, 0)
	RenderingServer.force_sync()

func _capture() -> Image:
	await _settle()
	return root.get_texture().get_image()

func _difference(a: Image, b: Image, minimum_y := 0, maximum_y := -1) -> int:
	var changed := 0
	var end_y := a.get_height() if maximum_y < 0 else mini(maximum_y, a.get_height())
	for y in range(maxi(0, minimum_y), end_y):
		for x in a.get_width():
			var ca := a.get_pixel(x, y)
			var cb := b.get_pixel(x, y)
			if maxf(absf(ca.r - cb.r), maxf(absf(ca.g - cb.g), absf(ca.b - cb.b))) > 0.035:
				changed += 1
	return changed

func _write_text(name: String, contents: String):
	var file := FileAccess.open(fixture_dir.path_join(name), FileAccess.WRITE)
	_check(file != null, "create " + name)
	if file != null:
		file.store_string(contents)
		file.close()

func _compressed_normal_data() -> SpineSkeletonDataResource:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(compressed_fixture_dir))
	var diffuse := Image.create(64, 64, false, Image.FORMAT_RGBA8)
	diffuse.fill(Color(0.75, 0.5, 0.3, 1))
	_check(diffuse.compress(Image.COMPRESS_S3TC, Image.COMPRESS_SOURCE_GENERIC) == OK, "compress DDS diffuse fixture")
	_check(diffuse.save_dds(compressed_fixture_dir.path_join("compressed.dds")) == OK, "save DDS diffuse fixture")
	var normal := Image.create(64, 64, false, Image.FORMAT_RGBA8)
	normal.fill(Color(0.9, 0.5, 1, 1))
	_check(normal.compress(Image.COMPRESS_S3TC, Image.COMPRESS_SOURCE_NORMAL) == OK, "compress two-channel DDS normal fixture")
	_check(normal.save_dds(compressed_fixture_dir.path_join("n_compressed.dds")) == OK, "save DDS normal fixture")
	var atlas_contents := "compressed.dds\nsize: 64, 64\nfilter: Linear, Linear\ncompressed\nbounds: 0, 0, 64, 64\n"
	var skeleton_contents := JSON.stringify({"skeleton": {"spine": "4.3.75"}, "bones": [{"name": "root"}],
		"slots": [{"name": "slot", "bone": "root", "attachment": "compressed"}], "skins": [{"name": "default", "attachments": {
			"slot": {"compressed": {"type": "region", "path": "compressed", "width": 200, "height": 200}}}}]})
	var atlas_file := FileAccess.open(compressed_fixture_dir.path_join("compressed.atlas"), FileAccess.WRITE)
	atlas_file.store_string(atlas_contents)
	atlas_file.close()
	var skeleton_json := FileAccess.open(compressed_fixture_dir.path_join("compressed.spine-json"), FileAccess.WRITE)
	skeleton_json.store_string(skeleton_contents)
	skeleton_json.close()
	var atlas := SpineAtlasResource.new()
	_check(atlas.load_from_atlas_file(compressed_fixture_dir.path_join("compressed.atlas")) == OK, "load DDS normal-map atlas")
	var skeleton_file := SpineSkeletonFileResource.new()
	_check(skeleton_file.load_from_file(compressed_fixture_dir.path_join("compressed.spine-json")) == OK, "load DDS normal-map skeleton")
	var data := SpineSkeletonDataResource.new()
	data.atlas_res = atlas
	data.skeleton_file_res = skeleton_file
	return data

func _half_alpha_data() -> SpineSkeletonDataResource:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(fixture_dir))
	var image := Image.create(8, 8, false, Image.FORMAT_RGBA8)
	image.fill(Color(1, 1, 1, 0.5))
	_check(image.save_png(fixture_dir.path_join("half.png")) == OK, "save half-alpha shadow texture")
	_write_text("half.atlas", "half.png\nsize: 8, 8\nfilter: Nearest, Nearest\nhalf\nbounds: 0, 0, 8, 8\n")
	_write_text("half.spine-json", JSON.stringify({"skeleton": {"spine": "4.3.75"}, "bones": [{"name": "root"}],
		"slots": [{"name": "slot", "bone": "root", "attachment": "half"}], "skins": [{"name": "default", "attachments": {
			"slot": {"half": {"type": "region", "path": "half", "width": 200, "height": 200}}}}]}))
	var atlas := SpineAtlasResource.new()
	_check(atlas.load_from_atlas_file(fixture_dir.path_join("half.atlas")) == OK, "load half-alpha atlas")
	var skeleton_file := SpineSkeletonFileResource.new()
	_check(skeleton_file.load_from_file(fixture_dir.path_join("half.spine-json")) == OK, "load half-alpha skeleton")
	var data := SpineSkeletonDataResource.new()
	data.atlas_res = atlas
	data.skeleton_file_res = skeleton_file
	return data

func _lit_pixels(image: Image, minimum_y: int) -> int:
	var count := 0
	for y in range(minimum_y, image.get_height()):
		for x in image.get_width():
			var color := image.get_pixel(x, y)
			if maxf(color.r, maxf(color.g, color.b)) > 0.18:
				count += 1
	return count

func _newly_black_fraction(candidate: Image, reference: Image) -> float:
	var expected_pixels := 0
	var newly_black := 0
	var x_from := int(reference.get_width() * 0.32)
	var x_to := int(reference.get_width() * 0.68)
	var y_from := int(reference.get_height() * 0.15)
	var y_to := int(reference.get_height() * 0.58)
	for y in range(y_from, y_to):
		for x in range(x_from, x_to):
			var expected := reference.get_pixel(x, y)
			var expected_brightness := maxf(expected.r, maxf(expected.g, expected.b))
			if expected_brightness <= 0.3:
				continue
			expected_pixels += 1
			var actual := candidate.get_pixel(x, y)
			if maxf(actual.r, maxf(actual.g, actual.b)) < expected_brightness * 0.2:
				newly_black += 1
	return float(newly_black) / expected_pixels if expected_pixels > 0 else 1.0

func _masked_character_brightness(candidate: Image, reference: Image) -> float:
	var candidate_total := 0.0
	var reference_total := 0.0
	var x_from := int(reference.get_width() * 0.32)
	var x_to := int(reference.get_width() * 0.68)
	var y_from := int(reference.get_height() * 0.15)
	var y_to := int(reference.get_height() * 0.58)
	for y in range(y_from, y_to):
		for x in range(x_from, x_to):
			var expected := reference.get_pixel(x, y)
			var expected_brightness := maxf(expected.r, maxf(expected.g, expected.b))
			if expected_brightness <= 0.3:
				continue
			var actual := candidate.get_pixel(x, y)
			candidate_total += maxf(actual.r, maxf(actual.g, actual.b))
			reference_total += expected_brightness
	return candidate_total / reference_total if reference_total > 0 else 0.0

func _run():
	if DisplayServer.get_name() == "headless":
		push_error("Spine 3D lighting tests require a GPU renderer.")
		quit(1)
		return
	var defaults := SpineSprite3D.new()
	_check(not defaults.lighting_enabled, "generated lighting defaults off")
	_check(defaults.normal_map_enabled and defaults.normal_map_flip_y and is_equal_approx(defaults.normal_scale, 1.0),
		"normal maps default enabled with the imported-map Y convention")
	_check(defaults.shadow_casting == SpineSprite3D.SHADOW_CASTING_OFF and is_equal_approx(defaults.shadow_alpha_cutoff, 0.3),
		"shadows default off with a 0.3 alpha cutoff")
	_check(defaults.get_render_statistics().shadow_instance_pool == 0, "shadow instances are allocated lazily")
	defaults.free()

	scene = load("res://examples/19-3d-lighting/3d-lighting.tscn").instantiate()
	root.add_child(scene)
	sprite = scene.get_node("Raptor")
	light = scene.get_node("OmniLight3D")
	await process_frame
	scene.auto_light.button_pressed = false
	sprite.update_mode = SpineConstant.UpdateMode_Manual
	sprite.update_skeleton(0.0)
	_check(sprite.get_skeleton_data_res().atlas_res.get_normal_maps().size() > 0, "Raptor atlas exposes n_raptor.png to native 3D")
	_check(sprite.lighting_enabled and sprite.normal_map_enabled and sprite.shadow_casting == SpineSprite3D.SHADOW_CASTING_DOUBLE_SIDED,
		"example19 enables lighting, normal maps and double-sided shadows")

	var normal_on := await _capture()
	var before: Dictionary = sprite.get_render_statistics()
	sprite.normal_map_enabled = false
	var normal_off := await _capture()
	_check(_difference(normal_on, normal_off) > 250, "atlas normal map changes lit Raptor pixels")
	_check(_masked_character_brightness(normal_on, normal_off) > 0.55,
		"the example's tuned normal strength retains readable lit artwork")
	_check(_newly_black_fraction(normal_on, normal_off) < 0.005,
		"the tuned normal map introduces no localized near-black patches")
	var after: Dictionary = sprite.get_render_statistics()
	_check(after.mesh_builds == before.mesh_builds and after.material_builds == before.material_builds,
		"normal-map toggles update uniforms without rebuilding warmed meshes/materials")
	var ground: MeshInstance3D = scene.get_node("Ground")
	ground.visible = false
	light.light_energy = 0.0
	var flat_omni_dark := await _capture()
	light.light_energy = 5.0
	var flat_omni_lit := await _capture()
	_check(_difference(flat_omni_dark, flat_omni_lit) > 200, "flat planar normals respond to omni lights without a normal map")
	light.visible = false
	var directional := DirectionalLight3D.new()
	directional.rotation_degrees = Vector3(-35, -25, 0)
	directional.light_energy = 0.0
	scene.add_child(directional)
	var flat_directional_dark := await _capture()
	directional.light_energy = 3.0
	var flat_directional_lit := await _capture()
	_check(_difference(flat_directional_dark, flat_directional_lit) > 200, "flat planar normals respond to directional lights")
	directional.rotation_degrees = Vector3.ZERO
	var positive_scale := await _capture()
	sprite.scale.x = -1
	var negative_scale := await _capture()
	var positive_lit_pixels := _lit_pixels(positive_scale, int(positive_scale.get_height() * 0.25))
	var negative_lit_pixels := _lit_pixels(negative_scale, int(negative_scale.get_height() * 0.25))
	_check(positive_lit_pixels > 500 and negative_lit_pixels > positive_lit_pixels * 0.8 and negative_lit_pixels < positive_lit_pixels * 1.2,
		"negative-scale mirroring preserves the lit front-face normal")
	sprite.scale.x = 1
	sprite.normal_map_enabled = true
	var positive_mapped_scale := await _capture()
	sprite.scale.x = -1
	var negative_mapped_scale := await _capture()
	var positive_mapped_pixels := _lit_pixels(positive_mapped_scale, int(positive_mapped_scale.get_height() * 0.25))
	var negative_mapped_pixels := _lit_pixels(negative_mapped_scale, int(negative_mapped_scale.get_height() * 0.25))
	_check(positive_mapped_pixels > 500 and negative_mapped_pixels > positive_mapped_pixels * 0.8 and
		negative_mapped_pixels < positive_mapped_pixels * 1.2, "negative-scale mirroring preserves tangent-space normal lighting")
	sprite.scale.x = 1
	directional.free()
	light.visible = true
	ground.visible = true
	sprite.normal_map_enabled = true
	sprite.normal_scale = 0.0
	var zero_normal := await _capture()
	_check(_difference(normal_off, zero_normal) < 100, "zero normal strength cleanly restores the flat planar normal")
	sprite.normal_scale = 1.0
	var unit_normal := await _capture()
	_check(_masked_character_brightness(unit_normal, normal_off) > 0.55,
		"Godot's compressed normal-map path retains a forward component instead of blacking out the character")
	sprite.normal_scale = 2.0
	var stronger_normal := await _capture()
	_check(_difference(unit_normal, stronger_normal) > 100, "normal strength changes mapped lighting")
	sprite.normal_scale = 1.0
	sprite.normal_map_flip_y = false
	var unflipped_normal := await _capture()
	_check(_difference(unit_normal, unflipped_normal) > 50, "normal-map Y convention is explicit and observable")
	sprite.normal_map_flip_y = true

	# Exercise a deterministic two-channel VRAM-compressed atlas normal map.
	sprite.visible = false
	var compressed_target := SpineSprite3D.new()
	compressed_target.update_mode = SpineConstant.UpdateMode_Manual
	compressed_target.pixel_size = 0.01
	compressed_target.lighting_enabled = true
	compressed_target.skeleton_data_res = _compressed_normal_data()
	scene.add_child(compressed_target)
	compressed_target.update_skeleton(0.0)
	var compressed_maps: Array = compressed_target.skeleton_data_res.atlas_res.get_normal_maps()
	_check(compressed_maps.size() == 1 and compressed_maps[0].get_image().is_compressed(),
		"DDS atlas fixture reaches generated lighting as an actual VRAM-compressed normal texture")
	var compressed_normal_on := await _capture()
	compressed_target.normal_map_enabled = false
	var compressed_normal_off := await _capture()
	_check(_difference(compressed_normal_on, compressed_normal_off) > 30,
		"Godot reconstructs and applies the two-channel compressed atlas normal")
	compressed_target.free()
	sprite.visible = true

	# Exercise dynamic UV-derived tangent packing rather than only initial mesh construction.
	var pose_stats: Dictionary = sprite.get_render_statistics()
	sprite.update_skeleton(0.25)
	var animated_normal := await _capture()
	var animated_stats: Dictionary = sprite.get_render_statistics()
	_check(animated_stats.vertex_uploads > pose_stats.vertex_uploads and animated_stats.tangent_uploads > pose_stats.tangent_uploads,
		"animated normal-mapped poses upload recomputed packed tangents")
	_check(_difference(animated_normal, unit_normal, 0, int(unit_normal.get_height() * 0.65)) > 200,
		"animated tangent updates produce a distinct readable mapped pose")
	sprite.normal_map_enabled = false
	var animated_flat := await _capture()
	_check(_newly_black_fraction(animated_normal, animated_flat) < 0.02,
		"animated normal-map tangents avoid localized near-black corruption")
	sprite.normal_map_enabled = true
	sprite.get_animation_state().get_track(0).set_track_time(0.0)
	sprite.update_skeleton(0.0)

	# Lit/unlit shader states are cached, while the dynamic PBR controls remain uniforms.
	sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_OFF
	sprite.lighting_enabled = false
	await _settle()
	sprite.lighting_enabled = true
	await _settle()
	var warmed: Dictionary = sprite.get_render_statistics()
	sprite.lighting_enabled = false
	await _settle()
	sprite.lighting_enabled = true
	sprite.specular = 0.8
	sprite.roughness = 0.25
	sprite.metallic = 0.15
	await _settle()
	var reused: Dictionary = sprite.get_render_statistics()
	_check(reused.mesh_builds == warmed.mesh_builds and reused.material_builds == warmed.material_builds,
		"repeated lighting state and PBR uniforms reuse warmed resources")

	# The shadow-only instance shares native geometry and is hidden/allocated only on demand.
	sprite.specular = 0.25
	sprite.roughness = 0.65
	sprite.metallic = 0.0
	var no_shadow := await _capture()
	var front_light_position := light.position
	sprite.cull_mode = SpineSprite3D.CULL_BACK
	light.position = Vector3(0, 2.5, -4)
	var backlit_no_shadow := await _capture()
	sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_ON
	var one_sided_shadow := await _capture()
	sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_DOUBLE_SIDED
	var backlit_double_sided_shadow := await _capture()
	_check(_difference(one_sided_shadow, backlit_double_sided_shadow, int(one_sided_shadow.get_height() * 0.45)) > 300 and
		_difference(backlit_no_shadow, backlit_double_sided_shadow, int(one_sided_shadow.get_height() * 0.45)) > 300,
		"one-sided and double-sided generated shadow culling remain distinct")
	sprite.cull_mode = SpineSprite3D.CULL_DISABLED
	light.position = front_light_position
	sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_DOUBLE_SIDED
	var with_shadow := await _capture()
	_check(_difference(no_shadow, with_shadow, int(with_shadow.get_height() * 0.45)) > 300, "double-sided generated shadow mode casts native geometry")
	var shadow_stats: Dictionary = sprite.get_render_statistics()
	_check(shadow_stats.shadow_instances == shadow_stats.batches and shadow_stats.shadow_instance_pool == shadow_stats.batch_pool,
		"one lazy shadow-only instance shares each active batch mesh")
	sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_SHADOWS_ONLY
	var shadows_only := await _capture()
	_check(_difference(with_shadow, shadows_only, 0, int(with_shadow.get_height() * 0.65)) > 300,
		"shadows-only mode hides visible Spine geometry")
	var alpha_uploads: int = sprite.get_render_statistics().attribute_uploads
	sprite.get_skeleton().set_color(Color(1, 1, 1, 0))
	sprite.update_skeleton(0.0)
	var transparent_generated_shadow := await _capture()
	_check(sprite.get_render_statistics().attribute_uploads > alpha_uploads and
		_difference(shadows_only, transparent_generated_shadow, int(shadows_only.get_height() * 0.45)) > 300,
		"alpha-only mesh updates invalidate stationary generated shadows")
	sprite.get_skeleton().set_color(Color.WHITE)
	sprite.update_skeleton(0.0)
	sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_OFF
	await _settle()
	_check(sprite.get_render_statistics().shadow_instances == 0 and sprite.get_render_statistics().shadow_instance_pool > 0,
		"disabling shadows hides but retains the warmed lazy pool")

	# Custom shaders remain authored contracts and cast directly rather than using generated shadow materials.
	var custom_source := "shader_type spatial;\nrender_mode unshaded, blend_mix, cull_disabled, depth_draw_opaque;\nuniform sampler2D spine_texture;\nvarying float tint_alpha;\nvoid vertex() { tint_alpha = CUSTOM0.a; }\nvoid fragment() { vec4 tex = texture(spine_texture, UV); ALBEDO = tex.rgb; ALPHA = tex.a * tint_alpha; ALPHA_SCISSOR_THRESHOLD = 0.3; }"
	var custom_shader := Shader.new()
	custom_shader.code = custom_source
	var custom_material := ShaderMaterial.new()
	custom_material.shader = custom_shader
	sprite.normal_material = custom_material
	sprite.additive_material = custom_material
	var custom_no_shadow := await _capture()
	sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_DOUBLE_SIDED
	var custom_shadow := await _capture()
	_check(_difference(custom_no_shadow, custom_shadow, int(custom_shadow.get_height() * 0.45)) > 300,
		"authored custom material participates directly in configured shadow casting")
	_check(sprite.get_render_statistics().shadow_instances == 0, "custom materials do not allocate generated shadow-only instances")
	_check(custom_shader.code == custom_source, "custom shader source is never rewritten")
	sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_SHADOWS_ONLY
	var opaque_custom_shadow_only := await _capture()
	sprite.get_skeleton().set_color(Color(1, 1, 1, 0))
	sprite.update_skeleton(0.0)
	var transparent_custom_shadow := await _capture()
	_check(_difference(opaque_custom_shadow_only, transparent_custom_shadow, int(custom_shadow.get_height() * 0.45)) > 300,
		"alpha-only mesh updates invalidate stationary authored custom shadows")
	sprite.get_skeleton().set_color(Color.WHITE)
	sprite.update_skeleton(0.0)
	sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_OFF
	sprite.normal_material = null
	sprite.additive_material = null

	# A half-alpha texture verifies that the independent shadow cutoff controls the shadow silhouette, not visible alpha.
	sprite.visible = false
	var empty_ground := await _capture()
	_check(_difference(shadows_only, empty_ground, int(empty_ground.get_height() * 0.45)) > 300,
		"shadows-only mode retains the generated shadow on the receiver")
	var shadow_target := SpineSprite3D.new()
	shadow_target.update_mode = SpineConstant.UpdateMode_Manual
	shadow_target.pixel_size = 0.01
	shadow_target.position = Vector3(0, 0.2, 0)
	shadow_target.shadow_alpha_cutoff = 0.3
	scene.add_child(shadow_target)
	shadow_target.skeleton_data_res = _half_alpha_data()
	shadow_target.update_skeleton(0)
	await _settle()
	shadow_target.visible = false
	shadow_target.shadow_casting = SpineSprite3D.SHADOW_CASTING_DOUBLE_SIDED
	var hidden_target := await _capture()
	var hidden_difference := _difference(empty_ground, hidden_target)
	_check(hidden_difference == 0 and shadow_target.get_render_statistics().shadow_instance_pool > 0,
		"shadow pool growth while hidden never exposes a new server instance")
	shadow_target.visible = true
	shadow_target.update_skeleton(0)
	var half_shadow := await _capture()
	shadow_target.scale.x = -1
	var mirrored_half_shadow := await _capture()
	_check(_difference(empty_ground, mirrored_half_shadow, int(empty_ground.get_height() * 0.45)) > 30,
		"negative-scale generated geometry retains alpha-tested shadow casting")
	shadow_target.scale.x = 1
	var cutoff_material_builds: int = shadow_target.get_render_statistics().material_builds
	shadow_target.shadow_alpha_cutoff = 0.7
	var cut_shadow := await _capture()
	var half_difference := _difference(half_shadow, cut_shadow, int(half_shadow.get_height() * 0.45))
	_check(shadow_target.get_render_statistics().material_builds == cutoff_material_builds,
		"animated shadow cutoff reuses its shadow material")
	_check(half_difference > 30,
		"shadow alpha cutoff removes a half-alpha silhouette without changing visible alpha")
	shadow_target.free()

	scene.free()
	await process_frame
	for name in ["half.png", "half.atlas", "half.spine-json"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir.path_join(name)))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(fixture_dir))
	for name in ["compressed.dds", "n_compressed.dds", "compressed.atlas", "compressed.spine-json"]:
		DirAccess.remove_absolute(ProjectSettings.globalize_path(compressed_fixture_dir.path_join(name)))
	DirAccess.remove_absolute(ProjectSettings.globalize_path(compressed_fixture_dir))
	if failures.is_empty():
		print("Spine 3D lighting and shadows regression passed.")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		print("Spine 3D lighting and shadows regression failed with %d assertion(s)." % failures.size())
		quit(1)
