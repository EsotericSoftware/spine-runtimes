package com.esotericsoftware.spine

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
import androidx.compose.ui.Modifier
import androidx.compose.ui.viewinterop.AndroidView
import androidx.navigation.NavHostController
import com.esotericsoftware.spine.android.DebugRenderer
import com.esotericsoftware.spine.android.SpineController
import com.esotericsoftware.spine.android.SpineView

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DebugRendering(nav: NavHostController) {

    val debugRenderer = remember {
        DebugRenderer()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = Destination.DebugRendering.title) },
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
        AndroidView(
            factory = { ctx ->
                SpineView(ctx).apply {
                    loadFromAsset(
                        "spineboy.atlas",
                        "spineboy-pro.json",
                        SpineController.Builder()
                            .setOnInitialized {
                                it.animationState.setAnimation(0, "walk", true)
                            }
                            .setOnAfterPaint { controller, canvas, commands ->
                                debugRenderer.render(controller.drawable, canvas, commands)
                            }
                            .build()
                    )
                }
            },
            modifier = Modifier.padding(paddingValues)
        )
    }
}
