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

import com.esotericsoftware.spine.Animation.MixDirection;
import com.esotericsoftware.spine.Animation.MixPose;
import com.esotericsoftware.spine.attachments.AtlasAttachmentLoader;
import com.esotericsoftware.spine.attachments.RegionAttachment;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.math.Matrix4;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.physics.box2d.Body;
import com.badlogic.gdx.physics.box2d.BodyDef;
import com.badlogic.gdx.physics.box2d.BodyDef.BodyType;
import com.badlogic.gdx.physics.box2d.Box2DDebugRenderer;
import com.badlogic.gdx.physics.box2d.FixtureDef;
import com.badlogic.gdx.physics.box2d.PolygonShape;
import com.badlogic.gdx.physics.box2d.World;
import com.badlogic.gdx.utils.Array;

public class Box2DExample extends ApplicationAdapter {
	SpriteBatch batch;
	ShapeRenderer renderer;
	SkeletonRenderer skeletonRenderer;

	TextureAtlas atlas;
	Skeleton skeleton;
	Animation animation;
	float time;
	Array<Event> events = new Array();

	OrthographicCamera camera;
	Box2DDebugRenderer box2dRenderer;
	World world;
	Body groundBody;
	Matrix4 transform = new Matrix4();
	Vector2 vector = new Vector2();

	public void create () {
		batch = new SpriteBatch();
		renderer = new ShapeRenderer();
		skeletonRenderer = new SkeletonRenderer();
		skeletonRenderer.setPremultipliedAlpha(true);

		atlas = new TextureAtlas(Gdx.files.internal("spineboy/spineboy-pma.atlas"));

		// This loader creates Box2dAttachments instead of RegionAttachments for an easy way to keep
		// track of the Box2D body for each attachment.
		AtlasAttachmentLoader atlasLoader = new AtlasAttachmentLoader(atlas) {
			public RegionAttachment newRegionAttachment (Skin skin, String name, String path) {
				Box2dAttachment attachment = new Box2dAttachment(name);
				AtlasRegion region = atlas.findRegion(attachment.getName());
				if (region == null) throw new RuntimeException("Region not found in atlas: " + attachment);
				attachment.setRegion(region);
				return attachment;
			}
		};
		SkeletonJson json = new SkeletonJson(atlasLoader);
		json.setScale(0.6f * 0.05f);
		SkeletonData skeletonData = json.readSkeletonData(Gdx.files.internal("spineboy/spineboy-ess.json"));
		animation = skeletonData.findAnimation("walk");

		skeleton = new Skeleton(skeletonData);
		skeleton.x = -32;
		skeleton.y = 1;
		skeleton.updateWorldTransform();

		// See Box2DTest in libgdx for more detailed information about Box2D setup.
		camera = new OrthographicCamera(48, 32);
		camera.position.set(0, 16, 0);
		box2dRenderer = new Box2DDebugRenderer();
		createWorld();

		// Create a body for each attachment. Note it is probably better to create just a few bodies rather than one for each
		// region attachment, but this is just an example.
		for (Slot slot : skeleton.getSlots()) {
			if (!(slot.getAttachment() instanceof Box2dAttachment)) continue;
			Box2dAttachment attachment = (Box2dAttachment)slot.getAttachment();

			PolygonShape boxPoly = new PolygonShape();
			boxPoly.setAsBox(attachment.getWidth() / 2 * attachment.getScaleX(), attachment.getHeight() / 2 * attachment.getScaleY(),
				vector.set(attachment.getX(), attachment.getY()), attachment.getRotation() * MathUtils.degRad);

			BodyDef boxBodyDef = new BodyDef();
			boxBodyDef.type = BodyType.StaticBody;
			attachment.body = world.createBody(boxBodyDef);
			attachment.body.createFixture(boxPoly, 1);

			boxPoly.dispose();
		}
	}

	public void render () {
		float delta = Gdx.graphics.getDeltaTime();
		float remaining = delta;
		while (remaining > 0) {
			float d = Math.min(0.016f, remaining);
			world.step(d, 8, 3);
			time += d;
			remaining -= d;
		}

		camera.update();

		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);
		batch.setProjectionMatrix(camera.projection);
		batch.setTransformMatrix(camera.view);
		batch.begin();

		animation.apply(skeleton, time, time, true, events, 1, MixPose.current, MixDirection.in);
		skeleton.x += 8 * delta;
		skeleton.updateWorldTransform();
		skeletonRenderer.draw(batch, skeleton);

		batch.end();

		// Position each attachment body.
		for (Slot slot : skeleton.getSlots()) {
			if (!(slot.getAttachment() instanceof Box2dAttachment)) continue;
			Box2dAttachment attachment = (Box2dAttachment)slot.getAttachment();
			if (attachment.body == null) continue;
			float x = slot.getBone().getWorldX();
			float y = slot.getBone().getWorldY();
			float rotation = slot.getBone().getWorldRotationX();
			attachment.body.setTransform(x, y, rotation * MathUtils.degRad);
		}

		box2dRenderer.render(world, camera.combined);
	}

	public void resize (int width, int height) {
		batch.setProjectionMatrix(camera.projection);
		renderer.setProjectionMatrix(camera.projection);
	}

	private void createWorld () {
		world = new World(new Vector2(0, -10), true);

		float[] vertices = {-0.07421887f, -0.16276085f, -0.12109375f, -0.22786504f, -0.157552f, -0.7122401f, 0.04296875f,
			-0.7122401f, 0.110677004f, -0.6419276f, 0.13151026f, -0.49869835f, 0.08984375f, -0.3190109f};

		PolygonShape shape = new PolygonShape();
		shape.set(vertices);

		// next we create a static ground platform. This platform
		// is not moveable and will not react to any influences from
		// outside. It will however influence other bodies. First we
		// create a PolygonShape that holds the form of the platform.
		// it will be 100 meters wide and 2 meters high, centered
		// around the origin
		PolygonShape groundPoly = new PolygonShape();
		groundPoly.setAsBox(50, 1);

		// next we create the body for the ground platform. It's
		// simply a static body.
		BodyDef groundBodyDef = new BodyDef();
		groundBodyDef.type = BodyType.StaticBody;
		groundBody = world.createBody(groundBodyDef);

		// finally we add a fixture to the body using the polygon
		// defined above. Note that we have to dispose PolygonShapes
		// and CircleShapes once they are no longer used. This is the
		// only time you have to care explicitely for memomry managment.
		FixtureDef fixtureDef = new FixtureDef();
		fixtureDef.shape = groundPoly;
		fixtureDef.filter.groupIndex = 0;
		groundBody.createFixture(fixtureDef);
		groundPoly.dispose();

		PolygonShape boxPoly = new PolygonShape();
		boxPoly.setAsBox(1, 1);

		// Next we create the 50 box bodies using the PolygonShape we just
		// defined. This process is similar to the one we used for the ground
		// body. Note that we reuse the polygon for each body fixture.
		for (int i = 0; i < 45; i++) {
			// Create the BodyDef, set a random position above the
			// ground and create a new body
			BodyDef boxBodyDef = new BodyDef();
			boxBodyDef.type = BodyType.DynamicBody;
			boxBodyDef.position.x = -24 + (float)(Math.random() * 48);
			boxBodyDef.position.y = 10 + (float)(Math.random() * 100);
			Body boxBody = world.createBody(boxBodyDef);

			boxBody.createFixture(boxPoly, 1);
		}

		// we are done, all that's left is disposing the boxPoly
		boxPoly.dispose();
	}

	public void dispose () {
		atlas.dispose();
	}

	static class Box2dAttachment extends RegionAttachment {
		Body body;

		public Box2dAttachment (String name) {
			super(name);
		}
	}

	public static void main (String[] args) throws Exception {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		config.title = "Box2D - Spine";
		config.width = 640;
		config.height = 480;
		new LwjglApplication(new Box2DExample(), config);
	}
}
