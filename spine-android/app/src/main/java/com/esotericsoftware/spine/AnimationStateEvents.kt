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

import android.util.Log
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.rounded.ArrowBack
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.viewinterop.AndroidView
import androidx.navigation.NavHostController
import com.badlogic.gdx.graphics.Color
import com.esotericsoftware.spine.android.SpineController
import com.esotericsoftware.spine.android.SpineView

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AnimationState(nav: NavHostController) {

    val TAG = "AnimationState"

    val controller = remember {
        SpineController { controller ->
            controller.skeleton.setScaleX(0.5f)
            controller.skeleton.setScaleY(0.5f)

            controller.skeleton.findSlot("gun")?.color = Color(1f, 0f, 0f, 1f)

            controller.animationStateData.setDefaultMix(0.2f)
            controller.animationState.setAnimation(0, "walk", true).setListener(object : AnimationState.AnimationStateListener {
                override fun start(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Walk animation event start")
                }

                override fun interrupt(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Walk animation event interrupt")
                }

                override fun end(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Walk animation event end")
                }

                override fun dispose(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Walk animation event dispose")
                }

                override fun complete(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Walk animation event complete")
                }

                override fun event(entry: AnimationState.TrackEntry?, event: Event?) {
                    Log.d(TAG, "Walk animation event event")
                }
            })
            controller.animationState.addAnimation(0, "jump", false, 2f)
            controller.animationState.addAnimation(0, "run", true, 0f).setListener(object : AnimationState.AnimationStateListener {
                override fun start(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Run animation event start")
                }

                override fun interrupt(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Run animation event interrupt")
                }

                override fun end(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Run animation event end")
                }

                override fun dispose(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Run animation event dispose")
                }

                override fun complete(entry: AnimationState.TrackEntry?) {
                    Log.d(TAG, "Run animation event complete")
                }

                override fun event(entry: AnimationState.TrackEntry?, event: Event?) {
                    Log.d(TAG, "Run animation event event")
                }
            })

            controller.animationState.addListener(object : AnimationState.AnimationStateListener {
                override fun start(entry: AnimationState.TrackEntry?) {}

                override fun interrupt(entry: AnimationState.TrackEntry?) {}

                override fun end(entry: AnimationState.TrackEntry?) {}

                override fun dispose(entry: AnimationState.TrackEntry?) {}

                override fun complete(entry: AnimationState.TrackEntry?) {}

                override fun event(entry: AnimationState.TrackEntry?, event: Event?) {
                    if (event != null) {
                        Log.d(TAG, "User event: { name: ${event.data.name}, intValue: ${event.int}, floatValue: ${event.float}, stringValue: ${event.string} }")
                    }
                }
            })
            Log.d(TAG, "Current: ${controller.animationState.getCurrent(0)?.getAnimation()?.getName()}");
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = Destination.AnimationStateEvents.title) },
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
        Column(
            modifier = Modifier.padding(paddingValues),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Text("See output in console!")
            AndroidView(
                factory = { context ->
                    SpineView.loadFromAssets(
                        "spineboy.atlas",
                        "spineboy-pro.json",
                        context,
                        controller
                    )
                }
            )
        }
    }
}
