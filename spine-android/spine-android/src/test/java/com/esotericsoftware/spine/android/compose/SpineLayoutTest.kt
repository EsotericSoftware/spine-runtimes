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
import org.junit.Assert.assertEquals
import org.junit.Test

class SpineLayoutTest {

    @Test fun fit_center_uniformlyScalesToSmallerDimension() {
        val bounds = Bounds(-50.0, -100.0, 100.0, 200.0) // 100x200 content centered
        val layout = computeLayout(
            viewWidth = 200f, viewHeight = 200f,
            bounds = bounds,
            alignment = Alignment.CENTER,
            contentMode = ContentMode.FIT,
        )
        // FIT picks min(200/100, 200/200) = 1.0
        assertEquals(1.0f, layout.scaleX, 0.0001f)
        assertEquals(1.0f, layout.scaleY, 0.0001f)
        assertEquals(100f, layout.offsetX, 0.0001f)
        assertEquals(100f, layout.offsetY, 0.0001f)
        assertEquals(0f, layout.x, 0.0001f)
        assertEquals(0f, layout.y, 0.0001f)
    }

    @Test fun fill_topLeft_picksLargerScale() {
        val bounds = Bounds(0.0, 0.0, 100.0, 50.0)
        val layout = computeLayout(
            viewWidth = 200f, viewHeight = 200f,
            bounds = bounds,
            alignment = Alignment.TOP_LEFT,
            contentMode = ContentMode.FILL,
        )
        // FILL picks max(200/100, 200/50) = 4.0
        assertEquals(4.0f, layout.scaleX, 0.0001f)
        assertEquals(4.0f, layout.scaleY, 0.0001f)
    }

    @Test fun zeroSize_returnsZeroScale() {
        val bounds = Bounds(0.0, 0.0, 100.0, 100.0)
        val layout = computeLayout(
            viewWidth = 0f, viewHeight = 0f,
            bounds = bounds,
            alignment = Alignment.CENTER,
            contentMode = ContentMode.FIT,
        )
        assertEquals(0f, layout.scaleX, 0.0001f)
    }
}
