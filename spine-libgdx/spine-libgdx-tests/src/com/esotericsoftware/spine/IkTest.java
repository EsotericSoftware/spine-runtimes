/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.math.Vector2;

public class IkTest extends ApplicationAdapter {
	OrthographicCamera camera;
	SpriteBatch batch;
	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;

	public void create () {
		camera = new OrthographicCamera();
		batch = new SpriteBatch();
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);
		debugRenderer = new SkeletonRendererDebug();
		debugRenderer.setBoundingBoxes(false);
		debugRenderer.setRegionAttachments(false);

		atlas = new TextureAtlas(Gdx.files.internal("spineboy/spineboy.atlas"));
		SkeletonJson json = new SkeletonJson(atlas); // This loads skeleton JSON data, which is stateless.
		json.setScale(0.6f); // Load the skeleton at 60% the size it was in Spine.
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy/spineboy.json"));

		skeleton = new Skeleton(skeletonData); // Skeleton holds skeleton state (bone positions, slot attachments, etc).
		skeleton.setPosition(250, 20);

		AnimationStateData stateData = new AnimationStateData(skeletonData); // Defines mixing (crossfading) between animations.
		stateData.setMix("run", "jump", 0.2f);
		stateData.setMix("jump", "run", 0.2f);

		state = new AnimationState(stateData); // Holds the animation state for a skeleton (current animation, time, etc).
		state.setTimeScale(0.5f); // Slow all animations down to 50% speed.
		state.setAnimation(0, "run", true);
		state.addAnimation(0, "jump", false, 2); // Jump after 2 seconds.
		state.addAnimation(0, "run", true, 0); // Run after the jump.

		// skeleton.findBone("front_foot").parent = skeleton.findBone("hip");

		IkConstraintData data;

		data = new IkConstraintData("head");
		data.getBones().add(skeletonData.findBone("torso"));
		data.getBones().add(skeletonData.findBone("head"));
		data.target = skeletonData.findBone("front_foot");
		data.setBendDirection(-1);
		skeleton.getIkConstraints().add(new IkConstraint(data, skeleton));

// data = new IkConstraintData("arm");
// data.getBones().add(skeletonData.findBone("front_upper_arm"));
// data.getBones().add(skeletonData.findBone("front_bracer"));
// data.setTarget(skeletonData.findBone("front_foot"));
// skeleton.getIkConstraints().add(new IkConstraint(data, skeleton));
//
// data = new IkConstraintData("leg");
// data.getBones().add(skeletonData.findBone("front_thigh"));
// data.getBones().add(skeletonData.findBone("front_shin"));
// data.target = skeletonData.findBone("front_foot");
// data.setBendDirection(-1);
// skeleton.getIkConstraints().add(new IkConstraint(data, skeleton));
// //skeleton.getIkConstraints().peek().setMix(0.5f);

		skeleton.updateCache();
		skeleton.updateWorldTransform();
	}

	public void render () {
		state.update(Gdx.graphics.getDeltaTime()); // Update the animation time.

		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		// state.apply(skeleton); // Poses skeleton using current animations. This sets the bones' local SRT.

// skeleton.findBone("front_shin").y = 40;
// skeleton.findBone("front_shin").scaleX = 2;
// skeleton.findBone("front_thigh").scaleX = 2;
// skeleton.findBone("front_bracer").y = 40;
// skeleton.findBone("front_bracer").scaleX = 2;
// skeleton.findBone("front_upper_arm").scaleX = 2;
// skeleton.getRootBone().setScale(1.3f, 0.6f);

		// skeleton.findBone("front_upper_arm").parent = skeleton.findBone("front_shin");
// skeleton.findBone("head").scaleX = 2;
// skeleton.findBone("head").x = 100;
// skeleton.findBone("head").y = 100;
// skeleton.findBone("head").rotation = 0;
// skeleton.findBone("neck").x = 100;
// skeleton.findBone("neck").y = 100;
// skeleton.findBone("neck").rotation = 45;

		skeleton.setPosition(250, 20);
// skeleton.setFlip(false, false);
		skeleton.setPosition(250, 20);
// skeleton.setFlipX(true);
// skeleton.setFlipY(false);

// skeleton.findBone("torso").setFlipX(true);
// skeleton.findBone("torso").setFlipY(true);

		Vector2 p = skeleton.findBone("front_foot").parent.worldToLocal(new Vector2(Gdx.input.getX() - skeleton.getX(),
			Gdx.graphics.getHeight() - Gdx.input.getY() - skeleton.getY()));
		skeleton.findBone("front_foot").setPosition(p.x, p.y);

		skeleton.updateWorldTransform(); // Uses the bones' local SRT to compute their world SRT.

		// Configure the camera, SpriteBatch, and SkeletonRendererDebug.
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);
		debugRenderer.getShapeRenderer().setProjectionMatrix(camera.combined);

		batch.begin();
		renderer.draw(batch, skeleton); // Draw the skeleton images.
		batch.end();

		debugRenderer.draw(skeleton); // Draw debug lines.
	}

	public void resize (int width, int height) {
		camera.setToOrtho(false); // Update camera with new size.
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new IkTest());
	}
}
