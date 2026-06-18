# spine-ts 4.3 changelog

## Unreleased

### spine-ts

- Updated release automation to publish from `spine-ts-x.y.z` tags via GitHub Actions and npm trusted publishing.

## 4.3.7 - 2026-06-05

### spine-pixi-v8

- Optimize pixi v8 inactive slot rendering. See #3103.

## 4.3.6 - 2026-06-03

### **BREAKING CHANGES**

- `Animation.apply()` and `Timeline.apply()` now take a `MixFrom` value instead of a boolean `fromSetup` argument. Replace `true` with `MixFrom.setup` and `false` with `MixFrom.current`, and import `MixFrom` from `@esotericsoftware/spine-core`. Caused by port of 1ebd39eb. (`e411dc8ca`)

### spine-core

- Port of 1ebd39eb: Fixed mixing timelines without a key on frame 0. (`e411dc8ca`)
- Port of 64606e48: Fixed timeline IDs for draw order. (`406d774a4`)

### spine-threejs

- Set `forceSinglePass` by default for transparent double-sided Spine materials to preserve slot draw order when negative bone scales flip triangle winding. (`ea0d35121`)

## 4.3.5 - 2026-05-26

### spine-core

- Fixed JSON transform constraint timelines to carry `mixShearY` forward between keyframes, and fixed world-space `ToShearY` scale-axis handling. (`cef55a40d`)
- Port of 71999c27: Fixed draw order timelines not mixing out to setup pose. (`7f6f47cb4`)
- Port of d463f340: Support nonessential slider max. (`39f6fc167`)

## 4.3.4 - 2026-05-25

### spine-core

- Fixed `SkeletonClipping.clipTrianglesUnpacked()` to update its typed output views before returning from inverse clipping. (`d1981317b`)

### spine-pixi-v8

- Fixed clipped attachments to force a Pixi batch rebuild when triangle indices change without changing the index count, preventing rendering glitches. (`6fc9a8bd4`)

### spine-ts

- Updated `publish.sh` to prompt for an npm one-time password and pass it to `npm publish`. (`9624f105a`)

## 4.3.3 - 2026-05-21

### spine-pixi-v7 / spine-pixi-v8

- Added `followSlotColor` for slot objects, allowing attached Pixi objects to be tinted from their Spine slot color. See #3053. (`90e0f0c92`)

## 4.3.2 - 2026-05-21

### spine-webgl

- Added `pmaAdditiveBatching` to allow batching additive slots together with normal slots when using premultiplied alpha. (`ab1cf3a60`)

## 4.3.1 - 2026-05-19

### spine-core

- Fixed `ScaleYMode.volume` so it does not produce extreme/infinite values for very small scale factors. (`51f430084`)
- Fixed `BonePose.updateLocalTransform()` for `noScale` and `noScaleOrReflection` inheritance. (`9459ef475`)
- Fixed bones that do not inherit rotation when parent scale is near zero. (`608e19dd0`)

### spine-phaser-v4

- Fixed batching issues. See #3086. (`2e57089a2`)

### spine-ts

- Updated development dependencies to resolve vulnerabilities. (`45a5efc87`)
- Updated README Spine compatibility text to 4.3. (`dd2e2cc63`)

## 4.3.0 - 2026-05-15

Initial 4.3 release for the TypeScript/JavaScript runtimes.

### spine-core

- Added slider constraint support: `Slider`, `SliderData`, `SliderTimeline`, and `SliderMixTimeline`.
- Added the new pose system with `BoneLocal`, `BonePose`, `Pose`, `Posed`, and `PosedActive`.
- Added unified constraint timeline/indexing APIs.
- Added animation bone index access via `Animation.getBones()`.
- Added `Skeleton` physics force direction properties: `windX`, `windY`, `gravityX`, and `gravityY`.
- Added sequence animation support via `SequenceTimeline`.
- Added linked mesh timeline propagation across source meshes in different slots.
- Added attachment timeline helpers: `Attachment.timelineSlots` and `Attachment.isTimelineActive()`.
- Added `DrawOrderFolderTimeline`.
- Added timeline blending capability flags and `TrackEntry.additive`.
- Added `TrackEntry.mixInterpolation` and `Interpolation` helpers for non-linear `AnimationState` mixes.
- Added support for passing `null` to `Skeleton.setAttachment()`.
- Added clipping runtime support for convex and inverse clipping.
- Added nonessential data accessors including `Animation.color` and bone icon size/rotation.
- Added `ScaleYMode`/IK scale-y support.
- Ported parser fixes from spine-libgdx, including path constraint flag fixes and weighted mesh binary vertex allocation/count fixes.
- Ported additive timeline and alpha/RGB timeline flicker fixes from spine-libgdx.
- Fixed `SkeletonData` default FPS and missing `PathAttachment` initialization.
- Fixed reverse IK bend positive logic and transform constraint/slider scaling issues.
- Fixed attachment timelines so hidden setup-pose attachments remain hidden while mixing out, preserving deform behavior.
- Fixed `AnimationState` listener dispatch, track/mix behavior, and draw order handling.
- Fixed clipping regressions and inverse clipping crash cases.

#### spine-core breaking changes

- `Bone`, `Slot`, constraints, and constraint data now expose state through the new pose/setup-pose APIs.
- `SkeletonData` now has a unified `constraints` list and unified `findConstraint()` API.
- Setup pose methods were renamed, for example `Skeleton.setToSetupPose()` -> `Skeleton.setupPose()`.
- `Physics` moved from nested `Skeleton.Physics` to standalone `Physics` export.
- Timeline `apply()` signatures now use `fromSetup`, `add`, `out`, and `appliedPose` instead of `MixBlend` and `MixDirection`.
- Removed `MixBlend`, `MixDirection`, `TrackEntry.holdPrevious`, and `TrackEntry.mixBlend`; use `TrackEntry.additive`.
- `AnimationState.setCurrent()` was renamed to `setTrack()`; `getCurrent()` is deprecated in favor of `getTrack()`.
- Attachment `computeWorldVertices()` methods now take a `skeleton` parameter.
- `MeshAttachment.getParentMesh()` / `setParentMesh()` were renamed to `getSourceMesh()` / `setSourceMesh()`.
- `RegionAttachment` and `MeshAttachment` constructors now take a non-null `Sequence`.
- Event setup values moved from `EventData` fields to `eventData.setupPose`.
- `SkinEntry.name` was renamed to `placeholderName`; attachment loader methods now receive both `placeholder` and resolved attachment `name`.
- `IkConstraintData.uniform` was replaced by `scaleYMode` / `ScaleYMode`.

### spine-canvas

- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs.

### spine-canvaskit

- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs.
- Fixed the headless example.
- Fixed clipping vertex alignment that could lead to crashes.

### spine-phaser-v3

- Added physics inheritance settings.
- Fixed example bundling/loader issues.
- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs.

### spine-phaser-v4

- Added physics inheritance settings.
- Moved to stable Phaser 4 support.
- Fixed ESM, mixin, camera, and framebuffer rendering issues.
- Fixed example bundling/loader issues.
- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs.

### spine-pixi-v7

- Added `createOptions`.
- Added `allowMissingRegions` support.
- Added physics inheritance settings.
- Added ticker options.
- Fixed asset loader and slot object transform/rendering issues.
- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs.

### spine-pixi-v8

- Added `createOptions`.
- Added `allowMissingRegions` support.
- Added physics inheritance settings.
- Added ticker options.
- Added canvas rendering support.
- Restored the control bones example.
- Fixed asset loader and slot object transform/rendering issues.
- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs.

### spine-player

- Added multiple skin support.
- Added boolean `debug` support.
- Improved progress bar interactions.
- Fixed Windows build issues.
- Fixed resize and empty-frame viewport regressions.
- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs.

### spine-threejs

- Added a React Three Fiber example.
- Added support for Three.js `0.162.0` through `0.184.x`.
- Added physics inheritance settings.
- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs.

### spine-webcomponents

- Improved asset caching.
- Fixed empty skin handling.
- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs.

### spine-webgl

- Added PMA metadata propagation through `AssetManagerBase` and texture loaders.
- Added `SkeletonRendererCore` for shared runtime rendering logic.
- Fixed empty atlas loops.
- Fixed WebGL context restore binding.
- Fixed canvas resizing beyond WebGL limits.
- Updated to use the new 4.3 TypeScript/JavaScript core runtime APIs and PMA handling.
