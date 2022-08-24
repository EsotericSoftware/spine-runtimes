import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter/scheduler.dart';
import 'package:spine_flutter/spine_flutter.dart' as spine_flutter;
import 'package:flutter/services.dart' show rootBundle;
import 'package:spine_flutter/spine_flutter.dart';

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
  SpineSkeletonDrawable? skeletonDrawable;

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
      return _SpineRenderObjectWidget(skeletonDrawable!);
    } else {
      print("Skeleton not loaded yet");
      return SizedBox();
    }
  }
}

class _SpineRenderObjectWidget extends LeafRenderObjectWidget {
  final SpineSkeletonDrawable skeletonDrawable;
  _SpineRenderObjectWidget(this.skeletonDrawable);

  @override
  RenderObject createRenderObject(BuildContext context) {
    return _SpineRenderObject(skeletonDrawable);
  }

  @override
  void updateRenderObject(BuildContext context, covariant _SpineRenderObject renderObject) {
    renderObject.skeletonDrawable = skeletonDrawable;
  }
}

class _SpineRenderObject extends RenderBox {
  SpineSkeletonDrawable _skeletonDrawable;
  double _deltaTime = 0;
  final Stopwatch _stopwatch = Stopwatch();

  _SpineRenderObject(this._skeletonDrawable);

  set skeletonDrawable(SpineSkeletonDrawable skeletonDrawable) {
    if (_skeletonDrawable == skeletonDrawable) return;

    // FIXME dispose old drawable here?
    _skeletonDrawable = skeletonDrawable;
    markNeedsPaint();
  }

  @override
  bool get sizedByParent => true;

  @override
  bool get isRepaintBoundary => true;

  @override
  bool hitTestSelf(Offset position) => true;

  @override
  void performResize() {
    size = constraints.biggest;
  }

  @override
  void attach(PipelineOwner owner) {
    super.attach(owner);
    _stopwatch.start();
  }

  @override
  void detach() {
    _stopwatch.stop();
    super.detach();
  }

  void _beginFrame(Duration duration) {
    _deltaTime = _stopwatch.elapsedTicks / _stopwatch.frequency;
    _stopwatch.reset();
    _stopwatch.start();
    markNeedsPaint();
  }

  @override
  void paint(PaintingContext context, Offset offset) {
    print("painting");
    final Canvas canvas = context.canvas
      ..save()
      ..clipRect(offset & size);

    final commands = _skeletonDrawable.render();
    canvas.save();
    canvas.translate(offset.dx, offset.dy);
    for (final cmd in commands) {
      canvas.drawVertices(cmd.vertices, BlendMode.modulate, _skeletonDrawable.atlas.atlasPagePaints[cmd.atlasPageIndex]);
    }

    canvas.restore();
    SchedulerBinding.instance.scheduleFrameCallback(_beginFrame);
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
    return const MaterialApp(
      home: SpineWidget("assets/spineboy-pro.json", "assets/spineboy.atlas")
    );
  }
}
