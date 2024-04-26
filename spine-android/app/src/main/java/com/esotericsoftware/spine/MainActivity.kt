package com.esotericsoftware.spine

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.ui.Modifier
import com.esotericsoftware.spine.ui.theme.SpineAndroidExamplesTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.painter.Painter
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.viewinterop.AndroidView
import com.esotericsoftware.spine.android.SpineView

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
    val context = LocalContext.current
    AndroidView(
        factory = { ctx ->
            SpineView(ctx).apply {
            }
        },
        modifier = modifier
    )
}
