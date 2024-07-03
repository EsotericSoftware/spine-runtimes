package com.esotericsoftware.spine.android.utils;

import com.esotericsoftware.spine.android.AndroidSkeletonDrawable;

@FunctionalInterface
public interface AndroidSkeletonDrawableLoader {
    AndroidSkeletonDrawable load();
}
