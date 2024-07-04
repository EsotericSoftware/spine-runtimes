package com.esotericsoftware.spine.android.bounds;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.android.AndroidSkeletonDrawable;

public class SetupPoseBounds implements BoundsProvider {

    @Override
    public Bounds computeBounds(AndroidSkeletonDrawable drawable) {
        return new Bounds(drawable.getSkeleton());
    }
}
