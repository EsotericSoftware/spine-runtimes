Contributed by ToddRivers

# Unity Sprite Shaders
An Uber Shader specialised for rendering Sprites in Unity.
Even though it's designed for Sprites it can be used for a whole range of uses. It supports a wide range of optional shader features that won't effect performance unless they are used.
It also supports per-pixel effects such as normal maps and diffuse ramping whilst using Vertex Lit rendering.

### Lighting
The shaders support lighting using both Forward Rendering and Vertex Lit Rendering.
Forward rendering is more accurate but is slower and crucially means the sprite has to write to depth using alpha clipping to avoid overdraw.
Vertex lit means all lighting can be done in one pass meaning full alpha can be used.

### Normal Mapping
Normals maps are supported in both lighting modes (in Vertex Lit rendering data for normal mapping is packed into texture channels and then processed per pixel).

### Blend Modes
Easily switch between blend modes including pre-multiplied alpha, additive, multiply etc.

### Rim Lighting
Camera-space rim lighting is supported in both lighting modes.

### Diffuse Ramp
A ramp texture is optionally supported for toon shading effects.

### Shadows
Shadows are supported using alpha clipping.

### Gradient based Ambient lighting
Both lighting modes support using a gradient for ambient light. In Vertex Lit mode the Spherical Harmonics is approximated from the ground, equator and sky colors.

### Emission Map
An optional emission map is supported.

### Camera Space Normals
As sprites are 2d their normals will always be constant. The shaders allow you to define a fixed normal in camera space rather than pass through mesh normals.
This not only saves vertex throughput but means lighting looks less 'flat' for rendering sprites with a perspective camera.

### Color Adjustment
The shaders allow optional adjustment of hue / saturation and brightness as well as applying a solid color overlay effect for flashing a sprite to a solid color (eg. for damage effects).

### Fog
Fog is optionally supported


## To Use
On your object's material click the drop down for shader and select Spine\Sprite\Pixel Lit, Vertex Lit or Unlit.
