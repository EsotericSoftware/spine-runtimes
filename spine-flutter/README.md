# spine-flutter

The spine-flutter runtime provides functionality to load, manipulate and render [Spine](https://esotericsoftware.com) skeletal animation data using [Flutter](https://flutter.dev/). spine-flutter is based on [spine-c](../spine-c) and supports desktop, mobile, and web Flutter deployment targets.

# See the [spine-flutter documentation](https://esotericsoftware.com/spine-flutter) for in-depth information.

## Licensing

You are welcome to evaluate the Spine Runtimes and the examples we provide in this repository free of charge.

You can integrate the Spine Runtimes into your software free of charge, but users of your software must have their own [Spine license](https://esotericsoftware.com/spine-purchase). Please make your users aware of this requirement! This option is often chosen by those making development tools, such as an SDK, game toolkit, or software library.

In order to distribute your software containing the Spine Runtimes to others that don't have a Spine license, you need a [Spine license](https://esotericsoftware.com/spine-purchase) at the time of integration. Then you can distribute your software containing the Spine Runtimes however you like, provided others don't modify it or use it to create new software. If others want to do that, they'll need their own Spine license.

For the official legal terms governing the Spine Runtimes, please read the [Spine Runtimes License Agreement](https://esotericsoftware.com/spine-runtimes-license) and Section 2 of the [Spine Editor License Agreement](https://esotericsoftware.com/spine-editor-license#s2).

## Spine version

spine-flutter works with data exported from Spine 4.3.xx.

spine-flutter supports all Spine features, except two-color tinting and the screen blend mode.

## Supported platforms
The spine-flutter runtime works on desktop, mobile and web.

## Setup
To add `spine_flutter` to your Flutter project, add the following dependency to your `pubspec.yaml` file:

```yaml
dependencies:
  ...
  # See https://pub.dev/packages/spine_flutter for the latest version
  spine_flutter: ^4.3.0
```

In your `main()`, add this line in the beginning to initialize the Spine Flutter runtime:

```dart
void main() async {
    await initSpineFlutter(enableMemoryDebugging: false);
    ...
}
```

## Example
You can run the example like this:

1. install the [Flutter SDK](https://docs.flutter.dev/get-started/install), then run `flutter doctor` which will instruct you what other dependencies to install.
2. Clone this repository `git clone https://github.com/esotericsoftware/spine-runtimes`
3. Run `setup.sh` in the `spine-flutter/` folder. On Windows, you can use [Git Bash](https://gitforwindows.org/) included in Git for Window to run the `setup.sh` Bash script.

You can then open `spine-flutter` in an IDE or editor of your choice that supports Flutter, like [IntelliJ IDEA/Android Studio](https://docs.flutter.dev/get-started/editor?tab=androidstudio) or [Visual Studio Code](https://docs.flutter.dev/get-started/editor?tab=vscode) to inspect and run the example.

Alternatively, you can run the example from the [command line](https://docs.flutter.dev/get-started/test-drive?tab=terminal).

## Custom attachments

A custom attachment uses a Flutter `Image` that is not part of the skeleton's original texture atlas. This is useful for
runtime customization such as downloaded avatar images, equipment, or clothing.

First decode the image and add it to the same `AtlasFlutter` used to render the skeleton. Then copy the existing region
or mesh attachment and set its region. This example places a copied hoverboard mesh in a custom skin because the
`hoverboard` animation has an attachment timeline for that slot:

```dart
import 'dart:ui' as ui;

import 'package:flutter/services.dart';
import 'package:spine_flutter/spine_flutter.dart';

final data = await rootBundle.load('assets/custom-hoverboard.png');
final bytes = data.buffer.asUint8List(data.offsetInBytes, data.lengthInBytes);
final codec = await ui.instantiateImageCodec(bytes);
late final ui.Image image;
try {
  image = (await codec.getNextFrame()).image;
} finally {
  codec.dispose();
}

late final AtlasRegion region;
try {
  region = drawable.atlasFlutter.addRegion('custom-hoverboard', image);
} finally {
  image.dispose(); // addRegion() keeps its own image handle.
}

final slot = drawable.skeleton.findSlot('hoverboard-board');
final originalAttachment = drawable.skeleton.getAttachment('hoverboard-board', 'hoverboard-board');
if (slot == null || originalAttachment is! MeshAttachment) {
  throw StateError('The hoverboard mesh attachment could not be found.');
}

final customAttachment = originalAttachment.copy() as MeshAttachment;
customAttachment.setRegion(region);

final customSkin = Skin('custom-hoverboard');
customSkin.setAttachment(slot.data.index, 'hoverboard-board', customAttachment);
drawable.skeleton
  ..setSkin2(customSkin)
  ..setupPoseSlots();
drawable.animationState.setAnimation(0, 'hoverboard', true);
drawable.animationState.apply(drawable.skeleton);

// Later, when cleaning up, dispose the skeleton before the skin. The skin owns customAttachment.
drawable.dispose();
customSkin.dispose();
```

`setRegion()` supports `RegionAttachment` and `MeshAttachment`. It preserves the attachment's geometry. Region
attachments may need their dimensions adjusted to match a differently shaped image. Mesh attachments retain their
existing vertices and UV layout, so replacement artwork should use a compatible layout. Copying avoids changing an
attachment that may be shared by other skeletons.

For a slot without an attachment timeline, the copied attachment can be assigned directly to `slot.pose.attachment`.
Such a copy remains caller-owned and must be disposed after no slot or skeleton uses it. An attachment timeline can
replace a direct assignment. Adding the copy under the same skin placeholder, as above, lets the timeline resolve the
custom attachment instead. A `Skin` owns attachments added to it; keep the skin alive while a skeleton uses it and do
not also dispose those attachments separately.

The atlas owns the returned region, so do not dispose it. The region must only be rendered with the `AtlasFlutter` that
created it and must remain valid while an attachment may use it. Each `addRegion()` call creates a full-image atlas page
and retains its cloned image handle until the atlas is disposed. There is no packing, cropping, deduplication, or
individual removal, so reuse returned regions rather than repeatedly adding the same image. Separate pages may also
prevent draw-call batching between attachments.

See [`custom_attachment.dart`](example/lib/custom_attachment.dart) for a complete animated example, including cleanup.

## Development
Run `./setup.sh` to copy over the spine-cpp and spine-c sources. This step needs to be executed every time spine-cpp or spine-c changes.

If all you modify are the Dart sources of the plugin, then the development setup is the same as the setup described under "Example" above.

If you need to update or modify the bindings generated from spine-c, run `./generate-bindings.sh`. If you regenerate the bindings, you must also compile the WASM binaries via `./compile-wasm.sh`.

The `./test` folder contains headless tests for the [spine-c](../spine-c) bindings and Flutter rendering tests.

## Releasing

`spine-flutter` is released to [pub.dev](https://pub.dev/packages/spine_flutter) using GitHub Actions and pub.dev automated publishing. The pub.dev package is configured to trust tags from `EsotericSoftware/spine-runtimes` matching `spine-flutter-{{version}}`.

1. Set the release version in `spine-flutter/pubspec.yaml`:

```yaml
version: 4.3.4
```

2. Add a matching entry to `spine-flutter/CHANGELOG.md`.

3. Commit and push the release version:

```bash
git add spine-flutter/pubspec.yaml spine-flutter/CHANGELOG.md
git commit -m "[flutter] Release spine-flutter 4.3.4"
git push origin 4.3
```

4. Tag that commit and push the tag:

```bash
git tag spine-flutter-4.3.4
git push origin spine-flutter-4.3.4
```

The tag triggers the GitHub Actions release workflow. It verifies the tag version, runs `./generate-bindings.sh` to copy native sources and compile WebAssembly, runs the spine-flutter tests, verifies generated source files are committed, then publishes to pub.dev from the same workspace so the copied native sources are included in the package.

5. Check the workflow result and pub.dev.

If publishing fails before upload, fix the issue and push a new release version. Versions uploaded to pub.dev cannot be overwritten.
