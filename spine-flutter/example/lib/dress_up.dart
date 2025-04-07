///
/// Spine Runtimes License Agreement
/// Last updated April 5, 2025. Replaces all prior versions.
///
/// Copyright (c) 2013-2025, Esoteric Software LLC
///
/// Integration of the Spine Runtimes into software or otherwise creating
/// derivative works of the Spine Runtimes is permitted under the terms and
/// conditions of Section 2 of the Spine Editor License Agreement:
/// http://esotericsoftware.com/spine-editor-license
///
/// Otherwise, it is permitted to integrate the Spine Runtimes into software
/// or otherwise create derivative works of the Spine Runtimes (collectively,
/// "Products"), provided that each user of the Products must obtain their own
/// Spine Editor license and redistribution of the Products in any form must
/// include this license and copyright notice.
///
/// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
/// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
/// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
/// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
/// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
/// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
/// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
/// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
/// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
/// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
///

import 'package:spine_flutter/raw_image_provider.dart';
import 'package:spine_flutter/spine_flutter.dart';
import 'package:flutter/material.dart';
import 'package:flutter/painting.dart' as painting;

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
        skeleton.update(0);
        skeleton.updateWorldTransform(Physics.update);
        _skinImages[skin.getName()] = await drawable.renderToRawImageData(thumbnailSize, thumbnailSize, 0xffffffff);
        _selectedSkins[skin.getName()] = false;
      }
      _toggleSkin("full-skins/girl");
      setState(() {});
    });
  }

  void _toggleSkin(String skinName) {
    _selectedSkins[skinName] = !_selectedSkins[skinName]!;
    _drawable.skeleton.setSkinByName("default");
    if (_customSkin != null) _customSkin?.dispose();
    _customSkin = Skin("custom-skin");
    for (var skinName in _selectedSkins.keys) {
      if (_selectedSkins[skinName] == true) {
        var skin = _drawable.skeletonData.findSkin(skinName);
        if (skin != null) _customSkin?.addSkin(skin);
      }
    }
    _drawable.skeleton.setSkin(_customSkin!);
    _drawable.skeleton.setSlotsToSetupPose();
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
            : Row(children: [
                SizedBox(
                  width: thumbnailSize,
                  child: ListView(
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
                            // Does not work on web.
                            //: ColorFiltered(colorFilter: const ColorFilter.mode(Colors.grey, painting.BlendMode.saturation,), child: box)
                            : Container(
                                foregroundDecoration: const BoxDecoration(
                                  color: Colors.grey,
                                  backgroundBlendMode: painting.BlendMode.saturation,
                                ),
                                child: box));
                  }).toList()),
                ),
                Expanded(
                    child: SpineWidget.fromDrawable(
                  _drawable,
                  controller,
                  boundsProvider: SkinAndAnimationBounds(skins: ["full-skins/girl"]),
                ))
              ]));
  }

  @override
  void dispose() {
    super.dispose();
    _drawable.dispose();
    _customSkin?.dispose();
  }
}
