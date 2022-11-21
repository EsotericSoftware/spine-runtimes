import 'package:flutter/material.dart';
import 'package:esotericsoftware_spine_flutter/spine_flutter.dart';

class PlayPauseAnimation extends StatefulWidget {
  const PlayPauseAnimation({Key? key}) : super(key: key);

  @override
  PlayPauseAnimationState createState() => PlayPauseAnimationState();
}

class PlayPauseAnimationState extends State<PlayPauseAnimation> {
  late SpineWidgetController controller;
  late bool isPlaying;

  @override
  void initState() {
    super.initState();
    controller = SpineWidgetController(onInitialized: (controller) {
      controller.animationState.setAnimationByName(0, "walk", true);
    });
    isPlaying = true;
  }

  void _togglePlay() {
    isPlaying = !isPlaying;
    controller.animationState.setTimeScale(isPlaying ? 1 : 0);
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    reportLeaks();

    return Scaffold(
      appBar: AppBar(title: const Text('Play/Pause')),
      body: SpineWidget.asset("assets/spineboy-pro.skel", "assets/spineboy.atlas", controller),
      floatingActionButton: FloatingActionButton(
        onPressed: _togglePlay,
        child: Icon(isPlaying ? Icons.pause : Icons.play_arrow),
      ),
    );
  }
}