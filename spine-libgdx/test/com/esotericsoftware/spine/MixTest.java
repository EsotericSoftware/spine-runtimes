
package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;

public class MixTest extends ApplicationAdapter {
	SpriteBatch batch;
	float time;
	ShapeRenderer renderer;

	SkeletonData skeletonData;
	Skeleton skeleton;
	Animation walkAnimation;
	Animation jumpAnimation;

	public void create () {
		batch = new SpriteBatch();
		renderer = new ShapeRenderer();

		final String name = "spineboy";

		TextureAtlas atlas = new TextureAtlas(Gdx.files.internal(name + ".atlas"));

		if (true) {
			SkeletonJson json = new SkeletonJson(atlas);
			// json.setScale(2);
			skeletonData = json.readSkeletonData(Gdx.files.internal(name + "-skeleton.json"));
			walkAnimation = json.readAnimation(Gdx.files.internal(name + "-walk.json"), skeletonData);
			jumpAnimation = json.readAnimation(Gdx.files.internal(name + "-jump.json"), skeletonData);
		} else {
			SkeletonBinary binary = new SkeletonBinary(atlas);
			// binary.setScale(2);
			skeletonData = binary.readSkeletonData(Gdx.files.internal(name + ".skel"));
			walkAnimation = binary.readAnimation(Gdx.files.internal(name + "-walk.anim"), skeletonData);
			jumpAnimation = binary.readAnimation(Gdx.files.internal(name + "-jump.anim"), skeletonData);
		}

		skeleton = new Skeleton(skeletonData);
		skeleton.setToBindPose();

		final Bone root = skeleton.getRootBone();
		root.x = -50;
		root.y = 20;
		root.scaleX = 1f;
		root.scaleY = 1f;
		skeleton.updateWorldTransform();
	}

	public void render () {
		float delta = Gdx.graphics.getDeltaTime() * 0.25f; // Reduced to make mixing easier to see.

		float jump = jumpAnimation.getDuration();
		float beforeJump = 1f;
		float blendIn = 0.4f;
		float blendOut = 0.4f;
		float blendOutStart = beforeJump + jump - blendOut;
		float total = 3.75f;

		time += delta;

		Bone root = skeleton.getRootBone();
		float speed = 180;
		if (time > beforeJump + blendIn && time < blendOutStart) speed = 360;
		root.setX(root.getX() + speed * delta);

		Gdx.gl.glClear(GL10.GL_COLOR_BUFFER_BIT);
		batch.begin();
		batch.setColor(Color.GRAY);

		// This shows how to manage state manually. See AnimationStatesTest.
		if (time > total) {
			// restart
			time = 0;
			root.setX(-50);
		} else if (time > beforeJump + jump) {
			// just walk after jump
			walkAnimation.apply(skeleton, time, true);
		} else if (time > blendOutStart) {
			// blend out jump
			walkAnimation.apply(skeleton, time, true);
			jumpAnimation.mix(skeleton, time - beforeJump, false, 1 - (time - blendOutStart) / blendOut);
		} else if (time > beforeJump + blendIn) {
			// just jump
			jumpAnimation.apply(skeleton, time - beforeJump, false);
		} else if (time > beforeJump) {
			// blend in jump
			walkAnimation.apply(skeleton, time, true);
			jumpAnimation.mix(skeleton, time - beforeJump, false, (time - beforeJump) / blendIn);
		} else {
			// just walk before jump
			walkAnimation.apply(skeleton, time, true);
		}

		skeleton.updateWorldTransform();
		skeleton.update(Gdx.graphics.getDeltaTime());
		skeleton.draw(batch);

		batch.end();

		// skeleton.drawDebug(renderer);
	}

	public void resize (int width, int height) {
		batch.getProjectionMatrix().setToOrtho2D(0, 0, width, height);
		renderer.setProjectionMatrix(batch.getProjectionMatrix());
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new MixTest());
	}
}
