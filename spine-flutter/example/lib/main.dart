///
/// Spine Runtimes License Agreement
/// Last updated April 5, 2025. Replaces all prior versions.
///
/// Copyright (c) 2013-2025, Esoteric Software LLC
///
/// Integration of the Spine Runtimes into software or otherwise creating
/// derivative works of the Spine Runtimes is permitted under the terms and
/// conditions of Section 2 of the Spine Editor License Agreement:
/// http://esotericsoftware.com/spine-editor-license
///
/// Otherwise, it is permitted to integrate the Spine Runtimes into software
/// or otherwise create derivative works of the Spine Runtimes (collectively,
/// "Products"), provided that each user of the Products must obtain their own
/// Spine Editor license and redistribution of the Products in any form must
/// include this license and copyright notice.
///
/// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
/// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
/// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
/// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
/// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
/// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
/// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
/// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
/// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
/// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
///

import 'package:spine_flutter/spine_flutter.dart';
import 'package:flutter/material.dart';
import 'package:spine_flutter_example/debug_rendering.dart';

import 'animation_state_events.dart';
import 'dress_up.dart';
import 'flame_example.dart';
import 'ik_following.dart';
import 'pause_play_animation.dart';
import 'physics.dart';
import 'simple_animation.dart';

class ExampleSelector extends StatelessWidget {
  const ExampleSelector({super.key});

  @override
  Widget build(BuildContext context) {
    const spacer = SizedBox(height: 10);
    return Scaffold(
        appBar: AppBar(title: const Text('Spine Examples')),
        body: Center(
            child: Column(mainAxisSize: MainAxisSize.min, children: [
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
            child: const Text('Debug Rendering'),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute<void>(
                  builder: (context) => const DebugRendering(),
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
                child: const Text('Physics'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const PhysicsTest(),
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
          spacer,
          ElevatedButton(
            child: const Text('Flame: Dragon Example'),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute<void>(
                  builder: (context) => SpineFlameGameWidget(DragonExample()),
                ),
              );
            },
          ),
          spacer,
        ])));
  }
}

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initSpineFlutter(enableMemoryDebugging: false);
  runApp(const MaterialApp(title: "Spine Examples", home: ExampleSelector()));
}
