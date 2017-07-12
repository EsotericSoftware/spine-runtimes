# spine-libgdx

The spine-libgdx runtime provides functionality to load, manipulate and render [Spine](http://esotericsoftware.com) skeletal animation data using [libgdx](http://www.libgdx.com/).

## Licensing

This Spine Runtime may only be used for personal or internal use, typically to evaluate Spine before purchasing. If you would like to incorporate a Spine Runtime into your applications, distribute software containing a Spine Runtime, or modify a Spine Runtime, then you will need a valid [Spine license](https://esotericsoftware.com/spine-purchase). Please see the [Spine Runtimes Software License](https://github.com/EsotericSoftware/spine-runtimes/blob/master/LICENSE) for detailed information.

The Spine Runtimes are developed with the intent to be used with data exported from Spine. By purchasing Spine, `Section 2` of the [Spine Software License](https://esotericsoftware.com/files/license.txt) grants the right to create and distribute derivative works of the Spine Runtimes.

## Spine version

spine-libgdx works with data exported from Spine 3.6.xx.

spine-libgdx supports all Spine features and is the reference runtime implementation.

## Setup

1. Download the Spine Runtimes source using [git](https://help.github.com/articles/set-up-git) or by downloading it [as a zip](https://github.com/EsotericSoftware/spine-runtimes/archive/3.6.zip).
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

You can find the latest SNAPSHOT version in the project's [pom.xml](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx/pom.xml#L13).

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
* [Simple example 1](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest1.java) Simplest possible example, fully commented.
* [Simple example 2](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest2.java) Shows events and bounding box hit detection.
* [Simple example 3](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/SimpleTest3.java) Shows mesh rendering and IK using the raptor example.
* [More examples](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-libgdx/spine-libgdx-tests/src/com/esotericsoftware/spine/)
