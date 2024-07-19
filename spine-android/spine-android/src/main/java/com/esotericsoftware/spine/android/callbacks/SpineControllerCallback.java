package com.esotericsoftware.spine.android.callbacks;

import com.esotericsoftware.spine.android.SpineController;

@FunctionalInterface
public interface SpineControllerCallback {
    void execute (SpineController controller);
}
