# spine-libgdx

The spine-libgdx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [libgdx](http://www.libgdx.com/).

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](http://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](http://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-libgdx works with data exported from Spine 3.8.xx.

spine-libgdx supports all Spine features and is the reference runtime implementation.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it as a zip via the download button above.
1. Using Eclipse, import the project by choosing File -> Import -> Existing projects. For other IDEs you will need to create a new project and import the source.

Alternatively, the contents of the `spine-libgdx/src` directory can be copied into your project.

## Notes

* The "test" source directory contains optional examples.
* spine-libgdx depends on the gdx-backend-lwjgl project so the tests can easily be run on the desktop. If the tests are excluded, spine-libgdx only needs to depend on the gdx project.
* spine-libgdx depends on the gdx-box2d extension project solely for the `Box2DExample` test.

## Maven & Gradle
The spine-libgdx runtime is released to Maven Central through SonaType. We also deploy snapshot builds on every commit to the master repository. You can find the Jenkins build [here](http://libgdx.badlogicgames.com:8080/job/spine-libgdx/).

### Versions

You can find the latest version for release builds [here](http://search.maven.org/#search%7Cga%7C1%7Cspine-libgdx).

You can find the latest SNAPSHOT version in the project's [pom.xml](spine-libgdx/pom.xml#L13).

If you want to use a different branch, e.g. `3.6-beta`, build the artifact locally:

```
cd spine-libgdx/spine-libgdx
mvn install
```

The version number is composed of the editor number at the time of release of the Maven artifact plus a patch number at the end. E.g. `3.5.51.3` means editor version `3.5.51`, and patch version `3` for the runtime. The editor version is updated everytime a new editor release is performed, the patch version is updated everytime a new fix or enhancement is released in the runtime.


### Maven
To add the spine-libgdx runtime to your Maven project, add this dependency:

```
<depenency>
	<groupId>com.esotericsoftware.spine</groupId>
	<artifactId>spine-libgdx</artifactId>
	<version>3.5.51.1</version>
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

        compile "com.esotericsoftware.spine:spine-libgdx:3.5.51.1"
    }
}
```

## Examples

* [HTML5 example](http://esotericsoftware.com/files/runtimes/spine-libgdx/raptor/)
* [Super Spineboy](https://github.com/EsotericSoftware/spine-superspineboy) Full game example done with Spine Essential, includes source code.
* [Simple example 1](spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest1.java) Simplest possible example, fully commented.
* [Simple example 2](spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest2.java) Shows events and bounding box hit detection.
* [Simple example 3](spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest3.java) Shows mesh rendering and IK using the raptor example.
* [More examples](spine-libgdx-tests/src/com/esotericsoftware/spine/)
