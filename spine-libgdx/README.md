# spine-libgdx

The spine-libgdx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [libgdx](http://www.libgdx.com/).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-libgdx works with data exported from Spine 4.1.xx.

spine-libgdx supports all Spine features and is the reference runtime implementation.

## Setup
The simplest way to add `spine-libgdx` to your libGDX project is to copy the contents of the `spine-libgdx/src` folder to your project's source folder. However, this is not the recommended approach. Generally, you want to use `spine-libgdx` by depending on it via `Maven` or `Gradle`.

## Maven & Gradle
The spine-libgdx runtime is released to Maven Central through SonaType. We also deploy snapshot builds on every commit to the repository via [GitHub Actions](https://github.com/EsotericSoftware/spine-runtimes/actions).

### Versions

You can find the latest version for release builds [here](http://search.maven.org/#search%7Cga%7C1%7Cspine-libgdx).

You can find the latest SNAPSHOT version in the project's [pom.xml](spine-libgdx/pom.xml#L13).

You can also build and install `spine-libgdx` into your local Maven repository:

```
cd spine-libgdx/spine-libgdx
mvn install
```

Up until Spine 4.0, the version number is composed of the editor number at the time of release of the Maven artifact plus a patch number at the end. E.g. `4.0.18.1` means editor version `4.0.18`, and patch version `1` for the runtime. The editor version is updated everytime a new editor release is performed, the patch version is updated everytime a new fix or enhancement is released in the runtime.

Starting from Spine 4.1, the version number is composed of the corresponding editor `major.minor` version, and runtime update version for the runtime. E.g. `4.1.10` means editor version 4.1, runtime update version 10. All runtime versions are compatible with the exports from the correspongind `major.minor` editor version.

### Maven
To add the spine-libgdx runtime to your Maven project, add this dependency:

```
<depenency>
	<groupId>com.esotericsoftware.spine</groupId>
	<artifactId>spine-libgdx</artifactId>
	<version>4.0.18.1</version>
</depenency>
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

        compile "com.esotericsoftware.spine:spine-libgdx:4.0.18.1"
    }
}
```

## Running the examples
Clone this repository and load the `spine-libgdx/build.gradle` file with IntelliJ IDEA or Eclipse. Alterantively, you can run `./gradlew eclipse` on the command line to generate Eclipse projects without having to use the Gradle build.

The `spine-libgdx-tests` project has various examples you can inspect and run.

## Building SkeletonViewer
To build SkeletonViewer, run:

```
./gradlew spine-libgdx:jar spine-skeletonviewer.jar
```

You can then find an uber-jar of SkeletonViewer in `spine-skeletonviewer/build/libs/spine-skeletonviewer.jar`. You can run it via `java -jar spine-skeletonviewer.jar` or double clicking it in the file explorer.