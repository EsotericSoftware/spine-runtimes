# 3.7

## AS3
* **Breaking changes**
  * The completion event will fire for looped 0 duration animations every frame.
  * `MixPose` is now called `MixBlend`
  * Skeleton `flipX/flipY` has been replaced with `scaleY/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
  * Mix time is no longer affected by `TrackEntry#timeScale`. See https://github.com/EsotericSoftware/spine-runtimes/issues/1194
* **Additions**
  * Added additive animation blending. When playing back multiple animations on different tracks, where each animation modifies the same skeleton property, the results of tracks with lower indices are discarded, and only the result from the track with the highest index is used. With animation blending, the results of all tracks are mixed together. This allows effects like mixing multiple facial expressions (angry, happy, sad) with percentage mixes. By default the old behaviour is retained (results from lower tracks are discarded). To enable additive blending across animation tracks, call `TrackEntry#setMixBlend(MixBlend.add)` on each track. To specify the blend percentage, set `TrackEntry#alpha`. See http://esotericsoftware.com/forum/morph-target-track-animation-mix-mode-9459 for a discussion.
  * Support for stretchy IK
  * Support for audio events, see `audioPath`, `volume` and `balance` fields on event (data).
  * `TrackEntry` has an additional field called `holdPrevious`. It can be used to counter act a limitation of `AnimationState` resulting in "dipping" of parts of the animation. For a full discussion of the problem and the solution we've implemented, see this [forum thread](http://esotericsoftware.com/forum/Probably-Easy-Animation-mixing-with-multiple-tracks-10682?p=48130&hilit=holdprevious#p48130).

### Starling
* Added support for vertex effects. See `RaptorExample.as`
* Added 'getTexture()' method to 'StarlingTextureAtlasAttachmentLoader'
* Breaking change: if a skeleton requires two color tinting, you have to enable it via `SkeletonSprite.twoColorTint = true`. In this case the skeleton will use the `TwoColorMeshStyle`, which internally uses a different vertex layout and shader. This means that skeletons with two color tinting enabled will break batching and hence increase the number of draw calls in your app.
* Added `VertexEffect` and implementations `JitterEffect` and `SwirlEffect`. Allows you to modify vertices before they are submitted for drawing. See Starling changes.
* Fix issues with StarlingAtlasAttachmentLoader, see https://github.com/EsotericSoftware/spine-runtimes/issues/939
* Fix issues with region trimming support, see https://github.com/EsotericSoftware/spine-runtimes/commit/262bc26c64d4111002d80e201cb1a3345e6727df
* Added support for overriding `StarlingAtlasAttachmentLoader#getTexture()`, see https://github.com/EsotericSoftware/spine-runtimes/commit/ea7dbecb98edc74e439aa9ef90dcf6eed865f718
* Texture atlas operations are no longer handled in `Starling#newRegionAttachment` and `Starling#newMeshAttachment` but delegated to the atlas.
* Added sample for additive animation blending, see https://github.com/EsotericSoftware/spine-runtimes/blob/6a556de01429878df47bb276a97959a8bdbbe32f/spine-starling/spine-starling-example/src/spine/examples/OwlExample.as
* Added sample on how to use bounding box attachment vertices https://github.com/EsotericSoftware/spine-runtimes/commit/e20428b02699226164fa73ba4b12f7d029ae6f4d
* Fully transparent meshes are not submitted for rendering.
* No hit-tests are performed when a skeleton is invisible.

## C
* **Breaking changes**
  * Listeners on `spAnimationState` and `spTrackEntry` will now also be called if a track entry gets disposed as part of disposing an animation state.
  * The completion event will fire for looped 0 duration animations every frame.
  * The spine-cocos2dx and spine-ue4 runtimes are now based on spine-cpp. See below for changes.
  * Skeleton `flipX/flipY` has been replaced with `scaleY/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
  * Mix time is no longer affected by `TrackEntry#timeScale`. See https://github.com/EsotericSoftware/spine-runtimes/issues/1194
  * `spMeshAttachment` has two new fields `regionTextureWith` and `regionTextureHeight`. These must be set in custom attachment loader. See `AtlasAttachmentLoader`.
* **Additions**
  * Added support for local and relative transform constraint calculation, including additional fields in `spTransformConstraintData`.
  * `Animation#apply` and `Timeline#apply`` now take enums `MixPose` and `MixDirection` instead of booleans
  * Added `spVertexEffect` and corresponding implementations `spJitterVertexEffect` and `spSwirlVertexEffect`. Create/dispose through the corresponding `spXXXVertexEffect_create()/dispose()` functions. Set on framework/engine specific renderer.
  * Functions in `extension.h` are not prefixed with `_sp` instead of just `_` to avoid interference with other libraries.
  * Introduced `SP_API` macro. Every spine-c function is prefixed with this macro. By default, it is an empty string. Can be used to markup spine-c functions with e.g. ``__declspec` when compiling to a dll or linking to that dll.
  * Added `void *userData` to `spAnimationState`to be consumed in callbacks.
  * Added additive animation blending. When playing back multiple animations on different tracks, where each animation modifies the same skeleton property, the results of tracks with lower indices are discarded, and only the result from the track with the highest index is used. With animation blending, the results of all tracks are mixed together. This allows effects like mixing multiple facial expressions (angry, happy, sad) with percentage mixes. By default the old behaviour is retained (results from lower tracks are discarded). To enable additive blending across animation tracks, call `spTrackEntry->mixBlend = SP_MIXBLEND_ADD)` on each track. To specify the blend percentage, set `spTrackEntry->alpha`. See http://esotericsoftware.com/forum/morph-target-track-animation-mix-mode-9459 for a discussion.
  * Optimized attachment lookup to give a 40x speed-up. See https://github.com/EsotericSoftware/spine-runtimes/commit/cab81276263890b65d07fa2329ace16db1e365ff
  * Support for stretchy IK
  * Support for audio events, see `audioPath`, `volume` and `balance` fields on event (data).
  * `spTrackEntry` has an additional field called `holdPrevious`. It can be used to counter act a limitation of `AnimationState` resulting in "dipping" of parts of the animation. For a full discussion of the problem and the solution we've implemented, see this [forum thread](http://esotericsoftware.com/forum/Probably-Easy-Animation-mixing-with-multiple-tracks-10682?p=48130&hilit=holdprevious#p48130).

### Cocos2d-Objc
* Added vertex effect support to modify vertices of skeletons on the CPU. See `RaptorExample.m`.
* Explanation how to handle ARC, see https://github.com/EsotericSoftware/spine-runtimes/commit/a4f122b08c5e2a51d6aad6fc5a947f7ec31f2eb8
* The super class `::update()` method of `SkeletonRenderer` is now called, see https://github.com/EsotericSoftware/spine-runtimes/commit/f7bb98185236a6d8f35bfefc70afe4f31e9ec9d2
* Added improved tint-black shader.

### SFML
* `spine-sfml.h` no longer defines `SPINE_SHORT_NAMES` to avoid collisions with other APIs. See #1058.
* Added support for vertex effects. See raptor example.
* Added premultiplied alpha support to `SkeletonDrawable`. Use `SkeletonDrawable::setUsePremultipliedAlpha()`, see https://github.com/EsotericSoftware/spine-runtimes/commit/34086c1f41415309b2ecce86055f6656fcba2950
* Added additive animation blending sample, see https://github.com/EsotericSoftware/spine-runtimes/blob/b7e712d3ca1d6be3ebcfe3254dc2cea9c44dda71/spine-sfml/example/main.cpp#L369

## C++
* ** Additions **
  * Added C++ Spine runtime. See the [spine-cpp Runtime Guide](https://esotericsoftware.com/spine-cpp) for more information on spine-cpp.
  * Added parsing of non-essential data (fps, images path, audio path) to for `.json`/`.skel` parsers.

### Cocos2d-x
* Added ETC1 alpha support, thanks @halx99! Does not work when two color tint is enabled.
* Added `spAtlasPage_setCustomTextureLoader()` which let's you do texture loading manually. Thanks @jareguo.
* Added `SkeletonRenderer:setSlotsRange()` and `SkeletonRenderer::createWithSkeleton()`. This allows you to split rendering of a skeleton up into multiple parts, and render other nodes in between. See `SkeletonRendererSeparatorExample.cpp` for an example.
* Fully transparent attachments will not be rendered, improving rendering performance.
* Added improved tint-black shader.
* Updated to cocos2d-x 3.16
* The skeleton setup pose and world transform are now calculated on initialization to avoid flickering on start-up.
* Updated to cocos2d-x 3.17.1
* **Breaking change**: Switched from [spine-c](spine-c) to [spine-cpp](spine-cpp) as the underlying Spine runtime. See the [spine-cpp Runtime Guide](https://esotericsoftware.com/spine-cpp) for more information on spine-cpp.
  * Added `Cocos2dAttachmentLoader` to be used when constructing an `Atlas`. Used by default by `SkeletonAnimation` and `SkeletonRenderer` when creating instances via the `createXXX` methods.
  * All C structs and enums `spXXX` have been replaced with their C++ equivalents `spine::XXX` in all public interfaces.
  * All instantiations via `new` of C++ classes from spine-cpp should contain `(__FILE__, __LINE__)`. This allows the tracking of instantations and detection of memory leaks via the `spine::DebugExtension`.

### SFML
* Create a second SFML backend using [spine-cpp](spine-cpp/). See the [spine-cpp Runtime Guide](https://esotericsoftware.com/spine-cpp) for more information on spine-cpp.
* Added support for vertex effects. See raptor example.
* Added premultiplied alpha support to `SkeletonDrawable`. Use `SkeletonDrawable::setUsePremultipliedAlpha()`, see https://github.com/EsotericSoftware/spine-runtimes/commit/34086c1f41415309b2ecce86055f6656fcba2950
* Added additive animation blending sample, see https://github.com/EsotericSoftware/spine-runtimes/blob/b7e712d3ca1d6be3ebcfe3254dc2cea9c44dda71/spine-sfml/example/main.cpp#L369

### UE4
 * spine-c is now exposed from the plugin shared library on Windows via __declspec.
 * Updated to Unreal Engine 4.18
 * Added C++ example, see https://github.com/EsotericSoftware/spine-runtimes/commit/15011e81b7061495dba45e28b4d3f4efb10d7f40
 * `SkeletonRendererComponent` generates collision meshes by default.
 * Disabled generation of collision meshes by `SkeletonRendererComponent`. Both `ProceduralMeshComponent` and `RuntimeMeshComponent` have a bug that generates a new PhysiX file every frame per component. Users are advised to add a separate collision shape to the root scene component of an actor instead.
 * Using UE4 `FMemory` allocator by default. This should fix issues on some consoles.
 * **Breaking change** moved away from `RuntimeMeshComponent`, as its maintainance has seized, back to `ProceduralMeshComponent`. Existing projects should just work. However, if you run into issues, you may have to remove the old `SpineSkeletonRendererComponent` and add a new one to your existing actors.
 * **Breaking change** due to the removal of `RuntimeMeshComponent` and reversal to `ProceduralMeshComponent`, two color tinting is currently not supported. `ProceduralMeshComponent` does not support enough vertex attributes for us to encode the second color in the vertex stream. You can remove the `RuntimeMeshComponent/` directory from your plugins directory and remove the component from any `build.cs` files that may reference it.
 * **Breaking change**: Switched from [spine-c](spine-c) to [spine-cpp](spine-cpp) as the underlying Spine runtime. See the [spine-cpp Runtime Guide](https://esotericsoftware.com/spine-cpp) for more information on spine-cpp.
  * All C structs and enums `spXXX` have been replaced with their C++ equivalents `spine::XXX` in all public interfaces.
  * All instantiations via `new` of C++ classes from spine-cpp should contain `(__FILE__, __LINE__)`. This allows the tracking of instantations and detection of memory leaks via the `spine::DebugExtension`.
* Updated to Unreal Engine 4.20 (samples require 4.17+), see the `spine-ue4/Plugins/SpinePlugin/Source/SpinePlugin/SpinePlugin.build.cs` file on how to compile in 4.20 with the latest UBT API changes.
* Updated to Unreal Engine 4.21 (samples require 4.21).
* **Breaking change**: `UBoneDriverComponent` and `UBoneFollowerComponent` are now `USceneComponent` instead of `UActorComponent`. They either update only themselves, or also the owning `UActor`, depending on whether the new flag `UseComponentTransform` is set. See https://github.com/EsotericSoftware/spine-runtimes/pull/1175
* Added query methods for slots, bones, skins and animations to `SpineSkeletonComponent` and `UTrackEntry`. These allow you to query these objects by name in both C++ and blueprints.
* Added `Preview Animation` and `Preview Skin` properties to `SpineSkeletonAnimationComponent`. Enter an animation or skin name to live-preview it in the editor. Enter an empty string to reset the animation or skin.

## C# ##
* **Breaking changes**
  * The completion event will fire for looped 0 duration animations every frame.
  * Skeleton `flipX/flipY` has been replaced with `scaleY/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
  * Mix time is no longer affected by `TrackEntry#timeScale`. See https://github.com/EsotericSoftware/spine-runtimes/issues/1194
* **Additions**
  * Added additive animation blending. When playing back multiple animations on different tracks, where each animation modifies the same skeleton property, the results of tracks with lower indices are discarded, and only the result from the track with the highest index is used. With animation blending, the results of all tracks are mixed together. This allows effects like mixing multiple facial expressions (angry, happy, sad) with percentage mixes. By default the old behaviour is retained (results from lower tracks are discarded). To enable additive blending across animation tracks, call `TrackEntry#MixBlend = MixBlend.add` on each track. To specify the blend percentage, set `TrackEntry#Alpha`. See http://esotericsoftware.com/forum/morph-target-track-animation-mix-mode-9459 for a discussion.
  * Support for stretchy IK
  * Support for audio events, see `audioPath`, `volume` and `balance` fields on event (data).
  * `TrackEntry` has an additional field called `holdPrevious`. It can be used to counter act a limitation of `AnimationState` resulting in "dipping" of parts of the animation. For a full discussion of the problem and the solution we've implemented, see this [forum thread](http://esotericsoftware.com/forum/Probably-Easy-Animation-mixing-with-multiple-tracks-10682?p=48130&hilit=holdprevious#p48130).

### Unity
* **Runtime and Editor, and Assembly Definition** Files and folders have been reorganized into "Runtime" and "Editor". Each of these have an `.asmdef` file that defines these separately as their own assembly in Unity *(Note: Spine `.asmdef` files are currently deactivated to `.txt` extension, see below)*. For projects not using assembly definition, you may delete the `.asmdef` files. These assembly definitions will be ignored by older versions of Unity that don't support it.
	* In this scheme, the entirety of the base spine-csharp runtime is inside the "Runtime" folder, to be compiled in the same assembly as spine-unity so they can continue to share internal members.
* **Spine `.asmdef` files are now deactivated (using `.txt` extension) by default** This prevents problems when updating Spine through unitypackages, overwriting the Timeline reference entry in `spine-unity.asmdef` (added automatically when enabling Unity 2019 Timeline support, see `Timeline Support for Unity 2019`), causing compile errors. In case you want to enable the `.asmdef` files, rename the files:
 `Spine/Runtime/spine-unity.txt` to `Spine/Runtime/spine-unity.asmdef` and
 `Spine/Editor/spine-unity-editor.txt` to `Spine/Editor/spine-unity-editor.asmdef`.
* **SkeletonAnimator is now SkeletonMecanim** The Spine-Unity Mecanim-driven component `SkeletonAnimator` has been renamed `SkeletonMecanim` to make it more autocomplete-friendly and more obvious at human-glance. The .meta files and guids should remain intact so existing projects and prefabs should not break. However, user code needs to be updated to use `SkeletonMecanim`.
*  **SpineAtlasAsset** The existing `AtlasAsset` type has been renamed to `SpineAtlasAsset` to signify that it specifically uses a Spine/libGDX atlas as its source. Serialization should be intact but user code will need to be updated to refer to existing atlases as `SpineAtlasAsset`.
	* **AtlasAssetBase** `SpineAtlasAsset` now has an abstract base class called `SpineAtlasAsset`. This is the base class to derive when using alternate atlas sources. Existing SkeletonDataAsset field "atlasAssets" now have the "AtlasAssetBase" type. Serialization should be intact, but user code will need to be updated to refer to the atlas assets accordingly.
	* This change is in preparation for alternate atlas options such as Unity's SpriteAtlas.
* **Optional Straight Alpha for shaders** Spine-Unity's included Unity shaders now have a `_STRAIGHT_ALPHA_INPUT` shader_feature, toggled as a checkbox in the Material's inspector. This allows the Material to use a non-premultiplied alpha/straight alpha input texture.
	* The following shaders now have the "Straight Alpha Texture" checkbox when used on a material:
		* `Spine/Skeleton`
		* `Spine/Skeleton Tint Black`
		* `Spine/Skeleton Lit`
		* `Spine/Skeleton Tint`
		* `Spine/Skeleton Fill`
		* `Spine/SkeletonGraphic (Premultiply Alpha)` was renamed to `Spine/SkeletonGraphic`
		* `Spine/SkeletonGraphic Tint Black (Premultiply Alpha)` was renamed to `Spine/SkeletonGraphic Tint Black`
		* `Spine/Skeleton PMA Multiply`
		* `Spine/Skeleton PMA Screen`
	* Dedicated straight alpha shaders were removed from the runtime.
		* `Spine/Straight Alpha/Skeleton Fill`
		* `Spine/Straight Alpha/Skeleton Tint`
* **Detection of Incorrect Texture Settings** Especially when atlas textures are exported with setting `Premultiply alpha` enabled, it is important to configure Unity's texture import settings correctly. By default, you will now receive warnings where texture settings are expected to cause incorrect rendering.
  * The following rules apply:
    * `sRGB (Color Texture)` shall be disabled when `Generate Mip Maps` is enabled, otherwise you will receive white border outlines.
    * `Alpha Is Transparency` shall be disabled on `Premultiply alpha` textures, otherwise you will receive light ghosting artifacts in transparent areas.
  * These warnings can be disabled in `Edit - Preferences - Spine`.
* **Sprite Mask Support for all Included Shaders** The `Skeleton Animation` and `Skeleton Mecanim` components now provide an additional `Mask Interaction` field in the Inspector, covering identical functionality as Unity's built in `Sprite Renderer` component:
    * `Mask Interaction` modes:
      * `None` - The sprite will not interact with the masking system. Default behavior.
      * `Visible Inside Mask` - The sprite will be visible only in areas where a mask is present.
      * `Visible Outside Mask` - The sprite will be visible only in areas where no mask is present.
  * `Automatically Generated Materials` When switching `Mask Interaction` modes in the Inspector outside of Play mode, the required additional material assets are generated for the respective `Stencil Compare` parameters - with file suffixes `'_InsideMask'` and `'_OutsideMask'`, placed in the same folder as the original materials. By default all generated materials are kept as references by the `Skeleton Animation` component for switching at runtime.
  These materials can be managed and optimized via the `SkeletonAnimation`'s `Advanced` section:
    * Using the `Clear` button you can clear the reference to unneeded materials,
    * Using the `Delete` button the respective assets are deleted as well as references cleared. Note that other `Skeleton Animation` GameObjects might still reference the materials, so use with caution!
    * With the `Set` button you can again assign a link to the respective materials to prepare them for runtime use. If the materials were not present or have been deleted, they are generated again based on the default materials.
  * When switching `Mask Interaction` mode at runtime, the previously prepared materials are switched active automatically. When the respective materials have not been prepared, material copies of the default materials are created on the fly. Note that these materials are not shared between similar `Skeleton Animation` GameObjects, so it is recommended to use the generated material assets where possible.
  * **Every shader now exposes the `Stencil Compare` parameter** for further customization. This way you have maximum flexibility to use custom mechanisms to switch materials at runtime if you should ever need more than the three materials generated by `Skeleton Animation`'s `Mask Interaction` parameter. Reference `Stencil Compare` values are:
    * `CompareFunction.Disabled` for `Mask Interaction - None`
    * `CompareFunction.LessEqual` for `Mask Interaction - Visible Inside Mask`
    * `CompareFunction.Greater` for `Mask Interaction - Visible Outside Mask`
* **RectMask2D Support for SkeletonGraphic** Both `SkeletonGraphic` shaders '`Spine/SkeletonGraphic`' and '`Spine/SkeletonGraphic Tint Black`' now respect masking areas defined via Unity's `RectMask2D` component.
* **Timeline Support for Unity 2019** using the existing Timeline components. By default, all Spine Timeline components are deactivated in Unity 2019 and **can be activated via the Spine Preferences menu**. This step became necessary because in Unity 2019, Timeline has been moved to a separate Package and is no longer included in the Unity core. Please visit `Edit - Preferences - Spine` and at `Timeline Package Support` hit `Enable` to automatically perform all necessary steps to activate the Timeline components.
This will automatically:
  1. download the Unity Timeline package
  2. activate the Spine Timeline components by setting the compile definition `SPINE_TIMELINE_PACKAGE_DOWNLOADED` for all platforms
  3. modify the `spine-unity.asmdef` file by adding the reference to the Unity Timeline library.
* Added `Create 2D Hinge Chain` functionality at `SkeletonUtilityBone` inspector, previously only `Create 3D Hinge Chain` was available.

### XNA/MonoGame
* Added support for any `Effect` to be used by `SkeletonRenderer`
* Added support for `IVertexEffect` to modify vertices of skeletons on the CPU. `IVertexEffect` instances can be set on the `SkeletonRenderer`. See example project.
* Added `SkeletonDebugRenderer`
* Made `MeshBatcher` of SkeletonRenderer accessible via a getter. Allows user to batch their own geometry together with skeleton meshes for maximum batching instead of using XNA SpriteBatcher.

## Java
* **Breaking changes**
  * Skeleton attachments: Moved update of attached skeleton out of libGDX `SkeletonRenderer`, added overloaded method `Skeleton#updateWorldTransform(Bone)`, used for `SkeletonAttachment`. You now MUST call this new method with the bone of the parent skeleton to which the child skeleton is attached. See `SkeletonAttachmentTest` for and example.
  * The completion event will fire for looped 0 duration animations every frame.
  * `MixPose` is now called `MixBlend`.
  * Skeleton `flipX/flipY` has been replaced with `scaleY/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
  * Mix time is no longer affected by `TrackEntry#timeScale`. See https://github.com/EsotericSoftware/spine-runtimes/issues/1194
* **Additions**
  * Added `EventData#audioPath` field. This field contains the file name of the audio file used for the event.
  * Added convenience method to add all attachments from one skin to another, see https://github.com/EsotericSoftware/spine-runtimes/commit/a0b7bb6c445efdfac12b0cdee2057afa3eff3ead
  * Added additive animation blending. When playing back multiple animations on different tracks, where each animation modifies the same skeleton property, the results of tracks with lower indices are discarded, and only the result from the track with the highest index is used. With animation blending, the results of all tracks are mixed together. This allows effects like mixing multiple facial expressions (angry, happy, sad) with percentage mixes. By default the old behaviour is retained (results from lower tracks are discarded). To enable additive blending across animation tracks, call `TrackEntry#setMixBlend(MixBlend.add)` on each track. To specify the blend percentage, set `TrackEntry#alpha`. See http://esotericsoftware.com/forum/morph-target-track-animation-mix-mode-9459 for a discussion.
  * Support for stretchy IK
  * Support for audio events, see `audioPath`, `volume` and `balance` fields on event (data).
  * `TrackEntry` has an additional field called `holdPrevious`. It can be used to counter act a limitation of `AnimationState` resulting in "dipping" of parts of the animation. For a full discussion of the problem and the solution we've implemented, see this [forum thread](http://esotericsoftware.com/forum/Probably-Easy-Animation-mixing-with-multiple-tracks-10682?p=48130&hilit=holdprevious#p48130).

### libGDX
* Added `VertexEffect` interface, instances of which can be set on `SkeletonRenderer`. Allows to modify vertices before submitting them to GPU. See `SwirlEffect`, `JitterEffect` and `VertexEffectTest`.
* Added improved tint-black shader.
* Improved performance by avoiding batch flush when not switching between normal and additive rendering with PMA
* Improvements to skeleton viewer.
* `TwoColorPolygonBatch` implements the `Batch` interface, allowing to the be used with other libGDX classes that require a batcher for drawing, potentially improving performance. See https://github.com/EsotericSoftware/spine-runtimes/commit/a46b3d1d0c135d51f9bef9ca17a5f8e5dda69927
* Added `SkeletonDrawable` to render skeletons in scene2d UI https://github.com/EsotericSoftware/spine-runtimes/commit/b93686c185e2c9d5466969a8e07eee573ebe4b97

## Lua
* **Breaking changes**
  * The completion event will fire for looped 0 duration animations every frame.
  * Skeleton `flipX/flipY` has been replaced with `scaleY/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
  * Mix time is no longer affected by `TrackEntry#timeScale`. See https://github.com/EsotericSoftware/spine-runtimes/issues/1194
* **Additions**
  * Added `JitterEffect` and `SwirlEffect` and support for vertex effects in Corona and Love
  * Added additive animation blending. When playing back multiple animations on different tracks, where each animation modifies the same skeleton property, the results of tracks with lower indices are discarded, and only the result from the track with the highest index is used. With animation blending, the results of all tracks are mixed together. This allows effects like mixing multiple facial expressions (angry, happy, sad) with percentage mixes. By default the old behaviour is retained (results from lower tracks are discarded). To enable additive blending across animation tracks, call `TrackEntry:setMixBlend(MixBlend.add)` on each track. To specify the blend percentage, set `TrackEntry.alpha`. See http://esotericsoftware.com/forum/morph-target-track-animation-mix-mode-9459 for a discussion.
  * Support for stretchy IK
  * Support for audio events, see `audioPath`, `volume` and `balance` fields on event (data).
  * `TrackEntry` has an additional field called `holdPrevious`. It can be used to counter act a limitation of `AnimationState` resulting in "dipping" of parts of the animation. For a full discussion of the problem and the solution we've implemented, see this [forum thread](http://esotericsoftware.com/forum/Probably-Easy-Animation-mixing-with-multiple-tracks-10682?p=48130&hilit=holdprevious#p48130).

### Love2D
* Added support for vertex effects. Set an implementation like "JitterEffect" on `Skeleton.vertexEffect`. See `main.lua` for an example.

### Corona
* Added support for vertex effects. Set an implementation like "JitterEffect" on `SkeletonRenderer.vertexEffect`. See `main.lua` for an example

## Typescript/Javascript
* **Breaking changes**
  * The completion event will fire for looped 0 duration animations every frame.
  * Skeleton `flipX/flipY` has been replaced with `scaleY/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
  * Mix time is no longer affected by `TrackEntry#timeScale`. See https://github.com/EsotericSoftware/spine-runtimes/issues/1194
* **Additions**
  * Added `AssetManager.loadTextureAtlas`. Instead of loading the `.atlas` and corresponding image files manually, you can simply specify the location of the `.atlas` file and AssetManager will load the atlas and all its images automatically. `AssetManager.get("atlasname.atlas")` will then return an instance of `spine.TextureAtlas`.
  * Added additive animation blending. When playing back multiple animations on different tracks, where each animation modifies the same skeleton property, the results of tracks with lower indices are discarded, and only the result from the track with the highest index is used. With animation blending, the results of all tracks are mixed together. This allows effects like mixing multiple facial expressions (angry, happy, sad) with percentage mixes. By default the old behaviour is retained (results from lower tracks are discarded). To enable additive blending across animation tracks, call `TrackEntry#setMixBlend(MixBlend.add)` on each track. To specify the blend percentage, set `TrackEntry#alpha`. See http://esotericsoftware.com/forum/morph-target-track-animation-mix-mode-9459 for a discussion. See https://github.com/EsotericSoftware/spine-runtimes/blob/f045d221836fa56191ccda73dd42ae884d4731b8/spine-ts/webgl/tests/test-additive-animation-blending.html for an example.
  * Added work-around for iOS WebKit JIT bug, see https://github.com/EsotericSoftware/spine-runtimes/commit/c28bbebf804980f55cdd773fed9ff145e0e7e76c
  * Support for stretchy IK
  * Support for audio events, see `audioPath`, `volume` and `balance` fields on event (data).
  * `TrackEntry` has an additional field called `holdPrevious`. It can be used to counter act a limitation of `AnimationState` resulting in "dipping" of parts of the animation. For a full discussion of the problem and the solution we've implemented, see this [forum thread](http://esotericsoftware.com/forum/Probably-Easy-Animation-mixing-with-multiple-tracks-10682?p=48130&hilit=holdprevious#p48130).

### WebGL backend
* Added `VertexEffect` interface, instances of which can be set on `SkeletonRenderer`. Allows to modify vertices before submitting them to GPU. See `SwirlEffect`, `JitterEffect`, and the example which allows to set effects.
* Added `slotRangeStart` and `slotRangeEnd` parameters to `SkeletonRenderer#draw` and `SceneRenderer#drawSkeleton`. This allows you to render only a range of slots in the draw order. See `spine-ts/webgl/tests/test-slot-range.html` for an example.
* Added improved tint-black shader.
* Added `SceneRenderer#drawTextureUV()`, allowing to draw a texture with manually specified texture coordinates.
* Exposed all renderers in `SceneRenderer`.

### Canvas backend
* Added support for shearing and non-uniform scaling inherited from parent bones.
* Added support for alpha tinting.

### Three.js backend
* Added `VertexEffect` interface, instances of which can be set on `SkeletonMesh`. Allows to modify vertices before submitting them to GPU. See `SwirlEffect`, `JitterEffect`.
* Added support for multi-page atlases

### Widget backend
 * Added fields `atlasContent`, `atlasPagesContent`, and `jsonContent` to `WidgetConfiguration` allowing you to directly pass the contents of the `.atlas`, atlas page `.png` files, and the `.json` file without having to do a request. See `README.md` and the example for details.
 * `SpineWidget.setAnimation()` now takes an additional optional parameter for callbacks when animations are completed/interrupted/etc.

# 3.6

## AS3
* **Breaking changes**
  * Removed `Bone.worldToLocalRotationX` and `Bone.worldToLocalRotationY`. Replaced by `Bone.worldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * Made `Bone` fields `_a`, `_b`, `_c`, `_d`, `_worldX` and `_worldY` public, removed underscore prefix.
  * Removed `VertexAttachment.computeWorldVertices` overload, changed `VertexAttachment.computeWorldVertices2` to `VertexAttachment.computeWorldVertices`, added `stride` parameter.
  * Removed `RegionAttachment.vertices` field. The vertices array is provided to `RegionAttachment.computeWorldVertices` by the API user now.
  * Removed `RegionAttachment.updateWorldVertices`, added `RegionAttachment.computeWorldVertices`. The new method now computes the x/y positions of the 4 vertices of the corner and places them in the provided `worldVertices` array, starting at `offset`, then moving by `stride` array elements when advancing to the next vertex. This allows to directly compose the vertex buffer and avoids a copy. The computation of the full vertices, including vertex colors and texture coordinates, is now done by the backend's respective renderer.
  * Replaced `r`, `g`, `b`, `a` fields with instances of new `Color` class in `RegionAttachment`, `MeshAttachment`, `Skeleton`, `SkeletonData`, `Slot` and `SlotData`.
  * The completion event will fire for looped 0 duration animations every frame.

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
 * Added 'getTexture()' method to 'StarlingTextureAtlasAttachmentLoader'
 * Breaking change: if a skeleton requires two color tinting, you have to enable it via `SkeletonSprite.twoColorTint = true`. In this case the skeleton will use the `TwoColorMeshStyle`, which internally uses a different vertex layout and shader. This means that skeletons with two color tinting enabled will break batching and hence increase the number of draw calls in your app.

## C
* **Breaking changes**
  * `spVertexAttachment_computeWorldVertices` and `spRegionAttachment_computeWorldVerticeS` now take new parameters to make it possible to directly output the calculated vertex positions to a vertex buffer. Removes the need for additional copies in the backends' respective renderers.
  * Removed `spBoundingBoxAttachment_computeWorldVertices`, superseded by `spVertexAttachment_computeWorldVertices`.
  * Removed `spPathAttachment_computeWorldVertices` and `spPathAttachment_computeWorldVertices1`, superseded by `spVertexAttachment_computeWorldVertices`.
  * Removed `sp_MeshAttachment_computeWorldVertices`, superseded by `spVertexAttachment_computeWorldVertices`.
  * Removed `spBone_worldToLocalRotationX` and `spBone_worldToLocalRotationY`. Replaced by `spBone_worldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * Replaced `r`, `g`, `b`, `a` fields with instances of new `spColor` struct in `spRegionAttachment`, `spMeshAttachment`, `spSkeleton`, `spSkeletonData`, `spSlot` and `spSlotData`.
  * Removed `spVertexIndex`from public API.
  * Listeners on `spAnimationState` or `spTrackEntry` will now be also called in case a track entry is disposed as part of dispoing the `spAnimationState`.
  * The completion event will fire for looped 0 duration animations every frame.
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
  * Introduced `SP_API` macro. Every spine-c function is prefixed with this macro. By default, it is an empty string. Can be used to markup spine-c functions with e.g. ``__declspec` when compiling to a dll or linking to that dll.

### Cocos2d-X
 * Fixed renderer to work with 3.6 changes
 * Optimized rendering by removing all per-frame allocation in `SkeletonRenderer`, resulting in 15% performance increase for large numbers of skeletons being rendered per frame.
 * Added support for two color tinting. Tinting is enabled/disabled per `SkeletonRenderer`/`SkeletonAnimation` instance. Use `SkeletonRenderer::setTwoColorTint()`. Note that two color tinting requires the use of a non-standard shader and vertex format. This means that skeletons rendered with two color tinting will break batching. However, skeletons with two color tinting enabled and rendered after each other will be batched.
 * Updated example to use Cocos2d-x 3.14.1.
 * Added mesh debug rendering. Enable/Disable via `SkeletonRenderer::setDebugMeshesEnabled()`.
 * Added support for clipping.
 * SkeletonRenderer now combines the displayed color of the Node (cascaded from all parents) with the skeleton color for tinting.
 * Added support for vertex effects. See `RaptorExample.cpp`.
 * Added ETC1 alpha support, thanks @halx99! Does not work when two color tint is enabled.
 * Added `spAtlasPage_setCustomTextureLoader()` which let's you do texture loading manually. Thanks @jareguo.
 * Added `SkeletonRenderer:setSlotsRange()` and `SkeletonRenderer::createWithSkeleton()`. This allows you to split rendering of a skeleton up into multiple parts, and render other nodes in between. See `SkeletonRendererSeparatorExample.cpp` for an example.

### Cocos2d-Objc
 * Fixed renderer to work with 3.6 changes
 * Added support for two color tinting. Tinting is enabled/disabled per `SkeletonRenderer/SkeletonAnimation.twoColorTint = true`. Note that two color tinted skeletons do not batch with other nodes.
 * Added support for clipping.

### SFML
 * Fixed renderer to work with 3.6 changes. Sadly, two color tinting does not work, as the vertex format in SFML is fixed.
 * Added support for clipping.
 * Added support for vertex effects. See raptor example.
 * Added premultiplied alpha support to `SkeletonDrawable`.

### Unreal Engine 4
 * Fixed renderer to work with 3.6 changes
 * Added new UPROPERTY to SpineSkeletonRendererComponent called `Color`. This allows to set the tint color of the skeleton in the editor, C++ and Blueprints. Under the hood, the `spSkeleton->color` will be set on every tick of the renderer component.
 * Added support for clipping.
 * Switched from built-in ProceduralMeshComponent to RuntimeMeshComponent by Koderz (https://github.com/Koderz/UE4RuntimeMeshComponent, MIT). Needed for more flexibility regarding vertex format, should not have an impact on existing code/assets. You need to copy the RuntimeMeshComponentPlugin from our repository in `spine-ue4\Plugins\` to your project as well!
 * Added support for two color tinting. All base materials, e.g. SpineUnlitNormalMaterial, now do proper two color tinting. No material parameters have changed.
 * Updated to Unreal Engine 4.16.1. Note that 4.16 has a regression which will make it impossible to compile plain .c files!
 * spine-c is now exposed from the plugin shared library on Windows via __declspec.

## C#
* **Breaking changes**
  *  `MeshAttachment.parentMesh` is now a private field to enforce using the `.ParentMesh` setter property in external code. The `MeshAttachment.ParentMesh` property is an appropriate replacement wherever `.parentMesh` was used.
  * `Skeleton.GetBounds` takes a scratch array as input so it doesn't have to allocate a new array on each invocation itself. Reduces GC activity.
  * Removed `Bone.WorldToLocalRotationX` and `Bone.WorldToLocalRotationY`. Replaced by `Bone.WorldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * Added `stride` parameter to `VertexAttachment.ComputeWorldVertices`.
  * Removed `RegionAttachment.Vertices` field. The vertices array is provided to `RegionAttachment.ComputeWorldVertices` by the API user now.
  * Removed `RegionAttachment.UpdateWorldVertices`, added `RegionAttachment.ComputeWorldVertices`. The new method now computes the x/y positions of the 4 vertices of the corner and places them in the provided `worldVertices` array, starting at `offset`, then moving by `stride` array elements when advancing to the next vertex. This allows to directly compose the vertex buffer and avoids a copy. The computation of the full vertices, including vertex colors and texture coordinates, is now done by the backend's respective renderer.
  * The completion event will fire for looped 0 duration animations every frame.

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
 * Added support for `IVertexEffect` to modify vertices of skeletons on the CPU. `IVertexEffect` instances can be set on the `SkeletonRenderer`. See example project.
 * Added `SkeletonDebugRenderer`
 * Made `MeshBatcher` of SkeletonRenderer accessible via a getter. Allows user to batch their own geometry together with skeleton meshes for maximum batching instead of using XNA SpriteBatcher.

## Java
* **Breaking changes**
  * `Skeleton.getBounds` takes a scratch array as input so it doesn't have to allocate a new array on each invocation itself. Reduces GC activity.
  * Removed `Bone.worldToLocalRotationX` and `Bone.worldToLocalRotationY`. Replaced by `Bone.worldToLocalRotation` (rotation given relative to x-axis, counter-clockwise, in degrees).
  * Added `stride` parameter to `VertexAttachment.computeWorldVertices`.
  * Removed `RegionAttachment.vertices` field. The vertices array is provided to `RegionAttachment.computeWorldVertices` by the API user now.
  * Removed `RegionAttachment.updateWorldVertices`, added `RegionAttachment.computeWorldVertices`. The new method now computes the x/y positions of the 4 vertices of the corner and places them in the provided `worldVertices` array, starting at `offset`, then moving by `stride` array elements when advancing to the next vertex. This allows to directly compose the vertex buffer and avoids a copy. The computation of the full vertices, including vertex colors and texture coordinates, is now done by the backend's respective renderer.
  * Skeleton attachments: Moved update of attached skeleton out of libGDX `SkeletonRenderer`, added overloaded method `Skeleton#updateWorldTransform(Bone), used for `SkeletonAttachment`. You now MUST call this new method
  with the bone of the parent skeleton to which the child skeleton is attached. See `SkeletonAttachmentTest` for and example.
  * The completion event will fire for looped 0 duration animations every frame.

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
  * The completion event will fire for looped 0 duration animations every frame.
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
  * The completion event will fire for looped 0 duration animations every frame.
  * Removed the Spine Widget in favor of [Spine Web Player](https://esotericsoftware.com/spine-player).

* **Additions**
  * Added support for local and relative transform constraint calculation, including additional fields in `TransformConstraintData`
  * Added `Bone.localToWorldRotation`(rotation given relative to x-axis, counter-clockwise, in degrees).
  * Added two color tinting support, including `TwoColorTimeline` and additional fields on `Slot` and `SlotData`.
  * Added `PointAttachment`, additional method `newPointAttachment` in `AttachmentLoader` interface.
  * Added `ClippingAttachment`, additional method `newClippingAttachment` in `AttachmentLoader` interface.
  * Added `SkeletonClipper` and `Triangulator`, used to implement software clipping of attachments.
  * `AnimationState#apply` returns boolean indicating if any timeline was applied or not.
  * `Animation#apply` and `Timeline#apply`` now take enums `MixPose` and `MixDirection` instead of booleans
  * Added `AssetManager.loadTextureAtlas`. Instead of loading the `.atlas` and corresponding image files manually, you can simply specify the location of the `.atlas` file and AssetManager will load the atlas and all its images automatically. `AssetManager.get("atlasname.atlas")` will then return an instance of `spine.TextureAtlas`.
  * Added the [Spine Web Player](https://esotericsoftware.com/spine-player)


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
 * Added `slotRangeStart` and `slotRangeEnd` parameters to `SkeletonRenderer#draw` and `SceneRenderer#drawSkeleton`. This allows you to render only a range of slots in the draw order. See `spine-ts/webgl/tests/test-slot-range.html` for an example.

### Canvas backend
 * Fixed renderer to work for 3.6 changes. Sadly, we can't support two color tinting via the Canvas API.
 * Added support for shearing and non-uniform scaling inherited from parent bones.
 * Added support for alpha tinting.

### Three.js backend
 * Fixed renderer to work with 3.6 changes. Two color tinting is not supported.
 * Added clipping support
 * Added `VertexEffect` interface, instances of which can be set on `SkeletonMesh`. Allows to modify vertices before submitting them to GPU. See `SwirlEffect`, `JitterEffect`.
 * Added support for multi-page atlases

### Widget backend
 * Fixed WebGL context loss (see WebGL backend changes). Enabled automatically.
 * Fixed renderer to work for 3.6 changes. Supports two color tinting & clipping (see WebGL backend changes for details).
 * Added fields `atlasContent`, `atlasPagesContent`, and `jsonContent` to `WidgetConfiguration` allowing you to directly pass the contents of the `.atlas`, atlas page `.png` files, and the `.json` file without having to do a request. See `README.md` and the example for details.
 * `SpineWidget.setAnimation()` now takes an additional optional parameter for callbacks when animations are completed/interrupted/etc.
