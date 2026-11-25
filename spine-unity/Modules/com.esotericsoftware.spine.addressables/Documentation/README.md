## Spine Addressables Extensions

See the [On-Demand Loading documentation page](https://esotericsoftware.com/spine-unity-on-demand-loading) for an illustrated version of this document.

The Spine Addressables plugin adds support for on-demand texture loading to the spine-unity runtime through integration with the [Unity Addressables](https://learn.unity.com/course/get-started-with-addressables) system.

This module covers all necessary steps to automatically replace your textures with low-resolution placeholder versions and loads high-resolution textures on demand. A pre-build step replaces the textures in the build output with their low-resolution counterparts, while a post-build step restores the original high-resolution textures in your project. No additional coding is required.

### Installation

The Spine Addressables Extensions package `com.esotericsoftware.spine.addressables` depends on the Spine On-Demand Loading Extension package `com.esotericsoftware.spine.on-demand-loading`. Please install the On-Demand Loading package before installing the Addressables package.

See section [Optional Extension UPM Packages](https://esotericsoftware.com/spine-unity-installation#Optional-Extension-UPM-Packages) on how to download and install UPM packages and section [Updating an Extension UPM Package](https://esotericsoftware.com/spine-unity-installation#Updating-an-Extension-UPM-Package) on how to update them.

### Usage

The following steps are all that is required to configure your textures to be replaced with low-resolution placeholders and high-resolution versions loaded automatically on demand.

1. Declare your original high-resolution target Material textures as [addressable](https://learn.unity.com/course/get-started-with-addressables).
2. Select the `SpineAtlasAsset`, right-click the `SpineAtlasAsset` Inspector heading and select `Add Addressables Loader`.
3. This generates an `AddressableTextureLoader` asset.
4. This asset provides configuration parameters and sets up low-resolution placeholder textures which are automatically assigned in a pre-build step when building your game executable.
5. Build your Addressables content as usual.

From now on when building your game executable, the low resolution placeholder textures are automatically assigned initially and the corresponding high-resolution textures loaded on-demand.

### Editor Preview

Please note that the low-resolution textures are only activated during the game build process. They are not visible in the Unity Editor, including during play mode.

If you'd like to preview the behavior with low-resolution placeholders in the Editor, you can temporarily assign them for testing. To do this, select the desired `AddressableTextureLoader` asset and hit `Testing` - `Assign Placeholders` to temporarily replace the high-resolution textures with their low-resolution placeholders.

You can then observe the high-resolution textures being loaded on-demand during play-mode. Note that this change is for preview purposes only and has no effect on the built game executable. There is no need to manually assign placeholder textures for builds - the system handles this automatically through a pre-build step, with the original textures restored in a post-build step.

> **Note:** `Assign Placeholders` only covers the materials of the `SpineAtlasAsset` and the blend mode materials of the `SkeletonDataAsset`. Other `Material` assets referencing the same textures, e.g. material copies using a different shader, are not modified. The pre-build step covers these as well if `Scan Additional Materials` is enabled in the [Spine Preferences](https://esotericsoftware.com/spine-unity-assets#Spine-Preferences) (default).

### Additional Material Textures

By default only the main texture of each atlas material is loaded on-demand. To cover additional textures such as normal maps, add their shader property names (e.g. `_BumpMap`) to the `Additional Texture Properties` list of the `AddressableTextureLoader` asset, or hit `Add All Textures` to add all texture properties in use at the atlas materials. These textures must be declared as addressable as well.

Blend mode materials and material copies are covered as well. `SkeletonGraphic` only uses the main texture of the atlas materials, so additional textures do not apply to it.
