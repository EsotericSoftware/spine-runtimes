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
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.esotericsoftware.spine.attachments.SkeletonAttachment;

public class SkeletonAttachmentTest extends ApplicationAdapter {
	OrthographicCamera camera;
	PolygonSpriteBatch batch;
	SkeletonRenderer renderer;

	Skeleton spineboy, goblin;
	AnimationState spineboyState, goblinState;

	public void create () {
		camera = new OrthographicCamera();
		batch = new PolygonSpriteBatch();
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);

		{
			TextureAtlas atlas = new TextureAtlas(Gdx.files.internal("spineboy/spineboy-pma.atlas"));
			SkeletonJson json = new SkeletonJson(atlas);
			json.setScale(0.6f);
			SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy/spineboy-ess.json"));
			spineboy = new Skeleton(skeletonData);
			spineboy.setPosition(320, 20);

			AnimationStateData stateData = new AnimationStateData(skeletonData);
			stateData.setMix("walk", "jump", 0.2f);
			stateData.setMix("jump", "walk", 0.2f);
			spineboyState = new AnimationState(stateData);
			spineboyState.addAnimation(0, "walk", true, 0);
		}

		{
			TextureAtlas atlas = new TextureAtlas(Gdx.files.internal("goblins/goblins-pma.atlas"));
			SkeletonJson json = new SkeletonJson(atlas);
			SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("goblins/goblins-pro.json"));
			goblin = new Skeleton(skeletonData);
			goblin.setSkin("goblin");
			goblin.setSlotsToSetupPose();

			goblinState = new AnimationState(new AnimationStateData(skeletonData));
			goblinState.setAnimation(0, "walk", true);

			// Instead of a right shoulder, spineboy will have a goblin!
			SkeletonAttachment skeletonAttachment = new SkeletonAttachment("goblin");
			skeletonAttachment.setSkeleton(goblin);
			spineboy.findSlot("front-upper-arm").setAttachment(skeletonAttachment);
		}
	}

	public void render () {
		spineboyState.update(Gdx.graphics.getDeltaTime());
		spineboyState.apply(spineboy);
		spineboy.updateWorldTransform();

		goblinState.update(Gdx.graphics.getDeltaTime());
		goblinState.apply(goblin);
		goblin.updateWorldTransform();

		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		camera.update();
		batch.getProjectionMatrix().set(camera.combined);
		batch.begin();
		renderer.draw(batch, spineboy);
		batch.end();
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false);
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new SkeletonAttachmentTest());
	}
}
