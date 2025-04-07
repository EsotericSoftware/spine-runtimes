/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl3.Lwjgl3Application;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.scenes.scene2d.Stage;
import com.badlogic.gdx.utils.viewport.ScreenViewport;

import com.esotericsoftware.spine.Skeleton.Physics;
import com.esotericsoftware.spine.utils.SkeletonActor;
import com.esotericsoftware.spine.utils.TwoColorPolygonBatch;

public class ActorTest extends ApplicationAdapter {
	Stage stage;
	TwoColorPolygonBatch batch;
	SkeletonRenderer renderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;
	SkeletonActor skeletonActor;

	public void create () {
		batch = new TwoColorPolygonBatch();

		stage = new Stage(new ScreenViewport(), batch);
		Gdx.input.setInputProcessor(stage);

		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);

		atlas = new TextureAtlas(Gdx.files.internal("spineboy/spineboy-pma.atlas"));
		SkeletonJson json = new SkeletonJson(atlas);
		json.setScale(0.6f);
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy/spineboy-pro.json"));

		skeleton = new Skeleton(skeletonData);

		AnimationStateData stateData = new AnimationStateData(skeletonData);
		stateData.setMix("run", "jump", 0.2f);
		stateData.setMix("jump", "run", 0.2f);

		state = new AnimationState(stateData);
		state.setTimeScale(0.5f);
		state.setAnimation(0, "run", true);
		state.addAnimation(0, "jump", false, 2);
		state.addAnimation(0, "run", true, 0);

		skeletonActor = new SkeletonActor(renderer, skeleton, state);

		stage.addActor(skeletonActor);
		skeletonActor.setPosition(200, 50);
	}

	public void render () {
		float delta = Gdx.graphics.getDeltaTime();
		state.update(delta);

		state.apply(skeleton);
		skeleton.update(delta);
		skeleton.updateWorldTransform(Physics.update);

		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		stage.draw();
	}

	public void resize (int width, int height) {
		stage.getViewport().update(width, height, true);
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		new Lwjgl3Application(new ActorTest());
	}
}
