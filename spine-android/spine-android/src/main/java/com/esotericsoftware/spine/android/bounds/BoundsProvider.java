package com.esotericsoftware.spine.android.bounds;

import com.esotericsoftware.spine.android.AndroidSkeletonDrawable;

/**
 * A {@link BoundsProvider} that calculates the bounding box of the skeleton based on the visible
 * attachments in the setup pose.
 */
public interface BoundsProvider {
    Bounds computeBounds(AndroidSkeletonDrawable drawable);
}
