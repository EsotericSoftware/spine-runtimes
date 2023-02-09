import 'package:spine_flutter/spine_flutter.dart';
import 'package:flutter/material.dart';

class IkFollowing extends StatefulWidget {
  const IkFollowing({Key? key}) : super(key: key);

  @override
  IkFollowingState createState() => IkFollowingState();
}

class IkFollowingState extends State<IkFollowing> {
  late SpineWidgetController controller;
  Offset? crossHairPosition;

  @override
  void initState() {
    super.initState();

    controller = SpineWidgetController(onInitialized: (controller) {
      // Set the walk animation on track 0, let it loop
      controller.animationState.setAnimationByName(0, "walk", true);
      controller.animationState.setAnimationByName(1, "aim", true);
    }, onAfterUpdateWorldTransforms: (controller) {
      var worldPosition = crossHairPosition;
      if (worldPosition == null) return;
      var bone = controller.skeleton.findBone("crosshair");
      if (bone == null) return;
      var parent = bone.getParent();
      if (parent == null) return;
      var position = parent.worldToLocal(worldPosition.dx, worldPosition.dy);
      bone.setX(position.x);
      bone.setY(position.y);
    });
  }

  void _updateBonePosition(Offset position) {
    crossHairPosition = controller.toSkeletonCoordinates(position);
  }

  @override
  Widget build(BuildContext context) {
    reportLeaks();

    return Scaffold(
        appBar: AppBar(title: const Text('IK Following')),
        body: GestureDetector(
          onPanDown: (drag) => _updateBonePosition(drag.localPosition),
          onPanUpdate: (drag) => _updateBonePosition(drag.localPosition),
          child: SpineWidget.fromAsset("assets/spineboy.atlas", "assets/spineboy-pro.skel", controller),
        ));
  }
}
