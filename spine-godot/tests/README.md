# Spine Godot regressions

## Controller and instance lifetime

`spine-godot/tests/controller.gd` validates shared-resource instance isolation, signal argument types and ordering, visibility timing, resource replacement from update and animation callbacks, global bone transforms, the modified-bone second world-transform update, and retained-wrapper invalidation after reload and teardown.

Run it with the module editor binary:

```sh
GODOT="$(pwd)/spine-godot/godot/bin/godot.macos.editor.arm64"
SCRIPT="$(pwd)/spine-godot/tests/controller.gd"
"$GODOT" --headless --path "$(pwd)/spine-godot/example-v4" --script "$SCRIPT"
```

Run it against the GDExtension example with a vanilla Godot editor after building the extension:

```sh
GODOT="/path/to/vanilla/Godot.app/Contents/MacOS/Godot"
"$GODOT" --headless --path "$(pwd)/spine-godot/example-v4-extension" --script "$SCRIPT"
```

The lifetime assertions intentionally call retained wrappers after invalidation, so Godot logs the expected `Native Spine object not set.` errors while the test still passes.

`callback-time-scale.gd` checks live `set_time_scale()` changes at each update callback for both sprite types, including zero scale, different animation/physics phase scales and hidden animation updates. Run headlessly with the same command pattern and require `Spine callback time-scale regression passed.` with no errors.

## Tint-black rendering

`spine-godot/tests/tint-black.gd` creates its Spine JSON, atlas, and texture fixture under `user://` at runtime. It renders a 48×16 black/gray/white texture band through a `SubViewport` and asserts captured GPU pixels for two-color tinting with HDR 2D disabled and enabled. Linear colors are expected only when a `RenderingDevice` is available; Compatibility must retain sRGB colors even with the HDR 2D viewport option enabled.

Run it with the module editor binary (not `--headless`):

```sh
GODOT="$(pwd)/spine-godot/godot/bin/godot.macos.editor.arm64"
SCRIPT="$(pwd)/spine-godot/tests/tint-black.gd"
"$GODOT" --path "$(pwd)/spine-godot/example-v4" --script "$SCRIPT"
```

Run the module test on the Compatibility renderer too:

```sh
"$GODOT" --rendering-method gl_compatibility --path "$(pwd)/spine-godot/example-v4" --script "$SCRIPT"
```

Run the same test against the GDExtension example after its binary has been built, using a vanilla Godot editor rather than the module editor binary:

```sh
GODOT="/path/to/vanilla/Godot.app/Contents/MacOS/Godot"
SCRIPT="$(pwd)/spine-godot/tests/tint-black.gd"
"$GODOT" --path "$(pwd)/spine-godot/example-v4-extension" --script "$SCRIPT"
```

The test waits for multiple `RenderingServer.frame_post_draw` signals after each state change. It covers region and mesh attachments, dynamic dark colors, disabling and re-enabling dark colors while reusing draw-order meshes, initial and updated clipping topology, additive and multiply blend modes, sprite and `SpineSlotNode` custom-material overrides, a zero RGB light color with parent `CanvasItem` modulation, and alpha. HDR 2D snapshots use linear expected colors when a `RenderingDevice` is active.

## AnimationPlayer (Godot 4)

`animation-player.gd` checks both sprite types: generated clips, direct/nested playback, loop/mix options, native poses, callback resource replacement and reparenting. `editor-animation-player.gd` opens the actual AnimationPlayer editor and checks forward/backward clip scrubbing with nested `root_node` paths, native poses, holding the scrubbed pose after closing/switching the editor, inactive-track preview preservation, and AnimationTree-editor clock handoff. Both run against module and GDExtension builds.

```sh
"$GODOT" --headless --path /path/to/example-project \
  --script "$(pwd)/spine-godot/tests/animation-player.gd"
bash spine-godot/tests/editor-animation-player.sh /absolute/path/to/module-godot
bash spine-godot/tests/editor-animation-player.sh /absolute/path/to/vanilla-godot \
  "$(pwd)/spine-godot/example-v4-extension/bin"
```

The editor launcher creates a disposable project and exits its test editor automatically. Do not enable the test plugin in a working project. Require the runtime/editor `Spine AnimationPlayer ... regression passed.` marker and no script/runtime errors.

## Experimental depth-write ordering probe (Godot 4)

`depth-order-3d.gd` uses custom materials on actual Spine batches without changing runtime defaults. It tests `depth_draw_always` plus an explicit alpha-threshold `discard`, one/two batches, animated draw order, front/back and perspective/orthographic views, scene depth composition and coplanar insertion. A built-in `ALPHA_SCISSOR_THRESHOLD` control reports opaque-queue reordering separately.

**This is a zero-spacing diagnostic experiment, not a passing release regression:** tilted/mirrored region-plus-mesh overlap reports depth failures under perspective. The native default-depth regression is `depth-policy-3d.gd` below. The same geometry passes with depth writes disabled or with identical diagnostic fragment depth. The latter is an isolation probe, not a proposed scene-depth implementation. Read the `DEPTH_PRECISION_PROBE` / `OPAQUE_QUEUE_CONTROL` output and failures together.

```sh
"$GODOT" --path /path/to/example-project --rendering-method gl_compatibility \
  --script "$(pwd)/spine-godot/tests/depth-order-3d.gd"
```

Pass `-- --offsets` to test the existing physical `slot_depth_offset` instead. The sweep covers zero through 0.01 units and requires **0.001** to fix the front-view rotated/mirrored repro for both draw orders and projection types. It rechecks scene depth composition and slot insertion with that offset. Back-view results are reported separately: physical layer separation makes earlier layers nearer from behind, so depth-writing transparent layers no longer reproduce the zero-offset painter composite. This test changes no runtime defaults.

`depth-orbit-scene.gd` tests example18's orbit, unchanged native draw order/depth spacing across camera angles, and depth/cutoff controls. GPU comparisons exclude the HUD and check unchanged front-view pixels when enabling the camera-relative shader flip, changed back-view composition, face-material opt-out, and the corrected/uncorrected slot marker. Run it with a GPU renderer and an isolated example project containing `examples/18-depth-offset-orbit/`. Optional `-- --snapshots=/absolute/path` saves comparison screenshots. Require `Spine depth-offset orbit scene regression passed.` and no shader/script errors. The scene now uses native sprite properties and `SpineSlotNode3D.depth_camera`; its custom shader include is checked against `get_depth_shader_code()`. It does not validate multi-camera children, physics or shadows.

## Native depth policy (Godot 4)

`depth-policy-3d.gd` exercises the generated depth-writing defaults: front/back ordering within and across native batches, perspective/orthographic projections, rotated/mirrored/nonuniform transforms, alpha cutoff, scene occlusion, no-write opt-out, warmed material reuse, custom helper/next-pass uniforms, simultaneous opposite views, expanded culling bounds, clipping/deform, and explicit-camera child lifecycle. It tests the default 0.001 gap at default pixel scale, plus a 0.01 gap for the much larger 48-unit fixture viewed from 60 units away. The smaller gap is not a universal precision guarantee.

```sh
"$GODOT" --path /path/to/example-project --rendering-method gl_compatibility \
  --script "$(pwd)/spine-godot/tests/depth-policy-3d.gd"
```

Repeat on Mobile and Forward+ for both module/GDExtension builds. Require `Spine native depth policy: PASS`, the rendering regression pass marker, and no errors. See [DEPTH_RENDERING.md](../DEPTH_RENDERING.md) for user-facing defaults and custom-shader/child contracts.

## Interactive benchmark scene (Godot 4)

`examples/17-interactive-benchmark/benchmark.tscn` provides 2D/3D population controls and live CPU/render statistics. `benchmark-scene.gd` tests both modes with 50, 96 and 1000 characters, counters, pause/resume, animation switching and sample reset. Run against an imported isolated example project, with a GPU renderer:

```sh
"$GODOT" --path /path/to/example-project --rendering-method gl_compatibility \
  --script "$(pwd)/spine-godot/tests/benchmark-scene.gd"
```

Repeat for Mobile and Forward+. Require `Spine interactive benchmark regression passed.` and no errors. This functional smoke test is not a substitute for settled frame measurements. `BENCH_SCENE_LOAD` includes population replacement and eight frames; it is not a per-frame timing.

`benchmark-frames.gd` collects a settled window, defaulting to 3 seconds warm-up plus 5 seconds capture, with VSync disabled. `SPINE_FRAME_BENCH` JSON reports mean/median/p95/worst frame interval and CPU-update milliseconds, measured FPS, sample count, actual duration, viewport size, VSync and build metadata. These are wall-clock frame-start intervals, not smoothed simulation deltas or GPU-only measurements. Run without other benchmarks competing for CPU/GPU.

```sh
"$GODOT" --path /path/to/example-project --rendering-method gl_compatibility \
  --script "$(pwd)/spine-godot/tests/benchmark-frames.gd" -- \
  --count=96 --dimension=3 --settle=3 --seconds=5 --vsync=false
"$GODOT" --headless --path /path/to/example-project \
  --script "$(pwd)/spine-godot/tests/benchmark-timing.gd"
```

`benchmark-timing.gd` deterministically checks warm-up exclusion, full-interval/CPU pairing, duration, quantiles, FPS and reset behavior using injected timestamps. The real frame-capture test additionally requires renderer draw events and the `Spine settled frame timing regression passed.` marker.

## 3D editor selection (Godot 4)

`editor-picking-3d.gd` is an editor-plugin regression that drives Godot's actual 3D viewport click and box-selection input paths. It checks geometry selection, changed poses and pixel size, removed attachments/padded buffers, visibility, and resource clear/replacement. The launcher creates and removes an isolated editor project; do not enable this auto-running plugin in a working example project. It exits the test editor when finished.

```sh
bash spine-godot/tests/editor-picking-3d.sh /absolute/path/to/module-godot
bash spine-godot/tests/editor-picking-3d.sh /absolute/path/to/vanilla-godot \
  "$(pwd)/spine-godot/example-v4-extension/bin"
```

Use an editor build with a graphical session. Check for `Spine 3D editor picking regression passed.` and no errors. Picking follows attachment triangles, like Godot's mesh gizmos, rather than individual texture-alpha pixels or custom vertex-shader displacement. The picking mesh is editor-only and cached until geometry changes; it does not add runtime draw calls.

## Native 3D (Godot 4)

`controller-3d.gd` covers mixed 2D/3D shared resources with independent state, 3D lifecycle signal arguments/order, hidden animation time, callback resource replacement, retained wrappers, shared-resource reload, world exit/re-entry, and character priorities independent of slot count (including 257 and 1024 slots). Its deliberate negative checks log **7** `Native Spine object not set.` errors. There is no slot-count rejection.

```sh
"$GODOT" --headless --path /path/to/example-project --script "$(pwd)/spine-godot/tests/controller-3d.gd"
```

`rendering-3d.gd` explicitly selects the original no-write, zero-spacing configuration to preserve its transparent-composition references; native defaults are tested separately above. It renders a native `SpineSprite3D` in a test-only `SubViewport`; the node itself creates only native mesh/instance RIDs, not a viewport or hidden sprite. Tests cover region/mesh/weighted attachments, deform and bounds updates, dynamic tint black, clipping topology transitions, pixel scaling, manual visibility refresh, animated Spine draw order from front/back cameras, culling, unlit shading, opaque depth occlusion, normal/additive blending, explicit unsupported multiply/screen fallbacks, unmodified custom spatial shaders, PMA-versus-straight alpha, and different character priorities. The two unsupported-blend warnings are intentional.

```sh
for renderer in gl_compatibility mobile forward_plus; do
  "$GODOT" --rendering-method "$renderer" --path /path/to/example-project \
    --script "$(pwd)/spine-godot/tests/rendering-3d.gd"
done
```

`batching-3d.gd` adds GPU ordering proofs and warm-update measurements: 512 compatible attachments in one draw, 16,385 attachments in one batch using indices above 65535, shrinking padded buffers, alternating render states sharing materials, insertion boundaries, multi-surface children, detached sorting restoration, front/back and perspective/orthographic cameras, material overrides/animated uniforms, culling/priority cache reuse, and animated clipping/deform. `BATCH_BENCH` JSON lines report CPU update costs, render counters and mesh/material/upload statistics. These are CPU-submission microbenchmarks, not GPU frame-time or FPS claims.

```sh
"$GODOT" --rendering-method gl_compatibility --path /path/to/example-project \
  --script "$(pwd)/spine-godot/tests/batching-3d.gd"
"$GODOT" --rendering-method gl_compatibility --path /path/to/example-project \
  --script "$(pwd)/spine-godot/tests/performance-3d.gd"
```

`render-lifecycle-3d.gd` adds GPU regressions for first batch allocation and pool growth while hidden (including inherited visibility), insertion sorting on manual/disabled sprites after unrelated tree additions/removals, inactive-bone child visibility, and selective restoration during child transfers. Run with a GPU renderer and require the rendering regression pass marker with no errors:

```sh
"$GODOT" --rendering-method gl_compatibility --path /path/to/example-project \
  --script "$(pwd)/spine-godot/tests/render-lifecycle-3d.gd"
```

`performance-3d.gd` also runs on the pre-batching Stage2 binary for an identical 128-compatible-quad comparison. Run both benchmarks on all three renderers. Godot4.6/4.7 **Mobile's viewport draw-call statistic counts visible instances**, not surfaces/passes; it underreports multi-draw children. The single-surface batch benchmark counts remain comparable, and multi-surface insertion correctness is checked against GPU reference pixels.

These tests create and remove their own fixtures under `user://`. They do not change example scenes or imports. The 3D GPU test explicitly calls `RenderingServer.force_draw()` before readback because a manual-mode sprite in an otherwise empty main window need not request window redraws. Check for the test's `regression passed.` message **and no `SCRIPT ERROR`**, rather than relying on Godot's process exit code alone.

Run the controller, tint-black, rendering-3d and batching-3d regression scripts for every supported Godot minor in `.github/workflows/spine-godot-v4-all.yml` and `spine-godot-extension-v4-all.yml`, using the matching vanilla editor for GDExtension tests. Module and GDExtension API builds must be validated independently: their include paths and APIs differ. Use isolated example copies for editor/demo imports, since Godot can automatically change shared textures' import settings after detecting 3D use.
