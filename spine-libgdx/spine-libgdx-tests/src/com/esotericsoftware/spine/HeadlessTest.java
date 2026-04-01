/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

import com.badlogic.gdx.ApplicationListener;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.headless.HeadlessApplication;
import com.badlogic.gdx.backends.headless.HeadlessApplicationConfiguration;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;

import com.esotericsoftware.spine.utils.SkeletonSerializer;

public class HeadlessTest implements ApplicationListener {
	private String skeletonPath;
	private String atlasPath;
	private String animationName;
	private String animationName2;

	public HeadlessTest (String skeletonPath, String atlasPath, String animationName, String animationName2) {
		this.skeletonPath = skeletonPath;
		this.atlasPath = atlasPath;
		this.animationName = animationName;
		this.animationName2 = animationName2;
	}

	static class MockTexture extends Texture {
		private int width, height;

		public MockTexture (int width, int height) {
			super();
			this.width = width;
			this.height = height;
		}

		@Override
		public int getWidth () {
			return width;
		}

		@Override
		public int getHeight () {
			return height;
		}

		@Override
		public void bind () {
		}

		@Override
		public void bind (int unit) {
		}

		@Override
		public void dispose () {
		}
	}

	static class HeadlessTextureAtlas extends TextureAtlas {
		public HeadlessTextureAtlas (FileHandle packFile) {
			TextureAtlasData data = new TextureAtlasData(packFile, packFile.parent(), false);

			for (TextureAtlasData.Page page : data.getPages()) {
				page.texture = new MockTexture(1024, 1024);
			}

			for (TextureAtlasData.Region region : data.getRegions()) {
				AtlasRegion atlasRegion = new AtlasRegion(region.page.texture, region.left, region.top,
					region.rotate ? region.height : region.width, region.rotate ? region.width : region.height);
				atlasRegion.index = region.index;
				atlasRegion.name = region.name;
				atlasRegion.offsetX = region.offsetX;
				atlasRegion.offsetY = region.offsetY;
				atlasRegion.originalHeight = region.originalHeight;
				atlasRegion.originalWidth = region.originalWidth;
				atlasRegion.rotate = region.rotate;
				atlasRegion.degrees = region.degrees;
				atlasRegion.names = region.names;
				atlasRegion.values = region.values;
				if (region.flip) atlasRegion.flip(false, true);
				getRegions().add(atlasRegion);
			}
		}
	}

	@Override
	public void create () {
		try {
			// Load atlas without textures
			FileHandle atlasFile = Gdx.files.absolute(atlasPath);
			TextureAtlas atlas = new HeadlessTextureAtlas(atlasFile);

			// Load skeleton data
			FileHandle skeletonFile = Gdx.files.absolute(skeletonPath);
			SkeletonData skeletonData;

			if (skeletonPath.endsWith(".json")) {
				SkeletonJson json = new SkeletonJson(atlas);
				skeletonData = json.readSkeletonData(skeletonFile);
			} else {
				SkeletonBinary binary = new SkeletonBinary(atlas);
				skeletonData = binary.readSkeletonData(skeletonFile);
			}

			// Create serializer
			SkeletonSerializer serializer = new SkeletonSerializer();

			// Print skeleton data as JSON
			System.out.println("=== SKELETON DATA ===");
			System.out.println(serializer.serializeSkeletonData(skeletonData));

			// Create skeleton instance
			Skeleton skeleton = new Skeleton(skeletonData);

			// Set animation if provided
			AnimationState state = null;
			if (animationName != null) {
				// Create animation state only when needed
				AnimationStateData stateData = new AnimationStateData(skeletonData);
				state = new AnimationState(stateData);

				// Find and set animation
				Animation animation = skeletonData.findAnimation(animationName);
				if (animation == null) {
					System.err.println("Animation not found: " + animationName);
					System.exit(1);
				}
				state.setAnimation(0, animation, true);
				// Update and apply
				state.update(0.016f);
				state.apply(skeleton);
			}

			skeleton.updateWorldTransform(Physics.update);

			// Print skeleton state as JSON
			System.out.println("\n=== SKELETON STATE ===");
			System.out.println(serializer.serializeSkeleton(skeleton));

			// Print animation state as JSON only if animation was loaded
			if (state != null) {
				System.out.println("\n=== ANIMATION STATE ===");
				System.out.println(serializer.serializeAnimationState(state));
			}

			// Transition test: if a second animation is provided, play A for 10 frames, transition to B,
			// then sample skeleton state at frames 5, 10, 15, 20 during the mix.
			if (state != null && animationName2 != null) {
				Animation animation2 = skeletonData.findAnimation(animationName2);
				if (animation2 == null) {
					System.err.println("Animation not found: " + animationName2);
					System.exit(1);
				}

				// Reset skeleton and state
				skeleton.setupPose();
				state.clearTracks();
				state.setAnimation(0, skeletonData.findAnimation(animationName), true);

				// Run 10 frames of animation A
				for (int i = 0; i < 10; i++) {
					state.update(1 / 60f);
					state.apply(skeleton);
					skeleton.updateWorldTransform(Physics.update);
				}

				// Transition to animation B
				state.setAnimation(0, animation2, true);

				// Run 20 frames through the mix, serializing at frames 5, 10, 15, 20
				for (int i = 1; i <= 20; i++) {
					state.update(1 / 60f);
					state.apply(skeleton);
					skeleton.updateWorldTransform(Physics.update);
					if (i == 5 || i == 10 || i == 15 || i == 20) {
						serializer = new SkeletonSerializer();
						System.out.println("\n=== TRANSITION FRAME " + i + " ===");
						System.out.println(serializer.serializeSkeleton(skeleton));
					}
				}
			}

		} catch (Exception e) {
			e.printStackTrace();
			System.exit(1);
		}

		// Exit after processing
		Gdx.app.exit();
	}

	@Override
	public void resize (int width, int height) {
	}

	@Override
	public void render () {
	}

	@Override
	public void pause () {
	}

	@Override
	public void resume () {
	}

	@Override
	public void dispose () {
	}

	public static void main (String[] args) {
		if (args.length < 2) {
			System.err.println("Usage: HeadlessTest <skeleton-path> <atlas-path> [animation-name]");
			System.exit(1);
		}

		HeadlessApplicationConfiguration config = new HeadlessApplicationConfiguration();
		config.updatesPerSecond = 60;
		String animationName = args.length >= 3 ? args[2] : null;
		String animationName2 = args.length >= 4 ? args[3] : null;
		new HeadlessApplication(new HeadlessTest(args[0], args[1], animationName, animationName2), config);
	}
}
