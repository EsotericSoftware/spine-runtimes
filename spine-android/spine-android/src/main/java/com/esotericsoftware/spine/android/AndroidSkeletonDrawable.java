package com.esotericsoftware.spine.android;

import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonData;

public class AndroidSkeletonDrawable {

    private final AndroidTextureAtlas atlas;

    private final SkeletonData skeletonData;

    private final Skeleton skeleton;

    private final AnimationStateData animationStateData;

    private final AnimationState animationState;

    public AndroidSkeletonDrawable(AndroidTextureAtlas atlas, SkeletonData skeletonData) {
        this.atlas = atlas;
        this.skeletonData = skeletonData;

        skeleton = new Skeleton(skeletonData);
        animationStateData = new AnimationStateData(skeletonData);
        animationState = new AnimationState(animationStateData);

        skeleton.updateWorldTransform(Skeleton.Physics.none);
    }

    public void update(float delta) {
        animationState.update(delta);
        animationState.apply(skeleton);

        skeleton.update(delta);
        skeleton.updateWorldTransform(Skeleton.Physics.update);
    }

    public AndroidTextureAtlas getAtlas() {
        return atlas;
    }

    public Skeleton getSkeleton() {
        return skeleton;
    }

    public SkeletonData getSkeletonData() {
        return skeletonData;
    }

    public AnimationStateData getAnimationStateData() {
        return animationStateData;
    }

    public AnimationState getAnimationState() {
        return animationState;
    }
}
