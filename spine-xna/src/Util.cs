/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *	list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *	this list of conditions and the following disclaimer in the documentation
 *	and/or other materials provided with the distribution.
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

﻿using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {

	static public class Util {
#if WINDOWS_STOREAPP
		private static async Task<Texture2D> LoadFile(GraphicsDevice device, String path) {
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			var file = await folder.GetFileAsync(path).AsTask().ConfigureAwait(false);
			try {
				return Util.LoadTexture(device, await file.OpenStreamForReadAsync().ConfigureAwait(false));
			} catch (Exception ex) {
				throw new Exception("Error reading texture file: " + path, ex);
			}
		}

		static public Texture2D LoadTexture (GraphicsDevice device, String path) {
			return LoadFile(device, path).Result;
		}
#else
		static public Texture2D LoadTexture (GraphicsDevice device, String path) {
			using (Stream input = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				try {
					return Util.LoadTexture(device, input);
				} catch (Exception ex) {
					throw new Exception("Error reading texture file: " + path, ex);
				}
			}
		}
#endif

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

			// Release the GPU back to drawing to the screen.
			device.SetRenderTarget(null);
			spriteBatch.Dispose();
			file.Dispose();

#if IOS
			return result as Texture2D;
#else
			// RenderTarget2D are volatile and will be lost on screen resolution changes.
			// So instead of using this directly, we create a non-voliate Texture2D.
			// This is computationally slower, but should be safe as long as it is done on load.
			Texture2D resultTexture = new Texture2D(device, file.Width, file.Height);
			Color[] resultContent = new Color[Convert.ToInt32(file.Width * file.Height)];
			result.GetData(resultContent);
			resultTexture.SetData(resultContent);
			result.Dispose(); // Dispose of the RenderTarget2D immediately.
			return resultTexture;
#endif
		}
	}
}
