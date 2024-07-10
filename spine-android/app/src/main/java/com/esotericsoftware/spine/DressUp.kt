package com.esotericsoftware.spine

import androidx.compose.foundation.Image
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.rounded.ArrowBack
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.mutableStateMapOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.drawWithCache
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Rect
import androidx.compose.ui.graphics.ColorFilter
import androidx.compose.ui.graphics.ColorMatrix
import androidx.compose.ui.graphics.ImageBitmap
import androidx.compose.ui.graphics.Paint
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.graphics.drawscope.drawIntoCanvas
import androidx.compose.ui.graphics.painter.BitmapPainter
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.navigation.NavHostController
import com.esotericsoftware.spine.android.AndroidSkeletonDrawable
import com.esotericsoftware.spine.android.SkeletonRenderer
import com.esotericsoftware.spine.android.SpineController
import com.esotericsoftware.spine.android.SpineView

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DressUp(nav: NavHostController) {

    val context = LocalContext.current
    val thumbnailSize = 150f

    val drawable = remember {
        AndroidSkeletonDrawable.fromAsset(
            "mix-and-match.atlas",
            "mix-and-match-pro.skel",
            context
        )
    }

    val renderer = remember {
        SkeletonRenderer()
    }

    val customSkin = remember {
        mutableStateOf<Skin?>(null)
    }

    val skinImages = remember {
        mutableStateMapOf<String, ImageBitmap>()
    }

    val selectedSkins = remember {
        mutableStateMapOf<String, Boolean>()
    }

    val controller = remember {
        SpineController { controller ->
            controller.animationState.setAnimation(0, "dance", true)
        }
    }

    fun toggleSkin(skinName: String) {
        selectedSkins[skinName] = !(selectedSkins[skinName] ?: false)
        drawable.skeleton.setSkin("default")
        customSkin.value = Skin("custom-skin");
        for (selectedSkinKey in selectedSkins.keys) {
            if (selectedSkins[selectedSkinKey] == true) {
                val selectedSkin = drawable.skeletonData.findSkin(selectedSkinKey)
                if (selectedSkin != null) customSkin.value?.addSkin(selectedSkin)
            }
        }
        val customSkinValue = customSkin.value
        if (customSkinValue != null) {
            drawable.skeleton.setSkin(customSkinValue)
        }
        drawable.skeleton.setSlotsToSetupPose()
    }

    val localDensity = LocalDensity.current

    LaunchedEffect(Unit) {
        for (skin in drawable.skeletonData.getSkins()) {
            if (skin.getName() == "default") continue
            val skeleton = drawable.skeleton
            skeleton.setSkin(skin)
            skeleton.setToSetupPose()
            skeleton.update(0f)
            skeleton.updateWorldTransform(Skeleton.Physics.update)
            skinImages[skin.getName()] = drawable.renderToBitmap(
                renderer,
                with(localDensity) { thumbnailSize.dp.toPx() },
                with(localDensity) { thumbnailSize.dp.toPx() },
                0xffffffff.toInt()
            ).asImageBitmap()
            selectedSkins[skin.getName()] = false
        }
        toggleSkin("full-skins/girl");
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = Destination.DressUp.title) },
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
        Row(
            modifier = Modifier
                .width(thumbnailSize.dp)
                .verticalScroll(rememberScrollState())
                .padding(paddingValues)
        ) {
            Column {
                skinImages.keys.forEach { skinName ->
                    Box(modifier = Modifier
                        .clickable {
                            toggleSkin(skinName)
                        }
                        .then(
                            if (selectedSkins[skinName] == true) {
                                Modifier
                            } else {
                                Modifier.grayScale()
                            }
                        )
                    ) {
                        Image(
                            painter = BitmapPainter(skinImages[skinName]!!),
                            contentDescription = null
                        )
                    }
                }
            }
            AndroidView(
                factory = { context ->
                    SpineView.loadFromDrawable(drawable, context, controller)
                },
                modifier = Modifier.padding(paddingValues)
            )
        }
    }
}

fun Modifier.grayScale(): Modifier {
    val saturationMatrix = ColorMatrix().apply { setToSaturation(0f) }
    val saturationFilter = ColorFilter.colorMatrix(saturationMatrix)
    val paint = Paint().apply { colorFilter = saturationFilter }

    return drawWithCache {
        val canvasBounds = Rect(Offset.Zero, size)
        onDrawWithContent {
            drawIntoCanvas {
                it.saveLayer(canvasBounds, paint)
                drawContent()
                it.restore()
            }
        }
    }
}
