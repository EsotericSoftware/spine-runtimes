# SpineSprite3D depth and custom shaders

## Default behavior

`SpineSprite3D` defaults to:

```gdscript
sprite.camera_relative_depth = true
sprite.depth_write_enabled = true
sprite.slot_depth_offset = 0.001
sprite.alpha_cutoff = 0.001
sprite.lighting_enabled = false
sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_OFF
```

The generated materials are unlit and double-sided by default. Each attachment gets a small local Z offset according to its position in the **applied Spine draw order**. From behind, the vertex shader flips that offset, putting later attachments nearer the camera again. Attachment, triangle and batch submission order do not reverse. This preserves the authored layering from both sides, rather than revealing the back of a physical stack. It does not turn planar artwork into a volumetric character.

The shader discards zero-alpha fragments and fragments below `alpha_cutoff`, using **texture alpha multiplied by the combined skeleton/slot/attachment tint alpha**. Surviving fractional alpha is blended and writes real scene depth. Opaque and transparent world geometry still depth-tests against the character. A higher `render_priority` changes submission order, not depth-test results; it cannot force hidden geometry through a nearer depth-writing surface.

Turn off `camera_relative_depth` for a fixed physical stack, like the depth-offset approach used in spine-ue and spine-threejs. Keep `depth_write_enabled` on to retain scene-depth occlusion. To explicitly restore the previous no-write, coplanar transparent behavior:

```gdscript
sprite.camera_relative_depth = false
sprite.depth_write_enabled = false
sprite.slot_depth_offset = 0
sprite.alpha_cutoff = 0
```

These depth-write settings control **generated materials**. Custom materials retain their own render modes.

### Choose spacing for your scene

`slot_depth_offset` is in node-local Godot units and is independent of `pixel_size`. It is affected by the node's 3D transform. Zero or insufficient spacing can cause depth conflicts, particularly with tilted overlapping triangles under perspective. No constant gap guarantees precision for all camera ranges, scales and hardware.

The regression covers default `pixel_size = 0.01` with a `0.001` gap, and a larger 48-unit-wide fixture viewed from 60 units away with a `0.01` gap. On the tested Compatibility backend, the latter needed more than `0.001` once vertex displacement was active. Tune spacing and camera clipping ranges for your scene. Larger gaps make the layered construction more visible and increase the position jump when the camera crosses edge-on.

Alpha fades and soft atlas edges also deserve testing: fragments disappear at the cutoff, and surviving translucent fragments can occlude later geometry behind them. This is not order-independent transparency.

## Generated lighting, normal maps, and shadows

Lighting is opt-in so existing scenes keep their authored unlit appearance:

```gdscript
sprite.lighting_enabled = true
sprite.normal_map_enabled = true
sprite.normal_map_flip_y = true
sprite.normal_scale = 1.0
sprite.specular = 0.25
sprite.roughness = 0.65
sprite.metallic = 0.0
```

Generated lit materials use the attachment plane's normal when no normal map exists. The runtime supplies standard mesh normals and UV-derived tangents, so atlas pages with imported normal maps can use Godot's compressed tangent-space normal-map path. Normal-map loading is not a separate 3D path: configure the atlas importer's existing normal-map prefix (for example, `n` resolves `n_raptor.png`) and `SpineAtlasResource` supplies the matching page texture. `normal_map_flip_y` defaults on for the convention used by the bundled Raptor asset; disable it for maps authored with the opposite Y convention. Example19 starts the high-contrast Raptor map at `normal_scale = 0.25`; tune strength for each asset and light rig. Animated normal/PBR controls are uniforms and do not rebuild mesh buffers or duplicate warmed materials.

Generated shadow casting is also opt-in:

```gdscript
sprite.shadow_casting = SpineSprite3D.SHADOW_CASTING_DOUBLE_SIDED
sprite.shadow_alpha_cutoff = 0.3
```

Modes are **Off**, **On**, **Double-Sided**, and **Shadows Only**. Generated shadows use dedicated shadows-only RenderingServer instances and alpha-tested materials. They share the visible batch meshes and are allocated lazily on first use; turning shadows off hides and retains the warmed pool. `shadow_alpha_cutoff` is independent of visible `alpha_cutoff`, includes skeleton/slot/attachment tint alpha, and can be animated without creating a material for each value. Lighting does not need to be enabled for a generated material to cast shadows.

`camera_relative_depth` still operates per rendering camera for visible generated shaders. A shadow map has the light's view, so test extreme slot spacing, culling, and lights crossing the planar character in your scene. The alpha-tested shadow is a planar silhouette, not volumetric self-shadowing.

## Custom Spine materials

No user shader source is rewritten. Every cached `ShaderMaterial` pass receives these reserved uniforms from the sprite:

- `spine_texture`: current atlas-page texture.
- `spine_premultiplied_alpha`: the atlas page's PMA flag.
- `spine_normal_texture`: current atlas-page normal map, when one was imported.
- `spine_has_normal_texture`: whether the current page has a normal map.
- `spine_camera_relative_depth`: the sprite's camera-relative-depth flag.
- `spine_alpha_cutoff`: the sprite's visible alpha cutoff.
- `spine_normal_map_enabled`, `spine_normal_map_flip_y`, and `spine_normal_scale`.
- `spine_specular`, `spine_roughness`, and `spine_metallic`.

Custom shaders receive light tint in `CUSTOM0`, dark tint in `CUSTOM1`, and standard mesh `NORMAL`/`TANGENT` attributes. They remain authored contracts: the runtime never inserts lighting, normal-map, blending, culling, depth, or shadow code into their source. Custom batches cast directly according to `shadow_casting`, so the shader's authored alpha/scissor and culling behavior controls its shadow pass; `shadow_alpha_cutoff` is only for generated shadow materials.

Use Godot's normal-map outputs in a custom lit shader:

```glsl
uniform sampler2D spine_normal_texture : hint_normal, repeat_disable;
uniform bool spine_has_normal_texture = false;
uniform bool spine_normal_map_enabled = true;
uniform bool spine_normal_map_flip_y = true;
uniform float spine_normal_scale = 1.0;

void fragment() {
    // Set ALBEDO and ALPHA first.
    if (spine_normal_map_enabled && spine_has_normal_texture) {
        vec3 mapped_normal = texture(spine_normal_texture, UV).rgb;
        if (spine_normal_map_flip_y) mapped_normal.g = 1.0 - mapped_normal.g;
        NORMAL_MAP = mapped_normal;
        NORMAL_MAP_DEPTH = spine_normal_scale;
    }
}
```

Do not manually rely on the sampled blue channel: Godot may import normal maps with two-channel compression and reconstruct Z internally when `NORMAL_MAP` is used. `NORMAL_MAP_DEPTH = 0.0` restores the flat normal.

Template values for reserved uniforms do not override the sprite. Other uniforms remain template-owned. Reassign a template after changing its parameters to refresh cached copies. Every visible custom pass, including `next_pass`, needs to implement the policy if it should match the generated geometry. A pass that ignores it retains physical vertex positions and can hide or detach facial details from the back. Non-shader next passes do not acquire custom vertex logic automatically.

### Obtain the shared helper

`SpineSprite3D.get_depth_shader_code()` returns the current helper declarations and functions. You can explicitly include that string when constructing a shader, or save a `ShaderInclude` once from an editor script:

```gdscript
@tool
extends EditorScript

func _run():
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://shaders"))
    var include := ShaderInclude.new()
    include.code = SpineSprite3D.get_depth_shader_code()
    var error := ResourceSaver.save(include, "res://shaders/spine-depth.gdshaderinc")
    assert(error == OK)
```

Regenerate a saved include when updating the runtime's shader contract. Example18 contains a copy checked against the runtime helper by the regression suite.

### Complete unlit normal-blend example

```glsl
shader_type spatial;
render_mode unshaded, cull_disabled, blend_mix, depth_draw_always;
#include "res://shaders/spine-depth.gdshaderinc"

uniform sampler2D spine_texture : repeat_disable;
uniform bool spine_premultiplied_alpha = false;
varying vec4 spine_light;
varying vec3 spine_dark;

vec3 srgb_to_linear(vec3 color) {
    return mix(pow((color + vec3(0.055)) / 1.055, vec3(2.4)),
        color / 12.92, lessThanEqual(color, vec3(0.04045)));
}

void vertex() {
    VERTEX = spine_apply_camera_depth(VERTEX, MODEL_MATRIX[3].xyz,
        MODEL_NORMAL_MATRIX[2], INV_VIEW_MATRIX[3].xyz);
    // Apply your additional 3D vertex deformation AFTER the spacing helper.
    spine_light = CUSTOM0;
    spine_dark = CUSTOM1.rgb;
    if (!OUTPUT_IS_SRGB) {
        spine_light.rgb = srgb_to_linear(spine_light.rgb);
        spine_dark = srgb_to_linear(spine_dark);
    }
}

void fragment() {
    vec4 tex = texture(spine_texture, UV);
    ALPHA = tex.a * spine_light.a;
    spine_apply_alpha_cutoff(ALPHA);
    if (spine_premultiplied_alpha)
        tex.rgb = tex.a > 0.000001 ? tex.rgb / tex.a : vec3(0.0);
    if (!OUTPUT_IS_SRGB) tex.rgb = srgb_to_linear(tex.rgb);
    ALBEDO = tex.rgb * spine_light.rgb + (vec3(1.0) - tex.rgb) * spine_dark;
}
```

Use `blend_add` for an additive template. Keep `ALPHA` output and explicit discard to stay in Godot's transparent ordering path. `ALPHA_SCISSOR_THRESHOLD` can select the opaque queue, which does not use the same batch-order sorting contract. `depth_write_enabled` cannot change a custom shader's `render_mode`: use `depth_draw_never` yourself if you want a no-write custom material.

The helper acts on the native attachment's input Z, which contains its slot spacing. **Do not apply it to every vertex of an arbitrary 3D child mesh**: that would reflect the child's shape, not merely move its attachment point.

## Slot children: explicit single-camera following

Ordinary `SpineSlotNode3D` children keep their physical transforms by default. The parent sprite's vertex shader does not move their nodes or rewrite their materials.

For a visual attachment intended for one camera, set the helper's `depth_camera`:

```gdscript
slot_node.depth_camera = slot_node.get_path_to(camera)
# Restore fixed physical depth:
slot_node.depth_camera = NodePath()
```

The camera must be a `Camera3D` in the same `World3D`. The helper changes only its anchor's depth sign, preserving child-local offsets, scale and mesh shape. It works with ordinary Godot child materials. It follows camera movement before rendering even if skeleton updates are manual or paused. Clearing the path, deleting the camera, selecting an invalid/different-world camera, or disabling the parent's camera-relative flag restores fixed physical depth.

This is explicitly a **single-camera policy**. Spine's own shader can render correctly into opposite views simultaneously, but a real child node cannot have two transforms at once. For shared-world multi-camera scenes, retain physical anchors or implement a separate per-view visual attachment arrangement. The helper reports this limitation in its configuration warnings.

The change affects **all descendants**, including physics/collision nodes. Prefer a fixed gameplay/collision hierarchy and a separate visual attachment hierarchy rather than moving physics bodies according to camera position. Inserted geometry is still not Spine-clipped, and transparent child passes still need the character's render priority.

## Bounds, picking and example

Native mesh culling bounds and `sprite.get_aabb()` include both possible depth directions when `camera_relative_depth` is enabled. Camera movement does not rebuild or upload Spine vertex buffers. Bounds exclude inserted children and additional custom shader displacement.

Editor click/box picking conservatively includes attachment triangles at both possible depth positions. It may select a triangle at the currently unused position, especially with exaggerated spacing. It does not follow texture alpha, arbitrary custom shader deformation, or gameplay collision shapes.

Run `examples/18-depth-offset-orbit/depth-offset-orbit.tscn` in either Godot 4 example project. It uses the native sprite properties and generated materials. Compare the front/back toggle, then use **Exaggerate gap**, **Marker follows flip**, and **Face shader ignores flip** to inspect the child/custom-material contracts. The default gap can be restored with **Gap 0.001**.

Run `examples/19-3d-lighting/3d-lighting.tscn` to inspect generated lighting, the Raptor atlas normal map, moving omni-light response, normal strength/Y convention, and texture-alpha shadows on opaque ground.
