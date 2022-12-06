# 4.2.10
* Update README.md to point to Junji's Dart-only Spine runtime.

# 4.2.9
* Fix atlas parsing.

# 4.2.8
* Change reversed positional argument order in `SpineWidget` constructors.

# 4.2.7
* Change package name from  `esotericsoftware_spine_flutter` to `spine_flutter`.

# 4.2.6
* Fix analyzer errors, fix code style to adhere to Dart standards.

# 4.2.5
* Implemented batching of render commands, reducing the number of draw calls. 60/120fps for 100 Spineboys on all platforms.

# 0.0.4
* Clean-up `fromAsset()` factory methods so the atlas comes before skeleton data file name.
* Rename `Vector2` to `Vec2`.
* Make the bundle configurable in `SpineWidget.asset()`.

# 0.0.3
* Lower macOS deployment target to 10.11.

# 0.0.2
* Fix package name in build system `spine_flutter` > `esotericsoftware_spine_flutter`.

# 0.0.1
Initial test release.