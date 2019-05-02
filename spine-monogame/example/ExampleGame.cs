/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spine {
	public class Example : Microsoft.Xna.Framework.Game {
		GraphicsDeviceManager graphics;
		SkeletonRenderer skeletonRenderer;
		Skeleton skeleton;
		Slot headSlot;
		AnimationState state;
		SkeletonBounds bounds = new SkeletonBounds();

		private string assetsFolder = "data/";

		public Example() {
			IsMouseVisible = true;

			graphics = new GraphicsDeviceManager(this);
			graphics.IsFullScreen = false;
			graphics.PreferredBackBufferWidth = 800;
			graphics.PreferredBackBufferHeight = 600;
		}

		protected override void LoadContent() {
			// Two color tint effect, comment line 80 to disable
			var spineEffect = Content.Load<Effect>("Content\\SpineEffect");
			spineEffect.Parameters["World"].SetValue(Matrix.Identity);
			spineEffect.Parameters["View"].SetValue(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up));

			skeletonRenderer = new SkeletonRenderer(GraphicsDevice);
			skeletonRenderer.PremultipliedAlpha = false;
			skeletonRenderer.Effect = spineEffect;

			// String name = "spineboy-ess";
			// String name = "goblins-pro";
			String name = "raptor-pro";
			// String name = "tank-pro";
			// String name = "coin-pro";
			String atlasName = name.Replace("-pro", "").Replace("-ess", "");
			bool binaryData = false;

			Atlas atlas = new Atlas(assetsFolder + atlasName + ".atlas", new XnaTextureLoader(GraphicsDevice));

			float scale = 1;
			if (name == "spineboy-ess") scale = 0.6f;
			if (name == "raptor-pro") scale = 0.5f;
			if (name == "tank-pro") scale = 0.3f;
			if (name == "coin-pro") scale = 1;

			SkeletonData skeletonData;
			if (binaryData) {
				SkeletonBinary binary = new SkeletonBinary(atlas);
				binary.Scale = scale;
				skeletonData = binary.ReadSkeletonData(assetsFolder + name + ".skel");
			}
			else {
				SkeletonJson json = new SkeletonJson(atlas);
				json.Scale = scale;
				skeletonData = json.ReadSkeletonData(assetsFolder + name + ".json");
			}
			skeleton = new Skeleton(skeletonData);
			if (name == "goblins-pro") skeleton.SetSkin("goblin");

			// Define mixing between animations.
			AnimationStateData stateData = new AnimationStateData(skeleton.Data);
			state = new AnimationState(stateData);

			if (name == "spineboy-ess") {
				skeleton.SetAttachment("head-bb", "head"); // Activate the head BoundingBoxAttachment.

				stateData.SetMix("run", "jump", 0.2f);
				stateData.SetMix("jump", "run", 0.4f);

				// Event handling for all animations.
				state.Start += Start;
				state.End += End;
				state.Complete += Complete;
				state.Event += Event;

				state.SetAnimation(0, "run", false);
				TrackEntry entry = state.AddAnimation(0, "jump", false, 0);
				entry.End += End; // Event handling for queued animations.
				state.AddAnimation(0, "run", true, 0);
			}
			else if (name == "raptor-pro") {
				state.SetAnimation(0, "walk", true);
				state.AddAnimation(1, "gun-grab", false, 2);
			}
			else if (name == "coin-pro") {
				state.SetAnimation(0, "animation", true);
			}
			else if (name == "tank-pro") {
				state.SetAnimation(0, "drive", true);
			}
			else {
				state.SetAnimation(0, "walk", true);
			}
           
			skeleton.X = 400 + (name == "tank-pro" ? 300 : 0);
			skeleton.Y = GraphicsDevice.Viewport.Height;
            skeleton.ScaleY = -1;
			skeleton.UpdateWorldTransform();

			headSlot = skeleton.FindSlot("head");
		}

		protected override void Update(GameTime gameTime) {
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.Black);

			state.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);
			state.Apply(skeleton);
			skeleton.UpdateWorldTransform();
			if (skeletonRenderer.Effect is BasicEffect) {
				((BasicEffect)skeletonRenderer.Effect).Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 1, 0);
			}
			else {
				skeletonRenderer.Effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 1, 0));
			}
			skeletonRenderer.Begin();
			skeletonRenderer.Draw(skeleton);
			skeletonRenderer.End();

			bounds.Update(skeleton, true);
			MouseState mouse = Mouse.GetState();
			if (headSlot != null) {
				headSlot.G = 1;
				headSlot.B = 1;
				if (bounds.AabbContainsPoint(mouse.X, mouse.Y)) {
					BoundingBoxAttachment hit = bounds.ContainsPoint(mouse.X, mouse.Y);
					if (hit != null) {
						headSlot.G = 0;
						headSlot.B = 0;
					}
				}
			}

			base.Draw(gameTime);
		}

		public void Start(TrackEntry entry) {
			Console.WriteLine(entry + ": start");
		}

		public void End(TrackEntry entry) {
			Console.WriteLine(entry + ": end");
		}

		public void Complete(TrackEntry entry) {
			Console.WriteLine(entry + ": complete ");
		}

		public void Event(TrackEntry entry, Event e) {
			Console.WriteLine(entry + ": event " + e);
		}
	}
}
