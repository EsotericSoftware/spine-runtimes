import 'dart:math';

import 'package:flutter/rendering.dart' as rendering;
import 'package:flutter/scheduler.dart';
import 'package:flutter/services.dart';
import 'package:flutter/widgets.dart';

import 'spine_flutter.dart';

class SpineWidgetController {
  SkeletonDrawable? _drawable;
  double _offsetX = 0, _offsetY = 0, _scaleX = 1, _scaleY = 1;
  final void Function(SpineWidgetController controller)? onInitialized;
  final void Function(SpineWidgetController controller)? onBeforeUpdateWorldTransforms;
  final void Function(SpineWidgetController controller)? onAfterUpdateWorldTransforms;
  final void Function(SpineWidgetController controller, Canvas canvas)? onBeforePaint;
  final void Function(SpineWidgetController controller, Canvas canvas)? onAfterPaint;

  SpineWidgetController({this.onInitialized, this.onBeforeUpdateWorldTransforms, this.onAfterUpdateWorldTransforms, this.onBeforePaint, this.onAfterPaint});

  void _initialize(SkeletonDrawable drawable) {
    if (_drawable != null) throw Exception("SpineWidgetController already initialized. A controller can only be used with one widget.");
    _drawable = drawable;
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

  void _setCoordinateTransform(double offsetX, double offsetY, double scaleX, double scaleY) {
    _offsetX = offsetX;
    _offsetY = offsetY;
    _scaleX = scaleX;
    _scaleY = scaleY;
  }

  Offset toSkeletonCoordinates(Offset position) {
    var x = position.dx;
    var y = position.dy;
    return Offset(x / _scaleX - _offsetX, y / _scaleY - _offsetY);
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
  AssetBundle? _bundle;
  final String? _skeletonFile;
  final String? _atlasFile;
  final SkeletonDrawable? _drawable;
  final SpineWidgetController _controller;
  final BoxFit _fit;
  final Alignment _alignment;
  final BoundsProvider _boundsProvider;
  final bool _sizedByBounds;

  SpineWidget.asset(this._skeletonFile, this._atlasFile, this._controller, {AssetBundle? bundle, BoxFit? fit, Alignment? alignment, BoundsProvider? boundsProvider, bool? sizedByBounds, super.key})
      : _assetType = AssetType.Asset,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null {
    _bundle = bundle ?? rootBundle;
  }

  SpineWidget.file(this._skeletonFile, this._atlasFile, this._controller, {BoxFit? fit, Alignment? alignment, BoundsProvider? boundsProvider, bool? sizedByBounds, super.key})
      : _assetType = AssetType.File,
        _bundle = null,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null;

  SpineWidget.http(this._skeletonFile, this._atlasFile, this._controller, {BoxFit? fit, Alignment? alignment, BoundsProvider? boundsProvider, bool? sizedByBounds, super.key})
      : _assetType = AssetType.Http,
        _bundle = null,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null;

  SpineWidget.drawable(this._drawable, this._controller, {BoxFit? fit, Alignment? alignment, BoundsProvider? boundsProvider, bool? sizedByBounds, super.key})
      : _assetType = AssetType.Drawable,
        _bundle = null,
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

  @override
  void initState() {
    super.initState();
    if (widget._assetType == AssetType.Drawable) {
      loadDrawable(widget._drawable!);
    } else {
      loadFromAsset(widget._bundle, widget._skeletonFile!, widget._atlasFile!, widget._assetType);
    }
  }

  void loadDrawable(SkeletonDrawable drawable) {
    widget._controller._initialize(drawable);
    setState(() {});
  }

  void loadFromAsset(AssetBundle? bundle, String atlasFile, String skeletonFile, AssetType assetType) async {
    switch (assetType) {
      case AssetType.Asset:
        loadDrawable(await SkeletonDrawable.fromAsset(atlasFile, skeletonFile, bundle: bundle));
        break;
      case AssetType.File:
        loadDrawable(await SkeletonDrawable.fromFile(atlasFile, skeletonFile));
        break;
      case AssetType.Http:
        loadDrawable(await SkeletonDrawable.fromHttp(atlasFile, skeletonFile));
        break;
      case AssetType.Drawable:
        throw Exception("Drawable can not be loaded via loadFromAsset().");
    }
  }

  @override
  Widget build(BuildContext context) {
    if (widget._controller._drawable != null) {
      return _SpineRenderObjectWidget(widget._controller._drawable!, widget._controller, widget._fit, widget._alignment, widget._boundsProvider, widget._sizedByBounds);
    } else {
      return const SizedBox();
    }
  }

  @override
  void dispose() {
    super.dispose();
    widget._controller._drawable?.dispose();
  }
}

class _SpineRenderObjectWidget extends LeafRenderObjectWidget {
  final SkeletonDrawable _skeletonDrawable;
  final SpineWidgetController _controller;
  final BoxFit _fit;
  final Alignment _alignment;
  final BoundsProvider _boundsProvider;
  final bool _sizedByBounds;

  const _SpineRenderObjectWidget(this._skeletonDrawable, this._controller, this._fit, this._alignment, this._boundsProvider, this._sizedByBounds);

  @override
  RenderObject createRenderObject(BuildContext context) {
    return _SpineRenderObject(_skeletonDrawable, _controller, _fit, _alignment, _boundsProvider, _sizedByBounds);
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
  SpineWidgetController _controller;
  double _deltaTime = 0;
  final Stopwatch _stopwatch = Stopwatch();
  BoxFit _fit;
  Alignment _alignment;
  BoundsProvider _boundsProvider;
  bool _sizedByBounds;
  Bounds _bounds;
  _SpineRenderObject(this._skeletonDrawable, this._controller, this._fit, this._alignment, this._boundsProvider, this._sizedByBounds): _bounds = _boundsProvider.computeBounds(_skeletonDrawable);

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
    _controller.onBeforeUpdateWorldTransforms?.call(_controller);
    _skeletonDrawable.update(_deltaTime);
    _controller.onAfterUpdateWorldTransforms?.call(_controller);
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

    var offsetX = offset.dx + size.width / 2.0 + (_alignment.x * size.width / 2.0);
    var offsetY = offset.dy + size.height / 2.0 + (_alignment.y * size.height / 2.0);
    canvas
      ..translate(offsetX, offsetY)
      ..scale(scaleX, scaleY)
      ..translate(x, y);
    _controller._setCoordinateTransform(x + offsetX / scaleY, y + offsetY / scaleY, scaleX, scaleY);
  }

  @override
  void paint(PaintingContext context, Offset offset) {
    final Canvas canvas = context.canvas
      ..save()
      ..clipRect(offset & size);

    canvas.save();
    _setCanvasTransform(canvas, offset);

    _controller.onBeforePaint?.call(_controller, canvas);
    _skeletonDrawable.renderToCanvas(canvas);
    _controller.onAfterPaint?.call(_controller, canvas);

    canvas.restore();
    SchedulerBinding.instance.scheduleFrameCallback(_beginFrame);
  }
}
