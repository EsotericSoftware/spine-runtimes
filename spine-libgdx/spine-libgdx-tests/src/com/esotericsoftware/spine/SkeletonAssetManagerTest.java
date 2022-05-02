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

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.assets.AssetManager;
import com.badlogic.gdx.backends.lwjgl3.Lwjgl3Application;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.ScreenUtils;

import com.esotericsoftware.spine.utils.SkeletonDataLoader;
import com.esotericsoftware.spine.utils.SkeletonDataLoader.SkeletonDataParameter;

/** Demonstrates loading an atlas and skeleton using {@link AssetManager}. */
public class SkeletonAssetManagerTest extends ApplicationAdapter {
	OrthographicCamera camera;
	PolygonSpriteBatch batch;
	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;

	AssetManager assetManager;
	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;

	public void create () {
		camera = new OrthographicCamera();
		batch = new PolygonSpriteBatch();
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true); // PMA results in correct blending without outlines.
		debugRenderer = new SkeletonRendererDebug();
		debugRenderer.setBoundingBoxes(false);
		debugRenderer.setRegionAttachments(false);

		assetManager = new AssetManager();
		assetManager.setLoader(SkeletonData.class, new SkeletonDataLoader(assetManager.getFileHandleResolver()));

		float scale = 0.6f;
		assetManager.load("spineboy/spineboy-pro.json", SkeletonData.class,
			new SkeletonDataParameter("spineboy/spineboy-pma.atlas", scale));
	}

	public void render () {
		ScreenUtils.clear(0, 0, 0, 0);

		if (skeleton == null) {
			// Not loaded yet.
			assetManager.update(16);
			// System.out.println(assetManager.getDiagnostics());
			if (!assetManager.isFinished()) return;

			// Assets are ready, set things up using them.
			SkeletonData skeletonData = assetManager.get("spineboy/spineboy-pro.json");

			skeleton = new Skeleton(skeletonData); // Skeleton holds skeleton state (bone positions, slot attachments, etc).
			skeleton.setPosition(250, 20);

			// Define the default mixing (crossfading) between animations.
			AnimationStateData stateData = new AnimationStateData(skeletonData);
			stateData.setMix("run", "jump", 0.2f);
			stateData.setMix("jump", "run", 0.2f);

			state = new AnimationState(stateData); // Holds the animation state for a skeleton (current animation, time, etc).
			state.setTimeScale(0.5f); // Slow all animations down to 50% speed.

			// Queue animations on track 0.
			state.setAnimation(0, "run", true);
			state.addAnimation(0, "jump", false, 2); // Jump after 2 seconds.
			state.addAnimation(0, "run", true, 0); // Run after the jump.
		}

		state.update(Gdx.graphics.getDeltaTime()); // Update the animation time.

		state.apply(skeleton); // Poses skeleton using current animations. This sets the bones' local SRT.
		skeleton.updateWorldTransform(); // Uses the bones' local SRT to compute their world SRT.

		// Configure the camera, SpriteBatch, and SkeletonRendererDebug.
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);
		debugRenderer.getShapeRenderer().setProjectionMatrix(camera.combined);

		batch.begin();
		renderer.draw(batch, skeleton); // Draw the skeleton images.
		batch.end();

		debugRenderer.draw(skeleton); // Draw debug lines.
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false); // Update camera with new size.
	}

	public void dispose () {
		assetManager.dispose();
	}

	public static void main (String[] args) throws Exception {
		new Lwjgl3Application(new SkeletonAssetManagerTest());
	}
}
