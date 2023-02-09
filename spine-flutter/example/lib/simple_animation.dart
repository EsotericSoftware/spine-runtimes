import 'package:spine_flutter/spine_flutter.dart';
import 'package:flutter/material.dart';

class SimpleAnimation extends StatelessWidget {
  const SimpleAnimation({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    reportLeaks();
    final controller = SpineWidgetController(onInitialized: (controller) {
      // Set the walk animation on track 0, let it loop
      controller.animationState.setAnimationByName(0, "walk", true);
    });

    return Scaffold(
      appBar: AppBar(title: const Text('Simple Animation')),
      body: SpineWidget.fromAsset("assets/spineboy.atlas", "assets/spineboy-pro.skel", controller),
      // body: SpineWidget.file( "/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy.atlas", "/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy-pro.skel", controller),
      // body: const SpineWidget.http("https://marioslab.io/dump/spineboy/spineboy.atlas", "https://marioslab.io/dump/spineboy/spineboy-pro.json"),
    );
  }
}
