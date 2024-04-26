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

import java.io.BufferedInputStream;
import java.io.IOException;
import java.io.InputStream;

import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.AnimationState;
import com.esotericsoftware.spine.AnimationStateData;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonBinary;
import com.esotericsoftware.spine.SkeletonData;

import android.content.Context;
import android.content.res.AssetManager;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.view.Choreographer;
import android.view.View;

public class SpineView extends View implements Choreographer.FrameCallback {
	private long lastTime = 0;
	private float delta = 0;
	private Paint textPaint;
	int instances = 1;
	Vector2[] coords = new Vector2[instances];
	AndroidTextureAtlas atlas;
	SkeletonData data;
	Array<Skeleton> skeletons = new Array<>();
	Array<AnimationState> states = new Array<>();
	SkeletonRenderer renderer = new SkeletonRenderer();

	public SpineView (Context context) {
		super(context);
		init();
	}

	public SpineView (Context context, AttributeSet attrs) {
		super(context, attrs);
		init();
	}

	public SpineView (Context context, AttributeSet attrs, int defStyle) {
		super(context, attrs, defStyle);
		init();
	}

	private void loadSkeleton () {
		String skel = "spineboy-pro.skel";
		String atlasFile = "spineboy.atlas";

		AssetManager assetManager = this.getContext().getAssets();
		atlas = AndroidTextureAtlas.loadFromAssets(atlasFile, assetManager);
		AndroidAtlasAttachmentLoader attachmentLoader = new AndroidAtlasAttachmentLoader(atlas);
		SkeletonBinary binary = new SkeletonBinary(attachmentLoader);
		try (InputStream in = new BufferedInputStream(assetManager.open(skel))) {
			data = binary.readSkeletonData(in);
		} catch (IOException e) {
			throw new RuntimeException(e);
		}
	}

	private void init () {
		textPaint = new Paint();
		textPaint.setColor(Color.WHITE); // Set the color of the paint
		textPaint.setTextSize(48);
		Choreographer.getInstance().postFrameCallback(this);

		loadSkeleton();

		for (int i = 0; i < instances; i++) {
			Skeleton skeleton = new Skeleton(data);
			skeleton.setScaleY(-1);
			skeleton.setToSetupPose();
			skeletons.add(skeleton);

			AnimationStateData stateData = new AnimationStateData(data);
			stateData.setDefaultMix(0.2f);
			AnimationState state = new AnimationState(stateData);
			state.setAnimation(0, "hoverboard", true);
			states.add(state);

			if (i == 0) {
				coords[i] = new Vector2(500, 1000);
			} else {
				coords[i] = new Vector2(MathUtils.random(1000), MathUtils.random(3000));
			}
		}
	}

	@Override
	public void onDraw (Canvas canvas) {
		super.onDraw(canvas);

		for (int i = 0; i < instances; i++) {
			AnimationState state = states.get(i);
			Skeleton skeleton = skeletons.get(i);
			state.update(delta);
			state.apply(skeleton);
			skeleton.update(delta);
			skeleton.updateWorldTransform(Skeleton.Physics.update);
			renderer.render(canvas, skeleton, coords[i].x, coords[i].y);
		}

		canvas.drawText(delta * 1000 + " ms", 100, 100, textPaint);
		canvas.drawText(instances + " instances", 100, 150, textPaint);
	}

	@Override
	public void doFrame (long frameTimeNanos) {
		if (lastTime != 0) delta = (frameTimeNanos - lastTime) / 1e9f;
		lastTime = frameTimeNanos;
		invalidate();
		Choreographer.getInstance().postFrameCallback(this);
	}
}
