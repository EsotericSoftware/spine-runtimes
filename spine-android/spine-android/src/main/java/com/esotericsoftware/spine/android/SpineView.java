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

import com.esotericsoftware.spine.android.utils.AndroidSkeletonDrawableLoader;

import android.content.Context;
import android.graphics.Canvas;
import android.os.Handler;
import android.os.Looper;
import android.util.AttributeSet;
import android.view.Choreographer;
import android.view.View;

import androidx.annotation.NonNull;

import java.io.File;
import java.net.URL;

public class SpineView extends View implements Choreographer.FrameCallback {
	private long lastTime = 0;
	private float delta = 0;
	SkeletonRenderer renderer = new SkeletonRenderer();
	SpineController controller;

	public SpineView (Context context) {
		super(context);
	}

	public SpineView (Context context, AttributeSet attrs) {
		super(context, attrs);
	}

	public SpineView (Context context, AttributeSet attrs, int defStyle) {
		super(context, attrs, defStyle);
	}

	public void loadFromAsset(String atlasFileName, String skeletonFileName, SpineController controller) {
		this.controller = controller;
		loadFrom(() -> AndroidSkeletonDrawable.fromAsset(atlasFileName, skeletonFileName, getContext()));
	}

	public void loadFromFile(File atlasFile, File skeletonFile, SpineController controller) {
		this.controller = controller;
		loadFrom(() -> AndroidSkeletonDrawable.fromFile(atlasFile, skeletonFile));
	}

	public void loadFromHttp(URL atlasUrl, URL skeletonUrl, SpineController controller) {
		this.controller = controller;
		loadFrom(() -> AndroidSkeletonDrawable.fromHttp(atlasUrl, skeletonUrl));
	}

	private void loadFrom(AndroidSkeletonDrawableLoader loader) {
		Handler mainHandler = new Handler(Looper.getMainLooper());
		Thread backgroundThread = new Thread(() -> {
			final AndroidSkeletonDrawable skeletonDrawable = loader.load();
			mainHandler.post(() -> {
				controller.init(skeletonDrawable);
				Choreographer.getInstance().postFrameCallback(SpineView.this);
			});
		});
		backgroundThread.start();
	}

	@Override
	public void onDraw (@NonNull Canvas canvas) {
		super.onDraw(canvas);
		if (!controller.isInitialized()) {
			return;
		}

		controller.getDrawable().update(delta);

		// TODO: Calculate scaling + position

		renderer.render(canvas, controller.getSkeleton(), 500f, 1000f);
	}

	// Choreographer.FrameCallback

	@Override
	public void doFrame (long frameTimeNanos) {
		if (lastTime != 0) delta = (frameTimeNanos - lastTime) / 1e9f;
		lastTime = frameTimeNanos;
		invalidate();
		Choreographer.getInstance().postFrameCallback(this);
	}
}
