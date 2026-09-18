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

package com.esotericsoftware.spine.android

import androidx.compose.runtime.withFrameNanos
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.drawscope.ContentDrawScope
import androidx.compose.ui.graphics.drawscope.drawIntoCanvas
import androidx.compose.ui.graphics.nativeCanvas
import androidx.compose.ui.node.DrawModifierNode
import androidx.compose.ui.node.LayoutAwareModifierNode
import androidx.compose.ui.node.ModifierNodeElement
import androidx.compose.ui.node.invalidateDraw
import androidx.compose.ui.platform.InspectorInfo
import androidx.compose.ui.unit.IntSize
import androidx.core.graphics.withTranslation
import com.esotericsoftware.spine.android.bounds.Alignment
import com.esotericsoftware.spine.android.bounds.Bounds
import com.esotericsoftware.spine.android.bounds.BoundsProvider
import com.esotericsoftware.spine.android.bounds.ContentMode
import com.esotericsoftware.spine.android.compose.SpineLayout
import com.esotericsoftware.spine.android.compose.SpineState
import com.esotericsoftware.spine.android.compose.computeLayout
import kotlinx.coroutines.Job
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch

internal class SpineModifierElement(
    private val state: SpineState,
    private val drawable: AndroidSkeletonDrawable,
    private val alignment: Alignment,
    private val contentMode: ContentMode,
    private val boundsProvider: BoundsProvider,
    private val rendering: Boolean,
) : ModifierNodeElement<SpineModifierNode>() {

    override fun create(): SpineModifierNode =
        SpineModifierNode(state, drawable, alignment, contentMode, boundsProvider, rendering)

    override fun update(node: SpineModifierNode) {
        node.update(state, drawable, alignment, contentMode, boundsProvider, rendering)
    }

    override fun hashCode(): Int {
        var r = state.hashCode()
        r = 31 * r + drawable.hashCode()
        r = 31 * r + alignment.hashCode()
        r = 31 * r + contentMode.hashCode()
        r = 31 * r + boundsProvider.hashCode()
        r = 31 * r + rendering.hashCode()
        return r
    }

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is SpineModifierElement) return false
        return state === other.state &&
            drawable === other.drawable &&
            alignment == other.alignment &&
            contentMode == other.contentMode &&
            boundsProvider === other.boundsProvider &&
            rendering == other.rendering
    }

    override fun InspectorInfo.inspectableProperties() {
        name = "spine"
        properties["alignment"] = alignment
        properties["contentMode"] = contentMode
        properties["boundsProvider"] = boundsProvider
        properties["rendering"] = rendering
    }
}

internal class SpineModifierNode(
    private var state: SpineState,
    private var drawable: AndroidSkeletonDrawable,
    private var alignment: Alignment,
    private var contentMode: ContentMode,
    private var boundsProvider: BoundsProvider,
    private var rendering: Boolean,
) : Modifier.Node(), DrawModifierNode, LayoutAwareModifierNode {

    private val renderer = SkeletonRenderer()
    private var layout: SpineLayout = SpineLayout(0f, 0f, 1f, 1f, 0f, 0f)
    private var computedBounds: Bounds = Bounds()
    private var lastComputedBounds: Bounds = Bounds()
    private var lastDrawable: AndroidSkeletonDrawable? = null
    private var lastController: SpineController? = null
    private var lastBoundsProvider: BoundsProvider? = null
    private var lastSize: IntSize = IntSize.Zero
    private var lastAlignment: Alignment = Alignment.CENTER
    private var lastContentMode: ContentMode = ContentMode.FIT
    private var pendingDeltaSeconds: Float = 0f
    private var frameJob: Job? = null

    override fun onAttach() {
        if (rendering) startFrameLoop()
    }

    override fun onDetach() {
        stopFrameLoop()
    }

    fun update(
        state: SpineState,
        drawable: AndroidSkeletonDrawable,
        alignment: Alignment,
        contentMode: ContentMode,
        boundsProvider: BoundsProvider,
        rendering: Boolean,
    ) {
        val wasRendering = this.rendering
        this.state = state
        this.drawable = drawable
        this.alignment = alignment
        this.contentMode = contentMode
        this.boundsProvider = boundsProvider
        this.rendering = rendering
        recomputeLayout(lastSize)
        if (rendering != wasRendering) {
            if (rendering) startFrameLoop() else stopFrameLoop()
        }
        invalidateDraw()
    }

    private fun startFrameLoop() {
        frameJob?.cancel()
        // Drop any delta accumulated while the loop was stopped or before the controller was
        // initialized, so the first played frame doesn't fast-forward the animation.
        pendingDeltaSeconds = 0f
        frameJob = coroutineScope.launch {
            var lastNanos = 0L
            while (isActive) {
                withFrameNanos { now ->
                    val delta = if (lastNanos == 0L) 0f else (now - lastNanos) / 1_000_000_000f
                    lastNanos = now
                    pendingDeltaSeconds += delta
                    invalidateDraw()
                }
            }
        }
    }

    private fun stopFrameLoop() {
        frameJob?.cancel()
        frameJob = null
    }

    override fun onRemeasured(size: IntSize) {
        lastSize = size
        recomputeLayout(size)
    }

    private fun needsReinitialize(): Boolean =
        drawable !== lastDrawable || state.controller !== lastController || boundsProvider !== lastBoundsProvider

    private fun recomputeLayout(size: IntSize) {
        if (needsReinitialize()) {
            val controllerOrDrawableChanged = drawable !== lastDrawable || state.controller !== lastController
            // Bounds first, then init - matches SpineView.setSkeletonDrawable. A destructive
            // BoundsProvider (e.g. SkinAndAnimationBounds.clearTracks) must run before
            // onInitialized sets the animation, otherwise it wipes it and nothing plays.
            computedBounds = boundsProvider.computeBounds(drawable)
            // Initialize on controller change too: a new controller paired with an
            // already-initialized drawable would otherwise never get its drawable set.
            if (controllerOrDrawableChanged) state.controller.init(drawable)
            lastDrawable = drawable
            lastController = state.controller
            lastBoundsProvider = boundsProvider
        }
        if (size.width == 0 || size.height == 0) return
        // Skip recomputation if size, alignment, contentMode, and bounds haven't changed.
        // SpineLayout is a simple data class, but avoiding allocation helps in hot paths.
        if (size == lastSize && alignment == lastAlignment && contentMode == lastContentMode &&
            !needsReinitialize() && computedBounds == lastComputedBounds) {
            return
        }
        lastSize = size
        lastAlignment = alignment
        lastContentMode = contentMode
        val l = computeLayout(
            viewWidth = size.width.toFloat(),
            viewHeight = size.height.toFloat(),
            bounds = computedBounds,
            alignment = alignment,
            contentMode = contentMode,
        )
        layout = l
        lastComputedBounds = computedBounds.copy()
        // Degenerate bounds (no visible attachments) yield scale 0; skip the transform so we
        // don't feed offset/0 = Infinity into toSkeletonCoordinates.
        if (l.scaleX > 0f && l.scaleY > 0f) {
            state.controller.setCoordinateTransform(
                (l.x + l.offsetX / l.scaleX).toDouble(),
                (l.y + l.offsetY / l.scaleY).toDouble(),
                l.scaleX.toDouble(),
                l.scaleY.toDouble(),
            )
        }
    }

    override fun ContentDrawScope.draw() {
        val controller = state.controller
        // First draw (or a drawable/controller/boundsProvider swap via update) runs
        // computeBounds + init before painting, in case draw happens before onRemeasured.
        if (needsReinitialize()) recomputeLayout(lastSize)
        if (!controller.isInitialized || !rendering) {
            // Don't let delta accumulate across skipped frames; applying it in one chunk
            // later would fast-forward the animation.
            pendingDeltaSeconds = 0f
            drawContent()
            return
        }

        if (controller.isPlaying) {
            controller.callOnBeforeUpdateWorldTransforms()
            controller.drawable.update(pendingDeltaSeconds)
            controller.callOnAfterUpdateWorldTransforms()
        }
        pendingDeltaSeconds = 0f

        val l = layout
        drawIntoCanvas { canvas ->
            val nc = canvas.nativeCanvas
            nc.withTranslation(l.offsetX, l.offsetY) {
                nc.scale(l.scaleX, -l.scaleY)
                nc.translate(l.x, l.y)
                controller.callOnBeforePaint(nc)
                val commands = renderer.render(controller.skeleton)
                renderer.renderToCanvas(nc, commands)
                controller.callOnAfterPaint(nc, commands)
            }
        }
        drawContent()
    }
}
