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

import static com.badlogic.gdx.math.Interpolation.*;
import static com.badlogic.gdx.scenes.scene2d.actions.Actions.*;

import java.io.File;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.InputAdapter;
import com.badlogic.gdx.InputMultiplexer;
import com.badlogic.gdx.Preferences;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.Texture.TextureFilter;
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
import com.badlogic.gdx.utils.Null;
import com.badlogic.gdx.utils.viewport.ScreenViewport;

import com.esotericsoftware.spine.Animation.MixBlend;
import com.esotericsoftware.spine.AnimationState.TrackEntry;

import java.awt.FileDialog;
import java.awt.Frame;

class SkeletonViewerUI {
	final SkeletonViewer viewer;
	final OrthographicCamera camera;

	boolean prefsLoaded;

	Stage stage = new Stage(new ScreenViewport());
	com.badlogic.gdx.scenes.scene2d.ui.Skin skin = new com.badlogic.gdx.scenes.scene2d.ui.Skin(
		Gdx.files.internal("skin/skin.json"));

	Window window = new Window("Skeleton", skin);
	Table root = new Table(skin);
	TextButton openButton = new TextButton("Open", skin);
	TextButton minimizeButton = new TextButton("-", skin);

	Slider loadScaleSlider = new Slider(0.1f, 3, 0.01f, false, skin);
	Label loadScaleLabel = new Label("100%", skin);
	TextButton loadScaleResetButton = new TextButton("Reload", skin);

	Slider zoomSlider = new Slider(0.01f, 10, 0.01f, false, skin);
	Label zoomLabel = new Label("100%", skin);
	TextButton zoomResetButton = new TextButton("Reset", skin);

	Slider xScaleSlider = new Slider(-2, 2, 0.01f, false, skin);
	Label xScaleLabel = new Label("100%", skin);
	TextButton xScaleResetButton = new TextButton("Reset", skin);

	Slider yScaleSlider = new Slider(-2, 2, 0.01f, false, skin);
	Label yScaleLabel = new Label("100%", skin);
	TextButton yScaleResetButton = new TextButton("Reset", skin);

	CheckBox debugBonesCheckbox = new CheckBox("Bones", skin);
	CheckBox debugRegionsCheckbox = new CheckBox("Regions", skin);
	CheckBox debugBoundingBoxesCheckbox = new CheckBox("Bounds", skin);
	CheckBox debugMeshHullCheckbox = new CheckBox("Mesh hull", skin);
	CheckBox debugMeshTrianglesCheckbox = new CheckBox("Triangles", skin);
	CheckBox debugPathsCheckbox = new CheckBox("Paths", skin);
	CheckBox debugPointsCheckbox = new CheckBox("Points", skin);
	CheckBox debugClippingCheckbox = new CheckBox("Clipping", skin);

	CheckBox pmaCheckbox = new CheckBox("Premultiplied", skin);

	CheckBox linearCheckbox = new CheckBox("Linear", skin);

	TextButton bonesSetupPoseButton = new TextButton("Bones", skin, "toggle");
	TextButton slotsSetupPoseButton = new TextButton("Slots", skin, "toggle");
	TextButton setupPoseButton = new TextButton("Both", skin, "toggle");

	List<String> skinList = new List(skin);
	ScrollPane skinScroll = new ScrollPane(skinList, skin, "bg");

	ButtonGroup<TextButton> trackButtons = new ButtonGroup();
	CheckBox loopCheckbox = new CheckBox("Loop", skin);
	CheckBox addCheckbox = new CheckBox("Add", skin);

	Slider alphaSlider = new Slider(0, 1, 0.01f, false, skin);
	Label alphaLabel = new Label("100%", skin);

	List<String> animationList = new List(skin);
	ScrollPane animationScroll = new ScrollPane(animationList, skin, "bg");

	Slider speedSlider = new Slider(0, 3, 0.01f, false, skin);
	Label speedLabel = new Label("1.0x", skin);
	TextButton speedResetButton = new TextButton("Reset", skin);

	CheckBox reverseCheckbox = new CheckBox("Reverse", skin);
	CheckBox holdPrevCheckbox = new CheckBox("Hold previous", skin);

	Slider mixSlider = new Slider(0, 4, 0.01f, false, skin);
	Label mixLabel = new Label("0.3s", skin);

	Label statusLabel = new Label("", skin);
	WidgetGroup toasts = new WidgetGroup();

	SkeletonViewerUI (SkeletonViewer viewer) {
		this.viewer = viewer;
		camera = viewer.camera;
		initialize();
		layout();
		events();
	}

	void initialize () {
		skin.getFont("default").getData().markupEnabled = true;

		for (int i = 0; i < 6; i++)
			trackButtons.add(new TextButton(i + "", skin, "toggle"));

		pmaCheckbox.setChecked(true);

		linearCheckbox.setChecked(true);

		new ButtonGroup(bonesSetupPoseButton, slotsSetupPoseButton, setupPoseButton).setMinCheckCount(0);

		loopCheckbox.setChecked(true);

		loadScaleSlider.setValue(1);
		loadScaleSlider.setSnapToValues(new float[] {0.5f, 1, 1.5f, 2, 2.5f}, 0.09f);

		zoomSlider.setValue(1);
		zoomSlider.setSnapToValues(new float[] {1, 2}, 0.30f);

		xScaleSlider.setValue(1);
		xScaleSlider.setSnapToValues(new float[] {-1.5f, -1, -0.5f, 0.5f, 1, 1.5f}, 0.12f);

		yScaleSlider.setValue(1);
		yScaleSlider.setSnapToValues(new float[] {-1.5f, -1, -0.5f, 0.5f, 1, 1.5f}, 0.12f);

		skinList.getSelection().setRequired(false);
		skinList.getSelection().setToggle(true);

		animationList.getSelection().setRequired(false);
		animationList.getSelection().setToggle(true);

		mixSlider.setValue(0.3f);
		mixSlider.setSnapToValues(new float[] {1, 1.5f, 2, 2.5f, 3, 3.5f}, 0.12f);

		speedSlider.setValue(1);
		speedSlider.setSnapToValues(new float[] {0.5f, 0.75f, 1, 1.25f, 1.5f, 2, 2.5f}, 0.09f);

		alphaSlider.setValue(1);
		alphaSlider.setDisabled(true);

		addCheckbox.setDisabled(true);
		holdPrevCheckbox.setDisabled(true);

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
		float resetWidth = loadScaleResetButton.getPrefWidth();

		root.defaults().space(6);
		root.columnDefaults(0).top().right().padTop(3);
		root.columnDefaults(1).left();
		root.add("Load scale:");
		{
			Table table = table();
			table.add(loadScaleLabel).width(29);
			table.add(loadScaleSlider).growX();
			table.add(loadScaleResetButton).width(resetWidth);
			root.add(table).fill().row();
		}
		root.add("Zoom:");
		{
			Table table = table();
			table.add(zoomLabel).width(29);
			table.add(zoomSlider).growX();
			table.add(zoomResetButton).width(resetWidth);
			root.add(table).fill().row();
		}
		root.add("Scale X:");
		{
			Table table = table();
			table.add(xScaleLabel).width(29);
			table.add(xScaleSlider).growX();
			table.add(xScaleResetButton).width(resetWidth);
			root.add(table).fill().row();
		}
		root.add("Scale Y:");
		{
			Table table = table();
			table.add(yScaleLabel).width(29);
			table.add(yScaleSlider).growX();
			table.add(yScaleResetButton).width(resetWidth);
			root.add(table).fill().row();
		}
		root.add("Debug:");
		root.add(table(debugBonesCheckbox, debugRegionsCheckbox, debugBoundingBoxesCheckbox)).row();
		root.add();
		root.add(table(debugPathsCheckbox, debugPointsCheckbox, debugClippingCheckbox)).row();
		root.add();
		root.add(table(debugMeshHullCheckbox, debugMeshTrianglesCheckbox)).row();
		root.add("Atlas alpha:");
		{
			Table table = table();
			table.add(pmaCheckbox);
			table.add("Filtering:").growX().getActor().setAlignment(Align.right);
			table.add(linearCheckbox);
			root.add(table).fill().row();
		}

		root.add(new Image(skin.newDrawable("white", new Color(0x4e4e4eff)))).height(1).fillX().colspan(2).pad(-3, 0, 1, 0).row();

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
		root.add();
		{
			Table table = table();
			table.add(reverseCheckbox);
			table.add(holdPrevCheckbox);
			table.add(addCheckbox);
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

		root.add(new Image(skin.newDrawable("white", new Color(0x4e4e4eff)))).height(1).fillX().colspan(2).pad(1, 0, 1, 0).row();

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
			table.add(new Label(SkeletonViewer.version, skin, "default", Color.LIGHT_GRAY));
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
				if (viewer.loadSkeleton(new FileHandle(new File(dir, name).getAbsolutePath()))) toast("Loaded.");
			}
		});

		setupPoseButton.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				if (viewer.skeleton != null) viewer.skeleton.setToSetupPose();
			}
		});
		bonesSetupPoseButton.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				if (viewer.skeleton != null) viewer.skeleton.setBonesToSetupPose();
			}
		});
		slotsSetupPoseButton.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				if (viewer.skeleton != null) viewer.skeleton.setSlotsToSetupPose();
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
					window.setHeight(Gdx.graphics.getHeight() / SkeletonViewer.uiScale + 8);
					minimizeButton.setText("-");
				}
			}
		});

		loadScaleSlider.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				loadScaleLabel.setText(Integer.toString((int)(loadScaleSlider.getValue() * 100)) + "%");
				if (!loadScaleSlider.isDragging() && viewer.loadSkeleton(viewer.skeletonFile)) toast("Reloaded.");
				loadScaleResetButton.setText(loadScaleSlider.getValue() == 1 ? "Reload" : "Reset");
			}
		});
		loadScaleResetButton.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				viewer.resetCameraPosition();
				if (loadScaleSlider.getValue() == 1) {
					if (viewer.loadSkeleton(viewer.skeletonFile)) toast("Reloaded.");
				} else
					loadScaleSlider.setValue(1);
				loadScaleResetButton.setText("Reload");
			}
		});

		zoomSlider.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				zoomLabel.setText(Integer.toString((int)(zoomSlider.getValue() * 100)) + "%");
				float newZoom = 1 / zoomSlider.getValue();
				camera.position.x -= window.getWidth() / 2 * (newZoom - camera.zoom);
				camera.zoom = newZoom;
			}
		});
		zoomResetButton.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				viewer.resetCameraPosition();
				float x = camera.position.x;
				zoomSlider.setValue(1);
				camera.position.x = x;
			}
		});

		xScaleSlider.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				if (xScaleSlider.getValue() == 0) xScaleSlider.setValue(0.01f);
				xScaleLabel.setText(Integer.toString((int)(xScaleSlider.getValue() * 100)) + "%");
			}
		});
		xScaleResetButton.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				xScaleSlider.setValue(1);
			}
		});

		yScaleSlider.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				if (yScaleSlider.getValue() == 0) yScaleSlider.setValue(0.01f);
				yScaleLabel.setText(Integer.toString((int)(yScaleSlider.getValue() * 100)) + "%");
			}
		});
		yScaleResetButton.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				yScaleSlider.setValue(1);
			}
		});

		speedSlider.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				speedLabel.setText(Float.toString((int)(speedSlider.getValue() * 100) / 100f) + "x");
			}
		});
		speedResetButton.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				speedSlider.setValue(1);
			}
		});

		alphaSlider.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				alphaLabel.setText(Integer.toString((int)(alphaSlider.getValue() * 100)) + "%");
				int track = trackButtons.getCheckedIndex();
				if (track > 0) {
					TrackEntry current = viewer.state.getCurrent(track);
					if (current != null) {
						current.setAlpha(alphaSlider.getValue());
						current.resetRotationDirections();
					}
				}
			}
		});

		mixSlider.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				mixLabel.setText(Float.toString((int)(mixSlider.getValue() * 100) / 100f) + "s");
				if (viewer.state != null) viewer.state.getData().setDefaultMix(mixSlider.getValue());
			}
		});

		InputListener scrollFocusListener = new InputListener() {
			public void enter (InputEvent event, float x, float y, int pointer, @Null Actor fromActor) {
				if (pointer == -1) stage.setScrollFocus(event.getListenerActor());
			}

			public void exit (InputEvent event, float x, float y, int pointer, @Null Actor toActor) {
				if (pointer == -1 && stage.getScrollFocus() == event.getListenerActor()) stage.setScrollFocus(null);
			}
		};

		animationList.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				if (viewer.state != null) {
					String name = animationList.getSelected();
					if (name == null)
						viewer.state.setEmptyAnimation(trackButtons.getCheckedIndex(), mixSlider.getValue());
					else
						viewer.setAnimation(false);
				}
			}
		});
		animationScroll.addListener(scrollFocusListener);

		ChangeListener setAnimation = new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				viewer.setAnimation(false);
			}
		};
		loopCheckbox.addListener(setAnimation);
		reverseCheckbox.addListener(setAnimation);
		holdPrevCheckbox.addListener(setAnimation);
		addCheckbox.addListener(setAnimation);

		linearCheckbox.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				if (viewer.atlas == null) return;
				TextureFilter filter = linearCheckbox.isChecked() ? TextureFilter.Linear : TextureFilter.Nearest;
				for (Texture texture : viewer.atlas.getTextures())
					texture.setFilter(filter, filter);
			}
		});

		skinList.addListener(new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				if (viewer.skeleton != null) {
					String skinName = skinList.getSelected();
					if (skinName == null)
						viewer.skeleton.setSkin((Skin)null);
					else
						viewer.skeleton.setSkin(skinName);
					viewer.skeleton.setSlotsToSetupPose();
				}
			}
		});
		skinScroll.addListener(scrollFocusListener);

		ChangeListener trackButtonListener = new ChangeListener() {
			public void changed (ChangeEvent event, Actor actor) {
				int track = trackButtons.getCheckedIndex();
				if (track == -1) return;
				TrackEntry current = viewer.state.getCurrent(track);
				animationList.getSelection().setProgrammaticChangeEvents(false);
				animationList.setSelected(current == null ? null : current.animation.name);
				animationList.getSelection().setProgrammaticChangeEvents(true);

				alphaSlider.setDisabled(track == 0);
				alphaSlider.setValue(current == null ? 1 : current.alpha);

				addCheckbox.setDisabled(track == 0);
				holdPrevCheckbox.setDisabled(track == 0);

				if (current != null) {
					loopCheckbox.setChecked(current.getLoop());
					addCheckbox.setChecked(current.getMixBlend() == MixBlend.add);
					reverseCheckbox.setChecked(current.getReverse());
					holdPrevCheckbox.setChecked(current.getHoldPrevious());
				}
			}
		};
		for (TextButton button : trackButtons.getButtons())
			button.addListener(trackButtonListener);

		Gdx.input.setInputProcessor(new InputMultiplexer(stage, new InputAdapter() {
			float offsetX;
			float offsetY;

			public boolean touchDown (int screenX, int screenY, int pointer, int button) {
				offsetX = screenX;
				offsetY = Gdx.graphics.getHeight() - 1 - screenY;
				return false;
			}

			public boolean touchDragged (int screenX, int screenY, int pointer) {
				float deltaX = screenX - offsetX;
				float deltaY = Gdx.graphics.getHeight() - 1 - screenY - offsetY;

				camera.position.x -= deltaX * camera.zoom;
				camera.position.y -= deltaY * camera.zoom;

				offsetX = screenX;
				offsetY = Gdx.graphics.getHeight() - 1 - screenY;
				return false;
			}

			public boolean touchUp (int screenX, int screenY, int pointer, int button) {
				savePrefs();
				return false;
			}

			public boolean scrolled (float amountX, float amountY) {
				float zoom = zoomSlider.getValue(), zoomMin = zoomSlider.getMinValue(), zoomMax = zoomSlider.getMaxValue();
				float speedAlpha = Math.min(1.2f, (zoom - zoomMin) / (zoomMax - zoomMin) * 3.5f);
				zoom -= linear.apply(0.02f, 0.2f, speedAlpha) * Math.signum(amountY);
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
		pmaCheckbox.addListener(savePrefsListener);
		linearCheckbox.addListener(savePrefsListener);
		bonesSetupPoseButton.addListener(savePrefsListener);
		slotsSetupPoseButton.addListener(savePrefsListener);
		setupPoseButton.addListener(savePrefsListener);
		loopCheckbox.addListener(savePrefsListener);
		addCheckbox.addListener(savePrefsListener);
		holdPrevCheckbox.addListener(savePrefsListener);
		reverseCheckbox.addListener(savePrefsListener);
		speedSlider.addListener(savePrefsListener);
		speedResetButton.addListener(savePrefsListener);
		mixSlider.addListener(savePrefsListener);
		loadScaleSlider.addListener(savePrefsListener);
		loadScaleResetButton.addListener(savePrefsListener);
		zoomSlider.addListener(savePrefsListener);
		zoomResetButton.addListener(savePrefsListener);
		animationList.addListener(savePrefsListener);
		skinList.addListener(savePrefsListener);
	}

	Table table (Actor... actors) {
		Table table = new Table(skin);
		table.defaults().space(6);
		table.add(actors);
		return table;
	}

	void render () {
		if (viewer.state != null && viewer.state.getCurrent(trackButtons.getCheckedIndex()) == null) {
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
		Preferences prefs = viewer.prefs;
		prefs.putBoolean("debugBones", debugBonesCheckbox.isChecked());
		prefs.putBoolean("debugRegions", debugRegionsCheckbox.isChecked());
		prefs.putBoolean("debugMeshHull", debugMeshHullCheckbox.isChecked());
		prefs.putBoolean("debugMeshTriangles", debugMeshTrianglesCheckbox.isChecked());
		prefs.putBoolean("debugPaths", debugPathsCheckbox.isChecked());
		prefs.putBoolean("debugPoints", debugPointsCheckbox.isChecked());
		prefs.putBoolean("debugClipping", debugClippingCheckbox.isChecked());
		prefs.putBoolean("premultiplied", pmaCheckbox.isChecked());
		prefs.putBoolean("linear", linearCheckbox.isChecked());
		if (bonesSetupPoseButton.isChecked())
			prefs.putString("setupPose", "bones");
		else if (slotsSetupPoseButton.isChecked())
			prefs.putString("setupPose", "slots");
		else if (setupPoseButton.isChecked()) //
			prefs.putString("setupPose", "both");
		else
			prefs.remove("setupPose");
		prefs.putBoolean("loop", loopCheckbox.isChecked());
		prefs.putBoolean("add", addCheckbox.isChecked());
		prefs.putBoolean("holdPrev", holdPrevCheckbox.isChecked());
		prefs.putBoolean("reverse", reverseCheckbox.isChecked());
		prefs.putFloat("speed", speedSlider.getValue());
		prefs.putFloat("mix", mixSlider.getValue());
		prefs.putFloat("scale", loadScaleSlider.getValue());
		prefs.putFloat("zoom", zoomSlider.getValue());
		prefs.putFloat("x", camera.position.x);
		prefs.putFloat("y", camera.position.y);
		if (viewer.state != null) {
			TrackEntry current = viewer.state.getCurrent(0);
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
		try {
			Preferences prefs = viewer.prefs;
			debugBonesCheckbox.setChecked(prefs.getBoolean("debugBones", true));
			debugRegionsCheckbox.setChecked(prefs.getBoolean("debugRegions", false));
			debugMeshHullCheckbox.setChecked(prefs.getBoolean("debugMeshHull", false));
			debugMeshTrianglesCheckbox.setChecked(prefs.getBoolean("debugMeshTriangles", false));
			debugPathsCheckbox.setChecked(prefs.getBoolean("debugPaths", true));
			debugPointsCheckbox.setChecked(prefs.getBoolean("debugPoints", true));
			debugClippingCheckbox.setChecked(prefs.getBoolean("debugClipping", true));
			pmaCheckbox.setChecked(prefs.getBoolean("premultiplied", true));
			linearCheckbox.setChecked(prefs.getBoolean("linear", true));
			String setupPose = prefs.getString("setupPose", "");
			bonesSetupPoseButton.setChecked(setupPose.equals("bones"));
			slotsSetupPoseButton.setChecked(setupPose.equals("slots"));
			setupPoseButton.setChecked(setupPose.equals("both"));
			loopCheckbox.setChecked(prefs.getBoolean("loop", true));
			addCheckbox.setChecked(prefs.getBoolean("add", false));
			holdPrevCheckbox.setChecked(prefs.getBoolean("holdPrev", false));
			reverseCheckbox.setChecked(prefs.getBoolean("reverse", false));
			speedSlider.setValue(prefs.getFloat("speed", 0.3f));
			mixSlider.setValue(prefs.getFloat("mix", 0.3f));

			zoomSlider.setValue(prefs.getFloat("zoom", 1));
			camera.zoom = 1 / prefs.getFloat("zoom", 1);
			camera.position.x = prefs.getFloat("x", 0);
			camera.position.y = prefs.getFloat("y", 0);

			loadScaleSlider.setValue(prefs.getFloat("scale", 1));
			animationList.setSelected(prefs.getString("animationName", null));
			skinList.setSelected(prefs.getString("skinName", null));
		} catch (Throwable ex) {
			System.out.println("Unable to read preferences:");
			ex.printStackTrace();
		}
	}
}
