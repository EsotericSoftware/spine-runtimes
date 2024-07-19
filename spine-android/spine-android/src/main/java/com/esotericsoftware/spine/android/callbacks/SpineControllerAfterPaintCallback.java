package com.esotericsoftware.spine.android.callbacks;

import android.graphics.Canvas;

import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.android.SkeletonRenderer;
import com.esotericsoftware.spine.android.SpineController;

import java.util.List;

@FunctionalInterface
public interface SpineControllerAfterPaintCallback {
    void execute (SpineController controller, Canvas canvas, Array<SkeletonRenderer.RenderCommand> commands);
}
