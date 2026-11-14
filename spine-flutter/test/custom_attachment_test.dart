//
// Spine Runtimes License Agreement
// Last updated April 5, 2025. Replaces all prior versions.
//
// Copyright (c) 2013-2025, Esoteric Software LLC
//
// Integration of the Spine Runtimes into software or otherwise creating
// derivative works of the Spine Runtimes is permitted under the terms and
// conditions of Section 2 of the Spine Editor License Agreement:
// http://esotericsoftware.com/spine-editor-license
//
// Otherwise, it is permitted to integrate the Spine Runtimes into software
// or otherwise create derivative works of the Spine Runtimes (collectively,
// "Products"), provided that each user of the Products must obtain their own
// Spine Editor license and redistribution of the Products in any form must
// include this license and copyright notice.
//
// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

import 'dart:io';
import 'dart:typed_data';
import 'dart:ui' as ui;

import 'package:flutter_test/flutter_test.dart';
import 'package:spine_flutter/spine_flutter.dart';

Future<Uint8List> _loadSpineboyFile(String name) {
  final fileName = name.replaceAll('\\', '/').split('/').last;
  return File('example/assets/$fileName').readAsBytes();
}

Future<ui.Image> _createSolidImage(ui.Color color) async {
  final recorder = ui.PictureRecorder();
  final canvas = ui.Canvas(recorder);
  canvas.drawRect(
    const ui.Rect.fromLTWH(0, 0, 64, 64),
    ui.Paint()..color = color,
  );
  final picture = recorder.endRecording();
  final image = await picture.toImage(64, 64);
  picture.dispose();
  return image;
}

int _countMagentaPixels(RawImageData image) {
  var count = 0;
  for (var i = 0; i < image.pixels.length; i += 4) {
    if (image.pixels[i] >= 240 &&
        image.pixels[i + 1] <= 20 &&
        image.pixels[i + 2] >= 240 &&
        image.pixels[i + 3] >= 240) {
      count++;
    }
  }
  return count;
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  test('custom image regions render with copied region and mesh attachments', () async {
    await initSpineFlutter(enableMemoryDebugging: true);
    final drawable = await SkeletonDrawableFlutter.fromMemory(
      'spineboy.atlas',
      'spineboy-pro.json',
      _loadSpineboyFile,
    );
    drawable.skeleton
      ..setupPose()
      ..updateWorldTransform(Physics.none);

    final originalPageCount = drawable.atlasFlutter.pages.length;
    final sourceImage = await _createSolidImage(const ui.Color(0xFFFF00FF));
    expect(() => drawable.atlasFlutter.addRegion('', sourceImage), throwsArgumentError);

    drawable.atlasFlutter.atlasPagePaints.add(<BlendMode, ui.Paint>{});
    expect(() => drawable.atlasFlutter.addRegion('inconsistent', sourceImage), throwsStateError);
    drawable.atlasFlutter.atlasPagePaints.removeLast();

    final customRegion = drawable.atlasFlutter.addRegion('custom', sourceImage);
    expect(sourceImage.debugGetOpenHandleStackTraces(), isNotEmpty);
    sourceImage.dispose();
    expect(sourceImage.debugGetOpenHandleStackTraces(), isNotEmpty);
    expect(customRegion.name, 'custom');
    expect(customRegion.page?.name, 'custom');
    expect(customRegion.page?.index, originalPageCount);
    expect(customRegion.page?.width, 64);
    expect(customRegion.page?.height, 64);
    expect(customRegion.originalWidth, 64);
    expect(customRegion.originalHeight, 64);
    expect(customRegion.u, 0);
    expect(customRegion.v, 0);
    expect(customRegion.u2, 1);
    expect(customRegion.v2, 1);
    expect(drawable.atlasFlutter.findRegion('custom')?.nativePtr.address, customRegion.nativePtr.address);
    expect(drawable.atlasFlutter.pages.length, originalPageCount + 1);
    expect(drawable.atlasFlutter.atlasPages.length, originalPageCount + 1);
    expect(drawable.atlasFlutter.atlasPagePaints.length, originalPageCount + 1);
    expect(drawable.atlasFlutter.atlasPagePaints.last.length, BlendMode.values.length);

    final pointAttachment = PointAttachment('point');
    expect(() => pointAttachment.setRegion(customRegion), throwsUnsupportedError);
    pointAttachment.dispose();

    final regionSlot = drawable.skeleton.findSlot('gun')!;
    final originalRegionAttachment = regionSlot.pose.attachment!;
    final customRegionAttachment = originalRegionAttachment.copy() as RegionAttachment;
    final originalWidth = customRegionAttachment.width;
    final originalHeight = customRegionAttachment.height;
    expect(() => customRegionAttachment.setRegion(customRegion, sequenceIndex: -1), throwsRangeError);
    expect(() => customRegionAttachment.setRegion(customRegion, sequenceIndex: 1), throwsRangeError);

    final sequenceRegions = customRegionAttachment.sequence.regions;
    sequenceRegions.add(sequenceRegions[0]);
    customRegionAttachment.setRegion(customRegion, sequenceIndex: 1);
    expect(sequenceRegions[1]?.rendererObject?.address, originalPageCount);
    expect(customRegionAttachment.width, originalWidth);
    expect(customRegionAttachment.height, originalHeight);

    regionSlot.pose.attachment = customRegionAttachment;
    regionSlot.pose.sequenceIndex = 1;
    drawable.skeleton.updateWorldTransform(Physics.none);
    expect(drawable.renderFlutter().any((command) => command.atlasPageIndex == originalPageCount), isTrue);
    expect(_countMagentaPixels(await drawable.renderToRawImageData(512, 512, 0xFFFFFFFF)), greaterThan(0));
    regionSlot.pose.attachment = originalRegionAttachment;
    customRegionAttachment.dispose();

    final meshSlot = drawable.skeleton.findSlot('hoverboard-board')!;
    final originalSkin = drawable.skeleton.skin;
    final originalMeshAttachment =
        drawable.skeleton.getAttachment('hoverboard-board', 'hoverboard-board')! as MeshAttachment;
    final customMeshAttachment = originalMeshAttachment.copy() as MeshAttachment;
    customMeshAttachment.setRegion(customRegion);

    final customSkin = Skin('custom-hoverboard');
    customSkin.setAttachment(meshSlot.data.index, 'hoverboard-board', customMeshAttachment);
    drawable.skeleton
      ..setSkin2(customSkin)
      ..setupPoseSlots();
    drawable.animationState
      ..setAnimation(0, 'hoverboard', true)
      ..update(0.25)
      ..apply(drawable.skeleton);
    drawable.skeleton.updateWorldTransform(Physics.none);
    expect(meshSlot.pose.attachment?.nativePtr.address, customMeshAttachment.nativePtr.address);
    expect(drawable.renderFlutter().any((command) => command.atlasPageIndex == originalPageCount), isTrue);
    expect(_countMagentaPixels(await drawable.renderToRawImageData(512, 512, 0xFFFFFFFF)), greaterThan(0));

    // The attachment timeline resolves through the selected skin on every apply.
    drawable.skeleton
      ..setSkin2(originalSkin)
      ..setupPoseSlots();
    drawable.animationState.apply(drawable.skeleton);
    expect(meshSlot.pose.attachment?.nativePtr.address, originalMeshAttachment.nativePtr.address);

    drawable.skeleton
      ..setSkin2(customSkin)
      ..setupPoseSlots();
    drawable.animationState.apply(drawable.skeleton);
    expect(meshSlot.pose.attachment?.nativePtr.address, customMeshAttachment.nativePtr.address);

    final imageAfterDispose = await _createSolidImage(const ui.Color(0xFF00FFFF));
    drawable.dispose();
    customSkin.dispose(); // The skin owns and disposes customMeshAttachment.
    expect(sourceImage.debugGetOpenHandleStackTraces(), isEmpty);
    expect(() => drawable.atlasFlutter.addRegion('after-dispose', imageAfterDispose), throwsStateError);
    imageAfterDispose.dispose();
    reportLeaks();
  });
}
