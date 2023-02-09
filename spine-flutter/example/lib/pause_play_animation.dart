import 'package:spine_flutter/spine_flutter.dart';
import 'package:flutter/material.dart';

class PlayPauseAnimation extends StatefulWidget {
  const PlayPauseAnimation({Key? key}) : super(key: key);

  @override
  PlayPauseAnimationState createState() => PlayPauseAnimationState();
}

class PlayPauseAnimationState extends State<PlayPauseAnimation> {
  late SpineWidgetController controller;

  @override
  void initState() {
    super.initState();
    controller = SpineWidgetController(onInitialized: (controller) {
      controller.animationState.setAnimationByName(0, "flying", true);
    });
  }

  void _togglePlay() {
    if (controller.isPlaying) {
      controller.pause();
    } else {
      controller.resume();
    }
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    reportLeaks();

    return Scaffold(
      appBar: AppBar(title: const Text('Play/Pause')),
      body: SpineWidget.fromAsset(
        "assets/dragon.atlas",
        "assets/dragon-ess.skel",
        controller,
        boundsProvider: SkinAndAnimationBounds(animation: "flying"),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _togglePlay,
        child: Icon(controller.isPlaying ? Icons.pause : Icons.play_arrow),
      ),
    );
  }
}
