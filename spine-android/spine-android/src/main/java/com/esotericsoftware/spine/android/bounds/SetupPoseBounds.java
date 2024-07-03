package com.esotericsoftware.spine.android.bounds;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.android.AndroidSkeletonDrawable;

public class SetupPoseBounds implements BoundsProvider {

    @Override
    public Bounds computeBounds(AndroidSkeletonDrawable drawable) {

        Vector2 offset = new Vector2(0, 0);
        Vector2 size = new Vector2(0, 0);
        FloatArray floatArray = new FloatArray();

        drawable.getSkeleton().getBounds(offset, size, floatArray);

        return new Bounds(offset.x, offset.y, size.x, size.y);
    }
}
