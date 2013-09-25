/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
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
 *****************************************************************************/

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
			return Texture2D.FromStream(device, input);
		}
	}
}
