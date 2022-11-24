import 'dart:math';
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:flutter/painting.dart' as painting;
import 'package:esotericsoftware_spine_flutter/spine_flutter.dart';
import 'package:flutter/services.dart';
import 'package:raw_image_provider/raw_image_provider.dart';

class DressUp extends StatefulWidget {
  const DressUp({Key? key}) : super(key: key);

  @override
  DressUpState createState() => DressUpState();
}

class DressUpState extends State<DressUp> {
  static const double thumbnailSize = 200;
  late SpineWidgetController _controller;
  SkeletonDrawable? _drawable;
  Skin? _customSkin;
  final Map<String, RawImageData> _skinImages = {};
  final Map<String, bool> _selectedSkins = {};

  @override
  void initState() {
    reportLeaks();
    super.initState();
    SkeletonDrawable.fromAsset("assets/mix-and-match.atlas", "assets/mix-and-match-pro.skel").then((drawable) async {
      for (var skin in drawable.skeletonData.getSkins()) {
        if (skin.getName() == "default") continue;

        var skeleton = drawable.skeleton;
        skeleton.setSkin(skin);
        skeleton.setToSetupPose();
        skeleton.updateWorldTransform();
        var bounds = skeleton.getBounds();
        var scale = 1 / (bounds.width > bounds.height ? bounds.width / thumbnailSize : bounds.height / thumbnailSize);

        var recorder = ui.PictureRecorder();
        var canvas = Canvas(recorder);
        var bgColor = Random().nextInt(0xffffffff) | 0xff0000000;
        var paint = Paint()
          ..color = ui.Color(bgColor)
          ..style = PaintingStyle.fill;
        canvas.drawRect(const Rect.fromLTWH(0, 0, thumbnailSize, thumbnailSize), paint);
        canvas.translate(thumbnailSize / 2, thumbnailSize / 2);
        canvas.scale(scale, scale);
        canvas.translate(-(bounds.x + bounds.width / 2), -(bounds.y + bounds.height / 2));
        canvas.drawRect(Rect.fromLTRB(-5, -5, 5, -5), paint..color = Colors.red);
        drawable.renderToCanvas(canvas);

        var rawImageData = (await (await recorder.endRecording().toImage(thumbnailSize.toInt(), thumbnailSize.toInt())).toByteData(format: ui.ImageByteFormat.rawRgba))!.buffer.asUint8List();
        _skinImages[skin.getName()] = (RawImageData(rawImageData, thumbnailSize.toInt(), thumbnailSize.toInt()));
        _selectedSkins[skin.getName()] = false;
      }
      _drawable = drawable;
      _controller = SpineWidgetController(onInitialized: (controller) {
        controller.animationState.setAnimationByName(0, "dance", true);
      });
      setState(() {
        _selectedSkins["full-skins/girl"] = true;
        drawable.skeleton.setSkinByName("full-skins/girl");
        drawable.skeleton.setToSetupPose();
      });
    });
  }

  void _toggleSkin(String skinName) {
    _selectedSkins[skinName] = !_selectedSkins[skinName]!;
    if (_customSkin != null) _customSkin?.dispose();
    _customSkin = Skin("custom-skin");
    for (var skinName in _selectedSkins.keys) {
      if (_selectedSkins[skinName] == true) {
        var skin = _controller.skeletonData.findSkin(skinName);
        if (skin != null) _customSkin?.addSkin(skin);
      }
    }
    _controller.skeleton.setSkin(_customSkin!);
    _controller.skeleton.setToSetupPose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
        appBar: AppBar(title: const Text('Dress Up')),
        body: _skinImages.isEmpty
            ? const SizedBox()
            : Row(
                  children: [
                    Container(width: thumbnailSize, child:
                      ListView(
                          children: _skinImages.keys.map((skinName) {
                            var rawImageData = _skinImages[skinName]!;
                            var image = Image(image: RawImageProvider(rawImageData));
                            var box = SizedBox(width: 200, height: 200, child: image);
                            return GestureDetector(
                                onTap: () {
                                  _toggleSkin(skinName);
                                  setState(() {});
                                },
                                child: _selectedSkins[skinName] == true
                                      ? box
                                      : ColorFiltered(colorFilter: ColorFilter.mode(Colors.grey, painting.BlendMode.saturation,), child: box)
                            );
                          }).toList()
                      ),
                    ),
                    Expanded(
                      child: SpineWidget.drawable(_drawable, _controller, boundsProvider: SkinAndAnimationBounds(skins: ["full-skins/girl"]),)
                    )
                  ]
              )
    );
  }

  @override
  void dispose() {
    super.dispose();
    _drawable?.dispose();
  }
}