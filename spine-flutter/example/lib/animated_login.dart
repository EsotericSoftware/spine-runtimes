import 'package:spine_flutter/spine_flutter.dart';
import 'package:flutter/material.dart';

class AnimatedLogin extends StatelessWidget {
  const AnimatedLogin({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    reportLeaks();
    final controller = SpineWidgetController(onInitialized: (controller) {
      controller.skeleton.setSkinByName("nate");
      controller.skeleton.setToSetupPose();
      controller.animationState.setAnimationByName(0, "login/look-left-down", true);
    });

    return Scaffold(
        appBar: AppBar(title: const Text('Animated login')),
        body: Container(
            margin: const EdgeInsets.all(15.0),
            padding: const EdgeInsets.all(3.0),
            decoration: BoxDecoration(border: Border.all(color: Colors.blueAccent)),
            child: SpineWidget.fromAsset(
              "assets/chibi/chibi-stickers.atlas",
              "assets/chibi/chibi-stickers.skel",
              controller,
              boundsProvider: SkinAndAnimationBounds(skins: ["nate"], animation: "login/look-left-down"),
              sizedByBounds: true,
            )));
  }
}
