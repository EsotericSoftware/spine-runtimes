# 4.1

## C
* **Additions**
  * Support for sequences.
  * Support for `shortestRotation` in animation state. See https://github.com/esotericsoftware/spine-runtimes/issues/2027.
  * Added CMake parameter `SPINE_SANITIZE` which will enable sanitizers on macOS and Linux.
  * Added `SPINE_MAJOR_VERSION`, `SPINE_MINOR_VERSION`, and `SPINE_VERSION_STRING`. Parsing skeleton .JSON and .skel files will report an error if the skeleton version does not match the runtime version.
* **Breaking changes**
  * `spRegionAttachment` and `spMeshAttachment` now contain a `spTextureRegion*` instead of encoding region fields directly.
  * `sp_AttachmentLoader_newRegionAttachment()` and `spAttachmentLoader_newMeshAttachment()` now take an additional `Sequence*` parameter.
  * `spMeshAttachment_updateUVs()` was renamed to `spMeshAttachment_updateRegion()`.
  * `spRegionAttachment_updateOffset()` was renamed to `spRegionAttachment_updateRegion()`, `spRegionAttachment_setUVs()` was merged into `spRegionAttachment_updateRegion()`.
  * `spSlot_getAttachmentTime()` and `spSlot_setAttachmentTime()` have been removed.
  * `spVertexAttachment->deformAttachment` was renamed to `spVertexAttachment->timelineAttachment`.
  * `spSkeleton_update()` has been removed.
  * `spSkeleton->time` has been removed.
  * `spVertexEffect` has been removed.

### SFML
  * Updated example to use SFML 2.5.1.
  * Added dragon example.

## C++
* **Additions**
  * Support for sequences.
  * Support for `shortestRotation` in animation state. See https://github.com/esotericsoftware/spine-runtimes/issues/2027.
  * Added CMake parameter `SPINE_SANITIZE` which will enable sanitizers on macOS and Linux.
    * Added `SPINE_MAJOR_VERSION`, `SPINE_MINOR_VERSION`, and `SPINE_VERSION_STRING`. Parsing skeleton .JSON and .skel files will report an error if the skeleton version does not match the runtime version.
* **Breaking changes**
  * `RegionAttachment` and `MeshAttachment` now contain a `TextureRegion*` instead of encoding region fields directly.
  * `AttachmentLoader::newRegionAttachment()` and `AttachmentLoader::newMeshAttachment()` now take an additional `Sequence*` parameter.
  * `MeshAttachment::updateUVs()` was renamed to `MeshAttachment::updateRegion()`.
  * `RegionAttachment::updateOffset()` was renamed to `RegionAttachment::updateRegion()`, `RegionAttachment::setUVs()` was merged into `updateRegion()`.
  * `Slot::getAttachmentTime()` and `Slot::setAttachmentTime()` have been removed.
  * `VertexAttachment::getDeformAttachment()` was renamed to `VertexAttachment::getTimelineAttachment()`.
  * `Skeleton::update()` has been removed.
  * `Skeleton::getTime()` has been removed.
  * `VertexEffect` has been removed.
  
### Cocos2d-x

### SFML
  * Updated example to use SFML 2.5.1.
  * Added dragon example.

### UE4
  * Updated example project to UE 4.27

## C# ##

* **Additions**
  * Full support for sequences.
  * Support for `shortestRotation` in animation state. See https://github.com/esotericsoftware/spine-runtimes/issues/2027.
  * `RegionAttachment` and `MeshAttachment` now provide a `Region` property. Use this property instead of the removed `RendererObject` property (see section *Breaking Changes* below).

* **Breaking changes**
  * Removed `RendererObject` property from `RegionAttachment` and `MeshAttachment`. Use `attachment.Region` property instead. Removed removed `IHasRendererObject` interface. Use `IHasTextureRegion` instead.
  * Replaced `RegionAttachment.UpdateOffset` and `MeshAttachment.UpdateUVs` with `Attachment.UpdateRegion`. The caller must ensure that the attachment's region is not `null`.
  * Removed `AttachmentRegionExtensions` methods `Attachment.SetRegion`, `MeshAttachment.SetRegion` and `RegionAttachment.SetRegion(region, update)`. Use `attachment.Region = region; if (update) attachment.UpdateRegion()` instead.
  * `AttachmentLoader.NewRegionAttachment()` and `AttachmentLoader.NewMeshAttachment()` take an additional `Sequence` parameter.
  * `VertexAttachment.DeformAttachment` property has been replaced with `VertexAttachment.TimelineAttachment`.
  * `RegionAttachment.ComputeWorldVertices()` takes a `Slot` instead of a `Bone` as the first argument.
  * Removed `Skeleton.Update(float deltaTime)` method.
  * Removed `Slot.AttachmentTime` property.
  * Removed extension method `AtlasRegion.GetSpineAtlasRect()` parameter `includeRotate` (with default value `true`). Most likely this method was never used with `includeRotate=false` in user code so no changes are required.
  * `AtlasRegion.PackedWidth` and `AtlasRegion.PackedHeight` are swapped compared to 4.0 when packing rotation is equal to 90 degrees. Most likely this property was never accessed in user code so no changes are required.

### Unity

* **Officially supported Unity versions are 2017.1-2022.1**.

* **Additions**
  * `SpineAtlasAsset.CreateRuntimeInstance` methods now provide an optional `newCustomTextureLoader` parameter (defaults to `null`) which can be set to e.g. `(a) => new YourCustomTextureLoader(a)` to use your own `TextureLoader` subclass instead of `MaterialsTextureLoader`.
  * `SkeletonAnimation`, `SkeletonMecanim` and `SkeletonGraphic` now provide an Inspector parameter `Advanced` - `Animation Update` with modes `In Update` **(previous behaviour, the default)**, `In FixedUpdate` and `Manual Update`. This allows to update animation in `FixedUpdate` when using the `SkeletonRootMotion` component (which is the recommended combination now, issuing a warning otherwise). The reason is that when root motion leads to a collision with a physics collider, it can introduce jittery excess movement when updating animation in `Update` due to more `Update` calls following a single `FixedUpdate` call.
  * Added `SkeletonRootMotion` properties `PreviousRigidbodyRootMotion` and `AdditionalRigidbody2DMovement`. Setting or querying these movement vectors can be necessary when multiple scripts call `Rigidbody2D.MovePosition` on the same object where the last call overwrites the effect of preceding ones.
  * `BoneFollower` and `BoneFollowerGraphic` now provide an additional `Follow Parent World Scale` parameter to allow following simple scale of parent bones (rotated/skewed scale can't be supported).
  * Improved `Advanced - Fix Prefab Override MeshFilter` property for `SkeletonRenderer` (and subclasses`SkeletonAnimation` and `SkeletonMecanim`), now providing an additional option to use a global value which can be set in `Edit - Preferences - Spine`.
  * Timeline naming improvements: `Spine AnimationState Clip` Inspector parameter `Custom Duration` changed and inverted to `Default Mix Duration` for more clarity. Shortened all Timeline add track menu entries from: `Spine.Unity.Playables - <track type>` to `Spine - <track type>`, `Spine Animation State Track` to `SkeletonAnimation Track`, `Spine AnimationState Graphic Track` to `SkeletonGraphic Track`, and `Spine Skeleton Flip Track` to `Skeleton Flip Track`.
  * Timeline track appearance and Inspector: Tracks now show icons and track colors to make them easier to distinguish. When a Track is selected, the Inspector now shows an editable track name which was previously only editable at the Timeline asset.
  * Added example component `SkeletonRenderTexture` to render a `SkeletonRenderer` to a `RenderTexture`, mainly for proper transparency. Added an example scene named `RenderTexture FadeOut Transparency` that demonstrates usage for a fadeout transparency effect.
  * Added another fadeout example component named `SkeletonRenderTextureFadeout` which takes over transparency fadeout when enabled. You can use this component as-is, attach it in disabled state and enable it to start a fadeout effect.
  * Timeline clips now offer an additional `Alpha` parameter for setting a custom constant mix alpha value other than 1.0, just as `TrackEntry.Alpha`. Defaults to 1.0.
  * `GetRemappedClone` copying from `Sprite` now provides additional `pmaCloneTextureFormat` and `pmaCloneMipmaps` parameters to explicitly specify the texture format of a newly created PMA texture.
  * Spine property Inspector fields (`Animation Name`, `Bone Name`, `Slot` and similar) now display the name in red when the respective animation/bone/etc no longer exists at the skeleton data. This may be helpful when such items have been renamed or deleted.
  * Added `UnscaledTime` property at `SkeletonAnimation` as well, behaving like `SkeletonGraphic.UnscaledTime`. If enabled, AnimationState uses unscaled game time (`Time.unscaledDeltaTime`), running animations independent of e.g. game pause (`Time.timeScale`).
  * `SkeletonAnimation`, `SkeletonMecanim` and `SkeletonGraphic` now provide an additional `OnAnimationRebuild` callback delegate which is issued after both the skeleton and the animation state have been initialized.
  * Timeline `SkeletonAnimation Track` and `SkeletonGraphic Track` now provide an `Unscaled Time` property. Whenever starting a new animation clip of this track, `SkeletonAnimation.UnscaledTime` or `SkeletonGraphic.UnscaledTime` will be set to this value. This allows you to play back Timeline clips either in normal game time or unscaled game time. Note that `PlayableDirector.UpdateMethod` is ignored and replaced by this property, which allows more fine-granular control per Timeline track.
 
* **Breaking changes**
  * Made `SkeletonGraphic.unscaledTime` parameter protected, use the new property `UnscaledTime` instead.
  * `SkeletonGraphic` `OnRebuild` callback delegate is now issued after the skeleton has been initialized, before the `AnimationState` component is initialized. This makes behaviour consistent with `SkeletonAnimation` and `SkeletonMecanim` component behaviour. Use the new callback `OnAnimationRebuild` if you want to receive a callback after the `SkeletonGraphic` `AnimationState` has been initialized.

* **Changes of default values**

* **Deprecated**

* **Restructuring (Non-Breaking)**

### XNA/MonoGame
* **Breaking change**: Removed spine-xna in favor of spine-monogame. See https://github.com/EsotericSoftware/spine-runtimes/issues/1949
* Added new spine-monogame solution. See [spine-monogame/README.md](spine-monogame/README.md) for updated instructions on how to use spine-monogame.

## Java
* **Additions**
  * Support for `shortestRotation` in animation state. See https://github.com/esotericsoftware/spine-runtimes/issues/2027.
  * Support for sequences.
* **Breaking changes**
  * `AttachmentLoader#newRegionAttachment()` and `AttachmentLoader#newMeshAttachment()` take an additional `Sequence` parameter.
  * `Slot#setAttachmentTime()` and `Slot#getAttachmentTime()` have been removed.
  * `VertexAttachment#setDeformAttachment()` and `VertexAttachment#getDeformAttachment()` have been replaced with `VertexAttachment#setTimelineAttachment()` and `VertexAttachment#getTimelineAttachment()`.
  * `RegionAttachment#updateOffset()` has been renamed to `RegionAttachment#updateRegion()`. The caller must ensure that the attachment's region is not `null`.
  * `RegionAttachment#computeWorldVertices()` takes a `Slot` instead of a `Bone` as the first argument.
  * `VertexEffect` has been removed.


### libGDX
* `spine-libgdx`, `spine-libgdx-tests`, and `spine-skeletonviewer` are now fully Gradle-ified.
* `spine-skeletonviewer` now supports quickly loading skeletons by dragging and dropping `.json` or `.skel` skeleton files onto the window.

## Typescript/Javascript
* **Additions**
  * full support for sequences.
  * Added `Promise` based `AssetManager.loadAll()`. Allows synchronous waiting via `await assetManager.loadAll()`, simplifying loader logic in applications.
  * Support for `shortestRotation` in animation state. See https://github.com/esotericsoftware/spine-runtimes/issues/2027.
  * Full support for sequences.
* **Breaking changes**
  * `AttachmentLoader#newRegionAttachment()` and `AttachmentLoader#newMeshAttachment()` take an additional `Sequence` parameter.
  * `Slot#attachmentTime` and has been removed.
  * `VertexAttachment#deformAttachment` has been replaced with `VertexAttachment#timelineAttachment`.
  * `RegionAttachment#updateOffset()` has been renamed to `RegionAttachment#updateRegion()`. The caller must ensure that the attachment's region is not `null`.
  * `RegionAttachment#computeWorldVertices()` takes a `Slot` instead of a `Bone` as the first argument.
  * Removed `PlayerEditor`.
  * `VertexEffect` has been removed.

### WebGL backend
  * `PolygonBatcher.start()` now disables culling and restores the previous state on `PolygonBatcher.end()`.
  * Added `SpineCanvas`, a simpler way to render a scene via spine-webgl. See `spine-ts/spine-webgl/examples/barebones.html` and `spine-ts/spine-webgl/examples/mix-and-match.html`.

### Canvas backend
  * Improved example.

### Three.js backend
  * Added orbital controls to THREJS example.
  * `SkeletonMesh` takes an optional `SkeletonMeshMaterialCustomizer`, allowing modification of materials used by `SkeletonMesh`.
  * Added `SkeletonMeshMaterial.alphaTest`, when > 0, alpha testing will be performed and fragments will not be written to the depth buffer, if depth writes are enabled. 

### Player
  * Added `SpinePlayer.dispose()` to explicitely dispose of all resources the player holds on to.

# 4.0

## AS3
**NOTE: Spine 4.0 will be the last release supporting spine-as3. Starting from Spine 4.1, spine-as3 will no longer be supported or maintained.**
* Switched projects from FDT to Visual Studio Code. See updated `README.md` files for instructions.
* Expose non-essential colors on bones, bounding box, clipping, and path attachments.
* Timeline API has been extended to support component-wise timelines exported by Spine 4.0.
* Added `AnimationState.clearNext()` which removes the given `TrackEntry` and all entries after it.
* Added support for texture atlas key value pairs, see `AtlasRegion.names` and `AtlasRegion.values`.
* Added support for reverse animation playback via `TrackEntry.reverse`.
* Added proportional spacing mode support for path constraints.
* Added support for uniform scaling for two bone IK.
* Fixed applying a constraint reverting changes from other constraints.

### Starling
**NOTE: Spine 4.0 will be the last release supporting spine-starling. Starting from Spine 4.1, spine-starling will no longer be supported or maintained.**
* Switched projects from FDT to Visual Studio Code. See updated `README.md` files for instructions.
* Updated to Starling 2.6 and Air SDK 33.

## C
* **Breaking change:** Removed `SPINE_SHORT_NAMES` define and C++ constructors.
* Timeline API has been extended to support component-wise timelines exported by Spine 4.0.
* Added `spAnimationState_clearNext()` which removes the given `spTrackEntry` and all entries after it.
* Added support for texture atlas key value pairs, see `spAtlasRegion.keyValues`.
* Added support for reverse animation playback via `spTrackEntry.reverse`.
* Added proportional spacing mode support for path constraints.
* Added support for uniform scaling for two bone IK.
* Fixed applying a constraint reverting changes from other constraints.

### Cocos2d-Objc
**NOTE: Spine 4.0 will be the last release supporting spine-cocos2d-objc. Starting from Spine 4.1, spine-cocos2d-objc will no longer be supported or maintained.**

### SFML
* Added `ikDemo()` in `main.cpp`. to illustrate how to drive a bone and IK chain through mouse movement.

## C++
* Removed dependency on STL throughout the code base, cutting down on the LOC that need parsing by 66%.
* Exposed `x` and `y` on `SkeletonData` through getters and setters.
* Expose non-essential colors on bones, bounding box, clipping, and path attachments.
* Timeline API has been extended to support component-wise timelines exported by Spine 4.0.
* Added `AnimationState.clearNext()` which removes the given `TrackEntry` and all entries after it.
* Added support for texture atlas key value pairs, see `AtlasRegion.names` and `AtlasRegion.values`.
* Added support for reverse animation playback via `TrackEntry.reverse`.
* Added proportional spacing mode support for path constraints.
* Added support for uniform scaling for two bone IK.
* Fixed applying a constraint reverting changes from other constraints.
* spine-cpp now requires C++11.

### Cocos2d-x
* Added `IKExample` scene to illustrate how to drive a bone and IK chain through mouse movement.
* Added `SkeletonAnimation::setPreUpdateWorldTransformsListener()` and `SkeletonAnimation::setPostUpdateWorldTransformsListener()`. This allows users to modify bone transforms and other skeleton properties before and after the world transforms of all bones are calculated. See the `IKExample` for a usage example.

### SFML
* Added `ikDemo()` in `main.cpp`. to illustrate how to drive a bone and IK chain through mouse movement.

### UE4
* `SpineWidget` now supports the full widget transform, including rendering scale/shear.
* Materials on `SkeletonRendererComponent` are now blueprint read and writeable. This allows setting dynamic material instances at runtime.
* Added `InitialSkin` property to `USpineWidget`. This allows previewing different skins in the UMG Designer. Initial skins can still be overridden via blueprint events such as `On Initialized`.
* **Breaking change:** `SpineWidget` no longer has the `Scale` property. Instead the size x/y properties can be used.
* Added `SetSlotColor` on `USpineSkeletonComponent` to easily set the color of a slot via blueprints.
* Changed mixes set on an `SkeletonDataAsset` will now be applied to instances of `USpineSkeletonComponent`.
* Generated normals are now correctly flipped for back faces.
* Modifying parent materials updates material instances accordingly.
* Only `.json` files that are actually encoding Spine skeletons will be loaded. Other `.json` files will be left to other importers.
* Updated example project to UE 4.27.


## C# ##
* Timeline API has been extended to support component-wise timelines exported by Spine 4.0.
* Added `AnimationState.clearNext()` which removes the given `TrackEntry` and all entries after it.
* Added support for texture atlas key value pairs, see `AtlasRegion.names` and `AtlasRegion.values`.
* Added support for reverse animation playback via `TrackEntry.Reverse`.
* Added proportional spacing mode support for path constraints.
* Added support for uniform scaling for two bone IK.
* Fixed applying a constraint reverting changes from other constraints.
* **Breaking change:** Removed `SkeletonData` and `Skeleton` methods: `FindBoneIndex`, `FindSlotIndex`. Bones and slots have an `Index` field that should be used instead. Be sure to check for e.g. `bone == null` accordingly before accessing `bone.Index`.

### Unity

* **Officially supported Unity versions are 2017.1-2022.1**.
* **Breaking changes**
  * Removed all `Spine.Unity.AttachmentTools.SkinUtilities` Skin extension methods. These have become obsoleted and error-prone since the introduction of the new Skin API in 3.8. To fix any compile errors, replace any usage of `Skin` extension methods with their counterparts, e.g. replace occurrances of `skin.AddAttachments()` with `skin.AddSkin()`. Please see the example scene `Mix and Match Skins` on how to use the new Skin API to combine skins, or the updated old example scenes `Mix and Match` and `Mix and Match Equip` on how you can update an existing project using the old workflow. If you are using `skeletonAnimation.Skeleton.UnshareSkin()` in your code, you can replace it with `Skin customSkin = new Skin("custom skin"); customSkin.AddSkin(skeletonAnimation.Skeleton.Skin);`.
  * `Skin.GetAttachments()` has been replaced by `Skin.Attachments`, returning an `ICollection<SkinEntry>`. This makes access more consistent and intuitive. To fix any compile errors, replace any occurrances of `Skin.GetAttachments()` by `Skin.Attachments`.
  * Removed redundant `Spine.Unity.AttachmentTools.AttachmentCloneExtensions` extension methods `Attachment.GetCopy()` and `Attachment.GetLinkedMesh()`. To fix any compile errors, replace any occurrances with `Attachment.Copy()` and `Attachment.NewLinkedMesh()`.
  * Removed `Spine.Unity.AttachmentTools.AttachmentRegionExtensions` extension methods `Attachment.GetRegion()`. Use `Attachment.RendererObject as AtlasRegion` instead.
  * Removed redundant `Spine.SkeletonExtensions` extension methods:
    Replace:
    * `Skeleton.SetPropertyToSetupPose()`
    * `Skeleton.SetDrawOrderToSetupPose()`
    * `Skeleton.SetSlotAttachmentsToSetupPose()`
    * `Skeleton.SetSlotAttachmentToSetupPose()`

    with `Skeleton.SetSlotsToSetupPose()`.
    Replace:
     * `Slot.SetColorToSetupPose()`
     * `Slot.SetAttachmentToSetupPose()`

    with `Slot.SetToSetupPose()`.

    Also removed less commonly used extension methods:
    `TrackEntry.AllowImmediateQueue()`, `Animation.SetKeyedItemsToSetupPose()` and `Attachment.IsRenderable()`.

  * **`SkeletonGraphic` now no longer uses a `RawImage` component at each submesh renderer** GameObject when `allowMultipleCanvasRenderers` is true. Instead,  a new custom component `SkeletonSubmeshGraphic` is used which is more resource friendly. Replacement of these components will be performed automatically through editor scripting, saving scenes or prefabs will persist the upgrade.
  * **Linear color space:** Previously Slot colors were not displayed the same in Unity as in the Spine Editor. This is now fixed at all shaders, including URP and LWRP shaders. See section *Additions* below for more details. If you have tweaked Slot colors to look correct in `Linear` color space in Unity but incorrect in Spine, you might want to adjust the tweaked colors. Slot colors displayed in Unity should now match colors displayed in the Spine Editor when configured to display as `Linear` color space in the Spine Editor Settings.
  * **Additive Slots have always been lit** before they were written to the target buffer. Now all lit shaders provide an additional parameter `Light Affects Additive` which defaults to `false`, as it is the more intuitive default value. You can enable the old behaviour by setting this parameter to `true`.
  * **Corrected blending behaviour of all `Sprite` shaders** in `Premultiply Alpha` blend mode (including URP and LWRP packages). Previously vertex color alpha was premultiplied again, even though `Premultiply Alpha` blend mode assumes PMA texture and PMA vertex color input. Slot-alpha blending will thus be correctly lighter after upgrading to 4.0. If you have compensated this problem by disabling `Advanced - PMA Vertex Colors` you can now re-enable this parameter, also allowing for rendering Additive slots in a single pass.
  * **Corrected all `Outline` shaders outline thickness** when `Advanced - Sample 8 Neighbourhood` is disabled (thus using `4 Neighbourhood`). Previously weighting was incorrectly thick (4x as thick) compared to 8 neighbourhood, now it is more consistent. This might require adjustment of all your outline materials where `Sample 8 Neighbourhood` is disabled to restore the previous outline thickness, by adjusting the `Outline Threshold` parameter through adding a `/4` to make the threshold 4 times smaller.
  * Reverted changes: `BoneFollower` property `followLocalScale` has intermediately been renamed to `followScale` but was renamed back to `followLocalScale`. Serialized values (scenes and prefabs) will automatically be upgraded, only code accessing `followScale` needs to be adapted.
  * Fixed Timeline not pausing (and resuming) clip playback on Director pause, this is now the default behaviour. If you require the old behaviour (e.g. to continue playing an idle animation during Director pause), there is now an additional parameter `Don't Pause with Director` provided that can be enabled for each Timeline clip.
  * Fixed Timeline `Spine AnimationState Clips` ignoring empty space on the Timeline after a clip's end. Timeline clips now also offer `Don't End with Clip` and `Clip End Mix Out Duration` parameters if you prefer the old behaviour of previous versions. By default when empty space follows the clip on the timeline, the empty animation is set on the track with a MixDuration of `Clip End Mix Out Duration`. Set `Don't End with Clip` to `true` to continue playing the clip's animation instead and mimic the old 3.8 behaviour. If you prefer pausing the animation instead of mixing out to the empty animation, set `Clip End Mix Out Duration` to a value less than 0, then the animation is paused instead.

* **Additions and Improvements**
  * Additional **Fix Draw Order** parameter at SkeletonRenderer, defaults to `disabled` (previous behaviour).
    Applies only when 3+ submeshes are used (2+ materials with alternating order, e.g. "A B A").
		If `true`, MaterialPropertyBlocks are assigned at each material to prevent aggressive batching of submeshes
		by e.g. the LWRP renderer, leading to incorrect draw order (e.g. "A1 B A2" changed to "A1A2 B").
		You can leave this parameter disabled when everything is drawn correctly to save the additional performance cost.
  * **Additional Timeline features.** SpineAnimationStateClip now provides a `Speed Multiplier`, a start time offset parameter `Clip In`, support for blending successive animations by overlapping tracks. An additional `Use Blend Duration` parameter *(defaults to true)* allows for automatic synchronisation of MixDuration with the current overlap blend duration. An additional Spine preferences parameter `Use Blend Duration` has been added which can be disabled to default to the previous behaviour before this update.
  * Additional `SpriteMask and RectMask2D` example scene added for demonstration of mask setup and interaction.
  * `Real physics hinge chains` for both 2D and 3D physics. The [SkeletonUtilityBone](http://esotericsoftware.com/spine-unity#SkeletonUtilityBone) Inspector provides an interface to create 2D and 3D hinge chains. Previously created chains have only been respecting gravity, but not momentum of the skeleton or parent bones. The new physics rig created when pressing `Create 3D Hinge Chain` and `Create 2D Hinge Chain` creates a more complex setup that also works when flipping the skeleton. Note that the chain root node is no longer parented to bones of the skeleton. This is a requirement in Unity to have momentum applied properly - do not reparent the chain root to bones of your skeleton, or you will loose any momentum applied by the skeleton's movement.
  * `Outline rendering functionality for all shaders.` Every shader now provides an additional set of `Outline` parameters to enable custom outline rendering. When outline rendering is enabled via the `Material` inspector, it automatically switches the shader to the respective `Spine/Outline` shader variant. Outlines are generated by sampling neighbour pixels, so be sure to add enough transparent padding when exporting your atlas textures to fit the desired outline width. In order to enable outline rendering at a skeleton, it is recommended to first prepare an additional outline material copy and then switch the material of the target skeleton to this material. This prevents unnecessary additional runtime material copies and drawcalls. Material switching can be prepared via a [SkeletonRendererCustomMaterials](http://esotericsoftware.com/spine-unity#SkeletonRendererCustomMaterials) component and then enabled or disabled at runtime. Alternatively, you can also directly modify the `SkeletonRenderer.CustomMaterialOverride` property.
  Outline rendering is fully supported on `SkeletonGraphic` shaders as well.
  * Added `SkeletonRenderer.EditorSkipSkinSync` scripting API property to be able to set custom skins in editor scripts. Enable this property when overwriting the Skeleton's skin from an editor script. Without setting this parameter, changes will be overwritten by the next inspector update. Only affects Inspector synchronisation of skin with `initialSkinName`, not startup initialization.
  * `AtlasUtilities.GetRepackedAttachments()` and `AtlasUtilities.GetRepackedSkin()` provide support for additional texture channels such as normal maps via the optional parameter `additionalTexturePropertyIDsToCopy `. See the spine-unity runtime documentation, section [Combining Skins - Advanced - Runtime Repacking with Normalmaps](http://esotericsoftware.com/spine-unity#Combining-Skins) for further info and example usage code.
  * `BoneFollower` can now optionally follow (uniform) world scale of the reference bone. There is now a `Mode` dropdown selector in the Inspector which can be set to either `Local` or `World Uniform`.
  * All `Spine/SkeletonGraphic` shaders now provide a parameter `CanvasGroup Compatible` which can be enabled to support `CanvasGroup` alpha blending. For correct results, you should then disable `Pma Vertex Colors` in the `SkeletonGraphic` Inspector, in section `Advanced` (otherwise Slot alpha will be applied twice).
  * **Now supporting Universal Render Pipeline (URP), including the 2D Renderer pipeline, through an additional UPM package.**
    * **Installation:** You can download the Unity Package Manager (UPM) package via the [download page](http://esotericsoftware.com/spine-unity-download) or find it in the [spine-runtimes/spine-unity/Modules](https://github.com/EsotericSoftware/spine-runtimes/tree/3.8-beta/spine-unity/Modules) subdirectory on the git repository. You can then either unzip (copy if using git) the package to
      * a) the `Packages` directory in your project where it will automatically be loaded, or
      * b) to an arbitrary directory outside the Assets directory and then open Package Manager in Unity, select the `+` icon, choose `Add package from disk..` and point it to the package.json file.

      The Project panel should now show an entry `Spine Universal RP Shaders` under `Packages`. If the directory is not yet listed, you will need to close and re-open Unity to have it display the directory and its contents.
    * **Usage:** The package provides two shaders specifically built for the universal render pipeline:
      * `Universal Render Pipeline/Spine/Skeleton`, as a universal variant of the `Spine/Skeleton` shader,
      * `Universal Render Pipeline/Spine/Skeleton Lit`, as a universal variant of the `Spine/Skeleton Lit` shader,
      * `Universal Render Pipeline/Spine/Sprite`, as a universal variant of the `Spine/Sprite/Vertex Lit` and `Pixel Lit` shaders, which were not functioning in the universal render pipeline,
	  * `Universal Render Pipeline/2D/Spine/Skeleton Lit`, as a universal 2D Renderer variant of the `Spine/Skeleton Lit` shader, and
      * `Universal Render Pipeline/2D/Spine/Sprite`, as a universal 2D Renderer variant of the `Spine/Sprite/Vertex Lit` and `Pixel Lit` shaders.
	  The shaders can be assigned to materials as usual and will respect your settings of the assigned `UniversalRenderPipelineAsset` under `Project Settings - Graphics`.
    * **Restrictions** As all Spine shaders, the URP shaders **do not support `Premultiply alpha` (PMA) atlas textures in Linear color space**. Please export your atlas textures as `straight alpha` textures with disabled `Premultiply alpha` setting when using Linear color space. You can check the current color space via `Project Settings - Player - Other Settings - Color Space.`.
    * **Example:** You can find an example scene in the package under `com.esotericsoftware.spine.urp-shaders-3.8/Examples/URP Shaders.unity` that demonstrates usage of the URP shaders.
  * Spine Preferences now provide an **`Atlas Texture Settings`** parameter for applying customizable texture import settings at all newly imported Spine atlas textures.
    When exporting atlas textures from Spine with `Premultiply alpha` enabled (the default), you can leave it at `PMATexturePreset`. If you have disabled `Premultiply alpha`, set it to the included `StraightAlphaTexturePreset` asset. You can also create your own `TextureImporter` `Preset` asset and assign it here (include `PMA` or `Straight` in the name). In Unity versions before 2018.3 you can use `Texture2D` template assets instead of the newer `Preset` assets. Materials created for imported textures will also have the `Straight Alpha Texture` parameter configured accordingly.
  * All `Sprite` shaders (including URP and LWRP extension packages) now provide an additional `Fixed Normal Space` option `World-Space`. PReviously options were limited to `View-Space` and `Model-Space`.
  * `SkeletonGraphic` now fully supports [`SkeletonUtility`](http://esotericsoftware.com/spine-unity#SkeletonUtility) for generating a hierarchy of [`SkeletonUtilityBones`](http://esotericsoftware.com/spine-unity#SkeletonUtilityBone) in both modes `Follow` and `Override`. This also enables creating hinge chain physics rigs and using `SkeletonUtilityConstraints` such as `SkeletonUtilityGroundConstraint` and `SkeletonUtilityEyeConstraint` on `SkeletonGraphic`.
  * Added **native support for slot blend modes** `Additive`, `Multiply` and `Screen` with automatic assignment at newly imported skeleton assets. `BlendModeMaterialAssets` are now obsolete and replaced by the native properties at `SkeletonDataAsset`. The `SkeletonDataAsset` Inspector provides a new `Blend Modes - Upgrade` button to upgrade an obsolete `BlendModeMaterialAsset` to the native blend modes properties. This upgrade will be performed automatically on imported and re-imported assets.
  * `BoneFollower` and `BoneFollowerGraphic` components now provide better support for following bones when the skeleton's Transform is not the parent of the follower's Transform. Previously e.g. rotating a common parent Transform did not lead to the desired result, as well as negatively scaling a skeleton's Transform when it is not a parent of the follower's Transform.
  * **Linear color space:** Previously Slot colors were not displayed the same in Unity as in the Spine Editor (when configured to display as `Linear` color space in Spine Editor Settings). This is now fixed at all shaders, including URP and LWRP shaders.
  * All Spine shaders (also including URP and LWRP shaders) now support `PMA Vertex Colors` in combination with `Linear` color space. Thus when using Spine shaders, you should always enable `PMA Vertex Colors` at the `SkeletonRenderer` component. This allows using single pass `Additive` Slots rendering. Note that textures shall still be exported as `Straight alpha` when using `Linear` color space, so combine `PMA Vertex Colors` with `Straight Texture`. All `Sprite` shaders now provide an additional blend mode for this, named `PMA Vertex, Straight Texture` which shall be the preferred Sprite shader blend mode in `Linear` color space.
  * Additive Slots have always been lit before they were written to the target buffer. Now all lit shaders provide an additional parameter `Light Affects Additive` which defaults to `false`, as it is the more intuitive default value. You can enable the old behaviour by setting this parameter to `true`.
  * `SkeletonRootMotion` and `SkeletonMecanimRootMotion` components now support arbitrary bones in the hierarchy as `Root Motion Bone`. Previously there were problems when selecting a non-root bone as `Root Motion Bone`. `Skeleton.ScaleX` and `.ScaleY` and parent bone scale is now respected as well.
  * URP and LWRP `Sprite` and `SkeletonLit` shaders no longer require `Advanced - Add Normals` enabled to properly cast and receive shadows. It is recommended to disable `Add Normals` if normals are otherwise not needed.
  * Added an example component `RootMotionDeltaCompensation` located in `Spine Examples/Scripts/Sample Components` which can be used for applying simple delta compensation. You can enable and disable the component to toggle delta compensation of the currently playing animation on and off.
  * `SkeletonRagdoll` and `SkeletonRagdoll2D` now support bone scale at any bone in the skeleton hierarchy. This includes negative scale and root bone scale.
  * `Attachment.GetRemappedClone(Sprite)` method now provides an additional optional parameter `useOriginalRegionScale`. When set to `true`, the replaced attachment's scale is used instead of the Sprite's `Pixel per Unity` setting, allowing for more consistent scaling. *Note:* When remapping Sprites, be sure to set the Sprite's `Mesh Type` to `Full Rect` and not `Tight`, otherwise the scale will be wrong.
  * `SkeletonGraphic` now **supports all Slot blend modes** when `Advanced - Multiple Canvas Renderers` is enabled in the Inspector. The `SkeletonGraphic` Inspector now provides a `Blend Mode Materials` section where you can assign `SkeletonGraphic` materials for each blend mode, or use the new default materials. New `SkeletonGraphic` shaders and materials have been added for each blend mode. The `BlendModes.unity` example scene has been extended to demonstrate this new feature. For detailed information see the [`SkeletonGraphic documentation page`](http://esotericsoftware.com/spine-unity#Parameters).
  * Timeline clips now also offer `Don't End with Clip` and `Clip End Mix Out Duration` parameters. By default when empty space follows the clip on the timeline, the empty animation is set on the track with a MixDuration of `Clip End Mix Out Duration`. Set `Don't End with Clip` to `true` to continue playing the clip's animation instead and mimic the old 3.8 behaviour. If you prefer pausing the animation instead of mixing out to the empty animation, set `Clip End Mix Out Duration` to a value less than 0, then the animation is paused instead.
  * Prefabs containing `SkeletonRenderer`, `SkeletonAnimation` and `SkeletonMecanim` now provide a proper Editor preview, including the preview thumbnail.
  * `SkeletonRenderer` (and subclasses`SkeletonAnimation` and `SkeletonMecanim`) now provide a property `Advanced - Fix Prefab Override MeshFilter`, which when enabled fixes the prefab always being marked as changed. It sets the MeshFilter's hide flags to `DontSaveInEditor`. Unfortunately this comes at the cost of references to the `MeshFilter` by other components being lost, therefore this parameter defaults to `false` to keep the safe existing behaviour.
  * `BoundingBoxFollower` and `BoundingBoxFollowerGraphic` now provide previously missing `usedByEffector` and `usedByComposite` parameters to be set at all generated colliders.
  * `BoneFollower` and `BoneFollowerGraphic` now provide an additional `Follow Parent World Scale` parameter to allow following simple scale of parent bones (rotated/skewed scale can't be supported).
  * Improved `Advanced - Fix Prefab Override MeshFilter` property for `SkeletonRenderer` (and subclasses`SkeletonAnimation` and `SkeletonMecanim`), now providing an additional option to use a global value which can be set in `Edit - Preferences - Spine`.
  * Timeline naming improvements: `Spine AnimationState Clip` Inspector parameter `Custom Duration` changed and inverted to `Default Mix Duration` for more clarity. Shortened all Timeline add track menu entries from: `Spine.Unity.Playables - <track type>` to `Spine - <track type>`, `Spine Animation State Track` to `SkeletonAnimation Track`, `Spine AnimationState Graphic Track` to `SkeletonGraphic Track`, and `Spine Skeleton Flip Track` to `Skeleton Flip Track`.
  * Timeline track appearance and Inspector: Tracks now show icons and track colors to make them easier to distinguish. When a Track is selected, the Inspector now shows an editable track name which was previously only editable at the Timeline asset.
  * Added example component `SkeletonRenderTexture` to render a `SkeletonRenderer` to a `RenderTexture`, mainly for proper transparency. Added an example scene named `RenderTexture FadeOut Transparency` that demonstrates usage for a fadeout transparency effect.
  * Added another fadeout example component named `SkeletonRenderTextureFadeout` which takes over transparency fadeout when enabled. You can use this component as-is, attach it in disabled state and enable it to start a fadeout effect.
  * Timeline clips now offer an additional `Alpha` parameter for setting a custom constant mix alpha value other than 1.0, just as `TrackEntry.Alpha`. Defaults to 1.0.

* **Changes of default values**

* **Deprecated**

* **Restructuring (Non-Breaking)**

### XNA/MonoGame
* Added normalmap support via `SpineEffectNormalmap` and support for loading multiple texture layers following a suffix-pattern. Please see the example code on how to use them.
* Added `Z` property to `SkeletonRenderer` to provide a constant Z offset that's added to all vertices.
* Added `ZSpacing` property to `SkeletonRenderer` to allow specifying the distance on the z-axis between attachments.
* SkeletonDebugRenderer bone color attributes are now public and modifiable by user.


## Java
* Timeline API has been extended to support component-wise timelines exported by Spine 4.0.
* Added `AnimationState.clearNext()` which removes the given `TrackEntry` and all entries after it.
* Added support for texture atlas key value pairs, see `Region.names` and `Region.values`.
* Added support for reverse animation playback via `TrackEntry.getReverse()` and `TrackEntry.setReverse()`.
* Added proportional spacing mode support for path constraints.
* Added support for uniform scaling for two bone IK.
* Fixed applying a constraint reverting changes from other constraints.
* **Breaking change:** Removed `SkeletonData` and `Skeleton` methods: `findBoneIndex`, `findSlotIndex`. Bones and slots have an `index` field that should be used instead.

### libGDX
* Exposed colors in `SkeletonRendererDebug`.
* Updated to libGDX 1.10.0.

## Lua
* Expose non-essential colors on bones, bounding box, clipping, and path attachments.
* Timeline API has been extended to support component-wise timelines exported by Spine 4.0.
* Added `AnimationState.clearNext()` which removes the given `TrackEntry` and all entries after it.
* Added support for texture atlas key value pairs, see `AtlasRegion.names` and `AtlasRegion.values`.
* Added support for reverse animation playback via `TrackEntry.reverse`.
* Added proportional spacing mode support for path constraints.
* Added support for uniform scaling for two bone IK.
* Fixed applying a constraint reverting changes from other constraints.

### Love2D

### Corona
* **Breaking change:** spine-corona has been renamed to spine-solar2d. Change your `require "spine-corona.spine"` statements to `require "spine-solar2d.spine"`

## Typescript/Javascript
* **Breaking change:** refactored to ECMAScript modules. See this [blog post](http://esotericsoftware.com/blog/spine-goes-npm) as well as the updated [README.md](spine-ts/README.md).
* **Breaking change:** the `build/` folder and compiled artifacts are no longer part of the repository. Instead, `npm run build` in `spine-ts/` to generate ECMAScript modules and IIFE modules in `spine-<module-name>/dist`.
* **Breaking change:** the `.npmignore` and `package.json` files in the root directory have been deleted. Use the corresponding files in `spine-ts/` instead, or better, depend on the packages from the NPM registry.
* Updated runtime to be compatible with TypeScript 3.6.3.
* Added `AssetManager#setRawDataURI(path, data)`. Allows to set raw data URIs for a specific path, which in turn enables embedding assets into JavaScript/HTML.
* Expose non-essential colors on bones, bounding box, clipping, and path attachments.
* Timeline API has been extended to support component-wise timelines exported by Spine 4.0.
* Added `AnimationState.clearNext()` which removes the given `TrackEntry` and all entries after it.
* Added support for texture atlas key value pairs, see `AtlasRegion.names` and `AtlasRegion.values`.
* Added support for reverse animation playback via `TrackEntry.reverse`.
* Added proportional spacing mode support for path constraints.
* Added support for uniform scaling for two bone IK.
* Fixed applying a constraint reverting changes from other constraints.
* `AssetManager` constructor now takes an option `Downloader` instance. Used to download assets only once and share them between `AssetManager` instances.
* Added web worker support to `AssetManager`.
* Added various default parameters to `AnimationState` methods for ease of use.
* Added `SpineCanvas`, a simpler way to render a scene via spine-webgl. See `spine-ts/spine-webgl/examples/barebones.html` and `spine-ts/spine-webgl/examples/mix-and-match.html`.

### WebGL backend
* **Breaking change:** removed `SharedAssetManager`. Use `AssetManager` with a shared `Downloader` instance instead.
* **Breaking change:** the global object `spine.webgl` no longer exists. All classes and functions are now exposed on the global `spine` object directly. Simply replace any reference to `spine.webgl.` in your source code with `spine.`.

### Canvas backend
* Renderer now accounts for whitespace stripping.
* **Breaking change:** the global object `spine.canvas` no longer exists. All classes and functions are now exposed on the global `spine` object directly. Simply replace any reference to `spine.canvas.` in your source code with `spine.`.

### Three.js backend
* `SkeletonMesh` now takes an optional `SkeletonMeshMaterialParametersCustomizer` function that allows you to modify the `ShaderMaterialParameters` before the material is finalized. Use it to modify things like THREEJS' `Material.depthTest` etc. See #1590.
* **Breaking change:** the global object `spine.canvas` no longer exists. All classes and functions are now exposed on the global `spine` object directly. Simply replace any reference to `spine.threejs.` in your source code with `spine.`.
* **Breaking change:** the default fragment shader of `SkeletonMeshMaterial` now explicitely discards fragments with alpha < 0.5. See https://github.com/EsotericSoftware/spine-runtimes/issues/1985
* **Breaking change:** reversal of the previous breaking change: the default fragment shader of `SkeletonMeshMaterial` does no longer discard fragments with alpha < 0.5. Pass a `SkeletonMeshMaterialParametersCustomizer` to the `SkeletonMesh` constructor, and modify `parameters.alphaTest` to be > 0.

### Player
* Added `SpinePlayerConfig.rawDataURIs`. Allows to embed data URIs for skeletons, atlases and atlas page images directly in the HTML/JS without needing to load it from a separate file. See the example for a demonstration.
* Added `SpinePlayerConfig.frame`. If set, the callback is called each frame, before the skeleton is posed or drawn.
* Added `SpinePlayerConfig.update`. If set, the callback is called each frame, just after the skeleton is posed.
* Added `SpinePlayerConfig.draw`. If set, the callback is called each frame, just after the skeleton is drawn.
* Added `SpinePlayerConfig.downloader`. The `spine.Downloader` instance can be shared between players so assets are only downloaded once.
* If `SpinePlayerConfig.jsonURL` ends with an anchor, the anchor text is used to find the skeleton in the specified JSON file.
* Added `SpinePlayer.dispose()`, disposes all CPU and GPU side resources, removes all listeners, and removes the player DOM from the parent.

# 3.8

## AS3
* **Breaking changes**
  * Renamed `Slot#getAttachmentVertices()` to `Slot#getDeform()`.
  * Changed the `.json` curve format and added more assumptions for omitted values, reducing the average size of JSON exports.
  * Renamed `Skin#addAttachment()` to `Skin#setAttachment()`.
  * Removed `VertexAttachment#applyDeform()` and replaced it with `VertexAttachment#deformAttachment`. The attachment set on this field is used to decide if a `DeformTimeline` should be applied to the attachment active on the slot to which the timeline is applied.
  * Removed `inheritDeform` field, getter, and setter from `MeshAttachment`.
  * Changed `.skel` binary format, added a string table. References to strings in the data resolve to this string table, reducing storage size of binary files considerably.
  * Changed the `.json` and `.skel` file formats to accomodate the new feature and file size optimiations. Old projects must be exported with Spine 3.8.20+ to be compatible with the 3.8 Spine runtimes.
  * Switched projects from FDT to Visual Studio Code. See updated `README.md` files for instructions.

* **Additions**
  * Added `SkeletonBinary` to load binary `.skel` files. See `MixAndMatchExample.as` in `spine-startling-example`.
  * Added `x` and `y` coordinates for setup pose AABB in `SkeletonData`.
  * Added support for rotated mesh region UVs.
  * Added skin-specific bones and constraints which are only updated if the skeleton's current skin contains them.
  * Improved Skin API to make it easier to handle mix-and-match use cases.
    * Added `Skin#getAttachments()`. Returns all attachments in the skin.
    * Added `Skin#getAttachments(int slotIndex)`. Returns all attachements in the skin for the given slot index.
    * Added `Skin#addSkin(Skin skin)`. Adds all attachments, bones, and skins from the specified skin to this skin.
    * Added `Skin#copySkin(Skin skin)`. Adds all attachments, bones, and skins from the specified skin to this skin. `VertexAttachment` are shallowly copied and will retain any parent mesh relationship. All other attachment types are deep copied.
  * Added `Attachment#copy()` to all attachment type implementations. This lets you deep copy an attachment to modify it independently from the original, i.e. when programmatically changing texture coordinates or mesh vertices.
  * Added `MeshAttachment#newLinkedMesh()`, creates a linked mesh linkted to either the original mesh, or the parent of the original mesh.
  * Added IK softness.

### Starling
* Added `MixAndMatchExample.as` to demonstrate the new Skin API additions and how to load binary `.skel` files.
* Switched projects from FDT to Visual Studio Code. See updated `README.md` files for instructions.

## C
* **Breaking changes**
  * Renamed `spSlot#attachmentVertices` to `spSlot#deform`.
  * Changed the `.json` curve format and added more assumptions for omitted values, reducing the average size of JSON exports.
  * Renamed `spSkin_addAttachment()` to `Skin#spSkin_addAttachment()`.
  * Removed `spVertexAttachment_applyDeform()` and replaced it with `VertexAttachment#deformAttachment`. The attachment set on this field is used to decide if a `spDeformTimeline` should be applied to the attachment active on the slot to which the timeline is applied.
  * Removed `inheritDeform` field, getter, and setter from `spMeshAttachment`.
  * Changed `.skel` binary format, added a string table. References to strings in the data resolve to this string table, reducing storage size of binary files considerably.
  * Changed the `.json` and `.skel` file formats to accomodate the new feature and file size optimiations. Old projects must be exported with Spine 3.8.20+ to be compatible with the 3.8 Spine runtimes.

* **Additions**
  * Added `x` and `y` coordinates for setup pose AABB in `spSkeletonData`.
  * Added support for rotated mesh region UVs.
  * Added skin-specific bones and constraints which are only updated if the skeleton's current skin contains them.
  * Improved Skin API to make it easier to handle mix-and-match use cases.
    * Added `spSkin_getAttachments()`. Returns all attachments in the skin.
    * Added `spSkin_getAttachments(int slotIndex)`. Returns all attachements in the skin for the given slot index.
    * Added `spSkin_addSkin(spSkin* skin)`. Adds all attachments, bones, and skins from the specified skin to this skin.
    * Added `spSkin_copySkin(spSkin* skin)`. Adds all attachments, bones, and skins from the specified skin to this skin. `spVertexAttachment` are shallowly copied and will retain any parent mesh relationship. All other attachment types are deep copied.
    * All attachments inserted into skins are reference counted. When the last skin referencing an attachment is disposed, the attachment will also be disposed.
  * Added `spAttachment_copy()` to all attachment type implementations. This lets you deep copy an attachment to modify it independently from the original, i.e. when programmatically changing texture coordinates or mesh vertices.
  * Added `spMeshAttachment_newLinkedMesh()`, creates a linked mesh linkted to either the original mesh, or the parent of the original mesh.
  * Added IK softness.

### Cocos2d-Objc
* Added mix-and-match example to demonstrate the new Skin API.
* Added `IKExample`.
* Added `SkeletonAnimation preUpdateWorldTransformsListener` and `SkeletonAnimation postUpdateWorldTransformsListener`. When set, these callbacks will be invokved before and after the skeleton's `updateWorldTransforms()` method is called. See the `IKExample` how it can be used.

### SFML
* Added mix-and-match example to demonstrate the new Skin API.
* Added `IKExample`.

## C++
* **Breaking Changes**
  * Renamed `Slot::getAttachmentVertices()` to `Slot::getDeform()`.
  * Changed the `.json` curve format and added more assumptions for omitted values, reducing the average size of JSON exports.
  * Renamed `Skin::addAttachment()` to `Skin::setAttachment()`.
  * Removed `VertexAttachment::applyDeform()` and replaced it with `VertexAttachment::getDeformAttachment()`. The attachment set on this field is used to decide if a `DeformTimeline` should be applied to the attachment active on the slot to which the timeline is applied.
  * Removed `_inheritDeform` field, getter, and setter from `MeshAttachment`.
  * Changed `.skel` binary format, added a string table. References to strings in the data resolve to this string table, reducing storage size of binary files considerably.
  * Changed the `.json` and `.skel` file formats to accomodate the new feature and file size optimiations. Old projects must be exported with Spine 3.8.20+ to be compatible with the 3.8 Spine runtimes.

* **Additions**
  * `AnimationState` and `TrackEntry` now also accept a subclass of `AnimationStateListenerObject` as a listener for animation events in the overloaded `setListener()` method.
  * `SkeletonBinary` and `SkeletonJson` now parse and set all non-essential data like audio path.
  * Added `x` and `y` coordinates for setup pose AABB in `SkeletonData`.
  * Added support for rotated mesh region UVs.
  * Added skin-specific bones and constraints which are only updated if the skeleton's current skin contains them.
  * Improved Skin API to make it easier to handle mix-and-match use cases.
    * Added `Skin#getAttachments()`. Returns all attachments in the skin.
    * Added `Skin#getAttachments(int slotIndex)`. Returns all attachements in the skin for the given slot index.
    * Added `Skin#addSkin(Skin &skin)`. Adds all attachments, bones, and skins from the specified skin to this skin.
    * Added `Skin#copySkin(Skin &skin)`. Adds all attachments, bones, and skins from the specified skin to this skin. `VertexAttachment` are shallowly copied and will retain any parent mesh relationship. All other attachment types are deep copied.
    * All attachments inserted into skins are reference counted. When the last skin referencing an attachment is disposed, the attachment will also be disposed.
  * Added `Attachment#copy()` to all attachment type implementations. This lets you deep copy an attachment to modify it independently from the original, i.e. when programmatically changing texture coordinates or mesh vertices.
  * Added `MeshAttachment#newLinkedMesh()`, creates a linked mesh linkted to either the original mesh, or the parent of the original mesh.
  * Added IK softness.
  * Exposed `x` and `y` on `SkeletonData` through getters and setters.

### Cocos2d-x
* Updated to cocos2d-x 3.17.1
* Added mix-and-match example to demonstrate the new Skin API.
* Exmaple project requires Visual Studio 2019 on Windows
* Added `IKExample`.
* Added `SkeletonAnimation::setPreUpdateWorldTransformsListener()` and `SkeletonAnimation::setPreUpdateWorldTransformsListener()`. When set, these callbacks will be invokved before and after the skeleton's `updateWorldTransforms()` method is called. See the `IKExample` how it can be used.

### SFML
* Added mix-and-match example to demonstrate the new Skin API.

### UE4
* Added `bAutoPlaying` flag to `USpineSkeletonAnimationComponent`. When `false`, the component will not update the internal animation state and skeleton.
* Updated example project to UE 4.22.
* (Re-)Importing Spine assets will perform a version compatibility check and alert users about mismatches in editor mode.
* `USpineSkeletonRendererComponent` allows passing a `USpineSkeletonComponent` to update it. This way, the renderer component can be used without a skeleton component on the same actor.
* Added blueprint-callable methods to `SpineSkeletonComponent` and `SpineSkeletonAnimationComponent` to query and set skins, and enumerate bones, slots, and animations.
* Extended skeleton data editor preview. The preview now shows bones, slots, animations, and skins found in the skeleton data. See this [blog post](http://esotericsoftware.com/blog/Unreal-Engine-4-quality-of-life-improvements).
* Added preview animation and skin fields, allowing you to preview animations and skins right in the editor. See this [blog post](http://esotericsoftware.com/blog/Unreal-Engine-4-quality-of-life-improvements).
* Removed dependency on `RHI`, `RenderCore`, and `ShaderCore`.
* Re-importing atlases and their textures now works consistently in all situations.
* Added mix-and-match example to demonstrate the new Skin API.
* Materials on `SkeletonRendererComponent` are now blueprint read and writeable. This allows setting dynamic material instances at runtime.
* Added `InitialSkin` property to `USpineWidget`. This allows previewing different skins in the UMG Designer. Initial skins can still be overridden via blueprint events such as `On Initialized`.

## C# ##
* **Breaking changes**
  * **Changed `IkConstraintData.Bones` type from `List<BoneData>` to `ExposedList<BoneData>`** for unification reasons. *Note: this modification will most likely not affect user code.*
  * Renamed `Slot.AttachmentVertices` to `Slot.Deform`.
  * Changed the `.json` curve format and added more assumptions for omitted values, reducing the average size of JSON exports.
  * Renamed `Skin.AddAttachment()` to `Skin.SetAttachment()`.
  * Removed `FindAttachmentsForSlot(int slotIndex, List<Attachment> attachments)` and `FindNamesForSlot (int slotIndex, List<string> names)` and replaced it with `Skin.GetAttachments(int slotIndex, List<SkinEntry> attachments)` which returns the combined `SkinEntry` object holding both name and attachment.
  * Removed `VertexAttachment.ApplyDeform()` and replaced it with `VertexAttachment.DeformAttachment`. The attachment set on this field is used to decide if a `DeformTimeline` should be applied to the attachment active on the slot to which the timeline is applied.
  * Removed `inheritDeform` field, getter, and setter from `MeshAttachment`.
  * Changed `.skel` binary format, added a string table. References to strings in the data resolve to this string table, reducing storage size of binary files considerably.
  * Changed the `.json` and `.skel` file formats to accomodate the new feature and file size optimiations. Old projects must be exported with Spine 3.8.20+ to be compatible with the 3.8 Spine runtimes.

* **Additions**
  * Added `x` and `y` coordinates for setup pose AABB in `SkeletonData`.
  * Added support for rotated mesh region UVs.
  * Added skin-specific bones and constraints which are only updated if the skeleton's current skin contains them.
  * Improved Skin API to make it easier to handle mix-and-match use cases.
    * Added `Skin.GetAttachments()`. Returns all attachments in the skin.
    * Added `Skin.GetAttachments(int slotIndex, List<SkinEntry> attachments)`. Returns all attachements in the skin for the given slot index. This method replaces `FindAttachmentsForSlot` and `FindNamesForSlot`.
    * Added `Skin.AddSkin(Skin skin)`. Adds all attachments, bones, and skins from the specified skin to this skin.
    * Added `Skin.CopySkin(Skin skin)`. Adds all attachments, bones, and skins from the specified skin to this skin. `VertexAttachment` are shallowly copied and will retain any parent mesh relationship. All other attachment types are deep copied.
  * Added `Attachment.Copy()` to all attachment type implementations. This lets you deep copy an attachment to modify it independently from the original, i.e. when programmatically changing texture coordinates or mesh vertices.
  * Added `MeshAttachment.NewLinkedMesh()`, creates a linked mesh linkted to either the original mesh, or the parent of the original mesh.
  * Added IK softness.

### Unity

* **Breaking changes**
  * **Officially supported Unity versions are 2017.1-2020.2**.
  * **Spine `.asmdef` files are again active by default**. They have previously been deactivated to `.txt` extension which is now no longer necessary.
  * **Removed PoseSkeleton() and PoseWithAnimation()** extension methods to prevent issues where animations are not mixed out. Problem was that these methods did not set AnimationState, leaving incorrect state at e.g. attachments enabled at slots when starting subsequent animations. As a replacement you can use `AnimationState.ClearTrack(0);` followed by `var entry = AnimationState.SetAnimation(0, animation, loop); entry.TrackTime = time` to achieve similar behaviour.
  * **The `Shadow alpha cutoff` shader parameter is now respecting slot-color alpha** values at all Spine shaders. A fragment's texture color alpha is multiplied with slot-color alpha before the result is tested against the `Shadow alpha cutoff` threshold.
  * **Removed redundant `Attachment.GetClone()` and `MeshAttachment.GetLinkedClone()` extension methods**. Use methods `Attachment.Copy` and `MeshAttachment.NewLinkedMesh()` instead.
  * **Renamed extension method `Attachment.GetClone(bool cloneMeshesAsLinked)` to `Attachment.GetCopy(bool cloneMeshesAsLinked)`** to follow the naming scheme of the Spine API.
  * `SkeletonDataAsset.atlasAssets` is now an array of the base class `AtlasAssetBase` instead of `SpineAtlasAsset`, which provides `IEnumerable<> Materials` instead of `List<> materials`. Replace any access via `atlasAsset.materials[0]` with `atlasAsset.Materials.First()` and add a `using System.Linq;` statement.
  * **Changed `MeshAttachment.GetLinkedMesh()` method signatures:** removed optional parameters `bool inheritDeform = true, bool copyOriginalProperties = false`.
  * Changed namespace `Spine.Unity.Modules` to `Spine.Unity` and `Spine.Unity.Examples` after restructuring (see section below) in respective classes:
    * When receiving namespace related errors, replace using statements of `using Spine.Unity.Modules.AttachmentTools;` with `using Spine.Unity.AttachmentTools;`. You can remove `using Spine.Unity.Modules;` statements when a `using Spine.Unity` statement is already present in the file.
    * `AttachmentTools`, `SkeletonPartsRenderer`, `SkeletonRenderSeparator`, `SkeletonRendererCustomMaterials` changed to namespace `Spine.Unity`.
    * `SkeletonGhost`, `SkeletonGhostRenderer`, `AtlasRegionAttacher`, `SkeletonGraphicMirror`, `SkeletonRagdoll`, `SkeletonRagdoll2D`, `SkeletonUtilityEyeConstraint`, `SkeletonUtilityGroundConstraint`, `SkeletonUtilityKinematicShadow` changed to namespace `Spine.Unity.Examples`.
  * Split `Editor/Utility/SpineEditorUtilities` class into multiple files with partial class qualifier.
    * Nested classes `SpineEditorUtilities.AssetUtility` and `SpineEditorUtilities.EditorInstantiation` are now no longer nested. If you receive namespace related errors, replace any occurrance of
      * `SpineEditorUtilities.AssetUtility` with `AssetUtility` and
      * `SpineEditorUtilities.EditorInstantiation` with `EditorInstantiation`.
  * **Timeline Support has been moved to a separate UPM Package** Previously the Spine Timeline integration was located in the `Modules/Timeline` directory and was deactivated by default, making it necessary to activate it via the Spine Preferences. Now the Timeline integration has been moved to an additional UPM package which can be found under `Modules/com.esotericsoftware.spine.timeline`.
   * **Installation:** You can download the Unity Package Manager (UPM) package via the [download page](http://esotericsoftware.com/spine-unity-download) or find it in the [spine-runtimes/spine-unity/Modules](https://github.com/EsotericSoftware/spine-runtimes/tree/3.8-beta/spine-unity/Modules) subdirectory on the git repository. You can then either unzip (copy if using git) the package to
      a) the `Packages` directory in your project where it will automatically be loaded, or
      b) to an arbitrary directory outside the Assets directory and then open Package Manager in Unity, select the `+` icon, choose `Add package from disk..` and point it to the package.json file.
      The Project panel should now show an entry `Spine Timeline Extensions` under `Packages`. If the directory is not yet listed, you will need to close and re-open Unity to have it display the directory and its contents.
  * `SkeletonMecanim`'s `Layer Mix Mode` enum name `MixMode.SpineStyle` has been renamed to `MixMode.Hard`. This is most likely not set via code and thus unlikely to be a problem. Serialized scenes and prefabs are unaffected.
  * `SkeletonRootMotion` and `SkeletonMecanimRootMotion` components now support arbitrary bones in the hierarchy as `Root Motion Bone`. Previously there were problems when selecting a non-root bone as `Root Motion Bone`. `Skeleton.ScaleX` and `.ScaleY` and parent bone scale is now respected as well.

* **Additions**
  * **Spine Preferences stored in Assets/Editor/SpineSettings.asset** Now Spine uses the new `SettingsProvider` API, storing settings in a SpineSettings.asset file which can be shared with team members. Your old preferences are automatically migrated to the new system.
  * Added support for Unity's SpriteMask to `SkeletonAnimation` and `SkeletonMecanim`. All mask interaction modes are supported. See this [blog post](http://esotericsoftware.com/blog/Unity-SpriteMask-and-RectMask2D-support).
  * Added support for Unity's RectMask2D to SkeletonGraphics. See this [blog post](http://esotericsoftware.com/blog/Unity-SpriteMask-and-RectMask2D-support).
  * Added `Create 2D Hinge Chain` button at `SkeletonUtilityBone` inspector, previously only `Create 3D Hinge Chain` was available.
  * **Now supporting Lightweight Render Pipeline (LWRP) through an additional UPM package.**
    * **Installation:** You can download the Unity Package Manager (UPM) package via the [download page](http://esotericsoftware.com/spine-unity-download) or find it in the [spine-runtimes/spine-unity/Modules](https://github.com/EsotericSoftware/spine-runtimes/tree/3.8-beta/spine-unity/Modules) subdirectory on the git repository. You can then either unzip (copy if using git) the package to
      * a) the `Packages` directory in your project where it will automatically be loaded, or
      * b) to an arbitrary directory outside the Assets directory and then open Package Manager in Unity, select the `+` icon, choose `Add package from disk..` and point it to the package.json file.

      > If you are using git and Unity 2019.2 or newer versions and receive an error that dependencies could not be resolved by the package manager (only higher versions of Unity's `Lightweight RP` package are available, e.g. `6.9.0` and up), please copy the prepared package-UNITYVERSION.json file for your Unity version (e.g. `package-2019.2.json`) over the existing package.json file to change the dependency accordingly. Unfortunately Unity's Package Manager does not provide a way to specify a version range for a dependency like "5.7.2 - 6.9.0" yet, so this manual step is necessary for git users.

      The Project panel should now show an entry `Spine Lightweight RP Shaders` under `Packages`. If the directory is not yet listed, you will need to close and re-open Unity to have it display the directory and its contents.
    * **Usage:** The package provides two shaders specifically built for the lightweight render pipeline:
      * `Lightweight Render Pipeline/Spine/Skeleton`, as a lightweight variant of the `Spine/Skeleton` shader,
      * `Lightweight Render Pipeline/Spine/Skeleton Lit`, as a lightweight variant of the `Spine/Skeleton Lit` shader and
      * `Lightweight Render Pipeline/Spine/Sprite`, as a lightweight variant of the `Spine/Sprite/Vertex Lit` and `Pixel Lit` shaders, which were not functioning in the lightweight render pipeline. The shaders can be assigned to materials as usual and will respect your settings of the assigned `LightweightRenderPipelineAsset` under `Project Settings - Graphics`.
    * **Restrictions** As all Spine shaders, the LWRP shaders **do not support `Premultiply alpha` (PMA) atlas textures in Linear color space**. Please export your atlas textures as `straight alpha` textures with disabled `Premultiply alpha` setting when using Linear color space. You can check the current color space via `Project Settings - Player - Other Settings - Color Space.`.
    * **Example:** You can find an example scene in the package under `com.esotericsoftware.spine.lwrp-shaders-3.8/Examples/LWRP Shaders.unity` that demonstrates usage of the LWRP shaders.
  * Added `Spine/Skeleton Lit ZWrite` shader. This variant of the `Spine/Skeleton Lit` shader writes to the depth buffer with configurable depth alpha threshold. Apart from that it is identical to `Spine/Skeleton Lit`.
  * Additional yield instructions to wait for animation track events `End`, `Complete` and `Interrupt`.
    * `WaitForSpineAnimationComplete` now proves an additional `bool includeEndEvent` parameter, defaults to `false` (previous behaviour).
    * Added a new `WaitForSpineAnimationEnd` yield instruction.
    * Added a new generic `WaitForSpineAnimation` yield instruction which can be configured to wait for any combination of animation track events. It is now used as base class for `WaitForSpineAnimationComplete` and `WaitForSpineAnimationEnd`.
  * Additional **Fix Draw Order** parameter at SkeletonRenderer, defaults to `disabled` (previous behaviour).
    Applies only when 3+ submeshes are used (2+ materials with alternating order, e.g. "A B A").
		If true, MaterialPropertyBlocks are assigned at each material to prevent aggressive batching of submeshes
		by e.g. the LWRP renderer, leading to incorrect draw order (e.g. "A1 B A2" changed to "A1A2 B").
		You can leave this parameter disabled when everything is drawn correctly to save the additional performance cost.
  * **Additional Timeline features.** SpineAnimationStateClip now provides a `Speed Multiplier`, a start time offset parameter `Clip In`, support for blending successive animations by overlapping tracks. An additional `Use Blend Duration` parameter *(defaults to true)* allows for automatic synchronisation of MixDuration with the current overlap blend duration. An additional Spine preferences parameter `Use Blend Duration` has been added which can be disabled to default to the previous behaviour before this update.
  * Additional `SpriteMask and RectMask2D` example scene added for demonstration of mask setup and interaction.
  * `Real physics hinge chains` for both 2D and 3D physics. The [SkeletonUtilityBone](http://esotericsoftware.com/spine-unity#SkeletonUtilityBone) Inspector provides an interface to create 2D and 3D hinge chains. Previously created chains have only been respecting gravity, but not momentum of the skeleton or parent bones. The new physics rig created when pressing `Create 3D Hinge Chain` and `Create 2D Hinge Chain` creates a more complex setup that also works when flipping the skeleton. Note that the chain root node is no longer parented to bones of the skeleton. This is a requirement in Unity to have momentum applied properly - do not reparent the chain root to bones of your skeleton, or you will loose any momentum applied by the skeleton's movement.
  * `Outline rendering functionality for all shaders.` Every shader now provides an additional set of `Outline` parameters to enable custom outline rendering. When outline rendering is enabled via the `Material` inspector, it automatically switches the shader to the respective `Spine/Outline` shader variant. Outlines are generated by sampling neighbour pixels, so be sure to add enough transparent padding when exporting your atlas textures to fit the desired outline width. In order to enable outline rendering at a skeleton, it is recommended to first prepare an additional outline material copy and then switch the material of the target skeleton to this material. This prevents unnecessary additional runtime material copies and drawcalls. Material switching can be prepared via a [SkeletonRendererCustomMaterials](http://esotericsoftware.com/spine-unity#SkeletonRendererCustomMaterials) component and then enabled or disabled at runtime. Alternatively, you can also directly modify the `SkeletonRenderer.CustomMaterialOverride` property.
  Outline rendering is fully supported on `SkeletonGraphic` shaders as well.
  * Added `SkeletonRenderer.EditorSkipSkinSync` scripting API property to be able to set custom skins in editor scripts. Enable this property when overwriting the Skeleton's skin from an editor script. Without setting this parameter, changes will be overwritten by the next inspector update. Only affects Inspector synchronisation of skin with `initialSkinName`, not startup initialization.
  * All `Spine/SkeletonGraphic` shaders now provide a parameter `CanvasGroup Compatible` which can be enabled to support `CanvasGroup` alpha blending. For correct results, you should then disable `Pma Vertex Colors` in the `SkeletonGraphic` Inspector, in section `Advanced` (otherwise Slot alpha will be applied twice).
  * **Now supporting Universal Render Pipeline (URP), including the 2D Renderer pipeline, through an additional UPM package.**
    * **Installation:** You can download the Unity Package Manager (UPM) package via the [download page](http://esotericsoftware.com/spine-unity-download) or find it in the [spine-runtimes/spine-unity/Modules](https://github.com/EsotericSoftware/spine-runtimes/tree/3.8-beta/spine-unity/Modules) subdirectory on the git repository. You can then either unzip (copy if using git) the package to
      * a) the `Packages` directory in your project where it will automatically be loaded, or
      * b) to an arbitrary directory outside the Assets directory and then open Package Manager in Unity, select the `+` icon, choose `Add package from disk..` and point it to the package.json file.

      The Project panel should now show an entry `Spine Universal RP Shaders` under `Packages`. If the directory is not yet listed, you will need to close and re-open Unity to have it display the directory and its contents.
    * **Usage:** The package provides two shaders specifically built for the universal render pipeline:
      * `Universal Render Pipeline/Spine/Skeleton`, as a universal variant of the `Spine/Skeleton` shader,
      * `Universal Render Pipeline/Spine/Skeleton Lit`, as a universal variant of the `Spine/Skeleton Lit` shader,
      * `Universal Render Pipeline/Spine/Sprite`, as a universal variant of the `Spine/Sprite/Vertex Lit` and `Pixel Lit` shaders, which were not functioning in the universal render pipeline,
	  * `Universal Render Pipeline/2D/Spine/Skeleton Lit`, as a universal 2D Renderer variant of the `Spine/Skeleton Lit` shader, and
      * `Universal Render Pipeline/2D/Spine/Sprite`, as a universal 2D Renderer variant of the `Spine/Sprite/Vertex Lit` and `Pixel Lit` shaders.
	  The shaders can be assigned to materials as usual and will respect your settings of the assigned `UniversalRenderPipelineAsset` under `Project Settings - Graphics`.
    * **Restrictions** As all Spine shaders, the URP shaders **do not support `Premultiply alpha` (PMA) atlas textures in Linear color space**. Please export your atlas textures as `straight alpha` textures with disabled `Premultiply alpha` setting when using Linear color space. You can check the current color space via `Project Settings - Player - Other Settings - Color Space.`.
    * **Example:** You can find an example scene in the package under `com.esotericsoftware.spine.urp-shaders-3.8/Examples/URP Shaders.unity` that demonstrates usage of the URP shaders.
  * Spine Preferences now provide an **`Atlas Texture Settings`** parameter for applying customizable texture import settings at all newly imported Spine atlas textures.
    When exporting atlas textures from Spine with `Premultiply alpha` enabled (the default), you can leave it at `PMATexturePreset`. If you have disabled `Premultiply alpha`, set it to the included `StraightAlphaTexturePreset` asset. You can also create your own `TextureImporter` `Preset` asset and assign it here (include `PMA` or `Straight` in the name). In Unity versions before 2018.3 you can use `Texture2D` template assets instead of the newer `Preset` assets. Materials created for imported textures will also have the `Straight Alpha Texture` parameter configured accordingly.
  * All `Sprite` shaders (including URP and LWRP extension packages) now provide an additional `Fixed Normal Space` option `World-Space`. PReviously options were limited to `View-Space` and `Model-Space`.
  * `SkeletonGraphic` now fully supports [`SkeletonUtility`](http://esotericsoftware.com/spine-unity#SkeletonUtility) for generating a hierarchy of [`SkeletonUtilityBones`](http://esotericsoftware.com/spine-unity#SkeletonUtilityBone) in both modes `Follow` and `Override`. This also enables creating hinge chain physics rigs and using `SkeletonUtilityConstraints` such as `SkeletonUtilityGroundConstraint` and `SkeletonUtilityEyeConstraint` on `SkeletonGraphic`.
  * Added `OnMeshAndMaterialsUpdated` callback event to `SkeletonRenderer` and `SkeletonGraphic`. It is issued at the end of `LateUpdate`, before rendering.
  * Added `Skeleton-OutlineOnly` single pass shader to LWRP and URP extension modules. It can be assigned to materials as `Universal Render Pipeline/Spine/Outline/Skeleton-OutlineOnly`. This allows for separate outline child *GameObjects* that reference the existing Mesh of their parent, and re-draw the mesh using this outline shader.
  * Added example component `RenderExistingMesh` to render a mesh again with different materials, as required by the new `Skeleton-OutlineOnly` shaders.
  In URP the outline has to be rendered via a separate GameObject as URP does not allow multiple render passes. To add an outline to your SkeletenRenderer:
    1) Add a child GameObject and move it a bit back (e.g. position Z = 0.01).
    2) Add a `RenderExistingMesh` component, provided in the `Spine Examples/Scripts/Sample Components` directory.
    3) Copy the original material, add *_Outline* to its name and set the shader to `Universal Render Pipeline/Spine/Outline/Skeleton-OutlineOnly`.
    4) Assign this *_Outline* material at the `RenderExistingMesh` component under *Replacement Materials*.
  * Added `Outline Shaders URP` example scene to URP extension module to demonstrate the above additions.
  * Added support for Unity's [`SpriteAtlas`](https://docs.unity3d.com/Manual/class-SpriteAtlas.html) as atlas provider (as an alternative to `.atlas.txt` and `.png` files) alongside a skeleton data file. There is now an additional `Spine SpriteAtlas Import` tool window accessible via `Window - Spine - SpriteAtlas Import`. Additional information can be found in a new section on the [spine-unity documentation page](http://esotericsoftware.com/spine-unity#Advanced---Using-Unity-SpriteAtlas-as-Atlas-Provider).
  * Added support for **multiple atlas textures at `SkeletonGraphic`**. You can enable this feature by enabling the parameter `Multiple CanvasRenders` in the `Advanced` section of the `SkeletonGraphic` Inspector. This automatically creates the required number of child `CanvasRenderer` GameObjects for each required draw call (submesh).
  * Added support for **Render Separator Slots** at `SkeletonGraphic`. Render separation can be enabled directly in the `Advanced` section of the `SkeletonGraphic` Inspector, it does not require any additional components (like `SkeletonRenderSeparator` or `SkeletonPartsRenderer` for `SkeletonRenderer` components). When enabled, additional separator GameObjects will be created automatically for each separation part, and `CanvasRenderer` GameObjects re-parented to them accordingly. The separator GameObjects can be moved around and re-parented in the hierarchy according to your requirements to achieve the desired draw order within your `Canvas`. A usage example can be found in the updated `Spine Examples/Other Examples/SkeletonRenderSeparator` scene.
  * Added `SkeletonGraphicCustomMaterials` component, providing functionality to override materials and textures of a `SkeletonGraphic`, similar to `SkeletonRendererCustomMaterials`. Note: overriding materials or textures per slot is not provided due to structural limitations.
  * Added **Root Motion support** for `SkeletonAnimation`, `SkeletonMecanim` and `SkeletonGraphic` via new components `SkeletonRootMotion` and `SkeletonMecanimRootMotion`. The `SkeletonAnimation` and `SkeletonGraphic` component Inspector now provides a line `Root Motion` with `Add Component` and `Remove Component` buttons to add/remove the new `SkeletonRootMotion` component to your GameObject. The `SkeletonMecanim` Inspector detects whether root motion is enabled at the `Animator` component and adds a `SkeletonMecanimRootMotion` component automatically.
  * `SkeletonMecanim` now provides an additional `Custom MixMode` parameter under `Mecanim Translator`. It is enabled by default in version 3.8 to maintain current behaviour, using the set `Mix Mode` for each Mecanim layer. When disabled, `SkeletonMecanim` will use the recommended `MixMode` according to the layer blend mode. Additional information can be found in the [Mecanim Translator section](http://esotericsoftware.com/spine-unity#Parameters-for-animation-blending-control) on the spine-unity documentation pages.
  * Added **SkeletonGraphic Timeline support**. Added supprot for multi-track Timeline preview in the Editor outside of play mode (multi-track scrubbing). See the [Timeline-Extension-UPM-Package](http://esotericsoftware.com/spine-unity#Timeline-Extension-UPM-Package) section of the spine-unity documentation for more information.
  * Added support for double-sided lighting at all `SkeletonLit` shaders (including URP and LWRP packages).
  * Added frustum culling update mode parameters `Update When Invisible` (Inspector parameter) and `UpdateMode` (available via code) to all Skeleton components. This provides a simple way to disable certain updates when the `Renderer` is no longer visible (outside all cameras, culled in frustum culling). The new `UpdateMode` property allows disabling updates at a finer granularity level than disabling the whole component. Available modes are: `Nothing`, `OnlyAnimationStatus`, `EverythingExceptMesh` and `FullUpdate`.
  * Added a new `Spine/Outline/OutlineOnly-ZWrite` shader to provide correct outline-only rendering. Note: the shader requires two render passes and is therefore not compatible with URP. The `Spine Examples/Other Examples/Outline Shaders` example scene has been updated to demonstrate the new shader.
  * Added `OnMeshAndMaterialsUpdated` callback event to `SkeletonRenderSeparator` and `SkeletonPartsRenderer`. It is issued at the end of `LateUpdate`, before rendering.
  * Added `Root Motion Scale X/Y` parameters to `SkeletonRootMotionBase` subclasses (`SkeletonRootMotion` and `SkeletonMecanimRootMotion`). Also providing `AdjustRootMotionToDistance()` and other methods to allow for easy delta compensation. Delta compensation can be used to e.g. stretch a jump to a given distance. Root motion can be adjusted at the start of an animation or every frame via `skeletonRootMotion.AdjustRootMotionToDistance(targetPosition - transform.position, trackIndex);`.
  * Now providing a `Canvas Group Tint Black` parameter at the `SkeletonGraphic` Inspector in the `Advanced` section. When using the `Spine/SkeletonGraphic Tint Black` shader you can enable this parameter to receive proper blending results when using `Additive` blend mode under a `CanvasGroup`. Be sure to also have the parameter `CanvasGroup Compatible` enabled at the shader. Note that the normal `Spine/SkeletonGraphic` does not support `Additive` blend mode at a `CanvasGroup`, as it requires additional shader channels to work.
  * Added `Mix and Match Skins` example scene to demonstrate how the 3.8 Skin API and combining skins can be used for a wardrobe and equipment use case.
  * Spine Timeline Extensions: Added `Hold Previous` parameter at `SpineAnimationStateClip`.
  * Added more warning messages at incompatible SkeletonRenderer/SkeletonGraphic Component vs Material settings. They appear both as an info box in the Inspector as well as upon initialization in the Console log window. The Inspector box warnings can be disabled via `Edit - Preferences - Spine`.
  * Now providing `BeforeApply` update callbacks at all skeleton animation components (`SkeletonAnimation`, `SkeletonMecanim` and `SkeletonGraphic`).
  * Added `BoundingBoxFollowerGraphic` component. This class is a counterpart of `BoundingBoxFollower` that can be used with `SkeletonGraphic`.
  * Added Inspector context menu functions `SkeletonRenderer - Add all BoundingBoxFollower GameObjects` and `SkeletonGraphic - Add all BoundingBoxFollowerGraphic GameObjects` that automatically generate bounding box follower GameObjects for every `BoundingBoxAttachment` for all skins of a skeleton.
  * `GetRemappedClone()` now provides an additional parameter `pivotShiftsMeshUVCoords` for `MeshAttachment` to prevent uv shifts at a non-central Sprite pivot. This parameter defaults to `true` to maintain previous behaviour.
  * `SkeletonRenderer` components now provide an additional update mode `Only Event Timelines` at the `Update When Invisible` property. This mode saves additional timeline updates compared to update mode `Everything Except Mesh`.
  * Now all URP (Universal Render Pipeline) and LWRP (Lightweight Render Pipeline) shaders support SRP (Scriptable Render Pipeline) batching. See [Unity SRPBatcher documentation pages](https://docs.unity3d.com/Manual/SRPBatcher.html) for additional information.
  * Sprite shaders now provide four `Diffuse Ramp` modes as an Inspector Material parameter: `Hard`, `Soft`, `Old Hard` and `Old Soft`. In spine-unity 3.8 it defaults to `Old Hard` to keep the behaviour of existing projects unchanged. From 4.0 on it defaults to `Hard` for newly created materials while existing ones remain unchanged. Note that `Old Hard` and `Old Soft` ramp versions were using only the right half of the ramp texture, and additionally multiplying the light intensity by 2, both leading to brighter lighting than without a ramp texture active. The new ramp modes `Hard` and `Soft` use the full ramp texture and do not modify light intensity, being consistent with lighting without a ramp texture active.
  * Added **native support for slot blend modes** `Additive`, `Multiply` and `Screen` with automatic assignment at newly imported skeleton assets. `BlendModeMaterialAssets` are now obsolete and replaced by the native properties at `SkeletonDataAsset`. The `SkeletonDataAsset` Inspector provides a new `Blend Modes - Upgrade` button to upgrade an obsolete `BlendModeMaterialAsset` to the native blend modes properties. This upgrade will be performed automatically on imported and re-imported assets in Unity 2020.1 and newer to prevent reported `BlendModeMaterialAsset` issues in these Unity versions. spine-unity 4.0 and newer will automatically perform this upgrade regardless of the Unity version.
  * `BoneFollower` and `BoneFollowerGraphic` components now provide better support for following bones when the skeleton's Transform is not the parent of the follower's Transform. Previously e.g. rotating a common parent Transform did not lead to the desired result, as well as negatively scaling a skeleton's Transform when it is not a parent of the follower's Transform.
  * URP and LWRP `Sprite` and `SkeletonLit` shaders no longer require `Advanced - Add Normals` enabled to properly cast and receive shadows. It is recommended to disable `Add Normals` if normals are otherwise not needed.
  * Added an example component `RootMotionDeltaCompensation` located in `Spine Examples/Scripts/Sample Components` which can be used for applying simple delta compensation. You can enable and disable the component to toggle delta compensation of the currently playing animation on and off.
  * Root motion delta compensation now allows to only adjust X or Y components instead of both. Adds two parameters to `SkeletonRootMotionBase.AdjustRootMotionToDistance()` which default to adjusting both X and Y as before. The `RootMotionDeltaCompensation` example component exposes these parameters as public attributes.
  * Root motion delta compensation now allows to also add translation root motion to e.g. adjust a horizontal jump upwards or downwards over time. This is necessary because a Y root motion of zero cannot be scaled to become non-zero.
  * `Attachment.GetRemappedClone(Sprite)` method now provides an additional optional parameter `useOriginalRegionScale`. When set to `true`, the replaced attachment's scale is used instead of the Sprite's `Pixel per Unity` setting, allowing for more consistent scaling. *Note:* When remapping Sprites, be sure to set the Sprite's `Mesh Type` to `Full Rect` and not `Tight`, otherwise the scale will be wrong.

* **Changes of default values**
  * `SkeletonMecanim`'s `Layer Mix Mode` now defaults to `MixMode.MixNext` instead of `MixMode.MixAlways`.
  * `BlendModeMaterialAsset` and it's instance `Default BlendModeMaterials.asset` now have `Apply Additive Material` set to `true` by default in order to apply all blend modes by default.

* **Deprecated**
  * Deprecated `Modules/SlotBlendModes/SlotBlendModes` component. Changed namespace from `Spine.Unity.Modules` to `Spine.Unity.Deprecated`. Moved to `Deprecated/SlotBlendModes`.

* **Restructuring (Non-Breaking)**

  Note: The following changes will most likely not affect users of the Spine-Unity runtime as the API remains unchanged and no references are invalidated.
  * Removed duplicates of `.cginc` files in `Modules/Shaders/Sprite` that were also present in the `Modules/Shaders/Sprite/CGIncludes` directory.
  * Moved shaders from `Modules/Shaders` to `Shaders` directory.
  * Moved shaders from `Modules/SkeletonGraphic/Shaders` to `Shaders/SkeletonGraphic`.
  * Renamed shader `Shaders/Spine-SkeletonLit.shader` to `Shaders/Spine-Skeleton-Lit.shader`.
  * Moved components from `SkeletonGraphic` to `Components` and `Components/Following` except for `SkeletonGraphicMirror` which was moved to `Spine Examples/Scripts/Sample Components`.
  * Moved `BoneFollower`, `BoneFollowerGraphic` and `PointFollower` from `Components` directory to `Components/Following`.
  * Moved `BoundingBoxFollower` component from `Modules/BoundingBoxFollower` to `Components/Following`.
  * Moved `Modules/SkeletonRenderSeparator` directory to `Components/SkeletonRenderSeparator`.
  * Moved `Modules/CustomMaterials` directory to `Components/SkeletonRendererCustomMaterials`.
  * Moved `Asset Types/BlendModeMaterialsAsset.cs` class, `Shaders/BlendModes/Default BlendModeMaterials.asset` and materials from `Shaders/BlendModes` to `SkeletonDataModifierAssets/BlendModeMaterials` directory.
  * Moved `Modules/Ghost` directory to `Spine Examples/Scripts/Sample Components/Ghost`.
  * Moved `Modules/SkeletonUtility Modules` directory to `Spine Examples/Scripts/Sample Components/SkeletonUtility Modules`.
  * Moved `Modules/AnimationMatchModifier` directory to `Spine Examples/Scripts/MecanimAnimationMatchModifier`.
  * Moved `SkeletonRagdoll` and `SkeletonRagdoll2D` components from `Modules/Ragdoll` directory to `Spine Examples/Scripts/Sample Components/SkeletonUtility Modules`.
  * Moved `AttachmentTools.cs` to `Utility` directory.
  * Split the file `AttachmentTools` into 5 separate files for each contained class. No namespace or other API changes performed.
  * Split the file `Mesh Generation/SpineMesh` into 4 separate files for each contained class. No namespace or other API changes performed.
  * Moved `SkeletonExtensions.cs` to `Utility` directory.
  * Moved `Modules/YieldInstructions` directory to `Utility/YieldInstructions`.
  * Moved corresponding editor scripts of the above components to restructured directories as well.
  * Renamed inspector editor class `PointFollowerEditor` to `PointFollowerInspector` for consistency reasons.

### XNA/MonoGame
* Updated to latest MonoGame version 3.7.1
* Rewrote example project to be cleaner and better demonstrate basic Spine features.
* Added mix-and-match example to demonstrate the new Skin API.
* Added normalmap support via `SpineEffectNormalmap` and support for loading multiple texture layers following a suffix-pattern. Please see the example code on how to use them.

## Java
* **Breaking changes**
  * Renamed `Slot#getAttachmentVertices()` to `Slot#getDeform()`.
  * Changed the `.json` curve format and added more assumptions for omitted values, reducing the average size of JSON exports.
  * Renamed `Skin#addAttachment()` to `Skin#setAttachment()`.
  * Removed `VertexAttachment#applyDeform()` and replaced it with `VertexAttachment#deformAttachment`. The attachment set on this field is used to decide if a `DeformTimeline` should be applied to the attachment active on the slot to which the timeline is applied.
  * Removed `inheritDeform` field, getter, and setter from `MeshAttachment`.
  * Changed `.skel` binary format, added a string table. References to strings in the data resolve to this string table, reducing storage size of binary files considerably.
  * `JsonRollback` tool now converts from 3.8 JSON to 3.7.
  * Changed the `.json` and `.skel` file formats to accomodate the new feature and file size optimiations. Old projects must be exported with Spine 3.8.20+ to be compatible with the 3.8 Spine runtimes.

* **Additions**
  * Added `x` and `y` coordinates for setup pose AABB in `SkeletonData`.
  * Added support for rotated mesh region UVs.
  * Added skin-specific bones and constraints which are only updated if the skeleton's current skin contains them.
  * Improved Skin API to make it easier to handle mix-and-match use cases.
    * Added `Skin#getAttachments()`. Returns all attachments in the skin.
    * Added `Skin#getAttachments(int slotIndex)`. Returns all attachements in the skin for the given slot index.
    * Added `Skin#addSkin(Skin skin)`. Adds all attachments, bones, and skins from the specified skin to this skin.
    * Added `Skin#copySkin(Skin skin)`. Adds all attachments, bones, and skins from the specified skin to this skin. `VertexAttachment` are shallowly copied and will retain any parent mesh relationship. All other attachment types are deep copied.
  * Added `Attachment#copy()` to all attachment type implementations. This lets you deep copy an attachment to modify it independently from the original, i.e. when programmatically changing texture coordinates or mesh vertices.
  * Added `MeshAttachment#newLinkedMesh()`, creates a linked mesh linkted to either the original mesh, or the parent of the original mesh.
  * Added IK softness.

### libGDX
* `SkeletonViewer` can load a skeleton by specifying it as the first argument on the command line.
* Added mix-and-match example to demonstrate the new Skin API.

## Lua
* **Breaking changes**
  * Renamed `Slot:getAttachmentVertices()` to `Slot#deform`.
  * Changed the `.json` curve format and added more assumptions for omitted values, reducing the average size of JSON exports.
  * Renamed `Skin:addAttachment()` to `Skin#setAttachment()`.
  * Removed `VertexAttachment:applyDeform()` and replaced it with `VertexAttachment#deformAttachment`. The attachment set on this field is used to decide if a `DeformTimeline` should be applied to the attachment active on the slot to which the timeline is applied.
  * Removed `inheritDeform` field, getter, and setter from `MeshAttachment`.
  * Changed the `.json` file format to accomodate the new feature and file size optimiations. Old projects must be exported with Spine 3.8.20+ to be compatible with the 3.8 Spine runtimes.

* **Additions**
  * Added `x` and `y` coordinates for setup pose AABB in `SkeletonData`.
  * Added support for rotated mesh region UVs.
  * Added skin-specific bones and constraints which are only updated if the skeleton's current skin contains them.
  * Improved Skin API to make it easier to handle mix-and-match use cases.
    * Added `Skin:getAttachments()`. Returns all attachments in the skin.
    * Added `Skin:getAttachments(slotIndex)`. Returns all attachements in the skin for the given slot index.
    * Added `Skin:addSkin(Skin skin)`. Adds all attachments, bones, and skins from the specified skin to this skin.
    * Added `Skin:copySkin(Skin skin)`. Adds all attachments, bones, and skins from the specified skin to this skin. `VertexAttachment` are shallowly copied and will retain any parent mesh relationship. All other attachment types are deep copied.
  * Added `Attachment:copy()` to all attachment type implementations. This lets you deep copy an attachment to modify it independently from the original, i.e. when programmatically changing texture coordinates or mesh vertices.
  * Added `MeshAttachment:newLinkedMesh()`, creates a linked mesh linkted to either the original mesh, or the parent of the original mesh.
  * Added IK softness.

### Love2D
* Added support for 0-1 RGBA color component range change in Löve 0.11+. Older Löve versions using the 0-255 range are still supported!
* Added mix-and-match example to demonstrate the new Skin API.

### Corona
* Added mix-and-match example to demonstrate the new Skin API.

## Typescript/Javascript
* **Breaking changes**
  * Renamed `MixDirection.in/out` to `MixDirection.mixIn/mixOut` as it was crashing a JS compressor.
  *  Renamed `Slot#getAttachmentVertices()` to `Slot#getDeform()`.
  * Changed the `.json` curve format and added more assumptions for omitted values, reducing the average size of JSON exports.
  * Renamed `Skin#addAttachment()` to `Skin#setAttachment()`.
  * Removed `VertexAttachment#applyDeform()` and replaced it with `VertexAttachment#deformAttachment`. The attachment set on this field is used to decide if a `DeformTimeline` should be applied to the attachment active on the slot to which the timeline is applied.
  * Removed `inheritDeform` field, getter, and setter from `MeshAttachment`.
  * Changed `.skel` binary format, added a string table. References to strings in the data resolve to this string table, reducing storage size of binary files considerably.
  * Changed the `.json` and `.skel` file formats to accomodate the new feature and file size optimiations. Old projects must be exported with Spine 3.8.20+ to be compatible with the 3.8 Spine runtimes.
  * Updated runtime to be compatible with TypeScript 3.6.3.

* **Additions**
  * Added support for loading binary data via `AssetManager#loadBinary()`. `AssetManager#get()` will return a `Uint8Array` for such assets.
  * Added support for loading binaries via new `SkeletonBinary`. Parses a `Uint8Array`.
  * Added `x` and `y` coordinates for setup pose AABB in `SkeletonData`.
  * Added support for rotated mesh region UVs.
  * Added skin-specific bones and constraints which are only updated if the skeleton's current skin contains them.
  * Improved Skin API to make it easier to handle mix-and-match use cases.
    * Added `Skin#getAttachments()`. Returns all attachments in the skin.
    * Added `Skin#getAttachments(slotIndex: number)`. Returns all attachements in the skin for the given slot index.
    * Added `Skin#addSkin(skin: Skin)`. Adds all attachments, bones, and skins from the specified skin to this skin.
    * Added `Skin#copySkin(skin: Skin)`. Adds all attachments, bones, and skins from the specified skin to this skin. `VertexAttachment` are shallowly copied and will retain any parent mesh relationship. All other attachment types are deep copied.
  * Added `Attachment#copy()` to all attachment type implementations. This lets you deep copy an attachment to modify it independently from the original, i.e. when programmatically changing texture coordinates or mesh vertices.
  * Added `MeshAttachment#newLinkedMesh()`, creates a linked mesh linkted to either the original mesh, or the parent of the original mesh.
  * Added IK softness.
  * Added `AssetManager.setRawDataURI(path, data)`. Allows to embed data URIs for skeletons, atlases and atlas page images directly in the HTML/JS without needing to load it from a separate file.
  * Added `AssetManager.loadAll()` to allow Promise/async/await based waiting for completion of asset load. See the `spine-canvas` examples.
  * Added `Skeleton.getBoundRect()` helper method to calculate the bouding rectangle of the current pose, returning the result as `{ x, y, width, height }`. Note that this method will create temporary objects which can add to garbage collection pressure.

### WebGL backend
* `Input` can now take a partially defined implementation of `InputListener`.
* Added mix-and-match example to demonstrate the new Skin API.

### Canvas backend

### Three.js backend
* `SkeletonMesh` now takes an optional `SkeletonMeshMaterialParametersCustomizer` function that allows you to modify the `ShaderMaterialParameters` before the material is finalized. Use it to modify things like THREEJS' `Material.depthTest` etc. See #1590.

### Player
* `SpinePlayer#setAnimation()` can now be called directly to set the animation being displayed.
* The player supports loading `.skel` binary skeleton files by setting the `SpinePlayerConfig#skelUrl` field instead of `SpinePlayerConfig#jsonUrl`.
* Added `SpinePlayerConfig#rawDataURIs`. Allows to embed data URIs for skeletons, atlases and atlas page images directly in the HTML/JS without needing to load it from a separate file. See the example for a demonstration.

# 3.7

## AS3
* **Breaking changes**
  * The completion event will fire for looped 0 duration animations every frame.
  * `MixPose` is now called `MixBlend`
  * Skeleton `flipX/flipY` has been replaced with `scaleX/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
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
  * Skeleton `flipX/flipY` has been replaced with `scaleX/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
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
  * Skeleton `flipX/flipY` has been replaced with `scaleX/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
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
  * Skeleton `flipX/flipY` has been replaced with `scaleX/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
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
  * Skeleton `flipX/flipY` has been replaced with `scaleX/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
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
  * Skeleton `flipX/flipY` has been replaced with `scaleX/scaleY`. This cleans up applying transforms and is more powerful. Allows scaling a whole skeleton which has bones that disallow scale inheritance
  * Mix time is no longer affected by `TrackEntry#timeScale`. See https://github.com/EsotericSoftware/spine-runtimes/issues/1194
* **Additions**
  * Added `AssetManager.loadTextureAtlas`. Instead of loading the `.atlas` and corresponding image files manually, you can simply specify the location of the `.atlas` file and AssetManager will load the atlas and all its images automatically. `AssetManager.get("atlasname.atlas")` will then return an instance of `spine.TextureAtlas`.
  * Added additive animation blending. When playing back multiple animations on different tracks, where each animation modifies the same skeleton property, the results of tracks with lower indices are discarded, and only the result from the track with the highest index is used. With animation blending, the results of all tracks are mixed together. This allows effects like mixing multiple facial expressions (angry, happy, sad) with percentage mixes. By default the old behaviour is retained (results from lower tracks are discarded). To enable additive blending across animation tracks, call `TrackEntry#setMixBlend(MixBlend.add)` on each track. To specify the blend percentage, set `TrackEntry#alpha`. See http://esotericsoftware.com/forum/morph-target-track-animation-mix-mode-9459 for a discussion. See https://github.com/EsotericSoftware/spine-runtimes/blob/f045d221836fa56191ccda73dd42ae884d4731b8/spine-ts/webgl/tests/test-additive-animation-blending.html for an example.
  * Added work-around for iOS WebKit JIT bug, see https://github.com/EsotericSoftware/spine-runtimes/commit/c28bbebf804980f55cdd773fed9ff145e0e7e76c
  * Support for stretchy IK
  * Support for audio events, see `audioPath`, `volume` and `balance` fields on event (data).
  * `TrackEntry` has an additional field called `holdPrevious`. It can be used to counter act a limitation of `AnimationState` resulting in "dipping" of parts of the animation. For a full discussion of the problem and the solution we've implemented, see this [forum thread](http://esotericsoftware.com/forum/Probably-Easy-Animation-mixing-with-multiple-tracks-10682?p=48130&hilit=holdprevious#p48130).
  * Added `AssetManager#setRawDataURI(path, data)`. Allows to set raw data URIs for a specific path, which in turn enables embedding assets into JavaScript/HTML.
  * `PolygonBatcher` will now disable `CULL_FACE` and restore the state as it was before rendering.

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
   * `SkeletonAnimator` `autoreset` and the `mixModes` array are now a part of SkeletonAnimator's MecanimTranslator `.Translator`. `autoReset` is set to `true` by default. Old prefabs and scene objects with Skeleton Animator may no longer have correct values set.
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
