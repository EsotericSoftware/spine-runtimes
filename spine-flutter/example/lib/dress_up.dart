import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:spine_flutter/spine_flutter.dart';

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
        var recorder = ui.PictureRecorder();
        var canvas = Canvas(recorder, const Rect.fromLTWH(0, 0, thumbnailSize, thumbnailSize));
        var paint = Paint()
          ..color = ui.Color(0xff995588)
          ..style = PaintingStyle.fill;
        canvas.drawRect(Rect.fromLTWH(0, 0, 200, 200), paint);
        var imageData = await (await recorder.endRecording().toImage(thumbnailSize.toInt(), thumbnailSize.toInt())).toByteData(format: ui.ImageByteFormat.png);
        _skinImages.add(Image.memory(imageData!.buffer.asUint8List(), fit: BoxFit.none));
      }
      _drawable = drawable;
      setState(() {});
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
        appBar: AppBar(title: const Text('Skins')),
        body: _skinImages.isEmpty
            ? const SizedBox()
            : Row(
            children: [
              Expanded(
                  child:ListView(
                      children: _skinImages
                  )
              ),
            ]
        )
    );
  }
}