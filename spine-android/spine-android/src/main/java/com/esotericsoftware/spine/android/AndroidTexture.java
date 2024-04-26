/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
