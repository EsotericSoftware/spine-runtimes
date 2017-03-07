# 3.6
* **[lua]**
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
  * **[love]**
    * Fixed renderer to work with 3.6 changes
  * **[corona]**
    * Fixed renderer to work with 3.6 changes. Sadly, two color tinting is not supported, as Corona doesn't let us change the vertex format needed and its doesn't allow to modify shaders in the way needed for two color tinting
* **[c]**
  * **Breaking changes**
    * `spVertexAttachment_computeWorldVertices` and `spRegionAttachment_computeWorldVerticeS` now take new parameters to make it possible to directly output the calculated vertex positions to a vertex buffer. Removes the need for additional copies in the backends' respective renderers.
    * Removed `spBoundingBoxAttachment_computeWorldVertices`, superseded by `spVertexAttachment_computeWorldVertices`.
  	* Removed `spPathAttachment_computeWorldVertices` and `spPathAttachment_computeWorldVertices1`, superseded by `spVertexAttachment_computeWorldVertices`.
  	* Removed `sp_MeshAttachment_computeWorldVertices`, superseded by `spVertexAttachment_computeWorldVertices`.
  	* Removed `spBone_worldToLocalRotationX` and `spBone_worldToLocalRotationY`. Replaced by `spBone_worldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
    * Replaced `r`, `g`, `b`, `a` fields with instances of new `spColor` struct in `spRegionAttachment`, `spMeshAttachment`, `spSkeleton`, `spSkeletonData`, `spSlot` and `spSlotData`.
  * **Additions**
    * Added support for local and relative transform constraint calculation, including additional fields in `spTransformConstraintData`.  
  	* Added `spPointAttachment`, additional method `spAtlasAttachmentLoadeR_newPointAttachment`.
  	* Added support for local and relative transform constraint calculation, including additional fields in `TransformConstraintData`
  	* Added `spBone_localToWorldRotation`(rotation given relative to x-axis, counter-clockwise, in degrees).  	
    * Added two color tinting support, including `spTwoColorTimeline` and additional fields on `spSlot` and `spSlotData`.
    * Added `userData` field to `spTrackEntry`, so users can expose data in `spAnimationState` callbacks.
    * Modified kvec.h used by SkeletonBinary.c to use Spine's MALLOC/FREE macros. That way there's only one place to inject custom allocators ([extension.h](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-c/spine-c/include/spine/extension.h)) [commit](https://github.com/EsotericSoftware/spine-runtimes/commit/c2cfbc6cb8709daa082726222d558188d75a004f)
  * **[cocos2d-x]**
    * Fixed renderer to work with 3.6 changes
    * Optimized rendering by removing all per-frame allocation in `SkeletonRenderer`, resulting in 15% performance increase for large numbers of skeletons being rendered per frame.
    * Added support for two color tinting. Tinting is enabled/disabled per `SkeletonRenderer`/`SkeletonAnimation` instance. Use `SkeletonRenderer::setTwoColorTint()`. Note that two color tinting requires the use of a non-standard shader and vertex format. This means that skeletons rendered with two color tinting will break batching. However, skeletons with two color tinting enabled and rendered after each other will be batched.
    * Updated example to use Cocos2d-x 3.14.1.
  * **[cocos2d-objc]**
    * Fixed renderer to work with 3.6 changes
  * **[sfml]**
  	* Fixed renderer to work with 3.6 changes. Sadly, two color tinting does not work, as the vertex format in SFML is fixed.
  * **[ue4]**
    * Fixed renderer to work with 3.6 changes
* **[as3]**
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
  * **[starling]**
    * Fixed renderer to work with 3.6 changes.
* **[ts]**
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
  * **[webgl backend]**
    * Fixed renderer to work with 3.6 changes.
    * Added support for two color tinting.
    * Improved performance by using `DYNAMIC_DRAW` for vertex buffer objects and fixing bug that copied to much data to the GPU each frame in `PolygonBatcher`/`Mesh`.
    * Added two color tinting support, enabled by default. You can disable it via the constructors of `SceneRenderer`, `SkeletonRenderer`and `PolygonBatcher`. Note that you will need to use a shader created via `Shader.newTwoColoredTexturedShader` shader with `SkeletonRenderer` and `PolygonBatcher` if two color tinting is enabled.
  * **[canvas backend]**
    * Fixed renderer to work for 3.6 changes. Sadly, we can't support two color tinting via the Canvas API.
    * Added support for shearing and non-uniform scaling inherited from parent bones.
    * Added support for alpha tinting.
  * **[threejs backend]**
    * Fixed renderer to work with 3.6 changes. Two color tinting is not supported.
  * **[widget backend]**
    * Fixed renderer to work for 3.6 changes. Supports two color tinting (see webgl backend changes for details).
    * Added fields `atlasContent` and `jsonContent` to `WidgetConfiguration` allowing you to directly pass the contents of the `.atlas` and `.json` file without having to do a request. See `README.md` and the example for details.
