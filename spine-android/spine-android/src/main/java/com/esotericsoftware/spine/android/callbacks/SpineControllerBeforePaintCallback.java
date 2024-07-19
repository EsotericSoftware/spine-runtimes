package com.esotericsoftware.spine.android.callbacks;

import android.graphics.Canvas;

import com.esotericsoftware.spine.android.SkeletonRenderer;
import com.esotericsoftware.spine.android.SpineController;

import java.util.List;

@FunctionalInterface
public interface SpineControllerBeforePaintCallback {
    void execute (SpineController controller, Canvas canvas);
}
