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
import com.badlogic.gdx.backends.lwjgl3.Lwjgl3ApplicationConfiguration;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.Pixmap;
import com.badlogic.gdx.graphics.Pixmap.Format;
import com.badlogic.gdx.graphics.PixmapIO;
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.graphics.glutils.FrameBuffer;
import com.badlogic.gdx.utils.ScreenUtils;

import com.esotericsoftware.spine.Animation.MixBlend;
import com.esotericsoftware.spine.Animation.MixDirection;
import com.esotericsoftware.spine.utils.TwoColorPolygonBatch;

/** Demonstrates rendering an animation to a frame buffer (FBO) and then writing each frame as a PNG. */
public class PngExportTest extends ApplicationAdapter {
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

		// Create an FBO and a texture region.
		fbo = new FrameBuffer(Pixmap.Format.RGBA8888, 512, 512, false);
		fboRegion = new TextureRegion(fbo.getColorBufferTexture());

		// Create a pixmap of the same size.
		Pixmap pixmap = new Pixmap(fbo.getWidth(), fbo.getHeight(), Format.RGBA8888);

		// Configure the camera and batch for rendering to the FBO's size.
		camera.setToOrtho(true, fbo.getWidth(), fbo.getHeight());
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);

		// Start rendering to the FBO.
		fbo.begin();

		// Pose the skeleton at regular intervals throughout the animation.
		Animation animation = skeletonData.findAnimation("run");
		float fps = 1 / 15f, time = 0;
		int frame = 1;
		while (time < animation.getDuration()) {
			animation.apply(skeleton, time, time, false, null, 1, MixBlend.first, MixDirection.in);
			skeleton.updateWorldTransform();

			// Render the skeleton to the FBO.
			ScreenUtils.clear(0, 0, 0, 0);
			batch.begin();
			renderer.draw(batch, skeleton);
			batch.end();

			// Copy the FBO to the pixmap and write it to a PNG file.
			String name = animation.getName() + "_" + frame + ".png";
			System.out.println(name);
			Gdx.gl.glPixelStorei(GL20.GL_PACK_ALIGNMENT, 1); // Have glReadPixels use byte alignment for each pixel row.
			Gdx.gl.glReadPixels(0, 0, fbo.getWidth(), fbo.getHeight(), GL20.GL_RGBA, GL20.GL_UNSIGNED_BYTE, pixmap.getPixels());
			PixmapIO.writePNG(new FileHandle(name), pixmap);

			frame++;
			time += fps;
		}

		pixmap.dispose();
		fbo.end();

		// Terminate without showing a window.
		System.exit(0);
	}

	static public void main (String[] args) throws Exception {
		Lwjgl3ApplicationConfiguration config = new Lwjgl3ApplicationConfiguration();
		config.setInitialVisible(false);
		new Lwjgl3Application(new PngExportTest(), config);
	}
}
