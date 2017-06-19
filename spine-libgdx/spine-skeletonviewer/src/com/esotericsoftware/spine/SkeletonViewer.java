/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.Interpolation.*;
import static com.badlogic.gdx.scenes.scene2d.actions.Actions.*;

import java.awt.FileDialog;
import java.awt.Frame;
import java.awt.Toolkit;
import java.io.File;
import java.lang.Thread.UncaughtExceptionHandler;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.InputAdapter;
import com.badlogic.gdx.InputMultiplexer;
import com.badlogic.gdx.Preferences;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.Pixmap;
import com.badlogic.gdx.graphics.Pixmap.Format;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.Texture.TextureFilter;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.TextureAtlasData;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.scenes.scene2d.Actor;
import com.badlogic.gdx.scenes.scene2d.InputEvent;
import com.badlogic.gdx.scenes.scene2d.InputListener;
import com.badlogic.gdx.scenes.scene2d.Stage;
import com.badlogic.gdx.scenes.scene2d.Touchable;
import com.badlogic.gdx.scenes.scene2d.ui.ButtonGroup;
import com.badlogic.gdx.scenes.scene2d.ui.CheckBox;
import com.badlogic.gdx.scenes.scene2d.ui.Image;
import com.badlogic.gdx.scenes.scene2d.ui.Label;
import com.badlogic.gdx.scenes.scene2d.ui.List;
import com.badlogic.gdx.scenes.scene2d.ui.ScrollPane;
import com.badlogic.gdx.scenes.scene2d.ui.Slider;
import com.badlogic.gdx.scenes.scene2d.ui.Table;
import com.badlogic.gdx.scenes.scene2d.ui.TextButton;
import com.badlogic.gdx.scenes.scene2d.ui.WidgetGroup;
import com.badlogic.gdx.scenes.scene2d.ui.Window;
import com.badlogic.gdx.scenes.scene2d.utils.ChangeListener;
import com.badlogic.gdx.scenes.scene2d.utils.ClickListener;
import com.badlogic.gdx.utils.Align;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.StringBuilder;
import com.badlogic.gdx.utils.viewport.ScreenViewport;
import com.esotericsoftware.spine.AnimationState.AnimationStateAdapter;
import com.esotericsoftware.spine.AnimationState.TrackEntry;
import com.esotericsoftware.spine.utils.TwoColorPolygonBatch;

public class SkeletonViewer extends ApplicationAdapter {
	static final float checkModifiedInterval = 0.250f;
	static final float reloadDelay = 1;
	static float uiScale = 1;
	static String[] atlasSuffixes = {".atlas", ".atlas.txt", "-pro.atlas", "-pro.atlas.txt", "-ess.atlas", "-ess.atlas.txt"};

	UI ui;

	OrthographicCamera camera;
	TwoColorPolygonBatch batch;
	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;
	SkeletonData skeletonData;
	Skeleton skeleton;
	AnimationState state;
	FileHandle skeletonFile;
	long lastModified;
	float lastModifiedCheck, reloadTimer;
	StringBuilder status = new StringBuilder();
	Preferences prefs;

	public void create () {
		Thread.setDefaultUncaughtExceptionHandler(new UncaughtExceptionHandler() {
			public void uncaughtException (Thread thread, Throwable ex) {
				ex.printStackTrace();
				Runtime.getRuntime().halt(0); // Prevent Swing from keeping JVM alive.
			}
		});

		prefs = Gdx.app.getPreferences("spine-skeletonviewer");
		ui = new UI();
		batch = new TwoColorPolygonBatch(3100);
		camera = new OrthographicCamera();
		renderer = new SkeletonRenderer();
		debugRenderer = new SkeletonRendererDebug();
		resetCameraPosition();
		ui.loadPrefs();

		loadSkeleton(
			Gdx.files.internal(Gdx.app.getPreferences("spine-skeletonviewer").getString("lastFile", "spineboy/spineboy.json")));

		ui.loadPrefs();
	}

	void loadSkeleton (final FileHandle skeletonFile) {
		if (skeletonFile == null) return;

		try {
			// Setup a texture atlas that uses a white image for images not found in the atlas.
			Pixmap pixmap = new Pixmap(32, 32, Format.RGBA8888);
			pixmap.setColor(new Color(1, 1, 1, 0.33f));
			pixmap.fill();
			final AtlasRegion fake = new AtlasRegion(new Texture(pixmap), 0, 0, 32, 32);
			pixmap.dispose();

			String atlasFileName = skeletonFile.nameWithoutExtension();
			if (atlasFileName.endsWith(".json")) atlasFileName = new FileHandle(atlasFileName).nameWithoutExtension();
			FileHandle atlasFile = skeletonFile.sibling(atlasFileName + ".atlas");
			if (!atlasFile.exists()) {
				if (atlasFileName.endsWith("-pro") || atlasFileName.endsWith("-ess"))
					atlasFileName = atlasFileName.substring(0, atlasFileName.length() - 4);
				for (String suffix : atlasSuffixes) {
					atlasFile = skeletonFile.sibling(atlasFileName + suffix);
					if (atlasFile.exists()) break;
				}
			}
			TextureAtlasData data = !atlasFile.exists() ? null : new TextureAtlasData(atlasFile, atlasFile.parent(), false);
			TextureAtlas atlas = new TextureAtlas(data) {
				public AtlasRegion findRegion (String name) {
					AtlasRegion region = super.findRegion(name);
					if (region == null) {
						// Look for separate image file.
						FileHandle file = skeletonFile.sibling(name + ".png");
						if (file.exists()) {
							Texture texture = new Texture(file);
							texture.setFilter(TextureFilter.Linear, TextureFilter.Linear);
							region = new AtlasRegion(texture, 0, 0, texture.getWidth(), texture.getHeight());
							region.name = name;
						}
					}
					return region != null ? region : fake;
				}
			};

			// Load skeleton data.
			String extension = skeletonFile.extension();
			if (extension.equalsIgnoreCase("json") || extension.equalsIgnoreCase("txt")) {
				SkeletonJson json = new SkeletonJson(atlas);
				json.setScale(ui.scaleSlider.getValue());
				skeletonData = json.readSkeletonData(skeletonFile);
			} else {
				SkeletonBinary binary = new SkeletonBinary(atlas);
				binary.setScale(ui.scaleSlider.getValue());
				skeletonData = binary.readSkeletonData(skeletonFile);
				if (skeletonData.getBones().size == 0) throw new Exception("No bones in skeleton data.");
			}
		} catch (Exception ex) {
			ex.printStackTrace();
			ui.toast("Error loading skeleton: " + skeletonFile.name());
			lastModifiedCheck = 5;
			return;
		}

		skeleton = new Skeleton(skeletonData);
		skeleton.setToSetupPose();
		skeleton = new Skeleton(skeleton); // Tests copy constructors.
		skeleton.updateWorldTransform();

		state = new AnimationState(new AnimationStateData(skeletonData));
		state.addListener(new AnimationStateAdapter() {

			public void event (TrackEntry entry, Event event) {
				ui.toast(event.getData().getName());
			}
		});

		this.skeletonFile = skeletonFile;
		prefs.putString("lastFile", skeletonFile.path());
		prefs.flush();
		lastModified = skeletonFile.lastModified();
		lastModifiedCheck = checkModifiedInterval;

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
		setAnimation();

		// ui.animationList.clearListeners();
		// state.setAnimation(0, "walk", true);
	}

	void setAnimation () {
		if (ui.animationList.getSelected() == null) return;
		int track = ui.trackButtons.getCheckedIndex();
		TrackEntry current = state.getCurrent(track);
		TrackEntry entry;
		if (current == null) {
			state.setEmptyAnimation(track, 0);
			entry = state.addAnimation(track, ui.animationList.getSelected(), ui.loopCheckbox.isChecked(), 0);
			entry.setMixDuration(ui.mixSlider.getValue());
		} else {
			entry = state.setAnimation(track, ui.animationList.getSelected(), ui.loopCheckbox.isChecked());
		}
		entry.setAlpha(ui.alphaSlider.getValue());
	}

	public void render () {
		Gdx.gl.glClearColor(0.3f, 0.3f, 0.3f, 1);
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
					if (time != 0 && lastModified != time) reloadTimer = reloadDelay;
				}
			} else {
				reloadTimer -= delta;
				if (reloadTimer <= 0) {
					loadSkeleton(skeletonFile);
					ui.toast("Reloaded.");
				}
			}

			// Pose and render skeleton.
			state.getData().setDefaultMix(ui.mixSlider.getValue());
			renderer.setPremultipliedAlpha(ui.premultipliedCheckbox.isChecked());

			skeleton.setFlip(ui.flipXCheckbox.isChecked(), ui.flipYCheckbox.isChecked());

			delta = Math.min(delta, 0.032f) * ui.speedSlider.getValue();
			skeleton.update(delta);
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
			for (int i = 0, n = state.getTracks().size; i < n; i++) {
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

	class UI {
		Stage stage = new Stage(new ScreenViewport());
		com.badlogic.gdx.scenes.scene2d.ui.Skin skin = new com.badlogic.gdx.scenes.scene2d.ui.Skin(
			Gdx.files.internal("skin/skin.json"));

		Window window = new Window("Skeleton", skin);
		Table root = new Table(skin);
		TextButton openButton = new TextButton("Open", skin);
		TextButton minimizeButton = new TextButton("-", skin);

		Slider scaleSlider = new Slider(0.1f, 3, 0.01f, false, skin);
		Label scaleLabel = new Label("1.0", skin);
		TextButton scaleResetButton = new TextButton("Reset", skin);

		Slider zoomSlider = new Slider(0.01f, 10, 0.01f, false, skin);
		Label zoomLabel = new Label("1.0", skin);
		TextButton zoomResetButton = new TextButton("Reset", skin);

		CheckBox flipXCheckbox = new CheckBox("X", skin);
		CheckBox flipYCheckbox = new CheckBox("Y", skin);

		CheckBox debugBonesCheckbox = new CheckBox("Bones", skin);
		CheckBox debugRegionsCheckbox = new CheckBox("Regions", skin);
		CheckBox debugBoundingBoxesCheckbox = new CheckBox("Bounds", skin);
		CheckBox debugMeshHullCheckbox = new CheckBox("Mesh hull", skin);
		CheckBox debugMeshTrianglesCheckbox = new CheckBox("Triangles", skin);
		CheckBox debugPathsCheckbox = new CheckBox("Paths", skin);
		CheckBox debugPointsCheckbox = new CheckBox("Points", skin);
		CheckBox debugClippingCheckbox = new CheckBox("Clipping", skin);

		CheckBox premultipliedCheckbox = new CheckBox("Premultiplied", skin);

		TextButton bonesSetupPoseButton = new TextButton("Bones", skin);
		TextButton slotsSetupPoseButton = new TextButton("Slots", skin);
		TextButton setupPoseButton = new TextButton("Both", skin);

		List<String> skinList = new List(skin);
		ScrollPane skinScroll = new ScrollPane(skinList, skin, "bg");

		ButtonGroup<TextButton> trackButtons = new ButtonGroup();
		CheckBox loopCheckbox = new CheckBox("Loop", skin);

		Slider alphaSlider = new Slider(0, 1, 0.01f, false, skin);
		Label alphaLabel = new Label("1.0", skin);

		List<String> animationList = new List(skin);
		ScrollPane animationScroll = new ScrollPane(animationList, skin, "bg");

		Slider speedSlider = new Slider(0, 3, 0.01f, false, skin);
		Label speedLabel = new Label("1.0", skin);
		TextButton speedResetButton = new TextButton("Reset", skin);

		Slider mixSlider = new Slider(0, 4, 0.01f, false, skin);
		Label mixLabel = new Label("0.3", skin);

		Label statusLabel = new Label("", skin);
		WidgetGroup toasts = new WidgetGroup();
		boolean prefsLoaded;

		UI () {
			initialize();
			layout();
			events();
		}

		void initialize () {
			skin.getFont("default").getData().markupEnabled = true;

			for (int i = 0; i < 6; i++)
				trackButtons.add(new TextButton(i + "", skin, "toggle"));

			animationList.getSelection().setRequired(false);

			premultipliedCheckbox.setChecked(true);

			loopCheckbox.setChecked(true);

			scaleSlider.setValue(1);
			scaleSlider.setSnapToValues(new float[] {0.5f, 1, 1.5f, 2, 2.5f, 3, 3.5f}, 0.01f);

			zoomSlider.setValue(1);
			zoomSlider.setSnapToValues(new float[] {0.5f, 1, 1.5f, 2, 2.5f, 3, 3.5f}, 0.01f);

			mixSlider.setValue(0.3f);
			mixSlider.setSnapToValues(new float[] {1, 1.5f, 2, 2.5f, 3, 3.5f}, 0.1f);

			speedSlider.setValue(1);
			speedSlider.setSnapToValues(new float[] {0.5f, 0.75f, 1, 1.25f, 1.5f, 2, 2.5f}, 0.01f);

			alphaSlider.setValue(1);
			alphaSlider.setDisabled(true);

			window.setMovable(false);
			window.setResizable(false);
			window.setKeepWithinStage(false);
			window.setX(-3);
			window.setY(-2);

			window.getTitleLabel().setColor(new Color(0xc1ffffff));
			window.getTitleTable().add(openButton).space(3);
			window.getTitleTable().add(minimizeButton).width(20);

			skinScroll.setFadeScrollBars(false);

			animationScroll.setFadeScrollBars(false);
		}

		void layout () {
			root.defaults().space(6);
			root.columnDefaults(0).top().right().padTop(3);
			root.columnDefaults(1).left();
			root.add("Scale:");
			{
				Table table = table();
				table.add(scaleLabel).width(29);
				table.add(scaleSlider).growX();
				table.add(scaleResetButton);
				root.add(table).fill().row();
			}
			root.add("Zoom:");
			{
				Table table = table();
				table.add(zoomLabel).width(29);
				table.add(zoomSlider).growX();
				table.add(zoomResetButton);
				root.add(table).fill().row();
			}
			root.add("Flip:");
			root.add(table(flipXCheckbox, flipYCheckbox)).row();
			root.add("Debug:");
			root.add(table(debugBonesCheckbox, debugRegionsCheckbox, debugBoundingBoxesCheckbox)).row();
			root.add();
			root.add(table(debugPathsCheckbox, debugPointsCheckbox, debugClippingCheckbox)).row();
			root.add();
			root.add(table(debugMeshHullCheckbox, debugMeshTrianglesCheckbox)).row();
			root.add("Atlas alpha:");
			root.add(premultipliedCheckbox).row();

			root.add(new Image(skin.newDrawable("white", new Color(0x4e4e4eff)))).height(1).fillX().colspan(2).pad(-3, 0, 1, 0)
				.row();

			root.add("Setup pose:");
			root.add(table(bonesSetupPoseButton, slotsSetupPoseButton, setupPoseButton)).row();
			root.add("Skin:");
			root.add(skinScroll).grow().minHeight(64).row();

			root.add(new Image(skin.newDrawable("white", new Color(0x4e4e4eff)))).height(1).fillX().colspan(2).pad(1, 0, 1, 0).row();

			root.add("Track:");
			{
				Table table = table();
				for (TextButton button : trackButtons.getButtons())
					table.add(button);
				table.add(loopCheckbox);
				root.add(table).row();
			}
			root.add("Entry alpha:");
			{
				Table table = table();
				table.add(alphaLabel).width(29);
				table.add(alphaSlider).growX();
				root.add(table).fill().row();
			}
			root.add("Animation:");
			root.add(animationScroll).grow().minHeight(64).row();
			root.add("Speed:");
			{
				Table table = table();
				table.add(speedLabel).width(29);
				table.add(speedSlider).growX();
				table.add(speedResetButton);
				root.add(table).fill().row();
			}
			root.add("Default mix:");
			{
				Table table = table();
				table.add(mixLabel).width(29);
				table.add(mixSlider).growX();
				root.add(table).fill().row();
			}

			window.add(root).grow();
			window.pack();
			stage.addActor(window);

			stage.addActor(statusLabel);

			{
				Table table = new Table();
				table.setFillParent(true);
				table.setTouchable(Touchable.disabled);
				stage.addActor(table);
				table.pad(10, 10, 22, 10).bottom().right();
				table.add(toasts);
			}

			{
				Table table = new Table();
				table.setFillParent(true);
				table.setTouchable(Touchable.disabled);
				stage.addActor(table);
				table.pad(10).top().right();
				table.defaults().right();
				table.add(new Label("", skin, "default", Color.LIGHT_GRAY)); // Version.
			}
		}

		void events () {
			window.addListener(new InputListener() {
				public boolean touchDown (InputEvent event, float x, float y, int pointer, int button) {
					event.cancel();
					return true;
				}
			});

			openButton.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					FileDialog fileDialog = new FileDialog((Frame)null, "Choose skeleton file");
					fileDialog.setMode(FileDialog.LOAD);
					fileDialog.setVisible(true);
					String name = fileDialog.getFile();
					String dir = fileDialog.getDirectory();
					if (name == null || dir == null) return;
					loadSkeleton(new FileHandle(new File(dir, name).getAbsolutePath()));
				}
			});

			setupPoseButton.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					if (skeleton != null) skeleton.setToSetupPose();
				}
			});
			bonesSetupPoseButton.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					if (skeleton != null) skeleton.setBonesToSetupPose();
				}
			});
			slotsSetupPoseButton.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					if (skeleton != null) skeleton.setSlotsToSetupPose();
				}
			});

			minimizeButton.addListener(new ClickListener() {
				public boolean touchDown (InputEvent event, float x, float y, int pointer, int button) {
					event.cancel();
					return super.touchDown(event, x, y, pointer, button);
				}

				public void clicked (InputEvent event, float x, float y) {
					if (minimizeButton.isChecked()) {
						window.getCells().get(0).setActor(null);
						window.setHeight(37);
						minimizeButton.setText("+");
					} else {
						window.getCells().get(0).setActor(root);
						window.setHeight(Gdx.graphics.getHeight() / uiScale + 8);
						minimizeButton.setText("-");
					}
				}
			});

			scaleSlider.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					scaleLabel.setText(Float.toString((int)(scaleSlider.getValue() * 100) / 100f));
					if (!scaleSlider.isDragging()) loadSkeleton(skeletonFile);
				}
			});
			scaleResetButton.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					resetCameraPosition();
					if (scaleSlider.getValue() == 1)
						loadSkeleton(skeletonFile);
					else
						scaleSlider.setValue(1);
				}
			});

			zoomSlider.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					zoomLabel.setText(Float.toString((int)(zoomSlider.getValue() * 100) / 100f));
					float newZoom = 1 / zoomSlider.getValue();
					camera.position.x -= window.getWidth() / 2 * (newZoom - camera.zoom);
					camera.zoom = newZoom;
				}
			});
			zoomResetButton.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					resetCameraPosition();
					float x = camera.position.x;
					zoomSlider.setValue(1);
					camera.position.x = x;
				}
			});

			speedSlider.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					speedLabel.setText(Float.toString((int)(speedSlider.getValue() * 100) / 100f));
				}
			});
			speedResetButton.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					speedSlider.setValue(1);
				}
			});

			alphaSlider.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					alphaLabel.setText(Float.toString((int)(alphaSlider.getValue() * 100) / 100f));
					int track = trackButtons.getCheckedIndex();
					if (track > 0) {
						TrackEntry current = state.getCurrent(track);
						if (current != null) {
							current.setAlpha(alphaSlider.getValue());
							current.resetRotationDirections();
						}
					}
				}
			});

			mixSlider.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					mixLabel.setText(Float.toString((int)(mixSlider.getValue() * 100) / 100f));
					if (state != null) state.getData().setDefaultMix(mixSlider.getValue());
				}
			});

			animationList.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					if (state != null) {
						String name = animationList.getSelected();
						if (name == null)
							state.setEmptyAnimation(trackButtons.getCheckedIndex(), mixSlider.getValue());
						else
							setAnimation();
					}
				}
			});

			loopCheckbox.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					setAnimation();
				}
			});

			skinList.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					if (skeleton != null) {
						String skinName = skinList.getSelected();
						if (skinName == null)
							skeleton.setSkin((Skin)null);
						else
							skeleton.setSkin(skinName);
						skeleton.setSlotsToSetupPose();
					}
				}
			});

			ChangeListener trackButtonListener = new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					int track = trackButtons.getCheckedIndex();
					if (track == -1) return;
					TrackEntry current = state.getCurrent(track);
					animationList.getSelection().setProgrammaticChangeEvents(false);
					animationList.setSelected(current == null ? null : current.animation.name);
					animationList.getSelection().setProgrammaticChangeEvents(true);

					alphaSlider.setDisabled(track == 0);
					alphaSlider.setValue(current == null ? 1 : current.alpha);

					if (current != null) loopCheckbox.setChecked(current.getLoop());
				}
			};
			for (TextButton button : trackButtons.getButtons())
				button.addListener(trackButtonListener);

			Gdx.input.setInputProcessor(new InputMultiplexer(stage, new InputAdapter() {
				float offsetX;
				float offsetY;

				public boolean touchDown (int screenX, int screenY, int pointer, int button) {
					offsetX = screenX;
					offsetY = Gdx.graphics.getHeight() - screenY;
					return false;
				}

				public boolean touchDragged (int screenX, int screenY, int pointer) {
					float deltaX = screenX - offsetX;
					float deltaY = Gdx.graphics.getHeight() - screenY - offsetY;

					camera.position.x -= deltaX * camera.zoom;
					camera.position.y -= deltaY * camera.zoom;

					offsetX = screenX;
					offsetY = Gdx.graphics.getHeight() - screenY;
					return false;
				}

				public boolean touchUp (int screenX, int screenY, int pointer, int button) {
					savePrefs();
					return false;
				}

				public boolean scrolled (int amount) {
					float zoom = zoomSlider.getValue(), zoomMin = zoomSlider.getMinValue(), zoomMax = zoomSlider.getMaxValue();
					float speedAlpha = Math.min(1.2f, (zoom - zoomMin) / (zoomMax - zoomMin) * 3.5f);
					zoom -= linear.apply(0.02f, 0.2f, speedAlpha) * Math.signum(amount);
					zoomSlider.setValue(MathUtils.clamp(zoom, zoomMin, zoomMax));
					return false;
				}
			}));

			ChangeListener savePrefsListener = new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					if (actor instanceof Slider && ((Slider)actor).isDragging()) return;
					savePrefs();
				}
			};
			debugBonesCheckbox.addListener(savePrefsListener);
			debugRegionsCheckbox.addListener(savePrefsListener);
			debugMeshHullCheckbox.addListener(savePrefsListener);
			debugMeshTrianglesCheckbox.addListener(savePrefsListener);
			debugPathsCheckbox.addListener(savePrefsListener);
			debugPointsCheckbox.addListener(savePrefsListener);
			debugClippingCheckbox.addListener(savePrefsListener);
			premultipliedCheckbox.addListener(savePrefsListener);
			loopCheckbox.addListener(savePrefsListener);
			speedSlider.addListener(savePrefsListener);
			speedResetButton.addListener(savePrefsListener);
			mixSlider.addListener(savePrefsListener);
			scaleSlider.addListener(savePrefsListener);
			scaleResetButton.addListener(savePrefsListener);
			zoomSlider.addListener(savePrefsListener);
			zoomResetButton.addListener(savePrefsListener);
			animationList.addListener(savePrefsListener);
			skinList.addListener(savePrefsListener);
		}

		Table table (Actor... actors) {
			Table table = new Table();
			table.defaults().space(6);
			table.add(actors);
			return table;
		}

		void render () {
			if (state != null && state.getCurrent(trackButtons.getCheckedIndex()) == null) {
				animationList.getSelection().setProgrammaticChangeEvents(false);
				animationList.setSelected(null);
				animationList.getSelection().setProgrammaticChangeEvents(true);
			}

			statusLabel.pack();
			if (minimizeButton.isChecked())
				statusLabel.setPosition(10, 25, Align.bottom | Align.left);
			else
				statusLabel.setPosition(window.getWidth() + 6, 5, Align.bottom | Align.left);

			stage.act();
			stage.draw();
		}

		void toast (String text) {
			Table table = new Table();
			table.add(new Label(text, skin));
			table.getColor().a = 0;
			table.pack();
			table.setPosition(-table.getWidth(), -3 - table.getHeight());
			table.addAction(sequence( //
				parallel(moveBy(0, table.getHeight(), 0.3f), fadeIn(0.3f)), //
				delay(5f), //
				parallel(moveBy(0, table.getHeight(), 0.3f), fadeOut(0.3f)), //
				removeActor() //
			));
			for (Actor actor : toasts.getChildren())
				actor.addAction(moveBy(0, table.getHeight(), 0.3f));
			toasts.addActor(table);
			toasts.getParent().toFront();
		}

		void savePrefs () {
			if (!prefsLoaded) return;
			prefs.putBoolean("debugBones", debugBonesCheckbox.isChecked());
			prefs.putBoolean("debugRegions", debugRegionsCheckbox.isChecked());
			prefs.putBoolean("debugMeshHull", debugMeshHullCheckbox.isChecked());
			prefs.putBoolean("debugMeshTriangles", debugMeshTrianglesCheckbox.isChecked());
			prefs.putBoolean("debugPaths", debugPathsCheckbox.isChecked());
			prefs.putBoolean("debugPoints", debugPointsCheckbox.isChecked());
			prefs.putBoolean("debugClipping", debugClippingCheckbox.isChecked());
			prefs.putBoolean("premultiplied", premultipliedCheckbox.isChecked());
			prefs.putBoolean("loop", loopCheckbox.isChecked());
			prefs.putFloat("speed", speedSlider.getValue());
			prefs.putFloat("mix", mixSlider.getValue());
			prefs.putFloat("scale", scaleSlider.getValue());
			prefs.putFloat("zoom", zoomSlider.getValue());
			prefs.putFloat("x", camera.position.x);
			prefs.putFloat("y", camera.position.y);
			if (state != null) {
				TrackEntry current = state.getCurrent(0);
				if (current != null) {
					String name = current.animation.name;
					if (name.equals("<empty>")) name = current.next == null ? "" : current.next.animation.name;
					prefs.putString("animationName", name);
				}
			}
			if (skinList.getSelected() != null) prefs.putString("skinName", skinList.getSelected());
			prefs.flush();
		}

		void loadPrefs () {
			debugBonesCheckbox.setChecked(prefs.getBoolean("debugBones", true));
			debugRegionsCheckbox.setChecked(prefs.getBoolean("debugRegions", false));
			debugMeshHullCheckbox.setChecked(prefs.getBoolean("debugMeshHull", false));
			debugMeshTrianglesCheckbox.setChecked(prefs.getBoolean("debugMeshTriangles", false));
			debugPathsCheckbox.setChecked(prefs.getBoolean("debugPaths", true));
			debugPointsCheckbox.setChecked(prefs.getBoolean("debugPoints", true));
			debugClippingCheckbox.setChecked(prefs.getBoolean("debugClipping", true));
			premultipliedCheckbox.setChecked(prefs.getBoolean("premultiplied", true));
			loopCheckbox.setChecked(prefs.getBoolean("loop", false));
			speedSlider.setValue(prefs.getFloat("speed", 0.3f));
			mixSlider.setValue(prefs.getFloat("mix", 0.3f));

			zoomSlider.setValue(prefs.getFloat("zoom", 1));
			camera.zoom = 1 / prefs.getFloat("zoom", 1);
			camera.position.x = prefs.getFloat("x", 0);
			camera.position.y = prefs.getFloat("y", 0);

			scaleSlider.setValue(prefs.getFloat("scale", 1));
			animationList.setSelected(prefs.getString("animationName", null));
			skinList.setSelected(prefs.getString("skinName", null));
			prefsLoaded = true;
		}
	}

	static public void main (String[] args) throws Exception {
		String os = System.getProperty("os.name");
		float dpiScale = 1;
		if (os.contains("Windows")) dpiScale = Toolkit.getDefaultToolkit().getScreenResolution() / 96f;
		if (os.contains("OS X")) {
			Object object = Toolkit.getDefaultToolkit().getDesktopProperty("apple.awt.contentScaleFactor");
			if (object instanceof Float && ((Float)object).intValue() >= 2) dpiScale = 2;
		}
		if (dpiScale >= 2.0f) uiScale = 2;

		LwjglApplicationConfiguration.disableAudio = true;
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		config.width = (int)(800 * uiScale);
		config.height = (int)(600 * uiScale);
		config.title = "Skeleton Viewer";
		config.allowSoftwareMode = true;
		new LwjglApplication(new SkeletonViewer(), config);
	}
}
