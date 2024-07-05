package com.esotericsoftware.spine.android;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.RectF;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.android.utils.SkeletonDataUtils;

import java.io.File;
import java.net.URL;

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

    public Bitmap renderToBitmap(SkeletonRenderer renderer, float width, float height, int bgColor) {
        Vector2 offset = new Vector2(0, 0);
        Vector2 size = new Vector2(0, 0);
        FloatArray floatArray = new FloatArray();

        getSkeleton().getBounds(offset, size, floatArray);

        RectF bounds = new RectF(offset.x, offset.y, offset.x + size.x, offset.y + size.y);
        float scale = (1 / (bounds.width() > bounds.height() ? bounds.width() / width : bounds.height() / height));

        Bitmap bitmap = Bitmap.createBitmap((int) width, (int) height, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmap);

        Paint paint = new Paint();
        paint.setColor(bgColor);
        paint.setStyle(Paint.Style.FILL);

        // Draw background
        canvas.drawRect(0, 0, width, height, paint);

        // Transform canvas
        canvas.translate(width / 2, height / 2);
        canvas.scale(scale, -scale);
        canvas.translate(-(bounds.left + bounds.width() / 2), -(bounds.top + bounds.height() / 2));

        renderer.render(canvas, renderer.render(skeleton));

        return bitmap;
    }
}
