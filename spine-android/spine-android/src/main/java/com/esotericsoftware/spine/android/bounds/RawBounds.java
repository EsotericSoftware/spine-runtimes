package com.esotericsoftware.spine.android.bounds;
import com.esotericsoftware.spine.android.AndroidSkeletonDrawable;

/**
 * A {@link BoundsProvider} that returns fixed bounds.
 */
public class RawBounds implements BoundsProvider {
    final Double x;
    final Double y;
    final Double width;
    final Double height;

    public RawBounds(Double x, Double y, Double width, Double height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    @Override
    public Bounds computeBounds(AndroidSkeletonDrawable drawable) {
        return new Bounds(x, y, width, height);
    }
}
