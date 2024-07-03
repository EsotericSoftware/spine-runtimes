package com.esotericsoftware.spine.android;

import android.content.Context;

import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.android.utils.SkeletonDataUtils;

import java.io.File;
import java.net.URL;

import kotlin.NotImplementedError;

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

    public static AndroidSkeletonDrawable fromAsset (String atlasFileName, String skeletonFileName, Context context) {
        AndroidTextureAtlas atlas = AndroidTextureAtlas.fromAsset(atlasFileName, context);
        SkeletonData skeletonData = SkeletonDataUtils.fromAsset(atlas, skeletonFileName, context);
        return new AndroidSkeletonDrawable(atlas, skeletonData);
    }

    public static AndroidSkeletonDrawable fromFile (File atlasFile, File skeletonFile) {
        AndroidTextureAtlas atlas = AndroidTextureAtlas.fromFile(atlasFile);
        SkeletonData skeletonData = SkeletonDataUtils.fromFile(atlas, skeletonFile);
        return new AndroidSkeletonDrawable(atlas, skeletonData);
    }

    public static AndroidSkeletonDrawable fromHttp (URL atlasUrl, URL skeletonUrl) {
        AndroidTextureAtlas atlas = AndroidTextureAtlas.fromHttp(atlasUrl);
        SkeletonData skeletonData = SkeletonDataUtils.fromHttp(atlas, skeletonUrl);
        return new AndroidSkeletonDrawable(atlas, skeletonData);
    }
}
