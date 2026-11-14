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

import androidx.compose.foundation.layout.Spacer
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import com.esotericsoftware.spine.android.SpineModifierElement
import com.esotericsoftware.spine.android.bounds.Alignment
import com.esotericsoftware.spine.android.bounds.BoundsProvider
import com.esotericsoftware.spine.android.bounds.ContentMode
import com.esotericsoftware.spine.android.bounds.SetupPoseBounds

/**
 * Renders a Spine skeleton inside a Compose layout. Reuses [SpineState.controller] to drive
 * animation. The per-frame loop runs inside the internal `SpineModifierNode`'s `coroutineScope`,
 * triggering `invalidateDraw()` each frame so only the draw phase re-runs — no recomposition.
 *
 * Example usage:
 * ```
 * val controller = remember { SpineController { it.animationState.setAnimation(0, "walk", true) } }
 * val state = rememberSpineStateFromAssets("spineboy.atlas", "spineboy-pro.json", controller)
 * Spine(
 *     state = state,
 *     modifier = Modifier.fillMaxSize(),
 *     contentMode = ContentMode.FIT
 * )
 * ```
 *
 * @param state Holder returned by one of the `rememberSpineStateFrom*` helpers.
 * @param modifier Standard Compose modifier. Apply size constraints here (e.g. `Modifier.fillMaxSize()`).
 *   The composable has no intrinsic size; in an unbounded slot it measures to 0.
 * @param alignment How the skeleton is aligned inside the layout box. Defaults to [Alignment.CENTER].
 * @param contentMode How the skeleton scales to fit the layout. Defaults to [ContentMode.FIT].
 * @param boundsProvider Computes the on-screen bounds of the skeleton. Defaults to [SetupPoseBounds].
 * @param rendering When `false`, skip animation update + draw to save CPU/GPU. Defaults to `true`.
 * @param loading Content shown while the skeleton is loading. Defaults to empty (renders nothing).
 * @param error Content shown if loading failed, receiving the cause. Defaults to empty (renders
 *   nothing). Callers needing finer control can read [SpineState.load] directly instead.
 */
@Composable
fun Spine(
    state: SpineState,
    modifier: Modifier = Modifier,
    alignment: Alignment = Alignment.CENTER,
    contentMode: ContentMode = ContentMode.FIT,
    boundsProvider: BoundsProvider = remember { SetupPoseBounds() },
    rendering: Boolean = true,
    loading: @Composable () -> Unit = {},
    error: @Composable (cause: Throwable) -> Unit = {},
) {
    when (val load = state.load) {
        is SpineLoad.Loading -> loading()
        is SpineLoad.Error -> error(load.cause)
        is SpineLoad.Success -> Spacer(
            modifier = modifier
                .spineSoftwareLayerIfNeeded()
                .then(
                    SpineModifierElement(
                        state = state,
                        drawable = load.drawable,
                        alignment = alignment,
                        contentMode = contentMode,
                        boundsProvider = boundsProvider,
                        rendering = rendering
                    )
                ),
        )
    }
}
