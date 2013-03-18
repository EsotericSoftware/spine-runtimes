
package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;

public class AnimationStateTest extends ApplicationAdapter {
	SpriteBatch batch;
	ShapeRenderer renderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	Animation walkAnimation;
	Animation jumpAnimation;
	Bone root;
	AnimationState state;

	public void create () {
		batch = new SpriteBatch();
		renderer = new ShapeRenderer();

		atlas = new TextureAtlas(Gdx.files.internal("spineboy.atlas"));
		SkeletonJson json = new SkeletonJson(atlas);
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy-skeleton.json"));
		walkAnimation = json.readAnimation(Gdx.files.internal("spineboy-walk.json"), skeletonData);
		jumpAnimation = json.readAnimation(Gdx.files.internal("spineboy-jump.json"), skeletonData);

		state = new AnimationState();
		// Define mixing from one animation to another.
		state.setMixing(walkAnimation, jumpAnimation, 0.4f);
		// Set current animation.
		state.setAnimation(walkAnimation, true);

		skeleton = new Skeleton(skeletonData);

		root = skeleton.getRootBone();
		root.setX(250);
		root.setY(20);

		skeleton.updateWorldTransform();
	}

	public void render () {
		state.update(Gdx.graphics.getDeltaTime());

		Gdx.gl.glClear(GL10.GL_COLOR_BUFFER_BIT);
		batch.begin();

		state.apply(skeleton);
		// After one second, change the current animation. Mixing is done by AnimationState for you.
		if (state.getTime() > 1 && state.getAnimation() == walkAnimation) state.setAnimation(jumpAnimation, false);
		skeleton.updateWorldTransform();
		skeleton.draw(batch);

		batch.end();
	}

	public void resize (int width, int height) {
		batch.getProjectionMatrix().setToOrtho2D(0, 0, width, height);
		renderer.setProjectionMatrix(batch.getProjectionMatrix());
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new AnimationStateTest());
	}
}
