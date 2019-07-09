
package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;

public class SkinBonesMixAndMatchTest extends ApplicationAdapter {
	OrthographicCamera camera;
	PolygonSpriteBatch batch;
	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;

	public void create () {
		camera = new OrthographicCamera();
		batch = new PolygonSpriteBatch();
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true); // PMA results in correct blending without outlines.
		debugRenderer = new SkeletonRendererDebug();
		debugRenderer.setBoundingBoxes(false);
		debugRenderer.setRegionAttachments(false);

		atlas = new TextureAtlas(Gdx.files.internal("mix-and-match/mix-and-match-pma.atlas"));
		SkeletonJson json = new SkeletonJson(atlas); // This loads skeleton JSON data, which is stateless.
		json.setScale(0.6f); // Load the skeleton at 60% the size it was in Spine.
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("mix-and-match/mix-and-match-pro.json"));

		skeleton = new Skeleton(skeletonData); // Skeleton holds skeleton state (bone positions, slot attachments, etc).
		skeleton.setPosition(320, 20);

		AnimationStateData stateData = new AnimationStateData(skeletonData); // Defines mixing (crossfading) between animations.
		state = new AnimationState(stateData); // Holds the animation state for a skeleton (current animation, time, etc).

		// Queue animations on track 0.
		state.setAnimation(0, "dance", true);

		// Create a new skin, by mixing and matching other skins
		// that fit together. Items making up the girl are individual
		// skins. Using the skin API, a new skin is created which is
		// a combination of all these individual item skins.
		Skin mixAndMatchSkin = new Skin("custom-girl");
		mixAndMatchSkin.addSkin(skeletonData.findSkin("skin-base"));
		mixAndMatchSkin.addSkin(skeletonData.findSkin("nose/short"));
		mixAndMatchSkin.addSkin(skeletonData.findSkin("eyelids/girly"));
		mixAndMatchSkin.addSkin(skeletonData.findSkin("eyes/violet"));
		mixAndMatchSkin.addSkin(skeletonData.findSkin("hair/brown"));
		mixAndMatchSkin.addSkin(skeletonData.findSkin("clothes/hoodie-orange"));
		mixAndMatchSkin.addSkin(skeletonData.findSkin("legs/pants-jeans"));
		mixAndMatchSkin.addSkin(skeletonData.findSkin("accessories/bag"));
		mixAndMatchSkin.addSkin(skeletonData.findSkin("accessories/hat-red-yellow"));
		skeleton.setSkin(mixAndMatchSkin);
	}

	public void render () {
		state.update(Gdx.graphics.getDeltaTime()); // Update the animation time.

		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		state.apply(skeleton); // Poses skeleton using current animations. This sets the bones' local SRT.
		skeleton.updateWorldTransform(); // Uses the bones' local SRT to compute their world SRT.

		// Configure the camera, and PolygonSpriteBatch
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);

		batch.begin();
		renderer.draw(batch, skeleton); // Draw the skeleton images.
		batch.end();
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false); // Update camera with new size.
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new SkinBonesMixAndMatchTest());
	}
}
