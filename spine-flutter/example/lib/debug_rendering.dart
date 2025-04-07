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

class DebugRendering extends StatelessWidget {
  const DebugRendering({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    reportLeaks();

    const debugRenderer = DebugRenderer();
    final controller = SpineWidgetController(onInitialized: (controller) {
      controller.animationState.setAnimationByName(0, "walk", true);
}, onBeforePaint: (controller, canvas) {
  // Save the current transform and other canvas state
  canvas.save();

  // Get the current canvas transform an invert it, so we can work in the
  // canvas coordinate system.
  final currentMatrix = canvas.getTransform();
  final invertedMatrix = Matrix4.tryInvert(Matrix4.fromFloat64List(currentMatrix));
  if (invertedMatrix != null) {
    canvas.transform(invertedMatrix.storage);
  }

  // Draw something.
  final Paint paint = Paint()
    ..color = Colors.black
    ..strokeWidth = 2.0;

  canvas.drawLine(
    Offset(0, 0),
    Offset(canvas.getLocalClipBounds().width, canvas.getLocalClipBounds().height),
    paint,
  );

  // Restore the old transform and canvas state
  canvas.restore();
}, onAfterPaint: (controller, canvas, commands) {
      debugRenderer.render(controller.drawable, canvas, commands);
    });

    return Scaffold(
      appBar: AppBar(title: const Text('Debug Renderer')),
      body: SpineWidget.fromAsset("assets/spineboy.atlas", "assets/spineboy-pro.skel", controller),
    );
  }
}
