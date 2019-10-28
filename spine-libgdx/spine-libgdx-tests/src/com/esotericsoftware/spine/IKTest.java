package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.math.Vector3;
import com.esotericsoftware.spine.utils.TwoColorPolygonBatch;

/** Demonstrates how to let the target bone of an IK constraint
 * follow the mouse or touch position, which in turn repositions
 * part of the skeleton, in this case Spineboy's back arm including
 * his gun.
 */
public class IKTest extends ApplicationAdapter {
	OrthographicCamera camera;
	TwoColorPolygonBatch batch;
	SkeletonRenderer renderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;
	
	Vector3 cameraCoords = new Vector3();
	Vector2 boneCoords = new Vector2();

	public void create () {
		// Create objects needed for rendering
		camera = new OrthographicCamera();
		batch = new TwoColorPolygonBatch();
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);

		// Load the texture atlas and skeleton data
		atlas = new TextureAtlas(Gdx.files.internal("spineboy/spineboy-pma.atlas"));
		SkeletonBinary json = new SkeletonBinary(atlas);
		json.setScale(0.6f);		
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy/spineboy-pro.skel"));

		// Create a skeleton from the skeleton data
		skeleton = new Skeleton(skeletonData);
		skeleton.setPosition(250, 20);
		
		// Create an animation satte
		AnimationStateData stateData = new AnimationStateData(skeletonData);
		state = new AnimationState(stateData);
		
		// Queue the "walk" animation on the first track.
		state.setAnimation(0, "walk", true);
		
		// Queue the "aim" animation on a higher track. 
		// It consists of a single frame that positions
		// the back arm and gun such that they point at
		// the "crosshair" bone. By setting this
		// animation on a higher track, it overrides
		// any changes to the back arm and gun made
		// by the walk animation, allowing us to
		// mix the two. The mouse position following
		// is performed in the render() method below.
		state.setAnimation(1, "aim", true);
	}

	public void render () {
		// Update and apply the animations to the skeleton,
		// then calculate the world transforms of every bone.
		// This is needed so we can call Bone#worldToLocal()
		// later.
		state.update(Gdx.graphics.getDeltaTime());		
		state.apply(skeleton);
		skeleton.updateWorldTransform();
		
		// Position the "crosshair" bone at the mouse
		// location. We do this before calling
		// skeleton.updateWorldTransform() below, so
		// our change is incorporated before the IK
		// constraint is applied.
		//
		// When setting the crosshair bone position
		// to the mouse position, we need to translate
		// from "mouse space" to "camera space"
		// and then to "local bone space". Note that the local 
		// bone space is calculated using the bone's parent
		// worldToLocal() function!
		cameraCoords.set(Gdx.input.getX(), Gdx.input.getY(), 0);
		camera.unproject(cameraCoords); // mouse space to camera space		
		
		Bone crosshair = skeleton.findBone("crosshair"); // Should be cached.
		boneCoords.set(cameraCoords.x, cameraCoords.y);
		crosshair.getParent().worldToLocal(boneCoords); // camera space to local bone space
		crosshair.setPosition(boneCoords.x, boneCoords.y); // override the crosshair position
		crosshair.setAppliedValid(false);
		
		// Calculate final world transform with the
		// crosshair bone set to the mouse cursor
		// position.
		skeleton.updateWorldTransform();

		// Clear the screen, update the camera and
		// render the skeleton.
		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);
		camera.update();
		
		batch.getProjectionMatrix().set(camera.combined);
		batch.begin();
		renderer.draw(batch, skeleton);
		batch.end();

	}

	public void resize (int width, int height) {
		camera.setToOrtho(false); // Update camera with new size.
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new IKTest());
	}
}
