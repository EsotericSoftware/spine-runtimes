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
        SpineController {
            it.animationState.setAnimation(0, "flying", true)
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
                SpineView.Builder(ctx)
                    .setBoundsProvider(SkinAndAnimationBounds("flying"))
                    .build()
                    .apply {
                        loadFromAsset(
                            "dragon.atlas",
                            "dragon-ess.skel",
                            controller
                        )
                    }
            },
            modifier = Modifier.padding(paddingValues)
        )
    }
}
