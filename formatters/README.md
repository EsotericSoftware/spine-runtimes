# Spine Runtimes Code Formatters

This directory contains formatting scripts and tools to ensure consistent code style across all Spine runtime implementations.

## Scripts

### format-cpp.sh
Formats C/C++ source files using clang-format.
- **Prerequisites**: clang-format 18.1.8
- **Coverage**: All C/C++ files in spine-cpp, spine-c, spine-godot, spine-ue, spine-glfw, spine-sdl, spine-sfml, spine-ios, spine-flutter
- **Configuration**: Uses .clang-format configuration file
- **Behavior**: Batches all files in a single clang-format call for performance

### format-csharp.sh
Formats C# source files using dotnet format.
- **Prerequisites**: .NET SDK with dotnet format tool
- **Coverage**: All C# files in spine-csharp, spine-monogame, spine-unity
- **Configuration**: Uses .editorconfig (temporarily copied to each project)
- **Behavior**: Runs with --no-restore and --verbosity quiet to suppress warnings

### format-dart.sh
Formats Dart source files using dart format.
- **Prerequisites**: Dart SDK
- **Coverage**: All Dart files in spine-flutter
- **Configuration**: Uses --page-width 120 parameter
- **Behavior**: Formats all files in place

### format-haxe.sh
Formats Haxe source files using haxelib formatter.
- **Prerequisites**: Haxe and haxelib formatter package
- **Coverage**: All Haxe files in spine-haxe
- **Configuration**: Uses hxformat.json configuration file
- **Behavior**: Formats all files in place

### format-java.sh
Formats Java source files using a custom Eclipse formatter.
- **Prerequisites**: Java JDK and Maven
- **Coverage**: All Java files in spine-libgdx and spine-android
- **Configuration**: Uses eclipse-formatter.xml configuration file
- **Behavior**: Builds Eclipse formatter JAR if needed, only outputs changed files

### format-ts.sh
Formats TypeScript source files using typescript-formatter.
- **Prerequisites**: Node.js and npm
- **Coverage**: All TypeScript files in spine-ts and tests
- **Configuration**: Uses tsfmt.json configuration files
- **Behavior**: Uses npx to auto-download formatter, validates tsfmt.json consistency

### setup-clang-format-docker.sh
Helper script for GitHub Actions to set up clang-format via Docker.
- **Prerequisites**: Docker
- **Purpose**: Creates a wrapper script that runs clang-format 18.1.8 in a Docker container
- **Usage**: Called by GitHub Actions workflow to ensure version consistency

## Configuration Files

- **.clang-format**: C/C++ formatting rules
- **.editorconfig**: C# formatting rules
- **eclipse-formatter.xml**: Java formatting rules for Eclipse formatter
- **hxformat.json**: Haxe formatting rules

## Eclipse Formatter

The eclipse-formatter directory contains a Maven project that builds a standalone Eclipse code formatter:
- **Source**: eclipse-formatter/src/main/java/com/esotericsoftware/spine/formatter/EclipseFormatter.java
- **Build**: Automatically built by format-java.sh when the JAR doesn't exist or source is newer
- **Output**: Only prints files that were actually modified

## GitHub Actions Workflow

The formatting check runs automatically on push and can be triggered manually:

1. **Workflow file**: .github/workflows/format-check-new.yml
2. **Process**:
   - Sets up all required formatters and dependencies
   - Runs all format scripts
   - Captures any file changes to format-diff.txt
   - Uploads diff as artifact
   - Fails if any files were modified

### Docker-based clang-format

To ensure consistent formatting across local development and CI, the workflow uses Docker to run clang-format 18.1.8. The setup-clang-format-docker.sh script creates a wrapper that:
- Mounts the project directory in the Docker container
- Converts relative paths to absolute paths
- Runs clang-format with the same version everywhere

## Local Development

To run formatters locally:

```bash
cd formatters
./format-cpp.sh     # Format C/C++ files
./format-csharp.sh  # Format C# files
./format-dart.sh    # Format Dart files
./format-haxe.sh    # Format Haxe files
./format-java.sh    # Format Java files
./format-ts.sh      # Format TypeScript files
./format.sh         # Format everything
```

Ensure you have the required tools installed for each formatter you want to run.