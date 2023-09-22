## Spine Addressables Extensions [Experimental]

This experimental plugin provides integration of Addressables on-demand texture loading for the spine-unity runtime. Please be sure to test this package first and create backups of your project before using.

### Usage

First declare your target Material textures as addressable. Then select the SpineAtlasAsset, right-click the SpineAtlasAsset Inspector heading and select 'Add Addressables Loader'. This generates an 'AddressableTextureLoader' asset providing configuration parameters and sets up low-resolution placeholder textures which are automatically assigned in a pre-build step when building your game executable.
