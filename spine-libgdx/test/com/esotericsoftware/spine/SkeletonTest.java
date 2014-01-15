/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.InputAdapter;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL10;
import com.badlogic.gdx.graphics.Pixmap;
import com.badlogic.gdx.graphics.Pixmap.Format;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.TextureAtlasData;
import com.badlogic.gdx.utils.Array;

public class SkeletonTest extends ApplicationAdapter {
	PolygonSpriteBatch batch;
	float time;
	SkeletonRenderer renderer;
	SkeletonRendererDebug debugRenderer;

	SkeletonData skeletonData;
	Skeleton skeleton;
	Animation animation;
	Array<Event> events = new Array();

	public void create () {
		batch = new PolygonSpriteBatch();
		renderer = new SkeletonRenderer();
		debugRenderer = new SkeletonRendererDebug();

		final String name = "goblins"; // "spineboy";

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
			System.out.println("JSON");
			SkeletonJson json = new SkeletonJson(atlas);
			// json.setScale(2);
			skeletonData = json.readSkeletonData(Gdx.files.internal(name + ".json"));
		} else {
			System.out.println("Binary");
			SkeletonBinary binary = new SkeletonBinary(atlas);
			// binary.setScale(2);
			skeletonData = binary.readSkeletonData(Gdx.files.internal(name + ".skel"));
		}
		animation = skeletonData.findAnimation("walk");

		skeleton = new Skeleton(skeletonData);
		if (name.equals("goblins")) skeleton.setSkin("goblin");
		skeleton.setToSetupPose();
		skeleton = new Skeleton(skeleton);
		skeleton.updateWorldTransform();

		Gdx.input.setInputProcessor(new InputAdapter() {
			public boolean touchDown (int screenX, int screenY, int pointer, int button) {
				keyDown(0);
				return true;
			}

			public boolean keyDown (int keycode) {
				if (name.equals("goblins")) {
					skeleton.setSkin(skeleton.getSkin().getName().equals("goblin") ? "goblingirl" : "goblin");
					skeleton.setSlotsToSetupPose();
				}
				return true;
			}
		});
	}

	public void render () {
		float lastTime = time;
		time += Gdx.graphics.getDeltaTime();

		float x = skeleton.getX() + 160 * Gdx.graphics.getDeltaTime() * (skeleton.getFlipX() ? -1 : 1);
		if (x > Gdx.graphics.getWidth()) skeleton.setFlipX(true);
		if (x < 0) skeleton.setFlipX(false);
		skeleton.setX(x);
		skeleton.setX(300);

		Gdx.gl.glClear(GL10.GL_COLOR_BUFFER_BIT);

		events.clear();
		animation.apply(skeleton, lastTime, time, true, events);
		if (events.size > 0) System.out.println(events);

		skeleton.updateWorldTransform();
		skeleton.update(Gdx.graphics.getDeltaTime());

		batch.begin();
		renderer.draw(batch, skeleton);
		batch.end();

		//debugRenderer.draw(skeleton);
	}

	public void resize (int width, int height) {
		batch.getProjectionMatrix().setToOrtho2D(0, 0, width, height);
		debugRenderer.getShapeRenderer().setProjectionMatrix(batch.getProjectionMatrix());
	}

	public static void main (String[] args) throws Exception {
		new LwjglApplication(new SkeletonTest());
	}
}
