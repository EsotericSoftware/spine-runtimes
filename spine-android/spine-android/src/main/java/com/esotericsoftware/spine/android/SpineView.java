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

import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.android.bounds.Alignment;
import com.esotericsoftware.spine.android.bounds.Bounds;
import com.esotericsoftware.spine.android.bounds.BoundsProvider;
import com.esotericsoftware.spine.android.bounds.SetupPoseBounds;
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

	public static class Builder {
		private final Context context;

		private final SpineController controller;

		private String atlasFileName;

		private String skeletonFileName;

		private BoundsProvider boundsProvider = new SetupPoseBounds();
		private Alignment alignment = Alignment.CENTER;

		public Builder(Context context, SpineController controller) {
			this.context = context;
			this.controller = controller;
		}

		public Builder setLoadFromAssets(String atlasFileName, String skeletonFileName) {
			this.atlasFileName = atlasFileName;
			this.skeletonFileName = skeletonFileName;
			return this;
		}

		public Builder setBoundsProvider(BoundsProvider boundsProvider) {
			this.boundsProvider = boundsProvider;
			return this;
		}

		public Builder setAlignment(Alignment alignment) {
			this.alignment = alignment;
			return this;
		}

		public SpineView build() {
			SpineView spineView = new SpineView(context, controller);
			spineView.boundsProvider = boundsProvider;
			spineView.alignment = alignment;
			if (atlasFileName != null && skeletonFileName != null) {
				spineView.loadFromAsset(atlasFileName, skeletonFileName);
			}
			return spineView;
		}
	}

	private long lastTime = 0;
	private float delta = 0;
	private float offsetX = 0;
	private float offsetY = 0;
	private float scaleX = 1;
	private float scaleY = 1;
	private float x = 0;
	private float y = 0;

	private final SkeletonRenderer renderer = new SkeletonRenderer();
	private Bounds computedBounds = new Bounds();

	SpineController controller;

	BoundsProvider boundsProvider = new SetupPoseBounds();

	Alignment alignment = Alignment.CENTER;

	public SpineView (Context context, SpineController controller) {
		super(context);
		this.controller = controller;
	}

	public SpineView (Context context, AttributeSet attrs) {
		super(context, attrs);
		this.controller = new SpineController();
	}

	public SpineView (Context context, AttributeSet attrs, int defStyle) {
		super(context, attrs, defStyle);
		this.controller = new SpineController();
	}

	public static SpineView loadFromAssets(String atlasFileName, String skeletonFileName, Context context, SpineController controller) {
		SpineView spineView = new SpineView(context, controller);
		spineView.loadFromAsset(atlasFileName, skeletonFileName);
		return spineView;
	}

	public static SpineView loadFromDrawable(AndroidSkeletonDrawable drawable, Context context, SpineController controller) {
		SpineView spineView = new SpineView(context, controller);
		spineView.loadFromDrawable(drawable);
		return spineView;
	}

	public void setController(SpineController controller) {
		this.controller = controller;
	}

	public void loadFromAsset(String atlasFileName, String skeletonFileName) {
		loadFrom(() -> AndroidSkeletonDrawable.fromAsset(atlasFileName, skeletonFileName, getContext()));
	}

	public void loadFromFile(File atlasFile, File skeletonFile) {
		loadFrom(() -> AndroidSkeletonDrawable.fromFile(atlasFile, skeletonFile));
	}

	public void loadFromHttp(URL atlasUrl, URL skeletonUrl) {
		loadFrom(() -> AndroidSkeletonDrawable.fromHttp(atlasUrl, skeletonUrl));
	}

	public void loadFromDrawable(AndroidSkeletonDrawable drawable) {
		loadFrom(() -> drawable);
	}

	private void loadFrom(AndroidSkeletonDrawableLoader loader) {
		Handler mainHandler = new Handler(Looper.getMainLooper());
		Thread backgroundThread = new Thread(() -> {
			final AndroidSkeletonDrawable skeletonDrawable = loader.load();
			mainHandler.post(() -> {
				computedBounds = boundsProvider.computeBounds(skeletonDrawable);
				updateCanvasTransform();

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

		if (controller.isPlaying()) {
            controller.callOnBeforeUpdateWorldTransforms();
			controller.getDrawable().update(delta);
			controller.callOnAfterUpdateWorldTransforms();
		}

		canvas.save();

		canvas.translate(offsetX, offsetY);
		canvas.scale(scaleX, scaleY * -1);
		canvas.translate(x, y);

		controller.callOnBeforePaint(canvas);
		Array<SkeletonRenderer.RenderCommand> commands = renderer.render(controller.getSkeleton());
		renderer.render(canvas, commands);
		controller.callOnAfterPaint(canvas, commands);

		canvas.restore();
	}

	@Override
	protected void onSizeChanged(int w, int h, int oldw, int oldh) {
		super.onSizeChanged(w, h, oldw, oldh);
		updateCanvasTransform();
	}

	private void updateCanvasTransform() {
		x = (float) (-computedBounds.getX() - computedBounds.getWidth() / 2.0 - (alignment.getX() * computedBounds.getWidth() / 2.0));
		y = (float) (-computedBounds.getY() - computedBounds.getHeight() / 2.0 - (alignment.getY() * computedBounds.getHeight() / 2.0));

		// contain
		scaleX = scaleY = (float) Math.min(getWidth() / computedBounds.getWidth(), getHeight() / computedBounds.getHeight());

		offsetX = (float) (getWidth() / 2.0 + (alignment.getX() * getWidth() / 2.0));
		offsetY = (float) (getHeight() / 2.0 + (alignment.getY() * getHeight() / 2.0));

		controller.setCoordinateTransform(x + offsetX / scaleX, y + offsetY / scaleY, scaleX, scaleY);
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
