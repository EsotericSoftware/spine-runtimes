# UE4 Runtime Mesh Component

**Branch with Slicer Support can be found here https://github.com/Koderz/UnrealEngine/tree/RMC_With_Slicer**
**Examples project can be found here https://github.com/Koderz/UE4RuntimeMeshComponentExamples**


**The RuntimeMeshComponent, or RMC for short, is a component designed specifically to support rendering and collision on meshes generated at runtime. This could be anything from voxel engines like Minecraft, to custom model viewers, or just supporting loading user models for things like modding. It has numerous different features to support most of the normal rendering needs of a game, as well as ability to support both static collision for things such as terrain, as well as dynamic collision for things that need to be able to move and bounce around!**

**Now, the RMC is very similar in purpose to the ProceduralMeshComponent or CustomMeshComponent currently found in UE4, but it far surpasses both in features, and efficiency! It on average uses half the memory of the ProceduralMeshComponent, while also being more efficient to render, and far faster to update mesh data. This is shown by the ability to update a 600k+ vertex mesh in real time! The RMC is also nearly 100% compatible with the ProceduralMeshComponent, while adding many features above what the PMC offers.**


*Current list of features in the RMC*
* Slicer Support!!  You can now use
* Collision cooking speed improvements.** (new)
* High precision normals support (new)
* Tessellation support (new)
* Navigation mesh support (new)
* Fully configurable vertex structure (new)
* Ability to save individual sections or the entire RMC to disk (new)
* RMC <-> StaticMesHComponent conversions.  SMC -> RMC at runtime or in editor.  RMC -> SMC in editor.  (new)
* Normal/Tangent calculation. (new) (will be getting speed improvements soon)
* Multiple UV channel support (up to 8 channels) 
* Fast path updates for meshes that need to update as fast as frame-by-frame
* Static render path for meshes that don't update frequently, this provides a slightly faster rendering performance.
* Collision only mesh sections.
* 50%+ memory reduction over the ProceduralMeshComponent and CustomMeshComponent
* Visibility and shadowing are configurable per section.
* Separate vertex positions for cases of only needing to update the position.
* Collision has lower overhead compared to ProceduralMeshComponent

**The RMC has picked up the collision cooking improvements done in UE4.14. This means that by default you'll see far faster collision updates, but at the cost of a little lower performance collision. You do however have the option to prioritize quality, which will slow down updates, but make actual collision detection a little faster**

**As a part of V2, there has also been some preliminary work done on threaded cooking. This can help to unblock the game thread from collision with large meshes. This is still a very new part, and not heavily tested or complete. To use this you'll have to use a source build of the engine. More information to come.**

For information on installation, usage and everything else, [please read the Wiki](https://github.com/Koderz/UE4RuntimeMeshComponent/wiki/)

**Some requested features that I'm looking into: (These aren't guaranteed to be added)**
* LOD (Potentially with dithering support)
* Dithered transitions for mesh updates.
* Mesh sharing, to allow multiple RMCs to have the same copy of the mesh to reduce memory overhead. This is much like how the StaticMeshComponent works.
* Instancing support.
* Multiple vertex buffer (In Addition to the current separate position vertex buffer)
* Mesh replication

**Supported Engine Versions:**
v1.2 supports engine versions 4.10+
v2.0 supports engine versions 4.12+

*The Runtime Mesh Component should support all UE4 platforms.*
*Collision MAY NOT be available on some platforms (HTML5, Mobile)*
