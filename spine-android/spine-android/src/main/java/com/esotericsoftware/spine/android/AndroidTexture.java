
package com.esotericsoftware.spine.android;

import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.utils.ObjectMap;
import com.esotericsoftware.spine.BlendMode;

import android.graphics.Bitmap;
import android.graphics.BitmapShader;
import android.graphics.Paint;
import android.graphics.PorterDuff;
import android.graphics.PorterDuffXfermode;
import android.graphics.Shader;

public class AndroidTexture extends Texture {
	private Bitmap bitmap;
	private ObjectMap<BlendMode, Paint> paints = new ObjectMap<>();

	protected AndroidTexture (Bitmap bitmap) {
		super();
		this.bitmap = bitmap;
		for (BlendMode blendMode : BlendMode.values()) {
			Paint paint = new Paint();
			BitmapShader shader = new BitmapShader(bitmap, Shader.TileMode.CLAMP, Shader.TileMode.CLAMP);
			paint.setShader(shader);

			switch (blendMode) {
			case normal:
				paint.setXfermode(new PorterDuffXfermode(PorterDuff.Mode.SRC_OVER));
				break;
			case multiply:
				paint.setXfermode(new PorterDuffXfermode(PorterDuff.Mode.MULTIPLY));
				break;
			case additive:
				paint.setXfermode(new PorterDuffXfermode(PorterDuff.Mode.ADD));
				break;
			case screen:
				paint.setXfermode(new PorterDuffXfermode(PorterDuff.Mode.SCREEN));
				break;
			default:
				break;
			}

			paints.put(blendMode, paint);
		}
	}

	public Bitmap getBitmap () {
		return bitmap;
	}

	public Paint getPaint (BlendMode blendMode) {
		return paints.get(blendMode);
	}

	@Override
	public int getWidth () {
		return bitmap.getWidth();
	}

	@Override
	public int getHeight () {
		return bitmap.getHeight();
	}

	@Override
	public void dispose () {
		bitmap.recycle();
	}
}
