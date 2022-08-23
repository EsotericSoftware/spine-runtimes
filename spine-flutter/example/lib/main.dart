import 'package:flutter/material.dart';
import 'package:spine_flutter/spine_flutter.dart' as spine_flutter;
import 'package:flutter/services.dart' show rootBundle;
import 'package:spine_flutter/spine_flutter_bindings_generated.dart';

void main() {
  runApp(const MyApp());
}

class SpineWidget extends StatefulWidget {
  final String skeletonFile;
  final String atlasFile;

  const SpineWidget(this.skeletonFile, this.atlasFile, {super.key});

  @override
  State<SpineWidget> createState() => _SpineWidgetState();
}

class _SpineWidgetState extends State<SpineWidget> {
  spine_flutter.SpineSkeletonDrawable? skeletonDrawable;

  @override
  void initState() {
    super.initState();
    loadSkeleton(widget.skeletonFile, widget.atlasFile);
  }

  void loadSkeleton(String skeletonFile, String atlasFile) async {
    final atlas = await spine_flutter.SpineAtlas.fromAsset(rootBundle, atlasFile);
    final skeletonData = skeletonFile.endsWith(".json") ?
    spine_flutter.SpineSkeletonData.fromJson(atlas, await rootBundle.loadString(skeletonFile))
        : spine_flutter.SpineSkeletonData.fromBinary(atlas, await rootBundle.load(skeletonFile));
    skeletonDrawable = spine_flutter.SpineSkeletonDrawable(atlas, skeletonData);
    skeletonDrawable?.update(0.016);
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    if (skeletonDrawable != null) {
      print("Skeleton loaded, rebuilding painter");
      return CustomPaint(
          painter: _SpinePainter(this),
          child: Container()
      );
    } else {
      print("Skeleton not loaded yet");
      return Container();
    }
  }
}

class _SpinePainter extends CustomPainter {
  final _SpineWidgetState state;

  _SpinePainter(this.state);

  @override
  void paint(Canvas canvas, Size size) {
    print("painting");
    final drawable = state.skeletonDrawable;
    if (drawable == null) return;
    final commands = drawable.render();
    canvas.save();
    canvas.translate(size.width / 2, size.height);
    for (final cmd in commands) {
      canvas.drawVertices(cmd.vertices, BlendMode.srcOut, drawable.atlas.atlasPagePaints[cmd.atlasPageIndex]);
    }
    canvas.restore();
    canvas.drawLine(Offset(0, 0), Offset(size.width, size.height), Paint()
      ..color = Colors.blue
      ..strokeWidth = 4);
  }

  @override
  bool shouldRepaint(CustomPainter oldDelegate) {
    return false;
  }
}

class MyApp extends StatefulWidget {
  const MyApp({Key? key}) : super(key: key);

  @override
  _MyAppState createState() => _MyAppState();
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
    return const MaterialApp(
      home: SpineWidget("assets/spineboy-pro.json", "assets/spineboy.atlas")
    );
  }
}
