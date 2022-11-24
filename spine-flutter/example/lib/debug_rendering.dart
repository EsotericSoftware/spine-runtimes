import 'package:flutter/material.dart';
import 'package:esotericsoftware_spine_flutter/spine_flutter.dart';

class DebugRendering extends StatelessWidget {
  const DebugRendering({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    reportLeaks();

    const debugRenderer = DebugRenderer();
    final controller = SpineWidgetController(
        onInitialized: (controller) {
          controller.animationState.setAnimationByName(0, "walk", true);
        },
        onAfterPaint: (controller, canvas, commands) {
          debugRenderer.render(controller.drawable, canvas, commands);
        }
    );

    return Scaffold(
      appBar: AppBar(title: const Text('Debug Renderer')),
      body: SpineWidget.asset("assets/spineboy.atlas", "assets/spineboy-pro.skel", controller),
    );
  }
}