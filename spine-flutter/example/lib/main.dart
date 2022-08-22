import 'package:flutter/material.dart';
import 'package:spine_flutter/spine_flutter.dart' as spine_flutter;
import 'package:flutter/services.dart' show rootBundle;
import 'package:spine_flutter/spine_flutter_bindings_generated.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatefulWidget {
  const MyApp({Key? key}) : super(key: key);

  @override
  _MyAppState createState() => _MyAppState();
}

class SpinePainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    var paint = Paint()
      ..color = Colors.teal
      ..strokeWidth = 5
      ..strokeCap = StrokeCap.round;
    canvas.drawLine(Offset(0, 0), Offset(size.width, size.height), paint);
  }

  @override
  bool shouldRepaint(CustomPainter oldDelegate) {
    return false;
  }
}

class SpineWidget extends StatelessWidget {
  String skeletonFile;
  String atlasFile;
  late spine_flutter.SpineSkeletonDrawable skeletonDrawable;

  SpineWidget(this.skeletonFile, this.atlasFile) {
    loadSkeleton();
  }

  void loadSkeleton() async {
    final atlas = await spine_flutter.SpineAtlas.fromAsset(rootBundle, atlasFile);
    final skeletonData = skeletonFile.endsWith(".json") ?
      spine_flutter.SpineSkeletonData.fromJson(atlas, await rootBundle.loadString(skeletonFile))
    : spine_flutter.SpineSkeletonData.fromBinary(atlas, await rootBundle.load(skeletonFile));
    skeletonDrawable = spine_flutter.SpineSkeletonDrawable(atlas, skeletonData);
    skeletonDrawable.update(0.016);
    print("Loaded skeleton");
  }

  @override
  Widget build(BuildContext context) {
    return CustomPaint(
        painter: SpinePainter(),
        child: Container()
      );
  }
}

class _MyAppState extends State<MyApp> {
  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    const textStyle = TextStyle(fontSize: 25);
    const spacerSmall = SizedBox(height: 10);
    return MaterialApp(
      home: SpineWidget("assets/skeleton.json", "assets/skeleton.atlas")
    );
  }
}
