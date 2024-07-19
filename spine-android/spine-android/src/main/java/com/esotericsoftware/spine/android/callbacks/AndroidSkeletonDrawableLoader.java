package com.esotericsoftware.spine.android.callbacks;

import com.esotericsoftware.spine.android.AndroidSkeletonDrawable;

@FunctionalInterface
public interface AndroidSkeletonDrawableLoader {
    AndroidSkeletonDrawable load();
}
