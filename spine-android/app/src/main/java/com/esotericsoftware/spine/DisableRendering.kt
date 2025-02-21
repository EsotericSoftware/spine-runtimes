/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine

import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.rounded.ArrowBack
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableFloatStateOf
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clipToBounds
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.layout.onGloballyPositioned
import androidx.compose.ui.layout.positionInParent
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.IntOffset
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.toSize
import androidx.compose.ui.viewinterop.AndroidView
import androidx.navigation.NavHostController
import com.esotericsoftware.spine.android.AndroidSkeletonDrawable
import com.esotericsoftware.spine.android.AndroidTextureAtlas
import com.esotericsoftware.spine.android.SpineController
import com.esotericsoftware.spine.android.SpineView
import com.esotericsoftware.spine.android.utils.SkeletonDataUtils
import kotlin.random.Random

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DisableRendering(nav: NavHostController) {
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = Destination.DisableRendering.title) },
                navigationIcon = {
                    IconButton({ nav.navigateUp() }) {
                        Icon(
                            Icons.Rounded.ArrowBack,
                            null,
                        )
                    }
                }
            )
        }
    ) { paddingValues ->

        val visibleSpineBoys = remember {
            mutableStateListOf<Int>()
        }

        Column(
            modifier = Modifier
                .padding(paddingValues)
                .padding()
                .onGloballyPositioned { coordinates ->
                    print(coordinates.size.toSize())
                }
        ) {
            Column(
                modifier = Modifier
                    .padding(8.dp)
            ) {
                Text("There are ${visibleSpineBoys.count()} spine boys visible. Scroll around to find the odd one out...")
                Text("Rendering is disabled when the spine view moves out of the viewport, preserving CPU/GPU resources.", color = Color.Gray)
            }
            SpineBoys(visibleSpineBoys)
        }
    }
}

@Composable
fun SpineBoys(visibleSpineBoys: MutableList<Int>) {
    var boxSize by remember { mutableStateOf(Size.Zero) }
    val offsetX = remember { mutableFloatStateOf(0f) }
    val offsetY = remember { mutableFloatStateOf(0f) }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .clipToBounds()
            .onGloballyPositioned { coordinates ->
                boxSize = coordinates.size.toSize()
            }
            .pointerInput(Unit) {
                detectDragGestures { change, dragAmount ->
                    change.consume()
                    offsetX.floatValue += dragAmount.x
                    offsetY.floatValue += dragAmount.y
                }
            }
    ) {
        if (boxSize != Size.Zero) {
            val contentSize = boxSize * 4f

            val context = LocalContext.current
            val cachedAtlas =
                remember { AndroidTextureAtlas.fromAsset("spineboy.atlas", context) }
            val cachedSkeletonData = remember {
                SkeletonDataUtils.fromAsset(
                    cachedAtlas,
                    "spineboy-pro.json",
                    context
                )
            }

            val spineboys = remember {
                val rng = Random(System.currentTimeMillis())
                List(100) { index ->
                    val scale = 0.1f + rng.nextFloat() * 0.2f
                    val position = Offset(
                        rng.nextFloat() * contentSize.width,
                        rng.nextFloat() * contentSize.height
                    )
                    SpineBoyData(
                        index,
                        scale,
                        position,
                        if (index == 99) "hoverboard" else "walk"
                    )
                }
            }

            spineboys.forEach { spineBoyData ->

                val isSpineBoyVisible = remember { mutableStateOf(false) }

                Box(modifier = Modifier
                    .offset {
                        IntOffset(
                            (-(contentSize.width / 2) + spineBoyData.position.x + offsetX.floatValue.toInt()).toInt(),
                            (-(contentSize.height / 2) + spineBoyData.position.y + offsetY.floatValue.toInt()).toInt(),
                        )
                    }
                    .size(
                        (boxSize.width * spineBoyData.scale).dp,
                        (boxSize.height * spineBoyData.scale).dp
                    )
                    .onGloballyPositioned { coordinates ->
                        val positionInRoot = coordinates.positionInParent()
                        val size = coordinates.size.toSize()

                        val isInViewport = positionInRoot.x < boxSize.width &&
                            positionInRoot.x + size.width > 0 &&
                            positionInRoot.y < boxSize.height &&
                            positionInRoot.y + size.height > 0

                        isSpineBoyVisible.value = isInViewport

                        val visibleSpineBoysAsSet = visibleSpineBoys.toMutableSet()
                        if (isInViewport) {
                            visibleSpineBoysAsSet.add(spineBoyData.id)
                        } else {
                            visibleSpineBoysAsSet.remove(spineBoyData.id)
                        }
                        visibleSpineBoys.clear()
                        visibleSpineBoys.addAll(visibleSpineBoysAsSet)
                    }
                ) {
                    AndroidView(
                        factory = { ctx ->
                            SpineView.loadFromDrawable(
                                AndroidSkeletonDrawable(cachedAtlas, cachedSkeletonData),
                                ctx,
                                SpineController {
                                    it.animationState.setAnimation(
                                        0,
                                        spineBoyData.animation,
                                        true
                                    )
                                }
                            ).apply {
                                isRendering = false
                            }
                        },
                        update = { view ->
                            view.isRendering = isSpineBoyVisible.value
                        }
                    )
                }
            }
        }
    }
}

data class SpineBoyData(
    val id: Int,
    val scale: Float,
    val position: Offset,
    val animation: String
)
