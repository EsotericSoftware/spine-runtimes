import 'dart:convert';
import 'dart:io';
import 'dart:math';

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
  final BoxFit? fit;
  final Alignment? alignment;
  final AssetType _assetType;

  const SpineWidget.asset(this.skeletonFile, this.atlasFile, this.controller, {this.fit, this.alignment, super.key})
      : _assetType = AssetType.Asset,
        atlas = null,
        skeletonData = null;

  const SpineWidget.file(this.skeletonFile, this.atlasFile, this.controller, {this.fit, this.alignment, super.key})
      : _assetType = AssetType.File,
        atlas = null,
        skeletonData = null;

  const SpineWidget.http(this.skeletonFile, this.atlasFile, this.controller, {this.fit, this.alignment, super.key})
      : _assetType = AssetType.Http,
        atlas = null,
        skeletonData = null;

  const SpineWidget.raw(this.skeletonData, this.atlas, this.controller, {this.fit, this.alignment, super.key})
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
    skeletonDrawable?.update(0);
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
      case AssetType.Raw:
        throw Exception("Raw assets can not be loaded via loadFromAsset().");
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
      return _SpineRenderObjectWidget(skeletonDrawable!, widget.controller, widget.fit, widget.alignment);
    } else {
      print("Skeleton not loaded yet");
      return const SizedBox();
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
  final BoxFit _fit;
  final Alignment _alignment;

  _SpineRenderObjectWidget(this._skeletonDrawable, this._controller, BoxFit? fit, Alignment? alignment) :
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center;

  @override
  RenderObject createRenderObject(BuildContext context) {
    return _SpineRenderObject(_skeletonDrawable, _controller, _fit, _alignment);
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
  BoxFit _fit;
  Alignment _alignment;
  Bounds _bounds;

  _SpineRenderObject(this._skeletonDrawable, this._controller, this._fit, this._alignment): _bounds = _computeBounds(_skeletonDrawable);

  static Bounds _computeBounds(SkeletonDrawable drawable) {
    return drawable.skeleton.getBounds();
  }

  BoxFit get fit => _fit;

  set fit(BoxFit fit) {
    if (fit != _fit) {
      _fit = fit;
      markNeedsPaint();
    }
  }

  Alignment get alignment => _alignment;

  set alignment(Alignment alignment) {
    if (alignment != _alignment) {
      _alignment = alignment;
      markNeedsPaint();
    }
  }

  set skeletonDrawable(SkeletonDrawable skeletonDrawable) {
    if (_skeletonDrawable == skeletonDrawable) return;

    _skeletonDrawable = skeletonDrawable;
    _bounds = _computeBounds(_skeletonDrawable);
    markNeedsPaint();
  }

  @override
  bool get sizedByParent => true;

  @override
  bool get isRepaintBoundary => true;

  @override
  bool hitTestSelf(Offset position) => true;

  @override
  double computeMinIntrinsicWidth(double height) {
    return _computeConstrainedSize(BoxConstraints.tightForFinite(height: height)).width;
  }

  @override
  double computeMaxIntrinsicWidth(double height) {
    return _computeConstrainedSize(BoxConstraints.tightForFinite(height: height)).width;
  }

  @override
  double computeMinIntrinsicHeight(double width) {
    return _computeConstrainedSize(BoxConstraints.tightForFinite(width: width)).height;
  }

  @override
  double computeMaxIntrinsicHeight(double width) {
    return _computeConstrainedSize(BoxConstraints.tightForFinite(width: width)).height;
  }

  // Called when not sizedByParent, uses the intrinsic width/height for sizing, while trying to retain aspect ratio.
  @override
  void performLayout() {
    if (!sizedByParent) size = _computeConstrainedSize(constraints);
  }

  // Called when sizedByParent, we want to go as big as possible.
  @override
  void performResize() {
    size = constraints.biggest;
  }

  Size _computeConstrainedSize(BoxConstraints constraints) {
    return sizedByParent ? constraints.smallest : constraints.constrainSizeAndAttemptToPreserveAspectRatio(Size(_bounds.width, _bounds.height));
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

  void _setCanvasTransform(Canvas canvas, Offset offset) {
    final double x = -_bounds.x - _bounds.width / 2.0 - (_alignment.x * _bounds.width / 2.0);
    final double y = -_bounds.y - _bounds.height / 2.0 + (_alignment.y * _bounds.height / 2.0);
    double scaleX = 1.0, scaleY = 1.0;

    switch (_fit) {
      case BoxFit.fill:
        scaleX = size.width / _bounds.width;
        scaleY = size.height / _bounds.height;
        break;
      case BoxFit.contain:
        scaleX = scaleY = min(size.width / _bounds.width, size.height / _bounds.height);
        break;
      case BoxFit.cover:
        scaleX = scaleY = max(size.width / _bounds.width, size.height / _bounds.height);
        break;
      case BoxFit.fitHeight:
        scaleX = scaleY = size.height / _bounds.height;
        break;
      case BoxFit.fitWidth:
        scaleX = scaleY = size.width / _bounds.width;
        break;
      case BoxFit.none:
        scaleX = scaleY = 1.0;
        break;
      case BoxFit.scaleDown:
        final double scale = min(size.width / _bounds.width, size.height / _bounds.height);
        scaleX = scaleY = scale < 1.0 ? scale : 1.0;
        break;
    }

    canvas
      ..translate(
          offset.dx + size.width / 2.0 + (_alignment.x * size.width / 2.0),
          offset.dy + size.height / 2.0 + (_alignment.y * size.height / 2.0))
      ..scale(scaleX, scaleY)
      ..translate(x, y);
  }

  @override
  void paint(PaintingContext context, Offset offset) {
    final Canvas canvas = context.canvas
      ..save()
      ..clipRect(offset & size);

    canvas.save();
    _setCanvasTransform(canvas, offset);

    final commands = _skeletonDrawable.render();
    for (final cmd in commands) {
      canvas.drawVertices(
          cmd.vertices, rendering.BlendMode.modulate, _skeletonDrawable.atlas.atlasPagePaints[cmd.atlasPageIndex]);
    }

    canvas.restore();
    SchedulerBinding.instance.scheduleFrameCallback(_beginFrame);
  }
}
