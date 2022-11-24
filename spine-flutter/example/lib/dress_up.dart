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
  late SkeletonDrawable _drawable;
  Skin? _customSkin;
  final Map<String, RawImageData> _skinImages = {};
  final Map<String, bool> _selectedSkins = {};

  @override
  void initState() {
    reportLeaks();
    super.initState();
    SkeletonDrawable.fromAsset("assets/mix-and-match.atlas", "assets/mix-and-match-pro.skel").then((drawable) async {
      _drawable = drawable;
      for (var skin in drawable.skeletonData.getSkins()) {
        if (skin.getName() == "default") continue;
        var skeleton = drawable.skeleton;
        skeleton.setSkin(skin);
        skeleton.setToSetupPose();
        skeleton.updateWorldTransform();
        _skinImages[skin.getName()] = await drawable.renderToRawImageData(thumbnailSize, thumbnailSize);
        _selectedSkins[skin.getName()] = false;
      }
      _toggleSkin("full-skins/girl");
      setState(() {});
    });
  }

  void _toggleSkin(String skinName) {
    _selectedSkins[skinName] = !_selectedSkins[skinName]!;
    if (_customSkin != null) _customSkin?.dispose();
    _customSkin = Skin("custom-skin");
    for (var skinName in _selectedSkins.keys) {
      if (_selectedSkins[skinName] == true) {
        var skin = _drawable.skeletonData.findSkin(skinName);
        if (skin != null) _customSkin?.addSkin(skin);
      }
    }
    _drawable.skeleton.setSkin(_customSkin!);
    _drawable.skeleton.setToSetupPose();
  }

  @override
  Widget build(BuildContext context) {
    final controller = SpineWidgetController(onInitialized: (controller) {
      controller.animationState.setAnimationByName(0, "dance", true);
    });

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
                      child: SpineWidget.drawable(_drawable, controller, boundsProvider: SkinAndAnimationBounds(skins: ["full-skins/girl"]),)
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