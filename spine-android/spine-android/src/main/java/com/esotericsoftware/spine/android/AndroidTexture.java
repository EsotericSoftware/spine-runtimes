package com.esotericsoftware.spine.android;

import android.graphics.Bitmap;
import android.graphics.BitmapShader;
import android.graphics.Paint;
import android.graphics.Shader;

import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.TextureData;

public class AndroidTexture extends Texture {
    private Bitmap bitmap;
    private Paint paint;

    protected AndroidTexture(Bitmap bitmap) {
        super();
        this.bitmap = bitmap;
        this.paint = new Paint();
        BitmapShader shader = new BitmapShader(bitmap, Shader.TileMode.CLAMP, Shader.TileMode.CLAMP);
        paint.setShader(shader);
    }

    public Bitmap getBitmap() {
        return bitmap;
    }

    public Paint getPaint() {
        return paint;
    }

    @Override
    public int getWidth() {
        return bitmap.getWidth();
    }

    @Override
    public int getHeight() {
        return bitmap.getHeight();
    }

    @Override
    public void dispose() {
        bitmap.recycle();
    }
}
