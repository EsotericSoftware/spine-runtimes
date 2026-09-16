## Spine Addressables Extensions

The Spine Addressables plugin provides integration of on-demand texture loading for the spine-unity runtime by using Addressables.

This module covers all necessary steps to automatically replace your textures with low-resolution placeholder textures and loading the high-resolution textures upon request. The textures are automatically replaced with the low-resolution texture when building your game executable via a pre-build step (and reset to the high-resolution textures afterwards in the post-build step). There is no additional coding required.

## Installation

The Spine Addressables Extensions package `com.esotericsoftware.spine.addressables` depends on the Spine On-Demand Loading Extension package `com.esotericsoftware.spine.on-demand-loading`. Please install the On-Demand Loading package before installing the Addressables package.

### Usage

The following steps are all that is required to setup your textures to be replaced with low resolution placeholders and downloaded on-demand:

1. Declare your original high-resolution target Material textures as [addressable](https://learn.unity.com/course/get-started-with-addressables).
2. Select the `SpineAtlasAsset`, right-click the `SpineAtlasAsset` Inspector heading and select `Add Addressables Loader`. This generates an `AddressableTextureLoader` asset. This asset provides configuration parameters and sets up low-resolution placeholder textures which are automatically assigned in a pre-build step when building your game executable.
3. Build your Addressables content as usual.

From now on when building your game executable, the low resolution placeholder textures are automatically assigned initially and the corresponding high-resolution textures loaded on-demand.

If the placeholder texture map no longer matches the atlas materials, e.g. after changing textures, the loader is skipped when building and its textures are included at full resolution. Hit `Regenerate` at the loader to update the map.

### Additional Textures (Normal Maps, etc.)

By default only the main texture of each atlas material is loaded on-demand. If your materials use additional textures such as normal maps, add the respective shader texture property names (e.g. `_BumpMap`) to the `Additional Texture Properties` list of the `AddressableTextureLoader` asset, or hit `Add All Textures` to fill the list with all texture properties which have a texture assigned at any atlas material. The loader then searches the atlas materials for these properties, creates low-resolution placeholder textures for the assigned textures and loads the high-resolution textures on-demand together with the main texture. Be sure to also declare these additional textures as addressable. Blend mode materials and copied materials referencing these textures are handled automatically as well. Textures at properties not listed (e.g. a differently named normal map property of a custom shader) are not loaded on demand.

### Editor Preview

Please note that the low-resolution textures are activated only when building the game executable, they are usually never shown in the Editor, also not in play-mode.

If you still want to temporarily assign the low-resolution placeholder textures for preview purposes, select the desired `AddressableTextureLoader` asset and hit `Testing` - `Assign Placeholders` to temporarily replace the high-resolution textures with their low-resolution placeholders. You can then see the high-resolution textures loaded on-demand when entering play-mode. Note that this is for preview purposes only and has no effect on the built game executable.

Placeholder textures need not be assigned manually, as these are automatically assigned via a pre-build step (and reset afterwards in the post-build step).
