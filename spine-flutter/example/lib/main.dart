import 'package:flutter/material.dart';
import 'package:spine_flutter/spine_flutter.dart';

class ExampleSelector extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    const spacer = SizedBox(height: 10);
    return Scaffold(
        appBar: AppBar(title: const Text('Spine Examples')),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              ElevatedButton(
                child: const Text('Simple Animation'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const SimpleAnimation(),
                    ),
                  );
                },
              ),
              spacer,
              ElevatedButton(
                child: const Text('Animation State Listener'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const AnimationStateEvents(),
                    ),
                  );
                },
              ),
              spacer
            ]
          )
        )
    );
  }
}

class SimpleAnimation extends StatelessWidget {
  const SimpleAnimation({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    reportLeaks();
    final controller = SpineWidgetController((controller) => controller.animationState?.setAnimation(0, "walk", true));

    return Scaffold(
      appBar: AppBar(title: const Text('Spineboy')),
      body: SpineWidget.asset("assets/spineboy-pro.skel", "assets/spineboy.atlas", controller),
      // body: const SpineWidget.file("/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy-pro.skel", "/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy.atlas"),
      // body: const SpineWidget.http("https://marioslab.io/dump/spineboy/spineboy-pro.json", "https://marioslab.io/dump/spineboy/spineboy.atlas"),
    );
  }
}

class AnimationStateEvents extends StatelessWidget {
  const AnimationStateEvents({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    reportLeaks();
    final controller = SpineWidgetController((controller) {
      for (final bone in controller.skeleton!.getBones()) {
        print(bone);
      }
      controller.skeleton?.setScaleX(0.5);
      controller.skeleton?.setScaleY(0.5);
      controller.skeleton?.setColor(Color(1, 1, 0, 1));
      controller.animationState?.setAnimation(0, "walk", true)?.setListener((event) {
        print("Walk animation event ${event.type}");
      });
      controller.animationState?.addAnimation(0, "run", true, 2)?.setListener((event) {
        print("Run animation event ${event.type}");
      });
    });

    return Scaffold(
      appBar: AppBar(title: const Text('Spineboy')),
      body: SpineWidget.asset("assets/spineboy-pro.skel", "assets/spineboy.atlas", controller),
    );
  }
}

void main() {
  runApp(MaterialApp(
      title: "Spine Examples",
      home: ExampleSelector()
  ));
}
