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
import androidx.compose.ui.Modifier
import androidx.compose.ui.viewinterop.AndroidView
import androidx.navigation.NavHostController
import com.esotericsoftware.spine.android.SpineController
import com.esotericsoftware.spine.android.SpineView

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SimpleAnimation(nav: NavHostController) {
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = Destination.SimpleAnimation.title) },
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
            factory = { context ->
                SpineView.loadFromAssets(
                    "spineboy.atlas",
                    "spineboy-pro.json",
                    context,
                    SpineController.Builder()
                        .setOnInitialized {
                            it.animationState.setAnimation(0, "walk", true)
                        }
                        .build()
                )
            },
            modifier = Modifier.padding(paddingValues)
        )
    }
}
