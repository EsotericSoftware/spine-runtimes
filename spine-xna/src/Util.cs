using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Spine {
	static public class Util {
		static public Texture2D LoadTexture (GraphicsDevice device, String path) {
			using (Stream input = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				try {
					return Util.LoadTexture(device, input);
				} catch (Exception ex) {
					throw new Exception("Error reading texture file: " + path, ex);
				}
			}
		}

		static public Texture2D LoadTexture (GraphicsDevice device, Stream input) {
			Texture2D file = Texture2D.FromStream(device, input);

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
}
