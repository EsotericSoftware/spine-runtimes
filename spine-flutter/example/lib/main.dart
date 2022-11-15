import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
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
              spacer,
              ElevatedButton(
                child: const Text('Skins'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const Skins(),
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
      appBar: AppBar(title: const Text('Simple Animation')),
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
      appBar: AppBar(title: const Text('Play/Pause')),
      body: SpineWidget.asset("assets/spineboy-pro.skel", "assets/spineboy.atlas", _controller),
      floatingActionButton: FloatingActionButton(
        onPressed: _togglePlaystate,
        child: Icon(_controller.isPlaying ? Icons.pause : Icons.play_arrow),
      ),
    );
  }
}

class Skins extends StatefulWidget {
  const Skins({Key? key}) : super(key: key);

  @override
  SkinsState createState() => SkinsState();
}

class SkinsState extends State<Skins> {
  Atlas? _atlas;
  SkeletonData? _skeletonData;
  final Map<String, bool> _availableSkins = {};
  Skin? _customSkin;
  late SpineWidgetController _controller;

  @override
  void initState() {
    super.initState();

    Atlas.fromAsset(rootBundle, "assets/mix-and-match.atlas").then((atlas) async {
      _skeletonData = await SkeletonData.fromAsset(atlas, rootBundle, "assets/mix-and-match-pro.skel");
      _atlas = atlas;
      for (var skin in _skeletonData?.getSkins() ?? []) {
        _availableSkins[skin.getName()] = false;
      }

      _controller = SpineWidgetController((controller) {
        controller.animationState?.setAnimationByName(0, "walk", true);
      });

      setState(() => _toggleSkin("full-skins/girl"));
    });
  }

  void _toggleSkin(String skinName) {
    _availableSkins[skinName] = !_availableSkins[skinName]!;

    if (_customSkin != null) {
      _customSkin?.dispose();
      _customSkin = null;
    }

    _customSkin = Skin("custom-skin");
    for (var skinName in _availableSkins.keys) {
      if (_availableSkins[skinName] == true) {
        var skin = _controller.skeletonData?.findSkin(skinName);
        if (skin != null)
          _customSkin?.addSkin(skin);
      }
    }
    _controller.skeleton?.setSkin(_customSkin!);
    _controller.skeleton?.setToSetupPose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
        appBar: AppBar(title: const Text('Skins')),
        body: _skeletonData == null
            ? SizedBox()
            : Row(
            children: [
              Expanded(
                  child:ListView(
                    children: _availableSkins.keys.map((skinName) {
                      return CheckboxListTile(
                        title: Text(skinName),
                        value: _availableSkins[skinName],
                        onChanged: (bool? value) {
                          _toggleSkin(skinName);
                          setState(() => {});
                        },
                      );
                    }).toList()
                  )
              ),
              Expanded(
                  child: SpineWidget.raw(_skeletonData, _atlas, _controller, boundsProvider: SkinAndAnimationBounds(["full-skins/girl"]))
              )
            ]
        )
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
