/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
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

#if WINDOWS_PHONE
            Stream stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(path);
            using (Stream input = stream)
            {
#else
            using (Stream input = new FileStream(path, FileMode.Open, FileAccess.Read)) {
#endif
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
