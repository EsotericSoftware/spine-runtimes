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

class PhysicsTest extends StatefulWidget {
  const PhysicsTest({Key? key}) : super(key: key);

  @override
  PhysicsState createState() => PhysicsState();
}

class PhysicsState extends State<PhysicsTest> {
  late SpineWidgetController controller;
  Offset? mousePosition;
  Offset? lastMousePosition;

  @override
  void initState() {
    super.initState();

    controller = SpineWidgetController(onInitialized: (controller) {
      controller.animationState.setAnimationByName(0, "eyeblink-long", true);
      controller.animationState.setAnimationByName(1, "wings-and-feet", true);
    }, onAfterUpdateWorldTransforms: (controller) {
      if (lastMousePosition == null) {
        lastMousePosition = mousePosition;
        return;
      }
      if (mousePosition == null) {
        return;
      }

      final dx = mousePosition!.dx - lastMousePosition!.dx;
      final dy = mousePosition!.dy - lastMousePosition!.dy;
      final position = controller.skeleton.getPosition();
      position.x += dx;
      position.y += dy;
      controller.skeleton.setPosition(position.x, position.y);
      lastMousePosition = mousePosition;
    });
  }

  void _updateBonePosition(Offset position) {
    mousePosition = controller.toSkeletonCoordinates(position);
  }

  @override
  Widget build(BuildContext context) {
    reportLeaks();

    return Scaffold(
        appBar: AppBar(title: const Text('Physics (drag anywhere)')),
        body: GestureDetector(
          onPanDown: (drag) => _updateBonePosition(drag.localPosition),
          onPanUpdate: (drag) => _updateBonePosition(drag.localPosition),
          child: SpineWidget.fromAsset("assets/celestial-circus.atlas", "assets/celestial-circus-pro.skel", controller),
        ));
  }
}
