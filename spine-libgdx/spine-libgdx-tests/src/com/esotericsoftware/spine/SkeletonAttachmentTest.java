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
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.ScreenUtils;

import com.esotericsoftware.spine.attachments.SkeletonAttachment;

/** Demonstrates using {@link SkeletonAttachment} to use an entire skeleton as an attachment. */
public class SkeletonAttachmentTest extends ApplicationAdapter {
	OrthographicCamera camera;
	PolygonSpriteBatch batch;
	SkeletonRenderer renderer;

	Skeleton spineboy, goblin;
	AnimationState spineboyState, goblinState;
	Bone attachmentBone;

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
			Slot slot = spineboy.findSlot("front-upper-arm");
			slot.setAttachment(skeletonAttachment);
			attachmentBone = slot.getBone();
		}
	}

	public void render () {
		spineboyState.update(Gdx.graphics.getDeltaTime());
		spineboyState.apply(spineboy);
		spineboy.updateWorldTransform();

		goblinState.update(Gdx.graphics.getDeltaTime());
		goblinState.apply(goblin);
		goblin.updateWorldTransform(attachmentBone);

		ScreenUtils.clear(0, 0, 0, 0);

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
		new Lwjgl3Application(new SkeletonAttachmentTest());
	}
}
