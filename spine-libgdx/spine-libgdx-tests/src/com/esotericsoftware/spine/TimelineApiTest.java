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
import com.badlogic.gdx.backends.lwjgl3.Lwjgl3Application;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.ScreenUtils;

import com.esotericsoftware.spine.Animation.MixBlend;
import com.esotericsoftware.spine.Animation.MixDirection;

/** Demonstrates using the timeline API. See {@link SimpleTest1} for a higher level API using {@link AnimationState}.
 * <p>
 * See: http://esotericsoftware.com/spine-applying-animations */
public class TimelineApiTest extends ApplicationAdapter {
	SpriteBatch batch;
	float time;
	Array<Event> events = new Array();

	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;

	SkeletonData skeletonData;
	Skeleton skeleton;
	Animation walkAnimation;
	Animation jumpAnimation;

	public void create () {
		batch = new SpriteBatch();
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);
		debugRenderer = new SkeletonRendererDebug();

		final String name = "spineboy/spineboy";

		TextureAtlas atlas = new TextureAtlas(Gdx.files.internal(name + "-pma.atlas"));

		if (true) {
			SkeletonJson json = new SkeletonJson(atlas);
			json.setScale(0.6f);
			skeletonData = json.readSkeletonData(Gdx.files.internal(name + "-ess.json"));
		} else {
			SkeletonBinary binary = new SkeletonBinary(atlas);
			binary.setScale(0.6f);
			skeletonData = binary.readSkeletonData(Gdx.files.internal(name + "-ess.skel"));
		}
		walkAnimation = skeletonData.findAnimation("walk");
		jumpAnimation = skeletonData.findAnimation("jump");

		skeleton = new Skeleton(skeletonData);
		skeleton.updateWorldTransform();
		skeleton.setPosition(-50, 20);
	}

	public void render () {
		float delta = Gdx.graphics.getDeltaTime() * 0.25f; // Reduced to make mixing easier to see.

		float jump = jumpAnimation.getDuration();
		float beforeJump = 1f;
		float blendIn = 0.2f;
		float blendOut = 0.2f;
		float blendOutStart = beforeJump + jump - blendOut;
		float total = 3.75f;

		time += delta;

		float speed = 180;
		if (time > beforeJump + blendIn && time < blendOutStart) speed = 360;
		skeleton.setX(skeleton.getX() + speed * delta);

		ScreenUtils.clear(0, 0, 0, 0);

		// This shows how to manage state manually. See SimpleTest1 for a higher level API using AnimationState.
		if (time > total) {
			// restart
			time = 0;
			skeleton.setX(-50);
		} else if (time > beforeJump + jump) {
			// just walk after jump
			walkAnimation.apply(skeleton, time, time, true, events, 1, MixBlend.first, MixDirection.in);
		} else if (time > blendOutStart) {
			// blend out jump
			walkAnimation.apply(skeleton, time, time, true, events, 1, MixBlend.first, MixDirection.in);
			jumpAnimation.apply(skeleton, time - beforeJump, time - beforeJump, false, events, 1 - (time - blendOutStart) / blendOut,
				MixBlend.first, MixDirection.in);
		} else if (time > beforeJump + blendIn) {
			// just jump
			jumpAnimation.apply(skeleton, time - beforeJump, time - beforeJump, false, events, 1, MixBlend.first, MixDirection.in);
		} else if (time > beforeJump) {
			// blend in jump
			walkAnimation.apply(skeleton, time, time, true, events, 1, MixBlend.first, MixDirection.in);
			jumpAnimation.apply(skeleton, time - beforeJump, time - beforeJump, false, events, (time - beforeJump) / blendIn,
				MixBlend.first, MixDirection.in);
		} else {
			// just walk before jump
			walkAnimation.apply(skeleton, time, time, true, events, 1, MixBlend.first, MixDirection.in);
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

	public static void main (String[] args) throws Exception {
		new Lwjgl3Application(new TimelineApiTest());
	}
}
