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
                child: const Text('Spineboy'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const Spineboy(),
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

class Spineboy extends StatelessWidget {
  const Spineboy({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
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
    final controller = SpineWidgetController((controller) {
      final trackEntry = controller.animationState?.setAnimation(0, "walk", true);
      controller.skeleton.setColor(1, 0, 0, 1);
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
