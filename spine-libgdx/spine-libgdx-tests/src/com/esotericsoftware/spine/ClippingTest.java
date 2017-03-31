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
// stateData.setMix("run", "jump", 0.2f);
// stateData.setMix("jump", "run", 0.2f);

		state = new AnimationState(stateData);
		state.setTimeScale(0.5f);

		state.setAnimation(0, "walk", true);
		state.addAnimation(0, "Jump", false, 2);
		state.addAnimation(0, "walk", true, 0);

		// Create a clipping attachment, slot data, and slot.
		ClippingAttachment clip = new ClippingAttachment("clip");

		// Spiral.
		clip.setVertices(new float[] {430.90802f, 278.212f, 72.164f, 361.816f, 31.143997f, 128.804f, 191.896f, 61.0f, 291.312f,
			175.73201f, 143.956f, 207.408f, 161.4f, 145.628f, 227.456f, 160.61601f, 224.392f, 126.535995f, 188.264f, 113.144f,
			147.13199f, 108.87601f, 77.035995f, 158.212f, 86.15199f, 220.676f, 102.77199f, 240.716f, 174.74399f, 243.20801f,
			250.572f, 216.74802f, 324.772f, 200.33202f, 309.388f, 124.968f, 258.168f, 60.503998f, 199.696f, 42.872f, 116.951996f,
			6.7400017f, 11.332001f, 72.48f, -6.708008f, 143.136f, 1.0679932f, 239.92801f, 26.5f, 355.6f, -47.380005f, 377.52798f,
			-40.608f, 303.1f, -53.584015f, 77.316f, 5.4600067f, 8.728001f, 113.343994f, -56.04f, 192.42801f, -45.112f, 274.564f,
			-38.784f, 322.592f, -10.604f, 371.98f, 21.920002f, 405.16f, 60.896004f, 428.68f, 104.852005f, 406.996f, 188.976f,
			364.58398f, 220.14401f, 309.3f, 238.788f, 263.232f, 244.75201f, 219.468f, 271.58002f, 210.824f, 294.176f, 250.664f,
			295.2f, 295.972f, 276.02f, 357.46f, 269.172f, 420.008f, 242.37201f, 466.63602f, 207.648f, 437.516f, -10.579998f,
			378.05603f, -64.624f, 465.24f, -104.992f, 554.11206f, 95.43199f, 514.89197f, 259.02f});

		// Polygon:
// clip.setVertices(
// new float[] { 94.0f, 84.0f, 45.0f, 165.0f, 218.0f, 292.0f, 476.0f, 227.0f, 480.0f, 125.0f, 325.0f, 191.0f, 333.0f, 77.0f,
// 302.0f, 30.0f, 175.0f, 140.0f });

		// Rectangle:
// new float[] { //
// -140, 50, //
// 250, 50, //
// 250, 350, //
// -140, 350, //
// });

		// Self intersection:
// clip.setVertices(new float[] { //
// -140, -50, //
// 120, 50, //
// 120, -50, //
// -140, 50, //
// });

		for (int j = 0; j < clip.getVertices().length; j += 2) {
			clip.getVertices()[j] = (clip.getVertices()[j] - 150f);
			clip.getVertices()[j + 1] = (clip.getVertices()[j + 1] + 100);
		}
		clip.setWorldVerticesLength(clip.getVertices().length);
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
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false);
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		config.width = 800;
		config.height = 600;
		new LwjglApplication(new ClippingTest(), config);
	}
}
