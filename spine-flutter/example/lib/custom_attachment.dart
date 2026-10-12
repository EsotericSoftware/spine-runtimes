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
// or otherwise creating derivative works of the Spine Runtimes (collectively,
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

import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:spine_flutter/spine_flutter.dart';

/// Demonstrates animating a copied mesh attachment with a standalone Flutter image.
class CustomAttachment extends StatefulWidget {
  const CustomAttachment({super.key});

  @override
  State<CustomAttachment> createState() => _CustomAttachmentState();
}

class _CustomAttachmentState extends State<CustomAttachment> {
  late final SpineWidgetController _controller;
  SkeletonDrawableFlutter? _drawable;
  Skin? _originalSkin;
  Skin? _customSkin;
  Object? _error;
  bool _showCustomAttachment = true;

  @override
  void initState() {
    super.initState();
    _controller = SpineWidgetController(
      onInitialized: (controller) {
        controller.animationState.setAnimation(0, 'hoverboard', true);
        _applySelectedSkin();
      },
    );
    _loadDrawable();
  }

  Future<ui.Image> _loadImage(String assetPath) async {
    final data = await rootBundle.load(assetPath);
    final bytes = data.buffer.asUint8List(data.offsetInBytes, data.lengthInBytes);
    final codec = await ui.instantiateImageCodec(bytes);
    try {
      return (await codec.getNextFrame()).image;
    } finally {
      codec.dispose();
    }
  }

  Future<void> _loadDrawable() async {
    SkeletonDrawableFlutter? drawable;
    Skin? customSkin;
    MeshAttachment? customAttachment;
    ui.Image? image;
    try {
      drawable = await SkeletonDrawableFlutter.fromAsset('assets/spineboy.atlas', 'assets/spineboy-pro.skel');
      final originalSkin = drawable.skeleton.skin;
      image = await _loadImage('assets/custom-hoverboard.png');
      final customRegion = drawable.atlasFlutter.addRegion('custom-hoverboard', image);

      final boardSlot = drawable.skeleton.findSlot('hoverboard-board');
      final originalAttachment = drawable.skeleton.getAttachment('hoverboard-board', 'hoverboard-board');
      if (boardSlot == null || originalAttachment is! MeshAttachment) {
        throw StateError('The Spineboy hoverboard mesh attachment could not be found.');
      }

      // Skeleton data attachments may be shared, so customize a copy rather than the original attachment.
      customAttachment = originalAttachment.copy() as MeshAttachment;
      customAttachment.setRegion(customRegion);

      // The hoverboard animation keys this attachment. A skin lets that timeline resolve the custom copy instead of
      // replacing a direct slot assignment with the original attachment.
      customSkin = Skin('custom-hoverboard');
      customSkin.setAttachment(boardSlot.data.index, 'hoverboard-board', customAttachment);
      customAttachment = null; // The skin now owns the copied attachment.

      if (!mounted) {
        drawable.dispose();
        customSkin.dispose();
        return;
      }

      setState(() {
        _drawable = drawable;
        _originalSkin = originalSkin;
        _customSkin = customSkin;
      });
    } catch (error) {
      drawable?.dispose();
      customSkin?.dispose();
      customAttachment?.dispose();
      if (mounted) setState(() => _error = error);
    } finally {
      image?.dispose();
    }
  }

  void _applySelectedSkin() {
    final drawable = _drawable;
    final customSkin = _customSkin;
    if (drawable == null || customSkin == null) return;

    drawable.skeleton
      ..setSkin2(_showCustomAttachment ? customSkin : _originalSkin)
      ..setupPoseSlots();
    drawable.animationState.apply(drawable.skeleton);
  }

  void _toggleAttachment() {
    _showCustomAttachment = !_showCustomAttachment;
    _applySelectedSkin();
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    final error = _error;
    final drawable = _drawable;
    if (error != null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Custom Attachment')),
        body: Center(child: Text('Could not load the example: $error')),
      );
    }
    if (drawable == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Custom Attachment')),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Custom Attachment')),
      body: Column(
        children: [
          Expanded(
            child: SpineWidget.fromDrawable(
              drawable,
              _controller,
              boundsProvider: SkinAndAnimationBounds(animation: 'hoverboard'),
            ),
          ),
          const Padding(
            padding: EdgeInsets.all(16),
            child: Text('The cyan hoverboard is a standalone PNG mapped to the animated mesh.'),
          ),
          Padding(
            padding: const EdgeInsets.only(bottom: 16),
            child: ElevatedButton(
              onPressed: _toggleAttachment,
              child: Text(_showCustomAttachment ? 'Show original board' : 'Show custom board'),
            ),
          ),
        ],
      ),
    );
  }

  @override
  void dispose() {
    // Dispose the skeleton before the custom skin and the copied attachment owned by that skin.
    _drawable?.dispose();
    _customSkin?.dispose();
    super.dispose();
  }
}
