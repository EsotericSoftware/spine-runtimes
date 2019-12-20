/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.math.Interpolation;
import com.esotericsoftware.spine.vertexeffects.SwirlEffect;

public class VertexEffectTest extends ApplicationAdapter {
	OrthographicCamera camera;
	PolygonSpriteBatch batch;
	SkeletonRenderer renderer;

	SwirlEffect swirl;
	float swirlTime;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;

	public void create () {
		camera = new OrthographicCamera();
		batch = new PolygonSpriteBatch(); // Required to render meshes. SpriteBatch can't render meshes.
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);

		atlas = new TextureAtlas(Gdx.files.internal("raptor/raptor-pma.atlas"));
		SkeletonJson json = new SkeletonJson(atlas); // This loads skeleton JSON data, which is stateless.
		json.setScale(0.5f); // Load the skeleton at 50% the size it was in Spine.
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("raptor/raptor-pro.json"));

		skeleton = new Skeleton(skeletonData); // Skeleton holds skeleton state (bone positions, slot attachments, etc).
		skeleton.setPosition(350, 45);

		AnimationStateData stateData = new AnimationStateData(skeletonData); // Defines mixing (crossfading) between animations.

		state = new AnimationState(stateData); // Holds the animation state for a skeleton (current animation, time, etc).
		state.setTimeScale(0.6f); // Slow all animations down to 60% speed.

		// Queue animations on tracks 0 and 1.
		state.setAnimation(0, "walk", true);
		state.addAnimation(1, "gun-grab", false, 2); // Keys in higher tracks override the pose from lower tracks.
		
		swirl = new SwirlEffect(400);
		swirl.setCenter(0, 200);
		renderer.setVertexEffect(swirl);
		// renderer.setVertexEffect(new JitterEffect(10, 10));
	}

	public void render () {
		// Update the skeleton and animation time.
		float delta = Gdx.graphics.getDeltaTime();
		skeleton.update(delta);
		state.update(delta);

		swirlTime += delta;
		float percent = swirlTime % 2;
		if (percent > 1) percent = 1 - (percent - 1);
		swirl.setAngle(Interpolation.pow2.apply(-60, 60, percent));

		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		state.apply(skeleton); // Poses skeleton using current animations. This sets the bones' local SRT.
		skeleton.updateWorldTransform(); // Uses the bones' local SRT to compute their world SRT.

		// Configure the camera, SpriteBatch, and SkeletonRendererDebug.
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);

		batch.begin();
		renderer.draw(batch, skeleton); // Draw the skeleton images.
		batch.end();
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false); // Update camera with new size.
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		config.width = 800;
		config.height = 600;
		new LwjglApplication(new VertexEffectTest(), config);
	}
}
