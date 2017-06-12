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

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.InputAdapter;
import com.badlogic.gdx.InputMultiplexer;
import com.badlogic.gdx.Preferences;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.glutils.ShaderProgram;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.math.Vector3;
import com.badlogic.gdx.scenes.scene2d.Actor;
import com.badlogic.gdx.scenes.scene2d.InputEvent;
import com.badlogic.gdx.scenes.scene2d.InputListener;
import com.badlogic.gdx.scenes.scene2d.Stage;
import com.badlogic.gdx.scenes.scene2d.ui.CheckBox;
import com.badlogic.gdx.scenes.scene2d.ui.Label;
import com.badlogic.gdx.scenes.scene2d.ui.Slider;
import com.badlogic.gdx.scenes.scene2d.ui.Table;
import com.badlogic.gdx.scenes.scene2d.ui.TextButton;
import com.badlogic.gdx.scenes.scene2d.ui.Window;
import com.badlogic.gdx.scenes.scene2d.utils.ChangeListener;
import com.badlogic.gdx.utils.Align;
import com.esotericsoftware.spine.Animation.MixDirection;
import com.esotericsoftware.spine.Animation.MixPose;

public class NormalMapTest extends ApplicationAdapter {
	String skeletonPath, animationName;
	SpriteBatch batch;
	float time;
	SkeletonRenderer renderer;
	Texture atlasTexture, normalMapTexture;
	ShaderProgram program;
	UI ui;

	SkeletonData skeletonData;
	Skeleton skeleton;
	Animation animation;

	final Vector3 ambientColor = new Vector3();
	final Vector3 lightColor = new Vector3();
	final Vector3 lightPosition = new Vector3();
	final Vector2 resolution = new Vector2();
	final Vector3 attenuation = new Vector3();

	public NormalMapTest (String skeletonPath, String animationName) {
		this.skeletonPath = skeletonPath;
		this.animationName = animationName;
	}

	public void create () {
		ui = new UI();

		program = createShader();
		batch = new SpriteBatch();
		batch.setShader(program);
		renderer = new SkeletonRenderer();

		FileHandle file = Gdx.files.internal(skeletonPath + "-diffuse.atlas");
		TextureAtlas atlas = new TextureAtlas(file);
		atlasTexture = atlas.getRegions().first().getTexture();

		normalMapTexture = new Texture(Gdx.files.internal(skeletonPath + "-normal.png"));
	
		SkeletonJson json = new SkeletonJson(atlas);
		skeletonData = json.readSkeletonData(Gdx.files.internal(skeletonPath + ".json"));
		if (animationName != null) animation = skeletonData.findAnimation(animationName);
		if (animation == null) animation = skeletonData.getAnimations().first();

		skeleton = new Skeleton(skeletonData);
		skeleton.setToSetupPose();
		skeleton = new Skeleton(skeleton);
		skeleton.setX(ui.prefs.getFloat("x", Gdx.graphics.getWidth() / 2));
		skeleton.setY(ui.prefs.getFloat("y", Gdx.graphics.getHeight() / 4));
		skeleton.updateWorldTransform();

		Gdx.input.setInputProcessor(new InputMultiplexer(ui.stage, new InputAdapter() {
			public boolean touchDown (int screenX, int screenY, int pointer, int button) {
				touchDragged(screenX, screenY, pointer);
				return true;
			}

			public boolean touchDragged (int screenX, int screenY, int pointer) {
				skeleton.setPosition(screenX, Gdx.graphics.getHeight() - screenY);
				return true;
			}

			public boolean touchUp (int screenX, int screenY, int pointer, int button) {
				ui.prefs.putFloat("x", skeleton.getX());
				ui.prefs.putFloat("y", skeleton.getY());
				ui.prefs.flush();
				return true;
			}
		}));
	}

	public void render () {
		float lastTime = time;
		time += Gdx.graphics.getDeltaTime();
		if (animation != null) animation.apply(skeleton, lastTime, time, true, null, 1, MixPose.current, MixDirection.in);
		skeleton.updateWorldTransform();
		skeleton.update(Gdx.graphics.getDeltaTime());

		lightPosition.x = Gdx.input.getX();
		lightPosition.y = (Gdx.graphics.getHeight() - Gdx.input.getY());

		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		ambientColor.x = ui.ambientColorR.getValue();
		ambientColor.y = ui.ambientColorG.getValue();
		ambientColor.z = ui.ambientColorB.getValue();
		lightColor.x = ui.lightColorR.getValue();
		lightColor.y = ui.lightColorG.getValue();
		lightColor.z = ui.lightColorB.getValue();
		attenuation.x = ui.attenuationX.getValue();
		attenuation.y = ui.attenuationY.getValue();
		attenuation.z = ui.attenuationZ.getValue();
		lightPosition.z = ui.lightZ.getValue();

		batch.begin();
		program.setUniformi("yInvert", ui.yInvert.isChecked() ? 1 : 0);
		program.setUniformf("resolution", resolution);
		program.setUniformf("ambientColor", ambientColor);
		program.setUniformf("ambientIntensity", ui.ambientIntensity.getValue());
		program.setUniformf("attenuation", attenuation);
		program.setUniformf("light", lightPosition);
		program.setUniformf("lightColor", lightColor);
		program.setUniformi("useNormals", ui.useNormals.isChecked() ? 1 : 0);
		program.setUniformi("useShadow", ui.useShadow.isChecked() ? 1 : 0);
		program.setUniformf("strength", ui.strength.getValue());
		normalMapTexture.bind(1);
		atlasTexture.bind(0);
		renderer.draw(batch, skeleton);
		batch.end();

		ui.stage.act();
		ui.stage.draw();
	}

	public void resize (int width, int height) {
		batch.getProjectionMatrix().setToOrtho2D(0, 0, width, height);
		ui.stage.getViewport().update(width, height, true);
		resolution.set(width, height);
	}

	private ShaderProgram createShader () {
		String vert = "attribute vec4 a_position;\n" //
			+ "attribute vec4 a_color;\n" //
			+ "attribute vec2 a_texCoord0;\n" //
			+ "uniform mat4 u_proj;\n" //
			+ "uniform mat4 u_trans;\n" //
			+ "uniform mat4 u_projTrans;\n" //
			+ "varying vec4 v_color;\n" //
			+ "varying vec2 v_texCoords;\n" //
			+ "\n" //
			+ "void main()\n" //
			+ "{\n" //
			+ "   v_color = a_color;\n" //
			+ "   v_texCoords = a_texCoord0;\n" //
			+ "   gl_Position =  u_projTrans * a_position;\n" //
			+ "}\n" //
			+ "";

		String frag = "#ifdef GL_ES\n" //
			+ "precision mediump float;\n" //
			+ "#endif\n" //
			+ "varying vec4 v_color;\n" //
			+ "varying vec2 v_texCoords;\n" //
			+ "uniform sampler2D u_texture;\n" //
			+ "uniform sampler2D u_normals;\n" //
			+ "uniform vec3 light;\n" //
			+ "uniform vec3 ambientColor;\n" //
			+ "uniform float ambientIntensity; \n" //
			+ "uniform vec2 resolution;\n" //
			+ "uniform vec3 lightColor;\n" //
			+ "uniform bool useNormals;\n" //
			+ "uniform bool useShadow;\n" //
			+ "uniform vec3 attenuation;\n" //
			+ "uniform float strength;\n" //
			+ "uniform bool yInvert;\n" //
			+ "\n" //
			+ "void main() {\n" //
			+ "  // sample color & normals from our textures\n" //
			+ "  vec4 color = texture2D(u_texture, v_texCoords.st);\n" //
			+ "  vec3 nColor = texture2D(u_normals, v_texCoords.st).rgb;\n" //
			+ "\n" //
			+ "  // some bump map programs will need the Y value flipped..\n" //
			+ "  nColor.g = yInvert ? 1.0 - nColor.g : nColor.g;\n" //
			+ "\n" //
			+ "  // this is for debugging purposes, allowing us to lower the intensity of our bump map\n" //
			+ "  vec3 nBase = vec3(0.5, 0.5, 1.0);\n" //
			+ "  nColor = mix(nBase, nColor, strength);\n" //
			+ "\n" //
			+ "  // normals need to be converted to [-1.0, 1.0] range and normalized\n" //
			+ "  vec3 normal = normalize(nColor * 2.0 - 1.0);\n" //
			+ "\n" //
			+ "  // here we do a simple distance calculation\n" //
			+ "  vec3 deltaPos = vec3( (light.xy - gl_FragCoord.xy) / resolution.xy, light.z );\n" //
			+ "\n" //
			+ "  vec3 lightDir = normalize(deltaPos);\n" //
			+ "  float lambert = useNormals ? clamp(dot(normal, lightDir), 0.0, 1.0) : 1.0;\n" //
			+ "  \n" //
			+ "  // now let's get a nice little falloff\n" //
			+ "  float d = sqrt(dot(deltaPos, deltaPos));  \n" //
			+ "  float att = useShadow ? 1.0 / ( attenuation.x + (attenuation.y*d) + (attenuation.z*d*d) ) : 1.0;\n" //
			+ "  \n" //
			+ "  vec3 result = (ambientColor * ambientIntensity) + (lightColor.rgb * lambert) * att;\n" //
			+ "  result *= color.rgb;\n" //
			+ "  \n" //
			+ "  gl_FragColor = v_color * vec4(result, color.a);\n" //
			+ "}";

		// System.out.println("VERTEX PROGRAM:\n------------\n\n" + vert);
		// System.out.println("FRAGMENT PROGRAM:\n------------\n\n" + frag);
		ShaderProgram program = new ShaderProgram(vert, frag);
		ShaderProgram.pedantic = false;
		if (!program.isCompiled()) throw new IllegalArgumentException("Error compiling shader: " + program.getLog());

		program.begin();
		program.setUniformi("u_texture", 0);
		program.setUniformi("u_normals", 1);
		program.end();

		return program;
	}

	class UI {
		Stage stage = new Stage();
		com.badlogic.gdx.scenes.scene2d.ui.Skin skin = new com.badlogic.gdx.scenes.scene2d.ui.Skin(
			Gdx.files.internal("skin/skin.json"));
		Preferences prefs = Gdx.app.getPreferences(".spine/NormalMapTest");

		Window window;
		Table root;
		Slider ambientColorR, ambientColorG, ambientColorB;
		Slider lightColorR, lightColorG, lightColorB, lightZ;
		Slider attenuationX, attenuationY, attenuationZ;
		Slider ambientIntensity;
		Slider strength;
		CheckBox useShadow, useNormals, yInvert;

		public UI () {
			create();
		}

		public void create () {
			window = new Window("Light", skin);

			root = new Table(skin);
			root.pad(2, 4, 4, 4).defaults().space(6);
			root.columnDefaults(0).top().right();
			root.columnDefaults(1).left();
			ambientColorR = slider("Ambient R", 1);
			ambientColorG = slider("Ambient G", 1);
			ambientColorB = slider("Ambient B", 1);
			ambientIntensity = slider("Ambient intensity", 0.35f);
			lightColorR = slider("Light R", 1);
			lightColorG = slider("Light G", 0.7f);
			lightColorB = slider("Light B", 0.6f);
			lightZ = slider("Light Z", 0.07f);
			attenuationX = slider("Attenuation", 0.4f);
			attenuationY = slider("Attenuation*d", 3);
			attenuationZ = slider("Attenuation*d*d", 5);
			strength = slider("Strength", 1);
			{
				Table table = new Table();
				table.defaults().space(12);
				table.add(useShadow = checkbox(" Use shadow", true));
				table.add(useNormals = checkbox(" Use normals", true));
				table.add(yInvert = checkbox(" Invert Y", true));
				root.add(table).colspan(2).row();
			}

			TextButton resetButton = new TextButton("Reset", skin);
			resetButton.getColor().a = 0.66f;
			window.getTitleTable().add(resetButton).height(20);

			window.add(root).expand().fill();
			window.pack();
			stage.addActor(window);

			// Events.

			window.addListener(new InputListener() {
				public boolean touchDown (InputEvent event, float x, float y, int pointer, int button) {
					event.cancel();
					return true;
				}
			});

			resetButton.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					window.remove();
					prefs.clear();
					prefs.flush();
					create();
				}
			});
		}

		private CheckBox checkbox (final String name, boolean defaultValue) {
			final CheckBox checkbox = new CheckBox(name, skin);
			checkbox.setChecked(prefs.getBoolean(checkbox.getText().toString(), defaultValue));

			checkbox.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					prefs.putBoolean(name, checkbox.isChecked());
					prefs.flush();
				}
			});

			return checkbox;
		}

		private Slider slider (final String name, float defaultValue) {
			final Slider slider = new Slider(0, 1, 0.01f, false, skin);
			slider.setValue(prefs.getFloat(name, defaultValue));

			final Label label = new Label("", skin);
			label.setAlignment(Align.right);
			label.setText(Float.toString((int)(slider.getValue() * 100) / 100f));

			slider.addListener(new ChangeListener() {
				public void changed (ChangeEvent event, Actor actor) {
					label.setText(Float.toString((int)(slider.getValue() * 100) / 100f));
					if (!slider.isDragging()) {
						prefs.putFloat(name, slider.getValue());
						prefs.flush();
					}
				}
			});

			Table table = new Table();
			table.add(label).width(35).space(12);
			table.add(slider);

			root.add(name);
			root.add(table).fill().row();
			return slider;
		}
	}

	public static void main (String[] args) throws Exception {
		if (args.length == 0)
			args = new String[] {"spineboy-old/spineboy-old", "walk"};
		else if (args.length == 1) //
			args = new String[] {args[0], null};

		new LwjglApplication(new NormalMapTest(args[0], args[1]));
	}
}
