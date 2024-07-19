package com.esotericsoftware.spine.android;

import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.RectF;

import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.Bone;

/**
 * Renders debug information for a {@link AndroidSkeletonDrawable}, like bone locations, to a {@link Canvas}.
 * See {@link DebugRenderer#render}.
 */
public class DebugRenderer {

    public void render(AndroidSkeletonDrawable drawable, Canvas canvas, Array<SkeletonRenderer.RenderCommand> commands) {
        Paint bonePaint = new Paint();
        bonePaint.setColor(android.graphics.Color.BLUE);
        bonePaint.setStyle(Paint.Style.FILL);

        for (Bone bone : drawable.getSkeleton().getBones()) {
            float x = bone.getWorldX();
            float y = bone.getWorldY();
            canvas.drawRect(new RectF(x - 2.5f, y - 2.5f, x + 2.5f, y + 2.5f), bonePaint);
        }
    }
}
