package com.esotericsoftware.spine.android;

import android.content.Context;
import android.content.res.AssetManager;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.view.Choreographer;
import android.view.View;

import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.BlendMode;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonBinary;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.Slot;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.utils.SkeletonClipping;

import java.io.BufferedInputStream;
import java.io.IOException;
import java.io.InputStream;

public class SpineView extends View implements Choreographer.FrameCallback {
    private long lastTime = 0;
    private long delta = 0;
    private Paint textPaint;
    int instances = 100;
    Vector2[] coords = new Vector2[instances];
    AndroidTextureAtlas atlas;
    SkeletonData data;
    Array<Skeleton> skeletons = new Array<>();

    Array<AnimationState> states = new Array<>();

    public SpineView(Context context) {
        super(context);
        init();
    }

    public SpineView(Context context, AttributeSet attrs) {
        super(context, attrs);
        init();
    }

    public SpineView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
        init();
    }

    private void loadSkeleton() {
        AssetManager assetManager = this.getContext().getAssets();
        atlas = AndroidTextureAtlas.loadFromAssets("spineboy.atlas", assetManager);
        AndroidAtlasAttachmentLoader attachmentLoader = new AndroidAtlasAttachmentLoader(atlas);
        SkeletonBinary binary = new SkeletonBinary(attachmentLoader);
        try (InputStream in = new BufferedInputStream(assetManager.open("spineboy-pro.skel"))) {
            data = binary.readSkeletonData(in);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }

    static private final short[] quadTriangles = {0, 1, 2, 2, 3, 0};
    private final FloatArray vertices = new FloatArray(32);
    private final FloatArray texCoords = new FloatArray(32);
    private final IntArray colors = new IntArray(32);
    private final SkeletonClipping clipper = new SkeletonClipping();

    public void render (Canvas canvas, Skeleton skeleton, float x, float y) {
        canvas.save();
        canvas.translate(x, y);
        canvas.scale(1, -1);
        BlendMode blendMode = null;
        int verticesLength = 0;
        short[] triangles = null;
        com.badlogic.gdx.graphics.Color color = null, skeletonColor = skeleton.getColor();
        float r = skeletonColor.r, g = skeletonColor.g, b = skeletonColor.b, a = skeletonColor.a;
        Object[] drawOrder = skeleton.getDrawOrder().items;
        for (int i = 0, n = skeleton.getDrawOrder().size; i < n; i++) {
            Slot slot = (Slot)drawOrder[i];
            if (!slot.getBone().isActive()) {
                clipper.clipEnd(slot);
                continue;
            }
            AndroidTexture texture = null;
            int vertexSize = 2;
            Attachment attachment = slot.getAttachment();
            if (attachment instanceof RegionAttachment) {
                RegionAttachment region = (RegionAttachment)attachment;
                verticesLength = vertexSize << 2;
                region.computeWorldVertices(slot, vertices.items, 0, vertexSize);
                triangles = quadTriangles;
                texture = (AndroidTexture)region.getRegion().getTexture();
                texCoords.clear();
                texCoords.addAll(region.getUVs());
                color = region.getColor();

            } else if (attachment instanceof MeshAttachment) {
                MeshAttachment mesh = (MeshAttachment)attachment;
                int count = mesh.getWorldVerticesLength();
                verticesLength = (count >> 1) * vertexSize;
                this.vertices.setSize(verticesLength);
                mesh.computeWorldVertices(slot, 0, count, vertices.items, 0, vertexSize);
                triangles = mesh.getTriangles();
                texture = (AndroidTexture)mesh.getRegion().getTexture();
                texCoords.clear();;
                texCoords.addAll(mesh.getUVs());
                color = mesh.getColor();

            } else if (attachment instanceof ClippingAttachment) {
                ClippingAttachment clip = (ClippingAttachment)attachment;
                clipper.clipStart(slot, clip);
                continue;

            } else {
                continue;
            }

            if (texture != null) {
                com.badlogic.gdx.graphics.Color slotColor = slot.getColor();
                float alpha = a * slotColor.a * color.a * 255;
                float multiplier = 255;

                BlendMode slotBlendMode = slot.getData().getBlendMode();
                if (slotBlendMode != blendMode) {
                    if (slotBlendMode == BlendMode.additive) {
                        slotBlendMode = BlendMode.normal;
                        alpha = 0;
                    }
                    blendMode = slotBlendMode;
                    // FIXME
                    // blendMode.apply(batch, pmaBlendModes);
                }

                int c =   (int)alpha << 24 //
                        | (int)(b * slotColor.b * color.b * multiplier) << 16 //
                        | (int)(g * slotColor.g * color.g * multiplier) << 8 //
                        | (int)(r * slotColor.r * color.r * multiplier);

                if (clipper.isClipping()) {
                    // FIXME
                    throw new RuntimeException("Not implemented, need to split positions, uvs, colors");
                    // clipper.clipTriangles(vertices, verticesLength, triangles, triangles.length, uvs, c, 0, false);
                    // FloatArray clippedVertices = clipper.getClippedVertices();
                    // ShortArray clippedTriangles = clipper.getClippedTriangles();
                    // batch.draw(texture, clippedVertices.items, 0, clippedVertices.size, clippedTriangles.items, 0,
                    //         clippedTriangles.size);
                } else {
                    float[] uvsArray = texCoords.items;
                    for (int ii = 0, w = texture.getWidth(), h = texture.getHeight(); ii < verticesLength; ii += 2) {
                        uvsArray[ii] = uvsArray[ii] * w;
                        uvsArray[ii + 1] = uvsArray[ii + 1] * h;
                    }
                    colors.setSize(verticesLength >> 1);
                    int[] colorsArray = colors.items;
                    for (int ii = 0, nn = verticesLength >> 1; ii < nn; ii++) {
                        colorsArray[ii] = c;
                    }
                    canvas.drawVertices(Canvas.VertexMode.TRIANGLES, verticesLength, vertices.items, 0, uvsArray, 0, colorsArray, 0, triangles, 0, triangles.length, texture.getPaint());
                }
            }

            clipper.clipEnd(slot);
        }
        clipper.clipEnd();
        canvas.restore();
    }

    private void init() {
        textPaint = new Paint();
        textPaint.setColor(Color.WHITE); // Set the color of the paint
        textPaint.setTextSize(48);
        Choreographer.getInstance().postFrameCallback(this);

        loadSkeleton();

        for (int i = 0; i < instances; i++) {
            Skeleton skeleton = new Skeleton(data);
            skeleton.setToSetupPose();
            skeletons.add(skeleton);

            AnimationStateData stateData = new AnimationStateData(data);
            stateData.setDefaultMix(0.2f);
            AnimationState state = new AnimationState(stateData);
            state.setAnimation(0, "walk", true);
            states.add(state);

            coords[i] = new Vector2(MathUtils.random(1000), MathUtils.random(2000));
        }
    }

    @Override
    public void onDraw(Canvas canvas) {
        super.onDraw(canvas);

        float deltaF = delta / 1e9f;

        for (int i = 0; i < instances; i++) {
            AnimationState state = states.get(i);
            Skeleton skeleton = skeletons.get(i);
            state.update(deltaF);
            state.apply(skeleton);
            skeleton.update(deltaF);
            skeleton.updateWorldTransform(Skeleton.Physics.update);
            render(canvas, skeleton, coords[i].x, coords[i].y);
        }
        // canvas.drawVertices(Canvas.VertexMode.TRIANGLES, vertices.size, vertices.items, 0, uvs.items, 0, null, 0, indices.items, 0, 3 * 75, paint);
        canvas.drawText(delta / 1e6 + " ms", 100, 100, textPaint);
        canvas.drawText(instances + " instances", 100, 150, textPaint);
    }

    @Override
    public void doFrame(long frameTimeNanos) {
        if (lastTime != 0) delta = frameTimeNanos - lastTime;
        lastTime = frameTimeNanos;

        // Invalidate this view, causing onDraw to be called at the next animation frame
        invalidate();
        Choreographer.getInstance().postFrameCallback(this);
    }
}
