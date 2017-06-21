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
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.Animation.MixDirection;
import com.esotericsoftware.spine.Animation.MixPose;

public class MixTest extends ApplicationAdapter {
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

		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		// This shows how to manage state manually. See SimpleTest1 for a higher level API using AnimationState.
		if (time > total) {
			// restart
			time = 0;
			skeleton.setX(-50);
		} else if (time > beforeJump + jump) {
			// just walk after jump
			walkAnimation.apply(skeleton, time, time, true, events, 1, MixPose.current, MixDirection.in);
		} else if (time > blendOutStart) {
			// blend out jump
			walkAnimation.apply(skeleton, time, time, true, events, 1, MixPose.current, MixDirection.in);
			jumpAnimation.apply(skeleton, time - beforeJump, time - beforeJump, false, events, 1 - (time - blendOutStart) / blendOut,
				MixPose.current, MixDirection.in);
		} else if (time > beforeJump + blendIn) {
			// just jump
			jumpAnimation.apply(skeleton, time - beforeJump, time - beforeJump, false, events, 1, MixPose.current, MixDirection.in);
		} else if (time > beforeJump) {
			// blend in jump
			walkAnimation.apply(skeleton, time, time, true, events, 1, MixPose.current, MixDirection.in);
			jumpAnimation.apply(skeleton, time - beforeJump, time - beforeJump, false, events, (time - beforeJump) / blendIn,
				MixPose.current, MixDirection.in);
		} else {
			// just walk before jump
			walkAnimation.apply(skeleton, time, time, true, events, 1, MixPose.current, MixDirection.in);
		}

		skeleton.updateWorldTransform();
		skeleton.update(Gdx.graphics.getDeltaTime());

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
		new LwjglApplication(new MixTest());
	}
}
