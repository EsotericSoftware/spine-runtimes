package com.esotericsoftware.spine.android.bounds;

import com.esotericsoftware.spine.android.AndroidSkeletonDrawable;

public interface BoundsProvider {
    Bounds computeBounds(AndroidSkeletonDrawable drawable);
}
