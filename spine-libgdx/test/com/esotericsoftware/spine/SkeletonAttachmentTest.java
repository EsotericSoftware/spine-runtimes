/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.AnimationState.AnimationStateAdapter;
import com.esotericsoftware.spine.attachments.SkeletonAttachment;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;

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
			TextureAtlas atlas = new TextureAtlas(Gdx.files.internal("spineboy/spineboy.atlas"));
			SkeletonJson json = new SkeletonJson(atlas);
			json.setScale(0.6f);
			SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy/spineboy.json"));
			spineboy = new Skeleton(skeletonData);
			spineboy.setPosition(320, 20);

			AnimationStateData stateData = new AnimationStateData(skeletonData);
			stateData.setMix("walk", "jump", 0.2f);
			stateData.setMix("jump", "walk", 0.2f);
			spineboyState = new AnimationState(stateData);
			new AnimationStateAdapter() {
				public void start (int trackIndex) {
					spineboyState.addAnimation(0, "walk", true, 0);
					spineboyState.addAnimation(0, "jump", false, 3).setListener(this);
				}
			}.start(0);
		}

		{
			TextureAtlas atlas = new TextureAtlas(Gdx.files.internal("goblins/goblins-ffd.atlas"));
			SkeletonJson json = new SkeletonJson(atlas);
			SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("goblins/goblins-ffd.json"));
			goblin = new Skeleton(skeletonData);
			goblin.setSkin("goblin");
			goblin.setSlotsToSetupPose();

			goblinState = new AnimationState(new AnimationStateData(skeletonData));
			goblinState.setAnimation(0, "walk", true);

			// Instead of a right shoulder, spineboy will have a goblin!
			SkeletonAttachment skeletonAttachment = new SkeletonAttachment("goblin");
			skeletonAttachment.setSkeleton(goblin);
			spineboy.findSlot("front_upper_arm").setAttachment(skeletonAttachment);
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
