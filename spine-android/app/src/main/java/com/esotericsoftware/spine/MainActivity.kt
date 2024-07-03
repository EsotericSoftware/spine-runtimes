package com.esotericsoftware.spine

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.viewinterop.AndroidView
import com.esotericsoftware.spine.android.SpineController
import com.esotericsoftware.spine.android.SpineView
import com.esotericsoftware.spine.ui.theme.SpineAndroidExamplesTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            AppContent()
        }
    }
}

@Composable
fun AppContent() {
    SpineAndroidExamplesTheme {
        Surface(
            modifier = Modifier.fillMaxSize(),
            color = MaterialTheme.colorScheme.background
        ) {
            Box {
                SpineViewComposable()
            }
        }
    }
}

@Composable
fun SpineViewComposable(modifier: Modifier = Modifier.fillMaxSize()) {
    AndroidView(
        factory = { ctx ->
            SpineView(ctx).apply {
                loadFromAsset(
                    "spineboy.atlas",
                    "spineboy-pro.json",
                    SpineController {
                        it.animationState.setAnimation(0, "walk", true)
                    }
                )
            }
        },
        modifier = modifier
    )
}
