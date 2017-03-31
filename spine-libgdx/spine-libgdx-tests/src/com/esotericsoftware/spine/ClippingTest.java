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
import com.badlogic.gdx.Input.Keys;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.math.WindowedMean;
import com.esotericsoftware.spine.attachments.ClippingAttachment;

public class ClippingTest extends ApplicationAdapter {
	OrthographicCamera camera;
	PolygonSpriteBatch batch;
	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;
	BitmapFont font;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;
	
	WindowedMean mean = new WindowedMean(30);	

	public void create () {
		camera = new OrthographicCamera();
		batch = new PolygonSpriteBatch(2048);
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);
		renderer.setSoftwareClipping(true);
		debugRenderer = new SkeletonRendererDebug();
		debugRenderer.setBoundingBoxes(false);
		debugRenderer.setRegionAttachments(false);
		font = new BitmapFont();

		atlas = new TextureAtlas(Gdx.files.internal("raptor/raptor-pma.atlas"));
		SkeletonJson json = new SkeletonJson(atlas);
		json.setScale(0.6f);
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("raptor/raptor.json"));

		skeleton = new Skeleton(skeletonData);
		skeleton.setPosition(250, 20);

		AnimationStateData stateData = new AnimationStateData(skeletonData);
//		stateData.setMix("run", "jump", 0.2f);
//		stateData.setMix("jump", "run", 0.2f);

		state = new AnimationState(stateData);
		state.setTimeScale(0.5f);

		state.setAnimation(0, "walk", true);
		state.addAnimation(0, "Jump", false, 2);
		state.addAnimation(0, "walk", true, 0);

		// Create a clipping attachment, slot data, and slot.
		ClippingAttachment clip = new ClippingAttachment("clip");
		// Rectangle:
		clip.setVertices(
//			new float[] { 87, 288, 217, 371, 456, 361, 539, 175, 304, 194, 392, 290, 193, 214, 123, 15, 14, 137 });	
		new float[] { //
			-140, 50, //
			250, 50, //
			250, 350, //
			-140, 350, //
		});
		// Self intersection:
//		clip.setVertices(new float[] { //
//			-140, -50, //
//			120, 50, //
//			120, -50, //
//			-140, 50, //
//		});
		clip.setWorldVerticesLength(8);
		clip.setEndSlot(skeleton.findSlot("front_hand").data.index);

		SlotData clipSlotData = new SlotData(skeletonData.getSlots().size, "clip slot", skeletonData.getBones().first());
		skeletonData.getSlots().add(clipSlotData);

		Slot clipSlot = new Slot(clipSlotData, skeleton.getRootBone());
		clipSlot.setAttachment(clip);
		skeleton.getSlots().add(clipSlot);
		skeleton.getDrawOrder().insert(skeletonData.findSlot("back_hand").getIndex(), clipSlot);
	}

	public void render () {
		 state.update(Gdx.graphics.getDeltaTime() * 0.3f);
		state.update(0);

		Gdx.gl.glClearColor(0.3f, 0.3f, 0.3f, 1);
		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		state.apply(skeleton);
		skeleton.updateWorldTransform();

		camera.update();
		batch.getProjectionMatrix().set(camera.combined);
		debugRenderer.getShapeRenderer().setProjectionMatrix(camera.combined);

		batch.begin();
		long start = System.nanoTime();
		renderer.draw(batch, skeleton);
		mean.addValue((System.nanoTime() - start) / 1000000.0f);
		renderer.setPremultipliedAlpha(false);
		font.draw(batch, "Time: " + mean.getMean() + "ms", 10, Gdx.graphics.getHeight() - font.getLineHeight());
		batch.end();

		debugRenderer.draw(skeleton);
		
		if (Gdx.input.isKeyJustPressed(Keys.S)) {
			renderer.setSoftwareClipping(!renderer.getSoftwareClipping());
			System.out.println("Software clipping: " + renderer.getSoftwareClipping());
		}
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false);
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		config.stencil = 8;
		new LwjglApplication(new ClippingTest(), config);
	}
}
