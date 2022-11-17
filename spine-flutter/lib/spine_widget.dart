import 'dart:math';

import 'package:flutter/rendering.dart' as rendering;
import 'package:flutter/scheduler.dart';
import 'package:flutter/widgets.dart';

import 'spine_flutter.dart';

class SpineWidgetController {
  SkeletonDrawable? _drawable;
  final void Function(SpineWidgetController controller)? onInitialized;
  bool initialized = false;

  SpineWidgetController([this.onInitialized]);

  void _initialize(SkeletonDrawable drawable) {
    if (_drawable != null) throw Exception("SpineWidgetController already initialized. A controller can only be used with one widget.");
    _drawable = drawable;
    initialized = true;
    onInitialized?.call(this);
  }

  Atlas get atlas  {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!.atlas;
  }

  SkeletonData get skeletonData {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!.skeletonData;
  }

  AnimationStateData get animationStateData {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!.animationStateData;
  }

  AnimationState get animationState {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!.animationState;
  }

  Skeleton get skeleton {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
      return _drawable!.skeleton;
  }

  SkeletonDrawable get drawable {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!;
  }
}

enum AssetType { Asset, File, Http, Drawable }

abstract class BoundsProvider {
  const BoundsProvider();

  Bounds computeBounds(SkeletonDrawable drawable);
}

class SetupPoseBounds extends BoundsProvider {
  const SetupPoseBounds();

  @override
  Bounds computeBounds(SkeletonDrawable drawable) {
    return drawable.skeleton.getBounds();
  }
}

class RawBounds extends BoundsProvider {
  final double x, y, width, height;

  RawBounds(this.x, this.y, this.width, this.height);

  @override
  Bounds computeBounds(SkeletonDrawable drawable) {
    return Bounds(x, y, width, height);
  }
}

class SkinAndAnimationBounds extends BoundsProvider {
  final List<String> _skins;
  final String? _animation;

  SkinAndAnimationBounds(this._skins, [this._animation]);

  @override
  Bounds computeBounds(SkeletonDrawable drawable) {
    var data = drawable.skeletonData;
    var oldSkin = drawable.skeleton.getSkin();
    var customSkin = Skin("custom-skin");
    for (var skinName in _skins) {
      var skin = data.findSkin(skinName);
      if (skin == null) continue;
      customSkin.addSkin(skin);
    }
    drawable.skeleton.setSkin(customSkin);
    drawable.skeleton.setToSetupPose();
    var bounds = drawable.skeleton.getBounds();
    customSkin.dispose();

    if (oldSkin == null) {
      drawable.skeleton.setSkinByName("");
    } else {
      drawable.skeleton.setSkin(oldSkin);
    }
    drawable.skeleton.setToSetupPose();
    return bounds;
  }
}

class ComputedBounds extends BoundsProvider {
  @override
  Bounds computeBounds(SkeletonDrawable drawable) {
    return Bounds(0, 0, 0, 0);
  }
}

class SpineWidget extends StatefulWidget {
  final AssetType _assetType;
  final String? _skeletonFile;
  final String? _atlasFile;
  final SkeletonDrawable? _drawable;
  final SpineWidgetController _controller;
  final BoxFit _fit;
  final Alignment _alignment;
  final BoundsProvider _boundsProvider;
  final bool _sizedByBounds;

  const SpineWidget.asset(this._skeletonFile, this._atlasFile, this._controller, {BoxFit? fit, Alignment? alignment, BoundsProvider? boundsProvider, bool? sizedByBounds, super.key})
      : _assetType = AssetType.Asset,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null;

  const SpineWidget.file(this._skeletonFile, this._atlasFile, this._controller, {BoxFit? fit, Alignment? alignment, BoundsProvider? boundsProvider, bool? sizedByBounds, super.key})
      : _assetType = AssetType.File,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null;

  const SpineWidget.http(this._skeletonFile, this._atlasFile, this._controller, {BoxFit? fit, Alignment? alignment, BoundsProvider? boundsProvider, bool? sizedByBounds, super.key})
      : _assetType = AssetType.Http,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null;

  const SpineWidget.drawable(this._drawable, this._controller, {BoxFit? fit, Alignment? alignment, BoundsProvider? boundsProvider, bool? sizedByBounds, super.key})
      : _assetType = AssetType.Drawable,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _skeletonFile = null,
        _atlasFile = null;

  @override
  State<SpineWidget> createState() => _SpineWidgetState();
}

class _SpineWidgetState extends State<SpineWidget> {
  SkeletonDrawable? skeletonDrawable;

  @override
  void initState() {
    super.initState();
    if (widget._assetType == AssetType.Drawable) {
      loadDrawable(widget._drawable!);
    } else {
      loadFromAsset(widget._skeletonFile!, widget._atlasFile!, widget._assetType);
    }
  }

  void loadDrawable(SkeletonDrawable drawable) {
    skeletonDrawable = drawable;
    widget._controller._initialize(skeletonDrawable!);
    skeletonDrawable?.update(0);
    setState(() {});
  }

  void loadFromAsset(String skeletonFile, String atlasFile, AssetType assetType) async {
    late Atlas atlas;
    late SkeletonData skeletonData;

    switch (assetType) {
      case AssetType.Asset:
        loadDrawable(await SkeletonDrawable.fromAsset(skeletonFile, atlasFile));
        break;
      case AssetType.File:
        loadDrawable(await SkeletonDrawable.fromFile(skeletonFile, atlasFile));
        break;
      case AssetType.Http:
        loadDrawable(await SkeletonDrawable.fromHttp(skeletonFile, atlasFile));
        break;
      case AssetType.Drawable:
        throw Exception("Drawable can not be loaded via loadFromAsset().");
    }
  }

  @override
  Widget build(BuildContext context) {
    if (skeletonDrawable != null) {
      print("Skeleton loaded, rebuilding painter");
      return _SpineRenderObjectWidget(skeletonDrawable!, widget._fit, widget._alignment, widget._boundsProvider, widget._sizedByBounds);
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
  final BoxFit _fit;
  final Alignment _alignment;
  final BoundsProvider _boundsProvider;
  final bool _sizedByBounds;

  _SpineRenderObjectWidget(this._skeletonDrawable, this._fit, this._alignment, this._boundsProvider, this._sizedByBounds);

  @override
  RenderObject createRenderObject(BuildContext context) {
    return _SpineRenderObject(_skeletonDrawable, _fit, _alignment, _boundsProvider, _sizedByBounds);
  }

  @override
  void updateRenderObject(BuildContext context, covariant _SpineRenderObject renderObject) {
    renderObject.skeletonDrawable = _skeletonDrawable;
    renderObject.fit = _fit;
    renderObject.alignment = _alignment;
    renderObject.boundsProvider = _boundsProvider;
    renderObject.sizedByBounds = _sizedByBounds;
  }
}

class _SpineRenderObject extends RenderBox {
  SkeletonDrawable _skeletonDrawable;
  double _deltaTime = 0;
  final Stopwatch _stopwatch = Stopwatch();
  BoxFit _fit;
  Alignment _alignment;
  BoundsProvider _boundsProvider;
  bool _sizedByBounds;
  Bounds _bounds;
  _SpineRenderObject(this._skeletonDrawable, this._fit, this._alignment, this._boundsProvider, this._sizedByBounds): _bounds = _boundsProvider.computeBounds(_skeletonDrawable);

  set skeletonDrawable(SkeletonDrawable skeletonDrawable) {
    if (_skeletonDrawable == skeletonDrawable) return;

    _skeletonDrawable = skeletonDrawable;
    _bounds = _boundsProvider.computeBounds(_skeletonDrawable);
    markNeedsLayout();
    markNeedsPaint();
  }

  BoxFit get fit => _fit;

  set fit(BoxFit fit) {
    if (fit != _fit) {
      _fit = fit;
      markNeedsLayout();
      markNeedsPaint();
    }
  }

  Alignment get alignment => _alignment;

  set alignment(Alignment alignment) {
    if (alignment != _alignment) {
      _alignment = alignment;
      markNeedsLayout();
      markNeedsPaint();
    }
  }

  BoundsProvider get boundsProvider => _boundsProvider;

  set boundsProvider(BoundsProvider boundsProvider) {
    if (boundsProvider != _boundsProvider) {
      _boundsProvider = boundsProvider;
      _bounds = boundsProvider.computeBounds(_skeletonDrawable);
      markNeedsLayout();
      markNeedsPaint();
    }
  }

  bool get sizedByBounds => _sizedByBounds;

  set sizedByBounds(bool sizedByBounds) {
    if (sizedByBounds != _sizedByBounds) {
      _sizedByBounds = _sizedByBounds;
      markNeedsLayout();
      markNeedsPaint();
    }
  }

  @override
  bool get sizedByParent => !_sizedByBounds;

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
    final double y = -_bounds.y - _bounds.height / 2.0 - (_alignment.y * _bounds.height / 2.0);
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

    _skeletonDrawable.renderToCanvas(canvas);

    canvas.restore();
    SchedulerBinding.instance.scheduleFrameCallback(_beginFrame);
  }
}
