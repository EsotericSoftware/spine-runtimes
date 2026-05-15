# Project: Spine Runtimes

Multi-language runtime libraries for Spine 2D skeletal animations. Provides core runtimes for different programming languages and integrations for game engines and frameworks, supporting advanced skeletal animation with physics, constraints, and dynamic content.

## Features

### Core Animation Features
- **Skeletal Animation**: Hierarchical bone-based animation system
- **Inverse Kinematics (IK)**: 1-2 bone IK constraints for natural positioning
- **Path Constraints**: Bones follow Bezier curve paths for complex motion
- **Transform Constraints**: Copy/mix transformations between bones
- **Physics Constraints**: Real-time physics simulation (4.2+)
- **Animation Mixing/Crossfading**: Smooth transitions with configurable durations
- **Animation Layering**: Multiple animations on different tracks

### Visual System
- **Attachments**: Regions, meshes, clipping, bounding boxes, paths, points, nested skeletons
- **Skins System**: Dynamic attachment swapping and layering
- **Blend Modes**: Normal, additive, multiply, screen rendering
- **Two-Color Tinting**: Advanced color effect
- **Sequences**: Frame-by-frame texture animation within skeletal structure
- **Events System**: Timeline-based event triggering for game logic

## Tech Stack

### Languages
- **Java** (spine-libgdx) - Reference implementation
- **C++** (spine-cpp) - C++11 with custom RTTI
- **C#** (spine-csharp) - .NET Framework 4.8, Unity 2018.3+
- **TypeScript/JavaScript** (spine-ts) - ES2015+ with ESM modules
- **C** (spine-c) - Standalone implementation with codegen
- **Haxe** (spine-haxe) - Cross-platform runtime
- **Swift** (spine-ios) - iOS native (5.0+)
- **Dart** (spine-flutter) - Flutter runtime (>=3.16.0)

### Build Systems
- **Gradle** (Kotlin DSL) - Java/Android projects
- **CMake** - C/C++ projects with presets
- **npm/esbuild** - TypeScript ecosystem
- **MSBuild/.NET CLI** - C# projects
- **Swift Package Manager** - iOS packages
- **SCons** - Godot extension builds
- **Flutter pub** - Dart package management

### Frameworks & Engines
- **Game Engines**: Unity, Unreal Engine (4.27-5.5), Godot, libGDX
- **Web**: Phaser.js (v3/v4), PixiJS (v7/v8), Three.js, HTML5 Canvas, WebGL
- **Mobile/Desktop**: Flutter, MonoGame, SDL, SFML, GLFW
- **Graphics**: OpenGL/ES, WebGL, Metal, DirectX (UE)

## Structure

### Core Runtimes (Language-specific)
- **spine-libgdx** (Java) - Reference implementation
- **spine-cpp** (C++) - Complete port with spine-cpp-lite FFI wrapper
- **spine-csharp** (C#) - .NET implementation
- **spine-ts** (TypeScript) - Modular web ecosystem
- **spine-c** (C) - Auto-generated from spine-cpp
- **spine-haxe** (Haxe) - Cross-platform port

### Integration Runtimes (Engine-specific)
**Using spine-cpp:**
- spine-flutter, spine-glfw, spine-godot, spine-ios, spine-sdl, spine-sfml, spine-ue

**Using spine-csharp:**
- spine-unity, spine-monogame, spine-xna

**Using spine-ts:**
- spine-canvas, spine-webgl, spine-phaser, spine-pixi, spine-threejs, spine-player, spine-canvaskit, spine-webcomponents

**Using spine-libgdx:**
- spine-android

### Special Components
- **spine-cpp-lite**: C FFI wrapper enabling Dart/Swift bindings
- **formatters/**: Universal code formatting for all languages
- **tests/**: Cross-runtime serialization validation
- **examples/**: Rich animation examples and educational content

## Architecture

### Reference Implementation Pattern
- **spine-libgdx** serves as authoritative reference
- All other core runtimes are systematic ports maintaining API compatibility
- Changes originate in spine-libgdx and are ported to other languages

### Data/Instance Separation
- **Data Classes** (`SkeletonData`, `BoneData`): Immutable setup pose
- **Instance Classes** (`Skeleton`, `Bone`): Mutable runtime state
- **Pose Classes**: Applied transformations and constraints (4.3+)

### Update Pipeline
1. Animation update (`AnimationState.update()`)
2. Apply animation to skeleton (`AnimationState.apply()`)
3. Skeleton update with physics (`Skeleton.update()`)
4. World transform update (`Skeleton.updateWorldTransform(Physics)`)

## Commands

### Build Commands
- **spine-libgdx**: `./gradlew build`
- **spine-cpp**: `./build.sh [release|clean|nofileio]`
- **spine-c**: `./build.sh [release|clean|codegen]`
- **spine-ts**: `npm install && npm run build`
- **spine-csharp**: `dotnet build`
- **spine-flutter**: `./setup.sh && flutter build`
- **spine-godot**: `scons [platform=linux|windows|macos]`
- **spine-haxe**: `./build.sh`
- **spine-ios**: `swift build`

### Test Commands
- **Cross-runtime**: `./tests/test.sh`
- **spine-cpp**: `./tests/test.sh`
- **spine-libgdx**: `./gradlew test`
- **spine-ts**: `npx tsx src/headless-test-runner.ts`
- **spine-flutter**: `flutter test`

### Utility Commands
- **Format all**: `./formatters/format.sh`
- **Format specific**: `./formatters/format.sh [java|ts|cpp|csharp|dart|haxe|swift]`

## Testing

### Cross-Runtime Validation
- Serialization consistency tests between Java, C++, C, and TypeScript
- Headless test runners for automated validation
- Output comparison for JSON/binary format validation

### Language-Specific Testing
- **Java**: JUnit tests via Gradle
- **C++**: Custom headless tests with multiple build variants
- **TypeScript**: Custom test runner with tsx
- **Flutter**: Flutter test framework
- **C#**: .NET test framework

### CI/CD Testing
- GitHub Actions workflows for all runtimes
- Automated formatting validation
- Cross-platform compatibility testing
- Automated publishing pipelines

## Development Workflow

1. **Changes begin in spine-libgdx** (reference implementation)
2. **Port to core runtimes** using git diffs and systematic translation
3. **Integration runtimes inherit** changes from their core runtime
4. **Validate with cross-runtime tests** to ensure consistency
5. **Format code** using universal formatting tools
6. **Publish** to appropriate package repositories (Maven, NPM, NuGet, etc.)

## Version Management
- **Current**: 4.3-beta (development branch)
- **Stable**: 4.2 (main branch for PRs)