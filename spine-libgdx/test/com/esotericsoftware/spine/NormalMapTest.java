/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.glutils.ShaderProgram;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.math.Vector3;

public class NormalMapTest extends ApplicationAdapter {
	SpriteBatch batch;
	float time;
	SkeletonRenderer renderer;
	Texture atlasTexture, normalMapTexture;
	ShaderProgram program;

	SkeletonData skeletonData;
	Skeleton skeleton;
	Animation animation;

	final Vector3 ambientColor = new Vector3(1, 1, 1);
	final Vector3 lightColor = new Vector3(1, 0.7f, 0.6f);
	final Vector3 lightPosition = new Vector3(0, 0, 0.07f);
	final Vector2 resolution = new Vector2();
	final Vector3 attenuation = new Vector3(0.4f, 3.0f, 5);
	float ambientIntensity = 0.35f;
	float strength = 1.0f;
	boolean useShadow = true;
	boolean useNormals = true;
	boolean flipY = false;

	public void create () {
		program = createShader();
		batch = new SpriteBatch();
		batch.setShader(program);
		renderer = new SkeletonRenderer();

		TextureAtlas atlas = new TextureAtlas(Gdx.files.internal("spineboy-ws.atlas"));
		atlasTexture = atlas.getRegions().first().getTexture();
		normalMapTexture = new Texture(Gdx.files.internal("spineboy-normal.png"));

		SkeletonJson json = new SkeletonJson(atlas);
		// json.setScale(2);
		skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy.json"));
		animation = skeletonData.findAnimation("walk");

		skeleton = new Skeleton(skeletonData);
		skeleton.setToSetupPose();
		skeleton = new Skeleton(skeleton);
		skeleton.setX(200);
		skeleton.setY(100);
		skeleton.updateWorldTransform();
	}

	public void render () {
		float lastTime = time;
		time += Gdx.graphics.getDeltaTime();
		animation.apply(skeleton, lastTime, time, true, null);
		skeleton.updateWorldTransform();
		skeleton.update(Gdx.graphics.getDeltaTime());

		lightPosition.x = Gdx.input.getX();
		lightPosition.y = (Gdx.graphics.getHeight() - Gdx.input.getY());

		Gdx.gl.glClear(GL10.GL_COLOR_BUFFER_BIT);

		batch.begin();
		program.setUniformf("ambientIntensity", ambientIntensity);
		program.setUniformf("attenuation", attenuation);
		program.setUniformf("light", lightPosition);
		program.setUniformi("useNormals", useNormals ? 1 : 0);
		program.setUniformi("useShadow", useShadow ? 1 : 0);
		program.setUniformf("strength", strength);
		normalMapTexture.bind(1);
		atlasTexture.bind(0);
		renderer.draw(batch, skeleton);
		batch.end();
	}

	public void resize (int width, int height) {
		batch.getProjectionMatrix().setToOrtho2D(0, 0, width, height);
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

		System.out.println("VERTEX PROGRAM:\n------------\n\n" + vert);
		System.out.println("FRAGMENT PROGRAM:\n------------\n\n" + frag);
		ShaderProgram program = new ShaderProgram(vert, frag);

		ShaderProgram.pedantic = false;
		if (!program.isCompiled()) throw new IllegalArgumentException("Error compiling shader: " + program.getLog());

		resolution.set(Gdx.graphics.getWidth(), Gdx.graphics.getHeight());

		program.begin();
		program.setUniformi("u_texture", 0);
		program.setUniformi("u_normals", 1);
		program.setUniformf("light", lightPosition);
		program.setUniformf("strength", strength);
		program.setUniformf("ambientIntensity", ambientIntensity);
		program.setUniformf("ambientColor", ambientColor);
		program.setUniformf("resolution", resolution);
		program.setUniformf("lightColor", lightColor);
		program.setUniformf("attenuation", attenuation);
		program.setUniformi("useShadow", useShadow ? 1 : 0);
		program.setUniformi("useNormals", useNormals ? 1 : 0);
		program.setUniformi("yInvert", flipY ? 1 : 0);
		program.end();

		return program;
	}

	public static void main (String[] args) throws Exception {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		config.useGL20 = true;
		new LwjglApplication(new NormalMapTest(), config);
	}
}
