# spine-libgdx

The spine-libgdx runtime provides functionality to load, manipulate and render [Spine](https://esotericsoftware.com) skeletal animation data using [libgdx](http://www.libgdx.com/).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](https://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](https://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-libgdx works with data exported from Spine 4.3.xx.

spine-libgdx supports all Spine features and is the reference runtime implementation.

## Setup

The simplest way to add `spine-libgdx` to your libGDX project is to copy the contents of the `spine-libgdx/src` folder to your project's source folder. However, this is not the recommended approach. Generally, you want to use `spine-libgdx` by depending on it via `Maven` or `Gradle`.

## Maven & Gradle

The spine-libgdx runtime is released to Maven Central through SonaType. We also deploy snapshot builds on every commit to the repository via [GitHub Actions](https://github.com/EsotericSoftware/spine-runtimes/actions).

### Versions

You can find the latest version for release builds [here](https://central.sonatype.com/artifact/com.esotericsoftware.spine/spine-libgdx).

You can find the latest SNAPSHOT version in this listing in the SonaType snapshot repository (https://oss.sonatype.org/content/repositories/snapshots/com/esotericsoftware/spine/spine-libgdx/).

You can also build and install `spine-libgdx` into your local Maven repository:

```
cd spine-libgdx/spine-libgdx
mvn install
```

The version number is composed of the corresponding editor `major.minor` version, and runtime update version for the runtime. E.g. `4.1.10` means editor version 4.1, runtime update version 10. All runtime versions are compatible with the exports from the correspongind `major.minor` editor version.

### Maven

To add the spine-libgdx runtime to your Maven project, add this dependency:

```
<dependency>
	<groupId>com.esotericsoftware.spine</groupId>
	<artifactId>spine-libgdx</artifactId>
	<version>4.3.0</version>
</dependency>
```

For SNAPSHOT versions, add the SonaType Snapshot repository to your `pom.xml`:

```
<repositories>
	<repository>
		<id>nightlies</id>
		<url>https://oss.sonatype.org/content/repositories/snapshots/</url>
	</repository>
</repositories>
```

### Gradle

To add the spine-libgdx runtime to your libGDX Gradle project, add the following dependencies to the `core` project in the `build.gradle` file at the root of your libGDX project:

```
project(":core") {
    apply plugin: "java"

    dependencies {
        compile "com.badlogicgames.gdx:gdx:$gdxVersion"
        compile "com.badlogicgames.gdx:gdx-box2d:$gdxVersion"

        compile "com.esotericsoftware.spine:spine-libgdx:4.3.+"
    }
}
```

Note that `4.3.+` will pull in the latest `-SNAPSHOT` release. Our snapshot releases are considered stable and based on the latest commit to the Spine Runtimes branch corresponding to the latest Spine Editor release version.

## Running the examples

Clone this repository and load the `spine-libgdx/build.gradle` file with IntelliJ IDEA or Eclipse. Alterantively, you can run `./gradlew eclipse` on the command line to generate Eclipse projects without having to use the Gradle build.

The `spine-libgdx-tests` project has various examples you can inspect and run.

## Building SkeletonViewer

To build SkeletonViewer, run:

```
./gradlew spine-libgdx:jar spine-skeletonviewer.jar
```

You can then find an uber-jar of SkeletonViewer in `spine-skeletonviewer/build/libs/spine-skeletonviewer.jar`. You can run it via `java -jar spine-skeletonviewer.jar` or double clicking it in the file explorer.

# Releasing

`spine-libgdx` and `spine-android` are released together. `spine-android` uses the version from `spine-libgdx/gradle.properties`.

1. Set the release version in `spine-libgdx/gradle.properties`:

```
version=4.3.2
```

Do not use `-SNAPSHOT` for a release.

2. Commit and push the release version:

```
git add spine-libgdx/gradle.properties
git commit -m "[libgdx][android] Release 4.3.2"
git push origin 4.3
```

3. Tag that commit and push the tag:

```
git tag spine-libgdx-4.3.2
git push origin spine-libgdx-4.3.2
```

The tag triggers the GitHub Actions release workflow. It verifies the tag version, publishes `spine-libgdx`, then publishes `spine-android`.

4. Check the workflow result and Maven Central.

If JReleaser fails before upload/validation, the artifacts were not published. If JReleaser uploads and validates successfully but later times out waiting for Central Portal's publishing step, the GitHub job may fail even though publishing still succeeds. Check Central Portal or Maven Central before retrying.

5. Bump to the next snapshot and push:

```
# spine-libgdx/gradle.properties
version=4.3.3-SNAPSHOT

git add spine-libgdx/gradle.properties
git commit -m "[libgdx][android] Begin 4.3.3-SNAPSHOT"
git push origin 4.3
```
