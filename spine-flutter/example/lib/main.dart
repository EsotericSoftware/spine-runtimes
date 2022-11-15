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
                child: const Text('Pause/Play animation'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const PlayPauseAnimation(),
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
    final controller = SpineWidgetController((controller) {
      // Set the walk animation on track 0, let it loop
      controller.animationState?.setAnimationByName(0, "walk", true);
    });

    return Scaffold(
      appBar: AppBar(title: const Text('Spineboy')),
      body: SpineWidget.asset("assets/spineboy-pro.skel", "assets/spineboy.atlas", controller),
      // body: SpineWidget.file("/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy-pro.skel", "/Users/badlogic/workspaces/spine-runtimes/examples/spineboy/export/spineboy.atlas", controller),
      // body: const SpineWidget.http("https://marioslab.io/dump/spineboy/spineboy-pro.json", "https://marioslab.io/dump/spineboy/spineboy.atlas"),
      // body: SpineWidget.asset("assets/skeleton.json", "assets/skeleton.atlas", controller, alignment: Alignment.topLeft, fit: BoxFit.cover),
    );
  }
}

class PlayPauseAnimation extends StatefulWidget {
  const PlayPauseAnimation({Key? key}) : super(key: key);

  @override
  PlayPauseAnimationState createState() => PlayPauseAnimationState();
}

class PlayPauseAnimationState extends State<PlayPauseAnimation> {
  late SpineWidgetController _controller;

  @override
  void initState() {
    super.initState();
    _controller = SpineWidgetController((controller) {
      controller.animationState?.setAnimationByName(0, "walk", true);
    });
  }

  void _togglePlaystate() {
    _controller.togglePlay();
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    reportLeaks();

    return Scaffold(
      appBar: AppBar(title: const Text('Spineboy')),
      body: SpineWidget.asset("assets/spineboy-pro.skel", "assets/spineboy.atlas", _controller),
      floatingActionButton: FloatingActionButton(
        onPressed: _togglePlaystate,
        child: Icon(_controller.isPlaying ? Icons.pause : Icons.play_arrow),
      ),
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
      controller.skeleton?.findSlot("gun")?.setColor(Color(1, 0, 0, 1));
      controller.animationStateData?.setDefaultMix(0.2);
      controller.animationState?.setAnimationByName(0, "walk", true)?.setListener((type, trackEntry, event) {
        print("Walk animation event ${type}");
      });
      controller.animationState?.addAnimationByName(0, "jump", false, 2);
      controller.animationState?.addAnimationByName(0, "run", true, 0)?.setListener((type, trackEntry, event) {
        print("Run animation event ${type}");
      });
      controller.animationState?.setListener((type, trackEntry, event) {
        if (type == EventType.Event) {
          print("User event: { name: ${event?.getData().getName()}, intValue: ${event?.getIntValue()}, floatValue: intValue: ${event?.getFloatValue()}, stringValue: ${event?.getStringValue()} }");
        }
      });
      print("Current: ${controller.animationState?.getCurrent(0)?.getAnimation().getName()}");
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
