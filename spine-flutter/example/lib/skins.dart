import 'package:flutter/material.dart';
import 'package:esotericsoftware_spine_flutter/spine_flutter.dart';

class Skins extends StatefulWidget {
  const Skins({Key? key}) : super(key: key);

  @override
  SkinsState createState() => SkinsState();
}

class SkinsState extends State<Skins> {
  SkeletonDrawable? _drawable;
  late SpineWidgetController _controller;
  final Map<String, bool> _selectedSkins = {};
  Skin? _customSkin;

  @override
  void initState() {
    super.initState();
    SkeletonDrawable.fromAsset("assets/mix-and-match.atlas", "assets/mix-and-match-pro.skel").then((drawable) {
      for (var skin in drawable.skeletonData.getSkins()) {
        _selectedSkins[skin.getName()] = false;
      }
      _controller = SpineWidgetController(onInitialized: (controller) {
        controller.animationState.setAnimationByName(0, "walk", true);
      });
      drawable.skeleton.setSkinByName("full-skins/girl");
      _selectedSkins["full-skins/girl"] = true;
      _drawable = drawable;
      setState(() {});
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
        appBar: AppBar(title: const Text('Skins')),
        body: _drawable == null
            ? const SizedBox()
            : Row(
            children: [
              Expanded(
                  child:ListView(
                      children: _selectedSkins.keys.map((skinName) {
                        return CheckboxListTile(
                          title: Text(skinName),
                          value: _selectedSkins[skinName],
                          onChanged: (bool? value) {
                            _toggleSkin(skinName);
                            setState(() => {});
                          },
                        );
                      }).toList()
                  )
              ),
              Expanded(
                  child: SpineWidget.drawable(_drawable, _controller, boundsProvider: SkinAndAnimationBounds(["full-skins/girl"]))
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