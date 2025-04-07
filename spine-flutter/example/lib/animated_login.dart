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

class AnimatedLogin extends StatelessWidget {
  const AnimatedLogin({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    reportLeaks();
    final controller = SpineWidgetController(onInitialized: (controller) {
      controller.skeleton.setSkinByName("nate");
      controller.skeleton.setToSetupPose();
      controller.animationState.setAnimationByName(0, "login/look-left-down", true);
    });

    return Scaffold(
        appBar: AppBar(title: const Text('Animated login')),
        body: Container(
            margin: const EdgeInsets.all(15.0),
            padding: const EdgeInsets.all(3.0),
            decoration: BoxDecoration(border: Border.all(color: Colors.blueAccent)),
            child: SpineWidget.fromAsset(
              "assets/chibi/chibi-stickers.atlas",
              "assets/chibi/chibi-stickers.skel",
              controller,
              boundsProvider: SkinAndAnimationBounds(skins: ["nate"], animation: "login/look-left-down"),
              sizedByBounds: true,
            )));
  }
}
