import 'dart:math';
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:spine_flutter/spine_flutter.dart';
import 'package:flutter/rendering.dart' as rendering;

class DressUp extends StatefulWidget {
  const DressUp({Key? key}) : super(key: key);

  @override
  DressUpState createState() => DressUpState();
}

class DressUpState extends State<DressUp> {
  static const double thumbnailSize = 200;
  SkeletonDrawable? _drawable;
  final List<Image> _skinImages = [];

  @override
  void initState() {
    super.initState();
    SkeletonDrawable.fromAsset("assets/mix-and-match-pro.skel", "assets/mix-and-match.atlas").then((drawable) async {
      for (var skin in drawable.skeletonData.getSkins()) {
        if (skin.getName() == "default") continue;

        var skeleton = drawable.skeleton;
        skeleton.setSkin(skin);
        skeleton.setToSetupPose();
        skeleton.updateWorldTransform();
        var bounds = skeleton.getBounds();
        var scale = 1 / (bounds.width > bounds.height ? bounds.width / thumbnailSize : bounds.height / thumbnailSize);
        scale *= 0.9;

        var recorder = ui.PictureRecorder();
        var canvas = Canvas(recorder);
        var bgColor = Random().nextInt(0xffffffff) | 0xff0000000;
        var paint = Paint()
          ..color = ui.Color(bgColor)
          ..style = PaintingStyle.fill;
        canvas.drawRect(const Rect.fromLTWH(0, 0, thumbnailSize, thumbnailSize), paint);
        canvas.scale(scale, scale);
        canvas.translate(-bounds.x, -bounds.y);
        canvas.drawRect(Rect.fromLTRB(-5, -5, 5, -5), paint..color = Colors.red);
        drawable.renderToCanvas(canvas);

        var imageData = await (await recorder.endRecording().toImage(thumbnailSize.toInt(), thumbnailSize.toInt())).toByteData(format: ui.ImageByteFormat.png);
        _skinImages.add(Image.memory(imageData!.buffer.asUint8List(), fit: BoxFit.contain));
      }
      _drawable = drawable;
      setState(() {});
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
        appBar: AppBar(title: const Text('Dress Up')),
        body: _skinImages.isEmpty
            ? const SizedBox()
            : Row(
            children: [
              Expanded(
                  child:ListView(
                      children: _skinImages.map((image) {
                        return SizedBox(width: 200, height: 200, child: image);
                      }).toList()
                  )
              ),
            ]
        )
    );
  }
}