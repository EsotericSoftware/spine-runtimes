/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.android.compose

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.Stable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.platform.LocalContext
import com.esotericsoftware.spine.android.AndroidSkeletonDrawable
import com.esotericsoftware.spine.android.SpineController
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.File
import java.net.URL

/**
 * The load state of a [SpineState]'s skeleton: [Loading] until the first load resolves, then
 * [Success] or [Error]. A reload (key change) resets to [Loading].
 * */
sealed interface SpineLoad {
    data object Loading : SpineLoad
    data class Success(val drawable: AndroidSkeletonDrawable) : SpineLoad
    data class Error(val cause: Throwable) : SpineLoad
}

/**
 * Holds the [SpineLoad] state and the [SpineController] for a Compose [Spine] call.
 */
@Stable
class SpineState internal constructor(
    val controller: SpineController,
) {
    /**
     * Current load state.
     */
    var load: SpineLoad by mutableStateOf(SpineLoad.Loading)
        private set

    /**
     * The loaded drawable, or `null` while [Loading][SpineLoad.Loading] or on
     * [Error][SpineLoad.Error].
     */
    val drawable: AndroidSkeletonDrawable? get() = (load as? SpineLoad.Success)?.drawable

    /**
     * The cause of the last failed load, or `null` otherwise. Lets callers render a fallback
     * instead of crashing the composition
     */
    val error: Throwable? get() = (load as? SpineLoad.Error)?.cause

    /**
     * Resets to [Loading][SpineLoad.Loading] at the start of an async (re)load, so a stale
     * drawable doesn't linger on screen while the new one loads.
     */
    internal fun setLoading() {
        load = SpineLoad.Loading
    }

    /**
     * Publishes a loaded drawable. The [controller] is *not* initialized here: the
     * `SpineModifierNode` initializes it only after computing bounds, so a destructive
     * [com.esotericsoftware.spine.android.bounds.BoundsProvider] (e.g. `SkinAndAnimationBounds`,
     * which calls `clearTracks()`) cannot wipe the animation set in `onInitialized`. Mirrors
     * `SpineView.setSkeletonDrawable`: computeBounds first, then `controller.init`.
     */
    internal fun set(drawable: AndroidSkeletonDrawable) {
        load = SpineLoad.Success(drawable)
    }

    internal fun setError(error: Throwable) {
        load = SpineLoad.Error(error)
    }
}

/**
 * Loads a skeleton from app assets. Reloads when [atlasFileName] or [skeletonFileName] change.
 *
 * Note: Files are loaded from the `assets` directory. The [controller] is not modified by this
 * function; set up animations and callbacks in the controller's initializer.
 *
 * @param atlasFileName The atlas file name (e.g., "spineboy.atlas")
 * @param skeletonFileName The skeleton JSON file name (e.g., "spineboy-pro.json")
 * @param controller The [SpineController] that will control the loaded skeleton
 * @return A [SpineState] that tracks the loading progress and holds the loaded drawable
 */
@Composable
fun rememberSpineStateFromAssets(
    atlasFileName: String,
    skeletonFileName: String,
    controller: SpineController,
): SpineState {
    val context = LocalContext.current
    val state = remember(controller) { SpineState(controller) }
    LaunchedEffect(atlasFileName, skeletonFileName, controller) {
        state.load { AndroidSkeletonDrawable.fromAsset(atlasFileName, skeletonFileName, context) }
    }
    return state
}

/**
 * Loads a skeleton from local files. Reloads when either file path changes.
 *
 * @param atlasPath Absolute path to the atlas file
 * @param skeletonPath Absolute path to the skeleton JSON file
 * @param controller The [SpineController] that will control the loaded skeleton
 * @return A [SpineState] that tracks the loading progress and holds the loaded drawable
 */
@Composable
fun rememberSpineStateFromFile(
    atlasPath: String,
    skeletonPath: String,
    controller: SpineController,
): SpineState {
    val state = remember(controller) { SpineState(controller) }
    LaunchedEffect(atlasPath, skeletonPath, controller) {
        state.load { AndroidSkeletonDrawable.fromFile(File(atlasPath), File(skeletonPath)) }
    }
    return state
}

/**
 * Loads a skeleton over HTTP. Downloads into [targetDirectory] on first load, then loads from cache.
 *
 * Note: Requires INTERNET permission. Files are downloaded on IO dispatcher.
 *
 * @param atlasUrl URL of the atlas file
 * @param skeletonUrl URL of the skeleton JSON file
 * @param targetDirectory Directory to cache downloaded files
 * @param controller The [SpineController] that will control the loaded skeleton
 * @return A [SpineState] that tracks the loading progress and holds the loaded drawable
 */
@Composable
fun rememberSpineStateFromHttp(
    atlasUrl: String,
    skeletonUrl: String,
    targetDirectory: String,
    controller: SpineController,
): SpineState {
    val state = remember(controller) { SpineState(controller) }
    LaunchedEffect(atlasUrl, skeletonUrl, targetDirectory, controller) {
        state.load { AndroidSkeletonDrawable.fromHttp(URL(atlasUrl), URL(skeletonUrl), File(targetDirectory)) }
    }
    return state
}

/**
 * Wraps an already-loaded [AndroidSkeletonDrawable] for use with the [Spine] composable.
 *
 * Use this when you've loaded the skeleton data through other means (e.g., from a custom cache
 * or preloaded at app startup).
 *
 * @param drawable The preloaded skeleton drawable
 * @param controller The [SpineController] that will control the skeleton
 * @return A [SpineState] that immediately transitions to Success state with the provided drawable
 */
@Composable
fun rememberSpineStateFromDrawable(
    drawable: AndroidSkeletonDrawable,
    controller: SpineController,
): SpineState {
    val state = remember(controller) { SpineState(controller) }
    LaunchedEffect(drawable, controller) {
        state.set(drawable)
    }
    return state
}

/**
 * Runs [loader] on [Dispatchers.IO] and publishes the result, capturing any throw as [error]
 * instead of letting it crash the composition. A [kotlinx.coroutines.CancellationException]
 * (recomposition cancelling the [LaunchedEffect]) is rethrown so structured concurrency works.
 */
private suspend fun SpineState.load(loader: () -> AndroidSkeletonDrawable) {
    setLoading()
    try {
        val drawable = withContext(Dispatchers.IO) { loader() }
        set(drawable)
    } catch (e: CancellationException) {
        throw e
    } catch (e: Exception) {
        setError(e)
    }
}
