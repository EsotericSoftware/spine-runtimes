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
import com.badlogic.gdx.Input;
import com.badlogic.gdx.backends.lwjgl3.Lwjgl3Application;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.Pixmap;
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.graphics.glutils.FrameBuffer;
import com.badlogic.gdx.utils.ScreenUtils;

import com.esotericsoftware.spine.utils.TwoColorPolygonBatch;

/** Demonstrates rendering an animation to a frame buffer (FBO) and then rendering the FBO to the screen. */
public class FboTest extends ApplicationAdapter {
	OrthographicCamera camera;
	TwoColorPolygonBatch batch;
	SkeletonRenderer renderer;
	BitmapFont font;

	TextureAtlas atlas;
	Skeleton skeleton;

	FrameBuffer fbo;
	TextureRegion fboRegion;
	boolean drawFbo = true;

	public void create () {
		camera = new OrthographicCamera();
		batch = new TwoColorPolygonBatch();
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);
		font = new BitmapFont();
		font.setColor(Color.BLACK);

		// Load the atlas and skeleton.
		atlas = new TextureAtlas(Gdx.files.internal("spineboy/spineboy-pma.atlas"));
		SkeletonJson json = new SkeletonJson(atlas);
		json.setScale(0.66f);
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy/spineboy-ess.json"));

		// Create a skeleton instance, set the position of its root bone, and update its world transform.
		skeleton = new Skeleton(skeletonData);
		skeleton.setPosition(250, 20);
		skeleton.updateWorldTransform();

		// Create an FBO and a texture region with Y flipped.
		fbo = new FrameBuffer(Pixmap.Format.RGBA8888, 512, 512, false);
		fboRegion = new TextureRegion(fbo.getColorBufferTexture());
		fboRegion.flip(false, true);

		// Configure the camera and batch for rendering to the FBO's size.
		camera.setToOrtho(false, fbo.getWidth(), fbo.getHeight());
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);

		// Render the skeleton to the FBO.
		fbo.begin();
		ScreenUtils.clear(0, 0, 0, 0);
		batch.begin();
		renderer.draw(batch, skeleton);
		batch.end();
		fbo.end();

		// Configure the camera and batch for rendering to the screen's size.
		camera.setToOrtho(false, Gdx.graphics.getWidth(), Gdx.graphics.getHeight());
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);
	}

	public void render () {
		ScreenUtils.clear(1, 1, 1, 1);

		batch.begin();

		if (drawFbo) {
			// Render the FBO color buffer texture to screen.
			batch.draw(fboRegion, 0, 0);
		} else {
			// Render the skeleton directly to the screen.
			renderer.draw(batch, skeleton);
		}

		font.draw(batch, drawFbo ? "Drawing FBO." : "Not drawing FBO.", 10, 10 + font.getCapHeight());
		batch.end();

		if (Gdx.input.justTouched() || Gdx.input.isKeyJustPressed(Input.Keys.SPACE)) {
			drawFbo = !drawFbo;
			Gdx.app.log("SpineFBOTest", "Using FBO: " + drawFbo);
		}
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false, width, height);
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);
	}

	static public void main (String[] args) throws Exception {
		new Lwjgl3Application(new FboTest());
	}
}
