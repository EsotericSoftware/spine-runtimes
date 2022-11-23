import 'package:flutter/material.dart';
import 'package:esotericsoftware_spine_flutter/spine_flutter.dart';

import 'flame_example.dart';
import 'simple_animation.dart';
import 'animation_state_events.dart';
import 'pause_play_animation.dart';
import 'skins.dart';
import 'dress_up.dart';
import 'ik_following.dart';

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
              spacer,
              ElevatedButton(
                child: const Text('IK Following'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const IkFollowing(),
                    ),
                  );
                },
              ),
              spacer,
              ElevatedButton(
                child: const Text('Flame: Simple Example'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => SpineFlameGameWidget(SimpleFlameExample()),
                    ),
                  );
                },
              ),
              spacer,
              ElevatedButton(
                child: const Text('Flame: Pre-load and share Spine data'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => SpineFlameGameWidget(PreloadAndShareSpineDataExample()),
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

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initSpineFlutter(enableMemoryDebugging: false);
  runApp(const MaterialApp(
      title: "Spine Examples",
      home: ExampleSelector()
  ));
}
