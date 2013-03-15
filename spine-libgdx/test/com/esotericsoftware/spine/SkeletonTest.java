
package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.Input.Keys;
import com.badlogic.gdx.InputAdapter;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.Pixmap;
import com.badlogic.gdx.graphics.Pixmap.Format;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.TextureAtlasData;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;

public class SkeletonTest extends ApplicationAdapter {
	SpriteBatch batch;
	float time;
	ShapeRenderer renderer;

	SkeletonData skeletonData;
	Skeleton skeleton;
	Animation animation;

	public void create () {
		batch = new SpriteBatch();
		renderer = new ShapeRenderer();

		final String name = "goblins";

		// A regular texture atlas would normally usually be used. This returns a white image for images not found in the atlas.
		Pixmap pixmap = new Pixmap(32, 32, Format.RGBA8888);
		pixmap.setColor(Color.WHITE);
		pixmap.fill();
		final AtlasRegion fake = new AtlasRegion(new Texture(pixmap), 0, 0, 32, 32);
		pixmap.dispose();
		FileHandle atlasFile = Gdx.files.internal(name + ".atlas");
		TextureAtlasData data = !atlasFile.exists() ? null : new TextureAtlasData(atlasFile, atlasFile.parent(), false);
		TextureAtlas atlas = new TextureAtlas(data) {
			public AtlasRegion findRegion (String name) {
				AtlasRegion region = super.findRegion(name);
				return region != null ? region : fake;
			}
		};

		if (true) {
			SkeletonJson json = new SkeletonJson(atlas);
			// json.setScale(2);
			skeletonData = json.readSkeletonData(Gdx.files.internal(name + "-skeleton.json"));
			animation = json.readAnimation(Gdx.files.internal(name + "-walk.json"), skeletonData);
		} else {
			SkeletonBinary binary = new SkeletonBinary(atlas);
			// binary.setScale(2);
			skeletonData = binary.readSkeletonData(Gdx.files.internal(name + ".skel"));
			animation = binary.readAnimation(Gdx.files.internal(name + "-walk.anim"), skeletonData);
		}

		skeleton = new Skeleton(skeletonData);
		if (name.equals("goblins")) skeleton.setSkin("goblin");
		skeleton.setToBindPose();

		Bone root = skeleton.getRootBone();
		root.x = 50;
		root.y = 20;
		root.scaleX = 1f;
		root.scaleY = 1f;
		skeleton.updateWorldTransform();

		Gdx.input.setInputProcessor(new InputAdapter() {
			public boolean keyDown (int keycode) {
				if (keycode == Keys.SPACE) {
					if (name.equals("goblins")) {
						skeleton.setSkin(skeleton.getSkin().getName().equals("goblin") ? "goblingirl" : "goblin");
						skeleton.setSlotsToBindPose();
					}
				}
				return true;
			}
		});
	}

	public void render () {
		time += Gdx.graphics.getDeltaTime();

		Bone root = skeleton.getRootBone();
		float x = root.getX() + 160 * Gdx.graphics.getDeltaTime() * (skeleton.getFlipX() ? -1 : 1);
		if (x > Gdx.graphics.getWidth()) skeleton.setFlipX(true);
		if (x < 0) skeleton.setFlipX(false);
		root.setX(x);

		Gdx.gl.glClear(GL10.GL_COLOR_BUFFER_BIT);
		batch.begin();
		batch.setColor(Color.GRAY);

		animation.apply(skeleton, time, true);
		skeleton.updateWorldTransform();
		skeleton.update(Gdx.graphics.getDeltaTime());
		skeleton.draw(batch);

		batch.end();

		skeleton.drawDebug(renderer);
	}

	public void resize (int width, int height) {
		batch.getProjectionMatrix().setToOrtho2D(0, 0, width, height);
		renderer.setProjectionMatrix(batch.getProjectionMatrix());
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new SkeletonTest());
	}
}
