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

import com.esotericsoftware.spine.android.bounds.Alignment
import com.esotericsoftware.spine.android.bounds.Bounds
import com.esotericsoftware.spine.android.bounds.ContentMode
import kotlin.math.max
import kotlin.math.min

internal data class SpineLayout(
    val offsetX: Float,
    val offsetY: Float,
    val scaleX: Float,
    val scaleY: Float,
    val x: Float,
    val y: Float,
)

/** Compose equivalent of `SpineView.updateCanvasTransform` — keep the math in sync. Unlike the
 * view, degenerate bounds yield scale 0 here; the caller skips the transform in that case. */
internal fun computeLayout(
    viewWidth: Float,
    viewHeight: Float,
    bounds: Bounds,
    alignment: Alignment,
    contentMode: ContentMode,
): SpineLayout {
    val bw = bounds.width
    val bh = bounds.height
    val scale = when {
        bw <= 0.0 || bh <= 0.0 || viewWidth <= 0f || viewHeight <= 0f -> 0f
        contentMode == ContentMode.FIT -> min(viewWidth / bw, viewHeight / bh).toFloat()
        else /* FILL */ -> max(viewWidth / bw, viewHeight / bh).toFloat()
    }
    val x = (-bounds.x - bw / 2.0 - alignment.x * bw / 2.0).toFloat()
    val y = (-bounds.y - bh / 2.0 - alignment.y * bh / 2.0).toFloat()
    val offsetX = (viewWidth / 2.0 + alignment.x * viewWidth / 2.0).toFloat()
    val offsetY = (viewHeight / 2.0 + alignment.y * viewHeight / 2.0).toFloat()
    return SpineLayout(offsetX, offsetY, scale, scale, x, y)
}
