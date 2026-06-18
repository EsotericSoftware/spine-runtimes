# spine-haxe 4.3 changelog

Recovered from the root `4.3` changelog up to the `4.3.0` release.

## Unreleased

## 4.3.1 - 2026-06-04

### **BREAKING CHANGES**

- `Animation.apply()` and `Timeline.apply()` now take a `MixFrom` value instead of a boolean `fromSetup` argument. Replace `true` with `MixFrom.setup` and `false` with `MixFrom.current`, and import `spine.animation.MixFrom`. Caused by port of spine-libgdx commit `1ebd39eb`. (`2e0d00998`)

### spine-haxe

- Updated `publish.sh` to optionally move unreleased changelog entries to the new version section with the release date.
- Fixed mixing timelines without a key on frame 0, ported from spine-libgdx commit `1ebd39eb`. (`2e0d00998`)
- Fixed timeline IDs for draw order, ported from spine-libgdx commit `64606e48`. (`ce3f4aa57`)
- Fixed draw order timelines not mixing out to setup pose, ported from spine-libgdx commit `71999c27`. (`af9e14302`)
- Renamed the `BoundsProvider` source folder to `boundsprovider` to match its package name and fix compilation on case-sensitive filesystems. (`25c38723d`)
- Ported support for nonessential slider constraint `max` data from spine-libgdx commit `d463f3406`. (`b1a875db4`)
- Fixed bones that don't inherit rotation when parent scale is near zero, ported from spine-libgdx commit `7dc4d495`. (`0f9a56929`)
- Fixed `BonePose.updateLocalTransform()` for `noScale` and `noScaleOrReflection` inheritance, ported from spine-libgdx commit `6532b08b`. (`f046a8233`)
- Fixed `ScaleYMode.volume` so it does not produce extreme/infinite values for very small scale factors, ported from spine-libgdx commit `701f883`. (`db01a8f06`)
- Updated README Spine compatibility text to 4.3. (`dd2e2cc63`)

## 4.3.0 - 2026-05-15

### spine-haxe

- **Additions**
  - Added `Slider` and `SliderData` classes for slider constraints
  - Added `SliderTimeline` and `SliderMixTimeline` for animating sliders
  - Added new pose system with `BoneLocal`, `BonePose`, and related classes
  - Added `Pose`, `Posed`, and `PosedActive` base classes for unified pose management
  - Added `ConstraintTimeline` interface for unified constraint timeline indexing
  - Added `Animation.getBones()` to get bone indices used by an animation
  - Added `Skeleton` properties `windX`, `windY`, `gravityX`, `gravityY` to allow rotating physics force directions
  - Added `SequenceTimeline` for sequence animation
  - Added `allowMissingRegions` parameter to `AtlasAttachmentLoader` constructor to support skeletons exported with per-skin atlases
  - Linked meshes can now inherit deform and sequence timelines from source meshes in different slots
  - Added `Attachment.timelineSlots` and `Attachment.isTimelineActive()` for attachment timeline propagation across linked meshes
  - Added `DrawOrderFolderTimeline` for animating draw order folders
  - Added `Timeline.additive` and `Timeline.instant` to query timeline blending capabilities
  - Added `TrackEntry.additive` to control additive blending per track entry
  - Added `TrackEntry.mixInterpolation` and `Interpolation` helpers for non-linear AnimationState mixes
  - Ported the latest additive timeline updates and alpha/RGB timeline flicker fixes from spine-libgdx
  - Ported the AnimationState additive/hold rework from spine-libgdx. `MixBlend` and `MixDirection` are no longer used by timelines. The new system uses `fromSetup`, `add`, and `out` parameters and automatically calculates the required hold state values
  - Ported the Skin placeholder name rename from spine-libgdx. `SkinEntry.name` renamed to `placeholderName` to better match Spine editor terminology
  - Ported the sequence attachment refactor from spine-libgdx. `Sequence` now precomputes per-frame regions, UVs, and region offsets, and `RegionAttachment` / `MeshAttachment` now mirror the libgdx implementation
  - Ported the latest clipping runtime changes from spine-libgdx, including convex and inverse clipping support and the inverse clipping crash fix
  - Added `ClippingAttachment.convex` and `ClippingAttachment.inverse`
  - Added `Animation.color` for the animation color as it was in Spine when nonessential data is exported
  - Added `BoneData` icon size and rotation accessors for nonessential editor data
  - Added `ScaleY` enum and `IkConstraintData.scaleY` to control how IK compress/stretch changes `BonePose.scaleY`, including volume preservation
  - Fixed `SkeletonData` default FPS and missing `PathAttachment` initialization
  - BoundsProvider System: added a new flexible BoundsProvider system to improve bounds calculation performance and correctness across all renderers.
    - Added `BoundsProvider` abstract class with interface for calculating skeleton bounding boxes
    - Implemented four concrete `BoundsProvider` classes:
      - `AABBRectangleBoundsProvider` - Uses a simple axis-aligned bounding box rectangle
      - `CurrentPoseBoundsProvider` - Calculates bounds dynamically from the current skeleton pose
      - `SetupPoseBoundsProvider` - Uses setup pose bounds (default implementation)
      - `SkinsAndAnimationBoundsProvider` - Calculates bounds based on specific skins and animations

- **Bug fixes**
  - Fixed attachment timelines so hidden setup-pose attachments remain hidden while mixing out, preserving deform behavior.

- **Breaking changes**
  - `Bone` now extends `PosedActive` with separate pose, constrained, and applied states
  - `Bone` local transform properties moved to `bone.getPose()`:
    ||||
    |--------------------------|-|-----------------------------|
    | bone.x                  |→| bone.getPose().x            |
    | bone.y                  |→| bone.getPose().y            |
    | bone.rotation           |→| bone.getPose().rotation     |
    | bone.scaleX             |→| bone.getPose().scaleX       |
    | bone.scaleY             |→| bone.getPose().scaleY       |
    | bone.shearX             |→| bone.getPose().shearX       |
    | bone.shearY             |→| bone.getPose().shearY       |
  - `Bone` world and applied transform properties moved to `bone.getAppliedPose()`:
    ||||
    |---------------------------|-|-------------------------------------|
    | bone.ax                   |→| bone.getAppliedPose().x            |
    | bone.ay                   |→| bone.getAppliedPose().y            |
    | bone.arotation            |→| bone.getAppliedPose().rotation     |
    | bone.ascaleX              |→| bone.getAppliedPose().scaleX       |
    | bone.ascaleY              |→| bone.getAppliedPose().scaleY       |
    | bone.ashearX              |→| bone.getAppliedPose().shearX       |
    | bone.ashearY              |→| bone.getAppliedPose().shearY       |
    | bone.worldX               |→| bone.getAppliedPose().worldX       |
    | bone.worldY               |→| bone.getAppliedPose().worldY       |
  - `Bone` no longer provides a `skeleton` property, constructor no longer takes a `skeleton` parameter
  - `Slot` properties moved to `slot.getAppliedPose()`:
    ||||
    |---------------------------|-|-------------------------------------|
    | slot.attachment           |→| slot.getAppliedPose().attachment   |
    | slot.deform               |→| slot.getAppliedPose().deform       |
    | slot.sequenceIndex        |→| slot.getAppliedPose().sequenceIndex |
  - `Constraint` properties moved to `constraint.getPose()`:
    ||||
    |----------------------------------|-|----------------------------------------|
    | ikConstraint.mix                |→| ikConstraint.getPose().mix            |
    | ikConstraint.softness           |→| ikConstraint.getPose().softness       |
    | ikConstraint.bendDirection      |→| ikConstraint.getPose().bendDirection  |
    | ikConstraint.compress           |→| ikConstraint.getPose().compress       |
    | ikConstraint.stretch            |→| ikConstraint.getPose().stretch        |

    ||||
    |--------------------------------------|-|---------------------------------------|
    | transformConstraint.mixRotate       |→| transformConstraint.getPose().mixRotate |
    | transformConstraint.mixX            |→| transformConstraint.getPose().mixX    |
    | transformConstraint.mixY            |→| transformConstraint.getPose().mixY    |
    | transformConstraint.mixScaleX       |→| transformConstraint.getPose().mixScaleX |
    | transformConstraint.mixScaleY       |→| transformConstraint.getPose().mixScaleY |
    | transformConstraint.mixShearY       |→| transformConstraint.getPose().mixShearY |

    ||||
    |----------------------------------|-|------------------------------------|
    | pathConstraint.position         |→| pathConstraint.getPose().position |
    | pathConstraint.spacing          |→| pathConstraint.getPose().spacing  |
    | pathConstraint.mixRotate        |→| pathConstraint.getPose().mixRotate |
    | pathConstraint.mixX             |→| pathConstraint.getPose().mixX     |
    | pathConstraint.mixY             |→| pathConstraint.getPose().mixY     |

    ||||
    |--------------------------------------|-|---------------------------------------|
    | physicsConstraint.mix               |→| physicsConstraint.getPose().mix      |
    | physicsConstraint.gravity           |→| physicsConstraint.getPose().gravity  |
    | physicsConstraint.strength          |→| physicsConstraint.getPose().strength |
    | physicsConstraint.damping           |→| physicsConstraint.getPose().damping  |
    | physicsConstraint.massInverse       |→| physicsConstraint.getPose().massInverse |
    | physicsConstraint.wind              |→| physicsConstraint.getPose().wind     |
  - `ConstraintData` properties moved to `constraintData.setup`:
    ||||
    |-----|-|-----|
    | ikConstraintData.mix |→| ikConstraintData.setup.mix |
    | ...| |...|

  - `SkeletonData` now provides a single `ConstraintData` list `constraints` instead of separate lists per constraint type
    ||||
    |-----|-|-----|
    | skeletonData.ikConstraints        |→| Filter skeletonData.constraints for IkConstraintData instances |
    | skeletonData.transformConstraints |→| Filter skeletonData.constraints for TransformConstraintData instances |
    | skeletonData.pathConstraints      |→| Filter skeletonData.constraints for PathConstraintData instances |
    | skeletonData.physicsConstraints   |→| Filter skeletonData.constraints for PhysicsConstraintData instances |
  - `SkeletonData` now provides unified `findConstraint()` method with Class parameter:
    ||||
    |-----|-|-----|
    | skeletonData.findIkConstraint(name)        |→| skeletonData.findConstraint(name, IkConstraintData) |
    | skeletonData.findTransformConstraint(name) |→| skeletonData.findConstraint(name, TransformConstraintData) |
    | skeletonData.findPathConstraint(name)      |→| skeletonData.findConstraint(name, PathConstraintData) |
    | skeletonData.findPhysicsConstraint(name)   |→| skeletonData.findConstraint(name, PhysicsConstraintData) |
  - Renamed setup pose methods:
    ||||
    |-----|-|-----|
    | `Skeleton.setToSetupPose()`       |→| `Skeleton.setupPose()` |
    | `Skeleton.setBonesToSetupPose()`  |→| `Skeleton.setupPoseBones()` |
    | `Skeleton.setSlotsToSetupPose()`  |→| `Skeleton.setupPoseSlots()` |
    | Bone.setToSetupPose()             |→| Bone.setupPose() |
    | Slot.setToSetupPose()             |→| Slot.setupPose() |
    | IkConstraint.setToSetupPose()     |→| IkConstraint.setupPose() |
  - `Physics` enum moved from nested `Skeleton.Physics` to standalone `Physics` class
    - `updateWorldTransform(Skeleton.Physics.update)` → `updateWorldTransform(Physics.update)`
  - Timeline `apply()` methods now take `fromSetup`, `add`, `out`, and `appliedPose` parameters instead of `MixBlend` and `MixDirection`
  - Removed `MixBlend` and `MixDirection`
  - Removed `TrackEntry.holdPrevious` and internal interrupt alpha state. New `AnimationState` hold system automatically calculates the required state values
  - Removed `TrackEntry.mixBlend`. Use `TrackEntry.additive` for additive blending
  - `AnimationState.setCurrent()` renamed to `AnimationState.setTrack()`; `AnimationState.getCurrent()` is deprecated in favor of `AnimationState.getTrack()`
  - Attachment `computeWorldVertices()` methods now take an additional `skeleton` parameter
  - `MeshAttachment.getParentMesh()` / `setParentMesh()` renamed to `getSourceMesh()` / `setSourceMesh()`
  - `RegionAttachment` and `MeshAttachment` now take a non-null `Sequence` in their constructors and use the new sequence attachment model
  - `SkinEntry.name` renamed to `placeholderName` to better match Spine editor terminology
  - `AttachmentLoader` methods now receive both the skin `placeholder` and resolved attachment `name`.
  - `IkConstraintData.uniform` replaced by `IkConstraintData.scaleY`. `IkConstraint.apply()` methods now take `ScaleY` instead of a boolean `uniform` parameter
  - `IkConstraintData.scaleY` and `ScaleY` renamed to `scaleYMode` and `ScaleYMode`.
  - Renamed timeline constraint index methods to use unified `getConstraintIndex()`

### spine-haxe-starling

- **Additions**
  - BoundsProvider Integration
    - Integrated BoundsProvider system into Starling renderer
    - Added `boundsProvider` public field for customizing bounds calculation strategy
    - Added `calculateBounds()` method to recalculate bounds on demand
    - Constructor now accepts optional third parameter `boundsProvider` (defaults to `SetupPoseBoundsProvider`)
    - Simplified `getBounds()` implementation to use `BoundsProvider` instead of direct calculation
  - Added physics position and rotation inheritance settings.
  - Scale Integration
    - Connected `SkeletonSprite.scale`, `scaleX`, and `scaleY` properties to `skeleton.scaleX/scaleY` values
    - Setting scale properties now automatically updates skeleton scale and recalculates bounds
    - Ensures consistent scaling behavior between display object and skeleton

- **Breaking changes**
  - Removed `getAnimationBounds()` method - replace with appropriate `BoundsProvider` implementation or create custom one
  - `hitTest()` now uses `BoundsProvider` using cached bounds from `BoundsProvider` instead of iterating all slots and attachments, for accurate hit testing with animated skeletons, use `CurrentPoseBoundsProvider` and call `calculateBounds()` each frame or on click
  - Changed `_state` to state (public field)
  - Changed `_skeleton` to skeleton (public field)

### spine-haxe-flixel

- **Additions**
  - BoundsProvider Integration
    - Integrated `BoundsProvider` system matching Starling implementation
    - Constructor now accepts optional third parameter `boundsProvider` (defaults to `SetupPoseBoundsProvider`)
    - Added `boundsProvider` public field for customizing bounds calculation strategy
    - Added `calculateBounds()` method to recalculate bounds on demand
    - Added `bounds` property to get the bounds coordinates
  - Added physics position and rotation inheritance settings.
- **Breaking changes**
  - `SkeletonSprite` now extends `FlxTypedGroup<FlxObject>` instead of FlxObject. This was necessary because `FlxObject` bounding/hitbox is always connected to its position and size and cannot be offset
    - This eables proper bounds handling independent of position
    - Added methods and properties to maintain FlxObject-like API despite extending FlxTypedGroup
  - Removed `getAnimationBounds()` method - replace with appropriate `BoundsProvider` implementation
  - Removed `setBoundingBox()` method - use `BoundsProvider` features instead
