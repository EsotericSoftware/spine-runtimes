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
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;

/** Smoke test for sequence attachments using the Dragon example. Mirrors the old runtime Sequence example setup: dragon-ess.skel
 * + dragon-pma.atlas + flying animation. */
public class SequenceTest extends ApplicationAdapter {
	OrthographicCamera camera;
	PolygonSpriteBatch batch;
	SkeletonRenderer renderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;

	public void create () {
		camera = new OrthographicCamera();
		camera.setToOrtho(false);
		batch = new PolygonSpriteBatch();
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);

		atlas = new TextureAtlas(Gdx.files.internal("dragon/dragon-pma.atlas"));
		SkeletonBinary binary = new SkeletonBinary(atlas);
		SkeletonData skeletonData = binary.readSkeletonData(Gdx.files.internal("dragon/dragon-ess.skel"));

		skeleton = new Skeleton(skeletonData);
		skeleton.setScale(0.6f, 0.6f);
		skeleton.setPosition(Gdx.graphics.getWidth() / 2f, 120);

		state = new AnimationState(new AnimationStateData(skeletonData));
		state.setAnimation(0, "flying", true);
	}

	public void render () {
		float delta = Gdx.graphics.getDeltaTime();
		state.update(delta);
		state.apply(skeleton);
		skeleton.update(delta);
		skeleton.updateWorldTransform(Physics.update);

		Gdx.gl.glClearColor(0.5f, 0.5f, 0.5f, 1);
		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		camera.update();
		batch.getProjectionMatrix().set(camera.combined);

		batch.begin();
		renderer.draw(batch, skeleton);
		batch.end();
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false);
		skeleton.setPosition(width / 2f, 120);
	}

	public void dispose () {
		atlas.dispose();
		batch.dispose();
	}

	static public void main (String[] args) throws Exception {
		new Lwjgl3Application(new SequenceTest());
	}
}
