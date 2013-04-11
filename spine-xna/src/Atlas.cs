using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Spine {
	public class Atlas : BaseAtlas {
		private GraphicsDevice device;

		public Atlas (GraphicsDevice device, String atlasFile) {
			this.device = device;
			using (StreamReader reader = new StreamReader(atlasFile)) {
				load(reader, Path.GetDirectoryName(atlasFile));
			}
		}

		override protected AtlasPage NewAtlasPage (String path) {
			XnaAtlasPage page = new XnaAtlasPage();
			page.Texture = loadTexture(path);
			return page;
		}

		private Texture2D loadTexture (string path) {
			Texture2D file;
			using (Stream fileStream = new FileStream(path, FileMode.Open)) {
				file = Texture2D.FromStream(device, fileStream);
			}

			// Setup a render target to hold our final texture which will have premulitplied alpha values
			RenderTarget2D result = new RenderTarget2D(device, file.Width, file.Height);
			device.SetRenderTarget(result);
			device.Clear(Color.Black);

			// Multiply each color by the source alpha, and write in just the color values into the final texture
			BlendState blendColor = new BlendState();
			blendColor.ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue;
			blendColor.AlphaDestinationBlend = Blend.Zero;
			blendColor.ColorDestinationBlend = Blend.Zero;
			blendColor.AlphaSourceBlend = Blend.SourceAlpha;
			blendColor.ColorSourceBlend = Blend.SourceAlpha;

			SpriteBatch spriteBatch = new SpriteBatch(device);
			spriteBatch.Begin(SpriteSortMode.Immediate, blendColor);
			spriteBatch.Draw(file, file.Bounds, Color.White);
			spriteBatch.End();

			// Now copy over the alpha values from the PNG source texture to the final one, without multiplying them
			BlendState blendAlpha = new BlendState();
			blendAlpha.ColorWriteChannels = ColorWriteChannels.Alpha;
			blendAlpha.AlphaDestinationBlend = Blend.Zero;
			blendAlpha.ColorDestinationBlend = Blend.Zero;
			blendAlpha.AlphaSourceBlend = Blend.One;
			blendAlpha.ColorSourceBlend = Blend.One;

			spriteBatch.Begin(SpriteSortMode.Immediate, blendAlpha);
			spriteBatch.Draw(file, file.Bounds, Color.White);
			spriteBatch.End();

			// Release the GPU back to drawing to the screen
			device.SetRenderTarget(null);

			return result as Texture2D;
		}
	}

	public class XnaAtlasPage : AtlasPage {
		public Texture2D Texture { get; set; }

		override public int GetTextureWidth () {
			return Texture.Width;
		}

		override public int GetTextureHeight () {
			return Texture.Height;
		}
	}
}
