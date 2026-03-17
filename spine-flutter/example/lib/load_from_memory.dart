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

import 'dart:typed_data';
import 'package:spine_flutter/spine_flutter.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

/// This example demonstrates loading Spine skeleton data from memory using the
/// fromMemory methods. This is useful when you want to:
/// - Load data from custom storage (e.g., encrypted assets, databases)
/// - Implement custom caching strategies
/// - Download and cache assets at runtime
/// - Pre-process assets before loading
///
/// The example loads all files (atlas, skeleton, images) into memory first,
/// then uses the fromMemory API to create a SpineWidget.
class LoadFromMemory extends StatefulWidget {
  const LoadFromMemory({super.key});

  @override
  State<LoadFromMemory> createState() => _LoadFromMemoryState();
}

class _LoadFromMemoryState extends State<LoadFromMemory> {
  // In-memory cache of all loaded files
  final Map<String, Uint8List> _fileCache = {};
  bool _isLoading = true;
  String _loadingStatus = 'Loading assets into memory...';

  @override
  void initState() {
    super.initState();
    _loadAssetsIntoMemory();
  }

  Future<void> _loadAssetsIntoMemory() async {
    try {
      // Step 1: Load atlas file into memory
      setState(() => _loadingStatus = 'Loading atlas file...');
      final atlasBytes = await rootBundle.load('assets/spineboy.atlas');
      _fileCache['assets/spineboy.atlas'] = atlasBytes.buffer.asUint8List();

      // Step 2: Load skeleton file into memory
      setState(() => _loadingStatus = 'Loading skeleton file...');
      final skelBytes = await rootBundle.load('assets/spineboy-pro.skel');
      _fileCache['assets/spineboy-pro.skel'] = skelBytes.buffer.asUint8List();

      // Step 3: Load image file(s) into memory
      setState(() => _loadingStatus = 'Loading image files...');
      final imageBytes = await rootBundle.load('assets/spineboy.png');
      _fileCache['assets/spineboy.png'] = imageBytes.buffer.asUint8List();

      // All files loaded into memory!
      setState(() {
        _loadingStatus = 'All assets loaded into memory (${_fileCache.length} files, ${_getTotalSize()} bytes)';
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _loadingStatus = 'Error loading assets: $e';
        _isLoading = false;
      });
    }
  }

  int _getTotalSize() {
    return _fileCache.values.fold(0, (sum, bytes) => sum + bytes.length);
  }

  // Custom file loader that returns data from our in-memory cache
  Future<Uint8List> _loadFromCache(String filename) async {
    final data = _fileCache[filename];
    if (data == null) {
      throw Exception('File not found in cache: $filename');
    }
    return data;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Load From Memory')),
      body: _isLoading
          ? Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const CircularProgressIndicator(),
                  const SizedBox(height: 16),
                  Text(_loadingStatus),
                ],
              ),
            )
          : Column(
              children: [
                Container(
                  padding: const EdgeInsets.all(16),
                  color: Colors.blue.shade50,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Files in Memory Cache:',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: 8),
                      ..._fileCache.entries.map((entry) => Text(
                            '  ${entry.key}: ${entry.value.length} bytes',
                            style: Theme.of(context).textTheme.bodySmall,
                          )),
                      const SizedBox(height: 8),
                      Text(
                        'Total: ${_getTotalSize()} bytes',
                        style: Theme.of(context).textTheme.titleSmall,
                      ),
                    ],
                  ),
                ),
                Expanded(
                  child: SpineWidget.fromMemory(
                    'assets/spineboy.atlas',
                    'assets/spineboy-pro.skel',
                    _loadFromCache,
                    SpineWidgetController(
                      onInitialized: (controller) {
                        controller.animationState.setAnimation(0, 'walk', true);
                      },
                    ),
                  ),
                ),
              ],
            ),
    );
  }
}
