import 'dart:convert';
import 'dart:io';

import 'package:flutter/rendering.dart' as rendering;
import 'package:flutter/scheduler.dart';
import 'package:flutter/services.dart';
import 'package:flutter/widgets.dart';
import 'package:http/http.dart' as http;

import 'spine_flutter.dart';

class SpineWidgetController {
  Atlas? _atlas;
  SkeletonData? _data;
  SkeletonDrawable? _drawable;
  final void Function(SpineWidgetController controller)? onInitialized;
  bool initialized = false;

  SpineWidgetController([this.onInitialized]);

  void _initialize(Atlas atlas, SkeletonData data, SkeletonDrawable drawable) {
    if (initialized)
      throw Exception("SpineWidgetController already initialized. A controller can only be used with one widget.");
    _atlas = atlas;
    _data = data;
    _drawable = drawable;
    onInitialized?.call(this);
    initialized = true;
  }

  Atlas? get atlas => _atlas;

  SkeletonData? get skeletonData => _data;

  AnimationStateData? get animationStateData => _drawable?.animationStateData;

  AnimationState? get animationState => _drawable?.animationState;

  Skeleton? get skeleton => _drawable?.skeleton;
}

enum AssetType { Asset, File, Http, Raw }

class SpineWidget extends StatefulWidget {
  final String? skeletonFile;
  final String? atlasFile;
  final SkeletonData? skeletonData;
  final Atlas? atlas;
  final SpineWidgetController controller;
  final AssetType _assetType;

  const SpineWidget.asset(this.skeletonFile, this.atlasFile, this.controller, {super.key})
      : _assetType = AssetType.Asset,
        atlas = null,
        skeletonData = null;

  const SpineWidget.file(this.skeletonFile, this.atlasFile, this.controller, {super.key})
      : _assetType = AssetType.File,
        atlas = null,
        skeletonData = null;

  const SpineWidget.http(this.skeletonFile, this.atlasFile, this.controller, {super.key})
      : _assetType = AssetType.Http,
        atlas = null,
        skeletonData = null;

  const SpineWidget.raw(this.skeletonData, this.atlas, this.controller, {super.key})
      : _assetType = AssetType.Raw,
        atlasFile = null,
        skeletonFile = null;

  @override
  State<SpineWidget> createState() => _SpineWidgetState();
}

class _SpineWidgetState extends State<SpineWidget> {
  SkeletonDrawable? skeletonDrawable;

  @override
  void initState() {
    super.initState();
    if (widget._assetType == AssetType.Raw) {
      loadRaw(widget.skeletonData!, widget.atlas!);
    } else {
      loadFromAsset(widget.skeletonFile!, widget.atlasFile!, widget._assetType);
    }
  }

  void loadRaw(SkeletonData skeletonData, Atlas atlas) {
    skeletonDrawable = SkeletonDrawable(atlas, skeletonData, false);
    skeletonDrawable?.update(0.016);
  }

  void loadFromAsset(String skeletonFile, String atlasFile, AssetType assetType) async {
    late Atlas atlas;
    late SkeletonData skeletonData;

    switch (assetType) {
      case AssetType.Asset:
        atlas = await Atlas.fromAsset(rootBundle, atlasFile);
        skeletonData = skeletonFile.endsWith(".json")
            ? SkeletonData.fromJson(atlas, await rootBundle.loadString(skeletonFile))
            : SkeletonData.fromBinary(atlas, (await rootBundle.load(skeletonFile)).buffer.asUint8List());
        break;
      case AssetType.File:
        atlas = await Atlas.fromFile(atlasFile);
        skeletonData = skeletonFile.endsWith(".json")
            ? SkeletonData.fromJson(atlas, utf8.decode(await File(skeletonFile).readAsBytes()))
            : SkeletonData.fromBinary(atlas, await File(skeletonFile).readAsBytes());
        break;
      case AssetType.Http:
        atlas = await Atlas.fromUrl(atlasFile);
        skeletonData = skeletonFile.endsWith(".json")
            ? SkeletonData.fromJson(atlas, utf8.decode((await http.get(Uri.parse(skeletonFile))).bodyBytes))
            : SkeletonData.fromBinary(atlas, (await http.get(Uri.parse(skeletonFile))).bodyBytes);
        break;
    }

    skeletonDrawable = SkeletonDrawable(atlas, skeletonData, true);
    widget.controller._initialize(atlas, skeletonData, skeletonDrawable!);
    skeletonDrawable?.update(0);
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    if (skeletonDrawable != null) {
      print("Skeleton loaded, rebuilding painter");
      return _SpineRenderObjectWidget(skeletonDrawable!, widget.controller);
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
  final SkeletonDrawable _skeletonDrawable;
  final SpineWidgetController _controller;

  _SpineRenderObjectWidget(this._skeletonDrawable, this._controller);

  @override
  RenderObject createRenderObject(BuildContext context) {
    return _SpineRenderObject(_skeletonDrawable, _controller);
  }

  @override
  void updateRenderObject(BuildContext context, covariant _SpineRenderObject renderObject) {
    renderObject.skeletonDrawable = _skeletonDrawable;
  }
}

class _SpineRenderObject extends RenderBox {
  SkeletonDrawable _skeletonDrawable;
  SpineWidgetController _controller;
  double _deltaTime = 0;
  final Stopwatch _stopwatch = Stopwatch();

  _SpineRenderObject(this._skeletonDrawable, this._controller);

  set skeletonDrawable(SkeletonDrawable skeletonDrawable) {
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
  void attach(rendering.PipelineOwner owner) {
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

    canvas.save();
    canvas.translate(offset.dx + size.width / 2, offset.dy + size.height);

    final commands = _skeletonDrawable.render();
    for (final cmd in commands) {
      canvas.drawVertices(
          cmd.vertices, rendering.BlendMode.modulate, _skeletonDrawable.atlas.atlasPagePaints[cmd.atlasPageIndex]);
    }

    canvas.restore();
    SchedulerBinding.instance.scheduleFrameCallback(_beginFrame);
  }
}
