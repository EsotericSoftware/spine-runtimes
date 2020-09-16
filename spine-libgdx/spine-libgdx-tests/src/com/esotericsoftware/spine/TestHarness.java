/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.math.Interpolation;
import com.esotericsoftware.spine.vertexeffects.SwirlEffect;

public class TestHarness extends ApplicationAdapter {
//	static String JSON = "coin/coin-pro.json";
//	static String ATLAS = "coin/coin-pma.atlas";
	
	static String JSON = "raptor/raptor-pro.json";
	static String ATLAS = "raptor/raptor-pma.atlas";
	
	OrthographicCamera camera;
	PolygonSpriteBatch batch;
	SkeletonRenderer renderer;
	
	ShapeRenderer shapes;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;
	
	SwirlEffect swirl;
	float swirlTime;

	public void create () {		
		camera = new OrthographicCamera();
		camera.setToOrtho(true);
		batch = new PolygonSpriteBatch();
		renderer = new SkeletonRenderer();
		renderer.setPremultipliedAlpha(true);		
		shapes = new ShapeRenderer();
		
		swirl = new SwirlEffect(400);
		swirl.setCenterY(-200);
		renderer.setVertexEffect(swirl);

		atlas = new TextureAtlas(Gdx.files.internal(ATLAS));
		SkeletonJson json = new SkeletonJson(atlas);
		json.setScale(0.5f);
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal(JSON));
		
		skeleton = new Skeleton(skeletonData);		
		skeleton.setPosition(320, 590);
		skeleton.setScaleY(-1);

		AnimationStateData stateData = new AnimationStateData(skeletonData);		
		state = new AnimationState(stateData);		
		// state.setAnimation(0, "rotate", false);
		state.update(0);
		state.apply(skeleton);
		skeleton.updateWorldTransform();
	}

	public void render () {
		if (Gdx.input.justTouched()) {
			state.update(0.25f); // Update the animation time.			
		}
		state.apply(skeleton); // Poses skeleton using current animations. This sets the bones' local SRT.
		skeleton.updateWorldTransform(); // Uses the bones' local SRT to compute their world SRT.

		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);
		
		swirlTime += Gdx.graphics.getDeltaTime();
		float percent = swirlTime % 2;
		if (percent > 1) percent = 1 - (percent - 1);
		swirl.setAngle(Interpolation.pow2.apply(-60, 60, percent));


		// Configure the camera, SpriteBatch, and SkeletonRendererDebug.
		camera.update();
		batch.getProjectionMatrix().set(camera.combined);		

		batch.begin();
		renderer.draw(batch, skeleton); // Draw the skeleton images.
		batch.end();		
	}

	public void resize (int width, int height) {
		camera.setToOrtho(true); // Update camera with new size.		
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new TestHarness(), "", 640, 640);
	}
}
