# Spine Runtime Changelog Generation

## Output File
Add changes to `/spine-runtimes/CHANGELOG.md` at the TOP of the file, under the current version heading (e.g., `# 4.3`).
Each language gets a `##` section, each engine integration gets a `###` subsection under its language.

**CRITICAL**: If entries already exist for the current version:
- **PRESERVE all existing human-written entries** - they may contain important manual additions
- **ADD new entries** to the appropriate sections
- **MOVE entries** if needed to maintain correct ordering, but NEVER delete or modify existing text
- **MERGE** your generated entries with existing ones, don't replace

## Step 1: Determine Comparison Branch
```bash
cd /spine-runtimes
git branch -r | grep origin | grep -E "^  origin/[0-9]\.[0-9]" | sort -V
```
Ask user: "Which branch to compare against? (typically the previous version, e.g., 4.2 for 4.3 changes)"

## Step 2: Analyze Java Reference Implementation
```bash
git diff COMPARE_BRANCH..CURRENT_BRANCH -- spine-libgdx/spine-libgdx/src/com/esotericsoftware/spine/
```

### What to Look For:
- **Public API changes** (public/protected methods, fields, classes)
- **Ignore**: private methods, internal implementation, formatting, comments
- **New files**: Check if they contain public classes
- **Deleted files**: Note if public classes were removed
- **Method signature changes**: Parameters added/removed/changed

## Step 3: Generate Changelog Entries

### Process Order:
1. Start with Java reference implementation changes
2. Port these to all language runtimes (with language-specific syntax)
3. Add language runtime specific changes
4. Add engine-specific changes
5. **Check off each item in the checklist as you complete it**

### Runtime Checklist (follows changelog.md structure):

- [ ] **C** (`spine-c/`) - C wrapper around C++ runtime
  - [ ] **SFML** (`spine-sfml/c/`)
  - [ ] **SDL** (`spine-sdl/`)
  - [ ] **GLFW** (`spine-glfw/`)

- [ ] **C++** (`spine-cpp/`) - Check `src/spine/*.cpp` and `include/spine/*.h`
  - [ ] **SFML** (`spine-sfml/cpp/`)
  - [ ] **UE** (`spine-ue/`)
  - [ ] **Godot** (`spine-godot/`)

- [ ] **C#** (`spine-csharp/src/`) - Check all `.cs` files
  - [ ] **Unity** (`spine-unity/`)
  - [ ] **MonoGame** (`spine-monogame/`)
  - [ ] **XNA** (`spine-xna/`)

- [ ] **Dart** (`spine-flutter/src/`) - Dart runtime
  - [ ] **Flutter** (`spine-flutter/`) - Flutter-specific integration

- [ ] **Haxe** (`spine-haxe/spine-haxe/`) - Check all `.hx` files

- [ ] **Java** (`spine-libgdx/spine-libgdx/src/`) - Reference implementation
  - [ ] **libGDX** (`spine-libgdx/`) - libGDX-specific integration
  - [ ] **Android** (`spine-android/`)

- [ ] **Swift** (`spine-ios/spine-ios/`) - Swift/Objective-C runtime
  - [ ] **iOS** (`spine-ios/`) - iOS-specific integration

- [ ] **TypeScript/JavaScript** (`spine-ts/spine-core/src/`) - Check all `.ts` files
  - [ ] **WebGL backend** (`spine-ts/spine-webgl/`)
  - [ ] **Canvas backend** (`spine-ts/spine-canvas/`)
  - [ ] **CanvasKit backend** (`spine-ts/spine-canvaskit/`)
  - [ ] **Three.js backend** (`spine-ts/spine-threejs/`)
  - [ ] **Player** (`spine-ts/spine-player/`)
  - [ ] **Pixi v7** (`spine-ts/spine-pixi-v7/`)
  - [ ] **Pixi v8** (`spine-ts/spine-pixi-v8/`)
  - [ ] **Phaser v3** (`spine-ts/spine-phaser-v3/`)
  - [ ] **Phaser v4** (`spine-ts/spine-phaser-v4/`)
  - [ ] **Web Components** (`spine-ts/spine-webcomponents/`)

## Entry Format Hierarchy

The changelog has a **strict two-level hierarchy**:

1. **Top level**: Language runtime (## C, ## C++, ## C#, ## Dart, ## Haxe, ## Java, ## Swift, ## TypeScript/JavaScript)
   - Contains changes to the core runtime implementation
   - All public API changes from Java reference must be ported here

2. **Second level**: Engine/platform integrations under their language (### Unity, ### libGDX, ### Godot)
   - Contains ONLY engine-specific changes
   - Does NOT repeat language runtime changes

### Example Structure:
```markdown
## C

- **Additions**
  - Added `spSkeleton_findSlider()` to query sliders by name (ported from Java)
  - Added `spSlider` and `spSliderData` types (ported from Java)
- **Breaking changes**
  - `spSkeleton_updateWorldTransform()` now requires `spPhysics` parameter (ported from Java)
  - Renamed `spTransformMode` to `spInherit` and all `SP_TRANSFORMMODE_*` to `SP_INHERIT_*`
- **Changes of default values**
  - Changed default mix duration from 0 to 0.2 seconds
- **Deprecated**
  - Deprecated `spBone_worldToLocalRotationX()` and `spBone_worldToLocalRotationY()`
- **Restructuring (Non-Breaking)**
  - Moved internal math utilities to separate module

### SFML

- **Additions**
  - Added example showing physics integration
  - New rendering optimization for batched sprites

### SDL

- **Additions**
  - Added SDL3 support
- **Breaking changes**
  - Removed SDL1 support

## C++

- **Additions**
  - Added `Skeleton::findSlider()` to query sliders by name (ported from Java)
  - Added `Slider` and `SliderData` classes (ported from Java)
- **Breaking changes**
  - `Skeleton::updateWorldTransform()` now requires `Physics` parameter (ported from Java)
  - Renamed `TransformMode` to `Inherit` and all `TransformMode_*` to `Inherit_*`
- **Deprecated**
  - Deprecated `Bone::worldToLocalRotationX()` and `Bone::worldToLocalRotationY()`

### UE

- **Additions**
  - Added Blueprint node for physics constraints
  - Compatible with Unreal Engine 5.4
- **Breaking changes**
  - Minimum Unreal Engine version is now 5.2

## C#

- **Additions**
  - Added `Skeleton.FindSlider()` to query sliders by name (ported from Java)
  - Added `Slider` and `SliderData` classes (ported from Java)
- **Breaking changes**
  - `Skeleton.UpdateWorldTransform()` now requires `Physics` parameter (ported from Java)
  - Renamed `TrackEntry.AttachmentThreshold` to `TrackEntry.MixAttachmentThreshold`
- **Changes of default values**
  - Changed default `SkeletonGraphic.MeshScale` from 1.0 to calculated value
- **Deprecated**
  - Deprecated `Bone.WorldToLocalRotationX` and `Bone.WorldToLocalRotationY`
- **Restructuring (Non-Breaking)**
  - Reorganized shader files into subdirectories

### Unity

- **Additions**
  - Added URP 2D shader variant
  - New Inspector property `Layout Scale Mode` for SkeletonGraphic
- **Breaking changes**
  - Changed default materials to have `CanvasGroup Compatible` disabled
- **Changes of default values**
  - Changed default atlas texture workflow from PMA to straight alpha textures

### MonoGame

- **Additions**
  - Updated to MonoGame 3.8.2
- **Restructuring (Non-Breaking)**
  - Updated project structure to use .NET 6
```

## Language-Specific Naming

### Java (Reference)
- Instance method: `Skeleton#updateWorldTransform()`
- Static method: `Skeleton.someStaticMethod()`
- Field: `skeleton.time`

### C
- Function: `spSkeleton_updateWorldTransform()`
- Type: `spPhysics`
- Enum: `SP_PHYSICS_UPDATE`

### C++
- Method: `Skeleton::updateWorldTransform()`
- Namespace: `spine::`
- Enum: `Physics::Update`

### C#
- Method: `Skeleton.UpdateWorldTransform()`
- Property: `Skeleton.Time`
- Namespace: `Spine`

### TypeScript/JavaScript
- Method: `skeleton.updateWorldTransform()`
- Property: `skeleton.time`
- Enum: `Physics.update`

### Dart
- Method: `skeleton.updateWorldTransform()`
- Property: `skeleton.time`
- Class: `Skeleton`

### Haxe
- Method: `skeleton.updateWorldTransform()`
- Property: `skeleton.time`
- Package: `spine`

### Swift
- Method: `skeleton.updateWorldTransform()`
- Property: `skeleton.time`
- Class: `Skeleton`

### Godot (GDScript)
- Method: `skeleton.update_world_transform()`
- Signal: `animation_completed`
- Property: `skeleton.time`

## Key Rules
1. **Only document public API changes** - ignore private/internal
2. **One line per change** - be concise
3. **Group related changes** with sub-bullets
4. **Omit empty categories**
5. **Use backticks** for all code references
6. **Check existing entries** in CHANGELOG.md for style reference

## Complete Execution Workflow

1. **Setup**:
   - Navigate to `/spine-runtimes/`
   - Determine current branch: `git branch --show-current`
   - List available branches: `git branch -r | grep origin | grep -E "^  origin/[0-9]\.[0-9]" | sort -V`
   - Ask user which branch to compare against
   - **Check CHANGELOG.md for existing entries under current version** - these must be preserved
   - Load checklist state from this document

2. **Check Java Reference Status**:
   - If **Java** is unchecked in the checklist:
     - **MUST process Java first** - it's the reference implementation
     - Run: `git diff COMPARE_BRANCH..CURRENT_BRANCH -- spine-libgdx/spine-libgdx/src/com/esotericsoftware/spine/`
     - Document public API changes in Java section
     - Check off Java in checklist
   - If Java is already checked, proceed to step 3

3. **Select Runtime to Process**:
   - Show user all **unchecked items** from the checklist
   - Ask: "Which runtime would you like to process? Options:"
     - List all unchecked language runtimes
     - List all unchecked engine integrations (grouped by language)
   - User selects one or multiple runtimes to process

4. **Process Selected Runtime(s)**:
   - For **language runtime**:
     - Port Java reference changes with language-specific syntax
     - Check for additional language-specific changes
     - Write entries under `## LanguageName` in CHANGELOG.md
   - For **engine integration**:
     - Check for engine-specific changes ONLY
     - Write entries under `### EngineName` in CHANGELOG.md
   - Check off processed items in checklist

5. **Repeat or Finalize**:
   - If unchecked items remain, ask if user wants to continue
   - If yes, return to step 3
   - If no or all complete, save CHANGELOG.md and updated checklist