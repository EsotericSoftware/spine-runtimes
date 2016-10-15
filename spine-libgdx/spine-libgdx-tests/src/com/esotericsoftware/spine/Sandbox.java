/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;

/**
 * Sandbox for comparing values when porting to other
 * runtimes.
 */
public class Sandbox extends ApplicationAdapter {
	static final String ATLAS = "../../examples/tank/export/tank.atlas";
	static final String JSON = "../../examples/tank/export/tank.json";
	static final float scale = 0.3f;
	static final float X = 400;
	static final float Y = 500;
	static final String ANIMATION = "drive";
	static final float ANIMATION_OFFSET = 0.5f;
	static final boolean ANIMATION_UPDATE = false;
	static final boolean Y_DOWN = true;
	static final boolean DRAW_DEBUG = false;
	
	OrthographicCamera camera;
	PolygonSpriteBatch batch;
	SkeletonMeshRenderer renderer;
	SkeletonRendererDebug debugRenderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	AnimationState state;

	public void create () {
		camera = new OrthographicCamera();
		camera.setToOrtho(Y_DOWN);
		batch = new PolygonSpriteBatch();
		renderer = new SkeletonMeshRenderer();
		renderer.setPremultipliedAlpha(false);
		debugRenderer = new SkeletonRendererDebug();
		debugRenderer.setBoundingBoxes(false);
		debugRenderer.setRegionAttachments(false);

		atlas = new TextureAtlas(Gdx.files.internal(ATLAS));
		SkeletonJson json = new SkeletonJson(atlas);
		json.setScale(scale);		
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal(JSON));

		skeleton = new Skeleton(skeletonData);
		skeleton.setFlipY(Y_DOWN);
		skeleton.setPosition(X, Y);

		AnimationStateData stateData = new AnimationStateData(skeletonData);
		state = new AnimationState(stateData);
		if (ANIMATION != null) state.setAnimation(0, ANIMATION, true);
		if (ANIMATION_OFFSET != 0) {
			state.update(ANIMATION_OFFSET);
			state.apply(skeleton);
			skeleton.updateWorldTransform();
		}
	}

	public void render () {		
		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		if (ANIMATION_UPDATE) {
			state.update(Gdx.graphics.getDeltaTime());
			state.apply(skeleton);			
		}
		skeleton.updateWorldTransform();

		camera.update();
		batch.getProjectionMatrix().set(camera.combined);
		debugRenderer.getShapeRenderer().setProjectionMatrix(camera.combined);

		batch.begin();
		renderer.draw(batch, skeleton);
		batch.end();

		if (DRAW_DEBUG) debugRenderer.draw(skeleton);
	}

	public void resize (int width, int height) {
		camera.setToOrtho(Y_DOWN);
	}

	public void dispose () {
		atlas.dispose();
	}

	public static void main (String[] args) throws Exception {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		config.useHDPI = true;
		config.width = 800;
		config.height = 600;
		new LwjglApplication(new Sandbox(), config);
	}
}
