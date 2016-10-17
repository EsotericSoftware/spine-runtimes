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

import static com.badlogic.gdx.scenes.scene2d.actions.Actions.*;

import java.awt.FileDialog;
import java.awt.Frame;
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
import com.badlogic.gdx.graphics.Pixmap;
import com.badlogic.gdx.graphics.Pixmap.Format;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.Texture.TextureFilter;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.TextureAtlasData;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.scenes.scene2d.Actor;
import com.badlogic.gdx.scenes.scene2d.InputEvent;
import com.badlogic.gdx.scenes.scene2d.InputListener;
import com.badlogic.gdx.scenes.scene2d.Stage;
import com.badlogic.gdx.scenes.scene2d.Touchable;
import com.badlogic.gdx.scenes.scene2d.ui.CheckBox;
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
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.viewport.ScreenViewport;
import com.esotericsoftware.spine.AnimationState.TrackEntry;

public class SkeletonViewer extends ApplicationAdapter {
	static final float checkModifiedInterval = 0.250f;
	static final float reloadDelay = 1;

	UI ui;

	PolygonSpriteBatch batch;
	SkeletonMeshRenderer renderer;
	SkeletonRendererDebug debugRenderer;
	SkeletonData skeletonData;
	Skeleton skeleton;
	AnimationState state;
	int skeletonX, skeletonY;
	FileHandle skeletonFile;
	long lastModified;
	float lastModifiedCheck, reloadTimer;

	public void create () {
		Thread.setDefaultUncaughtExceptionHandler(new UncaughtExceptionHandler() {
			public void uncaughtException (Thread thread, Throwable ex) {
				ex.printStackTrace();
				Runtime.getRuntime().halt(0); // Prevent Swing from keeping JVM alive.
			}
		});

		ui = new UI();
		batch = new PolygonSpriteBatch();
		renderer = new SkeletonMeshRenderer();
		debugRenderer = new SkeletonRendererDebug();
		skeletonX = (int)(ui.window.getWidth() + (Gdx.graphics.getWidth() - ui.window.getWidth()) / 2);
		skeletonY = Gdx.graphics.getHeight() / 4;

		loadSkeleton(
			Gdx.files.internal(Gdx.app.getPreferences("spine-skeletonviewer").getString("lastFile", "spineboy/spineboy.json")),
			false);
	}

	void loadSkeleton (final FileHandle skeletonFile, boolean reload) {
		if (skeletonFile == null) return;

		try {
			// A regular texture atlas would normally usually be used. This returns a white image for images not found in the atlas.
			Pixmap pixmap = new Pixmap(32, 32, Format.RGBA8888);
			pixmap.setColor(new Color(1, 1, 1, 0.33f));
			pixmap.fill();
			final AtlasRegion fake = new AtlasRegion(new Texture(pixmap), 0, 0, 32, 32);
			pixmap.dispose();

			String atlasFileName = skeletonFile.nameWithoutExtension();
			if (atlasFileName.endsWith(".json")) atlasFileName = new FileHandle(atlasFileName).nameWithoutExtension();
			FileHandle atlasFile = skeletonFile.sibling(atlasFileName + ".atlas");
			if (!atlasFile.exists()) atlasFile = skeletonFile.sibling(atlasFileName + ".atlas.txt");
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
		skeleton = new Skeleton(skeleton);
		skeleton.updateWorldTransform();

		state = new AnimationState(new AnimationStateData(skeletonData));

		this.skeletonFile = skeletonFile;
		Preferences prefs = Gdx.app.getPreferences("spine-skeletonviewer");
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

		// Configure skeleton from UI.

		if (ui.skinList.getSelected() != null) skeleton.setSkin(ui.skinList.getSelected());
		if (ui.animationList.getSelected() != null)
			state.setAnimation(0, ui.animationList.getSelected(), ui.loopCheckbox.isChecked());

		if (reload) ui.toast("Reloaded.");
	}

	public void render () {
		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		float delta = Gdx.graphics.getDeltaTime();

		if (skeleton != null) {
			if (reloadTimer <= 0) {
				lastModifiedCheck -= delta;
				if (lastModifiedCheck < 0) {
					lastModifiedCheck = checkModifiedInterval;
					long time = skeletonFile.lastModified();
					if (time != 0 && lastModified != time) reloadTimer = reloadDelay;
				}
			} else {
				reloadTimer -= delta;
				if (reloadTimer <= 0) loadSkeleton(skeletonFile, true);
			}

			state.getData().setDefaultMix(ui.mixSlider.getValue());
			renderer.setPremultipliedAlpha(ui.premultipliedCheckbox.isChecked());

			delta = Math.min(delta, 0.032f) * ui.speedSlider.getValue();
			skeleton.update(delta);
			skeleton.setFlip(ui.flipXCheckbox.isChecked(), ui.flipYCheckbox.isChecked());
			if (!ui.pauseButton.isChecked()) {
				state.update(delta);
				state.apply(skeleton);
			}
			skeleton.setPosition(skeletonX, skeletonY);
			skeleton.updateWorldTransform();

			batch.setColor(Color.WHITE);
			batch.begin();
			renderer.draw(batch, skeleton);
			batch.end();

			debugRenderer.setBones(ui.debugBonesCheckbox.isChecked());
			debugRenderer.setRegionAttachments(ui.debugRegionsCheckbox.isChecked());
			debugRenderer.setBoundingBoxes(ui.debugBoundingBoxesCheckbox.isChecked());
			debugRenderer.setMeshHull(ui.debugMeshHullCheckbox.isChecked());
			debugRenderer.setMeshTriangles(ui.debugMeshTrianglesCheckbox.isChecked());
			debugRenderer.setPaths(ui.debugPathsCheckbox.isChecked());
			debugRenderer.draw(skeleton);
		}

		ui.stage.act();
		ui.stage.draw();

		// Draw indicator for timeline position.
		if (state != null) {
			ShapeRenderer shapes = debugRenderer.getShapeRenderer();
			TrackEntry entry = state.getCurrent(0);
			if (entry != null) {
				float percent = entry.getTime() / entry.getEndTime();
				if (entry.getLoop()) percent %= 1;
				float x = ui.window.getRight() + (Gdx.graphics.getWidth() - ui.window.getRight()) * percent;
				shapes.setColor(Color.CYAN);
				shapes.begin(ShapeType.Line);
				shapes.line(x, 0, x, 20);
				shapes.end();
			}
		}
	}

	public void resize (int width, int height) {
		batch.getProjectionMatrix().setToOrtho2D(0, 0, width, height);
		debugRenderer.getShapeRenderer().setProjectionMatrix(batch.getProjectionMatrix());
		ui.stage.getViewport().update(width, height, true);
		if (!ui.minimizeButton.isChecked()) ui.window.setHeight(height + 8);
	}

	class UI {
		Stage stage = new Stage(new ScreenViewport());
		com.badlogic.gdx.scenes.scene2d.ui.Skin skin = new com.badlogic.gdx.scenes.scene2d.ui.Skin(
			Gdx.files.internal("skin/skin.json"));

		Window window = new Window("Skeleton", skin);
		Table root = new Table(skin);
		TextButton openButton = new TextButton("Open", skin);
		List<String> animationList = new List(skin);
		List<String> skinList = new List(skin);
		CheckBox loopCheckbox = new CheckBox("Loop", skin);
		CheckBox premultipliedCheckbox = new CheckBox("Premultiplied", skin);
		Slider mixSlider = new Slider(0f, 2, 0.01f, false, skin);
		Label mixLabel = new Label("0.3", skin);
		Slider speedSlider = new Slider(0.1f, 3, 0.01f, false, skin);
		Label speedLabel = new Label("1.0", skin);
		CheckBox flipXCheckbox = new CheckBox("X", skin);
		CheckBox flipYCheckbox = new CheckBox("Y", skin);
		CheckBox debugBonesCheckbox = new CheckBox("Bones", skin);
		CheckBox debugRegionsCheckbox = new CheckBox("Regions", skin);
		CheckBox debugBoundingBoxesCheckbox = new CheckBox("Bounds", skin);
		CheckBox debugMeshHullCheckbox = new CheckBox("Mesh hull", skin);
		CheckBox debugMeshTrianglesCheckbox = new CheckBox("Triangles", skin);
		CheckBox debugPathsCheckbox = new CheckBox("Paths", skin);
		Slider scaleSlider = new Slider(0.1f, 3, 0.01f, false, skin);
		Label scaleLabel = new Label("1.0", skin);
		TextButton pauseButton = new TextButton("Pause", skin, "toggle");
		TextButton minimizeButton = new TextButton("-", skin);
		TextButton bonesSetupPoseButton = new TextButton("Bones", skin);
		TextButton slotsSetupPoseButton = new TextButton("Slots", skin);
		TextButton setupPoseButton = new TextButton("Both", skin);
		WidgetGroup toasts = new WidgetGroup();

		public UI () {
			// Configure widgets.

			animationList.getSelection().setRequired(false);

			premultipliedCheckbox.setChecked(true);

			loopCheckbox.setChecked(true);

			scaleSlider.setValue(1);
			scaleSlider.setSnapToValues(new float[] {1}, 0.1f);

			mixSlider.setValue(0.3f);

			speedSlider.setValue(1);
			speedSlider.setSnapToValues(new float[] {1}, 0.1f);

			window.setMovable(false);
			window.setResizable(false);
			window.setKeepWithinStage(false);
			window.setX(-3);
			window.setY(-2);

			window.getTitleLabel().setColor(new Color(0.76f, 1, 1, 1));
			window.getTitleTable().add(openButton).space(3);
			window.getTitleTable().add(minimizeButton).width(20);

			ScrollPane skinScroll = new ScrollPane(skinList, skin, "bg");
			skinScroll.setFadeScrollBars(false);

			ScrollPane animationScroll = new ScrollPane(animationList, skin, "bg");
			animationScroll.setFadeScrollBars(false);

			// Layout.

			root.defaults().space(6);
			root.columnDefaults(0).top().right().padTop(3);
			root.columnDefaults(1).left();
			root.add("Scale:");
			{
				Table table = table();
				table.add(scaleLabel).width(29);
				table.add(scaleSlider).fillX().expandX();
				root.add(table).fill().row();
			}
			root.add("Flip:");
			root.add(table(flipXCheckbox, flipYCheckbox)).row();
			root.add("Debug:");
			root.add(table(debugBonesCheckbox, debugRegionsCheckbox, debugBoundingBoxesCheckbox)).row();
			root.add();
			root.add(table(debugMeshHullCheckbox, debugMeshTrianglesCheckbox, debugPathsCheckbox)).row();
			root.add("Alpha:");
			root.add(premultipliedCheckbox).row();
			root.add("Skin:");
			root.add(skinScroll).expand().fill().minHeight(75).row();
			root.add("Setup pose:");
			root.add(table(bonesSetupPoseButton, slotsSetupPoseButton, setupPoseButton)).row();
			root.add("Animation:");
			root.add(animationScroll).expand().fill().minHeight(75).row();
			root.add("Mix:");
			{
				Table table = table();
				table.add(mixLabel).width(29);
				table.add(mixSlider).fillX().expandX();
				root.add(table).fill().row();
			}
			root.add("Speed:");
			{
				Table table = table();
				table.add(speedLabel).width(29);
				table.add(speedSlider).fillX().expandX();
				root.add(table).fill().row();
			}
			root.add("Playback:");
			root.add(table(pauseButton, loopCheckbox)).row();

			window.add(root).expand().fill();
			window.pack();
			stage.addActor(window);

			{
				Table table = new Table(skin);
				table.setFillParent(true);
				table.setTouchable(Touchable.disabled);
				stage.addActor(table);
				table.pad(10).bottom().right();
				table.add(toasts);
			}

			{
				Table table = new Table();
				table.setFillParent(true);
				table.setTouchable(Touchable.disabled);
				stage.addActor(table);
				table.pad(10).top().right();
				table.add(new Label("", skin, "default", Color.LIGHT_GRAY)); // Version.
			}

			// Events.

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
					loadSkeleton(new FileHandle(new File(dir, name).getAbsolutePath()), false);
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
						ui.window.setHeight(Gdx.graphics.getHeight() + 8);
						minimizeButton.setText("-");
					}
				}
			});

			scaleSlider.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					scaleLabel.setText(Float.toString((int)(scaleSlider.getValue() * 100) / 100f));
					if (!scaleSlider.isDragging()) loadSkeleton(skeletonFile, false);
				}
			});

			speedSlider.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					speedLabel.setText(Float.toString((int)(speedSlider.getValue() * 100) / 100f));
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
							state.clearTrack(0);
						else
							state.setAnimation(0, name, loopCheckbox.isChecked());
					}
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

			Gdx.input.setInputProcessor(new InputMultiplexer(stage, new InputAdapter() {
				public boolean touchDown (int screenX, int screenY, int pointer, int button) {
					touchDragged(screenX, screenY, pointer);
					return false;
				}

				public boolean touchDragged (int screenX, int screenY, int pointer) {
					skeletonX = screenX;
					skeletonY = Gdx.graphics.getHeight() - screenY;
					return false;
				}
			}));
		}

		private Table table (Actor... actors) {
			Table table = new Table();
			table.defaults().space(6);
			table.add(actors);
			return table;
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
	}

	static public void main (String[] args) throws Exception {
		LwjglApplicationConfiguration.disableAudio = true;
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		config.width = 800;
		config.height = 600;
		config.title = "Skeleton Viewer";
		config.allowSoftwareMode = true;
		new LwjglApplication(new SkeletonViewer(), config);
	}
}
