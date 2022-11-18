import 'package:flutter/material.dart';
import 'package:spine_flutter/spine_flutter.dart';

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
      body: SpineWidget.asset("assets/spineboy-pro.skel", "assets/spineboy.atlas", controller),
      // body: SpineWidget.file("/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy-pro.skel", "/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy.atlas", controller),
      // body: const SpineWidget.http("https://marioslab.io/dump/spineboy/spineboy-pro.json", "https://marioslab.io/dump/spineboy/spineboy.atlas"),
      // body: SpineWidget.asset("assets/skeleton.json", "assets/skeleton.atlas", controller, alignment: Alignment.topLeft, fit: BoxFit.cover),
    );
  }
}