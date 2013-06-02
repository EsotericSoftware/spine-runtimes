/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.InputAdapter;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;

public class AnimationStateTest extends ApplicationAdapter {
	SpriteBatch batch;
	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;

	public void create () {
		batch = new SpriteBatch();
		renderer = new SkeletonRenderer();
		debugRenderer = new SkeletonRendererDebug();

		atlas = new TextureAtlas(Gdx.files.internal("spineboy.atlas"));
		SkeletonJson json = new SkeletonJson(atlas);
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy.json"));

		// Define mixing between animations.
		AnimationStateData stateData = new AnimationStateData(skeletonData);
		stateData.setMix("walk", "jump", 0.2f);
		stateData.setMix("jump", "walk", 0.4f);
		stateData.setMix("jump", "jump", 0.2f);

		state = new AnimationState(stateData);
		state.setAnimation("walk", true);

		skeleton = new Skeleton(skeletonData);
		skeleton.setX(250);
		skeleton.setY(20);
		skeleton.updateWorldTransform();

		Gdx.input.setInputProcessor(new InputAdapter() {
			public boolean touchDown (int screenX, int screenY, int pointer, int button) {
				keyDown(0);
				return true;
			}

			public boolean keyDown (int keycode) {
				state.setAnimation("jump", false);
				state.addAnimation("walk", true);
				return true;
			}
		});
	}

	public void render () {
		state.update(Gdx.graphics.getDeltaTime() / 3);

		Gdx.gl.glClear(GL10.GL_COLOR_BUFFER_BIT);

		state.apply(skeleton);
		if (state.getAnimation().getName().equals("walk")) {
			// After one second, change the current animation. Mixing is done by AnimationState for you.
			if (state.getTime() > 2) state.setAnimation("jump", false);
		} else {
			if (state.getTime() > 1) state.setAnimation("walk", true);
		}
		skeleton.updateWorldTransform();

		batch.begin();
		renderer.draw(batch, skeleton);
		batch.end();

		debugRenderer.draw(skeleton);
	}

	public void resize (int width, int height) {
		batch.getProjectionMatrix().setToOrtho2D(0, 0, width, height);
		debugRenderer.getShapeRenderer().setProjectionMatrix(batch.getProjectionMatrix());
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new AnimationStateTest());
	}
}
