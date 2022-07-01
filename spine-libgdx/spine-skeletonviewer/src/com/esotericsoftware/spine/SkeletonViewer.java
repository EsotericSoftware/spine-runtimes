/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import java.lang.Thread.UncaughtExceptionHandler;
import java.lang.reflect.Field;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.Preferences;
import com.badlogic.gdx.backends.lwjgl3.Lwjgl3Application;
import com.badlogic.gdx.backends.lwjgl3.Lwjgl3ApplicationConfiguration;
import com.badlogic.gdx.backends.lwjgl3.Lwjgl3WindowAdapter;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Null;
import com.badlogic.gdx.utils.StringBuilder;
import com.badlogic.gdx.utils.viewport.ScreenViewport;

import com.esotericsoftware.spine.Animation.MixBlend;
import com.esotericsoftware.spine.AnimationState.AnimationStateAdapter;
import com.esotericsoftware.spine.AnimationState.TrackEntry;
import com.esotericsoftware.spine.utils.TwoColorPolygonBatch;

import java.awt.Toolkit;

public class SkeletonViewer extends ApplicationAdapter {
	static final String version = ""; // Replaced by build.
	static final float checkModifiedInterval = 0.250f;
	static final float reloadDelay = 1;
	static final String[] startSuffixes = {"", "-pro", "-ess"};
	static final String[] dataSuffixes = {".json", ".skel"};
	static final String[] endSuffixes = {"", ".txt", ".bytes"};
	static final String[] atlasSuffixes = {".atlas", "-pma.atlas"};
	static String[] args;
	static float uiScale = 1;

	Preferences prefs;
	TwoColorPolygonBatch batch;
	OrthographicCamera camera;
	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;
	SkeletonViewerUI ui;

	SkeletonViewerAtlas atlas;
	SkeletonData skeletonData;
	Skeleton skeleton;
	AnimationState state;
	FileHandle skeletonFile;
	long skeletonModified, atlasModified;
	float lastModifiedCheck, reloadTimer;
	final StringBuilder status = new StringBuilder();

	public void create () {
		Thread.setDefaultUncaughtExceptionHandler(new UncaughtExceptionHandler() {
			public void uncaughtException (Thread thread, Throwable ex) {
				System.out.println("Uncaught exception:");
				ex.printStackTrace();
				Runtime.getRuntime().halt(0); // Prevent Swing from keeping JVM alive.
			}
		});

		prefs = Gdx.app.getPreferences("spine-skeletonviewer");
		batch = new TwoColorPolygonBatch(3100);
		camera = new OrthographicCamera();
		renderer = new SkeletonRenderer();
		debugRenderer = new SkeletonRendererDebug();
		ui = new SkeletonViewerUI(this);
		resetCameraPosition();
		ui.loadPrefs();

		if (args.length == 0) {
			FileHandle file = Gdx.files
				.internal(Gdx.app.getPreferences("spine-skeletonviewer").getString("lastFile", "spineboy/spineboy.json"));
			if (file.exists()) loadSkeleton(file);
		} else
			loadSkeleton(Gdx.files.internal(args[0]));

		ui.loadPrefs();
		ui.prefsLoaded = true;
		setAnimation(true);

		if (false) {
			ui.animationList.clearListeners();
			// Test code:
			// state.setAnimation(0, "walk", true);
		}
	}

	boolean loadSkeleton (@Null FileHandle skeletonFile) {
		if (skeletonFile == null) return false;

		try {
			skeletonFile = new FileHandle(skeletonFile.file().getCanonicalFile());
		} catch (Throwable ex) {
			skeletonFile = new FileHandle(skeletonFile.file().getAbsoluteFile());
		}

		FileHandle oldSkeletonFile = this.skeletonFile;
		this.skeletonFile = skeletonFile;
		reloadTimer = 0;

		try {
			atlas = new SkeletonViewerAtlas(this, skeletonFile);

			// Load skeleton data.
			String extension = skeletonFile.extension();
			SkeletonLoader loader;
			if (extension.equalsIgnoreCase("json") || extension.equalsIgnoreCase("txt"))
				loader = new SkeletonJson(atlas);
			else
				loader = new SkeletonBinary(atlas);
			loader.setScale(ui.loadScaleSlider.getValue());
			skeletonData = loader.readSkeletonData(skeletonFile);
			if (skeletonData.getBones().size == 0) throw new Exception("No bones in skeleton data.");
		} catch (Throwable ex) {
			System.out.println("Error loading skeleton: " + skeletonFile.path());
			ex.printStackTrace();
			ui.toast("Error loading skeleton: " + skeletonFile.name());
			this.skeletonFile = oldSkeletonFile;
			return false;
		}

		skeleton = new Skeleton(skeletonData);
		skeleton.updateWorldTransform();
		skeleton.setToSetupPose();
		skeleton = new Skeleton(skeleton); // Tests copy constructors.
		skeleton.updateWorldTransform();

		state = new AnimationState(new AnimationStateData(skeletonData));
		state.addListener(new AnimationStateAdapter() {
			public void event (TrackEntry entry, Event event) {
				ui.toast(event.getData().getName());
			}
		});

		skeletonModified = skeletonFile.lastModified();
		atlasModified = atlas.lastModified();
		lastModifiedCheck = checkModifiedInterval;
		prefs.putString("lastFile", skeletonFile.path());
		prefs.flush();

		// Populate UI.

		ui.window.getTitleLabel().setText(skeletonFile.name());
		{
			Array<String> items = new Array();
			for (Skin skin : skeletonData.getSkins())
				items.add(skin.getName());
			ui.skinList.setItems(items);
		}
		{
			Array<String> items = new Array();
			for (Animation animation : skeletonData.getAnimations())
				items.add(animation.getName());
			ui.animationList.setItems(items);
		}
		ui.trackButtons.getButtons().first().setChecked(true);

		// Configure skeleton from UI.

		if (ui.skinList.getSelected() != null) skeleton.setSkin(ui.skinList.getSelected());
		setAnimation(true);
		return true;
	}

	void setAnimation (boolean first) {
		if (!ui.prefsLoaded) return;
		if (ui.animationList.getSelected() == null) return;
		int track = ui.trackButtons.getCheckedIndex();
		TrackEntry entry;
		if (!first && state.getCurrent(track) == null) {
			state.setEmptyAnimation(track, 0);
			entry = state.addAnimation(track, ui.animationList.getSelected(), ui.loopCheckbox.isChecked(), 0);
			entry.setMixDuration(ui.mixSlider.getValue());
		} else {
			entry = state.setAnimation(track, ui.animationList.getSelected(), ui.loopCheckbox.isChecked());
			entry.setHoldPrevious(track > 0 && ui.holdPrevCheckbox.isChecked());
		}
		entry.setMixBlend(ui.addCheckbox.isChecked() ? MixBlend.add : MixBlend.replace);
		entry.setReverse(ui.reverseCheckbox.isChecked());
		entry.setAlpha(ui.alphaSlider.getValue());
	}

	public void render () {
		Gdx.gl.glClearColor(112 / 255f, 111 / 255f, 118 / 255f, 1);
		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		float delta = Gdx.graphics.getDeltaTime();
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);
		debugRenderer.getShapeRenderer().setProjectionMatrix(camera.combined);

		// Draw skeleton origin lines.
		ShapeRenderer shapes = debugRenderer.getShapeRenderer();
		if (state != null) {
			shapes.setColor(Color.DARK_GRAY);
			shapes.begin(ShapeType.Line);
			shapes.line(0, -99999, 0, 99999);
			shapes.line(-99999, 0, 99999, 0);
			shapes.end();
		}

		if (skeleton != null) {
			// Reload if skeleton file was modified.
			if (reloadTimer <= 0) {
				lastModifiedCheck -= delta;
				if (lastModifiedCheck < 0) {
					lastModifiedCheck = checkModifiedInterval;
					long time = skeletonFile.lastModified();
					if (time != 0 && skeletonModified != time) reloadTimer = reloadDelay;
					time = atlas.lastModified();
					if (time != 0 && atlasModified != 0 && atlasModified != time) reloadTimer = reloadDelay;
				}
			} else {
				reloadTimer -= delta;
				if (reloadTimer <= 0 && loadSkeleton(skeletonFile)) ui.toast("Reloaded.");
			}

			// Pose and render skeleton.
			state.getData().setDefaultMix(ui.mixSlider.getValue());
			renderer.setPremultipliedAlpha(ui.pmaCheckbox.isChecked());
			batch.setPremultipliedAlpha(ui.pmaCheckbox.isChecked());

			float scaleX = ui.xScaleSlider.getValue(), scaleY = ui.yScaleSlider.getValue();
			if (skeleton.scaleX == 0) skeleton.scaleX = 0.01f;
			if (skeleton.scaleY == 0) skeleton.scaleY = 0.01f;
			skeleton.setScale(scaleX, scaleY);

			if (ui.setupPoseButton.isChecked())
				skeleton.setToSetupPose();
			else if (ui.bonesSetupPoseButton.isChecked())
				skeleton.setBonesToSetupPose();
			else if (ui.slotsSetupPoseButton.isChecked()) //
				skeleton.setSlotsToSetupPose();

			delta = Math.min(delta, 0.032f) * ui.speedSlider.getValue();
			state.update(delta);
			state.apply(skeleton);
			skeleton.updateWorldTransform();

			batch.begin();
			renderer.draw(batch, skeleton);
			batch.end();

			debugRenderer.setBones(ui.debugBonesCheckbox.isChecked());
			debugRenderer.setRegionAttachments(ui.debugRegionsCheckbox.isChecked());
			debugRenderer.setBoundingBoxes(ui.debugBoundingBoxesCheckbox.isChecked());
			debugRenderer.setMeshHull(ui.debugMeshHullCheckbox.isChecked());
			debugRenderer.setMeshTriangles(ui.debugMeshTrianglesCheckbox.isChecked());
			debugRenderer.setPaths(ui.debugPathsCheckbox.isChecked());
			debugRenderer.setPoints(ui.debugPointsCheckbox.isChecked());
			debugRenderer.setClipping(ui.debugClippingCheckbox.isChecked());
			debugRenderer.draw(skeleton);
		}

		if (state != null) {
			// AnimationState status.
			status.setLength(0);
			for (int i = state.getTracks().size - 1; i >= 0; i--) {
				TrackEntry entry = state.getTracks().get(i);
				if (entry == null) continue;
				status.append(i);
				status.append(": [LIGHT_GRAY]");
				status(entry);
				status.append("[WHITE]");
				status.append(entry.animation.name);
				status.append('\n');
			}
			ui.statusLabel.setText(status);
		}

		// Render UI.
		ui.render();

		// Draw indicator lines for animation and mix times.
		if (state != null) {
			TrackEntry entry = state.getCurrent(0);
			if (entry != null) {
				shapes.getProjectionMatrix().setToOrtho2D(0, 0, Gdx.graphics.getWidth(), Gdx.graphics.getHeight());
				shapes.updateMatrices();
				shapes.begin(ShapeType.Line);

				float percent = entry.getAnimationTime() / entry.getAnimationEnd();
				float x = ui.window.getRight() * uiScale + (Gdx.graphics.getWidth() - ui.window.getRight() * uiScale) * percent;
				shapes.setColor(Color.CYAN);
				shapes.line(x, 0, x, 12);

				percent = entry.getMixDuration() == 0 ? 1 : Math.min(1, entry.getMixTime() / entry.getMixDuration());
				x = ui.window.getRight() * uiScale + (Gdx.graphics.getWidth() - ui.window.getRight() * uiScale) * percent;
				shapes.setColor(Color.RED);
				shapes.line(x, 0, x, 12);

				shapes.end();
			}
		}
	}

	void status (TrackEntry entry) {
		TrackEntry from = entry.mixingFrom;
		if (from == null) return;
		status(from);
		status.append(from.animation.name);
		status.append(' ');
		status.append(Math.min(100, (int)(entry.mixTime / entry.mixDuration * 100)));
		status.append("% -> ");
	}

	void resetCameraPosition () {
		camera.position.x = -ui.window.getWidth() / 2 * uiScale;
		camera.position.y = Gdx.graphics.getHeight() / 4;
	}

	public void resize (int width, int height) {
		float x = camera.position.x, y = camera.position.y;
		camera.setToOrtho(false);
		camera.position.set(x, y, 0);
		((ScreenViewport)ui.stage.getViewport()).setUnitsPerPixel(1 / uiScale);
		ui.stage.getViewport().update(width, height, true);
		if (!ui.minimizeButton.isChecked()) ui.window.setHeight(height / uiScale + 8);
	}

	static public void main (String[] args) throws Exception {
		try { // Try to turn off illegal access log messages.
			Class loggerClass = Class.forName("jdk.internal.module.IllegalAccessLogger");
			Field loggerField = loggerClass.getDeclaredField("logger");
			Class unsafeClass = Class.forName("sun.misc.Unsafe");
			Field unsafeField = unsafeClass.getDeclaredField("theUnsafe");
			unsafeField.setAccessible(true);
			Object unsafe = unsafeField.get(null);
			Long offset = (Long)unsafeClass.getMethod("staticFieldOffset", Field.class).invoke(unsafe, loggerField);
			unsafeClass.getMethod("putObjectVolatile", Object.class, long.class, Object.class) //
				.invoke(unsafe, loggerClass, offset, null);
		} catch (Throwable ex) {
		}

		SkeletonViewer.args = args;

		String os = System.getProperty("os.name");
		float dpiScale = 1;
		if (os.contains("Windows")) dpiScale = Toolkit.getDefaultToolkit().getScreenResolution() / 96f;
		if (os.contains("OS X")) {
			Object object = Toolkit.getDefaultToolkit().getDesktopProperty("apple.awt.contentScaleFactor");
			if (object instanceof Float && ((Float)object).intValue() >= 2) dpiScale = 2;
		}
		if (dpiScale >= 2.0f) uiScale = 2;

		final SkeletonViewer skeletonViewer = new SkeletonViewer();
		Lwjgl3ApplicationConfiguration config = new Lwjgl3ApplicationConfiguration();
		config.disableAudio(true);
		config.setWindowedMode((int)(800 * uiScale), (int)(600 * uiScale));
		config.setTitle("Skeleton Viewer " + version);
		config.setBackBufferConfig(8, 8, 8, 8, 24, 0, 2);
		config.setWindowListener(new Lwjgl3WindowAdapter() {
			@Override
			public void filesDropped (String[] files) {
				for (String file : files) {
					for (String endSuffix : endSuffixes) {
						for (String dataSuffix : dataSuffixes) {
							if (file.endsWith(dataSuffix + endSuffix) && skeletonViewer.loadSkeleton(Gdx.files.absolute(file))) return;
						}
					}
				}
			}
		});
		new Lwjgl3Application(skeletonViewer, config);
	}
}
