///
/// Spine Runtimes License Agreement
/// Last updated April 5, 2025. Replaces all prior versions.
///
/// Copyright (c) 2013-2025, Esoteric Software LLC
///
/// Integration of the Spine Runtimes into software or otherwise creating
/// derivative works of the Spine Runtimes is permitted under the terms and
/// conditions of Section 2 of the Spine Editor License Agreement:
/// http://esotericsoftware.com/spine-editor-license
///
/// Otherwise, it is permitted to integrate the Spine Runtimes into software
/// or otherwise create derivative works of the Spine Runtimes (collectively,
/// "Products"), provided that each user of the Products must obtain their own
/// Spine Editor license and redistribution of the Products in any form must
/// include this license and copyright notice.
///
/// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
/// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
/// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
/// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
/// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
/// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
/// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
/// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
/// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
/// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
///

import 'dart:math';

import 'package:spine_flutter/spine_flutter.dart';
import 'package:flame/components.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';

class SpineComponent extends PositionComponent {
  final BoundsProvider _boundsProvider;
  final SkeletonDrawable _drawable;
  late final Bounds _bounds;
  final bool _ownsDrawable;

  SpineComponent(
    this._drawable, {
    bool ownsDrawable = true,
    BoundsProvider boundsProvider = const SetupPoseBounds(),
    super.position,
    super.scale,
    double super.angle = 0.0,
    Anchor super.anchor = Anchor.topLeft,
    super.children,
    super.priority,
  })  : _ownsDrawable = ownsDrawable,
        _boundsProvider = boundsProvider {
    _drawable.update(0);
    _bounds = _boundsProvider.computeBounds(_drawable);
    size = Vector2(_bounds.width, _bounds.height);
  }

  static Future<SpineComponent> fromAssets(
    String atlasFile,
    String skeletonFile, {
    AssetBundle? bundle,
    BoundsProvider boundsProvider = const SetupPoseBounds(),
    Vector2? position,
    Vector2? scale,
    double angle = 0.0,
    Anchor anchor = Anchor.topLeft,
    Iterable<Component>? children,
    int? priority,
  }) async {
    return SpineComponent(await SkeletonDrawable.fromAsset(atlasFile, skeletonFile, bundle: bundle),
        ownsDrawable: true,
        boundsProvider: boundsProvider,
        position: position,
        scale: scale,
        angle: angle,
        anchor: anchor,
        children: children,
        priority: priority);
  }

  void dispose() {
    if (_ownsDrawable) {
      _drawable.dispose();
    }
  }

  @override
  void update(double dt) {
    _drawable.update(dt);
  }

  @override
  void render(Canvas canvas) {
    canvas.save();
    canvas.translate(-_bounds.x, -_bounds.y);
    _drawable.renderToCanvas(canvas);
    canvas.restore();
  }

  get animationState => _drawable.animationState;

  get animationStateData => _drawable.animationStateData;

  get skeleton => _drawable.skeleton;
}

class SimpleFlameExample extends FlameGame {
  late final SpineComponent spineboy;

  @override
  Future<void> onLoad() async {
    // Load the Spineboy atlas and skeleton data from asset files
    // and create a SpineComponent from them, scaled down and
    // centered on the screen
    spineboy = await SpineComponent.fromAssets("assets/spineboy.atlas", "assets/spineboy-pro.json",
        scale: Vector2(0.4, 0.4), anchor: Anchor.center, position: Vector2(size.x / 2, size.y / 2));

    // Set the "walk" animation on track 0 in looping mode
    spineboy.animationState.setAnimationByName(0, "walk", true);
    await add(spineboy);
  }

  @override
  void onDetach() {
    // Dispose the native resources that have been loaded for spineboy.
    spineboy.dispose();
  }
}

class DragonExample extends FlameGame {
  late final Atlas cachedAtlas;
  late final SkeletonData cachedSkeletonData;
  late final SpineComponent dragon;

  @override
  Future<void> onLoad() async {
    cachedAtlas = await Atlas.fromAsset("assets/dragon.atlas");
    cachedSkeletonData =  await SkeletonData.fromAsset(cachedAtlas, "assets/dragon-ess.skel");
    final drawable = SkeletonDrawable(cachedAtlas, cachedSkeletonData, false);
    dragon = SpineComponent(
      drawable,
      scale: Vector2(0.4, 0.4),
      anchor: Anchor.center,
      position: Vector2(size.x / 2, size.y / 2 - 150),
    );
    // Set the "walk" animation on track 0 in looping mode
    dragon.animationState.setAnimationByName(0, "flying", true);
    await add(dragon);
  }

  @override
  void onDetach() {
    // Dispose the native resources that have been loaded for spineboy.
    dragon.dispose();
    cachedSkeletonData.dispose();
    cachedAtlas.dispose();
  }
}

class PreloadAndShareSpineDataExample extends FlameGame {
  late final SkeletonData cachedSkeletonData;
  late final Atlas cachedAtlas;
  late final List<SpineComponent> spineboys = [];

  @override
  Future<void> onLoad() async {
    // Pre-load the atlas and skeleton data once.
    cachedAtlas = await Atlas.fromAsset("assets/spineboy.atlas");
    cachedSkeletonData = await SkeletonData.fromAsset(cachedAtlas, "assets/spineboy-pro.skel");

    // Instantiate many spineboys from the pre-loaded data. Each SpineComponent
    // gets their own SkeletonDrawable copy derived from the cached data. The
    // SkeletonDrawable copies do not own the underlying skeleton data and atlas.
    final rng = Random();
    for (int i = 0; i < 100; i++) {
      final drawable = SkeletonDrawable(cachedAtlas, cachedSkeletonData, false);
      final scale = 0.1 + rng.nextDouble() * 0.2;
      final position = Vector2(rng.nextDouble() * size.x, rng.nextDouble() * size.y);
      final spineboy = SpineComponent(drawable, scale: Vector2(scale, scale), position: position);
      spineboy.animationState.setAnimationByName(0, "walk", true);
      spineboys.add(spineboy);
      await add(spineboy);
    }
  }

  @override
  void onDetach() {
    // Dispose the pre-loaded atlas and skeleton data when the game/scene is removed.
    cachedAtlas.dispose();
    cachedSkeletonData.dispose();

    // Dispose each spineboy and its internal SkeletonDrawable.
    for (final spineboy in spineboys) {
      spineboy.dispose();
    }
  }
}

class SpineFlameGameWidget extends StatelessWidget {
  final FlameGame game;

  const SpineFlameGameWidget(this.game, {super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(appBar: AppBar(title: const Text('Flame Integration')), body: GameWidget(game: game));
  }
}
