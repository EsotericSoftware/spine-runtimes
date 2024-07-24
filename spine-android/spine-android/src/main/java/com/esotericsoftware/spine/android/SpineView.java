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
import com.esotericsoftware.spine.android.bounds.ContentMode;
import com.esotericsoftware.spine.android.bounds.SetupPoseBounds;
import com.esotericsoftware.spine.android.callbacks.AndroidSkeletonDrawableLoader;
import com.esotericsoftware.spine.Skeleton;

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

/** A {@link View} to display a Spine skeleton. The skeleton can be loaded from an asset bundle
 * ({@link SpineView#loadFromAssets(String, String, Context, SpineController)}), local files
 * ({@link SpineView#loadFromFile(File, File, Context, SpineController)}), URLs
 * ({@link SpineView#loadFromHttp(URL, URL, File, Context, SpineController)}), or a pre-loaded {@link AndroidSkeletonDrawable}
 * using ({@link SpineView#loadFromDrawable(AndroidSkeletonDrawable, Context, SpineController)}).
 *
 * The skeleton displayed by a {@link SpineView} can be controlled via a {@link SpineController}.
 *
 * The size of the widget can be derived from the bounds provided by a {@link BoundsProvider}. If the widget is not sized by the
 * bounds computed by the {@link BoundsProvider}, the widget will use the computed bounds to fit the skeleton inside the widget's
 * dimensions. */
public class SpineView extends View implements Choreographer.FrameCallback {

	/** Used to build {@link SpineView} instances. */
	public static class Builder {
		private final Context context;
		private final SpineController controller;
		private String atlasFileName;
		private String skeletonFileName;
		private File atlasFile;
		private File skeletonFile;
		private URL atlasUrl;
		private URL skeletonUrl;
		private File targetDirectory;
		private AndroidSkeletonDrawable drawable;
		private BoundsProvider boundsProvider = new SetupPoseBounds();
		private Alignment alignment = Alignment.CENTER;
		private ContentMode contentMode = ContentMode.FIT;

		/** Instantiate a {@link Builder} used to build a {@link SpineView}, which is a {@link View} to display a Spine skeleton.
		 *
		 * @param controller The skeleton displayed by a {@link SpineView} can be controlled via a {@link SpineController}. */
		public Builder (Context context, SpineController controller) {
			this.context = context;
			this.controller = controller;
		}

		/** Loads assets from your app assets for the {@link SpineView} if set. The {@code atlasFileName} specifies the `.atlas`
		 * file to be loaded for the images used to render the skeleton. The {@code skeletonFileName} specifies either a Skeleton
		 * `.json` or `.skel` file containing the skeleton data. */
		public Builder setLoadFromAssets (String atlasFileName, String skeletonFileName) {
			this.atlasFileName = atlasFileName;
			this.skeletonFileName = skeletonFileName;
			return this;
		}

		/** Loads assets from files for the {@link SpineView} if set. The {@code atlasFile} specifies the `.atlas` file to be loaded
		 * for the images used to render the skeleton. The {@code skeletonFile} specifies either a Skeleton `.json` or `.skel` file
		 * containing the skeleton data. */
		public Builder setLoadFromFile (File atlasFile, File skeletonFile) {
			this.atlasFile = atlasFile;
			this.skeletonFile = skeletonFile;
			return this;
		}

		/** Loads assets from http for the {@link SpineView} if set. The {@code atlasUrl} specifies the `.atlas` url to be loaded
		 * for the images used to render the skeleton. The {@code skeletonUrl} specifies either a Skeleton `.json` or `.skel` url
		 * containing the skeleton data. */
		public Builder setLoadFromHttp (URL atlasUrl, URL skeletonUrl, File targetDirectory) {
			this.atlasUrl = atlasUrl;
			this.skeletonUrl = skeletonUrl;
			this.targetDirectory = targetDirectory;
			return this;
		}

		/** Uses the {@link AndroidSkeletonDrawable} for the {@link SpineView} if set. */
		public Builder setLoadFromDrawable (AndroidSkeletonDrawable drawable) {
			this.drawable = drawable;
			return this;
		}

		/** Get the {@link BoundsProvider} used to compute the bounds of the {@link Skeleton} inside the view. The default is
		 * {@link SetupPoseBounds}. */
		public Builder setBoundsProvider (BoundsProvider boundsProvider) {
			this.boundsProvider = boundsProvider;
			return this;
		}

		/** Get the {@link ContentMode} used to fit the {@link Skeleton} inside the view. The default is {@link ContentMode#FIT}. */
		public Builder setContentMode (ContentMode contentMode) {
			this.contentMode = contentMode;
			return this;
		}

		/** Set the {@link Alignment} used to align the {@link Skeleton} inside the view. The default is {@link Alignment#CENTER} */
		public Builder setAlignment (Alignment alignment) {
			this.alignment = alignment;
			return this;
		}

		/** Builds a new {@link SpineView}.
		 *
		 * After initialization is complete, the provided {@code SpineController} is invoked as per the {@link SpineController}
		 * semantics, to allow modifying how the skeleton inside the widget is animated and rendered. */
		public SpineView build () {
			SpineView spineView = new SpineView(context, controller);
			spineView.boundsProvider = boundsProvider;
			spineView.alignment = alignment;
			spineView.contentMode = contentMode;
			if (atlasFileName != null && skeletonFileName != null) {
				spineView.loadFromAsset(atlasFileName, skeletonFileName);
			} else if (atlasFile != null && skeletonFile != null) {
				spineView.loadFromFile(atlasFile, skeletonFile);
			} else if (atlasUrl != null && skeletonUrl != null && targetDirectory != null) {
				spineView.loadFromHttp(atlasUrl, skeletonUrl, targetDirectory);
			} else if (drawable != null) {
				spineView.loadFromDrawable(drawable);
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
	private Boolean rendering = true;
	private Bounds computedBounds = new Bounds();

	private SpineController controller;
	private BoundsProvider boundsProvider = new SetupPoseBounds();
	private Alignment alignment = Alignment.CENTER;
	private ContentMode contentMode = ContentMode.FIT;

	/** Constructs a new {@link SpineView}.
	 *
	 * After initialization is complete, the provided {@code SpineController} is invoked as per the {@link SpineController}
	 * semantics, to allow modifying how the skeleton inside the widget is animated and rendered. */
	public SpineView (Context context, SpineController controller) {
		super(context);
		this.controller = controller;
	}

	/** Constructs a new {@link SpineView} without providing a {@link SpineController}, which you need to provide using
	 * {@link SpineView#setController(SpineController)}. */
	public SpineView (Context context, AttributeSet attrs) {
		super(context, attrs);
		// Set properties by view id
	}

	/** Constructs a new {@link SpineView} without providing a {@link SpineController}, which you need to provide using
	 * {@link SpineView#setController(SpineController)}. */
	public SpineView (Context context, AttributeSet attrs, int defStyle) {
		super(context, attrs, defStyle);
		// Set properties by view id
	}

	/** Constructs a new {@link SpineView} from files in your app assets. The {@code atlasFileName} specifies the `.atlas` file to
	 * be loaded for the images used to render the skeleton. The {@code skeletonFileName} specifies either a Skeleton `.json` or
	 * `.skel` file containing the skeleton data.
	 *
	 * After initialization is complete, the provided {@code controller} is invoked as per the {@link SpineController} semantics,
	 * to allow modifying how the skeleton inside the widget is animated and rendered. */
	public static SpineView loadFromAssets (String atlasFileName, String skeletonFileName, Context context,
		SpineController controller) {
		SpineView spineView = new SpineView(context, controller);
		spineView.loadFromAsset(atlasFileName, skeletonFileName);
		return spineView;
	}

	/** Constructs a new {@link SpineView} from files. The {@code atlasFile} specifies the `.atlas` file to be loaded for the
	 * images used to render the skeleton. The {@code skeletonFile} specifies either a Skeleton `.json` or `.skel` file containing
	 * the skeleton data.
	 *
	 * After initialization is complete, the provided {@code SpineController} is invoked as per the {@link SpineController}
	 * semantics, to allow modifying how the skeleton inside the widget is animated and rendered. */
	public static SpineView loadFromFile (File atlasFile, File skeletonFile, Context context, SpineController controller) {
		SpineView spineView = new SpineView(context, controller);
		spineView.loadFromFile(atlasFile, skeletonFile);
		return spineView;
	}

	/** Constructs a new {@link SpineView} from HTTP URLs. The {@code atlasUrl} specifies the `.atlas` url to be loaded for the
	 * images used to render the skeleton. The {@code skeletonUrl} specifies either a Skeleton `.json` or `.skel` url containing
	 * the skeleton data.
	 *
	 * After initialization is complete, the provided {@code SpineController} is invoked as per the {@link SpineController}
	 * semantics, to allow modifying how the skeleton inside the widget is animated and rendered. */
	public static SpineView loadFromHttp (URL atlasUrl, URL skeletonUrl, File targetDirectory, Context context,
		SpineController controller) {
		SpineView spineView = new SpineView(context, controller);
		spineView.loadFromHttp(atlasUrl, skeletonUrl, targetDirectory);
		return spineView;
	}

	/** Constructs a new {@link SpineView} from a {@link AndroidSkeletonDrawable}.
	 *
	 * After initialization is complete, the provided {@code SpineController} is invoked as per the {@link SpineController}
	 * semantics, to allow modifying how the skeleton inside the widget is animated and rendered. */
	public static SpineView loadFromDrawable (AndroidSkeletonDrawable drawable, Context context, SpineController controller) {
		SpineView spineView = new SpineView(context, controller);
		spineView.loadFromDrawable(drawable);
		return spineView;
	}

	/** The same as {@link SpineView#loadFromAssets(String, String, Context, SpineController)}, but can be used after instantiating
	 * the view via {@link SpineView#SpineView(Context, SpineController)}. */
	public void loadFromAsset (String atlasFileName, String skeletonFileName) {
		loadFrom( () -> AndroidSkeletonDrawable.fromAsset(atlasFileName, skeletonFileName, getContext()));
	}

	/** The same as {@link SpineView#loadFromFile(File, File, Context, SpineController)}, but can be used after instantiating the
	 * view via {@link SpineView#SpineView(Context, SpineController)}. */
	public void loadFromFile (File atlasFile, File skeletonFile) {
		loadFrom( () -> AndroidSkeletonDrawable.fromFile(atlasFile, skeletonFile));
	}

	/** The same as {@link SpineView#loadFromHttp(URL, URL, File, Context, SpineController)}, but can be used after instantiating
	 * the view via {@link SpineView#SpineView(Context, SpineController)}. */
	public void loadFromHttp (URL atlasUrl, URL skeletonUrl, File targetDirectory) {
		loadFrom( () -> AndroidSkeletonDrawable.fromHttp(atlasUrl, skeletonUrl, targetDirectory));
	}

	/** The same as {@link SpineView#loadFromDrawable(AndroidSkeletonDrawable, Context, SpineController)}, but can be used after
	 * instantiating the view via {@link SpineView#SpineView(Context, SpineController)}. */
	public void loadFromDrawable (AndroidSkeletonDrawable drawable) {
		loadFrom( () -> drawable);
	}

	/** Get the {@link SpineController} */
	public SpineController getController () {
		return controller;
	}

	/** Set the {@link SpineController}. Only do this if you use {@link SpineView#SpineView(Context, AttributeSet)},
	 * {@link SpineView#SpineView(Context, AttributeSet, int)}, or create the {@link SpineView} in an XML layout. */
	public void setController (SpineController controller) {
		this.controller = controller;
	}

	/** Get the {@link Alignment} used to align the {@link Skeleton} inside the view. The default is {@link Alignment#CENTER} */
	public Alignment getAlignment () {
		return alignment;
	}

	/** Set the {@link Alignment}. */
	public void setAlignment (Alignment alignment) {
		this.alignment = alignment;
		updateCanvasTransform();
	}

	/** Get the {@link ContentMode} used to fit the {@link Skeleton} inside the view. The default is {@link ContentMode#FIT}. */
	public ContentMode getContentMode () {
		return contentMode;
	}

	/** Set the {@link ContentMode}. */
	public void setContentMode (ContentMode contentMode) {
		this.contentMode = contentMode;
		updateCanvasTransform();
	}

	/** Get the {@link BoundsProvider} used to compute the bounds of the {@link Skeleton} inside the view. The default is
	 * {@link SetupPoseBounds}. */
	public BoundsProvider getBoundsProvider () {
		return boundsProvider;
	}

	/** Set the {@link BoundsProvider}. */
	public void setBoundsProvider (BoundsProvider boundsProvider) {
		this.boundsProvider = boundsProvider;
		updateCanvasTransform();
	}

	/** Check if rendering is enabled. */
	public Boolean isRendering () {
		return rendering;
	}

	/** Set to disable or enable rendering. Disable it when the spine view is out of bounds and you want to preserve CPU/GPU
	 * resources. */
	public void setRendering (Boolean rendering) {
		this.rendering = rendering;
	}

	private void loadFrom (AndroidSkeletonDrawableLoader loader) {
		Handler mainHandler = new Handler(Looper.getMainLooper());
		Thread backgroundThread = new Thread( () -> {
			final AndroidSkeletonDrawable skeletonDrawable = loader.load();
			mainHandler.post( () -> {
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
		if (controller == null || !controller.isInitialized() || !rendering) {
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
		renderer.renderToCanvas(canvas, commands);
		controller.callOnAfterPaint(canvas, commands);

		canvas.restore();
	}

	@Override
	protected void onSizeChanged (int w, int h, int oldw, int oldh) {
		super.onSizeChanged(w, h, oldw, oldh);
		updateCanvasTransform();
	}

	private void updateCanvasTransform () {
		if (controller == null) {
			return;
		}
		x = (float)(-computedBounds.getX() - computedBounds.getWidth() / 2.0
			- (alignment.getX() * computedBounds.getWidth() / 2.0));
		y = (float)(-computedBounds.getY() - computedBounds.getHeight() / 2.0
			- (alignment.getY() * computedBounds.getHeight() / 2.0));

		switch (contentMode) {
		case FIT:
			scaleX = scaleY = (float)Math.min(getWidth() / computedBounds.getWidth(), getHeight() / computedBounds.getHeight());
			break;
		case FILL:
			scaleX = scaleY = (float)Math.max(getWidth() / computedBounds.getWidth(), getHeight() / computedBounds.getHeight());
			break;
		}
		offsetX = (float)(getWidth() / 2.0 + (alignment.getX() * getWidth() / 2.0));
		offsetY = (float)(getHeight() / 2.0 + (alignment.getY() * getHeight() / 2.0));

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
