package com.esotericsoftware.spine.android.bounds;

import com.esotericsoftware.spine.Animation;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.Skin;
import com.esotericsoftware.spine.android.AndroidSkeletonDrawable;

import java.util.Collections;
import java.util.List;

public class SkinAndAnimationBounds implements BoundsProvider {
    private final List<String> skins;
    private final String animation;
    private final double stepTime;

    // Constructor
    public SkinAndAnimationBounds(List<String> skins, String animation, double stepTime) {
        this.skins = (skins == null || skins.isEmpty()) ? Collections.singletonList("default") : skins;
        this.animation = animation;
        this.stepTime = stepTime;
    }

    public SkinAndAnimationBounds(List<String> skins, String animation) {
        this(skins, animation, 0.1);
    }

    public SkinAndAnimationBounds(String animation) {
        this(Collections.emptyList(), animation, 0.1);
    }

    @Override
    public Bounds computeBounds(AndroidSkeletonDrawable drawable) {
        SkeletonData data = drawable.getSkeletonData();
        Skin oldSkin = drawable.getSkeleton().getSkin();
        Skin customSkin = new Skin("custom-skin");
        for (String skinName : skins) {
            Skin skin = data.findSkin(skinName);
            if (skin == null) continue;
            customSkin.addSkin(skin);
        }
        drawable.getSkeleton().setSkin(customSkin);
        drawable.getSkeleton().setToSetupPose();

        Animation animation = (this.animation != null) ? data.findAnimation(this.animation) : null;
        double minX = Double.POSITIVE_INFINITY;
        double minY = Double.POSITIVE_INFINITY;
        double maxX = Double.NEGATIVE_INFINITY;
        double maxY = Double.NEGATIVE_INFINITY;
        if (animation == null) {
            Bounds bounds = new Bounds(drawable.getSkeleton());
            minX = bounds.getX();
            minY = bounds.getY();
            maxX = minX + bounds.getWidth();
            maxY = minY + bounds.getHeight();
        } else {
            drawable.getAnimationState().setAnimation(0, animation, false);
            int steps = (int) Math.max( (animation.getDuration() / stepTime), 1.0);
            for (int i = 0; i < steps; i++) {
                drawable.update(i > 0 ? (float) stepTime : 0);
                Bounds bounds = new Bounds(drawable.getSkeleton());
                minX = Math.min(minX, bounds.getX());
                minY = Math.min(minY, bounds.getY());
                maxX = Math.max(maxX, minX + bounds.getWidth());
                maxY = Math.max(maxY, minY + bounds.getHeight());
            }
        }

        drawable.getSkeleton().setSkin("default");
        drawable.getAnimationState().clearTracks();
        if (oldSkin != null) drawable.getSkeleton().setSkin(oldSkin);
        drawable.getSkeleton().setToSetupPose();
        drawable.update(0);
        return new Bounds(minX, minY, maxX - minX, maxY - minY);
    }
}
