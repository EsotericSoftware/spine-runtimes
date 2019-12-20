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

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spine {

	public abstract class Screen {
		protected Example game;
		protected SkeletonRenderer skeletonRenderer;
		private MouseState lastMouseState;
		protected Boolean mouseClicked = false;

		public Screen(Example game) {
			this.game = game;
			skeletonRenderer = new SkeletonRenderer(game.GraphicsDevice);
			skeletonRenderer.PremultipliedAlpha = false;
		}

		public void UpdateInput() {
			MouseState state = Mouse.GetState();
			mouseClicked = lastMouseState.LeftButton == ButtonState.Pressed && state.LeftButton == ButtonState.Released;
			lastMouseState = state;
		}

		public abstract void Render(float deltaTime);
	}

	/// <summary>
	/// The raptor screen shows basic loading and rendering of a Spine skeleton.
	/// </summary>
	internal class RaptorScreen : Screen {
		Atlas atlas;
		Skeleton skeleton;
		AnimationState state;

		public RaptorScreen(Example game) : base (game) {
			// Load the texture atlas
			atlas = new Atlas("data/raptor.atlas", new XnaTextureLoader(game.GraphicsDevice));

			// Load the .json file using a scale of 0.5
			SkeletonJson json = new SkeletonJson(atlas);
			json.Scale = 0.5f;
			SkeletonData skeletonData = json.ReadSkeletonData("data/raptor-pro.json");

			// Create the skeleton and animation state
			skeleton = new Skeleton(skeletonData);
			AnimationStateData stateData = new AnimationStateData(skeleton.Data);
			state = new AnimationState(stateData);

			// Center within the viewport
			skeleton.X = game.GraphicsDevice.Viewport.Width / 2;
			skeleton.Y = game.GraphicsDevice.Viewport.Height;

			// Set the "walk" animation on track one and let it loop forever
			state.SetAnimation(0, "walk", true);
		}

		public override void Render(float deltaTime) {
			// Update the animation state and apply the animations
			// to the skeleton
			state.Update(deltaTime);
			state.Apply(skeleton);

			// Update the transformations of bones and other parts of the skeleton
			skeleton.UpdateWorldTransform();

			// Clear the screen and setup the projection matrix of the skeleton renderer
			game.GraphicsDevice.Clear(Color.Black);
			((BasicEffect)skeletonRenderer.Effect).Projection = Matrix.CreateOrthographicOffCenter(0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height, 0, 1, 0);

			// Draw the skeletons
			skeletonRenderer.Begin();
			skeletonRenderer.Draw(skeleton);
			skeletonRenderer.End();

			// Check if the mouse button was clicked and switch scene
			if (mouseClicked) game.currentScreen = new TankScreen(game);
		}
	}

	/// <summary>
	/// The tank screen shows how to enable two color tinting.
	/// </summary>
	internal class TankScreen : Screen {
		Atlas atlas;
		Skeleton skeleton;
		AnimationState state;

		public TankScreen(Example game) : base(game) {
			// Instantiate and configure the two color tinting effect and
			// assign it to the skeleton renderer
			var twoColorTintEffect = game.Content.Load<Effect>("Content\\SpineEffect");
			twoColorTintEffect.Parameters["World"].SetValue(Matrix.Identity);
			twoColorTintEffect.Parameters["View"].SetValue(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up));
			skeletonRenderer.Effect = twoColorTintEffect;

			// The remaining code loads the atlas and skeleton data as in the raptor screen
			atlas = new Atlas("data/tank.atlas", new XnaTextureLoader(game.GraphicsDevice));
			SkeletonJson json = new SkeletonJson(atlas);
			json.Scale = 0.25f;
			SkeletonData skeletonData = json.ReadSkeletonData("data/tank-pro.json");

			skeleton = new Skeleton(skeletonData);
			AnimationStateData stateData = new AnimationStateData(skeleton.Data);
			state = new AnimationState(stateData);

			skeleton.X = game.GraphicsDevice.Viewport.Width / 2 + 200;
			skeleton.Y = game.GraphicsDevice.Viewport.Height;

			state.SetAnimation(0, "shoot", true);
		}

		public override void Render(float deltaTime) {
			state.Update(deltaTime);
			state.Apply(skeleton);

			skeleton.UpdateWorldTransform();

			// Clear the screen and setup the projection matrix of the custom effect through the
			// "Projection" parameter.
			game.GraphicsDevice.Clear(Color.Black);
			skeletonRenderer.Effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height, 0, 1, 0));

			skeletonRenderer.Begin();
			skeletonRenderer.Draw(skeleton);
			skeletonRenderer.End();

			if (mouseClicked) game.currentScreen = new SpineboyScreen(game);
		}
	}

	/// <summary>
	/// The Spineboy screen shows how to queue up multiple animations via animation state,
	/// set the default mix time to smoothly transition between animations, and load a
	/// skeleton from a binary .skel file.
	/// </summary>
	internal class SpineboyScreen : Screen {
		Atlas atlas;
		Skeleton skeleton;
		AnimationState state;

		public SpineboyScreen(Example game) : base(game) {
			atlas = new Atlas("data/spineboy.atlas", new XnaTextureLoader(game.GraphicsDevice));

			SkeletonBinary binary = new SkeletonBinary(atlas);
			binary.Scale = 0.5f;
			SkeletonData skeletonData = binary.ReadSkeletonData("data/spineboy-pro.skel");

			skeleton = new Skeleton(skeletonData);
			AnimationStateData stateData = new AnimationStateData(skeleton.Data);
			state = new AnimationState(stateData);

			skeleton.X = game.GraphicsDevice.Viewport.Width / 2;
			skeleton.Y = game.GraphicsDevice.Viewport.Height;

			// We want 0.2 seconds of mixing time when transitioning from
			// any animation to any other animation.
			stateData.DefaultMix = 0.2f;

			// Set the "walk" animation on track one and let it loop forever
			state.SetAnimation(0, "walk", true);

			// Queue another animation after 2 seconds to let Spineboy jump
			state.AddAnimation(0, "jump", false, 2);

			// After the jump is complete, let Spineboy walk
			state.AddAnimation(0, "run", true, 0);
		}

		public override void Render(float deltaTime) {
			state.Update(deltaTime);
			state.Apply(skeleton);
			skeleton.UpdateWorldTransform();

			game.GraphicsDevice.Clear(Color.Black);
			((BasicEffect)skeletonRenderer.Effect).Projection = Matrix.CreateOrthographicOffCenter(0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height, 0, 1, 0);

			skeletonRenderer.Begin();
			skeletonRenderer.Draw(skeleton);
			skeletonRenderer.End();

			if (mouseClicked) game.currentScreen = new MixAndMatchScreen(game);
		}
	}

	/// <summary>
	/// The mix-and-match screen demonstrates how to create and apply a skin
	/// composed of other skins. This method can be used to create customizable
	/// avatar systems.
	/// </summary>
	internal class MixAndMatchScreen : Screen {
		Atlas atlas;
		Skeleton skeleton;
		AnimationState state;

		public MixAndMatchScreen(Example game) : base(game) {
			atlas = new Atlas("data/mix-and-match.atlas", new XnaTextureLoader(game.GraphicsDevice));

			SkeletonJson json = new SkeletonJson(atlas);
			json.Scale = 0.5f;
			SkeletonData skeletonData = json.ReadSkeletonData("data/mix-and-match-pro.json");

			skeleton = new Skeleton(skeletonData);
			AnimationStateData stateData = new AnimationStateData(skeleton.Data);
			state = new AnimationState(stateData);

			skeleton.X = game.GraphicsDevice.Viewport.Width / 2;
			skeleton.Y = game.GraphicsDevice.Viewport.Height;

			state.SetAnimation(0, "dance", true);

			// Create a new skin, by mixing and matching other skins
			// that fit together. Items making up the girl are individual
			// skins. Using the skin API, a new skin is created which is
			// a combination of all these individual item skins.
			var mixAndMatchSkin = new Spine.Skin("custom-girl");
			mixAndMatchSkin.AddSkin(skeletonData.FindSkin("skin-base"));
			mixAndMatchSkin.AddSkin(skeletonData.FindSkin("nose/short"));
			mixAndMatchSkin.AddSkin(skeletonData.FindSkin("eyelids/girly"));
			mixAndMatchSkin.AddSkin(skeletonData.FindSkin("eyes/violet"));
			mixAndMatchSkin.AddSkin(skeletonData.FindSkin("hair/brown"));
			mixAndMatchSkin.AddSkin(skeletonData.FindSkin("clothes/hoodie-orange"));
			mixAndMatchSkin.AddSkin(skeletonData.FindSkin("legs/pants-jeans"));
			mixAndMatchSkin.AddSkin(skeletonData.FindSkin("accessories/bag"));
			mixAndMatchSkin.AddSkin(skeletonData.FindSkin("accessories/hat-red-yellow"));
			skeleton.SetSkin(mixAndMatchSkin);
		}

		public override void Render(float deltaTime) {
			state.Update(deltaTime);
			state.Apply(skeleton);
			skeleton.UpdateWorldTransform();

			game.GraphicsDevice.Clear(Color.Black);
			((BasicEffect)skeletonRenderer.Effect).Projection = Matrix.CreateOrthographicOffCenter(0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height, 0, 1, 0);

			skeletonRenderer.Begin();
			skeletonRenderer.Draw(skeleton);
			skeletonRenderer.End();

			if (mouseClicked) game.currentScreen = new RaptorScreen(game);
		}
	}

	public class Example : Microsoft.Xna.Framework.Game {
		GraphicsDeviceManager graphics;
		public Screen currentScreen;

		public Example() {
			IsMouseVisible = true;

			graphics = new GraphicsDeviceManager(this);
			graphics.IsFullScreen = false;
			graphics.PreferredBackBufferWidth = 800;
			graphics.PreferredBackBufferHeight = 600;
		}

		protected override void LoadContent() {
			currentScreen = new MixAndMatchScreen(this);
		}

		protected override void Update(GameTime gameTime) {
			currentScreen.UpdateInput();
		}

		protected override void Draw(GameTime gameTime) {
			currentScreen.Render(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
		}
	}
}
