
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
