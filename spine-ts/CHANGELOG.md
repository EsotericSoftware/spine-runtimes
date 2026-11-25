# spine-ts 4.3 changelog

## Unreleased

### spine-canvas

- Added slot blend mode and clipping attachment support to triangle rendering, now backed by `SkeletonRendererCore`.
- Added blend/clipping examples and improved canvas example fitting.

## 4.3.13 - 2026-07-24

### spine-construct3

- Added an opt-in slot Z offset to mitigate z-fighting between overlapping attachments.

### spine-phaser-v3

- Automatically restored Spine WebGL resources, including atlas textures, after context restoration.

### spine-phaser-v4

- Restored the legacy `spine-webgl` backend after WebGL context restoration.

## 4.3.12 - 2026-07-23

### spine-core

- Expose restore WebGL logic on ManagedWebGLRenderingContext, so applications that pass an existing WebGL context instead of a canvas can restore all registered WebGL resources.

### spine-construct3

- Added planar 3D rotation for rendering; collisions, hit tests, and other gameplay logic remain 2D. Generated collision bodies synchronize their visual 3D transform.

## 4.3.11 - 2026-07-20

### **BREAKING CHANGES**

- `spine-phaser-v4` now requires Phaser 4.2.1 and uses Phaser's native `Mesh2D` renderer by default in WebGL games. Pass `{ renderer: "spine-webgl" }` when creating a `SpineGameObject` to retain the previous spine-webgl renderer. Phaser Canvas games use `renderer: "spine-canvas"`.
- In `spine-phaser-v3`, `spine-phaser-v4`, `spine-pixi-v7`, `spine-pixi-v8`, and `spine-threejs`, physics movement inheritance is now opt-in and defaults to `0`; the previous properties and methods have been replaced by the runtime object's `skeletonPhysics` API.

### spine-core

- Added the renderer-agnostic `SkeletonCoordinateConverter` and `SkeletonPhysicsMovement` APIs for converting runtime coordinates and optionally passing host-object translation and rotation to skeleton physics constraints. Movement adapters provide 3D world position, rotation around the skeleton plane normal, and world-to-skeleton conversion without exposing engine-specific matrix types; 2D runtimes use `z = 0`.

### spine-phaser-v3 / spine-phaser-v4

- Added `spineSkeleton(key, url, options)` as the unified skeleton data loader. It infers JSON or binary data from `.json` and `.skel` URL extensions, including URLs with query strings or fragments. Use `options.format` when the URL has no usable extension and `options.xhrSettings` to customize the request. The existing `spineJson()` and `spineBinary()` loaders remain available as deprecated aliases.
- Changed the preferred `spineAtlas()` third argument to `SpineAtlasFileOptions`, with XHR settings supplied through `options.xhrSettings`. Passing a raw `XHRSettingsObject` remains supported as a deprecated overload.
- Added the options-object `SpineGameObject` constructor and `SpineGameObjectFactoryOptions` for `this.add.spine()`. The previous positional constructor and direct bounds-provider factory argument remain available as deprecated overloads.
- Added `skeletonToGame()`, `gameToSkeleton()`, and `gameToBone()` coordinate conversion methods. The previous Phaser-specific method names remain available as deprecated aliases.
- Moved `SpineGameObjectBoundsProvider`, `AABBRectangleBoundsProvider`, `SetupPoseBoundsProvider`, and `SkinsAndAnimationBoundsProvider` into `SpineGameObjectBounds.ts` while preserving their existing exports.
- Added the opt-in `SpineGameObject.skeletonPhysics` movement inheritance API backed by `SkeletonPhysicsMovement`.
- Updated all examples to use the unified loading and game-object creation APIs.

### spine-phaser-v4

- Added selectable `"phaser"` (default), `"spine-webgl"`, and `"spine-canvas"` renderer backends. The `phaser` renderer uses the new Phaser 4.2.1 `Mesh2D`, and supports Spine tint, tint-black, alpha, blend modes, atlas texture filtering and wrapping, premultiplied-alpha handling, clipping, batching, and Phaser render-context integration.
- Added APIs for attaching Phaser game objects to Spine slots with before/after placement, attachment timeline following, clipping, and optional position preservation. Attached objects inherit the `SpineGameObject` alpha and scroll factor while rendered. Thanks to stencil-buffer integration, Spine clipping attachments can clip Phaser game objects attached to slots.
- Shared the spine-webgl `SceneRenderer` between scenes using the same Phaser WebGL renderer and exposed it through `SpinePlugin.webGLRenderer`.
- Updated all examples to use the unified loading and game-object creation APIs.

### spine-threejs

- Added `SkeletonMesh.skeletonPhysics` with 3D transform movement inheritance.

### spine-pixi-v7 / spine-pixi-v8

- Added `Spine.skeletonPhysics` and fixed rotation inheritance under non-uniformly scaled containers.

### spine-construct3

- Add drag and drop support for a zip file containing skeleton assets.

## 4.3.10 - 2026-07-06

### spine-core

- Fixed `SkeletonRendererCore` clipping lifecycle and missing texture handling.
- Port of 92be332e: Clipping performance improvements.
- Relax `findConstraint` constructor type.

## 4.3.9 - 2026-06-24

### spine-pixi-v8

- Add unloadFromCache to unload SkeletonData from cache. See ##3054.

### spine-core

- Port of 50e82f31: Fixed constraints overwriting bone transforms from other constraints.

## 4.3.8 - 2026-06-19

### spine-construct3

- Added the official spine-construct3 runtime/plugin.

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
