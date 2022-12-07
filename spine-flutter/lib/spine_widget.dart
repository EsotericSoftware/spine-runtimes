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
  final void Function(SpineWidgetController controller)?
      onBeforeUpdateWorldTransforms;
  final void Function(SpineWidgetController controller)?
      onAfterUpdateWorldTransforms;
  final void Function(SpineWidgetController controller, Canvas canvas)?
      onBeforePaint;
  final void Function(SpineWidgetController controller, Canvas canvas,
      List<RenderCommand> commands)? onAfterPaint;

  SpineWidgetController(
      {this.onInitialized,
      this.onBeforeUpdateWorldTransforms,
      this.onAfterUpdateWorldTransforms,
      this.onBeforePaint,
      this.onAfterPaint});

  void _initialize(SkeletonDrawable drawable) {
    if (_drawable != null)
      throw Exception(
          "SpineWidgetController already initialized. A controller can only be used with one widget.");
    _drawable = drawable;
    onInitialized?.call(this);
  }

  Atlas get atlas {
    if (_drawable == null)
      throw Exception("Controller is not initialized yet.");
    return _drawable!.atlas;
  }

  SkeletonData get skeletonData {
    if (_drawable == null)
      throw Exception("Controller is not initialized yet.");
    return _drawable!.skeletonData;
  }

  AnimationStateData get animationStateData {
    if (_drawable == null)
      throw Exception("Controller is not initialized yet.");
    return _drawable!.animationStateData;
  }

  AnimationState get animationState {
    if (_drawable == null)
      throw Exception("Controller is not initialized yet.");
    return _drawable!.animationState;
  }

  Skeleton get skeleton {
    if (_drawable == null)
      throw Exception("Controller is not initialized yet.");
    return _drawable!.skeleton;
  }

  SkeletonDrawable get drawable {
    if (_drawable == null)
      throw Exception("Controller is not initialized yet.");
    return _drawable!;
  }

  void _setCoordinateTransform(
      double offsetX, double offsetY, double scaleX, double scaleY) {
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

enum AssetType { asset, file, http, drawable }

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
  final List<String> skins;
  final String? animation;
  final double stepTime;

  SkinAndAnimationBounds(
      {List<String>? skins, this.animation, this.stepTime = 0.1})
      : skins = skins == null || skins.isEmpty ? ["default"] : skins;

  @override
  Bounds computeBounds(SkeletonDrawable drawable) {
    final data = drawable.skeletonData;
    final oldSkin = drawable.skeleton.getSkin();
    final customSkin = Skin("custom-skin");
    for (final skinName in skins) {
      final skin = data.findSkin(skinName);
      if (skin == null) continue;
      customSkin.addSkin(skin);
    }
    drawable.skeleton.setSkin(customSkin);
    drawable.skeleton.setToSetupPose();

    final animation =
        this.animation != null ? data.findAnimation(this.animation!) : null;
    double minX = double.infinity;
    double minY = double.infinity;
    double maxX = double.negativeInfinity;
    double maxY = double.negativeInfinity;
    if (animation == null) {
      final bounds = drawable.skeleton.getBounds();
      minX = bounds.x;
      minY = bounds.y;
      maxX = minX + bounds.width;
      maxY = minY + bounds.height;
    } else {
      drawable.animationState.setAnimation(0, animation, false);
      final steps = max(animation.getDuration() / stepTime, 1.0).toInt();
      for (int i = 0; i < steps; i++) {
        drawable.update(i > 0 ? stepTime : 0);
        final bounds = drawable.skeleton.getBounds();
        minX = min(minX, bounds.x);
        minY = min(minY, bounds.y);
        maxX = max(maxX, minX + bounds.width);
        maxY = max(maxY, minY + bounds.height);
      }
    }
    customSkin.dispose();
    drawable.skeleton.setSkinByName("default");
    drawable.animationState.clearTracks();
    if (oldSkin != null) drawable.skeleton.setSkin(oldSkin);
    drawable.skeleton.setToSetupPose();
    drawable.update(0);
    return Bounds(minX, minY, maxX - minX, maxY - minY);
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
  final AssetBundle? _bundle;
  final String? _skeletonFile;
  final String? _atlasFile;
  final SkeletonDrawable? _drawable;
  final SpineWidgetController _controller;
  final BoxFit _fit;
  final Alignment _alignment;
  final BoundsProvider _boundsProvider;
  final bool _sizedByBounds;

  SpineWidget.asset(this._atlasFile, this._skeletonFile, this._controller,
      {AssetBundle? bundle,
      BoxFit? fit,
      Alignment? alignment,
      BoundsProvider? boundsProvider,
      bool? sizedByBounds,
      super.key})
      : _assetType = AssetType.asset,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null,
        _bundle = bundle ?? rootBundle;

  const SpineWidget.file(this._atlasFile, this._skeletonFile, this._controller,
      {BoxFit? fit,
      Alignment? alignment,
      BoundsProvider? boundsProvider,
      bool? sizedByBounds,
      super.key})
      : _assetType = AssetType.file,
        _bundle = null,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null;

  const SpineWidget.http(this._atlasFile, this._skeletonFile, this._controller,
      {BoxFit? fit,
      Alignment? alignment,
      BoundsProvider? boundsProvider,
      bool? sizedByBounds,
      super.key})
      : _assetType = AssetType.http,
        _bundle = null,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null;

  const SpineWidget.drawable(this._drawable, this._controller,
      {BoxFit? fit,
      Alignment? alignment,
      BoundsProvider? boundsProvider,
      bool? sizedByBounds,
      super.key})
      : _assetType = AssetType.drawable,
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
  late Bounds _computedBounds;
  SkeletonDrawable? _drawable;

  @override
  void initState() {
    super.initState();
    if (widget._assetType == AssetType.drawable) {
      loadDrawable(widget._drawable!);
    } else {
      loadFromAsset(widget._bundle, widget._atlasFile!, widget._skeletonFile!,
          widget._assetType);
    }
  }

  void loadDrawable(SkeletonDrawable drawable) {
    _drawable = drawable;
    _computedBounds = widget._boundsProvider.computeBounds(drawable);
    widget._controller._initialize(drawable);
    setState(() {});
  }

  void loadFromAsset(AssetBundle? bundle, String atlasFile, String skeletonFile,
      AssetType assetType) async {
    switch (assetType) {
      case AssetType.asset:
        loadDrawable(await SkeletonDrawable.fromAsset(atlasFile, skeletonFile,
            bundle: bundle));
        break;
      case AssetType.file:
        loadDrawable(await SkeletonDrawable.fromFile(atlasFile, skeletonFile));
        break;
      case AssetType.http:
        loadDrawable(await SkeletonDrawable.fromHttp(atlasFile, skeletonFile));
        break;
      case AssetType.drawable:
        throw Exception("Drawable can not be loaded via loadFromAsset().");
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_drawable != null) {
      return _SpineRenderObjectWidget(
          _drawable!,
          widget._controller,
          widget._fit,
          widget._alignment,
          _computedBounds,
          widget._sizedByBounds);
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
  final Bounds _bounds;
  final bool _sizedByBounds;

  const _SpineRenderObjectWidget(this._skeletonDrawable, this._controller,
      this._fit, this._alignment, this._bounds, this._sizedByBounds);

  @override
  RenderObject createRenderObject(BuildContext context) {
    return _SpineRenderObject(_skeletonDrawable, _controller, _fit, _alignment,
        _bounds, _sizedByBounds);
  }

  @override
  void updateRenderObject(
      BuildContext context, covariant _SpineRenderObject renderObject) {
    renderObject.skeletonDrawable = _skeletonDrawable;
    renderObject.fit = _fit;
    renderObject.alignment = _alignment;
    renderObject.bounds = _bounds;
    renderObject.sizedByBounds = _sizedByBounds;
  }
}

class _SpineRenderObject extends RenderBox {
  SkeletonDrawable _skeletonDrawable;
  final SpineWidgetController _controller;
  double _deltaTime = 0;
  final Stopwatch _stopwatch = Stopwatch();
  BoxFit _fit;
  Alignment _alignment;
  Bounds _bounds;
  bool _sizedByBounds;

  _SpineRenderObject(this._skeletonDrawable, this._controller, this._fit,
      this._alignment, this._bounds, this._sizedByBounds);

  set skeletonDrawable(SkeletonDrawable skeletonDrawable) {
    if (_skeletonDrawable == skeletonDrawable) return;

    _skeletonDrawable = skeletonDrawable;
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

  Bounds get bounds => _bounds;

  set bounds(Bounds bounds) {
    if (bounds != _bounds) {
      _bounds = bounds;
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
    return _computeConstrainedSize(
            BoxConstraints.tightForFinite(height: height))
        .width;
  }

  @override
  double computeMaxIntrinsicWidth(double height) {
    return _computeConstrainedSize(
            BoxConstraints.tightForFinite(height: height))
        .width;
  }

  @override
  double computeMinIntrinsicHeight(double width) {
    return _computeConstrainedSize(BoxConstraints.tightForFinite(width: width))
        .height;
  }

  @override
  double computeMaxIntrinsicHeight(double width) {
    return _computeConstrainedSize(BoxConstraints.tightForFinite(width: width))
        .height;
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
    return sizedByParent
        ? constraints.smallest
        : constraints.constrainSizeAndAttemptToPreserveAspectRatio(
            Size(_bounds.width, _bounds.height));
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
    final double x =
        -_bounds.x - _bounds.width / 2.0 - (_alignment.x * _bounds.width / 2.0);
    final double y = -_bounds.y -
        _bounds.height / 2.0 -
        (_alignment.y * _bounds.height / 2.0);
    double scaleX = 1.0, scaleY = 1.0;

    switch (_fit) {
      case BoxFit.fill:
        scaleX = size.width / _bounds.width;
        scaleY = size.height / _bounds.height;
        break;
      case BoxFit.contain:
        scaleX = scaleY =
            min(size.width / _bounds.width, size.height / _bounds.height);
        break;
      case BoxFit.cover:
        scaleX = scaleY =
            max(size.width / _bounds.width, size.height / _bounds.height);
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
        final double scale =
            min(size.width / _bounds.width, size.height / _bounds.height);
        scaleX = scaleY = scale < 1.0 ? scale : 1.0;
        break;
    }

    var offsetX =
        offset.dx + size.width / 2.0 + (_alignment.x * size.width / 2.0);
    var offsetY =
        offset.dy + size.height / 2.0 + (_alignment.y * size.height / 2.0);
    canvas
      ..translate(offsetX, offsetY)
      ..scale(scaleX, scaleY)
      ..translate(x, y);
    _controller._setCoordinateTransform(
        x + offsetX / scaleY, y + offsetY / scaleY, scaleX, scaleY);
  }

  @override
  void paint(PaintingContext context, Offset offset) {
    final Canvas canvas = context.canvas
      ..save()
      ..clipRect(offset & size);

    canvas.save();
    _setCanvasTransform(canvas, offset);

    _controller.onBeforePaint?.call(_controller, canvas);
    var commands = _skeletonDrawable.render();
    for (final cmd in commands) {
      canvas.drawVertices(cmd.vertices, rendering.BlendMode.modulate,
          _skeletonDrawable.atlas.atlasPagePaints[cmd.atlasPageIndex]);
    }
    _controller.onAfterPaint?.call(_controller, canvas, commands);

    canvas.restore();
    SchedulerBinding.instance.scheduleFrameCallback(_beginFrame);
  }
}
