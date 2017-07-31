# 3.6

## AS3
 * **Breaking changes**
  * Removed `Bone.worldToLocalRotationX` and `Bone.worldToLocalRotationY`. Replaced by `Bone.worldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * Made `Bone` fields `_a`, `_b`, `_c`, `_d`, `_worldX` and `_worldY` public, removed underscore prefix.
  * Removed `VertexAttachment.computeWorldVertices` overload, changed `VertexAttachment.computeWorldVertices2` to `VertexAttachment.computeWorldVertices`, added `stride` parameter.
  * Removed `RegionAttachment.vertices` field. The vertices array is provided to `RegionAttachment.computeWorldVertices` by the API user now.
  * Removed `RegionAttachment.updateWorldVertices`, added `RegionAttachment.computeWorldVertices`. The new method now computes the x/y positions of the 4 vertices of the corner and places them in the provided `worldVertices` array, starting at `offset`, then moving by `stride` array elements when advancing to the next vertex. This allows to directly compose the vertex buffer and avoids a copy. The computation of the full vertices, including vertex colors and texture coordinates, is now done by the backend's respective renderer.
  * Replaced `r`, `g`, `b`, `a` fields with instances of new `Color` class in `RegionAttachment`, `MeshAttachment`, `Skeleton`, `SkeletonData`, `Slot` and `SlotData`.
 * **Additions**
  * Added `Skeleton.getBounds` from reference implementation.
  * Added support for local and relative transform constraint calculation, including additional fields in `TransformConstraintData`
  * Added `Bone.localToWorldRotation`(rotation given relative to x-axis, counter-clockwise, in degrees).    
  * Added two color tinting support, including `TwoColorTimeline` and additional fields on `Slot` and `SlotData`.
  * Added `PointAttachment`, additional method `newPointAttachment` in `AttachmentLoader` interface.
  * Added `ClippingAttachment`, additional method `newClippingAttachment` in `AttachmentLoader` interface.
  * `AnimationState#apply` returns boolean indicating if any timeline was applied or not.
  * `Animation#apply` and `Timeline#apply`` now take enums `MixPose` and `MixDirection` instead of booleans
  * Added `VertexEffect` and implementations `JitterEffect` and `SwirlEffect`. Allows you to modify vertices before they are submitted for drawing. See Starling changes.

### Starling
 * Fixed renderer to work with 3.6 changes.
 * Added support for two color tinting.
 * Added support for clipping.
 * Added support for rotated regions in texture atlas loaded via StarlingAtlasAttachmentLoader.
 * Added support for vertex effects. See `RaptorExample.as`

## C
 * **Breaking changes**
  * `spVertexAttachment_computeWorldVertices` and `spRegionAttachment_computeWorldVerticeS` now take new parameters to make it possible to directly output the calculated vertex positions to a vertex buffer. Removes the need for additional copies in the backends' respective renderers.
  * Removed `spBoundingBoxAttachment_computeWorldVertices`, superseded by `spVertexAttachment_computeWorldVertices`.
  * Removed `spPathAttachment_computeWorldVertices` and `spPathAttachment_computeWorldVertices1`, superseded by `spVertexAttachment_computeWorldVertices`.
  * Removed `sp_MeshAttachment_computeWorldVertices`, superseded by `spVertexAttachment_computeWorldVertices`.
  * Removed `spBone_worldToLocalRotationX` and `spBone_worldToLocalRotationY`. Replaced by `spBone_worldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * Replaced `r`, `g`, `b`, `a` fields with instances of new `spColor` struct in `spRegionAttachment`, `spMeshAttachment`, `spSkeleton`, `spSkeletonData`, `spSlot` and `spSlotData`.
  * Removed `spVertexIndex`from public API.
 * **Additions**
  * Added support for local and relative transform constraint calculation, including additional fields in `spTransformConstraintData`.  
  * Added `spPointAttachment`, additional method `spAtlasAttachmentLoadeR_newPointAttachment`.
  * Added support for local and relative transform constraint calculation, including additional fields in `TransformConstraintData`
  * Added `spBone_localToWorldRotation`(rotation given relative to x-axis, counter-clockwise, in degrees).  	
   * Added two color tinting support, including `spTwoColorTimeline` and additional fields on `spSlot` and `spSlotData`.
  * Added `userData` field to `spTrackEntry`, so users can expose data in `spAnimationState` callbacks.
  * Modified kvec.h used by SkeletonBinary.c to use Spine's MALLOC/FREE macros. That way there's only one place to inject custom allocators ([extension.h](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-c/spine-c/include/spine/extension.h)) [commit](https://github.com/EsotericSoftware/spine-runtimes/commit/c2cfbc6cb8709daa082726222d558188d75a004f)
  * Added macros to define typed dynamic arrays, see `Array.h/.c`
  * Added `spClippingAttachment` and respective enum.
  * Added `spSkeletonClipper` and `spTriangulator`, used to implement software clipping of attachments.
  * `AnimationState#apply` returns boolean indicating if any timeline was applied or not.
  * `Animation#apply` and `Timeline#apply`` now take enums `MixPose` and `MixDirection` instead of booleans
  * Added `spVertexEffect` and corresponding implementations `spJitterVertexEffect` and `spSwirlVertexEffect`. Create/dispose through the corresponding `spXXXVertexEffect_create()/dispose()` functions. Set on framework/engine specific renderer. See changes for spine-c based frameworks/engines below.
  * Functions in `extension.h` are not prefixed with `_sp` instead of just `_` to avoid interference with other libraries.

### Cocos2d-X
 * Fixed renderer to work with 3.6 changes
 * Optimized rendering by removing all per-frame allocation in `SkeletonRenderer`, resulting in 15% performance increase for large numbers of skeletons being rendered per frame.
 * Added support for two color tinting. Tinting is enabled/disabled per `SkeletonRenderer`/`SkeletonAnimation` instance. Use `SkeletonRenderer::setTwoColorTint()`. Note that two color tinting requires the use of a non-standard shader and vertex format. This means that skeletons rendered with two color tinting will break batching. However, skeletons with two color tinting enabled and rendered after each other will be batched.
 * Updated example to use Cocos2d-x 3.14.1.
 * Added mesh debug rendering. Enable/Disable via `SkeletonRenderer::setDebugMeshesEnabled()`.
 * Added support for clipping.
 * SkeletonRenderer now combines the displayed color of the Node (cascaded from all parents) with the skeleton color for tinting.
 * Added support for vertex effects. See `RaptorExample.cpp`.

### Cocos2d-Objc
 * Fixed renderer to work with 3.6 changes
 * Added support for two color tinting. Tinting is enabled/disabled per `SkeletonRenderer/SkeletonAnimation.twoColorTint = true`. Note that two color tinted skeletons do not batch with other nodes.
 * Added support for clipping.

### SFML
 * Fixed renderer to work with 3.6 changes. Sadly, two color tinting does not work, as the vertex format in SFML is fixed.
 * Added support for clipping.
 * Added support for vertex effects. See raptor example.

### Unreal Engine 4
 * Fixed renderer to work with 3.6 changes
 * Added new UPROPERTY to SpineSkeletonRendererComponent called `Color`. This allows to set the tint color of the skeleton in the editor, C++ and Blueprints. Under the hood, the `spSkeleton->color` will be set on every tick of the renderer component.
 * Added support for clipping.
 * Switched from built-in ProceduralMeshComponent to RuntimeMeshComponent by Koderz (https://github.com/Koderz/UE4RuntimeMeshComponent, MIT). Needed for more flexibility regarding vertex format, should not have an impact on existing code/assets. You need to copy the RuntimeMeshComponentPlugin from our repository in `spine-ue4\Plugins\` to your project as well!
 * Added support for two color tinting. All base materials, e.g. SpineUnlitNormalMaterial, now do proper two color tinting. No material parameters have changed.
 * Updated to Unreal Engine 4.16.1. Note that 4.16 has a regression which will make it impossible to compile plain .c files!

## C#
* **Breaking changes**
  *  `MeshAttachment.parentMesh` is now a private field to enforce using the `.ParentMesh` setter property in external code. The `MeshAttachment.ParentMesh` property is an appropriate replacement wherever `.parentMesh` was used.
  * `Skeleton.GetBounds` takes a scratch array as input so it doesn't have to allocate a new array on each invocation itself. Reduces GC activity.
  * Removed `Bone.WorldToLocalRotationX` and `Bone.WorldToLocalRotationY`. Replaced by `Bone.WorldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * Added `stride` parameter to `VertexAttachment.ComputeWorldVertices`.
  * Removed `RegionAttachment.Vertices` field. The vertices array is provided to `RegionAttachment.ComputeWorldVertices` by the API user now.
  * Removed `RegionAttachment.UpdateWorldVertices`, added `RegionAttachment.ComputeWorldVertices`. The new method now computes the x/y positions of the 4 vertices of the corner and places them in the provided `worldVertices` array, starting at `offset`, then moving by `stride` array elements when advancing to the next vertex. This allows to directly compose the vertex buffer and avoids a copy. The computation of the full vertices, including vertex colors and texture coordinates, is now done by the backend's respective renderer.
 * **Additions**  
  * Added support for local and relative transform constraint calculation, including additional fields in `TransformConstraintData`
  * Added `Bone.localToWorldRotation`(rotation given relative to x-axis, counter-clockwise, in degrees).  
  * Added two color tinting support, including `TwoColorTimeline` and additional fields on `Slot` and `SlotData`.  
  * Added `PointAttachment`, additional method `NewPointAttachment` in `AttachmentLoader` interface.
  * Added `ClippingAttachment`, additional method `NewClippingAttachment` in `AttachmentLoader` interface.
  * Added `SkeletonClipper` and `Triangulator`, used to implement software clipping of attachments.
  * `AnimationState.Apply` returns a bool indicating if any timeline was applied or not.
  * `Animation.Apply` and `Timeline.Apply`` now take enums `MixPose` and `MixDirection` instead of bools.

### Unity
 * Refactored renderer to work with new 3.6 features.
   * **Two color tinting** is currently supported via extra UV2 and UV3 mesh vertex streams. To use Two color tinting, you need to:
     * switch on "Tint Black" under "Advanced...",
     * use the new `Spine/Skeleton Tint Black` shader, or your own shader that treats the UV2 and UV3 streams similarly.
     * Additionally, for SkeletonGraphic, you can use `Spine/SkeletonGraphic Tint Black` (or the bundled SkeletonGraphicTintBlack material) or your own shader that uses UV2 and UV3 streams similarly. **Additional Shader Channels** TexCoord1 and TexCoord2 will need to be enabled from the Canvas component's inspector. These correspond to UV2 and UV3. 
   * **Clipping** is now supported. Caution: The SkeletonAnimation switches to slightly slower mesh generation code when clipping so limit your use of `ClippingAttachment`s when using on large numbers of skeletons.
 * **SkeletonRenderer.initialFlip** Spine components such as SkeletonRenderer, SkeletonAnimation, SkeletonAnimator now has `initialFlipX` and `initialFlipY` fields which are also visible in the inspector under "Advanced...". It will allow you to set and preview a starting flip value for your skeleton component. This is applied immediately when the internal skeleton object is instantiated. 
 * **[SpineAttribute] Improvements** 
   * **Icons have been added to SpineAttributeDrawers**. This should make your default inspectors easier to understand at a glance.
   * **Added Constraint Attributes** You can now use `[SpineIkConstraint]` `[SpineTransformConstraint]` `[SpinePathConstraint]`
   * **SpineAttribute dataField** parameter can also now detect sibling fields within arrays and serializable structs/classes.
   * **[SpineAttribute(includeNone:false)]** SpineAttributes now have an `includeNone` optional parameter to specify if you want to include or exclude a none ("") value option in the dropdown menu. Default is `includeNone:true`.
   * **[SpineAttachment(skinField:"mySkin")]** The SpineAttachment attribute now has a skinField optional parameter to limit the dropdown items to attachments in a specific skin instead of the just default skin or all the skins in SkeletonData.
 * **SkeletonDebugWindow**. Debugging tools have been moved from the SkeletonAnimation and SkeletonUtility component inspectors into its own utility window. You can access "Skeleton Debug" under the `Advanced...` foldout in the SkeletonAnimation inspector, or in SkeletonAnimation's right-click/context menu.
   * **Skeleton Baking Window** The old Skeleton Baking feature is also now accessible through the SkeletonDataAsset's right-click/context menu.
 * **AttachmentTools source material**. `AttachmentTools` methods can now accept a `sourceMaterial` argument to copy material properties from.
 * **AttachmentTools Skin Extensions**. Using AttachmentTools, you can now add entries by slot name by also providing a skeleton argument. Also `Append(Skin)`, `RemoveAttachment` and `Clear` have been added.
 * **BoneFollower and SkeletonUtilityBone Add RigidBody Button**. The BoneFollower and SkeletonUtilityBone component inspectors will now offer to add a `Rigidbody` or `Rigidbody2D` if it detects a collider of the appropriate type. Having a rigidbody on a moving transform with a collider fits better with the Unity physics systems and prevents excess calculations. It will not detect colliders on child objects so you have to add Rigidbody components manually accordingly.
 * **SkeletonRenderer.OnPostProcessVertices** is a new callback that gives you a reference to the MeshGenerator after it has generated a mesh from the current skeleton pose. You can access `meshGenerator.VertexBuffer` or `meshGenerator.ColorBuffer` to modify these before they get pushed into the UnityEngine.Mesh for rendering. This can be useful for non-shader vertex effects.
 * **Examples**
   * **Examples now use properties**. The code in the example scripts have been switched over to using properties instead of fields to encourage their use for consistency. This is in anticipation of both users who want to move the Spine folders to the Unity Plugins folder (compiled as a different assembly), and of Unity 2017's ability to manually define different assemblies for shorter compilation times.
   * **Mix And Match**. The mix-and-match example scene, code and data have been updated to reflect the current recommended setup for animation-compatible custom equip systems The underlying API has changed since 3.5 and the new API calls in MixAndMatch.cs is recommended. Documentation is in progress.  
   * **Sample Components**. `AtasRegionAttacher` and `SpriteAttacher` are now part of `Sample Components`, to reflect that they are meant to be used as sample code rather than production. A few other sample components have also been added. New imports of the unitypackage Examples folder will see a "Legacy" folder comprised of old sample components that no longer contain the most up-to-date and recommended workflows, but are kept in case old setups used them for production.
 * **Spine folder**. In the unitypackage, the "spine-csharp" and "spine-unity" folders are now inside a "Spine" folder. This change will only affect fresh imports. Importing the unitypackage to update Spine-Unity in your existing project will update the appropriate files however you chose to arrange them, as long as the meta files are intact.
 * **Breaking changes**
   * The Sprite shaders module was updated to the latest version from the [source](https://github.com/traggett/UnitySpriteShaders/commits/master). Some changes were made to the underlying keyword structure. You may need to review the settings of your lit materials. Particularly, your Fixed Normals settings.
   * The `Spine/Skeleton Lit` shader was switched over to non-fixed-function code. It now no longer requires mesh normals and has fixed normals at the shader level. 
   * The old MeshGenerator classes, interfaces and code in `Spine.Unity.MeshGeneration` are now deprecated. All mesh-generating components now share the class `Spine.Unity.MeshGenerator` defined in `SpineMesh.cs`. MeshGenerator is a serializable class.
     * The `SkeletonRenderer.renderMeshes` optimization is currently non-functional.
     * Old triangle-winding code has been removed from `SkeletonRenderer`. Please use shaders that have backface culling off.
     * Render settings in `SkeletonGraphic` can now be accessed under `SkeletonGraphic.MeshGenerator.settings`. This is visible in the SkeletonGraphic inspector as `Advanced...`
     * We will continue to bundle the unitypackage with the empty .cs files of deprecated classes until Spine 3.7 to ensure the upgrade process does not break.
   * The [SpineAttachment(slotField:)] optional parameter found property value now acts as a Find(slotName) argument rather than Contains(slotName).
   * `SkeletonAnimator` now uses a `SkeletonAnimator.MecanimTranslator` class to translate an Animator's Mecanim State Machine into skeleton poses. This makes code reuse possible for a Mecanim version of SkeletonGraphic.
   * `SkeletonAnimator` `autoreset` and the `mixModes` array are now a part of SkeletonAnimator's MecanimTranslator `.Translator`. `autoReset` is set to true by default. Old prefabs and scene objects with Skeleton Animator may no longer have correct values set.
   * Warnings and conditionals checking for specific Unity 5.2-and-below incompatibility have been removed.

## XNA/MonoGame
 * Added support for clipping
 * Removed `RegionBatcher` and `SkeletonRegionRenderer`, renamed `SkeletonMeshRenderer` to `SkeletonRenderer`
 * Added support for two color tint. For it to work, you need to add the `SpineEffect.fx` file to your content project, then load it via `var effect = Content.Load<Effect>("SpineEffect");`, and set it on the `SkeletonRenderer`. See the example project for code.
 * Added support for any `Effect` to be used by `SkeletonRenderer`
 * Added `SkeletonDebugRenderer`

## Java
 * **Breaking changes**
  * `Skeleton.getBounds` takes a scratch array as input so it doesn't have to allocate a new array on each invocation itself. Reduces GC activity.
  * Removed `Bone.worldToLocalRotationX` and `Bone.worldToLocalRotationY`. Replaced by `Bone.worldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * Added `stride` parameter to `VertexAttachment.computeWorldVertices`.
  * Removed `RegionAttachment.vertices` field. The vertices array is provided to `RegionAttachment.computeWorldVertices` by the API user now.
  * Removed `RegionAttachment.updateWorldVertices`, added `RegionAttachment.computeWorldVertices`. The new method now computes the x/y positions of the 4 vertices of the corner and places them in the provided `worldVertices` array, starting at `offset`, then moving by `stride` array elements when advancing to the next vertex. This allows to directly compose the vertex buffer and avoids a copy. The computation of the full vertices, including vertex colors and texture coordinates, is now done by the backend's respective renderer.
 * **Additions**  
  * Added support for local and relative transform constraint calculation, including additional fields in `TransformConstraintData`
  * Added `Bone.localToWorldRotation`(rotation given relative to x-axis, counter-clockwise, in degrees).  
  * Added two color tinting support, including `TwoColorTimeline` and additional fields on `Slot` and `SlotData`.  
  * Added `PointAttachment`, additional method `newPointAttachment` in `AttachmentLoader` interface.
  * Added `ClippingAttachment`, additional method `newClippingAttachment` in `AttachmentLoader` interface.
  * Added `SkeletonClipper` and `Triangulator`, used to implement software clipping of attachments.
  * `AnimationState#apply` returns boolean indicating if any timeline was applied or not.
  * `Animation#apply` and `Timeline#apply`` now take enums `MixPose` and `MixDirection` instead of booleans

### libGDX
 * Fixed renderer to work with 3.6 changes
 * Added support for two color tinting. Use the new `TwoColorPolygonBatch` together with `SkeletonRenderer`
 * Added support for clipping. See `SkeletonClipper`. Used automatically by `SkeletonRenderer`. Does not work when using a `SpriteBatch` with `SkeletonRenderer`. Use `PolygonSpriteBatch` or `TwoColorPolygonBatch` instead.
 * Added `VertexEffect` interface, instances of which can be set on `SkeletonRenderer`. Allows to modify vertices before submitting them to GPU. See `SwirlEffect`, `JitterEffect` and `VertexEffectTest`.

## Lua
 * **Breaking changes**
  * Removed `Bone:worldToLocalRotationX` and `Bone:worldToLocalRotationY`. Replaced by `Bone:worldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * `VertexAttachment:computeWorldVertices` now takes offsets and stride to allow compositing vertices directly in a vertex buffer to be send to the GPU. The compositing is now performed in the backends' respective renderers. This also affects the subclasses `MeshAttachment`, `BoundingBoxAttachment` and `PathAttachment`.
  * Removed `RegionAttachment:updateWorldVertices`, added `RegionAttachment:computeWorldVertices`, which takes offsets and stride to allow compositing vertices directly in a vertex buffer to be send to the GPU. The compositing is now performed in the backends' respective renderers.
  * Removed `MeshAttachment.worldVertices` field. Computation is now performed in each backends' respective renderer. The `uv` coordinates are now stored in `MeshAttachment.uvs`.
  * Removed `RegionAttachment.vertices` field. Computation is now performed in each backends respective renderer. The `uv` coordinates for each vertex are now stored in the `RegionAttachment.uvs` field.
 * **Additions**
  * Added `Bone:localToWorldRotation`(rotation given relative to x-axis, counter-clockwise, in degrees).
  * Added two color tinting support, including `TwoColorTimeline` and additional fields on `Slot` and `SlotData`.
  * Added `PointAttachment`, additional method `newPointAttachment` in `AttachmentLoader` interface.  
  * Added support for local and relative transform constraint calculation, including additional fields in `TransformConstraintData`
  * Added `ClippingAttachment`, additional method `newClippingAttachment` in `AttachmentLoader` interface.
  * Added `SkeletonClipper` and `Triangulator`, used to implement software clipping of attachments.
  * `AnimationState#apply` returns boolean indicating if any timeline was applied or not.
  * `Animation#apply` and `Timeline#apply`` now take enums `MixPose` and `MixDirection` instead of booleans
  * Added `JitterEffect` and `SwirlEffect` and support for vertex effects in Corona and Love

### Love2D
  * Fixed renderer to work with 3.6 changes
  * Added support for two color tinting. Enable it via `SkeletonRenderer.new(true)`.
  * Added clipping support.
  * Added support for vertex effects. Set an implementation like "JitterEffect" on `Skeleton.vertexEffect`. See `main.lua` for an example.

### Corona
  * Fixed renderer to work with 3.6 changes. Sadly, two color tinting is not supported, as Corona doesn't let us change the vertex format needed and its doesn't allow to modify shaders in the way needed for two color tinting
  * Added clipping support.
  * Added support for vertex effects. Set an implementation like "JitterEffect" on `SkeletonRenderer.vertexEffect`. See `main.lua` for an example

## Typescript/Javascript
 * **Breaking changes**
  * `Skeleton.getBounds` takes a scratch array as input so it doesn't have to allocate a new array on each invocation itself. Reduces GC activity.
  * Removed `Bone.worldToLocalRotationX` and `Bone.worldToLocalRotationY`. Replaced by `Bone.worldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * Removed `VertexAttachment.computeWorldVertices` overload, changed `VertexAttachment.computeWorldVerticesWith` to `VertexAttachment.computeWorldVertices`, added `stride` parameter.
  * Removed `RegionAttachment.vertices` field. The vertices array is provided to `RegionAttachment.computeWorldVertices` by the API user now.
  * Removed `RegionAttachment.updateWorldVertices`, added `RegionAttachment.computeWorldVertices`. The new method now computes the x/y positions of the 4 vertices of the corner and places them in the provided `worldVertices` array, starting at `offset`, then moving by `stride` array elements when advancing to the next vertex. This allows to directly compose the vertex buffer and avoids a copy. The computation of the full vertices, including vertex colors and texture coordinates, is now done by the backend's respective renderer.
 * **Additions**  
  * Added support for local and relative transform constraint calculation, including additional fields in `TransformConstraintData`
  * Added `Bone.localToWorldRotation`(rotation given relative to x-axis, counter-clockwise, in degrees).  
  * Added two color tinting support, including `TwoColorTimeline` and additional fields on `Slot` and `SlotData`.  
  * Added `PointAttachment`, additional method `newPointAttachment` in `AttachmentLoader` interface.
  * Added `ClippingAttachment`, additional method `newClippingAttachment` in `AttachmentLoader` interface.
  * Added `SkeletonClipper` and `Triangulator`, used to implement software clipping of attachments.
  * `AnimationState#apply` returns boolean indicating if any timeline was applied or not.
  * `Animation#apply` and `Timeline#apply`` now take enums `MixPose` and `MixDirection` instead of booleans


### WebGL backend
 * Fixed WebGL context loss
   * Added `Restorable` interface, implemented by any WebGL resource that needs restoration after a context loss. All WebGL resource classes (`Shader`, `Mesh`, `GLTexture`) implement this interface.
   * Added `ManagedWebGLRenderingContext`. Handles setup of a `WebGLRenderingContext` given a canvas element and restoration of WebGL resources (`Shader`, `Mesh`, `GLTexture`) on WebGL context loss. WebGL resources register themselves with the `ManagedWebGLRenderingContext`. If the context is informed of a context loss and restoration, the registered WebGL resources' `restore()` method is called. The `restore()` method implementation on each resource type will recreate the GPU side objects.
   * All classes that previously took a `WebGLRenderingContext` in the constructor now also allow a `ManagedWebGLRenderingContext`. This ensures existing applications do not break.
   * To use automatic context restauration:
    1. Create or fetch a canvas element from the DOM
    2. Instantiate a `ManagedWebGLRenderingContext`, passing the canvas to the constructor. This will setup a `WebGLRenderingContext` internally and manage context loss/restoration.
    3. Pass the `ManagedWebGLRenderingContext` to the constructors of classes that you previously passed a `WebGLRenderingContext` to (`AssetManager`, `GLTexture`, `Mesh`, `Shader`, `PolygonBatcher`, `SceneRenderer`, `ShapeRenderer`, `SkeletonRenderer`, `SkeletonDebugRenderer`).
 * Fixed renderer to work with 3.6 changes.
 * Added support for two color tinting.
 * Improved performance by using `DYNAMIC_DRAW` for vertex buffer objects and fixing bug that copied to much data to the GPU each frame in `PolygonBatcher`/`Mesh`.
 * Added two color tinting support, enabled by default. You can disable it via the constructors of `SceneRenderer`, `SkeletonRenderer`and `PolygonBatcher`. Note that you will need to use a shader created via `Shader.newTwoColoredTexturedShader` shader with `SkeletonRenderer` and `PolygonBatcher` if two color tinting is enabled.
 * Added clipping support
 * Added `VertexEffect` interface, instances of which can be set on `SkeletonRenderer`. Allows to modify vertices before submitting them to GPU. See `SwirlEffect`, `JitterEffect`, and the example which allows to set effects.

### Canvas backend
 * Fixed renderer to work for 3.6 changes. Sadly, we can't support two color tinting via the Canvas API.
 * Added support for shearing and non-uniform scaling inherited from parent bones.
 * Added support for alpha tinting.

### Three.js backend
 * Fixed renderer to work with 3.6 changes. Two color tinting is not supported.
 * Added clipping support
 * Added `VertexEffect` interface, instances of which can be set on `SkeletonMesh`. Allows to modify vertices before submitting them to GPU. See `SwirlEffect`, `JitterEffect`.

### Widget backend
 * Fixed WebGL context loss (see WebGL backend changes). Enabled automatically.
 * Fixed renderer to work for 3.6 changes. Supports two color tinting & clipping (see WebGL backend changes for details).
 * Added fields `atlasContent`, `atlasPagesContent`, and `jsonContent` to `WidgetConfiguration` allowing you to directly pass the contents of the `.atlas`, atlas page `.png` files, and the `.json` file without having to do a request. See `README.md` and the example for details.
 * `SpineWidget.setAnimation()` now takes an additional optional parameter for callbacks when animations are completed/interrupted/etc.
