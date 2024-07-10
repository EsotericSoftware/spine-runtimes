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
import com.badlogic.gdx.math.Vector2
import com.esotericsoftware.spine.android.SpineController
import com.esotericsoftware.spine.android.SpineView

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun IKFollowing(nav: NavHostController) {

    val containerHeight = remember { mutableIntStateOf(0) }
    val dragPosition = remember { mutableStateOf(Point(0, 0)) }
    val crossHairPosition = remember { mutableStateOf<Point?>(null) }

    val controller = remember {
        SpineController.Builder()
            .setOnInitialized {
                it.animationState.setAnimation(0, "walk", true)
                it.animationState.setAnimation(1, "aim", true)
            }
            .setOnAfterUpdateWorldTransforms {
                val worldPosition = crossHairPosition.value ?: return@setOnAfterUpdateWorldTransforms
                val skeleton = it.skeleton
                val bone = skeleton.findBone("crosshair") ?: return@setOnAfterUpdateWorldTransforms
                val parent = bone.parent ?: return@setOnAfterUpdateWorldTransforms
                val position = parent.worldToLocal(Vector2(worldPosition.x.toFloat(), worldPosition.y.toFloat()))
                bone.x = position.x
                bone.y = position.y
            }
            .build()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = Destination.IKFollowing.title) },
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
                        crossHairPosition.value = controller.toSkeletonCoordinates(
                            invertedYDragPosition
                        )
                    },
                )
            }
        ) {
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
