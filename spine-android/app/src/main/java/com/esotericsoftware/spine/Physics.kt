package com.esotericsoftware.spine

import android.graphics.Point
import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
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
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.layout.onGloballyPositioned
import androidx.compose.ui.viewinterop.AndroidView
import androidx.navigation.NavHostController
import com.esotericsoftware.spine.android.SpineController
import com.esotericsoftware.spine.android.SpineView

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun Physics(nav: NavHostController) {

    val containerHeight = remember { mutableIntStateOf(0) }
    val dragPosition = remember { mutableStateOf(Point(0, 0)) }

    val mousePosition = remember { mutableStateOf<Point?>(null) }
    val lastMousePosition = remember { mutableStateOf<Point?>(null) }

    val controller = remember {
        SpineController.Builder()
            .setOnInitialized { controller ->
                controller.animationState.setAnimation(0, "eyeblink-long", true)
                controller.animationState.setAnimation(1, "wings-and-feet", true)
            }
            .setOnAfterUpdateWorldTransforms { controller ->
                val lastMousePositionValue = lastMousePosition.value
                if (lastMousePositionValue == null) {
                    lastMousePosition.value = mousePosition.value
                    return@setOnAfterUpdateWorldTransforms
                }
                val mousePositionValue = mousePosition.value ?: return@setOnAfterUpdateWorldTransforms

                val dx = mousePositionValue.x - lastMousePositionValue.x
                val dy = mousePositionValue.y - lastMousePositionValue.y
                val position = Point(
                    controller.skeleton.x.toInt(),
                    controller.skeleton.y.toInt()
                )
                position.x += dx
                position.y += dy
                controller.skeleton.setPosition(position.x.toFloat(), position.y.toFloat());
                lastMousePosition.value = mousePositionValue
            }
            .build()
    }

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
        Box(modifier = Modifier
            .fillMaxSize()
            .padding(paddingValues)
            .onGloballyPositioned { coordinates ->
                containerHeight.intValue = coordinates.size.height
            }
            .pointerInput(Unit) {
                detectDragGestures(
                    onDragStart = { offset ->
                        dragPosition.value = Point(offset.x.toInt(), offset.y.toInt())
                    },
                    onDrag = { _, dragAmount ->
                        dragPosition.value = Point(
                            (dragPosition.value.x + dragAmount.x).toInt(),
                            (dragPosition.value.y + dragAmount.y).toInt()
                        )
                        val invertedYDragPosition = Point(
                            dragPosition.value.x,
                            containerHeight.intValue - dragPosition.value.y,
                        )
                        mousePosition.value = controller.toSkeletonCoordinates(
                            invertedYDragPosition
                        )
                    },
                    onDragEnd = { ->
                        mousePosition.value = null;
                        lastMousePosition.value = null;
                    }
                )
            }
        ) {
            AndroidView(
                factory = { ctx ->
                    SpineView(ctx).apply {
                        loadFromAsset(
                            "celestial-circus.atlas",
                            "celestial-circus-pro.skel",
                            controller
                        )
                    }
                },
                modifier = Modifier.padding(paddingValues)
            )
        }
    }
}
