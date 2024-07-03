package com.esotericsoftware.spine.android;

import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.android.utils.SpineControllerCallback;

public class SpineController {
    private final SpineControllerCallback onInitialized;
    private AndroidSkeletonDrawable drawable;

    private boolean playing = true;

    public SpineController(SpineControllerCallback onInitialized) {
        this.onInitialized = onInitialized;
    }

    protected void init(AndroidSkeletonDrawable drawable) {
        this.drawable = drawable;
        onInitialized.execute(this);
    }

    public AndroidTextureAtlas getAtlas() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getAtlas();
    }

    public SkeletonData getSkeletonDate() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getSkeletonData();
    }

    public Skeleton getSkeleton() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getSkeleton();
    }

    public AnimationStateData getAnimationStateData() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getAnimationStateData();
    }

    public AnimationState getAnimationState() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getAnimationState();
    }

    AndroidSkeletonDrawable getDrawable() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable;
    }

    public boolean isInitialized() {
        return drawable != null;
    };

    public boolean isPlaying() {
        return playing;
    }

    public void pause() {
        playing = false;
    }

    public void resume() {
        playing = true;
    }
}
