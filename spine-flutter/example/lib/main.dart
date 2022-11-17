import 'package:flutter/material.dart';

import 'simple_animation.dart';
import 'animation_state_events.dart';
import 'pause_play_animation.dart';
import 'skins.dart';
import 'dress_up.dart';

class ExampleSelector extends StatelessWidget {
  const ExampleSelector({super.key});

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
              spacer,
              ElevatedButton(
                child: const Text('Dress Up'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const DressUp(),
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

void main() {
  runApp(const MaterialApp(
      title: "Spine Examples",
      home: ExampleSelector()
  ));
}
