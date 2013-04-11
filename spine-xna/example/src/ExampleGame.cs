using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Spine;

namespace Spine {
	public class Example : Microsoft.Xna.Framework.Game {
		GraphicsDeviceManager graphics;
		SkeletonRenderer skeletonRenderer;
		Skeleton skeleton;
		Animation animation;
		float time;

		public Example () {
			graphics = new GraphicsDeviceManager(this);
			graphics.IsFullScreen = false;
			graphics.PreferredBackBufferWidth = 640;
			graphics.PreferredBackBufferHeight = 480;
		}

		protected override void Initialize () {
			// TODO: Add your initialization logic here

			base.Initialize();
		}

		protected override void LoadContent () {
			skeletonRenderer = new SkeletonRenderer(GraphicsDevice);
			Atlas atlas = new Atlas(GraphicsDevice, "data/spineboy.atlas");
			SkeletonJson json = new SkeletonJson(atlas);
			skeleton = new Skeleton(json.readSkeletonData("spineboy", File.ReadAllText("data/spineboy.json")));
			animation = skeleton.Data.FindAnimation("walk");

			skeleton.RootBone.X = 320;
			skeleton.RootBone.Y = 440;
			skeleton.UpdateWorldTransform();
		}

		protected override void UnloadContent () {
			// TODO: Unload any non ContentManager content here
		}

		protected override void Update (GameTime gameTime) {
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		protected override void Draw (GameTime gameTime) {
			GraphicsDevice.Clear(Color.Black);

			time += gameTime.ElapsedGameTime.Milliseconds / 1000f;
			animation.Apply(skeleton, time, true);
			skeleton.UpdateWorldTransform();
			skeletonRenderer.Draw(skeleton);

			base.Draw(gameTime);
		}
	}
}
