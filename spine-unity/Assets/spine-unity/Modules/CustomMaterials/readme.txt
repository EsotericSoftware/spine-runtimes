SkeletonRendererCustomMaterials by LostPolygon
===============================
This is a basic serialization and inspector implementation for custom material overrides for SkeletonRenderer and its derived classes (SkeletonAnimation, SkeletonAnimator).

## How to use
Right-click on your SkeletonRenderer and select "Add Basic Serialized Custom Materials". This will add and initialize the SkeletonRendererCustomMaterials to the same object.

You can use this to store material override settings for SkeletonRenderer instances/prefabs so they will be applied automatically when your scene starts or when the prefab is instantiated.

This script is not intended for use with code.
To dynamically set materials for your SkeletonRenderer through code, you can directly access `SkeletonRenderer.CustomMaterialOverride` for material array overrides and `SkeletonRenderer.CustomSlotMaterials` for slot material overrides.