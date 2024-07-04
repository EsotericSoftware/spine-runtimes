package com.esotericsoftware.spine.android;

import android.graphics.Canvas;
import android.graphics.Point;

import androidx.annotation.Nullable;

import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.android.utils.SpineControllerAfterPaintCallback;
import com.esotericsoftware.spine.android.utils.SpineControllerBeforePaintCallback;
import com.esotericsoftware.spine.android.utils.SpineControllerCallback;

public class SpineController {

    public static class Builder {
        private SpineControllerCallback onInitialized;
        private SpineControllerCallback onBeforeUpdateWorldTransforms;
        private SpineControllerCallback onAfterUpdateWorldTransforms;
        private SpineControllerBeforePaintCallback onBeforePaint;
        private SpineControllerAfterPaintCallback onAfterPaint;

        public Builder setOnInitialized(SpineControllerCallback onInitialized) {
            this.onInitialized = onInitialized;
            return this;
        }

        public Builder setOnBeforeUpdateWorldTransforms(SpineControllerCallback onBeforeUpdateWorldTransforms) {
            this.onBeforeUpdateWorldTransforms = onBeforeUpdateWorldTransforms;
            return this;
        }

        public Builder setOnAfterUpdateWorldTransforms(SpineControllerCallback onAfterUpdateWorldTransforms) {
            this.onAfterUpdateWorldTransforms = onAfterUpdateWorldTransforms;
            return this;
        }

        public Builder setOnBeforePaint(SpineControllerBeforePaintCallback onBeforePaint) {
            this.onBeforePaint = onBeforePaint;
            return this;
        }

        public Builder setOnAfterPaint(SpineControllerAfterPaintCallback onAfterPaint) {
            this.onAfterPaint = onAfterPaint;
            return this;
        }

        public SpineController build() {
            SpineController spineController = new SpineController();
            spineController.onInitialized = onInitialized;
            spineController.onBeforeUpdateWorldTransforms = onBeforeUpdateWorldTransforms;
            spineController.onAfterUpdateWorldTransforms = onAfterUpdateWorldTransforms;
            spineController.onBeforePaint = onBeforePaint;
            spineController.onAfterPaint = onAfterPaint;
            return spineController;
        }
    }

    private @Nullable SpineControllerCallback onInitialized;
    private @Nullable SpineControllerCallback onBeforeUpdateWorldTransforms;
    private @Nullable SpineControllerCallback onAfterUpdateWorldTransforms;
    private @Nullable SpineControllerBeforePaintCallback onBeforePaint;
    private @Nullable SpineControllerAfterPaintCallback onAfterPaint;
    private AndroidSkeletonDrawable drawable;
    private boolean playing = true;
    private double offsetX = 0;
    private double offsetY = 0;
    private double scaleX = 1;
    private double scaleY = 1;

    protected void init(AndroidSkeletonDrawable drawable) {
        this.drawable = drawable;
        if (onInitialized != null) {
            onInitialized.execute(this);
        }
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

    public AndroidSkeletonDrawable getDrawable() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable;
    }

    public boolean isInitialized() {
        return drawable != null;
    }

    public boolean isPlaying() {
        return playing;
    }

    public void pause() {
        playing = false;
    }

    public void resume() {
        playing = true;
    }

    public Point toSkeletonCoordinates(Point position) {
        int x = position.x;
        int y = position.y;
        return new Point((int) (x / scaleX - offsetX), (int) (y / scaleY - offsetY));
    }

    protected void setCoordinateTransform(double offsetX, double offsetY, double scaleX, double scaleY) {
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
    }

    protected void callOnBeforeUpdateWorldTransforms() {
        if (onBeforeUpdateWorldTransforms != null) {
            onBeforeUpdateWorldTransforms.execute(this);
        }
    }

    protected void callOnAfterUpdateWorldTransforms() {
        if (onAfterUpdateWorldTransforms != null) {
            onAfterUpdateWorldTransforms.execute(this);
        }
    }

    protected void callOnBeforePaint(Canvas canvas) {
        if (onBeforePaint != null) {
            onBeforePaint.execute(this, canvas);
        }
    }

    protected void callOnAfterPaint(Canvas canvas, Array<SkeletonRenderer.RenderCommand> renderCommands) {
        if (onAfterPaint != null) {
            onAfterPaint.execute(this, canvas, renderCommands);
        }
    }
}
