/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

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
		AnimationState state;

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

			String name = "spineboy"; // "goblins";

			Atlas atlas = new Atlas("data/" + name + ".atlas", new XnaTextureLoader(GraphicsDevice));
			SkeletonJson json = new SkeletonJson(atlas);
			skeleton = new Skeleton(json.ReadSkeletonData("data/" + name + ".json"));
			if (name == "goblins") skeleton.SetSkin("goblingirl");
			skeleton.SetSlotsToSetupPose(); // Without this the skin attachments won't be attached. See SetSkin.

			// Define mixing between animations.
			AnimationStateData stateData = new AnimationStateData(skeleton.Data);
			if (name == "spineboy") {
				stateData.SetMix("walk", "jump", 0.2f);
				stateData.SetMix("jump", "walk", 0.4f);
			}

			state = new AnimationState(stateData);
			state.SetAnimation("walk", false);
			state.AddAnimation("jump", false);
			state.AddAnimation("walk", true);

			skeleton.X = 320;
			skeleton.Y = 440;
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

			state.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);
			state.Apply(skeleton);
			skeleton.UpdateWorldTransform();
			skeletonRenderer.Begin();
			skeletonRenderer.Draw(skeleton);
			skeletonRenderer.End();

			base.Draw(gameTime);
		}
	}
}
