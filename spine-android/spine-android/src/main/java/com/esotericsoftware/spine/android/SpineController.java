package com.esotericsoftware.spine.android;

import android.graphics.Canvas;
import android.graphics.Point;

import androidx.annotation.Nullable;

import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.android.callbacks.SpineControllerAfterPaintCallback;
import com.esotericsoftware.spine.android.callbacks.SpineControllerBeforePaintCallback;
import com.esotericsoftware.spine.android.callbacks.SpineControllerCallback;

/**
 * Controls how the skeleton of a {@link SpineView} is animated and rendered.
 *
 * Upon initialization of a {@link SpineView}, the provided {@code onInitialized} callback method is called once. This method can be used
 * to set up the initial animation(s) of the skeleton, among other things.
 *
 * After initialization is complete, the {@link SpineView} is rendered at the screen refresh rate. In each frame,
 * the {@link AnimationState} is updated and applied to the {@link Skeleton}.
 *
 * Next, the optionally provided method {@code onBeforeUpdateWorldTransforms} is called, which can modify the
 * skeleton before its current pose is calculated using {@link Skeleton#updateWorldTransform(Skeleton.Physics)}. After
 * {@link Skeleton#updateWorldTransform(Skeleton.Physics)} has completed, the optional {@code onAfterUpdateWorldTransforms} method is
 * called, which can modify the current pose before rendering the skeleton.
 *
 * Before the skeleton's current pose is rendered by the {@link SpineView}, the optional {@code onBeforePaint} is called,
 * which allows rendering backgrounds or other objects that should go behind the skeleton on the {@link Canvas}. The
 * {@link SpineView} then renders the skeleton's current pose and finally calls the optional {@code onAfterPaint}, which
 * can render additional objects on top of the skeleton.
 *
 * The underlying {@link AndroidTextureAtlas}, {@link SkeletonData}, {@link Skeleton}, {@link AnimationStateData}, {@link AnimationState}, and {@link AndroidSkeletonDrawable}
 * can be accessed through their respective getters to inspect and/or modify the skeleton and its associated data. Accessing
 * this data is only allowed if the {@link SpineView} and its data have been initialized and have not been disposed of yet.
 *
 * By default, the widget updates and renders the skeleton every frame. The {@code pause} method can be used to pause updating
 * and rendering the skeleton. The {@link SpineController#resume()} method resumes updating and rendering the skeleton. The {@link SpineController#isPlaying()} getter
 * reports the current state.
 */
public class SpineController {
    /**
     * Used to build {@link SpineController} instances.
     * */
    public static class Builder {
        private final SpineControllerCallback onInitialized;
        private SpineControllerCallback onBeforeUpdateWorldTransforms;
        private SpineControllerCallback onAfterUpdateWorldTransforms;
        private SpineControllerBeforePaintCallback onBeforePaint;
        private SpineControllerAfterPaintCallback onAfterPaint;

        /**
         * Instantiate a {@link Builder} used to build a {@link SpineController}, which controls how the skeleton of a {@link SpineView}
         * is animated and rendered. Upon initialization of a {@link SpineView}, the provided {@code onInitialized} callback
         * method is called once. This method can be used to set up the initial animation(s) of the skeleton, among other things.
         *
         * @param onInitialized Upon initialization of a {@link SpineView}, the provided {@code onInitialized} callback
         *                      method is called once. This method can be used to set up the initial animation(s) of the skeleton,
         *                      among other things.
         */
        public Builder(SpineControllerCallback onInitialized) {
            this.onInitialized = onInitialized;
        }

        /**
         * Sets the {@code onBeforeUpdateWorldTransforms} callback. It is called before the skeleton's current pose is calculated
         * using {@link Skeleton#updateWorldTransform(Skeleton.Physics)}. It can be used to modify the skeleton before the pose calculation.
         */
        public Builder setOnBeforeUpdateWorldTransforms(SpineControllerCallback onBeforeUpdateWorldTransforms) {
            this.onBeforeUpdateWorldTransforms = onBeforeUpdateWorldTransforms;
            return this;
        }

        /**
         * Sets the {@code onAfterUpdateWorldTransforms} callback. This method is called after the skeleton's current pose is calculated using
         * {@link Skeleton#updateWorldTransform(Skeleton.Physics)}. It can be used to modify the current pose before rendering the skeleton.
         */
        public Builder setOnAfterUpdateWorldTransforms(SpineControllerCallback onAfterUpdateWorldTransforms) {
            this.onAfterUpdateWorldTransforms = onAfterUpdateWorldTransforms;
            return this;
        }

        /**
         * Sets the {@code onBeforePaint} callback. It is called before the skeleton's current pose is rendered by the
         * {@link SpineView}. It allows rendering backgrounds or other objects that should go behind the skeleton on the
         * {@link Canvas}.
         */
        public Builder setOnBeforePaint(SpineControllerBeforePaintCallback onBeforePaint) {
            this.onBeforePaint = onBeforePaint;
            return this;
        }

        /**
         * Sets the {@code onAfterPaint} callback. It is called after the skeleton's current pose is rendered by the
         * {@link SpineView}. It allows rendering additional objects on top of the skeleton.
         */
        public Builder setOnAfterPaint(SpineControllerAfterPaintCallback onAfterPaint) {
            this.onAfterPaint = onAfterPaint;
            return this;
        }

        public SpineController build() {
            SpineController spineController = new SpineController(onInitialized);
            spineController.onBeforeUpdateWorldTransforms = onBeforeUpdateWorldTransforms;
            spineController.onAfterUpdateWorldTransforms = onAfterUpdateWorldTransforms;
            spineController.onBeforePaint = onBeforePaint;
            spineController.onAfterPaint = onAfterPaint;
            return spineController;
        }
    }

    private final SpineControllerCallback onInitialized;
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

    /**
     * Instantiate a {@link SpineController}, which controls how the skeleton of a {@link SpineView} is animated and rendered.
     * Upon initialization of a {@link SpineView}, the provided {@code onInitialized} callback method is called once.
     * This method can be used to set up the initial animation(s) of the skeleton, among other things.
     *
     * @param onInitialized Upon initialization of a {@link SpineView}, the provided {@code onInitialized} callback
     *                      method is called once. This method can be used to set up the initial animation(s) of the skeleton,
     *                      among other things.
     */
    public SpineController(SpineControllerCallback onInitialized) {
        this.onInitialized = onInitialized;
    }

    protected void init(AndroidSkeletonDrawable drawable) {
        this.drawable = drawable;
        if (onInitialized != null) {
            onInitialized.execute(this);
        }
    }

    /**
     * The {@link AndroidTextureAtlas} from which images to render the skeleton are sourced.
     */
    public AndroidTextureAtlas getAtlas() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getAtlas();
    }

    /**
     * The setup-pose data used by the skeleton.
     */
    public SkeletonData getSkeletonDate() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getSkeletonData();
    }

    /**
     * The {@link Skeleton}.
     */
    public Skeleton getSkeleton() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getSkeleton();
    }

    /**
     * The mixing information used by the {@link AnimationState}.
     */
    public AnimationStateData getAnimationStateData() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getAnimationStateData();
    }

    /**
     * The {@link AnimationState} used to manage animations that are being applied to the
     * skeleton.
     */
    public AnimationState getAnimationState() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable.getAnimationState();
    }

    /**
     * The {@link AndroidSkeletonDrawable}.
     */
    public AndroidSkeletonDrawable getDrawable() {
        if (drawable == null) throw new RuntimeException("Controller is not initialized yet.");
        return drawable;
    }

    /**
     * Checks if the {@link  SpineView} is initialized.
     */
    public boolean isInitialized() {
        return drawable != null;
    }

    /**
     * Checks if the animation is currently playing.
     */
    public boolean isPlaying() {
        return playing;
    }

    /**
     * Pauses updating and rendering the skeleton.
     */
    public void pause() {
        if (playing) {
            playing = false;
        }
    }

    /**
     * Resumes updating and rendering the skeleton.
     */
    public void resume() {
        if (!playing) {
            playing = true;
        }
    }

    /**
     * Transforms the coordinates given in the {@link SpineView} coordinate system in {@code position} to
     * the skeleton coordinate system. See the {@code IKFollowing.kt} example for how to use this
     * to move a bone based on user touch input.
     */
    public Point toSkeletonCoordinates(Point position) {
        int x = position.x;
        int y = position.y;
        return new Point((int) (x / scaleX - offsetX), (int) (y / scaleY - offsetY));
    }

    /**
     * Sets the {@code onBeforeUpdateWorldTransforms} callback. It is called before the skeleton's current pose is calculated
     * using {@link Skeleton#updateWorldTransform(Skeleton.Physics)}. It can be used to modify the skeleton before the pose calculation.
     */
    public void setOnBeforeUpdateWorldTransforms(@Nullable SpineControllerCallback onBeforeUpdateWorldTransforms) {
        this.onBeforeUpdateWorldTransforms = onBeforeUpdateWorldTransforms;
    }

    /**
     * Sets the {@code onAfterUpdateWorldTransforms} callback. This method is called after the skeleton's current pose is calculated using
     * {@link Skeleton#updateWorldTransform(Skeleton.Physics)}. It can be used to modify the current pose before rendering the skeleton.
     */
    public void setOnAfterUpdateWorldTransforms(@Nullable SpineControllerCallback onAfterUpdateWorldTransforms) {
        this.onAfterUpdateWorldTransforms = onAfterUpdateWorldTransforms;
    }

    /**
     * Sets the {@code onBeforePaint} callback. It is called before the skeleton's current pose is rendered by the
     * {@link SpineView}. It allows rendering backgrounds or other objects that should go behind the skeleton on the
     * {@link Canvas}.
     */
    public void setOnBeforePaint(@Nullable SpineControllerBeforePaintCallback onBeforePaint) {
        this.onBeforePaint = onBeforePaint;
    }

    /**
     * Sets the {@code onAfterPaint} callback. It is called after the skeleton's current pose is rendered by the
     * {@link SpineView}. It allows rendering additional objects on top of the skeleton.
     */
    public void setOnAfterPaint(@Nullable SpineControllerAfterPaintCallback onAfterPaint) {
        this.onAfterPaint = onAfterPaint;
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
