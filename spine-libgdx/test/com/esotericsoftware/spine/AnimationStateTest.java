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

import com.esotericsoftware.spine.AnimationState.AnimationStateListener;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.InputAdapter;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.math.Vector3;

public class AnimationStateTest extends ApplicationAdapter {
	OrthographicCamera camera;
	SpriteBatch batch;
	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	SkeletonBounds bounds;
	AnimationState state;

	public void create () {
		camera = new OrthographicCamera();
		batch = new SpriteBatch();
		renderer = new SkeletonRenderer();
		debugRenderer = new SkeletonRendererDebug();

		atlas = new TextureAtlas(Gdx.files.internal("spineboy.atlas"));
		SkeletonJson json = new SkeletonJson(atlas); // This loads skeleton JSON data.
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy.json")); // SkeletonData is stateless.

		skeleton = new Skeleton(skeletonData); // Skeleton holds skeleton state (bone positions, slot attachments, etc).
		skeleton.setX(250);
		skeleton.setY(20);

		bounds = new SkeletonBounds(); // Convenience class to do hit detection with bounding boxes.

		AnimationStateData stateData = new AnimationStateData(skeletonData); // Defines mixing (crossfading) between animations.
		stateData.setMix("walk", "jump", 0.2f);
		stateData.setMix("jump", "walk", 0.4f);
		stateData.setMix("jump", "jump", 0.2f);

		state = new AnimationState(stateData); // Holds the animation state for a skeleton (current animation, time, etc).
		state.addListener(new AnimationStateListener() {
			public void event (int trackIndex, Event event) {
				System.out.println(trackIndex + " event: " + state.getCurrent(trackIndex) + ", " + event.getData().getName());
			}

			public void complete (int trackIndex, int loopCount) {
				System.out.println(trackIndex + " complete: " + state.getCurrent(trackIndex) + ", " + loopCount);
			}

			public void start (int trackIndex) {
				System.out.println(trackIndex + " start: " + state.getCurrent(trackIndex));
			}

			public void end (int trackIndex) {
				System.out.println(trackIndex + " end: " + state.getCurrent(trackIndex));
			}
		});
		state.setAnimation(0, "drawOrder", true);

		Gdx.input.setInputProcessor(new InputAdapter() {
			final Vector3 point = new Vector3();

			public boolean touchDown (int screenX, int screenY, int pointer, int button) {
				camera.unproject(point.set(screenX, screenY, 0)); // Convert window to world coordinates.
				bounds.update(skeleton, true); // Update SkeletonBounds with current skeleton bounding box positions.
				if (bounds.aabbContainsPoint(point.x, point.y)) { // Check if inside AABB first. This check is fast.
					BoundingBoxAttachment hit = bounds.containsPoint(point.x, point.y); // Check if inside a bounding box.
					if (hit != null) {
						System.out.println("hit: " + hit);
						skeleton.findSlot("head").getColor().set(Color.RED); // Turn head red until touchUp.
					}
				}
				return true;
			}

			public boolean touchUp (int screenX, int screenY, int pointer, int button) {
				skeleton.findSlot("head").getColor().set(Color.WHITE);
				return true;
			}

			public boolean keyDown (int keycode) {
				state.setAnimation(0, "jump", false); // Set animation on track 0 to jump.
				state.addAnimation(0, "walk", true, 0); // Queue walk to play after jump.
				return true;
			}
		});
	}

	public void render () {
		state.update(Gdx.graphics.getDeltaTime());

		Gdx.gl.glClear(GL10.GL_COLOR_BUFFER_BIT);

		state.apply(skeleton); // Poses skeleton using current animations. This sets the bone's local SRT.
		skeleton.updateWorldTransform(); // Uses the bone's local SRT to set their world SRT.

		// Configure the camera, SpriteBatch, and SkeletonRendererDebug.
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);
		debugRenderer.getShapeRenderer().getProjectionMatrix().set(camera.combined);

		batch.begin();
		renderer.draw(batch, skeleton); // Draw the skeleton images.
		batch.end();

		debugRenderer.draw(skeleton); // Draw debug lines.
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false); // Update camera with new size.
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new AnimationStateTest());
	}
}
