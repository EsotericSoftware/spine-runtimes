// ignore_for_file: avoid_print
import 'package:spine_flutter/spine_flutter.dart';
import 'package:flutter/material.dart';

class AnimationStateEvents extends StatelessWidget {
  const AnimationStateEvents({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    reportLeaks();
    final controller = SpineWidgetController(onInitialized: (controller) {
      controller.skeleton.setScaleX(0.5);
      controller.skeleton.setScaleY(0.5);
      controller.skeleton.findSlot("gun")?.setColor(Color(1, 0, 0, 1));
      controller.animationStateData.setDefaultMix(0.2);
      controller.animationState.setAnimationByName(0, "walk", true).setListener((type, trackEntry, event) {
        print("Walk animation event $type");
      });
      controller.animationState.addAnimationByName(0, "jump", false, 2);
      controller.animationState.addAnimationByName(0, "run", true, 0).setListener((type, trackEntry, event) {
        print("Run animation event $type");
      });
      controller.animationState.setListener((type, trackEntry, event) {
        if (type == EventType.event) {
          print(
              "User event: { name: ${event?.getData().getName()}, intValue: ${event?.getIntValue()}, floatValue: ${event?.getFloatValue()}, stringValue: ${event?.getStringValue()} }");
        }
      });
      print("Current: ${controller.animationState.getCurrent(0)?.getAnimation().getName()}");
    });

    return Scaffold(
        appBar: AppBar(title: const Text('Animation State Listener')),
        body: Column(children: [
          const Text("See output in console!"),
          Expanded(child: SpineWidget.fromAsset("assets/spineboy.atlas", "assets/spineboy-pro.skel", controller))
        ]));
  }
}
