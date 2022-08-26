import 'dart:convert';
import 'dart:io';

import 'package:flutter/rendering.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter/services.dart';
import 'package:flutter/widgets.dart';
import 'package:http/http.dart' as http;

import 'spine_flutter.dart';

abstract class SpineWidgetController {
}

enum AssetType {
  Asset,
  File,
  Http
}

class SpineWidget extends StatefulWidget {
  final String skeletonFile;
  final String atlasFile;
  final AssetType _assetType;

  const SpineWidget.asset(this.skeletonFile, this.atlasFile, {super.key}): _assetType = AssetType.Asset;
  const SpineWidget.file(this.skeletonFile, this.atlasFile, {super.key}): _assetType = AssetType.File;
  const SpineWidget.http(this.skeletonFile, this.atlasFile, {super.key}): _assetType = AssetType.Http;

  @override
  State<SpineWidget> createState() => _SpineWidgetState();
}

class _SpineWidgetState extends State<SpineWidget> {
  SpineSkeletonDrawable? skeletonDrawable;

  @override
  void initState() {
    super.initState();
    loadSkeleton(widget.skeletonFile, widget.atlasFile, widget._assetType);
  }

  void loadSkeleton(String skeletonFile, String atlasFile, AssetType assetType) async {
    late SpineAtlas atlas;
    late SpineSkeletonData skeletonData;

    switch (assetType) {
      case AssetType.Asset:
        atlas = await SpineAtlas.fromAsset(rootBundle, atlasFile);
        skeletonData = skeletonFile.endsWith(".json")
            ? SpineSkeletonData.fromJson(
            atlas, await rootBundle.loadString(skeletonFile))
            : SpineSkeletonData.fromBinary(
            atlas, (await rootBundle.load(skeletonFile)).buffer.asUint8List());
        break;
      case AssetType.File:
        atlas = await SpineAtlas.fromFile(atlasFile);
        skeletonData = skeletonFile.endsWith(".json")
            ? SpineSkeletonData.fromJson(
            atlas, utf8.decode(await File(skeletonFile).readAsBytes()))
            : SpineSkeletonData.fromBinary(
            atlas, await File(skeletonFile).readAsBytes());
        break;
      case AssetType.Http:
        atlas = await SpineAtlas.fromUrl(atlasFile);
        skeletonData = skeletonFile.endsWith(".json")
            ? SpineSkeletonData.fromJson(
            atlas, utf8.decode((await http.get(Uri.parse(skeletonFile))).bodyBytes))
            : SpineSkeletonData.fromBinary(
            atlas, (await http.get(Uri.parse(skeletonFile))).bodyBytes);
        break;
    }

    skeletonDrawable = SpineSkeletonDrawable(atlas, skeletonData);
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

  @override
  void dispose() {
    skeletonDrawable?.dispose();
    super.dispose();
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
  void updateRenderObject(BuildContext context,
      covariant _SpineRenderObject renderObject) {
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
    _skeletonDrawable.update(_deltaTime);
    markNeedsPaint();
  }

  @override
  void paint(PaintingContext context, Offset offset) {
    final Canvas canvas = context.canvas
      ..save()
      ..clipRect(offset & size);

    final commands = _skeletonDrawable.render();
    canvas.save();
    canvas.translate(offset.dx + size.width / 2, offset.dy + size.height);
    for (final cmd in commands) {
      canvas.drawVertices(cmd.vertices, BlendMode.modulate,
          _skeletonDrawable.atlas.atlasPagePaints[cmd.atlasPageIndex]);
    }

    canvas.restore();
    SchedulerBinding.instance.scheduleFrameCallback(_beginFrame);
  }
}
