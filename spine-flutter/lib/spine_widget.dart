//
// Spine Runtimes License Agreement
// Last updated April 5, 2025. Replaces all prior versions.
//
// Copyright (c) 2013-2025, Esoteric Software LLC
//
// Integration of the Spine Runtimes into software or otherwise creating
// derivative works of the Spine Runtimes is permitted under the terms and
// conditions of Section 2 of the Spine Editor License Agreement:
// http://esotericsoftware.com/spine-editor-license
//
// Otherwise, it is permitted to integrate the Spine Runtimes into software
// or otherwise create derivative works of the Spine Runtimes (collectively,
// "Products"), provided that each user of the Products must obtain their own
// Spine Editor license and redistribution of the Products in any form must
// include this license and copyright notice.
//
// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

import 'dart:math';
import 'dart:typed_data';

import 'package:flutter/rendering.dart' as rendering;
import 'package:flutter/scheduler.dart';
import 'package:flutter/services.dart';
import 'package:flutter/widgets.dart';

import 'spine_flutter.dart';

/// Controls how the skeleton of a [SpineWidget] is animated and rendered.
///
/// Upon initialization of a [SpineWidget] the provided [onInitialized] callback method is called once. This method can be used
/// to setup the initial animation(s) of the skeleton, among other things.
///
/// After initialization is complete, the [SpineWidget] is rendered at the screen refresh rate. In each frame,
/// the [AnimationState] is updated and applied to the [Skeleton].
///
/// Next the optionally provided method [onBeforeUpdateWorldTransforms] is called, which can modify the
/// skeleton before its current pose is calculated using [Skeleton.updateWorldTransforms]. After
/// [Skeleton.updateWorldTransforms] has completed, the optional [onAfterUpdateWorldTransforms] method is
/// called, which can modify the current pose before rendering the skeleton.
///
/// Before the skeleton's current pose is rendered by the [SpineWidget] the optional [onBeforePaint] is called,
/// which allows rendering backgrounds or other objects that should go behind the skeleton on the [Canvas]. The
/// [SpineWidget] then renderes the skeleton's current pose, and finally calls the optional [onAfterPaint], which
/// can render additional objects on top of the skeleton.
///
/// The underlying [AtlasFlutter], [SkeletonData], [Skeleton], [AnimationStateData], [AnimationState], and [SkeletonDrawableFlutter]
/// can be accessed through their respective getters to inspect and/or modify the skeleton and its associated data. Accessing
/// this data is only allowed if the [SpineWidget] and its data have been initialized and have not been disposed yet.
///
/// By default, the widget updates and renders the skeleton every frame. The [pause] method can be used to pause updating
/// and rendering the skeleton. The [resume] method resumes updating and rendering the skeleton. The [isPlaying] getter
/// reports the current state.
class SpineWidgetController {
  SkeletonDrawableFlutter? _drawable;
  double _offsetX = 0, _offsetY = 0, _scaleX = 1, _scaleY = 1;
  bool _isPlaying = true;
  _SpineRenderObject? _renderObject;
  final void Function(SpineWidgetController controller)? onInitialized;
  final void Function(SpineWidgetController controller)? onBeforeUpdateWorldTransforms;
  final void Function(SpineWidgetController controller)? onAfterUpdateWorldTransforms;
  final void Function(SpineWidgetController controller, Canvas canvas)? onBeforePaint;
  final void Function(SpineWidgetController controller, Canvas canvas, List<RenderCommandFlutter> commands)?
      onAfterPaint;

  /// Constructs a new [SpineWidget] controller. See the class documentation of [SpineWidgetController] for information on
  /// the optional arguments.
  SpineWidgetController({
    this.onInitialized,
    this.onBeforeUpdateWorldTransforms,
    this.onAfterUpdateWorldTransforms,
    this.onBeforePaint,
    this.onAfterPaint,
  });

  void _initialize(SkeletonDrawableFlutter drawable) {
    var wasInitialized = _drawable != null;
    _drawable = drawable;
    if (!wasInitialized) onInitialized?.call(this);
  }

  /// The [AtlasFlutter] from which images to render the skeleton are sourced.
  AtlasFlutter get atlasFlutter {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!.atlasFlutter;
  }

  /// The setup-pose data used by the skeleton.
  SkeletonData get skeletonData {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!.skeletonData;
  }

  /// The mixing information used by the [AnimationState]
  AnimationStateData get animationStateData {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!.animationStateData;
  }

  /// The [AnimationState] used to manage animations that are being applied to the
  /// skeleton.
  AnimationState get animationState {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!.animationState;
  }

  /// The [Skeleton]
  Skeleton get skeleton {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!.skeleton;
  }

  /// The [SkeletonDrawableFlutter]
  SkeletonDrawableFlutter get drawable {
    if (_drawable == null) throw Exception("Controller is not initialized yet.");
    return _drawable!;
  }

  void _setCoordinateTransform(double offsetX, double offsetY, double scaleX, double scaleY) {
    _offsetX = offsetX;
    _offsetY = offsetY;
    _scaleX = scaleX;
    _scaleY = scaleY;
  }

  void _setRenderObject(_SpineRenderObject? renderObject) {
    _renderObject = renderObject;
  }

  /// Transforms the coordinates given in the [SpineWidget] coordinate system in [position] to
  /// the skeleton coordinate system. See the `ik_following.dart` example how to use this
  /// to move a bone based on user touch input.
  Offset toSkeletonCoordinates(Offset position) {
    var x = position.dx;
    var y = position.dy;
    return Offset(x / _scaleX - _offsetX, y / _scaleY - _offsetY);
  }

  /// Pauses updating and rendering the skeleton.
  void pause() {
    _isPlaying = false;
  }

  /// Resumes updating and rendering the skeleton.
  void resume() {
    _isPlaying = true;
    _renderObject?._stopwatch.reset();
    _renderObject?._stopwatch.start();
    _renderObject?._scheduleFrame();
  }

  bool get isPlaying {
    return _isPlaying;
  }
}

enum _AssetType { asset, file, http, memory, drawable }

/// Base class for bounds providers. A bounds provider calculates the axis aligned bounding box
/// used to scale and fit a skeleton inside the bounds of a [SpineWidget].
abstract class BoundsProvider {
  const BoundsProvider();

  Bounds computeBounds(SkeletonDrawableFlutter drawable);
}

/// A [BoundsProvider] that calculates the bounding box of the skeleton based on the visible
/// attachments in the setup pose.
class SetupPoseBounds extends BoundsProvider {
  const SetupPoseBounds();

  @override
  Bounds computeBounds(SkeletonDrawableFlutter drawable) {
    drawable.skeleton.setupPose();
    drawable.skeleton.updateWorldTransform(Physics.none);
    return drawable.skeleton.bounds;
  }
}

/// A [BoundsProvider] that returns fixed bounds.
class RawBounds extends BoundsProvider {
  final double x, y, width, height;

  RawBounds(this.x, this.y, this.width, this.height);

  @override
  Bounds computeBounds(SkeletonDrawableFlutter drawable) {
    return Bounds(x: x, y: y, width: width, height: height);
  }
}

/// A [BoundsProvider] that calculates the bounding box needed for a combination of skins
/// and an animation.
class SkinAndAnimationBounds extends BoundsProvider {
  final List<String> skins;
  final String? animation;
  final double stepTime;

  /// Constructs a new provider that will use the given [skins] and [animation] to calculate
  /// the bounding box of the skeleton. If no skins are given, the default skin is used.
  /// The [stepTime], given in seconds, defines at what interval the bounds should be sampled
  /// across the entire animation.
  SkinAndAnimationBounds({List<String>? skins, this.animation, this.stepTime = 0.1})
      : skins = skins == null || skins.isEmpty ? ["default"] : skins;

  @override
  Bounds computeBounds(SkeletonDrawableFlutter drawable) {
    final data = drawable.skeletonData;
    final oldSkin = drawable.skeleton.skin;
    final customSkin = Skin("custom-skin");
    for (final skinName in skins) {
      final skin = data.findSkin(skinName);
      if (skin == null) continue;
      customSkin.addSkin(skin);
    }
    drawable.skeleton.setSkin2(customSkin);
    drawable.skeleton.setupPose();

    final animation = this.animation != null ? data.findAnimation(this.animation!) : null;
    double minX = double.infinity;
    double minY = double.infinity;
    double maxX = double.negativeInfinity;
    double maxY = double.negativeInfinity;
    if (animation == null) {
      drawable.skeleton.updateWorldTransform(Physics.none);
      final bounds = drawable.skeleton.bounds;
      minX = bounds.x;
      minY = bounds.y;
      maxX = minX + bounds.width;
      maxY = minY + bounds.height;
    } else {
      drawable.animationState.setAnimation(0, animation.name, false);
      final steps = max(animation.duration / stepTime, 1.0).toInt();
      for (int i = 0; i < steps; i++) {
        drawable.update(i > 0 ? stepTime : 0);
        drawable.skeleton.updateWorldTransform(Physics.none);
        final bounds = drawable.skeleton.bounds;
        minX = min(minX, bounds.x);
        minY = min(minY, bounds.y);
        maxX = max(maxX, minX + bounds.width);
        maxY = max(maxY, minY + bounds.height);
      }
    }
    drawable.skeleton.setSkin("default");
    drawable.animationState.clearTracks();
    if (oldSkin != null) drawable.skeleton.setSkin2(oldSkin);
    drawable.skeleton.setupPose();
    drawable.update(0);
    customSkin.dispose();
    return Bounds(x: minX, y: minY, width: maxX - minX, height: maxY - minY);
  }
}

/// A [StatefulWidget] to display a Spine skeleton. The skeleton can be loaded from an asset bundle ([SpineWidget.fromAsset],
/// local files [SpineWidget.fromFile], URLs [SpineWidget.fromHttp], memory ([SpineWidget.fromMemory]), or a pre-loaded
/// [SkeletonDrawableFlutter] ([SpineWidget.fromDrawable]).
///
/// The skeleton displayed by a `SpineWidget` can be controlled via a [SpineWidgetController].
///
/// The size of the widget can be derived from the bounds provided by a [BoundsProvider]. If the widget is not sized by the bounds
/// computed by the [BoundsProvider], the widget will use the computed bounds to fit the skeleton inside the widget's dimensions.
class SpineWidget extends StatefulWidget {
  final _AssetType _assetType;
  final AssetBundle? _bundle;
  final String? _skeletonFile;
  final String? _atlasFile;
  final SkeletonDrawableFlutter? _drawable;
  final Future<Uint8List> Function(String)? _loadFile;
  final SpineWidgetController _controller;
  final BoxFit _fit;
  final Alignment _alignment;
  final BoundsProvider _boundsProvider;
  final bool _sizedByBounds;

  /// Constructs a new [SpineWidget] from files in the root bundle or the optionally specified [bundle]. The [_atlasFile] specifies the
  /// `.atlas` file to be loaded for the images used to render the skeleton. The [_skeletonFile] specifies either a Skeleton `.json` or
  /// `.skel` file containing the skeleton data.
  ///
  /// After initialization is complete, the provided [_controller] is invoked as per the [SpineWidgetController] semantics, to allow
  /// modifying how the skeleton inside the widget is animated and rendered.
  ///
  /// The skeleton is fitted and aligned inside the widget as per the [fit] and [alignment] arguments. For this purpose, the skeleton
  /// bounds must be computed via a [BoundsProvider]. By default, [BoxFit.contain], [Alignment.center], and a [SetupPoseBounds] provider
  /// are used.
  ///
  /// The widget can optionally by sized by the bounds provided by the [BoundsProvider] by passing `true` for [sizedByBounds].
  SpineWidget.fromAsset(
    this._atlasFile,
    this._skeletonFile,
    this._controller, {
    AssetBundle? bundle,
    BoxFit? fit,
    Alignment? alignment,
    BoundsProvider? boundsProvider,
    bool? sizedByBounds,
    super.key,
  })  : _assetType = _AssetType.asset,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null,
        _loadFile = null,
        _bundle = bundle ?? rootBundle;

  /// Constructs a new [SpineWidget] from files. The [_atlasFile] specifies the `.atlas` file to be loaded for the images used to render
  /// the skeleton. The [_skeletonFile] specifies either a Skeleton `.json` or `.skel` file containing the skeleton data.
  ///
  /// After initialization is complete, the provided [_controller] is invoked as per the [SpineWidgetController] semantics, to allow
  /// modifying how the skeleton inside the widget is animated and rendered.
  ///
  /// The skeleton is fitted and aligned inside the widget as per the [fit] and [alignment] arguments. For this purpose, the skeleton
  /// bounds must be computed via a [BoundsProvider]. By default, [BoxFit.contain], [Alignment.center], and a [SetupPoseBounds] provider
  /// are used.
  ///
  /// The widget can optionally by sized by the bounds provided by the [BoundsProvider] by passing `true` for [sizedByBounds].
  const SpineWidget.fromFile(
    this._atlasFile,
    this._skeletonFile,
    this._controller, {
    BoxFit? fit,
    Alignment? alignment,
    BoundsProvider? boundsProvider,
    bool? sizedByBounds,
    super.key,
  })  : _assetType = _AssetType.file,
        _bundle = null,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null,
        _loadFile = null;

  /// Constructs a new [SpineWidget] from HTTP URLs. The [_atlasFile] specifies the `.atlas` file to be loaded for the images used to render
  /// the skeleton. The [_skeletonFile] specifies either a Skeleton `.json` or `.skel` file containing the skeleton data.
  ///
  /// After initialization is complete, the provided [_controller] is invoked as per the [SpineWidgetController] semantics, to allow
  /// modifying how the skeleton inside the widget is animated and rendered.
  ///
  /// The skeleton is fitted and aligned inside the widget as per the [fit] and [alignment] arguments. For this purpose, the skeleton
  /// bounds must be computed via a [BoundsProvider]. By default, [BoxFit.contain], [Alignment.center], and a [SetupPoseBounds] provider
  /// are used.
  ///
  /// The widget can optionally by sized by the bounds provided by the [BoundsProvider] by passing `true` for [sizedByBounds].
  const SpineWidget.fromHttp(
    this._atlasFile,
    this._skeletonFile,
    this._controller, {
    BoxFit? fit,
    Alignment? alignment,
    BoundsProvider? boundsProvider,
    bool? sizedByBounds,
    super.key,
  })  : _assetType = _AssetType.http,
        _bundle = null,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null,
        _loadFile = null;

  /// Constructs a new [SpineWidget] using a custom file loading function.
  ///
  /// This is the most flexible loading method that allows loading skeleton data from any source
  /// (memory, custom storage, network with caching, etc.).
  ///
  /// After initialization is complete, the provided [_controller] is invoked as per the [SpineWidgetController] semantics, to allow
  /// modifying how the skeleton inside the widget is animated and rendered.
  ///
  /// The skeleton is fitted and aligned inside the widget as per the [fit] and [alignment] arguments. For this purpose, the skeleton
  /// bounds must be computed via a [BoundsProvider]. By default, [BoxFit.contain], [Alignment.center], and a [SetupPoseBounds] provider
  /// are used.
  ///
  /// The widget can optionally by sized by the bounds provided by the [BoundsProvider] by passing `true` for [sizedByBounds].
  const SpineWidget.fromMemory(
    this._atlasFile,
    this._skeletonFile,
    this._loadFile,
    this._controller, {
    BoxFit? fit,
    Alignment? alignment,
    BoundsProvider? boundsProvider,
    bool? sizedByBounds,
    super.key,
  })  : _assetType = _AssetType.memory,
        _bundle = null,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _drawable = null;

  /// Constructs a new [SpineWidget] from a [SkeletonDrawableFlutter].
  ///
  /// After initialization is complete, the provided [_controller] is invoked as per the [SpineWidgetController] semantics, to allow
  /// modifying how the skeleton inside the widget is animated and rendered.
  ///
  /// The skeleton is fitted and aligned inside the widget as per the [fit] and [alignment] arguments. For this purpose, the skeleton
  /// bounds must be computed via a [BoundsProvider]. By default, [BoxFit.contain], [Alignment.center], and a [SetupPoseBounds] provider
  /// are used.
  ///
  /// The widget can optionally by sized by the bounds provided by the [BoundsProvider] by passing `true` for [sizedByBounds].
  const SpineWidget.fromDrawable(
    this._drawable,
    this._controller, {
    BoxFit? fit,
    Alignment? alignment,
    BoundsProvider? boundsProvider,
    bool? sizedByBounds,
    super.key,
  })  : _assetType = _AssetType.drawable,
        _bundle = null,
        _fit = fit ?? BoxFit.contain,
        _alignment = alignment ?? Alignment.center,
        _boundsProvider = boundsProvider ?? const SetupPoseBounds(),
        _sizedByBounds = sizedByBounds ?? false,
        _skeletonFile = null,
        _atlasFile = null,
        _loadFile = null;

  @override
  State<SpineWidget> createState() => _SpineWidgetState();
}

class _SpineWidgetState extends State<SpineWidget> {
  late Bounds _computedBounds;
  SkeletonDrawableFlutter? _drawable;
  int _loadGeneration = 0;

  @override
  void initState() {
    super.initState();
    _loadCurrentDrawable();
  }

  @override
  void didUpdateWidget(covariant SpineWidget oldWidget) {
    super.didUpdateWidget(oldWidget);

    // Check if the skeleton/atlas data has changed. Only re-create
    // everything if it has, otherwise, keep using what's already been
    // loaded.
    bool hasChanged = true;
    if (oldWidget._assetType == widget._assetType) {
      if (oldWidget._assetType == _AssetType.drawable && oldWidget._drawable == widget._drawable) {
        hasChanged = false;
      } else if (oldWidget._skeletonFile == widget._skeletonFile &&
          oldWidget._atlasFile == widget._atlasFile &&
          oldWidget._controller == widget._controller &&
          oldWidget._bundle == widget._bundle) {
        hasChanged = false;
      }
    }

    if (hasChanged) {
      _disposeDrawable(oldWidget._controller);
      _loadCurrentDrawable();
    }
  }

  void _loadCurrentDrawable() {
    final loadGeneration = ++_loadGeneration;
    if (widget._assetType == _AssetType.drawable) {
      loadDrawable(widget._drawable!);
    } else {
      loadFromAsset(widget._bundle, widget._atlasFile!, widget._skeletonFile!, widget._assetType, loadGeneration);
    }
  }

  void _disposeDrawable(SpineWidgetController controller) {
    final drawable = _drawable;
    if (drawable == null) return;

    if (controller._drawable == drawable) controller._drawable = null;
    drawable.dispose();
    _drawable = null;
  }

  void loadDrawable(SkeletonDrawableFlutter drawable) {
    _drawable = drawable;
    _computedBounds = widget._boundsProvider.computeBounds(drawable);
    widget._controller._initialize(drawable);
    setState(() {});
  }

  void loadFromAsset(
    AssetBundle? bundle,
    String atlasFile,
    String skeletonFile,
    _AssetType assetType,
    int loadGeneration,
  ) async {
    late final SkeletonDrawableFlutter drawable;
    switch (assetType) {
      case _AssetType.asset:
        drawable = await SkeletonDrawableFlutter.fromAsset(atlasFile, skeletonFile, bundle: bundle);
        break;
      case _AssetType.file:
        drawable = await SkeletonDrawableFlutter.fromFile(atlasFile, skeletonFile);
        break;
      case _AssetType.http:
        drawable = await SkeletonDrawableFlutter.fromHttp(atlasFile, skeletonFile);
        break;
      case _AssetType.memory:
        drawable = await SkeletonDrawableFlutter.fromMemory(atlasFile, skeletonFile, widget._loadFile!);
        break;
      case _AssetType.drawable:
        throw Exception("Drawable can not be loaded via loadFromAsset().");
    }

    if (!mounted || loadGeneration != _loadGeneration) {
      drawable.dispose();
      return;
    }

    loadDrawable(drawable);
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
        widget._sizedByBounds,
      );
    } else {
      return const SizedBox();
    }
  }

  @override
  void dispose() {
    _loadGeneration++;
    _disposeDrawable(widget._controller);
    super.dispose();
  }
}

class _SpineRenderObjectWidget extends LeafRenderObjectWidget {
  final SkeletonDrawableFlutter _skeletonDrawable;
  final SpineWidgetController _controller;
  final BoxFit _fit;
  final Alignment _alignment;
  final Bounds _bounds;
  final bool _sizedByBounds;

  const _SpineRenderObjectWidget(
    this._skeletonDrawable,
    this._controller,
    this._fit,
    this._alignment,
    this._bounds,
    this._sizedByBounds,
  );

  @override
  RenderObject createRenderObject(BuildContext context) {
    return _SpineRenderObject(_skeletonDrawable, _controller, _fit, _alignment, _bounds, _sizedByBounds);
  }

  @override
  void updateRenderObject(BuildContext context, covariant _SpineRenderObject renderObject) {
    renderObject.skeletonDrawable = _skeletonDrawable;
    renderObject.fit = _fit;
    renderObject.alignment = _alignment;
    renderObject.bounds = _bounds;
    renderObject.sizedByBounds = _sizedByBounds;
  }
}

class _SpineRenderObject extends RenderBox {
  SkeletonDrawableFlutter _skeletonDrawable;
  final SpineWidgetController _controller;
  double _deltaTime = 0;
  final Stopwatch _stopwatch = Stopwatch();
  BoxFit _fit;
  Alignment _alignment;
  Bounds _bounds;
  bool _sizedByBounds;
  bool _disposed = false;
  bool _firstUpdated = false;

  _SpineRenderObject(
    this._skeletonDrawable,
    this._controller,
    this._fit,
    this._alignment,
    this._bounds,
    this._sizedByBounds,
  );

  set skeletonDrawable(SkeletonDrawableFlutter skeletonDrawable) {
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
    return sizedByParent
        ? constraints.smallest
        : constraints.constrainSizeAndAttemptToPreserveAspectRatio(Size(_bounds.width, _bounds.height));
  }

  @override
  void attach(rendering.PipelineOwner owner) {
    super.attach(owner);
    _stopwatch.start();
    SchedulerBinding.instance.scheduleFrameCallback(_beginFrame);
    _controller._setRenderObject(this);
  }

  @override
  void detach() {
    _stopwatch.stop();
    super.detach();
    _controller._setRenderObject(null);
  }

  @override
  void dispose() {
    super.dispose();
    _disposed = true;
  }

  void _scheduleFrame() {
    SchedulerBinding.instance.scheduleFrameCallback(_beginFrame);
  }

  void _beginFrame(Duration duration) {
    if (_disposed) return;
    _deltaTime = _stopwatch.elapsedTicks / _stopwatch.frequency;
    _stopwatch.reset();
    _stopwatch.start();
    if (_controller.isPlaying) {
      _controller.onBeforeUpdateWorldTransforms?.call(_controller);
      _skeletonDrawable.update(_deltaTime);
      _controller.onAfterUpdateWorldTransforms?.call(_controller);
      markNeedsPaint();
      _scheduleFrame();
    }
    _firstUpdated = true;
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
    _controller._setCoordinateTransform(x + offsetX / scaleX, y + offsetY / scaleY, scaleX, scaleY);
  }

  @override
  void paint(PaintingContext context, Offset offset) {
    final Canvas canvas = context.canvas
      ..save()
      ..clipRect(offset & size);

    canvas.save();
    _setCanvasTransform(canvas, offset);

    if (_firstUpdated) {
      _controller.onBeforePaint?.call(_controller, canvas);
      final commands = _skeletonDrawable.renderFlutter();
      for (final cmd in commands) {
        final paint = _skeletonDrawable.atlasFlutter.atlasPagePaints[cmd.atlasPageIndex][cmd.blendMode]!;
        canvas.drawVertices(cmd.vertices, rendering.BlendMode.modulate, paint);
      }
      _controller.onAfterPaint?.call(_controller, canvas, commands);
    }

    canvas.restore();
  }
}
