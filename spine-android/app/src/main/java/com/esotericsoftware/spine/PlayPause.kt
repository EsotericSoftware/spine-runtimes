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

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.rounded.ArrowBack
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.viewinterop.AndroidView
import androidx.navigation.NavHostController
import com.esotericsoftware.spine.android.SpineController
import com.esotericsoftware.spine.android.SpineView
import com.esotericsoftware.spine.android.bounds.SkinAndAnimationBounds

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PlayPause(
    nav: NavHostController
) {
    val controller = remember {
        SpineController { controller ->
            controller.animationState.setAnimation(0, "flying", true)
        }
    }

    val isPlaying = remember { mutableStateOf(controller.isPlaying) }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = Destination.PlayPause.title) },
                navigationIcon = {
                    IconButton({ nav.navigateUp() }) {
                        Icon(
                            Icons.Rounded.ArrowBack,
                            null,
                        )
                    }
                },
                actions = {
                    Button(onClick = {
                        if (controller.isPlaying) controller.pause() else controller.resume()
                        isPlaying.value = controller.isPlaying
                    }) {
                        Text(text = if (isPlaying.value) "Pause" else "Play")
                    }
                }
            )
        }
    ) { paddingValues ->

        AndroidView(
            factory = { ctx ->
                SpineView.Builder(ctx, controller)
                    .setLoadFromAssets("dragon.atlas", "dragon-ess.skel")
                    .setBoundsProvider(SkinAndAnimationBounds("flying"))
                    .build()
            },
            modifier = Modifier.padding(paddingValues)
        )
    }
}
