## Spine Addressables Extensions [Experimental]

This experimental plugin provides integration of Addressables on-demand texture loading for the spine-unity runtime. Please be sure to test this package first and create backups of your project before using.

The Spine Addressables Extension module covers all necessary steps to automatically replace your textures with low-resolution placeholder textures and loading the high-resolution textures upon request. The textures are automatically replaced with the low-resolution texture when building your game executable via a pre-build step (and reset to the high-resolution textures afterwards in the post-build step). There is no additional coding required.

### Usage

The following steps are all that is required to setup your textures to be replaced with low resolution placeholders and downloaded on-demand:

1. Declare your original high-resolution target Material textures as [addressable](https://learn.unity.com/course/get-started-with-addressables).
2. Select the `SpineAtlasAsset`, right-click the `SpineAtlasAsset` Inspector heading and select `Add Addressables Loader`. This generates an `AddressableTextureLoader` asset. This asset provides configuration parameters and sets up low-resolution placeholder textures which are automatically assigned in a pre-build step when building your game executable.
3. Build your Addressables content as usual.

From now on when building your game executable, the low resolution placeholder textures are automatically assigned initially and the corresponding high-resolution textures loaded on-demand.

### Editor Preview

Please note that the low-resolution textures are activated only when building the game executable, they are usually never shown in the Editor, also not in play-mode.

If you still want to temporarily assign the low-resolution placeholder textures for preview purposes, select the desired `AddressableTextureLoader` asset and hit `Testing` - `Assign Placeholders` to temporarily replace the high-resolution textures with their low-resolution placeholders. You can then see the high-resolution textures loaded on-demand when entering play-mode. Note that this is for preview purposes only and has no effect on the built game executable.

Placeholder textures need not be assigned manually, as these are automatically assigned via a pre-build step (and reset afterwards in the post-build step).
